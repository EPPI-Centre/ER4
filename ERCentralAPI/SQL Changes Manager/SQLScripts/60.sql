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
			@GUID = @GUI OUTPUT
	SELECT	@GUI as N'@GUID'
	END
GO

USE [ReviewerAdmin]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_OnlineFeedbackList]')AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_OnlineFeedbackList]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_OnlineFeedbackList]
(
	@MAX_LENGTH INT = 5000
)

As

SELECT top (@MAX_LENGTH) f.*, c.CONTACT_NAME from TB_ONLINE_FEEDBACK f
inner join Reviewer.dbo.TB_CONTACT c on f.CONTACT_ID = c.CONTACT_ID
order by f.DATE desc

GO