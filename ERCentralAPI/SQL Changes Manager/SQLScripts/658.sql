USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateClassifierScoresUpdate]    Script Date: 21/03/2025 09:35:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdateClassifierScoresUpdate] 
	-- Add the parameters for the stored procedure here
	@MAG_AUTO_UPDATE_RUN_ID int,
	@Field nvarchar(30) = '',
	@StudyTypeClassifier nvarchar(50) = '',
	@UserClassifierModelId int = 0,
	@UserClassifierReviewId int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	if @Field = 'StudyTypeClassifierScore'
	BEGIN
		UPDATE MAUR
		SET StudyTypeClassifierScore = Score
		FROM TB_MAG_AUTO_UPDATE_RUN_PAPER MAUR
		INNER JOIN TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP mac ON mac.MAG_AUTO_UPDATE_RUN_ID = maur.MAG_AUTO_UPDATE_RUN_ID 
		and MAUR.MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID AND mac.PaperId = MAUR.PaperId

		UPDATE TB_MAG_AUTO_UPDATE_RUN
			SET STUDY_TYPE_CLASSIFIER = @StudyTypeClassifier
			WHERE MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID
	END

	IF @Field = 'UserClassifierScore'
	BEGIN
		UPDATE MAUR
		SET UserClassifierScore = SCORE
		FROM TB_MAG_AUTO_UPDATE_RUN_PAPER MAUR
		INNER JOIN TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP mac ON mac.MAG_AUTO_UPDATE_RUN_ID = MAUR.MAG_AUTO_UPDATE_RUN_ID 
		and MAUR.MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID AND mac.PaperId = MAUR.PaperId

		UPDATE TB_MAG_AUTO_UPDATE_RUN
			SET USER_CLASSIFIER_MODEL_ID = @UserClassifierModelId,
				USER_CLASSIFIER_REVIEW_ID = @UserClassifierReviewId
			WHERE MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID
	END

	DELETE FROM TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP
		WHERE MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID

END
GO