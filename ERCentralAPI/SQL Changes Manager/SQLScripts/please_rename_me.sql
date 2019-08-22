


-- changes for script
--x edit TB_SITE_LIC add EPPI_NOTES nvarchar(4000) NULL
--x INSERT into TB_MANAGMENT_EMAILS add - 'HELP: EPPI Admin License Details'
--x INSERT into TB_MANAGMENT_EMAILS add - 'HELP: Using the Site License'
--x new st_Site_Lic_Get_By_ID
--x new st_Site_Lic_Get_Accounts_ADM
--x new st_GetHelpText
--x new st_DetailedExtensionRecordGet
--x new st_ChangeLicenseModel
--x new st_Site_Lic_Get_Details_By_ID
--x new st_RemoveReviewsFromPreviousPackages
--x edit st_Site_Lic_Create_or_Edit
--x edit st_Site_Lic_Get_All
--x edit st_Site_Lic_Details_Create_or_Edit_AndOr_Activate
--x edit st_Site_Lic_Add_Remove_Admin
--x edit st_Site_Lic_Get
--x change 'No extension' to 'Unrecorded' in TB_EXTENSION_TYPES









USE [Reviewer]
GO

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
GO
IF COL_LENGTH('dbo.TB_SITE_LIC', 'EPPI_NOTES') IS NULL
BEGIN
	ALTER TABLE dbo.TB_SITE_LIC ADD
		EPPI_NOTES nvarchar(4000) NULL
END
GO

COMMIT
go



USE [ReviewerAdmin]
GO
declare @check nvarchar(50) = ''
select @check = EMAIL_NAME from TB_MANAGMENT_EMAILS where EMAIL_NAME = 'HELP: EPPI Admin License Details'
if @check is null OR @check != 'HELP: EPPI Admin License Details'
begin
	INSERT into ReviewerAdmin.dbo.TB_MANAGMENT_EMAILS (EMAIL_NAME)
	VALUES ('HELP: EPPI Admin License Details')
end

select @check = EMAIL_NAME from TB_MANAGMENT_EMAILS where EMAIL_NAME = 'HELP: Using the Site License'
if @check is null OR @check != 'HELP: Using the Site License'
begin
	INSERT into ReviewerAdmin.dbo.TB_MANAGMENT_EMAILS (EMAIL_NAME)
	VALUES ('HELP: Using the Site License')
end
----------------------------------------------------------------------------------






/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_By_ID]    Script Date: 21/08/2019 15:59:00 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Site_Lic_Get_By_ID]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Site_Lic_Get_By_ID]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_By_ID]    Script Date: 21/08/2019 15:27:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Site_Lic_Get_By_ID] 
	-- Add the parameters for the stored procedure here
	
	@admin_ID int,
	@site_license_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @License TABLE
(
    SITE_LIC_ID int,
    SITE_LIC_NAME nvarchar(50),
	COMPANY_NAME nvarchar(50),
	COMPANY_ADDRESS nvarchar(500),
	TELEPHONE nvarchar(50),
	NOTES nvarchar(4000),
	EPPI_NOTES nvarchar(4000),
	T_CREATOR_ID int,
	CREATOR_NAME nvarchar(255),
	ADMINISTRATOR_ID int,
	ADMINISTRATOR_NAME nvarchar(255),
	DATE_CREATED date,
	SITE_LIC_MODEL int,
	ALLOW_REVIEW_OWNERSHIP_CHANGE bit
)

	INSERT INTO @License (SITE_LIC_ID, SITE_LIC_NAME, COMPANY_NAME,
	COMPANY_ADDRESS, TELEPHONE, NOTES, EPPI_NOTES, T_CREATOR_ID, DATE_CREATED, ADMINISTRATOR_ID, SITE_LIC_MODEL, ALLOW_REVIEW_OWNERSHIP_CHANGE)
	select sl.SITE_LIC_ID, sl.SITE_LIC_NAME, sl.COMPANY_NAME, sl.COMPANY_ADDRESS,
	sl.TELEPHONE, sl.NOTES, sl.EPPI_NOTES, sl.CREATOR_ID, sl.DATE_CREATED, sla.CONTACT_ID, sl.SITE_LIC_MODEL, sl.ALLOW_REVIEW_OWNERSHIP_CHANGE
	from Reviewer.dbo.TB_SITE_LIC sl
	inner join TB_SITE_LIC_ADMIN sla on sla.SITE_LIC_ID = sl.SITE_LIC_ID
	where sla.CONTACT_ID = @admin_ID
	and sla.SITE_LIC_ID = @site_license_id

	update @License
	set CREATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = T_CREATOR_ID)
	
	update @License
	set ADMINISTRATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = ADMINISTRATOR_ID)

	select * from @License
	

