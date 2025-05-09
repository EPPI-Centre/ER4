USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_RobotApiPastJobs]    Script Date: 06/02/2025 15:34:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_RobotApiPastJobs] 
(
	@ROBOT_NAME nvarchar(255),
	@REVIEW_ID int,
	@CONTACT_ID int,
	@isSiteAdmin bit = 0
)
AS
BEGIN
	declare @rID int = (select ROBOT_ID from TB_CONTACT c 
							Inner join TB_ROBOT_ACCOUNT rc on c.CONTACT_ID = rc.CONTACT_ID and c.CONTACT_NAME = @ROBOT_NAME
							AND c.EMAIL like '%FAKE@ucl.ac.uk' AND c.EXPIRY_DATE is null);
	IF @rID is null OR @rID < 1
	BEGIN
		set @rID = -1;
	END
	declare @RobotContactId int = (select CONTACT_ID from TB_ROBOT_ACCOUNT where ROBOT_ID = @rID)
	if @isSiteAdmin = 0
	begin
		SELECT l.*, @RobotContactId as ROBOT_CONTACT_ID, c.CONTACT_NAME from TB_ROBOT_API_CALL_LOG l
			inner join TB_CONTACT c on l.CONTACT_ID = c.CONTACT_ID
			where ROBOT_ID = @rID AND
			(REVIEW_ID = @REVIEW_ID OR l.CONTACT_ID = @CONTACT_ID)
			AND
			([STATUS] = 'Finished' OR [STATUS] = 'Failed' OR [STATUS] = 'Failed(automated)' OR [STATUS] = 'Cancelled(manually)')
			order by ROBOT_API_CALL_ID desc;
		SELECT e.* from TB_ROBOT_API_CALL_LOG l
			inner join TB_ROBOT_API_CALL_ERROR_LOG e on l.ROBOT_API_CALL_ID = e.ROBOT_API_CALL_ID
			where ROBOT_ID = @rID AND
			(REVIEW_ID = @REVIEW_ID OR CONTACT_ID = @CONTACT_ID)
			AND
			([STATUS] = 'Finished' OR [STATUS] = 'Failed' OR [STATUS] = 'Failed(automated)' OR [STATUS] = 'Cancelled(manually)')
			order by l.ROBOT_API_CALL_ID desc;
	END
	else
	BEGIN
		declare @chk int = (SELECT count(*) from TB_CONTACT where CONTACT_ID = @CONTACT_ID and IS_SITE_ADMIN = 1)
		if @chk < 1 return;
		SELECT l.*, @RobotContactId as ROBOT_CONTACT_ID, c.CONTACT_NAME from TB_ROBOT_API_CALL_LOG l
			inner join TB_CONTACT c on l.CONTACT_ID = c.CONTACT_ID
			where ROBOT_ID = @rID AND
			([STATUS] = 'Finished' OR [STATUS] = 'Failed' OR [STATUS] = 'Failed(automated)' OR [STATUS] = 'Cancelled(manually)')
			order by ROBOT_API_CALL_ID desc;
		SELECT e.* from TB_ROBOT_API_CALL_LOG l
			inner join TB_ROBOT_API_CALL_ERROR_LOG e on l.ROBOT_API_CALL_ID = e.ROBOT_API_CALL_ID
			where ROBOT_ID = @rID AND
			([STATUS] = 'Finished' OR [STATUS] = 'Failed' OR [STATUS] = 'Failed(automated)' OR [STATUS] = 'Cancelled(manually)')
			order by l.ROBOT_API_CALL_ID desc;
	END

END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_RobotApiTopQueuedJobs]    Script Date: 06/02/2025 15:36:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER     PROCEDURE [dbo].[st_RobotApiTopQueuedJobs] 
(
	@ROBOT_NAME nvarchar(255)
)
AS
BEGIN
	declare @rID int = (select ROBOT_ID from TB_CONTACT c 
							Inner join TB_ROBOT_ACCOUNT rc on c.CONTACT_ID = rc.CONTACT_ID and c.CONTACT_NAME = @ROBOT_NAME
							AND c.EMAIL like '%FAKE@ucl.ac.uk' AND c.EXPIRY_DATE is null);
	IF @rID is null OR @rID < 1
	BEGIN
		set @rID = -1;
	END
	declare @RobotContactId int = (select CONTACT_ID from TB_ROBOT_ACCOUNT where ROBOT_ID = @rID)
	SELECT top 5000 *, @RobotContactId as ROBOT_CONTACT_ID, CONTACT_NAME from TB_ROBOT_API_CALL_LOG l
		inner join TB_CONTACT c on c.CONTACT_ID = l.CONTACT_ID
		where ROBOT_ID = @rID AND
		([STATUS] != 'Finished' AND [STATUS] != 'Failed' AND [STATUS] != 'Failed(automated)'  AND [STATUS] != 'Cancelled(manually)')
		order by ROBOT_API_CALL_ID desc;
	
END
GO

CREATE or ALTER   PROCEDURE [dbo].[st_RobotOpenAiCancelQueuedBatchJob] 
(
	@ROBOT_API_CALL_ID int,
	@REVIEW_ID int,
	@CONTACT_ID int,
	@isSiteAdmin bit = 0,
	@ReturnValue int = 0 output
)
AS
BEGIN
	declare @chk int;
	declare @validated_job_id int;
	set @ReturnValue = 0;
	If @isSiteAdmin = 1
	BEGIN
		set @chk = (SELECT count(*) from TB_CONTACT where CONTACT_ID = @CONTACT_ID and IS_SITE_ADMIN = 1);
		if @chk < 1 return;
		set @validated_job_id = @ROBOT_API_CALL_ID;
	END
	ELSE
	BEGIN
		--not a site admin, check that they have access to the job (either own it, or are currently logged in the review of the job)
		select @validated_job_id = ROBOT_API_CALL_ID from TB_ROBOT_API_CALL_LOG where ROBOT_API_CALL_ID = @ROBOT_API_CALL_ID and (REVIEW_ID = @REVIEW_ID OR CONTACT_ID = @CONTACT_ID)
	END
	if @validated_job_id is null OR @validated_job_id != @ROBOT_API_CALL_ID return;

	UPDATE TB_ROBOT_API_CALL_LOG set [STATUS] = 'Cancelled(manually)', SUCCESS = 0 where ROBOT_API_CALL_ID = @validated_job_id and [STATUS] = 'Queued'
	set @ReturnValue = @@ROWCOUNT
END
GO