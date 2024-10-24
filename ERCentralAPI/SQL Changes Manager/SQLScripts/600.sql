USE [Reviewer]
GO
IF EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[fn_PriorityScreeningIsRunning]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[fn_PriorityScreeningIsRunning]
GO 
/****** Object:  UserDefinedFunction [dbo].[fn_JobOfTypeIsRunning]   Script Date: 18/07/2024 11:21:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_JOB_TYPE_STATE_SUCCESS')
BEGIN
	DROP INDEX IX_JOB_TYPE_STATE_SUCCESS ON tb_REVIEW_JOB; 
END
CREATE NONCLUSTERED INDEX [IX_JOB_TYPE_STATE_SUCCESS] ON [dbo].[tb_REVIEW_JOB]
(
	[JOB_TYPE] ASC
)
INCLUDE ( 	[CURRENT_STATE],
	[SUCCESS]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = ON, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

GO



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	return values: 0 for "not running", 1 for "running OK", -1 for "says it's running, but likely dead, as it hasn't updated for 15m"
-- =============================================
CREATE or ALTER   FUNCTION [dbo].[fn_JobOfTypeIsRunning] 
(
	-- Add the parameters for the function here
	@REVIEW_ID int,
	@JOB_TYPE nvarchar(50),
	@TIMEOUT_MINUTES int = 15
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @JobId int = (select top 1 REVIEW_JOB_ID from tb_REVIEW_JOB where REVIEW_ID = @REVIEW_ID and SUCCESS IS NULL 
					AND JOB_TYPE = @JOB_TYPE
					order by REVIEW_JOB_ID DESC);
	IF @JobId is null return 0;
	ELSE
	BEGIN
		declare @LastActive datetime = (select END_TIME from tb_REVIEW_JOB where REVIEW_JOB_ID = @JobId);
		declare @diff int = DateDiff(MINUTE, @LastActive, getdate());
		if @diff > @TIMEOUT_MINUTES return -1;
		ELSE return 1;
	END
	RETURN 0;--this doesn't happen, all cases should be covered already...
END
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
	declare @PS_IS_RUNNING bit = CASE WHEN dbo.fn_JobOfTypeIsRunning(@REVIEW_ID, 'PS training', 15) = 1 then 1 ELSE 0 END;

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
	IF @jobid is not null set @state = dbo.fn_JobOfTypeIsRunning(@revID, 'PS training', 15);
	
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
		if @NewMSG is null set @NewMSG = 'Automated Failure: inactive for too long'
		else set @NewMSG = 'Automated Failure: inactive for too long' + CHAR(13)+ CHAR(10) + LEFT(@NewMSG,3950);
		 
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

ALTER procedure [dbo].[st_ClassifierSaveModel]
(
	@REVIEW_ID INT
,	@ATTRIBUTE_ID_ON BIGINT = NULL
,	@ATTRIBUTE_ID_NOT_ON BIGINT = NULL
,	@CONTACT_ID INT
,	@MODEL_TITLE NVARCHAR(1000) = NULL
,	@NEW_MODEL_ID INT OUTPUT
,	@NewJobId int = 0 output
)

As

SET NOCOUNT ON
	--get the last job ID, and acquire an exclusive table lock
	BEGIN TRAN A;
	declare @jobid int = (select top 1 REVIEW_JOB_ID 
							from tb_REVIEW_JOB WITH (TABLOCKX, HOLDLOCK) 
							where REVIEW_ID = @REVIEW_ID AND JOB_TYPE = 'Build Classifier' and SUCCESS is null
							order by REVIEW_JOB_ID desc);
	declare @state int = 0 --0 for not running, 1 for running normally, -1 for running and inactive for too long
	IF @jobid is not null set @state = dbo.fn_JobOfTypeIsRunning(@REVIEW_ID, 'Build Classifier', 15);
	if @state = 0
	BEGIN
		INSERT INTO tb_CLASSIFIER_MODEL(MODEL_TITLE, CONTACT_ID, REVIEW_ID, ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, TIME_STARTED, TIME_ENDED)
			VALUES(@MODEL_TITLE, @CONTACT_ID, @REVIEW_ID, @ATTRIBUTE_ID_ON, @ATTRIBUTE_ID_NOT_ON, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
		SET @NEW_MODEL_ID = SCOPE_IDENTITY();
		insert into tb_REVIEW_JOB (REVIEW_ID, CONTACT_ID, START_TIME, END_TIME, JOB_TYPE, CURRENT_STATE, JOB_MESSAGE) 
				VALUES (@REVIEW_ID, @CONTACT_ID, getdate(),getdate(), 'Build Classifier', 'Starting', 'ModelId:' + CAST(@NEW_MODEL_ID as varchar(20)));
		set @NewJobId = SCOPE_IDENTITY();
	END
	ELSE IF @state = -1 
	BEGIN
		--has been "running" without updates for too long
		--we'll mark the job as failed and start a new one
		declare @NewMSG nvarchar(4000) = (select JOB_MESSAGE from tb_REVIEW_JOB where REVIEW_JOB_ID = @jobid);

		--making sure we never get a truncation error! LEFT(...) does work OK if we ask for more Chars than the string contains
		if @NewMSG is null set @NewMSG = 'Automated Failure: inactive for too long'
		else set @NewMSG = 'Automated Failure: inactive for too long' + CHAR(13)+ CHAR(10) + LEFT(@NewMSG,3950);

		update tb_REVIEW_JOB set CURRENT_STATE = 'Failed', SUCCESS = 0, JOB_MESSAGE = @NewMSG where REVIEW_JOB_ID = @jobid

		INSERT INTO tb_CLASSIFIER_MODEL(MODEL_TITLE, CONTACT_ID, REVIEW_ID, ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, TIME_STARTED, TIME_ENDED)
			VALUES(@MODEL_TITLE, @CONTACT_ID, @REVIEW_ID, @ATTRIBUTE_ID_ON, @ATTRIBUTE_ID_NOT_ON, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
		SET @NEW_MODEL_ID = SCOPE_IDENTITY();
		insert into tb_REVIEW_JOB (REVIEW_ID, CONTACT_ID, START_TIME, END_TIME, JOB_TYPE, CURRENT_STATE, JOB_MESSAGE) 
				VALUES (@REVIEW_ID, @CONTACT_ID, getdate(),getdate(), 'Build Classifier', 'Starting', 'ModelId:' + CAST(@NEW_MODEL_ID as varchar(20)));
		set @NewJobId = SCOPE_IDENTITY();
	
	END
	ELSE
	BEGIN
		SET @NEW_MODEL_ID = 0;
	END

	COMMIT TRAN A;

SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierUpdateModelTitle]    Script Date: 19/07/2024 09:39:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_ClassifierCanRunCheckAndMarkAsStarting]
(
	@MODEL_ID INT
	,@REVIEW_ID int
	,@CONTACT_ID int
	,@TITLE nvarchar(1000) = ''
	,@IsApply bit
	,@NewJobId int = 0 output
)

As

SET NOCOUNT ON
BEGIN

	BEGIN TRAN A;
	declare @check int = (select count(*) from tb_CLASSIFIER_MODEL where MODEL_ID = @MODEL_ID and REVIEW_ID = @REVIEW_ID)
	if @check != 1
	BEGIN
		--check failed, we didn't find the right model for this review
		set @NewJobId = 0;--means no job can start, right now
		COMMIT TRAN A;
		return;
	END
	declare @JobT nvarchar(50) = ''
	IF @IsApply = 1 set @JobT = 'Apply Classifier';
	ELSE set @JobT = 'Build Classifier';

	declare @PreviousJobid int = (select top 1 REVIEW_JOB_ID 
							from tb_REVIEW_JOB WITH (TABLOCKX, HOLDLOCK) 
							where REVIEW_ID = @REVIEW_ID AND 
												JOB_TYPE = @JobT 
												and SUCCESS is null
							order by REVIEW_JOB_ID desc);

	declare @state int = 0 --0 for not running, 1 for running normally, -1 for running and inactive for too long
	IF @PreviousJobid is not null set @state = dbo.fn_JobOfTypeIsRunning(@REVIEW_ID, @JobT, 15);
	if @state = 0
	BEGIN
		IF @IsApply = 0 AND @TITLE != '' update tb_CLASSIFIER_MODEL set MODEL_TITLE = @TITLE WHERE MODEL_ID = @MODEL_ID
		insert into tb_REVIEW_JOB (REVIEW_ID, CONTACT_ID, START_TIME, END_TIME, JOB_TYPE, CURRENT_STATE, JOB_MESSAGE) 
				VALUES (@REVIEW_ID, @CONTACT_ID, getdate(),getdate(), @JobT, 'Starting', 'ModelId:' + CAST(@MODEL_ID as varchar(20)));
		set @NewJobId = SCOPE_IDENTITY();
	END
	ELSE IF @state = -1 
	BEGIN
		--has been "running" without updates for too long
		--we'll mark the job as failed and start a new one
		declare @NewMSG nvarchar(4000) = (select JOB_MESSAGE from tb_REVIEW_JOB where REVIEW_JOB_ID = @PreviousJobid);

		--making sure we never get a truncation error! LEFT(...) does work OK if we ask for more Chars than the string contains
		if @NewMSG is null set @NewMSG = 'Automated Failure: inactive for too long'
		else set @NewMSG = 'Automated Failure: inactive for too long' + CHAR(13)+ CHAR(10) + LEFT(@NewMSG,3950);

		update tb_REVIEW_JOB set CURRENT_STATE = 'Failed', SUCCESS = 0, JOB_MESSAGE = @NewMSG where REVIEW_JOB_ID = @PreviousJobid

		IF @IsApply = 0 AND @TITLE != '' update tb_CLASSIFIER_MODEL set MODEL_TITLE = @TITLE WHERE MODEL_ID = @MODEL_ID
		insert into tb_REVIEW_JOB (REVIEW_ID, CONTACT_ID, START_TIME, END_TIME, JOB_TYPE, CURRENT_STATE, JOB_MESSAGE) 
				VALUES (@REVIEW_ID, @CONTACT_ID, getdate(),getdate(), @JobT, 'Starting', 'ModelId:' + CAST(@MODEL_ID as varchar(20)));
		set @NewJobId = SCOPE_IDENTITY();
	
	END
	ELSE
	BEGIN
		SET @NewJobId = 0;
	END

	COMMIT TRAN A;
END

SET NOCOUNT OFF
GO