USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchedPapersInsert]    Script Date: 02/02/2020 10:29:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_item_mag_match based on manual lookup
-- =============================================
create or alter PROCEDURE [dbo].[st_MagMatchedPapersInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_ID BIGINT
,	@PaperId bigint
,	@AutoMatchScore float
,	@ManualTrueMatch bit = null
,	@ManualFalseMatch bit = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	declare @chk int
	select @chk = count(*) from tb_ITEM_MAG_MATCH
		where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId

	if @chk = 0
	begin
		insert into tb_ITEM_MAG_MATCH(REVIEW_ID, ITEM_ID, PaperId, ManualTrueMatch, ManualFalseMatch, AutoMatchScore)
		values (@REVIEW_ID, @ITEM_ID, @PaperId, @ManualTrueMatch, @ManualFalseMatch, @AutoMatchScore)
	end
	else
	begin
		update Reviewer.dbo.tb_ITEM_MAG_MATCH
			set ManualTrueMatch = @ManualTrueMatch,
				ManualFalseMatch = @ManualFalseMatch
			where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId
	end
END
