Use Reviewer
GO

declare @chk int = (Select count(*) from TB_CONTACT where CONTACT_NAME = 'Auto-Reconcile User')
IF @chk = 0
BEGIN
--generate random PW, so that the actual PW isn't in source control
	DECLARE @chars char(100) = '!т#$%&а()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~ийклм'
	declare @rnd varchar(24)
	declare @cnt int = 0
	set @rnd = ''
	WHILE (@cnt <= 24) 
	BEGIN
		SELECT @rnd = @rnd + 
			SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
		SELECT @cnt = @cnt + 1
	END

	DECLARE @C_ID int, @RC int
	declare @DATE_C datetime = GETDATE()

	EXECUTE @RC = [ReviewerAdmin].dbo.[st_ContactCreate] 
	   @CONTACT_NAME ='Auto-Reconcile User'
	  ,@USERNAME = 'Auto-Reconcile_User_BLOCKED'
	  ,@PASSWORD = @rnd
	  ,@DATE_CREATED = @DATE_C
	  ,@EMAIL = 'Auto-Reconcile_UserFAKE@ucl.ac.uk'
	  ,@DESCRIPTION = 'Stand-in account for the Auto-Reconcile user (used in "Retain All Include Codes" option). Needs to remain "not activated" with EXPIRY_DATE = NULL'
	  ,@CONTACT_ID = @C_ID OUTPUT
END
GO

IF TYPE_ID(N'ITEMS_CONTACT_INPUT_TB') IS not NULL 
BEGIN
	drop procedure dbo.st_TrainingNextItem
	drop procedure st_TrainingPreviousItem
	drop procedure st_TrainingUnlockTheseItems
	drop procedure st_TrainingLockTheseItems
	drop type dbo.ITEMS_CONTACT_INPUT_TB
END

CREATE TYPE dbo.ITEMS_CONTACT_INPUT_TB AS TABLE (ItemId bigint, CONTACT_ID int, primary key (ItemId, CONTACT_ID))

GO


CREATE OR ALTER procedure [dbo].[st_GetAutoReconcileDataForRAIC]
(
	@ReviewId int = 0
)
As
	declare @IsInReview bit = 0, @ScreeningSetId int = 0, @ReviewSetId int = 0, @nofPeoplePerItem int = 0;

	declare @cid int = (SELECT CONTACT_ID from TB_CONTACT where CONTACT_NAME = 'Auto-Reconcile User');
	if @reviewId > 0 and @cid is not null and @cid > 0
	begin
		declare @chk int = (Select count(*) from TB_REVIEW_CONTACT where CONTACT_ID = @cid);
		if @chk > 0 set @IsInReview = 1;
	end
	select @ScreeningSetId = SCREENING_CODE_SET_ID, @nofPeoplePerItem = SCREENING_N_PEOPLE from TB_REVIEW where REVIEW_ID = @ReviewId
	select @ReviewSetId = REVIEW_SET_ID from TB_REVIEW_SET where SET_ID = @ScreeningSetId and REVIEW_ID = @ReviewId
	select @cid [DUMMY_ID], @IsInReview [DUMMY_IS_IN_REVIEW], @ScreeningSetId [SCREENING_CODE_SET_ID], @ReviewSetId [REVIEW_SET_ID], @nofPeoplePerItem [SCREENING_N_PEOPLE];
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingNextItem]    Script Date: 27/10/2025 16:45:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_TrainingNextItem]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@TRAINING_CODE_SET_ID INT,
	@SIMULATE bit = 0,
	@USE_LIST_FROM_SEARCH bit = 0
)

As

