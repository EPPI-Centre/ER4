USE Reviewer
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE st_WebDbFrequencyCrosstabAndMap
	-- Add the parameters for the stored procedure here
	@attributeIdXAxis bigint 
	, @setIdXAxis int
	, @attributeIdYAxis bigint = 0
	, @included bit null
	, @setIdYAxis int = 0 
	, @segmentsParent bigint = 0
	, @setIdSegments int = 0
	, @onlyThisAttribute bigint = 0
	, @RevId int 
	, @WebDbId int 
AS
BEGIN

--declare 
	--@attributeIdXAxis bigint = 64472  --62475 0
	--, @setIdXAxis int = 644
	--, @attributeIdYAxis bigint = 0
	--, @setIdYAxis int = 0 --644 664
	--, @SegmentsParent bigint = 119121
	--, @setIdSegments int = 0--1880
	--, @OnlyThisAttribute bigint = 0
	--, @RevId int = 99
	--, @WebDbId int = 18


declare @items table (ItemId bigint primary key, X_atts varchar(max) null, Y_atts  varchar(max) null, segments varchar(max) null)
declare @attsX table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
declare @attsY table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
declare @segments table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)

--sanity check, ensure @RevId and @WebDbId match...
Declare @CheckWebDbId int = null
set @CheckWebDbId = (select WEBDB_ID from TB_WEBDB where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
IF @CheckWebDbId is null return;

declare @WebDbFilter bigint = (select w.WITH_ATTRIBUTE_ID from TB_WEBDB w where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
if @WebDbFilter is not null and @WebDbFilter > 1
BEGIN
	if @OnlyThisAttribute > 0
	BEGIN
		insert into @items select distinct ir.item_id, null, null, null from TB_ITEM_REVIEW ir
			inner join tb_item_set tis on ir.ITEM_ID = tis.ITEM_ID and ir.REVIEW_ID = @RevId 
			and (@included is null OR ir.IS_INCLUDED = @included)
			and ir.IS_DELETED = 0 and tis.IS_COMPLETED = 1
			inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tia.ATTRIBUTE_ID = @WebDbFilter
			inner join TB_ITEM_ATTRIBUTE tia2 on tia2.ITEM_ID = ir.ITEM_ID and tia2.ATTRIBUTE_ID = @OnlyThisAttribute 
			inner join TB_ITEM_SET tis2 on tia2.ITEM_SET_ID = tis2.ITEM_SET_ID and tis2.IS_COMPLETED = 1 
	END
	ELSE
	Begin
		insert into @items select distinct ir.item_id, null, null, null from TB_ITEM_REVIEW ir
			inner join tb_item_set tis on ir.ITEM_ID = tis.ITEM_ID and ir.REVIEW_ID = @RevId 
			and (@included is null OR ir.IS_INCLUDED = @included) 
			and ir.IS_DELETED = 0 and tis.IS_COMPLETED = 1
			inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tia.ATTRIBUTE_ID = @WebDbFilter
	END
END
else
BEGIN
	if @OnlyThisAttribute > 0
	BEGIN
		insert into @items select distinct ir.item_id, null, null, null from TB_ITEM_REVIEW ir
			inner join TB_ITEM_ATTRIBUTE tia on ir.ITEM_ID = tia.ITEM_ID and tia.ATTRIBUTE_ID = @OnlyThisAttribute 
				and ir.REVIEW_ID = @RevId and ir.IS_DELETED = 0
				and (@included is null OR ir.IS_INCLUDED = @included)  
			inner join TB_ITEM_SET tis on tia.ITEM_SET_ID = tis.ITEM_SET_ID and IS_COMPLETED = 1
	END
	ELSE
	Begin
		insert into @items select distinct ir.item_id, null, null, null from TB_ITEM_REVIEW ir
			where ir.REVIEW_ID = @RevId and ir.IS_DELETED = 0
			 and (@included is null OR ir.IS_INCLUDED = @included) 
	END
END

insert into @attsX select a.Attribute_id, a.ATTRIBUTE_NAME, ATTRIBUTE_ORDER, 0 from TB_ATTRIBUTE_SET tas
	 inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	 where tas.SET_ID = @setIdXAxis and PARENT_ATTRIBUTE_ID = @attributeIdXAxis
select ATTRIBUTE_ID, ATTRIBUTE_NAME from @attsX order by ord

IF @setIdYAxis > 0
BEGIN
	insert into @attsY select a.Attribute_id, a.ATTRIBUTE_NAME, ATTRIBUTE_ORDER, 0 from TB_ATTRIBUTE_SET tas
		 inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID 
		where SET_ID = @setIdYAxis and PARENT_ATTRIBUTE_ID = @attributeIdYAxis
	select ATTRIBUTE_ID, ATTRIBUTE_NAME from @attsY order by ord
END

If @SegmentsParent > 0
BEGIN
	insert into @segments select a.Attribute_id, a.ATTRIBUTE_NAME, ATTRIBUTE_ORDER, 0 from TB_ATTRIBUTE_SET tas
		 inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID 
		where SET_ID = @setIdSegments and PARENT_ATTRIBUTE_ID = @SegmentsParent
	select ATTRIBUTE_ID, ATTRIBUTE_NAME from @segments order by ord
END

--declare @currAtt bigint = (select top(1) ATTRIBUTE_ID from @attsX where done = 0 order by ord)
--declare @limit int = 1000, @cycle int = 0
--while @currAtt is not null and @currAtt > 0 and @cycle < @limit
--BEGIN
--	set @cycle = @cycle+1
--	update @attsX set done = 1 where ATTRIBUTE_ID = @currAtt

--	--select ItemId from @items i
--	--	inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdXAxis
--	--	inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ITEM_ID = i.ItemId and ia.ATTRIBUTE_ID = @currAtt
--	select @currAtt, STRING_AGG(cast (ItemId as nvarchar(max)), ',') from @items i
--		inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdXAxis
--		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ITEM_ID = i.ItemId and ia.ATTRIBUTE_ID = @currAtt

--	set @currAtt = (select top(1) ATTRIBUTE_ID from @attsX where done = 0 order by ord)
--	--if @currAtt is not null print  cast(@currAtt as nvarchar(200)) + '!'
--	--else print 'ending'
--END

--set @currAtt  = (select top(1) ATTRIBUTE_ID from @attsY where done = 0 order by ord)
--set @cycle = 0
--while @currAtt is not null and @currAtt > 0 and @cycle < @limit
--BEGIN
--	set @cycle = @cycle+1
--	update @attsY set done = 1 where ATTRIBUTE_ID = @currAtt

--	--select ItemId from @items i
--	--	inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdXAxis
--	--	inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ITEM_ID = i.ItemId and ia.ATTRIBUTE_ID = @currAtt
--	select @currAtt, STRING_AGG(cast (ItemId as nvarchar(max)), ',') from @items i
--		inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdYAxis
--		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ITEM_ID = i.ItemId and ia.ATTRIBUTE_ID = @currAtt

--	set @currAtt = (select top(1) ATTRIBUTE_ID from @attsY where done = 0 order by ord)
--	--if @currAtt is not null print  cast(@currAtt as nvarchar(200)) + '!'
--	--else print 'ending'
--END

update @items  set X_atts = Atts
from 
(
	select STRING_AGG(cast (ia.ATTRIBUTE_ID as nvarchar(max)), ',') as Atts, i.itemId as iid from @items i 
	inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdXAxis
	inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.ITEM_ID = ia.ITEM_ID
	inner join @attsX a on ia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
	group by ItemId 
	--order by ItemId
) as big
WHERE ItemId = Big.iid

if @setIdYAxis > 0
	update @items set Y_atts = Atts
	from 
	(
		select STRING_AGG(cast (ia.ATTRIBUTE_ID as nvarchar(max)), ',') as Atts, i.itemId as iid from @items i 
		inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdYAxis
		inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.ITEM_ID = ia.ITEM_ID
		inner join @attsY a on ia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
		group by ItemId 
		--order by ItemId
	) as big
	WHERE ItemId = Big.iid

if @setIdSegments > 0
update @items set segments = Atts
from 
(
	select STRING_AGG(cast (ia.ATTRIBUTE_ID as nvarchar(max)), ',') as Atts, i.itemId as iid from @items i 
	inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdSegments
	inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.ITEM_ID = ia.ITEM_ID
	inner join @segments a on ia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
	group by ItemId 
	--order by ItemId
) as big
WHERE ItemId = Big.iid

select * from @items
END
GO

CREATE OR ALTER PROCEDURE st_WebDbWithThisCode
	-- Add the parameters for the stored procedure here
	@attributeId bigint 
	, @included bit null
	, @RevId int 
	, @WebDbId int
      
    , @PageNum INT = 1
    , @PerPage INT = 100
    , @CurrentPage INT OUTPUT
    , @TotalPages INT OUTPUT
    , @TotalRows INT OUTPUT  
AS
SET NOCOUNT ON
	--sanity check, ensure @RevId and @WebDbId match...
	Declare @CheckWebDbId int = null
	set @CheckWebDbId = (select WEBDB_ID from TB_WEBDB where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	IF @CheckWebDbId is null return;

	declare @items table (ItemId bigint primary key)
	declare @WebDbFilter bigint = (select w.WITH_ATTRIBUTE_ID from TB_WEBDB w where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	if @WebDbFilter is not null and @WebDbFilter > 1
	BEGIN
		insert into @items select distinct ir.item_id from TB_ITEM_REVIEW ir
			inner join tb_item_set tis on ir.ITEM_ID = tis.ITEM_ID and ir.REVIEW_ID = @RevId 
			and (@included is null OR ir.IS_INCLUDED = @included)
			and ir.IS_DELETED = 0 and tis.IS_COMPLETED = 1
			inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tia.ATTRIBUTE_ID = @WebDbFilter
			inner join TB_ITEM_ATTRIBUTE tia2 on tia2.ITEM_ID = ir.ITEM_ID and tia2.ATTRIBUTE_ID = @attributeId 
			inner join TB_ITEM_SET tis2 on tia2.ITEM_SET_ID = tis2.ITEM_SET_ID and tis2.IS_COMPLETED = 1 
		SELECT @TotalRows = @@ROWCOUNT
	END
	ELSE
	BEGIN
		insert into @items select distinct ir.item_id from TB_ITEM_REVIEW ir
			inner join TB_ITEM_ATTRIBUTE tia on ir.ITEM_ID = tia.ITEM_ID and tia.ATTRIBUTE_ID = @attributeId 
				and ir.REVIEW_ID = @RevId and ir.IS_DELETED = 0
				and (@included is null OR ir.IS_INCLUDED = @included)  
			inner join TB_ITEM_SET tis on tia.ITEM_SET_ID = tis.ITEM_SET_ID and IS_COMPLETED = 1
		SELECT @TotalRows = @@ROWCOUNT
	END
	declare @RowsToRetrieve int
	set @TotalPages = @TotalRows/@PerPage

	if @PageNum < 1
	set @PageNum = 1

	if @TotalRows % @PerPage != 0
	set @TotalPages = @TotalPages + 1

	set @RowsToRetrieve = @PerPage * @PageNum
	set @CurrentPage = @PageNum;

	WITH SearchResults AS
	(
	SELECT DISTINCT (ir.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
		ROW_NUMBER() OVER(order by SHORT_TITLE, ir.ITEM_ID) RowNum
	FROM TB_ITEM i
		INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID
		INNER JOIN @items id on id.ItemId = ir.ITEM_ID
	)
	Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
				TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
				[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
				AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
				, SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
				--, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
		FROM SearchResults
				INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
				INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
		WHERE RowNum > @RowsToRetrieve - @PerPage
		AND RowNum <= @RowsToRetrieve
		ORDER BY RowNum
	SELECT	@CurrentPage as N'@CurrentPage',
		@TotalPages as N'@TotalPages',
		@TotalRows as N'@TotalRows'
SET NOCOUNT OFF

GO

CREATE OR ALTER PROCEDURE st_WebDbItemListFrequencyNoneOfTheAbove
	@ParentAttributeId bigint 
	, @included bit null
	, @RevId int 
	, @WebDbId int
	, @SetId int
	, @FilterAttributeId int
      
    , @PageNum INT = 1
    , @PerPage INT = 100
    , @CurrentPage INT OUTPUT
    , @TotalPages INT OUTPUT
    , @TotalRows INT OUTPUT  
AS
SET NOCOUNT ON
	--sanity check, ensure @RevId and @WebDbId match...
	Declare @CheckWebDbId int = null
	set @CheckWebDbId = (select WEBDB_ID from TB_WEBDB where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	IF @CheckWebDbId is null return;


	declare @WebDbFilter bigint = (select w.WITH_ATTRIBUTE_ID from TB_WEBDB w where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	declare @RowsToRetrieve int
	Declare @ID table (ItemID bigint ) --store IDs to build paged results as a simple join
	IF (@FilterAttributeId = 0)
	BEGIN
		insert into @ID
		Select DISTINCT TB_ITEM_REVIEW.ITEM_ID
				from TB_ITEM_REVIEW 
				INNER JOIN TB_ITEM_ATTRIBUTE IA3 ON IA3.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND 
						(IA3.ATTRIBUTE_ID = @WebDbFilter OR (@WebDbFilter is null OR @WebDbFilter = 0))
					INNER JOIN TB_ITEM_SET IS3 ON IS3.ITEM_SET_ID = IA3.ITEM_SET_ID AND IS3.IS_COMPLETED = 1
				where TB_ITEM_REVIEW.ITEM_ID not in 
						(
							select TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
							  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
									AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SetId
							  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
									AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ParentAttributeId AND TB_ATTRIBUTE_SET.SET_ID = @SetId
							  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
							  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
									AND TB_ITEM_REVIEW.REVIEW_ID = @RevId
									AND TB_ITEM_REVIEW.IS_INCLUDED = @included
									AND TB_ITEM_REVIEW.IS_DELETED = 0
						)
					AND TB_ITEM_REVIEW.REVIEW_ID = @RevId
									AND TB_ITEM_REVIEW.IS_INCLUDED = @included
									AND TB_ITEM_REVIEW.IS_DELETED = 0
	END
	ELSE
	BEGIN
		insert into @ID
		Select TB_ITEM_REVIEW.ITEM_ID
				from TB_ITEM_REVIEW 
				INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND IA2.ATTRIBUTE_ID = @FilterAttributeId
					INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 1
					INNER JOIN TB_ITEM_ATTRIBUTE IA3 ON IA3.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND 
						(IA3.ATTRIBUTE_ID = @WebDbFilter OR (@WebDbFilter is null OR @WebDbFilter = 0))
					INNER JOIN TB_ITEM_SET IS3 ON IS3.ITEM_SET_ID = IA3.ITEM_SET_ID AND IS3.IS_COMPLETED = 1
				where TB_ITEM_REVIEW.ITEM_ID not in 
						(
							select TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
							  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
									AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SetId
							  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
									AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ParentAttributeId AND TB_ATTRIBUTE_SET.SET_ID = @SetId
							  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
							  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
									AND TB_ITEM_REVIEW.REVIEW_ID = @RevId
									AND TB_ITEM_REVIEW.IS_INCLUDED = @included
									AND TB_ITEM_REVIEW.IS_DELETED = 0
							  INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND IA2.ATTRIBUTE_ID = @FilterAttributeId
							  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 1
						)
					AND TB_ITEM_REVIEW.REVIEW_ID = @RevId
									AND TB_ITEM_REVIEW.IS_INCLUDED = @included
									AND TB_ITEM_REVIEW.IS_DELETED = 0
	END
		--count results
		SELECT @TotalRows = count(ItemID) from @ID
		set @TotalPages = @TotalRows/@PerPage

		if @PageNum < 1
		set @PageNum = 1

		if @TotalRows % @PerPage != 0
		set @TotalPages = @TotalPages + 1

		set @RowsToRetrieve = @PerPage * @PageNum
		set @CurrentPage = @PageNum;

		WITH SearchResults AS
		(
			SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID
				, ROW_NUMBER() OVER(order by SHORT_TITLE, I.ITEM_ID) RowNum
			FROM TB_ITEM I
			INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] 
			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
				TB_ITEM_REVIEW.REVIEW_ID = @RevId
			INNER JOIN @ID on I.ITEM_ID = ItemID
		)
		Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
				TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
				[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
				AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
				, SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
			FROM SearchResults 
					  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
					  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
			WHERE RowNum > @RowsToRetrieve - @PerPage
			AND RowNum <= @RowsToRetrieve 
			ORDER BY RowNum


	SELECT	@CurrentPage as N'@CurrentPage',
			@TotalPages as N'@TotalPages',
			@TotalRows as N'@TotalRows'
SET NOCOUNT OFF
GO