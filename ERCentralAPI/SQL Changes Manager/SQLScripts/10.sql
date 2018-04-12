
USE [Reviewer]
GO

/*
   15 March 201812:03:52
   User: 
   Server: SSRU30
   Database: Reviewer
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
ALTER TABLE dbo.TB_REVIEW
	DROP CONSTRAINT DF_TB_REVIEW_MONTHS_CREDIT
GO
ALTER TABLE dbo.TB_REVIEW
	DROP CONSTRAINT DF_TB_REVIEW_SHOW_SCREENING
GO
ALTER TABLE dbo.TB_REVIEW
	DROP CONSTRAINT DF_TB_REVIEW_ALLOW_REVIEWER_TERMS
GO
ALTER TABLE dbo.TB_REVIEW
	DROP CONSTRAINT DF_TB_REVIEW_ALLOW_CLUSTERED_SEARCH
GO
CREATE TABLE dbo.Tmp_TB_REVIEW
	(
	REVIEW_ID int NOT NULL IDENTITY (1, 1),
	REVIEW_NAME nvarchar(1000) NULL,
	OLD_REVIEW_ID nvarchar(50) NULL,
	OLD_REVIEW_GROUP_ID nvarchar(50) NULL,
	DATE_CREATED datetime NULL,
	EXPIRY_DATE date NULL,
	MONTHS_CREDIT smallint NOT NULL,
	FUNDER_ID int NULL,
	ORGANISATION_ID int NOT NULL,
	REVIEW_NUMBER nvarchar(50) NULL,
	SHOW_SCREENING bit NOT NULL,
	ALLOW_REVIEWER_TERMS bit NOT NULL,
	ALLOW_CLUSTERED_SEARCH bit NOT NULL,
	SCREENING_CODE_SET_ID int NULL,
	ARCHIE_ID char(18) NULL,
	ARCHIE_CD nchar(8) NULL,
	IS_CHECKEDOUT_HERE bit NULL,
	CHECKED_OUT_BY int NULL,
	BL_ACCOUNT_CODE nvarchar(50) NULL,
	BL_AUTH_CODE nvarchar(50) NULL,
	BL_TX nvarchar(50) NULL,
	BL_CC_ACCOUNT_CODE nvarchar(50) NULL,
	BL_CC_AUTH_CODE nvarchar(50) NULL,
	BL_CC_TX nvarchar(50) NULL,
	SCREENING_MODE nvarchar(10) NULL,
	SCREENING_WHAT_ATTRIBUTE_ID bigint NULL,
	SCREENING_N_PEOPLE int NULL,
	SCREENING_RECONCILLIATION nvarchar(10) NULL,
	SCREENING_AUTO_EXCLUDE bit NULL,
	SCREENING_MODEL_RUNNING bit NULL,
	SCREENING_INDEXED bit NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_TB_REVIEW SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_TB_REVIEW ADD CONSTRAINT
	DF_TB_REVIEW_MONTHS_CREDIT DEFAULT ((0)) FOR MONTHS_CREDIT
GO
ALTER TABLE dbo.Tmp_TB_REVIEW ADD CONSTRAINT
	DF_TB_REVIEW_ORGANISATION_ID DEFAULT 0 FOR ORGANISATION_ID
GO
ALTER TABLE dbo.Tmp_TB_REVIEW ADD CONSTRAINT
	DF_TB_REVIEW_SHOW_SCREENING DEFAULT ((0)) FOR SHOW_SCREENING
GO
ALTER TABLE dbo.Tmp_TB_REVIEW ADD CONSTRAINT
	DF_TB_REVIEW_ALLOW_REVIEWER_TERMS DEFAULT ((0)) FOR ALLOW_REVIEWER_TERMS
GO
ALTER TABLE dbo.Tmp_TB_REVIEW ADD CONSTRAINT
	DF_TB_REVIEW_ALLOW_CLUSTERED_SEARCH DEFAULT ((0)) FOR ALLOW_CLUSTERED_SEARCH
GO
SET IDENTITY_INSERT dbo.Tmp_TB_REVIEW ON
GO
IF EXISTS(SELECT * FROM dbo.TB_REVIEW)
	 EXEC('INSERT INTO dbo.Tmp_TB_REVIEW (REVIEW_ID, REVIEW_NAME, OLD_REVIEW_ID, OLD_REVIEW_GROUP_ID, DATE_CREATED, EXPIRY_DATE, MONTHS_CREDIT, FUNDER_ID, REVIEW_NUMBER, SHOW_SCREENING, ALLOW_REVIEWER_TERMS, ALLOW_CLUSTERED_SEARCH, SCREENING_CODE_SET_ID, ARCHIE_ID, ARCHIE_CD, IS_CHECKEDOUT_HERE, CHECKED_OUT_BY, BL_ACCOUNT_CODE, BL_AUTH_CODE, BL_TX, BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX, SCREENING_MODE, SCREENING_WHAT_ATTRIBUTE_ID, SCREENING_N_PEOPLE, SCREENING_RECONCILLIATION, SCREENING_AUTO_EXCLUDE, SCREENING_MODEL_RUNNING, SCREENING_INDEXED)
		SELECT REVIEW_ID, REVIEW_NAME, OLD_REVIEW_ID, OLD_REVIEW_GROUP_ID, DATE_CREATED, EXPIRY_DATE, MONTHS_CREDIT, FUNDER_ID, REVIEW_NUMBER, SHOW_SCREENING, ALLOW_REVIEWER_TERMS, ALLOW_CLUSTERED_SEARCH, SCREENING_CODE_SET_ID, ARCHIE_ID, ARCHIE_CD, IS_CHECKEDOUT_HERE, CHECKED_OUT_BY, BL_ACCOUNT_CODE, BL_AUTH_CODE, BL_TX, BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX, SCREENING_MODE, SCREENING_WHAT_ATTRIBUTE_ID, SCREENING_N_PEOPLE, SCREENING_RECONCILLIATION, SCREENING_AUTO_EXCLUDE, SCREENING_MODEL_RUNNING, SCREENING_INDEXED FROM dbo.TB_REVIEW WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_TB_REVIEW OFF
GO
ALTER TABLE dbo.TB_TRAINING
	DROP CONSTRAINT FK_TB_TRAINING_TB_REVIEW
GO
ALTER TABLE dbo.TB_SITE_LIC_REVIEW
	DROP CONSTRAINT FK_TB_SITE_LIC_REVIEW_TB_REVIEW
GO
ALTER TABLE dbo.TB_ITEM_REVIEW
	DROP CONSTRAINT FK_tb_ITEM_REVIEW_tb_REVIEW
GO
ALTER TABLE dbo.TB_META_ANALYSIS
	DROP CONSTRAINT FK_TB_META_ANALYSIS_TB_REVIEW
GO
ALTER TABLE dbo.TB_DIAGRAM
	DROP CONSTRAINT FK_TB_DIAGRAM_tb_REVIEW
GO
ALTER TABLE dbo.TB_REPORT
	DROP CONSTRAINT FK_TB_REPORT_TB_REVIEW
GO
ALTER TABLE dbo.TB_SEARCH
	DROP CONSTRAINT FK_TB_SEARCH_tb_REVIEW
GO
ALTER TABLE dbo.TB_REVIEW_CONTACT
	DROP CONSTRAINT FK_TB_REVIEW_CONTACT_tb_REVIEW
GO
ALTER TABLE dbo.TB_WORK_ALLOCATION
	DROP CONSTRAINT FK_TB_WORK_ALLOCATION_TB_REVIEW
GO
ALTER TABLE dbo.TB_REVIEW_SET
	DROP CONSTRAINT FK_TB_REVIEW_SET_tb_REVIEW
GO
ALTER TABLE dbo.tb_COMPARISON
	DROP CONSTRAINT FK_tb_COMPARISON_TB_REVIEW
GO
ALTER TABLE dbo.TB_ITEM_DUPLICATES
	DROP CONSTRAINT FK_TB_ITEM_DUPLICATES_TB_REVIEW
GO
ALTER TABLE dbo.tb_CLASSIFIER_MODEL
	DROP CONSTRAINT FK_tb_CLASSIFIER_MODEL_TB_REVIEW
GO
ALTER TABLE dbo.TB_TERMINE_LOG
	DROP CONSTRAINT FK_TB_TERMINE_LOG_TB_REVIEW
GO
ALTER TABLE dbo.TB_ITEM_DUPLICATE_GROUP
	DROP CONSTRAINT FK_TB_ITEM_DUPLICATE_GROUP_TB_REVIEW
GO
ALTER TABLE dbo.TB_SOURCE
	DROP CONSTRAINT FK_TB_SOURCE_tb_REVIEW
GO
DROP TABLE dbo.TB_REVIEW
GO
EXECUTE sp_rename N'dbo.Tmp_TB_REVIEW', N'TB_REVIEW', 'OBJECT' 
GO
ALTER TABLE dbo.TB_REVIEW ADD CONSTRAINT
	PK_tb_REVIEW PRIMARY KEY CLUSTERED 
	(
	REVIEW_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_REVIEW', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_REVIEW', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_REVIEW', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_SOURCE WITH NOCHECK ADD CONSTRAINT
	FK_TB_SOURCE_tb_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_SOURCE SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_SOURCE', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_SOURCE', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_SOURCE', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_ITEM_DUPLICATE_GROUP ADD CONSTRAINT
	FK_TB_ITEM_DUPLICATE_GROUP_TB_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_ITEM_DUPLICATE_GROUP SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_ITEM_DUPLICATE_GROUP', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_ITEM_DUPLICATE_GROUP', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_ITEM_DUPLICATE_GROUP', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_TERMINE_LOG ADD CONSTRAINT
	FK_TB_TERMINE_LOG_TB_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_TERMINE_LOG SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_TERMINE_LOG', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_TERMINE_LOG', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_TERMINE_LOG', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.tb_CLASSIFIER_MODEL ADD CONSTRAINT
	FK_tb_CLASSIFIER_MODEL_TB_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.tb_CLASSIFIER_MODEL SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.tb_CLASSIFIER_MODEL', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.tb_CLASSIFIER_MODEL', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.tb_CLASSIFIER_MODEL', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_ITEM_DUPLICATES ADD CONSTRAINT
	FK_TB_ITEM_DUPLICATES_TB_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.TB_ITEM_DUPLICATES SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_ITEM_DUPLICATES', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_ITEM_DUPLICATES', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_ITEM_DUPLICATES', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.tb_COMPARISON ADD CONSTRAINT
	FK_tb_COMPARISON_TB_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.tb_COMPARISON SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.tb_COMPARISON', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.tb_COMPARISON', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.tb_COMPARISON', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_REVIEW_SET ADD CONSTRAINT
	FK_TB_REVIEW_SET_tb_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_REVIEW_SET SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_REVIEW_SET', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_REVIEW_SET', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_REVIEW_SET', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_WORK_ALLOCATION ADD CONSTRAINT
	FK_TB_WORK_ALLOCATION_TB_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_WORK_ALLOCATION SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_WORK_ALLOCATION', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_WORK_ALLOCATION', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_WORK_ALLOCATION', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_REVIEW_CONTACT ADD CONSTRAINT
	FK_TB_REVIEW_CONTACT_tb_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_REVIEW_CONTACT SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_REVIEW_CONTACT', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_REVIEW_CONTACT', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_REVIEW_CONTACT', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_SEARCH ADD CONSTRAINT
	FK_TB_SEARCH_tb_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_SEARCH SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_SEARCH', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_SEARCH', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_SEARCH', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_REPORT ADD CONSTRAINT
	FK_TB_REPORT_TB_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_REPORT SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_REPORT', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_REPORT', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_REPORT', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_DIAGRAM ADD CONSTRAINT
	FK_TB_DIAGRAM_tb_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_DIAGRAM SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_DIAGRAM', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_DIAGRAM', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_DIAGRAM', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_META_ANALYSIS ADD CONSTRAINT
	FK_TB_META_ANALYSIS_TB_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_META_ANALYSIS SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_META_ANALYSIS', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_META_ANALYSIS', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_META_ANALYSIS', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_ITEM_REVIEW ADD CONSTRAINT
	FK_tb_ITEM_REVIEW_tb_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_ITEM_REVIEW SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_ITEM_REVIEW', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_ITEM_REVIEW', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_ITEM_REVIEW', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_SITE_LIC_REVIEW ADD CONSTRAINT
	FK_TB_SITE_LIC_REVIEW_TB_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_SITE_LIC_REVIEW SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_SITE_LIC_REVIEW', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_SITE_LIC_REVIEW', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_SITE_LIC_REVIEW', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_TRAINING ADD CONSTRAINT
	FK_TB_TRAINING_TB_REVIEW FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_TRAINING SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_TRAINING', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_TRAINING', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_TRAINING', 'Object', 'CONTROL') as Contr_Per 

/*************************************************************************************/

