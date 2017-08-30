
USE [Reviewer]
GO
IF (select count(IMPORT_FILTER_ID) from TB_IMPORT_FILTER where IMPORT_FILTER_NAME = 'OVID RIS') < 1
BEGIN --add new OVID RIS filter
INSERT INTO [TB_IMPORT_FILTER]
           ([IMPORT_FILTER_NAME]
           ,[IMPORT_FILTER_NOTES]
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
           ,[KEYWORDS])
     select 'OVID RIS'
           ,'Identical to RIS, but maps "ELEC" to journal'
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
           ,[KEYWORDS] from TB_IMPORT_FILTER where [IMPORT_FILTER_NAME] = 'RIS'
Declare @N_ID int = (select IMPORT_FILTER_ID from TB_IMPORT_FILTER where IMPORT_FILTER_NAME = 'OVID RIS')
Declare @O_ID int = (select IMPORT_FILTER_ID from TB_IMPORT_FILTER where IMPORT_FILTER_NAME = 'RIS')
INSERT INTO [TB_IMPORT_FILTER_TYPE_MAP]
           ([IMPORT_FILTER_ID]
           ,[TYPE_CODE]
           ,[TYPE_REGEX])
     SELECT @N_ID
           ,[TYPE_CODE]
           ,[TYPE_REGEX] FROM TB_IMPORT_FILTER_TYPE_MAP where IMPORT_FILTER_ID = @O_ID

INSERT INTO [Reviewer].[dbo].[TB_IMPORT_FILTER_TYPE_RULE]
           ([IMPORT_FILTER_ID]
           ,[RULE_NAME]
           ,[RULE_REGEX]
           ,[TYPE_CODE])
     SELECT @N_ID
           ,[RULE_NAME]
           ,[RULE_REGEX]
           ,[TYPE_CODE] from TB_IMPORT_FILTER_TYPE_RULE where IMPORT_FILTER_ID = @O_ID

update TB_IMPORT_FILTER_TYPE_MAP set TYPE_REGEX = 'ICOMM' where IMPORT_FILTER_ID = @N_ID and TYPE_REGEX = 'ELEC|ICOMM'
update TB_IMPORT_FILTER_TYPE_MAP set TYPE_REGEX = 'JOUR|JFULL|ELEC' where IMPORT_FILTER_ID = @N_ID and TYPE_REGEX = 'JOUR|JFULL'

END
GO
--increase size of URL field
IF COL_LENGTH('TB_ITEM','URL') < 4000 --each NVARCHAR counts for 2!
BEGIN
	ALTER TABLE TB_ITEM ALTER COLUMN URL NVARCHAR(2000) NULL
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingNextItem]    Script Date: 08/29/2017 16:39:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_TrainingNextItem]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@TRAINING_CODE_SET_ID INT,
	@SIMULATE bit = 0
)

As

SET NOCOUNT ON

		DECLARE @CURRENT_TRAINING_ID INT
	--DECLARE @UPDATED_TRAINING_ITEM TABLE(TRAINING_ITEM_ID INT)

-- FIRST, GET THE CURRENT TRAINING 'RUN' (CAN'T SEND TO THE STORED PROC, AS IT MAY HAVE CHANGED)
	SELECT @CURRENT_TRAINING_ID = MAX(TRAINING_ID) FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID
		AND TIME_STARTED < TIME_ENDED

	--SECOND (ByS) GET the ITEM_ID you need to LOCK (for reuse) This isn't straightfoward as it needs to follow the rules implied in the Sreening tab settings!
	--this new bit includes a bugfix: non completed items (not completed as they are disagreements or b/c there is no auto completion) 
	--got fed to new people even when enough have coded them...
	--ALSO: get rid of stale LOCKS.
	Update TB_TRAINING_ITEM SET CONTACT_ID_CODING = 0, WHEN_LOCKED = NULL
				WHERE TRAINING_ID = @CURRENT_TRAINING_ID AND CONTACT_ID_CODING > 0 and WHEN_LOCKED < DATEADD(hour, -13, GETDATE())
	Declare @ListedItems TABLE(TRAINING_ITEM_ID int, ITEM_ID bigint, RANK int, CODED_COUNT int null)
	
	--insert into table var the items that current user might need to see (excluding the SCREENING_N_PEOPLE setting), that is:
	--those that are not locked by someone else  [AND (ti.CONTACT_ID_CODING = @CONTACT_ID OR ti.CONTACT_ID_CODING = 0)]
	--AND are not [AND tisSel.ITEM_SET_ID is NULL] (already completed OR coded by curr user) [and (tisSel.IS_COMPLETED = 1 OR tisSel.CONTACT_ID = @CONTACT_ID)]
	INSERT into @ListedItems 
		select TRAINING_ITEM_ID , ti.ITEM_ID , RANK , count(tisC.ITEM_SET_ID) as CODED_COUNT FROM
		TB_TRAINING_ITEM ti 
		LEFT OUTER JOIN TB_ITEM_SET tisSel on ti.ITEM_ID = tisSel.ITEM_ID and tisSel.SET_ID = @TRAINING_CODE_SET_ID 
			and (tisSel.IS_COMPLETED = 1 OR tisSel.CONTACT_ID = @CONTACT_ID)
		LEFT OUTER JOIN TB_ITEM_SET tisC on ti.ITEM_ID = tisC.ITEM_ID and tisC.SET_ID = @TRAINING_CODE_SET_ID
		where @CURRENT_TRAINING_ID = ti.TRAINING_ID AND tisSel.ITEM_SET_ID is NULL
		AND (ti.CONTACT_ID_CODING = @CONTACT_ID OR ti.CONTACT_ID_CODING = 0)
		GROUP BY TRAINING_ITEM_ID , ti.ITEM_ID , RANK

--SELECT * from @ListedItems
--SELECT * from @ListedItems where CODED_COUNT < (select SCREENING_N_PEOPLE from TB_REVIEW where REVIEW_ID = @REVIEW_ID)



-- NEXT, LOCK THE ITEM WE'RE GOING TO SEND BACK
	DECLARE @sendingBackTID int
	DECLARE @maxCoders int = 0 
	SELECT @maxCoders = SCREENING_N_PEOPLE from TB_REVIEW where REVIEW_ID = @REVIEW_ID
	IF @maxCoders = 0 OR @maxCoders is null
	BEGIN --We don't care about SCREENING_N_PEOPLE
		select @sendingBackTID = MIN(TRAINING_ITEM_ID) FROM @ListedItems
	END
	ELSE
	BEGIN --We ignore items already screened by enough people, as per SCREENING_N_PEOPLE
		select @sendingBackTID = MIN(TRAINING_ITEM_ID) FROM @ListedItems where CODED_COUNT < @maxCoders
	END
	
	IF @SIMULATE = 0 --we ARE doing it!
	BEGIN
		UPDATE TB_TRAINING_ITEM
			SET CONTACT_ID_CODING = @CONTACT_ID, WHEN_LOCKED = CURRENT_TIMESTAMP
			--OUTPUT INSERTED.TRAINING_ITEM_ID INTO @UPDATED_TRAINING_ITEM
			WHERE
			TRAINING_ITEM_ID = @sendingBackTID
				--(SELECT MIN(TRAINING_ITEM_ID) FROM TB_TRAINING_ITEM TI
				--	LEFT OUTER JOIN TB_ITEM_SET ISET ON ISET.ITEM_ID = TI.ITEM_ID AND (IS_COMPLETED = 'TRUE' OR ISET.CONTACT_ID = @CONTACT_ID) AND SET_ID = @TRAINING_CODE_SET_ID
				--	WHERE (CONTACT_ID_CODING = @CONTACT_ID OR CONTACT_ID_CODING = 0) AND TRAINING_ID = @CURRENT_TRAINING_ID AND ISET.ITEM_ID IS NULL)
	END
	--following ELSE isn't needed: in simulation, we don't need to do anything anymore (ByS)
	--ELSE --JUST a SIMULATION!
	--BEGIN
	--UPDATE TB_TRAINING_ITEM
	--	SET CONTACT_ID_CODING = 0
	--	OUTPUT INSERTED.TRAINING_ITEM_ID INTO @UPDATED_TRAINING_ITEM
	--	WHERE
	--	TRAINING_ITEM_ID = @sendingBackTID
	--		--(SELECT MIN(TRAINING_ITEM_ID) FROM TB_TRAINING_ITEM TI
	--		--	LEFT OUTER JOIN TB_ITEM_SET ISET ON ISET.ITEM_ID = TI.ITEM_ID AND (IS_COMPLETED = 'TRUE' OR ISET.CONTACT_ID = @CONTACT_ID) AND SET_ID = @TRAINING_CODE_SET_ID
	--		--	WHERE (CONTACT_ID_CODING = @CONTACT_ID OR CONTACT_ID_CODING = 0) AND TRAINING_ID = @CURRENT_TRAINING_ID AND ISET.ITEM_ID IS NULL)
	--END

-- FINALLY, SEND IT BACK

	SELECT TI.TRAINING_ITEM_ID, ITEM_ID, [RANK], TRAINING_ID, CONTACT_ID_CODING, SCORE
		FROM TB_TRAINING_ITEM TI
		WHERE TI.TRAINING_ITEM_ID = @sendingBackTID
		--INNER JOIN @UPDATED_TRAINING_ITEM UTI ON UTI.TRAINING_ITEM_ID = TI.TRAINING_ITEM_ID
SET NOCOUNT OFF

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeAutoReconcile]    Script Date: 08/24/2017 10:09:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemAttributeAutoReconcile]
(
	@ITEM_ID BIGINT,
	@SET_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@RECONCILLIATION_TYPE nvarchar(10),
	@N_PEOPLE int,
	@AUTO_EXCLUDE bit,
	@CONTACT_ID int
)

As
SET NOCOUNT ON

DECLARE @COUNT_RECS INT = 0
DECLARE @ITEM_SET_ID INT = 0

IF (@RECONCILLIATION_TYPE = 'no compl')
BEGIN
	SET @N_PEOPLE = 99 --(i.e. we don't do anything - none of the rest is executed)
END
ELSE
BEGIN

	-- **************** STAGE 1: GATHER DATA ON WHETHER RULES FOR AUTO-RECONCILLIATION ARE MET ********************

	IF (@RECONCILLIATION_TYPE = 'Single')
	BEGIN
		SET @COUNT_RECS = 99 -- i.e. we go through to automatic exclude check

		SELECT TOP(1) @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET
			WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID
	END
	
	IF (@RECONCILLIATION_TYPE = 'auto code') -- agreement at the code level
	BEGIN
		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE
			WHERE ITEM_ID = @ITEM_ID AND ATTRIBUTE_ID = @ATTRIBUTE_ID
		IF (@COUNT_RECS >= @N_PEOPLE)
		BEGIN
			SELECT TOP(1) @ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID FROM TB_ITEM_SET
				INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID
				WHERE TB_ITEM_SET.ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND CONTACT_ID = @CONTACT_ID
		END
		ELSE
		BEGIN
			SET @ITEM_SET_ID = 0
		END
	END

	-- one person has to tick 'include' for it to be included.-- N people agreeing on exclude if nobody has ticked 'include' before this threshold is met
	IF (@RECONCILLIATION_TYPE = 'auto safet') 
	BEGIN
		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 -- is anything included?

		SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 AND CONTACT_ID = @CONTACT_ID
		IF (@COUNT_RECS > 0)
		BEGIN
			SET @COUNT_RECS = @N_PEOPLE
		END
		ELSE
		BEGIN
			-- IF NO INCLUDE IS TICKED, HAVE N PEOPLE TICKED EXCLUDE? IF SO, WE DEFAULT TO THIS
			SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 -- EXCLUDED
			SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11  AND CONTACT_ID = @CONTACT_ID
		END
		IF (@COUNT_RECS < @N_PEOPLE)
		BEGIN
			SET @ITEM_SET_ID = 0
		END
	END
	
	 -- agreement at the include / exclude level
	IF (@RECONCILLIATION_TYPE = 'auto excl')
	BEGIN
		SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10 -- INCLUDED
		SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
			INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
			INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
			WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 10  AND CONTACT_ID = @CONTACT_ID

		IF (@COUNT_RECS < @N_PEOPLE)
		BEGIN
			SELECT @COUNT_RECS = COUNT(*) FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11 -- EXCLUDED
			SELECT TOP(1) @ITEM_SET_ID = ISE.ITEM_SET_ID FROM TB_ITEM_ATTRIBUTE IA
				INNER JOIN TB_ITEM_SET ISE ON ISE.ITEM_SET_ID = IA.ITEM_SET_ID
				INNER JOIN TB_ATTRIBUTE_SET AST ON AST.ATTRIBUTE_ID = IA.ATTRIBUTE_ID
				WHERE IA.ITEM_ID = @ITEM_ID AND ISE.SET_ID = @SET_ID AND AST.ATTRIBUTE_TYPE_ID = 11  AND CONTACT_ID = @CONTACT_ID
		END
	END

	

	-- *************************** STAGE 2: AUTO-RECONCILE AND AUTO-COMPLETE ***************************

	IF (@COUNT_RECS >= @N_PEOPLE) AND (@RECONCILLIATION_TYPE != 'Single') -- AUTO-RECONCILE (COMPLETE) WHERE RULES MET
	BEGIN
		DECLARE @CHECK_NONE_COMPLETED INT = 
			(SELECT COUNT(ITEM_SET_ID) FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND IS_COMPLETED = 'TRUE')

		IF (@CHECK_NONE_COMPLETED = 0)
		BEGIN
			UPDATE TB_ITEM_SET
				SET IS_COMPLETED = 'TRUE'
				WHERE ITEM_SET_ID = @ITEM_SET_ID
			UPDATE TB_TRAINING_ITEM
				SET CONTACT_ID_CODING = 0, WHEN_LOCKED = NULL
				WHERE CONTACT_ID_CODING = @CONTACT_ID AND ITEM_ID = @ITEM_ID
		END
	END
	ELSE -- RULES FOR AUTO-COMPLETING ARE NOT MET, SO WE REMOVE THE SCREENING LOCK SO SOMEONE ELSE CAN SCREEN THIS ITEM
	BEGIN
		UPDATE TB_TRAINING_ITEM
			SET CONTACT_ID_CODING = 0, WHEN_LOCKED = NULL
			WHERE CONTACT_ID_CODING = @CONTACT_ID AND ITEM_ID = @ITEM_ID
	END
	IF (@AUTO_EXCLUDE = 'TRUE' AND @ITEM_SET_ID > 0 and @COUNT_RECS >= @N_PEOPLE) -- AUTO EXCLUDE WHERE RULES MET
	BEGIN
		-- SECOND, AUTO INCLUDE / EXCLUDE
		DECLARE @IS_INCLUDED BIT = 'TRUE'
		SELECT TOP(1) @IS_INCLUDED = CASE WHEN ATTRIBUTE_TYPE_ID = 11 THEN 'FALSE' ELSE 'TRUE' END
			FROM TB_ATTRIBUTE_SET
				INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
				WHERE ITEM_SET_ID = @ITEM_SET_ID
		UPDATE TB_ITEM_REVIEW
			SET IS_INCLUDED = @IS_INCLUDED
			WHERE ITEM_ID = @ITEM_ID
	END
END
SET NOCOUNT OFF
GO

--USE [Reviewer]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ItemURLSet]    Script Date: 01/11/2017 15:42:11 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemURLSet]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ItemURLSet]
--GO

