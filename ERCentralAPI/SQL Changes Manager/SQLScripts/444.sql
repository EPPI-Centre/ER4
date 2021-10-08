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
	<img class="img" src="Images/EPPIVis_Create.png"><br>
	To create a new visualisation click <b>Add new</b>, enter a title, a subTitle and click <b>Save</b><br><br>
	<br><br>
	</div>
	<div class="col-sm">		
	<b>Edit visualisation - click Edit!</b> <img class="img" src="Images/EPPIVis_EditButton.png"><br>
	<img class="img" src="Images/EPPIVis_Edit.png"><br>
	Use the text editor to enter a description.<br>
	Click <b>Add</b> for <em>Only items with this code</em> to choose what items will be accessible to the visualisation.<br>
	Toggle <b>Open access</b> to enter a username and password for restricted access.<br>
	Be sure to click <b>Save</b> when you are finished.<br><br>
	</div>	
</div>

<div class="row">
	<div class="col-sm">
	<b>Adding logos</b> - click <b>Edit!</b> then <b>Edit images</b> <img class="img" src="Images/EPPIVis_EditImageButton.png"><br>
	<img class="img" src="Images/EPPIVis_AddLogos.png"><br>
	Select the image location and click <b>Select files</b> to choose and upload an image. Image size restrictions and image caching details 
	are described on the screen. Be sure to click <b>Save</b> when you are finished.<br><br> 
	</div>
	<div class="col-sm">
	<b>Adding logo links</b><br>
	<img class="img" src="Images/EPPIVis_LogoUrls.png"><br>
	Click <b>Edit!</b> and you will see text boxes below the description area.<br> 
	You can enter urls for your logos to make them linkable to a website.<br>
	Enter the full path including https://<br>
	Be sure to click <b>Save</b> when you are finished.<br><br>
	</div>
</div> 

<div class="row">
	<div class="col-sm">
	<b>Add coding tools</b><br>
	<img class="img" src="Images/EPPIVis_AddCodingTools.png"><br>
	Users can explore your data and generate reports based on the coding tools you add to the visualisation.<br>
	Select a coding tool from your review from the dropdown menu.<br>
	Click <b>Add!</b> and it will appear in the list on the left.<br><br>
	</div>
	<div class="col-sm">
	<b>Edit coding tools</b><br>
	<img class="img" src="Images/EPPIVis_EditTools01.png"><br>
	You can edit the wording of the coding tools and codes in the visualisation or delete sections of a coding tool without affecting the coding tool in your review.<br>
	<img class="img" src="Images/EPPIVis_EditTools02.png"><br>
	To edit the wording select the coding tool or code from the left side of the screen and click <b>Edit</b>. Edit the text and click <b>Save</b>.<br>
	If you delete a code it can be restored. Please note that a code deleted from the visualisation is still present and unaffected in your review.
	<br><br>
	</div>
</div>

<div class="row">
	<div class="col-sm">
	<b>View the visualisation</b><br>
	At any time you can preview the visualisation by clicking <b>View in EPPI-Vis</b><br>
	<img class="img" src="Images/EPPIVis_EditButton.png"><br>
	The actual link to your visualisation can be found further down the screen as the <b>Public URL</b><br>
	<img class="img" src="Images/EPPIVis_PublicURL.png">
	<br><br>
	</div>
	<div class="col-sm">
	<b>Example visualisation</b><br>
	<img class="img" src="Images/EPPIVis_example.png">
	<br><br>
	</div>
</div>

<hr>

<div class="row">
	<div class="col-sm">
	<b>Pre-configured maps</b><br>
	<img class="img" src="Images/EPPIVis_PreConfigMap01.png"><br>
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
	<img class="img" src="Images/EPPIVis_PreConfigMap02.png"><br>
	The pre-configured map(s) will be listed on the Home page of EPPI-Vis. You can set up multiple maps.<br>
	Click <b>Details</b> to show the information you entered in the Map description area.<br>
	Click <b>View map</b> to display the map that you set up previously.<br><br>
	</div>
	<div class="col-sm">
	<b>Viewing map in EPPI-Vis</b><br>
	<img class="img" src="Images/EPPIVis_PreConfigMap03.png"><br>
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