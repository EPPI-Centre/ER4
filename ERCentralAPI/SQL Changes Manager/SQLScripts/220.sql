USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersListIds]    Script Date: 17/05/2020 07:21:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagRelatedPapersListIds] 
	-- Add the parameters for the stored procedure here
	
	@MAG_RELATED_RUN_ID int = 0
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@REVIEW_ID int = 0
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	select @Total = count(*) from tb_MAG_RELATED_PAPERS mrp
		where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
	
	SELECT mrp.PaperId, mrp.SimilarityScore, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_MAG_RELATED_PAPERS mrp
		left outer join tb_ITEM_MAG_MATCH imm on (imm.PaperId = mrp.PaperId and imm.REVIEW_ID = @REVIEW_ID)
			and imm.ManualFalseMatch <> 'True' 
		where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
		order by mrp.SimilarityScore desc
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END
GO

use [Reviewer]
GO

IF COL_LENGTH('dbo.TB_MAG_SIMULATION', 'FOS_THRESHOLD') IS NULL
BEGIN
ALTER TABLE dbo.TB_MAG_SIMULATION ADD
	FOS_THRESHOLD float(53) NULL,
	SCORE_THRESHOLD float(53) NULL

ALTER TABLE dbo.TB_MAG_SIMULATION SET (LOCK_ESCALATION = TABLE)
end
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationInsert]    Script Date: 28/05/2020 15:09:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
create or ALTER           PROCEDURE [dbo].[st_MagSimulationInsert] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int = 0
,	@YEAR INT = 0
,	@CREATED_DATE DATETIME = NULL
,	@WITH_THIS_ATTRIBUTE_ID BIGINT = 0
,	@FILTERED_BY_ATTRIBUTE_ID BIGINT = 0
,	@SEARCH_METHOD NVARCHAR(50) = NULL
,	@NETWORK_STATISTIC NVARCHAR(50) = NULL
,	@STUDY_TYPE_CLASSIFIER NVARCHAR(50) = NULL
,	@USER_CLASSIFIER_MODEL_ID INT = NULL
,	@STATUS NVARCHAR(50) = NULL
,	@YEAR_END INT = NULL
,	@CREATED_DATE_END DATETIME = NULL
,	@USER_CLASSIFIER_REVIEW_ID INT = NULL
,	@FOS_THRESHOLD FLOAT = NULL
,	@SCORE_THRESHOLD FLOAT = NULL

,	@MAG_SIMULATION_ID INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	if @USER_CLASSIFIER_MODEL_ID = 0
		set @USER_CLASSIFIER_MODEL_ID = null

	INSERT INTO TB_MAG_SIMULATION
		(REVIEW_ID, YEAR, CREATED_DATE, WITH_THIS_ATTRIBUTE_ID, FILTERED_BY_ATTRIBUTE_ID, SEARCH_METHOD,
		NETWORK_STATISTIC, STUDY_TYPE_CLASSIFIER, USER_CLASSIFIER_MODEL_ID, STATUS, YEAR_END, CREATED_DATE_END,
		USER_CLASSIFIER_REVIEW_ID, FOS_THRESHOLD, SCORE_THRESHOLD)
	VALUES(@REVIEW_ID, @YEAR, @CREATED_DATE, @WITH_THIS_ATTRIBUTE_ID, @FILTERED_BY_ATTRIBUTE_ID, @SEARCH_METHOD,
		@NETWORK_STATISTIC, @STUDY_TYPE_CLASSIFIER, @USER_CLASSIFIER_MODEL_ID, @STATUS, @YEAR_END, @CREATED_DATE_END,
		@USER_CLASSIFIER_REVIEW_ID, @FOS_THRESHOLD, @SCORE_THRESHOLD)

	SET @MAG_SIMULATION_ID = @@IDENTITY
END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulations]    Script Date: 28/05/2020 15:11:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all simulations for a given review
-- =============================================
create or ALTER       PROCEDURE [dbo].[st_MagSimulations] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here


	SELECT ms.REVIEW_ID, ms.MAG_SIMULATION_ID, ms.YEAR, ms.CREATED_DATE, ms.WITH_THIS_ATTRIBUTE_ID, ms.FILTERED_BY_ATTRIBUTE_ID,
		ms.SEARCH_METHOD, ms.NETWORK_STATISTIC, ms.STUDY_TYPE_CLASSIFIER, ms.USER_CLASSIFIER_MODEL_ID, ms.USER_CLASSIFIER_REVIEW_ID,
		ms.STATUS, ms.tp, ms.FP, ms.FN, MS.NSEEDS, ms.YEAR_END, ms.CREATED_DATE_END, ms.FOS_THRESHOLD, ms.SCORE_THRESHOLD,
		a.ATTRIBUTE_NAME FilteredByAttribute,
		aa.ATTRIBUTE_NAME WithThisAttribute, cm.MODEL_TITLE
		
		from TB_MAG_SIMULATION ms
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = ms.FILTERED_BY_ATTRIBUTE_ID
		left outer join TB_ATTRIBUTE aa on aa.ATTRIBUTE_ID = ms.WITH_THIS_ATTRIBUTE_ID
		left outer join tb_CLASSIFIER_MODEL cm on cm.MODEL_ID = ms.USER_CLASSIFIER_MODEL_ID
		WHERE ms.REVIEW_ID = @REVIEW_ID
		order by ms.MAG_SIMULATION_ID
		

END
GO

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
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsInsert]    Script Date: 30/05/2020 12:52:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagRelatedPapersRunsInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int
,	@USER_DESCRIPTION NVARCHAR(1000)
,	@PaperIdList nvarchar(max)
,	@ATTRIBUTE_ID bigint = 0
,	@ALL_INCLUDED BIT
,	@DATE_FROM DATETIME
,	@AUTO_RERUN BIT
,	@MODE nvarchar(50)
,	@FILTERED NVARCHAR(50)
,	@STATUS nvarchar(50)
,	@MAG_RELATED_RUN_ID int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	insert into tb_MAG_RELATED_RUN(REVIEW_ID, USER_DESCRIPTION, PaperIdList, ATTRIBUTE_ID,
		ALL_INCLUDED, DATE_FROM, AUTO_RERUN, STATUS, USER_STATUS, MODE, Filtered, N_PAPERS)
	values(@REVIEW_ID, @USER_DESCRIPTION, @PaperIdList, @ATTRIBUTE_ID,
		@ALL_INCLUDED, @DATE_FROM, @AUTO_RERUN, @STATUS, 'Waiting', @MODE, @FILTERED, 0)

	set @MAG_RELATED_RUN_ID = @@IDENTITY

END
GO