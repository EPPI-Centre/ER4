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
CREATE UNIQUE INDEX UIX_TB_ZOTERO_REVIEW_CONNECTION_LibraryId ON dbo.TB_ZOTERO_REVIEW_CONNECTION(LibraryId)
GO
CREATE UNIQUE INDEX UIX_TB_ZOTERO_REVIEW_CONNECTION_ReviewId ON dbo.TB_ZOTERO_REVIEW_CONNECTION(ReviewId)
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
	@ZoteroKey NVARCHAR(50) = '',
	@ItemDocumentId bigint = -1 output 
)

As

SET NOCOUNT ON
SET @DOCUMENT_TEXT = replace(@DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10))
	INSERT INTO TB_ITEM_DOCUMENT(ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_TEXT)
	VALUES(@ITEM_ID, @DOCUMENT_TITLE, @DOCUMENT_EXTENSION, @DOCUMENT_TEXT)

IF @ZoteroKey != ''
BEGIN
	set @ItemDocumentId = SCOPE_IDENTITY()
	INSERT into TB_ZOTERO_ITEM_DOCUMENT(DocZoteroKey, ItemDocumentId) VALUES (@ZoteroKey, @ItemDocumentId)
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
	@ZoteroKey NVARCHAR(50) = '',
	@ItemDocumentId bigint = -1 output
)

As

SET NOCOUNT ON
SET @DOCUMENT_TEXT = replace(@DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10))
	INSERT INTO TB_ITEM_DOCUMENT(ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_BINARY, DOCUMENT_TEXT)
	VALUES(@ITEM_ID, @DOCUMENT_TITLE, @DOCUMENT_EXTENSION, @BIN, [dbo].fn_CLEAN_SIMPLE_TEXT(@DOCUMENT_TEXT))

IF @ZoteroKey != ''
BEGIN
	set @ItemDocumentId = SCOPE_IDENTITY()
	INSERT into TB_ZOTERO_ITEM_DOCUMENT(DocZoteroKey, ItemDocumentId) VALUES (@ZoteroKey, @ItemDocumentId)
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
IF TYPE_ID(N'ITEMS_ZOT_INPUT_TB') IS not NULL 
	BEGIN 
		DROP PROCEDURE [dbo].[st_ZoteroRebuildItemLinks]
		DROP TYPE dbo.ITEMS_ZOT_INPUT_TB
	END
CREATE TYPE dbo.ITEMS_ZOT_INPUT_TB AS TABLE (ERId bigint primary key, ZOTEROKEY varchar(10)) 
GO

CREATE OR ALTER PROCEDURE [dbo].[st_ZoteroRebuildItemLinks] 
	-- Add the parameters for the stored procedure here
	(
		@revID int
		,@itemsAndKeys ITEMS_ZOT_INPUT_TB READONLY
		,@docsAndKeys ITEMS_ZOT_INPUT_TB READONLY
	)
AS
BEGIN
 declare @missingItems table (ERId bigint primary key, ZOTEROKEY varchar(10)) 
 declare @missingDocs table (ERId bigint primary key, ZOTEROKEY varchar(10)) 

 insert into TB_ZOTERO_ITEM_REVIEW (ITEM_REVIEW_ID, ItemKey)
  select ir.ITEM_REVIEW_ID, iak.ZOTEROKEY
  from @itemsAndKeys iak inner join 
  TB_ITEM_REVIEW ir on iak.ERId = ir.ITEM_ID and ir.REVIEW_ID = @revID
  left join TB_ZOTERO_ITEM_REVIEW zir on zir.ITEM_REVIEW_ID = ir.ITEM_REVIEW_ID 
  where zir.ITEM_REVIEW_ID is null

 insert into TB_ZOTERO_ITEM_DOCUMENT (ItemDocumentId, DocZoteroKey)
  select d.ITEM_DOCUMENT_ID, dak.ZOTEROKEY
  from @docsAndKeys dak 
  inner join TB_ITEM_DOCUMENT d on dak.ERId = d.ITEM_DOCUMENT_ID
  inner join TB_ITEM_REVIEW ir on d.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @revID
  left join TB_ZOTERO_ITEM_DOCUMENT zid on zid.ItemDocumentId = d.ITEM_DOCUMENT_ID 
  where zid.ItemDocumentId is null

END

GO

USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'ZoteroSetup'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('ZoteroSetup', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p>The <strong>Zotero</strong> pages allow to exchange references data with a Zotero <strong>Group Library</strong>.</p>

      <div class="container-fluid">
        <div class="row">
          <div class="col-12 col-sm-6"><b>Overview</b>
          <br>
          <img class="img w-100" src="assets/Images/zotero_schema.gif" ></div>

          <div class="col-12 col-sm-6">
            <p>Within EPPI-Reviewer, each <strong>Review</strong> can be paired with one and only one Zotero <strong>Group
            Library</strong>.
            <br>
            This pairing is done (starting from within EPPI-Reviewer) by asking Zotero to provide an Access <strong>Key</strong> to
            a Group Library. Whoever obtains the Key will be considered as the <strong>Key Owner</strong>.
            <br>
            Having a <strong>Key</strong>, it becomes possible to <strong>Push</strong> review items into the Group Library and/or
            to Pull <strong>Library References</strong> into the Review.
            <br>
            EPPI-Reviewer will keep track of the status of pulled/pushed references/items and allow to pull/push them again,
            depending on which version is &quot;newer&quot;.</p>

            <p><strong>Review Members</strong> and <strong>Group Members</strong> do not need to include the same people, however,
            the Key Owner needs to have access to both the Review and the Group Library.</p>

			<p>[A <strong>longer introduction</strong> to the Zotero functionalities can be found <a 
			href="https://eppi.ioe.ac.uk/cms/Default.aspx?tabid=3885" target="_blank">here</a>.]</p>
          </div>
        </div>

        <div class="row border-top mt-1 border-info">
          <p><strong>Setup:</strong>
          <br>
          To get started, you&#39;ll need to <strong>pair your review with a Zotero Group Library</strong>. The setup page contains
          in-line detailed instructions about the whole setup process.
          <br>
          Since some of the steps are to be carried on the <strong>Zotero pages</strong>, you can use the &quot; <strong>Show me
          (new tab)</strong>&quot; buttons to open the instructions in a separate page, and thus have them visible while setting up
          things on the Zotero end.</p>
        </div>

        <div class="row border-top mt-1 border-info">
          <p><strong>Maintenance:</strong>
          <br>
          Once an Access Key is obtained, and the pairing within Review and Group Library is established, this page shows
          <strong>basic information about the pairing/key</strong>.
          <br>
          Since the Access Key is owned and controlled by one (and only one) specific user, if the current user is not the Key
          Owner, this page will mention who the owner is, as only the Key Owner can manage or delete the key.
          <br>
          If the current user is the Key Owner, they will be offered the chance to Delete the Key, and/or do other changes if/when
          they are possible.
          <br>
          Deleting the key removes the pairing(/link) between Review and Group Library. This could be useful to re-purpose the
          Group Library to support a different review, and/or to change the Key Owner, so to side-step storage quota limitations.
          <br>
          The page will also contain a link to the Zotero &quot;storage quota&quot; <a href=
          "https://www.zotero.org/settings/storage" target="_blank">page</a>, useful to check if they are running out of
          &quot;space&quot; on the Zotero (cloud storage) side.</p>
        </div>

        <div class="row border-top mt-1 border-info">
          <p><strong>Troubleshooting:</strong>
          <br>
          EPPI-Reviewer has limited access to the Zotero end, and there are numerous things that users could do on the Zotero side,
          which could break or compromise the Review/Library pairing. For example, a user could <strong>Delete</strong> the paired
          Group Library, or even revoke/delete/edit permissions on the Access Key.</p>

          <div class="bg-light text-dark p-1 rounded border border-danger m-2">
            <strong class="text-warning">&#9888;</strong><strong> Please Note:</strong> the Zotero API used to exchange data is known to be <em>ever changing</em>. 
			This (unfortunately) means that changes in the Zotero API might produce new, unforeseen problems <em>at any time</em>; if
            this happens, we would react and fix the problem at our end as quickly as possible. However, we cannot offer guarantees
            about how long it will take. As a consequence, by using these functionalities, <strong>you implicitly accept
            the</strong> (very small) <strong>risk of facing unforeseen interruptions in your workflow, which may last for days or
            more</strong>.<strong class="text-warning">&#9888;</strong>
          </div>

          <p>For a full discussion of troubleshooting procedures, please see <a href="https://eppi.ioe.ac.uk/cms/Default.aspx?tabid=3886" target=
          "_blank">this page</a> in the EPPI-Reviewer gateway.</p>
        </div>
        <br>
      </div>
    </div>

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'ZoteroSetup'

GO

USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'ZoteroSync'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('ZoteroSync', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
    <div class="container-fluid">
        <div class="row">
          <div class="bg-light text-dark p-1 rounded border border-danger m-2">
            <strong class="text-warning">&#9888;</strong> <strong>Please Note:</strong> the Zotero API used to exchange data is known to be <em>ever changing</em>. This
            (unfortunately) means that changes in the Zotero API might produce new, unforeseen problems <em>at any time</em>; if
            this happens, we would react and fix the problem at our end as quickly as possible. However, we cannot offer guarantees
            about how long it will take. As a consequence, by using these functionalities, <strong>you implicitly accept
            the</strong> (very small) <strong>risk of facing unforeseen interruptions in your workflow, which may last for days or
            more</strong>.<strong class="text-warning">&#9888;</strong>
          </div>
        </div>

        <div class="row">
          <div class="col-12 col-sm-6"><strong>Overview (Sync)</strong>
          <br>
          <img class="img w-100" src="assets/Images/zotero_schema.gif"></div>

          <div class="col-12 col-sm-6">
            <p>The current <strong>Review</strong> is paired with one Zotero <strong>Group Library</strong> via an Access
            <strong>Key</strong> owned by a Review Member.
            <br>
            You can <strong>Push</strong> review items into the Group Library and/or <strong>Pull Library References</strong> into
            the Review.</p>

            <div class="bg-light text-dark p-1 rounded border border-danger m-2">
              <strong class="text-warning">&#9888;</strong> Items pushed into the Zotero Group Library will contribute to the <a href=
              "https://www.zotero.org/settings/storage" target="_blank">Storage Quota</a> of the Access <strong>Key</strong> owner,
              and <strong>only</strong> to their quota.<strong class="text-warning">&#9888;</strong>
            </div>

            <p>You can list &quot;<strong>Items with this code</strong>&quot; to identify Items to push, while the list on the
            right always lists <strong>All References</strong> present on the Zotero Group Library.</p>

            <p>EPPI-Reviewer keeps track of the status of pulled/pushed references/items and allows to pull/push them again
            (updating the record on the destination), depending on which version is &quot;newer&quot;, &quot;can push/pull&quot;
            and &quot;up to date&quot; icons are shown for each Item and Reference.
            <br>
            <strong>Note:</strong> EPPI-Reviewer does <strong>not continuously monitor the status of Items/References</strong>;
            thus, in some cases, you may want to press the &quot;Refresh&quot; button, to ensure the &quot;can push/pull&quot; and
            &quot;up to date&quot; icons reflect the latest changes.</p>
          </div>
        </div>

        <div class=" border-top mt-1 border-info">
          <p><strong>Typical usage:</strong>
          <br>
          The functions available in this page have been designed <strong>explicitly</strong> to support <strong>two
          specific</strong> use-cases. EPPI-Reviewer will not try to stop using these functionalities in new/more/different ways,
          but you may encounter unforeseen obstacles, if you will try to do so.
          <br>
          The <strong>supported</strong> use cases are:</p>

          <ol>
            <li>Use Zotero to &quot;<strong>find the full-text documents</strong>&quot; after screening on Title and Abstract.</li>

            <li>Use Zotero to <strong>&quot;Cite While You Write&quot; (CWYW)</strong> to produce the final review report (and/or
            other reports).</li>
          </ol>

          <p>You can find a short description of both supported workflows below, followed by some troubleshooting hints.</p>
        </div>

        <div class="border-top mt-1 border-info">
          <p><strong>Using Zotero to &quot;find the full-text documents&quot;:</strong></p>

          <ol>
            <li>Start by making sure your Group Library contains no references. This is useful to ensure you won&#39;t risk
            importing unneeded references later on.</li>

            <li>In this (EPPI-Reviewer) page, list &quot;Items with this code&quot;, picking the code(s) that contain the included
            items after screening on title and abstract.</li>

            <li>Push all these items into the Group Library.</li>

            <li>In Zotero, use the built-in &quot;Find available PDF&quot; function for all references in the library.</li>

            <li>(Optional) Use other strategies to locate the full-text documents, and attach them to your references in
            Zotero.</li>

            <li>(Optional) You may also <em>edit</em> your references in Zotero, at this point. If you do so, your changes will be
            &quot;re-imported&quot; into EPPI-Reviewer at the next step.</li>

            <li>Visit this page again, and pull all references back into EPPI-Reviewer. This will <em>Update</em> the references
            you pushed in step 3, by adding to them all the PDFs you attached in steps 4 and 5, and updating the item records
            themselves, if step 6 applies.</li>
          </ol>
        </div>

        <div class="border-top mt-1 border-info">
          <p><strong>Using Zotero to &quot;Cite While You Write&quot; (CWYW):</strong></p>

          <ol>
            <li>Start by making sure your Group Library contains no references, or only the references you know are relevant to
            your report, but are not included in the review. This is useful to ensure you won&#39;t risk having irrelevant
            references in your library.</li>

            <li>In this (EPPI-Reviewer) page, list &quot;Items with this code&quot;, picking the code(s) that contain the included
            items after screening on <strong>Full Text</strong>.</li>

            <li>Push all these items into the Group Library.</li>

            <li>(Optional) You can &quot;list items with any code&quot; of course, in case you want to push and CWYW different sets
            of references. You can also choose to push individual items, by ticking the apposite checkboxes.</li>

            <li>(Optional) If the report will be written collaboratively, ensure your collaborators have access to the Group
            Library.</li>

            <li>(Optional) You can otherwise use the group library as a &quot;stepping stone&quot; and move/copy the references
            into whichever Zotero library (shared or personal) would suit your needs.</li>

            <li>You can now CWYW while having access to all Included/Relevant studies in your review.</li>
          </ol>
        </div>

        <div class="border-top mt-1 border-info">
          <p><strong>Troubleshooting:</strong>
          <br>
          The code that drives data exchanges between Zotero and EPPI-Reviewer is remarkably complex, mostly because of the need to
          &quot;translate&quot; references&#39; data from one &quot;language&quot; to the other, which is made complex because the
          two &quot;languages&quot; are indeed, very different.
          <br>
          For this reason, it is possible that problems (anticipated or not) will occur, and/or that the EPPI-Reviewer code
          harbours disruptive bugs. In general, we <em>anticipate</em> <strong>two distinct families</strong> of issues, when
          pulling/pushing references/items: <strong>errors preventing push or pull operations</strong> and
          <strong>&quot;translation&quot; errors</strong>, where data goes missing (or deteriorates) when pushing, pulling or
          cycling through both actions.</p>

          <p><strong>Explicit errors when pushing or pulling:</strong>
          <br>
          We made considerable efforts to ensure that, whenever possible, data exchanges do not &quot;fail in block&quot;: when
          trying to pull or push many references, ideally, a problem occurring with one specific reference <em>should not</em> foil
          the whole attempt. Concurrently, we also wrote the error-handling code so to report <em>individual</em> failures.
          <br>
          In practice, this means that, if a pull or push operation encountered an error, and if it was the kind of error that does
          not compromise the whole operation, EPPI-Reviewer will show an error mentioning the unique identifiers (Item/document ID
          and/or Zotero Key) of the records involved. Concurrently, the rest of the operation would have completed, thus resulting
          in the expected changes of &quot;can pull/push, up to date&quot; status for the references that could be pulled/pushed.
          <br>
          If this happens, please take a note of the unique identifiers mentioned in the error. Try again, and if all else fails,
          please contact EPPI-Support.
          <br>
          In some cases, the whole operation mail fail, and an &quot;overall&quot; message (which doesn&#39;t mention any unique
          identifiers) will be shown. Once again, if all &quot;retry&quot; attempts do fail, please contact EPPI-Support for
          assistance.</p>

          <p><strong>Data &quot;translation&quot; errors:</strong>
          <br>
          This kind of problem occurs when re-shaping data from the &quot;language&quot; of one system to the other is either
          inevitably &quot;lossy&quot; or is done in a less than ideal way. At the root, this class of errors happens because there
          never is a one-to-one perfect correspondence of data-fields between the two systems. For example, books and books
          sections do not have a DOI field in Zotero. In addition, the &quot;types&quot; of references (Journal article, book,
          conference proceeding, etc.) supported by the two systems also do not neatly match, complicating things further.<br>
		  [More details on how EPPI-Reviewer &quot;translates&quot; reference types are <a href="https://eppi.ioe.ac.uk/cms/Default.aspx?tabid=3888" 
		  target="_blank">here</a>.]
          <br>
          Thus, when designing these functionalities, we had to make hundreds of &quot;judgement calls&quot; and then many
          different tests to check that our &quot;translation system&quot; works well enough.
          <br>
          As a result, it is possible and <em>predictable</em> that some of our judgement calls will not be ideal for all use-cases
          and that, given the size of the task, we might also have made some actual mistakes, in some place.
          <br>
          In practice, if you should notice that data goes missing (or otherwise degrades) upon pushing or pulling, you should
          contact EPPI-Support, preferably already mentioning exactly what doesn&#39;t work in detail. In an ideal world, you would
          send us a RIS file with one or more examples of references that fail to push/pull data across well enough.
          <br>
          It&#39;s also important to note here that, unlike the first &quot;family&quot; of problems, it is very unlikely, but not
          impossible, that &quot;translation problems&quot; will generate explicit error messages. Thus, you will need to actually
          inspect references on either system, and consciously <em>notice</em> the problem.</p>

          <p>For a full discussion of troubleshooting procedures, please see <a href="https://eppi.ioe.ac.uk/cms/Default.aspx?tabid=3886" target=
          "_blank">this page</a> in the EPPI-Reviewer gateway.</p>
        </div>
        <br>
      </div>

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'ZoteroSync'

GO
USE [Reviewer]
GO
