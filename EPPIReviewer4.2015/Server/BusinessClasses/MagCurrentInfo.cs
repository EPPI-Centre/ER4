using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using Csla.DataPortalClient;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

//using Csla.Configuration;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using System.Configuration;
using System.Data;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Linq;
#if CSLA_NETCORE
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
#else
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
#endif
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagCurrentInfo : BusinessBase<MagCurrentInfo>
    {
        public static void GetMagCurrentInfo(EventHandler<DataPortalResult<MagCurrentInfo>> handler)
        {
            DataPortal<MagCurrentInfo> dp = new DataPortal<MagCurrentInfo>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
    public MagCurrentInfo() { }

        
#else
        public MagCurrentInfo() { }
#endif
        public static readonly PropertyInfo<int> MagCurrentInfoIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MagCurrentInfoId", "MagCurrentInfoId", 0));
        public int MagCurrentInfoId
        {
            get
            {
                return GetProperty(MagCurrentInfoIdProperty);
            }
            set
            {
                SetProperty(MagCurrentInfoIdProperty, value);
            }
        }
       
        public static readonly PropertyInfo<string> MagFolderProperty = RegisterProperty<string>(new PropertyInfo<string>("MagFolder", "MagFolder", ""));
        public string MagFolder
        {
            get
            {
                return GetProperty(MagFolderProperty);
            }
            set
            {
                SetProperty(MagFolderProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> MatchingAvailableProperty = RegisterProperty<bool>(new PropertyInfo<bool>("MatchingAvailable", "MatchingAvailable", true));
        public bool MatchingAvailable
        {
            get
            {
                return GetProperty(MatchingAvailableProperty);
            }
            set
            {
                SetProperty(MatchingAvailableProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> MagOnlineProperty = RegisterProperty<bool>(new PropertyInfo<bool>("MagOnline", "MagOnline", true));
        public bool MagOnline
        {
            get
            {
                return GetProperty(MagOnlineProperty);
            }
            set
            {
                SetProperty(MagOnlineProperty, value);
            }
        }

        public static readonly PropertyInfo<DateTime> WhenLiveProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("WhenLive", "WhenLive"));
        public DateTime WhenLive
        {
            get
            {
                return GetProperty(WhenLiveProperty);
            }
            set
            {
                SetProperty(WhenLiveProperty, value);
            }
        }

        public static readonly PropertyInfo<string> MakesEndPointProperty = RegisterProperty<string>(new PropertyInfo<string>("MakesEndPoint", "MakesEndPoint", ""));
        public string MakesEndPoint
        {
            get
            {
                return GetProperty(MakesEndPointProperty);
            }
            set
            {
                SetProperty(MakesEndPointProperty, value);
            }
        }

        public static readonly PropertyInfo<string> MakesDeploymentStatusProperty = RegisterProperty<string>(new PropertyInfo<string>("MakesDeploymentStatus", "MakesDeploymentStatus", ""));
        public string MakesDeploymentStatus
        {
            get
            {
                return GetProperty(MakesDeploymentStatusProperty);
            }
            set
            {
                SetProperty(MakesDeploymentStatusProperty, value);
            }
        }

        public static readonly PropertyInfo<List<string>> ListMAGConatainersStatusProperty = RegisterProperty<List<string>>(new PropertyInfo<List<string>>("ListMAGConatainersStatus", "ListMAGConatainersStatus", new List<string> { "" }));
        public List<string> ListMAGConatainersStatus
        {
            get
            {
                return GetProperty(ListMAGConatainersStatusProperty);
            }
            set
            {
                SetProperty(ListMAGConatainersStatusProperty, value);
            }
        }

        /*
        public static readonly PropertyInfo<MagCurrentInfoList> CitationsProperty = RegisterProperty<MagCurrentInfoList>(new PropertyInfo<MagCurrentInfoList>("Citations", "Citations"));
        public MagCurrentInfoList Citations
        {
            get
            {
                return GetProperty(CitationsProperty);
            }
            set
            {
                SetProperty(CitationsProperty, value);
            }
        }
        public static readonly PropertyInfo<MagCurrentInfoList> CitedByProperty = RegisterProperty<MagCurrentInfoList>(new PropertyInfo<MagCurrentInfoList>("CitedBy", "CitedBy"));
        public MagCurrentInfoList CitedBy
        {
            get
            {
                return GetProperty(CitedByProperty);
            }
            set
            {
                SetProperty(CitedByProperty, value);
            }
        }
        public static readonly PropertyInfo<MagCurrentInfoList> RecommendedProperty = RegisterProperty<MagCurrentInfoList>(new PropertyInfo<MagCurrentInfoList>("Recommended", "Recommended"));
        public MagCurrentInfoList Recommended
        {
            get
            {
                return GetProperty(RecommendedProperty);
            }
            set
            {
                SetProperty(RecommendedProperty, value);
            }
        }
        public static readonly PropertyInfo<MagCurrentInfoList> RecommendedByProperty = RegisterProperty<MagCurrentInfoList>(new PropertyInfo<MagCurrentInfoList>("RecommendedBy", "RecommendedBy"));
        public MagCurrentInfoList RecommendedBy
        {
            get
            {
                return GetProperty(RecommendedByProperty);
            }
            set
            {
                SetProperty(RecommendedByProperty, value);
            }
        }
        
        public void GetRelatedFieldOfStudyList(string listType)
        {
            DataPortal<MagCurrentInfoList> dp = new DataPortal<MagCurrentInfoList>();
            dp.FetchCompleted += (o, e2) =>
            {
                if (e2.Object != null)
                {
                    if (e2.Error == null)
                    {
                        this.Citations = e2.Object;
                        //this.MarkClean(); // don't want the object marked as 'dirty' just because it's loaded a new list
                    }
                }
                if (e2.Error != null)
                {
#if SILVERLIGHT
                    System.Windows.MessageBox.Show(e2.Error.Message);
#endif
                }
            };
            MagCurrentInfoListSelectionCriteria sc = new BusinessClasses.MagCurrentInfoListSelectionCriteria();
            sc.MagCurrentInfoId = this.FieldOfStudyId;
            sc.ListType = listType;
            dp.BeginFetch(sc);
        }
        */



        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(MagCurrentInfo), admin);
        //    //AuthorizationRules.AllowDelete(typeof(MagCurrentInfo), admin);
        //    //AuthorizationRules.AllowEdit(typeof(MagCurrentInfo), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(MagCurrentInfo), canRead);

        //    //AuthorizationRules.AllowRead(MagCurrentInfoIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(NameProperty, canRead);
        //    //AuthorizationRules.AllowRead(DetailProperty, canRead);

        //    //AuthorizationRules.AllowWrite(NameProperty, canWrite);
        //    //AuthorizationRules.AllowRead(DetailProperty, canRead);
        //}

        protected override void AddBusinessRules()
        {
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT

        protected override void DataPortal_Insert()
        {

            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagCurrentInfoInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_FOLDER", ReadProperty(MagFolderProperty)));
                    command.Parameters.Add(new SqlParameter("@WHEN_LIVE", ReadProperty(WhenLiveProperty)));
                    command.Parameters.Add(new SqlParameter("@MAG_ONLINE", ReadProperty(MagOnlineProperty)));
                    command.Parameters.Add(new SqlParameter("@MAKES_ENDPOINT", ReadProperty(MakesEndPointProperty)));
                    command.Parameters.Add(new SqlParameter("@MAKES_DEPLOYMENT_STATUS", ReadProperty(MakesDeploymentStatusProperty)));

                    SqlParameter par = new SqlParameter("@MAG_CURRENT_INFO_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par);
                    command.Parameters["@MAG_CURRENT_INFO_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MagCurrentInfoIdProperty, command.Parameters["@MAG_CURRENT_INFO_ID"].Value);
                }
                connection.Close();
            }

        }

        protected override void DataPortal_Update()
        {
            
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagCurrentInfoUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_CURRENT_INFO_ID", ReadProperty(MagCurrentInfoIdProperty)));
                    //command.Parameters.Add(new SqlParameter("@MAG_VERSION", ReadProperty(MagVersionProperty)));
                    command.Parameters.Add(new SqlParameter("@WHEN_LIVE", ReadProperty(WhenLiveProperty)));
                    command.Parameters.Add(new SqlParameter("@MAKES_DEPLOYMENT_STATUS", ReadProperty(MakesDeploymentStatusProperty)));
                    //command.Parameters.Add(new SqlParameter("@MATCHING_AVAILABLE", ReadProperty(MatchingAvailableProperty)));
                    command.Parameters.Add(new SqlParameter("@MAG_ONLINE", ReadProperty(MagOnlineProperty)));
                    //command.Parameters.Add(new SqlParameter("@MAKES_ENDPOINT", ReadProperty(MakesEndPointProperty)));
                    //command.Parameters.Add(new SqlParameter("@MAKES_DEPLOYMENT_STATUS", ReadProperty(MakesDeploymentStatusProperty)));
                    command.ExecuteNonQuery();
                    MarkOld();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagCurrentInfoDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_CURRENT_INFO_ID", ReadProperty(MagCurrentInfoIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        // This is used in app.xaml - the whole ER4 app knows about current version and status of MAG (query returns 1 row)
        protected void DataPortal_Fetch()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagCurrentInfo", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAKES_DEPLOYMENT_STATUS", "LIVE")); // only need live info for client side
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<int>(MagCurrentInfoIdProperty, reader.GetInt32("MAG_CURRENT_INFO_ID"));
                            LoadProperty<string>(MagFolderProperty, reader.GetString("MAG_FOLDER"));
                            LoadProperty<DateTime>(WhenLiveProperty, reader.GetDateTime("WHEN_LIVE"));
                            LoadProperty<bool>(MatchingAvailableProperty, reader.GetBoolean("MATCHING_AVAILABLE"));
                            LoadProperty<bool>(MagOnlineProperty, reader.GetBoolean("MAG_ONLINE"));
                            LoadProperty<string>(MakesEndPointProperty, reader.GetString("MAKES_ENDPOINT")); // don't need to send this information back to the client (and probably shouldn't)
                            LoadProperty<string>(MakesDeploymentStatusProperty, reader.GetString("MAKES_DEPLOYMENT_STATUS"));
                            MarkOld();
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static MagCurrentInfo GetMagCurrentInfo(SafeDataReader reader)
        {
            MagCurrentInfo returnValue = new MagCurrentInfo();

            returnValue.LoadProperty<int>(MagCurrentInfoIdProperty, reader.GetInt32("MAG_CURRENT_INFO_ID"));
            returnValue.LoadProperty<string>(MagFolderProperty, reader.GetString("MAG_FOLDER"));
            returnValue.LoadProperty<DateTime>(WhenLiveProperty, reader.GetDateTime("WHEN_LIVE"));
            returnValue.LoadProperty<bool>(MatchingAvailableProperty, reader.GetBoolean("MATCHING_AVAILABLE"));
            returnValue.LoadProperty<bool>(MagOnlineProperty, reader.GetBoolean("MAG_ONLINE"));
            returnValue.LoadProperty<string>(MakesEndPointProperty, reader.GetString("MAKES_ENDPOINT")); // don't need to send this information back to the client (and probably shouldn't)
            returnValue.LoadProperty<string>(MakesDeploymentStatusProperty, reader.GetString("MAKES_DEPLOYMENT_STATUS"));
            returnValue.MarkOld();
            return returnValue;
        }

        // gets the current live MAG (query returns 1 row)
        public static MagCurrentInfo GetMagCurrentInfoServerSide(string MakesDeploymentStatus)
        {
            MagCurrentInfo returnValue = new MagCurrentInfo();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagCurrentInfo", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAKES_DEPLOYMENT_STATUS", MakesDeploymentStatus));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            returnValue.LoadProperty<int>(MagCurrentInfoIdProperty, reader.GetInt32("MAG_CURRENT_INFO_ID"));
                            returnValue.LoadProperty<string>(MagFolderProperty, reader.GetString("MAG_FOLDER"));
                            returnValue.LoadProperty<DateTime>(WhenLiveProperty, reader.GetDateTime("WHEN_LIVE"));
                            returnValue.LoadProperty<bool>(MatchingAvailableProperty, reader.GetBoolean("MATCHING_AVAILABLE"));
                            returnValue.LoadProperty<bool>(MagOnlineProperty, reader.GetBoolean("MAG_ONLINE"));
                            returnValue.LoadProperty<string>(MakesEndPointProperty, reader.GetString("MAKES_ENDPOINT"));
                            returnValue.LoadProperty<string>(MakesDeploymentStatusProperty, reader.GetString("MAKES_DEPLOYMENT_STATUS"));
                        }
                    }
                }
                connection.Close();
            }
            returnValue.MarkOld();
            return returnValue;
        }

        public static void UpdateMagCurrentInfoStatic()
        {
            var currentMagContainerName = UpdateMagCurrentInfoTableWithMostRecentMagDBOnAzureAsync();
            if (currentMagContainerName != "")
            {
                int startIndex = currentMagContainerName.IndexOf("-");
                string mag_version = currentMagContainerName.Substring(startIndex + 1, currentMagContainerName.Length - startIndex - 1).Replace("-", "/");
                mag_version = swapDateFormat(mag_version);
                string unformattedDate = currentMagContainerName.Substring(startIndex, currentMagContainerName.Length - startIndex).Replace("-", "");
                string makes_endpoint = " http://eppimag" + unformattedDate + ".westeurope.cloudapp.azure.com";
                UpdateSQLMagCurrentInfoTable(mag_version, makes_endpoint);
            }
      
        }

        private static string UpdateMagCurrentInfoTableWithMostRecentMagDBOnAzureAsync()
        {
            string storageAccountName = AzureSettings.MAGStorageAccount;
            string storageAccountKey = AzureSettings.MAGStorageAccountKey;

            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
            storageConnectionString += storageAccountKey;
#if (CSLA_NETCORE)
            BlobServiceClient blobClient = new BlobServiceClient(storageConnectionString);
            var magContainers = ListContainersWithPrefixAsync(blobClient, "mag-");
#else
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            //CloudBlobContainer container = blobClient.GetContainerReference("experiments");
            //here need  list
            var magContainers = ListContainersWithPrefixAsync(blobClient, "mag-");
#endif

            //use LINQ to list them in date order
            var orderedMagContainers = magContainers.OrderByDescending(x => x.Name);
            //MagCurrentInfo returnValue = new MagCurrentInfo();
            //returnValue.ListMAGConatainersStatus = orderedMagContainers.Select( x=> x.Name).ToList();

            var mostRecentMag = orderedMagContainers.FirstOrDefault();
            if (mostRecentMag != null)
            {
                return mostRecentMag.Name;
            }
            else
            {
                return String.Empty;
            }
        }