--USE [Reviewer]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ItemURLSet]    Script Date: 01/11/2017 15:42:11 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ItemURLSet]
--	-- Add the parameters for the stored procedure here
--	@Rid int,
--	@ItemID bigint,
--	@Contact nvarchar(255),
--	@URL varchar(max),
--	@Result int = -1 OUTPUT -- -1 if fail, 1 otherwise
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    declare @Check int 
--    set @Result = -1
--	set @Check = (SELECT count(ITEM_ID) from TB_ITEM_REVIEW where ITEM_ID = @ItemID and REVIEW_ID = @Rid)
	
--	IF @Check = 1
--	BEGIN
--		UPDATE TB_ITEM set URL = @URL 
--			, EDITED_BY = @Contact
--			, DATE_EDITED = GETDATE()
--		where ITEM_ID = @ItemID
--		set @Result = 1
--	END
	
--END


--GO


----USE [Reviewer]
----GO

----/****** Object:  StoredProcedure [dbo].[st_ItemAttributeBulkAssignCodesFromMLsearchResults]    Script Date: 12/13/2016 14:16:31 ******/
----IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemAttributeBulkAssignCodesFromMLsearchResults]') AND type in (N'P', N'PC'))
----DROP PROCEDURE [dbo].[st_ItemAttributeBulkAssignCodesFromMLsearchResults]
----GO

----USE [Reviewer]
----GO

----/****** Object:  StoredProcedure [dbo].[st_ItemAttributeBulkAssignCodesFromMLsearchResults]    Script Date: 12/13/2016 14:16:31 ******/
----SET ANSI_NULLS ON
----GO

----SET QUOTED_IDENTIFIER ON
----GO

------ =============================================
------ Author:		<Author,,Name>
------ Create date: <Create Date,,>
------ Description:	<Description,,>
------ =============================================
----CREATE PROCEDURE [dbo].[st_ItemAttributeBulkAssignCodesFromMLsearchResults]
----	-- Add the parameters for the stored procedure here
----	@SearchID int,
----	@revID int,
----	@ParentAttributeID bigint,
----	@SetID int,
----	@SearchName nvarchar(4000),
----	@ContactID int
----AS
----BEGIN
----	-- SET NOCOUNT ON added to prevent extra result sets from
----	-- interfering with SELECT statements.
----	SET NOCOUNT ON;

----    -- Phases: 1. create codes using st_AttributeSetInsert
----    -- 2. fetch IDs (comma separated) for each bucket
----    -- 3. assign items to new codes via st_ItemAttributeBulkInsert
----	-- repeat 2 and 3 for each bucket.
	
----	--stuff we'll need:
----	declare @NEW_ATTRIBUTE_SET_ID bigint ,@NEW_ATTRIBUTE_ID_1 bigint
----			,@NEW_ATTRIBUTE_ID_2 bigint
----			,@NEW_ATTRIBUTE_ID_3 bigint
----			,@NEW_ATTRIBUTE_ID_4 bigint
----			,@NEW_ATTRIBUTE_ID_5 bigint
----			,@NEW_ATTRIBUTE_ID_6 bigint
----			,@NEW_ATTRIBUTE_ID_7 bigint
----			,@NEW_ATTRIBUTE_ID_8 bigint
----			,@NEW_ATTRIBUTE_ID_9 bigint
----			,@NEW_ATTRIBUTE_ID_10 bigint
----	declare @IDs varchar(MAX)		
	
----	--1. create codes using st_AttributeSetInsert
	
----	--first of all find the order...
----	declare @order int = (select MAX(ATTRIBUTE_ORDER) from TB_ATTRIBUTE_SET where SET_ID = @SetID and PARENT_ATTRIBUTE_ID = @ParentAttributeID)
----	IF @order = null set @order = 0
----	set @SearchName = 'FROM: ' + @SearchName --used as code description
	
----	--create codes & take IDs
----	set @order  = @order + 1
----	EXECUTE [Reviewer].[dbo].[st_AttributeSetInsert] 
----	   @SetID
----	  ,@ParentAttributeID
----	  ,1
----	  ,@SearchName
----	  ,@order
----	  ,'0-9% range'
----	  ,NULL
----	  ,@ContactID
----	  ,NULL
----	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
----	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_1 OUTPUT
----	set @order  = @order + 1
----	EXECUTE [Reviewer].[dbo].[st_AttributeSetInsert] 
----	   @SetID
----	  ,@ParentAttributeID
----	  ,1
----	  ,@SearchName
----	  ,@order
----	  ,'10-19% range'
----	  ,NULL
----	  ,@ContactID
----	  ,NULL
----	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
----	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_2 OUTPUT
----	set @order  = @order + 1
----	EXECUTE [Reviewer].[dbo].[st_AttributeSetInsert] 
----	   @SetID
----	  ,@ParentAttributeID
----	  ,1
----	  ,@SearchName
----	  ,@order
----	  ,'20-29% range'
----	  ,NULL
----	  ,@ContactID
----	  ,NULL
----	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
----	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_3 OUTPUT
----	set @order  = @order + 1
----	EXECUTE [Reviewer].[dbo].[st_AttributeSetInsert] 
----	   @SetID
----	  ,@ParentAttributeID
----	  ,1
----	  ,@SearchName
----	  ,@order
----	  ,'30-39% range'
----	  ,NULL
----	  ,@ContactID
----	  ,NULL
----	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
----	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_4 OUTPUT
----	set @order  = @order + 1
----	EXECUTE [Reviewer].[dbo].[st_AttributeSetInsert] 
----	   @SetID
----	  ,@ParentAttributeID
----	  ,1
----	  ,@SearchName
----	  ,@order
----	  ,'40-49% range'
----	  ,NULL
----	  ,@ContactID
----	  ,NULL
----	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
----	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_5 OUTPUT
----	EXECUTE [Reviewer].[dbo].[st_AttributeSetInsert] 
----	   @SetID
----	  ,@ParentAttributeID
----	  ,1
----	  ,@SearchName
----	  ,@order
----	  ,'50-59% range'
----	  ,NULL
----	  ,@ContactID
----	  ,NULL
----	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
----	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_6 OUTPUT
----	set @order  = @order + 1
----	EXECUTE [Reviewer].[dbo].[st_AttributeSetInsert] 
----	   @SetID
----	  ,@ParentAttributeID
----	  ,1
----	  ,@SearchName
----	  ,@order
----	  ,'60-69% range'
----	  ,NULL
----	  ,@ContactID
----	  ,NULL
----	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
----	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_7 OUTPUT
----	set @order  = @order + 1
----	EXECUTE [Reviewer].[dbo].[st_AttributeSetInsert] 
----	   @SetID
----	  ,@ParentAttributeID
----	  ,1
----	  ,@SearchName
----	  ,@order
----	  ,'70-79% range'
----	  ,NULL
----	  ,@ContactID
----	  ,NULL
----	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
----	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_8 OUTPUT
----	set @order  = @order + 1
----	EXECUTE [Reviewer].[dbo].[st_AttributeSetInsert] 
----	   @SetID
----	  ,@ParentAttributeID
----	  ,1
----	  ,@SearchName
----	  ,@order
----	  ,'80-89% range'
----	  ,NULL
----	  ,@ContactID
----	  ,NULL
----	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
----	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_9 OUTPUT
----	set @order  = @order + 1
----	EXECUTE [Reviewer].[dbo].[st_AttributeSetInsert] 
----	   @SetID
----	  ,@ParentAttributeID
----	  ,1
----	  ,@SearchName
----	  ,@order
----	  ,'90-99% range'
----	  ,NULL
----	  ,@ContactID
----	  ,NULL
----	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
----	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_10 OUTPUT
	
	
----	--2. fetch IDs (comma separated) for each bucket
----	Declare @Items table (ItemID bigint primary key)
----	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
----		[ITEM_RANK] >= 0 AND [ITEM_RANK] < 10 AND SEARCH_ID = @SearchID
----	set @IDs  = ''
----	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
----	IF LEN(@IDs) > 2
----	BEGIN
----		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
----		--3. Bulk insert
----		exec st_ItemAttributeBulkInsert @SetID,
----			1,
----			@ContactID,
----			@NEW_ATTRIBUTE_ID_1,
----			@IDs,
----			'',
----			@revID
----	END
----	--cleanup
----	DELETE from @Items
----	--REPEAT until all 10 codes are populated
----	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
----		[ITEM_RANK] >= 10 AND [ITEM_RANK] < 20 AND SEARCH_ID = @SearchID
----	set @IDs  = ''
----	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
----	IF LEN(@IDs) > 2
----	BEGIN
----		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
----		exec st_ItemAttributeBulkInsert @SetID,
----			1,
----			@ContactID,
----			@NEW_ATTRIBUTE_ID_2,
----			@IDs,
----			'',
----			@revID
----	END
----	--cleanup
----	DELETE from @Items
----	--REPEAT until all 10 codes are populated
----	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
----		[ITEM_RANK] >= 20 AND [ITEM_RANK] < 30 AND SEARCH_ID = @SearchID
----	set @IDs  = ''
----	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
----	IF LEN(@IDs) > 2
----	BEGIN
----		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
----		exec st_ItemAttributeBulkInsert @SetID,
----			1,
----			@ContactID,
----			@NEW_ATTRIBUTE_ID_3,
----			@IDs,
----			'',
----			@revID
----	END
----	--cleanup
----	DELETE from @Items
----	--REPEAT until all 10 codes are populated
----	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
----		[ITEM_RANK] >= 30 AND [ITEM_RANK] < 40 AND SEARCH_ID = @SearchID
----	set @IDs  = ''
----	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
----	IF LEN(@IDs) > 2
----	BEGIN
----		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
----		exec st_ItemAttributeBulkInsert @SetID,
----			1,
----			@ContactID,
----			@NEW_ATTRIBUTE_ID_4,
----			@IDs,
----			'',
----			@revID
----	END
----	--cleanup
----	DELETE from @Items
----	--REPEAT until all 10 codes are populated
----	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
----		[ITEM_RANK] >= 40 AND [ITEM_RANK] < 50 AND SEARCH_ID = @SearchID
----	set @IDs  = ''
----	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
----	IF LEN(@IDs) > 2
----	BEGIN
----		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
----		exec st_ItemAttributeBulkInsert @SetID,
----			1,
----			@ContactID,
----			@NEW_ATTRIBUTE_ID_5,
----			@IDs,
----			'',
----			@revID
----	END
----	--cleanup
----	DELETE from @Items
----	--REPEAT until all 10 codes are populated
----	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
----		[ITEM_RANK] >= 50 AND [ITEM_RANK] < 60 AND SEARCH_ID = @SearchID
----	set @IDs  = ''
----	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
----	IF LEN(@IDs) > 2
----	BEGIN
----		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
----		exec st_ItemAttributeBulkInsert @SetID,
----			1,
----			@ContactID,
----			@NEW_ATTRIBUTE_ID_6,
----			@IDs,
----			'',
----			@revID
----	END
----	--cleanup
----	DELETE from @Items
----	--REPEAT until all 10 codes are populated
----	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
----		[ITEM_RANK] >= 60 AND [ITEM_RANK] < 70 AND SEARCH_ID = @SearchID
----	set @IDs  = ''
----	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
----	IF LEN(@IDs) > 2
----	BEGIN
----		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
----		exec st_ItemAttributeBulkInsert @SetID,
----			1,
----			@ContactID,
----			@NEW_ATTRIBUTE_ID_7,
----			@IDs,
----			'',
----			@revID
----	END
----	--cleanup
----	DELETE from @Items
----	--REPEAT until all 10 codes are populated
----	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
----		[ITEM_RANK] >= 70 AND [ITEM_RANK] < 80 AND SEARCH_ID = @SearchID
----	set @IDs  = ''
----	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
----	IF LEN(@IDs) > 2
----	BEGIN
----		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
----		exec st_ItemAttributeBulkInsert @SetID,
----			1,
----			@ContactID,
----			@NEW_ATTRIBUTE_ID_8,
----			@IDs,
----			'',
----			@revID
----	END
----	--cleanup
----	DELETE from @Items
----	--REPEAT until all 10 codes are populated
----	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
----		[ITEM_RANK] >= 80 AND [ITEM_RANK] < 90 AND SEARCH_ID = @SearchID
----	set @IDs  = ''
----	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
----	IF LEN(@IDs) > 2
----	BEGIN
----		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
----		exec st_ItemAttributeBulkInsert @SetID,
----			1,
----			@ContactID,
----			@NEW_ATTRIBUTE_ID_9,
----			@IDs,
----			'',
----		@revID
----	END
----	--cleanup
----	DELETE from @Items
----	--REPEAT until all 10 codes are populated
----	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
----		[ITEM_RANK] >= 90 AND [ITEM_RANK] <= 100 AND SEARCH_ID = @SearchID
----	set @IDs  = ''
----	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
----	IF LEN(@IDs) > 2
----	BEGIN
----		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
----		exec st_ItemAttributeBulkInsert @SetID,
----			1,
----			@ContactID,
----			@NEW_ATTRIBUTE_ID_10,
----			@IDs,
----			'',
----			@revID
----	END
----	--cleanup
----	DELETE from @Items
----	--DONE
----END
----GO


