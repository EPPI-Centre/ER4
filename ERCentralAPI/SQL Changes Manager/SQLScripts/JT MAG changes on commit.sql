USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersListIds]    Script Date: 17/05/2020 07:21:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagRelatedPapersListIds] 
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
	
	SELECT mrp.PaperId, mrp.SimilarityScore, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_MAG_RELATED_PAPERS mrp
		left outer join tb_ITEM_MAG_MATCH imm on (imm.PaperId = mrp.PaperId and imm.REVIEW_ID = @REVIEW_ID)
			and imm.ManualFalseMatch <> 'True' 
		where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
		order by mrp.SimilarityScore desc
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END
GO