USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ClassifierDeleteModel]    Script Date: 10/17/2016 11:16:16 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ClassifierDeleteModel]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_ClassifierDeleteModel]
GO

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ClassifierDeleteModel]    Script Date: 10/17/2016 11:16:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ClassifierDeleteModel]
(
	@REVIEW_ID INT
,	@MODEL_ID INT OUTPUT
)

As

SET NOCOUNT ON

	DELETE FROM tb_CLASSIFIER_MODEL WHERE REVIEW_ID = @REVIEW_ID AND MODEL_ID = @MODEL_ID

	

SET NOCOUNT OFF


GO



Use Reviewer
GO
update TB_IMPORT_FILTER set OLD_ITEM_ID = 'PMID-' where IMPORT_FILTER_NAME = 'PubMed'
GO

/* ***********BELOW HERE YOU MAY ALREADY HAVE THESE CHANGES ****************** */
/* **************************************************************************** */



alter table dbo.tb_meta_analysis
ADD
	[Randomised] [int] NULL,
	[RoB] [int] NULL,
	[RoBComment] [ntext] NULL,
	[RoBSequence] [bit] NULL,
	[RoBConcealment] [bit] NULL,
	[RoBBlindingParticipants] [bit] NULL,
	[RoBBlindingAssessors] [bit] NULL,
	[RoBIncomplete] [bit] NULL,
	[RoBSelective] [bit] NULL,
	[RoBNoIntention] [bit] NULL,
	[RoBCarryover] [bit] NULL,
	[RoBStopped] [bit] NULL,
	[RoBUnvalidated] [bit] NULL,
	[RoBOther] [bit] NULL,
	[Incon] [int] NULL,
	[InconComment] [ntext] NULL,
	[InconPoint] [bit] NULL,
	[InconCIs] [bit] NULL,
	[InconDirection] [bit] NULL,
	[InconStatistical] [bit] NULL,
	[InconOther] [bit] NULL,
	[Indirect] [int] NULL,
	[IndirectComment] [ntext] NULL,
	[IndirectPopulation] [bit] NULL,
	[IndirectOutcome] [bit] NULL,
	[IndirectNoDirect] [bit] NULL,
	[IndirectIntervention] [bit] NULL,
	[IndirectTime] [bit] NULL,
	[IndirectOther] [bit] NULL,
	[Imprec] [int] NULL,
	[ImprecComment] [ntext] NULL,
	[ImprecWide] [bit] NULL,
	[ImprecFew] [bit] NULL,
	[ImprecOnlyOne] [bit] NULL,
	[ImprecOther] [bit] NULL,
	[PubBias] [int] NULL,
	[PubBiasComment] [ntext] NULL,
	[PubBiasCommercially] [bit] NULL,
	[PubBiasAsymmetrical] [bit] NULL,
	[PubBiasLimited] [bit] NULL,
	[PubBiasMissing] [bit] NULL,
	[PubBiasDiscontinued] [bit] NULL,
	[PubBiasDiscrepancy] [bit] NULL,
	[PubBiasOther] [bit] NULL,
	[UpgradeComment] [ntext] NULL,
	[UpgradeLarge] [bit] NULL,
	[UpgradeVeryLarge] [bit] NULL,
	[UpgradeAllPlausible] [bit] NULL,
	[UpgradeClear] [bit] NULL,
	[UpgradeNone] [bit] NULL,
	[CertaintyLevel] [int] NULL,
	[CertaintyLevelComment] [ntext] NULL

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisInsert]    Script Date: 10/17/2016 10:53:29 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_MetaAnalysisInsert]
(
	@TITLE NVARCHAR(255),
	@CONTACT_ID INT,
	@REVIEW_ID INT,
	@ATTRIBUTE_ID BIGINT = NULL,
	@SET_ID INT = NULL,
	@ATTRIBUTE_ID_INTERVENTION BIGINT,
	@ATTRIBUTE_ID_CONTROL BIGINT,
	@ATTRIBUTE_ID_OUTCOME BIGINT,
	@META_ANALYSIS_TYPE_ID INT,
	@OUTCOME_IDS nvarchar(max),
	@ATTRIBUTE_ID_ANSWER nvarchar(max) = '',
	@ATTRIBUTE_ID_QUESTION nvarchar(max) = '',
	@GRID_SETTINGS NVARCHAR(MAX) = '',
	@NEW_META_ANALYSIS_ID INT OUTPUT,

	@Randomised int  = NULL,
	@RoB [int]  = NULL,
	@RoBComment [ntext]  = NULL,
	@RoBSequence [bit] =  NULL,
	@RoBConcealment [bit] =  NULL,
	@RoBBlindingParticipants [bit]  = NULL,
	@RoBBlindingAssessors [bit]  = NULL,
	@RoBIncomplete [bit]  = NULL,
	@RoBSelective [bit]  = NULL,
	@RoBNoIntention [bit] =  NULL,
	@RoBCarryover [bit] =  NULL,
	@RoBStopped [bit]  = NULL,
	@RoBUnvalidated [bit]  = NULL,
	@RoBOther [bit] =  NULL,
	@Incon [int]  = NULL,
	@InconComment [ntext]  = NULL,
	@InconPoint [bit] =  NULL,
	@InconCIs [bit]  = NULL,
	@InconDirection [bit]  = NULL,
	@InconStatistical [bit]  = NULL,
	@InconOther [bit]  = NULL,
	@Indirect [int]  = NULL,
	@IndirectComment [ntext]  = NULL,
	@IndirectPopulation [bit] =  NULL,
	@IndirectOutcome [bit] =  NULL,
	@IndirectNoDirect [bit]  = NULL,
	@IndirectIntervention [bit]  = NULL,
	@IndirectTime [bit]  = NULL,
	@IndirectOther [bit]  = NULL,
	@Imprec [int]  = NULL,
	@ImprecComment [ntext]  = NULL,
	@ImprecWide [bit]  = NULL,
	@ImprecFew [bit]  = NULL,
	@ImprecOnlyOne [bit]  = NULL,
	@ImprecOther [bit]  = NULL,
	@PubBias [int]  = NULL,
	@PubBiasComment [ntext]  = NULL,
	@PubBiasCommercially [bit]  = NULL,
	@PubBiasAsymmetrical [bit] =  NULL,
	@PubBiasLimited [bit]  = NULL,
	@PubBiasMissing [bit] = NULL,
	@PubBiasDiscontinued [bit] =  NULL,
	@PubBiasDiscrepancy [bit] =  NULL,
	@PubBiasOther [bit] =  NULL,
	@UpgradeComment [ntext] =  NULL,
	@UpgradeLarge [bit] =  NULL,
	@UpgradeVeryLarge [bit]  = NULL,
	@UpgradeAllPlausible [bit]  = NULL,
	@UpgradeClear [bit]  = NULL,
	@UpgradeNone [bit]  = NULL,
	@CertaintyLevel [int]  = NULL,
	@CertaintyLevelComment [ntext] =  NULL,

	@ATTRIBUTE_ANSWER_TEXT NVARCHAR(MAX) OUTPUT,
	@ATTRIBUTE_QUESTION_TEXT NVARCHAR(MAX) OUTPUT
)

As

SET NOCOUNT ON
	
	INSERT INTO TB_META_ANALYSIS
	(	META_ANALYSIS_TITLE
	,	CONTACT_ID
	,	REVIEW_ID
	,	ATTRIBUTE_ID
	,	SET_ID
	,	ATTRIBUTE_ID_INTERVENTION
	,	ATTRIBUTE_ID_CONTROL
	,	ATTRIBUTE_ID_OUTCOME
	,	META_ANALYSIS_TYPE_ID
	,	ATTRIBUTE_ID_ANSWER
	,	ATTRIBUTE_ID_QUESTION
	,	GRID_SETTINGS,
	[Randomised],
	[RoB],
	[RoBComment],
	[RoBSequence],
	[RoBConcealment],
	[RoBBlindingParticipants],
	[RoBBlindingAssessors],
	[RoBIncomplete],
	[RoBSelective],
	[RoBNoIntention],
	[RoBCarryover],
	[RoBStopped],
	[RoBUnvalidated],
	[RoBOther],
	[Incon],
	[InconComment],
	[InconPoint],
	[InconCIs],
	[InconDirection],
	[InconStatistical],
	[InconOther],
	[Indirect],
	[IndirectComment],
	[IndirectPopulation],
	[IndirectOutcome],
	[IndirectNoDirect],
	[IndirectIntervention],
	[IndirectTime],
	[IndirectOther],
	[Imprec],
	[ImprecComment],
	[ImprecWide],
	[ImprecFew],
	[ImprecOnlyOne],
	[ImprecOther],
	[PubBias],
	[PubBiasComment],
	[PubBiasCommercially],
	[PubBiasAsymmetrical],
	[PubBiasLimited],
	[PubBiasMissing],
	[PubBiasDiscontinued],
	[PubBiasDiscrepancy],
	[PubBiasOther],
	[UpgradeComment],
	[UpgradeLarge],
	[UpgradeVeryLarge],
	[UpgradeAllPlausible],
	[UpgradeClear],
	[UpgradeNone],
	[CertaintyLevel],
	[CertaintyLevelComment]
	)	
	VALUES
	(
		@TITLE
	,	@CONTACT_ID
	,	@REVIEW_ID
	,	@ATTRIBUTE_ID
	,	@SET_ID
	,	@ATTRIBUTE_ID_INTERVENTION
	,	@ATTRIBUTE_ID_CONTROL
	,	@ATTRIBUTE_ID_OUTCOME
	,	@META_ANALYSIS_TYPE_ID
	,	@ATTRIBUTE_ID_ANSWER
	,	@ATTRIBUTE_ID_QUESTION
	,	@GRID_SETTINGS,
	@Randomised,
	@RoB,
	@RoBComment,
	@RoBSequence,
	@RoBConcealment,
	@RoBBlindingParticipants,
	@RoBBlindingAssessors,
	@RoBIncomplete,
	@RoBSelective,
	@RoBNoIntention,
	@RoBCarryover,
	@RoBStopped,
	@RoBUnvalidated,
	@RoBOther,
	@Incon,
	@InconComment,
	@InconPoint,
	@InconCIs,
	@InconDirection,
	@InconStatistical,
	@InconOther,
	@Indirect,
	@IndirectComment,
	@IndirectPopulation,
	@IndirectOutcome,
	@IndirectNoDirect,
	@IndirectIntervention,
	@IndirectTime,
	@IndirectOther,
	@Imprec,
	@ImprecComment,
	@ImprecWide,
	@ImprecFew,
	@ImprecOnlyOne,
	@ImprecOther,
	@PubBias,
	@PubBiasComment,
	@PubBiasCommercially,
	@PubBiasAsymmetrical,
	@PubBiasLimited,
	@PubBiasMissing,
	@PubBiasDiscontinued,
	@PubBiasDiscrepancy,
	@PubBiasOther,
	@UpgradeComment,
	@UpgradeLarge,
	@UpgradeVeryLarge,
	@UpgradeAllPlausible,
	@UpgradeClear,
	@UpgradeNone,
	@CertaintyLevel,
	@CertaintyLevelComment
	)
	-- Get the identity and return it
	SET @NEW_META_ANALYSIS_ID = @@identity
	
	IF (@OUTCOME_IDS != '')
	BEGIN
		INSERT INTO TB_META_ANALYSIS_OUTCOME (META_ANALYSIS_ID, OUTCOME_ID)
		SELECT @NEW_META_ANALYSIS_ID, VALUE from DBO.fn_Split_int(@OUTCOME_IDS, ',')
	END

	SELECT @ATTRIBUTE_ANSWER_TEXT = COALESCE(@ATTRIBUTE_ANSWER_TEXT + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_ANSWER, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL

	SELECT @ATTRIBUTE_QUESTION_TEXT = COALESCE(@ATTRIBUTE_QUESTION_TEXT + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_QUESTION, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL

SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisUpdate]    Script Date: 10/14/2016 2:29:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_MetaAnalysisUpdate]
(
	@META_ANALYSIS_ID INT,
	@TITLE NVARCHAR(255),
	@CONTACT_ID INT,
	@REVIEW_ID INT,
	@ATTRIBUTE_ID BIGINT = NULL,
	@SET_ID INT = NULL,
	@ATTRIBUTE_ID_INTERVENTION BIGINT,
	@ATTRIBUTE_ID_CONTROL BIGINT,
	@ATTRIBUTE_ID_OUTCOME BIGINT,
	@OUTCOME_IDS nvarchar(max),
	@ATTRIBUTE_ID_ANSWER NVARCHAR(MAX) = '',
	@ATTRIBUTE_ID_QUESTION NVARCHAR(MAX) = '',
	@META_ANALYSIS_TYPE_ID INT,
	@GRID_SETTINGS NVARCHAR(MAX) = '',

	@Randomised int  = NULL,
	@RoB [int]  = NULL,
	@RoBComment [ntext]  = NULL,
	@RoBSequence [bit] =  NULL,
	@RoBConcealment [bit] =  NULL,
	@RoBBlindingParticipants [bit]  = NULL,
	@RoBBlindingAssessors [bit]  = NULL,
	@RoBIncomplete [bit]  = NULL,
	@RoBSelective [bit]  = NULL,
	@RoBNoIntention [bit] =  NULL,
	@RoBCarryover [bit] =  NULL,
	@RoBStopped [bit]  = NULL,
	@RoBUnvalidated [bit]  = NULL,
	@RoBOther [bit] =  NULL,
	@Incon [int]  = NULL,
	@InconComment [ntext]  = NULL,
	@InconPoint [bit] =  NULL,
	@InconCIs [bit]  = NULL,
	@InconDirection [bit]  = NULL,
	@InconStatistical [bit]  = NULL,
	@InconOther [bit]  = NULL,
	@Indirect [int]  = NULL,
	@IndirectComment [ntext]  = NULL,
	@IndirectPopulation [bit] =  NULL,
	@IndirectOutcome [bit] =  NULL,
	@IndirectNoDirect [bit]  = NULL,
	@IndirectIntervention [bit]  = NULL,
	@IndirectTime [bit]  = NULL,
	@IndirectOther [bit]  = NULL,
	@Imprec [int]  = NULL,
	@ImprecComment [ntext]  = NULL,
	@ImprecWide [bit]  = NULL,
	@ImprecFew [bit]  = NULL,
	@ImprecOnlyOne [bit]  = NULL,
	@ImprecOther [bit]  = NULL,
	@PubBias [int]  = NULL,
	@PubBiasComment [ntext]  = NULL,
	@PubBiasCommercially [bit]  = NULL,
	@PubBiasAsymmetrical [bit] =  NULL,
	@PubBiasLimited [bit]  = NULL,
	@PubBiasMissing [bit] = NULL,
	@PubBiasDiscontinued [bit] =  NULL,
	@PubBiasDiscrepancy [bit] =  NULL,
	@PubBiasOther [bit] =  NULL,
	@UpgradeComment [ntext] =  NULL,
	@UpgradeLarge [bit] =  NULL,
	@UpgradeVeryLarge [bit]  = NULL,
	@UpgradeAllPlausible [bit]  = NULL,
	@UpgradeClear [bit]  = NULL,
	@UpgradeNone [bit]  = NULL,
	@CertaintyLevel [int]  = NULL,
	@CertaintyLevelComment [ntext] =  NULL,

	@ATTRIBUTE_ANSWER_TEXT NVARCHAR(MAX) OUTPUT,
	@ATTRIBUTE_QUESTION_TEXT NVARCHAR(MAX) OUTPUT
)

As

SET NOCOUNT ON
	
	UPDATE TB_META_ANALYSIS
	SET	META_ANALYSIS_TITLE = @TITLE
	,	CONTACT_ID = @CONTACT_ID
	,	REVIEW_ID = @REVIEW_ID
	,	ATTRIBUTE_ID = @ATTRIBUTE_ID
	,	SET_ID = @SET_ID
	,	ATTRIBUTE_ID_INTERVENTION = @ATTRIBUTE_ID_INTERVENTION
	,	ATTRIBUTE_ID_CONTROL = @ATTRIBUTE_ID_CONTROL
	,	ATTRIBUTE_ID_OUTCOME = @ATTRIBUTE_ID_OUTCOME
	,	META_ANALYSIS_TYPE_ID = @META_ANALYSIS_TYPE_ID
	,	ATTRIBUTE_ID_ANSWER = @ATTRIBUTE_ID_ANSWER
	,	ATTRIBUTE_ID_QUESTION = @ATTRIBUTE_ID_QUESTION
	,	GRID_SETTINGS = @GRID_SETTINGS
	,	Randomised = @Randomised
	,	Rob = @RoB
	,	RoBComment = @RoBComment
	,	RoBSequence = @RoBSequence
	,	RoBConcealment = @RoBConcealment
	,	RoBBlindingParticipants = @RoBBlindingParticipants
	,	RoBBlindingAssessors = @RoBBlindingAssessors
	,	RoBIncomplete = @RoBIncomplete
	,	RoBSelective = @RoBSelective
	,	RoBNoIntention = @RoBNoIntention
	,	RoBCarryover = @RoBCarryover
	,	RoBStopped = @RoBStopped
	,	RoBUnvalidated = @RoBUnvalidated
	,	RoBOther = @RoBOther
	,	Incon = @Incon
	,	InconComment = @InconComment
	,	InconPoint = @InconPoint
	,	InconCIs = @InconCIs
	,	InconDirection = @InconDirection
	,	InconStatistical = @InconStatistical
	,	InconOther = @InconOther
	,	Indirect = @Indirect
	,	IndirectComment = @IndirectComment
	,	IndirectPopulation = @IndirectPopulation
	,	IndirectOutcome = @IndirectOutcome
	,	IndirectNoDirect = @IndirectNoDirect
	,	IndirectIntervention = @IndirectIntervention
	,	IndirectTime = @IndirectTime
	,	IndirectOther = @IndirectOther
	,	Imprec = @Imprec
	,	ImprecComment = @ImprecComment
	,	ImprecWide = @ImprecWide
	,	ImprecFew = @ImprecFew
	,	ImprecOnlyOne = @ImprecOnlyOne
	,	ImprecOther = @ImprecOther
	,	PubBias = @PubBias
	,	PubBiasComment = @PubBiasComment
	,	PubBiasCommercially = @PubBiasCommercially
	,	PubBiasAsymmetrical = @PubBiasAsymmetrical
	,	PubBiasLimited = @PubBiasLimited
	,	PubBiasMissing = @PubBiasMissing
	,	PubBiasDiscontinued = @PubBiasDiscontinued
	,	PubBiasDiscrepancy = @PubBiasDiscrepancy
	,	PubBiasOther = @PubBiasOther
	,	UpgradeComment = @UpgradeComment
	,	UpgradeLarge = @UpgradeLarge
	,	UpgradeVeryLarge = @UpgradeVeryLarge
	,	UpgradeAllPlausible = @UpgradeAllPlausible
	,	UpgradeClear = @UpgradeClear
	,	UpgradeNone = @UpgradeNone
	,	CertaintyLevel = @CertaintyLevel
	,	CertaintyLevelComment = @CertaintyLevelComment
	
	WHERE META_ANALYSIS_ID = @META_ANALYSIS_ID
	
	DELETE FROM TB_META_ANALYSIS_OUTCOME WHERE META_ANALYSIS_ID = @META_ANALYSIS_ID
	
	IF (@OUTCOME_IDS != '')
	BEGIN
		INSERT INTO TB_META_ANALYSIS_OUTCOME (META_ANALYSIS_ID, OUTCOME_ID)
		SELECT @META_ANALYSIS_ID, VALUE from DBO.fn_Split_int(@OUTCOME_IDS, ',')
	END

	SELECT @ATTRIBUTE_ANSWER_TEXT = COALESCE(@ATTRIBUTE_ANSWER_TEXT + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_ANSWER, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL

	SELECT @ATTRIBUTE_QUESTION_TEXT = COALESCE(@ATTRIBUTE_QUESTION_TEXT + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_QUESTION, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL

SET NOCOUNT OFF

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisList]    Script Date: 10/14/2016 2:29:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_MetaAnalysisList]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT
)

