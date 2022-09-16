using NUnit.Framework;

namespace SyncAlgorithmTests
{
    public class SyncAlgorithmTests
    {


        [OneTimeTearDown]
        public void Disposal()
        {

        }

        [SetUp]
        public void Setup()
        {
            // 1 - Need to get the items to test on from the itemList with a specific criteria
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            DataPortal<ItemList> dp = new DataPortal<ItemList>();
            SelectionCriteria CSLAcrit = crit.CSLACriteria;
            ItemList result = dp.Fetch(CSLAcrit);
        }

        [Test]
        public void CheckSyncStatusOfItemsInErWeb()
        {
            // 2 - Need to then check the status via the middle table and return something 
        }
    }
}