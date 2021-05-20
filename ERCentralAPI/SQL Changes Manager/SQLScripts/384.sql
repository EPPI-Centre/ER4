USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbWithThisCode]    Script Date: 3/25/2021 2:37:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_WebDbWithThisCode]
	-- Add the parameters for the stored procedure here
	@attributeId bigint 
	, @included bit null
	, @RevId int 
	, @WebDbId int
      
    , @PageNum INT = 1
    , @PerPage INT = 100
    , @CurrentPage INT OUTPUT
    , @TotalPages INT OUTPUT
    , @TotalRows INT OUTPUT  
AS
SET NOCOUNT ON
	--sanity check, ensure @RevId and @WebDbId match...
	Declare @CheckWebDbId int = null
	set @CheckWebDbId = (select WEBDB_ID from TB_WEBDB where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	IF @CheckWebDbId is null return;

	declare @items table (ItemId bigint primary key)
	declare @WebDbFilter bigint = (select w.WITH_ATTRIBUTE_ID from TB_WEBDB w where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	if @WebDbFilter is not null and @WebDbFilter > 1
	BEGIN
		insert into @items select distinct ir.item_id from TB_ITEM_REVIEW ir
			inner join tb_item_set tis on ir.ITEM_ID = tis.ITEM_ID and ir.REVIEW_ID = @RevId 
			and (@included is null OR ir.IS_INCLUDED = @included)
			and ir.IS_DELETED = 0 and tis.IS_COMPLETED = 1
			inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tia.ATTRIBUTE_ID = @WebDbFilter
			inner join TB_ITEM_ATTRIBUTE tia2 on tia2.ITEM_ID = ir.ITEM_ID and tia2.ATTRIBUTE_ID = @attributeId 
			inner join TB_ITEM_SET tis2 on tia2.ITEM_SET_ID = tis2.ITEM_SET_ID and tis2.IS_COMPLETED = 1 
		SELECT @TotalRows = @@ROWCOUNT
	END
	ELSE
	BEGIN
		insert into @items select distinct ir.item_id from TB_ITEM_REVIEW ir
			inner join TB_ITEM_ATTRIBUTE tia on ir.ITEM_ID = tia.ITEM_ID and tia.ATTRIBUTE_ID = @attributeId 
				and ir.REVIEW_ID = @RevId and ir.IS_DELETED = 0
				and (@included is null OR ir.IS_INCLUDED = @included)  
			inner join TB_ITEM_SET tis on tia.ITEM_SET_ID = tis.ITEM_SET_ID and IS_COMPLETED = 1
		SELECT @TotalRows = @@ROWCOUNT
	END
	declare @RowsToRetrieve int
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
		ROW_NUMBER() OVER(order by SHORT_TITLE, ir.ITEM_ID) RowNum
	FROM TB_ITEM i
		INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @RevId
		INNER JOIN @items id on id.ItemId = ir.ITEM_ID
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
		ORDER BY RowNum
	SELECT	@CurrentPage as N'@CurrentPage',
		@TotalPages as N'@TotalPages',
		@TotalRows as N'@TotalRows'
SET NOCOUNT OFF

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbAllItems]    Script Date: 3/25/2021 2:20:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_WebDbAllItems]
	-- Add the parameters for the stored procedure here
	@included bit null
	, @RevId int 
	, @WebDbId int
      
    , @PageNum INT = 1
    , @PerPage INT = 100
    , @CurrentPage INT OUTPUT
    , @TotalPages INT OUTPUT
    , @TotalRows INT OUTPUT  
AS
SET NOCOUNT ON
	--sanity check, ensure @RevId and @WebDbId match...
	Declare @CheckWebDbId int = null
	set @CheckWebDbId = (select WEBDB_ID from TB_WEBDB where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	IF @CheckWebDbId is null return;

	declare @items table (ItemId bigint primary key)
	declare @WebDbFilter bigint = (select w.WITH_ATTRIBUTE_ID from TB_WEBDB w where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	if @WebDbFilter is not null and @WebDbFilter > 1
	BEGIN
		insert into @items select distinct ir.item_id from TB_ITEM_REVIEW ir
			inner join tb_item_set tis on ir.ITEM_ID = tis.ITEM_ID and ir.REVIEW_ID = @RevId 
			and (@included is null OR ir.IS_INCLUDED = @included)
			and ir.IS_DELETED = 0 and tis.IS_COMPLETED = 1
			inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tia.ATTRIBUTE_ID = @WebDbFilter
		SELECT @TotalRows = @@ROWCOUNT
	END
	ELSE
	BEGIN
		insert into @items select distinct ir.item_id from TB_ITEM_REVIEW ir
			WHERE ir.REVIEW_ID = @RevId and ir.IS_DELETED = 0
				and (@included is null OR ir.IS_INCLUDED = @included)
		SELECT @TotalRows = @@ROWCOUNT
	END
	declare @RowsToRetrieve int
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
		ROW_NUMBER() OVER(order by SHORT_TITLE, ir.ITEM_ID) RowNum
	FROM TB_ITEM i
		INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID
		INNER JOIN @items id on id.ItemId = ir.ITEM_ID and ir.REVIEW_ID = @RevId
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
		ORDER BY RowNum
	SELECT	@CurrentPage as N'@CurrentPage',
		@TotalPages as N'@TotalPages',
		@TotalRows as N'@TotalRows'
SET NOCOUNT OFF

GO