----use Reviewer
----go


----/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
----BEGIN TRANSACTION
----SET QUOTED_IDENTIFIER ON
----SET ARITHABORT ON
----SET NUMERIC_ROUNDABORT OFF
----SET CONCAT_NULL_YIELDS_NULL ON
----SET ANSI_NULLS ON
----SET ANSI_PADDING ON
----SET ANSI_WARNINGS ON
----COMMIT
----BEGIN TRANSACTION
----GO
----ALTER TABLE dbo.TB_ITEM_DUPLICATES
----	DROP CONSTRAINT FK_TB_ITEM_DUPLICATES_TB_ITEM
----GO
----ALTER TABLE dbo.TB_ITEM_DUPLICATES
----	DROP CONSTRAINT FK_TB_ITEM_DUPLICATES_TB_ITEM1
----GO
----ALTER TABLE dbo.TB_ITEM SET (LOCK_ESCALATION = TABLE)
----GO
----COMMIT
----BEGIN TRANSACTION
----GO
----ALTER TABLE dbo.TB_ITEM_DUPLICATES SET (LOCK_ESCALATION = TABLE)
----GO
----COMMIT
----GO


----/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
----BEGIN TRANSACTION
----SET QUOTED_IDENTIFIER ON
----SET ARITHABORT ON
----SET NUMERIC_ROUNDABORT OFF
----SET CONCAT_NULL_YIELDS_NULL ON
----SET ANSI_NULLS ON
----SET ANSI_PADDING ON
----SET ANSI_WARNINGS ON
----COMMIT
----BEGIN TRANSACTION
----GO
----CREATE NONCLUSTERED INDEX IX_TB_ITEM_ATTRIBUTE_PDF_DOC_ID ON dbo.TB_ITEM_ATTRIBUTE_PDF
----	(
----	ITEM_DOCUMENT_ID
----	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
----GO
----ALTER TABLE dbo.TB_ITEM_ATTRIBUTE_PDF SET (LOCK_ESCALATION = TABLE)
----GO
----COMMIT
----go


----BEGIN TRANSACTION
----SET QUOTED_IDENTIFIER ON
----SET ARITHABORT ON
----SET NUMERIC_ROUNDABORT OFF
----SET CONCAT_NULL_YIELDS_NULL ON
----SET ANSI_NULLS ON
----SET ANSI_PADDING ON
----SET ANSI_WARNINGS ON
----COMMIT
----BEGIN TRANSACTION
----GO
----CREATE NONCLUSTERED INDEX IX_TB_ITEM_ATTRIBUTE_TEXT_DOC_ID ON dbo.TB_ITEM_ATTRIBUTE_TEXT
----	(
----	ITEM_DOCUMENT_ID
----	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
----GO
----ALTER TABLE dbo.TB_ITEM_ATTRIBUTE_TEXT SET (LOCK_ESCALATION = TABLE)
----GO
----COMMIT
----go


----/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
----BEGIN TRANSACTION
----SET QUOTED_IDENTIFIER ON
----SET ARITHABORT ON
----SET NUMERIC_ROUNDABORT OFF
----SET CONCAT_NULL_YIELDS_NULL ON
----SET ANSI_NULLS ON
----SET ANSI_PADDING ON
----SET ANSI_WARNINGS ON
----COMMIT
----BEGIN TRANSACTION
----GO
----CREATE NONCLUSTERED INDEX IX_TB_SEARCH_ITEM_ITEM_ID ON dbo.TB_SEARCH_ITEM
----	(
----	ITEM_ID
----	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
----GO
----ALTER TABLE dbo.TB_SEARCH_ITEM SET (LOCK_ESCALATION = TABLE)
----GO
----COMMIT
----GO





------USE [Reviewer]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ReportData]    Script Date: 9/12/2016 12:28:52 PM ******/
------SET ANSI_NULLS ON
------GO
------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		<Author,,Name>
-------- Create date: <Create Date,,>
-------- Description:	<Description,,>
-------- =============================================
------ALTER PROCEDURE [dbo].[st_ReportData]
------	-- Add the parameters for the stored procedure here
------	@REVIEW_ID INT
------,	@ITEM_IDS NVARCHAR(MAX)
------,	@REPORT_ID INT
------,	@ORDER_BY NVARCHAR(15)
------,	@ATTRIBUTE_ID BIGINT
------,	@IS_QUESTION bit
------,	@FULL_DETAILS bit
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;
	
------	DECLARE @TT TABLE
------	(
------	  ITEM_ID BIGINT primary key
------	)
------	IF @ATTRIBUTE_ID != 0
------	BEGIN
------		INSERT INTO @TT
------			SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
------			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
------				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
------			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
------				AND TB_ITEM_REVIEW.IS_DELETED = 0
------				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
------			WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
------	END
------	ELSE
------	BEGIN
------		INSERT INTO @TT
------			SELECT VALUE FROM dbo.fn_Split_int(@ITEM_IDS, ',')
------	END
------    --First: the main report properties
------	SELECT * from TB_REPORT where REPORT_ID = @REPORT_ID
------	--Second: list of report columns
------	SELECT * from TB_REPORT_COLUMN where REPORT_ID = @REPORT_ID ORDER BY COLUMN_ORDER
------	--Third: what goes into each column, AKA "Rows" (In C# side)
------	SELECT * from TB_REPORT_COLUMN_CODE  
------		where REPORT_ID = @REPORT_ID ORDER BY CODE_ORDER
	
	
------	--Fourth: most of the real data
------	SELECT distinct cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID,cc.USER_DEF_TEXT
------				,a.*, ia.*, i.ITEM_ID, i.OLD_ITEM_ID, i.SHORT_TITLE, CODE_ORDER, ATTRIBUTE_ORDER 
------	from TB_REPORT_COLUMN_CODE cc
------	INNER JOIN TB_ATTRIBUTE_SET tas ON (--Question reports fetch data about a given code children
------										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
------										OR 
------										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
------									   )
------		AND tas.SET_ID = cc.SET_ID
------	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
------	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
------	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
------	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
------	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
------	where REPORT_ID = @REPORT_ID 
------	ORDER BY 
------		i.SHORT_TITLE -- we ignore the sorting order required for this report, sorting is done on c# side.
------		, i.ITEM_ID, CODE_ORDER, ATTRIBUTE_ORDER
	
	
------	--Fift: data about coded TXT, uses "UNION" to grab data from TXT and PDF tables
------	SELECT cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID, a.ATTRIBUTE_ID, tt.ITEM_ID, id.DOCUMENT_TITLE
------	, 'Page ' + CONVERT(varchar(10),PAGE) + ':' + CHAR(10) + '[¬s]"' + replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' CODED_TEXT
------	  from TB_REPORT_COLUMN_CODE cc
------	INNER JOIN TB_ATTRIBUTE_SET tas ON (
------										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
------										OR 
------										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
------									   )
------		AND tas.SET_ID = cc.SET_ID and cc.REPORT_ID = @REPORT_ID
------	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
------	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
------	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
------	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
------	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
------	inner join TB_ITEM_DOCUMENT id on ia.ITEM_ID = id.ITEM_ID
------	inner join TB_ITEM_ATTRIBUTE_PDF pdf on id.ITEM_DOCUMENT_ID = pdf.ITEM_DOCUMENT_ID and ia.ITEM_ATTRIBUTE_ID = pdf.ITEM_ATTRIBUTE_ID
------	UNION
------	SELECT cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID, a.ATTRIBUTE_ID, tt.ITEM_ID, id.DOCUMENT_TITLE
------	, SUBSTRING(
------					replace(id.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
------				 ) CODED_TEXT
------	  from TB_REPORT_COLUMN_CODE cc
------	INNER JOIN TB_ATTRIBUTE_SET tas ON (
------										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
------										OR 
------										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
------									   )
------		AND tas.SET_ID = cc.SET_ID and cc.REPORT_ID = @REPORT_ID
------	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
------	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
------	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
------	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
------	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
------	inner join TB_ITEM_DOCUMENT id on ia.ITEM_ID = id.ITEM_ID
------	inner join TB_ITEM_ATTRIBUTE_TEXT txt on id.ITEM_DOCUMENT_ID = txt.ITEM_DOCUMENT_ID and ia.ITEM_ATTRIBUTE_ID = txt.ITEM_ATTRIBUTE_ID
	
------	--sixth, items that do not have anything to report
	
------	SELECT i.ITEM_ID, i.OLD_ITEM_ID, i.SHORT_TITLE from TB_ITEM i
------	inner join @TT t on t.ITEM_ID = i.ITEM_ID
------	where t.ITEM_ID not in
------	(SELECT distinct tt.ITEM_ID
------	from TB_REPORT_COLUMN_CODE cc
------	INNER JOIN TB_ATTRIBUTE_SET tas ON (--Question reports fetch data about a given code children
------										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
------										OR 
------										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
------									   )
------		AND tas.SET_ID = cc.SET_ID
------	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
------	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
------	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
------	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
------	where REPORT_ID = @REPORT_ID)
	
	
------	ORDER BY 
------		i.SHORT_TITLE -- we ignore the sorting order required for this report, sorting is done on c# side.
------		, i.ITEM_ID
------	--optional Seventh: get Title, Abstract and Year, only if some of this is needed.
------	if (@FULL_DETAILS = 1)
------	BEGIN
------		select i.ITEM_ID, TITLE, ABSTRACT, [YEAR] from TB_ITEM i
------			inner join @TT t on t.ITEM_ID = i.ITEM_ID
------	END
------END
------GO
------USE [Reviewer]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ItemSetBulkCompleteOnAttribute]    Script Date: 09/26/2016 16:53:21 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemSetBulkCompleteOnAttribute]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ItemSetBulkCompleteOnAttribute]
------GO

------USE [Reviewer]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ItemSetBulkCompleteOnAttribute]    Script Date: 09/26/2016 16:53:21 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

------CREATE procedure [dbo].[st_ItemSetBulkCompleteOnAttribute]
------(
------	@SET_ID INT,
------	@ATTRIBUTE_ID bigint,
------	@COMPLETE BIT,
------	@REVIEW_ID INT,
------	@CONTACT_ID INT,
------	@Affected INT = 0 output
------)

------As

------SET NOCOUNT ON
------declare @Items table (itemID bigint primary key)

--------get all items that have the selection ATTRIBUTE
------insert into @Items select distinct tis.ITEM_ID from TB_ITEM_SET tis
------	inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 1 and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
------	inner join TB_ITEM_REVIEW ir on tis.ITEM_ID = ir.ITEM_ID and REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 0
------delete from @Items where itemID not in 
------	(
------		select tis.ITEM_ID from TB_ITEM_SET tis
------			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED != @COMPLETE and ir.IS_DELETED = 0 and tis.SET_ID = @SET_ID
------						and 
------						(
------						 (--we are completing someone's coding
------							tis.CONTACT_ID = @CONTACT_ID
------							AND
------							@COMPLETE = 1
------						 )
------						OR
------						 (-- we are un-completing everything that has the chosen ATTRIBUTE
------							@COMPLETE = 0
------						 )
------						)
			
------	)
------	UPDATE TB_ITEM_SET
------			SET IS_COMPLETED = @COMPLETE
------			WHERE SET_ID = @SET_ID
------				AND ITEM_ID IN (SELECT itemID from @Items)

------	set @Affected = @@ROWCOUNT

------SET NOCOUNT OFF


------GO
------USE [Reviewer]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ItemSetBulkCompleteOnAttributePreview]    Script Date: 09/26/2016 16:54:11 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemSetBulkCompleteOnAttributePreview]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_ItemSetBulkCompleteOnAttributePreview]
------GO

------USE [Reviewer]
------GO