As

SET NOCOUNT ON
	
	SELECT META_ANALYSIS_ID, META_ANALYSIS_TITLE, TB_META_ANALYSIS.CONTACT_ID, REVIEW_ID,
	TB_META_ANALYSIS.ATTRIBUTE_ID, SET_ID, ATTRIBUTE_ID_INTERVENTION, ATTRIBUTE_ID_CONTROL,
	ATTRIBUTE_ID_OUTCOME, TB_META_ANALYSIS.META_ANALYSIS_TYPE_ID, META_ANALYSIS_TYPE_TITLE,
	ATTRIBUTE_ID_ANSWER, ATTRIBUTE_ID_QUESTION,	GRID_SETTINGS,
	Randomised, RoB, RoBComment, RoBSequence, RoBConcealment, RoBBlindingParticipants, RoBBlindingAssessors, RoBIncomplete, RoBSelective, 
	 RoBNoIntention, RoBCarryover, RoBStopped, RoBUnvalidated, RoBOther, Incon, InconComment, InconPoint, InconCIs, InconDirection, 
	 InconStatistical, InconOther, Indirect, IndirectComment, IndirectPopulation, IndirectOutcome, IndirectNoDirect, IndirectIntervention, 
	 IndirectTime, IndirectOther, Imprec, ImprecComment, ImprecWide, ImprecFew, ImprecOnlyOne, ImprecOther, PubBias, PubBiasComment, 
	 PubBiasCommercially, PubBiasAsymmetrical, PubBiasLimited, PubBiasMissing, PubBiasDiscontinued, PubBiasDiscrepancy, PubBiasOther, 
	 UpgradeComment, UpgradeLarge, UpgradeVeryLarge, UpgradeAllPlausible, UpgradeClear, UpgradeNone, CertaintyLevel, CertaintyLevelComment,

	--A1.ATTRIBUTE_NAME AS INTERVENTION_TEXT,	a2.ATTRIBUTE_NAME AS CONTROL_TEXT, a3.ATTRIBUTE_NAME AS OUTCOME_TEXT,

	(SELECT ATTRIBUTE_NAME + '¬' FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_ANSWER, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		FOR XML PATH('')) AS ATTRIBUTE_ANSWER_TEXT,

	(SELECT ATTRIBUTE_NAME + '¬' FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_QUESTION, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		FOR XML PATH('')) AS ATTRIBUTE_QUESTION_TEXT
	
	FROM TB_META_ANALYSIS
	
	INNER JOIN TB_META_ANALYSIS_TYPE ON TB_META_ANALYSIS_TYPE.META_ANALYSIS_TYPE_ID =
		TB_META_ANALYSIS.META_ANALYSIS_TYPE_ID
	
	/*	
	don't need this any more (for old MA interface) ??
	left outer JOIN TB_ITEM_ATTRIBUTE IA1 ON IA1.ITEM_ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_INTERVENTION
	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_INTERVENTION 
	left outer JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_CONTROL
	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_CONTROL
	left outer JOIN TB_ITEM_ATTRIBUTE IA3 ON IA3.ITEM_ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_OUTCOME
	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_OUTCOME 
	*/
	WHERE REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF


/*

(SELECT COALESCE(ATTRIBUTE_ANSWER_TEXT + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_ANSWER, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL) ATTRIBUTE_ANSWER_TEXT

		*/
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysis]    Script Date: 10/14/2016 2:30:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_MetaAnalysis]
(
	@META_ANALYSIS_ID INT
)

As

SET NOCOUNT ON
	
	SELECT META_ANALYSIS_ID, META_ANALYSIS_TITLE, CONTACT_ID, REVIEW_ID,
	ATTRIBUTE_ID, SET_ID, ATTRIBUTE_ID_INTERVENTION, ATTRIBUTE_ID_CONTROL,
	ATTRIBUTE_ID_ANSWER, ATTRIBUTE_ID_QUESTION,	ATTRIBUTE_ID_OUTCOME, META_ANALYSIS_TYPE_ID,

	 Randomised, RoB, RoBComment, RoBSequence, RoBConcealment, RoBBlindingParticipants, RoBBlindingAssessors, RoBIncomplete, RoBSelective, 
	 RoBNoIntention, RoBCarryover, RoBStopped, RoBUnvalidated, RoBOther, Incon, InconComment, InconPoint, InconCIs, InconDirection, 
	 InconStatistical, InconOther, Indirect, IndirectComment, IndirectPopulation, IndirectOutcome, IndirectNoDirect, IndirectIntervention, 
	 IndirectTime, IndirectOther, Imprec, ImprecComment, ImprecWide, ImprecFew, ImprecOnlyOne, ImprecOther, PubBias, PubBiasComment, 
	 PubBiasCommercially, PubBiasAsymmetrical, PubBiasLimited, PubBiasMissing, PubBiasDiscontinued, PubBiasDiscrepancy, PubBiasOther, 
	 UpgradeComment, UpgradeLarge, UpgradeVeryLarge, UpgradeAllPlausible, UpgradeClear, UpgradeNone, CertaintyLevel, CertaintyLevelComment
	
	FROM TB_META_ANALYSIS
	
	WHERE META_ANALYSIS_ID = @META_ANALYSIS_ID

SET NOCOUNT OFF
go








USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierGetClassificationData]    Script Date: 10/14/2016 2:33:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ClassifierGetClassificationData]
(
	@REVIEW_ID INT
,	@ATTRIBUTE_ID_CLASSIFY_TO BIGINT = NULL
)

As

SET NOCOUNT ON

	DELETE FROM TB_CLASSIFIER_ITEM_TEMP WHERE REVIEW_ID = @REVIEW_ID	

	IF @ATTRIBUTE_ID_CLASSIFY_TO > -1
	BEGIN
		SELECT DISTINCT '99' LABEL, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		WHERE REVIEW_ID = @REVIEW_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_CLASSIFY_TO AND IS_DELETED = 'FALSE'
	END
	ELSE
	BEGIN
		SELECT DISTINCT '99' LABEL, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, KEYWORDS FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID
		WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED = 'FALSE'
	END
		
SET NOCOUNT OFF

go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSearchList]    Script Date: 10/14/2016 2:34:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemSearchList] (
      @REVIEW_ID INT,
      @SEARCH_ID INT,
      
      @PageNum INT = 1,
      @PerPage INT = 3,
      @CurrentPage INT OUTPUT,
      @TotalPages INT OUTPUT,
      @TotalRows INT OUTPUT
)

As

SET NOCOUNT ON

declare @RowsToRetrieve int

      SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
      FROM TB_ITEM_REVIEW I
      INNER JOIN TB_SEARCH_ITEM ON TB_SEARCH_ITEM.ITEM_ID = I.ITEM_ID
      AND TB_SEARCH_ITEM.SEARCH_ID = @SEARCH_ID
      AND I.REVIEW_ID = @REVIEW_ID

set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, ITEM_RANK, TB_ITEM_REVIEW.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by ITEM_RANK desc) RowNum
      FROM TB_ITEM I
            INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
                  TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            INNER JOIN TB_SEARCH_ITEM ON TB_SEARCH_ITEM.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
                  AND TB_SEARCH_ITEM.SEARCH_ID = @SEARCH_ID
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  ,SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS, ITEM_RANK
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve
                  ORDER BY ITEM_RANK desc
                  
      OPTION (OPTIMIZE FOR (@PerPage=700, @SEARCH_ID UNKNOWN))
SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'


SET NOCOUNT OFF

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemList]    Script Date: 10/14/2016 2:34:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemList]
(
      @REVIEW_ID INT,
      @SHOW_INCLUDED BIT = 'true',
      @SHOW_DELETED BIT = 'false',
      @SOURCE_ID INT = 0,
      @ATTRIBUTE_SET_ID_LIST NVARCHAR(MAX) = '',
      
      @PageNum INT = 1,
      @PerPage INT = 3,
      @CurrentPage INT OUTPUT,
      @TotalPages INT OUTPUT,
      @TotalRows INT OUTPUT 
)

As

SET NOCOUNT ON

declare @RowsToRetrieve int
Declare @ID table (ItemID bigint primary key )
IF (@SOURCE_ID = 0) AND (@ATTRIBUTE_SET_ID_LIST = '') /* LIST ALL ITEMS IN THE REVIEW */
BEGIN

       --store IDs to build paged results as a simple join
	  INSERT INTO @ID SELECT DISTINCT (I.ITEM_ID)
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
            AND TB_ITEM_REVIEW.IS_DELETED = @SHOW_DELETED

	  SELECT @TotalRows = @@ROWCOUNT

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (ir.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM i
			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID
			INNER JOIN @ID id on id.ItemID = ir.ITEM_ID
            
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve
            ORDER BY SHORT_TITLE
END
ELSE /* FILTER BY A LIST OF ATTRIBUTES */

IF (@ATTRIBUTE_SET_ID_LIST != '')
BEGIN
      SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
            FROM TB_ITEM_REVIEW I
      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
      INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
      INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!

      WHERE I.IS_INCLUDED = @SHOW_INCLUDED
            AND I.IS_DELETED = @SHOW_DELETED
            AND I.REVIEW_ID = @REVIEW_ID

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID, ADDITIONAL_TEXT,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM I
       INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
            AND TB_ITEM_REVIEW.IS_DELETED = @SHOW_DELETED

      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS, ADDITIONAL_TEXT
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve 
            ORDER BY SHORT_TITLE
END
ELSE -- LISTING SOURCELESS
IF (@SOURCE_ID = -1)
BEGIN
       --store IDs to build paged results as a simple join
	  INSERT INTO @ID SELECT DISTINCT IR.ITEM_ID
		from TB_ITEM_REVIEW IR 
      LEFT OUTER JOIN TB_ITEM_SOURCE TIS on IR.ITEM_ID = TIS.ITEM_ID
      LEFT OUTER JOIN TB_SOURCE TS on TIS.SOURCE_ID = TS.SOURCE_ID and IR.REVIEW_ID = TS.REVIEW_ID
      where IR.REVIEW_ID = @REVIEW_ID and TS.SOURCE_ID  is null

	  SELECT @TotalRows = @@ROWCOUNT
      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IR.IS_DELETED, IS_INCLUDED, IR.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] 
      INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = I.ITEM_ID AND IR.REVIEW_ID = @REVIEW_ID
      INNER JOIN @ID id on id.ItemID = I.ITEM_ID
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve 
            ORDER BY SHORT_TITLE
      
END
ELSE -- LISTING BY A SOURCE
BEGIN
      SELECT @TotalRows = count(I.ITEM_ID)
      FROM TB_ITEM_REVIEW I
      INNER JOIN TB_ITEM_SOURCE ON TB_ITEM_SOURCE.ITEM_ID = I.ITEM_ID AND TB_ITEM_SOURCE.SOURCE_ID = @SOURCE_ID
      WHERE I.REVIEW_ID = @REVIEW_ID

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
      INNER JOIN TB_ITEM_SOURCE ON TB_ITEM_SOURCE.ITEM_ID = I.ITEM_ID AND TB_ITEM_SOURCE.SOURCE_ID = @SOURCE_ID
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve
            ORDER BY SHORT_TITLE
      
END

SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'


SET NOCOUNT OFF

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierCreateSearchList]    Script Date: 10/14/2016 2:35:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ClassifierCreateSearchList]
(
	@REVIEW_ID INT
,	@CONTACT_ID INT
,	@SEARCH_TITLE NVARCHAR(4000)
,	@SEARCH_DESC varchar(4000) = null
,	@HITS_NO INT
,	@NEW_SEARCH_ID INT OUTPUT
)

As

SET NOCOUNT ON

	-- STEP 1: GET THE SEARCH NUMBER FOR THIS REVIEW
	DECLARE @SEARCH_NO INT
	SELECT @SEARCH_NO = ISNULL(MAX(SEARCH_NO), 0) + 1 FROM tb_SEARCH WHERE REVIEW_ID = @REVIEW_ID

	-- STEP 2: CREATE THE SEARCH RECORD
	INSERT INTO tb_SEARCH
	(	REVIEW_ID
	,	CONTACT_ID
	,	SEARCH_TITLE
	,	SEARCH_NO
	,	HITS_NO
	,	SEARCH_DATE
	)	
	VALUES
	(
		@REVIEW_ID
	,	@CONTACT_ID
	,	@SEARCH_TITLE
	,	@SEARCH_NO
	,	@HITS_NO
	,	GetDate()
	)
	-- Get the identity and return it
	SET @NEW_SEARCH_ID = @@identity
	
	-- STEP 3: PUT THE ITEMS INTO THE SEARCH LIST
	
	INSERT INTO TB_SEARCH_ITEM(ITEM_ID,SEARCH_ID, ITEM_RANK)
		SELECT CIT.ITEM_ID, @NEW_SEARCH_ID, CAST(SCORE * 100 AS INT)
				FROM TB_CLASSIFIER_ITEM_TEMP CIT
			ORDER BY CIT.SCORE DESC
	
	/*
	-- STEP 3: PUT THE ITEM_RANK VALUES IN (I.E. SCORES FROM 1 TO N)
	DECLARE @START_INDEX INT = 0
	SELECT @START_INDEX = MIN(SEARCH_ITEM_ID) FROM TB_SEARCH_ITEM WHERE SEARCH_ID = @NEW_SEARCH_ID
	UPDATE TB_SEARCH_ITEM
		SET ITEM_RANK = SEARCH_ITEM_ID - @START_INDEX + 1
		WHERE SEARCH_ID = @NEW_SEARCH_ID
	*/

	-- STEP 4: DELETE ITEMS FROM TEMP TABLE
	DELETE FROM TB_CLASSIFIER_ITEM_TEMP WHERE REVIEW_ID = @REVIEW_ID	
		
SET NOCOUNT OFF
go


/* ******************************* these are relatively new. Sorry - not sure which ones you'll have already run for testing... *************** */

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierUpdateModel]    Script Date: 10/17/2016 2:04:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ClassifierUpdateModel]
(
	@MODEL_ID INT
,	@TITLE nvarchar(1000) = NULL
,	@ACCURACY DECIMAL(18,18) = NULL
,	@AUC DECIMAL(18,18) = NULL
,	@PRECISION DECIMAL(18,18) = NULL
,	@RECALL DECIMAL(18,18) = NULL
,	@CHECK_MODEL_ID_EXISTS INT OUTPUT
)

As

SET NOCOUNT ON

SELECT @CHECK_MODEL_ID_EXISTS = COUNT(*) FROM tb_CLASSIFIER_MODEL WHERE MODEL_ID = @MODEL_ID

IF (@CHECK_MODEL_ID_EXISTS = 1)
BEGIN

	update tb_CLASSIFIER_MODEL
		SET TIME_ENDED = CURRENT_TIMESTAMP,
		MODEL_TITLE = @TITLE,
		ACCURACY = @ACCURACY,
		AUC = @AUC,
		[PRECISION] = @PRECISION,
		RECALL = @RECALL
		
	WHERE MODEL_ID = @MODEL_ID
END

SET NOCOUNT OFF

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierGetClassificationData]    Script Date: 10/18/2016 8:40:25 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ClassifierGetClassificationData]
(
	@REVIEW_ID INT
,	@ATTRIBUTE_ID_CLASSIFY_TO BIGINT = NULL
,	@SOURCE_ID INT = NULL
)

As

SET NOCOUNT ON

	DELETE FROM TB_CLASSIFIER_ITEM_TEMP WHERE REVIEW_ID = @REVIEW_ID	

	IF @ATTRIBUTE_ID_CLASSIFY_TO > -1
	BEGIN
		SELECT DISTINCT '99' LABEL, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		WHERE REVIEW_ID = @REVIEW_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_CLASSIFY_TO AND IS_DELETED = 'FALSE'
	END
	ELSE
	IF @SOURCE_ID > -1
	BEGIN
		SELECT DISTINCT '99' LABEL, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, KEYWORDS FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID
		INNER JOIN TB_ITEM_SOURCE ITS ON ITS.ITEM_ID = TB_ITEM.ITEM_ID AND ITS.SOURCE_ID = @SOURCE_ID
		WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED = 'FALSE'
	END
	ELSE
	IF @SOURCE_ID = -1
	BEGIN
		SELECT DISTINCT '99' LABEL, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, KEYWORDS FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID and IR.REVIEW_ID = @REVIEW_ID AND IR.IS_DELETED = 'FALSE'
		LEFT OUTER JOIN TB_ITEM_SOURCE ITS ON ITS.ITEM_ID = TB_ITEM.ITEM_ID 
		LEFT OUTER JOIN TB_SOURCE TS on ITS.SOURCE_ID = TS.SOURCE_ID and IR.REVIEW_ID = TS.REVIEW_ID
		WHERE TS.SOURCE_ID  is null
	END
	ELSE
	BEGIN
		SELECT DISTINCT '99' LABEL, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, KEYWORDS FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID
		WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED = 'FALSE'
	END
		
SET NOCOUNT OFF

GO



--USE [Reviewer]
--GO
--DROP PROCEDURE dbo.st_ClassifierGetModel, dbo.st_TrainingItemAttributeBulkInsert, st_TrainingStatistics

--USE [Reviewer]
--GO
--/****** Object:  StoredProcedure [dbo].[st_TrainingWriteDataToAzure]    Script Date: 6/17/2016 7:23:13 PM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--ALTER procedure [dbo].[st_TrainingWriteDataToAzure]
--(
--	@REVIEW_ID INT,
--	@SCREENING_INDEXED BIT = 'FALSE'
----,	@SCREENING_DATA_FILE NVARCHAR(50)
--)

--As

--SET NOCOUNT ON

--	DELETE FROM TB_SCREENING_ML_TEMP WHERE REVIEW_ID = @REVIEW_ID

--	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '1' INCLUDED FROM TB_ITEM_ATTRIBUTE
--	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
--	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
--		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
--	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
--		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
--	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID
			
--	UNION ALL
	
--	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '0' INCLUDED FROM TB_ITEM_ATTRIBUTE
--	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
--	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
--		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
--	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
--		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
--	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

--	UNION ALL
	
--	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_REVIEW.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '99' INCLUDED FROM TB_ITEM_REVIEW
--	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
--		where TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND not TB_ITEM_REVIEW.ITEM_ID in
--		(
--			SELECT ITEM_ID FROM TB_ITEM_SET
--				INNER JOIN TB_REVIEW ON TB_REVIEW.SCREENING_CODE_SET_ID = TB_ITEM_SET.SET_ID
--				WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_REVIEW.REVIEW_ID = @REVIEW_ID
--		)

--	UPDATE TB_REVIEW
--		SET SCREENING_INDEXED = @SCREENING_INDEXED,
--			SCREENING_MODEL_RUNNING = @SCREENING_INDEXED
--		WHERE REVIEW_ID = @REVIEW_ID

--SET NOCOUNT OFF

--go

--USE [Reviewer]
--GO
--/****** Object:  StoredProcedure [dbo].[st_TrainingScreeningCriteriaDeleteAll]    Script Date: 6/23/2016 10:13:24 AM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--create procedure [dbo].[st_TrainingScreeningCriteriaDeleteAll]
--(
--	@REVIEW_ID INT
--)

--As

--SET NOCOUNT ON

--	DELETE FROM TB_TRAINING_SCREENING_CRITERIA WHERE REVIEW_ID = @REVIEW_ID

--SET NOCOUNT OFF


----
--/*************** REALLY VERY NEW DATABASE CHANGES (21/04/2016) ******************************* */

