USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeBulkDelete]    Script Date: 4/3/2025 4:05:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemAttributeBulkDelete]
(
	@ATTRIBUTE_ID BIGINT,
	@ITEM_ID_LIST varchar(max),
	@SEARCH_ID_LIST varchar(max),
	@SET_ID INT,
	@CONTACT_ID INT,
	@REVIEW_ID INT
)
With Recompile
As
SET NOCOUNT ON



	DECLARE @ITEM_IDS TABLE
	(
		value bigint primary key
	)
	


	IF (@SEARCH_ID_LIST = '')
	BEGIN
		INSERT INTO @ITEM_IDS (VALUE) SELECT DISTINCT value FROM DBO.fn_split_int(@ITEM_ID_LIST, ',')
	END
	ELSE
	BEGIN
		INSERT INTO @ITEM_IDS (VALUE)
			SELECT DISTINCT ITEM_ID FROM TB_SEARCH_ITEM INNER JOIN DBO.fn_split_int(@SEARCH_ID_LIST, ',') ON
				TB_SEARCH_ITEM.SEARCH_ID = value
	END

	DELETE FROM TB_ITEM_ATTRIBUTE
		from TB_ITEM_ATTRIBUTE
		inner join @ITEM_IDS as theList on theList.value = tb_item_attribute.item_id 
		inner join TB_ITEM_REVIEW on TB_ITEM_REVIEW.ITEM_ID = theList.value and TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
			and TB_ITEM_SET.IS_COMPLETED = 'FALSE'
		where TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID
		AND NOT TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID IN -- so we don't delete uncompleted versions as well as completed 
													   -- (if there's a completed version, that is)
		(
			SELECT ITEM_ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE IA2
			inner join @ITEM_IDS as theList on theList.value = tb_item_attribute.item_id 
			inner join TB_ITEM_REVIEW on TB_ITEM_REVIEW.ITEM_ID = theList.value and TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
				and TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			where TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID
		)
		
		DELETE FROM TB_ITEM_ATTRIBUTE
		from TB_ITEM_ATTRIBUTE
		inner join @ITEM_IDS as theList on theList.value = tb_item_attribute.item_id 
		inner join TB_ITEM_REVIEW on TB_ITEM_REVIEW.ITEM_ID = theList.value and TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			and TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		where TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID
		
/*
IF (@IS_COMPLETED = 'TRUE')
	BEGIN 
		DELETE FROM TB_ITEM_ATTRIBUTE
		from TB_ITEM_ATTRIBUTE
		inner join @ITEM_IDS as theList on theList.value = tb_item_attribute.item_id 
		inner join TB_ITEM_REVIEW on TB_ITEM_REVIEW.ITEM_ID = theList.value and TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		where TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID
	END
	ELSE
	BEGIN
		DELETE FROM TB_ITEM_ATTRIBUTE
		from TB_ITEM_ATTRIBUTE
		inner join @ITEM_IDS as theList on theList.value = tb_item_attribute.item_id 
		inner join TB_ITEM_REVIEW on TB_ITEM_REVIEW.ITEM_ID = theList.value and TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
			and TB_ITEM_SET.IS_COMPLETED = 'FALSE'
		where TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID
		
		DELETE FROM TB_ITEM_ATTRIBUTE
		from TB_ITEM_ATTRIBUTE
		inner join @ITEM_IDS as theList on theList.value = tb_item_attribute.item_id 
		inner join TB_ITEM_REVIEW on TB_ITEM_REVIEW.ITEM_ID = theList.value and TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			and TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		where TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID
	END
*/

	DELETE FROM TB_ITEM_SET
	WHERE NOT ITEM_SET_ID IN 
	(
		SELECT DISTINCT ia.ITEM_SET_ID  
		FROM TB_ITEM_ATTRIBUTE ia
		inner join tb_item_review ir on ia.ITEM_ID = ir.ITEM_ID
		inner join TB_ITEM_SET tis on ia.ITEM_SET_ID = tis.ITEM_SET_ID and tis.SET_ID = @SET_ID
		and ir.REVIEW_ID = @REVIEW_ID
		union
		select tio.item_set_id
		from TB_ITEM_OUTCOME tio
		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.SET_ID = @SET_ID
		inner join TB_ITEM_REVIEW ir on tis.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	) and SET_ID = @SET_ID


SET NOCOUNT OFF

GO