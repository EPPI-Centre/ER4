USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReportData]    Script Date: 9/12/2016 12:28:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ReportData]
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_IDS NVARCHAR(MAX)
,	@REPORT_ID INT
,	@ORDER_BY NVARCHAR(15)
,	@ATTRIBUTE_ID BIGINT
,	@IS_QUESTION bit
,	@FULL_DETAILS bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @TT TABLE
	(
	  ITEM_ID BIGINT primary key
	)
	IF @ATTRIBUTE_ID != 0
	BEGIN
		INSERT INTO @TT
			SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
				AND TB_ITEM_REVIEW.IS_DELETED = 0
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
	END
	ELSE
	BEGIN
		INSERT INTO @TT
			SELECT VALUE FROM dbo.fn_Split_int(@ITEM_IDS, ',')
	END
    --First: the main report properties
	SELECT * from TB_REPORT where REPORT_ID = @REPORT_ID
	--Second: list of report columns
	SELECT * from TB_REPORT_COLUMN where REPORT_ID = @REPORT_ID ORDER BY COLUMN_ORDER
	--Third: what goes into each column, AKA "Rows" (In C# side)
	SELECT * from TB_REPORT_COLUMN_CODE  
		where REPORT_ID = @REPORT_ID ORDER BY CODE_ORDER
	
	
	--Fourth: most of the real data
	SELECT distinct cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID,cc.USER_DEF_TEXT
				,a.*, ia.*, i.ITEM_ID, i.OLD_ITEM_ID, i.SHORT_TITLE, CODE_ORDER, ATTRIBUTE_ORDER 
	from TB_REPORT_COLUMN_CODE cc
	INNER JOIN TB_ATTRIBUTE_SET tas ON (--Question reports fetch data about a given code children
										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
										OR 
										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
									   )
		AND tas.SET_ID = cc.SET_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	where REPORT_ID = @REPORT_ID 
	ORDER BY 
		i.SHORT_TITLE -- we ignore the sorting order required for this report, sorting is done on c# side.
		, i.ITEM_ID, CODE_ORDER, ATTRIBUTE_ORDER
	
	
	--Fift: data about coded TXT, uses "UNION" to grab data from TXT and PDF tables
	SELECT cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID, a.ATTRIBUTE_ID, tt.ITEM_ID, id.DOCUMENT_TITLE
	, 'Page ' + CONVERT(varchar(10),PAGE) + ':' + CHAR(10) + '[¬s]"' + replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' CODED_TEXT
	  from TB_REPORT_COLUMN_CODE cc
	INNER JOIN TB_ATTRIBUTE_SET tas ON (
										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
										OR 
										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
									   )
		AND tas.SET_ID = cc.SET_ID and cc.REPORT_ID = @REPORT_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	inner join TB_ITEM_DOCUMENT id on ia.ITEM_ID = id.ITEM_ID
	inner join TB_ITEM_ATTRIBUTE_PDF pdf on id.ITEM_DOCUMENT_ID = pdf.ITEM_DOCUMENT_ID and ia.ITEM_ATTRIBUTE_ID = pdf.ITEM_ATTRIBUTE_ID
	UNION
	SELECT cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID, a.ATTRIBUTE_ID, tt.ITEM_ID, id.DOCUMENT_TITLE
	, SUBSTRING(
					replace(id.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT
	  from TB_REPORT_COLUMN_CODE cc
	INNER JOIN TB_ATTRIBUTE_SET tas ON (
										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
										OR 
										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
									   )
		AND tas.SET_ID = cc.SET_ID and cc.REPORT_ID = @REPORT_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	inner join TB_ITEM_DOCUMENT id on ia.ITEM_ID = id.ITEM_ID
	inner join TB_ITEM_ATTRIBUTE_TEXT txt on id.ITEM_DOCUMENT_ID = txt.ITEM_DOCUMENT_ID and ia.ITEM_ATTRIBUTE_ID = txt.ITEM_ATTRIBUTE_ID
	
	--sixth, items that do not have anything to report
	
	SELECT i.ITEM_ID, i.OLD_ITEM_ID, i.SHORT_TITLE from TB_ITEM i
	inner join @TT t on t.ITEM_ID = i.ITEM_ID
	where t.ITEM_ID not in
	(SELECT distinct tt.ITEM_ID
	from TB_REPORT_COLUMN_CODE cc
	INNER JOIN TB_ATTRIBUTE_SET tas ON (--Question reports fetch data about a given code children
										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
										OR 
										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
									   )
		AND tas.SET_ID = cc.SET_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	where REPORT_ID = @REPORT_ID)
	
	
	ORDER BY 
		i.SHORT_TITLE -- we ignore the sorting order required for this report, sorting is done on c# side.
		, i.ITEM_ID
	--optional Seventh: get Title, Abstract and Year, only if some of this is needed.
	if (@FULL_DETAILS = 1)
	BEGIN
		select i.ITEM_ID, TITLE, ABSTRACT, [YEAR] from TB_ITEM i
			inner join @TT t on t.ITEM_ID = i.ITEM_ID
	END
END
GO



--USE [Reviewer]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ReviewStatisticsCodeSetsReviewersIncomplete]    Script Date: 7/19/2016 10:22:08 AM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO


--ALTER procedure [dbo].[st_ReviewStatisticsCodeSetsReviewersIncomplete]
--(
--	@REVIEW_ID INT,
--	@SET_ID INT 
--)

--As

--SET NOCOUNT ON

--declare @t table (ItemId bigint primary key)
--insert into @t SELECT distinct IS2.ITEM_ID
--	FROM TB_ITEM_SET IS2
--	INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = IS2.ITEM_ID AND IR2.REVIEW_ID = @REVIEW_ID
--	WHERE IS2.IS_COMPLETED = 'TRUE' AND IS2.SET_ID = @SET_ID

--SELECT SET_NAME, IS1.SET_ID, IS1.CONTACT_ID, CONTACT_NAME, COUNT(DISTINCT IS1.ITEM_ID) AS TOTAL
--FROM TB_ITEM_SET IS1
--INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = IS1.SET_ID
--INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.REVIEW_ID = TB_REVIEW_SET.REVIEW_ID AND TB_ITEM_REVIEW.ITEM_ID = IS1.ITEM_ID
--INNER JOIN TB_SET ON TB_SET.SET_ID = IS1.SET_ID
--INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = IS1.CONTACT_ID
--WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND IS1.IS_COMPLETED = 'FALSE' 
--AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND IS1.SET_ID = @SET_ID
--AND NOT IS1.ITEM_ID IN
--(
--	select ItemId from @t
--)
--GROUP BY IS1.SET_ID, SET_NAME, IS1.CONTACT_ID, CONTACT_NAME


--SET NOCOUNT OFF
--GO

--USE [Reviewer]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupCheckOngoing]    Script Date: 5/27/2016 2:39:05 PM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
---- =============================================
---- Author:		Sergio
---- Create date: 09/08/2010
---- Description:	check for pending SISS, attempt to save results
---- =============================================
--ALTER PROCEDURE [dbo].[st_ItemDuplicateGroupCheckOngoing] 
--	-- Add the parameters for the stored procedure here
--	@revID int
--	WITH RECOMPILE
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
--	Declare @guis_N int
--    -- Insert statements for procedure here
--	set @guis_N = (
--					SELECT COUNT(DISTINCT(EXTR_UI)) from TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID
--					AND EXTR_UI <> '10000000-0000-0000-0000-000000000000'
--					)
--	IF @guis_N = 1
--	BEGIN --send back a return code to signify that the SISS package is still running
--		return -2
--	END
--	ELSE
--	IF @guis_N > 1 --SISS package has saved data but results were never collected
--	BEGIN
--		declare @UI uniqueidentifier
--		UPDATE TB_ITEM_DUPLICATES_TEMP
--			SET EXTR_UI = '10000000-0000-0000-0000-000000000000'
--			WHERE EXTR_UI = '00000000-0000-0000-0000-000000000000' AND REVIEW_ID = @revID
		
--		set @UI = (SELECT top 1 EXTR_UI from TB_ITEM_DUPLICATES_TEMP where EXTR_UI <> '10000000-0000-0000-0000-000000000000' AND REVIEW_ID = @revID)
--		SET @guis_N = (SELECT COUNT(ITEM_DUPLICATES_ID) from TB_ITEM_DUPLICATES_TEMP where EXTR_UI = @UI and DESTINATION is null)
--		if @guis_N > 0 --do the costly bits only if they weren't done already
--		BEGIN
--		--delete sigleton rows from SSIS results
--			DELETE from TB_ITEM_DUPLICATES_TEMP 
--			where EXTR_UI = @UI AND _key_out not in
--				(
--					Select t1._key_in from TB_ITEM_DUPLICATES_TEMP t1 
--					inner join TB_ITEM_DUPLICATES_TEMP t2 on t1._key_in = t2._key_out and t1._key_in <> t2._key_in
--					and t1.EXTR_UI = t2.EXTR_UI and t1.EXTR_UI = @UI
--						GROUP by t1._key_in
--				)
			
--			--the difficult part: match the results in TB_ITEM_DUPLICATES_TEMP with existing groups
--			-- the system works indifferently for missing groups and missing groups members, and to make it relatively fast, 
--			-- we store the "destination" group ID in the temporary table, this is done in two parts,
--			--the following query matches the current SSIS results with existing groups and sets the DESTINATION field accordingly
--			--the remaining Null "destination" fields will signal that the group is new and has to be created
--			--after creating the new groups, the destination field will be populated for the remaining records and finally the new group
--			--members will be added to existing groups.

--			declare @i1i2 table (i1 int, i2 int) 
--					insert into @i1i2
--					select s.ITEM_ID, ss.ITEM_ID  from TB_ITEM_DUPLICATES_TEMP s 
--										inner join TB_ITEM_DUPLICATES_TEMP ss 
--										on s._key_out = ss._key_out and s._key_in = ss._key_out 
--										and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
--										and s.REVIEW_ID = @revID and ss.REVIEW_ID = @revID

--			UPDATE dt set DESTINATION = a.ITEM_DUPLICATE_GROUP_ID
--			 FROM TB_ITEM_DUPLICATES_TEMP dt INNER JOIN   
--				(
--					SELECT m.ITEM_DUPLICATE_GROUP_ID, COUNT(m.GROUP_MEMBER_ID) cc, ins._key_out Results_Group 
--					From TB_ITEM_DUPLICATE_GROUP_MEMBERS m 
--						inner join TB_ITEM_REVIEW IR on m.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.REVIEW_ID = @revID
--						inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS m1 
--							on m.item_duplicate_group_ID = m1.item_duplicate_group_ID
--							AND m.ITEM_REVIEW_ID <> m1.ITEM_REVIEW_ID
--						Inner join TB_ITEM_REVIEW IR1 on m1.ITEM_REVIEW_ID = IR1.ITEM_REVIEW_ID and IR1.REVIEW_ID = @revID
--						inner Join TB_ITEM_DUPLICATE_GROUP g on  m.item_duplicate_group_ID = g.item_duplicate_group_ID
--						Inner join TB_ITEM_DUPLICATES_TEMP ins on ins.ITEM_ID = IR.ITEM_ID
--						Inner join @i1i2 i1i2 on i1i2.i1 = IR.ITEM_ID AND i1i2.i2 = IR1.ITEM_ID
--						where g.Review_ID = @revID and ins.REVIEW_ID = @revID and ins.EXTR_UI = @UI
--						--AND (CAST(IR.ITEM_ID as nvarchar(20)) + '#' + CAST(IR1.ITEM_ID as nvarchar(20))) in 
--						--		(
--						--			select * from @has
--						--			--select (CAST(s.ITEM_ID as nvarchar(1000)) + '#' + CAST(ss.ITEM_ID as nvarchar(1000))) from TB_ITEM_DUPLICATES_TEMP s 
--						--			--	inner join TB_ITEM_DUPLICATES_TEMP ss 
--						--			--	on s._key_out = ss._key_out and s._key_in = ss._key_out 
--						--			--	and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
--						--			--	and s.REVIEW_ID = @revID and ss.REVIEW_ID = @revID
--						--		)
--					group by m.ITEM_DUPLICATE_GROUP_ID, ins._key_out
--				) a 
--				on dt._key_out = a.Results_Group 
--				WHERE a.cc > 0 and dt.EXTR_UI = @UI

--			--for groups that are not already present: add group & master
--			insert into TB_ITEM_DUPLICATE_GROUP (REVIEW_ID, ORIGINAL_ITEM_ID)
--				SELECT REVIEW_ID, Item_ID from TB_ITEM_DUPLICATES_TEMP where 
--					EXTR_UI = @UI
--					AND DESTINATION is null
--					AND _key_in = _key_out --this is how you identify groups...
--			--add the master record in the members table
--			INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
--			(
--				ITEM_DUPLICATE_GROUP_ID
--				,ITEM_REVIEW_ID
--				,SCORE
--				,IS_CHECKED
--				,IS_DUPLICATE
--			)
--			SELECT ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID, 1, 1, 0
--			FROM TB_ITEM_DUPLICATE_GROUP DG inner join TB_ITEM_DUPLICATES_TEMP dt 
--				on DG.ORIGINAL_ITEM_ID = dt.ITEM_ID
--				AND EXTR_UI = @UI
--				AND DESTINATION is null
--				AND _key_in = _key_out
--				INNER JOIN TB_ITEM_REVIEW IR  on IR.ITEM_ID = DG.ORIGINAL_ITEM_ID and IR.REVIEW_ID = @revID

--			--update TB_ITEM_DUPLICATE_GROUP to set the MASTER_MEMBER_ID correctly (now that we have a line in TB_ITEM_DUPLICATE_GROUP_MEMBERS)
--			UPDATE TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = a.GMI
--			FROM (
--					SELECT GROUP_MEMBER_ID GMI, IR.ITEM_ID from TB_ITEM_DUPLICATE_GROUP idg
--						inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on gm.ITEM_DUPLICATE_GROUP_ID = idg.ITEM_DUPLICATE_GROUP_ID and MASTER_MEMBER_ID is null
--						inner join TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID AND IR.REVIEW_ID = @revID
--						inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
--						AND EXTR_UI = @UI AND dt._key_in = dt._key_out and dt.DESTINATION is null
--			) a  
--			WHERE ORIGINAL_ITEM_ID = a.ITEM_ID and MASTER_MEMBER_ID is null

			
--			-- add the newly created group IDs to temporary table
--			UPDATE TB_ITEM_DUPLICATES_TEMP set DESTINATION = a.DGI
--			FROM (
--				SELECT ITEM_DUPLICATE_GROUP_ID DGI, dt.ITEM_ID MAST, dt1.ITEM_ID CURR_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
--					inner JOIN TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
--					inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
--					AND EXTR_UI = @UI AND dt._key_in = dt._key_out 
--					inner JOIN TB_ITEM_DUPLICATES_TEMP dt1 on dt._key_in = dt1._key_out and dt.DESTINATION is null
--					and dt1.EXTR_UI = @UI
--			) a
--			where a.CURR_ID = TB_ITEM_DUPLICATES_TEMP.ITEM_ID
--		END
--		-- add non master members that are not currently present
--		declare  @t table (goodIDs bigint)
--		insert into @t select distinct item_id from TB_ITEM_DUPLICATES_TEMP where EXTR_UI = @UI --and _key_in != _key_out 
--		--select COUNT (goodids) from @t
--		delete from @t where goodIDs in (
--			SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
--			inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
--			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
--			on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
--			)
--		--select COUNT (goodids) from @t
		
--		-- add non master members that are not currently present
--		INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
--		(
--			ITEM_DUPLICATE_GROUP_ID
--			,ITEM_REVIEW_ID
--			,SCORE
--			,IS_CHECKED
--			,IS_DUPLICATE
--		)
--		SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
--		from TB_ITEM_DUPLICATES_TEMP DT
--		inner join @t t on DT.ITEM_ID = t.goodIDs
--		inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
		
--		--INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
--		--(
--		--	ITEM_DUPLICATE_GROUP_ID
--		--	,ITEM_REVIEW_ID
--		--	,SCORE
--		--	,IS_CHECKED
--		--	,IS_DUPLICATE
--		--)
--		--SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
--		--from TB_ITEM_DUPLICATES_TEMP DT
--		--inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
--		--WHERE DT.ITEM_ID not in 
--		--(
--		--	SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
--		--	inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
--		--	inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
--		--	on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
--		--)
		
--		--remove temporary results.
--		delete FROM TB_ITEM_DUPLICATES_TEMP WHERE REVIEW_ID = @revID
--	END
--END
--GO

--USE [Reviewer]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateFindNew]    Script Date: 5/27/2016 4:30:26 PM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
---- =============================================
---- Author:		Sergio
---- Create date: 18/08/2010
---- Description:	BIG query to search for duplicates, will not delete or overwrite old items, it will add new duplicate canditades. 
---- =============================================
--ALTER PROCEDURE [dbo].[st_ItemDuplicateFindNew]
--	-- Add the parameters for the stored procedure here
--	@revID int = 0
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
--	-- First check: if there are no items to evaluate, just go back
--	declare @check int = (SELECT COUNT(ITEM_ID) from TB_ITEM_REVIEW where REVIEW_ID = @revID and IS_DELETED = 0)
--	if @check = 0 
--	BEGIN
--		Return -1
--	END
--	SET  @check =(SELECT COUNT(DISTINCT(EXTR_UI)) from TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID)
--	IF @check = 1 
--	BEGIN
--		return 1 --a SISS package is still running for Review, we should not run it again
--	END
--	ELSE IF @check > 1 --this should not happen: SISS package saved some data, but the result was not collected. 
--	-- Since new items might have been inserted in the mean time, we will delete old results and start over again.
--	BEGIN
--		DELETE FROM TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID 
--	END

--	declare @UI uniqueidentifier
--	set @UI = '00000000-0000-0000-0000-000000000000'
--	--insert a marker line to notify that the SISS package has been triggered
--	insert into TB_ITEM_DUPLICATES_TEMP (REVIEW_ID, EXTR_UI) Values (@revID, @UI)

--	set @UI = NEWID()

--	--run the package that populates the temp results table
--	declare @cmd varchar(1000)
--	select @cmd = 'dtexec /DT "File System\DuplicateCheckAzure"'
--	select @cmd = @cmd + ' /Rep N  /SET \Package.Variables[User::RevID].Properties[Value];"' + CAST(@revID as varchar(max))+ '"' 
--	select @cmd = @cmd + ' /SET \Package.Variables[User::UID].Properties[Value];"' + CAST(@UI as varchar(max))+ '"' 
--	EXEC xp_cmdshell @cmd
	
--	--delete sigleton rows from SSIS results
--	DELETE from TB_ITEM_DUPLICATES_TEMP 
--	where EXTR_UI = @UI AND _key_out not in
--		(
--			Select t1._key_in from TB_ITEM_DUPLICATES_TEMP t1 
--			inner join TB_ITEM_DUPLICATES_TEMP t2 on t1._key_in = t2._key_out and t1._key_in <> t2._key_in
--			and t1.EXTR_UI = t2.EXTR_UI and t1.EXTR_UI = @UI
--				GROUP by t1._key_in
--		)
	
--	--the difficult part: match the results in TB_ITEM_DUPLICATES_TEMP with existing groups
--	-- the system works indifferently for missing groups and missing groups members, and to make it relatively fast, 
--	-- we store the "destination" group ID in the temporary table, this is done in two parts,
--	--the following query matches the current SSIS results with existing groups and sets the DESTINATION field accordingly
--	--the remaining Null "destination" fields will signal that the group is new and has to be created
--	--after creating the new groups, the destination field will be populated for the remaining records and finally the new group
--	--members will be added to existing groups.

