USE [Reviewer]
GO

IF COL_LENGTH('dbo.TB_WEBDB', 'SHOW_WORLD_MAP') IS NULL
BEGIN
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON

COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo.TB_WEBDB ADD
	SHOW_WORLD_MAP int NOT NULL CONSTRAINT DF_TB_WEBDB_SHOW_WORLD_MAP DEFAULT 0,
	WORLD_MAP_SET_ID int NULL,
	WORLD_MAP_ATTRIBUTE_ID bigint NULL
ALTER TABLE dbo.TB_WEBDB SET (LOCK_ESCALATION = TABLE)
COMMIT

END
GO


USE [Reviewer]
GO

IF COL_LENGTH('dbo.TB_WEBDB', 'SHOW_WORLD_MAP') IS NULL
BEGIN
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON

COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo.TB_WEBDB_PUBLIC_ATTRIBUTE ADD
	WEBDB_COUNTRY_CODE nvarchar(2) NOT NULL CONSTRAINT DF_TB_WEBDB_PUBLIC_ATTRIBUTE_WEBDB_COUNTRY_CODE DEFAULT 0
ALTER TABLE dbo.TB_WEBDB_PUBLIC_ATTRIBUTE SET (LOCK_ESCALATION = TABLE)
COMMIT

END
GO





USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_WebDBget]    Script Date: 02/07/2021 12:51:15 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER procedure [dbo].[st_WebDBget]
(
	@WebDBid int,
	@RevId int	
)

As
BEGIN
select w.[WEBDB_ID]
		  ,[REVIEW_ID]
		  ,[WITH_ATTRIBUTE_ID]
		  ,[IS_OPEN]
		  ,w.[USERNAME]
		  ,[WEBDB_NAME]
		  ,SUBTITLE
		  ,w.[DESCRIPTION]
		  ,c1.CONTACT_NAME as [CREATED_BY]
		  ,c2.CONTACT_NAME as [EDITED_BY]
		  ,w.[MAP_TITLE]
		  ,w.[MAP_URL]
		  ,w.[HEADER_IMAGE_1_URL]
		  ,w.[HEADER_IMAGE_2_URL]
		  ,w.[HEADER_IMAGE_3_URL]
		  ,w.HEADER_IMAGE_1
		  ,w.HEADER_IMAGE_2
		  ,w.HEADER_IMAGE_3
		  ,w.SHOW_WORLD_MAP
		  ,w.WORLD_MAP_SET_ID
		  ,w.WORLD_MAP_ATTRIBUTE_ID
		  ,wpa.WEBDB_ATTRIBUTE_NAME as WORLD_MAP_ATTRIBUTE_NAME
	  FROM [TB_WEBDB] w
	  inner join TB_CONTACT c1 on w.CREATED_BY = c1.CONTACT_ID
	  inner join TB_CONTACT c2 on w.EDITED_BY = c2.CONTACT_ID 
	  left join TB_WEBDB_PUBLIC_ATTRIBUTE wpa on wpa.ATTRIBUTE_ID = w.WORLD_MAP_ATTRIBUTE_ID
	  where w.WEBDB_ID = @WebDBid AND w.REVIEW_ID = @RevId
END
GO

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_WebDbFrequencyCrosstabAndMap]    Script Date: 02/07/2021 12:53:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER   PROCEDURE [dbo].[st_WebDbFrequencyCrosstabAndMap]
	-- Add the parameters for the stored procedure here
	@attributeIdXAxis bigint 
	, @setIdXAxis int
	, @included bit null = null
	, @attributeIdYAxis bigint = 0
	, @setIdYAxis int = 0 
	, @segmentsParent bigint = 0
	, @setIdSegments int = 0
	, @onlyThisAttribute bigint = 0
	, @RevId int 
	, @WebDbId int 
AS
BEGIN

--declare 
	--@attributeIdXAxis bigint = 64472  --62475 0
	--, @setIdXAxis int = 644
	--, @attributeIdYAxis bigint = 0
	--, @setIdYAxis int = 0 --644 664
	--, @SegmentsParent bigint = 119121
	--, @setIdSegments int = 0--1880
	--, @OnlyThisAttribute bigint = 0
	--, @RevId int = 99
	--, @WebDbId int = 18


declare @items table (ItemId bigint primary key, X_atts varchar(max) null, Y_atts  varchar(max) null, segments varchar(max) null)
declare @attsX table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), COUNTRY_CODE nvarchar(2), ord int, done bit)
declare @attsY table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
declare @segments table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
--last minute table: the parent IDs and names are in @codeNames
declare @codeNames table (SETIDX_ID bigint primary key, SETIDX_NAME nvarchar(255), SETIDY_ID bigint, SETIDY_NAME nvarchar(255),
							ATTIBUTEIDX_ID bigint, ATTIBUTEIDX_NAME nvarchar(255), ATTIBUTEIDY_ID bigint, ATTIBUTEIDY_NAME nvarchar(255))

