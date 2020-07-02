USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupCreateFromERNativeResults]    Script Date: 02/07/2020 14:51:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


--CLEANUP of unneded things
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemDuplicateGroupCreateNew]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ItemDuplicateGroupCreateNew]
--GO
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemDuplicateGroupAddAddionalItem]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ItemDuplicateGroupAddAddionalItem]
--GO

--WE could remove TB_ITEM_DUPLICATES as well...


ALTER   PROCEDURE [dbo].[st_ItemDuplicateGroupCreateFromERNativeResults] 
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

	--2.2 find and record what goes where - this time, get Destination for the records where the master item is now a different one...
	update @work set destination = a.ITEM_DUPLICATE_GROUP_ID 
		from (
				select g.ITEM_DUPLICATE_GROUP_ID, w.MASTER_ID as mid from 
				TB_ITEM_DUPLICATE_GROUP g 
				inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on g.ITEM_DUPLICATE_GROUP_ID = gm.ITEM_DUPLICATE_GROUP_ID 
					and g.MASTER_MEMBER_ID = gm.GROUP_MEMBER_ID and g.REVIEW_ID = @revID
				inner join TB_ITEM_REVIEW ir on gm.ITEM_REVIEW_ID = ir.ITEM_REVIEW_ID and ir.REVIEW_ID = @revID
	  			inner join @work w on g.REVIEW_ID = @revID and w.MASTER_ID = ir.ITEM_ID
				) a
		where MASTER_ID = a.mid and destination is null
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