USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagContReviewRunGetSeedIds]    Script Date: 14/11/2020 20:03:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get seed MAG IDs for the Continuous Review run when a new MAG is available
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagContReviewRunGetSeedIds] 

AS
BEGIN
	SELECT imm.PaperId, mau.MAG_AUTO_UPDATE_ID AutoUpdateId, 1 as Included from tb_ITEM_MAG_MATCH imm
	inner join TB_ITEM_REVIEW ir on ir.REVIEW_ID = imm.REVIEW_ID and imm.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 'false'
	inner join TB_MAG_AUTO_UPDATE mau on mau.REVIEW_ID = ir.REVIEW_ID
	where  (mau.ATTRIBUTE_ID = 0 OR mau.ATTRIBUTE_ID is null) and
		(imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)

	UNION ALL

	SELECT imm.PaperId, mau.MAG_AUTO_UPDATE_ID AutoUpdateId, 1 as Included from tb_ITEM_MAG_MATCH imm
	inner join TB_ITEM_REVIEW ir on ir.REVIEW_ID = imm.REVIEW_ID and imm.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 'false'
	inner join TB_MAG_AUTO_UPDATE mau on mau.REVIEW_ID = ir.REVIEW_ID
	inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = ir.ITEM_ID and ia.ATTRIBUTE_ID = mau.ATTRIBUTE_ID
	inner join TB_ITEM_SET iset on iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
	where not mau.ATTRIBUTE_ID is null and
	(imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)		
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdates]    Script Date: 12/11/2020 14:35:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all runs for a given review
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagAutoUpdates] 
	-- Add the parameters for the stored procedure here	
	@REVIEW_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select mau.MAG_AUTO_UPDATE_ID, mau.USER_DESCRIPTION, mau.ATTRIBUTE_ID, mau.ALL_INCLUDED,
		mau.STUDY_TYPE_CLASSIFIER, mau.USER_CLASSIFIER_MODEL_ID, mau.USER_CLASSIFIER_REVIEW_ID,
		a.ATTRIBUTE_NAME, mau.REVIEW_ID, cm.MODEL_TITLE from TB_MAG_AUTO_UPDATE mau
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mau.ATTRIBUTE_ID
		left outer join tb_CLASSIFIER_MODEL cm on cm.MODEL_ID = mau.USER_CLASSIFIER_MODEL_ID
		where mau.REVIEW_ID = @REVIEW_ID
		order by MAU.MAG_AUTO_UPDATE_ID
		
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateInsert]    Script Date: 12/11/2020 12:19:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
CREATE OR ALTER     PROCEDURE [dbo].[st_MagAutoUpdateInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int
,	@USER_DESCRIPTION NVARCHAR(1000)
,	@ATTRIBUTE_ID bigint = 0
,	@ALL_INCLUDED BIT
,	@STUDY_TYPE_CLASSIFIER NVARCHAR(50)
,	@USER_CLASSIFIER_MODEL_ID INT = NULL
,	@USER_CLASSIFIER_MODEL_REVIEW_ID INT = NULL
,	@MAG_AUTO_UPDATE_ID int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	if @USER_CLASSIFIER_MODEL_ID = 0
	BEGIN
		SET @USER_CLASSIFIER_MODEL_ID = NULL
		SET @USER_CLASSIFIER_MODEL_REVIEW_ID = NULL
	END

	insert into TB_MAG_AUTO_UPDATE(REVIEW_ID, USER_DESCRIPTION, ATTRIBUTE_ID,
		ALL_INCLUDED, STUDY_TYPE_CLASSIFIER, USER_CLASSIFIER_MODEL_ID, USER_CLASSIFIER_REVIEW_ID)
	values(@REVIEW_ID, @USER_DESCRIPTION, @ATTRIBUTE_ID,
		@ALL_INCLUDED, @STUDY_TYPE_CLASSIFIER, @USER_CLASSIFIER_MODEL_ID, @USER_CLASSIFIER_MODEL_REVIEW_ID)

	set @MAG_AUTO_UPDATE_ID = @@IDENTITY

END
GO

IF COL_LENGTH('dbo.TB_MAG_AUTO_UPDATE', 'STUDY_TYPE_CLASSIFIER') IS NULL
BEGIN

BEGIN TRANSACTION
ALTER TABLE dbo.TB_MAG_AUTO_UPDATE
	DROP CONSTRAINT FK_TB_MAG_AUTO_UPDATE_tb_CLASSIFIER_MODEL

ALTER TABLE dbo.tb_CLASSIFIER_MODEL SET (LOCK_ESCALATION = TABLE)

COMMIT
BEGIN TRANSACTION

ALTER TABLE dbo.TB_MAG_AUTO_UPDATE
	DROP COLUMN STUDY_TYPE_CLASSIFIER, USER_CLASSIFIER_MODEL_ID, USER_CLASSIFIER_REVIEW_ID

ALTER TABLE dbo.TB_MAG_AUTO_UPDATE SET (LOCK_ESCALATION = TABLE)
COMMIT
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateInsert]    Script Date: 18/11/2020 17:40:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
CREATE OR ALTER       PROCEDURE [dbo].[st_MagAutoUpdateInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int
,	@USER_DESCRIPTION NVARCHAR(1000)
,	@ATTRIBUTE_ID bigint = 0
,	@ALL_INCLUDED BIT
,	@MAG_AUTO_UPDATE_ID int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here


	insert into TB_MAG_AUTO_UPDATE(REVIEW_ID, USER_DESCRIPTION, ATTRIBUTE_ID,
		ALL_INCLUDED)
	values(@REVIEW_ID, @USER_DESCRIPTION, @ATTRIBUTE_ID,
		@ALL_INCLUDED)

	set @MAG_AUTO_UPDATE_ID = @@IDENTITY

END
go
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRuns]    Script Date: 18/11/2020 17:42:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all runs for a given review
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_MagAutoUpdateRuns] 
	-- Add the parameters for the stored procedure here	
	@REVIEW_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select mau.MAG_AUTO_UPDATE_ID, mau.USER_DESCRIPTION, mau.ATTRIBUTE_ID, mau.ALL_INCLUDED,
		a.ATTRIBUTE_NAME, mau.DATE_RUN, mau.N_PAPERS from TB_MAG_AUTO_UPDATE_RUN mau
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mau.ATTRIBUTE_ID
		where mau.REVIEW_ID = @REVIEW_ID
		order by MAU.MAG_AUTO_UPDATE_ID
		
