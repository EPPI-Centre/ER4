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
	PWASHED binary(20) NULL,
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
	WEBDB_ATTRIBUTE_NAME nvarchar(255) NULL,
	WEBDB_ATTRIBUTE_DESCRIPTION nvarchar(2000) NULL
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
	@WebDbId int output,
	@Result int = 0 output
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
	set @Result = 0
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

	IF @WebDbId > 0
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

CREATE OR ALTER PROCEDURE [dbo].[st_WebDbListGet]
(
	@RevId INT,
	@ContactId int
)
As
	declare @check int = -1
	--check is this user in the review? (or a site_admin?)
	set @check = (select count(*) from TB_REVIEW_CONTACT rc
					inner join TB_CONTACT c on (rc.CONTACT_ID = @ContactId and rc.CONTACT_ID = c.CONTACT_ID and REVIEW_ID = @RevId)
												OR (c.CONTACT_ID = @ContactId and c.IS_SITE_ADMIN = 1)
				 )

	if @check < 1 return
	
	SELECT [WEBDB_ID]
		  ,[REVIEW_ID]
		  ,[WITH_ATTRIBUTE_ID]
		  ,[IS_OPEN]
		  ,w.[USERNAME]
		  
		  ,[WEBDB_NAME]
		  ,w.[DESCRIPTION]
		  ,c1.CONTACT_NAME as [CREATED_BY]
		  ,c2.CONTACT_NAME as [EDITED_BY]
	  FROM [TB_WEBDB] w
	  inner join TB_CONTACT c1 on w.CREATED_BY = c1.CONTACT_ID
	  inner join TB_CONTACT c2 on w.EDITED_BY = c2.CONTACT_ID
GO


GO

CREATE OR ALTER PROCEDURE [dbo].[st_WebDbDelete]
(
	@REVIEW_ID INT,
	@WEBDB_ID int
)
As
--basic check: does this thing exist?
if (SELECT count(*) from TB_WEBDB where WEBDB_ID = @WEBDB_ID and @REVIEW_ID = REVIEW_ID) != 1 return

--delete attributes
delete from TB_WEBDB_PUBLIC_ATTRIBUTE where WEBDB_ID = @WEBDB_ID 
--delete Sets
delete from TB_WEBDB_PUBLIC_SET where WEBDB_ID = @WEBDB_ID
--delete webdb
delete from TB_WEBDB where WEBDB_ID = @WEBDB_ID

GO


CREATE OR ALTER PROCEDURE [dbo].[st_WebDbGetCodesets]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@WEBDB_PUBLIC_SET_ID int = 0
)

As

SET NOCOUNT ON

	SELECT RS.REVIEW_SET_ID, REVIEW_ID, RS.SET_ID, ALLOW_CODING_EDITS, S.SET_TYPE_ID, 
		CASE 
			WHEN WEBDB_SET_NAME IS Null then SET_NAME
			else WEBDB_SET_NAME
		END as SET_NAME, 
		SET_TYPE, CODING_IS_FINAL, SET_ORDER, MAX_DEPTH, 
		CASE 
			WHEN WEBDB_SET_DESCRIPTION IS Null then S.SET_DESCRIPTION
			else WEBDB_SET_DESCRIPTION
		END as SET_DESCRIPTION, 
		S.ORIGINAL_SET_ID, S.USER_CAN_EDIT_URLS,
		WS.WEBDB_ID, WS.WEBDB_PUBLIC_SET_ID
	FROM TB_WEBDB_PUBLIC_SET WS
	INNER JOIN TB_REVIEW_SET RS on WS.WEBDB_ID = @WEBDB_ID and WS.REVIEW_SET_ID = RS.REVIEW_SET_ID and RS.REVIEW_ID = @REVIEW_ID
				AND (@WEBDB_PUBLIC_SET_ID = 0 OR WS.WEBDB_PUBLIC_SET_ID = @WEBDB_PUBLIC_SET_ID)--this allows to have one SP to get all or just one set
	INNER JOIN TB_SET S ON S.SET_ID = RS.SET_ID
	INNER JOIN TB_SET_TYPE ON TB_SET_TYPE.SET_TYPE_ID = S.SET_TYPE_ID

	ORDER BY RS.SET_ORDER, RS.SET_ID

SET NOCOUNT OFF
GO

CREATE OR ALTER procedure [dbo].[st_WebDbGetAllAttributesInSet]
(
	@SET_ID INT,
	@WEBDB_ID int
)

As

