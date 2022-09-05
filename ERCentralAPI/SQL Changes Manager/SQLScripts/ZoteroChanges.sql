USE [Reviewer]
GO
IF EXISTS(select * from sys.foreign_keys where [name] = 'FK_tb_ZOTERO_ITEM_REVIEW_tb_ITEM_REVIEW')
BEGIN
ALTER TABLE [dbo].[TB_ZOTERO_ITEM_REVIEW] DROP CONSTRAINT [FK_tb_ZOTERO_ITEM_REVIEW_tb_ITEM_REVIEW]
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_ZOTERO_ITEM_REVIEW')
BEGIN
/****** Object:  Table [dbo].[TB_ZOTERO_ITEM_REVIEW]    Script Date: 29/04/2022 14:04:28 ******/
DROP TABLE [dbo].[TB_ZOTERO_ITEM_REVIEW]
END
GO

/****** Object:  Table [dbo].[TB_ZOTERO_ITEM_REVIEW]    Script Date: 29/04/2022 14:04:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TB_ZOTERO_ITEM_REVIEW](
	[Zotero_item_review_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ItemKey] [nvarchar](50) NULL,
	[LibraryID] [nvarchar](50) NOT NULL,
	[ITEM_REVIEW_ID] [bigint] NOT NULL,
	[Version] [nvarchar](50) NULL,
	[LAST_MODIFIED] [date] NULL,
	[TypeName] [nvarchar](50) NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TB_ZOTERO_ITEM_REVIEW]  WITH CHECK ADD  CONSTRAINT [FK_tb_ZOTERO_ITEM_REVIEW_tb_ITEM_REVIEW] FOREIGN KEY([ITEM_REVIEW_ID])
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


IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_ZOTERO_ITEM_DOCUMENT')
BEGIN
/****** Object:  Table [dbo].[TB_ZOTERO_ITEM_REVIEW]    Script Date: 29/04/2022 14:04:28 ******/
DROP TABLE [dbo].[TB_ZOTERO_ITEM_DOCUMENT]
END
GO

/****** Object:  Table [dbo].[TB_ZOTERO_ITEM_DOCUMENT]    Script Date: 11/10/2021 09:37:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TB_ZOTERO_ITEM_DOCUMENT](
	[FileKey] [nvarchar](50) NOT NULL,
	[ITEM_DOCUMENT_ID] [bigint] NULL,
	[parentItem] [nvarchar](50) NULL,
	[Version] [nvarchar](50) NOT NULL,
	[LAST_MODIFIED] [date] NULL,
	[SimpleText] [nvarchar](max) NULL,
	[FileName] [nvarchar](255) NULL,
	[Extension] [nvarchar](5) NULL
 CONSTRAINT [PK_TB_ZOTERO_ITEM_DOCUMENT] PRIMARY KEY CLUSTERED 
(
	[FileKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TB_ZOTERO_ITEM_DOCUMENT]  WITH CHECK ADD FOREIGN KEY([ITEM_DOCUMENT_ID])
REFERENCES [dbo].[TB_ITEM_DOCUMENT] ([ITEM_DOCUMENT_ID])
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemReviewZoteroUpdate]    Script Date: 29/04/2022 14:24:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER   Procedure [dbo].[st_ItemReviewZoteroUpdate](
@Zotero_item_review_ID bigint NULL,
@ItemKey nvarchar(50) NULL,
@LibraryID nvarchar(50) NULL, 
@ITEM_REVIEW_ID bigint NULL, 
@Version nvarchar(50) NULL, 
@LAST_MODIFIED date NULL,
@TypeName nvarchar(50) NULL)
as
BEGIN
        UPDATE [dbo].[TB_ZOTERO_ITEM_REVIEW]
        SET    [ItemKey] = @ItemKey,
		[LibraryID] =@LibraryID, 
		[ITEM_REVIEW_ID] =@ITEM_REVIEW_ID,
		[Version] =@Version, 
		[LAST_MODIFIED] =@LAST_MODIFIED,
		[TypeName] = @TypeName
        WHERE [Zotero_item_review_ID]= @Zotero_item_review_ID
END

GO


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
/****** Object:  StoredProcedure [dbo].[st_ZoteroItemReviewCreate]    Script Date: 29/04/2022 14:22:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   Procedure [dbo].[st_ZoteroItemReviewCreate](
@ItemKey nvarchar(50) NULL,
@LibraryID nvarchar(50) NULL, 
@Version nvarchar(50) NULL, 
@LAST_MODIFIED date NULL,
@ITEM_REVIEW_ID BIGINT NULL,
@TypeName nvarchar(50) NULL)
as
Begin


	INSERT INTO [dbo].[TB_ZOTERO_ITEM_REVIEW]([ItemKey], [LibraryID],[ITEM_REVIEW_ID], [Version], [LAST_MODIFIED], [TypeName])
	VALUES(@ItemKey, @LibraryID,@ITEM_REVIEW_ID, @Version, @LAST_MODIFIED, @TypeName)
	   
End

GO

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
/****** Object:  StoredProcedure [dbo].[sp_Read_Zotero_Fetch_Item_Review_IDs]    Script Date: 27/09/2021 13:45:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER Procedure [dbo].[st_ZoteroItemReviewIDs](
@ItemIds nvarchar(50) NULL)
as
Begin	
  SELECT [ITEM_REVIEW_ID]
  FROM [Reviewer].[dbo].[TB_ITEM_REVIEW]
  Where ITEM_ID IN (SELECT value FROM [dbo].[fn_Split_int](@ItemIds, ','))
End

GO

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
@REVIEW_ID BIGINT NULL)
as
Begin
	--first check: ensure 
	declare @check int = (select count(*) from TB_ZOTERO_REVIEW_CONNECTION where ReviewId = @REVIEW_ID)
	if (@check > 0) THROW 51000, 'Review is already in use.', 1;
	INSERT INTO [dbo].[TB_ZOTERO_REVIEW_CONNECTION]([LibraryID], [ZoteroUserId], [ApiKey], [UserId], [ReviewId], DateCreated, [Version])
	VALUES(@LibraryID,@ZoteroUserId,@ApiKey,@USER_ID, @REVIEW_ID, GETDATE(),0)
	   
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

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemsInERWebANDZotero]    Script Date: 24/06/2022 19:38:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER  Procedure [dbo].[st_ItemsInERWebANDZotero]
as
Begin	
  SELECT ZIR.Zotero_item_review_ID, ZIR.ItemKey, ZIR.LibraryID, ZIR.Version,ZIR.ITEM_REVIEW_ID, ZIR.LAST_MODIFIED, IR.ITEM_ID, I.SHORT_TITLE,I.TITLE, ZIR.TypeName
  FROM [Reviewer].[dbo].[TB_ZOTERO_ITEM_REVIEW] ZIR
  INNER JOIN [Reviewer].[dbo].[TB_ITEM_REVIEW] IR
  ON ZIR.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
  INNER JOIN [Reviewer].[dbo].[TB_ITEM] I
  ON IR.ITEM_ID = I.ITEM_ID
  ORDER BY I.SHORT_TITLE
End

GO

USE [Reviewer]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   Procedure [dbo].[st_ZoteroItemReviewDelete](
@ItemKey nvarchar(50) NULL)
as
Begin

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


