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
    public class WorkAllocationFromWizardCommand : CommandBase<WorkAllocationFromWizardCommand>
    {
#if SILVERLIGHT
    public WorkAllocationFromWizardCommand(){}
#else
        public WorkAllocationFromWizardCommand() { }
#endif

        private string _filterType;
        private Int64 _attributeIdFilter;
        private int _setIdFilter;
        private Int64 _destination_Attribute_ID;
        private int _destination_Set_ID;//set where new allocation codes are created
        
        private int _percentageOfWholePot;
        private bool _included;

        public static readonly PropertyInfo<MobileList<MobileList<string>>> previewProperty = RegisterProperty<MobileList<MobileList<string>>>(new PropertyInfo<MobileList<MobileList<string>>>("preview", "preview"));
        public MobileList<MobileList<string>> preview
        {
            get { return ReadProperty(previewProperty); }
            set { LoadProperty(previewProperty, value); }
        }
        
        private int _isPreview = 1;
        private int _work_to_do_setID; //"assign codes from this set"
        private bool _oneGroupPerPerson = false;
        private int _peoplePerItem = 1;
        private string _reviewersIds = "";
        private string _reviewerNames = "";
        private string _itemsPerEachReviewer = "";
        private string _groupsPrefix = "";
        private int _numberOfItemsToAssign = 0;
        private int _numberOfAffectedItems = 0;
        private bool _isSuccess = false;
        //,@IsPreview int = 1
        //,@OneGroupPerPerson bit = 0
        //,@PeoplePerItem int = 1
        //,@ReviewersIds varchar(8000) = ''
        //,@ReviewerNames varchar(8000) = ''
        //,@ItemsPerEachReviewer varchar(8000) = ''
        //,@GroupsPrefix varchar(100) = ''
        //@Work_to_do_setID int = -1
        //@NumberOfAffectedItems

        public WorkAllocationFromWizardCommand(string filterType, Int64 attributeIdFilter, int setIdFilter, Int64 destination_Attribute_ID, int destination_Set_ID, int percentageOfWholePot, bool included)
        {
            _filterType = filterType;
            _attributeIdFilter = attributeIdFilter;
            _setIdFilter = setIdFilter;
            _destination_Attribute_ID = destination_Attribute_ID;
            _destination_Set_ID = destination_Set_ID;
            _percentageOfWholePot = percentageOfWholePot;
            _included = included;
            _isPreview = 1;
        }
        public WorkAllocationFromWizardCommand(string filterType, Int64 attributeIdFilter, int setIdFilter, Int64 destination_Attribute_ID, int destination_Set_ID, int percentageOfWholePot, bool included
                                                , int isPreview, int work_to_do_setID, bool oneGroupPerPerson, int PeoplePerItem, string reviewersIds, string reviewerNames
                                                , string itemsPerEachReviewer, string groupsPrefix, int numberOfItemsToAssign )
        {
            _filterType = filterType;
            _attributeIdFilter = attributeIdFilter;
            _setIdFilter = setIdFilter;
            _destination_Attribute_ID = destination_Attribute_ID;
            _destination_Set_ID = destination_Set_ID;
            _percentageOfWholePot = percentageOfWholePot;
            _included = included;
            _isPreview = isPreview;
            _work_to_do_setID = work_to_do_setID;
            _oneGroupPerPerson = oneGroupPerPerson;
            _peoplePerItem = PeoplePerItem;
            _reviewersIds = reviewersIds;
            _reviewerNames = reviewerNames;
            _itemsPerEachReviewer = itemsPerEachReviewer;
            _groupsPrefix = groupsPrefix;
            _numberOfItemsToAssign = numberOfItemsToAssign;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_filterType", _filterType);
            info.AddValue("_attributeIdFilter", _attributeIdFilter);
            info.AddValue("_setIdFilter", _setIdFilter);
            info.AddValue("_destination_Attribute_ID", _destination_Attribute_ID);
            info.AddValue("_destination_Set_ID", _destination_Set_ID);
            info.AddValue("_percentageOfWholePot", _percentageOfWholePot);
            info.AddValue("_included", _included);


            //info.AddValue("_preview", _preview);
            info.AddValue("_isPreview", _isPreview);
            info.AddValue("_work_to_do_setID", _work_to_do_setID);
            info.AddValue("_oneGroupPerPerson", _oneGroupPerPerson);
            info.AddValue("_PeoplePerItem", _peoplePerItem);
            info.AddValue("_reviewersIds", _reviewersIds);
            info.AddValue("_reviewerNames", _reviewerNames);
            info.AddValue("_itemsPerEachReviewer", _itemsPerEachReviewer);
            info.AddValue("_groupsPrefix", _groupsPrefix);
            info.AddValue("_numberOfItemsToAssign", _numberOfItemsToAssign);
            info.AddValue("_numberOfAffectedItems", _numberOfAffectedItems);
            info.AddValue("_isSuccess", _isSuccess); 
    }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _filterType = info.GetValue<string>("_filterType");
            _attributeIdFilter = info.GetValue<Int64>("_attributeIdFilter");
            _setIdFilter = info.GetValue<int>("_setIdFilter");
            _destination_Attribute_ID = info.GetValue<Int64>("_destination_Attribute_ID");
            _destination_Set_ID = info.GetValue<int>("_destination_Set_ID");
             _work_to_do_setID = info.GetValue<int>("_work_to_do_setID");
            _percentageOfWholePot = info.GetValue<int>("_percentageOfWholePot");
            _included = info.GetValue<bool>("_included");
            //_preview = info.GetValue<MobileList<MobileList<string>>>("_preview");

            _isPreview = info.GetValue<int>("_isPreview");
            _oneGroupPerPerson = info.GetValue<bool>("_oneGroupPerPerson");
            _peoplePerItem = info.GetValue<int>("_PeoplePerItem");
            _reviewersIds = info.GetValue<string>("_reviewersIds");
            _reviewerNames = info.GetValue<string>("_reviewerNames");
            _itemsPerEachReviewer = info.GetValue<string>("_itemsPerEachReviewer");
            _groupsPrefix = info.GetValue<string>("_groupsPrefix");
            _numberOfItemsToAssign = info.GetValue<int>("_numberOfItemsToAssign");
            _numberOfAffectedItems = info.GetValue<int>("_numberOfAffectedItems");
            _isSuccess = info.GetValue<bool>("_isSuccess"); 
        }
        public int NumberOfAffectedItems
        {
            get { return _numberOfAffectedItems; }
        }
        
        public bool IsSuccess
        {
            get { return _isSuccess; }
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_ReviewWorkAllocationCheckOrInsertFromWizard", connection))
                {
                    command.CommandTimeout = 500; // if you are allocating thousands of items, it can take a while
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@IsPreview", _isPreview));
                    command.Parameters.Add(new SqlParameter("@Requestor_id", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@FILTER_TYPE", _filterType));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_FILTER", _attributeIdFilter));
                    command.Parameters.Add(new SqlParameter("@SET_ID_FILTER", _setIdFilter));
                    command.Parameters.Add(new SqlParameter("@Destination_Attribute_ID", _destination_Attribute_ID));
                    command.Parameters.Add(new SqlParameter("@Destination_set_ID", _destination_Set_ID));
                    command.Parameters.Add(new SqlParameter("@PercentageOfWholePot", _percentageOfWholePot));
                    command.Parameters.Add(new SqlParameter("@INCLUDED", _included));
                    if (_isPreview != 1)
                    {
                        command.Parameters.Add(new SqlParameter("@Work_to_do_setID", _work_to_do_setID));
                        command.Parameters.Add(new SqlParameter("@OneGroupPerPerson", _oneGroupPerPerson));
                        command.Parameters.Add(new SqlParameter("@PeoplePerItem", _peoplePerItem));
                        command.Parameters.Add(new SqlParameter("@ReviewersIds", _reviewersIds));
                        command.Parameters.Add(new SqlParameter("@ReviewerNames", _reviewerNames));
                        command.Parameters.Add(new SqlParameter("@ItemsPerEachReviewer", _itemsPerEachReviewer));
                        command.Parameters.Add(new SqlParameter("@GroupsPrefix", _groupsPrefix));
                        command.Parameters.Add(new SqlParameter("@NumberOfItemsToAssign", _numberOfItemsToAssign));
                    }
                    SqlParameter par = new SqlParameter("@NumberOfAffectedItems", System.Data.SqlDbType.Int);
                    par.Direction = System.Data.ParameterDirection.Output;
                    SqlParameter par2 = new SqlParameter("@Success", System.Data.SqlDbType.Bit);
                    par2.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(par);
                    command.Parameters.Add(par2);
                    if (_isPreview == 2)
                    {
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {

                            preview = new MobileList<MobileList<string>>();
                            int columnsN = reader.FieldCount;
                            MobileList<string> FirstRow = new MobileList<string>();
                            for (int i = 0; i < columnsN; i++)
                            {
                                FirstRow.Add(reader.GetName(i));
                            }
                            preview.Add(FirstRow);
                            MobileList<string> OneRow = new MobileList<string>();
                            while (reader.Read())
                            {
                                OneRow = new MobileList<string>();
                                for (int i = 0; i < columnsN; i++)
                                {
                                    OneRow.Add(reader.GetString(FirstRow[i]));
                                }
                                preview.Add(OneRow);
                            }
                        }
                    }
                    else
                    {// we only care about the two output params.
                        command.ExecuteNonQuery();
                    }
                    _numberOfAffectedItems = (int)command.Parameters["@NumberOfAffectedItems"].Value;
                    _isSuccess = (bool)command.Parameters["@Success"].Value;
                }
                connection.Close();
            }
        }

#endif
    }
}
