USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_SearchForItemsWithLinkedReferences]    Script Date: 10/02/2022 13:24:59 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[st_SearchForItemsWithLinkedReferences]
(
      @SEARCH_ID int = null output
,     @CONTACT_ID nvarchar(50) = null
,     @REVIEW_ID nvarchar(50) = null
,     @SEARCH_TITLE varchar(4000) = null
,     @INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'
,     @SEARCH_ITEM_ID BIGINT = NULL

)
AS
      -- Step One: Insert record into tb_SEARCH
      EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, '', '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

      -- Step Two: Perform the search and get a hits count     
		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID, 1 FROM TB_ITEM_REVIEW
            INNER JOIN TB_ITEM_LINK ON TB_ITEM_LINK.ITEM_ID_PRIMARY = TB_ITEM_REVIEW.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      
      
      -- Step Three: Update the new search record in tb_SEARCH with the number of records added
      UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID
GO

