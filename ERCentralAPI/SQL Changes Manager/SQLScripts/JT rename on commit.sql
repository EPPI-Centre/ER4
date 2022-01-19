USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierGetClassificationDataToSQL]    Script Date: 16/01/2022 18:22:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   procedure [dbo].[st_ClassifierGetClassificationDataToSQL]
(
	@REVIEW_ID INT
,	@ATTRIBUTE_ID_CLASSIFY_TO BIGINT = NULL
,	@SOURCE_ID INT = NULL
,	@BatchGuid varchar(50) = NULL
,	@ReviewId int = NULL
,	@ContactId int = NULL
,	@MachineName nvarchar(20) = NULL
,	@ROWCOUNT int OUTPUT
)

As

SET NOCOUNT ON

	IF @ATTRIBUTE_ID_CLASSIFY_TO > -1
	BEGIN
		insert into ER4ML.ER4ML.dbo.Itemdata(BatchGuid, item_id, title, abstract, journal, ReviewId, ContactId, MachineName)
		SELECT DISTINCT @BatchGuid, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, PARENT_TITLE, @REVIEW_ID, @ContactId, @MachineName  FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		WHERE REVIEW_ID = @REVIEW_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_CLASSIFY_TO AND IS_DELETED = 'FALSE'
	END
	ELSE
	IF @SOURCE_ID > -1
	BEGIN
		insert into ER4ML.ER4ML.dbo.Itemdata(BatchGuid, item_id, title, abstract,journal, ReviewId, ContactId, MachineName)
		SELECT DISTINCT @BatchGuid, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, PARENT_TITLE, @REVIEW_ID, @ContactId, @MachineName FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID
		INNER JOIN TB_ITEM_SOURCE ITS ON ITS.ITEM_ID = TB_ITEM.ITEM_ID AND ITS.SOURCE_ID = @SOURCE_ID
		WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED = 'FALSE'
	END
	ELSE
	IF @SOURCE_ID = -1
	BEGIN
		insert into ER4ML.ER4ML.dbo.Itemdata(BatchGuid, item_id, title, abstract,journal, ReviewId, ContactId, MachineName)
		SELECT DISTINCT @BatchGuid, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, PARENT_TITLE, @REVIEW_ID, @ContactId, @MachineName FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID and IR.REVIEW_ID = @REVIEW_ID AND IR.IS_DELETED = 'FALSE'
		LEFT OUTER JOIN TB_ITEM_SOURCE ITS ON ITS.ITEM_ID = TB_ITEM.ITEM_ID 
		LEFT OUTER JOIN TB_SOURCE TS on ITS.SOURCE_ID = TS.SOURCE_ID and IR.REVIEW_ID = TS.REVIEW_ID
		WHERE TS.SOURCE_ID  is null
	END
	ELSE
	BEGIN
		insert into ER4ML.ER4ML.dbo.Itemdata(BatchGuid, item_id, title, abstract, journal, ReviewId, ContactId, MachineName)
		SELECT DISTINCT @BatchGuid, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, PARENT_TITLE, @REVIEW_ID, @ContactId, @MachineName FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID
		WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED = 'FALSE'
	END

	set @ROWCOUNT = @@ROWCOUNT
		
SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagGetOpenAlexFolders]    Script Date: 18/01/2022 10:33:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get information - last update and whether MAG is available
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagGetOpenAlexFolders] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @j nvarchar(max)

	SELECT @j= JSON_QUERY(f.openalexfolderjson, '$.childItems')
		FROM er4ml.er4ml.dbo.OpenAlexFolder f 
	
	select name as FolderName from OPENJSON(@j)
	WITH (name nvarchar(500), type nvarchar(500))
	where type = 'Folder'

END
GO