SELECT tas.ATTRIBUTE_SET_ID, tas.SET_ID, tas.ATTRIBUTE_ID, tas.PARENT_ATTRIBUTE_ID,
	tas.ATTRIBUTE_TYPE_ID, tas.ATTRIBUTE_SET_DESC, tas.ATTRIBUTE_ORDER, ATTRIBUTE_TYPE, 
	case 
		WHEN WEBDB_ATTRIBUTE_NAME is null then a.ATTRIBUTE_NAME
		else WEBDB_ATTRIBUTE_NAME
	END as ATTRIBUTE_NAME, 
	ATTRIBUTE_SET_DESC, CONTACT_ID, 
	case 
		WHEN WEBDB_ATTRIBUTE_DESCRIPTION is null then a.ATTRIBUTE_DESC
		else WEBDB_ATTRIBUTE_DESCRIPTION
	END as ATTRIBUTE_DESC, 
	Ext_URL, Ext_Type,
	ORIGINAL_ATTRIBUTE_ID

FROM TB_WEBDB_PUBLIC_ATTRIBUTE wa
INNER JOIN TB_ATTRIBUTE a on wa.WEBDB_ID = @WEBDB_ID and a.ATTRIBUTE_ID = wa.ATTRIBUTE_ID
INNER JOIN TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = a.ATTRIBUTE_ID and tas.SET_ID = @SET_ID and tas.PARENT_ATTRIBUTE_ID is not null
INNER JOIN TB_SET ON TB_SET.SET_ID = tas.SET_ID 
INNER JOIN TB_ATTRIBUTE_TYPE t ON t.ATTRIBUTE_TYPE_ID = tas.ATTRIBUTE_TYPE_ID

WHERE  dbo.fn_IsAttributeInTree(a.attribute_id) = 1

ORDER BY PARENT_ATTRIBUTE_ID, ATTRIBUTE_ORDER
GO

CREATE OR ALTER PROCEDURE [dbo].[st_WebDbCodesetAdd]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@Set_ID int,
	@WEBDB_PUBLIC_SET_ID int output
)
As
declare @r_set_id int = (select review_set_id from TB_WEBDB w
						inner join TB_REVIEW_SET rs on rs.SET_ID = @Set_ID and rs.REVIEW_ID = @REVIEW_ID and w.REVIEW_ID = rs.REVIEW_ID
						where w.WEBDB_ID = @WEBDB_ID and w.REVIEW_ID = @REVIEW_ID)
--Just a basic sanity check: can we get a REVIEW_SET_ID?
IF @r_set_id is null OR @r_set_id < 1 return

INSERT INTO [TB_WEBDB_PUBLIC_SET]
           ([WEBDB_ID]
           ,[REVIEW_SET_ID]
           ,[WEBDB_SET_NAME]
           ,[WEBDB_SET_DESCRIPTION])
     VALUES
           (@WEBDB_ID
           ,@r_set_id
           ,NULL, NULL)
set @WEBDB_PUBLIC_SET_ID = SCOPE_IDENTITY()
INSERT INTO [TB_WEBDB_PUBLIC_ATTRIBUTE]
           ([WEBDB_ID]
           ,[ATTRIBUTE_ID]
           ,[WEBDB_ATTRIBUTE_NAME]
           ,[WEBDB_ATTRIBUTE_DESCRIPTION])
     SELECT @WEBDB_ID, a.ATTRIBUTE_ID, NULL, NULL
	 FROM tb_attribute a
	 inner join TB_ATTRIBUTE_SET tas on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and tas.SET_ID = @Set_ID
				and dbo.fn_IsAttributeInTree(a.ATTRIBUTE_ID) = 1
	 inner join TB_REVIEW_SET rs on tas.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID and rs.REVIEW_SET_ID = @r_set_id

GO

CREATE OR ALTER PROCEDURE [dbo].[st_WebDbCodeSetDelete]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@Set_ID int
)
As
declare @r_set_id int = (select review_set_id from TB_WEBDB w
						inner join TB_REVIEW_SET rs on rs.SET_ID = @Set_ID and rs.REVIEW_ID = @REVIEW_ID and w.REVIEW_ID = rs.REVIEW_ID
						where w.WEBDB_ID = @WEBDB_ID and w.REVIEW_ID = @REVIEW_ID)
--Just a basic sanity check: can we get a REVIEW_SET_ID?
IF @r_set_id is null OR @r_set_id < 1 return

