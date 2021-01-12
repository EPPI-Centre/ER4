USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListMagMatches]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER procedure [dbo].[st_ItemListMagMatches]
(
      @REVIEW_ID INT,
      @SHOW_INCLUDED BIT = 'true',
      
      @PageNum INT = 1,
      @PerPage INT = 3,
      @CurrentPage INT OUTPUT,
      @TotalPages INT OUTPUT,
      @TotalRows INT OUTPUT 
)
with recompile
As
BEGIN
SET NOCOUNT ON

declare @RowsToRetrieve int
Declare @ID table (ItemID bigint primary key )

       --store IDs to build paged results as a simple join
	  INSERT INTO @ID
	  SELECT DISTINCT (I.ITEM_ID) FROM TB_ITEM_REVIEW I
			INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID 
			WHERE  (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			AND	I.REVIEW_ID = @REVIEW_ID AND I.IS_INCLUDED = @SHOW_INCLUDED
			AND I.IS_DELETED = 'FALSE'

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
      SELECT DISTINCT (i.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE, i.ITEM_ID) RowNum
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
            ORDER BY RowNum


SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'
end
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListMagNoMatches]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER procedure [dbo].[st_ItemListMagNoMatches]
(
      @REVIEW_ID INT,
      @SHOW_INCLUDED BIT = 'true',
      
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

       --store IDs to build paged results as a simple join
	  INSERT INTO @ID SELECT DISTINCT (I.ITEM_ID)
      FROM TB_ITEM I
      INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
			left outer JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID
			where imm.ITEM_ID is null
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
      SELECT DISTINCT (i.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE, i.ITEM_ID) RowNum
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
            ORDER BY RowNum


SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'

GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateClassifierScoresUpdate]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdateClassifierScoresUpdate] 
	-- Add the parameters for the stored procedure here
	@MAG_AUTO_UPDATE_RUN_ID int,
	@Field nvarchar(30) = '',
	@StudyTypeClassifier nvarchar(50) = '',
	@UserClassifierModelId int = 0,
	@UserClassifierReviewId int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	if @Field = 'StudyTypeClassifierScore'
	BEGIN
		UPDATE MAUR
		SET StudyTypeClassifierScore = Score
		FROM TB_MAG_AUTO_UPDATE_RUN_PAPER MAUR
		INNER JOIN TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP mac ON mac.MAG_AUTO_UPDATE_RUN_ID = maur.MAG_AUTO_UPDATE_RUN_ID and
			mac.PaperId = MAUR.PaperId

		UPDATE TB_MAG_AUTO_UPDATE_RUN
			SET STUDY_TYPE_CLASSIFIER = @StudyTypeClassifier
			WHERE MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID
	END

	IF @Field = 'UserClassifierScore'
	BEGIN
		UPDATE MAUR
		SET UserClassifierScore = SCORE
		FROM TB_MAG_AUTO_UPDATE_RUN_PAPER MAUR
		INNER JOIN TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP mac ON mac.MAG_AUTO_UPDATE_RUN_ID = MAUR.MAG_AUTO_UPDATE_RUN_ID and
			mac.PaperId = maur.PaperId

		UPDATE TB_MAG_AUTO_UPDATE_RUN
			SET USER_CLASSIFIER_MODEL_ID = @UserClassifierModelId,
				USER_CLASSIFIER_REVIEW_ID = @UserClassifierReviewId
			WHERE MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID
	END

	DELETE FROM TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP
		WHERE MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID

END
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateDelete]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdateDelete] 
	-- Add the parameters for the stored procedure here
	@MAG_AUTO_UPDATE_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	delete from TB_MAG_AUTO_UPDATE
	where MAG_AUTO_UPDATE_ID = @MAG_AUTO_UPDATE_ID

END
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateInsert]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdateInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int
,	@USER_DESCRIPTION NVARCHAR(1000)
,	@ATTRIBUTE_ID bigint = 0
,	@ALL_INCLUDED BIT
,	@MAG_AUTO_UPDATE_ID int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here


	insert into TB_MAG_AUTO_UPDATE(REVIEW_ID, USER_DESCRIPTION, ATTRIBUTE_ID,
		ALL_INCLUDED)
	values(@REVIEW_ID, @USER_DESCRIPTION, @ATTRIBUTE_ID,
		@ALL_INCLUDED)

	set @MAG_AUTO_UPDATE_ID = @@IDENTITY

END
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRunCountResults]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdateRunCountResults] 
	-- Add the parameters for the stored procedure here
	
	@MagAutoUpdateRunId int
,	@AutoUpdateScore float
,	@StudyTypeClassifierScore float
,	@UserClassifierScore float
,	@ResultCount int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select @ResultCount = count(PaperId) from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId and
			ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRunDelete]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdateRunDelete] 
	-- Add the parameters for the stored procedure here
	@MAG_AUTO_UPDATE_RUN_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	delete from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID

	delete from TB_MAG_AUTO_UPDATE_RUN
		WHERE MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID

END
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRunListIds]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdateRunListIds] 
	-- Add the parameters for the stored procedure here
	
	@MagAutoUpdateRunId int = 0
