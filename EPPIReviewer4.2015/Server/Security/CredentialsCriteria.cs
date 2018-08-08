using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Csla;
using Csla.Core;
using Csla.Serialization;

namespace BusinessLibrary.Security
{
    [Serializable()]
    public class CredentialsCriteria : CriteriaBase<CredentialsCriteria>
    {

        public CredentialsCriteria() { }

        private string _username;
        private string _password;
        private int _reviewId;
        private string _LoginMode;
        private string _ArchieState;
        private string _ArchieCode;
        
        public string Username
        {
            get
            {
                return _username;
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
        }

        public int ReviewId
        {
            get
            {
                return _reviewId;
            }
        }
        public string LoginMode
        {
            get
            {
                return _LoginMode;
            }
        }
        public string ArchieState
        {
            get
            {
                return _ArchieState;
            }
        }
        public string ArchieCode
        {
            get
            {
                return _ArchieCode;
            }
        }
        
#if (CSLA_NETCORE)
        private int _ContactId;
        public int ContactId
        {
            get
            {
                return _ContactId;
            }
        }
        private string _DisplayName;
        public string DisplayName
        {
            get
            {
                return _DisplayName;
            }
        }
        private ClaimsPrincipal _ClaimsP;
        public ClaimsPrincipal ClaimsP
        {
            get { return _ClaimsP; }
        }
#endif
        public CredentialsCriteria(string username, string password, int reviewid)
        //: base(typeof(CredentialsCriteria))
        {
            _username = username;
            _password = password;
            _reviewId = reviewid;
            _LoginMode = "Standard";
        }
        public CredentialsCriteria(string username, string password, int reviewid, string loginMode)
        //: base(typeof(CredentialsCriteria))
        {
            _username = username;
            _password = password;
            _reviewId = reviewid;
            _LoginMode = loginMode;
        }
#if (CSLA_NETCORE)
        public CredentialsCriteria(int contactID, int reviewid, string displayName, string loginMode)
        {
            _DisplayName = displayName;
            _ContactId = contactID;
            _reviewId = reviewid;
            _LoginMode = loginMode;
        }
        public CredentialsCriteria(ClaimsPrincipal CP)
        {//with this, we'll create a full RI based on the user logged on via its JWToken, without using the DB.
            //should be fast as this is done every time a controller will use a CSLA object that needs the ReviewerIdentity
            _ClaimsP = CP;
            _LoginMode = "MVC";
        }
#endif
        public CredentialsCriteria(string ArchieCode, string Status, string loginMode, int reviewid)
        {
            _password = "";
            _ArchieCode = ArchieCode;
            _ArchieState = Status;
            _reviewId = reviewid;
            _LoginMode = loginMode;
        }
        public CredentialsCriteria(string username, string password, string ArchieCode, string Status, string loginMode, int reviewid)
        {
            _username = username;
            _password = password;
            _ArchieCode = ArchieCode;
            _ArchieState = Status;
            _reviewId = reviewid;
            _LoginMode = loginMode;
        }
        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            info.AddValue("_username", _username);
            info.AddValue("_password", _password);
            info.AddValue("_reviewId", _reviewId);
            info.AddValue("_LoginMode", _LoginMode);
            info.AddValue("_ArchieState", _ArchieState);
            info.AddValue("_ArchieCode", _ArchieCode);
            base.OnGetState(info, mode);
        }

        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            _username = (string)info.Values["_username"].Value;
            _password = (string)info.Values["_password"].Value;
            _reviewId = (int)info.Values["_reviewId"].Value;
            _LoginMode = (string)info.Values["_LoginMode"].Value;
            _ArchieState = (string)info.Values["_ArchieState"].Value;
            _ArchieCode = (string)info.Values["_ArchieCode"].Value;
            base.OnSetState(info, mode);
        }
    }
}
