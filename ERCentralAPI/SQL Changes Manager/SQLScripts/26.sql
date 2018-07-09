--Sergio get by ReferenceID list (not pubmed IDs)
USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_findCitationsByExternalIDs]    Script Date: 02/07/2018 11:58:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_findCitationsByReferenceIDs]
(
	@RefIDs nvarchar(max)
)

As

SET NOCOUNT ON
declare @t table (RefId bigint primary key)

insert into @t (RefId) select distinct CAST([value] as bigint) from dbo.fn_Split(@RefIDs, '¬')

if (@@ROWCOUNT > 0)
BEGIN
	select *
	, rt.[TYPE_NAME]
	, dbo.fn_REBUILD_AUTHORS(REFERENCE_ID, 0) as AUTHORS
	, dbo.fn_REBUILD_AUTHORS(REFERENCE_ID, 1) as PARENT_AUTHORS
	, dbo.fn_REBUILD_EXTERNAL_IDS(REFERENCE_ID) as EXTERNAL_IDS
	 from TB_REFERENCE r
	 inner join TB_REFERENCE_TYPE rt on r.TYPE_ID = rt.TYPE_ID
	 inner join @t t on r.REFERENCE_ID = t.RefId
END

SET NOCOUNT OFF
GO


-- PATRICK SQL CHANGES
-- 2 SPs altered 02/07/2018

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_RCT_GET_LATEST_YEARLY_FILE_NAMES]    Script Date: 02/07/2018 12:35:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_RCT_GET_LATEST_YEARLY_FILE_NAMES]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT [RCT_FILE_NAME], [TB_RCT_UPDATE_FILE].RCT_UPLOAD_DATE
	FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
	WHERE [RCT_FILE_NAME] LIKE '%gz%'
	ORDER BY [RCT_UPLOAD_DATE] DESC
 
	SET NOCOUNT OFF
END


