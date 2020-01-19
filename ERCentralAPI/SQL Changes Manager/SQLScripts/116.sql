--use academicController

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
--CREATE TABLE dbo.MagInfo
--    (
--    MAG_ID int NOT NULL IDENTITY (1, 1),
--	Version nvarchar(50) NULL,
--	CreatedOn datetime NULL,
--	LiveOn datetime NULL,
--	ReplacedOn datetime NULL,
--	DeletedOn datetime NULL,
--	DatabaseName nvarchar(50) NULL,
--	nAffiliations int NULL,
--    nAuthors int NULL,
--    nFieldOfStudyChildren int NULL,
--    nFieldOfStudyRelationship int NULL,
--    nFieldsOfStudy int NULL,
--    nJournals int NULL,
--    nPaperAbstractsInvertedIndex int NULL,
--    nPaperAuthorAffiliations int NULL,
--    nPaperFieldsOfStudy int NULL,
--    nPaperRecommendations int NULL,
--    nPaperReferences int NULL,
--    nPapers int NULL,
--    nPaperUrls int NULL,
--    nRCTScores int NULL
--	)  ON[PRIMARY]
--GO
--ALTER TABLE dbo.MagInfo ADD CONSTRAINT
--    PK_MagInfo PRIMARY KEY CLUSTERED
--    (
--    MAG_ID

--    ) WITH(STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]

--GO
--ALTER TABLE dbo.MagInfo SET(LOCK_ESCALATION = TABLE)
--GO
--COMMIT



