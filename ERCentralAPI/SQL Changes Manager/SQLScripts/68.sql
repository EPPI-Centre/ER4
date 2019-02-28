USE Reviewer
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemDocumentDeleteWarning]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_ItemDocumentDeleteWarning]
GO


CREATE procedure [dbo].[st_ItemDocumentDeleteWarning]
(
	@ITEM_DOCUMENT_ID bigint
	, @NUM_CODING int output
)

As

SET NOCOUNT ON
select @NUM_CODING = count(item_attribute_id) from TB_ITEM_ATTRIBUTE_PDF where ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
select @NUM_CODING = @NUM_CODING + count(item_attribute_id) from TB_ITEM_ATTRIBUTE_TEXT where ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentBin]    Script Date: 28/02/2019 16:22:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER procedure [dbo].[st_ItemDocumentBin]
(
	@DOC_ID int,
	@REV_ID int
)

As
SELECT 
	CASE when LOWER(DOCUMENT_EXTENSION) = '.txt' THEN Null
		else DOCUMENT_BINARY
		END As "DOCUMENT_BINARY"
		,
	CASE when LOWER(DOCUMENT_EXTENSION) = '.txt' THEN DOCUMENT_TEXT
		else NULL
		END As "DOCUMENT_TEXT"
		,
	 DOCUMENT_EXTENSION, DOCUMENT_TITLE from tb_ITEM_DOCUMENT as I
	INNER JOIN TB_ITEM_REVIEW as R on I.ITEM_ID = R.ITEM_ID
WHERE ITEM_DOCUMENT_ID = @DOC_ID AND REVIEW_ID = @REV_ID
