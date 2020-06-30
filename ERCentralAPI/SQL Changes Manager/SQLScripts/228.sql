---------------ORIGINAL SP By JAMES----------------------------------------------------------
--USE [Reviewer]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesGetCandidatesOnSearchText]    Script Date: 02/06/2020 13:55:03 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--ALTER       procedure [dbo].[st_ItemDuplicatesGetCandidatesOnSearchText]
--(
--      @REVIEW_ID INT
--)
--As

--SET NOCOUNT ON

--	select        I.ITEM_ID, I2.ITEM_ID ITEM_ID2, I.TITLE, I2.TITLE TITLE2,
--				[dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
--                   I.PARENT_TITLE, I.[YEAR], I.VOLUME, I.PAGES, I.ISSUE, I.ABSTRACT,
--				  I.DOI, case when ise.item_id is null then 0 else 1 end as HAS_CODES,
--				  case when (NOT DG.MASTER_MEMBER_ID IS NULL AND (idg.GROUP_MEMBER_ID = DG.MASTER_MEMBER_ID))
--					then 1 else 0 end as IS_MASTER,

--				 [dbo].fn_REBUILD_AUTHORS(I2.ITEM_ID, 0) as AUTHORS2,
--                   I2.PARENT_TITLE PARENT_TITLE2, I2.[YEAR] YEAR2, I2.VOLUME VOLUME2, I2.PAGES PAGES2,
--				   I2.ISSUE ISSUE2, I2.ABSTRACT ABSTRACT2,
--				  I2.DOI DOI2,
--				  case when ise2.item_id is null then 0 else 1 end as HAS_CODES2,
--				case when (NOT DG2.MASTER_MEMBER_ID IS NULL AND (idg2.GROUP_MEMBER_ID = DG2.MASTER_MEMBER_ID))
--				then 1 else 0 end as IS_MASTER2,

--				i.SearchText, i2.SearchText
				  
--	from TB_ITEM I
--		INNER JOIN TB_ITEM I2 ON I2.SearchText = I.SearchText
--		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = i.ITEM_ID
--		INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = I2.ITEM_ID

--		LEFT OUTER JOIN TB_ITEM_SET ISE ON ISE.ITEM_ID = I.ITEM_ID
--		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS IDG ON IDG.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
--		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP DG ON DG.ITEM_DUPLICATE_GROUP_ID = IDG.ITEM_DUPLICATE_GROUP_ID

--		LEFT OUTER JOIN TB_ITEM_SET ISE2 ON ISE2.ITEM_ID = I2.ITEM_ID
--		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS IDG2 ON IDG2.ITEM_REVIEW_ID = IR2.ITEM_REVIEW_ID
--		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP DG2 ON DG2.ITEM_DUPLICATE_GROUP_ID = IDG2.ITEM_DUPLICATE_GROUP_ID

--		where ir.IS_DELETED = 'False' and ir.REVIEW_ID = @REVIEW_ID 
--			and ir2.IS_DELETED = 'False' and ir2.REVIEW_ID = @REVIEW_ID
--			and i.ITEM_ID != I2.ITEM_ID and i.ITEM_ID < i2.ITEM_ID
			
--			and (idg.GROUP_MEMBER_ID is null or (NOT DG.MASTER_MEMBER_ID IS NULL
--				AND (idg.GROUP_MEMBER_ID = DG.MASTER_MEMBER_ID)))
--			and (idg2.GROUP_MEMBER_ID is null or (NOT DG2.MASTER_MEMBER_ID IS NULL
--				AND (idg2.GROUP_MEMBER_ID = DG2.MASTER_MEMBER_ID)))
--		order by I.SearchText, IS_MASTER, IS_MASTER2
		
--SET NOCOUNT OFF
--GO


-----------NEW SP BROKEN DOWN IN STEPS------------------------------------------------
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesGetCandidatesOnSearchText]    Script Date: 02/06/2020 13:55:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER       procedure [dbo].[st_ItemDuplicatesGetCandidatesOnSearchText]
(
      @REVIEW_ID INT
)
As

