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

Use ReviewerAdmin
GO

declare @Content nvarchar(max) = '
<div class="container">
	<div class="row">
		<div class="col-sm">		
		<b class="text-primary">Reference list</b><br>
		<img class="img"  src="Images/ReferenceList.gif" /><br>
		You can select individual or all items from your reference list. Clicking at the top of a column will order the list by that column. By default, the list is paged by 100 items. To list items based on the Include, Exclude and Delete flags click on the <b>I</b>, <b>E</b>, and <b>D</b> buttons. All imported items start with the <b>I</b> flag.<br><br>
		</div>
		<div class="col-sm">		
		<b class="text-primary">View options</b><br>
		<img class="img"  src="Images/ViewOptions.gif" /><br>
		Clicking on <b>View options</b> will allow to change what fields are shown in the reference list and edit the number of items displayed in each page.<br><br>
		</div>	
	</div>

	<div class="row">
		<div class="col-sm">
		<b class="text-primary">List items by coding</b><br>
		<img class="img"  src="Images/ListItemsByCoding.gif" /><br>
		To list items based on coding, select a code in a coding tool and click on <b>With this code</b>. The <b>With this code (Excluded)</b> option lists the coded items with the <b>E</b> flag.<br><br> 
		</div>
		<div class="col-sm">
		<b class="text-primary">Assign/remove codes</b><br>
		<img class="img"  src="Images/AssignCodeToItems.gif" /><br>
		To assign a code to multiple items, select the items and the code and then click <b>Assign code</b> to apply the code to the items. If you select <b>Remove code from selection</b> the code will be removed from the selected items.<br><br>
		</div>
	</div> 

	<div class="row">
		<div class="col-sm">
		<b class="text-primary">Coding report</b><br>
		<img class="img"  src="Images/CodingReport.gif" /><br>
		To generate a coding report select the items to include in the report and click <b>Coding report</b>. Next, select the coding tool(s) that will make up the report and click <b>Get report</b> to generate the report. You can <b>Save</b> the report as well as display it in a new tab for printing.<br><br>
		</div>
		<div class="col-sm">
		<b class="text-primary">Cluster</b><br>
		<img class="img"  src="Images/Cluster.png" /><br>
		Cluster uses the Lingo3G clustering engine to automatically generate a coded map of your references based on their abstracts. You can select what items should be included in the map and the complexity of the coding can be adjusted using the available parameters.<br><br>
		</div>
	</div>

	  <div class="row">	
		<div class="col-sm">
		<b class="text-primary">Quick Question Report</b><br>
		<img class="img"  src="Images/QuickQuestionReport.gif" /><br>
		To generate a question report select the items to include in the report and select <b>Quick Question Report</b>
		from the <b>Coding Report</b> dropdown menu.<br><br>
		</div>	
		<div class="col-sm">
		<br>
		You can select any code that has a child code, from any codeset, and it will become a new column in the report.<br>
		There are options to display the title, Info box text and pdf coded text in your report.<br>
		You can <b>Save</b> the report as well as display it in a new tab for printing.<br><br>
		</div>
	</div>
	<div class="row">
		<div class="col">
		<b class="text-primary">Mark Items as Include/Excluded or Deleted</b><br>
		<img class="img"  src="Images/IncludeExcludeDelete.gif" /><br>
		By default, Items are imported as "<em>Included</em>" ("I" status flag). You can use this screen to change the status flag to sideline items in bulk, by flagging them as "Excluded" or "Deleted".<br>
		</div>
		<div class="col-md-12 col-lg-5">
		<br />
		The "In/Exclude" button allows to <b>mark items as included or excluded</b>. You can pick the items to be modified either by selecting them or by picking the "Documents with this code" option. <br />
		The "Trash" icon button allows to <b>mark as deleted</b> the items selected in the current list.<br />
		To "undelete" some deleted items, you can use the "In/Exclude" button to re-include the items in question or flag them as Excluded.<br />
		<b>Note:</b> excluded items can participate in frequency reports, if explicitly required; deleted items are ignored by most reporting features.<br />
		 Deleted items are usually understood as items that should not have been created/imported. Excluded items (optionally) represent items that have been screened and excluded.
		</div>
	</div>
	<br />
	<div class="row mt-2">
		<div class="col-sm">
		<b class="text-primary">Other options</b><br>
		Other options on this screen include:<br>
		<b>Import items</b> - takes you to the import items page. More help is available on that page.<br>
		<b>To RIS</b> - allows you to export your selected items as an RIS formatted text file.<br>	
		<br><br> 
		</div>
	</div>
</div>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'main\references'
GO