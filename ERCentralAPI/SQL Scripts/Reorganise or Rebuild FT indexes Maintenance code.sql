USE Reviewer
GO
SET NOCOUNT ON
--SETUP ADD/REMOVE lines to @indexesSettings table-------------------------------------------------------------------------------------------
--SET @whatif to 1 to only produce the commands, but avoid executing them

declare @whatif bit = 0;--set it to 0 to make sure the rebuild/reorg actually happens
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
	VALUES ('tb_ITEM_FTIndex', 5.0, 90.0, 5, 10, 0)
	,('tb_ITEM_ATTRIBUTE_FTIndex', 5.0, 30.0, 5, 17, 0)
	,('tb_ITEM_DOCUMENT_FTIndex', 5.0, 70.0, 5, 10, 0)

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
declare @tableName varchar(4000);
declare @toDoCase int = 0;--default;

declare @ToDo table (COMMAND varchar(4000), TableName varchar(4000), isRebuild bit, done bit)

--MAIN DECISIONS LOOP repeating for each line in @indexesSettings -------------------------------------------------------------------------
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
		print 'SANITY check passed for: ' + @catalog_name ;
		--adapted from: https://dba.stackexchange.com/a/108479
		SELECT 
			@CurrentFragmentsNumb = f.num_fragments
			,@CurrentFragmentationPerc = 100.0 * (f.fulltext_mb - f.largest_fragment_mb) / NULLIF(f.fulltext_mb, 0) --AS fulltext_fragmentation_in_percent
			,@tableName = OBJECT_SCHEMA_NAME(i.object_id) + '.' + OBJECT_NAME(i.object_id)
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
		--print 'Table NAME: ' + @tablename

		--what to do? we have 3 possiblities:
		-- (0) do nothing, when both curr vals are below the lower thresholds
		-- (1) reorganise if at least one current value is above the lower threshold, but none is above the rebuild threshold
		-- (2) rebuild when one of the curr vals is above the rebuild threshold

		
		IF @CurrentFragmentationPerc >= @RebuildLowerThresholdPercentage OR @CurrentFragmentsNumb >= @RebuildLowerThresholdFragmentsCount
			set @toDoCase = 2;
		ELSE IF @CurrentFragmentationPerc >= @ReorganiseLowerThresholdPercentage OR @CurrentFragmentsNumb >= @ReorganiseLowerThresholdFragmentsCount
			set @toDoCase = 1;
		declare @cmd varchar(4000) = ''
		if @toDoCase = 0 continue;
		else if @toDoCase = 1
		BEGIN
			print 'Will reorganise ' + @catalog_name + ' on table: ' + @tableName;
			set @cmd = 'ALTER FULLTEXT CATALOG ' + @catalog_name + ' REORGANIZE;';
			insert into @ToDo (COMMAND, tableName, isRebuild, done) values (@cmd, @tableName, 0, 0);
		END
		else if @toDoCase = 2
		BEGIN
			print 'Will rebuild ' + @catalog_name;
			set @cmd = 'ALTER FULLTEXT CATALOG ' + @catalog_name + ' REBUILD;';
			insert into @ToDo (COMMAND, tableName, isRebuild, done) values (@cmd, @tableName, 1, 0);
		END

		Print 'Command to execute: ' + @cmd;
		Print '';
	END
	ELSE Print 'Sanity check for ' + @catalog_name + ' failed. No index with that name.' --sanity check failed, we do nothing for this "line"...
END
--END of MAIN Decision LOOP  -------------------------------------------------------------------------------------------------------------------
Print 'END of decision loop'
--LAST block with nested LOOPs, IF we have things to do
--select count(*) from @ToDo
IF @whatif = 0 AND (select count(*) from @ToDo) > 0
BEGIN
	Print ''
	Print ''
	Print 'START of actions'
	Print ''
	SET @done = 0;
	set @tableName = '';
	--see https://learn.microsoft.com/en-us/sql/t-sql/functions/objectpropertyex-transact-sql?view=sql-server-ver16
	declare @chk1 int; --TableFulltextPopulateStatus 0 if it's not populating 6 if some error, in between otherwise
	declare @chk2 int; --TableFullTextMergeStatus 0 if it's not merging, 1 otherwise
	SET @cmd = ''
	SET @cmd = (select top 1 COMMAND from @ToDo where done = 0);
	WHILE @cmd != '' AND @cmd is NOT NULL and @done < 200
	BEGIN
		set @done = @done + 1;
		--select * from @ToDo
		update @ToDo set done = 1 where COMMAND = @cmd;
		set @tableName = (select top 1 TableName from @ToDo where COMMAND = @cmd)
		--select * from @ToDo

		--DO the command, and wait for it to finish!
		print 'executing: "' + @cmd + '" Table NAME: ' + @tableName + ' CYCLE N: ' + CAST(@done as varchar(10)) + ' TIME: ' + CAST(CONVERT(TIME, GETDATE()) as varchar(100));
		Execute (@cmd);
		print 'past executing: "' + @cmd + '". ' + CAST(CONVERT(TIME, GETDATE()) as varchar(100));
		WAITFOR DELAY '00:00:10';  
		SELECT @chk1 = CAST(OBJECTPROPERTYEX ( object_id(@tableName), N'TableFulltextPopulateStatus') as INT);
		SELECT @chk2 = CAST(OBJECTPROPERTYEX ( object_id(@tableName), N'TableFullTextMergeStatus') as INT);
		declare @maxwait datetime = DATEADD(hour, 12, GETDATE());
		
		--SELECT @chk1 chk1, @chk2 chk2, @maxwait maxwait, GETDATE() now, CASE when (@chk1 !=0 AND @chk1 != 6 OR @chk2 = 1) then '1st true' else '1st false' end
		--	, CASE when GETDATE() < @maxwait then '2nd true' else '2nd false' end
		
		WHILE (@chk1 !=0 AND @chk1 != 6 OR @chk2 = 1) AND GETDATE() < @maxwait
		BEGIN
			print 'busy... will wait 30s';
			WAITFOR DELAY '00:00:30';  
			SELECT @chk1 = CAST(OBJECTPROPERTYEX ( object_id(@tableName), N'TableFulltextPopulateStatus') as INT);
			SELECT @chk2 = CAST(OBJECTPROPERTYEX ( object_id(@tableName), N'TableFullTextMergeStatus') as INT);
		END
		print 'done waiting. Finished with TABLE: ' + @tableName; 
		print ''
		SET @cmd = (select top 1 COMMAND from @ToDo where done = 0);
		print 'NEXT COMMAND: ' + @cmd
		print ''
	END
END
SET NOCOUNT OFF
GO