END





GO


------------------------------------------------------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Accounts_ADM]    Script Date: 21/08/2019 15:59:00 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Site_Lic_Get_Accounts_ADM]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Site_Lic_Get_Accounts_ADM]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Accounts_ADM]    Script Date: 21/08/2019 15:28:11 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:           <Author,,Name>
-- Create date: <Create Date,,>
-- Description:      <Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Site_Lic_Get_Accounts_ADM] 
(
       @lic_id int
)
AS
BEGIN
       -- SET NOCOUNT ON added to prevent extra result sets from
       -- interfering with SELECT statements.
       SET NOCOUNT ON;

	   declare @contactList table
		(
			tv_contact_id int, 
			tv_contact_name nvarchar(255),
			tv_email nvarchar(500),
			tv_last_access datetime,
			tv_review_id int
		)

    -- Insert statements for procedure here
    declare @det_id int
    set @det_id = (select top 1 ld.SITE_LIC_DETAILS_ID from TB_SITE_LIC_DETAILS ld 
        inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and ld.VALID_FROM is not null
        and fs.IS_ACTIVE = 0 and ld.SITE_LIC_ID = @lic_id
        order by ld.VALID_FROM desc)
       --first, reviews that were added in the past (not associated with currently active SITE_LIC_DETAILS)
       select * from Reviewer.dbo.TB_REVIEW r
              inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID and SITE_LIC_ID = @lic_id and SITE_LIC_DETAILS_ID != @det_id
       --second, reviews that were added to the currently active SITE_LIC_DETAILS
       select * from Reviewer.dbo.TB_REVIEW r
              inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID and SITE_LIC_ID = @lic_id and SITE_LIC_DETAILS_ID = @det_id
       --accounts in the license
	   insert into @contactList (tv_contact_id, tv_contact_name, tv_email, tv_last_access)
	   select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, max(lt.CREATED) as LAST_LOGIN
		from Reviewer.dbo.TB_CONTACT c
		left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
		on c.CONTACT_ID = lt.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
		where c.CONTACT_ID in (select CONTACT_ID from Reviewer.dbo.TB_SITE_LIC_CONTACT where SITE_LIC_ID = @lic_id)	
		group by c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, c.DATE_CREATED
		UPDATE @contactList 
		SET tv_review_id = TB_LOGON_TICKET.REVIEW_ID 
		FROM TB_LOGON_TICKET, @contactList 
		WHERE CONTACT_ID = tv_contact_id
		and CREATED = tv_last_access
		select * from @contactList order by tv_contact_name
       -- select * from Reviewer.dbo.TB_CONTACT c
       --    inner join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID and lc.SITE_LIC_ID = @lic_id
       --license admins
       select * from Reviewer.dbo.TB_CONTACT c
              inner join TB_SITE_LIC_ADMIN la on c.CONTACT_ID = la.CONTACT_ID and la.SITE_LIC_ID = @lic_id
END

GO


-------------------------------------------------------------------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_GetHelpText]    Script Date: 21/08/2019 15:59:00 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_GetHelpText]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_GetHelpText]
GO

