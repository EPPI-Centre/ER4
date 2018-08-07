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
    public class ReviewerIdentityWebClient : CslaIdentity
    {
        public static readonly PropertyInfo<int> UserIdProperty = RegisterProperty<int>(typeof(ReviewerIdentityWebClient), new PropertyInfo<int>("UserId", "User Id", 0));
        public int UserId
        {
            get
            {
                return GetProperty<int>(UserIdProperty);
            }
        }
        public static readonly PropertyInfo<bool> IsSiteAdminProperty = RegisterProperty<bool>(typeof(ReviewerIdentityWebClient), new PropertyInfo<bool>("IsSiteAdmin", "IsSiteAdmin", false));
        public bool IsSiteAdmin
        {
            get
            {
                return GetProperty<bool>(IsSiteAdminProperty);
            }
        }
        public static readonly PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(typeof(ReviewerIdentityWebClient), new PropertyInfo<int>("ReviewId", "Review Id", 0));
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
        public static readonly PropertyInfo<string> TicketProperty = RegisterProperty<string>(typeof(ReviewerIdentityWebClient), new PropertyInfo<string>("Ticket", "Ticket", 0));
        public string Ticket
        {
            get
            {
                return GetProperty<string>(TicketProperty);
            }
        }
        public static readonly PropertyInfo<string> TokenProperty = RegisterProperty<string>(typeof(ReviewerIdentityWebClient), new PropertyInfo<string>("Token", "Token", 0));
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
        public static readonly PropertyInfo<DateTime> AccountExpirationProperty = RegisterProperty<DateTime>(typeof(ReviewerIdentityWebClient), new PropertyInfo<DateTime>("AccountExpiration", "AccountExpiration"));
        public DateTime AccountExpiration
        {
            get
            {
                return GetProperty<DateTime>(AccountExpirationProperty);
            }
        }
        public static readonly PropertyInfo<DateTime> ReviewExpirationProperty = RegisterProperty<DateTime>(typeof(ReviewerIdentityWebClient), new PropertyInfo<DateTime>("ReviewExpiration", "ReviewExpiration"));
        public DateTime ReviewExpiration
        {
            get
            {
                return GetProperty<DateTime>(ReviewExpirationProperty);
            }
        }
        public static readonly PropertyInfo<string> LoginModeProperty = RegisterProperty<string>(typeof(ReviewerIdentityWebClient), new PropertyInfo<string>("LoginMode", "LoginMode", 0));
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
            return (!IsInRole("ReadOnlyUser"));
        }

        public bool IsCochraneUser
        {
            get
            {
                return (IsInRole("CochraneUser"));
            }
        }

        public static ReviewerIdentityWebClient GetIdentity(string username, string password, int reviewId, string LoginMode, string roles)
        {
            return GetCslaIdentity<ReviewerIdentityWebClient>(new CredentialsCriteria(username, password, reviewId, LoginMode));
        }
#if (CSLA_NETCORE)
        public static ReviewerIdentityWebClient GetIdentity(int contactId, int reviewId)
        {
            return GetCslaIdentity<ReviewerIdentityWebClient>(new CredentialsCriteria(contactId, reviewId, "CSLA"));
        }
#endif
        DateTime ContactExp = new DateTime(1, 1, 1);
        DateTime ReviewExp = new DateTime(1, 1, 1);
        private void DataPortal_Fetch(CredentialsCriteria criteria)
        {
            LoadProperty(LoginModeProperty, criteria.LoginMode);
            if (criteria.Password != null && criteria.Password != "" && criteria.Username != null && criteria.Username != "" && (criteria.ArchieCode == null || criteria.ArchieCode == ""))
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

                                Roles = new MobileList<string>();
                                if (reader.GetInt32("IS_COCHRANE_USER") == 1)
                                {
                                    Roles.Add("CochraneUser");
                                }
                                reader.Close();
                                //if not logging on a review, give default "ReadOnly" role
                                if (criteria.ReviewId == 0)
                                {
                                    Roles.Add("RegularUser");//??? Check this
                                }//if is site admin, allow to log on any review, revIDs are passed as a negative ID to indicate this is a logon to an arbitrary id
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
                                        //{//different logic applies to decide if user has readonly role. 
                                        //    isValidCochraneUser = CheckCochraneUser(connection, criteria, null);
                                        //}
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
#if (CSLA_NETCORE)
            else if (criteria.ReviewId != 0 && criteria.ContactId != 0)
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    Roles = new MobileList<string>();
                    LoadProperty<int>(UserIdProperty, criteria.ContactId);
                    LoginToReview(connection, criteria);
                    if (ReviewId != 0 && Roles.Count > 0)
                    {//it worked
                        LoadProperty<int>(UserIdProperty, criteria.ContactId);
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
        }
#endif
        public void LoginToReview(SqlConnection connection, CredentialsCriteria criteria)
        {
            using (SqlCommand command2 = new SqlCommand("st_ContactLoginReview", connection))
            {
                DateTime check;
                command2.CommandType = System.Data.CommandType.StoredProcedure;
                command2.Parameters.Add(new SqlParameter("@userId", UserId));
                command2.Parameters.Add(new SqlParameter("@reviewId", criteria.ReviewId));
                command2.Parameters.Add(new SqlParameter("@IsArchieUser", false));
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
    }

}

