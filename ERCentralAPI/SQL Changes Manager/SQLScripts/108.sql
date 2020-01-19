

--USE [Reviewer]
--GO
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemListMaybeMagMatches]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ItemListMaybeMagMatches]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ItemListMaybeMagMatches]    Script Date: 18/08/2019 09:02:07 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--create procedure [dbo].[st_ItemListMaybeMagMatches]
--(
--      @REVIEW_ID INT,
--      @SHOW_INCLUDED BIT = 'true',
--      @ATTRIBUTE_SET_ID_LIST NVARCHAR(MAX) = '',
      
--      @PageNum INT = 1,
--      @PerPage INT = 3,
--      @CurrentPage INT OUTPUT,
--      @TotalPages INT OUTPUT,
--      @TotalRows INT OUTPUT 
--)
--with recompile
--As

--SET NOCOUNT ON

--declare @RowsToRetrieve int
--Declare @ID table (ItemID bigint primary key )
--IF (@ATTRIBUTE_SET_ID_LIST = '')
--BEGIN

--       --store IDs to build paged results as a simple join
--	  INSERT INTO @ID SELECT DISTINCT (I.ITEM_ID)
--      FROM TB_ITEM I
--      INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
--            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
--			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
--			INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID AND IMM.AutoMatchScore < 0.90
--				and IMM.ManualFalseMatch = 'FALSE' and IMM.ManualTrueMatch = 'FALSE'
--	  SELECT @TotalRows = @@ROWCOUNT

--      set @TotalPages = @TotalRows/@PerPage

--      if @PageNum < 1
--      set @PageNum = 1

--      if @TotalRows % @PerPage != 0
--      set @TotalPages = @TotalPages + 1

--      set @RowsToRetrieve = @PerPage * @PageNum
--      set @CurrentPage = @PageNum;

--      WITH SearchResults AS
--      (
--      SELECT DISTINCT (ir.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
--            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
--      FROM TB_ITEM i
--			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID
--			INNER JOIN @ID id on id.ItemID = ir.ITEM_ID
            
--      )
--      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
--                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
--                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
--                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
--                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
--                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
--            FROM SearchResults
--                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
--                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
--            WHERE RowNum > @RowsToRetrieve - @PerPage
--            AND RowNum <= @RowsToRetrieve
--            ORDER BY SHORT_TITLE
--END
--ELSE /* FILTER BY A LIST OF ATTRIBUTES */
--BEGIN
--	      SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
--            FROM TB_ITEM_REVIEW I
--      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
--      INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
--      INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
--      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
--      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!
--	  INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID AND IMM.AutoMatchScore < 0.7
--				and IMM.ManualFalseMatch = 'FALSE' and IMM.ManualTrueMatch = 'FALSE'

--      WHERE I.IS_INCLUDED = @SHOW_INCLUDED
--            AND I.REVIEW_ID = @REVIEW_ID
--			AND I.IS_DELETED = 'FALSE'

--      set @TotalPages = @TotalRows/@PerPage

--      if @PageNum < 1
--      set @PageNum = 1

--      if @TotalRows % @PerPage != 0
--      set @TotalPages = @TotalPages + 1

--      set @RowsToRetrieve = @PerPage * @PageNum
--      set @CurrentPage = @PageNum;

--      WITH SearchResults AS
--      (
--      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID, ADDITIONAL_TEXT,
--            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
--      FROM TB_ITEM I
--       INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
--            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED

--      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
--      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!
--      )
--      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
--                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
--                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
--                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
--                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS, ADDITIONAL_TEXT
--                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
--            FROM SearchResults
--                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
--                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
--            WHERE RowNum > @RowsToRetrieve - @PerPage
--            AND RowNum <= @RowsToRetrieve 
--            ORDER BY SHORT_TITLE
--END

--SELECT      @CurrentPage as N'@CurrentPage',
--            @TotalPages as N'@TotalPages',
--            @TotalRows as N'@TotalRows'

--GO


 

--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemListMagNoMatches]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ItemListMagNoMatches]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ItemListMagNoMatches]    Script Date: 23/08/2019 22:59:36 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--create procedure [dbo].[st_ItemListMagNoMatches]
--(
--      @REVIEW_ID INT,
--      @SHOW_INCLUDED BIT = 'true',
      
--      @PageNum INT = 1,
--      @PerPage INT = 3,
--      @CurrentPage INT OUTPUT,
--      @TotalPages INT OUTPUT,
--      @TotalRows INT OUTPUT 
--)
--with recompile
--As

--SET NOCOUNT ON

--declare @RowsToRetrieve int
--Declare @ID table (ItemID bigint primary key )

--       --store IDs to build paged results as a simple join
--	  INSERT INTO @ID SELECT DISTINCT (I.ITEM_ID)
--      FROM TB_ITEM I
--      INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
--            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
--			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
--			left outer JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID
--			where imm.ITEM_ID is null
--	  SELECT @TotalRows = @@ROWCOUNT

--      set @TotalPages = @TotalRows/@PerPage

