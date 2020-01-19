USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SearchDelete]    Script Date: 10/12/2019 18:45:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_SearchDelete]
(
	@SEARCHES NVARCHAR(MAX),
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

declare @TEMP table (SEARCH_ID int) 
insert into @TEMP SELECT SEARCH_ID FROM TB_SEARCH
INNER JOIN fn_split_int(@SEARCHES, ',') SearchList on SearchList.value = TB_SEARCH.SEARCH_ID
WHERE REVIEW_ID = @REVIEW_ID;

--Make a check here on the temp table 
if ( (SELECT COUNT(*) FROM @TEMP) != 
(SELECT COUNT(*) FROM fn_split_int(@SEARCHES, ',') ) ) return


DELETE FROM TB_SEARCH_ITEM
	FROM TB_SEARCH_ITEM INNER JOIN fn_split_int(@SEARCHES, ',') SearchList on SearchList.value = TB_SEARCH_ITEM.SEARCH_ID
		
DELETE FROM TB_SEARCH
	FROM TB_SEARCH INNER JOIN fn_split_int(@SEARCHES, ',') SearchList on SearchList.value = TB_SEARCH.SEARCH_ID

SET NOCOUNT OFF
GO
