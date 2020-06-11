

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupCheckOngoing]    Script Date: 04/06/2020 09:36:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[st_ItemDuplicateGroupCheckOngoingLog] 
	-- Add the parameters for the stored procedure here
	(
		@revID int
	)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--SP is called whenever the list of duplicates is retrieved AND when we are asking to find new duplicates...
	
	--get the last job ID
	declare @jobid int = (select top 1 REVIEW_JOB_ID from tb_REVIEW_JOB where REVIEW_ID = @revID AND JOB_TYPE = 'FindNewDuplicates' order by REVIEW_JOB_ID desc)
	--how did the last job go?
	declare @state nvarchar(50) = ''
	IF @jobid is not null set @state = (select current_state from tb_REVIEW_JOB where REVIEW_JOB_ID = @jobid)
	if @state = '' OR @state = 'Ended'
		BEGIN
			--EITHER never done for this review, or last run worked. From now on, we can assume @jobid is NOT NULL
			return 1
		END
	IF @state = 'Failed' return -3 --this makes the "get new duplicates" part start again.
	IF @state = 'running'
	BEGIN
		--dedup is running for this review, has it been running for too long?
		declare @ended datetime;
		set @ended = (select END_TIME from tb_REVIEW_JOB where REVIEW_JOB_ID = @jobid)
		declare @diff int = DateDiff(MINUTE, @ended, getdate())
		if @diff > 45 --this job has been inactive for 45 minutes, as dedup updates END_TIME regularly, we can assume it was interrupted...
		BEGIN
			--has been marked as running, but hasn't done anything for 45m or more, we'll mark it as failed, check number of failures and return -3 to ask for a rerun or -4 to abort
			update tb_REVIEW_JOB set CURRENT_STATE = 'Failed', SUCCESS = 0, JOB_MESSAGE = 'Automated Failure: inactive for too long' where REVIEW_JOB_ID = @jobid
			--for safety, we'll check that  the last 10 executions were not failures, for this review.
			declare @check int =
							(
								select SUM(A.FAILURES) from
								(select top 10 CASE when CURRENT_STATE = 'failed' then 1 else 0 end as FAILURES from tb_REVIEW_JOB where REVIEW_ID = @revID AND JOB_TYPE = 'FindNewDuplicates') as A
							)
			if @check >= 10 return -4 --this is BAD, we tried 10 times in a row, never succeeded
			else return -3 --this makes the "get new duplicates" part start again.
		END
		return -2; --normal "this is still running" value.
	END
	--We should never reach this, but just in case, we should return something...
	return -4; --signalling that something is WRONG!
	SET NOCOUNT OFF
END
GO
