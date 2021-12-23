using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using System.ComponentModel;
using Csla.DataPortalClient;


#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
//using SVM;
using System.IO;
using System.Xml;

using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using CsvHelper;

using System.Threading;
using System.Configuration;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;



#if (!CSLA_NETCORE)
using Microsoft.VisualBasic.FileIO;
#endif

using System.Data;

#endif

namespace BusinessLibrary.BusinessClasses
{
	[Serializable]
	public class ClassifierCommand : CommandBase<ClassifierCommand>
	{
#if SILVERLIGHT
    public ClassifierCommand(){}
#else
		public ClassifierCommand() { }
#endif
		// variables for training the classifier
		private string _title;
		private Int64 _attributeIdOn;
		private Int64 _attributeIdNotOn;
		private Int64 _attributeIdClassifyTo;
		private int _sourceId;

		// variables for applying the classifier
		private int _classifierId = -1;

		private string _returnMessage;

		public ClassifierCommand(string title, Int64 attributeIdOn, Int64 attributeIdNotOn, Int64 attributeIdClassifyTo, int classiferId, int sourceId)
		{
			_title = title;
			_attributeIdOn = attributeIdOn;
			_attributeIdNotOn = attributeIdNotOn;
			_returnMessage = "Success";
			_classifierId = classiferId;
			_attributeIdClassifyTo = attributeIdClassifyTo;
			_sourceId = sourceId;
		}

		public string ReturnMessage
		{
			get
			{
				return _returnMessage;
			}
		}

		public static readonly PropertyInfo<ReviewInfo> RevInfoProperty = RegisterProperty<ReviewInfo>(new PropertyInfo<ReviewInfo>("RevInfo", "RevInfo"));
		public ReviewInfo RevInfo
		{
			get { return ReadProperty(RevInfoProperty); }
			set { LoadProperty(RevInfoProperty, value); }
		}

		public static readonly PropertyInfo<string> ReportBackProperty = RegisterProperty<string>(new PropertyInfo<string>("ReportBack", "ReportBack"));
		public string ReportBack
		{
			get { return ReadProperty(ReportBackProperty); }
			set { LoadProperty(ReportBackProperty, value); }
		}

		protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
		{
			base.OnGetState(info, mode);
			info.AddValue("_title", _title);
			info.AddValue("_attributeIdOn", _attributeIdOn);
			info.AddValue("_attributeIdNotOn", _attributeIdNotOn);
			info.AddValue("_returnMessage", _returnMessage);
			info.AddValue("_classifierId", _classifierId);
			info.AddValue("_attributeIdClassifyTo", _attributeIdClassifyTo);
			info.AddValue("_sourceId", _sourceId);
		}
		protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
		{
			_title = info.GetValue<string>("_title");
			_attributeIdOn = info.GetValue<Int64>("_attributeIdOn");
			_attributeIdNotOn = info.GetValue<Int64>("_attributeIdNotOn");
			_returnMessage = info.GetValue<string>("_returnMessage");
			_classifierId = info.GetValue<int>("_classifierId");
			_attributeIdClassifyTo = info.GetValue<Int64>("_attributeIdClassifyTo");
			_sourceId = info.GetValue<int>("_sourceId");
		}


#if !SILVERLIGHT

		protected override async void DataPortal_Execute()
		{
			if (_title == "DeleteThisModel~~")
			{
				await DeleteModelAsync();
				return;
			}
			if (_attributeIdOn + _attributeIdNotOn != -2)
			{
				DoTrainClassifier(false);
			}
			else
			{
				DoApplyClassifier(_classifierId, _attributeIdClassifyTo);
			}
		}

		private async void DoTrainClassifier(bool applyToo)
		{
			using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
			{
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				int newModelId = 0;
				connection.Open();

                if (_classifierId == -1) // building a new classifier
                {
                    using (SqlCommand command = new SqlCommand("st_ClassifierSaveModel", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        command.Parameters.Add(new SqlParameter("@MODEL_TITLE", _title + " (in progress...)"));
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_ON", _attributeIdOn));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_NOT_ON", _attributeIdNotOn));
                        command.Parameters.Add(new SqlParameter("@NEW_MODEL_ID", 0));
                        command.Parameters["@NEW_MODEL_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        newModelId = Convert.ToInt32(command.Parameters["@NEW_MODEL_ID"].Value);
                    }
                    if (newModelId == 0) // i.e. another train session is running / it's not been the specified length of time between running training yet
                    {
                        _returnMessage = "Already running";
                        return;
                    }
                    else
                    {
                        _returnMessage = "";
                    }
                }
                else
                {
                    _returnMessage = "";
                    newModelId = _classifierId; // we're rebuilding an existing classifier

                    using (SqlCommand command2 = new SqlCommand("st_ClassifierUpdateModelTitle", connection))
                    {
                        command2.CommandType = System.Data.CommandType.StoredProcedure;

                        command2.Parameters.Add(new SqlParameter("@MODEL_ID", _classifierId));
                        command2.Parameters.Add(new SqlParameter("@TITLE", _title + " (rebuilding...)"));
                        command2.ExecuteNonQuery();
                    }
                }
				int ModelId = await UploadDataAndBuildModelAsync(newModelId);

				if (_returnMessage == "Insufficient data")
				{
					using (SqlCommand command = new SqlCommand("st_ClassifierDeleteModel", connection))
					{
						command.CommandType = System.Data.CommandType.StoredProcedure;
						command.Parameters.Add(new SqlParameter("@REVIEW_ID", RevInfo.ReviewId));
						command.Parameters.Add(new SqlParameter("@MODEL_ID", newModelId));
						command.ExecuteNonQuery();
					}
				}

                //if (applyToo == true)
                //{
                //    DoApplyClassifier(ModelId);
                //}
                connection.Close();
			}
		}
		
