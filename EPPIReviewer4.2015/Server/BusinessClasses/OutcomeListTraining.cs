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
    public class OutcomeListTraining : DynamicBindingListBase<Outcome>
    {

        public static void GetOutcomeListTraining(EventHandler<DataPortalResult<OutcomeListTraining>> handler)
        {
            DataPortal<OutcomeListTraining> dp = new DataPortal<OutcomeListTraining>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
        public OutcomeListTraining() { }

    protected override void AddNewCore()
    {
        Add(Outcome.NewOutcome());
    }

#else
        private OutcomeListTraining() { }
#endif

#if SILVERLIGHT
    
#else
        internal static OutcomeListTraining GetOutcomes()
        {
            OutcomeListTraining returnValue = new OutcomeListTraining();

            returnValue.RaiseListChangedEvents = false;
            Random rand = new Random();
            /*
            for (int i = 1; i < 4; i++)
            {
                Outcome outcome = new Outcome();
                outcome.OutcomeTypeId = 1;
                outcome.Title = "Title " + i.ToString();
                outcome.Data1 = rand.Next(1, 1 * i) + 50;
                outcome.Data2 = rand.Next(1, 1 * i) + 50;
                outcome.Data3 = 13.2 + rand.Next(2, 16);
                outcome.Data4 = 11.2 + rand.Next(2, 15);
                outcome.Data5 = 3.5;
                outcome.Data6 = 3.2;
                Add(outcome);
            }
             */
            Outcome outcome = new Outcome();
            outcome.OutcomeTypeId = 1;
            outcome.Title = "Study 1";
            outcome.Data1 = 23;
            outcome.Data2 = 23;
            outcome.Data3 = 3.1;
            outcome.Data4 = 1.8;
            outcome.Data5 = 1;
            outcome.Data6 = 1;
            outcome.IsSelected = true;
            returnValue.Add(outcome);
            outcome = new Outcome();
            outcome.OutcomeTypeId = 1;
            outcome.Title = "Study 2";
            outcome.Data1 = 33;
            outcome.Data2 = 43;
            outcome.Data3 = 33;
            outcome.Data4 = 32;
            outcome.Data5 = 2.5;
            outcome.Data6 = 2.5;
            outcome.IsSelected = true;
            returnValue.Add(outcome);
            outcome = new Outcome();
            outcome.OutcomeTypeId = 1;
            outcome.Title = "Study 3";
            outcome.Data1 = 78;
            outcome.Data2 = 78;
            outcome.Data3 = 24;
            outcome.Data4 = 22.3;
            outcome.Data5 = 2.8;
            outcome.Data6 = 2.8;
            outcome.IsSelected = true;
            returnValue.Add(outcome);
            outcome = new Outcome();
            outcome.OutcomeTypeId = 1;
            outcome.Title = "Study 4";
            outcome.Data1 = 230;
            outcome.Data2 = 229;
            outcome.Data3 = 33.9;
            outcome.Data4 = 33.25;
            outcome.Data5 = 1.6;
            outcome.Data6 = 1.7;
            outcome.IsSelected = true;
            returnValue.Add(outcome);
            outcome = new Outcome();
            outcome.OutcomeTypeId = 1;
            outcome.Title = "Study 5";
            outcome.Data1 = 20;
            outcome.Data2 = 20;
            outcome.Data3 = 2.1;
            outcome.Data4 = 0.8;
            outcome.Data5 = 1.5;
            outcome.Data6 = 1.4;
            outcome.IsSelected = true;
            returnValue.Add(outcome);
            outcome = new Outcome();
            outcome.OutcomeTypeId = 1;
            outcome.Title = "Study 6";
            outcome.Data1 = 56;
            outcome.Data2 = 57;
            outcome.Data3 = 18.3;
            outcome.Data4 = 18.18;
            outcome.Data5 = 1.2;
            outcome.Data6 = 1.3;
            outcome.IsSelected = true;
            returnValue.Add(outcome);
            returnValue.RaiseListChangedEvents = true;
            return returnValue;
        }

        protected void DataPortal_Fetch()
        {

        }
#endif

    }
}
