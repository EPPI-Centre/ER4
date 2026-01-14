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
using Csla.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Humanizer;




#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class OpenAlexOriginReportCommand : CommandBase<OpenAlexOriginReportCommand>
    {
        public OpenAlexOriginReportCommand() { }

        private Int64 _attId;
        private OaOriginSummary _Summary;

        public List<MagAutoUpdateRun> MagAutoUpdateRunList = new List<MagAutoUpdateRun>();
        public List<MagRelatedPapersRun> MagRelatedSearches = new List<MagRelatedPapersRun>();    
        public List<OaOriginReportItem> Items = new List<OaOriginReportItem>();

        public OpenAlexOriginReportCommand(Int64 attId)
        {
            _attId = attId;
        }

        public OaOriginSummary Summary
        {
            get { return _Summary; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_attId", _attId);
            //info.AddValue("_numCodings", _numCodings);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _attId = info.GetValue<Int64>("_attId");
            //_numCodings = info.GetValue<int>("_numCodings");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OpenAlexOriginReport", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@attributeid", _attId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        //first reader is one summary line
                        if (reader.Read()) 
                        {
                            _Summary = new OaOriginSummary(reader);
                        }
                        reader.NextResult();
                        //second reader is the list of auto update runs found
                        while (reader.Read()) 
                        {
                            MagAutoUpdateRunList.Add(MagAutoUpdateRun.GetMagAutoUpdateRun(reader));
                        }
                        reader.NextResult();
                        //3rd reader is the list of Related searches found
                        while (reader.Read()) 
                        { 
                            MagRelatedSearches.Add(MagRelatedPapersRun.GetMagRelatedPapersRun(reader));
                        }
                        reader.NextResult();
                        //4th and last reader contains all items (sometimes multiple row per item)
                        Int64 previousItemId = 0, currentItemId = 0, PaperId = 0;
                        OaOriginReportItem tmpItem = new OaOriginReportItem();
                        while (reader.Read())
                        {
                            currentItemId = reader.GetInt64("item_id");
                            if (previousItemId != currentItemId)
                            {
                                tmpItem = new OaOriginReportItem(reader);
                                Items.Add(tmpItem);
                                previousItemId = currentItemId;
                            }
                            PaperId = reader.GetInt64("PaperId");
                            if (PaperId > 0 && !tmpItem.OpenAlexPaperId.Contains(PaperId))
                            {
                                tmpItem.OpenAlexPaperId.Add(PaperId);
                            }
                            if (reader.GetBoolean("IsInAU") == true)
                            {
                                tmpItem.AutoUpdateResults.Add(reader.GetInt32("MAG_AUTO_UPDATE_RUN_ID"));
                            }
                            if (reader.GetBoolean("IsInRS") == true)
                            {
                                tmpItem.AutoUpdateResults.Add(reader.GetInt32("MAG_RELATED_RUN_ID"));
                            }
                        }
                    }
                }
                connection.Close();
            }
        }

#endif
    }
    public class OaOriginReportItem
    {
        public Int64 ItemId { get; private set; }
        public List<Int64> OpenAlexPaperId { get; private set; } = new List<Int64>();
        public string Title { get; private set; } = string.Empty;
        public string ShortTitle { get; private set; } = string.Empty;
        public string SourceName { get; private set; } = string.Empty;

        public List<int> RelatedSearches { get; private set; } = new List<int>();

        public List<int> AutoUpdateResults { get; private set; } = new List<int>();
        public OaOriginReportItem() 
        {
            ItemId = 0;
        }
        public OaOriginReportItem(SafeDataReader reader) 
        {
            ItemId = reader.GetInt64("item_id");
            long tmp = reader.GetInt64("PaperId");
            if (tmp > 0) OpenAlexPaperId.Add(tmp);
            Title = reader.GetString("TITLE");
            ShortTitle = reader.GetString("SHORT_TITLE");
            SourceName = reader.GetString("SOURCE_NAME");
        }
    }
    public class OaOriginSummary
    {
        public int TotalItems { get; private set; } = 0;
        public int Matched { get; private set; } = 0;
        public int NotMatched { get; private set; } = 0;
        public int InAutoUpdateResults { get; private set; } = 0;
        public int InRelatedSearches { get; private set; } = 0;
        public int InBoth { get; private set; } = 0;
        public int OtherMatched { get; private set; } = 0;
        public OaOriginSummary() { }
        public OaOriginSummary(SafeDataReader reader) 
        {
            TotalItems = reader.GetInt32("Computed Total");
            if (TotalItems != reader.GetInt32("TotalIR"))
            {
                throw new Exception("Error. Report contains inconsistent data, please contact EPPISupport.");
            }
            Matched = reader.GetInt32("Matched");
            NotMatched = reader.GetInt32("Not Matched");
            InAutoUpdateResults = reader.GetInt32("In AutoUpdate results");
            InRelatedSearches = reader.GetInt32("In Related Searches");
            InBoth = reader.GetInt32("in both");
            OtherMatched = reader.GetInt32("Other");
        }
    }
}
