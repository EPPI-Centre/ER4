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
using System.Configuration;
using System.Collections.Concurrent;
using Csla.Data;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Data;
#if !CSLA_NETCORE
using System.Web.Hosting;
#endif
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagNewPapersUpdateAbstractsCommand : CommandBase<MagNewPapersUpdateAbstractsCommand>
    {

#if SILVERLIGHT
    public MagNewPapersUpdateAbstractsCommand(){}
#else
        public MagNewPapersUpdateAbstractsCommand() { }
#endif


        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {

        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int contactId = ri.UserId;

#if CSLA_NETCORE
            System.Threading.Tasks.Task.Run(() => doMatchItems(contactId);
#else
            //see: https://codingcanvas.com/using-hostingenvironment-queuebackgroundworkitem-to-run-background-tasks-in-asp-net/
            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => doMatchItems(contactId));
#endif
               
        }

        class ItemPapers
        {
            public Int64 ITEM_ID;
            public Int64 PaperId;
        }

        private async Task<bool> doMatchItems(int contactId, CancellationToken cancellationToken = default(CancellationToken))
        {
            int MagLogId = MagLog.SaveLogEntry("Updating abstracts", "Running", "Getting currently used ids without abstracts", contactId);


            // 1. Get the list of currently used ids without abstracts and save to ItemPapers

            List<ItemPapers> ItemList = new List<ItemPapers>();
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_MagGetItemsWithMissingAbstracts", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        using (SafeDataReader reader = new SafeDataReader(command.ExecuteReader()))
                        {
                            if (reader.Read())
                            {
                                while (reader.Read())
                                {
                                    ItemList.Add(new ItemPapers
                                    {
                                        ITEM_ID = Convert.ToInt64(reader["ITEM_ID"].ToString()),
                                        PaperId = Convert.ToInt64(reader["PaperId"].ToString())
                                    });
                                }
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch
            {
                return false;
            }

            // 2. Look 'em up in MAKES
            MagLog.UpdateLogEntry("Running", "Local missing: " + ItemList.Count.ToString() + ". Now looking up in MAKES", MagLogId);

            int updated = 0;
            using (SqlConnection connection2 = new SqlConnection(DataConnection.ConnectionString))
            {
                connection2.Open();
                foreach (ItemPapers ip in ItemList)
                {
                    MagMakesHelpers.OaPaper pm = MagMakesHelpers.GetPaperMakesFromMakes(ip.PaperId);
                    if (pm != null && pm.abstract_inverted_index != null)
                    {
                        string ab = MagMakesHelpers.ReconstructInvertedAbstract(pm.abstract_inverted_index);
                        if (ab != "")
                        {
                            using (SqlCommand command2 = new SqlCommand("st_MagUpdateMissingAbstract", connection2))
                            {
                                command2.CommandType = CommandType.StoredProcedure;
                                command2.Parameters.Add(new SqlParameter("@ITEM_ID", ip.ITEM_ID));
                                command2.Parameters.Add(new SqlParameter("@ABSTRACT", ab));
                                command2.ExecuteNonQuery();
                            }
                            updated++;
                            MagLog.UpdateLogEntry("Running", "Local missing: " + ItemList.Count.ToString() + " so far updated: " + updated.ToString(), MagLogId);
                        }
                    }
                }
                connection2.Close();
            }
            
            
            // 3. finished
            MagLog.UpdateLogEntry("Complete", "Local missing: " + ItemList.Count.ToString() + "; updated: " + updated.ToString(), MagLogId);

            return true;
        }


#endif


    }
}
