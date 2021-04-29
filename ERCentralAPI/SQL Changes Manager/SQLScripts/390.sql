USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupList]    Script Date: 4/7/2021 10:16:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: 13/08/10
-- Description:	get the list of groups for a review with their current master ID
-- =============================================
ALTER PROCEDURE [dbo].[st_ItemDuplicateGroupList]
	-- Add the parameters for the stored procedure here
	@RevID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    --delete groups that don't make sense anymore: i.e. groups with 1 item only (this may happen if deleting forever duplicate candidates)
    DELETE TB_ITEM_DUPLICATE_GROUP WHERE ITEM_DUPLICATE_GROUP_ID IN 
	(
		SELECT G.ITEM_DUPLICATE_GROUP_ID from TB_ITEM_DUPLICATE_GROUP G
		Inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on GM.ITEM_DUPLICATE_GROUP_ID = G.ITEM_DUPLICATE_GROUP_ID and G.REVIEW_ID = @RevID
		group by G.ITEM_DUPLICATE_GROUP_ID
		having COUNT(gm.ITEM_DUPLICATE_GROUP_ID) = 1
	)

	SELECT g.ITEM_DUPLICATE_GROUP_ID GROUP_ID, IR.ITEM_ID MASTER_ITEM_ID, SHORT_TITLE, g.REVIEW_ID
		,CASE WHEN (COUNT(distinct(gm2.IS_CHECKED)) = 1) then 1 --master item is always marked "is checked" so if only one value is used for the group it has to be IS_CHECKED=true
			ELSE 0
		END
		as IS_COMPLETE		
		from TB_ITEM_DUPLICATE_GROUP g
		INNER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on g.MASTER_MEMBER_ID = gm.GROUP_MEMBER_ID
		INNER JOIN TB_ITEM_REVIEW IR on IR.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
		INNER JOIN TB_ITEM I on IR.ITEM_ID = I.ITEM_ID
		INNER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS gm2 on g.ITEM_DUPLICATE_GROUP_ID = gm2.ITEM_DUPLICATE_GROUP_ID
	WHERE g.REVIEW_ID = @RevID
	GROUP BY g.ITEM_DUPLICATE_GROUP_ID, IR.ITEM_ID, SHORT_TITLE, g.REVIEW_ID
END
GO
