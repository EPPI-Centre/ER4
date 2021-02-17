USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearchFilterToLatest]    Script Date: 17/02/2021 18:28:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Filter PaperIds to the last deployed set from MAG
-- =============================================
ALTER   PROCEDURE [dbo].[st_MagSearchFilterToLatest] 
	-- Add the parameters for the stored procedure here
	@MagVersion nvarchar(20)
,	@IDs nvarchar(max)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select mnp.PaperId
	from TB_MAG_NEW_PAPERS mnp
	inner join dbo.fn_Split_int(@IDs, ',') s on s.value = mnp.PaperId
	where mnp.MagVersion = @MagVersion

END
GO