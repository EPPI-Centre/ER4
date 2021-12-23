USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierGetClassificationDataToSQL]    Script Date: 22/12/2021 14:42:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or alter procedure [dbo].[st_ClassifierGetClassificationDataToSQL]
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
		insert into ER4ML.ER4ML.dbo.Itemdata(BatchGuid, item_id, title, abstract, ReviewId, ContactId, MachineName)
		SELECT DISTINCT @BatchGuid, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, @REVIEW_ID, @ContactId, @MachineName  FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		WHERE REVIEW_ID = @REVIEW_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_CLASSIFY_TO AND IS_DELETED = 'FALSE'
	END
	ELSE
	IF @SOURCE_ID > -1
	BEGIN
		insert into ER4ML.ER4ML.dbo.Itemdata(BatchGuid, item_id, title, abstract, ReviewId, ContactId, MachineName)
		SELECT DISTINCT @BatchGuid, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, @REVIEW_ID, @ContactId, @MachineName FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID
		INNER JOIN TB_ITEM_SOURCE ITS ON ITS.ITEM_ID = TB_ITEM.ITEM_ID AND ITS.SOURCE_ID = @SOURCE_ID
		WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED = 'FALSE'
	END
	ELSE
	IF @SOURCE_ID = -1
	BEGIN
		insert into ER4ML.ER4ML.dbo.Itemdata(BatchGuid, item_id, title, abstract, ReviewId, ContactId, MachineName)
		SELECT DISTINCT @BatchGuid, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, @REVIEW_ID, @ContactId, @MachineName FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID and IR.REVIEW_ID = @REVIEW_ID AND IR.IS_DELETED = 'FALSE'
		LEFT OUTER JOIN TB_ITEM_SOURCE ITS ON ITS.ITEM_ID = TB_ITEM.ITEM_ID 
		LEFT OUTER JOIN TB_SOURCE TS on ITS.SOURCE_ID = TS.SOURCE_ID and IR.REVIEW_ID = TS.REVIEW_ID
		WHERE TS.SOURCE_ID  is null
	END
	ELSE
	BEGIN
		insert into ER4ML.ER4ML.dbo.Itemdata(BatchGuid, item_id, title, abstract, ReviewId, ContactId, MachineName)
		SELECT DISTINCT @BatchGuid, TB_ITEM.ITEM_ID, TITLE, ABSTRACT, @REVIEW_ID, @ContactId, @MachineName FROM TB_ITEM
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM.ITEM_ID
		WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED = 'FALSE'
	END

	set @ROWCOUNT = @@ROWCOUNT
		
SET NOCOUNT OFF

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierInsertSearchAndScores]    Script Date: 22/12/2021 16:04:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or ALTER procedure [dbo].[st_ClassifierInsertSearchAndScores]
(
	@REVIEW_ID INT
,	@CONTACT_ID INT
,	@SearchTitle NVARCHAR(1000)
,	@BatchGuid varchar(50)
,	@PredictedLabel nvarchar(500)

)

As

SET NOCOUNT ON

	-- STEP 1: GET THE CURRENT SEARCH COUNT FOR THIS REVIEW
	DECLARE @SEARCH_NO INT
	DECLARE @NEW_SEARCH_ID INT
	SELECT @SEARCH_NO = ISNULL(MAX(SEARCH_NO), 0) + 1 FROM tb_SEARCH WHERE REVIEW_ID = @REVIEW_ID

	-- STEP 2: CREATE THE SEARCH RECORD
	INSERT INTO tb_SEARCH
	(	REVIEW_ID
	,	CONTACT_ID
	,	SEARCH_TITLE
	,	SEARCH_NO
	,	HITS_NO
	,	IS_CLASSIFIER_RESULT
	,	SEARCH_DATE
	)	
	VALUES
	(
		@REVIEW_ID
	,	@CONTACT_ID
	,	@SearchTitle
	,	@SEARCH_NO
	,	0
	,	'TRUE'
	,	GetDate()
	)
	-- Get the identity and store it
	SET @NEW_SEARCH_ID = @@identity


	-- STEP 3: PUT THE ITEMS INTO THE SEARCH LIST
	INSERT INTO TB_SEARCH_ITEM(ITEM_ID,SEARCH_ID, ITEM_RANK) 
		SELECT ITEM_ID, @NEW_SEARCH_ID, CAST(Score * 100 AS INT)
				FROM ER4ML.ER4ML.dbo.ItemScore where BatchGuid = @BatchGuid and PredictedLabel = @PredictedLabel
			ORDER BY Score DESC


	-- STEP 4: Update the hits count
	UPDATE TB_SEARCH
		SET HITS_NO = @@ROWCOUNT
		WHERE SEARCH_ID = @NEW_SEARCH_ID


	-- STEP 5: clean up Azure SQL DB (deletes information about this batch)
	DECLARE	@return_value int
	EXEC	@return_value = ER4ML.ER4ML.[dbo].[CleanUpBatch] @BatchGuid = @BatchGuid
		
SET NOCOUNT OFF

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierGetGetPredictedLabels]    Script Date: 22/12/2021 14:42:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or alter procedure [dbo].[st_ClassifierGetGetPredictedLabels]
(

	@BatchGuid varchar(50) = NULL

)

As

SET NOCOUNT ON

	DECLARE	@return_value int

	EXEC	@return_value = ER4ML.ER4ML.[dbo].[GetPredictedLabels] @BatchGuid = @BatchGuid

--SELECT	'Return Value' = @return_value
		
SET NOCOUNT OFF

GO

