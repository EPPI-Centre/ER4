
USE [ReviewerAdmin]
GO

/*
   20 November 202011:34:31
   User: 
   Server: SSRU30
   Database: ReviewerAdmin
   Application: 
*/

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
CREATE TABLE dbo.Tmp_TB_SITE_LIC_ADMIN
	(
	SITE_LIC_ADMIN_ID int NOT NULL IDENTITY (1, 1),
	SITE_LIC_ID int NOT NULL,
	CONTACT_ID int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_TB_SITE_LIC_ADMIN SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_TB_SITE_LIC_ADMIN OFF
GO
IF EXISTS(SELECT * FROM dbo.TB_SITE_LIC_ADMIN)
	 EXEC('INSERT INTO dbo.Tmp_TB_SITE_LIC_ADMIN (SITE_LIC_ID, CONTACT_ID)
		SELECT SITE_LIC_ID, CONTACT_ID FROM dbo.TB_SITE_LIC_ADMIN WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.TB_SITE_LIC_ADMIN
GO
EXECUTE sp_rename N'dbo.Tmp_TB_SITE_LIC_ADMIN', N'TB_SITE_LIC_ADMIN', 'OBJECT' 
GO
ALTER TABLE dbo.TB_SITE_LIC_ADMIN ADD CONSTRAINT
	PK_TB_SITE_LIC_ADMIN PRIMARY KEY CLUSTERED 
	(
	SITE_LIC_ADMIN_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_SITE_LIC_ADMIN', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_SITE_LIC_ADMIN', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_SITE_LIC_ADMIN', 'Object', 'CONTROL') as Contr_Per 


GO
-------------------------------------------


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Remove_Admin]    Script Date: 18/11/2020 15:41:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Remove_Admin]
(	
	@SITE_LIC_ADMIN_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here

	delete from TB_SITE_LIC_ADMIN 
	where SITE_LIC_ADMIN_ID = @SITE_LIC_ADMIN_ID

	
END




GO

------------------------------------