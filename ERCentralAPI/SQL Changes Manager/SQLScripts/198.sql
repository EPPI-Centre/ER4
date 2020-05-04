USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemListMagSimulationTPFN]    Script Date: 04/05/2020 09:17:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


create or ALTER   procedure [dbo].[st_ItemListMagSimulationTPFN]
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

		select @seekingids = (Select STRING_AGG(SeekingIds, ',') From TB_MAG_SIMULATION
			where MAG_SIMULATION_ID = @MAG_SIMULATION_ID)
			INSERT INTO @ID
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

