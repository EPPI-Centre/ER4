SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

sp_configure 'clr enabled', 1
GO
reconfigure
GO

ALTER ASSEMBLY UserFunctions
   WITH PERMISSION_SET = UNSAFE;
go

CREATE FUNCTION [dbo].[ToShortSearchText](@S1 [nvarchar](2000)) 
RETURNS [nvarchar](1000) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [UserFunctions].[StoredFunctions].[ToShortSearchText]
GO

CREATE FUNCTION [dbo].[LevenshteinSVF](@S1 [nvarchar](1000), @S2 [nvarchar](1000))
RETURNS [float] WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [UserFunctions].[StoredFunctions].[HaBoLevenshtein]
GO

