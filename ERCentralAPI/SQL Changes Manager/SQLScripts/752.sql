--we want a report that takes an “items with this code” input and spits out, 
--per item, whether that item appears in any “auto-update” or “network” search results, and if so, which one.
Use Reviewer
GO


IF COL_LENGTH('dbo.TB_MAG_SEARCH', 'CAN_RERUN') IS  NULL
BEGIN
 print 'adding NEW CAN_RERUN col'
 Alter TABLE TB_MAG_SEARCH ADD CAN_RERUN bit NOT NULL DEFAULT(1);
END
else 
BEGIN
 print 'adding DUMMY col'
 Alter TABLE TB_MAG_SEARCH ADD CAN_RERUN_DUMMY bit NULL;
END

GO
IF COL_LENGTH('dbo.TB_MAG_SEARCH', 'CAN_RERUN_DUMMY') IS NULL
BEGIN
 print 'updating new flag'
 UPDATE TB_MAG_SEARCH set CAN_RERUN = 0 where SEARCH_IDS_STORED = 1;
END
ELSE
BEGIN 
 print 'dropping dummy column'
 ALTER TABLE TB_MAG_SEARCH drop COLUMN CAN_RERUN_DUMMY;
END
GO

IF COL_LENGTH('dbo.TB_MAG_SEARCH', 'IDS_DATE') IS  NULL
BEGIN
 print 'adding NEW IDS_DATE col'
 Alter TABLE TB_MAG_SEARCH ADD IDS_DATE DateTime NULL;

END
ELSE
BEGIN
 print 'adding IDS_DATE_DUMMY col'
 Alter TABLE TB_MAG_SEARCH ADD IDS_DATE_DUMMY bit NULL;
END
GO


IF COL_LENGTH('dbo.TB_MAG_SEARCH', 'IDS_DATE_DUMMY') IS NULL
BEGIN
 print 'updating IDS_DATE'
 UPDATE TB_MAG_SEARCH set IDS_DATE = SEARCH_DATE where SEARCH_IDS_STORED = 1;
END
ELSE
BEGIN 
 print 'dropping IDS_DATE_DUMMY'
 ALTER TABLE TB_MAG_SEARCH drop COLUMN IDS_DATE_DUMMY;
END
GO


-- =============================================
-- Author:		SG
-- Create date: 13/01/2026
-- Description:	Add PaperIDs to an existing MAG_SEARCH
-- =============================================
CREATE or ALTER procedure st_MagSearchAddPaperIds
	@REVIEW_ID INT,
	@SEARCH_IDS NVARCHAR(MAX),
	@MAG_SEARCH_ID INT
AS
BEGIN
 declare @check int = (SELECT COUNT(*) from TB_MAG_SEARCH where REVIEW_ID = @REVIEW_ID and MAG_SEARCH_ID = @MAG_SEARCH_ID)
 if @check = 1
 BEGIN
	declare @count int = (select count(distinct value) from dbo.fn_Split_int(@SEARCH_IDS, ','))
	UPDATE TB_MAG_SEARCH 
		set SEARCH_IDS = @SEARCH_IDS, SEARCH_IDS_STORED = 1, HITS_NO = @count, IDS_DATE = GETDATE()
		where MAG_SEARCH_ID = @MAG_SEARCH_ID  and REVIEW_ID = @REVIEW_ID
 END
END
GO
-- =============================================
-- Author:		James
-- Create date: 
-- Description:	gets a single magSearch on demand
-- =============================================
ALTER   PROCEDURE [dbo].[st_MagSearch] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT,
	@MAG_SEARCH_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	SELECT S.*, C.CONTACT_ID, C.CONTACT_NAME
	FROM TB_MAG_SEARCH S
	INNER JOIN TB_CONTACT C ON C.CONTACT_ID = S.CONTACT_ID
	where REVIEW_ID = @REVIEW_ID AND MAG_SEARCH_ID = @MAG_SEARCH_ID
		order by SEARCH_NO desc
		