----USE [Reviewer]
----GO

----/****** Object:  Table [dbo].[TB_CLASSIFIER_ITEM_TEMP]    Script Date: 04/22/2016 09:49:57 ******/
----SET ANSI_NULLS ON
----GO

----SET QUOTED_IDENTIFIER ON
----GO

----CREATE TABLE [dbo].[TB_CLASSIFIER_ITEM_TEMP](
----	[REVIEW_ID] [int] NOT NULL,
----	[ITEM_ID] [bigint] NOT NULL,
----	[SCORE] [decimal](18, 18) NULL,
---- CONSTRAINT [PK_TB_CLASSIFIER_ITEM_TEMP] PRIMARY KEY CLUSTERED 
----(
----	[REVIEW_ID] ASC,
----	[ITEM_ID] ASC
----)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
----) ON [PRIMARY]

----GO


----/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
----BEGIN TRANSACTION
----SET QUOTED_IDENTIFIER ON
----SET ARITHABORT ON
----SET NUMERIC_ROUNDABORT OFF
----SET CONCAT_NULL_YIELDS_NULL ON
----SET ANSI_NULLS ON
----SET ANSI_PADDING ON
----SET ANSI_WARNINGS ON
----COMMIT
----BEGIN TRANSACTION
----GO
----ALTER TABLE dbo.tb_CLASSIFIER_MODEL ADD
----	ACCURACY decimal(18, 18) NULL,
----	AUC decimal(18, 18) NULL,
----	PRECISION decimal(18, 18) NULL,
----	RECALL decimal(18, 18) NULL
----GO
----ALTER TABLE dbo.tb_CLASSIFIER_MODEL
----	DROP COLUMN TP, FP, TN, FN
----GO
----ALTER TABLE dbo.tb_CLASSIFIER_MODEL SET (LOCK_ESCALATION = TABLE)
----GO
----COMMIT
----GO

----/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
----BEGIN TRANSACTION
----SET QUOTED_IDENTIFIER ON
----SET ARITHABORT ON
----SET NUMERIC_ROUNDABORT OFF
----SET CONCAT_NULL_YIELDS_NULL ON
----SET ANSI_NULLS ON
----SET ANSI_PADDING ON
----SET ANSI_WARNINGS ON
----COMMIT
----BEGIN TRANSACTION
----GO
----ALTER TABLE dbo.TB_CONTACT SET (LOCK_ESCALATION = TABLE)
----GO
----COMMIT
----BEGIN TRANSACTION
----GO
----ALTER TABLE dbo.TB_REVIEW SET (LOCK_ESCALATION = TABLE)
----GO
----COMMIT
----BEGIN TRANSACTION
----GO
----CREATE TABLE dbo.Tmp_tb_CLASSIFIER_MODEL
----	(
----	MODEL_ID int NOT NULL IDENTITY (1, 1),
----	MODEL_TITLE nvarchar(1000) NULL,
----	CONTACT_ID int NOT NULL,
----	REVIEW_ID int NOT NULL,
----	ATTRIBUTE_ID_ON bigint NULL,
----	ATTRIBUTE_ID_NOT_ON bigint NULL,
----	MODEL_INFO varbinary(MAX) NULL,
----	ACCURACY decimal(18, 18) NULL,
----	AUC decimal(18, 18) NULL,
----	PRECISION decimal(18, 18) NULL,
----	RECALL decimal(18, 18) NULL
----	)  ON [PRIMARY]
----	 TEXTIMAGE_ON [PRIMARY]
----GO
----ALTER TABLE dbo.Tmp_tb_CLASSIFIER_MODEL SET (LOCK_ESCALATION = TABLE)
----GO
----SET IDENTITY_INSERT dbo.Tmp_tb_CLASSIFIER_MODEL ON
----GO
----IF EXISTS(SELECT * FROM dbo.tb_CLASSIFIER_MODEL)
----	 EXEC('INSERT INTO dbo.Tmp_tb_CLASSIFIER_MODEL (MODEL_ID, MODEL_TITLE, CONTACT_ID, REVIEW_ID, ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, MODEL_INFO)
----		SELECT MODEL_ID, MODEL_TITLE, CONTACT_ID, REVIEW_ID, ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, MODEL_INFO FROM dbo.tb_CLASSIFIER_MODEL WITH (HOLDLOCK TABLOCKX)')
----GO
----SET IDENTITY_INSERT dbo.Tmp_tb_CLASSIFIER_MODEL OFF
----GO
----DROP TABLE dbo.tb_CLASSIFIER_MODEL
----GO
----EXECUTE sp_rename N'dbo.Tmp_tb_CLASSIFIER_MODEL', N'tb_CLASSIFIER_MODEL', 'OBJECT' 
----GO
----ALTER TABLE dbo.tb_CLASSIFIER_MODEL ADD CONSTRAINT
----	PK_tb_CLASSIFIER_MODEL PRIMARY KEY CLUSTERED 
----	(
----	MODEL_ID
----	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

----GO
----ALTER TABLE dbo.tb_CLASSIFIER_MODEL ADD CONSTRAINT
----	FK_tb_CLASSIFIER_MODEL_TB_REVIEW FOREIGN KEY
----	(
----	REVIEW_ID
----	) REFERENCES dbo.TB_REVIEW
----	(
----	REVIEW_ID
----	) ON UPDATE  NO ACTION 
----	 ON DELETE  NO ACTION 
	
----GO
----ALTER TABLE dbo.tb_CLASSIFIER_MODEL ADD CONSTRAINT
----	FK_tb_CLASSIFIER_MODEL_TB_CONTACT FOREIGN KEY
----	(
----	CONTACT_ID
----	) REFERENCES dbo.TB_CONTACT
----	(
----	CONTACT_ID
----	) ON UPDATE  NO ACTION 
----	 ON DELETE  NO ACTION 
	
----GO
----COMMIT

----GO

----/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
----BEGIN TRANSACTION
----SET QUOTED_IDENTIFIER ON
----SET ARITHABORT ON
----SET NUMERIC_ROUNDABORT OFF
----SET CONCAT_NULL_YIELDS_NULL ON
----SET ANSI_NULLS ON
----SET ANSI_PADDING ON
----SET ANSI_WARNINGS ON
----COMMIT
----BEGIN TRANSACTION
----GO
----ALTER TABLE dbo.tb_CLASSIFIER_MODEL ADD
----	TIME_STARTED datetime NULL,
----	TIME_ENDED datetime NULL
----GO
----ALTER TABLE dbo.tb_CLASSIFIER_MODEL
----	DROP COLUMN MODEL_INFO
----GO
----ALTER TABLE dbo.tb_CLASSIFIER_MODEL SET (LOCK_ESCALATION = TABLE)
----GO
----COMMIT
----GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ClassifierGetClassificationData]    Script Date: 04/21/2016 21:35:55 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_ClassifierGetClassificationData]
----(
----	@REVIEW_ID INT
----,	@ATTRIBUTE_ID_CLASSIFY_TO BIGINT = NULL
----)

----As

----SET NOCOUNT ON

----	DELETE FROM TB_CLASSIFIER_ITEM_TEMP WHERE REVIEW_ID = @REVIEW_ID	

----	SELECT DISTINCT '99' LABEL, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS FROM TB_ITEM_ATTRIBUTE
----	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----	WHERE REVIEW_ID = @REVIEW_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_CLASSIFY_TO AND IS_DELETED = 'FALSE'
	
		
----SET NOCOUNT OFF

----go

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ClassifierSaveModel]    Script Date: 04/21/2016 21:25:04 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_ClassifierSaveModel]
----(
----	@REVIEW_ID INT
----,	@ATTRIBUTE_ID_ON BIGINT = NULL
----,	@ATTRIBUTE_ID_NOT_ON BIGINT = NULL
----,	@CONTACT_ID INT
----,	@MODEL_TITLE NVARCHAR(1000) = NULL
----,	@NEW_MODEL_ID INT OUTPUT
----)

----As

----SET NOCOUNT ON

----	DECLARE @START_TIME DATETIME
----	DECLARE @END_TIME DATETIME

----	SELECT @START_TIME = MAX(TIME_STARTED) FROM tb_CLASSIFIER_MODEL
----		WHERE REVIEW_ID = @REVIEW_ID

----	SELECT @END_TIME = MAX(TIME_ENDED) FROM tb_CLASSIFIER_MODEL
----		WHERE REVIEW_ID = @REVIEW_ID
		
----	IF (@START_TIME IS NULL) OR (@START_TIME != @END_TIME) OR
----	(
----		(@START_TIME = @END_TIME) AND CURRENT_TIMESTAMP > DATEADD(HOUR, 2, @START_TIME) -- i.e. one review can only run one classification task at a time
----	)
----	BEGIN
----		INSERT INTO tb_CLASSIFIER_MODEL(MODEL_TITLE, CONTACT_ID, REVIEW_ID, ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, TIME_STARTED, TIME_ENDED)
----	VALUES(@MODEL_TITLE, @CONTACT_ID, @REVIEW_ID, @ATTRIBUTE_ID_ON, @ATTRIBUTE_ID_NOT_ON, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
		
----	SET @NEW_MODEL_ID = @@IDENTITY
----	END
----	ELSE
----	BEGIN
----		SET @NEW_MODEL_ID = 0
----	END

	

----SET NOCOUNT OFF

----go

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ClassifierModels]    Script Date: 04/21/2016 15:56:49 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_ClassifierModels]
----(
----	@REVIEW_ID INT
----)

----As

----SET NOCOUNT ON

----	select MODEL_ID, MODEL_TITLE, REVIEW_ID, A1.ATTRIBUTE_NAME ATTRIBUTE_ON, A2.ATTRIBUTE_NAME ATTRIBUTE_NOT_ON,
----		CONTACT_NAME, tb_CLASSIFIER_MODEL.CONTACT_ID, ACCURACY, AUC, [PRECISION], RECALL from tb_CLASSIFIER_MODEL
----	INNER JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_ON
----	INNER JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_NOT_ON
----	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = tb_CLASSIFIER_MODEL.CONTACT_ID
	
----	where REVIEW_ID = @REVIEW_ID
----	order by MODEL_ID

----SET NOCOUNT OFF

----go

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ClassifierUpdateModel]    Script Date: 04/21/2016 14:57:35 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_ClassifierUpdateModel]
----(
----	@MODEL_ID INT
----,	@ACCURACY DECIMAL(18,18) = NULL
----,	@AUC DECIMAL(18,18) = NULL
----,	@PRECISION DECIMAL(18,18) = NULL
----,	@RECALL DECIMAL(18,18) = NULL
----)

----As

----SET NOCOUNT ON
----	update tb_CLASSIFIER_MODEL
----		SET TIME_ENDED = CURRENT_TIMESTAMP,
----		ACCURACY = @ACCURACY,
----		AUC = @AUC,
----		[PRECISION] = @PRECISION,
----		RECALL = @RECALL
		
----	WHERE MODEL_ID = @MODEL_ID

----SET NOCOUNT OFF

----go

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ClassifierGetTrainingData]    Script Date: 04/21/2016 10:06:51 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_ClassifierGetTrainingData]
----(
----	@REVIEW_ID INT
----,	@ATTRIBUTE_ID_ON BIGINT = NULL
----,	@ATTRIBUTE_ID_NOT_ON BIGINT = NULL
----)

----As

----SET NOCOUNT ON

----	SELECT DISTINCT '1' LABEL, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS FROM TB_ITEM_ATTRIBUTE
----	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----	WHERE REVIEW_ID = @REVIEW_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_ON AND IS_DELETED = 'FALSE'
	
----	UNION ALL
	
----	SELECT DISTINCT '0' LABEL, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS FROM TB_ITEM_ATTRIBUTE
----	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----	WHERE REVIEW_ID = @REVIEW_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_NOT_ON AND IS_DELETED = 'FALSE'



----SET NOCOUNT OFF

----go

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ClassifierCreateSearchList]    Script Date: 04/22/2016 09:52:37 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----CREATE procedure [dbo].[st_ClassifierCreateSearchList]
----(
----	@REVIEW_ID INT
----,	@CONTACT_ID INT
----,	@SEARCH_TITLE NVARCHAR(4000)
----,	@SEARCH_DESC varchar(4000) = null
----,	@HITS_NO INT
----,	@NEW_SEARCH_ID INT OUTPUT
----)

----As

----SET NOCOUNT ON

----	-- STEP 1: GET THE SEARCH NUMBER FOR THIS REVIEW
----	DECLARE @SEARCH_NO INT
----	SELECT @SEARCH_NO = ISNULL(MAX(SEARCH_NO), 0) + 1 FROM tb_SEARCH WHERE REVIEW_ID = @REVIEW_ID

----	-- STEP 2: CREATE THE SEARCH RECORD
----	INSERT INTO tb_SEARCH
----	(	REVIEW_ID
----	,	CONTACT_ID
----	,	SEARCH_TITLE
----	,	SEARCH_NO
----	,	HITS_NO
----	,	SEARCH_DATE
----	)	
----	VALUES
----	(
----		@REVIEW_ID
----	,	@CONTACT_ID
----	,	@SEARCH_TITLE
----	,	@SEARCH_NO
----	,	@HITS_NO
----	,	GetDate()
----	)
----	-- Get the identity and return it
----	SET @NEW_SEARCH_ID = @@identity
	
----	-- STEP 3: PUT THE ITEMS INTO THE SEARCH LIST
	
----	INSERT INTO TB_SEARCH_ITEM(ITEM_ID,SEARCH_ID, ITEM_RANK)
----		SELECT CIT.ITEM_ID, @NEW_SEARCH_ID,  0
----				FROM TB_CLASSIFIER_ITEM_TEMP CIT
----			ORDER BY CIT.SCORE DESC
	
----	-- STEP 3: PUT THE ITEM_RANK VALUES IN (I.E. SCORES FROM 1 TO N)
----	DECLARE @START_INDEX INT = 0
----	SELECT @START_INDEX = MIN(SEARCH_ITEM_ID) FROM TB_SEARCH_ITEM WHERE SEARCH_ID = @NEW_SEARCH_ID
----	UPDATE TB_SEARCH_ITEM
----		SET ITEM_RANK = SEARCH_ITEM_ID - @START_INDEX + 1
----		WHERE SEARCH_ID = @NEW_SEARCH_ID
	
----	-- STEP 4: DELETE ITEMS FROM TEMP TABLE
----	DELETE FROM TB_CLASSIFIER_ITEM_TEMP WHERE REVIEW_ID = @REVIEW_ID	
		
----SET NOCOUNT OFF




----/************************************************************** NEW DATABASE CHANGES ***************************************************************** */

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_TrainingWriteDataToAzure]    Script Date: 13/04/2016 07:35:56 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_TrainingWriteDataToAzure]
----(
----	@REVIEW_ID INT
------,	@SCREENING_DATA_FILE NVARCHAR(50)
----)

----As

----SET NOCOUNT ON

----	DELETE FROM TB_SCREENING_ML_TEMP WHERE REVIEW_ID = @REVIEW_ID

----	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '1' INCLUDED FROM TB_ITEM_ATTRIBUTE
----	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
----		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
----	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID
			
----	UNION ALL
	
----	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '0' INCLUDED FROM TB_ITEM_ATTRIBUTE
----	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
----		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
----	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

----	UNION ALL
	
----	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_REVIEW.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '99' INCLUDED FROM TB_ITEM_REVIEW
----	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
----		where TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND not TB_ITEM_REVIEW.ITEM_ID in
----		(
----			SELECT ITEM_ID FROM TB_ITEM_SET
----				INNER JOIN TB_REVIEW ON TB_REVIEW.SCREENING_CODE_SET_ID = TB_ITEM_SET.SET_ID
----				WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_REVIEW.REVIEW_ID = @REVIEW_ID
----		)

----	UPDATE TB_REVIEW
----		SET SCREENING_INDEXED = 'TRUE',
----			SCREENING_MODEL_RUNNING = 'TRUE'
----		WHERE REVIEW_ID = @REVIEW_ID

----SET NOCOUNT OFF

----GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ReviewInfoUpdate]    Script Date: 13/04/2016 07:10:55 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_ReviewInfoUpdate]
----(
----	@REVIEW_ID int
----,	@SCREENING_CODE_SET_ID int
----,	@SCREENING_MODE nvarchar(10)
----,	@SCREENING_RECONCILLIATION nvarchar(10)
----,	@SCREENING_WHAT_ATTRIBUTE_ID bigint
----,	@SCREENING_N_PEOPLE int
----,	@SCREENING_AUTO_EXCLUDE bit
----,	@SCREENING_MODEL_RUNNING bit
----,	@SCREENING_INDEXED bit
----)

----As

----SET NOCOUNT ON

----UPDATE TB_REVIEW
----	SET SCREENING_CODE_SET_ID = @SCREENING_CODE_SET_ID
----,		SCREENING_MODE = @SCREENING_MODE
----,		SCREENING_RECONCILLIATION = @SCREENING_RECONCILLIATION
----,		SCREENING_WHAT_ATTRIBUTE_ID = @SCREENING_WHAT_ATTRIBUTE_ID
----,		SCREENING_N_PEOPLE = @SCREENING_N_PEOPLE
----,		SCREENING_AUTO_EXCLUDE = @SCREENING_AUTO_EXCLUDE
------,		SCREENING_MODEL_RUNNING = @SCREENING_MODEL_RUNNING
----,		SCREENING_INDEXED = @SCREENING_INDEXED
----WHERE REVIEW_ID = @REVIEW_ID
	

----SET NOCOUNT OFF

----GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ScreeningCreateMLList]    Script Date: 13/04/2016 07:36:36 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_ScreeningCreateMLList]
----(
----	@REVIEW_ID INT,
----	@CONTACT_ID INT,
----	@WHAT_ATTRIBUTE_ID BIGINT,
----	@SCREENING_MODE nvarchar(10),
----	@CODE_SET_ID INT,
----	@TRAINING_ID INT
----)

----As

----SET NOCOUNT ON

----	DECLARE @TP INT
----	DECLARE @TN INT

----	-- ***** FIRST, GET THE STATS IN TERMS OF # ITEMS SCREENED TO POPULATE THE TRIANING TABLE (GIVES US THE GRAPH ON THE SCREENING TAB)
----	SELECT @TP = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
----		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 10
----		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

----	SELECT @TN = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
----		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 11
----		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

