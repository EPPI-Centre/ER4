USE Reviewer
GO
IF COL_LENGTH('dbo.TB_META_ANALYSIS', 'ATTRIBUTE_ID_OUTCOME_ANSW') IS NULL
BEGIN
	Alter TABLE TB_META_ANALYSIS ADD ATTRIBUTE_ID_OUTCOME_ANSW varchar(4000) NULL DEFAULT(NULL);
	Alter TABLE TB_META_ANALYSIS ADD ATTRIBUTE_ID_OUTCOME_QST varchar(4000) NULL DEFAULT(NULL);
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_OutcomeList]    Script Date: 12/02/2026 09:49:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_OutcomeList]
(
	@REVIEW_ID INT,
	@META_ANALYSIS_ID INT,
	@QUESTIONS nvarchar(max) = '',
	@ANSWERS nvarchar(max) = '',
	@QUESTIONS_O varchar(4000) = '',
	@ANSWERS_O varchar(4000) = ''
)

As

SET NOCOUNT ON
	Declare @IOs table (OUTCOME_ID int, ITEM_ID bigint, ITEM_SET_ID bigint, OUTCOME_TITLE nvarchar(255))
	insert into @IOs select distinct OUTCOME_ID, ir.ITEM_ID, tis.ITEM_SET_ID, OUTCOME_TITLE
	from TB_ITEM_OUTCOME tio
	inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
	inner join TB_REVIEW_SET rs on tis.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID --stop outcomes for coming across if they belong to a deleted codeset
	inner join TB_ITEM_REVIEW ir on tis.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID


	SELECT distinct tio.OUTCOME_ID, ios.ITEM_ID, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, ios.OUTCOME_TITLE, SHORT_TITLE, OUTCOME_DESCRIPTION,
	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
	TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_OUTCOME_ID, ios.ITEM_SET_ID, TB_ITEM_ATTRIBUTE.ITEM_ID,
	A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT, A3.ATTRIBUTE_NAME OUTCOME_TEXT
	,	tio.ITEM_TIMEPOINT_ID
	,	tio.ITEM_ARM_ID_GRP1
	,	tio.ITEM_ARM_ID_GRP2
	,	CONCAT(TB_ITEM_TIMEPOINT.TIMEPOINT_VALUE, ' ', TB_ITEM_TIMEPOINT.TIMEPOINT_METRIC) TimepointDisplayValue
	,	arm1.ARM_NAME grp1ArmName
	,	arm2.ARM_NAME grp2ArmName
	FROM @IOs as ios
	inner join TB_ITEM_OUTCOME tio on ios.OUTCOME_ID = tio.OUTCOME_ID
	--inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tio.ITEM_SET_ID
	--	AND tis.IS_COMPLETED = 1
	--inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @REVIEW_ID and rs.SET_ID = tis.SET_ID
	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = ios.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = ios.ITEM_SET_ID
	inner join TB_ITEM on TB_ITEM.ITEM_ID = ios.ITEM_ID
	LEFT OUTER JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID
	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_INTERVENTION 
	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_CONTROL
	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_OUTCOME
	left outer join TB_ITEM_TIMEPOINT ON TB_ITEM_TIMEPOINT.ITEM_TIMEPOINT_ID = tio.ITEM_TIMEPOINT_ID
	left outer join TB_ITEM_ARM arm1 ON arm1.ITEM_ARM_ID = tio.ITEM_ARM_ID_GRP1
	left outer join TB_ITEM_ARM arm2 on arm2.ITEM_ARM_ID = tio.ITEM_ARM_ID_GRP2

	IF (@QUESTIONS is not null AND @QUESTIONS != '')
	BEGIN
		--second set of results, the answers as applied to the whole item
		--we need to get these, even if empty, so that we always get a reader
		declare @QT table ( AttID bigint primary key)
		insert into @QT select qss.value from dbo.fn_Split_int(@QUESTIONS, ',') as qss
		select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID
			, ATTRIBUTE_NAME as Codename
			, a.ATTRIBUTE_ID as ATTRIBUTE_ID
			, tas.PARENT_ATTRIBUTE_ID
		from @IOs tio
		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
		inner join TB_REVIEW_SET rs on tis.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_REVIEW ir ON IR.ITEM_ID = TIS.ITEM_ID AND IR.REVIEW_ID = @REVIEW_ID

		inner join TB_ITEM_SET tis2 on tis.ITEM_ID = tis2.ITEM_ID
		inner join TB_ITEM_ATTRIBUTE tia on tis2.ITEM_SET_ID = tia.ITEM_SET_ID
		inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
		inner join @QT Qs on Qs.AttID = tas.PARENT_ATTRIBUTE_ID
		inner join TB_ATTRIBUTE a on tas.ATTRIBUTE_ID = a.ATTRIBUTE_ID
		order by OUTCOME_ID, tas.PARENT_ATTRIBUTE_ID, tas.ATTRIBUTE_ORDER, a.ATTRIBUTE_ID
	END
	ELSE
	BEGIN
	select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID,
		tio.OUTCOME_TITLE as Codename
		, tio.ITEM_ATTRIBUTE_ID_CONTROL as ATTRIBUTE_ID, tio.ITEM_ATTRIBUTE_ID_CONTROL as PARENT_ATTRIBUTE_ID
		from TB_ITEM_OUTCOME tio
		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and 1=0
	END


	IF (@ANSWERS is not null AND @ANSWERS != '')
	BEGIN
		--third set of results, the questions as applied to the whole item
		declare @AT table ( AttID bigint primary key)
		insert into @AT select qss.value from dbo.fn_Split_int(@ANSWERS, ',') as qss
		
		select distinct tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 		
			tia.ATTRIBUTE_ID, a.ATTRIBUTE_NAME
		from @IOs tio 
		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
		inner join TB_REVIEW_SET rs on tis.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_REVIEW ir ON IR.ITEM_ID = TIS.ITEM_ID AND IR.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_SET tis2 on tis.ITEM_ID = tis2.ITEM_ID and tis2.IS_COMPLETED = 1
		inner join TB_ATTRIBUTE_SET tas on tis2.SET_ID = tas.SET_ID -- and tas.SET_ID = rs.SET_ID
		inner join TB_ITEM_ATTRIBUTE tia on tis2.ITEM_ID = tia.ITEM_ID and tas.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
		inner join @AT on AttID = tia.ATTRIBUTE_ID
		inner join TB_ATTRIBUTE a on tia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
		order by OUTCOME_ID
	END
	ELSE
	BEGIN
		select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
		tio.ITEM_ATTRIBUTE_ID_CONTROL as ATTRIBUTE_ID, tio.OUTCOME_TITLE as ATTRIBUTE_NAME
		from TB_ITEM_OUTCOME tio inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and 1=0
	
	END
	
	IF (@QUESTIONS_O is not null AND @QUESTIONS_O != '')
	BEGIN
		--4th set of results, the answers as applied to the actual outcome
		--we need to get these, even if empty, so that we always get a reader
		declare @QTO table ( AttID bigint primary key)
		insert into @QTO select qss.value from dbo.fn_Split_int(@QUESTIONS_O, ',') as qss

		select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID 
			, ATTRIBUTE_NAME as Codename
			, a.ATTRIBUTE_ID as ATTRIBUTE_ID
			, tas.PARENT_ATTRIBUTE_ID
		from @IOs tio 
		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
		inner join TB_REVIEW_SET rs on tis.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_REVIEW ir ON IR.ITEM_ID = TIS.ITEM_ID AND IR.REVIEW_ID = @REVIEW_ID

		inner join TB_ITEM_OUTCOME_ATTRIBUTE oa on tio.OUTCOME_ID = oa.OUTCOME_ID
		inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = oa.ATTRIBUTE_ID
		inner join @QTO Qs on Qs.AttID = tas.PARENT_ATTRIBUTE_ID
		inner join TB_ATTRIBUTE a on tas.ATTRIBUTE_ID = a.ATTRIBUTE_ID
		order by OUTCOME_ID, tas.PARENT_ATTRIBUTE_ID, tas.ATTRIBUTE_ORDER, a.ATTRIBUTE_ID
	END
	ELSE
	BEGIN
	select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
		tio.OUTCOME_TITLE as Codename
		, tio.ITEM_ATTRIBUTE_ID_CONTROL as ATTRIBUTE_ID, tio.ITEM_ATTRIBUTE_ID_CONTROL as PARENT_ATTRIBUTE_ID
		from TB_ITEM_OUTCOME tio
		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and 1=0
	END


	IF (@ANSWERS_O is not null AND @ANSWERS_O != '')
	BEGIN
	--5th set of results, the questions as applied to the actual outcome
		declare @ATO table ( AttID bigint primary key)
		insert into @ATO select qss.value from dbo.fn_Split_int(@ANSWERS_O, ',') as qss
	
		select distinct tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 		
			a.ATTRIBUTE_ID, a.ATTRIBUTE_NAME
		from @IOs tio 
		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
		inner join TB_REVIEW_SET rs on tis.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_REVIEW ir ON IR.ITEM_ID = TIS.ITEM_ID AND IR.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_OUTCOME_ATTRIBUTE oa on tio.OUTCOME_ID = oa.OUTCOME_ID
		inner join @ATO on AttID = oa.ATTRIBUTE_ID
		inner join TB_ATTRIBUTE a on oa.ATTRIBUTE_ID = a.ATTRIBUTE_ID
		order by OUTCOME_ID
	END
	ELSE
	BEGIN
		select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
		tio.ITEM_ATTRIBUTE_ID_CONTROL as ATTRIBUTE_ID, tio.OUTCOME_TITLE as ATTRIBUTE_NAME
		from TB_ITEM_OUTCOME tio inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and 1=0
	
	END



