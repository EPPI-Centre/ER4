using BusinessLibrary.BusinessClasses;
using Csla;
using Csla.Data;
using Humanizer;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;
using Serilog;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace ER_Web.Services
{
    public class LongLastingTaskResumer
    {
        private readonly Serilog.ILogger Logger;

        internal LongLastingTaskResumer(Serilog.ILogger logger)
        {
            Logger = logger;
        }

        internal void ResumePausedJobs()
        {
            LogInfoMessage("LongLastingTaskResumer is starting the resume task");
            Task.Run(()=>ActuallyResumeWithDelay());

            LogInfoMessage("LongLastingTaskResumer is returning");
        }

        private void ActuallyResumeWithDelay()
        { //use  Log.Error(...) to log exceptions to file
            try
            {
                int delay = 1000 * 1;//will wait this delay in ms

                LogInfoMessage("LongLastingTaskResumer fired and forgotten task is about to sleep for " + delay.ToString() + "ms.");
                Thread.Sleep(delay);
                List<RawTaskToResume> pausedTasks = GetTasksToResume();
                foreach (RawTaskToResume taskToResume in pausedTasks)
                {
                    try
                    {
                        var type = Type.GetType("BusinessLibrary.BusinessClasses." + taskToResume.ClassName);
                        if (type != null && type.GetInterface("iResumableLongLastingTask") != null)
                        {
                            var instance = Activator.CreateInstance(type) as iResumableLongLastingTask;
                            if (instance != null)
                            {
                                LogInfoMessage("LongLastingTaskResumer instantiated an object of type: " + taskToResume.ClassName);
                                //var pars = taskToResume.GetParamsList();
                                LogInfoMessage("LongLastingTaskResumer instantiated an object of type: " + taskToResume.ClassName);
                                Task.Run(() => instance.ResumeJob(taskToResume));
                            }
                            else
                            {
                                LogErrorMessage("LongLastingTaskResumer failed to instantiate an object of type: " + taskToResume.ClassName);
                            }
                        }
                        else
                        {
                            LogErrorMessage("LongLastingTaskResumer failed to GetType for: " + taskToResume.ClassName);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }
                }
                LogInfoMessage("LongLastingTaskResumer fired and forgotten task is done.");
            } 
            catch(Exception ex)
            {
                LogException(ex);
            }
        }

        private List<RawTaskToResume> GetTasksToResume()
        {
            List<RawTaskToResume> res = new List<RawTaskToResume>();

            using (SqlConnection connection = new SqlConnection(BusinessLibrary.Data.DataConnection.ConnectionString))
            {
                connection.Open();
                string SQLcmd = "SELECT * from TB_REVIEW_JOB where (SUCCESS = 0 OR SUCCESS is null) AND END_TIME > DATEADD(hour, -15,  GETDATE()) and CURRENT_STATE like 'Cancelled%'";
                using (SqlCommand command = new SqlCommand(SQLcmd, connection))
                {
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read()) 
                        {
                            RawTaskToResume tsk = new RawTaskToResume(reader);
                            if (tsk.ClassName != "unknown") res.Add(tsk);
                        }
                    }
                }
            }

            //KeyValuePair<string, object> t = new KeyValuePair<string, object>("RunType", "ApplyClassifier");
            //List<KeyValuePair<string, object>> pps = new List<KeyValuePair<string, object>>();
            //pps.Add(t);
            //res.Add(new RawTaskToResume(7, 1214, 1085, "ClassifierCommandV2", "Apply Classifier", "Cancelled during DF", "26b48927-046f-4b2e-bf42-a701a1dbb1b6", Newtonsoft.Json.JsonConvert.SerializeObject(pps)));
            //res.Add(new RawTaskToResume(7, 1214, 112, "TrainingRunCommandV2", "bbb", "Cancelled during DF"));
            return res;
        }


        private void LogInfoMessage(string message)
        {
            Log.Information("");
            Log.Information(message);
            Debug.WriteLine("");
            Debug.WriteLine(message);
            Debug.WriteLine("");
        }
        private void LogException(Exception ex)
        {
            try
            {
                Exception? t = ex;
                int counter = 0;
                while (t != null)
                {
                    if (counter == 0)
                    {
                        LogErrorMessage("Exception in LongLastingTaskResumer:");
                    }
                    else LogErrorMessage("Inner Exception (" + counter.ToString() + "):");
                    LogErrorMessage(t.Message);
                    if (t.StackTrace != null) LogErrorMessage(t.StackTrace);
                    t = t.InnerException;
                    counter++;
                }
                LogErrorMessage("");
            }
            catch { }
        }
        private void LogErrorMessage(string message)
        {
            Log.Error(message);
            Debug.WriteLine(message);
        }
    }
    public class RawTaskToResume
    {
        public string ClassName { get; private set; }
        public string ParamsInJson { get; private set; }
        public string CancelState { get; private set; }
        public string JobType { get; private set; }
        public string? DataFactoryRunId { get; private set; }
        public int ReviewId { get; private set; }
        public int JobId { get; private set; }
        public int ContactId { get; private set; }
        public RawTaskToResume(int reviewId, int contactId, int jobId, string classname, string jobType, string cancelState, string? dataFactoryRunId, string paramsInJson)
        {
            JobId = jobId;
            ReviewId = reviewId;
            ContactId = contactId;
            ClassName = classname;
            JobType = jobType;
            ParamsInJson = paramsInJson;
            CancelState = cancelState;
            DataFactoryRunId = dataFactoryRunId;
        }
        public RawTaskToResume(SafeDataReader reader)
        {
            JobId = reader.GetInt32("REVIEW_JOB_ID");
            ReviewId = reader.GetInt32("REVIEW_ID");
            ContactId = reader.GetInt32("CONTACT_ID");
            JobType = reader.GetString("JOB_TYPE");
            if (JobType == "Apply Classifier" || JobType == "Apply Classifier to OA run" || JobType == "Build Classifier"
                || JobType == "Check Screening" || JobType == "Priority screening simulation") ClassName = "ClassifierCommandV2";
            else ClassName = "unknown";
            ParamsInJson = reader.GetString("RESUME_PARAMETERS");
            CancelState = reader.GetString("CURRENT_STATE");
            string lookingForDfId = reader.GetString("JOB_MESSAGE");
            string[] splitted = lookingForDfId.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in splitted) 
            {
                if (s.StartsWith("DF RunId: "))
                {
                    DataFactoryRunId = s.Substring(9);
                    break;
                }
            }
        }
        public List<KeyValuePair<string, object>> GetParamsList()
        {
            List<KeyValuePair<string, object>> res = new List<KeyValuePair<string, object>>();
            if (DataFactoryRunId != null)
            {
                res.Add(new KeyValuePair<string, object>("DfRunId", DataFactoryRunId));
            }
            var paramss = Newtonsoft.Json.JsonConvert.DeserializeObject(ParamsInJson);
            foreach(var p in (paramss as JArray))
            {
                res.Add(new KeyValuePair<string, object>(p["Key"].ToString(), ((string?)p["Value"])));
            }
            return res;
        }
    }
}

