Use Reviewer
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_ROBOT_ACCOUNT_SETTING')
BEGIN
	DROP TABLE [dbo].[TB_ROBOT_ACCOUNT_SETTING]
END
GO
--New table for linking reviews/SLs to the ability to use robots
CREATE TABLE TB_ROBOT_ACCOUNT_SETTING
(
	ACCOUNT_SETTING_ID INT IDENTITY
	, ROBOT_ID INT FOREIGN KEY REFERENCES TB_ROBOT_ACCOUNT(ROBOT_ID)
	, SETTING_NAME nvarchar(200) not null 
	, SETTING_VALUE nvarchar(4000) not null 
	, CONSTRAINT PK_ROBOT_ACCOUNT_SETTING_ID PRIMARY KEY(ACCOUNT_SETTING_ID)
)
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_RobotCoderForSale]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_RobotCoderForSale]
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_RobotCoderForSale]    Script Date: 30/05/2025 13:45:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[st_RobotCoderForSaleAndSettings] 
(
	@RobotId int
)
AS
BEGIN
	
	declare @toSplit varchar(2000) = (select FOR_SALE_IDs from TB_ROBOT_ACCOUNT where ROBOT_ID = @RobotId)

	Select * from sTB_FOR_SALE fs inner join dbo.fn_Split_int(@toSplit, ',') s on fs.FOR_SALE_ID = s.value
	ORDER by idx

	SELECT * from TB_ROBOT_ACCOUNT_SETTING where ROBOT_ID = @RobotId
END
GO

--Now we add data in TB_ROBOT_ACCOUNT_SETTING, for robots that might be in TB_ROBOT_ACCOUNT already, no need for checks we re-created TB_ROBOT_ACCOUNT_SETTING above

declare @RobotId int
declare @robotName nvarchar(255) = 'OpenAI GPT4'
select @RobotId = ROBOT_ID from TB_ROBOT_ACCOUNT r
inner join TB_CONTACT c on r.CONTACT_ID = c.CONTACT_ID and c.CONTACT_NAME = @robotName
IF @RobotId is not null and @RobotId > 0
BEGIN
	INSERT INTO TB_ROBOT_ACCOUNT_SETTING (ROBOT_ID, SETTING_NAME, SETTING_VALUE) 
	VALUES (@RobotId, 'temperature', '0')
	,(@RobotId, 'frequency_penalty', '0')
	,(@RobotId, 'presence_penalty', '0')
	,(@RobotId, 'top_p', '0.95')
	,(@RobotId, 'response_format.type', 'json_object')
END

set @robotName = 'GPT-4.1-nano'	
set @RobotId = null
select @RobotId = ROBOT_ID from TB_ROBOT_ACCOUNT r
inner join TB_CONTACT c on r.CONTACT_ID = c.CONTACT_ID and c.CONTACT_NAME = @robotName
IF @RobotId is not null and @RobotId > 0
BEGIN
	INSERT INTO TB_ROBOT_ACCOUNT_SETTING (ROBOT_ID, SETTING_NAME, SETTING_VALUE) 
	VALUES (@RobotId, 'temperature', '0')
	,(@RobotId, 'frequency_penalty', '0')
	,(@RobotId, 'presence_penalty', '0')
	,(@RobotId, 'top_p', '0.95')
	,(@RobotId, 'response_format.type', 'json_object')
END

set @robotName = 'GPT-4.1'	
set @RobotId = null
select @RobotId = ROBOT_ID from TB_ROBOT_ACCOUNT r
inner join TB_CONTACT c on r.CONTACT_ID = c.CONTACT_ID and c.CONTACT_NAME = @robotName
IF @RobotId is not null and @RobotId > 0
BEGIN
	INSERT INTO TB_ROBOT_ACCOUNT_SETTING (ROBOT_ID, SETTING_NAME, SETTING_VALUE) 
	VALUES (@RobotId, 'temperature', '0')
	,(@RobotId, 'frequency_penalty', '0')
	,(@RobotId, 'presence_penalty', '0')
	,(@RobotId, 'top_p', '0.95')
	,(@RobotId, 'response_format.type', 'json_object')
