using System;
using System.Collections.Generic;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using Csla.DataPortalClient;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReportData : ReadOnlyBase<ReportData>
    {
#if !SILVERLIGHT
        internal void Get(string name,
                            string orderBy,
                            int report_id, 
                            bool isQuestion, 
                            bool isHorizontal, bool showItemId, bool showOldItemId, bool showOutcomes,
                            bool showFullTitle, bool showAbstract, bool showYear, bool showShortTitle,
                            Csla.Data.SafeDataReader reader)
        {
            LoadProperty(ReportNameProperty, name);
            LoadProperty(OrderByProperty, orderBy);
            LoadProperty(ReportIdProperty, report_id);
            LoadProperty(IsQuestionProperty, isQuestion);
            LoadProperty(IsHorizontalProperty, isHorizontal);
            LoadProperty(ShowItemIdProperty, showItemId);
            LoadProperty(ShowOldItemIdProperty, showOldItemId);
            LoadProperty(ShowOutcomesProperty, showOutcomes);
            LoadProperty(ShowFullTitleProperty, showFullTitle);
            LoadProperty(ShowAbstractProperty, showAbstract);
            LoadProperty(ShowYearProperty, showYear);
            LoadProperty(ShowShortTitleProperty, showShortTitle);
            LoadProperty(ReportColumnsProperty, new MobileList<ReportColumnData>());
            LoadProperty(ReportItemsProperty, new MobileList<ReportItem>());
            LoadProperty(ReportAttributesProperty, new MobileList<ReportAttribute>());
            reader.NextResult();
            while (reader.Read())
            {
                //get tb_report_column(s) to use in ReportColumns field
                ReportColumns.Add(new ReportColumnData(reader.GetInt32("REPORT_COLUMN_ID"), reader.GetString("REPORT_COLUMN_NAME"), reader.GetInt32("COLUMN_ORDER")));
            }
            reader.NextResult();
            while (reader.Read())
            {
                //get tb_report_column_code(s) foreach line, reportData.AddColumnCode(column id, new ColumnRow(...))
                AddColumnCode(reader.GetInt32("REPORT_COLUMN_ID"), new ColumnRow(reader.GetInt32("REPORT_COLUMN_CODE_ID")
                                                                                        , reader.GetString("USER_DEF_TEXT")
                                                                                        , reader.GetBoolean("DISPLAY_CODE")
                                                                                        , reader.GetBoolean("DISPLAY_ADDITIONAL_TEXT")
                                                                                        , reader.GetBoolean("DISPLAY_CODED_TEXT")
                                                                                        ));
            }
            reader.NextResult();
            ReportItem rit = new ReportItem(-1
                                        , ""
                                        , ""
                                        , ""
                                        );
            Int64 CurrID = -1;
            //get bigQuery: item_attributes joined with tb_report_column_code add each line as ReportItem, add attributes to their own list
            while (reader.Read())
            {
                //see if the current attribute is already known, if not add
                ReportAttribute ra = findAttributeById(reader.GetInt64("ATTRIBUTE_ID"), reader.GetString("ARM_NAME"));
                if (ra == null)
                {//create attr and add
                    ra = new ReportAttribute(reader.GetInt64("ATTRIBUTE_ID"), reader.GetString("ATTRIBUTE_NAME"), reader.GetString("ARM_NAME"));
                    if (!ReportAttributes.Contains(ra)) ReportAttributes.Add(ra);
                }

                if (rit.ItemId == -1 || reader.GetInt64("ITEM_ID") != CurrID)
                {
                    if (rit.ItemId != -1) ReportItems.Add(rit);
                    //create new reportItem
                    CurrID = reader.GetInt64("ITEM_ID");
                    rit = new ReportItem(CurrID
                                        , reader.GetString("SHORT_TITLE")
                                        , reader.GetString("USER_DEF_TEXT")
                                        , reader.GetString("OLD_ITEM_ID")
                                        );

                    //put data in ItemColumnsData
                    foreach (ReportColumnData rcd in ReportColumns)
                    {
                        rit.ItemColumnsData.Add(rcd.ColumnId, new MobileDictionary<int, MobileList<ReportAttribute>>());
                        
                        MobileDictionary<int, MobileList<ReportAttribute>> intern = rit.ItemColumnsData[rcd.ColumnId];
                        foreach (ColumnRow mlcr in rcd.ColumnRows)
                        { //MobileDictionary<ColumnID, MobileDictionary<rowID, list of attr>

                            intern.Add(mlcr.RowId, new MobileList<ReportAttribute>());
                        }
                    }
                }
                //change ItemColumnsData as rows come in
                rit.ItemColumnsData[reader.GetInt32("REPORT_COLUMN_ID")][reader.GetInt32("REPORT_COLUMN_CODE_ID")].Add(ra);
                //add attribute to item, if not already present
                if (!rit.ItemAttributes.Contains(ra)) rit.ItemAttributes.Add(ra);
                //add infobox txt if present
                if (reader["ADDITIONAL_TEXT"] != null && reader.GetString("ADDITIONAL_TEXT").Trim() !="")
                {
                    if (rit.ItemColumnsInfoBox.ContainsKey(reader.GetInt32("REPORT_COLUMN_CODE_ID")))
                    {
                        rit.ItemColumnsInfoBox[reader.GetInt32("REPORT_COLUMN_CODE_ID")].Add(ra, reader.GetString("ADDITIONAL_TEXT"));
                    }
                    else
                    {
                        rit.ItemColumnsInfoBox.Add(reader.GetInt32("REPORT_COLUMN_CODE_ID"), new MobileDictionary<ReportAttribute, string>());
                        rit.ItemColumnsInfoBox[reader.GetInt32("REPORT_COLUMN_CODE_ID")].Add(ra, reader.GetString("ADDITIONAL_TEXT"));
                    }
                }
            }
            if (rit.ItemId != -1) ReportItems.Add(rit);
            reader.NextResult();
            while (reader.Read())
            {//get coded texts, add each line reportData.AddItemText(item_id, columnId, RowID, text)
                AddItemText(reader.GetInt64("ITEM_ID"),
                                reader.GetInt32("REPORT_COLUMN_CODE_ID"),
                                reader.GetInt64("ATTRIBUTE_ID"),
                                reader.GetString("DOCUMENT_TITLE"),
                                reader.GetString("CODED_TEXT"),
                                reader.GetString("ARM_NAME"));
            }
            reader.NextResult();
            while (reader.Read())
            {//get items that don't have any codes for this report
                CurrID = reader.GetInt64("ITEM_ID");
                rit = new ReportItem(CurrID
                                    , reader.GetString("SHORT_TITLE")
                                    , ""
                                    , reader.GetString("OLD_ITEM_ID")
                                    );
                ReportItems.Add(rit);
            }
            //finally, get title and/or abstract and/or year, only if required
            if (ShowAbstract | ShowFullTitle | ShowYear)
            {
                reader.NextResult();
                AddItemsDetails(reader);
            }
        }
#else
#endif        
        public ReportData()
        {

        }


        //public ReportData(string name, int report_id, bool isQuestion, bool isHorizontal, bool showItemId, bool showOldItemId, bool showOutcomes)
        //{
        //    LoadProperty(ReportNameProperty, name);
        //    LoadProperty(ReportIdProperty, report_id);
        //    LoadProperty(IsQuestionProperty, isQuestion);
        //    LoadProperty(IsHorizontalProperty, isHorizontal);
        //    LoadProperty(ShowItemIdProperty, showItemId);
        //    LoadProperty(ShowOldItemIdProperty, showOldItemId);
        //    LoadProperty(ShowOutcomesProperty, showOutcomes);
        //    LoadProperty(ReportColumnsProperty, new MobileList<ReportColumnData>());
        //    LoadProperty(ReportItemsProperty, new MobileList<ReportItem>());
        //    LoadProperty(ReportAttributesProperty, new MobileList<ReportAttribute>());
        //}
        public void AddColumnCode(int ColumnID, ColumnRow CD)
        {
            foreach (ReportColumnData rcd in ReportColumns)
            {
                if (rcd.ColumnId == ColumnID)
                {
                    rcd.ColumnRows.Add(CD);
                    break;
                }
            }
        }
        public void AddItemText(Int64 item_id, int RowID, Int64 attributeID, string Document, string text, string armName)
        {
            ReportAttribute ra = this.findAttributeById(attributeID, armName);
            if (ra == null) return;

            foreach (ReportItem rit in ReportItems)
            {
                if (rit.ItemId == item_id)
                {
                    if (rit.ItemColumnsCodedTxt.ContainsKey(RowID))
                    {
                        MobileDictionary<ReportAttribute, MobileList<ReportCodedText>> di = rit.ItemColumnsCodedTxt[RowID];

                        if (di.ContainsKey(ra))
                        {
                            di[ra].Add(new ReportCodedText(Document, text));
                        }
                        else
                        {
                            MobileList<ReportCodedText> toadd = new MobileList<ReportCodedText>();
                            toadd.Add(new ReportCodedText(Document, text));
                            di.Add(ra, toadd);
                        }
                    }
                    else
                    {
                        MobileDictionary<ReportAttribute, MobileList<ReportCodedText>> di = new MobileDictionary<ReportAttribute, MobileList<ReportCodedText>>();
                        MobileList<ReportCodedText> toadd = new MobileList<ReportCodedText>();
                        toadd.Add(new ReportCodedText(Document, text));
                        di.Add(ra, toadd);
                        rit.ItemColumnsCodedTxt.Add(RowID, di);
                    }
                    break;
                }
            }
        }
#if !SILVERLIGHT
        internal void AddItemsDetails(Csla.Data.SafeDataReader reader)
        {
            while (reader.Read())
            {
                Int64 item_id = reader.GetInt64("ITEM_ID");
                foreach (ReportItem rit in ReportItems)
                {
                    if (rit.ItemId == item_id)
                    {
                        if (ShowAbstract)
                        {
                            rit.SetAbstract(reader.GetString("ABSTRACT"));
                        }
                        if (ShowFullTitle)
                        {
                            rit.SetFullTitle(reader.GetString("TITLE"));
                        }
                        if (ShowYear)
                        {
                            rit.SetYear(reader.GetString("YEAR"));
                        }
                        break;
                    }
                }
            }
        }
#endif
        // name
        private static PropertyInfo<string> ReportNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ReportName", "ReportName"));
        public string ReportName
        {
            get
            {
                return GetProperty(ReportNameProperty);
            }
        }
        // orderBy
        private static PropertyInfo<string> OrderByProperty = RegisterProperty<string>(new PropertyInfo<string>("OrderBy", "OrderBy"));
        public string OrderBy
        {
            get
            {
                return GetProperty(OrderByProperty);
            }
            set
            {
                LoadProperty(OrderByProperty, value);
            }
        }
        // id
        private static PropertyInfo<int> ReportIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReportId", "ReportId"));
        public int ReportId
        {
            get
            {
                return GetProperty(ReportIdProperty);
            }
        }
        // type (q | a)
        private static PropertyInfo<bool> IsQuestionProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsQuestion", "IsQuestion"));
        public bool IsQuestion
        {
            get
            {
                return GetProperty(IsQuestionProperty);
            }
        }
        // alignement (hor | ver)
        private static PropertyInfo<bool> IsHorizontalProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsHorizontal", "IsHorizontal"));
        public bool IsHorizontal
        {
            get
            {
                return GetProperty(IsHorizontalProperty);
            }
            set
            {
                LoadProperty(IsHorizontalProperty, value);
            }
        }
        // Show Item_id
        private static PropertyInfo<bool> ShowItemIdProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowItemId", "ShowItemId"));
        public bool ShowItemId
        {
            get
            {
                return GetProperty(ShowItemIdProperty);
            }
            set 
            { 
                LoadProperty(ShowItemIdProperty, value); 
            }
        }
        // show old item_id
        private static PropertyInfo<bool> ShowOldItemIdProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowOldItemId", "ShowOldItemId"));
        public bool ShowOldItemId
        {
            get
            {
                return GetProperty(ShowOldItemIdProperty);
            }
            set
            {
                LoadProperty(ShowOldItemIdProperty, value);
            }
        }
        // show outcomes
        private static PropertyInfo<bool> ShowOutcomesProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowOutcomes", "ShowOutcomes"));
        public bool ShowOutcomes
        {
            get
            {
                return GetProperty(ShowOutcomesProperty);
            }
        }
        // Show a 'question' report as RiskOfBias Figures
        private static PropertyInfo<bool> ShowRiskOfBiasProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowRiskOfBias", "ShowRiskOfBias"));
        public bool ShowRiskOfBias
        {
            get
            {
                return GetProperty(ShowRiskOfBiasProperty);
            }
            set
            {
                LoadProperty(ShowRiskOfBiasProperty, value);
            }
        }
        private static PropertyInfo<bool> ShowFullTitleProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowFullTitle", "ShowFullTitle"));
        public bool ShowFullTitle
        {
            get { return ReadProperty(ShowFullTitleProperty); }
            set
            {
                LoadProperty(ShowFullTitleProperty, value);
            }
        }
        private static PropertyInfo<bool> ShowAbstractProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowAbstract", "ShowAbstract"));
        public bool ShowAbstract
        {
            get { return ReadProperty(ShowAbstractProperty); }
            set
            {
                LoadProperty(ShowAbstractProperty, value);
            }
        }
        private static PropertyInfo<bool> ShowYearProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowYear", "ShowYear"));
        public bool ShowYear
        {
            get { return ReadProperty(ShowYearProperty); }
            set
            {
                LoadProperty(ShowYearProperty, value);
            }
        }
        private static PropertyInfo<bool> ShowShortTitleProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowShortTitle", "ShowShortTitle"));
        public bool ShowShortTitle
        {
            get { return ReadProperty(ShowShortTitleProperty); }
            set
            {
                LoadProperty(ShowShortTitleProperty, value);
            }
        }
        // List<columns>
        private static PropertyInfo<MobileList<ReportColumnData>> ReportColumnsProperty = RegisterProperty(new PropertyInfo<MobileList<ReportColumnData>>("ReportColumns", "ReportColumns"));
        public MobileList<ReportColumnData> ReportColumns
        {
            get
            {
                return GetProperty(ReportColumnsProperty);
            }
        }
        //List<ReportItem>
        private static PropertyInfo<MobileList<ReportItem>> ReportItemsProperty = RegisterProperty(new PropertyInfo<MobileList<ReportItem>>("ReportItems", "ReportItems"));
        public MobileList<ReportItem> ReportItems
        {
            get
            {
                return GetProperty(ReportItemsProperty);
            }
        }
        private static PropertyInfo<MobileList<ReportAttribute>> ReportAttributesProperty = RegisterProperty(new PropertyInfo<MobileList<ReportAttribute>>("ReportAttributes", "ReportAttributes"));
        public MobileList<ReportAttribute> ReportAttributes
        {
            get
            {
                return GetProperty(ReportAttributesProperty);
            }
        }
        private ReportAttribute findAttributeById(Int64 ID, string ArmName)
        {
            foreach (ReportAttribute ra in ReportAttributes)
            {
                if (ra.AttributeId == ID && ra.ArmName == ArmName) return ra;
            }
            return null;
        }
#if !SILVERLIGHT
        protected void DataPortal_Fetch(ReportDataSelectionCriteria c)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReportData", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ITEM_IDS", c.Items));
                    command.Parameters.Add(new SqlParameter("@REPORT_ID", c.ReportId));
                    command.Parameters.Add(new SqlParameter("@ORDER_BY", c.OrderBy));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", c.AttributeId));
                    command.Parameters.Add(new SqlParameter("@IS_QUESTION", c.IsQuestion));
                    command.Parameters.Add(new SqlParameter("@FULL_DETAILS", c.ShowAbstract || c.ShowFullTitle || c.ShowYear));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            Get(reader.GetString("NAME")
                                , c.OrderBy
                                , reader.GetInt32("REPORT_ID")
                                , reader.GetString("REPORT_TYPE") != "Single"
                                , c.isHorizontal
                                , c.ShowItemId
                                , c.ShowOldItemId
                                , c.ShowOutcomes
                                , c.ShowFullTitle
                                , c.ShowAbstract
                                , c.ShowYear
                                , c.ShowShortTitle
                                , reader);
                        }
                    }
                }
                connection.Close();
            }
        }