--	declare @i1i2 table (i1 int, i2 int) 
--					insert into @i1i2
--					select s.ITEM_ID, ss.ITEM_ID  from TB_ITEM_DUPLICATES_TEMP s 
--										inner join TB_ITEM_DUPLICATES_TEMP ss 
--										on s._key_out = ss._key_out and s._key_in = ss._key_out 
--										and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
--										and s.REVIEW_ID = @revID and ss.REVIEW_ID = @revID



--	UPDATE dt set DESTINATION = a.ITEM_DUPLICATE_GROUP_ID
--	 FROM TB_ITEM_DUPLICATES_TEMP dt INNER JOIN   
--		(
--			SELECT m.ITEM_DUPLICATE_GROUP_ID, COUNT(m.GROUP_MEMBER_ID) cc, ins._key_out Results_Group 
--				From TB_ITEM_DUPLICATE_GROUP_MEMBERS m 
--					inner join TB_ITEM_REVIEW IR on m.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.REVIEW_ID = @revID
--					inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS m1 
--						on m.item_duplicate_group_ID = m1.item_duplicate_group_ID
--						AND m.ITEM_REVIEW_ID <> m1.ITEM_REVIEW_ID
--					Inner join TB_ITEM_REVIEW IR1 on m1.ITEM_REVIEW_ID = IR1.ITEM_REVIEW_ID and IR1.REVIEW_ID = @revID
--					inner Join TB_ITEM_DUPLICATE_GROUP g on  m.item_duplicate_group_ID = g.item_duplicate_group_ID
--					Inner join TB_ITEM_DUPLICATES_TEMP ins on ins.ITEM_ID = IR.ITEM_ID
--					Inner join @i1i2 i1i2 on i1i2.i1 = IR.ITEM_ID AND i1i2.i2 = IR1.ITEM_ID
--					where g.Review_ID = @revID and ins.REVIEW_ID = @revID and ins.EXTR_UI = @UI
--					--AND (CAST(IR.ITEM_ID as nvarchar(20)) + '#' + CAST(IR1.ITEM_ID as nvarchar(20))) in 
--					--		(
--					--			select * from @has
--					--			--select (CAST(s.ITEM_ID as nvarchar(1000)) + '#' + CAST(ss.ITEM_ID as nvarchar(1000))) from TB_ITEM_DUPLICATES_TEMP s 
--					--			--	inner join TB_ITEM_DUPLICATES_TEMP ss 
--					--			--	on s._key_out = ss._key_out and s._key_in = ss._key_out 
--					--			--	and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
--					--			--	and s.REVIEW_ID = @revID and ss.REVIEW_ID = @revID
--					--		)
--				group by m.ITEM_DUPLICATE_GROUP_ID, ins._key_out
--		) a 
--		on dt._key_out = a.Results_Group
--		WHERE a.cc > 0  and dt.EXTR_UI = @UI

--	--for groups that are not already present: add group & master
--	insert into TB_ITEM_DUPLICATE_GROUP (REVIEW_ID, ORIGINAL_ITEM_ID)
--		SELECT REVIEW_ID, Item_ID from TB_ITEM_DUPLICATES_TEMP where 
--			EXTR_UI = @UI
--			AND DESTINATION is null
--			AND _key_in = _key_out --this is how you identify groups...
--	--add the master record in the members table
--	INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
--	(
--		ITEM_DUPLICATE_GROUP_ID
--		,ITEM_REVIEW_ID
--		,SCORE
--		,IS_CHECKED
--		,IS_DUPLICATE
--	)
--	SELECT ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID, 1, 1, 0
--	FROM TB_ITEM_DUPLICATE_GROUP DG inner join TB_ITEM_DUPLICATES_TEMP dt 
--		on DG.ORIGINAL_ITEM_ID = dt.ITEM_ID
--		AND EXTR_UI = @UI
--		AND DESTINATION is null
--		AND _key_in = _key_out
--		INNER JOIN TB_ITEM_REVIEW IR  on IR.ITEM_ID = DG.ORIGINAL_ITEM_ID and IR.REVIEW_ID = @revID

--	--update TB_ITEM_DUPLICATE_GROUP to set the MASTER_MEMBER_ID correctly (now that we have a line in TB_ITEM_DUPLICATE_GROUP_MEMBERS)
--	UPDATE TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = a.GMI
--	FROM (
--		SELECT GROUP_MEMBER_ID GMI, IR.ITEM_ID from TB_ITEM_DUPLICATE_GROUP idg
--			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on gm.ITEM_DUPLICATE_GROUP_ID = idg.ITEM_DUPLICATE_GROUP_ID and MASTER_MEMBER_ID is null
--			inner join TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID AND IR.REVIEW_ID = @revID
--			inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
--			AND EXTR_UI = @UI AND dt._key_in = dt._key_out and dt.DESTINATION is null
--	) a  
--	WHERE ORIGINAL_ITEM_ID = a.ITEM_ID and MASTER_MEMBER_ID is null

	
--	-- add the newly created group IDs to temporary table
--	UPDATE TB_ITEM_DUPLICATES_TEMP set DESTINATION = a.DGI
--	FROM (
--		SELECT ITEM_DUPLICATE_GROUP_ID DGI, dt.ITEM_ID MAST, dt1.ITEM_ID CURR_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
--			inner JOIN TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
--			inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
--			AND EXTR_UI = @UI AND dt._key_in = dt._key_out 
--			inner JOIN TB_ITEM_DUPLICATES_TEMP dt1 on dt._key_in = dt1._key_out and dt.DESTINATION is null
--			and dt1.EXTR_UI = @UI --!!!!!!!!!!!!!!
--	) a
--	where a.CURR_ID = TB_ITEM_DUPLICATES_TEMP.ITEM_ID

--	-- add non master members that are not currently present
--	--INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
--	--(
--	--	ITEM_DUPLICATE_GROUP_ID
--	--	,ITEM_REVIEW_ID
--	--	,SCORE
--	--	,IS_CHECKED
--	--	,IS_DUPLICATE
--	--)
--	--SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
--	--from TB_ITEM_DUPLICATES_TEMP DT
--	--inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
--	--WHERE DT.ITEM_ID not in 
--	--(
--	--	SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
--	--	inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
--	--	inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
--	--	on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
--	--)
--	declare  @t table (goodIDs bigint)
--		insert into @t select distinct item_id from TB_ITEM_DUPLICATES_TEMP where EXTR_UI = @UI --and _key_in != _key_out 
--		select COUNT (goodids) from @t
--		delete from @t where goodIDs in (
--			SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
--			inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
--			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
--			on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
--			)
--		select COUNT (goodids) from @t
		
--		-- add non master members that are not currently present
--		INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
--		(
--			ITEM_DUPLICATE_GROUP_ID
--			,ITEM_REVIEW_ID
--			,SCORE
--			,IS_CHECKED
--			,IS_DUPLICATE
--		)
--		SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
--		from TB_ITEM_DUPLICATES_TEMP DT
--		inner join @t t on DT.ITEM_ID = t.goodIDs
--		inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
--	--remove temporary results.
--	delete FROM TB_ITEM_DUPLICATES_TEMP WHERE REVIEW_ID = @revID
--	END

--GO


--USE [Reviewer]
--GO
--/****** Object:  StoredProcedure [dbo].[st_OutcomeList]    Script Date: 5/24/2016 5:08:56 PM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--ALTER procedure [dbo].[st_OutcomeList]
--(
--	@REVIEW_ID INT,
--	@META_ANALYSIS_ID INT,
--	@QUESTIONS nvarchar(max) = '',
--	@ANSWERS nvarchar(max) = ''
--	/*@SET_ID BIGINT,
--	@ITEM_ATTRIBUTE_ID_INTERVENTION BIGINT = NULL,
--	@ITEM_ATTRIBUTE_ID_CONTROL BIGINT = NULL,
--	@ITEM_ATTRIBUTE_ID_OUTCOME BIGINT = NULL,
--	@ATTRIBUTE_ID BIGINT = NULL,
	
	
--	@VARIABLES NVARCHAR(MAX) = NULL,
--	@ANSWERS NVARCHAR(MAX) = '',
--	@QUESTIONS NVARCHAR(MAX) = ''*/
--)

--As

--SET NOCOUNT ON
--	declare @t table (OUTCOME_ID int, META_ANALYSIS_OUTCOME_ID int)
--	insert into @t select tio.OUTCOME_ID, META_ANALYSIS_OUTCOME_ID from 
--	TB_ITEM_OUTCOME tio
--	inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tio.ITEM_SET_ID
--		AND tis.IS_COMPLETED = 1
--	inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @REVIEW_ID and rs.SET_ID = tis.SET_ID
--	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tis.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tis.ITEM_SET_ID
--	inner join TB_ITEM on TB_ITEM.ITEM_ID = tis.ITEM_ID
--	inner JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
--		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID

--	SELECT distinct tio.OUTCOME_ID, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
--	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, SHORT_TITLE, OUTCOME_DESCRIPTION,
--	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
--	t.META_ANALYSIS_OUTCOME_ID, 
--	tis.ITEM_SET_ID, TB_ITEM_ATTRIBUTE.ITEM_ID,
--	A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT, A3.ATTRIBUTE_NAME OUTCOME_TEXT
--	FROM TB_ITEM_OUTCOME tio
--	inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tio.ITEM_SET_ID
--		AND tis.IS_COMPLETED = 1
--	inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @REVIEW_ID and rs.SET_ID = tis.SET_ID
--	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tis.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tis.ITEM_SET_ID
--	inner join TB_ITEM on TB_ITEM.ITEM_ID = tis.ITEM_ID
--	LEFT OUTER JOIN @t t ON t.OUTCOME_ID = tio.OUTCOME_ID
--		--AND t.META_ANALYSIS_ID = @META_ANALYSIS_ID
--	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_INTERVENTION 
--	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_CONTROL
--	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_OUTCOME

--	--second sets of results, the answers
--	--we need to get these, even if empty, so that we always get a reader
	
--	IF (@QUESTIONS is not null AND @QUESTIONS != '')
--	BEGIN
--		declare @QT table ( AttID bigint primary key)
--		insert into @QT select qss.value from dbo.fn_Split_int(@QUESTIONS, ',') as qss
--		select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID 
--			, ATTRIBUTE_NAME as Codename
--			, a.ATTRIBUTE_ID as ATTRIBUTE_ID
--			, tas.PARENT_ATTRIBUTE_ID
--		from TB_ITEM_OUTCOME tio 
--		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
--		inner join TB_ITEM_SET tis2 on tis.ITEM_ID = tis2.ITEM_ID
--		inner join TB_ITEM_ATTRIBUTE tia on tis2.ITEM_SET_ID = tia.ITEM_SET_ID
--		inner join TB_REVIEW_SET rs on tis2.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
--		inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
--		inner join @QT Qs on Qs.AttID = tas.PARENT_ATTRIBUTE_ID
--		inner join TB_ATTRIBUTE a on tas.ATTRIBUTE_ID = a.ATTRIBUTE_ID
--		order by OUTCOME_ID, tas.PARENT_ATTRIBUTE_ID, tas.ATTRIBUTE_ORDER, a.ATTRIBUTE_ID
--	END
--	ELSE
--	BEGIN
--	select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
--		tio.OUTCOME_TITLE as Codename
--		, tio.ITEM_ATTRIBUTE_ID_CONTROL as ATTRIBUTE_ID, tio.ITEM_ATTRIBUTE_ID_CONTROL as PARENT_ATTRIBUTE_ID
--		from TB_ITEM_OUTCOME tio
--		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and 1=0
--	END


--	IF (@ANSWERS is not null AND @ANSWERS != '')
--	BEGIN
--	--third set of results, the questions
--	declare @AT table ( AttID bigint primary key)
--	insert into @AT select qss.value from dbo.fn_Split_int(@ANSWERS, ',') as qss
--	select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
--		--(Select top(1) a.ATTRIBUTE_NAME from @AT 
--		--inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = AttID
--		--inner join TB_ATTRIBUTE a on tas.ATTRIBUTE_ID = a.ATTRIBUTE_ID
--		--inner join TB_ITEM_ATTRIBUTE tia on a.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
--		--inner join TB_ITEM_SET tis1 on tia.ITEM_SET_ID = tis1.ITEM_SET_ID and tis1.IS_COMPLETED = 1 and tis.ITEM_ID = tis1.ITEM_ID
--		--inner join TB_REVIEW_SET rs1 on tis1.SET_ID = rs1.SET_ID and rs1.REVIEW_ID = @REVIEW_ID
--		--order by tas.ATTRIBUTE_ORDER ) as Codename
--		tia.ATTRIBUTE_ID, a.ATTRIBUTE_NAME
--	from TB_ITEM_OUTCOME tio 
--	inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
--	inner join TB_REVIEW_SET rs on tis.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
--	inner join TB_ITEM_SET tis2 on tis.ITEM_ID = tis2.ITEM_ID and tis2.IS_COMPLETED = 1
--	inner join TB_ATTRIBUTE_SET tas on tis2.SET_ID = tas.SET_ID
--	inner join TB_ITEM_ATTRIBUTE tia on tis2.ITEM_ID = tia.ITEM_ID and tas.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
--	inner join @AT on AttID = tia.ATTRIBUTE_ID
--	inner join TB_ATTRIBUTE a on tia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
--	order by OUTCOME_ID
--	END
--	ELSE
--	BEGIN
--		select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
--		tio.ITEM_ATTRIBUTE_ID_CONTROL as ATTRIBUTE_ID, tio.OUTCOME_TITLE as ATTRIBUTE_NAME
--	from TB_ITEM_OUTCOME tio inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and 1=0
	
--	END
	
----DECLARE @START_TEXT NVARCHAR(MAX) = N' SELECT distinct tio.OUTCOME_ID, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
----	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, SHORT_TITLE, OUTCOME_DESCRIPTION,
----	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
----	TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_OUTCOME_ID, TB_ITEM_SET.ITEM_SET_ID, TB_ITEM_ATTRIBUTE.ITEM_ID,
----	A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT, A3.ATTRIBUTE_NAME OUTCOME_TEXT'
	
----DECLARE @END_TEXT NVARCHAR(MAX) = N' FROM TB_ITEM_OUTCOME tio

----	inner join TB_ITEM_SET on tb_item_set.ITEM_SET_ID = tio.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = ''TRUE''
----	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tb_item_set.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
----	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tb_item_set.ITEM_SET_ID
----	inner join TB_ITEM on TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
	
----	LEFT OUTER JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
----		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID
		
----	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_INTERVENTION 
----	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_CONTROL
----	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_OUTCOME'
	
----DECLARE @QUERY NVARCHAR(MAX) = @VARIABLES + @START_TEXT + @ANSWERS + @QUESTIONS + @END_TEXT
	
----EXEC (@QUERY)

----/*
----SELECT distinct tio.OUTCOME_ID, SHORT_TITLE, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
----	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, OUTCOME_DESCRIPTION,
----	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
----	TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_OUTCOME_ID, TB_ITEM_SET.ITEM_SET_ID,
----	TB_ITEM_ATTRIBUTE.ITEM_ID, A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT,
----		A3.ATTRIBUTE_NAME OUTCOME_TEXT
	
----	FROM TB_ITEM_OUTCOME tio

----	inner join TB_ITEM_SET on tb_item_set.ITEM_SET_ID = tio.ITEM_SET_ID
----		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tb_item_set.ITEM_ID
----	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tb_item_set.ITEM_SET_ID
----	inner join TB_ITEM on TB_ITEM.ITEM_ID = TB_ITEM_SET.ITEM_ID
	
----	LEFT OUTER JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
----		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID
		
----	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_INTERVENTION 
----	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_CONTROL
----	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_OUTCOME
	
----	WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
----	AND (@ITEM_ATTRIBUTE_ID_INTERVENTION = 0 OR (ITEM_ATTRIBUTE_ID_INTERVENTION = @ITEM_ATTRIBUTE_ID_INTERVENTION))
----	AND (@ITEM_ATTRIBUTE_ID_CONTROL = 0 OR (ITEM_ATTRIBUTE_ID_CONTROL = @ITEM_ATTRIBUTE_ID_CONTROL))
----	AND (@ITEM_ATTRIBUTE_ID_OUTCOME = 0 OR (ITEM_ATTRIBUTE_ID_OUTCOME = @ITEM_ATTRIBUTE_ID_OUTCOME))
----	--	AND (@ATTRIBUTE_ID IS NULL OR (TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID))
----	--AND (
----	--	@ATTRIBUTE_ID IS NULL OR 
----	--		(
----	--		TB_ITEM_SET.ITEM_ID IN
----	--			( 
----	--			SELECT IA2.ITEM_ID FROM TB_ITEM_ATTRIBUTE IA2 
----	--			INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 'TRUE'
----	--			WHERE IA2.ATTRIBUTE_ID = @ATTRIBUTE_ID
----	--			)
----	--		)
----	--	)
----	--AND (--temp correction for before publishing: @ATTRIBUTE_ID is (because of bug) actually the item_attribute_id
----	--	@ATTRIBUTE_ID = 0 OR 
----	--		(
----	--		tio.OUTCOME_ID IN
----	--			( 
----	--				select tio2.OUTCOME_ID from TB_ATTRIBUTE_SET tas
----	--				inner join TB_ITEM_OUTCOME_ATTRIBUTE ioa on tas.ATTRIBUTE_ID = ioa.ATTRIBUTE_ID and tas.ATTRIBUTE_SET_ID = @ATTRIBUTE_ID
----	--				inner join TB_ITEM_OUTCOME tio2 on ioa.OUTCOME_ID = tio2.OUTCOME_ID
----	--			)
----	--		)
----	--	)--end of temp correction
----	AND (--real correction to use when bug is corrected in line 174 of dialogMetaAnalysisSetup.xaml.cs
----		@ATTRIBUTE_ID = 0 OR 
----			(
----			tio.OUTCOME_ID IN 
----				( 
----					select tio2.OUTCOME_ID from TB_ITEM_OUTCOME_ATTRIBUTE ioa  
----					inner join TB_ITEM_OUTCOME tio2 on ioa.OUTCOME_ID = tio2.OUTCOME_ID and ioa.ATTRIBUTE_ID = @ATTRIBUTE_ID
----				)
----			)
----		)--end of real correction
----	AND (@SET_ID = 0 OR (TB_ITEM_SET.SET_ID = @SET_ID))
	
----	--order by TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
----*/
--SET NOCOUNT OFF
--GO

--USE [Reviewer]
--GO
--CREATE NONCLUSTERED INDEX [IX_REVIEW_ID]
--ON [dbo].[TB_REVIEW_SET] ([REVIEW_ID])

--GO

--CREATE NONCLUSTERED INDEX [IX_ITEM_DUPLICATE_GROUP_ID_ITEM_REVIEW_ID]
--ON [dbo].[TB_ITEM_DUPLICATE_GROUP_MEMBERS] ([ITEM_DUPLICATE_GROUP_ID])
--INCLUDE ([ITEM_REVIEW_ID])
--GO

--CREATE NONCLUSTERED INDEX [IX_REVIEW_ID_ITEM_DUPLICATE_GROUP_ID]
--ON [dbo].[TB_ITEM_DUPLICATE_GROUP] ([REVIEW_ID])
--INCLUDE ([ITEM_DUPLICATE_GROUP_ID])
--GO

