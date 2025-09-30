USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ReportColumnCodeList]    Script Date: 21/07/2025 14:31:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_ReportColumnCodeList]
(
	@REPORT_COLUMN_ID INT
)

As

SET NOCOUNT ON

	--SELECT * FROM TB_REPORT_COLUMN
	--	WHERE REPORT_ID = @REPORT_ID
	--	ORDER BY COLUMN_ORDER


	declare @TV_REPORT_COLUMN_CODE table (REPORT_COLUMN_CODE_ID int, REPORT_ID int, REPORT_COLUMN_ID int, CODE_ORDER int,
		SET_ID int, ATTRIBUTE_ID bigint, PARENT_ATTRIBUTE_ID bigint, PARENT_ATTRIBUTE_TEXT nvarchar(255), USER_DEF_TEXT nvarchar(255),
		DISPLAY_CODE bit, DISPLAY_ADDITIONAL_TEXT bit, DISPLAY_CODED_TEXT bit, CODE_EXISTS bit)

	insert into @TV_REPORT_COLUMN_CODE (REPORT_COLUMN_CODE_ID, REPORT_ID, REPORT_COLUMN_ID, CODE_ORDER,
		SET_ID, ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID, PARENT_ATTRIBUTE_TEXT, USER_DEF_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, CODE_EXISTS)
	SELECT REPORT_COLUMN_CODE_ID, REPORT_ID, REPORT_COLUMN_ID, CODE_ORDER,
		SET_ID, ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID, PARENT_ATTRIBUTE_TEXT, USER_DEF_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, 0 FROM TB_REPORT_COLUMN_CODE
		WHERE REPORT_COLUMN_ID = @REPORT_COLUMN_ID
		ORDER BY CODE_ORDER

	-- set codeExists for the Codes
	update @TV_REPORT_COLUMN_CODE
	set CODE_EXISTS = 1
	where ATTRIBUTE_ID != 0
	and dbo.fn_IsAttributeInTree(ATTRIBUTE_ID) = 1

	-- set codeExists for the Coding tools
	update @TV_REPORT_COLUMN_CODE
	set CODE_EXISTS = 1
	where ATTRIBUTE_ID = 0
	and SET_ID in (select SET_ID from TB_REVIEW_SET)

	select * from @TV_REPORT_COLUMN_CODE

SET NOCOUNT OFF
GO