USE [ReviewerAdmin]
GO

/*
   15 March 201814:10:42
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
CREATE TABLE dbo.TB_ORGANISATION_ADMIN
	(
	ORGANISATION_ADMIN_ID int NOT NULL IDENTITY (1, 1),
	ORGANISATION_ID int NULL,
	CONTACT_ID int NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TB_ORGANISATION_ADMIN ADD CONSTRAINT
	PK_TB_ORGANISATION_ADMIN PRIMARY KEY CLUSTERED 
	(
	ORGANISATION_ADMIN_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.TB_ORGANISATION_ADMIN SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_ORGANISATION_ADMIN', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION_ADMIN', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION_ADMIN', 'Object', 'CONTROL') as Contr_Per 


/****************************************************************************************/

USE [ReviewerAdmin]
GO

/*
   15 March 201814:21:02
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
CREATE TABLE dbo.TB_ORGANISATION
	(
	ORGANISATION_ID int NOT NULL IDENTITY (1, 1),
	ORGANISATION_NAME nvarchar(100) NOT NULL,
	ORGANISATION_ADDRESS nvarchar(500) NOT NULL,
	TELEPHONE nvarchar(50) NULL,
	NOTES nvarchar(4000) NULL,
	CREATOR_ID int NOT NULL,
	DATE_CREATED datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TB_ORGANISATION ADD CONSTRAINT
	PK_TB_ORGANISATION PRIMARY KEY CLUSTERED 
	(
	ORGANISATION_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.TB_ORGANISATION SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_ORGANISATION', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION', 'Object', 'CONTROL') as Contr_Per 


/****************************************************************************************/

