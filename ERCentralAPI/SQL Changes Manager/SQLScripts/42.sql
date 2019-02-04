--SERGIO Bugfix re deleting arms.
USE [Reviewer]
GO
IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_TB_ITEM_ATTRIBUTE_ATTRIBUTE_ID_ITEM_ID_ITEM_SET_ID')
DROP INDEX [IX_TB_ITEM_ATTRIBUTE_ATTRIBUTE_ID_ITEM_ID_ITEM_SET_ID] ON [dbo].[TB_ITEM_ATTRIBUTE]
GO

/****** Object:  Index [IX_TB_ITEM_ATTRIBUTE_ATTRIBUTE_ID_ITEM_ID_ITEM_SET_ID]    Script Date: 10/11/2018 10:18:23 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_ATTRIBUTE_ATTRIBUTE_ID_ITEM_ID_ITEM_SET_ID] ON [dbo].[TB_ITEM_ATTRIBUTE]
(
	[ATTRIBUTE_ID] ASC
)
INCLUDE ([ITEM_ID],
	[ITEM_SET_ID], [ITEM_ARM_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

/****** Object:  StoredProcedure [dbo].[st_ItemArmDelete]    Script Date: 10/11/2018 9:55:33 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemArmDelete]
(
	@ITEM_ARM_ID BIGINT
)

As

SET NOCOUNT ON
declare @T table (ITEM_SET_ID bigint, SET_ID int, primary key (ITEM_SET_ID, SET_ID))
declare @rid int = (select review_id from TB_ITEM_ARM ia inner join TB_ITEM_REVIEW ir on ia.ITEM_ID = ir.ITEM_ID and ia.ITEM_ARM_ID = @ITEM_ARM_ID)
insert into @T select distinct tis.ITEM_SET_ID, set_id from TB_ITEM_ATTRIBUTE ia
inner join TB_ITEM_REVIEW ir on ia.ITEM_ID = ir.ITEM_ID and REVIEW_ID = @rid
inner join tb_item_set tis on ia.ITEM_SET_ID = tis.ITEM_SET_ID  AND ITEM_ARM_ID = @ITEM_ARM_ID

select * from @T
delete from @T where ITEM_SET_ID in (Select ia.ITEM_SET_ID from TB_ITEM_ATTRIBUTE ia where ITEM_SET_ID = ia.ITEM_SET_ID and (ia.ITEM_ARM_ID != @ITEM_ARM_ID OR ia.ITEM_ARM_ID is NULL))
--@T now contains only item_set_ids that have codes applied ONLY to current ARM
Begin Transaction --make sure it's all or nothing
	DELETE FROM TB_ITEM_ATTRIBUTE where ITEM_ARM_ID = @ITEM_ARM_ID
	DELETE FROM TB_ITEM_SET where item_set_id in (select ITEM_SET_ID from @T)
	DELETE FROM TB_ITEM_ARM
		WHERE ITEM_ARM_ID = @ITEM_ARM_ID
COMMIT Transaction
SET NOCOUNT OFF
GO
