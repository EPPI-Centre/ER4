USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClusterGetXmlAllDocs]    Script Date: 02/10/2026 09:52:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




ALTER   procedure [dbo].[st_ClusterGetXmlAllDocs]
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
inner join tb_item_review ir on ir.item_id = tb_item.item_id
inner join TB_ITEM_TO_DOCUMENT itd on ir.ITEM_ID = itd.ITEM_ID
inner join TB_ITEM_DOCUMENT d on d.ITEM_DOCUMENT_ID = itd.ITEM_DOCUMENT_ID
where ir.review_id = @REVIEW_ID
and ir.IS_INCLUDED = 'true' and ir.IS_DELETED != 'true'

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       CAST(Title as varchar(4000)) as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review ir on ir.item_id = tb_item.item_id
inner join TB_ITEM_TO_DOCUMENT itd on ir.ITEM_ID = itd.ITEM_ID
inner join TB_ITEM_DOCUMENT d on d.ITEM_DOCUMENT_ID= itd.ITEM_DOCUMENT_ID
where ir.review_id = @REVIEW_ID
and ir.IS_INCLUDED = 'true' and ir.IS_DELETED != 'true'

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		CAST(DOCUMENT_TEXT as varchar(max)) as [snippet!3]
from tb_item
inner join tb_item_review ir on ir.item_id = tb_item.item_id
inner join TB_ITEM_TO_DOCUMENT itd on ir.ITEM_ID = itd.ITEM_ID
inner join TB_ITEM_DOCUMENT d on itd.ITEM_DOCUMENT_ID = d.ITEM_DOCUMENT_ID
where ir.review_id = @REVIEW_ID
and ir.IS_INCLUDED = 'true' and ir.IS_DELETED != 'true'

order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')

GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClusterGetXmlFilteredCodeDocs]    Script Date: 02/10/2026 09:53:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   procedure [dbo].[st_ClusterGetXmlFilteredCodeDocs]
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
		inner join TB_ITEM_TO_DOCUMENT itd on tb_item.ITEM_ID = itd.ITEM_ID
		inner join TB_ITEM_DOCUMENT d on d.ITEM_DOCUMENT_ID = itd.ITEM_DOCUMENT_ID
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
		inner join TB_ITEM_TO_DOCUMENT itd on tb_item.ITEM_ID = itd.ITEM_ID
		inner join TB_ITEM_DOCUMENT d on d.ITEM_DOCUMENT_ID = itd.ITEM_DOCUMENT_ID
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
		inner join TB_ITEM_TO_DOCUMENT itd on tb_item.ITEM_ID = itd.ITEM_ID
		inner join TB_ITEM_DOCUMENT d on d.ITEM_DOCUMENT_ID = itd.ITEM_DOCUMENT_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'


order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClusterGetXmlFilteredDocs]    Script Date: 02/10/2026 09:53:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_GetItemDocumentIdsFromItemIds]    Script Date: 02/10/2026 09:53:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_GetItemDocumentIdsFromItemIds]
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
			INNER JOIN TB_ITEM_TO_DOCUMENT ID1 ON ID1.ITEM_ID = IL1.ITEM_ID_SECONDARY and IL1.ITEM_ID_PRIMARY != IL1.ITEM_ID_SECONDARY
			INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = ID1.ITEM_ID and IR.REVIEW_ID = @ReviewId
			where ID1.ITEM_ID not in (select ItemId from @t);
		insert into @t SELECT ID2.ITEM_ID FROM  @t t
			INNER JOIN TB_ITEM_LINK IL2 on IL2.ITEM_ID_SECONDARY = t.ItemId
			INNER JOIN TB_ITEM_TO_DOCUMENT ID2 ON ID2.ITEM_ID = IL2.ITEM_ID_PRIMARY and IL2.ITEM_ID_PRIMARY != IL2.ITEM_ID_SECONDARY
			INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = ID2.ITEM_ID and IR.REVIEW_ID = @ReviewId
			WHERE ID2.ITEM_ID not in (select ItemId from @t);
	END
	select distinct id.ITEM_DOCUMENT_ID, ITEM_ID, d.DOCUMENT_EXTENSION from @t t 
		inner join TB_ITEM_TO_DOCUMENT id on t.ItemId = id.ITEM_ID
		inner join TB_ITEM_DOCUMENT d on id.ITEM_DOCUMENT_ID = d.ITEM_DOCUMENT_ID;
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocument]    Script Date: 02/10/2026 09:54:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   procedure [dbo].[st_ItemDocument]
(
	@ITEM_ID int,
	@REVIEW_ID int 
)

