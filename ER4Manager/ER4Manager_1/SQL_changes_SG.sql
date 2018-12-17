--USE [Reviewer]
--GO

--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
--CREATE TABLE dbo.TB_SITE_LIC
--	(
--	SITE_LIC_ID int NOT NULL,
--	SITE_LIC_NAME nvarchar(50) NOT NULL,
--	COMPANY_NAME nvarchar(50) NOT NULL,
--	COMPANY_ADDRESS nvarchar(500) NOT NULL,
--	TELEPHONE nvarchar(50) NOT NULL,
--	NOTES nvarchar(4000) NULL,
--	EXPIRY_DATE datetime NULL,
--	CREATOR_ID int NOT NULL,
--	DATE_CREATED datetime NOT NULL
--	)  ON [PRIMARY]
--GO
--ALTER TABLE dbo.TB_SITE_LIC SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--GO



--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
--ALTER TABLE dbo.TB_SITE_LIC
--	DROP CONSTRAINT FK_TB_SITE_LIC_TB_CONTACT
--GO
--ALTER TABLE dbo.TB_CONTACT SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--BEGIN TRANSACTION
--GO
--CREATE TABLE dbo.Tmp_TB_SITE_LIC
--	(
--	SITE_LIC_ID int NOT NULL IDENTITY (1, 1),
--	SITE_LIC_NAME nvarchar(50) NOT NULL,
--	COMPANY_NAME nvarchar(50) NOT NULL,
--	COMPANY_ADDRESS nvarchar(500) NOT NULL,
--	TELEPHONE nvarchar(50) NULL,
--	NOTES nvarchar(4000) NULL,
--	EXPIRY_DATE datetime NULL,
--	CREATOR_ID int NOT NULL,
--	DATE_CREATED datetime NOT NULL
--	)  ON [PRIMARY]
--GO
--ALTER TABLE dbo.Tmp_TB_SITE_LIC SET (LOCK_ESCALATION = TABLE)
--GO
--SET IDENTITY_INSERT dbo.Tmp_TB_SITE_LIC ON
--GO
--IF EXISTS(SELECT * FROM dbo.TB_SITE_LIC)
--	 EXEC('INSERT INTO dbo.Tmp_TB_SITE_LIC (SITE_LIC_ID, SITE_LIC_NAME, COMPANY_NAME, COMPANY_ADDRESS, TELEPHONE, NOTES, EXPIRY_DATE, CREATOR_ID, DATE_CREATED)
--		SELECT SITE_LIC_ID, SITE_LIC_NAME, COMPANY_NAME, COMPANY_ADDRESS, TELEPHONE, NOTES, EXPIRY_DATE, CREATOR_ID, DATE_CREATED FROM dbo.TB_SITE_LIC WITH (HOLDLOCK TABLOCKX)')
--GO
--SET IDENTITY_INSERT dbo.Tmp_TB_SITE_LIC OFF
--GO
--DROP TABLE dbo.TB_SITE_LIC
--GO
--EXECUTE sp_rename N'dbo.Tmp_TB_SITE_LIC', N'TB_SITE_LIC', 'OBJECT' 
--GO
--ALTER TABLE dbo.TB_SITE_LIC ADD CONSTRAINT
--	PK_TB_SITE_LIC PRIMARY KEY CLUSTERED 
--	(
--	SITE_LIC_ID
--	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

--GO
--ALTER TABLE dbo.TB_SITE_LIC ADD CONSTRAINT
--	FK_TB_SITE_LIC_TB_CONTACT FOREIGN KEY
--	(
--	CREATOR_ID
--	) REFERENCES dbo.TB_CONTACT
--	(
--	CONTACT_ID
--	) ON UPDATE  NO ACTION 
--	 ON DELETE  NO ACTION 
	
--GO
--COMMIT
--GO

--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
--ALTER TABLE dbo.TB_CONTACT SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.TB_SITE_LIC ADD CONSTRAINT
--	FK_TB_SITE_LIC_TB_CONTACT FOREIGN KEY
--	(
--	CREATOR_ID
--	) REFERENCES dbo.TB_CONTACT
--	(
--	CONTACT_ID
--	) ON UPDATE  NO ACTION 
--	 ON DELETE  NO ACTION 
	
--GO
--ALTER TABLE dbo.TB_SITE_LIC ADD CONSTRAINT
--	DF_TB_SITE_LIC_DATE_CREATED DEFAULT GETDATE() FOR DATE_CREATED
--GO
--ALTER TABLE dbo.TB_SITE_LIC SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT

--GO
--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
--CREATE TABLE dbo.TB_SITE_LIC_CONTACT

--	(
--	SITE_LIC_ID int NOT NULL,
--	CONTACT_ID int NOT NULL
--	)  ON [PRIMARY]
--GO
--ALTER TABLE dbo.TB_SITE_LIC_CONTACT SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--GO

--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
--ALTER TABLE dbo.TB_CONTACT SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.TB_SITE_LIC SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.TB_SITE_LIC_CONTACT ADD CONSTRAINT
--	UK_TB_SITE_LIC_CONTACT UNIQUE NONCLUSTERED 
--	(
--	CONTACT_ID
--	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

--GO
--ALTER TABLE dbo.TB_SITE_LIC_CONTACT ADD CONSTRAINT
--	FK_TB_SITE_LIC_CONTACT_TB_SITE_LIC FOREIGN KEY
--	(
--	SITE_LIC_ID
--	) REFERENCES dbo.TB_SITE_LIC
--	(
--	SITE_LIC_ID
--	) ON UPDATE  CASCADE 
--	 ON DELETE  CASCADE 
	
--GO
--ALTER TABLE dbo.TB_SITE_LIC_CONTACT ADD CONSTRAINT
--	FK_TB_SITE_LIC_CONTACT_TB_CONTACT FOREIGN KEY
--	(
--	CONTACT_ID
--	) REFERENCES dbo.TB_CONTACT
--	(
--	CONTACT_ID
--	) ON UPDATE  NO ACTION 
--	 ON DELETE  NO ACTION 
	
--GO
--ALTER TABLE dbo.TB_SITE_LIC_CONTACT SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--GO


--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
--CREATE TABLE dbo.TB_SITE_LIC_REVIEW
--	(
--	SITE_LIC_ID int NOT NULL,
--	SITE_LIC_DETAILS_ID int NOT NULL,
--	REVIEW_ID int NOT NULL,
--	DATE_ADDED datetime NOT NULL,
--	ADDED_BY int NOT NULL
--	)  ON [PRIMARY]
--GO
--ALTER TABLE dbo.TB_SITE_LIC_REVIEW SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--GO

--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
--ALTER TABLE dbo.TB_CONTACT SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.TB_REVIEW SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.TB_SITE_LIC SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.TB_SITE_LIC_REVIEW ADD CONSTRAINT
--	DF_TB_SITE_LIC_REVIEW_DATE_ADDED DEFAULT GETDATE() FOR DATE_ADDED
--GO
--ALTER TABLE dbo.TB_SITE_LIC_REVIEW ADD CONSTRAINT
--	FK_TB_SITE_LIC_REVIEW_TB_SITE_LIC FOREIGN KEY
--	(
--	SITE_LIC_ID
--	) REFERENCES dbo.TB_SITE_LIC
--	(
--	SITE_LIC_ID
--	) ON UPDATE  CASCADE 
--	 ON DELETE  CASCADE 
	
--GO
--ALTER TABLE dbo.TB_SITE_LIC_REVIEW ADD CONSTRAINT
--	FK_TB_SITE_LIC_REVIEW_TB_REVIEW FOREIGN KEY
--	(
--	REVIEW_ID
--	) REFERENCES dbo.TB_REVIEW
--	(
--	REVIEW_ID
--	) ON UPDATE  NO ACTION 
--	 ON DELETE  NO ACTION 
	
--GO
--ALTER TABLE dbo.TB_SITE_LIC_REVIEW ADD CONSTRAINT
--	FK_TB_SITE_LIC_REVIEW_TB_CONTACT FOREIGN KEY
--	(
--	ADDED_BY
--	) REFERENCES dbo.TB_CONTACT
--	(
--	CONTACT_ID
--	) ON UPDATE  NO ACTION 
--	 ON DELETE  NO ACTION 
	
--GO
--ALTER TABLE dbo.TB_SITE_LIC_REVIEW SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--GO



--USE [ReviewerAdmin]
--GO
--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
--CREATE TABLE dbo.TB_SITE_LIC_DETAILS

--	(
--	SITE_LIC_DETAILS_ID int NOT NULL,
--	SITE_LIC_ID int NOT NULL,
--	FOR_SALE_ID int NOT NULL,
--	ACCOUNTS_ALLOWANCE int NOT NULL,
--	REVIEWS_ALLOWANCE int NOT NULL,
--	DATE_CREATED datetime NOT NULL,
--	MONTHS int NOT NULL,
--	VALID_FROM datetime NULL
--	)  ON [PRIMARY]
--GO
--ALTER TABLE dbo.TB_SITE_LIC_DETAILS
-- SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--GO


--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
--ALTER TABLE dbo.TB_SITE_LIC_DETAILS
--	DROP CONSTRAINT FK_TB_SITE_LIC_DETAILS_TB_FOR_SALE
--GO
--ALTER TABLE dbo.TB_FOR_SALE SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--BEGIN TRANSACTION
--GO
--CREATE TABLE dbo.Tmp_TB_SITE_LIC_DETAILS
--	(
--	SITE_LIC_DETAILS_ID int NOT NULL IDENTITY (1, 1),
--	SITE_LIC_ID int NOT NULL,
--	FOR_SALE_ID int NOT NULL,
--	ACCOUNTS_ALLOWANCE int NOT NULL,
--	REVIEWS_ALLOWANCE int NOT NULL,
--	DATE_CREATED datetime NOT NULL,
--	MONTHS int NOT NULL,
--	VALID_FROM datetime NULL
--	)  ON [PRIMARY]
--GO
--ALTER TABLE dbo.Tmp_TB_SITE_LIC_DETAILS SET (LOCK_ESCALATION = TABLE)
--GO
--SET IDENTITY_INSERT dbo.Tmp_TB_SITE_LIC_DETAILS ON
--GO
--IF EXISTS(SELECT * FROM dbo.TB_SITE_LIC_DETAILS)
--	 EXEC('INSERT INTO dbo.Tmp_TB_SITE_LIC_DETAILS (SITE_LIC_DETAILS_ID, SITE_LIC_ID, FOR_SALE_ID, ACCOUNTS_ALLOWANCE, REVIEWS_ALLOWANCE, DATE_CREATED, MONTHS, VALID_FROM)
--		SELECT SITE_LIC_DETAILS_ID, SITE_LIC_ID, FOR_SALE_ID, ACCOUNTS_ALLOWANCE, REVIEWS_ALLOWANCE, DATE_CREATED, MONTHS, VALID_FROM FROM dbo.TB_SITE_LIC_DETAILS WITH (HOLDLOCK TABLOCKX)')
--GO
--SET IDENTITY_INSERT dbo.Tmp_TB_SITE_LIC_DETAILS OFF
--GO
--DROP TABLE dbo.TB_SITE_LIC_DETAILS
--GO
--EXECUTE sp_rename N'dbo.Tmp_TB_SITE_LIC_DETAILS', N'TB_SITE_LIC_DETAILS', 'OBJECT' 
--GO
--ALTER TABLE dbo.TB_SITE_LIC_DETAILS ADD CONSTRAINT
--	PK_TB_SITE_LIC_DETAILS PRIMARY KEY CLUSTERED 
--	(
--	SITE_LIC_DETAILS_ID
--	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

--GO
--ALTER TABLE dbo.TB_SITE_LIC_DETAILS ADD CONSTRAINT
--	FK_TB_SITE_LIC_DETAILS_TB_FOR_SALE FOREIGN KEY
--	(
--	FOR_SALE_ID
--	) REFERENCES dbo.TB_FOR_SALE
--	(
--	FOR_SALE_ID
--	) ON UPDATE  NO ACTION 
--	 ON DELETE  NO ACTION 
	
--GO
--COMMIT
--GO
--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
--ALTER TABLE dbo.TB_FOR_SALE SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.TB_SITE_LIC_DETAILS ADD CONSTRAINT
--	FK_TB_SITE_LIC_DETAILS_TB_FOR_SALE FOREIGN KEY
--	(
--	FOR_SALE_ID
--	) REFERENCES dbo.TB_FOR_SALE
--	(
--	FOR_SALE_ID
--	) ON UPDATE  NO ACTION 
--	 ON DELETE  NO ACTION 
	
--GO
--ALTER TABLE dbo.TB_SITE_LIC_DETAILS ADD CONSTRAINT
--	DF_TB_SITE_LIC_DETAILS_DATE_CREATED DEFAULT GETDATE() FOR DATE_CREATED
--GO
--ALTER TABLE dbo.TB_SITE_LIC_DETAILS SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--GO
--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
--CREATE TABLE dbo.TB_SITE_LIC_ADMIN

--	(
--	SITE_LIC_ID int NOT NULL,
--	CONTACT_ID int NOT NULL
--	)  ON [PRIMARY]
--GO
--ALTER TABLE dbo.TB_SITE_LIC_ADMIN
-- SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--GO

--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
--CREATE TABLE dbo.TB_SITE_LIC_LOG
--	(
--	SITE_LIC_DETAILS_ID int NOT NULL,
--	CONTACT_ID int NOT NULL,
--	AFFECTED_ID int NOT NULL,
--	CHANGE_TYPE nvarchar(50) NOT NULL,
--	DATE datetime NOT NULL,
--	REASON nvarchar(2000) NULL
--	)  ON [PRIMARY]
--GO
--ALTER TABLE dbo.TB_SITE_LIC_LOG  ADD CONSTRAINT
--	DF_Table_1_DATE DEFAULT GETDATE() FOR DATE
--GO
--ALTER TABLE dbo.TB_SITE_LIC_LOG SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--GO

--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
--ALTER TABLE dbo.TB_SITE_LIC_DETAILS SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.TB_SITE_LIC_LOG ADD CONSTRAINT
--	FK_TB_SITE_LIC_LOG_TB_SITE_LIC_DETAILS FOREIGN KEY
--	(
--	SITE_LIC_DETAILS_ID
--	) REFERENCES dbo.TB_SITE_LIC_DETAILS
--	(
--	SITE_LIC_DETAILS_ID
--	) ON UPDATE  NO ACTION 
--	 ON DELETE  NO ACTION 
	
--GO
--ALTER TABLE dbo.TB_SITE_LIC_LOG SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--GO


--use [Reviewer]
--go

--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--ALTER procedure [dbo].[st_ContactLogin]
--(
--	@userName  varchar(50)	
--)
----note the GRACE_EXP field, how many months/days we add to EXPIRY_DATE defines how long is the grace period for the whole of ER4.
----during the grace period users can log on ER4 but will have read only access.
--As
--Select c.CONTACT_ID, c.contact_name, c.Password, 
--	DATEADD(m, 2, 
--			( CASE when sl.[EXPIRY_DATE] is not null 
--				and sl.[EXPIRY_DATE] > c.[EXPIRY_DATE]
--					then sl.[EXPIRY_DATE]
--				else c.[EXPIRY_DATE]
--				end
--			)) as GRACE_EXP,
--	[TYPE], IS_SITE_ADMIN  /* TB_CONTACT.[Role] */
--From TB_CONTACT c
--Left outer join TB_SITE_LIC_CONTACT lc on lc.CONTACT_ID = c.CONTACT_ID
--Left outer join TB_SITE_LIC sl on lc.SITE_LIC_ID = sl.SITE_LIC_ID
--Where c.UserName = @userName

--GO


--/****** Object:  StoredProcedure [dbo].[st_ContactLoginReview]    Script Date: 11/29/2011 15:56:22 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--ALTER procedure [dbo].[st_ContactLoginReview]
--(
--	@userId int,
--	@reviewId int,
--	@GUI uniqueidentifier OUTPUT
--)

--As
--SELECT TB_REVIEW.REVIEW_ID, ROLE_NAME as [ROLE], 
--		( CASE WHEN sl2.[EXPIRY_DATE] is not null
--				and sl2.[EXPIRY_DATE] > TB_REVIEW.[EXPIRY_DATE]
--					then sl2.[EXPIRY_DATE]
--				else TB_REVIEW.[EXPIRY_DATE]
--				end
--		) as REVIEW_EXP, 
--		( CASE when sl.[EXPIRY_DATE] is not null 
--				and sl.[EXPIRY_DATE] > c.[EXPIRY_DATE]
--					then sl.[EXPIRY_DATE]
--				else c.[EXPIRY_DATE]
--				end
--		) as CONTACT_EXP,
--		FUNDER_ID
--FROM TB_REVIEW_CONTACT
--	INNER JOIN TB_REVIEW on TB_REVIEW_CONTACT.REVIEW_ID = TB_REVIEW.REVIEW_ID
--	INNER JOIN TB_CONTACT c on TB_REVIEW_CONTACT.CONTACT_ID = c.CONTACT_ID
--	INNER JOIN TB_CONTACT_REVIEW_ROLE on TB_CONTACT_REVIEW_ROLE.REVIEW_CONTACT_ID = TB_REVIEW_CONTACT.REVIEW_CONTACT_ID
--	Left outer join TB_SITE_LIC_CONTACT lc on lc.CONTACT_ID = c.CONTACT_ID
--	Left outer join TB_SITE_LIC sl on lc.SITE_LIC_ID = sl.SITE_LIC_ID
--	left outer join TB_SITE_LIC_REVIEW lr on TB_REVIEW.REVIEW_ID = lr.REVIEW_ID
--	left outer join TB_SITE_LIC sl2 on lr.SITE_LIC_ID = sl2.SITE_LIC_ID
	
--WHERE TB_REVIEW.REVIEW_ID = @reviewId AND c.CONTACT_ID = @userId

--IF @@ROWCOUNT >= 1 
--	BEGIN
--	DECLARE	@return_value int,
--			@GUID uniqueidentifier
			
--	EXEC	@return_value = [ReviewerAdmin].[dbo].[st_LogonTicket_Insert]
--			@Contact_ID = @userId,
--			@Review_ID = @reviewId,
--			@GUID = @GUI OUTPUT
--	SELECT	@GUI as N'@GUID'
--	END
--GO


----st_ContactPurchasedDetails
----st_ContactDetails
----st_ContactReviewsShareable
----st_Site_Lic_Add_New
----st_Site_Lic_Get_Details
----st_Site_Lic_Get_Accounts
----st_Site_Lic_Add_Review
----st_Site_Lic_Add_Remove_Contact
----st_Site_Lic_Add_Remove_Admin
----st_Site_Lic_Activate
----st_ContactPurchasedAccounts
----st_ContactPurchasedReviews
----st_BillAddExistingReview
----st_BillAddExistingAccount

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedDetails]    Script Date: 12/05/2011 14:48:38 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactPurchasedDetails]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactPurchasedDetails]
--GO



--/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedDetails]    Script Date: 12/05/2011 14:48:38 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ContactPurchasedDetails] 
--(
--	@CONTACT_ID nvarchar(50)
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--        declare @PURCHASED_ACCOUNTS table ( 
--	CONTACT_ID int, 
--	CONTACT_NAME nchar(1000),
--	EMAIL nvarchar(500),
--	DATE_CREATED datetime,
--	[EXPIRY_DATE] date,
--	MONTHS_CREDIT smallint,
--	CREATOR_ID int, 
--	LAST_LOGIN datetime,
--	SITE_LIC_ID int null,
--	SITE_LIC_NAME nvarchar(50) null)

--	Insert Into @PURCHASED_ACCOUNTS (CONTACT_ID, CONTACT_NAME, EMAIL, DATE_CREATED, [EXPIRY_DATE],
--					MONTHS_CREDIT, CREATOR_ID, SITE_LIC_ID, SITE_LIC_NAME)
--	SELECT c.[CONTACT_ID], [CONTACT_NAME], EMAIL, c.[DATE_CREATED],
--		 CASE when l.[EXPIRY_DATE] is not null 
--				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
--					then l.[EXPIRY_DATE]
--				else c.[EXPIRY_DATE]
--				end as 'EXPIRY_DATE',
--		  [MONTHS_CREDIT], c.[CREATOR_ID], l.SITE_LIC_ID, l.SITE_LIC_NAME
--	  FROM [Reviewer].[dbo].[TB_CONTACT] c 
--	  left outer join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID
--	  left outer join Reviewer.dbo.TB_SITE_LIC l on lc.SITE_LIC_ID = l.SITE_LIC_ID
--	  where c.CREATOR_ID = @CONTACT_ID
--	  and ((c.EXPIRY_DATE is null and MONTHS_CREDIT != 0)
--			or
--			(c.EXPIRY_DATE is not null))
--	and c.CONTACT_ID != @CONTACT_ID
	

	

--	update  p_a
--	set p_a.LAST_LOGIN = l_t.CREATED
--	--max(l_t.CREATED)
--	from @PURCHASED_ACCOUNTS p_a, TB_LOGON_TICKET l_t
--	where l_t.CREATED in
--	(select max(l_t.CREATED)from TB_LOGON_TICKET l_t
--	where p_a.CONTACT_ID = l_t.CONTACT_ID)

--	select * from @PURCHASED_ACCOUNTS
    

--	RETURN

--END

--GO



--/****** Object:  StoredProcedure [dbo].[st_ContactDetails]    Script Date: 12/05/2011 14:50:16 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactDetails]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactDetails]
--GO



--/****** Object:  StoredProcedure [dbo].[st_ContactDetails]    Script Date: 12/05/2011 14:50:16 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ContactDetails] 
--(
--	@CONTACT_ID nvarchar(50)
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
--    max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
--				CASE when l.[EXPIRY_DATE] is not null 
--				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
--					then l.[EXPIRY_DATE]
--				else c.[EXPIRY_DATE]
--				end as 'EXPIRY_DATE', 
--	c.MONTHS_CREDIT, c.CREATOR_ID,
--    c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
--    l.SITE_LIC_ID, l.SITE_LIC_NAME
--    ,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
--	from Reviewer.dbo.TB_CONTACT c
--	left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
--	on c.CONTACT_ID = lt.CONTACT_ID
--	left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
--	left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
--	where c.CONTACT_ID = @CONTACT_ID
	
--	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
--	c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
--    c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME

--	RETURN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_ContactReviewsShareable]    Script Date: 12/05/2011 14:51:04 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactReviewsShareable]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactReviewsShareable]
--GO


--/****** Object:  StoredProcedure [dbo].[st_ContactReviewsShareable]    Script Date: 12/05/2011 14:51:04 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


---- =============================================
---- Author:		<Jeff>
---- ALTER date: <24/03/2010>
---- Description:	<gets the review data based on contact>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ContactReviewsShareable] 
--(
--	@CONTACT_ID nvarchar(50)
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
	
--    -- Insert statements for procedure here
--    declare @PURCHASED_REVIEWS table ( 
--	REVIEW_ID int, 
--	REVIEW_NAME nchar(1000),
--	DATE_CREATED datetime,
--	[EXPIRY_DATE] date,
--	MONTHS_CREDIT smallint,
--	FUNDER_ID int, 
--	LAST_LOGIN datetime,
--	SITE_LIC_ID int null,
--	SITE_LIC_NAME nvarchar(50) null)

--/*
--Insert Into @PURCHASED_REVIEWS (REVIEW_ID, REVIEW_NAME, DATE_CREATED, [EXPIRY_DATE],
--				MONTHS_CREDIT, FUNDER_ID)
--SELECT [REVIEW_ID], [REVIEW_NAME], [DATE_CREATED], [EXPIRY_DATE], [MONTHS_CREDIT], [FUNDER_ID]
--  FROM [Reviewer].[dbo].[TB_REVIEW] where FUNDER_ID = @CONTACT_ID
--  and ((EXPIRY_DATE is null and MONTHS_CREDIT != 0)
--		or
--		(EXPIRY_DATE is not null))
--*/

--Insert Into @PURCHASED_REVIEWS (REVIEW_ID, REVIEW_NAME, DATE_CREATED, [EXPIRY_DATE],
--				MONTHS_CREDIT, FUNDER_ID)
--SELECT distinct r.[REVIEW_ID], r.[REVIEW_NAME], r.[DATE_CREATED], r.[EXPIRY_DATE], 
--r.[MONTHS_CREDIT], r.[FUNDER_ID]
--  FROM [Reviewer].[dbo].[TB_REVIEW] r
--left outer join [Reviewer].[dbo].[TB_REVIEW_CONTACT] r_c
--on r.REVIEW_ID = r_c.REVIEW_ID
--left outer join [Reviewer].[dbo].[TB_CONTACT_REVIEW_ROLE] c_r_r
--on r_c.REVIEW_CONTACT_ID = c_r_r.REVIEW_CONTACT_ID
--and ((r_c.CONTACT_ID = @CONTACT_ID and c_r_r.ROLE_NAME = 'AdminUser') 
--		or (r.FUNDER_ID = @CONTACT_ID))
--where ((r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0 )
--		or (r.EXPIRY_DATE is not null)) 
--		and 
--		(r.FUNDER_ID = @CONTACT_ID or (c_r_r.ROLE_NAME is not null and ROLE_NAME = 'AdminUser')) 

--update  p_r
--set p_r.LAST_LOGIN = 
--l_t.CREATED
----max(l_t.CREATED)
--from @PURCHASED_REVIEWS p_r, TB_LOGON_TICKET l_t
--where l_t.CREATED in
--(select max(l_t.CREATED)from TB_LOGON_TICKET l_t
--where p_r.FUNDER_ID = l_t.CONTACT_ID
--and p_r.REVIEW_ID = l_t.REVIEW_ID)

--update @PURCHASED_REVIEWS set SITE_LIC_ID = a.site_lic_id, SITE_LIC_NAME = a.SITE_LIC_NAME
--	, [EXPIRY_DATE] = case when L_EXP is not null and [EXPIRY_DATE] > L_EXP  then [EXPIRY_DATE] 
--						else L_EXP
--					end
--from (select ld.SITE_LIC_ID, l.SITE_LIC_NAME, lr.REVIEW_ID as REV_ID , l.[EXPIRY_DATE] as L_EXP
--	from TB_SITE_LIC_DETAILS ld inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on ld.SITE_LIC_ID = lr.SITE_LIC_ID
--	inner join @PURCHASED_REVIEWS pr on pr.REVIEW_ID = lr.REVIEW_ID
--	inner join Reviewer.dbo.TB_SITE_LIC l on lr.SITE_LIC_ID = l.SITE_LIC_ID) as a
--where a.REV_ID = REVIEW_ID
--select * from @PURCHASED_REVIEWS
    
--END


--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_New]    Script Date: 12/05/2011 14:52:02 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Site_Lic_Add_New]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_Site_Lic_Add_New]
--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_New]    Script Date: 12/05/2011 14:52:02 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Add_New] 
--	-- Add the parameters for the stored procedure here
--	@creator_id INT
--	, @admin_id  int -- who will be the first administrator of the site lic, 
--	, @Accounts int 
--	, @reviews int
--	, @months int
--	, @Totalprice int
--	, @SITE_LIC_NAME nvarchar(50)
--	, @COMPANY_NAME nvarchar(50) = ''
--	, @COMPANY_ADDRESS nvarchar(500) = ''
--	, @TELEPHONE nvarchar(50) = ''
--	, @NOTES nvarchar(4000) = null
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    declare @ck int = 0
--    set @ck = (SELECT COUNT(contact_id) from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @creator_id and IS_SITE_ADMIN = 1)
--    if @ck <1 return -1 --only site_admins can create new site licenses!
--	declare @L_ID int, @FS_ID int
--	insert into Reviewer.dbo.TB_SITE_LIC 
--		(
--		SITE_LIC_NAME
--		, COMPANY_NAME
--		, COMPANY_ADDRESS
--		, TELEPHONE
--		, NOTES
--		, CREATOR_ID
--		)
--		VALUES
--		(
--		@SITE_LIC_NAME
--		, @COMPANY_NAME
--		, @COMPANY_ADDRESS
--		, @TELEPHONE
--		, @NOTES
--		, @creator_id
--		)
	
