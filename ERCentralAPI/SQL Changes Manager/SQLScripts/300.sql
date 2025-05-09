USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListMagMatches]    Script Date: 03/12/2020 15:30:36 ******/
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
			INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID and IMM.REVIEW_ID = @REVIEW_ID 
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
			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
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

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListMagNoMatches]    Script Date: 03/12/2020 15:31:59 ******/
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
			left outer JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID and IMM.REVIEW_ID = @REVIEW_ID
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
			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
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

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListMagSimulationTPFN]    Script Date: 03/12/2020 15:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER procedure [dbo].[st_ItemListMagSimulationTPFN]
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
			INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID and IMM.REVIEW_ID = I.REVIEW_ID
				AND (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			INNER JOIN TB_MAG_SIMULATION_RESULT MSR ON MSR.PaperId = IMM.PaperId
			WHERE I.REVIEW_ID = @REVIEW_ID AND I.IS_DELETED = 'FALSE' and MSR.INCLUDED = 'True'
				and msr.MAG_SIMULATION_ID = @MAG_SIMULATION_ID
	  END
	  else
	  begin

		declare @seekingids nvarchar(max)

		select @seekingids = (Select STRING_AGG(SeekingIds, ',') From TB_MAG_SIMULATION
			where MAG_SIMULATION_ID = @MAG_SIMULATION_ID)
			INSERT INTO @ID
		SELECT DISTINCT (I.ITEM_ID) FROM TB_ITEM_REVIEW I
			INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID AND IMM.REVIEW_ID = I.REVIEW_ID
				AND (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
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
      SELECT DISTINCT (i.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
            ROW_NUMBER() OVER(order by SHORT_TITLE, i.ITEM_ID) RowNum
      FROM TB_ITEM i
			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
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

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagItemMagRelatedPaperInsert]    Script Date: 03/12/2020 15:35:24 ******/
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
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and imm.REVIEW_ID = ir.REVIEW_ID
			where ir.REVIEW_ID = @REVIEW_id and ir.IS_DELETED = 'false' and
				(imm.AutoMatchScore > 0.8 or (imm.ManualFalseMatch = 'true' or imm.ManualTrueMatch = 'true')))

	
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagItemMatchedPapersIds]    Script Date: 03/12/2020 15:42:08 ******/
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
	inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
	where imm.ITEM_ID = @ITEM_ID
	ORDER BY ManualTrueMatch desc, imm.AutoMatchScore desc
	/*
	OFFSET (@PageNo-1) * @RowsPerPage ROWS
	FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
	*/
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagItemPaperInsert]    Script Date: 03/12/2020 15:43:05 ******/
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
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = imm.REVIEW_ID
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
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchedPapersClear]    Script Date: 03/12/2020 15:47:59 ******/
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
			where REVIEW_ID = @REVIEW_ID and item_id in
				(select ia.ITEM_ID from TB_ITEM_ATTRIBUTE ia inner join TB_ITEM_SET iset on
					iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'True'
					where ia.ATTRIBUTE_ID = @ATTRIBUTE_ID)

		else
			delete from tb_ITEM_MAG_MATCH
			where REVIEW_ID = @REVIEW_ID
    
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunGetSeedIds]    Script Date: 03/12/2020 15:50:21 ******/
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
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = ia.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
		WHERE (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		group by imm.PaperId
		order by imm.PaperId
	END
	else
	BEGIN
		select imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
		WHERE (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		group by imm.PaperId
		order by imm.PaperId
	END
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMatchedPapersIds]    Script Date: 03/12/2020 15:51:48 ******/
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
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
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
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
			and ir.IS_INCLUDED = 'false' and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		-- Insert statements for procedure here
		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
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
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
			and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		-- Insert statements for procedure here
		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
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

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMatchedPapersWithThisCodeIds]    Script Date: 03/12/2020 15:55:20 ******/
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
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false' and ir.REVIEW_ID = imm.REVIEW_ID
		inner join TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = ir.ITEM_ID
		inner join dbo.fn_Split_int(@ATTRIBUTE_IDs, ',') ids on ids.value = tia.ATTRIBUTE_ID
		inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'

		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		-- Insert statements for procedure here
		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false' and ir.REVIEW_ID = imm.REVIEW_ID
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

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationGetIds]    Script Date: 03/12/2020 15:57:28 ******/
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
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
			where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end
		else
		begin
			select DISTINCT imm.PaperId, case when iat.item_id is null then 0 else 1 end as Training from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER
			inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
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
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
			where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end
		else
		begin
			select DISTINCT imm.PaperId, case when iat.item_id is null then 0 else 1 end as Training from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
			left outer join TB_ITEM_ATTRIBUTE iat on iat.ITEM_ID = imm.ITEM_ID and iat.ATTRIBUTE_ID = @ATTRIBUTE_ID_SEED
			left outer join tb_item_set iset on iset.ITEM_SET_ID = iat.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
			where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end

	end
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SourceDeleteForever]    Script Date: 03/12/2020 16:00:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sergio
-- Create date: 20/7/09
-- Description:	(Un/)Delete a source and all its Items
-- =============================================
ALTER PROCEDURE [dbo].[st_SourceDeleteForever] 
	-- Add the parameters for the stored procedure here
	@srcID int,
	@revID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @check int = 0
	--make sure source belongs to review...
	set @check = (select count(source_id) from TB_SOURCE where SOURCE_ID = @srcID and REVIEW_ID = @revID)
	if (@check != 1) return

	Declare @tt TABLE
	(
		item_ID bigint PRIMARY KEY
		,cnt int  	
	)
	insert into @tt --First: get the ITEM_IDs we'll deal with and see if they appear in more than one review
	SELECT ITEM_ID, cnt FROM
		(select ir.ITEM_ID, COUNT(ir.item_id) cnt from TB_ITEM_REVIEW ir 
			inner join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
			where tis.SOURCE_ID = @srcID -- cnt = 1
			group by ir.ITEM_ID) cc

	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
	BEGIN TRANSACTION


	--Second: delete the records in TB_ITEM_REVIEW for the items that are shared across reviews
	delete from TB_ITEM_REVIEW 
		where REVIEW_ID = (select REVIEW_ID from TB_SOURCE where SOURCE_ID = @srcID)
			AND ITEM_ID in (select item_ID from @tt where cnt > 1)
	--Third: explicitly delete the records that can't be automatically deleted through the foreign key cascade actions
	-- the cnt=1 clause makes sure we don't touch data related to items that appear in other reviews.
	DELETE FROM TB_ITEM_DUPLICATES where ITEM_ID_OUT in (SELECT item_ID from @tt where cnt = 1)
	DELETE FROM TB_ITEM_LINK where ITEM_ID_SECONDARY in (SELECT item_ID from @tt where cnt = 1)
	DELETE FROM TB_ITEM_DOCUMENT where ITEM_ID in (SELECT item_ID from @tt where cnt = 1)
	DELETE From TB_ITEM_ATTRIBUTE where ITEM_ID in (SELECT item_ID from @tt where cnt = 1) and (ITEM_ARM_ID is not null AND ITEM_ARM_ID > 0)
	DELETE From TB_ITEM_ARM where ITEM_ID in (SELECT item_ID from @tt where cnt = 1)
	DELETE From tb_ITEM_MAG_MATCH where ITEM_ID in (SELECT item_ID from @tt ) and REVIEW_ID = @revID
	--Fourth: delete the items 
	DELETE FROM TB_ITEM WHERE ITEM_ID in (SELECT item_ID from @tt where cnt = 1)
	--Fifth: delete the source
	DELETE FROM TB_SOURCE WHERE SOURCE_ID = @srcID
	--there is a lot that happens automatically through the cascade option of foreing key relationships
	--if some cross-reference route is not covered by the explicit deletions the whole transaction should rollback thanks to the catch clause.
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	END CATCH
	END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListMaybeMagMatches]    Script Date: 03/12/2020 16:27:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER procedure [dbo].[st_ItemListMaybeMagMatches]
