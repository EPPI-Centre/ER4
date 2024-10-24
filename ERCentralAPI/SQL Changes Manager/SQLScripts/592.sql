USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_RobotApiCreateQueuedJob]    Script Date: 24/05/2024 14:13:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[st_RobotApiTopQueuedJobs] 
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
	SELECT top 5000 *, @RobotContactId as ROBOT_CONTACT_ID from TB_ROBOT_API_CALL_LOG 
		where ROBOT_ID = @rID AND
		([STATUS] != 'Finished' AND [STATUS] != 'Failed' AND [STATUS] != 'Failed(automated)')
		order by ROBOT_API_CALL_ID desc;
	
END
GO
--st_RobotApiCallLogMarkOldJobsAsFailed
CREATE OR ALTER   PROCEDURE st_RobotApiCallLogMarkOldJobsAsFailed
AS
BEGIN
	declare @CutoffDate datetime = (select top 1 DATE_UPDATED from TB_ROBOT_API_CALL_LOG where [STATUS] = 'Finished' order by ROBOT_API_CALL_ID desc);
	set @CutoffDate = DATEADD(MINUTE, -5, @CutoffDate);
	UPDATE TB_ROBOT_API_CALL_LOG set [STATUS] = 'Failed(automated)' where DATE_UPDATED < @CutoffDate and ([STATUS] = 'Starting' or [STATUS] = 'Running')
END
GO