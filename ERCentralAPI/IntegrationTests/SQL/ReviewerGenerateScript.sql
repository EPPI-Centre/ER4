
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [tempTestReviewer].[dbo].[sp_fulltext_database] @action = 'enable'
end