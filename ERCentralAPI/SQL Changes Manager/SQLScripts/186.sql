USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_PriorityScreeningTurnOnOff]    Script Date: 27/04/2020 14:10:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE or ALTER procedure [dbo].[st_EnableMag]
(
	@REVIEW_ID int,
	@SETTING int
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	
	update Reviewer.dbo.TB_REVIEW
	set MAG_ENABLED = @SETTING
	where REVIEW_ID = @REVIEW_ID
	

END
GO


USE [Reviewer]
GO
IF NOT EXISTS 
	(SELECT * 
	FROM sys.indexes 
	WHERE name='IX_ITEM_MAG_MATCH_ITEM_ID' AND object_id = OBJECT_ID('dbo.tb_ITEM_MAG_MATCH'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_ITEM_MAG_MATCH_ITEM_ID]
	ON [dbo].[tb_ITEM_MAG_MATCH] ([ITEM_ID])
	INCLUDE ([AutoMatchScore],[ManualTrueMatch],[ManualFalseMatch])
END
GO



