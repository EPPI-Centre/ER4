USE [tempTestReviewer]
GO

DECLARE	@return_value int
DECLARE @NEW_REVIEW_ID int

EXEC	@return_value = [dbo].[st_ReviewInsert]
		@REVIEW_NAME = 'Rev1', @CONTACT_ID = 1, @NEW_REVIEW_ID = @NEW_REVIEW_ID OUTPUT
EXEC	@return_value = [dbo].[st_ReviewInsert]
		@REVIEW_NAME = 'Rev2', @CONTACT_ID = 2, @NEW_REVIEW_ID = @NEW_REVIEW_ID OUTPUT
EXEC	@return_value = [dbo].[st_ReviewInsert]
		@REVIEW_NAME = 'Rev3', @CONTACT_ID = 3, @NEW_REVIEW_ID = @NEW_REVIEW_ID OUTPUT
EXEC	@return_value = [dbo].[st_ReviewInsert]
		@REVIEW_NAME = 'Rev4', @CONTACT_ID = 4, @NEW_REVIEW_ID = @NEW_REVIEW_ID OUTPUT
EXEC	@return_value = [dbo].[st_ReviewInsert]
		@REVIEW_NAME = 'Rev5', @CONTACT_ID = 5, @NEW_REVIEW_ID = @NEW_REVIEW_ID OUTPUT
EXEC	@return_value = [dbo].[st_ReviewInsert]
		@REVIEW_NAME = 'Rev6', @CONTACT_ID = 6, @NEW_REVIEW_ID = @NEW_REVIEW_ID OUTPUT
EXEC	@return_value = [dbo].[st_ReviewInsert]
		@REVIEW_NAME = 'Rev7', @CONTACT_ID = 7, @NEW_REVIEW_ID = @NEW_REVIEW_ID OUTPUT
EXEC	@return_value = [dbo].[st_ReviewInsert]
		@REVIEW_NAME = 'Rev8', @CONTACT_ID = 8, @NEW_REVIEW_ID = @NEW_REVIEW_ID OUTPUT
EXEC	@return_value = [dbo].[st_ReviewInsert]
		@REVIEW_NAME = 'Rev9', @CONTACT_ID = 9, @NEW_REVIEW_ID = @NEW_REVIEW_ID OUTPUT
EXEC	@return_value = [dbo].[st_ReviewInsert]
		@REVIEW_NAME = 'Rev10', @CONTACT_ID = 10, @NEW_REVIEW_ID = @NEW_REVIEW_ID OUTPUT


		
EXEC	@return_value = [dbo].[st_ReviewInsert]
		@REVIEW_NAME = 'Shared rev1 (id:12)', @CONTACT_ID = 5, @NEW_REVIEW_ID = @NEW_REVIEW_ID OUTPUT
		
EXEC	@return_value = [dbo].[st_ReviewInsert]
		@REVIEW_NAME = 'Shared rev2 (id:13)', @CONTACT_ID = 6, @NEW_REVIEW_ID = @NEW_REVIEW_ID OUTPUT

UPDATE TB_REVIEW Set  EXPIRY_DATE = DATEADD(yy, 1, GETDATE()) where Review_id > 11

USE [tempTestReviewerAdmin]
GO
DECLARE	@return_value int
EXEC	@return_value = st_ReviewAddMember @REVIEW_ID = 12, @CONTACT_ID= 4
EXEC	@return_value = st_ReviewAddMember @REVIEW_ID = 12, @CONTACT_ID= 6
EXEC	@return_value = st_ReviewAddMember @REVIEW_ID = 12, @CONTACT_ID= 7
EXEC	@return_value = st_ReviewAddMember @REVIEW_ID = 12, @CONTACT_ID= 8
EXEC	@return_value = st_ReviewAddMember @REVIEW_ID = 12, @CONTACT_ID= 9
EXEC	@return_value = st_ReviewAddMember @REVIEW_ID = 12, @CONTACT_ID= 10

EXEC	@return_value = st_ReviewRoleIsAdminUpdate @REVIEW_ID = 12 , @CONTACT_ID = 6, @IS_CHECKED = 1

EXEC	@return_value = st_ReviewAddMember @REVIEW_ID = 13, @CONTACT_ID= 5
EXEC	@return_value = st_ReviewAddMember @REVIEW_ID = 13, @CONTACT_ID= 7
EXEC	@return_value = st_ReviewAddMember @REVIEW_ID = 13, @CONTACT_ID= 8
EXEC	@return_value = st_ReviewAddMember @REVIEW_ID = 13, @CONTACT_ID= 9
EXEC	@return_value = st_ReviewAddMember @REVIEW_ID = 13, @CONTACT_ID= 10

EXEC	@return_value = st_ReviewRoleCodingOnlyUpdate @REVIEW_ID = 13 , @CONTACT_ID = 7, @IS_CHECKED = 1
EXEC	@return_value = st_ReviewRoleReadOnlyUpdate @REVIEW_ID = 13 , @CONTACT_ID = 8, @IS_CHECKED = 1

GO