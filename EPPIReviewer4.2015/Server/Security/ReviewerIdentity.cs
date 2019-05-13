using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.DataPortalClient;
using System.ComponentModel;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.Security
{
    [Serializable()]
    public class ReviewerIdentity : CslaIdentity
    {
        public static readonly PropertyInfo<int> UserIdProperty = RegisterProperty<int>(typeof(ReviewerIdentity), new PropertyInfo<int>("UserId", "User Id", 0));
        public int UserId
        {
            get
            {
                return GetProperty<int>(UserIdProperty);
            }
        }
        public static readonly PropertyInfo<bool> IsSiteAdminProperty = RegisterProperty<bool>(typeof(ReviewerIdentity), new PropertyInfo<bool>("IsSiteAdmin", "IsSiteAdmin", false));
        public bool IsSiteAdmin
        {
            get
            {
                return GetProperty<bool>(IsSiteAdminProperty);
            }
        }
        public static readonly PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(typeof(ReviewerIdentity), new PropertyInfo<int>("ReviewId", "Review Id", 0));
        public int ReviewId
        {
#if SILVERLIGHT
            get
            {
                return GetProperty<int>(ReviewIdProperty);
            }
#else

            get
            {
                if (Ticket == "")
                {
                    if (GetProperty<int>(ReviewIdProperty) == 0)
                    {//all is well: user doens't have a ticket, but is not logged on a review
                        return 0;
                    }
                    else
                    {
                        throw new Exception("Logon ERROR: current logon ticket is not valid.");
                    }
                   
                }
                //consider using try-catch!
                using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_LogonTicket_Check_RI", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@guid", new System.Data.SqlTypes.SqlGuid(GetProperty<string>(TicketProperty))));
                        command.Parameters.Add(new SqlParameter("@c_ID", GetProperty<int>(UserIdProperty)));
                        command.Parameters.Add("@ROWS", System.Data.SqlDbType.Int);
                        command.Parameters["@ROWS"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        //the following are options for the future: at one point we might want to kick out people at this stage,
                        //for now, we are happy to let this happen only through the keepalive system.
                        int rows = (int)command.Parameters["@ROWS"].Value;
                        if (rows == 1) return GetProperty<int>(ReviewIdProperty);
                        //else return 0;
                        else throw new Exception("Logon ERROR: current logon ticket is not valid.");
                        //return GetProperty<int>(ReviewIdProperty);
                    }
                }
                
            }
#endif

        }
        public static readonly PropertyInfo<string> TicketProperty = RegisterProperty<string>(typeof(ReviewerIdentity), new PropertyInfo<string>("Ticket", "Ticket", 0));
        public string Ticket
        {
            get
            {
                return GetProperty<string>(TicketProperty);
            }
        }
        public static readonly PropertyInfo<DateTime> AccountExpirationProperty = RegisterProperty<DateTime>(typeof(ReviewerIdentity), new PropertyInfo<DateTime>("AccountExpiration", "AccountExpiration"));
        public DateTime AccountExpiration
        {
            get
            {
                return GetProperty<DateTime>(AccountExpirationProperty);
            }
        }
        public static readonly PropertyInfo<DateTime> ReviewExpirationProperty = RegisterProperty<DateTime>(typeof(ReviewerIdentity), new PropertyInfo<DateTime>("ReviewExpiration", "ReviewExpiration"));
        public DateTime ReviewExpiration
        {
            get
            {
                return GetProperty<DateTime>(ReviewExpirationProperty);
            }
        }
        public static readonly PropertyInfo<string> LoginModeProperty = RegisterProperty<string>(typeof(ReviewerIdentity), new PropertyInfo<string>("LoginMode", "LoginMode", 0));
        public string LoginMode
        {
            get
            {
                return GetProperty<string>(LoginModeProperty);
            }
        }

        
        private bool IsInRole(string role)
        {
            return Roles.Contains(role);
        }

        public bool HasWriteRights()
        {
            if (this.Roles == null || Roles.Count == 0) return false;
            return (!IsInRole("ReadOnlyUser"));
        }

        public bool IsCochraneUser
        {
            get
            {
                return (IsInRole("CochraneUser"));
            }
        }
        public int DaysLeftAccount
        {
            get
            {
                return AccountExpiration.Subtract(DateTime.Today).Days;
            }
        }
        public int DaysLeftReview
        {
            get
            {
                return ReviewExpiration.Subtract(DateTime.Today).Days;
            }
        }

