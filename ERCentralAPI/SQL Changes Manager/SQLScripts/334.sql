USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMagInfo]    Script Date: 17/02/2021 17:10:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns statistics about matching review items to MAG
-- =============================================
ALTER PROCEDURE [dbo].[st_MagReviewMagInfo] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int = 0, 
	@NInReviewIncluded int = 0 OUTPUT,
	@NInReviewExcluded int = 0 OUTPUT,
	@NMatchedAccuratelyIncluded int = 0 OUTPUT,
	@NMatchedAccuratelyExcluded int = 0 OUTPUT,
	@NRequiringManualCheckIncluded int = 0 OUTPUT,
	@NRequiringManualCheckExcluded int = 0 OUTPUT,
	@NNotMatchedIncluded int = 0 OUTPUT,
	@NNotMatchedExcluded INT = 0 OUTPUT,
	@NPreviouslyMatched int = 0 OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select @NInReviewIncluded = count(*) from TB_ITEM_REVIEW ir
		where ir.IS_DELETED = 'FALSE' and ir.IS_INCLUDED = 'TRUE' and REVIEW_ID = @REVIEW_ID

	select @NInReviewExcluded = count(*) from TB_ITEM_REVIEW ir
		where ir.IS_DELETED = 'false' and ir.IS_INCLUDED = 'false' and REVIEW_ID = @REVIEW_ID

	select @NMatchedAccuratelyIncluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'true' and imm.REVIEW_ID = ir.REVIEW_ID
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NMatchedAccuratelyExcluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'false' and imm.REVIEW_ID = ir.REVIEW_ID
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NRequiringManualCheckIncluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'true' and imm.REVIEW_ID = ir.REVIEW_ID
		where imm.AutoMatchScore < 0.8 and ((imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null) and (imm.ManualTrueMatch = 'false' or imm.ManualTrueMatch is null))
			and not imm.ITEM_ID in 
			(
				select imm2.ITEM_ID from tb_ITEM_MAG_MATCH imm2 
				where imm.REVIEW_ID = @REVIEW_ID and imm2.ITEM_ID = imm.ITEM_ID
				AND (
						imm2.AutoMatchScore >=0.8 or 
						(
							imm2.ManualTrueMatch = 'true' and (imm2.ManualFalseMatch <> 'true' or imm2.ManualFalseMatch is null)
						)
					)
			)
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NRequiringManualCheckExcluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'false' and imm.REVIEW_ID = ir.REVIEW_ID
		where imm.AutoMatchScore < 0.8 and ((imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null) and (imm.ManualTrueMatch = 'false' or imm.ManualTrueMatch is null))
			and not imm.ITEM_ID in 
			(
				select imm2.ITEM_ID from tb_ITEM_MAG_MATCH imm2 
				where imm.REVIEW_ID = @REVIEW_ID and imm2.ITEM_ID = imm.ITEM_ID
				AND (
						imm2.AutoMatchScore >=0.8 or 
						(
							imm2.ManualTrueMatch = 'true' and (imm2.ManualFalseMatch <> 'true' or imm2.ManualFalseMatch is null)
						)
					)
			)
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NNotMatchedIncluded = count(distinct ir.ITEM_ID) from TB_ITEM_REVIEW ir
		left outer join tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = ir.ITEM_ID and imm.REVIEW_ID = ir.REVIEW_ID
			where ir.IS_INCLUDED = 'true' and imm.ITEM_ID is null and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'

	select @NNotMatchedExcluded = count(distinct ir.ITEM_ID) from TB_ITEM_REVIEW ir
		left outer join tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = ir.ITEM_ID and imm.REVIEW_ID = ir.REVIEW_ID
			where ir.IS_INCLUDED = 'false' and imm.ITEM_ID is null and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'
	
	select @NPreviouslyMatched = count(distinct ir.ITEM_ID) from TB_ITEM_REVIEW ir
		inner join TB_MAG_CHANGED_PAPER_IDS cpi on cpi.ITEM_ID = ir.ITEM_ID and
		cpi.NewPaperId = -1
		where ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'

END
GO