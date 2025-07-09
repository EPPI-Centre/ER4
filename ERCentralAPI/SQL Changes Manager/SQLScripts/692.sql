USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP where CONTEXT = 'openalex\bringuptodate'
if @@rowcount = 0
begin
	-- add a new row to TB_EXTENSION_TYPES
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	values ('openalex\bringuptodate', 
			'<b>Bring up-to-date</b><br>' +
			'Apologies, there currently is no help for this page/activity.<br>' +
			'Please check this help page later: we are adding new Help content regularly.'
			)
end

USE [ReviewerAdmin]
GO
select * from TB_ONLINE_HELP where CONTEXT = 'openalex\keepupdated'
if @@rowcount = 0
begin
	-- add a new row to TB_EXTENSION_TYPES
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	values ('openalex\keepupdated', 
			'<b>Keep up-to-date</b><br>' +
			'Apologies, there currently is no help for this page/activity.<br>' +
			'Please check this help page later: we are adding new Help content regularly.'
			)
end

USE [ReviewerAdmin]
GO
select * from TB_ONLINE_HELP where CONTEXT = 'openalex\history'
if @@rowcount = 0
begin
	-- add a new row to TB_EXTENSION_TYPES
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	values ('openalex\history', 
			'<b>History</b><br>' +
			'Apologies, there currently is no help for this page/activity.<br>' +
			'Please check this help page later: we are adding new Help content regularly.'
			)
end

USE [ReviewerAdmin]
GO
select * from TB_ONLINE_HELP where CONTEXT = 'openalex\matching'
if @@rowcount = 0
begin
	-- add a new row to TB_EXTENSION_TYPES
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	values ('openalex\matching', 
			'<b>Match records</b><br>' +
			'Apologies, there currently is no help for this page/activity.<br>' +
			'Please check this help page later: we are adding new Help content regularly.'
			)
end

USE [ReviewerAdmin]
GO
select * from TB_ONLINE_HELP where CONTEXT = 'openalex\search'
if @@rowcount = 0
begin
	-- add a new row to TB_EXTENSION_TYPES
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	values ('openalex\search', 
			'<b>Search and browse</b><br>' +
			'Apologies, there currently is no help for this page/activity.<br>' +
			'Please check this help page later: we are adding new Help content regularly.'
			)
end

USE [ReviewerAdmin]
GO
select * from TB_ONLINE_HELP where CONTEXT = 'openalex\selectedPapers'
if @@rowcount = 0
begin
	-- add a new row to TB_EXTENSION_TYPES
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	values ('openalex\SelectedPapers', 
			'<b>Paper listings</b><br>' +
			'Apologies, there currently is no help for this page/activity.<br>' +
			'Please check this help page later: we are adding new Help content regularly.'
			)
end