END
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to TB_MAG_SEARCH
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSearchInsert] 
	@REVIEW_ID INT,
	@CONTACT_ID int,
	@SEARCH_TEXT NVARCHAR(MAX) = NULL,
	@SEARCH_NO INT = 0,
	@HITS_NO INT = 0,
	@SEARCH_DATE DATETIME,
	@MAG_FOLDER NVARCHAR(15),
	@MAG_SEARCH_TEXT NVARCHAR(MAX),
	@SEARCH_IDS_STORED bit = 0,
	@CAN_RERUN bit = 1,
	@SEARCH_IDS NVARCHAR(MAX) = null,

	@MAG_SEARCH_ID INT OUTPUT
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT @SEARCH_NO = MAX(SEARCH_NO) + 1 FROM TB_MAG_SEARCH
		WHERE REVIEW_ID = @REVIEW_ID;

	IF @SEARCH_NO IS NULL
		SET @SEARCH_NO = 1;

	DECLARE @IDS_DATE Date = NULL;
	IF @SEARCH_IDS_STORED = 1 SET @IDS_DATE = GETDATE();

    INSERT INTO TB_MAG_SEARCH(REVIEW_ID, CONTACT_ID, SEARCH_TEXT, SEARCH_NO, HITS_NO,
		SEARCH_DATE, MAG_FOLDER, MAG_SEARCH_TEXT, SEARCH_IDS_STORED, SEARCH_IDS, CAN_RERUN, IDS_DATE)
	VALUES(@REVIEW_ID, @CONTACT_ID, @SEARCH_TEXT, @SEARCH_NO, @HITS_NO,
		@SEARCH_DATE, @MAG_FOLDER, @MAG_SEARCH_TEXT, @SEARCH_IDS_STORED, @SEARCH_IDS, @CAN_RERUN, @IDS_DATE);
	
	SET @MAG_SEARCH_ID = SCOPE_IDENTITY();
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearchList]    Script Date: 21/01/2026 09:35:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSearchList] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	SELECT S.*, C.CONTACT_ID, C.CONTACT_NAME
	FROM TB_MAG_SEARCH S
	INNER JOIN TB_CONTACT C ON C.CONTACT_ID = S.CONTACT_ID
	where REVIEW_ID = @REVIEW_ID
		order by s.SEARCH_NO desc
		
END
GO


-- =============================================
-- Author:		SG
-- Create date: 13/01/2026
-- Description:	Where, if any, items with this code appear in OA searches
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_OpenAlexOriginReport] 
	@ReviewId int 
	,@attributeid bigint 
