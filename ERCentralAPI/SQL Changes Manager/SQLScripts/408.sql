USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeChildFrequencies]    Script Date: 02/06/2021 15:43:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemAttributeChildFrequencies]
(
      @ATTRIBUTE_ID BIGINT = null,
      @SET_ID BIGINT,
      @IS_INCLUDED BIT = null,
      @FILTER_ATTRIBUTE_ID BIGINT,
      @REVIEW_ID INT
)

As

SET NOCOUNT ON

IF (@FILTER_ATTRIBUTE_ID = -1)
BEGIN

	SELECT ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID,
		  COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) AS ITEM_COUNT, ATTRIBUTE_ORDER FROM TB_ITEM_ATTRIBUTE
	      
		  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
		  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
				AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
		  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
		  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				AND (TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED OR @IS_INCLUDED is null)
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		  GROUP BY TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
		  union
		  Select 'None of the codes above', -@ATTRIBUTE_ID, -@SET_ID,  COUNT(DISTINCT ITEM_ID) AS ITEM_COUNT, 10000
			from TB_ITEM_REVIEW 
			where ITEM_ID not in 
					(
						select TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
						  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
								AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
						  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
								AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
						  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
								AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND (TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED OR @IS_INCLUDED is null)
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
					)
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND (TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED OR @IS_INCLUDED is null)
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		  ORDER BY ATTRIBUTE_ORDER
end
ELSE
BEGIN
	SELECT ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID,
		  COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) AS ITEM_COUNT, ATTRIBUTE_ORDER FROM TB_ITEM_ATTRIBUTE
	      
		  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
		  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
				AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
		  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
		  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				AND (TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED OR @IS_INCLUDED is null)
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		  INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND IA2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
		  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
		  GROUP BY TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
		  UNION
		  Select 'None of the codes above', -@ATTRIBUTE_ID, -@SET_ID,  COUNT(DISTINCT TB_ITEM_REVIEW.ITEM_ID) AS ITEM_COUNT, 10000
			from TB_ITEM_REVIEW 
			INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND IA2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
			where TB_ITEM_REVIEW.ITEM_ID not in 
					(
						select TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
						  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
								AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
						  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
								AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
						  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
								AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND (TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED OR @IS_INCLUDED is null)
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
						  INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND IA2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
					)
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND (TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED OR @IS_INCLUDED is null)
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		  ORDER BY ATTRIBUTE_ORDER
END

SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemList]    Script Date: 02/06/2021 16:12:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemList]
(
      @REVIEW_ID INT,
      @SHOW_INCLUDED BIT = null,
      @SHOW_DELETED BIT = 0,
      @SOURCE_ID INT = 0,
      @ATTRIBUTE_SET_ID_LIST NVARCHAR(MAX) = '',
      
      @PageNum INT = 1,
      @PerPage INT = 3,
      @CurrentPage INT OUTPUT,
      @TotalPages INT OUTPUT,
      @TotalRows INT OUTPUT 
)

As

SET NOCOUNT ON

declare @RowsToRetrieve int
Declare @ID table (ItemID bigint primary key )
IF (@SOURCE_ID = 0) AND (@ATTRIBUTE_SET_ID_LIST = '') /* LIST ALL ITEMS IN THE REVIEW */
BEGIN

       --store IDs to build paged results as a simple join
	  INSERT INTO @ID SELECT DISTINCT (I.ITEM_ID)
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            AND (TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED OR @SHOW_INCLUDED is null)
            AND TB_ITEM_REVIEW.IS_DELETED = @SHOW_DELETED

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
            ROW_NUMBER() OVER(order by SHORT_TITLE, ir.ITEM_ID) RowNum
      FROM TB_ITEM i
			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
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
            ORDER BY RowNum
END
ELSE /* FILTER BY A LIST OF ATTRIBUTES */

IF (@ATTRIBUTE_SET_ID_LIST != '')
BEGIN
      SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
            FROM TB_ITEM_REVIEW I
      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
      INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
      INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!

      WHERE (I.IS_INCLUDED = @SHOW_INCLUDED OR @SHOW_INCLUDED is null)
            AND I.IS_DELETED = @SHOW_DELETED
            AND I.REVIEW_ID = @REVIEW_ID

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
            ROW_NUMBER() OVER(order by SHORT_TITLE, I.ITEM_ID) RowNum
      FROM TB_ITEM I
       INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            AND (TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED OR @SHOW_INCLUDED is null)
            AND TB_ITEM_REVIEW.IS_DELETED = @SHOW_DELETED

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
            ORDER BY RowNum
