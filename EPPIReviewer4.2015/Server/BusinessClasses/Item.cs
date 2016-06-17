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
using Csla.DataPortalClient;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using AuthorsHandling;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class Item : BusinessBase<Item>
    {
#if SILVERLIGHT
    public Item() { }

        
#else
        private Item() { }
#endif

        public override string ToString()
        {
            return Title;
        }

        internal static Item NewItem()
        {
            Item returnValue = new Item();
            //returnValue.ValidationRules.CheckRules();
            return returnValue;
        }

        public static void GetItem(Int64 Id, EventHandler<DataPortalResult<Item>> handler)
        {
            DataPortal<Item> dp = new DataPortal<Item>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<Item, Int64>(Id));
        }

        /*
        public string GetOldCitation()
        {
            string retVal = "";
            switch (this.TypeId)
            {
                case 1: //Report
                    retVal = CleanAuthors(Authors) + " (" + Year + ") <i>" + Title + "</i>. " + City + ": " + Publisher + ". " + URL;
                    break;
                case 2: //Book, Whole
                    retVal = CleanAuthors(Authors) + " (" + Year + ") <i>" + Title + "</i>. " + City + ": " + Publisher + ".";
                    break;
                case 3: //Book, Chapter
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". In: " + CleanAuthors(ParentAuthors) + " <i>" + ParentTitle + ".</i> " + City + ": " + Publisher + ", pages " + Pages + ".";
                    break;
                case 4: //Dissertation
                    retVal = CleanAuthors(Authors) + " (" + Year + ") <i>" + Title + "</i>. " + Edition + ": " + Institution + ".";
                    break;
                case 5: //Conference Proceedings
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + CleanAuthors(ParentTitle) + ", " + City + ".";
                    break;
                case 6: //Document From Internet Site
                    retVal = CleanAuthors(Authors) + " (" + Year + ") <i>" + Title + "</i>. " + URL;
                    break;
                case 7: //Web Site
                    retVal = CleanAuthors(Authors) + " (" + Year + ") <i>" + Title + "</i>. " + URL;
                    break;
                case 8: //DVD, Video, Media
                    retVal = CleanAuthors(Authors) + " (" + Year + ") <i>" + Title + "</i>. " + City + ": " + Publisher + ".";
                    break;
                case 9: //Research project
                    retVal = CleanAuthors(Authors) + " (" + Year + ") <i>" + Title + "</i>. " + City + ": " + Publisher + ".";
                    break;
                case 10: //Article In A Periodical
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". <i>" + CleanAuthors(ParentTitle) + "</i>. " + Volume + (Issue != "" ? "(" + Issue + ")" : "") + ": " + Pages + ".";
                    break;
                case 11: //Interview
                    retVal = CleanAuthors(Authors) + " (" + Year + ") <i>" + Title + "</i>. " + City + ": " + Publisher + ".";
                    break;
                case 12: //Generic
                    retVal = CleanAuthors(Authors) + " (" + Year + ") <i>" + Title + "</i>. " + City + ": " + Publisher + ".";
                    break;
                case 14: //Journal, Article
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". <i>" + CleanAuthors(ParentTitle) + "</i>. " + Volume + (Issue != "" ? "(" + Issue + ")" : "") + ": " + Pages + ".";
                    break;
            }
            return retVal;
        }
        */

        private string CleanAuthors(string inputAuthors)
        {
            if (inputAuthors != "")
            {
                inputAuthors = inputAuthors.Replace(" ;", ",");
                inputAuthors = inputAuthors.Replace(";", ",");
                inputAuthors = inputAuthors.Trim();
                inputAuthors = inputAuthors.TrimEnd(',');
            }
            int commaCount = 0;
            for (int i = 0; i < inputAuthors.Length; i++) if (inputAuthors[i] == ',') commaCount++;
            if (commaCount > 0)
            {
                inputAuthors = inputAuthors.Insert(inputAuthors.LastIndexOf(",") + 1, " and");
            }
            return inputAuthors;
        }

        public string GetCitation()
        {
            string retVal = "";
            switch (this.TypeId)
            {
                case 1: //Report
                    retVal = CleanAuthors(Authors) + ". " + Year + ". <i>" + Title + "</i>. " + City + ": " + Publisher + ". ";
                    break;
                case 2: //Book, Whole
                    retVal = CleanAuthors(Authors) + ". " + Year + ". <i>" + Title + "</i>. " + City + ": " + Publisher + ".";
                    break;
                case 3: //Book, Chapter
                    retVal = CleanAuthors(Authors) + ". " + Year + ". " + Title + ". In <i>" + ParentTitle + "</i>, edited by " + CleanAuthors(ParentAuthors) + ", " +
                        Pages + ". " + City + ": " + Publisher + ".";
                    break;
                case 4: //Dissertation
                    retVal = CleanAuthors(Authors) + ". " + Year + ". \"" + Title + "\". " + Edition + ", " + Institution + ".";
                    break;
                case 5: //Conference Proceedings
                    retVal = CleanAuthors(Authors) + ". " + Year + ". " + Title + ". Paper presented at " + ParentTitle + ", " + City + ": " + Publisher + ".";
                    break;
                case 6: //Document From Internet Site
                    retVal = CleanAuthors(Authors) + ". " + Year + ". \"" + Title + "\". " + Publisher + ". " + URL +
                        (Availability == "" ? "" : " [Accessed " + Availability + "] ") + ".";
                    break;
                case 7: //Web Site
                    retVal = CleanAuthors(Authors) + ". (" + Year + "). <i>" + Title + "</i>. " + Publisher + ". " + URL +
                        (Availability == "" ? "" : " [Accessed " + Availability + "] ") + ".";
                    break;
                case 8: //DVD, Video, Media
                    retVal = "\"" + Title + "\". " + Year + ". " + (Availability == "" ? "" : " [" + Availability + "] ") +
                        City + ": " + CleanAuthors(Authors) + ".";
                    break;
                case 9: //Research project
                    retVal = CleanAuthors(Authors) + ". " + Year + ". \"" + Title + "\". " + City + ": " + Publisher + ".";
                    break;
                case 10: //Article In A Periodical
                    retVal = CleanAuthors(Authors) + ". " + Year + ". \"" + Title + "\". <i>" + CleanAuthors(ParentTitle) + "</i> " + Volume + (Issue != "" ? "(" + Issue + ")" : "") + ":" + Pages + ".";
                    break;
                case 11: //Interview
                    retVal = CleanAuthors(Authors) + ". " + Year + ". \"" + Title + "\". ";
                    break;
                case 12: //Generic
                    retVal = CleanAuthors(Authors) + ". " + Year + ". \"" + Title + "\". " + City + ": " + Publisher + ".";
                    break;
                case 14: //Journal, Article
                    retVal = CleanAuthors(Authors) + ". " + Year + ". \"" + Title + "\". <i>" + CleanAuthors(ParentTitle) + "</i> " + Volume + (Issue != "" ? "(" + Issue + ")" : "") + ":" + Pages + ".";
                    break;
            }
            return retVal;
        }

        public string GetHarvardCitation()
        {
            string retVal = "";
            switch (this.TypeId)
            {
                case 1: //Report
                    retVal = CleanAuthors(Authors) + ". (" + Year + "). <i>" + Title + "</i>. " + City + ": " + Publisher + ", pp." + Pages + ". " +
                        (URL == "" ? "" : "Available at: ") + URL + ".";
                    break;
                case 2: //Book, Whole
                    retVal = CleanAuthors(Authors) + ". (" + Year + "). <i>" + Title + "</i>. " + City + ": " + Publisher + ".";
                    break;
                case 3: //Book, Chapter
                    retVal = CleanAuthors(Authors) + ". (" + Year + "). " + Title + ". In: " + CleanAuthors(ParentAuthors) + ", ed., <i>" + ParentTitle + ".</i> " + City + ": " + Publisher + ", pp." + Pages + ".";
                    break;
                case 4: //Dissertation
                    retVal = CleanAuthors(Authors) + ". (" + Year + "). <i>" + Title + "</i>. " + Edition + ". " + Institution + ".";
                    break;
                case 5: //Conference Proceedings
                    retVal = CleanAuthors(Authors) + ". (" + Year + "). " + Title + ". In: " + ParentTitle + ". " + City + ": " + Publisher + ", pp." + Pages + ". " +
                        (URL == "" ? "" : "Available at: ") + URL + ".";
                    break;
                case 6: //Document From Internet Site
                    retVal = CleanAuthors(Authors) + ". (" + Year + "). <i>" + Title + "</i>. [online] " + Publisher + ". Available at: " + URL +
                        (Availability == "" ? "" : " [Accessed: " + Availability + "] ") + ".";
                    break;
                case 7: //Web Site
                    retVal = CleanAuthors(Authors) + ". (" + Year + "). <i>" + Title + "</i>. [online] " + Publisher + ". Available at: " + URL +
                        (Availability == "" ? "" : " [Accessed: " + Availability + "] ") + ".";
                    break;
                case 8: //DVD, Video, Media
                    retVal = "<i>" + Title + "</i>. " + " (" + Year + "). " + (Availability == "" ? "" : " [" + Availability + "] ") +
                        City + ": " + CleanAuthors(Authors) + ".";
                    break;
                case 9: //Research project
                    retVal = CleanAuthors(Authors) + ". (" + Year + "). <i>" + Title + "</i>. " + City + ": " + Publisher + ".";
                    break;
                case 10: //Article In A Periodical
                    retVal = CleanAuthors(Authors) + ". (" + Year + "). " + Title + ". <i>" + CleanAuthors(ParentTitle) + "</i>, " + Volume + (Issue != "" ? "(" + Issue + ")" : "") + ", pp." + Pages + ".";
                    break;
                case 11: //Interview
                    retVal = CleanAuthors(Authors) + ". (" + Year + "). <i>" + Title + "</i>. ";
                    break;
                case 12: //Generic
                    retVal = CleanAuthors(Authors) + ". (" + Year + "). <i>" + Title + "</i>. " + City + ": " + Publisher + ".";
                    break;
                case 14: //Journal, Article
                    retVal = CleanAuthors(Authors) + ". (" + Year + "). " + Title + ". <i>" + CleanAuthors(ParentTitle) + "</i>, " + Volume + (Issue != "" ? "(" + Issue + ")" : "") + ", pp." + Pages + ".";
                    break;
            }
            return retVal;
        }

        public string GetBritishLibraryCitation()
        {
            string retVal = "";
            switch (this.TypeId)
            {
                case 1: //Report (BL 'book' style)
                    retVal = Title + Environment.NewLine +
                        CleanAuthors(Authors) + Environment.NewLine +
                        PublisherAndPlace(Publisher, City) +
                        Year;
                    break;
                case 2: //Report (BL 'book' style)
                    retVal = Title + Environment.NewLine +
                        CleanAuthors(Authors) + Environment.NewLine +
                        PublisherAndPlace(Publisher, City) +
                        Year;
                    break;
                case 3: //Report (BL 'book' style)
                    retVal = Title + Environment.NewLine +
                        CleanAuthors(Authors) + Environment.NewLine +
                        PublisherAndPlace(Publisher, City) +
                        Year;
                    break;
                case 4: //Report (BL 'book' style)
                    retVal = Title + Environment.NewLine +
                        CleanAuthors(Authors) + Environment.NewLine +
                        PublisherAndPlace(Publisher, City) +
                        Year;
                    break;
                case 5: // Conference paper
                    retVal = ParentTitle + ": " + Environment.NewLine +
                        Publisher + Environment.NewLine +
                        Year + (Pages != "" ? " PP" + Pages : "") + Environment.NewLine +
                        Title;
                    break;
                case 6: //Document From Internet Site
                    retVal = Title + Environment.NewLine +
                        CleanAuthors(Authors) + Environment.NewLine +
                        PublisherAndPlace(Publisher, City) +
                        Year;
                    break;
                case 7: //Web Site
                    retVal = Title + Environment.NewLine +
                        CleanAuthors(Authors) + Environment.NewLine +
                        PublisherAndPlace(Publisher, City) +
                        Year;
                    break;
                case 8: //DVD, Video, Media
                    retVal = Title + Environment.NewLine +
                        CleanAuthors(Authors) + Environment.NewLine +
                        PublisherAndPlace(Publisher, City) +
                        Year;
                    break;
                case 9: //Research project
                    retVal = Title + Environment.NewLine +
                        CleanAuthors(Authors) + Environment.NewLine +
                        PublisherAndPlace(Publisher, City) +
                        Year;
                    break;
                case 10: //Article In A Periodical
                    retVal = CleanAuthors(ParentTitle) + Environment.NewLine +
                        Year + (Volume != "" ? " VOL " + Volume : "") + 
                        (Issue != "" ? " PT " + Issue : "") +
                        (Pages != "" ?  " PP " + Pages : "") + Environment.NewLine +
                        Title + Environment.NewLine +
                        CleanAuthors(Authors);
                    break;
                case 11: //Interview
                    retVal = Title + Environment.NewLine +
                        CleanAuthors(Authors) + Environment.NewLine +
                        PublisherAndPlace(Publisher, City) +
                        Year;
                    break;
                case 12: //Generic
                    retVal = Title + Environment.NewLine +
                        CleanAuthors(Authors) + Environment.NewLine +
                        PublisherAndPlace(Publisher, City) +
                        Year;
                    break;
                case 14: //Journal, Article
                    retVal = CleanAuthors(ParentTitle) + Environment.NewLine +
                        Year + (Volume != "" ? " VOL " + Volume : "") +
                        (Issue != "" ? " PT " + Issue : "") +
                        (Pages != "" ? " PP " + Pages : "") + Environment.NewLine +
                        Title + Environment.NewLine +
                        CleanAuthors(Authors);
                    break;
            }
            return retVal;
        }

        private string PublisherAndPlace(string pub, string pl)
        {
            string retval = pub;
            if (pl != "")
            {
                if (retval == "")
                    retval = pl;
                else
                    retval += ", " + pl;
            }
            if (retval != "")
                retval += Environment.NewLine;
            return retval;
        }

        /*
        public void GetDocumentList()
        {
            DataPortal<ItemDocumentList> dp = new DataPortal<ItemDocumentList>();
            dp.FetchCompleted += (o, e2) =>
            {
                if (e2.Object != null)
                {
                    if (e2.Error == null)
                    {
                        this.Documents = e2.Object;
                        //this.MarkClean(); // don't want the object marked as 'dirty' just because it's loaded its documents.
                    }
                }
                if (e2.Error != null)
                {
                    string eMsg = "Detail ";
                    //foreach (System.Collections.DictionaryEntry de in e2.Error.Data)
                    //    eMsg += String.Format("    The key is '{0}' and the value is: {1}",
                    //                                        de.Key, de.Value) + "\n";
                    if (e2.Error.Message.IndexOf("[Async_ExceptionOccurred]") > -1
                        & e2.Error.InnerException.Message.IndexOf("[Xml_InvalidCharacter]") > -1)
                        eMsg = "FAILED TO LOAD DOCUMENTS LIST: \nOne of the Documents appear to contain some illegal characters.\nPlease contact Support to fix this issue.";
                    else eMsg += "FAILED TO LOAD DOCUMENTS LIST: " + e2.Error.Message + " INNER EXC: " + e2.Error.InnerException.Message;
#if SILVERLIGHT
                    System.Windows.MessageBox.Show(eMsg);
#endif
                }
            };
            dp.BeginFetch(new SingleCriteria<ItemDocumentList, Int64>(this.ItemId));
        }
        */

        private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
            set
            {
                SetProperty(ItemIdProperty, value);
            }
        }
        private static PropertyInfo<Int64> MasterItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("MasterItemId", "MasterItemId"));
        public Int64 MasterItemId//is zero if no master
        {
            get
            {
                return GetProperty(MasterItemIdProperty);
            }
            set
            {
                SetProperty(MasterItemIdProperty, value);
            }
        }
        public bool IsDupilcate
        {
            get
            {
                return IsIncluded & IsItemDeleted & (MasterItemId > 0);
            }
        }
        private static PropertyInfo<int> TypeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("TypeId", "TypeId"));
        public int TypeId
        {
            get
            {
                return GetProperty(TypeIdProperty);
            }
            set
            {
                SetProperty(TypeIdProperty, value);
            }
        }

        private static PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title", string.Empty));
        public string Title
        {
            get
            {
                return GetProperty(TitleProperty);
            }
            set
            {
                SetProperty(TitleProperty, value);
            }
        }

        private static PropertyInfo<string> ParentTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ParentTitle", "ParentTitle", string.Empty));
        public string ParentTitle
        {
            get
            {
                return GetProperty(ParentTitleProperty);
            }
            set
            {
                SetProperty(ParentTitleProperty, value);
            }
        }

        private static PropertyInfo<string> ShortTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ShortTitle", "ShortTitle", string.Empty));
        public string ShortTitle
        {
            get
            {
                return GetProperty(ShortTitleProperty);
            }
            set
            {
                SetProperty(ShortTitleProperty, value);
            }
        }

        private static PropertyInfo<SmartDate> DateCreatedProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("DateCreated", "DateCreated"));
        public SmartDate DateCreated
        {
            get
            {
                return GetProperty(DateCreatedProperty);
            }
            set
            {
                SetProperty(DateCreatedProperty, value);
            }
        }

        private static PropertyInfo<string> CreatedByProperty = RegisterProperty<string>(new PropertyInfo<string>("CreatedBy", "CreatedBy", string.Empty));
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

        private static PropertyInfo<SmartDate> DateEditedProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("DateEdited", "DateEdited"));
        public SmartDate DateEdited
        {
            get
            {
                return GetProperty(DateEditedProperty);
            }
            set
            {
                SetProperty(DateEditedProperty, value);
            }
        }

        private static PropertyInfo<string> EditedByProperty = RegisterProperty<string>(new PropertyInfo<string>("EditedBy", "EditedBy", string.Empty));
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

        private static PropertyInfo<string> YearProperty = RegisterProperty<string>(new PropertyInfo<string>("Year", "Year", string.Empty));
        public string Year
        {
            get
            {
                return GetProperty(YearProperty);
            }
            set
            {
                SetProperty(YearProperty, value);
            }
        }

        private static PropertyInfo<string> MonthProperty = RegisterProperty<string>(new PropertyInfo<string>("Month", "Month", string.Empty));
        public string Month
        {
            get
            {
                return GetProperty(MonthProperty);
            }
            set
            {
                SetProperty(MonthProperty, value);
            }
        }

        private static PropertyInfo<string> StandardNumberProperty = RegisterProperty<string>(new PropertyInfo<string>("StandardNumber", "StandardNumber", string.Empty));
        public string StandardNumber
        {
            get
            {
                return GetProperty(StandardNumberProperty);
            }
            set
            {
                SetProperty(StandardNumberProperty, value);
            }
        }

        private static PropertyInfo<string> CityProperty = RegisterProperty<string>(new PropertyInfo<string>("City", "City", string.Empty));
        public string City
        {
            get
            {
                return GetProperty(CityProperty);
            }
            set
            {
                SetProperty(CityProperty, value);
            }
        }

        private static PropertyInfo<string> CountryProperty = RegisterProperty<string>(new PropertyInfo<string>("Country", "Country", string.Empty));
        public string Country
        {
            get
            {
                return GetProperty(CountryProperty);
            }
            set
            {
                SetProperty(CountryProperty, value);
            }
        }

        private static PropertyInfo<string> PublisherProperty = RegisterProperty<string>(new PropertyInfo<string>("Publisher", "Publisher", string.Empty));
        public string Publisher
        {
            get
            {
                return GetProperty(PublisherProperty);
            }
            set
            {
                SetProperty(PublisherProperty, value);
            }
        }

        private static PropertyInfo<string> InstitutionProperty = RegisterProperty<string>(new PropertyInfo<string>("Institution", "Institution", string.Empty));
        public string Institution
        {
            get
            {
                return GetProperty(InstitutionProperty);
            }
            set
            {
                SetProperty(InstitutionProperty, value);
            }
        }

        private static PropertyInfo<string> VolumeProperty = RegisterProperty<string>(new PropertyInfo<string>("Volume", "Volume", string.Empty));
        public string Volume
        {
            get
            {
                return GetProperty(VolumeProperty);
            }
            set
            {
                SetProperty(VolumeProperty, value);
            }
        }

        private static PropertyInfo<string> PagesProperty = RegisterProperty<string>(new PropertyInfo<string>("Pages", "Pages", string.Empty));
        public string Pages
        {
            get
            {
                return GetProperty(PagesProperty);
            }
            set
            {
                SetProperty(PagesProperty, value);
            }
        }

        private static PropertyInfo<string> EditionProperty = RegisterProperty<string>(new PropertyInfo<string>("Edition", "Edition", string.Empty));
        public string Edition
        {
            get
            {
                return GetProperty(EditionProperty);
            }
            set
            {
                SetProperty(EditionProperty, value);
            }
        }

        private static PropertyInfo<string> IssueProperty = RegisterProperty<string>(new PropertyInfo<string>("Issue", "Issue", string.Empty));
        public string Issue
        {
            get
            {
                return GetProperty(IssueProperty);
            }
            set
            {
                SetProperty(IssueProperty, value);
            }
        }

        private static PropertyInfo<bool> IsLocalProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsLocal", "IsLocal", false));
        public bool IsLocal
        {
            get
            {
                return GetProperty(IsLocalProperty);
            }
            set
            {
                SetProperty(IsLocalProperty, value);
            }
        }

        private static PropertyInfo<string> AvailabilityProperty = RegisterProperty<string>(new PropertyInfo<string>("Availability", "Availability", string.Empty));
        public string Availability
        {
            get
            {
                return GetProperty(AvailabilityProperty);
            }
            set
            {
                SetProperty(AvailabilityProperty, value);
            }
        }

        private static PropertyInfo<string> URLProperty = RegisterProperty<string>(new PropertyInfo<string>("URL", "URL", string.Empty));
        public string URL
        {
            get
            {
                return GetProperty(URLProperty);
            }
            set
            {
                SetProperty(URLProperty, value);
            }
        }

        private static PropertyInfo<string> OldItemIdProperty = RegisterProperty<string>(new PropertyInfo<string>("OldItemId", "OldItemId", string.Empty));
        public string OldItemId
        {
            get
            {
                return GetProperty(OldItemIdProperty);
            }
        }

        

        private static PropertyInfo<string> AbstractProperty = RegisterProperty<string>(new PropertyInfo<string>("Abstract", "Abstract", string.Empty));
        public string Abstract
        {
            get
            {
                return GetProperty(AbstractProperty);
            }
            set
            {
                SetProperty(AbstractProperty, value);
            }
        }

        private static PropertyInfo<string> CommentsProperty = RegisterProperty<string>(new PropertyInfo<string>("Comments", "Comments", string.Empty));
        public string Comments
        {
            get
            {
                return GetProperty(CommentsProperty);
            }
            set
            {
                SetProperty(CommentsProperty, value);
            }
        }

        private static PropertyInfo<string> TypeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("TypeName", "TypeName", string.Empty));
        public string TypeName
        {
            get
            {
                return GetProperty(TypeNameProperty);
            }
            set
            {
                SetProperty(TypeNameProperty, value);
            }
        }

        private static PropertyInfo<string> AuthorsProperty = RegisterProperty<string>(new PropertyInfo<string>("Authors", "Authors", string.Empty));
        public string Authors
        {
            get
            {
                return GetProperty(AuthorsProperty);
            }
            set
            {
                SetProperty(AuthorsProperty, value);
            }
        }
        private static PropertyInfo<string> ParentAuthorsProperty = RegisterProperty<string>(new PropertyInfo<string>("ParentAuthors", "ParentAuthors", string.Empty));
        public string ParentAuthors
        {
            get
            {
                return GetProperty(ParentAuthorsProperty);
            }
            set
            {
                SetProperty(ParentAuthorsProperty, value);
            }
        }
        private static PropertyInfo<string> DOIProperty = RegisterProperty<string>(new PropertyInfo<string>("DOI", "DOI", string.Empty));
        public string DOI
        {
            get
            {
                return GetProperty(DOIProperty);
            }
            set
            {
                SetProperty(DOIProperty, value);
            }
        }
        private static PropertyInfo<string> KeywordsProperty = RegisterProperty<string>(new PropertyInfo<string>("Keywords", "Keywords", string.Empty));
        public string Keywords
        {
            get
            {
                return GetProperty(KeywordsProperty);
            }
            set
            {
                SetProperty(KeywordsProperty, value);
            }
        }
        /*
        [NotUndoable]
        private static PropertyInfo<ItemDocumentList> DocumentsProperty = RegisterProperty<ItemDocumentList>(new PropertyInfo<ItemDocumentList>("Documents", "Documents"));
        public ItemDocumentList Documents
        {
            get
            {
                return GetProperty(DocumentsProperty);
            }
            set
            {
                SetProperty(DocumentsProperty, value);
            }
        }
        */

        //public ItemDocumentList Documents { get; set; }

        private static PropertyInfo<bool> IsItemDeletedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsItemDeleted", "IsItemDeleted"));
        public bool IsItemDeleted
        {
            get
            {
                return GetProperty(IsItemDeletedProperty);
            }
            set
            {
                SetProperty(IsItemDeletedProperty, value);
                SetItemStatusProperty();
            }
        }

        private static PropertyInfo<bool> IsIncludedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsIncluded", "IsIncluded"));
        public bool IsIncluded
        {
            get
            {
                return GetProperty(IsIncludedProperty);
            }
            set
            {
                SetProperty(IsIncludedProperty, value);
                SetItemStatusProperty();
            }
        }

        private static PropertyInfo<bool> IsSelectedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsSelected", "IsSelected", false));
        public bool IsSelected
        {
            get
            {
                return GetProperty(IsSelectedProperty);
            }
            set
            {
                SetProperty(IsSelectedProperty, value);
            }
        }

        private void SetItemStatusProperty()
        {
            if (IsIncluded == true && IsItemDeleted == false)
            {
                SetProperty(ItemStatusProperty, "I");
                SetProperty(ItemStatusTooltipProperty, "Included in review");
            }
            else if (IsIncluded == false && IsItemDeleted == false)
            {
                SetProperty(ItemStatusProperty, "E");
                SetProperty(ItemStatusTooltipProperty, "Excluded from review");
            }
            else if (IsItemDeleted == true && IsIncluded == false)
            {
                SetProperty(ItemStatusProperty, "D");
                SetProperty(ItemStatusTooltipProperty, "Deleted");
            }
            else
            {
                SetProperty(ItemStatusProperty, "S");
                SetProperty(ItemStatusTooltipProperty, "Shadow: this item is a duplicate or part of a deleted source");
            }
        }

        private static PropertyInfo<string> ItemStatusProperty = RegisterProperty<string>(new PropertyInfo<string>("ItemStatus", "ItemStatus"));
        public string ItemStatus
        {
            get
            {
                return GetProperty(ItemStatusProperty);
            }
            set
            {
                if (IsItemDeleted == true)
                    SetProperty(ItemStatusProperty, "D");
                else
                    if (IsIncluded == true) SetProperty(ItemStatusProperty, "I");
                    else SetProperty(ItemStatusProperty, "E");
            }
        }

        private static PropertyInfo<string> ItemStatusTooltipProperty = RegisterProperty<string>(new PropertyInfo<string>("ItemStatusTooltip", "ItemStatusTooltip"));
        public string ItemStatusTooltip
        {
            get
            {
                return GetProperty(ItemStatusTooltipProperty);
            }
            set
            {
                if (IsItemDeleted == true)
                    SetProperty(ItemStatusTooltipProperty, "Deleted");
                else
                    if (IsIncluded == true) SetProperty(ItemStatusTooltipProperty, "Included in review");
                    else SetProperty(ItemStatusTooltipProperty, "Excluded from review");
            }
        }



        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(Item), admin);
        //    //AuthorizationRules.AllowDelete(typeof(Item), admin);
        //    //AuthorizationRules.AllowEdit(typeof(Item), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(Item), canRead);

        //    //AuthorizationRules.AllowRead(ItemIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(TitleProperty, canRead);

        //    //AuthorizationRules.AllowWrite(TitleProperty, canWrite);
        //}

        protected override void AddBusinessRules()
        {
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT

        protected override void DataPortal_Insert()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                SetShortTitle();
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", ReadProperty(ItemIdProperty)));
                    command.Parameters.Add(new SqlParameter("@TITLE", ReadProperty(TitleProperty)));
                    command.Parameters.Add(new SqlParameter("@TYPE_ID", ReadProperty(TypeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@PARENT_TITLE", ReadProperty(ParentTitleProperty)));
                    command.Parameters.Add(new SqlParameter("@SHORT_TITLE", ReadProperty(ShortTitleProperty)));
                    command.Parameters.Add(new SqlParameter("@DATE_CREATED", ReadProperty(DateCreatedProperty).DBValue));
                    command.Parameters.Add(new SqlParameter("@CREATED_BY", ReadProperty(CreatedByProperty)));
                    command.Parameters.Add(new SqlParameter("@DATE_EDITED", ReadProperty(DateEditedProperty).DBValue));
                    command.Parameters.Add(new SqlParameter("@EDITED_BY", ReadProperty(EditedByProperty)));
                    command.Parameters.Add(new SqlParameter("@YEAR", ReadProperty(YearProperty)));
                    command.Parameters.Add(new SqlParameter("@MONTH", ReadProperty(MonthProperty)));
                    command.Parameters.Add(new SqlParameter("@STANDARD_NUMBER", ReadProperty(StandardNumberProperty)));
                    command.Parameters.Add(new SqlParameter("@CITY", ReadProperty(CityProperty)));
                    command.Parameters.Add(new SqlParameter("@COUNTRY", ReadProperty(CountryProperty)));
                    command.Parameters.Add(new SqlParameter("@PUBLISHER", ReadProperty(PublisherProperty)));
                    command.Parameters.Add(new SqlParameter("@INSTITUTION", ReadProperty(InstitutionProperty)));
                    command.Parameters.Add(new SqlParameter("@VOLUME", ReadProperty(VolumeProperty)));
                    command.Parameters.Add(new SqlParameter("@PAGES", ReadProperty(PagesProperty)));
                    command.Parameters.Add(new SqlParameter("@EDITION", ReadProperty(EditionProperty)));
                    command.Parameters.Add(new SqlParameter("@ISSUE", ReadProperty(IssueProperty)));
                    command.Parameters.Add(new SqlParameter("@IS_LOCAL", ReadProperty(IsLocalProperty)));
                    command.Parameters.Add(new SqlParameter("@AVAILABILITY", ReadProperty(AvailabilityProperty)));
                    command.Parameters.Add(new SqlParameter("@URL", ReadProperty(URLProperty)));
                    command.Parameters.Add(new SqlParameter("@COMMENTS", ReadProperty(CommentsProperty)));
                    command.Parameters.Add(new SqlParameter("@ABSTRACT", ReadProperty(AbstractProperty)));
                    command.Parameters.Add(new SqlParameter("@DOI", ReadProperty(DOIProperty)));
                    command.Parameters.Add(new SqlParameter("@KEYWORDS", ReadProperty(KeywordsProperty)));
                    command.Parameters.Add(new SqlParameter("@IS_INCLUDED", ReadProperty(IsIncludedProperty)));
                    command.Parameters["@ITEM_ID"].Direction = System.Data.ParameterDirection.Output;
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));

                    command.ExecuteNonQuery();
                    LoadProperty(ItemIdProperty, command.Parameters["@ITEM_ID"].Value);

                    SaveAuthors(connection);
                }
                connection.Close();
            }
        }

        protected void SetShortTitle()
        {
            if (ShortTitle == "")
            {
                if (Authors == "")
                {
                    ShortTitle = "Set short title";
                }
                else
                {
                    ShortTitle = Authors.Substring(0, Authors.IndexOf(" ") > 1 ? Authors.IndexOf(" ") : Math.Min(Authors.Length, 10)) + " (" + Year + ")";
                }
            }
        }

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                SetShortTitle();
                using (SqlCommand command = new SqlCommand("st_ItemUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", ReadProperty(ItemIdProperty)));
                    command.Parameters.Add(new SqlParameter("@TITLE", ReadProperty(TitleProperty)));
                    command.Parameters.Add(new SqlParameter("@TYPE_ID", ReadProperty(TypeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@PARENT_TITLE", ReadProperty(ParentTitleProperty)));
                    command.Parameters.Add(new SqlParameter("@SHORT_TITLE", ReadProperty(ShortTitleProperty)));
                    command.Parameters.Add(new SqlParameter("@DATE_CREATED", ReadProperty(DateCreatedProperty).DBValue));
                    command.Parameters.Add(new SqlParameter("@CREATED_BY", ReadProperty(CreatedByProperty)));
                    command.Parameters.Add(new SqlParameter("@DATE_EDITED", ReadProperty(DateEditedProperty).DBValue));
                    command.Parameters.Add(new SqlParameter("@EDITED_BY", ReadProperty(EditedByProperty)));
                    command.Parameters.Add(new SqlParameter("@YEAR", ReadProperty(YearProperty)));
                    command.Parameters.Add(new SqlParameter("@MONTH", ReadProperty(MonthProperty)));
                    command.Parameters.Add(new SqlParameter("@STANDARD_NUMBER", ReadProperty(StandardNumberProperty)));
                    command.Parameters.Add(new SqlParameter("@CITY", ReadProperty(CityProperty)));
                    command.Parameters.Add(new SqlParameter("@COUNTRY", ReadProperty(CountryProperty)));
                    command.Parameters.Add(new SqlParameter("@PUBLISHER", ReadProperty(PublisherProperty)));
                    command.Parameters.Add(new SqlParameter("@INSTITUTION", ReadProperty(InstitutionProperty)));
                    command.Parameters.Add(new SqlParameter("@VOLUME", ReadProperty(VolumeProperty)));
                    command.Parameters.Add(new SqlParameter("@PAGES", ReadProperty(PagesProperty)));
                    command.Parameters.Add(new SqlParameter("@EDITION", ReadProperty(EditionProperty)));
                    command.Parameters.Add(new SqlParameter("@ISSUE", ReadProperty(IssueProperty)));
                    command.Parameters.Add(new SqlParameter("@IS_LOCAL", ReadProperty(IsLocalProperty)));
                    command.Parameters.Add(new SqlParameter("@AVAILABILITY", ReadProperty(AvailabilityProperty)));
                    command.Parameters.Add(new SqlParameter("@URL", ReadProperty(URLProperty)));
                    command.Parameters.Add(new SqlParameter("@COMMENTS", ReadProperty(CommentsProperty)));
                    command.Parameters.Add(new SqlParameter("@ABSTRACT", ReadProperty(AbstractProperty)));
                    command.Parameters.Add(new SqlParameter("@IS_INCLUDED", ReadProperty(IsIncludedProperty)));
                    command.Parameters.Add(new SqlParameter("@DOI", ReadProperty(DOIProperty)));
                    command.Parameters.Add(new SqlParameter("@KEYWORDS", ReadProperty(KeywordsProperty)));

                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.ExecuteNonQuery();
                    SaveAuthors(connection);
                }
                connection.Close();
            }
        }

        protected void SaveAuthors(SqlConnection connection)
        {
            MobileList<AutH> AuthorLi = new MobileList<AutH>();
            if (Authors != null && Authors.Length > 0) AuthorLi.AddRange(NormaliseAuth.processField(ReadProperty(AuthorsProperty), 0));
            if (ParentAuthors != null && ParentAuthors.Length > 0) AuthorLi.AddRange(NormaliseAuth.processField(ReadProperty(ParentAuthorsProperty), 1));
            using (SqlCommand command = new SqlCommand("st_ItemAuthorDelete", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@ITEM_ID", ReadProperty(ItemIdProperty)));
                command.ExecuteNonQuery();
            }
            foreach (AutH a in AuthorLi)
            {
                if (a.LastName == "") continue;
                using (SqlCommand command = new SqlCommand("st_ItemAuthorUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", ReadProperty(ItemIdProperty)));
                    command.Parameters.Add(new SqlParameter("@RANK", a.Rank));
                    command.Parameters.Add(new SqlParameter("@ROLE", a.Role));
                    command.Parameters.Add(new SqlParameter("@LAST", a.LastName));
                    command.Parameters.Add(new SqlParameter("@FIRST", a.FirstName));
                    command.Parameters.Add(new SqlParameter("@SECOND", a.MiddleName));
                    command.ExecuteNonQuery();
                }
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", ReadProperty(ItemIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
             */
        }

        protected void DataPortal_Fetch(SingleCriteria<Item, Int64> criteria) // used to return a specific item
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_Item", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
                            LoadProperty<Int64>(MasterItemIdProperty, reader.GetInt64("MASTER_ITEM_ID"));
                            LoadProperty<int>(TypeIdProperty, reader.GetInt32("TYPE_ID"));
                            LoadProperty<string>(AuthorsProperty, reader.GetString("AUTHORS"));
                            LoadProperty<string>(ParentAuthorsProperty, reader.GetString("PARENTAUTHORS"));
                            LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
                            LoadProperty<string>(ParentTitleProperty, reader.GetString("PARENT_TITLE"));
                            LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
                            LoadProperty<SmartDate>(DateCreatedProperty, reader.GetSmartDate("DATE_CREATED"));
                            //LoadProperty<string>(CreatedByProperty, reader.GetString("CREATED_BY"));
                            LoadProperty<SmartDate>(DateEditedProperty, reader.GetSmartDate("DATE_EDITED"));
                            //LoadProperty<string>(EditedByProperty, reader.GetString("EDITED_BY"));
                            LoadProperty<string>(YearProperty, reader.GetString("YEAR"));
                            LoadProperty<string>(MonthProperty, reader.GetString("MONTH"));
                            LoadProperty<string>(StandardNumberProperty, reader.GetString("STANDARD_NUMBER"));
                            LoadProperty<string>(CityProperty, reader.GetString("CITY"));
                            LoadProperty<string>(CountryProperty, reader.GetString("COUNTRY"));
                            LoadProperty<string>(PublisherProperty, reader.GetString("PUBLISHER"));
                            LoadProperty<string>(InstitutionProperty, reader.GetString("INSTITUTION"));
                            LoadProperty<string>(VolumeProperty, reader.GetString("VOLUME"));
                            LoadProperty<string>(PagesProperty, reader.GetString("PAGES"));
                            LoadProperty<string>(EditionProperty, reader.GetString("EDITION"));
                            LoadProperty<string>(IssueProperty, reader.GetString("ISSUE"));
                            LoadProperty<bool>(IsLocalProperty, reader.GetBoolean("IS_LOCAL"));
                            LoadProperty<string>(AvailabilityProperty, reader.GetString("AVAILABILITY"));
                            LoadProperty<string>(URLProperty, reader.GetString("URL"));
                            LoadProperty<string>(CommentsProperty, reader.GetString("COMMENTS"));
                            LoadProperty<string>(TypeNameProperty, reader.GetString("TYPE_NAME"));
                            LoadProperty<string>(AbstractProperty, reader.GetString("ABSTRACT"));
                            LoadProperty<string>(OldItemIdProperty, reader.GetString("OLD_ITEM_ID"));
                            LoadProperty<bool>(IsItemDeletedProperty, reader.GetBoolean("IS_DELETED"));
                            LoadProperty<bool>(IsIncludedProperty, reader.GetBoolean("IS_INCLUDED"));
                            LoadProperty<string>(DOIProperty, reader.GetString("DOI"));
                            LoadProperty<string>(KeywordsProperty, reader.GetString("KEYWORDS"));
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static Item GetItem(SafeDataReader reader)
        {
            Item returnValue = new Item();
            returnValue.LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<Int64>(MasterItemIdProperty, reader.GetInt64("MASTER_ITEM_ID"));
            returnValue.LoadProperty<int>(TypeIdProperty, reader.GetInt32("TYPE_ID"));
            returnValue.LoadProperty<string>(AuthorsProperty, reader.GetString("AUTHORS"));
            returnValue.LoadProperty<string>(ParentAuthorsProperty, reader.GetString("PARENTAUTHORS"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
            returnValue.LoadProperty<string>(ParentTitleProperty, reader.GetString("PARENT_TITLE"));
            returnValue.LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
            returnValue.LoadProperty<SmartDate>(DateCreatedProperty, reader.GetSmartDate("DATE_CREATED"));
            returnValue.LoadProperty<string>(CreatedByProperty, reader.GetString("CREATED_BY"));
            returnValue.LoadProperty<SmartDate>(DateEditedProperty, reader.GetSmartDate("DATE_EDITED"));
            returnValue.LoadProperty<string>(EditedByProperty, reader.GetString("EDITED_BY"));
            returnValue.LoadProperty<string>(YearProperty, reader.GetString("YEAR"));
            returnValue.LoadProperty<string>(MonthProperty, reader.GetString("MONTH"));
            returnValue.LoadProperty<string>(StandardNumberProperty, reader.GetString("STANDARD_NUMBER"));
            returnValue.LoadProperty<string>(CityProperty, reader.GetString("CITY"));
            returnValue.LoadProperty<string>(CountryProperty, reader.GetString("COUNTRY"));
            returnValue.LoadProperty<string>(PublisherProperty, reader.GetString("PUBLISHER"));
            returnValue.LoadProperty<string>(InstitutionProperty, reader.GetString("INSTITUTION"));
            returnValue.LoadProperty<string>(VolumeProperty, reader.GetString("VOLUME"));
            returnValue.LoadProperty<string>(PagesProperty, reader.GetString("PAGES"));
            returnValue.LoadProperty<string>(EditionProperty, reader.GetString("EDITION"));
            returnValue.LoadProperty<string>(IssueProperty, reader.GetString("ISSUE"));
            returnValue.LoadProperty<bool>(IsLocalProperty, reader.GetBoolean("IS_LOCAL"));
            returnValue.LoadProperty<string>(AvailabilityProperty, reader.GetString("AVAILABILITY"));
            returnValue.LoadProperty<string>(URLProperty, reader.GetString("URL"));
            returnValue.LoadProperty<string>(CommentsProperty, reader.GetString("COMMENTS"));
            returnValue.LoadProperty<string>(TypeNameProperty, reader.GetString("TYPE_NAME"));
            returnValue.LoadProperty<string>(AbstractProperty, reader.GetString("ABSTRACT"));
            returnValue.LoadProperty<string>(OldItemIdProperty, reader.GetString("OLD_ITEM_ID"));
            returnValue.LoadProperty<bool>(IsItemDeletedProperty, reader.GetBoolean("IS_DELETED"));
            returnValue.LoadProperty<bool>(IsIncludedProperty, reader.GetBoolean("IS_INCLUDED"));
            returnValue.LoadProperty<string>(DOIProperty, reader.GetString("DOI"));
            returnValue.LoadProperty<string>(KeywordsProperty, reader.GetString("KEYWORDS"));
            returnValue.SetItemStatusProperty();
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
