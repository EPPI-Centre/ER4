USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierCanRunCheckAndMarkAsStarting]    Script Date: 20/11/2026 14:20:53 ******/
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
	,@JobType varchar(5) = 'Apply' --"Apply", "get_pdfs", "Build" or "ChckS" (for "Check Screening") or "PrioS" for priority screening simulation
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
	ELSE IF @JobType = 'pdfs' set @JobT = 'Fetch PDFs';
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
		IF @JobType = 'Build' AND @TITLE != ''
			update tb_CLASSIFIER_MODEL set MODEL_TITLE = @TITLE WHERE MODEL_ID = @MODEL_ID
		
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

IF COL_LENGTH(N'dbo.TB_CLASSIFIER_MODEL', N'USED_PROMPTS') IS NULL
BEGIN
    ALTER TABLE dbo.TB_CLASSIFIER_MODEL
    ADD USED_PROMPTS bit NULL;
END;
GO
IF COL_LENGTH(N'dbo.TB_CLASSIFIER_MODEL', N'MODEL_PERFORMANCE') IS NULL
BEGIN
    ALTER TABLE dbo.TB_CLASSIFIER_MODEL
    ADD MODEL_PERFORMANCE nvarchar(max) NULL;
END;
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierGet]    Script Date: 24/11/2026 22:10:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER    procedure [dbo].[st_ClassifierGet]
(
	@MODEL_ID INT,
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	SELECT MODEL_ID, MODEL_TITLE, CONTACT_ID, REVIEW_ID, ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, ACCURACY, AUC,
		[PRECISION], RECALL, ML_MODEL_NAME, USED_PROMPTS, MODEL_PERFORMANCE
		FROM tb_CLASSIFIER_MODEL
		WHERE MODEL_ID = @MODEL_ID
		AND REVIEW_ID = @REVIEW_ID



SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierContactModels]    Script Date: 24/11/2026 22:11:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_ClassifierContactModels]
(
	@CONTACT_ID INT
)

As

SET NOCOUNT ON
declare @chk int = (select count(contact_id) from TB_CONTACT where CONTACT_ID = @CONTACT_ID and IS_SITE_ADMIN = 1)
if (@chk = 1) --user is a site admin, current review might not have it as a member, so we need to do more work
 BEGIN
  declare @RID int = (select Review_id from sTB_LOGON_TICKET 
						where [STATE] = 1 and CONTACT_ID = @CONTACT_ID
							and LAST_RENEWED > DATEADD(hour, -1, getdate()))
  if @RID is null return
  --select @rid
  declare @rids table (REVIEW_ID int)
  insert into @rids select @rid
  insert into @rids select REVIEW_ID from TB_REVIEW_CONTACT rc where CONTACT_ID = @CONTACT_ID and rc.REVIEW_ID != @RID
  select MODEL_ID, MODEL_TITLE, CM.REVIEW_ID, R.REVIEW_NAME, 
	A1.ATTRIBUTE_NAME ATTRIBUTE_ON, A2.ATTRIBUTE_NAME ATTRIBUTE_NOT_ON,
	 ATTRIBUTE_ID_NOT_ON, ATTRIBUTE_ID_ON, ML_MODEL_NAME, USED_PROMPTS, MODEL_PERFORMANCE,
		CONTACT_NAME, CM.CONTACT_ID, ACCURACY, AUC, [PRECISION], RECALL
		from tb_CLASSIFIER_MODEL CM
	INNER JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = CM.ATTRIBUTE_ID_ON
	INNER JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = CM.ATTRIBUTE_ID_NOT_ON
	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = CM.CONTACT_ID
	INNER JOIN @rids rid on rid.REVIEW_ID = cm.REVIEW_ID
	inner join TB_REVIEW r on rid.REVIEW_ID = r.REVIEW_ID 
	--where RC.CONTACT_ID = @CONTACT_ID
	order by CM.REVIEW_ID, cm.MODEL_ID
 END

 ELSE
 BEGIN
	select MODEL_ID, MODEL_TITLE, CM.REVIEW_ID, R.REVIEW_NAME, 
	A1.ATTRIBUTE_NAME ATTRIBUTE_ON, A2.ATTRIBUTE_NAME ATTRIBUTE_NOT_ON,
	 ATTRIBUTE_ID_NOT_ON, ATTRIBUTE_ID_ON, ML_MODEL_NAME, USED_PROMPTS, MODEL_PERFORMANCE,
		CONTACT_NAME, CM.CONTACT_ID, ACCURACY, AUC, [PRECISION], RECALL
		from tb_CLASSIFIER_MODEL CM
	INNER JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = CM.ATTRIBUTE_ID_ON
	INNER JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = CM.ATTRIBUTE_ID_NOT_ON
	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = CM.CONTACT_ID
	INNER JOIN TB_REVIEW_CONTACT RC ON RC.REVIEW_ID = CM.REVIEW_ID
	INNER JOIN TB_REVIEW R ON R.REVIEW_ID = RC.REVIEW_ID
	
	where RC.CONTACT_ID = @CONTACT_ID
	order by CM.REVIEW_ID, MODEL_ID
