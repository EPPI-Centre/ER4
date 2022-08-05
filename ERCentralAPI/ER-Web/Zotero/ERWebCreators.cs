using ERxWebClient2.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ERxWebClient2.Zotero
{
    public abstract class ERWebCreators
    {
        public virtual List<CreatorsItem> ObtainCreators(string authors)
        {
            if (authors.Length == 0)
            {
                return new List<CreatorsItem>();
            }
            var authorsArray = authors.Split(';');
            var creatorsArray = new List<CreatorsItem>();
            foreach (var author in authorsArray)
            {
                var firstAndLastNames = author.TrimStart().TrimEnd().Split(' ');
                if (firstAndLastNames.Count() > 1)
                {
                    var creatorsItem = new CreatorsItem
                    {
                        creatorType = "author",
                        firstName = firstAndLastNames[0].ToString(),
                        lastName = firstAndLastNames[1].ToString()
                    };
                    creatorsArray.Add(creatorsItem);
                }
            }
            return creatorsArray;
        }
    }
}