------/****** Object:  StoredProcedure [dbo].[st_ItemSetBulkCompleteOnAttributePreview]    Script Date: 09/26/2016 16:54:11 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

------CREATE procedure [dbo].[st_ItemSetBulkCompleteOnAttributePreview]
------(
------	@SET_ID INT,
------	@ATTRIBUTE_ID bigint,
------	@COMPLETE BIT,
------	@REVIEW_ID INT,
------	@CONTACT_ID INT,
------	@PotentiallyAffected int = 0 output,
------	@WouldBeAffected INT = 0 output
------)

------As

------SET NOCOUNT ON
------declare @Items table (itemID bigint primary key)

--------get all items that have the selection ATTRIBUTE
------insert into @Items select distinct tis.ITEM_ID from TB_ITEM_SET tis
------	inner join TB_ITEM_ATTRIBUTE ia on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 1 and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
------	inner join TB_ITEM_REVIEW ir on tis.ITEM_ID = ir.ITEM_ID and REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 0
------set @PotentiallyAffected = (select count(itemID) from @Items)
------delete from @Items where itemID not in 
------	(
------		select tis.ITEM_ID from TB_ITEM_SET tis
------			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED != @COMPLETE and ir.IS_DELETED = 0 and tis.SET_ID = @SET_ID
------						and 
------						(
------						 (--we are completing someone's coding
------							tis.CONTACT_ID = @CONTACT_ID
------							AND
------							@COMPLETE = 1
------						 )
------						OR
------						 (-- we are un-completing everything that has the chosen ATTRIBUTE
------							@COMPLETE = 0
------						 )
------						)
			
------	)
------set @WouldBeAffected = (select count(itemID) from @Items)
------SET NOCOUNT OFF


------GO





--------USE [Reviewer]
--------GO
--------/****** Object:  StoredProcedure [dbo].[st_ReviewStatisticsCodeSetsReviewersIncomplete]    Script Date: 7/19/2016 10:22:08 AM ******/
--------SET ANSI_NULLS ON
--------GO
--------SET QUOTED_IDENTIFIER ON
--------GO


--------ALTER procedure [dbo].[st_ReviewStatisticsCodeSetsReviewersIncomplete]
--------(
--------	@REVIEW_ID INT,
--------	@SET_ID INT 
--------)

--------As

--------SET NOCOUNT ON

--------declare @t table (ItemId bigint primary key)
--------insert into @t SELECT distinct IS2.ITEM_ID
--------	FROM TB_ITEM_SET IS2
--------	INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = IS2.ITEM_ID AND IR2.REVIEW_ID = @REVIEW_ID
--------	WHERE IS2.IS_COMPLETED = 'TRUE' AND IS2.SET_ID = @SET_ID

--------SELECT SET_NAME, IS1.SET_ID, IS1.CONTACT_ID, CONTACT_NAME, COUNT(DISTINCT IS1.ITEM_ID) AS TOTAL
--------FROM TB_ITEM_SET IS1
--------INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = IS1.SET_ID
--------INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.REVIEW_ID = TB_REVIEW_SET.REVIEW_ID AND TB_ITEM_REVIEW.ITEM_ID = IS1.ITEM_ID
--------INNER JOIN TB_SET ON TB_SET.SET_ID = IS1.SET_ID
--------INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = IS1.CONTACT_ID
--------WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND IS1.IS_COMPLETED = 'FALSE' 
--------AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND IS1.SET_ID = @SET_ID
--------AND NOT IS1.ITEM_ID IN
--------(
--------	select ItemId from @t
--------)
--------GROUP BY IS1.SET_ID, SET_NAME, IS1.CONTACT_ID, CONTACT_NAME


--------SET NOCOUNT OFF
--------GO

--------USE [Reviewer]
--------GO
--------/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupCheckOngoing]    Script Date: 5/27/2016 2:39:05 PM ******/
--------SET ANSI_NULLS ON
--------GO
--------SET QUOTED_IDENTIFIER ON
--------GO
---------- =============================================
---------- Author:		Sergio
---------- Create date: 09/08/2010
---------- Description:	check for pending SISS, attempt to save results
---------- =============================================
--------ALTER PROCEDURE [dbo].[st_ItemDuplicateGroupCheckOngoing] 
--------	-- Add the parameters for the stored procedure here
--------	@revID int
--------	WITH RECOMPILE
--------AS
--------BEGIN
--------	-- SET NOCOUNT ON added to prevent extra result sets from
--------	-- interfering with SELECT statements.
--------	SET NOCOUNT ON;
--------	Declare @guis_N int
--------    -- Insert statements for procedure here
--------	set @guis_N = (
--------					SELECT COUNT(DISTINCT(EXTR_UI)) from TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID
--------					AND EXTR_UI <> '10000000-0000-0000-0000-000000000000'
--------					)
--------	IF @guis_N = 1
--------	BEGIN --send back a return code to signify that the SISS package is still running
--------		return -2
--------	END
--------	ELSE
--------	IF @guis_N > 1 --SISS package has saved data but results were never collected
--------	BEGIN
--------		declare @UI uniqueidentifier
--------		UPDATE TB_ITEM_DUPLICATES_TEMP
--------			SET EXTR_UI = '10000000-0000-0000-0000-000000000000'
--------			WHERE EXTR_UI = '00000000-0000-0000-0000-000000000000' AND REVIEW_ID = @revID
		
--------		set @UI = (SELECT top 1 EXTR_UI from TB_ITEM_DUPLICATES_TEMP where EXTR_UI <> '10000000-0000-0000-0000-000000000000' AND REVIEW_ID = @revID)
--------		SET @guis_N = (SELECT COUNT(ITEM_DUPLICATES_ID) from TB_ITEM_DUPLICATES_TEMP where EXTR_UI = @UI and DESTINATION is null)
--------		if @guis_N > 0 --do the costly bits only if they weren't done already
--------		BEGIN
--------		--delete sigleton rows from SSIS results
--------			DELETE from TB_ITEM_DUPLICATES_TEMP 
--------			where EXTR_UI = @UI AND _key_out not in
--------				(
--------					Select t1._key_in from TB_ITEM_DUPLICATES_TEMP t1 
--------					inner join TB_ITEM_DUPLICATES_TEMP t2 on t1._key_in = t2._key_out and t1._key_in <> t2._key_in
--------					and t1.EXTR_UI = t2.EXTR_UI and t1.EXTR_UI = @UI
--------						GROUP by t1._key_in
--------				)
			
--------			--the difficult part: match the results in TB_ITEM_DUPLICATES_TEMP with existing groups
--------			-- the system works indifferently for missing groups and missing groups members, and to make it relatively fast, 
--------			-- we store the "destination" group ID in the temporary table, this is done in two parts,
--------			--the following query matches the current SSIS results with existing groups and sets the DESTINATION field accordingly
--------			--the remaining Null "destination" fields will signal that the group is new and has to be created
--------			--after creating the new groups, the destination field will be populated for the remaining records and finally the new group
--------			--members will be added to existing groups.

--------			declare @i1i2 table (i1 int, i2 int) 
--------					insert into @i1i2
--------					select s.ITEM_ID, ss.ITEM_ID  from TB_ITEM_DUPLICATES_TEMP s 
--------										inner join TB_ITEM_DUPLICATES_TEMP ss 
--------										on s._key_out = ss._key_out and s._key_in = ss._key_out 
--------										and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
--------										and s.REVIEW_ID = @revID and ss.REVIEW_ID = @revID

--------			UPDATE dt set DESTINATION = a.ITEM_DUPLICATE_GROUP_ID
--------			 FROM TB_ITEM_DUPLICATES_TEMP dt INNER JOIN   
--------				(
--------					SELECT m.ITEM_DUPLICATE_GROUP_ID, COUNT(m.GROUP_MEMBER_ID) cc, ins._key_out Results_Group 
--------					From TB_ITEM_DUPLICATE_GROUP_MEMBERS m 
--------						inner join TB_ITEM_REVIEW IR on m.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.REVIEW_ID = @revID
--------						inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS m1 
--------							on m.item_duplicate_group_ID = m1.item_duplicate_group_ID
--------							AND m.ITEM_REVIEW_ID <> m1.ITEM_REVIEW_ID
--------						Inner join TB_ITEM_REVIEW IR1 on m1.ITEM_REVIEW_ID = IR1.ITEM_REVIEW_ID and IR1.REVIEW_ID = @revID
--------						inner Join TB_ITEM_DUPLICATE_GROUP g on  m.item_duplicate_group_ID = g.item_duplicate_group_ID
--------						Inner join TB_ITEM_DUPLICATES_TEMP ins on ins.ITEM_ID = IR.ITEM_ID
--------						Inner join @i1i2 i1i2 on i1i2.i1 = IR.ITEM_ID AND i1i2.i2 = IR1.ITEM_ID
--------						where g.Review_ID = @revID and ins.REVIEW_ID = @revID and ins.EXTR_UI = @UI
--------						--AND (CAST(IR.ITEM_ID as nvarchar(20)) + '#' + CAST(IR1.ITEM_ID as nvarchar(20))) in 
--------						--		(
--------						--			select * from @has
--------						--			--select (CAST(s.ITEM_ID as nvarchar(1000)) + '#' + CAST(ss.ITEM_ID as nvarchar(1000))) from TB_ITEM_DUPLICATES_TEMP s 
--------						--			--	inner join TB_ITEM_DUPLICATES_TEMP ss 
--------						--			--	on s._key_out = ss._key_out and s._key_in = ss._key_out 
--------						--			--	and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
--------						--			--	and s.REVIEW_ID = @revID and ss.REVIEW_ID = @revID
--------						--		)
--------					group by m.ITEM_DUPLICATE_GROUP_ID, ins._key_out
--------				) a 
--------				on dt._key_out = a.Results_Group 
--------				WHERE a.cc > 0 and dt.EXTR_UI = @UI

--------			--for groups that are not already present: add group & master
--------			insert into TB_ITEM_DUPLICATE_GROUP (REVIEW_ID, ORIGINAL_ITEM_ID)
--------				SELECT REVIEW_ID, Item_ID from TB_ITEM_DUPLICATES_TEMP where 
--------					EXTR_UI = @UI
--------					AND DESTINATION is null
--------					AND _key_in = _key_out --this is how you identify groups...
--------			--add the master record in the members table
--------			INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
--------			(
--------				ITEM_DUPLICATE_GROUP_ID
--------				,ITEM_REVIEW_ID
--------				,SCORE
--------				,IS_CHECKED
--------				,IS_DUPLICATE
--------			)
--------			SELECT ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID, 1, 1, 0
--------			FROM TB_ITEM_DUPLICATE_GROUP DG inner join TB_ITEM_DUPLICATES_TEMP dt 
--------				on DG.ORIGINAL_ITEM_ID = dt.ITEM_ID
--------				AND EXTR_UI = @UI
--------				AND DESTINATION is null
--------				AND _key_in = _key_out
--------				INNER JOIN TB_ITEM_REVIEW IR  on IR.ITEM_ID = DG.ORIGINAL_ITEM_ID and IR.REVIEW_ID = @revID

--------			--update TB_ITEM_DUPLICATE_GROUP to set the MASTER_MEMBER_ID correctly (now that we have a line in TB_ITEM_DUPLICATE_GROUP_MEMBERS)
--------			UPDATE TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = a.GMI
--------			FROM (
--------					SELECT GROUP_MEMBER_ID GMI, IR.ITEM_ID from TB_ITEM_DUPLICATE_GROUP idg
--------						inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on gm.ITEM_DUPLICATE_GROUP_ID = idg.ITEM_DUPLICATE_GROUP_ID and MASTER_MEMBER_ID is null
--------						inner join TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID AND IR.REVIEW_ID = @revID
--------						inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
--------						AND EXTR_UI = @UI AND dt._key_in = dt._key_out and dt.DESTINATION is null
--------			) a  
--------			WHERE ORIGINAL_ITEM_ID = a.ITEM_ID and MASTER_MEMBER_ID is null

			
--------			-- add the newly created group IDs to temporary table
--------			UPDATE TB_ITEM_DUPLICATES_TEMP set DESTINATION = a.DGI
--------			FROM (
--------				SELECT ITEM_DUPLICATE_GROUP_ID DGI, dt.ITEM_ID MAST, dt1.ITEM_ID CURR_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
--------					inner JOIN TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
--------					inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
--------					AND EXTR_UI = @UI AND dt._key_in = dt._key_out 
--------					inner JOIN TB_ITEM_DUPLICATES_TEMP dt1 on dt._key_in = dt1._key_out and dt.DESTINATION is null
--------					and dt1.EXTR_UI = @UI
--------			) a
--------			where a.CURR_ID = TB_ITEM_DUPLICATES_TEMP.ITEM_ID
--------		END
--------		-- add non master members that are not currently present
--------		declare  @t table (goodIDs bigint)
--------		insert into @t select distinct item_id from TB_ITEM_DUPLICATES_TEMP where EXTR_UI = @UI --and _key_in != _key_out 
--------		--select COUNT (goodids) from @t
--------		delete from @t where goodIDs in (
--------			SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
--------			inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
--------			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
--------			on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
--------			)
--------		--select COUNT (goodids) from @t
		
--------		-- add non master members that are not currently present
--------		INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
--------		(
--------			ITEM_DUPLICATE_GROUP_ID
--------			,ITEM_REVIEW_ID
--------			,SCORE
--------			,IS_CHECKED
--------			,IS_DUPLICATE
--------		)
--------		SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
--------		from TB_ITEM_DUPLICATES_TEMP DT
--------		inner join @t t on DT.ITEM_ID = t.goodIDs
--------		inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
		
