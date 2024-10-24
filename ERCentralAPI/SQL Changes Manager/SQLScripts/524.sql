USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ScreeningCreateNonMLList]    Script Date: 02/02/2023 10:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ScreeningCreateNonMLList]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@WHAT_ATTRIBUTE_ID BIGINT,
	@SCREENING_MODE nvarchar(10),
	@CODE_SET_ID INT,
	@TRIGGERING_ITEM_ID BIGINT = 0
)

As

SET NOCOUNT ON

	-- ***** PRELIMINARY CHECK (SG Feb 2023): if we received an ITEM_ID val, it's because the training was triggered automatically
	-- in which case we MIGHT not need to rebuild the list, because there is no point in re-shuffling a random list very often
	-- this check can produce 3 effects: 
	-- (1) do nothing (return immediately) when the "triggering item" has already triggered one of the other 2 outcomes
	-- We check for (1) by looking if this item has been coded already by other people, in which case it has (probably) been served by the PS list already
	-- It has been locked to the Other person, and lock has been released when that other person has screened the item.
	-- (2) Create a new record (with updated numbers!) in TB_TRAINING, but without changing the current list. 
	-- This is useful to ensure multiple coding remains efficient: serving the same Items to the required N of reviewers, can't happen if we scramble the items order all the time!
	-- (3) Rebuild the whole list
	DECLARE @shouldRebuild bit = 0
	DECLARE @LAST_TRAINING_ID INT = (select MAX(t.TRAINING_ID) FROM TB_TRAINING t 
									inner join TB_TRAINING_ITEM ti on t.REVIEW_ID = @REVIEW_ID and t.TRAINING_ID = ti.TRAINING_ID
									)
	--Nothing to check if @TRIGGERING_ITEM_ID = 0 as this means somebody ASKED to recreate the list, and thus we should just do that!
	if @TRIGGERING_ITEM_ID = 0 set @shouldRebuild = 1
	ELSE
	BEGIN
		--Check for case (1): is this item ALREADY coded by someone else? If it is, it has been "shown" to that other people already, so it almost certainly has already triggered this SP: we do NOT want
		DECLARE @codedCount int = (select count(ITEM_SET_ID) from tb_item_set where SET_ID = @CODE_SET_ID and ITEM_ID = @TRIGGERING_ITEM_ID and CONTACT_ID != @CONTACT_ID)
		
		-- ***** If this item is coded by someone else: RETURN
		if (@codedCount is not null AND @codedCount > 0) RETURN
	
		--Check for case (2) or (3)
		declare @TotInList int = (select count (TRAINING_ITEM_ID) from TB_TRAINING_ITEM where TRAINING_ID = @LAST_TRAINING_ID)
		declare @highestToDo int = (select RANK from TB_TRAINING_ITEM where ITEM_ID = @TRIGGERING_ITEM_ID and TRAINING_ID = @LAST_TRAINING_ID)
		IF @highestToDo is null --very odd, should not happen, unless we don't have a list to replace!!
				OR (
					@highestToDo >= 1000 -- next item for the current user is 1000 items down the list, enough already!
					OR @highestToDo >= (@TotInList / 2) --we're past half of the list
					)
				--OK we SHOULD rebuild
				set @shouldRebuild = 1
	END

	DECLARE @NEW_TRAINING_ID INT
	DECLARE @TP INT
	DECLARE @TN INT

	-- ***** FIRST, GET THE STATS IN TERMS OF # ITEMS SCREENED TO POPULATE THE TRIANING TABLE (GIVES US THE GRAPH ON THE SCREENING TAB)
	SELECT @TP = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 10
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

	SELECT @TN = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 11
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID


	-- ******** SECOND, ENTER A NEW LINE IN THE TRAINING TABLE ***************
	DECLARE @CURRENT_ITERATION INT
	
	SELECT @CURRENT_ITERATION = MAX(ITERATION) + 1 FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID

	IF (@CURRENT_ITERATION IS NULL)
	BEGIN
		SET @CURRENT_ITERATION = 1
	END

	INSERT INTO TB_TRAINING(REVIEW_ID, CONTACT_ID, TIME_STARTED, TIME_ENDED, TRUE_POSITIVES, TRUE_NEGATIVES, ITERATION)
		VALUES (@REVIEW_ID, @CONTACT_ID, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, @TP, @TN, @CURRENT_ITERATION)
	   
	SET @NEW_TRAINING_ID = @@IDENTITY

	-- ********** THIRD, ENTER THE LIST OF ITEMS INTO TB_TRAINING_ITEM ACCORDING TO WHETHER WE'RE FILTERING BY AN ATTRIBUTE OR DOING THE WHOLE REVIEW
	-- ********** NEW [SG Feb 2023] we also check if @shouldRebuild: if so, old code as usual,
	-- ********** OTHERWISE we simply UPDATE the TRAINING_ID in tb_TRAINING_ITEM
	IF @shouldRebuild = 1
	BEGIN
		IF @WHAT_ATTRIBUTE_ID > 0  -- i.e. we're filtering by a code
		BEGIN
			IF @SCREENING_MODE = 'Random' -- FILTERING BY A CODE AND ORDERING AT RANDOM
			BEGIN
				INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK])
				SELECT @NEW_TRAINING_ID, TB_ITEM_REVIEW.ITEM_ID, 0, 0 FROM TB_ITEM_REVIEW
				INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @WHAT_ATTRIBUTE_ID
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
				WHERE REVIEW_ID = @REVIEW_ID AND IS_INCLUDED = 'TRUE' AND IS_DELETED = 'FALSE' AND NOT TB_ITEM_REVIEW.ITEM_ID IN
					(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID)
				ORDER BY NEWID()
			END
			ELSE -- FILTERING BY A CODE, BUT ORDERING BY THE VALUE PUT IN THE ADDITIONAL_TEXT FIELD
			BEGIN
				INSERT INTO TB_TRAINING_ITEM([RANK], TRAINING_ID, ITEM_ID, CONTACT_ID_CODING)
				SELECT CASE WHEN ISNUMERIC(TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT)=1 THEN CAST(TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT AS INT) ELSE 0 END,
					@NEW_TRAINING_ID, TB_ITEM_REVIEW.ITEM_ID, 0
				FROM TB_ITEM_REVIEW
				INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @WHAT_ATTRIBUTE_ID
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
				WHERE REVIEW_ID = @REVIEW_ID AND IS_INCLUDED = 'TRUE' AND IS_DELETED = 'FALSE' AND NOT TB_ITEM_REVIEW.ITEM_ID IN
					(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID)
				ORDER BY TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT
			END
		END
		ELSE -- NOT FILTERING BY A CODE, SO EVERYTHING IN THE REVIEW THAT'S INCLUDED AND SO FAR UNCODED IS INCLUDED
		BEGIN
			INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK])
			SELECT @NEW_TRAINING_ID, ITEM_ID, 0, 0 FROM TB_ITEM_REVIEW
				WHERE REVIEW_ID = @REVIEW_ID AND IS_INCLUDED = 'TRUE' AND IS_DELETED = 'FALSE' AND NOT ITEM_ID IN
					(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID)
				ORDER BY NEWID()
		END
		/* SET THE RANKS TO INCREMENT */
		DECLARE @START_INDEX INT = 0
		SELECT @START_INDEX = MIN(TRAINING_ITEM_ID) FROM TB_TRAINING_ITEM WHERE TRAINING_ID = @NEW_TRAINING_ID
		UPDATE TB_TRAINING_ITEM
			SET [RANK] = TRAINING_ITEM_ID - @START_INDEX + 1
			WHERE TRAINING_ID = @NEW_TRAINING_ID

		-- FINALLY, MIGRATE ANY NON-STALE CODING LOCKS FROM THE PREVIOUS TRAINING RUN
		UPDATE A
			SET A.CONTACT_ID_CODING = B.CONTACT_ID_CODING,
			A.WHEN_LOCKED = B.WHEN_LOCKED
			FROM TB_TRAINING_ITEM A
			JOIN
			TB_TRAINING_ITEM B ON A.ITEM_ID = B.ITEM_ID AND CURRENT_TIMESTAMP < DATEADD(DAY, 7, B.WHEN_LOCKED) AND
				B.TRAINING_ID = @LAST_TRAINING_ID
			WHERE A.TRAINING_ID = @NEW_TRAINING_ID

	

		-- delete the old list(s) of items to screen for this review
		DELETE TI
		FROM TB_TRAINING_ITEM TI
		INNER JOIN TB_TRAINING T ON T.TRAINING_ID = TI.TRAINING_ID
		WHERE T.REVIEW_ID = @REVIEW_ID AND T.TRAINING_ID < @NEW_TRAINING_ID

		END
	ELSE
	BEGIN
		--@shouldRebuild = 0 so we don't scramble the list, just "link" the new TB_TRAINING record to what's already in TB_TRAINING_ITEM
		UPDATE TB_TRAINING_ITEM set TRAINING_ID = @NEW_TRAINING_ID where TRAINING_ID = @LAST_TRAINING_ID
	END

	--got to make sure TIME_ENDED is different from TIME_STARTED!
	UPDATE TB_TRAINING set TIME_ENDED = CURRENT_TIMESTAMP where TRAINING_ID = @NEW_TRAINING_ID

	UPDATE TB_REVIEW
		SET SCREENING_INDEXED = 'TRUE'
		WHERE REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF
