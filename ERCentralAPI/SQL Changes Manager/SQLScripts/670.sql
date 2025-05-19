
Use Reviewer
GO

IF COL_LENGTH('dbo.TB_ROBOT_ACCOUNT', 'PUBLIC_DESCRIPTION') IS NULL
BEGIN
	ALTER TABLE dbo.TB_ROBOT_ACCOUNT ADD
		PUBLIC_DESCRIPTION nvarchar(4000) not NULL
		CONSTRAINT D_TB_ROBOT_ACCOUNT_PUBLIC_DESCRIPTION 
		DEFAULT 'N/A'
END
GO
IF COL_LENGTH('dbo.TB_ROBOT_ACCOUNT', 'RETIREMENT_DATE') IS NULL
BEGIN
	ALTER TABLE dbo.TB_ROBOT_ACCOUNT ADD
		RETIREMENT_DATE DATETIME NULL
END
GO


--This part of the script ensures the one pre-existing ROBOT, 'OpenAI GPT4', has the correct values in TB_ROBOT_ACCOUNT
Use Reviewer
GO

declare @rid int = (select rc.ROBOT_ID from TB_ROBOT_ACCOUNT rc 
				inner join TB_CONTACT c on rc.CONTACT_ID = c.CONTACT_ID and c.CONTACT_NAME = 'OpenAI GPT4')
if @rid > 0
BEGIN
	update TB_ROBOT_ACCOUNT set [ENDPOINT] = 'https://gpt34eppi.openai.azure.com/openai/deployments/gpt-4o/chat/completions?api-version=2025-01-01-preview'											
		, IS_PUBLIC = 1
		, PUBLIC_DESCRIPTION = N'This is the first LLM deployed in EPPI Reviewer 2024. The model used is OpenAI "GPT-4o" with "model version = 2024-08-06".'
		, RETIREMENT_DATE = CAST('2025-08-20 00:00:00.000' AS DATETIME) 
	WHERE ROBOT_ID = @rid
END
GO