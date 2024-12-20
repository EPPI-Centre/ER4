USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupMemberUpdateWithScore]    Script Date: 28/04/2021 11:30:01 ******/
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
		--change the master of "manually added items" for this group
		--need to do this UPDATE to tb_item_review in this sproc because after running the above 
		--the info on the previous master is lost and can't be easily reconstructed.
			declare @ID bigint = (select item_id from TB_ITEM_REVIEW IR 
									inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on IR.ITEM_REVIEW_ID = GM.ITEM_REVIEW_ID
										and GM.GROUP_MEMBER_ID = @memberID)
			update IR set MASTER_ITEM_ID = @ID
				from TB_ITEM_REVIEW IR 
				inner join TB_ITEM_REVIEW IR2 on IR.MASTER_ITEM_ID = IR2.ITEM_ID and IR.REVIEW_ID = @RevId and IR.REVIEW_ID = IR2.REVIEW_ID
				Inner Join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on IR2.ITEM_REVIEW_ID = GM.ITEM_REVIEW_ID and GM.GROUP_MEMBER_ID = @current_master
				where IR.ITEM_REVIEW_ID not in (select gm2.ITEM_REVIEW_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm2 where gm2.ITEM_DUPLICATE_GROUP_ID = @groupID)
				--left outer join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM2 on IR.ITEM_REVIEW_ID = GM2.ITEM_REVIEW_ID and GM2.ITEM_DUPLICATE_GROUP_ID != @groupID
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

CREATE or ALTER PROCEDURE [dbo].[st_LogReviewJob]
	-- Add the parameters for the stored procedure here
	@ReviewId int,
	@ContactId int,
	@JobType nvarchar(50),
	@CurrentState nvarchar(50),
	@Success bit,
	@JobMessage nvarchar(4000)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	INSERT INTO [dbo].[tb_REVIEW_JOB]
           ([REVIEW_ID]
           ,[CONTACT_ID]
           ,[START_TIME]
           ,[END_TIME]
           ,[JOB_TYPE]
           ,[CURRENT_STATE]
           ,[SUCCESS]
           ,[JOB_MESSAGE])
     VALUES
           (@ReviewId
           ,@ContactId
           ,GETDATE()
           ,GETDATE()
           ,@JobType
           ,@CurrentState
           ,@Success
           ,@JobMessage)

END
GO


