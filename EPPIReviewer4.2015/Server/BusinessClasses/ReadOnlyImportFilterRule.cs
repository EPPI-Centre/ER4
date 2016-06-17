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
using System.Text.RegularExpressions;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyImportFilterRule : ReadOnlyBase<ReadOnlyImportFilterRule>
    {
#if SILVERLIGHT
    public ReadOnlyImportFilterRule()
    {
        
    }
#else
        private ReadOnlyImportFilterRule()
        {
            
        }
#endif
        public override string ToString()
        {
            return RuleName;
        }

        internal static ReadOnlyImportFilterRule NewReadOnlyImportFilterRule()
        {
            ReadOnlyImportFilterRule returnValue = new ReadOnlyImportFilterRule();
            return returnValue;
        }

        #region properties
        private static PropertyInfo<int> FilterIDProperty = RegisterProperty<int>(new PropertyInfo<int>("FilterID", "FilterID"));
        public int FilterID
        {
            get
            {
                return GetProperty(FilterIDProperty);
            }
        }
        private static PropertyInfo<MobileList<TypeRules>> typeRulesProperty = RegisterProperty<MobileList<TypeRules>>(new PropertyInfo<MobileList<TypeRules>>("typeRules", "typeRules"));
        public MobileList<TypeRules> typeRules
        {
            get
            {
                return GetProperty(typeRulesProperty);
            }
        }
        private static PropertyInfo<MobileDictionary<int, string>> typesMapProperty = RegisterProperty<MobileDictionary<int, string>>(new PropertyInfo<MobileDictionary<int, string>>("typesMap", "typesMap"));
        public MobileDictionary<int, string> typesMap
        {
            get
            {
                return GetProperty(typesMapProperty);
            }
        }
        private static PropertyInfo<string> RuleNameProperty = RegisterProperty<string>(new PropertyInfo<string>("RuleName", "RuleName"));
        public string RuleName
        {
            get
            {
                return GetProperty(RuleNameProperty);
            }
        }
        private static PropertyInfo<string> StartOfNewRecProperty = RegisterProperty<string>(new PropertyInfo<string>("StartOfNewRec", "StartOfNewRec"));
        public string StartOfNewRec
        {
            get
            {
                return GetProperty(StartOfNewRecProperty);
            }
        }
        private static PropertyInfo<string> typeFieldProperty = RegisterProperty<string>(new PropertyInfo<string>("typeField", "typeField"));
        public string typeField
        {
            get
            {
                return GetProperty(typeFieldProperty);
            }
        }
        private static PropertyInfo<string> StartOfNewFieldProperty = RegisterProperty<string>(new PropertyInfo<string>("StartOfNewField", "StartOfNewField"));
        public string StartOfNewField
        {
            get
            {
                return GetProperty(StartOfNewFieldProperty);
            }
        }
        private static PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title"));
        public string Title
        {
            get
            {
                return GetProperty(TitleProperty);
            }
        }
        private static PropertyInfo<string> pTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("pTitle", "pTitle"));
        public string pTitle
        {
            get
            {
                return GetProperty(pTitleProperty);
            }
        }
        private static PropertyInfo<string> shortTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("shortTitle", "shortTitle"));
        public string shortTitle
        {
            get
            {
                return GetProperty(shortTitleProperty);
            }
        }
        private static PropertyInfo<string> DateProperty = RegisterProperty<string>(new PropertyInfo<string>("Date", "Date"));
        public string Date
        {
            get
            {
                return GetProperty(DateProperty);
            }
        }
        private static PropertyInfo<string> MonthProperty = RegisterProperty<string>(new PropertyInfo<string>("Month", "Month"));
        public string Month
        {
            get
            {
                return GetProperty(MonthProperty);
            }
        }
        private static PropertyInfo<string> AuthorProperty = RegisterProperty<string>(new PropertyInfo<string>("Author", "Author"));
        public string Author
        {
            get
            {
                return GetProperty(AuthorProperty);
            }
        }
        private static PropertyInfo<string> ParentAuthorProperty = RegisterProperty<string>(new PropertyInfo<string>("ParentAuthor", "ParentAuthor"));
        public string ParentAuthor
        {
            get
            {
                return GetProperty(ParentAuthorProperty);
            }
        }
        private static PropertyInfo<string> StandardNProperty = RegisterProperty<string>(new PropertyInfo<string>("StandardN", "StandardN"));
        public string StandardN
        {
            get
            {
                return GetProperty(StandardNProperty);
            }
        }
        private static PropertyInfo<string> CityProperty = RegisterProperty<string>(new PropertyInfo<string>("City", "City"));
        public string City
        {
            get
            {
                return GetProperty(CityProperty);
            }
        }
        private static PropertyInfo<string> PublisherProperty = RegisterProperty<string>(new PropertyInfo<string>("Publisher", "Publisher"));
        public string Publisher
        {
            get
            {
                return GetProperty(PublisherProperty);
            }
        }
        private static PropertyInfo<string> InstitutionProperty = RegisterProperty<string>(new PropertyInfo<string>("Institution", "Institution"));
        public string Institution
        {
            get
            {
                return GetProperty(InstitutionProperty);
            }
        }
        private static PropertyInfo<string> VolumeProperty = RegisterProperty<string>(new PropertyInfo<string>("Volume", "Volume"));
        public string Volume
        {
            get
            {
                return GetProperty(VolumeProperty);
            }
        }
        private static PropertyInfo<string> IssueProperty = RegisterProperty<string>(new PropertyInfo<string>("Issue", "Issue"));
        public string Issue
        {
            get
            {
                return GetProperty(IssueProperty);
            }
        }
        private static PropertyInfo<string> EditionProperty = RegisterProperty<string>(new PropertyInfo<string>("Edition", "Edition"));
        public string Edition
        {
            get
            {
                return GetProperty(EditionProperty);
            }
        }
        private static PropertyInfo<string> StartPageProperty = RegisterProperty<string>(new PropertyInfo<string>("StartPage", "StartPage"));
        public string StartPage
        {
            get
            {
                return GetProperty(StartPageProperty);
            }
        }
        private static PropertyInfo<string> EndPageProperty = RegisterProperty<string>(new PropertyInfo<string>("EndPage", "EndPage"));
        public string EndPage
        {
            get
            {
                return GetProperty(EndPageProperty);
            }
        }
        private static PropertyInfo<string> PagesProperty = RegisterProperty<string>(new PropertyInfo<string>("Pages", "Pages"));
        public string Pages
        {
            get
            {
                return GetProperty(PagesProperty);
            }
        }
        private static PropertyInfo<string> AvailabilityProperty = RegisterProperty<string>(new PropertyInfo<string>("Availability", "Availability"));
        public string Availability
        {
            get
            {
                return GetProperty(AvailabilityProperty);
            }
        }
        private static PropertyInfo<string> UrlProperty = RegisterProperty<string>(new PropertyInfo<string>("Url", "Url"));
        public string Url
        {
            get
            {
                return GetProperty(UrlProperty);
            }
        }
        private static PropertyInfo<string> AbstractProperty = RegisterProperty<string>(new PropertyInfo<string>("Abstract", "Abstract"));
        public string Abstract
        {
            get
            {
                return GetProperty(AbstractProperty);
            }
        }
        private static PropertyInfo<string> OldItemIdProperty = RegisterProperty<string>(new PropertyInfo<string>("OldItemId", "OldItemId"));
        public string OldItemId
        {
            get
            {
                return GetProperty(OldItemIdProperty);
            }
        }
        private static PropertyInfo<string> NotesProperty = RegisterProperty<string>(new PropertyInfo<string>("Notes", "Notes"));
        public string Notes
        {
            get
            {
                return GetProperty(NotesProperty);
            }
        }
        private static PropertyInfo<string> DOIProperty = RegisterProperty<string>(new PropertyInfo<string>("DOI", "DOI"));
        public string DOI
        {
            get
            {
                return GetProperty(DOIProperty);
            }
        }
        private static PropertyInfo<string> KeywordsProperty = RegisterProperty<string>(new PropertyInfo<string>("Keywords", "Keywords"));
        public string Keywords
        {
            get
            {
                return GetProperty(KeywordsProperty);
            }
        }
        private static PropertyInfo<int> DefaultTypeCodeProperty = RegisterProperty<int>(new PropertyInfo<int>("DefaultTypeCode", "DefaultTypeCode"));
        public int DefaultTypeCode
        {
            get
            {
                return GetProperty(DefaultTypeCodeProperty);
            }
        }
        #endregion