#if SILVERLIGHT

    public ReviewerIdentity() {}

    public static void GetIdentity(string username, string password, int reviewId, string LoginMode, EventHandler<DataPortalResult<ReviewerIdentity>> completed)
    {
      GetCslaIdentity<ReviewerIdentity>(completed, new CredentialsCriteria(username, password, reviewId, LoginMode));
    }
    public static void GetIdentity(string ArchieCode, string Status, string LoginMode, int reviewId, EventHandler<DataPortalResult<ReviewerIdentity>> completed)
    {
        GetCslaIdentity<ReviewerIdentity>(completed, new CredentialsCriteria(ArchieCode, Status, LoginMode, reviewId));
    }
    public static void GetIdentity(string username, string password, string ArchieCode, string Status, string LoginMode, int reviewId, EventHandler<DataPortalResult<ReviewerIdentity>> completed)
    {
        GetCslaIdentity<ReviewerIdentity>(completed, new CredentialsCriteria(username, password, ArchieCode, Status, LoginMode, reviewId));
    }
#else

        public static ReviewerIdentity GetIdentity(string username, string password, int reviewId, string LoginMode, string roles)
        {
            return GetCslaIdentity<ReviewerIdentity>(new CredentialsCriteria(username, password, reviewId, LoginMode));
        }
#if (CSLA_NETCORE)
        public static ReviewerIdentity GetIdentity(string ArchieCode, string Status, string LoginMode, int reviewId)
        {
            return GetCslaIdentity<ReviewerIdentity>(new CredentialsCriteria(ArchieCode, Status, LoginMode, reviewId));
        }

        public static ReviewerIdentity GetIdentity(int contactId, int reviewId, string displayName, bool isArchie)
        {
            return GetCslaIdentity<ReviewerIdentity>(new CredentialsCriteria(contactId, reviewId, displayName, isArchie? "ERWebArchie" : "ERWeb"));
        }
        public static ReviewerIdentity GetIdentity(System.Security.Claims.ClaimsPrincipal CP)
        {
            return GetCslaIdentity<ReviewerIdentity>(new CredentialsCriteria(CP));
        }
        public static readonly PropertyInfo<string> TokenProperty = RegisterProperty<string>(typeof(ReviewerIdentity), new PropertyInfo<string>("Token", "Token", 0));
        public string Token
        {
            get
            {
                return GetProperty<string>(TokenProperty);
            }
            set
            {
                LoadProperty(TokenProperty, value);
            }
        }
