USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_LogReviewJob]    Script Date: 08/07/2024 13:24:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE or ALTER PROCEDURE [dbo].[st_LogReviewJobUpdate]
	-- Add the parameters for the stored procedure here
	@ReviewId int,
	@REVIEW_JOB_ID int,
	@CurrentState nvarchar(50),
	@Success bit null,
	@JobMessage nvarchar(4000)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	if @JobMessage is not null AND @JobMessage != '' 
		Update [tb_REVIEW_JOB] set
			   [END_TIME] = GETDATE()
			   ,[CURRENT_STATE] = @CurrentState
			   ,[SUCCESS] = @Success
			   ,[JOB_MESSAGE] = @JobMessage
		WHERE REVIEW_ID = @ReviewId
			and REVIEW_JOB_ID = @REVIEW_JOB_ID
	ELSE
		Update [tb_REVIEW_JOB] set
			   [END_TIME] = GETDATE()
			   ,[CURRENT_STATE] = @CurrentState
			   ,[SUCCESS] = @Success
			   --,[JOB_MESSAGE] = @JobMessage
		WHERE REVIEW_ID = @ReviewId
			and REVIEW_JOB_ID = @REVIEW_JOB_ID
     
END
GO

USE [Reviewer]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_IsAttributeInTree]    Script Date: 10/07/2024 12:33:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	return values: 0 for "not running", 1 for "running OK", -1 for "says it's running, but likely dead, as it hasn't updated for 15m"
-- =============================================
CREATE OR ALTER FUNCTION [dbo].[fn_PriorityScreeningIsRunning] 
(
	-- Add the parameters for the function here
	@REVIEW_ID int
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @JobId int = (select top 1 REVIEW_JOB_ID from tb_REVIEW_JOB where REVIEW_ID = @REVIEW_ID and SUCCESS IS NULL order by REVIEW_JOB_ID DESC);
	IF @JobId is null return 0;
	ELSE
	BEGIN
		declare @LastActive datetime = (select END_TIME from tb_REVIEW_JOB where REVIEW_JOB_ID = @JobId);
		declare @diff int = DateDiff(MINUTE, @LastActive, getdate());
		if @diff > 15 return -1;
		ELSE return 1;
	END
	RETURN 0;--this doesn't happen, all cases should be covered already...
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupCheckOngoingLog]    Script Date: 08/07/2024 13:31:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE or ALTER PROCEDURE [dbo].[st_TrainingScreeningCheckOngoingLog] 
	-- Add the parameters for the stored procedure here
	(
		@revID int,
		@CONTACT_ID int,
		@NewJobId int = 0 output,
		@NewTrainingId int = 0 output
	)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT, XACT_ABORT ON;
	--Based on the template of st_ItemDuplicateGroupCheckOngoingLog
	--see https://weblogs.sqlteam.com/dang/2007/10/28/conditional-insertupdate-race-condition/
	--for details on how this is dealing with concurrency problems. Aim is to avoid having two instances of this SP insert
	--SP is called whenever the list of duplicates is retrieved AND when we are asking to find new duplicates...
	
	--paired with the "lasting lock", the transaction prevents two instances to be triggered concurrently
	--without the lasting lock, the TRAN itself won't work, see link above for the details.
	BEGIN TRAN A

	--get the last job ID, and acquire an exclusive table lock
	declare @jobid int = (select top 1 REVIEW_JOB_ID 
							from tb_REVIEW_JOB WITH (TABLOCKX, HOLDLOCK) 
							where REVIEW_ID = @revID AND JOB_TYPE = 'PS training' and SUCCESS is null
							order by REVIEW_JOB_ID desc)
	--TABLOCKX: exclusive table lock; HOLDLOCK: keep this lock on until the whole transaction is done.
	
	--how did the last job go?
	declare @state int = 0 --0 for not running, 1 for running normally, -1 for running and inactive for too long
	IF @jobid is not null set @state = dbo.fn_PriorityScreeningIsRunning(@revID);
	
	if @state = 0
		BEGIN
			--EITHER never done for this review, or last run has finished. 
			--we insert the "PS training" line here, to make this properly ACID
			--to be "Atomic" we also have the transaction statement AND the lock hints on the first lookup for the @jobid
			insert into tb_REVIEW_JOB (REVIEW_ID, CONTACT_ID, START_TIME, END_TIME, JOB_TYPE, CURRENT_STATE) 
				VALUES (@revID, @CONTACT_ID, getdate(),getdate(), 'PS training', 'Starting')
			set @NewJobId = SCOPE_IDENTITY()

			insert into TB_TRAINING (REVIEW_ID, CONTACT_ID, TIME_STARTED, TIME_ENDED)
				VALUES (@revID, @CONTACT_ID, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
			set @NewTrainingId = SCOPE_IDENTITY()
				
			COMMIT TRAN A --Have to commit, before we can return!
			return 1
		END
	ELSE IF @state = -1
	BEGIN --has been "running" without updates for too long
		--we'll mark the job as failed and start a new one
		declare @NewMSG nvarchar(4000) = (select JOB_MESSAGE from tb_REVIEW_JOB where REVIEW_JOB_ID = @jobid);

		--making sure we never get a truncation error! LEFT(...) does work OK if we ask for more Chars than the string contains
		set @NewMSG = 'Automated Failure: inactive for too long' + CHAR(13)+ CHAR(10) + LEFT(@NewMSG,3950);
		 
		update tb_REVIEW_JOB set CURRENT_STATE = 'Failed', SUCCESS = 0, JOB_MESSAGE = @NewMSG where REVIEW_JOB_ID = @jobid

		insert into tb_REVIEW_JOB (REVIEW_ID, CONTACT_ID, START_TIME, END_TIME, JOB_TYPE, CURRENT_STATE) 
			VALUES (@revID, @CONTACT_ID, getdate(),getdate(), 'PS training', 'Starting')
		set @NewJobId = SCOPE_IDENTITY()

		insert into TB_TRAINING (REVIEW_ID, CONTACT_ID, TIME_STARTED, TIME_ENDED)
			VALUES (@revID, @CONTACT_ID, getdate(),getdate())
		set @NewTrainingId = SCOPE_IDENTITY()
				
		COMMIT TRAN A --Have to commit, before we can return!
		return 1
	END
	ELSE --must be "1" for "running OK"
	BEGIN
		COMMIT TRAN A --Have to commit, before we can return!
		return -1
	END
	--We should never reach this, but just in case, we should return something...
	COMMIT TRAN A --Have to commit, before we can return!
	return -4; --signalling that something is WRONG!
	SET NOCOUNT OFF
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewContact]    Script Date: 10/07/2024 10:44:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[st_ReviewContact]
(
	@CONTACT_ID INT
)

