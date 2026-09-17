USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ContactAddToReview]    Script Date: 17/09/2026 14:27:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER       procedure [dbo].[st_ContactAddToReview]
(
	@EMAIL nvarchar(500),
	@REVIEW_ID int
	--,@RESULT int output


	
	-- RESULT
        -- 0 - everything OK and account updated
        -- 1 - email not found
		-- 2 - there is more than 1 account with this email address

	
)

As

SET NOCOUNT ON

declare @RESULT int = 0
declare @BREAK int = 0
declare @CONTACT_ID int
declare @NEW_REVIEW_CONTACT_ID int
declare	@CONTACT_NAME nvarchar(255)
declare	@REVIEW_NAME nvarchar(1000)

	-- check if email is already in use
	declare @count int
	set @count = (select count(*) from sTB_CONTACT
	where EMAIL = @EMAIL)
	if @count = 0 
	begin
		set @RESULT = 1
		set @BREAK = 1
		set @CONTACT_NAME = 'Not found'
	end
	else if @count > 1
	begin 
		set @RESULT = 1
		set @BREAK = 1
		set @CONTACT_NAME = 'Multiple versions'
	end
	else -- @count = 1 so everything OK
	begin
		set @CONTACT_ID = (select CONTACT_ID from sTB_CONTACT where EMAIL = @EMAIL)

		insert into sTB_REVIEW_CONTACT(CONTACT_ID, REVIEW_ID)
		values (@CONTACT_ID, @REVIEW_ID)	
		set @NEW_REVIEW_CONTACT_ID = @@IDENTITY
		
		set @CONTACT_NAME = (select CONTACT_NAME from sTB_CONTACT where EMAIL = @EMAIL)

		insert into sTB_CONTACT_REVIEW_ROLE(REVIEW_CONTACT_ID, ROLE_NAME)
		values(@NEW_REVIEW_CONTACT_ID, 'RegularUser')

		set @REVIEW_NAME = (select REVIEW_NAME from sTB_REVIEW where REVIEW_ID = @REVIEW_ID)
	end

	declare @tb_values table (RESULT int, CONTACT_NAME nvarchar(255), REVIEW_NAME nvarchar(1000))
	insert into @tb_values (RESULT, CONTACT_NAME, REVIEW_NAME)
	values (@RESULT, @CONTACT_NAME, @REVIEW_NAME)
	select * from @tb_values



/*
RETURN @RESULT 
RETURN @CONTACT_NAME
RETURN @REVIEW_NAME*/
SET NOCOUNT OFF
GO

-------------------------------------------


USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewContactList]    Script Date: 17/09/2026 14:28:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER   procedure [dbo].[st_ReviewContactList]
(
	@REVIEW_ID INT
)

As

/* ADD TO THIS THE RETRIEVAL OF ROLES */

/* original routine
SELECT TB_REVIEW_CONTACT.CONTACT_ID, REVIEW_ID, CONTACT_NAME

FROM TB_REVIEW_CONTACT

INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_REVIEW_CONTACT.CONTACT_ID

WHERE REVIEW_ID = @REVIEW_ID
*/



declare @reviewMembers table (CONTACT_ID int, REVIEW_ID int, CONTACT_NAME nvarchar(255), EMAIL nvarchar(500), 
[EXPIRY_DATE] nvarchar(50), ROLE_NAME nvarchar(50), ROLES nvarchar(75), IS_EXPIRED int)

declare @tv_members table (tv_review_contact_id int,  tv_contact_id int, tv_contact_name nvarchar(255), tv_email nvarchar(500), 
tv_expiry_date date, tv_license_number int, tv_license_expiry datetime,  tv_role nvarchar(50), tv_roles nvarchar(75),
tv_last_expiry nvarchar(50), tv_is_expired int)

