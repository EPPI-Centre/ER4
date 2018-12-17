DECLARE @revID nvarchar(50)
--set @revID = 'REV371'
--set @revID = 'REV292' 
--set @revID = 'REV253'
--set @revID = 'REV241'
--set @revID = 'REV285'
--set @revID = 'REV127'
set @revID =  'REV236'

USE [reviewer]


/* NOTE: you need to add the source server to the known linked servers, i.e.:
EXEC sp_addlinkedserver epi2 */


/* ******* TODO / BUGS ********

1. CHANGE SCRIPT TO OPERATE PER REVIEW
2. TB_ITEM / AUTHORS TRANSFER



DELETE FROM reviewer.DBO.TB_REVIEW_SET
DELETE FROM reviewer.DBO.TB_ITEM_ATTRIBUTE_TEXT
DELETE FROM reviewer.DBO.TB_ITEM_ATTRIBUTE
DELETE FROM reviewer.DBO.TB_ITEM_SET
DELETE FROM reviewer.DBO.TB_ATTRIBUTE_SET
DELETE FROM reviewer.DBO.TB_ATTRIBUTE_TYPE
DELETE FROM reviewer.DBO.TB_SET
DELETE FROM reviewer.DBO.TB_SET_TYPE
DELETE FROM reviewer.DBO.TB_ATTRIBUTE
DELETE FROM reviewer.DBO.TB_ITEM_REVIEW
DELETE FROM reviewer.DBO.TB_CONTACT_REVIEW_ROLE
DELETE FROM reviewer.DBO.TB_REVIEW_CONTACT
DELETE FROM reviewer.DBO.TB_REVIEW_ROLE
DELETE FROM reviewer.DBO.TB_CONTACT
DELETE FROM reviewer.DBO.TB_ITEM_SOURCE
DELETE FROM reviewer.DBO.TB_SOURCE
DELETE FROM reviewer.DBO.tb_ITEM_DOCUMENT
DELETE FROM reviewer.DBO.TB_REVIEW
DELETE FROM reviewer.DBO.TB_ITEM
DELETE FROM reviewer.DBO.TB_TEMP_ITEM
DELETE FROM reviewer.DBO.TB_ITEM_TYPE
*/


INSERT INTO reviewer.DBO.TB_REVIEW(
 OLD_REVIEW_ID,
 OLD_REVIEW_GROUP_ID, 
 REVIEW_NAME, 
 DATE_CREATED, 
 FUNDER_ID, 
 REVIEW_NUMBER,
 [EXPIRY_DATE])
SELECT REVIEW_ID,
 REVIEW_GROUP_ID, 
 REVIEW, 
 DATE_REVIEW_CREATED, 
 FUNDER_ID, 
 REVIEW_NUMBER ,
 DATEADD(M,6, GETDATE())
FROM EPPI_WEB.DBO.TB_REVIEW s where s.review_id = @revID

INSERT INTO reviewer.DBO.TB_REVIEW_CONTACT(OLD_CONTACT_ID, OLD_REVIEW_ID, CONTACT_ID, REVIEW_ID)
SELECT CONTACT_ID, REVIEW_ID, 
		(SELECT reviewer.DBO.TB_CONTACT.CONTACT_ID 
			FROM reviewer.DBO.TB_CONTACT
			WHERE reviewer.DBO.TB_CONTACT.OLD_CONTACT_ID = src.CONTACT_ID COLLATE Latin1_General_CI_AS),
		(SELECT reviewer.DBO.TB_REVIEW.REVIEW_ID FROM reviewer.DBO.TB_REVIEW
			WHERE reviewer.DBO.TB_REVIEW.OLD_REVIEW_ID = src.REVIEW_ID COLLATE Latin1_General_CI_AS)
	FROM EPPI_WEB.DBO.TB_REVIEW_CONTACT as src where src.REVIEW_ID = @revID


INSERT INTO [reviewer].[dbo].[TB_CONTACT_REVIEW_ROLE]([REVIEW_CONTACT_ID],[ROLE_NAME])
SELECT (SELECT 
			REVIEW_CONTACT_ID FROM TB_REVIEW_CONTACT WHERE
			 reviewer.DBO.TB_REVIEW_CONTACT.OLD_CONTACT_ID = src.CONTACT_ID COLLATE Latin1_General_CI_AS
			 AND reviewer.DBO.TB_REVIEW_CONTACT.OLD_REVIEW_ID = src.REVIEW_ID COLLATE Latin1_General_CI_AS) as s_ID,
		ROLE_NAME = CASE
			WHEN ([ROLE] = 'LEADER') THEN 'AdminUser'
			ELSE 'RegularUser'
		END
	FROM EPPI_WEB.DBO.TB_REVIEW_CONTACT as src where src.REVIEW_ID = @revID


/* ************* BEGIN SERGIO'S ITEM TRANSFER CODE ******************** */

-- removed references to database er4testing (we 'use' the database at the beginning of the script)

--POPULATE TEMP TABLE: this applies to the real table tb_TEMP_ITEM where "temp" is just a name,
--can be easily adapted to use are genuine #temp table, or a @table variable.
delete from tb_TEMP_ITEM
DBCC CHECKIDENT('tb_TEMP_ITEM', RESEED, 0)
insert 
into tb_TEMP_ITEM

(
	TYPE_ID,
	ISWEB,
	mediumToEd,
	titleSource,
	ParentTSource,
	PUBDATA,
	INSTITUTION,
	ITEM_ID,
	ITEM,
	ITEM_DESCRIPTION,
	TYPE_CODE,
	DATE_CREATED,
	DATE_EDITED,
	CREATED_BY,
	EDITED_BY,
	AUTHOR_ANALYTIC,
	TITLE_ANALYTIC,
	MEDIUM,
	AUTHOR_MONO,
	AUTHOR_ROLE,
	TITLE_MONO,
	JOURNAL,
	TRANS_NEWS_TITLE,
	PLACE,
	EDITION,
	PLACE_OF_PUB,
	PUBLISHER,
	DATE_OF_PUB,
	VOLUME,
	REPORT_ID,
	ISSUE,
	PAGES,
	EXTENT_OF_WORK,
	CONTACT_DETAILS,
	SERIES_TITLE,
	SERIES_VOLUME,
	SERIES_ISSUE,
	WRITTEN_LANGUAGE,
	AVAILABILITY,
	LOCATION,
	EPIC_NO,
	ISSN,
	ISBN,
	NOTES,
	ABSTRACT,
	AGE_RANGE,
	SHORT_TITLE,
	ITEM_IDENTITY,
	CONFIDENTIAL_CONTACT_ID,
	IMPORTED_REF_ID
)

select 
	'TYPE_ID' = CASE
     WHEN (type_code = 'Journal, Article'
			or type_code = 'D'
			or type_code = 'JOUR      '
			or type_code = 'Journal, Whole'
			or (type_code = 'PUB' and (JOURNAL != '' and JOURNAL is not null)) ) THEN 14  
     WHEN (type_code = 'E') THEN 1
	 WHEN (type_code = 'Book, Whole') THEN 2 
     WHEN (type_code = 'Book, Chapter') THEN 3
	 WHEN (type_code = 'G') THEN 4  
     WHEN (type_code = 'K') THEN 5
	 WHEN (type_code = 'PUB' and (JOURNAL = '' or JOURNAL is null)) THEN 1 
	 WHEN (type_code = 'INT       ') THEN 11
	 WHEN (type_code = 'Electronic Citation'
			and ISWEB > 0) THEN 7
	 WHEN (type_code = 'Electronic Citation'
			and journal is not null and journal != '') THEN 0
	 WHEN (type_code = 'Electronic Citation'
			and (journal is null or journal = '')) THEN 8
	 WHEN (type_code = 'Research project') THEN 9
	 WHEN (type_code = 'PER       ' or type_code = 'F') THEN 10
	 WHEN (type_code = 'DVD, Video, Media' and isweb = 0) THEN 8
	 WHEN (type_code = 'DVD, Video, Media' and isweb > 0) THEN 7
	 WHEN (type_code = 'H') THEN 12
	 ELSE 12 
	END,
	*
