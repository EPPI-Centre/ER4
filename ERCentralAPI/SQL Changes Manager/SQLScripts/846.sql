USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateVisualise]    Script Date: 13/11/2026 13:40:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   procedure [dbo].[st_MagAutoUpdateVisualise]
(
	@MAG_AUTO_UPDATE_RUN_ID INT,
	@FIELD NVARCHAR(10)
)

As

SET NOCOUNT ON

	DECLARE @VALS TABLE
(
	VAL float
,	TITLE NVARCHAR(10)
)

IF @FIELD = 'AutoUpdate'
begin
INSERT INTO @VALS (VAL, TITLE)
--VALUES (0.4, '>=0.4'), (0.5, '0.5-'), (0.6, '0.6-'), (0.7, '0.7-'), (0.8, '0.8-'), (0.9, '>=0.9')
VALUES (0.9, '0.9'), (0.91, '0.91'), (0.92, '0.92'), (0.93, '0.93'), (0.94, '0.94'),
	(0.95, '0.95'),(0.96, '0.96'),(0.97, '0.97'),(0.98, '0.98'),(0.99, '0.99')
SELECT VAL, TITLE,
	(SELECT COUNT(*) FROM TB_MAG_AUTO_UPDATE_RUN_PAPER WHERE 
	ContReviewScore >= VAL AND ContReviewScore < VAL + 0.1 AND MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID) AS NUM
FROM @VALS
end

IF @FIELD = 'StudyType'
begin
INSERT INTO @VALS (VAL, TITLE)
VALUES (0, '0.0'), (0.1, '0.1'), (0.2, '0.2'), (0.3, '0.3'), (0.4, '0.4'), (0.5, '0.5'), (0.6, '0.6'), (0.7, '0.7'), (0.8, '0.8'), (0.9, '0.9')

SELECT VAL, TITLE,
	(SELECT COUNT(*) FROM TB_MAG_AUTO_UPDATE_RUN_PAPER WHERE 
	StudyTypeClassifierScore >= VAL AND StudyTypeClassifierScore < VAL + 0.1 AND MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID) AS NUM
FROM @VALS
end

IF @FIELD = 'User'
begin
INSERT INTO @VALS (VAL, TITLE)
VALUES (0, '0.0'), (0.1, '0.1'), (0.2, '0.2'), (0.3, '0.3'), (0.4, '0.4'), (0.5, '0.5'), (0.6, '0.6'), (0.7, '0.7'), (0.8, '0.8'), (0.9, '0.9')

SELECT VAL, TITLE,
	(SELECT COUNT(*) FROM TB_MAG_AUTO_UPDATE_RUN_PAPER WHERE 
	UserClassifierScore >= VAL AND UserClassifierScore < VAL + 0.1 AND MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID) AS NUM
FROM @VALS
end



SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierModels]    Script Date: 13/11/2026 17:36:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ClassifierModels]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	select MODEL_ID, MODEL_TITLE, REVIEW_ID, A1.ATTRIBUTE_NAME ATTRIBUTE_ON, A2.ATTRIBUTE_NAME ATTRIBUTE_NOT_ON,
		CONTACT_NAME, tb_CLASSIFIER_MODEL.CONTACT_ID, ACCURACY, AUC, [PRECISION], RECALL,
		ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, ML_MODEL_NAME
		from tb_CLASSIFIER_MODEL
	INNER JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_ON
	INNER JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_NOT_ON
	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = tb_CLASSIFIER_MODEL.CONTACT_ID
	
	where REVIEW_ID = @REVIEW_ID
	order by MODEL_ID

SET NOCOUNT OFF

GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierGet]    Script Date: 13/11/2026 19:27:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER    procedure [dbo].[st_ClassifierGet]
(
	@MODEL_ID INT,
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	SELECT MODEL_ID, MODEL_TITLE, CONTACT_ID, REVIEW_ID, ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON, ACCURACY, AUC,
		[PRECISION], RECALL, ML_MODEL_NAME FROM tb_CLASSIFIER_MODEL
		WHERE MODEL_ID = @MODEL_ID
		AND REVIEW_ID = @REVIEW_ID



SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierContactModels]    Script Date: 13/11/2026 19:33:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_ClassifierContactModels]
(
	@CONTACT_ID INT
)

As

