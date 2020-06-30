

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupCheckOngoing]    Script Date: 04/06/2020 09:36:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[st_ItemDuplicatesGetGroupMembersForScoring] 
	-- Add the parameters for the stored procedure here
	(
		@REVIEW_ID int,
		@ITEM_IDS nvarchar(max)
	)
AS
BEGIN
	declare @t Table (item_id bigint, HAS_CODES int)
	insert into @t select i.value, 0 from dbo.fn_Split_int(@ITEM_IDS, ',') i 
		inner join TB_ITEM_REVIEW ir on i.value = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	
	update A set HAS_CODES = 1 from 
		(select t.item_id, HAS_CODES from @t t inner join TB_ITEM_SET s on t.item_id = s.ITEM_ID) A

	select i.ITEM_ID, dbo.fn_REBUILD_AUTHORS(i.ITEM_ID, 0) as AUTHORS, TITLE, PARENT_TITLE, [YEAR]
		, VOLUME, PAGES, ISSUE, DOI, ABSTRACT, HAS_CODES, 0 as IS_MASTER
	from @t t inner join TB_ITEM i on t.item_id = I.ITEM_ID 
END
GO

CREATE OR ALTER PROCEDURE [dbo].[st_ItemDuplicatesGetGroupChangeOriginalMasterId] 
	-- Add the parameters for the stored procedure here
	(
		@REVIEW_ID int,
		@GROUP_ID int,
		@NEWMASTER_ID bigint
	)
AS
BEGIN
	Update TB_ITEM_DUPLICATE_GROUP set ORIGINAL_ITEM_ID = @NEWMASTER_ID where REVIEW_ID = @REVIEW_ID and ITEM_DUPLICATE_GROUP_ID = @GROUP_ID
END
GO

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupMemberUpdate]    Script Date: 15/06/2020 12:53:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sergio
-- Create date: 20/08/2010
-- Description:	Update a group member, this will also change the group master if needed.
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ItemDuplicateGroupMemberUpdateWithScore]
	-- Add the parameters for the stored procedure here
	@memberID int
	, @groupID int
	--, @item_review_id bigint
	--, @item_id bigint
	, @is_checked bit
	, @is_duplicate bit
	, @is_master bit
	,@score float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @IR_ID bigint
	-- get the current Item_review_id
	select @IR_ID = Item_review_id from TB_ITEM_DUPLICATE_GROUP_MEMBERS where GROUP_MEMBER_ID = @memberID
	-- update the group member record
	UPDATE TB_ITEM_DUPLICATE_GROUP_MEMBERS set
		IS_CHECKED = @is_checked
		,IS_DUPLICATE = @is_duplicate
		,SCORE = @score
		WHERE GROUP_MEMBER_ID = @memberID and ITEM_DUPLICATE_GROUP_ID = @groupID
	-- see if you need to set this as master
	IF @is_master = 1
	BEGIN
		-- see who is current master
		-- if item is master of some other group, abort
		declare @current_master int = (select MASTER_MEMBER_ID from TB_ITEM_DUPLICATE_GROUP where ITEM_DUPLICATE_GROUP_ID = @groupID)
		if (
			select COUNT(G.MASTER_MEMBER_ID) from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM inner join 
				TB_ITEM_DUPLICATE_GROUP_MEMBERS GM2 on GM.GROUP_MEMBER_ID = @memberID and GM2.ITEM_REVIEW_ID = GM.ITEM_REVIEW_ID and GM2.GROUP_MEMBER_ID != GM.GROUP_MEMBER_ID
				inner join TB_ITEM_DUPLICATE_GROUP G on GM2.ITEM_DUPLICATE_GROUP_ID = G.ITEM_DUPLICATE_GROUP_ID and GM2.GROUP_MEMBER_ID = G.MASTER_MEMBER_ID
					and G.MASTER_MEMBER_ID != @memberID
			) > 0 return;
		IF (@current_master <> @memberID)
		BEGIN --change master
			UPDATE TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = @memberID where ITEM_DUPLICATE_GROUP_ID = @groupID
			select @IR_ID = Item_review_id from TB_ITEM_DUPLICATE_GROUP_MEMBERS where GROUP_MEMBER_ID = @memberID
			-- also set as checked and not duplicate in all other groups where this item appears as not a master
			update gm set IS_DUPLICATE =  0, IS_CHECKED = 0
				from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm inner join TB_ITEM_DUPLICATE_GROUP g
					on g.ITEM_DUPLICATE_GROUP_ID = gm.ITEM_DUPLICATE_GROUP_ID and g.ITEM_DUPLICATE_GROUP_ID != @groupID
					and g.MASTER_MEMBER_ID != gm.GROUP_MEMBER_ID
					and ITEM_REVIEW_ID = @IR_ID --and ITEM_DUPLICATE_GROUP_ID != @groupID
		--change the master of items that are imported into this group
		-- need to do this on tb_item_review in this sproc because after running the above 
		--the info on the previous master is lost and can't be easily reconstructed.
			declare @ID bigint = (select item_id from TB_ITEM_REVIEW IR 
									inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on IR.ITEM_REVIEW_ID = GM.ITEM_REVIEW_ID
										and GM.GROUP_MEMBER_ID = @memberID)
			update IR set MASTER_ITEM_ID = @ID
				from TB_ITEM_REVIEW IR inner join TB_ITEM_REVIEW IR2 on IR.MASTER_ITEM_ID = IR2.ITEM_ID
				Inner Join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on IR2.ITEM_REVIEW_ID = GM.ITEM_REVIEW_ID and GM.GROUP_MEMBER_ID = @current_master
				left outer join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM2 on IR.ITEM_REVIEW_ID = GM2.ITEM_REVIEW_ID and GM2.ITEM_DUPLICATE_GROUP_ID != @groupID
				--where GM2.GROUP_MEMBER_ID is null
		END 
		
	
	End
	ELSE
		Begin
		-- set to "is checked" also all other appearences of the same item, 
		-- also set to "not a duplicate" in case this is being marked as a duplicate in the active group.
		--if @is_duplicate = 1
		--begin
			
			update TB_ITEM_DUPLICATE_GROUP_MEMBERS set IS_DUPLICATE =  0, IS_CHECKED = @is_checked 
				where ITEM_REVIEW_ID = @IR_ID and ITEM_DUPLICATE_GROUP_ID != @groupID
		END
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesGetCandidatesOnSearchText]    Script Date: 15/06/2020 15:00:12 ******/
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
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS IDG ON IDG.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP DG ON DG.ITEM_DUPLICATE_GROUP_ID = IDG.ITEM_DUPLICATE_GROUP_ID
		where ir.REVIEW_ID = @REVIEW_ID
			AND
			(
				ir.IS_DELETED = 0 -- ANY not deleted item
				OR (
					 DG.ITEM_DUPLICATE_GROUP_ID IS NOT NULL 
				) --IF item is deleted, ANY Member of a group
			)
			  


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