END
ELSE -- LISTING SOURCELESS
IF (@SOURCE_ID = -1)
BEGIN
       --store IDs to build paged results as a simple join
	  INSERT INTO @ID 	  
		select distinct IR.ITEM_ID
		from TB_ITEM_REVIEW ir   
		where ir.REVIEW_ID = @REVIEW_ID and ir.ITEM_ID not in 
		( 
			Select ir1.Item_id from tb_source s
			inner join TB_ITEM_SOURCE tis on s.SOURCE_ID = tis.SOURCE_ID and s.REVIEW_ID = @REVIEW_ID
			inner join TB_ITEM_REVIEW ir1 on tis.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @REVIEW_ID
		)
	  

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
      SELECT DISTINCT (I.ITEM_ID), IR.IS_DELETED, IS_INCLUDED, IR.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE, I.ITEM_ID) RowNum
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] 
      INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = I.ITEM_ID AND IR.REVIEW_ID = @REVIEW_ID
      INNER JOIN @ID id on id.ItemID = I.ITEM_ID
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
      
END
ELSE -- LISTING BY A SOURCE
BEGIN
      SELECT @TotalRows = count(I.ITEM_ID)
      FROM TB_ITEM_REVIEW I
      INNER JOIN TB_ITEM_SOURCE ON TB_ITEM_SOURCE.ITEM_ID = I.ITEM_ID AND TB_ITEM_SOURCE.SOURCE_ID = @SOURCE_ID
      WHERE I.REVIEW_ID = @REVIEW_ID

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE, I.ITEM_ID) RowNum
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
      INNER JOIN TB_ITEM_SOURCE ON TB_ITEM_SOURCE.ITEM_ID = I.ITEM_ID AND TB_ITEM_SOURCE.SOURCE_ID = @SOURCE_ID
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
      
END

SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'


SET NOCOUNT OFF

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListFrequencyNoneOfTheAbove]    Script Date: 02/06/2021 16:21:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemListFrequencyNoneOfTheAbove]
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
IF (@FILTER_ATTRIBUTE_ID = -1)
BEGIN
	insert into @ID
	Select DISTINCT ITEM_ID
			from TB_ITEM_REVIEW 
			where ITEM_ID not in 
					(
						select TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
						  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
								AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
						  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
								AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
						  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
								AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND (TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED OR @IS_INCLUDED is null)
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
					)
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND (TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED OR @IS_INCLUDED is null)
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
END
ELSE
BEGIN
	insert into @ID
	Select TB_ITEM_REVIEW.ITEM_ID
			from TB_ITEM_REVIEW 
			INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND IA2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
			where TB_ITEM_REVIEW.ITEM_ID not in 
					(
						select TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
						  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
								AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
						  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
								AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
						  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
								AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND (TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED OR @IS_INCLUDED is null)
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
						  INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND IA2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
					)
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND (TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED OR @IS_INCLUDED is null)
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
END
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

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbFrequencyCrosstabAndMap]    Script Date: 02/06/2021 16:31:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ErFrequencyCrosstabAndMap]
	-- Add the parameters for the stored procedure here
	@attributeIdXAxis bigint 
	, @setIdXAxis int
	, @included bit null = null
	, @attributeIdYAxis bigint = 0
	, @setIdYAxis int = 0 
	, @segmentsParent bigint = 0
	, @setIdSegments int = 0
	, @onlyThisAttribute bigint = 0
	, @RevId int 
AS
BEGIN

--declare 
	--@attributeIdXAxis bigint = 64472  --62475 0
	--, @setIdXAxis int = 644
	--, @attributeIdYAxis bigint = 0
	--, @setIdYAxis int = 0 --644 664
	--, @SegmentsParent bigint = 119121
	--, @setIdSegments int = 0--1880
	--, @OnlyThisAttribute bigint = 0
	--, @RevId int = 99
	--, @WebDbId int = 18


declare @items table (ItemId bigint primary key, X_atts varchar(max) null, Y_atts  varchar(max) null, segments varchar(max) null)
declare @attsX table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
declare @attsY table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
declare @segments table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
--last minute table: the parent IDs and names are in @codeNames
declare @codeNames table (SETIDX_ID bigint primary key, SETIDX_NAME nvarchar(255), SETIDY_ID bigint, SETIDY_NAME nvarchar(255),
							ATTIBUTEIDX_ID bigint, ATTIBUTEIDX_NAME nvarchar(255), ATTIBUTEIDY_ID bigint, ATTIBUTEIDY_NAME nvarchar(255))

