USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Archive_Review]    Script Date: 28/04/2021 10:39:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Archive_Review] 
	-- Add the parameters for the stored procedure here
(
	  @LIC_ID nvarchar(50)
	, @SITE_LIC_DETAILS_ID nvarchar(50)
	, @admin_id INT 
	, @review_id nvarchar(50)
	, @RESULT nvarchar(255) output
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	set @RESULT = 'Success'
	
	if @SITE_LIC_DETAILS_ID != 'N/A' 
	BEGIN

		declare @previousPackageID int
		set @previousPackageID = (select top 1 SITE_LIC_DETAILS_ID from TB_SITE_LIC_DETAILS
			where SITE_LIC_ID = @LIC_ID
			and SITE_LIC_DETAILS_ID != @SITE_LIC_DETAILS_ID
			and VALID_FROM is not null
			order by SITE_LIC_DETAILS_ID)

		if @previousPackageID != ''
		BEGIN
			-- change the package
			update Reviewer.dbo.TB_SITE_LIC_REVIEW
			set SITE_LIC_DETAILS_ID = @previousPackageID
			where SITE_LIC_ID = @LIC_ID
			and REVIEW_ID = @review_id
			and SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID

			-- log what has happened
			insert into TB_SITE_LIC_LOG ([SITE_LIC_DETAILS_ID], [CONTACT_ID], 
					[AFFECTED_ID],[CHANGE_TYPE], [REASON])
			values (@previousPackageID, @admin_id, @review_id,
				'ARCHIVE', 'This was done manually by eppiadmin')			
		END
		else
		begin
			set @RESULT = 'Unable to move review'
		end


	END
	else
	begin
		set @RESULT = 'Unable to move review'
	end
	
	return

END






GO


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_RemoveOffer]    Script Date: 28/04/2021 10:40:08 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







CREATE OR ALTER procedure [dbo].[st_Site_Lic_RemoveOffer]
(
	@SITE_LIC_DETAILS_ID int,
	@SITE_LIC_ID int
)

As

SET NOCOUNT ON

	delete from TB_SITE_LIC_DETAILS
	where SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
	and SITE_LIC_ID = @SITE_LIC_ID


SET NOCOUNT OFF





GO



USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Create_or_Edit]    Script Date: 28/04/2021 10:41:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Create_or_Edit] 
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
		
		insert into Reviewer.dbo.TB_SITE_LIC (SITE_LIC_NAME, COMPANY_NAME,
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
		update Reviewer.dbo.TB_SITE_LIC
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
				where CONTACT_ID != @admin_id

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


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Details_Create_or_Edit_AndOr_Activate]    Script Date: 28/04/2021 10:41:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Details_Create_or_Edit_AndOr_Activate] 
	-- Add the parameters for the stored procedure here