#endif
        public static int CompareItemsByShortTitle(ReportItem x, ReportItem y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    return x.ShortTitle.CompareTo(y.ShortTitle);
                }
            }
        }
        public static int CompareItemsByFullTitle(ReportItem x, ReportItem y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    return x.FullTitle.CompareTo(y.FullTitle);
                }
            }
        }
        public static int CompareItemsByItemID(ReportItem x, ReportItem y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    return x.ItemId.CompareTo(y.ItemId);
                }
            }
        }
        public static int CompareItemsByOldItemID(ReportItem x, ReportItem y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    return x.oldItemId.CompareTo(y.oldItemId);
                }
            }
        }
        public static int CompareItemsByYear(ReportItem x, ReportItem y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    return x.Year.CompareTo(y.Year);
                }
            }
        }
        //use the report data to build the report in real time
 #if SILVERLIGHT       
        public string ReportContent(bool isHorizontal, bool showItemID, bool showOldItemID, bool hideEmpty, bool showBullets, string InfoTxt, string orderBy, bool doRiskOfBiasFigure, ReviewSetsList ReviewSets
            ,bool showFullTitle, bool showAbstract, bool showYear, bool showShortTitle)
        {
            this.IsHorizontal = isHorizontal;
            this.OrderBy = orderBy;
            this.ShowItemId = showItemID;
            this.ShowOldItemId = showOldItemID;
            this.ShowRiskOfBias = doRiskOfBiasFigure;
            this.ShowFullTitle = showFullTitle;
            this.ShowAbstract = showAbstract;
            this.ShowYear = showYear;
            this.ShowShortTitle = showShortTitle;
            switch (OrderBy)
            {
                case "Short title":
                    ReportItems.Sort(CompareItemsByShortTitle);
                    break;
                case "Imported Id":
                    ReportItems.Sort(CompareItemsByOldItemID);
                    break;
                case "Item Id":
                    ReportItems.Sort(CompareItemsByItemID);
                    break;
                case "Title":
                    ReportItems.Sort(CompareItemsByFullTitle);
                    break;
                case "Year":
                    ReportItems.Sort(CompareItemsByYear);
                    break;
                default:
                    ReportItems.Sort(CompareItemsByShortTitle);
                    break;
            }
            List<int[]> RiskOfBSummaryL = new List<int[]>();
            
            string res = "<HTML><head><title>" + ReportName 
                + "</title>";
            string commonstyle = @"<style>
                                   
                                    
                                   </style>";
            res += commonstyle +"</head><body>";
            //1. Rep header: title (and columns, if it's horizontal)
            res += "<h1>" + ReportName + "</h1>";

            if (IsHorizontal && !ShowRiskOfBias)
            {
                res += "<table border='1' Style='width:100%' Cellpadding='33'><tr>";
                int smallcolumnscount = (ShowItemId ? 1 : 0) + (ShowOldItemId ? 1 : 0) ;
                int columnscount = 2 * ReportColumns.Count + smallcolumnscount;
                string shortSyle = " Style='width:" + (93/columnscount).ToString() +"%'";
                string longSyle = " Style='width:" + (186 / columnscount).ToString() + "%'";

                if (ShowItemId) res += "<th Style='width:7%'><p style='margin-left:5px; margin-right:5px;'>Item ID</p></th>";
                if (ShowOldItemId) res += "<th" + shortSyle + "><p style='margin-left:5px; margin-right:5px;'>Imported Id</p></th>";
                if (ShowShortTitle) res += "<th" + shortSyle + "><p style='margin-left:5px; margin-right:5px;'>Short Title</p></th>";
                if (ShowFullTitle) res += "<th" + shortSyle + "><p style='margin-left:5px; margin-right:5px;'>Title</p></th>";
                if (ShowYear) res += "<th" + shortSyle + "><p style='margin-left:5px; margin-right:5px;'>Year</p></th>";
                if (ShowAbstract) res += "<th" + shortSyle + "><p style='margin-left:5px; margin-right:5px;'>Abstract</p></th>";
                foreach (ReportColumnData rcd in ReportColumns)
                {
                    res += "<th" + longSyle + "><p style='margin-left:5px; margin-right:5px;'>" + rcd.ColumnName + "</p></th>";
                }
                res += "</tr>";
            }
            else
                if (ShowRiskOfBias)
                {
                    int smallcolumnscount = (ShowItemId ? 1 : 0) + (ShowOldItemId ? 1 : 0);
                    int columnscount = 2 * ReportColumns.Count + smallcolumnscount;
                    res += "<table border='1' Cellpadding='33' Style='width:100%'><tr>";
                    string shortSyle = " Style='width:" + (93 / columnscount).ToString() + "%'";
                    string longSyle = " Style='width:" + (186 / columnscount).ToString() + "%'";

                    if (ShowItemId) res += "<th Style='width:7%'><p style='margin-left:5px; margin-right:5px;'>Item ID</p></th>";
                    if (ShowOldItemId) res += "<th" + shortSyle + ">I<p style='margin-left:5px; margin-right:5px;'>mported Id</p></th>";
                    res += "<th" + shortSyle + "><p style='margin-left:5px; margin-right:5px;'>Short Title</p></th>";
                    foreach (ReportColumnData rcd in ReportColumns)
                    {
                        foreach (ColumnRow o in rcd.ColumnRows)
                        {
                            res += "<th" + longSyle + "><p style='margin-left:5px; margin-right:5px;'>" + o.RowName + "</p></th>";
                            RiskOfBSummaryL.Add(new int[] { 0, 0, 0 });//keep the three overall figures for each question
                        }
                    }
                    res += "</tr>";
                }
                else // it's a 'vertical' report
                {
                    res += "<table border='0' cellpadding='5'  Style='width:100%' valign='top'>";
                    res += Environment.NewLine + "<tr><td Style='width:15%'>&nbsp;</td><td Style='width:85%'>&nbsp;</td></tr>";
                }
            bool HasData = false;
            string ItemRow;
            //2. foreach item, cycle through columns & rows
            foreach (ReportItem rit in ReportItems)
            {
                ItemRow = "";
                HasData = false;
                if (IsHorizontal && !ShowRiskOfBias)
                {
                    ItemRow += "<tr>";
                    if (ShowItemId) ItemRow += "<td><p style='margin-left:5px; margin-right:5px;'>" + rit.ItemId + "</p></td>";
                    if (ShowOldItemId) ItemRow += "<td><p style='margin-left:5px; margin-right:5px;'>" + rit.oldItemId + "</p></td>"; 
                    if (ShowShortTitle) ItemRow += "<td><p style='margin-left:5px; margin-right:5px;'>" + rit.ShortTitle + "</p></td>";
                    if (ShowFullTitle) ItemRow += "<td><p style='margin-left:5px; margin-right:5px;'>" + rit.FullTitle + "</p></td>";
                    if (ShowYear) ItemRow += "<td><p style='margin-left:5px; margin-right:5px;'>" + rit.Year + "</p></td>";
                    if (ShowAbstract) ItemRow += "<td><p style='margin-left:5px; margin-right:5px;'>" + rit.Abstract + "</p></td>";
                    //ItemRow += "<td><p style='margin-left:5px; margin-right:5px;'>" + rit.ShortTitle + "</p></td>";
                    foreach (ReportColumnData rcd in ReportColumns)
                    {
                        ItemRow += "<td>";
                        foreach (ColumnRow Row in rcd.ColumnRows)
                        {
                            if (rit.ItemColumnsData.ContainsKey(rcd.ColumnId) && rit.ItemColumnsData[rcd.ColumnId].ContainsKey(Row.RowId) && rit.ItemColumnsData[rcd.ColumnId][Row.RowId].Count != 0)
                            {//item has data for this column+row
                                HasData = true;
                                ItemRow += "<p style='margin-left:5px; margin-right:5px;'><b>" + Row.RowName + "</b><br>";
                                foreach (ReportAttribute ratt in rit.ItemColumnsData[rcd.ColumnId][Row.RowId])
                                {
                                    //add code name & arm name
                                    if (Row.DisplayCode)
                                    {
                                        if (showBullets) ItemRow += "&bull; " + ratt.AttributeName;
                                        else ItemRow += ratt.AttributeName;
                                        if (ratt.ArmName != "")
                                        {
                                            ItemRow += "<span style='font-family:Times, serif; font-size: 76%;'>[Arm: " + ratt.ArmName + "]</span>";
                                        }
                                        ItemRow += "<br>";
                                    }
                                    else if (ratt.ArmName != "") ItemRow += "<span style='font-family:Times, serif; font-size: 76%;'>[Arm: " + ratt.ArmName + "]</span>";
                                    //add addtn txt
                                    if (Row.DisplayAddtlTxt && rit.ItemColumnsInfoBox.ContainsKey(Row.RowId)
                                            && rit.ItemColumnsInfoBox[Row.RowId].ContainsKey(ratt))
                                    {
                                        
                                        string infoTmp = rit.ItemColumnsInfoBox[Row.RowId][ratt];
                                        //infoTmp = infoTmp.Replace(new string('\ufb00', 1), "ff");
                                        //infoTmp = infoTmp.Replace(new string('\ufb01', 1), "fi");
                                        //infoTmp = infoTmp.Replace(new string('\ufb02', 1), "fl");
                                        //infoTmp = System.Windows.Browser.HttpUtility.HtmlEncode(infoTmp);
                                        //infoTmp = infoTmp.Substring(0, infoTmp.Length - (int)Math.Round(((double)infoTmp.Length / 3.9)));

                                        ItemRow += (InfoTxt.Trim().Length == 0 ? "<i>" : InfoTxt + "&nbsp;<i>") + infoTmp + "</i><br>";
                                    }
                                    //add coded txt
                                    if (Row.DisplayCodedTxt && rit.ItemColumnsCodedTxt.ContainsKey(Row.RowId)
                                            && rit.ItemColumnsCodedTxt[Row.RowId].ContainsKey(ratt))
                                    {
                                        foreach (ReportCodedText rct in rit.ItemColumnsCodedTxt[Row.RowId][ratt])
                                        {
                                            if (rct.CodedText.Contains("[¬s]"))
                                            {
                                                ItemRow += "[" + rct.DocumentName + "] " + rct.CodedText.Replace("[¬s]\"", "<i>").Replace("[¬e]\"", "</i>") + "<br>";
                                            } else 
                                            {
                                                ItemRow += "[" + rct.DocumentName + "] <span style='font-family:Courier New; font-size:12px;'>" 
                                                    + rct.CodedText + "</span><br>";
                                            }
                                        }
                                    }
                                    
                                } 
                                ItemRow += "</p>";
                                    
                            }
                        }
                        ItemRow += "</td>";   
                    }
                    ItemRow += "</tr>";
                }
                else if (ShowRiskOfBias)
                {
                    ItemRow += "<tr>";
                    if (ShowItemId) ItemRow += "<td><p style='margin-left:5px; margin-right:5px;'>" + rit.ItemId + "</p></td>";
                    if (ShowOldItemId) ItemRow += "<td><p style='margin-left:5px; margin-right:5px;'>" + rit.oldItemId + "</p></td>";
                    ItemRow += "<td><p style='margin-left:5px; margin-right:5px;'>" + rit.ShortTitle + "</p></td>";
                    int ColCount = 0;//keep track of the current column to update the correct element in RiskOfBSummaryL                    
                    foreach (ReportColumnData rcd in ReportColumns)
                    {
                        
                        foreach (ColumnRow Row in rcd.ColumnRows)
                        {   
                            bool AlreadyNotKnown = false;
                            if (rit.ItemColumnsData.ContainsKey(rcd.ColumnId) && rit.ItemColumnsData[rcd.ColumnId].ContainsKey(Row.RowId) && rit.ItemColumnsData[rcd.ColumnId][Row.RowId].Count != 0)
                            {//item has data for this column+row, we need to exclude data from arms, so one more check is needed.
                                ReportAttribute ratt = null;
                                foreach (ReportAttribute tRatt in rit.ItemColumnsData[rcd.ColumnId][Row.RowId])
                                {
                                    if (tRatt.ArmName == "")
                                    {//we can/should use this
                                        HasData = true;
                                        ratt = tRatt;
                                        break;
                                    }
                                }
                                if (ratt != null)
                                {
                                    AttributeSet atSet = ReviewSets.GetAttributeSetFromAttributeId(ratt.AttributeId);
                                    if (atSet.AttributeOrder == 0)
                                    {
                                        RiskOfBSummaryL[ColCount][0]++;//adds 1 to the count of items without risk for this question
                                        ItemRow += @"<td align=""center"" style=""background-color:rgb(111,235,121)""><font size=14>" + /*('\u263a').ToString()*/ "+" + "</font></td>";
                                    }
                                    else if (atSet.AttributeOrder == 1)
                                    {
                                        RiskOfBSummaryL[ColCount][1]++;
                                        ItemRow += @"<td align=""center"" style=""background-color:rgb(255,75,75)""><font size=14>" + /*('\u2639').ToString()*/ "-" + "</font></td>";
                                    }
                                    else
                                    {
                                        ItemRow += @"<td align=""center"" style=""background-color:rgb(254,250,98)""><font size=14>?</font></td>";
                                        if (!AlreadyNotKnown) RiskOfBSummaryL[ColCount][2]++;
                                        AlreadyNotKnown = true;
                                    }
                                }
                                else
                                {//we only had Arms data, nothing applied to the whole study, so this is an empty cell.
                                    ItemRow += @"<td align=""center"" style=""background-color:grey""><font size=14>N/A</font></td>";
                                }
                            }
                            else
                            {//fill the empty cell
                                ItemRow += @"<td align=""center"" style=""background-color:grey""><font size=14>N/A</font></td>";
                            }
                            ColCount++;
                        }
                        
                    }
                    ItemRow += "</tr>";
                }
                else // it's a 'vertical' report
                {
                    bool isHead = true;
                    if (ShowShortTitle)
                    {
                        ItemRow += "<tr><td valign='top'><h2>Item</h2></td><td valign='top'><h2>" + rit.ShortTitle + "</h2></td></tr>";
                        isHead = false;
                    }
                    if (ShowFullTitle)
                    {
                        if (isHead)
                        {
                            ItemRow += "<tr><td valign='top'><h2>Title</h2></td><td valign='top'><h2>" + rit.FullTitle + "</h2></td></tr>";
                            isHead = false;
                        }
                        else ItemRow += "<tr><td valign='top'>Title</td><td valign='top'>" + rit.FullTitle + "</td></tr>";
                    }
                    if (ShowItemId)
                    {
                        if (isHead)
                        {
                            ItemRow += "<tr><td valign='top'><h2>Item Id</h2></td><td valign='top'><h2>" + rit.ItemId + "</h2></td></tr>";
                            isHead = false;
                        }
                        else ItemRow += "<tr><td valign='top'>Item Id</td><td valign='top'>" + rit.ItemId + "</td></tr>";
                    }
                    if (showOldItemID)
                    {
                        if (isHead)
                        {
                            ItemRow += "<tr><td valign='top'><h2>Imported Id</h2></td><td valign='top'><h2>" + rit.oldItemId + "</h2></td></tr>";
                            isHead = false;
                        }
                        else ItemRow += "<tr><td valign='top'>Imported Id</td><td valign='top'>" + rit.oldItemId + "</td></tr>";
                    }
                    if (ShowYear)
                    {
                        ItemRow += "<tr><td valign='top'>Year</td><td valign='top'>" + rit.Year + "</td></tr>";
                    }
                    if (ShowAbstract)
                    {
                        ItemRow += "<tr><td valign='top'>Abstract</td><td valign='top'>" + rit.Abstract + "</td></tr>";
                    }
                    foreach (ReportColumnData rcd in ReportColumns)
                    {
                        bool AddName = true;
                        foreach (ColumnRow Row in rcd.ColumnRows)
                        {
                            if (rit.ItemColumnsData.ContainsKey(rcd.ColumnId) && rit.ItemColumnsData[rcd.ColumnId].ContainsKey(Row.RowId) && rit.ItemColumnsData[rcd.ColumnId][Row.RowId].Count != 0)
                            {//item has data for this column+row
                                HasData = true;
                                if (AddName)
                                {
                                    ItemRow += "<tr><td valign='top'>" + rcd.ColumnName + "</td><td>";
                                    AddName = false;
                                }
                                ItemRow += "<b>" + Row.RowName + "</b><br>";
                                foreach (ReportAttribute ratt in rit.ItemColumnsData[rcd.ColumnId][Row.RowId])
                                {
                                    //add code name
                                    if (Row.DisplayCode)
                                    {
                                        if (showBullets) ItemRow += "&bull; " + ratt.AttributeName + "<br>";
                                        else ItemRow += ratt.AttributeName + "<br>";
                                    }
                                    //add addtn txt
                                    if (Row.DisplayAddtlTxt && rit.ItemColumnsInfoBox.ContainsKey(Row.RowId)
                                            && rit.ItemColumnsInfoBox[Row.RowId].ContainsKey(ratt))
                                    {
                                        ItemRow += (InfoTxt.Trim().Length == 0 ? "<i>" : InfoTxt + "&nbsp;<i>") + rit.ItemColumnsInfoBox[Row.RowId][ratt] + "</i><br>";
                                    }
                                    //add coded txt
                                    if (Row.DisplayCodedTxt && rit.ItemColumnsCodedTxt.ContainsKey(Row.RowId)
                                            && rit.ItemColumnsCodedTxt[Row.RowId].ContainsKey(ratt))
                                    {
                                        foreach (ReportCodedText rct in rit.ItemColumnsCodedTxt[Row.RowId][ratt])
                                        {
                                            if (rct.CodedText.Contains("[¬s]"))
                                            {
                                                ItemRow += "[" + rct.DocumentName + "] " + rct.CodedText.Replace("[¬s]\"", "<i>").Replace("[¬e]\"", "</i>") + "<br>";
                                            }
                                            else
                                            {
                                                ItemRow += "[" + rct.DocumentName + "] <span style='font-family:Courier New; font-size:12px;'>"
                                                    + rct.CodedText + "</span><br>";
                                            }
                                        }
                                    }
                                }
                                //res += "</p>";
                            }
                        }
                        string cic = ItemRow.Substring(ItemRow.Length - 4);
                        if (ItemRow.Substring(ItemRow.Length - 4) == "<br>") ItemRow = ItemRow.Substring(0, ItemRow.Length - 4);
                        if (!AddName) ItemRow += "</td></tr>";
                    }
                    ItemRow += "<tr><td Style='border-top: 1px solid black'>&nbsp;</td><td>&nbsp;</td></tr>";
                }
                if (HasData || !hideEmpty) res += ItemRow;
            }
            if (ShowRiskOfBias)
            {//second RoB table, summaries for each category
                //first check if there is some data to display
                //int check = 0;
                //foreach (int[] sin in RiskOfBSummaryL)
                //{
                //    check = check + sin[0] + sin[1] + sin[2];
                //}
                //if (check == 0)
                //{//nothing here!!
                //    res += "</table></body></html>";
                //}
                //else
                //{
                    res += "</table><table Style='width:100%; border-width: 1px;' >";
                    int ColCount = 0;//keep track of the current column to update the correct element in RiskOfBSummaryL                    
                    foreach (ReportColumnData rcd in ReportColumns)
                    {
                        foreach (ColumnRow row in rcd.ColumnRows)
                        {
                            //res += "<tr><td>";
                            res += "<tr Style='height:35px; min-height:35px; max-height:35px;'><td Style='width:20%;' valign=middle align=right>" + row.RowName + "&nbsp;&nbsp;</td><td>";
                            int tot = RiskOfBSummaryL[ColCount][0] + RiskOfBSummaryL[ColCount][1] + RiskOfBSummaryL[ColCount][2];
                            double Good = Math.Round((100 * (double)RiskOfBSummaryL[ColCount][0] / tot), 0);
                            double Bad = Math.Round((100 * (double)RiskOfBSummaryL[ColCount][1] / tot), 0);
                            double Bah = 100 - Good - Bad;
                            //res += "<table style='margin:4px; borderwidth:0px; width:70%;'><tr><td Style='width:20%;'>" + rcd.ColumnName + "</td><td style='font-size: 1px;'>";
                            res += "<table style='margin:4px; borderwidth:0px; height:35px; min-height:35px; max-height:35px; width:100%'><tr>";
                            res += "<td style='width:" + (tot !=0 ?  Good.ToString() : "33%") + "%; background-color:rgb(111,235,121)' valign=middle align=center><b>" + (Good > 0 ? Good.ToString() + "%</b></td>" : "</b>&nbsp;</td>");
                            res += "<td style='width:" + (tot != 0 ? Bah.ToString() : "33%") + "%; background-color:rgb(254,250,98)' valign=middle align=center><b>" + (Bah > 0 ? Bah.ToString() + "%</b></td>" : "</b>&nbsp;</td>");
                            res += "<td style='width:" + (tot != 0 ? Bad.ToString() : "33%") + "%; background-color:rgb(255,75,75)' valign=middle align=center><b>" + (Bad > 0 ? Bad.ToString() + "%</b></td>" : "</b>&nbsp;</td>");
                            res += "</tr></table>";
                            ColCount++;
                        }
                    }
                    res += "</td></tr><tr><td colspan='2'>";
                    res += "<table width='100%' cellpadding=10><tr><td>&nbsp</td><td align=right style='width:165px;'> Low risk of bias: &nbsp;</td><td style='margin:12px; background-color:rgb(111,235,121); width:50px;'>&nbsp;</td>";
                    res += "<td align=right style='width:165px;'> Unclear risk of bias: &nbsp;</td><td style='margin:12px; background-color:rgb(254,250,98); width:50px;'>&nbsp;</td>";
                    res += "<td align=right style='width:165px;'> High risk of bias: &nbsp;</td><td style='margin:12px; background-color:rgb(255,75,75); width:50px;'>&nbsp;</td><td>&nbsp</td>";
                    res += "</tr></table>";
                    res += "</td></tr></table></body></html>";
                //}
            }
            else
            {
                res += "</table></body></html>";
            }
           return res;
        }