GO
/****** Object:  StoredProcedure [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]    Script Date: 02/07/2018 12:35:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP (1) [RCT_FILE_NAME], [TB_RCT_UPDATE_FILE].RCT_UPLOAD_DATE
	FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
	WHERE [RCT_FILE_NAME] NOT LIKE '%gz%'
	ORDER BY [RCT_UPLOAD_DATE] DESC

 
	SET NOCOUNT OFF
END
GO

-- PATRICK SQL Changes 03/07/2018
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
USE [DataService]
GO

BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_REFERENCE ADD
	HUMAN_SCORE float(53) NULL
GO
ALTER TABLE dbo.TB_REFERENCE SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_ReferenceUpdate_Arrow_Scores]    Script Date: 03/07/2018 11:13:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ReferenceUpdate_Arrow_Scores]
	-- Add the parameters for the stored procedure here
	(
		@IDS NVARCHAR(MAX) = NULL,
		@SCORES NVARCHAR(MAX) = NULL,
		@ID int = NULL
	)
AS
BEGIN

	SET NOCOUNT ON;

	declare @t table (tidx int, [value] nvarchar(100), score float null, ref_id bigint, primary key(value, ref_id))
	insert into @t 
	select t.*, null, -1 from dbo.fn_Split(@IDS, ',') t 
				
	update @t set score = s.value
	from dbo.fn_Split(@SCORES, ',') s
	where tidx = s.idx
	update @t set ref_id = reference_id from
	 TB_EXTERNALID e inner join @t t on e.TYPE = 'pubmed' and t.VALUE = e.[value]
  
	--select * from @t
	--delete from @t
	--select r.* from @t t inner join TB_EXTERNALID ext on ext.TYPE = 'pubmed' AND ext.VALUE = t.value
	--inner join TB_REFERENCE r on ext.REFERENCE_ID = r.REFERENCE_ID
	

	-- FROM here need to update the references table in the appropriate place
	IF @ID = 'RCT'
			UPDATE [dbo].[TB_REFERENCE]  
			SET ARROW_SCORE = t.score
			FROM @t t INNER JOIN [dbo].[TB_REFERENCE] R 
			ON t.ref_id = R.REFERENCE_ID
	ELSE
			UPDATE [dbo].[TB_REFERENCE]  
			SET HUMAN_SCORE = t.score
			FROM @t t INNER JOIN [dbo].[TB_REFERENCE] R 
			ON t.ref_id = R.REFERENCE_ID

	SET NOCOUNT OFF
END
GO

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]    Script Date: 03/07/2018 13:21:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP (1) [RCT_FILE_NAME], [TB_RCT_UPDATE_FILE].RCT_UPLOAD_DATE
	FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
	WHERE [RCT_FILE_NAME] NOT LIKE '%gz%' AND [RCT_FILE_NAME] LIKE '%rct%'
	ORDER BY [RCT_UPLOAD_DATE] DESC

 
	SET NOCOUNT OFF
END

GO

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]    Script Date: 03/07/2018 13:21:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_HUMAN_GET_LATEST_UPLOAD_FILE_NAME]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP (1) [RCT_FILE_NAME], [TB_RCT_UPDATE_FILE].RCT_UPLOAD_DATE
	FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
	WHERE [RCT_FILE_NAME] NOT LIKE '%gz%' AND [RCT_FILE_NAME] LIKE '%tagger%'
	ORDER BY [RCT_UPLOAD_DATE] DESC

 
	SET NOCOUNT OFF
END

GO

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.RCT_UPDATE_FILE_ID', N'Tmp_UPDATE_FILE_ID', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.RCT_FILE_NAME', N'Tmp_FILE_NAME_1', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.RCT_IMPORT_DATE', N'Tmp_IMPORT_DATE_2', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.RCT_UPLOAD_DATE', N'Tmp_UPLOAD_DATE_3', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.Tmp_UPDATE_FILE_ID', N'UPDATE_FILE_ID', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.Tmp_FILE_NAME_1', N'FILE_NAME', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.Tmp_IMPORT_DATE_2', N'IMPORT_DATE', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.Tmp_UPLOAD_DATE_3', N'UPLOAD_DATE', 'COLUMN' 
GO
ALTER TABLE dbo.TB_RCT_UPDATE_FILE SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

GO

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]    Script Date: 03/07/2018 13:28:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP (1) [FILE_NAME], [TB_RCT_UPDATE_FILE].UPLOAD_DATE
	FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
	WHERE [FILE_NAME] NOT LIKE '%gz%' AND [FILE_NAME] LIKE '%rct%'
	ORDER BY [UPLOAD_DATE] DESC

 
	SET NOCOUNT OFF
END

GO


USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_RCT_IMPORT_UPDATE_INSERT]    Script Date: 03/07/2018 13:28:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_RCT_IMPORT_UPDATE_INSERT]
	-- Add the parameters for the stored procedure here
	(
		@RCT_FILE_NAME varchar(max),
		@RCT_IMPORT_DATE datetime,
		@RCT_UPLOAD_DATE datetime
	)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

		INSERT INTO [dbo].[TB_RCT_UPDATE_FILE]
		([FILE_NAME], IMPORT_DATE, UPLOAD_DATE)
		VALUES (@RCT_FILE_NAME, @RCT_IMPORT_DATE, @RCT_UPLOAD_DATE )
 
	SET NOCOUNT OFF
END

GO

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_HUMAN_GET_LATEST_UPLOAD_FILE_NAME]    Script Date: 03/07/2018 13:30:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_HUMAN_GET_LATEST_UPLOAD_FILE_NAME]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP (1) [FILE_NAME], [TB_RCT_UPDATE_FILE].UPLOAD_DATE
	FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
	WHERE [FILE_NAME] NOT LIKE '%gz%' AND [FILE_NAME] LIKE '%tagger%'
	ORDER BY [UPLOAD_DATE] DESC

 
	SET NOCOUNT OFF
END
GO

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_RCT_GET_LATEST_YEARLY_FILE_NAMES]    Script Date: 03/07/2018 15:26:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:        <Author,,Name>
-- Create date: <Create Date,,>
-- Description:    <Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_RCT_GET_LATEST_YEARLY_FILE_NAMES]
    -- Add the parameters for the stored procedure here
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    SELECT [FILE_NAME], [TB_RCT_UPDATE_FILE].UPLOAD_DATE
    FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
    WHERE [FILE_NAME] LIKE '%gz%'
    ORDER BY [UPLOAD_DATE] DESC

    SET NOCOUNT OFF
END
GO

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]    Script Date: 03/07/2018 15:28:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:        <Author,,Name>
-- Create date: <Create Date,,>
-- Description:    <Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]
    -- Add the parameters for the stored procedure here
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    SELECT TOP (1) [FILE_NAME], [TB_RCT_UPDATE_FILE].UPLOAD_DATE
    FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
    WHERE [FILE_NAME] NOT LIKE '%gz%' AND [FILE_NAME] LIKE '%rct%'
    ORDER BY [UPLOAD_DATE] DESC


    SET NOCOUNT OFF
END
GO

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_HUMAN_GET_LATEST_UPLOAD_FILE_NAME]    Script Date: 03/07/2018 15:28:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:        <Author,,Name>
-- Create date: <Create Date,,>
-- Description:    <Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_HUMAN_GET_LATEST_UPLOAD_FILE_NAME]
    -- Add the parameters for the stored procedure here
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    SELECT TOP (1) [FILE_NAME], [TB_RCT_UPDATE_FILE].UPLOAD_DATE
    FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
    WHERE [FILE_NAME] NOT LIKE '%gz%' AND [FILE_NAME] LIKE '%tagger%'
    ORDER BY [UPLOAD_DATE] DESC


    SET NOCOUNT OFF
END
GO

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_ReferenceUpdate_Arrow_Scores]    Script Date: 03/07/2018 15:32:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:        <Author,,Name>
-- Create date: <Create Date,,>
-- Description:    <Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ReferenceUpdate_Arrow_Scores]
    -- Add the parameters for the stored procedure here
    (
        @IDS NVARCHAR(MAX) = NULL,
        @SCORES NVARCHAR(MAX) = NULL,
        @ID VARCHAR = NULL
    )
AS
BEGIN

    SET NOCOUNT ON;

    declare @t table (tidx int, [value] nvarchar(100), score float null, ref_id bigint, primary key(value, ref_id))
    insert into @t 
    select t.*, null, -1 from dbo.fn_Split(@IDS, ',') t 
                
    update @t set score = s.value
    from dbo.fn_Split(@SCORES, ',') s
    where tidx = s.idx
    update @t set ref_id = reference_id from
     TB_EXTERNALID e inner join @t t on e.TYPE = 'pubmed' and t.VALUE = e.[value]
 
    --select * from @t
    --delete from @t
    --select r.* from @t t inner join TB_EXTERNALID ext on ext.TYPE = 'pubmed' AND ext.VALUE = t.value
    --inner join TB_REFERENCE r on ext.REFERENCE_ID = r.REFERENCE_ID
    

    -- FROM here need to update the references table in the appropriate place
    IF @ID = 'RCT'
            UPDATE [dbo].[TB_REFERENCE]  
            SET ARROW_SCORE = t.score
            FROM @t t INNER JOIN [dbo].[TB_REFERENCE] R 
            ON t.ref_id = R.REFERENCE_ID
    ELSE
            UPDATE [dbo].[TB_REFERENCE]  
            SET HUMAN_SCORE = t.score
            FROM @t t INNER JOIN [dbo].[TB_REFERENCE] R 
            ON t.ref_id = R.REFERENCE_ID

    SET NOCOUNT OFF
END
GO

--PATRICK CHANGES FOR SQL TO BUL DELETE AND INSERT FOR FTPUPDATEFOLDER OPTION
USE [DataService]
GO

/****** Object:  StoredProcedure [dbo].[st_DeleteReferencesByREFID]    Script Date: 05/07/2018 16:21:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[st_DeleteReferencesByREFID]
(
	@RefIDs nvarchar(max)
)

As

SET NOCOUNT ON
-- 


BEGIN

declare @sql nvarchar(max)
set @sql = 'DELETE from [DataService].[dbo].[TB_REFERENCE] where REFERENCE_ID in (' + @RefIDs + ')'
Execute(@sql)

END

SET NOCOUNT OFF
GO


--SERGIO bugfix for ReviewerAdmin copy review.
USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep11]    Script Date: 09/07/2018 11:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER procedure [dbo].[st_CopyReviewStep11]
(	
	@CONTACT_ID int,
	@SOURCE_REVIEW_ID int,
	@DESTINATION_REVIEW_ID int,
	@SOURCE_SET_ID int,
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON


declare @DESTINATION_SET_ID int
declare @DESTINATION_REVIEW_SET_ID int
declare @DESTINATION_SET_NAME nvarchar (255)
declare @NEW_OUTCOME_ID int
--declare @SOURCE_REVIEW_ID int
declare @NEW_SET_ID int
declare @NEW_ITEM_SET_ID int
set @RESULT = '0'


declare @tb_item_attribute table
(NEW_ITEM_ATTRIBUTE_ID bigint,  OLD_ITEM_ATTRIBUTE_ID bigint,
 NEW_ITEM_ID bigint,  OLD_ITEM_ID bigint,  NEW_ITEM_SET_ID bigint,
 OLD_ITEM_SET_ID bigint,  NEW_ATTRIBUTE_ID bigint,
 ORIGINAL_ATTRIBUTE_ID nvarchar(50), -- needs to be nvarchar to deal with ER3 attributes
 ADDITIONAL_TEXT nvarchar(max))
 
declare @tb_item_set table
(NEW_ITEM_SET_ID bigint,  EXAMPLE_ITEM_SET_ID bigint,
 NEW_ITEM_ID bigint,  EXAMPLE_ITEM_ID bigint,
 EXAMPLE_IS_COMPLETED bit,  NEW_CONTACT_ID int,
 EXAMPLE_IS_LOCKED bit)
 
declare @tb_new_items table
 (ITEM_ID bigint,
 SOURCE_ITEM_ID bigint)
 
declare @tb_item_outcome table
(new_outcome_id int,  old_outcome_id int,
 new_item_set_id int,  old_item_set_id int,
 outcome_type_id int,
 new_item_attribute_id_intervention bigint,  old_item_attribute_id_intervention bigint,
 new_item_attribute_id_control bigint,  old_item_attribute_id_control bigint,
 new_item_attribute_id_outcome bigint,  old_item_attribute_id_outcome nvarchar(255),
 outcome_title nvarchar(255),
 data1 float, data2 float, data3 float, data4 float,
 data5 float, data6 float, data7 float, data8 float,
 data9 float, data10 float, data11 float, data12 float,
 data13 float, data14 float,
 outcome_description nvarchar(4000))

declare @tb_item_outcome_attribute table
(new_item_outcome_attribute_id int,  old_item_outcome_attribute_id int,
 new_outcome_id int,  old_outcome_id int,
 new_attribute_id bigint,  old_attribute_id bigint,
 additional_text nvarchar(max))


BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION


--01 get a few variables
	--select @SOURCE_REVIEW_ID = EXAMPLE_NON_SHAREABLE_REVIEW_ID from TB_MANAGEMENT_SETTINGS
	select @NEW_SET_ID = SET_ID from Reviewer.dbo.TB_SET 
	where OLD_GUIDELINE_ID = @SOURCE_SET_ID
	and SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET where REVIEW_ID = @DESTINATION_REVIEW_ID)

--02 get old data from Reviewer.dbo.TB_ITEM_SET and place in @tb_item_set
	insert into @tb_item_set (EXAMPLE_ITEM_SET_ID, EXAMPLE_ITEM_ID, EXAMPLE_IS_COMPLETED, NEW_CONTACT_ID, EXAMPLE_IS_LOCKED)
	select ITEM_SET_ID, ITEM_ID, IS_COMPLETED, CONTACT_ID, IS_LOCKED 
	from Reviewer.dbo.TB_ITEM_SET 
	where SET_ID = @SOURCE_SET_ID
	
	--select * from @tb_item_set -- 1 OK
	
--03 sets up a list of the new items so we don't get cross-review contamination
	-- seems like an extra step but has little overhead and makes it easier
	-- for me to follow
	insert into @tb_new_items (ITEM_ID, SOURCE_ITEM_ID)
	select ITEM_ID, OLD_ITEM_ID from Reviewer.dbo.TB_ITEM where ITEM_ID in 
	(select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
	--select * from @tb_new_items -- 2 OK
	
--04 update @tb_item_set with the new item_id  
	update @tb_item_set
	set NEW_ITEM_ID = ITEM_ID
	from @tb_new_items
	where EXAMPLE_ITEM_ID = SOURCE_ITEM_ID
	
	--select * from @tb_item_set -- 3 OK

--05 get old data from Reviewer.dbo.TB_ITEM_ATTRIBUTE and place in @tb_item_attribute
	insert into @tb_item_attribute (OLD_ITEM_ATTRIBUTE_ID, OLD_ITEM_ID, OLD_ITEM_SET_ID,
	 ORIGINAL_ATTRIBUTE_ID, ADDITIONAL_TEXT)
	select i_a.ITEM_ATTRIBUTE_ID, i_a.ITEM_ID, i_a.ITEM_SET_ID, i_a.ATTRIBUTE_ID, i_a.ADDITIONAL_TEXT  
	from Reviewer.dbo.TB_ITEM_ATTRIBUTE i_a
	inner join Reviewer.dbo.TB_ITEM_SET i_s on i_s.ITEM_SET_ID = i_a.ITEM_SET_ID
	where i_s.SET_ID = @SOURCE_SET_ID
	
	--select * from @tb_item_attribute -- 4 OK

--06 user cursors to place new row in TB_ITEM_SET, get the new ITEM_SET_ID and update @tb_item_set
	declare @WORKING_ITEM_ID int
	declare ITEM_ID_CURSOR cursor for
	select NEW_ITEM_ID FROM @tb_item_set
	open ITEM_ID_CURSOR
	fetch next from ITEM_ID_CURSOR
	into @WORKING_ITEM_ID
	while @@FETCH_STATUS = 0
	begin
		-- set CONTACT_ID to newly created user account (rather than orginal coder)
		-- set IS_LOCKED to 0 as we want new user to experiment with the coding
		insert into Reviewer.dbo.TB_ITEM_SET (ITEM_ID, SET_ID, CONTACT_ID, IS_LOCKED)	
		SELECT NEW_ITEM_ID, @NEW_SET_ID, @CONTACT_ID, 0 FROM @tb_item_set 
		WHERE NEW_ITEM_ID = @WORKING_ITEM_ID
		set @NEW_ITEM_SET_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_item_set set new_item_set_id = @NEW_ITEM_SET_ID
		WHERE NEW_ITEM_ID = @WORKING_ITEM_ID

		FETCH NEXT FROM ITEM_ID_CURSOR 
		INTO @WORKING_ITEM_ID
	END

	CLOSE ITEM_ID_CURSOR
	DEALLOCATE ITEM_ID_CURSOR
	
	--select * from Reviewer.dbo.TB_ITEM_SET where ITEM_ID in -- 5 OK
	--(select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	--select * from @tb_item_set -- 6 OK

--07 update @tb_item_attribute with NEW_ITEM_ID, NEW_ITEM_SET_ID, NEW_ATTRIBUTE_ID
	--update @tb_item_attribute
	--set NEW_ITEM_ID = 
	--(select NEW_ITEM_ID from @tb_item_set
	--where EXAMPLE_ITEM_ID = OLD_ITEM_ID)
	
	update t1
	set NEW_ITEM_ID = t2.NEW_ITEM_ID
	from @tb_item_attribute t1 inner join @tb_item_set t2
	on t2.EXAMPLE_ITEM_ID = t1.OLD_ITEM_ID
	
	--select * from @tb_item_attribute -- 7 OK
	
	--update @tb_item_attribute
	--set NEW_ITEM_SET_ID = 
	--(select NEW_ITEM_SET_ID from @tb_item_set
	--where EXAMPLE_ITEM_SET_ID = OLD_ITEM_SET_ID)
	
	update t1
	set NEW_ITEM_SET_ID = t2.NEW_ITEM_SET_ID
	from @tb_item_attribute t1 inner join @tb_item_set t2
	on t2.EXAMPLE_ITEM_ID = t1.OLD_ITEM_ID
	
	--select * from @tb_item_attribute -- 8 OK
	
	--update @tb_item_attribute
	--set NEW_ATTRIBUTE_ID = 
	--(select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE a
	--where a.OLD_ATTRIBUTE_ID = ORIGINAL_ATTRIBUTE_ID
	---- added JB 22/04/15
	--and a.CONTACT_ID = @CONTACT_ID)

	declare @step_table table (A_ID bigint, O_A_ID nvarchar(50))
	insert into @step_table select a.ATTRIBUTE_ID, ORIGINAL_ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE a
	inner join Reviewer.dbo.TB_ATTRIBUTE_SET tas on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and tas.SET_ID = @NEW_SET_ID

	update t1
	set NEW_ATTRIBUTE_ID = t2.A_ID
	from @tb_item_attribute t1 inner join @step_table t2
	on t2.O_A_ID = t1.ORIGINAL_ATTRIBUTE_ID
	--where t2.CONTACT_ID = @CONTACT_ID

	--update t1
	--set NEW_ATTRIBUTE_ID = t2.ATTRIBUTE_ID
	--from @tb_item_attribute t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	--on t2.OLD_ATTRIBUTE_ID = t1.ORIGINAL_ATTRIBUTE_ID
	--where t2.CONTACT_ID = @CONTACT_ID
	
	--select * from @tb_item_attribute -- 9 OK

--08 place new rows in TB_ITEM_ATTRIBUTE based on @tb_item_attribute
	insert into Reviewer.dbo.TB_ITEM_ATTRIBUTE (ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT)
	select tb_i_a.NEW_ITEM_ID, tb_i_a.NEW_ITEM_SET_ID, tb_i_a.NEW_ATTRIBUTE_ID, tb_i_a.ADDITIONAL_TEXT 
	from @tb_item_attribute tb_i_a
	
--09 update TB_ITEM_SET with IS_COMPLETED values in @tb_item_set	
	UPDATE Reviewer.dbo.TB_ITEM_SET
	SET IS_COMPLETED = i_s.EXAMPLE_IS_COMPLETED
	FROM @tb_item_set i_s
	where i_s.NEW_ITEM_SET_ID = ITEM_SET_ID
	and i_s.NEW_ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW -- to restrict it to items in new review
	where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
	--select * from Reviewer.dbo.TB_ITEM_SET where ITEM_ID in -- 10 OK
	--(select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
--10 fill up @tb_item_outcome
	insert into @tb_item_outcome (old_outcome_id, old_item_set_id, outcome_type_id,
	 old_item_attribute_id_intervention, old_item_attribute_id_control,
	 old_item_attribute_id_outcome, outcome_title, data1, data2, data3, data4, data5,
	 data6, data7, data8, data9, data10, data11, data12, data13,
	 data14, outcome_description)
	select i_c.OUTCOME_ID, i_c.ITEM_SET_ID, i_c.OUTCOME_TYPE_ID, i_c.ITEM_ATTRIBUTE_ID_INTERVENTION,
	i_c.ITEM_ATTRIBUTE_ID_CONTROL, i_c.ITEM_ATTRIBUTE_ID_OUTCOME, i_c.OUTCOME_TITLE,
	i_c.DATA1, i_c.DATA2, i_c.DATA3, i_c.DATA4, i_c.DATA5, i_c.DATA6, i_c.DATA7,
	i_c.DATA8, i_c.DATA9, i_c.DATA10, i_c.DATA11, i_c.DATA12, i_c.DATA13, i_c.DATA14,
	i_c.OUTCOME_DESCRIPTION
	from Reviewer.dbo.TB_ITEM_OUTCOME i_c 
	inner join Reviewer.dbo.TB_ITEM_SET i_s on i_s.ITEM_SET_ID = i_c.ITEM_SET_ID
	where i_s.SET_ID = @SOURCE_SET_ID
	
--11 update @tb_item_outcome with the 'new_' values	
	update t1 --new_item_set_id
	set new_item_set_id = t2.NEW_ITEM_SET_ID
	from @tb_item_outcome t1 inner join @tb_item_set t2
	on t2.EXAMPLE_ITEM_SET_ID = t1.old_item_set_id
	
	update t1 --new_item_attribute_id_intervention
	set new_item_attribute_id_intervention = t2.ATTRIBUTE_ID
	from @tb_item_outcome t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_intervention
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
	update t1 --new_item_attribute_id_control
	set new_item_attribute_id_control = t2.ATTRIBUTE_ID
	from @tb_item_outcome t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_control
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
	update t1 --new_item_attribute_id_outcome
	set new_item_attribute_id_outcome = t2.ATTRIBUTE_ID
	from @tb_item_outcome t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_outcome
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
--12 copy @tb_item_outcome into Reviewer.dbo.TB_ITEM_OUTCOME
	declare @WORKING_OUTCOME_ID int
	declare OUTCOME_ID_CURSOR cursor for
	select old_outcome_id FROM @tb_item_outcome
	open OUTCOME_ID_CURSOR
	fetch next from OUTCOME_ID_CURSOR
	into @WORKING_OUTCOME_ID
	while @@FETCH_STATUS = 0
	begin
		insert into Reviewer.dbo.TB_ITEM_OUTCOME (ITEM_SET_ID, OUTCOME_TYPE_ID, 
		ITEM_ATTRIBUTE_ID_INTERVENTION, ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, 
		OUTCOME_TITLE, DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7,
		DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14, OUTCOME_DESCRIPTION)	
		SELECT new_item_set_id, outcome_type_id, new_item_attribute_id_intervention, 
		new_item_attribute_id_control, new_item_attribute_id_outcome, outcome_title, 
		data1, data2, data3, data4, data5, data6, data7, data8, data9, data10, data11, 
		data12, data13, data14, outcome_description 
		FROM @tb_item_outcome 
		WHERE old_outcome_id = @WORKING_OUTCOME_ID
		set @NEW_OUTCOME_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_item_outcome set new_outcome_id = @NEW_OUTCOME_ID
		WHERE old_outcome_id = @WORKING_OUTCOME_ID

		FETCH NEXT FROM OUTCOME_ID_CURSOR 
		INTO @WORKING_OUTCOME_ID
	END

	CLOSE OUTCOME_ID_CURSOR
	DEALLOCATE OUTCOME_ID_CURSOR

--13 fill up @tb_item_outcome_attribute
	insert into @tb_item_outcome_attribute (old_item_outcome_attribute_id,
	 old_outcome_id, old_attribute_id, additional_text)
	select ITEM_OUTCOME_ATTRIBUTE_ID, OUTCOME_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT
	from Reviewer.dbo.TB_ITEM_OUTCOME_ATTRIBUTE i_o_a
	inner join @tb_item_outcome i_o on i_o.OLD_OUTCOME_ID = i_o_a.OUTCOME_ID	

--14 update tb_item_outcome_attribute with the 'new_' values
	update t1 --new_outcome_id
	set new_outcome_id = t2.new_outcome_id
	from @tb_item_outcome_attribute t1 inner join @tb_item_outcome t2
	on t2.old_outcome_id = t1.old_outcome_id
	
	update t1 --new_attribute_id
	set new_attribute_id = t2.ATTRIBUTE_ID
	from @tb_item_outcome_attribute t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_attribute_id
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
--15 copy @tb_item_outcome_attribute into Reviewer.dbo.TB_ITEM_OUTCOME_ATTRIBUTE
	insert into Reviewer.dbo.TB_ITEM_OUTCOME_ATTRIBUTE (OUTCOME_ID, ATTRIBUTE_ID,
	ADDITIONAL_TEXT)
	select new_outcome_id, new_attribute_id, additional_text from @tb_item_outcome_attribute
	
--16 place the outcome_id values into ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA for later use
	insert into ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA (NEW_REVIEW_ID, OLD_OUTCOME_ID, NEW_OUTCOME_ID)
	select @DESTINATION_REVIEW_ID, old_outcome_id, new_outcome_id  
	from @tb_item_outcome
	
COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to copy the coding from the old review to the new review'
end
END CATCH


RETURN

SET NOCOUNT OFF

GO



