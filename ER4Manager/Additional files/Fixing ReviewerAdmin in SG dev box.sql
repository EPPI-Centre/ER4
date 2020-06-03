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


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsCochrane]    Script Date: 8/28/2019 11:13:45 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ReviewDetailsCochrane]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_ReviewDetailsCochrane]
GO

-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ReviewDetailsCochrane] 
(
	@REVIEW_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here	
	select r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, r.REVIEW_NUMBER, 
	r.DATE_CREATED, r.OLD_REVIEW_GROUP_ID,
		CASE when l.[EXPIRY_DATE] is not null 
		and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
			then l.[EXPIRY_DATE]
		else r.[EXPIRY_DATE]
		end as 'EXPIRY_DATE', 
	r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
	l.SITE_LIC_ID, l.SITE_LIC_NAME, r.SHOW_SCREENING, r.ALLOW_REVIEWER_TERMS, r.ALLOW_CLUSTERED_SEARCH,
	r.ARCHIE_ID, r.ARCHIE_CD, r.IS_CHECKEDOUT_HERE, r.CHECKED_OUT_BY
	from Reviewer.dbo.TB_REVIEW r
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
	left join Reviewer.dbo.TB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
	left join Reviewer.dbo.TB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
	where r.REVIEW_ID = @REVIEW_ID
	
	group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
	r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
	l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME, r.REVIEW_NUMBER, r.OLD_REVIEW_GROUP_ID,
	r.SHOW_SCREENING, r.ALLOW_REVIEWER_TERMS, r.ALLOW_CLUSTERED_SEARCH,
	r.ARCHIE_ID, r.ARCHIE_CD, r.IS_CHECKEDOUT_HERE, r.CHECKED_OUT_BY
	order by r.REVIEW_NAME
	

	RETURN

END


GO

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsFilterCochrane]    Script Date: 8/28/2019 11:13:57 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ReviewDetailsFilterCochrane]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_ReviewDetailsFilterCochrane]
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ReviewDetailsFilterCochrane] 
(
	@SHAREABLE bit,
	@TEXT_BOX nvarchar(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    if @SHAREABLE = 1
	begin        
		/*
		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
		r.FUNDER_ID, c.CONTACT_NAME, r.MONTHS_CREDIT
		FROM Reviewer.dbo.tb_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
		where 
		(r.EXPIRY_DATE is not null) OR
		(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0)*/
		
		select r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME,  
		r.DATE_CREATED, 
			CASE when l.[EXPIRY_DATE] is not null 
			and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
				then l.[EXPIRY_DATE]
			else r.[EXPIRY_DATE]
			end as 'EXPIRY_DATE', 
		r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		from Reviewer.dbo.TB_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
		left join Reviewer.dbo.TB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
		where 
			((r.EXPIRY_DATE is not null) OR
			(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0))
		and ((r.REVIEW_ID like '%' + @TEXT_BOX + '%') OR
			(r.REVIEW_NAME like '%' + @TEXT_BOX + '%') OR
			(c.CONTACT_NAME like '%' + @TEXT_BOX + '%'))
		and r.ARCHIE_ID is not null
		
		
		group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
		r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
		l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		order by r.REVIEW_NAME
		 
	end
	else
	begin
		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
		r.FUNDER_ID, c.CONTACT_NAME
		FROM Reviewer.dbo.tb_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID 
		where (r.EXPIRY_DATE is null and r.MONTHS_CREDIT = 0)
		
		and ((r.REVIEW_ID like '%' + @TEXT_BOX + '%') OR
			(r.REVIEW_NAME like '%' + @TEXT_BOX + '%') OR
			(c.CONTACT_NAME like '%' + @TEXT_BOX + '%'))
		and r.ARCHIE_ID is not null
		
	end

	RETURN

END


GO

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsGetAllCochrane]    Script Date: 8/28/2019 11:14:10 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ReviewDetailsGetAllCochrane]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_ReviewDetailsGetAllCochrane]
GO

