USE [ReviewerAdmin]
GO

	INSERT INTO [TB_DB_VERSION]
           ([VERSION_NUMBER], [DATE_APPLIED])
     VALUES
           (-111, getdate())

GO
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
ALTER TABLE dbo.TB_DB_VERSION ADD
	FAKE_RUBBISH nvarchar(MAX) NULL
GO
ALTER TABLE dbo.TB_DB_VERSION ADD CONSTRAINT
	DF_TB_DB_VERSION_FAKE_RUBBISH DEFAULT 'rubbish' FOR FAKE_RUBBISH
GO
ALTER TABLE dbo.TB_DB_VERSION SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
GO


/****** Object:  StoredProcedure [dbo].[st_SetTypeList]    Script Date: 04/10/2018 11:39:41 ******/

DROP PROCEDURE [dbo].[st_fakeSproc]
GO

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--BEGIN TRANSACTION
--SET QUOTED_IDENTIFIER ON
--SET ARITHABORT ON
--SET NUMERIC_ROUNDABORT OFF
--SET CONCAT_NULL_YIELDS_NULL ON
--SET ANSI_NULLS ON
--SET ANSI_PADDING ON
--SET ANSI_WARNINGS ON
--COMMIT
--BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.TB_DB_VERSION
--	DROP CONSTRAINT DF_TB_DB_VERSION_FAKE_RUBBISH
--GO
--ALTER TABLE dbo.TB_DB_VERSION
--	DROP COLUMN FAKE_RUBBISH
--GO
--ALTER TABLE dbo.TB_DB_VERSION SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--GO


Use Reviewer
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE st_fakeSproc 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT 'I\''m a fake SPROC'
END
GO