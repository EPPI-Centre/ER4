USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SourceDelete]    Script Date: 05/05/2022 16:30:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[st_SourceDelete] 
	@source_ID int,
	@REVIEW_ID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

declare @check int = 0
set @check = (select count(SOURCE_ID) from TB_SOURCE 
where SOURCE_ID= @source_ID and REVIEW_ID = @REVIEW_ID)
if (@check != 1) return

--second check: is this source being deleted forever right now? In odd situations, this case may semi-legitimately happen...
set @check = null
set @check = (select count(REVIEW_JOB_ID) from tb_REVIEW_JOB where REVIEW_ID = @REVIEW_ID AND JOB_TYPE = 'delete source' and CURRENT_STATE = 'running' and SUCCESS = @source_ID * -1) 
if (@check is not null and @check > 0) return

declare @state bit;
declare @rev_ID int;
Set @state = 1 - (select IS_DELETED from TB_SOURCE where SOURCE_ID = @source_ID)
set @rev_ID = (select review_id from TB_SOURCE where SOURCE_ID = @source_ID)


BEGIN TRY

BEGIN TRANSACTION
update TB_SOURCE set IS_DELETED = @state where SOURCE_ID = @source_ID
update IR set IS_DELETED = @state, IS_INCLUDED = 1
	from TB_SOURCE inner join
		tb_item_source on TB_SOURCE.source_id = tb_item_source.source_id
		inner join tb_item on tb_item_source.item_id = tb_item.Item_ID
		inner join tb_item_review as IR on tb_item.Item_ID = IR.Item_ID
	where (TB_SOURCE.SOURCE_ID = @source_ID AND IR.REVIEW_ID = @rev_ID)
		AND (IR.MASTER_ITEM_ID is null OR IR.IS_DELETED = 0)

COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
END CATCH
END

GO