SET NOCOUNT ON

	DECLARE @CURRENT_TRAINING_ID INT, @CURRENT_TRAINING_FS_ID int;
	Declare @ListedItems TABLE(TRAINING_ITEM_ID int, ITEM_ID bigint, RANK int, CODED_COUNT int null);

	Declare @ItemsToUnlock ITEMS_CONTACT_INPUT_TB
	Declare @ReconcileMode nvarchar(10) = (select SCREENING_RECONCILLIATION from TB_REVIEW where REVIEW_ID = @REVIEW_ID);
	if @ReconcileMode is null set @ReconcileMode = '';

	--normal PS queue
	SELECT @CURRENT_TRAINING_ID = MAX(TRAINING_ID) FROM TB_TRAINING
			WHERE REVIEW_ID = @REVIEW_ID
			AND TIME_STARTED < TIME_ENDED
	--queue created from a search
	SELECT @CURRENT_TRAINING_FS_ID = MAX(TRAINING_FS_ID) FROM TB_TRAINING_FROM_SEARCH
			WHERE REVIEW_ID = @REVIEW_ID

	--To start, lets remove ALL stale locks - in two places (2nd place was added July 2025)
	--we need to add/remove locks in both lists always, as if one item is assigned to someone, it has to be assigned to that someone everywhere
	IF @ReconcileMode != 'raic'
	BEGIN
		--we DO NOT remove stale locks at this point if doing "retain all include codes" reconciliation
		Update TB_TRAINING_ITEM SET CONTACT_ID_CODING = 0, WHEN_LOCKED = NULL
					WHERE TRAINING_ID = @CURRENT_TRAINING_ID AND CONTACT_ID_CODING > 0 and WHEN_LOCKED < DATEADD(hour, -8, GETDATE())
		Update TB_TRAINING_FROM_SEARCH_ITEM SET CONTACT_ID_CODING = 0, WHEN_LOCKED = NULL
					WHERE TRAINING_FS_ID = @CURRENT_TRAINING_FS_ID AND CONTACT_ID_CODING > 0 and WHEN_LOCKED < DATEADD(hour, -8, GETDATE())
	END
	ELSE
	BEGIN -- doing "retain all include codes" reconciliation so we RETURN the items that should be unlocked, after checking if they also need to be auto-reconciled
	--we include all items locked by the current user, because in this mode, they don't get unlocked when adding/removing codes
		Insert into @ItemsToUnlock
			SELECT ITEM_ID, CONTACT_ID_CODING from TB_TRAINING_ITEM 
				where TRAINING_ID = @CURRENT_TRAINING_ID
				AND (CONTACT_ID_CODING > 0 and WHEN_LOCKED < DATEADD(hour, -8, GETDATE())
					OR (CONTACT_ID_CODING = @CONTACT_ID))
			UNION --exluding duplicates!
			SELECT ITEM_ID, CONTACT_ID_CODING from TB_TRAINING_FROM_SEARCH_ITEM 
				where TRAINING_FS_ID = @CURRENT_TRAINING_FS_ID 
				AND (CONTACT_ID_CODING > 0 and WHEN_LOCKED < DATEADD(hour, -8, GETDATE())
					OR (CONTACT_ID_CODING = @CONTACT_ID))
	END
	
	-- IF/ELSE ADDED JULY 2025 - depending on what list we're using!!
	IF @USE_LIST_FROM_SEARCH = 0
	BEGIN 
		--GET the ITEM_ID you need to LOCK (for reuse) This isn't straightfoward as it needs to follow the rules implied in the Sreening tab settings!
	
		--insert into table var the items that current user might need to see (excluding the SCREENING_N_PEOPLE setting), that is:
		--those that are not locked by someone else  [AND (ti.CONTACT_ID_CODING = @CONTACT_ID OR ti.CONTACT_ID_CODING = 0)]
		--AND are not [AND tisSel.ITEM_SET_ID is NULL] (already completed OR coded by curr user) [and (tisSel.IS_COMPLETED = 1 OR tisSel.CONTACT_ID = @CONTACT_ID)]
		INSERT into @ListedItems 
			select TRAINING_ITEM_ID , ti.ITEM_ID , RANK , count(tisC.ITEM_SET_ID) as CODED_COUNT FROM
			TB_TRAINING_ITEM ti 
			LEFT OUTER JOIN TB_ITEM_SET tisSel on ti.ITEM_ID = tisSel.ITEM_ID and tisSel.SET_ID = @TRAINING_CODE_SET_ID 
				and (tisSel.IS_COMPLETED = 1 OR tisSel.CONTACT_ID = @CONTACT_ID)
			LEFT OUTER JOIN TB_ITEM_SET tisC on ti.ITEM_ID = tisC.ITEM_ID and tisC.SET_ID = @TRAINING_CODE_SET_ID
			where @CURRENT_TRAINING_ID = ti.TRAINING_ID AND tisSel.ITEM_SET_ID is NULL
			AND (ti.CONTACT_ID_CODING = @CONTACT_ID OR ti.CONTACT_ID_CODING = 0)
			GROUP BY TRAINING_ITEM_ID , ti.ITEM_ID , RANK
		END
	ELSE --added July 2025
	BEGIN
		--GET the ITEM_ID you need to LOCK (for reuse) This isn't straightfoward as it needs to follow the rules implied in the Sreening tab settings!
			
		--insert into table var the items that current user might need to see (excluding the SCREENING_N_PEOPLE setting), that is:
		--those that are not locked by someone else  [AND (ti.CONTACT_ID_CODING = @CONTACT_ID OR ti.CONTACT_ID_CODING = 0)]
		--AND are not [AND tisSel.ITEM_SET_ID is NULL] (already completed OR coded by curr user) [and (tisSel.IS_COMPLETED = 1 OR tisSel.CONTACT_ID = @CONTACT_ID)]
		INSERT into @ListedItems 
			select TRAINING_FS_ITEM_ID , ti.ITEM_ID , RANK , count(tisC.ITEM_SET_ID) as CODED_COUNT FROM
			TB_TRAINING_FROM_SEARCH_ITEM ti 
			LEFT OUTER JOIN TB_ITEM_SET tisSel on ti.ITEM_ID = tisSel.ITEM_ID and tisSel.SET_ID = @TRAINING_CODE_SET_ID 
				and (tisSel.IS_COMPLETED = 1 OR tisSel.CONTACT_ID = @CONTACT_ID)
			LEFT OUTER JOIN TB_ITEM_SET tisC on ti.ITEM_ID = tisC.ITEM_ID and tisC.SET_ID = @TRAINING_CODE_SET_ID
			where @CURRENT_TRAINING_FS_ID = ti.TRAINING_FS_ID AND tisSel.ITEM_SET_ID is NULL
			AND (ti.CONTACT_ID_CODING = @CONTACT_ID OR ti.CONTACT_ID_CODING = 0)
			GROUP BY TRAINING_FS_ITEM_ID , ti.ITEM_ID , RANK
		
	END
	--SELECT * from @ListedItems
	--SELECT * from @ListedItems where CODED_COUNT < (select SCREENING_N_PEOPLE from TB_REVIEW where REVIEW_ID = @REVIEW_ID)

	

	-- NEXT, LOCK THE ITEM WE'RE GOING TO SEND BACK
	DECLARE @sendingBackTID int --this could be a TRAINING_ITEM_ID or TRAINING_FS_ITEM_ID, we need to then change it to an actual ITEM_ID
	DECLARE @maxCoders int = 0 
	SELECT @maxCoders = SCREENING_N_PEOPLE from TB_REVIEW where REVIEW_ID = @REVIEW_ID
	IF @maxCoders = 0 OR @maxCoders is null
	BEGIN --We don't care about SCREENING_N_PEOPLE
		select @sendingBackTID = MIN(TRAINING_ITEM_ID) FROM @ListedItems
	END
	ELSE
	BEGIN --We ignore items already screened by enough people, as per SCREENING_N_PEOPLE
		select @sendingBackTID = MIN(TRAINING_ITEM_ID) FROM @ListedItems where CODED_COUNT < @maxCoders
	END
	--remove ambiguity change @sendingBackTID to contain the actual ITEM_ID
	select @sendingBackTID = ITEM_ID from @ListedItems where TRAINING_ITEM_ID = @sendingBackTID
	
	IF @SIMULATE = 0 AND @sendingBackTID is not null AND @sendingBackTID > 0 --we ARE doing it!
	BEGIN
		--we need to do things twice, for both possible lists
		UPDATE TB_TRAINING_ITEM
			SET CONTACT_ID_CODING = @CONTACT_ID, WHEN_LOCKED = CURRENT_TIMESTAMP
			WHERE
			ITEM_ID = @sendingBackTID and TRAINING_ID = @CURRENT_TRAINING_ID
		IF @ReconcileMode != 'raic'
		BEGIN
			--[SG July 2025] given that we're locking a new item, we can and SHOULD remove all locks assigned to the current user
			--after all, user can only see one item at the time, so having done the UPDATE above, we now know all pre-existing locks for this user are stale.
			Update TB_TRAINING_ITEM SET CONTACT_ID_CODING = 0, WHEN_LOCKED = NULL
				WHERE TRAINING_ID = @CURRENT_TRAINING_ID AND CONTACT_ID_CODING = @CONTACT_ID and ITEM_ID != @sendingBackTID 
		END
		else DELETE from @ItemsToUnlock where ItemId = @sendingBackTID and CONTACT_ID = @CONTACT_ID --this can happen, at least in theory, don't want to unlock what we're supposed to lock
		--2nd repeat, for new (July 2025) lists created from searches
		UPDATE TB_TRAINING_FROM_SEARCH_ITEM
			SET CONTACT_ID_CODING = @CONTACT_ID, WHEN_LOCKED = CURRENT_TIMESTAMP
			WHERE
			ITEM_ID = @sendingBackTID and TRAINING_FS_ID = @CURRENT_TRAINING_FS_ID
		
		IF @ReconcileMode != 'raic'
		BEGIN
			--as above, make sure this person has only one item locked
			Update TB_TRAINING_FROM_SEARCH_ITEM SET CONTACT_ID_CODING = 0, WHEN_LOCKED = NULL
				WHERE TRAINING_FS_ID = @CURRENT_TRAINING_FS_ID AND CONTACT_ID_CODING = @CONTACT_ID and ITEM_ID != @sendingBackTID 
		END
	END
	

	--SEND ITEM BACK
	IF @USE_LIST_FROM_SEARCH = 0
	BEGIN
		SELECT TI.TRAINING_ITEM_ID, ITEM_ID, [RANK], TRAINING_ID, CONTACT_ID_CODING, SCORE
			FROM TB_TRAINING_ITEM TI
			WHERE TI.ITEM_ID = @sendingBackTID and TRAINING_ID = @CURRENT_TRAINING_ID
	end
	ELSE
	BEGIN
		SELECT TI.TRAINING_FS_ITEM_ID as TRAINING_ITEM_ID, ITEM_ID, [RANK], TRAINING_FS_ID as TRAINING_ID, CONTACT_ID_CODING, SCORE
			FROM TB_TRAINING_FROM_SEARCH_ITEM TI
			WHERE TI.ITEM_ID = @sendingBackTID and TRAINING_FS_ID = @CURRENT_TRAINING_FS_ID
		if (@SIMULATE = 1)
		BEGIN
		--to tell ReviewInfo about the search used to create this FS screening list
			SELECT * from TB_TRAINING_FROM_SEARCH fs
				left join TB_SEARCH s on fs.SEARCH_ID = s.SEARCH_ID
			where TRAINING_FS_ID = @CURRENT_TRAINING_FS_ID
		END
	END

	--IF we're doing "retain all include codes" we have one last reader to tell the BO what to try to auto-reconcile and what to subsequently unlock
	IF @ReconcileMode = 'raic' SELECT * from @ItemsToUnlock

SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingPreviousItem]    Script Date: 27/10/2025 16:55:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER procedure [dbo].[st_TrainingPreviousItem]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@ITEM_ID BIGINT,
	@USE_LIST_FROM_SEARCH bit = 0
)

As

SET NOCOUNT ON
	--WE go and fetch a specific training item, making sure we lock it and release any other lock for the same user, if successful
	DECLARE @CURRENT_TRAINING_ID INT, @CURRENT_TRAINING_FS_ID int;
	declare @trainingIId int;
	
	Declare @ItemsToUnlock ITEMS_CONTACT_INPUT_TB

-- FIRST, GET THE CURRENT TRAINING 'RUN' (CAN'T SEND TO THE STORED PROC, AS IT MAY HAVE CHANGED)
	--normal PS queue
	SELECT @CURRENT_TRAINING_ID = MAX(TRAINING_ID) FROM TB_TRAINING
			WHERE REVIEW_ID = @REVIEW_ID
			AND TIME_STARTED < TIME_ENDED
	--queue created from a search
	SELECT @CURRENT_TRAINING_FS_ID = MAX(TRAINING_FS_ID) FROM TB_TRAINING_FROM_SEARCH
			WHERE REVIEW_ID = @REVIEW_ID
	
	Declare @ReconcileMode nvarchar(10) = (select SCREENING_RECONCILLIATION from TB_REVIEW where REVIEW_ID = @REVIEW_ID);
	if @ReconcileMode is null set @ReconcileMode = '';
	else if @ReconcileMode = 'raic'
	BEGIN
		--we include all items from the current user, as they don't get unlocked when they get coded
		Insert into @ItemsToUnlock
		SELECT ITEM_ID, CONTACT_ID_CODING from TB_TRAINING_ITEM 
			where TRAINING_ID = @CURRENT_TRAINING_ID
			AND (CONTACT_ID_CODING > 0 and WHEN_LOCKED < DATEADD(hour, -8, GETDATE())
				OR (CONTACT_ID_CODING = @CONTACT_ID))
		UNION --exluding duplicates!
		SELECT ITEM_ID, CONTACT_ID_CODING from TB_TRAINING_FROM_SEARCH_ITEM 
			where TRAINING_FS_ID = @CURRENT_TRAINING_FS_ID 
			AND (CONTACT_ID_CODING > 0 and WHEN_LOCKED < DATEADD(hour, -8, GETDATE())
				OR (CONTACT_ID_CODING = @CONTACT_ID))
	END

	-- NEXT, TRY TO LOCK THE ITEM WE'RE GOING TO SEND BACK (BUT WE WON'T OVERRIDE SOMEONE ELSE'S LOCK)
	--[SG Edit: Feb 2023] we now unlock ALL OTHER items currently locked by the present user
	IF @USE_LIST_FROM_SEARCH = 0
	BEGIN
		 set @trainingIId = (select top 1 TRAINING_ITEM_ID from TB_TRAINING_ITEM 
									where ITEM_ID = @ITEM_ID AND CONTACT_ID_CODING = 0 AND TRAINING_ID = @CURRENT_TRAINING_ID)
	END
	ELSE
	BEGIN
		set @trainingIId  = (select top 1 TRAINING_FS_ITEM_ID from TB_TRAINING_FROM_SEARCH_ITEM 
									where ITEM_ID = @ITEM_ID AND CONTACT_ID_CODING = 0 AND TRAINING_FS_ID = @CURRENT_TRAINING_FS_ID)
	END


	IF @trainingIId is not null AND @trainingIId > 0
	BEGIN
		--Lock the item in both lists (if it exists)
		UPDATE TB_TRAINING_ITEM
			SET CONTACT_ID_CODING = @CONTACT_ID, WHEN_LOCKED = CURRENT_TIMESTAMP
			WHERE TRAINING_ID = @CURRENT_TRAINING_ID and ITEM_ID = @ITEM_ID
		UPDATE TB_TRAINING_FROM_SEARCH_ITEM
			SET CONTACT_ID_CODING = @CONTACT_ID, WHEN_LOCKED = CURRENT_TIMESTAMP
			WHERE TRAINING_FS_ID = @CURRENT_TRAINING_FS_ID and ITEM_ID = @ITEM_ID

		--We have just locked our ITEM, so we know we can safely unlock all other items currently assigned to this user
		--we unlock them IF we're not doing "Retain all include codes" reconciliation
		if @ReconcileMode != 'raic'
		BEGIN
			Update TB_TRAINING_ITEM SET CONTACT_ID_CODING = 0, WHEN_LOCKED = NULL
				WHERE CONTACT_ID_CODING = @CONTACT_ID AND TRAINING_ID = @CURRENT_TRAINING_ID and ITEM_ID != @ITEM_ID
			Update TB_TRAINING_FROM_SEARCH_ITEM SET CONTACT_ID_CODING = 0, WHEN_LOCKED = NULL
				WHERE CONTACT_ID_CODING = @CONTACT_ID AND TRAINING_FS_ID = @CURRENT_TRAINING_FS_ID and ITEM_ID != @ITEM_ID
		END
		else DELETE from @ItemsToUnlock where ItemId = @ITEM_ID and CONTACT_ID = @CONTACT_ID --this can happen, at least in theory, don't want to unlock what we're supposed to lock
		
		-- FINALLY, SEND IT BACK, if we did manage to lock it for the current user.
		IF @USE_LIST_FROM_SEARCH = 0
		BEGIN
			SELECT TI.TRAINING_ITEM_ID, ITEM_ID, [RANK], TRAINING_ID, @CONTACT_ID, SCORE
				FROM TB_TRAINING_ITEM TI
				WHERE TRAINING_ID = @CURRENT_TRAINING_ID AND ITEM_ID = @ITEM_ID
		END
		ELSE
		BEGIN
			SELECT TI.TRAINING_FS_ITEM_ID as TRAINING_ITEM_ID, ITEM_ID, [RANK], TRAINING_FS_ID as TRAINING_ID, CONTACT_ID_CODING, SCORE
				FROM TB_TRAINING_FROM_SEARCH_ITEM TI
				WHERE TRAINING_FS_ID = @CURRENT_TRAINING_FS_ID and TI.ITEM_ID = @ITEM_ID 
		END

		if @ReconcileMode = 'raic' select * from @ItemsToUnlock
	END
SET NOCOUNT OFF
GO

CREATE OR ALTER procedure [dbo].[st_TrainingUnlockTheseItems]
(
	@REVIEW_ID int
	,@ItemsToUnlock ITEMS_CONTACT_INPUT_TB READONLY
)

As
	DECLARE @CURRENT_TRAINING_ID INT = (select MAX(TRAINING_ID) FROM TB_TRAINING
			WHERE REVIEW_ID = @REVIEW_ID
			AND TIME_STARTED < TIME_ENDED)
	Declare @CURRENT_TRAINING_FS_ID int = (select MAX(TRAINING_FS_ID) FROM TB_TRAINING_FROM_SEARCH
			WHERE REVIEW_ID = @REVIEW_ID)
	Update ti SET CONTACT_ID_CODING = 0, WHEN_LOCKED = NULL from @ItemsToUnlock itu
		inner join TB_TRAINING_ITEM ti on ti.TRAINING_ID = @CURRENT_TRAINING_ID and itu.ItemId = ti.ITEM_ID and ti.CONTACT_ID_CODING = itu.CONTACT_ID
	Update ti SET CONTACT_ID_CODING = 0, WHEN_LOCKED = NULL from @ItemsToUnlock itu
		inner join TB_TRAINING_FROM_SEARCH_ITEM ti on ti.TRAINING_FS_ID = @CURRENT_TRAINING_FS_ID and itu.ItemId = ti.ITEM_ID and ti.CONTACT_ID_CODING = itu.CONTACT_ID
GO
Use Reviewer
GO
CREATE OR ALTER procedure [dbo].[st_TrainingLockTheseItems]
(
	@REVIEW_ID int
	,@ItemsToLock ITEMS_CONTACT_INPUT_TB READONLY
)
as
DECLARE @CURRENT_TRAINING_ID INT = (select MAX(TRAINING_ID) FROM TB_TRAINING
			WHERE REVIEW_ID = @REVIEW_ID
			AND TIME_STARTED < TIME_ENDED)
	Declare @CURRENT_TRAINING_FS_ID int = (select MAX(TRAINING_FS_ID) FROM TB_TRAINING_FROM_SEARCH
			WHERE REVIEW_ID = @REVIEW_ID)
	declare @missingItems table(item_id bigint, contact_id int)
	insert into @missingItems (item_id, contact_id) select l.ItemId, l.CONTACT_ID from @ItemsToLock l
		

	Update ti SET CONTACT_ID_CODING = itu.CONTACT_ID, WHEN_LOCKED = GETDATE() from @ItemsToLock itu
		inner join TB_TRAINING_ITEM ti on ti.TRAINING_ID = @CURRENT_TRAINING_ID and itu.ItemId = ti.ITEM_ID 
	Update ti SET CONTACT_ID_CODING = itu.CONTACT_ID, WHEN_LOCKED = GETDATE() from @ItemsToLock itu
		inner join TB_TRAINING_FROM_SEARCH_ITEM ti on ti.TRAINING_FS_ID = @CURRENT_TRAINING_FS_ID and itu.ItemId = ti.ITEM_ID 
GO

