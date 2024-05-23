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

#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class RobotOpenAiTaskReadOnlyList : ReadOnlyListBase<RobotOpenAiTaskReadOnlyList, RobotOpenAiTaskReadOnly>
    {
        public RobotOpenAiTaskReadOnlyList() 
        {
        }
        

#if !SILVERLIGHT

        private void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                //using (SqlCommand command = new SqlCommand("st_SourceFromReview_ID", connection))
                //{
                    
                //    command.CommandType = System.Data.CommandType.StoredProcedure;
                //    command.Parameters.Add(new SqlParameter("@revID", ri.ReviewId));
                //    //command.CommandTimeout = 100;
                //    //Sources = new MobileList<string>();
                //    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                //    {
                //        while (reader.Read())
                //        {
                //            Sources.Add(ReadOnlySource.GetReadOnlySource(reader));
                //        }
                //        reader.NextResult();
                //        if (reader.Read())
                //        {//second reader gives us the source ID of a "currently being deleted" source (if any)
                //            int DeletingThisSouceId = reader.GetInt32("SOURCE_ID");
                //            int index = Sources.FindIndex(f => f.Source_ID == DeletingThisSouceId);
                //            if (index > -1)
                //            {
                //                Sources[index].MarkAsBeingDeleted();
                //                LoadProperty(SomeSourceIsBeingDeletedProperty, true);

                //                //supplying 0 as the SourceId makes the command "check" is a source deletion needs to be resumed.
                //                SourceDeleteForeverCommand sdfc = new SourceDeleteForeverCommand(0);
                //                DataPortal<SourceDeleteForeverCommand> dp2 = new DataPortal<SourceDeleteForeverCommand>();
                //                sdfc = dp2.Execute(sdfc);//fire and forget, this will check and possibly resume deletion, but doesn't wait for the deletion to end.
                //            } 
                //            else
                //            {
                //                LoadProperty(SomeSourceIsBeingDeletedProperty, false);
                //            }
                //        }
                //        //LoadProperty(Sourceless_ItemsProperty, reader.GetInt32("Total_Items"));
                //    }
                //}
                //connection.Close();
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }

#endif
    }
}