--	set @L_ID = @@IDENTITY
	
--	insert into TB_FOR_SALE 
--	(
--	  [TYPE_NAME]
--      ,[IS_ACTIVE]
--      ,[LAST_CHANGED]
--      ,[PRICE_PER_MONTH]
--      ,[DETAILS]
--     )
--    VALUES
--    (
--     'Site License'
--     , 1
--     , GETDATE()
--     ,CAST((@Totalprice/@months) as int)
--     ,''
--    )
    
--    set @FS_ID = @@IDENTITY
    
--    INSERT into TB_SITE_LIC_DETAILS
--    ( 
--	  [SITE_LIC_ID]
--      ,[FOR_SALE_ID]
--      ,[ACCOUNTS_ALLOWANCE]
--      ,[REVIEWS_ALLOWANCE]
--      ,[MONTHS]
--      ,[VALID_FROM]
--    )
--    values
--    (
--     @L_ID
--     ,@FS_ID
--     ,@Accounts
--     ,@reviews
--     ,@months
--     ,null
--    )
--    insert into TB_SITE_LIC_ADMIN
--    ( 
--     [SITE_LIC_ID]
--     ,[CONTACT_ID]
--    ) VALUES (
--     @L_ID
--     ,@admin_id
--    ) 
--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Details]    Script Date: 12/05/2011 14:52:44 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Site_Lic_Get_Details]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_Site_Lic_Get_Details]
--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Details]    Script Date: 12/05/2011 14:52:44 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Get_Details] 
--	-- Add the parameters for the stored procedure here
	