As
SELECT id.ITEM_DOCUMENT_ID, DOCUMENT_TITLE FROM TB_ITEM_DOCUMENT id
inner join TB_ITEM_TO_DOCUMENT itd on id.ITEM_DOCUMENT_ID = itd.ITEM_DOCUMENT_ID and ITEM_ID = @ITEM_ID;
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentBin]    Script Date: 02/10/2026 09:55:02 ******/
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
	 inner JOIN TB_ITEM_TO_DOCUMENT itd on I.ITEM_DOCUMENT_ID = itd.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_REVIEW as R on itd.ITEM_ID = R.ITEM_ID
WHERE I.ITEM_DOCUMENT_ID = @DOC_ID AND REVIEW_ID = @REV_ID
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentBinInsert]    Script Date: 02/10/2026 09:56:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemDocumentBinInsert]
(
	@ITEM_ID BIGINT,
	@REVIEW_ID INT,
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

	declare @chk int = (SELECT count (ITEM_ID) from tb_item_review where ITEM_ID = @ITEM_ID and REVIEW_ID = @REVIEW_ID)
	if (@chk != 1)
	BEGIN
		Set @ItemDocumentId = -1;
		return;
	END
	SET @DOCUMENT_TEXT = replace(@DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10));
	declare @actualHash varbinary(20) = CONVERT(varbinary(20),@HashString, 1);

	INSERT INTO TB_ITEM_DOCUMENT(ITEM_ID_STALE, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_BINARY, DOCUMENT_TEXT, TXT_HASH)
	VALUES(NULL, @DOCUMENT_TITLE, @DOCUMENT_EXTENSION, @BIN, [dbo].fn_CLEAN_SIMPLE_TEXT(@DOCUMENT_TEXT),
		@actualHash);
	set @ItemDocumentId = SCOPE_IDENTITY();
	INSERT INTO TB_ITEM_TO_DOCUMENT (ITEM_ID, ITEM_DOCUMENT_ID) select @ITEM_ID, @ItemDocumentId;
	IF @ZoteroKey != ''
	BEGIN
		--?? Check this will work
		INSERT into TB_ZOTERO_ITEM_DOCUMENT(DocZoteroKey, ItemDocumentId) VALUES (@ZoteroKey, @ItemDocumentId);
	END

SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentInsert]    Script Date: 02/10/2026 09:57:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemDocumentInsert]
(
	@ITEM_ID BIGINT,
	@REVIEW_ID INT,
	@DOCUMENT_TITLE NVARCHAR(255),
	@DOCUMENT_EXTENSION NVARCHAR(5),
	@DOCUMENT_TEXT NVARCHAR(MAX),
	@ZoteroKey NVARCHAR(50) = '',
	@HashString char(42),
	@ItemDocumentId bigint = -1 output 
)

As

SET NOCOUNT ON
	declare @chk int = (SELECT count (ITEM_ID) from tb_item_review where ITEM_ID = @ITEM_ID and REVIEW_ID = @REVIEW_ID);
	if (@chk != 1)
	BEGIN
		Set @ItemDocumentId = -1;
		return;
	END
	SET @DOCUMENT_TEXT = replace(@DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10));
	declare @actualHash varbinary(20) = CONVERT(varbinary(20),@HashString, 1);

	INSERT INTO TB_ITEM_DOCUMENT(ITEM_ID_STALE, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_BINARY, DOCUMENT_TEXT, TXT_HASH)
	VALUES(NULL, @DOCUMENT_TITLE, @DOCUMENT_EXTENSION, NULL, [dbo].fn_CLEAN_SIMPLE_TEXT(@DOCUMENT_TEXT),
		@actualHash);
	set @ItemDocumentId = SCOPE_IDENTITY();
	INSERT INTO TB_ITEM_TO_DOCUMENT (ITEM_ID, ITEM_DOCUMENT_ID) select @ITEM_ID, @ItemDocumentId;
	IF @ZoteroKey != ''
	BEGIN
		--?? Check this will work
		INSERT into TB_ZOTERO_ITEM_DOCUMENT(DocZoteroKey, ItemDocumentId) VALUES (@ZoteroKey, @ItemDocumentId);
	END


SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentLinkInsert]    Script Date: 02/10/2026 09:56:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_ItemDocumentLinkInsert]
(
	@ITEM_ID BIGINT,
	@REVIEW_ID int,
	@ZoteroKey NVARCHAR(50) = '',
	@ItemDocumentId bigint
)

As

