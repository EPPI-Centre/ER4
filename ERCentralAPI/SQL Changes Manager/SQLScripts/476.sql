USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SourceFromReview_ID]    Script Date: 3/11/2022 2:56:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[st_SourceFromReview_ID] 
	-- Add the parameters for the stored procedure here
	@revID int = 0 
with recompile
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Select SOURCE_NAME, count(*) As 'Total_Items',
		sum(CASE WHEN (tb_item_review.IS_DELETED = 1 and tb_item_review.MASTER_ITEM_ID is null) then 1 else 0 END) as 'Deleted_Items',
		sum(CASE WHEN (
				tb_item_review.IS_DELETED = 1 and tb_item_review.is_included = 1 AND tb_item_review.MASTER_ITEM_ID is NOT null
			) then 1 else 0 END) as 'Duplicates',
		TB_SOURCE.IS_DELETED,
		TB_SOURCE.Source_ID,
		0 as TO_ORDER
		from TB_SOURCE inner join
		tb_item_source on TB_SOURCE.source_id = tb_item_source.source_id
		--inner join tb_item on tb_item_source.item_id = tb_item.Item_ID
		inner join tb_item_review on tb_item_source.Item_ID = tb_item_review.Item_ID
		left outer join TB_IMPORT_FILTER on TB_IMPORT_FILTER.IMPORT_FILTER_ID = TB_SOURCE.IMPORT_FILTER_ID
	where TB_SOURCE.review_ID = @RevID AND TB_ITEM_REVIEW.REVIEW_ID = @RevID
	group by SOURCE_NAME,
			 TB_SOURCE.Source_ID,
			 TB_SOURCE.IS_DELETED
	
	
	-- get sourceless items count in a second resultset
	--Select COUNT(ir.ITEM_REVIEW_ID) as 'SourcelessItems' from tb_item_review ir 
	--	left outer join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
	--	left outer join TB_SOURCE ts on tis.SOURCE_ID = ts.SOURCE_ID and ir.REVIEW_ID = ts.REVIEW_ID
	--where ir.REVIEW_ID = @revID and ts.SOURCE_ID is null
	UNION
	Select 'NN_SOURCELESS_NN' as SOURCE_NAME, count(ir.ITEM_REVIEW_ID) As 'Total_Items',
		sum(CASE WHEN (ir.IS_DELETED = 1 and ir.MASTER_ITEM_ID is null) then 1 else 0 END) as 'Deleted_Items',
		sum(CASE WHEN (
				ir.IS_DELETED = 1 and ir.is_included = 1 AND ir.MASTER_ITEM_ID is NOT null
			) then 1 else 0 END) as 'Duplicates',
		Case 
			when COUNT(ir.ITEM_ID) = Sum(
										case when ir.IS_DELETED = 1 then 1 else 0 end
										) 
			then 1 else 0 end
		 as IS_DELETED,
		-1 as Source_ID,
		1 as TO_ORDER
		from tb_item_review ir 
		where ir.REVIEW_ID = @revID 
			and ir.ITEM_ID not in 
				(
					Select ITEM_ID from TB_SOURCE s
					inner join TB_ITEM_SOURCE tis on s.SOURCE_ID = tis.SOURCE_ID and s.REVIEW_ID = @revID
				)
		order by TO_ORDER, TB_SOURCE.Source_ID
END


GO

