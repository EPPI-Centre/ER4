
USE [Reviewer]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Create_Zotero_ItemReview]') AND type in (N'P', N'PC'))
DROP PROCEDURE sp_Create_Zotero_ItemReview 
GO

/****** Object:  StoredProcedure [dbo].[st_ItemAttributeBulkAssignCodesFromMLsearchResults]    Script Date: 06/03/2023 10:43:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ItemAttributeBulkAssignCodesFromMLsearchResults]
	-- Add the parameters for the stored procedure here
	@SearchID int,
	@revID int,
	@ParentAttributeID bigint,
	@SetID int,
	@SearchName nvarchar(4000),
	@ContactID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Phases: 1. create codes using st_AttributeSetInsert
    -- 2. fetch IDs (comma separated) for each bucket
    -- 3. assign items to new codes via st_ItemAttributeBulkInsert
	-- repeat 2 and 3 for each bucket.
	
	--stuff we'll need:
	declare @NEW_ATTRIBUTE_SET_ID bigint ,@NEW_ATTRIBUTE_ID_1 bigint
			,@NEW_ATTRIBUTE_ID_2 bigint
			,@NEW_ATTRIBUTE_ID_3 bigint
			,@NEW_ATTRIBUTE_ID_4 bigint
			,@NEW_ATTRIBUTE_ID_5 bigint
			,@NEW_ATTRIBUTE_ID_6 bigint
			,@NEW_ATTRIBUTE_ID_7 bigint
			,@NEW_ATTRIBUTE_ID_8 bigint
			,@NEW_ATTRIBUTE_ID_9 bigint
			,@NEW_ATTRIBUTE_ID_10 bigint
	declare @IDs varchar(MAX)		
	
	--1. create codes using st_AttributeSetInsert
	
	--first of all find the order...
	declare @order int = (select MAX(ATTRIBUTE_ORDER) from TB_ATTRIBUTE_SET where SET_ID = @SetID and PARENT_ATTRIBUTE_ID = @ParentAttributeID)
	IF @order = null set @order = 0
	set @SearchName = 'FROM: ' + @SearchName --used as code description
	
	--create codes & take IDs
	set @order  = @order + 1
	EXECUTE [st_AttributeSetInsert] 
	   @SetID
	  ,@ParentAttributeID
	  ,1
	  ,@SearchName
	  ,@order
	  ,'0-9% range'
	  ,NULL
	  ,@ContactID
	  ,NULL
	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_1 OUTPUT
	set @order  = @order + 1
	EXECUTE [st_AttributeSetInsert] 
	   @SetID
	  ,@ParentAttributeID
	  ,1
	  ,@SearchName
	  ,@order
	  ,'10-19% range'
	  ,NULL
	  ,@ContactID
	  ,NULL
	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_2 OUTPUT
	set @order  = @order + 1
	EXECUTE [st_AttributeSetInsert] 
	   @SetID
	  ,@ParentAttributeID
	  ,1
	  ,@SearchName
	  ,@order
	  ,'20-29% range'
	  ,NULL
	  ,@ContactID
	  ,NULL
	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_3 OUTPUT
	set @order  = @order + 1
	EXECUTE [st_AttributeSetInsert] 
	   @SetID
	  ,@ParentAttributeID
	  ,1
	  ,@SearchName
	  ,@order
	  ,'30-39% range'
	  ,NULL
	  ,@ContactID
	  ,NULL
	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_4 OUTPUT
	set @order  = @order + 1
	EXECUTE [st_AttributeSetInsert] 
	   @SetID
	  ,@ParentAttributeID
	  ,1
	  ,@SearchName
	  ,@order
	  ,'40-49% range'
	  ,NULL
	  ,@ContactID
	  ,NULL
	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_5 OUTPUT
	EXECUTE [st_AttributeSetInsert] 
	   @SetID
	  ,@ParentAttributeID
	  ,1
	  ,@SearchName
	  ,@order
	  ,'50-59% range'
	  ,NULL
	  ,@ContactID
	  ,NULL
	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_6 OUTPUT
	set @order  = @order + 1
	EXECUTE [st_AttributeSetInsert] 
	   @SetID
	  ,@ParentAttributeID
	  ,1
	  ,@SearchName
	  ,@order
	  ,'60-69% range'
	  ,NULL
	  ,@ContactID
	  ,NULL
	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_7 OUTPUT
	set @order  = @order + 1
	EXECUTE [st_AttributeSetInsert] 
	   @SetID
	  ,@ParentAttributeID
	  ,1
	  ,@SearchName
	  ,@order
	  ,'70-79% range'
	  ,NULL
	  ,@ContactID
	  ,NULL
	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_8 OUTPUT
	set @order  = @order + 1
	EXECUTE [st_AttributeSetInsert] 
	   @SetID
	  ,@ParentAttributeID
	  ,1
	  ,@SearchName
	  ,@order
	  ,'80-89% range'
	  ,NULL
	  ,@ContactID
	  ,NULL
	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_9 OUTPUT
	set @order  = @order + 1
	EXECUTE [st_AttributeSetInsert] 
	   @SetID
	  ,@ParentAttributeID
	  ,1
	  ,@SearchName
	  ,@order
	  ,'90-99% range'
	  ,NULL
	  ,@ContactID
	  ,NULL
	  ,@NEW_ATTRIBUTE_SET_ID = @NEW_ATTRIBUTE_SET_ID OUTPUT
	  ,@NEW_ATTRIBUTE_ID = @NEW_ATTRIBUTE_ID_10 OUTPUT
	
	
	--2. fetch IDs (comma separated) for each bucket
	Declare @Items table (ItemID bigint primary key)
	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
		[ITEM_RANK] >= 0 AND [ITEM_RANK] < 10 AND SEARCH_ID = @SearchID
	set @IDs  = ''
	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
	IF LEN(@IDs) > 2
	BEGIN
		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
		--3. Bulk insert
		exec st_ItemAttributeBulkInsert @SetID,
			1,
			@ContactID,
			@NEW_ATTRIBUTE_ID_1,
			@IDs,
			'',
			@revID
	END
	--cleanup
	DELETE from @Items
	--REPEAT until all 10 codes are populated
	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
		[ITEM_RANK] >= 10 AND [ITEM_RANK] < 20 AND SEARCH_ID = @SearchID
	set @IDs  = ''
	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
	IF LEN(@IDs) > 2
	BEGIN
		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
		exec st_ItemAttributeBulkInsert @SetID,
			1,
			@ContactID,
			@NEW_ATTRIBUTE_ID_2,
			@IDs,
			'',
			@revID
	END
	--cleanup
	DELETE from @Items
	--REPEAT until all 10 codes are populated
	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
		[ITEM_RANK] >= 20 AND [ITEM_RANK] < 30 AND SEARCH_ID = @SearchID
	set @IDs  = ''
	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
	IF LEN(@IDs) > 2
	BEGIN
		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
		exec st_ItemAttributeBulkInsert @SetID,
			1,
			@ContactID,
			@NEW_ATTRIBUTE_ID_3,
			@IDs,
			'',
			@revID
	END
	--cleanup
	DELETE from @Items
	--REPEAT until all 10 codes are populated
	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
		[ITEM_RANK] >= 30 AND [ITEM_RANK] < 40 AND SEARCH_ID = @SearchID
	set @IDs  = ''
	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
	IF LEN(@IDs) > 2
	BEGIN
		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
		exec st_ItemAttributeBulkInsert @SetID,
			1,
			@ContactID,
			@NEW_ATTRIBUTE_ID_4,
			@IDs,
			'',
			@revID
	END
	--cleanup
	DELETE from @Items
	--REPEAT until all 10 codes are populated
	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
		[ITEM_RANK] >= 40 AND [ITEM_RANK] < 50 AND SEARCH_ID = @SearchID
	set @IDs  = ''
	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
	IF LEN(@IDs) > 2
	BEGIN
		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
		exec st_ItemAttributeBulkInsert @SetID,
			1,
			@ContactID,
			@NEW_ATTRIBUTE_ID_5,
			@IDs,
			'',
			@revID
	END
	--cleanup
	DELETE from @Items
	--REPEAT until all 10 codes are populated
	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
		[ITEM_RANK] >= 50 AND [ITEM_RANK] < 60 AND SEARCH_ID = @SearchID
	set @IDs  = ''
	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
	IF LEN(@IDs) > 2
	BEGIN
		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
		exec st_ItemAttributeBulkInsert @SetID,
			1,
			@ContactID,
			@NEW_ATTRIBUTE_ID_6,
			@IDs,
			'',
			@revID
	END
	--cleanup
	DELETE from @Items
	--REPEAT until all 10 codes are populated
	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
		[ITEM_RANK] >= 60 AND [ITEM_RANK] < 70 AND SEARCH_ID = @SearchID
	set @IDs  = ''
	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
	IF LEN(@IDs) > 2
	BEGIN
		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
		exec st_ItemAttributeBulkInsert @SetID,
			1,
			@ContactID,
			@NEW_ATTRIBUTE_ID_7,
			@IDs,
			'',
			@revID
	END
	--cleanup
	DELETE from @Items
	--REPEAT until all 10 codes are populated
	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
		[ITEM_RANK] >= 70 AND [ITEM_RANK] < 80 AND SEARCH_ID = @SearchID
	set @IDs  = ''
	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
	IF LEN(@IDs) > 2
	BEGIN
		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
		exec st_ItemAttributeBulkInsert @SetID,
			1,
			@ContactID,
			@NEW_ATTRIBUTE_ID_8,
			@IDs,
			'',
			@revID
	END
	--cleanup
	DELETE from @Items
	--REPEAT until all 10 codes are populated
	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
		[ITEM_RANK] >= 80 AND [ITEM_RANK] < 90 AND SEARCH_ID = @SearchID
	set @IDs  = ''
	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
	IF LEN(@IDs) > 2
	BEGIN
		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
		exec st_ItemAttributeBulkInsert @SetID,
			1,
			@ContactID,
			@NEW_ATTRIBUTE_ID_9,
			@IDs,
			'',
		@revID
	END
	--cleanup
	DELETE from @Items
	--REPEAT until all 10 codes are populated
	Insert into @Items select ITEM_ID FROM TB_SEARCH_ITEM WHERE 
		[ITEM_RANK] >= 90 AND [ITEM_RANK] <= 100 AND SEARCH_ID = @SearchID
	set @IDs  = ''
	select @IDs = @IDs + ',' + CONVERT(nvarchar(100), ItemID) from @Items
	IF LEN(@IDs) > 2
	BEGIN
		SET @IDs = RIGHT(@IDs, LEN(@IDs)-1)
		exec st_ItemAttributeBulkInsert @SetID,
			1,
			@ContactID,
			@NEW_ATTRIBUTE_ID_10,
			@IDs,
			'',
			@revID
	END
	--cleanup
	DELETE from @Items
	--DONE
END
GO

/****** Object:  StoredProcedure [dbo].[st_Termine_Log_Insert]    Script Date: 06/03/2023 10:45:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_Termine_Log_Insert]
	-- Add the parameters for the stored procedure here
	@C_ID int,
	@R_ID int,
	@BYTES int,
	@SUCCESS bit,
	@N int,
	@ERR nvarchar(2000) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO [TB_TERMINE_LOG]
           (CONTACT_ID
           ,REVIEW_ID
           ,BYTES
           ,SUCCESS
           ,N_OF_TERMS
           ,ERROR
           )
     VALUES
           (@C_ID
           ,@R_ID
           ,@BYTES
           ,@SUCCESS 
		   ,@N 
		   ,@ERR
           )

END
GO


/****** Object:  StoredProcedure [dbo].[st_CheckReviewHasUpdates]    Script Date: 06/03/2023 10:48:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Checks to see whether a review has any auto-identified studies for authors to check
-- =============================================
ALTER   PROCEDURE [dbo].[st_CheckReviewHasUpdates] 
	-- Add the parameters for the stored procedure here
	@REVIEW_id int = 0,
	@NUpdates INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    select @NUpdates = sum(N_PAPERS) from tb_MAG_RELATED_RUN
		where REVIEW_ID = @REVIEW_id
		and USER_STATUS = 'Unchecked'

	if @NUpdates is null
	begin
		set @NUpdates = 0
	end
END
GO


/****** Object:  StoredProcedure [dbo].[st_MagItemPaperInsert]    Script Date: 06/03/2023 10:49:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Pull a limited number of MAG papers into tb_ITEM based on a list of IDs. e.g. user 'selected' list
-- =============================================
ALTER PROCEDURE [dbo].[st_MagItemPaperInsert] 
	-- Add the parameters for the stored procedure here
	@SOURCE_NAME nvarchar(255),
	@CONTACT_ID INT = 0,
	@REVIEW_ID INT = 0,
	@GUID_JOB nvarchar(50),
	@MAG_RELATED_RUN_ID INT = 0,
	@N_IMPORTED INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @SourceId int = 0

	insert into TB_SOURCE(SOURCE_NAME, REVIEW_ID, IS_DELETED, DATE_OF_SEARCH, 
		DATE_OF_IMPORT, SOURCE_DATABASE,  SEARCH_DESCRIPTION, SEARCH_STRING, NOTES, IMPORT_FILTER_ID)
	values(@SOURCE_NAME, @REVIEW_id, 'false', GETDATE(),
		GETDATE(), 'Microsoft Academic', 'Browsing items related to a given item', 'Browse', '', 0)

	set @SourceId = @@IDENTITY

	declare @ItemIds Table(ITEM_ID bigint, PaperId bigint)

	insert into TB_ITEM(TYPE_ID, TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, YEAR, STANDARD_NUMBER, CITY,
		COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, URL, OLD_ITEM_ID, ABSTRACT, DOI, SearchText)
	output INSERTED.ITEM_ID, cast(inserted.OLD_ITEM_ID as bigint) into @ItemIds
	SELECT 
		/*   we don't currently have publication type in MAG items (once MAKES is live we will)
		case
			when p.DocType = 'Book' then 2
			when p.DocType = 'BookChapter' then 3
			when p.DocType = 'Conference' then 5
			when p.DocType = 'Journal' then 14
			when p.DocType = 'Patent' then 1
			else 14
		end
		*/
		14, mipi.title, mipi.journal,
			(select top(1) LAST from TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP where GUID_JOB = @GUID_JOB and PaperId = mipi.PaperId and RANK = 1 ) + ' (' + cast(mipi.year as nvarchar) + ')',
			GETDATE(), @CONTACT_ID, mipi.Year, 
			'', '', '', '', '', mipi.volume, mipi.pages, '', mipi.issue, '', cast(mipi.PaperId as nvarchar), mipi.abstract, mipi.doi, mipi.SearchText
	 FROM  TB_MAG_ITEM_PAPER_INSERT_TEMP mipi
	 where mipi.GUID_JOB = @GUID_JOB and not mipi.PaperId in 
		(select paperid from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = imm.REVIEW_ID
			where ir.REVIEW_ID = @REVIEW_id and ir.IS_DELETED = 'false' and
				(imm.AutoMatchScore > 0.8 or (imm.ManualFalseMatch = 'true' or imm.ManualTrueMatch = 'true')))

	set @N_IMPORTED = @@ROWCOUNT

	if @N_IMPORTED > 0
	begin

		 insert into TB_ITEM_SOURCE(ITEM_ID, SOURCE_ID)
		 select ITEM_ID, @SourceId from @ItemIds

		 insert into TB_ITEM_REVIEW(ITEM_ID, REVIEW_ID, IS_DELETED, IS_INCLUDED)
		 select item_id, @REVIEW_id, 'false', 'true' from @ItemIds

		 insert into tb_ITEM_MAG_MATCH(ITEM_ID, REVIEW_ID, PaperId, AutoMatchScore, ManualTrueMatch, ManualFalseMatch)
		 select item_id, @REVIEW_id, PaperId, 1, 'false', 'false' from @ItemIds

		 insert into TB_ITEM_AUTHOR(ITEM_ID, LAST, FIRST, SECOND, ROLE, RANK)
		 select ITEM_ID, LAST, FIRST, SECOND, 0, RANK FROM
			TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP auTemp INNER JOIN @ItemIds I on i.PaperId = auTemp.PaperId
			where auTemp.GUID_JOB = @GUID_JOB
	end

	IF @MAG_RELATED_RUN_ID > 0
	BEGIN
		update tb_MAG_RELATED_RUN
			set USER_STATUS = 'Imported'
			where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
	END

	delete from TB_MAG_ITEM_PAPER_INSERT_TEMP where GUID_JOB = @GUID_JOB
	delete from TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP where GUID_JOB = @GUID_JOB
