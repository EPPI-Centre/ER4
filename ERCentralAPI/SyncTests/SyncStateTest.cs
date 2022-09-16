using BusinessLibrary.BusinessClasses;
using Csla;
using Csla.Security;
using Moq;

namespace SyncTests
{
    public class SyncTests {
        private ZoteroItemReviewIDs _zoteroItemReviewIds;
        [SetUp]
        public void Setup()
        {

            string itemIds = "222788, 220330, 75006";

            var dp = new DataPortal<ZoteroItemReviewIDs>();
            var criteria = new SingleCriteria<string>(itemIds);
            _zoteroItemReviewIds = dp.Fetch(criteria);
        }

        [TearDown]
        public void TearDown()
        {
            _zoteroItemReviewIds.Clear();
        }

        [Test]
        public void CheckWhetherItemsExistInZotero(){

            var dpItemsInBoth = new DataPortal<ZoteroERWebReviewItemList>();
            var result = dpItemsInBoth.Fetch();

            var itemsInZotero = result.Select(x => x.ITEM_REVIEW_ID).Intersect(_zoteroItemReviewIds.Select(x => x.ITEM_REVIEW_ID));

            Assert.That(itemsInZotero.Any());
        }
    }
}