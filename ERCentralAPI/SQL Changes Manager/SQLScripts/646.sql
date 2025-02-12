USE Reviewer
GO
declare @chk int = (SELECT count(*)
		FROM sys.indexes 
		WHERE name='IX_TB_ZOTERO_ITEM_REVIEW_ITEM_REVIEW_ID' AND object_id = OBJECT_ID('[dbo].[TB_ZOTERO_ITEM_REVIEW]'))
If @chk = 1 
BEGIN

CREATE NONCLUSTERED INDEX IX_TB_ZOTERO_ITEM_REVIEW_ITEM_REVIEW_ID
ON [dbo].[TB_ZOTERO_ITEM_REVIEW] ([ITEM_REVIEW_ID])
END 

set @chk = (SELECT count(*)
		FROM sys.indexes 
		WHERE name='IX_TB_ZOTERO_ITEM_DOCUMENT_ITEM_DOC_ID_DOC_KEY' AND object_id = OBJECT_ID('[dbo].[TB_ZOTERO_ITEM_DOCUMENT]'))
If @chk = 1 
BEGIN

	CREATE NONCLUSTERED INDEX IX_TB_ZOTERO_ITEM_DOCUMENT_ITEM_DOC_ID_DOC_KEY
	ON [dbo].[TB_ZOTERO_ITEM_DOCUMENT] ([ItemDocumentId])
	INCLUDE ([DocZoteroKey])
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_RandomAllocate]    Script Date: 05/02/2025 15:30:13 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER   PROCEDURE [dbo].[st_RandomAllocate] (
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@FILTER_TYPE NVARCHAR(255),
	@ATTRIBUTE_ID_FILTER BIGINT,
	@SET_ID_FILTER INT,
	@ATTRIBUTE_ID BIGINT,
	@SET_ID INT,
	@HOW_MANY INT,
	@SAMPLE_NO INT,
	@INCLUDED BIT
)
AS
SET NOCOUNT ON
	--FIRST - check that our destination SET/Attribute is in the right review
	declare @check int
	if @ATTRIBUTE_ID = 0 set @check = (select count(*) from tb_REVIEW_SET where SET_ID = @SET_ID and REVIEW_ID = @REVIEW_ID)
	else set @check = (Select count(*) from tb_REVIEW_SET rs inner join TB_ATTRIBUTE_SET tas on rs.SET_ID = @SET_ID and rs.SET_ID = tas.SET_ID and tas.ATTRIBUTE_ID = @ATTRIBUTE_ID and rs.REVIEW_ID = @REVIEW_ID)

	if @check < 1 return;--we refuse to do anything if the place where we should add the new group codes isn't in the current review


	-- THEN, GET A LIST OF ALL THE ITEM_IDs THAT WE'RE WORKING WITH BY USING THE VARIOUS FILTER OPTIONS
	declare @IIds TABLE (idx BIGINT Primary Key, ui uniqueidentifier , destination bigint null)