USE [ReviewerAdmin]
GO

/*
   15 March 201814:24:10
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
CREATE TABLE dbo.TB_ORGANISATION_CONTACT
	(
	ORGANISATION_ID int NOT NULL,
	CONTACT_ID int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TB_ORGANISATION_CONTACT SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_ORGANISATION_CONTACT', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION_CONTACT', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION_CONTACT', 'Object', 'CONTROL') as Contr_Per 

/****************************************************************************************/

USE [ReviewerAdmin]
GO

/*
   15 March 201814:28:58
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
CREATE TABLE dbo.TB_ORGANISATION_REVIEW
	(
	ORGANISATION_ID int NOT NULL,
	REVIEW_ID int NOT NULL,
	DATE_ADDED datetime NOT NULL,
	ADDED_BY int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TB_ORGANISATION_REVIEW SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_ORGANISATION_REVIEW', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION_REVIEW', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION_REVIEW', 'Object', 'CONTROL') as Contr_Per 

/**************************************************************************************/

USE [ReviewerAdmin]
GO

/*
   15 March 201814:33:27
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
ALTER TABLE dbo.TB_ORGANISATION ADD
	IS_PUBLIC bit NOT NULL CONSTRAINT DF_TB_ORGANISATION_IS_PUBLIC DEFAULT 1
GO
ALTER TABLE dbo.TB_ORGANISATION SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_ORGANISATION', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION', 'Object', 'CONTROL') as Contr_Per 

/*************************************************************************************/

USE [ReviewerAdmin]
GO

/*
   15 March 201814:41:08
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
CREATE TABLE dbo.TB_ORGANISATION_REQUEST
	(
	ORGANISATION_REQUEST_ID int NOT NULL IDENTITY (1, 1),
	ORGANISATION_ID int NOT NULL,
	CONTACT_ID int NOT NULL,
	DATE_REQUESTED datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TB_ORGANISATION_REQUEST ADD CONSTRAINT
	PK_TB_ORGANISATION_REQUEST PRIMARY KEY CLUSTERED 
	(
	ORGANISATION_REQUEST_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.TB_ORGANISATION_REQUEST SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_ORGANISATION_REQUEST', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION_REQUEST', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION_REQUEST', 'Object', 'CONTROL') as Contr_Per 

/***************************************************************************************/

USE [ReviewerAdmin]
GO

/*
   19 March 201814:54:37
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
CREATE TABLE dbo.TB_ORGANISATION_LOG
	(
	ORGANISATION_LOG_ID int NOT NULL IDENTITY (1, 1),
	ORGANISATION_ID int NOT NULL,
	CONTACT_ID int NOT NULL,
	AFFECTED_ID int NOT NULL,
	CHANGE_TYPE nvarchar(50) NOT NULL,
	DATE datetime NOT NULL,
	REASON nvarchar(2000) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TB_ORGANISATION_LOG ADD CONSTRAINT
	PK_TB_ORGANISATION_LOG PRIMARY KEY CLUSTERED 
	(
	ORGANISATION_LOG_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.TB_ORGANISATION_LOG SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_ORGANISATION_LOG', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION_LOG', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION_LOG', 'Object', 'CONTROL') as Contr_Per 

/**************************************************************************************/

USE [ReviewerAdmin]
GO

/*
   19 March 201816:22:29
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
CREATE TABLE dbo.TB_ORGANSATION_LOG
	(
	ORGANISATION_LOG_ID int NOT NULL IDENTITY (1, 1),
	ORGANISATION_ID int NOT NULL,
	CONTACT_ID int NOT NULL,
	AFFECTED_ID int NOT NULL,
	CHANGE_TYPE nvarchar(50) NOT NULL,
	DATE datetime NOT NULL,
	REASON nvarchar(2000) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TB_ORGANSATION_LOG ADD CONSTRAINT
	PK_TB_ORGANSATION_LOG PRIMARY KEY CLUSTERED 
	(
	ORGANISATION_LOG_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT


/****************************************************************************************/

