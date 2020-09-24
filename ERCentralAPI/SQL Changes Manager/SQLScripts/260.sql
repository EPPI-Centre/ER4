
USE [Reviewer]
GO 

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = N'TB_MAG_SEARCH'))
begin
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
ALTER TABLE dbo.TB_CONTACT SET (LOCK_ESCALATION = TABLE)
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo.TB_REVIEW SET (LOCK_ESCALATION = TABLE)
COMMIT
BEGIN TRANSACTION
CREATE TABLE dbo.TB_MAG_SEARCH
	(
	MAG_SEARCH_ID int NOT NULL IDENTITY (1, 1),
	REVIEW_ID int NULL,
	CONTACT_ID int NULL,
	SEARCH_TEXT nvarchar(MAX) NULL,
	SEARCH_NO int NULL,
	HITS_NO int NULL,
	SEARCH_DATE datetime NULL,
	MAG_VERSION nvarchar(10) NULL,
	MAG_SEARCH_TEXT nvarchar(MAX) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
ALTER TABLE dbo.TB_MAG_SEARCH ADD CONSTRAINT
	PK_TB_MAG_SEARCH PRIMARY KEY CLUSTERED 
	(
	MAG_SEARCH_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.TB_MAG_SEARCH ADD CONSTRAINT
	FK_TB_MAG_SEARCH_TB_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.TB_MAG_SEARCH ADD CONSTRAINT
	FK_TB_MAG_SEARCH_TB_CONTACT FOREIGN KEY
	(
	CONTACT_ID
	) REFERENCES dbo.TB_CONTACT
	(
	CONTACT_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.TB_MAG_SEARCH SET (LOCK_ESCALATION = TABLE)
COMMIT
end
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearchList]    Script Date: 22/08/2020 15:43:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagSearchList] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	SELECT MAG_SEARCH_ID, REVIEW_ID, C.CONTACT_ID, C.CONTACT_NAME, SEARCH_TEXT, SEARCH_NO, HITS_NO, SEARCH_DATE, MAG_VERSION, MAG_SEARCH_TEXT
	FROM TB_MAG_SEARCH
	INNER JOIN TB_CONTACT C ON C.CONTACT_ID = TB_MAG_SEARCH.CONTACT_ID
	where REVIEW_ID = @REVIEW_ID
		order by SEARCH_NO desc
		
END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearchDelete]    Script Date: 22/08/2020 15:43:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagSearchDelete] 
	-- Add the parameters for the stored procedure here
	@MAG_SEARCH_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	DELETE FROM TB_MAG_SEARCH
		WHERE MAG_SEARCH_ID = @MAG_SEARCH_ID
		
END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearchInsert]    Script Date: 23/08/2020 08:26:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_MAG_LOG
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagSearchInsert] 
	@REVIEW_ID INT,
	@CONTACT_ID int,
	@SEARCH_TEXT NVARCHAR(MAX) = NULL,
	@SEARCH_NO INT = 0,
	@HITS_NO INT = 0,
	@SEARCH_DATE DATETIME,
	@MAG_VERSION NVARCHAR(10),
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
		SEARCH_DATE, MAG_VERSION, MAG_SEARCH_TEXT)
	VALUES(@REVIEW_ID, @CONTACT_ID, @SEARCH_TEXT, @SEARCH_NO, @HITS_NO,
		@SEARCH_DATE, @MAG_VERSION, @MAG_SEARCH_TEXT)
	
	SET @MAG_SEARCH_ID = @@IDENTITY
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagGetCurrentlyUsedPaperIds]    Script Date: 23/08/2020 18:23:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get the currently used PaperIds for checking deletions between MAG versions
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagGetCurrentlyUsedPaperIds] 
	@REVIEW_ID INT = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	IF @REVIEW_ID = 0
		SELECT distinct PaperId, ITEM_ID from tb_ITEM_MAG_MATCH
	ELSE
		SELECT distinct imm.PaperId, imm.ITEM_ID from tb_ITEM_MAG_MATCH imm
			inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false'
			WHERE imm.REVIEW_ID = @REVIEW_ID
		
END
go