#if (CSLA_NETCORE)
        private static IEnumerable<BlobContainerItem> ListContainersWithPrefixAsync(BlobServiceClient blobClient,
                                                        string prefix)
        {
            try
            {
                IEnumerable<BlobContainerItem> resultSegment = null;
                CancellationToken continuationToken = new CancellationToken();
                resultSegment = blobClient.GetBlobContainers(BlobContainerTraits.None,BlobContainerStates.None, prefix, continuationToken);
                return resultSegment;

            }
            catch (RequestFailedException e)
            {
                Console.WriteLine("HTTP error code {0} : {1}",
                                    e.ErrorCode,
                                    e.Message);
                return null;
            }
        }
#else
        private static IEnumerable<CloudBlobContainer> ListContainersWithPrefixAsync(CloudBlobClient blobClient,
                                                        string prefix)
        {
            try
            {
                BlobContinuationToken continuationToken = null;
                ContainerResultSegment resultSegment = null;
                do
                {
                    resultSegment = blobClient.ListContainersSegmentedAsync(
                        prefix, ContainerListingDetails.Metadata, 5, continuationToken, null, null).Result;


                    // Get the continuation token. If not null, get the next segment.
                    continuationToken = resultSegment.ContinuationToken;

                } while (continuationToken != null);

                return resultSegment.Results;

            }
            catch (StorageException e)
            {
                Console.WriteLine("HTTP error code {0} : {1}",
                                    e.RequestInformation.HttpStatusCode,
                                    e.RequestInformation.ErrorCode);
                Console.WriteLine(e.Message);
                return null;
            }
        }
#endif
        private static string swapDateFormat(string mag_version)
        {
            var date = DateTime.ParseExact(mag_version, "yyyy/MM/dd",
                                   CultureInfo.InvariantCulture);

            return date.ToString("dd/MM/yyy");
        }

        public static Task UpdateSQLMagCurrentInfoTable(string mag_version, string makes_endpoint)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagUpdateCurrentInfoLatestMag", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_VERSION", mag_version));
                    command.Parameters.Add(new SqlParameter("@WHEN_LIVE", DateTime.Now));
                    command.Parameters.Add(new SqlParameter("@MATCHING_AVAILABLE", 1));
                    command.Parameters.Add(new SqlParameter("@MAG_ONLINE", 1));
                    command.Parameters.Add(new SqlParameter("@MAKES_ENDPOINT", makes_endpoint));
                    command.Parameters.Add(new SqlParameter("@MAKES_DEPLOYMENT_STATUS", "LIVE"));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return Task.CompletedTask;
        }


#endif
    }
}