USE [ReviewerAdmin]
GO

/*
   19 March 201816:26:06
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
ALTER TABLE dbo.TB_ORGANISATION_LOG ADD CONSTRAINT
	DF_TB_ORGANISATION_LOG_DATE DEFAULT getdate() FOR DATE
GO
ALTER TABLE dbo.TB_ORGANISATION_LOG SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_ORGANISATION_LOG', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION_LOG', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_ORGANISATION_LOG', 'Object', 'CONTROL') as Contr_Per 

/**************************************************************************************/

USE [ReviewerAdmin]
GO

/*
   09 April 201814:38:31
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
ALTER TABLE dbo.TB_MANAGEMENT_SETTINGS ADD
	ENABLE_UCL_SHOP bit NOT NULL CONSTRAINT DF_TB_MANAGEMENT_SETTINGS_ENABLE_UCL_SHOP DEFAULT 0
GO
ALTER TABLE dbo.TB_MANAGEMENT_SETTINGS SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TB_MANAGEMENT_SETTINGS', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_MANAGEMENT_SETTINGS', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_MANAGEMENT_SETTINGS', 'Object', 'CONTROL') as Contr_Per 






/*************************************************************************************/
/*************************************************************************************/
/*************************************************************************************/
/**************************** Stored procedures **************************************/

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ArchieIsCheckedOutHere]    Script Date: 02/11/2017 09:29:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_ArchieIsCheckedOutHere]
(
	@REVIEW_ID int,
	@IS_CHECKEDOUT_HERE bit
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	update Reviewer.dbo.TB_REVIEW
		set IS_CHECKEDOUT_HERE = @IS_CHECKEDOUT_HERE
		where REVIEW_ID = @REVIEW_ID
	

END
GO

/*st_ContactLogin*/
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ContactLogin]    Script Date: 11/04/2018 14:01:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/10>
-- Description:	<gets contact details when loging in>
-- =============================================
ALTER PROCEDURE [dbo].[st_ContactLogin] 
(
	@USERNAME varchar(50),
	@PASSWORD varchar(50),
	@IP_ADDRESS nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
--first check if the username/pw are correct

	SELECT c.*, COUNT(sla.CONTACT_ID) as IsSLA, COUNT(o_a.CONTACT_ID) as IsOA
	FROM Reviewer.dbo.TB_CONTACT c
	Left outer join TB_SITE_LIC_ADMIN sla on sla.CONTACT_ID = c.CONTACT_ID
	Left outer join ReviewerAdmin.dbo.TB_ORGANISATION_ADMIN o_a on o_a.CONTACT_ID = c.CONTACT_ID
	where c.USERNAME = @USERNAME
	and HASHBYTES('SHA1', @Password + FLAVOUR) = PWASHED
	and EXPIRY_DATE is not null
	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME,
	c.PASSWORD, c.LAST_LOGIN, c.DATE_CREATED, c.EMAIL, c.EXPIRY_DATE,
	c.MONTHS_CREDIT, c.CREATOR_ID,c.TYPE, c.IS_SITE_ADMIN, c.DESCRIPTION, c.SEND_NEWSLETTER, c.FLAVOUR, c.PWASHED,
	c.ARCHIE_ACCESS_TOKEN, c.ARCHIE_ID, c.ARCHIE_REFRESH_TOKEN, c.ARCHIE_TOKEN_VALID_UNTIL, c.LAST_ARCHIE_CODE, c.LAST_ARCHIE_STATE
	
	RETURN
END



GO




/*********************************************************/

/*st_Organisation_Get_By_Admin*/
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_By_Admin]    Script Date: 11/04/2018 14:03:05 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Organisation_Get_By_Admin]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Organisation_Get_By_Admin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_By_Admin]    Script Date: 11/04/2018 14:03:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Organisation_Get_By_Admin] 
(	
	@organisation_adm_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select distinct o_a.ORGANISATION_ID, o.ORGANISATION_NAME from TB_ORGANISATION_ADMIN o_a
	inner join TB_ORGANISATION o on o.ORGANISATION_ID = o_a.ORGANISATION_ID
	where o_a.CONTACT_ID = @organisation_adm_ID



	
END





GO



/***********************************************************/

/*st_Organisation_Get_Accounts*/
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_Accounts]    Script Date: 11/04/2018 14:03:32 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Organisation_Get_Accounts]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Organisation_Get_Accounts]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_Accounts]    Script Date: 11/04/2018 14:03:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:           <Author,,Name>
-- Create date: <Create Date,,>
-- Description:      <Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Organisation_Get_Accounts] 
(
       @organisation_id int
)
AS
BEGIN
       -- SET NOCOUNT ON added to prevent extra result sets from
       -- interfering with SELECT statements.
       SET NOCOUNT ON;

    -- Insert statements for procedure here


       --reviews that were added to the currently active SITE_LIC_DETAILS
       select * from Reviewer.dbo.TB_REVIEW r
              inner join TB_ORGANISATION_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID 
			  and lr.ORGANISATION_ID = @organisation_id
       --accounts in the license
       select * from Reviewer.dbo.TB_CONTACT c
              inner join TB_ORGANISATION_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID 
			  and lc.ORGANISATION_ID = @organisation_id
       --license admins
       select * from Reviewer.dbo.TB_CONTACT c
              inner join TB_ORGANISATION_ADMIN la on c.CONTACT_ID = la.CONTACT_ID 
			  and la.ORGANISATION_ID = @organisation_id
END


GO



/**********************************************************************/

/*st_Organisation_Get*/
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Get]    Script Date: 11/04/2018 14:03:51 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Organisation_Get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Organisation_Get]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Get]    Script Date: 11/04/2018 14:03:51 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Organisation_Get] 
	-- Add the parameters for the stored procedure here
	
	@admin_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @Organisation TABLE
