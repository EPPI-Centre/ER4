USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_RobotApiTopQueuedJobs]    Script Date: 30/01/2025 16:29:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[st_RobotApiPastJobs] 
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
			([STATUS] = 'Finished' OR [STATUS] = 'Failed' OR [STATUS] = 'Failed(automated)')
			order by ROBOT_API_CALL_ID desc;
		SELECT e.* from TB_ROBOT_API_CALL_LOG l
			inner join TB_ROBOT_API_CALL_ERROR_LOG e on l.ROBOT_API_CALL_ID = e.ROBOT_API_CALL_ID
			where ROBOT_ID = @rID AND
			(REVIEW_ID = @REVIEW_ID OR CONTACT_ID = @CONTACT_ID)
			AND
			([STATUS] = 'Finished' OR [STATUS] = 'Failed' OR [STATUS] = 'Failed(automated)')
			order by l.ROBOT_API_CALL_ID desc;
	END
	else
	BEGIN
		declare @chk int = (SELECT count(*) from TB_CONTACT where CONTACT_ID = @CONTACT_ID and IS_SITE_ADMIN = 1)
		if @chk < 1 return;
		SELECT l.*, @RobotContactId as ROBOT_CONTACT_ID, c.CONTACT_NAME from TB_ROBOT_API_CALL_LOG l
			inner join TB_CONTACT c on l.CONTACT_ID = c.CONTACT_ID
			where ROBOT_ID = @rID AND
			([STATUS] = 'Finished' OR [STATUS] = 'Failed' OR [STATUS] = 'Failed(automated)')
			order by ROBOT_API_CALL_ID desc;
		SELECT e.* from TB_ROBOT_API_CALL_LOG l
			inner join TB_ROBOT_API_CALL_ERROR_LOG e on l.ROBOT_API_CALL_ID = e.ROBOT_API_CALL_ID
			where ROBOT_ID = @rID AND
			([STATUS] = 'Finished' OR [STATUS] = 'Failed' OR [STATUS] = 'Failed(automated)')
			order by l.ROBOT_API_CALL_ID desc;
	END

END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_RobotApiJobFetchNextCreditTasksByRobotName]    Script Date: 04/02/2025 10:45:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[st_RobotApiJobFetchNextCreditTasksByRobotName]    Script Date: 09/12/2024 11:22:57 ******/
ALTER   PROCEDURE [dbo].[st_RobotApiJobFetchNextCreditTasksByRobotName] 
(
	@ROBOT_NAME nvarchar(255)
)
AS
BEGIN
	declare @rID int = (select ROBOT_ID from TB_CONTACT c 
							Inner join TB_ROBOT_ACCOUNT rc on c.CONTACT_ID = rc.CONTACT_ID and c.CONTACT_NAME = @ROBOT_NAME
							AND c.EMAIL like '%FAKE@ucl.ac.uk' AND c.EXPIRY_DATE is null);
	declare @RobotContactId int = (select CONTACT_ID from TB_ROBOT_ACCOUNT where ROBOT_ID = @rID)
	select *, @RobotContactId as ROBOT_CONTACT_ID, CONTACT_NAME from TB_ROBOT_API_CALL_LOG l
		inner join TB_CONTACT c on l.CONTACT_ID = c.CONTACT_ID
		where ROBOT_ID = @rID and (STATUS = 'Queued' OR STATUS = 'Paused' OR STATUS = 'Running')
		order by l.DATE_CREATED asc
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_RobotApiTopQueuedJobs]    Script Date: 04/02/2025 10:52:39 ******/
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
		([STATUS] != 'Finished' AND [STATUS] != 'Failed' AND [STATUS] != 'Failed(automated)')
		order by ROBOT_API_CALL_ID desc;
	
END
GO