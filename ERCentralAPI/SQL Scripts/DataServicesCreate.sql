--NOTES:
--You need to edit where the new DB files will be stored, might also need to edit security manually (add logins, etc).


USE [master]
GO
/****** Object:  Database [DataService]    Script Date: 13/06/2018 10:07:33 ******/
CREATE DATABASE [DataService]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'DataService', FILENAME = N'd:\Microsoft SQL Server2008\Data\DataService.mdf' , SIZE = 54272KB , MAXSIZE = UNLIMITED, FILEGROWTH = 51200KB )
 LOG ON 
( NAME = N'DataService_log', FILENAME = N'd:\Microsoft SQL Server2008\Data\DataService_log.ldf' , SIZE = 4096KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [DataService] SET COMPATIBILITY_LEVEL = 120
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DataService].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DataService] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DataService] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DataService] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DataService] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DataService] SET ARITHABORT OFF 
GO
ALTER DATABASE [DataService] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DataService] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DataService] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DataService] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DataService] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DataService] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DataService] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DataService] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DataService] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DataService] SET  DISABLE_BROKER 
GO
ALTER DATABASE [DataService] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DataService] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DataService] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DataService] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DataService] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DataService] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [DataService] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DataService] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [DataService] SET  MULTI_USER 
GO
ALTER DATABASE [DataService] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DataService] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DataService] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DataService] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [DataService] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'DataService', N'ON'
GO
USE [DataService]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_REBUILD_AUTHORS]    Script Date: 13/06/2018 10:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION  [dbo].[fn_REBUILD_AUTHORS]

(

@id bigint,
@role tinyint = 0

)