(
    ORGANISATION_ID int,
    ORGANISATION_NAME nvarchar(50),
	ORGANISATION_ADDRESS nvarchar(500),
	TELEPHONE nvarchar(50),
	NOTES nvarchar(4000),
	T_CREATOR_ID int,
	CREATOR_NAME nvarchar(255),
	ADMINISTRATOR_ID int,
	ADMINISTRATOR_NAME nvarchar(255),
	DATE_CREATED date	
)

	INSERT INTO @Organisation (ORGANISATION_ID, ORGANISATION_NAME,
	ORGANISATION_ADDRESS, TELEPHONE, NOTES, T_CREATOR_ID, DATE_CREATED, ADMINISTRATOR_ID)
	select o.ORGANISATION_ID, o.ORGANISATION_NAME, o.ORGANISATION_ADDRESS,
	o.TELEPHONE, o.NOTES, o.CREATOR_ID, o.DATE_CREATED, o_a.CONTACT_ID
	from TB_ORGANISATION o
	inner join TB_ORGANISATION_ADMIN o_a on o_a.ORGANISATION_ID = o.ORGANISATION_ID
	where o_a.CONTACT_ID = @admin_ID

	update @Organisation
	set CREATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = T_CREATOR_ID)
	
	update @Organisation
	set ADMINISTRATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = ADMINISTRATOR_ID)

	select * from @Organisation
	

END




GO



/**********************************************************************/

/*st_Organisation_Add_Remove_Contact*/
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Add_Remove_Contact]    Script Date: 11/04/2018 14:04:20 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Organisation_Add_Remove_Contact]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Organisation_Add_Remove_Contact]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Add_Remove_Contact]    Script Date: 11/04/2018 14:04:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Organisation_Add_Remove_Contact]
(
	  @org_id int
	, @admin_ID int
	, @contact_email nvarchar(500)
	, @res int output
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	-- Insert statements for procedure here
	
	declare @contact_id int
	set @res = 0

	-- initial check to see if email exists
	select @contact_id = CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = @contact_email and EXPIRY_DATE > '2010-03-20 00:00:01'
	if @@ROWCOUNT = 1 
	begin
		declare @ck int, @ck2 int = 0, @acc_all int
		set @ck = (SELECT count(contact_id) from TB_ORGANISATION_ADMIN 
			where (CONTACT_ID = @admin_ID or @admin_ID in 
				(select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)) 
			and ORGANISATION_ID = @org_id)
		--set @acc_all = (select top 1 ACCOUNTS_ALLOWANCE from TB_SITE_LIC_DETAILS 
		--	where SITE_LIC_ID = @lic_id and VALID_FROM is not null order by VALID_FROM desc)
		--set @ck2 = (select @acc_all - COUNT(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT 
		--	where SITE_LIC_ID = @lic_id)
		if @ck < 1 set @res = -1 --first check: see if supplied admin id is actually an admin!
		
		--else if  (select count(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT 
		--	where @contact_id = CONTACT_ID and SITE_LIC_ID != @lic_id) != 0
		--	--second check, see if contact_id is already in a site license (not including this one)
		--	set @res = -2	
		
		else -- checks went OK, let's see if we can do it
		begin
			set @ck = (select count(contact_id) from TB_ORGANISATION_CONTACT 
				where CONTACT_ID = @contact_id
				and ORGANISATION_ID = @org_id)
			if @ck = 1 -- contact is in the license, we should remove it
			begin
				delete from TB_ORGANISATION_CONTACT where CONTACT_ID = @contact_id and @org_id = ORGANISATION_ID
				if @@ROWCOUNT = 1
				begin --write success log
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'remove contact')
					--) select @org_id, @admin_ID, @contact_id, 'remove contact'
					--	from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc
				end
				else --write failure log, if this is fired, there is a bug somewhere
				begin
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'remove contact: failed!')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove contact: failed!'
					--	from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc	
					set @res = -4
				end
			end	
			
			--else if @ck2 < 1 --accounts allowance is all used up
			--set @res = -3
			
			else --contact is not in the license, we should add it
			begin
				insert into TB_ORGANISATION_CONTACT (CONTACT_ID, ORGANISATION_ID)
				values (@contact_id, @org_id)
				if @@ROWCOUNT = 1
				begin
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'add contact')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add contact'
					--	from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc
				end
				else
				begin
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'add contact: failed!')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add contact: failed!'
					--	from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc	
					set @res = -5
				end
			end
		end
	end
	else
	begin
		set @res = -6
	end
	
	--select c.CONTACT_ID, CONTACT_NAME, EMAIL from Reviewer.dbo.TB_SITE_LIC_CONTACT lc 
	--	inner join Reviewer.dbo.TB_CONTACT c on lc.CONTACT_ID = c.CONTACT_ID
	--	where SITE_LIC_ID = @lic_id
	
	return @res
	-- error codes: -1 = supplied admin_id is not an admin of this site lice
	--				-2 = contact_id already in a site license
	--				-3 = no allowance available, all account slots for current license have been used
	--				-4 = tried to remove account but couldn't write changes! BUG ALERT
	--				-5 = tried to add account but couldn't write changes! BUG ALERT
	--				-6 = email check returned no contact_id or multiple contact_ids
END




GO



/***********************************************************************/

/*st_Organisation_Edit_By_OrgAdm*/
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Edit_By_OrgAdm]    Script Date: 11/04/2018 14:04:44 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Organisation_Edit_By_OrgAdm]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Organisation_Edit_By_OrgAdm]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Edit_By_OrgAdm]    Script Date: 11/04/2018 14:04:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE procedure [dbo].[st_Organisation_Edit_By_OrgAdm]
(
	@ORGANISATION_ID int,
	@ORGANISATION_ADDRESS nvarchar(500),
	@TELEPHONE nvarchar(50),
	@NOTES nvarchar(2000)
)

As

SET NOCOUNT ON

	update TB_ORGANISATION
	set ORGANISATION_ADDRESS = @ORGANISATION_ADDRESS,
	TELEPHONE = @TELEPHONE,
	NOTES = @NOTES
	where ORGANISATION_ID = @ORGANISATION_ID

SET NOCOUNT OFF



GO



