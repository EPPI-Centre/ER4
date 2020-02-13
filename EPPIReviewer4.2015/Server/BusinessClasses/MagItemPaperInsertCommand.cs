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
using System.Globalization;


#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using AuthorsHandling;
using BusinessLibrary.Security;
using System.Data;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagItemPaperInsertCommand : CommandBase<MagItemPaperInsertCommand>
    {

#if SILVERLIGHT
    public MagItemPaperInsertCommand(){}
#else
        public MagItemPaperInsertCommand() { }
#endif

        private string _PaperIds;
        private int _NImported;
        private string _SourceOfIds;
        private int _MagRelatedRunId;

        public int NImported
        {
            get
            {
                return _NImported;
            }
            set
            {
                _NImported = value;
            }
        }

        public MagItemPaperInsertCommand(string PaperIds, string SourceOfIds, int MagRelatedRunId)
        {
            _PaperIds = PaperIds;
            _SourceOfIds = SourceOfIds;
            _MagRelatedRunId = MagRelatedRunId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_PaperIds", _PaperIds);
            info.AddValue("_NImported", _NImported);
            info.AddValue("_SourceOfIds", _SourceOfIds);
            info.AddValue("_MagRelatedRunId", _MagRelatedRunId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _PaperIds = info.GetValue<string>("_PaperIds");
            _NImported = info.GetValue<int>("_NImported");
            _SourceOfIds = info.GetValue<string>("_SourceOfIds");
            _MagRelatedRunId = info.GetValue<int>("_MagRelatedRunId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                if (_SourceOfIds == "RelatedPapersSearch") // if not, then the list of ids has been sent from the browser
                {
                    
                    using (SqlCommand command = new SqlCommand("st_MagItemMagRelatedPaperInsert", connection))
                    {
                        command.CommandTimeout = 2000; // 2000 secs = about 2 hours?
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", _MagRelatedRunId));
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                _PaperIds += _PaperIds == "" ? reader["PaperId"].ToString() : "," + reader["PaperId"].ToString();
                            }
                        }
                    }
                }

                Guid GuidJob = Guid.NewGuid();
                TextInfo myTI = new CultureInfo("en-GB", false).TextInfo;

                DataTable dtItems = new DataTable("Items");
                dtItems.Columns.Add("id", typeof(Int64));
                dtItems.Columns.Add("year", typeof(string));
                dtItems.Columns.Add("journal", typeof(string));
                dtItems.Columns.Add("volume", typeof(string));
                dtItems.Columns.Add("issue", typeof(string));
                dtItems.Columns.Add("pages", typeof(string));
                dtItems.Columns.Add("title", typeof(string));
                dtItems.Columns.Add("doi", typeof(string));
                dtItems.Columns.Add("SearchText", typeof(string));
                dtItems.Columns.Add("GUID_JOB");

                DataTable dtAuthors = new DataTable("Authors");
                dtAuthors.Columns.Add("id", typeof(Int64));
                dtAuthors.Columns.Add("GUID_JOB");
                dtAuthors.Columns.Add("first", typeof(string));
                dtAuthors.Columns.Add("second", typeof(string));
                dtAuthors.Columns.Add("last", typeof(string));
                dtAuthors.Columns.Add("rank", typeof(int));

                foreach (string PaperId in _PaperIds.Split(','))
                {
                    MagPaper mp = MagPaper.GetMagPaperFromMakes(Convert.ToInt64(PaperId), null);
                    if (mp.PaperId > 0)
                    {
                        string[] authors = mp.Authors.Split(',');
                        for (int x = 0; x < authors.Count(); x++)
                        {
                            AutH author = NormaliseAuth.singleAuth(authors[x], 0, 0);
                            if (author != null)
                            {
                                DataRow newAuthor = dtAuthors.NewRow();
                                newAuthor["id"] = mp.PaperId;
                                newAuthor["GUID_JOB"] = GuidJob.ToString();
                                newAuthor["first"] = author.LastName; // looks odd, but order is reversed in a MAG list
                                newAuthor["second"] = author.MiddleName == "" ? author.MiddleName : author.FirstName;
                                newAuthor["last"] = author.MiddleName != "" ? author.MiddleName : author.FirstName;
                                newAuthor["rank"] = x + 1;
                                dtAuthors.Rows.Add(newAuthor);
                            }
                        }

                        DataRow dr = dtItems.NewRow();
                        dr["id"] = mp.PaperId;
                        dr["year"] = mp.Year;
                        dr["journal"] = mp.Journal != null ? myTI.ToTitleCase(mp.Journal) : "";
                        dr["volume"] = mp.Volume;
                        dr["issue"] = mp.Issue;
                        dr["pages"] = mp.FirstPage + "-" + mp.LastPage;
                        dr["title"] = mp.OriginalTitle;
                        dr["doi"] = mp.DOI;
                        dr["GUID_JOB"] = GuidJob.ToString();
                        dr["SearchText"] = Item.ToShortSearchText(mp.PaperTitle);
                        dtItems.Rows.Add(dr);
                    }
                }

                using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                {
                    sbc.DestinationTableName = "TB_MAG_ITEM_PAPER_INSERT_TEMP";
                    sbc.BatchSize = 1000;
                    sbc.WriteToServer(dtItems);
                }
                using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                {
                    sbc.DestinationTableName = "TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP";
                    sbc.BatchSize = 1000;
                    sbc.WriteToServer(dtAuthors);
                }
                using (SqlCommand command = new SqlCommand("st_MagItemPaperInsert", connection))
                {
                    command.CommandTimeout = 500; // should make this a nice long time
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@GUID_JOB", GuidJob));
                    string SourceText = "";
                    if (_SourceOfIds == "RelatedPapersSearch")
                    {
                        SourceText = "Automated search: " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToLongTimeString();
                    }
                    else
                    {
                        SourceText = "Selected items from MAG on " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToLongTimeString();
                    }
                    command.Parameters.Add(new SqlParameter("@SOURCE_NAME", SourceText));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", _MagRelatedRunId));
                    command.Parameters.Add(new SqlParameter("@N_IMPORTED", ri.UserId));
                    command.Parameters["@N_IMPORTED"].Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    _NImported = Convert.ToInt32(command.Parameters["@N_IMPORTED"].Value);
                }
                connection.Close();
            
            }
        }

#endif


    }
}
