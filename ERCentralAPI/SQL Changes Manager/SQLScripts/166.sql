USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunGetSeedIds]    Script Date: 06/03/2020 20:00:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Stage 1 in getting related papers: get the list of seed MAG IDs
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagRelatedPapersRunGetSeedIds] 
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

	/*
	declare @SeedIds table
	(
		PaperId bigint INDEX idx CLUSTERED
	)
	*/

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

	-- Create an in-memory table of all the TB_ITEM -> MAG matches that we're working from
	if (@PaperIdList = '')
	begin
		if not @ATTRIBUTE_ID = 0
			--insert into @SeedIds
			select imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
			inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = ia.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			group by imm.PaperId
		else
			--insert into @SeedIds
			select imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			group by imm.PaperId
	end
	else
		--insert into @SeedIds
			select value from Dbo.fn_Split_int(@PaperIdList, ',') 

	--SELECT * FROM @SeedIds
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersListIds]    Script Date: 08/03/2020 11:42:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagRelatedPapersListIds] 
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
	
	SELECT mrp.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_MAG_RELATED_PAPERS mrp
		left outer join tb_ITEM_MAG_MATCH imm on (imm.PaperId = mrp.PaperId and imm.REVIEW_ID = @REVIEW_ID)
			and imm.ManualFalseMatch <> 'True' 
		where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
		order by mrp.SimilarityScore
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END
GO