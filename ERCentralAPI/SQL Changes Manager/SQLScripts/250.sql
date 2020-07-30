USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupMembers]    Script Date: 29/07/2020 13:03:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ItemDuplicateGroupMembers]
	-- Add the parameters for the stored procedure here
	@GroupID int,
	@ReviewID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT GM.*
		,CASE (G.MASTER_MEMBER_ID)
			when (GM.GROUP_MEMBER_ID) Then 1
			ELSE 0
		END AS IS_MASTER
		, I.ITEM_ID
		, TITLE
		, SHORT_TITLE
		, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS
		, PARENT_TITLE
		, "YEAR"
		, "MONTH"
		, PAGES
		, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENT_AUTHORS
		, TYPE_NAME
		, (SELECT COUNT (ITEM_ATTRIBUTE_ID) FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ATTRIBUTE_SET ATS on IA.ATTRIBUTE_ID = ATS.ATTRIBUTE_ID
			INNER JOIN TB_REVIEW_SET RS on ATS.SET_ID = RS.SET_ID AND RS.REVIEW_ID = G.REVIEW_ID
			 WHERE IA.ITEM_ID = I.ITEM_ID) CODED_COUNT
		, (SELECT COUNT (ITEM_DOCUMENT_ID) FROM TB_ITEM_DOCUMENT d WHERE d.ITEM_ID = I.ITEM_ID) DOC_COUNT
		, (
				SELECT SOURCE_NAME from TB_SOURCE s inner join TB_ITEM_SOURCE tis 
				on s.SOURCE_ID = tis.SOURCE_ID and tis.ITEM_ID = I.ITEM_ID
		  ) as "SOURCE"
		, case when 
					(IR.MASTER_ITEM_ID is not null and
						(
							IR.MASTER_ITEM_ID not in 
							(--current master is not coming from current group
								Select rr.ITEM_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS mm
								inner join TB_ITEM_DUPLICATE_GROUP gg on mm.GROUP_MEMBER_ID = gg.MASTER_MEMBER_ID
								inner join TB_ITEM_REVIEW rr on rr.ITEM_REVIEW_ID = mm.ITEM_REVIEW_ID
								where gg.ITEM_DUPLICATE_GROUP_ID = @GroupID
							)
							
						)
					)
					or IR.ITEM_REVIEW_ID in 
						(--current item is master of some other group 
						 select GM1.item_review_id from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM1 
							inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm2 on GM1.GROUP_MEMBER_ID != gm2.GROUP_MEMBER_ID and GM1.ITEM_DUPLICATE_GROUP_ID = @GroupID
								and GM1.ITEM_REVIEW_ID = gm2.ITEM_REVIEW_ID and gm2.ITEM_DUPLICATE_GROUP_ID != GM1.ITEM_DUPLICATE_GROUP_ID
							inner join TB_ITEM_DUPLICATE_GROUP G2 on gm2.ITEM_DUPLICATE_GROUP_ID = G2.ITEM_DUPLICATE_GROUP_ID
								and G2.MASTER_MEMBER_ID = gm2.GROUP_MEMBER_ID
							--inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM3 on G2.ITEM_DUPLICATE_GROUP_ID = GM3.ITEM_DUPLICATE_GROUP_ID 
							--	and G2.MASTER_MEMBER_ID != GM.GROUP_MEMBER_ID
							--	and GM3.IS_DUPLICATE = 1
							
								
						 
						 
						 
						 --TB_ITEM_REVIEW rrr
							--inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS mmm on mmm.ITEM_REVIEW_ID = rrr.ITEM_REVIEW_ID
							--inner join TB_ITEM_DUPLICATE_GROUP ggg on mmm.GROUP_MEMBER_ID = ggg.MASTER_MEMBER_ID 
							--inner join TB_ITEM_REVIEW rr2 on rr2.MASTER_ITEM_ID = rrr.ITEM_ID and rr2.REVIEW_ID = rrr.REVIEW_ID
							--	and ggg.ITEM_DUPLICATE_GROUP_ID != @GroupID and rrr.REVIEW_ID = ggg.REVIEW_ID
						)
			then 1--is exported: should be 1 when the member has been manually imported as a duplicate in some other group
			--it is 1 also if the current master is from another group, on the interface this makes the group member read-only
			else 0
		  END
		AS IS_EXPORTED
	from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM
	INNER JOIN TB_ITEM_DUPLICATE_GROUP G on G.ITEM_DUPLICATE_GROUP_ID = GM.ITEM_DUPLICATE_GROUP_ID
	INNER JOIN TB_ITEM_REVIEW IR on GM.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.REVIEW_ID = @ReviewID
	INNER JOIN TB_ITEM I on IR.ITEM_ID = I.ITEM_ID
	INNER JOIN TB_ITEM_TYPE IT on I.TYPE_ID = IT.TYPE_ID
	WHERE G.ITEM_DUPLICATE_GROUP_ID = @GroupID
	
	SELECT ORIGINAL_ITEM_ID ORIGINAL_MASTER_ID from TB_ITEM_DUPLICATE_GROUP where ITEM_DUPLICATE_GROUP_ID = @GroupID
	
	
END
GO 
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SourceFromReview_ID]    Script Date: 7/30/2020 10:49:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[st_SourceFromReview_ID] 
	-- Add the parameters for the stored procedure here
	@revID int = 0 
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