--------		--INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
--------		--(
--------		--	ITEM_DUPLICATE_GROUP_ID
--------		--	,ITEM_REVIEW_ID
--------		--	,SCORE
--------		--	,IS_CHECKED
--------		--	,IS_DUPLICATE
--------		--)
--------		--SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
--------		--from TB_ITEM_DUPLICATES_TEMP DT
--------		--inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
--------		--WHERE DT.ITEM_ID not in 
--------		--(
--------		--	SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
--------		--	inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
--------		--	inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
--------		--	on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
--------		--)
		
--------		--remove temporary results.
--------		delete FROM TB_ITEM_DUPLICATES_TEMP WHERE REVIEW_ID = @revID
--------	END
--------END
--------GO

--------USE [Reviewer]
--------GO
--------/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateFindNew]    Script Date: 5/27/2016 4:30:26 PM ******/
--------SET ANSI_NULLS ON
--------GO
--------SET QUOTED_IDENTIFIER ON
--------GO
---------- =============================================
---------- Author:		Sergio
---------- Create date: 18/08/2010
---------- Description:	BIG query to search for duplicates, will not delete or overwrite old items, it will add new duplicate canditades. 
---------- =============================================
--------ALTER PROCEDURE [dbo].[st_ItemDuplicateFindNew]
--------	-- Add the parameters for the stored procedure here
--------	@revID int = 0
--------AS
--------BEGIN
--------	-- SET NOCOUNT ON added to prevent extra result sets from
--------	-- interfering with SELECT statements.
--------	SET NOCOUNT ON;
--------	-- First check: if there are no items to evaluate, just go back
--------	declare @check int = (SELECT COUNT(ITEM_ID) from TB_ITEM_REVIEW where REVIEW_ID = @revID and IS_DELETED = 0)
--------	if @check = 0 
--------	BEGIN
--------		Return -1
--------	END
--------	SET  @check =(SELECT COUNT(DISTINCT(EXTR_UI)) from TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID)
--------	IF @check = 1 
--------	BEGIN
--------		return 1 --a SISS package is still running for Review, we should not run it again
--------	END
--------	ELSE IF @check > 1 --this should not happen: SISS package saved some data, but the result was not collected. 
--------	-- Since new items might have been inserted in the mean time, we will delete old results and start over again.
--------	BEGIN
--------		DELETE FROM TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID 
--------	END

--------	declare @UI uniqueidentifier
--------	set @UI = '00000000-0000-0000-0000-000000000000'
--------	--insert a marker line to notify that the SISS package has been triggered
--------	insert into TB_ITEM_DUPLICATES_TEMP (REVIEW_ID, EXTR_UI) Values (@revID, @UI)

--------	set @UI = NEWID()

--------	--run the package that populates the temp results table
--------	declare @cmd varchar(1000)
--------	select @cmd = 'dtexec /DT "File System\DuplicateCheckAzure"'
--------	select @cmd = @cmd + ' /Rep N  /SET \Package.Variables[User::RevID].Properties[Value];"' + CAST(@revID as varchar(max))+ '"' 
--------	select @cmd = @cmd + ' /SET \Package.Variables[User::UID].Properties[Value];"' + CAST(@UI as varchar(max))+ '"' 
--------	EXEC xp_cmdshell @cmd
	
--------	--delete sigleton rows from SSIS results
--------	DELETE from TB_ITEM_DUPLICATES_TEMP 
--------	where EXTR_UI = @UI AND _key_out not in
--------		(
--------			Select t1._key_in from TB_ITEM_DUPLICATES_TEMP t1 
--------			inner join TB_ITEM_DUPLICATES_TEMP t2 on t1._key_in = t2._key_out and t1._key_in <> t2._key_in
--------			and t1.EXTR_UI = t2.EXTR_UI and t1.EXTR_UI = @UI
--------				GROUP by t1._key_in
--------		)
	
--------	--the difficult part: match the results in TB_ITEM_DUPLICATES_TEMP with existing groups
--------	-- the system works indifferently for missing groups and missing groups members, and to make it relatively fast, 
--------	-- we store the "destination" group ID in the temporary table, this is done in two parts,
--------	--the following query matches the current SSIS results with existing groups and sets the DESTINATION field accordingly
--------	--the remaining Null "destination" fields will signal that the group is new and has to be created
--------	--after creating the new groups, the destination field will be populated for the remaining records and finally the new group
--------	--members will be added to existing groups.

--------	declare @i1i2 table (i1 int, i2 int) 
--------					insert into @i1i2
--------					select s.ITEM_ID, ss.ITEM_ID  from TB_ITEM_DUPLICATES_TEMP s 
--------										inner join TB_ITEM_DUPLICATES_TEMP ss 
--------										on s._key_out = ss._key_out and s._key_in = ss._key_out 
--------										and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
--------										and s.REVIEW_ID = @revID and ss.REVIEW_ID = @revID



--------	UPDATE dt set DESTINATION = a.ITEM_DUPLICATE_GROUP_ID
--------	 FROM TB_ITEM_DUPLICATES_TEMP dt INNER JOIN   
--------		(
--------			SELECT m.ITEM_DUPLICATE_GROUP_ID, COUNT(m.GROUP_MEMBER_ID) cc, ins._key_out Results_Group 
--------				From TB_ITEM_DUPLICATE_GROUP_MEMBERS m 
--------					inner join TB_ITEM_REVIEW IR on m.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.REVIEW_ID = @revID
--------					inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS m1 
--------						on m.item_duplicate_group_ID = m1.item_duplicate_group_ID
--------						AND m.ITEM_REVIEW_ID <> m1.ITEM_REVIEW_ID
--------					Inner join TB_ITEM_REVIEW IR1 on m1.ITEM_REVIEW_ID = IR1.ITEM_REVIEW_ID and IR1.REVIEW_ID = @revID
--------					inner Join TB_ITEM_DUPLICATE_GROUP g on  m.item_duplicate_group_ID = g.item_duplicate_group_ID
--------					Inner join TB_ITEM_DUPLICATES_TEMP ins on ins.ITEM_ID = IR.ITEM_ID
--------					Inner join @i1i2 i1i2 on i1i2.i1 = IR.ITEM_ID AND i1i2.i2 = IR1.ITEM_ID
--------					where g.Review_ID = @revID and ins.REVIEW_ID = @revID and ins.EXTR_UI = @UI
--------					--AND (CAST(IR.ITEM_ID as nvarchar(20)) + '#' + CAST(IR1.ITEM_ID as nvarchar(20))) in 
--------					--		(
--------					--			select * from @has
--------					--			--select (CAST(s.ITEM_ID as nvarchar(1000)) + '#' + CAST(ss.ITEM_ID as nvarchar(1000))) from TB_ITEM_DUPLICATES_TEMP s 
--------					--			--	inner join TB_ITEM_DUPLICATES_TEMP ss 
--------					--			--	on s._key_out = ss._key_out and s._key_in = ss._key_out 
--------					--			--	and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
--------					--			--	and s.REVIEW_ID = @revID and ss.REVIEW_ID = @revID
--------					--		)
--------				group by m.ITEM_DUPLICATE_GROUP_ID, ins._key_out
--------		) a 
--------		on dt._key_out = a.Results_Group
--------		WHERE a.cc > 0  and dt.EXTR_UI = @UI

--------	--for groups that are not already present: add group & master
--------	insert into TB_ITEM_DUPLICATE_GROUP (REVIEW_ID, ORIGINAL_ITEM_ID)
--------		SELECT REVIEW_ID, Item_ID from TB_ITEM_DUPLICATES_TEMP where 
--------			EXTR_UI = @UI
--------			AND DESTINATION is null
--------			AND _key_in = _key_out --this is how you identify groups...
--------	--add the master record in the members table
--------	INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
--------	(
--------		ITEM_DUPLICATE_GROUP_ID
--------		,ITEM_REVIEW_ID
--------		,SCORE
--------		,IS_CHECKED
--------		,IS_DUPLICATE
--------	)
--------	SELECT ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID, 1, 1, 0
--------	FROM TB_ITEM_DUPLICATE_GROUP DG inner join TB_ITEM_DUPLICATES_TEMP dt 
--------		on DG.ORIGINAL_ITEM_ID = dt.ITEM_ID
--------		AND EXTR_UI = @UI
--------		AND DESTINATION is null
--------		AND _key_in = _key_out
--------		INNER JOIN TB_ITEM_REVIEW IR  on IR.ITEM_ID = DG.ORIGINAL_ITEM_ID and IR.REVIEW_ID = @revID

--------	--update TB_ITEM_DUPLICATE_GROUP to set the MASTER_MEMBER_ID correctly (now that we have a line in TB_ITEM_DUPLICATE_GROUP_MEMBERS)
--------	UPDATE TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = a.GMI
--------	FROM (
--------		SELECT GROUP_MEMBER_ID GMI, IR.ITEM_ID from TB_ITEM_DUPLICATE_GROUP idg
--------			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on gm.ITEM_DUPLICATE_GROUP_ID = idg.ITEM_DUPLICATE_GROUP_ID and MASTER_MEMBER_ID is null
--------			inner join TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID AND IR.REVIEW_ID = @revID
--------			inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
--------			AND EXTR_UI = @UI AND dt._key_in = dt._key_out and dt.DESTINATION is null
--------	) a  
--------	WHERE ORIGINAL_ITEM_ID = a.ITEM_ID and MASTER_MEMBER_ID is null

	
--------	-- add the newly created group IDs to temporary table
--------	UPDATE TB_ITEM_DUPLICATES_TEMP set DESTINATION = a.DGI
--------	FROM (
--------		SELECT ITEM_DUPLICATE_GROUP_ID DGI, dt.ITEM_ID MAST, dt1.ITEM_ID CURR_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
--------			inner JOIN TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
--------			inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
--------			AND EXTR_UI = @UI AND dt._key_in = dt._key_out 
--------			inner JOIN TB_ITEM_DUPLICATES_TEMP dt1 on dt._key_in = dt1._key_out and dt.DESTINATION is null
--------			and dt1.EXTR_UI = @UI --!!!!!!!!!!!!!!
--------	) a
--------	where a.CURR_ID = TB_ITEM_DUPLICATES_TEMP.ITEM_ID

--------	-- add non master members that are not currently present
--------	--INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
--------	--(
--------	--	ITEM_DUPLICATE_GROUP_ID
--------	--	,ITEM_REVIEW_ID
--------	--	,SCORE
--------	--	,IS_CHECKED
--------	--	,IS_DUPLICATE
--------	--)
--------	--SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
--------	--from TB_ITEM_DUPLICATES_TEMP DT
--------	--inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
--------	--WHERE DT.ITEM_ID not in 
--------	--(
--------	--	SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
--------	--	inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
--------	--	inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
--------	--	on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
--------	--)
--------	declare  @t table (goodIDs bigint)
--------		insert into @t select distinct item_id from TB_ITEM_DUPLICATES_TEMP where EXTR_UI = @UI --and _key_in != _key_out 
--------		select COUNT (goodids) from @t
--------		delete from @t where goodIDs in (
--------			SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
--------			inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
--------			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
--------			on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
--------			)
--------		select COUNT (goodids) from @t
		
--------		-- add non master members that are not currently present
--------		INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
--------		(
--------			ITEM_DUPLICATE_GROUP_ID
--------			,ITEM_REVIEW_ID
--------			,SCORE
--------			,IS_CHECKED
--------			,IS_DUPLICATE
--------		)
--------		SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
--------		from TB_ITEM_DUPLICATES_TEMP DT
--------		inner join @t t on DT.ITEM_ID = t.goodIDs
--------		inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
--------	--remove temporary results.
--------	delete FROM TB_ITEM_DUPLICATES_TEMP WHERE REVIEW_ID = @revID
--------	END

--------GO


--------USE [Reviewer]
--------GO
--------/****** Object:  StoredProcedure [dbo].[st_OutcomeList]    Script Date: 5/24/2016 5:08:56 PM ******/
--------SET ANSI_NULLS ON
--------GO
--------SET QUOTED_IDENTIFIER ON
--------GO
--------ALTER procedure [dbo].[st_OutcomeList]
--------(
--------	@REVIEW_ID INT,
--------	@META_ANALYSIS_ID INT,
--------	@QUESTIONS nvarchar(max) = '',
--------	@ANSWERS nvarchar(max) = ''
--------	/*@SET_ID BIGINT,
--------	@ITEM_ATTRIBUTE_ID_INTERVENTION BIGINT = NULL,
--------	@ITEM_ATTRIBUTE_ID_CONTROL BIGINT = NULL,
--------	@ITEM_ATTRIBUTE_ID_OUTCOME BIGINT = NULL,
--------	@ATTRIBUTE_ID BIGINT = NULL,
	
	
--------	@VARIABLES NVARCHAR(MAX) = NULL,
--------	@ANSWERS NVARCHAR(MAX) = '',
--------	@QUESTIONS NVARCHAR(MAX) = ''*/
--------)

--------As

