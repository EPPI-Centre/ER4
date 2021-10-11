USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_ContactDetailsGetAllFilter_2]    Script Date: 06/10/2021 10:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactDetailsGetAllFilter_2] 
(
	@ER4AccountsOnly bit,
	@TEXT_BOX nvarchar(255),
	@SiteLicence nvarchar(10)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 
	if @ER4AccountsOnly = 1
	begin        		
		if @SiteLicence != '0'
		begin			
			select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL from Reviewer.dbo.TB_CONTACT c
              inner join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID and lc.SITE_LIC_ID = @SiteLicence
              where ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
				(c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
				(c.EMAIL like '%' + @TEXT_BOX + '%'))		
			group by c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL	
		end
		else
		begin	
			select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
			max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
					CASE when l.[EXPIRY_DATE] is not null 
					and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
						then l.[EXPIRY_DATE]
					else c.[EXPIRY_DATE]
					end as 'EXPIRY_DATE', 
			c.MONTHS_CREDIT, c.CREATOR_ID,
			c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
			l.SITE_LIC_ID, l.SITE_LIC_NAME
			,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
			from Reviewer.dbo.TB_CONTACT c
			left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
			on c.CONTACT_ID = lt.CONTACT_ID
			left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
			left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
			
			--where (c.EXPIRY_DATE > '2010-03-20 00:00:01' or
			--(c.EXPIRY_DATE is null and MONTHS_CREDIT != 0))
			
			where (c.EXPIRY_DATE > '2010-03-20 00:00:01' or
			(c.EXPIRY_DATE is null /*and MONTHS_CREDIT != 0*/))
			
			and ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
				(c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
				(c.USERNAME like '%' + @TEXT_BOX + '%') OR
				(c.EMAIL like '%' + @TEXT_BOX + '%'))
			
			group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
			c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
			c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME	
		end
	end
	else
	begin
		select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
		max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
				CASE when l.[EXPIRY_DATE] is not null 
				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then l.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end as 'EXPIRY_DATE', 
		c.MONTHS_CREDIT, c.CREATOR_ID,
		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
		from Reviewer.dbo.TB_CONTACT c
		left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
		on c.CONTACT_ID = lt.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
		where ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
				(c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
				(c.USERNAME like '%' + @TEXT_BOX + '%') OR
				(c.EMAIL like '%' + @TEXT_BOX + '%'))
		
		group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		
	end
       
END

GO