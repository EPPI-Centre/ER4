USE [ReviewerAdmin]
GO
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_ONLINE_FEEDBACK')
BEGIN
/****** Object:  Table [dbo].[TB_ONLINE_HELP]    Script Date: 05/02/2019 10:14:58 ******/
DROP TABLE [dbo].[TB_ONLINE_FEEDBACK]
END
GO

CREATE TABLE [dbo].[TB_ONLINE_FEEDBACK](
	[ONLINE_FEEDBACK_ID] [int] IDENTITY(1,1) NOT NULL,
	[CONTEXT] [nchar](100) NULL,
	[CONTACT_ID] [int] NOT NULL,
	[REVIEW_ID] [int] NULL,
	[IS_ERROR] [bit] NOT NULL,
	[MESSAGE] [nvarchar](4000) NOT NULL,
	[DATE] [datetime] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TB_ONLINE_FEEDBACK] ADD  CONSTRAINT [DF_TB_ONLINE_FEEDBACK_DATE]  DEFAULT (getdate()) FOR [DATE]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_OnlineFeedbackCreate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_OnlineFeedbackCreate]
GO


CREATE procedure [dbo].[st_OnlineFeedbackCreate]
(
	@ONLINE_FEEDBACK_ID int output,
	@CONTEXT nchar(100) null,
	@CONTACT_ID int,
	@REVIEW_ID int,
	@IS_ERROR bit,
	@MESSAGE nvarchar(4000)
)

As

SET NOCOUNT ON
INSERT INTO [dbo].[TB_ONLINE_FEEDBACK]
           ([CONTEXT]
           ,[CONTACT_ID]
		   ,[REVIEW_ID]
           ,[IS_ERROR]
           ,[MESSAGE]
           )
     VALUES
           (@CONTEXT
           ,@CONTACT_ID
		   ,@REVIEW_ID
           ,@IS_ERROR
           ,@MESSAGE)
SET  @ONLINE_FEEDBACK_ID = @@IDENTITY
SET NOCOUNT OFF
