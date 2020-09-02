using System;
using System.Collections.Generic;
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
using System.Threading;
using Newtonsoft.Json;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.IO;
using System.Xml;

using System.Diagnostics;
using System.Globalization;
using System.Net.Http;

using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure;
//using Microsoft.Azure;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Data;
#endif

#if CSLA_NETCORE
using Microsoft.Extensions.Logging;
#endif

#if (!SILVERLIGHT && !CSLA_NETCORE)
using Microsoft.VisualBasic.FileIO;
#endif

namespace BusinessLibrary.BusinessClasses
{

    [Serializable]
    public class TrainingRunCommand : CommandBase<TrainingRunCommand>
    {

    public TrainingRunCommand(){}


        private string _title;
        private string _terms;
        private string _answers;
        private string _filterType;
        private int _searchId;
        private bool _included;

        private int _currentTrainingId;

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

        public static readonly PropertyInfo<string> ParametersProperty = RegisterProperty<string>(new PropertyInfo<string>("Parameters", "Parameters"));
        public string Parameters
        {
            get { return ReadProperty(ParametersProperty); }
            set { LoadProperty(ParametersProperty, value); }
        }

        public static readonly PropertyInfo<string> SimulationResultsProperty = RegisterProperty<string>(new PropertyInfo<string>("SimulationResults", "SimulationResults"));
        public string SimulationResults
        {
            get { return ReadProperty(SimulationResultsProperty); }
            set { LoadProperty(SimulationResultsProperty, value); }
        }

        public int SearchId
        {
            get
            {
                return _searchId;
            }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_title", _title);
            info.AddValue("_terms", _terms);
            info.AddValue("_answers", _answers);
            info.AddValue("_filterType", _filterType);
            info.AddValue("_searchId", _searchId);
            info.AddValue("_included", _included);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _title = info.GetValue<string>("_title");
            _terms = info.GetValue<string>("_terms");
            _answers = info.GetValue<string>("_answers");
            _filterType = info.GetValue<string>("_filterType");
            _searchId = info.GetValue<int>("_searchId");
            _included = info.GetValue<bool>("_included");
        }


#if !SILVERLIGHT

        protected override async void DataPortal_Execute() // can I override and make it async?? looks like it works
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (Parameters == "DoSimulation")
            {
                DoSimulation(ri.ReviewId, ri.UserId);
                return;
            }
            if (Parameters == "FetchSimulationResults")
            {
                FetchSimulationResults(ri.ReviewId);
                return;
            }
            bool justIndexed = false;
            if (RevInfo.ScreeningMode == "Random") // || RevInfo.ScreeningWhatAttributeId > 0)
            {
                CreateNonMLLIst(ri.ReviewId, ri.UserId);
                return;
            }

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();

                // ************* STAGE 1: check that we have sufficient data to run the ML
                // ************* a minimum of 5 includes and 5 excludes is currently looked for

                //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                int ReviewID = ri.ReviewId;
                int n_includes = 0;
                int n_excludes = 0;
                using (SqlCommand command = new SqlCommand("st_TrainingCheckData", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewID));
                    command.Parameters.Add(new SqlParameter("@N_INCLUDES", 0));
                    command.Parameters.Add(new SqlParameter("@N_EXCLUDES", 0));
                    command.Parameters["@N_INCLUDES"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@N_EXCLUDES"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    n_includes = Convert.ToInt32(command.Parameters["@N_INCLUDES"].Value);
                    n_excludes = Convert.ToInt32(command.Parameters["@N_EXCLUDES"].Value);
                }

                // if we don't have enough data, we default to creating a non-ML list
                if (n_includes < 6 || n_excludes < 6)
                {
                    RevInfo.ScreeningMode = "Random";
                    CreateNonMLLIst(ri.ReviewId, ri.UserId);
                    return;
                }

                // ************* STAGE 2: CHECK TO SEE WHETHER WE SHOULD RUN THE TRAINING (NOT IF ONE IS ALREADY RUNNING)

