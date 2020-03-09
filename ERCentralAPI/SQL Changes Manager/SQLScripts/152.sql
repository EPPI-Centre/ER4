use [reviewer]

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_MAG_ITEM_PAPER_INSERT_TEMP')
BEGIN
DROP TABLE [dbo].[TB_MAG_ITEM_PAPER_INSERT_TEMP]
END
GO
CREATE TABLE dbo.TB_MAG_ITEM_PAPER_INSERT_TEMP
	(
	PaperId bigint NOT NULL,
	year int NULL,
	journal nvarchar(500) NULL,
	--authors nvarchar(MAX) NULL,
	volume nchar(4) NULL,
	issue nvarchar(100) NULL,
	pages nvarchar(50),
	title nvarchar(4000) NULL,
	doi nvarchar(500) NULL,
	abstract nvarchar(max) NULL,
	SearchText nvarchar(500),
	GUID_JOB NVARCHAR(MAX)
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.TB_MAG_ITEM_PAPER_INSERT_TEMP SET (LOCK_ESCALATION = TABLE)
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagItemPaperInsert]    Script Date: 13/02/2020 17:35:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Pull a limited number of MAG papers into tb_ITEM based on a list of IDs. e.g. user 'selected' list
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagItemPaperInsert] 
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

	insert into Reviewer.dbo.TB_SOURCE(SOURCE_NAME, REVIEW_ID, IS_DELETED, DATE_OF_SEARCH, 
		DATE_OF_IMPORT, SOURCE_DATABASE,  SEARCH_DESCRIPTION, SEARCH_STRING, NOTES, IMPORT_FILTER_ID)
	values(@SOURCE_NAME, @REVIEW_id, 'false', GETDATE(),
		GETDATE(), 'Microsoft Academic', 'Browsing items related to a given item', 'Browse', '', 0)

	set @SourceId = @@IDENTITY

	declare @ItemIds Table(ITEM_ID bigint, PaperId bigint)

	insert into Reviewer.dbo.TB_ITEM(TYPE_ID, TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, YEAR, STANDARD_NUMBER, CITY,
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
	 FROM  reviewer.dbo.TB_MAG_ITEM_PAPER_INSERT_TEMP mipi
	 where mipi.GUID_JOB = @GUID_JOB and not mipi.PaperId in 
		(select paperid from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			where ir.REVIEW_ID = @REVIEW_id and ir.IS_DELETED = 'false' and
				(imm.AutoMatchScore > 0.7 or (imm.ManualFalseMatch = 'true' or imm.ManualTrueMatch = 'true')))

	set @N_IMPORTED = @@ROWCOUNT

	if @N_IMPORTED > 0
	begin

		 insert into Reviewer.dbo.TB_ITEM_SOURCE(ITEM_ID, SOURCE_ID)
		 select ITEM_ID, @SourceId from @ItemIds

		 insert into Reviewer.dbo.TB_ITEM_REVIEW(ITEM_ID, REVIEW_ID, IS_DELETED, IS_INCLUDED)
		 select item_id, @REVIEW_id, 'false', 'true' from @ItemIds

		 insert into reviewer.dbo.tb_ITEM_MAG_MATCH(ITEM_ID, REVIEW_ID, PaperId, AutoMatchScore, ManualTrueMatch, ManualFalseMatch)
		 select item_id, @REVIEW_id, PaperId, 1, 'false', 'false' from @ItemIds

		 insert into Reviewer.dbo.TB_ITEM_AUTHOR(ITEM_ID, LAST, FIRST, SECOND, ROLE, RANK)
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

USE Reviewer
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMatchedPapersIds]    Script Date: 14/02/2020 07:38:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns all the papers that are matched to a given tb_ITEM record
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagReviewMatchedPapersIds] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int = 0
,	@INCLUDED varchar(10) = 'included'
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if @INCLUDED = 'included'
	begin
		SELECT @Total = count(distinct imm.paperid) from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and not imm.ManualFalseMatch = 'true'

		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and not imm.ManualFalseMatch = 'true'
		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	end
	else if @INCLUDED = 'excluded'
	begin
		SELECT @Total = count(distinct imm.paperid) from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'false' and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and not imm.ManualFalseMatch = 'true'

		-- Insert statements for procedure here
		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'false' and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and not imm.ManualFalseMatch = 'true'
		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	end
	else
	begin -- i.e. included = included AND excluded
		SELECT @Total = count(distinct imm.paperid) from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and not imm.ManualFalseMatch = 'true'

		-- Insert statements for procedure here
		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_DELETED = 'false'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and not imm.ManualFalseMatch = 'true'
		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	end

	SELECT  @Total as N'@Total'

