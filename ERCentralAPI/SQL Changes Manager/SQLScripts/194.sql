USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationResults]    Script Date: 01/05/2020 09:57:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagSimulationResults] 
	-- Add the parameters for the stored procedure here
	
	@MagSimulationId int
,	@OrderBy nvarchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
if @OrderBy = 'Network'
	SELECT DISTINCT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.NETWORK_STATISTIC_SCORE desc

if @OrderBy = 'FoS'
	SELECT DISTINCT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.FOS_DISTANCE_SCORE

if @OrderBy = 'User'
	SELECT DISTINCT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.USER_CLASSIFIER_MODEL_SCORE desc

if @OrderBy = 'StudyType'
	SELECT DISTINCT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.STUDY_TYPE_CLASSIFIER_SCORE desc

if @OrderBy = 'Ensemble'
	SELECT DISTINCT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.ENSEMBLE_SCORE desc
		
END
GO

USE [Reviewer]

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_MAG_SIMULATION_CLASSIFIER_TEMP')
BEGIN
BEGIN TRANSACTION
CREATE TABLE dbo.TB_MAG_SIMULATION_CLASSIFIER_TEMP
	(
	MAG_SIMULATION_ID int NULL,
	PaperId bigint NULL,
	Score float(53) NULL
	)  ON [PRIMARY]
ALTER TABLE dbo.TB_MAG_SIMULATION_CLASSIFIER_TEMP SET (LOCK_ESCALATION = TABLE)
COMMIT
END
GO

USE [Reviewer]

IF COL_LENGTH('dbo.TB_MAG_SIMULATION', 'USER_CLASSIFIER_REVIEW_ID') IS NULL
BEGIN
BEGIN TRANSACTION
ALTER TABLE dbo.TB_MAG_SIMULATION ADD
	USER_CLASSIFIER_REVIEW_ID int NULL
ALTER TABLE dbo.TB_MAG_SIMULATION SET (LOCK_ESCALATION = TABLE)
COMMIT
end
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationInsert]    Script Date: 01/05/2020 10:54:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
CREATE OR ALTER         PROCEDURE [dbo].[st_MagSimulationInsert] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int = 0
,	@YEAR INT = 0
,	@CREATED_DATE DATETIME = NULL
,	@WITH_THIS_ATTRIBUTE_ID BIGINT = 0
,	@FILTERED_BY_ATTRIBUTE_ID BIGINT = 0
,	@SEARCH_METHOD NVARCHAR(50) = NULL
,	@NETWORK_STATISTIC NVARCHAR(50) = NULL
,	@STUDY_TYPE_CLASSIFIER NVARCHAR(50) = NULL
,	@USER_CLASSIFIER_MODEL_ID INT = NULL
,	@STATUS NVARCHAR(50) = NULL
,	@YEAR_END INT = NULL
,	@CREATED_DATE_END DATETIME = NULL
,	@USER_CLASSIFIER_REVIEW_ID INT = NULL

,	@MAG_SIMULATION_ID INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	if @USER_CLASSIFIER_MODEL_ID = 0
		set @USER_CLASSIFIER_MODEL_ID = null

	INSERT INTO TB_MAG_SIMULATION
		(REVIEW_ID, YEAR, CREATED_DATE, WITH_THIS_ATTRIBUTE_ID, FILTERED_BY_ATTRIBUTE_ID, SEARCH_METHOD,
		NETWORK_STATISTIC, STUDY_TYPE_CLASSIFIER, USER_CLASSIFIER_MODEL_ID, STATUS, YEAR_END, CREATED_DATE_END,
		USER_CLASSIFIER_REVIEW_ID)
	VALUES(@REVIEW_ID, @YEAR, @CREATED_DATE, @WITH_THIS_ATTRIBUTE_ID, @FILTERED_BY_ATTRIBUTE_ID, @SEARCH_METHOD,
		@NETWORK_STATISTIC, @STUDY_TYPE_CLASSIFIER, @USER_CLASSIFIER_MODEL_ID, @STATUS, @YEAR_END, @CREATED_DATE_END,
		@USER_CLASSIFIER_REVIEW_ID)

	SET @MAG_SIMULATION_ID = @@IDENTITY
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulations]    Script Date: 01/05/2020 10:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all simulations for a given review
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagSimulations] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here


	SELECT ms.REVIEW_ID, ms.MAG_SIMULATION_ID, ms.YEAR, ms.CREATED_DATE, ms.WITH_THIS_ATTRIBUTE_ID, ms.FILTERED_BY_ATTRIBUTE_ID,
		ms.SEARCH_METHOD, ms.NETWORK_STATISTIC, ms.STUDY_TYPE_CLASSIFIER, ms.USER_CLASSIFIER_MODEL_ID, ms.USER_CLASSIFIER_REVIEW_ID,
		ms.STATUS, ms.tp, ms.FP, ms.FN, MS.NSEEDS, ms.YEAR_END, ms.CREATED_DATE_END,		
		a.ATTRIBUTE_NAME FilteredByAttribute,
		aa.ATTRIBUTE_NAME WithThisAttribute, cm.MODEL_TITLE
		
		from TB_MAG_SIMULATION ms
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = ms.FILTERED_BY_ATTRIBUTE_ID
		left outer join TB_ATTRIBUTE aa on aa.ATTRIBUTE_ID = ms.WITH_THIS_ATTRIBUTE_ID
		left outer join tb_CLASSIFIER_MODEL cm on cm.MODEL_ID = ms.USER_CLASSIFIER_MODEL_ID
		WHERE ms.REVIEW_ID = @REVIEW_ID
		order by ms.MAG_SIMULATION_ID
		

END
go
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationClassifierScoresUpdate]    Script Date: 01/05/2020 10:19:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
create or ALTER  PROCEDURE [dbo].[st_MagSimulationClassifierScoresUpdate] 
	-- Add the parameters for the stored procedure here
	@MAG_SIMULATION_ID int,
	@Field nvarchar(30)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	if @Field = 'STUDY_TYPE_CLASSIFIER_SCORE'
	BEGIN
		UPDATE MSR
		SET STUDY_TYPE_CLASSIFIER_SCORE = SCORE
		FROM TB_MAG_SIMULATION_RESULT MSR
		INNER JOIN TB_MAG_SIMULATION_CLASSIFIER_TEMP MSC ON MSC.MAG_SIMULATION_ID = MSR.MAG_SIMULATION_ID and MSC.PaperId = MSR.PaperId
	END

	IF @Field = 'USER_CLASSIFIER_MODEL_SCORE'
	BEGIN
		UPDATE MSR
		SET USER_CLASSIFIER_MODEL_SCORE = SCORE
		FROM TB_MAG_SIMULATION_RESULT MSR
		INNER JOIN TB_MAG_SIMULATION_CLASSIFIER_TEMP MSC ON MSC.MAG_SIMULATION_ID = MSR.MAG_SIMULATION_ID and MSC.PaperId = MSR.PaperId
	END

	DELETE FROM TB_MAG_SIMULATION_CLASSIFIER_TEMP
		WHERE MAG_SIMULATION_ID = @MAG_SIMULATION_ID

	UPDATE TB_MAG_SIMULATION_RESULT
			SET ENSEMBLE_SCORE = (NETWORK_STATISTIC_SCORE + USER_CLASSIFIER_MODEL_SCORE + STUDY_TYPE_CLASSIFIER_SCORE) / 3
			WHERE MAG_SIMULATION_ID = @MAG_SIMULATION_ID
END
GO