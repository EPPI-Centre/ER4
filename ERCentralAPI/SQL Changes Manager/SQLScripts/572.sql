Use Reviewer
Go

update TB_IMPORT_FILTER set SHORTTITLE = '\\M\\w' where 
	(IMPORT_FILTER_ID = 3 and IMPORT_FILTER_NAME = 'RIS')
	OR (IMPORT_FILTER_ID = 8 and IMPORT_FILTER_NAME = 'OVID RIS')

GO

declare @id int = (select IMPORT_FILTER_ID from TB_IMPORT_FILTER where IMPORT_FILTER_NAME = 'RIS extended')
--select @id
if @id is not null And @id > 0 
BEGIN
	print 'will delete ''RIS extended'''
	delete from TB_IMPORT_FILTER_TYPE_MAP where IMPORT_FILTER_ID = @id 
	delete from TB_IMPORT_FILTER_TYPE_RULE where IMPORT_FILTER_ID = @id 
	delete from TB_IMPORT_FILTER where IMPORT_FILTER_ID = @id 
	BEGIN TRY  
	 declare @seed int = (select MAX(IMPORT_FILTER_ID) from TB_IMPORT_FILTER)
     DBCC CHECKIDENT(TB_IMPORT_FILTER, RESEED, @seed)
	 print 'Reseeded!'
	END TRY  
	BEGIN CATCH  
		 print 'Could not reseed!'
	END CATCH  
END

--begin transaction
insert into TB_IMPORT_FILTER select 
	'RIS extended'
    ,'Besides what''s in ''OVID RIS'', it also imports ''Short title'' data, as well as more tags/fields as they appear in the Wikipedia Page for RIS.'
    ,[STARTOFNEWREC]
    ,[TYPEFIELD]
    ,[STARTOFNEWFIELD]
    ,[TITLE]
    ,[PTITLE]
    ,[SHORTTITLE]
    ,[DATE]
    ,[MONTH]
    ,[AUTHOR]
    ,[PARENTAUTHOR]
    ,[STANDARDN]
    ,[CITY]
    ,[PUBLISHER]
    ,[INSTITUTION]
    ,[VOLUME]
    ,[ISSUE]
    ,[EDITION]
    ,[STARTPAGE]
    ,[ENDPAGE]
    ,[PAGES]
    ,[AVAILABILITY]
    ,[URL]
    ,[ABSTRACT]
    ,[OLD_ITEM_ID]
    ,[NOTES]
    ,[DEFAULTTYPECODE]
    ,[DOI]
    ,[KEYWORDS]
FROM [TB_IMPORT_FILTER] where IMPORT_FILTER_ID = 8
set @id = SCOPE_IDENTITY()
insert into TB_IMPORT_FILTER_TYPE_MAP select @id, TYPE_CODE, TYPE_REGEX from TB_IMPORT_FILTER_TYPE_MAP where IMPORT_FILTER_ID = 8

insert into TB_IMPORT_FILTER_TYPE_RULE select @id, RULE_NAME, RULE_REGEX, TYPE_CODE from TB_IMPORT_FILTER_TYPE_RULE where IMPORT_FILTER_ID = 8
insert into TB_IMPORT_FILTER_TYPE_RULE select @id, 'pTitle', '(JA|JF|T2|BT|T3|JO|WT)  -', 6 
insert into TB_IMPORT_FILTER_TYPE_RULE select @id, 'pTitle', '(JA|JF|T2|BT|T3|JO|WT)  -', 7 

update TB_IMPORT_FILTER set [DATE] = '(PY|Y1|YR)  -'
 ,SHORTTITLE = 'ST  -'
 ,CITY = '(CY|PP)  -'
 ,VOLUME = '(VL|VO)  -'
 ,EDITION = 'ET  -'
 ,[AVAILABILITY] = '(AV|DB|DP|DS|SR)  -'
 ,NOTES = '(N1|U5|NO)  -'
 ,KEYWORDS = '(KW|K1)  -'
WHERE IMPORT_FILTER_ID = @id

--select * from TB_IMPORT_FILTER where IMPORT_FILTER_ID = @id
--select * from TB_IMPORT_FILTER_TYPE_MAP where IMPORT_FILTER_ID = @id
--select * from TB_IMPORT_FILTER_TYPE_RULE where IMPORT_FILTER_ID = @id

--commit transaction

--set @id = (select MAX(IMPORT_FILTER_ID) from TB_IMPORT_FILTER)
--select * from TB_IMPORT_FILTER where IMPORT_FILTER_ID = @id
--select * from TB_IMPORT_FILTER_TYPE_MAP where IMPORT_FILTER_ID = @id
--select * from TB_IMPORT_FILTER_TYPE_RULE where IMPORT_FILTER_ID = @id


GO