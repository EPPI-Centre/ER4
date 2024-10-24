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
,	@CONTACT_ID nvarchar(50) = null
,	@REVIEW_ID nvarchar(50) = null
,	@SEARCH_TITLE varchar(4000) = null
,	@INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'
,	@DELETED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'
,	@DUPLICATES BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'
,	@SOURCE_IDs varchar(4000) = null
)
AS
	-- Step One: Insert record into tb_SEARCH
	EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, @SOURCE_IDs, '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

	-- Step Two: Perform the search and get a hits count
	-- NB: We're using a udf to split the string of answer id's into a table, joining this with the tb_EXTRACT_ATTR (and any others that are required)
	-- to perform the insert.  @ANSWERS should be passed in as 'AT10225, AT10226' (with a comma and a space separating each id)
	
	IF (@DUPLICATES='0')
		BEGIN
		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
		SELECT DISTINCT  ir.ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW ir
			INNER JOIN TB_ITEM i on ir.ITEM_ID = i.ITEM_ID
			INNER JOIN TB_ITEM_SOURCE its on its.ITEM_ID = ir.ITEM_ID
			INNER JOIN dbo.fn_Split_int(@SOURCE_IDs, ',') id ON id.value = its.SOURCE_ID
			WHERE ir.REVIEW_ID = @REVIEW_ID
			AND ir.IS_DELETED = @DELETED
			AND ir.IS_INCLUDED = @INCLUDED
			AND ir.MASTER_ITEM_ID IS NULL
		END
	ELSE
		BEGIN
		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
		SELECT DISTINCT  ir.ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW ir
			INNER JOIN TB_ITEM i on ir.ITEM_ID = i.ITEM_ID
			INNER JOIN TB_ITEM_SOURCE its on its.ITEM_ID = ir.ITEM_ID
			INNER JOIN dbo.fn_Split_int(@SOURCE_IDs, ',') id ON id.value = its.SOURCE_ID
			WHERE ir.REVIEW_ID = @REVIEW_ID
			AND ir.IS_DELETED = @DELETED
			AND ir.IS_INCLUDED = @INCLUDED
			AND ir.MASTER_ITEM_ID IS NOT NULL
		END
		
	
	-- Step Three: Update the new search record in tb_SEARCH with the number of records added
	UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID
GO