SET NOCOUNT ON
declare @Items table (item_id bigint primary key)
--FIRST of all, log that "find new duplicates" has started
insert into TB_MAG_LOG (TIME_SUBMITTED, TIME_UPDATED, JOB_TYPE, JOB_STATUS) 
	VALUES (getdate(),getdate(), 'dedup'+ cast(@REVIEW_ID as nvarchar(45)), 'running')


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
	declare @jobid int = (select top 1 MAG_LOG_ID from TB_MAG_LOG where JOB_TYPE = 'dedup'+ cast(@revID as nvarchar(45)) and JOB_STATUS = 'running' order by MAG_LOG_ID desc)
	if @jobid is null
		return 1
	--dedup is running for this review, has it been running for too long?
	declare @started datetime;
	set @started = (select TIME_SUBMITTED from TB_MAG_LOG where MAG_LOG_ID = @jobid)
	declare @diff int = DateDiff(hour, @started, getdate())
	if @diff > 6
	BEGIN
		--has been running for 6h or more, we'll mark it as failed, check number of failures and return -3 to ask for a rerun or -4 to abort
		update TB_MAG_LOG set JOB_STATUS = 'failed' where MAG_LOG_ID = @jobid
		--for safety, we'll check that  the last 10 executions were not failures, for this review.
		declare @check int =
						(
							select SUM(A.FAILURES) from
							(select top 10 CASE when JOB_STATUS = 'failed' then 1 else 0 end as FAILURES from TB_MAG_LOG where JOB_TYPE = 'dedup'+ cast(@revID as nvarchar(45))) as A
						)
		if @check = 10 return -4 --this is BAD, we tried 10 times in a row, never succeeded
		else return -3 --this makes the "get new duplicates" part start again.
	END
	return -2; --normal "this is still running" value.
	SET NOCOUNT OFF
END
GO

CREATE OR ALTER PROCEDURE [dbo].[st_ItemDuplicateGroupsMarkAsDoneChecking] 
	-- Add the parameters for the stored procedure here
	(
		@revID int,
		@state nvarchar(50),
		@message nvarchar(max) = ''
	)
AS
BEGIN
	SET NOCOUNT ON
	update TB_MAG_LOG set JOB_STATUS = @state, TIME_UPDATED = GETDATE(), JOB_MESSAGE = @message 
	where JOB_TYPE = 'dedup'+ cast(@revID as nvarchar(45)) and JOB_STATUS = 'running'
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

--CREATE our input TYPE to pass a table to new "createGroups"
IF TYPE_ID(N'CREATE_GROUPS_INPUT_TB') IS not NULL 
	BEGIN 
		DROP PROCEDURE [dbo].[st_ItemDuplicateGroupCreateFromERNativeResults] 
		DROP TYPE dbo.CREATE_GROUPS_INPUT_TB
	END
CREATE TYPE dbo.CREATE_GROUPS_INPUT_TB AS TABLE (MASTER_ID bigint, member_id bigint, IsNew bit, score float) 
GO

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
	SET nocount off
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_ITEM_DUPLICATE_GROUP_REVIEW_ID_ORIGINAL_ITEM_ID')
	CREATE NONCLUSTERED INDEX [IX_ITEM_DUPLICATE_GROUP_REVIEW_ID_ORIGINAL_ITEM_ID]
		ON  [dbo].[TB_ITEM_DUPLICATE_GROUP] ([REVIEW_ID],[ORIGINAL_ITEM_ID])
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_ITEM_DUPLICATE_GROUP_ORIGINAL_ITEM_ID')
	CREATE NONCLUSTERED INDEX [IX_ITEM_DUPLICATE_GROUP_ORIGINAL_ITEM_ID]
		ON [dbo].[TB_ITEM_DUPLICATE_GROUP] ([ORIGINAL_ITEM_ID])
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_I_DUP_GR_MEMBERS_ITEM_DUPLICATE_GROUP_ID')
	CREATE NONCLUSTERED INDEX [IX_I_DUP_GR_MEMBERS_ITEM_DUPLICATE_GROUP_ID]
		ON [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS] ([ITEM_DUPLICATE_GROUP_ID])

GO