--------SET NOCOUNT ON
--------	declare @t table (OUTCOME_ID int, META_ANALYSIS_OUTCOME_ID int)
--------	insert into @t select tio.OUTCOME_ID, META_ANALYSIS_OUTCOME_ID from 
--------	TB_ITEM_OUTCOME tio
--------	inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tio.ITEM_SET_ID
--------		AND tis.IS_COMPLETED = 1
--------	inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @REVIEW_ID and rs.SET_ID = tis.SET_ID
--------	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tis.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--------	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tis.ITEM_SET_ID
--------	inner join TB_ITEM on TB_ITEM.ITEM_ID = tis.ITEM_ID
--------	inner JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
--------		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID

--------	SELECT distinct tio.OUTCOME_ID, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
--------	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, SHORT_TITLE, OUTCOME_DESCRIPTION,
--------	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
--------	t.META_ANALYSIS_OUTCOME_ID, 
--------	tis.ITEM_SET_ID, TB_ITEM_ATTRIBUTE.ITEM_ID,
--------	A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT, A3.ATTRIBUTE_NAME OUTCOME_TEXT
--------	FROM TB_ITEM_OUTCOME tio
--------	inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tio.ITEM_SET_ID
--------		AND tis.IS_COMPLETED = 1
--------	inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @REVIEW_ID and rs.SET_ID = tis.SET_ID
--------	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tis.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--------	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tis.ITEM_SET_ID
--------	inner join TB_ITEM on TB_ITEM.ITEM_ID = tis.ITEM_ID
--------	LEFT OUTER JOIN @t t ON t.OUTCOME_ID = tio.OUTCOME_ID
--------		--AND t.META_ANALYSIS_ID = @META_ANALYSIS_ID
--------	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_INTERVENTION 
--------	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_CONTROL
--------	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_OUTCOME

--------	--second sets of results, the answers
--------	--we need to get these, even if empty, so that we always get a reader
	
--------	IF (@QUESTIONS is not null AND @QUESTIONS != '')
--------	BEGIN
--------		declare @QT table ( AttID bigint primary key)
--------		insert into @QT select qss.value from dbo.fn_Split_int(@QUESTIONS, ',') as qss
--------		select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID 
--------			, ATTRIBUTE_NAME as Codename
--------			, a.ATTRIBUTE_ID as ATTRIBUTE_ID
--------			, tas.PARENT_ATTRIBUTE_ID
--------		from TB_ITEM_OUTCOME tio 
--------		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
--------		inner join TB_ITEM_SET tis2 on tis.ITEM_ID = tis2.ITEM_ID
--------		inner join TB_ITEM_ATTRIBUTE tia on tis2.ITEM_SET_ID = tia.ITEM_SET_ID
--------		inner join TB_REVIEW_SET rs on tis2.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
--------		inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
--------		inner join @QT Qs on Qs.AttID = tas.PARENT_ATTRIBUTE_ID
--------		inner join TB_ATTRIBUTE a on tas.ATTRIBUTE_ID = a.ATTRIBUTE_ID
--------		order by OUTCOME_ID, tas.PARENT_ATTRIBUTE_ID, tas.ATTRIBUTE_ORDER, a.ATTRIBUTE_ID
--------	END
--------	ELSE
--------	BEGIN
--------	select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
--------		tio.OUTCOME_TITLE as Codename
--------		, tio.ITEM_ATTRIBUTE_ID_CONTROL as ATTRIBUTE_ID, tio.ITEM_ATTRIBUTE_ID_CONTROL as PARENT_ATTRIBUTE_ID
--------		from TB_ITEM_OUTCOME tio
--------		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and 1=0
--------	END


--------	IF (@ANSWERS is not null AND @ANSWERS != '')
--------	BEGIN
--------	--third set of results, the questions
--------	declare @AT table ( AttID bigint primary key)
--------	insert into @AT select qss.value from dbo.fn_Split_int(@ANSWERS, ',') as qss
--------	select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
--------		--(Select top(1) a.ATTRIBUTE_NAME from @AT 
--------		--inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = AttID
--------		--inner join TB_ATTRIBUTE a on tas.ATTRIBUTE_ID = a.ATTRIBUTE_ID
--------		--inner join TB_ITEM_ATTRIBUTE tia on a.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
--------		--inner join TB_ITEM_SET tis1 on tia.ITEM_SET_ID = tis1.ITEM_SET_ID and tis1.IS_COMPLETED = 1 and tis.ITEM_ID = tis1.ITEM_ID
--------		--inner join TB_REVIEW_SET rs1 on tis1.SET_ID = rs1.SET_ID and rs1.REVIEW_ID = @REVIEW_ID
--------		--order by tas.ATTRIBUTE_ORDER ) as Codename
--------		tia.ATTRIBUTE_ID, a.ATTRIBUTE_NAME
--------	from TB_ITEM_OUTCOME tio 
--------	inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
--------	inner join TB_REVIEW_SET rs on tis.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
--------	inner join TB_ITEM_SET tis2 on tis.ITEM_ID = tis2.ITEM_ID and tis2.IS_COMPLETED = 1
--------	inner join TB_ATTRIBUTE_SET tas on tis2.SET_ID = tas.SET_ID
--------	inner join TB_ITEM_ATTRIBUTE tia on tis2.ITEM_ID = tia.ITEM_ID and tas.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
--------	inner join @AT on AttID = tia.ATTRIBUTE_ID
--------	inner join TB_ATTRIBUTE a on tia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
--------	order by OUTCOME_ID
--------	END
--------	ELSE
--------	BEGIN
--------		select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
--------		tio.ITEM_ATTRIBUTE_ID_CONTROL as ATTRIBUTE_ID, tio.OUTCOME_TITLE as ATTRIBUTE_NAME
--------	from TB_ITEM_OUTCOME tio inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and 1=0
	
--------	END
	
----------DECLARE @START_TEXT NVARCHAR(MAX) = N' SELECT distinct tio.OUTCOME_ID, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
----------	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, SHORT_TITLE, OUTCOME_DESCRIPTION,
----------	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
----------	TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_OUTCOME_ID, TB_ITEM_SET.ITEM_SET_ID, TB_ITEM_ATTRIBUTE.ITEM_ID,
----------	A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT, A3.ATTRIBUTE_NAME OUTCOME_TEXT'
	
----------DECLARE @END_TEXT NVARCHAR(MAX) = N' FROM TB_ITEM_OUTCOME tio

----------	inner join TB_ITEM_SET on tb_item_set.ITEM_SET_ID = tio.ITEM_SET_ID
----------		AND TB_ITEM_SET.IS_COMPLETED = ''TRUE''
----------	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tb_item_set.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
----------	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tb_item_set.ITEM_SET_ID
----------	inner join TB_ITEM on TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
	
----------	LEFT OUTER JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
----------		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID
		
----------	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_INTERVENTION 
----------	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_CONTROL
----------	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_OUTCOME'
	
----------DECLARE @QUERY NVARCHAR(MAX) = @VARIABLES + @START_TEXT + @ANSWERS + @QUESTIONS + @END_TEXT
	
----------EXEC (@QUERY)

----------/*
----------SELECT distinct tio.OUTCOME_ID, SHORT_TITLE, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
----------	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, OUTCOME_DESCRIPTION,
----------	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
----------	TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_OUTCOME_ID, TB_ITEM_SET.ITEM_SET_ID,
----------	TB_ITEM_ATTRIBUTE.ITEM_ID, A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT,
----------		A3.ATTRIBUTE_NAME OUTCOME_TEXT
	
----------	FROM TB_ITEM_OUTCOME tio

----------	inner join TB_ITEM_SET on tb_item_set.ITEM_SET_ID = tio.ITEM_SET_ID
----------		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----------	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tb_item_set.ITEM_ID
----------	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tb_item_set.ITEM_SET_ID
----------	inner join TB_ITEM on TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
	
----------	LEFT OUTER JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
----------		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID
		
----------	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_INTERVENTION 
----------	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_CONTROL
----------	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_OUTCOME
	
----------	WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
----------	AND (@ITEM_ATTRIBUTE_ID_INTERVENTION = 0 OR (ITEM_ATTRIBUTE_ID_INTERVENTION = @ITEM_ATTRIBUTE_ID_INTERVENTION))
----------	AND (@ITEM_ATTRIBUTE_ID_CONTROL = 0 OR (ITEM_ATTRIBUTE_ID_CONTROL = @ITEM_ATTRIBUTE_ID_CONTROL))
----------	AND (@ITEM_ATTRIBUTE_ID_OUTCOME = 0 OR (ITEM_ATTRIBUTE_ID_OUTCOME = @ITEM_ATTRIBUTE_ID_OUTCOME))
----------	--	AND (@ATTRIBUTE_ID IS NULL OR (TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID))
----------	--AND (
----------	--	@ATTRIBUTE_ID IS NULL OR 
----------	--		(
----------	--		TB_ITEM_SET.ITEM_ID IN
----------	--			( 
----------	--			SELECT IA2.ITEM_ID FROM TB_ITEM_ATTRIBUTE IA2 
----------	--			INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
----------	--			WHERE IA2.ATTRIBUTE_ID = @ATTRIBUTE_ID
----------	--			)
----------	--		)
----------	--	)
----------	--AND (--temp correction for before publishing: @ATTRIBUTE_ID is (because of bug) actually the item_attribute_id
----------	--	@ATTRIBUTE_ID = 0 OR 
----------	--		(
----------	--		tio.OUTCOME_ID IN
----------	--			( 
----------	--				select tio2.OUTCOME_ID from TB_ATTRIBUTE_SET tas
----------	--				inner join TB_ITEM_OUTCOME_ATTRIBUTE ioa on tas.ATTRIBUTE_ID = ioa.ATTRIBUTE_ID and tas.ATTRIBUTE_SET_ID = @ATTRIBUTE_ID
----------	--				inner join TB_ITEM_OUTCOME tio2 on ioa.OUTCOME_ID = tio2.OUTCOME_ID
----------	--			)
----------	--		)
----------	--	)--end of temp correction
----------	AND (--real correction to use when bug is corrected in line 174 of dialogMetaAnalysisSetup.xaml.cs
----------		@ATTRIBUTE_ID = 0 OR 
----------			(
----------			tio.OUTCOME_ID IN 
----------				( 
----------					select tio2.OUTCOME_ID from TB_ITEM_OUTCOME_ATTRIBUTE ioa  
----------					inner join TB_ITEM_OUTCOME tio2 on ioa.OUTCOME_ID = tio2.OUTCOME_ID and ioa.ATTRIBUTE_ID = @ATTRIBUTE_ID
----------				)
----------			)
----------		)--end of real correction
----------	AND (@SET_ID = 0 OR (TB_ITEM_SET.SET_ID = @SET_ID))
	
----------	--order by TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
----------*/
--------SET NOCOUNT OFF
--------GO

--------USE [Reviewer]
--------GO
--------CREATE NONCLUSTERED INDEX [IX_REVIEW_ID]
--------ON [dbo].[TB_REVIEW_SET] ([REVIEW_ID])

--------GO

--------CREATE NONCLUSTERED INDEX [IX_ITEM_DUPLICATE_GROUP_ID_ITEM_REVIEW_ID]
--------ON [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS] ([ITEM_DUPLICATE_GROUP_ID])
--------INCLUDE ([ITEM_REVIEW_ID])
--------GO

--------CREATE NONCLUSTERED INDEX [IX_REVIEW_ID_ITEM_DUPLICATE_GROUP_ID]
--------ON [dbo].[TB_ITEM_DUPLICATE_GROUP] ([REVIEW_ID])
--------INCLUDE ([ITEM_DUPLICATE_GROUP_ID])
--------GO

----------USE [Reviewer]
----------GO
----------/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateFindNew]    Script Date: 03/11/2016 12:17:40 ******/
----------SET ANSI_NULLS ON
----------GO
----------SET QUOTED_IDENTIFIER ON
----------GO
------------ =============================================
------------ Author:		Sergio
------------ Create date: 18/08/2010
------------ Description:	BIG query to search for duplicates, will not delete or overwrite old items, it will add new duplicate canditades. 
------------ =============================================
----------ALTER PROCEDURE [dbo].[st_ItemDuplicateFindNew]
----------	-- Add the parameters for the stored procedure here
----------	@revID int = 0
----------AS
----------BEGIN
----------	-- SET NOCOUNT ON added to prevent extra result sets from
----------	-- interfering with SELECT statements.
----------	SET NOCOUNT ON;
----------	-- First check: if there are no items to evaluate, just go back
----------	declare @check int = (SELECT COUNT(ITEM_ID) from TB_ITEM_REVIEW where REVIEW_ID = @revID and IS_DELETED = 0)
----------	if @check = 0 
----------	BEGIN
----------		Return -1
----------	END
----------	SET  @check =(SELECT COUNT(DISTINCT(EXTR_UI)) from TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID)
----------	IF @check = 1 
----------	BEGIN
----------		return 1 --a SISS package is still running for Review, we should not run it again
----------	END
----------	ELSE IF @check > 1 --this should not happen: SISS package saved some data, but the result was not collected. 
----------	-- Since new items might have been inserted in the mean time, we will delete old results and start over again.
----------	BEGIN
----------		DELETE FROM TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID 
----------	END

----------	declare @UI uniqueidentifier
----------	set @UI = '00000000-0000-0000-0000-000000000000'
----------	--insert a marker line to notify that the SISS package has been triggered
----------	insert into TB_ITEM_DUPLICATES_TEMP (REVIEW_ID, EXTR_UI) Values (@revID, @UI)

----------	set @UI = NEWID()

