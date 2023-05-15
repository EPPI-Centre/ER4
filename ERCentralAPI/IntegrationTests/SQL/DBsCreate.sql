IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewer')
BEGIN
CREATE DATABASE [tempTestReviewer] COLLATE Latin1_General_CI_AS
END
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewerAdmin')
BEGIN
CREATE DATABASE [tempTestReviewerAdmin] COLLATE Latin1_General_CI_AS
END

GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewer')
BEGIN
ALTER DATABASE [tempTestReviewer] MODIFY FILE 
( NAME = N'tempTestReviewer', SIZE = 100MB , MAXSIZE = UNLIMITED, FILEGROWTH = 10MB )
--ALTER DATABASE [tempTestReviewer] MODIFY FILE
--( NAME = N'tempTestReviewer_log', SIZE = 73MB , MAXSIZE = UNLIMITED , FILEGROWTH = 10%)
END

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewerAdmin')
BEGIN
ALTER DATABASE [tempTestReviewerAdmin] MODIFY FILE 
( NAME = N'tempTestReviewerAdmin', SIZE = 10MB , MAXSIZE = UNLIMITED, FILEGROWTH = 1MB )
--ALTER DATABASE [tempTestReviewerAdmin] MODIFY FILE
--( NAME = N'tempTestReviewerAdmin_log', SIZE = 10MB , MAXSIZE = UNLIMITED , FILEGROWTH = 10%)
END

GO