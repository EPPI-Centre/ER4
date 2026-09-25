USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_SourceDeleteForeverInBatches]    Script Date: 25/09/2026 14:18:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_SourceDeleteForeverInBatches] 
	-- Add the parameters for the stored procedure here
	@srcID int = 0,
	@revID int,
	@contactID int,
	@JobId int,
	@batchSize int = 20,
	@result int = 0 output 
AS
BEGIN

	--@result values: 
	--		-1: failed to validate source, not deleted!
	--		-2: exception, job is now marked as failed!
	--		1: finished deleting this batch, more items to be deleted remain
	--		0: finished deleting - the whole source is now gone
	declare @check int = 0;
	set @check = (select count(source_id) from TB_SOURCE where SOURCE_ID = @srcID and REVIEW_ID = @revID and IS_DELETED = 1);
	if (@check != 1) 
	BEGIN
		set @result = -1; --error, should not happen, most likely, this source wasn't already marked as deleted.
		return;
	END
	Declare @tt TABLE
	(
		item_ID bigint PRIMARY KEY
	)
	declare @actualbsize int = @batchSize --actual size of batch we'll delete
	insert into @tt --First: get the ITEM_IDs we'll deal with, excluding those that appear in more than one review
		SELECT top (@batchSize) ITEM_ID FROM
			(select ir.ITEM_ID, COUNT(ir.item_id) cnt from TB_ITEM_REVIEW ir 
				inner join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
				where tis.SOURCE_ID = @srcID -- cnt = 1
				group by ir.ITEM_ID) cc
				where cnt=1
	Set @actualbsize = @@ROWCOUNT
	if @actualbsize > 0
	BEGIN
		BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
			BEGIN TRANSACTION
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

				--ADDED on 09/04/2026 
				DELETE FROM TB_ITEM_TIMEPOINT where ITEM_ID in (SELECT item_ID from @tt ) 
				--Fourth: delete the items 
				DELETE  FROM TB_ITEM WHERE ITEM_ID in (SELECT item_ID from @tt)
			COMMIT TRANSACTION;
			
			set @actualbsize = (select count(*) from TB_ITEM_SOURCE where SOURCE_ID = @srcID);
			if @actualbsize > 0
			BEGIN
				update tb_REVIEW_JOB set END_TIME = GetDATE() where REVIEW_JOB_ID = @JobId;
				set @result = 1;
				return;
			END
		END TRY

		BEGIN CATCH
		--select 'caught!!!!!'
			IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
			set @result = -2 --exception
			--we do the "CAST" to ensure the message fits, can't afford an exception in here...
			update tb_REVIEW_JOB set JOB_MESSAGE = CAST(JOB_MESSAGE + Char(10)+ Char(13) + 'ERROR: ' + ERROR_MESSAGE()
														+ Char(10)+ Char(13) + 'LINE: ' + cast(ERROR_LINE() as varchar(400)) as varchar(4000))
									, CURRENT_STATE = 'Failed'
									, SUCCESS = 0
									, END_TIME = GetDATE()
				where REVIEW_JOB_ID = @JobId
			return; --we stop if something broke down
		END CATCH
		
	END
	ELSE BEGIN --we had already finished items that are not shared, how about items in multiple reviews?
		insert into @tt 
		SELECT top (@batchSize) ITEM_ID FROM
			(select ir.ITEM_ID, COUNT(ir.item_id) cnt from TB_ITEM_REVIEW ir 
				inner join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
				where tis.SOURCE_ID = @srcID -- cnt = 1
				group by ir.ITEM_ID) cc
				where cnt>1
		Set @actualbsize = @@ROWCOUNT
		IF @actualbsize > 0
		BEGIN
			BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
				BEGIN TRANSACTION

					-- added JB 23/09/2026 to support shared items
					DELETE From tb_ITEM_MAG_MATCH where ITEM_ID in (SELECT item_ID from @tt ) and REVIEW_ID = @revID

					DELETE from TB_ITEM_REVIEW where ITEM_ID in (SELECT item_ID from @tt) and REVIEW_ID = @revID
			
					--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_REVIEW (shared items)'
					--set @start = GETDATE()

					DELETE from TB_ITEM_SOURCE where ITEM_ID in (SELECT item_ID from @tt) and SOURCE_ID = @srcID 
			
					--select DATEDIFF(millisecond, @start, getdate()) as 'TB_ITEM_SOURCE (shared items)'
					--set @start = GETDATE()
			
					

				COMMIT TRANSACTION
				set @actualbsize = (select count(*) from TB_ITEM_SOURCE where SOURCE_ID = @srcID);
				if @actualbsize > 0
				BEGIN
					update tb_REVIEW_JOB set END_TIME = GetDATE() where REVIEW_JOB_ID = @JobId;
					set @result = 1;
					return;
				END
			END TRY
			BEGIN CATCH
				--select 'caught!!!!!'
					IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
					set @result = -2 --exception
			
					--we do the "CAST" to ensure the message fits, can't afford an exception in here...
					update tb_REVIEW_JOB set JOB_MESSAGE = CAST(JOB_MESSAGE + Char(10)+ Char(13) + 'ERROR: ' + ERROR_MESSAGE()
																+ Char(10)+ Char(13) + 'LINE: ' + cast(ERROR_LINE() as varchar(400)) as varchar(4000))
										, CURRENT_STATE = 'Failed'
										, SUCCESS = 0
										, END_TIME = GetDATE()
						where REVIEW_JOB_ID = @JobId
					return --we stop if something broke down
			END CATCH
		END
	END--finished block that deletes shared items, if any
	--both "item deleting blocks" (non shared, and shared items) return whenever there are still items to delete
	--so if we reach this point, it's because we only have the source to delete...
	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
		BEGIN TRANSACTION
				
			DELETE FROM TB_SOURCE WHERE SOURCE_ID = @srcID

		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		--select 'caught!!!!!'
			IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
			set @result = -2 --exception
			
			--we do the "CAST" to ensure the message fits, can't afford an exception in here...
			update tb_REVIEW_JOB set JOB_MESSAGE = CAST(JOB_MESSAGE + Char(10)+ Char(13) + 'ERROR: ' + ERROR_MESSAGE()
															+ Char(10)+ Char(13) + 'LINE: ' + cast(ERROR_LINE() as varchar(400)) as varchar(4000))
									, CURRENT_STATE = 'Failed'
									, SUCCESS = 0
									, END_TIME = GetDATE()
					where REVIEW_JOB_ID = @JobId
			return --we stop if something broke down
	END CATCH
	--if we didn't "return" until now, it's because we have finished the whole job!
	set @result = 0;
	update tb_REVIEW_JOB set END_TIME = GETDATE(), CURRENT_STATE = 'Ended', SUCCESS = 1, JOB_MESSAGE = cast('SOURCE_ID = ' + CAST(@srcID as varchar(100)) +  Char(10)+ Char(13) + JOB_MESSAGE as varchar(4000)) where REVIEW_JOB_ID = @JobId
END
GO

