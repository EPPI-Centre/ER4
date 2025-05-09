USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListFrequencyWithFilter]    Script Date: 22/06/2021 10:45:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemListFrequencyWithFilter]
(
	@ATTRIBUTE_ID BIGINT = null,
	@SET_ID BIGINT,
	@IS_INCLUDED BIT = null,
	@FILTER_ATTRIBUTE_ID BIGINT,
	@REVIEW_ID INT,

	@PageNum INT = 1,
	@PerPage INT = 3,
	@CurrentPage INT OUTPUT,
	@TotalPages INT OUTPUT,
	@TotalRows INT OUTPUT 
)

As

SET NOCOUNT ON
	declare @RowsToRetrieve int
	Declare @ID table (ItemID bigint ) --store IDs to build paged results as a simple join
	insert into @ID
	Select DISTINCT ir.ITEM_ID
			from TB_ITEM_REVIEW ir
			inner join TB_ITEM_ATTRIBUTE ia on ir.REVIEW_ID = @REVIEW_ID
								AND (ir.IS_INCLUDED = @IS_INCLUDED OR @IS_INCLUDED is null)
								AND ir.IS_DELETED = 'FALSE'
								and ia.ITEM_ID = ir.ITEM_ID
								AND ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
			inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID
								AND tis.IS_COMPLETED = 1
								and tis.SET_ID = @SET_ID
			inner join TB_ITEM_ATTRIBUTE ia2  on  ia2.ITEM_ID = ir.ITEM_ID
								and ia2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
			inner join TB_ITEM_SET tis2 on tis2.ITEM_SET_ID = ia2.ITEM_SET_ID
								AND tis2.IS_COMPLETED = 1
												
	--count results
	SELECT @TotalRows = count(ItemID) from @ID
	set @TotalPages = @TotalRows/@PerPage

	if @PageNum < 1
	set @PageNum = 1

	if @TotalRows % @PerPage != 0
	set @TotalPages = @TotalPages + 1

	set @RowsToRetrieve = @PerPage * @PageNum
	set @CurrentPage = @PageNum;

	WITH SearchResults AS
	(
		SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID
			, ROW_NUMBER() OVER(order by SHORT_TITLE, I.ITEM_ID) RowNum
		FROM TB_ITEM I
		INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] 
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
			TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN @ID on I.ITEM_ID = ItemID
	)
	Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
			TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
			[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
			AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
			, SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
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
