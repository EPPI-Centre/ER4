USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_MagCurrentInfoInsert]    Script Date: 30/08/2020 13:22:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		POD AUGUST 2020
-- Create date: 
-- Description:	Insert a current MAG Info object
-- =============================================
CREATE OR ALTER  PROCEDURE [dbo].[st_MagCurrentInfoInsert] 
	-- Add the parameters for the stored procedure here
	@MAG_VERSION nvarchar(20),
	@WHEN_LIVE DateTime,
	@MATCHING_AVAILABLE bit,
	@MAG_ONLINE bit,
	@MAKES_ENDPOINT nvarchar(100),
	@MAKES_DEPLOYMENT_STATUS nvarchar(10),
	@NEW_MagCurrentInfo_ID INT OUTPUT
	
AS
BEGIN

	SET NOCOUNT ON;

	INSERT INTO TB_MAG_CURRENT_INFO (MAG_VERSION, WHEN_LIVE, MATCHING_AVAILABLE, MAG_ONLINE, MAKES_ENDPOINT,
		MAKES_DEPLOYMENT_STATUS)
	VALUES(@MAG_VERSION,
	@WHEN_LIVE ,
	@MATCHING_AVAILABLE ,
	@MAG_ONLINE ,
	@MAKES_ENDPOINT ,
	@MAKES_DEPLOYMENT_STATUS )

	SET @NEW_MagCurrentInfo_ID = @@identity
	
END
GO