GO




USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeAutoReconcileDelete]    Script Date: 17/02/2023 11:40:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemAttributeAutoReconcileDelete]
(
	@ITEM_ID BIGINT,
	@SET_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@RECONCILLIATION_TYPE nvarchar(10),
	@N_PEOPLE int,
	@AUTO_EXCLUDE bit,
	@CONTACT_ID int
)

As
SET NOCOUNT ON

DECLARE @COUNT_RECS INT = 0
DECLARE @ITEM_SET_ID INT = 0

IF (@RECONCILLIATION_TYPE = 'no compl')
BEGIN
	UPDATE TB_TRAINING_ITEM -- TRY TO RE-LOCK THE ITEM - IF SOMEONE ELSE HAS LOCKED IT, THERE'S NOT MUCH WE CAN DO ABOUT IT THOUGH!
			SET CONTACT_ID_CODING = @CONTACT_ID,
			WHEN_LOCKED = CURRENT_TIMESTAMP
			WHERE ITEM_ID = @ITEM_ID AND CONTACT_ID_CODING = 0
	SET @N_PEOPLE = 99 --(i.e. we don't do anything else - none of the rest is executed)
END
ELSE
BEGIN

	-- **************** STAGE 1: GATHER DATA ON WHETHER RULES FOR AUTO-RECONCILLIATION ARE MET ********************

	IF (@RECONCILLIATION_TYPE = 'Single')
	BEGIN
		SET @COUNT_RECS = 99 -- i.e. we go through to automatic exclude check

		SELECT TOP(1) @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET
			WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID
	END
	
	IF (@RECONCILLIATION_TYPE = 'auto code') -- agreement at the code level
	BEGIN
		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE
			WHERE ITEM_ID = @ITEM_ID AND ATTRIBUTE_ID = @ATTRIBUTE_ID

		IF (@COUNT_RECS < @N_PEOPLE)
		BEGIN
			SELECT TOP(1) @ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID FROM TB_ITEM_SET
				INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID
				WHERE TB_ITEM_SET.ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND IS_COMPLETED = 'TRUE' AND CONTACT_ID = @CONTACT_ID

			IF (@ITEM_SET_ID = 0)
			BEGIN -- WE TRY TO GET A COMPLETED ITEM_SET RECORD, BUT THERE MAY NOT BE ONE!
				SELECT TOP(1) @ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID FROM TB_ITEM_SET
					INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID
					WHERE TB_ITEM_SET.ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND CONTACT_ID = @CONTACT_ID
			END
		END
		ELSE
		BEGIN
			SET @ITEM_SET_ID = 0
		END
	END

	-- one person has to tick 'include' for it to be included.-- N people agreeing on exclude if nobody has ticked 'include' before this threshold is met
	IF (@RECONCILLIATION_TYPE = 'auto safet') 
	BEGIN
		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 -- is anything included?

		SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 AND IS_COMPLETED = 'TRUE' AND CONTACT_ID = @CONTACT_ID
		IF (@COUNT_RECS > 0) -- I.E. THE RULES FOR AUTO INCLUSION ARE STILL MET
		BEGIN
			--SET @COUNT_RECS = @N_PEOPLE
			SET @ITEM_SET_ID = 0
		END
		ELSE
		BEGIN
			-- IF NO INCLUDE IS TICKED, HAVE N PEOPLE TICKED EXCLUDE? IF SO, WE DEFAULT TO THIS
			SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 -- EXCLUDED
			SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 AND IS_COMPLETED = 'TRUE'  AND CONTACT_ID = @CONTACT_ID
		END
		IF (@COUNT_RECS >= @N_PEOPLE) -- I.E. RULE MET, SO WE DON'T UNCOMPLETE
		BEGIN
			SET @ITEM_SET_ID = 0
		END
	END
	
	 -- agreement at the include / exclude level
	IF (@RECONCILLIATION_TYPE = 'auto excl')
	BEGIN
		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 -- INCLUDED
		SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 AND IS_COMPLETED = 'TRUE'  AND CONTACT_ID = @CONTACT_ID

		IF (@COUNT_RECS < @N_PEOPLE)
		BEGIN
			SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 -- EXCLUDED
			SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 AND IS_COMPLETED = 'TRUE'  AND CONTACT_ID = @CONTACT_ID
		END
	END

	

	-- *************************** STAGE 2: AUTO-RECONCILE AND AUTO-COMPLETE ***************************

	IF (@COUNT_RECS < @N_PEOPLE) AND (@RECONCILLIATION_TYPE != 'Single') -- REMOVE AUTO-RECONCILLIATION WHERE RULES ARE NO LONGER MET
	BEGIN
		IF (@ITEM_SET_ID > 0)
		BEGIN
			UPDATE TB_ITEM_SET
				SET IS_COMPLETED = 'FALSE'
				WHERE ITEM_SET_ID = @ITEM_SET_ID
		END
	END
	IF (@AUTO_EXCLUDE = 'TRUE' AND ((@COUNT_RECS < @N_PEOPLE) OR @RECONCILLIATION_TYPE = 'Single')) -- WHERE RULES ARE NO LONGER MET - AUTO *INCLUDE*
	BEGIN
		-- SECOND, AUTO INCLUDE / EXCLUDE
		UPDATE TB_ITEM_REVIEW
			SET IS_INCLUDED = 'TRUE'
			WHERE ITEM_ID = @ITEM_ID
	END
		UPDATE TB_TRAINING_ITEM -- TRY TO RE-LOCK THE ITEM - IF SOMEONE ELSE HAS LOCKED IT, THERE'S NOT MUCH WE CAN DO ABOUT IT THOUGH!
			SET CONTACT_ID_CODING = @CONTACT_ID,
			WHEN_LOCKED = CURRENT_TIMESTAMP
			WHERE ITEM_ID = @ITEM_ID AND CONTACT_ID_CODING = 0
	
END
GO