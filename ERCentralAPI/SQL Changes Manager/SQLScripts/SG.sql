USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_AttributeTextAllItems]    Script Date: 21/06/2021 17:06:52 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE OR ALTER PROCEDURE [dbo].[st_ReportAllCodingCommand] (
        @ReviewId int
		,@SetId int
)
AS
BEGIN
	SET NOCOUNT ON


	--declare @ReviewId int = 99 --7
	--	 ,@SetId int = 894 --664 --27 --1851

	declare @fa table (a_id bigint, p_id bigint, done bit, a_order int, a_name nvarchar(500), full_path nvarchar(max) null, level int null)
	declare @items table (ItemId bigint, ItemSet int, ContactId int, ContactName varchar(255), Completed bit, primary key(ItemId, ItemSet))

	insert into @fa (a_id, p_id, done, a_order, a_name)
		Select a.ATTRIBUTE_ID, tas.PARENT_ATTRIBUTE_ID, 0, tas.ATTRIBUTE_ORDER, a.ATTRIBUTE_NAME from TB_ATTRIBUTE a
			inner join TB_ATTRIBUTE_SET tas on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and dbo.fn_IsAttributeInTree(a.ATTRIBUTE_ID) = 1
			inner join TB_REVIEW_SET rs on tas.SET_ID = rs.SET_ID and rs.SET_ID = @SetId


	update f set done = 1, full_path =  f.a_name
	from @fa f
	 where done = 0  and p_id = 0

	--select * from @fa order by p_id, a_order
	declare @levind int = 0
	declare @todoLines int = (select count(*) from @fa where done=0)
	while (@todoLines > 0 AND @levind < 20)
	BEGIN
		set @levind = @levind + 1
		update f1 
		set done = 1, full_path = f2.full_path + '\' + f1.a_name , level = @levind + 1
		from @fa f1 
			inner join @fa f2 on f1.p_id = f2.a_id and f2.done = 1
		where f1.done = 0

		set @todoLines = (select count(*) from @fa where done=0) 
	END
	update @fa set level = 1 where level is null and done = 1
	select * from @fa order by p_id, a_order

	insert into @items SELECT distinct tis.item_id, tis.ITEM_SET_ID, tis.CONTACT_ID, c.CONTACT_NAME, tis.IS_COMPLETED from tb_item_set tis
		inner join TB_ITEM_REVIEW ir on tis.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @ReviewId and tis.SET_ID = @SetId
		inner join TB_CONTACT c on tis.CONTACT_ID = c.CONTACT_ID

	select i.*, tia.ATTRIBUTE_ID, tia.ITEM_ATTRIBUTE_ID, tia.ITEM_ARM_ID, fa.a_name, arm.ARM_NAME, ii.SHORT_TITLE, ii.TITLE, tia.ADDITIONAL_TEXT from @items i 
		inner join TB_ITEM_ATTRIBUTE tia on i.ItemId = tia.ITEM_ID and i.ItemSet = tia.ITEM_SET_ID
		inner join @fa fa on tia.ATTRIBUTE_ID = fa.a_id
		inner join TB_ITEM ii on i.ItemId = ii.ITEM_ID
		left join TB_ITEM_ARM arm on arm.ITEM_ARM_ID = tia.ITEM_ARM_ID 
		order by ii.SHORT_TITLE, I.ItemId, i.ContactId, level, p_id, a_order

	select i.*, tia.ATTRIBUTE_ID, tia.ITEM_ATTRIBUTE_ID, p.PAGE, replace( SELECTION_TEXTS, '¬', '<br />') as [TEXT], d.DOCUMENT_TITLE from @items i 
		inner join TB_ITEM_ATTRIBUTE tia on i.ItemId = tia.ITEM_ID and i.ItemSet = tia.ITEM_SET_ID
		inner join TB_ITEM_ATTRIBUTE_PDF p on tia.ITEM_ATTRIBUTE_ID = p.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT d on p.ITEM_DOCUMENT_ID = d.ITEM_DOCUMENT_ID
	

	select * from @items i 
		inner join TB_ITEM_OUTCOME o on o.ITEM_SET_ID = i.ItemSet
END
GO 