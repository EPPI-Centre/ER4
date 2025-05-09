USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateUpdateTbItemReview]    Script Date: 05/03/2021 11:10:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sergio
-- Create date: 20/08/2010
-- Description:	Update a group member, this will also change the group master if needed.
-- =============================================
ALTER PROCEDURE [dbo].[st_ItemDuplicateUpdateTbItemReview]
	-- Add the parameters for the stored procedure here
	@groupID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	
	declare @t table (IR_ID bigint primary key, s_id int null, Is_Source_deleted bit null, incl bit null, deleted bit null, new_master bigint null)
	insert into @t
		select ir.ITEM_REVIEW_ID 
		, (select s.source_id from TB_SOURCE s inner join TB_ITEM_SOURCE tis on s.SOURCE_ID = tis.SOURCE_ID and tis.ITEM_ID = ir.ITEM_ID and s.REVIEW_ID= ir.REVIEW_ID)
		,null,null,null,null
		from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
		inner join TB_ITEM_DUPLICATE_GROUP DG on gm.ITEM_DUPLICATE_GROUP_ID = DG.ITEM_DUPLICATE_GROUP_ID and DG.ITEM_DUPLICATE_GROUP_ID = @groupID
		inner join TB_ITEM_REVIEW ir on ir.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
		Left outer join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM2 on IR.ITEM_REVIEW_ID = GM2.ITEM_REVIEW_ID 
				and GM2.ITEM_DUPLICATE_GROUP_ID != DG.ITEM_DUPLICATE_GROUP_ID
				and GM2.IS_CHECKED = 1 and GM2.IS_DUPLICATE = 1
			WHERE GM2.GROUP_MEMBER_ID is null
			--the LEFT OUTER join with "WHERE GM2.GROUP_MEMBER_ID is null" ensures we only ever touch items that are only in ONE group.
	--select * from @t

	update @t set Is_Source_deleted = s.IS_DELETED from TB_SOURCE s where s.SOURCE_ID = s_id

	--select * from @t

	declare @chk int = (select count(IR_ID) from @t where s_id is null)
	IF @chk > 0
	BEGIN
		--we have at least one item in this group that isn't in any source so we need to check if the "sourceless" source is all deleted...
		declare @revID int = (select review_id from TB_ITEM_DUPLICATE_GROUP where ITEM_DUPLICATE_GROUP_ID = @groupID)
		declare @is_del bit = (
							Select Case 
							when COUNT(ir.ITEM_ID) = Sum(
														case when ir.IS_DELETED = 1 then 1 else 0 end
														) 
							then 1 else 0 end as IS_DELETED
							from tb_item_review ir 
							where ir.REVIEW_ID = @revID 
								and ir.ITEM_ID not in 
									(
										Select ITEM_ID from TB_SOURCE s
										inner join TB_ITEM_SOURCE tis on s.SOURCE_ID = tis.SOURCE_ID and s.REVIEW_ID = @revID
									)
							)
		update @t set Is_Source_deleted = @is_del where s_id is null
		--select * from @t
	END 
	declare @true bit = 1, @false bit = 0

	update @t set incl = 
						CASE WHEN --source is deleted OR is marked as a duplicate, put it to @true always
							t.Is_Source_deleted = @true OR (gm.IS_CHECKED = @true and gm.IS_DUPLICATE = @true) then @true --
						 WHEN -- this is the master, goes to INCLUDED state IF it was previously marked as a duplicate 
							GM1.ITEM_REVIEW_ID = t.IR_ID AND ir.IS_INCLUDED = @true and ir.IS_DELETED = @true and ir.MASTER_ITEM_ID is not null then @true
						 WHEN --shouldn't happen, really. Not checked, not a duplicate, but marked as a duplicate in TB_ITEM_REVIEW...
							gm.IS_CHECKED = @false and gm.IS_DUPLICATE = @false AND ir.IS_INCLUDED = @true and ir.IS_DELETED = @true and ir.MASTER_ITEM_ID is not null then @true
						 WHEN --checked and is NOT a duplicate BUT was marked as a duplicate (user changed her mind)
							gm.IS_CHECKED = @true and gm.IS_DUPLICATE = @false AND ir.IS_INCLUDED = @true and ir.IS_DELETED = @true and ir.MASTER_ITEM_ID is not null then @true
						 ELSE -- no change!
							ir.IS_INCLUDED
						end
				, deleted =
						CASE WHEN --source is deleted OR is marked as a duplicate, put it to @true always
							t.Is_Source_deleted = @true OR (gm.IS_CHECKED = @true and gm.IS_DUPLICATE = @true) then @true --
						 WHEN -- this is the master, goes to INCLUDED state IF it was previously marked as a duplicate 
							GM1.ITEM_REVIEW_ID = t.IR_ID AND ir.IS_INCLUDED = @true and ir.IS_DELETED = @true and ir.MASTER_ITEM_ID is not null then @false
						 WHEN --shouldn't happen, really. Not checked, not a duplicate, but was marked as a duplicate in TB_ITEM_REVIEW...
							gm.IS_CHECKED = @false and gm.IS_DUPLICATE = @false AND ir.IS_INCLUDED = @true and ir.IS_DELETED = @true and ir.MASTER_ITEM_ID is not null then @false
						 WHEN --checked and is NOT a duplicate BUT was marked as a duplicate (user changed her mind)
							gm.IS_CHECKED = @true and gm.IS_DUPLICATE = @false AND ir.IS_INCLUDED = @true and ir.IS_DELETED = @true and ir.MASTER_ITEM_ID is not null then @false
						 ELSE -- no change!
							ir.IS_DELETED
						end
				, new_master = 
						CASE WHEN GM.IS_CHECKED = 1 and GM.IS_DUPLICATE = 1 then
							IR1.ITEM_ID
						ELSE Null
						END
		from @t t inner join 
			TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on t.IR_ID = gm.ITEM_REVIEW_ID and gm.ITEM_DUPLICATE_GROUP_ID = @groupID
			inner join TB_ITEM_DUPLICATE_GROUP DG on gm.ITEM_DUPLICATE_GROUP_ID = DG.ITEM_DUPLICATE_GROUP_ID
			inner join TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
			Inner Join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM1 on GM1.GROUP_MEMBER_ID = DG.MASTER_MEMBER_ID 
				and GM1.ITEM_DUPLICATE_GROUP_ID = @groupID --finding the current MASTER item
			INNER Join TB_ITEM_REVIEW IR1 on GM1.ITEM_REVIEW_ID = IR1.ITEM_REVIEW_ID --need the ITEM_ID of the master item
	
	--select t.*, ir.IS_INCLUDED, ir.IS_DELETED, MASTER_ITEM_ID from @t t
	--	inner join TB_ITEM_REVIEW ir on t.IR_ID = ir.ITEM_REVIEW_ID

	update TB_ITEM_REVIEW set IS_DELETED = t.deleted
						, IS_INCLUDED = t.incl
						,MASTER_ITEM_ID = t.new_master
		from TB_ITEM_REVIEW ir
		inner join @t t on t.IR_ID = ir.ITEM_REVIEW_ID

	--select ir.ITEM_ID, t.*, case when ir.ITEM_REVIEW_ID = ir1.ITEM_REVIEW_ID then 1 else 0 end as [is master]
	--	, ir.IS_INCLUDED, ir.IS_DELETED, ir.MASTER_ITEM_ID from @t t
	--	inner join TB_ITEM_REVIEW ir on t.IR_ID = ir.ITEM_REVIEW_ID
	--	inner join TB_ITEM_DUPLICATE_GROUP dg on dg.ITEM_DUPLICATE_GROUP_ID = @groupID
	--	Inner Join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM1 on dg.MASTER_MEMBER_ID = gm1.GROUP_MEMBER_ID
	--				and GM1.ITEM_DUPLICATE_GROUP_ID = @groupID --finding the current MASTER item
	--	INNER Join TB_ITEM_REVIEW IR1 on GM1.ITEM_REVIEW_ID = IR1.ITEM_REVIEW_ID
	
	
	--OLD version starts here:
	--update IR 
	--	set IR.MASTER_ITEM_ID = CASE
			
	--		WHEN GM.IS_CHECKED = 1 and GM.IS_DUPLICATE = 1 then
	--			IR1.ITEM_ID
	--		ELSE Null
	--		END
	--	, 
	--	IR.IS_DELETED = CASE
	--		WHEN GM1.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID --item is master of current group, leave as the source it belongs.
	--		--need left joins for manually created items (don't have a source!)
	--			then (select CASE when (s.is_deleted = 'True' )  Then 'True' else 'False' end from 
	--						TB_ITEM_REVIEW iir
	--						LEFT join TB_ITEM_SOURCE tis on iir.ITEM_ID = tis.ITEM_ID
	--						LEFT join TB_SOURCE s on tis.SOURCE_ID = s.SOURCE_ID
	--						where IR.ITEM_REVIEW_ID = iir.ITEM_REVIEW_ID
	--						--TB_SOURCE s inner join TB_ITEM_SOURCE tis 
	--						--on s.SOURCE_ID = tis.SOURCE_ID and tis.ITEM_ID = IR.ITEM_ID
	--						) --
	--		WHEN GM.IS_CHECKED = 1 and GM.IS_DUPLICATE = 1 then
	--			'True'
	--		ELSE 'False'
	--		END
	--	, IR.IS_INCLUDED = CASE
	--		WHEN GM.IS_DUPLICATE = 1 and GM.IS_CHECKED = 1 then --set is_included to true, to make the item 'shadow'
	--			'true'
	--		ELSE IR.IS_INCLUDED --leave untouched
	--		END
	--from TB_ITEM_REVIEW IR inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM 
	--	on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and GM.ITEM_DUPLICATE_GROUP_ID = @groupID
	--	Inner Join TB_ITEM_DUPLICATE_GROUP DG on DG.ITEM_DUPLICATE_GROUP_ID = gm.ITEM_DUPLICATE_GROUP_ID
	--	Inner Join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM1 on GM1.GROUP_MEMBER_ID = DG.MASTER_MEMBER_ID
	--	INNER Join TB_ITEM_REVIEW IR1 on GM1.ITEM_REVIEW_ID = IR1.ITEM_REVIEW_ID
	--	Left outer join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM2 on IR.ITEM_REVIEW_ID = GM2.ITEM_REVIEW_ID 
	--		and GM2.ITEM_DUPLICATE_GROUP_ID != DG.ITEM_DUPLICATE_GROUP_ID
	--		and GM2.IS_CHECKED = 1 and GM2.IS_DUPLICATE = 1
	--	where GM2.GROUP_MEMBER_ID is null
END

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupMemberUpdateWithScore]    Script Date: 3/9/2021 4:11:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sergio
-- Create date: 20/08/2010
-- Description:	Update a group member, this will also change the group master if needed.
-- =============================================
ALTER   PROCEDURE [dbo].[st_ItemDuplicateGroupMemberUpdateWithScore]
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
		declare @RevId int  = (select review_id from TB_ITEM_DUPLICATE_GROUP where ITEM_DUPLICATE_GROUP_ID = @groupID)
		declare @current_master int = (select MASTER_MEMBER_ID from TB_ITEM_DUPLICATE_GROUP where ITEM_DUPLICATE_GROUP_ID = @groupID)
		if (
			select COUNT(G.MASTER_MEMBER_ID) from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM inner join 
				TB_ITEM_DUPLICATE_GROUP_MEMBERS GM2 on GM.GROUP_MEMBER_ID = @memberID and GM2.ITEM_REVIEW_ID = GM.ITEM_REVIEW_ID and GM2.GROUP_MEMBER_ID != GM.GROUP_MEMBER_ID
				inner join TB_ITEM_DUPLICATE_GROUP G on GM2.ITEM_DUPLICATE_GROUP_ID = G.ITEM_DUPLICATE_GROUP_ID and GM2.GROUP_MEMBER_ID = G.MASTER_MEMBER_ID
					and G.MASTER_MEMBER_ID != @memberID and G.REVIEW_ID = @RevId
			) > 0 return;
		IF (@current_master <> @memberID)
		BEGIN --change master
			UPDATE TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = @memberID where ITEM_DUPLICATE_GROUP_ID = @groupID
			select @IR_ID = Item_review_id from TB_ITEM_DUPLICATE_GROUP_MEMBERS where GROUP_MEMBER_ID = @memberID
			-- also set as checked and not duplicate in all other groups where this item appears as not a master
			update gm set IS_DUPLICATE =  0, IS_CHECKED = 0
				from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm inner join TB_ITEM_DUPLICATE_GROUP g
					on g.REVIEW_ID = @RevId
					and g.ITEM_DUPLICATE_GROUP_ID = gm.ITEM_DUPLICATE_GROUP_ID and g.ITEM_DUPLICATE_GROUP_ID != @groupID
					and g.MASTER_MEMBER_ID != gm.GROUP_MEMBER_ID
					and ITEM_REVIEW_ID = @IR_ID --and ITEM_DUPLICATE_GROUP_ID != @groupID
		--change the master of items that are imported into this group
		-- need to do this on tb_item_review in this sproc because after running the above 
		--the info on the previous master is lost and can't be easily reconstructed.
			declare @ID bigint = (select item_id from TB_ITEM_REVIEW IR 
									inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on IR.ITEM_REVIEW_ID = GM.ITEM_REVIEW_ID
										and GM.GROUP_MEMBER_ID = @memberID)
			update IR set MASTER_ITEM_ID = @ID
				from TB_ITEM_REVIEW IR 
				inner join TB_ITEM_REVIEW IR2 on IR.MASTER_ITEM_ID = IR2.ITEM_ID and IR.REVIEW_ID = @RevId and IR.REVIEW_ID = IR2.REVIEW_ID
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


