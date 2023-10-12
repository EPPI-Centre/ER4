-------------------------------------------------------------------------------------------------
-- webdbs (old 444.sql)
-------------------------------------------------------------------------------------------------

USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'webdbs'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('webdbs', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">EPPI-Vis</p>
<p><b>EPPI-Vis</b> is the EPPI-Centre''s online webdatabase application for visualising and exploring EPPI-Reviewer data. It has been created to support knowledge translation of your review findings to the public in a user-friendly layout.<br />
</p>
<div class="container">
<div class="row">
	<div class="col-sm">		
	<b>Create a new visualisation.</b><br>
	<img class="img" src="assets/Images/EPPIVis_Create.png"><br>
	To create a new visualisation click <b>Add new</b>, enter a title, a subTitle and click <b>Save</b><br><br>
	<br><br>
	</div>
	<div class="col-sm">		
	<b>Edit visualisation - click Edit!</b> <img class="img" src="assets/Images/EPPIVis_EditButton.png"><br>
	<img class="img" src="assets/Images/EPPIVis_Edit.png"><br>
	Use the text editor to enter a description.<br>
	Click <b>Add</b> for <em>Only items with this code</em> to choose what items will be accessible to the visualisation.<br>
	Toggle <b>Open access</b> to enter a username and password for restricted access.<br>
	Be sure to click <b>Save</b> when you are finished.<br><br>
	</div>	
</div>

<div class="row">
	<div class="col-sm">
	<b>Adding logos</b> - click <b>Edit!</b> then <b>Edit images</b> <img class="img" src="assets/Images/EPPIVis_EditImageButton.png"><br>
	<img class="img" src="assets/Images/EPPIVis_AddLogos.png"><br>
	Select the image location and click <b>Select files</b> to choose and upload an image. Image size restrictions and image caching details 
	are described on the screen. Be sure to click <b>Save</b> when you are finished.<br><br> 
	</div>
	<div class="col-sm">
	<b>Adding logo links</b><br>
	<img class="img" src="assets/Images/EPPIVis_LogoUrls.png"><br>
	Click <b>Edit!</b> and you will see text boxes below the description area.<br> 
	You can enter urls for your logos to make them linkable to a website.<br>
	Enter the full path including https://<br>
	Be sure to click <b>Save</b> when you are finished.<br><br>
	</div>
</div> 

<div class="row">
	<div class="col-sm">
	<b>Add coding tools</b><br>
	<img class="img" src="assets/Images/EPPIVis_AddCodingTools.png"><br>
	Users can explore your data and generate reports based on the coding tools you add to the visualisation.<br>
	Select a coding tool from your review from the dropdown menu.<br>
	Click <b>Add!</b> and it will appear in the list on the left.<br><br>
	</div>
	<div class="col-sm">
	<b>Edit coding tools</b><br>
	<img class="img" src="assets/Images/EPPIVis_EditTools01.png"><br>
	You can edit the wording of the coding tools and codes in the visualisation or delete sections of a coding tool without affecting the coding tool in your review.<br>
	<img class="img" src="assets/Images/EPPIVis_EditTools02.png"><br>
	To edit the wording select the coding tool or code from the left side of the screen and click <b>Edit</b>. Edit the text and click <b>Save</b>.<br>
	If you delete a code it can be restored. Please note that a code deleted from the visualisation is still present and unaffected in your review.
	<br><br>
	</div>
</div>

<div class="row">
	<div class="col-sm">
	<b>View the visualisation</b><br>
	At any time you can preview the visualisation by clicking <b>View in EPPI-Vis</b><br>
	<img class="img" src="assets/Images/EPPIVis_EditButton.png"><br>
	The actual link to your visualisation can be found further down the screen as the <b>Public URL</b><br>
	<img class="img" src="assets/Images/EPPIVis_PublicURL.png">
	<br><br>
	</div>
	<div class="col-sm">
	<b>Example visualisation</b><br>
	<img class="img" src="assets/Images/EPPIVis_example.png">
	<br><br>
	</div>
</div>

<hr>

<div class="row">
	<div class="col-sm">
	<b>Pre-configured maps</b><br>
	<img class="img" src="assets/Images/EPPIVis_PreConfigMap01.png"><br>
	Click <b>Create map</b> to set up a pre-configured map.<br><br>
	</div>
	<div class="col-sm">
	<b>A Map is a graphical, interactive, 3-dimensional visualisation of research evidence in a particular domain.</b><br />
    It is a matrix of rows and columns with categorising segments in each node.<br />
    Set each dimension by picking a code/node from the tools on the left.<br>
	You must specify all 3 dimension along with a map name. You can also enter a map description using the text editor.<br>
	<b>Rows and columns</b> - you can pick any node/code that has at least one child code.<br>
    <b>Segments</b> - you can pick any node/code that has between one and six child code(s).<br>
    <br><br>
	</div>
</div>

<div class="row">
	<div class="col-sm">
	<b>List of maps in EPPI-Vis</b><br>
	<img class="img" src="assets/Images/EPPIVis_PreConfigMap02.png"><br>
	The pre-configured map(s) will be listed on the Home page of EPPI-Vis. You can set up multiple maps.<br>
	Click <b>Details</b> to show the information you entered in the Map description area.<br>
	Click <b>View map</b> to display the map that you set up previously.<br><br>
	</div>
	<div class="col-sm">
	<b>Viewing map in EPPI-Vis</b><br>
	<img class="img" src="assets/Images/EPPIVis_PreConfigMap03.png"><br>
	The map will open as a bubble map but can be changed to a table layout.<br>
	Hovering over a bubble will display a tooltip with the document count.<br>
	Clicking a bubble will list the relevant documents.<br>
	<br><br>
	</div>
</div>


</div>


</div>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'webdbs'

GO

-------------------------------------------------------------------------------------------------
-- itemdetails\arms  (old 496.sql)
-------------------------------------------------------------------------------------------------

USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'itemdetails\arms'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('itemdetails\arms', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Links, Arms and Timepoints</p>
<p>The <b>Links Arms Timepoints</b> tab has functionality to assist in linking related documents, dealing with multiple arm trials and the entry of numeric outcome data at specific timepoints.</p>
<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Linked items</b><br>
	<img class="img"  src="assets/Images/ItemDetails-Link.gif" />
	</div>
	<div class="col-sm">
	<br>
	The <b>Linking</b> function allows you to link <b>secondary</b> items to a <b>primary</b> item. This might be appropriate where there are multiple items/references describing the same study and you wish to carry out data extraction just once per study. The primary item could contain the full study data extraction and the secondary items will be linked to that primary item for easier access.<br>
	Starting from the primary item, create a new <b>link</b> by clicking the <b>Add new Link</b> button. Enter the ID of the secondary item, click <b>Find item</b> to check you have the correct item, enter a description and then click <b>Save</b>. You can link multiple secondary items to the primary item.<br>
	Once an item is linked you can <b>Edit</b> the link, view 👁️ the full reference or delete 🗑️ the link.<br>
	<b>Report</b> will display the primary item and the linked items in a new browser tab.
	</div>
</div>
<br>
<div class="row">
	<div class="col-sm">		
	<b>Arms and Timepoints</b><br>
	<img class="img"  src="assets/Images/ItemDetails-Arms.gif" />
	</div>
	<div class="col-sm">
	<br>
	To create a new <b>arm</b> enter a name for the arm and click the <b>Add New Study Arm</b> button. The new arm will be displayed in a table where you can also <b>Edit</b> or <b>Remove</b> the arm.<br>
	The new arm will also be listed in a dropdown menu at the top of the coding tool panel. An arm is specific to the item it was created for so all coding carried out on the item will be specific to the selected arm. The <b>Whole Study</b> option is for coding not tied to a specific arm.<br><br>  
	To create a <b>timepoint</b> enter a numeric value for the duration, select the time unit from the dropdown menu and click the <b>Add Timepoint</b> button. The new timepoint will be displayed in a table where you can also <b>Edit</b> or <b>Remove</b> the timepoint.<br>
	Timepoints are specific to the item it was created for and are selectable when entering numeric outcome data using <b>Outcome</b>, <b>Intervention</b> and <b>Comparison</b> codes.<br><br>
	When entering numeric outcome data any arms created for the item are also selectable in the on-screen numeric outcome data entry form.<br> 
	</div>
</div>
</div>

<br>
<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'itemdetails\arms'

GO

-------------------------------------------------------------------------------------------------
-- (codingui)main
-------------------------------------------------------------------------------------------------


USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = '(codingui)main'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('(codingui)main', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Review home</b><br>
	<img class="img"  src="assets/Images/CO_Home.gif" />
	</div>
	<div class="col-sm">
	<br>
	From the <b>Review home</b> screen in the coding interface you can access the studies from your coding assignments.<br>
	Your coding assignments are listed on the left of the screen. Clicking on the links for <b>Allocated</b>, <b>Started</b> and <b>Remaining</b> will list the appropriate studies 
	on the right side of the screen.<br>
	You can change the nunber of items in a page and the fields displayed by clicking <b>View options</b> and changing the parameters.<br>
	From View options you can also enable <b>Enhanced selection</b> for selecting multiple items. Further details can be found by clicking the <b>?</b> button in the View options panel.<br>
	Click on <b>Go</b> to access an item in the list of studies.<br>	
	You can also access your reviews by clicking on the <b>My reviews</b> button. From the list you can access a reviews using either the 
	Coding interface or the Full interface.<br>
	Enhanced selection....
	</div>
</div>
<br><br>
</div>

<br>
<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = '(codingui)main'

GO


-------------------------------------------------------------------------------------------------
-- buildmodel
-------------------------------------------------------------------------------------------------

USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'buildmodel'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('buildmodel', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Build Model</p>
<p>You can build your own custom classifiers to identify items based on previous coding.</p>
<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Build Model</b><br>
	<img class="img"  src="assets/Images/BuildModel.gif" /><br><br>
	</div>
	<div class="col-sm">
	<br>
	Custom classifiers allow you to identify items based on previous coding by specifying that you want to find items that are similar to one 
	code but not like another code.<br> 
	To build a model select the similarity code from the top dropdown, the dissimilar code from the second dropdown and give your model a
	name before clicking <b>Build Model</b>.
	When the model is ready to use it will be listed in the Model table.<br>
	The model may take a while to build so you can click <b>Refresh Models</b> to see it it is ready.<br>
	You can also delete selected models using the <b>Delete Selected Model(s)</b> button.<br>
	</div>
</div>
</div>

<br>
<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'buildmodel'

GO



