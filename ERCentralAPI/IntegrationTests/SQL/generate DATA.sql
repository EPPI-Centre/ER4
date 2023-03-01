--Script to populate an EMPTY tempTestReviewer database with minimum data.
--This is the LAST script to run, and Requires to already have a copy of tempTestReviewer (uses it to create users).
--INSTRUCTIONS: (15/06/2016)
--BEFORE YOU START: set passwords!
-- find the following text: "Declare @SiteAdmPw varchar(50)", set the two passwords thereabout.


--FIRST phase: static data

USE [tempTestReviewer]
GO
SET IDENTITY_INSERT [dbo].[TB_IMPORT_FILTER] ON 

GO
INSERT [dbo].[TB_IMPORT_FILTER] ([IMPORT_FILTER_ID], [IMPORT_FILTER_NAME], [IMPORT_FILTER_NOTES], [STARTOFNEWREC], [TYPEFIELD], [STARTOFNEWFIELD], [TITLE], [PTITLE], [SHORTTITLE], [DATE], [MONTH], [AUTHOR], [PARENTAUTHOR], [STANDARDN], [CITY], [PUBLISHER], [INSTITUTION], [VOLUME], [ISSUE], [EDITION], [STARTPAGE], [ENDPAGE], [PAGES], [AVAILABILITY], [URL], [ABSTRACT], [OLD_ITEM_ID], [NOTES], [DEFAULTTYPECODE], [DOI], [KEYWORDS]) VALUES (3, N'RIS', NULL, N'TY  -', N'TY  -', N'[A-Z][0-Z]  -', N'(T1|TI|CT)  -', N'(JA|JF|T2|BT|T3|JO)  -', N'\\M\\w', N'[PY][1Y]  -', N'Y2  -', N'A[U1]  -', N'(A2|ED)  -', N'SN  -', N'CY  -', N'PB  -', N'AD  - ', N'VL  -', N'(IS|CP)  -', N'\\M\\w', N'SP  -', N'EP  -', N'\\M\\w', N'AV  -', N'(UR|L1|L2)  -', N'(N2|AB)  -', N'(U1|ID)  -', N'(N1|U5|U4)  -', 14, N'DO  -', N'KW  -')
GO
INSERT [dbo].[TB_IMPORT_FILTER] ([IMPORT_FILTER_ID], [IMPORT_FILTER_NAME], [IMPORT_FILTER_NOTES], [STARTOFNEWREC], [TYPEFIELD], [STARTOFNEWFIELD], [TITLE], [PTITLE], [SHORTTITLE], [DATE], [MONTH], [AUTHOR], [PARENTAUTHOR], [STANDARDN], [CITY], [PUBLISHER], [INSTITUTION], [VOLUME], [ISSUE], [EDITION], [STARTPAGE], [ENDPAGE], [PAGES], [AVAILABILITY], [URL], [ABSTRACT], [OLD_ITEM_ID], [NOTES], [DEFAULTTYPECODE], [DOI], [KEYWORDS]) VALUES (4, N'PubMed', NULL, N'PMID-', N'PT  -', N'^[A-Za-z][A-Za-z][A-Za-z\s][A-Za-z\s]-', N'(T1|TI|CT)  -', N'JT  -', N'\\M\\w', N'DP  -', N'\\M\\w', N'A[U1]  -', N'(A2|ED)  -', N'(IS  - |PMID-)', N'PL  -', N'PB  -', N'CN  -', N'VI  -', N'IP  -', N'\\M\\w', N'SP  -', N'EP  -', N'PG  -', N'AV  -', N'(UR|L1|L2)  -', N'(N2|AB)  -', N'\\M\\w', N'GN  -', 14, N'[L|A]ID -(?=.*?\[doi\])¬\[doi\]', N'(MH|OT)  -')
GO
INSERT [dbo].[TB_IMPORT_FILTER] ([IMPORT_FILTER_ID], [IMPORT_FILTER_NAME], [IMPORT_FILTER_NOTES], [STARTOFNEWREC], [TYPEFIELD], [STARTOFNEWFIELD], [TITLE], [PTITLE], [SHORTTITLE], [DATE], [MONTH], [AUTHOR], [PARENTAUTHOR], [STANDARDN], [CITY], [PUBLISHER], [INSTITUTION], [VOLUME], [ISSUE], [EDITION], [STARTPAGE], [ENDPAGE], [PAGES], [AVAILABILITY], [URL], [ABSTRACT], [OLD_ITEM_ID], [NOTES], [DEFAULTTYPECODE], [DOI], [KEYWORDS]) VALUES (5, N'RefWorks', NULL, N'^RT ', N'RT ', N'^[A-Z][0-Z] ', N'T1 ', N'(JF|T2) ', N'ST', N'YR ', N'FD ', N'A1 ', N'A[2-5] ', N'SN ', N'PP ', N'PB ', N'\\M\\w', N'VL ', N'IS ', N'ED ', N'SP ', N'OP ', N'\\M\\w', N'AV ', N'UL ', N'AB ', N'\\M\\w', N'NO ', 12, N'DO ', N'K1 ')
GO
INSERT [dbo].[TB_IMPORT_FILTER] ([IMPORT_FILTER_ID], [IMPORT_FILTER_NAME], [IMPORT_FILTER_NOTES], [STARTOFNEWREC], [TYPEFIELD], [STARTOFNEWFIELD], [TITLE], [PTITLE], [SHORTTITLE], [DATE], [MONTH], [AUTHOR], [PARENTAUTHOR], [STANDARDN], [CITY], [PUBLISHER], [INSTITUTION], [VOLUME], [ISSUE], [EDITION], [STARTPAGE], [ENDPAGE], [PAGES], [AVAILABILITY], [URL], [ABSTRACT], [OLD_ITEM_ID], [NOTES], [DEFAULTTYPECODE], [DOI], [KEYWORDS]) VALUES (6, N'Web of Science', N'as in: http://images.webofknowledge.com/WOKRS57B4/help/WOS/hs_wos_fieldtags.html', N'^PT ', N'PT ', N'^[A-Z][0-Z] ', N'TI ', N'SO ', N'\\M\\w', N'P[D|Y] ', N'PD ', N'AU ', N'BA ', N'(SN|BN|UT) ', N'PI ', N'PU ', N'\\M\\w', N'VL', N'IS', N'\\M\\w', N'BP', N'EP', N'\\M\\w', N'\\M\\w', N'\\M\\w', N'AB ', N'\\M\\w', N'SC', 14, N'D[I|2] ', N'(DE|ID) ')
GO
INSERT [dbo].[TB_IMPORT_FILTER] ([IMPORT_FILTER_ID], [IMPORT_FILTER_NAME], [IMPORT_FILTER_NOTES], [STARTOFNEWREC], [TYPEFIELD], [STARTOFNEWFIELD], [TITLE], [PTITLE], [SHORTTITLE], [DATE], [MONTH], [AUTHOR], [PARENTAUTHOR], [STANDARDN], [CITY], [PUBLISHER], [INSTITUTION], [VOLUME], [ISSUE], [EDITION], [STARTPAGE], [ENDPAGE], [PAGES], [AVAILABILITY], [URL], [ABSTRACT], [OLD_ITEM_ID], [NOTES], [DEFAULTTYPECODE], [DOI], [KEYWORDS]) VALUES (7, N'psycINFO', N'', N'[ ]{0,5}Record: [0-9]+', N'AT- |PT- ', N'^[A-Z][0-Z]- ', N'TI- ', N'SO- ', N'\\M\\w', N'YR- |SD- ', N'SD-', N'AU- ', N'PA- ¬[(]', N'IB- |SN- ', N'PU- ¬:', N'PU- [^:]*:|PU- ', N'\\M\\w', N'VI- ', N'IP- ', N'\\M\\w', N'SP- ', N'EP- ', N'PG- ', N'MT- |MA- ', N'UR- ¬[ \r\n]', N'AB- ', N'\\M\\w', N'KP- |AG -', 12, N'\\M\\w', N'\\M\\w')
GO
SET IDENTITY_INSERT [dbo].[TB_IMPORT_FILTER] OFF
GO
SET IDENTITY_INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ON 

GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (7, 3, N'Edition', N'VL  -', 2)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (8, 3, N'Volume', N'\\M\\w', 2)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (9, 3, N'Edition', N'VL  -', 4)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (10, 3, N'Volume', N'\\M\\w', 4)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (11, 3, N'Institution', N'PB  -', 4)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (12, 3, N'Publisher', N'\\M\\w', 4)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (13, 4, N'Edition', N'VL  -', 2)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (14, 4, N'Volume', N'\\M\\w', 2)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (15, 4, N'Edition', N'VL  -', 4)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (16, 4, N'Volume', N'\\M\\w', 4)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (17, 4, N'Institution', N'PB  -', 4)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (18, 4, N'Publisher', N'\\M\\w', 4)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (19, 5, N'Institution', N'PB ', 4)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (20, 5, N'Publisher', N'\\M\\w', 4)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (21, 7, N'pTitle', N'PB- ', 3)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (36, 4, N'Title', N'BTI -', 2)
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] ([TB_IMPORT_FILTER_TYPE_RULE_ID], [IMPORT_FILTER_ID], [RULE_NAME], [RULE_REGEX], [TYPE_CODE]) VALUES (37, 4, N'pTitle', N'CTI -', 2)
GO
SET IDENTITY_INSERT [dbo].[TB_IMPORT_FILTER_TYPE_RULE] OFF
GO
SET IDENTITY_INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ON 

GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (21, 3, 14, N'JOUR|JFULL')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (22, 3, 1, N'RPRT|PAMP|PCOMM|UNPB')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (23, 3, 2, N'BOOK')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (24, 3, 3, N'CHAP')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (25, 3, 4, N'THES')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (26, 3, 5, N'CONF')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (27, 3, 6, N'ELEC|ICOMM')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (28, 3, 8, N'ADVS|VIDEO|ART|MPCT|MUSIC|SOUND|SLIDE')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (29, 3, 10, N'MGZN|NEWS')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (30, 3, 12, N'ABST|BILL|CASE|COMP|CTLG|DATA|GEN|HEAR|INPR|MAP|PAT|STAT|UNBILl')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (31, 4, 14, N'Journal Article')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (32, 5, 14, N'Journal Article|Journal, Electronic|Abstract')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (33, 5, 1, N'Report|Unpublished Material|Personal Communication')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (34, 5, 2, N'Book, Edited|Book, Whole|Monograph')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (35, 5, 3, N'Book, Section')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (36, 5, 4, N'Dissertation|Thesis|Dissertation/Thesis|Dissertation/Thesis, Unpublished|Dissertation, Unpublished|Thesis, Unpublished')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (37, 5, 5, N'Conference Proceedings')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (38, 5, 6, N'Web Page')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (39, 5, 8, N'Artwork|Motion Picture|Music Score|Sound Recording|Video/DVD|Video|DVD')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (40, 5, 9, N'Grant')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (41, 5, 10, N'Magazine Article|Newspaper Article')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (42, 5, 12, N'Bills/Resolutions|Bills|Resolutions|Case/Court Decisions|Case Decisions|Court Decisions|Computer Program|Generic|Hearing|Laws|Laws/Statutes|Statutes|Map|Online Discussion Forum|Patent')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (46, 7, 14, N'Journal Article|[jJ]ournal Citation|Review-Book|Review-Media')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (47, 7, 4, N'Dissertation')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (48, 7, 2, N'Book')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (49, 7, 3, N'Chapter|Encyclopedia Entry')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (55, 6, 14, N'J')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (56, 6, 3, N'B|S')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (57, 6, 12, N'P')
GO
INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] ([IMPORT_FILTER_TYPE_MAP_ID], [IMPORT_FILTER_ID], [TYPE_CODE], [TYPE_REGEX]) VALUES (58, 4, 2, N'Book')
GO
SET IDENTITY_INSERT [dbo].[TB_IMPORT_FILTER_TYPE_MAP] OFF
GO
INSERT [dbo].[TB_ATTRIBUTE_TYPE] ([ATTRIBUTE_TYPE_ID], [ATTRIBUTE_TYPE], [OLD_IS_ANSWER]) VALUES (1, N'Not selectable (no checkbox)', N'N')
GO
INSERT [dbo].[TB_ATTRIBUTE_TYPE] ([ATTRIBUTE_TYPE_ID], [ATTRIBUTE_TYPE], [OLD_IS_ANSWER]) VALUES (2, N'Selectable (show checkbox)', N'Y')
GO
INSERT [dbo].[TB_ATTRIBUTE_TYPE] ([ATTRIBUTE_TYPE_ID], [ATTRIBUTE_TYPE], [OLD_IS_ANSWER]) VALUES (3, N'Selectable (future radiobutton - N/A)', N'Y')
GO
INSERT [dbo].[TB_ATTRIBUTE_TYPE] ([ATTRIBUTE_TYPE_ID], [ATTRIBUTE_TYPE], [OLD_IS_ANSWER]) VALUES (4, N'Outcome', N'Y')
GO
INSERT [dbo].[TB_ATTRIBUTE_TYPE] ([ATTRIBUTE_TYPE_ID], [ATTRIBUTE_TYPE], [OLD_IS_ANSWER]) VALUES (5, N'Intervention', N'Y')
GO
INSERT [dbo].[TB_ATTRIBUTE_TYPE] ([ATTRIBUTE_TYPE_ID], [ATTRIBUTE_TYPE], [OLD_IS_ANSWER]) VALUES (6, N'Comparison', N'Y')
GO
INSERT [dbo].[TB_ATTRIBUTE_TYPE] ([ATTRIBUTE_TYPE_ID], [ATTRIBUTE_TYPE], [OLD_IS_ANSWER]) VALUES (7, N'Selectable (future Numeric value - N/A)', N'Y')
GO
INSERT [dbo].[TB_ATTRIBUTE_TYPE] ([ATTRIBUTE_TYPE_ID], [ATTRIBUTE_TYPE], [OLD_IS_ANSWER]) VALUES (8, N'Selectable (future Mark as included - N/A)', N'Y')
GO
INSERT [dbo].[TB_ATTRIBUTE_TYPE] ([ATTRIBUTE_TYPE_ID], [ATTRIBUTE_TYPE], [OLD_IS_ANSWER]) VALUES (9, N'Outcome classification code', N'N')
GO
INSERT [dbo].[TB_ATTRIBUTE_TYPE] ([ATTRIBUTE_TYPE_ID], [ATTRIBUTE_TYPE], [OLD_IS_ANSWER]) VALUES (10, N'Include', N'Y')
GO
INSERT [dbo].[TB_ATTRIBUTE_TYPE] ([ATTRIBUTE_TYPE_ID], [ATTRIBUTE_TYPE], [OLD_IS_ANSWER]) VALUES (11, N'Exclude', N'Y')
GO
SET IDENTITY_INSERT [dbo].[TB_SET_TYPE] ON 

