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
#if CSLA_NETCORE
using Microsoft.Extensions.Configuration;
#endif

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.IO;
using BusinessLibrary.BusinessClasses;
#endif

namespace BusinessLibrary.Security
{
    [Serializable]
    public class CreateER4ContactViaArchieCommand : CommandBase<CreateER4ContactViaArchieCommand>
    {
        //this objects checks if all details are OK on server side (you can't trust the client)
        //then creates an ER4 CONTACT record accordingly
        //Result will hold a message as to whether things worked
        //as this happens before logging on a review, if all is well, user will be properly logged on when they reach 

    public CreateER4ContactViaArchieCommand(){}

        string _code, _status, _username, _email, _fullname, _password;
        bool _sendNewsletter, _createExampleReview;
        string _result;

        public string Result
        {
            get { return _result; }
        }

        public CreateER4ContactViaArchieCommand(string code, string status, string username, string email, string fullname, string password, bool sendNewsletter, bool createExampleReview)
        {
            _code = code;
            _status = status;
            _username = username;
            _email = email;
            _fullname = fullname;
            _password = password;
            _sendNewsletter = sendNewsletter;
            _createExampleReview = createExampleReview;
        }
        
        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_code", _code);
            info.AddValue("_status", _status);
            info.AddValue("_username", _username);
            info.AddValue("_email", _email);
            info.AddValue("_fullname", _fullname);
            info.AddValue("_password", _password);
            info.AddValue("_sendNewsletter", _sendNewsletter);
            info.AddValue("_createExampleReview", _createExampleReview);
            info.AddValue("_result", _result);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _code = info.GetValue<string>("_code");
            _status = info.GetValue<string>("_status");
            _username = info.GetValue<string>("_username");
            _email = info.GetValue<string>("_email");
            _fullname = info.GetValue<string>("_fullname");
            _password = info.GetValue<string>("_password");
            _sendNewsletter = info.GetValue<bool>("_sendNewsletter");
            _createExampleReview = info.GetValue<bool>("_createExampleReview");
            _result = info.GetValue<string>("_result");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            //1. check the data has the right shape
            if (_fullname == null || _fullname.Trim().Length < 2)
            {
                _result = "Name is missing or too short";
                return;
            }
            if (_username == null || _username.Trim().Length < 4)
            {
                _result = "Username is missing or too short";
                return;
            }
            if (_email.Trim().Length < 1 //too short
                ||
                ((_email.Trim().IndexOf("@") < 2) ||
                    (_email.Trim().IndexOf("@") >= _email.Trim().Length - 1)
                )//@ symbol absent or too close to the string edges
                ||
                ((_email.Trim().LastIndexOf(".") < 2) ||
                    (_email.Trim().LastIndexOf(".") >= _email.Trim().Length - 1)
                )//. symbol absent or too close to the string edges
                )
            {
                _result = "Email is missing or invalid";
                return;
            }
            