insert into @tv_members (tv_contact_id, tv_contact_name, tv_email, tv_expiry_date, tv_license_number, tv_is_expired)
SELECT rc.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, c.[EXPIRY_DATE], 0, 0  
FROM TB_REVIEW_CONTACT rc
INNER JOIN TB_CONTACT c ON c.CONTACT_ID = rc.CONTACT_ID
WHERE rc.REVIEW_ID = @REVIEW_ID

update t1
set tv_review_contact_id = t2.REVIEW_CONTACT_ID
from @tv_members t1 inner join TB_REVIEW_CONTACT t2
on t2.CONTACT_ID = t1.tv_contact_id
and t2.REVIEW_ID = @REVIEW_ID

update t1
set tv_license_number = t2.SITE_LIC_ID
from @tv_members t1 inner join TB_SITE_LIC_CONTACT t2
on t2.CONTACT_ID = t1.tv_contact_id

update t1
set tv_license_expiry = t2.EXPIRY_DATE
from @tv_members t1 inner join TB_SITE_LIC t2
on t2.SITE_LIC_ID = t1.tv_license_number
and t1.tv_license_number > 0





-- get roles but be aware some older users have more than one role
-- If a user has multiple roles there is a hierarchal order that is also alphabetical
-- AdminUser, Coding only, ReadOnlyUser, RegularUser

declare @role nvarchar(50)
declare @tb_roles table (tv_role_names nvarchar(50))
declare @roles nvarchar(75)
declare @lastExpiry nvarchar(50)
declare @accountExpiry date
declare @licenseExpiry date
declare @licenseNumber int

declare @WORKING_REVIEW_CONTACT_ID int
	declare REVIEW_CONTACT_ID_CURSOR cursor for
	select tv_review_contact_id FROM @tv_members
	open REVIEW_CONTACT_ID_CURSOR
	fetch next from REVIEW_CONTACT_ID_CURSOR
	into @WORKING_REVIEW_CONTACT_ID
	while @@FETCH_STATUS = 0
	begin
		set @role = (select top 1 ROLE_NAME 
		from TB_CONTACT_REVIEW_ROLE
		where REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		order by ROLE_NAME asc)

		delete from @tb_roles
		insert into @tb_roles (tv_role_names)
		select ROLE_NAME from TB_CONTACT_REVIEW_ROLE
		where REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		order by ROLE_NAME asc

		set @roles = (SELECT STRING_AGG (tv_role_names, ',') FROM @tb_roles)

		update @tv_members
		set tv_role = @role
		where tv_review_contact_id = @WORKING_REVIEW_CONTACT_ID

		update @tv_members
		set tv_roles = @roles
		where tv_review_contact_id = @WORKING_REVIEW_CONTACT_ID

		set @licenseNumber = (select tv_license_number from @tv_members where tv_review_contact_id = @WORKING_REVIEW_CONTACT_ID)
		if @licenseNumber = 0
		begin
			-- get the expiry date
			set @accountExpiry = (select tv_expiry_date from @tv_members where tv_review_contact_id = @WORKING_REVIEW_CONTACT_ID)
			
			if @accountExpiry < GETDATE()
			begin
				update @tv_members
				set tv_is_expired = 1 where tv_review_contact_id = @WORKING_REVIEW_CONTACT_ID
			end

			update @tv_members
			set tv_last_expiry = CONVERT(VARCHAR(10),@accountExpiry,103)
			where tv_review_contact_id = @WORKING_REVIEW_CONTACT_ID
		end
		else
		begin
			-- in a site license so pick the last expiry but mention the site license
			set @accountExpiry = (select tv_expiry_date from @tv_members where tv_review_contact_id = @WORKING_REVIEW_CONTACT_ID)
			set @licenseExpiry = (select tv_license_expiry from @tv_members where tv_review_contact_id = @WORKING_REVIEW_CONTACT_ID)
			if @accountExpiry > @licenseExpiry
			begin
				-- last expiry is the account expiry
				if @accountExpiry < GETDATE()
				begin
					update @tv_members
					set tv_is_expired = 1 where tv_review_contact_id = @WORKING_REVIEW_CONTACT_ID
				end

				update @tv_members
				set tv_last_expiry = CONVERT(VARCHAR(10),@accountExpiry,103)  + ' (site lic: ' + CAST(@licenseNumber AS varchar) + ')' 
				where tv_review_contact_id = @WORKING_REVIEW_CONTACT_ID
			end
			else
			begin
				if @licenseExpiry < GETDATE()
				begin
					update @tv_members
					set tv_is_expired = 1 where tv_review_contact_id = @WORKING_REVIEW_CONTACT_ID
				end
				-- last expiry is the license expiry
				update @tv_members
				set tv_last_expiry = CONVERT(VARCHAR(10),@licenseExpiry,103)  + ' (site lic: ' + CAST(@licenseNumber AS varchar) + ')' 
				where tv_review_contact_id = @WORKING_REVIEW_CONTACT_ID
			end
		end
				
		FETCH NEXT FROM REVIEW_CONTACT_ID_CURSOR 
		INTO @WORKING_REVIEW_CONTACT_ID
	END

	CLOSE REVIEW_CONTACT_ID_CURSOR
	DEALLOCATE REVIEW_CONTACT_ID_CURSOR




