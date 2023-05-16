USE [Reviewer]
GO

IF (SELECT COL_LENGTH('TB_META_ANALYSIS_FILTER_SETTING', 'SELECTED_VALUES')) = 1000
BEGIN
	Alter table TB_META_ANALYSIS_FILTER_SETTING alter column SELECTED_VALUES nvarchar(MAX) null;
END

GO

USE [ReviewerAdmin]
GO
--removing 3 SPs that exist in some dev DBs, but not live (and are now abandoned)

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CopyCodeset_times_out]') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.st_CopyCodeset_times_out
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CopyReviewStep09altVersion]') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.st_CopyReviewStep09altVersion
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CopyCodeset_orig]') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.st_CopyCodeset_orig
GO

--the following 3 SPs don't exist LIVE either, but they do exist in some dev DBs and they ARE (Jeff's) Work in Progress, so we don't drop them

--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CopyReviewShareableStep01]') AND type in (N'P', N'PC'))
--DROP PROCEDURE dbo.st_CopyReviewShareableStep01
--GO
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CopyReviewShareableStep03]') AND type in (N'P', N'PC'))
--DROP PROCEDURE dbo.st_CopyReviewShareableStep03
--GO
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_GetInfo]') AND type in (N'P', N'PC'))
--DROP PROCEDURE dbo.st_GetInfo
--GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewInfoUpdate]    Script Date: 20/03/2023 11:42:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   procedure [dbo].[st_ReviewInfoUpdate]
(
	@REVIEW_ID int
,	@SCREENING_CODE_SET_ID int
,	@SCREENING_MODE nvarchar(10)
,	@SCREENING_RECONCILLIATION nvarchar(10)
,	@SCREENING_WHAT_ATTRIBUTE_ID bigint
,	@SCREENING_N_PEOPLE int
,	@SCREENING_AUTO_EXCLUDE bit
,	@SCREENING_MODEL_RUNNING bit
,	@SCREENING_INDEXED bit
,	@MAG_ENABLED INT
,	@SHOW_SCREENING bit
) 

As

SET NOCOUNT ON

UPDATE TB_REVIEW
	SET SCREENING_CODE_SET_ID = @SCREENING_CODE_SET_ID
,		SCREENING_MODE = @SCREENING_MODE
,		SCREENING_RECONCILLIATION = @SCREENING_RECONCILLIATION
,		SCREENING_WHAT_ATTRIBUTE_ID = @SCREENING_WHAT_ATTRIBUTE_ID
,		SCREENING_N_PEOPLE = @SCREENING_N_PEOPLE
,		SCREENING_AUTO_EXCLUDE = @SCREENING_AUTO_EXCLUDE
--,		SCREENING_MODEL_RUNNING = @SCREENING_MODEL_RUNNING
,		SCREENING_INDEXED = @SCREENING_INDEXED
,		MAG_ENABLED = @MAG_ENABLED
,		SHOW_SCREENING = @SHOW_SCREENING
WHERE REVIEW_ID = @REVIEW_ID
	

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
	,@SELECTED_VALUES nvarchar(MAX)
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
	,@SELECTED_VALUES nvarchar(MAX)
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

