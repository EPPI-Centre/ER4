Use REviewer
GO

IF COL_LENGTH('dbo.TB_MAG_SIMULATION', 'SeedIds') IS NULL
BEGIN

BEGIN TRANSACTION
ALTER TABLE dbo.TB_MAG_SIMULATION ADD
	SeedIds nvarchar(MAX) NULL,
	SeekingIds nvarchar(MAX) NULL
ALTER TABLE dbo.TB_MAG_SIMULATION SET (LOCK_ESCALATION = TABLE)
COMMIT
end
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationUpdatePostRun]    Script Date: 28/04/2020 19:48:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	After the simulation has run, this updates the table accordingly
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagSimulationUpdatePostRun] 
	-- Add the parameters for the stored procedure here
	@MAG_SIMULATION_ID int,
	@REVIEW_ID int,
	@SeedIds nvarchar(max),
	@NSeeds int,
	@InferenceIds nvarchar(max),
	@ATTRIBUTE_ID_FILTER bigint = 0,
	@ATTRIBUTE_ID_SEED bigint = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	
	declare @SeekingIdsTable TABLE (
		PaperId bigint
	)

	-- insert all the includes into the SeekingIds table
	if @ATTRIBUTE_ID_FILTER > 0
	insert into @SeekingIdsTable(PaperId)
		select value from dbo.fn_Split_int(@inferenceIds, ',') i
		inner join tb_ITEM_MAG_MATCH imm on imm.PaperId = i.value
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'False'
		inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER
		inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null) 
	else
	insert into @SeekingIdsTable(PaperId)
		select value from dbo.fn_Split_int(@inferenceIds, ',') i
		inner join tb_ITEM_MAG_MATCH imm on imm.PaperId = i.value
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'False'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null) 
	
	-- if we've split on an attribute, we remove everything WITHOUT the attribute
	if @ATTRIBUTE_ID_SEED > 0
	delete from @SeekingIdsTable where PaperId in
		(SELECT PaperId from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'False'
		inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID_SEED
		inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		)

	-- remove any seedids from results which have snuck in through the graph search
    delete from TB_MAG_SIMULATION_RESULT where MAG_SIMULATION_ID = @MAG_SIMULATION_ID and PaperId in
		(select value from dbo.fn_Split_int(@seedids, ','))

	-- clear out any seedids from the seeking ids table
	delete from @SeekingIdsTable where PaperId in
		(select value from dbo.fn_Split_int(@seedids, ','))

	-- mark all the results as 'included' if they are in the list of items we're seeking
	update sim
	SET INCLUDED = 'True'
	from TB_MAG_SIMULATION_RESULT sim
		INNER JOIN @SeekingIdsTable s on s.PaperId = sim.PaperId
		where MAG_SIMULATION_ID = @MAG_SIMULATION_ID
	
	-- finally calculate the stats
	declare @FN int = 0
	declare @FP int = 0
	declare @TP int = 0
	
	select @TP = count(*) from TB_MAG_SIMULATION_RESULT msr
		where msr.MAG_SIMULATION_ID = @MAG_SIMULATION_ID and msr.INCLUDED = 'True'

	select @FN = count(*) from @SeekingIdsTable
	set @FN = @FN - @TP

	select @FP = count(*) from TB_MAG_SIMULATION_RESULT msr
		where msr.MAG_SIMULATION_ID = @MAG_SIMULATION_ID and msr.INCLUDED = 'False'

	update TB_MAG_SIMULATION
		set STATUS = 'Complete',
		FP = @FP,
		FN = @FN,
		TP = @TP,
		SeedIds = @SeedIds,
		SeekingIds = (Select STRING_AGG(PaperId, ',') From @SeekingIdsTable),
		NSEEDS = @NSeeds
		where MAG_SIMULATION_ID = @MAG_SIMULATION_ID	
END

go



USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListMagSimulationTPFN]    Script Date: 28/04/2020 11:56:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


create or ALTER procedure [dbo].[st_ItemListMagSimulationTPFN]
(
      @REVIEW_ID INT,
      --@SHOW_INCLUDED BIT = 'true',
	  @MAG_SIMULATION_ID INT,
	  @FOUND BIT,
      
      @PageNum INT = 1,
      @PerPage INT = 3,
      @CurrentPage INT OUTPUT,
      @TotalPages INT OUTPUT,
      @TotalRows INT OUTPUT 
)
with recompile
As

SET NOCOUNT ON

