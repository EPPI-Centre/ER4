USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagLogUpdate]    Script Date: 24/04/2020 17:44:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Update record in tb_MAG_LOG
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagLogUpdate] 
	
	@MAG_LOG_ID int,
	@JOB_STATUS nvarchar(50),
	@JOB_MESSAGE nvarchar(max)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    UPDATE TB_MAG_LOG
		SET JOB_STATUS = @JOB_STATUS, JOB_MESSAGE = @JOB_MESSAGE, TIME_UPDATED = GETDATE()
		WHERE MAG_LOG_ID = @MAG_LOG_ID
END
go
