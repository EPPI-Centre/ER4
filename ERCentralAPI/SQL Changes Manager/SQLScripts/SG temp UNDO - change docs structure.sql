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


use Reviewer
go
IF COL_LENGTH('dbo.TB_ZOTERO_ITEM_DOCUMENT', 'ITEM_REVIEW_ID') IS NULL
BEGIN
	--alter table TB_ZOTERO_ITEM_DOCUMENT drop constraint FK_tb_ZOTERO_ITEM_DOCUMENTtb_ITEM_REVIEW
	alter table TB_ZOTERO_ITEM_DOCUMENT drop column ITEM_REVIEW_ID
END

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
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentUpdate]    Script Date: 02/10/2026 09:58:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemDocumentUpdate]
(
	@ITEM_DOCUMENT_ID BIGINT,
	@DOCUMENT_TITLE NVARCHAR(255),
	@DOCUMENT_FREE_NOTES NVARCHAR(MAX) = '',
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

declare @check int = 0

declare @itemID bigint = (select ITEM_ID from 
TB_ITEM_DOCUMENT where ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID) 

set @check = (select count(*) from 
TB_ITEM_REVIEW where ITEM_ID = @itemID AND REVIEW_ID = @REVIEW_ID)

if(@check != 1) return

	UPDATE TB_ITEM_DOCUMENT
	SET DOCUMENT_TITLE = @DOCUMENT_TITLE,
	DOCUMENT_FREE_NOTES = @DOCUMENT_FREE_NOTES
	WHERE ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID

SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateDirtyGroupMembers]    Script Date: 02/10/2026 09:59:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ItemDuplicateDirtyGroupMembers]
	-- Add the parameters for the stored procedure here
	@RevID int,
	@IDs varchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    SELECT  
			0 as GROUP_MEMBER_ID
			, 0 as ITEM_DUPLICATE_GROUP_ID
			,ITEM_REVIEW_ID
			, 0 as SCORE
			, 0 as IS_CHECKED
			, 0 as IS_DUPLICATE
			, 0 as IS_MASTER
			, I.ITEM_ID
			, TITLE
			, SHORT_TITLE
			, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS
			, PARENT_TITLE
			, "YEAR"
			, "MONTH"
			, PAGES
			, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENT_AUTHORS
			, TYPE_NAME
			, (SELECT COUNT (ITEM_ATTRIBUTE_ID) FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ATTRIBUTE_SET ATS on IA.ATTRIBUTE_ID = ATS.ATTRIBUTE_ID
				INNER JOIN TB_REVIEW_SET RS on ATS.SET_ID = RS.SET_ID AND RS.REVIEW_ID = IR.REVIEW_ID
				 WHERE IA.ITEM_ID = I.ITEM_ID) CODED_COUNT
			, (SELECT COUNT (ITEM_DOCUMENT_ID) FROM TB_ITEM_DOCUMENT d WHERE d.ITEM_ID = I.ITEM_ID) DOC_COUNT
			, ( SELECT SOURCE_NAME from TB_SOURCE s inner join TB_ITEM_SOURCE tis 
					on s.SOURCE_ID = tis.SOURCE_ID and tis.ITEM_ID = I.ITEM_ID
			  ) as "SOURCE"
			, (SELECT 
				CASE 
				when COUNT(GROUP_MEMBER_ID) >0 then 1
				else 0
				end
				from TB_ITEM_DUPLICATE_GROUP_MEMBERS where ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
			) AS IS_EXPORTED
			, (SELECT COUNT(GROUP_MEMBER_ID)
				from TB_ITEM_DUPLICATE_GROUP_MEMBERS where ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
			) AS RELATED_COUNT
			, (SELECT 
				CASE 
				when COUNT(GROUP_MEMBER_ID) >0 then 0
				else 1
				end
				from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM
				INNER JOIN TB_ITEM_DUPLICATE_GROUP G on GM.GROUP_MEMBER_ID = G.MASTER_MEMBER_ID
				where ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
			) AS IS_AVAILABLE
	from TB_ITEM_REVIEW IR 
	INNER JOIN TB_ITEM I on IR.ITEM_ID = I.ITEM_ID
	INNER JOIN TB_ITEM_TYPE IT on I.TYPE_ID = IT.TYPE_ID
	WHERE REVIEW_ID = @RevID and I.ITEM_ID in 
		(
			select value from dbo.fn_Split_int(@IDs, ',')
		)
	order by I.ITEM_ID
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupMembers]    Script Date: 02/10/2026 10:00:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ItemDuplicateGroupMembers]
	-- Add the parameters for the stored procedure here
	@GroupID int,
	@ReviewID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT GM.*
		,CASE (G.MASTER_MEMBER_ID)
			when (GM.GROUP_MEMBER_ID) Then 1
			ELSE 0
		END AS IS_MASTER
		, I.ITEM_ID
		, TITLE
		, SHORT_TITLE
		, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS
		, PARENT_TITLE
		, "YEAR"
		, "MONTH"
		, PAGES
		, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENT_AUTHORS
		, TYPE_NAME
		, (SELECT COUNT (ITEM_ATTRIBUTE_ID) FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ATTRIBUTE_SET ATS on IA.ATTRIBUTE_ID = ATS.ATTRIBUTE_ID
			INNER JOIN TB_REVIEW_SET RS on ATS.SET_ID = RS.SET_ID AND RS.REVIEW_ID = G.REVIEW_ID
			 WHERE IA.ITEM_ID = I.ITEM_ID) CODED_COUNT
		, (SELECT COUNT (ITEM_DOCUMENT_ID) FROM TB_ITEM_DOCUMENT d WHERE d.ITEM_ID = I.ITEM_ID) DOC_COUNT
		, (
				SELECT SOURCE_NAME from TB_SOURCE s inner join TB_ITEM_SOURCE tis 
				on s.SOURCE_ID = tis.SOURCE_ID and tis.ITEM_ID = I.ITEM_ID
				and s.REVIEW_ID = @ReviewID
		  ) as "SOURCE"
		, case when 
					(IR.MASTER_ITEM_ID is not null and
						(
							IR.MASTER_ITEM_ID not in 
							(--current master is not coming from current group
								Select rr.ITEM_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS mm
								inner join TB_ITEM_DUPLICATE_GROUP gg on mm.GROUP_MEMBER_ID = gg.MASTER_MEMBER_ID
								inner join TB_ITEM_REVIEW rr on rr.ITEM_REVIEW_ID = mm.ITEM_REVIEW_ID
								where gg.ITEM_DUPLICATE_GROUP_ID = @GroupID
							)
							
						)
					)
					or IR.ITEM_REVIEW_ID in 
						(--current item is master of some other group 
						 select GM1.item_review_id from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM1 
							inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm2 on GM1.GROUP_MEMBER_ID != gm2.GROUP_MEMBER_ID and GM1.ITEM_DUPLICATE_GROUP_ID = @GroupID
								and GM1.ITEM_REVIEW_ID = gm2.ITEM_REVIEW_ID and gm2.ITEM_DUPLICATE_GROUP_ID != GM1.ITEM_DUPLICATE_GROUP_ID
							inner join TB_ITEM_DUPLICATE_GROUP G2 on gm2.ITEM_DUPLICATE_GROUP_ID = G2.ITEM_DUPLICATE_GROUP_ID
								and G2.MASTER_MEMBER_ID = gm2.GROUP_MEMBER_ID
							--inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM3 on G2.ITEM_DUPLICATE_GROUP_ID = GM3.ITEM_DUPLICATE_GROUP_ID 
							--	and G2.MASTER_MEMBER_ID != GM.GROUP_MEMBER_ID
							--	and GM3.IS_DUPLICATE = 1
							
								
						 
						 
						 
						 --TB_ITEM_REVIEW rrr
							--inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS mmm on mmm.ITEM_REVIEW_ID = rrr.ITEM_REVIEW_ID
							--inner join TB_ITEM_DUPLICATE_GROUP ggg on mmm.GROUP_MEMBER_ID = ggg.MASTER_MEMBER_ID 
							--inner join TB_ITEM_REVIEW rr2 on rr2.MASTER_ITEM_ID = rrr.ITEM_ID and rr2.REVIEW_ID = rrr.REVIEW_ID
							--	and ggg.ITEM_DUPLICATE_GROUP_ID != @GroupID and rrr.REVIEW_ID = ggg.REVIEW_ID
						)
			then 1--is exported: should be 1 when the member has been manually imported as a duplicate in some other group
			--it is 1 also if the current master is from another group, on the interface this makes the group member read-only
			else 0
		  END
		AS IS_EXPORTED
		,I.DOI
	from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM
	INNER JOIN TB_ITEM_DUPLICATE_GROUP G on G.ITEM_DUPLICATE_GROUP_ID = GM.ITEM_DUPLICATE_GROUP_ID
	INNER JOIN TB_ITEM_REVIEW IR on GM.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.REVIEW_ID = @ReviewID
	INNER JOIN TB_ITEM I on IR.ITEM_ID = I.ITEM_ID
	INNER JOIN TB_ITEM_TYPE IT on I.TYPE_ID = IT.TYPE_ID
	WHERE G.ITEM_DUPLICATE_GROUP_ID = @GroupID
	
	SELECT ORIGINAL_ITEM_ID ORIGINAL_MASTER_ID from TB_ITEM_DUPLICATE_GROUP where ITEM_DUPLICATE_GROUP_ID = @GroupID
	
	
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesList]    Script Date: 02/10/2026 10:01:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER procedure [dbo].[st_ItemDuplicatesList]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON
DECLARE @True bit, @False bit;
SELECT @True = 1, @False = 0;