---------------------------------------------

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ReportData]    Script Date: 21/07/2025 14:32:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ReportData]
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_IDS NVARCHAR(MAX)
,	@REPORT_ID INT
,	@ORDER_BY NVARCHAR(15)
,	@ATTRIBUTE_ID BIGINT
,	@IS_QUESTION bit
,	@FULL_DETAILS bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @TT TABLE
	(
	  ITEM_ID BIGINT primary key
	)
	DECLARE @AA TABLE
	(
	  A_ID BIGINT 
	  , REPORT_COLUMN_CODE_ID int
	  , ATTRIBUTE_ORDER int
	)
	IF @ATTRIBUTE_ID != 0
	BEGIN
		INSERT INTO @TT
			SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
				AND TB_ITEM_REVIEW.IS_DELETED = 0
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
	END
	ELSE
	BEGIN
		INSERT INTO @TT
			SELECT VALUE FROM dbo.fn_Split_int(@ITEM_IDS, ',')
	END
	IF @IS_QUESTION = 1
	BEGIN
		INSERT INTO @AA SELECT distinct tas.ATTRIBUTE_ID, cc.REPORT_COLUMN_CODE_ID, tas.ATTRIBUTE_ORDER
			from TB_REPORT_COLUMN_CODE cc
			INNER JOIN TB_ATTRIBUTE_SET tas ON tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID 
				AND tas.SET_ID = cc.SET_ID And cc.REPORT_ID = @REPORT_ID
			inner join TB_ITEM_ATTRIBUTE ia on tas.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
			inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
			-- added by Jeff 16/07/2025 to avoid orphan codes in question reports
			where dbo.fn_IsAttributeInTree(cc.ATTRIBUTE_ID) = 1
			order by tas.ATTRIBUTE_ID
	END
	ELSE
	BEGIN
		INSERT INTO @AA SELECT distinct tas.ATTRIBUTE_ID, cc.REPORT_COLUMN_CODE_ID, tas.ATTRIBUTE_ORDER
			from TB_REPORT_COLUMN_CODE cc
			INNER JOIN TB_ATTRIBUTE_SET tas ON tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID 
				AND tas.SET_ID = cc.SET_ID And cc.REPORT_ID = @REPORT_ID
			inner join TB_ITEM_ATTRIBUTE ia on tas.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
			inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
			-- added by Jeff 16/07/2025 to avoid orphan codes in answer reports
			where dbo.fn_IsAttributeInTree(cc.ATTRIBUTE_ID) = 1
	END
	--select * from @AA
    --First: the main report properties
	SELECT * from TB_REPORT where REPORT_ID = @REPORT_ID
	--Second: list of report columns
	SELECT * from TB_REPORT_COLUMN where REPORT_ID = @REPORT_ID ORDER BY COLUMN_ORDER
	--Third: what goes into each column, AKA "Rows" (In C# side)
	SELECT * from TB_REPORT_COLUMN_CODE  
		where REPORT_ID = @REPORT_ID ORDER BY CODE_ORDER
	
	
	--Fourth: most of the real data
	SELECT distinct cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID,cc.USER_DEF_TEXT
				,a.*, ia.*, i.ITEM_ID, i.OLD_ITEM_ID, i.SHORT_TITLE, CODE_ORDER, ATTRIBUTE_ORDER
				, CASE when tia.ARM_NAME is null then '' else tia.ARM_NAME END as ARM_NAME
	from TB_REPORT_COLUMN_CODE cc
	inner join @AA ats on ats.REPORT_COLUMN_CODE_ID = cc.REPORT_COLUMN_CODE_ID
	--INNER JOIN TB_ATTRIBUTE_SET tas ON (--Question reports fetch data about a given code children
	--									(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
	--									OR 
	--									(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
	--								   )
	--	AND tas.SET_ID = cc.SET_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = ats.A_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID 
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	left outer join TB_ITEM_ARM tia on ia.ITEM_ARM_ID = tia.ITEM_ARM_ID
	where REPORT_ID = @REPORT_ID 
	ORDER BY 
		i.SHORT_TITLE -- we ignore the sorting order required for this report, sorting is done on c# side.
		, i.ITEM_ID, CODE_ORDER, ATTRIBUTE_ORDER, a.ATTRIBUTE_ID
	
	
	--Fift: data about coded TXT, uses "UNION" to grab data from TXT and PDF tables
	SELECT cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID, a.ATTRIBUTE_ID, tt.ITEM_ID, id.DOCUMENT_TITLE
	, 'Page ' + CONVERT(varchar(10),PAGE) + ':' + CHAR(10) + '[¬s]"' + replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' CODED_TEXT
	, CASE when tia.ARM_NAME is null then '' else tia.ARM_NAME END as ARM_NAME
	  from TB_REPORT_COLUMN_CODE cc
	  inner join @AA ats on ats.REPORT_COLUMN_CODE_ID = cc.REPORT_COLUMN_CODE_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = ats.A_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	inner join TB_ITEM_DOCUMENT id on ia.ITEM_ID = id.ITEM_ID
	inner join TB_ITEM_ATTRIBUTE_PDF pdf on id.ITEM_DOCUMENT_ID = pdf.ITEM_DOCUMENT_ID and ia.ITEM_ATTRIBUTE_ID = pdf.ITEM_ATTRIBUTE_ID
	left outer join TB_ITEM_ARM tia on ia.ITEM_ARM_ID = tia.ITEM_ARM_ID
	UNION
	SELECT cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID, a.ATTRIBUTE_ID, tt.ITEM_ID, id.DOCUMENT_TITLE
	, SUBSTRING(
					replace(id.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT
	, CASE when tia.ARM_NAME is null then '' else tia.ARM_NAME END as ARM_NAME
	  from TB_REPORT_COLUMN_CODE cc
	inner join @AA ats on ats.REPORT_COLUMN_CODE_ID = cc.REPORT_COLUMN_CODE_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = ats.A_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	inner join TB_ITEM_DOCUMENT id on ia.ITEM_ID = id.ITEM_ID
	inner join TB_ITEM_ATTRIBUTE_TEXT txt on id.ITEM_DOCUMENT_ID = txt.ITEM_DOCUMENT_ID and ia.ITEM_ATTRIBUTE_ID = txt.ITEM_ATTRIBUTE_ID
	left outer join TB_ITEM_ARM tia on ia.ITEM_ARM_ID = tia.ITEM_ARM_ID
	
	--sixth, items that do not have anything to report
	
	SELECT i.ITEM_ID, i.OLD_ITEM_ID, i.SHORT_TITLE from TB_ITEM i
	inner join @TT t on t.ITEM_ID = i.ITEM_ID
	where t.ITEM_ID not in
	(SELECT distinct tt.ITEM_ID
	from TB_REPORT_COLUMN_CODE cc
	INNER JOIN TB_ATTRIBUTE_SET tas ON (--Question reports fetch data about a given code children
										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
										OR 
										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
									   )
		AND tas.SET_ID = cc.SET_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	where REPORT_ID = @REPORT_ID)
	ORDER BY 
		i.SHORT_TITLE -- we ignore the sorting order required for this report, sorting is done on c# side.
		, i.ITEM_ID
	--optional Seventh: get Title, Abstract and Year, only if some of this is needed.
	if (@FULL_DETAILS = 1)
	BEGIN
		select i.ITEM_ID, TITLE, ABSTRACT, [YEAR] from TB_ITEM i
			inner join @TT t on t.ITEM_ID = i.ITEM_ID
	END
END
GO

-----------------------------

