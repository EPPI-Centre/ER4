USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesGetCandidatesOnSearchText]    Script Date: 7/1/2020 3:26:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE or ALTER       procedure [dbo].[st_ItemDuplicatesGetCandidatesOnSearchText]
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
select        I.ITEM_ID, I2.ITEM_ID ITEM_ID2, 
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
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = i.ITEM_ID
		INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = I2.ITEM_ID

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