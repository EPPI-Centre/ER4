using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;

namespace EppiReviewer4
{
    public class CustomFilterDescriptor : FilterDescriptorBase
    {
        private readonly CompositeFilterDescriptor compositeFilterDesriptor;
        private static readonly ConstantExpression TrueExpression = System.Linq.Expressions.Expression.Constant(true);
        private string filterValue;

        public CustomFilterDescriptor(IEnumerable<GridViewDataColumn> columns)
        {
            this.compositeFilterDesriptor = new CompositeFilterDescriptor();
            this.compositeFilterDesriptor.LogicalOperator = FilterCompositionLogicalOperator.Or;

            foreach (GridViewDataColumn column in columns)
            {
                //if (column.UniqueName == "Rank")
                //    continue;
                this.compositeFilterDesriptor.FilterDescriptors.Add(this.CreateFilterForColumn(column));
            }
        }

        public string FilterValue
        {
            get
            {
                return this.filterValue;
            }
            set
            {
                if (this.filterValue != value)
                {
                    this.filterValue = value;
                    this.UpdateCompositeFilterValues();
                    this.OnPropertyChanged("FilterValue");
                }
            }
        }

        protected override System.Linq.Expressions.Expression CreateFilterExpression(ParameterExpression parameterExpression)
        {
            if (string.IsNullOrEmpty(this.FilterValue))
            {
                return TrueExpression;
            }
            try
            {
                return this.compositeFilterDesriptor.CreateFilterExpression(parameterExpression);
            }
            catch
            {
            }

            return TrueExpression;
        }

        private IFilterDescriptor CreateFilterForColumn(GridViewDataColumn column)
        {
            FilterOperator filterOperator = GetFilterOperatorForType(column.DataType);
            FilterDescriptor descriptor = new FilterDescriptor(column.UniqueName, filterOperator, this.filterValue);
            descriptor.MemberType = column.DataType;

            return descriptor;
        }

        private static FilterOperator GetFilterOperatorForType(Type dataType)
        {
            return dataType == typeof(string) ? FilterOperator.Contains : FilterOperator.IsEqualTo;
        }

        private void UpdateCompositeFilterValues()
        {
            foreach (FilterDescriptor descriptor in this.compositeFilterDesriptor.FilterDescriptors)
            {
                object convertedValue = DefaultValue(descriptor.MemberType);

                try
                {
                    convertedValue = Convert.ChangeType(this.FilterValue, descriptor.MemberType, CultureInfo.CurrentCulture);
                }
                catch
                {
                }

                if (descriptor.MemberType.IsAssignableFrom(typeof(DateTime)))
                {
                    DateTime date;
                    if (DateTime.TryParse(this.FilterValue, out date))
                    {
                        convertedValue = date;
                    }

                }
                //else if (descriptor.MemberType.IsAssignableFrom(typeof(int)))
                //{
                //    continue;
                //    int integer;
                //    if (int.TryParse(this.FilterValue, out integer))
                //    {
                //        convertedValue = integer;
                //    }

                //}

                descriptor.Value = convertedValue;
            }
        }

        private static object DefaultValue(Type type)
        {
            if (type == typeof(int))
            {//problem, no data in "Item" becomes "0" in the UI, which is the default value for integers
             //so when filtering for a non number, you get the clause "(OR [integer column] IsEqualTo 0)" which always applies
             //thus, such columns make all filters match on "int" colums (if no data is present) and no filtering ever happens :-(
             //solution:
                return -2147483648;
                //smallest possible int "(OR [integer column] IsEqualTo -2147483648)" is used when the filter doesn't parse into an Int
            }
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
    }
}