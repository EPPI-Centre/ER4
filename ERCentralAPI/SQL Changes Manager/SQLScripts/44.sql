﻿--Sergio: BUGFIX, ChildFrequencies with filter was returning the wrong name for the "none" line.
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeChildFrequencies]    Script Date: 12/11/2018 14:54:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemAttributeChildFrequencies]
(
      @ATTRIBUTE_ID BIGINT = null,
      @SET_ID BIGINT,
      @IS_INCLUDED BIT,
      @FILTER_ATTRIBUTE_ID BIGINT,
      @REVIEW_ID INT
)

As

SET NOCOUNT ON

IF (@FILTER_ATTRIBUTE_ID = -1)
BEGIN

	SELECT ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID,
		  COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) AS ITEM_COUNT, ATTRIBUTE_ORDER FROM TB_ITEM_ATTRIBUTE
	      
		  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
		  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
				AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
		  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
		  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		  GROUP BY TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
		  union
		  Select 'None of the codes above', -@ATTRIBUTE_ID, -@SET_ID,  COUNT(DISTINCT ITEM_ID) AS ITEM_COUNT, 10000
			from TB_ITEM_REVIEW 
			where ITEM_ID not in 
					(
						select TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
						  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
								AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
						  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
								AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
						  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
								AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
					)
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		  ORDER BY ATTRIBUTE_ORDER
end
ELSE
BEGIN
	SELECT ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID,
		  COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) AS ITEM_COUNT, ATTRIBUTE_ORDER FROM TB_ITEM_ATTRIBUTE
	      
		  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
		  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
				AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
		  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
		  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		  INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND IA2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
		  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
		  GROUP BY TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
		  UNION
		  Select 'None of the codes above', -@ATTRIBUTE_ID, -@SET_ID,  COUNT(DISTINCT TB_ITEM_REVIEW.ITEM_ID) AS ITEM_COUNT, 10000
			from TB_ITEM_REVIEW 
			INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND IA2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
			where TB_ITEM_REVIEW.ITEM_ID not in 
					(
						select TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
						  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
								AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
						  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
								AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
						  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
								AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
						  INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND IA2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
					)
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		  ORDER BY ATTRIBUTE_ORDER
END

SET NOCOUNT OFF
GO
