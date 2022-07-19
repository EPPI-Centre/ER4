USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsUpdatePostRun]    Script Date: 02/07/2022 07:58:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	After the related item ids are found, this updates the record for the UI list
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagRelatedPapersRunsUpdatePostRun] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID int,
	@N_PAPERS int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	update tb_MAG_RELATED_RUN
		set [STATUS] = 'Complete',
		N_PAPERS = @N_PAPERS,
		DATE_RUN = GETDATE(),
		USER_STATUS = 'Not imported'
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
END
GO