                using (SqlCommand command = new SqlCommand("st_TrainingInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewID));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@NEW_TRAINING_ID", 0));
                    command.Parameters["@NEW_TRAINING_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    _currentTrainingId = Convert.ToInt32(command.Parameters["@NEW_TRAINING_ID"].Value);
                }
                if (_currentTrainingId == 0) // i.e. another train session is running / it's not been the specified length of time between running training yet
                {
                    ReportBack = "The list of items to screen is already being generated";
                    return;
                }
                else
                {
                    ReportBack = "";
                }

                //// (OLD) Stage 3: post the TB_ITEM data for the review up to azure if this hasn't already been done
                //if (RevInfo.ScreeningIndexed == false)
                //{
                //    UploadDataToAzureBlob(ReviewID, true); // ScreeningIndexed == true, which sets the 'screening indexed' flag in reviewinfo (means data uploaded AND vectorised, so ready for active learning)
                //    await InvokeBatchExecutionService(RevInfo, "Vectorise"); 
                //    justIndexed = true;
                //}

                // (NEW change by Sergio) Stage 3: post the TB_ITEM data, this is forced if RevInfoScreeningIndexed == false.
                // when ScreeningIndexed == true data will be uploaded only if it isn't already there, UploadDataToAzureBlob returns FALSE when nothing is done

                //BLOCK below is part of what forces to run Synchronously, which we actually don't really want this as it makes the MVC controller timeout
                //Task<bool> task = UploadDataToAzureBlob(ReviewID, RevInfo.ScreeningIndexed);
                //while (!task.IsCompleted) Thread.Sleep(100);
                //justIndexed = task.Result;

                //LINE below in MVC/CORE env makes the controller send the response without waiting... We're OK with this, for now.
                justIndexed = await UploadDataToAzureBlob(ReviewID, ri.UserId, RevInfo.ScreeningIndexed); 
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnection);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("uploads");
                CloudBlockBlob blockBlobData = container.GetBlockBlobReference(NameBase + "ReviewId" + RevInfo.ReviewId + "Vectors.csv");

#if (!CSLA_NETCORE)
                if (RevInfo.ScreeningIndexed == false || justIndexed || !blockBlobData.Exists())
#else
                Task<bool> task2 = blockBlobData.ExistsAsync();
                while (!task2.IsCompleted) Thread.Sleep(100);
                bool blockBlobDataExistsRes =  task2.Result;

                if (RevInfo.ScreeningIndexed == false || justIndexed || !blockBlobDataExistsRes)
#endif
                {//vectorise if:
                    //(currently user-set) flag forcing (re)indexing is set to FALSE ( = need to create index)
                    //OR we just built the index anyway (make sure vectors are up to date)
                    //OR there is no Vectors file...
                    await InvokeBatchExecutionService(RevInfo, "Vectorise");
                    //justIndexed = true;
                }



                // Stage 4: update Azure include / exclude data (this changes each iteration, unlike the review data)
                if (justIndexed == false)
                {// no need to update if we've only just written the data, as indexing also creates initial include/ exclude data file
                    UpdateAzureIncludeExcludeData(ReviewID);
                }

                // Stage 5: now call the Azure webservice which will take the review data, build the classifier,
                // apply the classifier to the unlabelled items ('99') and write their scores to the results blob
                await InvokeBatchExecutionService(RevInfo, "BuildAndScore");

                // Stage 6: bring the data down from Azure BLOB and write to the tb_training item table
                
                container = blobClient.GetContainerReference("results");
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + ".csv");
#if (!CSLA_NETCORE)
                byte[] myFile = Encoding.UTF8.GetBytes(blockBlob.DownloadText());
#else
                Task<string> task3 = blockBlob.DownloadTextAsync();
                while (!task3.IsCompleted) Thread.Sleep(100);
                byte[] myFile = Encoding.UTF8.GetBytes(task3.Result);
