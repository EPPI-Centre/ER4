USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisUpdate]    Script Date: 18/05/2023 13:06:22 ******/
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

	@SORTED_FIELD varchar(21) =  NULL,
	@SORT_DIRECTION bit =  NULL,

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
	,   SORTED_FIELD = @SORTED_FIELD 
	,   SORT_DIRECTION = @SORT_DIRECTION

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
Go
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisInsert]    Script Date: 18/05/2023 13:05:53 ******/
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
	@CertaintyLevelComment [ntext] =  NULL

	, @SORTED_FIELD varchar(21)
	, @SORT_DIRECTION bit = null

	, @ATTRIBUTE_ANSWER_TEXT NVARCHAR(MAX) OUTPUT
	, @ATTRIBUTE_QUESTION_TEXT NVARCHAR(MAX) OUTPUT
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

	, SORTED_FIELD 
	, SORT_DIRECTION
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

	, @SORTED_FIELD
	, @SORT_DIRECTION
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
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysis]    Script Date: 18/05/2023 12:26:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_MetaAnalysis]
(
	@META_ANALYSIS_ID INT
	, @REVIEW_ID INT
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

	 SORTED_FIELD, SORT_DIRECTION,

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
	WHERE REVIEW_ID = @REVIEW_ID and META_ANALYSIS_ID = @META_ANALYSIS_ID

SET NOCOUNT OFF

GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisList]    Script Date: 18/05/2023 12:25:38 ******/
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

	 SORTED_FIELD, SORT_DIRECTION,

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
GO