USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SearchDelete]    Script Date: 17/09/2019 09:46:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_SearchDelete]
(
	@SEARCHES NVARCHAR(MAX),
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

declare @TEMP table (SEARCH_ID int) 
insert into @TEMP SELECT SEARCH_ID FROM TB_SEARCH
INNER JOIN fn_split_int(@SEARCHES, ',') SearchList on SearchList.value = TB_SEARCH.SEARCH_ID
WHERE REVIEW_ID = @REVIEW_ID;

--Make a check here on the temp table 
if ( (SELECT COUNT(*) FROM @TEMP) != 
(SELECT COUNT(*)-1 FROM fn_split_int(@SEARCHES, ',') ) ) return


DELETE FROM TB_SEARCH_ITEM
	FROM TB_SEARCH_ITEM INNER JOIN fn_split_int(@SEARCHES, ',') SearchList on SearchList.value = TB_SEARCH_ITEM.SEARCH_ID
		
DELETE FROM TB_SEARCH
	FROM TB_SEARCH INNER JOIN fn_split_int(@SEARCHES, ',') SearchList on SearchList.value = TB_SEARCH.SEARCH_ID

SET NOCOUNT OFF
GO



USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonComplete]    Script Date: 17/09/2019 10:58:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ComparisonComplete]
(
	@COMPARISON_ID INT,
	@CONTACT_ID INT,
	@WHICH_REVIEWERS NVARCHAR(20),
	@IS_LOCKED bit = NULL,
	@REVIEW_ID INT
	--@RECORDS_AFFECTED INT OUTPUT
)

As

--SET NOCOUNT ON

declare @check int = 0
set @check = (select count(COMPARISON_ID) from 
tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID and REVIEW_ID = @REVIEW_ID)

if (@check != 1) return



DECLARE @T1 TABLE --item_attribute for R1, R1 and R2 are relative to the sproc, could be any couple from 
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)
DECLARE @T2 TABLE --item_attribute for R2
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)
DECLARE @T1a2 table (ITEM_ID bigint primary key)--snapshot agreements 1 v 2
--declare @c1 int
--declare @c2 int


--If @WHICH_REVIEWERS = 'Complete1vs2'
--	BEGIN
--		select @c1 = CONTACT_ID1, @c2 = CONTACT_ID2 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END
--ELSE If @WHICH_REVIEWERS = 'Complete1vs3'
--	BEGIN
--		select @c1 = CONTACT_ID1, @c2 = CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END
--ELSE  --Complete2vs3
--	BEGIN
--		select @c1 = CONTACT_ID2, @c2 = CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END

insert into @T1
select distinct ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
	inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID 
												and  cia.CONTACT_ID = 
													CASE  @WHICH_REVIEWERS
														WHEN 'Complete2vs3' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID1
													END 
												and c.COMPARISON_ID = @COMPARISON_ID

insert into @T2
select distinct ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID 
												and cia.CONTACT_ID = 
													CASE  @WHICH_REVIEWERS
														WHEN 'Complete1vs2' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID3
													END 
												and c.COMPARISON_ID = @COMPARISON_ID



insert into @T1a2 --add all agreements; see st_ComparisonStats to understand how this works
Select distinct t1.ITEM_ID from @T1 t1 
	inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID
	except
select distinct(t1.item_id) from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
	left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

--select * from @T1
--select * from @T2
--select * from @T1a2

Delete from @T1a2 where ITEM_ID in --remove all items that are already completed
(
	select tis.ITEM_ID from TB_ITEM_SET tis inner join 
		tb_COMPARISON c on tis.SET_ID = c.SET_ID and COMPARISON_ID = @COMPARISON_ID
		Inner join @T1a2 t1 on tis.ITEM_ID = t1.ITEM_ID and tis.IS_COMPLETED = 1
)

--select * from @T1a2

IF @IS_LOCKED is null
BEGIN
	Update TB_ITEM_SET set IS_COMPLETED = 1
		where CONTACT_ID = @CONTACT_ID
			and ITEM_SET_ID in
				(	
					select ITEM_SET_ID from TB_ITEM_SET tis 
					inner join tb_COMPARISON c on tis.SET_ID = c.SET_ID and c.COMPARISON_ID = @COMPARISON_ID
					inner join @T1a2 t on tis.ITEM_ID = t.ITEM_ID
				)
