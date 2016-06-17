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

#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyItemAttributeCrosstab : ReadOnlyBase<ReadOnlyItemAttributeCrosstab>
    {

#if SILVERLIGHT
    public ReadOnlyItemAttributeCrosstab() { }
#else
        private ReadOnlyItemAttributeCrosstab() { }
#endif

        private static PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
        }

        private static PropertyInfo<string> AttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeName", "AttributeName", string.Empty));
        public string AttributeName
        {
            get
            {
                return GetProperty(AttributeNameProperty);
            }
        }

        private static PropertyInfo<int> Field1Property = RegisterProperty<int>(new PropertyInfo<int>("Field1", "Field1", 0));
        public int Field1
        {
            get
            {
                return GetProperty(Field1Property);
            }
        }

        private static PropertyInfo<int> Field2Property = RegisterProperty<int>(new PropertyInfo<int>("Field2", "Field2", 0));
        public int Field2
        {
            get
            {
                return GetProperty(Field2Property);
            }
        }

        private static PropertyInfo<int> Field3Property = RegisterProperty<int>(new PropertyInfo<int>("Field3", "Field3", 0));
        public int Field3
        {
            get
            {
                return GetProperty(Field3Property);
            }
        }

        private static PropertyInfo<int> Field4Property = RegisterProperty<int>(new PropertyInfo<int>("Field4", "Field4", 0));
        public int Field4
        {
            get
            {
                return GetProperty(Field4Property);
            }
        }

        private static PropertyInfo<int> Field5Property = RegisterProperty<int>(new PropertyInfo<int>("Field5", "Field5", 0));
        public int Field5
        {
            get
            {
                return GetProperty(Field5Property);
            }
        }

        private static PropertyInfo<int> Field6Property = RegisterProperty<int>(new PropertyInfo<int>("Field6", "Field6", 0));
        public int Field6
        {
            get
            {
                return GetProperty(Field6Property);
            }
        }

        private static PropertyInfo<int> Field7Property = RegisterProperty<int>(new PropertyInfo<int>("Field7", "Field7", 0));
        public int Field7
        {
            get
            {
                return GetProperty(Field7Property);
            }
        }

        private static PropertyInfo<int> Field8Property = RegisterProperty<int>(new PropertyInfo<int>("Field8", "Field8", 0));
        public int Field8
        {
            get
            {
                return GetProperty(Field8Property);
            }
        }

        private static PropertyInfo<int> Field9Property = RegisterProperty<int>(new PropertyInfo<int>("Field9", "Field9", 0));
        public int Field9
        {
            get
            {
                return GetProperty(Field9Property);
            }
        }

        private static PropertyInfo<int> Field10Property = RegisterProperty<int>(new PropertyInfo<int>("Field10", "Field10", 0));
        public int Field10
        {
            get
            {
                return GetProperty(Field10Property);
            }
        }

        private static PropertyInfo<int> Field11Property = RegisterProperty<int>(new PropertyInfo<int>("Field11", "Field11", 0));
        public int Field11
        {
            get
            {
                return GetProperty(Field11Property);
            }
        }

        private static PropertyInfo<int> Field12Property = RegisterProperty<int>(new PropertyInfo<int>("Field12", "Field12", 0));
        public int Field12
        {
            get
            {
                return GetProperty(Field12Property);
            }
        }

        private static PropertyInfo<int> Field13Property = RegisterProperty<int>(new PropertyInfo<int>("Field13", "Field13", 0));
        public int Field13
        {
            get
            {
                return GetProperty(Field13Property);
            }
        }

        private static PropertyInfo<int> Field14Property = RegisterProperty<int>(new PropertyInfo<int>("Field14", "Field14", 0));
        public int Field14
        {
            get
            {
                return GetProperty(Field14Property);
            }
        }

        private static PropertyInfo<int> Field15Property = RegisterProperty<int>(new PropertyInfo<int>("Field15", "Field15", 0));
        public int Field15
        {
            get
            {
                return GetProperty(Field15Property);
            }
        }

        private static PropertyInfo<int> Field16Property = RegisterProperty<int>(new PropertyInfo<int>("Field16", "Field16", 0));
        public int Field16
        {
            get
            {
                return GetProperty(Field16Property);
            }
        }

        private static PropertyInfo<int> Field17Property = RegisterProperty<int>(new PropertyInfo<int>("Field17", "Field17", 0));
        public int Field17
        {
            get
            {
                return GetProperty(Field17Property);
            }
        }

        private static PropertyInfo<int> Field18Property = RegisterProperty<int>(new PropertyInfo<int>("Field18", "Field18", 0));
        public int Field18
        {
            get
            {
                return GetProperty(Field18Property);
            }
        }

        private static PropertyInfo<int> Field19Property = RegisterProperty<int>(new PropertyInfo<int>("Field19", "Field19", 0));
        public int Field19
        {
            get
            {
                return GetProperty(Field19Property);
            }
        }

        private static PropertyInfo<int> Field20Property = RegisterProperty<int>(new PropertyInfo<int>("Field20", "Field20", 0));
        public int Field20
        {
            get
            {
                return GetProperty(Field20Property);
            }
        }

        private static PropertyInfo<int> Field21Property = RegisterProperty<int>(new PropertyInfo<int>("Field21", "Field21", 0));
        public int Field21
        {
            get
            {
                return GetProperty(Field21Property);
            }
        }

        private static PropertyInfo<int> Field22Property = RegisterProperty<int>(new PropertyInfo<int>("Field22", "Field22", 0));
        public int Field22
        {
            get
            {
                return GetProperty(Field22Property);
            }
        }

        private static PropertyInfo<int> Field23Property = RegisterProperty<int>(new PropertyInfo<int>("Field23", "Field23", 0));
        public int Field23
        {
            get
            {
                return GetProperty(Field23Property);
            }
        }

        private static PropertyInfo<int> Field24Property = RegisterProperty<int>(new PropertyInfo<int>("Field24", "Field24", 0));
        public int Field24
        {
            get
            {
                return GetProperty(Field24Property);
            }
        }

        private static PropertyInfo<int> Field25Property = RegisterProperty<int>(new PropertyInfo<int>("Field25", "Field25", 0));
        public int Field25
        {
            get
            {
                return GetProperty(Field25Property);
            }
        }

        private static PropertyInfo<int> Field26Property = RegisterProperty<int>(new PropertyInfo<int>("Field26", "Field26", 0));
        public int Field26
        {
            get
            {
                return GetProperty(Field26Property);
            }
        }

        private static PropertyInfo<int> Field27Property = RegisterProperty<int>(new PropertyInfo<int>("Field27", "Field27", 0));
        public int Field27
        {
            get
            {
                return GetProperty(Field27Property);
            }
        }

        private static PropertyInfo<int> Field28Property = RegisterProperty<int>(new PropertyInfo<int>("Field28", "Field28", 0));
        public int Field28
        {
            get
            {
                return GetProperty(Field28Property);
            }
        }

        private static PropertyInfo<int> Field29Property = RegisterProperty<int>(new PropertyInfo<int>("Field29", "Field29", 0));
        public int Field29
        {
            get
            {
                return GetProperty(Field29Property);
            }
        }

        private static PropertyInfo<int> Field30Property = RegisterProperty<int>(new PropertyInfo<int>("Field30", "Field30", 0));
        public int Field30
        {
            get
            {
                return GetProperty(Field30Property);
            }
        }

        private static PropertyInfo<int> Field31Property = RegisterProperty<int>(new PropertyInfo<int>("Field31", "Field31", 0));
        public int Field31
        {
            get
            {
                return GetProperty(Field31Property);
            }
        }

        private static PropertyInfo<int> Field32Property = RegisterProperty<int>(new PropertyInfo<int>("Field32", "Field32", 0));
        public int Field32
        {
            get
            {
                return GetProperty(Field32Property);
            }
        }

        private static PropertyInfo<int> Field33Property = RegisterProperty<int>(new PropertyInfo<int>("Field33", "Field33", 0));
        public int Field33
        {
            get
            {
                return GetProperty(Field33Property);
            }
        }

        private static PropertyInfo<int> Field34Property = RegisterProperty<int>(new PropertyInfo<int>("Field34", "Field34", 0));
        public int Field34
        {
            get
            {
                return GetProperty(Field34Property);
            }
        }

        private static PropertyInfo<int> Field35Property = RegisterProperty<int>(new PropertyInfo<int>("Field35", "Field35", 0));
        public int Field35
        {
            get
            {
                return GetProperty(Field35Property);
            }
        }

        private static PropertyInfo<int> Field36Property = RegisterProperty<int>(new PropertyInfo<int>("Field36", "Field36", 0));
        public int Field36
        {
            get
            {
                return GetProperty(Field36Property);
            }
        }

        private static PropertyInfo<int> Field37Property = RegisterProperty<int>(new PropertyInfo<int>("Field37", "Field37", 0));
        public int Field37
        {
            get
            {
                return GetProperty(Field37Property);
            }
        }

        private static PropertyInfo<int> Field38Property = RegisterProperty<int>(new PropertyInfo<int>("Field38", "Field38", 0));
        public int Field38
        {
            get
            {
                return GetProperty(Field38Property);
            }
        }

        private static PropertyInfo<int> Field39Property = RegisterProperty<int>(new PropertyInfo<int>("Field39", "Field39", 0));
        public int Field39
        {
            get
            {
                return GetProperty(Field39Property);
            }
        }

        private static PropertyInfo<int> Field40Property = RegisterProperty<int>(new PropertyInfo<int>("Field40", "Field40", 0));
        public int Field40
        {
            get
            {
                return GetProperty(Field40Property);
            }
        }

        private static PropertyInfo<int> Field41Property = RegisterProperty<int>(new PropertyInfo<int>("Field41", "Field41", 0));
        public int Field41
        {
            get
            {
                return GetProperty(Field41Property);
            }
        }

        private static PropertyInfo<int> Field42Property = RegisterProperty<int>(new PropertyInfo<int>("Field42", "Field42", 0));
        public int Field42
        {
            get
            {
                return GetProperty(Field42Property);
            }
        }

        private static PropertyInfo<int> Field43Property = RegisterProperty<int>(new PropertyInfo<int>("Field43", "Field43", 0));
        public int Field43
        {
            get
            {
                return GetProperty(Field43Property);
            }
        }

        private static PropertyInfo<int> Field44Property = RegisterProperty<int>(new PropertyInfo<int>("Field44", "Field44", 0));
        public int Field44
        {
            get
            {
                return GetProperty(Field44Property);
            }
        }

        private static PropertyInfo<int> Field45Property = RegisterProperty<int>(new PropertyInfo<int>("Field45", "Field45", 0));
        public int Field45
        {
            get
            {
                return GetProperty(Field45Property);
            }
        }

        private static PropertyInfo<int> Field46Property = RegisterProperty<int>(new PropertyInfo<int>("Field46", "Field46", 0));
        public int Field46
        {
            get
            {
                return GetProperty(Field46Property);
            }
        }

        private static PropertyInfo<int> Field47Property = RegisterProperty<int>(new PropertyInfo<int>("Field47", "Field47", 0));
        public int Field47
        {
            get
            {
                return GetProperty(Field47Property);
            }
        }

        private static PropertyInfo<int> Field48Property = RegisterProperty<int>(new PropertyInfo<int>("Field48", "Field48", 0));
        public int Field48
        {
            get
            {
                return GetProperty(Field48Property);
            }
        }

        private static PropertyInfo<int> Field49Property = RegisterProperty<int>(new PropertyInfo<int>("Field49", "Field49", 0));
        public int Field49
        {
            get
            {
                return GetProperty(Field49Property);
            }
        }

        private static PropertyInfo<int> Field50Property = RegisterProperty<int>(new PropertyInfo<int>("Field50", "Field50", 0));
        public int Field50
        {
            get
            {
                return GetProperty(Field50Property);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyItemAttributeCrosstab), canRead);
        //}

