USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchItemsGetIdList]    Script Date: 12/02/2021 19:26:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Match EPPI-Reviewer TB_ITEM records to MAG Papers
-- =============================================
ALTER PROCEDURE [dbo].[st_MagMatchItemsGetIdList] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ATTRIBUTE_ID BIGINT = 0
AS
BEGIN

DECLARE @ITEMS TABLE
(
	ITEM_ID bigint
)

if @ATTRIBUTE_ID > 0
begin
	insert into @ITEMS
	select distinct tir.ITEM_ID
		from Reviewer.dbo.TB_ITEM_REVIEW tir 
		inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = tir.ITEM_ID and tia.ATTRIBUTE_ID = @ATTRIBUTE_ID
		inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
		where tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
			and not tir.ITEM_ID IN (SELECT ITEM_ID FROM tb_ITEM_MAG_MATCH IMM
				WHERE IMM.ITEM_ID = TIR.ITEM_ID AND IMM.REVIEW_ID = @REVIEW_ID)
end
else
begin
	INSERT INTO @ITEMS
	select distinct tir.ITEM_ID
		from Reviewer.dbo.TB_ITEM_REVIEW tir where tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
		and not tir.ITEM_ID IN (SELECT ITEM_ID FROM tb_ITEM_MAG_MATCH IMM
				WHERE IMM.ITEM_ID = TIR.ITEM_ID AND IMM.REVIEW_ID = @REVIEW_ID)
end

SELECT ITEM_ID FROM @ITEMS

END
GO