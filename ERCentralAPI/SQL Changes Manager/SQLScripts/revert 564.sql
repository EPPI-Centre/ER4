USE Reviewer
GO

select * from TB_ZOTERO_REVIEW_CONNECTION
declare @chk int = (SELECT count(*)
	FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'TB_ZOTERO_REVIEW_CONNECTION' and COLUMN_NAME = 'LibraryId' and IS_NULLABLE = 'YES')
If @chk = 1 
BEGIN
	Print('LibraryId is nullable, attempting to apply/revert all changes')

	SELECT @chk = count(*)
		FROM sys.indexes 
		WHERE name='UIX_TB_ZOTERO_REVIEW_CONNECTION_LibraryId' AND object_id = OBJECT_ID('[dbo].[TB_ZOTERO_REVIEW_CONNECTION]')
	If @chk = 1 
	BEGIN
		Print('LibraryId index exist, attempting to drop it')
		DROP INDEX [UIX_TB_ZOTERO_REVIEW_CONNECTION_LibraryId] ON [dbo].[TB_ZOTERO_REVIEW_CONNECTION]
	END

	Print('Putting placeholder values in TB_ZOTERO_REVIEW_CONNECTION.LibraryId where they had NULL value')
	update rc set LibraryId = CAST(-ZOTERO_CONNECTION_ID as NVARCHAR(50))
	from TB_ZOTERO_REVIEW_CONNECTION rc
	where LibraryId is NULL

	select * from TB_ZOTERO_REVIEW_CONNECTION

	ALTER TABLE TB_ZOTERO_REVIEW_CONNECTION ALTER COLUMN LibraryId NVARCHAR(50) NOT NULL
	Print('Re-creating the LibraryId (NOT filtered!!) index')
	CREATE UNIQUE NONCLUSTERED INDEX [UIX_TB_ZOTERO_REVIEW_CONNECTION_LibraryId] ON [dbo].[TB_ZOTERO_REVIEW_CONNECTION]
	(
		[LibraryId] ASC
	) 
	--where [LibraryId] IS NOT NULL
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

END
ELSE Print('LibraryId is not nullable, nothing to do')


--AFTER running the actual script, you can re-instate things as they were by running the commented bit below.
--NOTE: you may or may not be able to set values to "empty string ''" or NULL depending on whether you have only one NULL value (then you can) or more (you can't)
--Use/edit the command below if/as needed.
--update rc set LibraryId = NULL 
--from TB_ZOTERO_REVIEW_CONNECTION rc
--where CAST(LibraryId as int) = - ZOTERO_CONNECTION_ID