--	@admin_ID int
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- first results set: currently active license TB_SITE_LIC_DETAILS.VALID_FROM is something and FOR_SALE.IS_ACTIVE = false
--	SELECT TOP 1 * from Reviewer.dbo.TB_SITE_LIC sl 
--		inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
--		inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
--		inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and CONTACT_ID = @admin_ID
--	Order by ld.VALID_FROM desc	
--	-- second results set: currently active renewal offer TB_SITE_LIC_DETAILS.VALID_FROM is NULL and FOR_SALE.IS_ACTIVE = True
--	SELECT TOP 1 * from Reviewer.dbo.TB_SITE_LIC sl 
--		inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is null
--		inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 1
--		inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and CONTACT_ID = @admin_ID
--	Order by ld.VALID_FROM desc
--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Accounts]    Script Date: 12/05/2011 14:53:06 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Site_Lic_Get_Accounts]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_Site_Lic_Get_Accounts]
--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Accounts]    Script Date: 12/05/2011 14:53:06 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Get_Accounts] 
--	-- Add the parameters for the stored procedure here
--	@lic_id int
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    declare @det_id int
--    set @det_id = (select top 1 ld.SITE_LIC_DETAILS_ID from TB_SITE_LIC_DETAILS ld 
--					inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and ld.VALID_FROM is not null
--							and fs.IS_ACTIVE = 0 and ld.SITE_LIC_ID = @lic_id
--					order by ld.VALID_FROM desc)
--	--first, reviews that were added in the past (not associated with currently active SITE_LIC_DETAILS)
--	select * from Reviewer.dbo.TB_REVIEW r
--		inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID and SITE_LIC_ID = @lic_id and SITE_LIC_DETAILS_ID != @det_id
--	--second, reviews that were added to the currently active SITE_LIC_DETAILS
--	select * from Reviewer.dbo.TB_REVIEW r
--		inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID and SITE_LIC_ID = @lic_id and SITE_LIC_DETAILS_ID = @det_id
--	--accounts in the license
--	select * from Reviewer.dbo.TB_CONTACT c
--		inner join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID and lc.SITE_LIC_ID = @lic_id
--	--license admins
--	select * from Reviewer.dbo.TB_CONTACT c
--		inner join TB_SITE_LIC_ADMIN la on c.CONTACT_ID = la.CONTACT_ID and la.SITE_LIC_ID = @lic_id
--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Review]    Script Date: 12/05/2011 14:53:48 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Site_Lic_Add_Review]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_Site_Lic_Add_Review]
--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Review]    Script Date: 12/05/2011 14:53:48 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Add_Review]
--	-- Add the parameters for the stored procedure here
--	@lic_id int
--	,@admin_ID int
--	, @review_id int
--	, @review_name nvarchar(1000)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    declare @ck int, @ck2 int, @lic_det_id int, @res int = 0
--	set @ck = (SELECT count(contact_id) from TB_SITE_LIC_ADMIN 
--	where (CONTACT_ID = @admin_ID or @admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)
--		) and SITE_LIC_ID = @lic_id)
--	if @ck < 1 set @res = -1 --first check: see if supplied admin is actually and admin!
--	--second check, see if review_id exists and has the right name
--	else if  (select count(REVIEW_ID) from Reviewer.dbo.TB_REVIEW where @review_name = REVIEW_NAME and @review_id = REVIEW_ID) != 1
--		set @res = -2
--	else -- initial checks went OK, let's see if we can do it
--	begin
--		set @ck = (select count(review_id) from Reviewer.dbo.TB_SITE_LIC_REVIEW where REVIEW_ID = @review_id)
--		set @lic_det_id = (SELECT TOP 1 ld.SITE_LIC_DETAILS_ID from Reviewer.dbo.TB_SITE_LIC sl 
--								inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
--								inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
--							Order by ld.VALID_FROM desc
--							)
--		--@ck2 counts how many reviews can be added to current lic
--		--if @lic_det_id is null 
--		--	begin
--		--		set @ck2 = (select REVIEWS_ALLOWANCE from TB_SITE_LIC_DETAILS where SITE_LIC_DETAILS_ID = @lic_det_id) -- no review added yet, all allowance is available
--		--		set @lic_det_id 	
--		--	end
--		--else 
--		set @ck2 = (select d.REVIEWS_ALLOWANCE - count(review_id) from TB_SITE_LIC_DETAILS d inner join
--						 Reviewer.dbo.TB_SITE_LIC_REVIEW lr on lr.SITE_LIC_DETAILS_ID = @lic_det_id
--							and lr.SITE_LIC_DETAILS_ID = d.SITE_LIC_DETAILS_ID
--						 group by d.REVIEWS_ALLOWANCE
--						 )--count how many reviews can still be added
--		if @ck != 0 -- review is already in a site lic
--		begin
--			set @ck = (select count(review_id) from Reviewer.dbo.TB_SITE_LIC_REVIEW where REVIEW_ID = @review_id and SITE_LIC_ID = @lic_id)
--			if @ck = 1 set @res = -3 --review already in this site_lic
--			else set @res = -4 --review is in some other site_lic
--		end
--		else if @ck2 < 1 --no allowance available, all review slots for current license have been used
--		begin
--			set @res = -5
--		end
--		else
--		begin --all is well, let's do something!
--			begin transaction --make sure we don't commit only half of the mission critical data! (we assume the update below will work, only checking for the other statement)
--			update Reviewer.dbo.TB_REVIEW set [EXPIRY_DATE] = GETDATE()
--				where REVIEW_ID = @review_id and REVIEW_NAME = @review_name and [EXPIRY_DATE] is null
--			insert into Reviewer.dbo.TB_SITE_LIC_REVIEW (
--				[SITE_LIC_ID]
--				,[SITE_LIC_DETAILS_ID]
--				,[REVIEW_ID]
--				,[ADDED_BY]
--				) values (
--				@lic_id, @lic_det_id, @review_id, @admin_ID
--				)
--			if @@ROWCOUNT = 1
--			begin --write success log
--				commit transaction --all is well, commit
--				insert into TB_SITE_LIC_LOG (
--					[SITE_LIC_DETAILS_ID]
--					  ,[CONTACT_ID]
--					  ,[AFFECTED_ID]
--					  ,[CHANGE_TYPE]
--				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @review_id, 'add review'
--					from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--					order by DATE_CREATED desc
--			end
--			else --write failure log, if this is fired, there is a bug somewhere
--			begin
--				rollback transaction --BAD! something went wrong!
--				insert into TB_SITE_LIC_LOG (
--					[SITE_LIC_DETAILS_ID]
--					  ,[CONTACT_ID]
--					  ,[AFFECTED_ID]
--					  ,[CHANGE_TYPE]
--				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @review_id, 'add review: failed!'
--					from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--					order by DATE_CREATED desc	
--				set @res = -6
--			end
--		end
--	end
--	select r.review_id, review_name from reviewer.dbo.TB_SITE_LIC_REVIEW lr
--		inner join Reviewer.dbo.TB_REVIEW r on lr.REVIEW_ID = r.REVIEW_ID
--	 where SITE_LIC_ID = @lic_id
--	return @res
--	-- error codes: -1 = supplied admin_id is not an admin of this site lice
--	--				-2 = review_id, review_name do not match
--	--				-3 = review already in this site_lic
--	--				-4 = review is in some other site_lic
--	--				-5 = no allowance available, all review slots for current license have been used
--	--				-6 = all seemed well but couldn't write changes! BUG ALERT
--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Remove_Contact]    Script Date: 12/05/2011 14:54:08 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Site_Lic_Add_Remove_Contact]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_Site_Lic_Add_Remove_Contact]
--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Remove_Contact]    Script Date: 12/05/2011 14:54:08 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Add_Remove_Contact]
--	-- Add the parameters for the stored procedure here
--	@lic_id int
--	,@admin_ID int
--	, @contact_id int
--	, @contact_email nvarchar(500)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    declare @ck int, @ck2 int = 0, @res int = 0, @acc_all int
--	set @ck = (SELECT count(contact_id) from TB_SITE_LIC_ADMIN 
--		where (CONTACT_ID = @admin_ID or @admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)
--		) and SITE_LIC_ID = @lic_id)
--	set @acc_all = (select top 1 ACCOUNTS_ALLOWANCE from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id and VALID_FROM is not null order by VALID_FROM desc)
--	set @ck2 = (select @acc_all - COUNT(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT where SITE_LIC_ID = @lic_id)
--	if @ck < 1 set @res = -1 --first check: see if supplied admin id is actually and admin!
--	else if  (select count(contact_id) from Reviewer.dbo.TB_CONTACT where @contact_email = EMAIL and @contact_id = CONTACT_ID) != 1
--		--second check, see if c_id and email are actually present
--		set @res = -2
--	else if @ck2 < 1 --accounts allowance is all used up
--		set @res = -3	
--	else -- checks went OK, let's see if we can do it
--	begin
--		set @ck = (select count(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT where CONTACT_ID = @contact_id)
--		if @ck = 1 -- contact is in the license, we should remove it
--		begin
--			delete from Reviewer.dbo.TB_SITE_LIC_CONTACT where CONTACT_ID = @contact_id and @lic_id = SITE_LIC_ID
--			if @@ROWCOUNT = 1
--			begin --write success log
--				insert into TB_SITE_LIC_LOG (
--					[SITE_LIC_DETAILS_ID]
--					  ,[CONTACT_ID]
--					  ,[AFFECTED_ID]
--					  ,[CHANGE_TYPE]
--				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove contact'
--					from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--					order by DATE_CREATED desc
--			end
--			else --write failure log, if this is fired, there is a bug somewhere
--			begin
--				insert into TB_SITE_LIC_LOG (
--					[SITE_LIC_DETAILS_ID]
--					  ,[CONTACT_ID]
--					  ,[AFFECTED_ID]
--					  ,[CHANGE_TYPE]
--				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove contact: failed!'
--					from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--					order by DATE_CREATED desc	
--				set @res = -4
--			end
--		end	
		
--		else --contact is not in the license, we should add it
--		begin
--			insert into Reviewer.dbo.TB_SITE_LIC_CONTACT (CONTACT_ID, SITE_LIC_ID)
--			values (@contact_id, @lic_id)
--			if @@ROWCOUNT = 1
--			begin
--				insert into TB_SITE_LIC_LOG (
--					[SITE_LIC_DETAILS_ID]
--					  ,[CONTACT_ID]
--					  ,[AFFECTED_ID]
--					  ,[CHANGE_TYPE]
--				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add contact'
--					from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--					order by DATE_CREATED desc
--			end
--			else
--			begin
--				insert into TB_SITE_LIC_LOG (
--					[SITE_LIC_DETAILS_ID]
--					  ,[CONTACT_ID]
--					  ,[AFFECTED_ID]
--					  ,[CHANGE_TYPE]
--				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add contact: failed!'
--					from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--					order by DATE_CREATED desc	
--				set @res = -5
--			end
--		end
--	end
--	select c.CONTACT_ID, CONTACT_NAME, EMAIL from Reviewer.dbo.TB_SITE_LIC_CONTACT lc 
--		inner join Reviewer.dbo.TB_CONTACT c on lc.CONTACT_ID = c.CONTACT_ID
--	where SITE_LIC_ID = @lic_id
--	return @res
--	-- error codes: -1 = supplied admin_id is not an admin of this site lice
--	--				-2 = contact_id and email do not match
--	--				-3 = no allowance available, all account slots for current license have been used
--	--				-4 = tried to remove account but couldn't write changes! BUG ALERT
--	--				-5 = tried to add account but couldn't write changes! BUG ALERT
--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Remove_Admin]    Script Date: 12/05/2011 14:54:32 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Site_Lic_Add_Remove_Admin]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_Site_Lic_Add_Remove_Admin]
--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Remove_Admin]    Script Date: 12/05/2011 14:54:32 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Add_Remove_Admin]
--	-- Add the parameters for the stored procedure here
--	@lic_id int
--	,@admin_ID int
--	, @contact_id int
--	, @contact_email nvarchar(500)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    declare @ck int, @res int = 0
--	set @ck = (SELECT count(contact_id) from TB_SITE_LIC_ADMIN 
--	where (CONTACT_ID = @admin_ID or @admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1))
--			and SITE_LIC_ID = @lic_id)
--	if @ck < 1 set @res = -1 --first check: see if supplied admin is actually and admin!
--	else if  (select count(contact_id) from Reviewer.dbo.TB_CONTACT where @contact_email = EMAIL and @contact_id = CONTACT_ID) != 1
--		--second check, see if c_id and email exist
--		set @res = -2
--	else -- checks went OK, let's see if we can do it
--	begin
--		set @ck = (select count(contact_id) from TB_SITE_LIC_ADMIN where CONTACT_ID = @contact_id)
--		if @ck = 1 -- contact is an admin, we should remove it
--		begin
--			delete from TB_SITE_LIC_ADMIN where CONTACT_ID = @contact_id and @lic_id = SITE_LIC_ID
--			if @@ROWCOUNT = 1
--			begin --write success log
--				insert into TB_SITE_LIC_LOG (
--					[SITE_LIC_DETAILS_ID]
--					  ,[CONTACT_ID]
--					  ,[AFFECTED_ID]
--					  ,[CHANGE_TYPE]
--				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin'
--					from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
--					order by DATE_CREATED desc
--			end
--			else --write failure log, if this is fired, there is a bug somewhere
--			begin
--				insert into TB_SITE_LIC_LOG (
--					[SITE_LIC_DETAILS_ID]
--					  ,[CONTACT_ID]
--					  ,[AFFECTED_ID]
--					  ,[CHANGE_TYPE]
--				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin: failed!'
--					from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
--					order by DATE_CREATED desc	
--				set @res = -3
--			end
--		end	
		
--		else --contact is not an admin, we should add it
--		begin
--			insert into TB_SITE_LIC_ADMIN (CONTACT_ID, SITE_LIC_ID)
--			values (@contact_id, @lic_id)
--			if @@ROWCOUNT = 1
--			begin
--				insert into TB_SITE_LIC_LOG (
--					[SITE_LIC_DETAILS_ID]
--					  ,[CONTACT_ID]
--					  ,[AFFECTED_ID]
--					  ,[CHANGE_TYPE]
--				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin'
--					from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
--					order by DATE_CREATED desc
--			end
--			else
--			begin
--				insert into TB_SITE_LIC_LOG (
--					[SITE_LIC_DETAILS_ID]
--					  ,[CONTACT_ID]
--					  ,[AFFECTED_ID]
--					  ,[CHANGE_TYPE]
--				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin: failed!'
--					from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
--					order by DATE_CREATED desc	
--				set @res = -4
--			end
--		end
--	end
--	select c.CONTACT_ID, CONTACT_NAME, EMAIL from TB_SITE_LIC_ADMIN a
--		inner join Reviewer.dbo.TB_CONTACT c on a.CONTACT_ID = c.CONTACT_ID
--	where SITE_LIC_ID = @lic_id
--	return @res 
--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Activate]    Script Date: 12/05/2011 14:54:53 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_Site_Lic_Activate]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_Site_Lic_Activate]
--GO


--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Activate]    Script Date: 12/05/2011 14:54:53 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	manually activate a site license (not through the shop!)
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Activate]
--	-- Add the parameters for the stored procedure here
--	@lic_id int
--	, @admin_ID int
--	, @lic_details_ID int
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--	-- do all checks: see if you can get a joined record of site_lic, 
--	-- with a site_lic_details line that needs to be activated and a for_sale line that is active and can be administered by @admin_ID
--	declare @check int, @res int = 1
--	set @check = (
--				select count(sld.SITE_LIC_DETAILS_ID) from Reviewer.dbo.TB_SITE_LIC sl 
--					inner join TB_SITE_LIC_DETAILS sld on sl.SITE_LIC_ID = sld.SITE_LIC_ID and sld.VALID_FROM is null 
--						and sld.SITE_LIC_DETAILS_ID = @lic_details_ID
--					inner join TB_FOR_SALE fs on sld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 'True'
--					inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and 
--						(@admin_ID = la.CONTACT_ID or 
--						@admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)
--						)
--				)
--	if @check < 1 return -1 --don't really know what to do if this fails!
--	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
--	BEGIN TRANSACTION
--		declare @months int
--		set @months = (select  d.MONTHS from TB_SITE_LIC_DETAILS d
--						inner join Reviewer.dbo.TB_SITE_LIC l on l.SITE_LIC_ID = d.SITE_LIC_ID
--						where SITE_LIC_DETAILS_ID = @lic_details_ID)
--		--update expiration date
--		update Reviewer.dbo.TB_SITE_LIC 
--			set EXPIRY_DATE = CASE when [EXPIRY_DATE] is not null and [EXPIRY_DATE] > GETDATE()
--									then DATEADD(month, @months, [EXPIRY_DATE])
--								else DATEADD(month, @months, GETDATE())
--								end
--		where SITE_LIC_ID = @lic_id
--		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
--		begin
--			ROLLBACK TRANSACTION
--			return -2
--		end
--		-- update valid from
--		update TB_SITE_LIC_DETAILS set VALID_FROM = GETDATE() where SITE_LIC_DETAILS_ID = @lic_details_ID
--		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
--		begin
--			ROLLBACK TRANSACTION
--			return -2
--		end
--		-- make the offer inactive
--		update TB_FOR_SALE set IS_ACTIVE = 'False'
--		from TB_FOR_SALE fs inner join TB_SITE_LIC_DETAILS d on d.FOR_SALE_ID = fs.FOR_SALE_ID and d.SITE_LIC_DETAILS_ID = @lic_details_ID
--		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
--		begin
--			ROLLBACK TRANSACTION
--			return -2
--		end
--		-- create a record in the log
--		insert into TB_SITE_LIC_LOG
--		(
--		  [SITE_LIC_DETAILS_ID]
--		  ,[CONTACT_ID]
--		  ,[AFFECTED_ID]
--		  ,[CHANGE_TYPE]
--		  ,[REASON]
--		) values
--		(
--		  @lic_details_ID
--		  ,@admin_ID
--		  ,@lic_details_ID
--		  ,'ACTIVATE'
--		  ,'This was done manually, not via the shop'
--		)
--		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
--		begin
--			ROLLBACK TRANSACTION
--			return -2
--		end
--	COMMIT TRANSACTION
--	END TRY