As

SELECT REVIEW_CONTACT_ID, rc.REVIEW_ID, rc.CONTACT_ID, REVIEW_NAME, dbo.fn_REBUILD_ROLES(rc.REVIEW_CONTACT_ID) as ROLES
	,own.CONTACT_NAME as 'OWNER', case when LR is null
									then r.DATE_CREATED
									else LR
								 end
								 as 'LAST_ACCESS'
	, r.SHOW_SCREENING, r.SCREENING_CODE_SET_ID, r.SCREENING_MODE, r.SCREENING_WHAT_ATTRIBUTE_ID, r.SCREENING_N_PEOPLE
	, r.SCREENING_RECONCILLIATION, r.SCREENING_AUTO_EXCLUDE
	, BL_ACCOUNT_CODE,BL_AUTH_CODE, BL_TX, BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX
	, (SELECT SUM(N_PAPERS) from tb_MAG_RELATED_RUN MRR
			WHERE REVIEW_ID = rc.REVIEW_ID  and USER_STATUS = 'Unchecked') NAutoUpdates
FROM TB_REVIEW_CONTACT rc
INNER JOIN TB_REVIEW r ON rc.REVIEW_ID = r.REVIEW_ID
inner join TB_CONTACT own on r.FUNDER_ID = own.CONTACT_ID
left join (
			select MAX(LAST_RENEWED) LR, REVIEW_ID
			from sTB_LOGON_TICKET  
			where @CONTACT_ID = CONTACT_ID
			group by REVIEW_ID
			) as t
			on t.REVIEW_ID = r.REVIEW_ID
