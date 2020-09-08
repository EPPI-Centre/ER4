USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGetTitles]    Script Date: 9/8/2020 2:48:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   procedure [dbo].[st_ItemDuplicateGetTitles]
(
      @REVIEW_ID INT
)
As

SET NOCOUNT ON
	SELECT TITLE, i.ITEM_ID FROM TB_ITEM I
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = I.ITEM_ID
		WHERE IR.REVIEW_ID = @REVIEW_ID AND IR.IS_DELETED = 'FALSE'
		AND (SearchText is NULL or SearchText = '')
SET NOCOUNT OFF

go