--	BEGIN CATCH
--	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
--	set @res = -2 --to tell the code data was not committed!
--	END CATCH
	
--	return @res

--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedAccounts]    Script Date: 12/05/2011 14:55:32 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactPurchasedAccounts]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactPurchasedAccounts]
--GO


--/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedAccounts]    Script Date: 12/05/2011 14:55:32 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ContactPurchasedAccounts] 
--(
--	@CREATOR_ID int
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--	select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, 
--		c.DATE_CREATED, c.[EXPIRY_DATE], c.MONTHS_CREDIT, c.CREATOR_ID, l.SITE_LIC_ID, l.SITE_LIC_NAME
--		from Reviewer.dbo.TB_CONTACT c
--		left outer join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID
--		left outer join Reviewer.dbo.TB_SITE_LIC l on lc.SITE_LIC_ID = l.SITE_LIC_ID
--		where c.CREATOR_ID = @CREATOR_ID
--		or c.CONTACT_ID = @CREATOR_ID
--		group by c.CONTACT_ID, c.CONTACT_NAME,
--		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID, l.SITE_LIC_ID, l.SITE_LIC_NAME
		
--	UNION
--	select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, 
--		c.DATE_CREATED, c.[EXPIRY_DATE], c.MONTHS_CREDIT, c.CREATOR_ID, l.SITE_LIC_ID, l.SITE_LIC_NAME
--		from Reviewer.dbo.TB_CONTACT c
--		inner join TB_BILL b on b.PURCHASER_CONTACT_ID = @CREATOR_ID
--		inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID = c.CONTACT_ID
--		inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Professional'
--		left outer join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID
--		left outer join Reviewer.dbo.TB_SITE_LIC l on lc.SITE_LIC_ID = l.SITE_LIC_ID
--		group by c.CONTACT_ID, c.CONTACT_NAME,
--		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID, l.SITE_LIC_ID, l.SITE_LIC_NAME
	
--	order by c.CONTACT_NAME

--	RETURN

--END


--GO

--/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedReviews]    Script Date: 12/05/2011 14:55:52 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactPurchasedReviews]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactPurchasedReviews]
--GO


