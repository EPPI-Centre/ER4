USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagPaperListByIdIds]    Script Date: 01/03/2021 17:41:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List of MAG papers from a list of paper IDs
-- =============================================
ALTER PROCEDURE [dbo].[st_MagPaperListByIdIds] 
	-- Add the parameters for the stored procedure here
	@PaperIds nvarchar(max)
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@REVIEW_ID int = 0
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT @Total = count(value) from dbo.fn_Split_int(@PaperIds, ',')

    -- Insert statements for procedure here
	SELECT distinct ids.value PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from dbo.fn_Split_int(@PaperIds, ',') ids		
		left outer join tb_ITEM_MAG_MATCH imm on imm.PaperId = ids.value
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID and AutoMatchScore > 0.8
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'
		order by PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END

GO
