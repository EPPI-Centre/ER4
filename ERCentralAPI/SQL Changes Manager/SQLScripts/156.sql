USE Reviewer
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsDelete]    Script Date: 14/02/2020 09:27:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagRelatedPapersRunsDelete] 
	-- Add the parameters for the stored procedure here
	
	@MAG_RELATED_RUN_ID int = 0
,	@REVIEW_ID int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	delete from tb_MAG_RELATED_PAPERS
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID -- and REVIEW_ID = @REVIEW_ID

	delete from tb_MAG_RELATED_RUN
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID -- AND REVIEW_ID = @REVIEW_ID

END
GO

USE Reviewer
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsUpdate]    Script Date: 14/02/2020 09:28:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagRelatedPapersRunsUpdate] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID int,
	@AUTO_RERUN BIT,
	@USER_DESCRIPTION NVARCHAR(1000)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	update tb_MAG_RELATED_RUN
		set USER_STATUS = 'Checked',
		AUTO_RERUN = @AUTO_RERUN,
		USER_DESCRIPTION = @USER_DESCRIPTION
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
END

GO

USE Reviewer
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsInsert]    Script Date: 14/02/2020 09:29:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagRelatedPapersRunsInsert] 
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
		@ALL_INCLUDED, @DATE_FROM, @AUTO_RERUN, 'Pending', 'Waiting', @MODE, @FILTERED, 0)

	set @MAG_RELATED_RUN_ID = @@IDENTITY

	update tb_MAG_RELATED_RUN
		set PARENT_MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulations]    Script Date: 14/02/2020 09:38:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all simulations for a given review
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagSimulations] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here


	SELECT ms.REVIEW_ID, ms.MAG_SIMULATION_ID, ms.YEAR, ms.CREATED_DATE, ms.WITH_THIS_ATTRIBUTE_ID, ms.FILTERED_BY_ATTRIBUTE_ID,
		ms.SEARCH_METHOD, ms.NETWORK_STATISTIC, ms.STUDY_TYPE_CLASSIFIER, ms.USER_CLASSIFIER_MODEL_ID,
		ms.STATUS, ms.tp, ms.FP, ms.FN, MS.NSEEDS, a.ATTRIBUTE_NAME FilteredByAttribute,
		aa.ATTRIBUTE_NAME WithThisAttribute, cm.MODEL_TITLE from TB_MAG_SIMULATION ms
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = ms.FILTERED_BY_ATTRIBUTE_ID
		left outer join TB_ATTRIBUTE aa on aa.ATTRIBUTE_ID = ms.WITH_THIS_ATTRIBUTE_ID
		left outer join tb_CLASSIFIER_MODEL cm on cm.MODEL_ID = ms.USER_CLASSIFIER_MODEL_ID
		WHERE ms.REVIEW_ID = @REVIEW_ID
		order by ms.MAG_SIMULATION_ID
		

END
go

