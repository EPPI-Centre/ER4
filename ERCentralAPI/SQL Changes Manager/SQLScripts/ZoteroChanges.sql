USE [Reviewer]
GO

/****** Object:  Table [dbo].[TB_ZOTERO_ITEM_REVIEW]    Script Date: 08/10/2022 14:58:31 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_ZOTERO_ITEM_REVIEW]') AND type in (N'U'))
BEGIN
--ALTER TABLE [dbo].[TB_ZOTERO_ITEM_REVIEW] DROP CONSTRAINT [FK_tb_ZOTERO_ITEM_REVIEW_tb_ITEM]
DROP TABLE [dbo].[TB_ZOTERO_ITEM_REVIEW]
END
GO

/****** Object:  Table [dbo].[TB_ZOTERO_ITEM_REVIEW]    Script Date: 08/10/2022 14:58:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TB_ZOTERO_ITEM_REVIEW](
	[Zotero_item_review_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ItemKey] [nvarchar](50) NOT NULL,
	[ITEM_REVIEW_ID] [bigint] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TB_ZOTERO_ITEM_REVIEW]  WITH CHECK ADD  
CONSTRAINT [FK_tb_ZOTERO_ITEM_REVIEW_tb_ITEM_REVIEW] FOREIGN KEY([ITEM_REVIEW_ID])
REFERENCES [dbo].[TB_ITEM_REVIEW] ([ITEM_REVIEW_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[TB_ZOTERO_ITEM_REVIEW] CHECK CONSTRAINT [FK_tb_ZOTERO_ITEM_REVIEW_tb_ITEM_REVIEW]
GO


USE [Reviewer]
GO


/****** Object:  Table [dbo].[TB_Zotero_Review_Collection]    Script Date: 15/09/2021 10:36:49 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_ZOTERO_REVIEW_COLLECTION]') AND type in (N'U'))
DROP TABLE [dbo].[TB_ZOTERO_REVIEW_COLLECTION]
GO

/****** Object:  Table [dbo].[TB_ZOTERO_REVIEW_CONNECTION]    Script Date: 21/07/2022 15:41:03 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_ZOTERO_REVIEW_CONNECTION]') AND type in (N'U'))
DROP TABLE [dbo].[TB_ZOTERO_REVIEW_CONNECTION]
GO

/****** Object:  Table [dbo].[TB_ZOTERO_REVIEW_CONNECTION]    Script Date: 21/07/2022 15:41:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TB_ZOTERO_REVIEW_CONNECTION](
	[ZOTERO_CONNECTION_ID] [int] IDENTITY(1,1) NOT NULL,
	[LibraryId] [nvarchar](50) NOT NULL,
	[ZoteroUserId] [Int] NOT NULL,
	[ApiKey] [nvarchar](50) NOT NULL,
	[ReviewId] [int] NOT NULL UNIQUE,
	[UserId] [int] NULL,
	[Version] [bigint] NOT NULL,
	[DateCreated] [date] NOT NULL
 CONSTRAINT [PK_TB_ZOTERO_REVIEW_CONNECTION] PRIMARY KEY CLUSTERED 
(
	ZOTERO_CONNECTION_ID ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE dbo.TB_ZOTERO_REVIEW_CONNECTION ADD CONSTRAINT
	FK_TB_ZOTERO_REVIEW_CONNECTION_TB_REVIEW FOREIGN KEY
	(
	ReviewId
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_ZOTERO_REVIEW_CONNECTION ADD CONSTRAINT
	FK_TB_ZOTERO_REVIEW_CONNECTION_TB_CONTACT FOREIGN KEY
	(
	UserId
	) REFERENCES dbo.TB_CONTACT
	(
	CONTACT_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO




/****** Object:  Table [dbo].[TB_ZOTERO_ITEM_DOCUMENT]    Script Date: 09/10/2022 15:49:56 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_ZOTERO_ITEM_DOCUMENT]') AND type in (N'U'))
DROP TABLE [dbo].[TB_ZOTERO_ITEM_DOCUMENT]
GO

USE [Reviewer]
GO

/****** Object:  Table [dbo].[TB_ZOTERO_ITEM_DOCUMENT]    Script Date: 21/10/2022 11:05:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TB_ZOTERO_ITEM_DOCUMENT](
	[Zotero_Item_Document_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[DocZoteroKey] [nvarchar](50) NOT NULL,
	[ItemDocumentId] [bigint] NOT NULL,
 CONSTRAINT [PK_TB_ZOTERO_ITEM_DOCUMENT] PRIMARY KEY CLUSTERED 
(
	[Zotero_Item_Document_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TB_ZOTERO_ITEM_DOCUMENT]  WITH CHECK ADD  CONSTRAINT [FK_TB_ZOTERO_ITEM_DOCUMENT_TB_ITEM_DOCUMENT] FOREIGN KEY([ItemDocumentId])
REFERENCES [dbo].[TB_ITEM_DOCUMENT] ([ITEM_DOCUMENT_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[TB_ZOTERO_ITEM_DOCUMENT] CHECK CONSTRAINT [FK_TB_ZOTERO_ITEM_DOCUMENT_TB_ITEM_DOCUMENT]
GO






--USE [Reviewer]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ItemReviewZoteroUpdate]    Script Date: 09/10/2022 14:26:06 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO


--CREATE OR ALTER       Procedure [dbo].[st_ItemReviewZoteroUpdate](
--@Zotero_item_review_ID bigint NULL,
--@ItemKey nvarchar(50) NULL,
--@LibraryID nvarchar(50) NULL, 
--@ITEM_ID bigint NULL, 
--@ITEM_REVIEW_ID bigint NULL, 
--@Version nvarchar(50) NULL, 
--@LAST_MODIFIED date NULL,
--@TypeName nvarchar(50) NULL)
--as
--BEGIN
--        UPDATE [dbo].[TB_ZOTERO_ITEM_REVIEW]
--        SET    [ItemKey] = @ItemKey,
--		[LibraryID] =@LibraryID, 
--		[ITEM_ID] = @ITEM_ID,
--		[ITEM_REVIEW_ID] =@ITEM_REVIEW_ID,
--		[Version] =@Version, 
--		[LAST_MODIFIED] =@LAST_MODIFIED,
--		[TypeName] = @TypeName
--        WHERE [Zotero_item_review_ID]= @Zotero_item_review_ID
--END

--GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[sp_Read_Zotero_ItemReview]    Script Date: 11/10/2021 09:22:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER Procedure [dbo].[st_ItemReviewZotero](
@ItemKey nvarchar(50) NULL)
as
Begin
	SELECT * FROM [dbo].[TB_ZOTERO_ITEM_REVIEW]
	WHERE [ItemKey] = @ItemKey;
End

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemReviewZoteroDelete]    Script Date: 11/10/2021 09:15:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER Procedure [dbo].[st_ItemReviewZoteroDelete](
@Zotero_item_review_ID bigint NULL)
as
BEGIN
        DELETE FROM [dbo].[TB_ZOTERO_ITEM_REVIEW]
        WHERE [Zotero_item_review_ID]= @Zotero_item_review_ID
END

GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ZoteroItemReviewCreate]    Script Date: 08/10/2022 15:31:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER Procedure [dbo].[st_ZoteroItemReviewCreate](
@ItemKey nvarchar(50),
@ReviewId int, 
@ITEM_REVIEW_ID BIGINT
)
as
Begin
	declare @check int = (select count(ITEM_ID) from TB_ITEM_REVIEW where REVIEW_ID = @ReviewId and ITEM_REVIEW_ID = @ITEM_REVIEW_ID)

	if @check = 1
	INSERT INTO TB_ZOTERO_ITEM_REVIEW
	([ItemKey], 
	[ITEM_REVIEW_ID])
	VALUES (@ItemKey, @ITEM_REVIEW_ID)
	   
End

GO

--USE [Reviewer]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ZoteroItemReviewUpdate]    Script Date: 08/10/2022 14:44:22 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO


--CREATE OR ALTER       Procedure [dbo].[st_ZoteroItemReviewUpdate](
--@Zotero_item_review_ID bigint NULL,
--@ItemKey nvarchar(50) NULL,
--@LibraryID nvarchar(50) NULL, 
--@ITEM_REVIEW_ID bigint NULL, 
--@Version nvarchar(50) NULL, 
--@LAST_MODIFIED date NULL,
--@TypeName nvarchar(50) NULL)
--as
--BEGIN
--        UPDATE [dbo].[TB_ZOTERO_ITEM_REVIEW]
--        SET    [ItemKey] = @ItemKey,
--		[LibraryID] =@LibraryID, 
--		[ITEM_REVIEW_ID] =@ITEM_REVIEW_ID,
--		[Version] =@Version, 
--		[LAST_MODIFIED] =@LAST_MODIFIED,
--		[TypeName] = @TypeName
--        WHERE [Zotero_item_review_ID]= @Zotero_item_review_ID
--END

--GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_FetchItemReview]    Script Date: 11/10/2021 09:34:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_ItemReview]
(
@ITEM_ID BIGINT 
)
AS
SET NOCOUNT ON

SELECT * FROM [Reviewer].[dbo].[TB_ITEM_REVIEW]
WHERE ITEM_ID = @ITEM_ID

SET NOCOUNT OFF


GO


/****** Script for SelectTopNRows command from SSMS  ******/



USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ZoteroItemReviewIDs]    Script Date: 18/09/2022 15:56:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER  Procedure [dbo].[st_ZoteroItemReviewIDs](
@ItemIds nvarchar(50) NULL)
as
Begin	
  SELECT TIR.ITEM_REVIEW_ID, TIR.ITEM_ID, TID.ITEM_DOCUMENT_ID
  FROM [Reviewer].[dbo].[TB_ITEM_REVIEW] TIR
  INNER JOIN TB_ITEM_DOCUMENT TID
  on TID.ITEM_ID = TIR.ITEM_ID
  Where TIR.ITEM_ID IN (SELECT value FROM [dbo].[fn_Split_int](@ItemIds, ','))
End


GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemsNotInZotero]    Script Date: 23/12/2021 11:47:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER Procedure [dbo].[st_ItemsNotInZotero](
@ReviewID bigint NULL)
as
Begin	
  SELECT IR.[ITEM_REVIEW_ID],IR.[ITEM_ID], ID.ITEM_DOCUMENT_ID
  FROM [Reviewer].[dbo].[TB_ITEM_REVIEW] IR
  LEFT JOIN [Reviewer].[dbo].[TB_ITEM_DOCUMENT] ID
  ON IR.ITEM_ID = ID.ITEM_ID
  WHERE REVIEW_ID = @ReviewID
  AND ITEM_REVIEW_ID NOT IN (SELECT [ITEM_REVIEW_ID]
  FROM [Reviewer].[dbo].[TB_ZOTERO_ITEM_REVIEW])