END
GO

/****** Object:  StoredProcedure [dbo].[st_MagMatchedPapersInsert]    Script Date: 06/03/2023 10:50:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_item_mag_match based on manual lookup
-- =============================================
ALTER PROCEDURE [dbo].[st_MagMatchedPapersInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_ID BIGINT
,	@PaperId bigint
,	@AutoMatchScore float
,	@ManualTrueMatch bit = null
,	@ManualFalseMatch bit = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	declare @chk int
	select @chk = count(*) from tb_ITEM_MAG_MATCH
		where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId

	if @chk = 0
	begin
		insert into tb_ITEM_MAG_MATCH(REVIEW_ID, ITEM_ID, PaperId, ManualTrueMatch, ManualFalseMatch, AutoMatchScore)
		values (@REVIEW_ID, @ITEM_ID, @PaperId, @ManualTrueMatch, @ManualFalseMatch, @AutoMatchScore)
	end
	else
	begin
		update tb_ITEM_MAG_MATCH
			set ManualTrueMatch = @ManualTrueMatch,
				ManualFalseMatch = @ManualFalseMatch
			where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId
	end
END
GO


/****** Object:  StoredProcedure [dbo].[st_MagMatchItemsGetIdList]    Script Date: 06/03/2023 10:54:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Match EPPI-Reviewer TB_ITEM records to MAG Papers
-- =============================================
ALTER PROCEDURE [dbo].[st_MagMatchItemsGetIdList] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ATTRIBUTE_ID BIGINT = 0
AS
BEGIN

DECLARE @ITEMS TABLE
(
	ITEM_ID bigint
)

if @ATTRIBUTE_ID > 0
begin
	insert into @ITEMS
	select distinct tir.ITEM_ID
		from TB_ITEM_REVIEW tir 
		inner join TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = tir.ITEM_ID and tia.ATTRIBUTE_ID = @ATTRIBUTE_ID
		inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
		where tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
			and not tir.ITEM_ID IN (SELECT ITEM_ID FROM tb_ITEM_MAG_MATCH IMM
				WHERE IMM.ITEM_ID = TIR.ITEM_ID AND IMM.REVIEW_ID = @REVIEW_ID)
end
else
begin
	INSERT INTO @ITEMS
	select distinct tir.ITEM_ID
		from TB_ITEM_REVIEW tir where tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
		and not tir.ITEM_ID IN (SELECT ITEM_ID FROM tb_ITEM_MAG_MATCH IMM
				WHERE IMM.ITEM_ID = TIR.ITEM_ID AND IMM.REVIEW_ID = @REVIEW_ID)
end

SELECT ITEM_ID FROM @ITEMS

END
GO


/****** Object:  StoredProcedure [dbo].[st_MagPaper]    Script Date: 06/03/2023 10:55:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns a single paper
-- =============================================
ALTER PROCEDURE [dbo].[st_MagPaper] 
	-- Add the parameters for the stored procedure here
	@PaperId bigint = 0
,	@REVIEW_ID int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT 
		imm.ITEM_ID, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch
		from tb_ITEM_MAG_MATCH imm where imm.PaperId = @PaperId
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID
END
GO


/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunGetSeedIds]    Script Date: 06/03/2023 10:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Stage 1 in getting related papers: get the list of seed MAG IDs
-- =============================================
ALTER PROCEDURE [dbo].[st_MagRelatedPapersRunGetSeedIds] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID int
,	@REVIEW_ID INT
,	@ATTRIBUTE_ID BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if @ATTRIBUTE_ID > 0
	BEGIN
		select imm.PaperId from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
		inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = ia.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
		WHERE (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		group by imm.PaperId
		order by imm.PaperId
	END
	else
	BEGIN
		select imm.PaperId from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
		WHERE (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		group by imm.PaperId
		order by imm.PaperId
	END
END
GO


/****** Object:  StoredProcedure [dbo].[st_MagSimulationGetIds]    Script Date: 06/03/2023 10:56:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers 
-- =============================================
ALTER PROCEDURE [dbo].[st_MagSimulationGetIds] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int = 0
,	@ATTRIBUTE_ID_FILTER BIGINT = 0
,	@ATTRIBUTE_ID_SEED BIGINT = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here


	if not @ATTRIBUTE_ID_FILTER = 0
	begin
		if @ATTRIBUTE_ID_SEED = 0
		begin
			select DISTINCT imm.PaperId, 1 as Training from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER
			inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
			where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end
		else
		begin
			select DISTINCT imm.PaperId, case when iat.item_id is null then 0 else 1 end as Training from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER
			inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
			left outer join TB_ITEM_ATTRIBUTE iat on iat.ITEM_ID = imm.ITEM_ID and iat.ATTRIBUTE_ID = @ATTRIBUTE_ID_SEED
			left outer join tb_item_set iset on iset.ITEM_SET_ID = iat.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
			where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end
	end
	else
	begin
		if @ATTRIBUTE_ID_SEED = 0
		begin
			select DISTINCT imm.PaperId, 1 as Training from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
			where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end
		else
		begin
			select DISTINCT imm.PaperId, case when iat.item_id is null then 0 else 1 end as Training from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID and ir.REVIEW_ID = imm.REVIEW_ID
			left outer join TB_ITEM_ATTRIBUTE iat on iat.ITEM_ID = imm.ITEM_ID and iat.ATTRIBUTE_ID = @ATTRIBUTE_ID_SEED
			left outer join tb_item_set iset on iset.ITEM_SET_ID = iat.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
			where (imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
		end

	end
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_SVMReviewDatad]') AND type in (N'P', N'PC'))
DROP PROCEDURE st_SVMReviewData 
GO


/****** Object:  StoredProcedure [dbo].[st_TrainingWriteDataToAzure]    Script Date: 06/03/2023 11:00:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_TrainingWriteDataToAzure]
(
	@REVIEW_ID INT,
	@SCREENING_INDEXED BIT = 'FALSE'
--,	@SCREENING_DATA_FILE NVARCHAR(50)
)

As

SET NOCOUNT ON

	DELETE FROM TB_SCREENING_ML_TEMP WHERE REVIEW_ID = @REVIEW_ID

	SELECT DISTINCT @REVIEW_ID REVIEW_ID, tia.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '1' INCLUDED 
	FROM TB_ITEM_ATTRIBUTE tia
	INNER JOIN TB_ITEM_REVIEW ir on tia.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM I ON I.ITEM_ID = tia.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = tia.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		tia.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
	WHERE TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID
			
	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID REVIEW_ID, tia.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '0' INCLUDED  
	FROM TB_ITEM_ATTRIBUTE tia
	INNER JOIN TB_ITEM_REVIEW ir on tia.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM I ON I.ITEM_ID = tia.ITEM_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = tia.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		tia.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
	WHERE TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_REVIEW.ITEM_ID, TITLE, ABSTRACT, KEYWORDS, '99' INCLUDED FROM TB_ITEM_REVIEW
	INNER JOIN TB_ITEM I ON I.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		where TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND not TB_ITEM_REVIEW.ITEM_ID in
		(
			SELECT ITEM_ID FROM TB_ITEM_SET
				INNER JOIN TB_REVIEW ON TB_REVIEW.SCREENING_CODE_SET_ID = TB_ITEM_SET.SET_ID
				WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_REVIEW.REVIEW_ID = @REVIEW_ID
		)

	UPDATE TB_REVIEW
		SET SCREENING_INDEXED = @SCREENING_INDEXED,
			SCREENING_MODEL_RUNNING = @SCREENING_INDEXED
		WHERE REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF
GO

/****** Object:  StoredProcedure [dbo].[st_TrainingWriteIncludeExcludeToAzure]    Script Date: 06/03/2023 11:00:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_TrainingWriteIncludeExcludeToAzure]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	DELETE FROM TB_SCREENING_ML_TEMP WHERE REVIEW_ID = @REVIEW_ID

	SELECT DISTINCT @REVIEW_ID REVIEW_ID, tia.ITEM_ID, '1' INCLUDED  
	FROM TB_ITEM_ATTRIBUTE tia
	INNER JOIN TB_ITEM_REVIEW ir on tia.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = tia.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		tia.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
	WHERE TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID
			
	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID REVIEW_ID, tia.ITEM_ID, '0' INCLUDED  
	FROM TB_ITEM_ATTRIBUTE tia
	INNER JOIN TB_ITEM_REVIEW ir on tia.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = tia.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		tia.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
	WHERE TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID REVIEW_ID, TB_ITEM_REVIEW.ITEM_ID, '99' INCLUDED FROM TB_ITEM_REVIEW
		where TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND not TB_ITEM_REVIEW.ITEM_ID in
		(
			SELECT ITEM_ID FROM TB_ITEM_SET
				INNER JOIN TB_REVIEW ON TB_REVIEW.SCREENING_CODE_SET_ID = TB_ITEM_SET.SET_ID
				WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_REVIEW.REVIEW_ID = @REVIEW_ID
		)

	/*
	declare @tempTable table
	(
		REVIEW_ID int,
		ITEM_ID bigint,
		LABEL nvarchar(10)
	)
	
	delete from EPPI_ML.[EPPITest].dbo.TB_REVIEW_ITEM_LABELS WHERE REVIEW_ID = @REVIEW_ID

	INSERT INTO @tempTable(REVIEW_ID, ITEM_ID, LABEL)
	SELECT DISTINCT @REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, '1' FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'True'
	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID
			
	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, '0' FROM TB_ITEM_ATTRIBUTE
	INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	INNER JOIN TB_TRAINING_SCREENING_CRITERIA ON TB_TRAINING_SCREENING_CRITERIA.ATTRIBUTE_ID =
		TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_TRAINING_SCREENING_CRITERIA.INCLUDED = 'False'
	WHERE REVIEW_ID = @REVIEW_ID and TB_TRAINING_SCREENING_CRITERIA.REVIEW_ID = @REVIEW_ID

	UNION ALL
	
	SELECT DISTINCT @REVIEW_ID, TB_ITEM_REVIEW.ITEM_ID, '99' FROM TB_ITEM_REVIEW
		where TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE' AND not TB_ITEM_REVIEW.ITEM_ID in
		(
			SELECT ITEM_ID FROM TB_ITEM_SET
				INNER JOIN TB_REVIEW ON TB_REVIEW.SCREENING_CODE_SET_ID = TB_ITEM_SET.SET_ID
				WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND TB_REVIEW.REVIEW_ID = @REVIEW_ID
		)


	INSERT INTO EPPI_ML.[EPPITest].dbo.TB_REVIEW_ITEM_LABELS(REVIEW_ID, ITEM_ID, LABEL)
	select * from @tempTable
	*/
	