----	-- ********** SECOND, ENTER THE LIST OF ITEMS INTO TB_TRAINING_ITEM ACCORDING TO WHETHER WE'RE FILTERING BY AN ATTRIBUTE OR DOING THE WHOLE REVIEW

----	IF @WHAT_ATTRIBUTE_ID > 0  -- i.e. we're filtering by a code
----	BEGIN
----		INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK], SCORE)
----			SELECT @TRAINING_ID, AZ.ITEM_ID, 0, 0, AZ.SCORE
----				FROM TB_SCREENING_ML_TEMP AZ
----			INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = AZ.ITEM_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @WHAT_ATTRIBUTE_ID
----			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----			WHERE NOT AZ.ITEM_ID IN
----				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID) AND AZ.REVIEW_ID = @REVIEW_ID
----			ORDER BY AZ.SCORE DESC
			
----	END
----	ELSE -- NOT FILTERING BY A CODE, SO EVERYTHING IN THE REVIEW THAT'S INCLUDED AND SO FAR UNCODED IS INCLUDED
----	BEGIN
----		INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK], SCORE)
----		SELECT @TRAINING_ID, AZ.ITEM_ID, 0, 0, AZ.SCORE
----				FROM TB_SCREENING_ML_TEMP AZ
----			WHERE NOT AZ.ITEM_ID IN
----				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID) AND AZ.REVIEW_ID = @REVIEW_ID
----			ORDER BY AZ.SCORE DESC
----	END

----	/* SET THE RANKS TO INCREMENT */
----	DECLARE @START_INDEX INT = 0
----	SELECT @START_INDEX = MIN(TRAINING_ITEM_ID) FROM TB_TRAINING_ITEM WHERE TRAINING_ID = @TRAINING_ID
----	UPDATE TB_TRAINING_ITEM
----		SET [RANK] = TRAINING_ITEM_ID - @START_INDEX + 1
----		WHERE TRAINING_ID = @TRAINING_ID


----	-- FINALLY, MIGRATE ANY NON-STALE CODING LOCKS FROM THE PREVIOUS TRAINING RUN
----	DECLARE @LAST_TRAINING_ID INT

----	SELECT @LAST_TRAINING_ID = TRAINING_ID FROM TB_TRAINING
----		WHERE REVIEW_ID = @REVIEW_ID AND TRAINING_ID < (SELECT MAX(TRAINING_ID) FROM TB_TRAINING WHERE REVIEW_ID = @REVIEW_ID)

----	DECLARE @CURRENT_ITERATION INT
	
----	SELECT @CURRENT_ITERATION = MAX(ITERATION) + 1 FROM TB_TRAINING
----		WHERE REVIEW_ID = @REVIEW_ID
		
----	IF (@CURRENT_ITERATION IS NULL)
----	BEGIN
----		SET @CURRENT_ITERATION = 1
----	END

----	UPDATE A
----		SET A.CONTACT_ID_CODING = B.CONTACT_ID_CODING,
----		A.WHEN_LOCKED = B.WHEN_LOCKED
----		FROM TB_TRAINING_ITEM A
----		JOIN
----		TB_TRAINING_ITEM B ON A.ITEM_ID = B.ITEM_ID AND CURRENT_TIMESTAMP < DATEADD(DAY, 7, B.WHEN_LOCKED) AND
----			B.TRAINING_ID = @LAST_TRAINING_ID
----		WHERE A.TRAINING_ID = @TRAINING_ID

----	UPDATE TB_TRAINING
----		SET TIME_ENDED = CURRENT_TIMESTAMP,
----		ITERATION = @CURRENT_ITERATION,
----		TRUE_POSITIVES = @TP,
----		TRUE_NEGATIVES = @TN
----		WHERE TB_TRAINING.TRAINING_ID = @TRAINING_ID

----	-- delete the old list(s) of items to screen for this review
----	DELETE TI
----	FROM TB_TRAINING_ITEM TI
----	INNER JOIN TB_TRAINING T ON T.TRAINING_ID = TI.TRAINING_ID
----	WHERE T.REVIEW_ID = @REVIEW_ID AND T.TRAINING_ID < @TRAINING_ID

----	UPDATE TB_REVIEW
----		SET SCREENING_INDEXED = 'TRUE',
----			SCREENING_MODEL_RUNNING = 'FALSE'
----		WHERE REVIEW_ID = @REVIEW_ID

----	DELETE FROM TB_SCREENING_ML_TEMP WHERE REVIEW_ID = @REVIEW_ID

----SET NOCOUNT OFF


----GO



----/**************** END NEW DATABASE CHANGES ***************************** */


----USE [Reviewer]
----GO
----ALTER TABLE dbo.TB_REVIEW ADD
----	[SCREENING_MODE] [nvarchar](10) NULL,
----	[SCREENING_WHAT_ATTRIBUTE_ID] [bigint] NULL,
----	[SCREENING_N_PEOPLE] [int] NULL,
----	[SCREENING_RECONCILLIATION] [nvarchar](10) NULL,
----	[SCREENING_AUTO_EXCLUDE] [bit] NULL,
----	[SCREENING_MODEL_RUNNING] [bit] NULL,
----	[SCREENING_INDEXED] [bit] NULL
----GO

----ALTER TABLE dbo.TB_TRAINING_ITEM ADD
----	[WHEN_LOCKED] [datetime] NULL,
----	SCORE DECIMAL(6,6)
----GO

----DROP PROCEDURE DBO.ST_TRAININGUPDATE
----GO

----/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
----BEGIN TRANSACTION
----SET QUOTED_IDENTIFIER ON
----SET ARITHABORT ON
----SET NUMERIC_ROUNDABORT OFF
----SET CONCAT_NULL_YIELDS_NULL ON
----SET ANSI_NULLS ON
----SET ANSI_PADDING ON
----SET ANSI_WARNINGS ON
----COMMIT
----BEGIN TRANSACTION
----GO
----ALTER TABLE dbo.TB_TRAINING SET (LOCK_ESCALATION = TABLE)
----GO
----COMMIT
----BEGIN TRANSACTION
----GO
----ALTER TABLE dbo.TB_TRAINING_ITEM ADD CONSTRAINT
----	FK_TB_TRAINING_ITEM_TB_TRAINING FOREIGN KEY
----	(
----	TRAINING_ID
----	) REFERENCES dbo.TB_TRAINING
----	(
----	TRAINING_ID
----	) ON UPDATE  NO ACTION 
----	 ON DELETE  NO ACTION 
	
----GO
----ALTER TABLE dbo.TB_TRAINING_ITEM SET (LOCK_ESCALATION = TABLE)
----GO
----COMMIT
----go

----USE [Reviewer]
----GO

----/****** Object:  Table [dbo].[TB_SCREENING_ML_TEMP]    Script Date: 31/03/2016 17:36:50 ******/
----SET ANSI_NULLS ON
----GO

----SET QUOTED_IDENTIFIER ON
----GO

----CREATE TABLE [dbo].[TB_SCREENING_ML_TEMP](
----	[SCORE] [decimal](10, 10) NULL,
----	[ITEM_ID] [bigint] NOT NULL,
----	[REVIEW_ID] [int] NOT NULL,
---- CONSTRAINT [PK_TB_SCREENING_ML_TEMP] PRIMARY KEY CLUSTERED 
----(
----	[ITEM_ID] ASC,
----	[REVIEW_ID] ASC
----)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
----) ON [PRIMARY]

----GO


----/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
----BEGIN TRANSACTION
----SET QUOTED_IDENTIFIER ON
----SET ARITHABORT ON
----SET NUMERIC_ROUNDABORT OFF
----SET CONCAT_NULL_YIELDS_NULL ON
----SET ANSI_NULLS ON
----SET ANSI_PADDING ON
----SET ANSI_WARNINGS ON
----COMMIT
----BEGIN TRANSACTION
----GO
----ALTER TABLE dbo.TB_TRAINING_ITEM
----	DROP CONSTRAINT FK_TB_TRAINING_ITEM_TB_TRAINING
----GO
----ALTER TABLE dbo.TB_TRAINING SET (LOCK_ESCALATION = TABLE)
----GO
----COMMIT
----BEGIN TRANSACTION
----GO
----CREATE TABLE dbo.Tmp_TB_TRAINING_ITEM
----	(
----	TRAINING_ITEM_ID int NOT NULL IDENTITY (1, 1),
----	ITEM_ID bigint NULL,
----	RANK int NULL,
----	TRAINING_ID int NULL,
----	CONTACT_ID_CODING int NULL,
----	WHEN_LOCKED datetime NULL,
----	SCORE decimal(10, 10) NULL
----	)  ON [PRIMARY]
----GO
----ALTER TABLE dbo.Tmp_TB_TRAINING_ITEM SET (LOCK_ESCALATION = TABLE)
----GO
----SET IDENTITY_INSERT dbo.Tmp_TB_TRAINING_ITEM ON
----GO
----IF EXISTS(SELECT * FROM dbo.TB_TRAINING_ITEM)
----	 EXEC('INSERT INTO dbo.Tmp_TB_TRAINING_ITEM (TRAINING_ITEM_ID, ITEM_ID, RANK, TRAINING_ID, CONTACT_ID_CODING, WHEN_LOCKED, SCORE)
----		SELECT TRAINING_ITEM_ID, ITEM_ID, RANK, TRAINING_ID, CONTACT_ID_CODING, WHEN_LOCKED, SCORE FROM dbo.TB_TRAINING_ITEM WITH (HOLDLOCK TABLOCKX)')
----GO
----SET IDENTITY_INSERT dbo.Tmp_TB_TRAINING_ITEM OFF
----GO
----DROP TABLE dbo.TB_TRAINING_ITEM
----GO
----EXECUTE sp_rename N'dbo.Tmp_TB_TRAINING_ITEM', N'TB_TRAINING_ITEM', 'OBJECT' 
----GO
----ALTER TABLE dbo.TB_TRAINING_ITEM ADD CONSTRAINT
----	PK_TB_TRAINING_ITEM PRIMARY KEY CLUSTERED 
----	(
----	TRAINING_ITEM_ID
----	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

----GO
----CREATE NONCLUSTERED INDEX IX_TB_TRAINING_ITEM_TRAINING_ID_C_ID_CODING_TRAINING_ITEM_ID_RANK ON dbo.TB_TRAINING_ITEM
----	(
----	TRAINING_ID,
----	CONTACT_ID_CODING
----	) INCLUDE (TRAINING_ITEM_ID, RANK) 
---- WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
----GO
----ALTER TABLE dbo.TB_TRAINING_ITEM ADD CONSTRAINT
----	FK_TB_TRAINING_ITEM_TB_TRAINING FOREIGN KEY
----	(
----	TRAINING_ID
----	) REFERENCES dbo.TB_TRAINING
----	(
----	TRAINING_ID
----	) ON UPDATE  NO ACTION 
----	 ON DELETE  NO ACTION 
	
----GO
----COMMIT
----go

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ReviewContact]    Script Date: 12/03/2016 19:45:32 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_ReviewContact]
----(
----	@CONTACT_ID INT
----)

----As

----SELECT REVIEW_CONTACT_ID, rc.REVIEW_ID, rc.CONTACT_ID, REVIEW_NAME, dbo.fn_REBUILD_ROLES(rc.REVIEW_CONTACT_ID) as ROLES
----	,own.CONTACT_NAME as 'OWNER', case when LR is null
----									then r.DATE_CREATED
----									else LR
----								 end
----								 as 'LAST_ACCESS'
----	, r.SHOW_SCREENING, r.SCREENING_CODE_SET_ID, r.SCREENING_MODE, r.SCREENING_WHAT_ATTRIBUTE_ID, r.SCREENING_N_PEOPLE
----	, r.SCREENING_RECONCILLIATION, r.SCREENING_AUTO_EXCLUDE, SCREENING_MODEL_RUNNING, SCREENING_INDEXED
----	, BL_ACCOUNT_CODE,BL_AUTH_CODE, BL_TX, BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX
----FROM TB_REVIEW_CONTACT rc
----INNER JOIN TB_REVIEW r ON rc.REVIEW_ID = r.REVIEW_ID
----inner join TB_CONTACT own on r.FUNDER_ID = own.CONTACT_ID
----left join (
----			select MAX(LAST_RENEWED) LR, REVIEW_ID
----			from ReviewerAdmin.dbo.TB_LOGON_TICKET  
----			where @CONTACT_ID = CONTACT_ID
----			group by REVIEW_ID
----			) as t
----			on t.REVIEW_ID = r.REVIEW_ID
----WHERE rc.CONTACT_ID = @CONTACT_ID and (r.ARCHIE_ID is null OR r.ARCHIE_ID = 'prospective_______')
----ORDER BY REVIEW_NAME

----GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ReviewContactForSiteAdmin]    Script Date: 12/03/2016 19:46:12 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO

----ALTER procedure [dbo].[st_ReviewContactForSiteAdmin]
----(
----	@CONTACT_ID INT
----	,@REVIEW_ID int
----)

----As

----SELECT 0 as REVIEW_CONTACT_ID, - r.REVIEW_ID as REVIEW_ID, rc.CONTACT_ID, REVIEW_NAME, 'AdminUser;' as ROLES
----	,own.CONTACT_NAME as 'OWNER', case when LR is null
----									then r.DATE_CREATED
----									else LR
----								 end
----								 as 'LAST_ACCESS'
----	, r.SHOW_SCREENING, r.SCREENING_CODE_SET_ID, r.SCREENING_MODE, r.SCREENING_WHAT_ATTRIBUTE_ID, r.SCREENING_N_PEOPLE
----	, r.SCREENING_RECONCILLIATION, r.SCREENING_AUTO_EXCLUDE, SCREENING_MODEL_RUNNING, SCREENING_INDEXED
----	, BL_ACCOUNT_CODE,BL_AUTH_CODE, BL_TX, BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX
----FROM TB_CONTACT rc
----INNER JOIN TB_REVIEW r ON rc.CONTACT_ID = @CONTACT_ID and rc.IS_SITE_ADMIN = 1 and r.REVIEW_ID = @REVIEW_ID 
----inner join TB_CONTACT own on r.FUNDER_ID = own.CONTACT_ID
----left join (
----			select MAX(LAST_RENEWED) LR, REVIEW_ID
----			from ReviewerAdmin.dbo.TB_LOGON_TICKET  
----			where @CONTACT_ID = CONTACT_ID and REVIEW_ID = @REVIEW_ID
----			group by REVIEW_ID
----			) as t
----			on t.REVIEW_ID = r.REVIEW_ID
----WHERE rc.CONTACT_ID = @CONTACT_ID
----ORDER BY REVIEW_NAME

----GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ReviewInfo]    Script Date: 12/03/2016 22:44:25 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----CREATE procedure [dbo].[st_ReviewInfo]
----(
----	@REVIEW_ID INT
	
----)

----As
----BEGIN
----	SELECT * FROM TB_REVIEW
----	WHERE REVIEW_ID = @REVIEW_ID
----END

----GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ReviewInfoUpdate]    Script Date: 03/13/2016 11:48:26 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----CREATE procedure [dbo].[st_ReviewInfoUpdate]
----(
----	@REVIEW_ID int
----,	@SCREENING_CODE_SET_ID int
----,	@SCREENING_MODE nvarchar(10)
----,	@SCREENING_RECONCILLIATION nvarchar(10)
----,	@SCREENING_WHAT_ATTRIBUTE_ID bigint
----,	@SCREENING_N_PEOPLE int
----,	@SCREENING_AUTO_EXCLUDE bit
----,	@SCREENING_MODEL_RUNNING bit
----,	@SCREENING_INDEXED bit
----)

----As

----SET NOCOUNT ON

----UPDATE TB_REVIEW
----	SET SCREENING_CODE_SET_ID = @SCREENING_CODE_SET_ID
----,		SCREENING_MODE = @SCREENING_MODE
----,		SCREENING_RECONCILLIATION = @SCREENING_RECONCILLIATION
----,		SCREENING_WHAT_ATTRIBUTE_ID = @SCREENING_WHAT_ATTRIBUTE_ID
----,		SCREENING_N_PEOPLE = @SCREENING_N_PEOPLE
----,		SCREENING_AUTO_EXCLUDE = @SCREENING_AUTO_EXCLUDE
------,		SCREENING_MODEL_RUNNING = @SCREENING_MODEL_RUNNING
------,		SCREENING_INDEXED = @SCREENING_INDEXED
----WHERE REVIEW_ID = @REVIEW_ID
	

----SET NOCOUNT OFF

----go

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ScreeningCreateNonMLList]    Script Date: 12/04/2016 10:19:12 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_ScreeningCreateNonMLList]
----(
----	@REVIEW_ID INT,
----	@CONTACT_ID INT,
----	@WHAT_ATTRIBUTE_ID BIGINT,
----	@SCREENING_MODE nvarchar(10),
----	@CODE_SET_ID INT
----)

----As

----SET NOCOUNT ON

----	DECLARE @NEW_TRAINING_ID INT
----	DECLARE @TP INT
----	DECLARE @TN INT

----	-- ***** FIRST, GET THE STATS IN TERMS OF # ITEMS SCREENED TO POPULATE THE TRIANING TABLE (GIVES US THE GRAPH ON THE SCREENING TAB)
----	SELECT @TP = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
----		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 10
----		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

----	SELECT @TN = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
----		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 11
----		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID


----	-- ******** SECOND, ENTER A NEW LINE IN THE TRAINING TABLE ***************
----	INSERT INTO TB_TRAINING(REVIEW_ID, CONTACT_ID, TIME_STARTED, TIME_ENDED, TRUE_POSITIVES, TRUE_NEGATIVES)
----		VALUES (@REVIEW_ID, @CONTACT_ID, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, @TP, @TN)
	   
----	SET @NEW_TRAINING_ID = @@IDENTITY

----	-- ********** THIRD, ENTER THE LIST OF ITEMS INTO TB_TRAINING_ITEM ACCORDING TO WHETHER WE'RE FILTERING BY AN ATTRIBUTE OR DOING THE WHOLE REVIEW
----	IF @WHAT_ATTRIBUTE_ID > 0  -- i.e. we're filtering by a code
----	BEGIN
----		IF @SCREENING_MODE = 'Random' -- FILTERING BY A CODE AND ORDERING AT RANDOM
----		BEGIN
----			INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK])
----			SELECT @NEW_TRAINING_ID, TB_ITEM_REVIEW.ITEM_ID, 0, 0 FROM TB_ITEM_REVIEW
----			INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @WHAT_ATTRIBUTE_ID
----			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----			WHERE REVIEW_ID = @REVIEW_ID AND IS_INCLUDED = 'TRUE' AND IS_DELETED = 'FALSE' AND NOT TB_ITEM_REVIEW.ITEM_ID IN
----				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID)
----			ORDER BY NEWID()
----		END
----		ELSE -- FILTERING BY A CODE, BUT ORDERING BY THE VALUE PUT IN THE ADDITIONAL_TEXT FIELD
----		BEGIN
----			INSERT INTO TB_TRAINING_ITEM([RANK], TRAINING_ID, ITEM_ID, CONTACT_ID_CODING)
----			SELECT CASE WHEN ISNUMERIC(TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT)=1 THEN CAST(TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT AS INT) ELSE 0 END,
----				@NEW_TRAINING_ID, TB_ITEM_REVIEW.ITEM_ID, 0
----			FROM TB_ITEM_REVIEW
----			INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @WHAT_ATTRIBUTE_ID
----			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----			WHERE REVIEW_ID = @REVIEW_ID AND IS_INCLUDED = 'TRUE' AND IS_DELETED = 'FALSE' AND NOT TB_ITEM_REVIEW.ITEM_ID IN
----				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID)
----			ORDER BY TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT
----		END
----	END
----	ELSE -- NOT FILTERING BY A CODE, SO EVERYTHING IN THE REVIEW THAT'S INCLUDED AND SO FAR UNCODED IS INCLUDED
----	BEGIN
----		INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK])
----		SELECT @NEW_TRAINING_ID, ITEM_ID, 0, 0 FROM TB_ITEM_REVIEW
----			WHERE REVIEW_ID = @REVIEW_ID AND IS_INCLUDED = 'TRUE' AND IS_DELETED = 'FALSE' AND NOT ITEM_ID IN
----				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID)
----			ORDER BY NEWID()
----	END

