﻿
USE [Reviewer]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE [name] = N'tr_ZoteroCheckItem' AND [type] = 'TR')
BEGIN
    DROP TRIGGER [dbo].[tr_ZoteroCheckItem]
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE [name] = N'ZoteroCheckItem' AND [type] = 'TR')
BEGIN
    DROP TRIGGER [dbo].[ZoteroCheckItem]
END
GO
/****** Object:  Table [dbo].[Zotero_Item_Review]    Script Date: 15/09/2021 09:33:38 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_ZOTERO_ITEM_REVIEW]') AND type in (N'U'))
DROP TABLE [dbo].[TB_ZOTERO_ITEM_REVIEW]
GO

/****** Object:  Table [dbo].[TB_Zotero_Review_Collection]    Script Date: 15/09/2021 10:36:49 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_ZOTERO_REVIEW_COLLECTION]') AND type in (N'U'))
DROP TABLE [dbo].[TB_ZOTERO_REVIEW_COLLECTION]
GO


/****** Object:  Table [dbo].[Zotero_Item_Document]    Script Date: 15/09/2021 15:27:51 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_ZOTERO_ITEM_DOCUMENT]') AND type in (N'U'))
DROP TABLE [dbo].[TB_ZOTERO_ITEM_DOCUMENT]
GO


/****** Object:  Table [dbo].[TB_ERWEB_TO_ZOTERO_ITEM_TYPES]    Script Date: 30/11/2021 20:26:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_ERWEB_TO_ZOTERO_ITEM_TYPES]') AND type in (N'U'))
DROP TABLE [dbo].[TB_ERWEB_TO_ZOTERO_ITEM_TYPES]
GO


USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[sp_Create_Zotero_ItemReview]    Script Date: 16/09/2021 20:05:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroItemReviewCreate')
DROP PROCEDURE [dbo].[st_ZoteroItemReviewCreate]
GO


USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ItemInERWebANDZotero]    Script Date: 16/09/2021 20:05:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ItemInERWebANDZotero')
DROP PROCEDURE [dbo].[st_ItemInERWebANDZotero]
GO

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ZoteroItemDocumentCreate]    Script Date: 16/09/2021 20:05:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroItemDocumentCreate')
DROP PROCEDURE [dbo].[st_ZoteroItemDocumentCreate]
GO

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ItemReviewZoteroDelete]    Script Date: 16/09/2021 20:06:06 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ItemReviewZoteroDelete')
DROP PROCEDURE [dbo].[st_ItemReviewZoteroDelete]
GO

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ItemReviewZotero]   Script Date: 16/09/2021 20:06:18 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ItemReviewZotero') 
DROP PROCEDURE [dbo].[st_ItemReviewZotero]
GO

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ItemReviewZoteroUpdate]   Script Date: 16/09/2021 20:06:26 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ItemReviewZoteroUpdate') 
DROP PROCEDURE [dbo].[st_ItemReviewZoteroUpdate]

GO

/****** Object:  StoredProcedure [dbo].[st_ZoteroItemReviewIDs]    Script Date: 16/09/2021 20:06:26 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroItemReviewIDs') 
DROP PROCEDURE [dbo].[st_ZoteroItemReviewIDs]
GO

/****** Object:  StoredProcedure [dbo].[st_ItemsNotInZotero]    Script Date: 16/09/2021 20:06:26 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ItemsNotInZotero') 
DROP PROCEDURE [dbo].[st_ItemsNotInZotero]
GO


/****** Object:  StoredProcedure [dbo].[st_ERWebItemsNotInZoteroCreate]    Script Date: 16/09/2021 20:06:26 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ERWebItemsNotInZoteroCreate') 
DROP PROCEDURE [dbo].[st_ERWebItemsNotInZoteroCreate]
GO


/****** Object:  StoredProcedure [dbo].[st_ItemReviewIDsZotero]    Script Date: 16/09/2021 20:06:26 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ItemReviewIDsZotero') 
DROP PROCEDURE [dbo].[st_ItemReviewIDsZotero]
GO


/****** Object:  StoredProcedure [dbo].[st_ItemReview]    Script Date: 16/09/2021 20:06:26 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ItemReview') 
DROP PROCEDURE [dbo].[st_ItemReview]
GO

/****** Object:  StoredProcedure [dbo].[st_ItemIDPerItemReviewID]    Script Date: 16/09/2021 20:06:26 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ItemIDPerItemReviewID') 
DROP PROCEDURE [dbo].[st_ItemIDPerItemReviewID]
GO


USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ZoteroReviewCollection]    Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroReviewCollection') 
DROP PROCEDURE [dbo].[st_ZoteroReviewCollection]
GO

/****** Object:  StoredProcedure [dbo].[st_ZoteroCollectionCreate]    Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroCollectionCreate') 
DROP PROCEDURE [dbo].[st_ZoteroCollectionCreate]
GO

/****** Object:  StoredProcedure [dbo].[st_ZoteroReviewConnection]    Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroReviewConnection') 
DROP PROCEDURE [dbo].[st_ZoteroReviewConnection]
GO

/****** Object:  StoredProcedure [dbo].[st_ZoteroConnectionCreate]    Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroConnectionCreate') 
DROP PROCEDURE [dbo].[st_ZoteroConnectionCreate]
GO

--
/****** Object:  StoredProcedure [dbo].[st_ItemReviewItem]   Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ItemReviewItem') 
DROP PROCEDURE [dbo].[st_ItemReviewItem]
GO


--
/****** Object:  StoredProcedure [dbo].[st_ZoteroCollectionUpdate]  Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroCollectionUpdate') 
DROP PROCEDURE [dbo].[st_ZoteroCollectionUpdate]
GO
--
/****** Object:  StoredProcedure [dbo].[st_ZoteroConnectionUpdate]  Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroConnectionUpdate') 
DROP PROCEDURE [dbo].[st_ZoteroConnectionUpdate]
GO

--
/****** Object:  StoredProcedure [dbo].[st_ZoteroCollectionDelete]  Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroCollectionDelete') 
DROP PROCEDURE [dbo].[st_ZoteroCollectionDelete]
GO
--
/****** Object:  StoredProcedure [dbo].[st_ZoteroConnectionDelete]  Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroConnectionDelete') 
DROP PROCEDURE [dbo].[st_ZoteroConnectionDelete]
GO

/****** Object:  StoredProcedure [dbo].[st_ItemsInERWebANDZotero]  Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ItemsInERWebANDZotero') 
DROP PROCEDURE [dbo].[st_ItemsInERWebANDZotero]
GO


/****** Object:  StoredProcedure [dbo].[st_ZoteroItemReviewDelete]  Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroItemReviewDelete') 
DROP PROCEDURE [dbo].[st_ZoteroItemReviewDelete]
GO

/****** Object:  StoredProcedure [dbo].[st_ZoteroItemReviewDeleteInBulk]  Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroItemReviewDeleteInBulk') 
DROP PROCEDURE [dbo].[st_ZoteroItemReviewDeleteInBulk]
GO


/****** Object:  StoredProcedure [dbo].st_ZoteroItemDocumentDeleteInBulk]  Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroItemDocumentDeleteInBulk') 
DROP PROCEDURE [dbo].[st_ZoteroItemDocumentDeleteInBulk]
GO


/****** Object:  StoredProcedure [dbo].[st_ItemIDFromSource]  Script Date: 05/11/2021 20:04:52 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ItemIDFromSource') 
DROP PROCEDURE [dbo].[st_ItemIDFromSource]
GO

/****** Object:  StoredProcedure [dbo].[st_ZoteroERWebReviewItemList]  ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroERWebReviewItemList') 
DROP PROCEDURE [dbo].[st_ZoteroERWebReviewItemList]
GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentBinInsert]    Script Date: 21/10/2022 09:49:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemDocumentBinInsert]
(
	@ITEM_ID BIGINT,
	@DOCUMENT_TITLE NVARCHAR(255),
	@DOCUMENT_EXTENSION NVARCHAR(5),
	@BIN IMAGE,
	@DOCUMENT_TEXT NVARCHAR(MAX)
)

As

SET NOCOUNT ON
SET @DOCUMENT_TEXT = replace(@DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10))
	INSERT INTO TB_ITEM_DOCUMENT(ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_BINARY, DOCUMENT_TEXT)
	VALUES(@ITEM_ID, @DOCUMENT_TITLE, @DOCUMENT_EXTENSION, @BIN, [dbo].fn_CLEAN_SIMPLE_TEXT(@DOCUMENT_TEXT))

SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentInsert]    Script Date: 21/10/2022 09:53:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemDocumentInsert]
(
	@ITEM_ID BIGINT,
	@DOCUMENT_TITLE NVARCHAR(255),
	@DOCUMENT_EXTENSION NVARCHAR(5),
	@DOCUMENT_TEXT NVARCHAR(MAX)
)

As

SET NOCOUNT ON
SET @DOCUMENT_TEXT = replace(@DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10))
	INSERT INTO TB_ITEM_DOCUMENT(ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_TEXT)
	VALUES(@ITEM_ID, @DOCUMENT_TITLE, @DOCUMENT_EXTENSION, @DOCUMENT_TEXT)

SET NOCOUNT OFF
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'st_ZoteroRebuildItemLinks') 
DROP PROCEDURE [dbo].[st_ZoteroRebuildItemLinks]
GO
IF TYPE_ID(N'ITEMS_ZOT_INPUT_TB') IS not NULL 
	BEGIN 
		DROP TYPE dbo.ITEMS_ZOT_INPUT_TB
	END
GO