/********************************************************************************/

/*st_Organisation_Get_All*/
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_All]    Script Date: 11/04/2018 14:05:07 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Organisation_Get_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Organisation_Get_All]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_All]    Script Date: 11/04/2018 14:05:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Organisation_Get_All] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 

	DECLARE @Organisations TABLE
	(
		ORGANISATION_ID int,
		ORGANISATION_NAME nvarchar(50),
		CONTACT_ID int,
		CONTACT_NAME nvarchar(500)
	)
	DECLARE @org_lic_id int
	DECLARE @first_contact_id int
	DECLARE @counter int
	set @counter = 0

	INSERT INTO @Organisations (ORGANISATION_ID, ORGANISATION_NAME)	
	select ORGANISATION_ID, ORGANISATION_NAME
	from TB_ORGANISATION

	DECLARE org_id CURSOR FOR 
	SELECT ORGANISATION_ID from @Organisations 
	OPEN org_id
	FETCH NEXT FROM org_id INTO @org_lic_id

	WHILE @@FETCH_STATUS = 0
	BEGIN	
		DECLARE contact_id CURSOR FOR 
		SELECT CONTACT_ID from TB_ORGANISATION_ADMIN o_a
		where o_a.ORGANISATION_ID = @org_lic_id
		OPEN contact_id
		FETCH NEXT FROM contact_id INTO @first_contact_id
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if @counter = 0
			begin
				update @Organisations
				set CONTACT_ID = @first_contact_id
				where ORGANISATION_ID = @org_lic_id
				
				update @Organisations
				set CONTACT_NAME = 
				(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT c
				where c.CONTACT_ID = @first_contact_id)
				where ORGANISATION_ID = @org_lic_id

				set @counter = 1
			end
			FETCH NEXT FROM contact_id INTO @first_contact_id
		END
		CLOSE contact_id
		DEALLOCATE contact_id
		set @counter = 0
		FETCH NEXT FROM org_id INTO @org_lic_id
	END

	CLOSE org_id
	DEALLOCATE org_id


select * from @Organisations


       
END





GO



/**********************************************************************************/

/*st_Organisation_Create_or_Edit*/
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Create_or_Edit]    Script Date: 11/04/2018 14:05:29 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Organisation_Create_or_Edit]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Organisation_Create_or_Edit]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Create_or_Edit]    Script Date: 11/04/2018 14:05:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Organisation_Create_or_Edit] 
	-- Add the parameters for the stored procedure here
(
	  @ORG_ID nvarchar(50)
	, @creator_id INT
	, @admin_id  int -- who will be the first administrator of the site lic, 
	, @ORGANISATION_NAME nvarchar(50)
	, @ORGANISATION_ADDRESS nvarchar(500)
	, @TELEPHONE nvarchar(50)
	, @NOTES nvarchar(4000)
	, @DATE_CREATED datetime
	, @RESULT nvarchar(50) output
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @ck int

	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
	BEGIN TRANSACTION
	if @ORG_ID = 'N/A' -- we are creating a new site license
	BEGIN		
		declare @O_ID int
		
		insert into TB_ORGANISATION (ORGANISATION_NAME, ORGANISATION_ADDRESS, 
			TELEPHONE, NOTES, CREATOR_ID, DATE_CREATED)
		VALUES (@ORGANISATION_NAME, @ORGANISATION_ADDRESS, @TELEPHONE,
			@NOTES, @creator_id, @DATE_CREATED)	
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback1'
			return 
		end
		else
		begin
			set @O_ID = @@IDENTITY
			insert into TB_ORGANISATION_ADMIN([ORGANISATION_ID], [CONTACT_ID]) 
			VALUES (@O_ID, @admin_id)
			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
			begin
				ROLLBACK TRANSACTION
				set @RESULT = 'rollback2'
				return 
			end
			else
			begin
				set @RESULT = @O_ID
			end
		end		
	END
	else
	BEGIN
		-- get the existing admin
		declare @O_ADM_ID int
		select @O_ADM_ID = CONTACT_ID from TB_ORGANISATION_ADMIN where ORGANISATION_ID = @ORG_ID
				
		-- we are editing the site license details
		update TB_ORGANISATION
		set ORGANISATION_NAME = @ORGANISATION_NAME, 
		ORGANISATION_ADDRESS = @ORGANISATION_ADDRESS,
		TELEPHONE = @TELEPHONE,
		NOTES = @NOTES,
		DATE_CREATED = @DATE_CREATED
		where ORGANISATION_ID = @ORG_ID
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback3'
			return 
		end
		else
		begin			
			update TB_ORGANISATION_ADMIN
			set CONTACT_ID = @admin_id
			where CONTACT_ID = @O_ADM_ID
			and ORGANISATION_ID = @ORG_ID
			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
			begin
				ROLLBACK TRANSACTION
				set @RESULT = 'rollback4'
				return 
			end
			else
			begin			
				set @RESULT = 'valid'
			end
		end		

	END
	
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	set @RESULT = 'rollback' --to tell the code data was not committed!
	END CATCH
	
	return 

END






GO



/**********************************************************************************/

/*st_Organisation_Remove_Review*/
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Remove_Review]    Script Date: 11/04/2018 14:05:47 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Organisation_Remove_Review]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Organisation_Remove_Review]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Remove_Review]    Script Date: 11/04/2018 14:05:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Organisation_Remove_Review]
	-- Add the parameters for the stored procedure here
	@org_id int
	, @admin_ID int
	, @review_id int
	, @res int output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	set @res = 0

    -- Insert statements for procedure here
    declare @ck int, @ck2 int, @org_det_id int
	
	set @ck = (SELECT count(contact_id) from TB_ORGANISATION_ADMIN 
	where (CONTACT_ID = @admin_ID or @admin_ID in 
	(select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)
		   ) and ORGANISATION_ID = @org_id)
	if @ck < 1 set @res = -1 --first check: see if supplied admin is actually an admin!
	
	--second check, see if review_id exists
	else if  
	(select count(REVIEW_ID) from Reviewer.dbo.TB_REVIEW 
	 where @review_id = REVIEW_ID) != 1
		set @res = -2
	
	else -- initial checks went OK, let's see if we can do it
	begin
		set @ck = 
		(select count(review_id) from TB_ORGANISATION_REVIEW 
		where REVIEW_ID = @review_id and ORGANISATION_ID = @org_id)
		
		--set @lic_det_id = 
		--(SELECT TOP 1 ld.SITE_LIC_DETAILS_ID from Reviewer.dbo.TB_SITE_LIC sl 
		--	inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
		--	inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
		--	Order by ld.VALID_FROM desc)
		
		if @ck = 0 -- review is NOT in the site lic
		begin
			set @res = -3 --review not in this site_lic
		end
		else
		begin --all is well, let's do something!
			begin transaction --make sure we don't commit only half of the mission critical data! (we assume the update below will work, only checking for the other statement)
			
			delete from TB_ORGANISATION_REVIEW
			where REVIEW_ID = @review_id

			update Reviewer.dbo.TB_REVIEW set ORGANISATION_ID = 0
					where REVIEW_ID = @review_id
			
			if @@ROWCOUNT = 1
			begin --write success log
				commit transaction --all is well, commit
				insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE]) 
				values (@org_id, @admin_ID, @review_id, 'remove review')
			end
			else --write failure log, if this is fired, there is a bug somewhere
			begin
				rollback transaction --BAD! something went wrong!
				insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE]) 
				values (@org_id, @admin_ID, @review_id, 'remove review: failed!')	
				set @res = -4
			end
		end
	end
	--select r.review_id, review_name from reviewer.dbo.TB_SITE_LIC_REVIEW lr
	--	inner join Reviewer.dbo.TB_REVIEW r on lr.REVIEW_ID = r.REVIEW_ID
	-- where SITE_LIC_ID = @lic_id
	return @res
	-- error codes: -1 = supplied admin_id is not an admin of this site lice
	--				-2 = review_id does not exist
	--				-3 = review not in this site_lic
	--				-4 = all seemed well but couldn't write changes! BUG ALERT
