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
	To create a new visualisation click <b>Add new</b>, enter a title, a subTitle and click <b>Save</b><br>
	Add any time you can preview the visualisation by clicking <b>View in EPPI-Vis</b><br>
	<img class="img" src="Images/EPPIVis_EditButton.png"><br>
	The actual link to your visualisation can be found further down the screen as the <b>Public URL</b><br>
	<img class="img" src="Images/EPPIVis_PublicURL.png"><br><br>
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
	<b>Adding logos - click Edit! then Edit images</b> <img class="img" src="Images/EPPIVis_EditImageButton.png"><br>
	<img class="img" src="Images/EPPIVis_Logos.png"><br>
	Select the image number (up to 2) and click <b>Select files</b> to choose and upload an image.<br>
	Image size restrictions and image caching details are described on the screen.<br>
	Be sure to click <b>Save</b> when you are finished.<br><br> 
	</div>
	<div class="col-sm">
	<b>Adding logo links</b><br>
	<img class="img" src="Images/EPPIVis_LogosUrls.png"><br>
	Click <b>Edit!</b> and you will see two text boxes below the description area.<br> 
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



</div>


</div>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'webdbs'

GO