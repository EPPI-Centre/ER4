USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SourceDeleteForever]    Script Date: 12/07/2019 16:21:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sergio
-- Create date: 20/7/09
-- Description:	(Un/)Delete a source and all its Items
-- =============================================
ALTER PROCEDURE [dbo].[st_SourceDeleteForever] 
	-- Add the parameters for the stored procedure here
	@srcID int,
	@revID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @check int = 0
	--make sure source belongs to review...
	set @check = (select count(source_id) from TB_SOURCE where SOURCE_ID = @srcID and REVIEW_ID = @revID)
	if (@check != 1) return

	Declare @tt TABLE
	(
		item_ID bigint PRIMARY KEY
		,cnt int  	
	)
	insert into @tt --First: get the ITEM_IDs we'll deal with and see if they appear in more than one review
	SELECT ITEM_ID, cnt FROM
		(select ir.ITEM_ID, COUNT(ir.item_id) cnt from TB_ITEM_REVIEW ir 
			inner join TB_ITEM_SOURCE tis on ir.ITEM_ID = tis.ITEM_ID
			where tis.SOURCE_ID = @srcID -- cnt = 1
			group by ir.ITEM_ID) cc

	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
	BEGIN TRANSACTION


	--Second: delete the records in TB_ITEM_REVIEW for the items that are shared across reviews
	delete from TB_ITEM_REVIEW 
		where REVIEW_ID = (select REVIEW_ID from TB_SOURCE where SOURCE_ID = @srcID)
			AND ITEM_ID in (select item_ID from @tt where cnt > 1)
	--Third: explicitly delete the records that can't be automatically deleted through the foreign key cascade actions
	-- the cnt=1 clause makes sure we don't touch data related to items that appear in other reviews.
	DELETE FROM TB_ITEM_DUPLICATES where ITEM_ID_OUT in (SELECT item_ID from @tt where cnt = 1)
	DELETE FROM TB_ITEM_LINK where ITEM_ID_SECONDARY in (SELECT item_ID from @tt where cnt = 1)
	DELETE FROM TB_ITEM_DOCUMENT where ITEM_ID in (SELECT item_ID from @tt where cnt = 1)
	DELETE From TB_ITEM_ATTRIBUTE where ITEM_ID in (SELECT item_ID from @tt where cnt = 1) and (ITEM_ARM_ID is not null AND ITEM_ARM_ID > 0)
	DELETE From TB_ITEM_ARM where ITEM_ID in (SELECT item_ID from @tt where cnt = 1)
	--Fourth: delete the items 
	DELETE FROM TB_ITEM WHERE ITEM_ID in (SELECT item_ID from @tt where cnt = 1)
	--Fifth: delete the source
	DELETE FROM TB_SOURCE WHERE SOURCE_ID = @srcID
	--there is a lot that happens automatically through the cascade option of foreing key relationships
	--if some cross-reference route is not covered by the explicit deletions the whole transaction should rollback thanks to the catch clause.
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	END CATCH
	END

GO

-- =============================================
-- Author:		Sergio
-- Create date: 
-- Description:	Delete An ItemDocument and associated Item_Attribute_Text
-- =============================================
ALTER PROCEDURE [dbo].[st_ItemDocumentDelete] 
	-- Add the parameters for the stored procedure here
	@DocID bigint = 0,
	@RevID int
AS
BEGIN
	declare @check int = 0
	--make sure source belongs to review...
	set @check = (select count(ITEM_DOCUMENT_ID) from TB_ITEM_DOCUMENT id
		inner join TB_ITEM_REVIEW ir on id.ITEM_ID = ir.ITEM_ID and REVIEW_ID = @RevID and ITEM_DOCUMENT_ID = @DocID)
	if (@check != 1) return
	BEGIN TRY
		BEGIN TRANSACTION
		delete from TB_ITEM_ATTRIBUTE_TEXT where ITEM_DOCUMENT_ID = @DocID
		delete from tb_ITEM_DOCUMENT where ITEM_DOCUMENT_ID = @DocID
		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	END CATCH
END
GO