(
      @REVIEW_ID INT,
      @SHOW_INCLUDED BIT = 'true',
      @ATTRIBUTE_SET_ID_LIST NVARCHAR(MAX) = '',
      
      @PageNum INT = 1,
      @PerPage INT = 3,
      @CurrentPage INT OUTPUT,
      @TotalPages INT OUTPUT,
      @TotalRows INT OUTPUT 
)
with recompile
As
begin
SET NOCOUNT ON

declare @RowsToRetrieve int
Declare @ID table (ItemID bigint primary key )
IF (@ATTRIBUTE_SET_ID_LIST = '')
BEGIN

       --store IDs to build paged results as a simple join
	  INSERT INTO @ID
	  SELECT DISTINCT (I.ITEM_ID) FROM TB_ITEM I
      INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
			INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID AND imm.REVIEW_ID = @REVIEW_ID

			where imm.AutoMatchScore < 0.8 and ((imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null) and (imm.ManualTrueMatch = 'false' or imm.ManualTrueMatch is null))
			and not imm.ITEM_ID in 
			(
				select imm2.ITEM_ID from tb_ITEM_MAG_MATCH imm2 
				where imm.REVIEW_ID = @REVIEW_ID and imm2.ITEM_ID = imm.ITEM_ID
				AND (
						imm2.AutoMatchScore >=0.8 or 
						(
							imm2.ManualTrueMatch = 'true' and (imm2.ManualFalseMatch <> 'true' or imm2.ManualFalseMatch is null)
						)
					)
			)

	  SELECT @TotalRows = @@ROWCOUNT

