Use ReviewerAdmin
GO

declare @check int = 0
set @check = (select count(online_help_id) from TB_ONLINE_HELP where HELP_HTML like '%assets/Images/%')
if @check = 0
begin
    --select 'I''ll do it'
	update TB_ONLINE_HELP set HELP_HTML = REPLACE(HELP_HTML, 'Images/', 'assets/Images/') 
end
--else select 'not doing it'
--select HELP_HTML from TB_ONLINE_HELP
GO