-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ReviewDetailsGetAllCochrane] 
(
	@SHAREABLE bit
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    if @SHAREABLE = 1
	begin        
		/*
		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
		r.FUNDER_ID, c.CONTACT_NAME, r.MONTHS_CREDIT
		FROM Reviewer.dbo.tb_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
		where 
		(r.EXPIRY_DATE is not null) OR
		(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0)*/
		
		select r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME,  
		r.DATE_CREATED, 
			CASE when l.[EXPIRY_DATE] is not null 
			and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
				then l.[EXPIRY_DATE]
			else r.[EXPIRY_DATE]
			end as 'EXPIRY_DATE', 
		r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		from Reviewer.dbo.TB_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
		left join Reviewer.dbo.TB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
		where 
			(r.EXPIRY_DATE is not null) OR
			(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0)
		and r.ARCHIE_ID is not null
		
		group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
		r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
		l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		order by r.REVIEW_NAME
		 
	end
	else
	begin
		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
		r.FUNDER_ID, c.CONTACT_NAME
		FROM Reviewer.dbo.tb_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID 
		where (r.EXPIRY_DATE is null and r.MONTHS_CREDIT = 0)
		and r.ARCHIE_ID is not null
	end

	RETURN

END



GO

USE [Reviewer]
GO
IF COL_LENGTH('dbo.TB_SITE_LIC', 'BL_ACCOUNT_CODE') IS NULL
BEGIN
ALTER TABLE dbo.TB_SITE_LIC ADD BL_ACCOUNT_CODE nvarchar(50) NULL 
END
IF COL_LENGTH('dbo.TB_SITE_LIC', 'BL_AUTH_CODE') IS NULL
BEGIN
ALTER TABLE dbo.TB_SITE_LIC ADD BL_AUTH_CODE nvarchar(50) NULL 
END
IF COL_LENGTH('dbo.TB_SITE_LIC', 'BL_TX') IS NULL
BEGIN
ALTER TABLE dbo.TB_SITE_LIC ADD BL_TX nvarchar(50) NULL 
END
IF COL_LENGTH('dbo.TB_SITE_LIC', 'BL_CC_ACCOUNT_CODE') IS NULL
BEGIN
ALTER TABLE dbo.TB_SITE_LIC ADD BL_CC_ACCOUNT_CODE nvarchar(50) NULL 
END
IF COL_LENGTH('dbo.TB_SITE_LIC', 'BL_CC_AUTH_CODE') IS NULL
BEGIN
ALTER TABLE dbo.TB_SITE_LIC ADD BL_CC_AUTH_CODE nvarchar(50) NULL 
END
IF COL_LENGTH('dbo.TB_SITE_LIC', 'BL_CC_TX') IS NULL
BEGIN
ALTER TABLE dbo.TB_SITE_LIC ADD BL_CC_TX nvarchar(50) NULL 
END
IF COL_LENGTH('dbo.TB_SITE_LIC', 'ALLOW_REVIEW_OWNERSHIP_CHANGE') IS NULL
BEGIN
ALTER TABLE dbo.TB_SITE_LIC ADD ALLOW_REVIEW_OWNERSHIP_CHANGE bit NULL 
END
GO




USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BritishLibraryCCValuesSet]    Script Date: 8/28/2019 11:19:53 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BritishLibraryCCValuesSet]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_BritishLibraryCCValuesSet]
GO


CREATE procedure [dbo].[st_BritishLibraryCCValuesSet]
(
	@REVIEW_ID int,
	@BL_CC_ACCOUNT_CODE nvarchar(50), 
	@BL_CC_AUTH_CODE nvarchar(50), 
	@BL_CC_TX nvarchar(50)
)

As

SET NOCOUNT ON

	update Reviewer.dbo.TB_REVIEW
	set BL_CC_ACCOUNT_CODE = @BL_CC_ACCOUNT_CODE,
	BL_CC_AUTH_CODE = @BL_CC_AUTH_CODE,
	BL_CC_TX = @BL_CC_TX 
	where REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF




GO

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BritishLibraryCCValuesSetOnLicense]    Script Date: 8/28/2019 11:20:04 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BritishLibraryCCValuesSetOnLicense]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_BritishLibraryCCValuesSetOnLicense]
GO



CREATE procedure [dbo].[st_BritishLibraryCCValuesSetOnLicense]
(
	@SITE_LIC_ID int,
	@BL_CC_ACCOUNT_CODE nvarchar(50), 
	@BL_CC_AUTH_CODE nvarchar(50), 
	@BL_CC_TX nvarchar(50)
)

As

SET NOCOUNT ON

	update Reviewer.dbo.TB_SITE_LIC
	set BL_CC_ACCOUNT_CODE = @BL_CC_ACCOUNT_CODE,
	BL_CC_AUTH_CODE = @BL_CC_AUTH_CODE,
	BL_CC_TX = @BL_CC_TX 
	where SITE_LIC_ID = @SITE_LIC_ID