		private static string CleanText(Csla.Data.SafeDataReader reader, string field)
		{
			string text = reader.GetString(field);

			// Strip all HTML.
			text = Regex.Replace(text, "<[^<>]+>", "");

			// Strip numbers.
			//text = Regex.Replace(text, "[0-9]+", "number");

			// Strip urls.
			text = Regex.Replace(text, @"(http|https)://[^\s]*", "httpaddr");

			// Strip email addresses.
			text = Regex.Replace(text, @"[^\s]+@[^\s]+", "emailaddr");

			// Strip dollar sign.
			text = Regex.Replace(text, "[$]+", "dollar");

			// Strip usernames.
			text = Regex.Replace(text, @"@[^\s]+", "username");

			// Strip annoying punctuation
			text = text.Replace("'", " ").Replace("\"", " ").Replace(",", " ");

			// Strip newlines
			text = text.Replace(Environment.NewLine, " ").Replace("\n\r", " ").Replace("\n", " ").Replace("\r", " ");

			return text;

			// Tokenize and also get rid of any punctuation
			//return text.Split(" @$/#.-:&*+=[]?!(){},''\">_<;%\\".ToCharArray());
		}

		private async Task<int> UploadDataAndBuildModelAsync(int modelId)
		{
			ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnection);
			//StringBuilder data = new StringBuilder();
			//data.Append("\"ITEM_ID\",\"LABEL\",\"TITLE\",\"ABSTRACT\",\"KEYWORDS\"" + Environment.NewLine);
			List<Int64> ItemIds = new List<Int64>();
			int positiveClassCount = 0;
			int negativeClasscount = 0;

			using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
			{
				connection.Open();

#if (!CSLA_NETCORE)

				string fileName = System.Web.HttpRuntime.AppDomainAppPath + TempPath + "ReviewID" + ri.ReviewId.ToString() +
                    modelId.ToString() + "ContactId" + ri.UserId.ToString() + ".csv";
#else
                // This may need to be changed for production
                // 19-12-2018 Sergio mentions this file will not
                // be necessary come the use of sql machine learning
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                string fileName = tmpDir.FullName + "/ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + ".csv";
                //SG Edit. WAS:
                //string fileName = AppDomain.CurrentDomain.BaseDirectory + TempPath + "ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + ".csv";
#endif

                using (SqlCommand command = new SqlCommand("st_ClassifierGetTrainingData", connection))
				{
					command.CommandType = System.Data.CommandType.StoredProcedure;
					command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
					command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_ON", _attributeIdOn));
					command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_NOT_ON", _attributeIdNotOn));
					using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
					{
						using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, false))
						{
							file.WriteLine("\"ITEM_ID\",\"LABEL\",\"TITLE\",\"ABSTRACT\",\"KEYWORDS\"");
							while (reader.Read())
							{
								if (ItemIds.IndexOf(reader.GetInt64("ITEM_ID")) == -1)
								{
									ItemIds.Add(reader.GetInt64("ITEM_ID"));
									file.WriteLine("\"" + reader["item_id"].ToString() + "\"," +
										"\"" + reader["LABEL"].ToString() + "\"," +
										"\"" + CleanText(reader, "title") + "\"," +
										"\"" + CleanText(reader, "abstract") + "\"," +
										"\"" + CleanText(reader, "keywords") + "\"");
									//data.Append("\"" + reader["ITEM_ID"].ToString() + "\"," +
									//    "\"" + reader["LABEL"].ToString() + "\"," +
									//    "\"" + CleanText(reader, "TITLE") + "\"," +
									//    "\"" + CleanText(reader, "ABSTRACT") + "\"," +
									//    "\"" + CleanText(reader, "KEYWORDS") + "\"" + Environment.NewLine);

									if (reader["LABEL"].ToString() == "1")
										positiveClassCount++;
									else
										negativeClasscount++;
								}
							}
						}
					}
				}

				if (positiveClassCount < 2 || negativeClasscount < 2)
				{
					_returnMessage = "Insufficient data";
					return 0;
				}
				// upload data to blob
				CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
				CloudBlobContainer container = blobClient.GetContainerReference("attributemodeldata");
				
				CloudBlockBlob blockBlobData = container.GetBlockBlobReference(TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId + "ModelId" + modelId.ToString()
					+ ".csv");
				//blockBlobData.UploadText(data.ToString()); // I'm not convinced there's not a better way of doing this - seems expensive to convert to string??