SET NOCOUNT ON
declare @chk int = (select count(contact_id) from TB_CONTACT where CONTACT_ID = @CONTACT_ID and IS_SITE_ADMIN = 1)
if (@chk = 1) --user is a site admin, current review might not have it as a member, so we need to do more work
 BEGIN
  declare @RID int = (select Review_id from sTB_LOGON_TICKET 
						where [STATE] = 1 and CONTACT_ID = @CONTACT_ID
							and LAST_RENEWED > DATEADD(hour, -1, getdate()))
  if @RID is null return
  --select @rid
  declare @rids table (REVIEW_ID int)
  insert into @rids select @rid
  insert into @rids select REVIEW_ID from TB_REVIEW_CONTACT rc where CONTACT_ID = @CONTACT_ID and rc.REVIEW_ID != @RID
  select MODEL_ID, MODEL_TITLE, CM.REVIEW_ID, R.REVIEW_NAME, 
	A1.ATTRIBUTE_NAME ATTRIBUTE_ON, A2.ATTRIBUTE_NAME ATTRIBUTE_NOT_ON,
	 ATTRIBUTE_ID_NOT_ON, ATTRIBUTE_ID_ON, ML_MODEL_NAME,
		CONTACT_NAME, CM.CONTACT_ID, ACCURACY, AUC, [PRECISION], RECALL
		from tb_CLASSIFIER_MODEL CM
	INNER JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = CM.ATTRIBUTE_ID_ON
	INNER JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = CM.ATTRIBUTE_ID_NOT_ON
	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = CM.CONTACT_ID
	INNER JOIN @rids rid on rid.REVIEW_ID = cm.REVIEW_ID
	inner join TB_REVIEW r on rid.REVIEW_ID = r.REVIEW_ID 
	--where RC.CONTACT_ID = @CONTACT_ID
	order by CM.REVIEW_ID, cm.MODEL_ID
 END

 ELSE
 BEGIN
	select MODEL_ID, MODEL_TITLE, CM.REVIEW_ID, R.REVIEW_NAME, 
	A1.ATTRIBUTE_NAME ATTRIBUTE_ON, A2.ATTRIBUTE_NAME ATTRIBUTE_NOT_ON,
	 ATTRIBUTE_ID_NOT_ON, ATTRIBUTE_ID_ON, ML_MODEL_NAME,
		CONTACT_NAME, CM.CONTACT_ID, ACCURACY, AUC, [PRECISION], RECALL
		from tb_CLASSIFIER_MODEL CM
	INNER JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = CM.ATTRIBUTE_ID_ON
	INNER JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = CM.ATTRIBUTE_ID_NOT_ON
	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = CM.CONTACT_ID
	INNER JOIN TB_REVIEW_CONTACT RC ON RC.REVIEW_ID = CM.REVIEW_ID
	INNER JOIN TB_REVIEW R ON R.REVIEW_ID = RC.REVIEW_ID
	
	where RC.CONTACT_ID = @CONTACT_ID
	order by CM.REVIEW_ID, MODEL_ID
END
SET NOCOUNT OFF

GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_QuickCodingReportCodingData]    Script Date: 14/11/2026 19:03:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[st_QuickCodingReportCodingData] 
	-- Add the parameters for the stored procedure here
	(
		@revID int
		,@input ITEMS_INPUT_TB READONLY
		,@SetIds nvarchar(MAX)
	)