--sanity check, ensure @RevId and @WebDbId match...




	if @OnlyThisAttribute > 0
	BEGIN
		insert into @items select distinct ir.item_id, null, null, null from TB_ITEM_REVIEW ir
			inner join TB_ITEM_ATTRIBUTE tia on ir.ITEM_ID = tia.ITEM_ID and tia.ATTRIBUTE_ID = @OnlyThisAttribute 
				and ir.REVIEW_ID = @RevId and ir.IS_DELETED = 0
				and (@included is null OR ir.IS_INCLUDED = @included)  
			inner join TB_ITEM_SET tis on tia.ITEM_SET_ID = tis.ITEM_SET_ID and IS_COMPLETED = 1
	END
	ELSE
	Begin
		insert into @items select distinct ir.item_id, null, null, null from TB_ITEM_REVIEW ir
			where ir.REVIEW_ID = @RevId and ir.IS_DELETED = 0
			 and (@included is null OR ir.IS_INCLUDED = @included) 
	END


insert into @attsX select distinct a.Attribute_id, 
	 a.ATTRIBUTE_NAME
	 , ATTRIBUTE_ORDER, 0 from TB_ATTRIBUTE_SET tas
	 inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID

	 where tas.SET_ID = @setIdXAxis and PARENT_ATTRIBUTE_ID = @attributeIdXAxis
select ATTRIBUTE_ID, ATTRIBUTE_NAME from @attsX order by ord

IF @setIdYAxis > 0
BEGIN
	insert into @attsY select distinct a.Attribute_id, 
		 a.ATTRIBUTE_NAME
		 , ATTRIBUTE_ORDER, 0 from TB_ATTRIBUTE_SET tas
		 inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID 
		where SET_ID = @setIdYAxis and PARENT_ATTRIBUTE_ID = @attributeIdYAxis
	select ATTRIBUTE_ID, ATTRIBUTE_NAME from @attsY order by ord
END

-------------------------------------------------------
insert into @codeNames (SETIDX_ID, SETIDY_ID, ATTIBUTEIDX_ID, ATTIBUTEIDY_ID)
values (@setIdXAxis, @setIdYAxis, @attributeIdXAxis, @attributeIdYAxis)

update @codeNames set SETIDX_NAME = s.SET_NAME
										
	from TB_SET s
	inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @RevId and s.SET_ID = rs.SET_ID
	where s.SET_ID = SETIDX_ID

if @setIdYAxis != 0
begin
	update @codeNames set SETIDY_NAME = s.SET_NAME
	from TB_SET s
	inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @RevId and s.SET_ID = rs.SET_ID
	where s.SET_ID = SETIDY_ID
END
if @attributeIdXAxis != 0
begin
	update @codeNames set ATTIBUTEIDX_NAME = a.ATTRIBUTE_NAME
	from TB_ATTRIBUTE a
	where a.ATTRIBUTE_ID = ATTIBUTEIDX_ID
end
if @attributeIdYAxis != 0
begin
	update @codeNames set ATTIBUTEIDY_NAME = a.ATTRIBUTE_NAME
	from TB_ATTRIBUTE a
	where a.ATTRIBUTE_ID = ATTIBUTEIDY_ID
end
------------------------------------------------------------

If @SegmentsParent > 0
BEGIN
	insert into @segments select distinct a.Attribute_id,
		 a.ATTRIBUTE_NAME
		 , ATTRIBUTE_ORDER, 0 from TB_ATTRIBUTE_SET tas
		 inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID 
		where SET_ID = @setIdSegments and PARENT_ATTRIBUTE_ID = @SegmentsParent
	select ATTRIBUTE_ID, ATTRIBUTE_NAME from @segments order by ord
END

update @items  set X_atts = Atts
from 
(
	select STRING_AGG(cast (ia.ATTRIBUTE_ID as nvarchar(max)), ',') as Atts, i.itemId as iid from @items i 
	inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdXAxis
	inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.ITEM_ID = ia.ITEM_ID
	inner join @attsX a on ia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
	group by ItemId 
	--order by ItemId
) as big
WHERE ItemId = Big.iid

if @setIdYAxis > 0
	update @items set Y_atts = Atts
	from 
	(
		select STRING_AGG(cast (ia.ATTRIBUTE_ID as nvarchar(max)), ',') as Atts, i.itemId as iid from @items i 
		inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdYAxis
		inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.ITEM_ID = ia.ITEM_ID
		inner join @attsY a on ia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
		group by ItemId 
		--order by ItemId
	) as big
	WHERE ItemId = Big.iid

if @setIdSegments > 0
update @items set segments = Atts
from 
(
	select STRING_AGG(cast (ia.ATTRIBUTE_ID as nvarchar(max)), ',') as Atts, i.itemId as iid from @items i 
	inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdSegments
	inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.ITEM_ID = ia.ITEM_ID
	inner join @segments a on ia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
	group by ItemId 
	--order by ItemId
) as big
WHERE ItemId = Big.iid

select * from @items

