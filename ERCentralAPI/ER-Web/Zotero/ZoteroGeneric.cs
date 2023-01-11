using BusinessLibrary.BusinessClasses;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;
using Microsoft.IdentityModel.Tokens;

namespace ERxWebClient2.Zotero
{
    public class ZoteroGeneric : ZoteroReferenceCreator,  IMapZoteroReference
    {
        private Collection _genericItem;

        public ZoteroGeneric(Collection collection)
        {
            _genericItem = collection;
        }

        ERWebItem IMapZoteroReference.MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {
            try
            {
                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _genericItem);
                erWebItem.Item.TypeId = 12;
                erWebItem.Item.TypeName = "Generic";
				erWebItem.Item.IsIncluded = true;
                if (!string.IsNullOrWhiteSpace(_genericItem.data.publisher)) erWebItem.Item.ParentTitle = _genericItem.data.publisher.Trim();
                if (_genericItem.data.itemType == "case" 
                    && erWebItem.Item.Title.IsNullOrEmpty() 
                    && !_genericItem.data.caseName.IsNullOrEmpty()) erWebItem.Item.Title = _genericItem.data.caseName;
                else if(_genericItem.data.itemType == "statute"
                    && erWebItem.Item.Title.IsNullOrEmpty()
                    && !_genericItem.data.nameOfAct.IsNullOrEmpty()) erWebItem.Item.Title = _genericItem.data.nameOfAct; 

                if (_genericItem.data.itemType == "case"
                    && !_genericItem.data.dateDecided.IsNullOrEmpty()
                    && erWebItem.Item.Year.IsNullOrEmpty())
                {
                    SetYearAndMonth(erWebItem.Item, _genericItem.data.dateDecided);
                }
                else if (_genericItem.data.itemType == "patent"
                    && !_genericItem.data.issueDate.IsNullOrEmpty()
                    && erWebItem.Item.Year.IsNullOrEmpty())
                {
                    SetYearAndMonth(erWebItem.Item, _genericItem.data.issueDate);
                }
                else if (_genericItem.data.itemType == "statute"
                    && !_genericItem.data.dateEnacted.IsNullOrEmpty()
                    && erWebItem.Item.Year.IsNullOrEmpty())
                {
                    SetYearAndMonth(erWebItem.Item, _genericItem.data.dateEnacted);
                }
                return erWebItem;
                                
            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }
        }
        
    }
}