GO
INSERT [dbo].[TB_SET_TYPE] ([SET_TYPE_ID], [SET_TYPE], [SET_DESCRIPTION], [ALLOW_COMPARISON], [MAX_DEPTH], [ACCEPTS_RANDOM_ALLOCATIONS]) VALUES (3, N'Standard', N'The Standard codeset type is used for regular coding such as keywording or data-extraction. This codeset type can contain multiple levels of child codes but cannot contain the special code types "Include" and "Exclude".', 1, 10, 1)
GO
INSERT [dbo].[TB_SET_TYPE] ([SET_TYPE_ID], [SET_TYPE], [SET_DESCRIPTION], [ALLOW_COMPARISON], [MAX_DEPTH], [ACCEPTS_RANDOM_ALLOCATIONS]) VALUES (5, N'Screening', N'The screening codeset type has been designed to simplify coding comparisons by restricting it to the code types "Include" and "Exclude" and only allowing one level of hierarchy.', 1, 1, 0)
GO
INSERT [dbo].[TB_SET_TYPE] ([SET_TYPE_ID], [SET_TYPE], [SET_DESCRIPTION], [ALLOW_COMPARISON], [MAX_DEPTH], [ACCEPTS_RANDOM_ALLOCATIONS]) VALUES (6, N'Administration', N'The Administration codeset type is used for setting up codesets for activities such as Allocation, Retrieval and Reports. It only allows Selectable and Non-selectable code types and cannot be used for comparison coding.', 0, 10, 1)
GO
SET IDENTITY_INSERT [dbo].[TB_SET_TYPE] OFF
GO
SET IDENTITY_INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ON 

