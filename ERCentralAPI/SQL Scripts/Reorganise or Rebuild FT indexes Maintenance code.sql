--SETUP ADD/REMOVE lines to @indexesSettings table-------------------------------------------------------------------------------------------
--SET @whatif to 1 to only produce the commands, but avoid executing them

declare @whatif bit = 1;--set it to 0 to make sure the rebuild/reorg actually happens
declare @indexesSettings table (catalog_name nvarchar(200)
	, ReorganiseLowerThresholdPercentage float
	, RebuildLowerThresholdPercentage float
	, ReorganiseLowerThresholdFragmentsCount int
	, RebuildLowerThresholdFragmentsCount int
	, done bit)

insert into @indexesSettings (catalog_name 
	, ReorganiseLowerThresholdPercentage 
	, RebuildLowerThresholdPercentage 
	, ReorganiseLowerThresholdFragmentsCount 
	, RebuildLowerThresholdFragmentsCount 
	, done) 
	VALUES ('tb_ITEM_FTIndex', 5.0, 30.0, 5, 15, 0)
	,('tb_ITEM_DOCUMENT_FTIndex', 5.0, 30.0, 5, 15, 0)
	--,('tb_ITEM_ATTRIBUTE_FTIndex', 5.0, 30.0, 5, 15, 0)

--END of SETUP------------------------------------------------------------------------------------------------------------------

--MAIN ROUTINE------------------------------------------------------------------------------------------------------------------
declare @catalog_name nvarchar(200) = '';
declare @ReorganiseLowerThresholdPercentage float = 0;
declare @RebuildLowerThresholdPercentage float = 0; --does a rebuild if fragmentation is above this level
declare @ReorganiseLowerThresholdFragmentsCount int = 0;
declare @RebuildLowerThresholdFragmentsCount int = 0; --does a rebuild if fragmentation is above this level

declare @CurrentFragmentationPerc float;
declare @CurrentFragmentsNumb int;
declare @RowsToProcess int = (select count(*) from @indexesSettings);
declare @done int = 0;

--MAIN LOOP repeating for each line in @indexesSettings -------------------------------------------------------------------------
WHILE @done < @RowsToProcess AND @done < 200
BEGIN
	SELECT @catalog_name = catalog_name, @ReorganiseLowerThresholdPercentage = ReorganiseLowerThresholdPercentage, @RebuildLowerThresholdPercentage = RebuildLowerThresholdPercentage
			, @ReorganiseLowerThresholdFragmentsCount = ReorganiseLowerThresholdFragmentsCount, @RebuildLowerThresholdFragmentsCount = RebuildLowerThresholdFragmentsCount
		FROM (select top 1 * from @indexesSettings where done = 0) as a;
	print 'Processing: ' + @catalog_name;

	--Mark row as done and increase @done counter
	set @done = @done + 1;
	update @indexesSettings set done = 1 where done = 0 and catalog_name = @catalog_name

	--Sanity check, make sure the catalog indicated exists
	declare @chk int = (SELECT count(*) FROM sys.fulltext_catalogs c where c.name = @catalog_name);
	IF @chk = 1 
	BEGIN
		print 'SANITY check passed for: ' + @catalog_name 
		
		--adapted from: https://dba.stackexchange.com/a/108479
		SELECT 
			@CurrentFragmentsNumb = f.num_fragments,
			@CurrentFragmentationPerc = 100.0 * (f.fulltext_mb - f.largest_fragment_mb) / NULLIF(f.fulltext_mb, 0) --AS fulltext_fragmentation_in_percent
		FROM sys.fulltext_catalogs c
		JOIN sys.fulltext_indexes i
			ON i.fulltext_catalog_id = c.fulltext_catalog_id
		JOIN (
			-- Compute fragment data for each table with a full-text index
			SELECT table_id,
				COUNT(*) AS num_fragments,
				CONVERT(DECIMAL(9,2), SUM(data_size/(1024.*1024.))) AS fulltext_mb,
				CONVERT(DECIMAL(9,2), MAX(data_size/(1024.*1024.))) AS largest_fragment_mb
			FROM sys.fulltext_index_fragments
			GROUP BY table_id
		) f
			ON f.table_id = i.object_id
		WHERE c.name = @catalog_name;

		print 'Fragments on ' + @catalog_name + '. Percentage: ' + CAST(@CurrentFragmentationPerc as NVARCHAR(20)) + '. Fragments count: ' + CAST(@CurrentFragmentsNumb as NVARCHAR(20));

		--what to do? we have 3 possiblities:
		-- (0) do nothing, when both curr vals are below the lower thresholds
		-- (1) reorganise if at least one current value is above the lower threshold, but none is above the rebuild threshold
		-- (2) rebuild when one of the curr vals is above the rebuild threshold

		declare @toDoCase int = 0;--default
		IF @CurrentFragmentationPerc >= @RebuildLowerThresholdPercentage OR @CurrentFragmentsNumb >= @RebuildLowerThresholdFragmentsCount
			set @toDoCase = 2;
		ELSE IF @CurrentFragmentationPerc >= @ReorganiseLowerThresholdPercentage OR @CurrentFragmentsNumb >= @ReorganiseLowerThresholdFragmentsCount
			set @toDoCase = 1;
		declare @cmd nvarchar(max) = ''
		if @toDoCase = 0 continue;
		else if @toDoCase = 1
		BEGIN
			print 'Will reorganise ' + @catalog_name ;
			set @cmd = 'ALTER FULLTEXT CATALOG ' + @catalog_name + ' REORGANIZE;';
		END
		else if @toDoCase = 2
		BEGIN
			print 'Will rebuild ' + @catalog_name;
			set @cmd = 'ALTER FULLTEXT CATALOG ' + @catalog_name + ' REBUILD;';
		END

		Print 'Command to execute: ' + @cmd;
		IF @whatif = 0 Execute (@cmd);
	END
	ELSE Print 'Sanity check for ' + @catalog_name + ' failed. No index with that name.' --sanity check failed, we do nothing for this "line"...
END
--END of MAIN LOOP  -------------------------------------------------------------------------------------------------------------------
GO