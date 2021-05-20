USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_PushBackForTesting]    Script Date: 04/05/2021 15:17:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO









CREATE OR ALTER   procedure [dbo].[st_Site_Lic_PushBackForTesting]
(
	@SITE_LIC_ID int
)

As

SET NOCOUNT ON

	declare @tb_package_table table (tv_site_lic_details_id int, tv_date_created datetime, 
		tv_months int, tv_valid_from datetime)

	insert into @tb_package_table (tv_site_lic_details_id, tv_date_created, tv_months, tv_valid_from)
	select SITE_LIC_DETAILS_ID, DATE_CREATED, MONTHS, VALID_FROM from TB_SITE_LIC_DETAILS
	where SITE_LIC_ID = @SITE_LIC_ID
	and VALID_FROM is not null

	--select * from @tb_package_table

	declare @months int
	declare @validFrom datetime

	declare @WORKING_SITE_LIC_DETAILS_ID int
	declare SITE_LIC_DETAILS_ID_CURSOR cursor for
	select tv_site_lic_details_id FROM @tb_package_table
	open SITE_LIC_DETAILS_ID_CURSOR
	fetch next from SITE_LIC_DETAILS_ID_CURSOR
	into @WORKING_SITE_LIC_DETAILS_ID
	while @@FETCH_STATUS = 0
	begin		
		set @months = (select tv_months from @tb_package_table
		where tv_site_lic_details_id = @WORKING_SITE_LIC_DETAILS_ID)		
		
		update @tb_package_table
		set tv_date_created = DATEADD(MONTH, -@months, tv_date_created),
		tv_valid_from = DATEADD(MONTH, -@months, tv_valid_from)
		where tv_site_lic_details_id = @WORKING_SITE_LIC_DETAILS_ID

		set @validFrom = (select tv_valid_from from @tb_package_table
		where tv_site_lic_details_id = @WORKING_SITE_LIC_DETAILS_ID)

		FETCH NEXT FROM SITE_LIC_DETAILS_ID_CURSOR 
		INTO @WORKING_SITE_LIC_DETAILS_ID
	END

	CLOSE SITE_LIC_DETAILS_ID_CURSOR
	DEALLOCATE SITE_LIC_DETAILS_ID_CURSOR

	--select * from @tb_package_table

	--select * from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @SITE_LIC_ID 

	update TB_SITE_LIC_DETAILS
	set DATE_CREATED = tv_date_created,
	VALID_FROM = tv_valid_from
	from @tb_package_table
	where SITE_LIC_DETAILS_ID = tv_site_lic_details_id
	and SITE_LIC_ID = @SITE_LIC_ID -- for extra safety

	-- the last @months and @validFrom will be the latest package 
	-- so use this to adjust the license expiry date
	declare @newExpiryDate datetime
	set @newExpiryDate = DATEADD(MONTH, @months, @validFrom)

	update Reviewer.dbo.TB_SITE_LIC
	set EXPIRY_DATE = @newExpiryDate
	where SITE_LIC_ID = @SITE_LIC_ID

SET NOCOUNT OFF





GO


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_PullForwardForTesting]    Script Date: 04/05/2021 15:18:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO










CREATE OR ALTER   procedure [dbo].[st_Site_Lic_PullForwardForTesting]
(
	@SITE_LIC_ID int
)

As

