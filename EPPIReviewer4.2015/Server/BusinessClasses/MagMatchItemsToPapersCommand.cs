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
using System.Threading.Tasks;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagMatchItemsToPapersCommand : CommandBase<MagMatchItemsToPapersCommand>
    {

#if SILVERLIGHT
    public MagMatchItemsToPapersCommand(){}
#else
        public MagMatchItemsToPapersCommand() { }
#endif

        private bool _AllInReview;
        private Int64 _ITEM_ID;
        private Int64 _ATTRIBUTE_ID;
        private string _FindOrRemove;
        private string _currentStatus;
        [Newtonsoft.Json.JsonProperty]
        public string currentStatus
        {
            get
            {
                return _currentStatus;
            }
        }

        public MagMatchItemsToPapersCommand(string FindOrRemove, bool AllInReview, Int64 ItemId, Int64 AttributeId)
        {
            _FindOrRemove = FindOrRemove;
            _AllInReview = AllInReview;
            _ATTRIBUTE_ID = AttributeId;
            _ITEM_ID = ItemId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_FindOrRemove", _FindOrRemove);
            info.AddValue("_AllInReview", _AllInReview);
            info.AddValue("_ITEM_ID", _ITEM_ID);
            info.AddValue("_ATTRIBUTE_ID", _ATTRIBUTE_ID);
            info.AddValue("_currentStatus", _currentStatus);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _FindOrRemove = info.GetValue<string>("_FindOrRemove");
            _AllInReview = info.GetValue<bool>("_AllInReview");
            _currentStatus = info.GetValue<string>("_currentStatus");
            _ITEM_ID = info.GetValue<Int64>("_ITEM_ID");
            _ATTRIBUTE_ID = info.GetValue<Int64>("_ATTRIBUTE_ID");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (_FindOrRemove == "FindMatches")
            {
                if (_ITEM_ID > 0)
                {
                    MagPaperItemMatch.MatchItemToMag(_ITEM_ID, ri.ReviewId);
                }
                else
                {
                    
                    List<Int64> ItemIds = new List<long>();
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_MagMatchItemsGetIdList", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _ATTRIBUTE_ID));
                            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                            {
                                while (reader.Read())
                                {
                                    ItemIds.Add(reader.GetInt64("ITEM_ID"));
                                }
                            }
                        }
                        connection.Close();
                    }
                    int total = ItemIds.Count;
                    if (total > 0)
                    {
                        if (total < 7)
                        {
                            foreach (Int64 ItemId in ItemIds)
                            {
                                MagPaperItemMatch.MatchItemToMag(ItemId, ri.ReviewId);
                            }
                        }
                        else
                        {
                            int NumPerList = total / 6;
                            List<Int64> List1 = new List<long>();
                            List<Int64> List2 = new List<long>();
                            List<Int64> List3 = new List<long>();
                            List<Int64> List4 = new List<long>();
                            List<Int64> List5 = new List<long>();
                            List<Int64> List6 = new List<long>();
                            PutItemsInList(List1, ItemIds, 0, NumPerList);
                            PutItemsInList(List2, ItemIds, NumPerList, NumPerList);
                            PutItemsInList(List3, ItemIds, NumPerList * 2, NumPerList);
                            PutItemsInList(List4, ItemIds, NumPerList * 3, NumPerList);
                            PutItemsInList(List5, ItemIds, NumPerList * 4, NumPerList);
                            PutItemsInList(List6, ItemIds, NumPerList * 5, total - (NumPerList * 5));
                            Task.Run(() => { FindPaper(List1, ri.ReviewId, "t1"); });
                            Task.Run(() => { FindPaper(List2, ri.ReviewId, "t2"); });
                            Task.Run(() => { FindPaper(List3, ri.ReviewId, "t3"); });
                            Task.Run(() => { FindPaper(List4, ri.ReviewId, "t4"); });
                            Task.Run(() => { FindPaper(List5, ri.ReviewId, "t5"); });
                            Task.Run(() => { FindPaper(List6, ri.ReviewId, "t6"); });
                        }
                    }
                }
            }
        }

        private void FindPaper(List<Int64> ItemIds, int ReviewId, string taskname = "default")
        {
            //int counter = 1;
            foreach (Int64 ItemId in ItemIds)
            {
               MagPaperItemMatch.MatchItemToMag(ItemId, ReviewId);
                //Console.WriteLine("Task: " + taskname + " item " + counter + " of " + ItemIds.Count);
                //counter++;
            }
            //Console.WriteLine("Finished Task: " + taskname + " done " + (counter -1).ToString() + " of " + ItemIds.Count);
        }

        private List<Int64> PutItemsInList(List<Int64> dest, List<Int64> source, int from, int count)
        {
            for (int x = from; x < from + count; x++)
            {
                dest.Add(source[x]);
            }
            return dest;
        }

        private SqlCommand SpecifyCommand(SqlConnection connection, ReviewerIdentity ri)
        {
            SqlCommand command = null;
            switch (_FindOrRemove)
            {
                case "FindMatches":
                    if (_AllInReview == true)
                    {
                        //command = new SqlCommand("st_MatchItemsToPapers", connection); - now doing via worker process
                        command = new SqlCommand("st_MatchItemsToPapersAddJob", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                    }
                    else if (_ATTRIBUTE_ID != 0)
                    {   // this is currently unused. Should probably add a job rather than run immediately
                        command = new SqlCommand("st_MatchItemsToPapersWithAttribute", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _ATTRIBUTE_ID));
                    }
                    else if (_ITEM_ID != 0)
                    {
                        command = new SqlCommand("st_MatchItemsToPapersSingleItem", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ITEM_ID", _ITEM_ID));
                    }
                    break;
                case "RemoveMatches":
                    if (_AllInReview == true)
                    {
                        command = new SqlCommand("st_MagMatchItemsRemove", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                    }
                    else if (_ATTRIBUTE_ID != 0)
                    {
                        command = new SqlCommand("st_MagMatchItemsRemove", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _ATTRIBUTE_ID));
                    }
                    else if (_ITEM_ID != 0)
                    {
                        command = new SqlCommand("st_MagMatchItemsRemove", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ITEM_ID", _ITEM_ID));
                    }
                    break;
            }
            return command;
        }

#endif


    }
}