SELECT DISTINCT I.ITEM_ID ITEM_ID1,
	I.[TYPE_ID] TYPE_ID1,
	 I.OLD_ITEM_ID OLD_ITEM_ID1, 
	 [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS1,
	I.TITLE TITLE1, 
	I.PARENT_TITLE PARENT_TITLE1, 
	I.SHORT_TITLE SHORT_TITLE1, 
	I.DATE_CREATED DATE_CREATED1, 
	I.CREATED_BY CREATED_BY1, 
	I.DATE_EDITED DATE_EDITED1, 
	I.EDITED_BY EDITED_BY1,
	I.[YEAR] YEAR1, 
	I.[MONTH] MONTH1, 
	I.STANDARD_NUMBER STANDARD_NUMBER1, 
	I.CITY CITY1,
	I.COUNTRY COUNTRY1, 
	I.PUBLISHER PUBLISHER1, 
	I.INSTITUTION INSTITUTION1, 
	I.VOLUME VOLUME1, 
	I.PAGES PAGES1,
	I.EDITION EDITION1, 
	I.ISSUE ISSUE1, 
	I.URL URL1, 
	I.ABSTRACT ABSTRACT1, 
	I.COMMENTS COMMENTS1, 
	IT1.[TYPE_NAME] TYPE_NAME1,
	[dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENT_AUTHORS1, 
	ID1.[IS_CHECKED] IS_CHECKED1, 
	case 
		when IR1.MASTER_ITEM_ID = ID1.ITEM_ID_OUT
		THEN @True
		ELSE @False
	end
	IS_DUPLICATE1, 
	i.OLD_ITEM_ID OLD_ITEM_ID1,
	(SELECT COUNT (SET_ID) FROM TB_ITEM_SET WHERE TB_ITEM_SET.ITEM_ID = I.ITEM_ID) CODED_COUNT1,
	(SELECT COUNT (ITEM_DOCUMENT_ID) FROM TB_ITEM_DOCUMENT d WHERE d.ITEM_ID = I.ITEM_ID) DOC_COUNT1,
	(SELECT SOURCE_NAME from TB_SOURCE s inner join TB_ITEM_SOURCE tis on s.SOURCE_ID = tis.SOURCE_ID and tis.ITEM_ID = I.ITEM_ID)
	 SOURCE1,
	
	I2.ITEM_ID ITEM_ID2, 
	I2.[TYPE_ID] TYPE_ID2, 
	I2.OLD_ITEM_ID OLD_ITEM_ID2, 
	[dbo].fn_REBUILD_AUTHORS(I2.ITEM_ID, 0) as AUTHORS2,
	I2.TITLE TITLE2, 
	I2.PARENT_TITLE PARENT_TITLE2, 
	I2.SHORT_TITLE SHORT_TITLE2, 
	I2.DATE_CREATED DATE_CREATED2, 
	I2.CREATED_BY CREATED_BY2, 
	I2.DATE_EDITED DATE_EDITED2, 
	I2.EDITED_BY EDITED_BY2,
	I2.[YEAR] YEAR2, 
	I2.[MONTH] MONTH2, 
	I2.STANDARD_NUMBER STANDARD_NUMBER2, 
	I2.CITY CITY2, 
	I2.COUNTRY COUNTRY2, 
	I2.PUBLISHER PUBLISHER2, 
	I2.INSTITUTION INSTITUTION2, 
	I2.VOLUME VOLUME2, 
	I2.PAGES PAGES2,
	I2.EDITION EDITION2, 
	I2.ISSUE ISSUE2, 
	I2.URL URL2, 
	I2.ABSTRACT ABSTRACT2, 
	I2.COMMENTS COMMENTS2, 
	IT2.[TYPE_NAME] TYPE_NAME2,
	[dbo].fn_REBUILD_AUTHORS(I2.ITEM_ID, 1) as PARENT_AUTHORS2, 
	i2.OLD_ITEM_ID OLD_ITEM_ID2,
	
	
	--ID2.[IS_CHECKED] IS_CHECKED2, 
	case 
		when IR2.MASTER_ITEM_ID = ID1.ITEM_ID_IN
		THEN @True
		ELSE @False
	end IS_DUPLICATE2, 
	(SELECT COUNT (SET_ID) FROM TB_ITEM_SET WHERE TB_ITEM_SET.ITEM_ID = I2.ITEM_ID) CODED_COUNT2,
	(SELECT COUNT (ITEM_DOCUMENT_ID) FROM TB_ITEM_DOCUMENT d WHERE d.ITEM_ID = I2.ITEM_ID) DOC_COUNT2,
	(SELECT SOURCE_NAME from TB_SOURCE s inner join TB_ITEM_SOURCE tis on s.SOURCE_ID = tis.SOURCE_ID and tis.ITEM_ID = I2.ITEM_ID)
	 SOURCE2,
	id1.ITEM_DUPLICATES_ID ITEM_DUPLICATES_ID1, 
	ID1._SCORE SCORE1--,
	--ID2.ITEM_DUPLICATES_ID ITEM_DUPLICATES_ID2, 
	--ID2._SCORE SCORE2

FROM TB_ITEM_DUPLICATES ID1

--INNER JOIN TB_ITEM_DUPLICATES ID2 ON ID2._key_out = ID1._key_in
INNER JOIN TB_ITEM_REVIEW IR1 on ID1.ITEM_ID_IN = IR1.ITEM_ID AND IR1.REVIEW_ID = ID1.REVIEW_ID
INNER JOIN TB_ITEM_REVIEW IR2 on ID1.ITEM_ID_OUT = IR2.ITEM_ID AND IR2.REVIEW_ID = ID1.REVIEW_ID

INNER JOIN TB_ITEM I ON I.ITEM_ID = ID1.ITEM_ID_IN
INNER JOIN TB_ITEM I2 ON I2.ITEM_ID = ID1.ITEM_ID_OUT

INNER JOIN TB_ITEM_TYPE IT1 ON IT1.[TYPE_ID] = I.[TYPE_ID]
INNER JOIN TB_ITEM_TYPE IT2 ON IT2.[TYPE_ID] = I2.[TYPE_ID]

WHERE ID1.REVIEW_ID = @REVIEW_ID --AND ID2.REVIEW_ID = @REVIEW_ID AND ID1.ITEM_ID <> ID2.ITEM_ID

ORDER BY ITEM_ID1, I.TITLE, ITEM_ID2

SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SearchForUploadedFiles]    Script Date: 02/10/2026 10:01:39 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[st_SearchForUploadedFiles]
(
      @SEARCH_ID int = null output
,     @CONTACT_ID nvarchar(50) = null
,     @REVIEW_ID nvarchar(50) = null
,     @SEARCH_TITLE varchar(4000) = null
,	  @PRESENT BIT
,     @INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'
,     @SEARCH_ITEM_ID BIGINT = NULL

)
AS
      -- Step One: Insert record into tb_SEARCH
      EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, '', '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

      -- Step Two: Perform the search and get a hits count
      
      IF (@PRESENT = 'True')
      BEGIN
		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
            SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW
            INNER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      ELSE
      BEGIN
		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
            SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
            EXCEPT
            SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW
            INNER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      
      -- Step Three: Update the new search record in tb_SEARCH with the number of records added
      UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SearchFreeText]    Script Date: 02/10/2026 10:01:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[st_SearchFreeText]
(
      @SEARCH_ID int = null output
,     @CONTACT_ID nvarchar(50) = null
,     @REVIEW_ID nvarchar(50) = null
,     @SEARCH_TITLE varchar(4000) = null
,     @SEARCH_TEXT varchar(4000) = null
,     @SEARCH_WHAT nvarchar(20) = null
,     @INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'
,     @SEARCH_ITEM_ID BIGINT = NULL

)
with recompile
AS
      -- Step One: Insert record into tb_SEARCH
      EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, @SEARCH_TEXT, '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

      -- Step Two: Perform the search and get a hits count

      IF (@SEARCH_WHAT = 'TitleAbstract')
      BEGIN
            INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID, RANK FROM TB_ITEM_REVIEW
            INNER JOIN CONTAINSTABLE(TB_ITEM, (TITLE, ABSTRACT), @SEARCH_TEXT) AS KEY_TBL ON KEY_TBL.[KEY] = TB_ITEM_REVIEW.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      ELSE
	  IF (@SEARCH_WHAT = 'Title')
      BEGIN
            INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID, RANK FROM TB_ITEM_REVIEW
            INNER JOIN CONTAINSTABLE(TB_ITEM, (TITLE), @SEARCH_TEXT) AS KEY_TBL ON KEY_TBL.[KEY] = TB_ITEM_REVIEW.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      ELSE
	  IF (@SEARCH_WHAT = 'Abstract')
      BEGIN
            INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID, RANK FROM TB_ITEM_REVIEW
            INNER JOIN CONTAINSTABLE(TB_ITEM, (ABSTRACT), @SEARCH_TEXT) AS KEY_TBL ON KEY_TBL.[KEY] = TB_ITEM_REVIEW.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      ELSE
      IF (@SEARCH_WHAT = 'PubYear')
      BEGIN
            INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID, 0 FROM TB_ITEM_REVIEW
            INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
            AND TB_ITEM.[YEAR] LIKE '%' + @SEARCH_TEXT + '%'
      END
      ELSE
      IF (@SEARCH_WHAT = 'AdditionalText')
      BEGIN
            INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT  TB_ITEM_ATTRIBUTE.ITEM_ID, @SEARCH_ID, RANK FROM TB_ITEM_ATTRIBUTE
            INNER JOIN CONTAINSTABLE(TB_ITEM_ATTRIBUTE, ADDITIONAL_TEXT, @SEARCH_TEXT) AS KEY_TBL
                  ON KEY_TBL.[KEY] =  TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
            INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
                  AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
            INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_SET.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      ELSE
      IF (@SEARCH_WHAT = 'ItemId')
      BEGIN
            INSERT INTO TB_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT TB_ITEM.ITEM_ID, @SEARCH_ID, 0 FROM TB_ITEM
            INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM.ITEM_ID
                  WHERE (TB_ITEM.ITEM_ID = @SEARCH_ITEM_ID 
                        OR OLD_ITEM_ID LIKE ('%' + @SEARCH_TEXT + '%'))
                        AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
                        AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      ELSE
      IF (@SEARCH_WHAT = 'Authors')
      BEGIN
            INSERT INTO TB_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT TB_ITEM_AUTHOR.ITEM_ID, @SEARCH_ID, 0 FROM TB_ITEM_AUTHOR
            INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_AUTHOR.ITEM_ID
            WHERE (TB_ITEM_AUTHOR.LAST LIKE '%' + @SEARCH_TEXT + '%'
                  OR TB_ITEM_AUTHOR.FIRST LIKE '%' + @SEARCH_TEXT + '%'
                  OR TB_ITEM_AUTHOR.SECOND LIKE '%' + @SEARCH_TEXT + '%')
                  AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
                  AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      ELSE -- must be uploaded documents
      BEGIN
            INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT  tb_ITEM_DOCUMENT.ITEM_ID, @SEARCH_ID, MAX(RANK) FROM tb_ITEM_DOCUMENT
            INNER JOIN CONTAINSTABLE(TB_ITEM_DOCUMENT, DOCUMENT_TEXT, @SEARCH_TEXT) AS KEY_TBL 
                  ON KEY_TBL.[KEY] = tb_ITEM_DOCUMENT.ITEM_DOCUMENT_ID
            INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = tb_ITEM_DOCUMENT.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
			GROUP BY tb_ITEM_DOCUMENT.ITEM_ID
      END
      
      -- Step Three: Update the new search record in tb_SEARCH with the number of records added
      UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID

GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SourceDeleteForever]    Script Date: 02/10/2026 10:02:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sergio
-- Create date: 20/7/09 -update May 2022
-- Description:	(Un/)Delete a source and all its Items
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_SourceDeleteForever] 
	-- Add the parameters for the stored procedure here
	@srcID int = 0,
	@revID int,
	@contactID int,
	@result int = 0 output 
