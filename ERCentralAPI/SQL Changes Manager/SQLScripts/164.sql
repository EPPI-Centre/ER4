USE [Reviewer]
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_MAG_LOG')
BEGIN
DROP TABLE [dbo].[TB_MAG_LOG]
END
GO

/****** Object:  Table [dbo].[TB_MAG_LOG]    Script Date: 26/02/2020 08:21:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TB_MAG_LOG](
	[MAG_LOG_ID] [int] IDENTITY(1,1) NOT NULL,
	[TIME_SUBMITTED] [datetime] NULL,
	[CONTACT_ID] [int] NULL,
	[JOB_TYPE] [nvarchar](50) NULL,
	[JOB_STATUS] [nvarchar](50) NULL,
	[JOB_MESSAGE] [nvarchar](max) NULL,
 CONSTRAINT [PK_TB_MAG_LOG] PRIMARY KEY CLUSTERED 
(
	[MAG_LOG_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[TB_MAG_LOG]  WITH CHECK ADD  CONSTRAINT [FK_TB_MAG_LOG_TB_CONTACT] FOREIGN KEY([CONTACT_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
GO

ALTER TABLE [dbo].[TB_MAG_LOG] CHECK CONSTRAINT [FK_TB_MAG_LOG_TB_CONTACT]
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagLogList]    Script Date: 26/02/2020 08:32:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
create or alter PROCEDURE [dbo].[st_MagLogList] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select ML.CONTACT_ID, ML.JOB_MESSAGE, ML.JOB_STATUS, ML.JOB_TYPE, ML.MAG_LOG_ID, ML.TIME_SUBMITTED,
		C.CONTACT_NAME
	from TB_MAG_LOG ML
		INNER JOIN TB_CONTACT C ON C.CONTACT_ID = ML.CONTACT_ID
		order by ML.MAG_LOG_ID
		
END
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_MAG_CURRENT_INFO')
BEGIN
DROP TABLE [dbo].[TB_MAG_CURRENT_INFO]
END
GO
CREATE TABLE [dbo].[TB_MAG_CURRENT_INFO](
	[MAG_CURRENT_INFO_ID] [int] IDENTITY(1,1) NOT NULL,
	[MAG_VERSION] [nvarchar](20) NULL,
	[WHEN_LIVE] [datetime] NULL,
	[MATCHING_AVAILABLE] [bit] NULL,
	[MAG_ONLINE] [bit] NULL,
	[MAKES_ENDPOINT] [nvarchar](100) NULL,
	[MAKES_DEPLOYMENT_STATUS] [NVARCHAR](10),
 CONSTRAINT [PK_TB_MAG_CURRENT_INFO] PRIMARY KEY CLUSTERED 
(
	[MAG_CURRENT_INFO_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagCurrentInfo]    Script Date: 06/03/2020 00:20:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get information - last update and whether MAG is available
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagCurrentInfo] 
	-- Add the parameters for the stored procedure here
	@MAKES_DEPLOYMENT_STATUS nvarchar(10)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP(1) MAG_CURRENT_INFO_ID, MAG_VERSION, WHEN_LIVE, MATCHING_AVAILABLE, MAG_ONLINE, MAKES_ENDPOINT,
		MAKES_DEPLOYMENT_STATUS
	FROM TB_MAG_CURRENT_INFO
	where MAKES_DEPLOYMENT_STATUS = @MAKES_DEPLOYMENT_STATUS
	ORDER BY MAG_CURRENT_INFO_ID DESC
	
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagGetCurrentlyUsedPaperIds]    Script Date: 26/02/2020 13:32:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get the currently used PaperIds for checking deletions between MAG versions
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagGetCurrentlyUsedPaperIds] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	SELECT distinct PaperId, ITEM_ID from tb_ITEM_MAG_MATCH
		
END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagLogInsert]    Script Date: 27/02/2020 08:38:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_MAG_LOG
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagLogInsert] 
	
	@CONTACT_ID int,
	@JOB_TYPE nvarchar(50),
	@JOB_STATUS nvarchar(50),
	@JOB_MESSAGE nvarchar(max)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO TB_MAG_LOG(TIME_SUBMITTED, CONTACT_ID, JOB_TYPE, JOB_STATUS, JOB_MESSAGE)
	VALUES(GETDATE(), @CONTACT_ID, @JOB_TYPE, @JOB_STATUS, @JOB_MESSAGE)
	
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagLogList]    Script Date: 27/02/2020 09:40:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagLogList] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select ML.CONTACT_ID, ML.JOB_MESSAGE, ML.JOB_STATUS, ML.JOB_TYPE, ML.MAG_LOG_ID, ML.TIME_SUBMITTED,
		C.CONTACT_NAME
	from TB_MAG_LOG ML
		INNER JOIN TB_CONTACT C ON C.CONTACT_ID = ML.CONTACT_ID
		order by ML.MAG_LOG_ID desc
		
END
go


USE [Reviewer]
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_MAG_CHANGED_PAPER_IDS')
BEGIN
DROP TABLE [dbo].[TB_MAG_CHANGED_PAPER_IDS]
END
GO

/****** Object:  Table [dbo].[TB_MAG_CHANGED_PAPER_IDS]    Script Date: 27/02/2020 23:55:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TB_MAG_CHANGED_PAPER_IDS](
	[MagChangedPaperIdsId] [int] IDENTITY(1,1) NOT NULL,
	[OldPaperId] [bigint] NULL,
	[MagVersion] [nvarchar](20) NULL,
	[NewPaperId] [bigint] NULL,
	[ITEM_ID] [bigint] NULL,
	[NewAutoMatchScore] [float] NULL,
 CONSTRAINT [PK_TB_MAG_CHANGED_PAPER_IDS] PRIMARY KEY CLUSTERED 
(
	[MagChangedPaperIdsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagGetMissingPaperIds]    Script Date: 06/03/2020 00:35:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get the list of changed PaperIds to look up in the new deployment
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagGetMissingPaperIds] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	SELECT distinct OldPaperId, MagChangedPaperIdsId, ITEM_ID from TB_MAG_CHANGED_PAPER_IDS
		WHERE NewPaperId = -1
		
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagUpdateMissingPaperIds]    Script Date: 04/03/2020 11:03:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Update the list of changed PaperIds with a new paperid
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagUpdateMissingPaperIds] 
	@MagChangedPaperIdsId int,
	@NewAutoMatchScore float,
	@NewPaperId bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	update TB_MAG_CHANGED_PAPER_IDS
		set NewPaperId = @NewPaperId,
		NewAutoMatchScore = @NewAutoMatchScore
		where MagChangedPaperIdsId = @MagChangedPaperIdsId
		
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_Item]    Script Date: 05/03/2020 09:27:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_Item]
(
	@REVIEW_ID INT,
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON

if @REVIEW_ID > -1
begin
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
end
else
begin
	SELECT I.ITEM_ID, I.[TYPE_ID], [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENTAUTHORS,
		NULL IS_DELETED, NULL IS_INCLUDED, NULL MASTER_ITEM_ID, DOI, KEYWORDS
	FROM TB_ITEM I

	-- Not limiting to a given review
	--INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]

	WHERE I.ITEM_ID = @ITEM_ID
end

SET NOCOUNT OFF

GO

INSERT INTO TB_MAG_CURRENT_INFO(MAG_VERSION, WHEN_LIVE, MATCHING_AVAILABLE, MAG_ONLINE, MAKES_ENDPOINT, MAKES_DEPLOYMENT_STATUS)
VALUES ('30/01/2020', '2020-01-30 00:00:00.000', 1, 1, 'http://eppimag20200130.westeurope.cloudapp.azure.com', 'LIVE')
go

INSERT INTO TB_MAG_CURRENT_INFO(MAG_VERSION, WHEN_LIVE, MATCHING_AVAILABLE, MAG_ONLINE, MAKES_ENDPOINT, MAKES_DEPLOYMENT_STATUS)
VALUES ('07/02/2020', '2020-02-07 00:00:00.000', 1, 1, 'http://eppimag20200207.westeurope.cloudapp.azure.com', 'PENDING')
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagUpdateChangedIds]    Script Date: 06/03/2020 16:36:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagUpdateChangedIds] 
	-- Add the parameters for the stored procedure here
	@MagVersion nvarchar(20) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	update imm
		set PaperId = NewPaperId,
		AutoMatchScore = NewAutoMatchScore,
		ManualTrueMatch = 'false',
		ManualFalseMatch = 'false'
	from tb_ITEM_MAG_MATCH imm
		inner join TB_MAG_CHANGED_PAPER_IDS mcp on mcp.OldPaperId = imm.PaperId and mcp.ITEM_ID = imm.ITEM_ID
		where mcp.MagVersion = @MagVersion
		
END
GO