USE [Reviewer]
GO

IF COL_LENGTH('dbo.TB_WEBDB_MAP', 'MAP_ORDER') IS NULL
BEGIN
	ALTER TABLE [dbo].TB_WEBDB_MAP ADD 
	MAP_ORDER int NULL
END
GO

--------------------------------------------------


USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_WebDbMapsList]    Script Date: 04/06/2026 13:49:52 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER     PROCEDURE [dbo].[st_WebDbMapsList]
(
	@REVIEW_ID INT,
	@WEBDB_ID int
)
As
--first, check that all parameters match up...
declare @check int = (select WEBDB_ID from TB_WEBDB w
						where w.REVIEW_ID = @REVIEW_ID and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return
--OK, all checks match up, phew


-- added by JB 02/06/2026
-- We have added an ORDER_NUMBER field to the maps
-- when we grab old visualisations the maps will have NULL to start so we want to give them values.
-- this is a one-off operation
declare @numberOfNulls int = 0
declare @tb_order_check table (tv_id int identity(1,1)primary key, tv_webdb_map_id int, tv_orig_map_order int, tv_new_map_order int)
insert into @tb_order_check (tv_webdb_map_id, tv_orig_map_order)
select WEBDB_MAP_ID, MAP_ORDER from TB_WEBDB_MAP where WEBDB_ID = @WEBDB_ID

set @numberOfNulls = (select count(*) from @tb_order_check where tv_orig_map_order is null)
if @numberOfNulls > 0 -- we have nulls so we need to give all of the maps in this visualisation a value
begin
	UPDATE TB_WEBDB_MAP
	SET TB_WEBDB_MAP.MAP_ORDER = tv_id
	FROM TB_WEBDB_MAP
	JOIN @tb_order_check ON TB_WEBDB_MAP.WEBDB_MAP_ID = tv_webdb_map_id;
end

-- all maps in this visualisation now have a MAP_ORDER so carry on...

select m.*, s1.SET_ID as [COLUMNS_SET_ID], s2.SET_ID as [ROWS_SET_ID], s3.SET_ID as [SEGMENTS_SET_ID], m.MAP_ORDER
	--, a1.ATTRIBUTE_ID as [COLUMNS_ATTRIBUTE_ID], a2.ATTRIBUTE_ID as [ROWS_ATTRIBUTE_ID], a3.ATTRIBUTE_ID as [SEGMENTS_ATTRIBUTE_ID]

	, CASE when (dbo.fn_IsAttributeInTree(a1.ATTRIBUTE_ID) = 1) then a1.ATTRIBUTE_ID
		else -1
	END as COLUMNS_ATTRIBUTE_ID
	, CASE when (dbo.fn_IsAttributeInTree(a2.ATTRIBUTE_ID) = 1) then a2.ATTRIBUTE_ID
		else -1
	END as [ROWS_ATTRIBUTE_ID]
	, CASE when (dbo.fn_IsAttributeInTree(a3.ATTRIBUTE_ID) = 1) then a3.ATTRIBUTE_ID
		else -1
	END as [SEGMENTS_ATTRIBUTE_ID]

	, CASE when (ps1.WEBDB_SET_NAME = '' OR ps1.WEBDB_SET_NAME is null) then s1.SET_NAME
		else ps1.WEBDB_SET_NAME
	END as COLUMNS_SET_NAME
	, CASE when (ps2.WEBDB_SET_NAME = '' OR ps2.WEBDB_SET_NAME is null) then s2.SET_NAME
		else ps2.WEBDB_SET_NAME
	END as ROWS_SET_NAME
	, CASE when (ps3.WEBDB_SET_NAME = '' OR ps3.WEBDB_SET_NAME is null) then s3.SET_NAME
		else ps3.WEBDB_SET_NAME
	END as SEGMENTS_SET_NAME

	, CASE when ((pa1.WEBDB_ATTRIBUTE_NAME = '' OR pa1.WEBDB_ATTRIBUTE_NAME is null) AND m.COLUMNS_PUBLIC_ATTRIBUTE_ID > 0) then a1.ATTRIBUTE_NAME
		WHEN (pa1.WEBDB_ATTRIBUTE_NAME is null and m.COLUMNS_PUBLIC_ATTRIBUTE_ID = 0) then ''
		else pa1.WEBDB_ATTRIBUTE_NAME
	END as COLUMNS_ATTRIBUTE_NAME
	, CASE when ((pa2.WEBDB_ATTRIBUTE_NAME = '' OR pa2.WEBDB_ATTRIBUTE_NAME is null )AND m.ROWS_PUBLIC_ATTRIBUTE_ID > 0) then a2.ATTRIBUTE_NAME
		WHEN (pa2.WEBDB_ATTRIBUTE_NAME is null and m.ROWS_PUBLIC_ATTRIBUTE_ID = 0) then ''
		else pa2.WEBDB_ATTRIBUTE_NAME
	END as ROWS_ATTRIBUTE_NAME
	, CASE when ((pa3.WEBDB_ATTRIBUTE_NAME = '' OR pa3.WEBDB_ATTRIBUTE_NAME is null) AND m.SEGMENTS_PUBLIC_ATTRIBUTE_ID > 0) then a3.ATTRIBUTE_NAME
		WHEN (pa3.WEBDB_ATTRIBUTE_NAME is null and m.SEGMENTS_PUBLIC_ATTRIBUTE_ID = 0) then ''
		else pa3.WEBDB_ATTRIBUTE_NAME
	END as SEGMENTS_ATTRIBUTE_NAME


	from TB_WEBDB_MAP m
	inner join TB_WEBDB_PUBLIC_SET ps1 on m.COLUMNS_PUBLIC_SET_ID = ps1.WEBDB_PUBLIC_SET_ID and ps1.WEBDB_ID = m.WEBDB_ID
	inner join TB_REVIEW_SET rs1 on ps1.REVIEW_SET_ID = rs1.REVIEW_SET_ID and rs1.REVIEW_ID = @REVIEW_ID
	inner join tb_set s1 on rs1.SET_ID = s1.SET_ID
	left join TB_WEBDB_PUBLIC_ATTRIBUTE pa1 on m.COLUMNS_PUBLIC_ATTRIBUTE_ID = pa1.WEBDB_PUBLIC_ATTRIBUTE_ID
	left join TB_ATTRIBUTE a1 on pa1.ATTRIBUTE_ID = a1.ATTRIBUTE_ID

	inner join TB_WEBDB_PUBLIC_SET ps2 on m.ROWS_PUBLIC_SET_ID = ps2.WEBDB_PUBLIC_SET_ID and ps2.WEBDB_ID = m.WEBDB_ID
	inner join TB_REVIEW_SET rs2 on ps2.REVIEW_SET_ID = rs2.REVIEW_SET_ID and rs2.REVIEW_ID = @REVIEW_ID
	inner join tb_set s2 on rs2.SET_ID = s2.SET_ID
	left join TB_WEBDB_PUBLIC_ATTRIBUTE pa2 on m.ROWS_PUBLIC_ATTRIBUTE_ID = pa2.WEBDB_PUBLIC_ATTRIBUTE_ID
	left join TB_ATTRIBUTE a2 on pa2.ATTRIBUTE_ID = a2.ATTRIBUTE_ID

	inner join TB_WEBDB_PUBLIC_SET ps3 on m.SEGMENTS_PUBLIC_SET_ID = ps3.WEBDB_PUBLIC_SET_ID and ps3.WEBDB_ID = m.WEBDB_ID
	inner join TB_REVIEW_SET rs3 on ps3.REVIEW_SET_ID = rs3.REVIEW_SET_ID and rs3.REVIEW_ID = @REVIEW_ID
	inner join tb_set s3 on rs3.SET_ID = s3.SET_ID
	left join TB_WEBDB_PUBLIC_ATTRIBUTE pa3 on m.SEGMENTS_PUBLIC_ATTRIBUTE_ID = pa3.WEBDB_PUBLIC_ATTRIBUTE_ID
	left join TB_ATTRIBUTE a3 on pa3.ATTRIBUTE_ID = a3.ATTRIBUTE_ID
    where m.WEBDB_ID = @WEBDB_ID
	order by m.MAP_ORDER

GO

-------------------------------

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_WebDbMapEdit]    Script Date: 04/06/2026 13:50:42 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER   PROCEDURE [dbo].[st_WebDbMapEdit]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@WEBDB_MAP_ID int,
	@ColumnsSetID int,
	@ColumnsAttributeID bigint,
	@RowsSetID int,
	@RowsAttributeID bigint,
	@SegmentsSetID int,
	@SegmentsAttributeID bigint,
	@MapName nvarchar(1000),
	@MapDescription nvarchar(max),
	@MapOrder int
)
As
--first, check that all parameters match up and get the value we need...
declare @ColumnsPublicSetID int = (select rps.WEBDB_PUBLIC_SET_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rps 
							on  w.REVIEW_ID = @REVIEW_ID and rps.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID
						inner join TB_REVIEW_SET rs on rps.REVIEW_SET_ID = rs.REVIEW_SET_ID and rs.REVIEW_ID = w.REVIEW_ID and rs.SET_ID = @ColumnsSetID
						)
