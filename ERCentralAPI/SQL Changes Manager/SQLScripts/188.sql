USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunGetSeedIds]    Script Date: 27/04/2020 17:31:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Stage 1 in getting related papers: get the list of seed MAG IDs
-- =============================================
CREATE OR ALTER     PROCEDURE [dbo].[st_MagRelatedPapersRunGetSeedIds] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID int
,	@REVIEW_ID INT
,	@ATTRIBUTE_ID BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if @ATTRIBUTE_ID > 0
	BEGIN
		select imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
		inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = ia.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
		WHERE (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		group by imm.PaperId
		order by imm.PaperId
	END
	else
	BEGIN
		select imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
		WHERE (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		group by imm.PaperId
		order by imm.PaperId
	END
END