----------	--run the package that populates the temp results table
----------	declare @cmd varchar(1000)
----------	select @cmd = 'dtexec /DT "File System\DuplicateCheckAzure"'
----------	select @cmd = @cmd + ' /Rep N  /SET \Package.Variables[User::RevID].Properties[Value];"' + CAST(@revID as varchar(max))+ '"' 
----------	select @cmd = @cmd + ' /SET \Package.Variables[User::UID].Properties[Value];"' + CAST(@UI as varchar(max))+ '"' 
----------	EXEC xp_cmdshell @cmd
	
----------	--delete sigleton rows from SSIS results
----------	DELETE from TB_ITEM_DUPLICATES_TEMP 
----------	where EXTR_UI = @UI AND _key_out not in
----------		(
----------			Select t1._key_in from TB_ITEM_DUPLICATES_TEMP t1 
----------			inner join TB_ITEM_DUPLICATES_TEMP t2 on t1._key_in = t2._key_out and t1._key_in <> t2._key_in
----------			and t1.EXTR_UI = t2.EXTR_UI and t1.EXTR_UI = @UI
----------				GROUP by t1._key_in
----------		)
	
----------	--the difficult part: match the results in TB_ITEM_DUPLICATES_TEMP with existing groups
----------	-- the system works indifferently for missing groups and missing groups members, and to make it relatively fast, 
----------	-- we store the "destination" group ID in the temporary table, this is done in two parts,
----------	--the following query matches the current SSIS results with existing groups and sets the DESTINATION field accordingly
----------	--the remaining Null "destination" fields will signal that the group is new and has to be created
----------	--after creating the new groups, the destination field will be populated for the remaining records and finally the new group
----------	--members will be added to existing groups.
----------	UPDATE dt set DESTINATION = a.ITEM_DUPLICATE_GROUP_ID
----------	 FROM TB_ITEM_DUPLICATES_TEMP dt INNER JOIN   
----------		(
----------			SELECT m.ITEM_DUPLICATE_GROUP_ID, COUNT(m.GROUP_MEMBER_ID) cc, ins._key_out Results_Group 
----------			From TB_ITEM_DUPLICATE_GROUP_MEMBERS m 
----------				inner join TB_ITEM_REVIEW IR on m.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.REVIEW_ID = @revID
----------				inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS m1 
----------					on m.item_duplicate_group_ID = m1.item_duplicate_group_ID
----------					AND m.ITEM_REVIEW_ID <> m1.ITEM_REVIEW_ID
----------				Inner join TB_ITEM_REVIEW IR1 on m1.ITEM_REVIEW_ID = IR1.ITEM_REVIEW_ID and IR1.REVIEW_ID = @revID
----------				inner Join TB_ITEM_DUPLICATE_GROUP g on  m.item_duplicate_group_ID = g.item_duplicate_group_ID
----------				Inner join TB_ITEM_DUPLICATES_TEMP ins on ins.ITEM_ID = IR.ITEM_ID
----------				where g.Review_ID = @revID 
----------				AND (CAST(IR.ITEM_ID as nvarchar(1000)) + '#' + CAST(IR1.ITEM_ID as nvarchar(1000))) in 
----------						(
----------							select (CAST(s.ITEM_ID as nvarchar(1000)) + '#' + CAST(ss.ITEM_ID as nvarchar(1000))) from TB_ITEM_DUPLICATES_TEMP s 
----------								inner join TB_ITEM_DUPLICATES_TEMP ss 
----------								on s._key_out = ss._key_out and s._key_in = ss._key_out 
----------								and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
----------						)
----------			group by m.ITEM_DUPLICATE_GROUP_ID, ins._key_out
----------		) a 
----------		on dt._key_out = a.Results_Group
----------		WHERE a.cc > 0  and dt.EXTR_UI = @UI

----------	--for groups that are not already present: add group & master
----------	insert into TB_ITEM_DUPLICATE_GROUP (REVIEW_ID, ORIGINAL_ITEM_ID)
----------		SELECT REVIEW_ID, Item_ID from TB_ITEM_DUPLICATES_TEMP where 
----------			EXTR_UI = @UI
----------			AND DESTINATION is null
----------			AND _key_in = _key_out --this is how you identify groups...
----------	--add the master record in the members table
----------	INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
----------	(
----------		ITEM_DUPLICATE_GROUP_ID
----------		,ITEM_REVIEW_ID
----------		,SCORE
----------		,IS_CHECKED
----------		,IS_DUPLICATE
----------	)
----------	SELECT ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID, 1, 1, 0
----------	FROM TB_ITEM_DUPLICATE_GROUP DG inner join TB_ITEM_DUPLICATES_TEMP dt 
----------		on DG.ORIGINAL_ITEM_ID = dt.ITEM_ID
----------		AND EXTR_UI = @UI
----------		AND DESTINATION is null
----------		AND _key_in = _key_out
----------		INNER JOIN TB_ITEM_REVIEW IR  on IR.ITEM_ID = DG.ORIGINAL_ITEM_ID and IR.REVIEW_ID = @revID

----------	--update TB_ITEM_DUPLICATE_GROUP to set the MASTER_MEMBER_ID correctly (now that we have a line in TB_ITEM_DUPLICATE_GROUP_MEMBERS)
----------	UPDATE TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = a.GMI
----------	FROM (
----------		SELECT GROUP_MEMBER_ID GMI, IR.ITEM_ID from TB_ITEM_DUPLICATE_GROUP idg
----------			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on gm.ITEM_DUPLICATE_GROUP_ID = idg.ITEM_DUPLICATE_GROUP_ID and MASTER_MEMBER_ID is null
----------			inner join TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID AND IR.REVIEW_ID = @revID
----------			inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
----------			AND EXTR_UI = @UI AND dt._key_in = dt._key_out and dt.DESTINATION is null
----------	) a  
----------	WHERE ORIGINAL_ITEM_ID = a.ITEM_ID and MASTER_MEMBER_ID is null

	
----------	-- add the newly created group IDs to temporary table
----------	UPDATE TB_ITEM_DUPLICATES_TEMP set DESTINATION = a.DGI
----------	FROM (
----------		SELECT ITEM_DUPLICATE_GROUP_ID DGI, dt.ITEM_ID MAST, dt1.ITEM_ID CURR_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
----------			inner JOIN TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
----------			inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
----------			AND EXTR_UI = @UI AND dt._key_in = dt._key_out 
----------			inner JOIN TB_ITEM_DUPLICATES_TEMP dt1 on dt._key_in = dt1._key_out and dt.DESTINATION is null
----------			and dt1.EXTR_UI = @UI --!!!!!!!!!!!!!!
----------	) a
----------	where a.CURR_ID = TB_ITEM_DUPLICATES_TEMP.ITEM_ID

----------	-- add non master members that are not currently present
----------	--INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
----------	--(
----------	--	ITEM_DUPLICATE_GROUP_ID
----------	--	,ITEM_REVIEW_ID
----------	--	,SCORE
----------	--	,IS_CHECKED
----------	--	,IS_DUPLICATE
----------	--)
----------	--SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
----------	--from TB_ITEM_DUPLICATES_TEMP DT
----------	--inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
----------	--WHERE DT.ITEM_ID not in 
----------	--(
----------	--	SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
----------	--	inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
----------	--	inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
----------	--	on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
----------	--)
----------	declare  @t table (goodIDs bigint)
----------		insert into @t select distinct item_id from TB_ITEM_DUPLICATES_TEMP where EXTR_UI = @UI --and _key_in != _key_out 
----------		select COUNT (goodids) from @t
----------		delete from @t where goodIDs in (
----------			SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
----------			inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
----------			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
----------			on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
----------			)
----------		select COUNT (goodids) from @t
		
----------		-- add non master members that are not currently present
----------		INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
----------		(
----------			ITEM_DUPLICATE_GROUP_ID
----------			,ITEM_REVIEW_ID
----------			,SCORE
----------			,IS_CHECKED
----------			,IS_DUPLICATE
----------		)
----------		SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
----------		from TB_ITEM_DUPLICATES_TEMP DT
----------		inner join @t t on DT.ITEM_ID = t.goodIDs
----------		inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
----------	--remove temporary results.
----------	delete FROM TB_ITEM_DUPLICATES_TEMP WHERE REVIEW_ID = @revID
----------	END
----------GO


------------USE [Reviewer]
------------GO
------------/****** Object:  StoredProcedure [dbo].[st_ComparisonStats]    Script Date: 12/14/2015 10:37:37 ******/
------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------ALTER procedure [dbo].[st_ComparisonStats]
------------(
------------	@COMPARISON_ID INT,
------------	@Is_Screening bit OUTPUT
------------)
--------------with recompile
------------As

------------SET NOCOUNT ON

------------set @Is_Screening = (Select IS_SCREENING from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)

------------declare @n1 int , @n2 int , @n3 int --Total N items coded reviewer 1,2 & 3 (snapshot)
------------declare @c1 int = (select CONTACT_ID1 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
------------declare @c2 int = (select CONTACT_ID2 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
------------declare @c3 int = (select CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
------------declare @set int = (select SET_ID from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
------------DECLARE @T1 TABLE
------------	(
------------	  ITEM_ID BIGINT,
------------	  ATTRIBUTE_ID BIGINT,
------------	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
------------	)
------------DECLARE @T1c TABLE --current attributes Reviewer1
------------	(
------------	  ITEM_ID BIGINT,
------------	  ATTRIBUTE_ID BIGINT,
------------	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
------------	)
------------DECLARE @T2 TABLE
------------	(
------------	  ITEM_ID BIGINT,
------------	  ATTRIBUTE_ID BIGINT,
------------	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
------------	)
------------DECLARE @T2c TABLE --current attributes Reviewer2
------------	(
------------	  ITEM_ID BIGINT,
------------	  ATTRIBUTE_ID BIGINT,
------------	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
------------	)

------------DECLARE @T3 TABLE
------------	(
------------	  ITEM_ID BIGINT,
------------	  ATTRIBUTE_ID BIGINT,
------------	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
------------	)
------------DECLARE @T3c TABLE --current attributes Reviewer3
------------	(
------------	  ITEM_ID BIGINT,
------------	  ATTRIBUTE_ID BIGINT,
------------	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
------------	)
------------DECLARE @T1a2 table (ITEM_ID bigint primary key)--snapshot agreements 1 v 2
------------DECLARE @T1ca2 table (ITEM_ID bigint primary key)--current agreements 1 v 2
------------DECLARE @T1a3 table (ITEM_ID bigint primary key)--snapshot agreements 1 v 3
------------DECLARE @T1ca3 table (ITEM_ID bigint primary key)--current agreements 1 v 3
------------DECLARE @T2a3 table (ITEM_ID bigint primary key)--snapshot agreements 2 v 3
------------DECLARE @T2ca3 table (ITEM_ID bigint primary key)--current agreements 2 v 3
------------insert into @T1
------------select ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
------------inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID and c.CONTACT_ID1 = cia.CONTACT_ID and c.COMPARISON_ID = @COMPARISON_ID
------------set @n1 = ( select count(distinct(item_id)) from @T1 )

------------insert into @T1c 
------------select t1.ITEM_ID, tia.ATTRIBUTE_ID from (select distinct ITEM_ID from @T1) t1 --only items in the comparison
------------	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @set and tis.CONTACT_ID = @c1
------------	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID

------------insert into @T2
------------select ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
------------inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID and c.CONTACT_ID2 = cia.CONTACT_ID and c.COMPARISON_ID = @COMPARISON_ID
------------set @n2 = ( select count(distinct(item_id)) from @T2 )

------------insert into @T2c 
------------select t2.ITEM_ID, tia.ATTRIBUTE_ID from (select distinct ITEM_ID from @T2) t2 
------------	inner join TB_ITEM_SET tis on t2.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @set and tis.CONTACT_ID = @c2
------------	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID

------------insert into @T3
------------select ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
------------inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID and c.CONTACT_ID3 = cia.CONTACT_ID and c.COMPARISON_ID = @COMPARISON_ID
------------set @n3 = ( select count(distinct(item_id)) from @T3 )

------------insert into @T3c 
------------select t3.ITEM_ID, tia.ATTRIBUTE_ID from (select distinct ITEM_ID from @T3) t3 
------------	inner join TB_ITEM_SET tis on t3.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @set and tis.CONTACT_ID = @c3
------------	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID


-------------- Total N items coded reviewer 1
------------select @n1 as 'Total N items coded reviewer 1'
-------------- Total N items coded reviewer 2
------------select @n2 as 'Total N items coded reviewer 2'
-------------- Total N items coded reviewer 3
------------select @n3 as 'Total N items coded reviewer 3'

-------------- Total N items coded reviewer 1 & 2
------------select count(distinct(t1.item_id)) as 'Total N items coded reviewer 1 & 2' from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID

-------------- Total disagreements 1vs2
--------------the inner join (IJ) selects only records in t2 where the second reviewer has applied a different code, but this does not guarantee coding as a whole is different, we still need to check if:
--------------a)	R1 has also coded with the same attribute found in t2 through IJ. If that’s the case, then we should not count this as a disagreement.
--------------b)	R2 has also coded with the attribute from t1
--------------The second outer join (OJ2), with the “where t2b.ATTRIBUTE_ID is null” clause checks for a). The first outer join (OJ1), with the “where t1b.ATTRIBUTE_ID is null” clause, checks  for b).
--------------So overall, the first join spots all possible 1:1 coding differences, the two outer joins get rid of meaningless lines (where the differences are cancelled by other records). 
------------select count(distinct(t1.item_id)) as 'Total disagreements 1vs2'
------------	from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
------------	left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
------------	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
------------	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

