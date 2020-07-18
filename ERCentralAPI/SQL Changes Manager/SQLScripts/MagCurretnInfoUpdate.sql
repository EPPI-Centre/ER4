USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagUpdateCurrentInfoLatestMag]    Script Date: 18/07/2020 14:42:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<POD>
-- Create date: <28/06/2020>
-- Description:	<UPDTES MAG TABLE WITH THE LATEST AZURE DATA>
-- =============================================
ALTER PROCEDURE [dbo].[st_MagUpdateCurrentInfoLatestMag]
		@MAG_VERSION nvarchar(20),
		@WHEN_LIVE	datetime , 
		@MATCHING_AVAILABLE	bit, 
		@MAG_ONLINE bit, 
		@MAKES_ENDPOINT nvarchar(100),
		@MAKES_DEPLOYMENT_STATUS nvarchar(10)
AS
IF Object_ID('dbo.TB_MAG_CURRENT_INFO', 'U') IS NOT NULL
  BEGIN
  SET NOCOUNT ON;
    UPDATE TB_MAG_CURRENT_INFO 
	  SET MAG_VERSION = @MAG_VERSION,
	  WHEN_LIVE = @WHEN_LIVE,
	  MATCHING_AVAILABLE = @MATCHING_AVAILABLE,
	  MAG_ONLINE = @MAG_ONLINE,
	  MAKES_ENDPOINT = @MAKES_ENDPOINT,
	  MAKES_DEPLOYMENT_STATUS = @MAKES_DEPLOYMENT_STATUS
	WHERE MAKES_DEPLOYMENT_STATUS = 'LIVE'
  END

