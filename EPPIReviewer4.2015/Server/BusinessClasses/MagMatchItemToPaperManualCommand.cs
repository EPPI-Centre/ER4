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
    public class MagMatchItemToPaperManualCommand : CommandBase<MagMatchItemToPaperManualCommand>
    {

#if SILVERLIGHT
    public MagMatchItemToPaperManualCommand(){}
#else
        public MagMatchItemToPaperManualCommand() { }
#endif

        private Int64 _ITEM_ID;
        private Int64 _PaperId;
        private bool _ManualTrueMatch;
        private bool _ManualFalseMatch;

        public MagMatchItemToPaperManualCommand(Int64 ItemId, Int64 PaperId, bool ManualTrueMatch, bool ManualFalseMatch)
        {
            _ITEM_ID = ItemId;
            _PaperId = PaperId;
            _ManualFalseMatch = ManualFalseMatch;
            _ManualTrueMatch = ManualTrueMatch;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_ManualFalseMatch", _ManualFalseMatch);
            info.AddValue("_ManualTrueMatch", _ManualTrueMatch);
            info.AddValue("_ITEM_ID", _ITEM_ID);
            info.AddValue("_PaperId", _PaperId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _ManualFalseMatch = info.GetValue<bool>("_ManualFalseMatch");
            _ManualTrueMatch = info.GetValue<bool>("_ManualTrueMatch");
            _ITEM_ID = info.GetValue<Int64>("_ITEM_ID");
            _PaperId = info.GetValue<Int64>("_PaperId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            MagPaperItemMatch.MatchItemToMag(_PaperId);
        }

        

#endif


    }
}