----USE [Reviewer]
----GO
----/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateFindNew]    Script Date: 03/11/2016 12:17:40 ******/
----SET ANSI_NULLS ON
----GO
----SET QUOTED_IDENTIFIER ON
----GO
------ =============================================
------ Author:		Sergio
------ Create date: 18/08/2010
------ Description:	BIG query to search for duplicates, will not delete or overwrite old items, it will add new duplicate canditades. 
------ =============================================
----ALTER PROCEDURE [dbo].[st_ItemDuplicateFindNew]
----	-- Add the parameters for the stored procedure here
----	@revID int = 0
----AS
----BEGIN
----	-- SET NOCOUNT ON added to prevent extra result sets from
----	-- interfering with SELECT statements.
----	SET NOCOUNT ON;
----	-- First check: if there are no items to evaluate, just go back
----	declare @check int = (SELECT COUNT(ITEM_ID) from TB_ITEM_REVIEW where REVIEW_ID = @revID and IS_DELETED = 0)
----	if @check = 0 
----	BEGIN
----		Return -1
----	END
----	SET  @check =(SELECT COUNT(DISTINCT(EXTR_UI)) from TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID)
----	IF @check = 1 
----	BEGIN
----		return 1 --a SISS package is still running for Review, we should not run it again
----	END
----	ELSE IF @check > 1 --this should not happen: SISS package saved some data, but the result was not collected. 
----	-- Since new items might have been inserted in the mean time, we will delete old results and start over again.
----	BEGIN
----		DELETE FROM TB_ITEM_DUPLICATES_TEMP where REVIEW_ID = @revID 
----	END

----	declare @UI uniqueidentifier
----	set @UI = '00000000-0000-0000-0000-000000000000'
----	--insert a marker line to notify that the SISS package has been triggered
----	insert into TB_ITEM_DUPLICATES_TEMP (REVIEW_ID, EXTR_UI) Values (@revID, @UI)

----	set @UI = NEWID()

----	--run the package that populates the temp results table
----	declare @cmd varchar(1000)
----	select @cmd = 'dtexec /DT "File System\DuplicateCheckAzure"'
----	select @cmd = @cmd + ' /Rep N  /SET \Package.Variables[User::RevID].Properties[Value];"' + CAST(@revID as varchar(max))+ '"' 
----	select @cmd = @cmd + ' /SET \Package.Variables[User::UID].Properties[Value];"' + CAST(@UI as varchar(max))+ '"' 
----	EXEC xp_cmdshell @cmd
	
----	--delete sigleton rows from SSIS results
----	DELETE from TB_ITEM_DUPLICATES_TEMP 
----	where EXTR_UI = @UI AND _key_out not in
----		(
----			Select t1._key_in from TB_ITEM_DUPLICATES_TEMP t1 
----			inner join TB_ITEM_DUPLICATES_TEMP t2 on t1._key_in = t2._key_out and t1._key_in <> t2._key_in
----			and t1.EXTR_UI = t2.EXTR_UI and t1.EXTR_UI = @UI
----				GROUP by t1._key_in
----		)
	
----	--the difficult part: match the results in TB_ITEM_DUPLICATES_TEMP with existing groups
----	-- the system works indifferently for missing groups and missing groups members, and to make it relatively fast, 
----	-- we store the "destination" group ID in the temporary table, this is done in two parts,
----	--the following query matches the current SSIS results with existing groups and sets the DESTINATION field accordingly
----	--the remaining Null "destination" fields will signal that the group is new and has to be created
----	--after creating the new groups, the destination field will be populated for the remaining records and finally the new group
----	--members will be added to existing groups.
----	UPDATE dt set DESTINATION = a.ITEM_DUPLICATE_GROUP_ID
----	 FROM TB_ITEM_DUPLICATES_TEMP dt INNER JOIN   
----		(
----			SELECT m.ITEM_DUPLICATE_GROUP_ID, COUNT(m.GROUP_MEMBER_ID) cc, ins._key_out Results_Group 
----			From TB_ITEM_DUPLICATE_GROUP_MEMBERS m 
----				inner join TB_ITEM_REVIEW IR on m.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.REVIEW_ID = @revID
----				inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS m1 
----					on m.item_duplicate_group_ID = m1.item_duplicate_group_ID
----					AND m.ITEM_REVIEW_ID <> m1.ITEM_REVIEW_ID
----				Inner join TB_ITEM_REVIEW IR1 on m1.ITEM_REVIEW_ID = IR1.ITEM_REVIEW_ID and IR1.REVIEW_ID = @revID
----				inner Join TB_ITEM_DUPLICATE_GROUP g on  m.item_duplicate_group_ID = g.item_duplicate_group_ID
----				Inner join TB_ITEM_DUPLICATES_TEMP ins on ins.ITEM_ID = IR.ITEM_ID
----				where g.Review_ID = @revID 
----				AND (CAST(IR.ITEM_ID as nvarchar(1000)) + '#' + CAST(IR1.ITEM_ID as nvarchar(1000))) in 
----						(
----							select (CAST(s.ITEM_ID as nvarchar(1000)) + '#' + CAST(ss.ITEM_ID as nvarchar(1000))) from TB_ITEM_DUPLICATES_TEMP s 
----								inner join TB_ITEM_DUPLICATES_TEMP ss 
----								on s._key_out = ss._key_out and s._key_in = ss._key_out 
----								and s.ITEM_ID <> ss.ITEM_ID AND s.EXTR_UI = @UI and ss.EXTR_UI = @UI
----						)
----			group by m.ITEM_DUPLICATE_GROUP_ID, ins._key_out
----		) a 
----		on dt._key_out = a.Results_Group
----		WHERE a.cc > 0  and dt.EXTR_UI = @UI

----	--for groups that are not already present: add group & master
----	insert into TB_ITEM_DUPLICATE_GROUP (REVIEW_ID, ORIGINAL_ITEM_ID)
----		SELECT REVIEW_ID, Item_ID from TB_ITEM_DUPLICATES_TEMP where 
----			EXTR_UI = @UI
----			AND DESTINATION is null
----			AND _key_in = _key_out --this is how you identify groups...
----	--add the master record in the members table
----	INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
----	(
----		ITEM_DUPLICATE_GROUP_ID
----		,ITEM_REVIEW_ID
----		,SCORE
----		,IS_CHECKED
----		,IS_DUPLICATE
----	)
----	SELECT ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID, 1, 1, 0
----	FROM TB_ITEM_DUPLICATE_GROUP DG inner join TB_ITEM_DUPLICATES_TEMP dt 
----		on DG.ORIGINAL_ITEM_ID = dt.ITEM_ID
----		AND EXTR_UI = @UI
----		AND DESTINATION is null
----		AND _key_in = _key_out
----		INNER JOIN TB_ITEM_REVIEW IR  on IR.ITEM_ID = DG.ORIGINAL_ITEM_ID and IR.REVIEW_ID = @revID

----	--update TB_ITEM_DUPLICATE_GROUP to set the MASTER_MEMBER_ID correctly (now that we have a line in TB_ITEM_DUPLICATE_GROUP_MEMBERS)
----	UPDATE TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = a.GMI
----	FROM (
----		SELECT GROUP_MEMBER_ID GMI, IR.ITEM_ID from TB_ITEM_DUPLICATE_GROUP idg
----			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on gm.ITEM_DUPLICATE_GROUP_ID = idg.ITEM_DUPLICATE_GROUP_ID and MASTER_MEMBER_ID is null
----			inner join TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID AND IR.REVIEW_ID = @revID
----			inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
----			AND EXTR_UI = @UI AND dt._key_in = dt._key_out and dt.DESTINATION is null
----	) a  
----	WHERE ORIGINAL_ITEM_ID = a.ITEM_ID and MASTER_MEMBER_ID is null

	
----	-- add the newly created group IDs to temporary table
----	UPDATE TB_ITEM_DUPLICATES_TEMP set DESTINATION = a.DGI
----	FROM (
----		SELECT ITEM_DUPLICATE_GROUP_ID DGI, dt.ITEM_ID MAST, dt1.ITEM_ID CURR_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS gm
----			inner JOIN TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
----			inner JOIN TB_ITEM_DUPLICATES_TEMP dt on dt.ITEM_ID = IR.ITEM_ID
----			AND EXTR_UI = @UI AND dt._key_in = dt._key_out 
----			inner JOIN TB_ITEM_DUPLICATES_TEMP dt1 on dt._key_in = dt1._key_out and dt.DESTINATION is null
----			and dt1.EXTR_UI = @UI --!!!!!!!!!!!!!!
----	) a
----	where a.CURR_ID = TB_ITEM_DUPLICATES_TEMP.ITEM_ID

----	-- add non master members that are not currently present
----	--INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
----	--(
----	--	ITEM_DUPLICATE_GROUP_ID
----	--	,ITEM_REVIEW_ID
----	--	,SCORE
----	--	,IS_CHECKED
----	--	,IS_DUPLICATE
----	--)
----	--SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
----	--from TB_ITEM_DUPLICATES_TEMP DT
----	--inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
----	--WHERE DT.ITEM_ID not in 
----	--(
----	--	SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
----	--	inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
----	--	inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
----	--	on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
----	--)
----	declare  @t table (goodIDs bigint)
----		insert into @t select distinct item_id from TB_ITEM_DUPLICATES_TEMP where EXTR_UI = @UI --and _key_in != _key_out 
----		select COUNT (goodids) from @t
----		delete from @t where goodIDs in (
----			SELECT dt1.ITEM_ID from TB_ITEM_DUPLICATES_TEMP dt1
----			inner join TB_ITEM_REVIEW ir1 on dt1.ITEM_ID = ir1.ITEM_ID and ir1.REVIEW_ID = @revID
----			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm 
----			on dt1.DESTINATION = gm.ITEM_DUPLICATE_GROUP_ID and ir1.ITEM_REVIEW_ID = gm.ITEM_REVIEW_ID
----			)
----		select COUNT (goodids) from @t
		
----		-- add non master members that are not currently present
----		INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
----		(
----			ITEM_DUPLICATE_GROUP_ID
----			,ITEM_REVIEW_ID
----			,SCORE
----			,IS_CHECKED
----			,IS_DUPLICATE
----		)
----		SELECT DESTINATION, ITEM_REVIEW_ID, _SCORE, 0, 0
----		from TB_ITEM_DUPLICATES_TEMP DT
----		inner join @t t on DT.ITEM_ID = t.goodIDs
----		inner join TB_ITEM_REVIEW IR on DT.ITEM_ID = IR.ITEM_ID and IR.REVIEW_ID = @revID
----	--remove temporary results.
----	delete FROM TB_ITEM_DUPLICATES_TEMP WHERE REVIEW_ID = @revID
----	END
----GO


------USE [Reviewer]
------GO
------/****** Object:  StoredProcedure [dbo].[st_ComparisonStats]    Script Date: 12/14/2015 10:37:37 ******/
------SET ANSI_NULLS ON
------GO
------SET QUOTED_IDENTIFIER ON
------GO
------ALTER procedure [dbo].[st_ComparisonStats]
------(
------	@COMPARISON_ID INT,
------	@Is_Screening bit OUTPUT
------)
--------with recompile
------As

------SET NOCOUNT ON

------set @Is_Screening = (Select IS_SCREENING from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)

------declare @n1 int , @n2 int , @n3 int --Total N items coded reviewer 1,2 & 3 (snapshot)
------declare @c1 int = (select CONTACT_ID1 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
------declare @c2 int = (select CONTACT_ID2 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
------declare @c3 int = (select CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
------declare @set int = (select SET_ID from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
------DECLARE @T1 TABLE
------	(
------	  ITEM_ID BIGINT,
------	  ATTRIBUTE_ID BIGINT,
------	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
------	)
------DECLARE @T1c TABLE --current attributes Reviewer1
------	(
------	  ITEM_ID BIGINT,
------	  ATTRIBUTE_ID BIGINT,
------	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
------	)
------DECLARE @T2 TABLE
------	(
------	  ITEM_ID BIGINT,
------	  ATTRIBUTE_ID BIGINT,
------	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
------	)
------DECLARE @T2c TABLE --current attributes Reviewer2
------	(
------	  ITEM_ID BIGINT,
------	  ATTRIBUTE_ID BIGINT,
------	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
------	)

------DECLARE @T3 TABLE
------	(
------	  ITEM_ID BIGINT,
------	  ATTRIBUTE_ID BIGINT,
------	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
------	)
------DECLARE @T3c TABLE --current attributes Reviewer3
------	(
------	  ITEM_ID BIGINT,
------	  ATTRIBUTE_ID BIGINT,
------	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
------	)
------DECLARE @T1a2 table (ITEM_ID bigint primary key)--snapshot agreements 1 v 2
------DECLARE @T1ca2 table (ITEM_ID bigint primary key)--current agreements 1 v 2
------DECLARE @T1a3 table (ITEM_ID bigint primary key)--snapshot agreements 1 v 3
------DECLARE @T1ca3 table (ITEM_ID bigint primary key)--current agreements 1 v 3
------DECLARE @T2a3 table (ITEM_ID bigint primary key)--snapshot agreements 2 v 3
------DECLARE @T2ca3 table (ITEM_ID bigint primary key)--current agreements 2 v 3
------insert into @T1
------select ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
------inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID and c.CONTACT_ID1 = cia.CONTACT_ID and c.COMPARISON_ID = @COMPARISON_ID
------set @n1 = ( select count(distinct(item_id)) from @T1 )

------insert into @T1c 
------select t1.ITEM_ID, tia.ATTRIBUTE_ID from (select distinct ITEM_ID from @T1) t1 --only items in the comparison
------	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @set and tis.CONTACT_ID = @c1
------	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID

------insert into @T2
------select ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
------inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID and c.CONTACT_ID2 = cia.CONTACT_ID and c.COMPARISON_ID = @COMPARISON_ID
------set @n2 = ( select count(distinct(item_id)) from @T2 )

------insert into @T2c 
------select t2.ITEM_ID, tia.ATTRIBUTE_ID from (select distinct ITEM_ID from @T2) t2 
------	inner join TB_ITEM_SET tis on t2.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @set and tis.CONTACT_ID = @c2
------	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID

------insert into @T3
------select ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
------inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID and c.CONTACT_ID3 = cia.CONTACT_ID and c.COMPARISON_ID = @COMPARISON_ID
------set @n3 = ( select count(distinct(item_id)) from @T3 )

------insert into @T3c 
------select t3.ITEM_ID, tia.ATTRIBUTE_ID from (select distinct ITEM_ID from @T3) t3 
------	inner join TB_ITEM_SET tis on t3.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @set and tis.CONTACT_ID = @c3
------	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID


-------- Total N items coded reviewer 1
------select @n1 as 'Total N items coded reviewer 1'
-------- Total N items coded reviewer 2
------select @n2 as 'Total N items coded reviewer 2'
-------- Total N items coded reviewer 3
------select @n3 as 'Total N items coded reviewer 3'

-------- Total N items coded reviewer 1 & 2
------select count(distinct(t1.item_id)) as 'Total N items coded reviewer 1 & 2' from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID

-------- Total disagreements 1vs2
--------the inner join (IJ) selects only records in t2 where the second reviewer has applied a different code, but this does not guarantee coding as a whole is different, we still need to check if:
--------a)	R1 has also coded with the same attribute found in t2 through IJ. If that’s the case, then we should not count this as a disagreement.
--------b)	R2 has also coded with the attribute from t1
--------The second outer join (OJ2), with the “where t2b.ATTRIBUTE_ID is null” clause checks for a). The first outer join (OJ1), with the “where t1b.ATTRIBUTE_ID is null” clause, checks  for b).
--------So overall, the first join spots all possible 1:1 coding differences, the two outer joins get rid of meaningless lines (where the differences are cancelled by other records). 
------select count(distinct(t1.item_id)) as 'Total disagreements 1vs2'
------	from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
------	left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
------	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
------	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

-------- Total N items coded reviewer 2 & 3
------select count(distinct(t2.item_id)) as 'Total N items coded reviewer 2 & 3' from @T2 t2 inner join @T3 t3 on t2.ITEM_ID = t3.ITEM_ID

-------- Total disagreements 2vs3
------select count(distinct(t2.item_id)) as 'Total disagreements 2vs3'
------	from @T2 t2 inner join @T3 t3 on t2.ITEM_ID = t3.ITEM_ID and t2.ATTRIBUTE_ID != t3.ATTRIBUTE_ID
------	left outer join @T3 t3b on t2.ATTRIBUTE_ID = t3b.ATTRIBUTE_ID and t2.ITEM_ID = t3b.ITEM_ID
------	left outer join @T2 t2b on  t2b.ATTRIBUTE_ID = t3.ATTRIBUTE_ID and t2b.ITEM_ID = t3.ITEM_ID
------	where t2b.ATTRIBUTE_ID is null or t3b.ATTRIBUTE_ID is null

-------- Total N items coded reviewer 1 & 3
------select count(distinct(t1.item_id)) as 'Total N items coded reviewer 1 & 3' from @T1 t1 inner join @T3 t3 on t1.ITEM_ID = t3.ITEM_ID

-------- Total disagreements 1vs3
------select count(distinct(t1.item_id)) as 'Total disagreements 1vs3'
------	from @T1 t1 inner join @T3 t3 on t1.ITEM_ID = t3.ITEM_ID and t1.ATTRIBUTE_ID != t3.ATTRIBUTE_ID
------	left outer join @T3 t3b on t1.ATTRIBUTE_ID = t3b.ATTRIBUTE_ID and t1.ITEM_ID = t3b.ITEM_ID
------	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t3.ATTRIBUTE_ID and t1b.ITEM_ID = t3.ITEM_ID
------	where t1b.ATTRIBUTE_ID is null or t3b.ATTRIBUTE_ID is null

--------REAL AGREEMENTS: Combine items from R1 and R2 and get only those that are not currenlty disagreements
------insert into @T1ca2
------Select t1.item_id from @T1c t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
------	inner join @T2c t2 on t1.ITEM_ID = t2.ITEM_ID
------	except
------	select distinct(t1.item_id) from @T1c t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
------											-- then the double outer joins as before
------	inner join @T2c t2 on t1.ITEM_ID = t2.ITEM_ID
------				and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
------	left outer join @T1c tia1a on tia1a.ITEM_ID = t1.ITEM_ID and t2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
------	left outer join @T2c tia2a on tia2a.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
------	where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null


--------insert into @T1ca2
--------Select t1.item_id from @T1 t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
--------	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.CONTACT_ID = @c1 and tis.SET_ID = @set
--------	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID --need to join all the way to TB_ITEM_ATTRIBUTE to be 100% that some codes are present
--------	inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID
--------	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c2 and tis2.SET_ID = @set
--------	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID	
--------	except
------	--select * from @T1 t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
------	--										-- then the double outer joins as before
------	--inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.CONTACT_ID = @c1 and tis1.SET_ID = @set
------	--inner join TB_ITEM_ATTRIBUTE tia1 on tis1.ITEM_SET_ID = tia1.ITEM_SET_ID 
------	--inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID 
------	--inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c2 and tis2.SET_ID = @set
------	--inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID 
------	--			and tia1.ATTRIBUTE_ID != tia2.ATTRIBUTE_ID
------	--left outer join TB_ITEM_ATTRIBUTE tia1a on tis1.ITEM_SET_ID = tia1a.ITEM_SET_ID and tia1a.ITEM_ID = tis1.ITEM_ID and tia2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
------	--left outer join TB_ITEM_ATTRIBUTE tia2a on tis2.ITEM_SET_ID = tia2a.ITEM_SET_ID and tia2a.ITEM_ID = tis2.ITEM_ID and tia1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
------	--where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null

--------COMPARISON AGREEMENTS: 1 V 2, uses the same techniques as before to find the list of all agreed-items according to the comparison snapshot
------insert into @T1a2
------Select distinct t1.ITEM_ID from @T1 t1 
------	inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID
------	except
------select distinct(t1.item_id) from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
------	left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
------	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
------	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

