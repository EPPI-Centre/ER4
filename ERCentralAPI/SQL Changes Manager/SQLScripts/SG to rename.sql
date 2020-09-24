USE [Reviewer]
GO

IF COL_LENGTH('dbo.TB_REVIEW', 'HAS_WEB_DB') IS NULL
BEGIN

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

ALTER TABLE dbo.TB_REVIEW ADD
	HAS_WEB_DB bit NULL

COMMIT
END
GO

USE [Reviewer]
GO
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_WEBDB')
BEGIN
BEGIN TRANSACTION
	--drop the new tables
	DROP TABLE [dbo].TB_WEBDB_PUBLIC_ATTRIBUTE
	DROP TABLE [dbo].TB_WEBDB_PUBLIC_SET
	DROP TABLE [dbo].TB_WEBDB
	--wipe TB_REVIEW.HAS_WEB_DB to ensure consistency
	update TB_REVIEW SET HAS_WEB_DB = 0 where HAS_WEB_DB = 1
COMMIT
END
GO
--CREATE new tables
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
ALTER TABLE dbo.TB_REVIEW SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.TB_WEBDB
	(
	WEBDB_ID int NOT NULL IDENTITY (1, 1),
	REVIEW_ID int NOT NULL,
	WITH_ATTRIBUTE_ID bigint NULL,
	IS_OPEN bit NOT NULL,
	USERNAME varchar(50) NULL,
	FLAVOUR char(20) NULL,
	PWASHED binary(20) NOT NULL,
	WEBDB_NAME nvarchar(1000) NULL,
	DESCRIPTION nvarchar(MAX) NULL,
	CREATED_BY int,
	EDITED_BY int,
	HEADER_IMAGE_1 image NULL,
	HEADER_IMAGE_2 image NULL,
	HEADER_IMAGE_3 image NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.TB_WEBDB ADD CONSTRAINT
	DF_TB_WEBDB_IS_OPEN DEFAULT 1 FOR IS_OPEN
GO
ALTER TABLE dbo.TB_WEBDB ADD CONSTRAINT
	PK_TB_WEBDB PRIMARY KEY CLUSTERED 
	(
	WEBDB_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.TB_WEBDB ADD CONSTRAINT
	FK_TB_WEBDB_TB_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.TB_WEBDB ADD CONSTRAINT
	FK_TB_WEBDB_TB_CONTACT_C FOREIGN KEY
	(
	CREATED_BY
	) REFERENCES dbo.TB_CONTACT
	(
	CONTACT_ID
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
GO
ALTER TABLE dbo.TB_WEBDB ADD CONSTRAINT
	FK_TB_WEBDB_TB_CONTACT_E FOREIGN KEY
	(
	EDITED_BY
	) REFERENCES dbo.TB_CONTACT
	(
	CONTACT_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE NO ACTION	
GO
ALTER TABLE dbo.TB_WEBDB SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
GO

BEGIN TRANSACTION
GO
CREATE TABLE dbo.TB_WEBDB_PUBLIC_SET
	(
	WEBDB_PUBLIC_SET_ID int NOT NULL IDENTITY (1, 1),
	WEBDB_ID int NOT NULL,
	REVIEW_SET_ID int NOT NULL,
	WEBDB_SET_NAME nvarchar(255) NULL,
	WEBDB_SET_DESCRIPTION nvarchar(2000) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TB_WEBDB_PUBLIC_SET ADD CONSTRAINT
	PK_TB_WEBDB_PUBLIC_SET PRIMARY KEY CLUSTERED 
	(
	WEBDB_PUBLIC_SET_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.TB_WEBDB_PUBLIC_SET ADD CONSTRAINT
	FK_TB_WEBDB_PUBLIC_SET_TB_WEBDB FOREIGN KEY
	(
	WEBDB_ID
	) REFERENCES dbo.TB_WEBDB
	(
	WEBDB_ID
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.TB_WEBDB_PUBLIC_SET ADD CONSTRAINT
	FK_TB_WEBDB_PUBLIC_SET_TB_REVIEW_SET FOREIGN KEY
	(
	REVIEW_SET_ID
	) REFERENCES dbo.TB_REVIEW_SET
	(
	REVIEW_SET_ID
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.TB_WEBDB_PUBLIC_SET SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
GO
BEGIN TRANSACTION
GO
CREATE TABLE dbo.TB_WEBDB_PUBLIC_ATTRIBUTE
	(
	WEBDB_PUBLIC_ATTRIBUTE_ID int NOT NULL IDENTITY (1, 1),
	WEBDB_ID int NOT NULL,
	ATTRIBUTE_ID bigint NOT NULL,
	WEBDB_SET_NAME nvarchar(255) NULL,
	WEBDB_SET_DESCRIPTION nvarchar(2000) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TB_WEBDB_PUBLIC_ATTRIBUTE ADD CONSTRAINT
	PK_TB_WEBDB_PUBLIC_ATTRIBUTE PRIMARY KEY CLUSTERED 
	(
	WEBDB_PUBLIC_ATTRIBUTE_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.TB_WEBDB_PUBLIC_ATTRIBUTE ADD CONSTRAINT
	FK_TB_WEBDB_PUBLIC_ATTRIBUTE_TB_WEBDB FOREIGN KEY
	(
	WEBDB_ID
	) REFERENCES dbo.TB_WEBDB
	(
	WEBDB_ID
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.TB_WEBDB_PUBLIC_ATTRIBUTE ADD CONSTRAINT
	FK_TB_WEBDB_PUBLIC_ATTRIBUTE_TB_ATTRIBUTE FOREIGN KEY
	(
	ATTRIBUTE_ID
	) REFERENCES dbo.TB_ATTRIBUTE
	(
	ATTRIBUTE_ID
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.TB_WEBDB_PUBLIC_ATTRIBUTE SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
GO

USE [Reviewer]
GO
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[st_WebDbCreateOrEdit]
(
	@RevId int,
	@AttIdFilter bigint,
	@isOpen bit,
	@ContactId int,
	@Username varchar(50) = '',
	@Password nvarchar(2000) = '',
	@WebDbName nvarchar(1000),
	@Description nvarchar(max),
	@WebDbId int = 0 output,
	@Result int = 0
)
AS

--various possibilities: Create or Edit 
--Create if @WebDbId = 0;
--Edit if @WebDbId is supplied.
--BUT ALSO: generate and update password hash or not.
--Generate PW hash IF @Password != '' and @isOpen = 0 -- we will also update the username in this case.
--Ignore PW field otherwise and empty the Username.
--FAIL if @isOpen = 0 and we're creating a WebDb but don't have both username and password,
--FAIL (negative @Result) if @isOpen = 0 and we're editing a WebDB that doesn't already have username and password.
	
	--Initial checks (possible failures)
	IF @WebDbId <= 0 --Creating new WebDb
		AND @isOpen = 0 --we need username and PW
		AND 
		(
			LEN(@Username) <= 4 --username too short
			OR LEN(@Password) < 6 --password too short
		)
	BEGIN
		SET @Result = -1 --failure to pass first check, username or password are too short
		return
	END
	ELSE IF @WebDbId > 0  
		AND @isOpen = 0 --we might need username and PW
		AND 
		( --we have to change username/password as they are not present: 
				(select count(*) from TB_WEBDB where WEBDB_ID = @WebDbId AND PWASHED is null) = 1
		)	
		AND 
		( --we need username and password, are the one supplied long enough?
			LEN(@Username) <= 4 --username too short
			OR LEN(@Password) < 6 --password too short
		)
	BEGIN
		SET @Result = -1 --as above failure to pass first check, username or password are too short
		return
	END

	declare @check int = -1
	--other check is this user an admin in the review? (or a site_admin?)
	set @check = (select count(*) from TB_CONTACT c
					Where c.CONTACT_ID = @ContactId
						AND (
							@ContactId in 
									(
									select rc.CONTACT_ID 
									from TB_REVIEW_CONTACT rc inner join TB_CONTACT_REVIEW_ROLE crr on 
										rc.REVIEW_ID = @RevId 
										and rc.REVIEW_CONTACT_ID = crr.REVIEW_CONTACT_ID
										and crr.ROLE_NAME = 'AdminUser'
									) --user is an admin for this review
							OR
							c.IS_SITE_ADMIN = 1 --user is a site admin, can do anything
							)
					)
	if @check < 1
	BEGIN
		set @Result = -2 --not allowed!
		return
	END


	--from here on, we believe the input is good (1 exception), we're not asked to achieve the impossible.
	--Changing data: first we'll create OR edit the record, then IF @isOpen = 0 we'll do an update for username and PW.
	--IF @isOpen = 1 we will wipe Username and PW, this is because if one opens the WebDb and then wants to close it, they might have forgotten old PW,
	--so we are forcing them to re-set it...

	IF @WebDbId > 1
	BEGIN
		set @check = -1
		--we're updating something: can we find it? (should we also check if the user is an admin?)
		set @check = (Select count(*) from TB_WEBDB w 
								Inner join tb_review r on w.REVIEW_ID = @RevId and w.WEBDB_ID = @WebDbId and w.REVIEW_ID = r.REVIEW_ID
								)
		if @check != 1
		BEGIN
			set @Result = -3 --couldn't find this WebDb
			return
		END
		--All good, we've been asked to edit a webDB that exists and is tied to the correct review.
		UPDATE TB_WEBDB Set WITH_ATTRIBUTE_ID = @AttIdFilter, IS_OPEN = @isOpen, WEBDB_NAME = @WebDbName,
				[DESCRIPTION] = @Description,	EDITED_BY = @ContactId
				where WEBDB_ID = @WebDbId
		--wipe username and password if WebDb is supposed to be open:
		if @isOpen = 1 
			UPDATE TB_WEBDB Set PWASHED = null, USERNAME = null, FLAVOUR = null where WEBDB_ID = @WebDbId
	END
	ELSE
	BEGIN
		Insert into TB_WEBDB (REVIEW_ID, WITH_ATTRIBUTE_ID, IS_OPEN, WEBDB_NAME, [DESCRIPTION], CREATED_BY, EDITED_BY)
		Values (@RevId, @AttIdFilter, @isOpen, @WebDbName, @Description, @ContactId, @ContactId)
		select @WebDbId = SCOPE_IDENTITY()
	END

	--at this point we deal with username and password if needed. 
	if @isOpen = 0
	BEGIN
	--create salt!
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
		
	UPDATE TB_WEBDB set USERNAME = @Username, FLAVOUR = @rnd, PWASHED = HASHBYTES('SHA1', @PASSWORD + @rnd)
	where WEBDB_ID = @WebDbId
	END
GO



go