USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemArmDelete]    Script Date: 28/07/2023 10:58:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemArmDelete]
(
	@ITEM_ARM_ID BIGINT
)

As

SET NOCOUNT ON
	DELETE FROM TB_ITEM_ATTRIBUTE where ITEM_ARM_ID = @ITEM_ARM_ID
	DELETE FROM TB_ITEM_ARM
		WHERE ITEM_ARM_ID = @ITEM_ARM_ID

	UPDATE TB_ITEM_OUTCOME
		SET ITEM_ARM_ID_GRP1 = 0
		WHERE ITEM_ARM_ID_GRP1 = @ITEM_ARM_ID

	UPDATE TB_ITEM_OUTCOME
		SET ITEM_ARM_ID_GRP2 = 0
		WHERE ITEM_ARM_ID_GRP2 = @ITEM_ARM_ID

SET NOCOUNT OFF
GO

update TB_ITEM set SearchText = trim(SearchText) where SearchText like '% '
GO
