USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemIdFromOldItemId]    Script Date: 12/11/2026 16:23:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER     procedure [dbo].[st_ItemIdFromOldItemId]
(
	@REVIEW_ID INT,
	@OldItemId nvarchar(50),
	@ITEM_ID BIGINT OUTPUT
)

As

SET NOCOUNT ON

if @REVIEW_ID > -1
begin
	SELECT TOP (1) @ITEM_ID = I.ITEM_ID
	FROM TB_ITEM I

	-- Limit to a given review
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 0

	WHERE I.OLD_ITEM_ID = @OldItemId
	ORDER BY I.ITEM_ID DESC -- To ensure we get the most recently imported one (someone might have imported duplicates)
end
else
begin
	SELECT TOP (1) @ITEM_ID = I.ITEM_ID
	FROM TB_ITEM I

	-- Not limiting to a given review
	INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]

	WHERE I.OLD_ITEM_ID = @OldItemId
	ORDER BY I.ITEM_ID DESC
end

if @ITEM_ID is NULL
begin
	set @Item_ID = -1
end

SET NOCOUNT OFF
