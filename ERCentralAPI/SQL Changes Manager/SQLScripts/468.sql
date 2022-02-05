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
/****** Object:  StoredProcedure [dbo].[st_MagGetPaperIdsForFoSImport]    Script Date: 27/01/2022 11:15:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get PaperIds to get field of study ids for
-- =============================================
ALTER   PROCEDURE [dbo].[st_MagGetPaperIdsForFoSImport] 
	-- Add the parameters for the stored procedure here
	
	@ITEM_IDS nvarchar(max)
,	@ATTRIBUTE_ID bigint
,	@REVIEW_ID int
,	@REVIEW_SET_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @FILTERED_IDS TABLE (
	ITEM_ID BIGINT PRIMARY KEY
)
	-- these already have attributes in that set so we filter them out
	IF @REVIEW_SET_ID > 0
	BEGIN
		insert into @FILTERED_IDS
		select iset.ITEM_ID from TB_ITEM_SET iset
		inner join TB_REVIEW_SET rs on rs.SET_ID = iset.SET_ID
		where rs.REVIEW_SET_ID = @REVIEW_SET_ID and iset.IS_COMPLETED = 'true' AND RS.REVIEW_ID = @REVIEW_ID
	END

    -- Insert statements for procedure here
	
	if @ATTRIBUTE_ID < 1
	begin
		if @ITEM_IDS = '' -- everything in the review
		begin
			select PaperId, imm.ITEM_ID from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			where ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false' AND
				(imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
				and not imm.ITEM_ID in (select ITEM_ID from @FILTERED_IDS)
		end
		else
		begin -- filtred by the list of item_ids
			select PaperId, imm.ITEM_ID from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			inner join dbo.fn_Split_int(@ITEM_IDS, ',') ids on ids.value = ir.ITEM_ID
			where ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false' AND
				(imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			and not imm.ITEM_ID in (select ITEM_ID from @FILTERED_IDS)
		end
	end
	else
	begin
		if @ITEM_IDS = '' -- everything in the review filtered by attribute id
		begin
			select PaperId, imm.ITEM_ID from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
			inner join TB_ITEM_SET iset on iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
			where ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false' AND
				(imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			and not imm.ITEM_ID in (select ITEM_ID from @FILTERED_IDS)
		end
	end
		
END

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