END
ELSE
BEGIN
	Update TB_ITEM_SET set IS_COMPLETED = 1 , IS_LOCKED = @IS_LOCKED
		where CONTACT_ID = @CONTACT_ID
			and ITEM_SET_ID in
				(	
					select ITEM_SET_ID from TB_ITEM_SET tis 
					inner join tb_COMPARISON c on tis.SET_ID = c.SET_ID and c.COMPARISON_ID = @COMPARISON_ID
					inner join @T1a2 t on tis.ITEM_ID = t.ITEM_ID
				)
END








--INSERT INTO @TT(ITEM_ID, ATTRIBUTE_ID)
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--			CASE  @WHICH_REVIEWERS
--				WHEN 'Complete2vs3' THEN tb_COMPARISON.CONTACT_ID2
--				ELSE tb_COMPARISON.CONTACT_ID1
--			END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	INTERSECT --  ********** AGREEMENT - THEREFORE INTERSECT
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--		CASE @WHICH_REVIEWERS
--			WHEN 'Complete1vs2' THEN TB_COMPARISON.CONTACT_ID2
--			ELSE tb_COMPARISON.CONTACT_ID3
--		END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	ORDER BY ITEM_ID, ATTRIBUTE_ID

--UPDATE TB_ITEM_SET
--SET IS_COMPLETED = 'TRUE' WHERE TB_ITEM_SET.CONTACT_ID = @CONTACT_ID AND TB_ITEM_SET.ITEM_SET_ID IN
--(
--	SELECT ITEM_SET_ID FROM TB_ITEM_SET TB_IS
--	INNER JOIN tb_COMPARISON_ITEM_ATTRIBUTE ON
--		tb_COMPARISON_ITEM_ATTRIBUTE.SET_ID = TB_IS.SET_ID AND
--		tb_COMPARISON_ITEM_ATTRIBUTE.ITEM_ID = TB_IS.ITEM_ID AND
--		tb_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID = TB_IS.CONTACT_ID AND
--		tb_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID = @COMPARISON_ID
--	WHERE TB_IS.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
--)


SELECT @@ROWCOUNT
GO



USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_Source_Update]    Script Date: 17/09/2019 13:56:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[st_Source_Update] 
	-- Add the parameters for the stored procedure here
	@s_ID int = 0, 
	@sDB nvarchar(200),
	@Name nvarchar(255),
	@DoS date,
	@DoI date,
	@Descr nvarchar(4000),
	@s_Str nvarchar(MAX),
	@Notes nvarchar(4000),
	@REVIEW_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

declare @check int = 0
--make sure source belongs to review...
set @check = (select count(SOURCE_ID) from 
TB_SOURCE where SOURCE_ID = @s_ID and REVIEW_ID = @REVIEW_ID)
if(@check != 1) return

    -- Insert statements for procedure here
	UPDATE TB_SOURCE
	   SET [SOURCE_NAME] = @Name
		  ,[DATE_OF_SEARCH] = @DoS
		  ,[DATE_OF_IMPORT] = @DoI
		  ,[SOURCE_DATABASE] = @sDB
		  ,[SEARCH_DESCRIPTION] = @Descr
		  ,[SEARCH_STRING] = @s_Str
		  ,[NOTES] = @Notes
      
 WHERE SOURCE_ID = @s_ID
END
GO



USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_SearchUpdate]    Script Date: 17/09/2019 14:40:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_SearchUpdate]
(
	@SEARCH_ID INT,
	@SEARCH_TITLE NVARCHAR(4000),
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

declare @check int = 0
set @check = (select count(SEARCH_ID) from 
TB_SEARCH where SEARCH_ID = @SEARCH_ID and REVIEW_ID = @REVIEW_ID)
if(@check != 1) return


UPDATE TB_SEARCH
		SET SEARCH_TITLE = @SEARCH_TITLE
		WHERE SEARCH_ID = @SEARCH_ID

SET NOCOUNT OFF
GO



USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetUpdate]    Script Date: 17/09/2019 15:39:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ReviewSetUpdate]
(
	@REVIEW_SET_ID INT,
	@SET_ID INT,
	@ALLOW_CODING_EDITS BIT,
	@CODING_IS_FINAL BIT,
	@SET_NAME NVARCHAR(255),
	@SET_ORDER INT,
	@SET_DESCRIPTION nvarchar(2000),
	@ITEM_SET_ID BIGINT = NULL,
	@IS_COMPLETED BIT = NULL,
	@IS_LOCKED BIT = NULL,
	@REVIEW_ID INT
)