declare @RowsToRetrieve int
Declare @ID table (ItemID bigint primary key )
declare @SeedIds nvarchar(max)

	select @SeedIds = SeedIds from TB_MAG_SIMULATION
		where MAG_SIMULATION_ID = @MAG_SIMULATION_ID

       --store IDs to build paged results as a simple join

	  IF @FOUND = 'True'
	  begin
	  INSERT INTO @ID
	  SELECT DISTINCT (I.ITEM_ID) FROM TB_ITEM_REVIEW I
			INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID AND
				(imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)

			INNER JOIN TB_MAG_SIMULATION_RESULT MSR ON MSR.PaperId = IMM.PaperId
			WHERE I.REVIEW_ID = @REVIEW_ID AND I.IS_DELETED = 'FALSE' and MSR.INCLUDED = 'True'
				and msr.MAG_SIMULATION_ID = @MAG_SIMULATION_ID
	  END
	  else
	  begin

		declare @seekingids nvarchar(max)
		select @seekingids = SeekingIds from TB_MAG_SIMULATION
			where MAG_SIMULATION_ID = @MAG_SIMULATION_ID

		SELECT DISTINCT (I.ITEM_ID) FROM TB_ITEM_REVIEW I
			INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID AND
				(imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			inner join dbo.fn_Split_int(@seekingids, ',') s on s.value = imm.PaperId

			left outer join TB_MAG_SIMULATION_RESULT MSR ON MSR.PaperId = IMM.PaperId AND msr.INCLUDED = 'True'
				AND MSR.INCLUDED = 'True' and msr.MAG_SIMULATION_ID = @MAG_SIMULATION_ID
			WHERE I.REVIEW_ID = @REVIEW_ID AND I.IS_DELETED = 'FALSE' and msr.PaperId is null

	  end

	  SELECT @TotalRows = @@ROWCOUNT

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (ir.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM i
			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID
			INNER JOIN @ID id on id.ItemID = ir.ITEM_ID
            
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve
            ORDER BY SHORT_TITLE


SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'
GO

IF not COL_LENGTH('dbo.TB_MAG_SIMULATION_RESULT', 'FOUND') IS NULL
BEGIN
BEGIN TRANSACTION
ALTER TABLE dbo.TB_MAG_SIMULATION_RESULT
	DROP COLUMN FOUND, SEED
ALTER TABLE dbo.TB_MAG_SIMULATION_RESULT SET (LOCK_ESCALATION = TABLE)
COMMIT
END
GO


IF COL_LENGTH('dbo.TB_MAG_SIMULATION', 'YEAR_END') IS NULL
BEGIN
BEGIN TRANSACTION
ALTER TABLE dbo.TB_MAG_SIMULATION ADD
	YEAR_END int NULL,
	CREATED_DATE_END datetime NULL
ALTER TABLE dbo.TB_MAG_SIMULATION SET (LOCK_ESCALATION = TABLE)
COMMIT
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationInsert]    Script Date: 29/04/2020 23:41:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
create or ALTER       PROCEDURE [dbo].[st_MagSimulationInsert] 
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
		NETWORK_STATISTIC, STUDY_TYPE_CLASSIFIER, USER_CLASSIFIER_MODEL_ID, STATUS, YEAR_END, CREATED_DATE_END)
	VALUES(@REVIEW_ID, @YEAR, @CREATED_DATE, @WITH_THIS_ATTRIBUTE_ID, @FILTERED_BY_ATTRIBUTE_ID, @SEARCH_METHOD,
		@NETWORK_STATISTIC, @STUDY_TYPE_CLASSIFIER, @USER_CLASSIFIER_MODEL_ID, @STATUS, @YEAR_END, @CREATED_DATE_END)

	SET @MAG_SIMULATION_ID = @@IDENTITY
END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationResults]    Script Date: 29/04/2020 23:43:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagSimulationResults] 
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
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.NETWORK_STATISTIC_SCORE desc

if @OrderBy = 'FoS'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.FOS_DISTANCE_SCORE

if @OrderBy = 'User'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.USER_CLASSIFIER_MODEL_SCORE desc

if @OrderBy = 'StudyType'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.STUDY_TYPE_CLASSIFIER_SCORE desc

if @OrderBy = 'Ensemble'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.ENSEMBLE_SCORE desc
		
END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulations]    Script Date: 29/04/2020 23:46:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all simulations for a given review
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagSimulations] 
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
		ms.STATUS, ms.tp, ms.FP, ms.FN, MS.NSEEDS, ms.YEAR_END, ms.CREATED_DATE_END,		
		a.ATTRIBUTE_NAME FilteredByAttribute,
		aa.ATTRIBUTE_NAME WithThisAttribute, cm.MODEL_TITLE
		
		from TB_MAG_SIMULATION ms
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = ms.FILTERED_BY_ATTRIBUTE_ID
		left outer join TB_ATTRIBUTE aa on aa.ATTRIBUTE_ID = ms.WITH_THIS_ATTRIBUTE_ID
		left outer join tb_CLASSIFIER_MODEL cm on cm.MODEL_ID = ms.USER_CLASSIFIER_MODEL_ID
		WHERE ms.REVIEW_ID = @REVIEW_ID
		order by ms.MAG_SIMULATION_ID
		

END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchItemsGetIdList]    Script Date: 30/04/2020 15:47:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Match EPPI-Reviewer TB_ITEM records to MAG Papers
-- =============================================
create or ALTER  PROCEDURE [dbo].[st_MagMatchItemsGetIdList] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ATTRIBUTE_ID BIGINT = 0
AS
BEGIN

if @ATTRIBUTE_ID > 0
begin
	select distinct tir.ITEM_ID
		from Reviewer.dbo.TB_ITEM_REVIEW tir 
		inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = tir.ITEM_ID and tia.ATTRIBUTE_ID = @ATTRIBUTE_ID
		inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
		where tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
			and not tir.ITEM_ID IN (SELECT ITEM_ID FROM tb_ITEM_MAG_MATCH IMM
				WHERE IMM.ITEM_ID = TIR.ITEM_ID AND IMM.REVIEW_ID = @REVIEW_ID)
end
else
begin
	select distinct tir.ITEM_ID
		from Reviewer.dbo.TB_ITEM_REVIEW tir where tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
		and not tir.ITEM_ID IN (SELECT ITEM_ID FROM tb_ITEM_MAG_MATCH IMM
				WHERE IMM.ITEM_ID = TIR.ITEM_ID AND IMM.REVIEW_ID = @REVIEW_ID)
end

END
GO