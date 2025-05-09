USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearchList]    Script Date: 30/06/2022 14:35:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSearchList] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	SELECT MAG_SEARCH_ID, REVIEW_ID, C.CONTACT_ID, C.CONTACT_NAME, SEARCH_TEXT, SEARCH_NO, HITS_NO,
		SEARCH_DATE, MAG_FOLDER, MAG_SEARCH_TEXT, SEARCH_IDS_STORED
	FROM TB_MAG_SEARCH
	INNER JOIN TB_CONTACT C ON C.CONTACT_ID = TB_MAG_SEARCH.CONTACT_ID
	where REVIEW_ID = @REVIEW_ID
		order by SEARCH_NO desc
		
END
GO