GO
INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ([SET_TYPE_ATTRIBUTE_TYPE_ID], [SET_TYPE_ID], [ATTRIBUTE_TYPE_ID]) VALUES (1, 3, 1)
GO
INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ([SET_TYPE_ATTRIBUTE_TYPE_ID], [SET_TYPE_ID], [ATTRIBUTE_TYPE_ID]) VALUES (2, 3, 2)
GO
INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ([SET_TYPE_ATTRIBUTE_TYPE_ID], [SET_TYPE_ID], [ATTRIBUTE_TYPE_ID]) VALUES (3, 3, 3)
GO
INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ([SET_TYPE_ATTRIBUTE_TYPE_ID], [SET_TYPE_ID], [ATTRIBUTE_TYPE_ID]) VALUES (4, 3, 4)
GO
INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ([SET_TYPE_ATTRIBUTE_TYPE_ID], [SET_TYPE_ID], [ATTRIBUTE_TYPE_ID]) VALUES (5, 3, 5)
GO
INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ([SET_TYPE_ATTRIBUTE_TYPE_ID], [SET_TYPE_ID], [ATTRIBUTE_TYPE_ID]) VALUES (6, 3, 6)
GO
INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ([SET_TYPE_ATTRIBUTE_TYPE_ID], [SET_TYPE_ID], [ATTRIBUTE_TYPE_ID]) VALUES (7, 3, 7)
GO
INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ([SET_TYPE_ATTRIBUTE_TYPE_ID], [SET_TYPE_ID], [ATTRIBUTE_TYPE_ID]) VALUES (8, 3, 8)
GO
INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ([SET_TYPE_ATTRIBUTE_TYPE_ID], [SET_TYPE_ID], [ATTRIBUTE_TYPE_ID]) VALUES (9, 3, 9)
GO
INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ([SET_TYPE_ATTRIBUTE_TYPE_ID], [SET_TYPE_ID], [ATTRIBUTE_TYPE_ID]) VALUES (10, 5, 10)
GO
INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ([SET_TYPE_ATTRIBUTE_TYPE_ID], [SET_TYPE_ID], [ATTRIBUTE_TYPE_ID]) VALUES (11, 5, 11)
GO
INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ([SET_TYPE_ATTRIBUTE_TYPE_ID], [SET_TYPE_ID], [ATTRIBUTE_TYPE_ID]) VALUES (12, 6, 1)
GO
INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] ([SET_TYPE_ATTRIBUTE_TYPE_ID], [SET_TYPE_ID], [ATTRIBUTE_TYPE_ID]) VALUES (13, 6, 2)
GO
SET IDENTITY_INSERT [dbo].[TB_SET_TYPE_ATTRIBUTE_TYPE] OFF
GO
SET IDENTITY_INSERT [dbo].[TB_SET_TYPE_PASTE] ON 

