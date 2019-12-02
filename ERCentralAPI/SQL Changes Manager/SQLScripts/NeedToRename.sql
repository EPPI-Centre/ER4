USE [AcademicController]
GO

/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersWithDateFilter]    Script Date: 19/10/2019 03:02:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--RunningUpdates
-- DO NOT REMOVE ABOVE LINE! IT'S USED WHEN CHECKING IF THIS STORED PROCEDURE IS RUNNING
-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get the ranked list of papers related to a set of items
--		This is the heart of 'review updates' - and there are a number of options: hence the long, repetitive stored proc
-- =============================================
 CREATE OR ALTER   PROCEDURE [dbo].[st_MagRelatedPapersWithDateFilter] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @PaperIdList nvarchar(max) = ''
	declare @ATTRIBUTE_ID bigint = 0
	declare @ALL_INCLUDED BIT = 'false'
	declare @REVIEW_ID int
	declare @DATE_FROM datetime = null
	declare @STATUS nvarchar(50)
	declare @PARENT_MAG_RELATED_RUN_ID INT
	declare @DATE_FROM_INT int = 0
	declare @MODE nvarchar(50)
	declare @FILTERED nvarchar(50)
	declare @NumInserted int

	declare @SeedIds table
	(
		PaperId bigint INDEX idx CLUSTERED
	)

	select @PaperIdList = PaperIdList
		, @ATTRIBUTE_ID = ATTRIBUTE_ID
		, @ALL_INCLUDED = ALL_INCLUDED
		, @DATE_FROM = DATE_FROM
		, @STATUS = [STATUS]
		, @PARENT_MAG_RELATED_RUN_ID = PARENT_MAG_RELATED_RUN_ID
		, @MODE = MODE
		, @FILTERED = Filtered
		, @REVIEW_ID = REVIEW_ID
	FROM Reviewer.dbo.tb_MAG_RELATED_RUN mrr where mrr.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID

	if @STATUS = 'Complete' -- Create a new row for this 'run'
	begin
		update Reviewer.dbo.tb_MAG_RELATED_RUN -- Set autoupdate to 'false' for the older one, or we'll get duplicate reruns
			set AUTO_RERUN = 'false'
			where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID

		insert into Reviewer.dbo.tb_MAG_RELATED_RUN(REVIEW_ID, USER_DESCRIPTION, PaperIdList, ATTRIBUTE_ID, ALL_INCLUDED,
			DATE_FROM, DATE_RUN, AUTO_RERUN, STATUS, PARENT_MAG_RELATED_RUN_ID, MODE, Filtered, N_PAPERS)
		select REVIEW_ID, USER_DESCRIPTION, PaperIdList, ATTRIBUTE_ID, ALL_INCLUDED,
			DATE_FROM, GETDATE(), 'true', 'Running', @PARENT_MAG_RELATED_RUN_ID, MODE, FILTERED, 0 from Reviewer.dbo.tb_MAG_RELATED_RUN
				where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID

		set @MAG_RELATED_RUN_ID = @@IDENTITY
	end

	if not @DATE_FROM is null
		set @DATE_FROM_INT = year(@DATE_FROM)

	-- Create an in-memory table of all the TB_ITEM -> MAG matches that we're working from
	if (@PaperIdList = '')
	begin
		if not @ATTRIBUTE_ID = 0
			insert into @SeedIds
			select imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
			inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = ia.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			group by imm.PaperId
		else
			insert into @SeedIds
			select imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			group by imm.PaperId
	end
	else
		insert into @SeedIds
			select value from Dbo.fn_Split_int(@PaperIdList, ',') 

	-- Now the main identification work happens using the paper recommendations and references tables.
	-- Relationships can be uni- or bi-directional, so there are 6 possible methods.

	-- using the PaperRecommendations table:
	if @MODE = 'Recommended by'
	begin
		insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		SELECT @REVIEW_ID, pr.RecommendedPaperID, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, Sum(Score) SimilarityScore
		from PaperRecommendations pr
		inner join Papers p on p.PaperID = pr.RecommendedPaperID
		inner join @SeedIds spl on spl.PaperId = pr.PaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.RecommendedPaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.RecommendedPaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.RecommendedPaperID
	end
	else if @MODE = 'That recommend'
	begin
		insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		SELECT @REVIEW_ID, pr.PaperID, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, Sum(Score) SimilarityScore
		from PaperRecommendations pr
		inner join Papers p on p.PaperID = pr.PaperID
		inner join @SeedIds spl on spl.PaperId = pr.RecommendedPaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperID
	end
	else if @MODE = 'Recommendations'
	begin

		with cte (PaperId, SimilarityScore) as
		(
		SELECT pr.PaperID, Sum(Score) SimilarityScore
		from PaperRecommendations pr
		inner join Papers p on p.PaperID = pr.PaperID
		inner join @SeedIds spl on spl.PaperId = pr.RecommendedPaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperID

		union all

		SELECT pr.RecommendedPaperID, Sum(Score) SimilarityScore
		from PaperRecommendations pr
		inner join Papers p on p.PaperID = pr.RecommendedPaperID
		inner join @SeedIds spl on spl.PaperId = pr.PaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.RecommendedPaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.RecommendedPaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.RecommendedPaperID

		) insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		select @REVIEW_ID, PaperId, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, Sum(SimilarityScore)
			from cte group by PaperId

	end

	-- Using the PaperReferences table:
	else if @MODE = 'Bibliography'
	begin
		insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		SELECT @REVIEW_ID, pr.PaperReferenceID, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, 1
		from PaperReferences pr
		inner join Papers p on p.PaperID = pr.PaperReferenceID
		inner join @SeedIds spl on spl.PaperId = pr.PaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperReferenceID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperReferenceID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperReferenceID
	end
	else if @MODE = 'Cited by'
	begin
		insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		SELECT @REVIEW_ID, pr.PaperID, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, 1
		from PaperReferences pr
		inner join Papers p on p.PaperID = pr.PaperID
		inner join @SeedIds spl on spl.PaperId = pr.PaperReferenceID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperID
	end
	else if @MODE = 'BiCitation'
	begin
		with cte (PaperId) as
		(
		SELECT pr.PaperID
		from PaperReferences pr
		inner join Papers p on p.PaperID = pr.PaperID
		inner join @SeedIds spl on spl.PaperId = pr.PaperReferenceID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperID

		union all

		SELECT pr.PaperReferenceID
		from PaperReferences pr
		inner join Papers p on p.PaperID = pr.PaperReferenceID
		inner join @SeedIds spl on spl.PaperId = pr.PaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperReferenceID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperReferenceID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperReferenceID

		) insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		select @REVIEW_ID, PaperId, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, 1
			from cte group by PaperId
	end
	else if @MODE = 'Bi-Citation AND Recommendations'
	begin
		with cte (PaperId, SimilarityScore) as
		(
		SELECT pr.PaperID, Sum(Score) SimilarityScore
		from PaperRecommendations pr
		inner join Papers p on p.PaperID = pr.PaperID
		inner join @SeedIds spl on spl.PaperId = pr.RecommendedPaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperID

		union all

		SELECT pr.RecommendedPaperID, Sum(Score) SimilarityScore
		from PaperRecommendations pr
		inner join Papers p on p.PaperID = pr.RecommendedPaperID
		inner join @SeedIds spl on spl.PaperId = pr.PaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.RecommendedPaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.RecommendedPaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.RecommendedPaperID

		union all

		SELECT pr.PaperID, 1
		from PaperReferences pr
		inner join Papers p on p.PaperID = pr.PaperID
		inner join @SeedIds spl on spl.PaperId = pr.PaperReferenceID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperID

		union all

		SELECT pr.PaperReferenceID, 1
		from PaperReferences pr
		inner join Papers p on p.PaperID = pr.PaperReferenceID
		inner join @SeedIds spl on spl.PaperId = pr.PaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperReferenceID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperReferenceID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperReferenceID

		) insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		select @REVIEW_ID, PaperId, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, Sum(SimilarityScore)
			from cte group by PaperId
	end

	set @NumInserted = @@ROWCOUNT

	if not @FILTERED = 'None'
	begin
		if @FILTERED = 'Sensitive'
		begin
			DELETE mrp
			FROM Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp
			inner join RCTScores r on r.PaperId = mrp.PaperId
			where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID and r.Score < 0.002444887

			set @NumInserted = @NumInserted - @@ROWCOUNT
		end

		if @FILTERED = 'Precise'
		begin
			DELETE mrp
			FROM Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp
			inner join RCTScores r on r.PaperId = mrp.PaperId
			where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID and r.Score < 0.003897103

			set @NumInserted = @NumInserted - @@ROWCOUNT
		end
	end

	

	update Reviewer.dbo.tb_MAG_RELATED_RUN
		set [STATUS] = 'Complete',
		N_PAPERS = @NumInserted,
		DATE_RUN = GETDATE(),
		USER_STATUS = 'Unchecked'
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
END
GO

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ClassifierContactModels]    Script Date: 29/11/2019 16:53:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_ClassifierContactModels]
(
	@CONTACT_ID INT
)

