--Template/examples on how to do checks that will allow to run and re-run the same script multiple times without generating errors...


--when a script will CREATE a new SP, check if it's already there and drop it first:
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_NAME]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SP_NAME]
GO
--CREATE procedure [dbo].[SP_NAME] [...]...
--END of CREATE PROCEDURE Example



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


--EXPAND One or more column(s) example. This only works when we are adding to the lenght, obviously.
IF COL_LENGTH('dbo.AAA', 'Char1') <= 50
BEGIN 
	select 'I will do it'
	ALTER TABLE AAA ALTER COLUMN Char1 VARCHAR (51) NULL;
	ALTER TABLE AAA ALTER COLUMN Char2 VARCHAR (51) NULL;
END
select COL_LENGTH('dbo.AAA', 'Char1')
--END of EXPAND One or more column(s) example