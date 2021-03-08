USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagContReviewInsertResults]    Script Date: 07/03/2021 10:24:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Last stage in the ContReview workflow: put the 'found' papers in and update tb_MAG_RELATED_RUN
-- =============================================
ALTER PROCEDURE [dbo].[st_MagContReviewInsertResults] 
	-- Add the parameters for the stored procedure here
	@MAG_VERSION nvarchar(20)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE MAUR
		SET MAUR.N_PAPERS = idcount --,
			--MAUR.MAG_VERSION = @MAG_VERSION
		FROM TB_MAG_AUTO_UPDATE_RUN MAUR
		inner join (SELECT MAG_AUTO_UPDATE_RUN_ID, COUNT(*) idcount FROM TB_MAG_AUTO_UPDATE_RUN_PAPER
			GROUP BY MAG_AUTO_UPDATE_RUN_ID) AS COUNTS ON COUNTS.MAG_AUTO_UPDATE_RUN_ID = MAUR.MAG_AUTO_UPDATE_RUN_ID
		where maur.N_PAPERS = -1

	UPDATE MAURP
		SET MAURP.REVIEW_ID = MAUR.REVIEW_ID
		FROM TB_MAG_AUTO_UPDATE_RUN_PAPER MAURP
		INNER JOIN TB_MAG_AUTO_UPDATE_RUN MAUR on MAUR.MAG_AUTO_UPDATE_RUN_ID = MAURP.MAG_AUTO_UPDATE_RUN_ID
		WHERE MAURP.REVIEW_ID IS NULL

	-- i.e. in the rare situation that we have zero papers returned
	UPDATE TB_MAG_AUTO_UPDATE_RUN
		SET N_PAPERS = 0 --,
		--MAG_VERSION = @MAG_VERSION
		WHERE N_PAPERS = -1

END
GO


USE [Reviewer]
If IndexProperty(Object_Id('[dbo].[TB_MAG_AUTO_UPDATE_RUN_PAPER]'), 'idx_MagAutoUpdateRunId_and_ContReviewScore', 'IndexID') Is Null
  begin
    CREATE NONCLUSTERED INDEX idx_MagAutoUpdateRunId_and_ContReviewScore ON [dbo].TB_MAG_AUTO_UPDATE_RUN_PAPER
	(
		[MAG_AUTO_UPDATE_RUN_ID] ASC,
		[ContReviewScore] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
  end
GO

