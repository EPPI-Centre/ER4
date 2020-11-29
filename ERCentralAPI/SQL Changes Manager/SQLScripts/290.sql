USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRunListIds]    Script Date: 29/11/2020 09:49:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
create or ALTER       PROCEDURE [dbo].[st_MagAutoUpdateRunListIds] 
	-- Add the parameters for the stored procedure here
	
	@MagAutoUpdateRunId int = 0
,	@OrderBy nvarchar(20) = 'AutoUpdate'
,	@AutoUpdateScore float = 0
,	@StudyTypeClassifierScore float = 0
,	@UserClassifierScore float = 0

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

	select @Total = count(*) from TB_MAG_AUTO_UPDATE_RUN_PAPER maur
		where maur.MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId
		and ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore

if @OrderBy = 'AutoUpdate'
begin
	SELECT maur.PaperId, maur.ContReviewScore SimilarityScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from TB_MAG_AUTO_UPDATE_RUN_PAPER maur
		left outer join tb_ITEM_MAG_MATCH imm on (imm.PaperId = maur.PaperId and imm.REVIEW_ID = @REVIEW_ID)
			and imm.ManualFalseMatch <> 'True' 
		where maur.MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId
		and ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by maur.ContReviewScore desc
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

end
else if @OrderBy = 'StudyTypeClassifier'
begin

	SELECT maur.PaperId, maur.StudyTypeClassifierScore SimilarityScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from TB_MAG_AUTO_UPDATE_RUN_PAPER maur
		left outer join tb_ITEM_MAG_MATCH imm on (imm.PaperId = maur.PaperId and imm.REVIEW_ID = @REVIEW_ID)
			and imm.ManualFalseMatch <> 'True' 
		where maur.MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId
		and ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by maur.StudyTypeClassifierScore desc
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
end
else if @OrderBy = 'UserClassifier'
begin

	SELECT maur.PaperId, maur.UserClassifierScore SimilarityScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from TB_MAG_AUTO_UPDATE_RUN_PAPER maur
		left outer join tb_ITEM_MAG_MATCH imm on (imm.PaperId = maur.PaperId and imm.REVIEW_ID = @REVIEW_ID)
			and imm.ManualFalseMatch <> 'True' 
		where maur.MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId
		and ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by maur.UserClassifierScore desc
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
end

	SELECT  @Total as N'@Total'
END
GO