RETURNS nvarchar(max)

   

    BEGIN

        declare @res nvarchar(max)

        declare @res2 nvarchar(max)

        DECLARE cr CURSOR FAST_FORWARD FOR SELECT [LAST] + ' ' + [FIRST] 

        FROM [tb_REFERENCE_AUTHOR]  where REFERENCE_ID = @id AND ROLE = @role

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
/****** Object:  UserDefinedFunction [dbo].[fn_REBUILD_EXTERNAL_IDS]    Script Date: 13/06/2018 10:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION  [dbo].[fn_REBUILD_EXTERNAL_IDS]

(

@id bigint

)

RETURNS nvarchar(max)

   

    BEGIN

        declare @res nvarchar(max)

        declare @res2 nvarchar(max)

        DECLARE cr CURSOR FAST_FORWARD FOR SELECT [TYPE] + '¬' + [VALUE] 

        FROM [TB_EXTERNALID]  where REFERENCE_ID = @id 

        

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
/****** Object:  UserDefinedFunction [dbo].[fn_Split]    Script Date: 13/06/2018 10:07:33 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_Split](@sText varchar(MAX), @sDelim varchar(20) = ' ')
RETURNS @retArray TABLE (idx smallint Primary Key, value varchar(8000) COLLATE Latin1_General_CI_AS)
AS
BEGIN
DECLARE @idx smallint,
	@value varchar(MAX),
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
/****** Object:  Table [dbo].[TB_EXTERNALID]    Script Date: 13/06/2018 10:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_EXTERNALID](
	[EXTERNALID_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[REFERENCE_ID] [bigint] NOT NULL,
	[TYPE] [nchar](15) NOT NULL,
	[VALUE] [nvarchar](500) NOT NULL,
 CONSTRAINT [PK_TB_EXTERNALID] PRIMARY KEY CLUSTERED 
(
	[EXTERNALID_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TB_REFERENCE]    Script Date: 13/06/2018 10:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TB_REFERENCE](
	[REFERENCE_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[TYPE_ID] [int] NOT NULL CONSTRAINT [DF_tb_REFERENCE_TYPE_ID]  DEFAULT ((0)),
	[TITLE] [nvarchar](4000) NULL CONSTRAINT [DF_tb_REFERENCE_TITLE]  DEFAULT (''),
	[PARENT_TITLE] [nvarchar](4000) NULL CONSTRAINT [DF_tb_REFERENCE_PARENT_TITLE]  DEFAULT (''),
	[SHORT_TITLE] [nvarchar](70) NULL CONSTRAINT [DF_tb_REFERENCE_SHORT_TITLE]  DEFAULT (''),
	[DATE_CREATED] [datetime] NOT NULL CONSTRAINT [DF_TB_REFERENCE_DATE_CREATED]  DEFAULT (getdate()),
	[CREATED_BY] [nvarchar](50) NULL CONSTRAINT [DF_tb_REFERENCE_CREATED_BY]  DEFAULT (''),
	[DATE_EDITED] [datetime] NOT NULL CONSTRAINT [DF_TB_REFERENCE_DATE_EDITED]  DEFAULT (getdate()),
	[EDITED_BY] [nvarchar](50) NULL,
	[PUBMED_REVISED] [datetime] NULL,
	[PUBMED_PMID_VERSION] [smallint] NULL,
	[YEAR] [varchar](50) NULL CONSTRAINT [DF_tb_REFERENCE_YEAR]  DEFAULT ((0)),
	[MONTH] [varchar](10) NULL CONSTRAINT [DF_tb_REFERENCE_MONTH]  DEFAULT ((0)),
	[STANDARD_NUMBER] [nvarchar](255) NULL CONSTRAINT [DF_tb_REFERENCE_STANDARD_NUMBER]  DEFAULT (''),
	[CITY] [nvarchar](100) NULL CONSTRAINT [DF_tb_REFERENCE_CITY]  DEFAULT (''),
	[COUNTRY] [nvarchar](100) NULL CONSTRAINT [DF_tb_REFERENCE_COUNTRY]  DEFAULT (''),
	[PUBLISHER] [nvarchar](1000) NULL CONSTRAINT [DF_tb_REFERENCE_PUBLISHER]  DEFAULT (''),
	[INSTITUTION] [nvarchar](1000) NULL CONSTRAINT [DF_tb_REFERENCE_INSTITUTION]  DEFAULT (''),
	[VOLUME] [nvarchar](56) NULL CONSTRAINT [DF_tb_REFERENCE_VOLUME]  DEFAULT (''),
	[PAGES] [nvarchar](50) NULL CONSTRAINT [DF_tb_REFERENCE_PAGES]  DEFAULT (''),
	[EDITION] [nvarchar](200) NULL CONSTRAINT [DF_tb_REFERENCE_EDITION]  DEFAULT (''),
	[ISSUE] [nvarchar](100) NULL CONSTRAINT [DF_tb_REFERENCE_ISSUE]  DEFAULT (''),
	[URLS] [nvarchar](max) NULL CONSTRAINT [DF_tb_REFERENCE_URL]  DEFAULT (''),
	[ABSTRACT] [nvarchar](max) NULL CONSTRAINT [DF_tb_REFERENCE_ABSTRACT]  DEFAULT (''),
	[COMMENTS] [nvarchar](max) NULL CONSTRAINT [DF_tb_REFERENCE_COMMENTS]  DEFAULT (''),
	[MESHTERMS] [nvarchar](max) NULL CONSTRAINT [DF_tb_REFERENCE_MESHTERMS]  DEFAULT (''),
	[KEYWORDS] [nvarchar](max) NULL CONSTRAINT [DF_tb_REFERENCE_KEYWORDS]  DEFAULT (''),
 CONSTRAINT [PK_tb_REFERENCE] PRIMARY KEY CLUSTERED 
(
	[REFERENCE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TB_REFERENCE_AUTHOR]    Script Date: 13/06/2018 10:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_REFERENCE_AUTHOR](
	[REFERENCE_AUTHOR_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[REFERENCE_ID] [bigint] NOT NULL,
	[LAST] [nvarchar](50) NOT NULL,
	[FIRST] [nvarchar](50) NOT NULL CONSTRAINT [DF_tb_REFERENCE_AUTHORS_First]  DEFAULT (''),
	[ROLE] [tinyint] NOT NULL CONSTRAINT [DF_tb_REFERENCE_AUTHOR_ROLE]  DEFAULT ((0)),
	[RANK] [smallint] NOT NULL,
 CONSTRAINT [PK_tb_REFERENCE_AUTHOR] PRIMARY KEY CLUSTERED 
(
	[REFERENCE_AUTHOR_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_tb_REFERENCE_AUTHORS] UNIQUE NONCLUSTERED 
(
	[REFERENCE_ID] ASC,
	[LAST] ASC,
	[FIRST] ASC,
	[ROLE] ASC,
	[RANK] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TB_REFERENCE_TYPE]    Script Date: 13/06/2018 10:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_REFERENCE_TYPE](
	[TYPE_ID] [int] NOT NULL,
	[TYPE_NAME] [nvarchar](50) NOT NULL,
	[DESCRIPTION] [nvarchar](200) NOT NULL CONSTRAINT [DF_tb_REFERENCE_TYPE_DESCRIPTION]  DEFAULT (''),
	[NOTES] [nvarchar](200) NOT NULL CONSTRAINT [DF_tb_REFERENCE_TYPE_NOTES]  DEFAULT (''),
 CONSTRAINT [PK_tb_REFERENCE_TYPE] PRIMARY KEY CLUSTERED 
(
	[TYPE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[TB_EXTERNALID]  WITH CHECK ADD  CONSTRAINT [FK_TB_EXTERNALID_TB_REFERENCE] FOREIGN KEY([REFERENCE_ID])
REFERENCES [dbo].[TB_REFERENCE] ([REFERENCE_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_EXTERNALID] CHECK CONSTRAINT [FK_TB_EXTERNALID_TB_REFERENCE]
GO
ALTER TABLE [dbo].[TB_REFERENCE]  WITH CHECK ADD  CONSTRAINT [FK_TB_REFERENCE_TB_REFERENCE_TYPE] FOREIGN KEY([TYPE_ID])
REFERENCES [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID])
GO
ALTER TABLE [dbo].[TB_REFERENCE] CHECK CONSTRAINT [FK_TB_REFERENCE_TB_REFERENCE_TYPE]
GO
ALTER TABLE [dbo].[TB_REFERENCE_AUTHOR]  WITH CHECK ADD  CONSTRAINT [FK_tb_REFERENCE_AUTHORS_tb_REFERENCE] FOREIGN KEY([REFERENCE_ID])
REFERENCES [dbo].[TB_REFERENCE] ([REFERENCE_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TB_REFERENCE_AUTHOR] CHECK CONSTRAINT [FK_tb_REFERENCE_AUTHORS_tb_REFERENCE]
GO
/****** Object:  StoredProcedure [dbo].[st_findCitationByExternalID]    Script Date: 13/06/2018 10:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_findCitationByExternalID]
(
	@ExternalIDName nchar(15)
	,@ExternalIDValue nvarchar(500)
)

As

SET NOCOUNT ON
declare @refid bigint
select @refid = REFERENCE_ID from TB_EXTERNALID where [TYPE] = @ExternalIDName AND [VALUE] = @ExternalIDValue

if (@refid is not null)
BEGIN
	select *
	, rt.[TYPE_NAME]
	, dbo.fn_REBUILD_AUTHORS(@refid, 0) as AUTHORS
	, dbo.fn_REBUILD_AUTHORS(@refid, 1) as PARENT_AUTHORS
	, dbo.fn_REBUILD_EXTERNAL_IDS(@refid) as EXTERNAL_IDS
	 from TB_REFERENCE r
	 inner join TB_REFERENCE_TYPE rt on r.TYPE_ID = rt.TYPE_ID
	  where r.REFERENCE_ID = @refid
END

SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[st_ReferenceAuthorDelete]    Script Date: 13/06/2018 10:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_ReferenceAuthorDelete]
(
	@REFERENCE_ID BIGINT
)

As

SET NOCOUNT ON
DELETE from TB_REFERENCE_AUTHOR where REFERENCE_ID = @REFERENCE_ID 


SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[st_ReferenceAuthorUpdate]    Script Date: 13/06/2018 10:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create procedure [dbo].[st_ReferenceAuthorUpdate]
(
	@REFERENCE_ID BIGINT
,	@RANK smallint
,	@ROLE TINYINT = 0
,	@LAST NVARCHAR(50)
,	@FIRST NVARCHAR(50) = NULL

)

As

SET NOCOUNT ON

insert into TB_REFERENCE_AUTHOR 
(	
	REFERENCE_ID
,	LAST 
,	FIRST 
,	ROLE
,	RANK 
) VALUES (
	@REFERENCE_ID
,	@LAST
,	@FIRST
,	@ROLE
,	@RANK
)

SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[st_ReferenceInsert]    Script Date: 13/06/2018 10:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ReferenceInsert]
	-- Add the parameters for the stored procedure here
	(
	@REFERENCE_ID BIGINT OUTPUT
	,	@TITLE NVARCHAR(4000) = NULL
	,	@TYPE_ID TINYINT
	,	@PARENT_TITLE NVARCHAR(4000)
	,	@SHORT_TITLE NVARCHAR(70)
	,	@CREATED_BY NVARCHAR(50) = NULL
	,	@YEAR varchar(50) = NULL
	,	@MONTH NVARCHAR(10) = NULL
	,	@STANDARD_NUMBER NVARCHAR(255) = NULL
	,	@CITY NVARCHAR(100) = NULL
	,	@COUNTRY NVARCHAR(100) = NULL
	,	@PUBLISHER NVARCHAR(1000) = NULL
	,	@VOLUME NVARCHAR(56) = NULL
	,	@PAGES NVARCHAR(50) = NULL
	,	@EDITION NVARCHAR(200) = NULL
	,	@ISSUE NVARCHAR(100) = NULL
	,	@URLS NVARCHAR(MAX) = NULL
	,	@ABSTRACT NVARCHAR(MAX) = NULL
	,	@KEYWORDS NVARCHAR(MAX) = NULL
	,   @EXTERNAL_IDS NVARCHAR(MAX) = NULL
	)
	

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Insert into TB_REFERENCE (TITLE, [TYPE_ID], PARENT_TITLE, SHORT_TITLE, CREATED_BY, EDITED_BY,
	[YEAR],[MONTH],STANDARD_NUMBER,CITY,COUNTRY,PUBLISHER,VOLUME,PAGES,EDITION,ISSUE,URLS,
	ABSTRACT,KEYWORDS)
	VALUES (@TITLE,@TYPE_ID,@PARENT_TITLE,@SHORT_TITLE,@CREATED_BY,@CREATED_BY,
		@YEAR,@MONTH,@STANDARD_NUMBER,@CITY,@COUNTRY,@PUBLISHER,@VOLUME,@PAGES,@EDITION,@ISSUE,@URLS,
		@ABSTRACT,@KEYWORDS)

	SET  @REFERENCE_ID = @@IDENTITY
	if (@EXTERNAL_IDS is not null AND LEN(@EXTERNAL_IDS) > 0)
	BEGIN
		declare @t TABLE (name varchar(8000), value varchar(8000))
		insert into 
			@t select distinct SUBSTRING(value,1, CHARINDEX('¬',value)-1), LTRIM(SUBSTRING(value, CHARINDEX('¬',value) + 1, LEN(value) - CHARINDEX('¬',value))) 
			from dbo.fn_Split(@EXTERNAL_IDS, '; ')
		insert into TB_EXTERNALID (REFERENCE_ID, [TYPE], [VALUE]) select @REFERENCE_ID, t.name, t.value from @t as t
	END
	SET NOCOUNT OFF
END

GO
/****** Object:  StoredProcedure [dbo].[st_ReferenceUpdate]    Script Date: 13/06/2018 10:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ReferenceUpdate]
	-- Add the parameters for the stored procedure here
	(
	@REFERENCE_ID BIGINT OUTPUT
	,	@TITLE NVARCHAR(4000) = NULL
	,	@TYPE_ID TINYINT
	,	@PARENT_TITLE NVARCHAR(4000)
	,	@SHORT_TITLE NVARCHAR(70)
	,	@CREATED_BY NVARCHAR(50) = NULL
	,	@YEAR varchar(50) = NULL
	,	@MONTH NVARCHAR(10) = NULL
	,	@STANDARD_NUMBER NVARCHAR(255) = NULL
	,	@CITY NVARCHAR(100) = NULL
	,	@COUNTRY NVARCHAR(100) = NULL
	,	@PUBLISHER NVARCHAR(1000) = NULL
	,	@VOLUME NVARCHAR(56) = NULL
	,	@PAGES NVARCHAR(50) = NULL
	,	@EDITION NVARCHAR(200) = NULL
	,	@ISSUE NVARCHAR(100) = NULL
	,	@URLS NVARCHAR(MAX) = NULL
	,	@ABSTRACT NVARCHAR(MAX) = NULL
	,	@KEYWORDS NVARCHAR(MAX) = NULL
	,   @EXTERNAL_IDS NVARCHAR(MAX) = NULL
	)
	

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	UPDATE TB_REFERENCE 
		set TITLE = @TITLE
		, [TYPE_ID] = @TYPE_ID
		, PARENT_TITLE = @PARENT_TITLE
		, SHORT_TITLE = @SHORT_TITLE
		, CREATED_BY = @CREATED_BY
		, EDITED_BY = @CREATED_BY
		, [YEAR] = @YEAR
		, [MONTH] = @MONTH
		, STANDARD_NUMBER = @STANDARD_NUMBER
		, CITY = @CITY
		, COUNTRY = @COUNTRY
		, PUBLISHER = @PUBLISHER
		, VOLUME = @VOLUME
		, PAGES = @PAGES
		, EDITION = @EDITION
		, ISSUE = @ISSUE
		, URLS = @URLS
		, ABSTRACT = @ABSTRACT
		, KEYWORDS = @KEYWORDS
	WHERE REFERENCE_ID = @REFERENCE_ID

	if (@EXTERNAL_IDS is not null AND LEN(@EXTERNAL_IDS) > 0)
	BEGIN
		declare @t TABLE (name varchar(8000), value varchar(8000))
		insert into 
			@t select distinct SUBSTRING(value,1, CHARINDEX('¬',value)-1), LTRIM(SUBSTRING(value, CHARINDEX('¬',value) + 1, LEN(value) - CHARINDEX('¬',value))) 
			from dbo.fn_Split(@EXTERNAL_IDS, '; ')
		--we'll remove the external IDs that are coming in right now, then re-add them, perhaps better to update?
		delete from TB_EXTERNALID 
			from TB_EXTERNALID eid inner join @t t on eid.REFERENCE_ID = @REFERENCE_ID and eid.[TYPE] = t.name
		insert into TB_EXTERNALID (REFERENCE_ID, [TYPE], [VALUE]) select @REFERENCE_ID, t.name, t.value from @t as t
	END
	SET NOCOUNT OFF
END

GO
USE [master]
GO
ALTER DATABASE [DataService] SET  READ_WRITE 
GO

use [DataService]
GO

INSERT [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (1, N'Report', N'Old Code was E', N'Consider using Dissertation or Research Project when appropriate')
INSERT [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (2, N'Book, Whole', N'', N'')
INSERT [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (3, N'Book, Chapter', N'', N'')
INSERT [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (4, N'Dissertation', N'Old Code Was G', N'Use the Edition field to specify the thesis type')
INSERT [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (5, N'Conference Proceedings', N'Old Code Was K', N'')
INSERT [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (6, N'Document From Internet Site', N'This is a single document', N'Date fields refer to publication date, if known')
INSERT [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (7, N'Web Site', N'This is a multi-page website ', N'URL field should contain the root of the site hierarchy, date fields should refer to last visited date')
INSERT [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (8, N'DVD, Video, Media', N'', N'')
INSERT [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (9, N'Research project', N'', N'')
INSERT [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (10, N'Article In A Periodical', N'Newspapers, magazines and similar. Old codes are "PER" and F(newspaper)', N'')
INSERT [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (11, N'Interview', N'Old code was INT', N'Author role = 0 is for the Interviewer, Author role = 1 for the Interviewee')
INSERT [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (12, N'Generic', N'Whatever does not fit any of the above, only old code that fits here is H(trade catalogue)', N'')
INSERT [dbo].[TB_REFERENCE_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (14, N'Journal, Article', N'Used also for old "Journal, Whole", D(Journal short form) and "JOUR" codes', N'')

GO