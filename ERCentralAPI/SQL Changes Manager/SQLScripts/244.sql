USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSetDataList]    Script Date: 25/06/2020 14:02:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER procedure [dbo].[st_ItemSetDataList] (
	@REVIEW_ID INT,
	--@CONTACT_ID INT,
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON
	--this was changed on Aug 2013, previous version is commented below.
	--the new version gets: all completed sets for the item, plus all coded text
	--the old version was called by ItemSetList and was grabbing what was needed by the current user in DialogCoding:
	--that's the completed sets, plus the incomplete ones that belong to the user when a completed version isn't present.
	
	--25/06/2020 THIS SP is "mirrored" in st_QuickCodingReportCodingData -- changes done here are likely to be necessary there as well...

	--first, grab the completed item set (if any)
	SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
		CODING_IS_FINAL, SET_NAME, CONTACT_NAME
	FROM TB_ITEM_SET
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
	WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
		--AND TB_REVIEW_SET.CODING_IS_FINAL = 'true'
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	
	--second, get all data from TB_ITEM_ATTRIBUTE_PDF and TB_ITEM_ATTRIBUTE_TEXT using union and only from completed sets
	SELECT  tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, p.ITEM_ATTRIBUTE_PDF_ID as [ID]
			, 'Page ' + CONVERT
							(varchar(10),PAGE) 
							+ ':' + CHAR(10) + '[¬s]"' 
							+ replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' 
				as [TEXT] 
			, NULL as [TEXT_FROM], NULL as [TEXT_TO]
			, 1 as IS_FROM_PDF
			, CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END AS ARM_NAME
			, ia.ITEM_ARM_ID
		from TB_REVIEW_SET rs
		inner join TB_ITEM_SET tis on rs.REVIEW_ID = @REVIEW_ID and tis.SET_ID = rs.SET_ID and tis.ITEM_ID = @ITEM_ID
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE_PDF p on ia.ITEM_ATTRIBUTE_ID = p.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on p.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
		LEFT join TB_ITEM_ARM ON TB_ITEM_ARM.ITEM_ARM_ID = IA.ITEM_ARM_ID
	UNION
	SELECT tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, t.ITEM_ATTRIBUTE_TEXT_ID as [ID]
			, SUBSTRING(
					replace(id.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) 
				 as [TEXT]
			, TEXT_FROM, TEXT_TO 
			, 0 as IS_FROM_PDF
			, CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END AS ARM_NAME
			, ia.ITEM_ARM_ID
		from TB_REVIEW_SET rs
		inner join TB_ITEM_SET tis on rs.REVIEW_ID = @REVIEW_ID and tis.SET_ID = rs.SET_ID and tis.ITEM_ID = @ITEM_ID
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE_TEXT t on ia.ITEM_ATTRIBUTE_ID = t.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on t.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
		LEFT join TB_ITEM_ARM ON TB_ITEM_ARM.ITEM_ARM_ID = IA.ITEM_ARM_ID
	ORDER by IS_FROM_PDF, [TEXT]	
	--old version starts here
	/* Collects just the item sets that are needed by a given reviewer - not all of them for every item
	   Critically, this query NOTs out the set_ids already identified.
	 */

	-- first, grab the completed item set (if any)
	--SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
	--	CODING_IS_FINAL, SET_NAME, CONTACT_NAME
	--FROM TB_ITEM_SET
	--	INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
	--	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
	--	INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
	--WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
	--	--AND TB_REVIEW_SET.CODING_IS_FINAL = 'true'
	--	AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	
	--UNION
	----second get incomplete item_sets for the current Reviewer if no complete set is present
	--	SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
	--		CODING_IS_FINAL, SET_NAME, CONTACT_NAME
	--	FROM TB_ITEM_SET
	--		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
	--		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
	--		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
	--	WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
	--		and tb_ITEM_SET.IS_COMPLETED = 'false'
	--		and TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
	--	AND NOT TB_ITEM_SET.SET_ID IN
	--	(
	--		SELECT TB_ITEM_SET.SET_ID FROM TB_ITEM_SET
	--			INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
	--			WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
	--			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	--	)
	--end of old version
SET NOCOUNT OFF

GO

USE [Reviewer]
GO




IF TYPE_ID(N'ITEMS_INPUT_TB') IS not NULL 
	BEGIN 
		DROP PROCEDURE [dbo].[st_QuickCodingReportCodingData] 
		DROP TYPE dbo.ITEMS_INPUT_TB
	END
CREATE TYPE dbo.ITEMS_INPUT_TB AS TABLE (ItemId bigint primary key) 
GO
CREATE OR ALTER PROCEDURE [dbo].[st_QuickCodingReportCodingData] 
	-- Add the parameters for the stored procedure here
	(
		@revID int
		,@input ITEMS_INPUT_TB READONLY
		,@SetIds nvarchar(MAX)
	)
AS
BEGIN
	SET NOCOUNT ON



	Declare @Sets table (SetID int primary key)
	Insert into @Sets select [value] from dbo.fn_Split_int(@SetIds, ',') s
		 inner join TB_REVIEW_SET rs on s.value = rs.SET_ID and REVIEW_ID = @revID
	
	--FIRST reader: ordinary coding data
	SELECT ITEM_SET_ID, ir.ITEM_ID, tis.SET_ID, IS_COMPLETED, tis.CONTACT_ID, IS_LOCKED,
		CODING_IS_FINAL, SET_NAME, CONTACT_NAME
	FROM @input i
		Inner JOIN TB_ITEM_REVIEW ir on i.ItemId = ir.ITEM_ID and ir.REVIEW_ID = @revID
		INNER Join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID
		INNER JOIN @Sets ss on tis.SET_ID = ss.SetID
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = tis.SET_ID and TB_REVIEW_SET.REVIEW_ID = @revID
		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = tis.CONTACT_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = tis.SET_ID
	WHERE tis.IS_COMPLETED = 1

	--SECOND reader: PDF and Text coding data
	SELECT  tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, p.ITEM_ATTRIBUTE_PDF_ID as [ID]
			, 'Page ' + CONVERT
							(varchar(10),PAGE) 
							+ ':' + CHAR(10) + '[¬s]"' 
							+ replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' 
				as [TEXT] 
			, NULL as [TEXT_FROM], NULL as [TEXT_TO]
			, 1 as IS_FROM_PDF
			, CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END AS ARM_NAME
			, ia.ITEM_ARM_ID
		from @Sets ss
		inner join TB_REVIEW_SET rs on ss.SetID = rs.SET_ID
		inner join TB_ITEM_SET tis on tis.SET_ID = rs.SET_ID 
		inner join @input ii on tis.ITEM_ID = ii.ItemId
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and ii.ItemId = ia.ITEM_ID
		inner join TB_ITEM_ATTRIBUTE_PDF p on ia.ITEM_ATTRIBUTE_ID = p.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on p.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
		LEFT join TB_ITEM_ARM ON TB_ITEM_ARM.ITEM_ARM_ID = IA.ITEM_ARM_ID
	UNION
	SELECT tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, t.ITEM_ATTRIBUTE_TEXT_ID as [ID]
			, SUBSTRING(
					replace(id.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) 
				 as [TEXT]
			, TEXT_FROM, TEXT_TO 
			, 0 as IS_FROM_PDF
			, CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END AS ARM_NAME
			, ia.ITEM_ARM_ID
		from @Sets ss
		inner join TB_REVIEW_SET rs on ss.SetID = rs.SET_ID
		inner join TB_ITEM_SET tis on tis.SET_ID = rs.SET_ID 
		inner join @input ii on tis.ITEM_ID = ii.ItemId
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and ii.ItemId = ia.ITEM_ID
		inner join TB_ITEM_ATTRIBUTE_TEXT t on ia.ITEM_ATTRIBUTE_ID = t.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on t.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
		LEFT join TB_ITEM_ARM ON TB_ITEM_ARM.ITEM_ARM_ID = IA.ITEM_ARM_ID
	ORDER by IS_FROM_PDF, [TEXT]	
			
	SET nocount off
END
GO



GO
