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
	This page provides <b>basic support</b> for the most used deduplication functionalities. It <b>does not</b> yet support all the advanced functionalities available in EPPI-Reviewer 4. The functionalities available here mirror precisely those in EPPI-Reviewer 4, please refer to the <a href="https://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2933" target="_blank">user manual</a> to learn the details.
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
			 <li>Advanced mark automatically (allows to change thresholds).</li>
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
	Thus, it''s possible that handling duplicates will still require to occasionally use <a href="http://eppi.ioe.ac.uk/eppireviewer4/" target="_blank">EPPI-Reviewer 4</a>.
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