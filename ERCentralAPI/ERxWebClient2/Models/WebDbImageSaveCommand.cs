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
using EPPIiFilter;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class WebDbImageSaveCommand : CommandBase<WebDbImageSaveCommand>
    {
        public WebDbImageSaveCommand(){}

        private bool _isImage1;
        //private string _documentExtension;
        private int _webDbId;
        private byte[] _docbin;
        private string _fileName;



        public bool isImage1
        {
            get { return _isImage1; }
        }

        public int webDbId
        {
            get { return _webDbId; }
        }
        public string fileName
        {
            get { return _fileName; }
        }

        public byte[] docbin
        {
            get { return _docbin; }
        }

        public WebDbImageSaveCommand(int WebDbId, bool IsImage1, string filename, byte[] docbin)
        {
            _webDbId = WebDbId;
            _isImage1 = IsImage1;
            _fileName = filename;
            _docbin = docbin;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_fileName", _fileName);
            info.AddValue("_isImage1", _isImage1);
            info.AddValue("_docbin", _docbin);
            info.AddValue("_webDbId", _webDbId);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _fileName = info.GetValue<string>("_fileName");
            _isImage1 = info.GetValue<bool>("_isImage1");
            _docbin = info.GetValue<byte[]>("_docbin");
            _webDbId = info.GetValue<int>("_webDbId");
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbUploadImage", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WebDbId", _webDbId));
                    command.Parameters.Add(new SqlParameter("@BIN", _docbin));
                    command.Parameters.Add(new SqlParameter("@IsImage1", _isImage1));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
        public WebDbImageSaveCommand doItNow()
        {
            DataPortal_Execute();
            return this;
        }

#endif
    }

}
