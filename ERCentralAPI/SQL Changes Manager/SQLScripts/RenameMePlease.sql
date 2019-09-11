USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemUpdate]    Script Date: 21/07/2019 08:26:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemUpdate]
(
	@ITEM_ID BIGINT
,	@TITLE NVARCHAR(4000) = NULL
,	@TYPE_ID TINYINT
,	@PARENT_TITLE NVARCHAR(4000)
,	@SHORT_TITLE NVARCHAR(70)
,	@DATE_CREATED DATETIME = NULL
,	@CREATED_BY NVARCHAR(50) = NULL
,	@DATE_EDITED DATETIME = NULL
,	@EDITED_BY NVARCHAR(50) = NULL
,	@YEAR NCHAR(4) = NULL
,	@MONTH NVARCHAR(10) = NULL
,	@STANDARD_NUMBER NVARCHAR(255) = NULL
,	@CITY NVARCHAR(100) = NULL
,	@COUNTRY NVARCHAR(100) = NULL
,	@PUBLISHER NVARCHAR(1000) = NULL
,	@INSTITUTION NVARCHAR(1000) = NULL
,	@VOLUME NVARCHAR(56) = NULL
,	@PAGES NVARCHAR(50) = NULL
,	@EDITION NVARCHAR(200) = NULL
,	@ISSUE NVARCHAR(100) = NULL
,	@IS_LOCAL BIT = NULL
,	@AVAILABILITY NVARCHAR(255) = NULL
,	@URL NVARCHAR(500) = NULL
,	@COMMENTS NVARCHAR(MAX) = NULL
,	@ABSTRACT NVARCHAR(MAX) = NULL
,	@IS_INCLUDED BIT = NULL
,	@REVIEW_ID INT
,	@DOI NVARCHAR(500) = NULL
,	@KEYWORDS NVARCHAR(MAX) = NULL
,	@SEARCHTEXT NVARCHAR(500) = NULL
)

As

SET NOCOUNT ON

UPDATE TB_ITEM

SET TITLE = @TITLE
,	[TYPE_ID] = @TYPE_ID
,	PARENT_TITLE = @PARENT_TITLE
,	SHORT_TITLE = @SHORT_TITLE
,	DATE_CREATED = @DATE_CREATED
,	CREATED_BY = @CREATED_BY
,	DATE_EDITED = @DATE_EDITED
,	EDITED_BY = @EDITED_BY
,	[YEAR] = @YEAR
,	[MONTH] = @MONTH
,	STANDARD_NUMBER = @STANDARD_NUMBER
,	CITY = @CITY
,	COUNTRY = @COUNTRY
,	PUBLISHER = @PUBLISHER
,	INSTITUTION = @INSTITUTION
,	VOLUME = @VOLUME
,	PAGES = @PAGES
,	EDITION = @EDITION
,	ISSUE = @ISSUE
,	IS_LOCAL = @IS_LOCAL
,	AVAILABILITY = @AVAILABILITY
,	URL = @URL
,	COMMENTS = @COMMENTS
,	ABSTRACT = @ABSTRACT
,	DOI = @DOI
,	KEYWORDS = @KEYWORDS
,	SearchText = @SEARCHTEXT

WHERE ITEM_ID = @ITEM_ID

UPDATE TB_ITEM_REVIEW
SET IS_INCLUDED = @IS_INCLUDED 
WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID

SET NOCOUNT OFF
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListMaybeMagMatches]    Script Date: 18/08/2019 09:02:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[st_ItemListMaybeMagMatches]
(
      @REVIEW_ID INT,
      @SHOW_INCLUDED BIT = 'true',
      @ATTRIBUTE_SET_ID_LIST NVARCHAR(MAX) = '',
      
      @PageNum INT = 1,
      @PerPage INT = 3,
      @CurrentPage INT OUTPUT,
      @TotalPages INT OUTPUT,
      @TotalRows INT OUTPUT 
)
with recompile
As

SET NOCOUNT ON

