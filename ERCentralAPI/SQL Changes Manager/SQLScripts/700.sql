Use Reviewer
GO


IF COL_LENGTH('dbo.tb_CLASSIFIER_MODEL', 'ML_MODEL_NAME') IS NULL
BEGIN
	ALTER TABLE dbo.tb_CLASSIFIER_MODEL ADD
	ML_MODEL_NAME VARCHAR(10) not NULL
		CONSTRAINT D_TB_CLASSIFIER_MODEL_ML_MODEL_NAME
		DEFAULT 'oldLogReg'
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierSaveModel]    Script Date: 25/07/2025 09:18:01 ******/
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
		INSERT INTO tb_CLASSIFIER_MODEL(MODEL_TITLE, CONTACT_ID, REVIEW_ID, ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, TIME_STARTED, TIME_ENDED, ML_MODEL_NAME)
			VALUES(@MODEL_TITLE, @CONTACT_ID, @REVIEW_ID, @ATTRIBUTE_ID_ON, @ATTRIBUTE_ID_NOT_ON, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, @ML_MODEL_NAME);
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

		INSERT INTO tb_CLASSIFIER_MODEL(MODEL_TITLE, CONTACT_ID, REVIEW_ID, ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, TIME_STARTED, TIME_ENDED, ML_MODEL_NAME)
			VALUES(@MODEL_TITLE, @CONTACT_ID, @REVIEW_ID, @ATTRIBUTE_ID_ON, @ATTRIBUTE_ID_NOT_ON, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, @ML_MODEL_NAME);
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

