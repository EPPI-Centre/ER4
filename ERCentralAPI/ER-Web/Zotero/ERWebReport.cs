using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;
using Report = ERxWebClient2.Controllers.ReportZotero;

namespace ERxWebClient2.Zotero
{
	public class ERWebReport : ERWebCreators, IMapERWebReference
	{

		public ERWebReport(IItem item)
		{
			_item = item;
		}

		// Need to use erweb mapping table to get fields on rhs into the correct lhs zotero fields 
		ZoteroCollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
		{
			try
			{
				var newZoteroItem = new Report(_item);

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
