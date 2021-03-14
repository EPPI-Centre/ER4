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