
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
    public class ReviewSetCopyCommand : CommandBase<ReviewSetCopyCommand>
    {
#if SILVERLIGHT
    public ReviewSetCopyCommand(){}
#else
        protected ReviewSetCopyCommand() { }
#endif

        private int _ReviewSetId;
        private int _SetOrder;

        public ReviewSetCopyCommand(int ReviewSetId, int SetOrder)
        {
            _ReviewSetId = ReviewSetId;
            _SetOrder = SetOrder;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_ReviewSetId", _ReviewSetId);
            info.AddValue("_SetOrder", _SetOrder);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _ReviewSetId = info.GetValue<int>("_ReviewSetId");
            _SetOrder = info.GetValue<int>("_SetOrder");
        }


#if !SILVERLIGHT
        ReviewSet Dest;
        ReviewerIdentity ri;
            
        protected override void DataPortal_Execute()
        {
            ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int revid= ri.ReviewId;//checking the ticket is valid
            ReviewSet source = ReviewSet.GetReviewSet(_ReviewSetId);
            Dest = ReviewSet.GetReviewSet(ri.ReviewId, source.SetTypeId, source.AllowCodingEdits, source.SetName, source.CodingIsFinal, _SetOrder, source.SetDescription, source.SetId);
            Dest = Dest.Save();
            //Dest = Dest.Save(true);
            foreach (AttributeSet aSet in source.Attributes)
            {
                CopyAttributeSet(aSet, 0);
            }
        }

        
        private void CopyAttributeSet(AttributeSet SourceAtt, Int64 parentID)
        {
            AttributeSet destAtt = AttributeSet.NewAttributeSet();
            //command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
            //command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", ReadProperty(ParentAttributeIdProperty)));
            //command.Parameters.Add(new SqlParameter("@ATTRIBUTE_TYPE_ID", ReadProperty(AttributeTypeIdProperty)));
            //command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_DESC", ReadProperty(AttributeSetDescriptionProperty)));
            //command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ORDER", ReadProperty(AttributeOrderProperty)));
            //command.Parameters.Add(new SqlParameter("@ATTRIBUTE_NAME", ReadProperty(AttributeNameProperty)));
            //command.Parameters.Add(new SqlParameter("@ATTRIBUTE_DESC", ReadProperty(AttributeDescriptionProperty)));
            //command.Parameters.Add(new SqlParameter("@CONTACT_ID", ReadProperty(ContactIdProperty)));

            //command.Parameters.Add(new SqlParameter("@NEW_ATTRIBUTE_SET_ID", 0));
            //command.Parameters["@NEW_ATTRIBUTE_SET_ID"].Direction = System.Data.ParameterDirection.Output;
            //command.Parameters.Add(new SqlParameter("@NEW_ATTRIBUTE_ID", 0));
            destAtt.SetId = Dest.SetId;
            destAtt.ParentAttributeId = parentID;
            destAtt.AttributeTypeId = SourceAtt.AttributeTypeId;
            destAtt.AttributeSetDescription = SourceAtt.AttributeSetDescription;
            destAtt.AttributeOrder = SourceAtt.AttributeOrder;
            destAtt.AttributeName = SourceAtt.AttributeName;
            destAtt.AttributeDescription = SourceAtt.AttributeDescription;
            destAtt.ContactId = ri.UserId;
            destAtt.OriginalAttributeID = SourceAtt.AttributeId;
            destAtt = destAtt.Save(true);
            foreach (AttributeSet aSet in SourceAtt.Attributes)
            {
                CopyAttributeSet(aSet, destAtt.AttributeId);
            }
        }
#endif
    }
}

