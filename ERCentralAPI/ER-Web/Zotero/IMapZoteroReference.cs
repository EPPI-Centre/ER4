using AuthorsHandling;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.BusinessClasses.ImportItems;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public interface IMapZoteroReference
    {
        ERWebItem MapReferenceFromZoteroToErWeb(Item newERWebItem);
    }

    public class ERWebItem
    {
        public Item Item { get; set; }
        public ItemDocument ItemDocument { get; set; }

        public ItemIncomingData AuthorsListForIncomingData(CreatorsItem[] creators)
        {
            var itemIncomingData = new ItemIncomingData();
            if (itemIncomingData.AuthorsLi == null) itemIncomingData.AuthorsLi = new AutorsList();
            if (itemIncomingData.pAuthorsLi == null) itemIncomingData.pAuthorsLi = new Csla.Core.MobileList<AutH>();
            int AuthRank = 0;
            foreach (var Zau in creators)
            {
                if (Zau.creatorType == "author")
                {
                    AutH a = new AutH();
                    a.FirstName = Zau.firstName;
                    a.MiddleName = "";
                    a.LastName = Zau.lastName;
                    a.Role = 0;//only looking for "actual authors" not parent authors which can be Book editors and the like.
                    a.Rank = AuthRank;
                    AuthRank++;                  
                    itemIncomingData.AuthorsLi.Add(a);                    
                    //itemIncomingData.pAuthorsLi.Add(a);
                }
            }
            
            return itemIncomingData;

        }

        public ItemIncomingData CreateItemIncomingDataFromCollection(string zoteroKey, Collection? collectionItem, ERWebItem erWebItem, ItemIncomingData authors)
        {
            return new ItemIncomingData
            {
                Abstract = collectionItem.data.abstractNote ?? "",
                Year = collectionItem.data.date ?? "0",
                Title = collectionItem.data.title,
                Parent_title = collectionItem.data.publicationTitle,
                Short_title = collectionItem.data.shortTitle ?? "",
                Type_id = erWebItem.Item.TypeId,
                AuthorsLi = authors.AuthorsLi,
                pAuthorsLi = authors.pAuthorsLi,
                ZoteroKey = zoteroKey,
                DateEdited = DateTime.Parse(collectionItem.data.dateModified)

            };
        }

    }
}
