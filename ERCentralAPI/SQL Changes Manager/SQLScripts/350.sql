USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListMagPreviousMatches]    Script Date: 24/02/2021 18:36:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER   procedure [dbo].[st_ItemListMagPreviousMatches]
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
			INNER JOIN TB_MAG_CHANGED_PAPER_IDS cpi on cpi.ITEM_ID = i.ITEM_ID
			WHERE cpi.NewPaperId = -1
			AND I.IS_DELETED = 'FALSE' AND I.REVIEW_ID = @REVIEW_ID

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
/****** Object:  StoredProcedure [dbo].[st_MagGetPaperIdsForFoSImport]    Script Date: 26/02/2021 09:56:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get PaperIds to get field of study ids for
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagGetPaperIdsForFoSImport] 
	-- Add the parameters for the stored procedure here
	
	@ITEM_IDS nvarchar(max)
,	@ATTRIBUTE_IDS nvarchar(max)
,	@REVIEW_ID int
,	@REVIEW_SET_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @FILTERED_IDS TABLE (
	ITEM_ID BIGINT PRIMARY KEY
)
	-- these already have attributes in that set so we filter them out
	IF @REVIEW_SET_ID > 0
	BEGIN
		insert into @FILTERED_IDS
		select iset.ITEM_ID from TB_ITEM_SET iset
		inner join TB_REVIEW_SET rs on rs.SET_ID = iset.SET_ID
		where rs.REVIEW_SET_ID = @REVIEW_SET_ID and iset.IS_COMPLETED = 'true' AND RS.REVIEW_ID = @REVIEW_ID
	END

    -- Insert statements for procedure here
	
	if @ATTRIBUTE_IDS = ''
	begin
		if @ITEM_IDS = '' -- everything in the review
		begin
			select PaperId, imm.ITEM_ID from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			where ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false' AND
				(imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
				and not imm.ITEM_ID in (select ITEM_ID from @FILTERED_IDS)
		end
		else
		begin -- filtred by the list of item_ids
			select PaperId, imm.ITEM_ID from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			inner join dbo.fn_Split_int(@ITEM_IDS, ',') ids on ids.value = ir.ITEM_ID
			where ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false' AND
				(imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			and not imm.ITEM_ID in (select ITEM_ID from @FILTERED_IDS)
		end
	end
	else
	begin
		if @ITEM_IDS = '' -- everything in the review filtered by attribute id
		begin
			select PaperId, imm.ITEM_ID from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID
			inner join dbo.fn_Split_int(@ATTRIBUTE_IDS, ',') s on s.value = ia.ATTRIBUTE_ID
			inner join TB_ITEM_SET iset on iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
			where ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false' AND
				(imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			and not imm.ITEM_ID in (select ITEM_ID from @FILTERED_IDS)
		end
	end
		
END

GO