#endif
        DateTime ContactExp = new DateTime(1, 1, 1);
        DateTime ReviewExp = new DateTime(1, 1, 1);
        private void DataPortal_Fetch(CredentialsCriteria criteria)
        {
            LoadProperty(LoginModeProperty, criteria.LoginMode);
            if (criteria.Password != null && criteria.Password != "" && criteria.Username != null && criteria.Username != "" && (criteria.ArchieCode == null || criteria.ArchieCode ==""))
            {//standard ER4 authentication
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ContactLogin", connection))
                    {//st_ContactLogin needs to be changed so to wipe ArchieCode and Status
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@userName", criteria.Username));
                        command.Parameters.Add(new SqlParameter("@Password", criteria.Password));

                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            if (reader.Read())
                            {
                                IsAuthenticated = true;//we set it here, but might reverse if something down the line fails
                                LoadProperty<int>(UserIdProperty, reader.GetInt32("CONTACT_ID"));
                                Name = reader.GetString("contact_name");
                                LoadProperty<bool>(IsSiteAdminProperty, reader.GetBoolean("IS_SITE_ADMIN"));
                                LoadProperty<DateTime>(AccountExpirationProperty, reader.GetDateTime("GRACE_EXP"));//new: Sept 2018, used to control if user can create new review
                                Roles = new MobileList<string>();
                                if (reader.GetInt32("IS_COCHRANE_USER") == 1)
                                {
                                    Roles.Add("CochraneUser");
                                }
                                reader.Close();
                                //if not logging on a review, "ReadOnly" role if account is expired
                                if (criteria.ReviewId == 0)
                                {
                                    if (DateTime.Now > AccountExpiration) Roles.Add("ReadOnlyUser");
                                    Roles.Add("RegularUser");//??? Check this do we need to?
                                }
                                //if is site admin, allow to log on any review, revIDs are passed as a negative ID to indicate this is a logon to an arbitrary id
                                else if (IsSiteAdmin && criteria.ReviewId < 0)
                                {
                                    //new code: use a special SP to logon to any review.
                                    using (SqlCommand command2 = new SqlCommand("st_ContactAdminLoginReview", connection))
                                    {
                                        DateTime ContactExp = new DateTime(1, 1, 1);
                                        DateTime ReviewExp = new DateTime(1, 1, 1);
                                        command2.CommandType = System.Data.CommandType.StoredProcedure;
                                        command2.Parameters.Add(new SqlParameter("@userId", UserId));
                                        command2.Parameters.Add(new SqlParameter("@reviewId", -criteria.ReviewId));
#if (CSLA_NETCORE)
                                        //this is a quick, dirty, and probably sad way of recording how we're logging on,
                                        //but it does seem failsafe, as the compiling directive HAS to be correct...
                                        command2.Parameters.Add(new SqlParameter("@Client", "ERweb"));
#endif
                                        command2.Parameters.Add("@GUI", System.Data.SqlDbType.UniqueIdentifier);
                                        command2.Parameters["@GUI"].Direction = System.Data.ParameterDirection.Output;
                                        using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command2.ExecuteReader()))
                                        {
                                            if (reader2.Read())
                                            {
                                                if (-criteria.ReviewId == reader2.GetInt32("REVIEW_ID"))
                                                {
                                                    ContactExp = reader2.GetDateTime("CONTACT_EXP");
                                                    ReviewExp = reader2.GetDateTime("REVIEW_EXP");
                                                    if (ReviewExp == new DateTime(1, 1, 1)) ReviewExp = new DateTime(3000, 1, 1);
                                                    LoadProperty<int>(ReviewIdProperty, -criteria.ReviewId);

                                                    if (ContactExp < DateTime.Today)
                                                    {
                                                        Roles.Add("ReadOnlyUser");//may be a site admin, but his account is expired!
                                                    }
                                                    else Roles.Add("AdminUser");//we'll ignore all restrictions for this kind of logon
                                                }
                                                else
                                                    LoadProperty<int>(ReviewIdProperty, 0);
                                            }
                                            else
                                                LoadProperty<int>(ReviewIdProperty, 0);
                                        }
                                        if (GetProperty<int>(ReviewIdProperty) != 0)
                                        {
                                            LoadProperty(TicketProperty, command2.Parameters["@GUI"].Value.ToString());
                                            LoadProperty(ReviewExpirationProperty, ReviewExp);
                                            LoadProperty(AccountExpirationProperty, ContactExp);
                                        }
                                    }
                                }
                                else // logging into review
                                {
                                    bool isValidCochraneUser = false;
                                    if (IsInRole("CochraneUser"))
                                    {//different logic applies to decide if user has readonly role. 
                                        isValidCochraneUser = CheckCochraneUser(connection, criteria, null);
                                    }
                                    if (!isValidCochraneUser)
                                    {//this could happen if:
                                        //a. user is not Cochrane (no Archie IDs in ER4)
                                        //b. user has an Archie Id but tokens couldn't be refreshed
                                        //c. user was in Archie but isn't anymore.
                                        LoginToReview(connection, criteria);
                                    }
                                }
                                
                            }
                        }
                    }
                    connection.Close();
                }
            }
            //!!!NOTE: this hasn't been proved to work in MVC world.
            else if (criteria.ArchieState != "" 
                && criteria.ArchieState != null
                && criteria.ArchieCode != "" 
                && criteria.ArchieCode != null
                && (criteria.Password == null || criteria.Password == "")
                )
            {//authenticating via Archie
                if (criteria.ReviewId == 0)
                {
                    //this is a first logon: use State and Code to talk with Archie, verify user
                    //create user if necessary
                    //save tokens, Access & Refresh, Token expiration date, ArchieStatus and ArchieCode in TB_CONTACT
                    //Populate ReviewerIdentity with user information, but no ReviewID and no ticket.
                    //also: see below, there is additional trickery in case the ArchieID is not known to ER4*

                    //first, see if we can get an ArchieIdentity, if we can't login fails on Archie AND ER4
                    ArchieIdentity ArId;
                    if (criteria.LoginMode == "Archie0")
                    {//user is logging on again after authentication on Archie, finding that this is a new Archie user and creating the ER4 account
                        ArId = ArchieIdentity.GetArchieIdentityFromLocalDB(criteria.ArchieCode, criteria.ArchieState);
                    }
                    else
                    {//user just coming back from archie authentication
                        ArId = ArchieIdentity.GetArchieIdentity(criteria.ArchieCode, criteria.ArchieState);
                    }
                    if (!ArId.IsAuthenticated)
                    {//longon failed :-(
                        IsAuthenticated = false;
                        //LoadProperty(TicketProperty, "arid not auth");
                        LoadProperty(TicketProperty, "Error: "
                                                     + ArId.Error + Environment.NewLine
                                                     + ArId.ErrorReason);
                        return;
                    }
                    //second step: find the user from ArchieID
                    Roles = new MobileList<string>();
                    Roles.Add("RegularUser");
                    IsAuthenticated = true;
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_ArchieFindER4UserFromArchieID", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@ARCHIE_ID", ArId.ArchieID));
                            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                            {
                                if (reader.Read())
                                {//all good, we got an identity

                                    LoadProperty<int>(UserIdProperty, reader.GetInt32("CONTACT_ID"));
                                    Name = reader.GetString("contact_name");
                                    LoadProperty<bool>(IsSiteAdminProperty, reader.GetBoolean("IS_SITE_ADMIN"));
                                    reader.Close();
                                    Roles = new MobileList<string>();
                                    Roles.Add("RegularUser");
                                    Roles.Add("CochraneUser");
                                }
                                else
                                {//user was authenticated in Archie, but we don't know who this person is, save Archie Keys and signal the anomaly 
                                    LoadProperty<int>(UserIdProperty, 0);
                                    Name = "{UnidentifiedArchieUser}";//this tells the UI the user is unkown to ER4
                                    Roles.Add("CochraneUser");
                                }
                            }
                        }
                        if (UserId == 0 && Name == "{UnidentifiedArchieUser}")
                        {//ArchieID is not already in TB_contact, must save current keys to allow the user to create a contact without re-authenticating
                            using (SqlCommand command = new SqlCommand("st_ArchieSaveUnassignedTokens", connection))
                            {
                                command.CommandType = System.Data.CommandType.StoredProcedure;
                                command.Parameters.Add(new SqlParameter("@ARCHIE_ID", ArId.ArchieID));
                                command.Parameters.Add(new SqlParameter("@TOKEN", ArId.Token));
                                command.Parameters.Add(new SqlParameter("@VALID_UNTIL", ArId.ValidUntil));
                                command.Parameters.Add(new SqlParameter("@REFRESH_T", ArId.RefreshToken));
                                command.Parameters.Add(new SqlParameter("@ARCHIE_CODE", criteria.ArchieCode));
                                command.Parameters.Add(new SqlParameter("@ARCHIE_STATE", criteria.ArchieState));
                                //ADD EMAIL FIELD!?!
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    //if no user is found, save Archie info in TB_UNASSIGNED_ARCHIE_KEYS =>> ADD EMAIL FIELD!?!
                    //user is authenticated (for now) but UserID = 0, this triggers a special dialog on Client: complete new account
                    //if user is found load info and return

                }
                else
                {
                    //user is trying to access a review.
                    //use ArchieStatus and ArchieCode to identify and verify the user identity.
                    //1. lookup ArchieStatus and ArchieCode in TB_CONTACT, one and only one record is expected to come back
                    ArchieIdentity ArId = ArchieIdentity.GetArchieIdentityFromLocalDB(criteria.ArchieCode, criteria.ArchieState);
                    if (!ArId.IsAuthenticated && (ArId.ArchieID == null || ArId.ArchieID == ""))
                    {//there is the special case where Token is older than 1h (invalid) but user has a valid logonTicket at this time (checked below)
                        IsAuthenticated = false;
                        return;
                    }
                    string provisionalName = "";
                    bool provisionalIsSiteAdmin = false;
                    int ProvisionalCID = ArId.getContactID(out provisionalIsSiteAdmin, out provisionalName);
                    if (ProvisionalCID == 0)
                    {
                        IsAuthenticated = false;
                        return;
                    }
                    //2. find out if this user has an active and valid ticket
                    int res = 0;
                    if (!ArId.IsAuthenticated && Ticket != null && Ticket != "")
                    {
                        using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
                        {
                            connection.Open();
                            using (SqlCommand command = new SqlCommand("st_LogonTicket_Check_UserIsLogged", connection))
                            {
                                command.CommandType = System.Data.CommandType.StoredProcedure;
                                command.Parameters.Add(new SqlParameter("@c_ID", ProvisionalCID));
                                command.Parameters.Add("@RES", System.Data.SqlDbType.Int);
                                command.Parameters["@RES"].Direction = System.Data.ParameterDirection.Output;
                                command.ExecuteNonQuery();
                                try
                                {
                                    res = (int)command.Parameters["@RES"].Value; //1 means that user is currently logged on
                                }
                                catch
                                {
                                    res = 0;
                                }
                            }
                        }
                    }
                    //2.1 if s/he does, the user is currently logged on and is changing/reloading a review, this is OK
                    //see addition in the "native" ER4 authentication above: we know that ArchieStatus and A.Code are fresh.
                    if (ArId.IsAuthenticated || res == 1)
                    {//one or the other: if ArId.IsAuthenticated the token is fresh and we trust the user
                        //if res == 1 the current user is now logged on with a valid ticket, and we trust it becuase we know the last logon event was done 
                        //via archie, using the Code & Status strings provided
                        //in short: all is well, we trust this client
                        Name = provisionalName;
                        LoadProperty(UserIdProperty, ProvisionalCID);
                        LoadProperty(IsSiteAdminProperty, provisionalIsSiteAdmin);
                        IsAuthenticated = true;
                        Roles = new MobileList<string>();
                        Roles.Add("RegularUser");
                        Roles.Add("CochraneUser");
                        //basic user info is in, now log on the review
                        
                        using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                        {
                            connection.Open();
                            //in this case, CheckCochraneUser always returns true and does all that can be possibly done:
                            //if the Archie info can't be validated, user shouldn't even be here.
                            CheckCochraneUser(connection, criteria, ArId);
                        }
                    }
                    else
                    {
                        IsAuthenticated = false;
                    }
                    //2.2 if s/he does not have an active logon ticket 
                    //we want the request to be fresh. This is because otherwise it would be possible 
                    //to log on once using Archie and then alter the ER4 client to send just the last ArchieStatus and
                    //ArchieCode pairs, impersonating the user at will 
                    //(man in the middle style, or hiding from us the fact that one has been kicked out from Archie)
                    //in this case, we use ArchieStatus and ArchieCode to retrieve the current Token and check it with Archie.
                    //If the token is valid, the authentication is fresh and we can trust the client.
                    //We log on the user, create our own ticket and proceed as before.

                    //what to do when can't find/validate user based on code+status? Kick user out, somehow (client-side)

                }
            }
            //!!!NOTE: this hasn't been proved to work in MVC world.
            else if (criteria.Password != null && criteria.Password != "" 
                && criteria.Username != null && criteria.Username != "" 
                && criteria.ArchieCode == null || criteria.ArchieCode !=""
                &&criteria.ArchieState != null && criteria.ArchieState != "")//all parameters, we are linking an ER4 account to an Archie one, as well as logging on
            {//this should happen only once for each user who tries to link the accounts (if successful)
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ContactLogin", connection))
                    {//st_ContactLogin needs to be changed so to wipe ArchieCode and Status
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@userName", criteria.Username));
                        command.Parameters.Add(new SqlParameter("@Password", criteria.Password));

                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            if (reader.Read())
                            {
                                //if (criteria.Password == reader.GetString("Password") && reader.GetDateTime("GRACE_EXP") >= DateTime.Today)
                                //{
                                LoadProperty<int>(UserIdProperty, reader.GetInt32("CONTACT_ID"));
                                Name = reader.GetString("contact_name");
                                LoadProperty<bool>(IsSiteAdminProperty, reader.GetBoolean("IS_SITE_ADMIN"));
                                reader.Close();
                                Roles = new MobileList<string>();

                                if (criteria.ReviewId == 0)
                                { //we don't restrict by Expiry date as this is a cochrane user: they can create a review even if their account is not valid.
                                    Roles.Add("RegularUser");//??? Check this
                                }
                                else
                                {//this shouldn't ever happen, kick the hacker out!
                                    IsAuthenticated = false;
                                    return;
                                }
                                IsAuthenticated = true;
                            }
                            else
                            {//wrong ER4 username and PW!
                                IsAuthenticated = false;
                                return;
                            }
                        }
                    }
                    connection.Close();
                }
                //if this point is reached, ER4 authentication worked, we now need to do the linking

                ArchieIdentity ArId = ArchieIdentity.GetUnidentifiedArchieIdentity(criteria.ArchieCode, criteria.ArchieState, UserId);
                if (ArId.IsAuthenticated)
                {//all good user is linked and data was saved
                    IsAuthenticated = true;
                    Roles.Add("CochraneUser");
                }
                else
                {//booooo
                    IsAuthenticated = false;
                    LoadProperty<int>(UserIdProperty, 0);
                    Name = "";
                    LoadProperty<bool>(IsSiteAdminProperty, false);
                }
            }