--sanity check, ensure @RevId and @WebDbId match...
Declare @CheckWebDbId int = null
set @CheckWebDbId = (select WEBDB_ID from TB_WEBDB where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
IF @CheckWebDbId is null return;

declare @WebDbFilter bigint = (select w.WITH_ATTRIBUTE_ID from TB_WEBDB w where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
if @WebDbFilter is not null and @WebDbFilter > 1
BEGIN
	if @OnlyThisAttribute > 0
	BEGIN
		insert into @items select distinct ir.item_id, null, null, null from TB_ITEM_REVIEW ir
			inner join tb_item_set tis on ir.ITEM_ID = tis.ITEM_ID and ir.REVIEW_ID = @RevId 
			and (@included is null OR ir.IS_INCLUDED = @included)
			and ir.IS_DELETED = 0 and tis.IS_COMPLETED = 1
			inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tia.ATTRIBUTE_ID = @WebDbFilter
			inner join TB_ITEM_ATTRIBUTE tia2 on tia2.ITEM_ID = ir.ITEM_ID and tia2.ATTRIBUTE_ID = @OnlyThisAttribute 
			inner join TB_ITEM_SET tis2 on tia2.ITEM_SET_ID = tis2.ITEM_SET_ID and tis2.IS_COMPLETED = 1 
	END
	ELSE
	Begin
		insert into @items select distinct ir.item_id, null, null, null from TB_ITEM_REVIEW ir
			inner join tb_item_set tis on ir.ITEM_ID = tis.ITEM_ID and ir.REVIEW_ID = @RevId 
			and (@included is null OR ir.IS_INCLUDED = @included) 
			and ir.IS_DELETED = 0 and tis.IS_COMPLETED = 1
			inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tia.ATTRIBUTE_ID = @WebDbFilter
	END
END
else
BEGIN
	if @OnlyThisAttribute > 0
	BEGIN
		insert into @items select distinct ir.item_id, null, null, null from TB_ITEM_REVIEW ir
			inner join TB_ITEM_ATTRIBUTE tia on ir.ITEM_ID = tia.ITEM_ID and tia.ATTRIBUTE_ID = @OnlyThisAttribute 
				and ir.REVIEW_ID = @RevId and ir.IS_DELETED = 0
				and (@included is null OR ir.IS_INCLUDED = @included)  
			inner join TB_ITEM_SET tis on tia.ITEM_SET_ID = tis.ITEM_SET_ID and IS_COMPLETED = 1
	END
	ELSE
	Begin
		insert into @items select distinct ir.item_id, null, null, null from TB_ITEM_REVIEW ir
			where ir.REVIEW_ID = @RevId and ir.IS_DELETED = 0
			 and (@included is null OR ir.IS_INCLUDED = @included) 
	END
END

insert into @attsX select distinct a.Attribute_id, 
	 CASE when pa.WEBDB_ATTRIBUTE_NAME is null then a.ATTRIBUTE_NAME
		else pa.WEBDB_ATTRIBUTE_NAME
	 END AS ATTRIBUTE_NAME
	 , WEBDB_COUNTRY_CODE, ATTRIBUTE_ORDER, 0 from TB_ATTRIBUTE_SET tas
	 inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	 inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa on a.ATTRIBUTE_ID = pa.ATTRIBUTE_ID and pa.WEBDB_ID = @WebDbId
	 where tas.SET_ID = @setIdXAxis and PARENT_ATTRIBUTE_ID = @attributeIdXAxis
select ATTRIBUTE_ID, ATTRIBUTE_NAME, COUNTRY_CODE from @attsX order by ord

IF @setIdYAxis > 0
BEGIN
	insert into @attsY select distinct a.Attribute_id, 
		 CASE When pa.WEBDB_ATTRIBUTE_NAME is null then a.ATTRIBUTE_NAME
			else pa.WEBDB_ATTRIBUTE_NAME
		 END as ATTRIBUTE_NAME
		 , ATTRIBUTE_ORDER, 0 from TB_ATTRIBUTE_SET tas
		 inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID 
		 inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa on a.ATTRIBUTE_ID = pa.ATTRIBUTE_ID and pa.WEBDB_ID = @WebDbId
		where SET_ID = @setIdYAxis and PARENT_ATTRIBUTE_ID = @attributeIdYAxis
	select ATTRIBUTE_ID, ATTRIBUTE_NAME from @attsY order by ord
END

-------------------------------------------------------
insert into @codeNames (SETIDX_ID, SETIDY_ID, ATTIBUTEIDX_ID, ATTIBUTEIDY_ID)
values (@setIdXAxis, @setIdYAxis, @attributeIdXAxis, @attributeIdYAxis)

update @codeNames set SETIDX_NAME = (
										CASE when ps.WEBDB_SET_NAME is null then s.SET_NAME
										else ps.WEBDB_SET_NAME
										END
									) 
	from TB_SET s
	inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @RevId and s.SET_ID = rs.SET_ID
	inner join TB_WEBDB_PUBLIC_SET ps on rs.REVIEW_SET_ID = ps.REVIEW_SET_ID and ps.WEBDB_ID = @WebDbId
	where s.SET_ID = SETIDX_ID

if @setIdYAxis != 0
begin
	update @codeNames set SETIDY_NAME = (
										CASE when ps.WEBDB_SET_NAME is null then s.SET_NAME
										else ps.WEBDB_SET_NAME
										END
									) 
	from TB_SET s
	inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @RevId and s.SET_ID = rs.SET_ID
	inner join TB_WEBDB_PUBLIC_SET ps on rs.REVIEW_SET_ID = ps.REVIEW_SET_ID and ps.WEBDB_ID = @WebDbId
	where s.SET_ID = SETIDY_ID
END
if @attributeIdXAxis != 0
begin
	update @codeNames set ATTIBUTEIDX_NAME = (
												CASE when pa.WEBDB_ATTRIBUTE_NAME is null then a.ATTRIBUTE_NAME
												ELSE pa.WEBDB_ATTRIBUTE_NAME
												END
											)
	from TB_ATTRIBUTE a
	inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa on a.ATTRIBUTE_ID = pa.ATTRIBUTE_ID and pa.WEBDB_ID = @WebDbId
	where a.ATTRIBUTE_ID = ATTIBUTEIDX_ID
end
if @attributeIdYAxis != 0
begin
	update @codeNames set ATTIBUTEIDY_NAME = (
												CASE when pa.WEBDB_ATTRIBUTE_NAME is null then a.ATTRIBUTE_NAME
												ELSE pa.WEBDB_ATTRIBUTE_NAME
												END
											)
	from TB_ATTRIBUTE a
	inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa on a.ATTRIBUTE_ID = pa.ATTRIBUTE_ID and pa.WEBDB_ID = @WebDbId
	where a.ATTRIBUTE_ID = ATTIBUTEIDY_ID
end
------------------------------------------------------------

If @SegmentsParent > 0
BEGIN
	insert into @segments select distinct a.Attribute_id,
		 CASE when pa.WEBDB_ATTRIBUTE_NAME is null then a.ATTRIBUTE_NAME
			else pa.WEBDB_ATTRIBUTE_NAME
		 END AS ATTRIBUTE_NAME
		 , ATTRIBUTE_ORDER, 0 from TB_ATTRIBUTE_SET tas
		 inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID 
		 inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa on a.ATTRIBUTE_ID = pa.ATTRIBUTE_ID and pa.WEBDB_ID = @WebDbId
		where SET_ID = @setIdSegments and PARENT_ATTRIBUTE_ID = @SegmentsParent
	select ATTRIBUTE_ID, ATTRIBUTE_NAME from @segments order by ord
END


update @items  set X_atts = Atts
from 
(
	select STRING_AGG(cast (ia.ATTRIBUTE_ID as nvarchar(max)), ',') as Atts, i.itemId as iid from @items i 
	inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdXAxis
	inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.ITEM_ID = ia.ITEM_ID
	inner join @attsX a on ia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
	group by ItemId 
	--order by ItemId
) as big
WHERE ItemId = Big.iid

if @setIdYAxis > 0
	update @items set Y_atts = Atts
	from 
	(
		select STRING_AGG(cast (ia.ATTRIBUTE_ID as nvarchar(max)), ',') as Atts, i.itemId as iid from @items i 
		inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdYAxis
		inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.ITEM_ID = ia.ITEM_ID
		inner join @attsY a on ia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
		group by ItemId 
		--order by ItemId
	) as big
	WHERE ItemId = Big.iid

if @setIdSegments > 0
update @items set segments = Atts
from 
(
	select STRING_AGG(cast (ia.ATTRIBUTE_ID as nvarchar(max)), ',') as Atts, i.itemId as iid from @items i 
	inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdSegments
	inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.ITEM_ID = ia.ITEM_ID
	inner join @segments a on ia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
	group by ItemId 
	--order by ItemId
) as big
WHERE ItemId = Big.iid

select * from @items

select * from @codeNames
END

GO