/****** Object:  StoredProcedure [dbo].[st_GetHelpText]    Script Date: 21/08/2019 15:28:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_GetHelpText] 
(
	@HELP_NAME nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select * from TB_MANAGMENT_EMAILS 
	where EMAIL_NAME = @HELP_NAME


	RETURN

END



GO


------------------------------------------------------------------------------------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_DetailedExtensionRecordGet]    Script Date: 21/08/2019 15:59:00 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_DetailedExtensionRecordGet]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_DetailedExtensionRecordGet]
GO

/****** Object:  StoredProcedure [dbo].[st_DetailedExtensionRecordGet]    Script Date: 21/08/2019 15:29:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_DetailedExtensionRecordGet] 
(
	@r int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
		select c.CONTACT_NAME, c.CONTACT_ID, c.EMAIL, ed.*, et.EXTENSION_TYPE 
		 , case 
			when datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) < datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE) 
			then datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) 
			else datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE)
			END
			AS [Months_Ext]
		 , case 
			when datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) < datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE) 
			then datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) * 10
			else datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE) * 10
			END
			AS [£]
		from Reviewer.dbo.TB_REVIEW_CONTACT rc 
		inner join Reviewer.dbo.TB_CONTACT c on rc.REVIEW_ID = @r and rc.CONTACT_ID = c.CONTACT_ID
		inner join TB_EXPIRY_EDIT_LOG ed on ed.ID_EXTENDED = c.CONTACT_ID and EXTENSION_TYPE_ID !=19
		inner join TB_EXTENSION_TYPES et on ed.TYPE_EXTENDED = 1 and et.EXTENSION_TYPE_ID = ed.EXTENSION_TYPE_ID
		order by DATE_OF_EDIT


		select r.REVIEW_NAME, r.REVIEW_ID, ed.*, et.EXTENSION_TYPE
		 , case 
			when datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) < datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE) 
			then datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) 
			else datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE)
			END
			AS [MonthsExt]
		 , case 
			when datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) < datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE) 
			then datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) * 35
			else datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE) * 35
			END
			AS [£]
		from Reviewer.dbo.TB_REVIEW r
		inner join TB_EXPIRY_EDIT_LOG ed on ed.TYPE_EXTENDED = 0 and r.REVIEW_ID = @r and ed.ID_EXTENDED = r.REVIEW_ID and EXTENSION_TYPE_ID !=19
		inner join TB_EXTENSION_TYPES et on et.EXTENSION_TYPE_ID = ed.EXTENSION_TYPE_ID
		order by DATE_OF_EDIT

	RETURN

END




GO


---------------------------------------------------------------------------------------------------------------



USE [ReviewerAdmin]
GO


/****** Object:  StoredProcedure [dbo].[st_ChangeLicenseModel]    Script Date: 21/08/2019 15:59:00 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ChangeLicenseModel]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_ChangeLicenseModel]
GO

/****** Object:  StoredProcedure [dbo].[st_ChangeLicenseModel]    Script Date: 21/08/2019 15:29:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







CREATE procedure [dbo].[st_ChangeLicenseModel]
(
	@SITE_LIC_ID int,
	@SITE_LIC_MODEL int 
)

As

SET NOCOUNT ON

	update Reviewer.dbo.TB_SITE_LIC
	set SITE_LIC_MODEL = @SITE_LIC_MODEL
	where SITE_LIC_ID = @SITE_LIC_ID

SET NOCOUNT OFF







GO

--------------------------------------------------------------------------------------------------------------------


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Details_By_ID]    Script Date: 21/08/2019 15:59:00 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Site_Lic_Get_Details_By_ID]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Site_Lic_Get_Details_By_ID]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Details_By_ID]    Script Date: 21/08/2019 15:30:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Site_Lic_Get_Details_By_ID] 
(
	@admin_ID int,
	@site_license_id int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- first results set: currently active license TB_SITE_LIC_DETAILS.VALID_FROM is something and FOR_SALE.IS_ACTIVE = false
	SELECT TOP 1 *, c.CONTACT_NAME as ADMIN_NAME, ld.DATE_CREATED as DATE_PACKAGE_CREATED, SITE_LIC_MODEL from Reviewer.dbo.TB_SITE_LIC sl 
		inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
		inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
		inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and CONTACT_ID = @admin_ID
		inner join Reviewer.dbo.tb_CONTACT c on c.CONTACT_ID = @admin_ID
		where sl.SITE_LIC_ID = @site_license_id
	Order by ld.VALID_FROM desc	
	-- second results set: currently active renewal offer TB_SITE_LIC_DETAILS.VALID_FROM is NULL and FOR_SALE.IS_ACTIVE = True
	SELECT TOP 1 *, c.CONTACT_NAME as ADMIN_NAME, ld.DATE_CREATED as DATE_PACKAGE_CREATED, SITE_LIC_MODEL from Reviewer.dbo.TB_SITE_LIC sl 
		inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is null
		inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 1
		inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and CONTACT_ID = @admin_ID
		inner join Reviewer.dbo.tb_CONTACT c on c.CONTACT_ID = @admin_ID
		where sl.SITE_LIC_ID = @site_license_id
	Order by ld.VALID_FROM desc
END





GO


------------------------------------------------------------------------------------------------------------------------------



USE [ReviewerAdmin]
GO


/****** Object:  StoredProcedure [dbo].[st_RemoveReviewsFromPreviousPackages]    Script Date: 21/08/2019 15:59:00 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_RemoveReviewsFromPreviousPackages]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_RemoveReviewsFromPreviousPackages]
GO

/****** Object:  StoredProcedure [dbo].[st_RemoveReviewsFromPreviousPackages]    Script Date: 21/08/2019 15:31:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE procedure [dbo].[st_RemoveReviewsFromPreviousPackages]
(
	@SITE_LIC_ID int
)

As

SET NOCOUNT ON

	declare @det_id int
    set @det_id = (select top 1 ld.SITE_LIC_DETAILS_ID from TB_SITE_LIC_DETAILS ld 
        inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and ld.VALID_FROM is not null
        and fs.IS_ACTIVE = 0 and ld.SITE_LIC_ID = @SITE_LIC_ID
        order by ld.VALID_FROM desc)

		declare @reviewList table
		(
			tv_review_id int
		)


       --reviews that were added in the past (not associated with currently active SITE_LIC_DETAILS)
	   insert into @reviewList
       select r.REVIEW_ID from Reviewer.dbo.TB_REVIEW r
              inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID and SITE_LIC_ID = @SITE_LIC_ID and SITE_LIC_DETAILS_ID != @det_id

		delete from Reviewer.dbo.TB_SITE_LIC_REVIEW where REVIEW_ID = (select tv_review_id from @reviewList)


SET NOCOUNT OFF








GO





-- Edits
-------------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Create_or_Edit]    Script Date: 21/08/2019 15:32:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_Site_Lic_Create_or_Edit] 
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
			COMPANY_ADDRESS, TELEPHONE, NOTES, EPPI_NOTES, CREATOR_ID)
		VALUES (@SITE_LIC_NAME, @COMPANY_NAME, @COMPANY_ADDRESS, @TELEPHONE,
			@NOTES, @EPPI_NOTES, @creator_id)	
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
		-- get the existing admin
		declare @L_ADM_ID int
		select @L_ADM_ID = CONTACT_ID from TB_SITE_LIC_ADMIN where SITE_LIC_ID = @LIC_ID
				
		-- we are editing the site license details
		update Reviewer.dbo.TB_SITE_LIC
		set SITE_LIC_NAME = @SITE_LIC_NAME, 
		COMPANY_NAME = @COMPANY_NAME,
		COMPANY_ADDRESS = @COMPANY_ADDRESS,
		TELEPHONE = @TELEPHONE,
		NOTES = @NOTES,
		EPPI_NOTES = @EPPI_NOTES,
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
			update TB_SITE_LIC_ADMIN
			set CONTACT_ID = @admin_id
			where CONTACT_ID = @L_ADM_ID
			and SITE_LIC_ID = @LIC_ID
			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
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

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_All]    Script Date: 21/08/2019 15:33:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_Site_Lic_Get_All] 
(
	@TEXT_BOX nvarchar(255)
)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 

	DECLARE @Site_Licenses TABLE
	(
		SITE_LIC_ID int,
		SITE_LIC_NAME nvarchar(50),
		CONTACT_ID int,
		CONTACT_NAME nvarchar(500),
		[EXPIRY_DATE] date	
	)
	DECLARE @site_lic_id int
	DECLARE @first_contact_id int
	DECLARE @counter int
	set @counter = 0

	INSERT INTO @Site_Licenses (SITE_LIC_ID, SITE_LIC_NAME, [EXPIRY_DATE])	
	select s_l.SITE_LIC_ID, s_l.SITE_LIC_NAME, s_l.EXPIRY_DATE 
	from Reviewer.dbo.TB_SITE_LIC s_l

	DECLARE site_id CURSOR FOR 
	SELECT SITE_LIC_ID from @Site_Licenses 
	OPEN site_id
	FETCH NEXT FROM site_id INTO @site_lic_id

	WHILE @@FETCH_STATUS = 0
	BEGIN	
		DECLARE contact_id CURSOR FOR 
		SELECT CONTACT_ID from TB_SITE_LIC_ADMIN sla
		where sla.SITE_LIC_ID = @site_lic_id
		OPEN contact_id
		FETCH NEXT FROM contact_id INTO @first_contact_id
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if @counter = 0
			begin
				update @Site_Licenses
				set CONTACT_ID = @first_contact_id
				where SITE_LIC_ID = @site_lic_id
				
				update @Site_Licenses
				set CONTACT_NAME = 
				(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT c
				where c.CONTACT_ID = @first_contact_id)
				where SITE_LIC_ID = @site_lic_id

				set @counter = 1
			end
			FETCH NEXT FROM contact_id INTO @first_contact_id
		END
		CLOSE contact_id
		DEALLOCATE contact_id
		set @counter = 0
		FETCH NEXT FROM site_id INTO @site_lic_id
	END

	CLOSE site_id
	DEALLOCATE site_id


select * from @Site_Licenses
where ((SITE_LIC_ID like '%' + @TEXT_BOX + '%') OR
				(SITE_LIC_NAME like '%' + @TEXT_BOX + '%') OR
				(CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
				([EXPIRY_DATE] like '%' + @TEXT_BOX + '%'))

/*

	SELECT s_l.*, s_l_a.*, c.CONTACT_NAME 
	from Reviewer.dbo.TB_SITE_LIC s_l
	inner join TB_SITE_LIC_ADMIN s_l_a on s_l_a.SITE_LIC_ID = s_l.SITE_LIC_ID
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = s_l_a.CONTACT_ID
	order by s_l.SITE_LIC_ID
*/
       