END
SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierModels]    Script Date: 24/11/2026 22:13:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ClassifierModels]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	select MODEL_ID, MODEL_TITLE, REVIEW_ID, A1.ATTRIBUTE_NAME ATTRIBUTE_ON, A2.ATTRIBUTE_NAME ATTRIBUTE_NOT_ON,
		CONTACT_NAME, tb_CLASSIFIER_MODEL.CONTACT_ID, ACCURACY, AUC, [PRECISION], RECALL,
		ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, ML_MODEL_NAME, USED_PROMPTS, MODEL_PERFORMANCE
		from tb_CLASSIFIER_MODEL
	INNER JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_ON
	INNER JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_NOT_ON
	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = tb_CLASSIFIER_MODEL.CONTACT_ID
	
	where REVIEW_ID = @REVIEW_ID
	order by MODEL_ID

SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierUpdateModel]    Script Date: 24/11/2026 22:13:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ClassifierUpdateModel]
(
	@MODEL_ID INT
,	@TITLE nvarchar(1000) = NULL
,	@ACCURACY DECIMAL(18,18) = NULL
,	@AUC DECIMAL(18,18) = NULL
,	@PRECISION DECIMAL(18,18) = NULL
,	@RECALL DECIMAL(18,18) = NULL
,	@MODEL_PERFORMANCE NVARCHAR(MAX) = NULL
,	@USED_PROMPTS bit = NULL
,	@CHECK_MODEL_ID_EXISTS INT OUTPUT
)

As

SET NOCOUNT ON

SELECT @CHECK_MODEL_ID_EXISTS = COUNT(*) FROM tb_CLASSIFIER_MODEL WHERE MODEL_ID = @MODEL_ID

IF (@CHECK_MODEL_ID_EXISTS = 1)
BEGIN

	update tb_CLASSIFIER_MODEL
		SET TIME_ENDED = CURRENT_TIMESTAMP,
		MODEL_TITLE = @TITLE,
		ACCURACY = @ACCURACY,
		AUC = @AUC,
		[PRECISION] = @PRECISION,
		RECALL = @RECALL,
		MODEL_PERFORMANCE = @MODEL_PERFORMANCE,
		USED_PROMPTS = @USED_PROMPTS
		
	WHERE MODEL_ID = @MODEL_ID
END

SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierSaveModel]    Script Date: 24/11/2026 22:15:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER procedure [dbo].[st_ClassifierSaveModel]
(
	@REVIEW_ID INT
,	@ATTRIBUTE_ID_ON BIGINT = NULL
,	@ATTRIBUTE_ID_NOT_ON BIGINT = NULL
,	@CONTACT_ID INT
,	@MODEL_TITLE NVARCHAR(1000) = NULL
,	@ML_MODEL_NAME VARCHAR(10)
,	@NEW_MODEL_ID INT OUTPUT
,	@USED_PROMPTS BIT = 0
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
		INSERT INTO tb_CLASSIFIER_MODEL(MODEL_TITLE, CONTACT_ID, REVIEW_ID, ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, TIME_STARTED, TIME_ENDED, ML_MODEL_NAME, USED_PROMPTS)
			VALUES(@MODEL_TITLE, @CONTACT_ID, @REVIEW_ID, @ATTRIBUTE_ID_ON, @ATTRIBUTE_ID_NOT_ON, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, @ML_MODEL_NAME, @USED_PROMPTS);
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

		INSERT INTO tb_CLASSIFIER_MODEL(MODEL_TITLE, CONTACT_ID, REVIEW_ID, ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, TIME_STARTED, TIME_ENDED, ML_MODEL_NAME, USED_PROMPTS)
			VALUES(@MODEL_TITLE, @CONTACT_ID, @REVIEW_ID, @ATTRIBUTE_ID_ON, @ATTRIBUTE_ID_NOT_ON, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, @ML_MODEL_NAME, @USED_PROMPTS);
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



