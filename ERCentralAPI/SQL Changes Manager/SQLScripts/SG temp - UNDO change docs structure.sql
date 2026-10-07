--undo changes to data-structures and SPs

Use Reviewer
GO

--remove any doc-to-document that happens to be a duplicate: we can't represent them in the old structure

delete from TB_ITEM_TO_DOCUMENT 
from TB_ITEM_TO_DOCUMENT itd
inner join TB_ITEM_DOCUMENT d on itd.ITEM_DOCUMENT_ID = d.ITEM_DOCUMENT_ID and itd.ITEM_ID != d.ITEM_ID_STALE

--put data in TB_ITEM_TO_DOCUMENT for new records that used new SPs
IF COL_LENGTH('dbo.TB_ITEM_TO_DOCUMENT', 'DOCUMENT_TITLE') IS NULL
BEGIN
	EXECUTE sp_executesql N'update TB_ITEM_DOCUMENT set ITEM_ID_STALE = itd.ITEM_ID
		from TB_ITEM_TO_DOCUMENT itd
		inner join TB_ITEM_DOCUMENT d on itd.ITEM_DOCUMENT_ID = d.ITEM_DOCUMENT_ID and d.ITEM_ID_STALE is null'
END
ELSE
BEGIN
	EXECUTE sp_executesql N'update TB_ITEM_DOCUMENT set ITEM_ID_STALE = itd.ITEM_ID, DOCUMENT_TITLE_STALE = itd.DOCUMENT_TITLE
		from TB_ITEM_TO_DOCUMENT itd
		inner join TB_ITEM_DOCUMENT d on itd.ITEM_DOCUMENT_ID = d.ITEM_DOCUMENT_ID and d.ITEM_ID_STALE is null'
END

--rename stale cols
IF COL_LENGTH('dbo.TB_ITEM_DOCUMENT', 'ITEM_ID_STALE') IS NOT NULL
BEGIN
	EXEC sp_rename 'dbo.TB_ITEM_DOCUMENT.ITEM_ID_STALE', 'ITEM_ID', 'COLUMN';
END
GO

IF COL_LENGTH('dbo.TB_ITEM_DOCUMENT', 'DOCUMENT_TITLE_STALE') IS NOT NULL
BEGIN
	EXEC sp_rename 'dbo.TB_ITEM_DOCUMENT.DOCUMENT_TITLE_STALE', 'DOCUMENT_TITLE', 'COLUMN';
END
GO


