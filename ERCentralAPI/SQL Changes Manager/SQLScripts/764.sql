USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_RobotOpenAIPromptEvaluationResults]    Script Date: 16/02/2026 16:40:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_RobotOpenAIPromptEvaluationResults] (
		@ReviewId INT
,		@ATTRIBUTE_ID BIGINT
,		@OPENAI_PROMPT_EVALUATION_ID INT
,		@LlmTrue bit
,		@GoldTrue bit

,		@PageNum INT = 1
,		@PerPage INT = 3
,		@CurrentPage INT OUTPUT
,		@TotalPages INT OUTPUT
,		@TotalRows INT OUTPUT
)

As

SET NOCOUNT ON

declare @RowsToRetrieve int
declare @confusion_matrix table (ITERATION INT, ATTRIBUTE_ID BIGINT, ITEM_ID bigint, TP bigint, TN BIGINT, FP BIGINT, FN BIGINT)
declare @T table(ITEM_ID bigint)

;	WITH GoldStandard AS (  
    -- Get the gold standard ATTRIBUTE_IDs for each ITEM_ID  
    SELECT ITEM_ID, ATTRIBUTE_ID  
    FROM TB_OPENAI_PROMPT_EVALUATION_DATA  
    WHERE GOLD_STANDARD = 1   
      AND OPENAI_PROMPT_EVALUATION_ID = @OPENAI_PROMPT_EVALUATION_ID  
),  
  
LLMPredictions AS (  
    -- Get LLM predictions for each ITEM_ID and iteration  
    SELECT ITEM_ID, ATTRIBUTE_ID, ITERATION  
    FROM TB_OPENAI_PROMPT_EVALUATION_DATA  
    WHERE GOLD_STANDARD = 0   
      AND OPENAI_PROMPT_EVALUATION_ID = @OPENAI_PROMPT_EVALUATION_ID  
),  
  
AllATTRIBUTE_IDs AS (  
    -- Get all unique ATTRIBUTE_IDs in the dataset  
    SELECT DISTINCT ATTRIBUTE_ID  
    FROM TB_OPENAI_PROMPT_EVALUATION_DATA   
    WHERE OPENAI_PROMPT_EVALUATION_ID = @OPENAI_PROMPT_EVALUATION_ID  
),  
  
AllITEM_IDs AS (  
    -- Get ALL unique ITEM_IDs in the dataset (both gold standard AND LLM)  
    SELECT DISTINCT ITEM_ID  
    FROM TB_OPENAI_PROMPT_EVALUATION_DATA   
    WHERE OPENAI_PROMPT_EVALUATION_ID = @OPENAI_PROMPT_EVALUATION_ID  
),  
  
AllIterations AS (  
    -- Get all unique iterations from LLM predictions  
    SELECT DISTINCT ITERATION  
    FROM LLMPredictions  
),  
  
AllCombinations AS (  
    -- Create all combinations of ITEM_ID, ATTRIBUTE_ID, and ITERATION  
    -- Using all items (not just LLM items) to ensure complete coverage  
    SELECT   
        i.ITEM_ID,   
        a.ATTRIBUTE_ID,   
        it.ITERATION  
    FROM AllITEM_IDs i  
    CROSS JOIN AllATTRIBUTE_IDs a  
    CROSS JOIN AllIterations it  
),  
  