#endif

                MemoryStream ms = new MemoryStream(myFile);

                DataTable dt = new DataTable("Scores");
                dt.Columns.Add("SCORE");
                dt.Columns.Add("ITEM_ID");
                dt.Columns.Add("REVIEW_ID");
                bool MLreturnedEmptyLines = false;
#if !CSLA_NETCORE
                using (TextFieldParser csvReader = new TextFieldParser(ms))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = false;
                    while (!csvReader.EndOfData)
                    {
                        string[] data = csvReader.ReadFields();
                        if (data.Length == 3 && data[1].Length > 0 && data[2].Length > 0)
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
                        } else
                        {
                            MLreturnedEmptyLines = true;
                        }
                        //for (var i = 0; i < data.Length; i++)
                        //{
                        //    if (data[i] == "")
                        //    {
                        //        data[i] = null;
                        //    }
                        //    else if (data[i] == "1")
                        //    {
                        //        data[i] = "0.999999";
                        //    }
                        //    else if (data[i] == "0")
                        //    {
                        //        data[i] = "0.000001";
                        //    }
                        //}
                        
                    }
                }
#else
                using (StreamReader csvReader = new StreamReader(ms))
                {
                    //csvReader.SetDelimiters(new string[] { "," });
                    //csvReader.HasFieldsEnclosedInQuotes = false;
                    string line;
                    while ((line = csvReader.ReadLine()) != null)
                    {
                        string[] data = line.Split(',');
                        if (data.Length == 3 && data[1].Length > 0 && data[2].Length > 0)
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
                        else
                        {
                            MLreturnedEmptyLines = true;
                        }
                    }
                }
#endif
                using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                {
                    sbc.DestinationTableName = "TB_SCREENING_ML_TEMP";
                    sbc.ColumnMappings.Clear();
                    sbc.ColumnMappings.Add("SCORE", "SCORE");
                    sbc.ColumnMappings.Add("ITEM_ID", "ITEM_ID");
                    sbc.ColumnMappings.Add("REVIEW_ID", "REVIEW_ID");
                    sbc.BatchSize = 1000;
                    sbc.WriteToServer(dt);
                }
                using (SqlCommand command = new SqlCommand("st_ScreeningCreateMLList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewID));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@WHAT_ATTRIBUTE_ID", RevInfo.ScreeningWhatAttributeId));
                    command.Parameters.Add(new SqlParameter("@SCREENING_MODE", RevInfo.ScreeningMode));
                    command.Parameters.Add(new SqlParameter("@CODE_SET_ID", RevInfo.ScreeningCodeSetId));
                    command.Parameters.Add(new SqlParameter("@TRAINING_ID", _currentTrainingId));
                    command.CommandTimeout = 145;
                    if (dt.Rows.Count > 30000)
                    {//adjust timeout for large reviews: we don't care if this is slow, as it's a costly operation anyway.
                        int adjuster = dt.Rows.Count - 29999;
                        command.CommandTimeout = command.CommandTimeout + (int)Math.Round(((double)adjuster / 1000));
                    }
#if !CSLA_NETCORE
                    command.ExecuteNonQuery();
#else
                    //IN MVC project, this runs Asynchronously, thus outside the original try block. Might crash the whole server-side if not caught...
                    try
                    {
                        command.ExecuteNonQuery();

                        //code used to trigger an excemption almost always, used to test if logging from a CSLA object can work...
                        //naturally this code should never be active in production!!
                        //Random r = new Random();
                        //if (r.Next() > 0.0000001) throw new Exception("done manually for testing purpose...", new Exception("this is the inner exception"));
                    }
                    catch (Exception e)
                    {
                        ERxWebClient2.Startup.Logger.LogError(e, "List creation in TrainingRunCommand failed", command.Parameters);
                        
                        //the line below was (probably) creating the "review needs indexing" bug: when an exception happened (timeout?)
                        //then RevInfo.ScreeningIndexed = false was set in the lines below... (07/08/2019)
                        //MLreturnedEmptyLines = true;//send some signal that stuff didn't work...
                    }