--------REAL AGREEMENTS: Combine items from R1 and R3 and get only those that are not currenlty disagreements
------insert into @T1ca3
------Select t1.item_id from @T1c t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
------	inner join @T3c t3 on t1.ITEM_ID = t3.ITEM_ID
------	except
------	select distinct(t1.item_id) from @T1c t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
------											-- then the double outer joins as before
------	inner join @T3c t3 on t1.ITEM_ID = t3.ITEM_ID
------				and t1.ATTRIBUTE_ID != t3.ATTRIBUTE_ID
------	left outer join @T1c tia1a on tia1a.ITEM_ID = t1.ITEM_ID and t3.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
------	left outer join @T3c tia3a on tia3a.ITEM_ID = t3.ITEM_ID and t1.ATTRIBUTE_ID = tia3a.ATTRIBUTE_ID
------	where tia1a.ATTRIBUTE_ID is null or tia3a.ATTRIBUTE_ID is null


----------REAL AGREEMENTS: Combine items from R1 and R3 and get only those that are not currenlty disagreements
--------insert into @T1ca3
--------Select t1.item_id from @T1 t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
--------	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.CONTACT_ID = @c1 and tis.SET_ID = @set
--------	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID --need to join all the way to TB_ITEM_ATTRIBUTE to be 100% that some codes are present
--------	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
--------	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--------	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID	
--------	except
--------	select distinct(t1.item_id) from @T1 t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
--------											-- then the double outer joins as before
--------	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.CONTACT_ID = @c1 and tis1.SET_ID = @set
--------	inner join TB_ITEM_ATTRIBUTE tia1 on tis1.ITEM_SET_ID = tia1.ITEM_SET_ID 
--------	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID 
--------	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--------	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID 
--------				and tia1.ATTRIBUTE_ID != tia2.ATTRIBUTE_ID
--------	left outer join TB_ITEM_ATTRIBUTE tia1a on tis1.ITEM_SET_ID = tia1a.ITEM_SET_ID and tia2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
--------	left outer join TB_ITEM_ATTRIBUTE tia2a on tis2.ITEM_SET_ID = tia2a.ITEM_SET_ID and tia1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
--------	where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null

--------COMPARISON AGREEMENTS: 1 V 3, uses the same techniques as before to find the list of all agreed-items according to the comparison snapshot
------insert into @T1a3
------Select distinct t1.ITEM_ID from @T1 t1 
------	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
------	except
------select distinct(t1.item_id) from @T1 t1 inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
------	left outer join @T3 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
------	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
------	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

--------REAL AGREEMENTS: Combine items from R2 and R3 and get only those that are not currenlty disagreements
------insert into @T2ca3
------Select t2.item_id from @T2c t2 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
------	inner join @T3c t3 on t2.ITEM_ID = t3.ITEM_ID
------	except
------	select distinct(t2.item_id) from @T2c t2 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
------											-- then the double outer joins as before
------	inner join @T3c t3 on t2.ITEM_ID = t3.ITEM_ID
------				and t2.ATTRIBUTE_ID != t3.ATTRIBUTE_ID
------	left outer join @T2c tia2a on tia2a.ITEM_ID = t2.ITEM_ID and t3.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
------	left outer join @T3c tia3a on tia3a.ITEM_ID = t3.ITEM_ID and t2.ATTRIBUTE_ID = tia3a.ATTRIBUTE_ID
------	where tia2a.ATTRIBUTE_ID is null or tia3a.ATTRIBUTE_ID is null
	
----------REAL AGREEMENTS: Combine items from R2 and R3 and get only those that are not currenlty disagreements
--------insert into @T2ca3
--------Select t1.item_id from @T2 t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
--------	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.CONTACT_ID = @c2 and tis.SET_ID = @set
--------	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID --need to join all the way to TB_ITEM_ATTRIBUTE to be 100% that some codes are present
--------	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
--------	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--------	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID	
--------	except
--------	select distinct(t1.item_id) from @T2 t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
--------											-- then the double outer joins as before
--------	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.CONTACT_ID = @c2 and tis1.SET_ID = @set
--------	inner join TB_ITEM_ATTRIBUTE tia1 on tis1.ITEM_SET_ID = tia1.ITEM_SET_ID 
--------	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID 
--------	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--------	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID 
--------				and tia1.ATTRIBUTE_ID != tia2.ATTRIBUTE_ID
--------	left outer join TB_ITEM_ATTRIBUTE tia1a on tis1.ITEM_SET_ID = tia1a.ITEM_SET_ID and tia2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
--------	left outer join TB_ITEM_ATTRIBUTE tia2a on tis2.ITEM_SET_ID = tia2a.ITEM_SET_ID and tia1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
--------	where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null

--------COMPARISON AGREEMENTS: 2 V 3, uses the same techniques as before to find the list of all agreed-items according to the comparison snapshot
------insert into @T2a3
------Select distinct t1.ITEM_ID from @T2 t1 
------	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
------	except
------select distinct(t1.item_id) from @T2 t1 inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
------	left outer join @T3 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
------	left outer join @T2 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
------	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null


-------- Are all Comparison Agreements currently completed OR are current agreements different from the snapshot agreements?
-------- 1 V 2
------Select Case when (COUNT(distinct ITEM_ID) = SUM(Completed)
------				  OR 
------					( select 
------							Case when (SUM(sm.ss) > 0) then 1 --
------							else 0
------							end
------						 from
------						(
------						Select COUNT(t1.ITEM_ID) ss from @T1a2 t1 
------							left join @T1ca2 t2 on t1.ITEM_ID = t2.ITEM_ID
------							where t2.ITEM_ID is null
------						UNION
------						Select COUNT(t1.ITEM_ID) ss from @T1ca2 t1 
------							left join @T1a2 t2 on t1.ITEM_ID = t2.ITEM_ID
------							where t2.ITEM_ID is null
------						) AS sm
------					) = 1
------				  ) then 1 else 0 end 
------				  as '1v2 lock-completion OR changed'
------	from 
------	(Select distinct t1.ITEM_ID, Case
------								when (tis1.IS_COMPLETED = 1 ) then 1
------								else 0
------							end as Completed
------	from @T1a2 t1 
------	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.SET_ID = @set --joining on item and set, not on CONTACT_ID as item could be completed by anyone
------	) a

-------- Are all Comparison Agreements currently completed OR are current agreements different from the snapshot agreements?
-------- 1 V 3
------Select Case when (COUNT(distinct ITEM_ID) = SUM(Completed)
------				  OR 
------					( select 
------							Case when (SUM(sm.ss) > 0) then 1 --
------							else 0
------							end
------						 from
------						(
------						Select COUNT(t1.ITEM_ID) ss from @T1a3 t1 
------							left join @T1ca3 t2 on t1.ITEM_ID = t2.ITEM_ID
------							where t2.ITEM_ID is null
------						UNION
------						Select COUNT(t1.ITEM_ID) ss from @T1ca3 t1 
------							left join @T1a3 t2 on t1.ITEM_ID = t2.ITEM_ID
------							where t2.ITEM_ID is null
------						) AS sm
------					) = 1
------				  ) then 1 else 0 end 
------				  as '1v3 lock-completion OR changed'
------	from 
------	(Select distinct t1.ITEM_ID, Case
------								when (tis1.IS_COMPLETED = 1 ) then 1
------								else 0
------							end as Completed
------	from @T1a3 t1 
------	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.SET_ID = @set --joining on item and set, not on CONTACT_ID as item could be completed by anyone
------	) a

-------- Are all Comparison Agreements currently completed OR are current agreements different from the snapshot agreements?
-------- 2 V 3
------Select Case when (COUNT(distinct ITEM_ID) = SUM(Completed)
------				  OR 
------					( select 
------							Case when (SUM(sm.ss) > 0) then 1 --
------							else 0
------							end
------						 from
------						(
------						Select COUNT(t1.ITEM_ID) ss from @T2a3 t1 
------							left join @T2ca3 t2 on t1.ITEM_ID = t2.ITEM_ID
------							where t2.ITEM_ID is null
------						UNION
------						Select COUNT(t1.ITEM_ID) ss from @T2ca3 t1 
------							left join @T2a3 t2 on t1.ITEM_ID = t2.ITEM_ID
------							where t2.ITEM_ID is null
------						) AS sm
------					) = 1
------				  ) then 1 else 0 end 
------				  as '2v3 lock-completion OR changed'
------	from 
------	(Select distinct t1.ITEM_ID, Case
------								when (tis1.IS_COMPLETED = 1 ) then 1
------								else 0
------							end as Completed
------	from @T2a3 t1 
------	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.SET_ID = @set --joining on item and set, not on CONTACT_ID as item could be completed by anyone
------	) a

------GO



--------USE [Reviewer]
--------GO
--------/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--------BEGIN TRANSACTION
--------SET QUOTED_IDENTIFIER ON
--------SET ARITHABORT ON
--------SET NUMERIC_ROUNDABORT OFF
--------SET CONCAT_NULL_YIELDS_NULL ON
--------SET ANSI_NULLS ON
--------SET ANSI_PADDING ON
--------SET ANSI_WARNINGS ON
--------COMMIT
--------BEGIN TRANSACTION
--------GO
--------CREATE NONCLUSTERED INDEX IX_TB_ATTRIBUTE_PARENT_ID_ATTRIBUTE_ID ON dbo.TB_ATTRIBUTE_SET
--------	(
--------	PARENT_ATTRIBUTE_ID,
--------	ATTRIBUTE_ID,
--------	ATTRIBUTE_SET_ID
--------	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
--------GO
--------ALTER TABLE dbo.TB_ATTRIBUTE_SET SET (LOCK_ESCALATION = TABLE)
--------GO
--------COMMIT
--------GO

--------USE [Reviewer]
--------GO
--------/****** Object:  StoredProcedure [dbo].[st_OutcomeList]    Script Date: 10/29/2015 11:15:00 ******/
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
--------	SELECT distinct tio.OUTCOME_ID, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
--------	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, SHORT_TITLE, OUTCOME_DESCRIPTION,
--------	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
--------	TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_OUTCOME_ID, tis.ITEM_SET_ID, TB_ITEM_ATTRIBUTE.ITEM_ID,
--------	A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT, A3.ATTRIBUTE_NAME OUTCOME_TEXT
--------	FROM TB_ITEM_OUTCOME tio
--------	inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tio.ITEM_SET_ID
--------		AND tis.IS_COMPLETED = 1
--------	inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @REVIEW_ID and rs.SET_ID = tis.SET_ID
--------	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tis.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--------	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tis.ITEM_SET_ID
--------	inner join TB_ITEM on TB_ITEM.ITEM_ID = tis.ITEM_ID
--------	LEFT OUTER JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
--------		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID
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


------------Interim version that was never published but took a long time to write, selects only one Answer for questions columns, published version will get them all and parse on c#
----------USE [Reviewer]
----------GO
----------/****** Object:  StoredProcedure [dbo].[st_OutcomeList]    Script Date: 10/21/2015 14:32:55 ******/
----------SET ANSI_NULLS ON
----------GO
----------SET QUOTED_IDENTIFIER ON
----------GO
----------ALTER procedure [dbo].[st_OutcomeList]
----------(
----------	@REVIEW_ID INT,
----------	@META_ANALYSIS_ID INT,
----------	@QUESTIONS nvarchar(max) = '',
----------	@ANSWERS nvarchar(max) = ''
----------	/*@SET_ID BIGINT,
----------	@ITEM_ATTRIBUTE_ID_INTERVENTION BIGINT = NULL,
----------	@ITEM_ATTRIBUTE_ID_CONTROL BIGINT = NULL,
----------	@ITEM_ATTRIBUTE_ID_OUTCOME BIGINT = NULL,
----------	@ATTRIBUTE_ID BIGINT = NULL,
	
	
----------	@VARIABLES NVARCHAR(MAX) = NULL,
----------	@ANSWERS NVARCHAR(MAX) = '',
----------	@QUESTIONS NVARCHAR(MAX) = ''*/
----------)

----------As

----------SET NOCOUNT ON
----------	SELECT distinct tio.OUTCOME_ID, tio.ITEM_SET_ID, OUTCOME_TYPE_ID, ITEM_ATTRIBUTE_ID_INTERVENTION,
----------	ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, OUTCOME_TITLE, SHORT_TITLE, OUTCOME_DESCRIPTION,
----------	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14,
----------	TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_OUTCOME_ID, tis.ITEM_SET_ID, TB_ITEM_ATTRIBUTE.ITEM_ID,
----------	A1.ATTRIBUTE_NAME INTERVENTION_TEXT, A2.ATTRIBUTE_NAME CONTROL_TEXT, A3.ATTRIBUTE_NAME OUTCOME_TEXT
----------	FROM TB_ITEM_OUTCOME tio
----------	inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tio.ITEM_SET_ID
----------		AND tis.IS_COMPLETED = 1
----------	inner join TB_REVIEW_SET rs on rs.REVIEW_ID = @REVIEW_ID and rs.SET_ID = tis.SET_ID
----------	inner join TB_ITEM_REVIEW on tb_item_review.ITEM_ID = tis.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
----------	inner JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_SET_ID = tis.ITEM_SET_ID
----------	inner join TB_ITEM on TB_ITEM.ITEM_ID = tis.ITEM_ID
----------	LEFT OUTER JOIN TB_META_ANALYSIS_OUTCOME ON TB_META_ANALYSIS_OUTCOME.OUTCOME_ID = tio.OUTCOME_ID
----------		AND TB_META_ANALYSIS_OUTCOME.META_ANALYSIS_ID = @META_ANALYSIS_ID
----------	left outer JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_INTERVENTION 
----------	left outer JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_CONTROL
----------	left outer JOIN TB_ATTRIBUTE A3 ON A3.ATTRIBUTE_ID = tio.ITEM_ATTRIBUTE_ID_OUTCOME

----------	--second sets of results, the answers
----------	--we need to get these, even if empty, so that we always get a reader
	
----------	IF (@QUESTIONS is not null AND @QUESTIONS != '')
----------	BEGIN
----------		declare @QT table ( AttID bigint primary key)
----------		insert into @QT select qss.value from dbo.fn_Split_int(@QUESTIONS, ',') as qss
		
----------		select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
----------			(Select top(1) a.ATTRIBUTE_NAME from 
----------			TB_ATTRIBUTE a 
----------			inner join TB_ITEM_ATTRIBUTE tia on a.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
----------			inner join TB_ITEM_SET tis1 on tia.ITEM_SET_ID = tis1.ITEM_SET_ID and tis1.IS_COMPLETED = 1 and tis.ITEM_ID = tis1.ITEM_ID
----------			inner join TB_REVIEW_SET rs1 on tis1.SET_ID = rs1.SET_ID and rs1.REVIEW_ID = @REVIEW_ID
----------			where tas.ATTRIBUTE_ID = a.ATTRIBUTE_ID
----------			order by tas.ATTRIBUTE_ORDER ) as Codename
----------			, tas.ATTRIBUTE_ID, tas.PARENT_ATTRIBUTE_ID
----------		from TB_ITEM_OUTCOME tio 
----------		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
----------		inner join TB_REVIEW_SET rs on tis.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
----------		inner join TB_ITEM_SET tis2 on tis2.ITEM_ID = tis.ITEM_ID and tis2.IS_COMPLETED = 1
----------		inner join TB_ATTRIBUTE_SET tas on tas.SET_ID = tis2.SET_ID 
----------		inner join @QT on  tas.PARENT_ATTRIBUTE_ID = AttID
----------		where (Select top(1) a.ATTRIBUTE_NAME from 
----------			TB_ATTRIBUTE a 
----------			inner join TB_ITEM_ATTRIBUTE tia on a.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
----------			inner join TB_ITEM_SET tis1 on tia.ITEM_SET_ID = tis1.ITEM_SET_ID and tis1.IS_COMPLETED = 1 and tis.ITEM_ID = tis1.ITEM_ID
----------			inner join TB_REVIEW_SET rs1 on tis1.SET_ID = rs1.SET_ID and rs1.REVIEW_ID = @REVIEW_ID
----------			where tas.ATTRIBUTE_ID = a.ATTRIBUTE_ID
----------			order by tas.ATTRIBUTE_ORDER ) is not null
----------		order by OUTCOME_ID
----------	END
----------	ELSE
----------	BEGIN
----------	select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
----------		tio.OUTCOME_TITLE as Codename
----------		, tio.ITEM_ATTRIBUTE_ID_CONTROL as ATTRIBUTE_ID, tio.ITEM_ATTRIBUTE_ID_CONTROL as PARENT_ATTRIBUTE_ID
----------		from TB_ITEM_OUTCOME tio
----------		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and 1=0
----------	END


----------	IF (@ANSWERS is not null AND @ANSWERS != '')
----------	BEGIN
----------	--third set of results, the questions
----------	declare @AT table ( AttID bigint primary key)
----------	insert into @AT select qss.value from dbo.fn_Split_int(@ANSWERS, ',') as qss
----------	select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
----------		--(Select top(1) a.ATTRIBUTE_NAME from @AT 
----------		--inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = AttID
----------		--inner join TB_ATTRIBUTE a on tas.ATTRIBUTE_ID = a.ATTRIBUTE_ID
----------		--inner join TB_ITEM_ATTRIBUTE tia on a.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
----------		--inner join TB_ITEM_SET tis1 on tia.ITEM_SET_ID = tis1.ITEM_SET_ID and tis1.IS_COMPLETED = 1 and tis.ITEM_ID = tis1.ITEM_ID
----------		--inner join TB_REVIEW_SET rs1 on tis1.SET_ID = rs1.SET_ID and rs1.REVIEW_ID = @REVIEW_ID
----------		--order by tas.ATTRIBUTE_ORDER ) as Codename
----------		tia.ATTRIBUTE_ID, a.ATTRIBUTE_NAME
----------	from TB_ITEM_OUTCOME tio 
----------	inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1
----------	inner join TB_REVIEW_SET rs on tis.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID
----------	inner join TB_ITEM_SET tis2 on tis.ITEM_ID = tis2.ITEM_ID and tis2.IS_COMPLETED = 1
----------	inner join TB_ATTRIBUTE_SET tas on tis2.SET_ID = tas.SET_ID
----------	inner join TB_ITEM_ATTRIBUTE tia on tis2.ITEM_ID = tia.ITEM_ID and tas.ATTRIBUTE_ID = tia.ATTRIBUTE_ID
----------	inner join @AT on AttID = tia.ATTRIBUTE_ID
----------	inner join TB_ATTRIBUTE a on tia.ATTRIBUTE_ID = a.ATTRIBUTE_ID
----------	order by OUTCOME_ID
----------	END
----------	ELSE
----------	BEGIN
----------		select tio.OUTCOME_ID, tio.OUTCOME_TITLE, tis.ITEM_ID, 
----------		tio.ITEM_ATTRIBUTE_ID_CONTROL as ATTRIBUTE_ID, tio.OUTCOME_TITLE as ATTRIBUTE_NAME
----------	from TB_ITEM_OUTCOME tio inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and 1=0
	
----------	END
	



----------SET NOCOUNT OFF
----------GO



----------USE [Reviewer]
----------GO
----------/****** Object:  StoredProcedure [dbo].[st_ItemList]    Script Date: 09/15/2015 10:18:35 ******/
----------SET ANSI_NULLS ON
----------GO
----------SET QUOTED_IDENTIFIER ON
----------GO
----------ALTER procedure [dbo].[st_ItemList]
----------(
----------      @REVIEW_ID INT,
----------      @SHOW_INCLUDED BIT = 'true',
----------      @SHOW_DELETED BIT = 'false',
----------      @SOURCE_ID INT = 0,
----------      @ATTRIBUTE_SET_ID_LIST NVARCHAR(MAX) = '',
      
----------      @PageNum INT = 1,
----------      @PerPage INT = 3,
----------      @CurrentPage INT OUTPUT,
----------      @TotalPages INT OUTPUT,
----------      @TotalRows INT OUTPUT 
----------)

----------As

----------SET NOCOUNT ON

----------declare @RowsToRetrieve int
----------Declare @ID table (ItemID bigint primary key )
----------IF (@SOURCE_ID = 0) AND (@ATTRIBUTE_SET_ID_LIST = '') /* LIST ALL ITEMS IN THE REVIEW */
----------BEGIN

----------       --store IDs to build paged results as a simple join
----------	  INSERT INTO @ID SELECT DISTINCT (I.ITEM_ID)
----------      FROM TB_ITEM I
----------      INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
----------            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
----------            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
----------            AND TB_ITEM_REVIEW.IS_DELETED = @SHOW_DELETED

----------	  SELECT @TotalRows = @@ROWCOUNT

----------      set @TotalPages = @TotalRows/@PerPage

----------      if @PageNum < 1
----------      set @PageNum = 1

----------      if @TotalRows % @PerPage != 0
----------      set @TotalPages = @TotalPages + 1

----------      set @RowsToRetrieve = @PerPage * @PageNum
----------      set @CurrentPage = @PageNum;

----------      WITH SearchResults AS
----------      (
----------      SELECT DISTINCT (ir.ITEM_ID), IS_DELETED, IS_INCLUDED, ir.MASTER_ITEM_ID,
----------            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
----------      FROM TB_ITEM i
----------			INNER JOIN TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID
----------			INNER JOIN @ID id on id.ItemID = ir.ITEM_ID
            
----------      )
----------      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
----------                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
----------                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
----------                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
----------                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
----------                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
----------            FROM SearchResults
----------                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
----------                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
----------            WHERE RowNum > @RowsToRetrieve - @PerPage
----------            AND RowNum <= @RowsToRetrieve
----------            ORDER BY SHORT_TITLE
----------END
----------ELSE /* FILTER BY A LIST OF ATTRIBUTES */

----------IF (@ATTRIBUTE_SET_ID_LIST != '')
----------BEGIN
----------      SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
----------            FROM TB_ITEM_REVIEW I
----------      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID
----------      INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
----------      INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
----------      INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----------      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!

----------      WHERE I.IS_INCLUDED = @SHOW_INCLUDED
----------            AND I.IS_DELETED = @SHOW_DELETED
----------            AND I.REVIEW_ID = @REVIEW_ID

----------      set @TotalPages = @TotalRows/@PerPage

----------      if @PageNum < 1
----------      set @PageNum = 1

----------      if @TotalRows % @PerPage != 0
----------      set @TotalPages = @TotalPages + 1

----------      set @RowsToRetrieve = @PerPage * @PageNum
----------      set @CurrentPage = @PageNum;

----------      WITH SearchResults AS
----------      (
----------      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID, 
----------            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
----------      FROM TB_ITEM I
----------       INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
----------            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
----------            AND TB_ITEM_REVIEW.IS_INCLUDED = @SHOW_INCLUDED
----------            AND TB_ITEM_REVIEW.IS_DELETED = @SHOW_DELETED

----------      INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = I.ITEM_ID INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
----------      INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID -- Make sure the correct set is being used - the same code can appear in more than one set!
----------      )
----------      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
----------                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
----------                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
----------                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
----------                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
----------                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
----------            FROM SearchResults
----------                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
----------                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
----------            WHERE RowNum > @RowsToRetrieve - @PerPage
----------            AND RowNum <= @RowsToRetrieve 
----------            ORDER BY SHORT_TITLE
----------END
----------ELSE -- LISTING SOURCELESS
----------IF (@SOURCE_ID = -1)
----------BEGIN
----------       --store IDs to build paged results as a simple join
----------	  INSERT INTO @ID SELECT DISTINCT IR.ITEM_ID
----------		from TB_ITEM_REVIEW IR 
----------      LEFT OUTER JOIN TB_ITEM_SOURCE TIS on IR.ITEM_ID = TIS.ITEM_ID
----------      LEFT OUTER JOIN TB_SOURCE TS on TIS.SOURCE_ID = TS.SOURCE_ID and IR.REVIEW_ID = TS.REVIEW_ID
----------      where IR.REVIEW_ID = @REVIEW_ID and TS.SOURCE_ID  is null

----------	  SELECT @TotalRows = @@ROWCOUNT
----------      set @TotalPages = @TotalRows/@PerPage

----------      if @PageNum < 1
----------      set @PageNum = 1

----------      if @TotalRows % @PerPage != 0
----------      set @TotalPages = @TotalPages + 1

----------      set @RowsToRetrieve = @PerPage * @PageNum
----------      set @CurrentPage = @PageNum;

----------      WITH SearchResults AS
----------      (
----------      SELECT DISTINCT (I.ITEM_ID), IR.IS_DELETED, IS_INCLUDED, IR.MASTER_ITEM_ID,
----------            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
----------      FROM TB_ITEM I
----------      INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] 
----------      INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = I.ITEM_ID AND IR.REVIEW_ID = @REVIEW_ID
----------      INNER JOIN @ID id on id.ItemID = I.ITEM_ID
----------      )
----------      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
----------                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
----------                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
----------                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
----------                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
----------                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
----------            FROM SearchResults
----------                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
----------                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
----------            WHERE RowNum > @RowsToRetrieve - @PerPage
----------            AND RowNum <= @RowsToRetrieve 
----------            ORDER BY SHORT_TITLE
      
