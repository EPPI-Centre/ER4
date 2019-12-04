USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSearchList]    Script Date: 12/4/2019 12:57:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemSearchList] (
      @REVIEW_ID INT,
      @SEARCH_ID INT,
      
      @PageNum INT = 1,
      @PerPage INT = 3,
      @CurrentPage INT OUTPUT,
      @TotalPages INT OUTPUT,
      @TotalRows INT OUTPUT
)

As

SET NOCOUNT ON

declare @RowsToRetrieve int

      SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
      FROM TB_ITEM_REVIEW I
      INNER JOIN TB_SEARCH_ITEM ON TB_SEARCH_ITEM.ITEM_ID = I.ITEM_ID
      AND TB_SEARCH_ITEM.SEARCH_ID = @SEARCH_ID
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
      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, ITEM_RANK, TB_ITEM_REVIEW.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by ITEM_RANK desc) RowNum
      FROM TB_ITEM I
            INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
                  TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            INNER JOIN TB_SEARCH_ITEM ON TB_SEARCH_ITEM.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
                  AND TB_SEARCH_ITEM.SEARCH_ID = @SEARCH_ID
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  ,SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS, ITEM_RANK
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve
                  ORDER BY ITEM_RANK desc
                  
      OPTION (OPTIMIZE FOR (@PerPage=700, @SEARCH_ID UNKNOWN, @REVIEW_ID UNKNOWN))
SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'


SET NOCOUNT OFF

GO