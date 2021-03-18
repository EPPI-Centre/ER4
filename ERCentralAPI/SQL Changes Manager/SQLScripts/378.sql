USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchedPapersClearNonManual]    Script Date: 18/03/2021 10:13:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_item_mag_match based on manual lookup
-- =============================================
create or alter PROCEDURE [dbo].[st_MagMatchedPapersClearNonManual] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ATTRIBUTE_ID BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

		if @ATTRIBUTE_ID > 0
			delete from tb_ITEM_MAG_MATCH
			where REVIEW_ID = @REVIEW_ID and ManualFalseMatch is null and ManualTrueMatch is null
			and item_id in
				(select ia.ITEM_ID from TB_ITEM_ATTRIBUTE ia inner join TB_ITEM_SET iset on
					iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'True'
					where ia.ATTRIBUTE_ID = @ATTRIBUTE_ID)

		else
			delete from tb_ITEM_MAG_MATCH
			where REVIEW_ID = @REVIEW_ID and ManualFalseMatch is null and ManualTrueMatch is null
    
END
GO