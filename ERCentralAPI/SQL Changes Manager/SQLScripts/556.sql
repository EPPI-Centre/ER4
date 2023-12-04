USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbMap]    Script Date: 21/11/2023 15:48:00 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON 
GO

CREATE or ALTER   PROCEDURE [dbo].[st_WebDbMap]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@WEBDB_MAP_ID INT
)
As
--first, check that all parameters match up...
declare @check int = (select WEBDB_ID from TB_WEBDB w
						where w.REVIEW_ID = @REVIEW_ID and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return


-- second, since the @WEBDB_ID is in @REVIEW_ID, check that the @WEBDB_MAP_ID is in the @WEBDB_ID
declare @check2 int = (select WEBDB_MAP_ID from TB_WEBDB_MAP
						where WEBDB_ID = @WEBDB_ID and WEBDB_MAP_ID = @WEBDB_MAP_ID
						)
IF @check2 is null OR @check2 < 1 return

--OK, all checks match up, phew

select m.*, s1.SET_ID as [COLUMNS_SET_ID], s2.SET_ID as [ROWS_SET_ID], s3.SET_ID as [SEGMENTS_SET_ID]
	, a1.ATTRIBUTE_ID as [COLUMNS_ATTRIBUTE_ID], a2.ATTRIBUTE_ID as [ROWS_ATTRIBUTE_ID], a3.ATTRIBUTE_ID as [SEGMENTS_ATTRIBUTE_ID]
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
     where WEBDB_MAP_ID = @WEBDB_MAP_ID


GO


-----------------------------------------------------------------------

USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'itemdetails'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('itemdetails', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '


<p class="font-weight-bold">Items details</p>
<p>The <b>Items details</b> screen is where much of the data entry in EPPI-Reviewer takes place. You also have access to a number of other areas in the 
program using the tabs on the right side of the screen. Further help is available in those tabs.</p>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Coding</b><br>
	<img class="img"  src="assets/Images/ItemDetails-Coding.gif" />
	</div>
	<div class="col-sm">
	<br>
	<b>Coding</b> is carried out in the Item Details screen. On the right side of the screen is the citation while on the left are the coding tools.<br> 
	Clicking a check-box next to a code assigns that code to the item. Clicking the <b>Info</b> buttons allows you to add textual information to your selected code.<br>
	To remove coding, de-select a check-box and confirm your action.<br>
	To move through your references use the <b>Previous</b>, <b>Next</b>, <b>First</b> and <b>Last</b> buttons.<br>The <b>Auto-advance</b> option will automatically 
	move to the next item when you select a code.<br><br>
	<table>
		<tr>
		<td>
			<img class="img"  src="assets/Images/HotKeys.png" />
		</td>
		<td style="vertical-align:top; padding-left: 5px;">
			<b>Hot keys</b> - for ''mouse free'' coding.<br>
			Select a coding tool or section of coding tool and click the "Hot key" icon.<br>
			This will place numbers next to the child codes.<br>
			Clicking <b>Alt</b> and the number will select that code.
		</td>
		</tr>
	</table>
	</div>
</div>
<br><br>
<div class="row">
	<div class="col-sm">		
	<b>Edit coding tools</b><br>
	<img class="img"  src="assets/Images/ItemDetails-EditCodes.gif" />
	</div>
	<div class="col-sm">
	<br>
	You can <b>add</b> and <b>edit</b> codes using the buttons on the left side of the screen (sitting above the coding tools).<br>
	To add a code select the coding tool and a location in the coding tool (if the tool has multiple levels). Click on the <b>+</b> button and enter a code type, 
	code name and code description. Click <b>Create</b> and you will see the new code in the coding tool.<br>
	<img class="img"  src="assets/Images/CodeTreeIcons.png" /><br>	
	You can reposition the code by clicking on the <b>Up</b> and <b>Down</b> <i>arrow</i> buttons.<br>
	The <b>Up</b> and <b>Down</b> <i>chevron</i> buttons will move the selected code or coding tool to the top and bottom of the list.<br>
	To edit a code, select the code and click the <b>Edit</b> button (looks like a pencil). This will allow you to edit the code type, code name and description. 
	To save your edits click <b>Update</b>.<br>
	Deleting a code is the same as editing a code except you must click the <b>Delete Code</b> button and confirm your actions. 
	</div>
</div>	

<br><br>
<div class="row">
	<div class="col-sm">		
	<b>Term highlighting</b><br>
	<img class="img"  src="assets/Images/ItemDetails-TermHighlight.gif" />
	</div>
	<div class="col-sm">
	<br>
	You can highlight relevant and non-relevant terms in the title and abstract to assist in the screening process.<br>
	Check <b>Show terms</b> to make the highlighted terms visible.<br>
	To highlight terms, select words in the title and/or abstract and click either the <b>Add relevant term</b> or <b>Add irrelevant term</b> buttons.<br>
	To edit the list of terms click the <b>Show/Hide terms</b> button. You can now add, edit and delete terms and change whether they are 
	relevant or irrelevant terms.<br>
	The style of highlighting can be changed using the <b>Change style</b> dropdown menu.<br> 
	</div>
</div>	

<br><br>
<div class="row">
	<div class="col-sm">		
	<b>Numeric outcome data</b><br>
	<img class="img"  src="assets/Images/ItemDetails-Outcomes.gif" />
	</div>
	<div class="col-sm">
	<br>
	You can enter numeric outcome data into EPPI-Reviewer using forms specific to the type of outcomes found in your studies.<br>
	Any code with the codetype <b>Comparison</b>, <b>Outcome</b> or <b>Intervention</b> will have an <b>Outcomes</b> button next to it. 
	Clicking on the <b>Outcomes</b> button will open a panel allowing you to edit an existing outcome or create a new outcome.<br>
	To create a new outcome	specifiy the type (continuous or binary) and the format of the numeric outcome data and the form will 
	change accordingly.<br>
	Any predefined arms, time points, interventions, comparisons and outcomes are available for selection. As well you can select any codes 
	within your coding tool to help characterise the outcome data.<br>
	All saved outcome data is available to EPPI-Reviewer''s reporting and meta-analyis functions.
	</div>
</div>	


<br><br>
<div class="row">
<div class="col-sm">		
	<img class="img"  src="assets/Images/FindEdit.png" /><br>
	</div>
	<div class="col-sm">		
	<b>Find on Web</b><br>
	Use the <b>Find on Web</b> dropdown to generate a search for your item in Microsoft Academic, Google, or Google scholar.<br>
	The search will be based on the title and author of the reference and the results will be displayed in a new tab.<br>
	</div>
	<div class="col-sm">
	<b>Edit reference</b><br>
	To edit your reference click on the <b>Edit</b> button. A form will appear allowing you to edit the reference fields.<br>
	</div>
</div>	
</div>
<br>

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'itemdetails'

GO

------------------------------------------------------------------------

