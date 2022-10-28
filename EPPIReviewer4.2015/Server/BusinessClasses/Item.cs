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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

using System.Globalization;
using ERxWebClient2.Zotero;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using AuthorsHandling;
using System.Data.SqlTypes;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Item : BusinessBase<Item>, IItem
    {

    public Item() { }

        public override string ToString()
        {
            return Title;
        }
#if (!CSLA_NETCORE)
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new SkipEmptyContractResolver(), DateFormatString = "dd/MM/yyyy"});
        }
#endif
#if SILVERLIGHT
        public string ToJSON(List<ItemSet> itemSetList)
        {
            string ItemJSON = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new SkipEmptyContractResolver(), DateFormatString = "dd/MM/yyyy" });
            if (itemSetList != null)
            {
                JObject interim = JObject.Parse(ItemJSON);
                JArray codes = new JArray();
                JArray Outcomes = new JArray();
                foreach (ItemSet itemSet in itemSetList)
                {
                    if (itemSet.ItemId == this.ItemId)
                    {//collect the coding info in JSON requires a bit of rejingling to produce a flat list (ignoring where codes come from).
                        string AttributesJSON = JsonConvert.SerializeObject(itemSet, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new SkipEmptyContractResolver(), DateFormatString = "dd/MM/yyyy" });
                        JObject jo = JObject.Parse(AttributesJSON);
                        JArray internalVals = (JArray)jo["ItemAttributesList"];
                        JArray internalOutcomes = new JArray();
                        JToken tem = (JToken)jo["OutcomeItemList"];
                        if (tem != null)
                        {
                            internalOutcomes = (JArray)jo["OutcomeItemList"]["OutcomesList"];
                        }
                        if (internalVals != null)
                        {
                            foreach (JToken tok in internalVals)
                            {
                                codes.Add(tok);
                            }
                        }
                        if (internalOutcomes != null)
                        {
                            foreach (JToken tok in internalOutcomes)
                            {
                                Outcomes.Add(tok);
                            }
                        }
                    }
                }
                if (codes.Count > 0)
                {//add codes to object
                    JProperty prop = new JProperty("Codes", codes);
                    interim.Add(prop);
                    ItemJSON = interim.ToString(Formatting.Indented);
                }
                if (Outcomes.Count > 0)
                {//add codes to object
                    JProperty prop = new JProperty("Outcomes", Outcomes);
                    interim.Add(prop);
                    ItemJSON = interim.ToString(Formatting.Indented);
                }
            }
                    
            return ItemJSON;
        }