--re-create relationship from doc to item tables
IF NOT EXISTS(select * from sys.foreign_keys where [name] = 'FK_TB_ITEM_DOCUMENT_TB_ITEM')
BEGIN
ALTER TABLE dbo.TB_ITEM_DOCUMENT ADD CONSTRAINT
	FK_TB_ITEM_DOCUMENT_TB_ITEM FOREIGN KEY
	(
	ITEM_ID
	) REFERENCES dbo.TB_ITEM
	(
	ITEM_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
end
GO

--delete new table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_ITEM_TO_DOCUMENT')
BEGIN
DROP TABLE [dbo].[TB_ITEM_TO_DOCUMENT]
END
GO

--delete new synonym
use ReviewerAdmin
IF  EXISTS(SELECT * FROM sys.synonyms where name = 'sTB_ITEM_TO_DOCUMENT')
drop synonym sTB_ITEM_TO_DOCUMENT



-------------SPs-----------------
Use Reviewer
GO


CREATE OR ALTER procedure [dbo].[st_ClusterGetXmlAllDocs]
(
	@REVIEW_ID INT
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       CAST(Title as varchar(4000)) as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		CAST(DOCUMENT_TEXT as varchar(max)) as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')

GO

CREATE OR ALTER procedure [dbo].[st_ClusterGetXmlFilteredCodeDocs]
(
	@REVIEW_ID INT,
	@ATTRIBUTE_SET_ID_LIST NVARCHAR(max)
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       CAST(Title as varchar(4000)) as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		CAST(DOCUMENT_TEXT as varchar(max)) as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'


order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')

GO


ALTER   procedure [dbo].[st_ClusterGetXmlFilteredDocs]
(
	@REVIEW_ID INT,
	@ITEM_ID_LIST NVARCHAR(max)
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
inner join TB_ITEM_TO_DOCUMENT itd on TB_ITEM.ITEM_ID = itd.ITEM_ID
inner join TB_ITEM_DOCUMENT d on d.ITEM_DOCUMENT_ID = itd.ITEM_DOCUMENT_ID
where tb_item_review.review_id = @REVIEW_ID

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       CAST(Title as varchar(4000)) as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
inner join TB_ITEM_TO_DOCUMENT itd on TB_ITEM.ITEM_ID = itd.ITEM_ID
inner join TB_ITEM_DOCUMENT d on d.ITEM_DOCUMENT_ID = itd.ITEM_DOCUMENT_ID
where tb_item_review.review_id = @REVIEW_ID

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		CAST(DOCUMENT_TEXT as varchar(max)) as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
inner join TB_ITEM_TO_DOCUMENT itd on TB_ITEM.ITEM_ID = itd.ITEM_ID
inner join TB_ITEM_DOCUMENT d on d.ITEM_DOCUMENT_ID = itd.ITEM_DOCUMENT_ID
where tb_item_review.review_id = @REVIEW_ID


order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')
GO

CREATE or ALTER PROCEDURE st_GetItemDocumentIdsFromItemIds
	-- Add the parameters for the stored procedure here
	@ReviewId int,
	@ItemIds varchar(max),
	@AlsoFetchFromLinkedItems bit = 1

AS
BEGIN
	declare @t table (ItemId bigint primary key);
    insert into @t select value from dbo.fn_Split_int(@ItemIds, ',') s
		inner join TB_ITEM_REVIEW ir on s.value = ir.ITEM_ID and ir.REVIEW_ID = @ReviewId;

	IF @AlsoFetchFromLinkedItems = 1
	BEGIN
		insert into @t select ID1.ITEM_ID from @t t
			INNER JOIN TB_ITEM_LINK IL1 on ITEM_ID_PRIMARY = t.ItemId
			INNER JOIN TB_ITEM_DOCUMENT ID1 ON ID1.ITEM_ID = IL1.ITEM_ID_SECONDARY and IL1.ITEM_ID_PRIMARY != IL1.ITEM_ID_SECONDARY
			INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = ID1.ITEM_ID and IR.REVIEW_ID = @ReviewId
			where ID1.ITEM_ID not in (select ItemId from @t);
		insert into @t SELECT ID2.ITEM_ID FROM  @t t
			INNER JOIN TB_ITEM_LINK IL2 on IL2.ITEM_ID_SECONDARY = t.ItemId
			INNER JOIN TB_ITEM_DOCUMENT ID2 ON ID2.ITEM_ID = IL2.ITEM_ID_PRIMARY and IL2.ITEM_ID_PRIMARY != IL2.ITEM_ID_SECONDARY
			INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = ID2.ITEM_ID and IR.REVIEW_ID = @ReviewId
			WHERE ID2.ITEM_ID not in (select ItemId from @t);
	END
	select distinct id.ITEM_DOCUMENT_ID, ITEM_ID, id.DOCUMENT_EXTENSION from @t t 
		inner join TB_ITEM_DOCUMENT id on t.ItemId = id.ITEM_ID;
END

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
GO


ALTER procedure [dbo].[st_ItemDocumentBinInsert]
(
	@ITEM_ID BIGINT,
	@DOCUMENT_TITLE NVARCHAR(255),
	@DOCUMENT_EXTENSION NVARCHAR(5),
	@BIN IMAGE,
	@DOCUMENT_TEXT NVARCHAR(MAX),
	@ZoteroKey NVARCHAR(50) = '',
	@HashString char(42),
	@ItemDocumentId bigint = -1 output
)

As

SET NOCOUNT ON
SET @DOCUMENT_TEXT = replace(@DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10))
	INSERT INTO TB_ITEM_DOCUMENT(ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_BINARY, DOCUMENT_TEXT, TXT_HASH)
	VALUES(@ITEM_ID, @DOCUMENT_TITLE, @DOCUMENT_EXTENSION, @BIN, [dbo].fn_CLEAN_SIMPLE_TEXT(@DOCUMENT_TEXT),
		CONVERT(varbinary(20),@HashString, 1))
	set @ItemDocumentId = SCOPE_IDENTITY()

IF @ZoteroKey != ''
BEGIN
	INSERT into TB_ZOTERO_ITEM_DOCUMENT(DocZoteroKey, ItemDocumentId) VALUES (@ZoteroKey, @ItemDocumentId)
END

SET NOCOUNT OFF
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemDocumentLinkInsert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_ItemDocumentLinkInsert]
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemDocumentFindDuplicateCandidates]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_ItemDocumentFindDuplicateCandidates]
GO 