#if (CSLA_NETCORE)
            else if (criteria.ReviewId != 0 && criteria.ContactId != 0)
            {//opening a review, user is already authenticated via the [Authorise] directive in the controller.
             //what we need to check depends on whether we think the user is licensed via archie or not...
             //we use criteria.LoginMode to figure this out.
                
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    Roles = new MobileList<string>();
                    
                    LoadProperty<int>(UserIdProperty, criteria.ContactId);
                    if (criteria.LoginMode == "ERWebArchie")
                    {//this is a known archie user, let's check.
                        Roles.Add("CochraneUser");//this gets removed if check doesn't work
                        bool isValidCochraneUser = false;
                        //line below will also open the review and apply licensing rules, if user is indeed in Archie (access token is valid or refresh worked).
                        isValidCochraneUser = CheckCochraneUser(connection, criteria, null);
                        if (!isValidCochraneUser)
                        {//this could happen if:
                         //a. user is not Cochrane (no Archie IDs in ER4) - shouldn't happen in this particular case!
                         //b. user has an Archie Id but tokens couldn't be refreshed
                         //c. user was in Archie but isn't anymore.
                            LoginToReview(connection, criteria);
                        }
                    }
                    else
                    {//not an Archie user!
                        LoginToReview(connection, criteria);
                    }
                    if (ReviewId != 0 && Roles.Count > 0)
                    {//it worked
                     //LoadProperty<int>(UserIdProperty, criteria.ContactId);
                        LoadProperty<string>(NameProperty, criteria.DisplayName);
                        LoadProperty<string>(LoginModeProperty, criteria.LoginMode);
                        IsAuthenticated = true;
                    }
                    else
                    {//logon to review failed!
                        LoadProperty<int>(ReviewIdProperty, 0);
                        LoadProperty<int>(UserIdProperty, 0);
                        IsAuthenticated = false;
                    }
                }
                
            }
            else if (criteria.ClaimsP != null && criteria.ClaimsP.Claims != null && criteria.ClaimsP.Claims.Count() > 0)
            {//build the RI object quickly based on the data we got from the JWT, WITHOUT passing through the DB.
                //this is used when an MVC controller will rely on a CSLA BO that needs a fully formed RI object.
                //as a result, the Ticket will be checked against the DB if and where RI.ReviewId is retreived, as in ER4.
                //thus, it's OK to avoid checking on the DB at this stage.
                Roles = new MobileList<string>();
                foreach (System.Security.Claims.Claim claim in criteria.ClaimsP.Claims)
                {
                    switch (claim.Type)
                    {
                        case System.Security.Claims.ClaimTypes.Role:
                            Roles.Add(claim.Value);
                            break;
                        case "reviewId":
                            int tmp;
                            if (int.TryParse(claim.Value, out tmp)) LoadProperty<int>(ReviewIdProperty, tmp);
                            break;
                        case "userId":
                            int tmp2;
                            if (int.TryParse(claim.Value, out tmp2)) LoadProperty<int>(UserIdProperty, tmp2);
                            break;
                        case "name":
                            LoadProperty<string>(NameProperty, claim.Value);
                            break;
                        case "reviewTicket":
                            LoadProperty<string>(TicketProperty, claim.Value);
                            break;
                        case "isSiteAdmin":
                            LoadProperty<bool>(IsSiteAdminProperty, (claim.Value.ToLower() == "true"));
                            break;
                        default:
                            break;
                    }
                }
                if (Roles.Count > 0)
                {//ticket could be parsed/decrypted and it contained roles: user is for real
                    this.IsAuthenticated = true;
                }
            }