SET NOCOUNT ON
	declare @chk int = (SELECT count (ITEM_ID) from tb_item_review where ITEM_ID = @ITEM_ID and REVIEW_ID = @REVIEW_ID)
	if (@chk != 1)
	BEGIN
		Set @ItemDocumentId = -1;
		return;
	END
	INSERT INTO TB_ITEM_TO_DOCUMENT (ITEM_ID, ITEM_DOCUMENT_ID) select @ITEM_ID, @ItemDocumentId
IF @ZoteroKey != ''
BEGIN
	--?? Check this will work
	INSERT into TB_ZOTERO_ITEM_DOCUMENT(DocZoteroKey, ItemDocumentId) VALUES (@ZoteroKey, @ItemDocumentId)
END

SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentFindDuplicateCandidates]    Script Date: 02/10/2026 09:56:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_ItemDocumentFindDuplicateCandidates]
(
	@HashString char(42)
)

As

SET NOCOUNT ON
	declare @actualHash varbinary(20) = CONVERT(varbinary(20),@HashString, 1)
	SELECT * from TB_ITEM_DOCUMENT where TXT_HASH = @actualHash
	order by ITEM_DOCUMENT_ID

SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentDelete]    Script Date: 02/10/2026 09:57:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: 
-- Description:	Delete An ItemDocument and associated Item_Attribute_Text
-- =============================================
ALTER PROCEDURE [dbo].[st_ItemDocumentDelete] 
	-- Add the parameters for the stored procedure here
	@DocID bigint,
	@ItemID bigint,
	@RevID int,
	@IsLast bit = 0 OUTPUT,
	@Extension nvarchar(5) OUTPUT
AS
BEGIN
	set @Extension = 'fail';
	set @IsLast = 0;
	declare @check int = 0;
	--make sure doc and item belongs to review...
	set @check = (select count(id.ITEM_DOCUMENT_ID) from TB_ITEM_DOCUMENT id
		inner join TB_ITEM_TO_DOCUMENT itd on id.ITEM_DOCUMENT_ID = itd.ITEM_DOCUMENT_ID and itd.ITEM_ID = @ItemID
		inner join TB_ITEM_REVIEW ir on itd.ITEM_ID = ir.ITEM_ID and REVIEW_ID = @RevID and id.ITEM_DOCUMENT_ID = @DocID)
	if (@check != 1) return
	--next find out if we have multiple copies of this doc associated to different items
	set @check = (select count(ITEM_DOCUMENT_ID) from TB_ITEM_TO_DOCUMENT where ITEM_DOCUMENT_ID = @DocID)
	if (@check > 1)
	BEGIN --we only delete the doc for this item
		BEGIN TRY
			BEGIN TRANSACTION
			select @Extension = DOCUMENT_EXTENSION from TB_ITEM_DOCUMENT where ITEM_DOCUMENT_ID = @DocID;
			delete from TB_ITEM_ATTRIBUTE_TEXT where ITEM_DOCUMENT_ID = @DocID 
				and ITEM_ATTRIBUTE_ID in (select ITEM_ATTRIBUTE_ID from TB_ITEM_ATTRIBUTE where ITEM_ID = @ItemID);
			delete from TB_ITEM_ATTRIBUTE_PDF where ITEM_DOCUMENT_ID = @DocID 
				and ITEM_ATTRIBUTE_ID in (select ITEM_ATTRIBUTE_ID from TB_ITEM_ATTRIBUTE where ITEM_ID = @ItemID);
			delete from tb_ITEM_TO_DOCUMENT where ITEM_DOCUMENT_ID = @DocID and ITEM_ID = @ItemID;
			COMMIT TRANSACTION
		END TRY
		BEGIN CATCH
			IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
		END CATCH
	END
	else if (@check = 1)
	BEGIN --we delete everything and signal this fact - doc needs deleting on blob storage too
		BEGIN TRY
			BEGIN TRANSACTION
			set @IsLast = 1;
			select @Extension = DOCUMENT_EXTENSION from TB_ITEM_DOCUMENT where ITEM_DOCUMENT_ID = @DocID;
			delete from TB_ITEM_ATTRIBUTE_TEXT where ITEM_DOCUMENT_ID = @DocID 
				and ITEM_ATTRIBUTE_ID in (select ITEM_ATTRIBUTE_ID from TB_ITEM_ATTRIBUTE where ITEM_ID = @ItemID);
			delete from TB_ITEM_ATTRIBUTE_PDF where ITEM_DOCUMENT_ID = @DocID 
				and ITEM_ATTRIBUTE_ID in (select ITEM_ATTRIBUTE_ID from TB_ITEM_ATTRIBUTE where ITEM_ID = @ItemID);
			delete from tb_ITEM_TO_DOCUMENT where ITEM_DOCUMENT_ID = @DocID and ITEM_ID = @ItemID;
			delete from tb_ITEM_DOCUMENT where ITEM_DOCUMENT_ID = @DocID;
			COMMIT TRANSACTION
		END TRY
		BEGIN CATCH
			IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
		END CATCH
	END

	
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentDeleteWarning]    Script Date: 02/10/2026 09:57:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER procedure [dbo].[st_ItemDocumentDeleteWarning]
(
	@ITEM_DOCUMENT_ID bigint,
	@ITEM_ID bigint,
	@NUM_CODING int output
)

