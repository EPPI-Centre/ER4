--we want a report that takes an “items with this code” input and spits out, 
--per item, whether that item appears in any “auto-update” or “network” search results, and if so, which one.
Use Reviewer
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

	select count(distinct item_id) 'auto updates', 1+3 expected from @results where IsInAU = 1
	select count(distinct item_id) 'related search', 291+3 expected from @results where IsInRS = 1
	declare @AutoUpdates int = (select count(distinct item_id) from @results where IsInAU = 1)
	declare @RelatedSearches int = (select count(distinct item_id) from @results where IsInRS = 1)
	declare @Both int = (select count(distinct r1.item_id)  
		from @results r1
		inner join @results r2 on r1.item_id = r2.item_id and r1.IsInRS = 1 AND r2.IsInAU = 1)
	declare @MatchedOtherSources int = (Select count(distinct item_id)  from @results where PaperId is NOT null and IsInAU = 0 and IsInRS = 0)
	declare @totMatched int =  (Select count(distinct item_id)  from @MatchedItems)
	declare @TotFromIR int = (select count(item_id) from TB_ITEM_REVIEW where REVIEW_ID = @ReviewId and IS_DELETED = 0)
	declare @WitnSomeMatch int = (select count(distinct IR.item_id) from TB_ITEM_REVIEW IR
									inner join tb_ITEM_MAG_MATCH MM on IR.REVIEW_ID = @ReviewId and IS_DELETED = 0 and MM.ITEM_ID = IR.ITEM_ID)

	Select @UnmatchedItems + @totMatched as [Computed Total], @TotFromIR as [TotalIR]
		--,@WitnSomeMatch [with some match]
		,@totMatched as [Matched], @UnmatchedItems as [Not Matched]
		,@AutoUpdates [In AutoUpdate results], @RelatedSearches [In Related Searches], @Both [in both], @MatchedOtherSources [Other]
		--, @totMatched - @AutoUpdates - @RelatedSearches+ @Both as [Check]
	--checks need to ensure each item appears 

	select distinct au.*, aur.* from @results r 
	inner join TB_MAG_AUTO_UPDATE_RUN aur
		on r.IsInAU = 1 and r.MAG_AUTO_UPDATE_RUN_ID = aur.MAG_AUTO_UPDATE_RUN_ID
	inner join TB_MAG_AUTO_UPDATE au on aur.MAG_AUTO_UPDATE_ID = au.MAG_AUTO_UPDATE_ID and au.REVIEW_ID = @ReviewId

	select distinct rr.* from @results r
	inner join TB_MAG_RELATED_RUN rr on r.IsInRS = 1 and r.MAG_RELATED_RUN_ID = rr.MAG_RELATED_RUN_ID and rr.REVIEW_ID = @ReviewId

	select r.*, i.SHORT_TITLE, i.TITLE from @results r 
	inner join tb_item i on r.item_id = i.ITEM_ID 
	order by SHORT_TITLE, r.Item_id
END
GO
--select count(distinct item_id) from @results





