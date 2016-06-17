--Script to create an EMPTY Reviewer database, this is the first script to run, the second generates the minimum data required by ER4.
--INSTRUCTIONS: (15/06/2016)
--In the first lines below, the filenames for DB files are set: change them accordingly! Make sure a DB called "reviewer" is not already present.
--Make sure that that the filenames used do not already exist.
--The security part is tricky! Involves 2 users.
--First user is a local account (may already exist) defaults to [NT AUTHORITY\NETWORK SERVICE], which is the user running the IIS worker process.
--This user will have data-changing rights across the whole DB.
--Second user is an account you need to create on your local machine (not with Admin rights), call it "sqlcmdshell", set PW to never expire.
--this second user has some special rights to allow executing SSIS packages and minimal access to the DB.
--By default, this script doesn't create the Logins for SQL, assumes they exist already, uncomment the relevant lines if needed.
--Script will add the two users to the new database and assign specific rights to them.
--Second script is called "generateReviewer 2nd Passage - DATA.sql" and assumes the existence of ReviewerAdmin DB (uses a SP therein to create users)



USE [master]
GO
/****** Object:  Database [Reviewer]    Script Date: 05/19/2016 12:20:12 ******/
CREATE DATABASE [Reviewer] ON  PRIMARY 
( NAME = N'Reviewer', FILENAME = N'd:\Microsoft SQL Server2008\Data\ReviewerFromScratch.mdf' , SIZE = 18127872KB , MAXSIZE = UNLIMITED, FILEGROWTH = 102400KB )
 LOG ON 
( NAME = N'Reviewer_log', FILENAME = N'd:\Microsoft SQL Server2008\Data\ReviewerFromScratch_log.ldf' , SIZE = 705152KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [Reviewer] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Reviewer].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Reviewer] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [Reviewer] SET ANSI_NULLS OFF
GO
ALTER DATABASE [Reviewer] SET ANSI_PADDING OFF
GO
ALTER DATABASE [Reviewer] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [Reviewer] SET ARITHABORT OFF
GO
ALTER DATABASE [Reviewer] SET AUTO_CLOSE OFF
GO
ALTER DATABASE [Reviewer] SET AUTO_CREATE_STATISTICS ON
GO
ALTER DATABASE [Reviewer] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [Reviewer] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [Reviewer] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [Reviewer] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [Reviewer] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [Reviewer] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [Reviewer] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [Reviewer] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [Reviewer] SET  DISABLE_BROKER
GO
ALTER DATABASE [Reviewer] SET AUTO_UPDATE_STATISTICS_ASYNC ON
GO
ALTER DATABASE [Reviewer] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [Reviewer] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [Reviewer] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [Reviewer] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [Reviewer] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [Reviewer] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [Reviewer] SET  READ_WRITE
GO
ALTER DATABASE [Reviewer] SET RECOVERY SIMPLE
GO
ALTER DATABASE [Reviewer] SET  MULTI_USER
GO
ALTER DATABASE [Reviewer] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [Reviewer] SET DB_CHAINING OFF
GO
EXEC sys.sp_db_vardecimal_storage_format N'Reviewer', N'ON'
GO

--*********************************************************************************
--FIRST Security part, should be edited!!!!!
--*********************************************************************************
--creates 2 logins, DB users, 
--the first is the account used by the IIS worker process
--the second is for executing SSIS packages (dedup et al).



/****** Object:  Login [NT AUTHORITY\NETWORK SERVICE]    Script Date: 6/15/2016 11:49:43 AM ******/
--CREATE LOGIN [NT AUTHORITY\NETWORK SERVICE] FROM WINDOWS WITH DEFAULT_DATABASE=[cms], DEFAULT_LANGUAGE=[us_english]
GO
/****** Object:  Login [SSRU38\sqlcmdshell]    Script Date: 6/15/2016 11:49:43 AM ******/
--CREATE LOGIN [SSRU38\sqlcmdshell] FROM WINDOWS WITH DEFAULT_DATABASE=[Reviewer], DEFAULT_LANGUAGE=[us_english]
GO

USE [Reviewer]
GO

/****** Object:  User [NT AUTHORITY\NETWORK SERVICE]    Script Date: 6/15/2016 11:49:44 AM ******/
CREATE USER [NT AUTHORITY\NETWORK SERVICE] FOR LOGIN [NT AUTHORITY\NETWORK SERVICE] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [SSRU38\sqlcmdshell]    Script Date: 6/15/2016 11:49:44 AM ******/
CREATE USER [SSRU38\sqlcmdshell] FOR LOGIN [SSRU38\sqlcmdshell] WITH DEFAULT_SCHEMA=[dbo]
GO

/****** Object:  DatabaseRole [db_executor]    Script Date: 6/15/2016 11:49:44 AM ******/
CREATE ROLE [db_executor]
GO
ALTER AUTHORIZATION ON ROLE::[db_executor] TO [dbo]
GO
GRANT EXECUTE TO db_executor
GO
sys.sp_addrolemember @rolename = N'db_executor', @membername = N'NT AUTHORITY\NETWORK SERVICE'
GO
sys.sp_addrolemember @rolename = N'db_ddladmin', @membername = N'NT AUTHORITY\NETWORK SERVICE'
GO
sys.sp_addrolemember @rolename = N'db_datareader', @membername = N'NT AUTHORITY\NETWORK SERVICE'
GO
sys.sp_addrolemember @rolename = N'db_datawriter', @membername = N'NT AUTHORITY\NETWORK SERVICE'
GO


--*********************************************************************************
--END FIRST Security part!!!!!
--Other security specific grants are given to [machinename\]sqlcmdshell where needed
--*********************************************************************************



/****** Object:  FullTextCatalog [tb_ITEM_ATTRIBUTE_FTIndex]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE FULLTEXT CATALOG [tb_ITEM_ATTRIBUTE_FTIndex]WITH ACCENT_SENSITIVITY = OFF
AUTHORIZATION [dbo]

GO
/****** Object:  FullTextCatalog [tb_ITEM_DOCUMENT_FTIndex]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE FULLTEXT CATALOG [tb_ITEM_DOCUMENT_FTIndex]WITH ACCENT_SENSITIVITY = OFF
AUTHORIZATION [dbo]

GO
/****** Object:  FullTextCatalog [tb_ITEM_FTIndex]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE FULLTEXT CATALOG [tb_ITEM_FTIndex]WITH ACCENT_SENSITIVITY = OFF
AUTHORIZATION [dbo]

GO
/****** Object:  UserDefinedFunction [dbo].[fn_CLEAN_SIMPLE_TEXT]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sergio
-- Create date: 24/07/09
-- Description:	Function to clean simple text fields
-- =============================================
CREATE FUNCTION [dbo].[fn_CLEAN_SIMPLE_TEXT] 
(
	-- Add the parameters for the function here
	@txt nvarchar(max)
)
RETURNS nvarchar(max)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Res nvarchar(max)

	-- Add the T-SQL statements to compute the return value here
	SET @Res = replace(cast(@txt as nvarchar(max)), CAST(0x01 AS nvarchar), ' ')
	SET @Res = replace(cast(@Res as nvarchar(max)), CAST(0x02 AS nvarchar), ' ')
	SET @Res = replace(cast(@Res as nvarchar(max)), CAST(0x07 AS nvarchar), ' ')
	SET @Res = replace(cast(@Res as nvarchar(max)), CAST(0x10 AS nvarchar), ' ')
	SET @Res = replace(cast(@Res as nvarchar(max)), CAST(0x11 AS nvarchar), ' ')
	SET @Res = replace(cast(@Res as nvarchar(max)), CAST(0x15 AS nvarchar), ' ')
	SET @Res = replace(cast(@Res as nvarchar(max)), CAST(0x17 AS nvarchar), ' ')
	SET @Res = replace(cast(@Res as nvarchar(max)), CAST(0x0C AS nvarchar), ' ')
	SET @Res = replace(cast(@Res as nvarchar(max)), CAST(0x1E AS nvarchar), ' ')
	SET @Res = replace(cast(@Res as nvarchar(max)), CAST(0x1F AS nvarchar), ' ')
	-- Return the result of the function
	RETURN @Res

END

GO
ALTER AUTHORIZATION ON [dbo].[fn_CLEAN_SIMPLE_TEXT] TO  SCHEMA OWNER 
GO
/****** Object:  UserDefinedFunction [dbo].[fn_GetFirstImportFilterID]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		S
-- Create date: 
-- Description:	stupid function to provide a default value in the tb_SOURCE.IMPORT_FILTER_ID column
-- =============================================
CREATE FUNCTION [dbo].[fn_GetFirstImportFilterID] 
(
	-- Add the parameters for the function here
	 
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Result int

	-- Add the T-SQL statements to compute the return value here
	SELECT @Result = (select top 1 import_filter_ID from TB_IMPORT_FILTER order by IMPORT_FILTER_ID)

	-- Return the result of the function
	RETURN @Result

END

GO
ALTER AUTHORIZATION ON [dbo].[fn_GetFirstImportFilterID] TO  SCHEMA OWNER 
GO
/****** Object:  UserDefinedFunction [dbo].[fn_IsAttributeInTree]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[fn_IsAttributeInTree] 
(
	-- Add the parameters for the function here
	@ATTRIBUTE_ID int
)
RETURNS bit
AS
BEGIN
	-- Declare the return variable here
	DECLARE @tmp int;

	with AAA (ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID)
	as
	(
	select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from TB_ATTRIBUTE_SET tas
	where ATTRIBUTE_ID = @ATTRIBUTE_ID
	UNION ALL
	select tas.ATTRIBUTE_ID, tas.PARENT_ATTRIBUTE_ID from TB_ATTRIBUTE_SET tas
	inner join AAA on AAA.PARENT_ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	)
	select @tmp =  min( PARENT_ATTRIBUTE_ID) from AAA --where PARENT_ATTRIBUTE_ID = 0
	if @tmp = 0 RETURN 1
	RETURN 0
	
	

END

GO
ALTER AUTHORIZATION ON [dbo].[fn_IsAttributeInTree] TO  SCHEMA OWNER 
GO
/****** Object:  UserDefinedFunction [dbo].[fn_ItemAttributes]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_ItemAttributes]
(
	@CODING_IS_FINAL BIT,
	@SET_ID INT,
	@CONTACT_ID INT,
	@ITEM_ID BIGINT
)
RETURNS @retTable TABLE (ITEM_ATTRIBUTE_ID BIGINT, ITEM_SET_ID BIGINT, ATTRIBUTE_ID BIGINT, ADDITIONAL_TEXT NVARCHAR(MAX),
	CONTACT_ID INT, ATTRIBUTE_SET_ID BIGINT, IS_COMPLETED BIT, IS_LOCKED BIT)
AS
BEGIN

/*
Returns item attribute records depending whether we need to filter by contact_id. We filter by contact_id if:
 - The code set is not finalised and we are double-coding (coding_is_final is false).
*/

/* Has this item already been coded and finalised with this code set? */
DECLARE @CHECK_COMPLETED BIT
SET @CHECK_COMPLETED = 'False'
SELECT @CHECK_COMPLETED = IS_COMPLETED FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID

/* Returning filtered by contact id */
IF ((@CHECK_COMPLETED = 'False') AND (@CODING_IS_FINAL = 'False'))
BEGIN

INSERT INTO @retTable (ITEM_ATTRIBUTE_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT, CONTACT_ID, ATTRIBUTE_SET_ID,
	IS_COMPLETED, IS_LOCKED)
SELECT IA.ITEM_ATTRIBUTE_ID, IA.ITEM_SET_ID, IA.ATTRIBUTE_ID, IA.ADDITIONAL_TEXT, TB_ITEM_SET.CONTACT_ID, ATTRIBUTE_SET_ID,
	IS_COMPLETED, IS_LOCKED
	FROM TB_ITEM_ATTRIBUTE IA
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = IA.ITEM_SET_ID AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
		AND TB_ITEM_SET.SET_ID = @SET_ID
	WHERE IA.ITEM_ID = @ITEM_ID
END
ELSE
/* returning without taking contact_id as a filter */
BEGIN
	INSERT INTO @retTable (ITEM_ATTRIBUTE_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT, CONTACT_ID, ATTRIBUTE_SET_ID,
		IS_COMPLETED, IS_LOCKED)
	SELECT IA.ITEM_ATTRIBUTE_ID, IA.ITEM_SET_ID, IA.ATTRIBUTE_ID, IA.ADDITIONAL_TEXT, TB_ITEM_SET.CONTACT_ID, ATTRIBUTE_SET_ID,
		IS_COMPLETED, IS_LOCKED
	FROM TB_ITEM_ATTRIBUTE IA
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = IA.ITEM_SET_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
		AND TB_ITEM_SET.SET_ID = @SET_ID
	WHERE IA.ITEM_ID = @ITEM_ID
END

RETURN

END

GO
ALTER AUTHORIZATION ON [dbo].[fn_ItemAttributes] TO  SCHEMA OWNER 
GO
/****** Object:  UserDefinedFunction [dbo].[fn_ItemsSetUncoded]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_ItemsSetUncoded]
(
	@REVIEW_ID INT,
	@SET_ID INT, -- NEEDED, AS WE NEED TO KNOW WHETHER CONTACT_ID IS SIGNIFICANT (IN DOUBLE SCREENING IT IS)
	@IS_CODING_FINAL BIT, -- TRUE / FALSE
	@CONTACT_ID INT
)
RETURNS @retTable TABLE (ITEM_ID BIGINT)
AS
BEGIN

IF (@IS_CODING_FINAL = 'True')
BEGIN

INSERT INTO @retTable(ITEM_ID)
SELECT TB_ITEM_REVIEW.ITEM_ID FROM TB_ITEM_REVIEW
WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
AND NOT TB_ITEM_REVIEW.ITEM_ID IN
	(SELECT IA.ITEM_ID FROM TB_ITEM_ATTRIBUTE IA
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = IA.ITEM_SET_ID
			AND TB_ITEM_SET.SET_ID = @SET_ID)
END
ELSE
BEGIN

INSERT INTO @retTable(ITEM_ID)
SELECT TB_ITEM_REVIEW.ITEM_ID FROM TB_ITEM_REVIEW
WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
AND NOT TB_ITEM_REVIEW.ITEM_ID IN
	(SELECT IA.ITEM_ID FROM TB_ITEM_ATTRIBUTE IA
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = IA.ITEM_SET_ID 
			AND TB_ITEM_SET.SET_ID = @SET_ID
			AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID)

END

RETURN

END

GO
ALTER AUTHORIZATION ON [dbo].[fn_ItemsSetUncoded] TO  SCHEMA OWNER 
GO
/****** Object:  UserDefinedFunction [dbo].[fn_REBUILD_AUTHORS]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- De-normalising Authors function ByS --

CREATE FUNCTION  [dbo].[fn_REBUILD_AUTHORS]

(

@id bigint,
@role tinyint = 0

)

RETURNS nvarchar(max)

   

    BEGIN

        declare @res nvarchar(max)

        declare @res2 nvarchar(max)

        DECLARE cr CURSOR FAST_FORWARD FOR SELECT [LAST] + ' ' + [FIRST] + ' ' + [SECOND]

        FROM [tb_ITEM_AUTHOR]  where item_id = @id AND ROLE = @role

        ORDER BY [RANK]

        open cr

        set @res = ''

        FETCH NEXT FROM cr INTO @res2

         WHILE @@FETCH_STATUS = 0

        BEGIN

                Set @res = @res  + @res2

                FETCH NEXT FROM cr INTO @res2

                set @res = @res + '; '

                END
		CLOSE cr
	    DEALLOCATE cr

        return @res

    END;

-- END OF: De-normalising function ByS --


GO
ALTER AUTHORIZATION ON [dbo].[fn_REBUILD_AUTHORS] TO  SCHEMA OWNER 
GO
/****** Object:  UserDefinedFunction [dbo].[fn_REBUILD_ROLES]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- De-normalising review roles for contact function ByS --

CREATE FUNCTION  [dbo].[fn_REBUILD_ROLES]

(
	@REVIEW_CONTACT_ID int
)

RETURNS nvarchar(max)

   

    BEGIN

        declare @res nvarchar(max)

        declare @res2 nvarchar(max)

        DECLARE cr CURSOR FAST_FORWARD FOR SELECT ROLE_NAME

        from TB_CONTACT_REVIEW_ROLE where REVIEW_CONTACT_ID = @REVIEW_CONTACT_ID


        open cr

        set @res = ''

        FETCH NEXT FROM cr INTO @res2

         WHILE @@FETCH_STATUS = 0

        BEGIN

                Set @res = @res  + @res2

                FETCH NEXT FROM cr INTO @res2

                set @res = @res + '; '

                END

        return @res

    END;

-- END OF: De-normalising function ByS --



GO
ALTER AUTHORIZATION ON [dbo].[fn_REBUILD_ROLES] TO  SCHEMA OWNER 
GO
/****** Object:  UserDefinedFunction [dbo].[fn_Split]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_Split](@sText varchar(8000), @sDelim varchar(20) = ' ')
RETURNS @retArray TABLE (idx smallint Primary Key, value varchar(8000) COLLATE Latin1_General_CI_AS)
AS
BEGIN
DECLARE @idx smallint,
	@value varchar(8000),
	@bcontinue bit,
	@iStrike smallint,
	@iDelimlength tinyint

IF @sDelim = 'Space'
	BEGIN
	SET @sDelim = ' '
	END

SET @idx = 0
SET @sText = LTrim(RTrim(@sText))
SET @iDelimlength = DATALENGTH(@sDelim)
SET @bcontinue = 1

IF NOT ((@iDelimlength = 0) or (@sDelim = 'Empty'))
	BEGIN
	WHILE @bcontinue = 1
		BEGIN

--If you can find the delimiter in the text, retrieve the first element and
--insert it with its index into the return table.
 
		IF CHARINDEX(@sDelim, @sText)>0
			BEGIN
			SET @value = SUBSTRING(@sText,1, CHARINDEX(@sDelim,@sText)-1)
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, @value)
				END
			
--Trim the element and its delimiter from the front of the string.
			--Increment the index and loop.
SET @iStrike = DATALENGTH(@value) + @iDelimlength
			SET @idx = @idx + 1
			SET @sText = LTrim(Right(@sText,DATALENGTH(@sText) - @iStrike))
		
			END
		ELSE
			BEGIN
--If you can’t find the delimiter in the text, @sText is the last value in
--@retArray.
 SET @value = @sText
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, @value)
				END
			--Exit the WHILE loop.
SET @bcontinue = 0
			END
		END
	END
ELSE
	BEGIN
	WHILE @bcontinue=1
		BEGIN
		--If the delimiter is an empty string, check for remaining text
		--instead of a delimiter. Insert the first character into the
		--retArray table. Trim the character from the front of the string.
--Increment the index and loop.
		IF DATALENGTH(@sText)>1
			BEGIN
			SET @value = SUBSTRING(@sText,1,1)
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, @value)
				END
			SET @idx = @idx+1
			SET @sText = SUBSTRING(@sText,2,DATALENGTH(@sText)-1)
			
			END
		ELSE
			BEGIN
			--One character remains.
			--Insert the character, and exit the WHILE loop.
			INSERT @retArray (idx, value)
			VALUES (@idx, @sText)
			SET @bcontinue = 0	
			END
	END

END

RETURN
END

GO
ALTER AUTHORIZATION ON [dbo].[fn_Split] TO  SCHEMA OWNER 
GO
/****** Object:  UserDefinedFunction [dbo].[fn_Split_int]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_Split_int](@sText varchar(max), @sDelim varchar(20) = ' ')
RETURNS @retArray TABLE (idx int Primary Key, value bigint)
AS
BEGIN
DECLARE @idx int,
	@value varchar(8000),
	@bcontinue bit,
	@iStrike smallint,
	@iDelimlength tinyint
IF @sDelim = 'Space'
	BEGIN
	SET @sDelim = ' '
	END
SET @idx = 0
SET @sText = LTrim(RTrim(@sText))
SET @iDelimlength = DATALENGTH(@sDelim)
SET @bcontinue = 1
IF NOT ((@iDelimlength = 0) or (@sDelim = 'Empty'))
	BEGIN
	WHILE @bcontinue = 1
		BEGIN
--If you can find the delimiter in the text, retrieve the first element and
--insert it with its index into the return table.
 
		IF CHARINDEX(@sDelim, @sText)>0
			BEGIN
			SET @value = SUBSTRING(@sText,1, CHARINDEX(@sDelim,@sText)-1)
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, CAST(@value as bigint))
				END
			
--Trim the element and its delimiter from the front of the string.
			--Increment the index and loop.
SET @iStrike = DATALENGTH(@value) + @iDelimlength
			SET @idx = @idx + 1
			SET @sText = LTrim(Right(@sText,DATALENGTH(@sText) - @iStrike))
		
			END
		ELSE
			BEGIN
--If you can't find the delimiter in the text, @sText is the last value in
--@retArray.
 SET @value = @sText
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, CAST(@value AS bigint))
				END
			--Exit the WHILE loop.
SET @bcontinue = 0
			END
		END
	END
ELSE
	BEGIN
	WHILE @bcontinue=1
		BEGIN
		--If the delimiter is an empty string, check for remaining text
		--instead of a delimiter. Insert the first character into the
		--retArray table. Trim the character from the front of the string.
--Increment the index and loop.
		IF DATALENGTH(@sText)>1
			BEGIN
			SET @value = SUBSTRING(@sText,1,1)
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, CAST(@value as bigint))
				END
			SET @idx = @idx+1
			SET @sText = SUBSTRING(@sText,2,DATALENGTH(@sText)-1)
			
			END
		ELSE
			BEGIN
			--One character remains.
			--Insert the character, and exit the WHILE loop.
			INSERT @retArray (idx, value)
			VALUES (@idx, CAST(@sText as bigint))
			SET @bcontinue = 0	
			END
	END
END
RETURN
END



GO
ALTER AUTHORIZATION ON [dbo].[fn_Split_int] TO  SCHEMA OWNER 
GO
GRANT SELECT ON [dbo].[fn_Split_int] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
/****** Object:  Table [dbo].[TB_ATTRIBUTE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ATTRIBUTE](
	[ATTRIBUTE_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[CONTACT_ID] [int] NULL,
	[ATTRIBUTE_NAME] [nvarchar](255) NULL,
	[ATTRIBUTE_DESC] [nvarchar](2000) NULL,
	[OLD_ATTRIBUTE_ID] [nvarchar](50) NULL,
	[ORIGINAL_ATTRIBUTE_ID] [bigint] NULL,
 CONSTRAINT [PK_TB_ATTRIBUTE] PRIMARY KEY CLUSTERED 
(
	[ATTRIBUTE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ATTRIBUTE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ATTRIBUTE_SET]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ATTRIBUTE_SET](
	[ATTRIBUTE_SET_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ATTRIBUTE_ID] [bigint] NULL,
	[SET_ID] [int] NULL,
	[PARENT_ATTRIBUTE_ID] [bigint] NULL,
	[ATTRIBUTE_TYPE_ID] [int] NULL,
	[ATTRIBUTE_SET_DESC] [nvarchar](max) NULL,
	[ATTRIBUTE_ORDER] [int] NULL,
 CONSTRAINT [PK_TB_ATTRIBUTE_SET] PRIMARY KEY CLUSTERED 
(
	[ATTRIBUTE_SET_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ATTRIBUTE_SET] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ATTRIBUTE_TYPE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TB_ATTRIBUTE_TYPE](
	[ATTRIBUTE_TYPE_ID] [int] NOT NULL,
	[ATTRIBUTE_TYPE] [nvarchar](255) NULL,
	[OLD_IS_ANSWER] [varchar](1) NULL,
 CONSTRAINT [PK_TB_ATTRIBUTE_TYPE] PRIMARY KEY CLUSTERED 
(
	[ATTRIBUTE_TYPE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER AUTHORIZATION ON [dbo].[TB_ATTRIBUTE_TYPE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_CLASSIFIER_ITEM_TEMP]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_CLASSIFIER_ITEM_TEMP](
	[REVIEW_ID] [int] NOT NULL,
	[ITEM_ID] [bigint] NOT NULL,
	[SCORE] [decimal](18, 18) NULL,
 CONSTRAINT [PK_TB_CLASSIFIER_ITEM_TEMP] PRIMARY KEY CLUSTERED 
(
	[REVIEW_ID] ASC,
	[ITEM_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_CLASSIFIER_ITEM_TEMP] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[tb_CLASSIFIER_MODEL]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_CLASSIFIER_MODEL](
	[MODEL_ID] [int] IDENTITY(1,1) NOT NULL,
	[MODEL_TITLE] [nvarchar](1000) NULL,
	[CONTACT_ID] [int] NOT NULL,
	[REVIEW_ID] [int] NOT NULL,
	[ATTRIBUTE_ID_ON] [bigint] NULL,
	[ATTRIBUTE_ID_NOT_ON] [bigint] NULL,
	[ACCURACY] [decimal](18, 18) NULL,
	[AUC] [decimal](18, 18) NULL,
	[PRECISION] [decimal](18, 18) NULL,
	[RECALL] [decimal](18, 18) NULL,
	[TIME_STARTED] [datetime] NULL,
	[TIME_ENDED] [datetime] NULL,
 CONSTRAINT [PK_tb_CLASSIFIER_MODEL] PRIMARY KEY CLUSTERED 
(
	[MODEL_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[tb_CLASSIFIER_MODEL] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[tb_COMPARISON]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_COMPARISON](
	[COMPARISON_ID] [int] IDENTITY(1,1) NOT NULL,
	[REVIEW_ID] [int] NULL,
	[IN_GROUP_ATTRIBUTE_ID] [bigint] NULL,
	[SET_ID] [int] NULL,
	[COMPARISON_DATE] [date] NULL,
	[CONTACT_ID1] [int] NULL,
	[CONTACT_ID2] [int] NULL,
	[CONTACT_ID3] [int] NULL,
	[IS_SCREENING] [bit] NOT NULL CONSTRAINT [DF_tb_COMPARISON_IS_SCREENING]  DEFAULT ((0)),
 CONSTRAINT [PK_tb_COMPARISON] PRIMARY KEY CLUSTERED 
(
	[COMPARISON_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[tb_COMPARISON] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[tb_COMPARISON_ITEM_ATTRIBUTE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tb_COMPARISON_ITEM_ATTRIBUTE](
	[COMPARISON_ITEM_ATTRIBUTE_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[COMPARISON_ID] [int] NULL,
	[ITEM_ID] [bigint] NULL,
	[ATTRIBUTE_ID] [bigint] NULL,
	[ADDITIONAL_TEXT] [nvarchar](max) NULL,
	[CONTACT_ID] [int] NULL,
	[SET_ID] [int] NULL,
	[IS_INCLUDED] [bit] NULL,
 CONSTRAINT [PK_tb_ITEM_ATTRIBUTE_COMPARISON] PRIMARY KEY CLUSTERED 
(
	[COMPARISON_ITEM_ATTRIBUTE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[tb_COMPARISON_ITEM_ATTRIBUTE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_CONTACT]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TB_CONTACT](
	[CONTACT_ID] [int] IDENTITY(1,1) NOT NULL,
	[old_contact_id] [nvarchar](50) NULL,
	[CONTACT_NAME] [nvarchar](255) NULL,
	[USERNAME] [varchar](50) NULL,
	[PASSWORD] [varchar](50) NULL,
	[LAST_LOGIN] [datetime] NULL,
	[DATE_CREATED] [datetime] NULL,
	[EMAIL] [nvarchar](500) NULL,
	[EXPIRY_DATE] [date] NULL CONSTRAINT [DF_TB_CONTACT_EXPIRY_DATE]  DEFAULT (dateadd(month,(1),getdate())),
	[MONTHS_CREDIT] [smallint] NOT NULL CONSTRAINT [DF_TB_CONTACT_MONTHS_CREDIT]  DEFAULT ((0)),
	[CREATOR_ID] [int] NULL,
	[TYPE] [nchar](12) NOT NULL CONSTRAINT [DF_TB_CONTACT_TYPE]  DEFAULT ('Professional'),
	[IS_SITE_ADMIN] [bit] NOT NULL CONSTRAINT [DF_TB_CONTACT_IS_SITE_ADMIN]  DEFAULT ((0)),
	[DESCRIPTION] [nvarchar](1000) NULL,
	[SEND_NEWSLETTER] [bit] NOT NULL CONSTRAINT [DF_TB_CONTACT_SEND_NEWSLETTER]  DEFAULT ((0)),
	[FLAVOUR] [char](20) NULL,
	[PWASHED] [binary](20) NULL,
	[ARCHIE_ID] [varchar](32) NULL,
	[ARCHIE_ACCESS_TOKEN] [varchar](64) NULL,
	[ARCHIE_TOKEN_VALID_UNTIL] [datetime2](1) NULL,
	[ARCHIE_REFRESH_TOKEN] [varchar](64) NULL,
	[LAST_ARCHIE_CODE] [varchar](64) NULL,
	[LAST_ARCHIE_STATE] [varchar](10) NULL,
 CONSTRAINT [PK_tb_CONTACT] PRIMARY KEY CLUSTERED 
(
	[CONTACT_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER AUTHORIZATION ON [dbo].[TB_CONTACT] TO  SCHEMA OWNER 
GO

/****** Object:  Table [dbo].[TB_CONTACT_REVIEW_ROLE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_CONTACT_REVIEW_ROLE](
	[REVIEW_CONTACT_ID] [int] NOT NULL,
	[ROLE_NAME] [nvarchar](50) NOT NULL,
 CONSTRAINT [IX_TB_CONTACT_REVIEW_ROLE] UNIQUE NONCLUSTERED 
(
	[REVIEW_CONTACT_ID] ASC,
	[ROLE_NAME] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_CONTACT_REVIEW_ROLE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_DIAGRAM]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_DIAGRAM](
	[DIAGRAM_ID] [int] IDENTITY(1,1) NOT NULL,
	[REVIEW_ID] [int] NULL,
	[DIAGRAM_NAME] [nvarchar](255) NULL,
	[DIAGRAM_DETAIL] [nvarchar](max) NULL,
 CONSTRAINT [PK_TB_DIAGRAM] PRIMARY KEY CLUSTERED 
(
	[DIAGRAM_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_DIAGRAM] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_IMPORT_FILTER]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_IMPORT_FILTER](
	[IMPORT_FILTER_ID] [int] IDENTITY(1,1) NOT NULL,
	[IMPORT_FILTER_NAME] [nvarchar](60) NOT NULL,
	[IMPORT_FILTER_NOTES] [nvarchar](4000) NULL,
	[STARTOFNEWREC] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__START__7B47DA60]  DEFAULT ('\\M\\w'),
	[TYPEFIELD] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__TYPEF__7C3BFE99]  DEFAULT ('\\M\\w'),
	[STARTOFNEWFIELD] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__START__7D3022D2]  DEFAULT ('\\M\\w'),
	[TITLE] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__TITLE__7E24470B]  DEFAULT ('\\M\\w'),
	[PTITLE] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__PTITL__7F186B44]  DEFAULT ('\\M\\w'),
	[SHORTTITLE] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__SHORT__000C8F7D]  DEFAULT ('\\M\\w'),
	[DATE] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT___DATE__0100B3B6]  DEFAULT ('\\M\\w'),
	[MONTH] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__MONTH__01F4D7EF]  DEFAULT ('\\M\\w'),
	[AUTHOR] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__AUTHO__02E8FC28]  DEFAULT ('\\M\\w'),
	[PARENTAUTHOR] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__PAREN__03DD2061]  DEFAULT ('\\M\\w'),
	[STANDARDN] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__STAND__04D1449A]  DEFAULT ('\\M\\w'),
	[CITY] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT___CITY__05C568D3]  DEFAULT ('\\M\\w'),
	[PUBLISHER] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__PUBLI__06B98D0C]  DEFAULT ('\\M\\w'),
	[INSTITUTION] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__INSTI__07ADB145]  DEFAULT ('\\M\\w'),
	[VOLUME] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__VOLUM__08A1D57E]  DEFAULT ('\\M\\w'),
	[ISSUE] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__ISSUE__0995F9B7]  DEFAULT ('\\M\\w'),
	[EDITION] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__EDITI__0A8A1DF0]  DEFAULT ('\\M\\w'),
	[STARTPAGE] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__START__0B7E4229]  DEFAULT ('\\M\\w'),
	[ENDPAGE] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__ENDPA__0C726662]  DEFAULT ('\\M\\w'),
	[PAGES] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__PAGES__0D668A9B]  DEFAULT ('\\M\\w'),
	[AVAILABILITY] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__AVAIL__0E5AAED4]  DEFAULT ('\\M\\w'),
	[URL] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT_F__URL__0F4ED30D]  DEFAULT ('\\M\\w'),
	[ABSTRACT] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__ABSTR__1042F746]  DEFAULT ('\\M\\w'),
	[OLD_ITEM_ID] [nvarchar](400) NOT NULL CONSTRAINT [DF_TB_IMPORT_FILTER_OLDITEMID]  DEFAULT ('\\M\\w'),
	[NOTES] [nvarchar](400) NOT NULL CONSTRAINT [DF__TB_IMPORT__NOTES__11371B7F]  DEFAULT ('\\M\\w'),
	[DEFAULTTYPECODE] [tinyint] NOT NULL CONSTRAINT [DF__TB_IMPORT__DEFAU__122B3FB8]  DEFAULT ((12)),
	[DOI] [nvarchar](400) NOT NULL CONSTRAINT [DF_TB_IMPORT_FILTER_DOI]  DEFAULT (''),
	[KEYWORDS] [nvarchar](400) NOT NULL CONSTRAINT [DF_TB_IMPORT_FILTER_KEYWORDS]  DEFAULT (''),
 CONSTRAINT [PK_TB_IMPORT_FILTER] PRIMARY KEY CLUSTERED 
(
	[IMPORT_FILTER_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_TB_IMPORT_FILTER] UNIQUE NONCLUSTERED 
(
	[IMPORT_FILTER_NAME] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_IMPORT_FILTER] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_IMPORT_FILTER_TYPE_MAP]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_IMPORT_FILTER_TYPE_MAP](
	[IMPORT_FILTER_TYPE_MAP_ID] [int] IDENTITY(1,1) NOT NULL,
	[IMPORT_FILTER_ID] [int] NOT NULL,
	[TYPE_CODE] [tinyint] NOT NULL,
	[TYPE_REGEX] [nvarchar](400) NOT NULL,
 CONSTRAINT [PK_TB_IMPORT_FILTER_TYPE_MAP] PRIMARY KEY CLUSTERED 
(
	[IMPORT_FILTER_TYPE_MAP_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_IMPORT_FILTER_TYPE_MAP] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_IMPORT_FILTER_TYPE_RULE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_IMPORT_FILTER_TYPE_RULE](
	[TB_IMPORT_FILTER_TYPE_RULE_ID] [int] IDENTITY(1,1) NOT NULL,
	[IMPORT_FILTER_ID] [int] NOT NULL,
	[RULE_NAME] [nvarchar](50) NOT NULL,
	[RULE_REGEX] [nvarchar](400) NULL,
	[TYPE_CODE] [tinyint] NOT NULL,
 CONSTRAINT [PK_TB_IMPORT_FILTER_TYPE_RULE] PRIMARY KEY CLUSTERED 
(
	[TB_IMPORT_FILTER_TYPE_RULE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_IMPORT_FILTER_TYPE_RULE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TB_ITEM](
	[ITEM_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[TYPE_ID] [int] NOT NULL CONSTRAINT [DF_tb_ITEM_TYPE_ID]  DEFAULT ((0)),
	[TITLE] [nvarchar](4000) NULL CONSTRAINT [DF_tb_ITEM_TITLE]  DEFAULT (''),
	[PARENT_TITLE] [nvarchar](4000) NULL CONSTRAINT [DF_tb_ITEM_PARENT_TITLE]  DEFAULT (''),
	[SHORT_TITLE] [nvarchar](70) NULL CONSTRAINT [DF_tb_ITEM_SHORT_TITLE]  DEFAULT (''),
	[DATE_CREATED] [datetime] NULL,
	[CREATED_BY] [nvarchar](50) NULL CONSTRAINT [DF_tb_ITEM_CREATED_BY]  DEFAULT (''),
	[DATE_EDITED] [datetime] NULL,
	[EDITED_BY] [nvarchar](50) NULL,
	[YEAR] [nchar](4) NULL CONSTRAINT [DF_tb_ITEM_YEAR]  DEFAULT ((0)),
	[MONTH] [varchar](10) NULL CONSTRAINT [DF_tb_ITEM_MONTH]  DEFAULT ((0)),
	[STANDARD_NUMBER] [nvarchar](255) NULL CONSTRAINT [DF_tb_ITEM_STANDARD_NUMBER]  DEFAULT (''),
	[CITY] [nvarchar](100) NULL CONSTRAINT [DF_tb_ITEM_CITY]  DEFAULT (''),
	[COUNTRY] [nvarchar](100) NULL CONSTRAINT [DF_tb_ITEM_COUNTRY]  DEFAULT (''),
	[PUBLISHER] [nvarchar](1000) NULL CONSTRAINT [DF_tb_ITEM_PUBLISHER]  DEFAULT (''),
	[INSTITUTION] [nvarchar](1000) NULL CONSTRAINT [DF_tb_ITEM_INSTITUTION]  DEFAULT (''),
	[VOLUME] [nvarchar](56) NULL CONSTRAINT [DF_tb_ITEM_VOLUME]  DEFAULT (''),
	[PAGES] [nvarchar](50) NULL CONSTRAINT [DF_tb_ITEM_PAGES]  DEFAULT (''),
	[EDITION] [nvarchar](200) NULL CONSTRAINT [DF_tb_ITEM_EDITION]  DEFAULT (''),
	[ISSUE] [nvarchar](100) NULL CONSTRAINT [DF_tb_ITEM_ISSUE]  DEFAULT (''),
	[IS_LOCAL] [bit] NULL CONSTRAINT [DF_tb_ITEM_IS_LOCAL]  DEFAULT ((0)),
	[AVAILABILITY] [nvarchar](255) NULL CONSTRAINT [DF_tb_ITEM_AVAILABILITY]  DEFAULT (''),
	[URL] [nvarchar](500) NULL CONSTRAINT [DF_tb_ITEM_URL]  DEFAULT (''),
	[MASTER_ITEM_ID] [bigint] NULL,
	[OLD_ITEM_ID] [nvarchar](50) NULL,
	[ABSTRACT] [nvarchar](max) NULL CONSTRAINT [DF_tb_ITEM_ABSTRACT]  DEFAULT (''),
	[COMMENTS] [nvarchar](max) NULL CONSTRAINT [DF_tb_ITEM_COMMENTS]  DEFAULT (''),
	[DOI] [nvarchar](500) NULL CONSTRAINT [DF_TB_ITEM_DOI]  DEFAULT (''),
	[KEYWORDS] [nvarchar](max) NULL CONSTRAINT [DF_TB_ITEM_KEYWORDS]  DEFAULT (''),
 CONSTRAINT [PK_tb_ITEM] PRIMARY KEY CLUSTERED 
(
	[ITEM_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM] TO  SCHEMA OWNER 
GO


GRANT SELECT ON [dbo].[TB_ITEM] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
/****** Object:  Table [dbo].[TB_ITEM_ATTRIBUTE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_ATTRIBUTE](
	[ITEM_ATTRIBUTE_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ITEM_ID] [bigint] NULL,
	[ITEM_SET_ID] [bigint] NULL,
	[ATTRIBUTE_ID] [bigint] NULL,
	[ADDITIONAL_TEXT] [nvarchar](max) NULL,
 CONSTRAINT [PK_TB_ITEM_ATTRIBUTE] PRIMARY KEY CLUSTERED 
(
	[ITEM_ATTRIBUTE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_ATTRIBUTE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_ATTRIBUTE_PDF]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TB_ITEM_ATTRIBUTE_PDF](
	[ITEM_ATTRIBUTE_PDF_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ITEM_DOCUMENT_ID] [bigint] NOT NULL,
	[ITEM_ATTRIBUTE_ID] [bigint] NOT NULL,
	[PAGE] [int] NOT NULL,
	[SHAPE_TEXT] [varchar](max) NOT NULL,
	[SELECTION_INTERVALS] [varchar](max) NOT NULL,
	[SELECTION_TEXTS] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_TB_ITEM_ATTRIBUTE_PDF] PRIMARY KEY CLUSTERED 
(
	[ITEM_ATTRIBUTE_PDF_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_ATTRIBUTE_PDF] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_ATTRIBUTE_TEXT]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_ATTRIBUTE_TEXT](
	[ITEM_ATTRIBUTE_TEXT_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ITEM_DOCUMENT_ID] [bigint] NULL,
	[ITEM_ATTRIBUTE_ID] [bigint] NULL,
	[TEXT_FROM] [int] NULL,
	[TEXT_TO] [int] NULL,
 CONSTRAINT [PK_TB_ITEM_ATTRIBUTE_TEXT] PRIMARY KEY CLUSTERED 
(
	[ITEM_ATTRIBUTE_TEXT_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_ATTRIBUTE_TEXT] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_AUTHOR]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_AUTHOR](
	[ITEM_AUTHOR_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ITEM_ID] [bigint] NOT NULL,
	[LAST] [nvarchar](50) NOT NULL,
	[FIRST] [nvarchar](50) NOT NULL CONSTRAINT [DF_tb_ITEM_AUTHORS_First]  DEFAULT (''),
	[SECOND] [nvarchar](50) NOT NULL CONSTRAINT [DF_tb_ITEM_AUTHORS_Second]  DEFAULT (''),
	[ROLE] [tinyint] NOT NULL CONSTRAINT [DF_tb_ITEM_AUTHOR_ORIGIN]  DEFAULT ((0)),
	[RANK] [smallint] NOT NULL,
 CONSTRAINT [PK_tb_ITEM_AUTHOR] PRIMARY KEY CLUSTERED 
(
	[ITEM_AUTHOR_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_tb_ITEM_AUTHORS] UNIQUE NONCLUSTERED 
(
	[ITEM_ID] ASC,
	[LAST] ASC,
	[FIRST] ASC,
	[SECOND] ASC,
	[ROLE] ASC,
	[RANK] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_AUTHOR] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_DOCUMENT]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_DOCUMENT](
	[ITEM_DOCUMENT_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ITEM_ID] [bigint] NULL,
	[DOCUMENT_TITLE] [nvarchar](255) NULL,
	[DOCUMENT_BINARY] [image] NULL,
	[DOCUMENT_EXTENSION] [nvarchar](5) NULL,
	[DOCUMENT_TEXT] [nvarchar](max) NULL,
	[OLD_EXTRACT_ATTR_IDENTITY] [bigint] NULL,
	[DOCUMENT_FREE_NOTES] [nvarchar](max) NULL,
 CONSTRAINT [PK_tb_ITEM_DOCUMENT] PRIMARY KEY CLUSTERED 
(
	[ITEM_DOCUMENT_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_DOCUMENT] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_DUPLICATE_GROUP]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_DUPLICATE_GROUP](
	[ITEM_DUPLICATE_GROUP_ID] [int] IDENTITY(1,1) NOT NULL,
	[MASTER_MEMBER_ID] [int] NULL CONSTRAINT [DF_TB_ITEM_DUPLICATE_GROUP_MASTER_MEMBER_ID]  DEFAULT (NULL),
	[REVIEW_ID] [int] NOT NULL,
	[ORIGINAL_ITEM_ID] [bigint] NOT NULL,
 CONSTRAINT [PK_TB_ITEM_DUPLICATE_GROUP] PRIMARY KEY CLUSTERED 
(
	[ITEM_DUPLICATE_GROUP_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_DUPLICATE_GROUP] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS](
	[GROUP_MEMBER_ID] [int] IDENTITY(1,1) NOT NULL,
	[ITEM_DUPLICATE_GROUP_ID] [int] NOT NULL,
	[ITEM_REVIEW_ID] [bigint] NOT NULL,
	[SCORE] [float] NULL,
	[IS_CHECKED] [bit] NOT NULL CONSTRAINT [DF_TB_ITEM_DUPLICATE_GROUP_MEMBERS_IS_CHECKED]  DEFAULT ((0)),
	[IS_DUPLICATE] [bit] NULL,
 CONSTRAINT [PK_TB_ITEM_DUPLICATE_GROUP_MEMBERS] PRIMARY KEY CLUSTERED 
(
	[GROUP_MEMBER_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS] TO  SCHEMA OWNER 
GO



GRANT SELECT ON [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
/****** Object:  Table [dbo].[TB_ITEM_DUPLICATES]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_DUPLICATES](
	[ITEM_DUPLICATES_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ITEM_ID_IN] [bigint] NOT NULL,
	[_SCORE] [float] NOT NULL,
	[ITEM_ID_OUT] [bigint] NOT NULL,
	[REVIEW_ID] [int] NOT NULL,
	[IS_CHECKED] [bit] NULL,
	[IS_DUPLICATE] [bit] NULL,
 CONSTRAINT [PK_TB_ITEM_DUPLICATES] PRIMARY KEY CLUSTERED 
(
	[ITEM_DUPLICATES_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_DUPLICATES] TO  SCHEMA OWNER 
GO









/****** Object:  Table [dbo].[TB_ITEM_DUPLICATES_TEMP]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_DUPLICATES_TEMP](
	[ITEM_DUPLICATES_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[_key_in] [int] NULL,
	[_SCORE] [float] NULL,
	[_key_out] [int] NULL,
	[ITEM_ID] [bigint] NULL,
	[REVIEW_ID] [int] NULL,
	[EXTR_UI] [uniqueidentifier] NOT NULL,
	[DESTINATION] [int] NULL,
 CONSTRAINT [PK_TB_ITEM_DUPLICATES_TEMP] PRIMARY KEY CLUSTERED 
(
	[ITEM_DUPLICATES_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_DUPLICATES_TEMP] TO  SCHEMA OWNER 
GO









GRANT DELETE ON [dbo].[TB_ITEM_DUPLICATES_TEMP] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
GRANT INSERT ON [dbo].[TB_ITEM_DUPLICATES_TEMP] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
GRANT SELECT ON [dbo].[TB_ITEM_DUPLICATES_TEMP] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
GRANT UPDATE ON [dbo].[TB_ITEM_DUPLICATES_TEMP] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
/****** Object:  Table [dbo].[TB_ITEM_LINK]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_LINK](
	[ITEM_LINK_ID] [int] IDENTITY(1,1) NOT NULL,
	[ITEM_ID_PRIMARY] [bigint] NOT NULL,
	[ITEM_ID_SECONDARY] [bigint] NOT NULL,
	[LINK_DESCRIPTION] [nvarchar](255) NULL,
 CONSTRAINT [PK_TB_ITEM_LINK] PRIMARY KEY CLUSTERED 
(
	[ITEM_LINK_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_LINK] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_OUTCOME]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_OUTCOME](
	[OUTCOME_ID] [int] IDENTITY(1,1) NOT NULL,
	[ITEM_SET_ID] [bigint] NULL,
	[OUTCOME_TYPE_ID] [int] NULL,
	[ITEM_ATTRIBUTE_ID_INTERVENTION] [bigint] NULL,
	[ITEM_ATTRIBUTE_ID_CONTROL] [bigint] NULL,
	[ITEM_ATTRIBUTE_ID_OUTCOME] [bigint] NULL,
	[OUTCOME_TITLE] [nvarchar](255) NULL,
	[DATA1] [float] NULL,
	[DATA2] [float] NULL,
	[DATA3] [float] NULL,
	[DATA4] [float] NULL,
	[DATA5] [float] NULL,
	[DATA6] [float] NULL,
	[DATA7] [float] NULL,
	[DATA8] [float] NULL,
	[DATA9] [float] NULL,
	[DATA10] [float] NULL,
	[DATA11] [float] NULL,
	[DATA12] [float] NULL,
	[DATA13] [float] NULL,
	[DATA14] [float] NULL,
	[OUTCOME_DESCRIPTION] [nvarchar](4000) NULL,
 CONSTRAINT [PK_TB_OUTCOME] PRIMARY KEY CLUSTERED 
(
	[OUTCOME_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_OUTCOME] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_OUTCOME_ATTRIBUTE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_OUTCOME_ATTRIBUTE](
	[ITEM_OUTCOME_ATTRIBUTE_ID] [int] IDENTITY(1,1) NOT NULL,
	[OUTCOME_ID] [int] NULL,
	[ATTRIBUTE_ID] [bigint] NULL,
	[ADDITIONAL_TEXT] [nvarchar](max) NULL,
 CONSTRAINT [PK_TB_ITEM_OUTCOME_ATTRIBUTE] PRIMARY KEY CLUSTERED 
(
	[ITEM_OUTCOME_ATTRIBUTE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_OUTCOME_ATTRIBUTE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_REVIEW]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_REVIEW](
	[ITEM_REVIEW_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ITEM_ID] [bigint] NOT NULL,
	[REVIEW_ID] [int] NOT NULL,
	[IS_INCLUDED] [bit] NULL,
	[MASTER_ITEM_ID] [bigint] NULL,
	[IS_DELETED] [bit] NULL,
 CONSTRAINT [PK_tb_ITEM_REVIEW] PRIMARY KEY CLUSTERED 
(
	[ITEM_REVIEW_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_REVIEW] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_SET]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_SET](
	[ITEM_SET_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ITEM_ID] [bigint] NULL,
	[SET_ID] [int] NULL,
	[IS_COMPLETED] [bit] NULL,
	[CONTACT_ID] [int] NULL,
	[IS_LOCKED] [bit] NULL,
 CONSTRAINT [PK_TB_ITEM_SET] PRIMARY KEY CLUSTERED 
(
	[ITEM_SET_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_SET] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_SET_rec]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_SET_rec](
	[ITEM_SET_ID] [bigint] NOT NULL,
	[ITEM_ID] [bigint] NULL,
	[SET_ID] [int] NULL,
	[IS_COMPLETED] [bit] NULL,
	[CONTACT_ID] [int] NULL,
	[IS_LOCKED] [bit] NULL,
 CONSTRAINT [PK_TB_ITEM_SET_rec] PRIMARY KEY CLUSTERED 
(
	[ITEM_SET_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_SET_rec] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_SOURCE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_SOURCE](
	[ITEM_SOURCE_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ITEM_ID] [bigint] NOT NULL,
	[SOURCE_ID] [int] NOT NULL,
 CONSTRAINT [PK_TB_ITEM_SOURCE] PRIMARY KEY CLUSTERED 
(
	[ITEM_SOURCE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_SOURCE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_TERM]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_TERM](
	[ITEM_TERM_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ITEM_ID] [bigint] NULL,
	[REVIEW_ID] [int] NULL,
	[TERM] [nvarchar](255) NULL,
	[SCORE] [float] NULL,
 CONSTRAINT [PK_TB_ITEM_TERM] PRIMARY KEY CLUSTERED 
(
	[ITEM_TERM_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_TERM] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_TERM_DICTIONARY]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_TERM_DICTIONARY](
	[ITEM_TERM_DICTIONARY_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[TERM] [nvarchar](128) NULL,
	[SCORE] [float] NULL,
	[UI] [uniqueidentifier] NULL,
 CONSTRAINT [PK_TB_ITEM_TERM_DICTIONARY] PRIMARY KEY CLUSTERED 
(
	[ITEM_TERM_DICTIONARY_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_TERM_DICTIONARY] TO  SCHEMA OWNER 
GO









GRANT DELETE ON [dbo].[TB_ITEM_TERM_DICTIONARY] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
GRANT INSERT ON [dbo].[TB_ITEM_TERM_DICTIONARY] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
GRANT SELECT ON [dbo].[TB_ITEM_TERM_DICTIONARY] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
GRANT UPDATE ON [dbo].[TB_ITEM_TERM_DICTIONARY] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
/****** Object:  Table [dbo].[TB_ITEM_TERM_TEXT]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_TERM_TEXT](
	[ITEM_ID] [bigint] NULL,
	[REVIEW_ID] [int] NULL,
	[ITEM_TEXT] [nvarchar](4000) NULL
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_TERM_TEXT] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_TERM_VECTORS]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_TERM_VECTORS](
	[ITEM_ID] [bigint] NULL,
	[VECTORS] [nvarchar](max) NULL,
	[REVIEW_ID] [int] NULL,
	[RELEVANT_TERMS] [int] NULL,
	[IRRELEVANT_TERMS] [int] NULL,
	[PROBABILITY] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_TERM_VECTORS] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_ITEM_TYPE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_ITEM_TYPE](
	[TYPE_ID] [int] NOT NULL,
	[TYPE_NAME] [nvarchar](50) NOT NULL,
	[DESCRIPTION] [nvarchar](200) NOT NULL CONSTRAINT [DF_tb_ITEM_TYPE_DESCRIPTION]  DEFAULT (''),
	[NOTES] [nvarchar](200) NOT NULL CONSTRAINT [DF_tb_ITEM_TYPE_NOTES]  DEFAULT (''),
 CONSTRAINT [PK_tb_ITEM_TYPE] PRIMARY KEY CLUSTERED 
(
	[TYPE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_ITEM_TYPE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_META_ANALYSIS]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_META_ANALYSIS](
	[META_ANALYSIS_ID] [int] IDENTITY(1,1) NOT NULL,
	[META_ANALYSIS_TITLE] [nvarchar](255) NULL,
	[CONTACT_ID] [int] NULL,
	[REVIEW_ID] [int] NULL,
	[ATTRIBUTE_ID] [bigint] NULL,
	[SET_ID] [int] NULL,
	[ATTRIBUTE_ID_INTERVENTION] [bigint] NULL,
	[ATTRIBUTE_ID_CONTROL] [bigint] NULL,
	[ATTRIBUTE_ID_OUTCOME] [bigint] NULL,
	[META_ANALYSIS_TYPE_ID] [int] NULL,
	[ATTRIBUTE_ID_ANSWER] [nvarchar](max) NULL,
	[ATTRIBUTE_ID_QUESTION] [nvarchar](max) NULL,
	[GRID_SETTINGS] [nvarchar](max) NULL,
 CONSTRAINT [PK_TB_META_ANALYSIS] PRIMARY KEY CLUSTERED 
(
	[META_ANALYSIS_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_META_ANALYSIS] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_META_ANALYSIS_OUTCOME]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_META_ANALYSIS_OUTCOME](
	[META_ANALYSIS_OUTCOME_ID] [int] IDENTITY(1,1) NOT NULL,
	[META_ANALYSIS_ID] [int] NULL,
	[OUTCOME_ID] [int] NULL,
 CONSTRAINT [PK_TB_META_ANALYSIS_OUTCOME] PRIMARY KEY CLUSTERED 
(
	[META_ANALYSIS_OUTCOME_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_META_ANALYSIS_OUTCOME] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_META_ANALYSIS_TYPE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_META_ANALYSIS_TYPE](
	[META_ANALYSIS_TYPE_ID] [int] NOT NULL,
	[META_ANALYSIS_TYPE_TITLE] [nvarchar](255) NULL,
 CONSTRAINT [PK_TB_META_ANALYSIS_TYPE] PRIMARY KEY CLUSTERED 
(
	[META_ANALYSIS_TYPE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_META_ANALYSIS_TYPE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_OUTCOME_TYPE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_OUTCOME_TYPE](
	[OUTCOME_TYPE_ID] [int] NOT NULL,
	[OUTCOME_TYPE_NAME] [nvarchar](255) NULL,
 CONSTRAINT [PK_TB_OUTCOME_TYPE] PRIMARY KEY CLUSTERED 
(
	[OUTCOME_TYPE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_OUTCOME_TYPE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_REPORT]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_REPORT](
	[REPORT_ID] [int] IDENTITY(1,1) NOT NULL,
	[REVIEW_ID] [int] NULL,
	[CONTACT_ID] [int] NULL,
	[NAME] [nvarchar](255) NULL,
	[REPORT_TYPE] [nvarchar](10) NULL,
 CONSTRAINT [PK_TB_REPORT] PRIMARY KEY CLUSTERED 
(
	[REPORT_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_REPORT] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_REPORT_COLUMN]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_REPORT_COLUMN](
	[REPORT_COLUMN_ID] [int] IDENTITY(1,1) NOT NULL,
	[REPORT_ID] [int] NULL,
	[REPORT_COLUMN_NAME] [nvarchar](255) NULL,
	[COLUMN_ORDER] [int] NULL,
 CONSTRAINT [PK_TB_REPORT_COLUMN] PRIMARY KEY CLUSTERED 
(
	[REPORT_COLUMN_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_REPORT_COLUMN] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_REPORT_COLUMN_CODE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_REPORT_COLUMN_CODE](
	[REPORT_COLUMN_CODE_ID] [int] IDENTITY(1,1) NOT NULL,
	[REPORT_ID] [int] NULL,
	[REPORT_COLUMN_ID] [int] NULL,
	[CODE_ORDER] [int] NULL,
	[SET_ID] [int] NULL,
	[ATTRIBUTE_ID] [bigint] NULL,
	[PARENT_ATTRIBUTE_ID] [bigint] NULL,
	[PARENT_ATTRIBUTE_TEXT] [nvarchar](255) NULL,
	[USER_DEF_TEXT] [nvarchar](255) NULL,
	[DISPLAY_CODE] [bit] NULL,
	[DISPLAY_ADDITIONAL_TEXT] [bit] NULL,
	[DISPLAY_CODED_TEXT] [bit] NULL,
 CONSTRAINT [PK_TB_REPORT_COLUMN_CODE] PRIMARY KEY CLUSTERED 
(
	[REPORT_COLUMN_CODE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_REPORT_COLUMN_CODE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_REVIEW]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TB_REVIEW](
	[REVIEW_ID] [int] IDENTITY(1,1) NOT NULL,
	[REVIEW_NAME] [nvarchar](1000) NULL,
	[OLD_REVIEW_ID] [nvarchar](50) NULL,
	[OLD_REVIEW_GROUP_ID] [nvarchar](50) NULL,
	[DATE_CREATED] [datetime] NULL,
	[EXPIRY_DATE] [date] NULL,
	[MONTHS_CREDIT] [smallint] NOT NULL CONSTRAINT [DF_TB_REVIEW_MONTHS_CREDIT]  DEFAULT ((0)),
	[FUNDER_ID] [int] NULL,
	[REVIEW_NUMBER] [nvarchar](50) NULL,
	[SHOW_SCREENING] [bit] NOT NULL CONSTRAINT [DF_TB_REVIEW_SHOW_SCREENING]  DEFAULT ((0)),
	[ALLOW_REVIEWER_TERMS] [bit] NOT NULL CONSTRAINT [DF_TB_REVIEW_ALLOW_REVIEWER_TERMS]  DEFAULT ((0)),
	[ALLOW_CLUSTERED_SEARCH] [bit] NOT NULL CONSTRAINT [DF_TB_REVIEW_ALLOW_CLUSTERED_SEARCH]  DEFAULT ((0)),
	[SCREENING_CODE_SET_ID] [int] NULL,
	[ARCHIE_ID] [char](18) NULL,
	[ARCHIE_CD] [nchar](8) NULL,
	[IS_CHECKEDOUT_HERE] [bit] NULL,
	[CHECKED_OUT_BY] [int] NULL,
	[BL_ACCOUNT_CODE] [nvarchar](50) NULL,
	[BL_AUTH_CODE] [nvarchar](50) NULL,
	[BL_TX] [nvarchar](50) NULL,
	[BL_CC_ACCOUNT_CODE] [nvarchar](50) NULL,
	[BL_CC_AUTH_CODE] [nvarchar](50) NULL,
	[BL_CC_TX] [nvarchar](50) NULL,
	[SCREENING_MODE] [nvarchar](10) NULL,
	[SCREENING_WHAT_ATTRIBUTE_ID] [bigint] NULL,
	[SCREENING_N_PEOPLE] [int] NULL,
	[SCREENING_RECONCILLIATION] [nvarchar](10) NULL,
	[SCREENING_AUTO_EXCLUDE] [bit] NULL,
	[SCREENING_MODEL_RUNNING] [bit] NULL,
	[SCREENING_INDEXED] [bit] NULL,
 CONSTRAINT [PK_tb_REVIEW] PRIMARY KEY CLUSTERED 
(
	[REVIEW_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER AUTHORIZATION ON [dbo].[TB_REVIEW] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_REVIEW_CONTACT]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_REVIEW_CONTACT](
	[REVIEW_CONTACT_ID] [int] IDENTITY(1,1) NOT NULL,
	[REVIEW_ID] [int] NOT NULL,
	[CONTACT_ID] [int] NOT NULL,
	[old_review_id] [nvarchar](50) NULL,
	[old_contact_id] [nvarchar](50) NULL,
 CONSTRAINT [PK_TB_REVIEW_CONTACT] PRIMARY KEY CLUSTERED 
(
	[REVIEW_CONTACT_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_REVIEW_CONTACT] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_REVIEW_ROLE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_REVIEW_ROLE](
	[ROLE_NAME] [nvarchar](50) NOT NULL,
	[ROLE_DESCR] [nvarchar](2000) NOT NULL,
 CONSTRAINT [PK_TB_REVIEW_ROLE] PRIMARY KEY CLUSTERED 
(
	[ROLE_NAME] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_REVIEW_ROLE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_REVIEW_SET]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_REVIEW_SET](
	[REVIEW_SET_ID] [int] IDENTITY(1,1) NOT NULL,
	[REVIEW_ID] [int] NULL,
	[SET_ID] [int] NULL,
	[ALLOW_CODING_EDITS] [bit] NULL,
	[CODING_IS_FINAL] [bit] NULL,
	[SET_ORDER] [int] NULL,
 CONSTRAINT [PK_TB_REVIEW_SET] PRIMARY KEY CLUSTERED 
(
	[REVIEW_SET_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_REVIEW_SET] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_SCREENING_ML_TEMP]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_SCREENING_ML_TEMP](
	[SCORE] [decimal](10, 10) NULL,
	[ITEM_ID] [bigint] NOT NULL,
	[REVIEW_ID] [int] NOT NULL,
 CONSTRAINT [PK_TB_SCREENING_ML_TEMP] PRIMARY KEY CLUSTERED 
(
	[ITEM_ID] ASC,
	[REVIEW_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_SCREENING_ML_TEMP] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_SEARCH]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_SEARCH](
	[SEARCH_ID] [int] IDENTITY(1,1) NOT NULL,
	[REVIEW_ID] [int] NULL,
	[CONTACT_ID] [int] NULL,
	[SEARCH_TITLE] [nvarchar](4000) NULL,
	[SEARCH_NO] [int] NULL,
	[ANSWERS] [nvarchar](4000) NULL,
	[HITS_NO] [int] NULL,
	[SEARCH_DATE] [datetime] NULL,
 CONSTRAINT [PK_TB_SEARCH] PRIMARY KEY CLUSTERED 
(
	[SEARCH_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_SEARCH] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_SEARCH_ITEM]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_SEARCH_ITEM](
	[SEARCH_ITEM_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ITEM_ID] [bigint] NULL,
	[SEARCH_ID] [int] NULL,
	[ITEM_RANK] [int] NULL,
 CONSTRAINT [PK_TB_SEARCH_ITEM] PRIMARY KEY CLUSTERED 
(
	[SEARCH_ITEM_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_SEARCH_ITEM] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_SET]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_SET](
	[SET_ID] [int] IDENTITY(1,1) NOT NULL,
	[SET_TYPE_ID] [int] NOT NULL,
	[SET_NAME] [nvarchar](255) NULL,
	[OLD_GUIDELINE_ID] [nvarchar](50) NULL,
	[SET_DESCRIPTION] [nvarchar](2000) NULL,
	[ORIGINAL_SET_ID] [int] NULL,
 CONSTRAINT [PK_TB_SETS] PRIMARY KEY CLUSTERED 
(
	[SET_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_SET] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_SET_TYPE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_SET_TYPE](
	[SET_TYPE_ID] [int] IDENTITY(1,1) NOT NULL,
	[SET_TYPE] [nvarchar](50) NOT NULL,
	[SET_DESCRIPTION] [nvarchar](400) NOT NULL CONSTRAINT [DF_TB_SET_TYPE_SET_DESCRIPTION]  DEFAULT (''),
	[ALLOW_COMPARISON] [bit] NOT NULL CONSTRAINT [DF_TB_SET_TYPE_ALLOW_COMPARISON]  DEFAULT ((1)),
	[MAX_DEPTH] [int] NOT NULL CONSTRAINT [DF_TB_SET_TYPE_MAX_DEPTH]  DEFAULT ((0)),
	[ACCEPTS_RANDOM_ALLOCATIONS] [bit] NOT NULL CONSTRAINT [DF_TB_SET_TYPE_ACCEPTS_RANDOM_ALLOCATIONS]  DEFAULT ((1)),
 CONSTRAINT [PK_TB_SET_TYPE] PRIMARY KEY CLUSTERED 
(
	[SET_TYPE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_SET_TYPE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE](
	[SET_TYPE_ATTRIBUTE_TYPE_ID] [int] IDENTITY(1,1) NOT NULL,
	[SET_TYPE_ID] [int] NOT NULL,
	[ATTRIBUTE_TYPE_ID] [int] NOT NULL,
 CONSTRAINT [PK_TB_SET_TYPE_ATTRIBUTE_TYPE] PRIMARY KEY CLUSTERED 
(
	[SET_TYPE_ATTRIBUTE_TYPE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_SET_TYPE_PASTE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_SET_TYPE_PASTE](
	[SET_TYPE_PASTE_ID] [int] IDENTITY(1,1) NOT NULL,
	[DEST_SET_TYPE_ID] [int] NOT NULL,
	[SRC_SET_TYPE_ID] [int] NOT NULL,
 CONSTRAINT [PK_TB_SET_TYPE_PASTE] PRIMARY KEY CLUSTERED 
(
	[SET_TYPE_PASTE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_SET_TYPE_PASTE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_SITE_LIC]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_SITE_LIC](
	[SITE_LIC_ID] [int] IDENTITY(1,1) NOT NULL,
	[SITE_LIC_NAME] [nvarchar](50) NOT NULL,
	[COMPANY_NAME] [nvarchar](50) NOT NULL,
	[COMPANY_ADDRESS] [nvarchar](500) NOT NULL,
	[TELEPHONE] [nvarchar](50) NULL,
	[NOTES] [nvarchar](2000) NULL,
	[EXPIRY_DATE] [datetime] NULL,
	[CREATOR_ID] [int] NOT NULL,
	[DATE_CREATED] [datetime] NOT NULL CONSTRAINT [DF_TB_SITE_LIC_DATE_CREATED]  DEFAULT (getdate()),
	[BL_ACCOUNT_CODE] [nvarchar](50) NULL,
	[BL_AUTH_CODE] [nvarchar](50) NULL,
	[BL_TX] [nvarchar](50) NULL,
	[BL_CC_ACCOUNT_CODE] [nvarchar](50) NULL,
	[BL_CC_AUTH_CODE] [nvarchar](50) NULL,
	[BL_CC_TX] [nvarchar](50) NULL,
 CONSTRAINT [PK_TB_SITE_LIC] PRIMARY KEY CLUSTERED 
(
	[SITE_LIC_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_SITE_LIC] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_SITE_LIC_CONTACT]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_SITE_LIC_CONTACT](
	[SITE_LIC_ID] [int] NOT NULL,
	[CONTACT_ID] [int] NOT NULL,
 CONSTRAINT [UK_TB_SITE_LIC_CONTACT] UNIQUE NONCLUSTERED 
(
	[CONTACT_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_SITE_LIC_CONTACT] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_SITE_LIC_REVIEW]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_SITE_LIC_REVIEW](
	[SITE_LIC_ID] [int] NOT NULL,
	[SITE_LIC_DETAILS_ID] [int] NOT NULL,
	[REVIEW_ID] [int] NOT NULL,
	[DATE_ADDED] [datetime] NOT NULL CONSTRAINT [DF_TB_SITE_LIC_REVIEW_DATE_ADDED]  DEFAULT (getdate()),
	[ADDED_BY] [int] NOT NULL
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_SITE_LIC_REVIEW] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_SOURCE]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_SOURCE](
	[SOURCE_ID] [int] IDENTITY(1,1) NOT NULL,
	[SOURCE_NAME] [nvarchar](255) NOT NULL,
	[REVIEW_ID] [int] NULL,
	[IS_DELETED] [bit] NOT NULL CONSTRAINT [DF_TB_SOURCE_IS_DELETED]  DEFAULT ((0)),
	[DATE_OF_SEARCH] [date] NOT NULL CONSTRAINT [DF_TB_SOURCE_DATE_OF_SEARCH]  DEFAULT (getdate()),
	[DATE_OF_IMPORT] [date] NOT NULL CONSTRAINT [DF_TB_SOURCE_DATE_OF_IMPORT]  DEFAULT (getdate()),
	[SOURCE_DATABASE] [nvarchar](200) NOT NULL CONSTRAINT [DF_TB_SOURCE_SOURCE_DATABASE]  DEFAULT (''),
	[SEARCH_DESCRIPTION] [nvarchar](4000) NOT NULL CONSTRAINT [DF_TB_SOURCE_SEARCH_DESCRIPTION]  DEFAULT (''),
	[SEARCH_STRING] [nvarchar](max) NOT NULL CONSTRAINT [DF_TB_SOURCE_SEARCH_STRING]  DEFAULT (''),
	[NOTES] [nvarchar](4000) NOT NULL CONSTRAINT [DF_TB_SOURCE_NOTES]  DEFAULT (''),
	[IMPORT_FILTER_ID] [int] NULL CONSTRAINT [DF_TB_SOURCE_IMPORT_FILTER_ID]  DEFAULT ([dbo].[fn_GetFirstImportFilterID]()),
 CONSTRAINT [PK_TB_SOURCE] PRIMARY KEY CLUSTERED 
(
	[SOURCE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_SOURCE] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_TEMP_ITEM]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_TEMP_ITEM](
	[N_ITEM_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ISWEB] [tinyint] NOT NULL,
	[TYPE_ID] [tinyint] NOT NULL,
	[mediumToEd] [int] NOT NULL,
	[titleSource] [nvarchar](50) NOT NULL,
	[ParentTSource] [nvarchar](50) NOT NULL,
	[PubData] [nvarchar](50) NOT NULL,
	[INSTITUTION] [nvarchar](255) NOT NULL,
	[ITEM_ID] [nvarchar](50) NOT NULL,
	[ITEM] [nvarchar](255) NULL,
	[ITEM_DESCRIPTION] [ntext] NULL,
	[TYPE_CODE] [nvarchar](50) NOT NULL,
	[DATE_CREATED] [datetime] NULL,
	[DATE_EDITED] [datetime] NULL,
	[CREATED_BY] [nvarchar](50) NULL,
	[EDITED_BY] [nvarchar](50) NULL,
	[AUTHOR_ANALYTIC] [nvarchar](255) NULL,
	[TITLE_ANALYTIC] [nvarchar](500) NULL,
	[MEDIUM] [nvarchar](255) NULL,
	[AUTHOR_MONO] [nvarchar](255) NULL,
	[AUTHOR_ROLE] [nvarchar](255) NULL,
	[TITLE_MONO] [nvarchar](500) NULL,
	[JOURNAL] [nvarchar](500) NULL,
	[TRANS_NEWS_TITLE] [nvarchar](500) NULL,
	[PLACE] [nvarchar](255) NULL,
	[EDITION] [nvarchar](255) NULL,
	[PLACE_OF_PUB] [nvarchar](255) NULL,
	[PUBLISHER] [nvarchar](255) NULL,
	[DATE_OF_PUB] [nvarchar](255) NULL,
	[VOLUME] [nvarchar](255) NULL,
	[REPORT_ID] [nvarchar](255) NULL,
	[ISSUE] [nvarchar](255) NULL,
	[PAGES] [nvarchar](255) NULL,
	[EXTENT_OF_WORK] [nvarchar](255) NULL,
	[CONTACT_DETAILS] [nvarchar](255) NULL,
	[SERIES_TITLE] [nvarchar](255) NULL,
	[SERIES_VOLUME] [nvarchar](255) NULL,
	[SERIES_ISSUE] [nvarchar](255) NULL,
	[WRITTEN_LANGUAGE] [nvarchar](255) NULL,
	[AVAILABILITY] [nvarchar](255) NULL,
	[LOCATION] [nvarchar](255) NULL,
	[EPIC_NO] [nvarchar](255) NULL,
	[ISSN] [nvarchar](255) NULL,
	[ISBN] [nvarchar](255) NULL,
	[NOTES] [ntext] NULL,
	[ABSTRACT] [ntext] NULL,
	[AGE_RANGE] [nvarchar](255) NULL,
	[SHORT_TITLE] [nvarchar](70) NULL,
	[ITEM_IDENTITY] [bigint] NOT NULL,
	[CONFIDENTIAL_CONTACT_ID] [nvarchar](50) NULL,
	[IMPORTED_REF_ID] [nvarchar](50) NULL,
	[ER4_ITEM_ID] [bigint] NULL,
 CONSTRAINT [PK_tb_N_ITEM] PRIMARY KEY CLUSTERED 
(
	[N_ITEM_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_TEMP_ITEM] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_TERM_EXTR_T_MAP]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_TERM_EXTR_T_MAP](
	[ITEM_ID] [bigint] NOT NULL,
	[EXTR_UI] [uniqueidentifier] NOT NULL
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_TERM_EXTR_T_MAP] TO  SCHEMA OWNER 
GO







GRANT DELETE ON [dbo].[TB_TERM_EXTR_T_MAP] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
GRANT INSERT ON [dbo].[TB_TERM_EXTR_T_MAP] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
GRANT SELECT ON [dbo].[TB_TERM_EXTR_T_MAP] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
GRANT UPDATE ON [dbo].[TB_TERM_EXTR_T_MAP] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
/****** Object:  Table [dbo].[TB_TERMINE_LOG]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_TERMINE_LOG](
	[TERMINE_LOG_ID] [int] IDENTITY(1,1) NOT NULL,
	[CONTACT_ID] [int] NOT NULL,
	[REVIEW_ID] [int] NOT NULL,
	[BYTES] [int] NOT NULL,
	[DATE] [datetime] NOT NULL CONSTRAINT [DF_TB_TERMINE_LOG_DATE]  DEFAULT (getdate()),
	[SUCCESS] [bit] NOT NULL,
	[N_OF_TERMS] [int] NOT NULL,
	[ERROR] [nvarchar](2000) NULL,
 CONSTRAINT [PK_TB_TERMINE_LOG] PRIMARY KEY CLUSTERED 
(
	[TERMINE_LOG_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_TERMINE_LOG] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_TRAINING]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_TRAINING](
	[TRAINING_ID] [int] IDENTITY(1,1) NOT NULL,
	[CONTACT_ID] [int] NULL,
	[REVIEW_ID] [int] NULL,
	[TIME_STARTED] [datetime] NULL,
	[TIME_ENDED] [datetime] NULL,
	[ITERATION] [int] NULL,
	[N_TRAINING_INC] [int] NULL,
	[N_TRAINING_EXC] [int] NULL,
	[C] [float] NULL,
	[TRUE_POSITIVES] [int] NULL,
	[FALSE_POSITIVES] [int] NULL,
	[TRUE_NEGATIVES] [int] NULL,
	[FALSE_NEGATIVES] [int] NULL,
 CONSTRAINT [PK_TB_TRAINING] PRIMARY KEY CLUSTERED 
(
	[TRAINING_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_TRAINING] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_TRAINING_ITEM]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_TRAINING_ITEM](
	[TRAINING_ITEM_ID] [int] IDENTITY(1,1) NOT NULL,
	[ITEM_ID] [bigint] NULL,
	[RANK] [int] NULL,
	[TRAINING_ID] [int] NULL,
	[CONTACT_ID_CODING] [int] NULL,
	[WHEN_LOCKED] [datetime] NULL,
	[SCORE] [decimal](10, 10) NULL,
 CONSTRAINT [PK_TB_TRAINING_ITEM] PRIMARY KEY CLUSTERED 
(
	[TRAINING_ITEM_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_TRAINING_ITEM] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_TRAINING_REVIEWER_TERM]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_TRAINING_REVIEWER_TERM](
	[TRAINING_REVIEWER_TERM_ID] [int] IDENTITY(1,1) NOT NULL,
	[REVIEWER_TERM] [nvarchar](255) NULL,
	[INCLUDED] [bit] NULL,
	[ITEM_TERM_DICTIONARY_ID] [bigint] NULL,
	[REVIEW_ID] [int] NULL,
 CONSTRAINT [PK_TB_TRAINING_REVIEWER_TERM] PRIMARY KEY CLUSTERED 
(
	[TRAINING_REVIEWER_TERM_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_TRAINING_REVIEWER_TERM] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_TRAINING_SCREENING_CRITERIA]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_TRAINING_SCREENING_CRITERIA](
	[TRAINING_SCREENING_CRITERIA_ID] [int] IDENTITY(1,1) NOT NULL,
	[SET_ID] [int] NULL,
	[ATTRIBUTE_ID] [bigint] NULL,
	[INCLUDED] [bit] NULL,
	[REVIEW_ID] [int] NULL,
 CONSTRAINT [PK_TB_TRAINING_SCREENING_CRITERIA] PRIMARY KEY CLUSTERED 
(
	[TRAINING_SCREENING_CRITERIA_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_TRAINING_SCREENING_CRITERIA] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_TRAINING_STOPWORDS]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_TRAINING_STOPWORDS](
	[stopword] [nvarchar](255) NULL
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_TRAINING_STOPWORDS] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_UNASSIGNED_ARCHIE_KEYS]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TB_UNASSIGNED_ARCHIE_KEYS](
	[ARCHIE_ID] [varchar](32) NOT NULL,
	[ARCHIE_ACCESS_TOKEN] [varchar](64) NOT NULL,
	[ARCHIE_TOKEN_VALID_UNTIL] [datetime2](1) NOT NULL,
	[ARCHIE_REFRESH_TOKEN] [varchar](64) NOT NULL,
	[LAST_ARCHIE_CODE] [varchar](64) NOT NULL,
	[LAST_ARCHIE_STATE] [varchar](10) NOT NULL,
 CONSTRAINT [PK_TB_UNASSIGNED_ARCHIE_KEYS] PRIMARY KEY CLUSTERED 
(
	[ARCHIE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER AUTHORIZATION ON [dbo].[TB_UNASSIGNED_ARCHIE_KEYS] TO  SCHEMA OWNER 
GO
/****** Object:  Table [dbo].[TB_WORK_ALLOCATION]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_WORK_ALLOCATION](
	[WORK_ALLOCATION_ID] [int] IDENTITY(1,1) NOT NULL,
	[CONTACT_ID] [int] NULL,
	[REVIEW_ID] [int] NULL,
	[SET_ID] [int] NULL,
	[ATTRIBUTE_ID] [bigint] NULL,
 CONSTRAINT [PK_TB_WORK_ALLOCATION] PRIMARY KEY CLUSTERED 
(
	[WORK_ALLOCATION_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER AUTHORIZATION ON [dbo].[TB_WORK_ALLOCATION] TO  SCHEMA OWNER 
GO
/****** Object:  Index [IDX_TB_ATTRIBUTE_SET_ATTRIBUTE_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IDX_TB_ATTRIBUTE_SET_ATTRIBUTE_ID] ON [dbo].[TB_ATTRIBUTE_SET]
(
	[ATTRIBUTE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ATTRIBUTE_PARENT_ID_ATTRIBUTE_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ATTRIBUTE_PARENT_ID_ATTRIBUTE_ID] ON [dbo].[TB_ATTRIBUTE_SET]
(
	[PARENT_ATTRIBUTE_ID] ASC,
	[ATTRIBUTE_ID] ASC,
	[ATTRIBUTE_SET_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ATTRIBUTE_SET]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ATTRIBUTE_SET] ON [dbo].[TB_ATTRIBUTE_SET]
(
	[SET_ID] ASC,
	[ATTRIBUTE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_TB_ITEM_SHORT_TITLE]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_SHORT_TITLE] ON [dbo].[TB_ITEM]
(
	[SHORT_TITLE] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_ATTRIBUTE_ATTRIBUTE_ID_ITEM_ID_ITEM_SET_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_ATTRIBUTE_ATTRIBUTE_ID_ITEM_ID_ITEM_SET_ID] ON [dbo].[TB_ITEM_ATTRIBUTE]
(
	[ATTRIBUTE_ID] ASC
)
INCLUDE ( 	[ITEM_ID],
	[ITEM_SET_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_ATTRIBUTE_ITEM_ID_ITEM_ATTR_ID_ATTR_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_ATTRIBUTE_ITEM_ID_ITEM_ATTR_ID_ATTR_ID] ON [dbo].[TB_ITEM_ATTRIBUTE]
(
	[ITEM_ID] ASC
)
INCLUDE ( 	[ITEM_ATTRIBUTE_ID],
	[ATTRIBUTE_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_ATTRIBUTE_ITEM_SET_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_ATTRIBUTE_ITEM_SET_ID] ON [dbo].[TB_ITEM_ATTRIBUTE]
(
	[ITEM_SET_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_ATTRIBUTE_PDF_IA_PG]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_ATTRIBUTE_PDF_IA_PG] ON [dbo].[TB_ITEM_ATTRIBUTE_PDF]
(
	[ITEM_ATTRIBUTE_ID] ASC,
	[PAGE] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_DOCUMENT_ITEM_ID_ITEM_DOCUMENT_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_DOCUMENT_ITEM_ID_ITEM_DOCUMENT_ID] ON [dbo].[TB_ITEM_DOCUMENT]
(
	[ITEM_ID] ASC
)
INCLUDE ( 	[ITEM_DOCUMENT_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [<Name of Missing Index, sysname,>]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [<Name of Missing Index, sysname,>] ON [dbo].[TB_ITEM_DUPLICATE_GROUP]
(
	[REVIEW_ID] ASC
)
INCLUDE ( 	[ITEM_DUPLICATE_GROUP_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_ORIGINAL_ITEM_ID_ITEM_DUPLICATE_GROUP_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_ORIGINAL_ITEM_ID_ITEM_DUPLICATE_GROUP_ID] ON [dbo].[TB_ITEM_DUPLICATE_GROUP]
(
	[ORIGINAL_ITEM_ID] ASC
)
INCLUDE ( 	[ITEM_DUPLICATE_GROUP_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_REVIEW_ID_ITEM_DUPLICATE_GROUP_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_REVIEW_ID_ITEM_DUPLICATE_GROUP_ID] ON [dbo].[TB_ITEM_DUPLICATE_GROUP]
(
	[REVIEW_ID] ASC
)
INCLUDE ( 	[ITEM_DUPLICATE_GROUP_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_ITEM_DUPLICATE_GROUP_ID_ITEM_REVIEW_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_ITEM_DUPLICATE_GROUP_ID_ITEM_REVIEW_ID] ON [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS]
(
	[ITEM_DUPLICATE_GROUP_ID] ASC
)
INCLUDE ( 	[ITEM_REVIEW_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_DUPLICATE_GROUP_MEMBERS_ITEM_REVIEW_ID_ITEM_DUPLICATE_GROUP_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_DUPLICATE_GROUP_MEMBERS_ITEM_REVIEW_ID_ITEM_DUPLICATE_GROUP_ID] ON [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS]
(
	[ITEM_REVIEW_ID] ASC
)
INCLUDE ( 	[ITEM_DUPLICATE_GROUP_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_DUPLICATES]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_DUPLICATES] ON [dbo].[TB_ITEM_DUPLICATES]
(
	[ITEM_ID_IN] ASC,
	[ITEM_ID_OUT] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_DUPLICATES_TEMP]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_DUPLICATES_TEMP] ON [dbo].[TB_ITEM_DUPLICATES_TEMP]
(
	[ITEM_ID] ASC,
	[REVIEW_ID] ASC,
	[EXTR_UI] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_DUPLICATES_TEMP_EXTR_UI_DESTINATION]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_DUPLICATES_TEMP_EXTR_UI_DESTINATION] ON [dbo].[TB_ITEM_DUPLICATES_TEMP]
(
	[EXTR_UI] ASC,
	[DESTINATION] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_DUPLICATES_TEMP_ITEM_DUPLICATES_ID_DESTINATION]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_DUPLICATES_TEMP_ITEM_DUPLICATES_ID_DESTINATION] ON [dbo].[TB_ITEM_DUPLICATES_TEMP]
(
	[_key_out] ASC
)
INCLUDE ( 	[ITEM_DUPLICATES_ID],
	[DESTINATION]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_DUPLICATES_TEMP_ITEM_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_DUPLICATES_TEMP_ITEM_ID] ON [dbo].[TB_ITEM_DUPLICATES_TEMP]
(
	[ITEM_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_DUPLICATES_TEMP_REVIEW_ID_EXTR_UI]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_DUPLICATES_TEMP_REVIEW_ID_EXTR_UI] ON [dbo].[TB_ITEM_DUPLICATES_TEMP]
(
	[REVIEW_ID] ASC
)
INCLUDE ( 	[EXTR_UI]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_OUTCOME_ITEM_SET_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_OUTCOME_ITEM_SET_ID] ON [dbo].[TB_ITEM_OUTCOME]
(
	[ITEM_SET_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_OUTCOME_ATTRIBUTE_OUTCOME_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_OUTCOME_ATTRIBUTE_OUTCOME_ID] ON [dbo].[TB_ITEM_OUTCOME_ATTRIBUTE]
(
	[OUTCOME_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_ITEM_REVIEW_onREVIEW_ID_MASTER_ITEM_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_ITEM_REVIEW_onREVIEW_ID_MASTER_ITEM_ID] ON [dbo].[TB_ITEM_REVIEW]
(
	[REVIEW_ID] ASC
)
INCLUDE ( 	[ITEM_ID],
	[MASTER_ITEM_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_REVIEW]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_REVIEW] ON [dbo].[TB_ITEM_REVIEW]
(
	[MASTER_ITEM_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_REVIEW_ITEM_ID_IS_DELETED]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_REVIEW_ITEM_ID_IS_DELETED] ON [dbo].[TB_ITEM_REVIEW]
(
	[ITEM_ID] ASC
)
INCLUDE ( 	[IS_DELETED]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_REVIEW_onITEM_ID_IS_DELETED]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_REVIEW_onITEM_ID_IS_DELETED] ON [dbo].[TB_ITEM_REVIEW]
(
	[REVIEW_ID] ASC
)
INCLUDE ( 	[ITEM_ID],
	[IS_DELETED]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_REVIEW_REVIEW_ID_IS_INCLUDED_IS_DELETED_ITEM_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_REVIEW_REVIEW_ID_IS_INCLUDED_IS_DELETED_ITEM_ID] ON [dbo].[TB_ITEM_REVIEW]
(
	[REVIEW_ID] ASC,
	[IS_INCLUDED] ASC,
	[IS_DELETED] ASC
)
INCLUDE ( 	[ITEM_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_SET]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_SET] ON [dbo].[TB_ITEM_SET]
(
	[ITEM_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_SET_IS_COMPLETED_ITEM_ID_SET_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_SET_IS_COMPLETED_ITEM_ID_SET_ID] ON [dbo].[TB_ITEM_SET]
(
	[IS_COMPLETED] ASC
)
INCLUDE ( 	[ITEM_ID],
	[SET_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_SOURCE]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_SOURCE] ON [dbo].[TB_ITEM_SOURCE]
(
	[ITEM_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_ITEM_SOURCE_ITEM_ID_SOURCE_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_ITEM_SOURCE_ITEM_ID_SOURCE_ID] ON [dbo].[TB_ITEM_SOURCE]
(
	[SOURCE_ID] ASC
)
INCLUDE ( 	[ITEM_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_REVIEW_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_REVIEW_ID] ON [dbo].[TB_REVIEW_SET]
(
	[REVIEW_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_REVIEW_SET]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_REVIEW_SET] ON [dbo].[TB_REVIEW_SET]
(
	[SET_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_SEARCH_ITEM_SEARCH_ID_ITEM_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_SEARCH_ITEM_SEARCH_ID_ITEM_ID] ON [dbo].[TB_SEARCH_ITEM]
(
	[SEARCH_ID] ASC
)
INCLUDE ( 	[ITEM_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_TB_TRAINING_ITEM_TRAINING_ID_C_ID_CODING_TRAINING_ITEM_ID_RANK]    Script Date: 6/15/2016 11:49:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_TB_TRAINING_ITEM_TRAINING_ID_C_ID_CODING_TRAINING_ITEM_ID_RANK] ON [dbo].[TB_TRAINING_ITEM]
(
	[TRAINING_ID] ASC,
	[CONTACT_ID_CODING] ASC
)
INCLUDE ( 	[TRAINING_ITEM_ID],
	[RANK]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  FullTextIndex     Script Date: 6/15/2016 11:49:45 AM ******/
CREATE FULLTEXT INDEX ON [dbo].[TB_ITEM](
[ABSTRACT] LANGUAGE 'English', 
[TITLE] LANGUAGE 'English')
KEY INDEX [PK_tb_ITEM]ON ([tb_ITEM_FTIndex], FILEGROUP [PRIMARY])
WITH (CHANGE_TRACKING = AUTO, STOPLIST = OFF)


GO
/****** Object:  FullTextIndex     Script Date: 6/15/2016 11:49:45 AM ******/
CREATE FULLTEXT INDEX ON [dbo].[TB_ITEM_ATTRIBUTE](
[ADDITIONAL_TEXT] LANGUAGE 'English')
KEY INDEX [PK_TB_ITEM_ATTRIBUTE]ON ([tb_ITEM_ATTRIBUTE_FTIndex], FILEGROUP [PRIMARY])
WITH (CHANGE_TRACKING = AUTO, STOPLIST = OFF)


GO
/****** Object:  FullTextIndex     Script Date: 6/15/2016 11:49:45 AM ******/
CREATE FULLTEXT INDEX ON [dbo].[TB_ITEM_DOCUMENT](
[DOCUMENT_TEXT] LANGUAGE 'English')
KEY INDEX [PK_tb_ITEM_DOCUMENT]ON ([tb_ITEM_DOCUMENT_FTIndex], FILEGROUP [PRIMARY])
WITH (CHANGE_TRACKING = AUTO, STOPLIST = OFF)


GO
ALTER TABLE [dbo].[TB_TEMP_ITEM] ADD  CONSTRAINT [DF__tb_TEMP_I__TYPE___49C3F6B7]  DEFAULT ((0)) FOR [TYPE_ID]
GO
ALTER TABLE [dbo].[TB_TEMP_ITEM] ADD  CONSTRAINT [DF_tb_TEMP_ITEM_HasData]  DEFAULT (N'x') FOR [PubData]
GO
ALTER TABLE [dbo].[TB_TEMP_ITEM] ADD  CONSTRAINT [DF_tb_TEMP_ITEM_INSTITUTION]  DEFAULT ('') FOR [INSTITUTION]
GO
ALTER TABLE [dbo].[TB_ATTRIBUTE]  WITH CHECK ADD  CONSTRAINT [FK_TB_ATTRIBUTE_tb_CONTACT] FOREIGN KEY([CONTACT_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO
ALTER TABLE [dbo].[TB_ATTRIBUTE] CHECK CONSTRAINT [FK_TB_ATTRIBUTE_tb_CONTACT]
GO
ALTER TABLE [dbo].[TB_ATTRIBUTE_SET]  WITH CHECK ADD  CONSTRAINT [FK_TB_ATTRIBUTE_SET_TB_ATTRIBUTE] FOREIGN KEY([ATTRIBUTE_ID])
REFERENCES [dbo].[TB_ATTRIBUTE] ([ATTRIBUTE_ID])
GO
ALTER TABLE [dbo].[TB_ATTRIBUTE_SET] CHECK CONSTRAINT [FK_TB_ATTRIBUTE_SET_TB_ATTRIBUTE]
GO
ALTER TABLE [dbo].[TB_ATTRIBUTE_SET]  WITH CHECK ADD  CONSTRAINT [FK_TB_ATTRIBUTE_SET_TB_ATTRIBUTE_TYPE] FOREIGN KEY([ATTRIBUTE_TYPE_ID])
REFERENCES [dbo].[TB_ATTRIBUTE_TYPE] ([ATTRIBUTE_TYPE_ID])
GO
ALTER TABLE [dbo].[TB_ATTRIBUTE_SET] CHECK CONSTRAINT [FK_TB_ATTRIBUTE_SET_TB_ATTRIBUTE_TYPE]
GO
ALTER TABLE [dbo].[TB_ATTRIBUTE_SET]  WITH CHECK ADD  CONSTRAINT [FK_TB_ATTRIBUTE_SET_TB_SETS] FOREIGN KEY([SET_ID])
REFERENCES [dbo].[TB_SET] ([SET_ID])
GO
ALTER TABLE [dbo].[TB_ATTRIBUTE_SET] CHECK CONSTRAINT [FK_TB_ATTRIBUTE_SET_TB_SETS]
GO
ALTER TABLE [dbo].[tb_CLASSIFIER_MODEL]  WITH CHECK ADD  CONSTRAINT [FK_tb_CLASSIFIER_MODEL_TB_CONTACT] FOREIGN KEY([CONTACT_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO
ALTER TABLE [dbo].[tb_CLASSIFIER_MODEL] CHECK CONSTRAINT [FK_tb_CLASSIFIER_MODEL_TB_CONTACT]
GO
ALTER TABLE [dbo].[tb_CLASSIFIER_MODEL]  WITH CHECK ADD  CONSTRAINT [FK_tb_CLASSIFIER_MODEL_TB_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[tb_CLASSIFIER_MODEL] CHECK CONSTRAINT [FK_tb_CLASSIFIER_MODEL_TB_REVIEW]
GO
ALTER TABLE [dbo].[tb_COMPARISON]  WITH CHECK ADD  CONSTRAINT [FK_tb_COMPARISON_TB_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[tb_COMPARISON] CHECK CONSTRAINT [FK_tb_COMPARISON_TB_REVIEW]
GO
ALTER TABLE [dbo].[tb_COMPARISON_ITEM_ATTRIBUTE]  WITH NOCHECK ADD  CONSTRAINT [FK_tb_COMPARISON_ITEM_ATTRIBUTE_TB_ATTRIBUTE] FOREIGN KEY([ATTRIBUTE_ID])
REFERENCES [dbo].[TB_ATTRIBUTE] ([ATTRIBUTE_ID])
GO
ALTER TABLE [dbo].[tb_COMPARISON_ITEM_ATTRIBUTE] NOCHECK CONSTRAINT [FK_tb_COMPARISON_ITEM_ATTRIBUTE_TB_ATTRIBUTE]
GO
ALTER TABLE [dbo].[tb_COMPARISON_ITEM_ATTRIBUTE]  WITH CHECK ADD  CONSTRAINT [FK_tb_COMPARISON_ITEM_ATTRIBUTE_TB_ITEM] FOREIGN KEY([ITEM_ID])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[tb_COMPARISON_ITEM_ATTRIBUTE] CHECK CONSTRAINT [FK_tb_COMPARISON_ITEM_ATTRIBUTE_TB_ITEM]
GO
ALTER TABLE [dbo].[tb_COMPARISON_ITEM_ATTRIBUTE]  WITH CHECK ADD  CONSTRAINT [FK_tb_ITEM_ATTRIBUTE_COMPARISON_tb_COMPARISON] FOREIGN KEY([COMPARISON_ID])
REFERENCES [dbo].[tb_COMPARISON] ([COMPARISON_ID])
GO
ALTER TABLE [dbo].[tb_COMPARISON_ITEM_ATTRIBUTE] CHECK CONSTRAINT [FK_tb_ITEM_ATTRIBUTE_COMPARISON_tb_COMPARISON]
GO
ALTER TABLE [dbo].[TB_CONTACT_REVIEW_ROLE]  WITH CHECK ADD  CONSTRAINT [FK_TB_CONTACT_REVIEW_ROLE_TB_REVIEW_CONTACT] FOREIGN KEY([REVIEW_CONTACT_ID])
REFERENCES [dbo].[TB_REVIEW_CONTACT] ([REVIEW_CONTACT_ID])
GO
ALTER TABLE [dbo].[TB_CONTACT_REVIEW_ROLE] CHECK CONSTRAINT [FK_TB_CONTACT_REVIEW_ROLE_TB_REVIEW_CONTACT]
GO
ALTER TABLE [dbo].[TB_CONTACT_REVIEW_ROLE]  WITH CHECK ADD  CONSTRAINT [FK_TB_CONTACT_REVIEW_ROLE_TB_REVIEW_ROLE] FOREIGN KEY([ROLE_NAME])
REFERENCES [dbo].[TB_REVIEW_ROLE] ([ROLE_NAME])
GO
ALTER TABLE [dbo].[TB_CONTACT_REVIEW_ROLE] CHECK CONSTRAINT [FK_TB_CONTACT_REVIEW_ROLE_TB_REVIEW_ROLE]
GO
ALTER TABLE [dbo].[TB_DIAGRAM]  WITH CHECK ADD  CONSTRAINT [FK_TB_DIAGRAM_tb_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[TB_DIAGRAM] CHECK CONSTRAINT [FK_TB_DIAGRAM_tb_REVIEW]
GO
ALTER TABLE [dbo].[TB_IMPORT_FILTER_TYPE_MAP]  WITH CHECK ADD  CONSTRAINT [FK_TB_IMPORT_FILTER_TYPE_MAP_TB_IMPORT_FILTER] FOREIGN KEY([IMPORT_FILTER_ID])
REFERENCES [dbo].[TB_IMPORT_FILTER] ([IMPORT_FILTER_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_IMPORT_FILTER_TYPE_MAP] CHECK CONSTRAINT [FK_TB_IMPORT_FILTER_TYPE_MAP_TB_IMPORT_FILTER]
GO
ALTER TABLE [dbo].[TB_IMPORT_FILTER_TYPE_RULE]  WITH CHECK ADD  CONSTRAINT [FK_TB_IMPORT_FILTER_TYPE_RULE_TB_IMPORT_FILTER] FOREIGN KEY([IMPORT_FILTER_ID])
REFERENCES [dbo].[TB_IMPORT_FILTER] ([IMPORT_FILTER_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_IMPORT_FILTER_TYPE_RULE] CHECK CONSTRAINT [FK_TB_IMPORT_FILTER_TYPE_RULE_TB_IMPORT_FILTER]
GO
ALTER TABLE [dbo].[TB_ITEM]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_TB_ITEM_TYPE] FOREIGN KEY([TYPE_ID])
REFERENCES [dbo].[TB_ITEM_TYPE] ([TYPE_ID])
GO
ALTER TABLE [dbo].[TB_ITEM] CHECK CONSTRAINT [FK_TB_ITEM_TB_ITEM_TYPE]
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_TB_ATTRIBUTE] FOREIGN KEY([ATTRIBUTE_ID])
REFERENCES [dbo].[TB_ATTRIBUTE] ([ATTRIBUTE_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE] CHECK CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_TB_ATTRIBUTE]
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_tb_ITEM] FOREIGN KEY([ITEM_ID])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE] CHECK CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_tb_ITEM]
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_TB_ITEM_SET] FOREIGN KEY([ITEM_SET_ID])
REFERENCES [dbo].[TB_ITEM_SET] ([ITEM_SET_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE] CHECK CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_TB_ITEM_SET]
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE_PDF]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_PDF_TB_ITEM_ATTRIBUTE] FOREIGN KEY([ITEM_ATTRIBUTE_ID])
REFERENCES [dbo].[TB_ITEM_ATTRIBUTE] ([ITEM_ATTRIBUTE_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE_PDF] CHECK CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_PDF_TB_ITEM_ATTRIBUTE]
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE_PDF]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_PDF_TB_ITEM_DOCUMENT] FOREIGN KEY([ITEM_DOCUMENT_ID])
REFERENCES [dbo].[TB_ITEM_DOCUMENT] ([ITEM_DOCUMENT_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE_PDF] CHECK CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_PDF_TB_ITEM_DOCUMENT]
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE_TEXT]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_TEXT_TB_ITEM_ATTRIBUTE] FOREIGN KEY([ITEM_ATTRIBUTE_ID])
REFERENCES [dbo].[TB_ITEM_ATTRIBUTE] ([ITEM_ATTRIBUTE_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE_TEXT] CHECK CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_TEXT_TB_ITEM_ATTRIBUTE]
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE_TEXT]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_TEXT_tb_ITEM_DOCUMENT] FOREIGN KEY([ITEM_DOCUMENT_ID])
REFERENCES [dbo].[TB_ITEM_DOCUMENT] ([ITEM_DOCUMENT_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_ATTRIBUTE_TEXT] CHECK CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_TEXT_tb_ITEM_DOCUMENT]
GO
ALTER TABLE [dbo].[TB_ITEM_AUTHOR]  WITH CHECK ADD  CONSTRAINT [FK_tb_ITEM_AUTHORS_tb_ITEM] FOREIGN KEY([ITEM_ID])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_AUTHOR] CHECK CONSTRAINT [FK_tb_ITEM_AUTHORS_tb_ITEM]
GO
ALTER TABLE [dbo].[TB_ITEM_DOCUMENT]  WITH CHECK ADD  CONSTRAINT [FK_tb_ITEM_DOCUMENT_tb_ITEM] FOREIGN KEY([ITEM_ID])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_DOCUMENT] CHECK CONSTRAINT [FK_tb_ITEM_DOCUMENT_tb_ITEM]
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATE_GROUP]  WITH NOCHECK ADD  CONSTRAINT [FK_TB_ITEM_DUPLICATE_GROUP_TB_ITEM] FOREIGN KEY([ORIGINAL_ITEM_ID])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATE_GROUP] NOCHECK CONSTRAINT [FK_TB_ITEM_DUPLICATE_GROUP_TB_ITEM]
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATE_GROUP]  WITH NOCHECK ADD  CONSTRAINT [FK_TB_ITEM_DUPLICATE_GROUP_TB_ITEM_DUPLICATE_GROUP_MEMBERS] FOREIGN KEY([MASTER_MEMBER_ID])
REFERENCES [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS] ([GROUP_MEMBER_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATE_GROUP] NOCHECK CONSTRAINT [FK_TB_ITEM_DUPLICATE_GROUP_TB_ITEM_DUPLICATE_GROUP_MEMBERS]
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATE_GROUP]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_DUPLICATE_GROUP_TB_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATE_GROUP] CHECK CONSTRAINT [FK_TB_ITEM_DUPLICATE_GROUP_TB_REVIEW]
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_DUPLICATE_GROUP_MEMBERS_TB_ITEM_DUPLICATE_GROUP] FOREIGN KEY([ITEM_DUPLICATE_GROUP_ID])
REFERENCES [dbo].[TB_ITEM_DUPLICATE_GROUP] ([ITEM_DUPLICATE_GROUP_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS] CHECK CONSTRAINT [FK_TB_ITEM_DUPLICATE_GROUP_MEMBERS_TB_ITEM_DUPLICATE_GROUP]
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_DUPLICATE_GROUP_MEMBERS_TB_ITEM_REVIEW] FOREIGN KEY([ITEM_REVIEW_ID])
REFERENCES [dbo].[TB_ITEM_REVIEW] ([ITEM_REVIEW_ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS] CHECK CONSTRAINT [FK_TB_ITEM_DUPLICATE_GROUP_MEMBERS_TB_ITEM_REVIEW]
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATES]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_DUPLICATES_TB_ITEM] FOREIGN KEY([ITEM_ID_IN])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATES] CHECK CONSTRAINT [FK_TB_ITEM_DUPLICATES_TB_ITEM]
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATES]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_DUPLICATES_TB_ITEM1] FOREIGN KEY([ITEM_ID_OUT])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATES] CHECK CONSTRAINT [FK_TB_ITEM_DUPLICATES_TB_ITEM1]
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATES]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_DUPLICATES_TB_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_DUPLICATES] CHECK CONSTRAINT [FK_TB_ITEM_DUPLICATES_TB_REVIEW]
GO
ALTER TABLE [dbo].[TB_ITEM_LINK]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_LINK_tb_ITEM] FOREIGN KEY([ITEM_ID_PRIMARY])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_LINK] CHECK CONSTRAINT [FK_TB_ITEM_LINK_tb_ITEM]
GO
ALTER TABLE [dbo].[TB_ITEM_LINK]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_LINK_tb_ITEM1] FOREIGN KEY([ITEM_ID_SECONDARY])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_LINK] CHECK CONSTRAINT [FK_TB_ITEM_LINK_tb_ITEM1]
GO
ALTER TABLE [dbo].[TB_ITEM_OUTCOME]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_OUTCOME_TB_ITEM_SET] FOREIGN KEY([ITEM_SET_ID])
REFERENCES [dbo].[TB_ITEM_SET] ([ITEM_SET_ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_OUTCOME] CHECK CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_OUTCOME_TB_ITEM_SET]
GO
ALTER TABLE [dbo].[TB_ITEM_OUTCOME]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_OUTCOME_TB_OUTCOME_TYPE] FOREIGN KEY([OUTCOME_TYPE_ID])
REFERENCES [dbo].[TB_OUTCOME_TYPE] ([OUTCOME_TYPE_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_OUTCOME] CHECK CONSTRAINT [FK_TB_ITEM_ATTRIBUTE_OUTCOME_TB_OUTCOME_TYPE]
GO
ALTER TABLE [dbo].[TB_ITEM_OUTCOME_ATTRIBUTE]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_OUTCOME_ATTRIBUTE_TB_ATTRIBUTE] FOREIGN KEY([ATTRIBUTE_ID])
REFERENCES [dbo].[TB_ATTRIBUTE] ([ATTRIBUTE_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_OUTCOME_ATTRIBUTE] CHECK CONSTRAINT [FK_TB_ITEM_OUTCOME_ATTRIBUTE_TB_ATTRIBUTE]
GO
ALTER TABLE [dbo].[TB_ITEM_OUTCOME_ATTRIBUTE]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_OUTCOME_ATTRIBUTE_TB_ITEM_OUTCOME] FOREIGN KEY([OUTCOME_ID])
REFERENCES [dbo].[TB_ITEM_OUTCOME] ([OUTCOME_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_OUTCOME_ATTRIBUTE] CHECK CONSTRAINT [FK_TB_ITEM_OUTCOME_ATTRIBUTE_TB_ITEM_OUTCOME]
GO
ALTER TABLE [dbo].[TB_ITEM_REVIEW]  WITH CHECK ADD  CONSTRAINT [FK_tb_ITEM_REVIEW_tb_ITEM] FOREIGN KEY([ITEM_ID])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_REVIEW] CHECK CONSTRAINT [FK_tb_ITEM_REVIEW_tb_ITEM]
GO
ALTER TABLE [dbo].[TB_ITEM_REVIEW]  WITH CHECK ADD  CONSTRAINT [FK_tb_ITEM_REVIEW_tb_ITEM1] FOREIGN KEY([MASTER_ITEM_ID])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_REVIEW] CHECK CONSTRAINT [FK_tb_ITEM_REVIEW_tb_ITEM1]
GO
ALTER TABLE [dbo].[TB_ITEM_REVIEW]  WITH CHECK ADD  CONSTRAINT [FK_tb_ITEM_REVIEW_tb_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_REVIEW] CHECK CONSTRAINT [FK_tb_ITEM_REVIEW_tb_REVIEW]
GO
ALTER TABLE [dbo].[TB_ITEM_SET]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_SET_tb_CONTACT] FOREIGN KEY([CONTACT_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_SET] CHECK CONSTRAINT [FK_TB_ITEM_SET_tb_CONTACT]
GO
ALTER TABLE [dbo].[TB_ITEM_SET]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_SET_tb_ITEM] FOREIGN KEY([ITEM_ID])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_SET] CHECK CONSTRAINT [FK_TB_ITEM_SET_tb_ITEM]
GO
ALTER TABLE [dbo].[TB_ITEM_SET]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_SET_TB_SETS] FOREIGN KEY([SET_ID])
REFERENCES [dbo].[TB_SET] ([SET_ID])
GO
ALTER TABLE [dbo].[TB_ITEM_SET] CHECK CONSTRAINT [FK_TB_ITEM_SET_TB_SETS]
GO
ALTER TABLE [dbo].[TB_ITEM_SOURCE]  WITH NOCHECK ADD  CONSTRAINT [FK_TB_ITEM_SOURCE_tb_ITEM] FOREIGN KEY([ITEM_ID])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_SOURCE] CHECK CONSTRAINT [FK_TB_ITEM_SOURCE_tb_ITEM]
GO
ALTER TABLE [dbo].[TB_ITEM_SOURCE]  WITH NOCHECK ADD  CONSTRAINT [FK_TB_ITEM_SOURCE_TB_SOURCE] FOREIGN KEY([SOURCE_ID])
REFERENCES [dbo].[TB_SOURCE] ([SOURCE_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_SOURCE] CHECK CONSTRAINT [FK_TB_ITEM_SOURCE_TB_SOURCE]
GO
ALTER TABLE [dbo].[TB_ITEM_TERM]  WITH CHECK ADD  CONSTRAINT [FK_TB_ITEM_TERM_tb_ITEM] FOREIGN KEY([ITEM_ID])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_ITEM_TERM] CHECK CONSTRAINT [FK_TB_ITEM_TERM_tb_ITEM]
GO
ALTER TABLE [dbo].[TB_META_ANALYSIS]  WITH CHECK ADD  CONSTRAINT [FK_TB_META_ANALYSIS_TB_CONTACT] FOREIGN KEY([CONTACT_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO
ALTER TABLE [dbo].[TB_META_ANALYSIS] CHECK CONSTRAINT [FK_TB_META_ANALYSIS_TB_CONTACT]
GO
ALTER TABLE [dbo].[TB_META_ANALYSIS]  WITH CHECK ADD  CONSTRAINT [FK_TB_META_ANALYSIS_TB_META_ANALYSIS_TYPE] FOREIGN KEY([META_ANALYSIS_TYPE_ID])
REFERENCES [dbo].[TB_META_ANALYSIS_TYPE] ([META_ANALYSIS_TYPE_ID])
GO
ALTER TABLE [dbo].[TB_META_ANALYSIS] CHECK CONSTRAINT [FK_TB_META_ANALYSIS_TB_META_ANALYSIS_TYPE]
GO
ALTER TABLE [dbo].[TB_META_ANALYSIS]  WITH CHECK ADD  CONSTRAINT [FK_TB_META_ANALYSIS_TB_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[TB_META_ANALYSIS] CHECK CONSTRAINT [FK_TB_META_ANALYSIS_TB_REVIEW]
GO
ALTER TABLE [dbo].[TB_META_ANALYSIS_OUTCOME]  WITH CHECK ADD  CONSTRAINT [FK_TB_META_ANALYSIS_OUTCOME_TB_ITEM_OUTCOME] FOREIGN KEY([OUTCOME_ID])
REFERENCES [dbo].[TB_ITEM_OUTCOME] ([OUTCOME_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_META_ANALYSIS_OUTCOME] CHECK CONSTRAINT [FK_TB_META_ANALYSIS_OUTCOME_TB_ITEM_OUTCOME]
GO
ALTER TABLE [dbo].[TB_META_ANALYSIS_OUTCOME]  WITH CHECK ADD  CONSTRAINT [FK_TB_META_ANALYSIS_OUTCOME_TB_META_ANALYSIS] FOREIGN KEY([META_ANALYSIS_ID])
REFERENCES [dbo].[TB_META_ANALYSIS] ([META_ANALYSIS_ID])
GO
ALTER TABLE [dbo].[TB_META_ANALYSIS_OUTCOME] CHECK CONSTRAINT [FK_TB_META_ANALYSIS_OUTCOME_TB_META_ANALYSIS]
GO
ALTER TABLE [dbo].[TB_REPORT]  WITH CHECK ADD  CONSTRAINT [FK_TB_REPORT_TB_CONTACT] FOREIGN KEY([CONTACT_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO
ALTER TABLE [dbo].[TB_REPORT] CHECK CONSTRAINT [FK_TB_REPORT_TB_CONTACT]
GO
ALTER TABLE [dbo].[TB_REPORT]  WITH CHECK ADD  CONSTRAINT [FK_TB_REPORT_TB_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[TB_REPORT] CHECK CONSTRAINT [FK_TB_REPORT_TB_REVIEW]
GO
ALTER TABLE [dbo].[TB_REPORT_COLUMN]  WITH CHECK ADD  CONSTRAINT [FK_TB_REPORT_COLUMN_TB_REPORT] FOREIGN KEY([REPORT_ID])
REFERENCES [dbo].[TB_REPORT] ([REPORT_ID])
GO
ALTER TABLE [dbo].[TB_REPORT_COLUMN] CHECK CONSTRAINT [FK_TB_REPORT_COLUMN_TB_REPORT]
GO
ALTER TABLE [dbo].[TB_REPORT_COLUMN_CODE]  WITH CHECK ADD  CONSTRAINT [FK_TB_REPORT_COLUMN_CODE_TB_REPORT] FOREIGN KEY([REPORT_ID])
REFERENCES [dbo].[TB_REPORT] ([REPORT_ID])
GO
ALTER TABLE [dbo].[TB_REPORT_COLUMN_CODE] CHECK CONSTRAINT [FK_TB_REPORT_COLUMN_CODE_TB_REPORT]
GO
ALTER TABLE [dbo].[TB_REPORT_COLUMN_CODE]  WITH CHECK ADD  CONSTRAINT [FK_TB_REPORT_COLUMN_CODE_TB_REPORT_COLUMN] FOREIGN KEY([REPORT_COLUMN_ID])
REFERENCES [dbo].[TB_REPORT_COLUMN] ([REPORT_COLUMN_ID])
GO
ALTER TABLE [dbo].[TB_REPORT_COLUMN_CODE] CHECK CONSTRAINT [FK_TB_REPORT_COLUMN_CODE_TB_REPORT_COLUMN]
GO
ALTER TABLE [dbo].[TB_REVIEW_CONTACT]  WITH CHECK ADD  CONSTRAINT [FK_TB_REVIEW_CONTACT_tb_CONTACT] FOREIGN KEY([CONTACT_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO
ALTER TABLE [dbo].[TB_REVIEW_CONTACT] CHECK CONSTRAINT [FK_TB_REVIEW_CONTACT_tb_CONTACT]
GO
ALTER TABLE [dbo].[TB_REVIEW_CONTACT]  WITH CHECK ADD  CONSTRAINT [FK_TB_REVIEW_CONTACT_tb_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[TB_REVIEW_CONTACT] CHECK CONSTRAINT [FK_TB_REVIEW_CONTACT_tb_REVIEW]
GO
ALTER TABLE [dbo].[TB_REVIEW_SET]  WITH CHECK ADD  CONSTRAINT [FK_TB_REVIEW_SET_tb_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[TB_REVIEW_SET] CHECK CONSTRAINT [FK_TB_REVIEW_SET_tb_REVIEW]
GO
ALTER TABLE [dbo].[TB_REVIEW_SET]  WITH CHECK ADD  CONSTRAINT [FK_TB_REVIEW_SET_TB_SETS] FOREIGN KEY([SET_ID])
REFERENCES [dbo].[TB_SET] ([SET_ID])
GO
ALTER TABLE [dbo].[TB_REVIEW_SET] CHECK CONSTRAINT [FK_TB_REVIEW_SET_TB_SETS]
GO
ALTER TABLE [dbo].[TB_SEARCH]  WITH CHECK ADD  CONSTRAINT [FK_TB_SEARCH_tb_CONTACT] FOREIGN KEY([CONTACT_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO
ALTER TABLE [dbo].[TB_SEARCH] CHECK CONSTRAINT [FK_TB_SEARCH_tb_CONTACT]
GO
ALTER TABLE [dbo].[TB_SEARCH]  WITH CHECK ADD  CONSTRAINT [FK_TB_SEARCH_tb_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[TB_SEARCH] CHECK CONSTRAINT [FK_TB_SEARCH_tb_REVIEW]
GO
ALTER TABLE [dbo].[TB_SEARCH_ITEM]  WITH CHECK ADD  CONSTRAINT [FK_TB_SEARCH_ITEM_tb_ITEM] FOREIGN KEY([ITEM_ID])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_SEARCH_ITEM] CHECK CONSTRAINT [FK_TB_SEARCH_ITEM_tb_ITEM]
GO
ALTER TABLE [dbo].[TB_SEARCH_ITEM]  WITH CHECK ADD  CONSTRAINT [FK_TB_SEARCH_ITEM_TB_SEARCH] FOREIGN KEY([SEARCH_ID])
REFERENCES [dbo].[TB_SEARCH] ([SEARCH_ID])
GO
ALTER TABLE [dbo].[TB_SEARCH_ITEM] CHECK CONSTRAINT [FK_TB_SEARCH_ITEM_TB_SEARCH]
GO
ALTER TABLE [dbo].[TB_SET]  WITH CHECK ADD  CONSTRAINT [FK_TB_SET_TB_SET_TYPE] FOREIGN KEY([SET_TYPE_ID])
REFERENCES [dbo].[TB_SET_TYPE] ([SET_TYPE_ID])
GO
ALTER TABLE [dbo].[TB_SET] CHECK CONSTRAINT [FK_TB_SET_TB_SET_TYPE]
GO
ALTER TABLE [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE]  WITH CHECK ADD  CONSTRAINT [FK_TB_SET_TYPE_ATTRIBUTE_TYPE_TB_ATTRIBUTE_TYPE] FOREIGN KEY([ATTRIBUTE_TYPE_ID])
REFERENCES [dbo].[TB_ATTRIBUTE_TYPE] ([ATTRIBUTE_TYPE_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] CHECK CONSTRAINT [FK_TB_SET_TYPE_ATTRIBUTE_TYPE_TB_ATTRIBUTE_TYPE]
GO
ALTER TABLE [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE]  WITH CHECK ADD  CONSTRAINT [FK_TB_SET_TYPE_ATTRIBUTE_TYPE_TB_SET_TYPE] FOREIGN KEY([SET_TYPE_ID])
REFERENCES [dbo].[TB_SET_TYPE] ([SET_TYPE_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] CHECK CONSTRAINT [FK_TB_SET_TYPE_ATTRIBUTE_TYPE_TB_SET_TYPE]
GO
ALTER TABLE [dbo].[TB_SET_TYPE_PASTE]  WITH CHECK ADD  CONSTRAINT [FK_TB_SET_TYPE_PASTE_TB_SET_TYPE] FOREIGN KEY([DEST_SET_TYPE_ID])
REFERENCES [dbo].[TB_SET_TYPE] ([SET_TYPE_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_SET_TYPE_PASTE] CHECK CONSTRAINT [FK_TB_SET_TYPE_PASTE_TB_SET_TYPE]
GO
ALTER TABLE [dbo].[TB_SET_TYPE_PASTE]  WITH CHECK ADD  CONSTRAINT [FK_TB_SET_TYPE_PASTE_TB_SET_TYPE1] FOREIGN KEY([SRC_SET_TYPE_ID])
REFERENCES [dbo].[TB_SET_TYPE] ([SET_TYPE_ID])
GO
ALTER TABLE [dbo].[TB_SET_TYPE_PASTE] CHECK CONSTRAINT [FK_TB_SET_TYPE_PASTE_TB_SET_TYPE1]
GO
ALTER TABLE [dbo].[TB_SITE_LIC]  WITH CHECK ADD  CONSTRAINT [FK_TB_SITE_LIC_TB_CONTACT] FOREIGN KEY([CREATOR_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO
ALTER TABLE [dbo].[TB_SITE_LIC] CHECK CONSTRAINT [FK_TB_SITE_LIC_TB_CONTACT]
GO
ALTER TABLE [dbo].[TB_SITE_LIC_CONTACT]  WITH CHECK ADD  CONSTRAINT [FK_TB_SITE_LIC_CONTACT_TB_CONTACT] FOREIGN KEY([CONTACT_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO
ALTER TABLE [dbo].[TB_SITE_LIC_CONTACT] CHECK CONSTRAINT [FK_TB_SITE_LIC_CONTACT_TB_CONTACT]
GO
ALTER TABLE [dbo].[TB_SITE_LIC_CONTACT]  WITH CHECK ADD  CONSTRAINT [FK_TB_SITE_LIC_CONTACT_TB_SITE_LIC] FOREIGN KEY([SITE_LIC_ID])
REFERENCES [dbo].[TB_SITE_LIC] ([SITE_LIC_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_SITE_LIC_CONTACT] CHECK CONSTRAINT [FK_TB_SITE_LIC_CONTACT_TB_SITE_LIC]
GO
ALTER TABLE [dbo].[TB_SITE_LIC_REVIEW]  WITH CHECK ADD  CONSTRAINT [FK_TB_SITE_LIC_REVIEW_TB_CONTACT] FOREIGN KEY([ADDED_BY])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO
ALTER TABLE [dbo].[TB_SITE_LIC_REVIEW] CHECK CONSTRAINT [FK_TB_SITE_LIC_REVIEW_TB_CONTACT]
GO
ALTER TABLE [dbo].[TB_SITE_LIC_REVIEW]  WITH CHECK ADD  CONSTRAINT [FK_TB_SITE_LIC_REVIEW_TB_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[TB_SITE_LIC_REVIEW] CHECK CONSTRAINT [FK_TB_SITE_LIC_REVIEW_TB_REVIEW]
GO
ALTER TABLE [dbo].[TB_SITE_LIC_REVIEW]  WITH CHECK ADD  CONSTRAINT [FK_TB_SITE_LIC_REVIEW_TB_SITE_LIC] FOREIGN KEY([SITE_LIC_ID])
REFERENCES [dbo].[TB_SITE_LIC] ([SITE_LIC_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_SITE_LIC_REVIEW] CHECK CONSTRAINT [FK_TB_SITE_LIC_REVIEW_TB_SITE_LIC]
GO
ALTER TABLE [dbo].[TB_SOURCE]  WITH NOCHECK ADD  CONSTRAINT [FK_TB_SOURCE_TB_IMPORT_FILTER] FOREIGN KEY([IMPORT_FILTER_ID])
REFERENCES [dbo].[TB_IMPORT_FILTER] ([IMPORT_FILTER_ID])
GO
ALTER TABLE [dbo].[TB_SOURCE] NOCHECK CONSTRAINT [FK_TB_SOURCE_TB_IMPORT_FILTER]
GO
ALTER TABLE [dbo].[TB_SOURCE]  WITH NOCHECK ADD  CONSTRAINT [FK_TB_SOURCE_tb_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[TB_SOURCE] CHECK CONSTRAINT [FK_TB_SOURCE_tb_REVIEW]
GO
ALTER TABLE [dbo].[TB_TERM_EXTR_T_MAP]  WITH CHECK ADD  CONSTRAINT [FK_TB_TERM_EXTR_T_MAP_TB_ITEM] FOREIGN KEY([ITEM_ID])
REFERENCES [dbo].[TB_ITEM] ([ITEM_ID])
GO
ALTER TABLE [dbo].[TB_TERM_EXTR_T_MAP] CHECK CONSTRAINT [FK_TB_TERM_EXTR_T_MAP_TB_ITEM]
GO
ALTER TABLE [dbo].[TB_TERMINE_LOG]  WITH CHECK ADD  CONSTRAINT [FK_TB_TERMINE_LOG_TB_CONTACT] FOREIGN KEY([CONTACT_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO
ALTER TABLE [dbo].[TB_TERMINE_LOG] CHECK CONSTRAINT [FK_TB_TERMINE_LOG_TB_CONTACT]
GO
ALTER TABLE [dbo].[TB_TERMINE_LOG]  WITH CHECK ADD  CONSTRAINT [FK_TB_TERMINE_LOG_TB_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[TB_TERMINE_LOG] CHECK CONSTRAINT [FK_TB_TERMINE_LOG_TB_REVIEW]
GO
ALTER TABLE [dbo].[TB_TRAINING]  WITH CHECK ADD  CONSTRAINT [FK_TB_TRAINING_TB_CONTACT] FOREIGN KEY([CONTACT_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO
ALTER TABLE [dbo].[TB_TRAINING] CHECK CONSTRAINT [FK_TB_TRAINING_TB_CONTACT]
GO
ALTER TABLE [dbo].[TB_TRAINING]  WITH CHECK ADD  CONSTRAINT [FK_TB_TRAINING_TB_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[TB_TRAINING] CHECK CONSTRAINT [FK_TB_TRAINING_TB_REVIEW]
GO
ALTER TABLE [dbo].[TB_TRAINING_ITEM]  WITH CHECK ADD  CONSTRAINT [FK_TB_TRAINING_ITEM_TB_TRAINING] FOREIGN KEY([TRAINING_ID])
REFERENCES [dbo].[TB_TRAINING] ([TRAINING_ID])
GO
ALTER TABLE [dbo].[TB_TRAINING_ITEM] CHECK CONSTRAINT [FK_TB_TRAINING_ITEM_TB_TRAINING]
GO
ALTER TABLE [dbo].[TB_WORK_ALLOCATION]  WITH CHECK ADD  CONSTRAINT [FK_TB_WORK_ALLOCATION_TB_ATTRIBUTE] FOREIGN KEY([ATTRIBUTE_ID])
REFERENCES [dbo].[TB_ATTRIBUTE] ([ATTRIBUTE_ID])
GO
ALTER TABLE [dbo].[TB_WORK_ALLOCATION] CHECK CONSTRAINT [FK_TB_WORK_ALLOCATION_TB_ATTRIBUTE]
GO
ALTER TABLE [dbo].[TB_WORK_ALLOCATION]  WITH CHECK ADD  CONSTRAINT [FK_TB_WORK_ALLOCATION_tb_CONTACT] FOREIGN KEY([CONTACT_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO
ALTER TABLE [dbo].[TB_WORK_ALLOCATION] CHECK CONSTRAINT [FK_TB_WORK_ALLOCATION_tb_CONTACT]
GO
ALTER TABLE [dbo].[TB_WORK_ALLOCATION]  WITH CHECK ADD  CONSTRAINT [FK_TB_WORK_ALLOCATION_TB_REVIEW] FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO
ALTER TABLE [dbo].[TB_WORK_ALLOCATION] CHECK CONSTRAINT [FK_TB_WORK_ALLOCATION_TB_REVIEW]
GO
ALTER TABLE [dbo].[TB_WORK_ALLOCATION]  WITH CHECK ADD  CONSTRAINT [FK_TB_WORK_ALLOCATION_TB_SET] FOREIGN KEY([SET_ID])
REFERENCES [dbo].[TB_SET] ([SET_ID])
GO
ALTER TABLE [dbo].[TB_WORK_ALLOCATION] CHECK CONSTRAINT [FK_TB_WORK_ALLOCATION_TB_SET]
GO
/****** Object:  StoredProcedure [dbo].[st_ArchieFindER4UserFromArchieID]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ArchieFindER4UserFromArchieID]
	-- Add the parameters for the stored procedure here
	@ARCHIE_ID varchar(32)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	--first, check that we have one and only one result
	declare @ck int = (Select COUNT(CONTACT_ID) from TB_CONTACT where ARCHIE_ID = @ARCHIE_ID and ARCHIE_ID is not null)
	if @ck = 1
	BEGIN
		select * from TB_CONTACT where  ARCHIE_ID = @ARCHIE_ID and ARCHIE_ID is not null
	END
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ArchieFindER4UserFromArchieID] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ArchieIdentityFromCodeAndStatus]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ArchieIdentityFromCodeAndStatus]
	-- Add the parameters for the stored procedure here
	@ARCHIE_CODE varchar(64)
	,@ARCHIE_STATE varchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    declare @ck int = (Select COUNT(CONTACT_ID) from TB_CONTACT where @ARCHIE_CODE = LAST_ARCHIE_CODE and @ARCHIE_STATE = LAST_ARCHIE_STATE 
		and (LAST_ARCHIE_CODE is not null and LAST_ARCHIE_STATE is not null))
	if @ck = 1
	BEGIN
	SELECT * from TB_CONTACT where @ARCHIE_CODE = LAST_ARCHIE_CODE and @ARCHIE_STATE = LAST_ARCHIE_STATE 
		and (LAST_ARCHIE_CODE is not null and LAST_ARCHIE_STATE is not null)
	END
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ArchieIdentityFromCodeAndStatus] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ArchieIdentityFromReviewer]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ArchieIdentityFromReviewer]
	-- Add the parameters for the stored procedure here
	@CID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ARCHIE_ID, ARCHIE_ACCESS_TOKEN, ARCHIE_TOKEN_VALID_UNTIL, ARCHIE_REFRESH_TOKEN from TB_CONTACT where CONTACT_ID = @CID
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ArchieIdentityFromReviewer] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ArchieIdentityFromUnassignedCodeAndStatus]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[st_ArchieIdentityFromUnassignedCodeAndStatus]
	-- Add the parameters for the stored procedure here
	@CID int
	,@ARCHIE_CODE varchar(64)
	,@ARCHIE_STATE varchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    declare @ck int = (
						Select COUNT(u.ARCHIE_ID) from TB_UNASSIGNED_ARCHIE_KEYS u
						inner join TB_CONTACT c	
							on c.CONTACT_ID = @CID and c.ARCHIE_ID is null
						where @ARCHIE_CODE = u.LAST_ARCHIE_CODE and @ARCHIE_STATE = u.LAST_ARCHIE_STATE 
						and (u.LAST_ARCHIE_CODE is not null and u.LAST_ARCHIE_STATE is not null)
						)
	if @ck = 1
	BEGIN --all is well
		--1. Save Archie keys in tb_contact
		update TB_CONTACT set 
			ARCHIE_ID = au.ARCHIE_ID
			,ARCHIE_ACCESS_TOKEN = au.ARCHIE_ACCESS_TOKEN
			,ARCHIE_TOKEN_VALID_UNTIL = au.ARCHIE_TOKEN_VALID_UNTIL
			,ARCHIE_REFRESH_TOKEN = au.ARCHIE_REFRESH_TOKEN
			,LAST_ARCHIE_CODE = au.LAST_ARCHIE_CODE
			,LAST_ARCHIE_STATE = au.LAST_ARCHIE_STATE
		From (
				Select * from TB_UNASSIGNED_ARCHIE_KEYS u where @ARCHIE_CODE = u.LAST_ARCHIE_CODE 
				and @ARCHIE_STATE = u.LAST_ARCHIE_STATE 
				and (u.LAST_ARCHIE_CODE is not null and u.LAST_ARCHIE_STATE is not null)
				) au
		WHERE CONTACT_ID = @CID
		--2. delete record from TB_UNASSIGNED_ARCHIE_KEYS
		delete from TB_UNASSIGNED_ARCHIE_KEYS where @ARCHIE_CODE = LAST_ARCHIE_CODE 
				and @ARCHIE_STATE = LAST_ARCHIE_STATE 
				and (LAST_ARCHIE_CODE is not null and LAST_ARCHIE_STATE is not null)
		--3. get All user details
		SELECT * from TB_CONTACT where @ARCHIE_CODE = LAST_ARCHIE_CODE and @ARCHIE_STATE = LAST_ARCHIE_STATE 
			and (LAST_ARCHIE_CODE is not null and LAST_ARCHIE_STATE is not null)
	END
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ArchieIdentityFromUnassignedCodeAndStatus] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ArchieReviewFindFromArchieID]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ArchieReviewFindFromArchieID]
	-- Add the parameters for the stored procedure here
	@A_ID char(18)
	,@CID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @ck int = (select COUNT(Review_id) from TB_REVIEW where ARCHIE_ID = @A_ID and ARCHIE_ID is not null)
	IF @ck = 1
	BEGIN
		select r.* 
		 ,CASE 
			when rc.CONTACT_ID is not null then 1
			else 0
		END as CONTACT_IS_IN_REVIEW
		,dbo.fn_REBUILD_ROLES(rc.REVIEW_CONTACT_ID) as ROLES
		from TB_REVIEW r
		left outer join TB_REVIEW_CONTACT rc on r.REVIEW_ID = rc.REVIEW_ID and CONTACT_ID = @CID
		where ARCHIE_ID = @A_ID and ARCHIE_ID is not null
	END
END



GO
ALTER AUTHORIZATION ON [dbo].[st_ArchieReviewFindFromArchieID] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ArchieReviewLinkToER4Review]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ArchieReviewLinkToER4Review] 
	-- Add the parameters for the stored procedure here
	@RID int,
	@CID int,
	@ARID char(18),
	@ARCD char(8),
	@IS_CHECKEDOUT_HERE bit,
	@RES int out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @ck int = (select COUNT(r.REVIEW_ID) from TB_REVIEW r 
						inner join TB_REVIEW_CONTACT rc on r.REVIEW_ID = rc.REVIEW_ID and rc.CONTACT_ID = @CID
						 where @RID = r.REVIEW_ID and (r.IS_CHECKEDOUT_HERE is null OR r.IS_CHECKEDOUT_HERE = 0)
						 )
	if @ck != 1
	BEGIN
		set @RES = -1
		return
	END
	UPDATE TB_REVIEW set ARCHIE_ID = @ARID, ARCHIE_CD = @ARCD, IS_CHECKEDOUT_HERE = @IS_CHECKEDOUT_HERE
	WHERE REVIEW_ID = @RID
	if @@ROWCOUNT = 1
		Set @RES = 1
	else
		Set @RES = -2
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ArchieReviewLinkToER4Review] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ArchieReviewMarkAsCheckedInOut]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ArchieReviewMarkAsCheckedInOut] 
	-- Add the parameters for the stored procedure here
	@RID int,
	@CID int,
	@ARID char(18),
	@ARCD char(8),
	@IS_CHECKEDOUT_HERE bit,
	@RES int out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @ck int = (select COUNT(r.REVIEW_ID) from TB_REVIEW r 
						inner join TB_REVIEW_CONTACT rc on r.REVIEW_ID = rc.REVIEW_ID and rc.CONTACT_ID = @CID
						 where @RID = r.REVIEW_ID and (@ARID = r.ARCHIE_ID or r.ARCHIE_ID is null)
						 )
	if @ck != 1
	BEGIN
		set @RES = -1
		return
	END
	UPDATE TB_REVIEW set ARCHIE_ID = @ARID, ARCHIE_CD = @ARCD, IS_CHECKEDOUT_HERE = @IS_CHECKEDOUT_HERE
	WHERE REVIEW_ID = @RID
	if @@ROWCOUNT = 1
	BEGIN
		Set @RES = 1
		if @IS_CHECKEDOUT_HERE = 0
		--IF we are marking the review as CHECKED-IN in Archie, we need to kick out currenlty logged on users
		--we do this by adding a second "Active" ticket to all currently logged on users
		-- this gets picked up by the client when ticket & status get checked
		--on client side, the code will receive the message 'Multiple' which never happens otherwise
		--if the user is Cochrane, they will get an explanation
		--otherwise they get a generic error.
		BEGIN
			DECLARE @T TABLE 
			( 
				CI int,
				TK uniqueidentifier
			) 
			insert into @T
				SELECT CONTACT_ID, newid()
				from ReviewerAdmin.dbo.TB_LOGON_TICKET
				WHERE REVIEW_ID = @RID and [STATE] = 1 and LAST_RENEWED > DATEADD(HH, -3, GETDATE())
			
			INSERT into ReviewerAdmin.dbo.TB_LOGON_TICKET(TICKET_GUID, CONTACT_ID, REVIEW_ID)
			SELECT TK, CI, @RID From @T 
		END
	END
	else
		Set @RES = -2
	
END



GO
ALTER AUTHORIZATION ON [dbo].[st_ArchieReviewMarkAsCheckedInOut] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ArchieSaveTokens]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ArchieSaveTokens]
	-- Add the parameters for the stored procedure here
	@ARCHIE_ID varchar(32)
	,@TOKEN varchar(64)
	,@VALID_UNTIL datetime2(1)
	,@REFRESH_T varchar(64)
	,@ARCHIE_CODE varchar(64) = null
	,@ARCHIE_STATE varchar(10) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	Update TB_CONTACT set 
		ARCHIE_ACCESS_TOKEN = @TOKEN
		,ARCHIE_TOKEN_VALID_UNTIL = @VALID_UNTIL
		,ARCHIE_REFRESH_TOKEN = @REFRESH_T
		,LAST_ARCHIE_CODE = @ARCHIE_CODE
		,LAST_ARCHIE_STATE = @ARCHIE_STATE
		where ARCHIE_ID = @ARCHIE_ID
	
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ArchieSaveTokens] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ArchieSaveUnassignedTokens]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ArchieSaveUnassignedTokens]
	-- Add the parameters for the stored procedure here
	@ARCHIE_ID varchar(32)
	,@TOKEN varchar(64)
	,@VALID_UNTIL datetime2(1)
	,@REFRESH_T varchar(64)
	,@ARCHIE_CODE varchar(64)
	,@ARCHIE_STATE varchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	--first, make sure there are no duplicates
	delete from TB_UNASSIGNED_ARCHIE_KEYS where ARCHIE_ID = @ARCHIE_ID
	INSERT INTO [Reviewer].[dbo].[TB_UNASSIGNED_ARCHIE_KEYS]
           ([ARCHIE_ID]
           ,[ARCHIE_ACCESS_TOKEN]
           ,[ARCHIE_TOKEN_VALID_UNTIL]
           ,[ARCHIE_REFRESH_TOKEN]
           ,[LAST_ARCHIE_CODE]
           ,[LAST_ARCHIE_STATE])
     VALUES
           (@ARCHIE_ID
           ,@TOKEN
           ,@VALID_UNTIL
           ,@REFRESH_T
           ,@ARCHIE_CODE
           ,@ARCHIE_STATE)
	
END



GO
ALTER AUTHORIZATION ON [dbo].[st_ArchieSaveUnassignedTokens] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_AttributeSet]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_AttributeSet]
(
	@SET_ID INT,
	@PARENT_ATTRIBUTE_ID BIGINT = 0
)

As

SELECT TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID, TB_ATTRIBUTE_SET.SET_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_ID, TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID,
	TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_SET_DESC, TB_ATTRIBUTE_SET.ATTRIBUTE_ORDER,
	ATTRIBUTE_TYPE, ATTRIBUTE_NAME, ATTRIBUTE_SET_DESC, CONTACT_ID, TB_ATTRIBUTE.ATTRIBUTE_DESC

FROM TB_ATTRIBUTE_SET
INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ATTRIBUTE_SET.SET_ID
INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
INNER JOIN TB_ATTRIBUTE_TYPE ON TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID

WHERE TB_ATTRIBUTE_SET.SET_ID = @SET_ID
	AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @PARENT_ATTRIBUTE_ID

ORDER BY PARENT_ATTRIBUTE_ID, ATTRIBUTE_ORDER


GO
ALTER AUTHORIZATION ON [dbo].[st_AttributeSet] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_AttributeSetDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_AttributeSetDelete]
(
	@ATTRIBUTE_SET_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@PARENT_ATTRIBUTE_ID BIGINT,
	@ATTRIBUTE_ORDER INT,
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

DECLARE @SET_ID INT

SELECT @SET_ID = SET_ID FROM TB_ATTRIBUTE_SET
	WHERE ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID

DELETE FROM TB_ITEM_ATTRIBUTE_TEXT
FROM TB_ITEM_ATTRIBUTE_TEXT
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID
INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		AND TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID
		AND TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID

DELETE FROM TB_ITEM_ATTRIBUTE_PDF
FROM TB_ITEM_ATTRIBUTE_PDF
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE_PDF.ITEM_ATTRIBUTE_ID
INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		AND TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID
		AND TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID

DELETE FROM TB_ITEM_ATTRIBUTE
from TB_ITEM_ATTRIBUTE
INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		AND TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID
		AND TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		
DELETE FROM TB_ITEM_SET
WHERE NOT ITEM_SET_ID IN 
(
	SELECT DISTINCT ITEM_SET_ID  
    FROM TB_ITEM_ATTRIBUTE ia
    inner join tb_item_review ir on ia.ITEM_ID = ir.ITEM_ID 
    and ir.REVIEW_ID = @REVIEW_ID
    union
    select tio.item_set_id
    from TB_ITEM_OUTCOME tio
    inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID
    inner join TB_ITEM_REVIEW ir on tis.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
) and SET_ID = @SET_ID

	SELECT TB_ATTRIBUTE_SET.ATTRIBUTE_ID FROM TB_ATTRIBUTE_SET
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.SET_ID = TB_ATTRIBUTE_SET.SET_ID
		INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID
			AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
		WHERE TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID

	IF (@@ROWCOUNT = 0)
	BEGIN

		DELETE FROM TB_ATTRIBUTE_SET WHERE ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID

		UPDATE TB_ATTRIBUTE_SET
				SET ATTRIBUTE_ORDER = ATTRIBUTE_ORDER -1
				WHERE PARENT_ATTRIBUTE_ID = @PARENT_ATTRIBUTE_ID
				AND ATTRIBUTE_ORDER > @ATTRIBUTE_ORDER
				AND SET_ID = @SET_ID

		SELECT ATTRIBUTE_ID FROM TB_ATTRIBUTE_SET WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID

		IF (@@ROWCOUNT = 0)
		BEGIN
			DELETE FROM TB_ATTRIBUTE WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
		END

	END
	

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_AttributeSetDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_AttributeSetDeleteWarning]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_AttributeSetDeleteWarning]
(
	@ATTRIBUTE_SET_ID BIGINT,
	@SET_ID INT,
	@NUM_ITEMS BIGINT OUTPUT,
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	SELECT @NUM_ITEMS = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_SET.SET_ID
			AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
			AND TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID


SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_AttributeSetDeleteWarning] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_AttributeSetInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_AttributeSetInsert]
(
	@SET_ID INT,
	@PARENT_ATTRIBUTE_ID BIGINT = 0,
	@ATTRIBUTE_TYPE_ID INT = 1,
	@ATTRIBUTE_SET_DESC NVARCHAR(MAX) = null,
	@ATTRIBUTE_ORDER INT = 1,
	@ATTRIBUTE_NAME NVARCHAR(255),
	@ATTRIBUTE_DESC NVARCHAR(2000) = null,
	@CONTACT_ID INT,
	@ORIGINAL_ATTRIBUTE_ID BIGINT = null,

	@NEW_ATTRIBUTE_SET_ID BIGINT OUTPUT,
	@NEW_ATTRIBUTE_ID BIGINT OUTPUT
)

As

SET NOCOUNT ON

	INSERT INTO TB_ATTRIBUTE(CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, ORIGINAL_ATTRIBUTE_ID)
		VALUES(@CONTACT_ID, @ATTRIBUTE_NAME, @ATTRIBUTE_DESC, @ORIGINAL_ATTRIBUTE_ID)

	SET @NEW_ATTRIBUTE_ID = @@IDENTITY

	INSERT INTO TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)
		VALUES(@NEW_ATTRIBUTE_ID, @SET_ID, @PARENT_ATTRIBUTE_ID, @ATTRIBUTE_TYPE_ID, @ATTRIBUTE_SET_DESC, @ATTRIBUTE_ORDER)

	SET @NEW_ATTRIBUTE_SET_ID = @@IDENTITY


SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_AttributeSetInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_AttributeSetLimitedUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[st_AttributeSetLimitedUpdate]
(
	@ATTRIBUTE_ID BIGINT,
	@ATTRIBUTE_SET_ID BIGINT,
	@ATTRIBUTE_TYPE_ID INT,
	@ATTRIBUTE_NAME NVARCHAR(255),
	@ATTRIBUTE_DESCRIPTION NVARCHAR(MAX),
	@ATTRIBUTE_ORDER INT
)

As

SET NOCOUNT ON

	UPDATE TB_ATTRIBUTE
		SET ATTRIBUTE_NAME = @ATTRIBUTE_NAME
		WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
		
	UPDATE TB_ATTRIBUTE_SET
		SET ATTRIBUTE_SET_DESC = @ATTRIBUTE_DESCRIPTION,
		 ATTRIBUTE_TYPE_ID = @ATTRIBUTE_TYPE_ID,
		 ATTRIBUTE_ORDER = @ATTRIBUTE_ORDER
		WHERE ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_AttributeSetLimitedUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_AttributeSetMove]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_AttributeSetMove]
(
	@ATTRIBUTE_SET_ID BIGINT,
	@ATTRIBUTE_ORDER INT,
	@FROM BIGINT,
	@TO BIGINT
)

As

SET NOCOUNT ON

	DECLARE @OLD_CODE_ORDER bigint
	DECLARE @SET_ID BIGINT

	SELECT @OLD_CODE_ORDER = ATTRIBUTE_ORDER, @SET_ID = SET_ID
		FROM TB_ATTRIBUTE_SET
		WHERE ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID

	-- REMOVE FROM CURRENT SET
	UPDATE TB_ATTRIBUTE_SET
		SET ATTRIBUTE_ORDER = ATTRIBUTE_ORDER -1
		WHERE PARENT_ATTRIBUTE_ID = @FROM
		AND ATTRIBUTE_ORDER > @OLD_CODE_ORDER
		AND SET_ID = @SET_ID -- Need SetId as parent_attribute_id can be null or 0

	-- MAKE SPACE IN THE NEW ONE
	UPDATE TB_ATTRIBUTE_SET
		SET ATTRIBUTE_ORDER = ATTRIBUTE_ORDER + 1
		WHERE PARENT_ATTRIBUTE_ID = @TO
		AND ATTRIBUTE_ORDER >= @ATTRIBUTE_ORDER
		AND SET_ID = @SET_ID

	-- INSERT THE ATTRIBUTE IN ITS NEW PLACE
	UPDATE TB_ATTRIBUTE_SET
		SET ATTRIBUTE_ORDER = @ATTRIBUTE_ORDER,
			PARENT_ATTRIBUTE_ID = @TO
		WHERE ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID
		AND SET_ID = @SET_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_AttributeSetMove] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_AttributeSetUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_AttributeSetUpdate]
(
	@ATTRIBUTE_SET_ID BIGINT,
	@SET_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@PARENT_ATTRIBUTE_ID BIGINT,
	@ATTRIBUTE_TYPE_ID INT,
	@ATTRIBUTE_SET_DESC NVARCHAR(MAX),
	@ATTRIBUTE_ORDER INT,
	@ATTRIBUTE_NAME NVARCHAR(255),
	@ATTRIBUTE_DESC NVARCHAR(2000),
	@CONTACT_ID INT -- not used yet - maybe for authorisation
)

As

SET NOCOUNT ON

	UPDATE TB_ATTRIBUTE
		SET ATTRIBUTE_NAME = @ATTRIBUTE_NAME, ATTRIBUTE_DESC = @ATTRIBUTE_DESC
		WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID

	UPDATE TB_ATTRIBUTE_SET
		SET SET_ID = @SET_ID, PARENT_ATTRIBUTE_ID = @PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID = @ATTRIBUTE_TYPE_ID,
			ATTRIBUTE_SET_DESC = @ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER = @ATTRIBUTE_ORDER
		WHERE ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID


SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_AttributeSetUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_AttributeTextAllItems]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[st_AttributeTextAllItems] (
        @ATTRIBUTE_SET_ID BIGINT
)
AS
SET NOCOUNT ON


SELECT TB_ITEM.TITLE, SHORT_TITLE, TB_ITEM.ITEM_ID, ADDITIONAL_TEXT, TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID, DOCUMENT_TITLE
				, TEXT_FROM, TEXT_TO, 0 as [PAGE]
                ,        SUBSTRING(
                        replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
                        ) CODED_TEXT
                ,'None' as [ORIGIN]
                FROM tb_ITEM_ATTRIBUTE
                LEFT JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
                LEFT JOIN TB_ITEM_ATTRIBUTE_PDF ON TB_ITEM_ATTRIBUTE_PDF.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
                LEFT JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
                INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = tb_ITEM_ATTRIBUTE.ITEM_ID
                INNER JOIN TB_ITEM_REVIEW ir on TB_ITEM.ITEM_ID = ir.ITEM_ID and ir.IS_INCLUDED = 1 and ir.IS_DELETED = 0
                INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
                INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
                        AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = tb_ITEM_ATTRIBUTE.ATTRIBUTE_ID

                WHERE TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID AND IS_COMPLETED = 'TRUE'
					AND TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID is null
					AND TB_ITEM_ATTRIBUTE_PDF.ITEM_ATTRIBUTE_ID is null
					and ADDITIONAL_TEXT is not null
					AND LTRIM ( RTRIM(ADDITIONAL_TEXT )) != ''

UNION
SELECT TB_ITEM.TITLE, SHORT_TITLE, TB_ITEM.ITEM_ID, ADDITIONAL_TEXT, TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID, DOCUMENT_TITLE
				, TEXT_FROM, TEXT_TO, 0 as [PAGE]
                ,        SUBSTRING(
                        replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
                        ) CODED_TEXT
                ,'Text' as [ORIGIN]
                FROM tb_ITEM_ATTRIBUTE
                INNER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
                INNER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
                INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = tb_ITEM_ATTRIBUTE.ITEM_ID
                INNER JOIN TB_ITEM_REVIEW ir on TB_ITEM.ITEM_ID = ir.ITEM_ID and ir.IS_INCLUDED = 1 and ir.IS_DELETED = 0
                INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
                INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
                        AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = tb_ITEM_ATTRIBUTE.ATTRIBUTE_ID

                WHERE TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID AND IS_COMPLETED = 'TRUE'
UNION
SELECT TB_ITEM.TITLE, SHORT_TITLE, TB_ITEM.ITEM_ID, ADDITIONAL_TEXT, TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID, DOCUMENT_TITLE
				, 0 as [TEXT_FROM], 0 as [TEXT_TO], PAGE
                ,'Page ' + CONVERT(varchar(10),PAGE) + ':' + CHAR(10) + '[¬s]"' + replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"'
					AS [CODED_TEXT]
                ,'Pdf' as [ORIGIN]
                FROM tb_ITEM_ATTRIBUTE
                INNER JOIN TB_ITEM_ATTRIBUTE_PDF ON TB_ITEM_ATTRIBUTE_PDF.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
                INNER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_PDF.ITEM_DOCUMENT_ID
                INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = tb_ITEM_ATTRIBUTE.ITEM_ID
                INNER JOIN TB_ITEM_REVIEW ir on TB_ITEM.ITEM_ID = ir.ITEM_ID and ir.IS_INCLUDED = 1 and ir.IS_DELETED = 0
                INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
                INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
                        AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = tb_ITEM_ATTRIBUTE.ATTRIBUTE_ID

                WHERE TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID AND IS_COMPLETED = 'TRUE'
        ORDER BY SHORT_TITLE, TB_ITEM.ITEM_ID, ITEM_DOCUMENT_ID, ORIGIN, TEXT_FROM, PAGE
        
SET NOCOUNT OFF
        RETURN



GO
ALTER AUTHORIZATION ON [dbo].[st_AttributeTextAllItems] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_AttributeTypes]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_AttributeTypes]

As

SELECT * FROM TB_ATTRIBUTE_TYPE
ORDER BY ATTRIBUTE_TYPE_ID

GO
ALTER AUTHORIZATION ON [dbo].[st_AttributeTypes] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_CheckEmail]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		Sergio
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[st_CheckEmail] 
      @USERID nvarchar(50), 
      @EMAIL nvarchar(100)
AS

BEGIN
      SET NOCOUNT ON;

    -- Insert statements for procedure here
      SELECT EMAIL, [PASSWORD], CONTACT_NAME from TB_CONTACT WHERE USERNAME = @USERID 
            AND EMAIL = @EMAIL 
            AND EMAIL != ''
            AND (TB_CONTACT.EMAIL IS NOT NULL)
            --AND PASSWD != 'FROZEN_ACCOUNT' 
            --AND tb_CONTACT.IS_GROUP = 'N'
            --To add the expiration constraint uncomment the following:
            --AND DATEDIFF(day, tb_CONTACT.DATE_LAST_RENEWED, getdate()) <= tb_CONTACT.ACCESS_LENGTH_DAYS
END




GO
ALTER AUTHORIZATION ON [dbo].[st_CheckEmail] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierCreateSearchList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ClassifierCreateSearchList]
(
	@REVIEW_ID INT
,	@CONTACT_ID INT
,	@SEARCH_TITLE NVARCHAR(4000)
,	@SEARCH_DESC varchar(4000) = null
,	@HITS_NO INT
,	@NEW_SEARCH_ID INT OUTPUT
)

As

SET NOCOUNT ON

	-- STEP 1: GET THE SEARCH NUMBER FOR THIS REVIEW
	DECLARE @SEARCH_NO INT
	SELECT @SEARCH_NO = ISNULL(MAX(SEARCH_NO), 0) + 1 FROM tb_SEARCH WHERE REVIEW_ID = @REVIEW_ID

	-- STEP 2: CREATE THE SEARCH RECORD
	INSERT INTO tb_SEARCH
	(	REVIEW_ID
	,	CONTACT_ID
	,	SEARCH_TITLE
	,	SEARCH_NO
	,	HITS_NO
	,	SEARCH_DATE
	)	
	VALUES
	(
		@REVIEW_ID
	,	@CONTACT_ID
	,	@SEARCH_TITLE
	,	@SEARCH_NO
	,	@HITS_NO
	,	GetDate()
	)
	-- Get the identity and return it
	SET @NEW_SEARCH_ID = @@identity
	
	-- STEP 3: PUT THE ITEMS INTO THE SEARCH LIST
	
	INSERT INTO TB_SEARCH_ITEM(ITEM_ID,SEARCH_ID, ITEM_RANK)
		SELECT CIT.ITEM_ID, @NEW_SEARCH_ID,  0
				FROM TB_CLASSIFIER_ITEM_TEMP CIT
			ORDER BY CIT.SCORE DESC
	
	-- STEP 3: PUT THE ITEM_RANK VALUES IN (I.E. SCORES FROM 1 TO N)
	DECLARE @START_INDEX INT = 0
	SELECT @START_INDEX = MIN(SEARCH_ITEM_ID) FROM TB_SEARCH_ITEM WHERE SEARCH_ID = @NEW_SEARCH_ID
	UPDATE TB_SEARCH_ITEM
		SET ITEM_RANK = SEARCH_ITEM_ID - @START_INDEX + 1
		WHERE SEARCH_ID = @NEW_SEARCH_ID
	
	-- STEP 4: DELETE ITEMS FROM TEMP TABLE
	DELETE FROM TB_CLASSIFIER_ITEM_TEMP WHERE REVIEW_ID = @REVIEW_ID	
		
SET NOCOUNT OFF



GO
ALTER AUTHORIZATION ON [dbo].[st_ClassifierCreateSearchList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierGetClassificationData]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ClassifierGetClassificationData]
(
	@REVIEW_ID INT
,	@ATTRIBUTE_ID_CLASSIFY_TO BIGINT = NULL
)

As

SET NOCOUNT ON

	DELETE FROM TB_CLASSIFIER_ITEM_TEMP WHERE REVIEW_ID = @REVIEW_ID	

	SELECT DISTINCT '99' LABEL, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	WHERE REVIEW_ID = @REVIEW_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_CLASSIFY_TO AND IS_DELETED = 'FALSE'
	
		
SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ClassifierGetClassificationData] TO  SCHEMA OWNER 
GO
--/****** Object:  StoredProcedure [dbo].[st_ClassifierGetModel]    Script Date: 6/15/2016 11:49:45 AM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--CREATE procedure [dbo].[st_ClassifierGetModel]
--(
--	@MODEL_ID INT
--)

--As

--SET NOCOUNT ON

--	SELECT MODEL_INFO FROM tb_CLASSIFIER_MODEL
--	WHERE MODEL_ID = @MODEL_ID

--SET NOCOUNT OFF


--GO
--ALTER AUTHORIZATION ON [dbo].[st_ClassifierGetModel] TO  SCHEMA OWNER 
--GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierGetTrainingData]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ClassifierGetTrainingData]
(
	@REVIEW_ID INT
,	@ATTRIBUTE_ID_ON BIGINT = NULL
,	@ATTRIBUTE_ID_NOT_ON BIGINT = NULL
)

As

SET NOCOUNT ON

	SELECT DISTINCT '1' LABEL, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	WHERE REVIEW_ID = @REVIEW_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_ON AND IS_DELETED = 'FALSE'
	
	UNION ALL
	
	SELECT DISTINCT '0' LABEL, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	WHERE REVIEW_ID = @REVIEW_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_NOT_ON AND IS_DELETED = 'FALSE'



SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ClassifierGetTrainingData] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierModels]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ClassifierModels]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	select MODEL_ID, MODEL_TITLE, REVIEW_ID, A1.ATTRIBUTE_NAME ATTRIBUTE_ON, A2.ATTRIBUTE_NAME ATTRIBUTE_NOT_ON,
		CONTACT_NAME, tb_CLASSIFIER_MODEL.CONTACT_ID, ACCURACY, AUC, [PRECISION], RECALL from tb_CLASSIFIER_MODEL
	INNER JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_ON
	INNER JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_NOT_ON
	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = tb_CLASSIFIER_MODEL.CONTACT_ID
	
	where REVIEW_ID = @REVIEW_ID
	order by MODEL_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ClassifierModels] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierSaveModel]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ClassifierSaveModel]
(
	@REVIEW_ID INT
,	@ATTRIBUTE_ID_ON BIGINT = NULL
,	@ATTRIBUTE_ID_NOT_ON BIGINT = NULL
,	@CONTACT_ID INT
,	@MODEL_TITLE NVARCHAR(1000) = NULL
,	@NEW_MODEL_ID INT OUTPUT
)

As

SET NOCOUNT ON

	DECLARE @START_TIME DATETIME
	DECLARE @END_TIME DATETIME

	SELECT @START_TIME = MAX(TIME_STARTED) FROM tb_CLASSIFIER_MODEL
		WHERE REVIEW_ID = @REVIEW_ID

	SELECT @END_TIME = MAX(TIME_ENDED) FROM tb_CLASSIFIER_MODEL
		WHERE REVIEW_ID = @REVIEW_ID
		
	IF (@START_TIME IS NULL) OR (@START_TIME != @END_TIME) OR
	(
		(@START_TIME = @END_TIME) AND CURRENT_TIMESTAMP > DATEADD(HOUR, 2, @START_TIME) -- i.e. one review can only run one classification task at a time
	)
	BEGIN
		INSERT INTO tb_CLASSIFIER_MODEL(MODEL_TITLE, CONTACT_ID, REVIEW_ID, ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, TIME_STARTED, TIME_ENDED)
	VALUES(@MODEL_TITLE, @CONTACT_ID, @REVIEW_ID, @ATTRIBUTE_ID_ON, @ATTRIBUTE_ID_NOT_ON, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
		
	SET @NEW_MODEL_ID = @@IDENTITY
	END
	ELSE
	BEGIN
		SET @NEW_MODEL_ID = 0
	END

	

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ClassifierSaveModel] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierUpdateModel]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ClassifierUpdateModel]
(
	@MODEL_ID INT
,	@ACCURACY DECIMAL(18,18) = NULL
,	@AUC DECIMAL(18,18) = NULL
,	@PRECISION DECIMAL(18,18) = NULL
,	@RECALL DECIMAL(18,18) = NULL
)

As

SET NOCOUNT ON
	update tb_CLASSIFIER_MODEL
		SET TIME_ENDED = CURRENT_TIMESTAMP,
		ACCURACY = @ACCURACY,
		AUC = @AUC,
		[PRECISION] = @PRECISION,
		RECALL = @RECALL
		
	WHERE MODEL_ID = @MODEL_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ClassifierUpdateModel] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ClusterGetXmlAll]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ClusterGetXmlAll]
(
	@REVIEW_ID INT
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       Title as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		abstract as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')


GO
ALTER AUTHORIZATION ON [dbo].[st_ClusterGetXmlAll] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ClusterGetXmlAllDocs]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ClusterGetXmlAllDocs]
(
	@REVIEW_ID INT
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       Title as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		DOCUMENT_TEXT as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')


GO
ALTER AUTHORIZATION ON [dbo].[st_ClusterGetXmlAllDocs] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ClusterGetXmlFiltered]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ClusterGetXmlFiltered]
(
	@REVIEW_ID INT,
	@ITEM_ID_LIST NVARCHAR(max)
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       Title as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		abstract as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID


order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')

GO
ALTER AUTHORIZATION ON [dbo].[st_ClusterGetXmlFiltered] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ClusterGetXmlFilteredCode]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ClusterGetXmlFilteredCode]
(
	@REVIEW_ID INT,
	@ATTRIBUTE_SET_ID_LIST NVARCHAR(max)
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       Title as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		abstract as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'


order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')


GO
ALTER AUTHORIZATION ON [dbo].[st_ClusterGetXmlFilteredCode] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ClusterGetXmlFilteredCodeDocs]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[st_ClusterGetXmlFilteredCodeDocs]
(
	@REVIEW_ID INT,
	@ATTRIBUTE_SET_ID_LIST NVARCHAR(max)
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       Title as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		DOCUMENT_TEXT as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'


order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')


GO
ALTER AUTHORIZATION ON [dbo].[st_ClusterGetXmlFilteredCodeDocs] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ClusterGetXmlFilteredDocs]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[st_ClusterGetXmlFilteredDocs]
(
	@REVIEW_ID INT,
	@ITEM_ID_LIST NVARCHAR(max)
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       Title as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		DOCUMENT_TEXT as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID


order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')


GO
ALTER AUTHORIZATION ON [dbo].[st_ClusterGetXmlFilteredDocs] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonAttributesList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ComparisonAttributesList]
(
	@COMPARISON_ID INT,
	@PARENT_ATTRIBUTE_ID BIGINT = 0,
	@SET_ID INT
)

As

SET NOCOUNT ON

	SELECT COMPARISON_ITEM_ATTRIBUTE_ID, COMPARISON_ID, tb_COMPARISON_ITEM_ATTRIBUTE.ITEM_ID, tb_COMPARISON_ITEM_ATTRIBUTE.ATTRIBUTE_ID,
		ADDITIONAL_TEXT, tb_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID, tb_COMPARISON_ITEM_ATTRIBUTE.SET_ID,
		ATTRIBUTE_NAME, TITLE, CAST('FALSE' AS BIT) IS_COMPLETED
	FROM tb_COMPARISON_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = tb_COMPARISON_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = tb_COMPARISON_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = tb_COMPARISON_ITEM_ATTRIBUTE.ATTRIBUTE_ID
			AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @PARENT_ATTRIBUTE_ID
			AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
	WHERE COMPARISON_ID = @COMPARISON_ID
	
	UNION
	
	SELECT 0, @COMPARISON_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT, 
		TB_ITEM_SET.CONTACT_ID, TB_ITEM_SET.SET_ID, ATTRIBUTE_NAME, TITLE, CAST('TRUE' AS BIT) IS_COMPLETED
	FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.SET_ID = @SET_ID
		INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN tb_COMPARISON_ITEM_ATTRIBUTE ON tb_COMPARISON_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND tb_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID = @COMPARISON_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = tb_COMPARISON_ITEM_ATTRIBUTE.ATTRIBUTE_ID
			AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @PARENT_ATTRIBUTE_ID
			AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
	WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	
	ORDER BY ITEM_ID, CONTACT_ID
	
SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ComparisonAttributesList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonComplete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ComparisonComplete]
(
	@COMPARISON_ID INT,
	@CONTACT_ID INT,
	@WHICH_REVIEWERS NVARCHAR(20)
	--@RECORDS_AFFECTED INT OUTPUT
)

As

--SET NOCOUNT ON

DECLARE @T1 TABLE --item_attribute for R1, R1 and R2 are relative to the sproc, could be any couple from 
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)
DECLARE @T2 TABLE --item_attribute for R2
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)
DECLARE @T1a2 table (ITEM_ID bigint primary key)--snapshot agreements 1 v 2
--declare @c1 int
--declare @c2 int


--If @WHICH_REVIEWERS = 'Complete1vs2'
--	BEGIN
--		select @c1 = CONTACT_ID1, @c2 = CONTACT_ID2 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END
--ELSE If @WHICH_REVIEWERS = 'Complete1vs3'
--	BEGIN
--		select @c1 = CONTACT_ID1, @c2 = CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END
--ELSE  --Complete2vs3
--	BEGIN
--		select @c1 = CONTACT_ID2, @c2 = CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END

insert into @T1
select ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
	inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID 
												and  cia.CONTACT_ID = 
													CASE  @WHICH_REVIEWERS
														WHEN 'Complete2vs3' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID1
													END 
												and c.COMPARISON_ID = @COMPARISON_ID

insert into @T2
select ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID 
												and cia.CONTACT_ID = 
													CASE  @WHICH_REVIEWERS
														WHEN 'Complete1vs2' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID3
													END 
												and c.COMPARISON_ID = @COMPARISON_ID



insert into @T1a2 --add all agreements; see st_ComparisonStats to understand how this works
Select distinct t1.ITEM_ID from @T1 t1 
	inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID
	except
select distinct(t1.item_id) from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
	left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

--select * from @T1
--select * from @T2
--select * from @T1a2

Delete from @T1a2 where ITEM_ID in --remove all items that are already completed
(
	select tis.ITEM_ID from TB_ITEM_SET tis inner join 
		tb_COMPARISON c on tis.SET_ID = c.SET_ID and COMPARISON_ID = @COMPARISON_ID
		Inner join @T1a2 t1 on tis.ITEM_ID = t1.ITEM_ID and tis.IS_COMPLETED = 1
)

--select * from @T1a2


Update TB_ITEM_SET set IS_COMPLETED = 1 
	where CONTACT_ID = @CONTACT_ID
		and ITEM_SET_ID in
			(	
				select ITEM_SET_ID from TB_ITEM_SET tis 
				inner join tb_COMPARISON c on tis.SET_ID = c.SET_ID and c.COMPARISON_ID = @COMPARISON_ID
				inner join @T1a2 t on tis.ITEM_ID = t.ITEM_ID
			)









--INSERT INTO @TT(ITEM_ID, ATTRIBUTE_ID)
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--			CASE  @WHICH_REVIEWERS
--				WHEN 'Complete2vs3' THEN tb_COMPARISON.CONTACT_ID2
--				ELSE tb_COMPARISON.CONTACT_ID1
--			END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	INTERSECT --  ********** AGREEMENT - THEREFORE INTERSECT
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--		CASE @WHICH_REVIEWERS
--			WHEN 'Complete1vs2' THEN TB_COMPARISON.CONTACT_ID2
--			ELSE tb_COMPARISON.CONTACT_ID3
--		END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	ORDER BY ITEM_ID, ATTRIBUTE_ID

--UPDATE TB_ITEM_SET
--SET IS_COMPLETED = 'TRUE' WHERE TB_ITEM_SET.CONTACT_ID = @CONTACT_ID AND TB_ITEM_SET.ITEM_SET_ID IN
--(
--	SELECT ITEM_SET_ID FROM TB_ITEM_SET TB_IS
--	INNER JOIN tb_COMPARISON_ITEM_ATTRIBUTE ON
--		tb_COMPARISON_ITEM_ATTRIBUTE.SET_ID = TB_IS.SET_ID AND
--		tb_COMPARISON_ITEM_ATTRIBUTE.ITEM_ID = TB_IS.ITEM_ID AND
--		tb_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID = TB_IS.CONTACT_ID AND
--		tb_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID = @COMPARISON_ID
--	WHERE TB_IS.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
--)


SELECT @@ROWCOUNT

GO
ALTER AUTHORIZATION ON [dbo].[st_ComparisonComplete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ComparisonDelete]
(
	@COMPARISON_ID INT
)

As

SET NOCOUNT ON

	DELETE FROM tb_COMPARISON_ITEM_ATTRIBUTE WHERE COMPARISON_ID = @COMPARISON_ID
	
	DELETE FROM tb_COMPARISON WHERE COMPARISON_ID = @COMPARISON_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ComparisonDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ComparisonInsert]
(
	@REVIEW_ID INT,
	@IN_GROUP_ATTRIBUTE_ID BIGINT,
	@SET_ID INT,
	@COMPARISON_DATE DATE,
	@CONTACT_ID1 INT = NULL,
	@CONTACT_ID2 INT = NULL,
	@CONTACT_ID3 INT = NULL,
	@NEW_COMPARISON_ID INT OUTPUT,
	@Is_Screening bit OUTPUT
)

As

SET NOCOUNT ON
	
	set @Is_Screening = (Select CASE when SET_TYPE_ID = 5 then 1 else 0 end from TB_SET where SET_ID = @SET_ID)
	INSERT INTO tb_COMPARISON (REVIEW_ID, IN_GROUP_ATTRIBUTE_ID, SET_ID,
		COMPARISON_DATE, CONTACT_ID1, CONTACT_ID2, CONTACT_ID3, IS_SCREENING)
	VALUES (@REVIEW_ID, @IN_GROUP_ATTRIBUTE_ID, @SET_ID,
		@COMPARISON_DATE, @CONTACT_ID1, @CONTACT_ID2, @CONTACT_ID3, @Is_Screening)
	
	SET @NEW_COMPARISON_ID = @@IDENTITY
	
	IF (@IN_GROUP_ATTRIBUTE_ID IS NULL OR @IN_GROUP_ATTRIBUTE_ID = -1)
	BEGIN
	
		INSERT INTO tb_COMPARISON_ITEM_ATTRIBUTE (COMPARISON_ID, ITEM_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT, CONTACT_ID, SET_ID, IS_INCLUDED)
			
		SELECT DISTINCT @NEW_COMPARISON_ID, ia.ITEM_ID, ia.ATTRIBUTE_ID, 
			ia.ADDITIONAL_TEXT, tis.CONTACT_ID, tis.SET_ID
			, CASE 
				WHEN @Is_Screening != 1 then NULL
				WHEN tas.ATTRIBUTE_TYPE_ID = 10 then 1
				ELSE 0
			 END
			FROM TB_ITEM_ATTRIBUTE ia
			INNER JOIN TB_ITEM_SET tis ON tis.ITEM_SET_ID = ia.ITEM_SET_ID
				AND tis.SET_ID = @SET_ID
				AND tis.IS_COMPLETED = 'FALSE'
				AND ((@CONTACT_ID1 IS NULL OR (tis.CONTACT_ID = @CONTACT_ID1))
				OR (@CONTACT_ID2 IS NULL OR (tis.CONTACT_ID = @CONTACT_ID2))
				OR (@CONTACT_ID3 IS NULL OR (tis.CONTACT_ID = @CONTACT_ID3)))
			INNER JOIN TB_ITEM_REVIEW ir ON ir.ITEM_ID = ia.ITEM_ID
				AND ir.REVIEW_ID = @REVIEW_ID
				AND ir.IS_DELETED = 'FALSE'
			INNER JOIN TB_ATTRIBUTE_SET tas ON ia.ATTRIBUTE_ID =tas.ATTRIBUTE_ID and tas.SET_ID = @SET_ID
	END
	ELSE
	BEGIN
		INSERT INTO tb_COMPARISON_ITEM_ATTRIBUTE (COMPARISON_ID, ITEM_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT, CONTACT_ID, SET_ID, IS_INCLUDED)
			
		SELECT DISTINCT @NEW_COMPARISON_ID, ia.ITEM_ID, ia.ATTRIBUTE_ID, 
			ia.ADDITIONAL_TEXT, tis.CONTACT_ID, tis.SET_ID
			, CASE 
				WHEN @Is_Screening != 1 then NULL
				WHEN tas.ATTRIBUTE_TYPE_ID = 10 then 1
				ELSE 0
			 END
			FROM TB_ITEM_ATTRIBUTE ia
			INNER JOIN TB_ITEM_SET tis ON tis.ITEM_SET_ID = ia.ITEM_SET_ID
				AND tis.SET_ID = @SET_ID
				AND tis.IS_COMPLETED = 'FALSE'
				AND ((@CONTACT_ID1 IS NULL OR (tis.CONTACT_ID = @CONTACT_ID1))
				OR (@CONTACT_ID2 IS NULL OR (tis.CONTACT_ID = @CONTACT_ID2))
				OR (@CONTACT_ID3 IS NULL OR (tis.CONTACT_ID = @CONTACT_ID3)))
			INNER JOIN TB_ITEM_ATTRIBUTE IA_FILTER ON IA_FILTER.ITEM_ID = ia.ITEM_ID
				AND IA_FILTER.ATTRIBUTE_ID = @IN_GROUP_ATTRIBUTE_ID
				INNER JOIN TB_ITEM_SET IA_FILTER_ITEM_SET ON IA_FILTER_ITEM_SET.ITEM_SET_ID = IA_FILTER.ITEM_SET_ID
				AND IA_FILTER_ITEM_SET.IS_COMPLETED = 'TRUE'
			INNER JOIN TB_ITEM_REVIEW ir ON ir.ITEM_ID = ia.ITEM_ID
				AND ir.REVIEW_ID = @REVIEW_ID
				AND ir.IS_DELETED = 'FALSE'
			Inner Join TB_ATTRIBUTE_SET tas ON ia.ATTRIBUTE_ID =tas.ATTRIBUTE_ID and tas.SET_ID = @SET_ID
	
	END
			

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ComparisonInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ComparisonList]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	SELECT COMPARISON_ID, REVIEW_ID, IN_GROUP_ATTRIBUTE_ID, tb_COMPARISON.SET_ID, COMPARISON_DATE,
		CONTACT_ID1, CONTACT_ID2, CONTACT_ID3,
		CONTACT1.CONTACT_NAME CONTACT_NAME1, CONTACT2.CONTACT_NAME CONTACT_NAME2, CONTACT3.CONTACT_NAME CONTACT_NAME3,
		ATTRIBUTE_NAME, SET_NAME
	FROM tb_COMPARISON
		INNER JOIN TB_CONTACT CONTACT1 ON CONTACT1.CONTACT_ID = CONTACT_ID1
		INNER JOIN TB_CONTACT CONTACT2 ON CONTACT2.CONTACT_ID = CONTACT_ID2
		LEFT OUTER JOIN TB_CONTACT CONTACT3 ON CONTACT3.CONTACT_ID = CONTACT_ID3
		LEFT OUTER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = tb_COMPARISON.IN_GROUP_ATTRIBUTE_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = tb_COMPARISON.SET_ID
	WHERE REVIEW_ID = @REVIEW_ID
	ORDER BY COMPARISON_DATE

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ComparisonList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonScreeningComplete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ComparisonScreeningComplete]
(
	@COMPARISON_ID INT,
	@CONTACT_ID INT,
	@WHICH_REVIEWERS NVARCHAR(20)
	--@RECORDS_AFFECTED INT OUTPUT
)

As

--SET NOCOUNT ON

DECLARE @T1 TABLE --item_attribute for R1, R1 and R2 are relative to the sproc, could be any couple from 
	(
	  ITEM_ID BIGINT,
	  [STATE] char(1),
		  PRIMARY KEY (ITEM_ID, [STATE]) 
	)
DECLARE @T2 TABLE --item_attribute for R2
	(
	  ITEM_ID BIGINT,
	  [STATE] char(1),
		  PRIMARY KEY (ITEM_ID, [STATE]) 
	)
DECLARE @T1a2 table (ITEM_ID bigint primary key)--snapshot agreements 1 v 2
--declare @c1 int
--declare @c2 int


--If @WHICH_REVIEWERS = 'Complete1vs2'
--	BEGIN
--		select @c1 = CONTACT_ID1, @c2 = CONTACT_ID2 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END
--ELSE If @WHICH_REVIEWERS = 'Complete1vs3'
--	BEGIN
--		select @c1 = CONTACT_ID1, @c2 = CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END
--ELSE  --Complete2vs3
--	BEGIN
--		select @c1 = CONTACT_ID2, @c2 = CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END

insert into @T1
SELECT sub.ITEM_ID 
		 ,CASE 
			when [is incl] > 0 and [is ex] > 0 then 'B'
			when [is incl] > 0 then 'I'
			WHEN [is ex] > 0 then 'E'
			ELSE Null
		END
		from
		(select inc.ITEM_ID,  Sum(CASE inc.IS_INCLUDED when 1 then 1 else 0 end) [is incl]
			, Sum(CASE inc.IS_INCLUDED when 0 then 1 else 0 end) [is ex]
			from 
			tb_COMPARISON c
			left join tb_COMPARISON_ITEM_ATTRIBUTE inc on c.COMPARISON_ID = inc.COMPARISON_ID 
												and  inc.CONTACT_ID = 
													CASE  @WHICH_REVIEWERS
														WHEN 'Complete2vs3Sc' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID1
													END 
												and c.COMPARISON_ID = @COMPARISON_ID
			group by inc.ITEM_ID
		) sub
		where ITEM_ID is not null

insert into @T2
SELECT sub.ITEM_ID 
		 ,CASE 
			when [is incl] > 0 and [is ex] > 0 then 'B'
			when [is incl] > 0 then 'I'
			WHEN [is ex] > 0 then 'E'
			ELSE Null
		END
		from
		(select inc.ITEM_ID,  Sum(CASE inc.IS_INCLUDED when 1 then 1 else 0 end) [is incl]
			, Sum(CASE inc.IS_INCLUDED when 0 then 1 else 0 end) [is ex]
			from 
			tb_COMPARISON c
			left join tb_COMPARISON_ITEM_ATTRIBUTE inc on c.COMPARISON_ID = inc.COMPARISON_ID 
												and  inc.CONTACT_ID = 
													CASE  @WHICH_REVIEWERS
														WHEN 'Complete1vs2Sc' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID3
													END 
												and c.COMPARISON_ID = @COMPARISON_ID
			group by inc.ITEM_ID
		) sub
		where ITEM_ID is not null



insert into @T1a2
Select distinct t1.ITEM_ID from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.STATE = t2.STATE
	

--select * from @T1
--select * from @T2
--select * from @T1a2

Delete from @T1a2 where ITEM_ID in --remove all items that are already completed
(
	select tis.ITEM_ID from TB_ITEM_SET tis inner join 
		tb_COMPARISON c on tis.SET_ID = c.SET_ID and COMPARISON_ID = @COMPARISON_ID
		Inner join @T1a2 t1 on tis.ITEM_ID = t1.ITEM_ID and tis.IS_COMPLETED = 1
)

--select * from @T1a2


Update TB_ITEM_SET set IS_COMPLETED = 1 
	where CONTACT_ID = @CONTACT_ID
		and ITEM_SET_ID in
			(	
				select ITEM_SET_ID from TB_ITEM_SET tis 
				inner join tb_COMPARISON c on tis.SET_ID = c.SET_ID and c.COMPARISON_ID = @COMPARISON_ID
				inner join @T1a2 t on tis.ITEM_ID = t.ITEM_ID
			)

SELECT @@ROWCOUNT


GO
ALTER AUTHORIZATION ON [dbo].[st_ComparisonScreeningComplete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonScreeningStats]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ComparisonScreeningStats]
(
	@COMPARISON_ID INT
	,@Dis1v2 int OUTPUT
	,@Dis2v3 int OUTPUT
	,@Dis1v3 int OUTPUT
	,@CodesChanged bit OUTPUT
)
--with recompile
As

SET NOCOUNT ON
	DECLARE @T1 TABLE
		(
		  ITEM_ID BIGINT,
		  [STATE] char(1),
		  PRIMARY KEY (ITEM_ID, [STATE]) 
		)
	DECLARE @T2 TABLE
		(
		  ITEM_ID BIGINT,
		  [STATE] char(1),
		  PRIMARY KEY (ITEM_ID, [STATE]) 
		)

	DECLARE @T3 TABLE
		(
		  ITEM_ID BIGINT,
		  [STATE] char(1),
		  PRIMARY KEY (ITEM_ID, [STATE]) 
		)
	insert into @T1
	SELECT sub.ITEM_ID 
		 ,CASE 
			when [is incl] > 0 and [is ex] > 0 then 'B'
			when [is incl] > 0 then 'I'
			WHEN [is ex] > 0 then 'E'
			ELSE Null
		END
		from
		(select inc.ITEM_ID,  Sum(CASE inc.IS_INCLUDED when 1 then 1 else 0 end) [is incl]
			, Sum(CASE inc.IS_INCLUDED when 0 then 1 else 0 end) [is ex]
			from 
			tb_COMPARISON c
			left join tb_COMPARISON_ITEM_ATTRIBUTE inc on c.COMPARISON_ID = inc.COMPARISON_ID and inc.CONTACT_ID = c.CONTACT_ID1 --and inc.IS_INCLUDED = 1
		where c.COMPARISON_ID = @COMPARISON_ID
		group by inc.ITEM_ID) sub
		where ITEM_ID is not null
	order by ITEM_ID

	insert into @T2
	SELECT sub.ITEM_ID 
		 ,CASE 
			when [is incl] > 0 and [is ex] > 0 then 'B'
			when [is incl] > 0 then 'I'
			WHEN [is ex] > 0 then 'E'
			ELSE Null
		END
		from
		(select inc.ITEM_ID,  Sum(CASE inc.IS_INCLUDED when 1 then 1 else 0 end) [is incl]
			, Sum(CASE inc.IS_INCLUDED when 0 then 1 else 0 end) [is ex]
			from 
			tb_COMPARISON c
			left join tb_COMPARISON_ITEM_ATTRIBUTE inc on c.COMPARISON_ID = inc.COMPARISON_ID and inc.CONTACT_ID = c.CONTACT_ID2
		where c.COMPARISON_ID = @COMPARISON_ID
		group by inc.ITEM_ID) sub
		where ITEM_ID is not null
	order by ITEM_ID

	insert into @T3
	SELECT sub.ITEM_ID 
		 ,CASE 
			when [is incl] > 0 and [is ex] > 0 then 'B'
			when [is incl] > 0 then 'I'
			WHEN [is ex] > 0 then 'E'
			ELSE Null
		END
		from
		(select inc.ITEM_ID,  Sum(CASE inc.IS_INCLUDED when 1 then 1 else 0 end) [is incl]
			, Sum(CASE inc.IS_INCLUDED when 0 then 1 else 0 end) [is ex]
			from 
			tb_COMPARISON c
			left join tb_COMPARISON_ITEM_ATTRIBUTE inc on c.COMPARISON_ID = inc.COMPARISON_ID and inc.CONTACT_ID = c.CONTACT_ID3 
		where c.COMPARISON_ID = @COMPARISON_ID
		group by inc.ITEM_ID) sub
		where ITEM_ID is not null
	order by ITEM_ID

	--select * from @T1
	--select * from @T2
	--select * from @T3


	set @Dis1v2 = (select COUNT(t1.ITEM_ID) from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.STATE != t2.STATE)
	set @Dis2v3 = (select COUNT(t2.ITEM_ID) from @T2 t2 inner join @T3 t3 on t2.ITEM_ID = t3.ITEM_ID and t2.STATE != t3.STATE)
	set @Dis1v3 = (select COUNT(t1.ITEM_ID) from @T1 t1 inner join @T3 t3 on t1.ITEM_ID = t3.ITEM_ID and t1.STATE != t3.STATE)
	
	set @CodesChanged = (SELECT Case  when COUNT(distinct(ca.ATTRIBUTE_ID))> 0 then 1 else 0 end
								from tb_COMPARISON_ITEM_ATTRIBUTE ca
						inner join TB_ATTRIBUTE_SET tas on ca.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and ca.COMPARISON_ID = @COMPARISON_ID and tas.SET_ID = ca.SET_ID
							and (
									(ca.IS_INCLUDED = 1 and tas.ATTRIBUTE_TYPE_ID != 10)
									OR
									(ca.IS_INCLUDED = 0 and tas.ATTRIBUTE_TYPE_ID != 11)
								)
						)
SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ComparisonScreeningStats] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonStats]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ComparisonStats]
(
	@COMPARISON_ID INT,
	@Is_Screening bit OUTPUT
)
--with recompile
As

SET NOCOUNT ON

set @Is_Screening = (Select IS_SCREENING from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)

declare @n1 int , @n2 int , @n3 int --Total N items coded reviewer 1,2 & 3 (snapshot)
declare @c1 int = (select CONTACT_ID1 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
declare @c2 int = (select CONTACT_ID2 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
declare @c3 int = (select CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
declare @set int = (select SET_ID from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
DECLARE @T1 TABLE
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)
DECLARE @T1c TABLE --current attributes Reviewer1
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)
DECLARE @T2 TABLE
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)
DECLARE @T2c TABLE --current attributes Reviewer2
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)

DECLARE @T3 TABLE
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)
DECLARE @T3c TABLE --current attributes Reviewer3
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)
DECLARE @T1a2 table (ITEM_ID bigint primary key)--snapshot agreements 1 v 2
DECLARE @T1ca2 table (ITEM_ID bigint primary key)--current agreements 1 v 2
DECLARE @T1a3 table (ITEM_ID bigint primary key)--snapshot agreements 1 v 3
DECLARE @T1ca3 table (ITEM_ID bigint primary key)--current agreements 1 v 3
DECLARE @T2a3 table (ITEM_ID bigint primary key)--snapshot agreements 2 v 3
DECLARE @T2ca3 table (ITEM_ID bigint primary key)--current agreements 2 v 3
insert into @T1
select ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID and c.CONTACT_ID1 = cia.CONTACT_ID and c.COMPARISON_ID = @COMPARISON_ID
set @n1 = ( select count(distinct(item_id)) from @T1 )

insert into @T1c 
select t1.ITEM_ID, tia.ATTRIBUTE_ID from (select distinct ITEM_ID from @T1) t1 --only items in the comparison
	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @set and tis.CONTACT_ID = @c1
	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID

insert into @T2
select ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID and c.CONTACT_ID2 = cia.CONTACT_ID and c.COMPARISON_ID = @COMPARISON_ID
set @n2 = ( select count(distinct(item_id)) from @T2 )

insert into @T2c 
select t2.ITEM_ID, tia.ATTRIBUTE_ID from (select distinct ITEM_ID from @T2) t2 
	inner join TB_ITEM_SET tis on t2.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @set and tis.CONTACT_ID = @c2
	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID

insert into @T3
select ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID and c.CONTACT_ID3 = cia.CONTACT_ID and c.COMPARISON_ID = @COMPARISON_ID
set @n3 = ( select count(distinct(item_id)) from @T3 )

insert into @T3c 
select t3.ITEM_ID, tia.ATTRIBUTE_ID from (select distinct ITEM_ID from @T3) t3 
	inner join TB_ITEM_SET tis on t3.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @set and tis.CONTACT_ID = @c3
	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID


-- Total N items coded reviewer 1
select @n1 as 'Total N items coded reviewer 1'
-- Total N items coded reviewer 2
select @n2 as 'Total N items coded reviewer 2'
-- Total N items coded reviewer 3
select @n3 as 'Total N items coded reviewer 3'

-- Total N items coded reviewer 1 & 2
select count(distinct(t1.item_id)) as 'Total N items coded reviewer 1 & 2' from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID

-- Total disagreements 1vs2
--the inner join (IJ) selects only records in t2 where the second reviewer has applied a different code, but this does not guarantee coding as a whole is different, we still need to check if:
--a)	R1 has also coded with the same attribute found in t2 through IJ. If that’s the case, then we should not count this as a disagreement.
--b)	R2 has also coded with the attribute from t1
--The second outer join (OJ2), with the “where t2b.ATTRIBUTE_ID is null” clause checks for a). The first outer join (OJ1), with the “where t1b.ATTRIBUTE_ID is null” clause, checks  for b).
--So overall, the first join spots all possible 1:1 coding differences, the two outer joins get rid of meaningless lines (where the differences are cancelled by other records). 
select count(distinct(t1.item_id)) as 'Total disagreements 1vs2'
	from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
	left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

-- Total N items coded reviewer 2 & 3
select count(distinct(t2.item_id)) as 'Total N items coded reviewer 2 & 3' from @T2 t2 inner join @T3 t3 on t2.ITEM_ID = t3.ITEM_ID

-- Total disagreements 2vs3
select count(distinct(t2.item_id)) as 'Total disagreements 2vs3'
	from @T2 t2 inner join @T3 t3 on t2.ITEM_ID = t3.ITEM_ID and t2.ATTRIBUTE_ID != t3.ATTRIBUTE_ID
	left outer join @T3 t3b on t2.ATTRIBUTE_ID = t3b.ATTRIBUTE_ID and t2.ITEM_ID = t3b.ITEM_ID
	left outer join @T2 t2b on  t2b.ATTRIBUTE_ID = t3.ATTRIBUTE_ID and t2b.ITEM_ID = t3.ITEM_ID
	where t2b.ATTRIBUTE_ID is null or t3b.ATTRIBUTE_ID is null

-- Total N items coded reviewer 1 & 3
select count(distinct(t1.item_id)) as 'Total N items coded reviewer 1 & 3' from @T1 t1 inner join @T3 t3 on t1.ITEM_ID = t3.ITEM_ID

-- Total disagreements 1vs3
select count(distinct(t1.item_id)) as 'Total disagreements 1vs3'
	from @T1 t1 inner join @T3 t3 on t1.ITEM_ID = t3.ITEM_ID and t1.ATTRIBUTE_ID != t3.ATTRIBUTE_ID
	left outer join @T3 t3b on t1.ATTRIBUTE_ID = t3b.ATTRIBUTE_ID and t1.ITEM_ID = t3b.ITEM_ID
	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t3.ATTRIBUTE_ID and t1b.ITEM_ID = t3.ITEM_ID
	where t1b.ATTRIBUTE_ID is null or t3b.ATTRIBUTE_ID is null

--REAL AGREEMENTS: Combine items from R1 and R2 and get only those that are not currenlty disagreements
insert into @T1ca2
Select t1.item_id from @T1c t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
	inner join @T2c t2 on t1.ITEM_ID = t2.ITEM_ID
	except
	select distinct(t1.item_id) from @T1c t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
											-- then the double outer joins as before
	inner join @T2c t2 on t1.ITEM_ID = t2.ITEM_ID
				and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
	left outer join @T1c tia1a on tia1a.ITEM_ID = t1.ITEM_ID and t2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
	left outer join @T2c tia2a on tia2a.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
	where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null


--insert into @T1ca2
--Select t1.item_id from @T1 t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
--	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.CONTACT_ID = @c1 and tis.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID --need to join all the way to TB_ITEM_ATTRIBUTE to be 100% that some codes are present
--	inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID
--	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c2 and tis2.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID	
--	except
	--select * from @T1 t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
	--										-- then the double outer joins as before
	--inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.CONTACT_ID = @c1 and tis1.SET_ID = @set
	--inner join TB_ITEM_ATTRIBUTE tia1 on tis1.ITEM_SET_ID = tia1.ITEM_SET_ID 
	--inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID 
	--inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c2 and tis2.SET_ID = @set
	--inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID 
	--			and tia1.ATTRIBUTE_ID != tia2.ATTRIBUTE_ID
	--left outer join TB_ITEM_ATTRIBUTE tia1a on tis1.ITEM_SET_ID = tia1a.ITEM_SET_ID and tia1a.ITEM_ID = tis1.ITEM_ID and tia2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
	--left outer join TB_ITEM_ATTRIBUTE tia2a on tis2.ITEM_SET_ID = tia2a.ITEM_SET_ID and tia2a.ITEM_ID = tis2.ITEM_ID and tia1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
	--where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null

--COMPARISON AGREEMENTS: 1 V 2, uses the same techniques as before to find the list of all agreed-items according to the comparison snapshot
insert into @T1a2
Select distinct t1.ITEM_ID from @T1 t1 
	inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID
	except
select distinct(t1.item_id) from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
	left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

--REAL AGREEMENTS: Combine items from R1 and R3 and get only those that are not currenlty disagreements
insert into @T1ca3
Select t1.item_id from @T1c t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
	inner join @T3c t3 on t1.ITEM_ID = t3.ITEM_ID
	except
	select distinct(t1.item_id) from @T1c t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
											-- then the double outer joins as before
	inner join @T3c t3 on t1.ITEM_ID = t3.ITEM_ID
				and t1.ATTRIBUTE_ID != t3.ATTRIBUTE_ID
	left outer join @T1c tia1a on tia1a.ITEM_ID = t1.ITEM_ID and t3.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
	left outer join @T3c tia3a on tia3a.ITEM_ID = t3.ITEM_ID and t1.ATTRIBUTE_ID = tia3a.ATTRIBUTE_ID
	where tia1a.ATTRIBUTE_ID is null or tia3a.ATTRIBUTE_ID is null


----REAL AGREEMENTS: Combine items from R1 and R3 and get only those that are not currenlty disagreements
--insert into @T1ca3
--Select t1.item_id from @T1 t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
--	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.CONTACT_ID = @c1 and tis.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID --need to join all the way to TB_ITEM_ATTRIBUTE to be 100% that some codes are present
--	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
--	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID	
--	except
--	select distinct(t1.item_id) from @T1 t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
--											-- then the double outer joins as before
--	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.CONTACT_ID = @c1 and tis1.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia1 on tis1.ITEM_SET_ID = tia1.ITEM_SET_ID 
--	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID 
--	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID 
--				and tia1.ATTRIBUTE_ID != tia2.ATTRIBUTE_ID
--	left outer join TB_ITEM_ATTRIBUTE tia1a on tis1.ITEM_SET_ID = tia1a.ITEM_SET_ID and tia2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
--	left outer join TB_ITEM_ATTRIBUTE tia2a on tis2.ITEM_SET_ID = tia2a.ITEM_SET_ID and tia1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
--	where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null

--COMPARISON AGREEMENTS: 1 V 3, uses the same techniques as before to find the list of all agreed-items according to the comparison snapshot
insert into @T1a3
Select distinct t1.ITEM_ID from @T1 t1 
	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
	except
select distinct(t1.item_id) from @T1 t1 inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
	left outer join @T3 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

--REAL AGREEMENTS: Combine items from R2 and R3 and get only those that are not currenlty disagreements
insert into @T2ca3
Select t2.item_id from @T2c t2 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
	inner join @T3c t3 on t2.ITEM_ID = t3.ITEM_ID
	except
	select distinct(t2.item_id) from @T2c t2 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
											-- then the double outer joins as before
	inner join @T3c t3 on t2.ITEM_ID = t3.ITEM_ID
				and t2.ATTRIBUTE_ID != t3.ATTRIBUTE_ID
	left outer join @T2c tia2a on tia2a.ITEM_ID = t2.ITEM_ID and t3.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
	left outer join @T3c tia3a on tia3a.ITEM_ID = t3.ITEM_ID and t2.ATTRIBUTE_ID = tia3a.ATTRIBUTE_ID
	where tia2a.ATTRIBUTE_ID is null or tia3a.ATTRIBUTE_ID is null
	
----REAL AGREEMENTS: Combine items from R2 and R3 and get only those that are not currenlty disagreements
--insert into @T2ca3
--Select t1.item_id from @T2 t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
--	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.CONTACT_ID = @c2 and tis.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID --need to join all the way to TB_ITEM_ATTRIBUTE to be 100% that some codes are present
--	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
--	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID	
--	except
--	select distinct(t1.item_id) from @T2 t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
--											-- then the double outer joins as before
--	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.CONTACT_ID = @c2 and tis1.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia1 on tis1.ITEM_SET_ID = tia1.ITEM_SET_ID 
--	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID 
--	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID 
--				and tia1.ATTRIBUTE_ID != tia2.ATTRIBUTE_ID
--	left outer join TB_ITEM_ATTRIBUTE tia1a on tis1.ITEM_SET_ID = tia1a.ITEM_SET_ID and tia2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
--	left outer join TB_ITEM_ATTRIBUTE tia2a on tis2.ITEM_SET_ID = tia2a.ITEM_SET_ID and tia1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
--	where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null

--COMPARISON AGREEMENTS: 2 V 3, uses the same techniques as before to find the list of all agreed-items according to the comparison snapshot
insert into @T2a3
Select distinct t1.ITEM_ID from @T2 t1 
	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
	except
select distinct(t1.item_id) from @T2 t1 inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
	left outer join @T3 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
	left outer join @T2 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null


-- Are all Comparison Agreements currently completed OR are current agreements different from the snapshot agreements?
-- 1 V 2
Select Case when (COUNT(distinct ITEM_ID) = SUM(Completed)
				  OR 
					( select 
							Case when (SUM(sm.ss) > 0) then 1 --
							else 0
							end
						 from
						(
						Select COUNT(t1.ITEM_ID) ss from @T1a2 t1 
							left join @T1ca2 t2 on t1.ITEM_ID = t2.ITEM_ID
							where t2.ITEM_ID is null
						UNION
						Select COUNT(t1.ITEM_ID) ss from @T1ca2 t1 
							left join @T1a2 t2 on t1.ITEM_ID = t2.ITEM_ID
							where t2.ITEM_ID is null
						) AS sm
					) = 1
				  ) then 1 else 0 end 
				  as '1v2 lock-completion OR changed'
	from 
	(Select distinct t1.ITEM_ID, Case
								when (tis1.IS_COMPLETED = 1 ) then 1
								else 0
							end as Completed
	from @T1a2 t1 
	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.SET_ID = @set --joining on item and set, not on CONTACT_ID as item could be completed by anyone
	) a

-- Are all Comparison Agreements currently completed OR are current agreements different from the snapshot agreements?
-- 1 V 3
Select Case when (COUNT(distinct ITEM_ID) = SUM(Completed)
				  OR 
					( select 
							Case when (SUM(sm.ss) > 0) then 1 --
							else 0
							end
						 from
						(
						Select COUNT(t1.ITEM_ID) ss from @T1a3 t1 
							left join @T1ca3 t2 on t1.ITEM_ID = t2.ITEM_ID
							where t2.ITEM_ID is null
						UNION
						Select COUNT(t1.ITEM_ID) ss from @T1ca3 t1 
							left join @T1a3 t2 on t1.ITEM_ID = t2.ITEM_ID
							where t2.ITEM_ID is null
						) AS sm
					) = 1
				  ) then 1 else 0 end 
				  as '1v3 lock-completion OR changed'
	from 
	(Select distinct t1.ITEM_ID, Case
								when (tis1.IS_COMPLETED = 1 ) then 1
								else 0
							end as Completed
	from @T1a3 t1 
	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.SET_ID = @set --joining on item and set, not on CONTACT_ID as item could be completed by anyone
	) a

-- Are all Comparison Agreements currently completed OR are current agreements different from the snapshot agreements?
-- 2 V 3
Select Case when (COUNT(distinct ITEM_ID) = SUM(Completed)
				  OR 
					( select 
							Case when (SUM(sm.ss) > 0) then 1 --
							else 0
							end
						 from
						(
						Select COUNT(t1.ITEM_ID) ss from @T2a3 t1 
							left join @T2ca3 t2 on t1.ITEM_ID = t2.ITEM_ID
							where t2.ITEM_ID is null
						UNION
						Select COUNT(t1.ITEM_ID) ss from @T2ca3 t1 
							left join @T2a3 t2 on t1.ITEM_ID = t2.ITEM_ID
							where t2.ITEM_ID is null
						) AS sm
					) = 1
				  ) then 1 else 0 end 
				  as '2v3 lock-completion OR changed'
	from 
	(Select distinct t1.ITEM_ID, Case
								when (tis1.IS_COMPLETED = 1 ) then 1
								else 0
							end as Completed
	from @T2a3 t1 
	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.SET_ID = @set --joining on item and set, not on CONTACT_ID as item could be completed by anyone
	) a


GO
ALTER AUTHORIZATION ON [dbo].[st_ComparisonStats] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonStats.old]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ComparisonStats.old]
(
	@COMPARISON_ID INT
)
with recompile
As

SET NOCOUNT ON

-- Total N items coded reviewer 1
SELECT COUNT (DISTINCT ITEM_ID)
FROM tb_COMPARISON_ITEM_ATTRIBUTE cia
INNER JOIN tb_COMPARISON c ON c.CONTACT_ID1 = cia.CONTACT_ID and cia.COMPARISON_ID = c.COMPARISON_ID
WHERE cia.COMPARISON_ID = @COMPARISON_ID

-- Total N items coded reviewer 2
SELECT COUNT (DISTINCT ITEM_ID)
FROM tb_COMPARISON_ITEM_ATTRIBUTE cia
INNER JOIN tb_COMPARISON c ON c.CONTACT_ID2 = cia.CONTACT_ID and cia.COMPARISON_ID = c.COMPARISON_ID
WHERE cia.COMPARISON_ID = @COMPARISON_ID

-- Total N items coded reviewer 3
SELECT COUNT (DISTINCT ITEM_ID)
FROM tb_COMPARISON_ITEM_ATTRIBUTE cia
INNER JOIN tb_COMPARISON c ON c.CONTACT_ID3 = cia.CONTACT_ID and cia.COMPARISON_ID = c.COMPARISON_ID
WHERE cia.COMPARISON_ID = @COMPARISON_ID

-- Total that both 1 and 2 have coded
SELECT COUNT (DISTINCT CIA1.ITEM_ID)
FROM TB_COMPARISON_ITEM_ATTRIBUTE CIA1
INNER JOIN tb_COMPARISON_ITEM_ATTRIBUTE CIA2 ON CIA1.ITEM_ID = CIA2.ITEM_ID
INNER JOIN tb_COMPARISON COMP1 ON COMP1.CONTACT_ID1 = CIA1.CONTACT_ID AND COMP1.COMPARISON_ID = @COMPARISON_ID
INNER JOIN tb_COMPARISON COMP2 ON COMP2.CONTACT_ID2 = CIA2.CONTACT_ID AND COMP2.COMPARISON_ID = @COMPARISON_ID
WHERE CIA1.COMPARISON_ID = @COMPARISON_ID AND
	CIA2.COMPARISON_ID = @COMPARISON_ID

DECLARE @TT TABLE
	(
	  ITEM_ID BIGINT ,
	  ATTRIBUTE_ID BIGINT
	)
	
INSERT INTO @TT(ITEM_ID, ATTRIBUTE_ID)
SELECT ITEM_ID, ATTRIBUTE_ID
FROM TB_COMPARISON_ITEM_ATTRIBUTE
INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
	AND TB_COMPARISON.CONTACT_ID1 = TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID
	AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
WHERE tb_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID = @COMPARISON_ID
EXCEPT
SELECT ITEM_ID, ATTRIBUTE_ID
FROM TB_COMPARISON_ITEM_ATTRIBUTE
INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
	AND TB_COMPARISON.CONTACT_ID2 = TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID
	AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
WHERE tb_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID = @COMPARISON_ID
ORDER BY ITEM_ID, ATTRIBUTE_ID

-- Make sure that both have coded each item
DELETE FROM @TT WHERE NOT ITEM_ID IN
(
SELECT DISTINCT CIA1.ITEM_ID
FROM TB_COMPARISON_ITEM_ATTRIBUTE CIA1
INNER JOIN tb_COMPARISON_ITEM_ATTRIBUTE CIA2 ON CIA1.ITEM_ID = CIA2.ITEM_ID
INNER JOIN tb_COMPARISON COMP1 ON COMP1.CONTACT_ID1 = CIA1.CONTACT_ID AND COMP1.COMPARISON_ID = @COMPARISON_ID
INNER JOIN tb_COMPARISON COMP2 ON COMP2.CONTACT_ID2 = CIA2.CONTACT_ID AND COMP2.COMPARISON_ID = @COMPARISON_ID
WHERE CIA1.COMPARISON_ID = @COMPARISON_ID AND
	CIA2.COMPARISON_ID = @COMPARISON_ID
)

-- Total disagreements 1vs2
SELECT COUNT(DISTINCT ITEM_ID) FROM @TT

DELETE FROM @TT

-- Total that both 2 and 3 have coded
SELECT COUNT (DISTINCT CIA1.ITEM_ID)
FROM TB_COMPARISON_ITEM_ATTRIBUTE CIA1
INNER JOIN tb_COMPARISON_ITEM_ATTRIBUTE CIA2 ON CIA1.ITEM_ID = CIA2.ITEM_ID
INNER JOIN tb_COMPARISON COMP1 ON COMP1.CONTACT_ID2 = CIA1.CONTACT_ID AND COMP1.COMPARISON_ID = @COMPARISON_ID
INNER JOIN tb_COMPARISON COMP2 ON COMP2.CONTACT_ID3 = CIA2.CONTACT_ID AND COMP2.COMPARISON_ID = @COMPARISON_ID
WHERE CIA1.COMPARISON_ID = @COMPARISON_ID AND
	CIA2.COMPARISON_ID = @COMPARISON_ID

INSERT INTO @TT(ITEM_ID, ATTRIBUTE_ID)
SELECT ITEM_ID, ATTRIBUTE_ID
FROM TB_COMPARISON_ITEM_ATTRIBUTE
INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
	AND TB_COMPARISON.CONTACT_ID2 = TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID
	AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
EXCEPT
SELECT ITEM_ID, ATTRIBUTE_ID
FROM TB_COMPARISON_ITEM_ATTRIBUTE
INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
	AND TB_COMPARISON.CONTACT_ID3 = TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID
	AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
ORDER BY ITEM_ID, ATTRIBUTE_ID

-- Make sure that both have coded each item
DELETE FROM @TT WHERE NOT ITEM_ID IN
(
SELECT DISTINCT CIA1.ITEM_ID
FROM TB_COMPARISON_ITEM_ATTRIBUTE CIA1
INNER JOIN tb_COMPARISON_ITEM_ATTRIBUTE CIA2 ON CIA1.ITEM_ID = CIA2.ITEM_ID
INNER JOIN tb_COMPARISON COMP1 ON COMP1.CONTACT_ID2 = CIA1.CONTACT_ID AND COMP1.COMPARISON_ID = @COMPARISON_ID
INNER JOIN tb_COMPARISON COMP2 ON COMP2.CONTACT_ID3 = CIA2.CONTACT_ID AND COMP2.COMPARISON_ID = @COMPARISON_ID
WHERE CIA1.COMPARISON_ID = @COMPARISON_ID AND
	CIA2.COMPARISON_ID = @COMPARISON_ID
)

-- Total disagreements 2vs3
SELECT COUNT(DISTINCT ITEM_ID) FROM @TT

DELETE FROM @TT

-- Total being compared 1vs3
SELECT COUNT (DISTINCT CIA1.ITEM_ID)
FROM TB_COMPARISON_ITEM_ATTRIBUTE CIA1
INNER JOIN tb_COMPARISON_ITEM_ATTRIBUTE CIA2 ON CIA1.ITEM_ID = CIA2.ITEM_ID
INNER JOIN tb_COMPARISON COMP1 ON COMP1.CONTACT_ID1 = CIA1.CONTACT_ID AND COMP1.COMPARISON_ID = @COMPARISON_ID
INNER JOIN tb_COMPARISON COMP2 ON COMP2.CONTACT_ID3 = CIA2.CONTACT_ID AND COMP2.COMPARISON_ID = @COMPARISON_ID
WHERE CIA1.COMPARISON_ID = @COMPARISON_ID AND
	CIA2.COMPARISON_ID = @COMPARISON_ID

INSERT INTO @TT(ITEM_ID, ATTRIBUTE_ID)
SELECT ITEM_ID, ATTRIBUTE_ID
FROM TB_COMPARISON_ITEM_ATTRIBUTE
INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
	AND TB_COMPARISON.CONTACT_ID1 = TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID
	AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
EXCEPT
SELECT ITEM_ID, ATTRIBUTE_ID
FROM TB_COMPARISON_ITEM_ATTRIBUTE
INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
	AND TB_COMPARISON.CONTACT_ID3 = TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID
	AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
ORDER BY ITEM_ID, ATTRIBUTE_ID

-- Make sure that both have coded each item
DELETE FROM @TT WHERE NOT ITEM_ID IN
(
SELECT DISTINCT CIA1.ITEM_ID
FROM TB_COMPARISON_ITEM_ATTRIBUTE CIA1
INNER JOIN tb_COMPARISON_ITEM_ATTRIBUTE CIA2 ON CIA1.ITEM_ID = CIA2.ITEM_ID
INNER JOIN tb_COMPARISON COMP1 ON COMP1.CONTACT_ID1 = CIA1.CONTACT_ID AND COMP1.COMPARISON_ID = @COMPARISON_ID
INNER JOIN tb_COMPARISON COMP2 ON COMP2.CONTACT_ID3 = CIA2.CONTACT_ID AND COMP2.COMPARISON_ID = @COMPARISON_ID
WHERE CIA1.COMPARISON_ID = @COMPARISON_ID AND
	CIA2.COMPARISON_ID = @COMPARISON_ID
)

-- Total disagreements 1vs3
SELECT COUNT(DISTINCT ITEM_ID) FROM @TT

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ComparisonStats.old] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_Contact_Review_Role_Insert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Sergio>
-- Create date: <03/03/10>
-- Description:	<Add role to a user on a given review, user must already belong to a review, returns 1 on success>
-- =============================================
CREATE PROCEDURE [dbo].[st_Contact_Review_Role_Insert] 
	-- Add the parameters for the stored procedure here
	@Review_ID int = 0, 
	@Contact_ID int = 0,
	@Role nvarchar(50) = '',
	@Result bit OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SET @Result = 0
	DECLARE @R_C_ID int
    -- First, check if user has access to review
    set @R_C_ID = (SELECT REVIEW_CONTACT_ID
	FROM TB_REVIEW_CONTACT
	WHERE REVIEW_ID = @Review_ID AND CONTACT_ID = @Contact_ID)
	IF @@ROWCOUNT > 0 --user does have access
	BEGIN
		--second check if user already has selected role
		SELECT REVIEW_ID, ROLE_NAME as [ROLE]
		FROM TB_REVIEW_CONTACT
			INNER JOIN TB_CONTACT_REVIEW_ROLE 
			on TB_CONTACT_REVIEW_ROLE.REVIEW_CONTACT_ID = TB_REVIEW_CONTACT.REVIEW_CONTACT_ID
		WHERE REVIEW_ID = @Review_ID AND CONTACT_ID = @Contact_ID and TB_CONTACT_REVIEW_ROLE.ROLE_NAME = @Role
		IF @@ROWCOUNT < 1 -- user does not have selected role in the review
		BEGIN --try to add role
			INSERT INTO TB_CONTACT_REVIEW_ROLE(REVIEW_CONTACT_ID,ROLE_NAME)
			VALUES (@R_C_ID, @Role)
			IF @@ROWCOUNT = 1 --insert was successful mark it in the return value
			BEGIN
				SET @Result = 1
			END
		END
	END
		
	
END

GO
ALTER AUTHORIZATION ON [dbo].[st_Contact_Review_Role_Insert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ContactAdminLoginReview]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ContactAdminLoginReview]
(
	@userId int,
	@reviewId int,
	@GUI uniqueidentifier OUTPUT
)

As
declare @chk int
set @chk = (select count (CONTACT_ID) from TB_CONTACT where IS_SITE_ADMIN = 1 and CONTACT_ID = @userId)
if @chk != 1
begin
	RETURN --do nothing if user is not actually a site admin
end

SELECT  TB_REVIEW.REVIEW_ID, 'AdminUser' as [ROLE], 
		( CASE WHEN sl2.[EXPIRY_DATE] is not null
				and sl2.[EXPIRY_DATE] > TB_REVIEW.[EXPIRY_DATE]
					then sl2.[EXPIRY_DATE]
				else TB_REVIEW.[EXPIRY_DATE]
				end
		) as REVIEW_EXP, 
		(SELECT  CASE when sl.[EXPIRY_DATE] is not null 
				and sl.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then sl.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end
				from TB_CONTACT c 
				Left outer join TB_SITE_LIC_CONTACT lc on lc.CONTACT_ID = c.CONTACT_ID
				Left outer join TB_SITE_LIC sl on lc.SITE_LIC_ID = sl.SITE_LIC_ID
				where c.CONTACT_ID = @userId
		) as CONTACT_EXP,
		FUNDER_ID
		
FROM TB_REVIEW 
	left outer join TB_SITE_LIC_REVIEW lr on TB_REVIEW.REVIEW_ID = lr.REVIEW_ID
	left outer join TB_SITE_LIC sl2 on lr.SITE_LIC_ID = sl2.SITE_LIC_ID
	
WHERE TB_REVIEW.REVIEW_ID = @reviewId 

IF @@ROWCOUNT >= 1 
	BEGIN
	DECLARE	@return_value int,
			@GUID uniqueidentifier
			
	EXEC	@return_value = [ReviewerAdmin].[dbo].[st_LogonTicket_Insert]
			@Contact_ID = @userId,
			@Review_ID = @reviewId,
			@GUID = @GUI OUTPUT
	SELECT	@GUI as N'@GUID'
	END


GO
ALTER AUTHORIZATION ON [dbo].[st_ContactAdminLoginReview] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ContactLogin]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ContactLogin]
(
	@userName  varchar(50)	
	,@Password varchar(50)
	
)
--note the GRACE_EXP field, how many days we add to EXPIRY_DATE defines how long is the grace period for the whole of ER4.
--during the grace period users can log on ER4 but will have read only access.
As


--first check if the username/pw are correct
declare @chek int = (select count(Contact_id)  from TB_CONTACT c where c.USERNAME = @userName and HASHBYTES('SHA1', @Password + FLAVOUR) = PWASHED AND c.EXPIRY_DATE is not null)
if @chek = 1
BEGIN
	--second make sure old now stale ArchieStatus and ArchieCode are discharged, no matter what (this is a first logon via ER4 credentials)
	UPDATE TB_CONTACT SET  LAST_ARCHIE_CODE = null, LAST_ARCHIE_STATE = null
		where USERNAME = @userName and HASHBYTES('SHA1', @Password + FLAVOUR) = PWASHED AND EXPIRY_DATE is not null
	--third get some user info
	Select c.CONTACT_ID, c.contact_name, --c.Password, 
		DATEADD(m, 2, 
				( CASE when sl.[EXPIRY_DATE] is not null 
					and sl.[EXPIRY_DATE] > c.[EXPIRY_DATE]
						then sl.[EXPIRY_DATE]
					else c.[EXPIRY_DATE]
					end
				)) as GRACE_EXP,
		[TYPE], IS_SITE_ADMIN
		, CASE when c.ARCHIE_ID is null then 0
			ELSE 1
			END
		AS IS_COCHRANE_USER
		  /* TB_CONTACT.[Role] */
	From TB_CONTACT c
	Left outer join TB_SITE_LIC_CONTACT lc on lc.CONTACT_ID = c.CONTACT_ID
	Left outer join TB_SITE_LIC sl on lc.SITE_LIC_ID = sl.SITE_LIC_ID
	Where c.UserName = @userName and c.EXPIRY_DATE is not null
END

GO
ALTER AUTHORIZATION ON [dbo].[st_ContactLogin] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ContactLoginReview]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ContactLoginReview]
(
	@userId int,
	@reviewId int,
	@IsArchieUser bit,
	@GUI uniqueidentifier OUTPUT
)

As
if @IsArchieUser = 1
begin
--we need to make sure user has records in TB_REVIEW_CONTACT and TB_CONTACT_REVIEW_ROLE
	declare @ck int = (select count(crr.ROLE_NAME) from TB_REVIEW_CONTACT rc 
		inner join TB_CONTACT_REVIEW_ROLE crr on rc.REVIEW_ID = @reviewId and rc.CONTACT_ID = @userId and crr.REVIEW_CONTACT_ID = rc.REVIEW_CONTACT_ID
		)
	if @ck < 1
	BEGIN
		DECLARE @NEW_CONTACT_REVIEW_ID INT
		INSERT INTO TB_REVIEW_CONTACT(CONTACT_ID, REVIEW_ID)
		VALUES (@userId, @reviewId)
		SET @NEW_CONTACT_REVIEW_ID = @@IDENTITY
		INSERT INTO TB_CONTACT_REVIEW_ROLE(REVIEW_CONTACT_ID, ROLE_NAME)
		VALUES(@NEW_CONTACT_REVIEW_ID, 'RegularUser')
	END
end
SELECT TB_REVIEW.REVIEW_ID, TB_REVIEW.ARCHIE_ID, ROLE_NAME as [ROLE], 
		( CASE WHEN sl2.[EXPIRY_DATE] is not null
				and sl2.[EXPIRY_DATE] > TB_REVIEW.[EXPIRY_DATE]
					then sl2.[EXPIRY_DATE]
				else TB_REVIEW.[EXPIRY_DATE]
				end
		) as REVIEW_EXP, 
		( CASE when sl.[EXPIRY_DATE] is not null 
				and sl.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then sl.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end
		) as CONTACT_EXP,
		FUNDER_ID, tb_review.ARCHIE_ID, IS_CHECKEDOUT_HERE
		
FROM TB_REVIEW_CONTACT
	INNER JOIN TB_REVIEW on TB_REVIEW_CONTACT.REVIEW_ID = TB_REVIEW.REVIEW_ID
	INNER JOIN TB_CONTACT c on TB_REVIEW_CONTACT.CONTACT_ID = c.CONTACT_ID
	INNER JOIN TB_CONTACT_REVIEW_ROLE on TB_CONTACT_REVIEW_ROLE.REVIEW_CONTACT_ID = TB_REVIEW_CONTACT.REVIEW_CONTACT_ID
	Left outer join TB_SITE_LIC_CONTACT lc on lc.CONTACT_ID = c.CONTACT_ID
	Left outer join TB_SITE_LIC sl on lc.SITE_LIC_ID = sl.SITE_LIC_ID
	left outer join TB_SITE_LIC_REVIEW lr on TB_REVIEW.REVIEW_ID = lr.REVIEW_ID
	left outer join TB_SITE_LIC sl2 on lr.SITE_LIC_ID = sl2.SITE_LIC_ID
	
WHERE TB_REVIEW.REVIEW_ID = @reviewId AND c.CONTACT_ID = @userId

IF @@ROWCOUNT >= 1 
	BEGIN
	DECLARE	@return_value int,
			@GUID uniqueidentifier
			
	EXEC	@return_value = [ReviewerAdmin].[dbo].[st_LogonTicket_Insert]
			@Contact_ID = @userId,
			@Review_ID = @reviewId,
			@GUID = @GUI OUTPUT
	SELECT	@GUI as N'@GUID'
	END


GO
ALTER AUTHORIZATION ON [dbo].[st_ContactLoginReview] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ContactPasswordFromID]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ContactPasswordFromID]
(
	@ID int,
	@GUI uniqueidentifier
)

As
Select c.Password  
From TB_CONTACT c
	inner join ReviewerAdmin.dbo.TB_LOGON_TICKET t on c.CONTACT_ID = t.CONTACT_ID and t.STATE = 1
Where c.CONTACT_ID = @ID and t.TICKET_GUID = @GUI


GO
ALTER AUTHORIZATION ON [dbo].[st_ContactPasswordFromID] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_DiagramDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_DiagramDelete]
(
	@DIAGRAM_ID INT
)

As

SET NOCOUNT ON

	DELETE FROM TB_DIAGRAM
		WHERE DIAGRAM_ID = @DIAGRAM_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_DiagramDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_DiagramInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_DiagramInsert]
(
	@REVIEW_ID INT,
	@DIAGRAM_NAME NVARCHAR(255),
	@DIAGRAM_DETAIL NVARCHAR(MAX),
	@NEW_DIAGRAM_ID INT OUTPUT
)

As

SET NOCOUNT ON

	INSERT INTO TB_DIAGRAM(REVIEW_ID, DIAGRAM_NAME, DIAGRAM_DETAIL)
	VALUES(@REVIEW_ID, @DIAGRAM_NAME, @DIAGRAM_DETAIL)

	SET @NEW_DIAGRAM_ID = @@IDENTITY

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_DiagramInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_DiagramList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_DiagramList]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	SELECT * FROM TB_DIAGRAM
		WHERE REVIEW_ID = @REVIEW_ID
		ORDER BY DIAGRAM_NAME

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_DiagramList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_DiagramUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_DiagramUpdate]
(
	@DIAGRAM_NAME NVARCHAR(255),
	@DIAGRAM_DETAIL NVARCHAR(MAX),
	@DIAGRAM_ID INT
)

As

SET NOCOUNT ON

	UPDATE TB_DIAGRAM
		SET DIAGRAM_NAME = @DIAGRAM_NAME,
			DIAGRAM_DETAIL = @DIAGRAM_DETAIL
		WHERE DIAGRAM_ID = @DIAGRAM_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_DiagramUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_Extract_Terms]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		S
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[st_Extract_Terms] 
	-- Add the parameters for the stored procedure here
	@IDs nvarchar(max) = ''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DECLARE @ui uniqueidentifier 
	declare @cmd varchar(1000)
	declare @tres TABLE
	(
		xxx nvarchar(4000)
	)
	DECLARE @TT TABLE
	(
	  TERM nvarchar(128),
	  SCORE float
	)
	SET @ui = NEWID()
	INSERT into TB_TERM_EXTR_T_MAP (ITEM_ID, EXTR_UI)
		SELECT ITEM_ID, @ui from dbo.TB_ITEM 
		WHERE ITEM_ID in 
			(SELECT VALUE FROM dbo.fn_Split_int(@IDs, ',')) AND ABSTRACT != ''
	select @cmd = 'dtexec /DT "File System\TermLookupS"'
	select @cmd = @cmd + ' /Rep N  /SET \Package.Variables[User::UI].Properties[Value];"' + CAST(@ui as varchar(max))+ '"' 
	--this is a dirty trick to prevent the verbose output of SSIS execution to be passed on as results
	INSERT INTO @tres EXEC xp_cmdshell @cmd
	INSERT INTO @TT
		SELECT TERM, SCORE from dbo.TB_ITEM_TERM_DICTIONARY t
		WHERE t.UI = @ui
	--got the results from the SSIS package, clear them from the table used as a temporary store
	DELETE from dbo.TB_ITEM_TERM_DICTIONARY WHERE @ui = UI
	--delete also the source table
	DELETE from dbo.TB_TERM_EXTR_T_MAP WHERE @ui = EXTR_UI
	--pass on the results to the calling reader
	SELECT * FROM @TT
END

GO
ALTER AUTHORIZATION ON [dbo].[st_Extract_Terms] TO  SCHEMA OWNER 
GO


GRANT EXECUTE ON [dbo].[st_Extract_Terms] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[st_Generate_st_TempTermExtractionItemList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_Generate_st_TempTermExtractionItemList]
(
	@ITEMS NVARCHAR(max)
)

As


SET NOCOUNT ON

DELETE FROM TB_ITEM_TERM_DICTIONARY
DELETE FROM TB_ITEM_TERM

exec ('
ALTER procedure [dbo].st_TempTermExtractionItemList
As

SET NOCOUNT ON

SELECT I.ITEM_ID, I.ABSTRACT FROM TB_ITEM I
INNER JOIN dbo.fn_Split_int(''' + @ITEMS + ''', '','') ITEMS
ON I.ITEM_ID = ITEMS.value
WHERE I.ABSTRACT != ''''

SET NOCOUNT OFF
')

EXEC msdb.dbo.sp_start_job N'RunCreateDictionary'
WAITFOR DELAY '0:0:10' -- HORRIBLE, but works for now. (Otherwise, the table is queried before anything is entered)

GO
ALTER AUTHORIZATION ON [dbo].[st_Generate_st_TempTermExtractionItemList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_Generate_st_TempTermExtractionSelectedItems]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_Generate_st_TempTermExtractionSelectedItems]
(
	@ITEMS NVARCHAR(max)
)

As

SET NOCOUNT ON

exec ('
ALTER procedure [dbo].st_TempTermExtractionSelectedItems
As

SET NOCOUNT ON

SELECT I.ITEM_ID, I.ABSTRACT FROM TB_ITEM I
INNER JOIN dbo.fn_Split_int(''' + @ITEMS + ''', '','') ITEMS
ON I.ITEM_ID = ITEMS.value
WHERE I.ABSTRACT != ''''


SET NOCOUNT OFF

')

GO
ALTER AUTHORIZATION ON [dbo].[st_Generate_st_TempTermExtractionSelectedItems] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_Item]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_Item]
(
	@REVIEW_ID INT,
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON

	SELECT I.ITEM_ID, I.[TYPE_ID], [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENTAUTHORS,
		IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID, DOI, KEYWORDS
	FROM TB_ITEM I

	-- Limit to a given review
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]

	WHERE I.ITEM_ID = @ITEM_ID


SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_Item] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeAutoReconcile]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAttributeAutoReconcile]
(
	@ITEM_ID BIGINT,
	@SET_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@RECONCILLIATION_TYPE nvarchar(10),
	@N_PEOPLE int,
	@AUTO_EXCLUDE bit,
	@CONTACT_ID int
)

As
SET NOCOUNT ON

DECLARE @COUNT_RECS INT = 0
DECLARE @ITEM_SET_ID INT = 0

IF (@RECONCILLIATION_TYPE = 'no compl')
BEGIN
	SET @N_PEOPLE = 99 --(i.e. we don't do anything - none of the rest is executed)
END
ELSE
BEGIN

	-- **************** STAGE 1: GATHER DATA ON WHETHER RULES FOR AUTO-RECONCILLIATION ARE MET ********************

	IF (@RECONCILLIATION_TYPE = 'Single')
	BEGIN
		SET @COUNT_RECS = 99 -- i.e. we go through to automatic exclude check

		SELECT TOP(1) @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET
			WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID
	END
	
	IF (@RECONCILLIATION_TYPE = 'auto code') -- agreement at the code level
	BEGIN
		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE
			WHERE ITEM_ID = @ITEM_ID AND ATTRIBUTE_ID = @ATTRIBUTE_ID
		IF (@COUNT_RECS >= @N_PEOPLE)
		BEGIN
			SELECT TOP(1) @ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID FROM TB_ITEM_SET
				INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID
				WHERE TB_ITEM_SET.ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND CONTACT_ID = @CONTACT_ID
		END
		ELSE
		BEGIN
			SET @ITEM_SET_ID = 0
		END
	END

	-- one person has to tick 'include' for it to be included.-- N people agreeing on exclude if nobody has ticked 'include' before this threshold is met
	IF (@RECONCILLIATION_TYPE = 'auto safet') 
	BEGIN
		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 -- is anything included?

		SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 AND CONTACT_ID = @CONTACT_ID
		IF (@COUNT_RECS > 0)
		BEGIN
			SET @COUNT_RECS = @N_PEOPLE
		END
		ELSE
		BEGIN
			-- IF NO INCLUDE IS TICKED, HAVE N PEOPLE TICKED EXCLUDE? IF SO, WE DEFAULT TO THIS
			SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 -- EXCLUDED
			SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11  AND CONTACT_ID = @CONTACT_ID
		END
		IF (@COUNT_RECS < @N_PEOPLE)
		BEGIN
			SET @ITEM_SET_ID = 0
		END
	END
	
	 -- agreement at the include / exclude level
	IF (@RECONCILLIATION_TYPE = 'auto excl')
	BEGIN
		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 -- INCLUDED
		SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10  AND CONTACT_ID = @CONTACT_ID

		IF (@COUNT_RECS < @N_PEOPLE)
		BEGIN
			SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 -- EXCLUDED
			SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11  AND CONTACT_ID = @CONTACT_ID
		END
	END

	

	-- *************************** STAGE 2: AUTO-RECONCILE AND AUTO-COMPLETE ***************************

	IF (@COUNT_RECS >= @N_PEOPLE) AND (@RECONCILLIATION_TYPE != 'Single') -- AUTO-RECONCILE (COMPLETE) WHERE RULES MET
	BEGIN
		DECLARE @CHECK_NONE_COMPLETED INT = 0
		SELECT COUNT(*) FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND IS_COMPLETED = 'TRUE'

		IF (@CHECK_NONE_COMPLETED = 0)
		BEGIN
			UPDATE TB_ITEM_SET
				SET IS_COMPLETED = 'TRUE'
				WHERE ITEM_SET_ID = @ITEM_SET_ID
		END
	END
	ELSE -- RULES FOR AUTO-COMPLETING ARE NOT MET, SO WE REMOVE THE SCREENING LOCK SO SOMEONE ELSE CAN SCREEN THIS ITEM
	BEGIN
		UPDATE TB_TRAINING_ITEM
			SET CONTACT_ID_CODING = 0, WHEN_LOCKED = NULL
			WHERE CONTACT_ID_CODING = @CONTACT_ID AND ITEM_ID = @ITEM_ID
	END
	IF (@AUTO_EXCLUDE = 'TRUE' AND @ITEM_SET_ID > 0 and @COUNT_RECS >= @N_PEOPLE) -- AUTO EXCLUDE WHERE RULES MET
	BEGIN
		-- SECOND, AUTO INCLUDE / EXCLUDE
		DECLARE @IS_INCLUDED BIT = 'TRUE'
		SELECT TOP(1) @IS_INCLUDED = CASE WHEN ATTRIBUTE_TYPE_ID = 11 THEN 'FALSE' ELSE 'TRUE' END
			FROM TB_ATTRIBUTE_SET
				INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
				WHERE ITEM_SET_ID = @ITEM_SET_ID
		UPDATE TB_ITEM_REVIEW
			SET IS_INCLUDED = @IS_INCLUDED
			WHERE ITEM_ID = @ITEM_ID
	END
END


SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeAutoReconcile] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeAutoReconcileDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAttributeAutoReconcileDelete]
(
	@ITEM_ID BIGINT,
	@SET_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@RECONCILLIATION_TYPE nvarchar(10),
	@N_PEOPLE int,
	@AUTO_EXCLUDE bit,
	@CONTACT_ID int
)

As
SET NOCOUNT ON

DECLARE @COUNT_RECS INT = 0
DECLARE @ITEM_SET_ID INT = 0

IF (@RECONCILLIATION_TYPE = 'no compl')
BEGIN
	SET @N_PEOPLE = 99 --(i.e. we don't do anything - none of the rest is executed)
END
ELSE
BEGIN

	-- **************** STAGE 1: GATHER DATA ON WHETHER RULES FOR AUTO-RECONCILLIATION ARE MET ********************

	IF (@RECONCILLIATION_TYPE = 'Single')
	BEGIN
		SET @COUNT_RECS = 99 -- i.e. we go through to automatic exclude check

		SELECT TOP(1) @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET
			WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID
	END
	
	IF (@RECONCILLIATION_TYPE = 'auto code') -- agreement at the code level
	BEGIN
		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE
			WHERE ITEM_ID = @ITEM_ID AND ATTRIBUTE_ID = @ATTRIBUTE_ID

		IF (@COUNT_RECS < @N_PEOPLE)
		BEGIN
			SELECT TOP(1) @ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID FROM TB_ITEM_SET
				INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID
				WHERE TB_ITEM_SET.ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND IS_COMPLETED = 'TRUE' AND CONTACT_ID = @CONTACT_ID

			IF (@ITEM_SET_ID = 0)
			BEGIN -- WE TRY TO GET A COMPLETED ITEM_SET RECORD, BUT THERE MAY NOT BE ONE!
				SELECT TOP(1) @ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID FROM TB_ITEM_SET
					INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID
					WHERE TB_ITEM_SET.ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND CONTACT_ID = @CONTACT_ID
			END
		END
		ELSE
		BEGIN
			SET @ITEM_SET_ID = 0
		END
	END

	-- one person has to tick 'include' for it to be included.-- N people agreeing on exclude if nobody has ticked 'include' before this threshold is met
	IF (@RECONCILLIATION_TYPE = 'auto safet') 
	BEGIN
		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 -- is anything included?

		SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 AND IS_COMPLETED = 'TRUE' AND CONTACT_ID = @CONTACT_ID
		IF (@COUNT_RECS > 0) -- I.E. THE RULES FOR AUTO INCLUSION ARE STILL MET
		BEGIN
			--SET @COUNT_RECS = @N_PEOPLE
			SET @ITEM_SET_ID = 0
		END
		ELSE
		BEGIN
			-- IF NO INCLUDE IS TICKED, HAVE N PEOPLE TICKED EXCLUDE? IF SO, WE DEFAULT TO THIS
			SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 -- EXCLUDED
			SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 AND IS_COMPLETED = 'TRUE'  AND CONTACT_ID = @CONTACT_ID
		END
		IF (@COUNT_RECS >= @N_PEOPLE) -- I.E. RULE MET, SO WE DON'T UNCOMPLETE
		BEGIN
			SET @ITEM_SET_ID = 0
		END
	END
	
	 -- agreement at the include / exclude level
	IF (@RECONCILLIATION_TYPE = 'auto excl')
	BEGIN
		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 -- INCLUDED
		SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 AND IS_COMPLETED = 'TRUE'  AND CONTACT_ID = @CONTACT_ID

		IF (@COUNT_RECS < @N_PEOPLE)
		BEGIN
			SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 -- EXCLUDED
			SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 AND IS_COMPLETED = 'TRUE'  AND CONTACT_ID = @CONTACT_ID
		END
	END

	

	-- *************************** STAGE 2: AUTO-RECONCILE AND AUTO-COMPLETE ***************************

	IF (@COUNT_RECS < @N_PEOPLE) AND (@RECONCILLIATION_TYPE != 'Single') -- REMOVE AUTO-RECONCILLIATION WHERE RULES ARE NO LONGER MET
	BEGIN
		IF (@ITEM_SET_ID > 0)
		BEGIN
			UPDATE TB_ITEM_SET
				SET IS_COMPLETED = 'FALSE'
				WHERE ITEM_SET_ID = @ITEM_SET_ID
		END
	END
	IF (@AUTO_EXCLUDE = 'TRUE' AND ((@COUNT_RECS < @N_PEOPLE) OR @RECONCILLIATION_TYPE = 'Single')) -- WHERE RULES ARE NO LONGER MET - AUTO *INCLUDE*
	BEGIN
		-- SECOND, AUTO INCLUDE / EXCLUDE
		UPDATE TB_ITEM_REVIEW
			SET IS_INCLUDED = 'TRUE'
			WHERE ITEM_ID = @ITEM_ID
	END
		UPDATE TB_TRAINING_ITEM -- TRY TO RE-LOCK THE ITEM - IF SOMEONE ELSE HAS LOCKED IT, THERE'S NOT MUCH WE CAN DO ABOUT IT THOUGH!
			SET CONTACT_ID_CODING = @CONTACT_ID,
			WHEN_LOCKED = CURRENT_TIMESTAMP
			WHERE ITEM_ID = @ITEM_ID AND CONTACT_ID_CODING = 0
	
END


SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeAutoReconcileDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeBulkDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAttributeBulkDelete]
(
	@ATTRIBUTE_ID BIGINT,
	@ITEM_ID_LIST varchar(max),
	@SEARCH_ID_LIST varchar(max),
	@SET_ID INT,
	@CONTACT_ID INT,
	@REVIEW_ID INT
)
With Recompile
As
SET NOCOUNT ON

DECLARE @IS_COMPLETED BIT

SELECT @IS_COMPLETED = CODING_IS_FINAL FROM TB_REVIEW_SET WHERE SET_ID = @SET_ID

DECLARE @ITEM_IDS TABLE
	(
		value bigint primary key
	)
	
	SELECT @IS_COMPLETED = CODING_IS_FINAL FROM TB_REVIEW_SET
		WHERE SET_ID = @SET_ID AND REVIEW_ID = @REVIEW_ID

	IF (@SEARCH_ID_LIST = '')
	BEGIN
		INSERT INTO @ITEM_IDS (VALUE) SELECT DISTINCT value FROM DBO.fn_split_int(@ITEM_ID_LIST, ',')
	END
	ELSE
	BEGIN
		INSERT INTO @ITEM_IDS (VALUE)
			SELECT DISTINCT ITEM_ID FROM TB_SEARCH_ITEM INNER JOIN DBO.fn_split_int(@SEARCH_ID_LIST, ',') ON
				TB_SEARCH_ITEM.SEARCH_ID = value
	END

	DELETE FROM TB_ITEM_ATTRIBUTE
		from TB_ITEM_ATTRIBUTE
		inner join @ITEM_IDS as theList on theList.value = tb_item_attribute.item_id 
		inner join TB_ITEM_REVIEW on TB_ITEM_REVIEW.ITEM_ID = theList.value and TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
			and TB_ITEM_SET.IS_COMPLETED = 'FALSE'
		where TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID
		AND NOT TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID IN -- so we don't delete uncompleted versions as well as completed 
													   -- (if there's a completed version, that is)
		(
			SELECT ITEM_ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE IA2
			inner join @ITEM_IDS as theList on theList.value = tb_item_attribute.item_id 
			inner join TB_ITEM_REVIEW on TB_ITEM_REVIEW.ITEM_ID = theList.value and TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
				and TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			where TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID
		)
		
		DELETE FROM TB_ITEM_ATTRIBUTE
		from TB_ITEM_ATTRIBUTE
		inner join @ITEM_IDS as theList on theList.value = tb_item_attribute.item_id 
		inner join TB_ITEM_REVIEW on TB_ITEM_REVIEW.ITEM_ID = theList.value and TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			and TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		where TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID
		
/*
IF (@IS_COMPLETED = 'TRUE')
	BEGIN 
		DELETE FROM TB_ITEM_ATTRIBUTE
		from TB_ITEM_ATTRIBUTE
		inner join @ITEM_IDS as theList on theList.value = tb_item_attribute.item_id 
		inner join TB_ITEM_REVIEW on TB_ITEM_REVIEW.ITEM_ID = theList.value and TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		where TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID
	END
	ELSE
	BEGIN
		DELETE FROM TB_ITEM_ATTRIBUTE
		from TB_ITEM_ATTRIBUTE
		inner join @ITEM_IDS as theList on theList.value = tb_item_attribute.item_id 
		inner join TB_ITEM_REVIEW on TB_ITEM_REVIEW.ITEM_ID = theList.value and TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
			and TB_ITEM_SET.IS_COMPLETED = 'FALSE'
		where TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID
		
		DELETE FROM TB_ITEM_ATTRIBUTE
		from TB_ITEM_ATTRIBUTE
		inner join @ITEM_IDS as theList on theList.value = tb_item_attribute.item_id 
		inner join TB_ITEM_REVIEW on TB_ITEM_REVIEW.ITEM_ID = theList.value and TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			and TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		where TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID
	END
*/

DELETE FROM TB_ITEM_SET
WHERE NOT ITEM_SET_ID IN 
(
	SELECT DISTINCT ITEM_SET_ID  
	FROM TB_ITEM_ATTRIBUTE ia
	inner join tb_item_review ir on ia.ITEM_ID = ir.ITEM_ID 
	and ir.REVIEW_ID = @REVIEW_ID
	union
	select tio.item_set_id
	from TB_ITEM_OUTCOME tio
	inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID
	inner join TB_ITEM_REVIEW ir on tis.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
) and SET_ID = @SET_ID


SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeBulkDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeBulkInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAttributeBulkInsert]
(
	@SET_ID INT,
	@IS_COMPLETED BIT,
	@CONTACT_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@ITEM_ID_LIST varchar(max),
	@SEARCH_ID_LIST varchar(max) = '',
	@REVIEW_ID INT
)
With Recompile
As

SET NOCOUNT ON

-- NB THIS SP IS ALSO CALLED FROM st_TrainingItemAttributeBulkInsert 

	DECLARE @ITEM_IDS TABLE
	(
		--idx smallint Primary Key,
		value bigint primary key
	)
	
	SELECT @IS_COMPLETED = CODING_IS_FINAL FROM TB_REVIEW_SET
		WHERE SET_ID = @SET_ID AND REVIEW_ID = @REVIEW_ID

	IF (@SEARCH_ID_LIST = '')
	BEGIN
		INSERT INTO @ITEM_IDS (VALUE) SELECT DISTINCT value FROM DBO.fn_split_int(@ITEM_ID_LIST, ',')
	END
	ELSE
	BEGIN
		INSERT INTO @ITEM_IDS (VALUE)
			SELECT DISTINCT ITEM_ID FROM TB_SEARCH_ITEM INNER JOIN DBO.fn_split_int(@SEARCH_ID_LIST, ',') ON
				TB_SEARCH_ITEM.SEARCH_ID = value
	END
	
	-- FIRST, INSERT NECESSARY ITEM_SET RECORDS
	
	INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID)
		SELECT DISTINCT [VALUE], @SET_ID, @IS_COMPLETED, @CONTACT_ID FROM @ITEM_IDS ids
			EXCEPT
		SELECT [VALUE], @SET_ID, 'FALSE', @CONTACT_ID FROM @ITEM_IDS ids
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.SET_ID = @SET_ID -- DON'T WANT TO CREATE DUPLICATE RECORDS
				AND TB_ITEM_SET.ITEM_ID = [VALUE]
				AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
			EXCEPT
		SELECT [VALUE], @SET_ID, 'TRUE', @CONTACT_ID FROM @ITEM_IDS ids
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.SET_ID = @SET_ID -- DON'T WANT TO CREATE DUPLICATE RECORDS
				AND TB_ITEM_SET.ITEM_ID = [VALUE]
				AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
			EXCEPT
		SELECT [VALUE], @SET_ID, 'TRUE', @CONTACT_ID FROM @ITEM_IDS ids
			INNER JOIN TB_ITEM_SET IS2 ON IS2.SET_ID = @SET_ID -- in case it's already complete under another login
				AND IS2.ITEM_ID = [VALUE]
				AND IS2.IS_COMPLETED = 'TRUE'
		
		-- INSERT ALL ITEM_ATTRIBUTE RECORDS WHERE COMPLETE (CONTACT_ID IRRELEVANT)
		INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
		SELECT DISTINCT [VALUE], TB_ITEM_SET.ITEM_SET_ID, @ATTRIBUTE_ID FROM @ITEM_IDS ids
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.VALUE
			WHERE TB_ITEM_SET.SET_ID = @SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		EXCEPT
		SELECT TB_ITEM_ATTRIBUTE.ITEM_ID, TB_ITEM_ATTRIBUTE.ITEM_SET_ID, ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
			AND SET_ID = @SET_ID
			AND IS_COMPLETED = 'TRUE'
			
		-- INSERT INCOMPLETED ONES WHERE NECESSARY (I.E. WHERE NO COMPLETED EXIST AND ONLY FOR THIS CONTACT_ID)
		INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
		SELECT DISTINCT [VALUE], TB_ITEM_SET.ITEM_SET_ID, @ATTRIBUTE_ID FROM @ITEM_IDS ids
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.VALUE
			WHERE TB_ITEM_SET.SET_ID = @SET_ID
			AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'FALSE'
		EXCEPT
		SELECT TB_ITEM_ATTRIBUTE.ITEM_ID, TB_ITEM_ATTRIBUTE.ITEM_SET_ID, ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
			AND (TB_ITEM_SET.IS_COMPLETED = 'TRUE' or (TB_ITEM_SET.CONTACT_ID = @CONTACT_ID and TB_ITEM_SET.IS_COMPLETED = 'FALSE'))
			AND SET_ID = @SET_ID
	
	
/*	
	IF (@IS_COMPLETED = 'TRUE')
	BEGIN
		INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID)
		SELECT DISTINCT [VALUE], @SET_ID, 'TRUE', @CONTACT_ID FROM @ITEM_IDS ids
			EXCEPT
		SELECT [VALUE], @SET_ID, 'TRUE', @CONTACT_ID FROM @ITEM_IDS ids
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.SET_ID = @SET_ID
				AND TB_ITEM_SET.ITEM_ID = [VALUE]
				
		INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
		SELECT DISTINCT [VALUE], TB_ITEM_SET.ITEM_SET_ID, @ATTRIBUTE_ID FROM @ITEM_IDS ids
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.VALUE
			WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		EXCEPT
		SELECT TB_ITEM_ATTRIBUTE.ITEM_ID, TB_ITEM_ATTRIBUTE.ITEM_SET_ID, ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND SET_ID = @SET_ID
	END
	ELSE
	BEGIN
		INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID)
		SELECT DISTINCT [VALUE], @SET_ID, 'FALSE', @CONTACT_ID FROM @ITEM_IDS ids
			EXCEPT
		SELECT [VALUE], @SET_ID, 'FALSE', @CONTACT_ID FROM @ITEM_IDS ids
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.SET_ID = @SET_ID
				AND TB_ITEM_SET.ITEM_ID = [VALUE]
				AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
			EXCEPT
		SELECT [VALUE], @SET_ID, 'FALSE', @CONTACT_ID FROM @ITEM_IDS ids
			INNER JOIN TB_ITEM_SET IS2 ON IS2.SET_ID = @SET_ID -- in case it's already complete under another login
				AND IS2.ITEM_ID = [VALUE]
				AND IS2.IS_COMPLETED = 'TRUE'
		
		INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
		SELECT DISTINCT [VALUE], TB_ITEM_SET.ITEM_SET_ID, @ATTRIBUTE_ID FROM @ITEM_IDS ids
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.VALUE
			WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'FALSE'
		EXCEPT
		SELECT TB_ITEM_ATTRIBUTE.ITEM_ID, TB_ITEM_ATTRIBUTE.ITEM_SET_ID, ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
			AND CONTACT_ID = @CONTACT_ID
			AND SET_ID = @SET_ID
			--AND IS_COMPLETED = 'FALSE'
			
		-- deal with completed ones which are not necessarily under this login (THOUGH MAY BE)
		INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
		SELECT DISTINCT [VALUE], TB_ITEM_SET.ITEM_SET_ID, @ATTRIBUTE_ID FROM @ITEM_IDS ids
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.VALUE
			WHERE TB_ITEM_SET.SET_ID = @SET_ID --AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		EXCEPT
		SELECT TB_ITEM_ATTRIBUTE.ITEM_ID, TB_ITEM_ATTRIBUTE.ITEM_SET_ID, ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND SET_ID = @SET_ID

	END
*/
SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeBulkInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeChildFrequencies]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAttributeChildFrequencies]
(
      @ATTRIBUTE_ID BIGINT = null,
      @SET_ID BIGINT,
      @IS_INCLUDED BIT,
      @FILTER_ATTRIBUTE_ID BIGINT,
      @REVIEW_ID INT
)

As

SET NOCOUNT ON

IF (@FILTER_ATTRIBUTE_ID = -1)
BEGIN

	SELECT ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID,
		  COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) AS ITEM_COUNT, ATTRIBUTE_ORDER FROM TB_ITEM_ATTRIBUTE
	      
		  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
		  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
				AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
		  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
		  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		  GROUP BY TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
		  union
		  Select 'None of the codes above', -@ATTRIBUTE_ID, -@SET_ID,  COUNT(DISTINCT ITEM_ID) AS ITEM_COUNT, 10000
			from TB_ITEM_REVIEW 
			where ITEM_ID not in 
					(
						select TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
						  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
								AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
						  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
								AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
						  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
								AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
					)
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		  ORDER BY ATTRIBUTE_ORDER
end
ELSE
BEGIN
	SELECT ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID,
		  COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) AS ITEM_COUNT, ATTRIBUTE_ORDER FROM TB_ITEM_ATTRIBUTE
	      
		  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
		  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
				AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
		  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
		  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		  INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND IA2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
		  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
		  GROUP BY TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
		  UNION
		  Select 'None of the codes above', -@ATTRIBUTE_ID, -@SET_ID,  COUNT(DISTINCT TB_ITEM_REVIEW.ITEM_ID) AS ITEM_COUNT, 10000
			from TB_ITEM_REVIEW 
			INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND IA2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
			where TB_ITEM_REVIEW.ITEM_ID not in 
					(
						select TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
						  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
								AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
						  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
								AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
						  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
								AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
						  INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND IA2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
					)
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		  ORDER BY ATTRIBUTE_ORDER
END

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeChildFrequencies] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeCounts]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAttributeCounts]
(
	@ATTRIBUTE_LIST NVARCHAR(MAX),
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

SELECT DISTINCT ATTRIBUTE_ID, COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) AS NUM FROM TB_ITEM_ATTRIBUTE
	INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_LIST, ',') attribute_list
		ON attribute_list.value = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
	GROUP BY ATTRIBUTE_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeCounts] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeCrosstabs]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ItemAttributeCrosstabs]
(
	@PARENT_ATTRIBUTE_ID1 BIGINT,
	@PARENT_SET_ID1 INT,
	@PARENT_SET_ID2 INT,
	@PARENT_ATTRIBUTE_ID2 BIGINT,
	@FILTER_ATTRIBUTE_ID BIGINT,
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

DECLARE @TT TABLE
	(
	  ATTRIBUTE_ID BIGINT,
	  ATTRIBUTE_NAME NVARCHAR(255),
	  ATTRIBUTE_ORDER INT
	)
	
INSERT INTO @TT(ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER)
SELECT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER FROM TB_ATTRIBUTE
INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ATTRIBUTE.ATTRIBUTE_ID
	AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @PARENT_ATTRIBUTE_ID1
	AND TB_ATTRIBUTE_SET.SET_ID = @PARENT_SET_ID1
ORDER BY ATTRIBUTE_ORDER

DECLARE @cols NVARCHAR(2000)
SELECT  @cols = COALESCE(@cols + ',[' + CAST(ATTRIBUTE_ID AS NVARCHAR(10)) + ']',
                         '[' + CAST(ATTRIBUTE_ID AS NVARCHAR(10)) + ']')
FROM    @TT
ORDER BY ATTRIBUTE_ORDER

DECLARE @query NVARCHAR(4000)

IF (@FILTER_ATTRIBUTE_ID IS NULL OR @FILTER_ATTRIBUTE_ID = 0)
BEGIN
SET @query = N'SELECT ATTRIBUTE_NAME, ATTRIBUTE_ID2, ATTRIBUTE_ORDER2, '+
@cols +' 
FROM
(
select IA1.ATTRIBUTE_ID ATTRIBUTE_ID1, TBA2.ATTRIBUTE_NAME ATTRIBUTE_NAME, TBA2.ATTRIBUTE_ID ATTRIBUTE_ID2,
	IA1.ITEM_ID ITEM_ID1, AS2.ATTRIBUTE_ORDER ATTRIBUTE_ORDER2
from TB_ITEM_ATTRIBUTE IA1
INNER JOIN TB_ATTRIBUTE_SET AS1 ON AS1.ATTRIBUTE_ID = IA1.ATTRIBUTE_ID
	AND AS1.PARENT_ATTRIBUTE_ID = ' + CAST(@PARENT_ATTRIBUTE_ID1 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_SET IS1 ON IS1.ITEM_SET_ID = IA1.ITEM_SET_ID AND IS1.IS_COMPLETED = 1
	AND IS1.SET_ID = ' + CAST(@PARENT_SET_ID1 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = IA1.ITEM_ID
INNER JOIN TB_ATTRIBUTE_SET AS2 ON AS2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
	AND AS2.PARENT_ATTRIBUTE_ID = ' + CAST(@PARENT_ATTRIBUTE_ID2 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 1
	AND IS2.SET_ID = ' + CAST(@PARENT_SET_ID2 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ATTRIBUTE TBA2 ON TBA2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = IA1.ITEM_ID
	AND TB_ITEM_REVIEW.IS_DELETED = 0
	AND TB_ITEM_REVIEW.REVIEW_ID = ' + CAST(@REVIEW_ID AS NVARCHAR(10)) + ' 
) p
PIVOT
(
COUNT (P.ITEM_ID1)
FOR ATTRIBUTE_ID1 IN
( '+
@cols +' )
) AS pvt
ORDER BY ATTRIBUTE_ORDER2;'
END
ELSE
BEGIN
SET @query = N'SELECT ATTRIBUTE_NAME, ATTRIBUTE_ID2, ATTRIBUTE_ORDER2, '+
@cols +'
FROM
(
select IA1.ATTRIBUTE_ID ATTRIBUTE_ID1, TBA2.ATTRIBUTE_NAME ATTRIBUTE_NAME, TBA2.ATTRIBUTE_ID ATTRIBUTE_ID2,
	IA1.ITEM_ID ITEM_ID1, AS2.ATTRIBUTE_ORDER ATTRIBUTE_ORDER2
from TB_ITEM_ATTRIBUTE IA1
INNER JOIN TB_ATTRIBUTE_SET AS1 ON AS1.ATTRIBUTE_ID = IA1.ATTRIBUTE_ID
	AND AS1.PARENT_ATTRIBUTE_ID = ' + CAST(@PARENT_ATTRIBUTE_ID1 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_SET IS1 ON IS1.ITEM_SET_ID = IA1.ITEM_SET_ID AND IS1.IS_COMPLETED = 1
	AND IS1.SET_ID = ' + CAST(@PARENT_SET_ID1 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = IA1.ITEM_ID
INNER JOIN TB_ATTRIBUTE_SET AS2 ON AS2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
	AND AS2.PARENT_ATTRIBUTE_ID = ' + CAST(@PARENT_ATTRIBUTE_ID2 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 1
	AND IS2.SET_ID = ' + CAST(@PARENT_SET_ID2 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ATTRIBUTE TBA2 ON TBA2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = IA1.ITEM_ID
	AND TB_ITEM_REVIEW.IS_DELETED = 0
	AND TB_ITEM_REVIEW.REVIEW_ID = ' + CAST(@REVIEW_ID AS NVARCHAR(10)) + ' 
-- Code to filter
INNER JOIN TB_ITEM_ATTRIBUTE IA3 ON IA3.ITEM_ID = IA1.ITEM_ID
	AND IA3.ATTRIBUTE_ID = ' + CAST(@FILTER_ATTRIBUTE_ID AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_SET IS3 ON IS3.ITEM_SET_ID = IA3.ITEM_SET_ID
	AND IS3.IS_COMPLETED = 1
) p
PIVOT
(
COUNT (P.ITEM_ID1)
FOR ATTRIBUTE_ID1 IN
( '+
@cols +' )
) AS pvt
ORDER BY ATTRIBUTE_ORDER2;'
END

EXECUTE(@query)

SET NOCOUNT OFF



GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeCrosstabs] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAttributeDelete]
(
	@ITEM_ATTRIBUTE_ID BIGINT,
	@ITEM_SET_ID BIGINT
)

As
SET NOCOUNT ON
DELETE FROM TB_ITEM_ATTRIBUTE_PDF WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
DELETE FROM TB_ITEM_ATTRIBUTE WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID

DECLARE @CHECK BIGINT

set @CHECK = (SELECT COUNT(lines) from 
	(
		select ITEM_SET_ID as lines FROM TB_ITEM_ATTRIBUTE WHERE ITEM_SET_ID = @ITEM_SET_ID
		union 
		Select ITEM_SET_ID as lines from TB_ITEM_OUTCOME where ITEM_SET_ID = @ITEM_SET_ID
		) a
	)

IF (@CHECK = 0)
BEGIN
	DELETE FROM TB_ITEM_SET WHERE ITEM_SET_ID = @ITEM_SET_ID
END

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAttributeInsert]
(
	@ITEM_ID BIGINT,
	@SET_ID INT,
	@CONTACT_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@ADDITIONAL_TEXT nvarchar(max),
	@REVIEW_ID INT,

	@NEW_ITEM_ATTRIBUTE_ID BIGINT OUTPUT,
	@NEW_ITEM_SET_ID BIGINT OUTPUT
)

As
SET NOCOUNT ON

-- First get a valid item_set_id.
-- If is_coding_final for this review then contact_id is irrelevant.
-- If coding is complete the contact_id is irrelevant.
-- Otherwise, we need a item_set_id for this specific contact.

DECLARE @IS_CODING_FINAL BIT
DECLARE @ITEM_SET_ID BIGINT = NULL
DECLARE @CHECK BIGINT

SELECT @IS_CODING_FINAL = CODING_IS_FINAL FROM TB_REVIEW_SET WHERE SET_ID = @SET_ID AND REVIEW_ID = @REVIEW_ID

SELECT @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND IS_COMPLETED = 'True'
IF (@ITEM_SET_ID IS NULL)
BEGIN
	SELECT @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND CONTACT_ID = @CONTACT_ID
END
	
IF (@ITEM_SET_ID IS NULL) -- have to create one
BEGIN
	INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID)
	VALUES (@ITEM_ID, @SET_ID, @IS_CODING_FINAL, @CONTACT_ID)
	SET @ITEM_SET_ID = @@IDENTITY
END

-- We (finally) have an item_set_id we can use for our insert

SELECT TOP(1) @CHECK = ITEM_ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID AND ITEM_SET_ID = @ITEM_SET_ID

IF (@CHECK IS NULL) -- Not sure what to do if it's not null... - SHOULD REALLY THROW AN ERROR
BEGIN
	INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT)
	VALUES (@ITEM_ID, @ITEM_SET_ID, @ATTRIBUTE_ID, @ADDITIONAL_TEXT)
	SET @NEW_ITEM_ATTRIBUTE_ID = @@IDENTITY
END

SET @NEW_ITEM_SET_ID = @ITEM_SET_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeInsertSimple]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAttributeInsertSimple]
(
	@ITEM_ID BIGINT,
	@SET_ID INT,
	@CONTACT_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@ADDITIONAL_TEXT nvarchar(max),
	@REVIEW_ID INT
)

As
SET NOCOUNT ON

-- SIMPLE VERSION: NO RETURN VALUES

-- First get a valid item_set_id.
-- If is_coding_final for this review then contact_id is irrelevant.
-- If coding is complete the contact_id is irrelevant.
-- Otherwise, we need a item_set_id for this specific contact.

DECLARE @IS_CODING_FINAL BIT
DECLARE @ITEM_SET_ID BIGINT
DECLARE @CHECK BIGINT

SELECT @IS_CODING_FINAL = CODING_IS_FINAL FROM TB_REVIEW_SET WHERE SET_ID = @SET_ID AND REVIEW_ID = @REVIEW_ID

IF (@IS_CODING_FINAL = 'True')
BEGIN
	SELECT @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID
END
ELSE
BEGIN
	SELECT @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND IS_COMPLETED = 'True'
	IF (@ITEM_SET_ID IS NULL)
	BEGIN
		SELECT @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND CONTACT_ID = @CONTACT_ID
	END
END

if (@ITEM_SET_ID IS NULL) -- have to create one
BEGIN
	INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID)
	VALUES (@ITEM_ID, @SET_ID, @IS_CODING_FINAL, @CONTACT_ID)
	SET @ITEM_SET_ID = @@IDENTITY
END

-- We (finally) have an item_set_id we can use for our insert

SELECT TOP(1) @CHECK = ITEM_ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID AND ITEM_SET_ID = @ITEM_SET_ID

IF (@CHECK IS NULL) -- Not sure what to do if it's not null... - SHOULD REALLY THROW AN ERROR
BEGIN
	INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT)
	VALUES (@ITEM_ID, @ITEM_SET_ID, @ATTRIBUTE_ID, @ADDITIONAL_TEXT)
END

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeInsertSimple] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributePDF]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemAttributePDF]
	-- Add the parameters for the stored procedure here
	@ITEM_ATTRIBUTE_ID bigint
	,@ITEM_DOCUMENT_ID bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT * from TB_ITEM_ATTRIBUTE_PDF where ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID and @ITEM_DOCUMENT_ID = ITEM_DOCUMENT_ID
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributePDF] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributePDFDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemAttributePDFDelete]
	-- Add the parameters for the stored procedure here
	@ITEM_ATTRIBUTE_PDF_ID bigint
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    DELETE from TB_ITEM_ATTRIBUTE_PDF
    WHERE ITEM_ATTRIBUTE_PDF_ID = @ITEM_ATTRIBUTE_PDF_ID

END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributePDFDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributePDFInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemAttributePDFInsert]
	-- Add the parameters for the stored procedure here
	@ITEM_ATTRIBUTE_ID bigint
	,@ITEM_DOCUMENT_ID bigint
	,@PAGE int
	,@SHAPE_TEXT varchar(max)
	,@INTERVALS varchar(max)
	,@TEXTS nvarchar(max)
	,@ITEM_ATTRIBUTE_PDF_ID bigint output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    INSERT INTO TB_ITEM_ATTRIBUTE_PDF
           ([ITEM_DOCUMENT_ID]
           ,[ITEM_ATTRIBUTE_ID]
           ,[PAGE]
           ,[SHAPE_TEXT]
           ,[SELECTION_INTERVALS]
           ,[SELECTION_TEXTS])
     VALUES
           (@ITEM_DOCUMENT_ID
           ,@ITEM_ATTRIBUTE_ID
           ,@PAGE
           ,@SHAPE_TEXT
           ,@INTERVALS
           ,@TEXTS)
	set @ITEM_ATTRIBUTE_PDF_ID = @@IDENTITY
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributePDFInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributePDFReset]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemAttributePDFReset]
	-- Add the parameters for the stored procedure here
	@ITEM_ATTRIBUTE_ID bigint
	,@ITEM_DOCUMENT_ID bigint
	,@PAGE int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DELETE from TB_ITEM_ATTRIBUTE_PDF 
		where ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID AND
		ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
		AND (
				@PAGE = 0 --all document
				OR @PAGE = PAGE --just this page
			)
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributePDFReset] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributePDFUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemAttributePDFUpdate]
	-- Add the parameters for the stored procedure here
	@ITEM_ATTRIBUTE_PDF_ID bigint
	,@SHAPE_TEXT varchar(max)
	,@INTERVALS varchar(max)
	,@TEXTS nvarchar(max)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    UPDATE TB_ITEM_ATTRIBUTE_PDF
    SET SHAPE_TEXT = @SHAPE_TEXT
      ,SELECTION_INTERVALS = @INTERVALS
      ,SELECTION_TEXTS = @TEXTS
    WHERE ITEM_ATTRIBUTE_PDF_ID = @ITEM_ATTRIBUTE_PDF_ID


END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributePDFUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributes]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAttributes]
(
	@ITEM_SET_ID BIGINT
)

As

SET NOCOUNT ON

SELECT DISTINCT ITEM_ATTRIBUTE_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, TB_ITEM_ATTRIBUTE.ITEM_SET_ID,
	TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, ADDITIONAL_TEXT, CONTACT_ID, ATTRIBUTE_SET_ID
FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.ITEM_SET_ID = @ITEM_SET_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
WHERE TB_ITEM_ATTRIBUTE.ITEM_SET_ID = @ITEM_SET_ID


/*
SELECT distinct(TB_REVIEW_SET.set_id), @ITEM_ID ITEM_ID, IA.ITEM_ATTRIBUTE_ID, IA.ITEM_SET_ID, IA.ATTRIBUTE_ID, 
	IA.ADDITIONAL_TEXT, IA.CONTACT_ID, IA.ATTRIBUTE_SET_ID, IA.IS_COMPLETED, IA.IS_LOCKED
FROM TB_REVIEW_SET
CROSS APPLY dbo.fn_ItemAttributes(CODING_IS_FINAL, TB_REVIEW_SET.SET_ID, @CONTACT_ID, @ITEM_ID) IA
WHERE TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
*/

/*

SELECT IA.ITEM_ATTRIBUTE_ID, IA.ITEM_ID, IA.ITEM_SET_ID, IA.ATTRIBUTE_ID, IA.ADDITIONAL_TEXT, TB_ITEM_SET.CONTACT_ID, ATTRIBUTE_SET_ID
FROM TB_ITEM_ATTRIBUTE IA
INNER JOIN TB

*/


/*
IF (@CONTACT_ID = 0)
BEGIN
	SELECT IA.ITEM_ATTRIBUTE_ID, IA.ITEM_ID, IA.ITEM_SET_ID, IA.ATTRIBUTE_ID, IA.ADDITIONAL_TEXT, TB_ITEM_SET.CONTACT_ID, ATTRIBUTE_SET_ID
	FROM TB_ITEM_ATTRIBUTE IA
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = IA.ITEM_SET_ID
	INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
	WHERE IA.ITEM_ID = @ITEM_ID
END
ELSE
BEGIN
	SELECT IA.ITEM_ATTRIBUTE_ID, IA.ITEM_ID, IA.ITEM_SET_ID, IA.ATTRIBUTE_ID, IA.ADDITIONAL_TEXT, TB_ITEM_SET.CONTACT_ID, ATTRIBUTE_SET_ID
	FROM TB_ITEM_ATTRIBUTE IA
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = IA.ITEM_SET_ID AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
	INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
	WHERE IA.ITEM_ID = @ITEM_ID
END
*/

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributes] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributesAllFullTextDetailsList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ItemAttributesAllFullTextDetailsList] 
(
	@REVIEW_ID INT,
	--@CONTACT_ID INT,
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON
	SELECT  tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, p.ITEM_ATTRIBUTE_PDF_ID as [ID]
			, 'Page ' + CONVERT
							(varchar(10),PAGE) 
							+ ':' + CHAR(10) + '[¬s]"' 
							+ replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' 
				as [TEXT] 
			, NULL as [TEXT_FROM], NULL as [TEXT_TO]
			, 1 as IS_FROM_PDF
		from TB_REVIEW_SET rs
		inner join TB_ITEM_SET tis on rs.REVIEW_ID = @REVIEW_ID and tis.SET_ID = rs.SET_ID and tis.ITEM_ID = @ITEM_ID
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID --and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE_PDF p on ia.ITEM_ATTRIBUTE_ID = p.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on p.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
	UNION
	SELECT tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, t.ITEM_ATTRIBUTE_TEXT_ID as [ID]
			, SUBSTRING(
					replace(id.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) 
				 as [TEXT]
			, TEXT_FROM, TEXT_TO 
			, 0 as IS_FROM_PDF
		from TB_REVIEW_SET rs
		inner join TB_ITEM_SET tis on rs.REVIEW_ID = @REVIEW_ID and tis.SET_ID = rs.SET_ID and tis.ITEM_ID = @ITEM_ID
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID --and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE_TEXT t on ia.ITEM_ATTRIBUTE_ID = t.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on t.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
	ORDER by IS_FROM_PDF, [TEXT]	
	
SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributesAllFullTextDetailsList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributesContactFullTextDetailsList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ItemAttributesContactFullTextDetailsList] 
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON
	Declare @ItemSetIDs Table(SET_ID int primary key,ITEM_SET_ID bigint)--pre build list of concerned IDs
	--insert all completed items
	insert into @ItemSetIDs select s.SET_ID, Item_set_id from TB_ITEM_SET	tis
		inner join TB_SET s on tis.SET_ID = s.SET_ID and tis.ITEM_ID = @ITEM_ID and tis.IS_COMPLETED = 1
		inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @REVIEW_ID  and s.SET_ID = rs.SET_ID
	--insert the uncompleded items that belong to the user and are not in the temp table already
	insert into @ItemSetIDs select s.SET_ID, tis.ITEM_SET_ID from TB_ITEM_SET tis
		inner join TB_SET s on tis.SET_ID = s.SET_ID and tis.ITEM_ID = @ITEM_ID and tis.CONTACT_ID = @CONTACT_ID and tis.IS_COMPLETED = 0
		inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @REVIEW_ID  and s.SET_ID = rs.SET_ID
		where tis.SET_ID not in (select SET_ID from @ItemSetIDs)
	SELECT  tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, p.ITEM_ATTRIBUTE_PDF_ID as [ID]
			, 'Page ' + CONVERT
							(varchar(10),PAGE) 
							+ ':' + CHAR(10) + '[¬s]"' 
							+ replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' 
				as [TEXT] 
			, NULL as [TEXT_FROM], NULL as [TEXT_TO]
			, 1 as IS_FROM_PDF
		from @ItemSetIDs tis
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID --and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE_PDF p on ia.ITEM_ATTRIBUTE_ID = p.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on p.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
	UNION
	SELECT tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, t.ITEM_ATTRIBUTE_TEXT_ID as [ID]
			, SUBSTRING(
					replace(id.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) 
				 as [TEXT]
			, TEXT_FROM, TEXT_TO 
			, 0 as IS_FROM_PDF
		from @ItemSetIDs tis
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID --and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE_TEXT t on ia.ITEM_ATTRIBUTE_ID = t.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on t.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
	ORDER by IS_FROM_PDF, [TEXT]	
	
SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributesContactFullTextDetailsList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeSimpleBulkInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAttributeSimpleBulkInsert]
(
	@SET_ID INT,
	@CONTACT_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@ITEM_ID_LIST varchar(max)
)

As

SET NOCOUNT ON
	
	INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
	SELECT [VALUE], TB_ITEM_SET.ITEM_SET_ID, @ATTRIBUTE_ID FROM DBO.fn_split_int(@ITEM_ID_LIST, ',')
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = [VALUE]
			AND TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
	
SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeSimpleBulkInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeText]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[st_ItemAttributeText]
(
	@ITEM_ATTRIBUTE_ID BIGINT
)
AS
SET NOCOUNT ON

	SELECT ITEM_ATTRIBUTE_TEXT_ID,
		ITEM_DOCUMENT_ID,
		ITEM_ATTRIBUTE_ID,
		TEXT_FROM,
		TEXT_TO
	FROM TB_ITEM_ATTRIBUTE_TEXT
	WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
	ORDER BY TEXT_FROM
		


SET NOCOUNT OFF
	RETURN

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeText] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeTextDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[st_ItemAttributeTextDelete]
(
	@ITEM_ATTRIBUTE_ID BIGINT,
	@ITEM_DOCUMENT_ID BIGINT,
	@START_AT INT,
	@END_AT INT
)
AS
--SET NOCOUNT ON

	DECLARE @STORED_START int
	DECLARE @STORED_END int

	-- FIRST DELETE ALL CODES THAT LIE ENTIRELY INSIDE THE SELECTED SECTION
	DELETE FROM TB_ITEM_ATTRIBUTE_TEXT
		WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
		AND ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
		AND (TEXT_FROM >=  @START_AT AND TEXT_TO <= @END_AT)
	
	SET @STORED_START = NULL

	SELECT  @STORED_START = TEXT_FROM, @STORED_END = TEXT_TO FROM TB_ITEM_ATTRIBUTE_TEXT
		WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
		AND ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
		AND (TEXT_FROM <  @START_AT AND TEXT_TO > @END_AT)

	IF (@STORED_START != NULL) -- WE'RE DELETING A SECTION WITHIN A CURRENT CODE
	BEGIN
		UPDATE TB_ITEM_ATTRIBUTE_TEXT
			SET TEXT_TO = @START_AT
			WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
			AND ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
			AND (TEXT_FROM = @STORED_START AND TEXT_TO = @STORED_END)

		INSERT INTO TB_ITEM_ATTRIBUTE_TEXT(ITEM_DOCUMENT_ID, ITEM_ATTRIBUTE_ID, TEXT_FROM, TEXT_TO)
		VALUES (@ITEM_DOCUMENT_ID, @ITEM_ATTRIBUTE_ID, @END_AT, @STORED_END)

	END
	ELSE
	BEGIN

		-- OVERLAPPING AT START
		UPDATE TB_ITEM_ATTRIBUTE_TEXT
			SET TEXT_TO = @START_AT
			WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
			AND ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
			AND (TEXT_FROM  <=  @START_AT AND TEXT_TO <= @END_AT AND TEXT_TO > @START_AT)

		-- OVERLAPPING AT END
		UPDATE TB_ITEM_ATTRIBUTE_TEXT
			SET TEXT_FROM = @END_AT
			WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
			AND ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
			AND (TEXT_FROM  >=  @START_AT AND TEXT_FROM <= @END_AT AND TEXT_TO > @END_AT)

	END

--SET NOCOUNT OFF
	RETURN

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeTextDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeTextInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[st_ItemAttributeTextInsert]
(
	@ITEM_ATTRIBUTE_ID BIGINT,
	@ITEM_DOCUMENT_ID BIGINT,
	@START_AT INT,
	@END_AT INT
)
AS
SET NOCOUNT ON

	DECLARE @MIN_START bigint
	DECLARE @MAX_END bigint
	DECLARE @ITEM_ATTRIBUTE_TEXT_ID BIGINT
	DECLARE @CHECK_COUNT INT

	-- FIRST CHECK THAT WE HAVEN'T ALREADY GOT A CODE COVERING THIS
	SELECT @CHECK_COUNT = COUNT(*) FROM TB_ITEM_ATTRIBUTE_TEXT
		WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
		AND ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
		AND (TEXT_FROM <= @START_AT AND TEXT_TO >= @END_AT)

	IF (@CHECK_COUNT > 0)
		RETURN -- NO NEED TO GO ANY FURTHER: THERE IS A CODE THAT COVERS THIS ALREADY

   -- OVERLAPPING AT THE BEGINNING
	SELECT @MIN_START = TEXT_FROM, @MAX_END = TEXT_TO, @ITEM_ATTRIBUTE_TEXT_ID = ITEM_ATTRIBUTE_TEXT_ID
		FROM TB_ITEM_ATTRIBUTE_TEXT
		WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
		AND ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
		AND (TEXT_FROM <= @START_AT AND TEXT_TO >= @START_AT)
	IF (@MIN_START != NULL AND @MAX_END != NULL)
	BEGIN
		UPDATE TB_ITEM_ATTRIBUTE_TEXT
			SET TEXT_FROM = CASE WHEN @MIN_START < @START_AT THEN @MIN_START ELSE @START_AT END,
				TEXT_TO = CASE WHEN @MAX_END > @END_AT THEN @MAX_END ELSE @END_AT END
			WHERE ITEM_ATTRIBUTE_TEXT_ID = @ITEM_ATTRIBUTE_TEXT_ID
	END
	ELSE
	BEGIN
		-- OVERLAPPING AT THE END
		SELECT @MIN_START = TEXT_FROM, @MAX_END = TEXT_TO, @ITEM_ATTRIBUTE_TEXT_ID = ITEM_ATTRIBUTE_TEXT_ID
			FROM TB_ITEM_ATTRIBUTE_TEXT
			WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
			AND ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
			AND (TEXT_FROM <= @END_AT AND TEXT_TO >= @END_AT)
		IF (@MIN_START != NULL AND @MAX_END != NULL)
		BEGIN
			UPDATE TB_ITEM_ATTRIBUTE_TEXT
				SET TEXT_FROM = CASE WHEN @MIN_START < @START_AT THEN @MIN_START ELSE @START_AT END,
					TEXT_TO = CASE WHEN @MAX_END > @END_AT THEN @MAX_END ELSE @END_AT END
				WHERE ITEM_ATTRIBUTE_TEXT_ID = @ITEM_ATTRIBUTE_TEXT_ID
		END
		ELSE
		BEGIN
			-- OVERLAPPING AT BOTH ENDS
			SELECT @MIN_START = TEXT_FROM, @MAX_END = TEXT_TO, @ITEM_ATTRIBUTE_TEXT_ID = ITEM_ATTRIBUTE_TEXT_ID
				FROM TB_ITEM_ATTRIBUTE_TEXT
				WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
				AND ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
				AND (TEXT_FROM <= @START_AT AND TEXT_TO >= @END_AT)
			IF (@MIN_START != NULL AND @MAX_END != NULL)
			BEGIN
				UPDATE TB_ITEM_ATTRIBUTE_TEXT
					SET TEXT_FROM = CASE WHEN @MIN_START < @START_AT THEN @MIN_START ELSE @START_AT END,
						TEXT_TO = CASE WHEN @MAX_END > @END_AT THEN @MAX_END ELSE @END_AT END
					WHERE ITEM_ATTRIBUTE_TEXT_ID = @ITEM_ATTRIBUTE_TEXT_ID
			END
			ELSE
			BEGIN
				-- NOT OVERLAPPING ANYWHERE = NEW INSERT
				INSERT INTO TB_ITEM_ATTRIBUTE_TEXT (ITEM_DOCUMENT_ID, ITEM_ATTRIBUTE_ID, TEXT_FROM, TEXT_TO)
				VALUES (@ITEM_DOCUMENT_ID, @ITEM_ATTRIBUTE_ID, @START_AT, @END_AT)
			END
		END
	END
	


/*
	SELECT @MIN_START = MIN(TEXT_FROM), @MAX_END = MAX(TEXT_TO) FROM TB_ITEM_ATTRIBUTE_TEXT
		WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
		AND ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
		AND
		(
			(TEXT_FROM <= @START_AT AND TEXT_TO >= @START_AT) OR
			(TEXT_FROM <= @END_AT AND TEXT_TO > @END_AT) OR
			(TEXT_TO > @START_AT AND TEXT_TO < @END_AT)
		)

	IF ((@MIN_START != NULL) AND (@MIN_START < @START_AT))
	BEGIN
		SET @START_AT = @MIN_START
	END
	IF ((@MAX_END != NULL) AND (@MAX_END > @END_AT))
	BEGIN
		SET @END_AT = @MAX_END
	END

	DELETE FROM TB_ITEM_ATTRIBUTE_TEXT
	WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
		AND ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID
		AND (TEXT_FROM >= @START_AT AND TEXT_TO <= @END_AT)

	INSERT INTO TB_ITEM_ATTRIBUTE_TEXT (ITEM_DOCUMENT_ID, ITEM_ATTRIBUTE_ID, TEXT_FROM, TEXT_TO)
	VALUES (@ITEM_DOCUMENT_ID, @ITEM_ATTRIBUTE_ID, @START_AT, @END_AT)

*/


SET NOCOUNT OFF
	RETURN

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeTextInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAttributeUpdate]
(
	@ADDITIONAL_TEXT nvarchar(max),
	@ITEM_ATTRIBUTE_ID BIGINT
)

As
SET NOCOUNT ON

UPDATE TB_ITEM_ATTRIBUTE
	SET ADDITIONAL_TEXT = @ADDITIONAL_TEXT
	WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAttributeUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAuthorDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAuthorDelete]
(
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON
DELETE from TB_ITEM_AUTHOR where ITEM_ID = @ITEM_ID 


SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAuthorDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAuthorUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemAuthorUpdate]
(
	@ITEM_ID BIGINT
,	@RANK smallint
,	@ROLE TINYINT = 0
,	@LAST NVARCHAR(50)
,	@FIRST NVARCHAR(50) = NULL
,	@SECOND NVARCHAR(50) = NULL

)

As

SET NOCOUNT ON

insert into TB_ITEM_AUTHOR 
(	
	ITEM_ID
,	LAST 
,	FIRST 
,	SECOND 
,	ROLE
,	RANK 
) VALUES (
	@ITEM_ID
,	@LAST
,	@FIRST
,	@SECOND
,	@ROLE
,	@RANK
)

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemAuthorUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemComparisonList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemComparisonList] (
	@REVIEW_ID INT,
	@COMPARISON_ID INT,
	@LIST_WHAT NVARCHAR(25),
	
	@PageNum INT = 1,
	@PerPage INT = 3,
	@CurrentPage INT OUTPUT,
	@TotalPages INT OUTPUT,
	@TotalRows INT OUTPUT 
)

As
SET NOCOUNT ON
DECLARE @T1 TABLE --item_attribute for R1, R1 and R2 are relative to the sproc, could be any couple from 
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)
DECLARE @T2 TABLE --item_attribute for R2
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)
DECLARE @TT table (ITEM_ID bigint primary key)

insert into @T1 --item attributes from R1
select ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
	inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID 
												and  cia.CONTACT_ID = 
													CASE  @LIST_WHAT
														WHEN 'ComparisonAgree2vs3' THEN c.CONTACT_ID2
														WHEN 'ComparisonDisagree2vs3' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID1
													END 
												and c.COMPARISON_ID = @COMPARISON_ID

insert into @T2 --item attributes from R2
select ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID 
												and cia.CONTACT_ID = 
													CASE  @LIST_WHAT
														WHEN 'ComparisonAgree1vs2' THEN c.CONTACT_ID2
														WHEN 'ComparisonDisagree1vs2' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID3
													END 
												and c.COMPARISON_ID = @COMPARISON_ID


IF (@LIST_WHAT LIKE 'ComparisonAgree%')
BEGIN
	insert into @TT --add all agreements; see st_ComparisonStats to understand how this works
	Select distinct t1.ITEM_ID from @T1 t1 
		inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID
		except
	select distinct(t1.item_id) from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
		left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
		left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
		where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null
END
ELSE
BEGIN
	insert into @TT --add all disagreements; see st_ComparisonStats to understand how this works
	select distinct(t1.item_id)
	from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
	left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null
	
END

declare @RowsToRetrieve int

	SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
	FROM TB_ITEM I
	INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
		TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
	WHERE I.ITEM_ID IN (SELECT ITEM_ID FROM @TT)

	set @TotalPages = @TotalRows/@PerPage

	if @PageNum < 1
	set @PageNum = 1

	if @TotalRows % @PerPage != 0
	set @TotalPages = @TotalPages + 1

	set @RowsToRetrieve = @PerPage * @PageNum
	set @CurrentPage = @PageNum;

	WITH SearchResults AS
	(

SELECT DISTINCT(I.ITEM_ID), I.[TYPE_ID], I.OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
	TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
	[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
	AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENTAUTHORS
	,TB_ITEM_REVIEW.MASTER_ITEM_ID, DOI, KEYWORDS 
	, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
FROM TB_ITEM I
INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
	TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
WHERE I.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	
)
	Select ITEM_ID, [TYPE_ID], OLD_ITEM_ID, AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, PARENTAUTHORS
		,SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
		, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
	FROM SearchResults 
	WHERE RowNum > @RowsToRetrieve - @PerPage
	AND RowNum <= @RowsToRetrieve 

SELECT	@CurrentPage as N'@CurrentPage',
		@TotalPages as N'@TotalPages',
		@TotalRows as N'@TotalRows'

SET NOCOUNT OFF

--DECLARE @TT TABLE
--	(
--	  ITEM_ID BIGINT,
--	  ATTRIBUTE_ID BIGINT
--	)

--IF (@LIST_WHAT LIKE 'ComparisonAgree%')
--BEGIN
--	INSERT INTO @TT(ITEM_ID, ATTRIBUTE_ID)
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--			CASE  @LIST_WHAT
--				WHEN 'ComparisonAgree2vs3' THEN tb_COMPARISON.CONTACT_ID2
--				ELSE tb_COMPARISON.CONTACT_ID1
--			END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	INTERSECT --  ********** AGREEMENT - THEREFORE INTERSECT
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--		CASE @LIST_WHAT
--			WHEN 'ComparisonAgree1vs2' THEN TB_COMPARISON.CONTACT_ID2
--			ELSE tb_COMPARISON.CONTACT_ID3
--		END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	ORDER BY ITEM_ID, ATTRIBUTE_ID
--END
--ELSE
--BEGIN
--INSERT INTO @TT(ITEM_ID, ATTRIBUTE_ID)
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--			CASE  @LIST_WHAT
--				WHEN 'ComparisonDisagree2vs3' THEN tb_COMPARISON.CONTACT_ID2
--				ELSE tb_COMPARISON.CONTACT_ID1
--			END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	EXCEPT -- ******************* DISAGREEMENT THEREFORE EXCEPT
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--		CASE @LIST_WHAT
--			WHEN 'ComparisonDisagree1vs2' THEN TB_COMPARISON.CONTACT_ID2
--			ELSE tb_COMPARISON.CONTACT_ID3
--		END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	ORDER BY ITEM_ID, ATTRIBUTE_ID
	
--	-- Make sure that both have coded each item (only needed for disagreements - as agreements work by definition)
--	DELETE FROM @TT WHERE NOT ITEM_ID IN
--	(
--	SELECT DISTINCT CIA1.ITEM_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE CIA1
--		INNER JOIN tb_COMPARISON_ITEM_ATTRIBUTE CIA2 ON CIA1.ITEM_ID = CIA2.ITEM_ID
--		INNER JOIN tb_COMPARISON COMP1 ON COMP1.COMPARISON_ID = @COMPARISON_ID AND CIA1.CONTACT_ID =
--			CASE @LIST_WHAT
--				WHEN 'ComparisonDisagree2vs3' THEN COMP1.CONTACT_ID2
--				ELSE COMP1.CONTACT_ID1
--			END
--		INNER JOIN tb_COMPARISON COMP2 ON COMP2.COMPARISON_ID = @COMPARISON_ID AND CIA2.CONTACT_ID =
--			CASE @LIST_WHAT
--				WHEN 'ComparisonDisagree1vs2' THEN COMP2.CONTACT_ID2
--				ELSE COMP2.CONTACT_ID3
--			END
--	WHERE CIA1.COMPARISON_ID = @COMPARISON_ID AND CIA2.COMPARISON_ID = @COMPARISON_ID
--	)
	
--END

--declare @RowsToRetrieve int

--	SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
--	FROM TB_ITEM I
--	INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
--	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
--		TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
--	WHERE I.ITEM_ID IN (SELECT ITEM_ID FROM @TT)

--	set @TotalPages = @TotalRows/@PerPage

--	if @PageNum < 1
--	set @PageNum = 1

--	if @TotalRows % @PerPage != 0
--	set @TotalPages = @TotalPages + 1

--	set @RowsToRetrieve = @PerPage * @PageNum
--	set @CurrentPage = @PageNum;

--	WITH SearchResults AS
--	(

--SELECT DISTINCT(I.ITEM_ID), I.[TYPE_ID], I.OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
--	TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
--	[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
--	AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENTAUTHORS
--	,TB_ITEM_REVIEW.MASTER_ITEM_ID 
--	, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
--FROM TB_ITEM I
--INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
--INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
--	TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
--WHERE I.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	
--)
--	Select ITEM_ID, [TYPE_ID], OLD_ITEM_ID, AUTHORS,
--		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
--		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
--		AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, PARENTAUTHORS
--		,SearchResults.MASTER_ITEM_ID
--		, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
--	FROM SearchResults 
--	WHERE RowNum > @RowsToRetrieve - @PerPage
--	AND RowNum <= @RowsToRetrieve 

--SELECT	@CurrentPage as N'@CurrentPage',
--		@TotalPages as N'@TotalPages',
--		@TotalRows as N'@TotalRows'

--SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemComparisonList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemComparisonScreeningList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemComparisonScreeningList] (
	@REVIEW_ID INT,
	@COMPARISON_ID INT,
	@LIST_WHAT NVARCHAR(25),
	
	@PageNum INT = 1,
	@PerPage INT = 3,
	@CurrentPage INT OUTPUT,
	@TotalPages INT OUTPUT,
	@TotalRows INT OUTPUT 
)

As
SET NOCOUNT ON
DECLARE @T1 TABLE --item_attribute for R1, R1 and R2 are relative to the sproc, could be any couple from 
	(
	  ITEM_ID BIGINT,
	  [STATE] char(1),
		  PRIMARY KEY (ITEM_ID, [STATE]) 
	)
DECLARE @T2 TABLE --item_attribute for R2
	(
	  ITEM_ID BIGINT,
	  [STATE] char(1),
		  PRIMARY KEY (ITEM_ID, [STATE]) 
	)
DECLARE @TT table (ITEM_ID bigint primary key)

insert into @T1 --item attributes from R1
SELECT sub.ITEM_ID 
		 ,CASE 
			when [is incl] > 0 and [is ex] > 0 then 'B'
			when [is incl] > 0 then 'I'
			WHEN [is ex] > 0 then 'E'
			ELSE Null
		END
		from
		(select inc.ITEM_ID,  Sum(CASE inc.IS_INCLUDED when 1 then 1 else 0 end) [is incl]
			, Sum(CASE inc.IS_INCLUDED when 0 then 1 else 0 end) [is ex]
			from 
			tb_COMPARISON c
			left join tb_COMPARISON_ITEM_ATTRIBUTE inc on c.COMPARISON_ID = inc.COMPARISON_ID 
												and  inc.CONTACT_ID = 
													CASE  @LIST_WHAT
														WHEN 'ComparisonAgree2vs3Sc' THEN c.CONTACT_ID2
														WHEN 'ComparisonDisagree2vs3Sc' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID1
													END 
												and c.COMPARISON_ID = @COMPARISON_ID
			group by inc.ITEM_ID
		) sub
		where ITEM_ID is not null

insert into @T2 --item attributes from R2
SELECT sub.ITEM_ID 
		 ,CASE 
			when [is incl] > 0 and [is ex] > 0 then 'B'
			when [is incl] > 0 then 'I'
			WHEN [is ex] > 0 then 'E'
			ELSE Null
		END
		from
		(select inc.ITEM_ID,  Sum(CASE inc.IS_INCLUDED when 1 then 1 else 0 end) [is incl]
			, Sum(CASE inc.IS_INCLUDED when 0 then 1 else 0 end) [is ex]
			from 
			tb_COMPARISON c
			left join tb_COMPARISON_ITEM_ATTRIBUTE inc on c.COMPARISON_ID = inc.COMPARISON_ID 
												and  inc.CONTACT_ID = 
													CASE  @LIST_WHAT
														WHEN 'ComparisonAgree1vs2Sc' THEN c.CONTACT_ID2
														WHEN 'ComparisonDisagree1vs2Sc' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID3
													END 
												and c.COMPARISON_ID = @COMPARISON_ID
			group by inc.ITEM_ID
		) sub
		where ITEM_ID is not null



IF (@LIST_WHAT LIKE 'ComparisonAgree%')
BEGIN
	insert into @TT --add all agreements; see st_ComparisonStats to understand how this works
	select distinct(t1.item_id) from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.STATE = t2.STATE
		
END
ELSE
BEGIN
	insert into @TT --add all disagreements; see st_ComparisonStats to understand how this works
	select distinct(t1.item_id) from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.STATE != t2.STATE
	
END

declare @RowsToRetrieve int

	SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
	FROM TB_ITEM I
	INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
		TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
	WHERE I.ITEM_ID IN (SELECT ITEM_ID FROM @TT)

	set @TotalPages = @TotalRows/@PerPage

	if @PageNum < 1
	set @PageNum = 1

	if @TotalRows % @PerPage != 0
	set @TotalPages = @TotalPages + 1

	set @RowsToRetrieve = @PerPage * @PageNum
	set @CurrentPage = @PageNum;

	WITH SearchResults AS
	(

SELECT DISTINCT(I.ITEM_ID), I.[TYPE_ID], I.OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
	TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
	[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
	AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENTAUTHORS
	,TB_ITEM_REVIEW.MASTER_ITEM_ID, DOI, KEYWORDS 
	, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
FROM TB_ITEM I
INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
	TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
WHERE I.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	
)
	Select ITEM_ID, [TYPE_ID], OLD_ITEM_ID, AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, PARENTAUTHORS
		,SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
		, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
	FROM SearchResults 
	WHERE RowNum > @RowsToRetrieve - @PerPage
	AND RowNum <= @RowsToRetrieve 

SELECT	@CurrentPage as N'@CurrentPage',
		@TotalPages as N'@TotalPages',
		@TotalRows as N'@TotalRows'

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemComparisonScreeningList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemCreate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemCreate]
(
	@ITEM_ID BIGINT OUTPUT
,	@TITLE NVARCHAR(4000) = NULL
,	@TYPE_ID TINYINT
,	@PARENT_TITLE NVARCHAR(4000)
,	@SHORT_TITLE NVARCHAR(70)
,	@DATE_CREATED DATETIME = NULL
,	@CREATED_BY NVARCHAR(50) = NULL
,	@DATE_EDITED DATETIME = NULL
,	@EDITED_BY NVARCHAR(50) = NULL
,	@YEAR NCHAR(4) = NULL
,	@MONTH NVARCHAR(10) = NULL
,	@STANDARD_NUMBER NVARCHAR(255) = NULL
,	@CITY NVARCHAR(100) = NULL
,	@COUNTRY NVARCHAR(100) = NULL
,	@PUBLISHER NVARCHAR(1000) = NULL
,	@INSTITUTION NVARCHAR(1000) = NULL
,	@VOLUME NVARCHAR(56) = NULL
,	@PAGES NVARCHAR(50) = NULL
,	@EDITION NVARCHAR(200) = NULL
,	@ISSUE NVARCHAR(100) = NULL
,	@IS_LOCAL BIT = NULL
,	@AVAILABILITY NVARCHAR(255) = NULL
,	@URL NVARCHAR(500) = NULL
,	@COMMENTS NVARCHAR(MAX) = NULL
,	@ABSTRACT NVARCHAR(MAX) = NULL
,	@REVIEW_ID INT
,	@IS_INCLUDED BIT
,	@DOI NVARCHAR(500) = NULL
,	@KEYWORDS NVARCHAR(MAX) = NULL
)

As

SET NOCOUNT ON

INSERT INTO TB_ITEM (TITLE, [TYPE_ID], PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
	[YEAR],[MONTH],STANDARD_NUMBER,CITY,COUNTRY,PUBLISHER,INSTITUTION,VOLUME,PAGES,EDITION,ISSUE,IS_LOCAL,AVAILABILITY,URL,
	COMMENTS,ABSTRACT,DOI,KEYWORDS)
VALUES (@TITLE,@TYPE_ID,@PARENT_TITLE,@SHORT_TITLE,@DATE_CREATED,@CREATED_BY,@DATE_EDITED,@EDITED_BY,@YEAR,
	@MONTH,@STANDARD_NUMBER,@CITY,@COUNTRY,@PUBLISHER,@INSTITUTION,@VOLUME,@PAGES,@EDITION,@ISSUE,@IS_LOCAL,@AVAILABILITY,@URL,
	@COMMENTS,@ABSTRACT,@DOI,@KEYWORDS)

SET  @ITEM_ID = @@IDENTITY

INSERT INTO TB_ITEM_REVIEW (IS_DELETED, IS_INCLUDED, ITEM_ID, MASTER_ITEM_ID, REVIEW_ID)
VALUES ('FALSE', @IS_INCLUDED, @ITEM_ID, NULL, @REVIEW_ID)

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemCreate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemCrosstabsList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemCrosstabsList] (
	@REVIEW_ID INT,
	@XSET_ID INT,
	@YSET_ID INT,
	@FILTER_SET_ID INT,
	@XATTRIBUTE_ID BIGINT,
	@YATTRIBUTE_ID BIGINT,
	@FILTER_ATTRIBUTE_ID BIGINT,
	
	@PageNum INT = 1,
	@PerPage INT = 3,
	@CurrentPage INT OUTPUT,
	@TotalPages INT OUTPUT,
	@TotalRows INT OUTPUT 
)

As

SET NOCOUNT ON

declare @RowsToRetrieve int

IF (@FILTER_ATTRIBUTE_ID = 0)
BEGIN

SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
FROM TB_ITEM I
INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
	TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'

INNER JOIN TB_ITEM_ATTRIBUTE IAX ON IAX.ITEM_ID = I.ITEM_ID
	AND IAX.ATTRIBUTE_ID = @XATTRIBUTE_ID
INNER JOIN TB_ITEM_SET ISX ON ISX.ITEM_SET_ID = IAX.ITEM_SET_ID
	AND ISX.IS_COMPLETED = 'TRUE'
	AND ISX.SET_ID = @XSET_ID
	
INNER JOIN TB_ITEM_ATTRIBUTE IAY ON IAY.ITEM_ID = I.ITEM_ID
	AND IAY.ATTRIBUTE_ID = @YATTRIBUTE_ID
INNER JOIN TB_ITEM_SET ISY ON ISY.ITEM_SET_ID = IAY.ITEM_SET_ID
	AND ISY.IS_COMPLETED = 'TRUE'
	AND ISY.SET_ID = @YSET_ID
	
set @TotalPages = @TotalRows/@PerPage

	if @PageNum < 1
	set @PageNum = 1

	if @TotalRows % @PerPage != 0
	set @TotalPages = @TotalPages + 1

	set @RowsToRetrieve = @PerPage * @PageNum
	set @CurrentPage = @PageNum;

	WITH SearchResults AS
	(
	SELECT DISTINCT(I.ITEM_ID), I.[TYPE_ID], I.OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENTAUTHORS
		, TB_ITEM_REVIEW.MASTER_ITEM_ID, DOI, KEYWORDS
		, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum

	FROM TB_ITEM I
	INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
		TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'

	INNER JOIN TB_ITEM_ATTRIBUTE IAX ON IAX.ITEM_ID = I.ITEM_ID
		AND IAX.ATTRIBUTE_ID = @XATTRIBUTE_ID
	INNER JOIN TB_ITEM_SET ISX ON ISX.ITEM_SET_ID = IAX.ITEM_SET_ID
		AND ISX.IS_COMPLETED = 'TRUE'
		AND ISX.SET_ID = @XSET_ID
		
	INNER JOIN TB_ITEM_ATTRIBUTE IAY ON IAY.ITEM_ID = I.ITEM_ID
		AND IAY.ATTRIBUTE_ID = @YATTRIBUTE_ID
	INNER JOIN TB_ITEM_SET ISY ON ISY.ITEM_SET_ID = IAY.ITEM_SET_ID
		AND ISY.IS_COMPLETED = 'TRUE'
		AND ISY.SET_ID = @YSET_ID
	)
	Select ITEM_ID, [TYPE_ID], OLD_ITEM_ID, AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, PARENTAUTHORS
		,SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
		, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
	FROM SearchResults 
	WHERE RowNum > @RowsToRetrieve - @PerPage
	AND RowNum <= @RowsToRetrieve 
END
ELSE
BEGIN
SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
FROM TB_ITEM I
INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
	TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'

INNER JOIN TB_ITEM_ATTRIBUTE IAX ON IAX.ITEM_ID = I.ITEM_ID
	AND IAX.ATTRIBUTE_ID = @XATTRIBUTE_ID
INNER JOIN TB_ITEM_SET ISX ON ISX.ITEM_SET_ID = IAX.ITEM_SET_ID
	AND ISX.IS_COMPLETED = 'TRUE'
	AND ISX.SET_ID = @XSET_ID
	
INNER JOIN TB_ITEM_ATTRIBUTE IAY ON IAY.ITEM_ID = I.ITEM_ID
	AND IAY.ATTRIBUTE_ID = @YATTRIBUTE_ID
INNER JOIN TB_ITEM_SET ISY ON ISY.ITEM_SET_ID = IAY.ITEM_SET_ID
	AND ISY.IS_COMPLETED = 'TRUE'
	AND ISY.SET_ID = @YSET_ID
	
INNER JOIN TB_ITEM_ATTRIBUTE IAfilter ON IAfilter.ITEM_ID = I.ITEM_ID
	AND IAfilter.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
INNER JOIN TB_ITEM_SET ISfilter ON ISfilter.ITEM_SET_ID = IAfilter.ITEM_SET_ID
	AND ISfilter.IS_COMPLETED = 'TRUE'
	AND ISfilter.SET_ID = @FILTER_SET_ID
	
set @TotalPages = @TotalRows/@PerPage

	if @PageNum < 1
	set @PageNum = 1

	if @TotalRows % @PerPage != 0
	set @TotalPages = @TotalPages + 1

	set @RowsToRetrieve = @PerPage * @PageNum
	set @CurrentPage = @PageNum;

	WITH SearchResults AS
	(


SELECT DISTINCT(I.ITEM_ID), I.[TYPE_ID], I.OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
	TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
	[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
	AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENTAUTHORS
	,TB_ITEM_REVIEW.MASTER_ITEM_ID, DOI, KEYWORDS
	, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum

FROM TB_ITEM I
INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
	TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'

INNER JOIN TB_ITEM_ATTRIBUTE IAX ON IAX.ITEM_ID = I.ITEM_ID
	AND IAX.ATTRIBUTE_ID = @XATTRIBUTE_ID
INNER JOIN TB_ITEM_SET ISX ON ISX.ITEM_SET_ID = IAX.ITEM_SET_ID
	AND ISX.IS_COMPLETED = 'TRUE'
	AND ISX.SET_ID = @XSET_ID
	
INNER JOIN TB_ITEM_ATTRIBUTE IAY ON IAY.ITEM_ID = I.ITEM_ID
	AND IAY.ATTRIBUTE_ID = @YATTRIBUTE_ID
INNER JOIN TB_ITEM_SET ISY ON ISY.ITEM_SET_ID = IAY.ITEM_SET_ID
	AND ISY.IS_COMPLETED = 'TRUE'
	AND ISY.SET_ID = @YSET_ID
	
INNER JOIN TB_ITEM_ATTRIBUTE IAfilter ON IAfilter.ITEM_ID = I.ITEM_ID
	AND IAfilter.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
INNER JOIN TB_ITEM_SET ISfilter ON ISfilter.ITEM_SET_ID = IAfilter.ITEM_SET_ID
	AND ISfilter.IS_COMPLETED = 'TRUE'
	AND ISfilter.SET_ID = @FILTER_SET_ID
)
	Select ITEM_ID, [TYPE_ID], OLD_ITEM_ID, AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, PARENTAUTHORS
		,SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
		, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
	FROM SearchResults 
	WHERE RowNum > @RowsToRetrieve - @PerPage
	AND RowNum <= @RowsToRetrieve 
END

SELECT	@CurrentPage as N'@CurrentPage',
		@TotalPages as N'@TotalPages',
		@TotalRows as N'@TotalRows'

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemCrosstabsList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentBin]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ItemDocumentBin]
(
	@DOC_ID int,
	@REV_ID int
)

As
SELECT DOCUMENT_BINARY, DOCUMENT_EXTENSION, DOCUMENT_TITLE from tb_ITEM_DOCUMENT as I
	INNER JOIN TB_ITEM_REVIEW as R on I.ITEM_ID = R.ITEM_ID
WHERE ITEM_DOCUMENT_ID = @DOC_ID AND REVIEW_ID = @REV_ID


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDocumentBin] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentBinInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemDocumentBinInsert]
(
	@ITEM_ID BIGINT,
	@DOCUMENT_TITLE NVARCHAR(255),
	@DOCUMENT_EXTENSION NVARCHAR(5),
	@BIN IMAGE,
	@DOCUMENT_TEXT NVARCHAR(MAX)
)

As

SET NOCOUNT ON
SET @DOCUMENT_TEXT = replace(@DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10))
	INSERT INTO TB_ITEM_DOCUMENT(ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_BINARY, DOCUMENT_TEXT)
	VALUES(@ITEM_ID, @DOCUMENT_TITLE, @DOCUMENT_EXTENSION, @BIN, [dbo].fn_CLEAN_SIMPLE_TEXT(@DOCUMENT_TEXT))

SET NOCOUNT OFF


/****** Object:  StoredProcedure [dbo].[st_SearchDelete]    Script Date: 07/20/2009 21:32:31 ******/
SET ANSI_NULLS ON

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDocumentBinInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sergio
-- Create date: 
-- Description:	Delete An ItemDocument and associated Item_Attribute_Text
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDocumentDelete] 
	-- Add the parameters for the stored procedure here
	@DocID bigint = 0
AS
BEGIN
BEGIN TRY
	BEGIN TRANSACTION
	delete from TB_ITEM_ATTRIBUTE_TEXT where ITEM_DOCUMENT_ID = @DocID
	delete from tb_ITEM_DOCUMENT where ITEM_DOCUMENT_ID = @DocID
	COMMIT TRANSACTION
END TRY
BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
END CATCH
END

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDocumentDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemDocumentInsert]
(
	@ITEM_ID BIGINT,
	@DOCUMENT_TITLE NVARCHAR(255),
	@DOCUMENT_EXTENSION NVARCHAR(5),
	@DOCUMENT_TEXT NVARCHAR(MAX)
)

As

SET NOCOUNT ON
SET @DOCUMENT_TEXT = replace(@DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10))
	INSERT INTO TB_ITEM_DOCUMENT(ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_TEXT)
	VALUES(@ITEM_ID, @DOCUMENT_TITLE, @DOCUMENT_EXTENSION, @DOCUMENT_TEXT)

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDocumentInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemDocumentList]
(
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON

SELECT ITEM_DOCUMENT_ID, SHORT_TITLE, ID0.ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_TEXT, 1 IDX,
'DOC_BINARY' = CASE WHEN DOCUMENT_BINARY IS NULL THEN 'False' ELSE 'True' END, DOCUMENT_FREE_NOTES
FROM TB_ITEM_DOCUMENT ID0
INNER JOIN TB_ITEM I1 ON I1.ITEM_ID = ID0.ITEM_ID
WHERE ID0.ITEM_ID = @ITEM_ID

UNION

SELECT ITEM_DOCUMENT_ID, SHORT_TITLE, ID1.ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_TEXT, 2 IDX,
'DOC_BINARY' = CASE WHEN DOCUMENT_BINARY IS NULL THEN 'False' ELSE 'True' END, DOCUMENT_FREE_NOTES
FROM TB_ITEM_LINK IL1
INNER JOIN TB_ITEM_DOCUMENT ID1 ON ID1.ITEM_ID = IL1.ITEM_ID_SECONDARY and IL1.ITEM_ID_PRIMARY != IL1.ITEM_ID_SECONDARY
INNER JOIN TB_ITEM I2 ON I2.ITEM_ID = ID1.ITEM_ID
WHERE ITEM_ID_PRIMARY = @ITEM_ID

UNION

SELECT ITEM_DOCUMENT_ID, SHORT_TITLE, ID2.ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_TEXT, 2 IDX,
'DOC_BINARY' = CASE WHEN DOCUMENT_BINARY IS NULL THEN 'False' ELSE 'True' END, DOCUMENT_FREE_NOTES
FROM TB_ITEM_LINK IL2
INNER JOIN TB_ITEM_DOCUMENT ID2 ON ID2.ITEM_ID = IL2.ITEM_ID_PRIMARY and IL2.ITEM_ID_PRIMARY != IL2.ITEM_ID_SECONDARY
INNER JOIN TB_ITEM I3 ON I3.ITEM_ID = ID2.ITEM_ID
WHERE ITEM_ID_SECONDARY = @ITEM_ID

ORDER BY IDX ASC


SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDocumentList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemDocumentUpdate]
(
	@ITEM_DOCUMENT_ID BIGINT,
	@DOCUMENT_TITLE NVARCHAR(255),
	@DOCUMENT_FREE_NOTES NVARCHAR(MAX) = ''
)

As

SET NOCOUNT ON

	UPDATE TB_ITEM_DOCUMENT
	SET DOCUMENT_TITLE = @DOCUMENT_TITLE,
	DOCUMENT_FREE_NOTES = @DOCUMENT_FREE_NOTES
	WHERE ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDocumentUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateDeleteGroup]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	deletes data from group tables and optionally from tb_item_review
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateDeleteGroup]
	-- Add the parameters for the stored procedure here
	@ReviewID int,
	@GroupID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- find the master item_ID
	declare @master_ID bigint = (select ITEM_ID from TB_ITEM_DUPLICATE_GROUP g
		inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on g.REVIEW_ID = @ReviewID and g.ITEM_DUPLICATE_GROUP_ID = @GroupID
			and g.ITEM_DUPLICATE_GROUP_ID = gm.ITEM_DUPLICATE_GROUP_ID and g.MASTER_MEMBER_ID = gm.GROUP_MEMBER_ID
		inner join TB_ITEM_REVIEW ir on gm.ITEM_REVIEW_ID = ir.ITEM_REVIEW_ID)
	-- restore to "not checked" all items involved that appear in some other group, leave alone master items
	Update gm set IS_CHECKED = 0, IS_DUPLICATE = 0
		from TB_ITEM_REVIEW IR
			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on IR.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
			inner join TB_ITEM_DUPLICATE_GROUP g on gm.ITEM_DUPLICATE_GROUP_ID = g.ITEM_DUPLICATE_GROUP_ID and g.MASTER_MEMBER_ID != gm.GROUP_MEMBER_ID
		where ir.REVIEW_ID = @ReviewID and MASTER_ITEM_ID = @master_ID and g.ITEM_DUPLICATE_GROUP_ID != @GroupID
	-- restore to included all items that were duplicates of current master
	Update TB_ITEM_REVIEW set IS_INCLUDED = 1, IS_DELETED = 0, MASTER_ITEM_ID = null 
		from TB_ITEM_REVIEW IR
		where REVIEW_ID = @ReviewID and MASTER_ITEM_ID = @master_ID
	--delete group
	Delete from TB_ITEM_DUPLICATE_GROUP where REVIEW_ID = @ReviewID and ITEM_DUPLICATE_GROUP_ID = @GroupID
	--this deletes all as there is cascade relationship with TB_ITEM_DUPLICATE_GROUP_MEMBERS
END



GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateDeleteGroup] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateDirtyGroupMembers]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateDirtyGroupMembers]
	-- Add the parameters for the stored procedure here
	@RevID int,
	@IDs varchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    SELECT  
			0 as GROUP_MEMBER_ID
			, 0 as ITEM_DUPLICATE_GROUP_ID
			,ITEM_REVIEW_ID
			, 0 as SCORE
			, 0 as IS_CHECKED
			, 0 as IS_DUPLICATE
			, 0 as IS_MASTER
			, I.ITEM_ID
			, TITLE
			, SHORT_TITLE
			, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS
			, PARENT_TITLE
			, "YEAR"
			, "MONTH"
			, PAGES
			, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENT_AUTHORS
			, TYPE_NAME
			, (SELECT COUNT (ITEM_ATTRIBUTE_ID) FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ATTRIBUTE_SET ATS on IA.ATTRIBUTE_ID = ATS.ATTRIBUTE_ID
				INNER JOIN TB_REVIEW_SET RS on ATS.SET_ID = RS.SET_ID AND RS.REVIEW_ID = IR.REVIEW_ID
				 WHERE IA.ITEM_ID = I.ITEM_ID) CODED_COUNT
			, (SELECT COUNT (ITEM_DOCUMENT_ID) FROM TB_ITEM_DOCUMENT d WHERE d.ITEM_ID = I.ITEM_ID) DOC_COUNT
			, ( SELECT SOURCE_NAME from TB_SOURCE s inner join TB_ITEM_SOURCE tis 
					on s.SOURCE_ID = tis.SOURCE_ID and tis.ITEM_ID = I.ITEM_ID
			  ) as "SOURCE"
			, (SELECT 
				CASE 
				when COUNT(GROUP_MEMBER_ID) >0 then 1
				else 0
				end
				from TB_ITEM_DUPLICATE_GROUP_MEMBERS where ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
			) AS IS_EXPORTED
			, (SELECT COUNT(GROUP_MEMBER_ID)
				from TB_ITEM_DUPLICATE_GROUP_MEMBERS where ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
			) AS RELATED_COUNT
			, (SELECT 
				CASE 
				when COUNT(GROUP_MEMBER_ID) >0 then 0
				else 1
				end
				from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM
				INNER JOIN TB_ITEM_DUPLICATE_GROUP G on GM.GROUP_MEMBER_ID = G.MASTER_MEMBER_ID
				where ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
			) AS IS_AVAILABLE
	from TB_ITEM_REVIEW IR 
	INNER JOIN TB_ITEM I on IR.ITEM_ID = I.ITEM_ID
	INNER JOIN TB_ITEM_TYPE IT on I.TYPE_ID = IT.TYPE_ID
	WHERE REVIEW_ID = @RevID and I.ITEM_ID in 
		(
			select value from dbo.fn_Split_int(@IDs, ',')
		)
	order by I.ITEM_ID
END



GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateDirtyGroupMembers] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateFindNew]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sergio
-- Create date: 18/08/2010
-- Description:	BIG query to search for duplicates, will not delete or overwrite old items, it will add new duplicate canditades. 
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateFindNew]
	-- Add the parameters for the stored procedure here
	@revID int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	-- First check: if there are no items to evaluate, just go back
	declare @check int = (SELECT COUNT(ITEM_ID) from TB_ITEM_REVIEW where REVIEW_ID = @revID and IS_DELETED = 0)
	if @check = 0 
	BEGIN
		Return -1
	END
	SET  @check =(SELECT COUNT(DISTINCT(EXTR_UI)) from TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID)
	IF @check = 1 
	BEGIN
		return 1 --a SISS package is still running for Review, we should not run it again
	END
	ELSE IF @check > 1 --this should not happen: SISS package saved some data, but the result was not collected. 
	-- Since new items might have been inserted in the mean time, we will delete old results and start over again.
	BEGIN
		DELETE FROM TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID 
	END

	declare @UI uniqueidentifier
	set @UI = '00000000-0000-0000-0000-000000000000'
	--insert a marker line to notify that the SISS package has been triggered
	insert into TB_ITEM_DUPLICATES_TEMP (REVIEW_ID, EXTR_UI) Values (@revID, @UI)

	set @UI = NEWID()

	--run the package that populates the temp results table
	declare @cmd varchar(1000)
	select @cmd = 'dtexec /DT "File System\DuplicateCheckAzure"'
	select @cmd = @cmd + ' /Rep N  /SET \Package.Variables[User::RevID].Properties[Value];"' + CAST(@revID as varchar(max))+ '"' 
	select @cmd = @cmd + ' /SET \Package.Variables[User::UID].Properties[Value];"' + CAST(@UI as varchar(max))+ '"' 
	EXEC xp_cmdshell @cmd
	
	--delete sigleton rows from SSIS results
	DELETE from TB_ITEM_DUPLICATES_TEMP 
	where EXTR_UI = @UI AND _key_out not in
		(
			Select t1._key_in from TB_ITEM_DUPLICATES_TEMP t1 
			inner join TB_ITEM_DUPLICATES_TEMP t2 on t1._key_in = t2._key_out and t1._key_in <> t2._key_in
			and t1.EXTR_UI = t2.EXTR_UI and t1.EXTR_UI = @UI
				GROUP by t1._key_in
		)
	
	--the difficult part: match the results in TB_ITEM_DUPLICATES_TEMP with existing groups
	-- the system works indifferently for missing groups and missing groups members, and to make it relatively fast, 
	-- we store the "destination" group ID in the temporary table, this is done in two parts,
	--the following query matches the current SSIS results with existing groups and sets the DESTINATION field accordingly
	--the remaining Null "destination" fields will signal that the group is new and has to be created
	--after creating the new groups, the destination field will be populated for the remaining records and finally the new group
	--members will be added to existing groups.

	declare @i1i2 table (i1 int, i2 int) 
					insert into @i1i2
					select s.ITEM_ID, ss.ITEM_ID  from TB_ITEM_DUPLICATES_TEMP s 
										inner join TB_ITEM_DUPLICATES_TEMP ss 
										on s._key_out = ss._key_out and s._key_in = ss._key_out 
										and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
										and s.REVIEW_ID = @revID and ss.REVIEW_ID = @revID



	UPDATE dt set DESTINATION = a.ITEM_DUPLICATE_GROUP_ID
	 FROM TB_ITEM_DUPLICATES_TEMP dt INNER JOIN   
		(
			SELECT m.ITEM_DUPLICATE_GROUP_ID, COUNT(m.GROUP_MEMBER_ID) cc, ins._key_out Results_Group 
				From TB_ITEM_DUPLICATE_GROUP_MEMBERS m 
					inner join TB_ITEM_REVIEW IR on m.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.REVIEW_ID = @revID
					inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS m1 
						on m.item_duplicate_group_ID = m1.item_duplicate_group_ID
						AND m.ITEM_REVIEW_ID <> m1.ITEM_REVIEW_ID
					Inner join TB_ITEM_REVIEW IR1 on m1.ITEM_REVIEW_ID = IR1.ITEM_REVIEW_ID and IR1.REVIEW_ID = @revID
					inner Join TB_ITEM_DUPLICATE_GROUP g on  m.item_duplicate_group_ID = g.item_duplicate_group_ID
					Inner join TB_ITEM_DUPLICATES_TEMP ins on ins.ITEM_ID = IR.ITEM_ID
					Inner join @i1i2 i1i2 on i1i2.i1 = IR.ITEM_ID AND i1i2.i2 = IR1.ITEM_ID
					where g.Review_ID = @revID and ins.REVIEW_ID = @revID and ins.EXTR_UI = @UI
					--AND (CAST(IR.ITEM_ID as nvarchar(20)) + '#' + CAST(IR1.ITEM_ID as nvarchar(20))) in 
					--		(
					--			select * from @has
					--			--select (CAST(s.ITEM_ID as nvarchar(1000)) + '#' + CAST(ss.ITEM_ID as nvarchar(1000))) from TB_ITEM_DUPLICATES_TEMP s 
					--			--	inner join TB_ITEM_DUPLICATES_TEMP ss 
					--			--	on s._key_out = ss._key_out and s._key_in = ss._key_out 
					--			--	and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
					--			--	and s.REVIEW_ID = @revID and ss.REVIEW_ID = @revID
					--		)
				group by m.ITEM_DUPLICATE_GROUP_ID, ins._key_out
		) a 
		on dt._key_out = a.Results_Group
		WHERE a.cc > 0  and dt.EXTR_UI = @UI

	--for groups that are not already present: add group & master
	insert into TB_ITEM_DUPLICATE_GROUP (REVIEW_ID, ORIGINAL_ITEM_ID)
		SELECT REVIEW_ID, Item_ID from TB_ITEM_DUPLICATES_TEMP where 
			EXTR_UI = @UI
			AND DESTINATION is null
			AND _key_in = _key_out --this is how you identify groups...
	--add the master record in the members table
	INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
	(
		ITEM_DUPLICATE_GROUP_ID
		,ITEM_REVIEW_ID
		,SCORE
		,IS_CHECKED
		,IS_DUPLICATE
	)
	SELECT ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID, 1, 1, 0
	FROM TB_ITEM_DUPLICATE_GROUP DG inner join TB_ITEM_DUPLICATES_TEMP dt 
		on DG.ORIGINAL_ITEM_ID = dt.ITEM_ID
		AND EXTR_UI = @UI
		AND DESTINATION is null
		AND _key_in = _key_out
		INNER JOIN TB_ITEM_REVIEW IR  on IR.ITEM_ID = DG.ORIGINAL_ITEM_ID and IR.REVIEW_ID = @revID

	--update TB_ITEM_DUPLICATE_GROUP to set the MASTER_MEMBER_ID correctly (now that we have a line in TB_ITEM_DUPLICATE_GROUP_MEMBERS)
	UPDATE TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = a.GMI
	FROM (
		SELECT GROUP_MEMBER_ID GMI, IR.ITEM_ID from TB_ITEM_DUPLICATE_GROUP idg
			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on gm.ITEM_DUPLICATE_GROUP_ID = idg.ITEM_DUPLICATE_GROUP_ID and MASTER_MEMBER_ID is null
			inner join TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID AND IR.REVIEW_ID = @revID
			inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
			AND EXTR_UI = @UI AND dt._key_in = dt._key_out and dt.DESTINATION is null
	) a  
	WHERE ORIGINAL_ITEM_ID = a.ITEM_ID and MASTER_MEMBER_ID is null

	
	-- add the newly created group IDs to temporary table
	UPDATE TB_ITEM_DUPLICATES_TEMP set DESTINATION = a.DGI
	FROM (
		SELECT ITEM_DUPLICATE_GROUP_ID DGI, dt.ITEM_ID MAST, dt1.ITEM_ID CURR_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
			inner JOIN TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
			inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
			AND EXTR_UI = @UI AND dt._key_in = dt._key_out 
			inner JOIN TB_ITEM_DUPLICATES_TEMP dt1 on dt._key_in = dt1._key_out and dt.DESTINATION is null
			and dt1.EXTR_UI = @UI --!!!!!!!!!!!!!!
	) a
	where a.CURR_ID = TB_ITEM_DUPLICATES_TEMP.ITEM_ID

	-- add non master members that are not currently present
	--INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
	--(
	--	ITEM_DUPLICATE_GROUP_ID
	--	,ITEM_REVIEW_ID
	--	,SCORE
	--	,IS_CHECKED
	--	,IS_DUPLICATE
	--)
	--SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
	--from TB_ITEM_DUPLICATES_TEMP DT
	--inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
	--WHERE DT.ITEM_ID not in 
	--(
	--	SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
	--	inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
	--	inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
	--	on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
	--)
	declare  @t table (goodIDs bigint)
		insert into @t select distinct item_id from TB_ITEM_DUPLICATES_TEMP where EXTR_UI = @UI --and _key_in != _key_out 
		select COUNT (goodids) from @t
		delete from @t where goodIDs in (
			SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
			inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
			on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
			)
		select COUNT (goodids) from @t
		
		-- add non master members that are not currently present
		INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
		(
			ITEM_DUPLICATE_GROUP_ID
			,ITEM_REVIEW_ID
			,SCORE
			,IS_CHECKED
			,IS_DUPLICATE
		)
		SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
		from TB_ITEM_DUPLICATES_TEMP DT
		inner join @t t on DT.ITEM_ID = t.goodIDs
		inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
	--remove temporary results.
	delete FROM TB_ITEM_DUPLICATES_TEMP WHERE REVIEW_ID = @revID
	END

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateFindNew] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupAddNew]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Sergio>
-- Create date: 11/03/2011
-- Description:	adds a manually created group
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateGroupAddNew]
	-- Add the parameters for the stored procedure here
	@RevID int,
	@MasterID bigint,
	@IDs varchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @group_id int;
    -- Insert statements for procedure here
	insert into TB_ITEM_DUPLICATE_GROUP (REVIEW_ID, ORIGINAL_ITEM_ID)
		Values (@RevID, @MasterID)
	set @group_id = @@IDENTITY
	--add the master and normal records in the members table
	INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
	(
		ITEM_DUPLICATE_GROUP_ID
		,ITEM_REVIEW_ID
		,SCORE
		,IS_CHECKED
		,IS_DUPLICATE
	)
	SELECT ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID, 1, 1, 0
	FROM TB_ITEM_DUPLICATE_GROUP DG
		INNER JOIN TB_ITEM_REVIEW IR  on IR.ITEM_ID = DG.ORIGINAL_ITEM_ID and IR.REVIEW_ID = @revID 
		and DG.ORIGINAL_ITEM_ID = @MasterID and ITEM_DUPLICATE_GROUP_ID = @group_id
	UNION
	SELECT @group_id, ITEM_REVIEW_ID, 0, 1, 1
	from TB_ITEM_REVIEW where REVIEW_ID = @RevID and ITEM_ID in 
		(
			select value from dbo.fn_Split_int(@IDs, ',')
		)
	--update TB_ITEM_DUPLICATE_GROUP to set the MASTER_MEMBER_ID correctly (now that we have a line in TB_ITEM_DUPLICATE_GROUP_MEMBERS)
	UPDATE TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = a.GMI
	FROM (
		SELECT GROUP_MEMBER_ID GMI, IR.ITEM_ID from TB_ITEM_DUPLICATE_GROUP idg
			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on gm.ITEM_DUPLICATE_GROUP_ID = idg.ITEM_DUPLICATE_GROUP_ID 
				and MASTER_MEMBER_ID is null and idg.ITEM_DUPLICATE_GROUP_ID = @group_id
			inner join TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID AND IR.REVIEW_ID = @revID and ITEM_ID = @MasterID
	) a  
	WHERE ORIGINAL_ITEM_ID = a.ITEM_ID and MASTER_MEMBER_ID is null
	--mark relevant items as duplicates
	UPDATE TB_ITEM_REVIEW set MASTER_ITEM_ID = @MasterID, IS_DELETED = 1, IS_INCLUDED =1
	where REVIEW_ID = @RevID and ITEM_ID in 
		(
			select value from dbo.fn_Split_int(@IDs, ',')
		)
	--mark relevant items as checked and not duplicates in other groups
	update TB_ITEM_DUPLICATE_GROUP_MEMBERS set IS_DUPLICATE =  0, IS_CHECKED = 1 
	from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM
		where ITEM_REVIEW_ID in 
		(
			select ITEM_REVIEW_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS where ITEM_DUPLICATE_GROUP_ID = @group_id
		) and GM.ITEM_DUPLICATE_GROUP_ID != @group_id
	 
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateGroupAddNew] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupCheckOngoing]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sergio
-- Create date: 09/08/2010
-- Description:	check for pending SISS, attempt to save results
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateGroupCheckOngoing] 
	-- Add the parameters for the stored procedure here
	@revID int
	WITH RECOMPILE
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @guis_N int
    -- Insert statements for procedure here
	set @guis_N = (
					SELECT COUNT(DISTINCT(EXTR_UI)) from TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID
					AND EXTR_UI <> '10000000-0000-0000-0000-000000000000'
					)
	IF @guis_N = 1
	BEGIN --send back a return code to signify that the SISS package is still running
		return -2
	END
	ELSE
	IF @guis_N > 1 --SISS package has saved data but results were never collected
	BEGIN
		declare @UI uniqueidentifier
		UPDATE TB_ITEM_DUPLICATES_TEMP
			SET EXTR_UI = '10000000-0000-0000-0000-000000000000'
			WHERE EXTR_UI = '00000000-0000-0000-0000-000000000000' AND REVIEW_ID = @revID
		
		set @UI = (SELECT top 1 EXTR_UI from TB_ITEM_DUPLICATES_TEMP where EXTR_UI <> '10000000-0000-0000-0000-000000000000' AND REVIEW_ID = @revID)
		SET @guis_N = (SELECT COUNT(ITEM_DUPLICATES_ID) from TB_ITEM_DUPLICATES_TEMP where EXTR_UI = @UI and DESTINATION is null)
		if @guis_N > 0 --do the costly bits only if they weren't done already
		BEGIN
		--delete sigleton rows from SSIS results
			DELETE from TB_ITEM_DUPLICATES_TEMP 
			where EXTR_UI = @UI AND _key_out not in
				(
					Select t1._key_in from TB_ITEM_DUPLICATES_TEMP t1 
					inner join TB_ITEM_DUPLICATES_TEMP t2 on t1._key_in = t2._key_out and t1._key_in <> t2._key_in
					and t1.EXTR_UI = t2.EXTR_UI and t1.EXTR_UI = @UI
						GROUP by t1._key_in
				)
			
			--the difficult part: match the results in TB_ITEM_DUPLICATES_TEMP with existing groups
			-- the system works indifferently for missing groups and missing groups members, and to make it relatively fast, 
			-- we store the "destination" group ID in the temporary table, this is done in two parts,
			--the following query matches the current SSIS results with existing groups and sets the DESTINATION field accordingly
			--the remaining Null "destination" fields will signal that the group is new and has to be created
			--after creating the new groups, the destination field will be populated for the remaining records and finally the new group
			--members will be added to existing groups.

			declare @i1i2 table (i1 int, i2 int) 
					insert into @i1i2
					select s.ITEM_ID, ss.ITEM_ID  from TB_ITEM_DUPLICATES_TEMP s 
										inner join TB_ITEM_DUPLICATES_TEMP ss 
										on s._key_out = ss._key_out and s._key_in = ss._key_out 
										and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
										and s.REVIEW_ID = @revID and ss.REVIEW_ID = @revID

			UPDATE dt set DESTINATION = a.ITEM_DUPLICATE_GROUP_ID
			 FROM TB_ITEM_DUPLICATES_TEMP dt INNER JOIN   
				(
					SELECT m.ITEM_DUPLICATE_GROUP_ID, COUNT(m.GROUP_MEMBER_ID) cc, ins._key_out Results_Group 
					From TB_ITEM_DUPLICATE_GROUP_MEMBERS m 
						inner join TB_ITEM_REVIEW IR on m.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.REVIEW_ID = @revID
						inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS m1 
							on m.item_duplicate_group_ID = m1.item_duplicate_group_ID
							AND m.ITEM_REVIEW_ID <> m1.ITEM_REVIEW_ID
						Inner join TB_ITEM_REVIEW IR1 on m1.ITEM_REVIEW_ID = IR1.ITEM_REVIEW_ID and IR1.REVIEW_ID = @revID
						inner Join TB_ITEM_DUPLICATE_GROUP g on  m.item_duplicate_group_ID = g.item_duplicate_group_ID
						Inner join TB_ITEM_DUPLICATES_TEMP ins on ins.ITEM_ID = IR.ITEM_ID
						Inner join @i1i2 i1i2 on i1i2.i1 = IR.ITEM_ID AND i1i2.i2 = IR1.ITEM_ID
						where g.Review_ID = @revID and ins.REVIEW_ID = @revID and ins.EXTR_UI = @UI
						--AND (CAST(IR.ITEM_ID as nvarchar(20)) + '#' + CAST(IR1.ITEM_ID as nvarchar(20))) in 
						--		(
						--			select * from @has
						--			--select (CAST(s.ITEM_ID as nvarchar(1000)) + '#' + CAST(ss.ITEM_ID as nvarchar(1000))) from TB_ITEM_DUPLICATES_TEMP s 
						--			--	inner join TB_ITEM_DUPLICATES_TEMP ss 
						--			--	on s._key_out = ss._key_out and s._key_in = ss._key_out 
						--			--	and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
						--			--	and s.REVIEW_ID = @revID and ss.REVIEW_ID = @revID
						--		)
					group by m.ITEM_DUPLICATE_GROUP_ID, ins._key_out
				) a 
				on dt._key_out = a.Results_Group 
				WHERE a.cc > 0 and dt.EXTR_UI = @UI

			--for groups that are not already present: add group & master
			insert into TB_ITEM_DUPLICATE_GROUP (REVIEW_ID, ORIGINAL_ITEM_ID)
				SELECT REVIEW_ID, Item_ID from TB_ITEM_DUPLICATES_TEMP where 
					EXTR_UI = @UI
					AND DESTINATION is null
					AND _key_in = _key_out --this is how you identify groups...
			--add the master record in the members table
			INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
			(
				ITEM_DUPLICATE_GROUP_ID
				,ITEM_REVIEW_ID
				,SCORE
				,IS_CHECKED
				,IS_DUPLICATE
			)
			SELECT ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID, 1, 1, 0
			FROM TB_ITEM_DUPLICATE_GROUP DG inner join TB_ITEM_DUPLICATES_TEMP dt 
				on DG.ORIGINAL_ITEM_ID = dt.ITEM_ID
				AND EXTR_UI = @UI
				AND DESTINATION is null
				AND _key_in = _key_out
				INNER JOIN TB_ITEM_REVIEW IR  on IR.ITEM_ID = DG.ORIGINAL_ITEM_ID and IR.REVIEW_ID = @revID

			--update TB_ITEM_DUPLICATE_GROUP to set the MASTER_MEMBER_ID correctly (now that we have a line in TB_ITEM_DUPLICATE_GROUP_MEMBERS)
			UPDATE TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = a.GMI
			FROM (
					SELECT GROUP_MEMBER_ID GMI, IR.ITEM_ID from TB_ITEM_DUPLICATE_GROUP idg
						inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on gm.ITEM_DUPLICATE_GROUP_ID = idg.ITEM_DUPLICATE_GROUP_ID and MASTER_MEMBER_ID is null
						inner join TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID AND IR.REVIEW_ID = @revID
						inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
						AND EXTR_UI = @UI AND dt._key_in = dt._key_out and dt.DESTINATION is null
			) a  
			WHERE ORIGINAL_ITEM_ID = a.ITEM_ID and MASTER_MEMBER_ID is null

			
			-- add the newly created group IDs to temporary table
			UPDATE TB_ITEM_DUPLICATES_TEMP set DESTINATION = a.DGI
			FROM (
				SELECT ITEM_DUPLICATE_GROUP_ID DGI, dt.ITEM_ID MAST, dt1.ITEM_ID CURR_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
					inner JOIN TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
					inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
					AND EXTR_UI = @UI AND dt._key_in = dt._key_out 
					inner JOIN TB_ITEM_DUPLICATES_TEMP dt1 on dt._key_in = dt1._key_out and dt.DESTINATION is null
					and dt1.EXTR_UI = @UI
			) a
			where a.CURR_ID = TB_ITEM_DUPLICATES_TEMP.ITEM_ID
		END
		-- add non master members that are not currently present
		declare  @t table (goodIDs bigint)
		insert into @t select distinct item_id from TB_ITEM_DUPLICATES_TEMP where EXTR_UI = @UI --and _key_in != _key_out 
		--select COUNT (goodids) from @t
		delete from @t where goodIDs in (
			SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
			inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
			on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
			)
		--select COUNT (goodids) from @t
		
		-- add non master members that are not currently present
		INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
		(
			ITEM_DUPLICATE_GROUP_ID
			,ITEM_REVIEW_ID
			,SCORE
			,IS_CHECKED
			,IS_DUPLICATE
		)
		SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
		from TB_ITEM_DUPLICATES_TEMP DT
		inner join @t t on DT.ITEM_ID = t.goodIDs
		inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
		
		--INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
		--(
		--	ITEM_DUPLICATE_GROUP_ID
		--	,ITEM_REVIEW_ID
		--	,SCORE
		--	,IS_CHECKED
		--	,IS_DUPLICATE
		--)
		--SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
		--from TB_ITEM_DUPLICATES_TEMP DT
		--inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
		--WHERE DT.ITEM_ID not in 
		--(
		--	SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
		--	inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
		--	inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
		--	on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
		--)
		
		--remove temporary results.
		delete FROM TB_ITEM_DUPLICATES_TEMP WHERE REVIEW_ID = @revID
	END
END

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateGroupCheckOngoing] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: 13/08/10
-- Description:	get the list of groups for a review with their current master ID
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateGroupList]
	-- Add the parameters for the stored procedure here
	@RevID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    --delete groups that don't make sense anymore: i.e. groups with 1 item only (this may happen if deleting forever duplicate candidates)
    DELETE TB_ITEM_DUPLICATE_GROUP WHERE ITEM_DUPLICATE_GROUP_ID IN 
	(
		SELECT G.ITEM_DUPLICATE_GROUP_ID from TB_ITEM_DUPLICATE_GROUP G
		Inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on GM.ITEM_DUPLICATE_GROUP_ID = G.ITEM_DUPLICATE_GROUP_ID and G.REVIEW_ID = @RevID
		group by G.ITEM_DUPLICATE_GROUP_ID
		having COUNT(gm.ITEM_DUPLICATE_GROUP_ID) = 1
	)

	SELECT top 80000 g.ITEM_DUPLICATE_GROUP_ID GROUP_ID, IR.ITEM_ID MASTER_ITEM_ID, SHORT_TITLE, g.REVIEW_ID
		,CASE WHEN (COUNT(distinct(gm2.IS_CHECKED)) = 1) then 1 --master item is always marked "is checked" so if only one value is used for the group it has to be IS_CHECKED=true
			ELSE 0
		END
		as IS_COMPLETE		
		from TB_ITEM_DUPLICATE_GROUP g
		INNER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on g.MASTER_MEMBER_ID = gm.GROUP_MEMBER_ID
		INNER JOIN TB_ITEM_REVIEW IR on IR.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
		INNER JOIN TB_ITEM I on IR.ITEM_ID = I.ITEM_ID
		INNER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS gm2 on g.ITEM_DUPLICATE_GROUP_ID = gm2.ITEM_DUPLICATE_GROUP_ID
	WHERE g.REVIEW_ID = @RevID
	GROUP BY g.ITEM_DUPLICATE_GROUP_ID, IR.ITEM_ID, SHORT_TITLE, g.REVIEW_ID
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateGroupList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupListSearch]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	gets the list of groups that match the search criteria
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateGroupListSearch]
	-- Add the parameters for the stored procedure here
	@ItemIDs nvarchar(max) = '0',
	@revID int = 0,
	@GroupID int = 0
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    -- 0 delete groups that don't make sense anymore: i.e. groups with 1 item only (this may happen if deleting forever duplicate candidates)
    DELETE TB_ITEM_DUPLICATE_GROUP WHERE ITEM_DUPLICATE_GROUP_ID IN 
	(
		SELECT G.ITEM_DUPLICATE_GROUP_ID from TB_ITEM_DUPLICATE_GROUP G
		Inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on GM.ITEM_DUPLICATE_GROUP_ID = G.ITEM_DUPLICATE_GROUP_ID and G.REVIEW_ID = @RevID
		group by G.ITEM_DUPLICATE_GROUP_ID
		having COUNT(gm.ITEM_DUPLICATE_GROUP_ID) = 1
	)
	--1 get itemIDs
	declare @t TABLE (IDs bigint)
	declare @g TABLE (GIDs int)
    if @ItemIDs != '0' and @GroupID = 0 insert into @t (IDs) 
		SELECT value from dbo.fn_Split_int(@ItemIDs, ',') s 
		inner join TB_ITEM_REVIEW ir on s.value = ir.ITEM_ID and ir.REVIEW_ID = @revID
    else if @ItemIDs = '0' and @GroupID != 0 
    begin
		insert into @t (IDs)
			Select ITEM_ID from TB_ITEM_REVIEW ir inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
				on ir.REVIEW_ID = @revID and gm.ITEM_REVIEW_ID = ir.ITEM_REVIEW_ID and gm.ITEM_DUPLICATE_GROUP_ID = @GroupID
			
    end
    --select * from @t
    --1.1 add the ItemIDs of items that are manual duplicates of the current list
    insert into @t (IDs)
		Select ir2.ITEM_ID from TB_ITEM_REVIEW ir 
		inner join @t t on t.IDs = ir.ITEM_ID and ir.REVIEW_ID = @revID
		inner join TB_ITEM_REVIEW ir2 on ir.MASTER_ITEM_ID = ir2.ITEM_ID and ir2.REVIEW_ID = @revID
		
		where ir2.ITEM_ID not in (Select IDs from @t)
    --ir2.ITEM_ID from TB_ITEM_REVIEW ir inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
				--on ir.REVIEW_ID = @revID and gm.ITEM_REVIEW_ID = ir.ITEM_REVIEW_ID and gm.ITEM_DUPLICATE_GROUP_ID = @GroupID
				--inner join TB_ITEM_REVIEW ir2 on ir.ITEM_ID = ir2.MASTER_ITEM_ID
    --select * from @t
    --2 get groups they belong to
    insert into @g (GIDs)
			--groups they belong to
			select g.ITEM_DUPLICATE_GROUP_ID from TB_ITEM_DUPLICATE_GROUP g 
				inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
					on g.ITEM_DUPLICATE_GROUP_ID = gm.ITEM_DUPLICATE_GROUP_ID and g.REVIEW_ID = @revID
				inner join TB_ITEM_REVIEW ir on gm.ITEM_REVIEW_ID = ir.ITEM_REVIEW_ID and ir.REVIEW_ID = @revID
				inner Join @t on ir.ITEM_ID = IDs
			UNION
			--Groups they belong to as manually added items
			Select g.ITEM_DUPLICATE_GROUP_ID from TB_ITEM_DUPLICATE_GROUP g
				inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
					on g.ITEM_DUPLICATE_GROUP_ID = gm.ITEM_DUPLICATE_GROUP_ID and g.REVIEW_ID = @revID
				inner join TB_ITEM_REVIEW ir on gm.ITEM_REVIEW_ID = ir.ITEM_REVIEW_ID and ir.REVIEW_ID = @revID
				inner Join @t on ir.MASTER_ITEM_ID = IDs
	--select * from @g
	--3 get the corresponding list of groups
	SELECT g.ITEM_DUPLICATE_GROUP_ID GROUP_ID, IR.ITEM_ID MASTER_ITEM_ID, SHORT_TITLE, g.REVIEW_ID
		,CASE WHEN (COUNT(distinct(gm2.IS_CHECKED)) = 1) then 1 --master item is always marked "is checked" so if only one value is used for the group it has to be IS_CHECKED=true
			ELSE 0
		END
		as IS_COMPLETE		
		from TB_ITEM_DUPLICATE_GROUP g
		INNER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on g.MASTER_MEMBER_ID = gm.GROUP_MEMBER_ID
		INNER JOIN TB_ITEM_REVIEW IR on IR.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
		INNER JOIN TB_ITEM I on IR.ITEM_ID = I.ITEM_ID
		INNER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS gm2 on g.ITEM_DUPLICATE_GROUP_ID = gm2.ITEM_DUPLICATE_GROUP_ID
		INNER JOIN @g on GIDs = g.ITEM_DUPLICATE_GROUP_ID
	WHERE g.REVIEW_ID = @RevID
	GROUP BY g.ITEM_DUPLICATE_GROUP_ID, IR.ITEM_ID, SHORT_TITLE, g.REVIEW_ID
END




GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateGroupListSearch] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupManualAddItem]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: <Create Date,,>
-- Description:	Manually add Item to group, @MasterID is the destination master Item_ID
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateGroupManualAddItem]
	-- Add the parameters for the stored procedure here
	@MasterID int,
	@RevID int,
	@NewDuplicateItemID bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @GroupID int
	--get the group ID
	select @GroupID = G.ITEM_DUPLICATE_GROUP_ID from TB_ITEM_DUPLICATE_GROUP G
		inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on G.MASTER_MEMBER_ID = GM.GROUP_MEMBER_ID and G.REVIEW_ID = @RevID
		inner join TB_ITEM_REVIEW IR on IR.ITEM_REVIEW_ID = GM.ITEM_REVIEW_ID and IR.ITEM_ID = @MasterID
	if --if the item we are adding already belongs to the group, do nothing!
		(	
			(
				select COUNT(ITEM_ID) from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM inner join TB_ITEM_REVIEW IR 
				on GM.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.ITEM_ID = @NewDuplicateItemID and ITEM_DUPLICATE_GROUP_ID = @GroupID
			)
			> 0
		 ) Return;
	BEGIN TRY
	BEGIN TRANSACTION
	
	--mark ischecked and not duplicate all appeareances of manual item into group_members
	Update TB_ITEM_DUPLICATE_GROUP_MEMBERS
	 set IS_CHECKED = 1, IS_DUPLICATE = 0
	 from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
	 inner Join TB_ITEM_REVIEW ir on ir.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
	where ITEM_ID = @NewDuplicateItemID and REVIEW_ID = @RevID and ITEM_DUPLICATE_GROUP_ID != @GroupID
	
	
	Update TB_ITEM_REVIEW set MASTER_ITEM_ID = @MasterID, IS_INCLUDED = 1, IS_DELETED = 1
	where ITEM_ID = @NewDuplicateItemID and REVIEW_ID = @RevID AND ITEM_ID != @MasterID
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	END CATCH
END



GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateGroupManualAddItem] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupManualMembers]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: <Create Date,,>
-- Description:	get items that have been manually added to group
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateGroupManualMembers]
	-- Add the parameters for the stored procedure here
	@GroupID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select I.ITEM_ID
		, TITLE
		, SHORT_TITLE
		, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS
		, PARENT_TITLE
		, "YEAR"
		, "MONTH"
		, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENT_AUTHORS
		, TYPE_NAME 
		from TB_ITEM_DUPLICATE_GROUP g
		inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on g.MASTER_MEMBER_ID = gm.GROUP_MEMBER_ID
		inner join TB_ITEM_REVIEW ir on gm.ITEM_REVIEW_ID = ir.ITEM_REVIEW_ID
		inner join TB_ITEM_REVIEW ir1 on ir.ITEM_ID = ir1.MASTER_ITEM_ID and ir.REVIEW_ID = ir1.REVIEW_ID
		inner join TB_ITEM i on ir1.ITEM_ID = i.ITEM_ID
		inner join TB_ITEM_TYPE it on i.TYPE_ID = it.TYPE_ID
	where g.ITEM_DUPLICATE_GROUP_ID = @GroupID and ir1.ITEM_REVIEW_ID not in 
		(
			select ITEM_REVIEW_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm2
				where gm2.ITEM_DUPLICATE_GROUP_ID = @GroupID
		)
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateGroupManualMembers] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupManualMerge]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: <Create Date,,>
-- Description:	Merge groups, @MasterID is the destination master GROUP_MEMBER_ID
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateGroupManualMerge]
	-- Add the parameters for the stored procedure here
	@MasterID int, 
	@SourceGroupID int,
	@RevID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	BEGIN TRY
	BEGIN TRANSACTION
    
	Update TB_ITEM_DUPLICATE_GROUP_MEMBERS set IS_CHECKED = 1, IS_DUPLICATE = 0
	from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
	inner join TB_ITEM_REVIEW ir on gm.ITEM_REVIEW_ID = ir.ITEM_REVIEW_ID
	where ITEM_DUPLICATE_GROUP_ID = @SourceGroupID and ir.REVIEW_ID = @RevID
	
	Update TB_ITEM_REVIEW set MASTER_ITEM_ID = @MasterID, IS_INCLUDED = 1, IS_DELETED = 1
	where ITEM_REVIEW_ID in 
		(
			select gm.ITEM_REVIEW_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
				inner join TB_ITEM_REVIEW ir on gm.ITEM_REVIEW_ID = ir.ITEM_REVIEW_ID
				where ITEM_DUPLICATE_GROUP_ID = @SourceGroupID and ir.REVIEW_ID = @RevID		
		)
		and ITEM_REVIEW_ID not in --do not mark the master of the receiving group, otherwise it will become a master of itself
		(
			select gm2.ITEM_REVIEW_ID from TB_ITEM_REVIEW ir2 
			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm2 on gm2.ITEM_REVIEW_ID = ir2.ITEM_REVIEW_ID and ir2.ITEM_ID = @MasterID
			inner join TB_ITEM_DUPLICATE_GROUP g2 on gm2.ITEM_DUPLICATE_GROUP_ID = g2.ITEM_DUPLICATE_GROUP_ID
						and gm2.GROUP_MEMBER_ID = g2.MASTER_MEMBER_ID
		)
		-- to be really sure: limit to rev_id and forbid acting on the chosen master itself
		and REVIEW_ID = @RevID and ITEM_ID != @MasterID
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	END CATCH
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateGroupManualMerge] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupManualRemoveItem]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: <Create Date,,>
-- Description:	Remove ManualItem from group
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateGroupManualRemoveItem]
	-- Add the parameters for the stored procedure here
	
	@DuplicateItemID bigint,
	@RevID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	BEGIN TRY
	BEGIN TRANSACTION
    
	Update TB_ITEM_DUPLICATE_GROUP_MEMBERS set
		 IS_CHECKED = 
		 (
		 CASE
			WHEN gm.GROUP_MEMBER_ID = g.MASTER_MEMBER_ID then 1
			else 0
		 end
		 )	
		 ,IS_DUPLICATE = 0
	from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
		inner join TB_ITEM_DUPLICATE_GROUP g on gm.ITEM_DUPLICATE_GROUP_ID = g.ITEM_DUPLICATE_GROUP_ID
					and ir.REVIEW_ID = g.REVIEW_ID
	where ITEM_ID = @DuplicateItemID and ir.REVIEW_ID = @RevID
	
	Update TB_ITEM_REVIEW set MASTER_ITEM_ID = null, IS_INCLUDED = 1, IS_DELETED = 0
	where ITEM_ID = @DuplicateItemID
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	END CATCH
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateGroupManualRemoveItem] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupMembers]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateGroupMembers]
	-- Add the parameters for the stored procedure here
	@GroupID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT GM.*
		,CASE (G.MASTER_MEMBER_ID)
			when (GM.GROUP_MEMBER_ID) Then 1
			ELSE 0
		END AS IS_MASTER
		, I.ITEM_ID
		, TITLE
		, SHORT_TITLE
		, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS
		, PARENT_TITLE
		, "YEAR"
		, "MONTH"
		, PAGES
		, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENT_AUTHORS
		, TYPE_NAME
		, (SELECT COUNT (ITEM_ATTRIBUTE_ID) FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ATTRIBUTE_SET ATS on IA.ATTRIBUTE_ID = ATS.ATTRIBUTE_ID
			INNER JOIN TB_REVIEW_SET RS on ATS.SET_ID = RS.SET_ID AND RS.REVIEW_ID = G.REVIEW_ID
			 WHERE IA.ITEM_ID = I.ITEM_ID) CODED_COUNT
		, (SELECT COUNT (ITEM_DOCUMENT_ID) FROM TB_ITEM_DOCUMENT d WHERE d.ITEM_ID = I.ITEM_ID) DOC_COUNT
		, (
				SELECT SOURCE_NAME from TB_SOURCE s inner join TB_ITEM_SOURCE tis 
				on s.SOURCE_ID = tis.SOURCE_ID and tis.ITEM_ID = I.ITEM_ID
		  ) as "SOURCE"
		, case when 
					(IR.MASTER_ITEM_ID is not null and
						(
							IR.MASTER_ITEM_ID not in 
							(--current master is not coming from current group
								Select rr.ITEM_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS mm
								inner join TB_ITEM_DUPLICATE_GROUP gg on mm.GROUP_MEMBER_ID = gg.MASTER_MEMBER_ID
								inner join TB_ITEM_REVIEW rr on rr.ITEM_REVIEW_ID = mm.ITEM_REVIEW_ID
								where gg.ITEM_DUPLICATE_GROUP_ID = @GroupID
							)
							
						)
					)
					or IR.ITEM_REVIEW_ID in 
						(--current item is master of some other group 
						 select GM1.item_review_id from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM1 
							inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm2 on GM1.GROUP_MEMBER_ID != gm2.GROUP_MEMBER_ID and GM1.ITEM_DUPLICATE_GROUP_ID = @GroupID
								and GM1.ITEM_REVIEW_ID = gm2.ITEM_REVIEW_ID and gm2.ITEM_DUPLICATE_GROUP_ID != GM1.ITEM_DUPLICATE_GROUP_ID
							inner join TB_ITEM_DUPLICATE_GROUP G2 on gm2.ITEM_DUPLICATE_GROUP_ID = G2.ITEM_DUPLICATE_GROUP_ID
								and G2.MASTER_MEMBER_ID = gm2.GROUP_MEMBER_ID
							--inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM3 on G2.ITEM_DUPLICATE_GROUP_ID = GM3.ITEM_DUPLICATE_GROUP_ID 
							--	and G2.MASTER_MEMBER_ID != GM.GROUP_MEMBER_ID
							--	and GM3.IS_DUPLICATE = 1
							
								
						 
						 
						 
						 --TB_ITEM_REVIEW rrr
							--inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS mmm on mmm.ITEM_REVIEW_ID = rrr.ITEM_REVIEW_ID
							--inner join TB_ITEM_DUPLICATE_GROUP ggg on mmm.GROUP_MEMBER_ID = ggg.MASTER_MEMBER_ID 
							--inner join TB_ITEM_REVIEW rr2 on rr2.MASTER_ITEM_ID = rrr.ITEM_ID and rr2.REVIEW_ID = rrr.REVIEW_ID
							--	and ggg.ITEM_DUPLICATE_GROUP_ID != @GroupID and rrr.REVIEW_ID = ggg.REVIEW_ID
						)
			then 1--is exported: should be 1 when the member has been manually imported as a duplicate in some other group
			--it is 1 also if the current master is from another group, on the interface this makes the group member read-only
			else 0
		  END
		AS IS_EXPORTED
	from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM
	INNER JOIN TB_ITEM_DUPLICATE_GROUP G on G.ITEM_DUPLICATE_GROUP_ID = GM.ITEM_DUPLICATE_GROUP_ID
	INNER JOIN TB_ITEM_REVIEW IR on GM.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
	INNER JOIN TB_ITEM I on IR.ITEM_ID = I.ITEM_ID
	INNER JOIN TB_ITEM_TYPE IT on I.TYPE_ID = IT.TYPE_ID
	WHERE G.ITEM_DUPLICATE_GROUP_ID = @GroupID
	
	SELECT ORIGINAL_ITEM_ID ORIGINAL_MASTER_ID from TB_ITEM_DUPLICATE_GROUP where ITEM_DUPLICATE_GROUP_ID = @GroupID
	
	
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateGroupMembers] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupMemberUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: 20/08/2010
-- Description:	Update a group member, this will also change the group master if needed.
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateGroupMemberUpdate]
	-- Add the parameters for the stored procedure here
	@memberID int
	, @groupID int
	--, @item_review_id bigint
	--, @item_id bigint
	, @is_checked bit
	, @is_duplicate bit
	, @is_master bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @IR_ID bigint
	-- get the current Item_review_id
	select @IR_ID = Item_review_id from TB_ITEM_DUPLICATE_GROUP_MEMBERS where GROUP_MEMBER_ID = @memberID
	-- update the group member record
	UPDATE TB_ITEM_DUPLICATE_GROUP_MEMBERS set
		IS_CHECKED = @is_checked
		,IS_DUPLICATE = @is_duplicate
		WHERE GROUP_MEMBER_ID = @memberID
	-- see if you need to set this as master
	IF @is_master = 1
	BEGIN
		-- see who is current master
		-- if item is master of some other group, abort
		declare @current_master int = (select MASTER_MEMBER_ID from TB_ITEM_DUPLICATE_GROUP where ITEM_DUPLICATE_GROUP_ID = @groupID)
		if (
			select COUNT(G.MASTER_MEMBER_ID) from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM inner join 
				TB_ITEM_DUPLICATE_GROUP_MEMBERS GM2 on GM.GROUP_MEMBER_ID = @memberID and GM2.ITEM_REVIEW_ID = GM.ITEM_REVIEW_ID and GM2.GROUP_MEMBER_ID != GM.GROUP_MEMBER_ID
				inner join TB_ITEM_DUPLICATE_GROUP G on GM2.ITEM_DUPLICATE_GROUP_ID = G.ITEM_DUPLICATE_GROUP_ID and GM2.GROUP_MEMBER_ID = G.MASTER_MEMBER_ID
					and G.MASTER_MEMBER_ID != @memberID
			) > 0 return;
		IF (@current_master <> @memberID)
		BEGIN --change master
			UPDATE TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = @memberID where ITEM_DUPLICATE_GROUP_ID = @groupID
			select @IR_ID = Item_review_id from TB_ITEM_DUPLICATE_GROUP_MEMBERS where GROUP_MEMBER_ID = @memberID
			-- also set as checked and not duplicate in all other groups where this item appears as not a master
			update gm set IS_DUPLICATE =  0, IS_CHECKED = 0
				from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm inner join TB_ITEM_DUPLICATE_GROUP g
					on g.ITEM_DUPLICATE_GROUP_ID = gm.ITEM_DUPLICATE_GROUP_ID and g.ITEM_DUPLICATE_GROUP_ID != @groupID
					and g.MASTER_MEMBER_ID != gm.GROUP_MEMBER_ID
					and ITEM_REVIEW_ID = @IR_ID --and ITEM_DUPLICATE_GROUP_ID != @groupID
		--change the master of items that are imported into this group
		-- need to do this on tb_item_review in this sproc because after running the above 
		--the info on the previous master is lost and can't be easily reconstructed.
			declare @ID bigint = (select item_id from TB_ITEM_REVIEW IR 
									inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on IR.ITEM_REVIEW_ID = GM.ITEM_REVIEW_ID
										and GM.GROUP_MEMBER_ID = @memberID)
			update IR set MASTER_ITEM_ID = @ID
				from TB_ITEM_REVIEW IR inner join TB_ITEM_REVIEW IR2 on IR.MASTER_ITEM_ID = IR2.ITEM_ID
				Inner Join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on IR2.ITEM_REVIEW_ID = GM.ITEM_REVIEW_ID and GM.GROUP_MEMBER_ID = @current_master
				left outer join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM2 on IR.ITEM_REVIEW_ID = GM2.ITEM_REVIEW_ID and GM2.ITEM_DUPLICATE_GROUP_ID != @groupID
				--where GM2.GROUP_MEMBER_ID is null
		END 
		
	
	End
	ELSE
		Begin
		-- set to "is checked" also all other appearences of the same item, 
		-- also set to "not a duplicate" in case this is being marked as a duplicate in the active group.
		--if @is_duplicate = 1
		--begin
			
			update TB_ITEM_DUPLICATE_GROUP_MEMBERS set IS_DUPLICATE =  0, IS_CHECKED = @is_checked 
				where ITEM_REVIEW_ID = @IR_ID and ITEM_DUPLICATE_GROUP_ID != @groupID
		END
END

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateGroupMemberUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesCheckOngoing]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: 09/08/2010
-- Description:	check for pending SISS, attempt to save results
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicatesCheckOngoing]
	-- Add the parameters for the stored procedure here
	@revID int
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @guis_N int
    -- Insert statements for procedure here
	set @guis_N = (SELECT COUNT(DISTINCT(EXTR_UI)) from TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID)
	IF @guis_N = 1
	BEGIN --send back a return code to signify that the SISS package is still running
		return -2
	END
	ELSE
	IF @guis_N = 2 --SISS package has saved data but results were never collected
	BEGIN
	
		--INSERT only the new data in the final table
		--The select statement below is an attempt to avoid subqueries: it is necessary to avoid inserting duplicates of duplicates
		--the left outer join with the target table (t1) has the ON clause that is true only if the destination line already exists,
		--being an outer join, if this condition is false the fields from the joined table will be NULL
		--All I need to do is force these values to be NULL, and I can do it safely on t1.ITEM_ID_IN only 
		--because NULL values are not allowed on that field (no need to check from both, then)
		--RESULT: if the destination record already exists I will have some value in t1.ITEM_ID_IN and the line will not be selected
		--if the destination record does not exist t1.ITEM_ID_IN will be NULL and I will select (and insert) the new line
		--NOTE: I'm not sure this will be faster than having a subquery, but I think it is...
		--We should check when we'll have big duplicates tables...
		INSERT INTO [TB_ITEM_DUPLICATES]
				   ([ITEM_ID_IN]
				   ,[_SCORE]
				   ,[ITEM_ID_OUT]
				   ,[REVIEW_ID]
				   )
		SELECT ID1.ITEM_ID ITEM_ID_IN, ID2._SCORE, ID2.ITEM_ID ITEM_ID_OUT, @revID 
			FROM TB_ITEM_DUPLICATES_TEMP ID1
			INNER JOIN TB_ITEM_DUPLICATES_TEMP ID2 ON ID2._key_out = ID1._key_in --self join to match item_IDs
			LEFT OUTER JOIN TB_ITEM_DUPLICATES t1 ON ID1.ITEM_ID = t1.ITEM_ID_IN AND ID2.ITEM_ID = t1.ITEM_ID_OUT
			WHERE ID1.REVIEW_ID = @revID
			AND ID1.ITEM_ID <> ID2.ITEM_ID --reject lines that mark an item as its own duplicate
			AND ID1.EXTR_UI != '00000000-0000-0000-0000-000000000000' --make sure you fish data only from the real results
			AND T1.ITEM_ID_IN IS NULL --see long comment above
		--now that temporary data is saved, clean up:
		DELETE FROM TB_ITEM_DUPLICATES_TEMP WHERE REVIEW_ID = @revID
	END
	
	-- if @guis_N >2 then something is very wrong, but we leave the other SPs to deal with this situation
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicatesCheckOngoing] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesGetItemsList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ItemDuplicatesGetItemsList]
(
	@REVIEW_ID_ST VARCHAR(MAX)
)

As

SELECT I.ITEM_ID,
		CAST (TITLE AS NVARCHAR (1999)) AS TITLE,
		CAST (PARENT_TITLE AS NVARCHAR(1999)) AS PARENT_TITLE,
		CAST ([dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) AS NVARCHAR(1999)) as AUTHORS, 
		CAST ([dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) AS NVARCHAR(1999)) as PARENTAUTHORS,
		REVIEW_ID
FROM TB_ITEM I
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID
	AND TB_ITEM_REVIEW.REVIEW_ID = CAST(@REVIEW_ID_ST AS INT)
	AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicatesGetItemsList] TO  SCHEMA OWNER 
GO


GRANT EXECUTE ON [dbo].[st_ItemDuplicatesGetItemsList] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesGetItemsListNew]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--data source
CREATE procedure [dbo].[st_ItemDuplicatesGetItemsListNew]
(
	@REVIEW_ID_ST VARCHAR(MAX)
)

As
SELECT 
		ITEM_ID,
		TITLE,
		PARENT_TITLE,
		AUTHORS, 
		PARENTAUTHORS,
		REVIEW_ID
FROM 
(
SELECT I.ITEM_ID,
		CAST (TITLE AS NVARCHAR (1999)) AS TITLE,
		CAST (PARENT_TITLE AS NVARCHAR(1999)) AS PARENT_TITLE,
		CAST ([dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) AS NVARCHAR(1999)) as AUTHORS, 
		CAST ([dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) AS NVARCHAR(1999)) as PARENTAUTHORS,
		IR.REVIEW_ID, 
		0 sorter
FROM TB_ITEM I
INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = I.ITEM_ID
	AND IR.REVIEW_ID = CAST(@REVIEW_ID_ST AS INT)
	AND IR.IS_DELETED = 'TRUE'
INNER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS ID ON ID.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
UNION
SELECT I.ITEM_ID,
		CAST (TITLE AS NVARCHAR (1999)) AS TITLE,
		CAST (PARENT_TITLE AS NVARCHAR(1999)) AS PARENT_TITLE,
		CAST ([dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) AS NVARCHAR(1999)) as AUTHORS, 
		CAST ([dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) AS NVARCHAR(1999)) as PARENTAUTHORS,
		REVIEW_ID,
		1 sorter
FROM TB_ITEM I
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID
	AND TB_ITEM_REVIEW.REVIEW_ID = CAST(@REVIEW_ID_ST AS INT)
	AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
) a
order by sorter


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicatesGetItemsListNew] TO  SCHEMA OWNER 
GO


GRANT EXECUTE ON [dbo].[st_ItemDuplicatesGetItemsListNew] TO [SSRU38\sqlcmdshell] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE procedure [dbo].[st_ItemDuplicatesInsert]
(
	@REVIEW_ID INT
)

As
-- First check: if there are no items to evaluate, just go back
declare @check int = (SELECT COUNT(ITEM_ID) from TB_ITEM_REVIEW where REVIEW_ID = @REVIEW_ID and IS_DELETED = 0)
if @check = 0 
BEGIN
	Return -1
END
SET  @check =(SELECT COUNT(DISTINCT(EXTR_UI)) from TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @REVIEW_ID)
IF @check = 1 
BEGIN
	return 1 --a SISS package is still running for Review, we should not run it again
END
ELSE IF @check > 1 --this should not happen: SISS package saved some data, but the result was not collected. 
-- Since new items might have been inserted in the mean time, we will delete old (temp) results and start over again.
BEGIN
	DELETE FROM TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @REVIEW_ID 
END

declare @UI uniqueidentifier
set @UI = '00000000-0000-0000-0000-000000000000'
--insert a marker line to notify that the SISS package has been triggered
insert into TB_ITEM_DUPLICATES_TEMP (REVIEW_ID, EXTR_UI) Values (@REVIEW_ID, @UI)

set @UI = NEWID()

--run the package that populates the temp results table
declare @cmd varchar(1000)
select @cmd = 'dtexec /DT "File System\DuplicateCheck"'
select @cmd = @cmd + ' /Rep N  /SET \Package.Variables[User::RevID].Properties[Value];"' + CAST(@REVIEW_ID as varchar(max))+ '"' 
select @cmd = @cmd + ' /SET \Package.Variables[User::UID].Properties[Value];"' + CAST(@UI as varchar(max))+ '"' 
EXEC xp_cmdshell @cmd

--INSERT only the new data in the final table
--The select statement below is an attempt to avoid subqueries: it is necessary to avoid inserting duplicates of duplicates
--the left outer join with the target table (t1) has the ON clause that is true only if the destination line already exists,
--being an outer join, if this condition is false the fields from the joined table will be NULL
--All I need to do is force these values to be NULL, and I can do it safely on t1.ITEM_ID_IN only 
--because NULL values are not allowed on that field (no need to check from both, then)
--RESULT: if the destination record already exists I will have some value in t1.ITEM_ID_IN and the line will not be selected
--if the destination record does not exist t1.ITEM_ID_IN will be NULL and I will select (and insert) the new line
--NOTE: I'm not sure this will be faster than having a subquery, but I think it is...
--We should check when we'll have big duplicates tables...
INSERT INTO [TB_ITEM_DUPLICATES]
           ([ITEM_ID_IN]
           ,[_SCORE]
           ,[ITEM_ID_OUT]
           ,[REVIEW_ID]
           )
SELECT ID1.ITEM_ID ITEM_ID_IN, ID2._SCORE, ID2.ITEM_ID ITEM_ID_OUT, @REVIEW_ID 
	FROM TB_ITEM_DUPLICATES_TEMP ID1
	INNER JOIN TB_ITEM_DUPLICATES_TEMP ID2 ON ID2._key_out = ID1._key_in --self join to match item_IDs
	LEFT OUTER JOIN TB_ITEM_DUPLICATES t1 ON ID1.ITEM_ID = t1.ITEM_ID_IN AND ID2.ITEM_ID = t1.ITEM_ID_OUT
	WHERE ID1.REVIEW_ID = @REVIEW_ID --to be safe...
	AND ID1.ITEM_ID <> ID2.ITEM_ID --reject lines that mark an item as its own duplicate
	AND ID1.EXTR_UI = @UI --make sure you fish data only from the current check
	AND T1.ITEM_ID_IN IS NULL --see long comment above

--clear temp table
-- if the query has timed out in the mean time, either one or two different EXTR_UI will be present in the table, this will instruct other SP
-- to react differently according to the situation
DELETE from TB_ITEM_DUPLICATES_TEMP WHERE REVIEW_ID = @REVIEW_ID 



GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicatesInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE procedure [dbo].[st_ItemDuplicatesList]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON
DECLARE @True bit, @False bit;
SELECT @True = 1, @False = 0;

SELECT DISTINCT I.ITEM_ID ITEM_ID1,
	I.[TYPE_ID] TYPE_ID1,
	 I.OLD_ITEM_ID OLD_ITEM_ID1, 
	 [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS1,
	I.TITLE TITLE1, 
	I.PARENT_TITLE PARENT_TITLE1, 
	I.SHORT_TITLE SHORT_TITLE1, 
	I.DATE_CREATED DATE_CREATED1, 
	I.CREATED_BY CREATED_BY1, 
	I.DATE_EDITED DATE_EDITED1, 
	I.EDITED_BY EDITED_BY1,
	I.[YEAR] YEAR1, 
	I.[MONTH] MONTH1, 
	I.STANDARD_NUMBER STANDARD_NUMBER1, 
	I.CITY CITY1,
	I.COUNTRY COUNTRY1, 
	I.PUBLISHER PUBLISHER1, 
	I.INSTITUTION INSTITUTION1, 
	I.VOLUME VOLUME1, 
	I.PAGES PAGES1,
	I.EDITION EDITION1, 
	I.ISSUE ISSUE1, 
	I.URL URL1, 
	I.ABSTRACT ABSTRACT1, 
	I.COMMENTS COMMENTS1, 
	IT1.[TYPE_NAME] TYPE_NAME1,
	[dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENT_AUTHORS1, 
	ID1.[IS_CHECKED] IS_CHECKED1, 
	case 
		when IR1.MASTER_ITEM_ID = ID1.ITEM_ID_OUT
		THEN @True
		ELSE @False
	end
	IS_DUPLICATE1, 
	i.OLD_ITEM_ID OLD_ITEM_ID1,
	(SELECT COUNT (SET_ID) FROM TB_ITEM_SET WHERE TB_ITEM_SET.ITEM_ID = I.ITEM_ID) CODED_COUNT1,
	(SELECT COUNT (ITEM_DOCUMENT_ID) FROM TB_ITEM_DOCUMENT d WHERE d.ITEM_ID = I.ITEM_ID) DOC_COUNT1,
	(SELECT SOURCE_NAME from TB_SOURCE s inner join TB_ITEM_SOURCE tis on s.SOURCE_ID = tis.SOURCE_ID and tis.ITEM_ID = I.ITEM_ID)
	 SOURCE1,
	
	I2.ITEM_ID ITEM_ID2, 
	I2.[TYPE_ID] TYPE_ID2, 
	I2.OLD_ITEM_ID OLD_ITEM_ID2, 
	[dbo].fn_REBUILD_AUTHORS(I2.ITEM_ID, 0) as AUTHORS2,
	I2.TITLE TITLE2, 
	I2.PARENT_TITLE PARENT_TITLE2, 
	I2.SHORT_TITLE SHORT_TITLE2, 
	I2.DATE_CREATED DATE_CREATED2, 
	I2.CREATED_BY CREATED_BY2, 
	I2.DATE_EDITED DATE_EDITED2, 
	I2.EDITED_BY EDITED_BY2,
	I2.[YEAR] YEAR2, 
	I2.[MONTH] MONTH2, 
	I2.STANDARD_NUMBER STANDARD_NUMBER2, 
	I2.CITY CITY2, 
	I2.COUNTRY COUNTRY2, 
	I2.PUBLISHER PUBLISHER2, 
	I2.INSTITUTION INSTITUTION2, 
	I2.VOLUME VOLUME2, 
	I2.PAGES PAGES2,
	I2.EDITION EDITION2, 
	I2.ISSUE ISSUE2, 
	I2.URL URL2, 
	I2.ABSTRACT ABSTRACT2, 
	I2.COMMENTS COMMENTS2, 
	IT2.[TYPE_NAME] TYPE_NAME2,
	[dbo].fn_REBUILD_AUTHORS(I2.ITEM_ID, 1) as PARENT_AUTHORS2, 
	i2.OLD_ITEM_ID OLD_ITEM_ID2,
	
	
	--ID2.[IS_CHECKED] IS_CHECKED2, 
	case 
		when IR2.MASTER_ITEM_ID = ID1.ITEM_ID_IN
		THEN @True
		ELSE @False
	end IS_DUPLICATE2, 
	(SELECT COUNT (SET_ID) FROM TB_ITEM_SET WHERE TB_ITEM_SET.ITEM_ID = I2.ITEM_ID) CODED_COUNT2,
	(SELECT COUNT (ITEM_DOCUMENT_ID) FROM TB_ITEM_DOCUMENT d WHERE d.ITEM_ID = I2.ITEM_ID) DOC_COUNT2,
	(SELECT SOURCE_NAME from TB_SOURCE s inner join TB_ITEM_SOURCE tis on s.SOURCE_ID = tis.SOURCE_ID and tis.ITEM_ID = I2.ITEM_ID)
	 SOURCE2,
	id1.ITEM_DUPLICATES_ID ITEM_DUPLICATES_ID1, 
	ID1._SCORE SCORE1--,
	--ID2.ITEM_DUPLICATES_ID ITEM_DUPLICATES_ID2, 
	--ID2._SCORE SCORE2

FROM TB_ITEM_DUPLICATES ID1

--INNER JOIN TB_ITEM_DUPLICATES ID2 ON ID2._key_out = ID1._key_in
INNER JOIN TB_ITEM_REVIEW IR1 on ID1.ITEM_ID_IN = IR1.ITEM_ID AND IR1.REVIEW_ID = ID1.REVIEW_ID
INNER JOIN TB_ITEM_REVIEW IR2 on ID1.ITEM_ID_OUT = IR2.ITEM_ID AND IR2.REVIEW_ID = ID1.REVIEW_ID

INNER JOIN TB_ITEM I ON I.ITEM_ID = ID1.ITEM_ID_IN
INNER JOIN TB_ITEM I2 ON I2.ITEM_ID = ID1.ITEM_ID_OUT

INNER JOIN TB_ITEM_TYPE IT1 ON IT1.[TYPE_ID] = I.[TYPE_ID]
INNER JOIN TB_ITEM_TYPE IT2 ON IT2.[TYPE_ID] = I2.[TYPE_ID]

WHERE ID1.REVIEW_ID = @REVIEW_ID --AND ID2.REVIEW_ID = @REVIEW_ID AND ID1.ITEM_ID <> ID2.ITEM_ID

ORDER BY ITEM_ID1, I.TITLE, ITEM_ID2

SET NOCOUNT OFF



GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicatesList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesReadOnlyList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	gets the list of items that are currently marked as duplicates of the ID
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicatesReadOnlyList]
	-- Add the parameters for the stored procedure here
	@ItemID bigint = 0,
	@revID int = 0
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT SHORT_TITLE, I.ITEM_ID, SOURCE_NAME--, G.ITEM_DUPLICATE_GROUP_ID GROUP_ID 
	from TB_ITEM_REVIEW IR
	inner join TB_ITEM I on I.ITEM_ID = IR.ITEM_ID 
	inner join TB_ITEM_REVIEW IR2 on IR2.ITEM_ID = @ItemID and @revID = IR2.REVIEW_ID
	left outer JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on GM.ITEM_REVIEW_ID = IR2.ITEM_REVIEW_ID and GM.IS_CHECKED = 1 and GM.IS_DUPLICATE = 0
	left outer JOIN TB_ITEM_DUPLICATE_GROUP G on GM.ITEM_DUPLICATE_GROUP_ID = G.ITEM_DUPLICATE_GROUP_ID
	left outer JOIN TB_ITEM_SOURCE ITS on ITS.ITEM_ID = I.ITEM_ID
	left outer join TB_SOURCE S on ITS.SOURCE_ID = S.SOURCE_ID
	where IR.REVIEW_ID = @revID and IR.MASTER_ITEM_ID = @ItemID
	group by SHORT_TITLE, I.ITEM_ID, SOURCE_NAME--, G.ITEM_DUPLICATE_GROUP_ID
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicatesReadOnlyList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ItemDuplicatesUpdate]
(
	@REVIEW_ID INT,
	@ITEM_ID1 BIGINT,
	@ITEM_ID2 BIGINT,
	@IS_CHECKED BIT,
	@IS_DUPLICATE1 BIT,
	@IS_DUPLICATE2 BIT,
	@ITEM_DUPLICATES_ID1 BIGINT,
	@ITEM_DUPLICATES_ID2 BIGINT
)

As

	UPDATE TB_ITEM_DUPLICATES
	SET IS_CHECKED = @IS_CHECKED, IS_DUPLICATE = @IS_DUPLICATE1
	WHERE ITEM_DUPLICATES_ID = @ITEM_DUPLICATES_ID1
	
	--UPDATE TB_ITEM_DUPLICATES
	--SET IS_CHECKED = @IS_CHECKED, IS_DUPLICATE = @IS_DUPLICATE2
	--WHERE ITEM_DUPLICATES_ID = @ITEM_DUPLICATES_ID2
	
	IF (@IS_DUPLICATE1 = 'TRUE')
	BEGIN
		UPDATE TB_ITEM_REVIEW
		SET MASTER_ITEM_ID = @ITEM_ID2, IS_DELETED = 'TRUE' -- IE DELETED WHEN IS A DUPLICATE
		WHERE ITEM_ID = @ITEM_ID1 AND REVIEW_ID = @REVIEW_ID
	END
	ELSE
	BEGIN
		UPDATE TB_ITEM_REVIEW
		SET MASTER_ITEM_ID = NULL, IS_DELETED = 'FALSE' -- IE DELETED WHEN IS A DUPLICATE
		WHERE ITEM_ID = @ITEM_ID1 AND REVIEW_ID = @REVIEW_ID
	END
	
	IF (@IS_DUPLICATE2 = 'TRUE')
	BEGIN
		UPDATE TB_ITEM_REVIEW
		SET MASTER_ITEM_ID = @ITEM_ID1, IS_DELETED = 'TRUE' -- IE DELETED WHEN IS A DUPLICATE
		WHERE ITEM_ID = @ITEM_ID2 AND REVIEW_ID = @REVIEW_ID
	END
	ELSE
	BEGIN
		UPDATE TB_ITEM_REVIEW
		SET MASTER_ITEM_ID = NULL, IS_DELETED = 'FALSE' -- IE DELETED WHEN IS A DUPLICATE
		WHERE ITEM_ID = @ITEM_ID2 AND REVIEW_ID = @REVIEW_ID
	END

--USE [Reviewer]
--GO
--/****** Object:  UserDefinedFunction [dbo].[fn_REBUILD_AUTHORS]    Script Date: 05/26/2010 10:23:10 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
---- De-normalising Authors function ByS --

--ALTER FUNCTION  [dbo].[fn_REBUILD_AUTHORS]

--(

--@id bigint,
--@role tinyint = 0

--)

--RETURNS nvarchar(max)

   

--    BEGIN

--        declare @res nvarchar(max)

--        declare @res2 nvarchar(max)

--        DECLARE cr CURSOR FOR SELECT [LAST] + ' ' + [FIRST] + ' ' + [SECOND]

--        FROM [tb_ITEM_AUTHOR]  where item_id = @id AND ROLE = @role

--        ORDER BY [RANK]

--        open cr

--        set @res = ''

--        FETCH NEXT FROM cr INTO @res2

--         WHILE @@FETCH_STATUS = 0

--        BEGIN

--                Set @res = @res  + @res2

--                FETCH NEXT FROM cr INTO @res2

--                set @res = @res + '; '

--                END

--        return @res

--    END;

---- END OF: De-normalising function ByS --

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicatesUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateUpdateTbItemReview]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sergio
-- Create date: 20/08/2010
-- Description:	Update a group member, this will also change the group master if needed.
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateUpdateTbItemReview]
	-- Add the parameters for the stored procedure here
	@groupID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	update IR 
		set IR.MASTER_ITEM_ID = CASE
			
			WHEN GM.IS_CHECKED = 1 and GM.IS_DUPLICATE = 1 then
				IR1.ITEM_ID
			ELSE Null
			END
		, 
		IR.IS_DELETED = CASE
			WHEN GM1.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID --item is master of current group, leave as the source it belongs.
			--need left joins for manually created items (don't have a source!)
				then (select CASE when (s.is_deleted = 'True' )  Then 'True' else 'False' end from 
							TB_ITEM_REVIEW iir
							LEFT join TB_ITEM_SOURCE tis on iir.ITEM_ID = tis.ITEM_ID
							LEFT join TB_SOURCE s on tis.SOURCE_ID = s.SOURCE_ID
							where IR.ITEM_REVIEW_ID = iir.ITEM_REVIEW_ID
							--TB_SOURCE s inner join TB_ITEM_SOURCE tis 
							--on s.SOURCE_ID = tis.SOURCE_ID and tis.ITEM_ID = IR.ITEM_ID
							) --
			WHEN GM.IS_CHECKED = 1 and GM.IS_DUPLICATE = 1 then
				'True'
			ELSE 'False'
			END
		, IR.IS_INCLUDED = CASE
			WHEN GM.IS_DUPLICATE = 1 and GM.IS_CHECKED = 1 then --set is_included to true, to make the item 'shadow'
				'true'
			ELSE IR.IS_INCLUDED --leave untouched
			END
	from TB_ITEM_REVIEW IR inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM 
		on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and GM.ITEM_DUPLICATE_GROUP_ID = @groupID
		Inner Join TB_ITEM_DUPLICATE_GROUP DG on DG.ITEM_DUPLICATE_GROUP_ID = gm.ITEM_DUPLICATE_GROUP_ID
		Inner Join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM1 on GM1.GROUP_MEMBER_ID = DG.MASTER_MEMBER_ID
		INNER Join TB_ITEM_REVIEW IR1 on GM1.ITEM_REVIEW_ID = IR1.ITEM_REVIEW_ID
		Left outer join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM2 on IR.ITEM_REVIEW_ID = GM2.ITEM_REVIEW_ID 
			and GM2.ITEM_DUPLICATE_GROUP_ID != DG.ITEM_DUPLICATE_GROUP_ID
			and GM2.IS_CHECKED = 1 and GM2.IS_DUPLICATE = 1
		where GM2.GROUP_MEMBER_ID is null
END

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateUpdateTbItemReview] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateWipeData]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	deletes data from group tables and optionally from tb_item_review
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemDuplicateWipeData]
	-- Add the parameters for the stored procedure here
	@ReviewID int,
	@wipeAll bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	IF @wipeAll = 1
	Update TB_ITEM_REVIEW set IS_DELETED = 0, MASTER_ITEM_ID = null
		where REVIEW_ID = @ReviewID and MASTER_ITEM_ID is not null

	Delete from TB_ITEM_DUPLICATE_GROUP where REVIEW_ID = @ReviewID
	--this deletes all as there is cascade relationship with TB_ITEM_DUPLICATE_GROUP_MEMBERS
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemDuplicateWipeData] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemImportPrepare]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:              Sergio
-- Create date: 23-06-09
-- Description: Prepare Tables for Bulk Item import
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemImportPrepare]
        @Items_Number int,
        @Authors_Number int,
        @Item_Seed bigint OUTPUT,
        @Author_Seed bigint OUTPUT,
        @Source_Seed int OUTPUT,
        @Item_Source_Seed bigint OUTPUT,
        @Item_Review_Seed bigint OUTPUT
AS
BEGIN
SET NOCOUNT ON;
-- This procedure Reservs some Identinty values that will be inserted
-- from C# via a Dataset bulkcopy
-- Note the Table Lock Hints used to prevent insertions to happen while dealing with a particular table
Declare @temp bigint
BEGIN TRAN A
        set @Item_Seed = (SELECT top 1 IDENT_CURRENT('TB_ITEM') FROM TB_ITEM WITH (HOLDLOCK, TABLOCKX))
        set @temp = @Item_Seed + @Items_Number
        DBCC CHECKIDENT('TB_ITEM', RESEED, @temp)
COMMIT TRAN A

BEGIN TRAN B
        set @Author_Seed = (SELECT top 1 IDENT_CURRENT('tb_ITEM_AUTHOR') FROM tb_ITEM_AUTHOR WITH (HOLDLOCK, TABLOCKX))
        set @temp = @Author_Seed + @Authors_Number
        DBCC CHECKIDENT('tb_ITEM_AUTHOR', RESEED, @temp)
COMMIT TRAN B

BEGIN TRAN C
        set @Source_Seed = (SELECT top 1 IDENT_CURRENT('TB_SOURCE') FROM TB_SOURCE WITH (HOLDLOCK, TABLOCKX))
        set @temp = @Source_Seed + 1
        DBCC CHECKIDENT('TB_SOURCE', RESEED, @temp)
COMMIT TRAN C

BEGIN TRAN D
        set @Item_Source_Seed = (SELECT top 1 IDENT_CURRENT('TB_ITEM_SOURCE') FROM TB_ITEM_SOURCE WITH (HOLDLOCK, TABLOCKX))
        set @temp = @Item_Source_Seed + @Items_Number
        DBCC CHECKIDENT('TB_ITEM_SOURCE', RESEED, @temp)
COMMIT TRAN D

BEGIN TRAN E
        set @Item_Review_Seed = (SELECT top 1 IDENT_CURRENT('TB_ITEM_REVIEW') FROM TB_ITEM_REVIEW WITH (HOLDLOCK, TABLOCKX))
        set @temp = @Item_Review_Seed + @Items_Number
        DBCC CHECKIDENT('TB_ITEM_REVIEW', RESEED, @temp)
COMMIT TRAN E
END

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemImportPrepare] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemImportPrepareBatch]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:              Sergio
-- Create date: 23-06-09
-- Description: Prepare Tables for Bulk Item import
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemImportPrepareBatch]
        @Items_Number int,
        @Authors_Number int,
        @Source_Id int = 0,
        @Item_Seed bigint OUTPUT,
        @Author_Seed bigint OUTPUT,
        @Source_Seed int OUTPUT,
        @Item_Source_Seed bigint OUTPUT

AS
BEGIN
SET NOCOUNT ON;
-- This procedure Reservs some Identinty values that will be inserted
-- from C# via a Dataset bulkcopy
-- Note the Table Lock Hints used to prevent insertions to happen while dealing with a particular table
Declare @temp bigint
BEGIN TRAN A
        set @Item_Seed = (SELECT top 1 IDENT_CURRENT('TB_ITEM') FROM TB_ITEM WITH (HOLDLOCK, TABLOCKX))
        set @temp = @Item_Seed + @Items_Number
        DBCC CHECKIDENT('TB_ITEM', RESEED, @temp)
COMMIT TRAN A

BEGIN TRAN B
        set @Author_Seed = (SELECT top 1 IDENT_CURRENT('tb_ITEM_AUTHOR') FROM tb_ITEM_AUTHOR WITH (HOLDLOCK, TABLOCKX))
        set @temp = @Author_Seed + @Authors_Number
        DBCC CHECKIDENT('tb_ITEM_AUTHOR', RESEED, @temp)
COMMIT TRAN B

BEGIN TRAN C
	if (@Source_Id > 0)--no need to seed the source
	begin
			set @Source_Seed = -1 --make sure there is some value, just in case SQL gets upset
	end
	else 
	BEGIN
			set @Source_Seed = (SELECT top 1 IDENT_CURRENT('TB_SOURCE') FROM TB_SOURCE WITH (HOLDLOCK, TABLOCKX))
			set @temp = @Source_Seed + 1
			DBCC CHECKIDENT('TB_SOURCE', RESEED, @temp)
	END
COMMIT TRAN C


BEGIN TRAN D
        set @Item_Source_Seed = (SELECT top 1 IDENT_CURRENT('TB_ITEM_SOURCE') FROM TB_ITEM_SOURCE WITH (HOLDLOCK, TABLOCKX))
        set @temp = @Item_Source_Seed + @Items_Number
        DBCC CHECKIDENT('TB_ITEM_SOURCE', RESEED, @temp)
COMMIT TRAN D


END




GO
ALTER AUTHORIZATION ON [dbo].[st_ItemImportPrepareBatch] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemIncludeExclude]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemIncludeExclude]
(
	@INCLUDE BIT,
	@ITEM_ID_LIST varchar(max),
	@REVIEW_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@SET_ID INT
)

As

SET NOCOUNT ON

IF(@ITEM_ID_LIST = '')
BEGIN
	UPDATE TB_ITEM_REVIEW
	set IS_INCLUDED = @INCLUDE, IS_DELETED = 'FALSE'
	WHERE REVIEW_ID = @REVIEW_ID 
		 AND ITEM_ID IN
		(SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'AND TB_ITEM_SET.SET_ID = @SET_ID
			AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID
		)
		AND not( IS_DELETED = 1 and IS_INCLUDED = 1)--this leaves shadow items alone
END
ELSE
BEGIN
	UPDATE TB_ITEM_REVIEW
	set IS_INCLUDED = @INCLUDE, IS_DELETED = 'FALSE'
	WHERE REVIEW_ID = @REVIEW_ID 
		 AND ITEM_ID IN
		(SELECT VALUE FROM dbo.fn_Split_int(@ITEM_ID_LIST, ','))
		AND not( IS_DELETED = 1 and IS_INCLUDED = 1)--this leaves shadow items alone
END

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemIncludeExclude] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemLinkDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ItemLinkDelete]
(
	@ITEM_LINK_ID INT
)

As
	DELETE FROM TB_ITEM_LINK
	WHERE ITEM_LINK_ID = @ITEM_LINK_ID

SET NOCOUNT OFF



GO
ALTER AUTHORIZATION ON [dbo].[st_ItemLinkDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemLinkInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ItemLinkInsert]
(
	@ITEM_ID_PRIMARY BIGINT,
	@ITEM_ID_SECONDARY BIGINT,
	@LINK_DESCRIPTION NVARCHAR(255),
	@NEW_ITEM_LINK_ID INT OUTPUT
)

As

	INSERT INTO TB_ITEM_LINK(ITEM_ID_PRIMARY, ITEM_ID_SECONDARY, LINK_DESCRIPTION)
	VALUES (@ITEM_ID_PRIMARY, @ITEM_ID_SECONDARY, @LINK_DESCRIPTION)
	
	SET @NEW_ITEM_LINK_ID = @@IDENTITY

SET NOCOUNT OFF



GO
ALTER AUTHORIZATION ON [dbo].[st_ItemLinkInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemLinkList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ItemLinkList]
(
	@REVIEW_ID INT,
	@ITEM_ID BIGINT
)

As

	SELECT ITEM_LINK_ID, ITEM_ID_PRIMARY, ITEM_ID_SECONDARY, LINK_DESCRIPTION, SHORT_TITLE, TITLE
	FROM TB_ITEM_LINK
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_LINK.ITEM_ID_SECONDARY
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_LINK.ITEM_ID_PRIMARY
	WHERE ITEM_ID_PRIMARY = @ITEM_ID AND REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF



GO
ALTER AUTHORIZATION ON [dbo].[st_ItemLinkList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemLinkUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemLinkUpdate]
(
	@ITEM_LINK_ID INT,
	@ITEM_LINK_SECONDARY BIGINT,
	@LINK_DESCRIPTION NVARCHAR(255)
)

As
	UPDATE TB_ITEM_LINK
	SET LINK_DESCRIPTION = @LINK_DESCRIPTION,
	ITEM_ID_SECONDARY = @ITEM_LINK_SECONDARY
	WHERE ITEM_LINK_ID = @ITEM_LINK_ID

SET NOCOUNT OFF




GO
ALTER AUTHORIZATION ON [dbo].[st_ItemLinkUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemList]
(
      @REVIEW_ID INT,
      @SHOW_INCLUDED BIT = 'true',
      @SHOW_DELETED BIT = 'false',
      @SOURCE_ID INT = 0,
      @ATTRIBUTE_SET_ID_LIST NVARCHAR(MAX) = '',
      
      @PageNum INT = 1,
      @PerPage INT = 3,
      @CurrentPage INT OUTPUT,
      @TotalPages INT OUTPUT,
      @TotalRows INT OUTPUT 
)
with recompile
As

SET NOCOUNT ON

declare @RowsToRetrieve int
Declare @ID table (ItemID bigint primary key )
IF (@SOURCE_ID = 0) AND (@ATTRIBUTE_SET_ID_LIST = '') /* LIST ALL ITEMS IN THE REVIEW */
BEGIN

       --store IDs to build paged results as a simple join
	  INSERT INTO @ID SELECT DISTINCT (I.ITEM_ID)
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
            AND TB_ITEM_REVIEW.IS_DELETED = @SHOW_DELETED

	  SELECT @TotalRows = @@ROWCOUNT

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (ir.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM i
			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID
			INNER JOIN @ID id on id.ItemID = ir.ITEM_ID
            
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve
            ORDER BY SHORT_TITLE
END
ELSE /* FILTER BY A LIST OF ATTRIBUTES */

IF (@ATTRIBUTE_SET_ID_LIST != '')
BEGIN
      SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
            FROM TB_ITEM_REVIEW I
      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
      INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
      INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!

      WHERE I.IS_INCLUDED = @SHOW_INCLUDED
            AND I.IS_DELETED = @SHOW_DELETED
            AND I.REVIEW_ID = @REVIEW_ID

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID, 
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM I
       INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
            AND TB_ITEM_REVIEW.IS_DELETED = @SHOW_DELETED

      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve 
            ORDER BY SHORT_TITLE
END
ELSE -- LISTING SOURCELESS
IF (@SOURCE_ID = -1)
BEGIN
       --store IDs to build paged results as a simple join
	  INSERT INTO @ID SELECT DISTINCT IR.ITEM_ID
		from TB_ITEM_REVIEW IR 
      LEFT OUTER JOIN TB_ITEM_SOURCE TIS on IR.ITEM_ID = TIS.ITEM_ID
      LEFT OUTER JOIN TB_SOURCE TS on TIS.SOURCE_ID = TS.SOURCE_ID and IR.REVIEW_ID = TS.REVIEW_ID
      where IR.REVIEW_ID = @REVIEW_ID and TS.SOURCE_ID  is null

	  SELECT @TotalRows = @@ROWCOUNT
      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IR.IS_DELETED, IS_INCLUDED, IR.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] 
      INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = I.ITEM_ID AND IR.REVIEW_ID = @REVIEW_ID
      INNER JOIN @ID id on id.ItemID = I.ITEM_ID
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve 
            ORDER BY SHORT_TITLE
      
END
ELSE -- LISTING BY A SOURCE
BEGIN
      SELECT @TotalRows = count(I.ITEM_ID)
      FROM TB_ITEM_REVIEW I
      INNER JOIN TB_ITEM_SOURCE ON TB_ITEM_SOURCE.ITEM_ID = I.ITEM_ID AND TB_ITEM_SOURCE.SOURCE_ID = @SOURCE_ID
      WHERE I.REVIEW_ID = @REVIEW_ID

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
      INNER JOIN TB_ITEM_SOURCE ON TB_ITEM_SOURCE.ITEM_ID = I.ITEM_ID AND TB_ITEM_SOURCE.SOURCE_ID = @SOURCE_ID
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve
            ORDER BY SHORT_TITLE
      
END

SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'


SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListFrequencyNoneOfTheAbove]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemListFrequencyNoneOfTheAbove]
(
	@ATTRIBUTE_ID BIGINT = null,
	@SET_ID BIGINT,
	@IS_INCLUDED BIT,
	@FILTER_ATTRIBUTE_ID BIGINT,
	@REVIEW_ID INT,

	@PageNum INT = 1,
	@PerPage INT = 3,
	@CurrentPage INT OUTPUT,
	@TotalPages INT OUTPUT,
	@TotalRows INT OUTPUT 
)

As

SET NOCOUNT ON
	declare @RowsToRetrieve int
	Declare @ID table (ItemID bigint ) --store IDs to build paged results as a simple join
IF (@FILTER_ATTRIBUTE_ID = -1)
BEGIN
	insert into @ID
	Select DISTINCT ITEM_ID
			from TB_ITEM_REVIEW 
			where ITEM_ID not in 
					(
						select TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
						  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
								AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
						  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
								AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
						  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
								AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
					)
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
END
ELSE
BEGIN
	insert into @ID
	Select TB_ITEM_REVIEW.ITEM_ID
			from TB_ITEM_REVIEW 
			INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND IA2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
			where TB_ITEM_REVIEW.ITEM_ID not in 
					(
						select TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
						  INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
								AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_ITEM_SET.SET_ID = @SET_ID
						  RIGHT OUTER JOIN TB_ATTRIBUTE_SET ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
								AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
						  INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
								AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
						  INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND IA2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
						  INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
					)
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
								AND TB_ITEM_REVIEW.IS_INCLUDED = @IS_INCLUDED
								AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
END
	--count results
	SELECT @TotalRows = count(ItemID) from @ID
	set @TotalPages = @TotalRows/@PerPage

	if @PageNum < 1
	set @PageNum = 1

	if @TotalRows % @PerPage != 0
	set @TotalPages = @TotalPages + 1

	set @RowsToRetrieve = @PerPage * @PageNum
	set @CurrentPage = @PageNum;

	WITH SearchResults AS
	(
		SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID
			, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
		FROM TB_ITEM I
		INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] 
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
			TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN @ID on I.ITEM_ID = ItemID
	)
	Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
			TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
			[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
			AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
			, SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
		FROM SearchResults 
				  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
		WHERE RowNum > @RowsToRetrieve - @PerPage
		AND RowNum <= @RowsToRetrieve 
		ORDER BY SHORT_TITLE


SELECT	@CurrentPage as N'@CurrentPage',
		@TotalPages as N'@TotalPages',
		@TotalRows as N'@TotalRows'
SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemListFrequencyNoneOfTheAbove] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListFrequencyWithFilter]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemListFrequencyWithFilter]
(
	@ATTRIBUTE_ID BIGINT = null,
	@SET_ID BIGINT,
	@IS_INCLUDED BIT,
	@FILTER_ATTRIBUTE_ID BIGINT,
	@REVIEW_ID INT,

	@PageNum INT = 1,
	@PerPage INT = 3,
	@CurrentPage INT OUTPUT,
	@TotalPages INT OUTPUT,
	@TotalRows INT OUTPUT 
)

As

SET NOCOUNT ON
	declare @RowsToRetrieve int
	Declare @ID table (ItemID bigint ) --store IDs to build paged results as a simple join
	insert into @ID
	Select DISTINCT ir.ITEM_ID
			from TB_ITEM_REVIEW ir
			inner join TB_ITEM_ATTRIBUTE ia on ir.REVIEW_ID = @REVIEW_ID
								AND ir.IS_INCLUDED = @IS_INCLUDED
								AND ir.IS_DELETED = 'FALSE'
								and ia.ITEM_ID = ir.ITEM_ID
								AND ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
			inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID
								AND tis.IS_COMPLETED = 1
								and tis.SET_ID = @SET_ID
			inner join TB_ITEM_ATTRIBUTE ia2  on  ia2.ITEM_ID = ir.ITEM_ID
								and ia2.ATTRIBUTE_ID = @FILTER_ATTRIBUTE_ID
			inner join TB_ITEM_SET tis2 on tis2.ITEM_SET_ID = ia2.ITEM_SET_ID
								AND tis2.IS_COMPLETED = 1
												
	--count results
	SELECT @TotalRows = count(ItemID) from @ID
	set @TotalPages = @TotalRows/@PerPage

	if @PageNum < 1
	set @PageNum = 1

	if @TotalRows % @PerPage != 0
	set @TotalPages = @TotalPages + 1

	set @RowsToRetrieve = @PerPage * @PageNum
	set @CurrentPage = @PageNum;

	WITH SearchResults AS
	(
		SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID
			, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
		FROM TB_ITEM I
		INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] 
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
			TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN @ID on I.ITEM_ID = ItemID
	)
	Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
			TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
			[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
			AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
			, SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
		FROM SearchResults 
				  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
		WHERE RowNum > @RowsToRetrieve - @PerPage
		AND RowNum <= @RowsToRetrieve 
		ORDER BY SHORT_TITLE


SELECT	@CurrentPage as N'@CurrentPage',
		@TotalPages as N'@TotalPages',
		@TotalRows as N'@TotalRows'
SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemListFrequencyWithFilter] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListWithoutAttributes]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemListWithoutAttributes]
(
	@REVIEW_ID INT,
	@SHOW_INCLUDED BIT = 'true',
	@SHOW_DELETED BIT = 'false',
	@ATTRIBUTE_SET_ID_LIST NVARCHAR(MAX) = '',
	
	@PageNum INT = 1,
	@PerPage INT = 3,
	@CurrentPage INT OUTPUT,
	@TotalPages INT OUTPUT,
	@TotalRows INT OUTPUT 
)
WITH RECOMPILE
As

SET NOCOUNT ON

declare @RowsToRetrieve int
	Declare @ID table (ItemID bigint ) --store IDs to build paged results as a simple join

	--get the relevant IDs
	insert into @ID SELECT DISTINCT I.ITEM_ID
		FROM TB_ITEM I
		INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
		TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
		AND TB_ITEM_REVIEW.IS_DELETED = @SHOW_DELETED
		
		where NOT I.ITEM_ID in
		(
		SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE -- Make sure the correct set is being used - the same code can appear in more than one set!
			INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID 
			INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID 
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID 
			inner join TB_ITEM_REVIEW IIR on TB_ITEM_ATTRIBUTE.ITEM_ID = IIR.ITEM_ID and IIR.IS_INCLUDED = @SHOW_INCLUDED and IIR.IS_DELETED = @SHOW_DELETED
			--the last line is useful to reduce the number of lines to evaluate: it speeds up the (sub)query itself!
	)
	--count results
	SELECT @TotalRows = count(ItemID) from @ID
	set @TotalPages = @TotalRows/@PerPage

	if @PageNum < 1
	set @PageNum = 1

	if @TotalRows % @PerPage != 0
	set @TotalPages = @TotalPages + 1

	set @RowsToRetrieve = @PerPage * @PageNum
	set @CurrentPage = @PageNum;

	WITH SearchResults AS
	(
	SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID
		, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
	FROM TB_ITEM I
	INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
		TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
		AND TB_ITEM_REVIEW.IS_DELETED = @SHOW_DELETED
	INNER JOIN @ID on I.ITEM_ID = ItemID
		
		--old and slow way of selecting relevant IDs. We did this already when populating @ID
		--where NOT I.ITEM_ID in
		--(
		--	SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE 
		--		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID 
		--		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID 
		--		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		--		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID 
		--		inner join TB_ITEM_REVIEW IIR on TB_ITEM_ATTRIBUTE.ITEM_ID = IIR.ITEM_ID and IIR.IS_INCLUDED = @SHOW_INCLUDED and IIR.IS_DELETED = @SHOW_DELETED
		--		-- Make sure the correct set is being used - the same code can appear in more than one set!
		--)
	
	)
	Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
			TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
			[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
			AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
			, SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
			--, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
		FROM SearchResults 
				  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
		WHERE RowNum > @RowsToRetrieve - @PerPage
		AND RowNum <= @RowsToRetrieve 
		ORDER BY SHORT_TITLE


SELECT	@CurrentPage as N'@CurrentPage',
		@TotalPages as N'@TotalPages',
		@TotalRows as N'@TotalRows'

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemListWithoutAttributes] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemReviewDeleteUndelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemReviewDeleteUndelete]
(
	@DELETE BIT,
	@ITEM_ID_LIST varchar(max),
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	UPDATE TB_ITEM_REVIEW
	set IS_DELETED = @DELETE,
	 IS_INCLUDED = CASE WHEN @DELETE = 'true' THEN 'false' ELSE 'true' END
	WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID IN
		(SELECT VALUE FROM dbo.fn_Split_int(@ITEM_ID_LIST, ','))
	AND not( IS_DELETED = 1 and IS_INCLUDED = 1)--this leaves shadow items alone
SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemReviewDeleteUndelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemReviewerCodingList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ItemReviewerCodingList]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@SET_ID INT,
	@COMPLETED BIT,
	
	@PageNum INT = 1,
	@PerPage INT = 3,
	@CurrentPage INT OUTPUT,
	@TotalPages INT OUTPUT,
	@TotalRows INT OUTPUT 
)

As

SET NOCOUNT ON

declare @RowsToRetrieve int

SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
FROM TB_ITEM I
INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
	TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'TRUE'
INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID

WHERE TB_ITEM_SET.SET_ID = @SET_ID
AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
AND TB_ITEM_SET.IS_COMPLETED = @COMPLETED

set @TotalPages = @TotalRows/@PerPage

	if @PageNum < 1
	set @PageNum = 1

	if @TotalRows % @PerPage != 0
	set @TotalPages = @TotalPages + 1

	set @RowsToRetrieve = @PerPage * @PageNum
	set @CurrentPage = @PageNum;

	WITH SearchResults AS
	(
SELECT I.ITEM_ID, I.[TYPE_ID], I.OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
	TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
	[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
	AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENTAUTHORS
	,TB_ITEM_REVIEW.MASTER_ITEM_ID, DOI, KEYWORDS
	, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
FROM TB_ITEM I
INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
	TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID

WHERE TB_ITEM_SET.SET_ID = @SET_ID
AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
AND TB_ITEM_SET.IS_COMPLETED = @COMPLETED
AND IS_DELETED != 'TRUE'

)
	Select ITEM_ID, [TYPE_ID], OLD_ITEM_ID, AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, PARENTAUTHORS
		,SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
		, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
	FROM SearchResults 
	WHERE RowNum > @RowsToRetrieve - @PerPage
	AND RowNum <= @RowsToRetrieve 
	option (optimize for unknown)
SELECT	@CurrentPage as N'@CurrentPage',
		@TotalPages as N'@TotalPages',
		@TotalRows as N'@TotalRows'

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemReviewerCodingList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemReviewerCodingListUncomplete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ItemReviewerCodingListUncomplete]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@SET_ID INT,
	
	@PageNum INT = 1,
	@PerPage INT = 3,
	@CurrentPage INT OUTPUT,
	@TotalPages INT OUTPUT,
	@TotalRows INT OUTPUT 
)

As

SET NOCOUNT ON

declare @RowsToRetrieve int

SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
FROM TB_ITEM I
INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
	TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'TRUE'
INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID

WHERE (TB_ITEM_SET.SET_ID = @SET_ID
AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
AND TB_ITEM_SET.IS_COMPLETED = 'FALSE')
AND NOT TB_ITEM_SET.ITEM_ID IN
(
	SELECT IS2.ITEM_ID FROM TB_ITEM_SET IS2
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = IS2.ITEM_ID
		WHERE IS2.SET_ID = @SET_ID
		AND IS2.IS_COMPLETED = 'TRUE'
)

set @TotalPages = @TotalRows/@PerPage

	if @PageNum < 1
	set @PageNum = 1

	if @TotalRows % @PerPage != 0
	set @TotalPages = @TotalPages + 1

	set @RowsToRetrieve = @PerPage * @PageNum
	set @CurrentPage = @PageNum;

	WITH SearchResults AS
	(
SELECT I.ITEM_ID, I.[TYPE_ID], I.OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
	TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
	[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
	AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENTAUTHORS
	,TB_ITEM_REVIEW.MASTER_ITEM_ID, DOI, KEYWORDS
	, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
FROM TB_ITEM I
INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
	TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID

WHERE (TB_ITEM_SET.SET_ID = @SET_ID
AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
AND TB_ITEM_SET.IS_COMPLETED = 'FALSE'
AND IS_DELETED != 'TRUE')
AND NOT TB_ITEM_SET.ITEM_ID IN
(
	SELECT IS2.ITEM_ID FROM TB_ITEM_SET IS2
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = IS2.ITEM_ID
		WHERE IS2.SET_ID = @SET_ID
		AND IS2.IS_COMPLETED = 'TRUE'
)

)
	Select ITEM_ID, [TYPE_ID], OLD_ITEM_ID, AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, PARENTAUTHORS
		,SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
		, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
	FROM SearchResults 
	WHERE RowNum > @RowsToRetrieve - @PerPage
	AND RowNum <= @RowsToRetrieve 
	option (optimize for unknown)
SELECT	@CurrentPage as N'@CurrentPage',
		@TotalPages as N'@TotalPages',
		@TotalRows as N'@TotalRows'

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemReviewerCodingListUncomplete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemScreenNext]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemScreenNext]
(
	@REVIEW_ID INT,
	-- @CURRENT_ITEM_ID BIGINT, Can't remember why this parameter is there
	@SHOW_PREVIOUSLY_SCREENED BIT = 'false',
	@SET_ID INT, -- NEEDED, AS CONTACT_ID IS SIGNIFICANT SOMETIMES (IN DOUBLE SCREENING IT IS)
	@IS_CODING_FINAL BIT, -- TRUE / FALSE - DEPENDS ON THE SET_ID IN THE GIVEN REVIEW
	@CONTACT_ID INT,
	@ATTRIBUTE_SET_ID BIGINT = 0 -- SCREENING A PARTICULAR GROUP OF STUDIES, OR ALL?
)

/* Returns the next item to be screened by a reviewer. The set of documents to select within can either be a 'group' - i.e. those coded with
a given attribute, or all in the review. (Specific 'groups' can be assigned to particular reviewers for screening.)

First we decide which set of items we are selecting from - all in review, or those assigned with a particular attribute.

We also need to return either those items already screened, or all items (depending on user choice).
*/

As

SET NOCOUNT ON

IF (@ATTRIBUTE_SET_ID = 0) /* SELECT FROM ALL ITEMS IN THE REVIEW */
BEGIN

	IF (@SHOW_PREVIOUSLY_SCREENED = 'True')
	BEGIN
	-- First, just grab all the usual fields for item information
	SELECT TOP (1) I.ITEM_ID, I.[TYPE_ID], [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, DOI, KEYWORDS
	FROM TB_ITEM I

	-- Limit to a given review
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID

	ORDER BY NEWID()

	END
	ELSE
	BEGIN
	-- First, just grab all the usual fields for item information
	SELECT TOP(1) I.ITEM_ID, I.[TYPE_ID], [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, DOI, KEYWORDS
	FROM TB_ITEM I

	-- Limit to a given review
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID

	-- Limit to items not already coded with a given code set
	WHERE I.ITEM_ID IN (SELECT * FROM dbo.fn_ItemsSetUncoded(@REVIEW_ID, @SET_ID, @IS_CODING_FINAL, @CONTACT_ID))

	ORDER BY NEWID()

	END
END
ELSE
BEGIN

	IF (@SHOW_PREVIOUSLY_SCREENED = 'True')
	BEGIN
	-- First, just grab all the usual fields for item information
	SELECT TOP(1) I.ITEM_ID, I.[TYPE_ID], [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, DOI, KEYWORDS
	FROM TB_ITEM I

	-- Limit to a given review
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID

	-- Limit to a given attribute
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		AND TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID
	
	ORDER BY NEWID()

	END
	ELSE
	BEGIN
	-- First, just grab all the usual fields for item information
	SELECT TOP(1) I.ITEM_ID, I.[TYPE_ID], [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, DOI, KEYWORDS
	FROM TB_ITEM I

	-- Limit to a given review
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID

	-- Limit to a given attribute
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		AND TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID

	-- Limit to items not already coded with a given code set
	WHERE I.ITEM_ID IN (SELECT * FROM dbo.fn_ItemsSetUncoded(@REVIEW_ID, @SET_ID, @IS_CODING_FINAL, @CONTACT_ID))

	ORDER BY NEWID()

	END
END

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemScreenNext] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSearchList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemSearchList] (
      @REVIEW_ID INT,
      @SEARCH_ID INT,
      
      @PageNum INT = 1,
      @PerPage INT = 3,
      @CurrentPage INT OUTPUT,
      @TotalPages INT OUTPUT,
      @TotalRows INT OUTPUT
)

As

SET NOCOUNT ON

declare @RowsToRetrieve int

      SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
      FROM TB_ITEM_REVIEW I
      INNER JOIN TB_SEARCH_ITEM ON TB_SEARCH_ITEM.ITEM_ID = I.ITEM_ID
      AND TB_SEARCH_ITEM.SEARCH_ID = @SEARCH_ID
      AND I.REVIEW_ID = @REVIEW_ID

set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, ITEM_RANK, TB_ITEM_REVIEW.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by ITEM_RANK desc) RowNum
      FROM TB_ITEM I
            INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
                  TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            INNER JOIN TB_SEARCH_ITEM ON TB_SEARCH_ITEM.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
                  AND TB_SEARCH_ITEM.SEARCH_ID = @SEARCH_ID
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  ,SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS 
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve
                  ORDER BY ITEM_RANK desc
                  
      OPTION (OPTIMIZE FOR (@PerPage=700, @SEARCH_ID UNKNOWN))
SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'


SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemSearchList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSetBulkComplete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemSetBulkComplete]
(
	@SET_ID INT,
	@COMPLETE BIT,
	@REVIEW_ID INT,
	@CONTACT_ID INT
)

As

SET NOCOUNT ON

IF (@COMPLETE = 'FALSE')
BEGIN
	UPDATE TB_ITEM_SET
		SET IS_COMPLETED = 'FALSE'
		WHERE SET_ID = @SET_ID
			AND CONTACT_ID = @CONTACT_ID
			AND ITEM_ID IN (SELECT ITEM_ID FROM TB_ITEM_REVIEW WHERE REVIEW_ID = @REVIEW_ID)
			--AND NOT IS_LOCKED = 'TRUE'
END
ELSE
BEGIN
	UPDATE TB_ITEM_SET
		SET IS_COMPLETED = 'TRUE'
		WHERE SET_ID = @SET_ID
			AND CONTACT_ID = @CONTACT_ID
			AND ITEM_ID IN (SELECT ITEM_ID FROM TB_ITEM_REVIEW WHERE REVIEW_ID = @REVIEW_ID)
			AND NOT ITEM_ID IN (SELECT ITEM_ID FROM TB_ITEM_SET IS2 WHERE IS2.IS_COMPLETED = 'TRUE'
									AND IS2.SET_ID = @SET_ID)
			--AND NOT IS_LOCKED = 'TRUE'
END

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemSetBulkComplete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSetComplete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemSetComplete]
(
	@ITEM_SET_ID BIGINT,
	@COMPLETE BIT,
	@IS_LOCKED BIT,
	@SUCCESSFUL BIT OUTPUT
)

As

SET NOCOUNT ON

IF (@COMPLETE = 'FALSE')
BEGIN
	UPDATE TB_ITEM_SET
		SET IS_COMPLETED = 'FALSE', IS_LOCKED = @IS_LOCKED
		WHERE ITEM_SET_ID = @ITEM_SET_ID
	SET @SUCCESSFUL = 'TRUE'
END
ELSE
BEGIN
	SELECT IS1.ITEM_SET_ID FROM TB_ITEM_SET IS1
		INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_ID = IS1.ITEM_ID
			AND IS2.SET_ID = IS1.SET_ID AND IS2.IS_COMPLETED = 'TRUE'
			AND IS2.ITEM_SET_ID <> @ITEM_SET_ID
		WHERE IS1.ITEM_SET_ID = @ITEM_SET_ID
		
	IF (@@ROWCOUNT > 0)
	BEGIN
		SET @SUCCESSFUL = 'FALSE'
	END
	ELSE
	BEGIN
		UPDATE TB_ITEM_SET
			SET IS_COMPLETED = 'TRUE', IS_LOCKED = @IS_LOCKED
			WHERE ITEM_SET_ID = @ITEM_SET_ID
		SET @SUCCESSFUL = 'TRUE'
	END
END

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemSetComplete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSetDataList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemSetDataList] (
	@REVIEW_ID INT,
	--@CONTACT_ID INT,
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON
	--this was changed on Aug 2013, previous version is commented below.
	--the new version gets: all completed sets for the item, plus all coded text
	--the old version was called by ItemSetList and was grabbing what was needed by the current user in DialogCoding:
	--that's the completed sets, plus the incomplete ones that belong to the user when a completed version isn't present.
	
	--first, grab the completed item set (if any)
	SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
		CODING_IS_FINAL, SET_NAME, CONTACT_NAME
	FROM TB_ITEM_SET
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
	WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
		--AND TB_REVIEW_SET.CODING_IS_FINAL = 'true'
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	
	--second, get all data from TB_ITEM_ATTRIBUTE_PDF and TB_ITEM_ATTRIBUTE_TEXT using union and only from completed sets
	SELECT  tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, p.ITEM_ATTRIBUTE_PDF_ID as [ID]
			, 'Page ' + CONVERT
							(varchar(10),PAGE) 
							+ ':' + CHAR(10) + '[¬s]"' 
							+ replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' 
				as [TEXT] 
			, NULL as [TEXT_FROM], NULL as [TEXT_TO]
			, 1 as IS_FROM_PDF
		from TB_REVIEW_SET rs
		inner join TB_ITEM_SET tis on rs.REVIEW_ID = @REVIEW_ID and tis.SET_ID = rs.SET_ID and tis.ITEM_ID = @ITEM_ID
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE_PDF p on ia.ITEM_ATTRIBUTE_ID = p.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on p.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
	UNION
	SELECT tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, t.ITEM_ATTRIBUTE_TEXT_ID as [ID]
			, SUBSTRING(
					replace(id.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) 
				 as [TEXT]
			, TEXT_FROM, TEXT_TO 
			, 0 as IS_FROM_PDF
		from TB_REVIEW_SET rs
		inner join TB_ITEM_SET tis on rs.REVIEW_ID = @REVIEW_ID and tis.SET_ID = rs.SET_ID and tis.ITEM_ID = @ITEM_ID
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE_TEXT t on ia.ITEM_ATTRIBUTE_ID = t.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on t.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
	ORDER by IS_FROM_PDF, [TEXT]	
	--old version starts here
	/* Collects just the item sets that are needed by a given reviewer - not all of them for every item
	   Critically, this query NOTs out the set_ids already identified.
	 */

	-- first, grab the completed item set (if any)
	--SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
	--	CODING_IS_FINAL, SET_NAME, CONTACT_NAME
	--FROM TB_ITEM_SET
	--	INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
	--	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
	--	INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
	--WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
	--	--AND TB_REVIEW_SET.CODING_IS_FINAL = 'true'
	--	AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	
	--UNION
	----second get incomplete item_sets for the current Reviewer if no complete set is present
	--	SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
	--		CODING_IS_FINAL, SET_NAME, CONTACT_NAME
	--	FROM TB_ITEM_SET
	--		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
	--		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
	--		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
	--	WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
	--		and tb_ITEM_SET.IS_COMPLETED = 'false'
	--		and TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
	--	AND NOT TB_ITEM_SET.SET_ID IN
	--	(
	--		SELECT TB_ITEM_SET.SET_ID FROM TB_ITEM_SET
	--			INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
	--			WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
	--			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	--	)
	--end of old version
SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemSetDataList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSetDataListAll]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemSetDataListAll]
(
	@REVIEW_ID INT,
	--@CONTACT_ID INT,
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON

	/* Collects all the item sets in a review
	 */

	SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
		CODING_IS_FINAL, SET_NAME, CONTACT_NAME
	FROM TB_ITEM_SET
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
	WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
	
	ORDER BY SET_NAME

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemSetDataListAll] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSets]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemSets]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	SELECT REVIEW_SET_ID, REVIEW_ID, RS.SET_ID, ALLOW_CODING_EDITS, S.SET_TYPE_ID, SET_NAME, SET_TYPE, CODING_IS_FINAL
	FROM TB_REVIEW_SET RS
	INNER JOIN TB_SET S ON S.SET_ID = RS.SET_ID
	INNER JOIN TB_SET_TYPE ON TB_SET_TYPE.SET_TYPE_ID = S.SET_TYPE_ID

	WHERE RS.REVIEW_ID = @REVIEW_ID


SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemSets] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSingleSetDataList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ItemSingleSetDataList] (
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@ITEM_ID BIGINT,
	@SET_ID INT
)

As

SET NOCOUNT ON

	/* Collects just the item sets that are needed by a given reviewer - not all of them for every item
	   Critically, this query NOTs out the set_ids already identified.
	 */
	--the code below comes straight from st_ItemSetDataList with the added
	--AND TB_ITEM_SET.SET_ID = @SET_ID clauses so to get info only from one set
	
	-- first, grab the completed item set (if any)
	SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
		CODING_IS_FINAL, SET_NAME, CONTACT_NAME
	FROM TB_ITEM_SET
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
	WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
		--AND TB_REVIEW_SET.CODING_IS_FINAL = 'true'
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		AND TB_ITEM_SET.SET_ID = @SET_ID
	UNION
	--second get incomplete item_sets for the current Reviewer if no complete set is present
		SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
			CODING_IS_FINAL, SET_NAME, CONTACT_NAME
		FROM TB_ITEM_SET
			INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
			INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
			INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
		WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
			and tb_ITEM_SET.IS_COMPLETED = 'false'
			and TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
			AND TB_ITEM_SET.SET_ID = @SET_ID
		AND NOT TB_ITEM_SET.SET_ID IN
		(
			SELECT TB_ITEM_SET.SET_ID FROM TB_ITEM_SET
				INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
				WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
				AND TB_ITEM_SET.SET_ID = @SET_ID
		)

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ItemSingleSetDataList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSourceDetails]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemSourceDetails]
(
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON

	SELECT SOURCE_NAME, TB_SOURCE.SOURCE_ID
	FROM TB_SOURCE
	INNER JOIN TB_ITEM_SOURCE ON TB_ITEM_SOURCE.SOURCE_ID = TB_SOURCE.SOURCE_ID
		AND TB_ITEM_SOURCE.ITEM_ID = @ITEM_ID


SET NOCOUNT OFF
GO
ALTER AUTHORIZATION ON [dbo].[st_ItemSourceDetails] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemsWithCodes]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemsWithCodes]
(
	@REVIEW_ID INT,
	@CODES NVARCHAR(MAX),
	@CODES_FROM NVARCHAR(MAX)
)

As

SET NOCOUNT ON

	SELECT IA1.ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE IA1
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = IA1.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN dbo.fn_Split(@CODES, ',') CODES ON CODES.VALUE = IA1.ATTRIBUTE_ID
		INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA1.ITEM_ID = IA2.ITEM_ID
		INNER JOIN dbo.fn_Split(@CODES_FROM, ',') CODES_FROM ON CODES_FROM.VALUE = IA2.ATTRIBUTE_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemsWithCodes] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemTermDictionary]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemTermDictionary]

As

SET NOCOUNT ON

SELECT TERM, SCORE FROM TB_ITEM_TERM_DICTIONARY
ORDER BY SCORE DESC

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemTermDictionary] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemTypeList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemTypeList]


As

SET NOCOUNT ON

	SELECT * FROM TB_ITEM_TYPE
	order by [TYPE_ID]
		

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemTypeList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemUpdate]
(
	@ITEM_ID BIGINT
,	@TITLE NVARCHAR(4000) = NULL
,	@TYPE_ID TINYINT
,	@PARENT_TITLE NVARCHAR(4000)
,	@SHORT_TITLE NVARCHAR(70)
,	@DATE_CREATED DATETIME = NULL
,	@CREATED_BY NVARCHAR(50) = NULL
,	@DATE_EDITED DATETIME = NULL
,	@EDITED_BY NVARCHAR(50) = NULL
,	@YEAR NCHAR(4) = NULL
,	@MONTH NVARCHAR(10) = NULL
,	@STANDARD_NUMBER NVARCHAR(255) = NULL
,	@CITY NVARCHAR(100) = NULL
,	@COUNTRY NVARCHAR(100) = NULL
,	@PUBLISHER NVARCHAR(1000) = NULL
,	@INSTITUTION NVARCHAR(1000) = NULL
,	@VOLUME NVARCHAR(56) = NULL
,	@PAGES NVARCHAR(50) = NULL
,	@EDITION NVARCHAR(200) = NULL
,	@ISSUE NVARCHAR(100) = NULL
,	@IS_LOCAL BIT = NULL
,	@AVAILABILITY NVARCHAR(255) = NULL
,	@URL NVARCHAR(500) = NULL
,	@COMMENTS NVARCHAR(MAX) = NULL
,	@ABSTRACT NVARCHAR(MAX) = NULL
,	@IS_INCLUDED BIT = NULL
,	@REVIEW_ID INT
,	@DOI NVARCHAR(500) = NULL
,	@KEYWORDS NVARCHAR(MAX) = NULL
)

As

SET NOCOUNT ON

UPDATE TB_ITEM

SET TITLE = @TITLE
,	[TYPE_ID] = @TYPE_ID
,	PARENT_TITLE = @PARENT_TITLE
,	SHORT_TITLE = @SHORT_TITLE
,	DATE_CREATED = @DATE_CREATED
,	CREATED_BY = @CREATED_BY
,	DATE_EDITED = @DATE_EDITED
,	EDITED_BY = @EDITED_BY
,	[YEAR] = @YEAR
,	[MONTH] = @MONTH
,	STANDARD_NUMBER = @STANDARD_NUMBER
,	CITY = @CITY
,	COUNTRY = @COUNTRY
,	PUBLISHER = @PUBLISHER
,	INSTITUTION = @INSTITUTION
,	VOLUME = @VOLUME
,	PAGES = @PAGES
,	EDITION = @EDITION
,	ISSUE = @ISSUE
,	IS_LOCAL = @IS_LOCAL
,	AVAILABILITY = @AVAILABILITY
,	URL = @URL
,	COMMENTS = @COMMENTS
,	ABSTRACT = @ABSTRACT
,	DOI = @DOI
,	KEYWORDS = @KEYWORDS

WHERE ITEM_ID = @ITEM_ID

UPDATE TB_ITEM_REVIEW
SET IS_INCLUDED = @IS_INCLUDED 
WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ItemWorkAllocationList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ItemWorkAllocationList] (
      @REVIEW_ID INT,
      @WORK_ALLOCATION_ID INT,
      @WHICH_FILTER NVARCHAR(10),
      
      @PageNum INT = 1,
      @PerPage INT = 3,
      @CurrentPage INT OUTPUT,
      @TotalPages INT OUTPUT,
      @TotalRows INT OUTPUT 
)

As

SET NOCOUNT ON

declare @RowsToRetrieve int

IF (@WHICH_FILTER = 'REMAINING')
BEGIN
      SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
      FROM TB_ITEM_REVIEW I
      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
      INNER JOIN TB_WORK_ALLOCATION ON TB_WORK_ALLOCATION.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'

      WHERE NOT I.ITEM_ID IN
            (SELECT ITEM_ID FROM TB_ITEM_SET
                  WHERE TB_ITEM_SET.SET_ID = TB_WORK_ALLOCATION.SET_ID AND TB_ITEM_SET.CONTACT_ID = TB_WORK_ALLOCATION.CONTACT_ID)
            AND TB_WORK_ALLOCATION.WORK_ALLOCATION_ID = @WORK_ALLOCATION_ID
            AND I.REVIEW_ID = @REVIEW_ID AND I.IS_DELETED = 'FALSE'

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
            
      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
      INNER JOIN TB_WORK_ALLOCATION ON TB_WORK_ALLOCATION.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'

      WHERE NOT I.ITEM_ID IN
            (SELECT ITEM_ID FROM TB_ITEM_SET
                  WHERE TB_ITEM_SET.SET_ID = TB_WORK_ALLOCATION.SET_ID AND TB_ITEM_SET.CONTACT_ID = TB_WORK_ALLOCATION.CONTACT_ID)
            AND TB_WORK_ALLOCATION.WORK_ALLOCATION_ID = @WORK_ALLOCATION_ID
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve
                  ORDER BY SHORT_TITLE
END
ELSE
IF (@WHICH_FILTER = 'ALL')
BEGIN
      SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
      FROM TB_ITEM_REVIEW I
      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
      INNER JOIN TB_WORK_ALLOCATION ON TB_WORK_ALLOCATION.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
      WHERE TB_WORK_ALLOCATION.WORK_ALLOCATION_ID = @WORK_ALLOCATION_ID
      AND I.REVIEW_ID = @REVIEW_ID AND I.IS_DELETED = 'FALSE'

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
      INNER JOIN TB_WORK_ALLOCATION ON TB_WORK_ALLOCATION.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
      WHERE TB_WORK_ALLOCATION.WORK_ALLOCATION_ID = @WORK_ALLOCATION_ID
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  ,SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
      FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve 
            ORDER BY SHORT_TITLE
END
ELSE
IF (@WHICH_FILTER = 'STARTED')
BEGIN
SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
      FROM TB_ITEM_REVIEW I
      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
      INNER JOIN TB_WORK_ALLOCATION ON TB_WORK_ALLOCATION.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'

      WHERE I.ITEM_ID IN
      (SELECT ITEM_ID FROM TB_ITEM_SET
            WHERE TB_ITEM_SET.SET_ID = TB_WORK_ALLOCATION.SET_ID AND TB_ITEM_SET.CONTACT_ID = TB_WORK_ALLOCATION.CONTACT_ID)
      AND TB_WORK_ALLOCATION.WORK_ALLOCATION_ID = @WORK_ALLOCATION_ID
      AND I.REVIEW_ID = @REVIEW_ID AND I.IS_DELETED = 'FALSE'
      
      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
      INNER JOIN TB_WORK_ALLOCATION ON TB_WORK_ALLOCATION.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'

      WHERE I.ITEM_ID IN
            (SELECT ITEM_ID FROM TB_ITEM_SET
                  WHERE TB_ITEM_SET.SET_ID = TB_WORK_ALLOCATION.SET_ID AND TB_ITEM_SET.CONTACT_ID = TB_WORK_ALLOCATION.CONTACT_ID)
            AND TB_WORK_ALLOCATION.WORK_ALLOCATION_ID = @WORK_ALLOCATION_ID
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  ,SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
      FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve
            ORDER BY SHORT_TITLE
END

SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ItemWorkAllocationList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysis]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_MetaAnalysis]
(
	@META_ANALYSIS_ID INT
)

As

SET NOCOUNT ON
	
	SELECT META_ANALYSIS_ID, META_ANALYSIS_TITLE, CONTACT_ID, REVIEW_ID,
	ATTRIBUTE_ID, SET_ID, ATTRIBUTE_ID_INTERVENTION, ATTRIBUTE_ID_CONTROL,
	ATTRIBUTE_ID_ANSWER, ATTRIBUTE_ID_QUESTION,	ATTRIBUTE_ID_OUTCOME, META_ANALYSIS_TYPE_ID
	
	FROM TB_META_ANALYSIS
	
	WHERE META_ANALYSIS_ID = @META_ANALYSIS_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_MetaAnalysis] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_MetaAnalysisDelete]
(
	@META_ANALYSIS_ID INT
)

As

SET NOCOUNT ON
	
	DELETE FROM TB_META_ANALYSIS_OUTCOME
	WHERE META_ANALYSIS_ID = @META_ANALYSIS_ID
	
	DELETE FROM TB_META_ANALYSIS
	WHERE META_ANALYSIS_ID = @META_ANALYSIS_ID
	

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_MetaAnalysisDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_MetaAnalysisInsert]
(
	@TITLE NVARCHAR(255),
	@CONTACT_ID INT,
	@REVIEW_ID INT,
	@ATTRIBUTE_ID BIGINT = NULL,
	@SET_ID INT = NULL,
	@ATTRIBUTE_ID_INTERVENTION BIGINT,
	@ATTRIBUTE_ID_CONTROL BIGINT,
	@ATTRIBUTE_ID_OUTCOME BIGINT,
	@META_ANALYSIS_TYPE_ID INT,
	@OUTCOME_IDS nvarchar(max),
	@ATTRIBUTE_ID_ANSWER nvarchar(max) = '',
	@ATTRIBUTE_ID_QUESTION nvarchar(max) = '',
	@GRID_SETTINGS NVARCHAR(MAX) = '',
	@NEW_META_ANALYSIS_ID INT OUTPUT,

	@ATTRIBUTE_ANSWER_TEXT NVARCHAR(MAX) OUTPUT,
	@ATTRIBUTE_QUESTION_TEXT NVARCHAR(MAX) OUTPUT
)

As

SET NOCOUNT ON
	
	INSERT INTO TB_META_ANALYSIS
	(	META_ANALYSIS_TITLE
	,	CONTACT_ID
	,	REVIEW_ID
	,	ATTRIBUTE_ID
	,	SET_ID
	,	ATTRIBUTE_ID_INTERVENTION
	,	ATTRIBUTE_ID_CONTROL
	,	ATTRIBUTE_ID_OUTCOME
	,	META_ANALYSIS_TYPE_ID
	,	ATTRIBUTE_ID_ANSWER
	,	ATTRIBUTE_ID_QUESTION
	,	GRID_SETTINGS
	)	
	VALUES
	(
		@TITLE
	,	@CONTACT_ID
	,	@REVIEW_ID
	,	@ATTRIBUTE_ID
	,	@SET_ID
	,	@ATTRIBUTE_ID_INTERVENTION
	,	@ATTRIBUTE_ID_CONTROL
	,	@ATTRIBUTE_ID_OUTCOME
	,	@META_ANALYSIS_TYPE_ID
	,	@ATTRIBUTE_ID_ANSWER
	,	@ATTRIBUTE_ID_QUESTION
	,	@GRID_SETTINGS
	)
	-- Get the identity and return it
	SET @NEW_META_ANALYSIS_ID = @@identity
	
	IF (@OUTCOME_IDS != '')
	BEGIN
		INSERT INTO TB_META_ANALYSIS_OUTCOME (META_ANALYSIS_ID, OUTCOME_ID)
		SELECT @NEW_META_ANALYSIS_ID, VALUE from DBO.fn_Split_int(@OUTCOME_IDS, ',')
	END

	SELECT @ATTRIBUTE_ANSWER_TEXT = COALESCE(@ATTRIBUTE_ANSWER_TEXT + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_ANSWER, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL

	SELECT @ATTRIBUTE_QUESTION_TEXT = COALESCE(@ATTRIBUTE_QUESTION_TEXT + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_QUESTION, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_MetaAnalysisInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_MetaAnalysisList]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT
)

As

SET NOCOUNT ON
	
	SELECT META_ANALYSIS_ID, META_ANALYSIS_TITLE, TB_META_ANALYSIS.CONTACT_ID, REVIEW_ID,
	TB_META_ANALYSIS.ATTRIBUTE_ID, SET_ID, ATTRIBUTE_ID_INTERVENTION, ATTRIBUTE_ID_CONTROL,
	ATTRIBUTE_ID_OUTCOME, TB_META_ANALYSIS.META_ANALYSIS_TYPE_ID, META_ANALYSIS_TYPE_TITLE,
	ATTRIBUTE_ID_ANSWER, ATTRIBUTE_ID_QUESTION,	GRID_SETTINGS,
	--A1.ATTRIBUTE_NAME AS INTERVENTION_TEXT,	a2.ATTRIBUTE_NAME AS CONTROL_TEXT, a3.ATTRIBUTE_NAME AS OUTCOME_TEXT,

	(SELECT ATTRIBUTE_NAME + '¬' FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_ANSWER, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		FOR XML PATH('')) AS ATTRIBUTE_ANSWER_TEXT,

	(SELECT ATTRIBUTE_NAME + '¬' FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_QUESTION, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		FOR XML PATH('')) AS ATTRIBUTE_QUESTION_TEXT
	
	FROM TB_META_ANALYSIS
	
	INNER JOIN TB_META_ANALYSIS_TYPE ON TB_META_ANALYSIS_TYPE.META_ANALYSIS_TYPE_ID =
		TB_META_ANALYSIS.META_ANALYSIS_TYPE_ID
	
	/*	
	don't need this any more (for old MA interface) ??
	left outer JOIN TB_ITEM_ATTRIBUTE IA1 ON IA1.ITEM_ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_INTERVENTION
	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_INTERVENTION 
	left outer JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_CONTROL
	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_CONTROL
	left outer JOIN TB_ITEM_ATTRIBUTE IA3 ON IA3.ITEM_ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_OUTCOME
	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = TB_META_ANALYSIS.ATTRIBUTE_ID_OUTCOME 
	*/
	WHERE REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF


/*

(SELECT COALESCE(ATTRIBUTE_ANSWER_TEXT + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(ATTRIBUTE_ID_ANSWER, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL) ATTRIBUTE_ANSWER_TEXT

		*/

GO
ALTER AUTHORIZATION ON [dbo].[st_MetaAnalysisList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_MetaAnalysisUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_MetaAnalysisUpdate]
(
	@META_ANALYSIS_ID INT,
	@TITLE NVARCHAR(255),
	@CONTACT_ID INT,
	@REVIEW_ID INT,
	@ATTRIBUTE_ID BIGINT = NULL,
	@SET_ID INT = NULL,
	@ATTRIBUTE_ID_INTERVENTION BIGINT,
	@ATTRIBUTE_ID_CONTROL BIGINT,
	@ATTRIBUTE_ID_OUTCOME BIGINT,
	@OUTCOME_IDS nvarchar(max),
	@ATTRIBUTE_ID_ANSWER NVARCHAR(MAX) = '',
	@ATTRIBUTE_ID_QUESTION NVARCHAR(MAX) = '',
	@META_ANALYSIS_TYPE_ID INT,
	@GRID_SETTINGS NVARCHAR(MAX) = '',

	@ATTRIBUTE_ANSWER_TEXT NVARCHAR(MAX) OUTPUT,
	@ATTRIBUTE_QUESTION_TEXT NVARCHAR(MAX) OUTPUT
)

As

SET NOCOUNT ON
	
	UPDATE TB_META_ANALYSIS
	SET	META_ANALYSIS_TITLE = @TITLE
	,	CONTACT_ID = @CONTACT_ID
	,	REVIEW_ID = @REVIEW_ID
	,	ATTRIBUTE_ID = @ATTRIBUTE_ID
	,	SET_ID = @SET_ID
	,	ATTRIBUTE_ID_INTERVENTION = @ATTRIBUTE_ID_INTERVENTION
	,	ATTRIBUTE_ID_CONTROL = @ATTRIBUTE_ID_CONTROL
	,	ATTRIBUTE_ID_OUTCOME = @ATTRIBUTE_ID_OUTCOME
	,	META_ANALYSIS_TYPE_ID = @META_ANALYSIS_TYPE_ID
	,	ATTRIBUTE_ID_ANSWER = @ATTRIBUTE_ID_ANSWER
	,	ATTRIBUTE_ID_QUESTION = @ATTRIBUTE_ID_QUESTION
	,	GRID_SETTINGS = @GRID_SETTINGS
	
	WHERE META_ANALYSIS_ID = @META_ANALYSIS_ID
	
	DELETE FROM TB_META_ANALYSIS_OUTCOME WHERE META_ANALYSIS_ID = @META_ANALYSIS_ID
	
	IF (@OUTCOME_IDS != '')
	BEGIN
		INSERT INTO TB_META_ANALYSIS_OUTCOME (META_ANALYSIS_ID, OUTCOME_ID)
		SELECT @META_ANALYSIS_ID, VALUE from DBO.fn_Split_int(@OUTCOME_IDS, ',')
	END

	SELECT @ATTRIBUTE_ANSWER_TEXT = COALESCE(@ATTRIBUTE_ANSWER_TEXT + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_ANSWER, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL

	SELECT @ATTRIBUTE_QUESTION_TEXT = COALESCE(@ATTRIBUTE_QUESTION_TEXT + '¬', '') + ATTRIBUTE_NAME 
	FROM TB_ATTRIBUTE
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_ID_QUESTION, ',') id ON id.value = TB_ATTRIBUTE.ATTRIBUTE_ID
		WHERE ATTRIBUTE_NAME IS NOT NULL

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_MetaAnalysisUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_OutcomeItemAttributeList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_OutcomeItemAttributeList]
(
	@OUTCOME_ID INT
)

As

SET NOCOUNT ON

SELECT ITEM_OUTCOME_ATTRIBUTE_ID, OUTCOME_ID, a.ATTRIBUTE_ID, ADDITIONAL_TEXT, a.ATTRIBUTE_NAME
	FROM TB_ITEM_OUTCOME_ATTRIBUTE ioa
	inner join TB_ATTRIBUTE a on ioa.ATTRIBUTE_ID = a.ATTRIBUTE_ID
	WHERE OUTCOME_ID = @OUTCOME_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_OutcomeItemAttributeList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_OutcomeItemAttributesSave]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_OutcomeItemAttributesSave]
(
	@OUTCOME_ID INT,
	@ATTRIBUTES NVARCHAR(MAX)
)

As

SET NOCOUNT ON
	declare @t TABLE (value bigint)
	DELETE FROM TB_ITEM_OUTCOME_ATTRIBUTE
		WHERE OUTCOME_ID = @OUTCOME_ID
	INSERT into @t select distinct value from dbo.fn_Split_int(@ATTRIBUTES, ',') where value > 0 
	--you get one line with 0 as value if you try to split an empty string	
	if @@ROWCOUNT >0 
	BEGIN
		INSERT INTO TB_ITEM_OUTCOME_ATTRIBUTE(OUTCOME_ID, ATTRIBUTE_ID)
		SELECT @OUTCOME_ID, VALUE FROM @t
	END
SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_OutcomeItemAttributesSave] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_OutcomeItemDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[st_OutcomeItemDelete] 
	-- Add the parameters for the stored procedure here
	
	@OUTCOME_ID INT
AS
BEGIN
	DELETE FROM TB_ITEM_OUTCOME_ATTRIBUTE WHERE OUTCOME_ID = @OUTCOME_ID
	DELETE FROM TB_ITEM_OUTCOME WHERE OUTCOME_ID = @OUTCOME_ID
END


GO
ALTER AUTHORIZATION ON [dbo].[st_OutcomeItemDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_OutcomeItemInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_OutcomeItemInsert]
(
	@ITEM_SET_ID BIGINT,
	@OUTCOME_TYPE_ID INT = 1,
	@ITEM_ATTRIBUTE_ID_INTERVENTION BIGINT,
	@ITEM_ATTRIBUTE_ID_CONTROL BIGINT,
	@ITEM_ATTRIBUTE_ID_OUTCOME BIGINT,
	@OUTCOME_TITLE NVARCHAR(255),
	@OUTCOME_DESCRIPTION NVARCHAR(4000),
	@DATA1 DECIMAL (18, 9),
	@DATA2 DECIMAL (18, 9),
	@DATA3 DECIMAL (18, 9),
	@DATA4 DECIMAL (18, 9),
	@DATA5 DECIMAL (18, 9),
	@DATA6 DECIMAL (18, 9),
	@DATA7 DECIMAL (18, 9),
	@DATA8 DECIMAL (18, 9),
	@DATA9 DECIMAL (18, 9),
	@DATA10 DECIMAL (18, 9),
	@DATA11 DECIMAL (18, 9),
	@DATA12 DECIMAL (18, 9),
	@DATA13 DECIMAL (18, 9),
	@DATA14 DECIMAL (18, 9),
	@NEW_OUTCOME_ID INT OUTPUT
)

As

SET NOCOUNT ON
	
	INSERT INTO TB_ITEM_OUTCOME
	(	ITEM_SET_ID
	,	OUTCOME_TYPE_ID
	,	ITEM_ATTRIBUTE_ID_INTERVENTION
	,	ITEM_ATTRIBUTE_ID_CONTROL
	,	ITEM_ATTRIBUTE_ID_OUTCOME
	,	OUTCOME_TITLE
	,	OUTCOME_DESCRIPTION
	,	DATA1
	,	DATA2
	,	DATA3
	,	DATA4
	,	DATA5
	,	DATA6
	,	DATA7
	,	DATA8
	,	DATA9
	,	DATA10
	,	DATA11
	,	DATA12
	,	DATA13
	,	DATA14
	)	
	VALUES
	(
		@ITEM_SET_ID
	,	@OUTCOME_TYPE_ID
	,	@ITEM_ATTRIBUTE_ID_INTERVENTION
	,	@ITEM_ATTRIBUTE_ID_CONTROL
	,	@ITEM_ATTRIBUTE_ID_OUTCOME
	,	@OUTCOME_TITLE
	,	@OUTCOME_DESCRIPTION
	,	@DATA1
	,	@DATA2
	,	@DATA3
	,	@DATA4
	,	@DATA5
	,	@DATA6
	,	@DATA7
	,	@DATA8
	,	@DATA9
	,	@DATA10
	,	@DATA11
	,	@DATA12
	,	@DATA13
	,	@DATA14
	)
	-- Get the identity and return it
	SET @NEW_OUTCOME_ID = @@identity

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_OutcomeItemInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_OutcomeItemList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_OutcomeItemList]
(
	@REVIEW_ID INT,
	@ITEM_SET_ID BIGINT
)

As

SET NOCOUNT ON

SELECT OUTCOME_ID, SHORT_TITLE, TB_ITEM_OUTCOME.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, OUTCOME_DESCRIPTION,
	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
	A1.ATTRIBUTE_NAME AS INTERVENTION_TEXT,
	a2.ATTRIBUTE_NAME AS CONTROL_TEXT,
	a3.ATTRIBUTE_NAME AS OUTCOME_TEXT,
	0 as META_ANALYSIS_OUTCOME_ID -- Meta-analysis id. 0 as not selected
FROM TB_ITEM_OUTCOME
INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_OUTCOME.ITEM_SET_ID
INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
left outer JOIN TB_ATTRIBUTE IA1 ON IA1.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_INTERVENTION
left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = IA1.ATTRIBUTE_ID 
left outer JOIN TB_ATTRIBUTE IA2 ON IA2.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_CONTROL
left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
left outer JOIN TB_ATTRIBUTE IA3 ON IA3.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_OUTCOME
left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = IA3.ATTRIBUTE_ID 

WHERE TB_ITEM_OUTCOME.ITEM_SET_ID = @ITEM_SET_ID


SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_OutcomeItemList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_OutcomeItemUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_OutcomeItemUpdate]
(
	@OUTCOME_ID INT,
	@OUTCOME_TYPE_ID INT,
	@ITEM_ATTRIBUTE_ID_INTERVENTION BIGINT,
	@ITEM_ATTRIBUTE_ID_CONTROL BIGINT,
	@ITEM_ATTRIBUTE_ID_OUTCOME BIGINT,
	@OUTCOME_TITLE NVARCHAR(255),
	@OUTCOME_DESCRIPTION NVARCHAR(4000),
	@DATA1 DECIMAL (18, 9),
	@DATA2 DECIMAL (18, 9),
	@DATA3 DECIMAL (18, 9),
	@DATA4 DECIMAL (18, 9),
	@DATA5 DECIMAL (18, 9),
	@DATA6 DECIMAL (18, 9),
	@DATA7 DECIMAL (18, 9),
	@DATA8 DECIMAL (18, 9),
	@DATA9 DECIMAL (18, 9),
	@DATA10 DECIMAL (18, 9),
	@DATA11 DECIMAL (18, 9),
	@DATA12 DECIMAL (18, 9),
	@DATA13 DECIMAL (18, 9),
	@DATA14 DECIMAL (18, 9)
)

As

SET NOCOUNT ON
	
	UPDATE TB_ITEM_OUTCOME SET
	OUTCOME_TYPE_ID = @OUTCOME_TYPE_ID,
	ITEM_ATTRIBUTE_ID_INTERVENTION = @ITEM_ATTRIBUTE_ID_INTERVENTION,
	ITEM_ATTRIBUTE_ID_CONTROL = @ITEM_ATTRIBUTE_ID_CONTROL,
	ITEM_ATTRIBUTE_ID_OUTCOME = @ITEM_ATTRIBUTE_ID_OUTCOME,
	OUTCOME_TITLE = @OUTCOME_TITLE,
	OUTCOME_DESCRIPTION = @OUTCOME_DESCRIPTION,
	DATA1 = @DATA1,
	DATA2 = @DATA2,
	DATA3 = @DATA3,
	DATA4 = @DATA4,
	DATA5 = @DATA5,
	DATA6 = @DATA6,
	DATA7 = @DATA7,
	DATA8 = @DATA8,
	DATA9 = @DATA9,
	DATA10 = @DATA10,
	DATA11 = @DATA11,
	DATA12 = @DATA12,
	DATA13 = @DATA13,
	DATA14 = @DATA14
	
	WHERE OUTCOME_ID = @OUTCOME_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_OutcomeItemUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_OutcomeList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_OutcomeList]
(
	@REVIEW_ID INT,
	@META_ANALYSIS_ID INT,
	@QUESTIONS nvarchar(max) = '',
	@ANSWERS nvarchar(max) = ''
	/*@SET_ID BIGINT,
	@ITEM_ATTRIBUTE_ID_INTERVENTION BIGINT = NULL,
	@ITEM_ATTRIBUTE_ID_CONTROL BIGINT = NULL,
	@ITEM_ATTRIBUTE_ID_OUTCOME BIGINT = NULL,
	@ATTRIBUTE_ID BIGINT = NULL,
	
	
	@VARIABLES NVARCHAR(MAX) = NULL,
	@ANSWERS NVARCHAR(MAX) = '',
	@QUESTIONS NVARCHAR(MAX) = ''*/
)

As

SET NOCOUNT ON
	declare @t table (OUTCOME_ID int, META_ANALYSIS_OUTCOME_ID int)
	insert into @t select tio.OUTCOME_ID, META_ANALYSIS_OUTCOME_ID from 
	TB_ITEM_OUTCOME tio
	inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tio.ITEM_SET_ID
		AND tis.IS_COMPLETED = 1
	inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @REVIEW_ID and rs.SET_ID = tis.SET_ID
	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tis.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tis.ITEM_SET_ID
	inner join TB_ITEM on TB_ITEM.ITEM_ID = tis.ITEM_ID
	inner JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID

	SELECT distinct tio.OUTCOME_ID, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, SHORT_TITLE, OUTCOME_DESCRIPTION,
	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
	t.META_ANALYSIS_OUTCOME_ID, 
	tis.ITEM_SET_ID, TB_ITEM_ATTRIBUTE.ITEM_ID,
	A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT, A3.ATTRIBUTE_NAME OUTCOME_TEXT
	FROM TB_ITEM_OUTCOME tio
	inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tio.ITEM_SET_ID
		AND tis.IS_COMPLETED = 1
	inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @REVIEW_ID and rs.SET_ID = tis.SET_ID
	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tis.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tis.ITEM_SET_ID
	inner join TB_ITEM on TB_ITEM.ITEM_ID = tis.ITEM_ID
	LEFT OUTER JOIN @t t ON t.OUTCOME_ID = tio.OUTCOME_ID
		--AND t.META_ANALYSIS_ID = @META_ANALYSIS_ID
	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_INTERVENTION 
	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_CONTROL
	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_OUTCOME

	--second sets of results, the answers
	--we need to get these, even if empty, so that we always get a reader
	
	IF (@QUESTIONS is not null AND @QUESTIONS != '')
	BEGIN
		declare @QT table ( AttID bigint primary key)
		insert into @QT select qss.value from dbo.fn_Split_int(@QUESTIONS, ',') as qss
		select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID 
			, ATTRIBUTE_NAME as Codename
			, a.ATTRIBUTE_ID as ATTRIBUTE_ID
			, tas.PARENT_ATTRIBUTE_ID
		from TB_ITEM_OUTCOME tio 
		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
		inner join TB_ITEM_SET tis2 on tis.ITEM_ID = tis2.ITEM_ID
		inner join TB_ITEM_ATTRIBUTE tia on tis2.ITEM_SET_ID = tia.ITEM_SET_ID
		inner join TB_REVIEW_SET rs on tis2.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
		inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
		inner join @QT Qs on Qs.AttID = tas.PARENT_ATTRIBUTE_ID
		inner join TB_ATTRIBUTE a on tas.ATTRIBUTE_ID = a.ATTRIBUTE_ID
		order by OUTCOME_ID, tas.PARENT_ATTRIBUTE_ID, tas.ATTRIBUTE_ORDER, a.ATTRIBUTE_ID
	END
	ELSE
	BEGIN
	select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
		tio.OUTCOME_TITLE as Codename
		, tio.ITEM_ATTRIBUTE_ID_CONTROL as ATTRIBUTE_ID, tio.ITEM_ATTRIBUTE_ID_CONTROL as PARENT_ATTRIBUTE_ID
		from TB_ITEM_OUTCOME tio
		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and 1=0
	END


	IF (@ANSWERS is not null AND @ANSWERS != '')
	BEGIN
	--third set of results, the questions
	declare @AT table ( AttID bigint primary key)
	insert into @AT select qss.value from dbo.fn_Split_int(@ANSWERS, ',') as qss
	select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
		--(Select top(1) a.ATTRIBUTE_NAME from @AT 
		--inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = AttID
		--inner join TB_ATTRIBUTE a on tas.ATTRIBUTE_ID = a.ATTRIBUTE_ID
		--inner join TB_ITEM_ATTRIBUTE tia on a.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
		--inner join TB_ITEM_SET tis1 on tia.ITEM_SET_ID = tis1.ITEM_SET_ID and tis1.IS_COMPLETED = 1 and tis.ITEM_ID = tis1.ITEM_ID
		--inner join TB_REVIEW_SET rs1 on tis1.SET_ID = rs1.SET_ID and rs1.REVIEW_ID = @REVIEW_ID
		--order by tas.ATTRIBUTE_ORDER ) as Codename
		tia.ATTRIBUTE_ID, a.ATTRIBUTE_NAME
	from TB_ITEM_OUTCOME tio 
	inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
	inner join TB_REVIEW_SET rs on tis.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
	inner join TB_ITEM_SET tis2 on tis.ITEM_ID = tis2.ITEM_ID and tis2.IS_COMPLETED = 1
	inner join TB_ATTRIBUTE_SET tas on tis2.SET_ID = tas.SET_ID
	inner join TB_ITEM_ATTRIBUTE tia on tis2.ITEM_ID = tia.ITEM_ID and tas.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
	inner join @AT on AttID = tia.ATTRIBUTE_ID
	inner join TB_ATTRIBUTE a on tia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
	order by OUTCOME_ID
	END
	ELSE
	BEGIN
		select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
		tio.ITEM_ATTRIBUTE_ID_CONTROL as ATTRIBUTE_ID, tio.OUTCOME_TITLE as ATTRIBUTE_NAME
	from TB_ITEM_OUTCOME tio inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and 1=0
	
	END
	
--DECLARE @START_TEXT NVARCHAR(MAX) = N' SELECT distinct tio.OUTCOME_ID, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
--	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, SHORT_TITLE, OUTCOME_DESCRIPTION,
--	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
--	TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_OUTCOME_ID, TB_ITEM_SET.ITEM_SET_ID, TB_ITEM_ATTRIBUTE.ITEM_ID,
--	A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT, A3.ATTRIBUTE_NAME OUTCOME_TEXT'
	
--DECLARE @END_TEXT NVARCHAR(MAX) = N' FROM TB_ITEM_OUTCOME tio

--	inner join TB_ITEM_SET on tb_item_set.ITEM_SET_ID = tio.ITEM_SET_ID
--		AND TB_ITEM_SET.IS_COMPLETED = ''TRUE''
--	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tb_item_set.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tb_item_set.ITEM_SET_ID
--	inner join TB_ITEM on TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
	
--	LEFT OUTER JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
--		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID
		
--	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_INTERVENTION 
--	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_CONTROL
--	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_OUTCOME'
	
--DECLARE @QUERY NVARCHAR(MAX) = @VARIABLES + @START_TEXT + @ANSWERS + @QUESTIONS + @END_TEXT
	
--EXEC (@QUERY)

--/*
--SELECT distinct tio.OUTCOME_ID, SHORT_TITLE, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
--	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, OUTCOME_DESCRIPTION,
--	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
--	TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_OUTCOME_ID, TB_ITEM_SET.ITEM_SET_ID,
--	TB_ITEM_ATTRIBUTE.ITEM_ID, A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT,
--		A3.ATTRIBUTE_NAME OUTCOME_TEXT
	
--	FROM TB_ITEM_OUTCOME tio

--	inner join TB_ITEM_SET on tb_item_set.ITEM_SET_ID = tio.ITEM_SET_ID
--		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
--	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tb_item_set.ITEM_ID
--	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tb_item_set.ITEM_SET_ID
--	inner join TB_ITEM on TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
	
--	LEFT OUTER JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
--		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID
		
--	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_INTERVENTION 
--	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_CONTROL
--	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_OUTCOME
	
--	WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--	AND (@ITEM_ATTRIBUTE_ID_INTERVENTION = 0 OR (ITEM_ATTRIBUTE_ID_INTERVENTION = @ITEM_ATTRIBUTE_ID_INTERVENTION))
--	AND (@ITEM_ATTRIBUTE_ID_CONTROL = 0 OR (ITEM_ATTRIBUTE_ID_CONTROL = @ITEM_ATTRIBUTE_ID_CONTROL))
--	AND (@ITEM_ATTRIBUTE_ID_OUTCOME = 0 OR (ITEM_ATTRIBUTE_ID_OUTCOME = @ITEM_ATTRIBUTE_ID_OUTCOME))
--	--	AND (@ATTRIBUTE_ID IS NULL OR (TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID))
--	--AND (
--	--	@ATTRIBUTE_ID IS NULL OR 
--	--		(
--	--		TB_ITEM_SET.ITEM_ID IN
--	--			( 
--	--			SELECT IA2.ITEM_ID FROM TB_ITEM_ATTRIBUTE IA2 
--	--			INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
--	--			WHERE IA2.ATTRIBUTE_ID = @ATTRIBUTE_ID
--	--			)
--	--		)
--	--	)
--	--AND (--temp correction for before publishing: @ATTRIBUTE_ID is (because of bug) actually the item_attribute_id
--	--	@ATTRIBUTE_ID = 0 OR 
--	--		(
--	--		tio.OUTCOME_ID IN
--	--			( 
--	--				select tio2.OUTCOME_ID from TB_ATTRIBUTE_SET tas
--	--				inner join TB_ITEM_OUTCOME_ATTRIBUTE ioa on tas.ATTRIBUTE_ID = ioa.ATTRIBUTE_ID and tas.ATTRIBUTE_SET_ID = @ATTRIBUTE_ID
--	--				inner join TB_ITEM_OUTCOME tio2 on ioa.OUTCOME_ID = tio2.OUTCOME_ID
--	--			)
--	--		)
--	--	)--end of temp correction
--	AND (--real correction to use when bug is corrected in line 174 of dialogMetaAnalysisSetup.xaml.cs
--		@ATTRIBUTE_ID = 0 OR 
--			(
--			tio.OUTCOME_ID IN 
--				( 
--					select tio2.OUTCOME_ID from TB_ITEM_OUTCOME_ATTRIBUTE ioa  
--					inner join TB_ITEM_OUTCOME tio2 on ioa.OUTCOME_ID = tio2.OUTCOME_ID and ioa.ATTRIBUTE_ID = @ATTRIBUTE_ID
--				)
--			)
--		)--end of real correction
--	AND (@SET_ID = 0 OR (TB_ITEM_SET.SET_ID = @SET_ID))
	
--	--order by TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
--*/
SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_OutcomeList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_OutcomeSingle]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_OutcomeSingle]
(
	@OUTCOME_ID INT
)

As

SET NOCOUNT ON

SELECT OUTCOME_ID, SHORT_TITLE, TB_ITEM_OUTCOME.ITEM_SET_ID, TB_ITEM_OUTCOME.OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, OUTCOME_DESCRIPTION,
	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
	A1.ATTRIBUTE_NAME AS INTERVENTION_TEXT,
	a2.ATTRIBUTE_NAME AS CONTROL_TEXT,
	a3.ATTRIBUTE_NAME AS OUTCOME_TEXT,
	0 as META_ANALYSIS_OUTCOME_ID, -- Meta-analysis id. 0 as not selected
	OUTCOME_TYPE_NAME
FROM TB_ITEM_OUTCOME
INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_OUTCOME.ITEM_SET_ID
INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
INNER JOIN TB_OUTCOME_TYPE ON TB_OUTCOME_TYPE.OUTCOME_TYPE_ID = TB_ITEM_OUTCOME.OUTCOME_TYPE_ID
left outer JOIN TB_ATTRIBUTE IA1 ON IA1.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_INTERVENTION
left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = IA1.ATTRIBUTE_ID 
left outer JOIN TB_ATTRIBUTE IA2 ON IA2.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_CONTROL
left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
left outer JOIN TB_ATTRIBUTE IA3 ON IA3.ATTRIBUTE_ID = TB_ITEM_OUTCOME.ITEM_ATTRIBUTE_ID_OUTCOME
left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = IA3.ATTRIBUTE_ID 

WHERE TB_ITEM_OUTCOME.OUTCOME_ID = @OUTCOME_ID


SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_OutcomeSingle] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_RandomAllocate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[st_RandomAllocate] (
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@FILTER_TYPE NVARCHAR(255),
	@ATTRIBUTE_ID_FILTER BIGINT,
	@SET_ID_FILTER INT,
	@ATTRIBUTE_ID BIGINT,
	@SET_ID INT,
	@HOW_MANY INT,
	@SAMPLE_NO INT,
	@INCLUDED BIT
)
AS
SET NOCOUNT ON

	-- FIRST, GET A LIST OF ALL THE ITEM_IDs THAT WE'RE WORKING WITH BY USING THE VARIOUS FILTER OPTIONS
	declare @ItemIds TABLE (idx BIGINT Primary Key)
	
	-- NO CODE / CODE SET FILTER
	IF (@FILTER_TYPE = 'No code / code set filter')
	BEGIN
	INSERT INTO @ItemIds(idx)
		SELECT TOP (@SAMPLE_NO) PERCENT ITEM_ID FROM TB_ITEM_REVIEW
			WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
			ORDER BY NEWID()
	END
	
	-- FILTER BY ALL WITH THIS ATTRIBUTE
	IF (@FILTER_TYPE = 'All with this code')
	BEGIN
	INSERT INTO @ItemIds(idx)
		SELECT TOP (@SAMPLE_NO) PERCENT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
				AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			ORDER BY NEWID()
	END
		
	-- FILTER BY ALL WITHOUT THIS ATTRIBUTE
	IF (@FILTER_TYPE = 'All without this code')
	BEGIN
	INSERT INTO @ItemIds(idx)
		SELECT TOP (@SAMPLE_NO) PERCENT ITEM_ID FROM TB_ITEM_REVIEW
			WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
				AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
				AND NOT ITEM_ID IN
				(
					SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
					INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
					INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
					WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
				)
		ORDER BY NEWID()
	END
	
	-- FILTER BY 'ALL WITHOUT ANY CODES FROM THIS SET'
	IF (@FILTER_TYPE = 'All without any codes from this set')
	BEGIN
	INSERT INTO @ItemIds(idx)
		SELECT TOP (@SAMPLE_NO) PERCENT ITEM_ID FROM TB_ITEM_REVIEW
			WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
			AND NOT ITEM_ID IN
			(
			SELECT TB_ITEM_SET.ITEM_ID FROM TB_ITEM_SET
				INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_SET.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				WHERE TB_ITEM_SET.SET_ID = @SET_ID_FILTER AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			)
		ORDER BY NEWID()
	END
	
	-- FILTER BY 'ALL WITH ANY CODES FROM THIS SET'
	IF (@FILTER_TYPE = 'All with any codes from this set')
	BEGIN
	INSERT INTO @ItemIds(idx)
		SELECT TOP (@SAMPLE_NO) PERCENT TB_ITEM_REVIEW.ITEM_ID FROM TB_ITEM_REVIEW
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
				AND TB_ITEM_SET.SET_ID = @SET_ID_FILTER
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		ORDER BY NEWID()
	END
	
	-- NOW WE HAVE OUR LIST, CHECK THAT THERE ARE ENOUGH ITEM IDs IN IT
	DECLARE @CHECK_COUNT INT
	SELECT @CHECK_COUNT = COUNT(*) FROM @ItemIds

	IF (@CHECK_COUNT > @HOW_MANY)
	BEGIN
		-- ATTRIBUTE_IDs FOR OUR NEW ATTRIBUTES
		DECLARE @GROUP1 BIGINT
		DECLARE @GROUP2 BIGINT
		DECLARE @GROUP3 BIGINT
		DECLARE @GROUP4 BIGINT
		DECLARE @GROUP5 BIGINT
		DECLARE @GROUP6 BIGINT
		DECLARE @GROUP7 BIGINT
		DECLARE @GROUP8 BIGINT
		DECLARE @GROUP9 BIGINT
		DECLARE @GROUP10 BIGINT
		DECLARE @DUMMY_OUTPUT BIGINT -- WE'RE NOT INTERESTED IN THIS VALUE
		
		DECLARE @MAX_INDEX INT = 0
		
		SELECT @MAX_INDEX = MAX(ATTRIBUTE_ORDER) + 1 FROM TB_ATTRIBUTE_SET WHERE PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND SET_ID = @SET_ID
		IF (@MAX_INDEX IS NULL) SET @MAX_INDEX = 0
		
		EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 1', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP1 OUTPUT
		SET @MAX_INDEX = @MAX_INDEX + 1
		
		IF (@HOW_MANY > 1)
		BEGIN
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 2', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP2 OUTPUT
		END
		
		IF (@HOW_MANY > 2)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 3', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP3 OUTPUT
		END
		IF (@HOW_MANY > 3)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 4', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP4 OUTPUT
		END
		IF (@HOW_MANY > 4)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 5', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP5 OUTPUT
		END
		IF (@HOW_MANY > 5)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 6', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP6 OUTPUT
		END
		IF (@HOW_MANY > 6)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 7', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP7 OUTPUT
		END
		IF (@HOW_MANY > 7)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 8', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP8 OUTPUT
		END
		IF (@HOW_MANY > 8)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 9', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP9 OUTPUT
		END
		IF (@HOW_MANY > 9)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 10', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP10 OUTPUT
		END
		
		-- NOW WE DO THE ACTUAL INPUTTING OF VALUES
		-- FIRST, WE HAVE TO CREATE ITEM_SET RECORDS FOR ALL OF THE ITEMS
		
		INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID) 
			SELECT idx, @SET_ID, 'True', @CONTACT_ID FROM @ItemIds ids
			EXCEPT
			SELECT ITEM_ID, @SET_ID, 'True', @CONTACT_ID FROM TB_ITEM_SET WHERE TB_ITEM_SET.SET_ID = @SET_ID AND IS_COMPLETED = 'True'

		
		--INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID)
		--SELECT idx, @SET_ID, 'True', @CONTACT_ID FROM @ItemIds ids
		--	WHERE NOT idx IN 
		--	(SELECT ITEM_ID FROM TB_ITEM_SET WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.CONTACT_ID = @CONTACT_ID)
	
		-- NOW WE HAVE ITEM_SET RECORDS FOR EVERYTHING, WE ASSIGN THE ITEMS TO A GIVEN ATTRIBUTE AT RANDOM
		
		IF (@HOW_MANY = 1)
		BEGIN
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				ORDER BY NEWID()
		END
		
		IF (@HOW_MANY = 2)
		BEGIN
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (50) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1)
		END
		
		IF (@HOW_MANY = 3)
		BEGIN
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (33.333) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				ORDER BY NEWID()
		
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (50) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2)
		END
		
		IF (@HOW_MANY = 4)
		BEGIN
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (25) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				ORDER BY NEWID()
		
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (33.333) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (50) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 or ATTRIBUTE_ID = @GROUP2)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3)
				
		END
		
		IF (@HOW_MANY = 5)
		BEGIN
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (20) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				ORDER BY NEWID()
		
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (25) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (33.333) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 or ATTRIBUTE_ID = @GROUP2)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (50) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 or ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP5 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR ATTRIBUTE_ID = @GROUP4)
		END
		
		IF (@HOW_MANY = 6)
		BEGIN
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (16.666) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				ORDER BY NEWID()
		
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (20) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (25) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 or ATTRIBUTE_ID = @GROUP2)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (33.333) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 or ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (50) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP5 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR ATTRIBUTE_ID = @GROUP4)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP6 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5)
		END
		
		IF (@HOW_MANY = 7)
		BEGIN
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (14.2857) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				ORDER BY NEWID()
		
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (16.666) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (20) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 or ATTRIBUTE_ID = @GROUP2)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (25) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 or ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (33.333) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP5 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR ATTRIBUTE_ID = @GROUP4)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (50) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP6 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP7 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5 OR ATTRIBUTE_ID = @GROUP6)
		END
		
		IF (@HOW_MANY = 8)
		BEGIN
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (12.5) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				ORDER BY NEWID()
		
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (14.2857) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (16.666) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 or ATTRIBUTE_ID = @GROUP2)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (20) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 or ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (25) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP5 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR ATTRIBUTE_ID = @GROUP4)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (33.333) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP6 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (50) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP7 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5 OR ATTRIBUTE_ID = @GROUP6)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP8 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5 OR ATTRIBUTE_ID = @GROUP6
								OR ATTRIBUTE_ID = @GROUP7)
		END
		
		IF (@HOW_MANY = 9)
		BEGIN
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (11.111) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				ORDER BY NEWID()
		
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (12.5) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (14.2857) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 or ATTRIBUTE_ID = @GROUP2)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (16.666) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 or ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (20) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP5 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR ATTRIBUTE_ID = @GROUP4)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (25) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP6 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (33.333) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP7 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5 OR ATTRIBUTE_ID = @GROUP6)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (50) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP8 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5 OR ATTRIBUTE_ID = @GROUP6
								OR ATTRIBUTE_ID = @GROUP7)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP9 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5 OR ATTRIBUTE_ID = @GROUP6
								OR ATTRIBUTE_ID = @GROUP7 OR ATTRIBUTE_ID = @GROUP8)
		END
		
		IF (@HOW_MANY = 10)
		BEGIN
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (10) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				ORDER BY NEWID()
		
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (11.111) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (12.5) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 or ATTRIBUTE_ID = @GROUP2)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (14.2857) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 or ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3)
				ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (16.666) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP5 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR ATTRIBUTE_ID = @GROUP4)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (20) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP6 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (25) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP7 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5 OR ATTRIBUTE_ID = @GROUP6)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (33.333) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP8 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5 OR ATTRIBUTE_ID = @GROUP6
								OR ATTRIBUTE_ID = @GROUP7)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT TOP (50) PERCENT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP9 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5 OR ATTRIBUTE_ID = @GROUP6
								OR ATTRIBUTE_ID = @GROUP7 OR ATTRIBUTE_ID = @GROUP8)
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP10 FROM @ItemIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				AND NOT idx IN (SELECT ITEM_ID FROM TB_ITEM_ATTRIBUTE WHERE 
								ATTRIBUTE_ID = @GROUP1 OR ATTRIBUTE_ID = @GROUP2 or ATTRIBUTE_ID = @GROUP3 OR 
								ATTRIBUTE_ID = @GROUP4 OR ATTRIBUTE_ID = @GROUP5 OR ATTRIBUTE_ID = @GROUP6
								OR ATTRIBUTE_ID = @GROUP7 OR ATTRIBUTE_ID = @GROUP8 OR ATTRIBUTE_ID = @GROUP9)
		END
		
	END

SET NOCOUNT OFF
RETURN

GO
ALTER AUTHORIZATION ON [dbo].[st_RandomAllocate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReportColumnCodeInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[st_ReportColumnCodeInsert]
(
	@REPORT_ID INT,
	@REPORT_COLUMN_ID INT,
	@CODE_ORDER INT,
	@SET_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@PARENT_ATTRIBUTE_ID BIGINT,
	@PARENT_ATTRIBUTE_TEXT NVARCHAR(255),
	@USER_DEF_TEXT NVARCHAR(255),
	@DISPLAY_CODE BIT,
	@DISPLAY_ADDITIONAL_TEXT BIT,
	@DISPLAY_CODED_TEXT BIT
)

As

SET NOCOUNT ON

	INSERT INTO TB_REPORT_COLUMN_CODE(REPORT_ID, REPORT_COLUMN_ID, CODE_ORDER, SET_ID, ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID,
		PARENT_ATTRIBUTE_TEXT, USER_DEF_TEXT, DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT)
	VALUES (@REPORT_ID, @REPORT_COLUMN_ID, @CODE_ORDER, @SET_ID, @ATTRIBUTE_ID, @PARENT_ATTRIBUTE_ID,
		@PARENT_ATTRIBUTE_TEXT, @USER_DEF_TEXT, @DISPLAY_CODE, @DISPLAY_ADDITIONAL_TEXT, @DISPLAY_CODED_TEXT)
	
SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReportColumnCodeInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReportColumnCodeList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReportColumnCodeList]
(
	@REPORT_COLUMN_ID INT
)

As

SET NOCOUNT ON

	SELECT * FROM TB_REPORT_COLUMN_CODE
		WHERE REPORT_COLUMN_ID = @REPORT_COLUMN_ID
		ORDER BY CODE_ORDER

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReportColumnCodeList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReportColumnDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[st_ReportColumnDelete]
(
	@REPORT_ID INT
)

As

SET NOCOUNT ON

	DELETE FROM TB_REPORT_COLUMN_CODE WHERE REPORT_ID = @REPORT_ID
	DELETE FROM TB_REPORT_COLUMN WHERE REPORT_ID = @REPORT_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReportColumnDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReportColumnInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[st_ReportColumnInsert]
(
	@REPORT_COLUMN_NAME NVARCHAR(255),
	@REPORT_ID INT,
	@COLUMN_ORDER INT,
	@NEW_REPORT_COLUMN_ID INT OUTPUT
)

As

SET NOCOUNT ON

	INSERT INTO TB_REPORT_COLUMN(REPORT_COLUMN_NAME, COLUMN_ORDER, REPORT_ID)
	VALUES(@REPORT_COLUMN_NAME, @COLUMN_ORDER, @REPORT_ID)
	
	SET @NEW_REPORT_COLUMN_ID = @@IDENTITY

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReportColumnInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReportColumnList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReportColumnList]
(
	@REPORT_ID INT
)

As

SET NOCOUNT ON

	SELECT * FROM TB_REPORT_COLUMN
		WHERE REPORT_ID = @REPORT_ID
		ORDER BY COLUMN_ORDER

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReportColumnList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReportData]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ReportData]
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_IDS NVARCHAR(MAX)
,	@REPORT_ID INT
,	@ORDER_BY NVARCHAR(15)
,	@ATTRIBUTE_ID BIGINT
,	@IS_QUESTION bit
,	@FULL_DETAILS bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @TT TABLE
	(
	  ITEM_ID BIGINT primary key
	)
	IF @ATTRIBUTE_ID != 0
	BEGIN
		INSERT INTO @TT
			SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
				AND TB_ITEM_REVIEW.IS_DELETED = 0
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
	END
	ELSE
	BEGIN
		INSERT INTO @TT
			SELECT VALUE FROM dbo.fn_Split_int(@ITEM_IDS, ',')
	END
    --First: the main report properties
	SELECT * from TB_REPORT where REPORT_ID = @REPORT_ID
	--Second: list of report columns
	SELECT * from TB_REPORT_COLUMN where REPORT_ID = @REPORT_ID ORDER BY COLUMN_ORDER
	--Third: what goes into each column, AKA "Rows" (In C# side)
	SELECT * from TB_REPORT_COLUMN_CODE  
		where REPORT_ID = @REPORT_ID ORDER BY CODE_ORDER
	
	
	--Fourth: most of the real data
	SELECT distinct cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID,cc.USER_DEF_TEXT
				,a.*, ia.*, i.ITEM_ID, i.OLD_ITEM_ID, i.SHORT_TITLE, CODE_ORDER, ATTRIBUTE_ORDER 
	from TB_REPORT_COLUMN_CODE cc
	INNER JOIN TB_ATTRIBUTE_SET tas ON (--Question reports fetch data about a given code children
										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
										OR 
										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
									   )
		AND tas.SET_ID = cc.SET_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	where REPORT_ID = @REPORT_ID 
	ORDER BY 
		i.SHORT_TITLE -- we ignore the sorting order required for this report, sorting is done on c# side.
		, i.ITEM_ID, CODE_ORDER, ATTRIBUTE_ORDER
	
	
	--Fift: data about coded TXT, uses "UNION" to grab data from TXT and PDF tables
	SELECT cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID, a.ATTRIBUTE_ID, tt.ITEM_ID, id.DOCUMENT_TITLE
	, 'Page ' + CONVERT(varchar(10),PAGE) + ':' + CHAR(10) + '[¬s]"' + replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' CODED_TEXT
	  from TB_REPORT_COLUMN_CODE cc
	INNER JOIN TB_ATTRIBUTE_SET tas ON (
										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
										OR 
										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
									   )
		AND tas.SET_ID = cc.SET_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	inner join TB_ITEM_DOCUMENT id on ia.ITEM_ID = id.ITEM_ID
	inner join TB_ITEM_ATTRIBUTE_PDF pdf on id.ITEM_DOCUMENT_ID = pdf.ITEM_DOCUMENT_ID and ia.ITEM_ATTRIBUTE_ID = pdf.ITEM_ATTRIBUTE_ID
	UNION
	SELECT cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID, a.ATTRIBUTE_ID, tt.ITEM_ID, id.DOCUMENT_TITLE
	, SUBSTRING(
					replace(id.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT
	  from TB_REPORT_COLUMN_CODE cc
	INNER JOIN TB_ATTRIBUTE_SET tas ON (
										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
										OR 
										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
									   )
		AND tas.SET_ID = cc.SET_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	inner join TB_ITEM_DOCUMENT id on ia.ITEM_ID = id.ITEM_ID
	inner join TB_ITEM_ATTRIBUTE_TEXT txt on id.ITEM_DOCUMENT_ID = txt.ITEM_DOCUMENT_ID and ia.ITEM_ATTRIBUTE_ID = txt.ITEM_ATTRIBUTE_ID
	
	--sixth, items that do not have anything to report
	
	SELECT i.ITEM_ID, i.OLD_ITEM_ID, i.SHORT_TITLE from TB_ITEM i
	inner join @TT t on t.ITEM_ID = i.ITEM_ID
	where t.ITEM_ID not in
	(SELECT distinct tt.ITEM_ID
	from TB_REPORT_COLUMN_CODE cc
	INNER JOIN TB_ATTRIBUTE_SET tas ON (--Question reports fetch data about a given code children
										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
										OR 
										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
									   )
		AND tas.SET_ID = cc.SET_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	where REPORT_ID = @REPORT_ID)
	
	
	ORDER BY 
		i.SHORT_TITLE -- we ignore the sorting order required for this report, sorting is done on c# side.
		, i.ITEM_ID
	--optional Seventh: get Title, Abstract and Year, only if some of this is needed.
	if (@FULL_DETAILS = 1)
	BEGIN
		select i.ITEM_ID, TITLE, ABSTRACT, [YEAR] from TB_ITEM i
			inner join @TT t on t.ITEM_ID = i.ITEM_ID
	END
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ReportData] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReportDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[st_ReportDelete]
(
	@REPORT_ID INT
)

As

SET NOCOUNT ON

	DELETE FROM TB_REPORT_COLUMN_CODE WHERE REPORT_ID = @REPORT_ID
	DELETE FROM TB_REPORT_COLUMN WHERE REPORT_ID = @REPORT_ID
	DELETE FROM TB_REPORT WHERE REPORT_ID = @REPORT_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReportDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReportExecute]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[st_ReportExecute]
(
	@REVIEW_ID INT
,	@ITEM_IDS NVARCHAR(MAX)
,	@REPORT_ID INT
,	@ORDER_BY NVARCHAR(15)
,	@ATTRIBUTE_ID BIGINT
,	@SET_ID INT

)
AS
SET NOCOUNT ON

DECLARE @TT TABLE
	(
	  ITEM_ID BIGINT
	)

-- FIRST GET THE LIST OF ITEM_IDs THAT WE'RE USING INTO THE TEMPORARY TABLE: THEY CAN EITHER BE IN
-- THE @ITEM_IDS VARIABLE, OR THE RESULT OF A SEARCH ON THE @ATTRIBUTE_SET_ID

IF @ATTRIBUTE_ID != 0
BEGIN
	INSERT INTO @TT
		SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.IS_DELETED != 'TRUE' 
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
END
ELSE
BEGIN
	INSERT INTO @TT
		SELECT VALUE FROM dbo.fn_Split_int(@ITEM_IDS, ',')
END

-- GET THE NAMES OF THE COLUMNS AS THE FIRST RESULT FROM THE READER 
SELECT * FROM TB_REPORT_COLUMN WHERE REPORT_ID = @REPORT_ID
ORDER BY COLUMN_ORDER

-- 2ND RESULT FROM READER = THE DATA
IF (@ORDER_BY LIKE 'Short title')
BEGIN
	select TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, ATTRIBUTE_NAME, ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
		SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT
 	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
		AND TB_ATTRIBUTE_SET.SET_ID = TB_REPORT_COLUMN_CODE.SET_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		AND TB_ITEM_REVIEW.IS_DELETED != 'TRUE'
	--INNER JOIN dbo.fn_Split_int(@ITEM_IDS, ',') attribute_list ON attribute_list.value = TB_ITEM_ATTRIBUTE.ITEM_ID
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, ATTRIBUTE_ORDER
	option (optimize for unknown)
END
ELSE
IF (@ORDER_BY LIKE 'Item Id')
BEGIN
	select TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, ATTRIBUTE_NAME, ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT
	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
		AND TB_ATTRIBUTE_SET.SET_ID = TB_REPORT_COLUMN_CODE.SET_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		AND TB_ITEM_REVIEW.IS_DELETED != 'TRUE'
	--INNER JOIN dbo.fn_Split_int(@ITEM_IDS, ',') attribute_list ON attribute_list.value = TB_ITEM_ATTRIBUTE.ITEM_ID
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	ORDER BY TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, ATTRIBUTE_ORDER
	option (optimize for unknown)
END
ELSE
BEGIN
	select TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, ATTRIBUTE_NAME, ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT
	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
		AND TB_ATTRIBUTE_SET.SET_ID = TB_REPORT_COLUMN_CODE.SET_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		AND TB_ITEM_REVIEW.IS_DELETED != 'TRUE'
	--INNER JOIN dbo.fn_Split_int(@ITEM_IDS, ',') attribute_list ON attribute_list.value = TB_ITEM_ATTRIBUTE.ITEM_ID
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)	
	ORDER BY TB_ITEM.OLD_ITEM_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, ATTRIBUTE_ORDER
	option (optimize for unknown)
END


SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReportExecute] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReportExecuteSingleWithOutcomes]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[st_ReportExecuteSingleWithOutcomes]
(
	@REVIEW_ID INT
,	@ITEM_IDS NVARCHAR(MAX)
,	@REPORT_ID INT
,	@ORDER_BY NVARCHAR(15)
,	@ATTRIBUTE_ID BIGINT
,	@SET_ID INT

)
AS
SET NOCOUNT ON

DECLARE @TT TABLE
	(
	  ITEM_ID BIGINT
	)

-- FIRST GET THE LIST OF ITEM_IDs THAT WE'RE USING INTO THE TEMPORARY TABLE: THEY CAN EITHER BE IN
-- THE @ITEM_IDS VARIABLE, OR THE RESULT OF A SEARCH ON THE @ATTRIBUTE_SET_ID

IF @ATTRIBUTE_ID != 0
BEGIN
	INSERT INTO @TT
		SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.IS_DELETED != 'TRUE'
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
END
ELSE
BEGIN
	INSERT INTO @TT
		SELECT VALUE FROM dbo.fn_Split_int(@ITEM_IDS, ',')
END

-- GET THE NAMES OF THE COLUMNS AS THE FIRST RESULT FROM THE READER 
SELECT * FROM TB_REPORT_COLUMN WHERE REPORT_ID = @REPORT_ID
ORDER BY COLUMN_ORDER

-- 2ND RESULT: THE LIST OF ATTRIBUTES THAT HAVE BEEN APPLIED TO OUTCOMES IN THE MAIN DATA
SELECT DISTINCT ATTRIBUTE_NAME FROM TB_ATTRIBUTE
	INNER JOIN TB_ITEM_OUTCOME_ATTRIBUTE ON TB_ITEM_OUTCOME_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_OUTCOME ON TB_ITEM_OUTCOME.OUTCOME_ID = TB_ITEM_OUTCOME_ATTRIBUTE.OUTCOME_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_OUTCOME.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	WHERE TB_ITEM_SET.ITEM_ID IN (SELECT ITEM_ID FROM @TT)

-- 3RD RESULT FROM READER = THE DATA
IF (@ORDER_BY LIKE 'Short title')
BEGIN
	select distinct TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT,
		TB_ITEM_OUTCOME.OUTCOME_TITLE, AT2.ATTRIBUTE_NAME OUTCOME_ATTRIBUTE, TB_ITEM_OUTCOME.OUTCOME_ID, CODE_ORDER

	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	
	INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_ID = TB_ITEM_SET.ITEM_ID
		AND IS2.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM_OUTCOME ON TB_ITEM_OUTCOME.ITEM_SET_ID = IS2.ITEM_SET_ID
	LEFT OUTER JOIN TB_ITEM_OUTCOME_ATTRIBUTE ON TB_ITEM_OUTCOME_ATTRIBUTE.OUTCOME_ID = TB_ITEM_OUTCOME.OUTCOME_ID
	LEFT OUTER JOIN TB_ATTRIBUTE AT2 ON AT2.ATTRIBUTE_ID = TB_ITEM_OUTCOME_ATTRIBUTE.ATTRIBUTE_ID
	
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	--ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, OUTCOME_TITLE
	ORDER BY SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, OUTCOME_ID
END

ELSE
IF (@ORDER_BY LIKE 'Item Id')
BEGIN
	select distinct TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT,
		TB_ITEM_OUTCOME.OUTCOME_TITLE, AT2.ATTRIBUTE_NAME OUTCOME_ATTRIBUTE, TB_ITEM_OUTCOME.OUTCOME_ID, CODE_ORDER

	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	
	INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_ID = TB_ITEM_SET.ITEM_ID
		AND IS2.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM_OUTCOME ON TB_ITEM_OUTCOME.ITEM_SET_ID = IS2.ITEM_SET_ID
	LEFT OUTER JOIN TB_ITEM_OUTCOME_ATTRIBUTE ON TB_ITEM_OUTCOME_ATTRIBUTE.OUTCOME_ID = TB_ITEM_OUTCOME.OUTCOME_ID
	LEFT OUTER JOIN TB_ATTRIBUTE AT2 ON AT2.ATTRIBUTE_ID = TB_ITEM_OUTCOME_ATTRIBUTE.ATTRIBUTE_ID
	
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	--ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, OUTCOME_TITLE
	ORDER BY TB_ITEM_ATTRIBUTE.ITEM_ID, OUTCOME_ID
END
ELSE
BEGIN
	select distinct TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT,
		TB_ITEM_OUTCOME.OUTCOME_TITLE, AT2.ATTRIBUTE_NAME OUTCOME_ATTRIBUTE, TB_ITEM_OUTCOME.OUTCOME_ID, CODE_ORDER

	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	
	INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_ID = TB_ITEM_SET.ITEM_ID
		AND IS2.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM_OUTCOME ON TB_ITEM_OUTCOME.ITEM_SET_ID = IS2.ITEM_SET_ID
	LEFT OUTER JOIN TB_ITEM_OUTCOME_ATTRIBUTE ON TB_ITEM_OUTCOME_ATTRIBUTE.OUTCOME_ID = TB_ITEM_OUTCOME.OUTCOME_ID
	LEFT OUTER JOIN TB_ATTRIBUTE AT2 ON AT2.ATTRIBUTE_ID = TB_ITEM_OUTCOME_ATTRIBUTE.ATTRIBUTE_ID
	
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	--ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, OUTCOME_TITLE
	ORDER BY OLD_ITEM_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, OUTCOME_ID
END


SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReportExecuteSingleWithOutcomes] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReportExecuteSingleWithoutOutcomes]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[st_ReportExecuteSingleWithoutOutcomes]
(
	@REVIEW_ID INT
,	@ITEM_IDS NVARCHAR(MAX)
,	@REPORT_ID INT
,	@ORDER_BY NVARCHAR(15)
,	@ATTRIBUTE_ID BIGINT
,	@SET_ID INT

)
AS
SET NOCOUNT ON

DECLARE @TT TABLE
	(
	  ITEM_ID BIGINT
	)

-- FIRST GET THE LIST OF ITEM_IDs THAT WE'RE USING INTO THE TEMPORARY TABLE: THEY CAN EITHER BE IN
-- THE @ITEM_IDS VARIABLE, OR THE RESULT OF A SEARCH ON THE @ATTRIBUTE_SET_ID

IF @ATTRIBUTE_ID != 0
BEGIN
	INSERT INTO @TT
		SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.IS_DELETED != 'TRUE'
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
END
ELSE
BEGIN
	INSERT INTO @TT
		SELECT VALUE FROM dbo.fn_Split_int(@ITEM_IDS, ',')
END

-- GET THE NAMES OF THE COLUMNS AS THE FIRST RESULT FROM THE READER 
SELECT * FROM TB_REPORT_COLUMN WHERE REPORT_ID = @REPORT_ID
ORDER BY COLUMN_ORDER

-- 2nd RESULT FROM READER = THE DATA
IF (@ORDER_BY LIKE 'Short title')
BEGIN
	select distinct TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT, CODE_ORDER

	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	--ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, OUTCOME_TITLE
	ORDER BY SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID
END

ELSE
IF (@ORDER_BY LIKE 'Item Id')
BEGIN
	select distinct TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT, CODE_ORDER

	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	--ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, OUTCOME_TITLE
	ORDER BY TB_ITEM_ATTRIBUTE.ITEM_ID
END
ELSE
BEGIN
	select distinct TB_ITEM_ATTRIBUTE.ITEM_ID, OLD_ITEM_ID, SHORT_TITLE, TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID,
		REPORT_COLUMN_CODE_ID, COLUMN_ORDER, USER_DEF_TEXT, TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT,
		DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT, REPORT_COLUMN_NAME,
				SUBSTRING(
					replace(TB_ITEM_DOCUMENT.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT, CODE_ORDER

	FROM TB_REPORT_COLUMN_CODE
	INNER JOIN TB_REPORT_COLUMN ON TB_REPORT_COLUMN.REPORT_COLUMN_ID = TB_REPORT_COLUMN_CODE.REPORT_COLUMN_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_REPORT_COLUMN_CODE.ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_ATTRIBUTE_TEXT ON TB_ITEM_ATTRIBUTE_TEXT.ITEM_ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
	LEFT OUTER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_DOCUMENT_ID = TB_ITEM_ATTRIBUTE_TEXT.ITEM_DOCUMENT_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	
	WHERE TB_REPORT_COLUMN_CODE.REPORT_ID = @REPORT_ID
		AND TB_ITEM_ATTRIBUTE.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	--ORDER BY TB_ITEM.SHORT_TITLE, TB_ITEM_ATTRIBUTE.ITEM_ID, COLUMN_ORDER, CODE_ORDER, OUTCOME_TITLE
	ORDER BY OLD_ITEM_ID, TB_ITEM_ATTRIBUTE.ITEM_ID
END


SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReportExecuteSingleWithoutOutcomes] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReportInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReportInsert]
(
	@REPORT_NAME NVARCHAR(255),
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@REPORT_TYPE NVARCHAR(10),
	@NEW_REPORT_ID INT OUTPUT
)

As

SET NOCOUNT ON

	INSERT INTO TB_REPORT(NAME, REVIEW_ID, REPORT_TYPE, CONTACT_ID)
	VALUES (@REPORT_NAME, @REVIEW_ID, @REPORT_TYPE, @CONTACT_ID)
	
	SET @NEW_REPORT_ID = @@IDENTITY

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReportInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReportList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReportList]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	SELECT REPORT_ID, REVIEW_ID, NAME, REPORT_TYPE, TB_REPORT.CONTACT_ID, CONTACT_NAME FROM TB_REPORT
		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_REPORT.CONTACT_ID
		WHERE REVIEW_ID = @REVIEW_ID
		ORDER BY CONTACT_ID, NAME

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReportList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReportUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReportUpdate]
(
	@REPORT_NAME NVARCHAR(255),
	@REPORT_TYPE NVARCHAR(10),
	@REPORT_ID INT OUTPUT
)

As

SET NOCOUNT ON

	UPDATE TB_REPORT SET NAME = @REPORT_NAME, REPORT_TYPE = @REPORT_TYPE
	WHERE REPORT_ID = @REPORT_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReportUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewContact]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewContact]
(
	@CONTACT_ID INT
)

As

SELECT REVIEW_CONTACT_ID, rc.REVIEW_ID, rc.CONTACT_ID, REVIEW_NAME, dbo.fn_REBUILD_ROLES(rc.REVIEW_CONTACT_ID) as ROLES
	,own.CONTACT_NAME as 'OWNER', case when LR is null
									then r.DATE_CREATED
									else LR
								 end
								 as 'LAST_ACCESS'
	, r.SHOW_SCREENING, r.SCREENING_CODE_SET_ID, r.SCREENING_MODE, r.SCREENING_WHAT_ATTRIBUTE_ID, r.SCREENING_N_PEOPLE
	, r.SCREENING_RECONCILLIATION, r.SCREENING_AUTO_EXCLUDE, SCREENING_MODEL_RUNNING, SCREENING_INDEXED
	, BL_ACCOUNT_CODE,BL_AUTH_CODE, BL_TX, BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX
FROM TB_REVIEW_CONTACT rc
INNER JOIN TB_REVIEW r ON rc.REVIEW_ID = r.REVIEW_ID
inner join TB_CONTACT own on r.FUNDER_ID = own.CONTACT_ID
left join (
			select MAX(LAST_RENEWED) LR, REVIEW_ID
			from ReviewerAdmin.dbo.TB_LOGON_TICKET  
			where @CONTACT_ID = CONTACT_ID
			group by REVIEW_ID
			) as t
			on t.REVIEW_ID = r.REVIEW_ID
WHERE rc.CONTACT_ID = @CONTACT_ID and (r.ARCHIE_ID is null OR r.ARCHIE_ID = 'prospective_______')
ORDER BY REVIEW_NAME


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewContact] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewContactForSiteAdmin]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ReviewContactForSiteAdmin]
(
	@CONTACT_ID INT
	,@REVIEW_ID int
)

As

SELECT 0 as REVIEW_CONTACT_ID, - r.REVIEW_ID as REVIEW_ID, rc.CONTACT_ID, REVIEW_NAME, 'AdminUser;' as ROLES
	,own.CONTACT_NAME as 'OWNER', case when LR is null
									then r.DATE_CREATED
									else LR
								 end
								 as 'LAST_ACCESS'
	, r.SHOW_SCREENING, r.SCREENING_CODE_SET_ID, r.SCREENING_MODE, r.SCREENING_WHAT_ATTRIBUTE_ID, r.SCREENING_N_PEOPLE
	, r.SCREENING_RECONCILLIATION, r.SCREENING_AUTO_EXCLUDE, SCREENING_MODEL_RUNNING, SCREENING_INDEXED
	, BL_ACCOUNT_CODE,BL_AUTH_CODE, BL_TX, BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX
FROM TB_CONTACT rc
INNER JOIN TB_REVIEW r ON rc.CONTACT_ID = @CONTACT_ID and rc.IS_SITE_ADMIN = 1 and r.REVIEW_ID = @REVIEW_ID 
inner join TB_CONTACT own on r.FUNDER_ID = own.CONTACT_ID
left join (
			select MAX(LAST_RENEWED) LR, REVIEW_ID
			from ReviewerAdmin.dbo.TB_LOGON_TICKET  
			where @CONTACT_ID = CONTACT_ID and REVIEW_ID = @REVIEW_ID
			group by REVIEW_ID
			) as t
			on t.REVIEW_ID = r.REVIEW_ID
WHERE rc.CONTACT_ID = @CONTACT_ID
ORDER BY REVIEW_NAME


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewContactForSiteAdmin] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewContactList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewContactList]
(
	@REVIEW_ID INT
)

As

/* ADD TO THIS THE RETRIEVAL OF ROLES */

SELECT TB_REVIEW_CONTACT.CONTACT_ID, REVIEW_ID, CONTACT_NAME

FROM TB_REVIEW_CONTACT

INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_REVIEW_CONTACT.CONTACT_ID

WHERE REVIEW_ID = @REVIEW_ID

GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewContactList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewInfo]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewInfo]
(
	@REVIEW_ID INT
	
)

As
BEGIN
	SELECT * FROM TB_REVIEW
	WHERE REVIEW_ID = @REVIEW_ID
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewInfo] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewInfoUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewInfoUpdate]
(
	@REVIEW_ID int
,	@SCREENING_CODE_SET_ID int
,	@SCREENING_MODE nvarchar(10)
,	@SCREENING_RECONCILLIATION nvarchar(10)
,	@SCREENING_WHAT_ATTRIBUTE_ID bigint
,	@SCREENING_N_PEOPLE int
,	@SCREENING_AUTO_EXCLUDE bit
,	@SCREENING_MODEL_RUNNING bit
,	@SCREENING_INDEXED bit
)

As

SET NOCOUNT ON

UPDATE TB_REVIEW
	SET SCREENING_CODE_SET_ID = @SCREENING_CODE_SET_ID
,		SCREENING_MODE = @SCREENING_MODE
,		SCREENING_RECONCILLIATION = @SCREENING_RECONCILLIATION
,		SCREENING_WHAT_ATTRIBUTE_ID = @SCREENING_WHAT_ATTRIBUTE_ID
,		SCREENING_N_PEOPLE = @SCREENING_N_PEOPLE
,		SCREENING_AUTO_EXCLUDE = @SCREENING_AUTO_EXCLUDE
--,		SCREENING_MODEL_RUNNING = @SCREENING_MODEL_RUNNING
,		SCREENING_INDEXED = @SCREENING_INDEXED
WHERE REVIEW_ID = @REVIEW_ID
	

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewInfoUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[st_ReviewInsert]
(
	@REVIEW_NAME NVARCHAR(255),
	@CONTACT_ID INT,
	@NEW_REVIEW_ID INT OUTPUT
)

As

SET NOCOUNT ON

	INSERT INTO TB_REVIEW(REVIEW_NAME, FUNDER_ID, DATE_CREATED)
	VALUES (@REVIEW_NAME, @CONTACT_ID, CURRENT_TIMESTAMP)

	SET @NEW_REVIEW_ID = @@IDENTITY
	
	DECLARE @NEW_CONTACT_REVIEW_ID INT
	
	INSERT INTO TB_REVIEW_CONTACT(CONTACT_ID, REVIEW_ID)
	VALUES (@CONTACT_ID, @NEW_REVIEW_ID)
	
	SET @NEW_CONTACT_REVIEW_ID = @@IDENTITY
	
	INSERT INTO TB_CONTACT_REVIEW_ROLE(REVIEW_CONTACT_ID, ROLE_NAME)
	VALUES(@NEW_CONTACT_REVIEW_ID, 'AdminUser')
	
	
	
	

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSet]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ReviewSet]
	-- Add the parameters for the stored procedure here
	(
	@REVIEW_SET_ID INT
	)

As

SET NOCOUNT ON

	SELECT REVIEW_SET_ID, REVIEW_ID, RS.SET_ID, ALLOW_CODING_EDITS, S.SET_TYPE_ID, SET_NAME, SET_TYPE, CODING_IS_FINAL, SET_ORDER, MAX_DEPTH, S.SET_DESCRIPTION, S.ORIGINAL_SET_ID
	FROM TB_REVIEW_SET RS
	INNER JOIN TB_SET S ON S.SET_ID = RS.SET_ID
	INNER JOIN TB_SET_TYPE ON TB_SET_TYPE.SET_TYPE_ID = S.SET_TYPE_ID

	WHERE RS.REVIEW_SET_ID = @REVIEW_SET_ID
	ORDER BY RS.SET_ORDER, RS.SET_ID


SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewSet] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetCheckCodingStatus]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[st_ReviewSetCheckCodingStatus]
(
	@SET_ID INT,
	@REVIEW_ID INT,
	@PROBLEMATIC_ITEM_COUNT INT OUTPUT
)

As

SET NOCOUNT ON

SELECT DISTINCT tis.ITEM_ID FROM TB_ITEM_SET tis
	inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = tis.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 0
	WHERE SET_ID = @SET_ID AND IS_COMPLETED = 'FALSE'
	
	EXCEPT
	
	SELECT ITEM_ID FROM TB_ITEM_SET WHERE TB_ITEM_SET.SET_ID = @SET_ID AND IS_COMPLETED = 'TRUE'

set @PROBLEMATIC_ITEM_COUNT = @@ROWCOUNT

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewSetCheckCodingStatus] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetControls]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewSetControls]
(
	@ITEM_SET_ID INT = NULL,
	@SET_ID INT = NULL
)

As

SET NOCOUNT ON

IF (@SET_ID = 0)
BEGIN
	SELECT DISTINCT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
	FROM TB_ITEM_SET
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
	INNER JOIN TB_ATTRIBUTE_TYPE ON TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	WHERE TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = 6 AND TB_ITEM_SET.ITEM_SET_ID = @ITEM_SET_ID
	AND dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
	ORDER BY TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ATTRIBUTE_SET.ATTRIBUTE_ORDER
END
ELSE
BEGIN
	SELECT DISTINCT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
	FROM TB_ITEM_SET
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
	INNER JOIN TB_ATTRIBUTE_TYPE ON TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	WHERE TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = 6 AND TB_ITEM_SET.SET_ID = @SET_ID
	AND dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
	ORDER BY TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ATTRIBUTE_SET.ATTRIBUTE_ORDER
END

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewSetControls] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewSetDelete]
(
	@REVIEW_SET_ID INT,
	@SET_ID INT,
	@REVIEW_ID INT,
	@SET_ORDER INT = -1 --default value added by S
)

As

SET NOCOUNT ON
-- sergio's Edit, this fails in ER4 V.4.2.1.0 because ER4 does not send the @SET_ORDER parameter
-- hence, if this parameter is missing, I'll get it from the DB
IF @SET_ORDER = -1
begin
	set @SET_ORDER = (SELECT SET_ORDER from TB_REVIEW_SET where REVIEW_SET_ID = @REVIEW_SET_ID and REVIEW_ID = @REVIEW_ID)
end
-- HACK FOR CAMPBELL!! (Also protects against huge accidental data loss)

UPDATE TB_REVIEW_SET
	SET SET_ORDER = SET_ORDER -1
	WHERE REVIEW_ID = @REVIEW_ID
	AND SET_ORDER > @SET_ORDER

DELETE FROM TB_REVIEW_SET WHERE REVIEW_SET_ID = @REVIEW_SET_ID


/*

	SELECT TB_ITEM_SET.SET_ID FROM TB_ITEM_SET 
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_SET.ITEM_ID
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
	WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_REVIEW_SET.REVIEW_SET_ID = @REVIEW_SET_ID

	IF (@@ROWCOUNT = 0)
	BEGIN

		DELETE FROM TB_REVIEW_SET WHERE REVIEW_SET_ID = @REVIEW_SET_ID

		SELECT SET_ID FROM TB_REVIEW_SET WHERE SET_ID = @SET_ID

		IF (@@ROWCOUNT = 0)
		BEGIN
			DELETE FROM TB_SET WHERE SET_ID = @SET_ID
		END

	END

*/

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewSetDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetDeleteWarning]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[st_ReviewSetDeleteWarning]
(
	@ATTRIBUTE_SET_ID BIGINT,
	@SET_ID INT,
	@NUM_ITEMS BIGINT OUTPUT,
	@REVIEW_ID INT
)

As

SET NOCOUNT ON


	SELECT @NUM_ITEMS = COUNT(DISTINCT TB_ITEM_SET.ITEM_ID) FROM TB_ITEM_SET
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_SET.ITEM_ID
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		WHERE TB_ITEM_SET.SET_ID = @SET_ID


SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewSetDeleteWarning] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewSetInsert]
(
	@REVIEW_ID INT,
	@SET_TYPE_ID INT = 3,
	@ALLOW_CODING_EDITS BIT = false,
	@SET_NAME NVARCHAR(255),
	@CODING_IS_FINAL BIT = true,
	@SET_ORDER INT = 0,
	@SET_DESCRIPTION nvarchar(2000) = '',
	@ORIGINAL_SET_ID int = null,
	@NEW_REVIEW_SET_ID INT OUTPUT,
	@NEW_SET_ID INT OUTPUT
)

As

SET NOCOUNT ON

	INSERT INTO TB_SET(SET_TYPE_ID, SET_NAME, SET_DESCRIPTION, ORIGINAL_SET_ID)
		VALUES(@SET_TYPE_ID, @SET_NAME, @SET_DESCRIPTION, @ORIGINAL_SET_ID)

	SET @NEW_SET_ID = @@IDENTITY

	INSERT INTO TB_REVIEW_SET(REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL, SET_ORDER)
		VALUES(@REVIEW_ID, @NEW_SET_ID, @ALLOW_CODING_EDITS, @CODING_IS_FINAL, @SET_ORDER)

	SET @NEW_REVIEW_SET_ID = @@IDENTITY


SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewSetInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetInterventions]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewSetInterventions]
(
	@ITEM_SET_ID INT = NULL,
	@SET_ID INT = NULL
)

As

SET NOCOUNT ON
IF (@SET_ID = 0)
BEGIN
	SELECT DISTINCT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
	FROM TB_ITEM_SET
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
	INNER JOIN TB_ATTRIBUTE_TYPE ON TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	WHERE TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = 5 AND TB_ITEM_SET.ITEM_SET_ID = @ITEM_SET_ID
	AND dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
	ORDER BY TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ATTRIBUTE_SET.ATTRIBUTE_ORDER
END
ELSE
BEGIN
	SELECT DISTINCT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
	FROM TB_ITEM_SET
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
	INNER JOIN TB_ATTRIBUTE_TYPE ON TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	WHERE TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = 5 AND TB_ITEM_SET.SET_ID = @SET_ID
	AND dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
	ORDER BY TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ATTRIBUTE_SET.ATTRIBUTE_ORDER
END

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewSetInterventions] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetOutcomes]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewSetOutcomes]
(
	@ITEM_SET_ID INT = NULL,
	@SET_ID INT = NULL
)

As

SET NOCOUNT ON
IF (@SET_ID = 0)
BEGIN
	SELECT DISTINCT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
	FROM TB_ITEM_SET
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
	INNER JOIN TB_ATTRIBUTE_TYPE ON TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	WHERE TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = 4 AND TB_ITEM_SET.ITEM_SET_ID = @ITEM_SET_ID
	AND dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
	ORDER BY TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ATTRIBUTE_SET.ATTRIBUTE_ORDER
END
ELSE
BEGIN
	SELECT DISTINCT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
	FROM TB_ITEM_SET
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
	INNER JOIN TB_ATTRIBUTE_TYPE ON TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
	WHERE TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = 4 AND TB_ITEM_SET.SET_ID = @SET_ID
	AND dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
	ORDER BY TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ATTRIBUTE_SET.ATTRIBUTE_ORDER
END

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewSetOutcomes] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSets]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewSets]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	SELECT REVIEW_SET_ID, REVIEW_ID, RS.SET_ID, ALLOW_CODING_EDITS, S.SET_TYPE_ID, SET_NAME, SET_TYPE, CODING_IS_FINAL, SET_ORDER, MAX_DEPTH, S.SET_DESCRIPTION, S.ORIGINAL_SET_ID
	FROM TB_REVIEW_SET RS
	INNER JOIN TB_SET S ON S.SET_ID = RS.SET_ID
	INNER JOIN TB_SET_TYPE ON TB_SET_TYPE.SET_TYPE_ID = S.SET_TYPE_ID

	WHERE RS.REVIEW_ID = @REVIEW_ID
	ORDER BY RS.SET_ORDER, RS.SET_ID


SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewSets] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetsForCopy]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ReviewSetsForCopy]
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int,
	@CONTACT_ID int,
	@PRIVATE_SETS bit
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	if @PRIVATE_SETS = 1
	BEGIN
		SELECT REVIEW_SET_ID, RS.REVIEW_ID, RS.SET_ID, ALLOW_CODING_EDITS, S.SET_TYPE_ID, SET_NAME, SET_TYPE, CODING_IS_FINAL, SET_ORDER, MAX_DEPTH, S.SET_DESCRIPTION, S.ORIGINAL_SET_ID
		FROM TB_REVIEW_SET RS
		INNER JOIN TB_SET S ON S.SET_ID = RS.SET_ID
		INNER JOIN TB_SET_TYPE ON TB_SET_TYPE.SET_TYPE_ID = S.SET_TYPE_ID
		INNER JOIN TB_REVIEW_CONTACT rc on RS.REVIEW_ID = rc.REVIEW_ID and rc.CONTACT_ID = @CONTACT_ID
		inner join TB_CONTACT_REVIEW_ROLE crr on rc.REVIEW_CONTACT_ID = crr.REVIEW_CONTACT_ID and ROLE_NAME = 'AdminUser'
		WHERE RS.REVIEW_ID != @REVIEW_ID
		ORDER BY RS.REVIEW_ID, RS.SET_ORDER, RS.SET_ID
	END
	ELSE
	BEGIN
		SELECT REVIEW_SET_ID, RS.REVIEW_ID, RS.SET_ID, ALLOW_CODING_EDITS, S.SET_TYPE_ID, SET_NAME, SET_TYPE, CODING_IS_FINAL, SET_ORDER, MAX_DEPTH, S.SET_DESCRIPTION, S.ORIGINAL_SET_ID
		FROM TB_REVIEW_SET RS
		INNER JOIN TB_SET S ON S.SET_ID = RS.SET_ID
		INNER JOIN TB_SET_TYPE ON TB_SET_TYPE.SET_TYPE_ID = S.SET_TYPE_ID
		inner join ReviewerAdmin.dbo.TB_MANAGEMENT_SETTINGS ms on RS.REVIEW_ID = ms.PUBLIC_CODESETS_REVIEW_ID 
		ORDER BY RS.REVIEW_ID, RS.SET_ORDER, RS.SET_ID
	END
END


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewSetsForCopy] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewSetUpdate]
(
	@REVIEW_SET_ID INT,
	@SET_ID INT,
	@ALLOW_CODING_EDITS BIT,
	@CODING_IS_FINAL BIT,
	@SET_NAME NVARCHAR(255),
	@SET_ORDER INT,
	@SET_DESCRIPTION nvarchar(2000),
	@ITEM_SET_ID BIGINT = NULL,
	@IS_COMPLETED BIT = NULL,
	@IS_LOCKED BIT = NULL
)

As

SET NOCOUNT ON

	UPDATE TB_SET SET SET_NAME = @SET_NAME, SET_DESCRIPTION = @SET_DESCRIPTION 
	 WHERE SET_ID = @SET_ID
	UPDATE TB_REVIEW_SET SET ALLOW_CODING_EDITS = @ALLOW_CODING_EDITS,
		CODING_IS_FINAL = @CODING_IS_FINAL,
		SET_ORDER = @SET_ORDER
	WHERE REVIEW_SET_ID = @REVIEW_SET_ID
	
	IF (@ITEM_SET_ID > 0)
	BEGIN
		UPDATE TB_ITEM_SET
		SET IS_COMPLETED = @IS_COMPLETED, IS_LOCKED = @IS_LOCKED
		WHERE ITEM_SET_ID = @ITEM_SET_ID
	END

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewSetUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetUpdateOrder]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewSetUpdateOrder]
(
	@REVIEW_SET_ID INT,
	@OLD_SET_ORDER INT,
	@NEW_SET_ORDER INT,
	@REVIEW_ID INT
	
)

As

SET NOCOUNT ON

	UPDATE TB_REVIEW_SET
	SET SET_ORDER = SET_ORDER -1
	WHERE REVIEW_ID = @REVIEW_ID AND SET_ORDER > @OLD_SET_ORDER
	
	UPDATE TB_REVIEW_SET
	SET SET_ORDER = SET_ORDER +1
	WHERE REVIEW_ID = @REVIEW_ID AND SET_ORDER >= @NEW_SET_ORDER
	
	UPDATE TB_REVIEW_SET
	SET SET_ORDER = @NEW_SET_ORDER
	WHERE REVIEW_SET_ID = @REVIEW_SET_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewSetUpdateOrder] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewStatisticsCodeSetsComplete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE procedure [dbo].[st_ReviewStatisticsCodeSetsComplete]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

SELECT SET_NAME, TB_ITEM_SET.SET_ID, COUNT(DISTINCT TB_ITEM_SET.ITEM_ID) AS TOTAL
FROM TB_ITEM_SET
INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.REVIEW_ID = TB_REVIEW_SET.REVIEW_ID AND TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_SET.ITEM_ID
INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND IS_DELETED = 'FALSE'
GROUP BY TB_ITEM_SET.SET_ID, SET_NAME

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewStatisticsCodeSetsComplete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewStatisticsCodeSetsIncomplete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE procedure [dbo].[st_ReviewStatisticsCodeSetsIncomplete]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

SELECT SET_NAME, SET_ID, COUNT(DISTINCT ITEM_ID) AS TOTAL
FROM 
(
	SELECT SET_NAME AS SET_NAME, IS1.SET_ID, IS1.ITEM_ID FROM 
	TB_ITEM_SET IS1
	INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = IS1.SET_ID
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.REVIEW_ID = TB_REVIEW_SET.REVIEW_ID AND TB_ITEM_REVIEW.ITEM_ID = IS1.ITEM_ID
	INNER JOIN TB_SET ON TB_SET.SET_ID = IS1.SET_ID
	WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND IS1.IS_COMPLETED = 'FALSE' AND IS_DELETED != 'TRUE'
	
	EXCEPT
	
	SELECT SET_NAME, IS2.SET_ID, IS2.ITEM_ID
	FROM TB_ITEM_SET IS2
	INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = IS2.ITEM_ID AND IR2.REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'TRUE'
	INNER JOIN TB_SET ON TB_SET.SET_ID = IS2.SET_ID
	WHERE IS2.IS_COMPLETED = 'TRUE'
) AS X

GROUP BY SET_ID, SET_NAME


/*
SELECT SET_NAME, IS1.SET_ID, COUNT(DISTINCT IS1.ITEM_ID) AS TOTAL
FROM TB_ITEM_SET IS1
INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = IS1.SET_ID
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.REVIEW_ID = TB_REVIEW_SET.REVIEW_ID AND TB_ITEM_REVIEW.ITEM_ID = IS1.ITEM_ID
INNER JOIN TB_SET ON TB_SET.SET_ID = IS1.SET_ID
WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND IS1.IS_COMPLETED = 'FALSE'
AND NOT IS1.ITEM_ID IN
(
	SELECT IS2.ITEM_ID
	FROM TB_ITEM_SET IS2
	INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = IS2.ITEM_ID AND IR2.REVIEW_ID = @REVIEW_ID
	WHERE IS2.IS_COMPLETED = 'TRUE' AND IS2.SET_ID = IS1.SET_ID
)
GROUP BY IS1.SET_ID, SET_NAME
*/

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewStatisticsCodeSetsIncomplete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewStatisticsCodeSetsReviewersComplete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE procedure [dbo].[st_ReviewStatisticsCodeSetsReviewersComplete]
(
	@REVIEW_ID INT,
	@SET_ID INT
)

As

SET NOCOUNT ON

SELECT SET_NAME, TB_ITEM_SET.SET_ID, TB_ITEM_SET.CONTACT_ID, CONTACT_NAME, COUNT(DISTINCT TB_ITEM_SET.ITEM_ID) AS TOTAL
FROM TB_ITEM_SET
INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.REVIEW_ID = TB_REVIEW_SET.REVIEW_ID AND TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_SET.ITEM_ID
INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' and IS_DELETED = 'FALSE'
	AND TB_ITEM_SET.SET_ID = @SET_ID
GROUP BY TB_ITEM_SET.SET_ID, SET_NAME, TB_ITEM_SET.CONTACT_ID, CONTACT_NAME

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewStatisticsCodeSetsReviewersComplete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewStatisticsCodeSetsReviewersIncomplete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE procedure [dbo].[st_ReviewStatisticsCodeSetsReviewersIncomplete]
(
	@REVIEW_ID INT,
	@SET_ID INT 
)
with Recompile
As

SET NOCOUNT ON

SELECT SET_NAME, IS1.SET_ID, IS1.CONTACT_ID, CONTACT_NAME, COUNT(DISTINCT IS1.ITEM_ID) AS TOTAL
FROM TB_ITEM_SET IS1
INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = IS1.SET_ID
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.REVIEW_ID = TB_REVIEW_SET.REVIEW_ID AND TB_ITEM_REVIEW.ITEM_ID = IS1.ITEM_ID
INNER JOIN TB_SET ON TB_SET.SET_ID = IS1.SET_ID
INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = IS1.CONTACT_ID
WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND IS1.IS_COMPLETED = 'FALSE' 
AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND IS1.SET_ID = @SET_ID
AND NOT IS1.ITEM_ID IN
(
	SELECT IS2.ITEM_ID
	FROM TB_ITEM_SET IS2
	INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = IS2.ITEM_ID AND IR2.REVIEW_ID = @REVIEW_ID
	WHERE IS2.IS_COMPLETED = 'TRUE' AND IS2.SET_ID = IS1.SET_ID
)
GROUP BY IS1.SET_ID, SET_NAME, IS1.CONTACT_ID, CONTACT_NAME


SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewStatisticsCodeSetsReviewersIncomplete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewStatisticsCounts]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewStatisticsCounts]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

SELECT COUNT (DISTINCT ITEM_ID) FROM TB_ITEM_REVIEW
	WHERE REVIEW_ID = @REVIEW_ID
	AND IS_INCLUDED = 'TRUE'
	AND IS_DELETED = 'FALSE'
	
SELECT COUNT (DISTINCT ITEM_ID) FROM TB_ITEM_REVIEW
	WHERE REVIEW_ID = @REVIEW_ID
	AND IS_INCLUDED = 'FALSE'
	AND IS_DELETED = 'FALSE'
	
SELECT COUNT (DISTINCT ITEM_ID) FROM TB_ITEM_REVIEW
	WHERE REVIEW_ID = @REVIEW_ID
	AND IS_DELETED = 'TRUE'
	
SELECT COUNT (DISTINCT ITEM_ID) FROM TB_ITEM_REVIEW
	WHERE REVIEW_ID = @REVIEW_ID
	AND IS_DELETED = 'TRUE' and MASTER_ITEM_ID is not null

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewStatisticsCounts] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewWorkAllocation]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewWorkAllocation]
(
	@REVIEW_ID INT
)

As

SELECT CONTACT_NAME, TB_WORK_ALLOCATION.CONTACT_ID, SET_NAME, TB_WORK_ALLOCATION.SET_ID,
	WORK_ALLOCATION_ID, ATTRIBUTE_NAME, TB_WORK_ALLOCATION.ATTRIBUTE_ID,
	
	(SELECT COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			AND IS_DELETED = 'FALSE'
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_WORK_ALLOCATION.ATTRIBUTE_ID)
		AS TOTAL_ALLOCATION,
		
		
		(SELECT COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_WORK_ALLOCATION.ATTRIBUTE_ID
			AND TB_ITEM_ATTRIBUTE.ITEM_ID IN
			(
				SELECT ITEM_ID FROM TB_ITEM_SET WHERE TB_ITEM_SET.SET_ID = TB_WORK_ALLOCATION.SET_ID AND 
					TB_ITEM_SET.CONTACT_ID = TB_WORK_ALLOCATION.CONTACT_ID
			))
		AS TOTAL_STARTED
		
FROM TB_WORK_ALLOCATION

INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_WORK_ALLOCATION.CONTACT_ID
INNER JOIN TB_SET ON TB_SET.SET_ID = TB_WORK_ALLOCATION.SET_ID
INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_WORK_ALLOCATION.ATTRIBUTE_ID

WHERE REVIEW_ID = @REVIEW_ID


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewWorkAllocation] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewWorkAllocationContact]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewWorkAllocationContact]
(
	@REVIEW_ID INT, 
	@CONTACT_ID INT
)

As

SELECT CONTACT_NAME, TB_WORK_ALLOCATION.CONTACT_ID, SET_NAME, TB_WORK_ALLOCATION.SET_ID,
	WORK_ALLOCATION_ID, ATTRIBUTE_NAME, TB_WORK_ALLOCATION.ATTRIBUTE_ID,
	
	(SELECT COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_WORK_ALLOCATION.ATTRIBUTE_ID)
		AS TOTAL_ALLOCATION,
		
		(SELECT COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_WORK_ALLOCATION.ATTRIBUTE_ID
			AND TB_ITEM_ATTRIBUTE.ITEM_ID IN
			(
				SELECT ITEM_ID FROM TB_ITEM_SET WHERE TB_ITEM_SET.SET_ID = TB_WORK_ALLOCATION.SET_ID AND 
					TB_ITEM_SET.CONTACT_ID = TB_WORK_ALLOCATION.CONTACT_ID
			))
		AS TOTAL_STARTED

FROM TB_WORK_ALLOCATION

INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_WORK_ALLOCATION.CONTACT_ID
INNER JOIN TB_SET ON TB_SET.SET_ID = TB_WORK_ALLOCATION.SET_ID
INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_WORK_ALLOCATION.ATTRIBUTE_ID

WHERE TB_WORK_ALLOCATION.REVIEW_ID = @REVIEW_ID AND TB_WORK_ALLOCATION.CONTACT_ID = @CONTACT_ID


GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewWorkAllocationContact] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewWorkAllocationDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewWorkAllocationDelete]
(
	@WORK_ALLOCATION_ID INT
)

As

/* ADD TO THIS THE RETRIEVAL OF ROLES */

DELETE FROM TB_WORK_ALLOCATION
WHERE WORK_ALLOCATION_ID = @WORK_ALLOCATION_ID

GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewWorkAllocationDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewWorkAllocationInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReviewWorkAllocationInsert]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@SET_ID INT,
	@ATTRIBUTE_ID BIGINT,
	
	@NEW_WORK_ALLOCATION_ID INT OUTPUT
)

As

/* ADD TO THIS THE RETRIEVAL OF ROLES */

INSERT INTO TB_WORK_ALLOCATION (CONTACT_ID, SET_ID, REVIEW_ID, ATTRIBUTE_ID)
VALUES (@CONTACT_ID, @SET_ID, @REVIEW_ID, @ATTRIBUTE_ID)

SET @NEW_WORK_ALLOCATION_ID = @@IDENTITY

GO
ALTER AUTHORIZATION ON [dbo].[st_ReviewWorkAllocationInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ScreeningCreateMLList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ScreeningCreateMLList]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@WHAT_ATTRIBUTE_ID BIGINT,
	@SCREENING_MODE nvarchar(10),
	@CODE_SET_ID INT,
	@TRAINING_ID INT
)

As

SET NOCOUNT ON

	DECLARE @TP INT
	DECLARE @TN INT

	-- ***** FIRST, GET THE STATS IN TERMS OF # ITEMS SCREENED TO POPULATE THE TRIANING TABLE (GIVES US THE GRAPH ON THE SCREENING TAB)
	SELECT @TP = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 10
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

	SELECT @TN = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 11
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

	-- ********** SECOND, ENTER THE LIST OF ITEMS INTO TB_TRAINING_ITEM ACCORDING TO WHETHER WE'RE FILTERING BY AN ATTRIBUTE OR DOING THE WHOLE REVIEW

	IF @WHAT_ATTRIBUTE_ID > 0  -- i.e. we're filtering by a code
	BEGIN
		INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK], SCORE)
			SELECT @TRAINING_ID, AZ.ITEM_ID, 0, 0, AZ.SCORE
				FROM TB_SCREENING_ML_TEMP AZ
			INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = AZ.ITEM_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @WHAT_ATTRIBUTE_ID
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			WHERE NOT AZ.ITEM_ID IN
				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID) AND AZ.REVIEW_ID = @REVIEW_ID
			ORDER BY AZ.SCORE DESC
			
	END
	ELSE -- NOT FILTERING BY A CODE, SO EVERYTHING IN THE REVIEW THAT'S INCLUDED AND SO FAR UNCODED IS INCLUDED
	BEGIN
		INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK], SCORE)
		SELECT @TRAINING_ID, AZ.ITEM_ID, 0, 0, AZ.SCORE
				FROM TB_SCREENING_ML_TEMP AZ
			WHERE NOT AZ.ITEM_ID IN
				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID) AND AZ.REVIEW_ID = @REVIEW_ID
			ORDER BY AZ.SCORE DESC
	END

	/* SET THE RANKS TO INCREMENT */
	DECLARE @START_INDEX INT = 0
	SELECT @START_INDEX = MIN(TRAINING_ITEM_ID) FROM TB_TRAINING_ITEM WHERE TRAINING_ID = @TRAINING_ID
	UPDATE TB_TRAINING_ITEM
		SET [RANK] = TRAINING_ITEM_ID - @START_INDEX + 1
		WHERE TRAINING_ID = @TRAINING_ID


	-- FINALLY, MIGRATE ANY NON-STALE CODING LOCKS FROM THE PREVIOUS TRAINING RUN
	DECLARE @LAST_TRAINING_ID INT

	SELECT @LAST_TRAINING_ID = TRAINING_ID FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID AND TRAINING_ID < (SELECT MAX(TRAINING_ID) FROM TB_TRAINING WHERE REVIEW_ID = @REVIEW_ID)

	DECLARE @CURRENT_ITERATION INT
	
	SELECT @CURRENT_ITERATION = MAX(ITERATION) + 1 FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID
		
	IF (@CURRENT_ITERATION IS NULL)
	BEGIN
		SET @CURRENT_ITERATION = 1
	END

	UPDATE A
		SET A.CONTACT_ID_CODING = B.CONTACT_ID_CODING,
		A.WHEN_LOCKED = B.WHEN_LOCKED
		FROM TB_TRAINING_ITEM A
		JOIN
		TB_TRAINING_ITEM B ON A.ITEM_ID = B.ITEM_ID AND CURRENT_TIMESTAMP < DATEADD(DAY, 7, B.WHEN_LOCKED) AND
			B.TRAINING_ID = @LAST_TRAINING_ID
		WHERE A.TRAINING_ID = @TRAINING_ID

	UPDATE TB_TRAINING
		SET TIME_ENDED = CURRENT_TIMESTAMP,
		ITERATION = @CURRENT_ITERATION,
		TRUE_POSITIVES = @TP,
		TRUE_NEGATIVES = @TN
		WHERE TB_TRAINING.TRAINING_ID = @TRAINING_ID

	-- delete the old list(s) of items to screen for this review
	DELETE TI
	FROM TB_TRAINING_ITEM TI
	INNER JOIN TB_TRAINING T ON T.TRAINING_ID = TI.TRAINING_ID
	WHERE T.REVIEW_ID = @REVIEW_ID AND T.TRAINING_ID < @TRAINING_ID

	UPDATE TB_REVIEW
		SET SCREENING_INDEXED = 'TRUE',
			SCREENING_MODEL_RUNNING = 'FALSE'
		WHERE REVIEW_ID = @REVIEW_ID

	DELETE FROM TB_SCREENING_ML_TEMP WHERE REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF



GO
ALTER AUTHORIZATION ON [dbo].[st_ScreeningCreateMLList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_ScreeningCreateNonMLList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ScreeningCreateNonMLList]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@WHAT_ATTRIBUTE_ID BIGINT,
	@SCREENING_MODE nvarchar(10),
	@CODE_SET_ID INT
)

As

SET NOCOUNT ON

	DECLARE @NEW_TRAINING_ID INT
	DECLARE @TP INT
	DECLARE @TN INT

	-- ***** FIRST, GET THE STATS IN TERMS OF # ITEMS SCREENED TO POPULATE THE TRIANING TABLE (GIVES US THE GRAPH ON THE SCREENING TAB)
	SELECT @TP = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 10
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

	SELECT @TN = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 11
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID


	-- ******** SECOND, ENTER A NEW LINE IN THE TRAINING TABLE ***************
	INSERT INTO TB_TRAINING(REVIEW_ID, CONTACT_ID, TIME_STARTED, TIME_ENDED, TRUE_POSITIVES, TRUE_NEGATIVES)
		VALUES (@REVIEW_ID, @CONTACT_ID, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, @TP, @TN)
	   
	SET @NEW_TRAINING_ID = @@IDENTITY

	-- ********** THIRD, ENTER THE LIST OF ITEMS INTO TB_TRAINING_ITEM ACCORDING TO WHETHER WE'RE FILTERING BY AN ATTRIBUTE OR DOING THE WHOLE REVIEW
	IF @WHAT_ATTRIBUTE_ID > 0  -- i.e. we're filtering by a code
	BEGIN
		IF @SCREENING_MODE = 'Random' -- FILTERING BY A CODE AND ORDERING AT RANDOM
		BEGIN
			INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK])
			SELECT @NEW_TRAINING_ID, TB_ITEM_REVIEW.ITEM_ID, 0, 0 FROM TB_ITEM_REVIEW
			INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @WHAT_ATTRIBUTE_ID
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			WHERE REVIEW_ID = @REVIEW_ID AND IS_INCLUDED = 'TRUE' AND IS_DELETED = 'FALSE' AND NOT TB_ITEM_REVIEW.ITEM_ID IN
				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID)
			ORDER BY NEWID()
		END
		ELSE -- FILTERING BY A CODE, BUT ORDERING BY THE VALUE PUT IN THE ADDITIONAL_TEXT FIELD
		BEGIN
			INSERT INTO TB_TRAINING_ITEM([RANK], TRAINING_ID, ITEM_ID, CONTACT_ID_CODING)
			SELECT CASE WHEN ISNUMERIC(TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT)=1 THEN CAST(TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT AS INT) ELSE 0 END,
				@NEW_TRAINING_ID, TB_ITEM_REVIEW.ITEM_ID, 0
			FROM TB_ITEM_REVIEW
			INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID AND TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @WHAT_ATTRIBUTE_ID
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			WHERE REVIEW_ID = @REVIEW_ID AND IS_INCLUDED = 'TRUE' AND IS_DELETED = 'FALSE' AND NOT TB_ITEM_REVIEW.ITEM_ID IN
				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID)
			ORDER BY TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT
		END
	END
	ELSE -- NOT FILTERING BY A CODE, SO EVERYTHING IN THE REVIEW THAT'S INCLUDED AND SO FAR UNCODED IS INCLUDED
	BEGIN
		INSERT INTO TB_TRAINING_ITEM(TRAINING_ID, ITEM_ID, CONTACT_ID_CODING, [RANK])
		SELECT @NEW_TRAINING_ID, ITEM_ID, 0, 0 FROM TB_ITEM_REVIEW
			WHERE REVIEW_ID = @REVIEW_ID AND IS_INCLUDED = 'TRUE' AND IS_DELETED = 'FALSE' AND NOT ITEM_ID IN
				(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID)
			ORDER BY NEWID()
	END

	/* SET THE RANKS TO INCREMENT */
	DECLARE @START_INDEX INT = 0
	SELECT @START_INDEX = MIN(TRAINING_ITEM_ID) FROM TB_TRAINING_ITEM WHERE TRAINING_ID = @NEW_TRAINING_ID
	UPDATE TB_TRAINING_ITEM
		SET [RANK] = TRAINING_ITEM_ID - @START_INDEX + 1
		WHERE TRAINING_ID = @NEW_TRAINING_ID


	-- FINALLY, MIGRATE ANY NON-STALE CODING LOCKS FROM THE PREVIOUS TRAINING RUN
	DECLARE @LAST_TRAINING_ID INT

	SELECT @LAST_TRAINING_ID = TRAINING_ID FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID AND TRAINING_ID < (SELECT MAX(TRAINING_ID) FROM TB_TRAINING WHERE REVIEW_ID = @REVIEW_ID)

	DECLARE @CURRENT_ITERATION INT
	
	SELECT @CURRENT_ITERATION = MAX(ITERATION) + 1 FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID
		
	IF (@CURRENT_ITERATION IS NULL)
	BEGIN
		SET @CURRENT_ITERATION = 1
	END

	UPDATE A
		SET A.CONTACT_ID_CODING = B.CONTACT_ID_CODING,
		A.WHEN_LOCKED = B.WHEN_LOCKED
		FROM TB_TRAINING_ITEM A
		JOIN
		TB_TRAINING_ITEM B ON A.ITEM_ID = B.ITEM_ID AND CURRENT_TIMESTAMP < DATEADD(DAY, 7, B.WHEN_LOCKED) AND
			B.TRAINING_ID = @LAST_TRAINING_ID
		WHERE A.TRAINING_ID = @NEW_TRAINING_ID

	UPDATE TB_TRAINING
		SET TIME_ENDED = CURRENT_TIMESTAMP,
		ITERATION = @CURRENT_ITERATION
		WHERE TB_TRAINING.TRAINING_ID = @NEW_TRAINING_ID

	-- delete the old list(s) of items to screen for this review
	DELETE TI
	FROM TB_TRAINING_ITEM TI
	INNER JOIN TB_TRAINING T ON T.TRAINING_ID = TI.TRAINING_ID
	WHERE T.REVIEW_ID = @REVIEW_ID AND T.TRAINING_ID < @NEW_TRAINING_ID

	UPDATE TB_REVIEW
		SET SCREENING_INDEXED = 'TRUE'
		WHERE REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF



GO
ALTER AUTHORIZATION ON [dbo].[st_ScreeningCreateNonMLList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchCodes]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[st_SearchCodes]
(
	@SEARCH_ID int = null output
,	@CONTACT_ID nvarchar(50) = null
,	@REVIEW_ID nvarchar(50) = null
,	@SEARCH_TITLE varchar(4000) = null
,	@ATTRIBUTE_SET_ID_LIST varchar(max) = null
,	@INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'

)
AS
	-- Step One: Insert record into tb_SEARCH
	EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, @ATTRIBUTE_SET_ID_LIST, '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

	-- Step Two: Perform the search and get a hits count
	-- NB: We're using a udf to split the string of answer id's into a table, joining this with the tb_EXTRACT_ATTR (and any others that are required)
	-- to perform the insert.  @ANSWERS should be passed in as 'AT10225, AT10226' (with a comma and a space separating each id)

	
	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
	SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW
		INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!
	WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		AND IS_DELETED != 'true'
		AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
	 

	-- Step Three: Update the new search record in tb_SEARCH with the number of records added
	UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID

GO
ALTER AUTHORIZATION ON [dbo].[st_SearchCodes] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchCodeSetCheck]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[st_SearchCodeSetCheck]
(
	@SEARCH_ID int = null output
,	@CONTACT_ID nvarchar(50) = null
,	@REVIEW_ID nvarchar(50) = null
,	@SEARCH_TITLE varchar(4000) = null
,	@SET_ID INT = NULL
,	@IS_CODED BIT = null
,	@INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'

)

AS
	-- Step One: Insert record into tb_SEARCH
	EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, '', '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

	-- Step Two: Perform the search and get a hits count

	IF (@IS_CODED = 'TRUE')
	BEGIN

	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
		SELECT DISTINCT TB_ITEM_SET.ITEM_ID, @SEARCH_ID, 0 FROM TB_ITEM_SET
			INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_SET.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		WHERE SET_ID = @SET_ID AND REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true'
			AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
			
	END
	ELSE
	BEGIN
	
	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
		SELECT DISTINCT TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID, 0 FROM TB_ITEM_REVIEW
			WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
			
		EXCEPT
		
		SELECT DISTINCT TB_ITEM_SET.ITEM_ID, @SEARCH_ID, 0 FROM TB_ITEM_SET
			INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_SET.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		WHERE SET_ID = @SET_ID AND REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true'
			AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
	
	END
	
	-- Step Three: Update the new search record in tb_SEARCH with the number of records added
	UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID

GO
ALTER AUTHORIZATION ON [dbo].[st_SearchCodeSetCheck] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchCombine]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[st_SearchCombine] (
	@SEARCH_ID int = null output
,	@CONTACT_ID nvarchar(50) = null
,	@REVIEW_ID INT
,	@SEARCH_TITLE varchar(4000) = null
,	@SEARCHES varchar(MAX) = null
,	@COMBINE_TYPE nvarchar(10)
,	@INCLUDED BIT = NULL
)
AS

DECLARE @SEARCH_COMMAND nvarchar(MAX)

EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, @SEARCHES, '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

IF (@COMBINE_TYPE = 'OR')
BEGIN
	/*
	James changed: 14 Feb 2011 as previous code was entering duplicate values if different ranks had been identified
	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
		SELECT DISTINCT  ITEM_ID, @SEARCH_ID, ITEM_RANK FROM dbo.fn_Split_int
		(@SEARCHES, ',') JOIN tb_SEARCH_ITEM ON value = SEARCH_ID
	*/
	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
		SELECT DISTINCT  ITEM_ID, @SEARCH_ID, 0 FROM dbo.fn_Split_int
		(@SEARCHES, ',') JOIN tb_SEARCH_ITEM ON value = SEARCH_ID
END
ELSE
	IF (@COMBINE_TYPE = 'NOT')
	BEGIN
	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
		SELECT  ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW WHERE REVIEW_ID = @REVIEW_ID 
			AND IS_DELETED != 'true' AND IS_INCLUDED = @INCLUDED
		EXCEPT
		SELECT DISTINCT ITEM_ID, @SEARCH_ID FROM		
		 dbo.fn_Split_int
		(@SEARCHES, ',') JOIN tb_SEARCH_ITEM ON value = SEARCH_ID
	END
	ELSE
		IF (@COMBINE_TYPE = 'AND')
		BEGIN
		DECLARE @NUM_RECORDS INT
		DECLARE @DUMMY INT
		SELECT @NUM_RECORDS = COUNT(VALUE) FROM dbo.fn_Split_int (@SEARCHES, ',')

		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
			SELECT DISTINCT  ITEM_ID, @SEARCH_ID FROM
			dbo.fn_Split_int (@SEARCHES, ',') JOIN tb_SEARCH_ITEM ON value = SEARCH_ID
			GROUP BY ITEM_ID
			HAVING COUNT(ITEM_ID) = @NUM_RECORDS
		END

UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID


GO
ALTER AUTHORIZATION ON [dbo].[st_SearchCombine] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_SearchDelete]
(
	@SEARCHES NVARCHAR(MAX)
)

As

SET NOCOUNT ON

	DELETE FROM TB_SEARCH_ITEM
		FROM TB_SEARCH_ITEM INNER JOIN fn_split_int(@SEARCHES, ',') SearchList on SearchList.value = TB_SEARCH_ITEM.SEARCH_ID
		
	DELETE FROM TB_SEARCH
		FROM TB_SEARCH INNER JOIN fn_split_int(@SEARCHES, ',') SearchList on SearchList.value = TB_SEARCH.SEARCH_ID

	/*
	DELETE FROM TB_SEARCH_ITEM
		WHERE SEARCH_ID = @SEARCH_ID

	DELETE FROM TB_SEARCH
		WHERE SEARCH_ID = @SEARCH_ID
	*/

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_SearchDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchForUploadedFiles]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[st_SearchForUploadedFiles]
(
      @SEARCH_ID int = null output
,     @CONTACT_ID nvarchar(50) = null
,     @REVIEW_ID nvarchar(50) = null
,     @SEARCH_TITLE varchar(4000) = null
,	  @PRESENT BIT
,     @INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'
,     @SEARCH_ITEM_ID BIGINT = NULL

)
AS
      -- Step One: Insert record into tb_SEARCH
      EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, '', '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

      -- Step Two: Perform the search and get a hits count
      
      IF (@PRESENT = 'True')
      BEGIN
		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID, 1 FROM TB_ITEM_REVIEW
            INNER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      ELSE
      BEGIN
		INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID, 1 FROM TB_ITEM_REVIEW
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
            EXCEPT
            SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID, 1 FROM TB_ITEM_REVIEW
            INNER JOIN TB_ITEM_DOCUMENT ON TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      
      -- Step Three: Update the new search record in tb_SEARCH with the number of records added
      UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID

GO
ALTER AUTHORIZATION ON [dbo].[st_SearchForUploadedFiles] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchFreeText]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[st_SearchFreeText]
(
      @SEARCH_ID int = null output
,     @CONTACT_ID nvarchar(50) = null
,     @REVIEW_ID nvarchar(50) = null
,     @SEARCH_TITLE varchar(4000) = null
,     @SEARCH_TEXT varchar(4000) = null
,     @SEARCH_WHAT nvarchar(20) = null
,     @INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'
,     @SEARCH_ITEM_ID BIGINT = NULL

)
AS
      -- Step One: Insert record into tb_SEARCH
      EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, @SEARCH_TEXT, '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

      -- Step Two: Perform the search and get a hits count

      IF (@SEARCH_WHAT = 'TitleAbstract')
      BEGIN
            INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID, RANK FROM TB_ITEM_REVIEW
            INNER JOIN CONTAINSTABLE(TB_ITEM, (TITLE, ABSTRACT), @SEARCH_TEXT) AS KEY_TBL ON KEY_TBL.[KEY] = TB_ITEM_REVIEW.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      ELSE
      IF (@SEARCH_WHAT = 'PubYear')
      BEGIN
            INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID, 0 FROM TB_ITEM_REVIEW
            INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
            AND TB_ITEM.[YEAR] LIKE '%' + @SEARCH_TEXT + '%'
      END
      ELSE
      IF (@SEARCH_WHAT = 'AdditionalText')
      BEGIN
            INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT  TB_ITEM_ATTRIBUTE.ITEM_ID, @SEARCH_ID, RANK FROM TB_ITEM_ATTRIBUTE
            INNER JOIN CONTAINSTABLE(TB_ITEM_ATTRIBUTE, ADDITIONAL_TEXT, @SEARCH_TEXT) AS KEY_TBL
                  ON KEY_TBL.[KEY] =  TB_ITEM_ATTRIBUTE.ITEM_ATTRIBUTE_ID
            INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
                  AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
            INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_SET.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      ELSE
      IF (@SEARCH_WHAT = 'ItemId')
      BEGIN
            INSERT INTO TB_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT TB_ITEM.ITEM_ID, @SEARCH_ID, 0 FROM TB_ITEM
            INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM.ITEM_ID
                  WHERE (TB_ITEM.ITEM_ID = @SEARCH_ITEM_ID 
                        OR OLD_ITEM_ID LIKE ('%' + @SEARCH_TEXT + '%'))
                        AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
                        AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      ELSE
      IF (@SEARCH_WHAT = 'Authors')
      BEGIN
            INSERT INTO TB_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT TB_ITEM_AUTHOR.ITEM_ID, @SEARCH_ID, 0 FROM TB_ITEM_AUTHOR
            INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_AUTHOR.ITEM_ID
            WHERE (TB_ITEM_AUTHOR.LAST LIKE '%' + @SEARCH_TEXT + '%'
                  OR TB_ITEM_AUTHOR.FIRST LIKE '%' + @SEARCH_TEXT + '%'
                  OR TB_ITEM_AUTHOR.SECOND LIKE '%' + @SEARCH_TEXT + '%')
                  AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
                  AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      ELSE -- must be uploaded documents
      BEGIN
            INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
            SELECT DISTINCT  tb_ITEM_DOCUMENT.ITEM_ID, @SEARCH_ID, RANK FROM tb_ITEM_DOCUMENT
            INNER JOIN CONTAINSTABLE(TB_ITEM_DOCUMENT, DOCUMENT_TEXT, @SEARCH_TEXT) AS KEY_TBL 
                  ON KEY_TBL.[KEY] = tb_ITEM_DOCUMENT.ITEM_DOCUMENT_ID
            INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = tb_ITEM_DOCUMENT.ITEM_ID
            WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
      END
      
      -- Step Three: Update the new search record in tb_SEARCH with the number of records added
      UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID

GO
ALTER AUTHORIZATION ON [dbo].[st_SearchFreeText] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchImportedIDs]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[st_SearchImportedIDs]
(
	@SEARCH_ID int = null output
,	@CONTACT_ID nvarchar(50) = null
,	@REVIEW_ID nvarchar(50) = null
,	@SEARCH_TITLE varchar(4000) = null
,	@ITEM_ID_LIST varchar(4000) = null
,	@INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'

)
AS
	-- Step One: Insert record into tb_SEARCH
	EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, @ITEM_ID_LIST, '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

	-- Step Two: Perform the search and get a hits count
	-- NB: We're using a udf to split the string of answer id's into a table, joining this with the tb_EXTRACT_ATTR (and any others that are required)
	-- to perform the insert.  @ANSWERS should be passed in as 'AT10225, AT10226' (with a comma and a space separating each id)

	
	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
	SELECT DISTINCT  ir.ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW ir
		INNER JOIN TB_ITEM i on ir.ITEM_ID = i.ITEM_ID
		INNER JOIN dbo.fn_Split(@ITEM_ID_LIST, ',') id ON id.value = i.OLD_ITEM_ID
		WHERE ir.REVIEW_ID = @REVIEW_ID
		AND ir.IS_DELETED = '0'
		AND ir.IS_INCLUDED = @INCLUDED
	 

	-- Step Three: Update the new search record in tb_SEARCH with the number of records added
	UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID



GO
ALTER AUTHORIZATION ON [dbo].[st_SearchImportedIDs] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_SearchInsert]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@SEARCH_TITLE NVARCHAR(4000),
	@ANSWERS varchar(4000) = null,
	@SEARCH_DESC varchar(4000) = null,
	@NEW_SEARCH_ID INT OUTPUT
)

As

SET NOCOUNT ON

	DECLARE @SEARCH_NO INT
	
	SELECT @SEARCH_NO = ISNULL(MAX(SEARCH_NO), 0) + 1 FROM tb_SEARCH WHERE REVIEW_ID = @REVIEW_ID

	INSERT INTO tb_SEARCH
	(	REVIEW_ID
	,	CONTACT_ID
	,	SEARCH_TITLE
	,	SEARCH_NO
	,	ANSWERS
	,	HITS_NO
	,	SEARCH_DATE
	)	
	VALUES
	(
		@REVIEW_ID
	,	@CONTACT_ID
	,	@SEARCH_TITLE
	,	@SEARCH_NO
	,	@ANSWERS
	,	0
	,	GetDate()
	)
	-- Get the identity and return it
	SET @NEW_SEARCH_ID = @@identity

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_SearchInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchItems]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[st_SearchItems]
(
	@SEARCH_ID int = null output
,	@CONTACT_ID nvarchar(50) = null
,	@REVIEW_ID nvarchar(50) = null
,	@SEARCH_TITLE varchar(4000) = null
,	@ITEM_ID_LIST varchar(4000) = null
,	@INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'

)
AS
	-- Step One: Insert record into tb_SEARCH
	EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, @ITEM_ID_LIST, '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

	-- Step Two: Perform the search and get a hits count
	-- NB: We're using a udf to split the string of answer id's into a table, joining this with the tb_EXTRACT_ATTR (and any others that are required)
	-- to perform the insert.  @ANSWERS should be passed in as 'AT10225, AT10226' (with a comma and a space separating each id)

	
	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
	SELECT DISTINCT  ir.ITEM_ID, @SEARCH_ID FROM TB_ITEM_REVIEW ir
		INNER JOIN dbo.fn_Split_int(@ITEM_ID_LIST, ',') id ON id.value = ir.ITEM_ID
		WHERE ir.REVIEW_ID = @REVIEW_ID
		AND ir.IS_DELETED = '0'
		AND ir.IS_INCLUDED = @INCLUDED
	 

	-- Step Three: Update the new search record in tb_SEARCH with the number of records added
	UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID


GO
ALTER AUTHORIZATION ON [dbo].[st_SearchItems] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_SearchList]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT
)

As

SET NOCOUNT ON

	SELECT SEARCH_ID, REVIEW_ID, TB_SEARCH.CONTACT_ID, SEARCH_TITLE,SEARCH_NO, ANSWERS, HITS_NO, SEARCH_DATE,
		CONTACT_NAME FROM TB_SEARCH
		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_SEARCH.CONTACT_ID
		WHERE REVIEW_ID = @REVIEW_ID
		--AND CONTACT_ID = @CONTACT_ID
		ORDER BY SEARCH_NO

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_SearchList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchNullAbstract]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[st_SearchNullAbstract]
(
	@SEARCH_ID int = null output
,	@CONTACT_ID nvarchar(50) = null
,	@REVIEW_ID nvarchar(50) = null
,	@SEARCH_TITLE varchar(4000) = null
,	@INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'
)

AS
BEGIN
	-- Step One: Insert record into tb_SEARCH
	EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, '', '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

	-- Step Two: Perform the search and get a hits count

	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
		SELECT DISTINCT TB_ITEM.ITEM_ID, @SEARCH_ID, 0 FROM TB_ITEM
			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		WHERE TB_ITEM.ABSTRACT IS NULL OR TB_ITEM.ABSTRACT = ''
			AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
			
	-- Step Three: Update the new search record in tb_SEARCH with the number of records added
	UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID

END


GO
ALTER AUTHORIZATION ON [dbo].[st_SearchNullAbstract] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_SearchUpdate]
(
	@SEARCH_ID INT,
	@SEARCH_TITLE NVARCHAR(4000)
)

As

SET NOCOUNT ON

	UPDATE TB_SEARCH
		SET SEARCH_TITLE = @SEARCH_TITLE
		WHERE SEARCH_ID = @SEARCH_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_SearchUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchUpdateHitCount]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_SearchUpdateHitCount]
(
	@SEARCH_ID INT,
	@NEW_HITCOUNT INT = 0
)

As

SET NOCOUNT ON

	UPDATE TB_SEARCH
		SET HITS_NO = @NEW_HITCOUNT
		WHERE SEARCH_ID = @SEARCH_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_SearchUpdateHitCount] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchWeightedTerms]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[st_SearchWeightedTerms]
(
	@SEARCH_ID int = null output
,	@CONTACT_ID int = null
,	@REVIEW_ID int = null
,	@SEARCH_TITLE varchar(4000) = null
,	@TERMS NVARCHAR(4000) = NULL
,	@ANSWERS VARCHAR(max) = NULL
,	@INCLUDED BIT
,	@FILTER_TYPE NVARCHAR(10) = NULL

)
with RECOMPILE
AS

	-- Step One: Insert record into tb_SEARCH
	EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, @ANSWERS, @TERMS, @NEW_SEARCH_ID = @SEARCH_ID OUTPUT
	

	-- Step Two: Perform the search and get a hits count
DECLARE @t TABLE (item_id BIGINT PRIMARY KEY )
		-- No 'answers' to filter on - ADD FILTERS!
IF (@FILTER_TYPE = 'NONE')
BEGIN
	INSERT INTO @t (ITEM_ID)
	SELECT TB_ITEM_REVIEW.ITEM_ID FROM TB_ITEM_REVIEW
	WHERE REVIEW_ID = @REVIEW_ID AND IS_DELETED != 1 AND IS_INCLUDED = @INCLUDED
END
ELSE
IF (@FILTER_TYPE = 'INCLUDE') -- FILTER ON THE SET OF STUDIES IN THE LIST
BEGIN
	INSERT INTO @t (ITEM_ID)
	SELECT DISTINCT  TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
	INNER JOIN dbo.fn_Split_int(@ANSWERS, ',') ON value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.SET_ID = TB_ATTRIBUTE_SET.SET_ID AND
		TB_ITEM_SET.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		AND REVIEW_ID = @REVIEW_ID AND IS_DELETED != 'true' AND IS_INCLUDED = @INCLUDED
END
ELSE -- Final filter type is 'exclude': exclude the items in the list
BEGIN
	INSERT INTO @t (ITEM_ID)
	SELECT DISTINCT  ir.ITEM_ID FROM  TB_ITEM_REVIEW ir
	WHERE ir.REVIEW_ID = @REVIEW_ID AND ir.IS_DELETED != 'true' AND ir.IS_INCLUDED = @INCLUDED
		AND NOT ir.ITEM_ID IN
		(SELECT ir2.ITEM_ID FROM TB_ITEM_ATTRIBUTE IA2
			INNER JOIN TB_ATTRIBUTE_SET AS2 ON AS2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
			INNER JOIN dbo.fn_Split_int(@ANSWERS, ',') ON value = AS2.ATTRIBUTE_SET_ID
			INNER JOIN TB_ITEM_SET tis on IA2.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
			INNER JOIN TB_ITEM_REVIEW ir2 
				ON ir2.ITEM_ID = IA2.ITEM_ID and ir2.REVIEW_ID = @REVIEW_ID 
				AND ir2.IS_DELETED != 'true' AND ir2.IS_INCLUDED = @INCLUDED
		)			
		
		
END
	-- Step Three: do the tex_mining part and insert the results.
insert into tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)	
	SELECT DISTINCT  r.ITEM_ID, @SEARCH_ID, RANK 
	FROM @t r
	INNER JOIN CONTAINSTABLE(tb_item, (TITLE, ABSTRACT), @TERMS) AS KEY_TBL ON KEY_TBL.[KEY] = r.ITEM_ID	
	--for some reason, having the ids in @t helps CONTAINSTABLE not to waste too much work, presumably looking at all terms ins tb_item?
	--see email from Sergio on 17 August 2012 12:00

	
	-- Step Three: Update the new search record in tb_SEARCH with the number of records added
UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID




GO
ALTER AUTHORIZATION ON [dbo].[st_SearchWeightedTerms] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SearchWithoutCodes]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[st_SearchWithoutCodes]
(
	@SEARCH_ID int = null output
,	@CONTACT_ID nvarchar(50) = null
,	@REVIEW_ID nvarchar(50) = null
,	@SEARCH_TITLE varchar(4000) = null
,	@ATTRIBUTE_SET_ID_LIST varchar(max) = null
,	@INCLUDED BIT = NULL -- 'INCLUDED' OR 'EXCLUDED'

)
AS
	-- Step One: Insert record into tb_SEARCH
	EXECUTE st_SearchInsert @REVIEW_ID, @CONTACT_ID, @SEARCH_TITLE, @ATTRIBUTE_SET_ID_LIST, '', @NEW_SEARCH_ID = @SEARCH_ID OUTPUT

	-- Step Two: Perform the search and get a hits count
	-- NB: We're using a udf to split the string of answer id's into a table, joining this with the tb_EXTRACT_ATTR (and any others that are required)
	-- to perform the insert.  @ANSWERS should be passed in as 'AT10225, AT10226' (with a comma and a space separating each id)

	
	INSERT INTO tb_SEARCH_ITEM (ITEM_ID, SEARCH_ID)
	SELECT DISTINCT  TB_ITEM_REVIEW.ITEM_ID, @SEARCH_ID
		FROM TB_ITEM_REVIEW
		WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		AND IS_DELETED != 'true'
		AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
	EXCEPT SELECT TB_ITEM_ATTRIBUTE.ITEM_ID, @SEARCH_ID FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!

	-- Step Three: Update the new search record in tb_SEARCH with the number of records added
	UPDATE tb_SEARCH SET HITS_NO = @@ROWCOUNT WHERE SEARCH_ID = @SEARCH_ID


GO
ALTER AUTHORIZATION ON [dbo].[st_SearchWithoutCodes] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SetTypeList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_SetTypeList]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT * from TB_SET_TYPE order by SET_TYPE_ID
	SELECT * from TB_SET_TYPE_ATTRIBUTE_TYPE tat
	inner join TB_ATTRIBUTE_TYPE t on t.ATTRIBUTE_TYPE_ID = tat.ATTRIBUTE_TYPE_ID
	order by SET_TYPE_ID
	SELECT * from TB_SET_TYPE_PASTE order by DEST_SET_TYPE_ID
END


GO
ALTER AUTHORIZATION ON [dbo].[st_SetTypeList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_Source_Update]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		S
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[st_Source_Update] 
	-- Add the parameters for the stored procedure here
	@s_ID int = 0, 
	@sDB nvarchar(200),
	@Name nvarchar(255),
	@DoS date,
	@DoI date,
	@Descr nvarchar(4000),
	@s_Str nvarchar(MAX),
	@Notes nvarchar(4000)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	UPDATE TB_SOURCE
	   SET [SOURCE_NAME] = @Name
		  ,[DATE_OF_SEARCH] = @DoS
		  ,[DATE_OF_IMPORT] = @DoI
		  ,[SOURCE_DATABASE] = @sDB
		  ,[SEARCH_DESCRIPTION] = @Descr
		  ,[SEARCH_STRING] = @s_Str
		  ,[NOTES] = @Notes
      
 WHERE SOURCE_ID = @s_ID
END



GO
ALTER AUTHORIZATION ON [dbo].[st_Source_Update] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SourceAddToReview]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_SourceAddToReview]
	-- Add the parameters for the stored procedure here
	@Source_ID int
	,@Review_ID int
	,@Included bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	BEGIN TRAN A
	insert into TB_ITEM_REVIEW 
		select item_id, @Review_ID, @Included, null, 0
		from TB_SOURCE s
			inner join TB_ITEM_SOURCE tis on s.SOURCE_ID = tis.SOURCE_ID
					and s.REVIEW_ID = @Review_ID and s.SOURCE_ID = @Source_ID
	COMMIT TRAN A				
END


GO
ALTER AUTHORIZATION ON [dbo].[st_SourceAddToReview] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SourceDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sergio
-- Create date: 20/7/09
-- Description:	(Un/)Delete a source and all its Items
-- =============================================
CREATE PROCEDURE [dbo].[st_SourceDelete] 
	-- Add the parameters for the stored procedure here
	@source_ID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
declare @state bit;
declare @rev_ID int;
Set @state = 1 - (select IS_DELETED from TB_SOURCE where SOURCE_ID = @source_ID)
set @rev_ID = (select review_id from TB_SOURCE where SOURCE_ID = @source_ID)
BEGIN TRY

BEGIN TRANSACTION
update TB_SOURCE set IS_DELETED = @state where SOURCE_ID = @source_ID
update IR set IS_DELETED = @state, IS_INCLUDED = 1
	from TB_SOURCE inner join
		tb_item_source on TB_SOURCE.source_id = tb_item_source.source_id
		inner join tb_item on tb_item_source.item_id = tb_item.Item_ID
		inner join tb_item_review as IR on tb_item.Item_ID = IR.Item_ID
	where (TB_SOURCE.SOURCE_ID = @source_ID AND IR.REVIEW_ID = @rev_ID)
		AND (IR.MASTER_ITEM_ID is null OR IR.IS_DELETED = 0)

COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
END CATCH
END

GO
ALTER AUTHORIZATION ON [dbo].[st_SourceDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SourceDeleteForever]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sergio
-- Create date: 20/7/09
-- Description:	(Un/)Delete a source and all its Items
-- =============================================
CREATE PROCEDURE [dbo].[st_SourceDeleteForever] 
	-- Add the parameters for the stored procedure here
	@srcID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
Declare @tt TABLE
(
	item_ID bigint
	,cnt int	
)
insert into @tt --First: get the ITEM_IDs we'll deal with and see if they appear in more than one review
SELECT ITEM_ID, cnt FROM
	(select ir.ITEM_ID, COUNT(ir.item_id) cnt from TB_ITEM_REVIEW ir 
		inner join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
		where tis.SOURCE_ID = @srcID -- cnt = 1
		group by ir.ITEM_ID) cc

BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION


--Second: delete the records in TB_ITEM_REVIEW for the items that are shared across reviews
delete from TB_ITEM_REVIEW 
	where REVIEW_ID = (select REVIEW_ID from TB_SOURCE where SOURCE_ID = @srcID)
		AND ITEM_ID in (select item_ID from @tt where cnt > 1)
--Third: explicitly delete the records that can't be automatically deleted through the foreign key cascade actions
-- the cnt=1 clause makes sure we don't touch data related to items that appear in other reviews.
DELETE FROM TB_ITEM_DUPLICATES where ITEM_ID_OUT in (SELECT item_ID from @tt where cnt = 1)
DELETE FROM TB_ITEM_LINK where ITEM_ID_SECONDARY in (SELECT item_ID from @tt where cnt = 1)
DELETE FROM TB_ITEM_DOCUMENT where ITEM_ID in (SELECT item_ID from @tt where cnt = 1)
--Fourth: delete the items 
DELETE FROM TB_ITEM WHERE ITEM_ID in (SELECT item_ID from @tt where cnt = 1)
--Fifth: delete the source
DELETE FROM TB_SOURCE WHERE SOURCE_ID = @srcID
--there is a lot that happens automatically through the cascade option of foreing key relationships
--if some cross-reference route is not covered by the explicit deletions the whole transaction should rollback thanks to the catch clause.
COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
END CATCH
END



GO
ALTER AUTHORIZATION ON [dbo].[st_SourceDeleteForever] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SourceDetails]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sergio
-- Create date: 29-06-09
-- Description:	Gets Sources from Review_ID
-- =============================================
CREATE PROCEDURE [dbo].[st_SourceDetails] 
	-- Add the parameters for the stored procedure here
	@revID int = 0,
	@sourceID int = 0
	with recompile
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	--declare @l1 nvarchar(200)
	--declare @l2 nvarchar(200)
	--declare @l3 nvarchar(200)
	--declare @l4 nvarchar(200)
	--declare @l5 nvarchar(200)
	--declare @l6 nvarchar(200)
	--declare @l7 nvarchar(200)
	
	--set @l1 = CONVERT(varchar, SYSDATETIME(), 121)
	declare @tt table
	(
		SOURCE_NAME nvarchar(255)
		,IS_DELETED bit
		,Source_ID int
		,DATE_OF_SEARCH date
		,DATE_OF_IMPORT date
		,SOURCE_DATABASE nvarchar(200)
		,SEARCH_DESCRIPTION nvarchar(4000)
		,SEARCH_STRING nvarchar(MAX)
		,NOTES nvarchar(4000)
		,IMPORT_FILTER nvarchar(60)
		,REVIEW_ID int
		
	)
	declare @t1 table
	(
		Source_ID int
		,[Total_Items] int NULL
		, [Deleted_Items] int NULL
	)
	declare @t2 table
	(
		Source_ID int
		,CODES int NULL
		,IDUCTIVE_CODES int NULL
	)
	declare @t3 table
	(
		Source_ID int
		,[Attached Files]  int NULL
		,OUTCOMES  int NULL
	)
	declare @t4 table
	(
		Source_ID int
		,DUPLICATES int NULL
		,isMasterOf  int NULL
	)

	insert into @tt
	(	
		SOURCE_NAME
		,[REVIEW_ID]
		,[Source_ID]
		,[IS_DELETED]
		,[DATE_OF_SEARCH]
		,[DATE_OF_IMPORT]
		,[SOURCE_DATABASE]
		,[SEARCH_DESCRIPTION]
		,[SEARCH_STRING]
		,[NOTES]
		,[IMPORT_FILTER]
	)
	SELECT SOURCE_NAME
		,[REVIEW_ID]
		,SOURCE_ID
		,[IS_DELETED]
		,[DATE_OF_SEARCH]
		,[DATE_OF_IMPORT]
		,[SOURCE_DATABASE]
		,[SEARCH_DESCRIPTION]
		,[SEARCH_STRING]
		,ts.[NOTES]
		,tif.IMPORT_FILTER_NAME
	from TB_SOURCE ts Left outer join TB_IMPORT_FILTER tif on ts.IMPORT_FILTER_ID = tif.IMPORT_FILTER_ID
	where REVIEW_ID = @revID and SOURCE_ID = @sourceID
	--set @l2 = CONVERT(varchar, SYSDATETIME(), 121)

	insert into @t1 
		SELECT tt.source_id 
		,COUNT(distinct(tis.ITEM_ID)) 'Total_Items'
		,sum(CASE WHEN ir.IS_DELETED = 1 then 1 else 0 END) 
		--, (SELECT COUNT(distinct(ttis.ITEM_ID)) from TB_ITEM_REVIEW ttir
		--		inner join TB_ITEM_SOURCE ttis on ttis.ITEM_ID = ttir.ITEM_ID
		--		where ttis.SOURCE_ID = tt.SOURCE_ID and ttir.IS_DELETED = 1) 'Deleted_Items'
		from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
		WHERE ir.REVIEW_ID = @revID
		group by tt.Source_ID
	--set @l3 = CONVERT(varchar, SYSDATETIME(), 121)
	Insert into @t2
	(Source_ID, CODES, IDUCTIVE_CODES)  (select source_id, 0 ,0 from @tt)
	update @t2   set CODES = sub.CODES--, IDUCTIVE_CODES = sub.IDUCTIVE_CODES
	from
		(
			SELECT 
			tt.source_id SSID
			,COUNT(distinct(ia.ITEM_ID)) CODES 
			--,COUNT(distinct(iat.ITEM_ATTRIBUTE_TEXT_ID)) IDUCTIVE_CODES
			--,COUNT(distinct(tid.ITEM_DOCUMENT_ID)) [Attached Files]
			--,COUNT(distinct(tio.OUTCOME_ID)) OUTCOMES  
			from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
				inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
				inner join TB_REVIEW_SET rs on rs.REVIEW_ID = tt.REVIEW_ID
				inner join TB_ATTRIBUTE_SET tas on rs.SET_ID = tas.SET_ID
				inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_ID = ia.ITEM_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
				--left outer join TB_ITEM_ATTRIBUTE_TEXT iat on ia.ITEM_ATTRIBUTE_ID = iat.ITEM_ATTRIBUTE_ID
				--left outer join TB_ITEM_DOCUMENT tid on tid.ITEM_ID = tis.ITEM_ID
				--left outer join TB_ITEM_SET tes on tis.ITEM_ID = tes.ITEM_ID 
				--left outer join TB_ITEM_OUTCOME tio on tio.ITEM_SET_ID = tes.ITEM_SET_ID 
			WHERE ir.REVIEW_ID = @revID
			
			group by tt.Source_ID
			
		) sub
		
		where sub.SSID = Source_ID
		--OPTION (OPTIMIZE FOR UNKNOWN)
	--set @l4 = CONVERT(varchar, SYSDATETIME(), 121)
	Insert into @t3
		SELECT tt.source_id
		--,COUNT(distinct(ia.ITEM_ATTRIBUTE_ID)) CODES 
		--,COUNT(distinct(iat.ITEM_ATTRIBUTE_TEXT_ID)) IDUCTIVE_CODES
		,COUNT(distinct(tid.ITEM_DOCUMENT_ID)) [Attached Files]
		,COUNT(distinct(tio.OUTCOME_ID)) OUTCOMES  
		from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
			--left outer join TB_REVIEW_SET rs on rs.REVIEW_ID = tt.REVIEW_ID
			--left outer join TB_ATTRIBUTE_SET tas on rs.SET_ID = tas.SET_ID
			--left outer join TB_ITEM_ATTRIBUTE ia on tis.ITEM_ID = ia.ITEM_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
			--left outer join TB_ITEM_ATTRIBUTE_TEXT iat on ia.ITEM_ATTRIBUTE_ID = iat.ITEM_ATTRIBUTE_ID
			left outer join TB_ITEM_DOCUMENT tid on tid.ITEM_ID = tis.ITEM_ID
			left outer join TB_ITEM_SET tes on tis.ITEM_ID = tes.ITEM_ID 
			left outer join TB_ITEM_OUTCOME tio on tio.ITEM_SET_ID = tes.ITEM_SET_ID 
		WHERE ir.REVIEW_ID = @revID
		group by tt.Source_ID
	--set @l5 = CONVERT(varchar, SYSDATETIME(), 121)
	Insert into @t4
		SELECT
		tt.source_id
		--,(COUNT(distinct(dup.ITEM_DUPLICATES_ID)) + COUNT(distinct(dup2.ITEM_DUPLICATES_ID))) DUPLICATES
		--,COUNT(distinct(ir2.ITEM_REVIEW_ID)) isMasterOf
		,sum(CASE WHEN (
				ir.IS_DELETED = 1 and ir.is_included = 1 AND ir.MASTER_ITEM_ID is NOT null
			) then 1 else 0 END) as 'Duplicates'
		,0
		from 
			@tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
			--left outer join TB_ITEM_REVIEW ir2 on ir2.MASTER_ITEM_ID = ir.ITEM_ID and ir2.REVIEW_ID = @revID
		WHERE ir.REVIEW_ID = @revID
		
		group by tt.Source_ID

	update @t4 set isMasterOf = a.c
		from
		(Select COUNT (distinct ir2.item_id) as c from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.MASTER_ITEM_ID
			inner join TB_ITEM_REVIEW ir2 on ir.MASTER_ITEM_ID = ir2.ITEM_ID and ir2.REVIEW_ID = @revID
			) a
	--set @l6 = CONVERT(varchar, SYSDATETIME(), 121)
	select 
		SOURCE_NAME
		,[Total_Items]
		,[Deleted_Items]
		,IS_DELETED
		,t1.Source_ID
		,DATE_OF_SEARCH
		,DATE_OF_IMPORT
		,SOURCE_DATABASE
		,SEARCH_DESCRIPTION
		,SEARCH_STRING
		,NOTES
		,IMPORT_FILTER
		,REVIEW_ID
		,CODES
		,IDUCTIVE_CODES
		,[Attached Files]
		,DUPLICATES
		,isMasterOf
		,OUTCOMES
	from @tt tt
	inner join @t1 t1 on tt.Source_ID = t1.Source_ID
	inner join @t2 t2 on tt.Source_ID = t2.Source_ID
	inner join @t3 t3 on tt.Source_ID = t3.Source_ID
	inner join @t4 t4 on tt.Source_ID = t4.Source_ID
	order by Source_ID
	--set @l7 = CONVERT(varchar, SYSDATETIME(), 121)
	--select @l1, @l2, @l3, @l4, @l5, @l6, @l7
END

GO
ALTER AUTHORIZATION ON [dbo].[st_SourceDetails] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SourceFromReview_ID]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[st_SourceFromReview_ID] 
	-- Add the parameters for the stored procedure here
	@revID int = 0 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Select SOURCE_NAME, count(*) As 'Total_Items',
		sum(CASE WHEN (tb_item_review.IS_DELETED = 1 and tb_item_review.MASTER_ITEM_ID is null) then 1 else 0 END) as 'Deleted_Items',
		sum(CASE WHEN (
				tb_item_review.IS_DELETED = 1 and tb_item_review.is_included = 1 AND tb_item_review.MASTER_ITEM_ID is NOT null
			) then 1 else 0 END) as 'Duplicates',
		TB_SOURCE.IS_DELETED,
		TB_SOURCE.Source_ID,
		0 as TO_ORDER
		from TB_SOURCE inner join
		tb_item_source on TB_SOURCE.source_id = tb_item_source.source_id
		--inner join tb_item on tb_item_source.item_id = tb_item.Item_ID
		inner join tb_item_review on tb_item_source.Item_ID = tb_item_review.Item_ID
		left outer join TB_IMPORT_FILTER on TB_IMPORT_FILTER.IMPORT_FILTER_ID = TB_SOURCE.IMPORT_FILTER_ID
	where TB_SOURCE.review_ID = @RevID AND TB_ITEM_REVIEW.REVIEW_ID = @RevID
	group by SOURCE_NAME,
			 TB_SOURCE.Source_ID,
			 TB_SOURCE.IS_DELETED
	
	
	-- get sourceless items count in a second resultset
	--Select COUNT(ir.ITEM_REVIEW_ID) as 'SourcelessItems' from tb_item_review ir 
	--	left outer join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
	--	left outer join TB_SOURCE ts on tis.SOURCE_ID = ts.SOURCE_ID and ir.REVIEW_ID = ts.REVIEW_ID
	--where ir.REVIEW_ID = @revID and ts.SOURCE_ID is null
	UNION
	Select 'NN_SOURCELESS_NN' as SOURCE_NAME, count(ir.ITEM_REVIEW_ID) As 'Total_Items',
		sum(CASE WHEN (ir.IS_DELETED = 1 and ir.MASTER_ITEM_ID is null) then 1 else 0 END) as 'Deleted_Items',
		sum(CASE WHEN (
				ir.IS_DELETED = 1 and ir.is_included = 1 AND ir.MASTER_ITEM_ID is NOT null
			) then 1 else 0 END) as 'Duplicates',
		Case 
			when COUNT(ir.ITEM_ID) = Sum(
										case when ir.IS_DELETED = 1 then 1 else 0 end
										) 
			then 1 else 0 end
		 as IS_DELETED,
		-1 as Source_ID,
		1 as TO_ORDER
		from tb_item_review ir 
			left outer join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
			left outer join TB_SOURCE ts on tis.SOURCE_ID = ts.SOURCE_ID and ir.REVIEW_ID = ts.REVIEW_ID
		where ir.REVIEW_ID = @revID and ts.SOURCE_ID is null
		order by TO_ORDER, TB_SOURCE.Source_ID
END

GO
ALTER AUTHORIZATION ON [dbo].[st_SourceFromReview_ID] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SourceFromReview_ID_Extended]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: 29-06-09
-- Description:	Gets Sources from Review_ID
-- =============================================
CREATE PROCEDURE [dbo].[st_SourceFromReview_ID_Extended] 
	-- Add the parameters for the stored procedure here
	@revID int = 0 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @tt table
	(
		SOURCE_NAME nvarchar(255)
		,IS_DELETED bit
		,Source_ID int
		,DATE_OF_SEARCH date
		,DATE_OF_IMPORT date
		,SOURCE_DATABASE nvarchar(200)
		,SEARCH_DESCRIPTION nvarchar(4000)
		,SEARCH_STRING nvarchar(1000)
		,NOTES nvarchar(4000)
		,IMPORT_FILTER nvarchar(60)
		,REVIEW_ID int
		
	)
	declare @t1 table
	(
		Source_ID int
		,[Total_Items] int NULL
		, [Deleted_Items] int NULL
	)
	declare @t2 table
	(
		Source_ID int
		,CODES int NULL
		,IDUCTIVE_CODES int NULL
	)
	declare @t3 table
	(
		Source_ID int
		,[Attached Files]  int NULL
		,OUTCOMES  int NULL
	)
	declare @t4 table
	(
		Source_ID int
		,DUPLICATES int NULL
		,isMasterOf  int NULL
	)

	insert into @tt
	(	
		SOURCE_NAME
		,[REVIEW_ID]
		,[Source_ID]
		,[IS_DELETED]
		,[DATE_OF_SEARCH]
		,[DATE_OF_IMPORT]
		,[SOURCE_DATABASE]
		,[SEARCH_DESCRIPTION]
		,[SEARCH_STRING]
		,[NOTES]
		,[IMPORT_FILTER]
	)
	SELECT SOURCE_NAME
		,[REVIEW_ID]
		,SOURCE_ID
		,[IS_DELETED]
		,[DATE_OF_SEARCH]
		,[DATE_OF_IMPORT]
		,[SOURCE_DATABASE]
		,[SEARCH_DESCRIPTION]
		,[SEARCH_STRING]
		,ts.[NOTES]
		,tif.IMPORT_FILTER_NAME
	from TB_SOURCE ts Left outer join TB_IMPORT_FILTER tif on ts.IMPORT_FILTER_ID = tif.IMPORT_FILTER_ID
	where REVIEW_ID = @revID


	insert into @t1 
		SELECT tt.source_id 
		,COUNT(distinct(tis.ITEM_ID)) 'Total_Items'
		,sum(CASE WHEN ir.IS_DELETED = 1 then 1 else 0 END) 
		--, (SELECT COUNT(distinct(ttis.ITEM_ID)) from TB_ITEM_REVIEW ttir
		--		inner join TB_ITEM_SOURCE ttis on ttis.ITEM_ID = ttir.ITEM_ID
		--		where ttis.SOURCE_ID = tt.SOURCE_ID and ttir.IS_DELETED = 1) 'Deleted_Items'
		from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
		WHERE ir.REVIEW_ID = @revID
		group by tt.Source_ID

	Insert into @t2
	(Source_ID, CODES, IDUCTIVE_CODES)  (select source_id, 0 ,0 from @tt)
	update @t2   set CODES = sub.CODES, IDUCTIVE_CODES = sub.IDUCTIVE_CODES
	from
		(
			SELECT 
			tt.source_id SSID
			,COUNT(distinct(ia.ITEM_ATTRIBUTE_ID)) CODES 
			,COUNT(distinct(iat.ITEM_ATTRIBUTE_TEXT_ID)) IDUCTIVE_CODES
			--,COUNT(distinct(tid.ITEM_DOCUMENT_ID)) [Attached Files]
			--,COUNT(distinct(tio.OUTCOME_ID)) OUTCOMES  
			from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
				inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
				inner join TB_REVIEW_SET rs on rs.REVIEW_ID = tt.REVIEW_ID
				inner join TB_ATTRIBUTE_SET tas on rs.SET_ID = tas.SET_ID
				inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_ID = ia.ITEM_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
				left outer join TB_ITEM_ATTRIBUTE_TEXT iat on ia.ITEM_ATTRIBUTE_ID = iat.ITEM_ATTRIBUTE_ID
				--left outer join TB_ITEM_DOCUMENT tid on tid.ITEM_ID = tis.ITEM_ID
				--left outer join TB_ITEM_SET tes on tis.ITEM_ID = tes.ITEM_ID 
				--left outer join TB_ITEM_OUTCOME tio on tio.ITEM_SET_ID = tes.ITEM_SET_ID 
			WHERE ir.REVIEW_ID = @revID
			
			group by tt.Source_ID
			
		) sub
		
		where sub.SSID = Source_ID
		OPTION (OPTIMIZE FOR UNKNOWN)

	Insert into @t3
		SELECT tt.source_id
		--,COUNT(distinct(ia.ITEM_ATTRIBUTE_ID)) CODES 
		--,COUNT(distinct(iat.ITEM_ATTRIBUTE_TEXT_ID)) IDUCTIVE_CODES
		,COUNT(distinct(tid.ITEM_DOCUMENT_ID)) [Attached Files]
		,COUNT(distinct(tio.OUTCOME_ID)) OUTCOMES  
		from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
			--left outer join TB_REVIEW_SET rs on rs.REVIEW_ID = tt.REVIEW_ID
			--left outer join TB_ATTRIBUTE_SET tas on rs.SET_ID = tas.SET_ID
			--left outer join TB_ITEM_ATTRIBUTE ia on tis.ITEM_ID = ia.ITEM_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
			--left outer join TB_ITEM_ATTRIBUTE_TEXT iat on ia.ITEM_ATTRIBUTE_ID = iat.ITEM_ATTRIBUTE_ID
			left outer join TB_ITEM_DOCUMENT tid on tid.ITEM_ID = tis.ITEM_ID
			left outer join TB_ITEM_SET tes on tis.ITEM_ID = tes.ITEM_ID 
			left outer join TB_ITEM_OUTCOME tio on tio.ITEM_SET_ID = tes.ITEM_SET_ID 
		WHERE ir.REVIEW_ID = @revID
		group by tt.Source_ID

	Insert into @t4
		SELECT
		tt.source_id
		--,(COUNT(distinct(dup.ITEM_DUPLICATES_ID)) + COUNT(distinct(dup2.ITEM_DUPLICATES_ID))) DUPLICATES
		--,COUNT(distinct(ir2.ITEM_REVIEW_ID)) isMasterOf
		,sum(CASE WHEN (
				ir.IS_DELETED = 1 and ir.is_included = 1 AND ir.MASTER_ITEM_ID is NOT null
			) then 1 else 0 END) as 'Duplicates'
		,COUNT(distinct ir2.MASTER_ITEM_ID)
		from 
			@tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
			left outer join TB_ITEM_REVIEW ir2 on ir2.MASTER_ITEM_ID = ir.ITEM_ID and ir2.REVIEW_ID = @revID
		WHERE ir.REVIEW_ID = @revID
		
		group by tt.Source_ID
		--OPTION (OPTIMIZE FOR UNKNOWN)
	select 
		SOURCE_NAME
		,[Total_Items]
		,[Deleted_Items]
		,IS_DELETED
		,t1.Source_ID
		,DATE_OF_SEARCH
		,DATE_OF_IMPORT
		,SOURCE_DATABASE
		,SEARCH_DESCRIPTION
		,SEARCH_STRING
		,NOTES
		,IMPORT_FILTER
		,REVIEW_ID
		,CODES
		,IDUCTIVE_CODES
		,[Attached Files]
		,DUPLICATES
		,isMasterOf
		,OUTCOMES
	from @tt tt
	inner join @t1 t1 on tt.Source_ID = t1.Source_ID
	inner join @t2 t2 on tt.Source_ID = t2.Source_ID
	inner join @t3 t3 on tt.Source_ID = t3.Source_ID
	inner join @t4 t4 on tt.Source_ID = t4.Source_ID
	order by Source_ID
END



GO
ALTER AUTHORIZATION ON [dbo].[st_SourceFromReview_ID_Extended] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SourceLessDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: 20/7/09
-- Description:	(Un/)Delete a source and all its Items
-- =============================================
CREATE PROCEDURE [dbo].[st_SourceLessDelete] 
	-- Add the parameters for the stored procedure here

	@rev_ID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
declare @state bit;
--invert the cuerrent 'source' state (even if there is no source!)
--if all sourceless items are currently deleted @state becomes 0 otherwise 1.
-- we use the @state value to change the state of tb_item_review records...
Set @state = (
			SELECT Case 
			when COUNT(ir.ITEM_ID) = Sum(
										case when ir.IS_DELETED = 1 then 1 else 0 end
										) 
			then 0 else 1 end
		 	from tb_item_review ir 
			left outer join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
			left outer join TB_SOURCE ts on tis.SOURCE_ID = ts.SOURCE_ID and ir.REVIEW_ID = ts.REVIEW_ID
			where ir.REVIEW_ID = @rev_ID and ts.SOURCE_ID is null)
update IR set IS_DELETED = @state, IS_INCLUDED = 1
	from tb_item_review ir 
			left outer join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
			left outer join TB_SOURCE ts on tis.SOURCE_ID = ts.SOURCE_ID and ir.REVIEW_ID = ts.REVIEW_ID
			where ir.REVIEW_ID = @rev_ID and ts.SOURCE_ID is null
		AND (IR.MASTER_ITEM_ID is null OR IR.IS_DELETED = 0)		
END

GO
ALTER AUTHORIZATION ON [dbo].[st_SourceLessDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_SVMReviewData]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_SVMReviewData]
(
	@REVIEW_ID INT
,	@SCREENING_CODE_SET_ID INT = NULL
)

As

SET NOCOUNT ON




	SELECT DISTINCT '+1', TB_ITEM_TERM_VECTORS.ITEM_ID, VECTORS FROM ReviewerTerms.dbo.TB_ITEM_TERM_VECTORS
	INNER JOIN REVIEWER.DBO.TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
	INNER JOIN REVIEWER.DBO.TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
	INNER JOIN Reviewer.dbo.TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
	WHERE tb_item_term_vectors.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		
	UNION
	
	SELECT DISTINCT '-1', ITEM_ID, VECTOR_S FROM (SELECT /* TOP(@N_INCLUDED) */ VECTORS
		AS VECTOR_S, TB_ITEM_TERM_VECTORS.ITEM_ID FROM ReviewerTerms.dbo.TB_ITEM_TERM_VECTORS
	INNER JOIN REVIEWER.DBO.TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
	INNER JOIN REVIEWER.DBO.TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
	INNER JOIN Reviewer.dbo.TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
	WHERE TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
	/*ORDER BY NEWID()*/) AS X
	
	UNION
	
	SELECT DISTINCT '0', TB_ITEM_TERM_VECTORS.ITEM_ID, VECTORS FROM ReviewerTerms.dbo.TB_ITEM_TERM_VECTORS
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		WHERE TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID AND NOT TB_ITEM_TERM_VECTORS.ITEM_ID IN
	(SELECT TB_ITEM_SET.ITEM_ID FROM TB_ITEM_SET
		WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		AND TB_ITEM_SET.SET_ID = @SCREENING_CODE_SET_ID
	)
	
	/*
	-- OLD VERSION, BEFORE WE SIMPLY FILTER BY WHETHER A CODE IS PRESENT IN A GIVEN CODE SET
	
	SELECT DISTINCT '0', TB_ITEM_TERM_VECTORS.ITEM_ID, VECTORS FROM ReviewerTerms.dbo.TB_ITEM_TERM_VECTORS
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'
		AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		WHERE TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID AND NOT TB_ITEM_TERM_VECTORS.ITEM_ID IN
	(SELECT TB_ITEM_TERM_VECTORS.ITEM_ID FROM ReviewerTerms.dbo.TB_ITEM_TERM_VECTORS
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
	
	WHERE TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID)
	
	*/
	
	--select @N_INCLUDED

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_SVMReviewData] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TemplateReviewList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_TemplateReviewList]
	-- Add the parameters for the stored procedure here

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select * from ReviewerAdmin.dbo.TB_TEMPLATE_REVIEW order by TEMPLATE_REVIEW_ID
	
	Select * from ReviewerAdmin.dbo.TB_TEMPLATE_REVIEW tr
		inner join ReviewerAdmin.dbo.TB_TEMPLATE_REVIEW_SET trs on tr.TEMPLATE_REVIEW_ID = trs.TEMPLATE_REVIEW_ID
		inner join TB_REVIEW_SET rs on trs.REVIEW_SET_ID = rs.REVIEW_SET_ID
		inner join TB_SET s on s.SET_ID = rs.SET_ID
		Order by tr.TEMPLATE_REVIEW_ID, TEMPLATE_REVIEW_SET_ID
END


GO
ALTER AUTHORIZATION ON [dbo].[st_TemplateReviewList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TempTermExtractionItemList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TempTermExtractionItemList]
As

SET NOCOUNT ON

SELECT I.ITEM_ID, I.ABSTRACT FROM TB_ITEM I
INNER JOIN dbo.fn_Split_int('80272,80305,70069,69996,80281,66741,82475,72075,64944,80374,80392,80375,80308,80419,80301,80150,71125,80306,73339,80309,70539,80072,80372,69946,66073,69264,74623,66098,77740,80380,71290,77933,70250,64991,71036,80376,80307,70428,71034,67270', ',') ITEMS
ON I.ITEM_ID = ITEMS.value
WHERE I.ABSTRACT != ''

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_TempTermExtractionItemList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TempTermExtractionSelectedItems]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TempTermExtractionSelectedItems]
As

SET NOCOUNT ON

SELECT I.ITEM_ID, I.ABSTRACT FROM TB_ITEM I
INNER JOIN dbo.fn_Split_int('23514,23513,23512,23518,23515,23516,59038,59039,59037,23510,23511', ',') ITEMS
ON I.ITEM_ID = ITEMS.value
WHERE I.ABSTRACT != ''


SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_TempTermExtractionSelectedItems] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_Termine_Log_Insert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Termine_Log_Insert]
	-- Add the parameters for the stored procedure here
	@C_ID int,
	@R_ID int,
	@BYTES int,
	@SUCCESS bit,
	@N int,
	@ERR nvarchar(2000) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO TB_TERMINE_LOG
           (CONTACT_ID
           ,REVIEW_ID
           ,BYTES
           ,SUCCESS
           ,N_OF_TERMS
           ,ERROR
           )
     VALUES
           (@C_ID
           ,@R_ID
           ,@BYTES
           ,@SUCCESS 
		   ,@N 
		   ,@ERR
           )




END


GO
ALTER AUTHORIZATION ON [dbo].[st_Termine_Log_Insert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingCheckData]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingCheckData]
(
	@REVIEW_ID INT,
	@N_INCLUDES INT OUTPUT,
	@N_EXCLUDES INT OUTPUT
)

As

SET NOCOUNT ON

	SELECT @N_INCLUDES = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_TRAINING_SCREENING_CRITERIA
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'

	SELECT @N_EXCLUDES = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_TRAINING_SCREENING_CRITERIA
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'false'
	
SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingCheckData] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingClassificationSet]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create procedure [dbo].[st_TrainingClassificationSet]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	SELECT '0', TB_ITEM_TERM_VECTORS.ITEM_ID, VECTORS, PROBABILITY FROM TB_ITEM_TERM_VECTORS
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'
		WHERE TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID
	
	
	EXCEPT
	
	SELECT '0', TB_ITEM_TERM_VECTORS.ITEM_ID, VECTORS, PROBABILITY FROM TB_ITEM_TERM_VECTORS
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
	WHERE TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingClassificationSet] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingIncludedExcludedIds]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_TrainingIncludedExcludedIds]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	SELECT '1', TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
	WHERE TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID
		
	UNION
	
	SELECT '-1', TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
	WHERE TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingIncludedExcludedIds] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingInsert]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@N_SCREENED INT = NULL,
	@NEW_TRAINING_ID INT output
)

As

SET NOCOUNT ON

	DECLARE @START_TIME DATETIME
	DECLARE @END_TIME DATETIME

	SELECT @START_TIME = MAX(TIME_STARTED) FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID

	SELECT @END_TIME = MAX(TIME_ENDED) FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID
		
	IF (@START_TIME IS NULL) OR (@START_TIME != @END_TIME) OR
	(
		(@START_TIME = @END_TIME) AND CURRENT_TIMESTAMP > DATEADD(MINUTE, 1, @START_TIME) -- i.e. we run whenever something isn't already running and give up after 6 hours (i.e. try again, assuming there was an error)
	)
	BEGIN
		INSERT INTO TB_TRAINING(REVIEW_ID, CONTACT_ID, TIME_STARTED, TIME_ENDED)
		VALUES (@REVIEW_ID, @CONTACT_ID, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
	   
	    SET @NEW_TRAINING_ID = @@IDENTITY
	END
	ELSE
	BEGIN
		SET @NEW_TRAINING_ID = 0
	END

/*

	DECLARE @TIME_WAIT INT = 60  -- we have to wait at least 10 minutes between 'runs'

	--IF @N_SCREENED > 1000 BEGIN SET @TIME_WAIT = 600 END -- SO WE HAVE TO WAIT 10 MINUTES IF THERE ARE MORE THAN 1000 ITEMS BEING TRAINED
	
	DECLARE @START_TIME DATETIME
	
	--SELECT @START_TIME = TIME_STARTED FROM TB_TRAINING
	--	WHERE REVIEW_ID = @REVIEW_ID
	--	GROUP BY TIME_STARTED, ITERATION
	--	HAVING ITERATION = MAX(ITERATION)
	SELECT @START_TIME = MAX(TIME_STARTED) FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID
		
	IF (@START_TIME IS NULL OR (CURRENT_TIMESTAMP > DATEADD(SECOND, @TIME_WAIT, @START_TIME)))
	BEGIN
		INSERT INTO TB_TRAINING(REVIEW_ID, CONTACT_ID, TIME_STARTED, TIME_ENDED)
		VALUES (@REVIEW_ID, @CONTACT_ID, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
	   
	    SET @NEW_TRAINING_ID = @@IDENTITY
	END
	ELSE
	BEGIN
		SET @NEW_TRAINING_ID = 0
	END

*/
SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingInsert] TO  SCHEMA OWNER 
GO
--/****** Object:  StoredProcedure [dbo].[st_TrainingItemAttributeBulkInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--CREATE procedure [dbo].[st_TrainingItemAttributeBulkInsert]
--(
--	@SET_ID INT,
--	@CONTACT_ID INT,
--	@ATTRIBUTE_ID BIGINT,
--	@REVIEW_ID INT,
--	@INCLUDED BIT,
--	@TRAINING_ID INT
--)

--As

--SET NOCOUNT ON

--	DECLARE @ITEM_IDS NVARCHAR(MAX)
	
--	IF (@INCLUDED = 'TRUE')
--	BEGIN
--		SELECT @ITEM_IDS = ITEMS_INC FROM TB_TRAINING WHERE TRAINING_ID = @TRAINING_ID
--	END
--	ELSE
--	BEGIN
--		SELECT @ITEM_IDS = ITEMS_EXC FROM TB_TRAINING WHERE TRAINING_ID = @TRAINING_ID
--	END
	
--	EXECUTE st_ItemAttributeBulkInsert @SET_ID, 'TRUE', @CONTACT_ID, @ATTRIBUTE_ID, @ITEM_IDS, '', @REVIEW_ID	

--SET NOCOUNT OFF


--GO
--ALTER AUTHORIZATION ON [dbo].[st_TrainingItemAttributeBulkInsert] TO  SCHEMA OWNER 
--GO
/****** Object:  StoredProcedure [dbo].[st_TrainingItemText]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_TrainingItemText]
(
	@REVIEW_ID INT,
	@ITEM_ID_LIST NVARCHAR(MAX)
)

As

SET NOCOUNT ON
	
	SELECT TB_ITEM.ITEM_ID, @REVIEW_ID, TB_ITEM.TITLE + '. ' + TB_ITEM.PARENT_TITLE + '.' + TB_ITEM.ABSTRACT
	FROM TB_ITEM
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM.ITEM_ID
		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
	INNER JOIN dbo.fn_Split_int(@ITEM_ID_LIST, ',') IDS ON IDS.value = TB_ITEM.ITEM_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingItemText] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingList]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON
	
	SELECT TRAINING_ID, TB_TRAINING.CONTACT_ID, TIME_STARTED, TIME_ENDED, ITERATION,
		N_TRAINING_INC, N_TRAINING_EXC, CONTACT_NAME, C, TRUE_POSITIVES, TRUE_NEGATIVES, FALSE_POSITIVES, FALSE_NEGATIVES
	from TB_TRAINING
	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_TRAINING.CONTACT_ID
	WHERE REVIEW_ID = @REVIEW_ID
	
SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingNextItem]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingNextItem]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@TRAINING_CODE_SET_ID INT
)

As

SET NOCOUNT ON

	DECLARE @CURRENT_TRAINING_ID INT
	DECLARE @UPDATED_TRAINING_ITEM TABLE(TRAINING_ITEM_ID INT)

-- FIRST, GET THE CURRENT TRAINING 'RUN' (CAN'T SEND TO THE STORED PROC, AS IT MAY HAVE CHANGED)
	SELECT @CURRENT_TRAINING_ID = MAX(TRAINING_ID) FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID
		AND TIME_STARTED < TIME_ENDED

-- NEXT, LOCK THE ITEM WE'RE GOING TO SEND BACK

	UPDATE TB_TRAINING_ITEM
		SET CONTACT_ID_CODING = @CONTACT_ID, WHEN_LOCKED = CURRENT_TIMESTAMP
		OUTPUT INSERTED.TRAINING_ITEM_ID INTO @UPDATED_TRAINING_ITEM
		WHERE
		TRAINING_ITEM_ID = 
			(SELECT MIN(TRAINING_ITEM_ID) FROM TB_TRAINING_ITEM TI
				LEFT OUTER JOIN TB_ITEM_SET ISET ON ISET.ITEM_ID = TI.ITEM_ID AND (IS_COMPLETED = 'TRUE' OR ISET.CONTACT_ID = @CONTACT_ID) AND SET_ID = @TRAINING_CODE_SET_ID
				WHERE (CONTACT_ID_CODING = @CONTACT_ID OR CONTACT_ID_CODING = 0) AND TRAINING_ID = @CURRENT_TRAINING_ID AND ISET.ITEM_ID IS NULL)

		/*
			(SELECT MIN(TRAINING_ITEM_ID) FROM TB_TRAINING_ITEM TI
				WHERE (CONTACT_ID_CODING = 0 OR CONTACT_ID_CODING = @CONTACT_ID) -- i.e. if we've locked something, but not coded it, we are presented with it again
				 AND TRAINING_ID = @CURRENT_TRAINING_ID AND NOT ITEM_ID IN
					(SELECT ITEM_ID FROM TB_ITEM_SET WHERE (IS_COMPLETED = 'TRUE' OR CONTACT_ID = @CONTACT_ID) AND SET_ID = @TRAINING_CODE_SET_ID)) -- covers both completed / uncompleted multi-user scenarios
		*/
-- FINALLY, SEND IT BACK

	SELECT TI.TRAINING_ITEM_ID, ITEM_ID, [RANK], TRAINING_ID, CONTACT_ID_CODING, SCORE
		FROM TB_TRAINING_ITEM TI
		INNER JOIN @UPDATED_TRAINING_ITEM UTI ON UTI.TRAINING_ITEM_ID = TI.TRAINING_ITEM_ID
	
SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingNextItem] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingPreviousItem]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingPreviousItem]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON

DECLARE @CURRENT_TRAINING_ID INT
	DECLARE @UPDATED_TRAINING_ITEM TABLE(TRAINING_ITEM_ID INT)

-- FIRST, GET THE CURRENT TRAINING 'RUN' (CAN'T SEND TO THE STORED PROC, AS IT MAY HAVE CHANGED)
	SELECT @CURRENT_TRAINING_ID = MAX(TRAINING_ID) FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID
		AND TIME_STARTED < TIME_ENDED

-- NEXT, TRY TO LOCK THE ITEM WE'RE GOING TO SEND BACK (BUT WE WON'T OVERRIDE SOMEONE ELSE'S LOCK)

	UPDATE TB_TRAINING_ITEM
		SET CONTACT_ID_CODING = @CONTACT_ID, WHEN_LOCKED = CURRENT_TIMESTAMP
		WHERE
		ITEM_ID = @ITEM_ID AND CONTACT_ID_CODING = 0 AND TRAINING_ID = @CURRENT_TRAINING_ID

-- FINALLY, SEND IT BACK

	SELECT TI.TRAINING_ITEM_ID, ITEM_ID, [RANK], TRAINING_ID, @CONTACT_ID, SCORE
		FROM TB_TRAINING_ITEM TI
		WHERE TRAINING_ID = @CURRENT_TRAINING_ID AND ITEM_ID = @ITEM_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingPreviousItem] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingProcessTerms]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingProcessTerms]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT
)

As

SET NOCOUNT ON
	
	-- GETTING THE SCORES FOR REVIEWER TERMS

	UPDATE TB_ITEM_TERM_VECTORS
	SET RELEVANT_TERMS = 
	(SELECT COUNT(*) FROM TB_TRAINING_REVIEWER_TERM
		INNER JOIN TB_ITEM_TERM_DICTIONARY ON TB_ITEM_TERM_DICTIONARY.ITEM_TERM_DICTIONARY_ID =
			TB_TRAINING_REVIEWER_TERM.ITEM_TERM_DICTIONARY_ID
		INNER JOIN TB_ITEM_TERM ON TB_ITEM_TERM.TERM = TB_ITEM_TERM_DICTIONARY.TERM
		WHERE TB_TRAINING_REVIEWER_TERM.REVIEW_ID = @REVIEW_ID
		AND TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID
		AND TB_TRAINING_REVIEWER_TERM.INCLUDED = 'True'
		AND TB_ITEM_TERM_VECTORS.ITEM_ID = TB_ITEM_TERM.ITEM_ID)
	WHERE REVIEW_ID = @REVIEW_ID
		
		
	UPDATE TB_ITEM_TERM_VECTORS
	SET IRRELEVANT_TERMS = 
	(SELECT COUNT(*) FROM TB_TRAINING_REVIEWER_TERM
		INNER JOIN TB_ITEM_TERM_DICTIONARY ON TB_ITEM_TERM_DICTIONARY.ITEM_TERM_DICTIONARY_ID =
			TB_TRAINING_REVIEWER_TERM.ITEM_TERM_DICTIONARY_ID
		INNER JOIN TB_ITEM_TERM ON TB_ITEM_TERM.TERM = TB_ITEM_TERM_DICTIONARY.TERM
		WHERE TB_TRAINING_REVIEWER_TERM.REVIEW_ID = @REVIEW_ID
		AND TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID
		AND TB_TRAINING_REVIEWER_TERM.INCLUDED = 'False'
		AND TB_ITEM_TERM_VECTORS.ITEM_ID = TB_ITEM_TERM.ITEM_ID)
	WHERE REVIEW_ID = @REVIEW_ID
		
	UPDATE TB_ITEM_TERM_VECTORS
	SET PROBABILITY = LOG((CAST(RELEVANT_TERMS AS FLOAT) + 1) / (CAST(IRRELEVANT_TERMS AS FLOAT) + 1))
		WHERE REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingProcessTerms] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingProcessText]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingProcessText]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT
)

As


SET NOCOUNT ON

/*
	
	ALL DONE MANUALLY AT THE MOMENT, BUT THE CODE WORKS!
	
	-- ADD need to write to a log the fact that text is being processed
	
	-- CLEAN OUT EXISTING DATA (MAYBE NOT KEEP, SO THAT SUBSEQUENT PROCESSING IS QUICKER.
	DELETE FROM TB_ITEM_TERM_TEXT WHERE REVIEW_ID = @REVIEW_ID
	DELETE FROM TB_ITEM_TERM WHERE REVIEW_ID = @REVIEW_ID
	DELETE FROM TB_ITEM_TERM_VECTORS WHERE REVIEW_ID = @REVIEW_ID

	-- FIRST: PUT THE BITS OF TITLES AND ABSTRACTS INTO TB_ITEM_TERM_TEXT (21 secs on laptop for 44k rows)
	INSERT INTO TB_ITEM_TERM_TEXT(ITEM_ID, REVIEW_ID, ITEM_TEXT)
	SELECT TB_ITEM.ITEM_ID , @REVIEW_ID, TB_ITEM.TITLE + '. ' + TB_ITEM.PARENT_TITLE
	FROM TB_ITEM
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM.ITEM_ID
		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
	
	DECLARE @ITEM_ID BIGINT
	DECLARE @ABSTRACT NVARCHAR(MAX)
	DECLARE @LENGTH BIGINT
	DECLARE @START BIGINT
	DECLARE @END BIGINT
	DECLARE @CONTINUE BIT = 1
	DECLARE @SUBSTRING NVARCHAR(4000)

	Declare textCursor CURSOR READ_ONLY FOR
	SELECT TB_ITEM.ITEM_ID, ABSTRACT
	FROM TB_ITEM
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM.ITEM_ID
		AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		--WHERE LEN(TB_ITEM.ABSTRACT) > 4000

	Open textCursor
	Fetch next from textCursor into @ITEM_ID, @ABSTRACT
	While @@FETCH_STATUS = 0
	Begin
		SET @LENGTH = LEN(@ABSTRACT)
		
		IF (@LENGTH < 3999)
		BEGIN
			INSERT INTO TB_ITEM_TERM_TEXT(ITEM_ID, REVIEW_ID, ITEM_TEXT)
			VALUES(@ITEM_ID, @REVIEW_ID, CAST(@ABSTRACT AS NVARCHAR(4000)))
		END
		ELSE
		BEGIN
			SET @START = 0
			SET @CONTINUE = 1
			IF (CHARINDEX(' ', @ABSTRACT, 3980) > 0)
			BEGIN
				SET @END = CHARINDEX(' ', @ABSTRACT, 3980)
			END
			ELSE
			BEGIN
				SET @END = 3980
			END
			WHILE (@CONTINUE = 1)
			BEGIN
				SET @SUBSTRING = CAST(SUBSTRING(@ABSTRACT, @START, @END - @START) AS NVARCHAR(4000))
				INSERT INTO TB_ITEM_TERM_TEXT(ITEM_ID, REVIEW_ID, ITEM_TEXT)
				VALUES(@ITEM_ID, @REVIEW_ID, @SUBSTRING)
				
				SET @START = @END
				SET @END = @END + 3980
				
				IF (@END > @LENGTH)
				BEGIN
					SET @SUBSTRING = CAST(SUBSTRING(@ABSTRACT, @START, @LENGTH - @START) AS NVARCHAR(4000))
					INSERT INTO TB_ITEM_TERM_TEXT(ITEM_ID, REVIEW_ID, ITEM_TEXT)
					VALUES(@ITEM_ID, @REVIEW_ID, @SUBSTRING)
					SET @CONTINUE = 0
				END
				ELSE
				BEGIN
					IF (CHARINDEX(' ', @ABSTRACT, @END) > 0)
					BEGIN
						SET @END = CHARINDEX(' ', @ABSTRACT, @END)
					END
					ELSE
					BEGIN
						SET @END = @END + 3980
					END
				END
			END
		END
		
		Fetch next from textCursor into @ITEM_ID, @ABSTRACT
	End
	Close textCursor
	Deallocate textCursor
	
	DECLARE @MAX_IDENTITY INT
	SELECT @MAX_IDENTITY = MAX(ITEM_TERM_DICTIONARY_ID) FROM TB_ITEM_TERM_DICTIONARY
	DBCC CHECKIDENT (TB_ITEM_TERM_DICTIONARY, RESEED, @MAX_IDENTITY)
		
	-- HERE RUN THE DICTIONARY CREATION PACKAGE TERM EXTRACTION ************* ADD **************
	
	-- IN BETWEEN PACKAGES REMOVE DUPLICATE ENTRIES IN THE DICTIONARY TABLE
	DELETE
	FROM TB_ITEM_TERM_DICTIONARY
	WHERE item_term_dictionary_id NOT IN
	(
	SELECT MAX(item_term_dictionary_id)
	FROM TB_ITEM_TERM_DICTIONARY
	GROUP BY TERM
	)
	
	***************** ADD HERE - DBCC RESEED THE IDENTITY SO THAT IT FOLLOWS ON FROM MAX_ID *****************
	
	-- NOW RUN THE TERM LOOKUP PACKAGE ************* ADD ***************
	
	-- NOW CLEAN UP DUPLICATE ROWS
	delete T1
	from TB_ITEM_TERM T1, TB_ITEM_TERM T2
	where T1.ITEM_ID = T2.ITEM_ID AND T1.TERM = T2.TERM
	and T1.ITEM_TERM_ID > T2.ITEM_TERM_ID
	
	-- ONCE THE TERMS HAVE BEEN IDENTIFIED, THE FINAL TEXT PROCESSING STEP IS TO VECTORISE THEM (AROUND 1 MIN PER THOUSAND ITEMS ON MY LAPTOP)
	INSERT INTO TB_ITEM_TERM_VECTORS(ITEM_ID, VECTORS, REVIEW_ID)
	SELECT ITEM_ID, '', @REVIEW_ID FROM TB_ITEM_REVIEW
		WHERE REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'

	DECLARE @ITEM_TERM_DICTIONARY_ID BIGINT
	DECLARE @SCORE INT

	Declare textCursor CURSOR READ_ONLY FOR
	SELECT ITEM_ID, TB_ITEM_TERM_DICTIONARY.ITEM_TERM_DICTIONARY_ID, TB_ITEM_TERM.SCORE FROM TB_ITEM_TERM
	INNER JOIN TB_ITEM_TERM_DICTIONARY ON TB_ITEM_TERM_DICTIONARY.TERM = TB_ITEM_TERM.TERM
	WHERE TB_ITEM_TERM.REVIEW_ID = @REVIEW_ID
	ORDER BY ITEM_ID

	DECLARE @CURRENT_VECTORS NVARCHAR(MAX) = ''
	DECLARE @CURRENT_ITEM_ID BIGINT = 0
	Open textCursor
	Fetch next from textCursor into @ITEM_ID, @ITEM_TERM_DICTIONARY_ID, @SCORE
	While @@FETCH_STATUS = 0
	Begin
			IF (@CURRENT_ITEM_ID = 0)
			BEGIN
				SET @CURRENT_ITEM_ID = @ITEM_ID
			END
			
			IF (@CURRENT_ITEM_ID <> @ITEM_ID)
			BEGIN
				UPDATE TB_ITEM_TERM_VECTORS
				SET VECTORS = @CURRENT_VECTORS
				WHERE ITEM_ID = @CURRENT_ITEM_ID
				
				SET @CURRENT_VECTORS = ''
				SET @CURRENT_ITEM_ID = @ITEM_ID
			END
			
			SET @CURRENT_VECTORS = @CURRENT_VECTORS + CAST(@ITEM_TERM_DICTIONARY_ID AS NVARCHAR(10)) + ':1 '
	       
			Fetch next from textCursor into @ITEM_ID, @ITEM_TERM_DICTIONARY_ID, @SCORE
	End
	Close textCursor
	Deallocate textCursor
	
	-- (As the last one wasn't written within the while... loop)
	UPDATE TB_ITEM_TERM_VECTORS
	SET VECTORS = @CURRENT_VECTORS
	WHERE ITEM_ID = @ITEM_ID

	DELETE FROM TB_ITEM_TERM_VECTORS WHERE VECTORS = ''

*/

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingProcessText] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingReviewerTermDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingReviewerTermDelete]
(
	@REVIEW_ID INT,
	@TRAINING_REVIEWER_TERM_ID INT
)

As

SET NOCOUNT ON
	
	DELETE FROM TB_TRAINING_REVIEWER_TERM
		WHERE TRAINING_REVIEWER_TERM_ID = @TRAINING_REVIEWER_TERM_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingReviewerTermDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingReviewerTermInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingReviewerTermInsert]
(
	@REVIEW_ID INT,
	@REVIEWER_TERM NVARCHAR(255),
	@INCLUDED BIT,
	@NEW_TRAINING_REVIEWER_TERM_ID INT output
)

As

SET NOCOUNT ON
	
	DECLARE @ITEM_TERM_DICTIONARY_ID BIGINT
	
	SELECT @ITEM_TERM_DICTIONARY_ID = ITEM_TERM_DICTIONARY_ID FROM TB_ITEM_TERM_DICTIONARY
		WHERE TB_ITEM_TERM_DICTIONARY.TERM = @REVIEWER_TERM
	
	INSERT INTO TB_TRAINING_REVIEWER_TERM(REVIEWER_TERM, INCLUDED, REVIEW_ID, ITEM_TERM_DICTIONARY_ID)
	VALUES (@REVIEWER_TERM, @INCLUDED, @REVIEW_ID, @ITEM_TERM_DICTIONARY_ID)
	
	SET @NEW_TRAINING_REVIEWER_TERM_ID = @@IDENTITY

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingReviewerTermInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingReviewerTermList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingReviewerTermList]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON
	
	SELECT REVIEWER_TERM, TRAINING_REVIEWER_TERM_ID, INCLUDED, TB_TRAINING_REVIEWER_TERM.ITEM_TERM_DICTIONARY_ID, TERM
		FROM TB_TRAINING_REVIEWER_TERM
	LEFT OUTER JOIN TB_ITEM_TERM_DICTIONARY
		ON TB_ITEM_TERM_DICTIONARY.ITEM_TERM_DICTIONARY_ID = TB_TRAINING_REVIEWER_TERM.ITEM_TERM_DICTIONARY_ID
	WHERE REVIEW_ID = @REVIEW_ID
	
SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingReviewerTermList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingReviewerTermUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingReviewerTermUpdate]
(
	@REVIEW_ID INT,
	@REVIEWER_TERM NVARCHAR(255),
	@INCLUDED BIT,
	@TRAINING_REVIEWER_TERM_ID INT
)

As

SET NOCOUNT ON
	
	DECLARE @ITEM_TERM_DICTIONARY_ID BIGINT
	
	SELECT @ITEM_TERM_DICTIONARY_ID = ITEM_TERM_DICTIONARY_ID FROM TB_ITEM_TERM_DICTIONARY
		WHERE TB_ITEM_TERM_DICTIONARY.TERM = @REVIEWER_TERM
	
	UPDATE TB_TRAINING_REVIEWER_TERM
		SET REVIEWER_TERM = @REVIEWER_TERM,
		INCLUDED = @INCLUDED,
		ITEM_TERM_DICTIONARY_ID = @ITEM_TERM_DICTIONARY_ID
	WHERE
		TRAINING_REVIEWER_TERM_ID = @TRAINING_REVIEWER_TERM_ID
	
SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingReviewerTermUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingScreeningCriteriaDelete]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingScreeningCriteriaDelete]
(
	@TRAINING_SCREENING_CRITERIA_ID INT
)

As

SET NOCOUNT ON
	
	DELETE FROM TB_TRAINING_SCREENING_CRITERIA
		WHERE TRAINING_SCREENING_CRITERIA_ID = @TRAINING_SCREENING_CRITERIA_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingScreeningCriteriaDelete] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingScreeningCriteriaInsert]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingScreeningCriteriaInsert]
(
	@REVIEW_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@INCLUDED BIT,
	@NEW_TRAINING_SCREENING_CRITERIA_ID INT OUTPUT
)

As

SET NOCOUNT ON
	
	SELECT ATTRIBUTE_ID FROM TB_TRAINING_SCREENING_CRITERIA
		WHERE REVIEW_ID = @REVIEW_ID AND ATTRIBUTE_ID = @ATTRIBUTE_ID

	IF (@@ROWCOUNT = 0)
	BEGIN
		INSERT INTO TB_TRAINING_SCREENING_CRITERIA(REVIEW_ID, ATTRIBUTE_ID, INCLUDED)
		VALUES (@REVIEW_ID, @ATTRIBUTE_ID, @INCLUDED)
	END
	
	SELECT @NEW_TRAINING_SCREENING_CRITERIA_ID = @@IDENTITY

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingScreeningCriteriaInsert] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingScreeningCriteriaList]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingScreeningCriteriaList]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	SELECT TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID, INCLUDED, ATTRIBUTE_NAME, TRAINING_SCREENING_CRITERIA_ID
		FROM TB_TRAINING_SCREENING_CRITERIA
	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID
	WHERE TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingScreeningCriteriaList] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingScreeningCriteriaUpdate]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingScreeningCriteriaUpdate]
(
	@REVIEW_ID INT,
	@TRAINING_SCREENING_CRITERIA_ID int,
	@INCLUDED BIT
)

As

SET NOCOUNT ON
	
	UPDATE TB_TRAINING_SCREENING_CRITERIA
		SET INCLUDED = @INCLUDED
		WHERE TRAINING_SCREENING_CRITERIA_ID = @TRAINING_SCREENING_CRITERIA_ID

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingScreeningCriteriaUpdate] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingSearchWeightedTerms]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[st_TrainingSearchWeightedTerms]
(
	@REVIEW_ID int = null
,	@TERMS NVARCHAR(4000) = NULL
)
AS

SET NOCOUNT ON

DECLARE @ITEM_IDS TABLE
	(
	  ITEM_ID BIGINT,
	  --PROBABILITY INT,
	  IDX INT IDENTITY(1,1)
	)
	
DECLARE @ITEM_IDS_EX TABLE
(
	  ITEM_ID BIGINT
)
	
	INSERT INTO @ITEM_IDS(ITEM_ID)
	
		SELECT TB_ITEM_REVIEW.ITEM_ID FROM TB_ITEM_REVIEW 
		INNER JOIN CONTAINSTABLE(TB_ITEM, (TITLE, ABSTRACT), @TERMS) AS KEY_TBL ON KEY_TBL.[KEY] = TB_ITEM_REVIEW.ITEM_ID
		WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'
		AND NOT TB_ITEM_REVIEW.ITEM_ID IN
		(
		SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
			TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		)
		ORDER BY RANK
	
	-- EXCLUDED ITEMS (ACCORDING TO THE TERM SEARCH)
	INSERT INTO @ITEM_IDS_EX(ITEM_ID)
	SELECT TB_ITEM_REVIEW.ITEM_ID FROM TB_ITEM_REVIEW
	WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'
	EXCEPT
	
	(SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
				TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
	EXCEPT
	SELECT ITEM_ID FROM @ITEM_IDS
	)
	
	
	-- USE IDX SO THAT MOST RELEVANT (LOWEST RANK) HAVE THE HIGHEST SCORE
	SELECT '0', TB_ITEM_TERM_VECTORS.ITEM_ID, VECTORS, IDS.IDX FROM TB_ITEM_TERM_VECTORS
		INNER JOIN @ITEM_IDS IDS ON IDS.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
	UNION
	-- EXCLUDED ARE ALL EQUALLY IRRELEVANT - HAVE NO WAY OF DIFFERENTIATING THEM AT THE MOMENT
	SELECT '0', TB_ITEM_TERM_VECTORS.ITEM_ID, VECTORS, -1 FROM TB_ITEM_TERM_VECTORS
		INNER JOIN @ITEM_IDS_EX IDSX ON IDSX.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
	

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingSearchWeightedTerms] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingSetScreeningCodeSet]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingSetScreeningCodeSet]
(
	@REVIEW_ID INT,
	@CODE_SET_ID INT
)

As


SET NOCOUNT ON

UPDATE TB_REVIEW
SET SCREENING_CODE_SET_ID = @CODE_SET_ID
WHERE REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingSetScreeningCodeSet] TO  SCHEMA OWNER 
GO
--/****** Object:  StoredProcedure [dbo].[st_TrainingStatistics]    Script Date: 6/15/2016 11:49:45 AM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--CREATE procedure [dbo].[st_TrainingStatistics]
--(
--	@TRAINING_ID INT,
--	@REVIEW_ID INT,
--	@TRUE_POSITIVE INT OUTPUT,
--	@FALSE_POSITIVE INT OUTPUT,
--	@TRUE_NEGATIVE INT OUTPUT,
--	@FALSE_NEGATIVE INT OUTPUT
--)

--As

--SET NOCOUNT ON
	
--declare @items_inc nvarchar(max)
--declare @items_exc nvarchar(max)

--select @items_inc = ITEMS_INC from TB_TRAINING where training_id = @training_id
--select @items_exc = ITEMS_EXC from TB_TRAINING where training_id = @training_id

---- true positives
--select @TRUE_POSITIVE = COUNT(*) from TB_ITEM_ATTRIBUTE
--inner join dbo.fn_Split_int(@items_inc, ',') items on items.value = TB_ITEM_ATTRIBUTE.ITEM_ID
--inner join TB_TRAINING_SCREENING_CRITERIA on
--		TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
--	and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @review_id
--	and TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'true'
--inner join TB_ITEM_SET on TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
--	AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'

---- false positives
--select @FALSE_POSITIVE = COUNT(*) from TB_ITEM_ATTRIBUTE
--inner join dbo.fn_Split_int(@items_inc, ',') items on items.value = TB_ITEM_ATTRIBUTE.ITEM_ID
--inner join TB_TRAINING_SCREENING_CRITERIA on
--		TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
--	and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @review_id
--	and TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'false'
--inner join TB_ITEM_SET on TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
--	AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	
---- true negatives
--select @TRUE_NEGATIVE = COUNT(*) from TB_ITEM_ATTRIBUTE
--inner join dbo.fn_Split_int(@items_exc, ',') items on items.value = TB_ITEM_ATTRIBUTE.ITEM_ID
--inner join TB_TRAINING_SCREENING_CRITERIA on
--		TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
--	and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @review_id
--	and TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'false'
--inner join TB_ITEM_SET on TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
--	AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'

---- false negatives
--select @FALSE_NEGATIVE = COUNT(*) from TB_ITEM_ATTRIBUTE
--inner join dbo.fn_Split_int(@items_exc, ',') items on items.value = TB_ITEM_ATTRIBUTE.ITEM_ID
--inner join TB_TRAINING_SCREENING_CRITERIA on
--		TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
--	and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @review_id
--	and TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'true'
--inner join TB_ITEM_SET on TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
--	AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'

--SET NOCOUNT OFF

--GO
--ALTER AUTHORIZATION ON [dbo].[st_TrainingStatistics] TO  SCHEMA OWNER 
--GO
/****** Object:  StoredProcedure [dbo].[st_TrainingTrainingSet]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingTrainingSet]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	DECLARE @INCLUDE_COUNT INT
	
	SELECT @INCLUDE_COUNT = COUNT(distinct TB_ITEM_TERM_VECTORS.ITEM_ID) FROM TB_ITEM_TERM_VECTORS
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
	WHERE TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID

	SELECT '+1', VECTORS, TB_ITEM_TERM_VECTORS.ITEM_ID FROM TB_ITEM_TERM_VECTORS
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
	WHERE TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID
		
	UNION
	
	SELECT '-1', VECTOR_S, ITEM_ID FROM (SELECT TOP(@INCLUDE_COUNT) VECTORS
		AS VECTOR_S, TB_ITEM_TERM_VECTORS.ITEM_ID FROM TB_ITEM_TERM_VECTORS
	INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_TERM_VECTORS.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
	WHERE TB_ITEM_TERM_VECTORS.REVIEW_ID = @REVIEW_ID
	ORDER BY NEWID()) AS X

SET NOCOUNT OFF

GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingTrainingSet] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingWriteDataToAzure]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingWriteDataToAzure]
(
	@REVIEW_ID INT
--,	@SCREENING_DATA_FILE NVARCHAR(50)
)

As

SET NOCOUNT ON

	DELETE FROM TB_SCREENING_ML_TEMP WHERE REVIEW_ID = @REVIEW_ID

	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '1' INCLUDED FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID
			
	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '0' INCLUDED FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_REVIEW.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '99' INCLUDED FROM TB_ITEM_REVIEW
	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		where TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND not TB_ITEM_REVIEW.ITEM_ID in
		(
			SELECT ITEM_ID FROM TB_ITEM_SET
				INNER JOIN TB_REVIEW ON TB_REVIEW.SCREENING_CODE_SET_ID = TB_ITEM_SET.SET_ID
				WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_REVIEW.REVIEW_ID = @REVIEW_ID
		)

	UPDATE TB_REVIEW
		SET SCREENING_INDEXED = 'TRUE',
			SCREENING_MODEL_RUNNING = 'TRUE'
		WHERE REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingWriteDataToAzure] TO  SCHEMA OWNER 
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingWriteIncludeExcludeToAzure]    Script Date: 6/15/2016 11:49:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_TrainingWriteIncludeExcludeToAzure]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	DELETE FROM TB_SCREENING_ML_TEMP WHERE REVIEW_ID = @REVIEW_ID

	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, '1' INCLUDED FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID
			
	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, '0' INCLUDED FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_REVIEW.ITEM_ID, '99' INCLUDED FROM TB_ITEM_REVIEW
		where TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND not TB_ITEM_REVIEW.ITEM_ID in
		(
			SELECT ITEM_ID FROM TB_ITEM_SET
				INNER JOIN TB_REVIEW ON TB_REVIEW.SCREENING_CODE_SET_ID = TB_ITEM_SET.SET_ID
				WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_REVIEW.REVIEW_ID = @REVIEW_ID
		)

	/*
	declare @tempTable table
	(
		REVIEW_ID int,
		ITEM_ID bigint,
		LABEL nvarchar(10)
	)
	
	delete from EPPI_ML.[EPPITest].dbo.TB_REVIEW_ITEM_LABELS WHERE REVIEW_ID = @REVIEW_ID

	INSERT INTO @tempTable(REVIEW_ID, ITEM_ID, LABEL)
	SELECT DISTINCT @REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, '1' FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID
			
	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, '0' FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN REVIEWER.DBO.TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID, TB_ITEM_REVIEW.ITEM_ID, '99' FROM TB_ITEM_REVIEW
		where TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND not TB_ITEM_REVIEW.ITEM_ID in
		(
			SELECT ITEM_ID FROM TB_ITEM_SET
				INNER JOIN TB_REVIEW ON TB_REVIEW.SCREENING_CODE_SET_ID = TB_ITEM_SET.SET_ID
				WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_REVIEW.REVIEW_ID = @REVIEW_ID
		)


	INSERT INTO EPPI_ML.[EPPITest].dbo.TB_REVIEW_ITEM_LABELS(REVIEW_ID, ITEM_ID, LABEL)
	select * from @tempTable
	*/
	
SET NOCOUNT OFF


GO
ALTER AUTHORIZATION ON [dbo].[st_TrainingWriteIncludeExcludeToAzure] TO  SCHEMA OWNER 
GO
