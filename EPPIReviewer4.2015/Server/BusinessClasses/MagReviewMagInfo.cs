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
    public class MAgReviewMagInfoCommand : CommandBase<MAgReviewMagInfoCommand>
    {

#if SILVERLIGHT
    public MAgReviewMagInfoCommand(){}
#else
        public MAgReviewMagInfoCommand() { }
#endif

        private int _ReviewId;
        private int _NInReviewIncluded;
        private int _NInReviewExcluded;
        private int _NMatchedAccuratelyIncluded;
        private int _NMatchedAccuratelyExcluded;
        private int _NRequiringManualCheckIncluded;
        private int _NRequiringManualCheckExcluded;
        private int _NNotMatchedIncluded;
        private int _NNotMatchedExcluded;

        public int ReviewId
        {
            get { return _ReviewId; }
        }
        public int NInReviewIncluded
        {
            get { return _NInReviewIncluded; }
        }
        public int NInReviewExcluded
        {
            get { return _NInReviewExcluded; }
        }
        public int NMatchedAccuratelyIncluded
        {
            get { return _NMatchedAccuratelyIncluded; }
        }
        public int NMatchedAccuratelyExcluded
        {
            get { return _NMatchedAccuratelyExcluded; }
        }
        public int NRequiringManualCheckIncluded
        {
            get { return _NRequiringManualCheckIncluded; }
        }
        public int NRequiringManualCheckExcluded
        {
            get { return _NRequiringManualCheckExcluded; }
        }

        public int NNotMatchedIncluded
        {
            get { return _NNotMatchedIncluded; }
        }
        public int NNotMatchedExcluded
        {
            get { return _NNotMatchedExcluded; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_ReviewId", _ReviewId);
            info.AddValue("_NInReviewIncluded", _NInReviewIncluded);
            info.AddValue("_NInReviewExcluded", _NInReviewExcluded);
            info.AddValue("_NMatchedAccuratelyIncluded", _NMatchedAccuratelyIncluded);
            info.AddValue("_NMatchedAccuratelyExcluded", _NMatchedAccuratelyExcluded);
            info.AddValue("_NRequiringManualCheckIncluded", _NRequiringManualCheckIncluded);
            info.AddValue("_NRequiringManualCheckExcluded", _NRequiringManualCheckExcluded);
            info.AddValue("_NNotMatchedIncluded", _NNotMatchedIncluded);
            info.AddValue("_NNotMatchedExcluded", _NNotMatchedExcluded);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _ReviewId = info.GetValue<int>("_ReviewId");
            _NInReviewIncluded = info.GetValue<int>("_NInReviewIncluded");
            _NInReviewExcluded = info.GetValue<int>("_NInReviewExcluded");
            _NMatchedAccuratelyIncluded = info.GetValue<int>("_NMatchedAccuratelyIncluded");
            _NMatchedAccuratelyExcluded = info.GetValue<int>("_NMatchedAccuratelyExcluded");
            _NRequiringManualCheckIncluded = info.GetValue<int>("_NRequiringManualCheckIncluded");
            _NRequiringManualCheckExcluded = info.GetValue<int>("_NRequiringManualCheckExcluded");
            _NNotMatchedIncluded = info.GetValue<int>("_NNotMatchedIncluded");
            _NNotMatchedExcluded = info.GetValue<int>("_NNotMatchedExcluded");
        }

       

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_MagReviewMagInfo", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@NInReviewIncluded", 0));
                    command.Parameters["@NInReviewIncluded"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NInReviewExcluded", 0));
                    command.Parameters["@NInReviewExcluded"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NMatchedAccuratelyIncluded", 0));
                    command.Parameters["@NMatchedAccuratelyIncluded"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NMatchedAccuratelyExcluded", 0));
                    command.Parameters["@NMatchedAccuratelyExcluded"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NRequiringManualCheckIncluded", 0));
                    command.Parameters["@NRequiringManualCheckIncluded"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NRequiringManualCheckExcluded", 0));
                    command.Parameters["@NRequiringManualCheckExcluded"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NNotMatchedIncluded", 0));
                    command.Parameters["@NNotMatchedIncluded"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NNotMatchedExcluded", 0));
                    command.Parameters["@NNotMatchedExcluded"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    _NInReviewIncluded = Convert.ToInt32(command.Parameters["@NInReviewIncluded"].Value);
                    _NInReviewExcluded = Convert.ToInt32(command.Parameters["@NInReviewExcluded"].Value);
                    _NMatchedAccuratelyIncluded = Convert.ToInt32(command.Parameters["@NMatchedAccuratelyIncluded"].Value);
                    _NMatchedAccuratelyExcluded = Convert.ToInt32(command.Parameters["@NMatchedAccuratelyExcluded"].Value);
                    _NRequiringManualCheckIncluded = Convert.ToInt32(command.Parameters["@NRequiringManualCheckIncluded"].Value);
                    _NRequiringManualCheckExcluded = Convert.ToInt32(command.Parameters["@NRequiringManualCheckExcluded"].Value);
                    _NNotMatchedIncluded = Convert.ToInt32(command.Parameters["@NNotMatchedIncluded"].Value);
                    _NNotMatchedExcluded = Convert.ToInt32(command.Parameters["@NNotMatchedExcluded"].Value);
                }
                connection.Close();
            }
        }

      

#endif


    }
}
