USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagLogInsert]    Script Date: 24/04/2020 17:26:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_MAG_LOG
-- =============================================
ALTER   PROCEDURE [dbo].[st_MagLogInsert] 
	
	@CONTACT_ID int,
	@JOB_TYPE nvarchar(50),
	@JOB_STATUS nvarchar(50),
	@JOB_MESSAGE nvarchar(max),
	@MAG_LOG_ID int OUTPUT
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO TB_MAG_LOG(TIME_SUBMITTED, CONTACT_ID, JOB_TYPE, JOB_STATUS, JOB_MESSAGE)
	VALUES(GETDATE(), @CONTACT_ID, @JOB_TYPE, @JOB_STATUS, @JOB_MESSAGE)
	
	SET @MAG_LOG_ID = @@IDENTITY
END
GO

USE [Reviewer]
GO

IF COL_LENGTH('dbo.TB_MAG_LOG', 'TIME_UPDATED') IS NULL
BEGIN

BEGIN TRANSACTION

ALTER TABLE dbo.TB_MAG_LOG ADD
	TIME_UPDATED datetime NULL
ALTER TABLE dbo.TB_MAG_LOG SET (LOCK_ESCALATION = TABLE)
COMMIT
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagLogList]    Script Date: 24/04/2020 17:37:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagLogList] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select ML.CONTACT_ID, ML.JOB_MESSAGE, ML.JOB_STATUS, ML.JOB_TYPE, ML.MAG_LOG_ID, ML.TIME_SUBMITTED,
		C.CONTACT_NAME, ML.TIME_UPDATED
	from TB_MAG_LOG ML
		INNER JOIN TB_CONTACT C ON C.CONTACT_ID = ML.CONTACT_ID
		order by ML.MAG_LOG_ID desc
		
END
go