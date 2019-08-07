USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewStatisticsCodeSetsReviewersIncomplete]    Script Date: 8/7/2019 12:04:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER procedure [dbo].[st_ReviewStatisticsCodeSetsReviewersIncomplete]
(
	@REVIEW_ID INT,
	@SET_ID INT 
)
--with recompile
As

SET NOCOUNT ON
--the local variables here stop all sniffing possibilities, according to:
--https://stackoverflow.com/questions/440944/sql-server-query-fast-but-slow-from-procedure
--and: 
DECLARE @rid INT
set @rid = @REVIEW_ID
DECLARE @sid INT 
SET @sid = @SET_ID

declare @t table (ItemId bigint primary key)
declare @unt table (ItemId bigint primary key)
insert into @t SELECT distinct IS2.ITEM_ID
	FROM TB_ITEM_SET IS2
	INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = IS2.ITEM_ID AND IR2.REVIEW_ID = @rid
	WHERE IS2.IS_COMPLETED = 'TRUE' AND IS2.SET_ID = @sid AND IR2.IS_DELETED = 'FALSE'
	--option (optimize for unknown)
--select @@ROWCOUNT

insert into @unt SELECT distinct IS2.ITEM_ID
	FROM TB_ITEM_SET IS2
	INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = IS2.ITEM_ID AND IR2.REVIEW_ID = @rid
	WHERE IS2.IS_COMPLETED = 'FALSE' AND IS2.SET_ID = @sid AND IR2.IS_DELETED = 'FALSE'
	AND NOT IR2.ITEM_ID IN
	(
		select ItemId from @t
	)
	--option (optimize for unknown)
--select @@ROWCOUNT

SELECT SET_NAME, IS1.SET_ID, IS1.CONTACT_ID, CONTACT_NAME, COUNT(DISTINCT IS1.ITEM_ID) AS TOTAL
FROM @unt un Inner Join TB_ITEM_SET IS1 on IS1.ITEM_ID = un.ItemId and IS1.SET_ID = @sid
INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = IS1.SET_ID
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.REVIEW_ID = TB_REVIEW_SET.REVIEW_ID AND TB_ITEM_REVIEW.ITEM_ID = IS1.ITEM_ID
INNER JOIN TB_SET ON TB_SET.SET_ID = IS1.SET_ID
INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = IS1.CONTACT_ID
WHERE TB_ITEM_REVIEW.REVIEW_ID = @rid AND IS1.IS_COMPLETED = 'FALSE' 
--AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' 
GROUP BY IS1.SET_ID, SET_NAME, IS1.CONTACT_ID, CONTACT_NAME
--option (optimize for unknown)

SET NOCOUNT OFF
GO