				using (var fileStream = System.IO.File.OpenRead(fileName))
				{

#if (!CSLA_NETCORE)
					blockBlobData.UploadFromStream(fileStream);
#else

					await blockBlobData.UploadFromFileAsync(fileName);
#endif
				}

				File.Delete(fileName);

				_returnMessage = "Successful upload of data";

				await InvokeBatchExecutionService(RevInfo.ReviewId.ToString(), "BuildModel", modelId, "", "", "", "");
				// er4ml isharedkey =true
				CloudBlobClient blobClientStats = storageAccount.CreateCloudBlobClient();
				CloudBlobContainer containerStats = blobClient.GetContainerReference("attributemodels");
				CloudBlockBlob blockBlob = containerStats.GetBlockBlobReference(TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" +
					modelId.ToString() + "Stats.csv");

				double accuracy = 0;
				double auc = 0;
				double precision = 0;
				double recall = 0;
				try
				{

#if (!CSLA_NETCORE)

					byte[] myFile = Encoding.UTF8.GetBytes(blockBlob.DownloadText());
#else
					// This is not being handled correctly
					bool check = await blockBlob.ExistsAsync();
					string strResult = "";
					if (check)
					{
						var downloadTask = blockBlob.DownloadTextAsync();
						strResult = await downloadTask;
						
					}
					byte[] myFile = Encoding.UTF8.GetBytes(strResult);
#endif


					MemoryStream ms = new MemoryStream(myFile);

#if (!CSLA_NETCORE)

					using (TextFieldParser csvReader = new TextFieldParser(ms))
					{
						csvReader.SetDelimiters(new string[] { "," });
						csvReader.HasFieldsEnclosedInQuotes = false;
						while (!csvReader.EndOfData)
						{
							string[] data1 = csvReader.ReadFields(); // headers
							data1 = csvReader.ReadFields(); // now we get the data
							accuracy = GetSafeValue(data1[0]);
							auc = GetSafeValue(data1[1]);
							precision = GetSafeValue(data1[2]);
							recall = GetSafeValue(data1[3]);
						}
					}
#else

					using (TextReader tr = new StreamReader(ms))
					{
						//not implemented yet
						var csv = new CsvReader(tr);
						csv.Read();
						csv.ReadHeader();
						while (csv.Read())
						{
							var record = csv.GetRecord<SVMModel>();
							accuracy = GetSafeValue(record.accuracy.ToString());
							auc = GetSafeValue(record.auc.ToString());
							precision = GetSafeValue(record.precision.ToString());
							recall = GetSafeValue(record.recall.ToString());
						}


						//var records = csv.GetRecords(anonymousTypeDefinition);
					}
	
#endif

				}
				catch(Exception e)
				{
					_returnMessage = "BuildFailed";
					_title += " (failed)";
					accuracy = -0.99;
					auc = -0.99;
					precision = -0.99;
					recall = -0.99;
				}

				using (SqlCommand command2 = new SqlCommand("st_ClassifierUpdateModel", connection))
				{
					command2.CommandType = System.Data.CommandType.StoredProcedure;
					
					command2.Parameters.Add(new SqlParameter("@MODEL_ID", modelId));
					command2.Parameters.Add(new SqlParameter("@TITLE", _title));
					command2.Parameters.AddWithValue("@ACCURACY", accuracy);
					command2.Parameters.AddWithValue("@AUC", auc);
					command2.Parameters.AddWithValue("@PRECISION",  precision);
					command2.Parameters.AddWithValue("@RECALL",  recall);
					command2.Parameters.Add(new SqlParameter("@CHECK_MODEL_ID_EXISTS", 0));
					command2.Parameters["@CHECK_MODEL_ID_EXISTS"].Direction = System.Data.ParameterDirection.Output;
					command2.ExecuteNonQuery();
					if (Convert.ToInt32(command2.Parameters["@CHECK_MODEL_ID_EXISTS"].Value) == 0)
					{
						await DeleteModelAsync();
					}
				}
				connection.Close();
			}
			return modelId;
		}

		private double GetSafeValue(string data)
		{

			if (data == "1")
			{
				data = "0.999999";
			}
			else if (data == "0")
			{
				data = "0.000001";
			}
			else if (data.Length > 2 && data.Contains("E"))
			{
				double dbl = 0;
				double.TryParse(data, out dbl);
				//if (dbl == 0.0) throw new Exception("Gotcha!");
				data = dbl.ToString("F10");
			}
			return Convert.ToDouble(data);
		}

        private async void DoApplyClassifier(int modelId, Int64 ApplyToAttributeId)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnection);
                //StringBuilder data = new StringBuilder();
                //data.Append("\"ITEM_ID\",\"LABEL\",\"TITLE\",\"ABSTRACT\",\"KEYWORDS\",\"REVIEW_ID\"" + Environment.NewLine);
                List<Int64> ItemIds = new List<Int64>();


#if (!CSLA_NETCORE)

                string fileName = System.Web.HttpRuntime.AppDomainAppPath + TempPath + "ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + ".csv";
#else
                // same as comment above for same line
                //SG Edit:
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                string fileName = tmpDir.FullName + "/ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + ".csv";
				//string fileName = AppDomain.CurrentDomain.BaseDirectory + TempPath + "ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + ".csv";
