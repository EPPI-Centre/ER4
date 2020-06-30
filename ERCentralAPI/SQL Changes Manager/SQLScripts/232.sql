
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesGetCandidatesOnSearchText]    Script Date: 02/06/2020 13:55:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER       procedure [dbo].[st_ItemDuplicatesGetCandidatesOnSearchText]
(
      @REVIEW_ID INT,
	  @CONTACT_ID int
)
As

SET NOCOUNT ON
declare @Items table (item_id bigint primary key)
--FIRST of all, log that "find new duplicates" has started
insert into tb_REVIEW_JOB (REVIEW_ID, CONTACT_ID, START_TIME, END_TIME, JOB_TYPE, CURRENT_STATE) 
	VALUES (@REVIEW_ID, @CONTACT_ID, getdate(),getdate(), 'FindNewDuplicates', 'running')


--limit this to ONLY the items we need.
insert into @Items select ir.Item_id from TB_ITEM_REVIEW ir 
where ir.IS_DELETED = 0 and ir.REVIEW_ID = @REVIEW_ID

declare @matches table (item_id bigint, matched bigint, primary key(item_id, matched))

--get JUST the matches, ignore all other data
insert into @matches 
select distinct t.item_id, t2.item_id from @items t
inner join tb_item i on i.ITEM_ID = t.item_id
INNER JOIN TB_ITEM I2 ON I2.SearchText = I.SearchText 
inner join @Items t2 on  I2.ITEM_ID = t2.item_id and t.item_id < t2.item_id

declare @res table (ITEM_ID bigint, ITEM_ID2 bigint , HAS_CODES bit, IS_MASTER bit, HAS_CODES2 bit, IS_MASTER2 bit, searchtext nvarchar(500), searchtext2 nvarchar(500))
--Get the data that needs to be computed and/or used for sorting.
insert into @res 
select        I.ITEM_ID, I2.ITEM_ID ITEM_ID2, 
				  case when ise.item_id is null then 0 else 1 end as HAS_CODES,
				  case when (NOT DG.MASTER_MEMBER_ID IS NULL AND (idg.GROUP_MEMBER_ID = DG.MASTER_MEMBER_ID))
					then 1 else 0 end as IS_MASTER,
				  case when ise2.item_id is null then 0 else 1 end as HAS_CODES2,
					case when (NOT DG2.MASTER_MEMBER_ID IS NULL AND (idg2.GROUP_MEMBER_ID = DG2.MASTER_MEMBER_ID))
					then 1 else 0 end as IS_MASTER2,
				i.SearchText, i2.SearchText
				  
	from @matches m
	    inner join TB_ITEM I on i.ITEM_ID = m.item_id
		INNER JOIN TB_ITEM I2 ON I2.ITEM_ID = m.matched
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = i.ITEM_ID
		INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = I2.ITEM_ID

		LEFT OUTER JOIN TB_ITEM_SET ISE ON ISE.ITEM_ID = I.ITEM_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS IDG ON IDG.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP DG ON DG.ITEM_DUPLICATE_GROUP_ID = IDG.ITEM_DUPLICATE_GROUP_ID

		LEFT OUTER JOIN TB_ITEM_SET ISE2 ON ISE2.ITEM_ID = I2.ITEM_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS IDG2 ON IDG2.ITEM_REVIEW_ID = IR2.ITEM_REVIEW_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP DG2 ON DG2.ITEM_DUPLICATE_GROUP_ID = IDG2.ITEM_DUPLICATE_GROUP_ID

		where 
		--ir.IS_DELETED = 'False' and ir.REVIEW_ID = @REVIEW_ID 
		--	and ir2.IS_DELETED = 'False' and ir2.REVIEW_ID = @REVIEW_ID
		--	and i.ITEM_ID != I2.ITEM_ID and i.ITEM_ID < i2.ITEM_ID
			
		--	and 
			(idg.GROUP_MEMBER_ID is null or (NOT DG.MASTER_MEMBER_ID IS NULL
				AND (idg.GROUP_MEMBER_ID = DG.MASTER_MEMBER_ID)))
			and (idg2.GROUP_MEMBER_ID is null or (NOT DG2.MASTER_MEMBER_ID IS NULL
				AND (idg2.GROUP_MEMBER_ID = DG2.MASTER_MEMBER_ID)))