ConfusionMatrix AS (  
    SELECT   
        ac.ITERATION,  
        ac.ATTRIBUTE_ID,  
        ac.ITEM_ID,  
        -- True Positive: Both gold standard and LLM have the ATTRIBUTE_ID  
        CASE WHEN g.ITEM_ID IS NOT NULL AND l.ITEM_ID IS NOT NULL THEN 1 ELSE 0 END AS TP,  
        -- True Negative: Neither gold standard nor LLM have the ATTRIBUTE_ID  
        CASE WHEN g.ITEM_ID IS NULL AND l.ITEM_ID IS NULL THEN 1 ELSE 0 END AS TN,  
        -- False Positive: LLM has it, gold standard doesn't  
        CASE WHEN g.ITEM_ID IS NULL AND l.ITEM_ID IS NOT NULL THEN 1 ELSE 0 END AS FP,  
        -- False Negative: Gold standard has it, LLM doesn't  
        CASE WHEN g.ITEM_ID IS NOT NULL AND l.ITEM_ID IS NULL THEN 1 ELSE 0 END AS FN  
    FROM AllCombinations ac  
    LEFT JOIN GoldStandard g   
        ON ac.ITEM_ID = g.ITEM_ID   
        AND ac.ATTRIBUTE_ID = g.ATTRIBUTE_ID  
    LEFT JOIN LLMPredictions l   
        ON ac.ITEM_ID = l.ITEM_ID   
        AND ac.ATTRIBUTE_ID = l.ATTRIBUTE_ID   
        AND ac.ITERATION = l.ITERATION  
)
insert into @confusion_matrix(ITERATION, ATTRIBUTE_ID, ITEM_ID, TP, TN, FP, FN)
SELECT ITERATION, ATTRIBUTE_ID, ITEM_ID, TP, TN, FP, FN FROM ConfusionMatrix where ATTRIBUTE_ID = @ATTRIBUTE_ID;

if @GoldTrue = 1 and @LlmTrue = 1
begin
	insert into @T(ITEM_ID) select distinct item_id from @confusion_matrix where TP = 1
end

if @GoldTrue = 0 and @LlmTrue = 0
begin
	insert into @T(ITEM_ID) select distinct item_id from @confusion_matrix where TN = 1
end

if @GoldTrue = 1 and @LlmTrue = 0
begin
	insert into @T(ITEM_ID) select distinct item_id from @confusion_matrix where FN = 1
end

if @GoldTrue = 0 and @LlmTrue = 1
begin
	insert into @T(ITEM_ID) select distinct item_id from @confusion_matrix where FP = 1
end

	SELECT @TotalRows = count(*) FROM @T

	set @TotalPages = @TotalRows/@PerPage

	if @PageNum < 1
	set @PageNum = 1

	if @TotalRows % @PerPage != 0
	set @TotalPages = @TotalPages + 1

	set @RowsToRetrieve = @PerPage * @PageNum
	set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT I.ITEM_ID, 0 as ITEM_RANK,
            ROW_NUMBER() OVER(order by i.item_id) RowNum
      FROM @t I
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, 
				  [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], ir.IS_DELETED, IR.IS_INCLUDED,
				  [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS,
                  ir.MASTER_ITEM_ID as MASTER_ITEM_ID, DOI, KEYWORDS, 0 as ITEM_RANK
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID 
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
				  INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = II.ITEM_ID and IR.REVIEW_ID = @ReviewId
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve
                  ORDER BY RowNum desc
                
      --OPTION (OPTIMIZE FOR (@PerPage=700, @SEARCH_ID UNKNOWN, @REVIEW_ID UNKNOWN))
SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'


SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_UpdateOpenAiPromptEvaluation]    Script Date: 16/02/2026 19:25:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   procedure [dbo].[st_UpdateOpenAiPromptEvaluation]
(

	@OPENAI_PROMPT_EVALUATION_ID INT
,	@STATUS varchar(10)
)
As

SET NOCOUNT ON
	
DECLARE @TP INT, @TN INT, @FP INT, @FN INT

IF @STATUS = 'Failed'
BEGIN
	UPDATE TB_OPENAI_PROMPT_EVALUATION
		SET TITLE = CONCAT(TITLE, ' (FAILED)'),
		TP = 0, FP = 0, TN = 0, FN = 0
	WHERE OPENAI_PROMPT_EVALUATION_ID = @OPENAI_PROMPT_EVALUATION_ID
END
ELSE
BEGIN

;	WITH GoldStandard AS (  
    -- Get the gold standard ATTRIBUTE_IDs for each ITEM_ID  
    SELECT ITEM_ID, ATTRIBUTE_ID  
    FROM TB_OPENAI_PROMPT_EVALUATION_DATA  
    WHERE GOLD_STANDARD = 1   
      AND OPENAI_PROMPT_EVALUATION_ID = @OPENAI_PROMPT_EVALUATION_ID  
),  
  
LLMPredictions AS (  
    -- Get LLM predictions for each ITEM_ID and iteration  
    SELECT ITEM_ID, ATTRIBUTE_ID, ITERATION  
    FROM TB_OPENAI_PROMPT_EVALUATION_DATA  
    WHERE GOLD_STANDARD = 0   
      AND OPENAI_PROMPT_EVALUATION_ID = @OPENAI_PROMPT_EVALUATION_ID  
),  
  