END
ELSE /* FILTER BY A LIST OF ATTRIBUTES */
BEGIN
	  --store IDs to build paged results as a simple join
	  INSERT INTO @ID
	  SELECT DISTINCT I.ITEM_ID
	  --SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
            FROM TB_ITEM_REVIEW I
      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
      INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
      INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!
	  INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID and imm.REVIEW_ID = i.REVIEW_ID

      WHERE I.IS_INCLUDED = @SHOW_INCLUDED
            AND I.REVIEW_ID = @REVIEW_ID
			AND I.IS_DELETED = 'FALSE'

		and imm.AutoMatchScore < 0.8 and ((imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null) and (imm.ManualTrueMatch = 'false' or imm.ManualTrueMatch is null))
			and not imm.ITEM_ID in 
			(
				select imm2.ITEM_ID from tb_ITEM_MAG_MATCH imm2 
				where imm.REVIEW_ID = @REVIEW_ID and imm2.ITEM_ID = imm.ITEM_ID
				AND (
						imm2.AutoMatchScore >=0.8 or 
						(
							imm2.ManualTrueMatch = 'true' and (imm2.ManualFalseMatch <> 'true' or imm2.ManualFalseMatch is null)
						)
					)
			)
	  SELECT @TotalRows = @@ROWCOUNT
END
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
			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
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

END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMagInfo]    Script Date: 03/12/2020 17:35:04 ******/
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
			and ir.IS_INCLUDED = 'true' and imm.REVIEW_ID = ir.REVIEW_ID
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NMatchedAccuratelyExcluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'false' and imm.REVIEW_ID = ir.REVIEW_ID
		where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NRequiringManualCheckIncluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'true' and imm.REVIEW_ID = ir.REVIEW_ID
		where imm.AutoMatchScore < 0.8 and ((imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null) and (imm.ManualTrueMatch = 'false' or imm.ManualTrueMatch is null))
			and not imm.ITEM_ID in 
			(
				select imm2.ITEM_ID from tb_ITEM_MAG_MATCH imm2 
				where imm.REVIEW_ID = @REVIEW_ID and imm2.ITEM_ID = imm.ITEM_ID
				AND (
						imm2.AutoMatchScore >=0.8 or 
						(
							imm2.ManualTrueMatch = 'true' and (imm2.ManualFalseMatch <> 'true' or imm2.ManualFalseMatch is null)
						)
					)
			)
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NRequiringManualCheckExcluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'false' and imm.REVIEW_ID = ir.REVIEW_ID
		where imm.AutoMatchScore < 0.8 and ((imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null) and (imm.ManualTrueMatch = 'false' or imm.ManualTrueMatch is null))
			and not imm.ITEM_ID in 
			(
				select imm2.ITEM_ID from tb_ITEM_MAG_MATCH imm2 
				where imm.REVIEW_ID = @REVIEW_ID and imm2.ITEM_ID = imm.ITEM_ID
				AND (
						imm2.AutoMatchScore >=0.8 or 
						(
							imm2.ManualTrueMatch = 'true' and (imm2.ManualFalseMatch <> 'true' or imm2.ManualFalseMatch is null)
						)
					)
			)
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NNotMatchedIncluded = count(distinct ir.ITEM_ID) from TB_ITEM_REVIEW ir
		left outer join tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = ir.ITEM_ID and imm.REVIEW_ID = ir.REVIEW_ID
			where ir.IS_INCLUDED = 'true' and imm.ITEM_ID is null and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'

	select @NNotMatchedExcluded = count(distinct ir.ITEM_ID) from TB_ITEM_REVIEW ir
		left outer join tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = ir.ITEM_ID and imm.REVIEW_ID = ir.REVIEW_ID
			where ir.IS_INCLUDED = 'false' and imm.ITEM_ID is null and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'
	
END
GO





GO