END
GO


USE Reviewer
GO
/****** Object:  StoredProcedure [dbo].[st_MagItemMatchedPapersIds]    Script Date: 14/02/2020 07:40:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns all the papers that are matched to a given tb_ITEM record
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagItemMatchedPapersIds] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int = 0, 
	@ITEM_ID BIGINT = 0
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	/*
	Not bothering with paging, as the same item is highly unlikely to be matched with more than one or two MAG records

	SELECT @Total = count(*) from tb_ITEM_MAG_MATCH imm
	inner join Papers p on p.PaperID = imm.PaperId
	inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	where imm.ITEM_ID = @ITEM_ID
	*/

    -- Insert statements for procedure here
	SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
	from tb_ITEM_MAG_MATCH imm
	inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	where imm.ITEM_ID = @ITEM_ID
	ORDER BY ManualTrueMatch desc, imm.AutoMatchScore desc
	/*
	OFFSET (@PageNo-1) * @RowsPerPage ROWS
	FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
	*/
END
go

USE reviewer
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMatchedPapersWithThisCodeIds]    Script Date: 14/02/2020 07:40:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns all the papers that are matched to a given tb_ITEM record
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagReviewMatchedPapersWithThisCodeIds] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int = 0
,	@ATTRIBUTE_IDs nvarchar(max)
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

		SELECT @Total = count(distinct imm.paperid) from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		inner join TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = ir.ITEM_ID
		inner join dbo.fn_Split_int(@ATTRIBUTE_IDs, ',') ids on ids.value = tia.ATTRIBUTE_ID
		inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_ATTRIBUTE_ID and tis.IS_COMPLETED = 'true'

		-- Insert statements for procedure here
		SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		
		from tb_ITEM_MAG_MATCH imm
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		inner join TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = ir.ITEM_ID
		inner join dbo.fn_Split_int(@ATTRIBUTE_IDs, ',') ids on ids.value = tia.ATTRIBUTE_ID
		inner join TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_ATTRIBUTE_ID and tis.IS_COMPLETED = 'true'
		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	

	SELECT  @Total as N'@Total'

END

go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersListIds]    Script Date: 14/02/2020 07:47:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagRelatedPapersListIds] 
	-- Add the parameters for the stored procedure here
	
	@MAG_RELATED_RUN_ID int = 0
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@REVIEW_ID int = 0
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	select @Total = count(*) from tb_MAG_RELATED_PAPERS mrp
		where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
	
	SELECT imm.PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from tb_MAG_RELATED_PAPERS mrp
		left outer join tb_ITEM_MAG_MATCH imm on (imm.PaperId = imm.PaperID and imm.REVIEW_ID = @REVIEW_ID)
			and imm.ManualFalseMatch <> 'True' 
		where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
		order by mrp.SimilarityScore
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END
GO