----------END
----------ELSE -- LISTING BY A SOURCE
----------BEGIN
----------      SELECT @TotalRows = count(I.ITEM_ID)
----------      FROM TB_ITEM_REVIEW I
----------      INNER JOIN TB_ITEM_SOURCE ON TB_ITEM_SOURCE.ITEM_ID = I.ITEM_ID AND TB_ITEM_SOURCE.SOURCE_ID = @SOURCE_ID
----------      WHERE I.REVIEW_ID = @REVIEW_ID

----------      set @TotalPages = @TotalRows/@PerPage

----------      if @PageNum < 1
----------      set @PageNum = 1

----------      if @TotalRows % @PerPage != 0
----------      set @TotalPages = @TotalPages + 1

----------      set @RowsToRetrieve = @PerPage * @PageNum
----------      set @CurrentPage = @PageNum;

----------      WITH SearchResults AS
----------      (
----------      SELECT DISTINCT (I.ITEM_ID), IS_DELETED, IS_INCLUDED, TB_ITEM_REVIEW.MASTER_ITEM_ID,
----------            ROW_NUMBER() OVER(order by SHORT_TITLE) RowNum
----------      FROM TB_ITEM I
----------      INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID] INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
----------            TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
----------      INNER JOIN TB_ITEM_SOURCE ON TB_ITEM_SOURCE.ITEM_ID = I.ITEM_ID AND TB_ITEM_SOURCE.SOURCE_ID = @SOURCE_ID
----------      )
----------      Select SearchResults.ITEM_ID, II.[TYPE_ID], OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 0) as AUTHORS,
----------                  TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
----------                  [YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
----------                  AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(II.ITEM_ID, 1) as PARENTAUTHORS
----------                  , SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
----------                  --, ROW_NUMBER() OVER(order by authors, TITLE) RowNum
----------            FROM SearchResults
----------                  INNER JOIN TB_ITEM II ON SearchResults.ITEM_ID = II.ITEM_ID
----------                  INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = II.[TYPE_ID]
----------            WHERE RowNum > @RowsToRetrieve - @PerPage
----------            AND RowNum <= @RowsToRetrieve
----------            ORDER BY SHORT_TITLE
      
----------END

----------SELECT      @CurrentPage as N'@CurrentPage',
----------            @TotalPages as N'@TotalPages',
----------            @TotalRows as N'@TotalRows'


----------SET NOCOUNT OFF
----------GO

----------USE [Reviewer]
----------GO
----------/****** Object:  StoredProcedure [dbo].[st_ReviewStatisticsCodeSetsReviewersIncomplete]    Script Date: 09/14/2015 10:35:59 ******/
----------SET ANSI_NULLS ON
----------GO
----------SET QUOTED_IDENTIFIER ON
----------GO


----------ALTER procedure [dbo].[st_ReviewStatisticsCodeSetsReviewersIncomplete]
----------(
----------	@REVIEW_ID INT,
----------	@SET_ID INT 
----------)
----------with Recompile
----------As

----------SET NOCOUNT ON

----------SELECT SET_NAME, IS1.SET_ID, IS1.CONTACT_ID, CONTACT_NAME, COUNT(DISTINCT IS1.ITEM_ID) AS TOTAL
----------FROM TB_ITEM_SET IS1
----------INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = IS1.SET_ID
----------INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.REVIEW_ID = TB_REVIEW_SET.REVIEW_ID AND TB_ITEM_REVIEW.ITEM_ID = IS1.ITEM_ID
----------INNER JOIN TB_SET ON TB_SET.SET_ID = IS1.SET_ID
----------INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = IS1.CONTACT_ID
----------WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND IS1.IS_COMPLETED = 'FALSE' 
----------AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND IS1.SET_ID = @SET_ID
----------AND NOT IS1.ITEM_ID IN
----------(
----------	SELECT IS2.ITEM_ID
----------	FROM TB_ITEM_SET IS2
----------	INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = IS2.ITEM_ID AND IR2.REVIEW_ID = @REVIEW_ID
----------	WHERE IS2.IS_COMPLETED = 'TRUE' AND IS2.SET_ID = IS1.SET_ID
----------)
----------GROUP BY IS1.SET_ID, SET_NAME, IS1.CONTACT_ID, CONTACT_NAME


----------SET NOCOUNT OFF
----------GO



------------USE [ReviewerAdmin]
------------GO

------------/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Check_UserIsLogged]    Script Date: 03/10/2015 17:21:31 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_LogonTicket_Check_UserIsLogged]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_LogonTicket_Check_UserIsLogged]
------------GO

------------/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Check_UserIsLogged]    Script Date: 03/10/2015 17:21:31 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

-------------- =============================================
-------------- Author:		<Author,,Name>
-------------- Create date: <Create Date,,>
-------------- Description:	<Description,,>
-------------- =============================================
------------CREATE PROCEDURE [dbo].[st_LogonTicket_Check_UserIsLogged]
------------	-- Add the parameters for the stored procedure here
------------	@c_ID int
------------	,@RES int output --1 if user is logged
------------AS
------------BEGIN
------------	-- SET NOCOUNT ON added to prevent extra result sets from
------------	-- interfering with SELECT statements.
------------	SET NOCOUNT ON;

------------    -- Insert statements for procedure here
------------	SELECT @RES = COUNT(CONTACT_ID) from TB_LOGON_TICKET 
------------		where @c_ID = CONTACT_ID 
------------		and LAST_RENEWED > DATEADD(HH, -3, GETDATE())
------------		and STATE = 1
	
------------END

------------GO

------------/****** Object:  StoredProcedure [dbo].[st_ContactUndoCreate]    Script Date: 03/17/2015 11:04:55 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactUndoCreate]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_ContactUndoCreate]
------------GO

------------/****** Object:  StoredProcedure [dbo].[st_ContactUndoCreate]    Script Date: 03/17/2015 11:04:55 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

-------------- =============================================
-------------- Author:		<Author,,Name>
-------------- Create date: <Create Date,,>
-------------- Description:	<Description,,>
-------------- =============================================
------------CREATE PROCEDURE [dbo].[st_ContactUndoCreate]
------------	-- Add the parameters for the stored procedure here
------------	@CID int
------------	,@USERNAME nvarchar(50)
------------AS
------------BEGIN
------------	-- SET NOCOUNT ON added to prevent extra result sets from
------------	-- interfering with SELECT statements.
------------	SET NOCOUNT ON;

------------    -- Insert statements for procedure here
------------	Delete from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CID and [USERNAME] = @USERNAME
------------END

------------GO





------------Use [Reviewer]
------------GO

------------/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
------------BEGIN TRANSACTION
------------SET QUOTED_IDENTIFIER ON
------------SET ARITHABORT ON
------------SET NUMERIC_ROUNDABORT OFF
------------SET CONCAT_NULL_YIELDS_NULL ON
------------SET ANSI_NULLS ON
------------SET ANSI_PADDING ON
------------SET ANSI_WARNINGS ON
------------COMMIT
------------BEGIN TRANSACTION
------------GO
------------ALTER TABLE dbo.TB_CONTACT ADD
------------	ARCHIE_ID varchar(32) NULL,
------------	ARCHIE_ACCESS_TOKEN varchar(64) NULL,
------------	ARCHIE_TOKEN_VALID_UNTIL datetime2(1) NULL,
------------	ARCHIE_REFRESH_TOKEN varchar(64) NULL,
------------	LAST_ARCHIE_CODE varchar(64) NULL,
------------	LAST_ARCHIE_STATE varchar(10) NULL
------------GO
------------ALTER TABLE dbo.TB_CONTACT SET (LOCK_ESCALATION = TABLE)
------------GO
------------COMMIT
------------GO


--------------BEGIN TRANSACTION
--------------GO
--------------CREATE UNIQUE NONCLUSTERED INDEX [AK_TB_CONTACT_ARCHIE_ID]
--------------ON [TB_CONTACT] (ARCHIE_ID)
--------------WHERE [ARCHIE_ID] IS NOT NULL
--------------GO
--------------ALTER TABLE dbo.TB_CONTACT SET (LOCK_ESCALATION = TABLE)
--------------GO
--------------COMMIT
--------------GO


------------/****** Object:  Table [dbo].[TB_UNASSIGNED_ARCHIE_KEYS]    Script Date: 03/06/2015 14:42:44 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_UNASSIGNED_ARCHIE_KEYS]') AND type in (N'U'))
------------DROP TABLE [dbo].[TB_UNASSIGNED_ARCHIE_KEYS]
------------GO



------------/****** Object:  Table [dbo].[TB_UNASSIGNED_ARCHIE_KEYS]    Script Date: 03/06/2015 14:42:44 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

------------SET ANSI_PADDING ON
------------GO

------------CREATE TABLE [dbo].[TB_UNASSIGNED_ARCHIE_KEYS](
------------	[ARCHIE_ID] [varchar](32) NOT NULL,
------------	[ARCHIE_ACCESS_TOKEN] [varchar](64) NOT NULL,
------------	[ARCHIE_TOKEN_VALID_UNTIL] [datetime2](1) NOT NULL,
------------	[ARCHIE_REFRESH_TOKEN] [varchar](64) NOT NULL,
------------	[LAST_ARCHIE_CODE] [varchar](64) NOT NULL,
------------	[LAST_ARCHIE_STATE] [varchar](10) NOT NULL,
------------ CONSTRAINT [PK_TB_UNASSIGNED_ARCHIE_KEYS] PRIMARY KEY CLUSTERED 
------------(
------------	[ARCHIE_ID] ASC
------------)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
------------) ON [PRIMARY]

------------GO

------------SET ANSI_PADDING OFF
------------GO

------------/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
------------BEGIN TRANSACTION
------------SET QUOTED_IDENTIFIER ON
------------SET ARITHABORT ON
------------SET NUMERIC_ROUNDABORT OFF
------------SET CONCAT_NULL_YIELDS_NULL ON
------------SET ANSI_NULLS ON
------------SET ANSI_PADDING ON
------------SET ANSI_WARNINGS ON
------------COMMIT
------------BEGIN TRANSACTION
------------GO
------------ALTER TABLE dbo.TB_REVIEW ADD
------------	ARCHIE_ID char(18) NULL,
------------	ARCHIE_CD nchar(8) NULL,
------------	IS_CHECKEDOUT_HERE bit NULL,
------------	CHECKED_OUT_BY int NULL
------------GO
------------ALTER TABLE dbo.TB_REVIEW SET (LOCK_ESCALATION = TABLE)
------------GO
------------COMMIT




------------/****** Object:  StoredProcedure [dbo].[st_ContactLogin]    Script Date: 03/06/2015 15:10:11 ******/
------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------ALTER procedure [dbo].[st_ContactLogin]
------------(
------------	@userName  varchar(50)	
------------	,@Password varchar(50)
	
------------)
--------------note the GRACE_EXP field, how many days we add to EXPIRY_DATE defines how long is the grace period for the whole of ER4.
--------------during the grace period users can log on ER4 but will have read only access.
------------As


--------------first check if the username/pw are correct
------------declare @chek int = (select count(Contact_id)  from TB_CONTACT c where c.USERNAME = @userName and HASHBYTES('SHA1', @Password + FLAVOUR) = PWASHED AND c.EXPIRY_DATE is not null)
------------if @chek = 1
------------BEGIN
------------	--second make sure old now stale ArchieStatus and ArchieCode are discharged, no matter what (this is a first logon via ER4 credentials)
------------	UPDATE TB_CONTACT SET  LAST_ARCHIE_CODE = null, LAST_ARCHIE_STATE = null
------------		where USERNAME = @userName and HASHBYTES('SHA1', @Password + FLAVOUR) = PWASHED AND EXPIRY_DATE is not null
------------	--third get some user info
------------	Select c.CONTACT_ID, c.contact_name, --c.Password, 
------------		DATEADD(m, 2, 
------------				( CASE when sl.[EXPIRY_DATE] is not null 
------------					and sl.[EXPIRY_DATE] > c.[EXPIRY_DATE]
------------						then sl.[EXPIRY_DATE]
------------					else c.[EXPIRY_DATE]
------------					end
------------				)) as GRACE_EXP,
------------		[TYPE], IS_SITE_ADMIN
------------		, CASE when c.ARCHIE_ID is null then 0
------------			ELSE 1
------------			END
------------		AS IS_COCHRANE_USER
------------		  /* TB_CONTACT.[Role] */
------------	From TB_CONTACT c
------------	Left outer join TB_SITE_LIC_CONTACT lc on lc.CONTACT_ID = c.CONTACT_ID
------------	Left outer join TB_SITE_LIC sl on lc.SITE_LIC_ID = sl.SITE_LIC_ID
------------	Where c.UserName = @userName and c.EXPIRY_DATE is not null
------------END
------------GO

