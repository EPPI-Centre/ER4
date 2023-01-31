USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonInsert]    Script Date: 27/01/2023 09:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_ComparisonsAllPotentialPairs]
(
	@revId INT,
	@SetId INT
)

As

SET NOCOUNT ON

	declare @users table (cid int primary key, CONTACT_NAME nvarchar(255)) 

	declare @items table (itemId bigint primary key, done bit)
	insert into @items select distinct tis.ITEM_ID, 0 from
	TB_ITEM_SET tis
	inner join TB_CONTACT c on tis.SET_ID = @SetId and tis.CONTACT_ID = c.CONTACT_ID and tis.IS_COMPLETED = 0
	where tis.ITEM_ID not in (select ITEM_ID from TB_ITEM_SET tis2 where tis2.SET_ID = @SetId and tis2.IS_COMPLETED = 1)


	insert into @users select c.CONTACT_ID, CONTACT_NAME from
	@items i inner join TB_ITEM_SET tis on i.itemId = tis.ITEM_ID
	inner join TB_CONTACT c on tis.SET_ID = @SetId and tis.CONTACT_ID = c.CONTACT_ID and tis.IS_COMPLETED = 0
	group by c.contact_id, c.CONTACT_NAME

	--select * from @users 

	declare @pairs table (cid1 int, cid2 int, items_count int)
	insert into @pairs 
		select distinct u1.cid , u2.cid, 0 from 
		@users u1 inner join @users u2 on u1.cid > u2.cid 
	
	--select p.cid1, u.CONTACT_NAME, p.cid2, u2.CONTACT_NAME from @pairs p inner join @users u on p.cid1 = u.cid inner join @users u2 on p.cid2 = u2.cid
	--order by p.cid1


	select p.cid1, p.cid2, count(tis1.item_id) as OverlapCount from @pairs p
	inner join TB_ITEM_SET tis1 on tis1.CONTACT_ID = p.cid1 and tis1.SET_ID = @SetId and tis1.IS_COMPLETED = 0
	inner join TB_ITEM_SET tis2 on tis2.CONTACT_ID = p.cid2 and tis1.ITEM_ID = tis2.ITEM_ID and tis2.SET_ID = @SetId and tis2.IS_COMPLETED = 0
	inner join @items i on tis1.ITEM_ID	 = i.itemId
	group by p.cid1, p.cid2
	order by OverlapCount desc

SET NOCOUNT OFF
GO