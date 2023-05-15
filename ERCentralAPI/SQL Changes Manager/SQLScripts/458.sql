USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_GhostReviewActivate]    Script Date: 23/11/2021 15:02:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE or ALTER PROCEDURE [dbo].[st_GhostReviewActivate]
	-- Add the parameters for the stored procedure here
	@revID int,
	@Name Nvarchar(1000) = ''
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	declare @funderID int
	declare @months_credit smallint
	select @months_credit = MONTHS_CREDIT from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @revID
	
	if @months_credit != 0
	begin
		--WAITFOR DELAY '00:00:10'; --for testing

		-- Insert statements for procedure here
		if @Name = ''
		begin
			update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(month,MONTHS_CREDIT, GETDATE())
			,REVIEW_NAME = 'Please Edit (ID=' & @revID & ')' where REVIEW_ID = @revID 
		end
		else
		begin
			update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(month,MONTHS_CREDIT, GETDATE())
				,REVIEW_NAME = @Name where REVIEW_ID = @revID 
		end
	
	
	
		-- retrieve and then the months credit
		--select @months_credit = MONTHS_CREDIT from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @revID 
		update Reviewer.dbo.TB_REVIEW set MONTHS_CREDIT = 0 where REVIEW_ID = @revID 

		select @funderID = FUNDER_ID from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @revID

		-- add a line to TB_EXPIRY_EDIT_LOG to say the review has been activated
		insert into ReviewerAdmin.dbo.TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, OLD_EXPIRY_DATE,
			NEW_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
		values (GETDATE(), 0, @revID, null, DATEADD(month, @months_credit, GETDATE()), 
			@funderID, 18, 'The FunderID activated the review')

	end
	
END
GO