#endif
        }
        private void LoginToReview(SqlConnection connection, CredentialsCriteria criteria)
        {
            using (SqlCommand command2 = new SqlCommand("st_ContactLoginReview", connection))
            {
                DateTime check;
                command2.CommandType = System.Data.CommandType.StoredProcedure;
                command2.Parameters.Add(new SqlParameter("@userId", UserId));
                command2.Parameters.Add(new SqlParameter("@reviewId", criteria.ReviewId));
                command2.Parameters.Add(new SqlParameter("@IsArchieUser", false));
#if (CSLA_NETCORE)
                //this is a quick, dirty, and probably sad way of recording how we're logging on,
                //but it does seem failsafe, as the compiling directive HAS to be correct...
                command2.Parameters.Add(new SqlParameter("@Client", "ERweb"));
#endif
                command2.Parameters.Add("@GUI", System.Data.SqlDbType.UniqueIdentifier);
                command2.Parameters["@GUI"].Direction = System.Data.ParameterDirection.Output;
                command2.CommandTimeout = 60;
                using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command2.ExecuteReader()))
                {
                    if (reader2.Read())
                    {
                        if (criteria.ReviewId == reader2.GetInt32("REVIEW_ID")
                           && (reader2.GetString("ARCHIE_ID") == "" || reader2.GetString("ARCHIE_ID") == "prospective_______")
                           //we do not allow non archie users to access registered Archie reviews, 
                           //but we do allow non archie users to open prospective archie reviews
                           )
                        {
                            ContactExp = reader2.GetDateTime("CONTACT_EXP");
                            ReviewExp = reader2.GetDateTime("REVIEW_EXP");
                            if (ReviewExp == new DateTime(1, 1, 1)) ReviewExp = new DateTime(3000, 1, 1);
                            LoadProperty<int>(ReviewIdProperty, criteria.ReviewId);
                            LoadProperty<bool>(IsSiteAdminProperty, reader2.GetBoolean("IS_SITE_ADMIN"));
                            //if (check == new DateTime(1, 1, 1)) check = DateTime.Now.AddDays(1);
                            check = ContactExp < ReviewExp ? ContactExp : ReviewExp;
                            if (check < DateTime.Today && (reader2.GetInt32("FUNDER_ID") != UserId || ContactExp < DateTime.Today))
                            {//if either the account or the review are expired and (this is not the review owner, or the current account is expired) then we 
                             //mark the user as ReadOnly. 
                             //Hence, if the the review is expired, the account is not, and the user is the review owner, the user DOES NOT get the ReadOnly restrictions
                                Roles.Add("ReadOnlyUser");
                            }

                            Roles.Add(reader2.GetString("Role"));
                            while (reader2.Read())//query returns multiple lines, one per Role, so in case user has many roles, we read all lines
                            {
                                Roles.Add(reader2.GetString("Role"));
                            }
                            //Roles.Add("AdminUser");// ADDED BY JAMES TEMPORARILY!!!

                        }
                        else
                        {
                            LoadProperty<int>(ReviewIdProperty, 0);
                            IsAuthenticated = false;
                        }
                    }
                    else
                    {
                        LoadProperty<int>(ReviewIdProperty, 0);
                        IsAuthenticated = false;
                    }
                }


                if (GetProperty<int>(ReviewIdProperty) != 0)
                {
                    LoadProperty(TicketProperty, command2.Parameters["@GUI"].Value.ToString());
                    LoadProperty(ReviewExpirationProperty, ReviewExp);
                    LoadProperty(AccountExpirationProperty, ContactExp);
                }
            }
        }
        private bool CheckCochraneUser(SqlConnection connection, CredentialsCriteria criteria, ArchieIdentity Ai)
        {
            //with cochrane users, we want:
            //1. personal reviews are never readonly
            //2. cochrane reviews do not expire, readonly state depends on their Archie state
            //2.1 if a cochrane review is NOT "checkedOuthere" then it's readonly
            //3. shared reviews work normally. User needs the review and her own account to be paid-for.
            //this method is separate because it's called in two separate places in the fetch method and because it is expected to be extended.
            //checks that should happen are:
            //C1 check that the user still has access to Archie (re-authenticate!)
            //C2 if we are opening and Archie review (cochrane rev, not prospecitve) check that the user does really have access to the chosen Cochrane review.
            
            //check1 if user can't get valid tokens from Archie, (maybe isn't in Archie anymore, zap her ER4 archie credentials) remove the "cochraneUser" role and return false.
            if (Ai == null)
            {//user logged on via ER4 credentials, let's check if her Archie account is OK
                Ai = ArchieIdentity.GetArchieIdentity(this, true);
                if (!Ai.IsAuthenticated)
                {
                    //NOT IMPLEMENTED, zap the archie values for this user, not sure we should do this: we can't be sure user isn't in Archie anymore
                    Roles.Remove("CochraneUser");
                    return false;//will tell the rest of Ri to treat this user as a standard ER4 user
                }
            }
            if (!Ai.IsAuthenticated)
            {//shouldn't happen: we got here via authentication through archie, we don't like this!
                Roles.Remove("CochraneUser");
                IsAuthenticated = false;
                return true;//redundant, but signals that we should not treat this user as a standard ER4 account.
            }
            
            using (SqlCommand command2 = new SqlCommand("st_ContactLoginReview", connection))
            {//if accessing an Archie review, SQL will add user to it (users don't see the review if Archie don't believe they belong to it).
                DateTime check;
                command2.CommandType = System.Data.CommandType.StoredProcedure;
                command2.Parameters.Add(new SqlParameter("@userId", UserId));
                command2.Parameters.Add(new SqlParameter("@reviewId", criteria.ReviewId));
                command2.Parameters.Add(new SqlParameter("@IsArchieUser", true));
                command2.Parameters.Add("@GUI", System.Data.SqlDbType.UniqueIdentifier);
                command2.Parameters["@GUI"].Direction = System.Data.ParameterDirection.Output;
                command2.CommandTimeout = 60;
                using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command2.ExecuteReader()))
                {
                    if (reader2.Read())
                    {
                        if (criteria.ReviewId == reader2.GetInt32("REVIEW_ID"))
                        {
                            ContactExp = reader2.GetDateTime("CONTACT_EXP");
                            ReviewExp = reader2.GetDateTime("REVIEW_EXP");
                            if (ReviewExp == new DateTime(1, 1, 1))//either a personal review or a Cochrane one we have stuff to do
                            {
                                ReviewExp = new DateTime(3000, 1, 1);//doesn't expire
                                string CochRevID = reader2.GetString("ARCHIE_ID");
                                if (CochRevID != null && CochRevID != "")
                                {//this is a cochrane review
                                    if (CochRevID != "prospective_______")
                                    {
                                        //NOT IMPLEMENTED: check2, go to archie and see if the user does have access to this review there.
                                        //if not:
                                        //LoadProperty<int>(ReviewIdProperty, 0);//this tells the client user didn't manage to open the review
                                        //return true;//assuming Check1 user is valid cochrane, but can't open this review
                                    }

                                    if (CochRevID != "prospective_______" && reader2.GetBoolean("IS_CHECKEDOUT_HERE") == false)//not prospective review, and not checked out to ER4: read only!
                                    {
                                        Roles.Add("ReadOnlyUser");
                                    }
                                }
                                //this is the same for Cochrane and personal reviews
                                LoadProperty<int>(ReviewIdProperty, criteria.ReviewId);//all is fine
                                ContactExp = DateTime.Now.AddYears(1);//accessing a personal or Cochrane review, account doesn't really expire
                                Roles.Add(reader2.GetString("Role"));
                                while (reader2.Read())//query returns multiple lines, one per Role, so in case user has many roles, we read all lines
                                {
                                    Roles.Add(reader2.GetString("Role"));
                                }
                            }
                            else
                            {//not accessing a cochrane or private review, we don't care about the Cochraness of this account old sort of validation will happen
                                ContactExp = reader2.GetDateTime("CONTACT_EXP");
                                ReviewExp = reader2.GetDateTime("REVIEW_EXP");
                                if (ReviewExp == new DateTime(1, 1, 1)) ReviewExp = new DateTime(3000, 1, 1);
                                LoadProperty<int>(ReviewIdProperty, criteria.ReviewId);
                                //if (check == new DateTime(1, 1, 1)) check = DateTime.Now.AddDays(1);
                                check = ContactExp < ReviewExp ? ContactExp : ReviewExp;
                                if (check < DateTime.Today && (reader2.GetInt32("FUNDER_ID") != UserId || ContactExp < DateTime.Today))
                                {//if either the account or the review are expired and (this is not the review owner, or the current account is expired) then we 
                                    //mark the user as ReadOnly. 
                                    //Hence, if the the review is expired, the account is not, and the user is the review owner, the user DOES NOT get the ReadOnly restrictions
                                    Roles.Add("ReadOnlyUser");
                                }

                                Roles.Add(reader2.GetString("Role"));
                                while (reader2.Read())//query returns multiple lines, one per Role, so in case user has many roles, we read all lines
                                {
                                    Roles.Add(reader2.GetString("Role"));
                                }
                            }
                        }
                        else
                        {//SP didn't find expected review, uh?
                            LoadProperty<int>(ReviewIdProperty, 0);
                            IsAuthenticated = false;
                        }
                    }
                    else
                    {//SP didn't find any records!
                        LoadProperty<int>(ReviewIdProperty, 0);
                        IsAuthenticated = false;
                    }
                }

                if (GetProperty<int>(ReviewIdProperty) != 0)
                {
                    LoadProperty(TicketProperty, command2.Parameters["@GUI"].Value.ToString());
                    LoadProperty(ReviewExpirationProperty, ReviewExp);
                    LoadProperty(AccountExpirationProperty, ContactExp);
                }
            }
            return true;//we only return false if the Archie status didn't check out
        }
        

#endif
    }
    
}