SET NOCOUNT OFF
GO

/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisInsert]    Script Date: 12/02/2026 11:13:17 ******/
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
	@ATTRIBUTE_ID_OUTCOME_ANSW varchar(4000) = '',
	@ATTRIBUTE_ID_OUTCOME_QST varchar(4000) = '',

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
	, @ATTRIBUTE_ANSWER_TEXT_O NVARCHAR(MAX) OUTPUT
	, @ATTRIBUTE_QUESTION_TEXT_O NVARCHAR(MAX) OUTPUT
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
	ATTRIBUTE_ID_OUTCOME_ANSW,
	ATTRIBUTE_ID_OUTCOME_QST,
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
	,@CONTACT_ID
	,@REVIEW_ID
	,@ATTRIBUTE_ID
	,@SET_ID
	,@ATTRIBUTE_ID_INTERVENTION
	,@ATTRIBUTE_ID_CONTROL
	,@ATTRIBUTE_ID_OUTCOME
	,@META_ANALYSIS_TYPE_ID
	,@ATTRIBUTE_ID_ANSWER
	,@ATTRIBUTE_ID_QUESTION
	,@GRID_SETTINGS,

	@ATTRIBUTE_ID_OUTCOME_ANSW,
	@ATTRIBUTE_ID_OUTCOME_QST,

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
	
	SELECT @ATTRIBUTE_ANSWER_TEXT_O = COALESCE(@ATTRIBUTE_ANSWER_TEXT_O + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_OUTCOME_ANSW, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL

	SELECT @ATTRIBUTE_QUESTION_TEXT = COALESCE(@ATTRIBUTE_QUESTION_TEXT_O + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_OUTCOME_QST, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL

SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisUpdate]    Script Date: 12/02/2026 11:26:36 ******/
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
	@ATTRIBUTE_ID_OUTCOME_ANSW varchar(4000) = '',
	@ATTRIBUTE_ID_OUTCOME_QST varchar(4000) = '',

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
	@ATTRIBUTE_QUESTION_TEXT NVARCHAR(MAX) OUTPUT,
	@ATTRIBUTE_ANSWER_TEXT_O NVARCHAR(MAX) OUTPUT,
	@ATTRIBUTE_QUESTION_TEXT_O NVARCHAR(MAX) OUTPUT
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
	,	ATTRIBUTE_ID_OUTCOME_ANSW  = @ATTRIBUTE_ID_OUTCOME_ANSW
	,	ATTRIBUTE_ID_OUTCOME_QST   = @ATTRIBUTE_ID_OUTCOME_QST 
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

	SELECT @ATTRIBUTE_ANSWER_TEXT_O = COALESCE(@ATTRIBUTE_ANSWER_TEXT_O + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_OUTCOME_ANSW, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL

	SELECT @ATTRIBUTE_QUESTION_TEXT_O = COALESCE(@ATTRIBUTE_QUESTION_TEXT_O + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_OUTCOME_QST, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL

SET NOCOUNT OFF
GO


/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisList]    Script Date: 12/02/2026 11:21:40 ******/
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
	ATTRIBUTE_ID_OUTCOME_ANSW, ATTRIBUTE_ID_OUTCOME_QST,
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
		FOR XML PATH('')) AS ATTRIBUTE_QUESTION_TEXT,

	(SELECT ATTRIBUTE_NAME + '¬' FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_OUTCOME_ANSW, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		FOR XML PATH('')) AS ATTRIBUTE_ANSWER_TEXT_O,

	(SELECT ATTRIBUTE_NAME + '¬' FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_OUTCOME_QST, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		FOR XML PATH('')) AS ATTRIBUTE_QUESTION_TEXT_O
	
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

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysis]    Script Date: 12/02/2026 15:54:06 ******/
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
	ATTRIBUTE_ID_ANSWER, ATTRIBUTE_ID_QUESTION,	ATTRIBUTE_ID_OUTCOME_ANSW, ATTRIBUTE_ID_OUTCOME_QST,	
	GRID_SETTINGS,
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
		FOR XML PATH('')) AS ATTRIBUTE_QUESTION_TEXT,
	
	(SELECT ATTRIBUTE_NAME + '¬' FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_OUTCOME_ANSW, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		FOR XML PATH('')) AS ATTRIBUTE_ANSWER_TEXT_O,

	(SELECT ATTRIBUTE_NAME + '¬' FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_OUTCOME_QST, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		FOR XML PATH('')) AS ATTRIBUTE_QUESTION_TEXT_O
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
ALTER   PROCEDURE [dbo].[st_ReportAllCodingCommand] (
        @ReviewId int
		,@SetId int
)
AS
BEGIN
	SET NOCOUNT ON


	--declare @ReviewId int = 99 --7
	--	 ,@SetId int = 894 --664 --27 --1851

	declare @fa table (a_id bigint, p_id bigint, done bit, a_order int, a_name nvarchar(500), full_path nvarchar(max) null, level int null)
	declare @items table (ItemId bigint, ItemSet int, ContactId int, ContactName varchar(255), Completed bit, [State] varchar(25), primary key(ItemId, ItemSet))

	insert into @fa (a_id, p_id, done, a_order, a_name)
		Select a.ATTRIBUTE_ID, tas.PARENT_ATTRIBUTE_ID, 0, tas.ATTRIBUTE_ORDER, a.ATTRIBUTE_NAME from TB_ATTRIBUTE a
			inner join TB_ATTRIBUTE_SET tas on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and dbo.fn_IsAttributeInTree(a.ATTRIBUTE_ID) = 1
			inner join TB_REVIEW_SET rs on tas.SET_ID = rs.SET_ID and rs.SET_ID = @SetId


	update f set done = 1, full_path =  f.a_name
	from @fa f
	 where done = 0  and p_id = 0

	--select * from @fa order by p_id, a_order
	declare @levind int = 0
	declare @todoLines int = (select count(*) from @fa where done=0)
	while (@todoLines > 0 AND @levind < 20)
	BEGIN
		set @levind = @levind + 1
		update f1 
		set done = 1, full_path = f2.full_path + '\' + f1.a_name , level = @levind + 1
		from @fa f1 
			inner join @fa f2 on f1.p_id = f2.a_id and f2.done = 1
		where f1.done = 0

		set @todoLines = (select count(*) from @fa where done=0) 
	END
	update @fa set level = 1 where level is null and done = 1
	select * from @fa order by [level], p_id, a_order

	insert into @items SELECT distinct tis.item_id, tis.ITEM_SET_ID, tis.CONTACT_ID, c.CONTACT_NAME, tis.IS_COMPLETED 
	,case 
		when ir.IS_INCLUDED = 1 and ir.IS_DELETED = 0 then '(I) Included'
		when ir.IS_INCLUDED = 0 and ir.IS_DELETED = 0 then '(E) Excluded'
		when ir.IS_INCLUDED = 1 and ir.IS_DELETED = 1 and ir.MASTER_ITEM_ID is not null then '(S) Duplicate'
		when ir.IS_INCLUDED = 1 and ir.IS_DELETED = 1 then '(S) In deleted source'
		when ir.IS_INCLUDED = 0 and ir.IS_DELETED = 1 then '(D) Deleted'
		else ''
	end AS [STATE]
	from tb_item_set tis
		inner join TB_ITEM_REVIEW ir on tis.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @ReviewId and tis.SET_ID = @SetId
		inner join TB_CONTACT c on tis.CONTACT_ID = c.CONTACT_ID

	select i.*, tia.ATTRIBUTE_ID, tia.ITEM_ATTRIBUTE_ID, tia.ITEM_ARM_ID, fa.a_name, arm.ARM_NAME, ii.SHORT_TITLE, ii.TITLE, tia.ADDITIONAL_TEXT from @items i 
		inner join TB_ITEM_ATTRIBUTE tia on i.ItemId = tia.ITEM_ID and i.ItemSet = tia.ITEM_SET_ID
		inner join @fa fa on tia.ATTRIBUTE_ID = fa.a_id
		inner join TB_ITEM ii on i.ItemId = ii.ITEM_ID
		left join TB_ITEM_ARM arm on arm.ITEM_ARM_ID = tia.ITEM_ARM_ID 
		order by ii.SHORT_TITLE, I.ItemId, i.ContactId, level, p_id, a_order

	select i.*, tia.ATTRIBUTE_ID, tia.ITEM_ATTRIBUTE_ID, p.PAGE, replace( SELECTION_TEXTS, '¬', '<br />') as [TEXT], d.DOCUMENT_TITLE from @items i 
		inner join TB_ITEM_ATTRIBUTE tia on i.ItemId = tia.ITEM_ID and i.ItemSet = tia.ITEM_SET_ID
		inner join TB_ITEM_ATTRIBUTE_PDF p on tia.ITEM_ATTRIBUTE_ID = p.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT d on p.ITEM_DOCUMENT_ID = d.ITEM_DOCUMENT_ID
	

	select TB_ITEM.ITEM_ID, i.Completed, i.ContactName, i.ContactId, OUTCOME_ID, SHORT_TITLE, TB_ITEM_OUTCOME.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
			ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, OUTCOME_DESCRIPTION,
			DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
			A1.ATTRIBUTE_NAME AS INTERVENTION_TEXT,
			a2.ATTRIBUTE_NAME AS CONTROL_TEXT,
			a3.ATTRIBUTE_NAME AS OUTCOME_TEXT,
			0 as META_ANALYSIS_OUTCOME_ID -- Meta-analysis id. 0 as not selected
		,	TB_ITEM_OUTCOME.ITEM_TIMEPOINT_ID
		,	TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP1
		,	TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP2
		,	CONCAT(TB_ITEM_TIMEPOINT.TIMEPOINT_VALUE, ' ', TB_ITEM_TIMEPOINT.TIMEPOINT_METRIC) TimepointDisplayValue
		,	arm1.ARM_NAME grp1ArmName
		,	arm2.ARM_NAME grp2ArmName
		FROM @items i
		inner join TB_ITEM_OUTCOME on i.ItemSet = TB_ITEM_OUTCOME.ITEM_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_OUTCOME.ITEM_SET_ID
		INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
		left outer JOIN TB_ATTRIBUTE IA1 ON IA1.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_INTERVENTION
		left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = IA1.ATTRIBUTE_ID 
		left outer JOIN TB_ATTRIBUTE IA2 ON IA2.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_CONTROL
		left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
		left outer JOIN TB_ATTRIBUTE IA3 ON IA3.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_OUTCOME
		left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = IA3.ATTRIBUTE_ID 
		left outer join TB_ITEM_TIMEPOINT ON TB_ITEM_TIMEPOINT.ITEM_TIMEPOINT_ID = TB_ITEM_OUTCOME.ITEM_TIMEPOINT_ID
		left outer join TB_ITEM_ARM arm1 ON arm1.ITEM_ARM_ID = TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP1
		left outer join TB_ITEM_ARM arm2 on arm2.ITEM_ARM_ID = TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP2
	
