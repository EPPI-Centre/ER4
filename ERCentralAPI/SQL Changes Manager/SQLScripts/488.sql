USE [Reviewer]
GO
If IndexProperty(Object_Id('[dbo].[tb_COMPARISON_ITEM_ATTRIBUTE]'), 'IX_COMPARISON_ITEM_ATTRIBUTE_ITEM_ID', 'IndexID') Is Null
begin
	CREATE NONCLUSTERED INDEX [IX_COMPARISON_ITEM_ATTRIBUTE_ITEM_ID]
	ON [dbo].[tb_COMPARISON_ITEM_ATTRIBUTE] ([ITEM_ID])
END
If IndexProperty(Object_Id('[dbo].[TB_ITEM_ARM]'), 'IX_ITEM_ARM_ITEM_ID', 'IndexID') Is Null
begin
	CREATE NONCLUSTERED INDEX [IX_ITEM_ARM_ITEM_ID]
	ON [dbo].[TB_ITEM_ARM] ([ITEM_ID])
END
If IndexProperty(Object_Id('[dbo].[TB_ITEM_ATTRIBUTE_TEXT]'), 'IX_ITEM_ATTRIBUTE_TEXT_ITEM_ATTRIBUTE_ID', 'IndexID') Is Null
begin
	CREATE NONCLUSTERED INDEX [IX_ITEM_ATTRIBUTE_TEXT_ITEM_ATTRIBUTE_ID]
	ON [dbo].[TB_ITEM_ATTRIBUTE_TEXT] ([ITEM_ATTRIBUTE_ID])
END
If IndexProperty(Object_Id('[dbo].[TB_ITEM_LINK]'), 'IX_ITEM_LINK_ITEM_ID_SECONDARY', 'IndexID') Is Null
begin
	CREATE NONCLUSTERED INDEX [IX_ITEM_LINK_ITEM_ID_SECONDARY]
	ON [dbo].[TB_ITEM_LINK] ([ITEM_ID_SECONDARY])
END
If IndexProperty(Object_Id('[dbo].[TB_ITEM_DUPLICATES]'), 'IX_ITEM_DUPLICATES_ITEM_ID_OUT', 'IndexID') Is Null
begin
	CREATE NONCLUSTERED INDEX [IX_ITEM_DUPLICATES_ITEM_ID_OUT]
	ON [dbo].[TB_ITEM_DUPLICATES] ([ITEM_ID_OUT])
END
--If IndexProperty(Object_Id('[dbo].[TB_ITEM_TERM]'), 'IX_ITEM_TERM_ITEM_ID', 'IndexID') Is Null
--begin
----CREATE NONCLUSTERED INDEX [IX_ITEM_TERM_ITEM_ID]
----ON [dbo].[TB_ITEM_TERM] ([ITEM_ID])
--END
If IndexProperty(Object_Id('[dbo].[tb_REVIEW_JOB]'), 'IX_REVIEW_JOB_REVIEW_ID', 'IndexID') Is Null
begin
	CREATE NONCLUSTERED INDEX [IX_REVIEW_JOB_REVIEW_ID]
	ON [dbo].[tb_REVIEW_JOB] ([REVIEW_ID])
END

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SourceDeleteForever]    Script Date: 27/04/2022 09:05:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sergio
-- Create date: 20/7/09 -update May 2022
-- Description:	(Un/)Delete a source and all its Items
-- =============================================
ALTER PROCEDURE [dbo].[st_SourceDeleteForever] 
	-- Add the parameters for the stored procedure here
	@srcID int = 0,
	@revID int,
	@contactID int,
	@result int = 0 output 