As

SET NOCOUNT ON


declare @check int = 0
set @check = (select count(REVIEW_SET_ID) from 
TB_REVIEW_SET where REVIEW_SET_ID = @REVIEW_SET_ID and REVIEW_ID = @REVIEW_ID)
if(@check != 1) return

UPDATE TB_SET SET SET_NAME = @SET_NAME, SET_DESCRIPTION = @SET_DESCRIPTION 
	WHERE SET_ID = @SET_ID
UPDATE TB_REVIEW_SET SET ALLOW_CODING_EDITS = @ALLOW_CODING_EDITS,
	CODING_IS_FINAL = @CODING_IS_FINAL,
	SET_ORDER = @SET_ORDER
WHERE REVIEW_SET_ID = @REVIEW_SET_ID
	
IF (@ITEM_SET_ID > 0)
BEGIN
	UPDATE TB_ITEM_SET
	SET IS_COMPLETED = @IS_COMPLETED, IS_LOCKED = @IS_LOCKED
	WHERE ITEM_SET_ID = @ITEM_SET_ID
END

SET NOCOUNT OFF
GO



USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemTimepointUpdate]    Script Date: 17/09/2019 15:51:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemTimepointUpdate]
(
	@ITEM_TIMEPOINT_ID BIGINT
,	@TIMEPOINT_VALUE FLOAT
,	@TIMEPOINT_METRIC VARCHAR(50)
,   @REVIEW_ID INT
)

As

SET NOCOUNT ON


declare @check int = 0

declare @itemID int = (select ITEM_ID from 
TB_ITEM_TIMEPOINT where ITEM_TIMEPOINT_ID = @ITEM_TIMEPOINT_ID) 

set @check = (select count(*) from 
TB_ITEM_REVIEW where ITEM_ID = @itemID AND REVIEW_ID = @REVIEW_ID)

if(@check != 1) return


	UPDATE TB_ITEM_TIMEPOINT
		SET TIMEPOINT_VALUE = @TIMEPOINT_VALUE,
			TIMEPOINT_METRIC = @TIMEPOINT_METRIC
		WHERE ITEM_TIMEPOINT_ID = @ITEM_TIMEPOINT_ID

SET NOCOUNT OFF
GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentUpdate]    
Script Date: 17/09/2019 15:57:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemDocumentUpdate]
(
	@ITEM_DOCUMENT_ID BIGINT,
	@DOCUMENT_TITLE NVARCHAR(255),
	@DOCUMENT_FREE_NOTES NVARCHAR(MAX) = '',
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

declare @check int = 0

declare @itemID int = (select ITEM_ID from 
TB_ITEM_DOCUMENT where ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID) 

set @check = (select count(*) from 
TB_ITEM_REVIEW where ITEM_ID = @itemID AND REVIEW_ID = @REVIEW_ID)

if(@check != 1) return

	UPDATE TB_ITEM_DOCUMENT
	SET DOCUMENT_TITLE = @DOCUMENT_TITLE,
	DOCUMENT_FREE_NOTES = @DOCUMENT_FREE_NOTES
	WHERE ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID

SET NOCOUNT OFF
GO



USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributePDFUpdate]    Script Date: 17/09/2019 16:00:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[st_ItemAttributePDFUpdate]
	-- Add the parameters for the stored procedure here
	@ITEM_ATTRIBUTE_PDF_ID bigint
	,@SHAPE_TEXT varchar(max)
	,@INTERVALS varchar(max)
	,@TEXTS nvarchar(max)
	,@REVIEW_ID INT
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


declare @check int = 0

declare @itemID int = (select IA.ITEM_ID from 
TB_ITEM_ATTRIBUTE IA INNER JOIN 
TB_ITEM_ATTRIBUTE_PDF IAP ON IA.ITEM_ATTRIBUTE_ID = IAP.ITEM_ATTRIBUTE_ID
where ITEM_ATTRIBUTE_PDF_ID = @ITEM_ATTRIBUTE_PDF_ID) 

set @check = (select count(*) from 
TB_ITEM_REVIEW where ITEM_ID = @itemID AND REVIEW_ID = @REVIEW_ID)

if(@check != 1) return

-- Insert statements for procedure here
UPDATE TB_ITEM_ATTRIBUTE_PDF
SET SHAPE_TEXT = @SHAPE_TEXT
    ,SELECTION_INTERVALS = @INTERVALS
    ,SELECTION_TEXTS = @TEXTS