select * from @codeNames
END

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ErItemListWithWithoutCodes]    Script Date: 03/06/2021 09:47:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER   PROCEDURE [dbo].[st_ErItemListWithWithoutCodes]
	@WithAttributesIds varchar(max)
    , @WithSetIdsList varchar(max)
	, @included bit = null
    , @WithOutAttributesIdsList varchar(max) = ''
    , @WithOutSetIdsList varchar(max) = ''
	, @RevId int 
      
AS

BEGIN

	--check input: need to be able to match Att and Set IDs...
	--see: https://arulmouzhi.wordpress.com/2020/01/13/counting-number-of-occurrences-of-a-particular-word-inside-the-string-using-t-sql/
	declare @commas int = (LEN(@WithAttributesIds) - LEN(REPLACE(@WithAttributesIds,',','')))
	declare @commas2 int = (LEN(@WithOutAttributesIdsList) - LEN(REPLACE(@WithOutAttributesIdsList,',','')))
	
	if @commas != (LEN(@WithSetIdsList) - LEN(REPLACE(@WithSetIdsList,',','')))
		OR
		@commas2 != (LEN(@WithOutSetIdsList) - LEN(REPLACE(@WithOutSetIdsList,',',''))) RETURN


declare @items table (ItemId bigint primary key, With_atts varchar(max) null, reject varchar(max) null)
declare @attsX table (ATTRIBUTE_ID bigint primary key, SET_ID int, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
declare @attsY table (ATTRIBUTE_ID bigint primary key, SET_ID int, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
declare @segments table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)


	insert into @items select distinct ir.item_id, null, null from TB_ITEM_REVIEW ir
		where ir.REVIEW_ID = @RevId and ir.IS_DELETED = 0
			and (@included is null OR ir.IS_INCLUDED = @included) 


insert into @attsX select distinct a.Attribute_id, ss.value, a.ATTRIBUTE_NAME, ATTRIBUTE_ORDER, 0 from 
	dbo.fn_Split_int(@WithAttributesIds,',') s
	inner join dbo.fn_Split_int(@WithSetIdsList,',') ss on s.idx = ss.idx and s.value > 0
	inner join TB_ATTRIBUTE_SET tas on s.value = tas.ATTRIBUTE_ID and ss.value = tas.SET_ID
	inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	 
select ATTRIBUTE_ID, ATTRIBUTE_NAME from @attsX order by ord



insert into @attsY select distinct a.Attribute_id, ss.value, a.ATTRIBUTE_NAME, ATTRIBUTE_ORDER, 0 from 
	dbo.fn_Split_int(@WithOutAttributesIdsList,',') s
	inner join dbo.fn_Split_int(@WithOutSetIdsList,',') ss on s.idx = ss.idx and s.value > 0
	inner join TB_ATTRIBUTE_SET tas on s.value = tas.ATTRIBUTE_ID and ss.value = tas.SET_ID
	inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID 

select ATTRIBUTE_ID, ATTRIBUTE_NAME from @attsY order by ord



update @items  set With_atts = Atts
from 
(
	select STRING_AGG(cast (ia.ATTRIBUTE_ID as nvarchar(max)), ',') as Atts, i.itemId as iid from @items i 
	inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 
	inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.ITEM_ID = ia.ITEM_ID
	inner join @attsX a on ia.ATTRIBUTE_ID = a.ATTRIBUTE_ID and tis.SET_ID = a.SET_ID
	group by ItemId 
	--order by ItemId
) as big
WHERE ItemId = Big.iid

if @WithOutAttributesIdsList != ''
	update @items set reject = 1
	from 
	(
		select STRING_AGG(cast (ia.ATTRIBUTE_ID as nvarchar(max)), ',') as Atts, i.itemId as iid from @items i 
		inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.ITEM_ID = ia.ITEM_ID
		inner join @attsY a on ia.ATTRIBUTE_ID = a.ATTRIBUTE_ID and tis.SET_ID = a.SET_ID
		group by ItemId 
		--order by ItemId
	) as big
	WHERE ItemId = Big.iid



select ii.*, i.SHORT_TITLE from @items ii
	inner join tb_item i on ii.ItemId = i.ITEM_ID
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbItemListFromIDs]    Script Date: 03/06/2021 10:31:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER   PROCEDURE [dbo].[st_ErItemListFromIDs]
	@RevId int
    , @Items varchar(max)
as 
BEGIN
	Select II.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, II.CREATED_BY, DATE_EDITED, II.EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , II.MASTER_ITEM_ID, DOI, KEYWORDS
                  
            FROM dbo.fn_Split_int(@Items, ',')  SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.value = II.ITEM_ID
				  inner join TB_ITEM_REVIEW ir on II.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @RevId
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
			order by SHORT_TITLE, II.ITEM_ID
END

GO