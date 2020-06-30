USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[stSearchClassifierScores]    Script Date: 13/06/2020 18:18:20 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
create or ALTER PROCEDURE [dbo].[stSearchClassifierScores]
(
	@SEARCH_ID int = null output
,	@CONTACT_ID nvarchar(50) = null
,	@REVIEW_ID nvarchar(50) = null
,	@SEARCH_TYPE nvarchar(10) = null
,	@SEARCH_TITLE varchar(4000) = null
,	@SCORE1 int = null
,	@SCORE2 int = null
,	@ORIGINAL_SEARCH_ID int

)
AS
	-- Step One: Insert record into tb_SEARCH
	EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, '', '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

	-- Step Two: Perform the search and get a hits count
	   	
	if @SEARCH_TYPE = 'Less'
	BEGIN
		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
			SELECT DISTINCT  I.ITEM_ID, @SEARCH_ID, ITEM_RANK FROM TB_SEARCH_ITEM I
					where I.ITEM_RANK < @SCORE1 and i.SEARCH_ID = @ORIGINAL_SEARCH_ID

	END
	ELSE IF @SEARCH_TYPE = 'More'
	BEGIN
		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
			SELECT DISTINCT  I.ITEM_ID, @SEARCH_ID, ITEM_RANK FROM TB_SEARCH_ITEM I
				where I.ITEM_RANK > @SCORE1 and i.SEARCH_ID = @ORIGINAL_SEARCH_ID
	END
	ELSE IF @SEARCH_TYPE = 'Between'
	BEGIN
		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
			SELECT DISTINCT  I.ITEM_ID, @SEARCH_ID, ITEM_RANK FROM TB_SEARCH_ITEM I
				where i.ITEM_RANK >= @SCORE1 and i.ITEM_RANK <= @SCORE2 and i.SEARCH_ID = @ORIGINAL_SEARCH_ID
	END

	-- Step Three: Update the new search record in tb_SEARCH with the number of records added
	UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID

GO