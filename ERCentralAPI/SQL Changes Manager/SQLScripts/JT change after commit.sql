USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierInsertSearchAndScores]    Script Date: 04/01/2022 13:37:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or ALTER   procedure [dbo].[st_ClassifierInsertSearchAndScores]
(
	@REVIEW_ID INT
,	@CONTACT_ID INT
,	@SearchTitle NVARCHAR(1000)
,	@BatchGuid varchar(50)

)

As

SET NOCOUNT ON

--declare @BatchGuid varchar(50) = '8ae824e7-ccfb-4993-b615-3fd46e5a7652'

DECLARE @CurrentLabel nvarchar(500)

DECLARE label_cursor CURSOR FOR 
	SELECT distinct PredictedLabel from ER4ML.ER4ML.[dbo].ItemScore where BatchGuid = @BatchGuid

OPEN label_cursor  
FETCH NEXT FROM label_cursor INTO @CurrentLabel  

WHILE @@FETCH_STATUS = 0  
BEGIN  
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
	,	@SearchTitle + @CurrentLabel
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
				FROM ER4ML.ER4ML.dbo.ItemScore where BatchGuid = @BatchGuid and PredictedLabel = @CurrentLabel
			ORDER BY Score DESC


	-- STEP 4: Update the hits count
	UPDATE TB_SEARCH
		SET HITS_NO = @@ROWCOUNT
		WHERE SEARCH_ID = @NEW_SEARCH_ID


      FETCH NEXT FROM label_cursor INTO @CurrentLabel 
END 

CLOSE label_cursor  
DEALLOCATE label_cursor 


	-- STEP 5: clean up Azure SQL DB (deletes information about this batch)
	DECLARE	@return_value int
	EXEC	@return_value = ER4ML.ER4ML.[dbo].[CleanUpBatch] @BatchGuid = @BatchGuid
		
SET NOCOUNT OFF

GO