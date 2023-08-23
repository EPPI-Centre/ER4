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
Apologies, there currently is no help for the ''Meta-Analysis'' page.<br>
Please check this help page later: we are adding new Help content regularly.
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'metaanalysis'

GO

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
Apologies, there currently is no help for the ''Run Meta-Analysis'' page.<br>
Please check this help page later: we are adding new Help content regularly.
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'metaanalysis\run'

GO

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
Apologies, there currently is no help for the ''Network Meta-Analysis'' page.<br>
Please check this help page later: we are adding new Help content regularly.
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'metaanalysis\runnetwork'

GO