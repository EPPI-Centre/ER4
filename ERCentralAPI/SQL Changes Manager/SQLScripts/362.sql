USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagCurrentInfoInsert]    Script Date: 02/03/2021 09:50:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get information - last update and whether MAG is available
-- =============================================
ALTER   PROCEDURE [dbo].[st_MagCurrentInfoInsert] 
	-- Add the parameters for the stored procedure here
	@MAG_FOLDER NVARCHAR(20)
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

	INSERT INTO TB_MAG_CURRENT_INFO (MAG_FOLDER, WHEN_LIVE, MATCHING_AVAILABLE, MAG_ONLINE, MAKES_ENDPOINT, MAKES_DEPLOYMENT_STATUS)
	VALUES (@MAG_FOLDER, @WHEN_LIVE, 'TRUE', @MAG_ONLINE, @MAKES_ENDPOINT, @MAKES_DEPLOYMENT_STATUS)
	
	SET @MAG_CURRENT_INFO_ID = @@ROWCOUNT
END
GO