WHERE rc.CONTACT_ID = @CONTACT_ID and (r.ARCHIE_ID is null OR r.ARCHIE_ID = 'prospective_______')
ORDER BY REVIEW_NAME
GO

ALTER   PROCEDURE [dbo].[st_ReviewContactForSiteAdmin]
(
	@CONTACT_ID INT
	,@REVIEW_ID int
)

As

SELECT 0 as REVIEW_CONTACT_ID, - r.REVIEW_ID as REVIEW_ID, rc.CONTACT_ID, REVIEW_NAME, 'AdminUser;' as ROLES
	,own.CONTACT_NAME as 'OWNER', case when LR is null
									then r.DATE_CREATED
									else LR
								 end
								 as 'LAST_ACCESS'
	, r.SHOW_SCREENING, r.SCREENING_CODE_SET_ID, r.SCREENING_MODE, r.SCREENING_WHAT_ATTRIBUTE_ID, r.SCREENING_N_PEOPLE
	, r.SCREENING_RECONCILLIATION, r.SCREENING_AUTO_EXCLUDE
	, BL_ACCOUNT_CODE,BL_AUTH_CODE, BL_TX, BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX
	, (SELECT SUM(N_PAPERS) from tb_MAG_RELATED_RUN MRR
			WHERE REVIEW_ID = @REVIEW_ID  and USER_STATUS = 'Unchecked') NAutoUpdates
FROM TB_CONTACT rc
INNER JOIN TB_REVIEW r ON rc.CONTACT_ID = @CONTACT_ID and rc.IS_SITE_ADMIN = 1 and r.REVIEW_ID = @REVIEW_ID 
inner join TB_CONTACT own on r.FUNDER_ID = own.CONTACT_ID
left join (
			select MAX(LAST_RENEWED) LR, REVIEW_ID
			from sTB_LOGON_TICKET  
			where @CONTACT_ID = CONTACT_ID and REVIEW_ID = @REVIEW_ID
			group by REVIEW_ID
			) as t
			on t.REVIEW_ID = r.REVIEW_ID
WHERE rc.CONTACT_ID = @CONTACT_ID
ORDER BY REVIEW_NAME
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewInfo]    Script Date: 10/07/2024 10:46:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ReviewInfo]
(
	@REVIEW_ID INT
	
)

As
BEGIN
	declare @PS_IS_RUNNING bit = CASE WHEN dbo.fn_PriorityScreeningIsRunning(@REVIEW_ID) = 1 then 1 ELSE 0 END;

	SELECT *, @PS_IS_RUNNING as SCREENING_MODEL_RUNNING_V2 FROM TB_REVIEW	WHERE REVIEW_ID = @REVIEW_ID;

	--prep for the second reader, returning info about credit being (possibly) available for use with robots - short and sweet, this needs to be fast
	select distinct a.CREDIT_PURCHASE_ID from 
	(select CREDIT_PURCHASE_ID from TB_SITE_LIC_REVIEW lr
		inner join sTB_CREDIT_FOR_ROBOTS cfr on lr.REVIEW_ID = @REVIEW_ID and cfr.LICENSE_ID = lr.SITE_LIC_ID
	UNION
		select CREDIT_PURCHASE_ID from sTB_CREDIT_FOR_ROBOTS where REVIEW_ID = @REVIEW_ID
	) as a
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewInfoUpdate]    Script Date: 11/07/2024 10:05:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   procedure [dbo].[st_ReviewInfoUpdate]
(
	@REVIEW_ID int
,	@SCREENING_CODE_SET_ID int
,	@SCREENING_MODE nvarchar(10)
,	@SCREENING_RECONCILLIATION nvarchar(10)
,	@SCREENING_WHAT_ATTRIBUTE_ID bigint
,	@SCREENING_N_PEOPLE int
,	@SCREENING_AUTO_EXCLUDE bit
,	@MAG_ENABLED INT
,	@SHOW_SCREENING bit
) 