----	/* SET THE RANKS TO INCREMENT */
----	DECLARE @START_INDEX INT = 0
----	SELECT @START_INDEX = MIN(TRAINING_ITEM_ID) FROM TB_TRAINING_ITEM WHERE TRAINING_ID = @NEW_TRAINING_ID
----	UPDATE TB_TRAINING_ITEM
----		SET [RANK] = TRAINING_ITEM_ID - @START_INDEX + 1
----		WHERE TRAINING_ID = @NEW_TRAINING_ID


----	-- FINALLY, MIGRATE ANY NON-STALE CODING LOCKS FROM THE PREVIOUS TRAINING RUN
----	DECLARE @LAST_TRAINING_ID INT

----	SELECT @LAST_TRAINING_ID = TRAINING_ID FROM TB_TRAINING
----		WHERE REVIEW_ID = @REVIEW_ID AND TRAINING_ID < (SELECT MAX(TRAINING_ID) FROM TB_TRAINING WHERE REVIEW_ID = @REVIEW_ID)

----	DECLARE @CURRENT_ITERATION INT
	
----	SELECT @CURRENT_ITERATION = MAX(ITERATION) + 1 FROM TB_TRAINING
----		WHERE REVIEW_ID = @REVIEW_ID
		
----	IF (@CURRENT_ITERATION IS NULL)
----	BEGIN
----		SET @CURRENT_ITERATION = 1
----	END

----	UPDATE A
----		SET A.CONTACT_ID_CODING = B.CONTACT_ID_CODING,
----		A.WHEN_LOCKED = B.WHEN_LOCKED
----		FROM TB_TRAINING_ITEM A
----		JOIN
----		TB_TRAINING_ITEM B ON A.ITEM_ID = B.ITEM_ID AND CURRENT_TIMESTAMP < DATEADD(DAY, 7, B.WHEN_LOCKED) AND
----			B.TRAINING_ID = @LAST_TRAINING_ID
----		WHERE A.TRAINING_ID = @NEW_TRAINING_ID

----	UPDATE TB_TRAINING
----		SET TIME_ENDED = CURRENT_TIMESTAMP,
----		ITERATION = @CURRENT_ITERATION
----		WHERE TB_TRAINING.TRAINING_ID = @NEW_TRAINING_ID

----	-- delete the old list(s) of items to screen for this review
----	DELETE TI
----	FROM TB_TRAINING_ITEM TI
----	INNER JOIN TB_TRAINING T ON T.TRAINING_ID = TI.TRAINING_ID
----	WHERE T.REVIEW_ID = @REVIEW_ID AND T.TRAINING_ID < @NEW_TRAINING_ID

----	UPDATE TB_REVIEW
----		SET SCREENING_INDEXED = 'TRUE'
----		WHERE REVIEW_ID = @REVIEW_ID

----SET NOCOUNT OFF


----GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ItemAttributeAutoReconcile]    Script Date: 23/03/2016 09:56:40 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_ItemAttributeAutoReconcile]
----(
----	@ITEM_ID BIGINT,
----	@SET_ID INT,
----	@ATTRIBUTE_ID BIGINT,
----	@RECONCILLIATION_TYPE nvarchar(10),
----	@N_PEOPLE int,
----	@AUTO_EXCLUDE bit,
----	@CONTACT_ID int
----)

----As
----SET NOCOUNT ON

----DECLARE @COUNT_RECS INT = 0
----DECLARE @ITEM_SET_ID INT = 0

----IF (@RECONCILLIATION_TYPE = 'no compl')
----BEGIN
----	SET @N_PEOPLE = 99 --(i.e. we don't do anything - none of the rest is executed)
----END
----ELSE
----BEGIN

----	-- **************** STAGE 1: GATHER DATA ON WHETHER RULES FOR AUTO-RECONCILLIATION ARE MET ********************

----	IF (@RECONCILLIATION_TYPE = 'Single')
----	BEGIN
----		SET @COUNT_RECS = 99 -- i.e. we go through to automatic exclude check

----		SELECT TOP(1) @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET
----			WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID
----	END
	
----	IF (@RECONCILLIATION_TYPE = 'auto code') -- agreement at the code level
----	BEGIN
----		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE
----			WHERE ITEM_ID = @ITEM_ID AND ATTRIBUTE_ID = @ATTRIBUTE_ID
----		IF (@COUNT_RECS >= @N_PEOPLE)
----		BEGIN
----			SELECT TOP(1) @ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID FROM TB_ITEM_SET
----				INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID
----				WHERE TB_ITEM_SET.ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND CONTACT_ID = @CONTACT_ID
----		END
----		ELSE
----		BEGIN
----			SET @ITEM_SET_ID = 0
----		END
----	END

----	-- one person has to tick 'include' for it to be included.-- N people agreeing on exclude if nobody has ticked 'include' before this threshold is met
----	IF (@RECONCILLIATION_TYPE = 'auto safet') 
----	BEGIN
----		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
----			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 -- is anything included?

----		SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
----			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 AND CONTACT_ID = @CONTACT_ID
----		IF (@COUNT_RECS > 0)
----		BEGIN
----			SET @COUNT_RECS = @N_PEOPLE
----		END
----		ELSE
----		BEGIN
----			-- IF NO INCLUDE IS TICKED, HAVE N PEOPLE TICKED EXCLUDE? IF SO, WE DEFAULT TO THIS
----			SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
----				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 -- EXCLUDED
----			SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
----				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11  AND CONTACT_ID = @CONTACT_ID
----		END
----		IF (@COUNT_RECS < @N_PEOPLE)
----		BEGIN
----			SET @ITEM_SET_ID = 0
----		END
----	END
	
----	 -- agreement at the include / exclude level
----	IF (@RECONCILLIATION_TYPE = 'auto excl')
----	BEGIN
----		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
----			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 -- INCLUDED
----		SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
----			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10  AND CONTACT_ID = @CONTACT_ID

----		IF (@COUNT_RECS < @N_PEOPLE)
----		BEGIN
----			SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
----				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 -- EXCLUDED
----			SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
----				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11  AND CONTACT_ID = @CONTACT_ID
----		END
----	END

	

----	-- *************************** STAGE 2: AUTO-RECONCILE AND AUTO-COMPLETE ***************************

----	IF (@COUNT_RECS >= @N_PEOPLE) AND (@RECONCILLIATION_TYPE != 'Single') -- AUTO-RECONCILE (COMPLETE) WHERE RULES MET
----	BEGIN
----		DECLARE @CHECK_NONE_COMPLETED INT = 0
----		SELECT COUNT(*) FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND IS_COMPLETED = 'TRUE'

----		IF (@CHECK_NONE_COMPLETED = 0)
----		BEGIN
----			UPDATE TB_ITEM_SET
----				SET IS_COMPLETED = 'TRUE'
----				WHERE ITEM_SET_ID = @ITEM_SET_ID
----		END
----	END
----	ELSE -- RULES FOR AUTO-COMPLETING ARE NOT MET, SO WE REMOVE THE SCREENING LOCK SO SOMEONE ELSE CAN SCREEN THIS ITEM
----	BEGIN
----		UPDATE TB_TRAINING_ITEM
----			SET CONTACT_ID_CODING = 0, WHEN_LOCKED = NULL
----			WHERE CONTACT_ID_CODING = @CONTACT_ID AND ITEM_ID = @ITEM_ID
----	END
----	IF (@AUTO_EXCLUDE = 'TRUE' AND @ITEM_SET_ID > 0 and @COUNT_RECS >= @N_PEOPLE) -- AUTO EXCLUDE WHERE RULES MET
----	BEGIN
----		-- SECOND, AUTO INCLUDE / EXCLUDE
----		DECLARE @IS_INCLUDED BIT = 'TRUE'
----		SELECT TOP(1) @IS_INCLUDED = CASE WHEN ATTRIBUTE_TYPE_ID = 11 THEN 'FALSE' ELSE 'TRUE' END
----			FROM TB_ATTRIBUTE_SET
----				INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
----				WHERE ITEM_SET_ID = @ITEM_SET_ID
----		UPDATE TB_ITEM_REVIEW
----			SET IS_INCLUDED = @IS_INCLUDED
----			WHERE ITEM_ID = @ITEM_ID
----	END
----END


----SET NOCOUNT OFF

----go

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ItemAttributeAutoReconcileDelete]    Script Date: 23/03/2016 10:17:52 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_ItemAttributeAutoReconcileDelete]
----(
----	@ITEM_ID BIGINT,
----	@SET_ID INT,
----	@ATTRIBUTE_ID BIGINT,
----	@RECONCILLIATION_TYPE nvarchar(10),
----	@N_PEOPLE int,
----	@AUTO_EXCLUDE bit,
----	@CONTACT_ID int
----)

----As
----SET NOCOUNT ON

----DECLARE @COUNT_RECS INT = 0
----DECLARE @ITEM_SET_ID INT = 0

----IF (@RECONCILLIATION_TYPE = 'no compl')
----BEGIN
----	SET @N_PEOPLE = 99 --(i.e. we don't do anything - none of the rest is executed)
----END
----ELSE
----BEGIN

----	-- **************** STAGE 1: GATHER DATA ON WHETHER RULES FOR AUTO-RECONCILLIATION ARE MET ********************

----	IF (@RECONCILLIATION_TYPE = 'Single')
----	BEGIN
----		SET @COUNT_RECS = 99 -- i.e. we go through to automatic exclude check

----		SELECT TOP(1) @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET
----			WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID
----	END
	
----	IF (@RECONCILLIATION_TYPE = 'auto code') -- agreement at the code level
----	BEGIN
----		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE
----			WHERE ITEM_ID = @ITEM_ID AND ATTRIBUTE_ID = @ATTRIBUTE_ID

----		IF (@COUNT_RECS < @N_PEOPLE)
----		BEGIN
----			SELECT TOP(1) @ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID FROM TB_ITEM_SET
----				INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID
----				WHERE TB_ITEM_SET.ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND IS_COMPLETED = 'TRUE' AND CONTACT_ID = @CONTACT_ID

----			IF (@ITEM_SET_ID = 0)
----			BEGIN -- WE TRY TO GET A COMPLETED ITEM_SET RECORD, BUT THERE MAY NOT BE ONE!
----				SELECT TOP(1) @ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID FROM TB_ITEM_SET
----					INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID
----					WHERE TB_ITEM_SET.ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND CONTACT_ID = @CONTACT_ID
----			END
----		END
----		ELSE
----		BEGIN
----			SET @ITEM_SET_ID = 0
----		END
----	END

----	-- one person has to tick 'include' for it to be included.-- N people agreeing on exclude if nobody has ticked 'include' before this threshold is met
----	IF (@RECONCILLIATION_TYPE = 'auto safet') 
----	BEGIN
----		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
----			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 -- is anything included?

----		SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
----			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 AND IS_COMPLETED = 'TRUE' AND CONTACT_ID = @CONTACT_ID
----		IF (@COUNT_RECS > 0) -- I.E. THE RULES FOR AUTO INCLUSION ARE STILL MET
----		BEGIN
----			--SET @COUNT_RECS = @N_PEOPLE
----			SET @ITEM_SET_ID = 0
----		END
----		ELSE
----		BEGIN
----			-- IF NO INCLUDE IS TICKED, HAVE N PEOPLE TICKED EXCLUDE? IF SO, WE DEFAULT TO THIS
----			SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
----				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 -- EXCLUDED
----			SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
----				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 AND IS_COMPLETED = 'TRUE'  AND CONTACT_ID = @CONTACT_ID
----		END
----		IF (@COUNT_RECS >= @N_PEOPLE) -- I.E. RULE MET, SO WE DON'T UNCOMPLETE
----		BEGIN
----			SET @ITEM_SET_ID = 0
----		END
----	END
	
----	 -- agreement at the include / exclude level
----	IF (@RECONCILLIATION_TYPE = 'auto excl')
----	BEGIN
----		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
----			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 -- INCLUDED
----		SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
----			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 AND IS_COMPLETED = 'TRUE'  AND CONTACT_ID = @CONTACT_ID

----		IF (@COUNT_RECS < @N_PEOPLE)
----		BEGIN
----			SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
----				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 -- EXCLUDED
----			SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
----				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
----				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
----				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 AND IS_COMPLETED = 'TRUE'  AND CONTACT_ID = @CONTACT_ID
----		END
----	END

	

----	-- *************************** STAGE 2: AUTO-RECONCILE AND AUTO-COMPLETE ***************************

----	IF (@COUNT_RECS < @N_PEOPLE) AND (@RECONCILLIATION_TYPE != 'Single') -- REMOVE AUTO-RECONCILLIATION WHERE RULES ARE NO LONGER MET
----	BEGIN
----		IF (@ITEM_SET_ID > 0)
----		BEGIN
----			UPDATE TB_ITEM_SET
----				SET IS_COMPLETED = 'FALSE'
----				WHERE ITEM_SET_ID = @ITEM_SET_ID
----		END
----	END
----	IF (@AUTO_EXCLUDE = 'TRUE' AND ((@COUNT_RECS < @N_PEOPLE) OR @RECONCILLIATION_TYPE = 'Single')) -- WHERE RULES ARE NO LONGER MET - AUTO *INCLUDE*
----	BEGIN
----		-- SECOND, AUTO INCLUDE / EXCLUDE
----		UPDATE TB_ITEM_REVIEW
----			SET IS_INCLUDED = 'TRUE'
----			WHERE ITEM_ID = @ITEM_ID
----	END
----		UPDATE TB_TRAINING_ITEM -- TRY TO RE-LOCK THE ITEM - IF SOMEONE ELSE HAS LOCKED IT, THERE'S NOT MUCH WE CAN DO ABOUT IT THOUGH!
----			SET CONTACT_ID_CODING = @CONTACT_ID,
----			WHEN_LOCKED = CURRENT_TIMESTAMP
----			WHERE ITEM_ID = @ITEM_ID AND CONTACT_ID_CODING = 0
	
----END


----SET NOCOUNT OFF

----GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_TrainingNextItem]    Script Date: 07/04/2016 20:36:36 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_TrainingNextItem]
----(
----	@REVIEW_ID INT,
----	@CONTACT_ID INT,
----	@TRAINING_CODE_SET_ID INT
----)

----As

----SET NOCOUNT ON

----	DECLARE @CURRENT_TRAINING_ID INT
----	DECLARE @UPDATED_TRAINING_ITEM TABLE(TRAINING_ITEM_ID INT)

------ FIRST, GET THE CURRENT TRAINING 'RUN' (CAN'T SEND TO THE STORED PROC, AS IT MAY HAVE CHANGED)
----	SELECT @CURRENT_TRAINING_ID = MAX(TRAINING_ID) FROM TB_TRAINING
----		WHERE REVIEW_ID = @REVIEW_ID
----		AND TIME_STARTED < TIME_ENDED

------ NEXT, LOCK THE ITEM WE'RE GOING TO SEND BACK

----	UPDATE TB_TRAINING_ITEM
----		SET CONTACT_ID_CODING = @CONTACT_ID, WHEN_LOCKED = CURRENT_TIMESTAMP
----		OUTPUT INSERTED.TRAINING_ITEM_ID INTO @UPDATED_TRAINING_ITEM
----		WHERE
----		TRAINING_ITEM_ID = 
----			(SELECT MIN(TRAINING_ITEM_ID) FROM TB_TRAINING_ITEM TI
----				LEFT OUTER JOIN TB_ITEM_SET ISET ON ISET.ITEM_ID = TI.ITEM_ID AND (IS_COMPLETED = 'TRUE' OR ISET.CONTACT_ID = @CONTACT_ID) AND SET_ID = @TRAINING_CODE_SET_ID
----				WHERE (CONTACT_ID_CODING = @CONTACT_ID OR CONTACT_ID_CODING = 0) AND TRAINING_ID = @CURRENT_TRAINING_ID AND ISET.ITEM_ID IS NULL)

----		/*
----			(SELECT MIN(TRAINING_ITEM_ID) FROM TB_TRAINING_ITEM TI
----				WHERE (CONTACT_ID_CODING = 0 OR CONTACT_ID_CODING = @CONTACT_ID) -- i.e. if we've locked something, but not coded it, we are presented with it again
----				 AND TRAINING_ID = @CURRENT_TRAINING_ID AND NOT ITEM_ID IN
----					(SELECT ITEM_ID FROM TB_ITEM_SET WHERE (IS_COMPLETED = 'TRUE' OR CONTACT_ID = @CONTACT_ID) AND SET_ID = @TRAINING_CODE_SET_ID)) -- covers both completed / uncompleted multi-user scenarios
----		*/
------ FINALLY, SEND IT BACK

----	SELECT TI.TRAINING_ITEM_ID, ITEM_ID, [RANK], TRAINING_ID, CONTACT_ID_CODING, SCORE
----		FROM TB_TRAINING_ITEM TI
----		INNER JOIN @UPDATED_TRAINING_ITEM UTI ON UTI.TRAINING_ITEM_ID = TI.TRAINING_ITEM_ID
	
----SET NOCOUNT OFF

----GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_TrainingCheckData]    Script Date: 25/03/2016 11:50:54 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----CREATE procedure [dbo].[st_TrainingCheckData]
----(
----	@REVIEW_ID INT,
----	@N_INCLUDES INT OUTPUT,
----	@N_EXCLUDES INT OUTPUT
----)

----As

----SET NOCOUNT ON

----	SELECT @N_INCLUDES = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_TRAINING_SCREENING_CRITERIA
----	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID
----	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'

----	SELECT @N_EXCLUDES = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_TRAINING_SCREENING_CRITERIA
----	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID
----	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'false'
	
----SET NOCOUNT OFF

----GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_TrainingWriteDataToAzure]    Script Date: 30/03/2016 13:03:09 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_TrainingWriteDataToAzure]
----(
----	@REVIEW_ID INT
------,	@SCREENING_DATA_FILE NVARCHAR(50)
----)

----As

----SET NOCOUNT ON

----	DELETE FROM TB_SCREENING_ML_TEMP WHERE REVIEW_ID = @REVIEW_ID

----	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '1' INCLUDED FROM TB_ITEM_ATTRIBUTE
----	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
----		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
----	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID
			
----	UNION ALL
	
----	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '0' INCLUDED FROM TB_ITEM_ATTRIBUTE
----	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
----		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
----	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

----	UNION ALL
	
