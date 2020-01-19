declare @Content nvarchar(max) = '
<p class="font-weight-bold">Crosstab report</p>
<p>Using the crosstab report, it is possible to cross-tabulate the answers of one question against another into a matrix to 
find the relationship between certain answers. This function is useful when comparing things such as types of interventions against results.</p>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Crosstab</b><br>
	<img class="img"  src="Images/CrossTabs.gif" /><br><br>
	</div>
	<div class="col-sm">
	<br>
	Select a code or coding tool to be used for each axis and click <b>Set</b> for each axis.<br>
	The crosstab report will be generated based on the child codes below the selected codes. Click on <b>Get CrossTabs</b> to display the report.<br>
	The matrix is displayed with the answers for one question in rows and the answers for the other question in columns. 
	Clicking on the intersection point for two answers will list the documents that have been coded with both answers.<br>
	You can filter the Crosstab by selecting a code and clicking <b>Set filter</b>. Only items that have that code will be displayed in the 
	Crosstab report.<br> 
	The report will include items with <b>I</b> and <b>E</b> flags.<br>
	</div>
</div>	
</div>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'main\crosstabs'

GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Frequency report</p>
<p>Frequency reports allow a reviewer to gain an overview of the answers given to a particular question. It presents all possible answers together with the number of studies which were categorised with that answer and displays them in a table.</p>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Frequency</b><br>
	<img class="img"  src="Images/Frequencies.gif" /><br><br>
	</div>
	<div class="col-sm">
	<br>
	Select a code or coding tool to be used in the report and click <b>Set</b>. The frequency report will be generated based on the child codes below the selected code. Click on <b>Get Frequencies</b> to display the report.<br>
	The frequencies for each code are displayed in a table. Clicking on the frequency will list those items in the References tab.<br>
	You can filter the frequency by selecting a code and clicking <b>Set filter</b>. Only items that have that code will be displayed in the frequency report.<br> 
	You can also filter the report by the item''s <b>I</b> or <b>E</b> flags.<br>
	The report can be displayed as a Table, Pie chart or Bar chart.<br>
	</div>
</div>	
</div>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'main\frequencies'

GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Build Model</p>
<p>You can build your own custom classifiers to identify items based on previous coding.</p>

<div class="container-fluid">

<div class="row">
	<div class="col-sm">		
	<b>Build Model</b><br>
	<img class="img"  src="Images/BuildModel.gif" /><br><br>
	</div>
	<div class="col-sm">
	<br>
	Custom classifiers allow you to identify items based on previous coding by specifying that you want to find items that are similar to one 
	code but not like another code.<br> 
	To build a model select the similarity code from the top dropdown, the dissimilar code from the second dropdown and give your model a
	name before clicking <b>Build Model</b>.
	When the model is ready to use it will be listed in the Model table.<br>
	The model may take a while to build so you can click <b>Refresh Models</b> to see it it is ready.<br><br>
	</div>
</div>
</div>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'buildmodel'

GO