SET NOCOUNT OFF
GO


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BillMarkAsPaid]    Script Date: 06/03/2023 11:22:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[st_BillMarkAsPaid]
-- Add the parameters for the stored procedure here
@BILL_ID int
AS
	BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
	BEGIN TRY
	BEGIN TRANSACTION
	------------------------------------------------------------------------------------------------------------
	-- Existing Accounts
	--1a.collect data for TB_EXPIRY_EDIT_LOG
	CREATE TABLE #ExistingAccounts(DATE_OF_EDIT datetime, TYPE_EXTENDED int, ID_EXTENDED int,
	NEW_EXPIRY_DATE date, OLD_EXPIRY_DATE date, LENGTH_OF_EXTENSION int, EXTENDED_BY_ID int,
	EXTENSION_TYPE_ID int)
	INSERT INTO #ExistingAccounts (DATE_OF_EDIT, ID_EXTENDED, TYPE_EXTENDED, EXTENSION_TYPE_ID, LENGTH_OF_EXTENSION)
	Select GETDATE(), AFFECTED_ID as ID_EXTENDED, '1', '2', bl.MONTHS from TB_BILL_LINE bl
	inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
	and BILL_ID = @bill_ID and fs.TYPE_NAME = 'professional' and AFFECTED_ID is not null
	-- TYPE_EXTENDED = 1 for contacts
	update #ExistingAccounts set OLD_EXPIRY_DATE =
	(select c.EXPIRY_DATE from sTB_CONTACT c
	where c.CONTACT_ID = #ExistingAccounts.ID_EXTENDED)
	update #ExistingAccounts set EXTENDED_BY_ID =
	(select PURCHASER_CONTACT_ID from TB_BILL b
	inner join sTB_CONTACT c on c.CONTACT_ID = b.PURCHASER_CONTACT_ID
	where b.BILL_ID = @bill_ID)
	--1b.extend existing accounts
	update sTB_CONTACT set
	[EXPIRY_DATE] = case
	when ([EXPIRY_DATE] is null) then null
	when ([EXPIRY_DATE] > getdate()) then DATEADD(month, a.MONTHS, [EXPIRY_DATE])
	ELSE DATEADD(month, a.MONTHS, getdate())
	end
	, MONTHS_CREDIT = case when (EXPIRY_DATE is null and MONTHS_CREDIT is null)
	then MONTHS
	when (EXPIRY_DATE is null and MONTHS_CREDIT is not null) then MONTHS_CREDIT + a.MONTHS
	else 0
	end
	from (
	Select AFFECTED_ID, bl.MONTHS from TB_BILL_LINE bl
	inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
	and BILL_ID = @bill_ID and fs.TYPE_NAME = 'professional' and AFFECTED_ID is not null
	) a
	where CONTACT_ID = a.AFFECTED_ID
	--1c. update TB_EXPIRY_EDIT_LOG
	update #ExistingAccounts set NEW_EXPIRY_DATE =
	(select c.EXPIRY_DATE from sTB_CONTACT c
	where c.CONTACT_ID = #ExistingAccounts.ID_EXTENDED)
	insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
	OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID)
	select DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
	OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID
	from #ExistingAccounts
	drop table #ExistingAccounts
	------------------------------------------------------------------------------------------------------------
	-- Existing Reviews
	--2a.collect data for TB_EXPIRY_EDIT_LOG
	CREATE TABLE #ExistingReviews(DATE_OF_EDIT datetime, TYPE_EXTENDED int, ID_EXTENDED int,
	NEW_EXPIRY_DATE date, OLD_EXPIRY_DATE date, LENGTH_OF_EXTENSION int, EXTENDED_BY_ID int,
	EXTENSION_TYPE_ID int)
	INSERT INTO #ExistingReviews (DATE_OF_EDIT, ID_EXTENDED, TYPE_EXTENDED, EXTENSION_TYPE_ID, LENGTH_OF_EXTENSION)
	Select GETDATE(), AFFECTED_ID as ID_EXTENDED, '0', '2', bl.MONTHS from TB_BILL_LINE bl
	inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
	and BILL_ID = @bill_ID and fs.TYPE_NAME = 'Shareable Review' and AFFECTED_ID is not null
	-- TYPE_EXTENDED = 0 for reviews
	update #ExistingReviews set OLD_EXPIRY_DATE =
	(select r.EXPIRY_DATE from sTB_REVIEW r
	where r.REVIEW_ID = #ExistingReviews.ID_EXTENDED)
	update #ExistingReviews set EXTENDED_BY_ID = PURCHASER_CONTACT_ID from TB_BILL b
	where b.BILL_ID = @bill_ID
	--2b.extend existing reviews
	update sTB_REVIEW set
	[EXPIRY_DATE] = case
	When ([EXPIRY_DATE] is null) then null
	when ([EXPIRY_DATE] > getdate()) then DATEADD(month, a.MONTHS, [EXPIRY_DATE])
	ELSE DATEADD(month, a.MONTHS, getdate())
	end
	, MONTHS_CREDIT = Case When (EXPIRY_DATE is null and MONTHS_CREDIT is null) then a.MONTHS
	when (EXPIRY_DATE is null and MONTHS_CREDIT is not null) then MONTHS_CREDIT + a.MONTHS
	else 0
	end
	from (
	Select AFFECTED_ID, bl.MONTHS from TB_BILL_LINE bl
	inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
	and BILL_ID = @bill_ID and fs.TYPE_NAME = 'Shareable Review' and AFFECTED_ID is not null
	) a
	where REVIEW_ID = a.AFFECTED_ID
	--2c. update TB_EXPIRY_EDIT_LOG
	update #ExistingReviews set NEW_EXPIRY_DATE =
	(select r.EXPIRY_DATE from sTB_REVIEW r
	where r.REVIEW_ID = #ExistingReviews.ID_EXTENDED)
	insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
	OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID)
	select DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
	OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID
	from #ExistingReviews
	drop table #ExistingReviews
	------------------------------------------------------------------------------------------------------------
	--3.create accounts
	declare @bl int
	declare cr cursor FAST_FORWARD
	for select LINE_ID from tb_bill b
	inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID
	inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'professional'
	open cr
	fetch next from cr into @bl
	while @@fetch_status=0
	begin
	insert into sTB_CONTACT (CONTACT_NAME, [DATE_CREATED], [EXPIRY_DATE],
	MONTHS_CREDIT, CREATOR_ID, [TYPE], IS_SITE_ADMIN)
	Select Null ,getdate(), Null, MONTHS + 1, PURCHASER_CONTACT_ID, 'Professional', 0
	from TB_BILL b
	inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID and LINE_ID = @bl
	update TB_BILL_LINE set AFFECTED_ID = @@IDENTITY where LINE_ID = @bl
	fetch next from cr into @bl
	end
	close cr
	deallocate cr
	--4.create reviews
	declare cr cursor FAST_FORWARD
	for select LINE_ID from tb_bill b
	inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID
	inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Shareable Review'
	open cr
	fetch next from cr into @bl
	while @@fetch_status=0
	begin
	insert into sTB_REVIEW (REVIEW_NAME, [DATE_CREATED], [EXPIRY_DATE],
	MONTHS_CREDIT, FUNDER_ID)
	select Null, GETDATE(), Null, MONTHS, PURCHASER_CONTACT_ID
	from TB_BILL b
	inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID and LINE_ID = @bl
	update TB_BILL_LINE set AFFECTED_ID = @@IDENTITY where LINE_ID = @bl
	fetch next from cr into @bl
	end
	close cr
	deallocate cr
	---------------------------------------------------------------------------------------------
	--5. mark any outstanding fees as paid
	-- there should only be one line for outstanding fees in the bill as they get added up and put in one line
	declare @PurchaserContactID int
	set @PurchaserContactID = (select PURCHASER_CONTACT_ID from TB_BILL where BILL_ID = @bill_ID)
	declare @ForSaleID int
	set @ForSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Outstanding fee')
	select * from TB_BILL_LINE
	where BILL_ID = @bill_ID
	and FOR_SALE_ID = @ForSaleID
	if @@ROWCOUNT > 0
	begin
	-- there was an outstanding fee in the bill so mark them as paid in TB_OUTSTANDING_FEE
	update TB_OUTSTANDING_FEE
	set STATUS = 'Paid'
	where ACCOUNT_ID = @PurchaserContactID
	and STATUS like 'Outstanding'
	end
	--6. fill in TB_CREDIT_PURCHASE if a credit purchase was made
	set @PurchaserContactID = (select PURCHASER_CONTACT_ID from TB_BILL where BILL_ID = @bill_ID)
	set @ForSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Credit purchase')
	declare @amountPurchased int
	select * from TB_BILL_LINE
	where BILL_ID = @bill_ID
	and FOR_SALE_ID = @ForSaleID
	if @@ROWCOUNT > 0
	begin
	-- get the amount purchased
	set @amountPurchased = (select MONTHS from TB_BILL_LINE where BILL_ID = @bill_ID and FOR_SALE_ID = @ForSaleID)
	set @amountPurchased = @amountPurchased * 5
	-- there was a credit purchase in the bill so add it TB_CREDIT_PURCHASE
	insert into TB_CREDIT_PURCHASE (PURCHASER_CONTACT_ID, DATE_PURCHASED, CREDIT_PURCHASED, NOTES, PURCHASE_TYPE)
	values (@PurchaserContactID, getdate(), @amountPurchased, 'Online shop purchase', 'Shop')
	end
	--7.change bill to paid
	update TB_BILL set BILL_STATUS = 'OK: Paid and data committed', DATE_PURCHASED = GETDATE() where BILL_ID = @bill_ID
	COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
	IF (@@TRANCOUNT > 0)
	BEGIN
	--error corrections: 1.undo all changes
	ROLLBACK TRANSACTION
	--2.mark bill appropriately
	update TB_BILL set BILL_STATUS = 'FAILURE: paid but data NOT committed', DATE_PURCHASED = GETDATE() where BILL_ID = @bill_ID
	END
	END CATCH
END
GO

/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedDetails]    Script Date: 06/03/2023 11:25:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[st_ContactPurchasedDetails] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
        declare @PURCHASED_ACCOUNTS table ( 
	CONTACT_ID int, 
	CONTACT_NAME nchar(1000),
	EMAIL nvarchar(500),
	DATE_CREATED datetime,
	[EXPIRY_DATE] date,
	MONTHS_CREDIT smallint,
	CREATOR_ID int, 
	LAST_LOGIN datetime,
	SITE_LIC_ID int null,
	SITE_LIC_NAME nvarchar(50) null,
	FLAVOUR char(20) null,
	IS_FULLY_ACTIVE bit null,
	IS_STALE_AGHOST bit null)

	Insert Into @PURCHASED_ACCOUNTS (CONTACT_ID, CONTACT_NAME, EMAIL, DATE_CREATED, [EXPIRY_DATE],
					MONTHS_CREDIT, CREATOR_ID, SITE_LIC_ID, SITE_LIC_NAME, FLAVOUR, IS_FULLY_ACTIVE, IS_STALE_AGHOST)
	SELECT c.[CONTACT_ID], [CONTACT_NAME], EMAIL, c.[DATE_CREATED],
		 CASE when l.[EXPIRY_DATE] is not null 
				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then l.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end as 'EXPIRY_DATE',
		  [MONTHS_CREDIT], c.[CREATOR_ID], l.SITE_LIC_ID, l.SITE_LIC_NAME
		  ,FLAVOUR, 0, null
	  FROM sTB_CONTACT c 
	  left outer join sTB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID
	  left outer join sTB_SITE_LIC l on lc.SITE_LIC_ID = l.SITE_LIC_ID
	  where c.CREATOR_ID = @CONTACT_ID
	  and ((c.EXPIRY_DATE is null and MONTHS_CREDIT != 0)
			or
			(c.EXPIRY_DATE is not null))
	and c.CONTACT_ID != @CONTACT_ID

	update  p_a --add usage info
	set p_a.LAST_LOGIN = l_t.CREATED
	--max(l_t.CREATED)
	from @PURCHASED_ACCOUNTS p_a, TB_LOGON_TICKET l_t
	where l_t.CREATED in
	(select max(l_t.CREATED)from TB_LOGON_TICKET l_t
	where p_a.CONTACT_ID = l_t.CONTACT_ID)
	
	--set whether the account is fully active (the person who purchased it won't be able to edit the account details)
	update t set IS_FULLY_ACTIVE = 1
	from @PURCHASED_ACCOUNTS t where EMAIL is not null 
			and CONTACT_NAME is not null and [EXPIRY_DATE] is not null and FLAVOUR is not null
	
	--we need to figure out if ghost accounts awaiting for activation are out of time and need re-activation
	declare @DATES table (CONTACT_ID int, MDATE Datetime2(1))
			
	insert into @DATES
		SELECT t.CONTACT_ID,  MAX(c.DATE_CREATED)
	from @PURCHASED_ACCOUNTS t inner join TB_CHECKLINK c on t.CONTACT_ID = c.CONTACT_ID
											and c.TYPE = 'ActivateGhost'
											and t.IS_FULLY_ACTIVE = 0
		group by t.CONTACT_ID, c.TYPE, t.IS_FULLY_ACTIVE
	update t set IS_STALE_AGHOST = 1
	from @PURCHASED_ACCOUNTS t inner join @DATES d on t.CONTACT_ID = d.CONTACT_ID and DATEDIFF(D, d.MDATE, GETDATE()) > 14
	
	--mark ghost account that are waiting for the end user to activate them but are still in time to do so
	update t set IS_STALE_AGHOST = 0
	from @PURCHASED_ACCOUNTS t where t.CONTACT_NAME is null and t.FLAVOUR is null and IS_STALE_AGHOST is null and EMAIL is not null
	--email field is populated to send the activation request to the intended user
	
	select CONTACT_ID , 
	CONTACT_NAME ,
	EMAIL ,
	DATE_CREATED ,
	[EXPIRY_DATE] ,
	MONTHS_CREDIT ,
	CREATOR_ID , 
	LAST_LOGIN ,
	SITE_LIC_ID ,
	SITE_LIC_NAME  ,
	IS_FULLY_ACTIVE ,
	IS_STALE_AGHOST 
	 from @PURCHASED_ACCOUNTS
    

	RETURN

END
GO
/****** Object:  StoredProcedure [dbo].[st_ContactReviewsAdmin]    Script Date: 06/03/2023 11:26:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
ALTER     PROCEDURE [dbo].[st_ContactReviewsAdmin] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
    declare @PURCHASED_REVIEWS table ( 
	REVIEW_ID int, 
	REVIEW_NAME nchar(1000),
	DATE_CREATED datetime,
	[EXPIRY_DATE] date,
	MONTHS_CREDIT smallint,
	FUNDER_ID int, 
	LAST_LOGIN datetime,
	SITE_LIC_ID int null,
	SITE_LIC_NAME nvarchar(50) null)

/*
Insert Into @PURCHASED_REVIEWS (REVIEW_ID, REVIEW_NAME, DATE_CREATED, [EXPIRY_DATE],
				MONTHS_CREDIT, FUNDER_ID)
SELECT [REVIEW_ID], [REVIEW_NAME], [DATE_CREATED], [EXPIRY_DATE], [MONTHS_CREDIT], [FUNDER_ID]
  FROM sTB_REVIEW where FUNDER_ID = @CONTACT_ID
  and ((EXPIRY_DATE is null and MONTHS_CREDIT != 0)
		or
		(EXPIRY_DATE is not null))
*/

Insert Into @PURCHASED_REVIEWS (REVIEW_ID, REVIEW_NAME, DATE_CREATED, [EXPIRY_DATE],
				MONTHS_CREDIT, FUNDER_ID)
SELECT distinct r.[REVIEW_ID], r.[REVIEW_NAME], r.[DATE_CREATED], r.[EXPIRY_DATE], 
r.[MONTHS_CREDIT], r.[FUNDER_ID]
  FROM sTB_REVIEW r
left outer join sTB_REVIEW_CONTACT r_c
on r.REVIEW_ID = r_c.REVIEW_ID
left outer join sTB_CONTACT_REVIEW_ROLE c_r_r
on r_c.REVIEW_CONTACT_ID = c_r_r.REVIEW_CONTACT_ID
and ((r_c.CONTACT_ID = @CONTACT_ID and c_r_r.ROLE_NAME = 'AdminUser') 
		or (r.FUNDER_ID = @CONTACT_ID))
where ((r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0 )
		or (r.EXPIRY_DATE is not null)
		or (r.EXPIRY_DATE is null)) 
		and 
		(r.FUNDER_ID = @CONTACT_ID or (c_r_r.ROLE_NAME is not null and ROLE_NAME = 'AdminUser')) 

update  p_r
set p_r.LAST_LOGIN = 
l_t.CREATED
--max(l_t.CREATED)
from @PURCHASED_REVIEWS p_r, TB_LOGON_TICKET l_t
where l_t.CREATED in
(select max(l_t.CREATED)from TB_LOGON_TICKET l_t
where p_r.FUNDER_ID = l_t.CONTACT_ID
and p_r.REVIEW_ID = l_t.REVIEW_ID)

update @PURCHASED_REVIEWS set SITE_LIC_ID = a.site_lic_id, SITE_LIC_NAME = a.SITE_LIC_NAME
	, [EXPIRY_DATE] = case when L_EXP is not null and [EXPIRY_DATE] > L_EXP  then [EXPIRY_DATE] 
						else L_EXP
					end
from (select ld.SITE_LIC_ID, l.SITE_LIC_NAME, lr.REVIEW_ID as REV_ID , l.[EXPIRY_DATE] as L_EXP
	from TB_SITE_LIC_DETAILS ld inner join sTB_SITE_LIC_REVIEW lr on ld.SITE_LIC_ID = lr.SITE_LIC_ID
	inner join @PURCHASED_REVIEWS pr on pr.REVIEW_ID = lr.REVIEW_ID
	inner join sTB_SITE_LIC l on lr.SITE_LIC_ID = l.SITE_LIC_ID) as a
where a.REV_ID = REVIEW_ID
select * from @PURCHASED_REVIEWS
    
END
GO
/****** Object:  StoredProcedure [dbo].[st_ContactReviewsShareable]    Script Date: 06/03/2023 11:27:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
ALTER     PROCEDURE [dbo].[st_ContactReviewsShareable] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
    declare @PURCHASED_REVIEWS table ( 
	REVIEW_ID int, 
	REVIEW_NAME nchar(1000),
	DATE_CREATED datetime,
	[EXPIRY_DATE] date,
	MONTHS_CREDIT smallint,
	FUNDER_ID int, 
	LAST_LOGIN datetime,
	SITE_LIC_ID int null,
	SITE_LIC_NAME nvarchar(50) null)

/*
Insert Into @PURCHASED_REVIEWS (REVIEW_ID, REVIEW_NAME, DATE_CREATED, [EXPIRY_DATE],
				MONTHS_CREDIT, FUNDER_ID)
SELECT [REVIEW_ID], [REVIEW_NAME], [DATE_CREATED], [EXPIRY_DATE], [MONTHS_CREDIT], [FUNDER_ID]
  FROM sTB_REVIEW where FUNDER_ID = @CONTACT_ID
  and ((EXPIRY_DATE is null and MONTHS_CREDIT != 0)
		or
		(EXPIRY_DATE is not null))
*/

Insert Into @PURCHASED_REVIEWS (REVIEW_ID, REVIEW_NAME, DATE_CREATED, [EXPIRY_DATE],
				MONTHS_CREDIT, FUNDER_ID)
SELECT distinct r.[REVIEW_ID], r.[REVIEW_NAME], r.[DATE_CREATED], r.[EXPIRY_DATE], 
r.[MONTHS_CREDIT], r.[FUNDER_ID]
  FROM sTB_REVIEW r
left outer join sTB_REVIEW_CONTACT r_c
on r.REVIEW_ID = r_c.REVIEW_ID
left outer join sTB_CONTACT_REVIEW_ROLE c_r_r
on r_c.REVIEW_CONTACT_ID = c_r_r.REVIEW_CONTACT_ID
and ((r_c.CONTACT_ID = @CONTACT_ID and c_r_r.ROLE_NAME = 'AdminUser') 
		or (r.FUNDER_ID = @CONTACT_ID))
where ((r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0 )
		or (r.EXPIRY_DATE is not null)) 
		and 
		(r.FUNDER_ID = @CONTACT_ID or (c_r_r.ROLE_NAME is not null and ROLE_NAME = 'AdminUser')) 

update  p_r
set p_r.LAST_LOGIN = 
l_t.CREATED
--max(l_t.CREATED)
from @PURCHASED_REVIEWS p_r, TB_LOGON_TICKET l_t
where l_t.CREATED in
(select max(l_t.CREATED)from TB_LOGON_TICKET l_t
where p_r.FUNDER_ID = l_t.CONTACT_ID
and p_r.REVIEW_ID = l_t.REVIEW_ID)

update @PURCHASED_REVIEWS set SITE_LIC_ID = a.site_lic_id, SITE_LIC_NAME = a.SITE_LIC_NAME
	, [EXPIRY_DATE] = case when L_EXP is not null and [EXPIRY_DATE] > L_EXP  then [EXPIRY_DATE] 
						else L_EXP
					end
from (select ld.SITE_LIC_ID, l.SITE_LIC_NAME, lr.REVIEW_ID as REV_ID , l.[EXPIRY_DATE] as L_EXP
	from TB_SITE_LIC_DETAILS ld inner join sTB_SITE_LIC_REVIEW lr on ld.SITE_LIC_ID = lr.SITE_LIC_ID
	inner join @PURCHASED_REVIEWS pr on pr.REVIEW_ID = lr.REVIEW_ID
	inner join sTB_SITE_LIC l on lr.SITE_LIC_ID = l.SITE_LIC_ID) as a
where a.REV_ID = REVIEW_ID
select * from @PURCHASED_REVIEWS
order by REVIEW_ID desc
    
END
---------------------------------------------
GO
/****** Object:  StoredProcedure [dbo].[st_CopyCodeset]    Script Date: 06/03/2023 11:28:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   procedure [dbo].[st_CopyCodeset]
(
	@SOURCE_SET_ID int,
	@SOURCE_REVIEW_ID int,
	@DESTINATION_REVIEW_ID int,
	@CONTACT_ID int,
	@RESULT nvarchar(50) output
)
As
SET NOCOUNT ON
declare @DESTINATION_SET_ID int
declare @DESTINATION_REVIEW_SET_ID int
declare @DESTINATION_SET_NAME nvarchar (255)
set @RESULT = '0'

BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION

--01 create new row in sTB_SET and get @DESTINATION_SET_ID
	insert into sTB_SET (SET_TYPE_ID, SET_NAME)
	select s.SET_TYPE_ID, s.SET_NAME from sTB_SET s
	inner join sTB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
	where s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID
	set @DESTINATION_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

	-- prefix the new codeset with 'COPY - ' BUT first remove
	--  the 'LINKED - ' prefix if it exists
	select @DESTINATION_SET_NAME = SET_NAME
	from sTB_SET
	where SET_ID = @DESTINATION_SET_ID

	declare @Prefix nvarchar(6) 
	set @Prefix = substring (@DESTINATION_SET_NAME, 0, 7) 
	if @Prefix = 'LINKED'
	begin
		set @DESTINATION_SET_NAME = substring (@DESTINATION_SET_NAME, 9, len(@DESTINATION_SET_NAME) - 8)
	end
	
	update sTB_SET
	set SET_NAME = 'COPY - ' + @DESTINATION_SET_NAME
	where SET_ID = @DESTINATION_SET_ID

--02 create new row in sTB_REVIEW_SET and get @DESTINATION_REVIEW_SET_ID
	insert into sTB_REVIEW_SET (REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL)
	values (@DESTINATION_REVIEW_ID, @DESTINATION_SET_ID, 1, 1)
	set @DESTINATION_REVIEW_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

	-- @attribute_id
	declare @attribute_id table (attribute_id int, ok int)

--03 get source ATTRIBUTE_ID's and put in tmp table @attribute_id (avoids orphans)	
	insert into @attribute_id 
	Select a_s.ATTRIBUTE_ID, dbo.sfn_IsAttributeInTree(a_s.ATTRIBUTE_ID ) as ok 
	from sTB_ATTRIBUTE_SET a_s 
	inner join sTB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
	where a_s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID 
	
	--A
	--select * from @attribute_id
	
	-- @tb_attribute
	declare @tb_attribute table 
	(ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))
	
--04 use @attribute_id to get source data from TB_ATTRIBUTE and put in tmp table @tb_attribute
	insert into @tb_attribute (ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
		select ATTRIBUTE_ID, @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, ATTRIBUTE_ID -- use ATTRIBUTE_ID as OLD_ATTRIBUTE_ID
		from sTB_ATTRIBUTE
		where ATTRIBUTE_ID in 
		(select attribute_id from @attribute_id)

	--B
	--select * from @tb_attribute

	-- @tb_attribute_set
	declare @tb_attribute_set table 
	(ATTRIBUTE_SET_ID int, ATTRIBUTE_ID int, SET_ID int, PARENT_ATTRIBUTE_ID int, ATTRIBUTE_TYPE_ID int, 
	ATTRIBUTE_SET_DESC nvarchar(max), ATTRIBUTE_ORDER int)

--05 use @attribute_id to get source data from TB_ATTRIBUTE_SET and put in tmp table @tb_attribute_set	
	insert into @tb_attribute_set (ATTRIBUTE_SET_ID, ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)
		select ATTRIBUTE_SET_ID, ATTRIBUTE_ID, @DESTINATION_SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
		from sTB_ATTRIBUTE_SET
		where ATTRIBUTE_ID in 
		(select attribute_id from @attribute_id)
		-- 20/09/2012 JB added to avoid any orphan set_id's that are not in sTB_REVIEW_SET
		and SET_ID in (select SET_ID from sTB_REVIEW_SET)

	--C
	--select * from @tb_attribute_set

--06 put @tb_attribute into sTB_ATTRIBUTE
	insert into sTB_ATTRIBUTE (CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)	
		select @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID 
		from @tb_attribute

	-- @new_tb_attribute
	declare @new_tb_attribute table 
	(NEW_ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))

--07 get the new rows from sTB_ATTRIBUTE and put them into @new_tb_attribute
	insert into @new_tb_attribute (NEW_ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
		select ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID  
		from sTB_ATTRIBUTE
		where OLD_ATTRIBUTE_ID in -- old_attribute_id will identify the items we want
		(select attribute_id from @attribute_id) --wrong! you are getting attributes from other set_id's
		and OLD_ATTRIBUTE_ID not like 'AT%'
		and OLD_ATTRIBUTE_ID not like 'EX%'
		and OLD_ATTRIBUTE_ID not like 'IC%'

	--D
	--select * from @new_tb_attribute


--09 update @tb_attribute_set with the new ATTRIBUTE_IDs sitting in @new_tb_attribute
	update @tb_attribute_set 
	set ATTRIBUTE_ID = 
	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
	where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
	where exists
	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
		where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
	
	--E
	--select * from @tb_attribute_set

--08 update @tb_attribute_set with a new PARENT_ATTRIBUTE_ID
	-- the new PARENT_ATTRIBUTE_ID is the 
	update @tb_attribute_set
	set PARENT_ATTRIBUTE_ID = 
	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
	where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID
	and PARENT_ATTRIBUTE_ID != 0)
	where exists
	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
		where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID and PARENT_ATTRIBUTE_ID != 0)
	
	
	-- clean up the nulls
	update @tb_attribute_set
	set PARENT_ATTRIBUTE_ID = 0
	where PARENT_ATTRIBUTE_ID is null
	
	--F
	--select * from @tb_attribute_set
	


--10 put @tb_attribute_set into sTB_ATTRIBUTE_SET
	insert into sTB_ATTRIBUTE_SET (ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)	
		select ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
		from @tb_attribute_set

--11 place the new codeset at the bottom of the list.
	declare @number_sets int set @number_sets = 0
	select @number_sets = COUNT(*) from sTB_REVIEW_SET
	where REVIEW_ID = @DESTINATION_REVIEW_ID
	-- set the position
	update sTB_REVIEW_SET 
	set SET_ORDER = @number_sets - 1
	where REVIEW_ID = @DESTINATION_REVIEW_ID
	and SET_ID = @DESTINATION_SET_ID

--12 Clear out OLD_ATTRIBUTE_ID from TB_ATTRIBUTE in the new codeset since
	update sTB_ATTRIBUTE
	set OLD_ATTRIBUTE_ID = null
	where ATTRIBUTE_ID in 
	(select ATTRIBUTE_ID from sTB_ATTRIBUTE_SET
		where SET_ID = @DESTINATION_SET_ID)
	

COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to copy. Please contact EPPI-Support'
end
END CATCH


RETURN

SET NOCOUNT OFF
GO

/****** Object:  StoredProcedure [dbo].[st_CopyCodeset_times_out]    Script Date: 06/03/2023 11:30:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_CopyCodeset_times_out]
(
	@SOURCE_SET_ID int,
	@SOURCE_REVIEW_ID int,
	@DESTINATION_REVIEW_ID int,
	@CONTACT_ID int,
	@RESULT nvarchar(50) output
)
As
SET NOCOUNT ON

declare @DESTINATION_SET_ID int
declare @DESTINATION_REVIEW_SET_ID int
declare @DESTINATION_SET_NAME nvarchar (255)
set @RESULT = '0'
-- tables inserted into
--- sTB_SET
--- sTB_REVIEW_SET
--- sTB_ATTRIBUTE
--- sTB_ATTRIBUTE_SET

BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION

	-- create new row in sTB_SET and get @DESTINATION_SET_ID
	insert into sTB_SET (SET_TYPE_ID, SET_NAME)
	select s.SET_TYPE_ID, s.SET_NAME from sTB_SET s
	inner join sTB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
	where s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID
	set @DESTINATION_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

	-- prefix the new codeset with 'COPY - ' BUT first remove
	--  the 'LINKED - ' prefix if it exists
	select @DESTINATION_SET_NAME = SET_NAME
	from sTB_SET
	where SET_ID = @DESTINATION_SET_ID

	declare @Prefix nvarchar(6) 
	set @Prefix = substring (@DESTINATION_SET_NAME, 0, 7) 
	if @Prefix = 'LINKED'
	begin
		set @DESTINATION_SET_NAME = substring (@DESTINATION_SET_NAME, 9, len(@DESTINATION_SET_NAME) - 8)
	end
	
	update sTB_SET
	set SET_NAME = 'COPY - ' + @DESTINATION_SET_NAME
	where SET_ID = @DESTINATION_SET_ID

	-- create new row in sTB_REVIEW_SET and get @DESTINATION_REVIEW_SET_ID
	insert into sTB_REVIEW_SET (REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL)
	values (@DESTINATION_REVIEW_ID, @DESTINATION_SET_ID, 1, 1)
	set @DESTINATION_REVIEW_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

	-- use a temp table to hold the items the cursor will work with

	declare @attribute_id table (attribute_id int, ok int)
	insert into @attribute_id 
	Select a_s.ATTRIBUTE_ID, dbo.sfn_IsAttributeInTree(a_s.ATTRIBUTE_ID ) as ok 
	from sTB_ATTRIBUTE_SET a_s 
	inner join sTB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
	where a_s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID 


	declare @WORKING_ATTRIBUTE_ID nvarchar(50)
	declare @NEW_ATTRIBUTE_ID nvarchar(50)
	declare @WORKING_PARENT_ATTRIBUTE_ID nvarchar(50)
	declare @NEW_PARENT_ATTRIBUTE_ID bigint

	-- create new rows in TB_ATTRIBUTE based source codeset
	-- OLD_ATTRIBUTE_ID will temporarily hold the link between the old and new codeset
	declare ATTRIBUTE_ID_CURSOR cursor for
	select attribute_id from @attribute_id
	/*
	declare ATTRIBUTE_ID_CURSOR cursor for
	select a.ATTRIBUTE_ID from
	(Select a_s.ATTRIBUTE_ID, sfn_IsAttributeInTree(a_s.ATTRIBUTE_ID ) as ok from sTB_ATTRIBUTE_SET a_s 
	inner join sTB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
	where a_s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID
	) as a
	where a.ok = 1
	*/
	declare @TB_ATTRIBUTE table 
	(CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))
	
	open ATTRIBUTE_ID_CURSOR
	fetch next from ATTRIBUTE_ID_CURSOR
	into @WORKING_ATTRIBUTE_ID
	while @@FETCH_STATUS = 0
	begin
		--insert into sTB_ATTRIBUTE (CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)	
		insert into @TB_ATTRIBUTE (CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
		select @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, @WORKING_ATTRIBUTE_ID 
		from sTB_ATTRIBUTE
		where ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID

		fetch next from ATTRIBUTE_ID_CURSOR 
		into @WORKING_ATTRIBUTE_ID
	end

	close ATTRIBUTE_ID_CURSOR
	deallocate ATTRIBUTE_ID_CURSOR

	-- create new row in TB_ATTRIBUTE_SET using OLD_ATTRIBUTE_ID to link old and new
	declare ATTRIBUTE_ID_CURSOR cursor for
	select attribute_id from @attribute_id
	/*
	declare ATTRIBUTE_ID_CURSOR cursor for
	select a1.ATTRIBUTE_ID from
	(Select s.ATTRIBUTE_ID, sfn_IsAttributeInTree(s.ATTRIBUTE_ID) as ok1 from sTB_ATTRIBUTE_SET s 
	inner join sTB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
	where r_s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID
	) as a1
	where a1.ok1 = 1
	*/

	declare @TB_ATTRIBUTE_SET table 
	(ATTRIBUTE_ID int, SET_ID int, PARENT_ATTRIBUTE_ID int, ATTRIBUTE_TYPE_ID int, ATTRIBUTE_SET_DESC nvarchar(max), ATTRIBUTE_ORDER int)
	
	open ATTRIBUTE_ID_CURSOR
	fetch next from ATTRIBUTE_ID_CURSOR
	into @WORKING_ATTRIBUTE_ID
	while @@FETCH_STATUS = 0
	begin
		select @NEW_ATTRIBUTE_ID = ATTRIBUTE_ID from sTB_ATTRIBUTE 
		where OLD_ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
		-- handling the strange OLD_ATTRIBUTE_IDs from ER3
		and OLD_ATTRIBUTE_ID not like 'AT%'
		and OLD_ATTRIBUTE_ID not like 'EX%'
		and OLD_ATTRIBUTE_ID not like 'IC%'

		--insert into sTB_ATTRIBUTE_SET (ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)	
		insert into @TB_ATTRIBUTE_SET (ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)
		select @NEW_ATTRIBUTE_ID, @DESTINATION_SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
		from sTB_ATTRIBUTE_SET
		where ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID

		fetch next from ATTRIBUTE_ID_CURSOR 
		into @WORKING_ATTRIBUTE_ID
	end

	close ATTRIBUTE_ID_CURSOR
	deallocate ATTRIBUTE_ID_CURSOR

	--drop table #attribute_id

	-- update PARENT_ATTRIBUTE_ID from TB_ATTRIBUTE_SET in the new codeset. 
	-- Ignore the ones that are 0's (zero) as they are top level
	declare ATTRIBUTE_ID_CURSOR cursor for
	select ATTRIBUTE_ID from @TB_ATTRIBUTE_SET where SET_ID = @DESTINATION_SET_ID
	--select ATTRIBUTE_ID from sTB_ATTRIBUTE_SET where SET_ID = @DESTINATION_SET_ID
	open ATTRIBUTE_ID_CURSOR
	fetch next from ATTRIBUTE_ID_CURSOR
	into @WORKING_ATTRIBUTE_ID
	while @@FETCH_STATUS = 0
	begin	
		--select @WORKING_PARENT_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID from sTB_ATTRIBUTE_SET
		select @WORKING_PARENT_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID from @TB_ATTRIBUTE_SET 
			where ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
		if @WORKING_PARENT_ATTRIBUTE_ID != 0
		begin
			update sTB_ATTRIBUTE_SET
			set PARENT_ATTRIBUTE_ID = 
				(select ATTRIBUTE_ID from sTB_ATTRIBUTE 
				where OLD_ATTRIBUTE_ID = @WORKING_PARENT_ATTRIBUTE_ID
				-- handling the strange OLD_ATTRIBUTE_IDs from ER3
					and OLD_ATTRIBUTE_ID not like 'AT%'
					and OLD_ATTRIBUTE_ID not like 'EX%'
					and OLD_ATTRIBUTE_ID not like 'IC%')
			where SET_ID = @DESTINATION_SET_ID
			and ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
		end

		fetch next from ATTRIBUTE_ID_CURSOR 
		into @WORKING_ATTRIBUTE_ID
	end

	close ATTRIBUTE_ID_CURSOR
	deallocate ATTRIBUTE_ID_CURSOR

	-- place the new codeset at the bottom of the list.
	declare @number_sets int set @number_sets = 0
	select @number_sets = COUNT(*) from sTB_REVIEW_SET
	where REVIEW_ID = @DESTINATION_REVIEW_ID
	-- set the position
	update sTB_REVIEW_SET 
	set SET_ORDER = @number_sets - 1
	where REVIEW_ID = @DESTINATION_REVIEW_ID
	and SET_ID = @DESTINATION_SET_ID

	-- Clear out OLD_ATTRIBUTE_ID from TB_ATTRIBUTE in the new codeset since
	-- it is an editable copy with no connection to the original
	update sTB_ATTRIBUTE
	set OLD_ATTRIBUTE_ID = null
	where ATTRIBUTE_ID in 
	(select ATTRIBUTE_ID from sTB_ATTRIBUTE_SET
		where SET_ID = @DESTINATION_SET_ID)	

COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to copy codeset. Please contact EPPI-Support'
end
END CATCH


RETURN

SET NOCOUNT OFF
GO

/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep09]    Script Date: 06/03/2023 11:32:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or ALTER   procedure [dbo].[st_CopyReviewStep09]
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
--declare @SOURCE_REVIEW_ID int
set @RESULT = '0'


BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION

--00 get example review ID
	--select @SOURCE_REVIEW_ID = EXAMPLE_NON_SHAREABLE_REVIEW_ID from TB_MANAGEMENT_SETTINGS

--01 create new row in sTB_SET and get @DESTINATION_SET_ID
	insert into sTB_SET (SET_TYPE_ID, SET_NAME, OLD_GUIDELINE_ID)
	select s.SET_TYPE_ID, s.SET_NAME, @SOURCE_SET_ID from sTB_SET s
	inner join sTB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
	where s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID
	set @DESTINATION_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

--02 create new row in sTB_REVIEW_SET and get @DESTINATION_REVIEW_SET_ID
	insert into sTB_REVIEW_SET (REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL)
	values (@DESTINATION_REVIEW_ID, @DESTINATION_SET_ID, 1, 1)
	set @DESTINATION_REVIEW_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

	-- @attribute_id
	declare @attribute_id table (attribute_id int, ok int)

--03 get source ATTRIBUTE_ID's and put in tmp table @attribute_id (avoids orphans)	
	insert into @attribute_id 
	Select a_s.ATTRIBUTE_ID, dbo.sfn_IsAttributeInTree(a_s.ATTRIBUTE_ID ) as ok 
	from sTB_ATTRIBUTE_SET a_s 
	inner join sTB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
	where a_s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID 
	
	--A
	--select * from @attribute_id
	
	-- @tb_attribute
	declare @tb_attribute table 
	(ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))
	