AS
BEGIN
	
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	--declare @start datetime = getdate()
	
	declare @JobId int
	--FIRST BLOCK: we can call this SP for 2 reasons:
	--(A) to try deleting a source, which will happen ONLY if there isn't an older deletion pending
	-- (A.1) if no older deletion is pending, we trigger the new job
	-- (A.2) if an older deletion is pending, but has been active in the last 10m, we return without doing nothing (it might still be active)
	-- (A.3) if an older deletion is pending, but has NOT been active in the last 10m, we resume the older job, overwriting the @srcID supplied as input
	--(B) to check if an older JOB is pending - it was triggered but didn't finish within the 10m timeout:
	-- (B.1) if so, it should be resumed (in all cases).
	-- (B.2) otherwise end here: nothing to do.
	--Thus, we first check in tb_REVIEW_JOB
	--@JobId becomes not null if there is an active deletion for this review

	BEGIN TRANSACTION PRE
	--see https://weblogs.sqlteam.com/dang/2007/10/28/conditional-insertupdate-race-condition/
	--for details on how this is dealing with concurrency problems. Aim is to avoid having two instances of this SP insert
	--SP is called whenever the list of duplicates is retrieved AND when we are asking to find new duplicates...
	
	--paired with the "lasting lock", the transaction prevents two instances to be triggered concurrently
	--without the lasting lock, the TRAN itself won't work, see link above for the details.
	--WHILE running a deletion job holds the source_id in the "SUCCESS" field (it's an INT), but as a negative int, because 1 means "Success".
	set @JobId = (select top 1 REVIEW_JOB_ID from tb_REVIEW_JOB WITH (TABLOCKX, HOLDLOCK)
				where REVIEW_ID = @revID AND JOB_TYPE = 'delete source' and CURRENT_STATE = 'running' order by REVIEW_JOB_ID desc)
	IF @JobId is not null --cases (B.1, B.2, A.2 or A.3)
	BEGIN
		--2 possibilities: (1) last activity was more than 10m ago, it has TIMED OUT, so we'll resume it 
		--(2) last activity (END_TIME) was less than 10m ago, we assume it is still working and do nothing.
		if (select END_TIME from tb_REVIEW_JOB where REVIEW_JOB_ID = @JobId) < DATEADD(minute, -10, getdate()) --CASES A.3 or B.1
		BEGIN
			update tb_REVIEW_JOB set END_TIME = GETDATE() where REVIEW_JOB_ID = @JobId --if this SP is triggered again within 10m, check above will NOT come in here.
			if (@srcID > 0) --A.3
			BEGIN 
				set @result = -2 --overriding @srcID supplied, we'll resume the deletion that never finished.
			END
			--cases A.3 and B.1 the @srcID we'll work on is the one for the pre-existing job that we need to resume
			set @srcID = (SELECT SUCCESS * -1 from tb_REVIEW_JOB where REVIEW_JOB_ID = @JobId) 
			
		END
		ELSE
		BEGIN
			set @result = -1 --OLDER deletion JOB is still running
			COMMIT TRANSACTION PRE --can't return before we commit the transaction
			return
		END
	END
	IF @srcID = 0
	BEGIN
		--case B.2
		--means that this was triggered ONLY to check if a job is already running, and that we did not find a job to resume, so we should stop here
		set @result = 0 --nothing happening
		COMMIT TRANSACTION PRE --can't return before we commit the transaction
		return
	END

	--ALL cases in which we should NOT start/resume a deletion have "returned" already, if we reach the following it's because we have something to do.
	--TO stay safe, IF @srcID = 0, we don't know what to do and end here
	IF @srcID < 1 OR @srcID is null
	BEGIN
		set @result = -10 --unspecified error, should not happen
		COMMIT TRANSACTION PRE --can't return before we commit the transaction
		return
	END
	--LAST check: make sure the source belongs to review...
	declare @check int = 0
	set @check = (select count(source_id) from TB_SOURCE where SOURCE_ID = @srcID and REVIEW_ID = @revID and IS_DELETED = 1)
	if (@check != 1) 
	BEGIN
		set @result = -11 --another unspecified error, should not happen, most likely, this source wasn't already marked as deleted.
		COMMIT TRANSACTION PRE --can't return before we commit the transaction
		return
	END

	--IF user supplied a @srcID and no older job needed resuming (we got the @srcID already), we need to create a new record in TB_REVIEW_JOB and get the @JobId
	IF @JobId is null OR @JobId = 0
	BEGIN
	--we put "Source_id * -1" in the "Success" field, as it's of Int type (tells us what source is being deleted).
		insert into tb_REVIEW_JOB select @revID, @contactID, GETDATE(), GETDATE(), 'delete source', 'running', @srcID * -1, ''
		set @JobId = SCOPE_IDENTITY()
	END

	COMMIT TRANSACTION PRE --we've updated or inserted into tb_REVIEW_JOB, we can release the lock

	
	--IF we got all the way to here, we have work to do, at last!!

	Declare @tt TABLE
	(
		item_ID bigint PRIMARY KEY
	)
	declare @bsize int = 400 --max size of batch we'll delete
	declare @actualbsize int = @bsize --actual size of batch we'll delete
	declare @counter int = 0
	declare @delay nchar(8) = '00:00:04'--length of the pause after each batch

	

	--select DATEDIFF(millisecond, @start, getdate()) as 'prep'
	--set @start = GETDATE()

	while (@actualbsize > 0 AND @counter < 100000)
	BEGIN
		BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
			BEGIN TRANSACTION
				insert into @tt --First: get the ITEM_IDs we'll deal with, excluding those that appear in more than one review
				SELECT top (@bsize) ITEM_ID FROM
					(select ir.ITEM_ID, COUNT(ir.item_id) cnt from TB_ITEM_REVIEW ir 
						inner join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
						where tis.SOURCE_ID = @srcID -- cnt = 1
						group by ir.ITEM_ID) cc
						where cnt=1
				Set @actualbsize = @@ROWCOUNT
				--Second: delete the records in TB_ITEM_REVIEW for the items that are shared only in this review
				--ON 27/04/2022 we start doing this deletion on items that are NOT shared, before we explicitly did it only for shared items
				delete from TB_ITEM_REVIEW 
					where REVIEW_ID = @revID
						AND ITEM_ID in (select item_ID from @tt )

	
				--select DATEDIFF(millisecond, @start, getdate()) as 'ItemReview'
				--set @start = GETDATE()

				--Third: explicitly delete the records that can't be automatically deleted through the foreign key cascade actions
				-- the cnt=1 clause makes sure we don't touch data related to items that appear in other reviews.
				DELETE FROM TB_ITEM_DUPLICATES where ITEM_ID_OUT in (SELECT item_ID from @tt )	
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_DUPLICATES'
				--set @start = GETDATE()

				DELETE FROM TB_ITEM_LINK where ITEM_ID_SECONDARY in (SELECT item_ID from @tt)

				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_LINK'
				--set @start = GETDATE()

				DELETE FROM TB_ITEM_DOCUMENT where ITEM_ID in (SELECT item_ID from @tt)
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_DOCUMENT'
				--set @start = GETDATE()

				DELETE From TB_ITEM_ATTRIBUTE where ITEM_ID in (SELECT item_ID from @tt) --and (ITEM_ARM_ID is not null AND ITEM_ARM_ID > 0)
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_ATTRIBUTE'
				--set @start = GETDATE()

				DELETE From TB_ITEM_ARM where ITEM_ID in (SELECT item_ID from @tt)
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_ARM'
				--set @start = GETDATE()

				DELETE From tb_ITEM_MAG_MATCH where ITEM_ID in (SELECT item_ID from @tt ) and REVIEW_ID = @revID
	
				--select DATEDIFF(millisecond, @start, getdate()) as 'tb_ITEM_MAG_MATCH'
				--set @start = GETDATE()

				--ADDED on 27/04/2022 rewrite
				DELETE FROM TB_ITEM_SOURCE where ITEM_ID in (SELECT item_ID from @tt ) and SOURCE_ID = @srcID
				
				--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_SOURCE'
				--set @start = GETDATE()

				--Fourth: delete the items 
				DELETE  FROM TB_ITEM WHERE ITEM_ID in (SELECT item_ID from @tt)
				
				delete from @tt --we're adding to it at the top of the cycle
				
				set @counter = @counter+1
			COMMIT TRANSACTION
			update tb_REVIEW_JOB set END_TIME = GETDATE() where REVIEW_JOB_ID = @JobId
			
			--select DATEDIFF(millisecond, @start, getdate()), @counter as 'batch(r)'
			
			waitfor delay @delay
			
			--set @start = GETDATE()
		END TRY

		BEGIN CATCH
		--select 'caught!!!!!'
			IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
			set @result = -12 --exception
			--we do the "CAST" to ensure the message fits, can't afford an exception in here...
			update tb_REVIEW_JOB set JOB_MESSAGE = CAST(JOB_MESSAGE + Char(10)+ Char(13) + 'ERROR: ' + ERROR_MESSAGE()
														+ Char(10)+ Char(13) + 'LINE: ' + ERROR_LINE() as varchar(4000))
				where REVIEW_JOB_ID = @JobId
			return --we stop if something broke down
		END CATCH
	END --WHILE cycle

	--Fifth Items that are SHARED into multiple reviews, we delete them from TB_ITEM_SOURCE and TB_ITEM_REVIEW only
	delete from @tt
	insert into @tt 
	SELECT top (@bsize) ITEM_ID FROM
		(select ir.ITEM_ID, COUNT(ir.item_id) cnt from TB_ITEM_REVIEW ir 
			inner join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
			where tis.SOURCE_ID = @srcID -- cnt = 1
			group by ir.ITEM_ID) cc
			where cnt>1
	
	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
		BEGIN TRANSACTION
			DELETE from TB_ITEM_REVIEW where ITEM_ID in (SELECT item_ID from @tt) and REVIEW_ID = @revID
			
			--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_REVIEW (shared items)'
			--set @start = GETDATE()

			DELETE from TB_ITEM_SOURCE where ITEM_ID in (SELECT item_ID from @tt) and SOURCE_ID = @srcID 
			
			--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_SOURCE (shared items)'
			--set @start = GETDATE()

			--Sixth: delete the source
			DELETE FROM TB_SOURCE WHERE SOURCE_ID = @srcID
			
			--select DATEDIFF(millisecond, @start, getdate()) as 'TB_SOURCE'
			--set @start = GETDATE()

			--we do the "CAST" to ensure the message fits, can't afford an exception at this step...
			update tb_REVIEW_JOB set END_TIME = GETDATE(), CURRENT_STATE = 'Ended', SUCCESS = 1, JOB_MESSAGE = cast('SOURCE_ID = ' + CAST(@srcID as varchar(100)) +  Char(10)+ Char(13) + JOB_MESSAGE as varchar(4000)) where REVIEW_JOB_ID = @JobId
		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		--select 'caught!!!!!'
			IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
			set @result = -12 --exception
			
			--we do the "CAST" to ensure the message fits, can't afford an exception in here...
			update tb_REVIEW_JOB set JOB_MESSAGE = CAST(JOB_MESSAGE +  Char(10)+ Char(13) + 'ERROR: ' + ERROR_MESSAGE()
														+  Char(10)+ Char(13) +'LINE: ' + ERROR_LINE() as varchar(4000))
				where REVIEW_JOB_ID = @JobId
			return --we stop if something broke down
	END CATCH
	IF @result is null OR @result = 0 Set @result = 1 -- "-2" is for when we did delete a source, but not the one the user asked for, 1 is for when we deleted the expected source
END

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SourceFromReview_ID]    Script Date: 03/05/2022 17:40:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[st_SourceFromReview_ID] 
	-- Add the parameters for the stored procedure here
	@revID int = 0 