----	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_REVIEW.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '99' INCLUDED FROM TB_ITEM_REVIEW
----	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
----		where TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND not TB_ITEM_REVIEW.ITEM_ID in
----		(
----			SELECT ITEM_ID FROM TB_ITEM_SET
----				INNER JOIN TB_REVIEW ON TB_REVIEW.SCREENING_CODE_SET_ID = TB_ITEM_SET.SET_ID
----				WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_REVIEW.REVIEW_ID = @REVIEW_ID
----		)

----	UPDATE TB_REVIEW
----		--SET SCREENING_DATA_FILE = @SCREENING_DATA_FILE,
----		set SCREENING_INDEXED = 'TRUE'
----		WHERE REVIEW_ID = @REVIEW_ID

----	/*
----	-- using temp table so that we don't have long-running queries which cause deadlocks (writes to SQL azure can be slow)
----	declare @tempTable table
----	(
----		REVIEW_ID int,
----		ITEM_ID bigint,
----		TITLE nvarchar(4000),
----		ABSTRACT nvarchar(max),
----		KEYWORDS nvarchar(max)
----	)
	
----	delete from EPPI_ML.[EPPITest].dbo.TB_REVIEW_ITEM_DATA WHERE REVIEW_ID = @REVIEW_ID
----	delete from EPPI_ML.[EPPITest].dbo.TB_REVIEW_ITEM_LABELS WHERE REVIEW_ID = @REVIEW_ID

----	insert into @tempTable(REVIEW_ID, ITEM_ID, TITLE, ABSTRACT, KEYWORDS)
----	SELECT @REVIEW_ID, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, KEYWORDS FROM TB_ITEM
----		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM.ITEM_ID
----		WHERE TB_ITEM_REVIEW.IS_DELETED = 'FALSE' and TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID

----	insert into EPPI_ML.[EPPITest].dbo.TB_REVIEW_ITEM_DATA(REVIEW_ID, ITEM_ID, TITLE, ABSTRACT, KEYWORDS)
----	select * from @tempTable

----	UPDATE TB_REVIEW
----		SET SCREENING_INDEXED = 'TRUE'
----		WHERE REVIEW_ID = @REVIEW_ID
----	*/

----SET NOCOUNT OFF

----GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_TrainingWriteIncludeExcludeToAzure]    Script Date: 30/03/2016 15:53:21 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_TrainingWriteIncludeExcludeToAzure]
----(
----	@REVIEW_ID INT
----)

----As

----SET NOCOUNT ON

----	DELETE FROM TB_SCREENING_ML_TEMP WHERE REVIEW_ID = @REVIEW_ID

----	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, '1' INCLUDED FROM TB_ITEM_ATTRIBUTE
----	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
----		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
----	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID
			
----	UNION ALL
	
----	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, '0' INCLUDED FROM TB_ITEM_ATTRIBUTE
----	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
----		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
----	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

----	UNION ALL
	
----	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_REVIEW.ITEM_ID, '99' INCLUDED FROM TB_ITEM_REVIEW
----		where TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND not TB_ITEM_REVIEW.ITEM_ID in
----		(
----			SELECT ITEM_ID FROM TB_ITEM_SET
----				INNER JOIN TB_REVIEW ON TB_REVIEW.SCREENING_CODE_SET_ID = TB_ITEM_SET.SET_ID
----				WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_REVIEW.REVIEW_ID = @REVIEW_ID
----		)

----	/*
----	declare @tempTable table
----	(
----		REVIEW_ID int,
----		ITEM_ID bigint,
----		LABEL nvarchar(10)
----	)
	
----	delete from EPPI_ML.[EPPITest].dbo.TB_REVIEW_ITEM_LABELS WHERE REVIEW_ID = @REVIEW_ID

----	INSERT INTO @tempTable(REVIEW_ID, ITEM_ID, LABEL)
----	SELECT DISTINCT @REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, '1' FROM TB_ITEM_ATTRIBUTE
----	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
----		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
----	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID
			
----	UNION ALL
	
----	SELECT DISTINCT @REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, '0' FROM TB_ITEM_ATTRIBUTE
----	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
----		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
----	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

----	UNION ALL
	
----	SELECT DISTINCT @REVIEW_ID, TB_ITEM_REVIEW.ITEM_ID, '99' FROM TB_ITEM_REVIEW
----		where TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND not TB_ITEM_REVIEW.ITEM_ID in
----		(
----			SELECT ITEM_ID FROM TB_ITEM_SET
----				INNER JOIN TB_REVIEW ON TB_REVIEW.SCREENING_CODE_SET_ID = TB_ITEM_SET.SET_ID
----				WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_REVIEW.REVIEW_ID = @REVIEW_ID
----		)


----	INSERT INTO EPPI_ML.[EPPITest].dbo.TB_REVIEW_ITEM_LABELS(REVIEW_ID, ITEM_ID, LABEL)
----	select * from @tempTable
----	*/
	
----SET NOCOUNT OFF

----go

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ScreeningCreateMLList]    Script Date: 12/04/2016 18:05:53 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_ScreeningCreateMLList]
----(
----	@REVIEW_ID INT,
----	@CONTACT_ID INT,
----	@WHAT_ATTRIBUTE_ID BIGINT,
----	@SCREENING_MODE nvarchar(10),
----	@CODE_SET_ID INT,
----	@TRAINING_ID INT
----)

----As

----SET NOCOUNT ON

----	DECLARE @TP INT
----	DECLARE @TN INT

----	-- ***** FIRST, GET THE STATS IN TERMS OF # ITEMS SCREENED TO POPULATE THE TRIANING TABLE (GIVES US THE GRAPH ON THE SCREENING TAB)
----	SELECT @TP = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
----		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 10
----		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

----	SELECT @TN = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
----		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 11
----		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
----		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

----	-- ********** SECOND, ENTER THE LIST OF ITEMS INTO TB_TRAINING_ITEM ACCORDING TO WHETHER WE'RE FILTERING BY AN ATTRIBUTE OR DOING THE WHOLE REVIEW

----	IF @WHAT_ATTRIBUTE_ID > 0  -- i.e. we're filtering by a code
----	BEGIN
----		INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK], SCORE)
----			SELECT @TRAINING_ID, AZ.ITEM_ID, 0, 0, AZ.SCORE
----				FROM TB_SCREENING_ML_TEMP AZ
----			INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = AZ.ITEM_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @WHAT_ATTRIBUTE_ID
----			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----			WHERE NOT AZ.ITEM_ID IN
----				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID) AND AZ.REVIEW_ID = @REVIEW_ID
----			ORDER BY AZ.SCORE DESC
			
----	END
----	ELSE -- NOT FILTERING BY A CODE, SO EVERYTHING IN THE REVIEW THAT'S INCLUDED AND SO FAR UNCODED IS INCLUDED
----	BEGIN
----		INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK], SCORE)
----		SELECT @TRAINING_ID, AZ.ITEM_ID, 0, 0, AZ.SCORE
----				FROM TB_SCREENING_ML_TEMP AZ
----			WHERE NOT AZ.ITEM_ID IN
----				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID) AND AZ.REVIEW_ID = @REVIEW_ID
----			ORDER BY AZ.SCORE DESC
----	END

----	/* SET THE RANKS TO INCREMENT */
----	DECLARE @START_INDEX INT = 0
----	SELECT @START_INDEX = MIN(TRAINING_ITEM_ID) FROM TB_TRAINING_ITEM WHERE TRAINING_ID = @TRAINING_ID
----	UPDATE TB_TRAINING_ITEM
----		SET [RANK] = TRAINING_ITEM_ID - @START_INDEX + 1
----		WHERE TRAINING_ID = @TRAINING_ID


----	-- FINALLY, MIGRATE ANY NON-STALE CODING LOCKS FROM THE PREVIOUS TRAINING RUN
----	DECLARE @LAST_TRAINING_ID INT

----	SELECT @LAST_TRAINING_ID = TRAINING_ID FROM TB_TRAINING
----		WHERE REVIEW_ID = @REVIEW_ID AND TRAINING_ID < (SELECT MAX(TRAINING_ID) FROM TB_TRAINING WHERE REVIEW_ID = @REVIEW_ID)

----	DECLARE @CURRENT_ITERATION INT
	
----	SELECT @CURRENT_ITERATION = MAX(ITERATION) + 1 FROM TB_TRAINING
----		WHERE REVIEW_ID = @REVIEW_ID
		
----	IF (@CURRENT_ITERATION IS NULL)
----	BEGIN
----		SET @CURRENT_ITERATION = 1
----	END

----	UPDATE A
----		SET A.CONTACT_ID_CODING = B.CONTACT_ID_CODING,
----		A.WHEN_LOCKED = B.WHEN_LOCKED
----		FROM TB_TRAINING_ITEM A
----		JOIN
----		TB_TRAINING_ITEM B ON A.ITEM_ID = B.ITEM_ID AND CURRENT_TIMESTAMP < DATEADD(DAY, 7, B.WHEN_LOCKED) AND
----			B.TRAINING_ID = @LAST_TRAINING_ID
----		WHERE A.TRAINING_ID = @TRAINING_ID

----	UPDATE TB_TRAINING
----		SET TIME_ENDED = CURRENT_TIMESTAMP,
----		ITERATION = @CURRENT_ITERATION,
----		TRUE_POSITIVES = @TP,
----		TRUE_NEGATIVES = @TN
----		WHERE TB_TRAINING.TRAINING_ID = @TRAINING_ID

----	-- delete the old list(s) of items to screen for this review
----	DELETE TI
----	FROM TB_TRAINING_ITEM TI
----	INNER JOIN TB_TRAINING T ON T.TRAINING_ID = TI.TRAINING_ID
----	WHERE T.REVIEW_ID = @REVIEW_ID AND T.TRAINING_ID < @TRAINING_ID

----	UPDATE TB_REVIEW
----		SET SCREENING_INDEXED = 'TRUE'
----		WHERE REVIEW_ID = @REVIEW_ID


----SET NOCOUNT OFF

----GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_TrainingPreviousItem]    Script Date: 29/03/2016 13:48:32 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_TrainingPreviousItem]
----(
----	@REVIEW_ID INT,
----	@CONTACT_ID INT,
----	@ITEM_ID BIGINT
----)

----As

----SET NOCOUNT ON

----DECLARE @CURRENT_TRAINING_ID INT
----	DECLARE @UPDATED_TRAINING_ITEM TABLE(TRAINING_ITEM_ID INT)

------ FIRST, GET THE CURRENT TRAINING 'RUN' (CAN'T SEND TO THE STORED PROC, AS IT MAY HAVE CHANGED)
----	SELECT @CURRENT_TRAINING_ID = MAX(TRAINING_ID) FROM TB_TRAINING
----		WHERE REVIEW_ID = @REVIEW_ID
----		AND TIME_STARTED < TIME_ENDED

------ NEXT, TRY TO LOCK THE ITEM WE'RE GOING TO SEND BACK (BUT WE WON'T OVERRIDE SOMEONE ELSE'S LOCK)

----	UPDATE TB_TRAINING_ITEM
----		SET CONTACT_ID_CODING = @CONTACT_ID, WHEN_LOCKED = CURRENT_TIMESTAMP
----		WHERE
----		ITEM_ID = @ITEM_ID AND CONTACT_ID_CODING = 0 AND TRAINING_ID = @CURRENT_TRAINING_ID

------ FINALLY, SEND IT BACK

----	SELECT TI.TRAINING_ITEM_ID, ITEM_ID, [RANK], TRAINING_ID, @CONTACT_ID, SCORE
----		FROM TB_TRAINING_ITEM TI
----		WHERE TRAINING_ID = @CURRENT_TRAINING_ID AND ITEM_ID = @ITEM_ID

----SET NOCOUNT OFF

----GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_TrainingInsert]    Script Date: 30/03/2016 13:12:02 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
----ALTER procedure [dbo].[st_TrainingInsert]
----(
----	@REVIEW_ID INT,
----	@CONTACT_ID INT,
----	@N_SCREENED INT = NULL,
----	@NEW_TRAINING_ID INT output
----)

----As

----SET NOCOUNT ON

----	DECLARE @START_TIME DATETIME
----	DECLARE @END_TIME DATETIME

----	SELECT @START_TIME = MAX(TIME_STARTED) FROM TB_TRAINING
----		WHERE REVIEW_ID = @REVIEW_ID

----	SELECT @END_TIME = MAX(TIME_ENDED) FROM TB_TRAINING
----		WHERE REVIEW_ID = @REVIEW_ID
		
----	IF (@START_TIME IS NULL) OR (@START_TIME != @END_TIME) OR
----	(
----		(@START_TIME = @END_TIME) AND CURRENT_TIMESTAMP > DATEADD(MINUTE, 1, @START_TIME) -- i.e. we run whenever something isn't already running and give up after 6 hours (i.e. try again, assuming there was an error)
----	)
----	BEGIN
----		INSERT INTO TB_TRAINING(REVIEW_ID, CONTACT_ID, TIME_STARTED, TIME_ENDED)
----		VALUES (@REVIEW_ID, @CONTACT_ID, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
	   
----	    SET @NEW_TRAINING_ID = @@IDENTITY
----	END
----	ELSE
----	BEGIN
----		SET @NEW_TRAINING_ID = 0
----	END

----/*

----	DECLARE @TIME_WAIT INT = 60  -- we have to wait at least 10 minutes between 'runs'

----	--IF @N_SCREENED > 1000 BEGIN SET @TIME_WAIT = 600 END -- SO WE HAVE TO WAIT 10 MINUTES IF THERE ARE MORE THAN 1000 ITEMS BEING TRAINED
	
----	DECLARE @START_TIME DATETIME
	
----	--SELECT @START_TIME = TIME_STARTED FROM TB_TRAINING
----	--	WHERE REVIEW_ID = @REVIEW_ID
----	--	GROUP BY TIME_STARTED, ITERATION
----	--	HAVING ITERATION = MAX(ITERATION)
----	SELECT @START_TIME = MAX(TIME_STARTED) FROM TB_TRAINING
----		WHERE REVIEW_ID = @REVIEW_ID
		
----	IF (@START_TIME IS NULL OR (CURRENT_TIMESTAMP > DATEADD(SECOND, @TIME_WAIT, @START_TIME)))
----	BEGIN
----		INSERT INTO TB_TRAINING(REVIEW_ID, CONTACT_ID, TIME_STARTED, TIME_ENDED)
----		VALUES (@REVIEW_ID, @CONTACT_ID, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
	   
----	    SET @NEW_TRAINING_ID = @@IDENTITY
----	END
----	ELSE
----	BEGIN
----		SET @NEW_TRAINING_ID = 0
----	END

----*/
----SET NOCOUNT OFF

----GO



------USE [Reviewer]
------GO
------/****** Object:  StoredProcedure [dbo].[st_SearchNullAbstract]    Script Date: 11/01/2016 15:38:22 ******/
------SET ANSI_NULLS OFF
------GO
------SET QUOTED_IDENTIFIER ON
------GO
------CREATE PROCEDURE [dbo].[st_SearchNullAbstract]
------(
------	@SEARCH_ID int = null output
------,	@CONTACT_ID nvarchar(50) = null
------,	@REVIEW_ID nvarchar(50) = null
------,	@SEARCH_TITLE varchar(4000) = null
------,	@INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'
------)

------AS
------BEGIN
------	-- Step One: Insert record into tb_SEARCH
------	EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, '', '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

------	-- Step Two: Perform the search and get a hits count

------	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
------		SELECT DISTINCT TB_ITEM.ITEM_ID, @SEARCH_ID, 0 FROM TB_ITEM
------			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
------		WHERE TB_ITEM.ABSTRACT IS NULL OR TB_ITEM.ABSTRACT = ''
------			AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
			
------	-- Step Three: Update the new search record in tb_SEARCH with the number of records added
------	UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID

------END

------GO

------USE [Reviewer]
------GO
------ALTER TABLE dbo.TB_REVIEW ADD
------	[BL_ACCOUNT_CODE] [nvarchar](50) NULL,
------	[BL_AUTH_CODE] [nvarchar](50) NULL,
------	[BL_TX] [nvarchar](50) NULL,
------	[BL_CC_ACCOUNT_CODE] [nvarchar](50) NULL,
------	[BL_CC_AUTH_CODE] [nvarchar](50) NULL,
------	[BL_CC_TX] [nvarchar](50) NULL
------GO

------USE [Reviewer]
------GO
------/****** Object:  StoredProcedure [dbo].[st_ReviewContactForSiteAdmin]    Script Date: 04/02/2016 17:29:25 ******/
------SET ANSI_NULLS ON
------GO
------SET QUOTED_IDENTIFIER ON
------GO

------ALTER procedure [dbo].[st_ReviewContactForSiteAdmin]
------(
------	@CONTACT_ID INT
------	,@REVIEW_ID int
------)

------As

------SELECT 0 as REVIEW_CONTACT_ID, - r.REVIEW_ID as REVIEW_ID, rc.CONTACT_ID, REVIEW_NAME, 'AdminUser;' as ROLES
------	,own.CONTACT_NAME as 'OWNER', case when LR is null
------									then r.DATE_CREATED
------									else LR
------								 end
------								 as 'LAST_ACCESS'
------	, r.SHOW_SCREENING, r.ALLOW_REVIEWER_TERMS, r.ALLOW_CLUSTERED_SEARCH, r.SCREENING_CODE_SET_ID,
------	BL_ACCOUNT_CODE,BL_AUTH_CODE, BL_TX, BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX
------FROM TB_CONTACT rc
------INNER JOIN TB_REVIEW r ON rc.CONTACT_ID = @CONTACT_ID and rc.IS_SITE_ADMIN = 1 and r.REVIEW_ID = @REVIEW_ID 
------inner join TB_CONTACT own on r.FUNDER_ID = own.CONTACT_ID
------left join (
------			select MAX(LAST_RENEWED) LR, REVIEW_ID
------			from ReviewerAdmin.dbo.TB_LOGON_TICKET  
------			where @CONTACT_ID = CONTACT_ID and REVIEW_ID = @REVIEW_ID
------			group by REVIEW_ID
------			) as t
------			on t.REVIEW_ID = r.REVIEW_ID
------WHERE rc.CONTACT_ID = @CONTACT_ID
------ORDER BY REVIEW_NAME

------GO

------USE [Reviewer]
------GO
------/****** Object:  StoredProcedure [dbo].[st_ReviewContact]    Script Date: 04/02/2016 17:26:40 ******/
------SET ANSI_NULLS ON
------GO
------SET QUOTED_IDENTIFIER ON
------GO
------ALTER procedure [dbo].[st_ReviewContact]
------(
------	@CONTACT_ID INT
------)

------As

------SELECT REVIEW_CONTACT_ID, rc.REVIEW_ID, rc.CONTACT_ID, REVIEW_NAME, dbo.fn_REBUILD_ROLES(rc.REVIEW_CONTACT_ID) as ROLES
------	,own.CONTACT_NAME as 'OWNER', case when LR is null
------									then r.DATE_CREATED
------									else LR
------								 end
------								 as 'LAST_ACCESS'
------	, r.SHOW_SCREENING, r.ALLOW_REVIEWER_TERMS, r.ALLOW_CLUSTERED_SEARCH, r.SCREENING_CODE_SET_ID,
------	BL_ACCOUNT_CODE,BL_AUTH_CODE, BL_TX, BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX
------FROM TB_REVIEW_CONTACT rc
------INNER JOIN TB_REVIEW r ON rc.REVIEW_ID = r.REVIEW_ID
------inner join TB_CONTACT own on r.FUNDER_ID = own.CONTACT_ID
------left join (
------			select MAX(LAST_RENEWED) LR, REVIEW_ID
------			from ReviewerAdmin.dbo.TB_LOGON_TICKET  
------			where @CONTACT_ID = CONTACT_ID
------			group by REVIEW_ID
------			) as t
------			on t.REVIEW_ID = r.REVIEW_ID
------WHERE rc.CONTACT_ID = @CONTACT_ID and (r.ARCHIE_ID is null OR r.ARCHIE_ID = 'prospective_______')
------ORDER BY REVIEW_NAME

------GO


--------USE [Reviewer]
--------GO
--------ALTER TABLE dbo.TB_META_ANALYSIS ADD
--------	[ATTRIBUTE_ID_ANSWER] [nvarchar](max) NULL,
--------	[ATTRIBUTE_ID_QUESTION] [nvarchar](max) NULL,
--------	[GRID_SETTINGS] [nvarchar](max) NULL
--------GO

--------USE [Reviewer]
--------GO
--------/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisList]    Script Date: 07/09/2015 18:56:02 ******/
--------SET ANSI_NULLS ON
--------GO
--------SET QUOTED_IDENTIFIER ON
--------GO
--------ALTER procedure [dbo].[st_MetaAnalysisList]
--------(
--------	@REVIEW_ID INT,
--------	@CONTACT_ID INT
--------)