END
GO
ALTER procedure [dbo].[st_OutcomeItemList]
(
	@REVIEW_ID INT,
	@ITEM_SET_ID BIGINT
)

As

SET NOCOUNT ON

SELECT OUTCOME_ID, TB_ITEM.ITEM_ID, SHORT_TITLE, TB_ITEM_OUTCOME.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, OUTCOME_DESCRIPTION,
	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
	A1.ATTRIBUTE_NAME AS INTERVENTION_TEXT,
	a2.ATTRIBUTE_NAME AS CONTROL_TEXT,
	a3.ATTRIBUTE_NAME AS OUTCOME_TEXT,
	0 as META_ANALYSIS_OUTCOME_ID -- Meta-analysis id. 0 as not selected
,	TB_ITEM_OUTCOME.ITEM_TIMEPOINT_ID
,	TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP1
,	TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP2
,	CONCAT(TB_ITEM_TIMEPOINT.TIMEPOINT_VALUE, ' ', TB_ITEM_TIMEPOINT.TIMEPOINT_METRIC) TimepointDisplayValue
,	arm1.ARM_NAME grp1ArmName
,	arm2.ARM_NAME grp2ArmName
FROM TB_ITEM_OUTCOME
INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_OUTCOME.ITEM_SET_ID
INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
left outer JOIN TB_ATTRIBUTE IA1 ON IA1.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_INTERVENTION
left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = IA1.ATTRIBUTE_ID 
left outer JOIN TB_ATTRIBUTE IA2 ON IA2.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_CONTROL
left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
left outer JOIN TB_ATTRIBUTE IA3 ON IA3.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_OUTCOME
left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = IA3.ATTRIBUTE_ID 
left outer join TB_ITEM_TIMEPOINT ON TB_ITEM_TIMEPOINT.ITEM_TIMEPOINT_ID = TB_ITEM_OUTCOME.ITEM_TIMEPOINT_ID
left outer join TB_ITEM_ARM arm1 ON arm1.ITEM_ARM_ID = TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP1
left outer join TB_ITEM_ARM arm2 on arm2.ITEM_ARM_ID = TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP2

WHERE TB_ITEM_OUTCOME.ITEM_SET_ID = @ITEM_SET_ID

SET NOCOUNT OFF
GO

GO