--finally, get the results, data from @res, plus additional field in TB_ITEM (twice), we can now "sort by" quickly, as all data is at hand.
select I.ITEM_ID, I2.ITEM_ID ITEM_ID2, I.TITLE, I2.TITLE TITLE2,
				[dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
                   I.PARENT_TITLE, I.[YEAR], I.VOLUME, I.PAGES, I.ISSUE, I.ABSTRACT,
				  I.DOI, 
				  CAST(HAS_CODES as int) as HAS_CODES, CAST(IS_MASTER as int) as IS_MASTER,
		[dbo].fn_REBUILD_AUTHORS(I2.ITEM_ID, 0) as AUTHORS2,
                   I2.PARENT_TITLE PARENT_TITLE2, I2.[YEAR] YEAR2, I2.VOLUME VOLUME2, I2.PAGES PAGES2,
				   I2.ISSUE ISSUE2, I2.ABSTRACT ABSTRACT2,
				  I2.DOI DOI2,
				  CAST(HAS_CODES2 as int) as HAS_CODES2, CAST(IS_MASTER2 as int) as IS_MASTER2,
		r.searchtext, r.searchtext2

	 from @res r
		inner join tb_item i on r.ITEM_ID = i.ITEM_ID
		inner join tb_item i2 on r.ITEM_ID2 = i2.ITEM_ID
order by r.SearchText, IS_MASTER, IS_MASTER2, I.ITEM_ID, i2.ITEM_ID
		
SET NOCOUNT OFF
GO

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
			update tb_REVIEW_JOB set CURRENT_STATE = 'failed' where REVIEW_JOB_ID = @jobid
			--for safety, we'll check that  the last 10 executions were not failures, for this review.
			declare @check int =
							(
								select SUM(A.FAILURES) from
								(select top 10 CASE when CURRENT_STATE = 'failed' then 1 else 0 end as FAILURES from tb_REVIEW_JOB where REVIEW_ID = @revID AND JOB_TYPE = 'FindNewDuplicates') as A
							)
			if @check >= 4 return -4 --this is BAD, we tried 4 times in a row, never succeeded
			else return -3 --this makes the "get new duplicates" part start again.
		END
		return -2; --normal "this is still running" value.
	END
	--We should never reach this, but just in case, we should return something...
	return -4; --signalling that something is WRONG!
	SET NOCOUNT OFF
END
GO
IF COL_LENGTH('dbo.tb_REVIEW_JOB', 'JOB_MESSAGE') IS NULL
BEGIN
	BEGIN TRANSACTION
	SET QUOTED_IDENTIFIER ON
	SET ARITHABORT ON
	SET NUMERIC_ROUNDABORT OFF
	SET CONCAT_NULL_YIELDS_NULL ON
	SET ANSI_NULLS ON
	SET ANSI_PADDING ON
	SET ANSI_WARNINGS ON
	COMMIT
	BEGIN TRANSACTION
	ALTER TABLE dbo.tb_REVIEW_JOB ADD
		JOB_MESSAGE nvarchar(4000) NULL
	ALTER TABLE dbo.tb_REVIEW_JOB SET (LOCK_ESCALATION = TABLE)
	COMMIT
END
GO

CREATE OR ALTER PROCEDURE [dbo].[st_ItemDuplicateGroupsMarkAsDoneChecking] 
	-- Add the parameters for the stored procedure here
	(
		@revID int,
		@state nvarchar(50),
		@message nvarchar(4000) = ''
	)
AS
BEGIN
	SET NOCOUNT ON
	update tb_REVIEW_JOB set 
		CURRENT_STATE = @state
		, END_TIME = GETDATE()
		, JOB_MESSAGE = @message 
		, SUCCESS = CASE when @state = 'Ended' THEN 1 else 0 END
	where REVIEW_ID = @revID AND JOB_TYPE = 'FindNewDuplicates' and CURRENT_STATE = 'running'
	SET NOCOUNT OFF
END
GO


--CLEANUP of unneded things
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemDuplicateGroupCreateNew]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ItemDuplicateGroupCreateNew]
--GO
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemDuplicateGroupAddAddionalItem]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ItemDuplicateGroupAddAddionalItem]
--GO