END




GO



/*******************************************************************************/

/*st_Organisation_Add_Review*/
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Add_Review]    Script Date: 11/04/2018 14:06:07 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Organisation_Add_Review]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Organisation_Add_Review]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Add_Review]    Script Date: 11/04/2018 14:06:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Organisation_Add_Review]
	-- Add the parameters for the stored procedure here
	@org_id int
	, @admin_ID int
	, @review_id int
	, @res int output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @review_name nvarchar(1000)
	set @res = 0

    -- Insert statements for procedure here
    
    -- initial check to see if review exists
    select @review_name = REVIEW_NAME from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @review_id
	if @@ROWCOUNT = 1 
	begin
		declare @ck int, @ck2 int, @org_det_id int
		set @ck = (SELECT count(contact_id) from TB_ORGANISATION_ADMIN 
		where (CONTACT_ID = @admin_ID or @admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT 
		where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)
			) and ORGANISATION_ID = @org_id)
		if @ck < 1 set @res = -1 --first check: see if supplied admin is actually and admin!
		
		else -- initial checks went OK, let's see if we can do it
		begin
			set @ck = (select count(review_id) from TB_ORGANISATION_REVIEW where REVIEW_ID = @review_id)
			--set @org_det_id = (SELECT TOP 1 ld.SITE_LIC_DETAILS_ID from Reviewer.dbo.TB_SITE_LIC sl 
			--						inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
			--						inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
			--						and sl.SITE_LIC_ID = @org_id
			--					Order by ld.VALID_FROM desc
			--					)
			--@ck2 counts how many reviews can be added to current lic
			--set @ck2 = (select d.REVIEWS_ALLOWANCE - count(review_id) from TB_SITE_LIC_DETAILS d inner join
			--				 Reviewer.dbo.TB_SITE_LIC_REVIEW lr on lr.SITE_LIC_DETAILS_ID = @org_det_id
			--					and lr.SITE_LIC_DETAILS_ID = d.SITE_LIC_DETAILS_ID
			--				 group by d.REVIEWS_ALLOWANCE
			--				 )--count how many reviews can still be added
			if @ck != 0 -- review is already in an organisation
			begin
				--set @ck = (select count(review_id) from TB_ORGANISATION_REVIEW 
				--    where REVIEW_ID = @review_id and ORGANISATION_ID = @org_id)
				--if @ck = 1 set @res = -3 --review already in this site_lic
				--else 
				set @res = -4 --review is already in an organisation
			end
			--else if @ck2 < 1 --no allowance available, all review slots for current license have been used
			--begin
			--	set @res = -5
			--end
			else
			begin --all is well, let's do something!
				begin transaction --make sure we don't commit only half of the mission critical data! 
				--(we assume the update below will work, only checking for the other statement)
				update Reviewer.dbo.TB_REVIEW set ORGANISATION_ID = @org_id
					where REVIEW_ID = @review_id
				insert into TB_ORGANISATION_REVIEW ([ORGANISATION_ID], [REVIEW_ID], [DATE_ADDED], [ADDED_BY]) 
					values (@org_id, @review_id, getdate(), @admin_ID)
				if @@ROWCOUNT = 1
				begin --write success log
					commit transaction --all is well, commit
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE]) 
					values (@org_id, @admin_ID, @review_id, 'add review')															
				end
				else --write failure log, if this is fired, there is a bug somewhere
				begin
					rollback transaction --BAD! something went wrong!
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE]) 
					values (@org_id, @admin_ID, @review_id, 'add review: failed!')	
					set @res = -6
				end
			end
		end
	end
	else
	begin
		set @res = -2
	end
	
	--select r.review_id, review_name from reviewer.dbo.TB_SITE_LIC_REVIEW lr
	--	inner join Reviewer.dbo.TB_REVIEW r on lr.REVIEW_ID = r.REVIEW_ID
	--	where SITE_LIC_ID = @lic_id
	 
	return @res
	-- error codes: -1 = supplied admin_id is not an admin of this site lice
	--				-2 = review_id does not exist
	--				-3 = review already in this site_lic
	--				-4 = review is in some other site_lic
	--				-5 = no allowance available, all review slots for current license have been used
	--				-6 = all seemed well but couldn't write changes! BUG ALERT