AS
BEGIN
	SET NOCOUNT ON



	Declare @Sets table (SetID int primary key)
	Insert into @Sets select [value] from dbo.fn_Split_int(@SetIds, ',') s
		 inner join TB_REVIEW_SET rs on s.value = rs.SET_ID and REVIEW_ID = @revID
	
	--FIRST reader: ordinary coding data
	SELECT ITEM_SET_ID, ir.ITEM_ID, tis.SET_ID, IS_COMPLETED, tis.CONTACT_ID, IS_LOCKED,
		CODING_IS_FINAL, SET_NAME, CONTACT_NAME, Ext_URL, Ext_Type
	FROM @input i
		Inner JOIN TB_ITEM_REVIEW ir on i.ItemId = ir.ITEM_ID and ir.REVIEW_ID = @revID
		INNER Join TB_ITEM_SET tis on i.ItemId = tis.ITEM_ID
		INNER JOIN @Sets ss on tis.SET_ID = ss.SetID
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = tis.SET_ID and TB_REVIEW_SET.REVIEW_ID = @revID
		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = tis.CONTACT_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = tis.SET_ID
	WHERE tis.IS_COMPLETED = 1

	--SECOND reader: PDF and Text coding data
	SELECT  tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, p.ITEM_ATTRIBUTE_PDF_ID as [ID]
			, 'Page ' + CONVERT
							(varchar(10),PAGE) 
							+ ':' + CHAR(10) + '[¬s]"' 
							+ replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' 
				as [TEXT] 
			, NULL as [TEXT_FROM], NULL as [TEXT_TO]
			, 1 as IS_FROM_PDF
			, CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END AS ARM_NAME
			, ia.ITEM_ARM_ID
		from @Sets ss
		inner join TB_REVIEW_SET rs on ss.SetID = rs.SET_ID
		inner join TB_ITEM_SET tis on tis.SET_ID = rs.SET_ID 
		inner join @input ii on tis.ITEM_ID = ii.ItemId
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and ii.ItemId = ia.ITEM_ID
		inner join TB_ITEM_ATTRIBUTE_PDF p on ia.ITEM_ATTRIBUTE_ID = p.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on p.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
		LEFT join TB_ITEM_ARM ON TB_ITEM_ARM.ITEM_ARM_ID = IA.ITEM_ARM_ID
	UNION
	SELECT tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, t.ITEM_ATTRIBUTE_TEXT_ID as [ID]
			, SUBSTRING(
					replace(id.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) 
				 as [TEXT]
			, TEXT_FROM, TEXT_TO 
			, 0 as IS_FROM_PDF
			, CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END AS ARM_NAME
			, ia.ITEM_ARM_ID
		from @Sets ss
		inner join TB_REVIEW_SET rs on ss.SetID = rs.SET_ID
		inner join TB_ITEM_SET tis on tis.SET_ID = rs.SET_ID 
		inner join @input ii on tis.ITEM_ID = ii.ItemId
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and ii.ItemId = ia.ITEM_ID
		inner join TB_ITEM_ATTRIBUTE_TEXT t on ia.ITEM_ATTRIBUTE_ID = t.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on t.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
		LEFT join TB_ITEM_ARM ON TB_ITEM_ARM.ITEM_ARM_ID = IA.ITEM_ARM_ID
	ORDER by IS_FROM_PDF, [TEXT]	
			
	SET nocount off
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSetDataListAll]    Script Date: 14/11/2026 19:01:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemSetDataListAll]
(
	@REVIEW_ID INT,
	--@CONTACT_ID INT,
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON

	/* Collects all the item sets in a review
	 */

	SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
		CODING_IS_FINAL, SET_NAME, CONTACT_NAME, Ext_URL, Ext_Type
	FROM TB_ITEM_SET
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
	WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
	
	ORDER BY SET_NAME

SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSetDataList]    Script Date: 14/11/2026 19:01:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER procedure [dbo].[st_ItemSetDataList] (
	@REVIEW_ID INT,
	--@CONTACT_ID INT,
	@ITEM_ID BIGINT
)

As