Declare @atts table (A_ID bigint primary key)
INSERT into @atts select tas.Attribute_id from TB_WEBDB w
	inner join TB_WEBDB_PUBLIC_ATTRIBUTE wa on w.REVIEW_ID = @REVIEW_ID and w.WEBDB_ID = @WEBDB_ID and wa.WEBDB_ID = @WEBDB_ID
	inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = wa.ATTRIBUTE_ID and tas.SET_ID = @Set_ID
	inner join TB_REVIEW_SET rs on tas.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID and rs.REVIEW_SET_ID = @r_set_id

DELETE from TB_WEBDB_PUBLIC_ATTRIBUTE 
	where ATTRIBUTE_ID in (select * from @atts)
	and WEBDB_ID = @WEBDB_ID
DELETE from TB_WEBDB_PUBLIC_SET where WEBDB_ID = @WEBDB_ID and REVIEW_SET_ID = @r_set_id

GO

CREATE OR ALTER PROCEDURE [dbo].[st_WebDbCodeSetEdit]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@Set_ID int,
	@Public_Name nvarchar(255),
	@Public_Descr nvarchar(2000)
)
As
declare @r_set_id int = (select review_set_id from TB_WEBDB w
						inner join TB_REVIEW_SET rs on rs.SET_ID = @Set_ID and rs.REVIEW_ID = @REVIEW_ID and w.REVIEW_ID = rs.REVIEW_ID
						where w.WEBDB_ID = @WEBDB_ID and w.REVIEW_ID = @REVIEW_ID)
--Just a basic sanity check: can we get a REVIEW_SET_ID?
IF @r_set_id is null OR @r_set_id < 1 return
update TB_WEBDB_PUBLIC_SET set WEBDB_SET_NAME = @Public_Name, WEBDB_SET_DESCRIPTION = @Public_Descr 
 where REVIEW_SET_ID = @r_set_id
GO


CREATE OR ALTER PROCEDURE [dbo].[st_WebDbAttributeDelete]
(
	@ATTRIBUTE_ID bigint,
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@Set_ID int
)
As
declare @r_set_id int = (select review_set_id from TB_WEBDB w
						inner join TB_REVIEW_SET rs on rs.SET_ID = @Set_ID and rs.REVIEW_ID = @REVIEW_ID and w.REVIEW_ID = rs.REVIEW_ID
						where w.WEBDB_ID = @WEBDB_ID and w.REVIEW_ID = @REVIEW_ID)
--Just a basic sanity check: can we get a REVIEW_SET_ID?
IF @r_set_id is null OR @r_set_id < 1 return

--select @r_set_id

declare @dels table (d_id bigint primary key)
Declare @atts table (A_ID bigint primary key) 
declare @rows int = 1
declare @count int = 0

INSERT into @atts select tas.Attribute_id from TB_WEBDB w
	inner join TB_WEBDB_PUBLIC_ATTRIBUTE wa on w.REVIEW_ID = @REVIEW_ID and w.WEBDB_ID = @WEBDB_ID and wa.WEBDB_ID = @WEBDB_ID
	inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = wa.ATTRIBUTE_ID and tas.SET_ID = @Set_ID
	inner join TB_REVIEW_SET rs on tas.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID and rs.REVIEW_SET_ID = @r_set_id
--another check: is this Attribute inside the @atts table?
--select * from @atts order by A_ID
--select count(*) as c from @atts where A_ID = @ATTRIBUTE_ID

IF (select count(*) from @atts where A_ID = @ATTRIBUTE_ID) < 1 return
insert into @dels (d_id) values (@ATTRIBUTE_ID)

--limited recursion here: we want to remove all children of the code we're taking out
--500 rounds max: just making sure this can't run forever... Each round should handle one nesting level so in theory this works for trees that are 500 levels deep
while @rows > 0 and @count < 500 
BEGIN
	set @count = @count +1
	insert into @dels 
		SELECT attribute_id from @atts a
		inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = a.A_ID and tas.SET_ID = @Set_ID
		inner join TB_REVIEW_SET rs on tas.SET_ID = rs.SET_ID and rs.REVIEW_SET_ID = @r_set_id
		where tas.PARENT_ATTRIBUTE_ID in (select d_id from @dels) 
			AND A.A_ID not in (select d_id from @dels)--do not insert the same att twice
	set @rows = @@ROWCOUNT
END
DELETE from TB_WEBDB_PUBLIC_ATTRIBUTE 
	where ATTRIBUTE_ID in (select d_id from @dels)
	AND WEBDB_ID = @WEBDB_ID
GO

