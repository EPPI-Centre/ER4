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
    public class ComparisonItemAttributeSaveCommand : CommandBase<ComparisonItemAttributeSaveCommand>
    {

    public ComparisonItemAttributeSaveCommand(){}

        //@CurrentContactId = 1214,
        //@DestinationContactId = 1512,
        //@SourceContactId = 1214,
        //@attributeSetId = 62105,
        //@comparisonId = 658,
        //@IncludePDFcoding = 1,
        //@SET_ID = 790,
        //@ITEM_ID = 3264,
        //@REVIEW_ID = 7,
        //@ITEM_ARM_ID = NULL,
        //@Result = @Result OUTPUT,
        //@NEW_ITEM_ATTRIBUTE_ID = @NEW_ITEM_ATTRIBUTE_ID OUTPUT,
        //@NEW_ITEM_SET_ID = @NEW_ITEM_SET_ID OUTPUT
        private int _destinationContactId;
        private int _SourceContactId;
        private Int64 _attributeSetId;
        private int _comparisonId;
        private bool _IncludePDFcoding;
        private int _setId;
        private Int64 _itemId;
        private Int64 _itemArmId;

        private string _Result;
        private Int64 _ItemAttributeId;
        private Int64 _itemSetId;

        public int DestinationContactId
        {
            get { return _destinationContactId; }
        }
        public int SourceContactId
        {
            get { return _SourceContactId; }
        }
        public Int64 attributeSetId
        {
            get { return _attributeSetId; }
        }        
        public int ComparisonId
        {
            get { return _comparisonId; }
        }
        public bool IncludePDFcoding
        {
            get { return _IncludePDFcoding; }
        }
        public int SetId
        {
            get { return _setId; }
        }
        public Int64 ItemId
        {
            get { return _itemId; }
        }
        public Int64 ItemArmId
        {
            get { return _itemArmId; }
        }


        //the members below are output...
        public string Result
        {
            get { return _Result;}
        }
        public Int64 ItemAttributeId
        {
            get { return _ItemAttributeId; }
        }

        public Int64 ItemSetId
        {
            get { return _itemSetId; }
        }


        public ComparisonItemAttributeSaveCommand(int destContactId, int srcContactId, Int64 attrSetId, int comparisonID
            , bool includePdfCoding, int setId, Int64 itemId, Int64 itemArmId)
        {
            _destinationContactId = destContactId;
            _SourceContactId = srcContactId;
            _attributeSetId = attrSetId;
            _comparisonId = comparisonID;
            _IncludePDFcoding = includePdfCoding;
            _setId = setId;
            _itemId = itemId;
            _itemArmId = itemArmId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_destinationContactId", _destinationContactId);
            info.AddValue("_SourceContactId", _SourceContactId); 
            info.AddValue("_attributeSetId", _attributeSetId);
            info.AddValue("_comparisonId", _comparisonId);
            info.AddValue("_IncludePDFcoding", _IncludePDFcoding); 
            info.AddValue("_setId", _setId);
            info.AddValue("_itemId", _itemId);
            info.AddValue("_itemArmId", _itemArmId);

            info.AddValue("_Result", _Result); 
            info.AddValue("_ItemAttributeId", _ItemAttributeId);
            info.AddValue("_itemSetId", _itemSetId);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _destinationContactId = info.GetValue<int>("_destinationContactId");
            _SourceContactId = info.GetValue<int>("_SourceContactId");
            _attributeSetId = info.GetValue<Int64>("_attributeSetId");
            _comparisonId = info.GetValue<int>("_comparisonId");
            _IncludePDFcoding = info.GetValue<bool>("_IncludePDFcoding"); 
            _setId = info.GetValue<int>("_setId");
            _itemId = info.GetValue<Int64>("_itemId");
            _itemArmId = info.GetValue<Int64>("_itemArmId");

            _Result = info.GetValue<string>("_Result"); 
            _ItemAttributeId = info.GetValue<Int64>("_ItemAttributeId");
            _itemSetId = info.GetValue<Int64>("_itemSetId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();

                //step 1: check all is in order. SP will return "forbidden", if the current person can't perform this operation,
                //or if the "destinationContactId" is not in the comparison indicated...
                //otherwise it executes either st_ItemAttributeUpdate or st_ItemAttributeInsert, depending on what's needed...

                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_ComparisonItemAttributeSaveCheckAndRun", connection))
                {
                    
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CurrentContactId", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@DestinationContactId", _destinationContactId));
                    command.Parameters.Add(new SqlParameter("@SourceContactId", _SourceContactId)); 
                    command.Parameters.Add(new SqlParameter("@attributeSetId", _attributeSetId));
                    command.Parameters.Add(new SqlParameter("@comparisonId", _comparisonId));
                    command.Parameters.Add(new SqlParameter("@IncludePDFcoding", _IncludePDFcoding));
                    command.Parameters.Add(new SqlParameter("@SET_ID", _setId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", _itemId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID", _itemArmId == 0 ? (object)DBNull.Value : _itemArmId));

                    command.Parameters.Add(new SqlParameter("@Result", System.Data.SqlDbType.NVarChar, 20));
                    command.Parameters["@Result"].Value = "";
                    command.Parameters["@Result"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NEW_ITEM_ATTRIBUTE_ID", 0));
                    command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NEW_ITEM_SET_ID", 0));
                    command.Parameters["@NEW_ITEM_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    if (command.Parameters["@Result"].Value != DBNull.Value)
                    {
                        _Result = command.Parameters["@Result"].Value.ToString();
                    }
                    if (command.Parameters["@Result"].Value.ToString() == "success")
                    {
                        if (command.Parameters["@NEW_ITEM_SET_ID"].Value != DBNull.Value)
                            _itemSetId = (long)command.Parameters["@NEW_ITEM_SET_ID"].Value;
                        if (command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Value != DBNull.Value)
                            _ItemAttributeId = (long)command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Value;
                    }
                }
                connection.Close();
            }
        }

#endif
    }
}

/*
 * Rules for saving attributeSets for screening
 * 
 * Single screening: completed by default
 * Comparison screening: no auto-reconcilliation (so no auto exclude)
 * Comparison screening: auto-reconcile at code level.
 *      check whether N reviewers agree on code. If they do, it's completed.
 * Comparison screening: auto-reconcile at include / exclude level.
 *      as above. check whether N reviewers have screened, and if they agree on the include / exclude decision
 * Safety first screening: if any reviewer ticks 'include', it's finalised and removed from the 'todo' list
 * 
 * 
 * Include / exclude automatically - just depends whether something is completed. If this is 'true' and item is complete, then auto-exclude is triggered.
 * 
 * 
*/