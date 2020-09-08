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
using System.Security.Claims;

#if !SILVERLIGHT
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

            get
            {
                return GetProperty<int>(ReviewIdProperty);
            }

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
            if (Roles == null) return false;
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

        DateTime ContactExp = new DateTime(1, 1, 1);
        DateTime ReviewExp = new DateTime(1, 1, 1);
        private void DataPortal_Fetch(CredentialsCriteria criteria)
        {
            LoadProperty(LoginModeProperty, "WebDb");
            LoadProperty<int>(UserIdProperty, 0);
            IsAuthenticated = true;//we set it here, but might reverse if something down the line fails
            Name = "";
            //build the RI object quickly based on the data we got from the JWT, WITHOUT passing through the DB.
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
    }
    
}