--/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedReviews]    Script Date: 12/05/2011 14:55:52 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ContactPurchasedReviews] 
--(
--	@FUNDER_ID int
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--select r.REVIEW_ID, r.REVIEW_NAME, r.FUNDER_ID, 
--     r.DATE_CREATED, 
--		CASE when l.[EXPIRY_DATE] is not null 
--				and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
--					then l.[EXPIRY_DATE]
--				else r.[EXPIRY_DATE]
--				end as 'EXPIRY_DATE'
--     , r.MONTHS_CREDIT
--     , l.SITE_LIC_ID, l.SITE_LIC_NAME
--	from Reviewer.dbo.TB_REVIEW r
--	left outer join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID
--	left outer join Reviewer.dbo.TB_SITE_LIC l on lr.SITE_LIC_ID = l.SITE_LIC_ID
--	where r.FUNDER_ID = @FUNDER_ID
--	--and r.EXPIRY_DATE is not null
	
--	group by r.REVIEW_ID, r.REVIEW_NAME,
--	r.DATE_CREATED, r.[EXPIRY_DATE], r.MONTHS_CREDIT, r.FUNDER_ID, l.SITE_LIC_ID, l.[EXPIRY_DATE], l.SITE_LIC_NAME
--	UNION
	
--	select r.REVIEW_ID, r.REVIEW_NAME, r.FUNDER_ID, 
--     r.DATE_CREATED, 
--		CASE when l.[EXPIRY_DATE] is not null 
--				and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
--					then l.[EXPIRY_DATE]
--				else r.[EXPIRY_DATE]
--				end as 'EXPIRY_DATE'
--     , r.MONTHS_CREDIT
--    , l.SITE_LIC_ID, l.SITE_LIC_NAME   
--	from Reviewer.dbo.TB_REVIEW r
--	inner join Reviewer.dbo.TB_REVIEW_CONTACT rc
--	on rc.REVIEW_ID = r.REVIEW_ID and rc.CONTACT_ID = @FUNDER_ID
--	left outer join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID
--	left outer join Reviewer.dbo.TB_SITE_LIC l on lr.SITE_LIC_ID = l.SITE_LIC_ID
	
--	UNION
--	select r.REVIEW_ID, r.REVIEW_NAME, r.FUNDER_ID, 
--     r.DATE_CREATED,
--		CASE when l.[EXPIRY_DATE] is not null 
--				and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
--					then l.[EXPIRY_DATE]
--				else r.[EXPIRY_DATE]
--				end as 'EXPIRY_DATE'
--     , r.MONTHS_CREDIT
--     , l.SITE_LIC_ID, l.SITE_LIC_NAME  
--	from Reviewer.dbo.TB_REVIEW r
--	inner join TB_BILL b on b.PURCHASER_CONTACT_ID = @FUNDER_ID
--	inner join TB_BILL_LINE bl on bl.BILL_ID = b.BILL_ID and bl.AFFECTED_ID = r.REVIEW_ID
--	inner join TB_FOR_SALE fs on fs.FOR_SALE_ID = bl.FOR_SALE_ID and fs.TYPE_NAME ='Shareable Review'
--	left outer join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID
--	left outer join Reviewer.dbo.TB_SITE_LIC l on lr.SITE_LIC_ID = l.SITE_LIC_ID

--	group by r.REVIEW_ID, r.REVIEW_NAME,
--	r.DATE_CREATED, r.[EXPIRY_DATE], r.MONTHS_CREDIT, r.FUNDER_ID, l.SITE_LIC_ID, l.[EXPIRY_DATE], l.SITE_LIC_NAME
	
--	order by r.REVIEW_NAME

--	RETURN

--END


--GO


--/****** Object:  StoredProcedure [dbo].[st_BillAddExistingReview]    Script Date: 12/05/2011 14:56:54 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillAddExistingReview]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_BillAddExistingReview]
--GO


--/****** Object:  StoredProcedure [dbo].[st_BillAddExistingReview]    Script Date: 12/05/2011 14:56:54 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


---- =============================================
---- Author:		Sergio
---- Create date: <Create Date,,>
---- Description:	adds an existing review to a draft bill need exact review_ID and matching full name.
---- =============================================
--CREATE PROCEDURE [dbo].[st_BillAddExistingReview]
--	-- Add the parameters for the stored procedure here
--	@bill_ID int,
--	@revID int,
--	@Rev_name nvarchar(1000),
--	@Result nvarchar(100) = 'Success' out
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    declare @FOR_SALE_ID int
--	declare @CHK int
	
--	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
--	--if there is no draft corresponding to the current user, or if there is more than one, then return
--	if @CHK != 1 
--	begin 
--		set @Result = 'Current Draft Bill is invalid, please contact support.'
--		return
--	end
--	select @CHK = count(review_id) from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @revID and REVIEW_NAME = @Rev_name
--	if @CHK != 1 
--	begin 
--		set @Result = 'Could not find this review, please make sure that Review ID and Review Name are correct.'
--		return
--	end
--	select @CHK = COUNT(REVIEW_ID) from Reviewer.dbo.TB_SITE_LIC_REVIEW where REVIEW_ID = @revID 
--	if @CHK != 0 
--	begin 
--		set @Result = 'This review is in a Site License and can''t be purchased individually'
--		return
--	end
--	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
--	where [TYPE_NAME] = 'Shareable Review'
--	order by LAST_CHANGED desc
	
--	--set @EXPIRY_DATE = dateadd(month,@NUMBER_MONTHS, sysdatetime())
	
	
	
--		insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
--		VALUES (@BILL_ID, @FOR_SALE_ID, @revID, 3)
--		if @@ROWCOUNT != 1 set @Result = 'Unknown Error, please contact Support.'
--		else set @Result = 'Success'
	
	
--	RETURN
--END


--GO


--/****** Object:  StoredProcedure [dbo].[st_BillAddExistingAccount]    Script Date: 12/05/2011 15:42:23 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillAddExistingAccount]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_BillAddExistingAccount]
--GO


--/****** Object:  StoredProcedure [dbo].[st_BillAddExistingAccount]    Script Date: 12/05/2011 15:42:23 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



---- =============================================
---- Author:		Sergio
---- Create date: <Create Date,,>
---- Description:	adds an existing review to a draft bill need exact review_ID and matching full name.
---- =============================================
--CREATE PROCEDURE [dbo].[st_BillAddExistingAccount]
--	-- Add the parameters for the stored procedure here
--	@bill_ID int,
--	@ContactID int,
--	@Email nvarchar(1000),
--	@Result nvarchar(100) = 'Success' out
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    declare @FOR_SALE_ID int
--	declare @CHK int
--	set @Result  = 'Success'
--	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
--	--if there is no draft corresponding to the current user, or if there is more than one, then return
--	if @CHK != 1 
--	begin 
--		set @Result = 'Current Draft Bill is invalid, please contact support.'
--		return
--	end
--	select @CHK = count(contact_id) from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @ContactID and EMAIL = @Email
--	if @CHK != 1 
--	begin 
--		set @Result = 'Could not find this Account, please make sure that Account ID and E-Mail address are correct.'
--		return
--	end
--	select @CHK = COUNT(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT where CONTACT_ID = @ContactID 
--	if @CHK != 0 
--	begin 
--		set @Result = 'This account is in a Site License and can''t be purchased individually'
--		return
--	end
--	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
--	where [TYPE_NAME] = 'Professional'
--	order by LAST_CHANGED desc
	
--	--set @EXPIRY_DATE = dateadd(month,@NUMBER_MONTHS, sysdatetime())
	
	
	
--		insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
--		VALUES (@BILL_ID, @FOR_SALE_ID, @ContactID, 3)
--		if @@ROWCOUNT != 1 set @Result = 'Unknown Error, please contact Support.'
	
	
	
--	RETURN
--END
--GO


----USE [ReviewerAdmin]
----GO
----/****** Object:  StoredProcedure [dbo].[st_BillMarkAsPaid]    Script Date: 08/03/2011 15:51:36 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO

------ =============================================
------ Author:		<Author,,Name>
------ Create date: <Create Date,,>
------ Description:	<Description,,>
------ =============================================
----ALTER PROCEDURE [dbo].[st_BillMarkAsPaid]
----	-- Add the parameters for the stored procedure here
----	@BILL_ID int
----AS
----BEGIN
----	-- SET NOCOUNT ON added to prevent extra result sets from
----	-- interfering with SELECT statements.
----	SET NOCOUNT ON;

----    --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
----	BEGIN TRY   
----	BEGIN TRANSACTION
----	--1.extend existing accounts
----	 update Reviewer.dbo.TB_CONTACT set 
----		[EXPIRY_DATE] = case 
----			when ([EXPIRY_DATE] is null) then null
----			when ([EXPIRY_DATE] > getdate()) then DATEADD(month, a.MONTHS, [EXPIRY_DATE])
----			ELSE DATEADD(month, a.MONTHS, getdate())
----		end
----		, MONTHS_CREDIT = case when (EXPIRY_DATE is null and  MONTHS_CREDIT is null)
----			then MONTHS
----			when (EXPIRY_DATE is null and MONTHS_CREDIT is not null) then MONTHS_CREDIT + a.MONTHS
----		else 0
----		end
----		from (
----				Select AFFECTED_ID, bl.MONTHS from TB_BILL_LINE bl
----				inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
----				and BILL_ID = @bill_ID and fs.TYPE_NAME = 'professional' and AFFECTED_ID is not null
----			) a
----	 where CONTACT_ID = a.AFFECTED_ID
----	 --2.extend existing reviews
----	 update Reviewer.dbo.TB_REVIEW set 
----		[EXPIRY_DATE] = case 
----			When ([EXPIRY_DATE] is null) then null
----			when ([EXPIRY_DATE] > getdate()) then DATEADD(month, a.MONTHS, [EXPIRY_DATE])
----			ELSE DATEADD(month, a.MONTHS, getdate())
----		end
----		, MONTHS_CREDIT = Case When (EXPIRY_DATE is null and MONTHS_CREDIT is null) then a.MONTHS
----		when (EXPIRY_DATE is null and MONTHS_CREDIT is not null) then MONTHS_CREDIT + a.MONTHS
----		else 0
----		end
----		from (
----				Select AFFECTED_ID, bl.MONTHS from TB_BILL_LINE bl
----				inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
----				and BILL_ID = @bill_ID and fs.TYPE_NAME = 'Shareable Review' and AFFECTED_ID is not null
----			) a
----	 where REVIEW_ID = a.AFFECTED_ID
----	 --3.create accounts
----		declare @bl int
----		declare cr cursor FAST_FORWARD
----		for select LINE_ID from tb_bill b
----			inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID
----				inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'professional'
----		open cr
----		fetch next from cr into @bl
----		while @@fetch_status=0
----		begin
----			 insert into Reviewer.dbo.TB_CONTACT (CONTACT_NAME, [DATE_CREATED], [EXPIRY_DATE], 
----				MONTHS_CREDIT, CREATOR_ID, [TYPE], IS_SITE_ADMIN)
----				Select Null ,getdate(), Null, MONTHS + 1, PURCHASER_CONTACT_ID, 'Professional', 0
----					from TB_BILL b 
----						inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID and LINE_ID = @bl
----			update TB_BILL_LINE set AFFECTED_ID = @@IDENTITY where LINE_ID = @bl
----			fetch next from cr into @bl
----		end
----		close cr
----		deallocate cr
	
			
----	 --4.create reviews
----		declare cr cursor FAST_FORWARD
----		for select LINE_ID from tb_bill b
----			inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID
----				inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Shareable Review'
----		open cr
----		fetch next from cr into @bl
----		while @@fetch_status=0
----		begin
----			 insert into Reviewer.dbo.TB_REVIEW (REVIEW_NAME, [DATE_CREATED], [EXPIRY_DATE], 
----				MONTHS_CREDIT, FUNDER_ID)
----				select Null, GETDATE(), Null, MONTHS, PURCHASER_CONTACT_ID
----				from TB_BILL b 
----					inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID and LINE_ID = @bl
----			update TB_BILL_LINE set AFFECTED_ID = @@IDENTITY where LINE_ID = @bl
----			fetch next from cr into @bl
----		end
----		close cr
----		deallocate cr

----	--5.change bill to paid
----	update TB_BILL set BILL_STATUS = 'OK: Paid and data committed', DATE_PURCHASED = GETDATE() where BILL_ID = @bill_ID
----	COMMIT TRANSACTION
----	END TRY

----	BEGIN CATCH
----	IF (@@TRANCOUNT > 0) 
----		BEGIN 
----			--error corrections: 1.undo all changes
----			ROLLBACK TRANSACTION
----			--2.mark bill appropriately
----			update TB_BILL set BILL_STATUS = 'FAILURE: paid but data NOT committed', DATE_PURCHASED = GETDATE() where BILL_ID = @bill_ID
----		END 
----	END CATCH

----END



------Use [Reviewer]
------GO
------/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
------BEGIN TRANSACTION
------SET QUOTED_IDENTIFIER ON
------SET ARITHABORT ON
------SET NUMERIC_ROUNDABORT OFF
------SET CONCAT_NULL_YIELDS_NULL ON
------SET ANSI_NULLS ON
------SET ANSI_PADDING ON
------SET ANSI_WARNINGS ON
------COMMIT
------BEGIN TRANSACTION
------GO
------ALTER TABLE dbo.TB_CONTACT
------	DROP CONSTRAINT DF_TB_CONTACT_EXPIRY_DATE
------GO
------ALTER TABLE dbo.TB_CONTACT
------	DROP CONSTRAINT DF_TB_CONTACT_MONTHS_CREDIT
------GO
------ALTER TABLE dbo.TB_CONTACT
------	DROP CONSTRAINT DF_TB_CONTACT_TYPE
------GO
------ALTER TABLE dbo.TB_CONTACT
------	DROP CONSTRAINT DF_TB_CONTACT_IS_SITE_ADMIN
------GO
------CREATE TABLE dbo.Tmp_TB_CONTACT
------	(
------	CONTACT_ID int NOT NULL IDENTITY (1, 1),
------	old_contact_id nvarchar(50) NULL,
------	CONTACT_NAME nvarchar(255) NULL,
------	USERNAME varchar(50) NULL,
------	PASSWORD varchar(50) NULL,
------	LAST_LOGIN datetime NULL,
------	DATE_CREATED datetime NULL,
------	EMAIL nvarchar(500) NULL,
------	EXPIRY_DATE date NULL,
------	MONTHS_CREDIT smallint NOT NULL,
------	CREATOR_ID int NULL,
------	TYPE nchar(12) NOT NULL,
------	IS_SITE_ADMIN bit NOT NULL,
------	DESCRIPTION nvarchar(1000) NULL
------	)  ON [PRIMARY]
------GO
------ALTER TABLE dbo.Tmp_TB_CONTACT SET (LOCK_ESCALATION = TABLE)
------GO
------ALTER TABLE dbo.Tmp_TB_CONTACT ADD CONSTRAINT
------	DF_TB_CONTACT_EXPIRY_DATE DEFAULT (dateadd(month,(1),getdate())) FOR EXPIRY_DATE
------GO
------ALTER TABLE dbo.Tmp_TB_CONTACT ADD CONSTRAINT
------	DF_TB_CONTACT_MONTHS_CREDIT DEFAULT ((0)) FOR MONTHS_CREDIT
------GO
------ALTER TABLE dbo.Tmp_TB_CONTACT ADD CONSTRAINT
------	DF_TB_CONTACT_TYPE DEFAULT ('Professional') FOR TYPE
------GO
------ALTER TABLE dbo.Tmp_TB_CONTACT ADD CONSTRAINT
------	DF_TB_CONTACT_IS_SITE_ADMIN DEFAULT ((0)) FOR IS_SITE_ADMIN
------GO
------SET IDENTITY_INSERT dbo.Tmp_TB_CONTACT ON
------GO
------IF EXISTS(SELECT * FROM dbo.TB_CONTACT)
------	 EXEC('INSERT INTO dbo.Tmp_TB_CONTACT (CONTACT_ID, old_contact_id, CONTACT_NAME, USERNAME, PASSWORD, LAST_LOGIN, DATE_CREATED, EMAIL, EXPIRY_DATE, MONTHS_CREDIT, CREATOR_ID, TYPE, IS_SITE_ADMIN, DESCRIPTION)
------		SELECT CONTACT_ID, old_contact_id, CONTACT_NAME, USERNAME, PASSWORD, LAST_LOGIN, DATE_CREATED, EMAIL, EXPIRY_DATE, MONTHS_CREDIT, CREATOR_ID, TYPE, IS_SITE_ADMIN, DESCRIPTION FROM dbo.TB_CONTACT WITH (HOLDLOCK TABLOCKX)')
------GO
------SET IDENTITY_INSERT dbo.Tmp_TB_CONTACT OFF
------GO
------ALTER TABLE dbo.TB_TRAINING
------	DROP CONSTRAINT FK_TB_TRAINING_TB_CONTACT
------GO
------ALTER TABLE dbo.TB_ATTRIBUTE
------	DROP CONSTRAINT FK_TB_ATTRIBUTE_tb_CONTACT
------GO
------ALTER TABLE dbo.TB_META_ANALYSIS
------	DROP CONSTRAINT FK_TB_META_ANALYSIS_TB_CONTACT
------GO
------ALTER TABLE dbo.TB_REPORT
------	DROP CONSTRAINT FK_TB_REPORT_TB_CONTACT
------GO
------ALTER TABLE dbo.TB_SEARCH
------	DROP CONSTRAINT FK_TB_SEARCH_tb_CONTACT
------GO
------ALTER TABLE dbo.TB_REVIEW_CONTACT
------	DROP CONSTRAINT FK_TB_REVIEW_CONTACT_tb_CONTACT
------GO
------ALTER TABLE dbo.TB_WORK_ALLOCATION
------	DROP CONSTRAINT FK_TB_WORK_ALLOCATION_tb_CONTACT
------GO
------ALTER TABLE dbo.TB_ITEM_SET
------	DROP CONSTRAINT FK_TB_ITEM_SET_tb_CONTACT
------GO
------DROP TABLE dbo.TB_CONTACT
------GO
------EXECUTE sp_rename N'dbo.Tmp_TB_CONTACT', N'TB_CONTACT', 'OBJECT' 
------GO
------ALTER TABLE dbo.TB_CONTACT ADD CONSTRAINT
------	PK_tb_CONTACT PRIMARY KEY CLUSTERED 
------	(
------	CONTACT_ID
------	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

------GO
------COMMIT
------BEGIN TRANSACTION
------GO
------ALTER TABLE dbo.TB_ITEM_SET ADD CONSTRAINT
------	FK_TB_ITEM_SET_tb_CONTACT FOREIGN KEY
------	(
------	CONTACT_ID
------	) REFERENCES dbo.TB_CONTACT
------	(
------	CONTACT_ID
------	) ON UPDATE  NO ACTION 
------	 ON DELETE  NO ACTION 
	
------GO
------ALTER TABLE dbo.TB_ITEM_SET SET (LOCK_ESCALATION = TABLE)
------GO
------COMMIT
------BEGIN TRANSACTION
------GO
------ALTER TABLE dbo.TB_WORK_ALLOCATION ADD CONSTRAINT
------	FK_TB_WORK_ALLOCATION_tb_CONTACT FOREIGN KEY
------	(
------	CONTACT_ID
------	) REFERENCES dbo.TB_CONTACT
------	(
------	CONTACT_ID
------	) ON UPDATE  NO ACTION 
------	 ON DELETE  NO ACTION 
	
------GO
------ALTER TABLE dbo.TB_WORK_ALLOCATION SET (LOCK_ESCALATION = TABLE)
------GO
------COMMIT
------BEGIN TRANSACTION
------GO
------ALTER TABLE dbo.TB_REVIEW_CONTACT ADD CONSTRAINT
------	FK_TB_REVIEW_CONTACT_tb_CONTACT FOREIGN KEY
------	(
------	CONTACT_ID
------	) REFERENCES dbo.TB_CONTACT
------	(
------	CONTACT_ID
------	) ON UPDATE  NO ACTION 
------	 ON DELETE  NO ACTION 
	
------GO
------ALTER TABLE dbo.TB_REVIEW_CONTACT SET (LOCK_ESCALATION = TABLE)
------GO
------COMMIT
------BEGIN TRANSACTION
------GO
------ALTER TABLE dbo.TB_SEARCH ADD CONSTRAINT
------	FK_TB_SEARCH_tb_CONTACT FOREIGN KEY
------	(
------	CONTACT_ID
------	) REFERENCES dbo.TB_CONTACT
------	(
------	CONTACT_ID
------	) ON UPDATE  NO ACTION 
------	 ON DELETE  NO ACTION 
	
------GO
------ALTER TABLE dbo.TB_SEARCH SET (LOCK_ESCALATION = TABLE)
------GO
------COMMIT
------BEGIN TRANSACTION
------GO
------ALTER TABLE dbo.TB_REPORT ADD CONSTRAINT
------	FK_TB_REPORT_TB_CONTACT FOREIGN KEY
------	(
------	CONTACT_ID
------	) REFERENCES dbo.TB_CONTACT
------	(
------	CONTACT_ID
------	) ON UPDATE  NO ACTION 
------	 ON DELETE  NO ACTION 
	
------GO
------ALTER TABLE dbo.TB_REPORT SET (LOCK_ESCALATION = TABLE)
------GO
------COMMIT
------BEGIN TRANSACTION
------GO
------ALTER TABLE dbo.TB_META_ANALYSIS ADD CONSTRAINT
------	FK_TB_META_ANALYSIS_TB_CONTACT FOREIGN KEY
------	(
------	CONTACT_ID
------	) REFERENCES dbo.TB_CONTACT
------	(
------	CONTACT_ID
------	) ON UPDATE  NO ACTION 
------	 ON DELETE  NO ACTION 
	
------GO
------ALTER TABLE dbo.TB_META_ANALYSIS SET (LOCK_ESCALATION = TABLE)
------GO
------COMMIT
------BEGIN TRANSACTION
------GO
------ALTER TABLE dbo.TB_ATTRIBUTE ADD CONSTRAINT
------	FK_TB_ATTRIBUTE_tb_CONTACT FOREIGN KEY
------	(
------	CONTACT_ID
------	) REFERENCES dbo.TB_CONTACT
------	(
------	CONTACT_ID
------	) ON UPDATE  NO ACTION 
------	 ON DELETE  NO ACTION 
	
------GO
------ALTER TABLE dbo.TB_ATTRIBUTE SET (LOCK_ESCALATION = TABLE)
------GO
------COMMIT
------BEGIN TRANSACTION
------GO
------ALTER TABLE dbo.TB_TRAINING ADD CONSTRAINT
------	FK_TB_TRAINING_TB_CONTACT FOREIGN KEY
------	(
------	CONTACT_ID
------	) REFERENCES dbo.TB_CONTACT
------	(
------	CONTACT_ID
------	) ON UPDATE  NO ACTION 
------	 ON DELETE  NO ACTION 
	
------GO
------ALTER TABLE dbo.TB_TRAINING SET (LOCK_ESCALATION = TABLE)
------GO
------COMMIT

------Use [ReviewerAdmin]
------GO

------/*** To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
------BEGIN TRANSACTION
------SET QUOTED_IDENTIFIER ON
------SET ARITHABORT ON
------SET NUMERIC_ROUNDABORT OFF
------SET CONCAT_NULL_YIELDS_NULL ON
------SET ANSI_NULLS ON
------SET ANSI_PADDING ON
------SET ANSI_WARNINGS ON
------COMMIT
------BEGIN TRANSACTION
------GO
------ALTER TABLE dbo.TB_BILL
------	DROP CONSTRAINT DF_TB_BILL_BILL_STATUS
------GO
------ALTER TABLE dbo.TB_BILL ADD CONSTRAINT
------	DF_TB_BILL_BILL_STATUS DEFAULT ('Draft') FOR BILL_STATUS
------GO
------ALTER TABLE dbo.TB_BILL SET (LOCK_ESCALATION = TABLE)
------GO
------COMMIT
------GO



------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillAddContactExtension]    Script Date: 06/21/2011 16:05:29 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillAddContactExtension]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillAddContactExtension]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillAddContactExtension]    Script Date: 06/21/2011 16:05:29 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

------CREATE procedure [dbo].[st_BillAddContactExtension]
------(
------	@ACCOUNT_CREATOR_ID int,
------	@CONTACT_ID int,
------	@EXTEND_BY int,
------	@BILL_ID int
------)

------As

------SET NOCOUNT ON
--------declare @COST int
--------declare @ACCOUNT_COST int
------declare @FOR_SALE_ID int
------declare @CHK int
------	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
------	--if there is no draft corresponding to the current user, or if there is more than one, then return
------	if @CHK != 1 return
	
	
------	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
------	where [TYPE_NAME] = 'Professional'
------	order by LAST_CHANGED desc
------	--set @ACCOUNT_COST = @COST * @EXTEND_BY 
------	SELECT @CHK = COUNT(line_ID) from TB_BILL_LINE
------		where BILL_ID = @BILL_ID and AFFECTED_ID = @CONTACT_ID and FOR_SALE_ID = @FOR_SALE_ID
------	if @CHK = 1 
------	begin 
------		if @EXTEND_BY= 0 delete from TB_BILL_LINE where BILL_ID = @BILL_ID and AFFECTED_ID = @CONTACT_ID and FOR_SALE_ID = @FOR_SALE_ID
------		else update TB_BILL_LINE set MONTHS = @EXTEND_BY 
------			where BILL_ID = @BILL_ID and AFFECTED_ID = @CONTACT_ID and FOR_SALE_ID = @FOR_SALE_ID
------	end
------	else
------	begin
------		if @EXTEND_BY= 0 delete from TB_BILL_LINE where BILL_ID = @BILL_ID and AFFECTED_ID = @CONTACT_ID and FOR_SALE_ID = @FOR_SALE_ID
------		else insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
------			values (@BILL_ID, @FOR_SALE_ID, @CONTACT_ID, @EXTEND_BY)
------	end

------SET NOCOUNT OFF

------GO
------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillAddExistingAccount]    Script Date: 06/21/2011 16:06:18 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillAddExistingAccount]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillAddExistingAccount]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillAddExistingAccount]    Script Date: 06/21/2011 16:06:18 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO


-------- =============================================
-------- Author:		Sergio
-------- Create date: <Create Date,,>
-------- Description:	adds an existing review to a draft bill need exact review_ID and matching full name.
-------- =============================================
------CREATE PROCEDURE [dbo].[st_BillAddExistingAccount]
------	-- Add the parameters for the stored procedure here
------	@bill_ID int,
------	@ContactID int,
------	@Email nvarchar(1000),
------	@Result nvarchar(100) = 'Success' out
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    declare @FOR_SALE_ID int
------	declare @CHK int
------	set @Result  = 'Success'
------	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
------	--if there is no draft corresponding to the current user, or if there is more than one, then return
------	if @CHK != 1 
------	begin 
------		set @Result = 'Current Draft Bill is invalid, please contact support.'
------		return
------	end
------	select @CHK = count(contact_id) from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @ContactID and EMAIL = @Email
------	if @CHK != 1 
------	begin 
------		set @Result = 'Could not find this Account, please make sure that Account ID and E-Mail address are correct.'
------		return
------	end
------	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
------	where [TYPE_NAME] = 'Professional'
------	order by LAST_CHANGED desc
	
------	--set @EXPIRY_DATE = dateadd(month,@NUMBER_MONTHS, sysdatetime())
	
	
	
------		insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
------		VALUES (@BILL_ID, @FOR_SALE_ID, @ContactID, 3)
------		if @@ROWCOUNT != 1 set @Result = 'Unknown Error, please contact Support.'
	
	
	
------	RETURN
------END


------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillAddExistingReview]    Script Date: 06/21/2011 16:06:44 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillAddExistingReview]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillAddExistingReview]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillAddExistingReview]    Script Date: 06/21/2011 16:06:44 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		Sergio
-------- Create date: <Create Date,,>
-------- Description:	adds an existing review to a draft bill need exact review_ID and matching full name.
-------- =============================================
------CREATE PROCEDURE [dbo].[st_BillAddExistingReview]
------	-- Add the parameters for the stored procedure here
------	@bill_ID int,
------	@revID int,
------	@Rev_name nvarchar(1000),
------	@Result nvarchar(100) = 'Success' out
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    declare @FOR_SALE_ID int
------	declare @CHK int
	
------	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
------	--if there is no draft corresponding to the current user, or if there is more than one, then return
------	if @CHK != 1 
------	begin 
------		set @Result = 'Current Draft Bill is invalid, please contact support.'
------		return
------	end
------	select @CHK = count(review_id) from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @revID and REVIEW_NAME = @Rev_name
------	if @CHK != 1 
------	begin 
------		set @Result = 'Could not find this review, please make sure that Review ID and Review Name are correct.'
------		return
------	end
------	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
------	where [TYPE_NAME] = 'Shareable Review'
------	order by LAST_CHANGED desc
	
------	--set @EXPIRY_DATE = dateadd(month,@NUMBER_MONTHS, sysdatetime())
	
	
	
------		insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
------		VALUES (@BILL_ID, @FOR_SALE_ID, @revID, 3)
------		if @@ROWCOUNT != 1 set @Result = 'Unknown Error, please contact Support.'
------		else set @Result = 'Success'
	
	
------	RETURN
------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillAddNewContact]    Script Date: 06/21/2011 16:07:15 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillAddNewContact]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillAddNewContact]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillAddNewContact]    Script Date: 06/21/2011 16:07:15 ******/
------SET ANSI_NULLS OFF
------GO

------SET QUOTED_IDENTIFIER ON
------GO

------CREATE PROCEDURE [dbo].[st_BillAddNewContact]
------(
------	@CREATOR_ID int, 
------	@NUMBER_MONTHS int,
------	@BILL_ID int 
------	--@WHEN_TO_START nvarchar(50)
------)
------AS

------	declare @FOR_SALE_ID int
------	declare @CHK int
	
------	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
------	--if there is no draft corresponding to the current user, or if there is more than one, then return
------	if @CHK != 1 return
------	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
------	where [TYPE_NAME] = 'Professional'
------	order by LAST_CHANGED desc
	
------	--set @EXPIRY_DATE = dateadd(month,@NUMBER_MONTHS, sysdatetime())
	
	
	
------		insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
------		VALUES (@BILL_ID, @FOR_SALE_ID, NULL, @NUMBER_MONTHS)
		
------		--INSERT INTO TB_CONTACT_TMP (PURCHASER_ID, DATE_CREATED, EXPIRY_DATE, WHEN_TO_START, NUMBER_MONTHS, MONTHS_CREDIT)
------		--VALUES (@CREATOR_ID, GETDATE(), null, @WHEN_TO_START, @NUMBER_MONTHS, @NUMBER_MONTHS)
------	--END
------	--ELSE
------	--BEGIN
------	--	INSERT INTO TB_CONTACT_TMP (PURCHASER_ID, DATE_CREATED, EXPIRY_DATE, WHEN_TO_START, NUMBER_MONTHS, MONTHS_CREDIT)
------	--	VALUES (@CREATOR_ID, GETDATE(), @EXPIRY_DATE, @WHEN_TO_START, @NUMBER_MONTHS, '0')	
------	--END
	
------	--set @NEW_CONTACT_ID = @@IDENTITY
	
------	--insert into TB_ACCOUNT_EXTENSION (ACCOUNT_CREATOR_ID, CONTACT_ID, EXTEND_BY)
------	--values (@CREATOR_ID, @NEW_CONTACT_ID, @NUMBER_MONTHS)
	
	
------	RETURN

------GO


------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillAddNewReview]    Script Date: 06/21/2011 16:07:32 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillAddNewReview]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillAddNewReview]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillAddNewReview]    Script Date: 06/21/2011 16:07:32 ******/
------SET ANSI_NULLS OFF
------GO