--------As

--------SET NOCOUNT ON
	
--------	SELECT META_ANALYSIS_ID, META_ANALYSIS_TITLE, TB_META_ANALYSIS.CONTACT_ID, REVIEW_ID,
--------	TB_META_ANALYSIS.ATTRIBUTE_ID, SET_ID, ATTRIBUTE_ID_INTERVENTION, ATTRIBUTE_ID_CONTROL,
--------	ATTRIBUTE_ID_OUTCOME, TB_META_ANALYSIS.META_ANALYSIS_TYPE_ID, META_ANALYSIS_TYPE_TITLE,
--------	ATTRIBUTE_ID_ANSWER, ATTRIBUTE_ID_QUESTION,	GRID_SETTINGS,
--------	--A1.ATTRIBUTE_NAME AS INTERVENTION_TEXT,	a2.ATTRIBUTE_NAME AS CONTROL_TEXT, a3.ATTRIBUTE_NAME AS OUTCOME_TEXT,

--------	(SELECT ATTRIBUTE_NAME + '¬' FROM TB_ATTRIBUTE
--------		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_ANSWER, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
--------		FOR XML PATH('')) AS ATTRIBUTE_ANSWER_TEXT,

--------	(SELECT ATTRIBUTE_NAME + '¬' FROM TB_ATTRIBUTE
--------		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_QUESTION, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
--------		FOR XML PATH('')) AS ATTRIBUTE_QUESTION_TEXT
	
--------	FROM TB_META_ANALYSIS
	
--------	INNER JOIN TB_META_ANALYSIS_TYPE ON TB_META_ANALYSIS_TYPE.META_ANALYSIS_TYPE_ID =
--------		TB_META_ANALYSIS.META_ANALYSIS_TYPE_ID
	
--------	/*	
--------	don't need this any more (for old MA interface) ??
--------	left outer JOIN TB_ITEM_ATTRIBUTE IA1 ON IA1.ITEM_ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_INTERVENTION
--------	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_INTERVENTION 
--------	left outer JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_CONTROL
--------	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_CONTROL
--------	left outer JOIN TB_ITEM_ATTRIBUTE IA3 ON IA3.ITEM_ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_OUTCOME
--------	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_OUTCOME 
--------	*/
--------	WHERE REVIEW_ID = @REVIEW_ID

--------SET NOCOUNT OFF


--------/*

--------(SELECT COALESCE(ATTRIBUTE_ANSWER_TEXT + '¬', '') + ATTRIBUTE_NAME 
--------	FROM TB_ATTRIBUTE
--------		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_ANSWER, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
--------		WHERE ATTRIBUTE_NAME IS NOT NULL) ATTRIBUTE_ANSWER_TEXT

--------		*/
--------GO

--------USE [Reviewer]
--------GO
--------/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisInsert]    Script Date: 15/09/2015 23:16:43 ******/
--------SET ANSI_NULLS ON
--------GO
--------SET QUOTED_IDENTIFIER ON
--------GO
--------ALTER procedure [dbo].[st_MetaAnalysisInsert]
--------(
--------	@TITLE NVARCHAR(255),
--------	@CONTACT_ID INT,
--------	@REVIEW_ID INT,
--------	@ATTRIBUTE_ID BIGINT = NULL,
--------	@SET_ID INT = NULL,
--------	@ATTRIBUTE_ID_INTERVENTION BIGINT,
--------	@ATTRIBUTE_ID_CONTROL BIGINT,
--------	@ATTRIBUTE_ID_OUTCOME BIGINT,
--------	@META_ANALYSIS_TYPE_ID INT,
--------	@OUTCOME_IDS nvarchar(max),
--------	@ATTRIBUTE_ID_ANSWER nvarchar(max) = '',
--------	@ATTRIBUTE_ID_QUESTION nvarchar(max) = '',
--------	@GRID_SETTINGS NVARCHAR(MAX) = '',
--------	@NEW_META_ANALYSIS_ID INT OUTPUT,

--------	@ATTRIBUTE_ANSWER_TEXT NVARCHAR(MAX) OUTPUT,
--------	@ATTRIBUTE_QUESTION_TEXT NVARCHAR(MAX) OUTPUT
--------)

--------As

--------SET NOCOUNT ON
	
--------	INSERT INTO TB_META_ANALYSIS
--------	(	META_ANALYSIS_TITLE
--------	,	CONTACT_ID
--------	,	REVIEW_ID
--------	,	ATTRIBUTE_ID
--------	,	SET_ID
--------	,	ATTRIBUTE_ID_INTERVENTION
--------	,	ATTRIBUTE_ID_CONTROL
--------	,	ATTRIBUTE_ID_OUTCOME
--------	,	META_ANALYSIS_TYPE_ID
--------	,	ATTRIBUTE_ID_ANSWER
--------	,	ATTRIBUTE_ID_QUESTION
--------	,	GRID_SETTINGS
--------	)	
--------	VALUES
--------	(
--------		@TITLE
--------	,	@CONTACT_ID
--------	,	@REVIEW_ID
--------	,	@ATTRIBUTE_ID
--------	,	@SET_ID
--------	,	@ATTRIBUTE_ID_INTERVENTION
--------	,	@ATTRIBUTE_ID_CONTROL
--------	,	@ATTRIBUTE_ID_OUTCOME
--------	,	@META_ANALYSIS_TYPE_ID
--------	,	@ATTRIBUTE_ID_ANSWER
--------	,	@ATTRIBUTE_ID_QUESTION
--------	,	@GRID_SETTINGS
--------	)
--------	-- Get the identity and return it
--------	SET @NEW_META_ANALYSIS_ID = @@identity
	
--------	IF (@OUTCOME_IDS != '')
--------	BEGIN
--------		INSERT INTO TB_META_ANALYSIS_OUTCOME (META_ANALYSIS_ID, OUTCOME_ID)
--------		SELECT @NEW_META_ANALYSIS_ID, VALUE from DBO.fn_Split_int(@OUTCOME_IDS, ',')
--------	END

--------	SELECT @ATTRIBUTE_ANSWER_TEXT = COALESCE(@ATTRIBUTE_ANSWER_TEXT + '¬', '') + ATTRIBUTE_NAME 
--------	FROM TB_ATTRIBUTE
--------		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_ANSWER, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
--------		WHERE ATTRIBUTE_NAME IS NOT NULL

--------	SELECT @ATTRIBUTE_QUESTION_TEXT = COALESCE(@ATTRIBUTE_QUESTION_TEXT + '¬', '') + ATTRIBUTE_NAME 
--------	FROM TB_ATTRIBUTE
--------		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_QUESTION, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
--------		WHERE ATTRIBUTE_NAME IS NOT NULL

--------SET NOCOUNT OFF

--------GO

--------USE [Reviewer]
--------GO
--------/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisUpdate]    Script Date: 07/09/2015 18:50:17 ******/
--------SET ANSI_NULLS ON
--------GO
--------SET QUOTED_IDENTIFIER ON
--------GO
--------ALTER procedure [dbo].[st_MetaAnalysisUpdate]
--------(
--------	@META_ANALYSIS_ID INT,
--------	@TITLE NVARCHAR(255),
--------	@CONTACT_ID INT,
--------	@REVIEW_ID INT,
--------	@ATTRIBUTE_ID BIGINT = NULL,
--------	@SET_ID INT = NULL,
--------	@ATTRIBUTE_ID_INTERVENTION BIGINT,
--------	@ATTRIBUTE_ID_CONTROL BIGINT,
--------	@ATTRIBUTE_ID_OUTCOME BIGINT,
--------	@OUTCOME_IDS nvarchar(max),
--------	@ATTRIBUTE_ID_ANSWER NVARCHAR(MAX) = '',
--------	@ATTRIBUTE_ID_QUESTION NVARCHAR(MAX) = '',
--------	@META_ANALYSIS_TYPE_ID INT,
--------	@GRID_SETTINGS NVARCHAR(MAX) = '',

--------	@ATTRIBUTE_ANSWER_TEXT NVARCHAR(MAX) OUTPUT,
--------	@ATTRIBUTE_QUESTION_TEXT NVARCHAR(MAX) OUTPUT
--------)

--------As

--------SET NOCOUNT ON
	
--------	UPDATE TB_META_ANALYSIS
--------	SET	META_ANALYSIS_TITLE = @TITLE
--------	,	CONTACT_ID = @CONTACT_ID
--------	,	REVIEW_ID = @REVIEW_ID
--------	,	ATTRIBUTE_ID = @ATTRIBUTE_ID
--------	,	SET_ID = @SET_ID
--------	,	ATTRIBUTE_ID_INTERVENTION = @ATTRIBUTE_ID_INTERVENTION
--------	,	ATTRIBUTE_ID_CONTROL = @ATTRIBUTE_ID_CONTROL
--------	,	ATTRIBUTE_ID_OUTCOME = @ATTRIBUTE_ID_OUTCOME
--------	,	META_ANALYSIS_TYPE_ID = @META_ANALYSIS_TYPE_ID
--------	,	ATTRIBUTE_ID_ANSWER = @ATTRIBUTE_ID_ANSWER
--------	,	ATTRIBUTE_ID_QUESTION = @ATTRIBUTE_ID_QUESTION
--------	,	GRID_SETTINGS = @GRID_SETTINGS
	
--------	WHERE META_ANALYSIS_ID = @META_ANALYSIS_ID
	
--------	DELETE FROM TB_META_ANALYSIS_OUTCOME WHERE META_ANALYSIS_ID = @META_ANALYSIS_ID
	
--------	IF (@OUTCOME_IDS != '')
--------	BEGIN
--------		INSERT INTO TB_META_ANALYSIS_OUTCOME (META_ANALYSIS_ID, OUTCOME_ID)
--------		SELECT @META_ANALYSIS_ID, VALUE from DBO.fn_Split_int(@OUTCOME_IDS, ',')
--------	END

--------	SELECT @ATTRIBUTE_ANSWER_TEXT = COALESCE(@ATTRIBUTE_ANSWER_TEXT + '¬', '') + ATTRIBUTE_NAME 
--------	FROM TB_ATTRIBUTE
--------		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_ANSWER, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
--------		WHERE ATTRIBUTE_NAME IS NOT NULL

--------	SELECT @ATTRIBUTE_QUESTION_TEXT = COALESCE(@ATTRIBUTE_QUESTION_TEXT + '¬', '') + ATTRIBUTE_NAME 
--------	FROM TB_ATTRIBUTE
--------		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_QUESTION, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
--------		WHERE ATTRIBUTE_NAME IS NOT NULL

--------SET NOCOUNT OFF

--------go

--------USE [Reviewer]
--------GO
--------/****** Object:  StoredProcedure [dbo].[st_MetaAnalysis]    Script Date: 05/05/2015 22:42:26 ******/
--------SET ANSI_NULLS ON
--------GO
--------SET QUOTED_IDENTIFIER ON
--------GO
--------ALTER procedure [dbo].[st_MetaAnalysis]
--------(
--------	@META_ANALYSIS_ID INT
--------)

--------As

--------SET NOCOUNT ON
	
--------	SELECT META_ANALYSIS_ID, META_ANALYSIS_TITLE, CONTACT_ID, REVIEW_ID,
--------	ATTRIBUTE_ID, SET_ID, ATTRIBUTE_ID_INTERVENTION, ATTRIBUTE_ID_CONTROL,
--------	ATTRIBUTE_ID_ANSWER, ATTRIBUTE_ID_QUESTION,	ATTRIBUTE_ID_OUTCOME, META_ANALYSIS_TYPE_ID
	
--------	FROM TB_META_ANALYSIS
	
--------	WHERE META_ANALYSIS_ID = @META_ANALYSIS_ID

--------SET NOCOUNT OFF
--------GO

--------USE [Reviewer]
--------GO
--------/****** Object:  StoredProcedure [dbo].[st_OutcomeList]    Script Date: 15/09/2015 23:07:07 ******/
--------SET ANSI_NULLS ON
--------GO
--------SET QUOTED_IDENTIFIER ON
--------GO
--------ALTER procedure [dbo].[st_OutcomeList]
--------(
--------	/*
--------	@REVIEW_ID INT,
--------	@SET_ID BIGINT,
--------	@ITEM_ATTRIBUTE_ID_INTERVENTION BIGINT = NULL,
--------	@ITEM_ATTRIBUTE_ID_CONTROL BIGINT = NULL,
--------	@ITEM_ATTRIBUTE_ID_OUTCOME BIGINT = NULL,
--------	@ATTRIBUTE_ID BIGINT = NULL,
--------	@META_ANALYSIS_ID INT = NULL
--------	*/
--------	@VARIABLES NVARCHAR(MAX) = NULL,
--------	@ANSWERS NVARCHAR(MAX) = '',
--------	@QUESTIONS NVARCHAR(MAX) = ''
--------)

--------As

--------SET NOCOUNT ON

--------DECLARE @START_TEXT NVARCHAR(MAX) = N' SELECT distinct tio.OUTCOME_ID, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
--------	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, SHORT_TITLE, OUTCOME_DESCRIPTION,
--------	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
--------	TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_OUTCOME_ID, TB_ITEM_SET.ITEM_SET_ID, TB_ITEM_ATTRIBUTE.ITEM_ID,
--------	A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT, A3.ATTRIBUTE_NAME OUTCOME_TEXT'
	
--------DECLARE @END_TEXT NVARCHAR(MAX) = N' FROM TB_ITEM_OUTCOME tio

--------	inner join TB_ITEM_SET on tb_item_set.ITEM_SET_ID = tio.ITEM_SET_ID
--------		AND TB_ITEM_SET.IS_COMPLETED = ''TRUE''
--------	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tb_item_set.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--------	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tb_item_set.ITEM_SET_ID
--------	inner join TB_ITEM on TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
	
--------	LEFT OUTER JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
--------		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID
		
--------	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_INTERVENTION 
--------	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_CONTROL
--------	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_OUTCOME'
	
--------DECLARE @QUERY NVARCHAR(MAX) = @VARIABLES + @START_TEXT + @ANSWERS + @QUESTIONS + @END_TEXT
	
--------EXEC (@QUERY)

--------/*
--------SELECT distinct tio.OUTCOME_ID, SHORT_TITLE, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
--------	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, OUTCOME_DESCRIPTION,
--------	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
--------	TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_OUTCOME_ID, TB_ITEM_SET.ITEM_SET_ID,
--------	TB_ITEM_ATTRIBUTE.ITEM_ID, A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT,
--------		A3.ATTRIBUTE_NAME OUTCOME_TEXT
	
--------	FROM TB_ITEM_OUTCOME tio

--------	inner join TB_ITEM_SET on tb_item_set.ITEM_SET_ID = tio.ITEM_SET_ID
--------		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
--------	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tb_item_set.ITEM_ID
--------	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tb_item_set.ITEM_SET_ID
--------	inner join TB_ITEM on TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
	
--------	LEFT OUTER JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
--------		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID
		
--------	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_INTERVENTION 
--------	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_CONTROL
--------	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_OUTCOME
	
--------	WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--------	AND (@ITEM_ATTRIBUTE_ID_INTERVENTION = 0 OR (ITEM_ATTRIBUTE_ID_INTERVENTION = @ITEM_ATTRIBUTE_ID_INTERVENTION))
--------	AND (@ITEM_ATTRIBUTE_ID_CONTROL = 0 OR (ITEM_ATTRIBUTE_ID_CONTROL = @ITEM_ATTRIBUTE_ID_CONTROL))
--------	AND (@ITEM_ATTRIBUTE_ID_OUTCOME = 0 OR (ITEM_ATTRIBUTE_ID_OUTCOME = @ITEM_ATTRIBUTE_ID_OUTCOME))
--------	--	AND (@ATTRIBUTE_ID IS NULL OR (TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID))
--------	--AND (
--------	--	@ATTRIBUTE_ID IS NULL OR 
--------	--		(
--------	--		TB_ITEM_SET.ITEM_ID IN
--------	--			( 
--------	--			SELECT IA2.ITEM_ID FROM TB_ITEM_ATTRIBUTE IA2 
--------	--			INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
--------	--			WHERE IA2.ATTRIBUTE_ID = @ATTRIBUTE_ID
--------	--			)
--------	--		)
--------	--	)
--------	--AND (--temp correction for before publishing: @ATTRIBUTE_ID is (because of bug) actually the item_attribute_id
--------	--	@ATTRIBUTE_ID = 0 OR 
--------	--		(
--------	--		tio.OUTCOME_ID IN
--------	--			( 
--------	--				select tio2.OUTCOME_ID from TB_ATTRIBUTE_SET tas
--------	--				inner join TB_ITEM_OUTCOME_ATTRIBUTE ioa on tas.ATTRIBUTE_ID = ioa.ATTRIBUTE_ID and tas.ATTRIBUTE_SET_ID = @ATTRIBUTE_ID
--------	--				inner join TB_ITEM_OUTCOME tio2 on ioa.OUTCOME_ID = tio2.OUTCOME_ID
--------	--			)
--------	--		)
--------	--	)--end of temp correction
--------	AND (--real correction to use when bug is corrected in line 174 of dialogMetaAnalysisSetup.xaml.cs
--------		@ATTRIBUTE_ID = 0 OR 
--------			(
--------			tio.OUTCOME_ID IN 
--------				( 
--------					select tio2.OUTCOME_ID from TB_ITEM_OUTCOME_ATTRIBUTE ioa  
--------					inner join TB_ITEM_OUTCOME tio2 on ioa.OUTCOME_ID = tio2.OUTCOME_ID and ioa.ATTRIBUTE_ID = @ATTRIBUTE_ID
--------				)
--------			)
--------		)--end of real correction
--------	AND (@SET_ID = 0 OR (TB_ITEM_SET.SET_ID = @SET_ID))
	
--------	--order by TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
--------*/
--------SET NOCOUNT OFF