End


GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ERWebItemsNotInZoteroCreate]   Script Date: 11/10/2021 09:13:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER   Procedure [dbo].[st_ERWebItemsNotInZoteroCreate](
@ItemIDs nvarchar(50) NULL)
as
Begin	
  SELECT *
  FROM [Reviewer].[dbo].[TB_ITEM]
  WHERE ITEM_ID IN (SELECT value FROM [dbo].[fn_Split_int](@ItemIDs, ','))
End


GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[sp_Read_Zotero_Fetch_Item_Review_IDs]    Script Date: 11/10/2021 09:21:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER Procedure [dbo].[st_ItemReviewIDsZotero](
@ItemIds nvarchar(50) NULL)
as
Begin	
	SELECT l.[ITEM_REVIEW_ID]
	FROM [Reviewer].[dbo].[TB_ITEM_REVIEW] l
	left outer join [Reviewer].[dbo].[TB_ZOTERO_ITEM_REVIEW] r
	on l.ITEM_REVIEW_ID = r.ITEM_REVIEW_ID
	Where l.ITEM_ID IN (SELECT value FROM [dbo].[fn_Split_int](@ItemIds, ','))
	AND r.ITEM_REVIEW_ID is NULL
End

GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemIDPerItemReviewID]    Script Date: 17/10/2021 20:55:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER   Procedure [dbo].[st_ItemIDPerItemReviewID](
@ItemReviewID bigint NULL)
as
Begin
	SELECT l.ITEM_ID FROM [dbo].[TB_ITEM] l
	inner join [dbo].[TB_ITEM_REVIEW] r
	on l.ITEM_ID = r.ITEM_ID
	WHERE r.ITEM_REVIEW_ID = @ItemReviewID;
