USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersListIds]    Script Date: 01/03/2021 17:38:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
ALTER PROCEDURE [dbo].[st_MagRelatedPapersListIds] 
	-- Add the parameters for the stored procedure here
	
	@MAG_RELATED_RUN_ID int = 0
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@REVIEW_ID int = 0
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	select @Total = count(*) from tb_MAG_RELATED_PAPERS mrp
		where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
	
	SELECT mrp.PaperId, mrp.SimilarityScore, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, ir.ITEM_ID
		from tb_MAG_RELATED_PAPERS mrp
		left outer join tb_ITEM_MAG_MATCH imm on imm.PaperId = mrp.PaperId
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID and AutoMatchScore > 0.8
		left outer join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'
		where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
		order by mrp.SimilarityScore desc
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END
GO