END

set @robotName = 'OpenAI o3-mini'	
set @RobotId = null
select @RobotId = ROBOT_ID from TB_ROBOT_ACCOUNT r
inner join TB_CONTACT c on r.CONTACT_ID = c.CONTACT_ID and c.CONTACT_NAME = @robotName
IF @RobotId is not null and @RobotId > 0
BEGIN
	INSERT INTO TB_ROBOT_ACCOUNT_SETTING (ROBOT_ID, SETTING_NAME, SETTING_VALUE) 
	VALUES (@RobotId, 'response_format.type', 'json_object')
	,(@RobotId, 'reasoning_effort', 'low')
END

set @robotName = 'OpenAI o4-mini'	
set @RobotId = null
select @RobotId = ROBOT_ID from TB_ROBOT_ACCOUNT r
inner join TB_CONTACT c on r.CONTACT_ID = c.CONTACT_ID and c.CONTACT_NAME = @robotName
IF @RobotId is not null and @RobotId > 0
BEGIN
	INSERT INTO TB_ROBOT_ACCOUNT_SETTING (ROBOT_ID, SETTING_NAME, SETTING_VALUE) 
	VALUES (@RobotId, 'response_format.type', 'json_object')
	--,(@RobotId, 'reasoning_effort', 'low')
END

set @robotName = 'OpenAI o3'	
set @RobotId = null
select @RobotId = ROBOT_ID from TB_ROBOT_ACCOUNT r
inner join TB_CONTACT c on r.CONTACT_ID = c.CONTACT_ID and c.CONTACT_NAME = @robotName
IF @RobotId is not null and @RobotId > 0
BEGIN
	INSERT INTO TB_ROBOT_ACCOUNT_SETTING (ROBOT_ID, SETTING_NAME, SETTING_VALUE) 
	VALUES (@RobotId, 'response_format.type', 'json_object')
END
GO

--NOW that data is in, we can delete columns from TB_ROBOT_ACCOUNT
IF COL_LENGTH('dbo.TB_ROBOT_ACCOUNT', 'TOP_P') IS NOT NULL
BEGIN
	ALTER TABLE TB_ROBOT_ACCOUNT DROP CONSTRAINT D_TB_ROBOT_ACCOUNT_TOP_P
	ALTER TABLE TB_ROBOT_ACCOUNT DROP COLUMN TOP_P;
END
IF COL_LENGTH('dbo.TB_ROBOT_ACCOUNT', 'TEMPERATURE') IS NOT NULL
BEGIN
	ALTER TABLE TB_ROBOT_ACCOUNT DROP CONSTRAINT D_TB_ROBOT_ACCOUNT_TEMPERATURE
	ALTER TABLE TB_ROBOT_ACCOUNT DROP COLUMN TEMPERATURE;
END
IF COL_LENGTH('dbo.TB_ROBOT_ACCOUNT', 'FREQUENCY_PENALTY') IS NOT NULL
BEGIN
	ALTER TABLE TB_ROBOT_ACCOUNT DROP CONSTRAINT D_TB_ROBOT_ACCOUNT_FREQUENCY_PENALTY
	ALTER TABLE TB_ROBOT_ACCOUNT DROP COLUMN FREQUENCY_PENALTY;
END
IF COL_LENGTH('dbo.TB_ROBOT_ACCOUNT', 'PRESENCE_PENALTY') IS NOT NULL
BEGIN
	ALTER TABLE TB_ROBOT_ACCOUNT DROP CONSTRAINT D_TB_ROBOT_ACCOUNT_PRESENCE_PENALTY
	ALTER TABLE TB_ROBOT_ACCOUNT DROP COLUMN PRESENCE_PENALTY;
END
GO


