USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedRun_Update]    Script Date: 22/02/2021 10:10:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: 
-- Description:	mark a line in tb_MAG_RELATED_RUN as @STATUS
-- =============================================
ALTER PROCEDURE [dbo].[st_MagRelatedRun_Update] 
	-- Add the parameters for the stored procedure here
	@USER_STATUS nvarchar(50),
	@STATUS nvarchar(50),
	@REVIEW_ID INT,
	@MAG_RELATED_RUN_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	if @STATUS = ''
	update tb_MAG_RELATED_RUN
			set USER_STATUS = @USER_STATUS
			where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID and REVIEW_ID = @REVIEW_ID
	ELSE
	update tb_MAG_RELATED_RUN
			set USER_STATUS = @USER_STATUS,
			[STATUS] = @STATUS
			where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID and REVIEW_ID = @REVIEW_ID
END
GO