--04 use @attribute_id to get source data from TB_ATTRIBUTE and put in tmp table @tb_attribute
	insert into @tb_attribute (ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
		select ATTRIBUTE_ID, @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, ATTRIBUTE_ID -- use ATTRIBUTE_ID as OLD_ATTRIBUTE_ID
		from sTB_ATTRIBUTE
		where ATTRIBUTE_ID in 
		(select attribute_id from @attribute_id)

	--B
	--select * from @tb_attribute

	-- @tb_attribute_set
	declare @tb_attribute_set table 
	(ATTRIBUTE_SET_ID int, ATTRIBUTE_ID int, SET_ID int, PARENT_ATTRIBUTE_ID int, ATTRIBUTE_TYPE_ID int, 
	ATTRIBUTE_SET_DESC nvarchar(max), ATTRIBUTE_ORDER int)

--05 use @attribute_id to get source data from TB_ATTRIBUTE_SET and put in tmp table @tb_attribute_set	
	insert into @tb_attribute_set (ATTRIBUTE_SET_ID, ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)
		select ATTRIBUTE_SET_ID, ATTRIBUTE_ID, @DESTINATION_SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
		from sTB_ATTRIBUTE_SET
		where ATTRIBUTE_ID in 
		(select attribute_id from @attribute_id)
		-- 20/09/2012 JB added to avoid any orphan set_id's that are not in sTB_REVIEW_SET
		and SET_ID in (select SET_ID from sTB_REVIEW_SET)

	--C
	--select * from @tb_attribute_set

--06 put @tb_attribute into sTB_ATTRIBUTE
	insert into sTB_ATTRIBUTE (CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)	
		select @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID 
		from @tb_attribute

	-- @new_tb_attribute
	declare @new_tb_attribute table 
	(NEW_ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))

