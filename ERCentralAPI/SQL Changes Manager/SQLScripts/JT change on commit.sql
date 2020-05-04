USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagCheckContReviewRunning]    Script Date: 04/05/2020 12:56:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns statistics about matching review items to MAG
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagCheckContReviewRunning] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select top(1) * from TB_MAG_LOG
		where JOB_TYPE = 'ContReview process'
		order by MAG_LOG_ID desc
	
END
go