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
using System.ComponentModel;
using Csla.DataPortalClient;
using System.Threading;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ComparisonCreateAllCommand : CommandBase<ComparisonCreateAllCommand>
    {

        public ComparisonCreateAllCommand() { }

        private int _setId;
        private int _ComparisonsCreated = 0;

        public ComparisonCreateAllCommand(int setId)
        {
            _setId = setId;
        }

        public int ComparisonsCreated
        {
            get { return _ComparisonsCreated; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_setId", _setId);
            info.AddValue("_ComparisonsCreated", _ComparisonsCreated);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _setId = info.GetValue<int>("_setId");
            _ComparisonsCreated = info.GetValue<int>("_ComparisonsCreated");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            List<ComparisonPair> pairs = new List<ComparisonPair>();//we get these from the db
            List<ComparisonTriplet> triplets = new List<ComparisonTriplet>(); //we produce these from the pairs
            
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ComparisonsAllPotentialPairs", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SetId", _setId));
                    command.Parameters.Add(new SqlParameter("@revId", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        //SP gives us all the viable pairs of ppl to "compare"
                        //viable: they have uncomplete coding in common
                        //they pairs are ordered with the highest n of overlaps on top
                        while (reader.Read())
                        {
                            pairs.Add(new ComparisonPair(reader.GetInt32("cid1"), reader.GetInt32("cid2"), reader.GetInt32("OverlapCount")));
                        }
                    }
                }
                connection.Close();
            }
            foreach (ComparisonPair pair in pairs)
            {
                if (pair.Done) continue;
                else
                {
                    int cid1 = pair.ContactId1;
                    int cid2 = pair.ContactId2;
                    int cid3 = -1;
                    ComparisonPair? secondpair = pairs.FirstOrDefault(
                        x => x !=pair //we're not using the same pair
                        && x.Done == false //we're not using a pair that is already in use
                        && (
                            (x.ContactId1 == cid1)
                            || (x.ContactId1 == cid2)
                            || (x.ContactId2 == cid1)
                            || (x.ContactId2 == cid2) //any member of the current pair, appearing in either position in another pair
                        )

                        //!!![SG Jan 2023]
                        //!!! the condition below is probably correct, but would need an SQL changes to work
                        //if-re instated, it would change how 3-ways comparisons are picked
                        //without this option, comparisons that contain the same pair can and do get created (fairly often)
                        //with this option, they would not, which means that we'd get more comparisons with 2 people only, and little-no reduction of the 
                        //total number of comparisons. Which is why the code is commented out (letting people "see" more data in one screen is better).
                        //however, for the code to work, we need to receive _all possible pairs_ from SQL, including the empty ones (I think: not verified!)
                        //and then we'd need to mark the empty ones as "already done" on creation, I think.

                        //&& (//(IF) we do not want to re-create a pair we already have, so we "look into the future" to see what cid3 would be for this x
                        //    //then we check if the pair 1-3 appears in any "already done" pair
                        //    //or if the pair 2-3 appears in any "already done" pair:
                        //    //if so, the result of this final FirstOrDefault is not null, and we don't like this triplet...
                        //    pairs.FirstOrDefault(
                        //        f=> f.Done == true
                        //        && (
                        //                (x.ContactId1 != cid1 && x.ContactId1 != cid2 //meaning: cid3 will be x.ContactId1
                        //                && (
                        //                    (cid1 == f.ContactId1 && x.ContactId1 == f.ContactId2)//1st line for possible pair in positions 1-3
                        //                    || (cid1 == f.ContactId2 && x.ContactId1 == f.ContactId1)//2nd line for possible pair in positions 1-3
                        //                    || (cid2 == f.ContactId1 && x.ContactId1 == f.ContactId2)//1st line for possible pair in positions 2-3
                        //                    || (cid2 == f.ContactId2 && x.ContactId1 == f.ContactId1)//2nd line for possible pair in positions 2-3
                        //                    ))
                        //                || (x.ContactId2 != cid1 && x.ContactId2 != cid2 //meaning: cid3 will be x.ContactId2
                        //                && (
                        //                    (cid1 == f.ContactId1 && x.ContactId2 == f.ContactId2)//1st line for possible pair in positions 1-3
                        //                    || (cid1 == f.ContactId2 && x.ContactId2 == f.ContactId1)//2nd line for possible pair in positions 1-3
                        //                    || (cid2 == f.ContactId1 && x.ContactId2 == f.ContactId2)//1st line for possible pair in positions 2-3
                        //                    || (cid2 == f.ContactId2 && x.ContactId2 == f.ContactId1)//2nd line for possible pair in positions 2-3
                        //                    ))
                        //           )
                        //        ) == null
                        //)
                    );
                    if (secondpair != null)
                    {//ok, we can use three ppl in the comparison, need to identify the 3rd member
                        if (secondpair.ContactId1 != cid1 && secondpair.ContactId1 != cid2)
                        {
                            cid3 = secondpair.ContactId1;
                            secondpair.Done = true;
                        }
                        else if (secondpair.ContactId2 != cid1 && secondpair.ContactId2 != cid2)
                        {
                            cid3 = secondpair.ContactId2;
                            secondpair.Done = true;
                        }
                        if (secondpair.Done == true)
                        {
                            //but wait, each triplet comprises of 3 pairs: 1-2, 1-3, 2-3 so we need to find also the third pair we're covering, and mark it as done (if present)
                            ComparisonPair? thirdpair = pairs.FirstOrDefault(
                                x => x != pair  //we're not using the same pair
                                && x.Done == false //we're not using a pair that is already in use
                                && (
                                    (x.ContactId1 == cid1 && x.ContactId2 == cid2)//1st line for possible pair in positions 1-2
                                    || (x.ContactId1 == cid2 && x.ContactId2 == cid1)//2nd line for possible pair in positions 1-2
                                    || (x.ContactId1 == cid1 && x.ContactId2 == cid3)//1st line for possible pair in positions 1-3
                                    || (x.ContactId1 == cid3 && x.ContactId2 == cid1)//2nd line for possible pair in positions 1-3
                                    || (x.ContactId1 == cid2 && x.ContactId2 == cid3)//1st line for possible pair in positions 2-3
                                    || (x.ContactId1 == cid3 && x.ContactId2 == cid2)//2nd line for possible pair in positions 2-3
                                    )
                                );
                            if (thirdpair != null) thirdpair.Done = true;//because it's covered by the 3-way comparison we just "found"...
                        }
                        
                    }
                    triplets.Add(new ComparisonTriplet(cid1, cid2, cid3));//cid3 is "-1" if the comparison we'll create has only 2 members
                    pair.Done = true;
                }
            }
            foreach (ComparisonTriplet tripl in triplets) 
            {//at last, we can create the comparisons...
                Comparison comp = new Comparison();
                comp.InGroupAttributeId = -1;
                comp.SetId = _setId;
                comp.ContactId1 = tripl.ContactId1;
                comp.ContactId2 = tripl.ContactId2;
                comp.ContactId3 = tripl.ContactId3;
                comp = comp.Save();
                _ComparisonsCreated++;
            } 
        }
        private class ComparisonPair
        {
            public ComparisonPair(int contactId1, int contactId2, int overlapCount)
            {
                ContactId1 = contactId1;
                ContactId2 = contactId2;
                OverlapCount = overlapCount;
            }

            public int ContactId1 { get; set; }
            public int ContactId2 { get; set; }
            public int OverlapCount { get; set; }
            public bool Done { get; set; } = false;
        }
        private class ComparisonTriplet
        {
            public ComparisonTriplet(int contactId1, int contactId2, int contactId3)
            {
                ContactId1 = contactId1;
                ContactId2 = contactId2;
                ContactId3 = contactId3;
            }

            public int ContactId1 { get; set; }
            public int ContactId2 { get; set; }
            public int ContactId3 { get; set; }
        }
#endif
    }
}