--07 get the new rows from sTB_ATTRIBUTE and put them into @new_tb_attribute
	insert into @new_tb_attribute (NEW_ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
		select ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID  
		from sTB_ATTRIBUTE
		where OLD_ATTRIBUTE_ID in -- old_attribute_id will identify the items we want
		(select attribute_id from @attribute_id) --does @attribute_id have bad data?
		and OLD_ATTRIBUTE_ID not like 'AT%'
		and OLD_ATTRIBUTE_ID not like 'EX%'
		and OLD_ATTRIBUTE_ID not like 'IC%'
		--and ATTRIBUTE_ID in (select ATTRIBUTE_ID from sTB_ATTRIBUTE_SET
		--where SET_ID = @DESTINATION_SET_ID)
		-- added by JB 21/04/15 to be sure we are only getting data for this review
		and CONTACT_ID = @CONTACT_ID
		
--- we need to restrict this to the correct set but we haven't populated
--- sTB_ATTRIBUTE_SET yet!!!!!!!!!!!!!!!!!		
		
	--D
	--select * from @new_tb_attribute

--09 update @tb_attribute_set with the new ATTRIBUTE_IDs sitting in @new_tb_attribute
	update @tb_attribute_set 
	set ATTRIBUTE_ID = 
	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
	where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
	where exists
	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
		where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
	
	--E
	--select * from @tb_attribute_set

--08 update @tb_attribute_set with a new PARENT_ATTRIBUTE_ID
	-- the new PARENT_ATTRIBUTE_ID is the 
	update @tb_attribute_set
	set PARENT_ATTRIBUTE_ID = 
	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
	where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID
	and PARENT_ATTRIBUTE_ID != 0)
	where exists
	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
		where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID and PARENT_ATTRIBUTE_ID != 0)
	
	-- clean up the nulls
	update @tb_attribute_set
	set PARENT_ATTRIBUTE_ID = 0
	where PARENT_ATTRIBUTE_ID is null
	
	--F
	--select * from @tb_attribute_set

--10 put @tb_attribute_set into sTB_ATTRIBUTE_SET
	insert into sTB_ATTRIBUTE_SET (ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)	
		select ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
		from @tb_attribute_set

--11 place the new codeset at the bottom of the list.
	declare @number_sets int set @number_sets = 0
	select @number_sets = COUNT(*) from sTB_REVIEW_SET
	where REVIEW_ID = @DESTINATION_REVIEW_ID
	-- set the position
	update sTB_REVIEW_SET
	set SET_ORDER = @number_sets - 1
	where REVIEW_ID = @DESTINATION_REVIEW_ID
	and SET_ID = @DESTINATION_SET_ID

-- keep the old_attribute_id values as we need to copy over the data
/*
--12 Clear out OLD_ATTRIBUTE_ID from TB_ATTRIBUTE in the new codeset since
	update sTB_ATTRIBUTE
	set OLD_ATTRIBUTE_ID = null
	where ATTRIBUTE_ID in 
	(select ATTRIBUTE_ID from sTB_ATTRIBUTE_SET
		where SET_ID = @DESTINATION_SET_ID)
*/

COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to set up copies of the source codesets in the new review'
end
END CATCH

RETURN

SET NOCOUNT OFF
GO

/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep09altVersion]    Script Date: 06/03/2023 11:34:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_CopyReviewStep09altVersion]
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
--declare @SOURCE_REVIEW_ID int
set @RESULT = '0'

BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION

--00 get example review ID
	--select @SOURCE_REVIEW_ID = EXAMPLE_NON_SHAREABLE_REVIEW_ID from TB_MANAGEMENT_SETTINGS

--01 create new row in sTB_SET and get @DESTINATION_SET_ID
	insert into sTB_SET (SET_TYPE_ID, SET_NAME, OLD_GUIDELINE_ID)
	select s.SET_TYPE_ID, s.SET_NAME, @SOURCE_SET_ID from sTB_SET s
	inner join sTB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
	where s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID
	set @DESTINATION_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

--02 create new row in sTB_REVIEW_SET and get @DESTINATION_REVIEW_SET_ID
	insert into sTB_REVIEW_SET (REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL)
	values (@DESTINATION_REVIEW_ID, @DESTINATION_SET_ID, 1, 1)
	set @DESTINATION_REVIEW_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

	-- @attribute_id
	declare @attribute_id table (attribute_id int, ok int)

--03 get source ATTRIBUTE_ID's and put in tmp table @attribute_id (avoids orphans)	
	insert into @attribute_id 
	Select a_s.ATTRIBUTE_ID, dbo.sfn_IsAttributeInTree(a_s.ATTRIBUTE_ID ) as ok 
	from sTB_ATTRIBUTE_SET a_s 
	inner join sTB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
	where a_s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID 
	
	--A
	--select * from @attribute_id
	
	-- @tb_attribute
	declare @tb_attribute table 
	(ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))
	
--04 use @attribute_id to get source data from TB_ATTRIBUTE and put in tmp table @tb_attribute
	insert into @tb_attribute (ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
		select ATTRIBUTE_ID, @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, ATTRIBUTE_ID -- use ATTRIBUTE_ID as OLD_ATTRIBUTE_ID
		from sTB_ATTRIBUTE
		where ATTRIBUTE_ID in 
		(select attribute_id from @attribute_id)

	--B
	--select * from @tb_attribute

	-- @tb_attribute_set
	declare @tb_attribute_set table 
	(ATTRIBUTE_SET_ID int, ATTRIBUTE_ID int, SET_ID int, PARENT_ATTRIBUTE_ID int, ATTRIBUTE_TYPE_ID int, 
	ATTRIBUTE_SET_DESC nvarchar(max), ATTRIBUTE_ORDER int)

--05 use @attribute_id to get source data from TB_ATTRIBUTE_SET and put in tmp table @tb_attribute_set	
	insert into @tb_attribute_set (ATTRIBUTE_SET_ID, ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)
		select ATTRIBUTE_SET_ID, ATTRIBUTE_ID, @DESTINATION_SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
		from sTB_ATTRIBUTE_SET
		where ATTRIBUTE_ID in 
		(select attribute_id from @attribute_id)

	--C
	--select * from @tb_attribute_set

--06 put @tb_attribute into sTB_ATTRIBUTE
	insert into sTB_ATTRIBUTE (CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)	
		select @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID 
		from @tb_attribute

	-- @new_tb_attribute
	declare @new_tb_attribute table 
	(NEW_ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))

--07 get the new rows from sTB_ATTRIBUTE and put them into @new_tb_attribute
	insert into @new_tb_attribute (NEW_ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
		select ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID  
		from sTB_ATTRIBUTE
		where OLD_ATTRIBUTE_ID in -- old_attribute_id will identify the items we want
		(select attribute_id from @attribute_id) --wrong! you are getting attributes from other set_id's
		and OLD_ATTRIBUTE_ID not like 'AT%'
		and OLD_ATTRIBUTE_ID not like 'EX%'
		and OLD_ATTRIBUTE_ID not like 'IC%'
		--and ATTRIBUTE_ID in (select ATTRIBUTE_ID from sTB_ATTRIBUTE_SET
		--where SET_ID = @DESTINATION_SET_ID)
		
--- we need to restrict this to the correct set but we haven't populated
--- sTB_ATTRIBUTE_SET yet!!!!!!!!!!!!!!!!!		
	
	--D
	--select * from @new_tb_attribute

--09 update @tb_attribute_set with the new ATTRIBUTE_IDs sitting in @new_tb_attribute
	update @tb_attribute_set 
	set ATTRIBUTE_ID = 
	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
	where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
	where exists
	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
		where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
	
	--E
	--select * from @tb_attribute_set

--08 update @tb_attribute_set with a new PARENT_ATTRIBUTE_ID
	-- the new PARENT_ATTRIBUTE_ID is the 
	update @tb_attribute_set
	set PARENT_ATTRIBUTE_ID = 
	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
	where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID
	and PARENT_ATTRIBUTE_ID != 0)
	where exists
	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
		where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID and PARENT_ATTRIBUTE_ID != 0)
	
	-- clean up the nulls
	update @tb_attribute_set
	set PARENT_ATTRIBUTE_ID = 0
	where PARENT_ATTRIBUTE_ID is null
	
	--F
	--select * from @tb_attribute_set
	
--10 put @tb_attribute_set into sTB_ATTRIBUTE_SET
	insert into sTB_ATTRIBUTE_SET (ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)	
		select ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
		from @tb_attribute_set