#endif
                }
                connection.Close();
                //new Add (Sept 2017): if ML returned empty lines, update the reviewinfo object with ScreeningIndexed = false, that's because new items were added
                //next time the training runs, it will re-index.
                if (MLreturnedEmptyLines)
                {
                    RevInfo.ScreeningIndexed = false;
                    RevInfo.ApplyEdit();
                    RevInfo.BeginSave(true);
                }
            }
        }
        private void CreateNonMLLIst(int ReviewId, int UserId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();

                //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_ScreeningCreateNonMLList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", UserId));
                    command.Parameters.Add(new SqlParameter("@WHAT_ATTRIBUTE_ID", RevInfo.ScreeningWhatAttributeId));
                    command.Parameters.Add(new SqlParameter("SCREENING_MODE", RevInfo.ScreeningMode));
                    command.Parameters.Add(new SqlParameter("CODE_SET_ID", RevInfo.ScreeningCodeSetId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        // based on: https://github.com/primaryobjects/TFIDF/blob/master/TFIDFExample/TFIDF.cs#L67
        // with additions (many changes) from JT

     /// Copyright (c) 2013 Kory Becker http://www.primaryobjects.com/kory-becker.aspx 
     ///  
     /// Permission is hereby granted, free of charge, to any person obtaining 
     /// a copy of this software and associated documentation files (the 
     /// "Software"), to deal in the Software without restriction, including 
     /// without limitation the rights to use, copy, modify, merge, publish, 
     /// distribute, sublicense, and/or sell copies of the Software, and to 
     /// permit persons to whom the Software is furnished to do so, subject to 
     /// the following conditions: 
     ///  
     /// The above copyright notice and this permission notice shall be 
     /// included in all copies or substantial portions of the Software. 
     ///  
     /// Description: 
     /// Performs a TF*IDF (Term Frequency * Inverse Document Frequency) transformation on an array of documents. 
     /// Each document string is transformed into an array of doubles, cooresponding to their associated TF*IDF values. 
     ///  
     /// Usage: 
     /// string[] documents = LoadYourDocuments(); 
     /// 
     /// double[][] inputs = TFIDF.Transform(documents); 
     /// inputs = TFIDF.Normalize(inputs); 
     ///  

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

        //private bool UploadDataToAzureBlob(int ReviewID, bool ScreeningIndexed)

        private async Task<bool> UploadDataToAzureBlob(int ReviewID, int UserId, bool ScreeningIndexed)

        {
            //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnection);
            //StringBuilder data = new StringBuilder();
            StringBuilder labels = new StringBuilder();
            //data.Append("\"REVIEW_ID\",\"ITEM_ID\",\"TITLE\",\"ABSTRACT\",\"KEYWORDS\"" + Environment.NewLine);
            labels.Append("\"REVIEW_ID\",\"ITEM_ID\",\"LABEL\"" + Environment.NewLine);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("uploads");
            CloudBlockBlob blockBlobData = container.GetBlockBlobReference(NameBase + "ReviewId" + RevInfo.ReviewId + ".csv");
            CloudBlockBlob blockBlobLabels = container.GetBlockBlobReference(NameBase + "ReviewId" + RevInfo.ReviewId + "Labels.csv");
#if (!CSLA_NETCORE)
                if (!ScreeningIndexed
                ||
                !blockBlobData.Exists()
                ||
                !blockBlobLabels.Exists()
                )
#else
            bool blockBlobDataExists = await blockBlobData.ExistsAsync();
            bool blockBlobLabelsExists = await blockBlobLabels.ExistsAsync();

            if (!ScreeningIndexed
                ||
                !blockBlobDataExists
                ||
                !blockBlobLabelsExists
                )
#endif

            {//we only do something if: 
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
#if (!CSLA_NETCORE)
                    string fileName = System.Web.HttpRuntime.AppDomainAppPath + TempPath + UserId.ToString() + ".csv";
#else
                    string fileName = "";
                    if (Directory.Exists(@"\UserTempUploads"))
                    {
                         fileName = @".\UserTempUploads" + @"\" + UserId.ToString() + ".csv";
                    }
                    else
                    {
                        DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory(@"\UserTempUploads");
                        fileName = tmpDir.FullName + @"\" + UserId.ToString() + ".csv";
                    }
                    

                    //string fileName = Path.GetTempPath() + ri.UserId.ToString() + ".csv";
#endif
                    using (SqlCommand command = new SqlCommand("st_TrainingWriteDataToAzure", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewID));
                        //OLD version 
                        //command.Parameters.Add(new SqlParameter("@SCREENING_INDEXED", ScreeningIndexed));
                        
                        //NEW: we are setting DB flag to true as we are actually indexing right now! 
                        //Flag can only be re-flipped by users, when they want to. In the future might be flipped when importing new items or editing existing ones...
                        command.Parameters.Add(new SqlParameter("@SCREENING_INDEXED", true));
                        command.CommandTimeout = 600;
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            List<Int64> ItemIds = new List<Int64>();
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, false))
                            {
                                file.WriteLine("\"REVIEW_ID\",\"ITEM_ID\",\"TITLE\",\"ABSTRACT\",\"KEYWORDS\"");
                                while (reader.Read())
                                {
                                    if (ItemIds.IndexOf(reader.GetInt64("item_id")) == -1)
                                    {
                                        ItemIds.Add(reader.GetInt64("item_id"));

                                        file.WriteLine("\"" + reader["review_id"].ToString() + "\"," +
                                            "\"" + reader["item_id"].ToString() + "\"," +
                                            "\"" + CleanText(reader, "title") + "\"," +
                                            "\"" + CleanText(reader, "abstract") + "\"," +
                                            "\"" + CleanText(reader, "keywords") + "\"");

                                        labels.Append("\"" + reader["review_id"].ToString() + "\"," +
                                            "\"" + reader["item_id"].ToString() + "\"," +
                                            "\"" + CleanText(reader, "included") + "\"" + Environment.NewLine);
                                    }
                                }
                            }
                        }
                    }
                    connection.Close();
#if (!CSLA_NETCORE)
                    using (var fileStream = System.IO.File.OpenRead(fileName))
                    {
                        blockBlobData.UploadFromStream(fileStream);
                    }
                    File.Delete(fileName);
                    //blockBlobData.UploadText(data.ToString()); // I'm not convinced there's not a better way of doing this - seems expensive to convert to string??
                    blockBlobLabels.UploadText(labels.ToString());
#else
                    using (var fileStream = System.IO.File.OpenRead(fileName))
                    {
                        await blockBlobData.UploadFromStreamAsync(fileStream);
                    }
                    File.Delete(fileName);
                    //blockBlobData.UploadText(data.ToString()); // I'm not convinced there's not a better way of doing this - seems expensive to convert to string??
                    await blockBlobLabels.UploadTextAsync(labels.ToString());
#endif
                }
                return true;//we actually did re-index
            }
            return false;//didn't re-index, wasn't necessary.
        }
