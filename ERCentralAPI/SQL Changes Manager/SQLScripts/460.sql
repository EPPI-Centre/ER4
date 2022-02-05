USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewRoleUpdateByContactID]    Script Date: 16/12/2021 09:45:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE or ALTER  procedure [dbo].[st_ReviewRoleUpdateByContactID]
(
	@REVIEW_ID int,
	@CONTACT_ID int,
	@ROLE_NAME nvarchar(50)
)

As

SET NOCOUNT ON



	declare @REVIEW_CONTACT_ID int
	-- get REVIEW_CONTACT_ID
	set @REVIEW_CONTACT_ID = (select REVIEW_CONTACT_ID 
    from Reviewer.dbo.TB_REVIEW_CONTACT
    where REVIEW_ID = @REVIEW_ID
    and CONTACT_ID = @CONTACT_ID)

	if ((@REVIEW_CONTACT_ID = '') or (@REVIEW_CONTACT_ID is null)) 
	begin
		-- something is going wrong so let's not do anything
		declare @RESULT int = 1
	end
	else
	begin
		-- remove all existing roles for this user in this review (to avoid duplicates)
		delete from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE 
			where REVIEW_CONTACT_ID = @REVIEW_CONTACT_ID

		-- add this role
		insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
			values (@REVIEW_CONTACT_ID, @ROLE_NAME)
	end



SET NOCOUNT OFF



GO