--11 place the new codeset at the bottom of the list.
	declare @number_sets int set @number_sets = 0
	select @number_sets = COUNT(*) from sTB_REVIEW_SET
	where REVIEW_ID = @DESTINATION_REVIEW_ID
	-- set the position
	update sTB_REVIEW_SET
	set SET_ORDER = @number_sets - 1
	where REVIEW_ID = @DESTINATION_REVIEW_ID
	and SET_ID = @DESTINATION_SET_ID

-- keep the old_attribute_id values as we need to copy over the data
/*
--12 Clear out OLD_ATTRIBUTE_ID from TB_ATTRIBUTE in the new codeset since
	update sTB_ATTRIBUTE
	set OLD_ATTRIBUTE_ID = null
	where ATTRIBUTE_ID in 
	(select ATTRIBUTE_ID from sTB_ATTRIBUTE_SET
		where SET_ID = @DESTINATION_SET_ID)
*/

COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to create new review'
end
END CATCH

RETURN

SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[st_CopyCodeset_orig]    Script Date: 06/03/2023 11:42:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_CopyCodeset_orig]
(
	@SOURCE_SET_ID int,
	@SOURCE_REVIEW_ID int,
	@DESTINATION_REVIEW_ID int,
	@CONTACT_ID int
)

As

SET NOCOUNT ON


declare @DESTINATION_SET_ID int
declare @DESTINATION_REVIEW_SET_ID int

insert into sTB_SET (SET_TYPE_ID, SET_NAME)
select s.SET_TYPE_ID, s.SET_NAME from sTB_SET s
inner join sTB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
where s.SET_ID = @SOURCE_SET_ID
and r_s.REVIEW_ID = @SOURCE_REVIEW_ID
SET @DESTINATION_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

insert into sTB_REVIEW_SET (REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL)
values (@DESTINATION_REVIEW_ID, @DESTINATION_SET_ID, 1, 1)
SET @DESTINATION_REVIEW_SET_ID = CAST(SCOPE_IDENTITY() AS INT)


DECLARE @WORKING_ATTRIBUTE_ID nvarchar(50)
DECLARE @NEW_ATTRIBUTE_ID nvarchar(50)
DECLARE @WORKING_PARENT_ATTRIBUTE_ID nvarchar(50)
DECLARE @NEW_PARENT_ATTRIBUTE_ID bigint

-- create new row in TB_ATTRIBUTE
-- OLD_ATTRIBUTE_ID will temporarily hold the link between the old and new codeset
DECLARE ATTRIBUTE_ID_CURSOR CURSOR FOR
SELECT a_s.ATTRIBUTE_ID FROM sTB_ATTRIBUTE_SET a_s
inner join sTB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
where a_s.SET_ID = @SOURCE_SET_ID
and r_s.REVIEW_ID = @SOURCE_REVIEW_ID
OPEN ATTRIBUTE_ID_CURSOR
FETCH NEXT FROM ATTRIBUTE_ID_CURSOR
INTO @WORKING_ATTRIBUTE_ID
WHILE @@FETCH_STATUS = 0
BEGIN
	INSERT INTO sTB_ATTRIBUTE (CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)	
	SELECT @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, @WORKING_ATTRIBUTE_ID FROM sTB_ATTRIBUTE
	WHERE ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID

	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
    INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR


-- create new row in TB_ATTRIBUTE_SET using OLD_ATTRIBUTE_ID to link old and new 
DECLARE ATTRIBUTE_ID_CURSOR CURSOR FOR
SELECT s.ATTRIBUTE_ID FROM sTB_ATTRIBUTE_SET s
inner join sTB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID 
where r_s.SET_ID = @SOURCE_SET_ID
and r_s.REVIEW_ID = @SOURCE_REVIEW_ID
OPEN ATTRIBUTE_ID_CURSOR
FETCH NEXT FROM ATTRIBUTE_ID_CURSOR
INTO @WORKING_ATTRIBUTE_ID
WHILE @@FETCH_STATUS = 0
BEGIN
	select @NEW_ATTRIBUTE_ID = ATTRIBUTE_ID from sTB_ATTRIBUTE 
	where OLD_ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	and OLD_ATTRIBUTE_ID not like 'AT%'
	and OLD_ATTRIBUTE_ID not like 'EX%'
	and OLD_ATTRIBUTE_ID not like 'IC%'
	INSERT INTO sTB_ATTRIBUTE_SET (ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)	
	SELECT @NEW_ATTRIBUTE_ID, @DESTINATION_SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
	 FROM sTB_ATTRIBUTE_SET
	WHERE ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID

	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
    INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR


-- update PARENT_ATTRIBUTE_ID from TB_ATTRIBUTE_SET in the new codeset. 
-- Ignore the ones that are 0's (zero) 
DECLARE ATTRIBUTE_ID_CURSOR CURSOR FOR
SELECT ATTRIBUTE_ID FROM sTB_ATTRIBUTE_SET where SET_ID = @DESTINATION_SET_ID
OPEN ATTRIBUTE_ID_CURSOR
FETCH NEXT FROM ATTRIBUTE_ID_CURSOR
INTO @WORKING_ATTRIBUTE_ID
WHILE @@FETCH_STATUS = 0
BEGIN	
	select @WORKING_PARENT_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID from sTB_ATTRIBUTE_SET 
		where ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	if @WORKING_PARENT_ATTRIBUTE_ID != 0
	BEGIN
		update sTB_ATTRIBUTE_SET
		set PARENT_ATTRIBUTE_ID = 
			(select ATTRIBUTE_ID from sTB_ATTRIBUTE 
			where OLD_ATTRIBUTE_ID = @WORKING_PARENT_ATTRIBUTE_ID
				and OLD_ATTRIBUTE_ID not like 'AT%'
				and OLD_ATTRIBUTE_ID not like 'EX%'
				and OLD_ATTRIBUTE_ID not like 'IC%')
		where SET_ID = @DESTINATION_SET_ID
		and ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	END

	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
    INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR

-- clear out OLD_ATTRIBUTE_ID from TB_ATTRIBUTE in the new codeset

/*
update sTB_ATTRIBUTE
set OLD_ATTRIBUTE_ID = null
where ATTRIBUTE_ID in 
(select ATTRIBUTE_ID from sTB_ATTRIBUTE_SET
	where SET_ID = @DESTINATION_SET_ID)
*/

SET NOCOUNT OFF
GO

