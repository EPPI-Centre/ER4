USE [ReviewerAdmin]
GO
IF COL_LENGTH('dbo.TB_LOGON_TICKET', 'CLIENT') IS NULL
BEGIN
    

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION

ALTER TABLE dbo.TB_LOGON_TICKET ADD
	CLIENT nvarchar(10) NULL

ALTER TABLE dbo.TB_LOGON_TICKET SET (LOCK_ESCALATION = TABLE)

COMMIT
END
GO

USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Insert]    Script Date: 15/02/2019 15:35:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sergio
-- ALTER date: 
-- Description:	ALTER a new ticket, relies on default values of most of the columns
-- =============================================
ALTER PROCEDURE [dbo].[st_LogonTicket_Insert] 
	-- Add the parameters for the stored procedure here
	@Contact_ID int = 0, 
	@Review_ID int = 0,
	@Client nvarchar(10) = 'ER4',
	@GUID uniqueidentifier OUTPUT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here
    set @GUID = newid()
    UPDATE TB_LOGON_TICKET
    SET STATE = 0
    WHERE CONTACT_ID = @Contact_ID AND STATE = 1
    INSERT into TB_LOGON_TICKET(TICKET_GUID, CONTACT_ID, REVIEW_ID, CLIENT)
	VALUES (@GUID, @Contact_ID, @Review_ID, @Client)
	
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ContactLoginReview]    Script Date: 14/02/2019 11:02:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ContactLoginReview]
(
	@userId int,
	@reviewId int,
	@IsArchieUser bit,
	@Client nvarchar(10) = 'ER4',
	@GUI uniqueidentifier OUTPUT
)

As
if @IsArchieUser = 1
begin
--we need to make sure user has records in TB_REVIEW_CONTACT and TB_CONTACT_REVIEW_ROLE
	declare @ck int = (select count(crr.ROLE_NAME) from TB_REVIEW_CONTACT rc 
		inner join TB_CONTACT_REVIEW_ROLE crr on rc.REVIEW_ID = @reviewId and rc.CONTACT_ID = @userId and crr.REVIEW_CONTACT_ID = rc.REVIEW_CONTACT_ID
		)
	if @ck < 1
	BEGIN
		DECLARE @NEW_CONTACT_REVIEW_ID INT
		INSERT INTO TB_REVIEW_CONTACT(CONTACT_ID, REVIEW_ID)
		VALUES (@userId, @reviewId)
		SET @NEW_CONTACT_REVIEW_ID = @@IDENTITY
		INSERT INTO TB_CONTACT_REVIEW_ROLE(REVIEW_CONTACT_ID, ROLE_NAME)
		VALUES(@NEW_CONTACT_REVIEW_ID, 'RegularUser')
	END
end
SELECT TB_REVIEW.REVIEW_ID, TB_REVIEW.ARCHIE_ID, ROLE_NAME as [ROLE], 
		( CASE WHEN sl2.[EXPIRY_DATE] is not null
				and sl2.[EXPIRY_DATE] > TB_REVIEW.[EXPIRY_DATE]
					then sl2.[EXPIRY_DATE]
				else TB_REVIEW.[EXPIRY_DATE]
				end
		) as REVIEW_EXP, 
		( CASE when sl.[EXPIRY_DATE] is not null 
				and sl.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then sl.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end
		) as CONTACT_EXP,
		FUNDER_ID, tb_review.ARCHIE_ID, IS_CHECKEDOUT_HERE, IS_SITE_ADMIN
		
FROM TB_REVIEW_CONTACT
	INNER JOIN TB_REVIEW on TB_REVIEW_CONTACT.REVIEW_ID = TB_REVIEW.REVIEW_ID
	INNER JOIN TB_CONTACT c on TB_REVIEW_CONTACT.CONTACT_ID = c.CONTACT_ID
	INNER JOIN TB_CONTACT_REVIEW_ROLE on TB_CONTACT_REVIEW_ROLE.REVIEW_CONTACT_ID = TB_REVIEW_CONTACT.REVIEW_CONTACT_ID
	Left outer join TB_SITE_LIC_CONTACT lc on lc.CONTACT_ID = c.CONTACT_ID
	Left outer join TB_SITE_LIC sl on lc.SITE_LIC_ID = sl.SITE_LIC_ID
	left outer join TB_SITE_LIC_REVIEW lr on TB_REVIEW.REVIEW_ID = lr.REVIEW_ID
	left outer join TB_SITE_LIC sl2 on lr.SITE_LIC_ID = sl2.SITE_LIC_ID
	
WHERE TB_REVIEW.REVIEW_ID = @reviewId AND c.CONTACT_ID = @userId

IF @@ROWCOUNT >= 1 
	BEGIN
	DECLARE	@return_value int,
			@GUID uniqueidentifier
			
	EXEC	@return_value = [ReviewerAdmin].[dbo].[st_LogonTicket_Insert]
			@Contact_ID = @userId,
			@Review_ID = @reviewId,
			@Client = @Client,
			@GUID = @GUI OUTPUT
	SELECT	@GUI as N'@GUID'
	END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ContactAdminLoginReview]    Script Date: 15/02/2019 16:05:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER procedure [dbo].[st_ContactAdminLoginReview]
(
	@userId int,
	@reviewId int,
	@Client nvarchar(10) = 'ER4',
	@GUI uniqueidentifier OUTPUT
)

As
declare @chk int
set @chk = (select count (CONTACT_ID) from TB_CONTACT where IS_SITE_ADMIN = 1 and CONTACT_ID = @userId)
if @chk != 1
begin
	RETURN --do nothing if user is not actually a site admin
end

SELECT  TB_REVIEW.REVIEW_ID, 'AdminUser' as [ROLE], 
		( CASE WHEN sl2.[EXPIRY_DATE] is not null
				and sl2.[EXPIRY_DATE] > TB_REVIEW.[EXPIRY_DATE]
					then sl2.[EXPIRY_DATE]
				else TB_REVIEW.[EXPIRY_DATE]
				end
		) as REVIEW_EXP, 
		(SELECT  CASE when sl.[EXPIRY_DATE] is not null 
				and sl.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then sl.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end
				from TB_CONTACT c 
				Left outer join TB_SITE_LIC_CONTACT lc on lc.CONTACT_ID = c.CONTACT_ID
				Left outer join TB_SITE_LIC sl on lc.SITE_LIC_ID = sl.SITE_LIC_ID
				where c.CONTACT_ID = @userId
		) as CONTACT_EXP,
		FUNDER_ID
FROM TB_REVIEW 
	left outer join TB_SITE_LIC_REVIEW lr on TB_REVIEW.REVIEW_ID = lr.REVIEW_ID
	left outer join TB_SITE_LIC sl2 on lr.SITE_LIC_ID = sl2.SITE_LIC_ID
	
WHERE TB_REVIEW.REVIEW_ID = @reviewId 

IF @@ROWCOUNT >= 1 
	BEGIN
	DECLARE	@return_value int,
			@GUID uniqueidentifier
			
	EXEC	@return_value = [ReviewerAdmin].[dbo].[st_LogonTicket_Insert]
			@Contact_ID = @userId,
			@Review_ID = @reviewId,
			@Client = @Client,
			@GUID = @GUI OUTPUT
	SELECT	@GUI as N'@GUID'
	END
GO