#endif
    }
    [Serializable]
    public class ReportColumnData : ReadOnlyBase<ReportColumnData>
    {
        public ReportColumnData() { }
        public ReportColumnData(int ColumnId, string ColumnName, int ColumnOrder)
        {
            LoadProperty(ColumnIdProperty, ColumnId);
            LoadProperty(ColumnNameProperty, ColumnName);
            LoadProperty(ColumnOrderProperty, ColumnOrder);
            LoadProperty(ColumnRowsProperty, new MobileList<ColumnRow>());
        }
        //col_id
        private static PropertyInfo<int> ColumnIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ColumnId", "ColumnId"));
        public int ColumnId
        {
            get
            {
                return GetProperty(ColumnIdProperty);
            }
        }
        //col_name
        private static PropertyInfo<string> ColumnNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ColumnName", "ColumnName"));
        public string ColumnName
        {
            get
            {
                return GetProperty(ColumnNameProperty);
            }
        }
        //col order (sort by!)
        private static PropertyInfo<int> ColumnOrderProperty = RegisterProperty<int>(new PropertyInfo<int>("ColumnOrder", "ColumnOrder"));
        public int ColumnOrder
        {
            get
            {
                return GetProperty(ColumnOrderProperty);
            }
        }
        //List<ColumnRow>
        private static PropertyInfo<MobileList<ColumnRow>> ColumnRowsProperty = RegisterProperty<MobileList<ColumnRow>>(new PropertyInfo<MobileList<ColumnRow>>("ColumnRows", "ColumnRows"));
        public MobileList<ColumnRow> ColumnRows
        {
            get
            {
                return GetProperty(ColumnRowsProperty);
            }
        }
    }
    [Serializable]
    public class ColumnRow : ReadOnlyBase<ColumnRow>
    {
        public ColumnRow() { }
        public ColumnRow(int RowId, string RowName, bool DisplayCode, bool DisplayAddtlTxt, bool DisplayCodedTxt)
        {
            LoadProperty(RowIdProperty, RowId);
            LoadProperty(RowNameProperty, RowName);
            LoadProperty(DisplayCodeProperty, DisplayCode);
            LoadProperty(DisplayAddtlTxtProperty, DisplayAddtlTxt);
            LoadProperty(DisplayCodedTxtProperty, DisplayCodedTxt);
        }
        private static PropertyInfo<int> RowIdProperty = RegisterProperty<int>(new PropertyInfo<int>("RowId", "RowId"));
        public int RowId
        {
            get
            {
                return GetProperty(RowIdProperty);
            }
        }
        //Row title (name AKA Column Name) USER_DEF_TEXT
        private static PropertyInfo<string> RowNameProperty = RegisterProperty<string>(new PropertyInfo<string>("RowName", "RowName"));
        public string RowName
        {
            get
            {
                return GetProperty(RowNameProperty);
            }
        }
        //bool display code
        private static PropertyInfo<bool> DisplayCodeProperty = RegisterProperty<bool>(new PropertyInfo<bool>("DisplayCode", "DisplayCode"));
        public bool DisplayCode
        {
            get
            {
                return GetProperty(DisplayCodeProperty);
            }
        }
        //bool display add txt
        private static PropertyInfo<bool> DisplayAddtlTxtProperty = RegisterProperty<bool>(new PropertyInfo<bool>("DisplayAddtlTxt", "DisplayAddtlTxt"));
        public bool DisplayAddtlTxt
        {
            get
            {
                return GetProperty(DisplayAddtlTxtProperty);
            }
        }
        //bool display coded txt
        private static PropertyInfo<bool> DisplayCodedTxtProperty = RegisterProperty<bool>(new PropertyInfo<bool>("DisplayCodedTxt", "DisplayCodedTxt"));
        public bool DisplayCodedTxt
        {
            get
            {
                return GetProperty(DisplayCodedTxtProperty);
            }
        }
    }
    [Serializable]
    public class ReportItem : ReadOnlyBase<ReportItem>
    {
        public ReportItem() { }
        public ReportItem(Int64 ItemId, string ShortTitle, string RowName, string oldItemId)
        {
            LoadProperty(ItemIdProperty, ItemId);
            LoadProperty(ShortTitleProperty, ShortTitle);
            LoadProperty(RowNameProperty, RowName);
            LoadProperty(oldItemIdProperty, oldItemId);
            LoadProperty(ItemAttributesProperty, new MobileList<ReportAttribute>());
            LoadProperty(ItemColumnsDataProperty, new MobileDictionary<int, MobileDictionary<int, MobileList<ReportAttribute>>>());
            LoadProperty(ItemColumnsInfoBoxProperty, new MobileDictionary<int, MobileDictionary<ReportAttribute, string>>());
            LoadProperty(ItemColumnsCodedTxtProperty, new MobileDictionary<int, MobileDictionary<ReportAttribute, MobileList<ReportCodedText>>>());
        }
        //item_ID
        private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
        }
        //ColumnDataRow_name
        private static PropertyInfo<string> ShortTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ShortTitle", "ShortTitle"));
        public string ShortTitle
        {
            get
            {
                return GetProperty(ShortTitleProperty);
            }
        }
        private static PropertyInfo<string> FullTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("FullTitle", "FullTitle"));
        public string FullTitle
        {
            get
            {
                return GetProperty(FullTitleProperty);
            }
        }
        internal void SetFullTitle(string fullTitle)//necessary to keep properties encapsulated and read-only!
        {
            LoadProperty(FullTitleProperty, fullTitle);
        }
        private static PropertyInfo<string> AbstractProperty = RegisterProperty<string>(new PropertyInfo<string>("Abstract", "Abstract"));
        public string Abstract
        {
            get
            {
                return GetProperty(AbstractProperty);
            }
        }
        internal void SetAbstract(string abstractStr)
        {
            LoadProperty(AbstractProperty, abstractStr);
        }
        private static PropertyInfo<string> YearProperty = RegisterProperty<string>(new PropertyInfo<string>("Year", "Year"));
        public string Year
        {
            get
            {
                return GetProperty(YearProperty);
            }
        }
        internal void SetYear(string year)
        {
            LoadProperty(YearProperty, year);
        }
        //ColumnDataRow_name
        private static PropertyInfo<string> RowNameProperty = RegisterProperty<string>(new PropertyInfo<string>("RowName", "RowName"));
        public string RowName
        {
            get
            {
                return GetProperty(RowNameProperty);
            }
        }

        //old item_ID
        private static PropertyInfo<string> oldItemIdProperty = RegisterProperty<string>(new PropertyInfo<string>("oldItemId", "oldItemId"));
        public string oldItemId
        {
            get
            {
                return GetProperty(oldItemIdProperty);
            }
        }
        //all attributes for this item/report
        private static PropertyInfo<MobileList<ReportAttribute>> ItemAttributesProperty = RegisterProperty(new PropertyInfo<MobileList<ReportAttribute>>("ItemAttributes", "ItemAttributes"));
        public MobileList<ReportAttribute> ItemAttributes
        {
            get
            {
                return GetProperty(ItemAttributesProperty);
            }
        }
        //AllColumns, all rows, with true or false (whether the item is associated with the code represented by the row)
        //MobileDictionary<ColumnID, MobileDictionary<rowID, attributeID (0 if not present)>
        private static PropertyInfo<MobileDictionary<int, MobileDictionary<int, MobileList<ReportAttribute>>>> ItemColumnsDataProperty = RegisterProperty(new PropertyInfo<MobileDictionary<int, MobileDictionary<int, MobileList<ReportAttribute>>>>("ItemColumnsData", "ItemColumnsData"));
        public MobileDictionary<int, MobileDictionary<int, MobileList<ReportAttribute>>> ItemColumnsData
        {
            get
            {
                return GetProperty(ItemColumnsDataProperty);
            }
        }
        //NOT (!!) AllColumns, all rows, with string (text in the "info" box)
        //MobileDictionary<rowID, MobileDictionary<AttributeID, bool ("info" box)>
        private static PropertyInfo<MobileDictionary<int, MobileDictionary<ReportAttribute, string>>> ItemColumnsInfoBoxProperty = RegisterProperty(new PropertyInfo<MobileDictionary<int, MobileDictionary<ReportAttribute, string>>>("ItemColumnsInfoBox", "ItemColumnsInfoBox"));
        public MobileDictionary<int, MobileDictionary<ReportAttribute, string>> ItemColumnsInfoBox
        {
            get
            {
                return GetProperty(ItemColumnsInfoBoxProperty);
            }
        }
        //NOT (!!) AllColumns, all rows, with List<string> (coded text)
        //MobileDictionary<rowID, MobileDictionary<AttributeID, ReportCodedText>
        private static PropertyInfo<MobileDictionary<int, MobileDictionary<ReportAttribute, MobileList<ReportCodedText>>>> ItemColumnsCodedTxtProperty = RegisterProperty(new PropertyInfo<MobileDictionary<int, MobileDictionary<ReportAttribute, MobileList<ReportCodedText>>>>("ItemColumnsCodedTxt", "ItemColumnsCodedTxt"));
        public MobileDictionary<int, MobileDictionary<ReportAttribute, MobileList<ReportCodedText>>> ItemColumnsCodedTxt
        {
            get
            {
                return GetProperty(ItemColumnsCodedTxtProperty);
            }
        }
    }
    [Serializable]
    public class ReportAttribute : ReadOnlyBase<ReportAttribute>, IEquatable<ReportAttribute>
    {
        public ReportAttribute() { }
        public ReportAttribute(Int64 attributeId, string attributeName, string armName)
        {
            LoadProperty(AttributeIdProperty, attributeId);
            LoadProperty(AttributeNameProperty, attributeName);
            LoadProperty(ArmNameProperty, armName);
        }
        private static PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
        }
        //ARMS! this becomes part of the identity for this row. A code can be applied to the same item, if it's applied to different arms.
        private static PropertyInfo<string> ArmNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ArmName", "ArmName"));
        public string ArmName
        {
            get
            {
                return GetProperty(ArmNameProperty);
            }
        }
        //ColumnDataRow_name
        private static PropertyInfo<string> AttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeName", "AttributeName"));
        public string AttributeName
        {
            get
            {
                return GetProperty(AttributeNameProperty);
            }
        }
        public bool Equals(ReportAttribute other)
        {
            if (other == null)
                return false;

            if (this.AttributeId == other.AttributeId && this.ArmName == other.ArmName)
                return true;
            else
                return false;
        }
    }
    [Serializable]
    public class ReportCodedText : ReadOnlyBase<ReportCodedText>
    {
        public ReportCodedText() { }
        public ReportCodedText(string documentName, string codedText)
        {
            LoadProperty(DocumentNameProperty, documentName);
            LoadProperty(CodedTextProperty, codedText);
        }
        private static PropertyInfo<string> DocumentNameProperty = RegisterProperty<string>(new PropertyInfo<string>("DocumentName", "DocumentName"));
        public string DocumentName
        {
            get
            {
                return GetProperty(DocumentNameProperty);
            }
        }

        private static PropertyInfo<string> CodedTextProperty = RegisterProperty<string>(new PropertyInfo<string>("CodedText", "CodedText"));
        public string CodedText
        {
            get
            {
                return GetProperty(CodedTextProperty);
            }
        }
    }
    [Serializable]
    public class ReportDataSelectionCriteria : BusinessBase //Csla.CriteriaBase
    {
        private static PropertyInfo<bool> IsQuestionProperty = RegisterProperty<bool>(typeof(ReportDataSelectionCriteria), new PropertyInfo<bool>("IsQuestion", "IsQuestion"));
        public bool IsQuestion
        {
            get { return ReadProperty(IsQuestionProperty); }
            set
            {
                SetProperty(IsQuestionProperty, value);
            }
        }
        private static PropertyInfo<string> ItemsProperty = RegisterProperty<string>(typeof(ReportDataSelectionCriteria), new PropertyInfo<string>("Items", "Items"));
        public string Items
        {
            get { return ReadProperty(ItemsProperty); }
            set
            {
                SetProperty(ItemsProperty, value);
            }
        }
        private static PropertyInfo<int> ReportIdProperty = RegisterProperty<int>(typeof(ReportDataSelectionCriteria), new PropertyInfo<int>("ReportId", "ReportId"));
        public int ReportId
        {
            get { return ReadProperty(ReportIdProperty); }
            set
            {
                SetProperty(ReportIdProperty, value);
            }
        }
        private static PropertyInfo<string> OrderByProperty = RegisterProperty<string>(typeof(ReportDataSelectionCriteria), new PropertyInfo<string>("OrderBy", "OrderBy"));
        public string OrderBy
        {
            get { return ReadProperty(OrderByProperty); }
            set
            {
                SetProperty(OrderByProperty, value);
            }
        }
        private static PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(typeof(ReportDataSelectionCriteria), new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get { return ReadProperty(AttributeIdProperty); }
            set
            {
                SetProperty(AttributeIdProperty, value);
            }
        }
        private static PropertyInfo<int> SetIdProperty = RegisterProperty<int>(typeof(ReportDataSelectionCriteria), new PropertyInfo<int>("SetId", "SetId"));
        public int SetId
        {
            get { return ReadProperty(SetIdProperty); }
            set
            {
                SetProperty(SetIdProperty, value);
            }
        }
        
        private static PropertyInfo<bool> isHorizontalProperty = RegisterProperty<bool>(typeof(ReportDataSelectionCriteria), new PropertyInfo<bool>("isHorizontal", "isHorizontal"));
        public bool isHorizontal
        {
            get { return ReadProperty(isHorizontalProperty); }
            set
            {
                SetProperty(isHorizontalProperty, value);
            }
        }
        private static PropertyInfo<bool> ShowItemIdProperty = RegisterProperty<bool>(typeof(ReportDataSelectionCriteria), new PropertyInfo<bool>("ShowItemId", "ShowItemId"));
        public bool ShowItemId
        {
            get { return ReadProperty(ShowItemIdProperty); }
            set
            {
                SetProperty(ShowItemIdProperty, value);
            }
        }
        private static PropertyInfo<bool> ShowOldItemIdProperty = RegisterProperty<bool>(typeof(ReportDataSelectionCriteria), new PropertyInfo<bool>("ShowOldItemId", "ShowOldItemId"));
        public bool ShowOldItemId
        {
            get { return ReadProperty(ShowOldItemIdProperty); }
            set
            {
                SetProperty(ShowOldItemIdProperty, value);
            }
        }
        private static PropertyInfo<bool> ShowOutcomesProperty = RegisterProperty<bool>(typeof(ReportDataSelectionCriteria), new PropertyInfo<bool>("ShowOutcomes", "ShowOutcomes"));
        public bool ShowOutcomes
        {
            get { return ReadProperty(ShowOutcomesProperty); }
            set
            {
                SetProperty(ShowOutcomesProperty, value);
            }
        }
        private static PropertyInfo<bool> ShowFullTitleProperty = RegisterProperty<bool>(typeof(ReportDataSelectionCriteria), new PropertyInfo<bool>("ShowFullTitle", "ShowFullTitle"));
        public bool ShowFullTitle
        {
            get { return ReadProperty(ShowFullTitleProperty); }
            set
            {
                SetProperty(ShowFullTitleProperty, value);
            }
        }
        private static PropertyInfo<bool> ShowAbstractProperty = RegisterProperty<bool>(typeof(ReportDataSelectionCriteria), new PropertyInfo<bool>("ShowAbstract", "ShowAbstract"));
        public bool ShowAbstract
        {
            get { return ReadProperty(ShowAbstractProperty); }
            set
            {
                SetProperty(ShowAbstractProperty, value);
            }
        }
        private static PropertyInfo<bool> ShowYearProperty = RegisterProperty<bool>(typeof(ReportDataSelectionCriteria), new PropertyInfo<bool>("ShowYear", "ShowYear"));
        public bool ShowYear
        {
            get { return ReadProperty(ShowYearProperty); }
            set
            {
                SetProperty(ShowYearProperty, value);
            }
        }
        private static PropertyInfo<bool> ShowShortTitleProperty = RegisterProperty<bool>(typeof(ReportDataSelectionCriteria), new PropertyInfo<bool>("ShowShortTitle", "ShowShortTitle"));
        public bool ShowShortTitle
        {
            get { return ReadProperty(ShowShortTitleProperty); }
            set
            {
                SetProperty(ShowShortTitleProperty, value);
            }
        }
        public ReportDataSelectionCriteria()
        { }
        public ReportDataSelectionCriteria( bool isQuestion
                                            , string items
                                            , int reportId
                                            , string orderBy
                                            , Int64 attributeId
                                            , int setId
                                            , bool IsHoriz, bool showItemId, bool showOldID, bool showOutcomes
                                            , bool showFullTitle, bool showAbstract, bool showYear, bool showShortTitle
                                            )
        {
            this.IsQuestion = isQuestion;
            this.Items = items;
            this.ReportId = reportId;
            this.OrderBy = orderBy;
            this.AttributeId = attributeId;
            this.SetId = setId;
            this.isHorizontal = IsHoriz;
            this.ShowItemId = showItemId;
            this.ShowOldItemId = showOldID;
            this.ShowOutcomes = showOutcomes;
            this.ShowFullTitle = showFullTitle;
            this.ShowAbstract = showAbstract;
            this.ShowYear = showYear;
            this.ShowShortTitle = showShortTitle;
        }

    }
}