(
	  @LIC_ID int
	, @SITE_LIC_DETAILS_ID nvarchar(50)
	, @creator_id INT 
	, @Accounts int 
	, @reviews int
	, @months int
	, @Totalprice int
	, @pricePerMonth int
	, @dateCreated date
	, @forSaleID nvarchar(50)
	, @VALID_FROM nvarchar(50)
	, @EXPIRY_DATE nvarchar(50)
	, @ER4_ADM nvarchar(50)
	, @EXTENSION_TYPE nvarchar(50)
	, @EXTENSION_NOTES nvarchar(4000)
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
	
	if @SITE_LIC_DETAILS_ID = 'N/A' -- we are creating a new site license package
	BEGIN	
	
		declare @FS_ID int
		declare @SLD_ID int
		
		insert into TB_FOR_SALE ([TYPE_NAME],[IS_ACTIVE],[LAST_CHANGED],
			[PRICE_PER_MONTH],[DETAILS])
		VALUES ('Site License', 1, GETDATE(),CAST((@Totalprice/@months) as int),'')
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback'
			return 
		end
		else
		begin
			set @FS_ID = @@IDENTITY
			INSERT into TB_SITE_LIC_DETAILS ([SITE_LIC_ID],[FOR_SALE_ID],
			[ACCOUNTS_ALLOWANCE],[REVIEWS_ALLOWANCE],[MONTHS],[VALID_FROM])
			values (@LIC_ID,@FS_ID,@Accounts,@reviews,@months,null)
				
			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
			begin
				ROLLBACK TRANSACTION
				set @RESULT = 'rollback'
				return 
			end
			else
			begin
				set @SLD_ID = @@IDENTITY
				
				if (@VALID_FROM != '') and (@EXPIRY_DATE != '')
				begin
					-- activate the package
					update Reviewer.dbo.TB_SITE_LIC 
					set EXPIRY_DATE = @EXPIRY_DATE
					where SITE_LIC_ID = @LIC_ID
				
					update TB_SITE_LIC_DETAILS 
					set VALID_FROM = @VALID_FROM
					where SITE_LIC_DETAILS_ID = @SLD_ID
					
					update TB_FOR_SALE 
					set IS_ACTIVE = 'False'
					from TB_FOR_SALE fs 
					inner join TB_SITE_LIC_DETAILS d on d.FOR_SALE_ID = fs.FOR_SALE_ID 
					and d.SITE_LIC_DETAILS_ID = @SLD_ID	
					
					insert into TB_SITE_LIC_LOG ([SITE_LIC_DETAILS_ID], [CONTACT_ID], 
						[AFFECTED_ID],[CHANGE_TYPE], [REASON])
					values (@SITE_LIC_DETAILS_ID, @ER4_ADM, @SITE_LIC_DETAILS_ID,
						'ACTIVATE', 'This was done manually, not via the shop')	
						
					set @RESULT = 'valid'
				end
				else
				begin
					set @RESULT = 'new offer'
				end
			end
		end

	END
	else
	BEGIN
		-- this is an existing site license
		update TB_SITE_LIC_DETAILS
			set ACCOUNTS_ALLOWANCE = @Accounts, 
			REVIEWS_ALLOWANCE = @reviews,
			DATE_CREATED = @dateCreated,
			MONTHS = @months
			where SITE_LIC_ID = @LIC_ID
			and SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback'
			return 
		end
		else
		begin	
			update TB_FOR_SALE
				set LAST_CHANGED = GETDATE(),
				PRICE_PER_MONTH = @pricePerMonth
				where FOR_SALE_ID = @forSaleID
			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
			begin
				ROLLBACK TRANSACTION
				set @RESULT = 'rollback'
				return 
			end
			else
			begin
				if @EXTENSION_TYPE > 0
				begin
					-- put an entry into TB_EXPIRY_EDIT_LOG			
					insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
						OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
					select GETDATE(), 2, @SITE_LIC_DETAILS_ID, @EXPIRY_DATE, s_l.EXPIRY_DATE, @ER4_ADM, @EXTENSION_TYPE, @EXTENSION_NOTES 
					from Reviewer.dbo.TB_SITE_LIC s_l
					where s_l.SITE_LIC_ID = @LIC_ID
				end
			end

			if (@VALID_FROM != '') and (@EXPIRY_DATE != '') 
			begin
				-- change expiry date or activate the package
				update Reviewer.dbo.TB_SITE_LIC 
				set EXPIRY_DATE = @EXPIRY_DATE
				where SITE_LIC_ID = @LIC_ID
			
				update TB_SITE_LIC_DETAILS 
				set VALID_FROM = @VALID_FROM
				where SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
				
				update TB_FOR_SALE 
				set IS_ACTIVE = 'False'
				from TB_FOR_SALE fs 
				inner join TB_SITE_LIC_DETAILS d on d.FOR_SALE_ID = fs.FOR_SALE_ID 
				and d.SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID	
				
				insert into TB_SITE_LIC_LOG (
				  [SITE_LIC_DETAILS_ID], [CONTACT_ID], [AFFECTED_ID]
				  ,[CHANGE_TYPE], [REASON])
				values (
				  @SITE_LIC_DETAILS_ID, @ER4_ADM, @SITE_LIC_DETAILS_ID
				  ,'Activate or Extension', 'This was done manually, not via the shop')		
				  
				-- update the license type (it might be a change)
				update Reviewer.dbo.TB_SITE_LIC 
				set SITE_LIC_MODEL = @SITE_LIC_MODEL
				where SITE_LIC_ID = @LIC_ID
			end
			set @RESULT = 'valid'

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