--WE could remove TB_ITEM_DUPLICATES as well...


CREATE OR ALTER PROCEDURE [dbo].[st_ItemDuplicateGroupCreateFromERNativeResults] 
	-- Add the parameters for the stored procedure here
	(
		@revID int,
		@input CREATE_GROUPS_INPUT_TB READONLY
	)
AS
BEGIN
	SET NOCOUNT ON

	declare @work table (MASTER_ID bigint, member_id bigint, IsNew bit, score float, destination int, primary key (MASTER_ID, member_id))
	insert into @work (master_id, member_id, IsNew, score) select * from @input

	--1. create groups
	Insert into TB_ITEM_DUPLICATE_GROUP (REVIEW_ID, ORIGINAL_ITEM_ID) 
		select distinct @revID, master_id from @work where IsNew = 1
			and master_id not in (SELECT ORIGINAL_ITEM_ID from TB_ITEM_DUPLICATE_GROUP g where g.REVIEW_ID = @revID) --sanity clause, avoid producing 2 groups with the same master
	--2. find and record what goes where
	update @work set destination = a.ITEM_DUPLICATE_GROUP_ID 
		from (select ITEM_DUPLICATE_GROUP_ID, w.MASTER_ID as mid from TB_ITEM_DUPLICATE_GROUP g 
				inner join @work w on g.REVIEW_ID = @revID and w.MASTER_ID = g.ORIGINAL_ITEM_ID) a
		where MASTER_ID = a.mid
	--select * from @work
	--3. create master records
	insert into TB_ITEM_DUPLICATE_GROUP_MEMBERS	(ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID, SCORE, IS_CHECKED, IS_DUPLICATE) 
				select distinct destination, ITEM_REVIEW_ID, 1, 1, 0 from @work w
						inner join TB_ITEM_REVIEW ir on ir.REVIEW_ID = @revID and w.MASTER_ID = ir.ITEM_ID and w.IsNew = 1
						inner join TB_ITEM_DUPLICATE_GROUP g on ir.ITEM_ID = g.ORIGINAL_ITEM_ID
	--set the MASTER_MEMBER_ID
	update TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = a.GROUP_MEMBER_ID from 
		( select distinct GROUP_MEMBER_ID, ITEM_DUPLICATE_GROUP_ID from @work w 
			inner join TB_ITEM_REVIEW ir on w.MASTER_ID = ir.ITEM_ID and ir.REVIEW_ID = @revID and w.IsNew = 1
			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS m on w.destination = m.ITEM_DUPLICATE_GROUP_ID
		) a
		where TB_ITEM_DUPLICATE_GROUP.ITEM_DUPLICATE_GROUP_ID = a.ITEM_DUPLICATE_GROUP_ID and TB_ITEM_DUPLICATE_GROUP.MASTER_MEMBER_ID is null
	--4. create members. From here, we don't care about @work.IsNew = 1
	insert into TB_ITEM_DUPLICATE_GROUP_MEMBERS	(ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID, SCORE, IS_CHECKED, IS_DUPLICATE) 
				select destination, ITEM_REVIEW_ID, score, 0, 0 from @work w
						inner join TB_ITEM_REVIEW ir on ir.REVIEW_ID = @revID and w.member_id = ir.ITEM_ID 
	--Signal progress in tb_REVIEW_JOB
	update tb_REVIEW_JOB set 
		END_TIME = GETDATE()
	where REVIEW_ID = @revID AND JOB_TYPE = 'FindNewDuplicates' and CURRENT_STATE = 'running'
	SET nocount off
END
GO