#if !SILVERLIGHT

        internal static ReadOnlyImportFilterRule GetReadOnlyImportFilterRule(SafeDataReader reader)
        {
            ReadOnlyImportFilterRule returnValue = new ReadOnlyImportFilterRule();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("select * from TB_IMPORT_FILTER_TYPE_MAP where IMPORT_FILTER_ID = " + reader.GetInt32("IMPORT_FILTER_ID"), connection))
                {
                    using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        MobileDictionary<int, string> tmpDic = new MobileDictionary<int, string>();
                        while (reader2.Read())
                        {
                            tmpDic.Add(
                                reader2.GetByte("TYPE_CODE"),
                                reader2.GetString("TYPE_REGEX")
                                );
                        }
                        reader2.Close();
                        returnValue.LoadProperty<MobileDictionary<int, string>>(typesMapProperty, tmpDic);
                    }
                }
                using (SqlCommand command = new SqlCommand("select * from TB_IMPORT_FILTER_TYPE_RULE where IMPORT_FILTER_ID = " + reader.GetInt32("IMPORT_FILTER_ID"), connection))
                {
                    using (Csla.Data.SafeDataReader reader3 = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        MobileList<TypeRules> tmpML = new MobileList<TypeRules>();
                        while (reader3.Read())
                        {
                            tmpML.Add(TypeRules.newTypeRule(
                                reader3.GetString("RULE_NAME"),
                                reader3.GetString("RULE_REGEX"),
                                reader3.GetByte("TYPE_CODE")
                                ));
                        }
                        reader3.Close();
                        returnValue.LoadProperty <MobileList<TypeRules>>(typeRulesProperty, tmpML);
                    }
                }
                connection.Close();
            }
            returnValue.LoadProperty<int>(FilterIDProperty, reader.GetInt32("IMPORT_FILTER_ID"));
            returnValue.LoadProperty<string>(RuleNameProperty, reader.GetString("IMPORT_FILTER_NAME"));
            returnValue.LoadProperty<string>(StartOfNewRecProperty, reader.GetString("STARTOFNEWREC"));
            returnValue.LoadProperty<string>(typeFieldProperty, reader.GetString("TYPEFIELD"));
            returnValue.LoadProperty<string>(StartOfNewFieldProperty, reader.GetString("STARTOFNEWFIELD"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
            returnValue.LoadProperty<string>(pTitleProperty, reader.GetString("PTITLE"));
            returnValue.LoadProperty<string>(shortTitleProperty, reader.GetString("SHORTTITLE"));
            returnValue.LoadProperty<string>(DateProperty, reader.GetString("DATE"));
            returnValue.LoadProperty<string>(MonthProperty, reader.GetString("MONTH"));
            returnValue.LoadProperty<string>(AuthorProperty, reader.GetString("AUTHOR"));
            returnValue.LoadProperty<string>(ParentAuthorProperty, reader.GetString("PARENTAUTHOR"));
            returnValue.LoadProperty<string>(StandardNProperty, reader.GetString("STANDARDN"));
            returnValue.LoadProperty<string>(CityProperty, reader.GetString("CITY"));
            returnValue.LoadProperty<string>(PublisherProperty, reader.GetString("PUBLISHER"));
            returnValue.LoadProperty<string>(InstitutionProperty, reader.GetString("INSTITUTION"));
            returnValue.LoadProperty<string>(VolumeProperty, reader.GetString("VOLUME"));
            returnValue.LoadProperty<string>(IssueProperty, reader.GetString("ISSUE"));
            returnValue.LoadProperty<string>(EditionProperty, reader.GetString("EDITION"));
            returnValue.LoadProperty<string>(StartPageProperty, reader.GetString("STARTPAGE"));
            returnValue.LoadProperty<string>(EndPageProperty, reader.GetString("ENDPAGE"));
            returnValue.LoadProperty<string>(PagesProperty, reader.GetString("PAGES"));
            returnValue.LoadProperty<string>(AvailabilityProperty, reader.GetString("AVAILABILITY"));
            returnValue.LoadProperty<string>(UrlProperty, reader.GetString("URL"));
            returnValue.LoadProperty<string>(AbstractProperty, reader.GetString("ABSTRACT"));
            returnValue.LoadProperty<string>(OldItemIdProperty, reader.GetString("OLD_ITEM_ID"));
            returnValue.LoadProperty<string>(NotesProperty, reader.GetString("NOTES"));
            returnValue.LoadProperty<string>(DOIProperty, reader.GetString("DOI"));
            returnValue.LoadProperty<string>(KeywordsProperty, reader.GetString("KEYWORDS"));
            returnValue.LoadProperty<int>(DefaultTypeCodeProperty, reader.GetByte("DEFAULTTYPECODE"));
            

            return returnValue;
        }

#endif
    }
    [Serializable]
    public class TypeRules : ReadOnlyBase<TypeRules>
    {
#if SILVERLIGHT
    public TypeRules()
    {
        
    }
#else
        private TypeRules()
        {
        }
        public static TypeRules newTypeRule(string rulename, string ruleregexst, int type_id)
        {
            TypeRules result = new TypeRules();
            result.LoadProperty<string>(RuleNameProperty, rulename);
            result.LoadProperty<string>(RuleRegexStProperty, ruleregexst);
            result.LoadProperty<int>(Type_IDProperty, type_id);
            return result;
        }
#endif
        private static PropertyInfo<string> RuleNameProperty = RegisterProperty<string>(new PropertyInfo<string>("RuleName", "RuleName"));
        public string RuleName
        {
            get
            {
                return GetProperty(RuleNameProperty);
            }
        }
        private static PropertyInfo<string> RuleRegexStProperty = RegisterProperty<string>(new PropertyInfo<string>("RuleRegexSt", "RuleRegexSt"));
        public string RuleRegexSt
        {
            get
            {
                return GetProperty(RuleRegexStProperty);
            }
        }
        private static PropertyInfo<int> Type_IDProperty = RegisterProperty<int>(new PropertyInfo<int>("Type_ID", "Type_ID"));
        public int Type_ID
        {
            get
            {
                return GetProperty(Type_IDProperty);
            }
        }
        
        
    }
}
