USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Create_or_Edit]    Script Date: 9/27/2024 4:13:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER     PROCEDURE [dbo].[st_Site_Lic_Create_or_Edit] 
	-- Add the parameters for the stored procedure here
(
	  @LIC_ID nvarchar(50)
	, @creator_id INT
	, @admin_id  int -- who will be the first administrator of the site lic, 
	, @SITE_LIC_NAME nvarchar(50)
	, @COMPANY_NAME nvarchar(50)
	, @COMPANY_ADDRESS nvarchar(500)
	, @TELEPHONE nvarchar(50)
	, @NOTES nvarchar(4000)
	, @EPPI_NOTES nvarchar(4000)
	, @DATE_CREATED datetime
	, @SITE_LIC_MODEL int
	, @RESULT nvarchar(50) output
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @ck int
	
	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
	BEGIN TRANSACTION
	if @LIC_ID = 'N/A' -- we are creating a new site license
	BEGIN		
		declare @L_ID int
		
		insert into sTB_SITE_LIC (SITE_LIC_NAME, COMPANY_NAME,
			COMPANY_ADDRESS, TELEPHONE, NOTES, EPPI_NOTES, CREATOR_ID, SITE_LIC_MODEL)
		VALUES (@SITE_LIC_NAME, @COMPANY_NAME, @COMPANY_ADDRESS, @TELEPHONE,
			@NOTES, @EPPI_NOTES, @creator_id, @SITE_LIC_MODEL)	
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback1'
			return 
		end
		else
		begin
			set @L_ID = @@IDENTITY
			insert into TB_SITE_LIC_ADMIN([SITE_LIC_ID], [CONTACT_ID]) 
			VALUES (@L_ID, @admin_id)
			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
			begin
				ROLLBACK TRANSACTION
				set @RESULT = 'rollback2'
				return 
			end
			else
			begin
				set @RESULT = @L_ID
			end
		end		
	END
	else
	BEGIN
		-- get the existing top admin
		declare @TOP_ADM_ID int
		set @TOP_ADM_ID = (select top 1 CONTACT_ID from TB_SITE_LIC_ADMIN where SITE_LIC_ID = @LIC_ID)
				
		-- we are editing the site license details
		update sTB_SITE_LIC
		set SITE_LIC_NAME = @SITE_LIC_NAME, 
		COMPANY_NAME = @COMPANY_NAME,
		COMPANY_ADDRESS = @COMPANY_ADDRESS,
		TELEPHONE = @TELEPHONE,
		NOTES = @NOTES,
		EPPI_NOTES = @EPPI_NOTES,
		SITE_LIC_MODEL = @SITE_LIC_MODEL,
		DATE_CREATED = @DATE_CREATED
		where SITE_LIC_ID = @LIC_ID
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback3'
			return 
		end
		else
		begin	
			-- check if @admin_id is at top of list of admins
			if @TOP_ADM_ID = @admin_id -- already top admin so nothing to do
			begin
				set @RESULT = 'valid'
			end
			else  -- replace the list of admins with a new list where @admin_id is at the test
			begin
				-- create a table to contain what the admin list should look like
				declare @tb_listOfAdmins table (tv_site_lic_admin_id int, tv_site_lic_id int, tv_contact_id int)

				-- top admin
				insert into @tb_listOfAdmins (tv_site_lic_admin_id, tv_site_lic_id, tv_contact_id)
				values (0, @LIC_ID, @admin_id)

				-- rest of the admins
				insert into @tb_listOfAdmins (tv_site_lic_admin_id, tv_site_lic_id, tv_contact_id)
				select SITE_LIC_ADMIN_ID, SITE_LIC_ID, CONTACT_ID from TB_SITE_LIC_ADMIN
				where CONTACT_ID != @admin_id and SITE_LIC_ID = @LIC_ID

				-- delete the existing list
				delete from TB_SITE_LIC_ADMIN where SITE_LIC_ID = @LIC_ID
				
				-- insert the new list
				insert into TB_SITE_LIC_ADMIN (SITE_LIC_ID, CONTACT_ID)
				select tv_site_lic_id, tv_contact_id from @tb_listOfAdmins order by tv_site_lic_admin_id
				
				-- check that the top admin is now there
				select * from TB_SITE_LIC_ADMIN where CONTACT_ID = @admin_id and SITE_LIC_ID = @LIC_ID
				if @@ROWCOUNT = 0 -- top admin is missing
				begin
					ROLLBACK TRANSACTION
					set @RESULT = 'rollback4'
					return 
				end
				else
				begin			
					set @RESULT = 'valid'
				end
			end
							
		end
	END
	
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	set @RESULT = 'rollback' --to tell the code data was not committed!
	END CATCH
	
	return 

END
GO