IF @ColumnsPublicSetID is null OR @ColumnsPublicSetID < 1 return

declare @RowsPublicSetID int = (select rps.WEBDB_PUBLIC_SET_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rps 
							on  w.REVIEW_ID = @REVIEW_ID and rps.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID
						inner join TB_REVIEW_SET rs on rps.REVIEW_SET_ID = rs.REVIEW_SET_ID and rs.REVIEW_ID = w.REVIEW_ID and rs.SET_ID = @RowsSetID
						)
IF @RowsPublicSetID is null OR @RowsPublicSetID < 1 return

declare @SegmentsPublicSetID int = (select rps.WEBDB_PUBLIC_SET_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rps 
							on  w.REVIEW_ID = @REVIEW_ID and rps.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID
						inner join TB_REVIEW_SET rs on rps.REVIEW_SET_ID = rs.REVIEW_SET_ID and rs.REVIEW_ID = w.REVIEW_ID and rs.SET_ID = @SegmentsSetID
						)
IF @SegmentsPublicSetID is null OR @SegmentsPublicSetID < 1 return

declare @ColumnsPublicAttributeID int
IF @ColumnsAttributeID > 0
begin 
	set @ColumnsPublicAttributeID = (select pa.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa 
							on w.REVIEW_ID = @REVIEW_ID and pa.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						inner join TB_ATTRIBUTE a on pa.ATTRIBUTE_ID = a.ATTRIBUTE_ID and pa.ATTRIBUTE_ID = @ColumnsAttributeID
						)
	IF @ColumnsPublicAttributeID is null OR @ColumnsPublicAttributeID < 1 return
