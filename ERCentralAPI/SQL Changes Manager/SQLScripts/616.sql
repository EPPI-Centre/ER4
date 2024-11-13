USE Reviewer
GO

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
		ON fk.parent_object_id = ct.[object_id] and ct.name = 'TB_ROBOT_API_CALL_LOG')
	print 'dropping ' + @indexName;
	set @command = 'ALTER TABLE TB_ROBOT_API_CALL_LOG DROP CONSTRAINT ' + QUOTENAME(@indexname)
	EXEC sp_executesql @command
	select @chk = count(*) from sys.foreign_keys AS fk
		INNER JOIN sys.tables AS ct
		ON fk.parent_object_id = ct.[object_id] and ct.name = 'TB_ROBOT_API_CALL_LOG'
END
GO

ALTER TABLE [dbo].[TB_ROBOT_API_CALL_LOG]  WITH CHECK 
ADD CONSTRAINT FK_TB_ROBOT_API_CALL_LOG_REVIEW_ID
FOREIGN KEY([REVIEW_ID])
REFERENCES [dbo].[TB_REVIEW] ([REVIEW_ID])
GO


ALTER TABLE [dbo].[TB_ROBOT_API_CALL_LOG]  WITH NOCHECK 
ADD CONSTRAINT FK_TB_ROBOT_API_CALL_LOG_REVIEW_SET_ID 
FOREIGN KEY([REVIEW_SET_ID])
REFERENCES [dbo].[TB_REVIEW_SET] ([REVIEW_SET_ID])
GO
ALTER TABLE dbo.TB_ROBOT_API_CALL_LOG
	NOCHECK CONSTRAINT FK_TB_ROBOT_API_CALL_LOG_REVIEW_SET_ID
GO


ALTER TABLE [dbo].[TB_ROBOT_API_CALL_LOG]  WITH CHECK 
ADD CONSTRAINT FK_TB_ROBOT_API_CALL_LOG_ROBOT_ID
FOREIGN KEY([ROBOT_ID])
REFERENCES [dbo].[TB_ROBOT_ACCOUNT] ([ROBOT_ID])
GO

ALTER TABLE [dbo].[TB_ROBOT_API_CALL_LOG]  WITH CHECK ADD  CONSTRAINT [FK_TB_ROBOT__CONTACT_ID] FOREIGN KEY([CONTACT_ID])
REFERENCES [dbo].[TB_CONTACT] ([CONTACT_ID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[TB_ROBOT_API_CALL_LOG] CHECK CONSTRAINT [FK_TB_ROBOT__CONTACT_ID]
GO

