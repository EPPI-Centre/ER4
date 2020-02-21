USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMatchedPapersIds]    Script Date: 21/02/2020 00:22:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns all the papers that are matched to a given tb_ITEM record
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagReviewMatchedPapersIds] 
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
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and 
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
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		-- Insert statements for procedure here
		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'false' and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and 
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
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		-- Insert statements for procedure here
		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)
		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	end

	SELECT  @Total as N'@Total'

END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMatchedPapersWithThisCodeIds]    Script Date: 21/02/2020 00:40:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns all the papers that are matched to a given tb_ITEM record
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagReviewMatchedPapersWithThisCodeIds] 
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
		inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_ATTRIBUTE_ID and tis.IS_COMPLETED = 'true'

		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		-- Insert statements for procedure here
		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		inner join TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = ir.ITEM_ID
		inner join dbo.fn_Split_int(@ATTRIBUTE_IDs, ',') ids on ids.value = tia.ATTRIBUTE_ID
		inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_ATTRIBUTE_ID and tis.IS_COMPLETED = 'true'

		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	

	SELECT  @Total as N'@Total'

END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListMaybeMagMatches]    Script Date: 21/02/2020 00:44:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER     procedure [dbo].[st_ItemListMaybeMagMatches]
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
			INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID 

			where imm.AutoMatchScore < 0.7 and ((imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null) and (imm.ManualTrueMatch = 'false' or imm.ManualTrueMatch is null))
			and not imm.ITEM_ID in (select imm2.ITEM_ID from tb_ITEM_MAG_MATCH imm2 inner join TB_ITEM_REVIEW ir2 on ir2.REVIEW_ID = imm.REVIEW_ID
				where imm2.AutoMatchScore >=0.7 or imm.ManualTrueMatch = 'true' and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null))

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
END
ELSE /* FILTER BY A LIST OF ATTRIBUTES */
BEGIN
	      SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
            FROM TB_ITEM_REVIEW I
      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
      INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
      INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!
	  INNER JOIN tb_ITEM_MAG_MATCH IMM ON IMM.ITEM_ID = I.ITEM_ID 

      WHERE I.IS_INCLUDED = @SHOW_INCLUDED
            AND I.REVIEW_ID = @REVIEW_ID
			AND I.IS_DELETED = 'FALSE'

		and imm.AutoMatchScore < 0.7 and ((imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null) and (imm.ManualTrueMatch = 'false' or imm.ManualTrueMatch is null))
			and not imm.ITEM_ID in (select imm2.ITEM_ID from tb_ITEM_MAG_MATCH imm2 inner join TB_ITEM_REVIEW ir2 on ir2.REVIEW_ID = imm.REVIEW_ID
				where imm2.AutoMatchScore >=0.7 or imm.ManualTrueMatch = 'true' and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null))

      set @TotalPages = @TotalRows/@PerPage

      if @PageNum < 1
      set @PageNum = 1

      if @TotalRows % @PerPage != 0
      set @TotalPages = @TotalPages + 1

      set @RowsToRetrieve = @PerPage * @PageNum
      set @CurrentPage = @PageNum;

      WITH SearchResults AS
      (
      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID, ADDITIONAL_TEXT,
            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
      FROM TB_ITEM I
       INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED

      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!
      )
      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS, ADDITIONAL_TEXT
                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
            FROM SearchResults
                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
            WHERE RowNum > @RowsToRetrieve - @PerPage
            AND RowNum <= @RowsToRetrieve 
            ORDER BY SHORT_TITLE
END

SELECT      @CurrentPage as N'@CurrentPage',
            @TotalPages as N'@TotalPages',
            @TotalRows as N'@TotalRows'

END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListMagMatches]    Script Date: 21/02/2020 00:52:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


create or ALTER     procedure [dbo].[st_ItemListMagMatches]
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
			WHERE  (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
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
end
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMatchedPapersWithThisCodeIds]    Script Date: 21/02/2020 00:55:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns all the papers that are matched to a given tb_ITEM record
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagReviewMatchedPapersWithThisCodeIds] 
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

		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		-- Insert statements for procedure here
		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		inner join TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = ir.ITEM_ID
		inner join dbo.fn_Split_int(@ATTRIBUTE_IDs, ',') ids on ids.value = tia.ATTRIBUTE_ID
		inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'

		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and 
			(imm.ManualFalseMatch = 'false' or imm.ManualFalseMatch is null)

		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	

	SELECT  @Total as N'@Total'

END
GO