--      if @PageNum < 1
--      set @PageNum = 1

--      if @TotalRows % @PerPage != 0
--      set @TotalPages = @TotalPages + 1

--      set @RowsToRetrieve = @PerPage * @PageNum
--      set @CurrentPage = @PageNum;

--      WITH SearchResults AS
--      (
--      SELECT DISTINCT (ir.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
--            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
--      FROM TB_ITEM i
--			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID
--			INNER JOIN @ID id on id.ItemID = ir.ITEM_ID
            
--      )
--      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
--                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
--                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
--                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
--                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
--                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
--            FROM SearchResults
--                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
--                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
--            WHERE RowNum > @RowsToRetrieve - @PerPage
--            AND RowNum <= @RowsToRetrieve
--            ORDER BY SHORT_TITLE


--SELECT      @CurrentPage as N'@CurrentPage',
--            @TotalPages as N'@TotalPages',
--            @TotalRows as N'@TotalRows'

--GO


--USE [Reviewer]
--GO
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CheckReviewHasUpdates]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_CheckReviewHasUpdates]
--GO
--/****** Object:  StoredProcedure [dbo].[st_CheckReviewHasUpdates]    Script Date: 21/09/2019 18:23:05 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
---- =============================================
---- Author:		James
---- Create date: 
---- Description:	Checks to see whether a review has any auto-identified studies for authors to check
---- =============================================
--CREATE PROCEDURE [dbo].[st_CheckReviewHasUpdates] 
--	-- Add the parameters for the stored procedure here
--	@REVIEW_id int = 0,
--	@NUpdates INT OUTPUT
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    select @NUpdates = sum(N_PAPERS) from Reviewer.dbo.tb_MAG_RELATED_RUN
--		where REVIEW_ID = @REVIEW_id
--		and USER_STATUS = 'Unchecked'

--	if @NUpdates is null
--	begin
--		set @NUpdates = 0
--	end
--END
--GO

--USE [Reviewer]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ItemListMaybeMagMatches]    Script Date: 30/09/2019 22:54:28 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

--CREATE OR ALTER procedure [dbo].[st_ItemListMaybeMagMatches]
--(
--      @REVIEW_ID INT,
--      @SHOW_INCLUDED BIT = 'true',
--      @ATTRIBUTE_SET_ID_LIST NVARCHAR(MAX) = '',
      
--      @PageNum INT = 1,
--      @PerPage INT = 3,
--      @CurrentPage INT OUTPUT,
--      @TotalPages INT OUTPUT,
--      @TotalRows INT OUTPUT 
--)
--with recompile
--As

--SET NOCOUNT ON

--declare @RowsToRetrieve int
--Declare @ID table (ItemID bigint primary key )
--IF (@ATTRIBUTE_SET_ID_LIST = '')
--BEGIN

--       --store IDs to build paged results as a simple join
--	  INSERT INTO @ID
--	  SELECT DISTINCT (I.ITEM_ID) FROM TB_ITEM I
--      INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
--            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
--			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
--			INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID AND IMM.AutoMatchScore < 0.70
--				and IMM.ManualFalseMatch = 'FALSE' and IMM.ManualTrueMatch = 'FALSE'
--	  SELECT @TotalRows = @@ROWCOUNT

--      set @TotalPages = @TotalRows/@PerPage

--      if @PageNum < 1
--      set @PageNum = 1

--      if @TotalRows % @PerPage != 0
--      set @TotalPages = @TotalPages + 1

--      set @RowsToRetrieve = @PerPage * @PageNum
--      set @CurrentPage = @PageNum;

--      WITH SearchResults AS
--      (
--      SELECT DISTINCT (ir.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
--            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
--      FROM TB_ITEM i
--			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID
--			INNER JOIN @ID id on id.ItemID = ir.ITEM_ID
            
--      )
--      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
--                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
--                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
--                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
--                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
--                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
--            FROM SearchResults
--                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
--                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
--            WHERE RowNum > @RowsToRetrieve - @PerPage
--            AND RowNum <= @RowsToRetrieve
--            ORDER BY SHORT_TITLE
--END
--ELSE /* FILTER BY A LIST OF ATTRIBUTES */
--BEGIN
--	      SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
--            FROM TB_ITEM_REVIEW I
--      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
--      INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
--      INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
--      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
--      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!
--	  INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID AND IMM.AutoMatchScore < 0.7
--				and IMM.ManualFalseMatch = 'FALSE' and IMM.ManualTrueMatch = 'FALSE'

--      WHERE I.IS_INCLUDED = @SHOW_INCLUDED
--            AND I.REVIEW_ID = @REVIEW_ID
--			AND I.IS_DELETED = 'FALSE'

--      set @TotalPages = @TotalRows/@PerPage

--      if @PageNum < 1
--      set @PageNum = 1

--      if @TotalRows % @PerPage != 0
--      set @TotalPages = @TotalPages + 1

--      set @RowsToRetrieve = @PerPage * @PageNum
--      set @CurrentPage = @PageNum;