from ( select 
			'isweb' = CASE 
				 WHEN ((len(AVAILABILITY) > 5 and left(AVAILABILITY, 4) = 'www.')
							or (len(AVAILABILITY) > 7 and left(AVAILABILITY, 7) = 'http://')
							or (len(AVAILABILITY) > 8 and left(AVAILABILITY, 8) = 'https://')
							) 
				 THEN 1
				 WHEN ((len(LOCATION) > 5 and left(LOCATION, 4) = 'www.')
							or (len(LOCATION) > 7 and left(LOCATION, 7) = 'http://')
							or (len(LOCATION) > 5 and left(LOCATION, 5) = 'http')
							or (len(LOCATION) > 8 and left(LOCATION, 8) = 'https://')
							)
				 THEN 2
				 WHEN ((len(JOURNAL) > 5 and left(JOURNAL, 4) = 'www.')
							or (len(JOURNAL) > 7 and left(JOURNAL, 7) = 'http://')
							or (len(JOURNAL) > 8 and left(JOURNAL, 8) = 'https://')
							)
				 THEN 3
				 ELSE 0 
			END,
			mediumToEd = CASE
				when (type_code = 'G' and (Journal = '' or Journal is null) and (medium is not null and medium !=''))
				then 1
				else 0
			END,
			titleSource = CASE
				when (type_code = 'Book, Whole' and (title_mono != '' and title_mono is not null))
				then 'title_mono'	
				else 'title_analytic'
			END,
			ParentTSource = CASE
				when (type_code = 'PUB' and Journal != '' and Journal is not null)
					or 
					(
						(type_code = 'Journal, Article' or type_code = 'Journal, Whole'
						or type_code = 'D' or  type_code = 'jour' ) and 
						( journal is not null and journal !='') 
					) or (
						type_code = 'K' and JOURNAL != '' and JOURNAL is not null and 
							(title_mono not like '155%' or title_mono is null)
					) or (
						type_code = 'PUB' and (JOURNAL != '' and JOURNAL is not null)
					)  or (
						type_code = 'Electronic Citation' and (JOURNAL != '' and JOURNAL is not null) 
						and not ((len(JOURNAL) > 5 and left(JOURNAL, 4) = 'www.')
							or (len(JOURNAL) > 7 and left(JOURNAL, 7) = 'http://')
							or (len(JOURNAL) > 8 and left(JOURNAL, 8) = 'https://')
							)
					)
					then 'Journal'
				when (
						(type_code = 'Book, Whole' and (SERIES_TITLE is null or SERIES_TITLE = ''))
						or (type_code = 'G' and (Journal is null or journal = '') 
							and (title_mono is  null or title_mono = ''))) then '0'
				when (type_code = 'Book, Whole' and SERIES_TITLE is not null and SERIES_TITLE != '') then 'SERIES_TITLE'
				when (type_code = 'G' and Journal is not null and journal !='') then 'Journal'	
				else 'TITLE_MONO'
			END,
			PubData = CASE
				when (date_of_PUB like 'win [0-9][0-9][0-9][0-9]'	
						or date_of_PUB like 'sum [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'wnt [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'spr [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'fal [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'aut [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'win [0-9][0-9][0-9][0-9][,.:; ¦|\/]'	
						or date_of_PUB like 'sum [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'wnt [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'spr [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'fal [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'aut [0-9][0-9][0-9][0-9][,.:; ¦|\/]')
					then substring(DATE_OF_PUB, 5,4)
				when (date_of_PUB like '[a-z][a-z][a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like '[0-9][0-9][0-9][0-9] [a-z][a-z][a-z] [0-9][0-9]'
						or date_of_PUB like 'January [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'February [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'March [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'April [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'May [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'June [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'July [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'August [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'September [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'October [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'November [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'December [0-9][0-9][0-9][0-9]'
						--or date_of_PUB like '[0-9][0-9][0-9][0-9]'	
						)	
					then DATE_OF_PUB
				when (DATE_OF_PUB like '[0-9][0-9][0-9][0-9][,.:; ¦|\/]')
					then substring(DATE_OF_PUB, 1,4)
				when (date_of_PUB like '[a-z][a-z][a-z] [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'January [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'February [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'March [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'April [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'May [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'June [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'July [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'August [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'September [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'October [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'November [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'December [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like '[a-z][a-z][a-z] [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'January [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'February [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'March [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'April [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'May [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'June [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'July [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'August [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'September [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'October [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'November [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'December [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like '[a-z][a-z][a-z] [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'January [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'February [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'March [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'April [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'May [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'June [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'July [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'August [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'September [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'October [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'November [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'December [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						)	
					then DATE_OF_PUB
				when (date_of_PUB like '[a-z][a-z][a-z] [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'January [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'February [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'March [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'April [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'May [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'June [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'July [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'August [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'September [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'October [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'November [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'December [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						
						)	
					then substring(DATE_OF_PUB, 1, PATINDEX('%[0-9][0-9][0-9][0-9]%', DATE_OF_PUB) + 3)
				when (date_of_PUB like '[0-9][0-9][0-9][0-9] [a-z][a-z][a-z] [0-9][0-9][,.:; ¦|\/]')	
					then  left(DATE_OF_PUB, 11)
				when (date_of_PUB like 'May [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,8)
				when (date_of_PUB like 'June [0-9][0-9][0-9][0-9]%'
						or date_of_PUB like 'July [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,9)
				when (date_of_PUB like 'March [0-9][0-9][0-9][0-9]%'
						or date_of_PUB like 'April [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,10)
				when (date_of_PUB like 'August [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,11)
				when (date_of_PUB like 'January [0-9][0-9][0-9][0-9]%'
						or date_of_PUB like 'October [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,12)
				when (date_of_PUB like 'February [0-9][0-9][0-9][0-9]%'
						or date_of_PUB like 'November [0-9][0-9][0-9][0-9]%'
						or date_of_PUB like 'December [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,13)
				when (date_of_PUB like 'September [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,14)
				when (date_of_PUB like 'winter [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'spring [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'summer [0-9][0-9][0-9][0-9]')
					then substring(DATE_OF_PUB, 8,4)
				when (date_of_PUB like 'winter [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'spring [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'summer [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'winter [0-9][0-9][0-9][0-9][ ]'
						or date_of_PUB like 'spring [0-9][0-9][0-9][0-9][ ]'
						or date_of_PUB like 'summer [0-9][0-9][0-9][0-9][ ]')
					then substring(DATE_OF_PUB, 8,4)
				when (date_of_PUB like 'fall [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'fall [0-9][0-9][0-9][0-9][ ]')
					then substring(DATE_OF_PUB, 6,4)
				when (date_of_pub like'[0-9][0-9][0-9][0-9]/[0-9][0-9]'
						or date_of_pub like'[0-9][0-9][0-9][0-9]/[0-9][0-9]//')
					then substring(date_of_pub, 6, 2) + '/01/' + left(date_of_pub, 4)
				when ((date_of_pub like'[0-9][0-9][0-9][0-9] [jfmasond][aepuco][nbrynlgptvc]'
						or date_of_pub like'[0-9][0-9][0-9][0-9] [jfmasond][aepuco][nbrynlgptvc][,.:; ¦|\/]')
						and date_of_pub not like '[0-9][0-9][0-9][0-9] spr%')
					then substring(date_of_pub, 6, 3) + ' ' + left(date_of_pub, 4)
				when (date_of_pub like'[0-9][0-9][0-9][0-9]/[0-9][0-9]/[0-9][0-9]/')
					then substring(date_of_pub, 6, 2) + '/' + substring(date_of_pub, 9, 2) + '/' + left(date_of_pub, 4)
				when (date_of_pub like '[0-9][0-9][0-9][0-9]%' and left(date_of_pub, 4) > 1900 and left(date_of_pub, 4) < 2010)
					then  + left(date_of_pub, 4)
				when (date_of_pub like '%[0-9][0-9][0-9][0-9]%'
						and substring(date_of_pub, PATINDEX('%[0-9][0-9][0-9][0-9]%', date_of_pub), 4) > 1900
						and substring(date_of_pub, PATINDEX('%[0-9][0-9][0-9][0-9]%', date_of_pub), 4) < 2010)
					then substring(date_of_pub, PATINDEX('%[0-9][0-9][0-9][0-9]%', date_of_pub), 4)
				else 'x'
			END,

			'institution' = CASE
				when ((type_code = 'e' or type_code = 'g' or type_code = 'Research project') and publisher is not null and publisher != 'unpublished') then publisher	
				else ''
			END,
			*
		from EPPI_WEB.DBO.tb_ITEM src
		where exists (SELECT REVIEW_ID from EPPI_WEB.DBO.TB_ITEM i inner join
						EPPI_WEB.DBO.tb_ITEM_REVIEW j
						on j.ITEM_ID = i.ITEM_ID
						 where j.REVIEW_ID = @revID
						 and src.item_ID = i.item_id)
	) a

update tb_TEMP_ITEM SET ITEM = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( ITEM)
	,ITEM_DESCRIPTION = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( ITEM_DESCRIPTION)
	,TITLE_ANALYTIC = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( TITLE_ANALYTIC)
	,TITLE_MONO = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( TITLE_MONO)
	,NOTES = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( NOTES)
	,INSTITUTION = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( INSTITUTION)
	,PUBLISHER = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( PUBLISHER)
	,TRANS_NEWS_TITLE = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( TRANS_NEWS_TITLE)

--populate tb_ITEM from temp table:
--delete from tb_ITEM
--DBCC CHECKIDENT('tb_ITEM', RESEED, 0)

insert into tb_ITEM
(
	"TYPE_ID", TITLE, PARENT_TITLE,
	SHORT_TITLE, DATE_CREATED, CREATED_BY,
	DATE_EDITED, EDITED_BY, "YEAR",
	"MONTH", STANDARD_NUMBER, CITY,
	PUBLISHER, INSTITUTION,
	VOLUME, PAGES, EDITION,
	ISSUE, AVAILABILITY,
	URL, OLD_ITEM_ID, ABSTRACT,
	COMMENTS
)
select 
	"TYPE_ID", 
	TITLE = Case
		When (titlesource = 'TITLE_MONO' ) then TITLE_MONO
		WHEN (title_analytic is not null) then title_analytic
		else ''
	END,
	PARENT_TITLE = Case
		When (ParentTSource = 'TITLE_MONO' AND TITLE_MONO is not null) then TITLE_MONO
		When (ParentTSource = 'SERIES_TITLE' AND SERIES_TITLE is not null) then SERIES_TITLE
		When (ParentTSource = 'JOURNAL' AND JOURNAL is not null) then JOURNAL	
		else ''
	END,
	SHORT_TITLE= Case
		When ( SHORT_TITLE is null) then ''
		ELSE SHORT_TITLE
	END,
	DATE_CREATED = Case
		When (DATE_CREATED = '' or DATE_CREATED is null) then '2000-01-01 00:00:00.0'
		ELSE DATE_CREATED
	END,
	CREATED_BY = Case
		WHEN (CREATED_BY is null) then ''
		ELSE CREATED_BY
	END,
	DATE_EDITED = Case
		WHEN ((DATE_CREATED = '' or DATE_CREATED is null)
			and (DATE_EDITED = '' or DATE_EDITED is null)) then '2000-01-01 00:00:00.0'
		WHEN ((DATE_CREATED != '' and DATE_CREATED is not null)
			and (DATE_EDITED = '' or DATE_EDITED is null)) then DATE_CREATED
		ELSE DATE_EDITED
	END,
	EDITED_BY = Case
		WHEN ((CREATED_BY = '' or CREATED_BY is null)
			and (EDITED_BY = '' or EDITED_BY is null)) then ''
		WHEN ((CREATED_BY != '' and CREATED_BY is not null)
			and (EDITED_BY = '' or EDITED_BY is null)) then CREATED_BY
		ELSE EDITED_BY
	END,
	
	'YEAR' = CASE 
		WHEN (len(PubData) > 2 ) THEN Datename(yyyy, cast (PubData as datetime))  
		ELSE ''
	END,

	'MONTH' = CASE
		WHEN (len(PubData) > 4 ) THEN Datename(mm, cast (PubData as datetime)) 
		ELSE ''
	END,

	STANDARD_NUMBER = CASE
		WHEN ((ISSN != '' and ISSN is not null) and (ISBN != '' and ISBN is not null))
			THEN cast(('ISSN: ' + cast(ISSN as nvarchar(249)) + ' ISBN: ' + cast(ISBN as nvarchar(249))) as nvarchar(254))
		WHEN ((ISSN = '' or ISSN is null) and (ISBN != '' and ISBN is not null))
			THEN 'ISBN: ' + cast(ISBN as nvarchar(249))
		WHEN ((ISSN != '' and ISSN is not null) and (ISBN = '' or ISBN is null))
			THEN 'ISSN: ' + cast(ISSN as nvarchar(249))
		ELSE ''
	END,
	CITY = CASE 
		WHEN (PLACE_OF_PUB is null) then ''
		ELSE cast(PLACE_OF_PUB as nvarchar(100))
	END,
	PUBLISHER  = CASE 
		WHEN (PUBLISHER is null) then ''
		ELSE PUBLISHER
	END,
	INSTITUTION,
	VOLUME = CASE
		WHEN (VOLUME is not null) then cast (VOLUME as nvarchar(56))
		WHEN ((VOLUME is null or VOLUME = '') and (SERIES_VOLUME != '' and SERIES_VOLUME is not null))
			then cast (SERIES_VOLUME as nvarchar(56))
		ELSE ''
	END,
	PAGES = CASE 
		WHEN (PAGES is null) then ''
		ELSE cast (PAGES as nvarchar(50))
	END,
	EDITION = CASE 
		WHEN (EDITION is null) then ''
		ELSE EDITION
	END,
	ISSUE = CASE
		WHEN (SERIES_ISSUE is not null and SERIES_ISSUE != '') then cast (SERIES_ISSUE as nvarchar(100))
		WHEN (SERIES_ISSUE is not null) then cast (ISSUE as nvarchar(100))
		ELSE ''
	END,
	AVAILABILITY = CASE
		WHEN (ISWEB = 1 or AVAILABILITY is null) then ''
		WHEN ((AVAILABILITY is null or AVAILABILITY = '') and (LOCATION is not null and LOCATION != '')) 
			then dbo.fn_CLEAN_SIMPLE_TEXT(LOCATION)
		ELSE dbo.fn_CLEAN_SIMPLE_TEXT(AVAILABILITY)
	END,
	URL = CASE
		WHEN (ISWEB = 1) then dbo.fn_CLEAN_SIMPLE_TEXT(AVAILABILITY)
		WHEN (ISWEB = 2) then dbo.fn_CLEAN_SIMPLE_TEXT(LOCATION)
		WHEN (ISWEB = 3) then dbo.fn_CLEAN_SIMPLE_TEXT(JOURNAL)
		ELSE ''
	END,
	ITEM_ID,
	ABSTRACT = CASE 
		WHEN (ABSTRACT is null) then ''
		ELSE dbo.fn_CLEAN_SIMPLE_TEXT(ABSTRACT)
	END,
	COMMENTS = CASE
		WHEN (NOTES is not null AND EXTENT_OF_WORK is not null AND CONTACT_DETAILS is not null)
			THEN LTRIM(RTRIM(convert(nvarchar(max), NOTES) + ' ' + EXTENT_OF_WORK + ' ' + CONTACT_DETAILS))
		ELSE ''
	end
from tb_TEMP_ITEM ti where not exists (SELECT old_ITEM_ID from tb_ITEM i where i.old_ITEM_ID = ti.ITEM_ID)



-- populate tb_ITEM_AUTHOR
--DELETE from tb_ITEM_AUTHOR

insert into tb_ITEM_AUTHOR (
	ITEM_ID, LAST, FIRST, SECOND, ROLE, RANK
)
Select T.ITEM_ID, LAST, FIRST, SECOND, ORIGIN, RANK 
From tb_ITEM as T 
	inner join EPPI_WEB.DBO.tb_ITEM_AUTHOR as S 
	on T.OLD_ITEM_ID COLLATE SQL_Latin1_General_CP1_CI_AS = S.ITEM_ID 
	inner join EPPI_WEB.DBO.tb_ITEM_REVIEW j 
	on S.ITEM_ID = j.ITEM_ID
	WHERE j.review_ID = @revID
	AND not EXISTS (SELECT ia.ITEM_ID from tb_ITEM_AUTHOR ia
					WHERE ia.ITEM_ID = T.Item_ID
					AND S.RANK = ia.RANK
					AND S.ORIGIN = ia.ROLE)
	order by item_id, rank
delete from tb_TEMP_ITEM

/* ********************* END SERGIO'S TRANSFER CODE *********************** */
/* ********************* DO IT AGAIN for screening items *********************** */
delete from tb_TEMP_ITEM
DBCC CHECKIDENT('tb_TEMP_ITEM', RESEED, 0)
/*go 

DECLARE @revID nvarchar(50)
set @revID = 'REV154'
*/
insert into tb_TEMP_ITEM

(
	TYPE_ID,
	ISWEB,
	mediumToEd,
	titleSource,
	ParentTSource,
	PUBDATA,
	INSTITUTION,
	ITEM_ID,
	ITEM,
	ITEM_DESCRIPTION,
	TYPE_CODE,
	DATE_CREATED,
	DATE_EDITED,
	CREATED_BY,
	EDITED_BY,
	AUTHOR_ANALYTIC,
	TITLE_ANALYTIC,
	MEDIUM,
	AUTHOR_MONO,
	AUTHOR_ROLE,
	TITLE_MONO,
	JOURNAL,
	TRANS_NEWS_TITLE,
	PLACE,
	EDITION,
	PLACE_OF_PUB,
	PUBLISHER,
	DATE_OF_PUB,
	VOLUME,
	REPORT_ID,
	ISSUE,
	PAGES,
	EXTENT_OF_WORK,
	CONTACT_DETAILS,
	SERIES_TITLE,
	SERIES_VOLUME,
	SERIES_ISSUE,
	WRITTEN_LANGUAGE,
	AVAILABILITY,
	LOCATION,
	EPIC_NO,
	ISSN,
	ISBN,
	NOTES,
	ABSTRACT,
	AGE_RANGE,
	SHORT_TITLE,
	ITEM_IDENTITY,
	CONFIDENTIAL_CONTACT_ID,
	IMPORTED_REF_ID
)

select 
	'TYPE_ID' = CASE
     WHEN (type_code = 'Journal, Article'
			or type_code = 'D'
			or type_code = 'JOUR      '
			or type_code = 'Journal, Whole'
			or (type_code = 'PUB' and (JOURNAL != '' and JOURNAL is not null)) ) THEN 14  
     WHEN (type_code = 'E') THEN 1
	 WHEN (type_code = 'Book, Whole') THEN 2 
     WHEN (type_code = 'Book, Chapter') THEN 3
	 WHEN (type_code = 'G') THEN 4  
     WHEN (type_code = 'K') THEN 5
	 WHEN (type_code = 'PUB' and (JOURNAL = '' or JOURNAL is null)) THEN 1 
	 WHEN (type_code = 'INT       ') THEN 11
	 WHEN (type_code = 'Electronic Citation'
			and isweb > 0) THEN 7
	 WHEN (type_code = 'Electronic Citation'
			and journal is not null and journal != '') THEN 0
	 WHEN (type_code = 'Electronic Citation'
			and (journal is null or journal = '')) THEN 8
	 WHEN (type_code = 'Research project') THEN 9
	 WHEN (type_code = 'PER       ' or type_code = 'F') THEN 10
	 WHEN (type_code = 'DVD, Video, Media' and isweb = 0) THEN 8
	 WHEN (type_code = 'DVD, Video, Media' and isweb > 0) THEN 7
	 WHEN (type_code = 'H') THEN 12
	 ELSE 12 
	END,
	*
from ( select 
			'isweb' = CASE 
				 WHEN ((len(AVAILABILITY) > 5 and left(AVAILABILITY, 4) = 'www.')
							or (len(AVAILABILITY) > 7 and left(AVAILABILITY, 7) = 'http://')
							or (len(AVAILABILITY) > 8 and left(AVAILABILITY, 8) = 'https://')
							) 
				 THEN 1
				 WHEN ((len(LOCATION) > 5 and left(LOCATION, 4) = 'www.')
							or (len(LOCATION) > 7 and left(LOCATION, 7) = 'http://')
							or (len(LOCATION) > 5 and left(LOCATION, 5) = 'http')
							or (len(LOCATION) > 8 and left(LOCATION, 8) = 'https://')
							)
				 THEN 2
				 WHEN ((len(JOURNAL) > 5 and left(JOURNAL, 4) = 'www.')
							or (len(JOURNAL) > 7 and left(JOURNAL, 7) = 'http://')
							or (len(JOURNAL) > 8 and left(JOURNAL, 8) = 'https://')
							)
				 THEN 3
				 ELSE 0 
			END,
			mediumToEd = CASE
				when (type_code = 'G' and (Journal = '' or Journal is null) and (medium is not null and medium !=''))
				then 1
				else 0
			END,
			titleSource = CASE
				when (type_code = 'Book, Whole' and (title_mono != '' and title_mono is not null))
				then 'title_mono'	
				else 'title_analytic'
			END,
			ParentTSource = CASE
				when (type_code = 'PUB' and Journal != '' and Journal is not null)
					or 
					(
						(type_code = 'Journal, Article' or type_code = 'Journal, Whole'
						or type_code = 'D' or  type_code = 'jour' ) and 
						( journal is not null and journal !='') 
					) or (
						type_code = 'K' and JOURNAL != '' and JOURNAL is not null and 
							(title_mono not like '155%' or title_mono is null)
					) or (
						type_code = 'PUB' and (JOURNAL != '' and JOURNAL is not null)
					)  or (
						type_code = 'Electronic Citation' and (JOURNAL != '' and JOURNAL is not null) 
						and not ((len(JOURNAL) > 5 and left(JOURNAL, 4) = 'www.')
							or (len(JOURNAL) > 7 and left(JOURNAL, 7) = 'http://')
							or (len(JOURNAL) > 8 and left(JOURNAL, 8) = 'https://')
							)
					)
					then 'Journal'
				when (
						(type_code = 'Book, Whole' and (SERIES_TITLE is null or SERIES_TITLE = ''))
						or (type_code = 'G' and (Journal is null or journal = '') 
							and (title_mono is  null or title_mono = ''))) then '0'
				when (type_code = 'Book, Whole' and SERIES_TITLE is not null and SERIES_TITLE != '') then 'SERIES_TITLE'
				when (type_code = 'G' and Journal is not null and journal !='') then 'Journal'	
				else 'TITLE_MONO'
			END,
			PubData = CASE
				when (date_of_PUB like 'win [0-9][0-9][0-9][0-9]'	
						or date_of_PUB like 'sum [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'wnt [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'spr [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'fal [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'aut [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'win [0-9][0-9][0-9][0-9][,.:; ¦|\/]'	
						or date_of_PUB like 'sum [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'wnt [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'spr [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'fal [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'aut [0-9][0-9][0-9][0-9][,.:; ¦|\/]')
					then substring(DATE_OF_PUB, 5,4)
				when (date_of_PUB like '[a-z][a-z][a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like '[0-9][0-9][0-9][0-9] [a-z][a-z][a-z] [0-9][0-9]'
						or date_of_PUB like 'January [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'February [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'March [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'April [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'May [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'June [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'July [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'August [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'September [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'October [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'November [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'December [0-9][0-9][0-9][0-9]'
						--or date_of_PUB like '[0-9][0-9][0-9][0-9]'	
						)	
					then DATE_OF_PUB
				when (DATE_OF_PUB like '[0-9][0-9][0-9][0-9][,.:; ¦|\/]')
					then substring(DATE_OF_PUB, 1,4)
				when (date_of_PUB like '[a-z][a-z][a-z] [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'January [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'February [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'March [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'April [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'May [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'June [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'July [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'August [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'September [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'October [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'November [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'December [0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like '[a-z][a-z][a-z] [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'January [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'February [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'March [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'April [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'May [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'June [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'July [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'August [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'September [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'October [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'November [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'December [0-9][0-9] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like '[a-z][a-z][a-z] [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'January [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'February [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'March [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'April [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'May [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'June [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'July [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'August [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'September [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'October [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'November [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'December [0-9][0-9][^a-z] [0-9][0-9][0-9][0-9]'
						)	
					then DATE_OF_PUB
				when (date_of_PUB like '[a-z][a-z][a-z] [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'January [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'February [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'March [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'April [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'May [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'June [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'July [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'August [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'September [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'October [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'November [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'December [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						
						)	
					then substring(DATE_OF_PUB, 1, PATINDEX('%[0-9][0-9][0-9][0-9]%', DATE_OF_PUB) + 3)
				when (date_of_PUB like '[0-9][0-9][0-9][0-9] [a-z][a-z][a-z] [0-9][0-9][,.:; ¦|\/]')	
					then  left(DATE_OF_PUB, 11)
				when (date_of_PUB like 'May [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,8)
				when (date_of_PUB like 'June [0-9][0-9][0-9][0-9]%'
						or date_of_PUB like 'July [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,9)
				when (date_of_PUB like 'March [0-9][0-9][0-9][0-9]%'
						or date_of_PUB like 'April [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,10)
				when (date_of_PUB like 'August [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,11)
				when (date_of_PUB like 'January [0-9][0-9][0-9][0-9]%'
						or date_of_PUB like 'October [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,12)
				when (date_of_PUB like 'February [0-9][0-9][0-9][0-9]%'
						or date_of_PUB like 'November [0-9][0-9][0-9][0-9]%'
						or date_of_PUB like 'December [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,13)
				when (date_of_PUB like 'September [0-9][0-9][0-9][0-9]%')	
					then substring(DATE_OF_PUB, 1,14)
				when (date_of_PUB like 'winter [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'spring [0-9][0-9][0-9][0-9]'
						or date_of_PUB like 'summer [0-9][0-9][0-9][0-9]')
					then substring(DATE_OF_PUB, 8,4)
				when (date_of_PUB like 'winter [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'spring [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'summer [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'winter [0-9][0-9][0-9][0-9][ ]'
						or date_of_PUB like 'spring [0-9][0-9][0-9][0-9][ ]'
						or date_of_PUB like 'summer [0-9][0-9][0-9][0-9][ ]')
					then substring(DATE_OF_PUB, 8,4)
				when (date_of_PUB like 'fall [0-9][0-9][0-9][0-9][,.:; ¦|\/]'
						or date_of_PUB like 'fall [0-9][0-9][0-9][0-9][ ]')
					then substring(DATE_OF_PUB, 6,4)
				when (date_of_pub like'[0-9][0-9][0-9][0-9]/[0-9][0-9]'
						or date_of_pub like'[0-9][0-9][0-9][0-9]/[0-9][0-9]//')
					then substring(date_of_pub, 6, 2) + '/01/' + left(date_of_pub, 4)
				when ((date_of_pub like'[0-9][0-9][0-9][0-9] [jfmasond][aepuco][nbrynlgptvc]'
						or date_of_pub like'[0-9][0-9][0-9][0-9] [jfmasond][aepuco][nbrynlgptvc][,.:; ¦|\/]')
						and date_of_pub not like '[0-9][0-9][0-9][0-9] spr%')
					then substring(date_of_pub, 6, 3) + ' ' + left(date_of_pub, 4)
				when (date_of_pub like'[0-9][0-9][0-9][0-9]/[0-9][0-9]/[0-9][0-9]/')
					then substring(date_of_pub, 6, 2) + '/' + substring(date_of_pub, 9, 2) + '/' + left(date_of_pub, 4)
				when (date_of_pub like '[0-9][0-9][0-9][0-9]%' and left(date_of_pub, 4) > 1900 and left(date_of_pub, 4) < 2010)
					then  + left(date_of_pub, 4)
				when (date_of_pub like '%[0-9][0-9][0-9][0-9]%'
						and substring(date_of_pub, PATINDEX('%[0-9][0-9][0-9][0-9]%', date_of_pub), 4) > 1900
						and substring(date_of_pub, PATINDEX('%[0-9][0-9][0-9][0-9]%', date_of_pub), 4) < 2010)
					then substring(date_of_pub, PATINDEX('%[0-9][0-9][0-9][0-9]%', date_of_pub), 4)
				else 'x'
			END,

			'institution' = CASE
				when ((type_code = 'e' or type_code = 'g' or type_code = 'Research project') and publisher is not null and publisher != 'unpublished') then publisher	
				else ''
			END,
			cast (ITEM_ID as nvarchar(50)) as ITEM_ID,
	ITEM,
	ITEM_DESCRIPTION,
	TYPE_CODE,
	DATE_CREATED,
	DATE_EDITED,
	CREATED_BY,
	EDITED_BY,
	AUTHOR_ANALYTIC,
	TITLE_ANALYTIC,
	MEDIUM,
	AUTHOR_MONO,
	AUTHOR_ROLE,
	TITLE_MONO,
	JOURNAL,
	TRANS_NEWS_TITLE,
	PLACE,
	EDITION,
	PLACE_OF_PUB,
	PUBLISHER,
	DATE_OF_PUB,
	VOLUME,
	REPORT_ID,
	ISSUE,
	PAGES,
	EXTENT_OF_WORK,
	CONTACT_DETAILS,
	SERIES_TITLE,
	SERIES_VOLUME,
	SERIES_ISSUE,
	WRITTEN_LANGUAGE,
	AVAILABILITY,
	LOCATION,
	EPIC_NO,
	ISSN,
	ISBN,
	NOTES,
	ABSTRACT,
	AGE_RANGE,
	SHORT_TITLE,
	0 as ITEM_IDENTITY,
	NULL as CONFIDENTIAL_CONTACT_ID,
	IMPORTED_REF_ID
		from EPPI_WEB.DBO.tb_REFERENCE_ITEM_FOR_SCREENING src
		where REVIEW_ID = @revID
	) a
update tb_TEMP_ITEM SET ITEM = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( ITEM)
	,ITEM_DESCRIPTION = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( ITEM_DESCRIPTION)
	,TITLE_ANALYTIC = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( TITLE_ANALYTIC)
	,TITLE_MONO = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( TITLE_MONO)
	,NOTES = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( NOTES)
	,INSTITUTION = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( INSTITUTION)
	,PUBLISHER = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( PUBLISHER)
	,TRANS_NEWS_TITLE = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( TRANS_NEWS_TITLE)

/*go
DECLARE @revID nvarchar(50)
set @revID = 'REV154'
*/
--populate tb_ITEM from temp table:
--delete from tb_ITEM
--DBCC CHECKIDENT('tb_ITEM', RESEED, 0)
select COUNT(item_ID) from tb_TEMP_ITEM
insert into tb_ITEM
(
	"TYPE_ID", TITLE, PARENT_TITLE,
	SHORT_TITLE, DATE_CREATED, CREATED_BY,
	DATE_EDITED, EDITED_BY, "YEAR",
	"MONTH", STANDARD_NUMBER, CITY,
	PUBLISHER, INSTITUTION,
	VOLUME, PAGES, EDITION,
	ISSUE, AVAILABILITY,
	URL, OLD_ITEM_ID, ABSTRACT,
	COMMENTS
)
select 
	"TYPE_ID", 
	TITLE = Case
		When (titlesource = 'TITLE_MONO' ) then TITLE_MONO
		WHEN (title_analytic is not null) then title_analytic
		else ''
	END,
	PARENT_TITLE = Case
		When (ParentTSource = 'TITLE_MONO' AND TITLE_MONO is not null) then TITLE_MONO
		When (ParentTSource = 'SERIES_TITLE' AND SERIES_TITLE is not null) then SERIES_TITLE
		When (ParentTSource = 'JOURNAL' AND JOURNAL is not null) then JOURNAL	
		else ''
	END,
	SHORT_TITLE= Case
		When ( SHORT_TITLE is null) then ''
		ELSE SHORT_TITLE
	END,
	DATE_CREATED = Case
		When (DATE_CREATED = '' or DATE_CREATED is null) then '2000-01-01 00:00:00.0'
		ELSE DATE_CREATED
	END,
	CREATED_BY = Case
		WHEN (CREATED_BY is null) then ''
		ELSE CREATED_BY
	END,
	DATE_EDITED = Case
		WHEN ((DATE_CREATED = '' or DATE_CREATED is null)
			and (DATE_EDITED = '' or DATE_EDITED is null)) then '2000-01-01 00:00:00.0'
		WHEN ((DATE_CREATED != '' and DATE_CREATED is not null)
			and (DATE_EDITED = '' or DATE_EDITED is null)) then DATE_CREATED
		ELSE DATE_EDITED
	END,
	EDITED_BY = Case
		WHEN ((CREATED_BY = '' or CREATED_BY is null)
			and (EDITED_BY = '' or EDITED_BY is null)) then ''
		WHEN ((CREATED_BY != '' and CREATED_BY is not null)
			and (EDITED_BY = '' or EDITED_BY is null)) then CREATED_BY
		ELSE EDITED_BY
	END,
	
	'YEAR' = CASE 
		WHEN (len(PubData) > 2 ) THEN Datename(yyyy, cast (PubData as datetime))  
		ELSE ''
	END,

	'MONTH' = CASE
		WHEN (len(PubData) > 4 ) THEN Datename(mm, cast (PubData as datetime)) 
		ELSE ''
	END,

	STANDARD_NUMBER = CASE
		WHEN ((ISSN != '' and ISSN is not null) and (ISBN != '' and ISBN is not null))
			THEN cast(('ISSN: ' + cast(ISSN as nvarchar(249)) + ' ISBN: ' + cast(ISBN as nvarchar(249))) as nvarchar(254))
		WHEN ((ISSN = '' or ISSN is null) and (ISBN != '' and ISBN is not null))
			THEN 'ISBN: ' + cast(ISBN as nvarchar(249))
		WHEN ((ISSN != '' and ISSN is not null) and (ISBN = '' or ISBN is null))
			THEN 'ISSN: ' + cast(ISSN as nvarchar(249))
		ELSE ''
	END,
	CITY = CASE 
		WHEN (PLACE_OF_PUB is null) then ''
		ELSE cast(PLACE_OF_PUB as nvarchar(100))
	END,
	PUBLISHER  = CASE 
		WHEN (PUBLISHER is null) then ''
		ELSE PUBLISHER
	END,
	INSTITUTION,
	VOLUME = CASE
		WHEN (VOLUME is not null) then cast (VOLUME as nvarchar(56))
		WHEN ((VOLUME is null or VOLUME = '') and (SERIES_VOLUME != '' and SERIES_VOLUME is not null))
			then cast (SERIES_VOLUME as nvarchar(56))
		ELSE ''
	END,
	PAGES = CASE 
		WHEN (PAGES is null) then ''
		ELSE cast (PAGES as nvarchar(50))
	END,
	EDITION = CASE 
		WHEN (EDITION is null) then ''
		ELSE EDITION
	END,
	ISSUE = CASE
		WHEN (SERIES_ISSUE is not null and SERIES_ISSUE != '') then cast (SERIES_ISSUE as nvarchar(100))
		WHEN (SERIES_ISSUE is not null) then cast (ISSUE as nvarchar(100))
		ELSE ''
	END,
	AVAILABILITY = CASE
		WHEN (ISWEB = 1 or AVAILABILITY is null) then ''
		WHEN ((AVAILABILITY is null or AVAILABILITY = '') and (LOCATION is not null and LOCATION != '')) 
			then dbo.fn_CLEAN_SIMPLE_TEXT(LOCATION)
		ELSE dbo.fn_CLEAN_SIMPLE_TEXT(AVAILABILITY)
	END,
	URL = CASE
		WHEN (ISWEB = 1) then dbo.fn_CLEAN_SIMPLE_TEXT(AVAILABILITY)
		WHEN (ISWEB = 2) then dbo.fn_CLEAN_SIMPLE_TEXT(LOCATION)
		WHEN (ISWEB = 3) then dbo.fn_CLEAN_SIMPLE_TEXT(JOURNAL)
		ELSE ''
	END,
	'SCR' + CAST(ITEM_ID AS NVARCHAR(10)),
	ABSTRACT = CASE 
		WHEN (ABSTRACT is null) then ''
		ELSE dbo.fn_CLEAN_SIMPLE_TEXT(ABSTRACT)
	END,
	COMMENTS = CASE
		WHEN (NOTES is not null AND EXTENT_OF_WORK is not null AND CONTACT_DETAILS is not null)
			THEN LTRIM(RTRIM(convert(nvarchar(max), NOTES) + ' ' + EXTENT_OF_WORK + ' ' + CONTACT_DETAILS))
		ELSE ''
	end
from tb_TEMP_ITEM  



-- populate tb_ITEM_AUTHOR
--DELETE from tb_ITEM_AUTHOR

/*insert into tb_ITEM_AUTHOR (
	ITEM_ID, LAST, FIRST, SECOND, ROLE, RANK
)
Select T.ITEM_ID, LAST, FIRST, SECOND, ORIGIN, RANK 
From tb_ITEM as T 
	inner join EPPI_WEB.DBO.tb_ITEM_AUTHOR as S 
	on T.OLD_ITEM_ID COLLATE SQL_Latin1_General_CP1_CI_AS = S.ITEM_ID 
	inner join EPPI_WEB.DBO.tb_ITEM_REVIEW j 
	on S.ITEM_ID = j.ITEM_ID
	inner join reviewer.dbo.TB_TEMP_ITEM ti
	on T.OLD_ITEM_ID = 'SCR' + CAST(ti.ITEM_ID AS NVARCHAR(10))
	WHERE j.review_ID = @revID
	order by item_id, rank
	*/
	
INSERT INTO TB_ITEM_REVIEW(ITEM_ID, REVIEW_ID, IS_INCLUDED, IS_DELETED)
SELECT i.ITEM_ID, r.REVIEW_ID, 'FALSE', 'FALSE' FROM TB_ITEM i
INNER JOIN TB_TEMP_ITEM ti on 'SCR' + CAST(ti.ITEM_ID AS NVARCHAR(10)) = i.OLD_ITEM_ID
INNER JOIN TB_REVIEW r on r.OLD_REVIEW_ID = @revID

delete from tb_TEMP_ITEM

/* ********************* END SERGIO'S TRANSFER CODE *********************** */
INSERT INTO reviewer.DBO.TB_SET(SET_TYPE_ID, SET_NAME, OLD_GUIDELINE_ID)
SELECT (SELECT reviewer.DBO.TB_SET_TYPE.SET_TYPE_ID FROM reviewer.DBO.TB_SET_TYPE
		WHERE reviewer.DBO.TB_SET_TYPE.SET_TYPE_ID = src.GUIDELINE_STATUS_ID),
		rtrim(GUIDELINE), src.GUIDELINE_ID FROM EPPI_WEB.DBO.TB_GUIDELINE as src
		inner join EPPI_WEB.DBO.tb_REVIEW_GUIDELINE as rg on rg.GUIDELINE_ID = src.GUIDELINE_ID
WHERE not EXISTS 
	(SELECT OLD_GUIDELINE_ID from reviewer.DBO.TB_SET se 
	where src.GUIDELINE_ID = se.OLD_GUIDELINE_ID COLLATE Latin1_General_CI_AS)
AND rg.REVIEW_ID = @revID

INSERT INTO reviewer.DBO.TB_ITEM_REVIEW(ITEM_ID, REVIEW_ID, IS_INCLUDED, MASTER_ITEM_ID, IS_DELETED)
SELECT ti.ITEM_ID,
	r.REVIEW_ID,
	'TRUE', NULL, 'FALSE'
	FROM EPPI_WEB.DBO.TB_ITEM_REVIEW as src 
	inner join Reviewer.dbo.TB_ITEM ti on src.ITEM_ID = ti.OLD_ITEM_ID COLLATE Latin1_General_CI_AS
	inner join Reviewer.dbo.TB_REVIEW r on src.REVIEW_ID = r.OLD_REVIEW_ID COLLATE Latin1_General_CI_AS
	where src.REVIEW_ID = @revID

/* the following depends on what we'll do with data synch, this  will work if no active synch mechanism will be used
it moves attributes that belong to a review if they are not already present in ER4*/
INSERT INTO reviewer.DBO.TB_ATTRIBUTE(CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
SELECT (SELECT reviewer.DBO.TB_CONTACT.CONTACT_ID FROM reviewer.DBO.TB_CONTACT
		WHERE reviewer.DBO.TB_CONTACT.OLD_CONTACT_ID = src.CONTACT_ID COLLATE Latin1_General_CI_AS),
	ATTRIBUTE, ATTRIBUTE_DESC, src.ATTRIBUTE_ID
	FROM EPPI_WEB.DBO.TB_ATTRIBUTE as src
		inner join EPPI_WEB.DBO.tb_GUIDE_STRUCT gs
		ON src.ATTRIBUTE_ID = gs.ATTRIBUTE_ID
		inner join EPPI_WEB.DBO.tb_REVIEW_GUIDELINE rg 
		ON rg.GUIDELINE_ID = gs.GUIDELINE_ID
	Where rg.REVIEW_ID = @revID AND
		not EXISTS 
		(
			SELECT dest.ATTRIBUTE_ID from reviewer.DBO.TB_ATTRIBUTE as dest 
			where src.ATTRIBUTE_ID = dest.OLD_ATTRIBUTE_ID COLLATE Latin1_General_CI_AS
		)

--as above
INSERT INTO reviewer.DBO.TB_SET (SET_TYPE_ID, SET_NAME, OLD_GUIDELINE_ID)
SELECT (SELECT reviewer.DBO.TB_SET_TYPE.SET_TYPE_ID FROM reviewer.DBO.TB_SET_TYPE
		WHERE reviewer.DBO.TB_SET_TYPE.SET_TYPE_ID = src.GUIDELINE_STATUS_ID),
		rtrim(GUIDELINE), src.GUIDELINE_ID
		 FROM EPPI_WEB.DBO.TB_GUIDELINE as src
		 inner join EPPI_WEB.DBO.tb_REVIEW_GUIDELINE rg on 
		 src.GUIDELINE_ID = rg.GUIDELINE_ID
WHERE not EXISTS 
	(
		SELECT OLD_GUIDELINE_ID from reviewer.DBO.TB_SET se 
		where src.GUIDELINE_ID = se.OLD_GUIDELINE_ID COLLATE Latin1_General_CI_AS
	) 
AND rg.REVIEW_ID = @revID



-- TWO STAGES - PARENTS WHERE SUP_ATTRIBUTE_ID = NULL FIRST
INSERT INTO reviewer.DBO.TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)
SELECT (SELECT reviewer.DBO.TB_ATTRIBUTE.ATTRIBUTE_ID FROM reviewer.DBO.TB_ATTRIBUTE
		WHERE reviewer.DBO.TB_ATTRIBUTE.OLD_ATTRIBUTE_ID = src.ATTRIBUTE_ID COLLATE Latin1_General_CI_AS),
		(SELECT reviewer.DBO.TB_SET.SET_ID FROM reviewer.DBO.TB_SET
		WHERE reviewer.DBO.TB_SET.OLD_GUIDELINE_ID = src.GUIDELINE_ID COLLATE Latin1_General_CI_AS),
		0,
		(SELECT TOP(1) reviewer.DBO.TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID FROM reviewer.DBO.TB_ATTRIBUTE_TYPE
		WHERE reviewer.DBO.TB_ATTRIBUTE_TYPE.OLD_IS_ANSWER = src.IS_ANSWER COLLATE Latin1_General_CI_AS),
		CAST(GUIDE_STRUCT_DESC AS NVARCHAR(MAX)),
		ATTR_ORDER
		FROM EPPI_WEB.DBO.TB_GUIDE_STRUCT as src 
		inner join EPPI_WEB.DBO.tb_REVIEW_GUIDELINE rg on 
		 src.GUIDELINE_ID = rg.GUIDELINE_ID
		WHERE src.SUP_ATTRIBUTE_ID IS NULL
		--AND not EXISTS 
		--(
		--	SELECT OLD_GUIDELINE_ID from reviewer.DBO.TB_SET se 
		--	where src.GUIDELINE_ID = se.OLD_GUIDELINE_ID COLLATE Latin1_General_CI_AS
		--)
		AND rg.REVIEW_ID = @revID
		

-- SECOND STAGE - WHERE NOT NULL
INSERT INTO reviewer.DBO.TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)
SELECT (SELECT reviewer.DBO.TB_ATTRIBUTE.ATTRIBUTE_ID FROM reviewer.DBO.TB_ATTRIBUTE
		WHERE reviewer.DBO.TB_ATTRIBUTE.OLD_ATTRIBUTE_ID = src.ATTRIBUTE_ID COLLATE Latin1_General_CI_AS),
		(SELECT reviewer.DBO.TB_SET.SET_ID FROM reviewer.DBO.TB_SET
		WHERE reviewer.DBO.TB_SET.OLD_GUIDELINE_ID = src.GUIDELINE_ID COLLATE Latin1_General_CI_AS),
		(SELECT reviewer.DBO.TB_ATTRIBUTE.ATTRIBUTE_ID FROM reviewer.DBO.TB_ATTRIBUTE
		WHERE reviewer.DBO.TB_ATTRIBUTE.OLD_ATTRIBUTE_ID = src.SUP_ATTRIBUTE_ID COLLATE Latin1_General_CI_AS),
		(SELECT TOP(1) reviewer.DBO.TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID FROM reviewer.DBO.TB_ATTRIBUTE_TYPE
		WHERE reviewer.DBO.TB_ATTRIBUTE_TYPE.OLD_IS_ANSWER = src.IS_ANSWER COLLATE Latin1_General_CI_AS),
		CAST(GUIDE_STRUCT_DESC AS NVARCHAR(MAX)),
		ATTR_ORDER
		FROM EPPI_WEB.DBO.TB_GUIDE_STRUCT as src
		inner join EPPI_WEB.DBO.tb_REVIEW_GUIDELINE rg on 
		 src.GUIDELINE_ID = rg.GUIDELINE_ID
		WHERE src.SUP_ATTRIBUTE_ID IS NOT NULL
		--AND not EXISTS 
		--(
		--	SELECT OLD_GUIDELINE_ID from reviewer.DBO.TB_SET se 
		--	where src.GUIDELINE_ID = se.OLD_GUIDELINE_ID COLLATE Latin1_General_CI_AS
		--)
		AND rg.REVIEW_ID = @revID

/* old version */
/*
INSERT INTO reviewer.DBO.TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID)
SELECT (SELECT top 1 reviewer.DBO.TB_ITEM.ITEM_ID FROM reviewer.DBO.TB_ITEM
		WHERE reviewer.DBO.TB_ITEM.OLD_ITEM_ID = src.ITEM_ID COLLATE Latin1_General_CI_AS),
		(SELECT reviewer.DBO.TB_SET.SET_ID FROM reviewer.DBO.TB_SET
		WHERE reviewer.DBO.TB_SET.OLD_GUIDELINE_ID = src.GUIDELINE_ID COLLATE Latin1_General_CI_AS),
		CASE WHEN src.STATUS_ID = 'COMP' THEN 'TRUE' ELSE 'FALSE' END,
		(SELECT reviewer.DBO.TB_CONTACT.CONTACT_ID FROM reviewer.DBO.TB_CONTACT
			WHERE reviewer.DBO.TB_CONTACT.OLD_CONTACT_ID = src.CONTACT_ID COLLATE Latin1_General_CI_AS)
		FROM EPPI_WEB.DBO.TB_DATA_EXTRACTION_GUIDELINE as src
		inner join EPPI_WEB.DBO.tb_REVIEW_GUIDELINE rg on 
		src.GUIDELINE_ID = rg.GUIDELINE_ID
		WHERE /*not EXISTS 
		(
			SELECT OLD_GUIDELINE_ID from reviewer.DBO.TB_SET se 
			where src.GUIDELINE_ID = se.OLD_GUIDELINE_ID COLLATE Latin1_General_CI_AS
		)
		AND */
		rg.REVIEW_ID = @revID
INSERT INTO reviewer.DBO.TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT)
SELECT (SELECT reviewer.DBO.TB_ITEM.ITEM_ID FROM reviewer.DBO.TB_ITEM
		WHERE reviewer.DBO.TB_ITEM.OLD_ITEM_ID = src1.ITEM_ID COLLATE Latin1_General_CI_AS),
		(SELECT reviewer.DBO.TB_ITEM_SET.ITEM_SET_ID FROM reviewer.DBO.TB_ITEM_SET WHERE
			reviewer.DBO.TB_ITEM_SET.ITEM_ID = 
				(SELECT reviewer.DBO.TB_ITEM.ITEM_ID FROM reviewer.DBO.TB_ITEM
					WHERE reviewer.DBO.TB_ITEM.OLD_ITEM_ID = src1.ITEM_ID COLLATE Latin1_General_CI_AS)
			AND reviewer.DBO.TB_ITEM_SET.SET_ID = (SELECT reviewer.DBO.TB_SET.SET_ID FROM reviewer.DBO.TB_SET
				WHERE reviewer.DBO.TB_SET.OLD_GUIDELINE_ID = src1.GUIDELINE_ID COLLATE Latin1_General_CI_AS)
			AND reviewer.DBO.TB_ITEM_SET.CONTACT_ID = (SELECT reviewer.DBO.TB_CONTACT.CONTACT_ID FROM reviewer.DBO.TB_CONTACT
				WHERE reviewer.DBO.TB_CONTACT.OLD_CONTACT_ID = src1.CONTACT_ID COLLATE Latin1_General_CI_AS)
			AND reviewer.DBO.TB_ITEM_SET.IS_COMPLETED = 'TRUE'),
		(SELECT dbo.fn_CLEAN_SIMPLE_TEXT(reviewer.DBO.TB_ATTRIBUTE.ATTRIBUTE_ID) FROM reviewer.DBO.TB_ATTRIBUTE
		WHERE reviewer.DBO.TB_ATTRIBUTE.OLD_ATTRIBUTE_ID = src1.ATTRIBUTE_ID COLLATE Latin1_General_CI_AS),
		EXTRACT_ATTR_DESC
	FROM EPPI_WEB.DBO.TB_EXTRACT_ATTR as src1
	INNER JOIN EPPI_WEB.DBO.TB_ITEM_REVIEW as src ON src.ITEM_ID = src1.ITEM_ID
	inner join EPPI_WEB.DBO.tb_ITEM i on 
		i.ITEM_ID = src.ITEM_ID
	WHERE src.REVIEW_ID = @revID
	AND NOT EXISTS 
		(
			SELECT IA.ITEM_ID from TB_ITEM_ATTRIBUTE IA
			inner JOIN tb_ITEM i2 on IA.ITEM_ID = i2.ITEM_ID
			INNER JOIN EPPI_WEB.DBO.TB_ITEM_REVIEW as src2 ON src2.ITEM_ID = i2.OLD_ITEM_ID COLLATE Latin1_General_CI_AS
		)
*/

/*new version*/
INSERT INTO reviewer.DBO.TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID)
		SELECT i.item_ID --(SELECT top 1 reviewer.DBO.TB_ITEM.ITEM_ID FROM reviewer.DBO.TB_ITEM
		--WHERE reviewer.DBO.TB_ITEM.OLD_ITEM_ID = src.ITEM_ID COLLATE Latin1_General_CI_AS AND ITEM_ID is not null) iid,
		,(SELECT reviewer.DBO.TB_SET.SET_ID FROM reviewer.DBO.TB_SET
		WHERE reviewer.DBO.TB_SET.OLD_GUIDELINE_ID = src.GUIDELINE_ID COLLATE Latin1_General_CI_AS) setID,
		CASE WHEN src.STATUS_ID = 'COMP' THEN 'TRUE' ELSE 'FALSE' END IS_COMPLETED,
		(SELECT reviewer.DBO.TB_CONTACT.CONTACT_ID FROM reviewer.DBO.TB_CONTACT
			WHERE reviewer.DBO.TB_CONTACT.OLD_CONTACT_ID = src.CONTACT_ID COLLATE Latin1_General_CI_AS) CONTACT_ID
		FROM EPPI_WEB.DBO.TB_DATA_EXTRACTION_GUIDELINE as src
		inner join EPPI_WEB.DBO.tb_REVIEW_GUIDELINE rg on 
		src.GUIDELINE_ID = rg.GUIDELINE_ID
		inner join EPPI_WEB.DBO.tb_ITEM_REVIEW ir  on ir.REVIEW_ID = rg.REVIEW_ID
		inner join reviewer.DBO.TB_ITEM i on i.OLD_ITEM_ID = ir.ITEM_ID COLLATE Latin1_General_CI_AS
		WHERE /*not EXISTS 
		(
			SELECT OLD_GUIDELINE_ID from reviewer.DBO.TB_SET se 
			where src.GUIDELINE_ID = se.OLD_GUIDELINE_ID COLLATE Latin1_General_CI_AS
		)
		AND */
		rg.REVIEW_ID = @revID
		group by i.item_ID, src.GUIDELINE_ID, src.STATUS_ID, src.CONTACT_ID
				 

INSERT INTO reviewer.DBO.TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT)
SELECT ti.ITEM_ID ITEM_ID,
		tis.ITEM_SET_ID,
		ta.ATTRIBUTE_ID,
		EXTRACT_ATTR_DESC
	FROM EPPI_WEB.DBO.TB_EXTRACT_ATTR as src1
	INNER JOIN EPPI_WEB.DBO.TB_ITEM_REVIEW as src ON src.ITEM_ID = src1.ITEM_ID
	inner join EPPI_WEB.DBO.tb_ITEM i on i.ITEM_ID = src.ITEM_ID
	inner join Reviewer.dbo.TB_ITEM ti on ti.OLD_ITEM_ID = src.ITEM_ID COLLATE Latin1_General_CI_AS
	inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_ID = ti.ITEM_ID
	inner join Reviewer.dbo.TB_REVIEW_SET rs on rs.SET_ID = tis.SET_ID
	inner join Reviewer.dbo.TB_ATTRIBUTE ta on ta.OLD_ATTRIBUTE_ID = src1.ATTRIBUTE_ID COLLATE Latin1_General_CI_AS
	inner join Reviewer.dbo.TB_CONTACT c on src1.CONTACT_ID = c.old_contact_id COLLATE Latin1_General_CI_AS
	WHERE src.REVIEW_ID = @revID and tis.IS_COMPLETED = 'TRUE'  
	ORDER by ITEM_ID
	

INSERT INTO reviewer.DBO.TB_REVIEW_SET(REVIEW_ID, SET_ID, ALLOW_CODING_EDITS)
SELECT (SELECT reviewer.DBO.TB_REVIEW.REVIEW_ID FROM reviewer.DBO.TB_REVIEW
			WHERE reviewer.DBO.TB_REVIEW.OLD_REVIEW_ID = src.REVIEW_ID COLLATE Latin1_General_CI_AS),
		(SELECT reviewer.DBO.TB_SET.SET_ID FROM reviewer.DBO.TB_SET
				WHERE reviewer.DBO.TB_SET.OLD_GUIDELINE_ID = src.GUIDELINE_ID COLLATE Latin1_General_CI_AS),
		'FALSE'
		FROM EPPI_WEB.DBO.TB_REVIEW_GUIDELINE as src
		WHERE src.REVIEW_ID = @revID


-- TRANSFER INDUCTIVE CODES
INSERT INTO reviewer.DBO.TB_SET(OLD_GUIDELINE_ID, SET_TYPE_ID, SET_NAME)
SELECT DISTINCT(REVIEW_ID), 3, 'Inductive codes'  FROM EPPI_WEB.DBO.TB_INDUCTIVE_CODE src
WHERE src.REVIEW_ID = @revID

INSERT INTO reviewer.DBO.TB_REVIEW_SET(REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL)
SELECT REVIEW_ID, SET_ID, 'True', 'True' 
FROM TB_SET INNER JOIN TB_REVIEW ON TB_REVIEW.OLD_REVIEW_ID = TB_SET.OLD_GUIDELINE_ID
WHERE TB_REVIEW.OLD_REVIEW_ID = @revID

INSERT INTO reviewer.DBO.TB_ATTRIBUTE(CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
SELECT (SELECT reviewer.DBO.TB_CONTACT.CONTACT_ID FROM reviewer.DBO.TB_CONTACT
	WHERE reviewer.DBO.TB_CONTACT.OLD_CONTACT_ID = src.CONTACT_ID COLLATE Latin1_General_CI_AS),
	INDUCTIVE_CODE,
	CAST([DESCRIPTION] AS NVARCHAR(2000)),
	CAST('IC' + CAST(src.INDUCTIVE_CODE_ID AS NVARCHAR(6)) AS NVARCHAR(50))
FROM EPPI_WEB.DBO.TB_INDUCTIVE_CODE as src
WHERE src.REVIEW_ID = @revID


-- TWO STAGES - PARENTS WHERE parent_ID = 0 FIRST
INSERT INTO reviewer.DBO.TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_ORDER)
SELECT reviewer.DBO.TB_ATTRIBUTE.ATTRIBUTE_ID, reviewer.DBO.TB_SET.SET_ID, 0, 1, CODE_ORDER
FROM EPPI_WEB.DBO.TB_INDUCTIVE_CODE as src
INNER JOIN reviewer.DBO.TB_ATTRIBUTE ON
	reviewer.DBO.TB_ATTRIBUTE.OLD_ATTRIBUTE_ID = 
			CAST('IC' + CAST(src.INDUCTIVE_CODE_ID AS NVARCHAR(6)) AS NVARCHAR(10)) COLLATE Latin1_General_CI_AS
INNER JOIN reviewer.DBO.TB_SET ON
	reviewer.DBO.TB_SET.OLD_GUIDELINE_ID =
			src.REVIEW_ID COLLATE Latin1_General_CI_AS
WHERE PARENT_ID = 0 AND src.REVIEW_ID = @revID

-- SECOND STAGE - WHERE NOT == 0

INSERT INTO reviewer.DBO.TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID,
	ATTRIBUTE_ORDER)
SELECT (SELECT reviewer.DBO.TB_ATTRIBUTE.ATTRIBUTE_ID FROM reviewer.DBO.TB_ATTRIBUTE
		WHERE reviewer.DBO.TB_ATTRIBUTE.OLD_ATTRIBUTE_ID = 
			CAST('IC' + CAST(src.INDUCTIVE_CODE_ID AS NVARCHAR(6)) AS NVARCHAR(10)) COLLATE Latin1_General_CI_AS),
		(SELECT SET_ID FROM reviewer.DBO.TB_SET WHERE reviewer.DBO.TB_SET.OLD_GUIDELINE_ID =
			src.REVIEW_ID COLLATE Latin1_General_CI_AS),
		(SELECT ATTRIBUTE_ID FROM reviewer.DBO.TB_ATTRIBUTE WHERE OLD_ATTRIBUTE_ID =
			CAST('IC' + CAST(src.PARENT_ID AS NVARCHAR(6)) AS NVARCHAR(10)) COLLATE Latin1_General_CI_AS),
		1,
		CODE_ORDER
		FROM EPPI_WEB.DBO.TB_INDUCTIVE_CODE as src
		WHERE PARENT_ID != 0 AND src.REVIEW_ID = @revID


INSERT INTO reviewer.DBO.TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID)
SELECT DISTINCT reviewer.DBO.TB_ITEM.ITEM_ID, reviewer.DBO.TB_SET.SET_ID, 'True', reviewer.DBO.TB_CONTACT.CONTACT_ID
FROM EPPI_WEB.DBO.TB_INDUCTIVE_CODE_EXTRACT_ATTR as src
INNER JOIN reviewer.DBO.TB_ITEM ON
	reviewer.DBO.TB_ITEM.OLD_ITEM_ID = src.ITEM_ID COLLATE Latin1_General_CI_AS
INNER JOIN EPPI_WEB.DBO.TB_INDUCTIVE_CODE as src1 ON
	src1.INDUCTIVE_CODE_ID = src.INDUCTIVE_CODE_ID
INNER JOIN reviewer.DBO.TB_SET ON
	reviewer.DBO.TB_SET.OLD_GUIDELINE_ID = src1.REVIEW_ID COLLATE Latin1_General_CI_AS
INNER JOIN reviewer.DBO.TB_CONTACT ON
	reviewer.DBO.TB_CONTACT.OLD_CONTACT_ID = src.CONTACT_ID COLLATE Latin1_General_CI_AS
WHERE src1.REVIEW_ID = @revID


INSERT INTO reviewer.DBO.TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
SELECT DISTINCT reviewer.DBO.TB_ITEM.ITEM_ID, reviewer.DBO.TB_ITEM_SET.ITEM_SET_ID, reviewer.DBO.TB_ATTRIBUTE.ATTRIBUTE_ID
FROM EPPI_WEB.DBO.TB_INDUCTIVE_CODE_EXTRACT_ATTR as src
INNER JOIN reviewer.DBO.TB_ITEM ON
	reviewer.DBO.TB_ITEM.OLD_ITEM_ID = src.ITEM_ID COLLATE Latin1_General_CI_AS
INNER JOIN reviewer.DBO.TB_ITEM_SET ON
	reviewer.DBO.TB_ITEM_SET.ITEM_ID = reviewer.DBO.TB_ITEM.ITEM_ID
	INNER JOIN reviewer.DBO.TB_SET ON reviewer.DBO.TB_SET.SET_ID = reviewer.DBO.TB_ITEM_SET.SET_ID
		AND reviewer.DBO.TB_SET.OLD_GUIDELINE_ID = 
					(SELECT REVIEW_ID FROM EPPI_WEB.DBO.tb_INDUCTIVE_CODE as src1
						WHERE src1.INDUCTIVE_CODE_ID =
							src.INDUCTIVE_CODE_ID) COLLATE Latin1_General_CI_AS
INNER JOIN reviewer.DBO.TB_ATTRIBUTE ON
		reviewer.DBO.TB_ATTRIBUTE.OLD_ATTRIBUTE_ID =
			CAST('IC' + CAST(src.INDUCTIVE_CODE_ID AS NVARCHAR(6)) AS NVARCHAR(10)) 
				COLLATE Latin1_General_CI_AS
WHERE reviewer.DBO.TB_SET.OLD_GUIDELINE_ID = @revID

-- TRANSFER TEXT FOR INDUCTIVE CODES AS ITEM DOCUMENTS
INSERT INTO reviewer.DBO.TB_ITEM_DOCUMENT(ITEM_ID, DOCUMENT_TITLE, DOCUMENT_TEXT, OLD_EXTRACT_ATTR_IDENTITY)
SELECT DISTINCT (SELECT reviewer.DBO.TB_ITEM.ITEM_ID FROM reviewer.DBO.TB_ITEM
		WHERE reviewer.DBO.TB_ITEM.OLD_ITEM_ID = src.ITEM_ID COLLATE Latin1_General_CI_AS),
		'Transferred document',
		cast(EXTRACT_ATTR_DESC as nvarchar(max)),
		[IDENTITY]
FROM EPPI_WEB.DBO.TB_EXTRACT_ATTR as src
INNER JOIN EPPI_WEB.DBO.TB_INDUCTIVE_CODE_EXTRACT_ATTR as src1 ON
	src1.ATTRIBUTE_ID = src.ATTRIBUTE_ID
	AND src1.GUIDELINE_ID = src.GUIDELINE_ID
	AND src1.ITEM_ID = src.ITEM_ID
INNER JOIN EPPI_WEB.DBO.TB_INDUCTIVE_CODE as src2 ON
	src1.INDUCTIVE_CODE_ID = src2.INDUCTIVE_CODE_ID
WHERE src2.REVIEW_ID = @revID

declare @ITEM_ATTRIBUTE_ID BIGINT
declare	@ITEM_DOCUMENT_ID BIGINT
declare	@START_AT INT
declare	@END_AT INT

Declare textCursor CURSOR READ_ONLY FOR
SELECT ITEM_DOCUMENT_ID, ITEM_ATTRIBUTE_ID, START_AT, END_AT
	from EPPI_WEB.DBO.TB_EXTRACT_ATTR as src
	INNER JOIN EPPI_WEB.DBO.TB_INDUCTIVE_CODE_EXTRACT_ATTR as src1 ON
	src1.ATTRIBUTE_ID = src.ATTRIBUTE_ID
	AND src1.GUIDELINE_ID = src.GUIDELINE_ID
	AND src1.ITEM_ID = src.ITEM_ID
	inner join EPPI_WEB.DBO.tb_INDUCTIVE_CODE src2
	ON src2.INDUCTIVE_CODE_ID = src1.INDUCTIVE_CODE_ID
	inner join TB_ITEM_DOCUMENT on OLD_EXTRACT_ATTR_IDENTITY = src.[IDENTITY]
	INNER JOIN TB_ITEM_ATTRIBUTE ON
	TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_DOCUMENT.ITEM_ID
	INNER JOIN reviewer.DBO.TB_ATTRIBUTE ON
	reviewer.DBO.TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
	AND reviewer.DBO.TB_ATTRIBUTE.OLD_ATTRIBUTE_ID =
		CAST('IC' + CAST(src1.INDUCTIVE_CODE_ID AS NVARCHAR(6)) AS NVARCHAR(10)) 
				COLLATE Latin1_General_CI_AS
	WHERE src2.REVIEW_ID = @revID
Open textCursor
Fetch next from textCursor into  @ITEM_DOCUMENT_ID, @ITEM_ATTRIBUTE_ID, @START_AT, @END_AT
While @@FETCH_STATUS = 0
Begin
          exec st_ItemAttributeTextInsert @ITEM_ATTRIBUTE_ID, @ITEM_DOCUMENT_ID, @START_AT, @END_AT
			Fetch next from textCursor into @ITEM_DOCUMENT_ID, @ITEM_ATTRIBUTE_ID, @START_AT, @END_AT
End
Close textCursor
Deallocate textCursor

DELETE FROM TB_ITEM_ATTRIBUTE_TEXT WHERE TEXT_TO - TEXT_FROM < 5



-- TRANSFER EXCLUSION CRITERIA

INSERT INTO reviewer.DBO.TB_SET(OLD_GUIDELINE_ID, SET_TYPE_ID, SET_NAME)
SELECT DISTINCT 'EX' + CAST(ELEMENT_GROUP_ID AS NVARCHAR(8)), 
	(select set_type_ID from reviewer.dbo.TB_SET_TYPE where SET_TYPE = 'Review specific keywords')
	, 'Inclusion / exclusion criteria' 
FROM EPPI_WEB.DBO.tb_REFERENCE_ELEMENT_GROUPS src WHERE TYPE = 'EXCLUSION'
	AND src.REVIEW_ID = @revID

INSERT INTO reviewer.DBO.TB_REVIEW_SET(REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL)
SELECT TB_REVIEW.REVIEW_ID, SET_ID, 'True', 'True' FROM TB_SET
INNER JOIN EPPI_WEB.DBO.tb_REFERENCE_ELEMENT_GROUPS as src ON
	'EX' + CAST(src.ELEMENT_GROUP_ID AS NVARCHAR(8)) = TB_SET.OLD_GUIDELINE_ID
INNER JOIN TB_REVIEW ON TB_REVIEW.OLD_REVIEW_ID = src.REVIEW_ID  COLLATE Latin1_General_CI_AS
WHERE src.REVIEW_ID = @revID

INSERT INTO reviewer.DBO.TB_ATTRIBUTE(CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
SELECT NULL,
	ELEMENT_NAME,
	CAST(ELEMENT_CRITERIA AS NVARCHAR(2000)),
	CAST('EX' + CAST(src.ELEMENT_ID AS NVARCHAR(8)) AS NVARCHAR(50))
FROM EPPI_WEB.DBO.tb_REFERENCE_ELEMENTS as src
INNER JOIN EPPI_WEB.DBO.tb_REFERENCE_ELEMENT_GROUPS as src1 on src.ELEMENT_GROUP_ID = src1.ELEMENT_GROUP_ID
WHERE src1.REVIEW_ID = @revID

INSERT INTO reviewer.DBO.TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_ORDER)
SELECT reviewer.DBO.TB_ATTRIBUTE.ATTRIBUTE_ID, reviewer.DBO.TB_SET.SET_ID, 0, 2, ORDER_NUMBER
FROM EPPI_WEB.DBO.tb_REFERENCE_ELEMENTS as src
INNER JOIN reviewer.DBO.TB_ATTRIBUTE ON
	reviewer.DBO.TB_ATTRIBUTE.OLD_ATTRIBUTE_ID = 
			'EX' + CAST(src.ELEMENT_ID AS NVARCHAR(6)) COLLATE Latin1_General_CI_AS
INNER JOIN EPPI_WEB.DBO.tb_REFERENCE_ELEMENT_GROUPS as src1
	ON src1.ELEMENT_GROUP_ID = src.ELEMENT_GROUP_ID
INNER JOIN reviewer.DBO.TB_SET ON
	reviewer.DBO.TB_SET.OLD_GUIDELINE_ID =
			'EX' + CAST(src1.ELEMENT_GROUP_ID AS NVARCHAR(8)) COLLATE Latin1_General_CI_AS
WHERE src1.REVIEW_ID = @revID

-- TRANSFER ITEMS IN TB_REFERENCE_ITEM_FOR_SCREENING

/*
insert into tb_ITEM
(
	TITLE, PARENT_TITLE, 
	OLD_ITEM_ID, ABSTRACT,
	COMMENTS, [TYPE_ID]
)

select 
	TITLE = Case
		WHEN (title_analytic is not null) then title_analytic
		else TITLE_MONO
	END,
	PARENT_TITLE = Case
		When (TITLE_MONO is not null) then TITLE_MONO
		When (SERIES_TITLE is not null) then SERIES_TITLE
		When (JOURNAL is not null) then JOURNAL	
		else ''
	END,
	'SCR' + CAST(ITEM_ID AS NVARCHAR(10)),
	ABSTRACT = CASE 
		WHEN (ABSTRACT is null) then ''
		ELSE ABSTRACT
	END,
	COMMENTS = CASE
		WHEN (NOTES is not null AND EXTENT_OF_WORK is not null AND CONTACT_DETAILS is not null)
			THEN LTRIM(RTRIM(convert(nvarchar(max), NOTES) + ' ' + EXTENT_OF_WORK + ' ' + CONTACT_DETAILS))
		ELSE ''
	end,
	0
from EPPI_WEB.DBO.tb_REFERENCE_ITEM_FOR_SCREENING as src
WHERE src.REVIEW_ID = 'REV373'
*/

	
declare @ITEM_ID BIGINT
declare	@ATTRIBUTE_ID BIGINT
declare	@SET_ID INT
declare	@CONTACT_ID INT
DECLARE @REVIEW_ID INT
Declare textCursor2 CURSOR READ_ONLY FOR
SELECT DISTINCT TB_ITEM.ITEM_ID, SET_ID, TB_CONTACT.CONTACT_ID, ATTRIBUTE_ID, TB_REVIEW.REVIEW_ID
FROM EPPI_WEB.DBO.tb_REFERENCE_ITEM_ELEMENTS as src
INNER JOIN reviewer.DBO.TB_ITEM ON
	reviewer.DBO.TB_ITEM.OLD_ITEM_ID = 'SCR' + CAST(src.ITEM_ID AS NVARCHAR(10)) COLLATE Latin1_General_CI_AS
INNER JOIN EPPI_WEB.DBO.tb_REFERENCE_ELEMENTS as src1 ON
	src1.ELEMENT_ID = src.ELEMENT_ID
INNER JOIN reviewer.DBO.TB_SET ON
	reviewer.DBO.TB_SET.OLD_GUIDELINE_ID = 'EX' + CAST(src1.ELEMENT_GROUP_ID AS NVARCHAR(8)) COLLATE Latin1_General_CI_AS
	AND src1.ELEMENT_ID = src.ELEMENT_ID
INNER JOIN reviewer.DBO.TB_CONTACT ON
	reviewer.DBO.TB_CONTACT.OLD_CONTACT_ID = src.CONTACT_ID COLLATE Latin1_General_CI_AS
INNER JOIN TB_ATTRIBUTE ON
	TB_ATTRIBUTE.OLD_ATTRIBUTE_ID = 'EX' + CAST(src1.ELEMENT_ID AS NVARCHAR(8)) COLLATE Latin1_General_CI_AS
INNER JOIN EPPI_WEB.DBO.tb_REFERENCE_ELEMENT_GROUPS as src2 ON
	src2.ELEMENT_GROUP_ID = src1.ELEMENT_GROUP_ID
INNER JOIN TB_REVIEW ON
	TB_REVIEW.OLD_REVIEW_ID = src2.REVIEW_ID  COLLATE Latin1_General_CI_AS
	where src2.REVIEW_ID = @revID
Open textCursor2
Fetch next from textCursor2 into  @ITEM_ID, @SET_ID, @CONTACT_ID, @ATTRIBUTE_ID, @REVIEW_ID
While @@FETCH_STATUS = 0
Begin
          exec DBO.st_ItemAttributeInsertSimple @ITEM_ID, @SET_ID, @CONTACT_ID, @ATTRIBUTE_ID, '', @REVIEW_ID
			Fetch next from textCursor2 into @ITEM_ID, @SET_ID, @CONTACT_ID, @ATTRIBUTE_ID, @REVIEW_ID
End
Close textCursor2
Deallocate textCursor2


-- ************************************ cleaning data **********************************************

--update tb_set set set_name = rtrim(set_name) where SET_NAME like '% '

--update TB_ITEM_ATTRIBUTE set ADDITIONAL_TEXT = dbo.fn_CLEAN_SIMPLE_TEXT(ADDITIONAL_TEXT)

--update TB_ITEM set TITLE = dbo.fn_CLEAN_SIMPLE_TEXT(title)

--update TB_ITEM set ABSTRACT = dbo.fn_CLEAN_SIMPLE_TEXT(abstract)

--update TB_ITEM set publisher = dbo.fn_CLEAN_SIMPLE_TEXT(publisher)
--update TB_ITEM set URL = REVIEWER.dbo.fn_CLEAN_SIMPLE_TEXT( URL)
-- **************************************************************************************************
