USE [Reviewer]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_RobotApiJobFetchNextCreditTaksByRobotName]') AND type in (N'P', N'PC'))
BEGIN
 --Print 'renaming'
 EXEC sp_rename 'st_RobotApiJobFetchNextCreditTaksByRobotName', 'st_RobotApiJobFetchNextCreditTasksByRobotName';
END
GO

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
	select *, @RobotContactId as ROBOT_CONTACT_ID from TB_ROBOT_API_CALL_LOG 
		where ROBOT_ID = @rID and (STATUS = 'Queued' OR STATUS = 'Paused' OR STATUS = 'Running')
		order by DATE_CREATED asc
END