,	@OrderBy nvarchar(20) = 'AutoUpdate'
,	@AutoUpdateScore float = 0
,	@StudyTypeClassifierScore float = 0
,	@UserClassifierScore float = 0

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

	select @Total = count(*) from TB_MAG_AUTO_UPDATE_RUN_PAPER maur
		where maur.MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId
		and ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore

if @OrderBy = 'AutoUpdate'
begin
	SELECT maur.PaperId, maur.ContReviewScore SimilarityScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from TB_MAG_AUTO_UPDATE_RUN_PAPER maur
		left outer join tb_ITEM_MAG_MATCH imm on (imm.PaperId = maur.PaperId and imm.REVIEW_ID = @REVIEW_ID)
			and imm.ManualFalseMatch <> 'True' 
		where maur.MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId
		and ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by maur.ContReviewScore desc
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

end
else if @OrderBy = 'StudyTypeClassifier'
begin

	SELECT maur.PaperId, maur.StudyTypeClassifierScore SimilarityScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from TB_MAG_AUTO_UPDATE_RUN_PAPER maur
		left outer join tb_ITEM_MAG_MATCH imm on (imm.PaperId = maur.PaperId and imm.REVIEW_ID = @REVIEW_ID)
			and imm.ManualFalseMatch <> 'True' 
		where maur.MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId
		and ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by maur.StudyTypeClassifierScore desc
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
end
else if @OrderBy = 'UserClassifier'
begin

	SELECT maur.PaperId, maur.UserClassifierScore SimilarityScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from TB_MAG_AUTO_UPDATE_RUN_PAPER maur
		left outer join tb_ITEM_MAG_MATCH imm on (imm.PaperId = maur.PaperId and imm.REVIEW_ID = @REVIEW_ID)
			and imm.ManualFalseMatch <> 'True' 
		where maur.MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId
		and ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by maur.UserClassifierScore desc
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
end

	SELECT  @Total as N'@Total'
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRunResults]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdateRunResults] 
	-- Add the parameters for the stored procedure here
	
	@MagAutoUpdateRunId int
,	@OrderBy nvarchar(10)
,	@AutoUpdateScore float
,	@StudyTypeClassifierScore float
,	@UserClassifierScore float
,	@TopN int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

if @OrderBy = 'AutoUpdate'
begin
	select top(@TopN) PaperId from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId and
			ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by ContReviewScore desc
end
else if @OrderBy = 'StudyTypeClassifier'
begin
	select top(@TopN) PaperId from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId and
			ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by StudyTypeClassifierScore desc
end
else if @OrderBy = 'UserClassifier'
begin
	select top(@TopN) PaperId from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId and
			ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
		order by UserClassifierScore desc
end


		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRuns]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all runs for a given review
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdateRuns] 
	-- Add the parameters for the stored procedure here	
	@REVIEW_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select mau.MAG_AUTO_UPDATE_RUN_ID, mau.USER_DESCRIPTION, mau.ATTRIBUTE_ID, mau.ALL_INCLUDED,
		a.ATTRIBUTE_NAME, mau.DATE_RUN, mau.N_PAPERS, mau.REVIEW_ID, mau.MAG_AUTO_UPDATE_ID,
		mau.STUDY_TYPE_CLASSIFIER, mau.USER_CLASSIFIER_MODEL_ID, mau.USER_CLASSIFIER_REVIEW_ID,
		cm.MODEL_TITLE
		from TB_MAG_AUTO_UPDATE_RUN mau
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mau.ATTRIBUTE_ID
		left outer join tb_CLASSIFIER_MODEL cm on cm.MODEL_ID = mau.USER_CLASSIFIER_MODEL_ID
		where mau.REVIEW_ID = @REVIEW_ID
		order by MAU.MAG_AUTO_UPDATE_ID
		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdates]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all runs for a given review
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdates] 
	-- Add the parameters for the stored procedure here	
	@REVIEW_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select mau.MAG_AUTO_UPDATE_ID, mau.USER_DESCRIPTION, mau.ATTRIBUTE_ID, mau.ALL_INCLUDED,
		mau.STUDY_TYPE_CLASSIFIER, mau.USER_CLASSIFIER_MODEL_ID, mau.USER_CLASSIFIER_REVIEW_ID,
		a.ATTRIBUTE_NAME, mau.REVIEW_ID, cm.MODEL_TITLE from TB_MAG_AUTO_UPDATE mau
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mau.ATTRIBUTE_ID
		left outer join tb_CLASSIFIER_MODEL cm on cm.MODEL_ID = mau.USER_CLASSIFIER_MODEL_ID
		where mau.REVIEW_ID = @REVIEW_ID
		order by MAU.MAG_AUTO_UPDATE_ID
		
END
GO

/****** Object:  StoredProcedure [dbo].[st_MagCheckContReviewRunning]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns statistics about matching review items to MAG
-- =============================================
ALTER PROCEDURE [dbo].[st_MagCheckContReviewRunning] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select top(1) * from TB_MAG_LOG
		where JOB_TYPE = 'ContReview process'
		order by MAG_LOG_ID desc
	
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagContReviewInsertResults]    Script Date: 30/11/2020 10:47:18 ******/
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
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE MAUR
		SET MAUR.N_PAPERS = idcount
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
		SET N_PAPERS = 0
		WHERE N_PAPERS = -1

