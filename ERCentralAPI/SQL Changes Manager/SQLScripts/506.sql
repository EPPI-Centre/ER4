USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_SearchForItemsWithDuplicateReferences]    Script Date: 20/07/2022 10:23:24 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE or ALTER   PROCEDURE [dbo].[st_SearchForItemsWithDuplicateReferences]
(
      @SEARCH_ID int = null output
,     @CONTACT_ID nvarchar(50) = null
,     @REVIEW_ID nvarchar(50) = null
,     @SEARCH_TITLE varchar(4000) = null
,     @INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'
,     @SEARCH_ITEM_ID BIGINT = NULL

)
AS
      -- Step One: Insert record into tb_SEARCH
      EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, '', '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

      -- Step Two: Perform the search and get a hits count     

		declare @tb_duplicate_group_master_id_list table (tv_item_duplicate_group_id int, tv_original_item_id bigint)

		insert into @tb_duplicate_group_master_id_list (tv_item_duplicate_group_id, tv_original_item_id)
		select ITEM_DUPLICATE_GROUP_ID, ORIGINAL_ITEM_ID from TB_ITEM_DUPLICATE_GROUP -- the id of the master item
		where REVIEW_ID = @REVIEW_ID

		--select * from @tb_duplicate_group_master_id_list

		declare @tb_item_duplicate_group_members table (tv_item_duplicate_group_id int, tv_item_review_id bigint)

		insert into @tb_item_duplicate_group_members (tv_item_duplicate_group_id, tv_item_review_id)
		select ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS IDGM
		inner join @tb_duplicate_group_master_id_list on tv_item_duplicate_group_id = IDGM.ITEM_DUPLICATE_GROUP_ID
		where IS_CHECKED = 1 and IS_DUPLICATE = 1

		--select * from @tb_item_duplicate_group_members

		declare @tb_item_review table (tv_item_review_id bigint, tv_master_item_id bigint)

		insert into @tb_item_review (tv_master_item_id)
		select MASTER_ITEM_ID from TB_ITEM_REVIEW IR
		inner join @tb_item_duplicate_group_members idgm on idgm.tv_item_review_id = IR.ITEM_REVIEW_ID

		--select distinct tv_master_item_id from @tb_item_review

		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT tv_master_item_id, @SEARCH_ID, 1 FROM @tb_item_review

      
      -- Step Three: Update the new search record in tb_SEARCH with the number of records added
      UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID
GO


