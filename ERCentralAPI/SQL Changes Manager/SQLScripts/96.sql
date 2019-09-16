USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemTimepointDelete]    Script Date: 16/09/2019 15:03:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemTimepointDelete]
(
	@ITEM_TIMEPOINT_ID BIGINT,
	@REVIEW_ID INT
)

As

SET NOCOUNT ON


declare @ITEM_ID int = 0;

set @ITEM_ID = (select ITEM_ID from TB_ITEM_TIMEPOINT WHERE 
ITEM_TIMEPOINT_ID = @ITEM_TIMEPOINT_ID)

declare @check int = 0
--make sure source belongs to review...
set @check = (select count(ITEM_ID) from TB_ITEM_REVIEW where ITEM_ID = @ITEM_ID and REVIEW_ID = @REVIEW_ID)
if (@check != 1) return

UPDATE TB_ITEM_OUTCOME
	SET ITEM_TIMEPOINT_ID = NULL
	WHERE ITEM_TIMEPOINT_ID = @ITEM_TIMEPOINT_ID

DELETE FROM TB_ITEM_TIMEPOINT
	WHERE ITEM_TIMEPOINT_ID = @ITEM_TIMEPOINT_ID

SET NOCOUNT OFF
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewWorkAllocationDelete]    Script Date: 16/09/2019 15:12:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ReviewWorkAllocationDelete]
(
	@WORK_ALLOCATION_ID INT,
	@REVIEW_ID INT
)

As

/* ADD TO THIS THE RETRIEVAL OF ROLES */
declare @check int = 0
--make sure source belongs to review...
set @check = (select count(WORK_ALLOCATION_ID) from TB_WORK_ALLOCATION 
where WORK_ALLOCATION_ID= @WORK_ALLOCATION_ID and REVIEW_ID = @REVIEW_ID)
if (@check != 1) return


DELETE FROM TB_WORK_ALLOCATION
WHERE WORK_ALLOCATION_ID = @WORK_ALLOCATION_ID
GO

/****** Object:  StoredProcedure [dbo].[st_SourceDelete]    Script Date: 16/09/2019 15:20:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sergio
-- Create date: 20/7/09
-- Description:	(Un/)Delete a source and all its Items
-- =============================================
ALTER PROCEDURE [dbo].[st_SourceDelete] 
	-- Add the parameters for the stored procedure here
	@source_ID int,
	@REVIEW_ID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

declare @check int = 0
--make sure source belongs to review...
set @check = (select count(SOURCE_ID) from TB_SOURCE 
where SOURCE_ID= @source_ID and REVIEW_ID = @REVIEW_ID)
if (@check != 1) return



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
ALTER procedure [dbo].[st_SearchDelete]
(
	@SEARCHES NVARCHAR(MAX),
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

declare @check int = 0
--make sure source belongs to review...
set @check = (SELECT COUNT(*) FROM TB_SEARCH
INNER JOIN fn_split_int(@SEARCHES, ',') SearchList on SearchList.value = TB_SEARCH.SEARCH_ID
WHERE REVIEW_ID = @REVIEW_ID)

if (@check < 1) return

DELETE FROM TB_SEARCH_ITEM
	FROM TB_SEARCH_ITEM INNER JOIN fn_split_int(@SEARCHES, ',') SearchList on SearchList.value = TB_SEARCH_ITEM.SEARCH_ID
		
DELETE FROM TB_SEARCH
	FROM TB_SEARCH INNER JOIN fn_split_int(@SEARCHES, ',') SearchList on SearchList.value = TB_SEARCH.SEARCH_ID

SET NOCOUNT OFF
GO