SET NOCOUNT ON
	--this was changed on Aug 2013, previous version is commented below.
	--the new version gets: all completed sets for the item, plus all coded text
	--the old version was called by ItemSetList and was grabbing what was needed by the current user in DialogCoding:
	--that's the completed sets, plus the incomplete ones that belong to the user when a completed version isn't present.
	
	--25/06/2020 THIS SP is "mirrored" in st_QuickCodingReportCodingData -- changes done here are likely to be necessary there as well...

	--first, grab the completed item set (if any)
	SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
		CODING_IS_FINAL, SET_NAME, CONTACT_NAME, Ext_URL, Ext_Type
	FROM TB_ITEM_SET
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
	WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
		--AND TB_REVIEW_SET.CODING_IS_FINAL = 'true'
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	
	--second, get all data from TB_ITEM_ATTRIBUTE_PDF and TB_ITEM_ATTRIBUTE_TEXT using union and only from completed sets
	SELECT  tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, p.ITEM_ATTRIBUTE_PDF_ID as [ID]
			, 'Page ' + CONVERT
							(varchar(10),PAGE) 
							+ ':' + CHAR(10) + '[¬s]"' 
							+ replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' 
				as [TEXT] 
			, NULL as [TEXT_FROM], NULL as [TEXT_TO]
			, 1 as IS_FROM_PDF
			, CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END AS ARM_NAME
			, ia.ITEM_ARM_ID
		from TB_REVIEW_SET rs
		inner join TB_ITEM_SET tis on rs.REVIEW_ID = @REVIEW_ID and tis.SET_ID = rs.SET_ID and tis.ITEM_ID = @ITEM_ID
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE_PDF p on ia.ITEM_ATTRIBUTE_ID = p.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on p.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
		LEFT join TB_ITEM_ARM ON TB_ITEM_ARM.ITEM_ARM_ID = IA.ITEM_ARM_ID
	UNION
	SELECT tis.ITEM_SET_ID, ia.ITEM_ATTRIBUTE_ID, id.ITEM_DOCUMENT_ID, id.DOCUMENT_TITLE, t.ITEM_ATTRIBUTE_TEXT_ID as [ID]
			, SUBSTRING(
					replace(id.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) 
				 as [TEXT]
			, TEXT_FROM, TEXT_TO 
			, 0 as IS_FROM_PDF
			, CASE WHEN ARM_NAME IS NULL THEN '' ELSE ARM_NAME END AS ARM_NAME
			, ia.ITEM_ARM_ID
		from TB_REVIEW_SET rs
		inner join TB_ITEM_SET tis on rs.REVIEW_ID = @REVIEW_ID and tis.SET_ID = rs.SET_ID and tis.ITEM_ID = @ITEM_ID
		inner join TB_ATTRIBUTE_SET tas on tis.SET_ID = tas.SET_ID and tis.IS_COMPLETED = 1 
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_SET_ID = tis.ITEM_SET_ID and ia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE_TEXT t on ia.ITEM_ATTRIBUTE_ID = t.ITEM_ATTRIBUTE_ID
		inner join TB_ITEM_DOCUMENT id on t.ITEM_DOCUMENT_ID = id.ITEM_DOCUMENT_ID
		LEFT join TB_ITEM_ARM ON TB_ITEM_ARM.ITEM_ARM_ID = IA.ITEM_ARM_ID
	ORDER by IS_FROM_PDF, [TEXT]	
	--old version starts here
	/* Collects just the item sets that are needed by a given reviewer - not all of them for every item
	   Critically, this query NOTs out the set_ids already identified.
	 */

	-- first, grab the completed item set (if any)
	--SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
	--	CODING_IS_FINAL, SET_NAME, CONTACT_NAME
	--FROM TB_ITEM_SET
	--	INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
	--	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
	--	INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
	--WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
	--	--AND TB_REVIEW_SET.CODING_IS_FINAL = 'true'
	--	AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	
	--UNION
	----second get incomplete item_sets for the current Reviewer if no complete set is present
	--	SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
	--		CODING_IS_FINAL, SET_NAME, CONTACT_NAME
	--	FROM TB_ITEM_SET
	--		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
	--		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
	--		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
	--	WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
	--		and tb_ITEM_SET.IS_COMPLETED = 'false'
	--		and TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
	--	AND NOT TB_ITEM_SET.SET_ID IN
	--	(
	--		SELECT TB_ITEM_SET.SET_ID FROM TB_ITEM_SET
	--			INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
	--			WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
	--			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	--	)
	--end of old version
SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSingleSetDataList]    Script Date: 14/11/2026 19:12:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER procedure [dbo].[st_ItemSingleSetDataList] (
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@ITEM_ID BIGINT,
	@SET_ID INT
)

As

SET NOCOUNT ON

	/* Collects just the item sets that are needed by a given reviewer - not all of them for every item
	   Critically, this query NOTs out the set_ids already identified.
	 */
	--the code below comes straight from st_ItemSetDataList with the added
	--AND TB_ITEM_SET.SET_ID = @SET_ID clauses so to get info only from one set
	
	-- first, grab the completed item set (if any)
	SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
		CODING_IS_FINAL, SET_NAME, CONTACT_NAME, Ext_URL, Ext_Type
	FROM TB_ITEM_SET
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
		INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
	WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
		--AND TB_REVIEW_SET.CODING_IS_FINAL = 'true'
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		AND TB_ITEM_SET.SET_ID = @SET_ID
	UNION
	--second get incomplete item_sets for the current Reviewer if no complete set is present
		SELECT ITEM_SET_ID, ITEM_ID, TB_ITEM_SET.SET_ID, IS_COMPLETED, TB_ITEM_SET.CONTACT_ID, IS_LOCKED,
			CODING_IS_FINAL, SET_NAME, CONTACT_NAME, Ext_URL, Ext_Type
		FROM TB_ITEM_SET
			INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
			INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = TB_ITEM_SET.CONTACT_ID
			INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
		WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
			and tb_ITEM_SET.IS_COMPLETED = 'false'
			and TB_ITEM_SET.CONTACT_ID = @CONTACT_ID
			AND TB_ITEM_SET.SET_ID = @SET_ID
		AND NOT TB_ITEM_SET.SET_ID IN
		(
			SELECT TB_ITEM_SET.SET_ID FROM TB_ITEM_SET
				INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID
				WHERE REVIEW_ID = @REVIEW_ID AND ITEM_ID = @ITEM_ID
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
				AND TB_ITEM_SET.SET_ID = @SET_ID
		)

SET NOCOUNT OFF
GO