#endif
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

        

        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public string FirstPage()
        {
            if (Pages == "")
                return "";
            int i = this.Pages.IndexOf("-");
            if (i == -1)
                return Pages;
            else
                return Pages.Substring(0, i);
        }

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

        public string GetNICECitation()
        {
            string retVal = "";
            switch (this.TypeId)
            {
                case 1: //Report
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + City + ": " + Publisher + ", " + Pages;
                    break;
                case 2: //Book, Whole
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + City + ": " + Publisher;
                    break;
                case 3: //Book, Chapter
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". In: " + CleanAuthors(ParentAuthors) + ", editors. " + ParentTitle + ". " + City + ": " + Publisher + ", p" + Pages;
                    break;
                case 4: //Dissertation
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + Edition + ", " + Institution + ".";
                    break;
                case 5: //Conference Proceedings
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". In: " + ParentTitle + ", " + City + ". " + Publisher + ", p" + Pages;
                    break;
                case 6: //Document From Internet Site
                    retVal = CleanAuthors(Authors) + " (" + Year + ") <a href='" + URL + "'>" + Title + "</a>. " + Publisher;
                    break;
                case 7: //Web Site
                    retVal = CleanAuthors(Authors) + " (" + Year + ") <a href='" + URL + "'>" + Title + "</a> " + (Availability == "" ? "" : " [online; accessed: " + Availability + "]");
                    break;
                case 8: //DVD, Video, Media
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + (Availability == "" ? "" : " [online; accessed: " + Availability + "]");
                    break;
                case 9: //Research project
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + City + ": " + Publisher + ", ";
                    break;
                case 10: //Article In A Periodical
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + CleanAuthors(ParentTitle) + " " + Volume + (Issue != "" ? "(" + Issue + ")" : "") + ", " + Pages;
                    break;
                case 11: //Interview
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". ";
                    break;
                case 12: //Generic
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + City + ": " + Publisher;
                    break;
                case 14: //Journal, Article
                    retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + CleanAuthors(ParentTitle) + " " + Volume + (Issue != "" ? "(" + Issue + ")" : "") + ", " + Pages;
                    break;
            }
            return retVal;
        }
        public static readonly PropertyInfo<string> QuickCitationProperty = RegisterProperty<string>(new PropertyInfo<string>("QuickCitation", "QuickCitation", string.Empty));
        [JsonProperty]
        public string QuickCitation
        {
            get
            {
                string retVal = "";
                switch (this.TypeId)
                {
                    case 1: //Report
                        retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + City + ": " + Publisher + ", " + Pages +
                            ( DOI == "" ? "" : " DOI: " + DOI);
                        break;
                    case 2: //Book, Whole
                        retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + City + ": " + Publisher +
                            (DOI == "" ? "" : " DOI: " + DOI);
                        break;
                    case 3: //Book, Chapter
                        retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". In: " + CleanAuthors(ParentAuthors) + ", editors. " + ParentTitle + ". " + City + ": " + Publisher + ", p" + Pages +
                            (DOI == "" ? "" : " DOI: " + DOI);
                        break;
                    case 4: //Dissertation
                        retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + Edition + ", " + Institution + "." +
                            (DOI == "" ? "" : " DOI: " + DOI);
                        break;
                    case 5: //Conference Proceedings
                        retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". In: " + ParentTitle + ", " + City + ". " + Publisher + ", p" + Pages +
                            (DOI == "" ? "" : " DOI: " + DOI);
                        break;
                    case 6: //Document From Internet Site
                        retVal = CleanAuthors(Authors) + " (" + Year + ") " + URL + Title + ". " + Publisher +
                            (DOI == "" ? "" : " DOI: " + DOI);
                        break;
                    case 7: //Web Site
                        retVal = CleanAuthors(Authors) + " (" + Year + ") " + URL + Title + " " + (Availability == "" ? "" : " [online; accessed: " + Availability + "]") +
                            (DOI == "" ? "" : " DOI: " + DOI);
                        break;
                    case 8: //DVD, Video, Media
                        retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + (Availability == "" ? "" : " [online; accessed: " + Availability + "]" +
                            (DOI == "" ? "" : " DOI: " + DOI));
                        break;
                    case 9: //Research project
                        retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + City + ": " + Publisher + ". " +
                            (DOI == "" ? "" : " DOI: " + DOI);
                        break;
                    case 10: //Article In A Periodical
                        retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + CleanAuthors(ParentTitle) + " " + Volume + (Issue != "" ? "(" + Issue + ")" : "") + ", " + Pages +
                            (DOI == "" ? "" : " DOI: " + DOI);
                        break;
                    case 11: //Interview
                        retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " +
                            (DOI == "" ? "" : " DOI: " + DOI);
                        break;
                    case 12: //Generic
                        retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + City + ": " + Publisher +
                            (DOI == "" ? "" : " DOI: " + DOI);
                        break;
                    case 14: //Journal, Article
                        retVal = CleanAuthors(Authors) + " (" + Year + ") " + Title + ". " + CleanAuthors(ParentTitle) + " " + Volume + (Issue != "" ? "(" + Issue + ")" : "") + ", " + Pages +
                            ( DOI == "" ? "" : " DOI: " + DOI);
                        break;
                }
                return retVal;
            }
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
        
        public void EnrichWithMicrosoftAcademicData(MagPaper mp)
        {
            if (mp.OriginalTitle != null && this.Title.Length < mp.OriginalTitle.Length)
                this.Title = mp.OriginalTitle;
            if (mp.Abstract != null && this.Abstract.Length < mp.Abstract.Length)
                this.Abstract = mp.Abstract;
            if (mp.Issue != null && this.Issue.Length < mp.Issue.Length)
                this.Issue = mp.Issue;
            if (mp.Volume != null && this.Volume.Length < mp.Volume.Length)
                this.Volume = mp.Volume;
            if (this.URL == "")
                this.URL = "https://explore.openalex.org/works/W" + mp.PaperId.ToString();
            if (mp.Journal != null && this.ParentTitle.Length < mp.Journal.Length)
                this.ParentTitle = mp.Journal;
            if (mp.Authors != null && this.Authors == "")
                this.Authors = mp.Authors;
            if (mp.FirstPage != null && mp.LastPage != null && this.Pages.Length < (mp.FirstPage + "-" + mp.LastPage).Length)
                this.Pages = mp.FirstPage + "-" + mp.LastPage;
            if (this.Year.Length == 0)
                this.Year = mp.Year.ToString();
            if (mp.DOI != null && this.DOI.Length < 5)
                this.DOI = mp.DOI;
            if (mp.Publisher != null & this.Publisher.Length < mp.Publisher.Length)
                this.Publisher = mp.Publisher;
        }
        
        public static readonly PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        [JsonProperty]
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
        public static readonly PropertyInfo<Int64> MasterItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("MasterItemId", "MasterItemId"));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
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
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public bool IsDupilcate
        {
            get
            {
                return IsIncluded & IsItemDeleted & (MasterItemId > 0);
            }
        }
        public static readonly PropertyInfo<int> TypeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("TypeId", "TypeId"));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
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

        public static readonly PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> ParentTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ParentTitle", "ParentTitle", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> ShortTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ShortTitle", "ShortTitle", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<SmartDate> DateCreatedProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("DateCreated", "DateCreated"));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> CreatedByProperty = RegisterProperty<string>(new PropertyInfo<string>("CreatedBy", "CreatedBy", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<SmartDate> DateEditedProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("DateEdited", "DateEdited"));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> EditedByProperty = RegisterProperty<string>(new PropertyInfo<string>("EditedBy", "EditedBy", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> YearProperty = RegisterProperty<string>(new PropertyInfo<string>("Year", "Year", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> MonthProperty = RegisterProperty<string>(new PropertyInfo<string>("Month", "Month", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> StandardNumberProperty = RegisterProperty<string>(new PropertyInfo<string>("StandardNumber", "StandardNumber", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> CityProperty = RegisterProperty<string>(new PropertyInfo<string>("City", "City", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> CountryProperty = RegisterProperty<string>(new PropertyInfo<string>("Country", "Country", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> PublisherProperty = RegisterProperty<string>(new PropertyInfo<string>("Publisher", "Publisher", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> InstitutionProperty = RegisterProperty<string>(new PropertyInfo<string>("Institution", "Institution", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> VolumeProperty = RegisterProperty<string>(new PropertyInfo<string>("Volume", "Volume", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> PagesProperty = RegisterProperty<string>(new PropertyInfo<string>("Pages", "Pages", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> EditionProperty = RegisterProperty<string>(new PropertyInfo<string>("Edition", "Edition", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> IssueProperty = RegisterProperty<string>(new PropertyInfo<string>("Issue", "Issue", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<bool> IsLocalProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsLocal", "IsLocal", false));
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

        public static readonly PropertyInfo<string> AvailabilityProperty = RegisterProperty<string>(new PropertyInfo<string>("Availability", "Availability", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> URLProperty = RegisterProperty<string>(new PropertyInfo<string>("URL", "URL", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> OldItemIdProperty = RegisterProperty<string>(new PropertyInfo<string>("OldItemId", "OldItemId", string.Empty));
        [JsonProperty]
        public string OldItemId
        {
            get
            {
                return GetProperty(OldItemIdProperty);
            }
        }



        public static readonly PropertyInfo<string> AbstractProperty = RegisterProperty<string>(new PropertyInfo<string>("Abstract", "Abstract", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> CommentsProperty = RegisterProperty<string>(new PropertyInfo<string>("Comments", "Comments", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> TypeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("TypeName", "TypeName", string.Empty));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> AuthorsProperty = RegisterProperty<string>(new PropertyInfo<string>("Authors", "Authors", string.Empty));
        [JsonProperty]
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
        public static readonly PropertyInfo<string> ParentAuthorsProperty = RegisterProperty<string>(new PropertyInfo<string>("ParentAuthors", "ParentAuthors", string.Empty));
        [JsonProperty]
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
        public static readonly PropertyInfo<string> DOIProperty = RegisterProperty<string>(new PropertyInfo<string>("DOI", "DOI", string.Empty));
        [JsonProperty]
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
        public static readonly PropertyInfo<string> KeywordsProperty = RegisterProperty<string>(new PropertyInfo<string>("Keywords", "Keywords", string.Empty));
        [JsonProperty]
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
        public static readonly PropertyInfo<string> AttributeAdditionalTextProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeAdditionalText", "AttributeAdditionalText", string.Empty));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public string AttributeAdditionalText
        {
            get
            {
                return GetProperty(AttributeAdditionalTextProperty);
            }
            set
            {
                SetProperty(AttributeAdditionalTextProperty, value);
            }
        }

        public static readonly PropertyInfo<int> RankProperty = RegisterProperty<int>(new PropertyInfo<int>("Rank", "Rank", 0));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public int Rank
        {
            get
            {
                return GetProperty(RankProperty);
            }
            set
            {
                SetProperty(RankProperty, value);
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

        public static readonly PropertyInfo<bool> IsItemDeletedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsItemDeleted", "IsItemDeleted"));
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

        public static readonly PropertyInfo<bool> IsIncludedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsIncluded", "IsIncluded"));
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

        public static readonly PropertyInfo<bool> IsSelectedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsSelected", "IsSelected", false));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
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

        public static readonly PropertyInfo<string> ItemStatusProperty = RegisterProperty<string>(new PropertyInfo<string>("ItemStatus", "ItemStatus"));
        [JsonProperty]
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

        public static readonly PropertyInfo<string> ItemStatusTooltipProperty = RegisterProperty<string>(new PropertyInfo<string>("ItemStatusTooltip", "ItemStatusTooltip"));
        [JsonProperty]
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

        public static readonly PropertyInfo<ItemArmList> ArmsProperty = RegisterProperty<ItemArmList>(new PropertyInfo<ItemArmList>("Arms", "Arms"));
        public ItemArmList Arms
        {
            get
            {
                ItemArmList result = GetProperty(ArmsProperty);
                if (result != null)
                {
                    return result;
                }
                result = ItemArmList.NewItemArmList();
                this.LoadProperty(ArmsProperty, result);
                return GetProperty(ArmsProperty);

            }
            set
            {
                SetProperty(ArmsProperty, value);
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
                    command.Parameters.Add(new SqlParameter("@OLD_ITEM_ID", ReadProperty(OldItemIdProperty)));
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
                    command.Parameters.Add(new SqlParameter("@SearchText", ToShortSearchText(ReadProperty(TitleProperty))));

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
                            LoadProperty<string>(CreatedByProperty, reader.GetString("CREATED_BY"));
                            LoadProperty<SmartDate>(DateEditedProperty, reader.GetSmartDate("DATE_EDITED"));
                            LoadProperty<string>(EditedByProperty, reader.GetString("EDITED_BY"));
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
                            SetItemStatusProperty();
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static Item GetItem(SafeDataReader reader)
        {
            bool HasAdditionalText = false;
            bool HasRank = false;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals("ADDITIONAL_TEXT", StringComparison.InvariantCultureIgnoreCase))
                    HasAdditionalText = true;
                if (reader.GetName(i).Equals("ITEM_RANK", StringComparison.InvariantCultureIgnoreCase))
                    HasRank = true;
            }
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
            if (HasAdditionalText == true)
                returnValue.LoadProperty<string>(AttributeAdditionalTextProperty, reader.GetString("ADDITIONAL_TEXT"));
            if (HasRank == true)
                returnValue.LoadProperty<Int32>(RankProperty, reader.GetInt32("ITEM_RANK"));
            returnValue.SetItemStatusProperty();
            returnValue.MarkOld();
            return returnValue;
        }

        // *********************** BEGIN TOSHORTSEARCHTEXT **********************

        private static readonly Lazy<Regex> alphaNumericRegex = new Lazy<Regex>(() => new Regex("[^a-zA-Z0-9]"));

        public static string ToShortSearchText(string ss)
        {
            if (ss == "")
                return "";
            else if (ss.Length == 1) return ss;//what else can we do? Added by SG on 22/09/2021
            string orig = ss;
            ss = RemoveLanguageAndThesisText(ss);
            ss = MagMakesHelpers.CleanText(ss);//the true paramater would it use a less aggressive stripping out if the original string gets shortened by 90% or more
            string r = Truncate(ToSimpleText(RemoveDiacritics(ss))
                    .Replace("a", "")
                    .Replace("e", "")
                    .Replace("i", "")
                    .Replace("o", "")
                    .Replace("u", "")
                    .Replace("ize", "")
                    .Replace("ise", ""), 500);

            
            if (r.Length > 1)
            {// additional tweak to reduce the length of longer strings and often reduce noise
                if (r.Length < 20)
                    return r;
                else if (r.Length <= 30)
                {
                    r = r.Substring(0, r.Length - 5);
                }
                else
                {
                    // JT more radical change - truncate at 30 characters
                    r = r.Substring(0, 30);
                }
                return r;
            }
            else return Item.DoCharsBasedShortString(orig.ToLower().Trim());//apparently "ToLower()" does a decent job even on non-latin alphabets...
        }
        private static string DoCharsBasedShortString(string s)
        {
            if (_UnicRanges == null)
            {
                Item.BuildUnicodeRanges();//only done once per thread(?): save CPU cycles...
            }
            bool found = false;
            UnicodeRange range = _UnicRanges[5];//default case, fictional: we only care about the rules, not the char-range
            foreach (Char c in s)
            { //we look for the first instance where 2 chars fall in the same "range" and thus get the rules we'll use for setting the search text
                int ind = _UnicRanges.FindIndex(f => f.CharIsInThisRange(c));
                if (ind > -1)
                {
                    if (found)
                    {
                        range = _UnicRanges[ind];//the previous char did fall in a range!
                        break;
                    }
                    else found = true;
                }
                else found = false;
            }
            int len = s.Length;
            if (range.WordCharacters)
            {//ideograms, we'll take just 4
                if (len <= 4) return s;
                else return s.Substring(0, 4);
            }
            //case dealing with Right-To-Left languages, not needed, as something inverts the order for us when chars come from such languages!
            //else if (range.LeftToRight == false)
            //{//get the "end" as the beginning is on the right
            //    if (len <= 15) return s;
            //    else return s.Substring(0, 15);//the last 15 chars, because reading goes from right to left
            //}
            else {
                if (len <= 15) return s;
                else return s.Substring(0, 15);//first 15 chars
            }
        }
        private static List<UnicodeRange> _UnicRanges;
        private static void BuildUnicodeRanges()
        {
            _UnicRanges = new List<UnicodeRange>();
            //ranges are unified by keeping continuous ranges in one entry, if rules don't change
            //also vaguely ordered to keep common cases on top (save CPU cycles!)
            //then the cases that have "default" rules are commented out: we don't need them, we will "just" use the last (fake) case which picked by default.

            _UnicRanges.Add(new UnicodeRange(1424, 1871)); //Hebrew, Arabic, Urdu, Syriac et. al.
            //_UnicRanges.Add(new UnicodeRange(1024, 1279)); //Cyrillic
            _UnicRanges.Add(new UnicodeRange(12800, 40959, true)); //Chinese, I think/hope
            _UnicRanges.Add(new UnicodeRange(63744, 64255, true)); //more chinese ideograms, I think/hope 
            //_UnicRanges.Add(new UnicodeRange(2304, 4991)); //Devanagari, Bengali, Gurmukhi, Gujarati, Oriya,Tamil,Telugu,Kannada,Malayalam,Gurmukhi,Gujarati,Oriya...Ethiopic
            //_UnicRanges.Add(new UnicodeRange(880, 1023)); //Greek
            _UnicRanges.Add(new UnicodeRange(1920, 2047)); //Thaana, NKo
            //_UnicRanges.Add(new UnicodeRange(1328, 1423)); //Armenian
            
            //_UnicRanges.Add(new UnicodeRange(5024, 5887)); //Cherokee ,Unified Canadian Aboriginal Syllabics,Ogham,Runic

            //_UnicRanges.Add(new UnicodeRange(6016, 6319)); //Khmer  

            _UnicRanges.Add(new UnicodeRange(12272, 12287, true)); //??Ideographic Description Characters ??
            //_UnicRanges.Add(new UnicodeRange(12352, 12735)); //Hiragana ... Bopomofo Extended

            //_UnicRanges.Add(new UnicodeRange(40960, 42191)); //Yi Syllables, Yi Radicals

            //_UnicRanges.Add(new UnicodeRange(44032, 55203)); //Hangul Syllables 
            _UnicRanges.Add(new UnicodeRange(55300, 55300));//fake, used as the default

        }
        public static string RemoveDiacritics(string stIn)
        {
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        public static string ToSimpleText(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            return alphaNumericRegex.Value.Replace(s, "").ToLower();
        }

        // This added by JT. Removes [German] and [Master's thesis] etc from the end of the string
        // so long as the text removed isn't more than 33% of the length of the title
        public static string RemoveLanguageAndThesisText(string s)
        {
            s = s.Trim();
            int max = 4; //we'll remove a max of four sequences of [abc][def][ghk]...
            int counter = 1;
            while (counter < max)
            {
                if (s.EndsWith("]"))
                {
                    int i = s.LastIndexOf('[');
                    if (i > 0 && (s.Length - i) * 3 < s.Length)
                    {
                        s = s.Substring(0, i).Trim();
                        counter++;
                    }
                    else break;
                }
                else break;
            }
            return s;
        }
        
        private class UnicodeRange
        {
            public UnicodeRange(int start, int end, bool wChar = false)
            {
                RangeStart = start;
                RangeEnd = end;
                WordCharacters = wChar;
                //LeftToRight = leftToR;
            }
            public int RangeStart { get; set; }
            public int RangeEnd { get; set; }
            public bool WordCharacters { get; set; }
            //public bool LeftToRight { get; set; }
            public bool CharIsInThisRange(Char c)
            {
                int index = (int)c;
                if (index >= RangeStart && index <= RangeEnd) return true;
                else return false;
            }
        }
#endif

    }
	public interface IErWebItem
	{
	}
	public interface IItem : IErWebItem
	{
		long ItemId { get; set; }
		long MasterItemId { get; set; }
		int TypeId { get; set; }
		string Authors { get; set; }
		string ParentAuthors { get; set; }
		string Title { get; set; }
		string ParentTitle { get; set; }
		string ShortTitle { get; set; }
		SmartDate DateCreated { get; set; }
		string CreatedBy { get; set; }
		SmartDate DateEdited { get; set; }
		string EditedBy { get; set; }
		string Year { get; set; }
		string Month { get; set; }
		string StandardNumber { get; set; }
		string City { get; set; }
		string Country { get; set; }
		string Publisher { get; set; }
		string Institution { get; set; }
		string Volume { get; set; }
		string Pages { get; set; }
		string Edition { get; set; }
		string Issue { get; set; }
		bool IsLocal { get; set; }
		string Availability { get; set; }
		string URL { get; set; }
		string Comments { get; set; }
		string TypeName { get; set; }
		string Abstract { get; set; }
		bool IsItemDeleted { get; set; }
		bool IsIncluded { get; set; }
		string DOI { get; set; }
		string Keywords { get; set; }
	}
}
