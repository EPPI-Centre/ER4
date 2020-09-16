using BusinessLibrary.BusinessClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDatabasesMVC.ViewModels
{
    public class FullItemDetails
    {
        public Item Item { get; set; }
        public ItemDocumentList Documents { get; set; }
        public ItemTimepointList Timepoints { get; set; }

        public ReadOnlySource Source { get; set; }
        public ItemDuplicatesReadOnlyList Duplicates { get; set; }

        public string URLLink() {
            if (Item == null || Item.URL.Trim() == "") return "";
            else 
            {
                Uri uriResult;
                bool isUrl = Uri.TryCreate(Item.URL, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (isUrl) return uriResult.AbsoluteUri;
                else return "";
            }
        }
        public string DOILink()
        {
            if (Item == null || Item.DOI.Trim() == "") return "";
            else {
                string chk = Item.DOI.ToLower();
                int ind = chk.IndexOf("doi.org/");
                if (chk.StartsWith("http://")
                    || chk.StartsWith("https://")
                )
                {
                    if (ind > 6 && ind < 12) return Item.DOI;
                    else return "";
                }
                else if (ind == -1 && chk.IndexOf('/') > 0)
                {
                    return "https://doi.org/" + Item.DOI;
                }
                else return "";
        }
        }

        public string FieldNameForThisByPubType(string FieldName)
        {
            string lcFieldName = FieldName.ToLower();
            if (Item == null)
            {
                if (lcFieldName == "parenttitle") return "Parent Title";
                else if (lcFieldName == "parentauthors") return "Parent Authors";
                else if (lcFieldName == "standardnumber") return "Standard Number";
                else return FieldName;
            }
            else if (Item.TypeId == 14)
            {
                if (lcFieldName == "parenttitle") return "Journal";
                else if (lcFieldName == "parentauthors") return "Parent Authors";
                else if (lcFieldName == "standardnumber") return "ISSN";
                else return FieldName;
            }
            else if (Item.TypeId == 2)
            {
                if (lcFieldName == "parenttitle") return "Parent Title";
                else if (lcFieldName == "parentauthors") return "Parent Authors";
                else if (lcFieldName == "standardnumber") return "ISBN";
                else return FieldName;
            }
            else if (Item.TypeId == 3)
            {
                if (lcFieldName == "parenttitle") return "Book Title";
                else if (lcFieldName == "parentauthors") return "Editors";
                else if (lcFieldName == "standardnumber") return "ISBN";
                else return FieldName;
            }
            else if (Item.TypeId == 4)
            {
                if (lcFieldName == "parenttitle") return "Publ. Title";
                else if (lcFieldName == "parentauthors") return "Parent Authors";
                else if (lcFieldName == "standardnumber") return "ISSN/ISBN";
                else return FieldName;
            }
            else if (Item.TypeId == 5)
            {
                if (lcFieldName == "parenttitle") return "Conference";
                else if (lcFieldName == "parentauthors") return "Parent Authors";
                else if (lcFieldName == "standardnumber") return "ISSN/ISBN";
                else return FieldName;
            }
            else if (Item.TypeId == 10)
            {
                if (lcFieldName == "parenttitle") return "Periodical";
                else if (lcFieldName == "parentauthors") return "Parent Authors";
                else if (lcFieldName == "standardnumber") return "ISSN/ISBN";
                else return FieldName;
            }
            else if (Item.TypeId == 0)
            {
                if (lcFieldName == "parenttitle") return "Parent Title";
                else if (lcFieldName == "parentauthors") return "Parent Authors";
                else if (lcFieldName == "standardnumber") return "Standard Number";
                else return FieldName;
            }
            else
            {
                if (lcFieldName == "parenttitle") return "Parent Title";
                else if (lcFieldName == "parentauthors") return "Parent Authors";
                else if (lcFieldName == "standardnumber") return "Standard Number";
                else return FieldName;
            }
        }
    }
}
