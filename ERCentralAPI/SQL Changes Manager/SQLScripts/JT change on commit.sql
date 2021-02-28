USE REVIEWER

IF COL_LENGTH('dbo.TB_MAG_CURRENT_INFO', 'MAG_FOLDER') IS NULL
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

ALTER TABLE dbo.TB_MAG_CURRENT_INFO ADD
	MAG_FOLDER nvarchar(15) NULL
ALTER TABLE dbo.TB_MAG_CURRENT_INFO SET (LOCK_ESCALATION = TABLE)
COMMIT

UPDATE TB_MAG_CURRENT_INFO
	SET MAG_FOLDER = 
			concat('mag-'
,	DATEPART(year,convert(datetime, mag_version, 103)),'-'
,	right('0'+right(datepart(month,convert(datetime, mag_version, 103)),2),2),'-'
,	right('0'+right(datepart(day,convert(datetime, mag_version, 103)),2),2))
end
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagCurrentInfoInsert]    Script Date: 27/02/2021 21:24:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get information - last update and whether MAG is available
-- =============================================
ALTER   PROCEDURE [dbo].[st_MagCurrentInfoInsert] 
	-- Add the parameters for the stored procedure here
	@MAG_FOLDER NVARCHAR(20)
,	@WHEN_LIVE DATETIME
,	@MAKES_DEPLOYMENT_STATUS NVARCHAR(10)
,	@MAKES_ENDPOINT NVARCHAR(100)
,	@MAG_ONLINE BIT
,	@MAG_CURRENT_INFO_ID INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO TB_MAG_CURRENT_INFO (MAG_VERSION, WHEN_LIVE, MATCHING_AVAILABLE, MAG_ONLINE, MAKES_ENDPOINT, MAKES_DEPLOYMENT_STATUS)
	VALUES (@MAG_FOLDER, @WHEN_LIVE, 'TRUE', @MAG_ONLINE, @MAKES_ENDPOINT, @MAKES_DEPLOYMENT_STATUS)
	
	SET @MAG_CURRENT_INFO_ID = @@ROWCOUNT
END
go
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagCurrentInfo]    Script Date: 27/02/2021 21:25:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get information - last update and whether MAG is available
-- =============================================
ALTER PROCEDURE [dbo].[st_MagCurrentInfo] 
	-- Add the parameters for the stored procedure here
	@MAKES_DEPLOYMENT_STATUS nvarchar(10)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP(1) MAG_CURRENT_INFO_ID, MAG_FOLDER, WHEN_LIVE, MATCHING_AVAILABLE, MAG_ONLINE, MAKES_ENDPOINT,
		MAKES_DEPLOYMENT_STATUS
	FROM TB_MAG_CURRENT_INFO
	where MAKES_DEPLOYMENT_STATUS = @MAKES_DEPLOYMENT_STATUS
	ORDER BY MAG_CURRENT_INFO_ID DESC
	
END
go

IF COL_LENGTH('dbo.TB_MAG_SEARCH', 'MAG_FOLDER') IS NULL
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

ALTER TABLE dbo.TB_MAG_SEARCH ADD
	MAG_FOLDER nvarchar(15) NULL
ALTER TABLE dbo.TB_MAG_SEARCH SET (LOCK_ESCALATION = TABLE)
COMMIT
UPDATE TB_MAG_SEARCH
	SET MAG_FOLDER = 
			concat('mag-'
,	DATEPART(year,convert(datetime, mag_version, 103)),'-'
,	right('0'+right(datepart(month,convert(datetime, mag_version, 103)),2),2),'-'
,	right('0'+right(datepart(day,convert(datetime, mag_version, 103)),2),2))

end
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearchInsert]    Script Date: 27/02/2021 22:01:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_MAG_LOG
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSearchInsert] 
	@REVIEW_ID INT,
	@CONTACT_ID int,
	@SEARCH_TEXT NVARCHAR(MAX) = NULL,
	@SEARCH_NO INT = 0,
	@HITS_NO INT = 0,
	@SEARCH_DATE DATETIME,
	@MAG_FOLDER NVARCHAR(15),
	@MAG_SEARCH_TEXT NVARCHAR(MAX),

	@MAG_SEARCH_ID INT OUTPUT
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT @SEARCH_NO = MAX(SEARCH_NO) + 1 FROM TB_MAG_SEARCH
		WHERE REVIEW_ID = @REVIEW_ID

	IF @SEARCH_NO IS NULL
		SET @SEARCH_NO = 1

    INSERT INTO TB_MAG_SEARCH(REVIEW_ID, CONTACT_ID, SEARCH_TEXT, SEARCH_NO, HITS_NO,
		SEARCH_DATE, MAG_FOLDER, MAG_SEARCH_TEXT)
	VALUES(@REVIEW_ID, @CONTACT_ID, @SEARCH_TEXT, @SEARCH_NO, @HITS_NO,
		@SEARCH_DATE, @MAG_FOLDER, @MAG_SEARCH_TEXT)
	
	SET @MAG_SEARCH_ID = @@IDENTITY
END
go
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearchList]    Script Date: 27/02/2021 22:08:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSearchList] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	SELECT MAG_SEARCH_ID, REVIEW_ID, C.CONTACT_ID, C.CONTACT_NAME, SEARCH_TEXT, SEARCH_NO, HITS_NO,
		SEARCH_DATE, MAG_FOLDER, MAG_SEARCH_TEXT
	FROM TB_MAG_SEARCH
	INNER JOIN TB_CONTACT C ON C.CONTACT_ID = TB_MAG_SEARCH.CONTACT_ID
	where REVIEW_ID = @REVIEW_ID
		order by SEARCH_NO desc
		
END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagCurrentInfoList]    Script Date: 27/02/2021 22:11:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get information - last update and whether MAG is available
-- =============================================
ALTER   PROCEDURE [dbo].[st_MagCurrentInfoList] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT MAG_CURRENT_INFO_ID, MAG_FOLDER, WHEN_LIVE, MATCHING_AVAILABLE, MAG_ONLINE, MAKES_ENDPOINT,
		MAKES_DEPLOYMENT_STATUS
	FROM TB_MAG_CURRENT_INFO
	
	ORDER BY MAG_CURRENT_INFO_ID DESC
	
END
GO
