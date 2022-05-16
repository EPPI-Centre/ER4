USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SourceDetails]    Script Date: 5/16/2022 4:43:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sergio
-- Create date: 29-06-09
-- Description:	Gets Sources from Review_ID
-- =============================================
ALTER PROCEDURE [dbo].[st_SourceDetails] 
	-- Add the parameters for the stored procedure here
	@revID int = 0,
	@sourceID int = 0
	with recompile
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	--declare @l1 nvarchar(200)
	--declare @l2 nvarchar(200)
	--declare @l3 nvarchar(200)
	--declare @l4 nvarchar(200)
	--declare @l5 nvarchar(200)
	--declare @l6 nvarchar(200)
	--declare @l7 nvarchar(200)
	
	--set @l1 = CONVERT(varchar, SYSDATETIME(), 121)
	declare @tt table
	(
		SOURCE_NAME nvarchar(255)
		,IS_DELETED bit
		,Source_ID int
		,DATE_OF_SEARCH date
		,DATE_OF_IMPORT date
		,SOURCE_DATABASE nvarchar(200)
		,SEARCH_DESCRIPTION nvarchar(4000)
		,SEARCH_STRING nvarchar(MAX)
		,NOTES nvarchar(4000)
		,IMPORT_FILTER nvarchar(60)
		,REVIEW_ID int
		
	)
	declare @t1 table
	(
		Source_ID int
		,[Total_Items] int NULL
		, [Deleted_Items] int NULL
	)
	declare @t2 table
	(
		Source_ID int
		,CODES int NULL
		,IDUCTIVE_CODES int NULL
	)
	declare @t3 table
	(
		Source_ID int
		,[Attached Files]  int NULL
		,OUTCOMES  int NULL
	)
	declare @t4 table
	(
		Source_ID int
		,DUPLICATES int NULL
		,isMasterOf  int NULL
	)

	insert into @tt
	(	
		SOURCE_NAME
		,[REVIEW_ID]
		,[Source_ID]
		,[IS_DELETED]
		,[DATE_OF_SEARCH]
		,[DATE_OF_IMPORT]
		,[SOURCE_DATABASE]
		,[SEARCH_DESCRIPTION]
		,[SEARCH_STRING]
		,[NOTES]
		,[IMPORT_FILTER]
	)
	SELECT SOURCE_NAME
		,[REVIEW_ID]
		,SOURCE_ID
		,[IS_DELETED]
		,[DATE_OF_SEARCH]
		,[DATE_OF_IMPORT]
		,[SOURCE_DATABASE]
		,[SEARCH_DESCRIPTION]
		,[SEARCH_STRING]
		,ts.[NOTES]
		,tif.IMPORT_FILTER_NAME
	from TB_SOURCE ts Left outer join TB_IMPORT_FILTER tif on ts.IMPORT_FILTER_ID = tif.IMPORT_FILTER_ID
	where REVIEW_ID = @revID and SOURCE_ID = @sourceID
	--set @l2 = CONVERT(varchar, SYSDATETIME(), 121)

	insert into @t1 
		SELECT tt.source_id 
		,COUNT(distinct(tis.ITEM_ID)) 'Total_Items'
		,sum(CASE WHEN ir.IS_DELETED = 1 then 1 else 0 END) 
		--, (SELECT COUNT(distinct(ttis.ITEM_ID)) from TB_ITEM_REVIEW ttir
		--		inner join TB_ITEM_SOURCE ttis on ttis.ITEM_ID = ttir.ITEM_ID
		--		where ttis.SOURCE_ID = tt.SOURCE_ID and ttir.IS_DELETED = 1) 'Deleted_Items'
		from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
		WHERE ir.REVIEW_ID = @revID
		group by tt.Source_ID
	--set @l3 = CONVERT(varchar, SYSDATETIME(), 121)
	Insert into @t2
	(Source_ID, CODES, IDUCTIVE_CODES)  (select source_id, 0 ,0 from @tt)
	update @t2   set CODES = sub.CODES--, IDUCTIVE_CODES = sub.IDUCTIVE_CODES
	from
		(
			SELECT 
			tt.source_id SSID
			,COUNT(distinct(ia.ITEM_ID)) CODES 
			--,COUNT(distinct(iat.ITEM_ATTRIBUTE_TEXT_ID)) IDUCTIVE_CODES
			--,COUNT(distinct(tid.ITEM_DOCUMENT_ID)) [Attached Files]
			--,COUNT(distinct(tio.OUTCOME_ID)) OUTCOMES  
			from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
				--inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
				inner join TB_REVIEW_SET rs on rs.REVIEW_ID = tt.REVIEW_ID and tt.REVIEW_ID = @revID
				inner join TB_ATTRIBUTE_SET tas on rs.SET_ID = tas.SET_ID
				inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_ID = ia.ITEM_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
				--left outer join TB_ITEM_ATTRIBUTE_TEXT iat on ia.ITEM_ATTRIBUTE_ID = iat.ITEM_ATTRIBUTE_ID
				--left outer join TB_ITEM_DOCUMENT tid on tid.ITEM_ID = tis.ITEM_ID
				--left outer join TB_ITEM_SET tes on tis.ITEM_ID = tes.ITEM_ID 
				--left outer join TB_ITEM_OUTCOME tio on tio.ITEM_SET_ID = tes.ITEM_SET_ID 
			--WHERE ir.REVIEW_ID = @revID
			
			group by tt.Source_ID
			
		) sub
		where sub.SSID = Source_ID
		--OPTION (OPTIMIZE FOR UNKNOWN)
	--set @l4 = CONVERT(varchar, SYSDATETIME(), 121)
	Insert into @t3
		SELECT tt.source_id
		--,COUNT(distinct(ia.ITEM_ATTRIBUTE_ID)) CODES 
		--,COUNT(distinct(iat.ITEM_ATTRIBUTE_TEXT_ID)) IDUCTIVE_CODES
		,COUNT(distinct(tid.ITEM_DOCUMENT_ID)) [Attached Files]
		,COUNT(distinct(tio.OUTCOME_ID)) OUTCOMES  
		from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
			--left outer join TB_REVIEW_SET rs on rs.REVIEW_ID = tt.REVIEW_ID
			--left outer join TB_ATTRIBUTE_SET tas on rs.SET_ID = tas.SET_ID
			--left outer join TB_ITEM_ATTRIBUTE ia on tis.ITEM_ID = ia.ITEM_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
			--left outer join TB_ITEM_ATTRIBUTE_TEXT iat on ia.ITEM_ATTRIBUTE_ID = iat.ITEM_ATTRIBUTE_ID
			left outer join TB_ITEM_DOCUMENT tid on tid.ITEM_ID = tis.ITEM_ID
			left outer join TB_ITEM_SET tes on tis.ITEM_ID = tes.ITEM_ID 
			left outer join TB_ITEM_OUTCOME tio on tio.ITEM_SET_ID = tes.ITEM_SET_ID 
		WHERE ir.REVIEW_ID = @revID
		group by tt.Source_ID
	--set @l5 = CONVERT(varchar, SYSDATETIME(), 121)
	Insert into @t4
		SELECT
		tt.source_id
		--,(COUNT(distinct(dup.ITEM_DUPLICATES_ID)) + COUNT(distinct(dup2.ITEM_DUPLICATES_ID))) DUPLICATES
		--,COUNT(distinct(ir2.ITEM_REVIEW_ID)) isMasterOf
		,sum(CASE WHEN (
				ir.IS_DELETED = 1 and ir.is_included = 1 AND ir.MASTER_ITEM_ID is NOT null
			) then 1 else 0 END) as 'Duplicates'
		,0
		from 
			@tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.Item_ID
			--left outer join TB_ITEM_REVIEW ir2 on ir2.MASTER_ITEM_ID = ir.ITEM_ID and ir2.REVIEW_ID = @revID
		WHERE ir.REVIEW_ID = @revID
		
		group by tt.Source_ID

	update @t4 set isMasterOf = a.c
		from
		(Select COUNT (distinct ir2.item_id) as c from @tt tt	inner join tb_item_source tis on tt.source_id = tis.source_id
			inner join tb_item_review ir on tis.Item_ID = ir.MASTER_ITEM_ID
			inner join TB_ITEM_REVIEW ir2 on ir.MASTER_ITEM_ID = ir2.ITEM_ID and ir2.REVIEW_ID = @revID
			) a
	--set @l6 = CONVERT(varchar, SYSDATETIME(), 121)
	select 
		SOURCE_NAME
		,[Total_Items]
		,[Deleted_Items]
		,IS_DELETED
		,t1.Source_ID
		,DATE_OF_SEARCH
		,DATE_OF_IMPORT
		,SOURCE_DATABASE
		,SEARCH_DESCRIPTION
		,SEARCH_STRING
		,NOTES
		,IMPORT_FILTER
		,REVIEW_ID
		,CODES
		,IDUCTIVE_CODES
		,[Attached Files]
		,DUPLICATES
		,isMasterOf
		,OUTCOMES
	from @tt tt
	inner join @t1 t1 on tt.Source_ID = t1.Source_ID
	inner join @t2 t2 on tt.Source_ID = t2.Source_ID
	inner join @t3 t3 on tt.Source_ID = t3.Source_ID
	inner join @t4 t4 on tt.Source_ID = t4.Source_ID
	order by Source_ID
	--set @l7 = CONVERT(varchar, SYSDATETIME(), 121)
	--select @l1, @l2, @l3, @l4, @l5, @l6, @l7
END
GO