USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SearchForItemsWithDuplicateReferences]    Script Date: 01/08/2022 16:28:56 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO



ALTER     PROCEDURE [dbo].[st_SearchForItemsWithDuplicateReferences]
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

	  --Two: get current dups
	  INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
			SELECT distinct MASTER_ITEM_ID, @SEARCH_ID, NULL from TB_ITEM_REVIEW 
			where REVIEW_ID = @REVIEW_ID and MASTER_ITEM_ID is not null and IS_DELETED = 1 and IS_INCLUDED = 1
      
      -- Step Three: Update the new search record in tb_SEARCH with the number of records added
      UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID

GO
