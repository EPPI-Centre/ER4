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
using Csla.DataPortalClient;
using Newtonsoft.Json;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
#if (!CSLA_NETCORE)
using MetaGraphs.ForestPlot;
#endif
using System.Drawing;
#else
using System.Windows.Media.Imaging; 
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Outcome : BusinessBase<Outcome>
    {
#if SILVERLIGHT
    public Outcome() { OutcomeCodes = OutcomeItemAttributeList.NewOutcomeItemAttributeList(); }

        
#else
        //private Outcome() { }
        public Outcome() { OutcomeCodes = OutcomeItemAttributeList.NewOutcomeItemAttributeList(); }
#endif

        public override string ToString()
        {
            return Title; 
        }

        internal static Outcome NewOutcome()
        {
            Outcome returnValue = new Outcome();
            //returnValue.ValidationRules.CheckRules();
            return returnValue;
        }

        public static void GetOutcome(Int64 Id, EventHandler<DataPortalResult<Item>> handler)
        {
            DataPortal<Item> dp = new DataPortal<Item>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<Item, Int64>(Id));
        }

        public static readonly PropertyInfo<int> OutcomeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("OutcomeId", "OutcomeId"));
        [JsonProperty]
        public int OutcomeId
        {
            get
            {
                return GetProperty(OutcomeIdProperty);
            }
        }

        public static readonly PropertyInfo<Int64> ItemSetIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemSetId", "ItemSetId"));

        [JsonProperty]
        public Int64 ItemSetId
        {
            get
            {
                return GetProperty(ItemSetIdProperty);
            }
            set
            {
                SetProperty(ItemSetIdProperty, value);
            }
        }

        public static readonly PropertyInfo<int> OutcomeTypeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("OutcomeTypeId", "OutcomeTypeId"));
        public int OutcomeTypeId
        {
            get
            {
                return GetProperty(OutcomeTypeIdProperty);
            }
            set
            {
                SetProperty(OutcomeTypeIdProperty, value);
                this.SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<int> ManuallyEnteredOutcomeTypeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ManuallyEnteredOutcomeTypeId", "ManuallyEnteredOutcomeTypeId"));
        public int ManuallyEnteredOutcomeTypeId
        {
            get
            {
                return GetProperty(ManuallyEnteredOutcomeTypeIdProperty);
            }
            set
            {
                SetProperty(ManuallyEnteredOutcomeTypeIdProperty, value);
            }
        }

        // This combines the manually entered with the other outcome types in order that we can present a unified type
        public static readonly PropertyInfo<int> UnifiedOutcomeTypeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("UnifiedOutcomeTypeId", "UnifiedOutcomeTypeId"));
        public int UnifiedOutcomeTypeId
        {
            get
            {
                return GetProperty(UnifiedOutcomeTypeIdProperty);
            }
            set
            {
                SetProperty(UnifiedOutcomeTypeIdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> OutcomeTypeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("OutcomeTypeName", "OutcomeTypeName"));
        [JsonProperty]
        public string OutcomeTypeName
        {
            get
            {
                return GetProperty(OutcomeTypeNameProperty);
            }
            set
            {
                SetProperty(OutcomeTypeNameProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> ItemAttributeIdInterventionProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemAttributeIdIntervention", "ItemAttributeIdIntervention"));
        [JsonProperty]
        public Int64 ItemAttributeIdIntervention
        {
            get
            {
                return GetProperty(ItemAttributeIdInterventionProperty);
            }
            set
            {
                SetProperty(ItemAttributeIdInterventionProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> ItemAttributeIdControlProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemAttributeIdControl", "ItemAttributeIdControl"));
        [JsonProperty]
        public Int64 ItemAttributeIdControl
        {
            get
            {
                return GetProperty(ItemAttributeIdControlProperty);
            }
            set
            {
                SetProperty(ItemAttributeIdControlProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> ItemAttributeIdOutcomeProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemAttributeIdOutcome", "ItemAttributeIdOutcome"));
        [JsonProperty]
        public Int64 ItemAttributeIdOutcome
        {
            get
            {
                return GetProperty(ItemAttributeIdOutcomeProperty);
            }
            set
            {
                SetProperty(ItemAttributeIdOutcomeProperty, value);
            }
        }

        public static readonly PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title", string.Empty));
        [JsonProperty]
        public string Title
        {
            get
            {
                return GetProperty(TitleProperty);
            }
            set
            {
                SetProperty(TitleProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ShortTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ShortTitle", "ShortTitle", string.Empty));
        [JsonProperty]
        public string ShortTitle
        {
            get
            {
                return GetProperty(ShortTitleProperty);
            }
            set
            {
                SetProperty(ShortTitleProperty, value);
            }
        }

        public static readonly PropertyInfo<string> OutcomeDescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("OutcomeDescription", "OutcomeDescription", string.Empty));
        [JsonProperty]
        public string OutcomeDescription
        {
            get
            {
                return GetProperty(OutcomeDescriptionProperty);
            }
            set
            {
                SetProperty(OutcomeDescriptionProperty, value);
            }
        }

        public static readonly PropertyInfo<double> Data1Property = RegisterProperty<double>(new PropertyInfo<double>("Data1", "Data1"));
        [JsonProperty]
        public double Data1
        {
            get
            {
                return GetProperty(Data1Property);
            }
            set
            {
                SetProperty(Data1Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<double> Data2Property = RegisterProperty<double>(new PropertyInfo<double>("Data2", "Data2"));
        [JsonProperty]
        public double Data2
        {
            get
            {
                return GetProperty(Data2Property);
            }
            set
            {
                SetProperty(Data2Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<double> Data3Property = RegisterProperty<double>(new PropertyInfo<double>("Data3", "Data3"));
        [JsonProperty]
        public double Data3
        {
            get
            {
                return GetProperty(Data3Property);
            }
            set
            {
                SetProperty(Data3Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<double> Data4Property = RegisterProperty<double>(new PropertyInfo<double>("Data4", "Data4"));
        [JsonProperty]
        public double Data4
        {
            get
            {
                return GetProperty(Data4Property);
            }
            set
            {
                SetProperty(Data4Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<double> Data5Property = RegisterProperty<double>(new PropertyInfo<double>("Data5", "Data5"));
        [JsonProperty]
        public double Data5
        {
            get
            {
                return GetProperty(Data5Property);
            }
            set
            {
                SetProperty(Data5Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<double> Data6Property = RegisterProperty<double>(new PropertyInfo<double>("Data6", "Data6"));
        [JsonProperty]
        public double Data6
        {
            get
            {
                return GetProperty(Data6Property);
            }
            set
            {
                SetProperty(Data6Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<double> Data7Property = RegisterProperty<double>(new PropertyInfo<double>("Data7", "Data7"));
        [JsonProperty]
        public double Data7
        {
            get
            {
                return GetProperty(Data7Property);
            }
            set
            {
                SetProperty(Data7Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<double> Data8Property = RegisterProperty<double>(new PropertyInfo<double>("Data8", "Data8"));
        [JsonProperty]
        public double Data8
        {
            get
            {
                return GetProperty(Data8Property);
            }
            set
            {
                SetProperty(Data8Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<double> Data9Property = RegisterProperty<double>(new PropertyInfo<double>("Data9", "Data9"));
        [JsonProperty]
        public double Data9
        {
            get
            {
                return GetProperty(Data9Property);
            }
            set
            {
                SetProperty(Data9Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<double> Data10Property = RegisterProperty<double>(new PropertyInfo<double>("Data10", "Data10"));
        [JsonProperty]
        public double Data10
        {
            get
            {
                return GetProperty(Data10Property);
            }
            set
            {
                SetProperty(Data10Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<double> Data11Property = RegisterProperty<double>(new PropertyInfo<double>("Data11", "Data11"));
        [JsonProperty]
        public double Data11
        {
            get
            {
                return GetProperty(Data11Property);
            }
            set
            {
                SetProperty(Data11Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<double> Data12Property = RegisterProperty<double>(new PropertyInfo<double>("Data12", "Data12"));
        [JsonProperty]
        public double Data12
        {
            get
            {
                return GetProperty(Data12Property);
            }
            set
            {
                SetProperty(Data12Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<double> Data13Property = RegisterProperty<double>(new PropertyInfo<double>("Data13", "Data13"));
        [JsonProperty]
        public double Data13
        {
            get
            {
                return GetProperty(Data13Property);
            }
            set
            {
                SetProperty(Data13Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<double> Data14Property = RegisterProperty<double>(new PropertyInfo<double>("Data14", "Data14"));
        [JsonProperty]
        public double Data14
        {
            get
            {
                return GetProperty(Data14Property);
            }
            set
            {
                SetProperty(Data14Property, value);
                SetCalculatedValues();
            }
        }

        public static readonly PropertyInfo<string> InterventionTextProperty = RegisterProperty<string>(new PropertyInfo<string>("InterventionText", "InterventionText", string.Empty));
        [JsonProperty]
        public string InterventionText
        {
            get
            {
                return GetProperty(InterventionTextProperty);
            }
            set
            {
                SetProperty(InterventionTextProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ControlTextProperty = RegisterProperty<string>(new PropertyInfo<string>("ControlText", "ControlText", string.Empty));
        [JsonProperty]
        public string ControlText
        {
            get
            {
                return GetProperty(ControlTextProperty);
            }
            set
            {
                SetProperty(ControlTextProperty, value);
            }
        }

        public static readonly PropertyInfo<string> OutcomeTextProperty = RegisterProperty<string>(new PropertyInfo<string>("OutcomeText", "OutcomeText", string.Empty));
        [JsonProperty]
        public string OutcomeText
        {
            get
            {
                return GetProperty(OutcomeTextProperty);
            }
            set
            {
                SetProperty(OutcomeTextProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> IsSelectedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsSelected", "IsSelected"));
        public bool IsSelected
        {
            get
            {
                return GetProperty(IsSelectedProperty);
            }
            set
            {
                if (CanSelect)
                {
                    SetProperty(IsSelectedProperty, value);
                }
                else
                {
                    SetProperty(IsSelectedProperty, false);
                }
            }
        }

        public static readonly PropertyInfo<bool> CanSelectProperty = RegisterProperty<bool>(new PropertyInfo<bool>("CanSelect", "CanSelect"));
        public bool CanSelect
        {
            get
            {
                return GetProperty(CanSelectProperty);
            }
            set
            {
                SetProperty(CanSelectProperty, value);
            }
        }

        /* Meta-analysis types:
        *     0: Continuous: d (Hedges g)
        *     1: Continuous: r
        *     2: Binary: odds ratio
        *     3: Binary: risk ratio
        *     4: Binary: risk difference
        *     5: Binary: diagnostic test OR
        *     6: Binary: Peto OR
        *     7: Continuous: mean difference
        */
        public void updateCanSelect(int MAType)
        {
            CanSelect = false;
            if (SEES == 0 || double.IsNaN(SEES))
            {
                return;
            }
            switch (MAType)
            {
                // Hedges g
                case 0: if (UnifiedOutcomeTypeId == 1 || UnifiedOutcomeTypeId == 3 || UnifiedOutcomeTypeId == 4 || UnifiedOutcomeTypeId == 5) CanSelect = true;
                    break;
                // correlation coefficient
                case 1: if (UnifiedOutcomeTypeId == 7) CanSelect = true;
                    break;
                // mean difference
                case 7: if (UnifiedOutcomeTypeId == 1 || UnifiedOutcomeTypeId == 3 || UnifiedOutcomeTypeId == 4 || UnifiedOutcomeTypeId == 5) CanSelect = true;
                    break; 
                // 2, 3, 4, 5, 6 - binary outcomes
                default: if (UnifiedOutcomeTypeId == 2) CanSelect = true;
                    break;
            }
        }

        [NotUndoable]
        public static readonly PropertyInfo<OutcomeItemAttributeList> OutcomeCodesProperty = RegisterProperty<OutcomeItemAttributeList>(new PropertyInfo<OutcomeItemAttributeList>("OutcomeCodes", "OutcomeCodes"));

        public OutcomeItemAttributeList OutcomeCodes
        {
            get
            {
                return GetProperty(OutcomeCodesProperty);
            }
            set
            {
                SetProperty(OutcomeCodesProperty, value);
            }
        }

        /**** OUTCOME CLASSIFICATION CODES - NORMALISED FOR GRIDVIEW *****/

        public static readonly PropertyInfo<int> occ1Property = RegisterProperty<int>(new PropertyInfo<int>("occ1", "occ1"));
        public int occ1
        {
            get
            {
                return GetProperty(occ1Property);
            }
            set
            {
                SetProperty(occ1Property, value);
            }
        }

       public static readonly PropertyInfo<int> occ2Property = RegisterProperty<int>(new PropertyInfo<int>("occ2", "occ2"));
        public int occ2
        {
            get
            {
                return GetProperty(occ2Property);
            }
            set
            {
                SetProperty(occ2Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ3Property = RegisterProperty<int>(new PropertyInfo<int>("occ3", "occ3"));
        public int occ3
        {
            get
            {
                return GetProperty(occ3Property);
            }
            set
            {
                SetProperty(occ3Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ4Property = RegisterProperty<int>(new PropertyInfo<int>("occ4", "occ4"));
        public int occ4
        {
            get
            {
                return GetProperty(occ4Property);
            }
            set
            {
                SetProperty(occ4Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ5Property = RegisterProperty<int>(new PropertyInfo<int>("occ5", "occ5"));
        public int occ5
        {
            get
            {
                return GetProperty(occ5Property);
            }
            set
            {
                SetProperty(occ5Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ6Property = RegisterProperty<int>(new PropertyInfo<int>("occ6", "occ6"));
        public int occ6
        {
            get
            {
                return GetProperty(occ6Property);
            }
            set
            {
                SetProperty(occ6Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ7Property = RegisterProperty<int>(new PropertyInfo<int>("occ7", "occ7"));
        public int occ7
        {
            get
            {
                return GetProperty(occ7Property);
            }
            set
            {
                SetProperty(occ7Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ8Property = RegisterProperty<int>(new PropertyInfo<int>("occ8", "occ8"));
        public int occ8
        {
            get
            {
                return GetProperty(occ8Property);
            }
            set
            {
                SetProperty(occ8Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ9Property = RegisterProperty<int>(new PropertyInfo<int>("occ9", "occ9"));
        public int occ9
        {
            get
            {
                return GetProperty(occ9Property);
            }
            set
            {
                SetProperty(occ9Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ10Property = RegisterProperty<int>(new PropertyInfo<int>("occ10", "occ10"));
        public int occ10
        {
            get
            {
                return GetProperty(occ10Property);
            }
            set
            {
                SetProperty(occ10Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ11Property = RegisterProperty<int>(new PropertyInfo<int>("occ11", "occ11"));
        public int occ11
        {
            get
            {
                return GetProperty(occ11Property);
            }
            set
            {
                SetProperty(occ11Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ12Property = RegisterProperty<int>(new PropertyInfo<int>("occ12", "occ12"));
        public int occ12
        {
            get
            {
                return GetProperty(occ12Property);
            }
            set
            {
                SetProperty(occ12Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ13Property = RegisterProperty<int>(new PropertyInfo<int>("occ13", "occ13"));
        public int occ13
        {
            get
            {
                return GetProperty(occ13Property);
            }
            set
            {
                SetProperty(occ13Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ14Property = RegisterProperty<int>(new PropertyInfo<int>("occ14", "occ14"));
        public int occ14
        {
            get
            {
                return GetProperty(occ14Property);
            }
            set
            {
                SetProperty(occ14Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ15Property = RegisterProperty<int>(new PropertyInfo<int>("occ15", "occ15"));
        public int occ15
        {
            get
            {
                return GetProperty(occ15Property);
            }
            set
            {
                SetProperty(occ15Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ16Property = RegisterProperty<int>(new PropertyInfo<int>("occ16", "occ16"));
        public int occ16
        {
            get
            {
                return GetProperty(occ16Property);
            }
            set
            {
                SetProperty(occ16Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ17Property = RegisterProperty<int>(new PropertyInfo<int>("occ17", "occ17"));
        public int occ17
        {
            get
            {
                return GetProperty(occ17Property);
            }
            set
            {
                SetProperty(occ17Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ18Property = RegisterProperty<int>(new PropertyInfo<int>("occ18", "occ18"));
        public int occ18
        {
            get
            {
                return GetProperty(occ18Property);
            }
            set
            {
                SetProperty(occ18Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ19Property = RegisterProperty<int>(new PropertyInfo<int>("occ19", "occ19"));
        public int occ19
        {
            get
            {
                return GetProperty(occ19Property);
            }
            set
            {
                SetProperty(occ19Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ20Property = RegisterProperty<int>(new PropertyInfo<int>("occ20", "occ20"));
        public int occ20
        {
            get
            {
                return GetProperty(occ20Property);
            }
            set
            {
                SetProperty(occ20Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ21Property = RegisterProperty<int>(new PropertyInfo<int>("occ21", "occ21"));
        public int occ21
        {
            get
            {
                return GetProperty(occ21Property);
            }
            set
            {
                SetProperty(occ21Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ22Property = RegisterProperty<int>(new PropertyInfo<int>("occ22", "occ22"));
        public int occ22
        {
            get
            {
                return GetProperty(occ22Property);
            }
            set
            {
                SetProperty(occ22Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ23Property = RegisterProperty<int>(new PropertyInfo<int>("occ23", "occ23"));
        public int occ23
        {
            get
            {
                return GetProperty(occ23Property);
            }
            set
            {
                SetProperty(occ23Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ24Property = RegisterProperty<int>(new PropertyInfo<int>("occ24", "occ24"));
        public int occ24
        {
            get
            {
                return GetProperty(occ24Property);
            }
            set
            {
                SetProperty(occ24Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ25Property = RegisterProperty<int>(new PropertyInfo<int>("occ25", "occ25"));
        public int occ25
        {
            get
            {
                return GetProperty(occ25Property);
            }
            set
            {
                SetProperty(occ25Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ26Property = RegisterProperty<int>(new PropertyInfo<int>("occ26", "occ26"));
        public int occ26
        {
            get
            {
                return GetProperty(occ26Property);
            }
            set
            {
                SetProperty(occ26Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ27Property = RegisterProperty<int>(new PropertyInfo<int>("occ27", "occ27"));
        public int occ27
        {
            get
            {
                return GetProperty(occ27Property);
            }
            set
            {
                SetProperty(occ27Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ28Property = RegisterProperty<int>(new PropertyInfo<int>("occ28", "occ28"));
        public int occ28
        {
            get
            {
                return GetProperty(occ28Property);
            }
            set
            {
                SetProperty(occ28Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ29Property = RegisterProperty<int>(new PropertyInfo<int>("occ29", "occ29"));
        public int occ29
        {
            get
            {
                return GetProperty(occ29Property);
            }
            set
            {
                SetProperty(occ29Property, value);
            }
        }

        public static readonly PropertyInfo<int> occ30Property = RegisterProperty<int>(new PropertyInfo<int>("occ30", "occ30"));
        public int occ30
        {
            get
            {
                return GetProperty(occ30Property);
            }
            set
            {
                SetProperty(occ30Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa1Property = RegisterProperty<long>(new PropertyInfo<long>("aa1", "aa1"));
        public long aa1
        {
            get
            {
                return GetProperty(aa1Property);
            }
            set
            {
                SetProperty(aa1Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa2Property = RegisterProperty<long>(new PropertyInfo<long>("aa2", "aa2"));
        public long aa2
        {
            get
            {
                return GetProperty(aa2Property);
            }
            set
            {
                SetProperty(aa2Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa3Property = RegisterProperty<long>(new PropertyInfo<long>("aa3", "aa3"));
        public long aa3
        {
            get
            {
                return GetProperty(aa3Property);
            }
            set
            {
                SetProperty(aa3Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa4Property = RegisterProperty<long>(new PropertyInfo<long>("aa4", "aa4"));
        public long aa4
        {
            get
            {
                return GetProperty(aa4Property);
            }
            set
            {
                SetProperty(aa4Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa5Property = RegisterProperty<long>(new PropertyInfo<long>("aa5", "aa5"));
        public long aa5
        {
            get
            {
                return GetProperty(aa5Property);
            }
            set
            {
                SetProperty(aa5Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa6Property = RegisterProperty<long>(new PropertyInfo<long>("aa6", "aa6"));
        public long aa6
        {
            get
            {
                return GetProperty(aa6Property);
            }
            set
            {
                SetProperty(aa6Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa7Property = RegisterProperty<long>(new PropertyInfo<long>("aa7", "aa7"));
        public long aa7
        {
            get
            {
                return GetProperty(aa7Property);
            }
            set
            {
                SetProperty(aa7Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa8Property = RegisterProperty<long>(new PropertyInfo<long>("aa8", "aa8"));
        public long aa8
        {
            get
            {
                return GetProperty(aa8Property);
            }
            set
            {
                SetProperty(aa8Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa9Property = RegisterProperty<long>(new PropertyInfo<long>("aa9", "aa9"));
        public long aa9
        {
            get
            {
                return GetProperty(aa9Property);
            }
            set
            {
                SetProperty(aa9Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa10Property = RegisterProperty<long>(new PropertyInfo<long>("aa10", "aa10"));
        public long aa10
        {
            get
            {
                return GetProperty(aa10Property);
            }
            set
            {
                SetProperty(aa10Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa11Property = RegisterProperty<long>(new PropertyInfo<long>("aa11", "aa11"));
        public long aa11
        {
            get
            {
                return GetProperty(aa11Property);
            }
            set
            {
                SetProperty(aa11Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa12Property = RegisterProperty<long>(new PropertyInfo<long>("aa12", "aa12"));
        public long aa12
        {
            get
            {
                return GetProperty(aa12Property);
            }
            set
            {
                SetProperty(aa12Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa13Property = RegisterProperty<long>(new PropertyInfo<long>("aa13", "aa13"));
        public long aa13
        {
            get
            {
                return GetProperty(aa13Property);
            }
            set
            {
                SetProperty(aa13Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa14Property = RegisterProperty<long>(new PropertyInfo<long>("aa14", "aa14"));
        public long aa14
        {
            get
            {
                return GetProperty(aa14Property);
            }
            set
            {
                SetProperty(aa14Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa15Property = RegisterProperty<long>(new PropertyInfo<long>("aa15", "aa15"));
        public long aa15
        {
            get
            {
                return GetProperty(aa15Property);
            }
            set
            {
                SetProperty(aa15Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa16Property = RegisterProperty<long>(new PropertyInfo<long>("aa16", "aa16"));
        public long aa16
        {
            get
            {
                return GetProperty(aa16Property);
            }
            set
            {
                SetProperty(aa16Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa17Property = RegisterProperty<long>(new PropertyInfo<long>("aa17", "aa17"));
        public long aa17
        {
            get
            {
                return GetProperty(aa17Property);
            }
            set
            {
                SetProperty(aa17Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa18Property = RegisterProperty<long>(new PropertyInfo<long>("aa18", "aa18"));
        public long aa18
        {
            get
            {
                return GetProperty(aa18Property);
            }
            set
            {
                SetProperty(aa18Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa19Property = RegisterProperty<long>(new PropertyInfo<long>("aa19", "aa19"));
        public long aa19
        {
            get
            {
                return GetProperty(aa19Property);
            }
            set
            {
                SetProperty(aa19Property, value);
            }
        }

        public static readonly PropertyInfo<long> aa20Property = RegisterProperty<long>(new PropertyInfo<long>("aa20", "aa20"));
        public long aa20
        {
            get
            {
                return GetProperty(aa20Property);
            }
            set
            {
                SetProperty(aa20Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq1Property = RegisterProperty<string>(new PropertyInfo<string>("aq1", "aq1"));
        public string aq1
        {
            get
            {
                return GetProperty(aq1Property);
            }
            set
            {
                SetProperty(aq1Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq2Property = RegisterProperty<string>(new PropertyInfo<string>("aq2", "aq2"));
        public string aq2
        {
            get
            {
                return GetProperty(aq2Property);
            }
            set
            {
                SetProperty(aq2Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq3Property = RegisterProperty<string>(new PropertyInfo<string>("aq3", "aq3"));
        public string aq3
        {
            get
            {
                return GetProperty(aq3Property);
            }
            set
            {
                SetProperty(aq3Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq4Property = RegisterProperty<string>(new PropertyInfo<string>("aq4", "aq4"));
        public string aq4
        {
            get
            {
                return GetProperty(aq4Property);
            }
            set
            {
                SetProperty(aq4Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq5Property = RegisterProperty<string>(new PropertyInfo<string>("aq5", "aq5"));
        public string aq5
        {
            get
            {
                return GetProperty(aq5Property);
            }
            set
            {
                SetProperty(aq5Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq6Property = RegisterProperty<string>(new PropertyInfo<string>("aq6", "aq6"));
        public string aq6
        {
            get
            {
                return GetProperty(aq6Property);
            }
            set
            {
                SetProperty(aq6Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq7Property = RegisterProperty<string>(new PropertyInfo<string>("aq7", "aq7"));
        public string aq7
        {
            get
            {
                return GetProperty(aq7Property);
            }
            set
            {
                SetProperty(aq7Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq8Property = RegisterProperty<string>(new PropertyInfo<string>("aq8", "aq8"));
        public string aq8
        {
            get
            {
                return GetProperty(aq8Property);
            }
            set
            {
                SetProperty(aq8Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq9Property = RegisterProperty<string>(new PropertyInfo<string>("aq9", "aq9"));
        public string aq9
        {
            get
            {
                return GetProperty(aq9Property);
            }
            set
            {
                SetProperty(aq9Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq10Property = RegisterProperty<string>(new PropertyInfo<string>("aq10", "aq10"));
        public string aq10
        {
            get
            {
                return GetProperty(aq10Property);
            }
            set
            {
                SetProperty(aq10Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq11Property = RegisterProperty<string>(new PropertyInfo<string>("aq11", "aq11"));
        public string aq11
        {
            get
            {
                return GetProperty(aq11Property);
            }
            set
            {
                SetProperty(aq11Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq12Property = RegisterProperty<string>(new PropertyInfo<string>("aq12", "aq12"));
        public string aq12
        {
            get
            {
                return GetProperty(aq12Property);
            }
            set
            {
                SetProperty(aq12Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq13Property = RegisterProperty<string>(new PropertyInfo<string>("aq13", "aq13"));
        public string aq13
        {
            get
            {
                return GetProperty(aq13Property);
            }
            set
            {
                SetProperty(aq13Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq14Property = RegisterProperty<string>(new PropertyInfo<string>("aq14", "aq14"));
        public string aq14
        {
            get
            {
                return GetProperty(aq14Property);
            }
            set
            {
                SetProperty(aq14Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq15Property = RegisterProperty<string>(new PropertyInfo<string>("aq15", "aq15"));
        public string aq15
        {
            get
            {
                return GetProperty(aq15Property);
            }
            set
            {
                SetProperty(aq15Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq16Property = RegisterProperty<string>(new PropertyInfo<string>("aq16", "aq16"));
        public string aq16
        {
            get
            {
                return GetProperty(aq16Property);
            }
            set
            {
                SetProperty(aq16Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq17Property = RegisterProperty<string>(new PropertyInfo<string>("aq17", "aq17"));
        public string aq17
        {
            get
            {
                return GetProperty(aq17Property);
            }
            set
            {
                SetProperty(aq17Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq18Property = RegisterProperty<string>(new PropertyInfo<string>("aq18", "aq18"));
        public string aq18
        {
            get
            {
                return GetProperty(aq18Property);
            }
            set
            {
                SetProperty(aq18Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq19Property = RegisterProperty<string>(new PropertyInfo<string>("aq19", "aq19"));
        public string aq19
        {
            get
            {
                return GetProperty(aq19Property);
            }
            set
            {
                SetProperty(aq19Property, value);
            }
        }

        public static readonly PropertyInfo<string> aq20Property = RegisterProperty<string>(new PropertyInfo<string>("aq20", "aq20"));
        public string aq20
        {
            get
            {
                return GetProperty(aq20Property);
            }
            set
            {
                SetProperty(aq20Property, value);
            }
        }

       
        

        /*
         * For N, Means and SD: Data1: N 1, Data2: N 2, Data3: mean 1, Data4: mean 2, Data5: SD 1, Data6: SD 2
         * 
         * 
         */

        public static readonly PropertyInfo<double> feWeightProperty = RegisterProperty<double>(new PropertyInfo<double>("feWeight", "feWeight"));
        [JsonProperty]
        public double feWeight // just used when sending objects to the forest plotter.
        {
            get
            {
                return GetProperty(feWeightProperty);
            }
            set
            {
                SetProperty(feWeightProperty, value);
            }
        }

        public static readonly PropertyInfo<double> reWeightProperty = RegisterProperty<double>(new PropertyInfo<double>("reWeight", "reWeight"));
        [JsonProperty]
        public double reWeight // just used when sending objects to the forest plotter.
        {
            get
            {
                return GetProperty(reWeightProperty);
            }
            set
            {
                SetProperty(reWeightProperty, value);
            }
        }

        public void SetCalculatedValues()
        {
            SetEffectSizes();
            switch (OutcomeTypeId)
            {
                case 0: // manual entry
                    SetProperty(ESDescProperty, "Effect size");
                    SetProperty(SEDescProperty, "SE");
                    SetProperty(NRowsProperty, 6);
                    SetProperty(Data1DescProperty, "SMD");
                    SetProperty(Data2DescProperty, "standard error");
                    SetProperty(Data3DescProperty, "r");
                    SetProperty(Data4DescProperty, "SE (Z transformed)");
                    SetProperty(Data5DescProperty, "odds ratio");
                    SetProperty(Data6DescProperty, "SE (log OR)");
                    SetProperty(Data7DescProperty, "risk ratio");
                    SetProperty(Data8DescProperty, "SE (log RR)");
                    SetProperty(Data9DescProperty, "");
                    SetProperty(Data10DescProperty, "");
                    SetProperty(Data11DescProperty, "risk difference");
                    SetProperty(Data12DescProperty, "standard error");
                    SetProperty(Data13DescProperty, "mean difference");
                    SetProperty(Data14DescProperty, "standard error");
                    break;

                case 1: // n, mean, SD
                    SetProperty(ESDescProperty, "SMD");
                    SetProperty(SEDescProperty, "SE");
                    SetProperty(NRowsProperty, 3);
                    SetProperty(Data1DescProperty, "Group 1 N");
                    SetProperty(Data2DescProperty, "Group 2 N");
                    SetProperty(Data3DescProperty, "Group 1 mean");
                    SetProperty(Data4DescProperty, "Group 2 mean");
                    SetProperty(Data5DescProperty, "Group 1 SD");
                    SetProperty(Data6DescProperty, "Group 2 SD");
                    SetProperty(Data7DescProperty, "");
                    SetProperty(Data8DescProperty, "");
                    SetProperty(Data9DescProperty, "");
                    SetProperty(Data10DescProperty, "");
                    SetProperty(Data11DescProperty, "");
                    SetProperty(Data12DescProperty, "");
                    SetProperty(Data13DescProperty, "");
                    SetProperty(Data14DescProperty, "");
                    SetProperty(OutcomeTypeNameProperty, "Continuous");
                    break;

                case 2: // binary 2 x 2 table
                    SetProperty(ESDescProperty, "OR");
                    SetProperty(SEDescProperty, "SE (log OR)");
                    SetProperty(NRowsProperty, 2);
                    SetProperty(Data1DescProperty, "Group 1 events");
                    SetProperty(Data2DescProperty, "Group 2 events");
                    SetProperty(Data3DescProperty, "Group 1 no events");
                    SetProperty(Data4DescProperty, "Group 2 no events");
                    SetProperty(Data5DescProperty, "");
                    SetProperty(Data6DescProperty, "");
                    SetProperty(Data7DescProperty, "");
                    SetProperty(Data8DescProperty, "");
                    SetProperty(Data9DescProperty, "");
                    SetProperty(Data10DescProperty, "");
                    SetProperty(Data11DescProperty, "");
                    SetProperty(Data12DescProperty, "");
                    SetProperty(Data13DescProperty, "");
                    SetProperty(Data14DescProperty, "");
                    SetProperty(OutcomeTypeNameProperty, "Binary");
                    break;

                case 3: //n, mean SE
                    SetProperty(ESDescProperty, "SMD");
                    SetProperty(SEDescProperty, "SE");
                    SetProperty(NRowsProperty, 3);
                    SetProperty(Data1DescProperty, "Group 1 N");
                    SetProperty(Data2DescProperty, "Group 2 N");
                    SetProperty(Data3DescProperty, "Group 1 mean");
                    SetProperty(Data4DescProperty, "Group 2 mean");
                    SetProperty(Data5DescProperty, "Group 1 SE");
                    SetProperty(Data6DescProperty, "Group 2 SE");
                    SetProperty(Data7DescProperty, "");
                    SetProperty(Data8DescProperty, "");
                    SetProperty(Data9DescProperty, "");
                    SetProperty(Data10DescProperty, "");
                    SetProperty(Data11DescProperty, "");
                    SetProperty(Data12DescProperty, "");
                    SetProperty(Data13DescProperty, "");
                    SetProperty(Data14DescProperty, "");
                    SetProperty(OutcomeTypeNameProperty, "Continuous");
                    break;

                case 4: //n, mean CI
                    SetProperty(ESDescProperty, "SMD");
                    SetProperty(SEDescProperty, "SE");
                    SetProperty(NRowsProperty, 4);
                    SetProperty(Data1DescProperty, "Group 1 N");
                    SetProperty(Data2DescProperty, "Group 2 N");
                    SetProperty(Data3DescProperty, "Group 1 mean");
                    SetProperty(Data4DescProperty, "Group 2 mean");
                    SetProperty(Data5DescProperty, "Group 1 CI lower");
                    SetProperty(Data6DescProperty, "Group 2 CI lower");
                    SetProperty(Data7DescProperty, "Group 1 CI upper");
                    SetProperty(Data8DescProperty, "Group 2 CI upper");
                    SetProperty(Data9DescProperty, "");
                    SetProperty(Data10DescProperty, "");
                    SetProperty(Data11DescProperty, "");
                    SetProperty(Data12DescProperty, "");
                    SetProperty(Data13DescProperty, "");
                    SetProperty(Data14DescProperty, "");
                    SetProperty(OutcomeTypeNameProperty, "Continuous");
                    break;

                case 5: //n, t or p value
                    SetProperty(ESDescProperty, "SMD");
                    SetProperty(SEDescProperty, "SE");
                    SetProperty(NRowsProperty, 2);
                    SetProperty(Data1DescProperty, "Group 1 N");
                    SetProperty(Data2DescProperty, "Group 2 N");
                    SetProperty(Data3DescProperty, "t-value");
                    SetProperty(Data4DescProperty, "p-value");
                    SetProperty(Data5DescProperty, "");
                    SetProperty(Data6DescProperty, "");
                    SetProperty(Data7DescProperty, "");
                    SetProperty(Data8DescProperty, "");
                    SetProperty(Data9DescProperty, "");
                    SetProperty(Data10DescProperty, "");
                    SetProperty(Data11DescProperty, "");
                    SetProperty(Data12DescProperty, "");
                    SetProperty(Data13DescProperty, "");
                    SetProperty(Data14DescProperty, "");
                    SetProperty(OutcomeTypeNameProperty, "Continuous");
                    break;

                case 6: // diagnostic test 2 x 2 table
                    SetProperty(ESDescProperty, "Diagnostic OR");
                    SetProperty(SEDescProperty, "SE");
                    SetProperty(NRowsProperty, 2);
                    SetProperty(Data1DescProperty, "True positive");
                    SetProperty(Data2DescProperty, "False positive");
                    SetProperty(Data3DescProperty, "False negative");
                    SetProperty(Data4DescProperty, "True negative");
                    SetProperty(Data5DescProperty, "");
                    SetProperty(Data6DescProperty, "");
                    SetProperty(Data7DescProperty, "");
                    SetProperty(Data8DescProperty, "");
                    SetProperty(Data9DescProperty, "");
                    SetProperty(Data10DescProperty, "");
                    SetProperty(Data11DescProperty, "");
                    SetProperty(Data12DescProperty, "");
                    SetProperty(Data13DescProperty, "");
                    SetProperty(Data14DescProperty, "");
                    SetProperty(OutcomeTypeNameProperty, "Binary");
                    break;

                case 7: // correlation coefficient r
                    SetProperty(ESDescProperty, "r");
                    SetProperty(SEDescProperty, "SE (Z transformed)");
                    SetProperty(NRowsProperty, 1);
                    SetProperty(Data1DescProperty, "group(s) size");
                    SetProperty(Data2DescProperty, "correlation");
                    SetProperty(Data3DescProperty, "");
                    SetProperty(Data4DescProperty, "");
                    SetProperty(Data5DescProperty, "");
                    SetProperty(Data6DescProperty, "");
                    SetProperty(Data7DescProperty, "");
                    SetProperty(Data8DescProperty, "");
                    SetProperty(Data9DescProperty, "");
                    SetProperty(Data10DescProperty, "");
                    SetProperty(Data11DescProperty, "");
                    SetProperty(Data12DescProperty, "");
                    SetProperty(Data13DescProperty, "");
                    SetProperty(Data14DescProperty, "");
                    SetProperty(OutcomeTypeNameProperty, "Correlation");
                    break;

                default:
                    SetProperty(Data1DescProperty, "");
                    SetProperty(Data2DescProperty, "");
                    SetProperty(Data3DescProperty, "");
                    SetProperty(Data4DescProperty, "");
                    SetProperty(Data5DescProperty, "");
                    SetProperty(Data6DescProperty, "");
                    SetProperty(Data7DescProperty, "");
                    SetProperty(Data8DescProperty, "");
                    SetProperty(Data9DescProperty, "");
                    SetProperty(Data10DescProperty, "");
                    SetProperty(Data11DescProperty, "");
                    SetProperty(Data12DescProperty, "");
                    SetProperty(Data13DescProperty, "");
                    SetProperty(Data14DescProperty, "");
                    SetProperty(OutcomeTypeNameProperty, "");
                    break;
            }
        }

        public string GetOutcomeHeaders()
        {
            string retVal = "<tr bgcolor='silver'><td>Title</td><td>Description</td><td>Outcome</td><td>Intervention</td><td>Control</td><td>Type</td>";
            switch (OutcomeTypeId)
            {
                case 0: // manual entry
                    retVal += "<td>SMD</td><td>SE</td><td>r</td><td>SE</td><td>Odds ratio</td><td>SE</td><td>Risk ratio</td><td>SE</td><td>Risk difference</td><td>SE</td><td>Mean difference</td><td>SE</td>";
                    break;

                case 1: // n, mean, SD
                    retVal += "<td>Group 1 N</td><td>Group 2 N</td><td>Group 1 mean</td><td>Group 2 mean</td><td>Group 1 SD</td>" + 
                        "<td>Group 2 SD</td><td>SMD</td><td>SE</td>";
                    break;

                case 2: // binary 2 x 2 table
                    retVal += "<td>Group 1 events</td><td>Group 2 events</td><td>Group 1 no events</td><td>Group 2 no events</td><td>Odds ratio</td><td>SE (log OR)</td>";
                    break;

                case 3: //n, mean SE
                    retVal += "<td>Group 1 N</td><td>Group 2 N</td><td>Group 1 mean</td><td>Group 2 mean</td><td>Group 1 SE</td>" +
                        "<td>Group 2 SE</td><td>SMD</td><td>SE</td>";
                    break;

                case 4: //n, mean CI
                    retVal += "<td>Group 1 N</td><td>Group 2 N</td><td>Group 1 mean</td><td>Group 2 mean</td><td>Group 1 CI lower</td>" +
                        "<td>Group 1 CI upper</td><td>Group 2 CI lower</td><td>Group 2 CI upper</td><td>SMD</td><td>SE</td>";
                    break;

                case 5: //n, t or p value
                    retVal += "<td>Group 1 N</td><td>Group 2 N</td><td>Group 1 mean</td><td>Group 2 mean</td><td>t-value</td>" +
                        "<td>p-value</td><td>SMD</td><td>SE</td>";
                    break;

                case 6: // diagnostic test 2 x 2 table
                    retVal += "<td>True positive</td><td>False positive</td><td>False negative</td><td>True negative</td><td>Diagnostic odds ratio</td><td>SE (log dOR)</td>";
                    break;

                case 7: // correlation coeffiecient r
                    retVal += "<td>Group size</td><td>r</td><td>SE (Z transformed)</td>";
                    break;

                default:
                    break;
            }
            return retVal + "<td>Outcome Classifications</td></tr>";
        }

        public string GetOutcomeStats()
        {
            string retVal = "<tr><td>" + Title + "</td><td>" + OutcomeDescription.Replace("\r", "<br />") + "</td><td>" + OutcomeText + "</td><td>" + InterventionText +
                "</td><td>" + ControlText + "</td>";
            switch (OutcomeTypeId)
            {
                case 0: // manual entry
                    retVal += "<td>Manual entry</td>" +
                        "<td>" + Data1.ToString("G3") + "</td>" +
                        "<td>" + Data2.ToString("G3") + "</td>" +
                        "<td>" + Data3.ToString("G3") + "</td>" +
                        "<td>" + Data4.ToString("G3") + "</td>" +
                        "<td>" + Data5.ToString("G3") + "</td>" +
                        "<td>" + Data6.ToString("G3") + "</td>" +
                        "<td>" + Data7.ToString("G3") + "</td>" +
                        "<td>" + Data8.ToString("G3") + "</td>" +
                        "<td>" + Data11.ToString("G3") + "</td>" +
                        "<td>" + Data12.ToString("G3") + "</td>" +
                        "<td>" + Data13.ToString("G3") + "</td>" +
                        "<td>" + Data14.ToString("G3") + "</td>";
                    break;

                case 1: // n, mean, SD
                    retVal += "<td>Continuous: Ns, means and SD</td>" +
                        "<td>" + Data1.ToString("G3") + "</td>" +
                        "<td>" + Data2.ToString("G3") + "</td>" +
                        "<td>" + Data3.ToString("G3") + "</td>" +
                        "<td>" + Data4.ToString("G3") + "</td>" +
                        "<td>" + Data5.ToString("G3") + "</td>" +
                        "<td>" + Data6.ToString("G3") + "</td>" +
                        "<td>" + ES.ToString("G3") + "</td><td>" + SEES.ToString("G3") + "</td>";
                    break;

                case 2: // binary 2 x 2 table
                    retVal += "<td>Binary: 2 x 2 table</td>" +
                        "<td>" + Data1.ToString("G3") + "</td>" +
                        "<td>" + Data2.ToString("G3") + "</td>" +
                        "<td>" + Data3.ToString("G3") + "</td>" +
                        "<td>" + Data4.ToString("G3") + "</td>" +
                        "<td>" + ES.ToString("G3") + "</td><td>" + SEES.ToString("G3") + "</td>";
                    break;

                case 3: //n, mean SE
                    retVal += "<td>Continuous: N, Mean, SE</td>" +
                        "<td>" + Data1.ToString("G3") + "</td>" +
                        "<td>" + Data2.ToString("G3") + "</td>" +
                        "<td>" + Data3.ToString("G3") + "</td>" +
                        "<td>" + Data4.ToString("G3") + "</td>" +
                        "<td>" + Data5.ToString("G3") + "</td>" +
                        "<td>" + Data6.ToString("G3") + "</td>" +
                        "<td>" + ES.ToString("G3") + "</td><td>" + SEES.ToString("G3") + "</td>";
                    break;

                case 4: //n, mean CI
                    retVal += "<td>Continuous: N, Mean, CI</td>" +
                        "<td>" + Data1.ToString("G3") + "</td>" +
                        "<td>" + Data2.ToString("G3") + "</td>" +
                        "<td>" + Data3.ToString("G3") + "</td>" +
                        "<td>" + Data4.ToString("G3") + "</td>" +
                        "<td>" + Data5.ToString("G3") + "</td>" +
                        "<td>" + Data6.ToString("G3") + "</td>" +
                        "<td>" + Data7.ToString("G3") + "</td>" +
                        "<td>" + Data8.ToString("G3") + "</td>" +
                        "<td>" + ES.ToString("G3") + "</td><td>" + SEES.ToString("G3") + "</td>";
                    break;

                case 5: //n, t or p value
                    retVal += "<td>Continuous: N, t- or p-value</td>" +
                        "<td>" + Data1.ToString("G3") + "</td>" +
                        "<td>" + Data2.ToString("G3") + "</td>" +
                        "<td>" + Data3.ToString("G3") + "</td>" +
                        "<td>" + Data4.ToString("G3") + "</td>" +
                        "<td>" + ES.ToString("G3") + "</td><td>" + SEES.ToString("G3") + "</td>";
                    break;

                case 6: // binary 2 x 2 table
                    retVal += "<td>Diagnostic test: 2 x 2 table</td>" +
                        "<td>" + Data1.ToString("G3") + "</td>" +
                        "<td>" + Data2.ToString("G3") + "</td>" +
                        "<td>" + Data3.ToString("G3") + "</td>" +
                        "<td>" + Data4.ToString("G3") + "</td>" +
                        "<td>" + ES.ToString("G3") + "</td><td>" + SEES.ToString("G3") + "</td>";
                    break;

                case 7: // correlation coefficient r
                    retVal += "<td>Correlation coefficient r</td>" +
                        "<td>" + Data1.ToString("G3") + "</td>" +
                        "<td>" + Data2.ToString("G3") + "</td>" +
                        "<td>" + SEES.ToString("G3") + "</td>";
                    break;

                default:
                    break;
            }
            
            retVal += "<td>";
            foreach (OutcomeItemAttribute OIA in OutcomeCodes)
            {
                retVal += OIA.AttributeName + "<br>";
            }
            return retVal + "</td></tr>";
        }

        private void SetEffectSizes()
        {
            double es = 0;
            double se = 0;
            SetProperty(UnifiedOutcomeTypeIdProperty, OutcomeTypeId); // i.e. always the same as outcometypeid except for manual entry
            switch (OutcomeTypeId)
            {
                case 0: // manual entry
                    SetProperty(SMDProperty, Data1);
                    SetProperty(SESMDProperty, CorrectForClustering(Data2));
                    SetProperty(RProperty, Data3);
                    SetProperty(SERProperty, Data4);
                    SetProperty(OddsRatioProperty, Data5);
                    SetProperty(SEOddsRatioProperty, CorrectForClustering(Data6));
                    SetProperty(RiskRatioProperty, Data7);
                    SetProperty(SERiskRatioProperty, CorrectForClustering(Data8));
                    SetProperty(RiskDifferenceProperty, Data11);
                    SetProperty(SERiskDifferenceProperty, CorrectForClustering(Data12));
                    SetProperty(MeanDifferenceProperty, Data13);
                    SetProperty(SEMeanDifferenceProperty, CorrectForClustering(Data14));
                    SetESforManualEntry();
                    break;

                case 1: // n, mean, SD
                    SetProperty(SMDProperty, SmdFromNMeanSD());
                    SetProperty(SESMDProperty, CorrectForClustering(getSEforD(Data1, Data2, SMD)));
                    SetProperty(MeanDifferenceProperty, MeanDiff());
                    SetProperty(SEMeanDifferenceProperty, CorrectForClustering(getSEforMeanDiff(Data1, Data2, Data5, Data6)));
                    SetProperty(ESProperty, SMD);
                    SetProperty(SEESProperty, SESMD);
                    break;

                case 2: // binary 2 x 2 table
                    SetProperty(OddsRatioProperty, calcOddsRatio());
                    SetProperty(SEOddsRatioProperty, CorrectForClustering(calcOddsRatioSE()));
                    SetProperty(RiskRatioProperty, calcRiskRatio());
                    SetProperty(SERiskRatioProperty, CorrectForClustering(calcRiskRatioSE()));
                    SetProperty(RiskDifferenceProperty, calcRiskDifference());
                    SetProperty(SERiskDifferenceProperty, CorrectForClustering(calcRiskDifferenceSE()));
                    SetProperty(PetoORProperty, calcPetoOR());
                    SetProperty(SEPetoORProperty, CorrectForClustering(calcPetoORSE()));
                    SetProperty(ESProperty, OddsRatio);
                    SetProperty(SEESProperty, SEOddsRatio);
                    break;

                case 3: //n, mean SE
                    SetProperty(SMDProperty, SmdFromNMeanSe());
                    SetProperty(SESMDProperty, CorrectForClustering(getSEforD(Data1, Data2, SMD)));
                    SetProperty(MeanDifferenceProperty, MeanDiff());
                    SetProperty(SEMeanDifferenceProperty, CorrectForClustering(getSEforMeanDiff(Data1, Data2, Data5, Data6)));
                    SetProperty(ESProperty, SMD);
                    SetProperty(SEESProperty, SESMD);
                    break;

                case 4: //n, mean CI
                    SetProperty(SMDProperty, SmdFromNMeanCI());
                    SetProperty(SESMDProperty, CorrectForClustering(getSEforD(Data1, Data2, SMD)));
                    SetProperty(MeanDifferenceProperty, MeanDiff());
                    SetProperty(SEMeanDifferenceProperty, CorrectForClustering(getSEforMeanDiff(Data1, Data2, Data5, Data6)));
                    SetProperty(ESProperty, SMD);
                    SetProperty(SEESProperty, SESMD);
                    break;

                case 5: // N, t- or p-value
                    if (Data4 != 0)
                    {
                        SetProperty(SMDProperty, CorrectG(Data1, Data2, SmdFromP()));
                    }
                    else
                    {
                        SetProperty(SMDProperty, SmdFromT(Data3));
                    }
                    SetProperty(SESMDProperty, CorrectForClustering(getSEforD(Data1, Data2, SMD)));
                    SetProperty(MeanDifferenceProperty, MeanDiff());
                    SetProperty(SEMeanDifferenceProperty, CorrectForClustering(getSEforMeanDiff(Data1, Data2, Data5, Data6)));
                    SetProperty(ESProperty, SMD);
                    SetProperty(SEESProperty, SESMD);
                    break;

                case 6: // diagnostic binary 2 x 2 table
                    es = calcOddsRatio();
                    se = calcOddsRatioSE();
                    SetProperty(ESProperty, OddsRatio);
                    SetProperty(SEESProperty, SEOddsRatio);
                    break;

                case 7: // correlation coefficient r
                    SetProperty(RProperty, Data2);
                    SetProperty(SERProperty, Math.Sqrt(1 / (Data1 - 3)));
                    SetProperty(ESProperty, R);
                    SetProperty(SEESProperty, SER);
                    break;

                default:
                    break;
            }
            SetProperty(CILowerProperty, SMD - (1.96 * SEES));
            SetProperty(CIUpperProperty, SMD + (1.96 * SEES));
        }

        public void SetESforManualEntry()
        {
            if (OddsRatio == 0 && RiskRatio == 0 && RiskDifference == 0 && PetoOR == 0 && this.R == 0 && MeanDifference == 0)
            {
                SetProperty(ESProperty, SMD);
                SetProperty(SEESProperty, SESMD);
                UnifiedOutcomeTypeId = 1;
                SetProperty(OutcomeTypeNameProperty, "Continuous");
            }
            if (SMD == 0 && RiskRatio == 0 && RiskDifference == 0 && PetoOR == 0 && this.R == 0 && MeanDifference == 0)
            {
                SetProperty(ESProperty, OddsRatio);
                SetProperty(SEESProperty, SEOddsRatio);
                UnifiedOutcomeTypeId = 2;
                SetProperty(OutcomeTypeNameProperty, "Binary");
            }
            if (SMD == 0 && OddsRatio == 0 && RiskDifference == 0 && PetoOR == 0 && this.R == 0 && MeanDifference == 0)
            {
                SetProperty(ESProperty, RiskRatio);
                SetProperty(SEESProperty, SERiskRatio);
                UnifiedOutcomeTypeId = 2;
                SetProperty(OutcomeTypeNameProperty, "Binary");
            }
            if (SMD == 0 && OddsRatio == 0 && RiskRatio == 0 && PetoOR == 0 && this.R == 0 && MeanDifference == 0)
            {
                SetProperty(ESProperty, RiskDifference);
                SetProperty(SEESProperty, SERiskDifference);
                UnifiedOutcomeTypeId = 2;
                SetProperty(OutcomeTypeNameProperty, "Binary");
            }
            if (SMD == 0 && OddsRatio == 0 && RiskRatio == 0 && RiskDifference == 0 && this.R == 0 && MeanDifference == 0)
            {
                SetProperty(ESProperty, PetoOR);
                SetProperty(SEESProperty, SEPetoOR);
                UnifiedOutcomeTypeId = 2;
                SetProperty(OutcomeTypeNameProperty, "Binary");
            }
            if (SMD == 0 && OddsRatio == 0 && RiskRatio == 0 && RiskDifference == 0 && PetoOR == 0 && MeanDifference == 0)
            {
                SetProperty(ESProperty, this.R);
                SetProperty(SEESProperty, SER);
                UnifiedOutcomeTypeId = 7;
                SetProperty(OutcomeTypeNameProperty, "Correlation");
            }
            if (SMD == 0 && OddsRatio == 0 && RiskRatio == 0 && RiskDifference == 0 && PetoOR == 0 && this.R == 0)
            {
                SetProperty(ESProperty, MeanDifference);
                SetProperty(SEESProperty, SEMeanDifference);
                UnifiedOutcomeTypeId = 1;
                SetProperty(OutcomeTypeNameProperty, "Continuous");
            }
        }

        public void SetESForThisOutcomeType(int MAType)
        {
            /* Meta-analysis types:
         *     0: Continuous: d (Hedges g)
         *     1: Continuous: r
         *     2: Binary: odds ratio
         *     3: Binary: risk ratio
         *     4: Binary: risk difference
         *     5: Binary: diagnostic test OR
         *     6: Binary: Peto OR
         *     7: Continuous: mean difference
         */
            if (OutcomeTypeId != 0)
            {
                SetProperty(ESProperty, 0.0);
                SetProperty(SEESProperty, 0.0);
                switch (MAType)
                {
                    case 0: SetProperty(ESProperty, SMD); SetProperty(SEESProperty, SESMD); break;
                    case 1: SetProperty(ESProperty, this.R); SetProperty(SEESProperty, SER); break;
                    case 2: SetProperty(ESProperty, OddsRatio); SetProperty(SEESProperty, SEOddsRatio); break;
                    case 3: SetProperty(ESProperty, RiskRatio); SetProperty(SEESProperty, SERiskRatio); break;
                    case 4: SetProperty(ESProperty, RiskDifference); SetProperty(SEESProperty, SERiskDifference); break;
                    case 5: SetProperty(ESProperty, OddsRatio); SetProperty(SEESProperty, SEOddsRatio); break;
                    case 6: SetProperty(ESProperty, PetoOR); SetProperty(SEESProperty, SEPetoOR); break;
                    case 7: SetProperty(ESProperty, MeanDifference); SetProperty(SEESProperty, SEMeanDifference); break;
                    default: break;
                }
            }
        }

        private double getSEforD(double n1, double n2, double d)
        {
            double top, lower, left, right, se;

            left = (n1 + n2) / (n1 * n2);
            top = d * d;
            lower = 2 * (n1 + n2 - 3.94);
            right = top / lower;
            se = Math.Sqrt(left + right);
            return se;
        }
        private double getSEforMeanDiff(double n1, double n2, double SD1, double SD2)
        {
            return Math.Sqrt(SD1 * SD1 / n1 + SD2 * SD2 / n2);
        }
        private double SmdFromNMeanSD()
        {
            double SD = PoolSDs(Data1, Data2, Data5, Data6);
            if (SD == 0)
            {
                return 0;
            }
            double cohensD = (Data3 - Data4) / SD;
            return cohensD * (1 - (3 / (4 * (Data1 + Data2) - 9)));
        }

        private double MeanDiff()
        {
            return Data3 - Data4;
        }

        private double SmdFromNMeanSe()
        {
            double SD = PoolSDs(Data1, Data2, SdFromSe(Data5, Data1), SdFromSe(Data6, Data1));
            if (SD == 0)
            {
                return 0;
            }
            double cohensD = (Data3 - Data4) / SD;
            return cohensD * (1 - (3 / (4 * (Data1 + Data2) - 9)));
        }

        private double SmdFromNMeanCI()
        {
            double SD = PoolSDs(Data1, Data2, SdFromSe(SeFromCi(Data7, Data5), Data1), SdFromSe(SeFromCi(Data8, Data6), Data1));
            if (SD == 0)
            {
                return 0;
            }
            double cohensD = (Data3 - Data4) / SD;
            return cohensD * (1 - (3 / (4 * (Data1 + Data2) - 9)));
        }

        private double SmdFromP()
        {
            double t = StatFunctions.qt(Data4 / 2, Data1 + Data2 - 2, false);
            return SmdFromT(t);
        }

        private double SmdFromT(double t)
        {
            double g, top, lower;

            if (Data1 == Data2)
            {
                top = 2 * t;
                lower = System.Math.Sqrt(Data1 + Data2);
            }
            else
            {
                top = t * System.Math.Sqrt(Data1 + Data2);
                lower = System.Math.Sqrt(Data1 * Data2);
            }
            if (lower != 0)
            {
                g = top / lower;
            }
            else
            {
                g = 0;
            }
            return g;
        }

        private double CorrectG(double n1, double n2, double g) // for single group studies n2=0
        {
            double gc, top, lower;

            top = 3;
            lower = (4 * (n1 + n2)) - 9;
            if (lower != 0)
            {
                gc = g * (1 - (top / lower));
            }
            else
            {
                gc = 0;
            }
            return gc;
        }

        private double calcOddsRatio()
        {
            double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
            if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0))
            {
                d1 = Data1 + 0.5;
                d2 = Data2 + 0.5;
                d3 = Data3 + 0.5;
                d4 = Data4 + 0.5;
            }
            else
            {
                d1 = Data1;
                d2 = Data2;
                d3 = Data3;
                d4 = Data4;
            }
            return (d1 * d4) / (d3 * d2);
        }

        private double calcOddsRatioSE()
        {
            double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
            if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0))
            {
                d1 = Data1 + 0.5;
                d2 = Data2 + 0.5;
                d3 = Data3 + 0.5;
                d4 = Data4 + 0.5;
            }
            else
            {
                d1 = Data1;
                d2 = Data2;
                d3 = Data3;
                d4 = Data4;
            }
            return Math.Sqrt((1 / d1) + (1 / d2) + (1 / d3) + (1 / d4));
        }

        private double calcRiskRatio()
        {
            double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
            if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0))
            {
                d1 = Data1 + 0.5;
                d2 = Data2 + 0.5;
                d3 = Data3 + 0.5;
                d4 = Data4 + 0.5;
            }
            else
            {
                d1 = Data1;
                d2 = Data2;
                d3 = Data3;
                d4 = Data4;
            }
            return (d1 / (d1 + d3)) / (d2 / (d2 + d4));
        }

        private double calcRiskRatioSE()
        {
            double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
            if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0))
            {
                d1 = Data1 + 0.5;
                d2 = Data2 + 0.5;
                d3 = Data3 + 0.5;
                d4 = Data4 + 0.5;
            }
            else
            {
                d1 = Data1;
                d2 = Data2;
                d3 = Data3;
                d4 = Data4;
            }
            return Math.Sqrt((1 / d1) + (1 / d2) - (1 / (d1 + d3)) - (1 / (d2 + d4)));
        }

        private double calcRiskDifference()
        {
            double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
            if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0))
            {
                d1 = Data1 + 0.5;
                d2 = Data2 + 0.5;
                d3 = Data3 + 0.5;
                d4 = Data4 + 0.5;
            }
            else
            {
                d1 = Data1;
                d2 = Data2;
                d3 = Data3;
                d4 = Data4;
            }
            return (d1 / (d1 + d3)) - (d2 / (d2 + d4));
        }

        private double calcRiskDifferenceSE()
        {
            double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
            if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0))
            {
                d1 = Data1 + 0.5;
                d2 = Data2 + 0.5;
                d3 = Data3 + 0.5;
                d4 = Data4 + 0.5;
            }
            else
            {
                d1 = Data1;
                d2 = Data2;
                d3 = Data3;
                d4 = Data4;
            }
            return Math.Sqrt((d1 * d3 / Math.Pow((d1 + d3), 3)) + (d2 * d4 / Math.Pow((d2 + d4), 3)));
        }

        private double calcPetoOR()
        {
            double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
            if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0))
            {
                d1 = Data1 + 0.5;
                d2 = Data2 + 0.5;
                d3 = Data3 + 0.5;
                d4 = Data4 + 0.5;
            }
            else
            {
                d1 = Data1;
                d2 = Data2;
                d3 = Data3;
                d4 = Data4;
            }
            double n1 = d1 + d3;
            double n2 = d2 + d4;
            double n = n1 + n2;

            double v = (n1 * n2 * (d1 + d2) * (d3 + d4)) / (n * n * (n - 1));
            double e = n1 * (d1 + d2) / n;
            return Math.Exp((d1 - e) / v);
        }

        private double calcPetoORSE()
        {
            double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
            if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0))
            {
                d1 = Data1 + 0.5;
                d2 = Data2 + 0.5;
                d3 = Data3 + 0.5;
                d4 = Data4 + 0.5;
            }
            else
            {
                d1 = Data1;
                d2 = Data2;
                d3 = Data3;
                d4 = Data4;
            }
            double n1 = d1 + d3;
            double n2 = d2 + d4;
            double n = n1 + n2;

            double v = (n1 * n2 * (d1 + d2) * (d3 + d4)) / (n * n * (n - 1));
            return Math.Sqrt(1 / v);
        }

        private double PoolSDs(double n1, double n2, double sd1, double sd2)
        {
            if (n1 + n2 < 3)
            {
                return 0;
            }
            double s = (((n1 - 1) * sd1 * sd1) + ((n2 - 1) * sd2 * sd2)) / (n1 + n2 - 2);
            s = Math.Sqrt(s);
            return s;
        }

        private double SdFromSe(double se, double n)
        {
            return se * Math.Sqrt(n);
        }

        private double SeFromCi(double ciUpper, double ciLower)
        {
            double se = Math.Abs((0.5 * (ciUpper - ciLower)) / 1.96);
            return se;
        }

        private double CorrectForClustering(double se)
        {
            if (Data9 != 0)
            {
                double deff = 1 + (Data9 - 1) * Data10;
                return se * Math.Sqrt(deff);
            }
            else
            {
                return se;
            }
        }

        /* Meta-analysis types:
         *     0: Continuous: d (Hedges g)
         *     1: Continuous: r
         *     2: Binary: odds ratio
         *     3: Binary: risk ratio
         *     4: Binary: risk difference
         *     5: Binary: diagnostic test OR
         *     6: Binary: Peto OR
         *     7: Continuous: mean difference
         */
        public double GetEffectSizeCombining(int MAType)
        {
            switch (MAType)
            {
                case 0: return this.SMD;
                case 1: return 0.5 * Math.Log((1 + this.R) / (1 - this.R));
                case 2: return Math.Log(this.OddsRatio);
                case 3: return Math.Log(this.RiskRatio);
                case 4: return this.RiskDifference;
                case 5: return Math.Log(this.OddsRatio);
                case 6: return Math.Log(this.PetoOR);
                case 7: return this.MeanDifference;
            }
            return 0;
        }

        public double GetStandardErrorCombining(int MAType)
        {
            switch (MAType)
            {
                case 0: return this.SESMD;
                case 1: return this.SER;
                case 2: return this.SEOddsRatio;
                case 3: return this.SERiskRatio;
                case 4: return this.SERiskDifference;
                case 5: return this.SEOddsRatio;
                case 6: return this.SEPetoOR;
                case 7: return this.SEMeanDifference;
            }
            return 0;
        }

        public double GetEffectSizeDisplaying(int MAType)
        {
            switch (MAType)
            {
                case 0: return this.SMD;
                case 1: return this.R;
                case 2: return this.OddsRatio;
                case 3: return this.RiskRatio;
                case 4: return this.RiskDifference;
                case 5: return this.OddsRatio;
                case 6: return this.PetoOR;
                case 7: return this.MeanDifference;
            }
            return 0;
        }

        public double GetStandardErrorDisplaying(int MAType)
        {
            switch (MAType)
            {
                case 0: return this.SESMD;
                case 1: return (Math.Exp(2 * this.SER) - 1) / (Math.Exp(2 * this.SER) + 1); //return Math.Pow((1 - Math.Pow(this.R, 2)), 2) / (this.Data1 - 1);
                case 2: return Math.Exp(this.SEOddsRatio);
                case 3: return Math.Exp(this.SERiskRatio);
                case 4: return this.SERiskDifference;
                case 5: return Math.Exp(this.SEOddsRatio);
                case 6: return Math.Exp(this.SEPetoOR);
                case 7: return this.SEMeanDifference;
            }
            return 0;
        }

        public static readonly PropertyInfo<double> SMDProperty = RegisterProperty<double>(new PropertyInfo<double>("SMD", "SMD"));
        [JsonProperty]
        public double SMD
        {
            get
            {
                return GetProperty(SMDProperty);
            }
        }

        public static readonly PropertyInfo<double> SESMDProperty = RegisterProperty<double>(new PropertyInfo<double>("SESMD", "SESMD"));
        [JsonProperty]
        public double SESMD
        {
            get
            {
                return GetProperty(SESMDProperty);
            }
        }

        public static readonly PropertyInfo<double> RProperty = RegisterProperty<double>(new PropertyInfo<double>("R", "R"));
        [JsonProperty]
        public double R
        {
            get
            {
                return GetProperty(RProperty);
            }
        }

        public static readonly PropertyInfo<double> SERProperty = RegisterProperty<double>(new PropertyInfo<double>("SER", "SER"));
        [JsonProperty]
        public double SER
        {
            get
            {
                return GetProperty(SERProperty);
            }
        }

        public static readonly PropertyInfo<double> OddsRatioProperty = RegisterProperty<double>(new PropertyInfo<double>("OddsRatio", "OddsRatio"));
        [JsonProperty]
        public double OddsRatio
        {
            get
            {
                return GetProperty(OddsRatioProperty);
            }
        }

        public static readonly PropertyInfo<double> SEOddsRatioProperty = RegisterProperty<double>(new PropertyInfo<double>("SEOddsRatio", "SEOddsRatio"));
        [JsonProperty]
        public double SEOddsRatio
        {
            get
            {
                return GetProperty(SEOddsRatioProperty);
            }
        }

        public static readonly PropertyInfo<double> RiskRatioProperty = RegisterProperty<double>(new PropertyInfo<double>("RiskRatio", "RiskRatio"));
        [JsonProperty]
        public double RiskRatio
        {
            get
            {
                return GetProperty(RiskRatioProperty);
            }
        }

        public static readonly PropertyInfo<double> SERiskRatioProperty = RegisterProperty<double>(new PropertyInfo<double>("SERiskRatio", "SERiskRatio"));
        [JsonProperty]
        public double SERiskRatio
        {
            get
            {
                return GetProperty(SERiskRatioProperty);
            }
        }

        [JsonProperty]
        public double CIUpperSMD
        {
            get
            {
                return SMD + (GetProperty(SESMDProperty) * 1.96);
            }
        }

        [JsonProperty]
        public double CILowerSMD
        {
            get
            {
                return SMD - (GetProperty(SESMDProperty) * 1.96);
            }
        }

        [JsonProperty]
        public double CIUpperR
        {
            get
            {
                double z = this.GetEffectSizeCombining(1) + (this.SER * 1.96);
                return (Math.Exp(z * 2) - 1) / (Math.Exp(z * 2) + 1);
            }
        }

        [JsonProperty]
        public double CILowerR
        {
            get
            {
                double z = this.GetEffectSizeCombining(1) - (this.SER * 1.96);
                return (Math.Exp(z * 2) - 1) / (Math.Exp(z * 2) + 1);
            }
        }

        [JsonProperty]
        public double CIUpperOddsRatio
        {
            get
            {
                return Math.Exp(Math.Log(OddsRatio) + (GetProperty(SEOddsRatioProperty) * 1.96));
            }
        }

        [JsonProperty]
        public double CILowerOddsRatio
        {
            get
            {
                return Math.Exp(Math.Log(OddsRatio) - (GetProperty(SEOddsRatioProperty) * 1.96));
            }
        }

        [JsonProperty]
        public double CIUpperRiskRatio
        {
            get
            {
                return Math.Exp( Math.Log(RiskRatio) + (GetProperty(SERiskRatioProperty) * 1.96));
            }
        }

        [JsonProperty]
        public double CILowerRiskRatio
        {
            get
            {
                return Math.Exp(Math.Log(RiskRatio) - (GetProperty(SERiskRatioProperty) * 1.96));
            }
        }

        [JsonProperty]
        public double CIUpperRiskDifference
        {
            get
            {
                return RiskDifference + (GetProperty(SERiskDifferenceProperty) * 1.96);
            }
        }

        [JsonProperty]
        public double CILowerRiskDifference
        {
            get
            {
                return RiskDifference - (GetProperty(SERiskDifferenceProperty) * 1.96);
            }
        }

        [JsonProperty]
        public double CIUpperPetoOddsRatio
        {
            get
            {
                return Math.Exp(Math.Log(PetoOR) + (GetProperty(SEPetoORProperty) * 1.96));
            }
        }

        [JsonProperty]
        public double CILowerPetoOddsRatio
        {
            get
            {
                return Math.Exp(Math.Log(PetoOR) - (GetProperty(SEPetoORProperty) * 1.96));
            }
        }

        [JsonProperty]
        public double CIUpperMeanDifference
        {
            get
            {
                return MeanDifference + (GetProperty(SEMeanDifferenceProperty) * 1.96);
            }
        }

        [JsonProperty]
        public double CILowerMeanDifference
        {
            get
            {
                return MeanDifference - (GetProperty(SEMeanDifferenceProperty) * 1.96);
            }
        }

        public static readonly PropertyInfo<double> RiskDifferenceProperty = RegisterProperty<double>(new PropertyInfo<double>("RiskDifference", "RiskDifference"));
        [JsonProperty]
        public double RiskDifference
        {
            get
            {
                return GetProperty(RiskDifferenceProperty);
            }
        }

        public static readonly PropertyInfo<double> SERiskDifferenceProperty = RegisterProperty<double>(new PropertyInfo<double>("SERiskDifference", "SERiskDifference"));
        [JsonProperty]
        public double SERiskDifference
        {
            get
            {
                return GetProperty(SERiskDifferenceProperty);
            }
        }

        public static readonly PropertyInfo<double> MeanDifferenceProperty = RegisterProperty<double>(new PropertyInfo<double>("MeanDifference", "MeanDifference"));
        [JsonProperty]
        public double MeanDifference
        {
            get
            {
                return GetProperty(MeanDifferenceProperty);
            }
        }

        public static readonly PropertyInfo<double> SEMeanDifferenceProperty = RegisterProperty<double>(new PropertyInfo<double>("SEMeanDifference", "SEMeanDifference"));
        [JsonProperty]
        public double SEMeanDifference
        {
            get
            {
                return GetProperty(SEMeanDifferenceProperty);
            }
        }

        public static readonly PropertyInfo<double> PetoORProperty = RegisterProperty<double>(new PropertyInfo<double>("PetoOR", "PetoOR"));
        [JsonProperty]
        public double PetoOR
        {
            get
            {
                return GetProperty(PetoORProperty);
            }
        }

        public static readonly PropertyInfo<double> SEPetoORProperty = RegisterProperty<double>(new PropertyInfo<double>("SEPetoOR", "SEPetoOR"));
        [JsonProperty]
        public double SEPetoOR
        {
            get
            {
                return GetProperty(SEPetoORProperty);
            }
        }

        public static readonly PropertyInfo<double> ESProperty = RegisterProperty<double>(new PropertyInfo<double>("ES", "ES"));
        [JsonProperty]
        public double ES
        {
            get
            {
                return GetProperty(ESProperty);
            }
        }

        public static readonly PropertyInfo<double> SEESProperty = RegisterProperty<double>(new PropertyInfo<double>("SEES", "SEES"));
        [JsonProperty]
        public double SEES
        {
            get
            {
                return GetProperty(SEESProperty);
            }
        }

        public static readonly PropertyInfo<int> NRowsProperty = RegisterProperty<int>(new PropertyInfo<int>("NRows", "NRows"));
        [JsonProperty]
        public int NRows
        {
            get
            {
                return GetProperty(NRowsProperty);
            }
        }

        public static readonly PropertyInfo<double> CILowerProperty = RegisterProperty<double>(new PropertyInfo<double>("CILower", "CILower"));
        [JsonProperty]
        public double CILower
        {
            get
            {
                return GetProperty(CILowerProperty);
            }
        }

        public static readonly PropertyInfo<double> CIUpperProperty = RegisterProperty<double>(new PropertyInfo<double>("CIUpper", "CIUpper"));
        [JsonProperty]
        public double CIUpper
        {
            get
            {
                return GetProperty(CIUpperProperty);
            }
        }

        public static readonly PropertyInfo<string> ESDescProperty = RegisterProperty<string>(new PropertyInfo<string>("ESDesc", "ESDesc", string.Empty));
        [JsonProperty]
        public string ESDesc
        {
            get
            {
                return GetProperty(ESDescProperty);
            }
        }

        public static readonly PropertyInfo<string> SEDescProperty = RegisterProperty<string>(new PropertyInfo<string>("SEDesc", "SEDesc", string.Empty));
        [JsonProperty]
        public string SEDesc
        {
            get
            {
                return GetProperty(SEDescProperty);
            }
        }

        public static readonly PropertyInfo<string> Data1DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data1Desc", "Data1Desc", string.Empty));
        [JsonProperty]
        public string Data1Desc
        {
            get
            {
                return GetProperty(Data1DescProperty);
            }
        }

        public static readonly PropertyInfo<string> Data2DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data2Desc", "Data2Desc", string.Empty));
        [JsonProperty]
        public string Data2Desc
        {
            get
            {
                return GetProperty(Data2DescProperty);
            }
        }
        
        public static readonly PropertyInfo<string> Data3DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data3Desc", "Data3Desc", string.Empty));
        [JsonProperty]
        public string Data3Desc
        {
            get
            {
                return GetProperty(Data3DescProperty);
            }
        }
        
        public static readonly PropertyInfo<string> Data4DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data4Desc", "Data4Desc", string.Empty));
        [JsonProperty]
        public string Data4Desc
        {
            get
            {
                return GetProperty(Data4DescProperty);
            }
        }
        
        public static readonly PropertyInfo<string> Data5DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data5Desc", "Data5Desc", string.Empty));
        [JsonProperty]
        public string Data5Desc
        {
            get
            {
                return GetProperty(Data5DescProperty);
            }
        }
        
        public static readonly PropertyInfo<string> Data6DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data6Desc", "Data6Desc", string.Empty));
        [JsonProperty]
        public string Data6Desc
        {
            get
            {
                return GetProperty(Data6DescProperty);
            }
        }

        public static readonly PropertyInfo<string> Data7DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data7Desc", "Data7Desc", string.Empty));
        [JsonProperty]
        public string Data7Desc
        {
            get
            {
                return GetProperty(Data7DescProperty);
            }
        }

        public static readonly PropertyInfo<string> Data8DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data8Desc", "Data8Desc", string.Empty));
        [JsonProperty]
        public string Data8Desc
        {
            get
            {
                return GetProperty(Data8DescProperty);
            }
        }

        public static readonly PropertyInfo<string> Data9DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data9Desc", "Data9Desc", string.Empty));
        [JsonProperty]
        public string Data9Desc
        {
            get
            {
                return GetProperty(Data9DescProperty);
            }
        }

        public static readonly PropertyInfo<string> Data10DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data10Desc", "Data10Desc", string.Empty));
        [JsonProperty]
        public string Data10Desc
        {
            get
            {
                return GetProperty(Data10DescProperty);
            }
        }

        public static readonly PropertyInfo<string> Data11DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data11Desc", "Data11Desc", string.Empty));
        [JsonProperty]
        public string Data11Desc
        {
            get
            {
                return GetProperty(Data11DescProperty);
            }
        }

        public static readonly PropertyInfo<string> Data12DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data12Desc", "Data12Desc", string.Empty));
        [JsonProperty]
        public string Data12Desc
        {
            get
            {
                return GetProperty(Data12DescProperty);
            }
        }

        public static readonly PropertyInfo<string> Data13DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data13Desc", "Data13Desc", string.Empty));
        [JsonProperty]
        public string Data13Desc
        {
            get
            {
                return GetProperty(Data13DescProperty);
            }
        }

        public static readonly PropertyInfo<string> Data14DescProperty = RegisterProperty<string>(new PropertyInfo<string>("Data14Desc", "Data14Desc", string.Empty));
        [JsonProperty]
        public string Data14Desc
        {
            get
            {
                return GetProperty(Data14DescProperty);
            }
        }
        
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(Item), admin);
        //    //AuthorizationRules.AllowDelete(typeof(Item), admin);
        //    //AuthorizationRules.AllowEdit(typeof(Item), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(Item), canRead);

        //    //AuthorizationRules.AllowRead(TitleProperty, canRead);

        //    //AuthorizationRules.AllowWrite(TitleProperty, canWrite);
        //}

        protected override void AddBusinessRules()
        {
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT

        protected override void DataPortal_Insert()
        {
            insert_object();
        }

        protected override void DataPortal_Update()
        {
            if (OutcomeId == 0)
            {
                insert_object();
            }
            else
            {
                update_object();
            }
        }

        protected void update_object()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OutcomeItemUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@OUTCOME_ID", ReadProperty(OutcomeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@OUTCOME_TYPE_ID", ReadProperty(OutcomeTypeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_INTERVENTION", ReadProperty(ItemAttributeIdInterventionProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_CONTROL", ReadProperty(ItemAttributeIdControlProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_OUTCOME", ReadProperty(ItemAttributeIdOutcomeProperty)));
                    command.Parameters.Add(new SqlParameter("@OUTCOME_TITLE", ReadProperty(TitleProperty)));
                    command.Parameters.Add(new SqlParameter("@OUTCOME_DESCRIPTION", ReadProperty(OutcomeDescriptionProperty)));
                    command.Parameters.Add(new SqlParameter("@DATA1", ReadProperty(Data1Property)));
                    command.Parameters.Add(new SqlParameter("@DATA2", ReadProperty(Data2Property)));
                    command.Parameters.Add(new SqlParameter("@DATA3", ReadProperty(Data3Property)));
                    command.Parameters.Add(new SqlParameter("@DATA4", ReadProperty(Data4Property)));
                    command.Parameters.Add(new SqlParameter("@DATA5", ReadProperty(Data5Property)));
                    command.Parameters.Add(new SqlParameter("@DATA6", ReadProperty(Data6Property)));
                    command.Parameters.Add(new SqlParameter("@DATA7", ReadProperty(Data7Property)));
                    command.Parameters.Add(new SqlParameter("@DATA8", ReadProperty(Data8Property)));
                    command.Parameters.Add(new SqlParameter("@DATA9", ReadProperty(Data9Property)));
                    command.Parameters.Add(new SqlParameter("@DATA10", ReadProperty(Data10Property)));
                    command.Parameters.Add(new SqlParameter("@DATA11", ReadProperty(Data11Property)));
                    command.Parameters.Add(new SqlParameter("@DATA12", ReadProperty(Data12Property)));
                    command.Parameters.Add(new SqlParameter("@DATA13", ReadProperty(Data13Property)));
                    command.Parameters.Add(new SqlParameter("@DATA14", ReadProperty(Data14Property)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected void insert_object()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OutcomeItemInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", ReadProperty(ItemSetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@OUTCOME_TYPE_ID", ReadProperty(OutcomeTypeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_INTERVENTION", ReadProperty(ItemAttributeIdInterventionProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_CONTROL", ReadProperty(ItemAttributeIdControlProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_OUTCOME", ReadProperty(ItemAttributeIdOutcomeProperty)));
                    command.Parameters.Add(new SqlParameter("@OUTCOME_TITLE", ReadProperty(TitleProperty)));
                    command.Parameters.Add(new SqlParameter("@OUTCOME_DESCRIPTION", ReadProperty(OutcomeDescriptionProperty)));
                    command.Parameters.Add(new SqlParameter("@DATA1", ReadProperty(Data1Property)));
                    command.Parameters.Add(new SqlParameter("@DATA2", ReadProperty(Data2Property)));
                    command.Parameters.Add(new SqlParameter("@DATA3", ReadProperty(Data3Property)));
                    command.Parameters.Add(new SqlParameter("@DATA4", ReadProperty(Data4Property)));
                    command.Parameters.Add(new SqlParameter("@DATA5", ReadProperty(Data5Property)));
                    command.Parameters.Add(new SqlParameter("@DATA6", ReadProperty(Data6Property)));
                    command.Parameters.Add(new SqlParameter("@DATA7", ReadProperty(Data7Property)));
                    command.Parameters.Add(new SqlParameter("@DATA8", ReadProperty(Data8Property)));
                    command.Parameters.Add(new SqlParameter("@DATA9", ReadProperty(Data9Property)));
                    command.Parameters.Add(new SqlParameter("@DATA10", ReadProperty(Data10Property)));
                    command.Parameters.Add(new SqlParameter("@DATA11", ReadProperty(Data11Property)));
                    command.Parameters.Add(new SqlParameter("@DATA12", ReadProperty(Data12Property)));
                    command.Parameters.Add(new SqlParameter("@DATA13", ReadProperty(Data13Property)));
                    command.Parameters.Add(new SqlParameter("@DATA14", ReadProperty(Data14Property)));
                    SqlParameter par = new SqlParameter("@NEW_OUTCOME_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par);
                    command.Parameters["@NEW_OUTCOME_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(OutcomeIdProperty, command.Parameters["@NEW_OUTCOME_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OutcomeItemDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@OUTCOME_ID", ReadProperty(OutcomeIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected void DataPortal_Fetch(SingleCriteria<Item, Int64> criteria) // used to return a specific item
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_Item", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            //LoadProperty<Int64>(OutcomeIdProperty, reader.GetInt64("ITEM_ID"));
                            LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
                        }
                    }
                }
                connection.Close();
            }
            */
        }


        /* USED IN REPORTS N.B. DOES *NOT* GET THE ATTRIBUTES THAT APPEAR IN THE META-ANALYSIS GRID */
        public static Outcome GetSingleOutcome(int OutcomeId)
        {
            Outcome returnValue = new Outcome();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OutcomeSingle", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@OUTCOME_ID", OutcomeId));
                    returnValue.OutcomeCodes.RaiseListChangedEvents = false;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            returnValue.LoadProperty<int>(OutcomeIdProperty, reader.GetInt32("OUTCOME_ID"));
                            returnValue.LoadProperty<Int64>(ItemSetIdProperty, reader.GetInt64("ITEM_SET_ID"));
                            returnValue.LoadProperty<int>(OutcomeTypeIdProperty, reader.GetInt32("OUTCOME_TYPE_ID"));
                            returnValue.LoadProperty<string>(OutcomeTypeNameProperty, reader.GetString("OUTCOME_TYPE_NAME"));
                            returnValue.LoadProperty<Int64>(ItemAttributeIdInterventionProperty, reader.GetInt64("ITEM_ATTRIBUTE_ID_INTERVENTION"));
                            returnValue.LoadProperty<Int64>(ItemAttributeIdControlProperty, reader.GetInt64("ITEM_ATTRIBUTE_ID_CONTROL"));
                            returnValue.LoadProperty<Int64>(ItemAttributeIdOutcomeProperty, reader.GetInt64("ITEM_ATTRIBUTE_ID_OUTCOME"));
                            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("OUTCOME_TITLE"));
                            returnValue.LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
                            returnValue.LoadProperty<string>(OutcomeDescriptionProperty, reader.GetString("OUTCOME_DESCRIPTION"));
                            returnValue.LoadProperty<double>(Data1Property, reader.GetDouble("DATA1"));
                            returnValue.LoadProperty<double>(Data2Property, reader.GetDouble("DATA2"));
                            returnValue.LoadProperty<double>(Data3Property, reader.GetDouble("DATA3"));
                            returnValue.LoadProperty<double>(Data4Property, reader.GetDouble("DATA4"));
                            returnValue.LoadProperty<double>(Data5Property, reader.GetDouble("DATA5"));
                            returnValue.LoadProperty<double>(Data6Property, reader.GetDouble("DATA6"));
                            returnValue.LoadProperty<double>(Data7Property, reader.GetDouble("DATA7"));
                            returnValue.LoadProperty<double>(Data8Property, reader.GetDouble("DATA8"));
                            returnValue.LoadProperty<double>(Data9Property, reader.GetDouble("DATA9"));
                            returnValue.LoadProperty<double>(Data10Property, reader.GetDouble("DATA10"));
                            returnValue.LoadProperty<double>(Data11Property, reader.GetDouble("DATA11"));
                            returnValue.LoadProperty<double>(Data12Property, reader.GetDouble("DATA12"));
                            returnValue.LoadProperty<double>(Data13Property, reader.GetDouble("DATA13"));
                            returnValue.LoadProperty<double>(Data14Property, reader.GetDouble("DATA14"));
                            returnValue.LoadProperty<bool>(IsSelectedProperty, reader.GetInt32("META_ANALYSIS_OUTCOME_ID") == 0 ? false : true);
                            returnValue.LoadProperty<string>(InterventionTextProperty, reader.GetString("INTERVENTION_TEXT"));
                            returnValue.LoadProperty<string>(ControlTextProperty, reader.GetString("CONTROL_TEXT"));
                            returnValue.LoadProperty<string>(OutcomeTextProperty, reader.GetString("OUTCOME_TEXT"));
                            returnValue.MarkOld();
                            returnValue.SetCalculatedValues();
                        }
                        reader.Close();
                    }
                    returnValue.OutcomeCodes.RaiseListChangedEvents = true;
                }
                connection.Close();
            }
            return returnValue;
        }

        internal static Outcome GetOutcome(SafeDataReader reader)
        {
            Outcome returnValue = new Outcome();

            //returnValue.OutcomeCodes = OutcomeItemAttributeList.NewOutcomeItemAttributeList();

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OutcomeItemAttributeList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@OUTCOME_ID", reader.GetInt32("OUTCOME_ID")));
                    returnValue.OutcomeCodes.RaiseListChangedEvents = false;
                    using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader2.Read())
                        {
                            OutcomeItemAttribute newOutcomeItemAttribute = OutcomeItemAttribute.GetOutcomeItemAttribute(reader2);
                            returnValue.OutcomeCodes.Add(newOutcomeItemAttribute);
                        }
                        reader2.Close();
                    }
                    returnValue.OutcomeCodes.RaiseListChangedEvents = true;
                }
                connection.Close();
            }

            returnValue.LoadProperty<int>(OutcomeIdProperty, reader.GetInt32("OUTCOME_ID"));
            returnValue.LoadProperty<Int64>(ItemSetIdProperty, reader.GetInt64("ITEM_SET_ID"));
            returnValue.LoadProperty<int>(OutcomeTypeIdProperty, reader.GetInt32("OUTCOME_TYPE_ID"));
            // returnValue.LoadProperty<string>(OutcomeTypeNameProperty, reader.GetString("OUTCOME_TYPE_NAME")); <- not currently in query
            returnValue.LoadProperty<Int64>(ItemAttributeIdInterventionProperty, reader.GetInt64("ITEM_ATTRIBUTE_ID_INTERVENTION"));
            returnValue.LoadProperty<Int64>(ItemAttributeIdControlProperty, reader.GetInt64("ITEM_ATTRIBUTE_ID_CONTROL"));
            returnValue.LoadProperty<Int64>(ItemAttributeIdOutcomeProperty, reader.GetInt64("ITEM_ATTRIBUTE_ID_OUTCOME"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("OUTCOME_TITLE"));
            returnValue.LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
            returnValue.LoadProperty<string>(OutcomeDescriptionProperty, reader.GetString("OUTCOME_DESCRIPTION"));
            returnValue.LoadProperty<double>(Data1Property, reader.GetDouble("DATA1"));
            returnValue.LoadProperty<double>(Data2Property, reader.GetDouble("DATA2"));
            returnValue.LoadProperty<double>(Data3Property, reader.GetDouble("DATA3"));
            returnValue.LoadProperty<double>(Data4Property, reader.GetDouble("DATA4"));
            returnValue.LoadProperty<double>(Data5Property, reader.GetDouble("DATA5"));
            returnValue.LoadProperty<double>(Data6Property, reader.GetDouble("DATA6"));
            returnValue.LoadProperty<double>(Data7Property, reader.GetDouble("DATA7"));
            returnValue.LoadProperty<double>(Data8Property, reader.GetDouble("DATA8"));
            returnValue.LoadProperty<double>(Data9Property, reader.GetDouble("DATA9"));
            returnValue.LoadProperty<double>(Data10Property, reader.GetDouble("DATA10"));
            returnValue.LoadProperty<double>(Data11Property, reader.GetDouble("DATA11"));
            returnValue.LoadProperty<double>(Data12Property, reader.GetDouble("DATA12"));
            returnValue.LoadProperty<double>(Data13Property, reader.GetDouble("DATA13"));
            returnValue.LoadProperty<double>(Data14Property, reader.GetDouble("DATA14"));
            returnValue.LoadProperty<bool>(IsSelectedProperty, reader.GetInt32("META_ANALYSIS_OUTCOME_ID") == 0 ? false : true);
            returnValue.LoadProperty<string>(InterventionTextProperty, reader.GetString("INTERVENTION_TEXT"));
            returnValue.LoadProperty<string>(ControlTextProperty, reader.GetString("CONTROL_TEXT"));
            returnValue.LoadProperty<string>(OutcomeTextProperty, reader.GetString("OUTCOME_TEXT"));
            //returnValue.LoadProperty<int>(aa1Property, getAttributeAnswer(reader, 1, "aa"));
            //returnValue.LoadProperty<int>(aa2Property, getAttributeAnswer(reader, 2, "aa"));
            //returnValue.LoadProperty<int>(aa3Property, getAttributeAnswer(reader, 3, "aa"));
            //returnValue.LoadProperty<int>(aa4Property, getAttributeAnswer(reader, 4, "aa"));
            //returnValue.LoadProperty<int>(aa5Property, getAttributeAnswer(reader, 5, "aa"));
            //returnValue.LoadProperty<int>(aa6Property, getAttributeAnswer(reader, 6, "aa"));
            //returnValue.LoadProperty<int>(aa7Property, getAttributeAnswer(reader, 7, "aa"));
            //returnValue.LoadProperty<int>(aa8Property, getAttributeAnswer(reader, 8, "aa"));
            //returnValue.LoadProperty<int>(aa9Property, getAttributeAnswer(reader, 9, "aa"));
            //returnValue.LoadProperty<int>(aa10Property, getAttributeAnswer(reader, 10, "aa"));
            //returnValue.LoadProperty<int>(aa11Property, getAttributeAnswer(reader, 11, "aa"));
            //returnValue.LoadProperty<int>(aa12Property, getAttributeAnswer(reader, 12, "aa"));
            //returnValue.LoadProperty<int>(aa13Property, getAttributeAnswer(reader, 13, "aa"));
            //returnValue.LoadProperty<int>(aa14Property, getAttributeAnswer(reader, 14, "aa"));
            //returnValue.LoadProperty<int>(aa15Property, getAttributeAnswer(reader, 15, "aa"));
            //returnValue.LoadProperty<int>(aa16Property, getAttributeAnswer(reader, 16, "aa"));
            //returnValue.LoadProperty<int>(aa17Property, getAttributeAnswer(reader, 17, "aa"));
            //returnValue.LoadProperty<int>(aa18Property, getAttributeAnswer(reader, 18, "aa"));
            //returnValue.LoadProperty<int>(aa19Property, getAttributeAnswer(reader, 19, "aa"));
            //returnValue.LoadProperty<int>(aa20Property, getAttributeAnswer(reader, 20, "aa"));
            //returnValue.LoadProperty<string>(aq1Property, getAttributeQuestion(reader, 1, "aq"));
            //returnValue.LoadProperty<string>(aq2Property, getAttributeQuestion(reader, 2, "aq"));
            //returnValue.LoadProperty<string>(aq3Property, getAttributeQuestion(reader, 3, "aq"));
            //returnValue.LoadProperty<string>(aq4Property, getAttributeQuestion(reader, 4, "aq"));
            //returnValue.LoadProperty<string>(aq5Property, getAttributeQuestion(reader, 5, "aq"));
            //returnValue.LoadProperty<string>(aq6Property, getAttributeQuestion(reader, 6, "aq"));
            //returnValue.LoadProperty<string>(aq7Property, getAttributeQuestion(reader, 7, "aq"));
            //returnValue.LoadProperty<string>(aq8Property, getAttributeQuestion(reader, 8, "aq"));
            //returnValue.LoadProperty<string>(aq9Property, getAttributeQuestion(reader, 9, "aq"));
            //returnValue.LoadProperty<string>(aq10Property, getAttributeQuestion(reader, 10, "aq"));
            //returnValue.LoadProperty<string>(aq11Property, getAttributeQuestion(reader, 11, "aq"));
            //returnValue.LoadProperty<string>(aq12Property, getAttributeQuestion(reader, 12, "aq"));
            //returnValue.LoadProperty<string>(aq13Property, getAttributeQuestion(reader, 13, "aq"));
            //returnValue.LoadProperty<string>(aq14Property, getAttributeQuestion(reader, 14, "aq"));
            //returnValue.LoadProperty<string>(aq15Property, getAttributeQuestion(reader, 15, "aq"));
            //returnValue.LoadProperty<string>(aq16Property, getAttributeQuestion(reader, 16, "aq"));
            //returnValue.LoadProperty<string>(aq17Property, getAttributeQuestion(reader, 17, "aq"));
            //returnValue.LoadProperty<string>(aq18Property, getAttributeQuestion(reader, 18, "aq"));
            //returnValue.LoadProperty<string>(aq19Property, getAttributeQuestion(reader, 19, "aq"));
            //returnValue.LoadProperty<string>(aq20Property, getAttributeQuestion(reader, 20, "aq"));

            returnValue.MarkOld();
            returnValue.SetCalculatedValues();
            return returnValue;
        }

        private static int getAttributeAnswer(SafeDataReader reader, int index, string answerOrQuestion)
        {
            for (int fieldNo = 0; fieldNo < reader.FieldCount; fieldNo++)
            {
                if (reader.GetName(fieldNo).Equals(answerOrQuestion + index.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    return reader.GetInt32(answerOrQuestion + index.ToString());
                }
            }
            return -1;
        }

        private static string getAttributeQuestion(SafeDataReader reader, int index, string answerOrQuestion)
        {
            for (int fieldNo = 0; fieldNo < reader.FieldCount; fieldNo++)
            {
                if (reader.GetName(fieldNo).Equals(answerOrQuestion + index.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    return reader.GetString(answerOrQuestion + index.ToString());
                }
            }
            return "";
        }

#endif


    }
}
