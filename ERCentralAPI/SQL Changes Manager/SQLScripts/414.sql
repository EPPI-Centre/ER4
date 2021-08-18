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
	declare @items table (ItemId bigint, ItemSet int, ContactId int, ContactName varchar(255), Completed bit, [State] varchar(25), primary key(ItemId, ItemSet))

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
	select * from @fa order by [level], p_id, a_order

	insert into @items SELECT distinct tis.item_id, tis.ITEM_SET_ID, tis.CONTACT_ID, c.CONTACT_NAME, tis.IS_COMPLETED 
	,case 
		when ir.IS_INCLUDED = 1 and ir.IS_DELETED = 0 then 'Included'
		when ir.IS_INCLUDED = 0 and ir.IS_DELETED = 0 then 'Excluded'
		when ir.IS_INCLUDED = 1 and ir.IS_DELETED = 1 and ir.MASTER_ITEM_ID is not null then 'Duplicate'
		when ir.IS_INCLUDED = 1 and ir.IS_DELETED = 1 then 'In Deleted Source'
		when ir.IS_INCLUDED = 0 and ir.IS_DELETED = 1 then 'Deleted'
		else ''
	end AS [STATE]
	from tb_item_set tis
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
	

	select i.ItemId, i.Completed, i.ContactName, i.ContactId, OUTCOME_ID, SHORT_TITLE, TB_ITEM_OUTCOME.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
			ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, OUTCOME_DESCRIPTION,
			DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
			A1.ATTRIBUTE_NAME AS INTERVENTION_TEXT,
			a2.ATTRIBUTE_NAME AS CONTROL_TEXT,
			a3.ATTRIBUTE_NAME AS OUTCOME_TEXT,
			0 as META_ANALYSIS_OUTCOME_ID -- Meta-analysis id. 0 as not selected
		,	TB_ITEM_OUTCOME.ITEM_TIMEPOINT_ID
		,	TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP1
		,	TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP2
		,	CONCAT(TB_ITEM_TIMEPOINT.TIMEPOINT_VALUE, ' ', TB_ITEM_TIMEPOINT.TIMEPOINT_METRIC) TimepointDisplayValue
		,	arm1.ARM_NAME grp1ArmName
		,	arm2.ARM_NAME grp2ArmName
		FROM @items i
		inner join TB_ITEM_OUTCOME on i.ItemSet = TB_ITEM_OUTCOME.ITEM_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_OUTCOME.ITEM_SET_ID
		INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
		left outer JOIN TB_ATTRIBUTE IA1 ON IA1.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_INTERVENTION
		left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = IA1.ATTRIBUTE_ID 
		left outer JOIN TB_ATTRIBUTE IA2 ON IA2.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_CONTROL
		left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
		left outer JOIN TB_ATTRIBUTE IA3 ON IA3.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_OUTCOME
		left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = IA3.ATTRIBUTE_ID 
		left outer join TB_ITEM_TIMEPOINT ON TB_ITEM_TIMEPOINT.ITEM_TIMEPOINT_ID = TB_ITEM_OUTCOME.ITEM_TIMEPOINT_ID
		left outer join TB_ITEM_ARM arm1 ON arm1.ITEM_ARM_ID = TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP1
		left outer join TB_ITEM_ARM arm2 on arm2.ITEM_ARM_ID = TB_ITEM_OUTCOME.ITEM_ARM_ID_GRP2
	
END
GO 