Use Reviewer
GO

IF COL_LENGTH('dbo.TB_WEBDB', 'SUBTITLE') IS NULL
BEGIN
BEGIN Transaction
ALTER TABLE dbo.TB_WEBDB ADD
	SUBTITLE nvarchar(1000) NULL

ALTER TABLE dbo.TB_WEBDB SET (LOCK_ESCALATION = TABLE)

COMMIT
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbCreateOrEdit]    Script Date: 27/04/2021 09:10:54 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_WebDbCreateOrEdit]
(
	@RevId int,
	@AttIdFilter bigint,
	@isOpen bit,
	@ContactId int,
	@Username varchar(50) = '',
	@Password nvarchar(2000) = '',
	@WebDbName nvarchar(1000),
	@Subtitle nvarchar(1000) ,
	@HeaderImage1Url nvarchar(1000) ,
	@HeaderImage2Url nvarchar(1000) ,
	@HeaderImage3Url nvarchar(1000) ,
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
				[DESCRIPTION] = @Description, EDITED_BY = @ContactId, SUBTITLE = @Subtitle
				, HEADER_IMAGE_1_URL = @HeaderImage1Url, HEADER_IMAGE_2_URL = @HeaderImage2Url, HEADER_IMAGE_3_URL = @HeaderImage3Url
				where WEBDB_ID = @WebDbId
		--wipe username and password if WebDb is supposed to be open:
		if @isOpen = 1 
			UPDATE TB_WEBDB Set PWASHED = null, USERNAME = null, FLAVOUR = null where WEBDB_ID = @WebDbId
	END
	ELSE
	BEGIN
		Insert into TB_WEBDB 
			(REVIEW_ID, WITH_ATTRIBUTE_ID, IS_OPEN, WEBDB_NAME, [DESCRIPTION], CREATED_BY, EDITED_BY
			, SUBTITLE, HEADER_IMAGE_1_URL, HEADER_IMAGE_2_URL, HEADER_IMAGE_3_URL)
		Values (@RevId, @AttIdFilter, @isOpen, @WebDbName, @Description, @ContactId, @ContactId
			,@Subtitle, @HeaderImage1Url, @HeaderImage2Url, @HeaderImage3Url)
		select @WebDbId = SCOPE_IDENTITY()
	END

	--at this point we deal with username and password if needed. 
	if @isOpen = 0
	BEGIN
	--create salt!
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
		
	UPDATE TB_WEBDB set USERNAME = @Username, FLAVOUR = @rnd, PWASHED = HASHBYTES('SHA1', @PASSWORD + @rnd)
	where WEBDB_ID = @WebDbId
	END
GO

ALTER       procedure [dbo].[st_WebDBget]
(
	@WebDBid int,
	@RevId int	
)

As
BEGIN
select [WEBDB_ID]
		  ,[REVIEW_ID]
		  ,[WITH_ATTRIBUTE_ID]
		  ,[IS_OPEN]
		  ,w.[USERNAME]
		  ,[WEBDB_NAME]
		  ,SUBTITLE
		  ,w.[DESCRIPTION]
		  ,c1.CONTACT_NAME as [CREATED_BY]
		  ,c2.CONTACT_NAME as [EDITED_BY]
		  ,w.[MAP_TITLE]
		  ,w.[MAP_URL]
		  ,w.[HEADER_IMAGE_1_URL]
		  ,w.[HEADER_IMAGE_2_URL]
		  ,w.[HEADER_IMAGE_3_URL]
		  ,w.HEADER_IMAGE_1
		  ,w.HEADER_IMAGE_2
		  ,w.HEADER_IMAGE_3
	  FROM [TB_WEBDB] w
	  inner join TB_CONTACT c1 on w.CREATED_BY = c1.CONTACT_ID
	  inner join TB_CONTACT c2 on w.EDITED_BY = c2.CONTACT_ID 
	  where w.WEBDB_ID = @WebDBid AND w.REVIEW_ID = @RevId
END
GO


ALTER   PROCEDURE [dbo].[st_WebDbListGet]
(
	@RevId INT,
	@ContactId int
)
As
BEGIN
	declare @check int = -1
	--check is this user in the review? (or a site_admin?)
		set @check = (select count(*) from TB_REVIEW_CONTACT rc
					inner join TB_CONTACT c on (rc.CONTACT_ID = @ContactId and rc.CONTACT_ID = c.CONTACT_ID and REVIEW_ID = @RevId)
												--OR (c.CONTACT_ID = @ContactId and c.IS_SITE_ADMIN = 1)
				 )

	if @check < 1 
	BEGIN
		set @check = (select count(*) from TB_CONTACT where CONTACT_ID = @ContactId and IS_SITE_ADMIN = 1)
	END
	if @check < 1 return
	
	SELECT [WEBDB_ID]
		  ,[REVIEW_ID]
		  ,[WITH_ATTRIBUTE_ID]
		  ,[IS_OPEN]
		  ,w.[USERNAME]
		  ,[WEBDB_NAME]
		  ,SUBTITLE
		  ,w.[DESCRIPTION]
		  ,c1.CONTACT_NAME as [CREATED_BY]
		  ,c2.CONTACT_NAME as [EDITED_BY]
		  , w.HEADER_IMAGE_1
		  , w.HEADER_IMAGE_2
		  , w.HEADER_IMAGE_3
		  , w.MAP_TITLE
		  , w.MAP_URL
		  , w.HEADER_IMAGE_1_URL
		  , w.HEADER_IMAGE_2_URL
		  , w.HEADER_IMAGE_3_URL
	  FROM [TB_WEBDB] w
	  inner join TB_CONTACT c1 on w.CREATED_BY = c1.CONTACT_ID
	  inner join TB_CONTACT c2 on w.EDITED_BY = c2.CONTACT_ID
	  where REVIEW_ID = @RevId
END
GO