As

SET NOCOUNT ON

UPDATE TB_REVIEW
	SET SCREENING_CODE_SET_ID = @SCREENING_CODE_SET_ID
,		SCREENING_MODE = @SCREENING_MODE
,		SCREENING_RECONCILLIATION = @SCREENING_RECONCILLIATION
,		SCREENING_WHAT_ATTRIBUTE_ID = @SCREENING_WHAT_ATTRIBUTE_ID
,		SCREENING_N_PEOPLE = @SCREENING_N_PEOPLE
,		SCREENING_AUTO_EXCLUDE = @SCREENING_AUTO_EXCLUDE
,		MAG_ENABLED = @MAG_ENABLED
,		SHOW_SCREENING = @SHOW_SCREENING
WHERE REVIEW_ID = @REVIEW_ID
	

SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ScreeningCreateMLList]    Script Date: 10/07/2024 10:47:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE or ALTER procedure [dbo].[st_ScreeningCreateMLList]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@WHAT_ATTRIBUTE_ID BIGINT,
	@SCREENING_MODE nvarchar(10),
	@CODE_SET_ID INT,
	@TRAINING_ID INT
)

As

SET NOCOUNT ON

	DECLARE @TP INT
	DECLARE @TN INT

	-- ***** FIRST, GET THE STATS IN TERMS OF # ITEMS SCREENED TO POPULATE THE TRIANING TABLE (GIVES US THE GRAPH ON THE SCREENING TAB)
	SELECT @TP = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 10
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

	SELECT @TN = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 11
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

	-- ********** SECOND, ENTER THE LIST OF ITEMS INTO TB_TRAINING_ITEM ACCORDING TO WHETHER WE'RE FILTERING BY AN ATTRIBUTE OR DOING THE WHOLE REVIEW

	IF @WHAT_ATTRIBUTE_ID > 0  -- i.e. we're filtering by a code
	BEGIN
		INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK], SCORE)
			SELECT @TRAINING_ID, AZ.ITEM_ID, 0, 0, AZ.SCORE
				FROM TB_SCREENING_ML_TEMP AZ
			INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = AZ.ITEM_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @WHAT_ATTRIBUTE_ID
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND IR.REVIEW_ID = @REVIEW_ID
			WHERE NOT AZ.ITEM_ID IN
				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID) AND AZ.REVIEW_ID = @REVIEW_ID
			ORDER BY AZ.SCORE DESC
			
	END
	ELSE -- NOT FILTERING BY A CODE, SO EVERYTHING IN THE REVIEW THAT'S INCLUDED AND SO FAR UNCODED IS INCLUDED
	BEGIN
		INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK], SCORE)
		SELECT @TRAINING_ID, AZ.ITEM_ID, 0, 0, AZ.SCORE
				FROM TB_SCREENING_ML_TEMP AZ
			WHERE NOT AZ.ITEM_ID IN
				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID) AND AZ.REVIEW_ID = @REVIEW_ID
			ORDER BY AZ.SCORE DESC
	END

	/* SET THE RANKS TO INCREMENT */
	DECLARE @START_INDEX INT = 0
	SELECT @START_INDEX = MIN(TRAINING_ITEM_ID) FROM TB_TRAINING_ITEM WHERE TRAINING_ID = @TRAINING_ID
	UPDATE TB_TRAINING_ITEM
		SET [RANK] = TRAINING_ITEM_ID - @START_INDEX + 1
		WHERE TRAINING_ID = @TRAINING_ID


	-- FINALLY, MIGRATE ANY NON-STALE CODING LOCKS FROM THE PREVIOUS TRAINING RUN
	DECLARE @LAST_TRAINING_ID INT

	SELECT @LAST_TRAINING_ID = TRAINING_ID FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID AND TRAINING_ID < (SELECT MAX(TRAINING_ID) FROM TB_TRAINING WHERE REVIEW_ID = @REVIEW_ID)

	DECLARE @CURRENT_ITERATION INT
	
	SELECT @CURRENT_ITERATION = MAX(ITERATION) + 1 FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID
		
	IF (@CURRENT_ITERATION IS NULL)
	BEGIN
		SET @CURRENT_ITERATION = 1
	END

	UPDATE A
		SET A.CONTACT_ID_CODING = B.CONTACT_ID_CODING,
		A.WHEN_LOCKED = B.WHEN_LOCKED
		FROM TB_TRAINING_ITEM A
		JOIN
		TB_TRAINING_ITEM B ON A.ITEM_ID = B.ITEM_ID AND CURRENT_TIMESTAMP < DATEADD(DAY, 7, B.WHEN_LOCKED) AND
			B.TRAINING_ID = @LAST_TRAINING_ID
		WHERE A.TRAINING_ID = @TRAINING_ID

	UPDATE TB_TRAINING
		SET TIME_ENDED = CURRENT_TIMESTAMP,
		ITERATION = @CURRENT_ITERATION,
		TRUE_POSITIVES = @TP,
		TRUE_NEGATIVES = @TN
		WHERE TB_TRAINING.TRAINING_ID = @TRAINING_ID

	-- delete the old list(s) of items to screen for this review
	DELETE TI
	FROM TB_TRAINING_ITEM TI
	INNER JOIN TB_TRAINING T ON T.TRAINING_ID = TI.TRAINING_ID
	WHERE T.REVIEW_ID = @REVIEW_ID AND T.TRAINING_ID < @TRAINING_ID

	DELETE FROM TB_SCREENING_ML_TEMP WHERE REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ScreeningCreateNonMLList]    Script Date: 10/07/2024 10:48:11 ******/
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

	--UPDATE TB_REVIEW
	--	SET SCREENING_INDEXED = 'TRUE'
	--	WHERE REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingWriteDataToAzure]    Script Date: 10/07/2024 10:48:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_TrainingWriteDataToAzure]
