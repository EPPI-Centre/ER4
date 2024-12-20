USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupMembers]    Script Date: 04/10/2021 17:57:27 ******/
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
				and s.REVIEW_ID = @ReviewID
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
		,I.DOI
	from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM
	INNER JOIN TB_ITEM_DUPLICATE_GROUP G on G.ITEM_DUPLICATE_GROUP_ID = GM.ITEM_DUPLICATE_GROUP_ID
	INNER JOIN TB_ITEM_REVIEW IR on GM.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.REVIEW_ID = @ReviewID
	INNER JOIN TB_ITEM I on IR.ITEM_ID = I.ITEM_ID
	INNER JOIN TB_ITEM_TYPE IT on I.TYPE_ID = IT.TYPE_ID
	WHERE G.ITEM_DUPLICATE_GROUP_ID = @GroupID
	
	SELECT ORIGINAL_ITEM_ID ORIGINAL_MASTER_ID from TB_ITEM_DUPLICATE_GROUP where ITEM_DUPLICATE_GROUP_ID = @GroupID
	
	
END

GO