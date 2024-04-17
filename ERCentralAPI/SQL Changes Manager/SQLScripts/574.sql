Use [Reviewer]
GO

update TB_IMPORT_FILTER set IMPORT_FILTER_NOTES = 'Standard RIS import filter, suitable in most cases. Follows the original specification closely, while supporting most RIS "dialects".' 
	where IMPORT_FILTER_ID = 3
update TB_IMPORT_FILTER set IMPORT_FILTER_NOTES = N'Implements the MEDLINE/PubMed Journal Article Citation Format.' 
	where IMPORT_FILTER_ID = 4
update TB_IMPORT_FILTER set IMPORT_FILTER_NOTES = N'A very old tagged format, where each reference starts with the "RT" tag.' 
	where IMPORT_FILTER_ID = 5
update TB_IMPORT_FILTER set IMPORT_FILTER_NOTES = N'As described in: http://images.webofknowledge.com/WOKRS57B4/help/WOS/hs_wos_fieldtags.html' 
	where IMPORT_FILTER_ID = 6
update TB_IMPORT_FILTER set IMPORT_FILTER_NOTES = N'An old tagged format where each reference starts with "Record: NN".' 
	where IMPORT_FILTER_ID = 7
update TB_IMPORT_FILTER set IMPORT_FILTER_NOTES = N'Identical to the standard RIS filter, but maps "ELEC" reference type to "journal article".' 
	where IMPORT_FILTER_ID = 8

GO


