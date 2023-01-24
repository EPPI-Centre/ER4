using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
	public class ERWebProject : ERWebCreators, IMapERWebReference
	{

		public ERWebProject(IItem item)
		{
			_item = item;
		}

		// Need to use erweb mapping table to get fields on rhs into the correct lhs zotero fields 
		ZoteroCollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
		{
			try
			{
				var newZoteroItem = new ReportZotero(_item);

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