with recompile
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Select SOURCE_NAME, count(*) As 'Total_Items',
		sum(CASE WHEN (tb_item_review.IS_DELETED = 1 and tb_item_review.MASTER_ITEM_ID is null) then 1 else 0 END) as 'Deleted_Items',
		sum(CASE WHEN (
				tb_item_review.IS_DELETED = 1 and tb_item_review.is_included = 1 AND tb_item_review.MASTER_ITEM_ID is NOT null
			) then 1 else 0 END) as 'Duplicates',
		TB_SOURCE.IS_DELETED,
		TB_SOURCE.Source_ID,
		0 as TO_ORDER
		from TB_SOURCE inner join
		tb_item_source on TB_SOURCE.source_id = tb_item_source.source_id
		--inner join tb_item on tb_item_source.item_id = tb_item.Item_ID
		inner join tb_item_review on tb_item_source.Item_ID = tb_item_review.Item_ID
		left outer join TB_IMPORT_FILTER on TB_IMPORT_FILTER.IMPORT_FILTER_ID = TB_SOURCE.IMPORT_FILTER_ID
	where TB_SOURCE.review_ID = @RevID AND TB_ITEM_REVIEW.REVIEW_ID = @RevID
	group by SOURCE_NAME,
			 TB_SOURCE.Source_ID,
			 TB_SOURCE.IS_DELETED
	
	
	-- get sourceless items count in a second resultset
	--Select COUNT(ir.ITEM_REVIEW_ID) as 'SourcelessItems' from tb_item_review ir 
	--	left outer join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
	--	left outer join TB_SOURCE ts on tis.SOURCE_ID = ts.SOURCE_ID and ir.REVIEW_ID = ts.REVIEW_ID
	--where ir.REVIEW_ID = @revID and ts.SOURCE_ID is null
	UNION
	Select 'NN_SOURCELESS_NN' as SOURCE_NAME, count(ir.ITEM_REVIEW_ID) As 'Total_Items',
		sum(CASE WHEN (ir.IS_DELETED = 1 and ir.MASTER_ITEM_ID is null) then 1 else 0 END) as 'Deleted_Items',
		sum(CASE WHEN (
				ir.IS_DELETED = 1 and ir.is_included = 1 AND ir.MASTER_ITEM_ID is NOT null
			) then 1 else 0 END) as 'Duplicates',
		Case 
			when COUNT(ir.ITEM_ID) = Sum(
										case when ir.IS_DELETED = 1 then 1 else 0 end
										) 
			then 1 else 0 end
		 as IS_DELETED,
		-1 as Source_ID,
		1 as TO_ORDER
		from tb_item_review ir 
		where ir.REVIEW_ID = @revID 
			and ir.ITEM_ID not in 
				(
					Select ITEM_ID from TB_SOURCE s
					inner join TB_ITEM_SOURCE tis on s.SOURCE_ID = tis.SOURCE_ID and s.REVIEW_ID = @revID
				)
		order by TO_ORDER, TB_SOURCE.Source_ID

	select TOP 1 SUCCESS * -1 as SOURCE_ID from tb_REVIEW_JOB
				where REVIEW_ID = @revID AND JOB_TYPE = 'delete source' and CURRENT_STATE = 'running' order by REVIEW_JOB_ID desc