declare @RowsToRetrieve int
Declare @ID table (ItemID bigint primary key )
IF (@ATTRIBUTE_SET_ID_LIST = '')
BEGIN

       --store IDs to build paged results as a simple join
	  INSERT INTO @ID SELECT DISTINCT (I.ITEM_ID)
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
			INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID AND IMM.AutoMatchScore < 0.90
				and IMM.ManualFalseMatch = 'FALSE' and IMM.ManualTrueMatch = 'FALSE'
	  SELECT @TotalRows = @@ROWCOUNT

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (ir.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM i
			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID
			INNER JOIN @ID id on id.ItemID = ir.ITEM_ID
            
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve
            ORDER BY SHORT_TITLE
END
ELSE /* FILTER BY A LIST OF ATTRIBUTES */
BEGIN
	      SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
            FROM TB_ITEM_REVIEW I
      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
      INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
      INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!
	  INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID AND IMM.AutoMatchScore < 0.7
				and IMM.ManualFalseMatch = 'FALSE' and IMM.ManualTrueMatch = 'FALSE'

      WHERE I.IS_INCLUDED = @SHOW_INCLUDED
            AND I.REVIEW_ID = @REVIEW_ID
			AND I.IS_DELETED = 'FALSE'

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID, ADDITIONAL_TEXT,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM I
       INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED

      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS, ADDITIONAL_TEXT
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve 
            ORDER BY SHORT_TITLE
END

SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'

GO

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_REVIEW SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_tb_ITEM_MAG_MATCH
	(
	ITEM_ID bigint NOT NULL,
	REVIEW_ID int NOT NULL,
	PaperId bigint NOT NULL,
	AutoMatchScore float(53) NULL,
	ManualTrueMatch bit NULL,
	ManualFalseMatch bit NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_tb_ITEM_MAG_MATCH SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM dbo.tb_ITEM_MAG_MATCH)
	 EXEC('INSERT INTO dbo.Tmp_tb_ITEM_MAG_MATCH (ITEM_ID, REVIEW_ID, PaperId, AutoMatchScore, ManualTrueMatch, ManualFalseMatch)
		SELECT ITEM_ID, REVIEW_ID, PaperId, AutoMatchScore, ManualTrueMatch, ManualFalseMatch FROM dbo.tb_ITEM_MAG_MATCH WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.tb_ITEM_MAG_MATCH
GO
EXECUTE sp_rename N'dbo.Tmp_tb_ITEM_MAG_MATCH', N'tb_ITEM_MAG_MATCH', 'OBJECT' 
GO
ALTER TABLE dbo.tb_ITEM_MAG_MATCH ADD CONSTRAINT
	FK_tb_ITEM_MAG_MATCH_TB_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_ITEM SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.tb_ITEM_MAG_MATCH ADD CONSTRAINT
	FK_tb_ITEM_MAG_MATCH_TB_ITEM FOREIGN KEY
	(
	ITEM_ID
	) REFERENCES dbo.TB_ITEM
	(
	ITEM_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.tb_ITEM_MAG_MATCH SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

USE [Reviewer]
GO

USE [Reviewer]

GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20190911-093715] ON [dbo].[tb_ITEM_MAG_MATCH]
(
	[PaperId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

GO


/****** Object:  StoredProcedure [dbo].[st_ItemListMagNoMatches]    Script Date: 23/08/2019 22:59:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[st_ItemListMagNoMatches]
(
      @REVIEW_ID INT,
      @SHOW_INCLUDED BIT = 'true',
      
      @PageNum INT = 1,
      @PerPage INT = 3,
      @CurrentPage INT OUTPUT,
      @TotalPages INT OUTPUT,
      @TotalRows INT OUTPUT 
)
with recompile
As

SET NOCOUNT ON

declare @RowsToRetrieve int
Declare @ID table (ItemID bigint primary key )

       --store IDs to build paged results as a simple join
	  INSERT INTO @ID SELECT DISTINCT (I.ITEM_ID)
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
			left outer JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID
			where imm.ITEM_ID is null
	  SELECT @TotalRows = @@ROWCOUNT

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (ir.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM i
			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID
			INNER JOIN @ID id on id.ItemID = ir.ITEM_ID
            
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve
            ORDER BY SHORT_TITLE


SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewContactForSiteAdmin]    Script Date: 08/09/2019 23:12:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER procedure [dbo].[st_ReviewContactForSiteAdmin]
(
	@CONTACT_ID INT
	,@REVIEW_ID int
)

As

SELECT 0 as REVIEW_CONTACT_ID, - r.REVIEW_ID as REVIEW_ID, rc.CONTACT_ID, REVIEW_NAME, 'AdminUser;' as ROLES
	,own.CONTACT_NAME as 'OWNER', case when LR is null
									then r.DATE_CREATED
									else LR
								 end
								 as 'LAST_ACCESS'
	, r.SHOW_SCREENING, r.SCREENING_CODE_SET_ID, r.SCREENING_MODE, r.SCREENING_WHAT_ATTRIBUTE_ID, r.SCREENING_N_PEOPLE
	, r.SCREENING_RECONCILLIATION, r.SCREENING_AUTO_EXCLUDE, SCREENING_MODEL_RUNNING, SCREENING_INDEXED
	, BL_ACCOUNT_CODE,BL_AUTH_CODE, BL_TX, BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX
	, (SELECT SUM(N_PAPERS) from Reviewer.dbo.tb_MAG_RELATED_RUN MRR
			WHERE REVIEW_ID = @REVIEW_ID  and USER_STATUS = 'Unchecked') NAutoUpdates
FROM TB_CONTACT rc
INNER JOIN TB_REVIEW r ON rc.CONTACT_ID = @CONTACT_ID and rc.IS_SITE_ADMIN = 1 and r.REVIEW_ID = @REVIEW_ID 
inner join TB_CONTACT own on r.FUNDER_ID = own.CONTACT_ID
left join (
			select MAX(LAST_RENEWED) LR, REVIEW_ID
			from ReviewerAdmin.dbo.TB_LOGON_TICKET  
			where @CONTACT_ID = CONTACT_ID and REVIEW_ID = @REVIEW_ID
			group by REVIEW_ID
			) as t
			on t.REVIEW_ID = r.REVIEW_ID
WHERE rc.CONTACT_ID = @CONTACT_ID
ORDER BY REVIEW_NAME

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewContact]    Script Date: 08/09/2019 23:09:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ReviewContact]
(
	@CONTACT_ID INT
)

As

SELECT REVIEW_CONTACT_ID, rc.REVIEW_ID, rc.CONTACT_ID, REVIEW_NAME, dbo.fn_REBUILD_ROLES(rc.REVIEW_CONTACT_ID) as ROLES
	,own.CONTACT_NAME as 'OWNER', case when LR is null
									then r.DATE_CREATED
									else LR
								 end
								 as 'LAST_ACCESS'
	, r.SHOW_SCREENING, r.SCREENING_CODE_SET_ID, r.SCREENING_MODE, r.SCREENING_WHAT_ATTRIBUTE_ID, r.SCREENING_N_PEOPLE
	, r.SCREENING_RECONCILLIATION, r.SCREENING_AUTO_EXCLUDE, SCREENING_MODEL_RUNNING, SCREENING_INDEXED
	, BL_ACCOUNT_CODE,BL_AUTH_CODE, BL_TX, BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX
	, (SELECT SUM(N_PAPERS) from Reviewer.dbo.tb_MAG_RELATED_RUN MRR
			WHERE REVIEW_ID = rc.REVIEW_ID  and USER_STATUS = 'Unchecked') NAutoUpdates
FROM TB_REVIEW_CONTACT rc
INNER JOIN TB_REVIEW r ON rc.REVIEW_ID = r.REVIEW_ID
inner join TB_CONTACT own on r.FUNDER_ID = own.CONTACT_ID
left join (
			select MAX(LAST_RENEWED) LR, REVIEW_ID
			from ReviewerAdmin.dbo.TB_LOGON_TICKET  
			where @CONTACT_ID = CONTACT_ID
			group by REVIEW_ID
			) as t
			on t.REVIEW_ID = r.REVIEW_ID
WHERE rc.CONTACT_ID = @CONTACT_ID and (r.ARCHIE_ID is null OR r.ARCHIE_ID = 'prospective_______')
ORDER BY REVIEW_NAME

GO
