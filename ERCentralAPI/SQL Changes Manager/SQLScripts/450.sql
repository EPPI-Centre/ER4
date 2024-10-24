USE [Reviewer]
GO



/****** Object:  StoredProcedure [dbo].[st_ErItemListWithWithoutCodes]    Script Date: 21/10/2021 16:08:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



ALTER     PROCEDURE [dbo].[st_ErItemListWithWithoutCodes]
	@WithAttributesIds varchar(max)
    , @WithSetIdsList varchar(max)
	, @included bit = null
    , @WithOutAttributesIdsList varchar(max) = ''
    , @WithOutSetIdsList varchar(max) = ''
	, @RevId int 
      
AS

BEGIN

declare @start datetime = getdate()
declare @commas int = (LEN(@WithAttributesIds) - LEN(REPLACE(@WithAttributesIds,',','')))
	declare @commas2 int = (LEN(@WithOutAttributesIdsList) - LEN(REPLACE(@WithOutAttributesIdsList,',','')))
	
	if @commas != (LEN(@WithSetIdsList) - LEN(REPLACE(@WithSetIdsList,',','')))
		OR
		@commas2 != (LEN(@WithOutSetIdsList) - LEN(REPLACE(@WithOutSetIdsList,',',''))) RETURN


declare @items table (ItemId bigint primary key, With_atts varchar(max) null, reject varchar(max) null)
declare @attsX table (ATTRIBUTE_ID bigint primary key, SET_ID int, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
declare @attsY table (ATTRIBUTE_ID bigint primary key, SET_ID int, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
declare @segments table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)

insert into @attsX select distinct a.Attribute_id, ss.value, a.ATTRIBUTE_NAME, ATTRIBUTE_ORDER, 0 from 
	dbo.fn_Split_int(@WithAttributesIds,',') s
	inner join dbo.fn_Split_int(@WithSetIdsList,',') ss on s.idx = ss.idx and s.value > 0
	inner join TB_ATTRIBUTE_SET tas on s.value = tas.ATTRIBUTE_ID and ss.value = tas.SET_ID
	inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	
--select DATEDIFF(second,@start, getdate()) as [1]
--set @start = GETDATE()

declare @a1 bigint
if @WithAttributesIds = ''
 insert into @items select distinct ir.item_id, null, null from TB_ITEM_REVIEW ir
		where ir.REVIEW_ID = @RevId and ir.IS_DELETED = 0
			and (@included is null OR ir.IS_INCLUDED = @included) 
else 
begin 
    --we limit the @items content to only the items that have the first ATT_ID in @WithAttributesIds
	--using fn_Split_int to get the 1st one and be sure this works even when @WithAttributesIds contains two identical values (diagonal in self-crosstabs!)
	--we want to get the first one, as in rare cases, inverting the axes can ensure creating theses lists won't take forever
	set @a1 = (select top 1 ATTRIBUTE_ID from dbo.fn_Split_int(@WithAttributesIds,',') sp inner join @attsX a on a.ATTRIBUTE_ID = sp.value order by sp.idx)

	--select @a1

	insert into @items select distinct ir.item_id, @a1, null from TB_ITEM_REVIEW ir
	inner join TB_ITEM_ATTRIBUTE ia on ir.ITEM_ID = ia.ITEM_ID and ia.ATTRIBUTE_ID = @a1
	inner join tb_item_set tis on ia.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
	inner join @attsX x on x.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
		where ir.REVIEW_ID = @RevId and ir.IS_DELETED = 0
			and (@included is null OR ir.IS_INCLUDED = @included) 
end

--select DATEDIFF(second,@start, getdate()) as [2]
--set @start = GETDATE()

select ATTRIBUTE_ID, ATTRIBUTE_NAME from @attsX order by ord

 
--select DATEDIFF(second,@start, getdate()) as [3]
--set @start = GETDATE()

insert into @attsY select distinct a.Attribute_id, ss.value, a.ATTRIBUTE_NAME, ATTRIBUTE_ORDER, 0 from 
	dbo.fn_Split_int(@WithOutAttributesIdsList,',') s
	inner join dbo.fn_Split_int(@WithOutSetIdsList,',') ss on s.idx = ss.idx and s.value > 0
	inner join TB_ATTRIBUTE_SET tas on s.value = tas.ATTRIBUTE_ID and ss.value = tas.SET_ID
	inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID 
 
--select DATEDIFF(second,@start, getdate()) as [4]
--set @start = GETDATE()

select ATTRIBUTE_ID, ATTRIBUTE_NAME from @attsY order by ord
 
--select DATEDIFF(second,@start, getdate()) as [5]
--set @start = GETDATE()


update @items  set With_atts = Atts
from 
(
	select cast(@a1 as nvarchar(max)) + ',' + STRING_AGG(cast (ia.ATTRIBUTE_ID as nvarchar(max)), ',') as Atts, i.itemId as iid from @items i 
	inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 
	inner join @attsX a on  tis.SET_ID = a.SET_ID and a.ATTRIBUTE_ID != @a1
	inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and ia.ATTRIBUTE_ID = a.ATTRIBUTE_ID and tis.ITEM_ID = ia.ITEM_ID
	group by ItemId 
	--order by ItemId
) as big
WHERE ItemId = Big.iid
 
--select DATEDIFF(second,@start, getdate()) as [6]
--set @start = GETDATE() 

if @WithOutAttributesIdsList != ''
	update @items set reject = 1
	from 
	(
		select STRING_AGG(cast (ia.ATTRIBUTE_ID as nvarchar(max)), ',') as Atts, i.itemId as iid from @items i 
		inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.ITEM_ID = ia.ITEM_ID
		inner join @attsY a on ia.ATTRIBUTE_ID = a.ATTRIBUTE_ID and tis.SET_ID = a.SET_ID
		group by ItemId 
		--order by ItemId
	) as big
	WHERE ItemId = Big.iid

	 
--select DATEDIFF(second,@start, getdate()) as [7]
--set @start = GETDATE()

select ii.*, i.SHORT_TITLE from @items ii
	inner join tb_item i on ii.ItemId = i.ITEM_ID

	--order by len(with_atts)

 
--select DATEDIFF(second,@start, getdate()) as [8]
--set @start = GETDATE()

END

GO