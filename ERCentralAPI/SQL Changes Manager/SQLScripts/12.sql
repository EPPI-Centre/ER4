USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Add_Remove_Admin]    Script Date: 12/04/2018 10:56:23 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Organisation_Add_Remove_Admin]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Organisation_Add_Remove_Admin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Add_Remove_Admin]    Script Date: 12/04/2018 10:56:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Organisation_Add_Remove_Admin]
	-- Add the parameters for the stored procedure here
	@org_id int
	, @admin_ID int
	, @contact_email nvarchar(500)
	, @res int output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here
    declare @contact_id int
	set @res = 0
    
    -- initial check to see if email exists
	select @contact_id = CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = @contact_email and EXPIRY_DATE > '2010-03-20 00:00:01'
	if @@ROWCOUNT = 1 
	begin
		declare @ck int
		set @ck = (SELECT count(contact_id) from TB_ORGANISATION_ADMIN 
		where (CONTACT_ID = @admin_ID or @admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1
				and EXPIRY_DATE > '2010-03-20 00:00:01')) -- we only want ER4 accounts
				and ORGANISATION_ID = @org_id)
		if @ck < 1 set @res = -1 --first check: see if supplied admin is actually an admin!
		else if  (select count(contact_id) from Reviewer.dbo.TB_CONTACT where @contact_email = EMAIL 
			and @contact_id = CONTACT_ID
			and EXPIRY_DATE > '2010-03-20 00:00:01') != 1
			--second check, see if c_id and email exist
			set @res = -2
		else -- checks went OK, let's see if we can do it
		begin
			set @ck = (select count(contact_id) from TB_ORGANISATION_ADMIN where CONTACT_ID = @contact_id and ORGANISATION_ID = @org_id)
			if @ck = 1 -- contact is an admin, we should remove it
			begin
				delete from TB_ORGANISATION_ADMIN where CONTACT_ID = @contact_id and @org_id = ORGANISATION_ID
				if @@ROWCOUNT = 1
				begin --write success log
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'remove contact')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin'
					--	from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc
				end
				else --write failure log, if this is fired, there is a bug somewhere
				begin
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'remove admin: failed!')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin: failed!'
					--	from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc	
					set @res = -3
				end
			end	
			
			else --contact is not an admin, we should add it
			begin
				insert into TB_ORGANISATION_ADMIN (CONTACT_ID, ORGANISATION_ID)
				values (@contact_id, @org_id)
				if @@ROWCOUNT = 1
				begin
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'add admin')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin'
					--	from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc
				end
				else
				begin
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'add admin: failed!')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin: failed!'
					--	from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc	
					set @res = -4
				end
			end
		end
	end
	--select c.CONTACT_ID, CONTACT_NAME, EMAIL from TB_SITE_LIC_ADMIN a
	--	inner join Reviewer.dbo.TB_CONTACT c on a.CONTACT_ID = c.CONTACT_ID
	--where SITE_LIC_ID = @lic_id
	
	return @res 
	-- error codes: -1 = supplied admin_id is not an admin of this site lice
	--				-2 = contact_id already in a site license
	--				-3 = no allowance available, all account slots for current license have been used
	--				-4 = tried to remove account but couldn't write changes! BUG ALERT
	--				-5 = tried to add account but couldn't write changes! BUG ALERT
	--				-6 = email check returned no contact_id or multiple contact_ids
	
END





GO


