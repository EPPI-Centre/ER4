--This is necessary to apply SQL changes scripts without worries

USE master;  
GO 
--First, set real "Reviewer" and "ReviewerAdmin" aside!!
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'Reviewer')
 ALTER DATABASE Reviewer SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'Reviewer')
 ALTER DATABASE Reviewer MODIFY NAME = ReviewerSetAside;
GO  
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerSetAside')
    ALTER DATABASE ReviewerSetAside SET MULTI_USER;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerAdmin')
 ALTER DATABASE ReviewerAdmin SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerAdmin')
 ALTER DATABASE ReviewerAdmin MODIFY NAME = ReviewerAdminSetAside;
GO  
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerAdminSetAside')
    ALTER DATABASE ReviewerAdminSetAside SET MULTI_USER;
GO


--Then we (temporarily) give our Temp DBs the identity of the "normal" ones
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewer')
 ALTER DATABASE tempTestReviewer SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewer')
 ALTER DATABASE tempTestReviewer MODIFY NAME = Reviewer;
GO  
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'Reviewer')
    ALTER DATABASE Reviewer SET MULTI_USER;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewerAdmin')
 ALTER DATABASE tempTestReviewerAdmin SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewerAdmin')
 ALTER DATABASE tempTestReviewerAdmin MODIFY NAME = ReviewerAdmin;
GO  
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ReviewerAdmin')
    ALTER DATABASE ReviewerAdmin SET MULTI_USER;
GO