            Regex passwordRegex = new Regex("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$");
            Match m = passwordRegex.Match(_password.Trim());
            if (!m.Success || _password.Trim().Length < 8 || _password.Trim() != _password.Trim())
            {
                _result = "Password is missing or invalid";
                return;
            }
            int CID;
            //2. check user details (uname and email) are not in use
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ContactTableByName", connection))
                {//st_ContactLogin needs to be changed so to wipe ArchieCode and Status
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@USERNAME", _username));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            _result = "Username is already in use.";
                            return;
                        }
                    }
                }
                using (SqlCommand command = new SqlCommand("st_ContactTableByEmail", connection))
                {//st_ContactLogin needs to be changed so to wipe ArchieCode and Status

                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@EMAIL", _email));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            _result = "Email is already in use. Please select another or link to the (already) existing account.";
                            return;
                        }
                    }
                }
                //3. create standard account
            //if we've got this far, it's time to create the user account, we are provisionally assuming the user is logged on via Archie, will check and undo if necessary later one
                using (SqlCommand command = new SqlCommand("st_ContactCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_NAME", _fullname));
                    command.Parameters.Add(new SqlParameter("@USERNAME", _username));
                    command.Parameters.Add(new SqlParameter("@PASSWORD", _password));
                    command.Parameters.Add(new SqlParameter("@DATE_CREATED", DateTime.Now));
                    command.Parameters.Add(new SqlParameter("@EMAIL", _email));
                    command.Parameters.Add(new SqlParameter("@DESCRIPTION", "Cochrane User: account created after logging on through Archie"));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", System.Data.SqlDbType.Int));
                    command.Parameters["@CONTACT_ID"].Direction = System.Data.ParameterDirection.Output;

                    command.ExecuteNonQuery();
                    int.TryParse(command.Parameters["@CONTACT_ID"].Value.ToString(), out CID);
                    if (CID < 1)
                    {
                        _result = "Failed to create account, please contact EPPISupport@ioe.ac.uk";
                        return;
                    }
                }
                //4. link to archie Identity

                ArchieIdentity ArId = ArchieIdentity.GetUnidentifiedArchieIdentity(_code, _status, CID);
                
                if (ArId.IsAuthenticated)
                {//all good user is linked and data was saved
                    _result = "Done";
                }
                else
                {//if user did not pass this check, we need to remove the newly created account: we shouldn't trust this request!
                    _result = "Account not created: failed to link to the Archie Identity";
                    DeleteAbortedAccount(CID);
                    return;
                }
                //5. send activate email
                //create linkcheck record
                //string host = Environment.MachineName.ToLower();
                string BaseUrl = AzureSettings.AccountManagerURL;
                //if (host == "epi3b" || host == "epi3" || host == "eppi.ioe.ac.uk" || host == "epi2.ioe.ac.uk")
                //{//use live address: this is the real published ER4
                //    BaseUrl = "https://eppi.ioe.ac.uk/ER4Manager/";
                //}
                //else if (host == "bk-epi" | host == "bk-epi.ioead" | host == "bk-epi.inst.ioe.ac.uk")
                //{//this is our testing environment, the first tests should be against the test archie, otherwise the real one
                //    //changes are to be made here depending on what test we're doing
                //    BaseUrl = "http://bk-epi/testing/ER4Manager/";
                //}
                //else
                //{//not a live publish, use test archie
                //    //this won't work if used on a machine that isn't mine!!!!
                //    //!!!needs to be changed for ER4.
                //    BaseUrl = "http://localhost/ER4Manager/";
                //}
                string LinkUI = "";
                using (SqlCommand command = new SqlCommand("st_CheckLinkCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@TYPE", "CheckEmail"));
                    command.Parameters.Add(new SqlParameter("@CID", CID));
                    command.Parameters.Add(new SqlParameter("@CC_EMAIL", null ));
                    command.Parameters.Add(new SqlParameter("@UID", System.Data.SqlDbType.UniqueIdentifier));
                    command.Parameters["@UID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LinkUI = command.Parameters["@UID"].Value.ToString();
                }
                string sending = "OK";
                if (LinkUI != null && LinkUI != "")
                {//the thing worked, send the actual email and notify us
                    sending = VerifyAccountEmail(_email, _fullname, _username , LinkUI, CID.ToString(), BaseUrl, "");
                    if (sending == "OK")
                    {
                        string adminMsg = "ACCOUNT CREATED DETAILS:<BR> CONTACT_ID = " + CID.ToString()
                                            + "<BR> USERNAME = " + _username
                                            + "<BR> EMAIL = " + _email
                                            + "<BR> DESCRIPTION = Cochrane User: account created after logging on through Archie";
                        VerifyAccountEmail("EPPIsupport@ioe.ac.uk", _fullname, _username, LinkUI, CID.ToString(), BaseUrl, adminMsg);
                    }
                }
                else if ((LinkUI == null || LinkUI == "") || sending != "OK")
                {//didn't work, notify us and the user via the UI
                    VerifyAccountEmail("EPPIsupport@ioe.ac.uk", _fullname, _username, "", CID.ToString(), BaseUrl, "NOT SENT! <BR> Account was created but could not create link <BR>");
                    _result = "Account not created: failed to generate the activation link";
                    //delete account record
                    DeleteAbortedAccount(CID);
                    return;
                }
            }
            //6.[not implemented] if necessary copy the example review

        }
        private void DeleteAbortedAccount(int CID)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ContactUndoCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CID", CID));
                    command.Parameters.Add(new SqlParameter("@USERNAME", _username));
                    command.ExecuteNonQuery();
                }
            }
        }
        public string VerifyAccountEmail(string mailTo, string newUser, string userName, string LinkUI, string CID, string BaseUrl, string stAdditional)
        {
            string emailID = "7"; // this is based on the values in the database
            
            MailMessage msg = new MailMessage();
            msg.To.Add(mailTo);
            //if (mailTo != mailFrom)
            //{
            //    msg.Bcc.Add(mailFrom);
            //}
            

            msg.Subject = "EPPI Reviewer: Account Activation";
            msg.IsBodyHtml = true;


            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_EmailGet", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@EMAIL_ID", emailID));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            msg.Body = reader["EMAIL_MESSAGE"].ToString();
                        }
                    }
                }
            }
            msg.Body = msg.Body.Replace("FullNameHere", newUser);
            string queStr = BaseUrl + "LinkCheck.aspx?LUID=" + LinkUI + "&CID=" + CID;
            msg.Body = msg.Body.Replace("linkURLhere", queStr);
            if (stAdditional != "")
            {
                msg.Body = msg.Body.Replace("AdminMessageHere", stAdditional);
            }
            else
            {
                msg.Body = msg.Body.Replace("AdminMessageHere", "");
            }

            string SMTP = AzureSettings.SMTP;
            string SMTPUser = AzureSettings.SMTPUser;
            string fromCred = AzureSettings.SMTPAuthentic;
            string mailFrom = AzureSettings.mailFrom;

            msg.From = new MailAddress(mailFrom);
            SmtpClient smtp = new SmtpClient(SMTP);

            System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential(SMTPUser, fromCred);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = SMTPUserInfo;
            smtp.EnableSsl = true; smtp.Port = 587;
            try
            {
                smtp.Send(msg);
                return "OK";
            }
            catch (Exception ex)
            {
                return "Could not send verification email";// +ex.ToString();
            }
        }
#endif
        }
}

