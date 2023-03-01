

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierCleanUpBatch]    Script Date: 27/01/2022 10:20:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or ALTER     procedure [dbo].[st_ClassifierCleanUpBatch]
(
	@BatchGuid varchar(50)

)

As

SET NOCOUNT ON

	DECLARE	@return_value int
	EXEC	@return_value = ER4ML.ER4ML.[dbo].[CleanUpBatch] @BatchGuid = @BatchGuid
		
SET NOCOUNT OFF

GO



USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierGetClassificationDataToSQL]    Script Date: 27/01/2022 09:34:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or ALTER     procedure [dbo].[st_ClassifierGetClassificationDataToSQL]
(
	@REVIEW_ID INT
,	@ATTRIBUTE_ID_CLASSIFY_TO BIGINT = NULL
,	@ITEM_ID_LIST nvarchar(max) = NULL
,	@SOURCE_ID INT = NULL
,	@BatchGuid varchar(50) = NULL
,	@ContactId int = NULL
,	@MachineName nvarchar(20) = NULL
,	@ROWCOUNT int OUTPUT
)

As

SET NOCOUNT ON

-- Inserts into the tmp table are quick and then release tb_item etc for other work
-- Inserts into the Azure DB are slow, so better to do them from a table variable
DECLARE @TMP_ITEM TABLE
(
	BatchGuid nvarchar(50)
,	Item_id bigint
,	Title nvarchar(4000) null
,	abstract nvarchar(max) null
,	journal nvarchar(4000) null
,	ReviewId int
,	ContactId int
,	MachineName nvarchar(20)
)

	IF @ATTRIBUTE_ID_CLASSIFY_TO > -1
	BEGIN
		insert into @TMP_ITEM(BatchGuid, item_id, title, abstract, journal, ReviewId, ContactId, MachineName)
		SELECT DISTINCT @BatchGuid, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, PARENT_TITLE, @REVIEW_ID, @ContactId, @MachineName  FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		WHERE REVIEW_ID = @REVIEW_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_CLASSIFY_TO AND IS_DELETED = 'FALSE'
	END
	ELSE
	IF @ITEM_ID_LIST <> ''
	BEGIN
		insert into @TMP_ITEM(BatchGuid, item_id, title, abstract, journal, ReviewId, ContactId, MachineName)
		SELECT DISTINCT @BatchGuid, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, PARENT_TITLE, @REVIEW_ID, @ContactId, @MachineName FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID
		INNER JOIN DBO.fn_Split_int(@ITEM_ID_LIST, ',') ids ON ids.value = TB_ITEM.ITEM_ID
		WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED = 'FALSE'
	END
	ELSE
	IF @SOURCE_ID > -1
	BEGIN
		insert into @TMP_ITEM(BatchGuid, item_id, title, abstract,journal, ReviewId, ContactId, MachineName)
		SELECT DISTINCT @BatchGuid, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, PARENT_TITLE, @REVIEW_ID, @ContactId, @MachineName FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID
		INNER JOIN TB_ITEM_SOURCE ITS ON ITS.ITEM_ID = TB_ITEM.ITEM_ID AND ITS.SOURCE_ID = @SOURCE_ID
		WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED = 'FALSE'
	END
	ELSE
	IF @SOURCE_ID = -1
	BEGIN
		insert into @TMP_ITEM(BatchGuid, item_id, title, abstract,journal, ReviewId, ContactId, MachineName)
		SELECT DISTINCT @BatchGuid, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, PARENT_TITLE, @REVIEW_ID, @ContactId, @MachineName FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID and IR.REVIEW_ID = @REVIEW_ID AND IR.IS_DELETED = 'FALSE'
		LEFT OUTER JOIN TB_ITEM_SOURCE ITS ON ITS.ITEM_ID = TB_ITEM.ITEM_ID 
		LEFT OUTER JOIN TB_SOURCE TS on ITS.SOURCE_ID = TS.SOURCE_ID and IR.REVIEW_ID = TS.REVIEW_ID
		WHERE TS.SOURCE_ID  is null
	END
	ELSE
	BEGIN
		insert into @TMP_ITEM(BatchGuid, item_id, title, abstract, journal, ReviewId, ContactId, MachineName)
		SELECT DISTINCT @BatchGuid, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, PARENT_TITLE, @REVIEW_ID, @ContactId, @MachineName FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID
		WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED = 'FALSE'
	END

	set @ROWCOUNT = @@ROWCOUNT

	insert into ER4ML.ER4ML.dbo.Itemdata(BatchGuid, item_id, title, abstract, journal, ReviewId, ContactId, MachineName)
	select BatchGuid, item_id, title, abstract, journal, ReviewId, ContactId, MachineName
		from @TMP_ITEM
		
SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierGetClassificationScores]    Script Date: 27/01/2022 10:06:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or ALTER     procedure [dbo].[st_ClassifierGetClassificationScores]
(
	@BatchGuid varchar(50)
)

As

SET NOCOUNT ON

SELECT ITEM_ID, PredictedLabel, Score
		FROM ER4ML.ER4ML.dbo.ItemScore where BatchGuid = @BatchGuid
	ORDER BY Score DESC

		
SET NOCOUNT OFF

GO