end
else SET @ColumnsPublicAttributeID = 0


declare @RowsPublicAttributeID int
IF @RowsAttributeID > 0
BEGIN
	set @RowsPublicAttributeID = (select pa.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa 
							on w.REVIEW_ID = @REVIEW_ID and pa.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						inner join TB_ATTRIBUTE a on pa.ATTRIBUTE_ID = a.ATTRIBUTE_ID and pa.ATTRIBUTE_ID = @RowsAttributeID
						)
	IF @RowsPublicAttributeID is null OR @RowsPublicAttributeID < 1 return
END
else SET @RowsPublicAttributeID = 0

declare @SegmentsPublicAttributeID int
IF @SegmentsAttributeID > 0
BEGIN
	set @SegmentsPublicAttributeID = (select pa.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa 
							on w.REVIEW_ID = @REVIEW_ID and pa.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						inner join TB_ATTRIBUTE a on pa.ATTRIBUTE_ID = a.ATTRIBUTE_ID and pa.ATTRIBUTE_ID = @SegmentsAttributeID
						)
	IF @SegmentsPublicAttributeID is null OR @SegmentsPublicAttributeID < 1 return
END
else SET @SegmentsPublicAttributeID = 0
--OK, all checks match up, phew



-- update everything except the MAP_ORDER field
UPDATE TB_WEBDB_MAP set 
           [COLUMNS_PUBLIC_ATTRIBUTE_ID] =  @ColumnsPublicAttributeID
           ,[COLUMNS_PUBLIC_SET_ID] = @ColumnsPublicSetID
           ,[ROWS_PUBLIC_ATTRIBUTE_ID] = @RowsPublicAttributeID
           ,[ROWS_PUBLIC_SET_ID] = @RowsPublicSetID
           ,[SEGMENTS_PUBLIC_ATTRIBUTE_ID] = @SegmentsPublicAttributeID
           ,[SEGMENTS_PUBLIC_SET_ID] = @SegmentsPublicSetID
           ,[MAP_NAME] = @MapName
           ,[MAP_DESCRIPTION] = @MapDescription
     where WEBDB_MAP_ID = @WEBDB_MAP_ID and WEBDB_ID = @WEBDB_ID