END




GO



/******************************************************************************/

/*st_Organisation_Get_ByID*/
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_ByID]    Script Date: 11/04/2018 14:06:21 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Organisation_Get_ByID]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Organisation_Get_ByID]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_ByID]    Script Date: 11/04/2018 14:06:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Organisation_Get_ByID] 
	-- Add the parameters for the stored procedure here
	
	@admin_ID int,
	@organisation_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @Organisation TABLE
(
    ORGANISATION_ID int,
    ORGANISATION_NAME nvarchar(50),
	ORGANISATION_ADDRESS nvarchar(500),
	TELEPHONE nvarchar(50),
	NOTES nvarchar(4000),
	T_CREATOR_ID int,
	CREATOR_NAME nvarchar(255),
	ADMINISTRATOR_ID int,
	ADMINISTRATOR_NAME nvarchar(255),
	DATE_CREATED date	
)

	INSERT INTO @Organisation (ORGANISATION_ID, ORGANISATION_NAME,
	ORGANISATION_ADDRESS, TELEPHONE, NOTES, T_CREATOR_ID, DATE_CREATED, ADMINISTRATOR_ID)
	select o.ORGANISATION_ID, o.ORGANISATION_NAME, o.ORGANISATION_ADDRESS,
	o.TELEPHONE, o.NOTES, o.CREATOR_ID, o.DATE_CREATED, o_a.CONTACT_ID
	from TB_ORGANISATION o
	inner join TB_ORGANISATION_ADMIN o_a on o_a.ORGANISATION_ID = o.ORGANISATION_ID
	where o_a.CONTACT_ID = @admin_ID
	and o.ORGANISATION_ID = @organisation_id

	update @Organisation
	set CREATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = T_CREATOR_ID)
	
	update @Organisation
	set ADMINISTRATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = ADMINISTRATOR_ID)

	select * from @Organisation
	

END





GO



/*****************************************************************************/

/*st_Organisation_Get_By_ContactID*/
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_By_ContactID]    Script Date: 11/04/2018 14:06:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Organisation_Get_By_ContactID]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_Organisation_Get_By_ContactID]
GO

/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_By_ContactID]    Script Date: 11/04/2018 14:06:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_Organisation_Get_By_ContactID] 
	-- Add the parameters for the stored procedure here
	
	@CONTACT_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select ORGANISATION_ID, ORGANISATION_NAME from TB_ORGANISATION
	where ORGANISATION_ID in (select ORGANISATION_ID from TB_ORGANISATION_CONTACT
	where CONTACT_ID = @CONTACT_ID)


	

END





GO



/*****************************************************************************/

/*st_OrganisationRemoveMember*/
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_OrganisationRemoveMember]    Script Date: 11/04/2018 14:06:54 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_OrganisationRemoveMember]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_OrganisationRemoveMember]
GO

/****** Object:  StoredProcedure [dbo].[st_OrganisationRemoveMember]    Script Date: 11/04/2018 14:06:54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE procedure [dbo].[st_OrganisationRemoveMember]
(
	@ORGANISATION_ID int,
	@CONTACT_ID int
)

As

SET NOCOUNT ON


	                
	delete from TB_ORGANISATION_CONTACT 
	where CONTACT_ID = @CONTACT_ID
	and ORGANISATION_ID = @ORGANISATION_ID

		

SET NOCOUNT OFF


GO



/*****************************************************************************/



/************************** old bug ******************************************/

USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewContacts]    Script Date: 4/11/2018 11:17:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- Create date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
ALTER PROCEDURE [dbo].[st_ReviewContacts] 
(
	@REVIEW_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here 
    CREATE TABLE #REVIEW_CONTACTS (
	CONTACT_ID int,
	CONTACT_NAME nvarchar(255),
	EMAIL nvarchar(500),
	LAST_LOGIN datetime,
	EXPIRY_DATE date,
	HOURS int)
    
    insert into #REVIEW_CONTACTS (CONTACT_ID, CONTACT_NAME, EMAIL, EXPIRY_DATE)
	select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, c.EXPIRY_DATE 
	FROM Reviewer.dbo.tb_CONTACT c
	INNER JOIN Reviewer.dbo.tb_REVIEW_CONTACT r_c ON c.CONTACT_ID = r_c.CONTACT_ID
	AND r_c.REVIEW_ID = @REVIEW_ID
	group by c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, c.EXPIRY_DATE
	ORDER BY c.CONTACT_NAME
	
	update #REVIEW_CONTACTS
	set HOURS = 
	(select SUM(DATEDIFF(HOUR,l_t.CREATED ,l_t.LAST_RENEWED )) as HOURS
	from TB_LOGON_TICKET l_t where l_t.REVIEW_ID = @REVIEW_ID 
	and CONTACT_ID = #REVIEW_CONTACTS.CONTACT_ID)

	update #REVIEW_CONTACTS
	set LAST_LOGIN = 
	(select max(l_t.CREATED) as LAST_LOGIN
	from TB_LOGON_TICKET l_t where l_t.REVIEW_ID = @REVIEW_ID 
	and CONTACT_ID = #REVIEW_CONTACTS.CONTACT_ID)
	
    select * from #REVIEW_CONTACTS

	drop table #REVIEW_CONTACTS
	
    /*
    select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL,
    c.LAST_LOGIN, c.EXPIRY_DATE, SUM(DATEDIFF(HOUR,l_t.CREATED ,l_t.LAST_RENEWED )) as HOURS
    FROM Reviewer.dbo.tb_CONTACT c
    INNER JOIN Reviewer.dbo.tb_REVIEW_CONTACT r_c ON c.CONTACT_ID = r_c.CONTACT_ID 
    inner join TB_LOGON_TICKET l_t on l_t.CONTACT_ID = r_c.CONTACT_ID and l_t.REVIEW_ID = @REVIEW_ID
    AND r_c.REVIEW_ID = @REVIEW_ID
    group by c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, c.LAST_LOGIN, c.EXPIRY_DATE
    ORDER BY c.CONTACT_NAME*/

END
GO

/*****************************************************************************/