END





GO


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Details_Create_or_Edit_AndOr_Activate]    Script Date: 21/08/2019 15:34:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_Site_Lic_Details_Create_or_Edit_AndOr_Activate] 
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
				end					
				set @RESULT = 'valid'
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


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Remove_Admin]    Script Date: 21/08/2019 15:35:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_Site_Lic_Add_Remove_Admin]
	-- Add the parameters for the stored procedure here
	@lic_id int
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
	select @contact_id = CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = @contact_email
	if @@ROWCOUNT = 1 
	begin
		declare @ck int
		set @ck = (SELECT count(contact_id) from TB_SITE_LIC_ADMIN 
		where (CONTACT_ID = @admin_ID or @admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1))
				and SITE_LIC_ID = @lic_id)
		if @ck < 1 set @res = -1 --first check: see if supplied admin is actually and admin!
		else if  (select count(contact_id) from Reviewer.dbo.TB_CONTACT where @contact_email = EMAIL and @contact_id = CONTACT_ID) != 1
			--second check, see if c_id and email exist
			set @res = -2
		else -- checks went OK, let's see if we can do it
		begin
			set @ck = (select count(contact_id) from TB_SITE_LIC_ADMIN where CONTACT_ID = @contact_id and SITE_LIC_ID = @lic_id)
			if @ck = 1 -- contact is an admin, we should remove it
			begin
				delete from TB_SITE_LIC_ADMIN where CONTACT_ID = @contact_id and @lic_id = SITE_LIC_ID
				if @@ROWCOUNT = 1
				begin --write success log
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin'
						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc
				end
				else --write failure log, if this is fired, there is a bug somewhere
				begin
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin: failed!'
						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc	
					set @res = -3
				end
			end	
			
			else --contact is not an admin, we should add it
			begin
				insert into TB_SITE_LIC_ADMIN (CONTACT_ID, SITE_LIC_ID)
				values (@contact_id, @lic_id)
				if @@ROWCOUNT = 1
				begin
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin'
						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc
				end
				else
				begin
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin: failed!'
						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc	
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


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get]    Script Date: 21/08/2019 15:35:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_Site_Lic_Get] 
	-- Add the parameters for the stored procedure here
	
	@admin_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @License TABLE