------SET QUOTED_IDENTIFIER ON
------GO

------CREATE PROCEDURE [dbo].[st_BillAddNewReview]
------(
------	@CREATOR_ID int, 
------	@NUMBER_MONTHS int,
------	@BILL_ID int 
------	--@WHEN_TO_START nvarchar(50)
------)
------AS

------	declare @FOR_SALE_ID int
------	declare @CHK int
	
------	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
------	--if there is no draft corresponding to the current user, or if there is more than one, then return
------	if @CHK != 1 return
------	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
------	where [TYPE_NAME] = 'Shareable Review'
------	order by LAST_CHANGED desc
------	insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
------	VALUES (@BILL_ID, @FOR_SALE_ID, NULL, @NUMBER_MONTHS)
------	RETURN

------GO


------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillCreate]    Script Date: 06/21/2011 16:17:38 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillCreate]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillCreate]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillCreate]    Script Date: 06/21/2011 16:17:38 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

------CREATE procedure [dbo].[st_BillCreate]
------(
------	@CONTACT_ID int,
------	@NOMINAL_PRICE int,
------	@VAT nvarchar(50),
------	@DUE_PRICE nvarchar(50),
------	@BILL_ID int output
------)

------As

------SET NOCOUNT ON
------declare @NEW_BILL_ID int
------declare @VAT_CHARGED bit
------declare @VAT_RATE nvarchar(50)
------declare @EU_VAT_REG_NUMBER nvarchar(50)

------	insert into TB_BILL(PURCHASER_CONTACT_ID, NOMINAL_PRICE, VAT, DUE_PRICE)
------	values (@CONTACT_ID, @NOMINAL_PRICE, @VAT, 0)
	
------	set @NEW_BILL_ID = @@IDENTITY
	
------	-- add tax and condition details
	
------	update TB_BILL
------	set CONDITIONS_ID = 
------	(select top 1 CONDITIONS_ID from TB_TERMS_AND_CONDITIONS order by CONDITIONS_ID DESC)
------	where BILL_ID = @NEW_BILL_ID
	
------	update TB_BILL
------	set VAT_RATE = 
------	(select top 1 VAT_RATE from TB_TERMS_AND_CONDITIONS order by CONDITIONS_ID DESC)
------	where BILL_ID = @NEW_BILL_ID
	
------	update TB_BILL
------	set VAT = @VAT where BILL_ID = @NEW_BILL_ID
	
------	select @BILL_ID = BILL_ID from TB_BILL where BILL_ID = @NEW_BILL_ID

------SET NOCOUNT OFF

------GO


------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillExtendGhost]    Script Date: 06/21/2011 16:20:34 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillExtendGhost]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillExtendGhost]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillExtendGhost]    Script Date: 06/21/2011 16:20:34 ******/
------SET ANSI_NULLS OFF
------GO

------SET QUOTED_IDENTIFIER ON
------GO

------CREATE PROCEDURE [dbo].[st_BillExtendGhost]
------(
------	@CREATOR_ID int, 
------	@BILL_LINE_ID int,
------	@NUMBER_MONTHS int,
------	@BILL_ID int
------)
------AS

------	declare @FOR_SALE_ID int
------	declare @CHK int
	
------	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
------	--if there is no draft corresponding to the current user, or if there is more than one, then return
------	if @CHK != 1 return
------	SELECT @CHK = bl.LINE_ID from TB_BILL_LINE bl 
------		inner join TB_BILL b on b.BILL_ID = bl.BILL_ID and bl.BILL_ID = @BILL_ID and LINE_ID = @BILL_LINE_ID and b.BILL_STATUS = 'Draft'

------	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
------	where [TYPE_NAME] = 'Professional'
------	order by LAST_CHANGED desc
------	if @NUMBER_MONTHS = 0 delete from TB_BILL_LINE where LINE_ID = @CHK AND @FOR_SALE_ID = FOR_SALE_ID
------	else UPDATE TB_BILL_LINE set MONTHS = @NUMBER_MONTHS
------		where LINE_ID = @CHK AND @FOR_SALE_ID = FOR_SALE_ID

------GO


------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillGetDraft]    Script Date: 06/21/2011 16:21:25 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillGetDraft]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillGetDraft]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillGetDraft]    Script Date: 06/21/2011 16:21:25 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		<Author,,Name>
-------- Create date: <Create Date,,>
-------- Description:	<Description,,>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_BillGetDraft]
------(
------	@CONTACT int
------)
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;
------	--first, mark as expired the submitted bills that did not end up with success or failure
------	update TB_BILL set BILL_STATUS = 'submitted to WPM: timed out'
------		where BILL_STATUS = 'submitted to WPM' and PURCHASER_CONTACT_ID = @CONTACT
------    -- get the details from tb_bill
------	select c.CONTACT_NAME, b.BILL_ID, b.DATE_PURCHASED, b.NOMINAL_PRICE, b.DISCOUNT,
------	b.DUE_PRICE, b.CONDITIONS_ID, b.BILL_STATUS, b.DATE_PAYMENT_RECEIVED,
------	b.PURCHASER_CONTACT_ID, b.VAT
------	from TB_BILL b
------	inner join Reviewer.dbo.TB_CONTACT c
------	on b.PURCHASER_CONTACT_ID = c.CONTACT_ID AND c.CONTACT_ID = @CONTACT
------	and b.BILL_STATUS = 'Draft'
------	--second reader get the bill lines
------	select LINE_ID, bl.BILL_ID, bl.FOR_SALE_ID, fs.TYPE_NAME, AFFECTED_ID, MONTHS MONTHS_CREDIT, b.PURCHASER_CONTACT_ID, PRICE_PER_MONTH * MONTHS COST
------	, c.CONTACT_NAME as AFFECTED_NAME
------	From TB_BILL_LINE bl inner join TB_BILL b 
------		on bl.BILL_ID = b.BILL_ID and b.BILL_STATUS = 'Draft' and b.PURCHASER_CONTACT_ID = @CONTACT
------		inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Professional'
------		left outer join Reviewer.dbo.TB_CONTACT c on bl.AFFECTED_ID = c.CONTACT_ID and bl.AFFECTED_ID is not null
------	UNION
------	select LINE_ID, bl.BILL_ID, bl.FOR_SALE_ID, fs.TYPE_NAME, AFFECTED_ID, MONTHS MONTHS_CREDIT, b.PURCHASER_CONTACT_ID, PRICE_PER_MONTH * MONTHS COST
------	, r.REVIEW_NAME as AFFECTED_NAME
------	From TB_BILL_LINE bl inner join TB_BILL b 
------		on bl.BILL_ID = b.BILL_ID and b.BILL_STATUS = 'Draft' and b.PURCHASER_CONTACT_ID = @CONTACT
------		inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Shareable Review'
------		left outer join Reviewer.dbo.TB_REVIEW r on bl.AFFECTED_ID = r.REVIEW_ID and bl.AFFECTED_ID is not null
------	RETURN

