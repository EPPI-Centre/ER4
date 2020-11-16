
USE [Reviewer]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_WebDBgetOpenAccess]
(
	@WebDBid int	
)

As
select * from TB_WEBDB w where w.WEBDB_ID = @WebDBid AND (w.USERNAME is NULL OR w.USERNAME = '') and PWASHED is null

GO


USE [Reviewer]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_WebDBgetClosedAccess]
(
	@WebDBid int
	,@userName  varchar(50)	
	,@Password nvarchar(2000)
)

As

select *  from TB_WEBDB w where w.WEBDB_ID = @WebDBid AND w.USERNAME = @userName 
	and HASHBYTES('SHA1', @Password + FLAVOUR) = PWASHED 
GO