------------/****** Object:  StoredProcedure [dbo].[st_ArchieIdentityFromReviewer]    Script Date: 03/09/2015 15:26:22 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ArchieIdentityFromReviewer]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_ArchieIdentityFromReviewer]
------------GO


------------/****** Object:  StoredProcedure [dbo].[st_ArchieIdentityFromReviewer]    Script Date: 03/09/2015 15:26:22 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

-------------- =============================================
-------------- Author:		<Author,,Name>
-------------- Create date: <Create Date,,>
-------------- Description:	<Description,,>
-------------- =============================================
------------CREATE PROCEDURE [dbo].[st_ArchieIdentityFromReviewer]
------------	-- Add the parameters for the stored procedure here
------------	@CID int
------------AS
------------BEGIN
------------	-- SET NOCOUNT ON added to prevent extra result sets from
------------	-- interfering with SELECT statements.
------------	SET NOCOUNT ON;

------------    -- Insert statements for procedure here
------------	SELECT ARCHIE_ID, ARCHIE_ACCESS_TOKEN, ARCHIE_TOKEN_VALID_UNTIL, ARCHIE_REFRESH_TOKEN from TB_CONTACT where CONTACT_ID = @CID
------------END

------------GO



------------/****** Object:  StoredProcedure [dbo].[st_ArchieSaveTokens]    Script Date: 03/09/2015 15:30:49 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ArchieSaveTokens]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_ArchieSaveTokens]
------------GO



------------/****** Object:  StoredProcedure [dbo].[st_ArchieSaveTokens]    Script Date: 03/09/2015 15:30:49 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

-------------- =============================================
-------------- Author:		<Author,,Name>
-------------- Create date: <Create Date,,>
-------------- Description:	<Description,,>
-------------- =============================================
------------CREATE PROCEDURE [dbo].[st_ArchieSaveTokens]
------------	-- Add the parameters for the stored procedure here
------------	@ARCHIE_ID varchar(32)
------------	,@TOKEN varchar(64)
------------	,@VALID_UNTIL datetime2(1)
------------	,@REFRESH_T varchar(64)
------------	,@ARCHIE_CODE varchar(64) = null
------------	,@ARCHIE_STATE varchar(10) = null
------------AS
------------BEGIN
------------	-- SET NOCOUNT ON added to prevent extra result sets from
------------	-- interfering with SELECT statements.
------------	SET NOCOUNT ON;

------------    -- Insert statements for procedure here
	
------------	Update TB_CONTACT set 
------------		ARCHIE_ACCESS_TOKEN = @TOKEN
------------		,ARCHIE_TOKEN_VALID_UNTIL = @VALID_UNTIL
------------		,ARCHIE_REFRESH_TOKEN = @REFRESH_T
------------		,LAST_ARCHIE_CODE = @ARCHIE_CODE
------------		,LAST_ARCHIE_STATE = @ARCHIE_STATE
------------		where ARCHIE_ID = @ARCHIE_ID
	
------------END

------------GO


------------/****** Object:  StoredProcedure [dbo].[st_ArchieFindER4UserFromArchieID]    Script Date: 03/09/2015 16:16:27 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ArchieFindER4UserFromArchieID]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_ArchieFindER4UserFromArchieID]
------------GO

------------/****** Object:  StoredProcedure [dbo].[st_ArchieFindER4UserFromArchieID]    Script Date: 03/09/2015 16:16:27 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

-------------- =============================================
-------------- Author:		<Author,,Name>
-------------- Create date: <Create Date,,>
-------------- Description:	<Description,,>
-------------- =============================================
------------CREATE PROCEDURE [dbo].[st_ArchieFindER4UserFromArchieID]
------------	-- Add the parameters for the stored procedure here
------------	@ARCHIE_ID varchar(32)
------------AS
------------BEGIN
------------	-- SET NOCOUNT ON added to prevent extra result sets from
------------	-- interfering with SELECT statements.
------------	SET NOCOUNT ON;

------------    -- Insert statements for procedure here
------------	--first, check that we have one and only one result
------------	declare @ck int = (Select COUNT(CONTACT_ID) from TB_CONTACT where ARCHIE_ID = @ARCHIE_ID and ARCHIE_ID is not null)
------------	if @ck = 1
------------	BEGIN
------------		select * from TB_CONTACT where  ARCHIE_ID = @ARCHIE_ID and ARCHIE_ID is not null
------------	END
------------END

------------GO


------------/****** Object:  StoredProcedure [dbo].[st_ArchieSaveUnassignedTokens]    Script Date: 03/09/2015 17:09:51 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ArchieSaveUnassignedTokens]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_ArchieSaveUnassignedTokens]
------------GO


------------/****** Object:  StoredProcedure [dbo].[st_ArchieSaveUnassignedTokens]    Script Date: 03/09/2015 17:09:51 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

-------------- =============================================
-------------- Author:		<Author,,Name>
-------------- Create date: <Create Date,,>
-------------- Description:	<Description,,>
-------------- =============================================
------------CREATE PROCEDURE [dbo].[st_ArchieSaveUnassignedTokens]
------------	-- Add the parameters for the stored procedure here
------------	@ARCHIE_ID varchar(32)
------------	,@TOKEN varchar(64)
------------	,@VALID_UNTIL datetime2(1)
------------	,@REFRESH_T varchar(64)
------------	,@ARCHIE_CODE varchar(64)
------------	,@ARCHIE_STATE varchar(10)
------------AS
------------BEGIN
------------	-- SET NOCOUNT ON added to prevent extra result sets from
------------	-- interfering with SELECT statements.
------------	SET NOCOUNT ON;

------------    -- Insert statements for procedure here
------------	--first, make sure there are no duplicates
------------	delete from TB_UNASSIGNED_ARCHIE_KEYS where ARCHIE_ID = @ARCHIE_ID
------------	INSERT INTO [Reviewer].[dbo].[TB_UNASSIGNED_ARCHIE_KEYS]
------------           ([ARCHIE_ID]
------------           ,[ARCHIE_ACCESS_TOKEN]
------------           ,[ARCHIE_TOKEN_VALID_UNTIL]
------------           ,[ARCHIE_REFRESH_TOKEN]
------------           ,[LAST_ARCHIE_CODE]
------------           ,[LAST_ARCHIE_STATE])
------------     VALUES
------------           (@ARCHIE_ID
------------           ,@TOKEN
------------           ,@VALID_UNTIL
------------           ,@REFRESH_T
------------           ,@ARCHIE_CODE
------------           ,@ARCHIE_STATE)
	
------------END


------------GO



------------/****** Object:  StoredProcedure [dbo].[st_ArchieIdentityFromCodeAndStatus]    Script Date: 03/10/2015 11:39:32 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ArchieIdentityFromCodeAndStatus]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_ArchieIdentityFromCodeAndStatus]
------------GO


------------/****** Object:  StoredProcedure [dbo].[st_ArchieIdentityFromCodeAndStatus]    Script Date: 03/10/2015 11:39:32 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

-------------- =============================================
-------------- Author:		<Author,,Name>
-------------- Create date: <Create Date,,>
-------------- Description:	<Description,,>
-------------- =============================================
------------CREATE PROCEDURE [dbo].[st_ArchieIdentityFromCodeAndStatus]
------------	-- Add the parameters for the stored procedure here
------------	@ARCHIE_CODE varchar(64)
------------	,@ARCHIE_STATE varchar(10)
------------AS
------------BEGIN
------------	-- SET NOCOUNT ON added to prevent extra result sets from
------------	-- interfering with SELECT statements.
------------	SET NOCOUNT ON;

------------    -- Insert statements for procedure here
------------    declare @ck int = (Select COUNT(CONTACT_ID) from TB_CONTACT where @ARCHIE_CODE = LAST_ARCHIE_CODE and @ARCHIE_STATE = LAST_ARCHIE_STATE 
------------		and (LAST_ARCHIE_CODE is not null and LAST_ARCHIE_STATE is not null))
------------	if @ck = 1
------------	BEGIN
------------	SELECT * from TB_CONTACT where @ARCHIE_CODE = LAST_ARCHIE_CODE and @ARCHIE_STATE = LAST_ARCHIE_STATE 
------------		and (LAST_ARCHIE_CODE is not null and LAST_ARCHIE_STATE is not null)
------------	END
------------END

------------GO



------------/****** Object:  StoredProcedure [dbo].[st_ArchieIdentityFromUnassignedCodeAndStatus]    Script Date: 03/12/2015 16:32:38 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ArchieIdentityFromUnassignedCodeAndStatus]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_ArchieIdentityFromUnassignedCodeAndStatus]
------------GO



------------/****** Object:  StoredProcedure [dbo].[st_ArchieIdentityFromUnassignedCodeAndStatus]    Script Date: 03/12/2015 16:32:38 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

------------CREATE PROCEDURE [dbo].[st_ArchieIdentityFromUnassignedCodeAndStatus]
------------	-- Add the parameters for the stored procedure here
------------	@CID int
------------	,@ARCHIE_CODE varchar(64)
------------	,@ARCHIE_STATE varchar(10)
------------AS
------------BEGIN
------------	-- SET NOCOUNT ON added to prevent extra result sets from
------------	-- interfering with SELECT statements.
------------	SET NOCOUNT ON;

------------    -- Insert statements for procedure here
------------    declare @ck int = (
------------						Select COUNT(u.ARCHIE_ID) from TB_UNASSIGNED_ARCHIE_KEYS u
------------						inner join TB_CONTACT c	
------------							on c.CONTACT_ID = @CID and c.ARCHIE_ID is null
------------						where @ARCHIE_CODE = u.LAST_ARCHIE_CODE and @ARCHIE_STATE = u.LAST_ARCHIE_STATE 
------------						and (u.LAST_ARCHIE_CODE is not null and u.LAST_ARCHIE_STATE is not null)
------------						)
------------	if @ck = 1
------------	BEGIN --all is well
------------		--1. Save Archie keys in tb_contact
------------		update TB_CONTACT set 
------------			ARCHIE_ID = au.ARCHIE_ID
------------			,ARCHIE_ACCESS_TOKEN = au.ARCHIE_ACCESS_TOKEN
------------			,ARCHIE_TOKEN_VALID_UNTIL = au.ARCHIE_TOKEN_VALID_UNTIL
------------			,ARCHIE_REFRESH_TOKEN = au.ARCHIE_REFRESH_TOKEN
------------			,LAST_ARCHIE_CODE = au.LAST_ARCHIE_CODE
------------			,LAST_ARCHIE_STATE = au.LAST_ARCHIE_STATE
------------		From (
------------				Select * from TB_UNASSIGNED_ARCHIE_KEYS u where @ARCHIE_CODE = u.LAST_ARCHIE_CODE 
------------				and @ARCHIE_STATE = u.LAST_ARCHIE_STATE 
------------				and (u.LAST_ARCHIE_CODE is not null and u.LAST_ARCHIE_STATE is not null)
------------				) au
------------		WHERE CONTACT_ID = @CID
------------		--2. delete record from TB_UNASSIGNED_ARCHIE_KEYS
------------		delete from TB_UNASSIGNED_ARCHIE_KEYS where @ARCHIE_CODE = LAST_ARCHIE_CODE 
------------				and @ARCHIE_STATE = LAST_ARCHIE_STATE 
------------				and (LAST_ARCHIE_CODE is not null and LAST_ARCHIE_STATE is not null)
------------		--3. get All user details
------------		SELECT * from TB_CONTACT where @ARCHIE_CODE = LAST_ARCHIE_CODE and @ARCHIE_STATE = LAST_ARCHIE_STATE 
------------			and (LAST_ARCHIE_CODE is not null and LAST_ARCHIE_STATE is not null)
------------	END
------------END

------------GO


------------/****** Object:  StoredProcedure [dbo].[st_ArchieReviewFindFromArchieID]    Script Date: 03/20/2015 14:40:30 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ArchieReviewFindFromArchieID]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_ArchieReviewFindFromArchieID]
------------GO


------------/****** Object:  StoredProcedure [dbo].[st_ArchieReviewFindFromArchieID]    Script Date: 03/20/2015 14:40:30 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

-------------- =============================================
-------------- Author:		<Author,,Name>
-------------- Create date: <Create Date,,>
-------------- Description:	<Description,,>
-------------- =============================================
------------CREATE PROCEDURE [dbo].[st_ArchieReviewFindFromArchieID]
------------	-- Add the parameters for the stored procedure here
------------	@A_ID char(18)
------------	,@CID int
------------AS
------------BEGIN
------------	-- SET NOCOUNT ON added to prevent extra result sets from
------------	-- interfering with SELECT statements.
------------	SET NOCOUNT ON;

------------    -- Insert statements for procedure here
------------	declare @ck int = (select COUNT(Review_id) from TB_REVIEW where ARCHIE_ID = @A_ID and ARCHIE_ID is not null)
------------	IF @ck = 1
------------	BEGIN
------------		select r.* 
------------		 ,CASE 
------------			when rc.CONTACT_ID is not null then 1
------------			else 0
------------		END as CONTACT_IS_IN_REVIEW
------------		,dbo.fn_REBUILD_ROLES(rc.REVIEW_CONTACT_ID) as ROLES
------------		from TB_REVIEW r
------------		left outer join TB_REVIEW_CONTACT rc on r.REVIEW_ID = rc.REVIEW_ID and CONTACT_ID = @CID
------------		where ARCHIE_ID = @A_ID and ARCHIE_ID is not null
------------	END
------------END


------------GO



------------/****** Object:  StoredProcedure [dbo].[st_ArchieReviewLinkToER4Review]    Script Date: 04/01/2015 17:02:53 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ArchieReviewLinkToER4Review]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_ArchieReviewLinkToER4Review]
------------GO


------------/****** Object:  StoredProcedure [dbo].[st_ArchieReviewLinkToER4Review]    Script Date: 04/01/2015 17:02:53 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

-------------- =============================================
-------------- Author:		<Author,,Name>
-------------- Create date: <Create Date,,>
-------------- Description:	<Description,,>
-------------- =============================================
------------CREATE PROCEDURE [dbo].[st_ArchieReviewLinkToER4Review] 
------------	-- Add the parameters for the stored procedure here
------------	@RID int,
------------	@CID int,
------------	@ARID char(18),
------------	@ARCD char(8),
------------	@IS_CHECKEDOUT_HERE bit,
------------	@RES int out
------------AS
------------BEGIN
------------	-- SET NOCOUNT ON added to prevent extra result sets from
------------	-- interfering with SELECT statements.
------------	SET NOCOUNT ON;

------------    -- Insert statements for procedure here
------------	declare @ck int = (select COUNT(r.REVIEW_ID) from TB_REVIEW r 
------------						inner join TB_REVIEW_CONTACT rc on r.REVIEW_ID = rc.REVIEW_ID and rc.CONTACT_ID = @CID
------------						 where @RID = r.REVIEW_ID and (r.IS_CHECKEDOUT_HERE is null OR r.IS_CHECKEDOUT_HERE = 0)
------------						 )
------------	if @ck != 1
------------	BEGIN
------------		set @RES = -1
------------		return
------------	END
------------	UPDATE TB_REVIEW set ARCHIE_ID = @ARID, ARCHIE_CD = @ARCD, IS_CHECKEDOUT_HERE = @IS_CHECKEDOUT_HERE
------------	WHERE REVIEW_ID = @RID
------------	if @@ROWCOUNT = 1
------------		Set @RES = 1
------------	else
------------		Set @RES = -2
------------END

------------GO



------------/****** Object:  StoredProcedure [dbo].[st_ArchieReviewMarkAsCheckedInOut]    Script Date: 04/14/2015 17:00:27 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ArchieReviewMarkAsCheckedInOut]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_ArchieReviewMarkAsCheckedInOut]
------------GO


------------/****** Object:  StoredProcedure [dbo].[st_ArchieReviewMarkAsCheckedInOut]    Script Date: 04/14/2015 17:00:27 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

-------------- =============================================
-------------- Author:		<Author,,Name>
-------------- Create date: <Create Date,,>
-------------- Description:	<Description,,>
-------------- =============================================
------------CREATE PROCEDURE [dbo].[st_ArchieReviewMarkAsCheckedInOut] 
------------	-- Add the parameters for the stored procedure here
------------	@RID int,
------------	@CID int,
------------	@ARID char(18),
------------	@ARCD char(8),
------------	@IS_CHECKEDOUT_HERE bit,
------------	@RES int out
------------AS
------------BEGIN
------------	-- SET NOCOUNT ON added to prevent extra result sets from
------------	-- interfering with SELECT statements.
------------	SET NOCOUNT ON;

------------    -- Insert statements for procedure here
------------	declare @ck int = (select COUNT(r.REVIEW_ID) from TB_REVIEW r 
------------						inner join TB_REVIEW_CONTACT rc on r.REVIEW_ID = rc.REVIEW_ID and rc.CONTACT_ID = @CID
------------						 where @RID = r.REVIEW_ID and (@ARID = r.ARCHIE_ID or r.ARCHIE_ID is null)
------------						 )
------------	if @ck != 1
------------	BEGIN
------------		set @RES = -1
------------		return
------------	END
------------	UPDATE TB_REVIEW set ARCHIE_ID = @ARID, ARCHIE_CD = @ARCD, IS_CHECKEDOUT_HERE = @IS_CHECKEDOUT_HERE
------------	WHERE REVIEW_ID = @RID
------------	if @@ROWCOUNT = 1
------------	BEGIN
------------		Set @RES = 1
------------		if @IS_CHECKEDOUT_HERE = 0
------------		--IF we are marking the review as CHECKED-IN in Archie, we need to kick out currenlty logged on users
------------		--we do this by adding a second "Active" ticket to all currently logged on users
------------		-- this gets picked up by the client when ticket & status get checked
------------		--on client side, the code will receive the message 'Multiple' which never happens otherwise
------------		--if the user is Cochrane, they will get an explanation
------------		--otherwise they get a generic error.
------------		BEGIN
------------			DECLARE @T TABLE 
------------			( 
------------				CI int,
------------				TK uniqueidentifier
------------			) 
------------			insert into @T
------------				SELECT CONTACT_ID, newid()
------------				from ReviewerAdmin.dbo.TB_LOGON_TICKET
------------				WHERE REVIEW_ID = @RID and [STATE] = 1 and LAST_RENEWED > DATEADD(HH, -3, GETDATE())
			
------------			INSERT into ReviewerAdmin.dbo.TB_LOGON_TICKET(TICKET_GUID, CONTACT_ID, REVIEW_ID)
------------			SELECT TK, CI, @RID From @T 
------------		END
------------	END
------------	else
------------		Set @RES = -2
	
------------END


------------GO

------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------ALTER procedure [dbo].[st_ReviewContact]
------------(
------------	@CONTACT_ID INT
------------)

------------As

------------SELECT REVIEW_CONTACT_ID, rc.REVIEW_ID, rc.CONTACT_ID, REVIEW_NAME, dbo.fn_REBUILD_ROLES(rc.REVIEW_CONTACT_ID) as ROLES
------------	,own.CONTACT_NAME as 'OWNER', case when LR is null
------------									then r.DATE_CREATED
------------									else LR
------------								 end
------------								 as 'LAST_ACCESS'
------------	, r.SHOW_SCREENING, r.ALLOW_REVIEWER_TERMS, r.ALLOW_CLUSTERED_SEARCH, r.SCREENING_CODE_SET_ID
------------FROM TB_REVIEW_CONTACT rc
------------INNER JOIN TB_REVIEW r ON rc.REVIEW_ID = r.REVIEW_ID
------------inner join TB_CONTACT own on r.FUNDER_ID = own.CONTACT_ID
------------left join (
------------			select MAX(LAST_RENEWED) LR, REVIEW_ID
------------			from ReviewerAdmin.dbo.TB_LOGON_TICKET  
------------			where @CONTACT_ID = CONTACT_ID
------------			group by REVIEW_ID
------------			) as t
------------			on t.REVIEW_ID = r.REVIEW_ID
------------WHERE rc.CONTACT_ID = @CONTACT_ID and (r.ARCHIE_ID is null OR r.ARCHIE_ID = 'prospective_______')
------------ORDER BY REVIEW_NAME

------------GO

------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------ALTER procedure [dbo].[st_ContactLoginReview]
------------(
------------	@userId int,
------------	@reviewId int,
------------	@IsArchieUser bit,
------------	@GUI uniqueidentifier OUTPUT
------------)

------------As
------------if @IsArchieUser = 1
------------begin
--------------we need to make sure user has records in TB_REVIEW_CONTACT and TB_CONTACT_REVIEW_ROLE
------------	declare @ck int = (select count(crr.ROLE_NAME) from TB_REVIEW_CONTACT rc 
------------		inner join TB_CONTACT_REVIEW_ROLE crr on rc.REVIEW_ID = @reviewId and rc.CONTACT_ID = @userId and crr.REVIEW_CONTACT_ID = rc.REVIEW_CONTACT_ID
------------		)
------------	if @ck < 1
------------	BEGIN
------------		DECLARE @NEW_CONTACT_REVIEW_ID INT
------------		INSERT INTO TB_REVIEW_CONTACT(CONTACT_ID, REVIEW_ID)
------------		VALUES (@userId, @reviewId)
------------		SET @NEW_CONTACT_REVIEW_ID = @@IDENTITY
------------		INSERT INTO TB_CONTACT_REVIEW_ROLE(REVIEW_CONTACT_ID, ROLE_NAME)
------------		VALUES(@NEW_CONTACT_REVIEW_ID, 'RegularUser')
------------	END
------------end
------------SELECT TB_REVIEW.REVIEW_ID, TB_REVIEW.ARCHIE_ID, ROLE_NAME as [ROLE], 
------------		( CASE WHEN sl2.[EXPIRY_DATE] is not null
------------				and sl2.[EXPIRY_DATE] > TB_REVIEW.[EXPIRY_DATE]
------------					then sl2.[EXPIRY_DATE]
------------				else TB_REVIEW.[EXPIRY_DATE]
------------				end
------------		) as REVIEW_EXP, 
------------		( CASE when sl.[EXPIRY_DATE] is not null 
------------				and sl.[EXPIRY_DATE] > c.[EXPIRY_DATE]
------------					then sl.[EXPIRY_DATE]
------------				else c.[EXPIRY_DATE]
------------				end
------------		) as CONTACT_EXP,
------------		FUNDER_ID, tb_review.ARCHIE_ID, IS_CHECKEDOUT_HERE
		
------------FROM TB_REVIEW_CONTACT
------------	INNER JOIN TB_REVIEW on TB_REVIEW_CONTACT.REVIEW_ID = TB_REVIEW.REVIEW_ID
------------	INNER JOIN TB_CONTACT c on TB_REVIEW_CONTACT.CONTACT_ID = c.CONTACT_ID
------------	INNER JOIN TB_CONTACT_REVIEW_ROLE on TB_CONTACT_REVIEW_ROLE.REVIEW_CONTACT_ID = TB_REVIEW_CONTACT.REVIEW_CONTACT_ID
------------	Left outer join TB_SITE_LIC_CONTACT lc on lc.CONTACT_ID = c.CONTACT_ID
------------	Left outer join TB_SITE_LIC sl on lc.SITE_LIC_ID = sl.SITE_LIC_ID
------------	left outer join TB_SITE_LIC_REVIEW lr on TB_REVIEW.REVIEW_ID = lr.REVIEW_ID
------------	left outer join TB_SITE_LIC sl2 on lr.SITE_LIC_ID = sl2.SITE_LIC_ID
	
------------WHERE TB_REVIEW.REVIEW_ID = @reviewId AND c.CONTACT_ID = @userId

------------IF @@ROWCOUNT >= 1 
------------	BEGIN
------------	DECLARE	@return_value int,
------------			@GUID uniqueidentifier
			
------------	EXEC	@return_value = [ReviewerAdmin].[dbo].[st_LogonTicket_Insert]
------------			@Contact_ID = @userId,
------------			@Review_ID = @reviewId,
------------			@GUID = @GUI OUTPUT
------------	SELECT	@GUI as N'@GUID'
------------	END

------------GO

------------USE [ReviewerAdmin]
------------GO

------------/****** Object:  Table [dbo].[TB_TEMPLATE_REVIEW]    Script Date: 06/11/2015 10:22:28 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_TEMPLATE_REVIEW]') AND type in (N'U'))
------------DROP TABLE [dbo].[TB_TEMPLATE_REVIEW]
------------GO


------------/****** Object:  Table [dbo].[TB_TEMPLATE_REVIEW]    Script Date: 06/11/2015 10:22:28 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

------------CREATE TABLE [dbo].[TB_TEMPLATE_REVIEW](
------------	[TEMPLATE_REVIEW_ID] [int] NOT NULL
------------) ON [PRIMARY]

------------GO

------------/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
------------BEGIN TRANSACTION
------------SET QUOTED_IDENTIFIER ON
------------SET ARITHABORT ON
------------SET NUMERIC_ROUNDABORT OFF
------------SET CONCAT_NULL_YIELDS_NULL ON
------------SET ANSI_NULLS ON
------------SET ANSI_PADDING ON
------------SET ANSI_WARNINGS ON
------------COMMIT
------------BEGIN TRANSACTION
------------GO
------------CREATE TABLE dbo.Tmp_TB_TEMPLATE_REVIEW
------------	(
------------	TEMPLATE_REVIEW_ID int NOT NULL IDENTITY (1, 1),
------------	TEMPLATE_NAME nvarchar(1000) NOT NULL,
------------	TEMPLATE_DESCRIPTION nvarchar(2000) NOT NULL
------------	)  ON [PRIMARY]
------------GO
------------ALTER TABLE dbo.Tmp_TB_TEMPLATE_REVIEW SET (LOCK_ESCALATION = TABLE)
------------GO
------------SET IDENTITY_INSERT dbo.Tmp_TB_TEMPLATE_REVIEW ON
------------GO
------------IF EXISTS(SELECT * FROM dbo.TB_TEMPLATE_REVIEW)
------------	 EXEC('INSERT INTO dbo.Tmp_TB_TEMPLATE_REVIEW (TEMPLATE_REVIEW_ID)
------------		SELECT TEMPLATE_REVIEW_ID FROM dbo.TB_TEMPLATE_REVIEW WITH (HOLDLOCK TABLOCKX)')
------------GO
------------SET IDENTITY_INSERT dbo.Tmp_TB_TEMPLATE_REVIEW OFF
------------GO
------------DROP TABLE dbo.TB_TEMPLATE_REVIEW
------------GO
------------EXECUTE sp_rename N'dbo.Tmp_TB_TEMPLATE_REVIEW', N'TB_TEMPLATE_REVIEW', 'OBJECT' 
------------GO
------------ALTER TABLE dbo.TB_TEMPLATE_REVIEW ADD CONSTRAINT
------------	PK_TB_TEMPLATE_REVIEW PRIMARY KEY CLUSTERED 
------------	(
------------	TEMPLATE_REVIEW_ID
------------	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

------------GO
------------COMMIT
------------GO

------------/****** Object:  Table [dbo].[TB_TEMPLATE_REVIEW_SET]    Script Date: 06/11/2015 10:24:56 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_TEMPLATE_REVIEW_SET]') AND type in (N'U'))
------------DROP TABLE [dbo].[TB_TEMPLATE_REVIEW_SET]
------------GO


------------/****** Object:  Table [dbo].[TB_TEMPLATE_REVIEW_SET]    Script Date: 06/11/2015 10:24:56 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

------------CREATE TABLE [dbo].[TB_TEMPLATE_REVIEW_SET](
------------	[TEMPLATE_REVIEW_SET_ID] [int] NOT NULL
------------) ON [PRIMARY]

------------GO

------------/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
------------BEGIN TRANSACTION
------------SET QUOTED_IDENTIFIER ON
------------SET ARITHABORT ON
------------SET NUMERIC_ROUNDABORT OFF
------------SET CONCAT_NULL_YIELDS_NULL ON
------------SET ANSI_NULLS ON
------------SET ANSI_PADDING ON
------------SET ANSI_WARNINGS ON
------------COMMIT
------------BEGIN TRANSACTION
------------GO
------------ALTER TABLE dbo.TB_TEMPLATE_REVIEW SET (LOCK_ESCALATION = TABLE)
------------GO
------------COMMIT
------------BEGIN TRANSACTION
------------GO
------------CREATE TABLE dbo.Tmp_TB_TEMPLATE_REVIEW_SET
------------	(
------------	TEMPLATE_REVIEW_SET_ID int NOT NULL IDENTITY (1, 1),
------------	TEMPLATE_REVIEW_ID int NOT NULL,
------------	REVIEW_SET_ID int NOT NULL
------------	)  ON [PRIMARY]
------------GO
------------ALTER TABLE dbo.Tmp_TB_TEMPLATE_REVIEW_SET SET (LOCK_ESCALATION = TABLE)
------------GO
------------SET IDENTITY_INSERT dbo.Tmp_TB_TEMPLATE_REVIEW_SET ON
------------GO
------------IF EXISTS(SELECT * FROM dbo.TB_TEMPLATE_REVIEW_SET)
------------	 EXEC('INSERT INTO dbo.Tmp_TB_TEMPLATE_REVIEW_SET (TEMPLATE_REVIEW_SET_ID)
------------		SELECT TEMPLATE_REVIEW_SET_ID FROM dbo.TB_TEMPLATE_REVIEW_SET WITH (HOLDLOCK TABLOCKX)')
------------GO
------------SET IDENTITY_INSERT dbo.Tmp_TB_TEMPLATE_REVIEW_SET OFF
------------GO
------------DROP TABLE dbo.TB_TEMPLATE_REVIEW_SET
------------GO
------------EXECUTE sp_rename N'dbo.Tmp_TB_TEMPLATE_REVIEW_SET', N'TB_TEMPLATE_REVIEW_SET', 'OBJECT' 
------------GO
------------ALTER TABLE dbo.TB_TEMPLATE_REVIEW_SET ADD CONSTRAINT
------------	PK_TB_TEMPLATE_REVIEW_SET PRIMARY KEY CLUSTERED 
------------	(
------------	TEMPLATE_REVIEW_SET_ID
------------	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

------------GO
------------ALTER TABLE dbo.TB_TEMPLATE_REVIEW_SET ADD CONSTRAINT
------------	FK_TB_TEMPLATE_REVIEW_SET_TB_TEMPLATE_REVIEW FOREIGN KEY
------------	(
------------	TEMPLATE_REVIEW_ID
------------	) REFERENCES dbo.TB_TEMPLATE_REVIEW
------------	(
------------	TEMPLATE_REVIEW_ID
------------	) ON UPDATE  CASCADE 
------------	 ON DELETE  CASCADE 
	
------------GO
------------COMMIT
------------GO

------------Use [Reviewer]
------------GO

------------/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
------------BEGIN TRANSACTION
------------SET QUOTED_IDENTIFIER ON
------------SET ARITHABORT ON
------------SET NUMERIC_ROUNDABORT OFF
------------SET CONCAT_NULL_YIELDS_NULL ON
------------SET ANSI_NULLS ON
------------SET ANSI_PADDING ON
------------SET ANSI_WARNINGS ON
------------COMMIT
------------BEGIN TRANSACTION
------------GO
------------ALTER TABLE dbo.TB_SET
------------	DROP CONSTRAINT FK_TB_SET_TB_SET_TYPE
------------GO
------------ALTER TABLE dbo.TB_SET_TYPE SET (LOCK_ESCALATION = TABLE)
------------GO
------------COMMIT
------------BEGIN TRANSACTION
------------GO
------------CREATE TABLE dbo.Tmp_TB_SET
------------	(
------------	SET_ID int NOT NULL IDENTITY (1, 1),
------------	SET_TYPE_ID int NOT NULL,
------------	SET_NAME nvarchar(255) NULL,
------------	OLD_GUIDELINE_ID nvarchar(50) NULL,
------------	SET_DESCRIPTION nvarchar(2000) NULL,
------------	ORIGINAL_SET_ID int NULL
------------	)  ON [PRIMARY]
------------GO
------------ALTER TABLE dbo.Tmp_TB_SET SET (LOCK_ESCALATION = TABLE)
------------GO
------------SET IDENTITY_INSERT dbo.Tmp_TB_SET ON
------------GO
------------IF EXISTS(SELECT * FROM dbo.TB_SET)
------------	 EXEC('INSERT INTO dbo.Tmp_TB_SET (SET_ID, SET_TYPE_ID, SET_NAME, OLD_GUIDELINE_ID)
------------		SELECT SET_ID, SET_TYPE_ID, SET_NAME, OLD_GUIDELINE_ID FROM dbo.TB_SET WITH (HOLDLOCK TABLOCKX)')
------------GO
------------SET IDENTITY_INSERT dbo.Tmp_TB_SET OFF
------------GO
------------ALTER TABLE dbo.TB_WORK_ALLOCATION
------------	DROP CONSTRAINT FK_TB_WORK_ALLOCATION_TB_SET
------------GO
------------ALTER TABLE dbo.TB_REVIEW_SET
------------	DROP CONSTRAINT FK_TB_REVIEW_SET_TB_SETS
------------GO
------------ALTER TABLE dbo.TB_ITEM_SET
------------	DROP CONSTRAINT FK_TB_ITEM_SET_TB_SETS
------------GO
------------ALTER TABLE dbo.TB_ATTRIBUTE_SET
------------	DROP CONSTRAINT FK_TB_ATTRIBUTE_SET_TB_SETS
------------GO
------------DROP TABLE dbo.TB_SET
------------GO
------------EXECUTE sp_rename N'dbo.Tmp_TB_SET', N'TB_SET', 'OBJECT' 
------------GO
------------ALTER TABLE dbo.TB_SET ADD CONSTRAINT
------------	PK_TB_SETS PRIMARY KEY CLUSTERED 
------------	(
------------	SET_ID
------------	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

------------GO
------------ALTER TABLE dbo.TB_SET ADD CONSTRAINT
------------	FK_TB_SET_TB_SET_TYPE FOREIGN KEY
------------	(
------------	SET_TYPE_ID
------------	) REFERENCES dbo.TB_SET_TYPE
------------	(
------------	SET_TYPE_ID
------------	) ON UPDATE  NO ACTION 
------------	 ON DELETE  NO ACTION 
	
------------GO
------------COMMIT
------------BEGIN TRANSACTION
------------GO
------------ALTER TABLE dbo.TB_ATTRIBUTE_SET ADD CONSTRAINT
------------	FK_TB_ATTRIBUTE_SET_TB_SETS FOREIGN KEY
------------	(
------------	SET_ID
------------	) REFERENCES dbo.TB_SET
------------	(
------------	SET_ID
------------	) ON UPDATE  NO ACTION 
------------	 ON DELETE  NO ACTION 
	
------------GO
------------ALTER TABLE dbo.TB_ATTRIBUTE_SET SET (LOCK_ESCALATION = TABLE)
------------GO
------------COMMIT
------------BEGIN TRANSACTION
------------GO
------------ALTER TABLE dbo.TB_ITEM_SET ADD CONSTRAINT
------------	FK_TB_ITEM_SET_TB_SETS FOREIGN KEY
------------	(
------------	SET_ID
------------	) REFERENCES dbo.TB_SET
------------	(
------------	SET_ID
------------	) ON UPDATE  NO ACTION 
------------	 ON DELETE  NO ACTION 
	
------------GO
------------ALTER TABLE dbo.TB_ITEM_SET SET (LOCK_ESCALATION = TABLE)
------------GO
------------COMMIT
------------BEGIN TRANSACTION
------------GO
------------ALTER TABLE dbo.TB_REVIEW_SET ADD CONSTRAINT
------------	FK_TB_REVIEW_SET_TB_SETS FOREIGN KEY
------------	(
------------	SET_ID
------------	) REFERENCES dbo.TB_SET
------------	(
------------	SET_ID
------------	) ON UPDATE  NO ACTION 
------------	 ON DELETE  NO ACTION 
	
------------GO
------------ALTER TABLE dbo.TB_REVIEW_SET SET (LOCK_ESCALATION = TABLE)
------------GO
------------COMMIT
------------BEGIN TRANSACTION
------------GO
------------ALTER TABLE dbo.TB_WORK_ALLOCATION ADD CONSTRAINT
------------	FK_TB_WORK_ALLOCATION_TB_SET FOREIGN KEY
------------	(
------------	SET_ID
------------	) REFERENCES dbo.TB_SET
------------	(
------------	SET_ID
------------	) ON UPDATE  NO ACTION 
------------	 ON DELETE  NO ACTION 
	
------------GO
------------ALTER TABLE dbo.TB_WORK_ALLOCATION SET (LOCK_ESCALATION = TABLE)
------------GO
------------COMMIT
------------GO

------------/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
------------BEGIN TRANSACTION
------------SET QUOTED_IDENTIFIER ON
------------SET ARITHABORT ON
------------SET NUMERIC_ROUNDABORT OFF
------------SET CONCAT_NULL_YIELDS_NULL ON
------------SET ANSI_NULLS ON
------------SET ANSI_PADDING ON
------------SET ANSI_WARNINGS ON
------------COMMIT
------------BEGIN TRANSACTION
------------GO
------------ALTER TABLE dbo.TB_ATTRIBUTE ADD
------------	ORIGINAL_ATTRIBUTE_ID bigint NULL
------------GO
------------ALTER TABLE dbo.TB_ATTRIBUTE SET (LOCK_ESCALATION = TABLE)
------------GO
------------COMMIT
------------GO

------------------------------------changes in SPs
------------Use [Reviewer]
------------GO

------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------ALTER procedure [dbo].[st_ReviewSets]
------------(
------------	@REVIEW_ID INT
------------)

------------As

------------SET NOCOUNT ON

------------	SELECT REVIEW_SET_ID, REVIEW_ID, RS.SET_ID, ALLOW_CODING_EDITS, S.SET_TYPE_ID, SET_NAME, SET_TYPE, CODING_IS_FINAL, SET_ORDER, MAX_DEPTH, S.SET_DESCRIPTION, S.ORIGINAL_SET_ID
------------	FROM TB_REVIEW_SET RS
------------	INNER JOIN TB_SET S ON S.SET_ID = RS.SET_ID
------------	INNER JOIN TB_SET_TYPE ON TB_SET_TYPE.SET_TYPE_ID = S.SET_TYPE_ID

------------	WHERE RS.REVIEW_ID = @REVIEW_ID
------------	ORDER BY RS.SET_ORDER, RS.SET_ID


------------SET NOCOUNT OFF
------------GO


------------/****** Object:  StoredProcedure [dbo].[st_ReviewSetInsert]    Script Date: 06/11/2015 11:13:11 ******/
------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------ALTER procedure [dbo].[st_ReviewSetInsert]
------------(
------------	@REVIEW_ID INT,
------------	@SET_TYPE_ID INT = 3,
------------	@ALLOW_CODING_EDITS BIT = false,
------------	@SET_NAME NVARCHAR(255),
------------	@CODING_IS_FINAL BIT = true,
------------	@SET_ORDER INT = 0,
------------	@SET_DESCRIPTION nvarchar(2000) = '',
------------	@ORIGINAL_SET_ID int = null,
------------	@NEW_REVIEW_SET_ID INT OUTPUT,
------------	@NEW_SET_ID INT OUTPUT
------------)