------END

------GO


------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillGetSubmitted]    Script Date: 06/21/2011 16:21:48 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillGetSubmitted]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillGetSubmitted]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillGetSubmitted]    Script Date: 06/21/2011 16:21:48 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		<Author,,Name>
-------- Create date: <Create Date,,>
-------- Description:	<Description,,>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_BillGetSubmitted]
------	-- Add the parameters for the stored procedure here
------	@CONTACT_ID int,
------	@BILL_ID int
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    -- Insert statements for procedure here
------	SELECT * from TB_BILL
------	where TB_BILL.BILL_ID = @BILL_ID and (BILL_STATUS = 'submitted to WPM' or BILL_STATUS = 'submitted to WPM: timed out')
------	 and PURCHASER_CONTACT_ID = @CONTACT_ID
------END

------GO


------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillLineCreate]    Script Date: 06/21/2011 16:26:12 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillLineCreate]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillLineCreate]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillMarkAsFailed]    Script Date: 06/21/2011 16:27:31 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillMarkAsFailed]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillMarkAsFailed]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillMarkAsFailed]    Script Date: 06/21/2011 16:27:31 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		<Author,,Name>
-------- Create date: <Create Date,,>
-------- Description:	<Description,,>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_BillMarkAsFailed]
------	-- Add the parameters for the stored procedure here
------	@BILL_ID int,
------	@WHY nvarchar(50)
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
	
------	update TB_BILL set BILL_STATUS = @WHY where BILL_ID = @bill_ID
	

------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillMarkAsPaid]    Script Date: 06/21/2011 16:27:43 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillMarkAsPaid]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillMarkAsPaid]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillMarkAsPaid]    Script Date: 06/21/2011 16:27:43 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		<Author,,Name>
-------- Create date: <Create Date,,>
-------- Description:	<Description,,>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_BillMarkAsPaid]
------	-- Add the parameters for the stored procedure here
------	@BILL_ID int
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
------	BEGIN TRY   
------	BEGIN TRANSACTION
------	--1.extend existing accounts
------	 update Reviewer.dbo.TB_CONTACT set 
------		[EXPIRY_DATE] = case 
------			when ([EXPIRY_DATE] is null) then null
------			when ([EXPIRY_DATE] > getdate()) then DATEADD(month, a.MONTHS, [EXPIRY_DATE])
------			ELSE DATEADD(month, a.MONTHS, getdate())
------		end
------		, MONTHS_CREDIT = case when (EXPIRY_DATE is null and  MONTHS_CREDIT is null)
------			then MONTHS
------			when (EXPIRY_DATE is null and MONTHS_CREDIT is not null) then MONTHS_CREDIT + a.MONTHS
------		else 0
------		end
------		from (
------				Select AFFECTED_ID, bl.MONTHS from TB_BILL_LINE bl
------				inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
------				and BILL_ID = @bill_ID and fs.TYPE_NAME = 'professional' and AFFECTED_ID is not null
------			) a
------	 where CONTACT_ID = a.AFFECTED_ID
------	 --2.extend existing reviews
------	 update Reviewer.dbo.TB_REVIEW set 
------		[EXPIRY_DATE] = case 
------			When ([EXPIRY_DATE] is null) then null
------			when ([EXPIRY_DATE] > getdate()) then DATEADD(month, a.MONTHS, [EXPIRY_DATE])
------			ELSE DATEADD(month, a.MONTHS, getdate())
------		end
------		, MONTHS_CREDIT = Case When (EXPIRY_DATE is null and MONTHS_CREDIT is null) then a.MONTHS
------		when (EXPIRY_DATE is null and MONTHS_CREDIT is not null) then MONTHS_CREDIT + a.MONTHS
------		else 0
------		end
------		from (
------				Select AFFECTED_ID, bl.MONTHS from TB_BILL_LINE bl
------				inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
------				and BILL_ID = @bill_ID and fs.TYPE_NAME = 'Shareable Review' and AFFECTED_ID is not null
------			) a
------	 where REVIEW_ID = a.AFFECTED_ID
------	 --3.create accounts
------	 insert into Reviewer.dbo.TB_CONTACT (CONTACT_NAME, [DATE_CREATED], [EXPIRY_DATE], 
------		MONTHS_CREDIT, CREATOR_ID, [TYPE], IS_SITE_ADMIN)
------		Select Null ,getdate(), Null, MONTHS + 1, PURCHASER_CONTACT_ID, 'Professional', 0
------			from TB_BILL b 
------				inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID
------				inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'professional'
			
------	 --4.create reviews
------	 insert into Reviewer.dbo.TB_REVIEW (REVIEW_NAME, [DATE_CREATED], [EXPIRY_DATE], 
------		MONTHS_CREDIT, FUNDER_ID)
------		select Null, GETDATE(), Null, MONTHS, PURCHASER_CONTACT_ID
------		from TB_BILL b 
------			inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID
------			inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Shareable Review'
------	--5.change bill to paid
------	update TB_BILL set BILL_STATUS = 'OK: Paid and data committed' where BILL_ID = @bill_ID
------	COMMIT TRANSACTION
------	END TRY

------	BEGIN CATCH
------	IF (@@TRANCOUNT > 0) 
------		BEGIN 
------			--error corrections: 1.undo all changes
------			ROLLBACK TRANSACTION
------			--2.mark bill appropriately
------			update TB_BILL set BILL_STATUS = 'FAILURE: paid but data NOT committed' where BILL_ID = @bill_ID
------		END 
------	END CATCH

------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillMarkAsSubmitted]    Script Date: 06/21/2011 16:28:08 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillMarkAsSubmitted]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillMarkAsSubmitted]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillMarkAsSubmitted]    Script Date: 06/21/2011 16:28:08 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		<Author,,Name>
-------- Create date: <Create Date,,>
-------- Description:	<Description,,>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_BillMarkAsSubmitted]
------	-- Add the parameters for the stored procedure here
------	@CONTACT_ID int,
------	@BILL_ID int,
------	@VAT float 
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    -- Insert statements for procedure here
------	UPDATE TB_BILL set BILL_STATUS = 'submitted to WPM', NOMINAL_PRICE = price, DUE_PRICE = price, VAT = @VAT --CONVERT(nvarchar(50),price*VAT_RATE/100) 
------	from (
------		select CAST(sum(bl.MONTHS * fs.PRICE_PER_MONTH) as float ) price from TB_BILL_LINE bl 
------			inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
------		where bl.BILL_ID = @BILL_ID
------		) as a
------	where TB_BILL.BILL_ID = @BILL_ID and BILL_STATUS = 'Draft' and PURCHASER_CONTACT_ID = @CONTACT_ID
------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillRemoveGhost]    Script Date: 06/21/2011 16:28:34 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillRemoveGhost]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillRemoveGhost]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillRemoveGhost]    Script Date: 06/21/2011 16:28:34 ******/
------SET ANSI_NULLS OFF
------GO

------SET QUOTED_IDENTIFIER ON
------GO

------CREATE PROCEDURE [dbo].[st_BillRemoveGhost]
------(
------	@CREATOR_ID int, 
------	@BILL_LINE_ID int,
------	@BILL_ID int
------)
------AS

------	declare @FOR_SALE_ID int
------	declare @CHK int
	
------	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
------	--if there is no draft corresponding to the current user, or if there is more than one, then return
------	if @CHK != 1 return
------	SELECT @CHK = bl.LINE_ID from TB_BILL_LINE bl 
------		inner join TB_BILL b on b.BILL_ID = bl.BILL_ID and bl.BILL_ID = @BILL_ID and LINE_ID = @BILL_LINE_ID and b.BILL_STATUS = 'Draft'

------	DELETE from TB_BILL_LINE where LINE_ID = @CHK

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillReviewExtend]    Script Date: 06/21/2011 16:31:00 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillReviewExtend]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillReviewExtend]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillReviewExtend]    Script Date: 06/21/2011 16:31:00 ******/
------SET ANSI_NULLS OFF
------GO

------SET QUOTED_IDENTIFIER ON
------GO

------CREATE PROCEDURE [dbo].[st_BillReviewExtend]
------(
------	@CREATOR_ID int, 
------	@REVIEW_ID int,
------	@NUMBER_MONTHS int,
------	@BILL_ID int
------)
------AS

------	SET NOCOUNT ON
--------declare @COST int
--------declare @ACCOUNT_COST int
------declare @FOR_SALE_ID int
------declare @CHK int
------	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
------	--if there is no draft corresponding to the current user, or if there is more than one, then return
------	if @CHK != 1 return
	
	
------	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
------	where [TYPE_NAME] = 'Shareable Review'
------	order by LAST_CHANGED desc
	
------	SELECT @CHK = COUNT(line_ID) from TB_BILL_LINE
------		where BILL_ID = @BILL_ID and AFFECTED_ID = @REVIEW_ID and FOR_SALE_ID = @FOR_SALE_ID
------	if @CHK = 1 
------	begin 
------		if @NUMBER_MONTHS = 0 delete from TB_BILL_LINE where BILL_ID = @BILL_ID and AFFECTED_ID = @REVIEW_ID and FOR_SALE_ID = @FOR_SALE_ID
------		else update TB_BILL_LINE set MONTHS = @NUMBER_MONTHS 
------			where BILL_ID = @BILL_ID and AFFECTED_ID = @REVIEW_ID and FOR_SALE_ID = @FOR_SALE_ID
------	end
------	else
------	begin
------		if @NUMBER_MONTHS = 0 delete from TB_BILL_LINE 
------			where BILL_ID = @BILL_ID and AFFECTED_ID = @REVIEW_ID and FOR_SALE_ID = @FOR_SALE_ID
------		else insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
------			values (@BILL_ID, @FOR_SALE_ID, @REVIEW_ID, @NUMBER_MONTHS)
------	end

------SET NOCOUNT OFF

------GO

------USE [ReviewerAdmin]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_CheckGhostAccountBeforeActivation]    Script Date: 06/28/2011 10:22:05 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CheckGhostAccountBeforeActivation]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_CheckGhostAccountBeforeActivation]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_CheckGhostAccountBeforeActivation]    Script Date: 06/28/2011 10:22:05 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO


-------- =============================================
-------- Author:		<Author,,Name>
-------- ALTER date: <ALTER Date,,>
-------- Description:	<Description,,>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_CheckGhostAccountBeforeActivation] 
------(
------	@CONTACT_ID int,
------	@USERNAME nvarchar(50),
------	@EMAIL nvarchar(500),
------	@RESULT nvarchar(100) out
------)
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    -- Insert statements for procedure here
------    Declare @Chk int = 0
------     set @Chk = (select COUNT(Contact_id) from Reviewer.dbo.TB_CONTACT where USERNAME = @USERNAME and CONTACT_ID != @CONTACT_ID)
------     if @Chk > 0
------     begin
------		set @RESULT = 'Username is already in use, please choose a different username'
------		RETURN
------     end
------     else
------     begin
------		set @Chk = (select COUNT(Contact_id) from Reviewer.dbo.TB_CONTACT where EMAIL = @EMAIL and CONTACT_ID != @CONTACT_ID)
------		if @Chk > 0 set @RESULT = 'E-Mail is already in use, please choose a different E-Mail'
------		else set @RESULT = 'Valid'
------     end
------	RETURN

------END


------GO



------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactAccountExtensionClear]    Script Date: 06/21/2011 16:32:43 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactAccountExtensionClear]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactAccountExtensionClear]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactAccountExtensionGet]    Script Date: 06/21/2011 16:33:04 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactAccountExtensionGet]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactAccountExtensionGet]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactAddCreatorID]    Script Date: 06/21/2011 16:34:38 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactAddCreatorID]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactAddCreatorID]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactDetailsUsername]    Script Date: 06/21/2011 16:37:26 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactDetailsUsername]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactDetailsUsername]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactExtendAccount]    Script Date: 06/21/2011 16:39:24 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactExtendAccount]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactExtendAccount]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactExtendReview]    Script Date: 06/21/2011 16:40:00 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactExtendReview]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactExtendReview]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactIDGet]    Script Date: 06/21/2011 16:40:26 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactIDGet]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactIDGet]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedAccounts]    Script Date: 06/21/2011 16:40:45 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactPurchasedAccounts]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactPurchasedAccounts]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedAccounts]    Script Date: 06/21/2011 16:40:45 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		<Author,,Name>
-------- ALTER date: <ALTER Date,,>
-------- Description:	<Description,,>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_ContactPurchasedAccounts] 
------(
------	@CREATOR_ID int
------)
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    -- Insert statements for procedure here
------	select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, 
------		c.DATE_CREATED, c.[EXPIRY_DATE], c.MONTHS_CREDIT, c.CREATOR_ID
------		from Reviewer.dbo.TB_CONTACT c
------		where c.CREATOR_ID = @CREATOR_ID
------		or c.CONTACT_ID = @CREATOR_ID
------		group by c.CONTACT_ID, c.CONTACT_NAME,
------		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID
------	UNION
------	select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, 
------		c.DATE_CREATED, c.[EXPIRY_DATE], c.MONTHS_CREDIT, c.CREATOR_ID
------		from Reviewer.dbo.TB_CONTACT c
------		inner join TB_BILL b on b.PURCHASER_CONTACT_ID = @CREATOR_ID
------		inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID = c.CONTACT_ID
------		inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Professional'
		
