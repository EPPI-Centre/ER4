USE [ReviewerAdmin]
GO
DECLARE	@return_value int,
		@CONTACT_ID int

Declare @SiteAdmPw varchar(50) = 'theAdminPassword!'
Declare @NormalUserPw varchar(50) = '123'

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'EIS NICE 3',
  @USERNAME = N'EISNICE3',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'EISNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'Medicines NICE 1',
  @USERNAME = N'MedicinesNICE1',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'MedicinesNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'Medicines NICE 2',
  @USERNAME = N'MedicinesNICE2',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'MedicinesNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'Medicines NICE 3',
  @USERNAME = N'MedicinesNICE3',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'MedicinesNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'Updates NICE 1',
  @USERNAME = N'UpdatesNICE1',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'UpdatesNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'Updates NICE 2',
  @USERNAME = N'UpdatesNICE2',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'UpdatesNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'Updates NICE 3',
  @USERNAME = N'UpdatesNICE3',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'UpdatesNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'EIS NICE 1',
  @USERNAME = N'EISNICE1',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'EISNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'EIS NICE 2',
  @USERNAME = N'EISNICE2',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'EISNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'Surveillance NICE 1',
  @USERNAME = N'SurveillanceNICE1',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'SurveillanceNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'Surveillance NICE 2',
  @USERNAME = N'SurveillanceNICE2',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'SurveillanceNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'Surveillance NICE 3',
  @USERNAME = N'SurveillanceNICE3',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'SurveillanceNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'Caroline K',
  @USERNAME = N'CK',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'c.k@ioe.ac.uk',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGA TEST 2',
  @USERNAME = N'NGATEST2',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGA TEST 3',
  @USERNAME = N'NGATEST3',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGA TEST 4',
  @USERNAME = N'NGATEST4',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGA TEST 5',
  @USERNAME = N'NGATEST5',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGA TEST 1',
  @USERNAME = N'NGATEST1',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGATEST 6',
  @USERNAME = N'NGATEST6',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGATEST 9',
  @USERNAME = N'NGATEST9',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGATEST 8',
  @USERNAME = N'NGATEST8',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGATEST 7',
  @USERNAME = N'NGATEST7',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGATEST 12',
  @USERNAME = N'NGATEST12',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGATEST 11',
  @USERNAME = N'NGATEST11',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGATEST 10',
  @USERNAME = N'NGATEST10',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGATEST 16',
  @USERNAME = N'NGATEST16',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGATEST 15',
  @USERNAME = N'NGATEST15',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGATEST 14',
  @USERNAME = N'NGATEST14',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGATEST 13',
  @USERNAME = N'NGATEST13',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGATEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'economicsNICE7',
  @USERNAME = N'economicsNICE7',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'economicsNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'economicsNICE8',
  @USERNAME = N'economicsNICE8',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'economicsNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'economicsNICE9',
  @USERNAME = N'economicsNICE9',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'economicsNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'economicsNICE10',
  @USERNAME = N'economicsNICE10',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'economicsNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'economicsNICE5',
  @USERNAME = N'economicsNICE5',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'economicsNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'economicsNICE6',
  @USERNAME = N'economicsNICE6',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'economicsNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'economicsNICE2',
  @USERNAME = N'economicsNICE2',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'economicsNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'economicsNICE3',
  @USERNAME = N'economicsNICE3',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'economicsNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'economicsNICE4',
  @USERNAME = N'economicsNICE4',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'economicsNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'economicsNICE1',
  @USERNAME = N'economicsNICE1',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'economicsNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GIS NICE 1',
  @USERNAME = N'GISNICE1',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GISNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GIS NICE 2',
  @USERNAME = N'GISNICE2',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GISNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GIS NICE 3',
  @USERNAME = N'GISNICE3',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GISNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GUTNICE6',
  @USERNAME = N'GUTNICE6',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GUTNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GUTNICE7',
  @USERNAME = N'GUTNICE7',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GUTNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GUTNICE8',
  @USERNAME = N'GUTNICE8',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GUTNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GUTNICE9',
  @USERNAME = N'GUTNICE9',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GUTNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GUTNICE10',
  @USERNAME = N'GUTNICE10',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GUTNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GUTNICE11',
  @USERNAME = N'GUTNICE11',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GUTNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GUTNICE1',
  @USERNAME = N'GUTNICE1',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GUTNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GUTNICE2',
  @USERNAME = N'GUTNICE2',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GUTNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GUTNICE3',
  @USERNAME = N'GUTNICE3',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GUTNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GUTNICE4',
  @USERNAME = N'GUTNICE4',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GUTNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'GUTNICE5',
  @USERNAME = N'GUTNICE5',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'GUTNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'CHTE NICE 1',
  @USERNAME = N'CHTENICE1',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'CHTENICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'CHTE NICE 2',
  @USERNAME = N'CHTENICE2',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'CHTENICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'CHTE NICE 3',
  @USERNAME = N'CHTENICE3',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'CHTENICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'CHTE NICE 4',
  @USERNAME = N'CHTENICE4',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'CHTENICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'CHTE NICE 5',
  @USERNAME = N'CHTENICE5',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'CHTENICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'CHTE NICE 6',
  @USERNAME = N'CHTENICE6',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'CHTETEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'CHTE NICE 7',
  @USERNAME = N'CHTENICE7',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'CHTETEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'CHTE NICE 8',
  @USERNAME = N'CHTENICE8',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'CHTETEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'CHTE NICE 9',
  @USERNAME = N'CHTENICE9',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'CHTETEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'CHTE NICE 10',
  @USERNAME = N'CHTENICE10',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'CHTETEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGC TEST 1',
  @USERNAME = N'NGCTEST1',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGCTEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGC TEST 2',
  @USERNAME = N'NGCTEST2',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGCTEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGC TEST 3',
  @USERNAME = N'NGCTEST3',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGCTEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGC TEST 4',
  @USERNAME = N'NGCTEST4',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGCTEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGC TEST 5',
  @USERNAME = N'NGCTEST5',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGCTEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGC TEST 6',
  @USERNAME = N'NGCTEST6',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGCTEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGC TEST 6',
  @USERNAME = N'NGCTEST6',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGCTEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGC TEST 7',
  @USERNAME = N'NGCTEST7',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGCTEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGC TEST 8',
  @USERNAME = N'NGCTEST8',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGCTEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGC TEST 9',
  @USERNAME = N'NGCTEST9',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGCTEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'NGC TEST 10',
  @USERNAME = N'NGCTEST10',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'NGCTEST@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'QS TEST 1',
  @USERNAME = N'QSTEST1',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'QSNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'QS TEST 2',
  @USERNAME = N'QSTEST2',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'QSNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'QS TEST 3',
  @USERNAME = N'QSTEST3',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'QSNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'QS TEST 4',
  @USERNAME = N'QSTEST4',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'QSNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

