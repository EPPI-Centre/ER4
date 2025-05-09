﻿USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewStatisticsCodeSetsIncomplete]    Script Date: 5/12/2020 5:46:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER procedure [dbo].[st_ReviewStatisticsCodeSetsIncomplete]
(
	@REVIEW_ID INT
)
WITH RECOMPILE
As

SET NOCOUNT ON

SELECT SET_NAME, SET_ID, COUNT(DISTINCT ITEM_ID) AS TOTAL
FROM 
(
	SELECT SET_NAME AS SET_NAME, IS1.SET_ID, IS1.ITEM_ID FROM 
	TB_ITEM_SET IS1
	INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = IS1.SET_ID
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.REVIEW_ID = TB_REVIEW_SET.REVIEW_ID AND TB_ITEM_REVIEW.ITEM_ID = IS1.ITEM_ID
	INNER JOIN TB_SET ON TB_SET.SET_ID = IS1.SET_ID
	WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND IS1.IS_COMPLETED = 'FALSE' AND IS_DELETED != 'TRUE'
	
	EXCEPT
	
	SELECT SET_NAME, IS2.SET_ID, IS2.ITEM_ID
	FROM TB_ITEM_SET IS2
	INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = IS2.ITEM_ID AND IR2.REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'TRUE'
	INNER JOIN TB_SET ON TB_SET.SET_ID = IS2.SET_ID
	WHERE IS2.IS_COMPLETED = 'TRUE'
) AS X

GROUP BY SET_ID, SET_NAME


/*
SELECT SET_NAME, IS1.SET_ID, COUNT(DISTINCT IS1.ITEM_ID) AS TOTAL
FROM TB_ITEM_SET IS1
INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = IS1.SET_ID
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.REVIEW_ID = TB_REVIEW_SET.REVIEW_ID AND TB_ITEM_REVIEW.ITEM_ID = IS1.ITEM_ID
INNER JOIN TB_SET ON TB_SET.SET_ID = IS1.SET_ID
WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND IS1.IS_COMPLETED = 'FALSE'
AND NOT IS1.ITEM_ID IN
(
	SELECT IS2.ITEM_ID
	FROM TB_ITEM_SET IS2
	INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = IS2.ITEM_ID AND IR2.REVIEW_ID = @REVIEW_ID
	WHERE IS2.IS_COMPLETED = 'TRUE' AND IS2.SET_ID = IS1.SET_ID
)
GROUP BY IS1.SET_ID, SET_NAME
*/

SET NOCOUNT OFF

GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewWorkAllocationContact]    Script Date: 5/12/2020 5:47:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ReviewWorkAllocationContact]
(
	@REVIEW_ID INT, 
	@CONTACT_ID INT
)
with recompile
As

SELECT CONTACT_NAME, TB_WORK_ALLOCATION.CONTACT_ID, SET_NAME, TB_WORK_ALLOCATION.SET_ID,
	WORK_ALLOCATION_ID, ATTRIBUTE_NAME, TB_WORK_ALLOCATION.ATTRIBUTE_ID,
	
	(SELECT COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_WORK_ALLOCATION.ATTRIBUTE_ID)
		AS TOTAL_ALLOCATION,
		
		(SELECT COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_WORK_ALLOCATION.ATTRIBUTE_ID
			AND TB_ITEM_ATTRIBUTE.ITEM_ID IN
			(
				SELECT ITEM_ID FROM TB_ITEM_SET WHERE TB_ITEM_SET.SET_ID = TB_WORK_ALLOCATION.SET_ID AND 
					TB_ITEM_SET.CONTACT_ID = TB_WORK_ALLOCATION.CONTACT_ID
			))
		AS TOTAL_STARTED

FROM TB_WORK_ALLOCATION

INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_WORK_ALLOCATION.CONTACT_ID
INNER JOIN TB_SET ON TB_SET.SET_ID = TB_WORK_ALLOCATION.SET_ID
INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_WORK_ALLOCATION.ATTRIBUTE_ID

WHERE TB_WORK_ALLOCATION.REVIEW_ID = @REVIEW_ID AND TB_WORK_ALLOCATION.CONTACT_ID = @CONTACT_ID

GO