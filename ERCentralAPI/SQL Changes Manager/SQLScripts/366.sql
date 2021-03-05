USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagContReviewRunGetSeedIds]    Script Date: 05/03/2021 10:29:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get seed MAG IDs for the Continuous Review run when a new MAG is available
-- =============================================
ALTER PROCEDURE [dbo].[st_MagContReviewRunGetSeedIds] 
	@MAG_VERSION nvarchar(15)
AS
BEGIN

	-- First, create new lines for each update in each review
	INSERT INTO TB_MAG_AUTO_UPDATE_RUN(MAG_AUTO_UPDATE_ID, USER_DESCRIPTION, ATTRIBUTE_ID,
		ALL_INCLUDED, DATE_RUN, N_PAPERS, REVIEW_ID, MAG_VERSION)
	SELECT MAG_AUTO_UPDATE_ID, USER_DESCRIPTION, ATTRIBUTE_ID, ALL_INCLUDED, GETDATE(), -1, REVIEW_ID, @MAG_VERSION
		FROM TB_MAG_AUTO_UPDATE

	-- Next, grab the seed ids (not filtered by an attribute)
	SELECT imm.PaperId, mau.MAG_AUTO_UPDATE_RUN_ID AutoUpdateId, 1 as Included
	from tb_ITEM_MAG_MATCH imm
	inner join TB_ITEM_REVIEW ir on ir.REVIEW_ID = imm.REVIEW_ID and imm.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 'false'
	inner join TB_MAG_AUTO_UPDATE_RUN mau on mau.REVIEW_ID = ir.REVIEW_ID
	where mau.N_PAPERS = -1 and(mau.ATTRIBUTE_ID = 0 OR mau.ATTRIBUTE_ID is null) and
		(imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)

	UNION ALL

	-- seed ids filtered by a given attribute
	SELECT imm.PaperId, mau.MAG_AUTO_UPDATE_RUN_ID AutoUpdateId, 1 as Included
	from tb_ITEM_MAG_MATCH imm
	inner join TB_ITEM_REVIEW ir on ir.REVIEW_ID = imm.REVIEW_ID and imm.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 'false'
	inner join TB_MAG_AUTO_UPDATE_RUN mau on mau.REVIEW_ID = ir.REVIEW_ID
	inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = ir.ITEM_ID and ia.ATTRIBUTE_ID = mau.ATTRIBUTE_ID
	inner join TB_ITEM_SET iset on iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
	where mau.N_PAPERS = -1 and not mau.ATTRIBUTE_ID is null and
	(imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)		
END
GO