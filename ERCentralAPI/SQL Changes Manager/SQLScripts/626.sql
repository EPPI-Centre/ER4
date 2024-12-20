USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ArchieIdentityFromUnassignedCodeAndStatus]    Script Date: 07/11/2024 15:52:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[st_ArchieUnlinkAccount]
	-- Add the parameters for the stored procedure here
	@CONTACT_ID int
	,@ARCHIE_ID varchar(32)
AS
BEGIN
	UPDATE TB_CONTACT 
		set ARCHIE_ID = null
		, ARCHIE_ACCESS_TOKEN = null
		, ARCHIE_TOKEN_VALID_UNTIL = null
		, ARCHIE_REFRESH_TOKEN = null
		, LAST_ARCHIE_CODE = null
		, LAST_ARCHIE_STATE = null
		where CONTACT_ID = @CONTACT_ID and ARCHIE_ID = @ARCHIE_ID
END
GO

