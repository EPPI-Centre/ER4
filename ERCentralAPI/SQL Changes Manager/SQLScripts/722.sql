USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_ContactLogin]    Script Date: 03/10/2025 10:33:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/10>
-- Description:	<gets contact details when loging in>
-- =============================================
CREATE or ALTER   PROCEDURE [dbo].[st_ContactLogin] 
(
	@USERNAME varchar(50),
	@PASSWORD varchar(50),
	@IP_ADDRESS nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
--first check if the username/pw are correct

	SELECT c.*, COUNT(sla.CONTACT_ID) as IsSLA, COUNT(o_a.CONTACT_ID) as IsOA
	FROM sTB_CONTACT c
	Left outer join TB_SITE_LIC_ADMIN sla on sla.CONTACT_ID = c.CONTACT_ID
	Left outer join TB_ORGANISATION_ADMIN o_a on o_a.CONTACT_ID = c.CONTACT_ID
	where c.USERNAME = @USERNAME
	and HASHBYTES('SHA1', @Password + FLAVOUR) = PWASHED
	and EXPIRY_DATE is not null
	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME,
	c.PASSWORD, c.LAST_LOGIN, c.DATE_CREATED, c.EMAIL, c.EXPIRY_DATE,
	c.MONTHS_CREDIT, c.CREATOR_ID,c.TYPE, c.IS_SITE_ADMIN, c.DESCRIPTION, c.SEND_NEWSLETTER, c.FLAVOUR, c.PWASHED,
	c.ARCHIE_ACCESS_TOKEN, c.ARCHIE_ID, c.ARCHIE_REFRESH_TOKEN, c.ARCHIE_TOKEN_VALID_UNTIL, c.LAST_ARCHIE_CODE, c.LAST_ARCHIE_STATE,
	c.IS_METHODOLOGIST
	
	RETURN
END

GO

-----------------------------------------------------------

USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_ContactDetails]    Script Date: 03/10/2025 11:16:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE or ALTER   PROCEDURE [dbo].[st_ContactDetails] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
    max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
				CASE when l.[EXPIRY_DATE] is not null 
				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then l.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end as 'EXPIRY_DATE', 
	c.MONTHS_CREDIT, c.CREATOR_ID,
    c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], c.SEND_NEWSLETTER, c.ARCHIE_ID, c.IS_METHODOLOGIST,
    l.SITE_LIC_ID, l.SITE_LIC_NAME
    ,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
	from sTB_CONTACT c
	left join TB_LOGON_TICKET lt
	on c.CONTACT_ID = lt.CONTACT_ID
	left join sTB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
	left join sTB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
	where c.CONTACT_ID = @CONTACT_ID
	
	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
	c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID, c.SEND_NEWSLETTER,
    c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], c.ARCHIE_ID, c.IS_METHODOLOGIST, l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME

	RETURN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

END

GO

---------------------------------------------------

USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_ContactDetailsFullCreateOrEdit]    Script Date: 03/10/2025 11:11:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   procedure [dbo].[st_ContactDetailsFullCreateOrEdit]
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
	@IS_METHODOLOGIST bit,
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
		INSERT INTO sTB_CONTACT(CONTACT_NAME, USERNAME, /*[PASSWORD],*/ 
			DATE_CREATED, [EXPIRY_DATE], EMAIL, [DESCRIPTION], CREATOR_ID, MONTHS_CREDIT, IS_METHODOLOGIST)
		VALUES (@CONTACT_NAME, @USERNAME, /*@PASSWORD,*/ @DATE_CREATED, @EXPIRY_DATE,
			@EMAIL, @DESCRIPTION, @CREATOR_ID, @MONTHS_CREDIT, @IS_METHODOLOGIST)
	
		set @RESULT = @@IDENTITY
		if (@CREATOR_ID != @CONTACT_ID)
		begin
			update sTB_CONTACT 
			set CREATOR_ID = @RESULT
			where CONTACT_ID = @RESULT
			and USERNAME = @USERNAME
		end
		
		DECLARE @chars char(100) = '!ò#$%&à()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~èéêëì'
		declare @rnd varchar(20)
		declare @cnt int = 0
		set @rnd = ''
		WHILE (@cnt <= 20) 
		BEGIN
			SELECT @rnd = @rnd + 
				SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
			SELECT @cnt = @cnt + 1
		END
		update sTB_CONTACT 
		set FLAVOUR = @rnd, PWASHED = HASHBYTES('SHA1', @PASSWORD + @rnd)
		where CONTACT_ID = @RESULT
		
    end
	else  -- edit an existing account
	begin	
		-- get original expiry date from TB_CONTACT
		select @ORIGINAL_EXPIRY = c.EXPIRY_DATE 
		from sTB_CONTACT c
		where c.CONTACT_ID = @CONTACT_ID
		
		--update TB_CONTACT
		update sTB_CONTACT 
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
		ARCHIE_ID = @ARCHIE_ID,
		IS_METHODOLOGIST = @IS_METHODOLOGIST
		where CONTACT_ID = @CONTACT_ID
		
		-- to get the null value correct
		if @ARCHIE_ID = ''
		begin
			update sTB_CONTACT 
			set ARCHIE_ID = null
			where CONTACT_ID = @CONTACT_ID
		end
		
		if (@PASSWORD != '')
		begin
			DECLARE @chars1 char(100) = '!ò#$%&à()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~èéêëì'
			declare @rnd1 varchar(20)
			declare @cnt1 int = 0
			set @rnd1 = ''
			WHILE (@cnt1 <= 20) 
			BEGIN
				SELECT @rnd1 = @rnd1 + 
					SUBSTRING(@chars1, CONVERT(int, RAND() * 100), 1)
				SELECT @cnt1 = @cnt1 + 1
			END
			update sTB_CONTACT 
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
--------------------------


USE [ReviewerAdmin]
GO

select * from TB_EXTENSION_TYPES where EXTENSION_TYPE = 'Add/Remove methodologist role'
if @@rowcount = 0
begin
	-- add a new row to TB_EXTENSION_TYPES
	insert into TB_EXTENSION_TYPES (EXTENSION_TYPE, [DESCRIPTION], APPLIES_TO, [ORDER])
	values ('Add/Remove methodologist role', 
			'Adding or removing the methodologist role for a user',
			'111',
			'15'
			)
end