--declare @IIds TABLE (idx BIGINT , ui uniqueidentifier , destination bigint null, Primary Key(idx, ui))
IF (@FILTER_TYPE = 'No code / code set filter')
	BEGIN
	INSERT INTO @IIds(idx, ui)
		SELECT TOP (@SAMPLE_NO) PERCENT ITEM_ID, NEWID() as uuu FROM TB_ITEM_REVIEW
			WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
			ORDER BY uuu
	END
	
	-- FILTER BY ALL WITH THIS ATTRIBUTE
	IF (@FILTER_TYPE = 'All with this code')
	BEGIN
	INSERT INTO @IIds(idx, ui)
		SELECT TOP (@SAMPLE_NO) PERCENT TB_ITEM_ATTRIBUTE.ITEM_ID, NEWID() as uuu FROM TB_ITEM_ATTRIBUTE
			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
				AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			ORDER BY uuu
	END
		
	-- FILTER BY ALL WITHOUT THIS ATTRIBUTE
	IF (@FILTER_TYPE = 'All without this code')
	BEGIN
	INSERT INTO @IIds(idx, ui)
		SELECT TOP (@SAMPLE_NO) PERCENT ITEM_ID, NEWID() as uuu  FROM TB_ITEM_REVIEW
			WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
				AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
				AND NOT ITEM_ID IN
				(
					SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
					INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
					INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
					WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
				)
		ORDER BY uuu
	END
	
	-- FILTER BY 'ALL WITHOUT ANY CODES FROM THIS SET'
	IF (@FILTER_TYPE = 'All without any codes from this set')
	BEGIN
	INSERT INTO @IIds(idx, ui)
		SELECT TOP (@SAMPLE_NO) PERCENT ITEM_ID, NEWID() as uuu  FROM TB_ITEM_REVIEW
			WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
			AND NOT ITEM_ID IN
			(
			SELECT TB_ITEM_SET.ITEM_ID FROM TB_ITEM_SET
				INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_SET.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				WHERE TB_ITEM_SET.SET_ID = @SET_ID_FILTER AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			)
		ORDER BY uuu
	END
	
	-- FILTER BY 'ALL WITH ANY CODES FROM THIS SET'
	IF (@FILTER_TYPE = 'All with any codes from this set')
	BEGIN
	INSERT INTO @IIds(idx, ui)
		SELECT TOP (@SAMPLE_NO) PERCENT TB_ITEM_REVIEW.ITEM_ID, NEWID() as uuu  FROM TB_ITEM_REVIEW
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
				AND TB_ITEM_SET.SET_ID = @SET_ID_FILTER
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		ORDER BY uuu
	END
	
	-- NOW WE HAVE OUR LIST, CHECK THAT THERE ARE ENOUGH ITEM IDs IN IT
	DECLARE @CHECK_COUNT INT
	SELECT @CHECK_COUNT = COUNT(*) FROM @IIds

	DECLARE @GROUP1 BIGINT
	DECLARE @GROUP2 BIGINT
	DECLARE @GROUP3 BIGINT
	DECLARE @GROUP4 BIGINT
	DECLARE @GROUP5 BIGINT
	DECLARE @GROUP6 BIGINT
	DECLARE @GROUP7 BIGINT
	DECLARE @GROUP8 BIGINT
	DECLARE @GROUP9 BIGINT
	DECLARE @GROUP10 BIGINT

	IF (@CHECK_COUNT > @HOW_MANY)
	BEGIN
		-- ATTRIBUTE_IDs FOR OUR NEW ATTRIBUTES
		DECLARE @DUMMY_OUTPUT BIGINT -- WE'RE NOT INTERESTED IN THIS VALUE
		
		DECLARE @MAX_INDEX INT = 0 --used for the order of new attributes create

		--How many items per group, and how many would remain out if we didn't account for the remainder
		declare @perGroup int = Round( (select (Count(*)/@HOW_MANY) from @IIds), 0, 1)--we are truncating!
		declare @remainder int = (select (Count(*) - (@HOW_MANY * @perGroup)) from @IIds)
		
		set @DUMMY_OUTPUT = null -- WE'RE NOT INTERESTED IN THIS VALUE
		
		set @MAX_INDEX  = 0
		
		SELECT @MAX_INDEX = MAX(ATTRIBUTE_ORDER) + 1 FROM TB_ATTRIBUTE_SET WHERE PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID AND SET_ID = @SET_ID
		IF (@MAX_INDEX IS NULL) SET @MAX_INDEX = 0
		
		EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 1', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP1 OUTPUT
		SET @MAX_INDEX = @MAX_INDEX + 1

		
		IF (@HOW_MANY > 1)
		BEGIN
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 2', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP2 OUTPUT
		END
		
		IF (@HOW_MANY > 2)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 3', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP3 OUTPUT
		END
		IF (@HOW_MANY > 3)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 4', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP4 OUTPUT
		END
		IF (@HOW_MANY > 4)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 5', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP5 OUTPUT
		END
		IF (@HOW_MANY > 5)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 6', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP6 OUTPUT
		END
		IF (@HOW_MANY > 6)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 7', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP7 OUTPUT
		END
		IF (@HOW_MANY > 7)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 8', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP8 OUTPUT
		END
		IF (@HOW_MANY > 8)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 9', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP9 OUTPUT
		END
		IF (@HOW_MANY > 9)
		BEGIN
			SET @MAX_INDEX = @MAX_INDEX + 1
			EXECUTE st_AttributeSetInsert @SET_ID, @ATTRIBUTE_ID, 1, '', @MAX_INDEX, 'Group 10', '', @CONTACT_ID, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP10 OUTPUT
		END
		
		-- NOW WE DO THE ACTUAL INPUTTING OF VALUES
		-- FIRST, WE HAVE TO CREATE ITEM_SET RECORDS FOR ALL OF THE ITEMS
		
		INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID) 
			SELECT idx, @SET_ID, 'True', @CONTACT_ID FROM @IIds ids
			EXCEPT
			SELECT ITEM_ID, @SET_ID, 'True', @CONTACT_ID FROM TB_ITEM_SET WHERE TB_ITEM_SET.SET_ID = @SET_ID AND IS_COMPLETED = 'True'

		--Associating items with code, we do it in 10 different cases, to keep the code intelligible, see IF (@HOW_MANY = 3) for all details
		IF (@HOW_MANY = 1)
		BEGIN
			--reserve @perGroup items to be assigned to this new code
			update @IIds set destination = @GROUP1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				--ORDER BY NEWID()
		END
		
		IF (@HOW_MANY = 2)
		BEGIN
			--reserve @perGroup items to be assigned to this new code
			--the CASE statement adds 1 ref to the group(s) that might need it, to distribute items as evenly as possible
			update  @IIds set destination = @GROUP1
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds ORDER BY ui)
			update @IIds set destination = @GROUP2 where destination is null

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND ids.destination = @GROUP1
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP2
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
		END
		
		IF (@HOW_MANY = 3)
		BEGIN
			--reserve @perGroup items to be assigned to this new code
			--the CASE statement adds 1 ref to the group(s) that might need it, to distribute items as evenly as possible
			--this is the first IF clause where the full pattern of statements is used. 
			--First, we set the destination ATTRIBUTE_ID in @IIds (first series of 'update' statements), then insert into TB_ITEM_ATTRIBUTE accordingly (series of INSERTs).
			--The Where idx in (...) clause does most of the work. 
			-->>'ORDER BY ui' = get a random selection from the ITEMS that still needs allocating
			-->>'where destination is null' = assign only items that have not been assigned already
			-->>'top(CASE when [...])' this controls how many items get assigned to a given Attribute. We use the @perGroup value, +1 (if items are still in the remainder)
			--We then reduce the value in the remainder if we added one ITEM to @perGroup value in the previous statement.
			--Details change for first and last group, as follows:
			--For Group 1 'destination' is null for all records, so we omit 'where destination is null' in the WHERE sub-query.
			--Before the last group, we don't need to reduce the value in the remainder, as it's always going to be 0 already AND
			--The last group always finishes up whatever has not been assigned already.
			--At this point, all decisions have been made, the insert statements can "just" use the destination value to insert where needed.
			update @IIds set destination = @GROUP1
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP2 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			update @IIds set destination = @GROUP3 where destination is null

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND ids.destination = @GROUP1
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				--ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP2
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP3
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
		END
		
		IF (@HOW_MANY = 4)
		BEGIN
			--reserve @perGroup items to be assigned to this new code
			--the CASE statement adds 1 ref to the group(s) that might need it, to distribute items as evenly as possible
			update @IIds set destination = @GROUP1
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP2 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP3 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			update @IIds set destination = @GROUP4 where destination is null

			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND ids.destination = @GROUP1
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				--ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP2
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP3
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP4
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
		END
		
		IF (@HOW_MANY = 5)
		BEGIN
			--reserve @perGroup items to be assigned to this new code
			--the CASE statement adds 1 ref to the group(s) that might need it, to distribute items as evenly as possible
			update @IIds set destination = @GROUP1
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP2 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP3 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP4 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			update @IIds set destination = @GROUP5 where destination is null
			
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND ids.destination = @GROUP1
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				--ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP2
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP3
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP4
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP5 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP5
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
		END
		
		IF (@HOW_MANY = 6)
		BEGIN
			--reserve @perGroup items to be assigned to this new code
			--the CASE statement adds 1 ref to the group(s) that might need it, to distribute items as evenly as possible
			update @IIds set destination = @GROUP1
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP2 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP3 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP4 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP5 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			update @IIds set destination = @GROUP6 where destination is null
			
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND ids.destination = @GROUP1
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				--ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP2
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP3
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP4
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP5 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP5
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP6 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP6
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
		END
		
		IF (@HOW_MANY = 7)
		BEGIN
			--reserve @perGroup items to be assigned to this new code
			--the CASE statement adds 1 ref to the group(s) that might need it, to distribute items as evenly as possible
			update @IIds set destination = @GROUP1
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP2 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP3 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP4 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP5 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP6 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			update @IIds set destination = @GROUP7 where destination is null
		
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND ids.destination = @GROUP1
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				--ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP2
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP3
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP4
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP5 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP5
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP6 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP6
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP7 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP7
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
		END
		
		IF (@HOW_MANY = 8)
		BEGIN
			--reserve @perGroup items to be assigned to this new code
			--the CASE statement adds 1 ref to the group(s) that might need it, to distribute items as evenly as possible
			update @IIds set destination = @GROUP1
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP2 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP3 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP4 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP5 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP6 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP7 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			update @IIds set destination = @GROUP8 where destination is null
			
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND ids.destination = @GROUP1
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				--ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP2
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP3
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP4
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP5 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP5
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP6 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP6
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP7 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP7
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP8 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP8
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
		END
		
		IF (@HOW_MANY = 9)
		BEGIN
			--reserve @perGroup items to be assigned to this new code
			--the CASE statement adds 1 ref to the group(s) that might need it, to distribute items as evenly as possible
			update @IIds set destination = @GROUP1
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP2 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP3 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP4 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP5 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP6 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP7 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP8 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			update @IIds set destination = @GROUP9 where destination is null
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND ids.destination = @GROUP1
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				--ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP2
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP3
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP4
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP5 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP5
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP6 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP6
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP7 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP7
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP8 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP8
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP9 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP9
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
		END
		
		IF (@HOW_MANY = 10)
		BEGIN
			--reserve @perGroup items to be assigned to this new code
			--the CASE statement adds 1 ref to the group(s) that might need it, to distribute items as evenly as possible
			update @IIds set destination = @GROUP1
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP2 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP3 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP4 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP5 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP6
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP7 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP8 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			if @remainder > 0 set @remainder = @remainder -1
			update @IIds set destination = @GROUP9 
				Where idx in (SELECT top(case when @remainder > 0 then @perGroup + 1 else @perGroup end) idx from @IIds where destination is null ORDER BY ui)
			update @IIds set destination = @GROUP10 where destination is null
			
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND ids.destination = @GROUP1
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				--ORDER BY NEWID()
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP2 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP2
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP3 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP3
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP4 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP4
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1

			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP5 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP5
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP6 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP6
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP7 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP7
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP8 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP8
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP9 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP9
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				
			INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
				SELECT idx, TB_ITEM_SET.ITEM_SET_ID, @GROUP10 FROM @IIds ids
				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.idx AND  ids.destination = @GROUP10
				WHERE TB_ITEM_SET.SET_ID = @SET_ID AND TB_ITEM_SET.IS_COMPLETED = 1
		END
		
	END
GO
