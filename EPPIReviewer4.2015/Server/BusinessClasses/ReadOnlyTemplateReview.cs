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

#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyTemplateReview : ReadOnlyBase<ReadOnlyTemplateReview>
    {
        public ReadOnlyTemplateReview()
        {
            LoadProperty(ReviewSetIdsProperty, new MobileList<int>());
        }

        public override string ToString()
        {
            return this.TemplateName;
        }
        
        public static readonly PropertyInfo<int> TemplateIdProperty = RegisterProperty<int>(new PropertyInfo<int>("TemplateId", "TemplateId", 0));
        public int TemplateId
        {
            get
            {
                
                return GetProperty(TemplateIdProperty);
            }
        }

        public static readonly PropertyInfo<string> TemplateNameProperty = RegisterProperty<string>(new PropertyInfo<string>("TemplateName", "TemplateName", string.Empty));
        public string TemplateName
        {
            get
            {
                return GetProperty(TemplateNameProperty);
            }
        }
        public static readonly PropertyInfo<string> TemplateDescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("TemplateDescription", "TemplateDescription", string.Empty));
        public string TemplateDescription
        {
            get
            {
                return GetProperty(TemplateDescriptionProperty);
            }
        }
        public static readonly PropertyInfo<MobileList<int>> ReviewSetIdsProperty = RegisterProperty<MobileList<int>>(new PropertyInfo<MobileList<int>>("ReviewSetIds", "ReviewSetIds"));
        public MobileList<int> ReviewSetIds
        {
            get
            {
                return GetProperty(ReviewSetIdsProperty);
            }
        }
       
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyReview), canRead);
        //    //AuthorizationRules.AllowRead(ReviewNameProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
        //}

#if !SILVERLIGHT

        public static ReadOnlyTemplateReview GetReadOnlyTemplateReview(int templateId, string name, string description)
        {
            ReadOnlyTemplateReview res = new ReadOnlyTemplateReview();
            res.LoadProperty(TemplateIdProperty, templateId);
            res.LoadProperty(TemplateNameProperty, name);
            res.LoadProperty(TemplateDescriptionProperty, description);
            return res;
        }



#endif
    }
}
