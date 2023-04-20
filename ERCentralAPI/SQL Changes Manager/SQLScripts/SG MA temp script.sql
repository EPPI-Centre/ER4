USE [Reviewer]
GO


GO

/****** Object:  Table [dbo].[TB_META_ANALYSIS_FILTER_SETTING]    Script Date: 14/04/2023 11:02:41 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_META_ANALYSIS_FILTER_SETTING]') AND type in (N'U'))
BEGIN
	ALTER TABLE [dbo].[TB_META_ANALYSIS_FILTER_SETTING] DROP CONSTRAINT [FK_TB_META_ANALYSIS_FILTER_SETTING_TB_META_ANALYSIS]
	DROP TABLE [dbo].[TB_META_ANALYSIS_FILTER_SETTING]
END
GO

/****** Object:  Table [dbo].[TB_META_ANALYSIS_FILTER_SETTING]    Script Date: 14/04/2023 11:02:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TB_META_ANALYSIS_FILTER_SETTING](
	[META_ANALYSIS_FILTER_SETTING_ID] [int] IDENTITY(1,1) NOT NULL,
	[META_ANALYSIS_ID] [int] NOT NULL,
	[COLUMN_NAME] [varchar](20) NOT NULL,
	[SELECTED_VALUES] [nvarchar](500) NULL,
	[FILTER_1_VALUE] [nvarchar](500) NULL,
	[FILTER_1_OPERATOR] [varchar](30) NULL,
	[FILTER_1_CASE_SENSITIVE] [bit] NULL,
	[FIELD_FILTER_LOGICAL_OPERATOR] [varchar](3) NULL,
	[FILTER_2_VALUE] [nvarchar](500) NULL,
	[FILTER_2_OPERATOR] [varchar](30) NULL,
	[FILTER_2_CASE_SENSITIVE] [bit] NULL,
 CONSTRAINT [PK_TB_META_ANALYSIS_FILTER_SETTING] PRIMARY KEY CLUSTERED 
(
	[META_ANALYSIS_FILTER_SETTING_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TB_META_ANALYSIS_FILTER_SETTING]  WITH CHECK ADD  CONSTRAINT [FK_TB_META_ANALYSIS_FILTER_SETTING_TB_META_ANALYSIS] FOREIGN KEY([META_ANALYSIS_ID])
REFERENCES [dbo].[TB_META_ANALYSIS] ([META_ANALYSIS_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[TB_META_ANALYSIS_FILTER_SETTING] CHECK CONSTRAINT [FK_TB_META_ANALYSIS_FILTER_SETTING_TB_META_ANALYSIS]
GO

/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisFilterSettings]    Script Date: 17/04/2023 13:37:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_MetaAnalysisFilterSettings]
(
	@REVIEW_ID INT,
	@META_ANALYSIS_ID INT
)

As

SET NOCOUNT ON

	SELECT FS.* from 
	TB_META_ANALYSIS ma
	inner join TB_META_ANALYSIS_FILTER_SETTING FS on FS.META_ANALYSIS_ID = MA.META_ANALYSIS_ID and MA.META_ANALYSIS_ID = @META_ANALYSIS_ID and MA.REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF
GO

/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisFilterSettingCreate]    Script Date: 17/04/2023 13:37:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_MetaAnalysisFilterSettingCreate]
(
	@REVIEW_ID INT
	,@META_ANALYSIS_ID INT
	,@COLUMN_NAME varchar(20)
	,@SELECTED_VALUES nvarchar(500)
	,@FILTER_1_VALUE nvarchar(500)
	,@FILTER_1_OPERATOR varchar(30)
	,@FILTER_1_CASE_SENSITIVE bit = 0
	,@FIELD_FILTER_LOGICAL_OPERATOR varchar(3)
	,@FILTER_2_VALUE nvarchar(500)
	,@FILTER_2_OPERATOR varchar(30)
	,@FILTER_2_CASE_SENSITIVE bit = 0

	,@META_ANALYSIS_FILTER_SETTING_ID INT = null OUTPUT 
)

As

SET NOCOUNT ON

	declare @check int = (select count(*) from TB_META_ANALYSIS where REVIEW_ID = @REVIEW_ID and META_ANALYSIS_ID = @META_ANALYSIS_ID)
	if @check is null and @check != 1
	BEGIN
		set @META_ANALYSIS_FILTER_SETTING_ID = -1
		return
	END
	INSERT INTO TB_META_ANALYSIS_FILTER_SETTING
           (META_ANALYSIS_ID
           ,COLUMN_NAME
           ,SELECTED_VALUES
           ,FILTER_1_VALUE
           ,FILTER_1_OPERATOR
           ,FILTER_1_CASE_SENSITIVE
           ,FIELD_FILTER_LOGICAL_OPERATOR
           ,FILTER_2_VALUE
           ,FILTER_2_OPERATOR
           ,FILTER_2_CASE_SENSITIVE)
     VALUES
           (@META_ANALYSIS_ID
			,@COLUMN_NAME
			,@SELECTED_VALUES
			,@FILTER_1_VALUE
			,@FILTER_1_OPERATOR
			,@FILTER_1_CASE_SENSITIVE
			,@FIELD_FILTER_LOGICAL_OPERATOR
			,@FILTER_2_VALUE
			,@FILTER_2_OPERATOR 
			,@FILTER_2_CASE_SENSITIVE)
	set @META_ANALYSIS_FILTER_SETTING_ID = CAST(SCOPE_IDENTITY() AS INT)
SET NOCOUNT OFF
GO

/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisFilterSettingUpdate]    Script Date: 17/04/2023 13:37:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_MetaAnalysisFilterSettingUpdate]
(
	@REVIEW_ID INT
	,@META_ANALYSIS_FILTER_SETTING_ID INT 
	,@META_ANALYSIS_ID INT
	,@COLUMN_NAME varchar(20)
	,@SELECTED_VALUES nvarchar(500)
	,@FILTER_1_VALUE nvarchar(500)
	,@FILTER_1_OPERATOR varchar(30)
	,@FILTER_1_CASE_SENSITIVE bit = 0
	,@FIELD_FILTER_LOGICAL_OPERATOR varchar(3)
	,@FILTER_2_VALUE nvarchar(500)
	,@FILTER_2_OPERATOR varchar(30)
	,@FILTER_2_CASE_SENSITIVE bit = 0
)

As

SET NOCOUNT ON

	declare @check int = (select count(*) from TB_META_ANALYSIS where REVIEW_ID = @REVIEW_ID and META_ANALYSIS_ID = @META_ANALYSIS_ID)
	if @check is null and @check != 1 return

	UPDATE TB_META_ANALYSIS_FILTER_SETTING
	SET
           COLUMN_NAME = @COLUMN_NAME
           ,SELECTED_VALUES = @SELECTED_VALUES
           ,FILTER_1_VALUE = @FILTER_1_VALUE
           ,FILTER_1_OPERATOR = @FILTER_1_OPERATOR
           ,FILTER_1_CASE_SENSITIVE = @FILTER_1_CASE_SENSITIVE
           ,FIELD_FILTER_LOGICAL_OPERATOR = @FIELD_FILTER_LOGICAL_OPERATOR
           ,FILTER_2_VALUE = @FILTER_2_VALUE
           ,FILTER_2_OPERATOR = @FILTER_2_OPERATOR 
           ,FILTER_2_CASE_SENSITIVE = @FILTER_2_CASE_SENSITIVE
     where META_ANALYSIS_FILTER_SETTING_ID = @META_ANALYSIS_FILTER_SETTING_ID  and META_ANALYSIS_ID = @META_ANALYSIS_ID
			
SET NOCOUNT OFF
GO

/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisFilterSettingDelete]    Script Date: 17/04/2023 13:37:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_MetaAnalysisFilterSettingDelete]
(
	@REVIEW_ID INT
	,@META_ANALYSIS_FILTER_SETTING_ID INT 
	,@META_ANALYSIS_ID INT	
)

As

SET NOCOUNT ON

	declare @check int = (
			select count(*) from TB_META_ANALYSIS MA
				Inner Join TB_META_ANALYSIS_FILTER_SETTING FS on MA.META_ANALYSIS_ID = @META_ANALYSIS_ID and MA.META_ANALYSIS_ID = FS.META_ANALYSIS_ID
						AND MA.REVIEW_ID = @REVIEW_ID
						AND FS.META_ANALYSIS_FILTER_SETTING_ID = @META_ANALYSIS_FILTER_SETTING_ID
			)
	if @check is null and @check != 1 return

	delete from TB_META_ANALYSIS_FILTER_SETTING
     where META_ANALYSIS_FILTER_SETTING_ID = @META_ANALYSIS_FILTER_SETTING_ID  and META_ANALYSIS_ID = @META_ANALYSIS_ID
			
SET NOCOUNT OFF
GO

USE Reviewer
GO
IF COL_LENGTH('dbo.TB_META_ANALYSIS', 'SORTED_FIELD') IS NULL
BEGIN
	
		SET QUOTED_IDENTIFIER ON
		SET ARITHABORT ON
		SET NUMERIC_ROUNDABORT OFF
		SET CONCAT_NULL_YIELDS_NULL ON
		SET ANSI_NULLS ON
		SET ANSI_PADDING ON
		SET ANSI_WARNINGS ON
	

		ALTER TABLE dbo.TB_META_ANALYSIS ADD
			SORTED_FIELD varchar(20) NULL,
			SORT_DIRECTION bit NULL

		ALTER TABLE dbo.TB_META_ANALYSIS SET (LOCK_ESCALATION = TABLE)

	
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysis]    Script Date: 20/04/2023 08:49:06 ******/
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

	 RoB, RoBComment, RoBSequence, RoBConcealment, RoBBlindingParticipants, RoBBlindingAssessors, RoBIncomplete, RoBSelective, 
	 RoBNoIntention, RoBCarryover, RoBStopped, RoBUnvalidated, RoBOther, Incon, InconComment, InconPoint, InconCIs, InconDirection, 
	 InconStatistical, InconOther, Indirect, IndirectComment, IndirectPopulation, IndirectOutcome, IndirectNoDirect, IndirectIntervention, 
	 IndirectTime, IndirectOther, Imprec, ImprecComment, ImprecWide, ImprecFew, ImprecOnlyOne, ImprecOther, PubBias, PubBiasComment, 
	 PubBiasCommercially, PubBiasAsymmetrical, PubBiasLimited, PubBiasMissing, PubBiasDiscontinued, PubBiasDiscrepancy, PubBiasOther, 
	 UpgradeComment, UpgradeLarge, UpgradeVeryLarge, UpgradeAllPlausible, UpgradeClear, UpgradeNone, CertaintyLevel, CertaintyLevelComment
	 
	 ,SORTED_FIELD, SORT_DIRECTION

	FROM TB_META_ANALYSIS
	
	WHERE META_ANALYSIS_ID = @META_ANALYSIS_ID

SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisInsert]    Script Date: 20/04/2023 08:50:27 ******/
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

	, @SORTED_FIELD varchar(20)
	, @SORT_DIRECTION bit

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
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisList]    Script Date: 20/04/2023 08:53:31 ******/
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


/*

(SELECT COALESCE(ATTRIBUTE_ANSWER_TEXT + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_ANSWER, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL) ATTRIBUTE_ANSWER_TEXT

		*/

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisUpdate]    Script Date: 20/04/2023 08:55:18 ******/
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

	@SORTED_FIELD varchar(20) =  NULL,
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
GO

