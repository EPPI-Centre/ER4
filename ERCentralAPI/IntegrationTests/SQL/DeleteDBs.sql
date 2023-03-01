IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewer')
BEGIN
ALTER DATABASE [tempTestReviewer] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE
END

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewerAdmin')
BEGIN
ALTER DATABASE [tempTestReviewerAdmin] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE 
END

GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewer')
BEGIN
DROP DATABASE [tempTestReviewer] 
END

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewerAdmin')
BEGIN
DROP DATABASE [tempTestReviewerAdmin]
END
GO