End


GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ZoteroReviewConnection]    Script Date: 05/04/2022 11:33:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER     Procedure [dbo].[st_ZoteroReviewConnection](
@ReviewID int)
as
Begin
	SELECT * FROM [dbo].[TB_ZOTERO_REVIEW_CONNECTION]
	WHERE ReviewId = @ReviewID;
End

GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ZoteroConnectionCreate]    Script Date: 01/06/2022 11:31:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER     Procedure [dbo].[st_ZoteroConnectionCreate](
	@LibraryID nvarchar(50) NULL,
	@ApiKey nvarchar(50) NULL,
	@ZoteroUserId int NULL,
	@USER_ID INT NULL,
	@REVIEW_ID BIGINT NULL,
	@ZOTERO_CONNECTION_ID INT OUT
)
as
Begin
	--first check: ensure 
	declare @check int = (select count(*) from TB_ZOTERO_REVIEW_CONNECTION where ReviewId = @REVIEW_ID)
	if (@check > 0) THROW 51000, 'Review is already in use.', 1;
	INSERT INTO [dbo].[TB_ZOTERO_REVIEW_CONNECTION]([LibraryID], [ZoteroUserId], [ApiKey], [UserId], [ReviewId], DateCreated, [Version])
	VALUES(@LibraryID,@ZoteroUserId,@ApiKey,@USER_ID, @REVIEW_ID, GETDATE(),0)
	set @ZOTERO_CONNECTION_ID = SCOPE_IDENTITY()
End
GO

