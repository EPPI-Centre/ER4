using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLibrary.BusinessClasses.Data
{
    public class ImportItemsDataset
    {
        public DataTable TB_ITEM;
        public DataTable TB_ITEM_SOURCE;
        public DataTable TB_SOURCE;
        public DataTable TB_ITEM_AUTHOR;
        public DataTable TB_ITEM_REVIEW;
        public DataTable TB_ITEM_MAG_MATCH;
        public DataTable TB_ZOTERO_ITEM_REVIEW;
        public ImportItemsDataset()
        {
            makeTB_Item();
            makeTB_Item_Author();
            makeTB_Source();
            makeTB_Item_Source();
            makeTB_Item_Review();
            makeTB_Item_Mag_Match();
            makeTB_Zotero_Item_Review();
        }
        private void makeTB_Item()
        {
            this.TB_ITEM = new DataTable("TB_ITEM");
            DataColumn tCol = new DataColumn("ITEM_ID", typeof(long));
            tCol.AutoIncrement = true;
            tCol.AutoIncrementSeed = -1;
            tCol.AutoIncrementStep = 1;
            tCol.AllowDBNull = false;
            tCol.ReadOnly = true;
            tCol.Unique = true;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("TYPE_ID", typeof(byte));
            tCol.AllowDBNull = false;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("TITLE", typeof(string));
            tCol.MaxLength = 4000;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("PARENT_TITLE", typeof(string));
            tCol.MaxLength = 4000;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("SHORT_TITLE", typeof(string));
            tCol.MaxLength = 70;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("DATE_CREATED", typeof(DateTime));
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("CREATED_BY", typeof(string));
            tCol.MaxLength = 50;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("DATE_EDITED", typeof(DateTime));
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("EDITED_BY", typeof(string));
            tCol.MaxLength = 50;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("YEAR", typeof(string));
            tCol.MaxLength = 4;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("MONTH", typeof(string));
            tCol.MaxLength = 10;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("STANDARD_NUMBER", typeof(string));
            tCol.MaxLength = 255;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("CITY", typeof(string));
            tCol.MaxLength = 100;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("COUNTRY", typeof(string));
            tCol.MaxLength = 100;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("PUBLISHER", typeof(string));
            tCol.MaxLength = 1000;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("INSTITUTION", typeof(string));
            tCol.MaxLength = 1000;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("VOLUME", typeof(string));
            tCol.MaxLength = 56;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("PAGES", typeof(string));
            tCol.MaxLength = 50;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("EDITION", typeof(string));
            tCol.MaxLength = 200;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("ISSUE", typeof(string));
            tCol.MaxLength = 100;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("IS_LOCAL", typeof(bool));
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("AVAILABILITY", typeof(string));
            tCol.MaxLength = 255;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("URL", typeof(string));
            tCol.MaxLength = 500;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("OLD_ITEM_ID", typeof(string));
            tCol.MaxLength = 50;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("ABSTRACT", typeof(string));
            tCol.MaxLength = 2147483647;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("COMMENTS", typeof(string));
            tCol.MaxLength = 2147483647;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("DOI", typeof(string));
            tCol.MaxLength = 500;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("KEYWORDS", typeof(string));
            tCol.MaxLength = 2147483647;
            TB_ITEM.Columns.Add(tCol);
            tCol = new DataColumn("SearchText", typeof(string));
            tCol.MaxLength = 500;
            TB_ITEM.Columns.Add(tCol);

        }
        private void makeTB_Item_Source() {
            this.TB_ITEM_SOURCE = new DataTable("TB_ITEM_SOURCE");
            DataColumn tCol = new DataColumn("ITEM_SOURCE_ID", typeof(long));
            tCol.Unique = true;
            tCol.AllowDBNull = false;
            TB_ITEM_SOURCE.Columns.Add(tCol);
            //TB_ITEM_SOURCE.Constraints.Add(new UniqueConstraint("itemSourceUnique", new DataColumn[] {tCol}, true));
            tCol = new DataColumn("ITEM_ID", typeof(long));
            TB_ITEM_SOURCE.Columns.Add(tCol);
            tCol = new DataColumn("SOURCE_ID", typeof(int));
            TB_ITEM_SOURCE.Columns.Add(tCol);
        }
        private void makeTB_Source()
        {
            this.TB_SOURCE = new DataTable("TB_SOURCE");
            DataColumn tCol = new DataColumn("SOURCE_ID", typeof(int));
            tCol.AutoIncrement = true;
            tCol.AutoIncrementSeed = -1;
            tCol.AutoIncrementStep = -1;
            tCol.AllowDBNull = false;
            tCol.ReadOnly = true;
            tCol.Unique = true;
            TB_SOURCE.Columns.Add(tCol);
            //this.TB_SOURCE.Constraints.Add(new UniqueConstraint("SourceUnique", new DataColumn[] {tCol}, true));
            tCol = new DataColumn("SOURCE_NAME", typeof(string));
            tCol.MaxLength = 255;
            TB_SOURCE.Columns.Add(tCol);
            tCol = new DataColumn("REVIEW_ID", typeof(int));
            TB_SOURCE.Columns.Add(tCol);
            tCol = new DataColumn("DATE_OF_SEARCH", typeof(DateTime));
            TB_SOURCE.Columns.Add(tCol);
            tCol = new DataColumn("DATE_OF_IMPORT", typeof(DateTime));
            TB_SOURCE.Columns.Add(tCol);
            tCol = new DataColumn("SOURCE_DATABASE", typeof(string));
            tCol.DefaultValue = ((string)("\'\'"));
            TB_SOURCE.Columns.Add(tCol);
            tCol = new DataColumn("SEARCH_DESCRIPTION", typeof(string));
            tCol.DefaultValue = ((string)("\'\'"));
            TB_SOURCE.Columns.Add(tCol);
            tCol = new DataColumn("SEARCH_STRING", typeof(string));
            tCol.DefaultValue = ((string)("\'\'"));
            TB_SOURCE.Columns.Add(tCol);
            tCol = new DataColumn("NOTES", typeof(string));
            tCol.DefaultValue = ((string)("\'\'"));
            TB_SOURCE.Columns.Add(tCol);
            tCol = new DataColumn("IMPORT_FILTER_ID", typeof(int));
            TB_SOURCE.Columns.Add(tCol);
        }
        private void makeTB_Item_Author()
        {
            this.TB_ITEM_AUTHOR = new DataTable("TB_ITEM_AUTHOR");
            DataColumn tCol = new DataColumn("ITEM_AUTHOR_ID", typeof(long));
            tCol.AutoIncrement = true;
            tCol.AutoIncrementSeed = -1;
            tCol.AutoIncrementStep = -1;
            tCol.AllowDBNull = false;
            tCol.ReadOnly = true;
            tCol.Unique = true;
            TB_ITEM_AUTHOR.Columns.Add(tCol);
            //TB_ITEM_AUTHOR.Constraints.Add(new UniqueConstraint("itemAuthorUnique", new DataColumn[] { tCol }, true));
            tCol = new DataColumn("ITEM_ID", typeof(long));
            tCol.AllowDBNull = false;
            TB_ITEM_AUTHOR.Columns.Add(tCol);
            tCol = new DataColumn("LAST", typeof(string));
            tCol.MaxLength = 50;
            tCol.AllowDBNull = false;
            TB_ITEM_AUTHOR.Columns.Add(tCol);
            tCol = new DataColumn("FIRST", typeof(string));
            tCol.MaxLength = 50;
            tCol.AllowDBNull = false;
            TB_ITEM_AUTHOR.Columns.Add(tCol);
            tCol = new DataColumn("SECOND", typeof(string));
            tCol.MaxLength = 50;
            tCol.AllowDBNull = false;
            TB_ITEM_AUTHOR.Columns.Add(tCol);
            tCol = new DataColumn("ROLE", typeof(byte));
            tCol.AllowDBNull = false;
            TB_ITEM_AUTHOR.Columns.Add(tCol);
            tCol = new DataColumn("RANK", typeof(short));
            tCol.AllowDBNull = false;
            TB_ITEM_AUTHOR.Columns.Add(tCol);
        }
        private void makeTB_Item_Review()
        {
            this.TB_ITEM_REVIEW = new DataTable("TB_ITEM_REVIEW");
            DataColumn tCol = new DataColumn("ITEM_REVIEW_ID", typeof(long));
            tCol.AutoIncrement = true;
            tCol.AutoIncrementSeed = -1;
            tCol.AutoIncrementStep = -1;
            tCol.AllowDBNull = false;
            tCol.ReadOnly = true;
            tCol.Unique = true;
            TB_ITEM_REVIEW.Columns.Add(tCol);
            //TB_ITEM_REVIEW.Constraints.Add(new UniqueConstraint("ItemReviewUnique", new DataColumn[] { tCol }, true));
            tCol = new DataColumn("ITEM_ID", typeof(long));
            tCol.AllowDBNull = false;
            TB_ITEM_REVIEW.Columns.Add(tCol);
            tCol = new DataColumn("REVIEW_ID", typeof(int));
            tCol.AllowDBNull = false;
            TB_ITEM_REVIEW.Columns.Add(tCol);
            tCol = new DataColumn("IS_INCLUDED", typeof(bool));
            TB_ITEM_REVIEW.Columns.Add(tCol);
            tCol = new DataColumn("MASTER_ITEM_ID", typeof(long));
            TB_ITEM_REVIEW.Columns.Add(tCol);
            tCol = new DataColumn("IS_DELETED", typeof(bool));
            TB_ITEM_REVIEW.Columns.Add(tCol);
        }
        private void makeTB_Item_Mag_Match()
        {
            TB_ITEM_MAG_MATCH = new DataTable("TB_ITEM_MAG_MATCH");
            DataColumn tCol = new DataColumn("ITEM_ID", typeof(long));
            tCol.AllowDBNull = false;
            TB_ITEM_MAG_MATCH.Columns.Add(tCol);

            tCol = new DataColumn("REVIEW_ID", typeof(int));
            tCol.AllowDBNull = false;
            TB_ITEM_MAG_MATCH.Columns.Add(tCol);

            tCol = new DataColumn("PaperId", typeof(long));
            tCol.AllowDBNull = false;
            TB_ITEM_MAG_MATCH.Columns.Add(tCol);

            tCol = new DataColumn("AutoMatchScore", typeof(Double));
            tCol.AllowDBNull = false;
            TB_ITEM_MAG_MATCH.Columns.Add(tCol);

            tCol = new DataColumn("ManualTrueMatch", typeof(Boolean));
            tCol.AllowDBNull = false;
            TB_ITEM_MAG_MATCH.Columns.Add(tCol);

            tCol = new DataColumn("ManualFalseMatch", typeof(Boolean));
            tCol.AllowDBNull = false;
            TB_ITEM_MAG_MATCH.Columns.Add(tCol);
        }
        private void makeTB_Zotero_Item_Review()
        {
            TB_ZOTERO_ITEM_REVIEW = new DataTable("TB_ZOTERO_ITEM_REVIEW");
            DataColumn tCol = new DataColumn("Zotero_item_review_ID", typeof(long));
            tCol.DefaultValue = 0;
            TB_ZOTERO_ITEM_REVIEW.Columns.Add(tCol);

            tCol = new DataColumn("ItemKey", typeof(string));
            tCol.MaxLength = 50;
            tCol.AllowDBNull = false;
            TB_ZOTERO_ITEM_REVIEW.Columns.Add(tCol);

            tCol = new DataColumn("ITEM_REVIEW_ID", typeof(long));
            tCol.AllowDBNull = false;
            TB_ZOTERO_ITEM_REVIEW.Columns.Add(tCol);
        }
    }
}
