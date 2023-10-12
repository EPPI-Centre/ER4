USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'metaanalysis'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('metaanalysis', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Create and Edit Meta-analysis</p>
<p>From this page you can create new and edit existing meta-analyes. This includes setting the type of meta-analysis and selecting the outcomes 
that it will contain. A number of filtering options are available to help identify and select the appropriate outcomes.</p>
<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>New and Existing Meta-analysis</b><br>
	<img class="img"  src="assets/Images/CreateEditMA.png" />
	</div>
	<div class="col-sm">
	<br>
	Click on <b>New MA</b> to create a new meta-analysis. Enter a name for the meta-analysis and select a type from the dropdown menu.<br>
	Click <b>Save</b> and the meta-analysis will be added to the table of existing meta-analyses.<br> 
	To edit or delete an existing meta-analysis, click the <b>Edit/Run</b> button or the trash icon.<br> 
	All of the outcomes in your review are visible in the Outcomes Table. You can select the outcomes appropriate to your meta-analysis using the 
	checkbox in each row. As well, each column is sortable and filterable.<br>
	Please note that the <b>Save</b> button will become enabled and clickable whenever a change has been made to a Meta-analysis. 
	</div>
</div>
<br>
<div class="row">
	<div class="col-sm">		
	<b>Filtering outcomes</b><br>
	<img class="img"  src="assets/Images/FilterMA.png" />
	</div>
	<div class="col-sm">
	<br>
	Outcome filtering is useful when you have many, perhaps hundreds, of outcomes and you need a way to identify the ones relevant 
	to you meta-analysis.<br>
	To filter a column, click on the filter icon at the top of a column in the Outcomes Table. You can also click the <b>Edit Filters</b> button and 
	select the column you wish to filter on.<br>
	From the filter screen you can select the individual outcomes to display in the Outcomes table.<br>
	You can also use up to 2 text filters per column, each with a number of powerful selection criteria, to filter the outcomes. 
	</div>
</div>
<br>
<div class="row">
	<div class="col-sm">		
	<b>Adding Columns</b><br>
	<img class="img"  src="assets/Images/AddingColumns.png" />
	</div>
	<div class="col-sm">
	<br>
	Another way to filter outcomes is by assigning individual codes to each outcome during the coding process. This process could be used to group
	outcomes before running meta-analyses.<br>
	To display the codes assigned to outcomes, click the <b>Add Column</b> button. Select whether you are looking at ''Answer'' codes (an indiviudal code) 
	or ''Question'' codes (the child codes under a parent code).<br>
	Select the code from the coding tool in the dropdown menu and it will appear as a column in the Outcomes table.<br>
	You can now use that column as a filter to identify appropriate outcomes.<br>
	</div>
</div>
<br>
<div class="row">
	<p>
	To run a meta-analysis, click the <b>Run</b> button. More help content is available once you click on that button.<br>
	To run a Network meta-analysis, click the <b>Network MA (Run)</b> button. More help content is available once you click on that button.
	</p>
</div>
</div>

<br>
<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'metaanalysis'

GO


------------------------------------------------------------------------------------------------------------

USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'metaanalysis\run'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('metaanalysis\run', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Running a Meta-analysis</p>
<p>From this screen you can set a number of options for running your meta-analysis. This will affect the numerical results and 
the viusals displayed in the report. You can also set moderators where primary studies may differ from each other.</p>
<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Meta-analysis and visualisation options</b><br>
	<img class="img"  src="assets/Images/RunMAScreen.png" />
	</div>
	<div class="col-sm">
	<br>
	The meta-analysis results are provided by <a href="https://metafor-project.org/doku.php/metafor" target="_blank">The metafor Package: A Meta-Analysis Package for R.</a><br>
	Further details about some of the options can also be found <a href="https://search.r-project.org/CRAN/refmans/metafor/html/00Index.html" target="_blank">here.</a><br>
	</div>
</div>
<br>
<div class="row">
	<div class="col-sm">		
	<b>Meta-analysis options:</b><br>
	<b>Model</b> - pick the model driving the analysis and forest plot<br>
	<b>Output Style</b> - sets the verbosity of information returned in the report<br>
	<b>Significance Level</b> - can range from 75 to 99<br>
	<b>Decimal places</b> - can range from 2 to 8<br>
	<b>Rank correlation test</b> -  carries out the rank correlation test as described by Begg and Mazumdar.<br>
	<b>Egger''s regress</b> - can be used to carry out Egger''s regression test for funnel plot asymmetry<br>
	<b>Display fit statistics</b> - functions to extract the log-likelihood, deviance, AIC, BIC, and AICc values<br>
	<b>Trim and Fill</b> - The trim and fill method is a nonparametric (rank-based) data augmentation technique proposed by Duval and Tweedie<br>
	<b>Knapp & Hartung adjustment</b> - makes use of this method<br>
	<b>Display confidence intervals</b> - computes confidence intervals for the model coefficients and/or other parameters in the model<br>
	</div>
	<div class="col-sm">
	<b>Forest plot options:</b><br>
	<b>X Axis Title</b> - customise the X-axis title in the forest plot<br>
	<b>Summary Estimate (Title)</b> - customise the summary estimate title in the forest plot<br>
	<b>Add annotations</b> - display the values data in the forest plot<br>
	<b>Show summary estimate...</b> - display the summary estimate value in the forest plot<br>
	<b>Show boxplot and QQ graphs</b> - include these visualisations in the report<br>
	<b>Show weights along annotations</b> - show the % weight with the values data in the forest plot<br>
	<b>Show credibility/prediction levels</b> -  this information will be displayed in the visualisation<br>
	<b>Show funnel plots</b> - include this visualisation in the report<br>
	</div>
</div>
<br>
<div class="row">
	<div class="col-sm">		
	<b>Moderators</b><br>
	<img class="img"  src="assets/Images/Moderators.png" />
	</div>
	<div class="col-sm">
	<br>
	Click <b>Moderators</b> to open the Moderators table. This is a table of the outcome columns.<br>
	You can select a column and chose if it will be a factor. The Reference dropdown contains the options in the column for all outcomes.<br>
	If you pick a moderator that won''t work (given the outcomes in the MA) the <b>Run</b> button is disabled, 	with an explanation.
	</div>
</div>
<br>
<div class="row">
	<p>
	To run a Network meta-analysis, click the <b>Network MA (Run)</b> button. More help content is available once you click on that button.
	</p>
</div>
</div>

<br>
<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'metaanalysis\run'

GO

---------------------------------------------------------------------------

USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'metaanalysis\runnetwork'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('metaanalysis\runnetwork', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Running a Network Meta-analysis</p>
<p>From this screen you have access to a number of functons to build a "network" of comparisons for running a Network Meta-analysis. 
For example, if we have an outcome/study that measures the difference between "drug A" and "placebo", 
and another one that measures the difference between "drug B" and "placebo", we can statistically estimate the difference between "drug A" and "drug B".</p>
<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Network Meta-analysis</b><br>
	<img class="img"  src="assets/Images/NetworkMA.png" />
	</div>
	<div class="col-sm">
	<br>
	Click on <b>Network MA (run)</b> to open the Network Meta-analysis options panel.<br>
	You can select the analysis model (Fixed Effect or Random Effect) and the Forest plot reference (ie. the available interventions).<br>
	There is also a <b>Large values are good</b> option that affects the (short) "Intervention Ranking" section in the results.
	</div>
</div>
<br>
<div class="row">
	<div class="col-sm">		
	<b>Mapping (selected) Outcomes</b><br>
	<img class="img"  src="assets/Images/MapNetworkMA.png" />
	</div>
	<div class="col-sm">
	<br>
	Before you can run a network meta-analysis you must click the <b>Map (selected) outcomes</b> button.<br>
	Clicking this button will activate a set of UI features that analyse the network(s) specified by the selected outcomes.<br>
	It will look for incomplete outcomes and give you the option to remove them from your network.<br>
	It will also determine if you have created multiple networks and provide an <b>Unselect</b> button to remove them from the analysis.<br>
	To help understand your selections, the Intervention and Comparator pairs are presented and the number of outcomes for each pair are displayed.<br>
	Some reviews may contain many outcomes making it difficult to see which ones are selected. If you click the <b>Show Selected Outcomes</b> 
	button, only the selected outcomes will be listed in a table.<br>		
	</div>
</div>
<br>
<div class="row">
	<div class="col-sm">		
	<b>Running the Network Meta-analysis</b><br>
	<img class="img"  src="assets/Images/RunNetworkMA.png" />
	</div>
	<div class="col-sm">
	<br>
	<b>Please note</b>: The UI data checking features can only determine if you have selected ''runable'' data. It <b>cannot</b> determine if your network 
	is sensible with respect to your data and it is the responsability of the reviewer to make that determination.<br>
	If your selections are ''runable'' the <b>Run</b> button will be enabled.<br>
	The output will include Intervention Ranking, Network Characteristics, a Network Graph, Forest Plot, along with other visualisations.<br>
	As with running a normal meta-analysis, you can save your results as an html file.
	</div>
</div>

</div>

<br>
<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'metaanalysis\runnetwork'

GO