WHERE ITEM_ATTRIBUTE_PDF_ID = @ITEM_ATTRIBUTE_PDF_ID


END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemArmUpdate]    Script Date: 17/09/2019 16:10:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemArmUpdate]
(
	@ITEM_ARM_ID BIGINT
,	@ORDERING INT
,	@ARM_NAME NVARCHAR(500)
,	 @REVIEW_ID INT
)

As

SET NOCOUNT ON

declare @check int = 0

declare @itemID int = (select ITEM_ID from 
TB_ITEM_ARM where ITEM_ARM_ID = @ITEM_ARM_ID) 

set @check = (select count(*) from 
TB_ITEM_REVIEW where ITEM_ID = @itemID AND REVIEW_ID = @REVIEW_ID)

if(@check != 1) return

UPDATE TB_ITEM_ARM
	SET ORDERING = @ORDERING,
		ARM_NAME = @ARM_NAME
	WHERE ITEM_ARM_ID = @ITEM_ARM_ID

SET NOCOUNT OFF

GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_AttributeSetLimitedUpdate]    Script Date: 17/09/2019 16:14:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_AttributeSetLimitedUpdate]
(
	@ATTRIBUTE_ID BIGINT,
	@ATTRIBUTE_SET_ID BIGINT,
	@ATTRIBUTE_TYPE_ID INT,
	@ATTRIBUTE_NAME NVARCHAR(255),
	@ATTRIBUTE_DESCRIPTION NVARCHAR(MAX),
	@ATTRIBUTE_ORDER INT,
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

declare @check int = 0

declare @itemID int = (select IA.ITEM_ID from 
TB_ITEM_ATTRIBUTE IA INNER JOIN 
TB_ATTRIBUTE A ON IA.ATTRIBUTE_ID = A.ATTRIBUTE_ID
where A.ATTRIBUTE_ID = @ATTRIBUTE_ID) 

set @check = (select count(*) from 
TB_ITEM_REVIEW where ITEM_ID = @itemID AND REVIEW_ID = @REVIEW_ID)

if(@check != 1) return


	UPDATE TB_ATTRIBUTE
		SET ATTRIBUTE_NAME = @ATTRIBUTE_NAME
		WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
		
	UPDATE TB_ATTRIBUTE_SET
		SET ATTRIBUTE_SET_DESC = @ATTRIBUTE_DESCRIPTION,
		 ATTRIBUTE_TYPE_ID = @ATTRIBUTE_TYPE_ID,
		 ATTRIBUTE_ORDER = @ATTRIBUTE_ORDER
		WHERE ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID

SET NOCOUNT OFF

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_AttributeSetUpdate]    Script Date: 17/09/2019 16:18:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_AttributeSetUpdate]
(
	@ATTRIBUTE_SET_ID BIGINT,
	@SET_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@PARENT_ATTRIBUTE_ID BIGINT,
	@ATTRIBUTE_TYPE_ID INT,
	@ATTRIBUTE_SET_DESC NVARCHAR(MAX),
	@ATTRIBUTE_ORDER INT,
	@ATTRIBUTE_NAME NVARCHAR(255),
	@ATTRIBUTE_DESC NVARCHAR(2000),
	@CONTACT_ID INT -- not used yet - maybe for authorisation,
	,@REVIEW_ID INT
)

As

SET NOCOUNT ON

declare @check int = 0

declare @itemID int = (select IA.ITEM_ID from 
TB_ITEM_ATTRIBUTE IA INNER JOIN 
TB_ATTRIBUTE A ON IA.ATTRIBUTE_ID = A.ATTRIBUTE_ID
where A.ATTRIBUTE_ID = @ATTRIBUTE_ID) 

set @check = (select count(*) from 
TB_ITEM_REVIEW where ITEM_ID = @itemID AND REVIEW_ID = @REVIEW_ID)

if(@check != 1) return


	UPDATE TB_ATTRIBUTE
		SET ATTRIBUTE_NAME = @ATTRIBUTE_NAME, ATTRIBUTE_DESC = @ATTRIBUTE_DESC
		WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID

	UPDATE TB_ATTRIBUTE_SET
		SET SET_ID = @SET_ID, PARENT_ATTRIBUTE_ID = @PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID = @ATTRIBUTE_TYPE_ID,
			ATTRIBUTE_SET_DESC = @ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER = @ATTRIBUTE_ORDER
		WHERE ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID


SET NOCOUNT OFF

GO












