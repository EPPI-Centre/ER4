using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    //public class ReviewStatisticsCodeSetList : DynamicBindingListBase<ReviewStatisticsCodeSet>
    public class ReviewStatisticsCodeSetList2 : BusinessListBase<ReviewStatisticsCodeSetList2, ReviewStatisticsCodeSet2>
    {

        
        
        public ReviewStatisticsCodeSetList2() { }

#if SILVERLIGHT
       
#else
        protected void DataPortal_Fetch()
        {
            this.FillList();
        }

        protected void FillList()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewStatisticsAllCodeSets", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    Dictionary<int, string> Codesets = new Dictionary<int, string>();
                    Dictionary<int, string> Users = new Dictionary<int, string>();
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Codesets.Add(reader.GetInt32("SET_ID"), reader.GetString("SET_NAME"));
                        }
                        reader.NextResult();
                        while (reader.Read())
                        {
                            Users.Add(reader.GetInt32("CONTACT_ID"), reader.GetString("CONTACT_NAME"));
                        }
                        reader.NextResult();
                        int CurrentSet = 0;
                        long CurrentItem = 0;
                        bool ItemAddedAsCompleted = false;
                        int CurrentItemContactId = 0;
                        bool CurrentItemIsCompleted = false;
                        int tSet = 0;
                        long tItem = 0;
                        ReviewStatisticsCodeSet2 currentCSstats = ReviewStatisticsCodeSet2.GetReviewStatisticsCodeSet(0,"");
                        //this reader gives us ALL ItemSets in the review, "order by s.SET_ID, ir.ITEM_ID, tis.IS_COMPLETED desc"
                        //thus, we can and do process one set at the time, then item, while receiving the completed ItemSet first (if any)
                        while (reader.Read())
                        {
                            tSet = reader.GetInt32("SET_ID");
                            tItem = reader.GetInt64("ITEM_ID");
                            CurrentItemIsCompleted = reader.GetBoolean("IS_COMPLETED");
                            CurrentItemContactId = reader.GetInt32("CONTACT_ID");
                            if (tSet != CurrentSet)
                            {//new everything (Set, Item)
                                if (currentCSstats.ReviewerStatistics.Count > 0)
                                {
                                    this.Add(currentCSstats);
                                }
                                CurrentSet = tSet;
                                CurrentItem = tItem;
                                ItemAddedAsCompleted = false;
                                currentCSstats = ReviewStatisticsCodeSet2.GetReviewStatisticsCodeSet(CurrentSet, Codesets[CurrentSet]);
                                //we add the counts for this item here, as below we only have "else ifs"...
                                if (CurrentItemIsCompleted)
                                {
                                    ItemAddedAsCompleted = true;
                                    currentCSstats.NumItemsCompleted++;
                                }
                                else
                                {
                                    ItemAddedAsCompleted = false;
                                    currentCSstats.NumItemsIncomplete++;
                                }
                                currentCSstats.AddReviewerCount(CurrentItemContactId, CurrentItemIsCompleted, Users[CurrentItemContactId]);
                            }
                            else if (tItem != CurrentItem)
                            {//moving onto a new item, but not a new set
                                if (CurrentItemIsCompleted)
                                {
                                    ItemAddedAsCompleted = true;
                                    currentCSstats.NumItemsCompleted++;
                                }
                                else
                                {
                                    ItemAddedAsCompleted = false;
                                    currentCSstats.NumItemsIncomplete++;
                                }
                                CurrentItem = tItem;
                                //this is the first time we see this pair of CodeSet&Item, so we always count it in the per-reviewer data 
                                currentCSstats.AddReviewerCount(CurrentItemContactId, CurrentItemIsCompleted, Users[CurrentItemContactId]);

                            }
                            else if (ItemAddedAsCompleted == false)
                            {//this is a new line, for an already started pair of CodeSet&Item (else), and this item doesn't have a completed ItemSet for this CodeSet (ItemAddedAsCompleted == false)
                                //thus, we count this item for all incomplete ItemSets we have
                                //Otherwise, when item has a completed ItemSet, we cound it ONLY in the "completed" (per reviewer) counts
                                currentCSstats.AddReviewerCount(CurrentItemContactId, CurrentItemIsCompleted, Users[CurrentItemContactId]);
                            }
                            else if (ItemAddedAsCompleted == true && CurrentItemIsCompleted == true) 
                            {//uhm, this shouldn't happen (same item completed twice for 2 people on the same codeset
                                currentCSstats.AddReviewerCount(CurrentItemContactId, CurrentItemIsCompleted, Users[CurrentItemContactId]);
                            }
                            //"else", do nothing - this is a new line, for an already started pair of CodeSet&Item (else), and this item DOES have a completed ItemSet for this CodeSet
                            //so we have counted it alread the first time we saw this pair of CodeSet&Item
                        }
                        //finish by adding the last coding tool we've processed, if it contains data
                        if (currentCSstats.ReviewerStatistics.Count > 0)
                        {
                            this.Add(currentCSstats);
                        }
                    }
                    connection.Close();
                }
                RaiseListChangedEvents = true;
            }

        }    

#endif
    }
}