#if (!CSLA_NETCORE)
        private void UpdateAzureIncludeExcludeData(int ReviewID)
#else
        private async void UpdateAzureIncludeExcludeData(int ReviewID)
#endif
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnection);
            StringBuilder labels = new StringBuilder();
            labels.Append("\"REVIEW_ID\",\"ITEM_ID\",\"LABEL\"" + Environment.NewLine);

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TrainingWriteIncludeExcludeToAzure", connection))
                {
                    //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewID));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        List<Int64> ItemIds = new List<Int64>();
                        while (reader.Read())
                        {
                            if (ItemIds.IndexOf(reader.GetInt64("ITEM_ID")) == -1)
                            {
                                ItemIds.Add(reader.GetInt64("ITEM_ID"));
                                labels.Append("\"" + reader["REVIEW_ID"].ToString() + "\"," +
                               "\"" + reader["ITEM_ID"].ToString() + "\"," +
                               "\"" + CleanText(reader, "INCLUDED") + "\"" + Environment.NewLine);
                            }
                        }
                    }
                }
                connection.Close();

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("uploads");
                CloudBlockBlob blockBlobLabels = container.GetBlockBlobReference(NameBase + "ReviewId" + RevInfo.ReviewId + "Labels.csv");
#if (!CSLA_NETCORE)
                blockBlobLabels.UploadText(labels.ToString());
