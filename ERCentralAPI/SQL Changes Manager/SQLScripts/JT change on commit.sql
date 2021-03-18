USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateVisualise]    Script Date: 14/03/2021 12:16:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   procedure [dbo].[st_MagAutoUpdateVisualise]
(
	@MAG_AUTO_UPDATE_RUN_ID INT,
	@FIELD NVARCHAR(10)
)

As

SET NOCOUNT ON

	DECLARE @VALS TABLE
(
	VAL float
,	TITLE NVARCHAR(10)
)

IF @FIELD = 'AutoUpdate'
begin
INSERT INTO @VALS (VAL, TITLE)
VALUES (0.4, '0.4-'), (0.5, '0.5-'), (0.6, '0.6-'), (0.7, '0.7-'), (0.8, '0.8-'), (0.9, '-0.99')

SELECT VAL, TITLE,
	(SELECT COUNT(*) FROM TB_MAG_AUTO_UPDATE_RUN_PAPER WHERE 
	ContReviewScore >= VAL AND ContReviewScore < VAL + 0.1 AND MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID) AS NUM
FROM @VALS
end

IF @FIELD = 'StudyType'
begin
INSERT INTO @VALS (VAL, TITLE)
VALUES (0, '0.0-'), (0.1, '0.1-'), (0.2, '0.2-'), (0.3, '0.3-'), (0.4, '0.4-'), (0.5, '0.5-'), (0.6, '0.6-'), (0.7, '0.7-'), (0.8, '0.8-'), (0.9, '-0.99')

SELECT VAL, TITLE,
	(SELECT COUNT(*) FROM TB_MAG_AUTO_UPDATE_RUN_PAPER WHERE 
	StudyTypeClassifierScore >= VAL AND StudyTypeClassifierScore < VAL + 0.1 AND MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID) AS NUM
FROM @VALS
end

IF @FIELD = 'User'
begin
INSERT INTO @VALS (VAL, TITLE)
VALUES (0, '0.0-'), (0.1, '0.1-'), (0.2, '0.2-'), (0.3, '0.3-'), (0.4, '0.4-'), (0.5, '0.5-'), (0.6, '0.6-'), (0.7, '0.7-'), (0.8, '0.8-'), (0.9, '-0.99')

SELECT VAL, TITLE,
	(SELECT COUNT(*) FROM TB_MAG_AUTO_UPDATE_RUN_PAPER WHERE 
	UserClassifierScore >= VAL AND UserClassifierScore < VAL + 0.1 AND MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID) AS NUM
FROM @VALS
end



SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagGetCurrentlyUsedPaperIds]    Script Date: 18/03/2021 09:39:37 ******/
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
			where ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'False' and
			  (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
	ELSE -- i.e. every currently used PaperId - includes additional tables. Used when updating changed PaperIda
		SELECT DISTINCT PaperId, ITEM_ID from tb_ITEM_MAG_MATCH	
		UNION
			select distinct PaperId, 0 from TB_MAG_AUTO_UPDATE_RUN_PAPER
		UNION
			select distinct PaperId, 0 from TB_MAG_RELATED_PAPERS



END
GO