SET NOCOUNT ON

	declare @tb_package_table table (tv_site_lic_details_id int, tv_date_created datetime, 
		tv_months int, tv_valid_from datetime)

	insert into @tb_package_table (tv_site_lic_details_id, tv_date_created, tv_months, tv_valid_from)
	select SITE_LIC_DETAILS_ID, DATE_CREATED, MONTHS, VALID_FROM from TB_SITE_LIC_DETAILS
	where SITE_LIC_ID = @SITE_LIC_ID
	and VALID_FROM is not null

	--select * from @tb_package_table

	declare @months int
	declare @validFrom datetime

	declare @WORKING_SITE_LIC_DETAILS_ID int
	declare SITE_LIC_DETAILS_ID_CURSOR cursor for
	select tv_site_lic_details_id FROM @tb_package_table
	open SITE_LIC_DETAILS_ID_CURSOR
	fetch next from SITE_LIC_DETAILS_ID_CURSOR
	into @WORKING_SITE_LIC_DETAILS_ID
	while @@FETCH_STATUS = 0
	begin		
		set @months = (select tv_months from @tb_package_table
		where tv_site_lic_details_id = @WORKING_SITE_LIC_DETAILS_ID)		

		update @tb_package_table
		set tv_date_created = DATEADD(MONTH, @months, tv_date_created),
		tv_valid_from = DATEADD(MONTH, @months, tv_valid_from)
		where tv_site_lic_details_id = @WORKING_SITE_LIC_DETAILS_ID

		set @validFrom = (select tv_valid_from from @tb_package_table
		where tv_site_lic_details_id = @WORKING_SITE_LIC_DETAILS_ID)

		FETCH NEXT FROM SITE_LIC_DETAILS_ID_CURSOR 
		INTO @WORKING_SITE_LIC_DETAILS_ID
	END

	CLOSE SITE_LIC_DETAILS_ID_CURSOR
	DEALLOCATE SITE_LIC_DETAILS_ID_CURSOR

	--select * from @tb_package_table

	--select * from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @SITE_LIC_ID 

	update TB_SITE_LIC_DETAILS
	set DATE_CREATED = tv_date_created,
	VALID_FROM = tv_valid_from
	from @tb_package_table
	where SITE_LIC_DETAILS_ID = tv_site_lic_details_id
	and SITE_LIC_ID = @SITE_LIC_ID -- for extra safety

	
	-- the last @months and @validFrom will be the latest package 
	-- so use this to adjust the license expiry date
	declare @newExpiryDate datetime
	set @newExpiryDate = DATEADD(MONTH, @months, @validFrom)

	update Reviewer.dbo.TB_SITE_LIC
	set EXPIRY_DATE = @newExpiryDate
	where SITE_LIC_ID = @SITE_LIC_ID

SET NOCOUNT OFF





GO

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Details_Create_or_Edit_AndOr_Activate]    Script Date: 04/05/2021 15:18:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_Site_Lic_Details_Create_or_Edit_AndOr_Activate] 
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
	, @LATEST_SITE_LIC_DETAILS_ID nvarchar(50)
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
		-- this is an existing site license package

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

				-- is this an activation?				
				declare @validFrom datetime
				declare @initialModelType int
				declare @rm2rm int = 0
				set @validFrom = (select VALID_FROM from TB_SITE_LIC_DETAILS
					where SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID)
				-- if VALID_FROM is null in the database for this package but a value is being passed 
				-- for @VALID_FROM then it is a renewal/activation
				if @validFrom is null
				begin
					-- we are activating but are we renewing as well?
					-- (activing the first package in a license is not a renewal!)
					-- is there a 'latest' package?
					if @LATEST_SITE_LIC_DETAILS_ID != 'NA'
					begin
						-- we are renewing
						-- but are we renewing as 'removeable' to 'removeable'
						set @initialModelType = (select SITE_LIC_MODEL from Reviewer.dbo.TB_SITE_LIC 
						where SITE_LIC_ID = @LIC_ID)
						if @initialModelType = 2 and @SITE_LIC_MODEL = 2
						begin
							set @rm2rm = 1
						end
					end
				end

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

				-- is this a 'removeable' to 'removeable' renewal
				if @rm2rm = 1
				begin
					
					-- how many slots are availble in the new package?
					declare @reviewsAllowance int = 
						(select REVIEWS_ALLOWANCE from TB_SITE_LIC_DETAILS 
						where SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID)

					-- rather than removing reviews from the license when the renewal has fewer slots the 
					-- check will be made in code and the EPPIAdmin can sort it out
					update Reviewer.dbo.TB_SITE_LIC_REVIEW 
					set SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
					where SITE_LIC_DETAILS_ID = @LATEST_SITE_LIC_DETAILS_ID
					and SITE_LIC_ID = @LIC_ID --extra check for safety
				end



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

