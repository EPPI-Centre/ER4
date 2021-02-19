USE [Reviewer]
GO

IF COL_LENGTH('dbo.TB_WEBDB', 'MAP_URL') IS NULL
BEGIN
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION

ALTER TABLE dbo.TB_WEBDB ADD
	MAP_URL nvarchar(1000) NULL,
	MAP_TITLE nvarchar(150) NULL

ALTER TABLE dbo.TB_WEBDB SET (LOCK_ESCALATION = TABLE)

COMMIT
END
GO

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_WebDBget]    Script Date: 18/02/2021 13:59:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER     procedure [dbo].[st_WebDBget]
(
	@WebDBid int,
	@RevId int	
)

As
BEGIN
select [WEBDB_ID]
		  ,[REVIEW_ID]
		  ,[WITH_ATTRIBUTE_ID]
		  ,[IS_OPEN]
		  ,w.[USERNAME]
		  
		  ,[WEBDB_NAME]
		  ,w.[DESCRIPTION]
		  ,c1.CONTACT_NAME as [CREATED_BY]
		  ,c2.CONTACT_NAME as [EDITED_BY]
		  ,[MAP_TITLE]
		  ,[MAP_URL]
		  , w.HEADER_IMAGE_1
		  , w.HEADER_IMAGE_2
	  FROM [TB_WEBDB] w
	  inner join TB_CONTACT c1 on w.CREATED_BY = c1.CONTACT_ID
	  inner join TB_CONTACT c2 on w.EDITED_BY = c2.CONTACT_ID 
	  where w.WEBDB_ID = @WebDBid AND w.REVIEW_ID = @RevId
END
GO

