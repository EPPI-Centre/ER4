USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_SearchCombine]    Script Date: 20/08/2026 09:37:58 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[st_SearchCombine] (
	@SEARCH_ID int = null output
,	@CONTACT_ID nvarchar(50) = null
,	@REVIEW_ID INT
,	@SEARCH_TITLE varchar(4000) = null
,	@SEARCHES varchar(MAX) = null
,	@COMBINE_TYPE nvarchar(10)
,	@INCLUDED BIT = NULL
)
AS

EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, @SEARCHES, '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

IF (@COMBINE_TYPE = 'OR')
BEGIN
	/*
	James changed: 14 Feb 2011 as previous code was entering duplicate values if different ranks had been identified
	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
		SELECT DISTINCT  ITEM_ID, @SEARCH_ID, ITEM_RANK FROM dbo.fn_Split_int
		(@SEARCHES, ',') JOIN tb_SEARCH_ITEM ON value = SEARCH_ID
	*/
	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
		SELECT DISTINCT  ITEM_ID, @SEARCH_ID, 0 FROM dbo.fn_Split_int
		(@SEARCHES, ',') JOIN tb_SEARCH_ITEM ON value = SEARCH_ID
END
ELSE
	IF (@COMBINE_TYPE = 'NOT')
	BEGIN
	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
		SELECT  ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW WHERE REVIEW_ID = @REVIEW_ID 
			AND IS_DELETED != 'true' AND IS_INCLUDED = @INCLUDED
		EXCEPT
		SELECT DISTINCT ITEM_ID, @SEARCH_ID FROM		
		 dbo.fn_Split_int
		(@SEARCHES, ',') JOIN tb_SEARCH_ITEM ON value = SEARCH_ID
	END
	ELSE
		IF (@COMBINE_TYPE = 'AND')
		BEGIN
		DECLARE @NUM_RECORDS INT
		DECLARE @DUMMY INT
		SELECT @NUM_RECORDS = COUNT(VALUE) FROM dbo.fn_Split_int (@SEARCHES, ',')

		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
			SELECT DISTINCT  ITEM_ID, @SEARCH_ID FROM
			dbo.fn_Split_int (@SEARCHES, ',') JOIN tb_SEARCH_ITEM ON value = SEARCH_ID
			GROUP BY ITEM_ID
			HAVING COUNT(ITEM_ID) = @NUM_RECORDS
		

		END

	UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID

	declare @searchCount int = 0
	Select @searchCount = (len(@SEARCHES) - len(replace(@SEARCHES,',','')))
	--big part: combine 2 searches by also combining their RANK values
	if (@COMBINE_TYPE = 'AND' and @searchCount = 1)
		begin
			declare @row1 int
			declare @row2 int
			select @row1 = (select top 1 value from dbo.fn_Split_int(@SEARCHES, ',') ORDER BY value Asc);
			select @row2 = (select top 1 value from dbo.fn_Split_int(@SEARCHES, ',') ORDER BY value Desc);
			--we're combining two searches but do we have ranking values?
			declare @Search1HasRANK bit 
			IF (Select MAX(ITEM_RANK) from TB_SEARCH_ITEM where SEARCH_ID = @row1) > 0
				set @Search1HasRANK = 1 ELSE set @Search1HasRANK = 0
			declare @Search2HasRANK bit
			IF (Select MAX(ITEM_RANK) from TB_SEARCH_ITEM where SEARCH_ID = @row2) > 0
				set @Search2HasRANK = 1 ELSE set @Search2HasRANK = 0
			--now we know which searches have a rank, 3 cases: both - multiply RANKS; only one, multiply rank, consider the other to be 100
			-- final case, do nothing, no ranks to work on.
			if (@Search1HasRANK = 1 AND @Search2HasRANK = 1)
			BEGIN
				update si
					SET si.ITEM_RANK = ROUND(si1.item_rank * si2.item_rank / 100, 0)
					from TB_SEARCH_ITEM si
						inner join TB_SEARCH_ITEM si1 on si1.ITEM_ID = si.ITEM_ID and si1.SEARCH_ID = @row1
						inner join TB_SEARCH_ITEM si2 on si2.ITEM_ID = si.ITEM_ID and si2.SEARCH_ID = @row2
					where si.SEARCH_ID = @SEARCH_ID
			END
			ELSE IF (@Search1HasRANK = 0 AND @Search2HasRANK = 1)
			BEGIN
				update si
					SET si.ITEM_RANK = si2.item_rank -- would be 100[si1 default rank] * si2.item_rank /100 = si2.item_rank
					from TB_SEARCH_ITEM si
						inner join TB_SEARCH_ITEM si1 on si1.ITEM_ID = si.ITEM_ID and si1.SEARCH_ID = @row1
						inner join TB_SEARCH_ITEM si2 on si2.ITEM_ID = si.ITEM_ID and si2.SEARCH_ID = @row2
					where si.SEARCH_ID = @SEARCH_ID
			END
			ELSE IF (@Search1HasRANK = 1 AND @Search2HasRANK = 0)
			BEGIN
				update si
					SET si.ITEM_RANK = si1.item_rank -- would be 100[si2 default rank] * si1.item_rank /100 = si1.item_rank
					from TB_SEARCH_ITEM si
						inner join TB_SEARCH_ITEM si1 on si1.ITEM_ID = si.ITEM_ID and si1.SEARCH_ID = @row1
						inner join TB_SEARCH_ITEM si2 on si2.ITEM_ID = si.ITEM_ID and si2.SEARCH_ID = @row2
					where si.SEARCH_ID = @SEARCH_ID
			END
			--NO third case, we have nothing to update.
			--FINALLY: do we set the IS_CLASSIFIER_RESULT to TRUE?

			-- added by Jeff 19/08/2026 to fix bug			
			declare @IsSearch1Classifier bit = 0
			declare @IsSearch2Classifier bit = 0
			set @IsSearch1Classifier = (select IS_CLASSIFIER_RESULT from TB_SEARCH where SEARCH_ID = @row1)
			set @IsSearch2Classifier = (select IS_CLASSIFIER_RESULT from TB_SEARCH where SEARCH_ID = @row2)
			if ((@IsSearch1Classifier = 1) and (@IsSearch2Classifier = 1))
			begin
				UPDATE TB_SEARCH SET IS_CLASSIFIER_RESULT = 'TRUE' WHERE SEARCH_ID = @SEARCH_ID
			end
			
			/*
			DECLARE @Max int, @Min int
			select @Max = MAX(ITEM_RANK), @Min = MIN(ITEM_RANK) from TB_SEARCH_ITEM where SEARCH_ID = @SEARCH_ID
			IF (Select IS_CLASSIFIER_RESULT from TB_SEARCH where SEARCH_ID = @row1) = 1
				set @Search1HasRANK = 1 ELSE set @Search1HasRANK = 0
			IF (Select IS_CLASSIFIER_RESULT from TB_SEARCH where SEARCH_ID = @row2) = 1
				set @Search2HasRANK = 1 ELSE set @Search2HasRANK = 0
			IF (
				(@Search1HasRANK = 1 and @Search2HasRANK = 1) --both searches are ranked searches
				OR
				(@Max <= 100 AND @Min >= 0) --can "visualise" distribution
				)
			BEGIN
				UPDATE TB_SEARCH SET IS_CLASSIFIER_RESULT = 'TRUE' WHERE SEARCH_ID = @SEARCH_ID
			END
			*/

		END
		
GO