#endif
                //[SG]: new 27/09/2021: find out the reviewId for this model, as it might be from a different review
                //added bonus, ensures the current user has access to this model, I guess.
                int ModelReviewId = -1; //will be used later on so this addition includes later refs to this object.
                if (modelId > 0) //no need to check for the general pre-built models which are less than zero...
                {
                    using (SqlCommand command = new SqlCommand("st_ClassifierContactModels", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                int tempModelid = reader.GetInt32("MODEL_ID");
                                if (tempModelid == modelId)
                                {
                                    //we found it, we can stop after getting the actual ReviewId where this model was built: we need it for the filename of the model in the blob
                                    ModelReviewId = reader.GetInt32("REVIEW_ID");
                                    break;
                                }
                            }
                        }
                    }
                    if (ModelReviewId == -1)
                    {
                        //the query above didn't find the current model, so we can't/should not continue...
                        return;
                    }
                }
                //end of 27/09/2021 addition

                if (modelId == -5) // the covid19 categories using the BERT model, new Azure ML environment and SQL database. This will become default over time.
                {
					System.Threading.Tasks.Task.Run(() => DoNewMethod(modelId, ApplyToAttributeId));
                    _returnMessage = "The data will be submitted and scored. Please monitor the list of search results for output.";
                    return;
                }
                else
                {
                    using (SqlCommand command = new SqlCommand("st_ClassifierGetClassificationData", connection))// also deletes data from the classification temp table
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_CLASSIFY_TO", ApplyToAttributeId));
                        command.Parameters.Add(new SqlParameter("@SOURCE_ID", _sourceId));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, false))
                            {
                                file.WriteLine("\"ITEM_ID\",\"LABEL\",\"TITLE\",\"ABSTRACT\",\"KEYWORDS\",\"REVIEW_ID\"");
                                while (reader.Read())
                                {
                                    if (ItemIds.IndexOf(reader.GetInt64("ITEM_ID")) == -1)
                                    {
                                        ItemIds.Add(reader.GetInt64("ITEM_ID"));
                                        file.WriteLine("\"" + reader["item_id"].ToString() + "\"," +
                                            "\"" + reader["LABEL"].ToString() + "\"," +
                                            "\"" + CleanText(reader, "title") + "\"," +
                                            "\"" + CleanText(reader, "abstract") + "\"," +
                                            "\"" + CleanText(reader, "keywords") + "\"," + "\"" + RevInfo.ReviewId.ToString() + "\"");

                                        //data.Append("\"" + reader["ITEM_ID"].ToString() + "\"," +
                                        //    "\"" + reader["LABEL"].ToString() + "\"," +
                                        //    "\"" + CleanText(reader, "TITLE") + "\"," +
                                        //    "\"" + CleanText(reader, "ABSTRACT") + "\"," +
                                        //    "\"" + CleanText(reader, "KEYWORDS") + "\"," + "\"" + ri.ReviewId.ToString() + "\"" + Environment.NewLine);
                                    }
                                }
                            }
                        }
                    }

                    // upload data to blob
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference("attributemodeldata");
                    CloudBlockBlob blockBlobData;

                    blockBlobData = container.GetBlockBlobReference(TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "ToScore.csv");
                    using (var fileStream = System.IO.File.OpenRead(fileName))
                    {


#if (!CSLA_NETCORE)
                        blockBlobData.UploadFromStream(fileStream);
#else

					await blockBlobData.UploadFromFileAsync(fileName);
#endif

                    }
                    File.Delete(fileName);
                    _returnMessage = "Successful upload of data";

                    string DataFile = @"attributemodeldata/" + TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "ToScore.csv";
                    string ModelFile = @"attributemodels/" + (modelId > 0 ? TrainingRunCommand.NameBase : "") + ReviewIdForScoring(modelId, ModelReviewId.ToString()) + ".csv";
                    string ResultsFile1 = @"attributemodels/" + TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "Scores.csv";
                    string ResultsFile2 = "";
                    if (modelId == -4)
                    {
                        ResultsFile1 = "attributemodels/" + TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "RCTScores.csv";
                        ResultsFile2 = @"attributemodels/" + TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "NonRCTScores.csv";
                    }
                    await InvokeBatchExecutionService(RevInfo.ReviewId.ToString(), "ScoreModel", modelId, DataFile, ModelFile, ResultsFile1, ResultsFile2);

                    if (modelId == -4) // new RCT model = two searches to create, one for the RCTs, one for the non-RCTs
                    {
                        // load RCTs
                        DataTable RCTs = await DownloadResults(storageAccount, "attributemodels", TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "RCTScores.csv");
                        _title = "Cochrane RCT Classifier: may be RCTs";
                        LoadResultsIntoDatabase(RCTs, connection, ri);

                        // load non-RCTs
                        DataTable nRCTs = await DownloadResults(storageAccount, "attributemodels", TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "NonRCTScores.csv");
                        _title = "Cochrane RCT Classifier: unlikely to be RCTs";
                        LoadResultsIntoDatabase(nRCTs, connection, ri);
                    }
                    else
                    {
                        DataTable Scores = await DownloadResults(storageAccount, "attributemodels", TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "Scores.csv");
                        LoadResultsIntoDatabase(Scores, connection, ri);
                    }

                    connection.Close();
                }
            } // end if check for using covid categories / BERT models / SQL database
        }

		private async Task<DataTable> DownloadResults(CloudStorageAccount storageAccount, string container, string filename)
		{
			CloudBlobClient blobClient2 = storageAccount.CreateCloudBlobClient();
			CloudBlobContainer container2 = blobClient2.GetContainerReference(container);
			CloudBlockBlob blockBlob = container2.GetBlockBlobReference(filename);

#if (!CSLA_NETCORE)

				byte[] myFile = Encoding.UTF8.GetBytes(blockBlob.DownloadText());

#else
			// same as comment above for same line
			bool check = await blockBlob.ExistsAsync();
				string strResult = "";
				if (check)
				{
					var downloadTask = blockBlob.DownloadTextAsync();
					strResult = await downloadTask;

				}
				byte[] myFile = Encoding.UTF8.GetBytes(strResult);
#endif

			MemoryStream ms = new MemoryStream(myFile);

			DataTable dt = new DataTable("Scores");
			dt.Columns.Add("SCORE");
			dt.Columns.Add("ITEM_ID");
			dt.Columns.Add("REVIEW_ID");

#if (!CSLA_NETCORE)

			using (TextFieldParser csvReader = new TextFieldParser(ms))
			{
				csvReader.SetDelimiters(new string[] { "," });
				csvReader.HasFieldsEnclosedInQuotes = false;
				while (!csvReader.EndOfData)
				{
					string[] data = csvReader.ReadFields();
					if (data.Length == 3)
					{
						if (data[0] == "1")
						{
							data[0] = "0.999999";
						}
						else if (data[0] == "0")
						{
							data[0] = "0.000001";
						}
						else if (data[0].Length > 2 && data[0].Contains("E"))
						{
							double dbl = 0;
							double.TryParse(data[0], out dbl);
							//if (dbl == 0.0) throw new Exception("Gotcha!");
							data[0] = dbl.ToString("F10");
						}
						dt.Rows.Add(data);
					}

					//var data1 = csvReader.ReadFields();
					//for (var i = 0; i < data1.Length; i++)
					//{
					//    if (data1[i] == "")
					//    {
					//        data1[i] = null;
					//    }
					//}
					//dt.Rows.Add(data);
				}
			}

#else


			using (TextReader tr = new StreamReader(ms))
			{
				var csv = new CsvReader(tr);			
				while (csv.Read())
				{

					var SCORE = csv.GetField(0);
					var ITEM_ID = csv.GetField(1);
					var REVIEW_ID = csv.GetField(2);

					if (SCORE == "1")
					{
						SCORE = "0.999999";
					}
					else if (SCORE == "0")
					{
						SCORE = "0.000001";
					}
					else if (SCORE.Length > 2 && SCORE.Contains("E"))
					{
						double dbl = 0;
						double.TryParse(SCORE, out dbl);

						SCORE = dbl.ToString("F10");
					}

					DataRow row = dt.NewRow();
					row["SCORE"]= SCORE;
					row["ITEM_ID"] = ITEM_ID;
					row["REVIEW_ID"] = REVIEW_ID;
					dt.Rows.Add(row);

				}
			}
#endif

                return dt;
		}

		private void LoadResultsIntoDatabase(DataTable dt, SqlConnection connection, ReviewerIdentity ri)
		{
			using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
			{
				sbc.DestinationTableName = "TB_CLASSIFIER_ITEM_TEMP";
				sbc.ColumnMappings.Clear();
				sbc.ColumnMappings.Add("SCORE", "SCORE");
				sbc.ColumnMappings.Add("ITEM_ID", "ITEM_ID");
				sbc.ColumnMappings.Add("REVIEW_ID", "REVIEW_ID");
				sbc.BatchSize = 1000;
				sbc.WriteToServer(dt);
			}

			// Create a new search to 'hold' the results
			//int searchId = 0;
			using (SqlCommand command = new SqlCommand("st_ClassifierCreateSearchList", connection))
			{
				command.CommandType = System.Data.CommandType.StoredProcedure;
				command.CommandTimeout = 300;
				command.Parameters.Add(new SqlParameter("@REVIEW_ID", RevInfo.ReviewId));
				command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
				command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", "Items classified according to model: " + _title));
				command.Parameters.Add(new SqlParameter("@HITS_NO", dt.Rows.Count));
				command.Parameters.Add(new SqlParameter("@NEW_SEARCH_ID", 0));
				command.Parameters["@NEW_SEARCH_ID"].Direction = System.Data.ParameterDirection.Output;
				command.ExecuteNonQuery();
				//searchId = Convert.ToInt32(command.Parameters["@NEW_SEARCH_ID"].Value); not sure we need this any more
			}
		}

		private async Task DeleteModelAsync()
		{
			using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
			{
				connection.Open();

				using (SqlCommand command = new SqlCommand("st_ClassifierDeleteModel", connection))
				{
					command.CommandType = System.Data.CommandType.StoredProcedure;
					command.Parameters.Add(new SqlParameter("@REVIEW_ID", RevInfo.ReviewId));
					command.Parameters.Add(new SqlParameter("@MODEL_ID", _classifierId));
					command.ExecuteNonQuery();
				}
				connection.Close();
			}

			// now remove the blob from Azure. the fact that it's always tied to the reviewId means that people can't delete the public classifiers (RCT / DARE)
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnection);
			CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
			CloudBlobContainer container = blobClient.GetContainerReference("attributemodels");
			CloudBlockBlob blockBlobModel = container.GetBlockBlobReference(TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + _classifierId.ToString() + ".csv");
			CloudBlockBlob blockBlobStats = container.GetBlockBlobReference(TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + _classifierId.ToString() + "Stats.csv");
			try
			{
#if (!CSLA_NETCORE)

				blockBlobModel.Delete();
				blockBlobStats.Delete();
#else

                Task<bool> task1 = blockBlobModel.ExistsAsync();
                while (!task1.IsCompleted) Thread.Sleep(50);
                if (task1.Result)
                {
                    Task task2 = blockBlobModel.DeleteAsync();
                    while (!task2.IsCompleted) Thread.Sleep(50);
                }
                task1 = blockBlobStats.ExistsAsync();
                while (!task1.IsCompleted) Thread.Sleep(50);
                if (task1.Result)
                {
                    Task task2 = blockBlobStats.DeleteAsync();
                    while (!task2.IsCompleted) Thread.Sleep(50);
                }

#endif// goes in cricles.

			}
			catch
			{
				_returnMessage = "Error deleting. Blobs possibly not found.";
			}

		}

		public static string ModelIdForScoring(int modId)
		{
			string retval = "RCT";
			if (modId > 0)
			{
				retval = modId.ToString();
			}
			else
			if (modId == -2)
			{
				retval = "DARE";
			}
			else
				if (modId == -3)
			{
				retval = "NHSEED";
			}
			else
				if (modId == -4)
			{
				retval = "NewRCTModel";
			}
            else
                if (modId == -5)
            {
                retval = "CovidCategories";
            }

			return retval;
		}
		public static string ReviewIdForScoring(int modId, string reviewId)
		{
			string retval = "RCTModel";
			if (modId > 0)
			{
				retval = "ReviewId" + reviewId + "ModelId" + modId.ToString();
			}
			else
			if (modId == -2)
			{
				retval = "DAREModel";
			}
			else
				if (modId == -3)
			{
				retval = "NHSEEDModel";
			}
            else
                if (modId == -5)
            {
                retval = "CovidCategoriesModel";
            }
			return retval;
		}

		public enum BatchScoreStatusCode
		{
			NotStarted,
			Running,
			Failed,
			Cancelled,
			Finished
		}

		public class BatchScoreStatus
		{
			// Status code for the batch scoring job
			public BatchScoreStatusCode StatusCode { get; set; }

			// Locations for the potential multiple batch scoring outputs
			//public IDictionary<string, AzureBlobDataReference> Results { get; set; }

			// Error details, if any
			public string Details { get; set; }
		}

		public class BatchExecutionRequest
		{

			//public IDictionary<string, AzureBlobDataReference> Inputs { get; set; }
			public IDictionary<string, string> GlobalParameters { get; set; }

			// Locations for the potential multiple batch scoring outputs
			//public IDictionary<string, AzureBlobDataReference> Outputs { get; set; }
		}

		static async Task WriteFailedResponse(HttpResponseMessage response)
		{
			Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

			// Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
			Console.WriteLine(response.Headers.ToString());

			string responseContent = await response.Content.ReadAsStringAsync();
			Console.WriteLine(responseContent);
		}

		// these should all be stored in app.config really
		const string blobConnection = "***REMOVED***";
		const string BaseUrlScoreModel = "***REMOVED***";
		const string apiKeyScoreModel = "***REMOVED***"; //EPPI-R Models: Apply Attribute Model
		const string BaseUrlBuildModel = "***REMOVED***";
		const string apiKeyBuildModel = "***REMOVED***"; //EPPI-R Models: Build Attribute Model
		const string BaseUrlScoreNewRCTModel = "***REMOVED***";
		const string apiKeyScoreNewRCTModel = "***REMOVED***"; // Cochrane RCT Classifier v.2 (ensemble) blob storage
		const string TempPath = @"UserTempUploads/ContactId";

		const int TimeOutInMilliseconds = 360 * 50000; // 5 hours?

		public static async Task InvokeBatchExecutionService(string ReviewId, string ApiCall, int modelId, string DataFile, 
            string ModelFile, string ResultsFile1, string ResultsFile2, CancellationToken cancellationToken = default(CancellationToken))
		{
			using (HttpClient client = new HttpClient())
			{
				BatchExecutionRequest request;
				string apiKey;
				string BaseUrl;

				Dictionary<string, string> GlobalParameters = new Dictionary<string, string>();

				// set parameters etc for the appropriate call (currently not using the vectorise - pending ability to save / load the vectors!!)
				if (ApiCall == "BuildModel")
				{
					apiKey = apiKeyBuildModel;
					BaseUrl = BaseUrlBuildModel;
					request = new BatchExecutionRequest()
					{
						GlobalParameters = new Dictionary<string, string>()
						{
							{ "DataFile", "attributemodeldata/" + TrainingRunCommand.NameBase + "ReviewId" + ReviewId + "ModelId" + modelId.ToString() + ".csv" },
							{ "ModelFile", "attributemodels/" + TrainingRunCommand.NameBase + "ReviewId" + ReviewId + "ModelId" + modelId.ToString() + ".csv" },
							{ "StatsFile", "attributemodels/" + TrainingRunCommand.NameBase + "ReviewId" + ReviewId + "ModelId" + modelId.ToString() + "Stats.csv" },
						}
					};
				}
				else
				{
					if (modelId == -4)
					{
						apiKey = apiKeyScoreNewRCTModel;
						BaseUrl = BaseUrlScoreNewRCTModel;
						request = new BatchExecutionRequest()
						{
							GlobalParameters = new Dictionary<string, string>()
							{
								{ "DataFile", DataFile },
								{ "RCTResultsFile", ResultsFile1 },
								{ "NonRCTResultsFile", ResultsFile2 },
							}
						};
					}
					else
					{
						apiKey = apiKeyScoreModel;
						BaseUrl = BaseUrlScoreModel;
						request = new BatchExecutionRequest()
						{
							GlobalParameters = new Dictionary<string, string>()
							{
								{ "DataFile", DataFile },
								{ "ModelFile", ModelFile },
								{ "ResultsFile", ResultsFile1 },
							}
						};
					}
				}
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

				// submit the job


				var response = await client.PostAsJsonAsync(BaseUrl + "?api-version=2.0", request);

				if (!response.IsSuccessStatusCode)
				{
					await WriteFailedResponse(response);
					return;
				}
                if (cancellationToken.IsCancellationRequested)
                {
                    return;//not much to do here, we don't log in here...
                }

				string jobId = await response.Content.ReadAsAsync<string>();


				// start the job
				response = await client.PostAsync(BaseUrl + "/" + jobId + "/start?api-version=2.0", null);
				if (!response.IsSuccessStatusCode)
				{
					await WriteFailedResponse(response);
					return;
				}

				string jobLocation = BaseUrl + "/" + jobId + "?api-version=2.0";
				Stopwatch watch = Stopwatch.StartNew();
				bool done = false;
				while (!done)
				{
					response = await client.GetAsync(jobLocation);
					if (!response.IsSuccessStatusCode)
					{
						await WriteFailedResponse(response);
						return;
					}


					BatchScoreStatus status = await response.Content.ReadAsAsync<BatchScoreStatus>();


					if (watch.ElapsedMilliseconds > TimeOutInMilliseconds || cancellationToken.IsCancellationRequested)
					{
						done = true;
						await client.DeleteAsync(jobLocation);
                    }
					switch (status.StatusCode)
					{
						case BatchScoreStatusCode.NotStarted:
							break;
						case BatchScoreStatusCode.Running:
							break;
						case BatchScoreStatusCode.Failed:
							done = true;
							break;
						case BatchScoreStatusCode.Cancelled:
							done = true;
							break;
						case BatchScoreStatusCode.Finished:
							done = true;
							break;
					}

					if (!done)
					{
						Thread.Sleep(1000); // Wait one second
					}
				}
//#else



//				var response = await client.PostAsJsonAsync(BaseUrl + "?api-version=2.0", request);

//				if (!response.IsSuccessStatusCode)
//				{
//					await WriteFailedResponse(response);
//					return;
//				}

//				string jobId = await response.Content.ReadAsAsync<string>();

//				// start the job
//				response = await client.PostAsync(BaseUrl + "/" + jobId + "/start?api-version=2.0", null);
//				if (!response.IsSuccessStatusCode)
//				{
//					await WriteFailedResponse(response);
//					return;
//				}

//				string jobLocation = BaseUrl + "/" + jobId + "?api-version=2.0";
//				Stopwatch watch = Stopwatch.StartNew();
//				bool done = false;
//				while (!done)
//				{
//					response = await client.GetAsync(jobLocation);
//					if (!response.IsSuccessStatusCode)
//					{
//						await WriteFailedResponse(response);
//						return;
//					}

//					BatchScoreStatus status = await response.Content.ReadAsAsync<BatchScoreStatus>();
//					if (watch.ElapsedMilliseconds > TimeOutInMilliseconds)
//					{
//						done = true;
//						await client.DeleteAsync(jobLocation);
//					}
//					switch (status.StatusCode)
//					{
//						case BatchScoreStatusCode.NotStarted:
//							break;
//						case BatchScoreStatusCode.Running:
//							break;
//						case BatchScoreStatusCode.Failed:
//							done = true;
//							break;
//						case BatchScoreStatusCode.Cancelled:
//							done = true;
//							break;
//						case BatchScoreStatusCode.Finished:
//							done = true;
//							break;
//					}

//					if (!done)
//					{
//						Thread.Sleep(1000); // Wait one second
//					}
//				}

//#endif


			}
		}

        private async Task DoNewMethod(int modelId, Int64 ApplyToAttributeId)
        {
            // Much simpler approach: 1) write data to Azure SQL; 2) trigger the DataFactory pipeline; 3) download scores from Azure SQL and insert into Reviewer DB

            string BatchGuid = Guid.NewGuid().ToString();
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int userId = ri.UserId;
            int reviewId = ri.ReviewId;
            int rowcount = 0;

            // 1) write the data to the Azure SQL database
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ClassifierGetClassificationDataToSQL", connection))
                {
                    command.CommandTimeout = 6000; // 10 minutes - if there are tens of thousands of items it can take a while
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_CLASSIFY_TO", ApplyToAttributeId));
                    command.Parameters.Add(new SqlParameter("@SOURCE_ID", _sourceId));
                    command.Parameters.Add(new SqlParameter("@BatchGuid", BatchGuid));
                    command.Parameters.Add(new SqlParameter("@ContactId", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@MachineName", TrainingRunCommand.NameBase));
                    command.Parameters.Add(new SqlParameter("@ROWCOUNT", 0));
                    command.Parameters["@ROWCOUNT"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    rowcount = Convert.ToInt32(command.Parameters["@ROWCOUNT"].Value);
                }
            }
            if (rowcount == 0)
            {
                _returnMessage = "Zero rows to score!";
                return;
            }


#if (CSLA_NETCORE)

            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureContReviewSettings");

#else
            var configuration = ConfigurationManager.AppSettings;

#endif

            // 2) trigger the data factory run (which in turn calls the Azure ML pipeline)

            string tenantID = configuration["tenantID"];
            string appClientId = configuration["appClientId"];
            string appClientSecret = configuration["appClientSecret"];
            string subscriptionId = configuration["subscriptionId"];
            string resourceGroup = configuration["resourceGroup"];
            string dataFactoryName = configuration["dataFactoryName"];
            string covidClassifierPipelineName = configuration["covidClassifierPipelineName"];

            var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
            ClientCredential cc = new ClientCredential(appClientId, appClientSecret);
            AuthenticationResult result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
            ServiceClientCredentials cred = new TokenCredentials(result.AccessToken);
            var client = new DataFactoryManagementClient(cred)
            {
                SubscriptionId = subscriptionId
            };
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"BatchGuid", BatchGuid}
            };

            CreateRunResponse runResponse = client.Pipelines.CreateRunWithHttpMessagesAsync(resourceGroup, dataFactoryName, covidClassifierPipelineName, parameters: parameters).Result.Body;

            string runStatus = client.PipelineRuns.GetAsync(resourceGroup, dataFactoryName, runResponse.RunId).Result.Status;
            while (runStatus.Equals("InProgress") || runStatus.Equals("Queued"))
            {
                int count = 0;
                Console.WriteLine(runStatus);

                if (DateTime.Now.ToUniversalTime().AddMinutes(5) > result.ExpiresOn) // the token expires after an hour
                {
                    count++;
                    string accessToken = result.AccessToken;
                    result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
                    cred = new TokenCredentials(result.AccessToken);
                    client = new DataFactoryManagementClient(cred)
                    {
                        SubscriptionId = subscriptionId
                    };
                }

                Thread.Sleep(5 * 1000);
                try
                {
                    PipelineRun pr = client.PipelineRuns.Get(resourceGroup, dataFactoryName, runResponse.RunId); //Microsoft.Rest.Azure.CloudException if token has expired
                    if (pr != null)
                    {
                        runStatus = pr.Status;
                    }
                }
                catch (Microsoft.Rest.Azure.CloudException e)
                {
                    if (e != null)
                    {
                        result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
                        cred = new TokenCredentials(result.AccessToken);
                        client = new DataFactoryManagementClient(cred)
                        {
                            SubscriptionId = subscriptionId
                        };
                    }
                }
            }


            // 3) download the scores and insert them into the Reviewer database. This stored proc also cleans up the data in the Azure SQL database (i.e. deletes rows associated with this BatchGuid)
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ClassifierGetGetPredictedLabels", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@BatchGuid", BatchGuid));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            string PredictedLabel = reader["PredictedLabel"].ToString();
                            using (SqlConnection connection2 = new SqlConnection(DataConnection.ConnectionString))
                            {
                                connection2.Open();
                                using (SqlCommand command2 = new SqlCommand("st_ClassifierInsertSearchAndScores", connection2))
                                {
                                    command2.CommandType = System.Data.CommandType.StoredProcedure;
                                    command2.Parameters.Add(new SqlParameter("@BatchGuid", BatchGuid));
                                    command2.Parameters.Add(new SqlParameter("@PredictedLabel", PredictedLabel));
                                    command2.Parameters.Add(new SqlParameter("@REVIEW_ID", reviewId));
                                    command2.Parameters.Add(new SqlParameter("@CONTACT_ID", userId));
                                    command2.Parameters.Add(new SqlParameter("@SearchTitle", "COVID-19 map category: " + PredictedLabel));
                                    command2.ExecuteNonQuery();
                                }
                            }

                        }
                    }
                }
            }
        }

#endif
    }
}


public class SVMModel
{
	public double accuracy { get; set; }
	public double auc { get; set; }
	public double precision { get; set; }
	public double recall { get; set; }
	
}
