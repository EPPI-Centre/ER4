USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSetDataList]    Script Date: 22/01/2021 11:29:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE or ALTER procedure [dbo].[st_WebDbItemSetDataList] (
	@ITEM_ID BIGINT
	, @RevId int 
	, @WebDbId int
)

As

SET NOCOUNT ON
	--sanity check, ensure @RevId and @WebDbId match...
	Declare @CheckWebDbId int = null
	set @CheckWebDbId = (select WEBDB_ID from TB_WEBDB where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	IF @CheckWebDbId is null return;

	--first, grab the completed item set (if any)
	SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED
		--, TB_ITEM_SET.CONTACT_ID
		, IS_LOCKED,
		CODING_IS_FINAL, 
		CASE 
			WHEN WEBDB_SET_NAME IS Null then SET_NAME
			else WEBDB_SET_NAME
		END as SET_NAME 
		--, CONTACT_NAME
	FROM TB_ITEM_SET
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
		--INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
		inner JOIN TB_WEBDB_PUBLIC_SET ps on ps.REVIEW_SET_ID = TB_REVIEW_SET.REVIEW_SET_ID and ps.WEBDB_ID = @WebDbId
	WHERE REVIEW_ID = @RevId AND ITEM_ID = @ITEM_ID
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
		inner JOIN TB_WEBDB_PUBLIC_SET ps on ps.REVIEW_SET_ID = rs.REVIEW_SET_ID and ps.WEBDB_ID = @WebDbId
		inner join TB_ITEM_SET tis on rs.REVIEW_ID = @RevId and tis.SET_ID = rs.SET_ID and tis.ITEM_ID = @ITEM_ID
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
		inner JOIN TB_WEBDB_PUBLIC_SET ps on ps.REVIEW_SET_ID = rs.REVIEW_SET_ID and ps.WEBDB_ID = @WebDbId
		inner join TB_ITEM_SET tis on rs.REVIEW_ID = @RevId and tis.SET_ID = rs.SET_ID and tis.ITEM_ID = @ITEM_ID
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE_TEXT t on ia.ITEM_ATTRIBUTE_ID = t.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on t.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
		LEFT join TB_ITEM_ARM ON TB_ITEM_ARM.ITEM_ARM_ID = IA.ITEM_ARM_ID
	ORDER by IS_FROM_PDF, [TEXT]	
	
SET NOCOUNT OFF
GO



CREATE or ALTER procedure [dbo].[st_WebDbItemAttributes]
(
	@ITEM_SET_ID BIGINT
	, @RevId int 
	, @WebDbId int
)

As

SET NOCOUNT ON

--sanity check, ensure @RevId and @WebDbId match...
	Declare @CheckWebDbId int = null
	set @CheckWebDbId = (select WEBDB_ID from TB_WEBDB where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	IF @CheckWebDbId is null return;

	SELECT DISTINCT ITEM_ATTRIBUTE_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, TB_ITEM_ATTRIBUTE.ITEM_SET_ID,
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, ADDITIONAL_TEXT, TB_ITEM_ATTRIBUTE.ITEM_ARM_ID, CONTACT_ID, ATTRIBUTE_SET_ID
		,CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END AS ARM_TITLE
	FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_WEBDB_PUBLIC_ATTRIBUTE pa on TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = pa.ATTRIBUTE_ID and pa.WEBDB_ID = @WebDbId
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.ITEM_SET_ID = @ITEM_SET_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		LEFT OUTER JOIN TB_ITEM_ARM ON TB_ITEM_ARM.ITEM_ARM_ID = TB_ITEM_ATTRIBUTE.ITEM_ARM_ID
	WHERE TB_ITEM_ATTRIBUTE.ITEM_SET_ID = @ITEM_SET_ID



SET NOCOUNT OFF
GO



GO