(
    SITE_LIC_ID int,
    SITE_LIC_NAME nvarchar(50),
	COMPANY_NAME nvarchar(50),
	COMPANY_ADDRESS nvarchar(500),
	TELEPHONE nvarchar(50),
	NOTES nvarchar(4000),
	T_CREATOR_ID int,
	CREATOR_NAME nvarchar(255),
	ADMINISTRATOR_ID int,
	ADMINISTRATOR_NAME nvarchar(255),
	DATE_CREATED date	
)

	INSERT INTO @License (SITE_LIC_ID, SITE_LIC_NAME, COMPANY_NAME,
	COMPANY_ADDRESS, TELEPHONE, NOTES, T_CREATOR_ID, DATE_CREATED, ADMINISTRATOR_ID)
	select sl.SITE_LIC_ID, sl.SITE_LIC_NAME, sl.COMPANY_NAME, sl.COMPANY_ADDRESS,
	sl.TELEPHONE, sl.NOTES, sl.CREATOR_ID, sl.DATE_CREATED, sla.CONTACT_ID
	from Reviewer.dbo.TB_SITE_LIC sl
	inner join TB_SITE_LIC_ADMIN sla on sla.SITE_LIC_ID = sl.SITE_LIC_ID
	where sla.CONTACT_ID = @admin_ID

	update @License
	set CREATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = T_CREATOR_ID)
	
	update @License
	set ADMINISTRATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = ADMINISTRATOR_ID)

	select * from @License
	

END



GO

--------------------------------------------------------------

update [ReviewerAdmin].[dbo].[TB_EXTENSION_TYPES]
set EXTENSION_TYPE = 'Unreorded' where EXTENSION_TYPE_ID = 1

---------------------------------------------------------

