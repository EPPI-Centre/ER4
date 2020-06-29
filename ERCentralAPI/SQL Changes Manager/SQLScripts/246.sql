USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_MagUpdateCurrentInfoLatestMag]    Script Date: 29/06/2020 16:49:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<POD>
-- Create date: <28/06/2020>
-- Description:	<UPDTES MAG TABLE WITH THE LATEST AZURE DATA>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagUpdateCurrentInfoLatestMag]
		@MAG_VERSION nvarchar(50),
		@MAKES_ENDPOINT nvarchar(max)
AS
IF Object_ID('dbo.TB_MAG_CURRENT_INFO', 'U') IS NOT NULL
  BEGIN
  SET NOCOUNT ON;

    UPDATE TB_MAG_CURRENT_INFO 
	  SET MAG_VERSION = @MAG_VERSION,
	  MAKES_ENDPOINT = @MAKES_ENDPOINT
	WHERE MAKES_DEPLOYMENT_STATUS = 'LIVE'
  END

GO


