USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRunListIds]    Script Date: 20/11/2020 09:01:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagAutoUpdateRunListIds] 
	-- Add the parameters for the stored procedure here
	
	@MagAutoUpdateRunId int = 0
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
	
	SELECT maur.PaperId, maur.ContReviewScore SimilarityScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from TB_MAG_AUTO_UPDATE_RUN_PAPER maur
		left outer join tb_ITEM_MAG_MATCH imm on (imm.PaperId = maur.PaperId and imm.REVIEW_ID = @REVIEW_ID)
			and imm.ManualFalseMatch <> 'True' 
		where maur.MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId
		order by maur.ContReviewScore desc
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRunResults]    Script Date: 20/11/2020 08:55:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
CREATE OR ALTER       PROCEDURE [dbo].[st_MagAutoUpdateRunResults] 
	-- Add the parameters for the stored procedure here
	
	@MagAutoUpdateRunId int
,	@OrderBy nvarchar(10)
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
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRuns]    Script Date: 20/11/2020 07:34:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all runs for a given review
-- =============================================
ALTER         PROCEDURE [dbo].[st_MagAutoUpdateRuns] 
	-- Add the parameters for the stored procedure here	
	@REVIEW_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select mau.MAG_AUTO_UPDATE_RUN_ID, mau.USER_DESCRIPTION, mau.ATTRIBUTE_ID, mau.ALL_INCLUDED,
		a.ATTRIBUTE_NAME, mau.DATE_RUN, mau.N_PAPERS, mau.REVIEW_ID, mau.MAG_AUTO_UPDATE_ID,
		mau.STUDY_TYPE_CLASSIFIER, mau.USER_CLASSIFIER_MODEL_ID, mau.USER_CLASSIFIER_REVIEW_ID,
		cm.MODEL_TITLE
		from TB_MAG_AUTO_UPDATE_RUN mau
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mau.ATTRIBUTE_ID
		left outer join tb_CLASSIFIER_MODEL cm on cm.MODEL_ID = mau.USER_CLASSIFIER_MODEL_ID
		where mau.REVIEW_ID = @REVIEW_ID
		order by MAU.MAG_AUTO_UPDATE_ID
		
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRunCountResults]    Script Date: 20/11/2020 15:59:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
create or ALTER         PROCEDURE [dbo].[st_MagAutoUpdateRunCountResults] 
	-- Add the parameters for the stored procedure here
	
	@MagAutoUpdateRunId int
,	@AutoUpdateScore float
,	@StudyTypeClassifierScore float
,	@UserClassifierScore float
,	@ResultCount int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select @ResultCount = count(PaperId) from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId and
			ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateVisualise]    Script Date: 21/11/2020 11:03:01 ******/
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

INSERT INTO @VALS (VAL, TITLE)
VALUES (0, '0.0-'), (0.1, '0.1-'), (0.2, '0.2-'), (0.3, '0.3-'), (0.4, '0.4-'), (0.5, '0.5-'), (0.6, '0.6-'), (0.7, '0.7-'), (0.8, '0.8-'), (0.9, '-0.99')

IF @FIELD = 'AutoUpdate'
begin
SELECT VAL, TITLE,
	(SELECT COUNT(*) FROM TB_MAG_AUTO_UPDATE_RUN_PAPER WHERE 
	ContReviewScore >= VAL AND ContReviewScore < VAL + 10 AND MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID) AS NUM
FROM @VALS
end

IF @FIELD = 'StudyType'
begin
SELECT VAL, TITLE,
	(SELECT COUNT(*) FROM TB_MAG_AUTO_UPDATE_RUN_PAPER WHERE 
	StudyTypeClassifierScore >= VAL AND StudyTypeClassifierScore < VAL + 0.1 AND MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID) AS NUM
FROM @VALS
end

IF @FIELD = 'User'
begin
SELECT VAL, TITLE,
	(SELECT COUNT(*) FROM TB_MAG_AUTO_UPDATE_RUN_PAPER WHERE 
	UserClassifierScore >= VAL AND UserClassifierScore < VAL + 10 AND MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID) AS NUM
FROM @VALS
end



SET NOCOUNT OFF


GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateClassifierScoresUpdate]    Script Date: 22/11/2020 21:05:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
CREATE OR ALTER      PROCEDURE [dbo].[st_MagAutoUpdateClassifierScoresUpdate] 
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
		INNER JOIN TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP mac ON mac.MAG_AUTO_UPDATE_RUN_ID = maur.MAG_AUTO_UPDATE_RUN_ID and
			mac.PaperId = MAUR.PaperId

		UPDATE TB_MAG_AUTO_UPDATE_RUN
			SET STUDY_TYPE_CLASSIFIER = @StudyTypeClassifier
			WHERE MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID
	END

	IF @Field = 'UserClassifierScore'
	BEGIN
		UPDATE MAUR
		SET UserClassifierScore = SCORE
		FROM TB_MAG_AUTO_UPDATE_RUN_PAPER MAUR
		INNER JOIN TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP mac ON mac.MAG_AUTO_UPDATE_RUN_ID = MAUR.MAG_AUTO_UPDATE_RUN_ID and
			mac.PaperId = maur.PaperId

		UPDATE TB_MAG_AUTO_UPDATE_RUN
			SET USER_CLASSIFIER_MODEL_ID = @UserClassifierModelId,
				USER_CLASSIFIER_REVIEW_ID = @UserClassifierReviewId
			WHERE MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID
	END

	DELETE FROM TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP
		WHERE MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID

END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateVisualise]    Script Date: 22/11/2020 13:30:04 ******/
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
VALUES (0.3, '0.3-'), (0.4, '0.4-'), (0.5, '0.5-'), (0.6, '0.6-'), (0.7, '0.7-'), (0.8, '0.8-'), (0.9, '-0.99')

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

go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagContReviewInsertResults]    Script Date: 21/11/2020 21:00:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Last stage in the ContReview workflow: put the 'found' papers in and update tb_MAG_RELATED_RUN
-- =============================================
ALTER     PROCEDURE [dbo].[st_MagContReviewInsertResults] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE MAUR
		SET MAUR.N_PAPERS = idcount
		FROM TB_MAG_AUTO_UPDATE_RUN MAUR
		inner join (SELECT MAG_AUTO_UPDATE_RUN_ID, COUNT(*) idcount FROM TB_MAG_AUTO_UPDATE_RUN_PAPER
			GROUP BY MAG_AUTO_UPDATE_RUN_ID) AS COUNTS ON COUNTS.MAG_AUTO_UPDATE_RUN_ID = MAUR.MAG_AUTO_UPDATE_RUN_ID
		where maur.N_PAPERS = -1

	UPDATE MAURP
		SET MAURP.REVIEW_ID = MAUR.REVIEW_ID
		FROM TB_MAG_AUTO_UPDATE_RUN_PAPER MAURP
		INNER JOIN TB_MAG_AUTO_UPDATE_RUN MAUR on MAUR.MAG_AUTO_UPDATE_RUN_ID = MAURP.MAG_AUTO_UPDATE_RUN_ID
		WHERE MAURP.REVIEW_ID IS NULL

	-- i.e. in the rare situation that we have zero papers returned
	UPDATE TB_MAG_AUTO_UPDATE_RUN
		SET N_PAPERS = 0
		WHERE N_PAPERS = -1

END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagContReviewRunGetSeedIds]    Script Date: 22/11/2020 09:05:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get seed MAG IDs for the Continuous Review run when a new MAG is available
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagContReviewRunGetSeedIds] 

AS
BEGIN

	-- First, create new lines for each update in each review
	INSERT INTO TB_MAG_AUTO_UPDATE_RUN(MAG_AUTO_UPDATE_ID, USER_DESCRIPTION, ATTRIBUTE_ID,
		ALL_INCLUDED, DATE_RUN, N_PAPERS, REVIEW_ID)
	SELECT MAG_AUTO_UPDATE_ID, USER_DESCRIPTION, ATTRIBUTE_ID, ALL_INCLUDED, GETDATE(), -1, REVIEW_ID
		FROM TB_MAG_AUTO_UPDATE

	-- Next, grab the seed ids
	SELECT imm.PaperId, mau.MAG_AUTO_UPDATE_RUN_ID AutoUpdateId, 1 as Included
	from tb_ITEM_MAG_MATCH imm
	inner join TB_ITEM_REVIEW ir on ir.REVIEW_ID = imm.REVIEW_ID and imm.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 'false'
	inner join TB_MAG_AUTO_UPDATE_RUN mau on mau.REVIEW_ID = ir.REVIEW_ID
	where  (mau.ATTRIBUTE_ID = 0 OR mau.ATTRIBUTE_ID is null) and
		(imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)

	UNION ALL

	SELECT imm.PaperId, mau.MAG_AUTO_UPDATE_RUN_ID AutoUpdateId, 1 as Included
	from tb_ITEM_MAG_MATCH imm
	inner join TB_ITEM_REVIEW ir on ir.REVIEW_ID = imm.REVIEW_ID and imm.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 'false'
	inner join TB_MAG_AUTO_UPDATE_RUN mau on mau.REVIEW_ID = ir.REVIEW_ID
	inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = ir.ITEM_ID and ia.ATTRIBUTE_ID = mau.ATTRIBUTE_ID
	inner join TB_ITEM_SET iset on iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
	where not mau.ATTRIBUTE_ID is null and
	(imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)		
END
GO