-- finally, check if this was a reorder
-- if it was, then we need to adjust the ORDER_NUMBER for all of the maps in this WebDB
declare @originalOrdernumber int = (select MAP_ORDER from TB_WEBDB_MAP where WEBDB_MAP_ID = @WEBDB_MAP_ID)
if @originalOrdernumber != @MapOrder
begin
	if @originalOrdernumber > @MapOrder 
	begin		
		-- this is a move up so flip order with the map above it.
		update TB_WEBDB_MAP set MAP_ORDER = @originalOrdernumber where MAP_ORDER = @MapOrder and WEBDB_ID = @WEBDB_ID
		update TB_WEBDB_MAP set MAP_ORDER = @MapOrder where WEBDB_MAP_ID = @WEBDB_MAP_ID
	end
	else 
	begin
		-- this is a move down so flip order with the map below it.
		update TB_WEBDB_MAP set MAP_ORDER = @originalOrdernumber where MAP_ORDER = @MapOrder and WEBDB_ID = @WEBDB_ID
		update TB_WEBDB_MAP set MAP_ORDER = @MapOrder where WEBDB_MAP_ID = @WEBDB_MAP_ID
	end
end





GO

------------------------------------


USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_WebDbMapAdd]    Script Date: 04/06/2026 13:51:20 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER   PROCEDURE [dbo].[st_WebDbMapAdd]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@ColumnsSetID int,
	@ColumnsAttributeID bigint,
	@RowsSetID int,
	@RowsAttributeID bigint,
	@SegmentsSetID int,
	@SegmentsAttributeID bigint,
	@MapName nvarchar(1000),
	@MapDescription nvarchar(max),
	@WEBDB_MAP_ID int output
)
As
--first, check that all parameters match up and get the value we need...
declare @ColumnsPublicSetID int = (select rps.WEBDB_PUBLIC_SET_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rps 
							on  w.REVIEW_ID = @REVIEW_ID and rps.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID
						inner join TB_REVIEW_SET rs on rps.REVIEW_SET_ID = rs.REVIEW_SET_ID and rs.REVIEW_ID = w.REVIEW_ID and rs.SET_ID = @ColumnsSetID
						)
IF @ColumnsPublicSetID is null OR @ColumnsPublicSetID < 1 return

declare @RowsPublicSetID int = (select rps.WEBDB_PUBLIC_SET_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rps 
							on  w.REVIEW_ID = @REVIEW_ID and rps.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID
						inner join TB_REVIEW_SET rs on rps.REVIEW_SET_ID = rs.REVIEW_SET_ID and rs.REVIEW_ID = w.REVIEW_ID and rs.SET_ID = @RowsSetID
						)
IF @RowsPublicSetID is null OR @RowsPublicSetID < 1 return

declare @SegmentsPublicSetID int = (select rps.WEBDB_PUBLIC_SET_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rps 
							on  w.REVIEW_ID = @REVIEW_ID and rps.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID
						inner join TB_REVIEW_SET rs on rps.REVIEW_SET_ID = rs.REVIEW_SET_ID and rs.REVIEW_ID = w.REVIEW_ID and rs.SET_ID = @SegmentsSetID
						)
IF @SegmentsPublicSetID is null OR @SegmentsPublicSetID < 1 return

declare @ColumnsPublicAttributeID int
IF @ColumnsAttributeID > 0
begin 
	set @ColumnsPublicAttributeID = (select pa.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa 
							on w.REVIEW_ID = @REVIEW_ID and pa.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						inner join TB_ATTRIBUTE a on pa.ATTRIBUTE_ID = a.ATTRIBUTE_ID and pa.ATTRIBUTE_ID = @ColumnsAttributeID
						)
	IF @ColumnsPublicAttributeID is null OR @ColumnsPublicAttributeID < 1 return
end
else SET @ColumnsPublicAttributeID = 0


declare @RowsPublicAttributeID int
IF @RowsAttributeID > 0
BEGIN
	set @RowsPublicAttributeID = (select pa.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa 
							on w.REVIEW_ID = @REVIEW_ID and pa.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						inner join TB_ATTRIBUTE a on pa.ATTRIBUTE_ID = a.ATTRIBUTE_ID and pa.ATTRIBUTE_ID = @RowsAttributeID
						)
	IF @RowsPublicAttributeID is null OR @RowsPublicAttributeID < 1 return