AllATTRIBUTE_IDs AS (  
    -- Get all unique ATTRIBUTE_IDs in the dataset  
    SELECT DISTINCT ATTRIBUTE_ID  
    FROM TB_OPENAI_PROMPT_EVALUATION_DATA   
    WHERE OPENAI_PROMPT_EVALUATION_ID = @OPENAI_PROMPT_EVALUATION_ID  
),  
  
AllITEM_IDs AS (  
    -- Get ALL unique ITEM_IDs in the dataset (both gold standard AND LLM)  
    SELECT DISTINCT ITEM_ID  
    FROM TB_OPENAI_PROMPT_EVALUATION_DATA   
    WHERE OPENAI_PROMPT_EVALUATION_ID = @OPENAI_PROMPT_EVALUATION_ID  
),  
  
AllIterations AS (  
    -- Get all unique iterations from LLM predictions  
    SELECT DISTINCT ITERATION  
    FROM LLMPredictions  
),  
  
AllCombinations AS (  
    -- Create all combinations of ITEM_ID, ATTRIBUTE_ID, and ITERATION  
    -- Using all items (not just LLM items) to ensure complete coverage  
    SELECT   
        i.ITEM_ID,   
        a.ATTRIBUTE_ID,   
        it.ITERATION  
    FROM AllITEM_IDs i  
    CROSS JOIN AllATTRIBUTE_IDs a  
    CROSS JOIN AllIterations it  
),  
  
ConfusionMatrix AS (  
    SELECT   
        ac.ITERATION,  
        ac.ATTRIBUTE_ID,  
        ac.ITEM_ID,  
        -- True Positive: Both gold standard and LLM have the ATTRIBUTE_ID  
        CASE WHEN g.ITEM_ID IS NOT NULL AND l.ITEM_ID IS NOT NULL THEN 1 ELSE 0 END AS TP,  
        -- True Negative: Neither gold standard nor LLM have the ATTRIBUTE_ID  
        CASE WHEN g.ITEM_ID IS NULL AND l.ITEM_ID IS NULL THEN 1 ELSE 0 END AS TN,  
        -- False Positive: LLM has it, gold standard doesn't  
        CASE WHEN g.ITEM_ID IS NULL AND l.ITEM_ID IS NOT NULL THEN 1 ELSE 0 END AS FP,  
        -- False Negative: Gold standard has it, LLM doesn't  
        CASE WHEN g.ITEM_ID IS NOT NULL AND l.ITEM_ID IS NULL THEN 1 ELSE 0 END AS FN  
    FROM AllCombinations ac  
    LEFT JOIN GoldStandard g   
        ON ac.ITEM_ID = g.ITEM_ID   
        AND ac.ATTRIBUTE_ID = g.ATTRIBUTE_ID  
    LEFT JOIN LLMPredictions l   
        ON ac.ITEM_ID = l.ITEM_ID   
        AND ac.ATTRIBUTE_ID = l.ATTRIBUTE_ID   
        AND ac.ITERATION = l.ITERATION 
 ),
  
IterationStats AS (  
    -- Sum up confusion matrix values per iteration  
    SELECT   
        ITERATION,  
        SUM(TP) AS Total_TP,  
        SUM(TN) AS Total_TN,  
        SUM(FP) AS Total_FP,  
        SUM(FN) AS Total_FN  
    FROM ConfusionMatrix  
    GROUP BY ITERATION  
)  
  
-- Calculate averages across all iterations  
SELECT   
    @TP = AVG(CAST(Total_TP AS FLOAT)),  
    @TN = AVG(CAST(Total_TN AS FLOAT)),  
    @FP = AVG(CAST(Total_FP AS FLOAT)),  
    @FN = AVG(CAST(Total_FN AS FLOAT))  
FROM IterationStats;  
  
UPDATE TB_OPENAI_PROMPT_EVALUATION  
SET TP = @TP,  
    FP = @FP,  
    TN = @TN,  
    FN = @FN  
WHERE OPENAI_PROMPT_EVALUATION_ID = @OPENAI_PROMPT_EVALUATION_ID;  

END

SET NOCOUNT OFF
GO