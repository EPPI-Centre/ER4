IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewer')
BEGIN
DROP DATABASE [tempTestReviewer] 
END

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tempTestReviewerAdmin')
BEGIN
DROP DATABASE [tempTestReviewerAdmin]
END