/****** Object:  StoredProcedure [dbo].[st_CopyReviewShareableStep01]    Script Date: 06/03/2023 11:47:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_CopyReviewShareableStep01]
(
	@CONTACT_ID int,
	@SOURCE_REVIEW_ID int,
	@NEW_REVIEW_ID int output,
	@RESULT nvarchar(50) output
)

As
SET NOCOUNT ON

BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION

declare @NEW_REVIEW_CONTACT_ID int
declare @tb_review_contact table (new_review_contact_id int, review_id int, contact_id int, old_review_id int)
declare @tb_contact_review_role table (review_contact_id int, role_name nvarchar(50))

declare @tb_review_name table (review_name1 nvarchar(255))
declare @REVIEW_NAME nvarchar(1000) 
declare @REVIEW_CONTACT_ID int
set @RESULT = '0'

insert into @tb_review_name (review_name1)select REVIEW_NAME from sTB_REVIEW where REVIEW_ID = @SOURCE_REVIEW_ID
update @tb_review_name set review_name1 = 'COPY - ' + review_name1
set @REVIEW_NAME = (select review_name1 from @tb_review_name)

--select * from @tb_review_contact
--select * from @tb_contact_review_role


--01 create new row in sTB_REVIEW and get @NEW_REVIEW_ID
	insert into sTB_REVIEW (REVIEW_NAME, DATE_CREATED, FUNDER_ID)
	values (@REVIEW_NAME, getdate(), @CONTACT_ID)
	set @NEW_REVIEW_ID = CAST(SCOPE_IDENTITY() AS INT)

insert into @tb_review_contact (review_id, old_review_id, contact_id)
select @NEW_REVIEW_ID, @SOURCE_REVIEW_ID, CONTACT_ID from sTB_REVIEW_CONTACT where REVIEW_ID = @SOURCE_REVIEW_ID
	
--select * from @tb_review_contact
--select * from @tb_contact_review_role

declare @WORKING_CONTACT_ID int
	declare CONTACT_ID_CURSOR cursor for
	select contact_id FROM @tb_review_contact
	open CONTACT_ID_CURSOR
	fetch next from CONTACT_ID_CURSOR
	into @WORKING_CONTACT_ID
	while @@FETCH_STATUS = 0
	begin
		insert into sTB_REVIEW_CONTACT (REVIEW_ID, CONTACT_ID, old_review_id)	
		select review_id, contact_id, old_review_id FROM @tb_review_contact 
		where contact_id = @WORKING_CONTACT_ID
		set @NEW_REVIEW_CONTACT_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_review_contact set new_review_contact_id = @NEW_REVIEW_CONTACT_ID
		where contact_id = @WORKING_CONTACT_ID

		insert into @tb_contact_review_role (review_contact_id, role_name)
		select @NEW_REVIEW_CONTACT_ID, ROLE_NAME from sTB_CONTACT_REVIEW_ROLE	
			where REVIEW_CONTACT_ID = (select REVIEW_CONTACT_ID from sTB_REVIEW_CONTACT
			where REVIEW_ID = @SOURCE_REVIEW_ID and CONTACT_ID = @WORKING_CONTACT_ID)

		fetch next from CONTACT_ID_CURSOR 
		into @WORKING_CONTACT_ID
	end

	close CONTACT_ID_CURSOR
	deallocate CONTACT_ID_CURSOR

insert into sTB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)	
select review_contact_id, role_name FROM @tb_contact_review_role 

--select * from @tb_review_contact
--select * from @tb_contact_review_role

COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to create the new review'
end
END CATCH


RETURN

SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[st_CopyReviewShareableStep03]    Script Date: 06/03/2023 11:48:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_CopyReviewShareableStep03]
(
	@CONTACT_ID int,
	@SOURCE_REVIEW_ID int,
	@NEW_REVIEW_ID int,
	@ATTRIBUTE_FOR_COPY int,
	@RESULT nvarchar(50) output
)
As
SET NOCOUNT ON

declare @NEW_ITEM_ID int
declare @REVIEW_CONTACT_ID int
declare @NEW_SOURCE_ID int
set @RESULT = '0'

declare @tb_item table (new_item_id int, [type_id] int, 
TITLE nvarchar(4000), PARENT_TITLE nvarchar(4000), SHORT_TITLE nvarchar(70),
DATE_CREATED datetime, CREATED_BY nvarchar(50), DATE_EDITED datetime,
EDITED_BY nvarchar(50), YEAR nchar(4), MONTH varchar(10), STANDARD_NUMBER nvarchar(255),
CITY nvarchar(100), COUNTRY nvarchar(100), PUBLISHER nvarchar(1000), INSTITUTION nvarchar(1000),
VOLUME nvarchar(56), PAGES nvarchar(50), EDITION nvarchar(200), ISSUE nvarchar(100),
IS_LOCAL bit, AVAILABILITY nvarchar(200), URL nvarchar(500), MASTER_ITEM_ID bigint,
source_item_id int, ABSTRACT nvarchar(max), COMMENTS nvarchar(max))

declare @tb_item_review table (new_item_id bigint, old_item_id bigint, is_included bit,
new_master_item_id bigint, old_master_item_id bigint, is_deleted bit)

declare @tb_item_author table (new_item_author_id bigint, new_item_id bigint, old_item_id bigint,
LAST nvarchar(50), FIRST nvarchar(50), SECOND nvarchar(50), ROLE tinyint, RANK smallint)

declare @tb_source table (old_source_id int, new_source_id int, source_name nvarchar(255),
new_review_id int, is_deleted bit, date_of_search date, date_of_import date, source_database nvarchar(200),
search_description nvarchar(4000), search_string nvarchar(max), notes nvarchar(4000), import_filter_id int)

declare @tb_item_source table (old_item_source_id int, new_item_source_id int, old_item_id int, 
new_item_id int, original_source_id int, dest_source_id int)

BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION

--01 get example review ID
	--select @SOURCE_REVIEW_ID = EXAMPLE_NON_SHAREABLE_REVIEW_ID from TB_MANAGEMENT_SETTINGS
	
--02 get the item_id's to copy from sTB_ITEM_REVIEW
	if @ATTRIBUTE_FOR_COPY = 0 -- all items
	begin
		insert into @tb_item ([type_id], source_item_id)
		Select [TYPE_ID], i.ITEM_ID from sTB_ITEM i
		inner join sTB_ITEM_REVIEW i_r on i_r.ITEM_ID = i.ITEM_ID
		where i_r.REVIEW_ID = @SOURCE_REVIEW_ID
	end
	else -- items selected by attribute
	begin
		insert into @tb_item ([type_id], source_item_id)
		Select [TYPE_ID], i.ITEM_ID from sTB_ITEM i
		inner join sTB_ITEM_REVIEW i_r on i_r.ITEM_ID = i.ITEM_ID
		inner join sTB_ITEM_ATTRIBUTE i_a on i_a.ITEM_ID = i.ITEM_ID
		where i_r.REVIEW_ID = @SOURCE_REVIEW_ID
		and i_a.ATTRIBUTE_ID = @ATTRIBUTE_FOR_COPY
		--and i_r.IS_DELETED = 0
	end

--03 user cursors to place new row in TB_ITEM, get the new ITEM_ID and update @tb_item
	declare @WORKING_ITEM_ID int
	declare ITEM_ID_CURSOR cursor for
	select source_item_id FROM @tb_item
	open ITEM_ID_CURSOR
	fetch next from ITEM_ID_CURSOR
	into @WORKING_ITEM_ID
	while @@FETCH_STATUS = 0
	begin
		insert into sTB_ITEM ([TYPE_ID], OLD_ITEM_ID)	
		SELECT [type_id], source_item_id FROM @tb_item
		WHERE source_item_id = @WORKING_ITEM_ID
		set @NEW_ITEM_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_item set new_item_id = @NEW_ITEM_ID
		WHERE source_item_id = @WORKING_ITEM_ID

		FETCH NEXT FROM ITEM_ID_CURSOR 
		INTO @WORKING_ITEM_ID
	END

	CLOSE ITEM_ID_CURSOR
	DEALLOCATE ITEM_ID_CURSOR

--04 insert into TB_ITEM_REVIEW based on @item_id
-- need to find correct bit for IS_DELETED and fill in MASTER_ITEM_ID
	insert into @tb_item_review (old_item_id, is_included, old_master_item_id, is_deleted)
	Select ITEM_ID, IS_INCLUDED, MASTER_ITEM_ID, IS_DELETED 
	from sTB_ITEM_REVIEW 
	where REVIEW_ID = @SOURCE_REVIEW_ID

	update t1
	set t1.new_item_id = t2.new_item_id
	from @tb_item_review t1 inner join @tb_item t2
	on t2.source_item_id = t1.old_item_id
	
	update t1
	set t1.new_master_item_id = t2.new_item_id
	from @tb_item_review t1 inner join @tb_item t2
	on t2.source_item_id = t1.old_master_item_id

	insert into sTB_ITEM_REVIEW (ITEM_ID, REVIEW_ID, IS_INCLUDED, MASTER_ITEM_ID, IS_DELETED)
	select new_item_id, @NEW_REVIEW_ID, is_included, new_master_item_id, is_deleted
	from @tb_item_review


--05 insert into @tb_item_author and TB_ITEM_AUTHOR based on @item_id
	insert into @tb_item_author (old_item_id, LAST, FIRST, SECOND, ROLE, RANK)
	select ITEM_ID, LAST, FIRST, SECOND, ROLE, RANK from sTB_ITEM_AUTHOR 
	where ITEM_ID in 
	 (select ITEM_ID from sTB_ITEM_REVIEW where REVIEW_ID = @SOURCE_REVIEW_ID)

	update @tb_item_author
	set new_item_id = i.new_item_id
	from @tb_item i
	where old_item_id = i.source_item_id

	insert into sTB_ITEM_AUTHOR (ITEM_ID, LAST, FIRST, SECOND, ROLE, RANK)
	select new_item_id, LAST, FIRST, SECOND, ROLE, RANK from @tb_item_author

	
--06 update missing info in TB_ITEM
	update @tb_item
	set TITLE = i.TITLE, PARENT_TITLE = i.PARENT_TITLE, SHORT_TITLE = i.SHORT_TITLE,
	 DATE_CREATED = i.DATE_CREATED, CREATED_BY = i.CREATED_BY, DATE_EDITED = i.DATE_EDITED, 
	 EDITED_BY = i.EDITED_BY, YEAR = i.YEAR, MONTH = i.MONTH, STANDARD_NUMBER = i.STANDARD_NUMBER,
	 CITY = i.CITY, COUNTRY = i.COUNTRY, PUBLISHER = i.PUBLISHER, INSTITUTION = i.INSTITUTION, 
	 VOLUME = i.VOLUME, PAGES = i.PAGES, EDITION = i.EDITION, ISSUE = i.ISSUE, IS_LOCAL = i.IS_LOCAL, 
	 AVAILABILITY = i.AVAILABILITY, URL = i.URL, MASTER_ITEM_ID = i.MASTER_ITEM_ID, 
	 ABSTRACT = i.ABSTRACT, COMMENTS = i.COMMENTS
	from sTB_ITEM i
	where source_item_id = i.ITEM_ID
	
	update sTB_ITEM
	set TITLE = tb_i.TITLE, PARENT_TITLE = tb_i.PARENT_TITLE, SHORT_TITLE = tb_i.SHORT_TITLE,
	 DATE_CREATED = tb_i.DATE_CREATED, CREATED_BY = tb_i.CREATED_BY, DATE_EDITED = tb_i.DATE_EDITED, 
	 EDITED_BY = tb_i.EDITED_BY, YEAR = tb_i.YEAR, MONTH = tb_i.MONTH, STANDARD_NUMBER = tb_i.STANDARD_NUMBER,
	 CITY = tb_i.CITY, COUNTRY = tb_i.COUNTRY, PUBLISHER = tb_i.PUBLISHER, INSTITUTION = tb_i.INSTITUTION, 
	 VOLUME = tb_i.VOLUME, PAGES = tb_i.PAGES, EDITION = tb_i.EDITION, ISSUE = tb_i.ISSUE, IS_LOCAL = tb_i.IS_LOCAL, 
	 AVAILABILITY = tb_i.AVAILABILITY, URL = tb_i.URL, MASTER_ITEM_ID = tb_i.MASTER_ITEM_ID, 
	 ABSTRACT = tb_i.ABSTRACT, COMMENTS = tb_i.COMMENTS
	from @tb_item tb_i
	where new_item_id = ITEM_ID

	if @ATTRIBUTE_FOR_COPY = 0 -- all items
	begin
	--07 collect the old source data
		insert into @tb_source (old_source_id, source_name, new_review_id, is_deleted,
		date_of_search, date_of_import, source_database, search_description, 
		search_string, notes, import_filter_id)
		select SOURCE_ID, SOURCE_NAME, @NEW_REVIEW_ID, IS_DELETED, DATE_OF_SEARCH, DATE_OF_IMPORT,
		SOURCE_DATABASE, SEARCH_DESCRIPTION, SEARCH_STRING, NOTES, IMPORT_FILTER_ID
		from sTB_SOURCE s
		where s.REVIEW_ID = @SOURCE_REVIEW_ID


	--08 create new rows in TB_SOURCE
		declare @WORKING_SOURCE_ID int
		declare SOURCE_ID_CURSOR cursor for
		select old_source_id FROM @tb_source
		open SOURCE_ID_CURSOR
		fetch next from SOURCE_ID_CURSOR
		into @WORKING_SOURCE_ID
		while @@FETCH_STATUS = 0
		begin
			insert into sTB_SOURCE (SOURCE_NAME, REVIEW_ID, 
				IS_DELETED, DATE_OF_SEARCH, DATE_OF_IMPORT, SOURCE_DATABASE, 
				SEARCH_DESCRIPTION, SEARCH_STRING, NOTES, IMPORT_FILTER_ID)	
			SELECT source_name, new_review_id, is_deleted,
				date_of_search, date_of_import, source_database, search_description, 
				search_string, notes, import_filter_id FROM @tb_source
				WHERE old_source_id = @WORKING_SOURCE_ID
			set @NEW_SOURCE_ID = CAST(SCOPE_IDENTITY() AS INT)
			
			update @tb_source set new_source_id = @NEW_SOURCE_ID
			WHERE old_source_id = @WORKING_SOURCE_ID

			FETCH NEXT FROM SOURCE_ID_CURSOR 
			INTO @WORKING_SOURCE_ID
		END

		CLOSE SOURCE_ID_CURSOR
		DEALLOCATE SOURCE_ID_CURSOR

		--select * from @tb_source

	--09 collect the old item_source data
		insert into @tb_item_source (old_item_source_id, old_item_id, original_source_id)
		select ITEM_SOURCE_ID, ITEM_ID, SOURCE_ID
		from sTB_ITEM_SOURCE i_s
		where i_s.SOURCE_ID in (select old_source_id from @tb_source)
		
		--select * from @tb_item_source

	--10 update @tb_item_source with the new_source_id and new_item_id
		update t1
		set dest_source_id = t2.new_source_id
		from @tb_item_source t1 inner join @tb_source t2
		on t2.old_source_id = t1.original_source_id
		
		--select * from @tb_item_source
		
		update t1
		set new_item_id = t2.ITEM_ID
		from @tb_item_source t1 inner join sTB_ITEM t2
		on t2.OLD_ITEM_ID = t1.old_item_id
		where t2.ITEM_ID in (select ITEM_ID from sTB_ITEM_REVIEW i_r
		where i_r.REVIEW_ID = @NEW_REVIEW_ID)
			
		--select * from @tb_item_source
		
	--11 place new rows in TB_ITEM_SOURCE based on tb_item_source
		insert into sTB_ITEM_SOURCE (ITEM_ID, SOURCE_ID)
		select new_item_id, dest_source_id from @tb_item_source	
		
	end
	
	else -- it is selected data so place it into a single source
	
	begin
		-- create a new source 
		insert into sTB_SOURCE (SOURCE_NAME, REVIEW_ID, 
			IS_DELETED, DATE_OF_SEARCH, DATE_OF_IMPORT, SOURCE_DATABASE, 
			SEARCH_DESCRIPTION, SEARCH_STRING, NOTES, IMPORT_FILTER_ID)	
		values ('Items from old review', @NEW_REVIEW_ID, 0, GETDATE(), GETDATE(),
			'N/A', 'N/A', 'N/A', 'N/A', 1)
		set @NEW_SOURCE_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		-- and put everything from @tb_item into it
		insert into sTB_ITEM_SOURCE (SOURCE_ID, ITEM_ID)
		select @NEW_SOURCE_ID, new_item_id from @tb_item			 
	end


COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to copy items and place items in new review'
end
END CATCH


RETURN

SET NOCOUNT OFF
GO

/****** Object:  StoredProcedure [dbo].[st_GetInfo]    Script Date: 06/03/2023 11:51:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jeff
-- ALTER date: <>
-- Description:	gets contact table for a contactID
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_GetInfo] 
(
	@ITEM_ATTRIBUTE_ID int
)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

		select ADDITIONAL_TEXT from sTB_ITEM_ATTRIBUTE
		where ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
    	
	RETURN
END
GO

/****** Object:  StoredProcedure [dbo].[st_NewslettersGet]    Script Date: 06/03/2023 11:52:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[st_NewslettersGet] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    /*
    declare @EPPIsupportID int
	select @EPPIsupportID = CONTACT_ID from sTB_CONTACT where EMAIL = 'EPPIsupport@ioe.ac.uk'

	select n.NEWSLETTER_ID, n.[YEAR], n.[MONTH], COUNT(n_c.CONTACT_ID) as NUMBER_SENT  
	from TB_NEWSLETTER n
	left join TB_NEWSLETTER_CONTACT n_c on n_c.NEWSLETTER_ID = n.NEWSLETTER_ID
	where n_c.CONTACT_ID != @EPPIsupportID 
	group by n_c.NEWSLETTER_ID, n.NEWSLETTER_ID, n.YEAR, n.MONTH
	*/

	select n.NEWSLETTER_ID, n.[YEAR], n.[MONTH], COUNT(n_c.CONTACT_ID) as NUMBER_SENT  
	from TB_NEWSLETTER n
	left join TB_NEWSLETTER_CONTACT n_c on n_c.NEWSLETTER_ID = n.NEWSLETTER_ID
	group by n_c.NEWSLETTER_ID, n.NEWSLETTER_ID, n.YEAR, n.MONTH

    	
	RETURN
END
GO







/****** Object:  StoredProcedure [dbo].[st_WebDBWriteToLog]    Script Date: 06/03/2023 12:01:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER       procedure [dbo].[st_WebDBWriteToLog] (
 
	@WebDbId int
	, @ReviewID int
	, @Type nvarchar(25)
	, @Details nvarchar(max)
)
As
SET NOCOUNT ON

	if @Type = 'GetSetFrequency'
	begin
		set @Details = (select SET_NAME from sTB_SET where SET_ID = @Details)
	end

	if @Type = 'GetFrequency'
	begin
		set @Details = (select ATTRIBUTE_NAME from sTB_ATTRIBUTE where ATTRIBUTE_ID = @Details)
	end

	if @Type = 'ItemDetailsFromList'
	begin
		declare @itemID nvarchar(20) = @Details
		set @Details = @itemID + ',' + 
		(select SHORT_TITLE from sTB_ITEM where ITEM_ID = @itemID)
	end
	
	insert into TB_WEBDB_LOG (WEBDB_ID, REVIEW_ID, LOG_TYPE, DETAILS)
	values (@WebDbId, @ReviewID, @Type, @Details)


	
SET NOCOUNT OFF
GO
