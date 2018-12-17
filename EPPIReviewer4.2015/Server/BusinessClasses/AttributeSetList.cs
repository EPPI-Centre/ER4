using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Newtonsoft.Json;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;

#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    //public class AttributeSetList : BusinessListBase<AttributeSetList, AttributeSet>
    public class AttributeSetList : DynamicBindingListBase<AttributeSet>
    {
        public static void GetAttributeSetList(int reviewId, EventHandler<DataPortalResult<AttributeSetList>> handler)
        {
            DataPortal<AttributeSetList> dp = new DataPortal<AttributeSetList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<AttributeSetList, int>(reviewId));
        }
        
        public string AddAttribute(AttributeSet attribute, AttributeSet hostAttribute, int position)// adds to the attributes list (not necessarily a new one - from drag/drop)
        {
            if (this.IndexOf(attribute) != -1)
            {
                return "Error: code already in this list. Please go to the 'My info' tab and reload your review.";
            }
            if ((hostAttribute != null) && (hostAttribute.Attributes != this))
            {
                return "Error in move. Please go to the 'My info' tab and reload your review.";
            }
            for (int i = position; i < this.Count; i++)
            {
                this[i].SetIsNew();
                this[i].AttributeOrder = this[i].AttributeOrder + 1;
                this[i].SetIsOld();
            }
            attribute.SetIsNew();
            if (hostAttribute == null)
            {
                attribute.HostAttribute = null;
                attribute.ParentAttributeId = 0;
            }
            else
            {
                attribute.HostAttribute = hostAttribute;
                attribute.ParentAttributeId = hostAttribute.AttributeId;
            }
            this.Insert(position, attribute);
            attribute.AttributeOrder = position;
            attribute.SetIsOld();
            if (this.IndexOf(attribute) == -1)
            {
                return "Error: could not insert code. Please go to the 'My info' tab and reload your review.";
            }
            else
            {
                return "";
            }
        }

        public string MoveWithinList(AttributeSet attribute, int position)
        {
            if (this.IndexOf(attribute) == -1)
            {
                return "Error: code not in list. Please go to the 'My info' tab and reload your review.";
            }
            AttributeSet hostAttribute = attribute.HostAttribute; // as remove (next line) deletes this value
            RemoveAttribute(attribute);
            for (int i = position; i < this.Count; i++)
            {
                this[i].SetIsNew();
                this[i].AttributeOrder = this[i].AttributeOrder + 1;
                this[i].SetIsOld();
            }
            attribute.SetIsNew();
            this.Insert(position, attribute);
            attribute.AttributeOrder = position;
            if (hostAttribute != null)
            {
                attribute.HostAttribute = hostAttribute; // put back the same host attribute
                attribute.ParentAttributeId = hostAttribute.AttributeId;
            }
            attribute.SetIsOld();
            return "";
        }

        public string RemoveAttribute(AttributeSet attribute)
        {
            if (this.IndexOf(attribute) == -1)
            {
                return "Error: code not in list. Please go to the 'My info' tab and reload your review.";
            }
            for (int i = this.IndexOf(attribute) + 1; i < this.Count; i++)
            {
                this[i].SetIsNew();
                this[i].AttributeOrder = this[i].AttributeOrder - 1;
                this[i].SetIsOld();
            }
            attribute.SetIsNew(); // marking the attribute as 'new' means that its removal doesn't get sent to the dataportal
            attribute.HostAttribute = null;
            attribute.ParentAttributeId = 0;
            Remove(attribute);
            attribute.SetIsOld();
            if (this.IndexOf(attribute) == -1)
            {
                return "";
            }
            else
            {
                return "Error: code could not be removed from list. Please go to the 'My info' tab and reload your review.";
            }
            //this.OnRemovingItem(attribute);
        }

        /*
        public void SetIsLocked(bool value)
        {
            foreach (AttributeSet attributeSet in this)
            {
                attributeSet.IsLocked = value;
            }
        }
        */
        [JsonProperty]
        public List<AttributeSet> AttributesList
        {
            get
            {
                return this.ToList<AttributeSet>();
            }
        }


        public AttributeSetList() { }
#if SILVERLIGHT
    protected override void AddNewCore()
    {
        Add(AttributeSet.NewAttributeSet());
    }
#endif

        internal static AttributeSetList NewAttributeSetList()
        {
            return new AttributeSetList();
        }


#if SILVERLIGHT
    public static void GetAttributeSetList(EventHandler<DataPortalResult<AttributeSetList>> handler, int reviewId)
    {
      DataPortal<AttributeSetList> dp = new DataPortal<AttributeSetList>();
      dp.FetchCompleted += handler;
      dp.BeginFetch(new SingleCriteria<AttributeSetList, int>(reviewId));
    }
#else
        protected void DataPortal_Fetch(SingleCriteria<AttributeSetList, int> criteria)
        {
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AttributeSet", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            AttributeSet newAttributeSet = AttributeSet.GetAttributeSet(reader, 0);
                            Add(newAttributeSet);
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }
#endif

    }
}
