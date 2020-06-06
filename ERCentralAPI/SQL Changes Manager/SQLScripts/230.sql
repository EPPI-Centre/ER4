USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchedPapersClear]    Script Date: 06/06/2020 11:43:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_item_mag_match based on manual lookup
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagMatchedPapersClear] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_ID BIGINT
,	@ATTRIBUTE_ID BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if @ITEM_ID > 0
		delete from tb_ITEM_MAG_MATCH
		where ITEM_ID = @ITEM_ID and REVIEW_ID = @REVIEW_ID

	else
		if @ATTRIBUTE_ID > 0
			delete from tb_ITEM_MAG_MATCH
			where REVIEW_ID = REVIEW_ID and item_id in
				(select ia.ITEM_ID from TB_ITEM_ATTRIBUTE ia inner join TB_ITEM_SET iset on
					iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'True'
					where ia.ATTRIBUTE_ID = @ATTRIBUTE_ID)

		else
			delete from tb_ITEM_MAG_MATCH
			where REVIEW_ID = @REVIEW_ID
    
END
GO