USE Reviewer
GO
/****** Object:  StoredProcedure [dbo].[st_MagPaperListByIdIds]    Script Date: 14/02/2020 07:47:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List of MAG papers from a list of paper IDs
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagPaperListByIdIds] 
	-- Add the parameters for the stored procedure here
	@PaperIds nvarchar(max)
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@REVIEW_ID int = 0
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT @Total = count(value) from dbo.fn_Split_int(@PaperIds, ',')

    -- Insert statements for procedure here
	SELECT distinct ids.value PaperId, imm.AutoMatchScore, imm.ManualFalseMatch, imm.ManualTrueMatch, imm.ITEM_ID
		from dbo.fn_Split_int(@PaperIds, ',') ids		
		left outer join tb_ITEM_MAG_MATCH imm on imm.PaperId = ids.value
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID and AutoMatchScore > 0.7
		order by PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagCurrentInfo]    Script Date: 14/02/2020 08:57:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get information - last update and whether MAG is available
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagCurrentInfo] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @current_version_date datetime = getdate()-- '2019-09-02 14:03:44.917'

	SELECT @current_version_date current_version, 'available' current_availability	
END
go


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagReviewMagInfo]    Script Date: 14/02/2020 09:01:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns statistics about matching review items to MAG
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagReviewMagInfo] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int = 0, 
	@NInReviewIncluded int = 0 OUTPUT,
	@NInReviewExcluded int = 0 OUTPUT,
	@NMatchedAccuratelyIncluded int = 0 OUTPUT,
	@NMatchedAccuratelyExcluded int = 0 OUTPUT,
	@NRequiringManualCheckIncluded int = 0 OUTPUT,
	@NRequiringManualCheckExcluded int = 0 OUTPUT,
	@NNotMatchedIncluded int = 0 OUTPUT,
	@NNotMatchedExcluded INT = 0 OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select @NInReviewIncluded = count(*) from TB_ITEM_REVIEW ir
		where ir.IS_DELETED = 'FALSE' and ir.IS_INCLUDED = 'TRUE' and REVIEW_ID = @REVIEW_ID

	select @NInReviewExcluded = count(*) from TB_ITEM_REVIEW ir
		where ir.IS_DELETED = 'false' and ir.IS_INCLUDED = 'false' and REVIEW_ID = @REVIEW_ID

	select @NMatchedAccuratelyIncluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'true'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and not imm.ManualFalseMatch = 'true'
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NMatchedAccuratelyExcluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'false'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and not imm.ManualFalseMatch = 'true'
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NRequiringManualCheckIncluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'true'
		where imm.AutoMatchScore < 0.7 and (imm.ManualFalseMatch = 'false' and imm.ManualTrueMatch = 'false')
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NRequiringManualCheckExcluded = count(distinct imm.ITEM_ID) from tb_ITEM_MAG_MATCH imm
		inner join tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'false'
		where imm.AutoMatchScore < 0.7 and (imm.ManualFalseMatch = 'false' and imm.ManualTrueMatch = 'false')
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NNotMatchedIncluded = count(distinct ir.ITEM_ID) from TB_ITEM_REVIEW ir
		left outer join tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = ir.ITEM_ID
			where ir.IS_INCLUDED = 'true' and imm.ITEM_ID is null and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'

	select @NNotMatchedExcluded = count(distinct ir.ITEM_ID) from TB_ITEM_REVIEW ir
		left outer join tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = ir.ITEM_ID
			where ir.IS_INCLUDED = 'false' and imm.ITEM_ID is null and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'
	
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRuns]    Script Date: 14/02/2020 09:08:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all runs for a given review
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagRelatedPapersRuns] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select mrr.MAG_RELATED_RUN_ID, mrr.REVIEW_ID, mrr.USER_DESCRIPTION, mrr.ATTRIBUTE_ID, a.ATTRIBUTE_NAME,
		mrr.ALL_INCLUDED, mrr.DATE_FROM, mrr.DATE_RUN, mrr.AUTO_RERUN, mrr.STATUS, mrr.USER_STATUS, mrr.N_PAPERS,
		mrr.MODE, mrr.Filtered from tb_MAG_RELATED_RUN mrr
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mrr.ATTRIBUTE_ID
		where review_id = @REVIEW_ID
		order by MAG_RELATED_RUN_ID
		
END

GO