AS
BEGIN
	
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	--declare @start datetime = getdate()
	
	declare @JobId int
	--FIRST BLOCK: we can call this SP for 2 reasons:
	--(A) to try deleting a source, which will happen ONLY if there isn't an older deletion pending
	-- (A.1) if no older deletion is pending, we trigger the new job
	-- (A.2) if an older deletion is pending, but has been active in the last 10m, we return without doing nothing (it might still be active)
	-- (A.3) if an older deletion is pending, but has NOT been active in the last 10m, we resume the older job, overwriting the @srcID supplied as input
	--(B) to check if an older JOB is pending - it was triggered but didn't finish within the 10m timeout:
	-- (B.1) if so, it should be resumed (in all cases).
	-- (B.2) otherwise end here: nothing to do.
	--Thus, we first check in tb_REVIEW_JOB
	--@JobId becomes not null if there is an active deletion for this review

	BEGIN TRANSACTION PRE
	--see https://weblogs.sqlteam.com/dang/2007/10/28/conditional-insertupdate-race-condition/
	--for details on how this is dealing with concurrency problems. Aim is to avoid having two instances of this SP insert
	--SP is called whenever the list of duplicates is retrieved AND when we are asking to find new duplicates...
	
	--paired with the "lasting lock", the transaction prevents two instances to be triggered concurrently
	--without the lasting lock, the TRAN itself won't work, see link above for the details.
	--WHILE running a deletion job holds the source_id in the "SUCCESS" field (it's an INT), but as a negative int, because 1 means "Success".
	set @JobId = (select top 1 REVIEW_JOB_ID from tb_REVIEW_JOB WITH (TABLOCKX, HOLDLOCK)
				where REVIEW_ID = @revID AND JOB_TYPE = 'delete source' and CURRENT_STATE = 'running' order by REVIEW_JOB_ID desc)
	IF @JobId is not null --cases (B.1, B.2, A.2 or A.3)
	BEGIN
		--2 possibilities: (1) last activity was more than 10m ago, it has TIMED OUT, so we'll resume it 
		--(2) last activity (END_TIME) was less than 10m ago, we assume it is still working and do nothing.
		if (select END_TIME from tb_REVIEW_JOB where REVIEW_JOB_ID = @JobId) < DATEADD(minute, -10, getdate()) --CASES A.3 or B.1
		BEGIN
			update tb_REVIEW_JOB set END_TIME = GETDATE() where REVIEW_JOB_ID = @JobId --if this SP is triggered again within 10m, check above will NOT come in here.
			if (@srcID > 0) --A.3
			BEGIN 
				set @result = -2 --overriding @srcID supplied, we'll resume the deletion that never finished.
			END
			--cases A.3 and B.1 the @srcID we'll work on is the one for the pre-existing job that we need to resume
			set @srcID = (SELECT SUCCESS * -1 from tb_REVIEW_JOB where REVIEW_JOB_ID = @JobId) 
			
		END
		ELSE
		BEGIN
			set @result = -1 --OLDER deletion JOB is still running
			COMMIT TRANSACTION PRE --can't return before we commit the transaction
			return
		END
	END
	IF @srcID = 0
	BEGIN
		--case B.2
		--means that this was triggered ONLY to check if a job is already running, and that we did not find a job to resume, so we should stop here
		set @result = 0 --nothing happening
		COMMIT TRANSACTION PRE --can't return before we commit the transaction
		return
	END

	--ALL cases in which we should NOT start/resume a deletion have "returned" already, if we reach the following it's because we have something to do.
	--TO stay safe, IF @srcID = 0, we don't know what to do and end here
	IF @srcID < 1 OR @srcID is null
	BEGIN
		set @result = -10 --unspecified error, should not happen
		COMMIT TRANSACTION PRE --can't return before we commit the transaction
		return
	END
	--LAST check: make sure the source belongs to review...
	declare @check int = 0
	set @check = (select count(source_id) from TB_SOURCE where SOURCE_ID = @srcID and REVIEW_ID = @revID and IS_DELETED = 1)
	if (@check != 1) 
	BEGIN
		set @result = -11 --another unspecified error, should not happen, most likely, this source wasn't already marked as deleted.
		COMMIT TRANSACTION PRE --can't return before we commit the transaction
		return
	END

	--IF user supplied a @srcID and no older job needed resuming (we got the @srcID already), we need to create a new record in TB_REVIEW_JOB and get the @JobId
	IF @JobId is null OR @JobId = 0
	BEGIN
	--we put "Source_id * -1" in the "Success" field, as it's of Int type (tells us what source is being deleted).
		insert into tb_REVIEW_JOB (REVIEW_ID, CONTACT_ID, START_TIME,END_TIME, JOB_TYPE, CURRENT_STATE, SUCCESS, JOB_MESSAGE) 
			select @revID, @contactID, GETDATE(), GETDATE(), 'delete source', 'running', @srcID * -1, ''
		set @JobId = SCOPE_IDENTITY()
	END

	COMMIT TRANSACTION PRE --we've updated or inserted into tb_REVIEW_JOB, we can release the lock

	
	--IF we got all the way to here, we have work to do, at last!!

	Declare @tt TABLE
	(
		item_ID bigint PRIMARY KEY
	)
	declare @bsize int = 400 --max size of batch we'll delete
	declare @actualbsize int = @bsize --actual size of batch we'll delete
	declare @counter int = 0
	declare @delay nchar(8) = '00:00:04'--length of the pause after each batch

	

	--select DATEDIFF(millisecond, @start, getdate()) as 'prep'
	--set @start = GETDATE()

	while (@actualbsize > 0 AND @counter < 100000)
	BEGIN
		BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
			BEGIN TRANSACTION
				insert into @tt --First: get the ITEM_IDs we'll deal with, excluding those that appear in more than one review
				SELECT top (@bsize) ITEM_ID FROM
					(select ir.ITEM_ID, COUNT(ir.item_id) cnt from TB_ITEM_REVIEW ir 
						inner join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
						where tis.SOURCE_ID = @srcID -- cnt = 1
						group by ir.ITEM_ID) cc
						where cnt=1
				Set @actualbsize = @@ROWCOUNT
				--Second: delete the records in TB_ITEM_REVIEW for the items that are shared only in this review
				--ON 27/04/2022 we start doing this deletion on items that are NOT shared, before we explicitly did it only for shared items
				delete from TB_ITEM_REVIEW 
					where REVIEW_ID = @revID
						AND ITEM_ID in (select item_ID from @tt )

	
				--select DATEDIFF(millisecond, @start, getdate()) as 'ItemReview'
				--set @start = GETDATE()

				--Third: explicitly delete the records that can't be automatically deleted through the foreign key cascade actions
				-- the cnt=1 clause makes sure we don't touch data related to items that appear in other reviews.
				DELETE FROM TB_ITEM_DUPLICATES where ITEM_ID_OUT in (SELECT item_ID from @tt )	
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_DUPLICATES'
				--set @start = GETDATE()

				DELETE FROM TB_ITEM_LINK where ITEM_ID_SECONDARY in (SELECT item_ID from @tt)

				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_LINK'
				--set @start = GETDATE()

				DELETE FROM TB_ITEM_DOCUMENT where ITEM_ID in (SELECT item_ID from @tt)
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_DOCUMENT'
				--set @start = GETDATE()

				DELETE From TB_ITEM_ATTRIBUTE where ITEM_ID in (SELECT item_ID from @tt) --and (ITEM_ARM_ID is not null AND ITEM_ARM_ID > 0)
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_ATTRIBUTE'
				--set @start = GETDATE()

				DELETE From TB_ITEM_ARM where ITEM_ID in (SELECT item_ID from @tt)
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_ARM'
				--set @start = GETDATE()

				DELETE From tb_ITEM_MAG_MATCH where ITEM_ID in (SELECT item_ID from @tt ) and REVIEW_ID = @revID
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'tb_ITEM_MAG_MATCH'
				--set @start = GETDATE()

				--ADDED on 27/04/2022 rewrite
				DELETE FROM TB_ITEM_SOURCE where ITEM_ID in (SELECT item_ID from @tt ) and SOURCE_ID = @srcID
				
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_SOURCE'
				--set @start = GETDATE()

				--ADDED on 09/04/2026 
				DELETE FROM TB_ITEM_TIMEPOINT where ITEM_ID in (SELECT item_ID from @tt ) 
				--Fourth: delete the items 
				DELETE  FROM TB_ITEM WHERE ITEM_ID in (SELECT item_ID from @tt)
				
				delete from @tt --we're adding to it at the top of the cycle
				
				set @counter = @counter+1
			COMMIT TRANSACTION
			update tb_REVIEW_JOB set END_TIME = GETDATE() where REVIEW_JOB_ID = @JobId
			
			--select DATEDIFF(millisecond, @start, getdate()), @counter as 'batch(r)'
			
			waitfor delay @delay
			
			--set @start = GETDATE()
		END TRY

		BEGIN CATCH
		--select 'caught!!!!!'
			IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
			set @result = -12 --exception
			--we do the "CAST" to ensure the message fits, can't afford an exception in here...
			update tb_REVIEW_JOB set JOB_MESSAGE = CAST(JOB_MESSAGE + Char(10)+ Char(13) + 'ERROR: ' + ERROR_MESSAGE()
														+ Char(10)+ Char(13) + 'LINE: ' + cast(ERROR_LINE() as varchar(400)) as varchar(4000))
									, CURRENT_STATE = 'Failed'
				where REVIEW_JOB_ID = @JobId
			return --we stop if something broke down
		END CATCH
	END --WHILE cycle

	--Fifth Items that are SHARED into multiple reviews, we delete them from TB_ITEM_SOURCE and TB_ITEM_REVIEW only
	delete from @tt
	insert into @tt 
	SELECT top (@bsize) ITEM_ID FROM
		(select ir.ITEM_ID, COUNT(ir.item_id) cnt from TB_ITEM_REVIEW ir 
			inner join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
			where tis.SOURCE_ID = @srcID -- cnt = 1
			group by ir.ITEM_ID) cc
			where cnt>1
	
	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
		BEGIN TRANSACTION
			DELETE from TB_ITEM_REVIEW where ITEM_ID in (SELECT item_ID from @tt) and REVIEW_ID = @revID
			
			--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_REVIEW (shared items)'
			--set @start = GETDATE()

			DELETE from TB_ITEM_SOURCE where ITEM_ID in (SELECT item_ID from @tt) and SOURCE_ID = @srcID 
			
			--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_SOURCE (shared items)'
			--set @start = GETDATE()

			--Sixth: delete the source
			DELETE FROM TB_SOURCE WHERE SOURCE_ID = @srcID
			
			--select DATEDIFF(millisecond, @start, getdate()) as 'TB_SOURCE'
			--set @start = GETDATE()

			--we do the "CAST" to ensure the message fits, can't afford an exception at this step...
			update tb_REVIEW_JOB set END_TIME = GETDATE(), CURRENT_STATE = 'Ended', SUCCESS = 1, JOB_MESSAGE = cast('SOURCE_ID = ' + CAST(@srcID as varchar(100)) +  Char(10)+ Char(13) + JOB_MESSAGE as varchar(4000)) where REVIEW_JOB_ID = @JobId
		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		--select 'caught!!!!!'
			IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
			set @result = -12 --exception
			
			--we do the "CAST" to ensure the message fits, can't afford an exception in here...
			update tb_REVIEW_JOB set JOB_MESSAGE = CAST(JOB_MESSAGE + Char(10)+ Char(13) + 'ERROR: ' + ERROR_MESSAGE()
														+ Char(10)+ Char(13) + 'LINE: ' + cast(ERROR_LINE() as varchar(400)) as varchar(4000))
									, CURRENT_STATE = 'Failed'
				where REVIEW_JOB_ID = @JobId
			return --we stop if something broke down
	END CATCH
	IF @result is null OR @result = 0 Set @result = 1 -- "-2" is for when we did delete a source, but not the one the user asked for, 1 is for when we deleted the expected source
END
GO
USE [Reviewer]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_SourceGetAllNotDuplicateDocsIDs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_SourceGetAllNotDuplicateDocsIDs]
GO

