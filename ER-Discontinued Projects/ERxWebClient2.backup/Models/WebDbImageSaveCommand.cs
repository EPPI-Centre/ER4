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

        private short _imageNumber;
        //private string _documentExtension;
        private int _webDbId;
        private byte[] _docbin;
        private string _extension;



        public short imageNumber
        {
            get { return _imageNumber; }
        }

        public int webDbId
        {
            get { return _webDbId; }
        }
        public string extension
        {
            get { return _extension; }
        }

        public byte[] docbin
        {
            get { return _docbin; }
        }

        public WebDbImageSaveCommand(int WebDbId, short ImageNumber, string extension, byte[] docbin)
        {
            _webDbId = WebDbId;
            _imageNumber = ImageNumber;
            _extension = extension;
            _docbin = docbin;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_extension", _extension);
            info.AddValue("_imageNumber", _imageNumber);
            info.AddValue("_docbin", _docbin);
            info.AddValue("_webDbId", _webDbId);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _extension = info.GetValue<string>("_extension");
            _imageNumber = info.GetValue<short>("_imageNumber");
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
                    command.Parameters.Add(new SqlParameter("@imageNumber", _imageNumber));
                    command.Parameters.Add(new SqlParameter("@Extension", _extension));
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
