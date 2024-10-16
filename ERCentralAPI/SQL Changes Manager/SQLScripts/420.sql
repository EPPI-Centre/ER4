USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SearchSources]    Script Date: 18/07/2021 16:38:08 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[st_SearchSources]
(
     @SEARCH_ID int = null output
,    @CONTACT_ID nvarchar(50) = null
,    @REVIEW_ID nvarchar(50) = null
,    @SEARCH_TITLE varchar(4000) = null
,    @SEARCH_WHAT varchar(20)
,    @SOURCE_IDs varchar(4000) = null
)
AS

-- Step One: Insert record into tb_SEARCH
EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, @SOURCE_IDs, '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

IF (@SEARCH_WHAT = 'AllItems')
BEGIN
		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
			SELECT DISTINCT  ir.ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW ir
			INNER JOIN TB_ITEM i on ir.ITEM_ID = i.ITEM_ID
			INNER JOIN TB_ITEM_SOURCE its on its.ITEM_ID = ir.ITEM_ID
			INNER JOIN dbo.fn_Split_int(@SOURCE_IDs, ',') id ON id.value = its.SOURCE_ID
			WHERE ir.REVIEW_ID = @REVIEW_ID

END
ELSE IF (@SEARCH_WHAT = 'Included')
BEGIN
	   INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
			SELECT DISTINCT  ir.ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW ir
			INNER JOIN TB_ITEM i on ir.ITEM_ID = i.ITEM_ID
			INNER JOIN TB_ITEM_SOURCE its on its.ITEM_ID = ir.ITEM_ID
			INNER JOIN dbo.fn_Split_int(@SOURCE_IDs, ',') id ON id.value = its.SOURCE_ID
			WHERE ir.REVIEW_ID = @REVIEW_ID
			AND ir.IS_DELETED = '0'
			AND ir.IS_INCLUDED = '1'
END
ELSE IF (@SEARCH_WHAT = 'Excluded')
BEGIN
	   INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
			SELECT DISTINCT  ir.ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW ir
			INNER JOIN TB_ITEM i on ir.ITEM_ID = i.ITEM_ID
			INNER JOIN TB_ITEM_SOURCE its on its.ITEM_ID = ir.ITEM_ID
			INNER JOIN dbo.fn_Split_int(@SOURCE_IDs, ',') id ON id.value = its.SOURCE_ID
			WHERE ir.REVIEW_ID = @REVIEW_ID
			AND ir.IS_DELETED = '0'
			AND ir.IS_INCLUDED = '0'
END
ELSE IF (@SEARCH_WHAT = 'Deleted')
BEGIN
	   INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
			SELECT DISTINCT  ir.ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW ir
			INNER JOIN TB_ITEM i on ir.ITEM_ID = i.ITEM_ID
			INNER JOIN TB_ITEM_SOURCE its on its.ITEM_ID = ir.ITEM_ID
			INNER JOIN dbo.fn_Split_int(@SOURCE_IDs, ',') id ON id.value = its.SOURCE_ID
			WHERE ir.REVIEW_ID = @REVIEW_ID
			AND ir.IS_DELETED = '1'
			AND ir.MASTER_ITEM_ID IS NULL
END
ELSE IF (@SEARCH_WHAT = 'Duplicates')
BEGIN
		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
			SELECT DISTINCT  ir.ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW ir
			INNER JOIN TB_ITEM i on ir.ITEM_ID = i.ITEM_ID
			INNER JOIN TB_ITEM_SOURCE its on its.ITEM_ID = ir.ITEM_ID
			INNER JOIN dbo.fn_Split_int(@SOURCE_IDs, ',') id ON id.value = its.SOURCE_ID
			WHERE ir.REVIEW_ID = @REVIEW_ID
			AND ir.IS_DELETED = '1'
			AND ir.MASTER_ITEM_ID IS NOT NULL
END
		
	
-- Step Three: Update the new search record in tb_SEARCH with the number of records added
UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID

GO