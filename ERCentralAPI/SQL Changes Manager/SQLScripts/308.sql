USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagCurrentInfoList]    Script Date: 21/01/2021 14:56:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get information - last update and whether MAG is available
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagCurrentInfoList] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT MAG_CURRENT_INFO_ID, MAG_VERSION, WHEN_LIVE, MATCHING_AVAILABLE, MAG_ONLINE, MAKES_ENDPOINT,
		MAKES_DEPLOYMENT_STATUS
	FROM TB_MAG_CURRENT_INFO
	
	ORDER BY MAG_CURRENT_INFO_ID DESC
	
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagCurrentInfoUpdate]    Script Date: 21/01/2021 14:56:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get information - last update and whether MAG is available
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagCurrentInfoUpdate] 
	-- Add the parameters for the stored procedure here
	@WHEN_LIVE DATETIME
,	@MAKES_DEPLOYMENT_STATUS NVARCHAR(10)
,	@MAG_ONLINE BIT
,	@MAG_CURRENT_INFO_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE TB_MAG_CURRENT_INFO
		SET WHEN_LIVE = @WHEN_LIVE,
			MAKES_DEPLOYMENT_STATUS = @MAKES_DEPLOYMENT_STATUS,
			MAG_ONLINE = @MAG_ONLINE

		WHERE MAG_CURRENT_INFO_ID = @MAG_CURRENT_INFO_ID

	IF @MAKES_DEPLOYMENT_STATUS = 'PENDING'
		UPDATE TB_MAG_CURRENT_INFO
			SET MAKES_DEPLOYMENT_STATUS = 'OLD'
		WHERE MAG_CURRENT_INFO_ID <> @MAG_CURRENT_INFO_ID AND MAKES_DEPLOYMENT_STATUS = 'PENDING'

	IF @MAKES_DEPLOYMENT_STATUS = 'LIVE'
		UPDATE TB_MAG_CURRENT_INFO
			SET MAKES_DEPLOYMENT_STATUS = 'OLD'
		WHERE MAG_CURRENT_INFO_ID <> @MAG_CURRENT_INFO_ID AND MAKES_DEPLOYMENT_STATUS = 'LIVE'
	
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagCurrentInfoInsert]    Script Date: 21/01/2021 14:56:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get information - last update and whether MAG is available
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagCurrentInfoInsert] 
	-- Add the parameters for the stored procedure here
	@MAG_VERSION NVARCHAR(20)
,	@WHEN_LIVE DATETIME
,	@MAKES_DEPLOYMENT_STATUS NVARCHAR(10)
,	@MAKES_ENDPOINT NVARCHAR(100)
,	@MAG_ONLINE BIT
,	@MAG_CURRENT_INFO_ID INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO TB_MAG_CURRENT_INFO (MAG_VERSION, WHEN_LIVE, MATCHING_AVAILABLE, MAG_ONLINE, MAKES_ENDPOINT, MAKES_DEPLOYMENT_STATUS)
	VALUES (@MAG_VERSION, @WHEN_LIVE, 'TRUE', @MAG_ONLINE, @MAKES_ENDPOINT, @MAKES_DEPLOYMENT_STATUS)
	
	SET @MAG_CURRENT_INFO_ID = @@ROWCOUNT
END

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagCurrentInfoDelete]    Script Date: 21/01/2021 19:06:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get information - last update and whether MAG is available
-- =============================================
CREATE OR ALTER  PROCEDURE [dbo].[st_MagCurrentInfoDelete] 
	-- Add the parameters for the stored procedure here

	@MAG_CURRENT_INFO_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DELETE FROM TB_MAG_CURRENT_INFO
		WHERE MAG_CURRENT_INFO_ID = @MAG_CURRENT_INFO_ID
	
END
GO