As

SET NOCOUNT ON
select @NUM_CODING = count(p.item_attribute_id) from TB_ITEM_ATTRIBUTE_PDF p
	inner join TB_ITEM_TO_DOCUMENT itd on p.ITEM_DOCUMENT_ID = itd.ITEM_DOCUMENT_ID and p.ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID and itd.ITEM_ID = @ITEM_ID
	inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_ATTRIBUTE_ID = p.ITEM_ATTRIBUTE_ID and ia.ITEM_ID = @ITEM_ID

select @NUM_CODING = @NUM_CODING + count(t.item_attribute_id) 
	from TB_ITEM_ATTRIBUTE_TEXT t
	inner join TB_ITEM_TO_DOCUMENT itd on t.ITEM_DOCUMENT_ID = itd.ITEM_DOCUMENT_ID and t.ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID and itd.ITEM_ID = @ITEM_ID
	inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_ATTRIBUTE_ID = t.ITEM_ATTRIBUTE_ID and ia.ITEM_ID = @ITEM_ID
	
SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentList]    Script Date: 02/10/2026 09:58:16 ******/
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

SELECT d.ITEM_DOCUMENT_ID, SHORT_TITLE, ID0.ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_TEXT, 1 IDX,
'DOC_BINARY' = CASE WHEN DOCUMENT_BINARY IS NULL THEN 'False' ELSE 'True' END, DOCUMENT_FREE_NOTES
FROM TB_ITEM_DOCUMENT d
INNER JOIN TB_ITEM_TO_DOCUMENT ID0 on ID0.ITEM_DOCUMENT_ID = d.ITEM_DOCUMENT_ID
INNER JOIN TB_ITEM I1 ON I1.ITEM_ID = ID0.ITEM_ID
WHERE ID0.ITEM_ID = @ITEM_ID

UNION

SELECT d.ITEM_DOCUMENT_ID, SHORT_TITLE, ID1.ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_TEXT, 2 IDX,
'DOC_BINARY' = CASE WHEN DOCUMENT_BINARY IS NULL THEN 'False' ELSE 'True' END, DOCUMENT_FREE_NOTES
FROM TB_ITEM_LINK IL1
INNER JOIN TB_ITEM_TO_DOCUMENT ID1 ON ID1.ITEM_ID = IL1.ITEM_ID_SECONDARY and IL1.ITEM_ID_PRIMARY != IL1.ITEM_ID_SECONDARY
INNER JOIN TB_ITEM_DOCUMENT d ON d.ITEM_DOCUMENT_ID = ID1.ITEM_DOCUMENT_ID
INNER JOIN TB_ITEM I2 ON I2.ITEM_ID = ID1.ITEM_ID
WHERE ITEM_ID_PRIMARY = @ITEM_ID

UNION

SELECT d.ITEM_DOCUMENT_ID, SHORT_TITLE, ID2.ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_TEXT, 2 IDX,
'DOC_BINARY' = CASE WHEN DOCUMENT_BINARY IS NULL THEN 'False' ELSE 'True' END, DOCUMENT_FREE_NOTES
FROM TB_ITEM_LINK IL2
INNER JOIN TB_ITEM_TO_DOCUMENT ID2 ON ID2.ITEM_ID = IL2.ITEM_ID_PRIMARY and IL2.ITEM_ID_PRIMARY != IL2.ITEM_ID_SECONDARY
INNER JOIN TB_ITEM_DOCUMENT d on d.ITEM_DOCUMENT_ID = ID2.ITEM_DOCUMENT_ID
INNER JOIN TB_ITEM I3 ON I3.ITEM_ID = ID2.ITEM_ID
WHERE ITEM_ID_SECONDARY = @ITEM_ID

ORDER BY IDX ASC


SET NOCOUNT OFF
GO