As

SET NOCOUNT ON

	select MODEL_ID, MODEL_TITLE, CM.REVIEW_ID, A1.ATTRIBUTE_NAME ATTRIBUTE_ON, A2.ATTRIBUTE_NAME ATTRIBUTE_NOT_ON,
		CONTACT_NAME, CM.CONTACT_ID, ACCURACY, AUC, [PRECISION], RECALL, R.REVIEW_NAME
		from tb_CLASSIFIER_MODEL CM
	INNER JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = CM.ATTRIBUTE_ID_ON
	INNER JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = CM.ATTRIBUTE_ID_NOT_ON
	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = CM.CONTACT_ID
	INNER JOIN TB_REVIEW_CONTACT RC ON RC.REVIEW_ID = CM.REVIEW_ID
	INNER JOIN TB_REVIEW R ON R.REVIEW_ID = RC.REVIEW_ID
	
	where RC.CONTACT_ID = @CONTACT_ID
	order by CM.REVIEW_ID, MODEL_ID

SET NOCOUNT OFF


GO

USE [AcademicController]
GO

/****** Object:  StoredProcedure [dbo].[st_MagSimulations]    Script Date: 29/11/2019 17:34:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all simulations for a given review
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_MagSimulations] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	SELECT * FROM REVIEWER.DBO.TB_MAG_SIMULATION
		WHERE REVIEW_ID = @REVIEW_ID
		
END
GO

USE [AcademicController]
GO

/****** Object:  StoredProcedure [dbo].[st_MagSimulationInsert]    Script Date: 29/11/2019 17:33:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_MagSimulationInsert] 
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
,	@TP INT = 0
,	@FP INT = 0
,	@TN INT = 0
,	@FN INT = 0
,	@MAG_SIMULATION_ID INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	INSERT INTO REVIEWER.DBO.TB_MAG_SIMULATION
		(YEAR, CREATED_DATE, WITH_THIS_ATTRIBUTE_ID, FILTERED_BY_ATTRIBUTE_ID, SEARCH_METHOD,
		NETWORK_STATISTIC, STUDY_TYPE_CLASSIFIER, USER_CLASSIFIER_MODEL_ID, STATUS, TP, FP, TN, FN)
	VALUES(@YEAR, @CREATED_DATE, @WITH_THIS_ATTRIBUTE_ID, @FILTERED_BY_ATTRIBUTE_ID, @SEARCH_METHOD,
		@NETWORK_STATISTIC, @STUDY_TYPE_CLASSIFIER, @USER_CLASSIFIER_MODEL_ID, @STATUS, @TP, @FP, @TN, @FN)

	SET @MAG_SIMULATION_ID = @@IDENTITY
END
GO

USE [AcademicController]
GO

/****** Object:  StoredProcedure [dbo].[st_MagSimulationDelete]    Script Date: 29/11/2019 17:33:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_MagSimulationDelete] 
	-- Add the parameters for the stored procedure here
	
	@MAG_SIMULATION_ID int = 0
,	@REVIEW_ID int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	delete from Reviewer.dbo.TB_MAG_SIMULATION_RESULT
		where MAG_SIMULATION_ID = @MAG_SIMULATION_ID

	delete from Reviewer.dbo.TB_MAG_SIMULATION
		where MAG_SIMULATION_ID = @MAG_SIMULATION_ID AND REVIEW_ID = @REVIEW_ID

END
GO
USE [AcademicController]
GO

/****** Object:  StoredProcedure [dbo].[st_MagSimulationInsert]    Script Date: 29/11/2019 18:55:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_MagSimulationInsert] 
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
,	@TP INT = 0
,	@FP INT = 0
,	@TN INT = 0
,	@FN INT = 0
,	@MAG_SIMULATION_ID INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	if @USER_CLASSIFIER_MODEL_ID = 0
		set @USER_CLASSIFIER_MODEL_ID = null

	INSERT INTO REVIEWER.DBO.TB_MAG_SIMULATION
		(REVIEW_ID, YEAR, CREATED_DATE, WITH_THIS_ATTRIBUTE_ID, FILTERED_BY_ATTRIBUTE_ID, SEARCH_METHOD,
		NETWORK_STATISTIC, STUDY_TYPE_CLASSIFIER, USER_CLASSIFIER_MODEL_ID, STATUS, TP, FP, TN, FN)
	VALUES(@REVIEW_ID, @YEAR, @CREATED_DATE, @WITH_THIS_ATTRIBUTE_ID, @FILTERED_BY_ATTRIBUTE_ID, @SEARCH_METHOD,
		@NETWORK_STATISTIC, @STUDY_TYPE_CLASSIFIER, @USER_CLASSIFIER_MODEL_ID, @STATUS, @TP, @FP, @TN, @FN)

	SET @MAG_SIMULATION_ID = @@IDENTITY
END
GO





