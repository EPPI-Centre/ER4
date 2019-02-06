USE [ReviewerAdmin]
GO
delete from 
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