AS
BEGIN
	declare @MatchedItems table (Item_id bigint primary key)
	declare @AllItems table (Item_id bigint primary key)
	declare @UnmatchedItemsT table (Item_id bigint primary key)
	declare @ItemsMatch table (Item_id bigint, PaperId bigint, primary key(Item_id, PaperId))
	declare @results table (item_id bigint, PaperId bigint null, IsInAU bit, MAG_AUTO_UPDATE_RUN_ID int null, IsInRS bit, MAG_RELATED_RUN_ID int null)

	insert into @AllItems --items with this code, ignoring deleted items and incomplete coding
		SELECT DISTINCT I.ITEM_ID FROM TB_ITEM_REVIEW I
		INNER join TB_ITEM_ATTRIBUTE tia on tia.ATTRIBUTE_ID = @attributeid and tia.ITEM_ID = I.ITEM_ID and I.REVIEW_ID = @ReviewId
		INNER Join TB_ITEM_SET tis on tia.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1

	insert into @MatchedItems 
		SELECT DISTINCT (I.ITEM_ID) FROM @AllItems I
		INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID and IMM.REVIEW_ID = @ReviewId 
		AND ((imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null))

	insert into @UnmatchedItemsT
		SELECT DISTINCT (I.ITEM_ID) FROM @AllItems I
		WHERE I.ITEM_ID not in (SELECT ITEM_ID from @MatchedItems)
	declare @UnmatchedItems int = (select @@ROWCOUNT)

	Insert into @ItemsMatch(Item_id, PaperId)
		SELECT DISTINCT I.ITEM_ID, PaperId FROM @MatchedItems I
		INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID and IMM.REVIEW_ID = @ReviewId
		WHERE  (imm.ManualTrueMatch = 'true')

	Insert into @ItemsMatch(Item_id, PaperId)
		SELECT DISTINCT I.ITEM_ID, PaperId FROM @MatchedItems I
		INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID and IMM.REVIEW_ID = @ReviewId
		WHERE  (imm.AutoMatchScore >= 0.8) AND I.Item_id not in (select Item_id from @ItemsMatch)
	
	--select * from @ItemsMatch


	--Add items found in AutoUpdate runs
	insert into @results (item_id, PaperId, IsInAU, MAG_AUTO_UPDATE_RUN_ID, IsInRS)
	select Item_id, rr.PaperId, 1, aur.MAG_AUTO_UPDATE_RUN_ID, 0  from TB_MAG_AUTO_UPDATE au
		inner join TB_MAG_AUTO_UPDATE_RUN aur on au.REVIEW_ID = @ReviewId and au.MAG_AUTO_UPDATE_ID = aur.MAG_AUTO_UPDATE_ID
		inner join TB_MAG_AUTO_UPDATE_RUN_PAPER rr on aur.MAG_AUTO_UPDATE_RUN_ID = rr.MAG_AUTO_UPDATE_RUN_ID
		inner join @ItemsMatch im on rr.PaperId = im.PaperId

	--Add items found in Related Searches
	insert into @results (item_id, PaperId, IsInRS, MAG_RELATED_RUN_ID, IsInAU)
	select im.Item_id, im.PaperId, 1, rr.MAG_RELATED_RUN_ID, 0 from TB_MAG_RELATED_RUN rr
		inner join TB_MAG_RELATED_PAPERS rp on rr.MAG_RELATED_RUN_ID = rp.MAG_RELATED_RUN_ID and rr.REVIEW_ID = @ReviewId
		inner join @ItemsMatch im on rp.PaperId = im.PaperId

	--add items with matches, but not in the searches
	insert into @results(item_id, PaperId, IsInAU, IsInRS)
		select Item_id, PaperId, 0, 0 from @ItemsMatch where Item_id not in (Select Item_id from @results)
	--add the unmatched
	insert into @results (item_id, IsInAU, IsInRS) select item_id,0,0 from @UnmatchedItemsT

	--select count(distinct item_id) 'auto updates', 1+3 expected from @results where IsInAU = 1
	--select count(distinct item_id) 'related search', 291+3 expected from @results where IsInRS = 1
	declare @AutoUpdates int = (select count(distinct item_id) from @results where IsInAU = 1)
	declare @RelatedSearches int = (select count(distinct item_id) from @results where IsInRS = 1)
	declare @Both int = (select count(distinct r1.item_id)  
		from @results r1
		inner join @results r2 on r1.item_id = r2.item_id and r1.IsInRS = 1 AND r2.IsInAU = 1)
	declare @MatchedOtherSources int = (Select count(distinct item_id)  from @results where PaperId is NOT null and IsInAU = 0 and IsInRS = 0)
	declare @totMatched int =  (Select count(distinct item_id)  from @MatchedItems)
	--declare @TotFromIR int = (select count(item_id) from TB_ITEM_REVIEW where REVIEW_ID = @ReviewId and IS_DELETED = 0)
	declare @WitnSomeMatch int = (select count(distinct IR.item_id) from TB_ITEM_REVIEW IR
									inner join tb_ITEM_MAG_MATCH MM on IR.REVIEW_ID = @ReviewId and IS_DELETED = 0 and MM.ITEM_ID = IR.ITEM_ID)

	Select @UnmatchedItems + @totMatched as [Computed Total]--, @TotFromIR as [TotalIR]
		--,@WitnSomeMatch [with some match]
		,@totMatched as [Matched], @UnmatchedItems as [Not Matched]
		,@AutoUpdates [In AutoUpdate results], @RelatedSearches [In Related Searches], @Both [in both], @MatchedOtherSources [Other]
		--, @totMatched - @AutoUpdates - @RelatedSearches+ @Both as [Check]
	--checks need to ensure each item appears 

	select distinct au.MAG_AUTO_UPDATE_ID, au.REVIEW_ID, au.USER_DESCRIPTION, au.ATTRIBUTE_ID, a.ATTRIBUTE_NAME
	,au.ALL_INCLUDED, cm.MODEL_TITLE, aur.STUDY_TYPE_CLASSIFIER, aur.USER_CLASSIFIER_MODEL_ID, aur.USER_CLASSIFIER_REVIEW_ID
	, aur.MAG_AUTO_UPDATE_RUN_ID, aur.DATE_RUN, aur.N_PAPERS, aur.MAG_VERSION
	, count(r.item_id) [ITEMS_COUNT] from @results r 
	inner join TB_MAG_AUTO_UPDATE_RUN aur
		on r.IsInAU = 1 and r.MAG_AUTO_UPDATE_RUN_ID = aur.MAG_AUTO_UPDATE_RUN_ID
	inner join TB_MAG_AUTO_UPDATE au on aur.MAG_AUTO_UPDATE_ID = au.MAG_AUTO_UPDATE_ID and au.REVIEW_ID = @ReviewId
	left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = au.ATTRIBUTE_ID
	left outer join tb_CLASSIFIER_MODEL cm on cm.MODEL_ID = au.USER_CLASSIFIER_MODEL_ID
	group by au.MAG_AUTO_UPDATE_ID, au.REVIEW_ID, au.USER_DESCRIPTION, au.ATTRIBUTE_ID, a.ATTRIBUTE_NAME
	,au.ALL_INCLUDED, cm.MODEL_TITLE, aur.STUDY_TYPE_CLASSIFIER, aur.USER_CLASSIFIER_MODEL_ID, aur.USER_CLASSIFIER_REVIEW_ID
	, aur.MAG_AUTO_UPDATE_RUN_ID, aur.DATE_RUN, aur.N_PAPERS, aur.MAG_VERSION

	select distinct rr.MAG_RELATED_RUN_ID, rr.REVIEW_ID, rr.USER_DESCRIPTION, rr.ATTRIBUTE_ID, a.ATTRIBUTE_NAME, rr.ALL_INCLUDED, rr.DATE_FROM
	, rr.DATE_RUN, rr.STATUS, rr.USER_STATUS, rr.N_PAPERS, rr.MODE, rr.FILTERED
	, count(r.item_id) [ITEMS_COUNT] from @results r
	inner join TB_MAG_RELATED_RUN rr on r.IsInRS = 1 and r.MAG_RELATED_RUN_ID = rr.MAG_RELATED_RUN_ID and rr.REVIEW_ID = @ReviewId
	left join TB_ATTRIBUTE a on rr.ATTRIBUTE_ID = a.ATTRIBUTE_ID
	group by rr.MAG_RELATED_RUN_ID, rr.REVIEW_ID, rr.USER_DESCRIPTION, rr.ATTRIBUTE_ID, a.ATTRIBUTE_NAME, rr.ALL_INCLUDED, rr.DATE_FROM
	, rr.DATE_RUN, rr.STATUS, rr.USER_STATUS, rr.N_PAPERS, rr.MODE, rr.FILTERED

	SELECT s.*, C.CONTACT_ID, C.CONTACT_NAME
	FROM TB_MAG_SEARCH S
	INNER JOIN TB_CONTACT C ON C.CONTACT_ID = S.CONTACT_ID
	where REVIEW_ID = @ReviewId AND SEARCH_IDS_STORED = 1


	select distinct r.*, i.SHORT_TITLE, i.TITLE--, tis.*, s.*
		,CASE when (s.SOURCE_ID is null) then 'Manual creation' else s.SOURCE_NAME end as [SOURCE_NAME]
	from @results r 
	inner join tb_item i on r.item_id = i.ITEM_ID 
	left join TB_ITEM_SOURCE tis on i.ITEM_ID = tis.ITEM_ID
	left join TB_SOURCE s on s.SOURCE_ID = tis.SOURCE_ID and s.REVIEW_ID = @ReviewId
	WHERE (s.SOURCE_NAME is NOT NULL OR (s.SOURCE_NAME IS NULL AND tis.SOURCE_ID is NULL))
	order by SHORT_TITLE, r.Item_id
END
GO