END
else SET @RowsPublicAttributeID = 0

declare @SegmentsPublicAttributeID int
IF @SegmentsAttributeID > 0
BEGIN
	set @SegmentsPublicAttributeID = (select pa.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa 
							on w.REVIEW_ID = @REVIEW_ID and pa.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						inner join TB_ATTRIBUTE a on pa.ATTRIBUTE_ID = a.ATTRIBUTE_ID and pa.ATTRIBUTE_ID = @SegmentsAttributeID
						)
	IF @SegmentsPublicAttributeID is null OR @SegmentsPublicAttributeID < 1 return
END
else SET @SegmentsPublicAttributeID = 0
--OK, all checks match up, phew

-- put map at bottom of list of maps
declare @last_map_position int
set @last_map_position = (select top 1 MAP_ORDER from TB_WEBDB_MAP where WEBDB_ID = @WEBDB_ID order by MAP_ORDER desc)
If @last_map_position is null
begin
	-- if this is the first map in the visualisation
	set @last_map_position = 0
end

INSERT INTO TB_WEBDB_MAP
           ([WEBDB_ID]
           ,[COLUMNS_PUBLIC_ATTRIBUTE_ID]
           ,[COLUMNS_PUBLIC_SET_ID]
           ,[ROWS_PUBLIC_ATTRIBUTE_ID]
           ,[ROWS_PUBLIC_SET_ID]
           ,[SEGMENTS_PUBLIC_ATTRIBUTE_ID]
           ,[SEGMENTS_PUBLIC_SET_ID]
           ,[MAP_NAME]
           ,[MAP_DESCRIPTION]
		   ,[MAP_ORDER])
     VALUES
           (@WEBDB_ID
           , @ColumnsPublicAttributeID
           , @ColumnsPublicSetID
           , @RowsPublicAttributeID
           , @RowsPublicSetID
           , @SegmentsPublicAttributeID
           , @SegmentsPublicSetID
           , @MapName
           , @MapDescription
		   , @last_map_position + 1)

set @WEBDB_MAP_ID = SCOPE_IDENTITY()

GO

-------------------------------

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_WebDbMapDelete]    Script Date: 04/06/2026 13:52:20 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER   PROCEDURE [dbo].[st_WebDbMapDelete]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@WEBDB_MAP_ID int
)
As
--first, check that all parameters match up...
declare @check int = (select m.WEBDB_MAP_ID from TB_WEBDB w
						inner join TB_WEBDB_MAP m
							on m.WEBDB_MAP_ID = @WEBDB_MAP_ID and w.REVIEW_ID = @REVIEW_ID and m.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return
--OK, all checks match up, phew


declare @deletedMapOrder int = (select MAP_ORDER from TB_WEBDB_MAP where WEBDB_MAP_ID = @WEBDB_MAP_ID)
declare @numberOfMaps int = (select count(*) from TB_WEBDB_MAP where WEBDB_ID = @WEBDB_ID)

delete from TB_WEBDB_MAP
     where WEBDB_MAP_ID = @WEBDB_MAP_ID and WEBDB_ID = @WEBDB_ID

-- update the MAP_ORDER
if (@deletedMapOrder != @numberOfMaps) -- nothing to do if it is the last map
begin
	-- update ORDER_NUMBER for all of the maps from @deletedMapOrder+1 to the end
	declare @tb_maps_to_adjust table (tv_webdb_map_id int, tv_old_order_number int, tv_new_order_number int)
	insert into @tb_maps_to_adjust (tv_webdb_map_id, tv_old_order_number)
	select WEBDB_MAP_ID, MAP_ORDER from TB_WEBDB_MAP where WEBDB_ID = @WEBDB_ID and MAP_ORDER > @deletedMapOrder order by MAP_ORDER
	
	update @tb_maps_to_adjust set tv_new_order_number = tv_old_order_number - 1

	update TB_WEBDB_MAP 
	set MAP_ORDER = tv_new_order_number 
	FROM @tb_maps_to_adjust 
	WHERE tv_webdb_map_id = WEBDB_MAP_ID
end


GO

