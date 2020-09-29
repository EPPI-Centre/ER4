using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
using Csla.Rules.CommonRules;
using Csla.Rules;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Collections;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class WebDB : BusinessBase<WebDB>
    {
        public WebDB() { }


        public override string ToString()
        {
            return WebDBName;
        }



        public static readonly PropertyInfo<int> WebDBIdProperty = RegisterProperty<int>(new PropertyInfo<int>("WebDBId", "WebDBId", 0));
        public int WebDBId
        {
            get
            {
                return GetProperty(WebDBIdProperty);
            }
            set
            {
                SetProperty(WebDBIdProperty, value);
            }
        }
        

        public static readonly PropertyInfo<string> WebDBNameProperty = RegisterProperty<string>(new PropertyInfo<string>("WebDBName", "WebDBName", ""));
        public string WebDBName
        {
            get
            {
                return GetProperty(WebDBNameProperty);
            }
            set
            {
                SetProperty(WebDBNameProperty,
                    value.Length > 1000 ? value.Substring(0,1000) : value);
            }
        }
        public static readonly PropertyInfo<string> WebDBDescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("WebDBDescription", "WebDBDescription", ""));
        public string WebDBDescription
        {
            get
            {
                return GetProperty(WebDBDescriptionProperty);
            }
            set
            {
                SetProperty(WebDBDescriptionProperty, value);
            }
        }

        public static readonly PropertyInfo<long> AttributeIdFilterProperty = RegisterProperty<long>(new PropertyInfo<long>("AttributeIdFilter", "AttributeIdFilter"));
        public long AttributeIdFilter
        {
            get
            {
                return GetProperty(AttributeIdFilterProperty);
            }
            set
            {
                SetProperty(AttributeIdFilterProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> IsOpenProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsOpen", "IsOpen"));
        public bool IsOpen
        {
            get
            {
                return GetProperty(IsOpenProperty);
            }
            set
            {
                SetProperty(IsOpenProperty, value);
            }
        }

        public static readonly PropertyInfo<string> UserNameProperty = RegisterProperty<string>(new PropertyInfo<string>("UserName", "UserName", ""));
        public string UserName
        {
            get
            {
                return GetProperty(UserNameProperty);
            }
            set
            {
                SetProperty(UserNameProperty,
                    value.Length > 50 ? value.Substring(0, 50) : value);
            }
        }

        public static readonly PropertyInfo<string> PasswordProperty = RegisterProperty<string>(new PropertyInfo<string>("Password", "Password", ""));
        public string Password
        {
            get
            {
                return GetProperty(PasswordProperty);
            }
            set
            {
                SetProperty(PasswordProperty,
                    value.Length > 2000 ? value.Substring(0, 2000) : value);
            }
        }

        public static readonly PropertyInfo<string> CreatedByProperty = RegisterProperty<string>(new PropertyInfo<string>("CreatedBy", "CreatedBy", ""));
        public string CreatedBy
        {
            get
            {
                return GetProperty(CreatedByProperty);
            }
            set
            {
                SetProperty(CreatedByProperty, value);
            }
        }
        public static readonly PropertyInfo<string> EditedByProperty = RegisterProperty<string>(new PropertyInfo<string>("EditedBy", "EditedBy", ""));
        public string EditedBy
        {
            get
            {
                return GetProperty(EditedByProperty);
            }
            set
            {
                SetProperty(EditedByProperty, value);
            }
        }
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser", "RegularUser" };
        //    string[] denyEditSave = new string[] { "ReadOnlyUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ReviewSet), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ReviewSet), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ReviewSet), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ReviewSet), canRead);
        //    AuthorizationRules.DenyEdit(typeof(ReviewSet), denyEditSave);

        //    //AuthorizationRules.AllowRead(ReviewSetIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(SetIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(AllowCodingEditsProperty, canRead);
        //    //AuthorizationRules.AllowRead(SetTypeIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(SetNameProperty, canRead);
        //    //AuthorizationRules.AllowRead(SetTypeProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeSetProperty, canRead);
        //    //AuthorizationRules.AllowRead(CodingIsFinalProperty, canRead);

        //    //AuthorizationRules.AllowWrite(AllowCodingEditsProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(SetNameProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(CodingIsFinalProperty, canWrite);
        //}

        protected override void AddBusinessRules()
        {
            BusinessRules.AddRule(new IsNotInRole(AuthorizationActions.EditObject, "ReadOnlyUser"));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT
        public void MarkAsOldAndDirty()
        {
            this.MarkOld();
            this.MarkDirty(true);
        }
        

        protected override void DataPortal_Insert()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbCreateOrEdit", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId ));
                    command.Parameters.Add(new SqlParameter("@AttIdFilter", AttributeIdFilter)); 
                    command.Parameters.Add(new SqlParameter("@isOpen", IsOpen));
                    command.Parameters.Add(new SqlParameter("@ContactId", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@Username", UserName));
                    if (UserName != null && UserName.Length > 0)
                        command.Parameters.Add(new SqlParameter("@Password", Password));

                    command.Parameters.Add(new SqlParameter("@WebDbName", WebDBName));
                    command.Parameters.Add(new SqlParameter("@Description", WebDBDescription));

                    SqlParameter par = new SqlParameter("@WebDbId", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    par.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(par); 

                    SqlParameter par2 = new SqlParameter("@Result", System.Data.SqlDbType.Int);
                    par2.Value = 0;
                    par.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(par2); 
                    command.ExecuteNonQuery();
                    int? res = par2.Value as int?;
                    if (res == -1)
                    {
                        throw new Exception("Can't create Web Database: Username or Password are too short.");
                    }
                    else if (res == -2)
                    {
                        throw new Exception("Can't create Web Database: current user isn't authorised.");
                    }
                    else if (res == null || res < 0)
                    {
                        throw new Exception("Can't create Web Database: an unexpected error occurred.");
                    }
                    //if we're here, no exceptions happened: all is well
                    Password = ""; //From now on, it's hashed and should not appear in this object.
                    LoadProperty(WebDBIdProperty, par.Value);
                    LoadProperty(CreatedByProperty, ri.Name);
                    LoadProperty(EditedByProperty, ri.Name);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbCreateOrEdit", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@AttIdFilter", AttributeIdFilter));
                    command.Parameters.Add(new SqlParameter("@isOpen", IsOpen));
                    command.Parameters.Add(new SqlParameter("@ContactId", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@Username", UserName));
                    if (UserName != null && UserName.Length > 0)
                        command.Parameters.Add(new SqlParameter("@Password", Password));

                    command.Parameters.Add(new SqlParameter("@WebDbName", WebDBName));
                    command.Parameters.Add(new SqlParameter("@Description", WebDBDescription));

                    SqlParameter par = new SqlParameter("@WebDbId", System.Data.SqlDbType.Int);
                    par.Value = WebDBId;
                    par.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(par);

                    SqlParameter par2 = new SqlParameter("@Result", System.Data.SqlDbType.Int);
                    par2.Value = 0;
                    par.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(par2);
                    command.ExecuteNonQuery();
                    int? res = par2.Value as int?;
                    if (res == -1)
                    {
                        throw new Exception("Can't edit Web Database: Username or Password are too short.");
                    }
                    else if (res == -2)
                    {
                        throw new Exception("Can't edit Web Database: current user isn't authorised.");
                    }
                    else if (res == -3)
                    {
                        throw new Exception("Can't edit Web Database: WabDatabase not found.");
                    }
                    else if (res == null || res < 0)
                    {
                        throw new Exception("Can't create Web Database: an unexpected error occurred.");
                    }
                    //if we're here, no exceptions happened: all is well
                    Password = ""; //From now on, it's hashed and should not appear in this object.
                    LoadProperty(EditedByProperty, ri.Name);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_ID", WebDBId));
	                command.ExecuteNonQuery();
                }
                connection.Close();
            }

            
        }

        internal static WebDB GetWebDb(SafeDataReader reader)
        {
            WebDB returnValue = new WebDB();
            returnValue.MarkOld();
            returnValue.LoadProperty(WebDBIdProperty, reader.GetInt32("WEBDB_ID"));
            returnValue.LoadProperty(AttributeIdFilterProperty, reader.GetInt64("WITH_ATTRIBUTE_ID"));
            returnValue.LoadProperty(IsOpenProperty, reader.GetBoolean("IS_OPEN"));
            returnValue.LoadProperty(UserNameProperty, reader.GetString("USERNAME"));
            returnValue.LoadProperty(WebDBNameProperty, reader.GetString("WEBDB_NAME"));
            returnValue.LoadProperty(WebDBDescriptionProperty, reader.GetString("DESCRIPTION"));
            returnValue.LoadProperty(CreatedByProperty, reader.GetString("CREATED_BY"));
            returnValue.LoadProperty(EditedByProperty, reader.GetString("EDITED_BY"));
            return returnValue;
        }

#endif
    }
}
