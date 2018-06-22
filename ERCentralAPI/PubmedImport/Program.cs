using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;


namespace PubmedImport
{
	
	class Program
	{
		//private static string DoWhat = "SampleFile";
		private static bool success = false;
		public static bool simulate = false;
		private static string errorMsg = "";
		public static int maxCount = int.MaxValue;
		public static int currCount = 0;
		public static int FTPretryCount = 0;
		public static bool deleteRecords = false;
		private static string SingleFile = "";
		private static string FTPUpdatesFolder = "";
		private static string FTPBaselineFolder = "";
		//public static string RavenHost = "";
		public static SQLHelper SqlHelper = null;
		private static bool WaitOnExit = false;
		private static bool SaveLog = false;
		private static string LogFileFullPath = "";
		static void Main(string[] args)
		{
			//https://blog.bitscry.com/2017/05/30/appsettings-json-in-net-core-console-app/
			PubMedUpdateFileImportJobLog result = new PubMedUpdateFileImportJobLog(args);

			//DateTime globalStart = DateTime.Now;
			foreach (string s in args)
			{//main block to decide what to do!
				if (s.Length > 7 && s.Substring(0, 7).ToLower() == "dowhat:")
				{//Supported values are: ftpsamplefile, ftpbaselinefolder (used for yearly update), ftpupdatefolder (to download new daily updates)
				 //and singlefile paired with "file:" to download a single file
					if (s.Substring(7).Trim().Length > 0)
						result.DoWhat = s.Substring(7).Trim().ToLower();
				}
				else if (s.Length > 9 && s.Substring(0, 9).ToLower() == "maxcount:")
				{
					int tmp;
					if (int.TryParse(s.Substring(9).Trim(), out tmp))
					{
						maxCount = tmp;
					}
				}
				else if (s.Length == 6 && s.Substring(0, 6).ToLower() == "whatif")
				{
					simulate = true;//does not save!
				}
				else if (s.Length == 13 && s.Substring(0, 13).ToLower() == "deleterecords")
				{
					deleteRecords = true;//deletes records that already exist and does not add new ones!
				}
				else if (s.Length > 5 && s.Substring(0, 5).ToLower() == "file:")
				{
					if (s.Substring(5).Trim().Length > 0)
						SingleFile = s.Substring(5).Trim().ToLower();
				}
				else if (s.Length == 10 && s.ToLower() == "waitonexit")
				{
					WaitOnExit = true;
				}
				else if (s.Length == 7 && s.ToLower() == "savelog")
				{
					SaveLog = true;
				}
				else
				{//this prevents utility from doing anything (as some input wasn't recognised)
					//will show quick help instead.
					result.DoWhat = "Nothing";
					break;//so that we won't overwrite DoWhat!
				}
			}
			GetAppSettings();
			if (SqlHelper == null)
			{
				Program.LogMessageLine("");
				Program.LogMessageLine("Error connecting to DBs!");
				Program.LogMessageLine("Please check that appsettings.json values have the right values and that SQL instance is running and reachable.");
				Program.LogMessageLine("Aborting...");
				Program.LogMessageLine("");
				System.Environment.Exit(0);
			}
            //Program.LogMessageLine("Parser testing!");
            if (result.DoWhat == "ftpsamplefile")
            {
                Program.LogMessageLine("Importing PubMed Sample XML file.");
                if (deleteRecords) Program.LogMessageLine("DeleteRecords option: will delete records that already exist and won't add new ones!");
                //URLs below not included in appsettings.json as we expect to run this routine only for debugging purposes, hence it's OK to hardcode...
                (string Pathname, List<string> messages) = WebRequestGet.getFTPBinaryFiles("ftp://ftp.ncbi.nlm.nih.gov/pubmed/baseline-2018-sample/", "pubmedsample18n0001.xml.gz");
                if (messages != null && messages.Count > 0)
                {//this only happens if an exception was raised!
                    FileParserResult spoof = new FileParserResult("Preliminary: get sample file.", deleteRecords);
                    spoof.ErrorCount++;
                    spoof.Messages.AddRange(messages);
                    spoof.Success = false;
                    result.ProcessedFilesResults.Add(spoof);
                }
                else result.ProcessedFilesResults.Add(FileParser.ParseFile(@"TmpFiles\" + Pathname));
            }
            else if (result.DoWhat == "singlefile")
            {
                if (SingleFile == "")
                {//can't do this, we don't know what to download...
                    Program.LogMessageLine("Please provide URL of file to download via the \"file:\" option.");
                    Program.LogMessageLine("Nothing to do here, aborting...");
                }
                else
                {//"Unable to connect to the remote server" "The operation has timed out."
                    Program.LogMessageLine("Importing single XML file (" + SingleFile + ").");
                    if (deleteRecords) Program.LogMessageLine("DeleteRecords option: will delete records that already exist and won't add new ones!");
                    bool canProceed = false;
                    (string Pathname, List<string> messages) = WebRequestGet.getFTPBinaryFiles(SingleFile);
                    if (messages != null && messages.Count > 0)
                    {//this only happens if an exception was raised!
                        FileParserResult spoof = new FileParserResult("Preliminary: get file to process.", deleteRecords);
                        spoof.ErrorCount++;
                        spoof.Success = false;
                        canProceed = false;
                        bool TryAgain = true;
                        spoof.Messages.AddRange(messages);
                        FTPretryCount = 1;
                        while (FTPretryCount <= 3 && TryAgain)
                        {
                            int seconds = 60;
                            if (FTPretryCount == 2)
                            {
                                seconds = 120;
                            }
                            else if (FTPretryCount >= 2)
                            {
                                seconds = 180;
                            }
                            spoof.Messages.Add("FTP call failed, will sleep for " + seconds.ToString() + "s and try again. Retry count:" + FTPretryCount.ToString());
                            LogMessageLine("FTP call failed, will sleep for " + seconds.ToString() + "s and try again. Retry count:" + FTPretryCount.ToString());
                            System.Threading.Thread.Sleep(seconds * 1000);
                            messages = new List<string>();
                            (Pathname, messages) = WebRequestGet.getFTPBinaryFiles(SingleFile);
                            if (messages != null && messages.Count > 0)
                            {//still didn't work!
                                spoof.ErrorCount++;
                                spoof.Messages.AddRange(messages);
                                FTPretryCount++;
                            }
                            else
                            {
                                spoof.Success = true;
                                canProceed = true;
                                FTPretryCount = 0;
                                TryAgain = false;
                            }
                        }
                        result.ProcessedFilesResults.Add(spoof);
                    }
                    else
                    {
                        canProceed = true;
                    }
                    if (canProceed) result.ProcessedFilesResults.Add(FileParser.ParseFile(@"TmpFiles\" + Pathname));
                }
            }

            else if (result.DoWhat == "ftpbaselinefolder")
            {
                Program.LogMessageLine("Importing all XML files in FTP (baseline) folder.");
                //although this mode is technically supported by the remaining DoWhat modes, we don't think it's safe to use it, so this option disables it...
                if (deleteRecords)
                {
                    result.TotalErrorCount++;
                    Program.LogMessageLine("DeleteRecords option is not supported in \"ftpbaselinefolder\" mode.");
                    //Program.LogMessageLine("DeleteRecords option: will delete records that already exist and won't add new ones!");
                }
                else
                {
                    DoFTPFolder(result);
                }
            }
            else if (result.DoWhat == "ftpupdatefolder")
            {
                Program.LogMessageLine("Importing files from Updates folder (only those that were not imported already).");
                //although this mode is technically supported by the remaining DoWhat modes, we don't think it's safe to use it, so this option disables it...
                if (deleteRecords)
                {
                    result.TotalErrorCount++;
                    //Program.LogMessageLine("DeleteRecords option: will delete records that already exist and won't add new ones!");
                    Program.LogMessageLine("DeleteRecords option is not supported in \"ftpupdatefolder\" mode.");
                }
                else
                {
                    DoFTPUpdateFiles(result);
                }
            }
            else if (result.DoWhat == "dorctscores")
            {
                Console.WriteLine("doing rct scores");
            }
            if (
					result.DoWhat == "Nothing"
					|| args == null
					|| args.Length == 0
					|| !(//check the dowhat value makes sense...
						result.DoWhat == "ftpsamplefile"
						|| result.DoWhat == "singlefile"
						|| result.DoWhat == "ftpbaselinefolder"
						|| result.DoWhat == "ftpupdatefolder"
                        || result.DoWhat == "dorctscores"
                        )
					|| (result.DoWhat == "singlefile" && SingleFile == "")
				)
			{
				result.Summary = "Nothing was done, incorrect parameters.";
				ShowHelp();
				if (WaitOnExit)
				{
					Program.LogMessageLine("All done, press a key to quit.");
					Console.ReadKey();
				}
			}
			else
			{
				result.Summary = "Processed " + currCount.ToString() + " records in " + Duration(result.StartTime);
				Program.LogMessageLine("Summary: " + result.Summary);
				if (Program.simulate == false)
				{
                        Program.LogMessageLine("Saving job summary to DB");
						result.UpdateErrors();
                        using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
                        {

                            conn.Open();
                            SaveJobSummary(conn, result);

                        }

                    }

				if (WaitOnExit)
				{
					Program.LogMessageLine("All done, press a key to quit.");
					Console.ReadKey();
					Program.LogMessageLine("");
				}
				else
				{
					Program.LogMessageLine("All done, exiting.");
					Program.LogMessageLine("");
				}
			}
			
		}

        private static void SaveJobSummary(SqlConnection conn, PubMedUpdateFileImportJobLog result)
        {
            string argStr = "";
            foreach (var item in result.Arguments)
            {
                argStr += "," +  item.ToString();
            }
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            SqlParameter IdParam = new SqlParameter("@jobID", (Int64)(-1));
            IdParam.Direction = System.Data.ParameterDirection.Output;
            sqlParams.Add(IdParam);
            sqlParams.Add(new SqlParameter("@IsDeleting", result.IsDeleting));
            sqlParams.Add(new SqlParameter("@TotalErrorCount", result.TotalErrorCount));
            sqlParams.Add(new SqlParameter("@Summary", result.Summary));
            sqlParams.Add(new SqlParameter("@Arguments", argStr));
            sqlParams.Add(new SqlParameter("@StartTime", result.StartTime));
            sqlParams.Add(new SqlParameter("@EndTime", result.EndTime));
            sqlParams.Add(new SqlParameter("@HasError", result.HasErrors));
             
            SqlParameter[] parameters = new SqlParameter[8];
            parameters = sqlParams.ToArray();

            try
            {


                    SQLHelper.ExecuteNonQuerySP(conn, "st_PubMedJobLogInsert", parameters);
                    var jobID = (Int64)IdParam.Value;
                    //conn.Close();

                    // Can loop through the number of FileParserResults and insert into the relevant table
                    foreach (var fileParser in result.ProcessedFilesResults)
                    {
                        if (fileParser.UpdatedPMIDs == null)
                        {
                            fileParser.UpdatedPMIDs = "";
                        }
                        string argStrF = "";
                        foreach (var item in result.Arguments)
                        {
                            argStrF += "," + item.ToString();
                        }
                        //conn.Open();
                        SQLHelper.ExecuteNonQuerySP(conn, "st_FileParserResultInsert"
                                           , new SqlParameter("@Success", fileParser.Success)
                                           , new SqlParameter("@IsDeleting", fileParser.IsDeleting)
                                           , new SqlParameter("@ErrorCount", fileParser.ErrorCount)
                                           , new SqlParameter("@FileName", fileParser.FileName)
                                           , new SqlParameter("@UpdatedPMIDs", fileParser.UpdatedPMIDs)
                                           , new SqlParameter("@CitationsInFile", fileParser.CitationsInFile)
                                           , new SqlParameter("@CitationsCommitted", fileParser.CitationsCommitted)
                                           , new SqlParameter("@StartTime", fileParser.StartTime)
                                           , new SqlParameter("@EndTime", fileParser.EndTime)
                                           , new SqlParameter("@HasErrors", fileParser.HasErrors)
                                           , new SqlParameter("@Messages", argStrF)
                                           , new SqlParameter("@PubMedUpdateFileImportJobLogID", jobID)
                                       );
                    }

                

            }
            catch (Exception e)
            {
                Program.LogException(e, "Error inserting joblog entry into sql.");
            }
        
        }

        static void GetAppSettings()
		{
			System.IO.Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Tmpfiles");
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
			IConfigurationRoot configuration = builder.Build();
			try
			{
				FTPBaselineFolder = configuration["AppSettings:FTPBaselineFolder"];
				if (FTPBaselineFolder == null || FTPBaselineFolder == "")
					throw new Exception("ERROR: could not get value for FTPBaselineFolder, please check appsettings.json file.");
				FTPUpdatesFolder = configuration["AppSettings:FTPUpdatesFolder"];
				if (FTPUpdatesFolder == null || FTPUpdatesFolder == "")
					throw new Exception("ERROR: could not get value for FTPUpdatesFolder, please check appsettings.json file.");
                
                SqlHelper = new SQLHelper(configuration);
                if (SqlHelper == null || SqlHelper.DataServiceDB == "")
					throw new Exception("ERROR: could not get value for DatabaseName, please check appsettings.json file.");
			}
			catch (Exception e)
			{
				Program.LogMessageLine("");
				Program.LogMessageLine("Error reading config file, details are:");
				Program.LogMessageLine(e.Message);
				Program.LogMessageLine("Aborting...");
				Program.LogMessageLine("");
				System.Environment.Exit(0);
			}
		}

		static void DoFTPUpdateFiles(PubMedUpdateFileImportJobLog result)
		{
			(List<PubMedUpdateFileImport> updateFiles, List<string> messages)  = WebRequestGet.getUpdateFTPFiles(FTPUpdatesFolder);// "ftp://ftp.ncbi.nlm.nih.gov/pubmed/updatefiles/");
			if (messages != null && messages.Count > 0)
			{//this only happens if an exception was raised!
				FileParserResult spoof = new FileParserResult("Preliminary: get list of update files", deleteRecords);
				spoof.ErrorCount++;
				spoof.Messages.AddRange(messages);
				spoof.Success = false;
				bool TryAgain = true;
				FTPretryCount = 1;
				while (FTPretryCount <= 3 && TryAgain)
				{
					int seconds = 60;
					if (FTPretryCount == 2)
					{
						seconds = 120;
					}
					else if (FTPretryCount >= 2)
					{
						seconds = 180;
					}
					spoof.Messages.Add("FTP call failed, will sleep for " + seconds.ToString() + "s and try again. Retry count:" + FTPretryCount.ToString());
					LogMessageLine("FTP call failed, will sleep for " + seconds.ToString() + "s and try again. Retry count:" + FTPretryCount.ToString());
					System.Threading.Thread.Sleep(seconds * 1000);
					messages = new List<string>();
					(updateFiles, messages) = WebRequestGet.getUpdateFTPFiles(FTPUpdatesFolder);// "ftp://ftp.ncbi.nlm.nih.gov/pubmed/updatefiles/");
					if (messages != null && messages.Count > 0)
					{//still didn't work!
						spoof.ErrorCount++;
						spoof.Messages.AddRange(messages);
						FTPretryCount++;
					}
					else
					{
						spoof.Success = true;
						FTPretryCount = 0;
						TryAgain = false;
					}
				}
				result.ProcessedFilesResults.Add(spoof);
			}                                                                                                                      //list above is sorted while fetching it...

			for (int i = 0; i < updateFiles.Count(); i++)
			{
				try
				{
					string Pathname;
					bool canProceed = true;
					(Pathname,  messages) = WebRequestGet.getFTPBinaryFiles(FTPUpdatesFolder, updateFiles[i].FileName);
					if (messages != null && messages.Count > 0)
					{//this only happens if an exception was raised!
						FileParserResult spoof = new FileParserResult("Preliminary: fetch update file " + updateFiles[i].FileName, deleteRecords);
						//spoof.ErrorCount++;
						//spoof.Success = false;
						//spoof.Messages.AddRange(messages);
						//result.ProcessedFilesResults.Add(spoof);
						////skip this file :-(
						//continue;
						spoof.ErrorCount++;
						spoof.Messages.AddRange(messages);
						spoof.Success = false;
						canProceed = false;
						bool TryAgain = true;
						FTPretryCount = 1;
						while (FTPretryCount <= 3 && TryAgain)
						{
							int seconds = 60;
							if (FTPretryCount == 2)
							{
								seconds = 120;
							}
							else if (FTPretryCount >= 2)
							{
								seconds = 180;
							}
							spoof.Messages.Add("FTP call failed, will sleep for " + seconds.ToString() + "s and try again. Retry count:" + FTPretryCount.ToString());
							LogMessageLine("FTP call failed, will sleep for " + seconds.ToString() + "s and try again. Retry count:" + FTPretryCount.ToString());
							System.Threading.Thread.Sleep(seconds * 1000);
							messages = new List<string>();
							(Pathname, messages) = WebRequestGet.getFTPBinaryFiles(FTPUpdatesFolder, updateFiles[i].FileName);
							if (messages != null && messages.Count > 0)
							{//still didn't work!
								spoof.ErrorCount++;
								spoof.Messages.AddRange(messages);
								FTPretryCount++;
							}
							else
							{
								spoof.Success = true;
								canProceed = true;
								FTPretryCount = 0;
								TryAgain = false;
							}
						}
						result.ProcessedFilesResults.Add(spoof);
					}
					FileParserResult currentJobResult = new FileParserResult(updateFiles[i].FileName, Program.deleteRecords);
					if (canProceed)
					{
						currentJobResult = FileParser.ParseFile(@"TmpFiles\" + Pathname);

						//save to DB the PubMedUpdateFileImport object, used to keep track of already processed daily upates
						if (currentJobResult.Success)
						{
							bool saveUpdateFileImport = false;
							if (currCount < maxCount) saveUpdateFileImport = true;
							else if (currCount == maxCount)
							{
								saveUpdateFileImport = true;
								foreach (string msg in currentJobResult.Messages)
								{
									if (msg.Contains("Maxcount reached, processing stopped after "))
									{//only processed partially, don't mark it as done on DB
										saveUpdateFileImport = false;
										break;
									}
								}
							}
							if (saveUpdateFileImport)
							{
                                using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
                                {
                                    updateFiles[i].ImportDate = DateTime.Now;
                                    if (Program.simulate == false) updateFiles[i].SaveSelf(conn);

                                }
                            }
						}
						result.ProcessedFilesResults.Add(currentJobResult);
					}
				}
				catch (Exception e)
				{
					FileParserResult currentJobResult = new FileParserResult(updateFiles[i].FileName, deleteRecords);
					currentJobResult.Success = false;
					currentJobResult.ErrorCount++;
					currentJobResult.Messages.Add("Error processing file: " + updateFiles[i].FileName);
					currentJobResult.Messages.Add("Error Message: " + e.Message);
					currentJobResult.Messages.Add(e.StackTrace);
					result.ProcessedFilesResults.Add(currentJobResult);
					Program.LogMessageLine("Error processing file: " + updateFiles[i].FileName);
					Program.LogMessageLine("Error Message: " + e.Message);
					Program.LogMessageLine(e.StackTrace);
				}
				if (currCount >= maxCount) break;
			}

		}

		static void DoFTPFolder(PubMedUpdateFileImportJobLog result)
		{
			(List<string> lstFiles, List<string> messages) = WebRequestGet.GetPubMedBaselineFilesList(FTPBaselineFolder);//"ftp://ftp.ncbi.nlm.nih.gov/pubmed/baseline/");
			if (messages != null && messages.Count > 0)
			{//this only happens if an exception was raised!
				FileParserResult spoof = new FileParserResult("Preliminary: get list of baseline files", deleteRecords);
				spoof.ErrorCount++;
				spoof.Messages.AddRange(messages);
				spoof.Success = false;
				bool TryAgain = true;
				FTPretryCount = 1;
				while (FTPretryCount <= 3 && TryAgain)
				{
					int seconds = 60;
					if (FTPretryCount == 2)
					{
						seconds = 120;
					}
					else if (FTPretryCount >= 2)
					{
						seconds = 180;
					}
					spoof.Messages.Add("FTP call failed, will sleep for " + seconds.ToString() + "s and try again. Retry count:" + FTPretryCount.ToString());
					LogMessageLine("FTP call failed, will sleep for " + seconds.ToString() + "s and try again. Retry count:" + FTPretryCount.ToString());
					System.Threading.Thread.Sleep(seconds * 1000);
					messages = new List<string>();
					(lstFiles, messages) = WebRequestGet.GetPubMedBaselineFilesList(FTPBaselineFolder);
					if (messages != null && messages.Count > 0)
					{//still didn't work!
						spoof.ErrorCount++;
						spoof.Messages.AddRange(messages);
						FTPretryCount++;
					}
					else
					{
						spoof.Success = true;
						FTPretryCount = 0;
						TryAgain = false;
					}
				}
				result.ProcessedFilesResults.Add(spoof);
			}
			if (lstFiles.Count > 0)//we have files to process!
			{
				
				lstFiles.Sort();
				for (int i = 1; i < lstFiles.Count(); i++)
				{
					string pubMedFileName = lstFiles[i];
					string Pathname;
					bool canProceed = true;
					(Pathname, messages) = WebRequestGet.getFTPBinaryFiles(FTPBaselineFolder, pubMedFileName);
					if (messages != null && messages.Count > 0)
					{//this only happens if an exception was raised!
						FileParserResult spoof = new FileParserResult("Preliminary: fetch baseline file " + pubMedFileName, deleteRecords);
						spoof.ErrorCount++;
						spoof.Messages.AddRange(messages);
						spoof.Success = false;
						canProceed = false;
						bool TryAgain = true;
						FTPretryCount = 1;
						while (FTPretryCount <= 3 && TryAgain)
						{
							int seconds = 60;
							if (FTPretryCount == 2)
							{
								seconds = 120;
							}
							else if (FTPretryCount >= 2)
							{
								seconds = 180;
							}
							spoof.Messages.Add("FTP call failed, will sleep for " + seconds.ToString() + "s and try again. Retry count:" + FTPretryCount.ToString());
							LogMessageLine("FTP call failed, will sleep for " + seconds.ToString() + "s and try again. Retry count:" + FTPretryCount.ToString());
							System.Threading.Thread.Sleep(seconds * 1000);
							messages = new List<string>();
							(Pathname, messages) = WebRequestGet.getFTPBinaryFiles(FTPBaselineFolder, pubMedFileName);
							if (messages != null && messages.Count > 0)
							{//still didn't work!
								spoof.ErrorCount++;
								spoof.Messages.AddRange(messages);
								FTPretryCount++;
							}
							else
							{
								spoof.Success = true;
								canProceed = true;
								FTPretryCount = 0;
								TryAgain = false;
							}
						}
						result.ProcessedFilesResults.Add(spoof);
					}
					else
					{
						canProceed = true;
					}
					if (canProceed)
						result.ProcessedFilesResults.Add(FileParser.ParseFile(@"TmpFiles\" + Pathname));
					if (currCount >= maxCount) break;
				}
			}
		}

		static void ShowHelp()
		{
			FileStream fileStream = new FileStream("QuickHelp.txt", FileMode.Open);
			using (StreamReader reader = new StreamReader(fileStream))
			{
				while (!reader.EndOfStream)
				{
					Console.WriteLine(reader.ReadLine());
				}
			}
		}
        public static void LogException(Exception e, string Description)
        {
            LogMessageLine(Description);
            if (e.Message != null && e.Message != "")
                LogMessageLine("MSG: " + e.Message);
            if (e.StackTrace != null && e.StackTrace != "")
                LogMessageLine("STACK TRC:" + e.StackTrace);
            if (e.InnerException != null)
            {
                LogMessageLine("Inner Exception(s): ");
                Exception ie = e.InnerException;
                int i = 0;
                while (ie != null && i < 10)
                {
                    i++;
                    if (ie.Message != null && ie.Message != "")
                        LogMessageLine("MSG(" + i.ToString() + "): " + ie.Message);
                    if (ie.StackTrace != null && ie.StackTrace != "")
                        LogMessageLine("STACK TRC(" + i.ToString() + "):" + ie.StackTrace);
                    ie = ie.InnerException;
                }
            }
        }
        public static void LogMessageLine(string line)
		{//will also log multiple lines, TBH - this method ensures line ends with NewLine
			if (!line.EndsWith(Environment.NewLine)) line += Environment.NewLine;
			LogMessageString(line);
		}
		public static void LogMessageString(string MessageToLog)
		{//can be used when we want to progressively append without adding a newline after each addition
			if (MessageToLog == null || MessageToLog == "") return;
			if (SaveLog)
			{
				if (LogFileFullPath == "")
				{
					LogFileFullPath = CreateLogFileName();
				}
				File.AppendAllText(LogFileFullPath, MessageToLog);
			}
			Console.Write(MessageToLog);
		}
		private static string CreateLogFileName()
		{
			DirectoryInfo logDir = System.IO.Directory.CreateDirectory("LogFiles");
			string LogFilename = logDir.FullName + @"\" + "PubmedImportLog-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
			//if (!System.IO.File.Exists(LogFilename)) System.IO.File.Create(LogFilename);
			return LogFilename;
		}
		public static string Duration(DateTime starttime)
		{
			double ms = DateTime.Now.Subtract(starttime).TotalMilliseconds;
			string duration = Math.Round(ms).ToString() + "ms.";
			if (ms > 180000)
			{
				ms = DateTime.Now.Subtract(starttime).TotalMinutes;
				duration = Math.Round(ms).ToString() + "m.";
			}
			else if (ms > 4000)
			{
				duration = Math.Round(ms / 1000).ToString() + "s.";
			}
			return duration;
		}
	}
	public class PubMedUpdateFileImport
	{
		public DateTime UploadDate { get; set; }
		public string FileName { get; set; }
		public DateTime ImportDate { get; set; }

        public void SaveSelf(SqlConnection conn)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            Parameters.Add(new SqlParameter("@PUBMED_FILE_NAME", FileName));
            Parameters.Add(new SqlParameter("@PUBMED_UPLOAD_DATE", UploadDate));
            SQLHelper.ExecuteNonQuerySP(conn, "st_PubMedUpdateFileInsert", Parameters.ToArray());
        }

        public static PubMedUpdateFileImport GetPubMedUpdateFileImport(SqlDataReader reader)
        {
            PubMedUpdateFileImport res = new PubMedUpdateFileImport();
           
            res.UploadDate = (DateTime)reader["PUBMED_UPLOAD_DATE"];
            res.ImportDate = (DateTime)reader["PUBMED_IMPORT_DATE"];
            res.FileName = reader["PUBMED_FILE_NAME"].ToString();
            return res;
        }

        public override bool Equals(object obj)
		{//we use this to quickly find if already uploaded files contain ones we may want to download & process.
		 //adapted from: http://codebetter.com/davidhayden/2005/02/22/object-identity-vs-object-equality-overriding-system-object-equalsobject-obj/
			if (obj == null) return false;
			if (Object.ReferenceEquals(this, obj)) return true;
			if (this.GetType() != obj.GetType()) return false;

			PubMedUpdateFileImport incoming = (PubMedUpdateFileImport)obj;
			if (incoming.FileName == this.FileName
				&&
				incoming.UploadDate == this.UploadDate
				) return true;//the two objects are the same IFF the upload dates are the same (as well as filename)
							  //i.e. if the filename has been used, but upload date (when it was put on the FTP folder) is different, it might have changed...

			return false;
		}
		public override int GetHashCode()
		{
			string toHash = this.FileName + this.UploadDate.ToBinary().ToString();
			return toHash.GetHashCode();
		}
	}
	public class PubMedUpdateFileImportJobLog 
	{
		public bool IsDeleting { get; set; }
		public int TotalErrorCount { get; set; }
		private string _Summary;
		public string DoWhat { get; set; }
		public string Summary
		{
			get
			{
				return _Summary;
			}
			set
			{
				_Summary = value;
				EndTime = DateTime.Now;
			}
		}
		public string[] Arguments { get; set; }
		public List<FileParserResult> ProcessedFilesResults { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool HasErrors
		{
			get { return (TotalErrorCount > 0); }
		}
		public PubMedUpdateFileImportJobLog(string[] arguments)
		{
			Arguments = arguments;
			if (Arguments.Contains("deleterecords")) IsDeleting = true;
			else IsDeleting = false;
			StartTime = DateTime.Now;
			ProcessedFilesResults = new List<FileParserResult>();
		}
		public void UpdateErrors()
		{
			foreach (FileParserResult step in ProcessedFilesResults)
			{
				if (step.ErrorCount > 0) TotalErrorCount = TotalErrorCount + step.ErrorCount;
			}
		}
	}
	public static class WebRequestGet
	{
		public static (List<string> files, List<string> messages) GetPubMedBaselineFilesList(string RemoteFtpPath)
		{
			string Username = "anonymous";
			string Password = "";
			List<string> messages = new List<string>();
			List<string> filesList = new List<string>();

			FtpWebRequest fwr = (FtpWebRequest)FtpWebRequest.Create(new Uri(RemoteFtpPath));
			fwr.Credentials = new NetworkCredential(Username, Password);
			fwr.Method = WebRequestMethods.Ftp.ListDirectory;
			fwr.UsePassive = true;//otherwise some Azure firewall protests! https://stackoverflow.com/questions/4604693/ftp-throws-webexception-only-in-net-4-0
			try
			{ 
				//Can object of type StreamReader as given below
				StreamReader sr = new StreamReader(fwr.GetResponse().GetResponseStream());
				string str = sr.ReadLine();

				while (str != null)
				{
					if (str.ToLower().EndsWith(".xml.gz"))
					{
						filesList.Add(str);
					}
					str = sr.ReadLine();
				}
			}
			catch (Exception e)
			{
				LogFTPexceptionSafely(e, messages, "fetching list of Baseline files");
			}
			return (filesList, messages);
		}
		public static (string localfile, List<string> messages) getFTPBinaryFiles(string FullFtpFilePath)
		{
			int filenameIndex = FullFtpFilePath.LastIndexOf('/');
			if (filenameIndex < 1 || FullFtpFilePath.IndexOf(".xml.gz") < 1)
			{
				List<string> failing = new List<string>();
				failing.Add("Failing: tried to process an unexpected file, not '.xml.gz' or malformed URL.");
				Program.LogMessageLine(failing[0]);
				return ("", failing);
			}
			string basepath = FullFtpFilePath.Substring(0, filenameIndex + 1);
			string ftpFileName = FullFtpFilePath.Substring(filenameIndex + 1);

			return getFTPBinaryFiles(basepath, ftpFileName);
		}
		public static (string localfile, List<string> messages) getFTPBinaryFiles(string basePath, String FtpFileName)
		{
			string RemoteFtpPath = basePath + FtpFileName;
			List<string> messages = new List<string>();
			int index = FtpFileName.IndexOf(".gz");
			string unZippedFileName = FtpFileName.Substring(0, index);
			string LocalDestinationPath = Directory.GetCurrentDirectory() + @"\Tmpfiles\" + FtpFileName;
			string Username = "anonymous";
			string Password = "";
			bool UseBinary = true; // use true for .zip file or false for a text file
			Program.LogMessageLine("Downloading " + RemoteFtpPath + ".");
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create(RemoteFtpPath);
			request.Method = WebRequestMethods.Ftp.DownloadFile;
			request.KeepAlive = true;
			request.UsePassive = true;
			request.UseBinary = UseBinary;

			request.Credentials = new NetworkCredential(Username, Password);
			try
			{
				FtpWebResponse response = (FtpWebResponse)request.GetResponse();

				Stream responseStream = response.GetResponseStream();
				//StreamReader reader = new StreamReader(responseStream);

				using (FileStream writer = new FileStream(LocalDestinationPath, FileMode.Create))
				{

					long length = response.ContentLength;
					int bufferSize = 2048;
					int readCount;
					byte[] buffer = new byte[2048];

					readCount = responseStream.Read(buffer, 0, bufferSize);
					while (readCount > 0)
					{
						writer.Write(buffer, 0, readCount);
						readCount = responseStream.Read(buffer, 0, bufferSize);
					}
				}
				FileInfo fileToBeGZipped = new FileInfo(LocalDestinationPath);
				string decompressedFileName = Directory.GetCurrentDirectory() + @"\Tmpfiles\" + unZippedFileName;
				Program.LogMessageLine("Decompressing " + RemoteFtpPath + ".");
				using (FileStream fileToDecompressAsStream = fileToBeGZipped.OpenRead())
				{
					using (FileStream decompressedStream = File.Create(decompressedFileName))
					{
						using (GZipStream decompressionStream = new GZipStream(fileToDecompressAsStream, CompressionMode.Decompress))
						{
							try
							{
								decompressionStream.CopyTo(decompressedStream);
							}
							catch (Exception ex)
							{
								LogFTPexceptionSafely(ex, messages, "uncompressing file to parse.");
							}
						}
					}
				}
				if (File.Exists(fileToBeGZipped.FullName))
				{
					try
					{
						File.Delete(fileToBeGZipped.FullName);
					}
					catch (Exception ex)
					{
						
						LogFTPexceptionSafely(ex, messages, "deleting the compressed file.");
					}
				}
			}
			catch (Exception e)
			{
				//"Unable to connect to the remote server" "The operation has timed out."
				LogFTPexceptionSafely(e, messages, "fetching the file to parse.");

			}
			return (unZippedFileName, messages);
		}
		public static (List<PubMedUpdateFileImport> fileList, List<string> messages) getUpdateFTPFiles(string updateFTPPath)
		{
			List<PubMedUpdateFileImport> fileList = new List<PubMedUpdateFileImport>();
			List<PubMedUpdateFileImport> MatchingFilesList = new List<PubMedUpdateFileImport>();
			List<string> messages = new List<string>();
			
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create(updateFTPPath);
			request.Method = WebRequestMethods.Ftp.ListDirectory;
			string Username = "anonymous";
			string Password = "";
			request.Credentials = new NetworkCredential(Username, Password);
			request.UsePassive = true;
			List<string> tmpFileList = new List<string>();
			try
			{
				using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
				{
					Stream responseStream = response.GetResponseStream();
					StreamReader reader = new StreamReader(responseStream);

					while (!reader.EndOfStream)
					{
						string str = reader.ReadLine();
						if (str.ToLower().EndsWith(".xml.gz"))
							tmpFileList.Add(str);
					}
				}
			}
			catch (Exception e)
			{
				LogFTPexceptionSafely(e, messages, "fetching list of update files.");
			}
			if (tmpFileList == null)
			{
				tmpFileList = new List<string>();
			}
			
			tmpFileList.Sort();
			Uri ftp = new Uri(updateFTPPath);
			PubMedUpdateFileImport tmp = new PubMedUpdateFileImport();
            //now get the list of files we have imported already...
            List<PubMedUpdateFileImport> knownUpdateFiles = new List<PubMedUpdateFileImport>();
            try
            {
                using (SqlDataReader reader = SQLHelper.ExecuteQuerySP(Program.SqlHelper.DataServiceDB, "st_PubMedUpdateFileGetAll"))
                {
                    //if (!reader.IsClosed)
                    //{
                        while (reader.Read())
                        {
                            knownUpdateFiles.Add(PubMedUpdateFileImport.GetPubMedUpdateFileImport(reader));
                        }
                    //}
                }
            }
            catch (Exception e)
            {
                Program.LogException(e, "FATAL ERROR fetching list of already processed UpdateFiles.");
                Program.LogMessageLine("Aborting...");
                Program.LogMessageLine("");
                System.Environment.Exit(0);
            }

            foreach (var f in tmpFileList)
            {
                try
                {
                    FtpWebRequest req = (FtpWebRequest)WebRequest.Create(new Uri(ftp, f));
                    req.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                    req.UsePassive = true;
                    req.Credentials = new NetworkCredential(Username, Password);

                    using (FtpWebResponse resp = (FtpWebResponse)req.GetResponse())
                    {
                        //the commented lines are renmant for an approach that wouldn't work.
                        //you can't get all records from Raven (either 128 or up to 1024, if you don't change defaults)
                        tmp = new PubMedUpdateFileImport() { FileName = f, UploadDate = resp.LastModified };
                        //if (!ProcessedfileList.Contains(tmp)) fileList.Add(tmp);
                    }
                }
                catch (Exception e)
                {
                    LogFTPexceptionSafely(e, messages, "fetching list of update files.");
                    break;
                }
                try
                {
                    MatchingFilesList = knownUpdateFiles.Where(x => x.FileName == f).ToList();
                    if (MatchingFilesList.Count == 0)//we haven't processed this file
                    {
                        fileList.Add(tmp);
                    }
                    else
                    {//we have processed this file already, does the one on FTP Pubmed folder have a timestamp that is newer?
                        bool WeNeedThis = true;
                        foreach (PubMedUpdateFileImport processed in MatchingFilesList)
                        {
                            if (processed.UploadDate >= tmp.UploadDate)// we imported this *after* the "last modified" timestamp of PubMed file (in FTP folder), so we don't need this file
                                WeNeedThis = false;
                        }
                        if (WeNeedThis) fileList.Add(tmp);
                    }
                }
                catch (Exception e)
                {
                    messages.Add("Catastrophic FAILURE: could not fetch list of already imported UpdateFiles.");
                    Program.LogMessageLine("Catastrophic FAILURE: could not fetch list of already imported UpdateFiles.");
                    messages.Add("Error Message: " + e.Message);
                    Program.LogMessageLine("Error Message: " + e.Message);
                    return (fileList, messages);
                }

            }

			//fileList = fileList.Where(p => p.UploadDate >= DateTime.Today.AddDays(daysbehind)).ToList();

			return (fileList, messages);
		}
		private static void LogFTPexceptionSafely(Exception e, List<string> messages, string doingWhat)
		{
			if (e == null || e.Message == null || e.Message == "")
			{
				messages.Add("Unknown error " + doingWhat);
			}
			else
			{
				messages.Add("Error " + doingWhat + " At time: " + DateTime.Now.ToString("HH:mm:ss"));
				Program.LogMessageLine("Error " + doingWhat);
				messages.Add(e.Message);
				Program.LogMessageLine(e.Message);
				Program.LogMessageLine(e.StackTrace);
				messages.Add(e.StackTrace);
			}
		}
	}
}
