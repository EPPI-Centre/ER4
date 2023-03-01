--When we're done applying SQL changes scripts we swap back things

USE master;  
GO 
--First, set temp "Reviewer" and "ReviewerAdmin" back to their original names!!
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'Reviewer') AND NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewer')
 ALTER DATABASE Reviewer SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'Reviewer') AND NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewer')
 ALTER DATABASE Reviewer MODIFY NAME = tempTestReviewer;
GO  
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewer')
    ALTER DATABASE tempTestReviewer SET MULTI_USER;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerAdmin') AND NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewerAdmin')
 ALTER DATABASE ReviewerAdmin SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerAdmin') AND NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewerAdmin')
 ALTER DATABASE ReviewerAdmin MODIFY NAME = tempTestReviewerAdmin;
GO  
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewerAdmin')
    ALTER DATABASE tempTestReviewerAdmin SET MULTI_USER;
GO


--Then we give our Real DBs their original names
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerSetAside') AND NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Reviewer')
 ALTER DATABASE ReviewerSetAside SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerSetAside') AND NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Reviewer')
 ALTER DATABASE ReviewerSetAside MODIFY NAME = Reviewer;
GO  
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'Reviewer')
    ALTER DATABASE Reviewer SET MULTI_USER;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerAdminSetAside') AND NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerAdmin')
 ALTER DATABASE ReviewerAdminSetAside SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerAdminSetAside') AND NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerAdmin')
 ALTER DATABASE ReviewerAdminSetAside MODIFY NAME = ReviewerAdmin;
GO  
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerAdmin')
    ALTER DATABASE ReviewerAdmin SET MULTI_USER;
GO