END
GO
/****** Object:  StoredProcedure [dbo].[st_MagContReviewRunGetSeedIds]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get seed MAG IDs for the Continuous Review run when a new MAG is available
-- =============================================
ALTER PROCEDURE [dbo].[st_MagContReviewRunGetSeedIds] 

AS
BEGIN

	-- First, create new lines for each update in each review
	INSERT INTO TB_MAG_AUTO_UPDATE_RUN(MAG_AUTO_UPDATE_ID, USER_DESCRIPTION, ATTRIBUTE_ID,
		ALL_INCLUDED, DATE_RUN, N_PAPERS, REVIEW_ID)
	SELECT MAG_AUTO_UPDATE_ID, USER_DESCRIPTION, ATTRIBUTE_ID, ALL_INCLUDED, GETDATE(), -1, REVIEW_ID
		FROM TB_MAG_AUTO_UPDATE

	-- Next, grab the seed ids
	SELECT imm.PaperId, mau.MAG_AUTO_UPDATE_RUN_ID AutoUpdateId, 1 as Included
	from tb_ITEM_MAG_MATCH imm
	inner join TB_ITEM_REVIEW ir on ir.REVIEW_ID = imm.REVIEW_ID and imm.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 'false'
	inner join TB_MAG_AUTO_UPDATE_RUN mau on mau.REVIEW_ID = ir.REVIEW_ID
	where  (mau.ATTRIBUTE_ID = 0 OR mau.ATTRIBUTE_ID is null) and
		(imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)

	UNION ALL

	SELECT imm.PaperId, mau.MAG_AUTO_UPDATE_RUN_ID AutoUpdateId, 1 as Included
	from tb_ITEM_MAG_MATCH imm
	inner join TB_ITEM_REVIEW ir on ir.REVIEW_ID = imm.REVIEW_ID and imm.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 'false'
	inner join TB_MAG_AUTO_UPDATE_RUN mau on mau.REVIEW_ID = ir.REVIEW_ID
	inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = ir.ITEM_ID and ia.ATTRIBUTE_ID = mau.ATTRIBUTE_ID
	inner join TB_ITEM_SET iset on iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
	where not mau.ATTRIBUTE_ID is null and
	(imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagCurrentInfo]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get information - last update and whether MAG is available
-- =============================================
ALTER PROCEDURE [dbo].[st_MagCurrentInfo] 
	-- Add the parameters for the stored procedure here
	@MAKES_DEPLOYMENT_STATUS nvarchar(10)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP(1) MAG_CURRENT_INFO_ID, MAG_VERSION, WHEN_LIVE, MATCHING_AVAILABLE, MAG_ONLINE, MAKES_ENDPOINT,
		MAKES_DEPLOYMENT_STATUS
	FROM TB_MAG_CURRENT_INFO
	where MAKES_DEPLOYMENT_STATUS = @MAKES_DEPLOYMENT_STATUS
	ORDER BY MAG_CURRENT_INFO_ID DESC
	
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagGetCurrentlyUsedPaperIds]    Script Date: 30/11/2020 10:47:18 ******/
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
	@REVIEW_ID INT = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	IF @REVIEW_ID = 0
		SELECT distinct PaperId, ITEM_ID from tb_ITEM_MAG_MATCH
	ELSE
		SELECT distinct imm.PaperId, imm.ITEM_ID from tb_ITEM_MAG_MATCH imm
			inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false'
			WHERE imm.REVIEW_ID = @REVIEW_ID
		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagGetMissingPaperIds]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get the list of changed PaperIds to look up in the new deployment
-- =============================================
ALTER PROCEDURE [dbo].[st_MagGetMissingPaperIds] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	SELECT distinct OldPaperId, MagChangedPaperIdsId, ITEM_ID from TB_MAG_CHANGED_PAPER_IDS
		WHERE NewPaperId = -1
		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagItemMagRelatedPaperInsert]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Pull MAG papers found in a related papers run into tb_ITEM
-- =============================================
ALTER PROCEDURE [dbo].[st_MagItemMagRelatedPaperInsert] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID INT,
	@REVIEW_ID int = 0
