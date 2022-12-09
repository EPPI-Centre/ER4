using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
	public class ERWebPeriodical : ERWebCreators, IMapERWebReference
	{

		public ERWebPeriodical(IItem item)
		{
			_item = item;
		}

		// Need to use erweb mapping table to get fields on rhs into the correct lhs zotero fields 
		ZoteroCollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
		{
			try
			{
				var newZoteroItem = new Generic(_item);

				return newZoteroItem;

			}
			catch (System.Exception ex)
			{
				var detailOfError = ex;
				throw;
			}
		}
	}
}
