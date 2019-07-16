USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewMembers_2]    Script Date: 6/26/2019 11:14:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ReviewMembers_2]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_ReviewMembers_2]
GO

-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
create PROCEDURE [dbo].[st_ReviewMembers_2] 
(
	@REVIEW_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
    declare @tb_review_members table 
		(contact_id int, review_contact_id int, contact_name nvarchar(255), email nvarchar(500),
		last_login datetime, review_role nvarchar(50), review_role_1 nvarchar(50), review_role_2 nvarchar(50),
		is_coding_only bit, is_read_only bit, is_admin bit)
		
	declare @review_contact_id int
    
    insert into @tb_review_members (contact_id, review_contact_id, contact_name, email)
	select c.CONTACT_ID, rc.REVIEW_CONTACT_ID, c.CONTACT_NAME, c.EMAIL
	from Reviewer.dbo.TB_CONTACT c
	right join Reviewer.dbo.TB_REVIEW_CONTACT rc
	on c.CONTACT_ID = rc.CONTACT_ID
	where rc.REVIEW_ID = @REVIEW_ID
	order by c.CONTACT_NAME
    
    --select * from @tb_review_members
    
    update @tb_review_members
	set last_login = 
	(select max(lt.CREATED) as LAST_LOGIN from TB_LOGON_TICKET lt
	where contact_id = lt.CONTACT_ID
	and lt.REVIEW_ID = @REVIEW_ID)
    
    update @tb_review_members
	set is_coding_only = 0
    
    --select * from @tb_review_members
    
	declare @WORKING_REVIEW_CONTACT_ID int
	declare REVIEW_CONTACT_ID_CURSOR cursor for
	select review_contact_id FROM @tb_review_members
	open REVIEW_CONTACT_ID_CURSOR
	fetch next from REVIEW_CONTACT_ID_CURSOR
	into @WORKING_REVIEW_CONTACT_ID
	while @@FETCH_STATUS = 0
	begin 		
		-- see if the person is coding only for this review
		update @tb_review_members 
		set review_role_2 = 
		(select CRR.ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'Coding only'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID
		
		-- see if the person is read only for this review
		update @tb_review_members 
		set review_role_1 = 
		(select CRR.ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'ReadOnlyUser'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID
		
		update @tb_review_members 
		set review_role = 
		(select CRR.ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'AdminUser'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID

		FETCH NEXT FROM REVIEW_CONTACT_ID_CURSOR 
		INTO @WORKING_REVIEW_CONTACT_ID
	END

	CLOSE REVIEW_CONTACT_ID_CURSOR
	DEALLOCATE REVIEW_CONTACT_ID_CURSOR
    
    --select * from @tb_review_members
    update @tb_review_members
	set is_coding_only = 1
	where review_role_2 = 'Coding only'
    
    update @tb_review_members
	set is_read_only = 1
	where review_role_1 = 'ReadOnlyUser'
	
	update @tb_review_members
	set is_admin = 1
	where review_role = 'AdminUser'
	
	select * from @tb_review_members
	




END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactDetailsFullCreateOrEdit]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_ContactDetailsFullCreateOrEdit]
GO


CREATE procedure [dbo].[st_ContactDetailsFullCreateOrEdit]
(
	@NEW_ACCOUNT bit,
	@CONTACT_NAME nvarchar(255),
	@USERNAME nvarchar(50),
	@PASSWORD varchar(50),
	@DATE_CREATED datetime,
	@EXPIRY_DATE date,
	@CREATOR_ID int,
	@EMAIL nvarchar(500),
	@DESCRIPTION nvarchar(1000),
	@IS_SITE_ADMIN bit,
	@CONTACT_ID nvarchar(50), -- it might say 'New'
	@EXTENSION_TYPE_ID int,
	@EXTENSION_NOTES nvarchar(500),
	@EDITOR_ID int,
	@MONTHS_CREDIT nvarchar(50),
	@ARCHIE_ID varchar(32),
	@RESULT nvarchar(50) output
)


As

SET NOCOUNT ON
DECLARE @ORIGINAL_EXPIRY datetime
DECLARE @NEW_ROW_ID int

if @EXPIRY_DATE = ''
begin
	SET @EXPIRY_DATE = NULL
end

BEGIN TRY

BEGIN TRANSACTION
	if @NEW_ACCOUNT = 1
	begin
				
		-- create a new account
		INSERT INTO Reviewer.dbo.TB_CONTACT(CONTACT_NAME, USERNAME, /*[PASSWORD],*/ 
			DATE_CREATED, [EXPIRY_DATE], EMAIL, [DESCRIPTION], CREATOR_ID, MONTHS_CREDIT)
		VALUES (@CONTACT_NAME, @USERNAME, /*@PASSWORD,*/ @DATE_CREATED, @EXPIRY_DATE,
			@EMAIL, @DESCRIPTION, @CREATOR_ID, @MONTHS_CREDIT)
	
		set @RESULT = @@IDENTITY
		if (@CREATOR_ID != @CONTACT_ID)
		begin
			update Reviewer.dbo.TB_CONTACT 
			set CREATOR_ID = @RESULT
			where CONTACT_ID = @RESULT
			and USERNAME = @USERNAME
		end
		
		DECLARE @chars char(100) = '!т#$%&а()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~ийклм'
		declare @rnd varchar(20)
		declare @cnt int = 0
		set @rnd = ''
		WHILE (@cnt <= 20) 
		BEGIN
			SELECT @rnd = @rnd + 
				SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
			SELECT @cnt = @cnt + 1
		END
		update Reviewer.dbo.TB_CONTACT 
		set FLAVOUR = @rnd, PWASHED = HASHBYTES('SHA1', @PASSWORD + @rnd)
		where CONTACT_ID = @RESULT
		
		
		
    end
	else  -- edit an existing account
	begin	
		-- get original expiry date from TB_CONTACT
		select @ORIGINAL_EXPIRY = c.EXPIRY_DATE 
		from Reviewer.dbo.TB_CONTACT c
		where c.CONTACT_ID = @CONTACT_ID
		
		--update TB_CONTACT
		update Reviewer.dbo.TB_CONTACT 
		set 
		CONTACT_NAME = @CONTACT_NAME,
		USERNAME = @USERNAME,
		/*[PASSWORD] = @PASSWORD,*/
		DATE_CREATED = @DATE_CREATED,
		[EXPIRY_DATE] = @EXPIRY_DATE,
		CREATOR_ID = @CREATOR_ID,
		EMAIL = @EMAIL,
		[DESCRIPTION] = @DESCRIPTION,
		IS_SITE_ADMIN = @IS_SITE_ADMIN,
		MONTHS_CREDIT = @MONTHS_CREDIT,
		ARCHIE_ID = @ARCHIE_ID
		where CONTACT_ID = @CONTACT_ID
		
		-- to get the null value correct
		if @ARCHIE_ID = ''
		begin
			update Reviewer.dbo.TB_CONTACT 
			set ARCHIE_ID = null
			where CONTACT_ID = @CONTACT_ID
		end
		
		
		
		if (@PASSWORD != '')
		begin
			DECLARE @chars1 char(100) = '!т#$%&а()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~ийклм'
			declare @rnd1 varchar(20)
			declare @cnt1 int = 0
			set @rnd1 = ''
			WHILE (@cnt1 <= 20) 
			BEGIN
				SELECT @rnd1 = @rnd1 + 
					SUBSTRING(@chars1, CONVERT(int, RAND() * 100), 1)
				SELECT @cnt1 = @cnt1 + 1
			END
			update Reviewer.dbo.TB_CONTACT 
			set FLAVOUR = @rnd1, PWASHED = HASHBYTES('SHA1', @PASSWORD + @rnd1)
			where CONTACT_ID = @CONTACT_ID
		end
		
		
		if (@EXTENSION_TYPE_ID > 1)
		begin
			-- create new row in TB_EXPIRY_EDIT_LOG -- using bogus old expiry date
			insert into TB_EXPIRY_EDIT_LOG
			(DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
			OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
			values (GETDATE(), 1, @CONTACT_ID, @EXPIRY_DATE, @EXPIRY_DATE,
			@EDITOR_ID, @EXTENSION_TYPE_ID, @EXTENSION_NOTES)
			set @NEW_ROW_ID = @@IDENTITY
			
			--  update TB_EXPIRY_EDIT_LOG
			update TB_EXPIRY_EDIT_LOG
			set OLD_EXPIRY_DATE = @ORIGINAL_EXPIRY
			where EXPIRY_EDIT_ID = @NEW_ROW_ID
	
			set @RESULT = 'Valid'		 
		end
	end
	
	
	
COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Invalid'
end
END CATCH

RETURN

SET NOCOUNT OFF



GO