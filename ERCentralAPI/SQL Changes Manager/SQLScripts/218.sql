USE [ReviewerAdmin]
GO

/****** Object:  Table [dbo].[TB_ACCESS_CONTROL]    Script Date: 28/05/2020 17:05:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = N'TB_ACCESS_CONTROL'))
begin
CREATE TABLE [dbo].[TB_ACCESS_CONTROL](
	[ACCESS_CONTROL_ID] [int] IDENTITY(1,1) NOT NULL,
	[CONTACT_ID] [int] NULL,
	[PAGE_TO_ACCESS] [nvarchar](20) NULL,
 CONSTRAINT [PK_TB_ACCESS_CONTROL] PRIMARY KEY CLUSTERED 
(
	[ACCESS_CONTROL_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
end
GO


------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ContactAccessControl]    Script Date: 28/05/2020 17:07:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactAccessControl] 
(
	@CONTACT_ID int,
	@PAGE_TO_ACCESS nvarchar(20)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select * from TB_ACCESS_CONTROL 
	where CONTACT_ID = @CONTACT_ID
	and PAGE_TO_ACCESS = @PAGE_TO_ACCESS


	RETURN

END



GO