-------------- Total N items coded reviewer 2 & 3
------------select count(distinct(t2.item_id)) as 'Total N items coded reviewer 2 & 3' from @T2 t2 inner join @T3 t3 on t2.ITEM_ID = t3.ITEM_ID

-------------- Total disagreements 2vs3
------------select count(distinct(t2.item_id)) as 'Total disagreements 2vs3'
------------	from @T2 t2 inner join @T3 t3 on t2.ITEM_ID = t3.ITEM_ID and t2.ATTRIBUTE_ID != t3.ATTRIBUTE_ID
------------	left outer join @T3 t3b on t2.ATTRIBUTE_ID = t3b.ATTRIBUTE_ID and t2.ITEM_ID = t3b.ITEM_ID
------------	left outer join @T2 t2b on  t2b.ATTRIBUTE_ID = t3.ATTRIBUTE_ID and t2b.ITEM_ID = t3.ITEM_ID
------------	where t2b.ATTRIBUTE_ID is null or t3b.ATTRIBUTE_ID is null

-------------- Total N items coded reviewer 1 & 3
------------select count(distinct(t1.item_id)) as 'Total N items coded reviewer 1 & 3' from @T1 t1 inner join @T3 t3 on t1.ITEM_ID = t3.ITEM_ID

-------------- Total disagreements 1vs3
------------select count(distinct(t1.item_id)) as 'Total disagreements 1vs3'
------------	from @T1 t1 inner join @T3 t3 on t1.ITEM_ID = t3.ITEM_ID and t1.ATTRIBUTE_ID != t3.ATTRIBUTE_ID
------------	left outer join @T3 t3b on t1.ATTRIBUTE_ID = t3b.ATTRIBUTE_ID and t1.ITEM_ID = t3b.ITEM_ID
------------	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t3.ATTRIBUTE_ID and t1b.ITEM_ID = t3.ITEM_ID
------------	where t1b.ATTRIBUTE_ID is null or t3b.ATTRIBUTE_ID is null

--------------REAL AGREEMENTS: Combine items from R1 and R2 and get only those that are not currenlty disagreements
------------insert into @T1ca2
------------Select t1.item_id from @T1c t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
------------	inner join @T2c t2 on t1.ITEM_ID = t2.ITEM_ID
------------	except
------------	select distinct(t1.item_id) from @T1c t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
------------											-- then the double outer joins as before
------------	inner join @T2c t2 on t1.ITEM_ID = t2.ITEM_ID
------------				and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
------------	left outer join @T1c tia1a on tia1a.ITEM_ID = t1.ITEM_ID and t2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
------------	left outer join @T2c tia2a on tia2a.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
------------	where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null


--------------insert into @T1ca2
--------------Select t1.item_id from @T1 t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
--------------	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.CONTACT_ID = @c1 and tis.SET_ID = @set
--------------	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID --need to join all the way to TB_ITEM_ATTRIBUTE to be 100% that some codes are present
--------------	inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID
--------------	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c2 and tis2.SET_ID = @set
--------------	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID	
--------------	except
------------	--select * from @T1 t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
------------	--										-- then the double outer joins as before
------------	--inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.CONTACT_ID = @c1 and tis1.SET_ID = @set
------------	--inner join TB_ITEM_ATTRIBUTE tia1 on tis1.ITEM_SET_ID = tia1.ITEM_SET_ID 
------------	--inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID 
------------	--inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c2 and tis2.SET_ID = @set
------------	--inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID 
------------	--			and tia1.ATTRIBUTE_ID != tia2.ATTRIBUTE_ID
------------	--left outer join TB_ITEM_ATTRIBUTE tia1a on tis1.ITEM_SET_ID = tia1a.ITEM_SET_ID and tia1a.ITEM_ID = tis1.ITEM_ID and tia2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
------------	--left outer join TB_ITEM_ATTRIBUTE tia2a on tis2.ITEM_SET_ID = tia2a.ITEM_SET_ID and tia2a.ITEM_ID = tis2.ITEM_ID and tia1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
------------	--where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null

--------------COMPARISON AGREEMENTS: 1 V 2, uses the same techniques as before to find the list of all agreed-items according to the comparison snapshot
------------insert into @T1a2
------------Select distinct t1.ITEM_ID from @T1 t1 
------------	inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID
------------	except
------------select distinct(t1.item_id) from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
------------	left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
------------	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
------------	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

--------------REAL AGREEMENTS: Combine items from R1 and R3 and get only those that are not currenlty disagreements
------------insert into @T1ca3
------------Select t1.item_id from @T1c t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
------------	inner join @T3c t3 on t1.ITEM_ID = t3.ITEM_ID
------------	except
------------	select distinct(t1.item_id) from @T1c t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
------------											-- then the double outer joins as before
------------	inner join @T3c t3 on t1.ITEM_ID = t3.ITEM_ID
------------				and t1.ATTRIBUTE_ID != t3.ATTRIBUTE_ID
------------	left outer join @T1c tia1a on tia1a.ITEM_ID = t1.ITEM_ID and t3.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
------------	left outer join @T3c tia3a on tia3a.ITEM_ID = t3.ITEM_ID and t1.ATTRIBUTE_ID = tia3a.ATTRIBUTE_ID
------------	where tia1a.ATTRIBUTE_ID is null or tia3a.ATTRIBUTE_ID is null


----------------REAL AGREEMENTS: Combine items from R1 and R3 and get only those that are not currenlty disagreements
--------------insert into @T1ca3
--------------Select t1.item_id from @T1 t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
--------------	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.CONTACT_ID = @c1 and tis.SET_ID = @set
--------------	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID --need to join all the way to TB_ITEM_ATTRIBUTE to be 100% that some codes are present
--------------	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
--------------	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--------------	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID	
--------------	except
--------------	select distinct(t1.item_id) from @T1 t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
--------------											-- then the double outer joins as before
--------------	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.CONTACT_ID = @c1 and tis1.SET_ID = @set
--------------	inner join TB_ITEM_ATTRIBUTE tia1 on tis1.ITEM_SET_ID = tia1.ITEM_SET_ID 
--------------	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID 
--------------	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--------------	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID 
--------------				and tia1.ATTRIBUTE_ID != tia2.ATTRIBUTE_ID
--------------	left outer join TB_ITEM_ATTRIBUTE tia1a on tis1.ITEM_SET_ID = tia1a.ITEM_SET_ID and tia2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
--------------	left outer join TB_ITEM_ATTRIBUTE tia2a on tis2.ITEM_SET_ID = tia2a.ITEM_SET_ID and tia1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
--------------	where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null

--------------COMPARISON AGREEMENTS: 1 V 3, uses the same techniques as before to find the list of all agreed-items according to the comparison snapshot
------------insert into @T1a3
------------Select distinct t1.ITEM_ID from @T1 t1 
------------	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
------------	except
------------select distinct(t1.item_id) from @T1 t1 inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
------------	left outer join @T3 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
------------	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
------------	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

--------------REAL AGREEMENTS: Combine items from R2 and R3 and get only those that are not currenlty disagreements
------------insert into @T2ca3
------------Select t2.item_id from @T2c t2 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
------------	inner join @T3c t3 on t2.ITEM_ID = t3.ITEM_ID
------------	except
------------	select distinct(t2.item_id) from @T2c t2 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
------------											-- then the double outer joins as before
------------	inner join @T3c t3 on t2.ITEM_ID = t3.ITEM_ID
------------				and t2.ATTRIBUTE_ID != t3.ATTRIBUTE_ID
------------	left outer join @T2c tia2a on tia2a.ITEM_ID = t2.ITEM_ID and t3.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
------------	left outer join @T3c tia3a on tia3a.ITEM_ID = t3.ITEM_ID and t2.ATTRIBUTE_ID = tia3a.ATTRIBUTE_ID
------------	where tia2a.ATTRIBUTE_ID is null or tia3a.ATTRIBUTE_ID is null
	
----------------REAL AGREEMENTS: Combine items from R2 and R3 and get only those that are not currenlty disagreements
--------------insert into @T2ca3
--------------Select t1.item_id from @T2 t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
--------------	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.CONTACT_ID = @c2 and tis.SET_ID = @set
--------------	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID --need to join all the way to TB_ITEM_ATTRIBUTE to be 100% that some codes are present
--------------	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
--------------	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--------------	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID	
--------------	except
--------------	select distinct(t1.item_id) from @T2 t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
--------------											-- then the double outer joins as before
--------------	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.CONTACT_ID = @c2 and tis1.SET_ID = @set
--------------	inner join TB_ITEM_ATTRIBUTE tia1 on tis1.ITEM_SET_ID = tia1.ITEM_SET_ID 
--------------	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID 
--------------	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--------------	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID 
--------------				and tia1.ATTRIBUTE_ID != tia2.ATTRIBUTE_ID
--------------	left outer join TB_ITEM_ATTRIBUTE tia1a on tis1.ITEM_SET_ID = tia1a.ITEM_SET_ID and tia2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
--------------	left outer join TB_ITEM_ATTRIBUTE tia2a on tis2.ITEM_SET_ID = tia2a.ITEM_SET_ID and tia1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
--------------	where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null

--------------COMPARISON AGREEMENTS: 2 V 3, uses the same techniques as before to find the list of all agreed-items according to the comparison snapshot
------------insert into @T2a3
------------Select distinct t1.ITEM_ID from @T2 t1 
------------	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
------------	except
------------select distinct(t1.item_id) from @T2 t1 inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
------------	left outer join @T3 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
------------	left outer join @T2 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
------------	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null


-------------- Are all Comparison Agreements currently completed OR are current agreements different from the snapshot agreements?
-------------- 1 V 2
------------Select Case when (COUNT(distinct ITEM_ID) = SUM(Completed)
------------				  OR 
------------					( select 
------------							Case when (SUM(sm.ss) > 0) then 1 --
------------							else 0
------------							end
------------						 from
------------						(
------------						Select COUNT(t1.ITEM_ID) ss from @T1a2 t1 
------------							left join @T1ca2 t2 on t1.ITEM_ID = t2.ITEM_ID
------------							where t2.ITEM_ID is null
------------						UNION
------------						Select COUNT(t1.ITEM_ID) ss from @T1ca2 t1 
------------							left join @T1a2 t2 on t1.ITEM_ID = t2.ITEM_ID
------------							where t2.ITEM_ID is null
------------						) AS sm
------------					) = 1
------------				  ) then 1 else 0 end 
------------				  as '1v2 lock-completion OR changed'
------------	from 
------------	(Select distinct t1.ITEM_ID, Case
------------								when (tis1.IS_COMPLETED = 1 ) then 1
------------								else 0
------------							end as Completed
------------	from @T1a2 t1 
------------	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.SET_ID = @set --joining on item and set, not on CONTACT_ID as item could be completed by anyone
------------	) a

-------------- Are all Comparison Agreements currently completed OR are current agreements different from the snapshot agreements?
-------------- 1 V 3
------------Select Case when (COUNT(distinct ITEM_ID) = SUM(Completed)
------------				  OR 
------------					( select 
------------							Case when (SUM(sm.ss) > 0) then 1 --
------------							else 0
------------							end
------------						 from
------------						(
------------						Select COUNT(t1.ITEM_ID) ss from @T1a3 t1 
------------							left join @T1ca3 t2 on t1.ITEM_ID = t2.ITEM_ID
------------							where t2.ITEM_ID is null
------------						UNION
------------						Select COUNT(t1.ITEM_ID) ss from @T1ca3 t1 
------------							left join @T1a3 t2 on t1.ITEM_ID = t2.ITEM_ID
------------							where t2.ITEM_ID is null
------------						) AS sm
------------					) = 1
------------				  ) then 1 else 0 end 
------------				  as '1v3 lock-completion OR changed'
------------	from 
------------	(Select distinct t1.ITEM_ID, Case
------------								when (tis1.IS_COMPLETED = 1 ) then 1
------------								else 0
------------							end as Completed
------------	from @T1a3 t1 
------------	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.SET_ID = @set --joining on item and set, not on CONTACT_ID as item could be completed by anyone
------------	) a

-------------- Are all Comparison Agreements currently completed OR are current agreements different from the snapshot agreements?
-------------- 2 V 3
------------Select Case when (COUNT(distinct ITEM_ID) = SUM(Completed)
------------				  OR 
------------					( select 
------------							Case when (SUM(sm.ss) > 0) then 1 --
------------							else 0
------------							end
------------						 from
------------						(
------------						Select COUNT(t1.ITEM_ID) ss from @T2a3 t1 
------------							left join @T2ca3 t2 on t1.ITEM_ID = t2.ITEM_ID
------------							where t2.ITEM_ID is null
------------						UNION
------------						Select COUNT(t1.ITEM_ID) ss from @T2ca3 t1 
------------							left join @T2a3 t2 on t1.ITEM_ID = t2.ITEM_ID
------------							where t2.ITEM_ID is null
------------						) AS sm
------------					) = 1
------------				  ) then 1 else 0 end 
------------				  as '2v3 lock-completion OR changed'
------------	from 
------------	(Select distinct t1.ITEM_ID, Case
------------								when (tis1.IS_COMPLETED = 1 ) then 1
------------								else 0
------------							end as Completed
------------	from @T2a3 t1 
------------	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.SET_ID = @set --joining on item and set, not on CONTACT_ID as item could be completed by anyone
------------	) a

------------GO