/****** Object:  StoredProcedure [dbo].[st_SourceGetAllDocsIDs]    Script Date: 02/10/2026 10:06:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   PROCEDURE [dbo].[st_SourceGetAllDocsIDs] 
	@source_ID int,
	@REVIEW_ID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--the cc trick is taken from st_SourceDeleteForever ensures we're looking ONLY at items that belong only to this source
	--so removes items that are shared into other reviews.
	select ITEM_DOCUMENT_ID, d.DOCUMENT_EXTENSION from 
			(select ir.ITEM_ID, COUNT(ir.item_id) cnt from TB_ITEM_REVIEW ir 
				inner join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
				where tis.SOURCE_ID = @source_ID -- cnt = 1
				group by ir.ITEM_ID) cc
			inner join TB_ITEM_DOCUMENT d on d.ITEM_ID = cc.ITEM_ID
				where cnt=1

	SET NOCOUNT OFF;
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SourceDeleteForeverInBatches]    Script Date: 02/10/2026 10:03:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_SourceDeleteForeverInBatches] 
	-- Add the parameters for the stored procedure here
	@srcID int = 0,
	@revID int,
	@contactID int,
	@JobId int,
	@batchSize int = 20,
	@result int = 0 output 
AS
BEGIN

	--@result values: 
	--		-1: failed to validate source, not deleted!
	--		-2: exception, job is now marked as failed!
	--		1: finished deleting this batch, more items to be deleted remain
	--		0: finished deleting - the whole source is now gone
	declare @check int = 0;
	set @check = (select count(source_id) from TB_SOURCE where SOURCE_ID = @srcID and REVIEW_ID = @revID and IS_DELETED = 1);
	if (@check != 1) 
	BEGIN
		set @result = -1; --error, should not happen, most likely, this source wasn't already marked as deleted.
		return;
	END
	Declare @tt TABLE
	(
		item_ID bigint PRIMARY KEY
	)
	declare @actualbsize int = @batchSize --actual size of batch we'll delete
	insert into @tt --First: get the ITEM_IDs we'll deal with, excluding those that appear in more than one review
		SELECT top (@batchSize) ITEM_ID FROM
			(select ir.ITEM_ID, COUNT(ir.item_id) cnt from TB_ITEM_REVIEW ir 
				inner join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
				where tis.SOURCE_ID = @srcID -- cnt = 1
				group by ir.ITEM_ID) cc
				where cnt=1
	Set @actualbsize = @@ROWCOUNT
	if @actualbsize > 0
	BEGIN
		BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
			BEGIN TRANSACTION
				--Second: delete the records in TB_ITEM_REVIEW for the items that are shared only in this review
				--ON 27/04/2022 we start doing this deletion on items that are NOT shared, before we explicitly did it only for shared items
				delete from TB_ITEM_REVIEW 
					where REVIEW_ID = @revID
						AND ITEM_ID in (select item_ID from @tt )

	
				--select DATEDIFF(millisecond, @start, getdate()) as 'ItemReview'
				--set @start = GETDATE()

				--Third: explicitly delete the records that can't be automatically deleted through the foreign key cascade actions
				-- the cnt=1 clause makes sure we don't touch data related to items that appear in other reviews.
				DELETE FROM TB_ITEM_DUPLICATES where ITEM_ID_OUT in (SELECT item_ID from @tt )	
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_DUPLICATES'
				--set @start = GETDATE()

				DELETE FROM TB_ITEM_LINK where ITEM_ID_SECONDARY in (SELECT item_ID from @tt)

				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_LINK'
				--set @start = GETDATE()

				DELETE FROM TB_ITEM_DOCUMENT where ITEM_ID in (SELECT item_ID from @tt)
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_DOCUMENT'
				--set @start = GETDATE()

				DELETE From TB_ITEM_ATTRIBUTE where ITEM_ID in (SELECT item_ID from @tt) --and (ITEM_ARM_ID is not null AND ITEM_ARM_ID > 0)
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_ATTRIBUTE'
				--set @start = GETDATE()

				DELETE From TB_ITEM_ARM where ITEM_ID in (SELECT item_ID from @tt)
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_ARM'
				--set @start = GETDATE()

				DELETE From tb_ITEM_MAG_MATCH where ITEM_ID in (SELECT item_ID from @tt ) and REVIEW_ID = @revID
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'tb_ITEM_MAG_MATCH'
				--set @start = GETDATE()

				--ADDED on 27/04/2022 rewrite
				DELETE FROM TB_ITEM_SOURCE where ITEM_ID in (SELECT item_ID from @tt ) and SOURCE_ID = @srcID
				
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_SOURCE'
				--set @start = GETDATE()

				--ADDED on 09/04/2026 
				DELETE FROM TB_ITEM_TIMEPOINT where ITEM_ID in (SELECT item_ID from @tt ) 
				--Fourth: delete the items 
				DELETE  FROM TB_ITEM WHERE ITEM_ID in (SELECT item_ID from @tt)
			COMMIT TRANSACTION;
			
			set @actualbsize = (select count(*) from TB_ITEM_SOURCE where SOURCE_ID = @srcID);
			if @actualbsize > 0
			BEGIN
				update tb_REVIEW_JOB set END_TIME = GetDATE() where REVIEW_JOB_ID = @JobId;
				set @result = 1;
				return;
			END
		END TRY

		BEGIN CATCH
		--select 'caught!!!!!'
			IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
			set @result = -2 --exception
			--we do the "CAST" to ensure the message fits, can't afford an exception in here...
			update tb_REVIEW_JOB set JOB_MESSAGE = CAST(JOB_MESSAGE + Char(10)+ Char(13) + 'ERROR: ' + ERROR_MESSAGE()
														+ Char(10)+ Char(13) + 'LINE: ' + cast(ERROR_LINE() as varchar(400)) as varchar(4000))
									, CURRENT_STATE = 'Failed'
									, SUCCESS = 0
									, END_TIME = GetDATE()
				where REVIEW_JOB_ID = @JobId
			return; --we stop if something broke down
		END CATCH
		
	END
	ELSE BEGIN --we had already finished items that are not shared, how about items in multiple reviews?
		insert into @tt 
		SELECT top (@batchSize) ITEM_ID FROM
			(select ir.ITEM_ID, COUNT(ir.item_id) cnt from TB_ITEM_REVIEW ir 
				inner join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
				where tis.SOURCE_ID = @srcID -- cnt = 1
				group by ir.ITEM_ID) cc
				where cnt>1
		Set @actualbsize = @@ROWCOUNT
		IF @actualbsize > 0
		BEGIN
			BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
				BEGIN TRANSACTION

					-- added JB 23/09/2026 to support shared items
					DELETE From tb_ITEM_MAG_MATCH where ITEM_ID in (SELECT item_ID from @tt ) and REVIEW_ID = @revID

					DELETE from TB_ITEM_REVIEW where ITEM_ID in (SELECT item_ID from @tt) and REVIEW_ID = @revID
			
					--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_REVIEW (shared items)'
					--set @start = GETDATE()

					DELETE from TB_ITEM_SOURCE where ITEM_ID in (SELECT item_ID from @tt) and SOURCE_ID = @srcID 
			
					--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_SOURCE (shared items)'
					--set @start = GETDATE()
			
					

				COMMIT TRANSACTION
				set @actualbsize = (select count(*) from TB_ITEM_SOURCE where SOURCE_ID = @srcID);
				if @actualbsize > 0
				BEGIN
					update tb_REVIEW_JOB set END_TIME = GetDATE() where REVIEW_JOB_ID = @JobId;
					set @result = 1;
					return;
				END
			END TRY
			BEGIN CATCH
				--select 'caught!!!!!'
					IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
					set @result = -2 --exception
			
					--we do the "CAST" to ensure the message fits, can't afford an exception in here...
					update tb_REVIEW_JOB set JOB_MESSAGE = CAST(JOB_MESSAGE + Char(10)+ Char(13) + 'ERROR: ' + ERROR_MESSAGE()
																+ Char(10)+ Char(13) + 'LINE: ' + cast(ERROR_LINE() as varchar(400)) as varchar(4000))
										, CURRENT_STATE = 'Failed'
										, SUCCESS = 0
										, END_TIME = GetDATE()
						where REVIEW_JOB_ID = @JobId
					return --we stop if something broke down
			END CATCH
		END
	END--finished block that deletes shared items, if any
	--both "item deleting blocks" (non shared, and shared items) return whenever there are still items to delete
	--so if we reach this point, it's because we only have the source to delete...
	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
		BEGIN TRANSACTION
				
			DELETE FROM TB_SOURCE WHERE SOURCE_ID = @srcID

		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		--select 'caught!!!!!'
			IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
			set @result = -2 --exception
			
			--we do the "CAST" to ensure the message fits, can't afford an exception in here...
			update tb_REVIEW_JOB set JOB_MESSAGE = CAST(JOB_MESSAGE + Char(10)+ Char(13) + 'ERROR: ' + ERROR_MESSAGE()
															+ Char(10)+ Char(13) + 'LINE: ' + cast(ERROR_LINE() as varchar(400)) as varchar(4000))
									, CURRENT_STATE = 'Failed'
									, SUCCESS = 0
									, END_TIME = GetDATE()
					where REVIEW_JOB_ID = @JobId
			return --we stop if something broke down
	END CATCH
	--if we didn't "return" until now, it's because we have finished the whole job!
	set @result = 0;
	update tb_REVIEW_JOB set END_TIME = GETDATE(), CURRENT_STATE = 'Ended', SUCCESS = 1, JOB_MESSAGE = cast('SOURCE_ID = ' + CAST(@srcID as varchar(100)) +  Char(10)+ Char(13) + JOB_MESSAGE as varchar(4000)) where REVIEW_JOB_ID = @JobId
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SourceDetails]    Script Date: 02/10/2026 10:06:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sergio
-- Create date: 29-06-09
-- Description:	Gets Sources from Review_ID
-- =============================================
ALTER PROCEDURE [dbo].[st_SourceDetails] 
	-- Add the parameters for the stored procedure here
	@revID int = 0,
	@sourceID int = 0
	with recompile
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	--declare @l1 nvarchar(200)
	--declare @l2 nvarchar(200)
	--declare @l3 nvarchar(200)
	--declare @l4 nvarchar(200)
	--declare @l5 nvarchar(200)
	--declare @l6 nvarchar(200)
	--declare @l7 nvarchar(200)
	
	--set @l1 = CONVERT(varchar, SYSDATETIME(), 121)
	declare @tt table
	(
		SOURCE_NAME nvarchar(255)
		,IS_DELETED bit
		,Source_ID int
		,DATE_OF_SEARCH date
		,DATE_OF_IMPORT date
		,SOURCE_DATABASE nvarchar(200)
		,SEARCH_DESCRIPTION nvarchar(4000)
		,SEARCH_STRING nvarchar(MAX)
		,NOTES nvarchar(4000)
		,IMPORT_FILTER nvarchar(60)
		,REVIEW_ID int
		
	)
	declare @t1 table
	(
		Source_ID int
		,[Total_Items] int NULL
		, [Deleted_Items] int NULL
	)
	declare @t2 table
	(
		Source_ID int
		,CODES int NULL
		,IDUCTIVE_CODES int NULL
	)
	declare @t3 table
	(
		Source_ID int
		,[Attached Files]  int NULL
		,OUTCOMES  int NULL
	)
	declare @t4 table
	(
		Source_ID int
		,DUPLICATES int NULL
		,isMasterOf  int NULL
	)

	insert into @tt
	(	
		SOURCE_NAME
		,[REVIEW_ID]
		,[Source_ID]
		,[IS_DELETED]
		,[DATE_OF_SEARCH]
		,[DATE_OF_IMPORT]
		,[SOURCE_DATABASE]
		,[SEARCH_DESCRIPTION]
		,[SEARCH_STRING]
		,[NOTES]
		,[IMPORT_FILTER]
	)
	SELECT SOURCE_NAME
		,[REVIEW_ID]
		,SOURCE_ID
		,[IS_DELETED]
		,[DATE_OF_SEARCH]
		,[DATE_OF_IMPORT]
		,[SOURCE_DATABASE]
		,[SEARCH_DESCRIPTION]
		,[SEARCH_STRING]
		,ts.[NOTES]
		,tif.IMPORT_FILTER_NAME
	from TB_SOURCE ts Left outer join TB_IMPORT_FILTER tif on ts.IMPORT_FILTER_ID = tif.IMPORT_FILTER_ID
	where REVIEW_ID = @revID and SOURCE_ID = @sourceID
	--set @l2 = CONVERT(varchar, SYSDATETIME(), 121)

	insert into @t1 
		SELECT tt.source_id 
		,COUNT(distinct(tis.ITEM_ID)) 'Total_Items'
		,sum(CASE WHEN ir.IS_DELETED = 1 then 1 else 0 END) 
		--, (SELECT COUNT(distinct(ttis.ITEM_ID)) from TB_ITEM_REVIEW ttir
		--		inner join TB_ITEM_SOURCE ttis on ttis.ITEM_ID = ttir.ITEM_ID
		--		where ttis.SOURCE_ID = tt.SOURCE_ID and ttir.IS_DELETED = 1) 'Deleted_Items'
		from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
		WHERE ir.REVIEW_ID = @revID
		group by tt.Source_ID
	--set @l3 = CONVERT(varchar, SYSDATETIME(), 121)
	Insert into @t2
	(Source_ID, CODES, IDUCTIVE_CODES)  (select source_id, 0 ,0 from @tt)
	update @t2   set CODES = sub.CODES--, IDUCTIVE_CODES = sub.IDUCTIVE_CODES
	from
		(
			SELECT 
			tt.source_id SSID
			,COUNT(distinct(ia.ITEM_ID)) CODES 
			--,COUNT(distinct(iat.ITEM_ATTRIBUTE_TEXT_ID)) IDUCTIVE_CODES
			--,COUNT(distinct(tid.ITEM_DOCUMENT_ID)) [Attached Files]
			--,COUNT(distinct(tio.OUTCOME_ID)) OUTCOMES  
			from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
				--inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
				inner join TB_REVIEW_SET rs on rs.REVIEW_ID = tt.REVIEW_ID and tt.REVIEW_ID = @revID
				inner join TB_ATTRIBUTE_SET tas on rs.SET_ID = tas.SET_ID
				inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_ID = ia.ITEM_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
				--left outer join TB_ITEM_ATTRIBUTE_TEXT iat on ia.ITEM_ATTRIBUTE_ID = iat.ITEM_ATTRIBUTE_ID
				--left outer join TB_ITEM_DOCUMENT tid on tid.ITEM_ID = tis.ITEM_ID
				--left outer join TB_ITEM_SET tes on tis.ITEM_ID = tes.ITEM_ID 
				--left outer join TB_ITEM_OUTCOME tio on tio.ITEM_SET_ID = tes.ITEM_SET_ID 
			--WHERE ir.REVIEW_ID = @revID
			
			group by tt.Source_ID
			
		) sub
		where sub.SSID = Source_ID
		--OPTION (OPTIMIZE FOR UNKNOWN)
	--set @l4 = CONVERT(varchar, SYSDATETIME(), 121)
	Insert into @t3
		SELECT tt.source_id
		--,COUNT(distinct(ia.ITEM_ATTRIBUTE_ID)) CODES 
		--,COUNT(distinct(iat.ITEM_ATTRIBUTE_TEXT_ID)) IDUCTIVE_CODES
		,COUNT(distinct(tid.ITEM_DOCUMENT_ID)) [Attached Files]
		,COUNT(distinct(tio.OUTCOME_ID)) OUTCOMES  
		from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
			--left outer join TB_REVIEW_SET rs on rs.REVIEW_ID = tt.REVIEW_ID
			--left outer join TB_ATTRIBUTE_SET tas on rs.SET_ID = tas.SET_ID
			--left outer join TB_ITEM_ATTRIBUTE ia on tis.ITEM_ID = ia.ITEM_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
			--left outer join TB_ITEM_ATTRIBUTE_TEXT iat on ia.ITEM_ATTRIBUTE_ID = iat.ITEM_ATTRIBUTE_ID
			left outer join TB_ITEM_DOCUMENT tid on tid.ITEM_ID = tis.ITEM_ID
			left outer join TB_ITEM_SET tes on tis.ITEM_ID = tes.ITEM_ID 
			left outer join TB_ITEM_OUTCOME tio on tio.ITEM_SET_ID = tes.ITEM_SET_ID 
		WHERE ir.REVIEW_ID = @revID
		group by tt.Source_ID
	--set @l5 = CONVERT(varchar, SYSDATETIME(), 121)
	Insert into @t4
		SELECT
		tt.source_id
		--,(COUNT(distinct(dup.ITEM_DUPLICATES_ID)) + COUNT(distinct(dup2.ITEM_DUPLICATES_ID))) DUPLICATES
		--,COUNT(distinct(ir2.ITEM_REVIEW_ID)) isMasterOf
		,sum(CASE WHEN (
				ir.IS_DELETED = 1 and ir.is_included = 1 AND ir.MASTER_ITEM_ID is NOT null
			) then 1 else 0 END) as 'Duplicates'
		,0
		from 
			@tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
			--left outer join TB_ITEM_REVIEW ir2 on ir2.MASTER_ITEM_ID = ir.ITEM_ID and ir2.REVIEW_ID = @revID
		WHERE ir.REVIEW_ID = @revID
		
		group by tt.Source_ID

	update @t4 set isMasterOf = a.c
		from
		(Select COUNT (distinct ir2.item_id) as c from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.MASTER_ITEM_ID
			inner join TB_ITEM_REVIEW ir2 on ir.MASTER_ITEM_ID = ir2.ITEM_ID and ir2.REVIEW_ID = @revID
			) a
	--set @l6 = CONVERT(varchar, SYSDATETIME(), 121)
	select 
		SOURCE_NAME
		,[Total_Items]
		,[Deleted_Items]
		,IS_DELETED
		,t1.Source_ID
		,DATE_OF_SEARCH
		,DATE_OF_IMPORT
		,SOURCE_DATABASE
		,SEARCH_DESCRIPTION
		,SEARCH_STRING
		,NOTES
		,IMPORT_FILTER
		,REVIEW_ID
		,CODES
		,IDUCTIVE_CODES
		,[Attached Files]
		,DUPLICATES
		,isMasterOf
		,OUTCOMES
	from @tt tt
	inner join @t1 t1 on tt.Source_ID = t1.Source_ID
	inner join @t2 t2 on tt.Source_ID = t2.Source_ID
	inner join @t3 t3 on tt.Source_ID = t3.Source_ID
	inner join @t4 t4 on tt.Source_ID = t4.Source_ID
	order by Source_ID
	--set @l7 = CONVERT(varchar, SYSDATETIME(), 121)
	--select @l1, @l2, @l3, @l4, @l5, @l6, @l7
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_AttributeTextAllItems]    Script Date: 02/10/2026 10:09:40 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
ALTER PROCEDURE [dbo].[st_AttributeTextAllItems] (
        @ATTRIBUTE_SET_ID BIGINT
)
AS
SET NOCOUNT ON


SELECT TB_ITEM.TITLE, SHORT_TITLE, TB_ITEM.ITEM_ID, ADDITIONAL_TEXT, TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID, DOCUMENT_TITLE
				, TEXT_FROM, TEXT_TO, 0 as [PAGE]
                ,        SUBSTRING(
                        replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
                        ) CODED_TEXT
                ,'None' as [ORIGIN]
                FROM tb_ITEM_ATTRIBUTE
                LEFT JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
                LEFT JOIN TB_ITEM_ATTRIBUTE_PDF ON TB_ITEM_ATTRIBUTE_PDF.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
                LEFT JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
                INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = tb_ITEM_ATTRIBUTE.ITEM_ID
                INNER JOIN TB_ITEM_REVIEW ir on TB_ITEM.ITEM_ID = ir.ITEM_ID and ir.IS_INCLUDED = 1 and ir.IS_DELETED = 0
                INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
                INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
                        AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = tb_ITEM_ATTRIBUTE.ATTRIBUTE_ID

                WHERE TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID AND IS_COMPLETED = 'TRUE'
					AND TB_ITEM_ATTRIBUTE_TEXT .ITEM_ATTRIBUTE_ID is null
					AND TB_ITEM_ATTRIBUTE_PDF.ITEM_ATTRIBUTE_ID is null
					and ADDITIONAL_TEXT is not null
					AND LTRIM ( RTRIM(ADDITIONAL_TEXT )) != ''

UNION
SELECT TB_ITEM.TITLE, SHORT_TITLE, TB_ITEM.ITEM_ID, ADDITIONAL_TEXT, TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID, DOCUMENT_TITLE
				, TEXT_FROM, TEXT_TO, 0 as [PAGE]
                ,        SUBSTRING(
                        replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
                        ) CODED_TEXT
                ,'Text' as [ORIGIN]
                FROM tb_ITEM_ATTRIBUTE
                INNER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
                INNER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
                INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = tb_ITEM_ATTRIBUTE.ITEM_ID
                INNER JOIN TB_ITEM_REVIEW ir on TB_ITEM.ITEM_ID = ir.ITEM_ID and ir.IS_INCLUDED = 1 and ir.IS_DELETED = 0
                INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
                INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
                        AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = tb_ITEM_ATTRIBUTE.ATTRIBUTE_ID

                WHERE TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID AND IS_COMPLETED = 'TRUE'
UNION
SELECT TB_ITEM.TITLE, SHORT_TITLE, TB_ITEM.ITEM_ID, ADDITIONAL_TEXT, TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID, DOCUMENT_TITLE
				, 0 as [TEXT_FROM], 0 as [TEXT_TO], PAGE
                ,'Page ' + CONVERT(varchar(10),PAGE) + ':' + CHAR(10) + '[¬s]"' + replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"'
					AS [CODED_TEXT]
                ,'Pdf' as [ORIGIN]
                FROM tb_ITEM_ATTRIBUTE
                INNER JOIN TB_ITEM_ATTRIBUTE_PDF ON TB_ITEM_ATTRIBUTE_PDF.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
                INNER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_PDF.ITEM_DOCUMENT_ID
                INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = tb_ITEM_ATTRIBUTE.ITEM_ID
                INNER JOIN TB_ITEM_REVIEW ir on TB_ITEM.ITEM_ID = ir.ITEM_ID and ir.IS_INCLUDED = 1 and ir.IS_DELETED = 0
                INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
                INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
                        AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = tb_ITEM_ATTRIBUTE.ATTRIBUTE_ID

                WHERE TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID AND IS_COMPLETED = 'TRUE'
        ORDER BY SHORT_TITLE, TB_ITEM.ITEM_ID, ITEM_DOCUMENT_ID, ORIGIN, TEXT_FROM, PAGE
        
SET NOCOUNT OFF
RETURN
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributesContactFullTextDetailsList]    Script Date: 02/10/2026 10:15:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemAttributesContactFullTextDetailsList] 
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON
	Declare @ItemSetIDs Table(SET_ID int primary key,ITEM_SET_ID bigint)--pre build list of concerned IDs
	--insert all completed items
	insert into @ItemSetIDs select s.SET_ID, Item_set_id from TB_ITEM_SET	tis
		inner join TB_SET s on tis.SET_ID = s.SET_ID and tis.ITEM_ID = @ITEM_ID and tis.IS_COMPLETED = 1
		inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @REVIEW_ID  and s.SET_ID = rs.SET_ID
	--insert the uncompleded items that belong to the user and are not in the temp table already
	insert into @ItemSetIDs select s.SET_ID, tis.ITEM_SET_ID from TB_ITEM_SET tis
		inner join TB_SET s on tis.SET_ID = s.SET_ID and tis.ITEM_ID = @ITEM_ID and tis.CONTACT_ID = @CONTACT_ID and tis.IS_COMPLETED = 0
		inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @REVIEW_ID  and s.SET_ID = rs.SET_ID
		where tis.SET_ID not in (select SET_ID from @ItemSetIDs)
	SELECT  tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, p.ITEM_ATTRIBUTE_PDF_ID as [ID]
			, 'Page ' + CONVERT
							(varchar(10),PAGE) 
							+ ':' + CHAR(10) + '[¬s]"' 
							+ replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' 
				as [TEXT] 
			, NULL as [TEXT_FROM], NULL as [TEXT_TO]
			, 1 as IS_FROM_PDF
			,IA.ITEM_ARM_ID
			, CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END AS [ARM_NAME]
		from @ItemSetIDs tis
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID --and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE_PDF p on ia.ITEM_ATTRIBUTE_ID = p.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on p.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
		left join TB_ITEM_ARM iarm on ia.ITEM_ARM_ID = iarm.ITEM_ARM_ID
	UNION
	SELECT tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, t.ITEM_ATTRIBUTE_TEXT_ID as [ID]
			, SUBSTRING(
					replace(id.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) 
				 as [TEXT]
			, TEXT_FROM, TEXT_TO 
			, 0 as IS_FROM_PDF
			,IA.ITEM_ARM_ID
			, CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END AS [ARM_NAME]
		from @ItemSetIDs tis
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID --and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE_TEXT t on ia.ITEM_ATTRIBUTE_ID = t.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on t.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
		left join TB_ITEM_ARM iarm on ia.ITEM_ARM_ID = iarm.ITEM_ARM_ID
	ORDER by IS_FROM_PDF, [TEXT]	
	
SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSetDataList]    Script Date: 02/10/2026 10:18:50 ******/
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
/****** Object:  StoredProcedure [dbo].[st_QuickCodingReportCodingData]    Script Date: 02/10/2026 10:19:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[st_QuickCodingReportCodingData] 
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

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReportAllCodingCommand]    Script Date: 02/10/2026 10:20:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[st_ReportAllCodingCommand] (
        @ReviewId int
		,@SetId int
		,@AttributeSetId bigint
)
AS
BEGIN
	SET NOCOUNT ON


	--declare @ReviewId int = 99 --7
	--	 ,@SetId int = 894 --664 --27 --1851

	declare @fa table (a_id bigint, p_id bigint, done bit, a_order int, a_name nvarchar(500), full_path nvarchar(max) null, level int null)
	declare @items table (ItemId bigint, ItemSet int, ContactId int, ContactName varchar(255), Completed bit, [State] varchar(25), primary key(ItemId, ItemSet))

	insert into @fa (a_id, p_id, done, a_order, a_name)
		Select a.ATTRIBUTE_ID, tas.PARENT_ATTRIBUTE_ID, 0, tas.ATTRIBUTE_ORDER, a.ATTRIBUTE_NAME from TB_ATTRIBUTE a
			inner join TB_ATTRIBUTE_SET tas on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and dbo.fn_IsAttributeInTree(a.ATTRIBUTE_ID) = 1
			inner join TB_REVIEW_SET rs on tas.SET_ID = rs.SET_ID and rs.SET_ID = @SetId


	update f set done = 1, full_path =  f.a_name
	from @fa f
	 where done = 0  and p_id = 0

	--select * from @fa order by p_id, a_order
	declare @levind int = 0
	declare @todoLines int = (select count(*) from @fa where done=0)
	while (@todoLines > 0 AND @levind < 20)
	BEGIN
		set @levind = @levind + 1
		update f1 
		set done = 1, full_path = f2.full_path + '\' + f1.a_name , level = @levind + 1
		from @fa f1 
			inner join @fa f2 on f1.p_id = f2.a_id and f2.done = 1
		where f1.done = 0

		set @todoLines = (select count(*) from @fa where done=0) 
	END
	update @fa set level = 1 where level is null and done = 1
	select * from @fa order by [level], p_id, a_order
	if @AttributeSetId < 1
	BEGIN
		insert into @items SELECT distinct tis.item_id, tis.ITEM_SET_ID, tis.CONTACT_ID, c.CONTACT_NAME, tis.IS_COMPLETED 
		,case 
			when ir.IS_INCLUDED = 1 and ir.IS_DELETED = 0 then '(I) Included'
			when ir.IS_INCLUDED = 0 and ir.IS_DELETED = 0 then '(E) Excluded'
			when ir.IS_INCLUDED = 1 and ir.IS_DELETED = 1 and ir.MASTER_ITEM_ID is not null then '(S) Duplicate'
			when ir.IS_INCLUDED = 1 and ir.IS_DELETED = 1 then '(S) In deleted source'
			when ir.IS_INCLUDED = 0 and ir.IS_DELETED = 1 then '(D) Deleted'
			else ''
		end AS [STATE]
		from tb_item_set tis
			inner join TB_ITEM_REVIEW ir on tis.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @ReviewId and tis.SET_ID = @SetId
			inner join TB_CONTACT c on tis.CONTACT_ID = c.CONTACT_ID
	END
	ELSE
	BEGIN
		insert into @items SELECT distinct tis.item_id, tis.ITEM_SET_ID, tis.CONTACT_ID, c.CONTACT_NAME, tis.IS_COMPLETED 
		,case 
			when ir.IS_INCLUDED = 1 and ir.IS_DELETED = 0 then '(I) Included'
			when ir.IS_INCLUDED = 0 and ir.IS_DELETED = 0 then '(E) Excluded'
			when ir.IS_INCLUDED = 1 and ir.IS_DELETED = 1 and ir.MASTER_ITEM_ID is not null then '(S) Duplicate'
			when ir.IS_INCLUDED = 1 and ir.IS_DELETED = 1 then '(S) In deleted source'
			when ir.IS_INCLUDED = 0 and ir.IS_DELETED = 1 then '(D) Deleted'
			else ''
		end AS [STATE]
		from tb_item_set tis
			inner join TB_ITEM_REVIEW ir on tis.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @ReviewId and tis.SET_ID = @SetId
			inner join TB_CONTACT c on tis.CONTACT_ID = c.CONTACT_ID
			inner join TB_ITEM_ATTRIBUTE ia on ir.ITEM_ID = ia.ITEM_ID
			inner join TB_ITEM_SET tis2 on ia.ITEM_SET_ID = tis2.ITEM_SET_ID and tis2.IS_COMPLETED = 1
			inner join TB_ATTRIBUTE_SET tas on ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and tas.ATTRIBUTE_SET_ID = @AttributeSetId
	END

	select i.*, tia.ATTRIBUTE_ID, tia.ITEM_ATTRIBUTE_ID, tia.ITEM_ARM_ID, fa.a_name, arm.ARM_NAME, ii.SHORT_TITLE, ii.TITLE, tia.ADDITIONAL_TEXT from @items i 
		inner join TB_ITEM_ATTRIBUTE tia on i.ItemId = tia.ITEM_ID and i.ItemSet = tia.ITEM_SET_ID
		inner join @fa fa on tia.ATTRIBUTE_ID = fa.a_id
		inner join TB_ITEM ii on i.ItemId = ii.ITEM_ID
		left join TB_ITEM_ARM arm on arm.ITEM_ARM_ID = tia.ITEM_ARM_ID 
		order by ii.SHORT_TITLE, I.ItemId, i.ContactId, level, p_id, a_order

	select i.*, tia.ATTRIBUTE_ID, tia.ITEM_ATTRIBUTE_ID, p.PAGE, replace( SELECTION_TEXTS, '¬', '<br />') as [TEXT], d.DOCUMENT_TITLE from @items i 
		inner join TB_ITEM_ATTRIBUTE tia on i.ItemId = tia.ITEM_ID and i.ItemSet = tia.ITEM_SET_ID
		inner join TB_ITEM_ATTRIBUTE_PDF p on tia.ITEM_ATTRIBUTE_ID = p.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT d on p.ITEM_DOCUMENT_ID = d.ITEM_DOCUMENT_ID
	

	select TB_ITEM.ITEM_ID, i.Completed, i.ContactName, i.ContactId, OUTCOME_ID, SHORT_TITLE, TB_ITEM_OUTCOME.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
			ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, OUTCOME_DESCRIPTION,
			DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
			A1.ATTRIBUTE_NAME AS INTERVENTION_TEXT,
			a2.ATTRIBUTE_NAME AS CONTROL_TEXT,
			a3.ATTRIBUTE_NAME AS OUTCOME_TEXT,
			0 as META_ANALYSIS_OUTCOME_ID -- Meta-analysis id. 0 as not selected
		,	TB_ITEM_OUTCOME.ITEM_TIMEPOINT_ID
		,	TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP1
		,	TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP2
		,	CONCAT(TB_ITEM_TIMEPOINT.TIMEPOINT_VALUE, ' ', TB_ITEM_TIMEPOINT.TIMEPOINT_METRIC) TimepointDisplayValue
		,	arm1.ARM_NAME grp1ArmName
		,	arm2.ARM_NAME grp2ArmName
		FROM @items i
		inner join TB_ITEM_OUTCOME on i.ItemSet = TB_ITEM_OUTCOME.ITEM_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_OUTCOME.ITEM_SET_ID
		INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
		left outer JOIN TB_ATTRIBUTE IA1 ON IA1.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_INTERVENTION
		left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = IA1.ATTRIBUTE_ID 
		left outer JOIN TB_ATTRIBUTE IA2 ON IA2.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_CONTROL
		left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
		left outer JOIN TB_ATTRIBUTE IA3 ON IA3.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_OUTCOME
		left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = IA3.ATTRIBUTE_ID 
		left outer join TB_ITEM_TIMEPOINT ON TB_ITEM_TIMEPOINT.ITEM_TIMEPOINT_ID = TB_ITEM_OUTCOME.ITEM_TIMEPOINT_ID
		left outer join TB_ITEM_ARM arm1 ON arm1.ITEM_ARM_ID = TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP1
		left outer join TB_ITEM_ARM arm2 on arm2.ITEM_ARM_ID = TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP2
	
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReportData]    Script Date: 02/10/2026 10:20:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER       PROCEDURE [dbo].[st_ReportData]
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
			SELECT DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
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
			-- new addition
			inner join TB_REVIEW_SET rs ON tas.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
			-- added by Jeff 16/07/2025 to avoid orphan codes in question reports
			where dbo.fn_IsAttributeInTree(cc.ATTRIBUTE_ID) = 1 OR cc.ATTRIBUTE_ID = 0
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
			-- new addition
			inner join TB_REVIEW_SET rs ON tas.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
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
	inner join TB_ITEM_ATTRIBUTE_PDF pdf on ia.ITEM_ATTRIBUTE_ID = pdf.ITEM_ATTRIBUTE_ID
	inner join TB_ITEM_DOCUMENT id on id.ITEM_DOCUMENT_ID = pdf.ITEM_DOCUMENT_ID
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
	inner join TB_ITEM_ATTRIBUTE_TEXT txt on ia.ITEM_ATTRIBUTE_ID = txt.ITEM_ATTRIBUTE_ID
	inner join TB_ITEM_DOCUMENT id on id.ITEM_DOCUMENT_ID = txt.ITEM_DOCUMENT_ID
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

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReportExecute]    Script Date: 02/10/2026 10:20:40 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[st_ReportExecute]
(
	@REVIEW_ID INT
,	@ITEM_IDS NVARCHAR(MAX)
,	@REPORT_ID INT
,	@ORDER_BY NVARCHAR(15)
,	@ATTRIBUTE_ID BIGINT
,	@SET_ID INT

)
AS
SET NOCOUNT ON

DECLARE @TT TABLE
	(
	  ITEM_ID BIGINT
	)

-- FIRST GET THE LIST OF ITEM_IDs THAT WE'RE USING INTO THE TEMPORARY TABLE: THEY CAN EITHER BE IN
-- THE @ITEM_IDS VARIABLE, OR THE RESULT OF A SEARCH ON THE @ATTRIBUTE_SET_ID

IF @ATTRIBUTE_ID != 0
BEGIN
	INSERT INTO @TT
		SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.IS_DELETED != 'TRUE' 
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
END
ELSE
BEGIN
	INSERT INTO @TT
		SELECT VALUE FROM dbo.fn_Split_int(@ITEM_IDS, ',')
END

-- GET THE NAMES OF THE COLUMNS AS THE FIRST RESULT FROM THE READER 
SELECT * FROM TB_REPORT_COLUMN WHERE REPORT_ID = @REPORT_ID
ORDER BY COLUMN_ORDER

-- 2ND RESULT FROM READER = THE DATA
IF (@ORDER_BY LIKE 'Short title')
BEGIN
	select TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, ATTRIBUTE_NAME, ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
		SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT
		, 'Page ' + CONVERT(varchar(10),PAGE) + ':' + CHAR(10) + '[¬s]"' + replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' PDF_TEXT
		,DOCUMENT_TITLE
 	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
		AND TB_ATTRIBUTE_SET.SET_ID = TB_REPORT_COLUMN_CODE.SET_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_PDF ON TB_ITEM_ATTRIBUTE_PDF.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
										OR TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_PDF.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		AND TB_ITEM_REVIEW.IS_DELETED != 'TRUE'
	--INNER JOIN dbo.fn_Split_int(@ITEM_IDS, ',') attribute_list ON attribute_list.value = TB_ITEM_ATTRIBUTE.ITEM_ID
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, ATTRIBUTE_ORDER, DOCUMENT_TITLE, PAGE
	option (optimize for unknown)
END
ELSE
IF (@ORDER_BY LIKE 'Item Id')
BEGIN
	select TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, ATTRIBUTE_NAME, ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT
	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
		AND TB_ATTRIBUTE_SET.SET_ID = TB_REPORT_COLUMN_CODE.SET_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		AND TB_ITEM_REVIEW.IS_DELETED != 'TRUE'
	--INNER JOIN dbo.fn_Split_int(@ITEM_IDS, ',') attribute_list ON attribute_list.value = TB_ITEM_ATTRIBUTE.ITEM_ID
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	ORDER BY TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, ATTRIBUTE_ORDER
	option (optimize for unknown)
END
ELSE
BEGIN
	select TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, ATTRIBUTE_NAME, ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT
	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
		AND TB_ATTRIBUTE_SET.SET_ID = TB_REPORT_COLUMN_CODE.SET_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		AND TB_ITEM_REVIEW.IS_DELETED != 'TRUE'
	--INNER JOIN dbo.fn_Split_int(@ITEM_IDS, ',') attribute_list ON attribute_list.value = TB_ITEM_ATTRIBUTE.ITEM_ID
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)	
	ORDER BY TB_ITEM.OLD_ITEM_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, ATTRIBUTE_ORDER
	option (optimize for unknown)
END


SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbItemSetDataList]    Script Date: 02/10/2026 10:21:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   procedure [dbo].[st_WebDbItemSetDataList] (
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

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReportExecuteSingleWithOutcomes]    Script Date: 02/10/2026 10:24:24 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[st_ReportExecuteSingleWithOutcomes]
(
	@REVIEW_ID INT
,	@ITEM_IDS NVARCHAR(MAX)
,	@REPORT_ID INT
,	@ORDER_BY NVARCHAR(15)
,	@ATTRIBUTE_ID BIGINT
,	@SET_ID INT

)
AS
SET NOCOUNT ON

DECLARE @TT TABLE
	(
	  ITEM_ID BIGINT
	)

-- FIRST GET THE LIST OF ITEM_IDs THAT WE'RE USING INTO THE TEMPORARY TABLE: THEY CAN EITHER BE IN
-- THE @ITEM_IDS VARIABLE, OR THE RESULT OF A SEARCH ON THE @ATTRIBUTE_SET_ID

IF @ATTRIBUTE_ID != 0
BEGIN
	INSERT INTO @TT
		SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.IS_DELETED != 'TRUE'
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
END
ELSE
BEGIN
	INSERT INTO @TT
		SELECT VALUE FROM dbo.fn_Split_int(@ITEM_IDS, ',')
END

-- GET THE NAMES OF THE COLUMNS AS THE FIRST RESULT FROM THE READER 
SELECT * FROM TB_REPORT_COLUMN WHERE REPORT_ID = @REPORT_ID
ORDER BY COLUMN_ORDER

-- 2ND RESULT: THE LIST OF ATTRIBUTES THAT HAVE BEEN APPLIED TO OUTCOMES IN THE MAIN DATA
SELECT DISTINCT ATTRIBUTE_NAME FROM TB_ATTRIBUTE
	INNER JOIN TB_ITEM_OUTCOME_ATTRIBUTE ON TB_ITEM_OUTCOME_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_OUTCOME ON TB_ITEM_OUTCOME.OUTCOME_ID = TB_ITEM_OUTCOME_ATTRIBUTE.OUTCOME_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_OUTCOME.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	WHERE TB_ITEM_SET.ITEM_ID IN (SELECT ITEM_ID FROM @TT)

-- 3RD RESULT FROM READER = THE DATA
IF (@ORDER_BY LIKE 'Short title')
BEGIN
	select distinct TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT,
		TB_ITEM_OUTCOME.OUTCOME_TITLE, AT2.ATTRIBUTE_NAME OUTCOME_ATTRIBUTE, TB_ITEM_OUTCOME.OUTCOME_ID, CODE_ORDER,
		TB_ITEM_ATTRIBUTE.ITEM_ARM_ID, CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END

	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID and dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	LEFT OUTER JOIN TB_ITEM_ARM ON TB_ITEM_ARM.ITEM_ARM_ID = TB_ITEM_ATTRIBUTE.ITEM_ARM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_REVIEW_SET RS on RS.SET_ID = TB_ITEM_SET.SET_ID AND RS.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	
	INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_ID = TB_ITEM_SET.ITEM_ID
		AND IS2.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_REVIEW_SET RS2 ON RS2.SET_ID = IS2.SET_ID AND RS2.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM_OUTCOME ON TB_ITEM_OUTCOME.ITEM_SET_ID = IS2.ITEM_SET_ID
	LEFT OUTER JOIN TB_ITEM_OUTCOME_ATTRIBUTE ON TB_ITEM_OUTCOME_ATTRIBUTE.OUTCOME_ID = TB_ITEM_OUTCOME.OUTCOME_ID
	LEFT OUTER JOIN TB_ATTRIBUTE AT2 ON AT2.ATTRIBUTE_ID = TB_ITEM_OUTCOME_ATTRIBUTE.ATTRIBUTE_ID
	
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	--ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, OUTCOME_TITLE
	ORDER BY SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, OUTCOME_ID
END

ELSE
IF (@ORDER_BY LIKE 'Item Id')
BEGIN
	select distinct TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT,
		TB_ITEM_OUTCOME.OUTCOME_TITLE, AT2.ATTRIBUTE_NAME OUTCOME_ATTRIBUTE, TB_ITEM_OUTCOME.OUTCOME_ID, CODE_ORDER,
		TB_ITEM_ATTRIBUTE.ITEM_ARM_ID, CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END

	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID and dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	LEFT OUTER JOIN TB_ITEM_ARM ON TB_ITEM_ARM.ITEM_ARM_ID = TB_ITEM_ATTRIBUTE.ITEM_ARM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_REVIEW_SET RS on RS.SET_ID = TB_ITEM_SET.SET_ID AND RS.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	
	INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_ID = TB_ITEM_SET.ITEM_ID
		AND IS2.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_REVIEW_SET RS2 ON RS2.SET_ID = IS2.SET_ID AND RS2.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM_OUTCOME ON TB_ITEM_OUTCOME.ITEM_SET_ID = IS2.ITEM_SET_ID
	LEFT OUTER JOIN TB_ITEM_OUTCOME_ATTRIBUTE ON TB_ITEM_OUTCOME_ATTRIBUTE.OUTCOME_ID = TB_ITEM_OUTCOME.OUTCOME_ID
	LEFT OUTER JOIN TB_ATTRIBUTE AT2 ON AT2.ATTRIBUTE_ID = TB_ITEM_OUTCOME_ATTRIBUTE.ATTRIBUTE_ID
	
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	--ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, OUTCOME_TITLE
	ORDER BY TB_ITEM_ATTRIBUTE.ITEM_ID, OUTCOME_ID
END
ELSE
BEGIN
	select distinct TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT,
		TB_ITEM_OUTCOME.OUTCOME_TITLE, AT2.ATTRIBUTE_NAME OUTCOME_ATTRIBUTE, TB_ITEM_OUTCOME.OUTCOME_ID, CODE_ORDER,
		TB_ITEM_ATTRIBUTE.ITEM_ARM_ID, CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END

	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID and dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	LEFT OUTER JOIN TB_ITEM_ARM ON TB_ITEM_ARM.ITEM_ARM_ID = TB_ITEM_ATTRIBUTE.ITEM_ARM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_REVIEW_SET RS on RS.SET_ID = TB_ITEM_SET.SET_ID AND RS.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	
	INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_ID = TB_ITEM_SET.ITEM_ID
		AND IS2.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_REVIEW_SET RS2 ON RS2.SET_ID = IS2.SET_ID AND RS2.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM_OUTCOME ON TB_ITEM_OUTCOME.ITEM_SET_ID = IS2.ITEM_SET_ID
	LEFT OUTER JOIN TB_ITEM_OUTCOME_ATTRIBUTE ON TB_ITEM_OUTCOME_ATTRIBUTE.OUTCOME_ID = TB_ITEM_OUTCOME.OUTCOME_ID
	LEFT OUTER JOIN TB_ATTRIBUTE AT2 ON AT2.ATTRIBUTE_ID = TB_ITEM_OUTCOME_ATTRIBUTE.ATTRIBUTE_ID
	
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	--ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, OUTCOME_TITLE
	ORDER BY OLD_ITEM_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, OUTCOME_ID
END


SET NOCOUNT OFF
GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReportExecuteSingleWithoutOutcomes]    Script Date: 02/10/2026 10:24:42 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[st_ReportExecuteSingleWithoutOutcomes]
(
	@REVIEW_ID INT
,	@ITEM_IDS NVARCHAR(MAX)
,	@REPORT_ID INT
,	@ORDER_BY NVARCHAR(15)
,	@ATTRIBUTE_ID BIGINT
,	@SET_ID INT

)
AS
SET NOCOUNT ON

DECLARE @TT TABLE
	(
	  ITEM_ID BIGINT
	)

-- FIRST GET THE LIST OF ITEM_IDs THAT WE'RE USING INTO THE TEMPORARY TABLE: THEY CAN EITHER BE IN
-- THE @ITEM_IDS VARIABLE, OR THE RESULT OF A SEARCH ON THE @ATTRIBUTE_SET_ID

IF @ATTRIBUTE_ID != 0
BEGIN
	INSERT INTO @TT
		SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.IS_DELETED != 'TRUE'
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
END
ELSE
BEGIN
	INSERT INTO @TT
		SELECT VALUE FROM dbo.fn_Split_int(@ITEM_IDS, ',')
END

-- GET THE NAMES OF THE COLUMNS AS THE FIRST RESULT FROM THE READER 
SELECT * FROM TB_REPORT_COLUMN WHERE REPORT_ID = @REPORT_ID
ORDER BY COLUMN_ORDER

-- 2nd RESULT FROM READER = THE DATA
IF (@ORDER_BY LIKE 'Short title')
BEGIN
	select distinct TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT, CODE_ORDER

	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	--ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, OUTCOME_TITLE
	ORDER BY SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID
END

ELSE
IF (@ORDER_BY LIKE 'Item Id')
BEGIN
	select distinct TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT, CODE_ORDER

	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	--ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, OUTCOME_TITLE
	ORDER BY TB_ITEM_ATTRIBUTE.ITEM_ID
END
ELSE
BEGIN
	select distinct TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT, CODE_ORDER

	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	--ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, OUTCOME_TITLE
	ORDER BY OLD_ITEM_ID, TB_ITEM_ATTRIBUTE.ITEM_ID
END


SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SourceFromReview_ID_Extended]    Script Date: 02/10/2026 10:25:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: 29-06-09
-- Description:	Gets Sources from Review_ID
-- =============================================
ALTER PROCEDURE [dbo].[st_SourceFromReview_ID_Extended] 
	-- Add the parameters for the stored procedure here
	@revID int = 0 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @tt table
	(
		SOURCE_NAME nvarchar(255)
		,IS_DELETED bit
		,Source_ID int
		,DATE_OF_SEARCH date
		,DATE_OF_IMPORT date
		,SOURCE_DATABASE nvarchar(200)
		,SEARCH_DESCRIPTION nvarchar(4000)
		,SEARCH_STRING nvarchar(1000)
		,NOTES nvarchar(4000)
		,IMPORT_FILTER nvarchar(60)
		,REVIEW_ID int
		
	)
	declare @t1 table
	(
		Source_ID int
		,[Total_Items] int NULL
		, [Deleted_Items] int NULL
	)
	declare @t2 table
	(
		Source_ID int
		,CODES int NULL
		,IDUCTIVE_CODES int NULL
	)
	declare @t3 table
	(
		Source_ID int
		,[Attached Files]  int NULL
		,OUTCOMES  int NULL
	)
	declare @t4 table
	(
		Source_ID int
		,DUPLICATES int NULL
		,isMasterOf  int NULL
	)

	insert into @tt
	(	
		SOURCE_NAME
		,[REVIEW_ID]
		,[Source_ID]
		,[IS_DELETED]
		,[DATE_OF_SEARCH]
		,[DATE_OF_IMPORT]
		,[SOURCE_DATABASE]
		,[SEARCH_DESCRIPTION]
		,[SEARCH_STRING]
		,[NOTES]
		,[IMPORT_FILTER]
	)
	SELECT SOURCE_NAME
		,[REVIEW_ID]
		,SOURCE_ID
		,[IS_DELETED]
		,[DATE_OF_SEARCH]
		,[DATE_OF_IMPORT]
		,[SOURCE_DATABASE]
		,[SEARCH_DESCRIPTION]
		,[SEARCH_STRING]
		,ts.[NOTES]
		,tif.IMPORT_FILTER_NAME
	from TB_SOURCE ts Left outer join TB_IMPORT_FILTER tif on ts.IMPORT_FILTER_ID = tif.IMPORT_FILTER_ID
	where REVIEW_ID = @revID


	insert into @t1 
		SELECT tt.source_id 
		,COUNT(distinct(tis.ITEM_ID)) 'Total_Items'
		,sum(CASE WHEN ir.IS_DELETED = 1 then 1 else 0 END) 
		--, (SELECT COUNT(distinct(ttis.ITEM_ID)) from TB_ITEM_REVIEW ttir
		--		inner join TB_ITEM_SOURCE ttis on ttis.ITEM_ID = ttir.ITEM_ID
		--		where ttis.SOURCE_ID = tt.SOURCE_ID and ttir.IS_DELETED = 1) 'Deleted_Items'
		from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
		WHERE ir.REVIEW_ID = @revID
		group by tt.Source_ID

	Insert into @t2
	(Source_ID, CODES, IDUCTIVE_CODES)  (select source_id, 0 ,0 from @tt)
	update @t2   set CODES = sub.CODES, IDUCTIVE_CODES = sub.IDUCTIVE_CODES
	from
		(
			SELECT 
			tt.source_id SSID
			,COUNT(distinct(ia.ITEM_ATTRIBUTE_ID)) CODES 
			,COUNT(distinct(iat.ITEM_ATTRIBUTE_TEXT_ID)) IDUCTIVE_CODES
			--,COUNT(distinct(tid.ITEM_DOCUMENT_ID)) [Attached Files]
			--,COUNT(distinct(tio.OUTCOME_ID)) OUTCOMES  
			from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
				inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
				inner join TB_REVIEW_SET rs on rs.REVIEW_ID = tt.REVIEW_ID
				inner join TB_ATTRIBUTE_SET tas on rs.SET_ID = tas.SET_ID
				inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_ID = ia.ITEM_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
				left outer join TB_ITEM_ATTRIBUTE_TEXT iat on ia.ITEM_ATTRIBUTE_ID = iat.ITEM_ATTRIBUTE_ID
				--left outer join TB_ITEM_DOCUMENT tid on tid.ITEM_ID = tis.ITEM_ID
				--left outer join TB_ITEM_SET tes on tis.ITEM_ID = tes.ITEM_ID 
				--left outer join TB_ITEM_OUTCOME tio on tio.ITEM_SET_ID = tes.ITEM_SET_ID 
			WHERE ir.REVIEW_ID = @revID
			
			group by tt.Source_ID
			
		) sub
		
		where sub.SSID = Source_ID
		OPTION (OPTIMIZE FOR UNKNOWN)

	Insert into @t3
		SELECT tt.source_id
		--,COUNT(distinct(ia.ITEM_ATTRIBUTE_ID)) CODES 
		--,COUNT(distinct(iat.ITEM_ATTRIBUTE_TEXT_ID)) IDUCTIVE_CODES
		,COUNT(distinct(tid.ITEM_DOCUMENT_ID)) [Attached Files]
		,COUNT(distinct(tio.OUTCOME_ID)) OUTCOMES  
		from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
			--left outer join TB_REVIEW_SET rs on rs.REVIEW_ID = tt.REVIEW_ID
			--left outer join TB_ATTRIBUTE_SET tas on rs.SET_ID = tas.SET_ID
			--left outer join TB_ITEM_ATTRIBUTE ia on tis.ITEM_ID = ia.ITEM_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
			--left outer join TB_ITEM_ATTRIBUTE_TEXT iat on ia.ITEM_ATTRIBUTE_ID = iat.ITEM_ATTRIBUTE_ID
			left outer join TB_ITEM_DOCUMENT tid on tid.ITEM_ID = tis.ITEM_ID
			left outer join TB_ITEM_SET tes on tis.ITEM_ID = tes.ITEM_ID 
			left outer join TB_ITEM_OUTCOME tio on tio.ITEM_SET_ID = tes.ITEM_SET_ID 
		WHERE ir.REVIEW_ID = @revID
		group by tt.Source_ID

	Insert into @t4
		SELECT
		tt.source_id
		--,(COUNT(distinct(dup.ITEM_DUPLICATES_ID)) + COUNT(distinct(dup2.ITEM_DUPLICATES_ID))) DUPLICATES
		--,COUNT(distinct(ir2.ITEM_REVIEW_ID)) isMasterOf
		,sum(CASE WHEN (
				ir.IS_DELETED = 1 and ir.is_included = 1 AND ir.MASTER_ITEM_ID is NOT null
			) then 1 else 0 END) as 'Duplicates'
		,COUNT(distinct ir2.MASTER_ITEM_ID)
		from 
			@tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
			left outer join TB_ITEM_REVIEW ir2 on ir2.MASTER_ITEM_ID = ir.ITEM_ID and ir2.REVIEW_ID = @revID
		WHERE ir.REVIEW_ID = @revID
		
		group by tt.Source_ID
		--OPTION (OPTIMIZE FOR UNKNOWN)
	select 
		SOURCE_NAME
		,[Total_Items]
		,[Deleted_Items]
		,IS_DELETED
		,t1.Source_ID
		,DATE_OF_SEARCH
		,DATE_OF_IMPORT
		,SOURCE_DATABASE
		,SEARCH_DESCRIPTION
		,SEARCH_STRING
		,NOTES
		,IMPORT_FILTER
		,REVIEW_ID
		,CODES
		,IDUCTIVE_CODES
		,[Attached Files]
		,DUPLICATES
		,isMasterOf
		,OUTCOMES
	from @tt tt
	inner join @t1 t1 on tt.Source_ID = t1.Source_ID
	inner join @t2 t2 on tt.Source_ID = t2.Source_ID
	inner join @t3 t3 on tt.Source_ID = t3.Source_ID
	inner join @t4 t4 on tt.Source_ID = t4.Source_ID
	order by Source_ID
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ZoteroERWebReviewItemList]    Script Date: 02/10/2026 10:26:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER       Procedure [dbo].[st_ZoteroERWebReviewItemList]
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
/****** Object:  StoredProcedure [dbo].[st_ZoteroItemDocumentCreate]    Script Date: 02/10/2026 10:27:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER     Procedure [dbo].[st_ZoteroItemDocumentCreate](
@DocZoteroKey  nvarchar(50),
@ItemDocumentId  bigint )
as
Begin

	INSERT INTO [dbo].[TB_ZOTERO_ITEM_DOCUMENT]([DocZoteroKey], [ItemDocumentId])
	VALUES( @DocZoteroKey, @ItemDocumentId)
	   
End
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ZoteroItemDocumentDeleteInBulk]    Script Date: 02/10/2026 10:27:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER     Procedure [dbo].[st_ZoteroItemDocumentDeleteInBulk](
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
/****** Object:  StoredProcedure [dbo].[st_ZoteroRebuildItemLinks]    Script Date: 02/10/2026 10:28:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_ZoteroRebuildItemLinks] 
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