END

GO
CREATE or ALTER PROCEDURE [dbo].[st_SourceDeleteForeverIsRunning] 
	@revID int,
	@result int = 0 output 
AS
BEGIN
	
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	set @result = (select TOP 1 SUCCESS * -1 as SOURCE_ID from tb_REVIEW_JOB
				where REVIEW_ID = @revID AND JOB_TYPE = 'delete source' and CURRENT_STATE = 'running' order by REVIEW_JOB_ID desc)
	IF @result is null set @result = 0
END
GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbFrequencyCrosstabAndMap]    Script Date: 04/05/2022 12:40:36 ******/
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
declare @attsX table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
declare @attsY table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
declare @segments table (ATTRIBUTE_ID bigint primary key, ATTRIBUTE_NAME nvarchar(255), ord int, done bit)
--last minute table: the parent IDs and names are in @codeNames
declare @codeNames table (SETIDX_ID bigint primary key, SETIDX_NAME nvarchar(255), SETIDY_ID bigint, SETIDY_NAME nvarchar(255),
							ATTIBUTEIDX_ID bigint, ATTIBUTEIDX_NAME nvarchar(255), ATTIBUTEIDY_ID bigint, ATTIBUTEIDY_NAME nvarchar(255)
							, SEGMENTS_PARENT_NAME nvarchar(255))

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
	 , ATTRIBUTE_ORDER, 0 from TB_ATTRIBUTE_SET tas
	 inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	 inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa on a.ATTRIBUTE_ID = pa.ATTRIBUTE_ID and pa.WEBDB_ID = @WebDbId
	 where tas.SET_ID = @setIdXAxis and PARENT_ATTRIBUTE_ID = @attributeIdXAxis
