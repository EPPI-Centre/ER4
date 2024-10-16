USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupCheckOngoingLog]    Script Date: 11/03/2024 16:49:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[st_MagMatchingCheckOngoingLog] 
	-- Add the parameters for the stored procedure here
	(
		@revID int,
		@customTimeoutInMinutes int = 10
	)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT, XACT_ABORT ON;
	--see https://weblogs.sqlteam.com/dang/2007/10/28/conditional-insertupdate-race-condition/
	--for details on how this is dealing with concurrency problems. Aim is to avoid having two instances of this SP insert
	--SP is called whenever the list of duplicates is retrieved AND when we are asking to find new duplicates...
	
	--paired with the "lasting lock", the transaction prevents two instances to be triggered concurrently
	--without the lasting lock, the TRAN itself won't work, see link above for the details.
	declare @t_cache table (MAG_LOG_ID int primary key, TIME_SUBMITTED datetime null, TIME_UPDATED datetime null,
						JOB_STATUS nvarchar(50), JOB_MESSAGE nvarchar(max))
	declare @revString varchar(100) = 'Review: ' + cast(@revId as varchar(12)) + ', %'
	declare @state nvarchar(50) = ''
	BEGIN TRAN A
	--get the last 200 'MAG matching' jobs in a local cache, to speed up finding those for our review! (speed matters, because of table locking!)
		insert into @t_cache select top 200 MAG_LOG_ID, TIME_SUBMITTED, TIME_UPDATED, JOB_STATUS, JOB_MESSAGE
			from TB_MAG_LOG WITH (TABLOCKX, HOLDLOCK) where JOB_TYPE = 'MAG matching' order by MAG_LOG_ID desc
		--TABLOCKX: exclusive table lock; HOLDLOCK: keep this log on until the whole transaction is done.
	
		--get the last job ID, and acquire an exclusive table lock
		declare @jobid int = (select top 1 MAG_LOG_ID from @t_cache where JOB_MESSAGE like @revString order by MAG_LOG_ID desc)
	
		IF @jobid is null --all good, no chance we have OA matching running for this review
		BEGIN
			COMMIT TRAN A --Have to commit, before we can return!
			return 1 -- "plus one" means all OK matching job is not running for this review
		END
		--how did the last job go?
		set @state = (select JOB_STATUS from @t_cache where MAG_LOG_ID = @jobid)
		if @state = 'Complete' OR @state ='CancelToken(1)!' OR @state = 'CancelToken(2)!' OR @state = 'Abnormally ended'
		BEGIN --last job completed, all good
			COMMIT TRAN A --Have to commit, before we can return!
			return 1
		END

		IF @state = 'Running' OR @state = 'Starting'
		BEGIN
			declare @updated datetime = (select TIME_UPDATED from @t_cache where MAG_LOG_ID = @jobid)
			if @updated is null set @updated = (select TIME_SUBMITTED from @t_cache where MAG_LOG_ID = @jobid)
			if GETDATE() > DATEADD(minute, @customTimeoutInMinutes, @updated)
			BEGIN --work to do, job is flagged as running, but has failed to yeild anything for at least the last @customTimeoutInMinutes
				--we'll mark this job as abnormally ended
				UPDATE TB_MAG_LOG set JOB_STATUS = 'Abnormally ended' 
					, JOB_MESSAGE = 'Last known job message - ' + JOB_MESSAGE
					where MAG_LOG_ID = @jobid
				COMMIT TRAN A --Have to commit, before we can return!
				return 1
			END
			--OH job is actually running, return -1!
			COMMIT TRAN A --Have to commit, before we can return!
			return -1
		END
	--we don't expect this to ever happen (cases above should be exhaustive), but we'll return something that DOES allow to trigger a new job
	COMMIT TRAN A --Have to commit, before we can return!
	return 2

	SET NOCOUNT OFF
END
GO
