USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_SetTypeList]    Script Date: 04/10/2018 11:39:41 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_fakeSproc]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_fakeSproc]
GO

Use Reviewer
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE st_fakeSproc 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT 'I\''m a fake SPROC'
END
GO
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_SetTypeList]    Script Date: 04/10/2018 11:39:41 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_fakeSproc]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_fakeSproc]
GO

Use ReviewerAdmin
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE st_fakeSproc 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT 'I\''m a fake SPROC'
END
GO