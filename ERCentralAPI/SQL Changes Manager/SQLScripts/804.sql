USE [Reviewer]
GO

declare @chk int = (SELECT count(*)
		FROM sys.indexes 
		WHERE name='IX_TB_MAG_AUTO_UPDATE_RUN_PAPER_PaperId_RUN_ID' AND object_id = OBJECT_ID('[dbo].[TB_MAG_AUTO_UPDATE_RUN_PAPER]'))
If @chk = 0 
BEGIN
	print 'creating index in TB_MAG_AUTO_UPDATE_RUN_PAPER'
	CREATE NONCLUSTERED INDEX [IX_TB_MAG_AUTO_UPDATE_RUN_PAPER_PaperId_RUN_ID]
	ON [dbo].[TB_MAG_AUTO_UPDATE_RUN_PAPER] ([PaperId])
	INCLUDE ([MAG_AUTO_UPDATE_RUN_ID])
END
GO
declare @chk int = (SELECT count(*)
		FROM sys.indexes 
		WHERE name='IX_REPORT_COLUMN_CODE_SET_ID_REPORT_COLUMN_ID' AND object_id = OBJECT_ID('[dbo].[TB_REPORT_COLUMN_CODE]'))
If @chk = 0
BEGIN
	print 'creating index IX_REPORT_COLUMN_CODE_SET_ID_REPORT_COLUMN_ID'

	CREATE NONCLUSTERED INDEX [IX_REPORT_COLUMN_CODE_SET_ID_REPORT_COLUMN_ID]
	ON [dbo].[TB_REPORT_COLUMN_CODE] ([SET_ID])
	INCLUDE ([REPORT_COLUMN_ID])
END

GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbMapsList]    Script Date: 05/06/2026 09:25:38 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO



ALTER       PROCEDURE [dbo].[st_WebDbMapsList]
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

	, CASE when m.COLUMNS_PUBLIC_ATTRIBUTE_ID = 0 THEN 0
		WHEN (dbo.fn_IsAttributeInTree(a1.ATTRIBUTE_ID) = 1) then a1.ATTRIBUTE_ID
		else -1
	END as COLUMNS_ATTRIBUTE_ID
	, CASE when m.ROWS_PUBLIC_ATTRIBUTE_ID = 0 THEN 0
		WHEN (dbo.fn_IsAttributeInTree(a2.ATTRIBUTE_ID) = 1) then a2.ATTRIBUTE_ID
		else -1
	END as [ROWS_ATTRIBUTE_ID]
	, CASE when m.SEGMENTS_PUBLIC_ATTRIBUTE_ID = 0 THEN 0
		WHEN (dbo.fn_IsAttributeInTree(a3.ATTRIBUTE_ID) = 1) then a3.ATTRIBUTE_ID
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