USE [ReviewerAdmin]
GO
/****** Object:  Table [dbo].[TB_WEBDB_LOG]    Script Date: 16/01/2020 15:45:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_WEBDB_LOG')
begin
CREATE TABLE dbo.TB_WEBDB_LOG
	(
	WEBDB_LOG_IDENTITY int NOT NULL IDENTITY (1, 1),
	WEBDB_ID int NOT NULL,
	REVIEW_ID int NOT NULL,
	CREATED datetime NOT NULL,
	LOG_TYPE nvarchar(25) NULL,
	DETAILS nvarchar(MAX) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.TB_WEBDB_LOG ADD CONSTRAINT
	DF_TB_WEBDB_LOG_CREATED DEFAULT getdate() FOR CREATED

ALTER TABLE dbo.TB_WEBDB_LOG ADD CONSTRAINT
	PK_TB_WEBDB_LOG PRIMARY KEY CLUSTERED 
	(
	WEBDB_LOG_IDENTITY
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


ALTER TABLE dbo.TB_WEBDB_LOG SET (LOCK_ESCALATION = TABLE)

select Has_Perms_By_Name(N'dbo.TB_WEBDB_LOG', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_WEBDB_LOG', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_WEBDB_LOG', 'Object', 'CONTROL') as Contr_Per 
end
GO


-------------------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_WebDBWriteToLog]    Script Date: 18/10/2021 13:41:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER   procedure [dbo].[st_WebDBWriteToLog] (
 
	@WebDbId int
	, @Type nvarchar(25)
	, @Details nvarchar(max)
)

As

SET NOCOUNT ON

	Declare @RevId int
	set @RevId = (select REVIEW_ID from Reviewer.dbo.TB_WEBDB where WEBDB_ID = @WebDbId)


	if @Type = 'GetSetFrequency'
	begin
		set @Details = (select SET_NAME from Reviewer.dbo.TB_SET where SET_ID = @Details)
	end

		if @Type = 'GetFrequency'
	begin
		set @Details = (select ATTRIBUTE_NAME from Reviewer.dbo.TB_ATTRIBUTE where ATTRIBUTE_ID = @Details)
	end

	if @Type = 'ItemDetailsFromList'
	begin
		declare @itemID nvarchar(20) = @Details
		set @Details = 'ID: ' + @itemID + ' - ' + 
		(select SHORT_TITLE from Reviewer.dbo.TB_ITEM where ITEM_ID = @itemID) + ' ' +
		(select TITLE from Reviewer.dbo.TB_ITEM where ITEM_ID = @itemID)
	end
	

	insert into TB_WEBDB_LOG (WEBDB_ID, REVIEW_ID, LOG_TYPE, DETAILS)
	values (@WebDbId, @RevId, @Type, @Details)


	
SET NOCOUNT OFF
GO

