USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SearchFreeText]    Script Date: 01/12/2020 09:30:00 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE or ALTER PROCEDURE [dbo].[st_WebDbYearFrequency]
(	
		@RevId int
		, @WebDbId int
		, @Included bit = null
)
AS
BEGIN
	--sanity check, ensure @RevId and @WebDbId match...
	Declare @CheckWebDbId int = null
	set @CheckWebDbId = (select WEBDB_ID from TB_WEBDB where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	IF @CheckWebDbId is null return;

	--declare @items table (ItemId bigint, year nchar(4), primary key (ItemId, year))
	declare @WebDbFilter bigint = (select w.WITH_ATTRIBUTE_ID from TB_WEBDB w where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	if (@WebDbFilter is not null and @WebDbFilter > 0)
	Begin 
		--insert into @items 
		select count(distinct i.item_id) AS [Count], year from TB_ITEM i
			inner join TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 0  
			and (ir.IS_INCLUDED = @Included OR @Included is null) and ir.REVIEW_ID = @RevId
			inner join TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = i.ITEM_ID and tia.ATTRIBUTE_ID = @WebDbFilter
			inner join TB_ITEM_SET tis2 on tia.ITEM_SET_ID = tis2.ITEM_SET_ID and tis2.IS_COMPLETED = 1 
			group by YEAR order by YEAR
	end
	else 
	begin
		--insert into @items 
		select count(distinct i.item_id) AS [Count], year from TB_ITEM i
			inner join TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 0  
			and (ir.IS_INCLUDED = @Included OR @Included is null) and ir.REVIEW_ID = @RevId
			group by YEAR order by YEAR
	end
END
GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SearchFreeText]    Script Date: 01/12/2020 09:30:00 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE or ALTER PROCEDURE [dbo].[st_WebDbSearchFreeText]
(
     @RevId int 
    ,@WebDbId int
	,@SEARCH_TEXT varchar(4000) 
	,@SEARCH_WHAT nvarchar(20) 
	,@INCLUDED BIT = NULL -- 'INCLUDED', 'EXCLUDED' or BOTH


	
    , @PageNum INT = 1
    , @PerPage INT = 100
    , @CurrentPage INT OUTPUT
    , @TotalPages INT OUTPUT
    , @TotalRows INT OUTPUT  
)

AS
    Declare @CheckWebDbId int = null
	set @CheckWebDbId = (select WEBDB_ID from TB_WEBDB where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	IF @CheckWebDbId is null return;

	declare @items table (ItemId bigint, found bit, primary key(ItemId, found))
	declare @WebDbFilter bigint = (select w.WITH_ATTRIBUTE_ID from TB_WEBDB w where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	if (@WebDbFilter is not null and @WebDbFilter > 0)
	Begin 
		insert into @items 
		select distinct i.item_id, 0 from TB_ITEM i
			inner join TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 0  
			and (ir.IS_INCLUDED = @Included OR @Included is null) and ir.REVIEW_ID = @RevId
			inner join TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = i.ITEM_ID and tia.ATTRIBUTE_ID = @WebDbFilter
			inner join TB_ITEM_SET tis2 on tia.ITEM_SET_ID = tis2.ITEM_SET_ID and tis2.IS_COMPLETED = 1 
	end
	else 
	begin
		insert into @items 
		select distinct i.item_id, 0 from TB_ITEM i
			inner join TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 0  
			and (ir.IS_INCLUDED = @Included OR @Included is null) and ir.REVIEW_ID = @RevId
	end

    IF (@SEARCH_WHAT = 'TitleAbstract')
      BEGIN
			UPDATE @items set found = 1 from @items i 
            INNER JOIN CONTAINSTABLE(TB_ITEM, (TITLE, ABSTRACT), @SEARCH_TEXT) AS KEY_TBL ON KEY_TBL.[KEY] = i.ItemId
      END
    ELSE IF (@SEARCH_WHAT = 'Title')
      BEGIN
            UPDATE @items set found = 1 from @items i
			INNER JOIN CONTAINSTABLE(TB_ITEM, (TITLE), @SEARCH_TEXT) AS KEY_TBL ON KEY_TBL.[KEY] = i.ItemId
      END
    ELSE IF (@SEARCH_WHAT = 'Abstract')
      BEGIN
            UPDATE @items set found = 1 from @items i
			INNER JOIN CONTAINSTABLE(TB_ITEM, (ABSTRACT), @SEARCH_TEXT) AS KEY_TBL ON KEY_TBL.[KEY] = i.ItemId
      END
    ELSE IF (@SEARCH_WHAT = 'PubYear')
      BEGIN
		declare @len int = Len(@SEARCH_TEXT)
		if @len = 4
            UPDATE @items set found = 1 from @items i
			INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = i.ItemId
            WHERE TB_ITEM.[YEAR] =  @SEARCH_TEXT
		else if @SEARCH_TEXT = 'Unknown' OR @len = 0
			UPDATE @items set found = 1 from @items i
			INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = i.ItemId
            WHERE TB_ITEM.[YEAR] =  '    ' OR TB_ITEM.[YEAR] ='0   ' OR TB_ITEM.[YEAR] =' 0  ' OR TB_ITEM.[YEAR] ='  0 ' OR TB_ITEM.[YEAR] ='   0'
		else if @len = 2
			UPDATE @items set found = 1 from @items i
			INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = i.ItemId
				WHERE TB_ITEM.[YEAR] = @SEARCH_WHAT + '  ' 
				OR TB_ITEM.[YEAR] =' ' + @SEARCH_WHAT + ' ' 
				OR TB_ITEM.[YEAR] ='  ' + @SEARCH_WHAT
		else if @len = 3
			UPDATE @items set found = 1 from @items i
			INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = i.ItemId
				WHERE TB_ITEM.[YEAR] = @SEARCH_WHAT + ' ' 
				OR TB_ITEM.[YEAR] =' ' + @SEARCH_WHAT
		else if @len = 1
			UPDATE @items set found = 1 from @items i
			INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = i.ItemId
				WHERE TB_ITEM.[YEAR] = @SEARCH_WHAT + '   ' 
				OR TB_ITEM.[YEAR] =' ' + @SEARCH_WHAT + '  ' 
				OR TB_ITEM.[YEAR] ='  ' + @SEARCH_WHAT + ' ' 
				OR TB_ITEM.[YEAR] ='   ' + @SEARCH_WHAT
      END
    ELSE IF (@SEARCH_WHAT = 'AdditionalText')
      BEGIN
            UPDATE @items set found = 1 from @items i
			INNER JOIN TB_ITEM_SET tis ON tis.ITEM_ID = i.ItemId AND tis.IS_COMPLETED = 1
			INNER JOIN TB_ITEM_ATTRIBUTE tia ON tis.ITEM_SET_ID = tia.ITEM_SET_ID
			inner JOIN TB_WEBDB_PUBLIC_ATTRIBUTE pa on pa.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
			INNER JOIN CONTAINSTABLE(TB_ITEM_ATTRIBUTE, ADDITIONAL_TEXT, @SEARCH_TEXT) AS k
                  ON k.[KEY] =  tia.ITEM_ATTRIBUTE_ID 
      END
    ELSE IF (@SEARCH_WHAT = 'ItemId')
      BEGIN
			UPDATE @items set found = 1 from @items i
			INNER JOIN dbo.fn_Split(@SEARCH_TEXT, ',') id ON id.value = cast(i.ItemId as varchar(8000))
      END
	ELSE IF (@SEARCH_WHAT = 'OldItemId')
      BEGIN
            UPDATE @items set found = 1 from @items i
			inner join tb_item ii on i.ItemId = ii.ITEM_ID
			INNER JOIN dbo.fn_Split(@SEARCH_TEXT, ',') id ON id.value = ii.OLD_ITEM_ID
      END
    ELSE IF (@SEARCH_WHAT = 'Authors')
      BEGIN
            UPDATE @items set found = 1 from @items i
			inner join TB_ITEM_AUTHOR ia on i.ItemId = ia.ITEM_ID
				and (
					dbo.fn_REBUILD_AUTHORS(i.ItemId, 0) like '%'+ @SEARCH_TEXT +'%'
				)
      END

	--get results!
	declare @RowsToRetrieve int
	SELECT @TotalRows = count(ItemId) from @items where found = 1
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
			TB_ITEM_REVIEW.REVIEW_ID = @RevId
		INNER JOIN @items ii on I.ITEM_ID = ii.ItemId and found = 1
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
      
GO      

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemList]    Script Date: 02/12/2020 16:22:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemList]
(
      @REVIEW_ID INT,
      @SHOW_INCLUDED BIT = 'true',
      @SHOW_DELETED BIT = 'false',
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
            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
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

      WHERE I.IS_INCLUDED = @SHOW_INCLUDED
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
            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
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
	  INSERT INTO @ID SELECT DISTINCT IR.ITEM_ID
		from TB_ITEM_REVIEW IR 
      LEFT OUTER JOIN TB_ITEM_SOURCE TIS on IR.ITEM_ID = TIS.ITEM_ID
      LEFT OUTER JOIN TB_SOURCE TS on TIS.SOURCE_ID = TS.SOURCE_ID and IR.REVIEW_ID = TS.REVIEW_ID
      where IR.REVIEW_ID = @REVIEW_ID and TS.SOURCE_ID  is null

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