--not used, original record in Raven does not have a username.
--EXEC @return_value = [dbo].[st_ContactCreate]
--  @CONTACT_NAME = N'User Admin',
--  @USERNAME = N'UserAdmin',
--  @PASSWORD = @SiteAdmPw,
--  @DATE_CREATED = N'2018-03-12 12:00:00.000',
--  @EMAIL = N'admin@EPPIReviewer.org.uk',
--  @DESCRIPTION = N'NICE testing user (converted)',
--  @CONTACT_ID = @CONTACT_ID OUTPUT

--update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
-- where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'ShaileshKumar ',
  @USERNAME = N'skumar',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'ShaileshsFAKEaddres@googlemail.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'DJ',
  @USERNAME = N'DJ',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'dJ@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'Sergio Graziosi',
  @USERNAME = N'sg',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N's.graziosi1@ioe.ac.uk',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'Claire Stansfield',
  @USERNAME = N'CS',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'c.s@ioe.ac.uk',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'James Thomas',
  @USERNAME = N'JT',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'j.thomas@ioe.ac.uk',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'Mark S',
  @USERNAME = N'Msalmon',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'Msalmon@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'PH NICE 3',
  @USERNAME = N'PHNICE3',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'PHNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'PH NICE 4',
  @USERNAME = N'PHNICE4',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'PHNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'PH NICE 5',
  @USERNAME = N'PHNICE5',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'PHNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'PH NICE 1',
  @USERNAME = N'PHNICE1',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'PHNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'PH NICE 2',
  @USERNAME = N'PHNICE2',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'PHNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'PH NICE 9',
  @USERNAME = N'PHNICE9',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'PHNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'PH NICE 10',
  @USERNAME = N'PHNICE10',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'PHNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'PH NICE 6',
  @USERNAME = N'PHNICE6',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'PHNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'PH NICE 7',
  @USERNAME = N'PHNICE7',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'PHNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

EXEC @return_value = [dbo].[st_ContactCreate]
  @CONTACT_NAME = N'PH NICE 8',
  @USERNAME = N'PHNICE8',
  @PASSWORD = @NormalUserPw,
  @DATE_CREATED = N'2018-03-12 12:00:00.000',
  @EMAIL = N'PHNICE@test.com',
  @DESCRIPTION = N'NICE testing user (converted)',
  @CONTACT_ID = @CONTACT_ID OUTPUT

update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = N'2030-01-01 12:00:00.000'
 where CONTACT_ID = @CONTACT_ID

