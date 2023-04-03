use [ReviewerAdmin]
GO
IF NOT EXISTS(SELECT * FROM sys.synonyms where name = 'sTB_REVIEW_ROLE')
 CREATE SYNONYM sTB_REVIEW_ROLE FOR Reviewer.dbo.TB_REVIEW_ROLE;
GO