#else
                await blockBlobLabels.UploadTextAsync(labels.ToString());
#endif

            }
        }

        private async void DoSimulation(int ReviewID, int UserId)
        {
            // Delete existing results file (if any)
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnection);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("simulations");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(NameBase + "ReviewId" + ReviewID.ToString() + ".csv");
#if (!CSLA_NETCORE)
            blockBlob.DeleteIfExists();
#else
            await blockBlob.DeleteIfExistsAsync();
#endif
            // Simple: upload data if needed
            bool justIndexed = false;
            if (RevInfo.ScreeningIndexed == false)
            {
                await UploadDataToAzureBlob(ReviewID, UserId, false); // ScreeningIndexed == false because we're not vectorising (line below)
                //await InvokeBatchExecutionService(RevInfo, "Vectorise"); commented out - don't need to Vectorise
                justIndexed = true;
            }

            // Make sure we have include / exclude data written
            if (justIndexed == false) // no need to update if we've only just written the data, as indexing also creates initial include/ exclude data file
            {
                UpdateAzureIncludeExcludeData(ReviewID);
            }

            // Then call the simulation
            await InvokeBatchExecutionService(RevInfo, "Simulation");
        }
#if (!CSLA_NETCORE)
        private void FetchSimulationResults(int ReviewID)
#else
        private async void FetchSimulationResults(int ReviewID)
