-- PATRICK CHANGES TO STORED PROCEDURE TO INSERT INTO PUBMED JOBLOG 20/07/2018
USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_PubMedJobLogInsert]    Script Date: 20/07/2018 14:32:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_PubMedJobLogInsert]
	-- Add the parameters for the stored procedure here
	(
		@jobID int OUTPUT
	,	@IsDeleting bit = NULL
	,	@TotalErrorCount INT
	,	@Summary VARCHAR(MAX)
	,	@Arguments VARCHAR(2000)
	,	@StartTime datetime = NULL
	,	@EndTime datetime = NULL
	,	@HasError bit = NULL
	)
	

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE  @TEST INT = NULL;
    -- Insert statements for procedure here

	Insert into [dbo].[TB_PUBMEDJOBLOG] (IS_DELETING, TOTAL_ERROR_COUNT, SUMMARY, ARGUMENTS, START_TIME, END_TIME,
	HAS_ERRORS)
	VALUES (@IsDeleting, @TotalErrorCount, @Summary, @Arguments, @StartTime, @EndTime,
		@HasError)
	select @jobID =  IDENT_CURRENT( '[dbo].[TB_PUBMEDJOBLOG]' )

	SET NOCOUNT OFF
END

