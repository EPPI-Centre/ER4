Use ReviewerAdmin
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'duplicates'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('duplicates', 'tmp_place_holder')
end
GO
declare @Content nvarchar(max) = '
<p class="mb-1">
	This page provides <b>basic support</b> for the most used deduplication functionalities. It <b>does not</b> yet support all the advanced functionalities available in EPPI-Reviewer 4. The functionalities available here mirror precisely those in EPPI-Reviewer 4 so please refer to the <a href="https://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2933" target="_blank">user manual</a> for further details.
</p>
<div class="container">
	<div class="row">
		<div class="col-md-5 col-12 bg-light rounded mx-0">	
			<p class="font-weight-bold alert alert-success mt-2">
				Supported features:
				<!-- You can use this page to trigger the "Get new Duplicates" routine, and evaluate the results to mark items as "Duplicate" or "Not Duplicate". This page also supports "Mark automatically" and "Advanced Mark Automatically". However, this page <b>Does NOT support</b> the following: -->
			</p>
			<ol>
			 <li>Get new duplicates (finds <em>possible duplicates</em>).</li>
			 <li>Mark as Duplicate / Not a Duplicate.</li>
			 <li>Change "Master" for a Group of Duplicates.</li>
			 <li>Mark automatically.</li>
			 <li>Advanced mark automatically (allows you to change thresholds).</li>
			</ol>
		</div>
		<div class="mx-1 col-1"></div>
		<div class="col-md-5 col-12 bg-light rounded mx-0">	
			<p class=" alert alert-warning mt-2">
				<b>Not supported features</b> (please use <a href="http://eppi.ioe.ac.uk/eppireviewer4/" target="_blank">EPPI-Reviewer 4</a>):
			</p>
			<ol>
			 <li>Manually adding and removing duplicates to an existing group.</li>
			 <li>Manually creating new groups.</li>
			 <li>Searching for groups.</li>
			 <li>Deleting Groups.</li>
			 <li>Resetting the duplicates data.</li>
			</ol>
		</div>
	</div>
</div>

<p class="mt-2 mb-1">
	Thus, it''s possible that handling duplicates will still require occasional use of <a href="http://eppi.ioe.ac.uk/eppireviewer4/" target="_blank">EPPI-Reviewer 4</a>.
</p>
<div class="mx-3 mt-0 rounded border border-danger bg-light">
	<p class="m-2 alert alert-danger">
		<b>If you do not have access to EPPI-Reviewer 4:</b>
	</p> 
	<p class="mx-2">(Applies to users of MacOS "Catalina" from version 10.15.) To ensure you won''t need to use the advanced deduplication functionalities that are not yet available in EPPI-Reviewer Web, it is <b>very important</b> to proceed as follows:
	
	<ol >
	 <li>Import <b>all your search results</b>, without triggering "Get new Duplicates".</li>
	 <li>Once you have imported all items that need deduplicating, click "Get new Duplicates" exactly once.</li>
	</ol>
	<p class="mx-2">
		This will prevent the appearance of "overlapping groups", which (currently) require access to EPPI-Reviewer 4 (see the <a href="https://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2933" target="_blank">user manual</a> for details).
	</p>
</div>
<p>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'duplicates'

GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Items details</p>
<p>The <b>Items details</b> screen is where much of the data entry in EPPI-Reviewer takes place. You also have access to a number of other areas in the program using the tabs on the right side of the screen. Further help is available in those tabs.</p>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Coding</b><br>
	<img class="img"  src="Images/ItemDetails-Coding.gif" />
	</div>
	<div class="col-sm">
	<br>
	<b>Coding</b> is carried out in the Item Details screen. On the right side of the screen is the citation while on the left are the coding tools.<br> Clicking a check-box next to a code assigns that code to the item. Clicking the <b>Info</b> buttons allows you to add textual information to your selected code.<br>
	To remove coding, de-select a check-box and confirm your action.<br>
	To move through your references use the <b>Previous</b>, <b>Next</b>, <b>First</b> and <b>Last</b> buttons.<br>The <b>Auto-advance</b> option will automatically move to the next item when you select a code.
	</div>
</div>
<br><br>
<div class="row">
	<div class="col-sm">		
	<b>Edit coding tools</b><br>
	<img class="img"  src="Images/ItemDetails-EditCodes.gif" />
	</div>
	<div class="col-sm">
	<br>
	You can <b>add</b> and <b>edit</b> codes using the buttons on the left side of the screen (sitting above the coding tools).<br>
	To add a code select the coding tool and a location in the coding tool (if the tool has multiple levels). Click on the <b>+</b> button and enter a code type, code name and code description. Click <b>Create</b> and you will see the new code in the coding tool.<br>
	You can reposition the code by clicking on the <b>Up</b> and <b>Down</b> arrow buttons.<br>
	To edit a code, select the code and click the <b>Edit</b> button (looks like a pencil). This will allow you to edit the code type, code name and description. To save your edits click <b>Update</b>.<br>
	Deleting a code is the same as editing a code except you must click the <b>Delete Code</b> button and confirm your actions. 
	</div>
</div>	


<br><br>
<div class="row">
	<div class="col-sm">		
	<b>Term highlighting</b><br>
	<img class="img"  src="Images/ItemDetails-TermHighlight.gif" />
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
</div>

<br><br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'itemdetails'

GO