USE Reviewer
GO
/****** Object:  StoredProcedure [dbo].[st_MagPaper]    Script Date: 14/02/2020 09:44:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns a single paper
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagPaper] 
	-- Add the parameters for the stored procedure here
	@PaperId bigint = 0
,	@REVIEW_ID int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT 
		imm.ITEM_ID, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch
		from Reviewer.dbo.tb_ITEM_MAG_MATCH imm where imm.PaperId = @PaperId
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID
END
GO

USE Reviewer
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationResults]    Script Date: 14/02/2020 10:03:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagSimulationResults] 
	-- Add the parameters for the stored procedure here
	
	@MagSimulationId int
,	@OrderBy nvarchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
if @OrderBy = 'Network'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.NETWORK_STATISTIC_SCORE desc

if @OrderBy = 'FoS'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.FOS_DISTANCE_SCORE

if @OrderBy = 'User'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.USER_CLASSIFIER_MODEL_SCORE desc

if @OrderBy = 'StudyType'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.STUDY_TYPE_CLASSIFIER_SCORE desc

if @OrderBy = 'Ensemble'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.ENSEMBLE_SCORE desc
		
END
go

USE Reviewer
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationDelete]    Script Date: 14/02/2020 10:07:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Delete simulation
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagSimulationDelete] 
	-- Add the parameters for the stored procedure here
	
	@MAG_SIMULATION_ID int = 0
,	@REVIEW_ID int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	delete from TB_MAG_SIMULATION_RESULT
		where MAG_SIMULATION_ID = @MAG_SIMULATION_ID

	delete from TB_MAG_SIMULATION
		where MAG_SIMULATION_ID = @MAG_SIMULATION_ID AND REVIEW_ID = @REVIEW_ID

END
GO

USE Reviewer
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationInsert]    Script Date: 14/02/2020 10:09:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagSimulationInsert] 
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
		NETWORK_STATISTIC, STUDY_TYPE_CLASSIFIER, USER_CLASSIFIER_MODEL_ID, STATUS)
	VALUES(@REVIEW_ID, @YEAR, @CREATED_DATE, @WITH_THIS_ATTRIBUTE_ID, @FILTERED_BY_ATTRIBUTE_ID, @SEARCH_METHOD,
		@NETWORK_STATISTIC, @STUDY_TYPE_CLASSIFIER, @USER_CLASSIFIER_MODEL_ID, @STATUS)

	SET @MAG_SIMULATION_ID = @@IDENTITY
END

GO

USE Reviewer
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationResults]    Script Date: 14/02/2020 10:19:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagSimulationResults] 
	-- Add the parameters for the stored procedure here
	
	@MagSimulationId int
,	@OrderBy nvarchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
if @OrderBy = 'Network'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.NETWORK_STATISTIC_SCORE desc

if @OrderBy = 'FoS'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.FOS_DISTANCE_SCORE

if @OrderBy = 'User'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.USER_CLASSIFIER_MODEL_SCORE desc

if @OrderBy = 'StudyType'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.STUDY_TYPE_CLASSIFIER_SCORE desc

if @OrderBy = 'Ensemble'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.ENSEMBLE_SCORE desc
		
END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMagInfo]    Script Date: 14/02/2020 09:01:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns statistics about matching review items to MAG
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagReviewMagInfo] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int = 0, 
	@NInReviewIncluded int = 0 OUTPUT,
	@NInReviewExcluded int = 0 OUTPUT,
	@NMatchedAccuratelyIncluded int = 0 OUTPUT,
	@NMatchedAccuratelyExcluded int = 0 OUTPUT,
	@NRequiringManualCheckIncluded int = 0 OUTPUT,
	@NRequiringManualCheckExcluded int = 0 OUTPUT,
	@NNotMatchedIncluded int = 0 OUTPUT,
	@NNotMatchedExcluded INT = 0 OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select @NInReviewIncluded = count(*) from TB_ITEM_REVIEW ir
		where ir.IS_DELETED = 'FALSE' and ir.IS_INCLUDED = 'TRUE' and REVIEW_ID = @REVIEW_ID

	select @NInReviewExcluded = count(*) from TB_ITEM_REVIEW ir
		where ir.IS_DELETED = 'false' and ir.IS_INCLUDED = 'false' and REVIEW_ID = @REVIEW_ID

	select @NMatchedAccuratelyIncluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'true'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NMatchedAccuratelyExcluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'false'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NRequiringManualCheckIncluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'true'
		where imm.AutoMatchScore < 0.7 and ((imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null) and (imm.ManualTrueMatch = 'false' or imm.ManualTrueMatch is null))
			and not imm.ITEM_ID in (select imm2.ITEM_ID from tb_ITEM_MAG_MATCH imm2 inner join TB_ITEM_REVIEW ir2 on ir2.REVIEW_ID = imm.REVIEW_ID
				where imm2.AutoMatchScore >=0.7 or imm.ManualTrueMatch = 'true' and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null))
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NRequiringManualCheckExcluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'false'
		where imm.AutoMatchScore < 0.7 and ((imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null) and (imm.ManualTrueMatch = 'false' or imm.ManualTrueMatch is null))
			and not imm.ITEM_ID in (select imm2.ITEM_ID from tb_ITEM_MAG_MATCH imm2 inner join TB_ITEM_REVIEW ir2 on ir2.REVIEW_ID = imm.REVIEW_ID
				where imm2.AutoMatchScore >=0.7 or imm.ManualTrueMatch = 'true' and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null))
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NNotMatchedIncluded = count(distinct ir.ITEM_ID) from TB_ITEM_REVIEW ir
		left outer join tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = ir.ITEM_ID
			where ir.IS_INCLUDED = 'true' and imm.ITEM_ID is null and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'

	select @NNotMatchedExcluded = count(distinct ir.ITEM_ID) from TB_ITEM_REVIEW ir
		left outer join tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = ir.ITEM_ID
			where ir.IS_INCLUDED = 'false' and imm.ITEM_ID is null and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'
	
END
GO