------------As

------------SET NOCOUNT ON

------------	INSERT INTO TB_SET(SET_TYPE_ID, SET_NAME, SET_DESCRIPTION, ORIGINAL_SET_ID)
------------		VALUES(@SET_TYPE_ID, @SET_NAME, @SET_DESCRIPTION, @ORIGINAL_SET_ID)

------------	SET @NEW_SET_ID = @@IDENTITY

------------	INSERT INTO TB_REVIEW_SET(REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL, SET_ORDER)
------------		VALUES(@REVIEW_ID, @NEW_SET_ID, @ALLOW_CODING_EDITS, @CODING_IS_FINAL, @SET_ORDER)

------------	SET @NEW_REVIEW_SET_ID = @@IDENTITY


------------SET NOCOUNT OFF
------------GO

------------/****** Object:  StoredProcedure [dbo].[st_ReviewSetUpdate]    Script Date: 06/11/2015 11:28:40 ******/
------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------ALTER procedure [dbo].[st_ReviewSetUpdate]
------------(
------------	@REVIEW_SET_ID INT,
------------	@SET_ID INT,
------------	@ALLOW_CODING_EDITS BIT,
------------	@CODING_IS_FINAL BIT,
------------	@SET_NAME NVARCHAR(255),
------------	@SET_ORDER INT,
------------	@SET_DESCRIPTION nvarchar(2000),
------------	@ITEM_SET_ID BIGINT = NULL,
------------	@IS_COMPLETED BIT = NULL,
------------	@IS_LOCKED BIT = NULL
------------)

