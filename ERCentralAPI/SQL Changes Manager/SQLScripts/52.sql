USE [ReviewerAdmin]
GO
delete from TB_ONLINE_HELP
GO

declare @Content nvarchar(max) = '
<p class="font-weight-bold">Welcome-Page Help</p>
<p>This page allows to choose the review you want to work on, and/or create a new (private) review.</p>
<p>To create a new (private) review, please click on the "Create Review", type a name for the review and click "Create".<br />
<span class="small">The new review will be ''private'', to share the review, you will need to purchase a subscription via the 
<a href="https://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2935" target="_blank">online shop</a>.</span></p>
<p>You can open your reviews in 2 ways:
<ol>
<li>To open the Full user inteface, click on the review name.</li>
<li>To open the Coding Interface (optimised for small screens), click on the "Coding UI" button.</li>
</ol>
</p>
'
INSERT into TB_ONLINE_HELP ([CONTEXT]
           ,[HELP_HTML])
     VALUES
           ('intropage'
           ,@Content)
GO
declare @Content nvarchar(max) = '
<p class="font-weight-bold">Review-Home Help</p>
<p>This page contains many commonly used functionalities. On the main toolbar, you''ll find buttons that allow you to import new items, edit your codesets and import codesets.</p>
<p>The "<b>Review Items</b>" and "<b>Coding Progress</b>" panels show summary numbers about the whole review and single codesets.<br />
The codesets can be found by clicking the <span class="bg-success">green "Codes"</span> button on the right hand side.
</p>
<p>Additional functions are:
<ol>
<li><b>My Reviews</b>: allows you to see all your reviews and to open them up in the full or "coding" user interfaces.</li>
<li><b>My Work</b>: allows you access your own Coding Assigments/Work allocation and to access the Priority Screening list (when present).</li>
<li><b>Sources</b>: allows you to see the list of imported sources. Therein you can also list all items from a given source, as well as bulk-delete all items imported with a given source.</li>
</ol>
</p>
'
INSERT into TB_ONLINE_HELP ([CONTEXT]
           ,[HELP_HTML])
     VALUES
           ('main\reviewhome'
           ,@Content)
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">References-tab Help</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
INSERT into TB_ONLINE_HELP ([CONTEXT]
           ,[HELP_HTML])
     VALUES
           ('main\references'
           ,@Content)
GO



declare @Content nvarchar(max) = '<p class="font-weight-bold">Frequencies-tab Help</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
INSERT into TB_ONLINE_HELP ([CONTEXT]
           ,[HELP_HTML])
     VALUES
           ('main\frequencies'
           ,@Content)
GO



declare @Content nvarchar(max) = '<p class="font-weight-bold">Crosstabs Help</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
INSERT into TB_ONLINE_HELP ([CONTEXT]
           ,[HELP_HTML])
     VALUES
           ('main\crosstabs'
           ,@Content)
GO



declare @Content nvarchar(max) = '<p class="font-weight-bold">Search-tab Help</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
INSERT into TB_ONLINE_HELP ([CONTEXT]
           ,[HELP_HTML])
     VALUES
           ('main\search'
           ,@Content)
GO