CREATE OR ALTER PROCEDURE [dbo].[st_WebDbAttributeAdd]
(
	@ATTRIBUTE_ID bigint,
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@Set_ID int
)
As
declare @r_set_id int = (select review_set_id from TB_WEBDB w
						inner join TB_REVIEW_SET rs on rs.SET_ID = @Set_ID and rs.REVIEW_ID = @REVIEW_ID and w.REVIEW_ID = rs.REVIEW_ID
						where w.WEBDB_ID = @WEBDB_ID and w.REVIEW_ID = @REVIEW_ID)
--Just a basic sanity check: can we get a REVIEW_SET_ID?
IF @r_set_id is null OR @r_set_id < 1 return

--select @r_set_id

declare @adders table (d_id bigint primary key)
Declare @atts table (A_ID bigint primary key) --Attributes currently in the WebDB for this set
Declare @All_atts table (AA_ID bigint primary key) --All valid attributes currently in the set (excluding detached ones)
declare @rows int = 1
declare @count int = 0

INSERT into @atts select tas.Attribute_id from TB_WEBDB w
	inner join TB_WEBDB_PUBLIC_ATTRIBUTE wa on w.REVIEW_ID = @REVIEW_ID and w.WEBDB_ID = @WEBDB_ID and wa.WEBDB_ID = @WEBDB_ID
	inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = wa.ATTRIBUTE_ID and tas.SET_ID = @Set_ID
	inner join TB_REVIEW_SET rs on tas.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID and rs.REVIEW_SET_ID = @r_set_id
--select * from @atts order by A_ID
--select count(*) as c from @atts where A_ID = @ATTRIBUTE_ID

insert into @All_atts select a.ATTRIBUTE_ID from tb_attribute a
	 inner join TB_ATTRIBUTE_SET tas on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and tas.SET_ID = @Set_ID
				and dbo.fn_IsAttributeInTree(a.ATTRIBUTE_ID) = 1
	 inner join TB_REVIEW_SET rs on tas.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID and rs.REVIEW_SET_ID = @r_set_id

--another check: is this Attribute inside the @All_atts table?
IF (select count(*) from @All_atts where AA_ID = @ATTRIBUTE_ID) < 1 return
insert into @adders (d_id) values (@ATTRIBUTE_ID)

--limited recursion here: we want to add all children of the code we're taking out
--500 rounds max: just making sure this can't run forever... Each round should handle one nesting level so in theory this works for trees that are 500 levels deep
while @rows > 0 and @count < 500 
BEGIN
	set @count = @count +1
	insert into @adders 
		SELECT attribute_id from @All_atts a
		inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = a.AA_ID and tas.SET_ID = @Set_ID
		inner join TB_REVIEW_SET rs on tas.SET_ID = rs.SET_ID and rs.REVIEW_SET_ID = @r_set_id
		where tas.PARENT_ATTRIBUTE_ID in (select d_id from @adders) 
			AND A.AA_ID not in (select d_id from @adders)--those we haven't found already
			AND A.AA_ID not in (select A_ID from @atts)--those that aren't already in the WEBDB
	set @rows = @@ROWCOUNT
END
Insert into [TB_WEBDB_PUBLIC_ATTRIBUTE]
           ([WEBDB_ID]
           ,[ATTRIBUTE_ID]
           ,[WEBDB_ATTRIBUTE_NAME]
           ,[WEBDB_ATTRIBUTE_DESCRIPTION])
     SELECT @WEBDB_ID, a.d_id, NULL, NULL
	 FROM @adders a
GO

CREATE OR ALTER PROCEDURE [dbo].[st_WebDbAttributeEdit]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@ATTRIBUTE_ID bigint,
	@Public_Name nvarchar(255),
	@Public_Descr nvarchar(2000)
)
As
declare @WEBDB_PUBLIC_ATTRIBUTE_ID int = (select WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE a 
							on a.WEBDB_ID = w.WEBDB_ID and w.WEBDB_ID = @WEBDB_ID 
							and ATTRIBUTE_ID = @ATTRIBUTE_ID and w.REVIEW_ID = @REVIEW_ID)
--Just a basic sanity check: can we get the record to edit?
IF @WEBDB_PUBLIC_ATTRIBUTE_ID is null OR @WEBDB_PUBLIC_ATTRIBUTE_ID < 1 return
update TB_WEBDB_PUBLIC_ATTRIBUTE set WEBDB_ATTRIBUTE_NAME = @Public_Name, WEBDB_ATTRIBUTE_DESCRIPTION = @Public_Descr 
 where WEBDB_PUBLIC_ATTRIBUTE_ID = @WEBDB_PUBLIC_ATTRIBUTE_ID
GO



go