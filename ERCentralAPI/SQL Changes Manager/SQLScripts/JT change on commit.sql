USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearch]    Script Date: 20/06/2022 09:50:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	gets a single magSearch on demand
-- =============================================
CREATE PROCEDURE [dbo].[st_MagSearch] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT,
	@MAG_SEARCH_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	SELECT MAG_SEARCH_ID, REVIEW_ID, C.CONTACT_ID, C.CONTACT_NAME, SEARCH_TEXT, SEARCH_NO, HITS_NO,
		SEARCH_DATE, MAG_FOLDER, MAG_SEARCH_TEXT, SEARCH_IDS_STORED, SEARCH_IDS
	FROM TB_MAG_SEARCH
	INNER JOIN TB_CONTACT C ON C.CONTACT_ID = TB_MAG_SEARCH.CONTACT_ID
	where REVIEW_ID = @REVIEW_ID AND MAG_SEARCH_ID = @MAG_SEARCH_ID
		order by SEARCH_NO desc
		
END
go

IF COL_LENGTH('dbo.TB_MAG_SEARCH', 'SEARCH_IDS_STORED') IS NULL
BEGIN
BEGIN TRANSACTION

ALTER TABLE dbo.TB_MAG_SEARCH ADD
	SEARCH_IDS_STORED bit NULL,
	SEARCH_IDS nvarchar(MAX) NULL
ALTER TABLE dbo.TB_MAG_SEARCH SET (LOCK_ESCALATION = TABLE)

COMMIT
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearchInsert]    Script Date: 20/06/2022 15:55:44 ******/
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
	@SEARCH_IDS_STORED bit = 0,
	@SEARCH_IDS NVARCHAR(MAX) = null,

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
		SEARCH_DATE, MAG_FOLDER, MAG_SEARCH_TEXT, SEARCH_IDS_STORED, SEARCH_IDS)
	VALUES(@REVIEW_ID, @CONTACT_ID, @SEARCH_TEXT, @SEARCH_NO, @HITS_NO,
		@SEARCH_DATE, @MAG_FOLDER, @MAG_SEARCH_TEXT, @SEARCH_IDS_STORED, @SEARCH_IDS)
	
	SET @MAG_SEARCH_ID = @@IDENTITY
END
GO

