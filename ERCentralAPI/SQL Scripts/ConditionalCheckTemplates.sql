--Template/examples on how to do checks that will allow to run and re-run the same script multiple times without generating errors...


--when a script will CREATE a new SP, check if it's already there and drop it first:
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_NAME]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SP_NAME]
GO
--CREATE procedure [dbo].[SP_NAME] [...]...
--END of CREATE PROCEDURE Example


--TO USE SYNONYMS, when accessing Tables in another DB:
IF NOT EXISTS(SELECT * FROM sys.synonyms where name = 'sTB_SITE_LIC')
 CREATE SYNONYM sTB_SITE_LIC FOR Reviewer.dbo.TB_SITE_LIC;


--when a script will CREATE a new column, you can check if the column exists and do the creation only if needed:
--we don't drop and re-create the column as this might require to move data to a temp-table which doesn't work well as a template.
--effect is that this example isn't ideal: it does not re-establish the starting condition before applying the change, but does ensure one can re-run the script without errors.
--EXAMPLE:
IF COL_LENGTH('dbo.TB_LOGON_TICKET', 'CLIENT') IS NULL
BEGIN

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

ALTER TABLE dbo.TB_LOGON_TICKET ADD
	CLIENT nvarchar(10) NULL

ALTER TABLE dbo.TB_LOGON_TICKET SET (LOCK_ESCALATION = TABLE)

COMMIT
END
GO
--END of CREATE COLUMN Example

--(1) When script will CREATE a NEW TABLE we can drop the table if it exists. 
--However, this will FAIL if/when, on a re-run DB contains tables that depend on the to-be-dropped table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TABLE_NAME')
BEGIN
DROP TABLE [dbo].[TABLE_NAME]
END
GO
--END of CREATE a NEW TABLE Example (1)


--(2) When script will CREATE a NEW TABLEs we can drop the table if it exists. 
--If/when we are creating a few connected tables in the same script, we can put together multiple drops like below:
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'PARENT_TABLE_NAME')
BEGIN
DROP TABLE [dbo].[CHILD_TABLE_NAME]
DROP TABLE [dbo].[PARENT_TABLE_NAME]
END
GO
--CREATE statements go below...
--END of CREATE a NEW TABLEs Example (2)

--Add a FOREING KEY constrain...
--we can easily check by the name of the FK relationship, so NOTE:
--THIS WON'T WORK IF a different FK name is used elsewhere!
-- a better way would be to join to the actual table, but it becomes cumbersome, so I hope this would be enough.
IF NOT EXISTS(select * from sys.foreign_keys where [name] = 'FK_TB_MY_TABLE_TB_FOREIGNTABLE')
BEGIN
--might need to set some more things such as:
--BEGIN TRANSACTION
--	ALTER TABLE dbo.TB_FOREIGNTABLE (LOCK_ESCALATION = TABLE)
--	COMMIT
ALTER TABLE dbo.TB_MY_TABLE ADD CONSTRAINT
		FK_TB_MY_TABLE_TB_FOREIGNTABLE FOREIGN KEY
		(
		FOREIGN_KEY_ID
		) REFERENCES dbo.TB_FOREIGNTABLE
		(
		FOREIGN_KEY_ID
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 
	
END
GO

--EXPAND One or more column(s) example. This only works when we are adding to the lenght, obviously.
IF COL_LENGTH('dbo.AAA', 'Char1') <= 50
BEGIN 
	select 'I will do it'
	ALTER TABLE AAA ALTER COLUMN Char1 VARCHAR (51) NULL;
	ALTER TABLE AAA ALTER COLUMN Char2 VARCHAR (51) NULL;
END
select COL_LENGTH('dbo.AAA', 'Char1')
--END of EXPAND One or more column(s) example


-- IF YOU NEED TO CREATE A NEW INDEX AND WANT TO CHECK IF IT ALREADY EXISTS:
If IndexProperty(Object_Id('[dbo].[TABLE_NAME]'), 'INDEX_NAME', 'IndexID') Is Null
  begin
    CREATE NONCLUSTERED INDEX [INDEX_NAME] ON [dbo].[TABLE_NAME]
	(
		[FIELD] ASC
	)
	INCLUDE([INCLUDE_FIELD],[INCLUDE_FIELD]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
  end
GO
--ALTERNATIVE(/BETTER?) APPROACH To indexes:
declare @chk int = (SELECT count(*)
		FROM sys.indexes 
		WHERE name='INDEX_NAME' AND object_id = OBJECT_ID('[dbo].[TB_SOMETABLE]'))
If @chk = 1 
BEGIN
	DROP INDEX [INDEX_NAME] ON [dbo].[TB_SOMETABLE]
END


--Check if a field is nullable
--it is possible that the change will be blocked by SQL if there are indexes/constraints in place for the column in question!
declare @chk int = (SELECT count(*)
	FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'TB_SOMETABLE' and COLUMN_NAME = 'SOME_COLUMN' and IS_NULLABLE = 'NO')
IF @chk = 1 
BEGIN
	ALTER TABLE TB_SOMETABLE ALTER COLUMN SOME_COLUMN NVARCHAR(50) NULL
END
GO

--when FK constraints were created without giving them an explicit name, they get a random name, which will change in different environments
--this is a problem for changes scripts, as they can't alter constraints easily without knowing their name
--One solution is to delete all FK constraints for the affected table and then recreate them
--The snippet below uses a while to cycle through all FK constraints in the table and drop them.
--AFTER that, you need to re-create all of them!
declare @chk int
declare @safety int = 0
declare @indexName varchar(255) = ''
declare @command nvarchar(max) = ''
select @chk = count(*) from sys.foreign_keys AS fk
INNER JOIN sys.tables AS ct
  ON fk.parent_object_id = ct.[object_id] and ct.name = 'TB_ROBOT_API_CALL_LOG'
--select @chk 

WHile @chk > 0 and @safety < 15
BEGIN
	set @safety = @safety +1;

	set @indexName = (select top 1 fk.name from sys.foreign_keys AS fk
		INNER JOIN sys.tables AS ct
		ON fk.parent_object_id = ct.[object_id] and ct.name = 'TB_SOMETABLE')
	print 'dropping ' + @indexName;
	set @command = 'ALTER TABLE TB_SOMETABLE DROP CONSTRAINT ' + QUOTENAME(@indexname)
	EXEC sp_executesql @command
	select @chk = count(*) from sys.foreign_keys AS fk
		INNER JOIN sys.tables AS ct
		ON fk.parent_object_id = ct.[object_id] and ct.name = 'TB_SOMETABLE'
END
GO --You NEED to recreate all foreing keys explicitly, now!