------------As

------------SET NOCOUNT ON

------------	UPDATE TB_SET SET SET_NAME = @SET_NAME, SET_DESCRIPTION = @SET_DESCRIPTION 
------------	 WHERE SET_ID = @SET_ID
------------	UPDATE TB_REVIEW_SET SET ALLOW_CODING_EDITS = @ALLOW_CODING_EDITS,
------------		CODING_IS_FINAL = @CODING_IS_FINAL,
------------		SET_ORDER = @SET_ORDER
------------	WHERE REVIEW_SET_ID = @REVIEW_SET_ID
	
------------	IF (@ITEM_SET_ID > 0)
------------	BEGIN
------------		UPDATE TB_ITEM_SET
------------		SET IS_COMPLETED = @IS_COMPLETED, IS_LOCKED = @IS_LOCKED
------------		WHERE ITEM_SET_ID = @ITEM_SET_ID
------------	END

------------SET NOCOUNT OFF
------------GO



------------/****** Object:  StoredProcedure [dbo].[st_TemplateReviewList]    Script Date: 06/11/2015 15:20:53 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_TemplateReviewList]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_TemplateReviewList]
------------GO


------------/****** Object:  StoredProcedure [dbo].[st_TemplateReviewList]    Script Date: 06/11/2015 15:20:53 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

-------------- =============================================
-------------- Author:		<Author,,Name>
-------------- Create date: <Create Date,,>
-------------- Description:	<Description,,>
-------------- =============================================
------------CREATE PROCEDURE [dbo].[st_TemplateReviewList]
------------	-- Add the parameters for the stored procedure here

------------AS
------------BEGIN
------------	-- SET NOCOUNT ON added to prevent extra result sets from
------------	-- interfering with SELECT statements.
------------	SET NOCOUNT ON;

------------    -- Insert statements for procedure here
------------	Select * from ReviewerAdmin.dbo.TB_TEMPLATE_REVIEW order by TEMPLATE_REVIEW_ID
	
------------	Select * from ReviewerAdmin.dbo.TB_TEMPLATE_REVIEW tr
------------		inner join ReviewerAdmin.dbo.TB_TEMPLATE_REVIEW_SET trs on tr.TEMPLATE_REVIEW_ID = trs.TEMPLATE_REVIEW_ID
------------		inner join TB_REVIEW_SET rs on trs.REVIEW_SET_ID = rs.REVIEW_SET_ID
------------		inner join TB_SET s on s.SET_ID = rs.SET_ID
------------		Order by tr.TEMPLATE_REVIEW_ID, TEMPLATE_REVIEW_SET_ID
------------END

------------GO


------------USE [Reviewer]
------------GO

------------/****** Object:  StoredProcedure [dbo].[st_ReviewSet]    Script Date: 06/16/2015 11:29:47 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ReviewSet]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_ReviewSet]
------------GO

------------USE [Reviewer]
------------GO

------------/****** Object:  StoredProcedure [dbo].[st_ReviewSet]    Script Date: 06/16/2015 11:29:47 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

-------------- =============================================
-------------- Author:		<Author,,Name>
-------------- Create date: <Create Date,,>
-------------- Description:	<Description,,>
-------------- =============================================
------------CREATE PROCEDURE [dbo].[st_ReviewSet]
------------	-- Add the parameters for the stored procedure here
------------	(
------------	@REVIEW_SET_ID INT
------------	)

------------As

------------SET NOCOUNT ON

------------	SELECT REVIEW_SET_ID, REVIEW_ID, RS.SET_ID, ALLOW_CODING_EDITS, S.SET_TYPE_ID, SET_NAME, SET_TYPE, CODING_IS_FINAL, SET_ORDER, MAX_DEPTH, S.SET_DESCRIPTION, S.ORIGINAL_SET_ID
------------	FROM TB_REVIEW_SET RS
------------	INNER JOIN TB_SET S ON S.SET_ID = RS.SET_ID
------------	INNER JOIN TB_SET_TYPE ON TB_SET_TYPE.SET_TYPE_ID = S.SET_TYPE_ID

------------	WHERE RS.REVIEW_SET_ID = @REVIEW_SET_ID
------------	ORDER BY RS.SET_ORDER, RS.SET_ID


------------SET NOCOUNT OFF

------------GO

------------USE [Reviewer]
------------GO
------------/****** Object:  StoredProcedure [dbo].[st_AttributeSetInsert]    Script Date: 06/16/2015 11:27:24 ******/
------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------ALTER procedure [dbo].[st_AttributeSetInsert]
------------(
------------	@SET_ID INT,
------------	@PARENT_ATTRIBUTE_ID BIGINT = 0,
------------	@ATTRIBUTE_TYPE_ID INT = 1,
------------	@ATTRIBUTE_SET_DESC NVARCHAR(MAX) = null,
------------	@ATTRIBUTE_ORDER INT = 1,
------------	@ATTRIBUTE_NAME NVARCHAR(255),
------------	@ATTRIBUTE_DESC NVARCHAR(2000) = null,
------------	@CONTACT_ID INT,
------------	@ORIGINAL_ATTRIBUTE_ID BIGINT = null,

------------	@NEW_ATTRIBUTE_SET_ID BIGINT OUTPUT,
------------	@NEW_ATTRIBUTE_ID BIGINT OUTPUT
------------)

------------As

------------SET NOCOUNT ON

------------	INSERT INTO TB_ATTRIBUTE(CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, ORIGINAL_ATTRIBUTE_ID)
------------		VALUES(@CONTACT_ID, @ATTRIBUTE_NAME, @ATTRIBUTE_DESC, @ORIGINAL_ATTRIBUTE_ID)

------------	SET @NEW_ATTRIBUTE_ID = @@IDENTITY

------------	INSERT INTO TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)
------------		VALUES(@NEW_ATTRIBUTE_ID, @SET_ID, @PARENT_ATTRIBUTE_ID, @ATTRIBUTE_TYPE_ID, @ATTRIBUTE_SET_DESC, @ATTRIBUTE_ORDER)

------------	SET @NEW_ATTRIBUTE_SET_ID = @@IDENTITY


------------SET NOCOUNT OFF
------------GO

------------/****** Object:  StoredProcedure [dbo].[st_ReviewSetsForCopy]    Script Date: 06/25/2015 17:12:10 ******/
------------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ReviewSetsForCopy]') AND type in (N'P', N'PC'))
------------DROP PROCEDURE [dbo].[st_ReviewSetsForCopy]
------------GO


------------/****** Object:  StoredProcedure [dbo].[st_ReviewSetsForCopy]    Script Date: 06/25/2015 17:12:10 ******/
------------SET ANSI_NULLS ON
------------GO

------------SET QUOTED_IDENTIFIER ON
------------GO

-------------- =============================================
-------------- Author:		<Author,,Name>
-------------- Create date: <Create Date,,>
-------------- Description:	<Description,,>
-------------- =============================================
------------CREATE PROCEDURE [dbo].[st_ReviewSetsForCopy]
------------	-- Add the parameters for the stored procedure here
	
------------	@REVIEW_ID int,
------------	@CONTACT_ID int,
------------	@PRIVATE_SETS bit
	
------------AS
------------BEGIN
------------	-- SET NOCOUNT ON added to prevent extra result sets from
------------	-- interfering with SELECT statements.
------------	SET NOCOUNT ON;
------------	if @PRIVATE_SETS = 1
------------	BEGIN
------------		SELECT REVIEW_SET_ID, RS.REVIEW_ID, RS.SET_ID, ALLOW_CODING_EDITS, S.SET_TYPE_ID, SET_NAME, SET_TYPE, CODING_IS_FINAL, SET_ORDER, MAX_DEPTH, S.SET_DESCRIPTION, S.ORIGINAL_SET_ID
------------		FROM TB_REVIEW_SET RS
------------		INNER JOIN TB_SET S ON S.SET_ID = RS.SET_ID
------------		INNER JOIN TB_SET_TYPE ON TB_SET_TYPE.SET_TYPE_ID = S.SET_TYPE_ID
------------		INNER JOIN TB_REVIEW_CONTACT rc on RS.REVIEW_ID = rc.REVIEW_ID and rc.CONTACT_ID = @CONTACT_ID
------------		inner join TB_CONTACT_REVIEW_ROLE crr on rc.REVIEW_CONTACT_ID = crr.REVIEW_CONTACT_ID and ROLE_NAME = 'AdminUser'
------------		WHERE RS.REVIEW_ID != @REVIEW_ID
------------		ORDER BY RS.REVIEW_ID, RS.SET_ORDER, RS.SET_ID
------------	END
------------	ELSE
------------	BEGIN
------------		SELECT REVIEW_SET_ID, RS.REVIEW_ID, RS.SET_ID, ALLOW_CODING_EDITS, S.SET_TYPE_ID, SET_NAME, SET_TYPE, CODING_IS_FINAL, SET_ORDER, MAX_DEPTH, S.SET_DESCRIPTION, S.ORIGINAL_SET_ID
------------		FROM TB_REVIEW_SET RS
------------		INNER JOIN TB_SET S ON S.SET_ID = RS.SET_ID
------------		INNER JOIN TB_SET_TYPE ON TB_SET_TYPE.SET_TYPE_ID = S.SET_TYPE_ID
------------		inner join ReviewerAdmin.dbo.TB_MANAGEMENT_SETTINGS ms on RS.REVIEW_ID = ms.PUBLIC_CODESETS_REVIEW_ID 
------------		ORDER BY RS.REVIEW_ID, RS.SET_ORDER, RS.SET_ID
------------	END
------------END

------------GO

------------/*NOTE!! this UPDATE requires also to add data in Reviewer admin, but the details depend on the data present in the actual DB! 
------------**for the live version the following was used:*/
------------Use [ReviewerAdmin]
------------GO

------------Insert into TB_TEMPLATE_REVIEW
------------     VALUES
------------           ('Standard Review'
------------           ,'This template contains a selection of codesets that most reviews would include. There are two screening rounds, an Allocations codeset, a Data Extraction and a Risk of Bias codeset. If in doubt, this template is your best choice. You will be able to edit the imported Codesets, remove the unwanted ones and/or add more.')
------------Insert into TB_TEMPLATE_REVIEW
------------     VALUES
------------           ('Minimal Review'
------------           ,'This template contains minimal selection of preconfigured but mostly empty codesets. If you know your review will not follow the typical workflow, this is the template to pick. You will be able to edit the imported Codesets, remove the unwanted ones and/or add more.')
------------GO

------------declare @currTemplate int  = (select TEMPLATE_REVIEW_ID from TB_TEMPLATE_REVIEW where TEMPLATE_NAME = 'Standard Review')
--------------insert list of review_set_id in here

------------insert into TB_TEMPLATE_REVIEW_SET
------------	VALUES
------------	(
------------	@currTemplate
------------	,3296 --pick your review_set_id of choice!!
------------	)
------------insert into TB_TEMPLATE_REVIEW_SET
------------	VALUES
------------	(
------------	@currTemplate
------------	,3297 --pick your review_set_id of choice!!
------------	)
------------insert into TB_TEMPLATE_REVIEW_SET
------------	VALUES
------------	(
------------	@currTemplate
------------	,3299 --pick your review_set_id of choice!!
------------	)
------------insert into TB_TEMPLATE_REVIEW_SET
------------	VALUES
------------	(
------------	@currTemplate
------------	,3298 --pick your review_set_id of choice!!
------------	)
------------insert into TB_TEMPLATE_REVIEW_SET
------------	VALUES
------------	(
------------	@currTemplate
------------	,12615 --pick your review_set_id of choice!!
------------	)
------------insert into TB_TEMPLATE_REVIEW_SET
------------	VALUES
------------	(
------------	@currTemplate
------------	,35467 --pick your review_set_id of choice!!
------------	)

--------------repeat as needed
--------------[...]


--------------second bit for the other template
------------set @currTemplate = (select TEMPLATE_REVIEW_ID from TB_TEMPLATE_REVIEW where TEMPLATE_NAME = 'Minimal Review')
--------------insert list of review_set_id in here

------------insert into TB_TEMPLATE_REVIEW_SET
------------	VALUES
------------	(
------------	@currTemplate
------------	,35458 --pick your review_set_id of choice!!
------------	)
------------insert into TB_TEMPLATE_REVIEW_SET
------------	VALUES
------------	(
------------	@currTemplate
------------	,35459 --pick your review_set_id of choice!!
------------	)
------------insert into TB_TEMPLATE_REVIEW_SET
------------	VALUES
------------	(
------------	@currTemplate
------------	,35467 --pick your review_set_id of choice!!
------------	)

------------Use [Reviewer]
------------GO


------------Update tb_set set SET_DESCRIPTION = 'This codeset is a standard risk of bias tool derived from the Cochrane Collaboration Guidelines. If this is a Cochrane review you will want to use this codeset.' 
------------where SET_ID =12608

------------Update tb_set set SET_DESCRIPTION = 'This is the screening codeset included in the ‘Minimal’ review template. You will want to edit and add more child codes to match your screening criteria. A typical arrangement would have many ‘Exclude’ codes and one or two ‘Include’ codes. Make sure you select the correct code-type (Include or Exclude) when adding codes (Exclude type codes are shown in Red).' 
------------where SET_ID =35444

------------Update tb_set set SET_DESCRIPTION = 'This is an administration codeset included in the ‘Minimal’ review template. This could be adapted and used for a number of different purposes such as Allocation, document retrieval or general review management.' 
------------where SET_ID =35445

------------Update tb_set set SET_DESCRIPTION = 'This codeset is for screening on Title and Abstract. It is constructed of Exclude and Include codes and has been set for Comparison coding. 
------------The criteria shown are for example purposes only and would most likely change depending on your review question.' 
------------where SET_ID =3292

------------Update tb_set set SET_DESCRIPTION = 'This codeset is used for screening the full text documents. It is constructed of Exclude and Include codes and has been set for Normal coding. 
------------This stage would take place after ''Screening on Title & Abstract'' and retrieval of the ''Included'' references.
------------The criteria shown are for example purposes only and would most likely change depending on your review question.' 
------------where SET_ID =3293

------------Update tb_set set SET_DESCRIPTION = 'Allocation codesets will contain codes to identify the items that will be used in your coding assignments. It is a good idea to have sections in this codeset for the different stages of the review process.' 
------------where SET_ID =3295

------------Update tb_set set SET_DESCRIPTION = 'The retrieval codeset can be used to help manage the full text retrieval of your included references. If you begin by assigning a ''Not in File'' code to all of your included items you can then change the code when the paper has been ordered or retrieved.' 
------------where SET_ID =3294

------------Update tb_set set SET_DESCRIPTION = 'The data extraction codeset should be used to collect the data from your studies. It is best to set it up in sections with each section containing relevant questions and each question containing a number of possible answers. Please see the Data Extraction tool in the example review for guidance.' 
------------where SET_ID =35453

------------/*END of DB-dependent Data addition*/

------------USE [TestReviewer]
------------GO
------------/****** Object:  StoredProcedure [dbo].[st_ReviewSetInterventions]    Script Date: 07/21/2015 10:37:43 ******/
------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------ALTER procedure [dbo].[st_ReviewSetInterventions]
------------(
------------	@ITEM_SET_ID INT = NULL,
------------	@SET_ID INT = NULL
------------)

------------As

------------SET NOCOUNT ON
------------IF (@SET_ID = 0)
------------BEGIN
------------	SELECT DISTINCT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
------------	FROM TB_ITEM_SET
------------	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
------------	INNER JOIN TB_ATTRIBUTE_TYPE ON TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID
------------	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
------------	WHERE TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = 5 AND TB_ITEM_SET.ITEM_SET_ID = @ITEM_SET_ID
------------	AND dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
------------	ORDER BY TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ATTRIBUTE_SET.ATTRIBUTE_ORDER
------------END
------------ELSE
------------BEGIN
------------	SELECT DISTINCT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
------------	FROM TB_ITEM_SET
------------	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
------------	INNER JOIN TB_ATTRIBUTE_TYPE ON TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID
------------	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
------------	WHERE TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = 5 AND TB_ITEM_SET.SET_ID = @SET_ID
------------	AND dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
------------	ORDER BY TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ATTRIBUTE_SET.ATTRIBUTE_ORDER
------------END

------------SET NOCOUNT OFF

------------GO

------------USE [TestReviewer]
------------GO
------------/****** Object:  StoredProcedure [dbo].[st_ReviewSetControls]    Script Date: 07/21/2015 11:32:33 ******/
------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------ALTER procedure [dbo].[st_ReviewSetControls]
------------(
------------	@ITEM_SET_ID INT = NULL,
------------	@SET_ID INT = NULL
------------)

------------As

------------SET NOCOUNT ON

------------IF (@SET_ID = 0)
------------BEGIN
------------	SELECT DISTINCT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
------------	FROM TB_ITEM_SET
------------	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
------------	INNER JOIN TB_ATTRIBUTE_TYPE ON TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID
------------	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
------------	WHERE TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = 6 AND TB_ITEM_SET.ITEM_SET_ID = @ITEM_SET_ID
------------	AND dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
------------	ORDER BY TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ATTRIBUTE_SET.ATTRIBUTE_ORDER
------------END
------------ELSE
------------BEGIN
------------	SELECT DISTINCT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
------------	FROM TB_ITEM_SET
------------	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
------------	INNER JOIN TB_ATTRIBUTE_TYPE ON TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID
------------	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
------------	WHERE TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = 6 AND TB_ITEM_SET.SET_ID = @SET_ID
------------	AND dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
------------	ORDER BY TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ATTRIBUTE_SET.ATTRIBUTE_ORDER
------------END

------------SET NOCOUNT OFF

------------GO


------------USE [TestReviewer]
------------GO
------------/****** Object:  StoredProcedure [dbo].[st_ReviewSetOutcomes]    Script Date: 07/21/2015 11:31:01 ******/
------------SET ANSI_NULLS ON
------------GO
------------SET QUOTED_IDENTIFIER ON
------------GO
------------ALTER procedure [dbo].[st_ReviewSetOutcomes]
------------(
------------	@ITEM_SET_ID INT = NULL,
------------	@SET_ID INT = NULL
------------)

------------As

------------SET NOCOUNT ON
------------IF (@SET_ID = 0)
------------BEGIN
------------	SELECT DISTINCT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
------------	FROM TB_ITEM_SET
------------	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
------------	INNER JOIN TB_ATTRIBUTE_TYPE ON TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID
------------	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
------------	WHERE TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = 4 AND TB_ITEM_SET.ITEM_SET_ID = @ITEM_SET_ID
------------	AND dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
------------	ORDER BY TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ATTRIBUTE_SET.ATTRIBUTE_ORDER
------------END
------------ELSE
------------BEGIN
------------	SELECT DISTINCT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER
------------	FROM TB_ITEM_SET
------------	INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID
------------	INNER JOIN TB_ATTRIBUTE_TYPE ON TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID
------------	INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ATTRIBUTE_SET.ATTRIBUTE_ID
------------	WHERE TB_ATTRIBUTE_TYPE.ATTRIBUTE_TYPE_ID = 4 AND TB_ITEM_SET.SET_ID = @SET_ID
------------	AND dbo.fn_IsAttributeInTree(TB_ATTRIBUTE.ATTRIBUTE_ID) = 1
------------	ORDER BY TB_ATTRIBUTE.ATTRIBUTE_NAME, TB_ATTRIBUTE_SET.ATTRIBUTE_ORDER
------------END

------------SET NOCOUNT OFF
------------GO



