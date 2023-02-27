IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewer')
BEGIN
CREATE DATABASE [tempTestReviewer] COLLATE Latin1_General_CI_AS
END
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewerAdmin')
BEGIN
CREATE DATABASE [tempTestReviewerAdmin] COLLATE Latin1_General_CI_AS
END