END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateVisualise]    Script Date: 19/11/2020 11:04:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or ALTER procedure [dbo].[st_MagAutoUpdateVisualise]
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
VALUES (0, '0-.09'), (0.1, '.1-.19'), (0.2, '.2-.29'), (0.3, '.3-.39'), (0.4, '.4-.49'), (0.5, '.5-.59'), (0.6, '.6-.69'), (0.7, '.7-.79'), (0.8, '.8-.89'), (0.9, '.9-.99')

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

/****** Object:  Table [dbo].[TB_MAG_SIMULATION_CLASSIFIER_TEMP]    Script Date: 19/11/2020 18:39:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP')
BEGIN
DROP TABLE [dbo].[TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP]
END
GO
CREATE TABLE [dbo].[TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP](
	[MAG_AUTO_UPDATE_RUN_ID] [int] NULL,
	[PaperId] [bigint] NULL,
	[Score] [float] NULL
) ON [PRIMARY]
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRunResults]    Script Date: 19/11/2020 17:49:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagAutoUpdateRunResults] 
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
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateClassifierScoresUpdate]    Script Date: 19/11/2020 18:41:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
create or ALTER  PROCEDURE [dbo].[st_MagAutoUpdateClassifierScoresUpdate] 
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
	END

	DELETE FROM TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP
		WHERE MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID

END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRuns]    Script Date: 19/11/2020 23:01:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all runs for a given review
-- =============================================
create or ALTER         PROCEDURE [dbo].[st_MagAutoUpdateRuns] 
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
		mau.STUDY_TYPE_CLASSIFIER, mau.USER_CLASSIFIER_MODEL_ID, mau.USER_CLASSIFIER_REVIEW_ID
		from TB_MAG_AUTO_UPDATE_RUN mau
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mau.ATTRIBUTE_ID
		where mau.REVIEW_ID = @REVIEW_ID
		order by MAU.MAG_AUTO_UPDATE_ID
		
END
go


