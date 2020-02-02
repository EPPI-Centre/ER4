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

#if!SILVERLIGHT
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
            MagPaperItemMatch.MatchItemToMag(_ITEM_ID);
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.AcademicControllerConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = SpecifyCommand(connection, ri))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@RESULT", 0));
                    command.Parameters["@RESULT"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    string res = command.Parameters["@RESULT"].Value.ToString();
                    if (command.CommandText == "st_MatchItemsToPapersSingleItem")
                    {
                        _currentStatus = "The automated matching identified " +
                       command.Parameters["@RESULT"].Value.ToString() + (res == "1" ? " possible match" :
                       " possible matches");
                    }
                    else
                    {
                        _currentStatus = "Successfully added this review to the queue to be auto-matched";
                    }
                   
                }
                connection.Close();
            }
            */
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