------		group by c.CONTACT_ID, c.CONTACT_NAME,
------		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID
	
------	order by c.CONTACT_NAME

------	RETURN

------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedReviews]    Script Date: 06/21/2011 16:42:10 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactPurchasedReviews]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactPurchasedReviews]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedReviews]    Script Date: 06/21/2011 16:42:10 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		<Author,,Name>
-------- ALTER date: <ALTER Date,,>
-------- Description:	<Description,,>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_ContactPurchasedReviews] 
------(
------	@FUNDER_ID int
------)
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    -- Insert statements for procedure here
------select r.REVIEW_ID, r.REVIEW_NAME, r.FUNDER_ID, 
------     r.DATE_CREATED, r.[EXPIRY_DATE], r.MONTHS_CREDIT
      
------	from Reviewer.dbo.TB_REVIEW r
------	where r.FUNDER_ID = @FUNDER_ID
------	--and r.EXPIRY_DATE is not null
	
------	group by r.REVIEW_ID, r.REVIEW_NAME,
------	r.DATE_CREATED, r.[EXPIRY_DATE], r.MONTHS_CREDIT, r.FUNDER_ID
	
------	order by r.REVIEW_NAME

------	RETURN

------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactReviewExtensionClear]    Script Date: 06/21/2011 16:42:38 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactReviewExtensionClear]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactReviewExtensionClear]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactReviewExtensionGet]    Script Date: 06/21/2011 16:43:49 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactReviewExtensionGet]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactReviewExtensionGet]
------GO


------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactReviewsShareable]    Script Date: 06/21/2011 16:48:06 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactReviewsShareable]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactReviewsShareable]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactReviewsShareable]    Script Date: 06/21/2011 16:48:06 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		<Jeff>
-------- ALTER date: <24/03/2010>
-------- Description:	<gets the review data based on contact>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_ContactReviewsShareable] 
------(
------	@CONTACT_ID nvarchar(50)
------)
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;
	
------    -- Insert statements for procedure here
------    declare @PURCHASED_REVIEWS table ( 
------	REVIEW_ID int, 
------	REVIEW_NAME nchar(1000),
------	DATE_CREATED datetime,
------	[EXPIRY_DATE] date,
------	MONTHS_CREDIT smallint,
------	FUNDER_ID int, 
------	LAST_LOGIN datetime )

------/*
------Insert Into @PURCHASED_REVIEWS (REVIEW_ID, REVIEW_NAME, DATE_CREATED, [EXPIRY_DATE],
------				MONTHS_CREDIT, FUNDER_ID)
------SELECT [REVIEW_ID], [REVIEW_NAME], [DATE_CREATED], [EXPIRY_DATE], [MONTHS_CREDIT], [FUNDER_ID]
------  FROM [Reviewer].[dbo].[TB_REVIEW] where FUNDER_ID = @CONTACT_ID
------  and ((EXPIRY_DATE is null and MONTHS_CREDIT != 0)
------		or
------		(EXPIRY_DATE is not null))
------*/

------Insert Into @PURCHASED_REVIEWS (REVIEW_ID, REVIEW_NAME, DATE_CREATED, [EXPIRY_DATE],
------				MONTHS_CREDIT, FUNDER_ID)
------SELECT distinct r.[REVIEW_ID], r.[REVIEW_NAME], r.[DATE_CREATED], r.[EXPIRY_DATE], 
------r.[MONTHS_CREDIT], r.[FUNDER_ID]
------  FROM [Reviewer].[dbo].[TB_REVIEW] r
------left outer join [Reviewer].[dbo].[TB_REVIEW_CONTACT] r_c
------on r.REVIEW_ID = r_c.REVIEW_ID
------left outer join [Reviewer].[dbo].[TB_CONTACT_REVIEW_ROLE] c_r_r
------on r_c.REVIEW_CONTACT_ID = c_r_r.REVIEW_CONTACT_ID
------and ((r_c.CONTACT_ID = @CONTACT_ID and c_r_r.ROLE_NAME = 'AdminUser') 
------		or (r.FUNDER_ID = @CONTACT_ID))
------where ((r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0 )
------		or (r.EXPIRY_DATE is not null)) and r.FUNDER_ID = @CONTACT_ID

------update  p_r
------set p_r.LAST_LOGIN = 
------l_t.CREATED
--------max(l_t.CREATED)
------from @PURCHASED_REVIEWS p_r, TB_LOGON_TICKET l_t
------where l_t.CREATED in
------(select max(l_t.CREATED)from TB_LOGON_TICKET l_t
------where p_r.FUNDER_ID = l_t.CONTACT_ID
------and p_r.REVIEW_ID = l_t.REVIEW_ID)

------select * from @PURCHASED_REVIEWS
    
------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactTmpClear]    Script Date: 06/21/2011 16:50:28 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactTmpClear]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactTmpClear]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactTmpCreate]    Script Date: 06/21/2011 16:50:50 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactTmpCreate]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactTmpCreate]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactTmpDetails]    Script Date: 06/21/2011 16:51:26 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactTmpDetails]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactTmpDetails]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactTmpEdit]    Script Date: 06/21/2011 16:51:43 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactTmpEdit]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactTmpEdit]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactTmpMakeReal]    Script Date: 06/21/2011 16:51:55 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactTmpMakeReal]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactTmpMakeReal]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_GhostReviewActivate]    Script Date: 06/21/2011 16:57:48 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_GhostReviewActivate]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_GhostReviewActivate]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_GhostReviewActivate]    Script Date: 06/21/2011 16:57:48 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		<Author,,Name>
-------- Create date: <Create Date,,>
-------- Description:	<Description,,>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_GhostReviewActivate]
------	-- Add the parameters for the stored procedure here
------	@revID int,
------	@Name Nvarchar(1000) = ''
	
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    -- Insert statements for procedure here
------	if @Name = ''
------	begin
------		update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(month,MONTHS_CREDIT, GETDATE())
------		,REVIEW_NAME = 'Please Edit (ID=' & @revID & ')' where REVIEW_ID = @revID 
------	end
------	else
------	begin
------		update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(month,MONTHS_CREDIT, GETDATE())
------			,REVIEW_NAME = @Name where REVIEW_ID = @revID 
------	end
	
------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsTmp]    Script Date: 06/21/2011 17:01:19 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ReviewDetailsTmp]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ReviewDetailsTmp]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ReviewTmpClear]    Script Date: 06/21/2011 17:02:22 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ReviewTmpClear]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ReviewTmpClear]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ReviewTmpCreate]    Script Date: 06/21/2011 17:02:34 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ReviewTmpCreate]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ReviewTmpCreate]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ReviewTmpDetails]    Script Date: 06/21/2011 17:02:41 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ReviewTmpDetails]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ReviewTmpDetails]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ReviewTmpEdit]    Script Date: 06/21/2011 17:02:48 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ReviewTmpEdit]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ReviewTmpEdit]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ReviewTmpMakeReal]    Script Date: 06/21/2011 17:02:56 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ReviewTmpMakeReal]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ReviewTmpMakeReal]
------GO

------USE [ReviewerAdmin]
------GO

------IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_TB_REVIEW_TMP_COST]') AND type = 'D')
------BEGIN
------ALTER TABLE [dbo].[TB_REVIEW_TMP] DROP CONSTRAINT [DF_TB_REVIEW_TMP_COST]
------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  Table [dbo].[TB_REVIEW_TMP]    Script Date: 06/21/2011 17:05:13 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_REVIEW_TMP]') AND type in (N'U'))
------DROP TABLE [dbo].[TB_REVIEW_TMP]
------GO

------USE [ReviewerAdmin]
------GO

------IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_TB_REVIEW_EXTENSION_COST]') AND type = 'D')
------BEGIN
------ALTER TABLE [dbo].[TB_REVIEW_EXTENSION] DROP CONSTRAINT [DF_TB_REVIEW_EXTENSION_COST]
------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  Table [dbo].[TB_REVIEW_EXTENSION]    Script Date: 06/21/2011 17:05:55 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_REVIEW_EXTENSION]') AND type in (N'U'))
------DROP TABLE [dbo].[TB_REVIEW_EXTENSION]
------GO


------USE [ReviewerAdmin]
------GO

------IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_TB_CONTACT_TMP_COST]') AND type = 'D')
------BEGIN
------ALTER TABLE [dbo].[TB_CONTACT_TMP] DROP CONSTRAINT [DF_TB_CONTACT_TMP_COST]
------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  Table [dbo].[TB_CONTACT_TMP]    Script Date: 06/21/2011 17:07:17 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_CONTACT_TMP]') AND type in (N'U'))
------DROP TABLE [dbo].[TB_CONTACT_TMP]
------GO

------USE [ReviewerAdmin]
------GO

------IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_TB_ACCOUNT_EXTENSION_COST]') AND type = 'D')
------BEGIN
------ALTER TABLE [dbo].[TB_ACCOUNT_EXTENSION] DROP CONSTRAINT [DF_TB_ACCOUNT_EXTENSION_COST]
------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  Table [dbo].[TB_ACCOUNT_EXTENSION]    Script Date: 06/21/2011 17:07:39 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_ACCOUNT_EXTENSION]') AND type in (N'U'))
------DROP TABLE [dbo].[TB_ACCOUNT_EXTENSION]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedReviews]    Script Date: 06/22/2011 16:56:43 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactPurchasedReviews]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactPurchasedReviews]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedReviews]    Script Date: 06/22/2011 16:56:43 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO


-------- =============================================
-------- Author:		<Author,,Name>
-------- ALTER date: <ALTER Date,,>
-------- Description:	<Description,,>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_ContactPurchasedReviews] 
------(
------	@FUNDER_ID int
------)
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    -- Insert statements for procedure here
------select r.REVIEW_ID, r.REVIEW_NAME, r.FUNDER_ID, 
------     r.DATE_CREATED, r.[EXPIRY_DATE], r.MONTHS_CREDIT
      
------	from Reviewer.dbo.TB_REVIEW r
------	where r.FUNDER_ID = @FUNDER_ID
------	--and r.EXPIRY_DATE is not null
	
------	group by r.REVIEW_ID, r.REVIEW_NAME,
------	r.DATE_CREATED, r.[EXPIRY_DATE], r.MONTHS_CREDIT, r.FUNDER_ID
------	UNION
	
------	select r.REVIEW_ID, r.REVIEW_NAME, r.FUNDER_ID, 
------     r.DATE_CREATED, r.[EXPIRY_DATE], r.MONTHS_CREDIT
      
------	from Reviewer.dbo.TB_REVIEW r
------	inner join Reviewer.dbo.TB_REVIEW_CONTACT rc
------	on rc.REVIEW_ID = r.REVIEW_ID and rc.CONTACT_ID = @FUNDER_ID
	
------	UNION
------	select r.REVIEW_ID, r.REVIEW_NAME, r.FUNDER_ID, 
------     r.DATE_CREATED, r.[EXPIRY_DATE], r.MONTHS_CREDIT
      
------	from Reviewer.dbo.TB_REVIEW r
------	inner join TB_BILL b on b.PURCHASER_CONTACT_ID = @FUNDER_ID
------	inner join TB_BILL_LINE bl on bl.BILL_ID = b.BILL_ID and bl.AFFECTED_ID = r.REVIEW_ID
------	inner join TB_FOR_SALE fs on fs.FOR_SALE_ID = bl.FOR_SALE_ID and fs.TYPE_NAME ='Shareable Review'
	
	
------	--and r.EXPIRY_DATE is not null
	
------	group by r.REVIEW_ID, r.REVIEW_NAME,
------	r.DATE_CREATED, r.[EXPIRY_DATE], r.MONTHS_CREDIT, r.FUNDER_ID
	
------	order by r.REVIEW_NAME

------	RETURN

------END


------GO

------USE [ReviewerAdmin]
------GO
------/****** Object:  StoredProcedure [dbo].[st_VatGet]    Script Date: 06/23/2011 11:44:23 ******/
------SET ANSI_NULLS ON
------GO
------SET QUOTED_IDENTIFIER ON
------GO
-------- =============================================
-------- Author:		<Author,,Name>
-------- ALTER date: <ALTER Date,,>
-------- Description:	<Description,,>
-------- =============================================
------ALTER PROCEDURE [dbo].[st_VatGet] 
------(
------	@COUNTRY_ID int
------)
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    -- Insert statements for procedure here
------    declare @VAT_REQUIRED bit
    
------	select @VAT_REQUIRED = VAT_REQUIRED from TB_COUNTRIES
------	where COUNTRY_ID = @COUNTRY_ID
	
------	if @VAT_REQUIRED = 1
------	BEGIN
------		select top 1 VAT_RATE from TB_TERMS_AND_CONDITIONS
------		ORDER by CONDITIONS_ID desc
------	END
	
------	RETURN

------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactBills]    Script Date: 06/27/2011 15:13:18 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactBills]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ContactBills]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ContactBills]    Script Date: 06/27/2011 15:13:18 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		<Author,,Name>
-------- ALTER date: <ALTER Date,,>
-------- Description:	<Description,,>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_ContactBills] 
------(
------	@CONTACT_ID int
------)
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    -- Insert statements for procedure here
------	select c.CONTACT_NAME, b.BILL_ID, b.DATE_PURCHASED, b.NOMINAL_PRICE, b.DISCOUNT,
------	b.DUE_PRICE, b.CONDITIONS_ID, b.BILL_STATUS, b.DATE_PAYMENT_RECEIVED,
------	b.PURCHASER_CONTACT_ID, b.VAT 
------	from TB_BILL b
------	inner join Reviewer.dbo.TB_CONTACT c
------	on b.PURCHASER_CONTACT_ID = c.CONTACT_ID
------	where b.PURCHASER_CONTACT_ID = @CONTACT_ID

------	RETURN

------END

------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillMarkAsCancelled]    Script Date: 06/27/2011 15:37:22 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_BillMarkAsCancelled]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_BillMarkAsCancelled]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_BillMarkAsCancelled]    Script Date: 06/27/2011 15:37:22 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO


-------- =============================================
-------- Author:		<Author,,Name>
-------- Create date: <Create Date,,>
-------- Description:	<Description,,>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_BillMarkAsCancelled]
------	-- Add the parameters for the stored procedure here
------	@CONTACT_ID int,
------	@BILL_ID int
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    -- Insert statements for procedure here
------	UPDATE TB_BILL set BILL_STATUS = 'Cancelled by User'
------	where TB_BILL.BILL_ID = @BILL_ID 
------		and (BILL_STATUS = 'submitted to WPM' or BILL_STATUS = 'submitted to WPM: timed out') 
------		and PURCHASER_CONTACT_ID = @CONTACT_ID
------END


------GO