ALTER PROCEDURE [dbo].[st_ItemDocumentDelete] 
	-- Add the parameters for the stored procedure here
	@DocID bigint = 0,
	@RevID int
AS
BEGIN
	declare @check int = 0
	--make sure source belongs to review...
	set @check = (select count(ITEM_DOCUMENT_ID) from TB_ITEM_DOCUMENT id
		inner join TB_ITEM_REVIEW ir on id.ITEM_ID = ir.ITEM_ID and REVIEW_ID = @RevID and ITEM_DOCUMENT_ID = @DocID)
	if (@check != 1) return
	BEGIN TRY
		BEGIN TRANSACTION
		delete from TB_ITEM_ATTRIBUTE_TEXT where ITEM_DOCUMENT_ID = @DocID
		delete from tb_ITEM_DOCUMENT where ITEM_DOCUMENT_ID = @DocID
		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	END CATCH
END
GO

ALTER procedure [dbo].[st_ItemDocumentDeleteWarning]
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

/****** Object:  StoredProcedure [dbo].[st_ItemDocumentList]    Script Date: 06/03/2023 10:19:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemDocumentList]
(
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON

SELECT ITEM_DOCUMENT_ID, SHORT_TITLE, ID0.ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_TEXT, 1 IDX,
'DOC_BINARY' = CASE WHEN DOCUMENT_BINARY IS NULL THEN 'False' ELSE 'True' END, DOCUMENT_FREE_NOTES
FROM TB_ITEM_DOCUMENT ID0
INNER JOIN TB_ITEM I1 ON I1.ITEM_ID = ID0.ITEM_ID
WHERE ID0.ITEM_ID = @ITEM_ID

UNION

SELECT ITEM_DOCUMENT_ID, SHORT_TITLE, ID1.ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_TEXT, 2 IDX,
'DOC_BINARY' = CASE WHEN DOCUMENT_BINARY IS NULL THEN 'False' ELSE 'True' END, DOCUMENT_FREE_NOTES
FROM TB_ITEM_LINK IL1
INNER JOIN TB_ITEM_DOCUMENT ID1 ON ID1.ITEM_ID = IL1.ITEM_ID_SECONDARY and IL1.ITEM_ID_PRIMARY != IL1.ITEM_ID_SECONDARY
INNER JOIN TB_ITEM I2 ON I2.ITEM_ID = ID1.ITEM_ID
WHERE ITEM_ID_PRIMARY = @ITEM_ID

UNION

SELECT ITEM_DOCUMENT_ID, SHORT_TITLE, ID2.ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_TEXT, 2 IDX,
'DOC_BINARY' = CASE WHEN DOCUMENT_BINARY IS NULL THEN 'False' ELSE 'True' END, DOCUMENT_FREE_NOTES
FROM TB_ITEM_LINK IL2
INNER JOIN TB_ITEM_DOCUMENT ID2 ON ID2.ITEM_ID = IL2.ITEM_ID_PRIMARY and IL2.ITEM_ID_PRIMARY != IL2.ITEM_ID_SECONDARY
INNER JOIN TB_ITEM I3 ON I3.ITEM_ID = ID2.ITEM_ID
WHERE ITEM_ID_SECONDARY = @ITEM_ID

ORDER BY IDX ASC


SET NOCOUNT OFF

GO





