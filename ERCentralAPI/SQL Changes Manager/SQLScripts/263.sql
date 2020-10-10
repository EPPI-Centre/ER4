USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupMembers]    Script Date: 10/5/2020 10:48:17 AM ******/
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
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesGetCandidatesOnSearchText]    Script Date: 10/5/2020 12:03:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER         procedure [dbo].[st_ItemDuplicatesGetCandidatesOnSearchText]
(
      @REVIEW_ID INT,
	  @CONTACT_ID int
)
As

SET NOCOUNT ON
declare @Items table (item_id bigint primary key)
--FIRST of all, log that "find new duplicates" has started
insert into tb_REVIEW_JOB (REVIEW_ID, CONTACT_ID, START_TIME, END_TIME, JOB_TYPE, CURRENT_STATE) 
	VALUES (@REVIEW_ID, @CONTACT_ID, getdate(),getdate(), 'FindNewDuplicates', 'running')


--limit this to ONLY the items we need.
insert into @Items select distinct ir.Item_id from TB_ITEM_REVIEW ir 
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS IDG ON IDG.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP DG ON DG.ITEM_DUPLICATE_GROUP_ID = IDG.ITEM_DUPLICATE_GROUP_ID
		where ir.REVIEW_ID = @REVIEW_ID
			AND
			(
				ir.IS_DELETED = 0 -- ANY not deleted item
				OR (
					 DG.ITEM_DUPLICATE_GROUP_ID IS NOT NULL 
				) --IF item is deleted, ANY Member of a group
			)
			  


declare @matches table (item_id bigint, matched bigint, primary key(item_id, matched))

--get JUST the matches, ignore all other data
insert into @matches 
select distinct t.item_id, t2.item_id from @items t
inner join tb_item i on i.ITEM_ID = t.item_id
INNER JOIN TB_ITEM I2 ON I2.SearchText = I.SearchText 
inner join @Items t2 on  I2.ITEM_ID = t2.item_id and t.item_id < t2.item_id

declare @res table (ITEM_ID bigint, ITEM_ID2 bigint , HAS_CODES bit, IS_MASTER bit, HAS_CODES2 bit, IS_MASTER2 bit, searchtext nvarchar(500), searchtext2 nvarchar(500))
--Get the data that needs to be computed and/or used for sorting.
insert into @res 
select distinct I.ITEM_ID, I2.ITEM_ID ITEM_ID2, 
				  case when ise.item_id is null then 0 else 1 end as HAS_CODES,
				  case when (NOT DG.MASTER_MEMBER_ID IS NULL AND (idg.GROUP_MEMBER_ID = DG.MASTER_MEMBER_ID))
					then 1 else 0 end as IS_MASTER,
				  case when ise2.item_id is null then 0 else 1 end as HAS_CODES2,
					case when (NOT DG2.MASTER_MEMBER_ID IS NULL AND (idg2.GROUP_MEMBER_ID = DG2.MASTER_MEMBER_ID))
					then 1 else 0 end as IS_MASTER2,
				i.SearchText, i2.SearchText
				  
	from @matches m
	    inner join TB_ITEM I on i.ITEM_ID = m.item_id
		INNER JOIN TB_ITEM I2 ON I2.ITEM_ID = m.matched
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = i.ITEM_ID  and ir.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = I2.ITEM_ID  and ir2.REVIEW_ID = @REVIEW_ID

		LEFT OUTER JOIN TB_ITEM_SET ISE ON ISE.ITEM_ID = I.ITEM_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS IDG ON IDG.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP DG ON DG.ITEM_DUPLICATE_GROUP_ID = IDG.ITEM_DUPLICATE_GROUP_ID

		LEFT OUTER JOIN TB_ITEM_SET ISE2 ON ISE2.ITEM_ID = I2.ITEM_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS IDG2 ON IDG2.ITEM_REVIEW_ID = IR2.ITEM_REVIEW_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP DG2 ON DG2.ITEM_DUPLICATE_GROUP_ID = IDG2.ITEM_DUPLICATE_GROUP_ID

		where 
		--ir.IS_DELETED = 'False' and ir.REVIEW_ID = @REVIEW_ID 
		--	and ir2.IS_DELETED = 'False' and ir2.REVIEW_ID = @REVIEW_ID
		--	and i.ITEM_ID != I2.ITEM_ID and i.ITEM_ID < i2.ITEM_ID
			
		--	and 
			(idg.GROUP_MEMBER_ID is null or (NOT DG.MASTER_MEMBER_ID IS NULL
				AND (idg.GROUP_MEMBER_ID = DG.MASTER_MEMBER_ID)))
			and (idg2.GROUP_MEMBER_ID is null or (NOT DG2.MASTER_MEMBER_ID IS NULL
				AND (idg2.GROUP_MEMBER_ID = DG2.MASTER_MEMBER_ID)))


--finally, get the results, data from @res, plus additional field in TB_ITEM (twice), we can now "sort by" quickly, as all data is at hand.
select I.ITEM_ID, I2.ITEM_ID ITEM_ID2, I.TITLE, I2.TITLE TITLE2,
				[dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
                   I.PARENT_TITLE, I.[YEAR], I.VOLUME, I.PAGES, I.ISSUE, I.ABSTRACT,
				  I.DOI, 
				  CAST(HAS_CODES as int) as HAS_CODES, CAST(IS_MASTER as int) as IS_MASTER,
		[dbo].fn_REBUILD_AUTHORS(I2.ITEM_ID, 0) as AUTHORS2,
                   I2.PARENT_TITLE PARENT_TITLE2, I2.[YEAR] YEAR2, I2.VOLUME VOLUME2, I2.PAGES PAGES2,
				   I2.ISSUE ISSUE2, I2.ABSTRACT ABSTRACT2,
				  I2.DOI DOI2,
				  CAST(HAS_CODES2 as int) as HAS_CODES2, CAST(IS_MASTER2 as int) as IS_MASTER2,
		r.searchtext, r.searchtext2

	 from @res r
		inner join tb_item i on r.ITEM_ID = i.ITEM_ID
		inner join tb_item i2 on r.ITEM_ID2 = i2.ITEM_ID
order by r.SearchText, IS_MASTER, IS_MASTER2, I.ITEM_ID, i2.ITEM_ID
		
SET NOCOUNT OFF


GO