#if !SILVERLIGHT

        public static ReadOnlyItemAttributeCrosstab GetReadOnlyItemAttributeCrosstab(SafeDataReader reader, int NXAxis)
        {
            return DataPortal.FetchChild<ReadOnlyItemAttributeCrosstab>(reader, NXAxis);
        }

        private void Child_Fetch(SafeDataReader reader, int NXAxis)
        {
            PropertyInfo<int> [] fields = new PropertyInfo<int>[50];
            fields[0] = Field1Property;
            fields[1] = Field2Property;
            fields[2] = Field3Property;
            fields[3] = Field4Property;
            fields[4] = Field5Property;
            fields[5] = Field6Property;
            fields[6] = Field7Property;
            fields[7] = Field8Property;
            fields[8] = Field9Property;
            fields[9] = Field10Property;
            fields[10] = Field11Property;
            fields[11] = Field12Property;
            fields[12] = Field13Property;
            fields[13] = Field14Property;
            fields[14] = Field15Property;
            fields[15] = Field16Property;
            fields[16] = Field17Property;
            fields[17] = Field18Property;
            fields[18] = Field19Property;
            fields[19] = Field20Property;
            fields[20] = Field21Property;
            fields[21] = Field22Property;
            fields[22] = Field23Property;
            fields[23] = Field24Property;
            fields[24] = Field25Property;
            fields[25] = Field26Property;
            fields[26] = Field27Property;
            fields[27] = Field28Property;
            fields[28] = Field29Property;
            fields[29] = Field30Property;
            fields[30] = Field31Property;
            fields[31] = Field32Property;
            fields[32] = Field33Property;
            fields[33] = Field34Property;
            fields[34] = Field35Property;
            fields[35] = Field36Property;
            fields[36] = Field37Property;
            fields[37] = Field38Property;
            fields[38] = Field39Property;
            fields[39] = Field40Property;
            fields[40] = Field41Property;
            fields[41] = Field42Property;
            fields[42] = Field43Property;
            fields[43] = Field44Property;
            fields[44] = Field45Property;
            fields[45] = Field46Property;
            fields[46] = Field47Property;
            fields[47] = Field48Property;
            fields[48] = Field49Property;
            fields[49] = Field50Property;

            LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID2"));
            LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
            for (int i = 0; i < Math.Min(NXAxis, 50); i++)
            {
                LoadProperty<int>(fields[i], reader.GetInt32(i + 3));
            }
        }


#endif
    }
}