--      WITH SearchResults AS
--      (
--      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID, ADDITIONAL_TEXT,
--            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
--      FROM TB_ITEM I
--       INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
--            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED

--      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
--      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!
--      )
--      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
--                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
--                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
--                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
--                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS, ADDITIONAL_TEXT
--                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
--            FROM SearchResults
--                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
--                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
--            WHERE RowNum > @RowsToRetrieve - @PerPage
--            AND RowNum <= @RowsToRetrieve 
--            ORDER BY SHORT_TITLE
--END

--SELECT      @CurrentPage as N'@CurrentPage',
--            @TotalPages as N'@TotalPages',
--            @TotalRows as N'@TotalRows'
--GO


--USE [Reviewer]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ItemListMagNoMatches]    Script Date: 30/09/2019 22:54:22 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

--CREATE OR ALTER procedure [dbo].[st_ItemListMagNoMatches]
--(
--      @REVIEW_ID INT,
--      @SHOW_INCLUDED BIT = 'true',
      
--      @PageNum INT = 1,
--      @PerPage INT = 3,
--      @CurrentPage INT OUTPUT,
--      @TotalPages INT OUTPUT,
--      @TotalRows INT OUTPUT 
--)
--with recompile
--As

--SET NOCOUNT ON

--declare @RowsToRetrieve int
--Declare @ID table (ItemID bigint primary key )

--       --store IDs to build paged results as a simple join
--	  INSERT INTO @ID SELECT DISTINCT (I.ITEM_ID)
--      FROM TB_ITEM I
--      INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
--            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
--			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
--			left outer JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID
--			where imm.ITEM_ID is null
--	  SELECT @TotalRows = @@ROWCOUNT

--      set @TotalPages = @TotalRows/@PerPage

--      if @PageNum < 1
--      set @PageNum = 1

--      if @TotalRows % @PerPage != 0
--      set @TotalPages = @TotalPages + 1

--      set @RowsToRetrieve = @PerPage * @PageNum
--      set @CurrentPage = @PageNum;

--      WITH SearchResults AS
--      (
--      SELECT DISTINCT (ir.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
--            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
--      FROM TB_ITEM i
--			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID
--			INNER JOIN @ID id on id.ItemID = ir.ITEM_ID
            
--      )
--      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
--                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
--                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
--                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
--                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
--                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
--            FROM SearchResults
--                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
--                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
--            WHERE RowNum > @RowsToRetrieve - @PerPage
--            AND RowNum <= @RowsToRetrieve
--            ORDER BY SHORT_TITLE


--SELECT      @CurrentPage as N'@CurrentPage',
--            @TotalPages as N'@TotalPages',
--            @TotalRows as N'@TotalRows'
--GO


--USE [Reviewer]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ItemListMagMatches]    Script Date: 30/09/2019 22:54:04 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

--CREATE OR ALTER procedure [dbo].[st_ItemListMagMatches]
--(
--      @REVIEW_ID INT,
--      @SHOW_INCLUDED BIT = 'true',
      
--      @PageNum INT = 1,
--      @PerPage INT = 3,
--      @CurrentPage INT OUTPUT,
--      @TotalPages INT OUTPUT,
--      @TotalRows INT OUTPUT 
--)
--with recompile
--As

--SET NOCOUNT ON

--declare @RowsToRetrieve int
--Declare @ID table (ItemID bigint primary key )

--       --store IDs to build paged results as a simple join
--	  INSERT INTO @ID
--	  SELECT DISTINCT (I.ITEM_ID) FROM TB_ITEM_REVIEW I
--			INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID AND IMM.AutoMatchScore >= 0.70
--				and IMM.ManualFalseMatch = 'FALSE' and IMM.ManualTrueMatch = 'FALSE'
--			WHERE I.REVIEW_ID = @REVIEW_ID AND I.IS_INCLUDED = @SHOW_INCLUDED
--			AND I.IS_DELETED = 'FALSE'

--	  SELECT @TotalRows = @@ROWCOUNT

--      set @TotalPages = @TotalRows/@PerPage

--      if @PageNum < 1
--      set @PageNum = 1

--      if @TotalRows % @PerPage != 0
--      set @TotalPages = @TotalPages + 1

--      set @RowsToRetrieve = @PerPage * @PageNum
--      set @CurrentPage = @PageNum;

--      WITH SearchResults AS
--      (
--      SELECT DISTINCT (ir.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
--            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
--      FROM TB_ITEM i
--			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID
--			INNER JOIN @ID id on id.ItemID = ir.ITEM_ID
            
--      )
--      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
--                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
--                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
--                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
--                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
--                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
--            FROM SearchResults
--                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
--                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
--            WHERE RowNum > @RowsToRetrieve - @PerPage
--            AND RowNum <= @RowsToRetrieve
--            ORDER BY SHORT_TITLE


--SELECT      @CurrentPage as N'@CurrentPage',
--            @TotalPages as N'@TotalPages',
--            @TotalRows as N'@TotalRows'
--GO

