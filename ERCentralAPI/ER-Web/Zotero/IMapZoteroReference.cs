using AuthorsHandling;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.BusinessClasses.ImportItems;
using ERxWebClient2.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                    itemIncomingData.pAuthorsLi.Add(a);
                }
            }
            
            return itemIncomingData;

        }

    }
}
