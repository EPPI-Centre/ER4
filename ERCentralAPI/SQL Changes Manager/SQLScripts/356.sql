USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagGetCurrentlyUsedPaperIds]    Script Date: 01/03/2021 17:23:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get the currently used PaperIds for checking deletions between MAG versions
-- =============================================
ALTER PROCEDURE [dbo].[st_MagGetCurrentlyUsedPaperIds] 
	@REVIEW_ID INT = 0 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	if @REVIEW_ID > 0 
		SELECT distinct PaperId, imm.ITEM_ID from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			where ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'False'
	ELSE -- i.e. every currently used PaperId - includes additional tables. Used when updating changed PaperIda
		SELECT DISTINCT PaperId, ITEM_ID from tb_ITEM_MAG_MATCH	
		UNION
			select distinct PaperId, 0 from TB_MAG_AUTO_UPDATE_RUN_PAPER
		UNION
			select distinct PaperId, 0 from TB_MAG_RELATED_PAPERS



END
GO