(
	@REVIEW_ID INT,
	@SCREENING_INDEXED BIT = 'FALSE'
--,	@SCREENING_DATA_FILE NVARCHAR(50)
)

As

SET NOCOUNT ON

	DELETE FROM TB_SCREENING_ML_TEMP WHERE REVIEW_ID = @REVIEW_ID

	SELECT DISTINCT @REVIEW_ID REVIEW_ID, tia.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '1' INCLUDED 
	FROM TB_ITEM_ATTRIBUTE tia
	INNER JOIN TB_ITEM_REVIEW ir on tia.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM I ON I.ITEM_ID = tia.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = tia.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		tia.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
	WHERE TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID
			
	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID REVIEW_ID, tia.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '0' INCLUDED  
	FROM TB_ITEM_ATTRIBUTE tia
	INNER JOIN TB_ITEM_REVIEW ir on tia.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM I ON I.ITEM_ID = tia.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = tia.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		tia.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
	WHERE TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_REVIEW.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '99' INCLUDED FROM TB_ITEM_REVIEW
	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		where TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND not TB_ITEM_REVIEW.ITEM_ID in
		(
			SELECT ITEM_ID FROM TB_ITEM_SET
				INNER JOIN TB_REVIEW ON TB_REVIEW.SCREENING_CODE_SET_ID = TB_ITEM_SET.SET_ID
				WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_REVIEW.REVIEW_ID = @REVIEW_ID
		)


SET NOCOUNT OFF
GO


--Destructive part, disabled for now...
--USE [Reviewer]
--GO
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_TrainingItemText]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_TrainingItemText]
--GO


--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_TrainingInsert]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_TrainingInsert]
--GO
--IF COL_LENGTH('dbo.TB_REVIEW', 'SCREENING_MODEL_RUNNING') IS NULL
--BEGIN
--	ALTER TABLE TB_REVIEW
--	DROP COLUMN SCREENING_MODEL_RUNNING, SCREENING_INDEXED;
--END
--GO