GO
INSERT [dbo].[TB_SET_TYPE_PASTE] ([SET_TYPE_PASTE_ID], [DEST_SET_TYPE_ID], [SRC_SET_TYPE_ID]) VALUES (1, 3, 3)
GO
INSERT [dbo].[TB_SET_TYPE_PASTE] ([SET_TYPE_PASTE_ID], [DEST_SET_TYPE_ID], [SRC_SET_TYPE_ID]) VALUES (2, 5, 5)
GO
INSERT [dbo].[TB_SET_TYPE_PASTE] ([SET_TYPE_PASTE_ID], [DEST_SET_TYPE_ID], [SRC_SET_TYPE_ID]) VALUES (3, 6, 6)
GO
INSERT [dbo].[TB_SET_TYPE_PASTE] ([SET_TYPE_PASTE_ID], [DEST_SET_TYPE_ID], [SRC_SET_TYPE_ID]) VALUES (4, 3, 6)
GO
SET IDENTITY_INSERT [dbo].[TB_SET_TYPE_PASTE] OFF
GO
INSERT [dbo].[TB_ITEM_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (1, N'Report', N'Old Code was E', N'Consider using Dissertation or Research Project when appropriate')
GO
INSERT [dbo].[TB_ITEM_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (2, N'Book, Whole', N'', N'')
GO
INSERT [dbo].[TB_ITEM_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (3, N'Book, Chapter', N'', N'')
GO
INSERT [dbo].[TB_ITEM_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (4, N'Dissertation', N'Old Code Was G', N'Use the Edition field to specify the thesis type')
GO
INSERT [dbo].[TB_ITEM_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (5, N'Conference Proceedings', N'Old Code Was K', N'')
GO
INSERT [dbo].[TB_ITEM_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (6, N'Document From Internet Site', N'This is a single document', N'Date fields refer to publication date, if known')
GO
INSERT [dbo].[TB_ITEM_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (7, N'Web Site', N'This is a multi-page website ', N'URL field should contain the root of the site hierarchy, date fields should refer to last visited date')
GO
INSERT [dbo].[TB_ITEM_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (8, N'DVD, Video, Media', N'', N'')
GO
INSERT [dbo].[TB_ITEM_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (9, N'Research project', N'', N'')
GO
INSERT [dbo].[TB_ITEM_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (10, N'Article In A Periodical', N'Newspapers, magazines and similar. Old codes are "PER" and F(newspaper)', N'')
GO
INSERT [dbo].[TB_ITEM_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (11, N'Interview', N'Old code was INT', N'Author role = 0 is for the Interviewer, Author role = 1 for the Interviewee')
GO
INSERT [dbo].[TB_ITEM_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (12, N'Generic', N'Whatever does not fit any of the above, only old code that fits here is H(trade catalogue)', N'')
GO
INSERT [dbo].[TB_ITEM_TYPE] ([TYPE_ID], [TYPE_NAME], [DESCRIPTION], [NOTES]) VALUES (14, N'Journal, Article', N'Used also for old "Journal, Whole", D(Journal short form) and "JOUR" codes', N'')
GO
INSERT [dbo].[TB_META_ANALYSIS_TYPE] ([META_ANALYSIS_TYPE_ID], [META_ANALYSIS_TYPE_TITLE]) VALUES (0, N'Continuous: d (Hedges g)')
GO
INSERT [dbo].[TB_META_ANALYSIS_TYPE] ([META_ANALYSIS_TYPE_ID], [META_ANALYSIS_TYPE_TITLE]) VALUES (1, N'Continuous: r')
GO
INSERT [dbo].[TB_META_ANALYSIS_TYPE] ([META_ANALYSIS_TYPE_ID], [META_ANALYSIS_TYPE_TITLE]) VALUES (2, N'Binary: odds ratio')
GO
INSERT [dbo].[TB_META_ANALYSIS_TYPE] ([META_ANALYSIS_TYPE_ID], [META_ANALYSIS_TYPE_TITLE]) VALUES (3, N'Binary: risk ratio')
GO
INSERT [dbo].[TB_META_ANALYSIS_TYPE] ([META_ANALYSIS_TYPE_ID], [META_ANALYSIS_TYPE_TITLE]) VALUES (4, N'Binary: risk difference')
GO
INSERT [dbo].[TB_META_ANALYSIS_TYPE] ([META_ANALYSIS_TYPE_ID], [META_ANALYSIS_TYPE_TITLE]) VALUES (5, N'Binary: diagnostic test OR')
GO
INSERT [dbo].[TB_META_ANALYSIS_TYPE] ([META_ANALYSIS_TYPE_ID], [META_ANALYSIS_TYPE_TITLE]) VALUES (6, N'Binary: Peto OR')
GO
INSERT [dbo].[TB_META_ANALYSIS_TYPE] ([META_ANALYSIS_TYPE_ID], [META_ANALYSIS_TYPE_TITLE]) VALUES (7, N'Continuous: mean difference')
GO
INSERT [dbo].[TB_OUTCOME_TYPE] ([OUTCOME_TYPE_ID], [OUTCOME_TYPE_NAME]) VALUES (0, N'Manual entry')
GO
INSERT [dbo].[TB_OUTCOME_TYPE] ([OUTCOME_TYPE_ID], [OUTCOME_TYPE_NAME]) VALUES (1, N'Continuous: Ns, means and SD')
GO
INSERT [dbo].[TB_OUTCOME_TYPE] ([OUTCOME_TYPE_ID], [OUTCOME_TYPE_NAME]) VALUES (2, N'Binary: 2 x 2 table')
GO
INSERT [dbo].[TB_OUTCOME_TYPE] ([OUTCOME_TYPE_ID], [OUTCOME_TYPE_NAME]) VALUES (3, N'Continuous: N, Mean, SE')
GO
INSERT [dbo].[TB_OUTCOME_TYPE] ([OUTCOME_TYPE_ID], [OUTCOME_TYPE_NAME]) VALUES (4, N'Continuous: N, Mean, CI')
GO
INSERT [dbo].[TB_OUTCOME_TYPE] ([OUTCOME_TYPE_ID], [OUTCOME_TYPE_NAME]) VALUES (5, N'Continuous: N, t- or p-value')
GO
INSERT [dbo].[TB_OUTCOME_TYPE] ([OUTCOME_TYPE_ID], [OUTCOME_TYPE_NAME]) VALUES (6, N'Diagnostic test: 2 x 2 table')
GO
INSERT [dbo].[TB_OUTCOME_TYPE] ([OUTCOME_TYPE_ID], [OUTCOME_TYPE_NAME]) VALUES (7, N'Correlation coefficient r')
GO
INSERT [dbo].[TB_REVIEW_ROLE] ([ROLE_NAME], [ROLE_DESCR]) VALUES (N'AdminUser', N'Has full rights over the review')
GO
INSERT [dbo].[TB_REVIEW_ROLE] ([ROLE_NAME], [ROLE_DESCR]) VALUES (N'Coding only', N'This is a role that forces users to be able to assign codes to items in their work allocations, and nothing else')
GO
INSERT [dbo].[TB_REVIEW_ROLE] ([ROLE_NAME], [ROLE_DESCR]) VALUES (N'ReadOnlyUser', N'Read Only access to the review')
GO
INSERT [dbo].[TB_REVIEW_ROLE] ([ROLE_NAME], [ROLE_DESCR]) VALUES (N'RegularUser', N'Has ordinary rights, but can''t modify rights of other users')
GO


--Second Phase: seed the required tables



INSERT INTO [tempTestReviewer].[dbo].[TB_CONTACT]
           ([old_contact_id]
           ,[CONTACT_NAME]
           ,[USERNAME]
           ,[PASSWORD]
           ,[LAST_LOGIN]
           ,[DATE_CREATED]
           ,[EMAIL]
           ,[EXPIRY_DATE]
           ,[MONTHS_CREDIT]
           ,[CREATOR_ID]
           ,[TYPE]
           ,[IS_SITE_ADMIN]
           ,[DESCRIPTION]
           ,[SEND_NEWSLETTER]
           ,[FLAVOUR]
           ,[PWASHED]
           )
     VALUES
           (NULL
           ,'seedContact'
           ,'seedContact'
           ,''
           ,NULL
           ,GETDATE()
           ,'seedContact@email'
           ,GETDATE()
           ,0
           ,1
           ,'Professional'
           ,0
           ,'seedContact'
           ,0
           ,'p#%Hg.ancJèrb:1&Jblw'
           ,0xB28538632CB80C39FDD93F6080071D3B6BB26B3C
           )
GO




INSERT INTO [tempTestReviewer].[dbo].[TB_REVIEW]
           ([REVIEW_NAME]
           ,[DATE_CREATED]
           ,[MONTHS_CREDIT]
           ,[FUNDER_ID]
           )
     VALUES
           ('SeedReview'
           ,GETDATE()
           ,0
           ,1
           )
GO

INSERT INTO [tempTestReviewer].[dbo].[TB_SOURCE]
           ([SOURCE_NAME]
           ,[REVIEW_ID]
           ,[IS_DELETED]
           ,[DATE_OF_SEARCH]
           ,[DATE_OF_IMPORT]
           ,[SOURCE_DATABASE]
           ,[SEARCH_DESCRIPTION]
           ,[SEARCH_STRING]
           ,[NOTES]
           ,[IMPORT_FILTER_ID])
     VALUES
           ('SeedSource'
           ,(select top 1 REVIEW_ID from TB_REVIEW order by REVIEW_ID asc)
           ,0
           ,GETDATE()
           ,GETDATE()
           ,''
           ,''
           ,''
           ,''
           ,(select top 1 IMPORT_FILTER_ID from TB_IMPORT_FILTER order by IMPORT_FILTER_ID asc)
           )
GO

INSERT INTO [tempTestReviewer].[dbo].[TB_ITEM]
           ([TYPE_ID]
           ,[TITLE]
           ,[SHORT_TITLE]
           ,[DATE_CREATED]
           ,[CREATED_BY]
           ,[DATE_EDITED]
           ,[EDITED_BY]
           ,[YEAR]
           ,[MONTH])
     VALUES
           ( (select top 1 TYPE_ID from TB_ITEM_TYPE order by TYPE_ID asc)
           ,'SeedItem'
           ,'SeedI'
           ,GETDATE()
           ,(select top 1 CONTACT_NAME from TB_CONTACT order by CONTACT_ID asc)
           ,GETDATE()
           ,(select top 1 CONTACT_NAME from TB_CONTACT order by CONTACT_ID asc)
           ,'1900'
           ,'1')
GO
INSERT INTO [tempTestReviewer].[dbo].[TB_ITEM_AUTHOR]
           ([ITEM_ID]
           ,[LAST]
           ,[FIRST]
           ,[SECOND]
           ,[ROLE]
           ,[RANK])
     VALUES
           (( select top 1 ITEM_ID from TB_ITEM order by ITEM_ID asc)
           ,'Seed'
           ,'Author'
           ,''
           ,0
           ,1)
GO

INSERT INTO [tempTestReviewer].[dbo].[TB_ITEM_SOURCE]
           ([ITEM_ID]
           ,[SOURCE_ID])
     VALUES
           ((select top 1 ITEM_ID from TB_ITEM order by ITEM_ID asc)
           ,(select top 1 SOURCE_ID from TB_SOURCE order by SOURCE_ID asc)
           )
GO


INSERT INTO [tempTestReviewer].[dbo].[TB_ITEM_REVIEW]
           ([ITEM_ID]
           ,[REVIEW_ID]
           ,[IS_INCLUDED]
           ,[IS_DELETED])
     VALUES
           ((select top 1 ITEM_ID from TB_ITEM order by ITEM_ID asc)
           , (select top 1 REVIEW_ID from TB_REVIEW order by REVIEW_ID asc)
           ,1
           ,0)
GO
--[OPTIONAL] seeding contacts to high number to avoid clashes with real ER users (better safe than sorry).
--DBCC CHECKIDENT ('TB_CONTACT', RESEED, 500000);  
GO  


USE [tempTestReviewerAdmin]
GO

Declare @SiteAdmPw varchar(50) = 'aa123'
Declare @NormalUserPw varchar(50) = '123'
Declare @now datetime = GetDate()

DECLARE	@return_value int,
		@CONTACT_ID int

EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Default SuperUser',
		@USERNAME = N'SuperC',
		@PASSWORD = @SiteAdmPw,
		@DATE_CREATED = @now,
		@EMAIL = N'fakeEmal@ucl.ac.uk',
		@DESCRIPTION = N'Default SuperUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

--SELECT	@CONTACT_ID as N'@CONTACT_ID'

--SELECT	'Return Value' = @return_value
update tempTestReviewer.dbo.TB_CONTACT set IS_SITE_ADMIN = 1, EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID

EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Default NormalUser',
		@USERNAME = N'NormalC',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'fakeEmal2@ucl.ac.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID

EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Alice Fake',
		@USERNAME = N'alice',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'alice@EPPIReviewer.org.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID

EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Bob Fake',
		@USERNAME = N'bob',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'bob@EPPIReviewer.org.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID

EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Steve Fake',
		@USERNAME = N'steve',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'steve@EPPIReviewer.org.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID




EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Jane Fake',
		@USERNAME = N'Jane',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'Jane@EPPIReviewer.org.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID



EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'John Fake',
		@USERNAME = N'john',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'john@EPPIReviewer.org.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID


EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Tracy Fake',
		@USERNAME = N'tracy',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'tracy@EPPIReviewer.org.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID



EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'David Fake',
		@USERNAME = N'david',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'david@expired.org.uk',
		@DESCRIPTION = N'Default NormalUser)',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID


SET @now = DATEADD(month, -1, @now)

EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Mary Fake',
		@USERNAME = N'mary',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'mary@expired.org.uk',
		@DESCRIPTION = N'Default NormalUser (expired)',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(day, -1, GETDATE())
	where CONTACT_ID = @CONTACT_ID

GO



USE [tempTestReviewerAdmin]
GO

INSERT INTO [dbo].[TB_LATEST_SERVER_MESSAGE]
           ([MESSAGE]
           )
     VALUES
           ('Normal'
           )
GO

USE [tempTestReviewerAdmin]
GO

INSERT INTO [dbo].[TB_UPDATE_MSG]
           ([VERSION_NUMBER]
           ,[DESCRIPTION]
           ,[URL]
           )
     VALUES
           ('4.0.0.0'
           ,'First message, nothing to read, here.'
           ,'https://eppi.ioe.ac.uk'
           )
GO
--commented as IDs don't agree with at least one SProc
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('No extension', 'This No change to the extension date', '111', '0')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Purchase', 'Extension due to purchase', '111', '1')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Staff', 'Extension for EPPI staff', '111', '2')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Maintenance', 'Extension for network down time', '111', '3')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Making review non-shareable', 'Change review to non-shareable', '010', '4')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Has budget code', 'The project has a budget code without definate expiry date', '111', '5')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Restart trial', 'The user never used their trial access', '111', '6')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Return read-only access', 'Move the expiry date to less than two months in the past', '111', '7')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Working on EPPI-Centre project', 'The user is working on an EPPI-Centre project', '111', '8')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Activate review', 'The review has been activated', '111', '9')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Activate user account', 'The user account has been activated', '111', '10')
--GO