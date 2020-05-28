USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationUpdateStatus]    Script Date: 28/05/2020 18:28:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Update the status on a simulation (e.g. failure)
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagSimulationUpdateStatus] 
	-- Add the parameters for the stored procedure here
	@MAG_SIMULATION_ID int,
	@STATUS nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;



	update TB_MAG_SIMULATION
		set STATUS = @STATUS
		where MAG_SIMULATION_ID = @MAG_SIMULATION_ID	
END
GO