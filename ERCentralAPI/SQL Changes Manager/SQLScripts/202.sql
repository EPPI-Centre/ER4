USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationUpdatePostRun]    Script Date: 05/05/2020 07:22:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	After the simulation has run, this updates the table accordingly
-- =============================================
ALTER     PROCEDURE [dbo].[st_MagSimulationUpdatePostRun] 
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