SET NOCOUNT OFF




GO

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BritishLibraryValuesGetAll]    Script Date: 8/28/2019 11:20:13 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BritishLibraryValuesGetAll]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_BritishLibraryValuesGetAll]
GO

-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<gets contact table for a contactID>
-- =============================================
CREATE PROCEDURE [dbo].[st_BritishLibraryValuesGetAll] 
(
	@REVIEW_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    -- need to lose the milliseconds as the database is setting it to 000

	select BL_ACCOUNT_CODE, BL_AUTH_CODE, BL_TX, 
	BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX 
	from Reviewer.dbo.TB_REVIEW 
	where REVIEW_ID = @REVIEW_ID
    	
	RETURN
END


GO

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BritishLibraryValuesGetFromLicense]    Script Date: 8/28/2019 11:20:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BritishLibraryValuesGetFromLicense]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_BritishLibraryValuesGetFromLicense]
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<gets contact table for a contactID>
-- =============================================
CREATE PROCEDURE [dbo].[st_BritishLibraryValuesGetFromLicense] 
(
	@SITE_LIC_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    -- need to lose the milliseconds as the database is setting it to 000

	select BL_ACCOUNT_CODE, BL_AUTH_CODE, BL_TX, 
	BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX 
	from Reviewer.dbo.TB_SITE_LIC 
	where SITE_LIC_ID = @SITE_LIC_ID
    	
	RETURN
END


GO

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BritishLibraryValuesSet]    Script Date: 8/28/2019 11:20:39 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BritishLibraryValuesSet]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_BritishLibraryValuesSet]
GO


CREATE procedure [dbo].[st_BritishLibraryValuesSet]
(
	@REVIEW_ID int,
	@BL_ACCOUNT_CODE nvarchar(50), 
	@BL_AUTH_CODE nvarchar(50), 
	@BL_TX nvarchar(50)
)

As

SET NOCOUNT ON

	update Reviewer.dbo.TB_REVIEW
	set BL_ACCOUNT_CODE = @BL_ACCOUNT_CODE,
	BL_AUTH_CODE = @BL_AUTH_CODE,
	BL_TX = @BL_TX 
	where REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF



GO

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BritishLibraryValuesSetOnLicense]    Script Date: 8/28/2019 11:20:47 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].st_BritishLibraryValuesSetOnLicense') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_BritishLibraryValuesSetOnLicense]
GO



CREATE procedure [dbo].[st_BritishLibraryValuesSetOnLicense]
(
	@SITE_LIC_ID int,
	@BL_ACCOUNT_CODE nvarchar(50), 
	@BL_AUTH_CODE nvarchar(50), 
	@BL_TX nvarchar(50)
)

As

SET NOCOUNT ON

	update Reviewer.dbo.TB_SITE_LIC
	set BL_ACCOUNT_CODE = @BL_ACCOUNT_CODE,
	BL_AUTH_CODE = @BL_AUTH_CODE,
	BL_TX = @BL_TX 
	where SITE_LIC_ID = @SITE_LIC_ID

SET NOCOUNT OFF



GO

USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewRemoveMember_1]    Script Date: 6/3/2020 10:55:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE or ALTER procedure [dbo].[st_ReviewRemoveMember_1]
(
	@REVIEW_ID int,
	@CONTACT_ID int
)

As

SET NOCOUNT ON

	declare @funder_id int
	
	select @funder_id = FUNDER_ID from Reviewer.dbo.TB_REVIEW
	where REVIEW_ID = @REVIEW_ID
	
	if @funder_id != @CONTACT_ID
	begin
		delete Reviewer.dbo.TB_CONTACT_REVIEW_ROLE from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE crr
		inner join Reviewer.dbo.TB_REVIEW_CONTACT rc
		on rc.REVIEW_CONTACT_ID = crr.REVIEW_CONTACT_ID
		and rc.CONTACT_ID = @CONTACT_ID
		and rc.REVIEW_ID = @REVIEW_ID
	                
		delete from Reviewer.dbo.TB_REVIEW_CONTACT 
		where CONTACT_ID = @CONTACT_ID
		and REVIEW_ID = @REVIEW_ID
    end
		

SET NOCOUNT OFF
GO
