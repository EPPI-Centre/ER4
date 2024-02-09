Use Reviewer
GO

BEGIN TRANSACTION
--Check if LibraryId is nullable, if it is, this script has already done its job
declare @chk int = (SELECT count(*)
	FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'TB_ZOTERO_REVIEW_CONNECTION' and COLUMN_NAME = 'LibraryId' and IS_NULLABLE = 'NO')

IF @chk = 1 
BEGIN
	Print('LibraryId is not nullable, attempting to apply all changes')
	--check if the old index on LibraryId exists, drop it if so, as we're re-creating it
	SELECT @chk = count(*)
		FROM sys.indexes 
		WHERE name='UIX_TB_ZOTERO_REVIEW_CONNECTION_LibraryId' AND object_id = OBJECT_ID('[dbo].[TB_ZOTERO_REVIEW_CONNECTION]')
	If @chk = 1 
	BEGIN
		Print('LibraryId index exist, attempting to drop it')
		DROP INDEX [UIX_TB_ZOTERO_REVIEW_CONNECTION_LibraryId] ON [dbo].[TB_ZOTERO_REVIEW_CONNECTION]
	END

	Print('Attempting to make LibraryId nullable')
	ALTER TABLE TB_ZOTERO_REVIEW_CONNECTION ALTER COLUMN LibraryId NVARCHAR(50) NULL

	Print('Setting LibraryId values to null, where they are an empty string')
	UPDATE TB_ZOTERO_REVIEW_CONNECTION set LibraryId = null where LibraryId = '' 
	
	Print('Re-creating the LibraryId (FILTERED) index')
	CREATE UNIQUE NONCLUSTERED INDEX [UIX_TB_ZOTERO_REVIEW_CONNECTION_LibraryId] ON [dbo].[TB_ZOTERO_REVIEW_CONNECTION]
	(
		[LibraryId] ASC
	) 
	where [LibraryId] IS NOT NULL
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
ELSE Print('LibraryId is nullable, nothing to do')

COMMIT

GO