USE [Reviewer]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_ERWEB_TO_ZOTERO_ITEM_TYPES]') AND type in (N'U'))
DROP TABLE [dbo].[TB_ERWEB_TO_ZOTERO_ITEM_TYPES]
GO
/****** Object:  Table [dbo].[TB_ERWEB_TO_ZOTERO_ITEM_TYPES]    Script Date: 30/11/2021 20:26:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TB_ERWEB_TO_ZOTERO_ITEM_TYPES](
	[erWebTypeId] [int] NULL,
	[zoteroTypeId] [int] NULL,
	[erWebTypeName] [nvarchar](50) NULL,
	[zoteroTypeName] [nvarchar](50) NULL
) ON [PRIMARY]
GO

USE [Reviewer]
GO

INSERT INTO [dbo].[TB_ERWEB_TO_ZOTERO_ITEM_TYPES]
           ([erWebTypeId]
           ,[zoteroTypeId]
           ,[erWebTypeName]
           ,[zoteroTypeName])
     VALUES
        (2,	1,	'Book', 'Whole	book'),
		(14,	2,	'Journal, Article',	'journalArticle')
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemReview]    Script Date: 30/11/2021 20:26:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_ItemReviewItem]
(
@ITEM_REVIEW_ID BIGINT 
)
AS
SET NOCOUNT ON

SELECT ITEM_ID FROM [Reviewer].[dbo].[TB_ITEM_REVIEW]
WHERE ITEM_REVIEW_ID = @ITEM_REVIEW_ID

SET NOCOUNT OFF

GO

USE [Reviewer]
GO


CREATE  OR ALTER  Procedure [dbo].[st_ZoteroConnectionUpdate](
@LibraryID nvarchar(50),
@ZoteroUserId int,
@ApiKey nvarchar(50),
@USER_ID INT,
@REVIEW_ID INT)
as
Begin

UPDATE [dbo].[TB_ZOTERO_REVIEW_CONNECTION]
SET LibraryID = @LibraryID, ApiKey = @ApiKey , UserId = @USER_ID , ZoteroUserId = @ZoteroUserId
WHERE ReviewId = @REVIEW_ID

END

GO





USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ZoteroCollectionDelete]    Script Date: 03/04/2022 20:33:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER Procedure [dbo].[st_ZoteroConnectionDelete](
@ApiKey nvarchar(50), @REVIEW_ID INT)
as
Begin

DELETE FROM [dbo].[TB_ZOTERO_REVIEW_CONNECTION]
WHERE ApiKey = @ApiKey AND ReviewId = @REVIEW_ID
END

GO

--USE [Reviewer]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ItemsInERWebANDZotero]    Script Date: 19/09/2022 13:58:12 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO


--CREATE OR ALTER    Procedure [dbo].[st_ItemsInERWebANDZotero]
--as
--Begin	
--  SELECT ZIR.Zotero_item_review_ID, ZIR.ItemKey, ZIR.LibraryID, ZIR.Version,ZIR.ITEM_REVIEW_ID, ZIR.LAST_MODIFIED, IR.ITEM_ID, I.SHORT_TITLE,I.TITLE, ZIR.TypeName, ZIR.SyncState
--  FROM [Reviewer].[dbo].[TB_ZOTERO_ITEM_REVIEW] ZIR
--  INNER JOIN [Reviewer].[dbo].[TB_ITEM_REVIEW] IR
--  ON ZIR.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
--  INNER JOIN [Reviewer].[dbo].[TB_ITEM] I
--  ON IR.ITEM_ID = I.ITEM_ID
--  ORDER BY I.SHORT_TITLE
--End

--GO

USE [Reviewer]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   Procedure [dbo].[st_ZoteroItemReviewDelete](
@ReviewId int,
@ItemKey nvarchar(50)
)
as
Begin

	declare @check int = (
		select count(ITEM_ID) from TB_ITEM_REVIEW ir
		inner join TB_ZOTERO_ITEM_REVIEW zir on ir.REVIEW_ID = @ReviewId and ir.ITEM_REVIEW_ID = zir.ITEM_REVIEW_ID
	)

	if @check = 1
		DELETE FROM [dbo].[TB_ZOTERO_ITEM_REVIEW]
		WHERE ItemKey = @ItemKey
	   
End

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemIDFromSource]    Script Date: 04/07/2022 16:47:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER  procedure [dbo].[st_ItemIDsFromSource]
(
	@SOURCE_ID INT
)

As

SET NOCOUNT ON

	SELECT I.ITEM_ID
	FROM [dbo].[TB_ITEM_SOURCE] ISO
	INNER JOIN [TB_ITEM] I ON 
	I.ITEM_ID = ISO.ITEM_ID
	WHERE SOURCE_ID = @SOURCE_ID
	ORDER BY DATE_CREATED DESC

SET NOCOUNT OFF

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocument]    Script Date: 11/06/2022 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_ItemDocument]
(
	@ITEM_ID int,
	@REVIEW_ID int 
)

As
SELECT ITEM_DOCUMENT_ID, DOCUMENT_TITLE FROM TB_ITEM_DOCUMENT
WHERE ITEM_ID = @ITEM_ID;

GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ZoteroReviewItemIdsPerSource]    Script Date: 04/07/2022 16:47:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER  procedure [dbo].[st_ZoteroReviewItemIdsPerSource]
(
	@SOURCE_ID INT
)

As

SET NOCOUNT ON

	SELECT IR.ITEM_REVIEW_ID
	FROM [dbo].[TB_ITEM_SOURCE] ISO
	INNER JOIN [TB_ITEM] I ON 
	I.ITEM_ID = ISO.ITEM_ID
	INNER JOIN [TB_ITEM_REVIEW] IR ON
	IR.ITEM_ID = I.ITEM_ID
	WHERE ISO.SOURCE_ID = @SOURCE_ID
	ORDER BY DATE_CREATED DESC

SET NOCOUNT OFF

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemInERWebANDZotero]    Script Date: 09/10/2022 13:32:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER      Procedure [dbo].[st_ItemInERWebANDZotero]
(
@ItemReviewId [bigint]
)
as
Begin	
  SELECT ZIR.Zotero_item_review_ID, ZIR.ItemKey, 
  --ZIR.LibraryID, 
  --ZIR.Version,
  ZIR.ITEM_REVIEW_ID, I.DATE_EDITED as LAST_MODIFIED, 
  IR.ITEM_ID, I.SHORT_TITLE, I.TITLE, ty.TYPE_NAME AS TypeName
  FROM [TB_ZOTERO_ITEM_REVIEW] ZIR
  INNER JOIN [TB_ITEM_REVIEW] IR
	ON ZIR.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
  INNER JOIN [TB_ITEM] I
	ON IR.ITEM_ID = I.ITEM_ID
  INNER JOIN TB_ITEM_TYPE ty on I.TYPE_ID = ty.TYPE_ID
  WHERE ZIR.ITEM_REVIEW_ID = @ItemReviewId
  ORDER BY I.SHORT_TITLE
End

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ZoteroERWebReviewItemList]    Script Date: 10/10/2022 09:56:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER     Procedure [dbo].[st_ZoteroERWebReviewItemList]
(
	@AttributeId bigint,
	@ReviewId int
)
as
Begin	
  declare @ids table (ItemId bigint, ITEM_REVIEW_ID bigint, Primary key(ItemId, ITEM_REVIEW_ID))

  --to start, find the itemIDs we want, we'll use this table for both results we return
  if @AttributeId > 0
  BEGIN
	--getting "items with this code", this is used to drive the "left side" table in the UI, showing what can be done with Items to the user
	  Insert into @ids Select distinct ir.ITEM_ID, ir.ITEM_REVIEW_ID from TB_ITEM_REVIEW ir
	  inner join TB_ITEM_ATTRIBUTE tia on ir.REVIEW_ID = @ReviewId and tia.ATTRIBUTE_ID = @AttributeId and ir.ITEM_ID = tia.ITEM_ID and ir.IS_DELETED = 0 and ir.IS_INCLUDED = 1
	  inner join tb_item_set tis on tia.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
  END
  ELSE
  BEGIN
	--no meaningful @AttributeId, so we get ALL items known to be present in Zotero, this is used to find out the sync state of refs present on the Zotero side.
	Insert into @ids Select distinct ir.ITEM_ID, ir.ITEM_REVIEW_ID from TB_ITEM_REVIEW ir
	inner join TB_ZOTERO_ITEM_REVIEW zi on ir.REVIEW_ID = @ReviewId and ir.ITEM_REVIEW_ID = zi.ITEM_REVIEW_ID --and ir.IS_DELETED = 0 and ir.IS_INCLUDED = 1
  END

  --first set of results, the data we want about ITEMs
  select I.ITEM_ID, I.DATE_EDITED,
	t.TYPE_NAME AS TypeName, ids.ITEM_REVIEW_ID, zi.Zotero_item_review_ID, zi.ItemKey, i.DATE_EDITED as LAST_MODIFIED, I.TITLE, I.SHORT_TITLE
  from @ids ids
  inner join TB_ITEM I on ids.ItemId = I.ITEM_ID
  inner join TB_ITEM_TYPE t on i.TYPE_ID = t.TYPE_ID
  LEFT JOIN TB_ZOTERO_ITEM_REVIEW zi on zi.ITEM_REVIEW_ID = ids.ITEM_REVIEW_ID

  --2nd set of results, the data about DOCUMENTS
  select id.ITEM_ID, id.ITEM_DOCUMENT_ID,id.DOCUMENT_TITLE, zid.DocZoteroKey from @ids ids
  inner join TB_ITEM_DOCUMENT id on ids.ItemId = id.ITEM_ID
  left join TB_ZOTERO_ITEM_DOCUMENT zid on id.ITEM_DOCUMENT_ID = zid.ItemDocumentId

End

GO

USE [Reviewer]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   Procedure [dbo].[st_ZoteroItemReviewDeleteInBulk](
@ItemKeys varchar(8000) ,
@ReviewId int 
)
as
Begin

	DELETE FROM [dbo].[TB_ZOTERO_ITEM_REVIEW]
	WHERE ITEM_REVIEW_ID in (
		select ir.ITEM_REVIEW_ID from TB_ITEM_REVIEW ir 
		inner join TB_ZOTERO_ITEM_REVIEW zi on ir.REVIEW_ID = @ReviewId and ir.ITEM_REVIEW_ID = zi.ITEM_REVIEW_ID
		inner join dbo.fn_Split(@ItemKeys, ',') s on s.value = zi.ItemKey
	)
	   
End

GO

USE [Reviewer]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   Procedure [dbo].[st_ZoteroItemDocumentDeleteInBulk](
@DocumentKeys varchar(8000),
@ReviewId int 
)
as
Begin

	DELETE FROM [dbo].[TB_ZOTERO_ITEM_DOCUMENT]
	WHERE ItemDocumentId in (
		select id.ITEM_DOCUMENT_ID from TB_ITEM_REVIEW ir 
		inner join TB_ITEM_DOCUMENT id on ir.REVIEW_ID = @ReviewId and ir.ITEM_ID = id.ITEM_ID
		inner join TB_ZOTERO_ITEM_DOCUMENT zi on  id.ITEM_DOCUMENT_ID = zi.ItemDocumentId
		inner join dbo.fn_Split(@DocumentKeys, ',') s on s.value = zi.DocZoteroKey
	)
	   
End

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
	@DOCUMENT_TEXT NVARCHAR(MAX),
	@ZoteroKey NVARCHAR(50) = ''
)

As

SET NOCOUNT ON
SET @DOCUMENT_TEXT = replace(@DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10))
	INSERT INTO TB_ITEM_DOCUMENT(ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_TEXT)
	VALUES(@ITEM_ID, @DOCUMENT_TITLE, @DOCUMENT_EXTENSION, @DOCUMENT_TEXT)

IF @ZoteroKey != ''
BEGIN
	declare @IdocID bigint = SCOPE_IDENTITY()
	INSERT into TB_ZOTERO_ITEM_DOCUMENT(DocZoteroKey, ItemDocumentId) VALUES (@ZoteroKey, @IdocID)
END

SET NOCOUNT OFF
GO
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
	@DOCUMENT_TEXT NVARCHAR(MAX),
	@ZoteroKey NVARCHAR(50) = ''
)

As

SET NOCOUNT ON
SET @DOCUMENT_TEXT = replace(@DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10))
	INSERT INTO TB_ITEM_DOCUMENT(ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_BINARY, DOCUMENT_TEXT)
	VALUES(@ITEM_ID, @DOCUMENT_TITLE, @DOCUMENT_EXTENSION, @BIN, [dbo].fn_CLEAN_SIMPLE_TEXT(@DOCUMENT_TEXT))

IF @ZoteroKey != ''
BEGIN
	declare @IdocID bigint = SCOPE_IDENTITY()
	INSERT into TB_ZOTERO_ITEM_DOCUMENT(DocZoteroKey, ItemDocumentId) VALUES (@ZoteroKey, @IdocID)
END

SET NOCOUNT OFF
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   Procedure [dbo].[st_ZoteroItemDocumentCreate](
@DocZoteroKey  nvarchar(50),
@ItemDocumentId  bigint )
as
Begin

	INSERT INTO [dbo].[TB_ZOTERO_ITEM_DOCUMENT]([DocZoteroKey], [ItemDocumentId])
	VALUES( @DocZoteroKey, @ItemDocumentId)
	   
End

GO




GO

