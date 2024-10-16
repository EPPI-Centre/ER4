USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupManualAddItem]    Script Date: 31/03/2021 13:20:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: <Create Date,,>
-- Description:	Manually add Item to group, @MasterID is the destination master Item_ID
-- =============================================
ALTER PROCEDURE [dbo].[st_ItemDuplicateGroupManualAddItem]
	-- Add the parameters for the stored procedure here
	@MasterID int,
	@RevID int,
	@NewDuplicateItemID bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @GroupID int
	--get the group ID
	select @GroupID = G.ITEM_DUPLICATE_GROUP_ID from TB_ITEM_DUPLICATE_GROUP G
		inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on G.MASTER_MEMBER_ID = GM.GROUP_MEMBER_ID and G.REVIEW_ID = @RevID
		inner join TB_ITEM_REVIEW IR on IR.ITEM_REVIEW_ID = GM.ITEM_REVIEW_ID and IR.ITEM_ID = @MasterID
	if --if the item we are adding doesn't belong to the review, do nothing!
		(	
			(
				select COUNT(ITEM_ID) from TB_ITEM_REVIEW IR 
				where ir.REVIEW_ID = @RevID and ITEM_ID = @NewDuplicateItemID
			)
			!= 1
		 ) Return;
	if --if the item we are adding already belongs to the group, do nothing!
		(	
			(
				select COUNT(ITEM_ID) from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM inner join TB_ITEM_REVIEW IR 
				on GM.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.ITEM_ID = @NewDuplicateItemID and ITEM_DUPLICATE_GROUP_ID = @GroupID
			)
			> 0
		 ) Return;
	BEGIN TRY
	BEGIN TRANSACTION
	
	--mark ischecked and not duplicate all appeareances of manual item into group_members
	Update TB_ITEM_DUPLICATE_GROUP_MEMBERS
	 set IS_CHECKED = 1, IS_DUPLICATE = 0
	 from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
	 inner Join TB_ITEM_REVIEW ir on ir.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
	where ITEM_ID = @NewDuplicateItemID and REVIEW_ID = @RevID and ITEM_DUPLICATE_GROUP_ID != @GroupID
	
	
	Update TB_ITEM_REVIEW set MASTER_ITEM_ID = @MasterID, IS_INCLUDED = 1, IS_DELETED = 1
	where ITEM_ID = @NewDuplicateItemID and REVIEW_ID = @RevID AND ITEM_ID != @MasterID
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	END CATCH
END
GO

