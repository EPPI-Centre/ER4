USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierCanRunCheckAndMarkAsStarting]    Script Date: 04/02/2025 21:00:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   procedure [dbo].[st_ClassifierCanRunCheckAndMarkAsStarting]
(
	@MODEL_ID INT
	,@REVIEW_ID int
	,@REVIEW_ID_OF_MODEL int
	,@CONTACT_ID int
	,@TITLE nvarchar(1000) = ''
	,@JobType varchar(5) = 'Apply' --"Apply", "Build" or "ChckS" (for "Check Screening") or "PrioS" for priority screening simulation
	,@NewJobId int = 0 output
)

As

SET NOCOUNT ON
BEGIN

	BEGIN TRAN A;
	if @MODEL_ID > 0
	BEGIN
		declare @check int = (select count(*) from tb_CLASSIFIER_MODEL where MODEL_ID = @MODEL_ID and REVIEW_ID = @REVIEW_ID_OF_MODEL)
		if @check != 1
		BEGIN
			--check failed, we didn't find the right model for this review
			set @NewJobId = 0;--means no job can start, right now
			COMMIT TRAN A;
			return;
		END
	END
	declare @JobT nvarchar(50) = ''
	IF @JobType = 'Apply' 
	BEGIN
		if @TITLE = '[Apply to OpenAlex Auto Update]' set @JobT = 'Apply Classifier to OA run';
		ELSE set @JobT = 'Apply Classifier';
	END
	ELSE IF @JobType = 'Build' set @JobT = 'Build Classifier';
	ELSE IF @JobType = 'ChckS' set @JobT = 'Check Screening';
	ELSE IF @JobType = 'PrioS' set @JobT = 'Priority screening simulation';
	ELSE 
	BEGIN
		--don't know what to do, we'll make this fail
			set @NewJobId = 0;--means no job can start, right now
			COMMIT TRAN A;
			return;
	END

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
		IF @JobType = 'Build' AND @TITLE != '' update tb_CLASSIFIER_MODEL set MODEL_TITLE = @TITLE WHERE MODEL_ID = @MODEL_ID
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

		IF @JobType = 'Build' AND @TITLE != '' update tb_CLASSIFIER_MODEL set MODEL_TITLE = @TITLE WHERE MODEL_ID = @MODEL_ID
		insert into tb_REVIEW_JOB (REVIEW_ID, CONTACT_ID, START_TIME, END_TIME, JOB_TYPE, CURRENT_STATE, JOB_MESSAGE) 
				VALUES (@REVIEW_ID, @CONTACT_ID, getdate(),getdate(), @JobT, 'Starting', 'ModelId:' + CAST(@MODEL_ID as varchar(20)));
		set @NewJobId = SCOPE_IDENTITY();
	
	END
	ELSE --must be: @state = 1, so there is a job already running
	BEGIN
		SET @NewJobId = 0;
	END

	COMMIT TRAN A;
END

SET NOCOUNT OFF

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRunResults]    Script Date: 13/02/2025 08:43:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdateRunResults] 
	-- Add the parameters for the stored procedure here
	
	@MagAutoUpdateRunId int
,	@OrderBy nvarchar(19)
,	@AutoUpdateScore float
,	@StudyTypeClassifierScore float
,	@UserClassifierScore float
,	@TopN int = 100000 -- basically everything if not specified
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

if @OrderBy = 'AutoUpdate'
begin
	select top(@TopN) PaperId from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId and
			ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by ContReviewScore desc
end
else if @OrderBy = 'StudyTypeClassifier'
begin
	select top(@TopN) PaperId from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId and
			ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by StudyTypeClassifierScore desc
end
else if @OrderBy = 'UserClassifier'
begin
	select top(@TopN) PaperId from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId and
			ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by UserClassifierScore desc
end
END

GO