--select * from @tv_members

insert into @reviewMembers (CONTACT_ID, REVIEW_ID, CONTACT_NAME, EMAIL, [EXPIRY_DATE], ROLE_NAME, ROLES, IS_EXPIRED)
select tv_contact_id, @REVIEW_ID, tv_contact_name, tv_email, tv_last_expiry, tv_role, tv_roles, tv_is_expired from @tv_members

update @reviewMembers
set ROLE_NAME = 'Review admin' where ROLE_NAME = 'AdminUser'
update @reviewMembers
set ROLE_NAME = 'Reviewer' where ROLE_NAME = 'RegularUser'
update @reviewMembers
set ROLE_NAME = 'Read only' where ROLE_NAME = 'ReadOnlyUser'

update @reviewMembers
set ROLES = replace(ROLES, 'AdminUser', 'Review admin')
update @reviewMembers
set ROLES = replace(ROLES, 'RegularUser', 'Reviewer')
update @reviewMembers
set ROLES = replace(ROLES, 'ReadOnlyUser', 'Read only')

select * from @reviewMembers
GO

-----------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewRoleAddRemoveByContactID]    Script Date: 17/09/2026 14:29:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER        procedure [dbo].[st_ReviewRoleAddRemoveByContactID]
(
	@REVIEW_ID int,
	@CONTACT_ID int,
	@ROLE_NAME nvarchar(50)
)

As

SET NOCOUNT ON

	declare @REVIEW_CONTACT_ID int
	-- get REVIEW_CONTACT_ID
	set @REVIEW_CONTACT_ID = (select REVIEW_CONTACT_ID 
    from sTB_REVIEW_CONTACT
    where REVIEW_ID = @REVIEW_ID
    and CONTACT_ID = @CONTACT_ID)

	if ((@REVIEW_CONTACT_ID = '') or (@REVIEW_CONTACT_ID is null)) 
	begin
		-- something is going wrong so let's not do anything
		declare @RESULT int = 1
	end
	else
	begin
		declare @numberOccurrences int
		set @numberOccurrences = (select count(*) from sTB_CONTACT_REVIEW_ROLE 
			where REVIEW_CONTACT_ID = @REVIEW_CONTACT_ID and ROLE_NAME = @ROLE_NAME)
		
		if @numberOccurrences = 0
		begin
			-- we are adding the role
			insert into sTB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
			values (@REVIEW_CONTACT_ID, @ROLE_NAME)
		end
		else
		begin
			-- we are removing the role
			delete from sTB_CONTACT_REVIEW_ROLE 
			where REVIEW_CONTACT_ID = @REVIEW_CONTACT_ID and ROLE_NAME = @ROLE_NAME
		end		
	end

SET NOCOUNT OFF

GO

---------------------------------
