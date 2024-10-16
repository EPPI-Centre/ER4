USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRunResults]    Script Date: 16/02/2021 17:55:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdateRunResults] 
	-- Add the parameters for the stored procedure here
	
	@MagAutoUpdateRunId int
,	@OrderBy nvarchar(19)
,	@AutoUpdateScore float
,	@StudyTypeClassifierScore float
,	@UserClassifierScore float
,	@TopN int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

if @OrderBy = 'AutoUpdate'
begin
	select top(@TopN) PaperId from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId and
			ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by ContReviewScore desc
end
else if @OrderBy = 'StudyTypeClassifier'
begin
	select top(@TopN) PaperId from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId and
			ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by StudyTypeClassifierScore desc
end
else if @OrderBy = 'UserClassifier'
begin
	select top(@TopN) PaperId from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId and
			ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by UserClassifierScore desc
end
END
GO
