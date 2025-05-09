USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesGetGroupMembersForScoring]    Script Date: 26/11/2020 14:22:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_ItemDuplicatesGetGroupMembersForScoring] 
	-- Add the parameters for the stored procedure here
	(
		@REVIEW_ID int,
		@ITEM_IDS nvarchar(max)
	)
AS
BEGIN
	declare @t Table (item_id bigint, HAS_CODES int)
	insert into @t select i.value, 0 from dbo.fn_Split_int(@ITEM_IDS, ',') i 
		inner join TB_ITEM_REVIEW ir on i.value = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	
	update A set HAS_CODES = 1 from 
		(select t.item_id, HAS_CODES from @t t inner join TB_ITEM_SET s on t.item_id = s.ITEM_ID) A

	select i.ITEM_ID, dbo.fn_REBUILD_AUTHORS(i.ITEM_ID, 0) as AUTHORS, TITLE, PARENT_TITLE, [YEAR]
		, VOLUME, PAGES, ISSUE, DOI, ABSTRACT, HAS_CODES, 0 as IS_MASTER, i.TYPE_ID
	from @t t inner join TB_ITEM i on t.item_id = I.ITEM_ID 
END
GO