USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ContactDetailsEmail]    Script Date: 15/10/2026 15:56:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_ContactDetailsEmail] 
(
	@EMAIL nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
    max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, c.[EXPIRY_DATE], c.MONTHS_CREDIT, c.CREATOR_ID,
    c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN
	from sTB_CONTACT c
	left join TB_LOGON_TICKET lt
	on c.CONTACT_ID = lt.CONTACT_ID
	where c.EMAIL like @EMAIL
	and @EMAIL not like '%FAKE%'
	
	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
	c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
    c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN

	RETURN

END
GO

