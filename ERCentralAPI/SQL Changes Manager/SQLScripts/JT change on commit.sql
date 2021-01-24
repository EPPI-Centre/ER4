USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagGetCurrentlyUsedPaperIds]    Script Date: 22/01/2021 09:05:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get the currently used PaperIds for checking deletions between MAG versions
-- =============================================
ALTER PROCEDURE [dbo].[st_MagGetCurrentlyUsedPaperIds] 
	--@REVIEW_ID INT = 0 -- n.b. review id not used at present
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

		SELECT distinct PaperId, ITEM_ID from tb_ITEM_MAG_MATCH
	UNION
		select distinct PaperId, 0 from TB_MAG_AUTO_UPDATE_RUN_PAPER
	UNION
		select distinct PaperId, 0 from TB_MAG_RELATED_PAPERS
		
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagGetCurrentlyUsedPaperIds]    Script Date: 24/01/2021 15:39:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get the currently used PaperIds for checking deletions between MAG versions
-- =============================================
ALTER PROCEDURE [dbo].[st_MagGetCurrentlyUsedPaperIds] 
	--@REVIEW_ID INT = 0 -- n.b. review id not used at present
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

		SELECT distinct PaperId, ITEM_ID from tb_ITEM_MAG_MATCH

	/*

	UNION
		select distinct PaperId, 0 from TB_MAG_AUTO_UPDATE_RUN_PAPER
	UNION
		select distinct PaperId, 0 from TB_MAG_RELATED_PAPERS
	*/

END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagUpdateChangedIds]    Script Date: 22/01/2021 10:31:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
ALTER PROCEDURE [dbo].[st_MagUpdateChangedIds] 
	-- Add the parameters for the stored procedure here
	@MagVersion nvarchar(20) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	update imm
		set PaperId = NewPaperId,
		AutoMatchScore = NewAutoMatchScore,
		ManualTrueMatch = 'false',
		ManualFalseMatch = 'false'
	from tb_ITEM_MAG_MATCH imm
		inner join TB_MAG_CHANGED_PAPER_IDS mcp on mcp.OldPaperId = imm.PaperId
			and mcp.ITEM_ID = imm.ITEM_ID
		where mcp.MagVersion = @MagVersion
		
	/* Only trying to update the items that are 'known' to reviewers
	update mrp
		set PaperId = NewPaperId
	from TB_MAG_RELATED_PAPERS mrp
		inner join TB_MAG_CHANGED_PAPER_IDS mcp on mcp.OldPaperId = mrp.PaperId
		where mcp.MagVersion = @MagVersion

	update maur
		set maur.PaperId = NewPaperId
	from TB_MAG_AUTO_UPDATE_RUN_PAPER maur
		inner join TB_MAG_CHANGED_PAPER_IDS mcp on mcp.OldPaperId = maur.PaperId
		where mcp.MagVersion = @MagVersion
	*/

	-- There's a small chance that the above could put duplicates in the tables
	-- e.g. if MAG had a duplicate that has now been removed and so we now have two rows pointing to the same paper
	-- The following removes the duplicates and updates the N_PAPERS counts if so

	;WITH cte AS (
		SELECT ITEM_ID, REVIEW_ID, PaperId, 
			row_number() OVER(PARTITION BY ITEM_ID, REVIEW_ID, PaperId ORDER BY AutoMatchScore DESC) AS [rn]
		FROM tb_ITEM_MAG_MATCH
		)
		DELETE cte WHERE [rn] > 1

	/*
	;WITH cte AS (
		SELECT PaperId, REVIEW_ID, MAG_AUTO_UPDATE_RUN_ID, 
			row_number() OVER(PARTITION BY REVIEW_ID, PaperId, MAG_AUTO_UPDATE_RUN_ID ORDER BY MAG_AUTO_UPDATE_RUN_PAPER_ID) AS [rn]
		FROM TB_MAG_AUTO_UPDATE_RUN_PAPER
		)
		DELETE cte WHERE [rn] > 1

	IF @@ROWCOUNT > 0
	BEGIN
		UPDATE TB_MAG_AUTO_UPDATE_RUN
		set N_PAPERS = (SELECT COUNT(*) from TB_MAG_AUTO_UPDATE_RUN_PAPER
			where TB_MAG_AUTO_UPDATE_RUN_PAPER.MAG_AUTO_UPDATE_RUN_ID = TB_MAG_AUTO_UPDATE_RUN.MAG_AUTO_UPDATE_RUN_ID)
	END

	;WITH cte AS (
		SELECT PaperId, REVIEW_ID, MAG_RELATED_RUN_ID, 
			row_number() OVER(PARTITION BY REVIEW_ID, PaperId, MAG_RELATED_RUN_ID ORDER BY MAG_RELATED_PAPERS_ID) AS [rn]
		FROM TB_MAG_RELATED_PAPERS
		)
		DELETE cte WHERE [rn] > 1

	IF @@ROWCOUNT > 0
	BEGIN
		UPDATE TB_MAG_RELATED_RUN
		set N_PAPERS = (SELECT COUNT(*) from TB_MAG_RELATED_PAPERS
			where TB_MAG_RELATED_PAPERS.MAG_RELATED_RUN_ID = TB_MAG_RELATED_RUN.MAG_RELATED_RUN_ID)
	END

	-- Finally, there may be papers that could not be auto-matched; we just delete them and update counts

	delete from maurp
	from TB_MAG_AUTO_UPDATE_RUN_PAPER maurp
	inner join TB_MAG_CHANGED_PAPER_IDS cpi on cpi.OldPaperId = maurp.PaperId
	where cpi.NewPaperId = -1 and cpi.MagVersion = @MagVersion

	IF @@ROWCOUNT > 0
	BEGIN
		UPDATE TB_MAG_AUTO_UPDATE_RUN
		set N_PAPERS = (SELECT COUNT(*) from TB_MAG_AUTO_UPDATE_RUN_PAPER
			where TB_MAG_AUTO_UPDATE_RUN_PAPER.MAG_AUTO_UPDATE_RUN_ID = TB_MAG_AUTO_UPDATE_RUN.MAG_AUTO_UPDATE_RUN_ID)
	END

	delete from mrp
	from TB_MAG_RELATED_PAPERS mrp
	inner join TB_MAG_CHANGED_PAPER_IDS cpi on cpi.OldPaperId = mrp.PaperId
	where cpi.NewPaperId = -1 and cpi.MagVersion = @MagVersion

	IF @@ROWCOUNT > 0
	BEGIN
		UPDATE TB_MAG_RELATED_RUN
		set N_PAPERS = (SELECT COUNT(*) from TB_MAG_RELATED_PAPERS
			where TB_MAG_RELATED_PAPERS.MAG_RELATED_RUN_ID = TB_MAG_RELATED_RUN.MAG_RELATED_RUN_ID)
	END
	*/

END
GO