--WITH RECOMPILE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select PaperId
	 FROM tb_MAG_RELATED_PAPERS mrp
		where mrp.REVIEW_ID = @REVIEW_ID and mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
	 
	 and not mrp.PaperId in 
		(select paperid from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			where ir.REVIEW_ID = @REVIEW_id and ir.IS_DELETED = 'false' and
				(imm.AutoMatchScore > 0.8 or (imm.ManualFalseMatch = 'true' or imm.ManualTrueMatch = 'true')))

	
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagItemMatchedPapersIds]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns all the papers that are matched to a given tb_ITEM record
-- =============================================
ALTER PROCEDURE [dbo].[st_MagItemMatchedPapersIds] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int = 0, 
	@ITEM_ID BIGINT = 0
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	/*
	Not bothering with paging, as the same item is highly unlikely to be matched with more than one or two MAG records

	SELECT @Total = count(*) from tb_ITEM_MAG_MATCH imm
	inner join Papers p on p.PaperID = imm.PaperId
	inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	where imm.ITEM_ID = @ITEM_ID
	*/

    -- Insert statements for procedure here
	SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
	from tb_ITEM_MAG_MATCH imm
	inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	where imm.ITEM_ID = @ITEM_ID
	ORDER BY ManualTrueMatch desc, imm.AutoMatchScore desc
	/*
	OFFSET (@PageNo-1) * @RowsPerPage ROWS
	FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
	*/
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagItemPaperInsert]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Pull a limited number of MAG papers into tb_ITEM based on a list of IDs. e.g. user 'selected' list
-- =============================================
ALTER PROCEDURE [dbo].[st_MagItemPaperInsert] 
	-- Add the parameters for the stored procedure here
	@SOURCE_NAME nvarchar(255),
	@CONTACT_ID INT = 0,
	@REVIEW_ID INT = 0,
	@GUID_JOB nvarchar(50),
	@MAG_RELATED_RUN_ID INT = 0,
	@N_IMPORTED INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @SourceId int = 0

	insert into Reviewer.dbo.TB_SOURCE(SOURCE_NAME, REVIEW_ID, IS_DELETED, DATE_OF_SEARCH, 
		DATE_OF_IMPORT, SOURCE_DATABASE,  SEARCH_DESCRIPTION, SEARCH_STRING, NOTES, IMPORT_FILTER_ID)
	values(@SOURCE_NAME, @REVIEW_id, 'false', GETDATE(),
		GETDATE(), 'Microsoft Academic', 'Browsing items related to a given item', 'Browse', '', 0)

	set @SourceId = @@IDENTITY

	declare @ItemIds Table(ITEM_ID bigint, PaperId bigint)

	insert into Reviewer.dbo.TB_ITEM(TYPE_ID, TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, YEAR, STANDARD_NUMBER, CITY,
		COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, URL, OLD_ITEM_ID, ABSTRACT, DOI, SearchText)
	output INSERTED.ITEM_ID, cast(inserted.OLD_ITEM_ID as bigint) into @ItemIds
	SELECT 
		/*   we don't currently have publication type in MAG items (once MAKES is live we will)
		case
			when p.DocType = 'Book' then 2
			when p.DocType = 'BookChapter' then 3
			when p.DocType = 'Conference' then 5
			when p.DocType = 'Journal' then 14
			when p.DocType = 'Patent' then 1
			else 14
		end
		*/
		14, mipi.title, mipi.journal,
			(select top(1) LAST from TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP where GUID_JOB = @GUID_JOB and PaperId = mipi.PaperId and RANK = 1 ) + ' (' + cast(mipi.year as nvarchar) + ')',
			GETDATE(), @CONTACT_ID, mipi.Year, 
			'', '', '', '', '', mipi.volume, mipi.pages, '', mipi.issue, '', cast(mipi.PaperId as nvarchar), mipi.abstract, mipi.doi, mipi.SearchText
	 FROM  reviewer.dbo.TB_MAG_ITEM_PAPER_INSERT_TEMP mipi
	 where mipi.GUID_JOB = @GUID_JOB and not mipi.PaperId in 
		(select paperid from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			where ir.REVIEW_ID = @REVIEW_id and ir.IS_DELETED = 'false' and
				(imm.AutoMatchScore > 0.8 or (imm.ManualFalseMatch = 'true' or imm.ManualTrueMatch = 'true')))

	set @N_IMPORTED = @@ROWCOUNT

	if @N_IMPORTED > 0
	begin

		 insert into Reviewer.dbo.TB_ITEM_SOURCE(ITEM_ID, SOURCE_ID)
		 select ITEM_ID, @SourceId from @ItemIds

		 insert into Reviewer.dbo.TB_ITEM_REVIEW(ITEM_ID, REVIEW_ID, IS_DELETED, IS_INCLUDED)
		 select item_id, @REVIEW_id, 'false', 'true' from @ItemIds

		 insert into reviewer.dbo.tb_ITEM_MAG_MATCH(ITEM_ID, REVIEW_ID, PaperId, AutoMatchScore, ManualTrueMatch, ManualFalseMatch)
		 select item_id, @REVIEW_id, PaperId, 1, 'false', 'false' from @ItemIds

		 insert into Reviewer.dbo.TB_ITEM_AUTHOR(ITEM_ID, LAST, FIRST, SECOND, ROLE, RANK)
		 select ITEM_ID, LAST, FIRST, SECOND, 0, RANK FROM
			TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP auTemp INNER JOIN @ItemIds I on i.PaperId = auTemp.PaperId
			where auTemp.GUID_JOB = @GUID_JOB
	end

	IF @MAG_RELATED_RUN_ID > 0
	BEGIN
		update tb_MAG_RELATED_RUN
			set USER_STATUS = 'Imported'
			where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
	END

	delete from TB_MAG_ITEM_PAPER_INSERT_TEMP where GUID_JOB = @GUID_JOB
	delete from TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP where GUID_JOB = @GUID_JOB
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagItemPaperInsertAvoidDuplicates]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: 
-- Description:	check what "selected" items actually need to be imported.
-- =============================================
ALTER PROCEDURE [dbo].[st_MagItemPaperInsertAvoidDuplicates] 
	-- Add the parameters for the stored procedure here
	@MAG_IDs nvarchar(4000),
	@REVIEW_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @IDs table (PaperId bigint)
	insert into @IDs select value from dbo.fn_Split_int(@MAG_IDs, ',')
	where value not in (
		select paperid from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and imm.REVIEW_ID = @REVIEW_ID
		where ir.REVIEW_ID = @REVIEW_id and ir.IS_DELETED = 'false' and
			(imm.AutoMatchScore > 0.8 or (imm.ManualFalseMatch = 'true' or imm.ManualTrueMatch = 'true'))
		)
	select * from @IDs
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagLogInsert]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_MAG_LOG
-- =============================================
ALTER PROCEDURE [dbo].[st_MagLogInsert] 
	
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
/****** Object:  StoredProcedure [dbo].[st_MagLogList]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
ALTER PROCEDURE [dbo].[st_MagLogList] 
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
GO
/****** Object:  StoredProcedure [dbo].[st_MagLogUpdate]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Update record in tb_MAG_LOG
-- =============================================
ALTER PROCEDURE [dbo].[st_MagLogUpdate] 
	
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
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchedPaperManualEdit]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_item_mag_match based on manual lookup
-- =============================================
ALTER PROCEDURE [dbo].[st_MagMatchedPaperManualEdit] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_ID BIGINT
,	@PaperId BIGINT
,	@ManualTrueMatch bit
,	@ManualFalseMatch bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	declare @chk int
	select @chk = count(*) from tb_ITEM_MAG_MATCH
		where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId

	if @chk = 0
	begin
		insert into tb_ITEM_MAG_MATCH(REVIEW_ID, ITEM_ID, PaperId, ManualTrueMatch, ManualFalseMatch, AutoMatchScore)
		values (@REVIEW_ID, @ITEM_ID, @PaperId, @ManualTrueMatch, @ManualFalseMatch, 1)
	end
	else
	begin
		update tb_ITEM_MAG_MATCH
			set ManualTrueMatch = @ManualTrueMatch,
				ManualFalseMatch = @ManualFalseMatch
			where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId
	end
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchedPapersClear]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_item_mag_match based on manual lookup
-- =============================================
ALTER PROCEDURE [dbo].[st_MagMatchedPapersClear] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_ID BIGINT
,	@ATTRIBUTE_ID BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if @ITEM_ID > 0
		delete from tb_ITEM_MAG_MATCH
		where ITEM_ID = @ITEM_ID and REVIEW_ID = @REVIEW_ID

	else
		if @ATTRIBUTE_ID > 0
			delete from tb_ITEM_MAG_MATCH
			where REVIEW_ID = REVIEW_ID and item_id in
				(select ia.ITEM_ID from TB_ITEM_ATTRIBUTE ia inner join TB_ITEM_SET iset on
					iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'True'
					where ia.ATTRIBUTE_ID = @ATTRIBUTE_ID)

		else
			delete from tb_ITEM_MAG_MATCH
			where REVIEW_ID = @REVIEW_ID
    
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchedPapersInsert]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_item_mag_match based on manual lookup
-- =============================================
ALTER PROCEDURE [dbo].[st_MagMatchedPapersInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_ID BIGINT
,	@PaperId bigint
,	@AutoMatchScore float
,	@ManualTrueMatch bit = null
,	@ManualFalseMatch bit = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	declare @chk int
	select @chk = count(*) from tb_ITEM_MAG_MATCH
		where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId

	if @chk = 0
	begin
		insert into tb_ITEM_MAG_MATCH(REVIEW_ID, ITEM_ID, PaperId, ManualTrueMatch, ManualFalseMatch, AutoMatchScore)
		values (@REVIEW_ID, @ITEM_ID, @PaperId, @ManualTrueMatch, @ManualFalseMatch, @AutoMatchScore)
	end
	else
	begin
		update Reviewer.dbo.tb_ITEM_MAG_MATCH
			set ManualTrueMatch = @ManualTrueMatch,
				ManualFalseMatch = @ManualFalseMatch
			where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId
	end
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchItemsGetIdList]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Match EPPI-Reviewer TB_ITEM records to MAG Papers
-- =============================================
ALTER PROCEDURE [dbo].[st_MagMatchItemsGetIdList] 
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
/****** Object:  StoredProcedure [dbo].[st_MagPaper]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns a single paper
-- =============================================
ALTER PROCEDURE [dbo].[st_MagPaper] 
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
/****** Object:  StoredProcedure [dbo].[st_MagPaperListByIdIds]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List of MAG papers from a list of paper IDs
-- =============================================
ALTER PROCEDURE [dbo].[st_MagPaperListByIdIds] 
	-- Add the parameters for the stored procedure here
	@PaperIds nvarchar(max)
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@REVIEW_ID int = 0
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT @Total = count(value) from dbo.fn_Split_int(@PaperIds, ',')

    -- Insert statements for procedure here
	SELECT distinct ids.value PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from dbo.fn_Split_int(@PaperIds, ',') ids		
		left outer join tb_ITEM_MAG_MATCH imm on imm.PaperId = ids.value
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID and AutoMatchScore > 0.8
		order by PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END

GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersListIds]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
ALTER PROCEDURE [dbo].[st_MagRelatedPapersListIds] 
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
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunGetSeedIds]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Stage 1 in getting related papers: get the list of seed MAG IDs
-- =============================================
ALTER PROCEDURE [dbo].[st_MagRelatedPapersRunGetSeedIds] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID int
,	@REVIEW_ID INT
,	@ATTRIBUTE_ID BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if @ATTRIBUTE_ID > 0
	BEGIN
		select imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
		inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = ia.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
		WHERE (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		group by imm.PaperId
		order by imm.PaperId
	END
	else
	BEGIN
		select imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
		WHERE (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		group by imm.PaperId
		order by imm.PaperId
	END
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRuns]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all runs for a given review
-- =============================================
ALTER PROCEDURE [dbo].[st_MagRelatedPapersRuns] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select mrr.MAG_RELATED_RUN_ID, mrr.REVIEW_ID, mrr.USER_DESCRIPTION, mrr.ATTRIBUTE_ID, a.ATTRIBUTE_NAME,
		mrr.ALL_INCLUDED, mrr.DATE_FROM, mrr.DATE_RUN, mrr.STATUS, mrr.USER_STATUS, mrr.N_PAPERS,
		mrr.MODE, mrr.Filtered from tb_MAG_RELATED_RUN mrr
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mrr.ATTRIBUTE_ID
		where review_id = @REVIEW_ID
		order by MAG_RELATED_RUN_ID
		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsDelete]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
ALTER PROCEDURE [dbo].[st_MagRelatedPapersRunsDelete] 
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
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsInsert]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
ALTER PROCEDURE [dbo].[st_MagRelatedPapersRunsInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int
,	@USER_DESCRIPTION NVARCHAR(1000)
,	@PaperIdList nvarchar(max)
,	@ATTRIBUTE_ID bigint = 0
,	@ALL_INCLUDED BIT
,	@DATE_FROM DATETIME
--,	@AUTO_RERUN BIT
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
		ALL_INCLUDED, DATE_FROM, STATUS, USER_STATUS, MODE, Filtered, N_PAPERS)
	values(@REVIEW_ID, @USER_DESCRIPTION, @PaperIdList, @ATTRIBUTE_ID,
		@ALL_INCLUDED, @DATE_FROM, @STATUS, 'Waiting', @MODE, @FILTERED, 0)

	set @MAG_RELATED_RUN_ID = @@IDENTITY

END
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsUpdate]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
ALTER PROCEDURE [dbo].[st_MagRelatedPapersRunsUpdate] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID int,
	--@AUTO_RERUN BIT,
	@USER_DESCRIPTION NVARCHAR(1000)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	update tb_MAG_RELATED_RUN
		set USER_STATUS = 'Checked',
		--AUTO_RERUN = @AUTO_RERUN,
		USER_DESCRIPTION = @USER_DESCRIPTION
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsUpdatePostRun]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	After the related item ids are found, this updates the record for the UI list
-- =============================================
ALTER PROCEDURE [dbo].[st_MagRelatedPapersRunsUpdatePostRun] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID int,
	@N_PAPERS int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	update tb_MAG_RELATED_RUN
		set [STATUS] = 'Complete',
		N_PAPERS = @N_PAPERS,
		DATE_RUN = GETDATE(),
		USER_STATUS = 'Not imported'
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedRun_Update]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: 
-- Description:	mark a line in tb_MAG_RELATED_RUN as @STATUS
-- =============================================
ALTER PROCEDURE [dbo].[st_MagRelatedRun_Update] 
	-- Add the parameters for the stored procedure here
	@STATUS nvarchar(50),
	@REVIEW_ID INT,
	@MAG_RELATED_RUN_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	update tb_MAG_RELATED_RUN
			set USER_STATUS = @STATUS
			where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID and REVIEW_ID = @REVIEW_ID
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMagInfo]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns statistics about matching review items to MAG
-- =============================================
ALTER PROCEDURE [dbo].[st_MagReviewMagInfo] 
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
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NMatchedAccuratelyExcluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'false'
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NRequiringManualCheckIncluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'true'
		where imm.AutoMatchScore < 0.8 and ((imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null) and (imm.ManualTrueMatch = 'false' or imm.ManualTrueMatch is null))
			and not imm.ITEM_ID in (select imm2.ITEM_ID from tb_ITEM_MAG_MATCH imm2 inner join TB_ITEM_REVIEW ir2 on ir2.REVIEW_ID = imm.REVIEW_ID
				where imm2.AutoMatchScore >=0.8 or imm.ManualTrueMatch = 'true' and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null))
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NRequiringManualCheckExcluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'false'
		where imm.AutoMatchScore < 0.8 and ((imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null) and (imm.ManualTrueMatch = 'false' or imm.ManualTrueMatch is null))
			and not imm.ITEM_ID in (select imm2.ITEM_ID from tb_ITEM_MAG_MATCH imm2 inner join TB_ITEM_REVIEW ir2 on ir2.REVIEW_ID = imm.REVIEW_ID
				where imm2.AutoMatchScore >=0.8 or imm.ManualTrueMatch = 'true' and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null))
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NNotMatchedIncluded = count(distinct ir.ITEM_ID) from TB_ITEM_REVIEW ir
		left outer join tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = ir.ITEM_ID
			where ir.IS_INCLUDED = 'true' and imm.ITEM_ID is null and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'

	select @NNotMatchedExcluded = count(distinct ir.ITEM_ID) from TB_ITEM_REVIEW ir
		left outer join tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = ir.ITEM_ID
			where ir.IS_INCLUDED = 'false' and imm.ITEM_ID is null and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'
	
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMatchedPapersIds]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns all the papers that are matched to a given tb_ITEM record
-- =============================================
ALTER PROCEDURE [dbo].[st_MagReviewMatchedPapersIds] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int = 0
,	@INCLUDED varchar(10) = 'included'
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if @INCLUDED = 'included'
	begin
		SELECT @Total = count(distinct imm.paperid) from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)
		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	end
	else if @INCLUDED = 'excluded'
	begin
		SELECT @Total = count(distinct imm.paperid) from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'false' and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		-- Insert statements for procedure here
		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'false' and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)
		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	end
	else
	begin -- i.e. included = included AND excluded
		SELECT @Total = count(distinct imm.paperid) from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		-- Insert statements for procedure here
		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)
		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	end

	SELECT  @Total as N'@Total'

END
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMatchedPapersWithThisCodeIds]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns all the papers that are matched to a given tb_ITEM record
-- =============================================
ALTER PROCEDURE [dbo].[st_MagReviewMatchedPapersWithThisCodeIds] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int = 0
,	@ATTRIBUTE_IDs nvarchar(max)
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

		SELECT @Total = count(distinct imm.paperid) from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		inner join TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = ir.ITEM_ID
		inner join dbo.fn_Split_int(@ATTRIBUTE_IDs, ',') ids on ids.value = tia.ATTRIBUTE_ID
		inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'

		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		-- Insert statements for procedure here
		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		inner join TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = ir.ITEM_ID
		inner join dbo.fn_Split_int(@ATTRIBUTE_IDs, ',') ids on ids.value = tia.ATTRIBUTE_ID
		inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'

		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	

	SELECT  @Total as N'@Total'

END
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearchDelete]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSearchDelete] 
	-- Add the parameters for the stored procedure here
	@MAG_SEARCH_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	DELETE FROM TB_MAG_SEARCH
		WHERE MAG_SEARCH_ID = @MAG_SEARCH_ID
		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearchInsert]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_MAG_LOG
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSearchInsert] 
	@REVIEW_ID INT,
	@CONTACT_ID int,
	@SEARCH_TEXT NVARCHAR(MAX) = NULL,
	@SEARCH_NO INT = 0,
	@HITS_NO INT = 0,
	@SEARCH_DATE DATETIME,
	@MAG_VERSION NVARCHAR(10),
	@MAG_SEARCH_TEXT NVARCHAR(MAX),

	@MAG_SEARCH_ID INT OUTPUT
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT @SEARCH_NO = MAX(SEARCH_NO) + 1 FROM TB_MAG_SEARCH
		WHERE REVIEW_ID = @REVIEW_ID

	IF @SEARCH_NO IS NULL
		SET @SEARCH_NO = 1

    INSERT INTO TB_MAG_SEARCH(REVIEW_ID, CONTACT_ID, SEARCH_TEXT, SEARCH_NO, HITS_NO,
		SEARCH_DATE, MAG_VERSION, MAG_SEARCH_TEXT)
	VALUES(@REVIEW_ID, @CONTACT_ID, @SEARCH_TEXT, @SEARCH_NO, @HITS_NO,
		@SEARCH_DATE, @MAG_VERSION, @MAG_SEARCH_TEXT)
	
	SET @MAG_SEARCH_ID = @@IDENTITY
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearchList]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSearchList] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	SELECT MAG_SEARCH_ID, REVIEW_ID, C.CONTACT_ID, C.CONTACT_NAME, SEARCH_TEXT, SEARCH_NO, HITS_NO, SEARCH_DATE, MAG_VERSION, MAG_SEARCH_TEXT
	FROM TB_MAG_SEARCH
	INNER JOIN TB_CONTACT C ON C.CONTACT_ID = TB_MAG_SEARCH.CONTACT_ID
	where REVIEW_ID = @REVIEW_ID
		order by SEARCH_NO desc
		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationClassifierScoresUpdate]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSimulationClassifierScoresUpdate] 
	-- Add the parameters for the stored procedure here
	@MAG_SIMULATION_ID int,
	@Field nvarchar(30)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	if @Field = 'STUDY_TYPE_CLASSIFIER_SCORE'
	BEGIN
		UPDATE MSR
		SET STUDY_TYPE_CLASSIFIER_SCORE = SCORE
		FROM TB_MAG_SIMULATION_RESULT MSR
		INNER JOIN TB_MAG_SIMULATION_CLASSIFIER_TEMP MSC ON MSC.MAG_SIMULATION_ID = MSR.MAG_SIMULATION_ID and MSC.PaperId = MSR.PaperId
	END

	IF @Field = 'USER_CLASSIFIER_MODEL_SCORE'
	BEGIN
		UPDATE MSR
		SET USER_CLASSIFIER_MODEL_SCORE = SCORE
		FROM TB_MAG_SIMULATION_RESULT MSR
		INNER JOIN TB_MAG_SIMULATION_CLASSIFIER_TEMP MSC ON MSC.MAG_SIMULATION_ID = MSR.MAG_SIMULATION_ID and MSC.PaperId = MSR.PaperId
	END

	DELETE FROM TB_MAG_SIMULATION_CLASSIFIER_TEMP
		WHERE MAG_SIMULATION_ID = @MAG_SIMULATION_ID

	UPDATE TB_MAG_SIMULATION_RESULT
			SET ENSEMBLE_SCORE = (NETWORK_STATISTIC_SCORE + USER_CLASSIFIER_MODEL_SCORE + STUDY_TYPE_CLASSIFIER_SCORE) / 3
			WHERE MAG_SIMULATION_ID = @MAG_SIMULATION_ID
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationDelete]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Delete simulation
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSimulationDelete] 
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
/****** Object:  StoredProcedure [dbo].[st_MagSimulationGetIds]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers 
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSimulationGetIds] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int = 0
,	@ATTRIBUTE_ID_FILTER BIGINT = 0
,	@ATTRIBUTE_ID_SEED BIGINT = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here


	if not @ATTRIBUTE_ID_FILTER = 0
	begin
		if @ATTRIBUTE_ID_SEED = 0
		begin
			select DISTINCT imm.PaperId, 1 as Training from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER
			inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end
		else
		begin
			select DISTINCT imm.PaperId, case when iat.item_id is null then 0 else 1 end as Training from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER
			inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			left outer join TB_ITEM_ATTRIBUTE iat on iat.ITEM_ID = imm.ITEM_ID and iat.ATTRIBUTE_ID = @ATTRIBUTE_ID_SEED
			left outer join tb_item_set iset on iset.ITEM_SET_ID = iat.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
			where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end
	end
	else
	begin
		if @ATTRIBUTE_ID_SEED = 0
		begin
			select DISTINCT imm.PaperId, 1 as Training from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end
		else
		begin
			select DISTINCT imm.PaperId, case when iat.item_id is null then 0 else 1 end as Training from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			left outer join TB_ITEM_ATTRIBUTE iat on iat.ITEM_ID = imm.ITEM_ID and iat.ATTRIBUTE_ID = @ATTRIBUTE_ID_SEED
			left outer join tb_item_set iset on iset.ITEM_SET_ID = iat.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
			where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end

	end
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationInsert]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSimulationInsert] 
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
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationResults]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSimulationResults] 
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
	SELECT DISTINCT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.NETWORK_STATISTIC_SCORE desc

if @OrderBy = 'FoS'
	SELECT DISTINCT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.FOS_DISTANCE_SCORE

if @OrderBy = 'User'
	SELECT DISTINCT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.USER_CLASSIFIER_MODEL_SCORE desc

if @OrderBy = 'StudyType'
	SELECT DISTINCT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.STUDY_TYPE_CLASSIFIER_SCORE desc

if @OrderBy = 'Ensemble'
	SELECT DISTINCT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId
		order by msr.ENSEMBLE_SCORE desc
		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulations]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all simulations for a given review
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSimulations] 
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
/****** Object:  StoredProcedure [dbo].[st_MagSimulationUpdatePostRun]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	After the simulation has run, this updates the table accordingly
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSimulationUpdatePostRun] 
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
	insert into @SeekingIdsTable(PaperId)
		select value from dbo.fn_Split_int(@InferenceIds, ',')

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

	select @FN = count(*) - @TP from @SeekingIdsTable

	select @FP = count(*) from TB_MAG_SIMULATION_RESULT msr
		where msr.MAG_SIMULATION_ID = @MAG_SIMULATION_ID and msr.INCLUDED = 'False'

	update TB_MAG_SIMULATION
		set STATUS = 'Complete',
		FP = @FP,
		FN = @FN,
		TP = @TP,
		SeedIds = @SeedIds,
		SeekingIds = (Select STRING_AGG(CAST(PaperId as nvarchar(max)), ',') From @SeekingIdsTable),
		NSEEDS = @NSeeds
		where MAG_SIMULATION_ID = @MAG_SIMULATION_ID	
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationUpdateStatus]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Update the status on a simulation (e.g. failure)
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSimulationUpdateStatus] 
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
/****** Object:  StoredProcedure [dbo].[st_MagUpdateChangedIds]    Script Date: 30/11/2020 10:47:18 ******/
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
		inner join TB_MAG_CHANGED_PAPER_IDS mcp on mcp.OldPaperId = imm.PaperId and mcp.ITEM_ID = imm.ITEM_ID
		where mcp.MagVersion = @MagVersion
		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagUpdateMissingPaperIds]    Script Date: 30/11/2020 10:47:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Update the list of changed PaperIds with a new paperid
-- =============================================
ALTER PROCEDURE [dbo].[st_MagUpdateMissingPaperIds] 
	@MagChangedPaperIdsId int,
	@NewAutoMatchScore float,
	@NewPaperId bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	update TB_MAG_CHANGED_PAPER_IDS
		set NewPaperId = @NewPaperId,
		NewAutoMatchScore = @NewAutoMatchScore
		where MagChangedPaperIdsId = @MagChangedPaperIdsId
		
END
GO
