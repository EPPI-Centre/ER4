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
using BusinessLibrary.Security;
#else
using System.Windows.Media;
using System.Windows;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ItemAttributePDF : BusinessBase<ItemAttributePDF>
    {
#if SILVERLIGHT
        public ItemAttributePDF() { }
        public ItemAttributePDF(Int64 itemDocumentId, Int64 itemAttributeId, int page)
        {
            inPageSelections = new MobileList<InPageSelections>();
            _Shape = new PathGeometry { FillRule = FillRule.Nonzero };
            ItemAttributeId = itemAttributeId;
            ItemDocumentId = itemDocumentId;
            LoadProperty(PageProperty, page);
            LoadProperty(ItemAttributePDFIdProperty, -1);
            this.MarkNew();
        }
        private PathGeometry _Shape;
        public PathGeometry Shape
        { 
            get 
            {
                if (_Shape == null) 
                {
                    if (ShapeTxt != null && ShapeTxt != "")
                        _Shape = makeGeom(ShapeTxt);
                    else _Shape = new PathGeometry();
                }
                return _Shape;
            }
            set
            {
                _Shape = value;
                LoadProperty(ShapeTxtProperty,_Shape.ToString());
            }
        }
        public static PathGeometry makeGeom(string rep)
        {
            PathGeometry res = new PathGeometry();
            if (rep.Length <= 3) return res;
            rep = rep.Substring(3);
            string[] s1 = rep.Split('M');
            string[] s2 = null;
            string[] s3 = null;
            char[] charsToTrim = { 'z' };
            System.Globalization.CultureInfo cult = new System.Globalization.CultureInfo("en-GB");

            foreach (string ts in s1)
            {
                try
                {
                    PathFigure pf = new PathFigure();
                    PolyLineSegment pls = new PolyLineSegment();
                    s2 = ts.Split('L');

                    s3 = s2[0].Split(',');
                    pf.StartPoint = new Point(double.Parse(s3[0], cult), double.Parse(s3[1], cult));
                    s3 = s2[1].Split(',');
                    pls.Points.Add(new Point(double.Parse(s3[0], cult), double.Parse(s3[1], cult)));
                    s3 = s2[2].Split(',');
                    pls.Points.Add(new Point(double.Parse(s3[0], cult), double.Parse(s3[1], cult)));
                    s3 = s2[3].Split(',');
                    pls.Points.Add(new Point(double.Parse(s3[0], cult), double.Parse(s3[1].Trim(charsToTrim), cult)));
                    pf.Segments.Add(pls);
                    pf.IsClosed = true;
                    pf.IsFilled = true;
                    res.Figures.Add(pf);
                }
                catch (Exception e)
                {
                    string oops = e.Message;
                }
            }
            return res;
        }

#else
        private ItemAttributePDF() 
        {
            inPageSelections = new MobileList<InPageSelections>();
        }
        
#endif
        private static readonly char[] charsTosnip = { '¬' };
        private static PropertyInfo<Int64> ItemAttributePDFIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemAttributePDFId", "ItemAttributePDFId"));
        public Int64 ItemAttributePDFId
        {
            get
            {
                return GetProperty(ItemAttributePDFIdProperty);
            }
        }

        private static PropertyInfo<Int64> ItemDocumentIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemDocumentId", "ItemDocumentId"));
        public Int64 ItemDocumentId
        {
            get
            {
                return GetProperty(ItemDocumentIdProperty);
            }
            set
            {
                SetProperty(ItemDocumentIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> ItemAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemAttributeId", "ItemAttributeId"));
        public Int64 ItemAttributeId
        {
            get
            {
                return GetProperty(ItemAttributeIdProperty);
            }
            set
            {
                SetProperty(ItemAttributeIdProperty, value);
            }
        }
        private static PropertyInfo<string> ShapeTxtProperty = RegisterProperty<string>(new PropertyInfo<string>("ShapeTxt", "ShapeTxt"));
        public string ShapeTxt
        {
            get { return GetProperty(ShapeTxtProperty); }
            set
            {
                SetProperty(ShapeTxtProperty, value);
            }
        }

        private static PropertyInfo<MobileList<InPageSelections>> inPageSelectionsProperty = RegisterProperty<MobileList<InPageSelections>>(new PropertyInfo<MobileList<InPageSelections>>("inPageSelections", "inPageSelections"));
        public MobileList<InPageSelections> inPageSelections
        {
            get { return GetProperty(inPageSelectionsProperty); }
            set { SetProperty(inPageSelectionsProperty, value); }
        }

        private static PropertyInfo<int> PageProperty = RegisterProperty<int>(new PropertyInfo<int>("Page", "Page"));
        public int Page
        {
            get
            {
                if (GetProperty(PageProperty) != null) return GetProperty(PageProperty);
                else return -1;
            }
        }
        private string Intervals
        {
            get
            {
                string res = "";
                foreach (InPageSelections sel in inPageSelections)
                {
                    res += sel.Start.ToString() + ";" + sel.End.ToString() + "¬";
                }
                res = res.Trim(charsTosnip);
                return res;
            }
        }
        private string Texts
        {
            get
            {
                string res = "";
                foreach (InPageSelections sel in inPageSelections)
                {
                    res += sel.SelTxt + "¬";
                }
                res = res.Trim(charsTosnip);
                return res;
            }
        }
#if !SILVERLIGHT
        public static ItemAttributePDF GetItemAttributePDF(SafeDataReader reader)
        {
            
            string[] sten;
            string[] textsels;
            ItemAttributePDF returnValue = new ItemAttributePDF();
            returnValue.LoadProperty<Int64>(ItemAttributePDFIdProperty, reader.GetInt64("ITEM_ATTRIBUTE_PDF_ID"));
            returnValue.LoadProperty<Int64>(ItemDocumentIdProperty, reader.GetInt64("ITEM_DOCUMENT_ID"));
            returnValue.LoadProperty<Int64>(ItemAttributeIdProperty, reader.GetInt64("ITEM_ATTRIBUTE_ID"));
            returnValue.LoadProperty<int>(PageProperty, reader.GetInt32("PAGE"));
            returnValue.LoadProperty<string>(ShapeTxtProperty, reader.GetString("SHAPE_TEXT"));
            sten = reader.GetString("SELECTION_INTERVALS").Split(charsTosnip);
            textsels = reader.GetString("SELECTION_TEXTS").Split(charsTosnip);
            int start, end;
            string tStr;
            string[] tarr;
            InPageSelections ppas;
            for (int i = 0; i < sten.Length; i++)
            {
                tarr = sten[i].Split(';');
                start = int.Parse(tarr[0]);
                end = int.Parse(tarr[1]);
                tStr = textsels[i];
                ppas = new InPageSelections(start, end, tStr);
                returnValue.inPageSelections.Add(ppas);
            }
            returnValue.MarkOld();
            return returnValue;
        }
        protected override void DataPortal_Insert()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int tocheck = ri.ReviewId;

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributePDFInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID", ReadProperty(ItemAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_DOCUMENT_ID", ReadProperty(ItemDocumentIdProperty)));
                    command.Parameters.Add(new SqlParameter("@PAGE", ReadProperty(PageProperty)));
                    command.Parameters.Add(new SqlParameter("@SHAPE_TEXT", ShapeTxt));
                    command.Parameters.Add(new SqlParameter("@INTERVALS", Intervals));
                    command.Parameters.Add(new SqlParameter("@TEXTS", Texts));
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_PDF_ID", 0));
                    command.Parameters["@ITEM_ATTRIBUTE_PDF_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ItemAttributePDFIdProperty, command.Parameters["@ITEM_ATTRIBUTE_PDF_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int tocheck = ri.ReviewId;
            if (ItemAttributePDFId == -1) //for some reason, that I don't understand, a new item created on ui side, has the right "isnew" value in SL, but becomes "isnew = false" on server side!
            {//this detects that the item is new, and sends it to the right method
                DataPortal_Insert();
                return;
            }
            else if
              (inPageSelections == null || inPageSelections.Count < 1)
            {
                this.MarkDeleted();
                DataPortal_DeleteSelf();
            }
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributePDFUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_PDF_ID", ReadProperty(ItemAttributePDFIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SHAPE_TEXT", ShapeTxt));
                    command.Parameters.Add(new SqlParameter("@INTERVALS", Intervals));
                    command.Parameters.Add(new SqlParameter("@TEXTS", Texts));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int tocheck = ri.ReviewId;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributePDFDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_PDF_ID", ReadProperty(ItemAttributePDFIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        

#endif
    }
    [Serializable]
    public class InPageSelections : BusinessBase<InPageSelections>, IComparable<InPageSelections>
    {
        
        private static PropertyInfo<int> StartProperty = RegisterProperty<int>(new PropertyInfo<int>("Start", "Start"));
        public int Start
        {
            get { return GetProperty(StartProperty); }
            set
            {
                SetProperty(StartProperty, value);
            }
        }
        
        private static PropertyInfo<int> EndProperty = RegisterProperty<int>(new PropertyInfo<int>("End", "End"));
        public int End
        {
            get { return GetProperty(EndProperty); }
            set
            {
                SetProperty(EndProperty, value);
            }
        }
        
        private static PropertyInfo<string> SelTxtProperty = RegisterProperty<string>(new PropertyInfo<string>("SelTxt", "SelTxt"));
        public string SelTxt
        {
            get { return GetProperty(SelTxtProperty); }
            set
            {
                SetProperty(SelTxtProperty, value);
            }
        }
        public InPageSelections()
        {
        }
        public InPageSelections(int start, int end, string selTxt)
        {
            Start = start;
            End = end;
            selTxt = selTxt.Replace('¬', '?');
            selTxt = selTxt.Replace(Environment.NewLine, " ");
            selTxt = selTxt.Replace("\n", " ");
            selTxt = selTxt.Replace("\r", " ");
            SelTxt = selTxt;
        }
        public int CompareTo(InPageSelections other)
        {
            // If other is not a valid object reference, this instance is greater.
            if (other == null) return 1;

            // The temperature comparison depends on the comparison of 
            // the underlying Double values. 
            return Start.CompareTo(other.Start);
        }
    }
}