#endif
        {
            // Stage 6: bring the data down from Azure BLOB and write to the tb_training item table
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnection);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("simulations");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(NameBase + "ReviewId" + ReviewID.ToString() + ".csv");

            try // if the simulation hasn't finished yet, there will be no file
            {
#if (!CSLA_NETCORE)
                byte[] myFile = Encoding.UTF8.GetBytes(blockBlob.DownloadText());
#else
                byte[] myFile = Encoding.UTF8.GetBytes(await blockBlob.DownloadTextAsync());
#endif
                MemoryStream ms = new MemoryStream(myFile);
                SimulationResults = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
            catch
            {
                SimulationResults = "";
            }
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
        const string BaseUrlBuildAndScore = "***REMOVED***";
        const string apiKeyBuildAndScore = "***REMOVED***"; //EPPI-R - build + score (blob storage)
        const string BaseUrlVectorise = "***REMOVED***";
        const string apiKeyVectorise = "***REMOVED***"; //EPPI-R AL: index review
        const string BaseUrlSimulation5 = "***REMOVED***";
        const string apiKeySimulation5 = "***REMOVED***"; // EPPI-R: active learning simulation (x10)
        const string TempPath = @"UserTempUploads/ReviewId";

        public static string NameBase
        {//used to generate different files on the cloud, based on where this is running, as it could be any dev/test machine as well as the live one (EPI3).
            get
            {
                string name = Environment.MachineName;
                if (name.ToLower() == "epi3") return "";
                else return name;
            }
        }

        const int TimeOutInMilliseconds = 360 * 50000; // 5 hours?

        static async Task InvokeBatchExecutionService(ReviewInfo revInfo, string ApiCall)
        {
            using (HttpClient client = new HttpClient())
            {
                BatchExecutionRequest request;
                string apiKey;
                string BaseUrl;

                Dictionary<string, string> GlobalParameters = new Dictionary<string, string>();

                // set parameters etc for the appropriate call (currently not using the vectorise - pending ability to save / load the vectors!!)
                if (ApiCall == "Vectorise")
                {
                    apiKey = apiKeyVectorise;
                    BaseUrl = BaseUrlVectorise;
                    request = new BatchExecutionRequest()
                    {
                        GlobalParameters = new Dictionary<string, string>()
                        {
                            { "DataFile", "uploads/" + NameBase + "ReviewId" + revInfo.ReviewId.ToString() + ".csv" },
                            { "LabelsFile", "uploads/" + NameBase + "ReviewId" + revInfo.ReviewId.ToString() + "Labels.csv" },
                            { "VectorsFile", "uploads/" + NameBase + "ReviewId" + revInfo.ReviewId.ToString() + "Vectors.csv" },
                        }
                    };
                }
                else if (ApiCall == "BuildAndScore")
                {
                    apiKey = apiKeyBuildAndScore;
                    BaseUrl = BaseUrlBuildAndScore;
                    request = new BatchExecutionRequest()
                    {
                        GlobalParameters = new Dictionary<string, string>()
                        {
                            { "DataFile", @"uploads/" + NameBase + "ReviewId" + revInfo.ReviewId.ToString() + ".csv" },
                            { "LabelsFile", @"uploads/" + NameBase + "ReviewId" + revInfo.ReviewId.ToString() + "Labels.csv" },
                            { "ResultsFile", @"results/" + NameBase + "ReviewId" + revInfo.ReviewId.ToString() + ".csv" },
                            { "VectorsFile", "uploads/" + NameBase + "ReviewId" + revInfo.ReviewId.ToString() + "Vectors.csv" },
                        }
                    };
                }
                else
                {
                    apiKey = apiKeySimulation5;
                    BaseUrl = BaseUrlSimulation5;
                    request = new BatchExecutionRequest()
                    {
                        GlobalParameters = new Dictionary<string, string>()
                        {
                            { "DataFile", "uploads/" + NameBase + "ReviewId" + revInfo.ReviewId.ToString() + ".csv" },
                            { "LabelsFile", "uploads/" + NameBase + "ReviewId" + revInfo.ReviewId.ToString() + "Labels.csv" },
                            { "ResultsFile", "simulations/" + NameBase + "ReviewId" + revInfo.ReviewId.ToString() + ".csv" },
                        }
                    };
                }
                        
                        
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                // submit the job
#if (!CSLA_NETCORE)
                var response = await client.PostAsJsonAsync(BaseUrl + "?api-version=2.0", request);
#else
                Task<HttpResponseMessage> task = client.PostAsync(BaseUrl + "?api-version=2.0",
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")
                    );
                while (!task.IsCompleted) Thread.Sleep(100);
                var response = task.Result;
#endif
                if (!response.IsSuccessStatusCode)
                {
                    await WriteFailedResponse(response);
                    return;
                }
#if (!CSLA_NETCORE)
                string jobId = await response.Content.ReadAsAsync<string>();
                response = await client.PostAsync(BaseUrl + "/" + jobId + "/start?api-version=2.0", null);
#else
                string jobId = await response.Content.ReadAsStringAsync();
                jobId = jobId.Trim('"');
                task = client.PostAsync(BaseUrl + "/" + jobId + "/start?api-version=2.0", null);
                while (!task.IsCompleted) Thread.Sleep(100);
                response = task.Result;
                //response = await client.PostAsync(BaseUrl + "/" + jobId + "/start?api-version=2.0", null);
#endif
                // start the job

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
#if (!CSLA_NETCORE)
                    response = await client.GetAsync(jobLocation);
                    if (!response.IsSuccessStatusCode)
                    {
                        await WriteFailedResponse(response);
                        return;
                    }

                    BatchScoreStatus status = await response.Content.ReadAsAsync<BatchScoreStatus>();
#else
                    task = client.GetAsync(jobLocation);
                    while (!task.IsCompleted) Thread.Sleep(100);
                    response = task.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        await WriteFailedResponse(response);
                        return;
                    }
                    BatchScoreStatus status = JsonConvert.DeserializeObject<BatchScoreStatus>(await response.Content.ReadAsStringAsync());
#endif
                    if (watch.ElapsedMilliseconds > TimeOutInMilliseconds)
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
            }
        }





#endif
        }

    }
