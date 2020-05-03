USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationGetIds]    Script Date: 03/05/2020 10:03:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers 
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagSimulationGetIds] 
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
			where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end
		else
		begin
			select DISTINCT imm.PaperId, case when iat.item_id is null then 0 else 1 end as Training from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER
			inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			left outer join TB_ITEM_ATTRIBUTE iat on iat.ITEM_ID = imm.ITEM_ID and iat.ATTRIBUTE_ID = @ATTRIBUTE_ID_SEED
			left outer join tb_item_set iset on iset.ITEM_SET_ID = iat.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
			where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end
	end
	else
	begin
		if @ATTRIBUTE_ID_SEED = 0
		begin
			select DISTINCT imm.PaperId, 1 as Training from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end
		else
		begin
			select DISTINCT imm.PaperId, case when iat.item_id is null then 0 else 1 end as Training from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			left outer join TB_ITEM_ATTRIBUTE iat on iat.ITEM_ID = imm.ITEM_ID and iat.ATTRIBUTE_ID = @ATTRIBUTE_ID_SEED
			left outer join tb_item_set iset on iset.ITEM_SET_ID = iat.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
			where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end

	end
END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationUpdatePostRun]    Script Date: 03/05/2020 10:01:13 ******/
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
		SeekingIds = (Select STRING_AGG(PaperId, ',') From @SeekingIdsTable),
		NSEEDS = @NSeeds
		where MAG_SIMULATION_ID = @MAG_SIMULATION_ID	
END
GO