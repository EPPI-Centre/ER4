USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SourceDeleteForever]    Script Date: 11/06/2019 13:06:06 ******/
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
	@srcID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
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

Use ReviewerAdmin
GO

declare @ctx nvarchar(100) = 'main\collaborate'
declare @Content nvarchar(max) = '
<p class="font-weight-bold">Collaborate</p>
<p>The collaborate tab allows to, create, manage and use <b>Coding Assignments</b> and <b>Comparisons</b>. The process of creating Coding Assignments starts with creating or identifying the code(s) which will contain the group of items used for an assignment. Users can then create coding assignments using this code(s) as the <em>Reference group to assign</em>.<br />
Comparisons are used to identify and reconcile disagreements resulting from double and multiple coding exercises (where more than one reviewer will code/screen the same items independently).
</p>
<div class="container">
<div class="row">
	<div class="col-sm">		
	<b>Expand/Collapse the area of interest.</b><br>
	<img class="img" src="Images/CollaborateExpandCollapse.gif"><br>
	You can <b>expand</b> or <b>collapse</b> the two main areas of the screen, depending on what you need to use.<br><br>
	</div>
	<div class="col-sm">		
	<b>Toolbar commands.</b><br>
	<img class="img" src="Images/CollaborateToolbar.gif"><br>
	The main toolbar allows to: <b>Create Reference Groups</b>, which are used to drive Coding Assignments; <b>Create a new code</b>, which might be useful to organise "Reference Groups"; <b>Create Coding Assignments</b> and <b>Create Comparisons</b>.<br><br>
	</div>	
</div>

<div class="row">
	<div class="col-sm">
	<b>Create Reference Groups</b><br>
	<img class="img" src="Images/CollaborateCreateRNDcodes.gif"><br>
	This option allows to create groups of randomly selected references, using various criteria to identify the relevant references. These groups can then be used to assign work (Create Coding Assignments). Once created, it is <b>recommended to rename the new codes</b> so to make their purpose clear.<br><br> 
	</div>
	<div class="col-sm">
	<b>Create Coding Assignments</b><br>
	<img class="img" src="Images/CollaborateCreateCodingAssignments.gif"><br>
	Coding Assignments allow to <b>assign work</b> to a given reviewer and to <b>keep track</b> of the progress made. For double screening the same reference Group can be used to create two assignments, one for each reviewer.<br><br>
	</div>
</div> 

<div class="row">
	<div class="col-sm">
	<b>Create Comparisons</b><br>
	<img class="img" src="Images/CollaborateCreateComparison.gif"><br>
	Comparisons are used to <b>find, count and resolve disagreements</b> in double/multiple coding exercises. Comparisons also provide a <b>snapshot</b> of the Agreements Vs. Disagreement counts as present <b>when the comparison was created</b>.<br><br>
	</div>
	<div class="col-sm">
	<b>Using Comparisons</b><br>
	<img class="img" src="Images/CollaborateUsingComparisons.gif"><br>
	Using Comparisons, you can <b>list the Agreed and Disagreed</b> references. You can also "<b>Complete</b>" the Agreed coding, and access the <b>Reconcile Page</b> to assess the Disagreed coding in detail. Please note that the Reconcile Page is optimised to deal with (simple) screening disagreements.<br><br>
	</div>
</div>
</div>
'

IF exists (select * from TB_ONLINE_HELP where CONTEXT = @ctx)
BEGIN
	update TB_ONLINE_HELP set HELP_HTML = @Content where CONTEXT = @ctx
END
ELSE
BEGIN
	INSERT into TB_ONLINE_HELP ([CONTEXT]
			   ,[HELP_HTML])
		 VALUES
			   (@ctx
			   ,@Content)
END
GO