--------GO

----------USE [Reviewer]
----------GO

----------/****** Object:  Table [dbo].[tb_CLASSIFIER_MODEL]    Script Date: 06/10/2014 13:44:41 ******/
----------SET ANSI_NULLS ON
----------GO

----------SET QUOTED_IDENTIFIER ON
----------GO

----------SET ANSI_PADDING ON
----------GO

----------CREATE TABLE [dbo].[tb_CLASSIFIER_MODEL](
----------	[MODEL_ID] [int] IDENTITY(1,1) NOT NULL,
----------	[MODEL_TITLE] [nvarchar](1000) NULL,
----------	[CONTACT_ID] [int] NULL,
----------	[REVIEW_ID] [int] NULL,
----------	[ATTRIBUTE_ID_ON] [bigint] NULL,
----------	[ATTRIBUTE_ID_NOT_ON] [bigint] NULL,
----------	[MODEL_INFO] [varbinary](max) NULL,
----------	[TP] [float] NULL,
----------	[FP] [float] NULL,
----------	[TN] [float] NULL,
----------	[FN] [float] NULL,
---------- CONSTRAINT [PK_tb_CLASSIFIER_MODEL] PRIMARY KEY CLUSTERED 
----------(
----------	[MODEL_ID] ASC
----------)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
----------) ON [PRIMARY]

----------GO

----------SET ANSI_PADDING OFF
----------GO

----------USE [Reviewer]
----------GO
----------/****** Object:  StoredProcedure [dbo].[st_ClassifierGetTrainingData]    Script Date: 06/09/2014 11:37:11 ******/
----------SET ANSI_NULLS ON
----------GO
----------SET QUOTED_IDENTIFIER ON
----------GO
----------create procedure [dbo].[st_ClassifierGetTrainingData]
----------(
----------	@REVIEW_ID INT
----------,	@ATTRIBUTE_ID_ON BIGINT = NULL
----------,	@ATTRIBUTE_ID_NOT_ON BIGINT = NULL
----------)

----------As

----------SET NOCOUNT ON

----------	SELECT DISTINCT '+1', RT_ITV.ITEM_ID, VECTORS FROM ReviewerTerms.dbo.TB_ITEM_TERM_VECTORS RT_ITV
----------	INNER JOIN REVIEWER.DBO.TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = RT_ITV.ITEM_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_ON
----------	INNER JOIN REVIEWER.DBO.TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----------		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----------	INNER JOIN Reviewer.dbo.TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = RT_ITV.ITEM_ID
----------	WHERE RT_ITV.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		
----------	UNION
	
----------	SELECT DISTINCT '-1', RT_ITV.ITEM_ID, VECTORS FROM ReviewerTerms.dbo.TB_ITEM_TERM_VECTORS RT_ITV
----------	INNER JOIN REVIEWER.DBO.TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = RT_ITV.ITEM_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_NOT_ON
----------	INNER JOIN REVIEWER.DBO.TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----------		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----------	INNER JOIN Reviewer.dbo.TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = RT_ITV.ITEM_ID
----------	WHERE RT_ITV.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'

----------SET NOCOUNT OFF

----------GO

----------USE [Reviewer]
----------GO
----------/****** Object:  StoredProcedure [dbo].[st_ClassifierModels]    Script Date: 06/09/2014 11:37:11 ******/
----------SET ANSI_NULLS ON
----------GO
----------SET QUOTED_IDENTIFIER ON
----------GO
----------create procedure [dbo].[st_ClassifierModels]
----------(
----------	@REVIEW_ID INT
----------)

----------As

----------SET NOCOUNT ON

----------	select MODEL_ID, MODEL_TITLE, REVIEW_ID, A1.ATTRIBUTE_NAME ATTRIBUTE_ON, A2.ATTRIBUTE_NAME ATTRIBUTE_NOT_ON,
----------		CONTACT_NAME, tb_CLASSIFIER_MODEL.CONTACT_ID, MODEL_INFO, TP, TN, FP, FN from tb_CLASSIFIER_MODEL
----------	INNER JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_ON
----------	INNER JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_NOT_ON
----------	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = tb_CLASSIFIER_MODEL.CONTACT_ID
	
----------	where REVIEW_ID = @REVIEW_ID
----------	order by MODEL_ID

----------SET NOCOUNT OFF

----------go

----------USE [Reviewer]
----------GO
----------/****** Object:  StoredProcedure [dbo].[st_ClassifierSaveModel]    Script Date: 06/09/2014 11:37:11 ******/
----------SET ANSI_NULLS ON
----------GO
----------SET QUOTED_IDENTIFIER ON
----------GO
----------create procedure [dbo].[st_ClassifierSaveModel]
----------(
----------	@REVIEW_ID INT
----------,	@ATTRIBUTE_ID_ON BIGINT = NULL
----------,	@ATTRIBUTE_ID_NOT_ON BIGINT = NULL
----------,	@CONTACT_ID INT
----------,	@MODEL_TITLE NVARCHAR(1000) = NULL
----------,	@MODEL VARBINARY(MAX)
----------,	@TP NUMERIC = NULL
----------,	@TN NUMERIC = NULL
----------,	@FP NUMERIC = NULL
----------,	@FN NUMERIC = NULL
----------)

----------As

----------SET NOCOUNT ON

----------	INSERT INTO tb_CLASSIFIER_MODEL(MODEL_TITLE, CONTACT_ID, REVIEW_ID, ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, MODEL_INFO, TP, TN, FP, FN)
----------	VALUES(@MODEL_TITLE, @CONTACT_ID, @REVIEW_ID, @ATTRIBUTE_ID_ON, @ATTRIBUTE_ID_NOT_ON, @MODEL, @TP, @TN, @FP, @FN)

----------SET NOCOUNT OFF

----------GO

----------USE [Reviewer]
----------GO
----------/****** Object:  StoredProcedure [dbo].[st_ClassifierModels]    Script Date: 06/09/2014 11:37:11 ******/
----------SET ANSI_NULLS ON
----------GO
----------SET QUOTED_IDENTIFIER ON
----------GO
----------create procedure [dbo].[st_ClassifierModels]
----------(
----------	@REVIEW_ID INT
----------)

----------As

----------SET NOCOUNT ON

----------	select MODEL_ID, MODEL_TITLE, REVIEW_ID, A1.ATTRIBUTE_NAME ATTRIBUTE_ON, A2.ATTRIBUTE_NAME ATTRIBUTE_NOT_ON,
----------		CONTACT_NAME, tb_CLASSIFIER_MODEL.CONTACT_ID, MODEL_INFO, TP, TN, FP, FN from tb_CLASSIFIER_MODEL
----------	INNER JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_ON
----------	INNER JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_NOT_ON
----------	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = tb_CLASSIFIER_MODEL.CONTACT_ID
	
----------	where REVIEW_ID = @REVIEW_ID
----------	order by MODEL_ID

----------SET NOCOUNT OFF

----------GO

----------USE [Reviewer]
----------GO
----------/****** Object:  StoredProcedure [dbo].[st_ClassifierGetModel]    Script Date: 06/10/2014 13:17:48 ******/
----------SET ANSI_NULLS ON
----------GO
----------SET QUOTED_IDENTIFIER ON
----------GO
----------CREATE procedure [dbo].[st_ClassifierGetModel]
----------(
----------	@MODEL_ID INT
----------)

----------As

----------SET NOCOUNT ON

----------	SELECT MODEL_INFO FROM tb_CLASSIFIER_MODEL
----------	WHERE MODEL_ID = @MODEL_ID

----------SET NOCOUNT OFF

----------GO

----------USE [Reviewer]
----------GO
----------/****** Object:  StoredProcedure [dbo].[st_ClassifierGetClassificationData]    Script Date: 06/10/2014 13:19:46 ******/
----------SET ANSI_NULLS ON
----------GO
----------SET QUOTED_IDENTIFIER ON
----------GO
----------CREATE procedure [dbo].[st_ClassifierGetClassificationData]
----------(
----------	@REVIEW_ID INT
----------,	@ATTRIBUTE_ID_ON BIGINT = NULL
----------)

----------As

----------SET NOCOUNT ON

----------	SELECT DISTINCT '0', RT_ITV.ITEM_ID, VECTORS FROM ReviewerTerms.dbo.TB_ITEM_TERM_VECTORS RT_ITV
----------	INNER JOIN REVIEWER.DBO.TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = RT_ITV.ITEM_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_ON
----------	INNER JOIN REVIEWER.DBO.TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
----------		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----------	INNER JOIN Reviewer.dbo.TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = RT_ITV.ITEM_ID
----------	WHERE RT_ITV.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		
----------SET NOCOUNT OFF

----------GO

----------USE [Reviewer]
----------GO
----------/****** Object:  StoredProcedure [dbo].[st_SearchUpdateHitCount]    Script Date: 06/10/2014 13:42:07 ******/
----------SET ANSI_NULLS ON
----------GO
----------SET QUOTED_IDENTIFIER ON
----------GO
----------CREATE procedure [dbo].[st_SearchUpdateHitCount]
----------(
----------	@SEARCH_ID INT,
----------	@NEW_HITCOUNT INT = 0
----------)

----------As

----------SET NOCOUNT ON

----------	UPDATE TB_SEARCH
----------		SET HITS_NO = @NEW_HITCOUNT
----------		WHERE SEARCH_ID = @SEARCH_ID

----------SET NOCOUNT OFF

----------GO


-------------- *********** CREATE FIELD IN TB_REVIEW: SCREENING_CODE_SET_ID INT ************************************************
------------USE [Reviewer]
------------GO
------------/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
------------BEGIN TRANSACTION
------------SET QUOTED_IDENTIFIER ON
------------SET ARITHABORT ON
------------SET NUMERIC_ROUNDABORT OFF
------------SET CONCAT_NULL_YIELDS_NULL ON
------------SET ANSI_NULLS ON
------------SET ANSI_PADDING ON
------------SET ANSI_WARNINGS ON
------------COMMIT
------------BEGIN TRANSACTION
------------GO
------------ALTER TABLE dbo.TB_REVIEW ADD
------------	SCREENING_CODE_SET_ID int NULL
------------GO
------------ALTER TABLE dbo.TB_REVIEW SET (LOCK_ESCALATION = TABLE)
------------GO
------------COMMIT

------------USE [Reviewer]
------------GO
------------/****** Object:  StoredProcedure [dbo].[st_ContactLoginReview]    Script Date: 01/09/2014 16:27:43 ******/
------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------ALTER procedure [dbo].[st_ContactLoginReview]
------------(
------------	@userId int,
------------	@reviewId int,
------------	@GUI uniqueidentifier OUTPUT
------------)

------------As
------------SELECT TB_REVIEW.REVIEW_ID, ROLE_NAME as [ROLE], 
------------		( CASE WHEN sl2.[EXPIRY_DATE] is not null
------------				and sl2.[EXPIRY_DATE] > TB_REVIEW.[EXPIRY_DATE]
------------					then sl2.[EXPIRY_DATE]
------------				else TB_REVIEW.[EXPIRY_DATE]
------------				end
------------		) as REVIEW_EXP, 
------------		( CASE when sl.[EXPIRY_DATE] is not null 
------------				and sl.[EXPIRY_DATE] > c.[EXPIRY_DATE]
------------					then sl.[EXPIRY_DATE]
------------				else c.[EXPIRY_DATE]
------------				end
------------		) as CONTACT_EXP,
------------		FUNDER_ID
		
------------FROM TB_REVIEW_CONTACT
------------	INNER JOIN TB_REVIEW on TB_REVIEW_CONTACT.REVIEW_ID = TB_REVIEW.REVIEW_ID
------------	INNER JOIN TB_CONTACT c on TB_REVIEW_CONTACT.CONTACT_ID = c.CONTACT_ID
------------	INNER JOIN TB_CONTACT_REVIEW_ROLE on TB_CONTACT_REVIEW_ROLE.REVIEW_CONTACT_ID = TB_REVIEW_CONTACT.REVIEW_CONTACT_ID
------------	Left outer join TB_SITE_LIC_CONTACT lc on lc.CONTACT_ID = c.CONTACT_ID
------------	Left outer join TB_SITE_LIC sl on lc.SITE_LIC_ID = sl.SITE_LIC_ID
------------	left outer join TB_SITE_LIC_REVIEW lr on TB_REVIEW.REVIEW_ID = lr.REVIEW_ID
------------	left outer join TB_SITE_LIC sl2 on lr.SITE_LIC_ID = sl2.SITE_LIC_ID
	
------------WHERE TB_REVIEW.REVIEW_ID = @reviewId AND c.CONTACT_ID = @userId

------------IF @@ROWCOUNT >= 1 
------------	BEGIN
------------	DECLARE	@return_value int,
------------			@GUID uniqueidentifier
			
------------	EXEC	@return_value = [ReviewerAdmin].[dbo].[st_LogonTicket_Insert]
------------			@Contact_ID = @userId,
------------			@Review_ID = @reviewId,
------------			@GUID = @GUI OUTPUT
------------	SELECT	@GUI as N'@GUID'
------------	END

------------GO

------------USE [Reviewer]
------------GO
------------/****** Object:  StoredProcedure [dbo].[st_TrainingSetScreeningCodeSet]    Script Date: 01/09/2014 17:04:56 ******/
------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------CREATE procedure [dbo].[st_TrainingSetScreeningCodeSet]
------------(
------------	@REVIEW_ID INT,
------------	@CODE_SET_ID INT
------------)

------------As


------------SET NOCOUNT ON

------------UPDATE TB_REVIEW
------------SET SCREENING_CODE_SET_ID = @CODE_SET_ID
------------WHERE REVIEW_ID = @REVIEW_ID

------------SET NOCOUNT OFF

------------GO

------------USE [Reviewer]
------------GO
------------/****** Object:  StoredProcedure [dbo].[st_SVMReviewData]    Script Date: 01/11/2014 11:50:47 ******/
------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------ALTER procedure [dbo].[st_SVMReviewData]
------------(
------------	@REVIEW_ID INT
------------,	@SCREENING_CODE_SET_ID INT = NULL
------------)

------------As

------------SET NOCOUNT ON




------------	SELECT DISTINCT '+1', TB_ITEM_TERM_VECTORS.ITEM_ID, VECTORS FROM ReviewerTerms.dbo.TB_ITEM_TERM_VECTORS
------------	INNER JOIN REVIEWER.DBO.TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
------------	INNER JOIN REVIEWER.DBO.TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
------------		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
------------	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
------------		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
------------	INNER JOIN Reviewer.dbo.TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
------------	WHERE tb_item_term_vectors.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		
------------	UNION
	
------------	SELECT DISTINCT '-1', ITEM_ID, VECTOR_S FROM (SELECT /* TOP(@N_INCLUDED) */ VECTORS
------------		AS VECTOR_S, TB_ITEM_TERM_VECTORS.ITEM_ID FROM ReviewerTerms.dbo.TB_ITEM_TERM_VECTORS
------------	INNER JOIN REVIEWER.DBO.TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
------------	INNER JOIN REVIEWER.DBO.TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
------------		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
------------	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
------------		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
------------	INNER JOIN Reviewer.dbo.TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
------------	WHERE TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
------------	/*ORDER BY NEWID()*/) AS X
	
------------	UNION
	
------------	SELECT DISTINCT '0', TB_ITEM_TERM_VECTORS.ITEM_ID, VECTORS FROM ReviewerTerms.dbo.TB_ITEM_TERM_VECTORS
------------	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
------------		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
------------		AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
------------		WHERE TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID AND NOT TB_ITEM_TERM_VECTORS.ITEM_ID IN
------------	(SELECT TB_ITEM_SET.ITEM_ID FROM TB_ITEM_SET
------------		WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE'
------------		AND TB_ITEM_SET.SET_ID = @SCREENING_CODE_SET_ID
------------	)
	
------------	/*
------------	-- OLD VERSION, BEFORE WE SIMPLY FILTER BY WHETHER A CODE IS PRESENT IN A GIVEN CODE SET
	
------------	SELECT DISTINCT '0', TB_ITEM_TERM_VECTORS.ITEM_ID, VECTORS FROM ReviewerTerms.dbo.TB_ITEM_TERM_VECTORS
------------	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
------------		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
------------		AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'
------------		AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
------------		WHERE TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID AND NOT TB_ITEM_TERM_VECTORS.ITEM_ID IN
------------	(SELECT TB_ITEM_TERM_VECTORS.ITEM_ID FROM ReviewerTerms.dbo.TB_ITEM_TERM_VECTORS
------------	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
------------	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
------------		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
------------	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
------------		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
	
------------	WHERE TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID)
	
------------	*/
	
------------	--select @N_INCLUDED

------------SET NOCOUNT OFF

------------GO

------------USE [Reviewer]
------------GO
------------/****** Object:  StoredProcedure [dbo].[st_ContactAdminLoginReview]    Script Date: 01/29/2014 10:29:31 ******/
------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO

------------ALTER procedure [dbo].[st_ContactAdminLoginReview]
------------(
------------	@userId int,
------------	@reviewId int,
------------	@GUI uniqueidentifier OUTPUT
------------)

------------As
------------declare @chk int
------------set @chk = (select count (CONTACT_ID) from TB_CONTACT where IS_SITE_ADMIN = 1 and CONTACT_ID = @userId)
------------if @chk != 1
------------begin
------------	RETURN --do nothing if user is not actually a site admin
------------end

------------SELECT  TB_REVIEW.REVIEW_ID, 'AdminUser' as [ROLE], 
------------		( CASE WHEN sl2.[EXPIRY_DATE] is not null
------------				and sl2.[EXPIRY_DATE] > TB_REVIEW.[EXPIRY_DATE]
------------					then sl2.[EXPIRY_DATE]
------------				else TB_REVIEW.[EXPIRY_DATE]
------------				end
------------		) as REVIEW_EXP, 
------------		(SELECT  CASE when sl.[EXPIRY_DATE] is not null 
------------				and sl.[EXPIRY_DATE] > c.[EXPIRY_DATE]
------------					then sl.[EXPIRY_DATE]
------------				else c.[EXPIRY_DATE]
------------				end
------------				from TB_CONTACT c 
------------				Left outer join TB_SITE_LIC_CONTACT lc on lc.CONTACT_ID = c.CONTACT_ID
------------				Left outer join TB_SITE_LIC sl on lc.SITE_LIC_ID = sl.SITE_LIC_ID
------------				where c.CONTACT_ID = @userId
------------		) as CONTACT_EXP,
------------		FUNDER_ID
		
------------FROM TB_REVIEW 
------------	left outer join TB_SITE_LIC_REVIEW lr on TB_REVIEW.REVIEW_ID = lr.REVIEW_ID
------------	left outer join TB_SITE_LIC sl2 on lr.SITE_LIC_ID = sl2.SITE_LIC_ID
	
------------WHERE TB_REVIEW.REVIEW_ID = @reviewId 

------------IF @@ROWCOUNT >= 1 
------------	BEGIN
------------	DECLARE	@return_value int,
------------			@GUID uniqueidentifier
			
------------	EXEC	@return_value = [ReviewerAdmin].[dbo].[st_LogonTicket_Insert]
------------			@Contact_ID = @userId,
------------			@Review_ID = @reviewId,
------------			@GUID = @GUI OUTPUT
------------	SELECT	@GUI as N'@GUID'
------------	END

------------GO