select ATTRIBUTE_ID, ATTRIBUTE_NAME from @attsX order by ord

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

If @setIdSegments > 0
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

	update @codeNames set SEGMENTS_PARENT_NAME = (
												CASE when pa.WEBDB_ATTRIBUTE_NAME is null then a.ATTRIBUTE_NAME
												ELSE pa.WEBDB_ATTRIBUTE_NAME
												END
											)
	from TB_ATTRIBUTE a
	inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa on a.ATTRIBUTE_ID = @segmentsParent and pa.ATTRIBUTE_ID = a.ATTRIBUTE_ID and pa.WEBDB_ID = @WebDbId
	
END

--declare @currAtt bigint = (select top(1) ATTRIBUTE_ID from @attsX where done = 0 order by ord)
--declare @limit int = 1000, @cycle int = 0
--while @currAtt is not null and @currAtt > 0 and @cycle < @limit
--BEGIN
--	set @cycle = @cycle+1
--	update @attsX set done = 1 where ATTRIBUTE_ID = @currAtt

--	--select ItemId from @items i
--	--	inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdXAxis
--	--	inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ITEM_ID = i.ItemId and ia.ATTRIBUTE_ID = @currAtt
--	select @currAtt, STRING_AGG(cast (ItemId as nvarchar(max)), ',') from @items i
--		inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdXAxis
--		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ITEM_ID = i.ItemId and ia.ATTRIBUTE_ID = @currAtt

--	set @currAtt = (select top(1) ATTRIBUTE_ID from @attsX where done = 0 order by ord)
--	--if @currAtt is not null print  cast(@currAtt as nvarchar(200)) + '!'
--	--else print 'ending'
--END

--set @currAtt  = (select top(1) ATTRIBUTE_ID from @attsY where done = 0 order by ord)
--set @cycle = 0
--while @currAtt is not null and @currAtt > 0 and @cycle < @limit
--BEGIN
--	set @cycle = @cycle+1
--	update @attsY set done = 1 where ATTRIBUTE_ID = @currAtt

--	--select ItemId from @items i
--	--	inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdXAxis
--	--	inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ITEM_ID = i.ItemId and ia.ATTRIBUTE_ID = @currAtt
--	select @currAtt, STRING_AGG(cast (ItemId as nvarchar(max)), ',') from @items i
--		inner join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.SET_ID = @setIdYAxis
--		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ITEM_ID = i.ItemId and ia.ATTRIBUTE_ID = @currAtt

--	set @currAtt = (select top(1) ATTRIBUTE_ID from @attsY where done = 0 order by ord)
--	--if @currAtt is not null print  cast(@currAtt as nvarchar(200)) + '!'
--	--else print 'ending'
--END

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