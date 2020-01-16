USE [AcademicController]
GO

/****** Object:  Table [dbo].[MagInfo]    Script Date: 16/01/2020 16:30:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MagInfo](
	[MAG_ID] [int] IDENTITY(1,1) NOT NULL,
	[Version] [nvarchar](50) NULL,
	[CreatedOn] [datetime] NULL,
	[LiveOn] [datetime] NULL,
	[ReplacedOn] [datetime] NULL,
	[DeletedOn] [datetime] NULL,
	[DatabaseName] [nvarchar](50) NULL,
	[nAffiliations] [int] NULL,
	[nAuthors] [int] NULL,
	[nFieldOfStudyChildren] [int] NULL,
	[nFieldOfStudyRelationship] [int] NULL,
	[nFieldsOfStudy] [int] NULL,
	[nJournals] [int] NULL,
	[nPaperAbstractsInvertedIndex] [int] NULL,
	[nPaperAuthorAffiliations] [int] NULL,
	[nPaperFieldsOfStudy] [int] NULL,
	[nPaperRecommendations] [int] NULL,
	[nPaperReferences] [int] NULL,
	[nPapers] [int] NULL,
	[nPaperUrls] [int] NULL,
	[nRCTScores] [int] NULL,
 CONSTRAINT [PK_MagInfo] PRIMARY KEY CLUSTERED 
(
	[MAG_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO





USE [AcademicController]
GO
/****** Object:  StoredProcedure [dbo].[st_AggregateFoSPaperList]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get the aggregate fields of study for a list of PaperIds
-- =============================================
CREATE   PROCEDURE [dbo].[st_AggregateFoSPaperList] 
	-- Add the parameters for the stored procedure here
	@PaperIdList nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT fos.FieldOfStudyId, fos.DisplayName, count(*) n_papers, sum(pfos.Score) sum_similarity,
		avg(pfos.Score) avg_similarity, fos.PaperCount from PaperFieldsOfStudy pfos
	inner join FieldsOfStudy fos on fos.FieldOfStudyId = pfos.FieldOfStudyID
	inner join fn_Split_int(@PaperIdList, ',') ids on ids.value = pfos.PaperID
	group by fos.FieldOfStudyId, fos.DisplayName, fos.PaperCount
	order by sum_similarity desc
END
GO
/****** Object:  StoredProcedure [dbo].[st_AuthorPapers]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Grab fields of study for a given Paper
-- =============================================
CREATE   PROCEDURE [dbo].[st_AuthorPapers] 
	-- Add the parameters for the stored procedure here
	@AuthorId bigint = 0
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@REVIEW_ID int = 0
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT @Total = count(*) from Papers p
		inner join PaperAuthorAffiliations paa on paa.PaperID = p.PaperID
		inner join Authors a on a.AuthorID = paa.AuthorId
		where paa.AuthorId = @AuthorId

    -- Insert statements for procedure here
	SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, pai.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
		from Papers p
		inner join PaperAuthorAffiliations paa on paa.PaperID = p.PaperID
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
		inner join Authors a on a.AuthorID = paa.AuthorId
		left outer join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.PaperId = p.PaperID
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID
		where paa.AuthorId = @AuthorId
		order by p.PaperID desc
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY	

	SELECT  @Total as N'@Total'
END
GO
/****** Object:  StoredProcedure [dbo].[st_CheckReviewHasUpdates]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Checks to see whether a review has any auto-identified studies for authors to check
-- =============================================
CREATE   PROCEDURE [dbo].[st_CheckReviewHasUpdates] 
	-- Add the parameters for the stored procedure here
	@REVIEW_id int = 0,
	@NUpdates INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    select @NUpdates = sum(N_PAPERS) from Reviewer.dbo.tb_MAG_RELATED_RUN
		where REVIEW_ID = @REVIEW_id
		and USER_STATUS = 'Unchecked'

	if @NUpdates is null
	begin
		set @NUpdates = 0
	end
END
GO
/****** Object:  StoredProcedure [dbo].[st_CurrentMagInfo]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get information - last update and whether MAG is available
-- =============================================
CREATE   PROCEDURE [dbo].[st_CurrentMagInfo] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @current_version_date datetime = getdate()-- '2019-09-02 14:03:44.917'

	SELECT @current_version_date current_version, 'available' current_availability	
END
GO
/****** Object:  StoredProcedure [dbo].[st_FieldOfStudyPapers]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Grab fields of study for a given Paper
-- =============================================
CREATE   PROCEDURE [dbo].[st_FieldOfStudyPapers] 
	-- Add the parameters for the stored procedure here
	@FieldOfStudyId bigint = 0
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@REVIEW_ID INT = 0
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT @Total = count(pfos.PaperId) from PaperFieldsOfStudy pfos
		where pfos.FieldOfStudyID = @FieldOfStudyId

    -- Insert statements for procedure here
	SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, pai.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
	from Papers p
		inner join PaperFieldsOfStudy pfos on pfos.PaperID = p.PaperID
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
		left outer join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.PaperId = p.PaperID
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID
		where pfos.FieldOfStudyID = @FieldOfStudyId
		order by pfos.Score desc
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY	

	SELECT  @Total as N'@Total'
END
GO
/****** Object:  StoredProcedure [dbo].[st_FieldsOfStudyChildrenList]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get the aggregate fields of study for a list of PaperIds
-- =============================================
CREATE   PROCEDURE [dbo].[st_FieldsOfStudyChildrenList] 
	-- Add the parameters for the stored procedure here
	@FieldOfStudyId bigint = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT fos.FieldOfStudyId, fos.DisplayName, 0 n_papers, 0 sum_similarity, 0 avg_similarity,
		fos.PaperCount
		from FieldOfStudyChildren fosc
		inner join FieldsOfStudy fos on fos.FieldOfStudyId = fosc.ChildFieldOfStudyId
		where fosc.FieldOfStudyId = @FieldOfStudyId

END
GO
/****** Object:  StoredProcedure [dbo].[st_FieldsOfStudyParentsList]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get the aggregate fields of study for a list of PaperIds
-- =============================================
CREATE   PROCEDURE [dbo].[st_FieldsOfStudyParentsList] 
	-- Add the parameters for the stored procedure here
	@FieldOfStudyId bigint = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT fos.FieldOfStudyId, fos.DisplayName, 0 n_papers, 0 sum_similarity, 0 avg_similarity,
		fos.PaperCount
		from FieldOfStudyChildren fosc
		inner join FieldsOfStudy fos on fos.FieldOfStudyId = fosc.FieldOfStudyId
		where fosc.ChildFieldOfStudyId = @FieldOfStudyId

END
GO
/****** Object:  StoredProcedure [dbo].[st_FieldsOfStudyRelatedFoSList]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get the aggregate fields of study for a list of PaperIds
-- =============================================
CREATE   PROCEDURE [dbo].[st_FieldsOfStudyRelatedFoSList] 
	-- Add the parameters for the stored procedure here
	@FieldOfStudyId bigint = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select pfos.FieldOfStudyID, fos.DisplayName, count(*) n_papers
		, sum(pfos.Score) sum_similarity, avg(pfos.Score) avg_similarity,
		fos.PaperCount
	from PaperFieldsOfStudy pfos

	inner join PaperFieldsOfStudy pfos2 on pfos2.PaperID =
	pfos.PaperID and pfos2.FieldOfStudyID = @FieldOfStudyId
	inner join FieldsOfStudy fos on fos.FieldOfStudyId = pfos.FieldOfStudyID
	where pfos.FieldOfStudyID <> @FieldOfStudyId
	group by pfos.FieldOfStudyID, fos.DisplayName, fos.PaperCount
	having count(*) > 1
	order by avg_similarity desc

END
GO
/****** Object:  StoredProcedure [dbo].[st_FieldsOfStudySearch]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Freetext search for fields of study
-- =============================================
CREATE   PROCEDURE [dbo].[st_FieldsOfStudySearch] 
	-- Add the parameters for the stored procedure here
	@SearchText nvarchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT fos.FieldOfStudyId, fos.DisplayName, 0 n_papers, 0 sum_similarity, 0 avg_similarity,
		fos.PaperCount
		from FieldsOfStudy fos
		where contains(NormalizedName, @SearchText)
		order by fos.PaperCount desc


END
GO
/****** Object:  StoredProcedure [dbo].[st_ItemMagRelatedPaperInsert]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Pull MAG papers found in a related papers run into tb_ITEM
-- =============================================
CREATE   PROCEDURE [dbo].[st_ItemMagRelatedPaperInsert] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID INT,
	@REVIEW_id int = 0,
	@SOURCE_NAME nvarchar(255),
	@CONTACT_ID INT = 0,
	@N_IMPORTED INT OUTPUT
--WITH RECOMPILE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	set @N_IMPORTED = 0

    -- Insert statements for procedure here
	declare @SourceId int = 0
	declare @NewItems Table
	(
		[TYPE_ID] int,
		TITLE nvarchar(4000),
		PARENT_TITLE nvarchar(4000),
		SHORT_TITLE nvarchar(70),
		DATE_CREATED datetime,
		CREATED_BY nvarchar(50),
		[YEAR] nchar(4),
		STANDARD_NUMBER nvarchar(255),
		CITY nvarchar(100),
		COUNTRY nvarchar(100),
		PUBLISHER nvarchar(1000),
		INSTITUTION nvarchar(1000),
		VOLUME nvarchar(56),
		PAGES nvarchar(50),
		EDITION nvarchar(200),
		ISSUE nvarchar(200),
		[URL] nvarchar(2000),
		OLD_ITEM_ID nvarchar(50),
		ABSTRACT nvarchar(max),
		DOI nvarchar(500),
		SearchText nvarchar(500)
	)

	insert into Reviewer.dbo.TB_SOURCE(SOURCE_NAME, REVIEW_ID, IS_DELETED, DATE_OF_SEARCH, 
		DATE_OF_IMPORT, SOURCE_DATABASE, SEARCH_DESCRIPTION, SEARCH_STRING, NOTES, IMPORT_FILTER_ID)
	select @SOURCE_NAME, @REVIEW_id, 'false', DATE_RUN,
		GETDATE(), 'Microsoft Academic', cast(USER_DESCRIPTION as nvarchar(4000)), MODE, '', 0
		from Reviewer.dbo.tb_MAG_RELATED_RUN where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID

	set @SourceId = @@IDENTITY

	declare @ItemIds Table(ITEM_ID bigint, PaperId bigint)



	insert into @NewItems(TYPE_ID, TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, YEAR, STANDARD_NUMBER, CITY,
		COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, URL, OLD_ITEM_ID, ABSTRACT, DOI, SearchText)
	--output INSERTED.ITEM_ID, cast(inserted.OLD_ITEM_ID as bigint) into @ItemIds
	SELECT 
		case
			when p.DocType = 'Book' then 2
			when p.DocType = 'BookChapter' then 3
			when p.DocType = 'Conference' then 5
			when p.DocType = 'Journal' then 14
			when p.DocType = 'Patent' then 1
			else 14
		end, p.OriginalTitle, j.DisplayName, cast(dbo.fn_PaperFirstAuthor(p.PaperID) + ' (' + cast(p.Year as nvarchar) + ')' as nvarchar(70)), p.CreatedDate, @CONTACT_ID, p.Year, j.ISSN, '',
		'', j.Publisher, '', p.Volume, p.FirstPage + '-' + p.LastPage, '', p.Issue, '', cast(p.PaperId as nvarchar), pai.IndexedAbstract, p.doi, p.SearchText
	 FROM Papers p
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
	 INNER JOIN Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = p.PaperID and mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
	 
	 and not mrp.PaperId in 
		(select paperid from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			where ir.REVIEW_ID = @REVIEW_id and ir.IS_DELETED = 'false' and
				(imm.AutoMatchScore > 0.7 or (imm.ManualFalseMatch = 'true' or imm.ManualTrueMatch = 'true')))

	insert into Reviewer.dbo.TB_ITEM([TYPE_ID], TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, YEAR, STANDARD_NUMBER, CITY,
		COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, URL, OLD_ITEM_ID, ABSTRACT, DOI, SearchText)
	output INSERTED.ITEM_ID, cast(inserted.OLD_ITEM_ID as bigint) into @ItemIds
	select [TYPE_ID], TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, YEAR, STANDARD_NUMBER, CITY,
		COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, URL, OLD_ITEM_ID, ABSTRACT, DOI, SearchText
		from @NewItems

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
		 select distinct ITEM_ID,
			case when charindex(' ', cast(a.DisplayName as nvarchar(50))) = 0 then a.DisplayName else cast(right(a.DisplayName, charindex(' ', reverse(a.DisplayName) + '_') - 1) as nvarchar(50)) end,
			case when CHARINDEX(' ', cast(a.DisplayName as nvarchar(50))) = 0 then '' else  cast(left(a.DisplayName, len(a.DisplayName) - charindex(' ', reverse(a.DisplayName) + ' ')) as nvarchar(50)) end,
			'', 0, paa.AuthorSequenceNumber -1
		 from @ItemIds ids
			inner join PaperAuthorAffiliations paa on paa.PaperId = ids.PaperId
			inner join Authors a on a.AuthorID = paa.AuthorId

	end

	update Reviewer.dbo.tb_MAG_RELATED_RUN
		set USER_STATUS = 'Imported'
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID

END
GO
/****** Object:  StoredProcedure [dbo].[st_ItemMatchedPaperManualInsert]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_item_mag_match based on manual lookup
-- =============================================
CREATE   PROCEDURE [dbo].[st_ItemMatchedPaperManualInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_ID BIGINT
,	@PaperId BIGINT
,	@ManualTrueMatch bit
,	@ManualFalseMatch bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	declare @chk int
	select @chk = count(*) from Reviewer.dbo.tb_ITEM_MAG_MATCH
		where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId

	if @chk = 0
	begin
		insert into Reviewer.dbo.tb_ITEM_MAG_MATCH(REVIEW_ID, ITEM_ID, PaperId, ManualTrueMatch, ManualFalseMatch, AutoMatchScore)
		values (@REVIEW_ID, @ITEM_ID, @PaperId, @ManualTrueMatch, @ManualFalseMatch, 1)
	end
	else
	begin
		update Reviewer.dbo.tb_ITEM_MAG_MATCH
			set ManualTrueMatch = @ManualTrueMatch,
				ManualFalseMatch = @ManualFalseMatch
			where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId
	end
END
GO
/****** Object:  StoredProcedure [dbo].[st_ItemMatchedPapers]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns all the papers that are matched to a given tb_ITEM record
-- =============================================
CREATE   PROCEDURE [dbo].[st_ItemMatchedPapers] 
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

	SELECT @Total = count(*) from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
	inner join Papers p on p.PaperID = imm.PaperId
	inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	where imm.ITEM_ID = @ITEM_ID
	*/

    -- Insert statements for procedure here
	SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, pai.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
	from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
	inner join Papers p on p.PaperID = imm.PaperId
	left outer join Journals j on j.JournalId = p.JournalID
	left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
	inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
	where imm.ITEM_ID = @ITEM_ID
	ORDER BY ManualTrueMatch desc, imm.AutoMatchScore desc
	/*
	OFFSET (@PageNo-1) * @RowsPerPage ROWS
	FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
	*/
END
GO
/****** Object:  StoredProcedure [dbo].[st_ItemMatchedPaperUpdate]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Set manual decision on whether a given paper is / isn't a correct match to an item
-- =============================================
CREATE   PROCEDURE [dbo].[st_ItemMatchedPaperUpdate] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_ID BIGINT
,	@PaperId BIGINT
,	@ManualTrueMatch bit
,	@ManualFalseMatch bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	update Reviewer.dbo.tb_ITEM_MAG_MATCH
		set ManualTrueMatch = @ManualTrueMatch,
			ManualFalseMatch = @ManualFalseMatch
		where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId
END
GO
/****** Object:  StoredProcedure [dbo].[st_ItemPaperInsert]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Pull a limited number of MAG papers into tb_ITEM based on a list of IDs. e.g. user 'selected' list
-- =============================================
CREATE   PROCEDURE [dbo].[st_ItemPaperInsert] 
	-- Add the parameters for the stored procedure here
	@PaperIds varchar(max) = '', 
	@REVIEW_id int = 0,
	@SOURCE_NAME nvarchar(255),
	@CONTACT_ID INT = 0,
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
		case
			when p.DocType = 'Book' then 2
			when p.DocType = 'BookChapter' then 3
			when p.DocType = 'Conference' then 5
			when p.DocType = 'Journal' then 14
			when p.DocType = 'Patent' then 1
			else 14
		end, p.OriginalTitle, j.DisplayName, dbo.fn_PaperFirstAuthor(p.PaperID) + ' (' + cast(p.Year as nvarchar) + ')', p.CreatedDate, @CONTACT_ID, p.Year, j.ISSN, '',
		'', j.Publisher, '', p.Volume, p.FirstPage + '-' + p.LastPage, '', p.Issue, '', cast(p.PaperId as nvarchar), pai.IndexedAbstract, p.doi, p.SearchText
	 FROM Papers p
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
	 INNER JOIN DBO.fn_split_int(@PaperIds, ',') ON	p.PaperID = value
	 and not p.PaperId in 
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
		 select distinct ITEM_ID, 
			case when charindex(' ', cast(a.DisplayName as nvarchar(50))) = 0 then a.DisplayName else cast(right(a.DisplayName, charindex(' ', reverse(a.DisplayName) + '_') - 1) as nvarchar(50)) end,
			case when CHARINDEX(' ', cast(a.DisplayName as nvarchar(50))) = 0 then '' else  cast(left(a.DisplayName, len(a.DisplayName) - charindex(' ', reverse(a.DisplayName) + ' ')) as nvarchar(50)) end,'', 0, paa.AuthorSequenceNumber -1
		 from @ItemIds ids
			inner join PaperAuthorAffiliations paa on paa.PaperId = ids.PaperId
			inner join Authors a on a.AuthorID = paa.AuthorId

	end
		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchItemsRemove]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
CREATE   PROCEDURE [dbo].[st_MagMatchItemsRemove] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int = 0
,	@ITEM_ID bigint = 0
,	@ATTRIBUTE_ID bigint = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    IF (@ITEM_ID <> 0)
	BEGIN
		delete from Reviewer.dbo.tb_ITEM_MAG_MATCH
			where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID
	END

	ELSE IF (@ATTRIBUTE_ID <> 0)
	BEGIN
		delete r
			from Reviewer.dbo.tb_ITEM_MAG_MATCH r
			inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = r.ITEM_ID
			inner join Reviewer.dbo.TB_ITEM_SET iset on iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
			where r.REVIEW_ID = @REVIEW_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
	END

	ELSE
	BEGIN
		delete from Reviewer.dbo.tb_ITEM_MAG_MATCH
			where REVIEW_ID = @REVIEW_ID
	END

END
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersList]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
CREATE   PROCEDURE [dbo].[st_MagRelatedPapersList] 
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

	select @Total = count(*) from Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp
		inner join papers p on p.PaperID = mrp.PaperId
		where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
	
	SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, pai.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
		from Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp
		inner join papers p on p.PaperID = mrp.PaperId
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
		left outer join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on (imm.PaperId = p.PaperID and imm.REVIEW_ID = @REVIEW_ID)
			and imm.ManualFalseMatch <> 'True' 
		where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
		order by mrp.SimilarityScore
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRun]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get a single run (not sure this will ever be used)
-- =============================================
CREATE   PROCEDURE [dbo].[st_MagRelatedPapersRun] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_id int = 0
,	@MAG_RELATED_RUN_ID INT = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here


	
	select mrr.MAG_RELATED_RUN_ID, mrr.REVIEW_ID, mrr.USER_DESCRIPTION, mrr.ATTRIBUTE_ID, a.ATTRIBUTE_NAME,
		mrr.ALL_INCLUDED, mrr.DATE_FROM, mrr.DATE_RUN, mrr.AUTO_RERUN, mrr.STATUS, mrr.USER_STATUS, mrr.N_PAPERS,
		mrr.MODE, mrr.Filtered
		
		from Reviewer.dbo.tb_MAG_RELATED_RUN mrr
		left outer join Reviewer.dbo.TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mrr.ATTRIBUTE_ID
		where review_id = @REVIEW_id AND MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
		order by MAG_RELATED_RUN_ID
		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRuns]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all runs for a given review
-- =============================================
CREATE   PROCEDURE [dbo].[st_MagRelatedPapersRuns] 
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
		mrr.MODE, mrr.Filtered from Reviewer.dbo.tb_MAG_RELATED_RUN mrr
		left outer join Reviewer.dbo.TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mrr.ATTRIBUTE_ID
		where review_id = @REVIEW_ID
		order by MAG_RELATED_RUN_ID
		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsDelete]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
CREATE   PROCEDURE [dbo].[st_MagRelatedPapersRunsDelete] 
	-- Add the parameters for the stored procedure here
	
	@MAG_RELATED_RUN_ID int = 0
,	@REVIEW_ID int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	delete from Reviewer.dbo.tb_MAG_RELATED_PAPERS
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID and REVIEW_ID = @REVIEW_ID

	delete from Reviewer.dbo.tb_MAG_RELATED_RUN
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID AND REVIEW_ID = @REVIEW_ID

END
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsInsert]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
CREATE   PROCEDURE [dbo].[st_MagRelatedPapersRunsInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int
,	@USER_DESCRIPTION NVARCHAR(1000)
,	@PaperIdList nvarchar(max)
,	@ATTRIBUTE_ID bigint = 0
,	@ALL_INCLUDED BIT
,	@DATE_FROM DATETIME
,	@AUTO_RERUN BIT
,	@MODE nvarchar(50)
,	@FILTERED NVARCHAR(50)
,	@MAG_RELATED_RUN_ID int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	insert into Reviewer.dbo.tb_MAG_RELATED_RUN(REVIEW_ID, USER_DESCRIPTION, PaperIdList, ATTRIBUTE_ID,
		ALL_INCLUDED, DATE_FROM, AUTO_RERUN, STATUS, USER_STATUS, MODE, Filtered, N_PAPERS)
	values(@REVIEW_ID, @USER_DESCRIPTION, @PaperIdList, @ATTRIBUTE_ID,
		@ALL_INCLUDED, @DATE_FROM, @AUTO_RERUN, 'Pending', 'Waiting', @MODE, @FILTERED, 0)

	set @MAG_RELATED_RUN_ID = @@IDENTITY

	update Reviewer.dbo.tb_MAG_RELATED_RUN
		set PARENT_MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsUpdate]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
CREATE   PROCEDURE [dbo].[st_MagRelatedPapersRunsUpdate] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID int,
	@AUTO_RERUN BIT,
	@USER_DESCRIPTION NVARCHAR(1000)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	update Reviewer.dbo.tb_MAG_RELATED_RUN
		set USER_STATUS = 'Checked',
		AUTO_RERUN = @AUTO_RERUN,
		USER_DESCRIPTION = @USER_DESCRIPTION
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersWithDateFilter]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


--RunningUpdates
-- DO NOT REMOVE ABOVE LINE! IT'S USED WHEN CHECKING IF THIS STORED PROCEDURE IS RUNNING
-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get the ranked list of papers related to a set of items
--		This is the heart of 'review updates' - and there are a number of options: hence the long, repetitive stored proc
-- =============================================
 CREATE     PROCEDURE [dbo].[st_MagRelatedPapersWithDateFilter] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @PaperIdList nvarchar(max) = ''
	declare @ATTRIBUTE_ID bigint = 0
	declare @ALL_INCLUDED BIT = 'false'
	declare @REVIEW_ID int
	declare @DATE_FROM datetime = null
	declare @STATUS nvarchar(50)
	declare @PARENT_MAG_RELATED_RUN_ID INT
	declare @DATE_FROM_INT int = 0
	declare @MODE nvarchar(50)
	declare @FILTERED nvarchar(50)
	declare @NumInserted int

	declare @SeedIds table
	(
		PaperId bigint INDEX idx CLUSTERED
	)

	select @PaperIdList = PaperIdList
		, @ATTRIBUTE_ID = ATTRIBUTE_ID
		, @ALL_INCLUDED = ALL_INCLUDED
		, @DATE_FROM = DATE_FROM
		, @STATUS = [STATUS]
		, @PARENT_MAG_RELATED_RUN_ID = PARENT_MAG_RELATED_RUN_ID
		, @MODE = MODE
		, @FILTERED = Filtered
		, @REVIEW_ID = REVIEW_ID
	FROM Reviewer.dbo.tb_MAG_RELATED_RUN mrr where mrr.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID

	if @STATUS = 'Complete' -- Create a new row for this 'run'
	begin
		update Reviewer.dbo.tb_MAG_RELATED_RUN -- Set autoupdate to 'false' for the older one, or we'll get duplicate reruns
			set AUTO_RERUN = 'false'
			where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID

		insert into Reviewer.dbo.tb_MAG_RELATED_RUN(REVIEW_ID, USER_DESCRIPTION, PaperIdList, ATTRIBUTE_ID, ALL_INCLUDED,
			DATE_FROM, DATE_RUN, AUTO_RERUN, STATUS, PARENT_MAG_RELATED_RUN_ID, MODE, Filtered, N_PAPERS)
		select REVIEW_ID, USER_DESCRIPTION, PaperIdList, ATTRIBUTE_ID, ALL_INCLUDED,
			DATE_FROM, GETDATE(), 'true', 'Running', @PARENT_MAG_RELATED_RUN_ID, MODE, FILTERED, 0 from Reviewer.dbo.tb_MAG_RELATED_RUN
				where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID

		set @MAG_RELATED_RUN_ID = @@IDENTITY
	end

	if not @DATE_FROM is null
		set @DATE_FROM_INT = year(@DATE_FROM)

	-- Create an in-memory table of all the TB_ITEM -> MAG matches that we're working from
	if (@PaperIdList = '')
	begin
		if not @ATTRIBUTE_ID = 0
			insert into @SeedIds
			select imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
			inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = ia.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			group by imm.PaperId
		else
			insert into @SeedIds
			select imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			group by imm.PaperId
	end
	else
		insert into @SeedIds
			select value from Dbo.fn_Split_int(@PaperIdList, ',') 

	-- Now the main identification work happens using the paper recommendations and references tables.
	-- Relationships can be uni- or bi-directional, so there are 6 possible methods.

	-- using the PaperRecommendations table:
	if @MODE = 'Recommended by'
	begin
		insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		SELECT @REVIEW_ID, pr.RecommendedPaperID, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, Sum(Score) SimilarityScore
		from PaperRecommendations pr
		inner join Papers p on p.PaperID = pr.RecommendedPaperID
		inner join @SeedIds spl on spl.PaperId = pr.PaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.RecommendedPaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.RecommendedPaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.RecommendedPaperID
	end
	else if @MODE = 'That recommend'
	begin
		insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		SELECT @REVIEW_ID, pr.PaperID, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, Sum(Score) SimilarityScore
		from PaperRecommendations pr
		inner join Papers p on p.PaperID = pr.PaperID
		inner join @SeedIds spl on spl.PaperId = pr.RecommendedPaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperID
	end
	else if @MODE = 'Recommendations'
	begin

		with cte (PaperId, SimilarityScore) as
		(
		SELECT pr.PaperID, Sum(Score) SimilarityScore
		from PaperRecommendations pr
		inner join Papers p on p.PaperID = pr.PaperID
		inner join @SeedIds spl on spl.PaperId = pr.RecommendedPaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperID

		union all

		SELECT pr.RecommendedPaperID, Sum(Score) SimilarityScore
		from PaperRecommendations pr
		inner join Papers p on p.PaperID = pr.RecommendedPaperID
		inner join @SeedIds spl on spl.PaperId = pr.PaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.RecommendedPaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.RecommendedPaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.RecommendedPaperID

		) insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		select @REVIEW_ID, PaperId, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, Sum(SimilarityScore)
			from cte group by PaperId

	end

	-- Using the PaperReferences table:
	else if @MODE = 'Bibliography'
	begin
		insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		SELECT @REVIEW_ID, pr.PaperReferenceID, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, 1
		from PaperReferences pr
		inner join Papers p on p.PaperID = pr.PaperReferenceID
		inner join @SeedIds spl on spl.PaperId = pr.PaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperReferenceID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperReferenceID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperReferenceID
	end
	else if @MODE = 'Cited by'
	begin
		insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		SELECT @REVIEW_ID, pr.PaperID, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, 1
		from PaperReferences pr
		inner join Papers p on p.PaperID = pr.PaperID
		inner join @SeedIds spl on spl.PaperId = pr.PaperReferenceID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperID
	end
	else if @MODE = 'BiCitation'
	begin
		with cte (PaperId) as
		(
		SELECT pr.PaperID
		from PaperReferences pr
		inner join Papers p on p.PaperID = pr.PaperID
		inner join @SeedIds spl on spl.PaperId = pr.PaperReferenceID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperID

		union all

		SELECT pr.PaperReferenceID
		from PaperReferences pr
		inner join Papers p on p.PaperID = pr.PaperReferenceID
		inner join @SeedIds spl on spl.PaperId = pr.PaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperReferenceID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperReferenceID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperReferenceID

		) insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		select @REVIEW_ID, PaperId, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, 1
			from cte group by PaperId
	end
	else if @MODE = 'Bi-Citation AND Recommendations'
	begin
		with cte (PaperId, SimilarityScore) as
		(
		SELECT pr.PaperID, Sum(Score) SimilarityScore
		from PaperRecommendations pr
		inner join Papers p on p.PaperID = pr.PaperID
		inner join @SeedIds spl on spl.PaperId = pr.RecommendedPaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperID

		union all

		SELECT pr.RecommendedPaperID, Sum(Score) SimilarityScore
		from PaperRecommendations pr
		inner join Papers p on p.PaperID = pr.RecommendedPaperID
		inner join @SeedIds spl on spl.PaperId = pr.PaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.RecommendedPaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.RecommendedPaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.RecommendedPaperID

		union all

		SELECT pr.PaperID, 1
		from PaperReferences pr
		inner join Papers p on p.PaperID = pr.PaperID
		inner join @SeedIds spl on spl.PaperId = pr.PaperReferenceID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperID

		union all

		SELECT pr.PaperReferenceID, 1
		from PaperReferences pr
		inner join Papers p on p.PaperID = pr.PaperReferenceID
		inner join @SeedIds spl on spl.PaperId = pr.PaperID
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm_filter on imm_filter.PaperId = pr.PaperReferenceID
		left outer join Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp on mrp.PaperId = pr.PaperReferenceID and mrp.PARENT_MAG_RELATED_RUN_ID = @PARENT_MAG_RELATED_RUN_ID
		where imm_filter.PaperId is null and mrp.PaperId is null and p.Year >= @DATE_FROM_INT
		group by pr.PaperReferenceID

		) insert into Reviewer.dbo.tb_MAG_RELATED_PAPERS(REVIEW_ID, PaperId, MAG_RELATED_RUN_ID,
			PARENT_MAG_RELATED_RUN_ID, SimilarityScore)
		select @REVIEW_ID, PaperId, @MAG_RELATED_RUN_ID, @PARENT_MAG_RELATED_RUN_ID, Sum(SimilarityScore)
			from cte group by PaperId
	end

	set @NumInserted = @@ROWCOUNT

	if not @FILTERED = 'None'
	begin
		if @FILTERED = 'Sensitive'
		begin
			DELETE mrp
			FROM Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp
			inner join RCTScores r on r.PaperId = mrp.PaperId
			where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID and r.Score < 0.002444887

			set @NumInserted = @NumInserted - @@ROWCOUNT
		end

		if @FILTERED = 'Precise'
		begin
			DELETE mrp
			FROM Reviewer.dbo.tb_MAG_RELATED_PAPERS mrp
			inner join RCTScores r on r.PaperId = mrp.PaperId
			where mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID and r.Score < 0.003897103

			set @NumInserted = @NumInserted - @@ROWCOUNT
		end
	end

	

	update Reviewer.dbo.tb_MAG_RELATED_RUN
		set [STATUS] = 'Complete',
		N_PAPERS = @NumInserted,
		DATE_RUN = GETDATE(),
		USER_STATUS = 'Unchecked'
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationDelete]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
CREATE   PROCEDURE [dbo].[st_MagSimulationDelete] 
	-- Add the parameters for the stored procedure here
	
	@MAG_SIMULATION_ID int = 0
,	@REVIEW_ID int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	delete from Reviewer.dbo.TB_MAG_SIMULATION_RESULT
		where MAG_SIMULATION_ID = @MAG_SIMULATION_ID

	delete from Reviewer.dbo.TB_MAG_SIMULATION
		where MAG_SIMULATION_ID = @MAG_SIMULATION_ID AND REVIEW_ID = @REVIEW_ID

END
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationInsert]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
CREATE     PROCEDURE [dbo].[st_MagSimulationInsert] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int = 0
,	@YEAR INT = 0
,	@CREATED_DATE DATETIME = NULL
,	@WITH_THIS_ATTRIBUTE_ID BIGINT = 0
,	@FILTERED_BY_ATTRIBUTE_ID BIGINT = 0
,	@SEARCH_METHOD NVARCHAR(50) = NULL
,	@NETWORK_STATISTIC NVARCHAR(50) = NULL
,	@STUDY_TYPE_CLASSIFIER NVARCHAR(50) = NULL
,	@USER_CLASSIFIER_MODEL_ID INT = NULL
,	@STATUS NVARCHAR(50) = NULL

,	@MAG_SIMULATION_ID INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	if @USER_CLASSIFIER_MODEL_ID = 0
		set @USER_CLASSIFIER_MODEL_ID = null

	INSERT INTO REVIEWER.DBO.TB_MAG_SIMULATION
		(REVIEW_ID, YEAR, CREATED_DATE, WITH_THIS_ATTRIBUTE_ID, FILTERED_BY_ATTRIBUTE_ID, SEARCH_METHOD,
		NETWORK_STATISTIC, STUDY_TYPE_CLASSIFIER, USER_CLASSIFIER_MODEL_ID, STATUS)
	VALUES(@REVIEW_ID, @YEAR, @CREATED_DATE, @WITH_THIS_ATTRIBUTE_ID, @FILTERED_BY_ATTRIBUTE_ID, @SEARCH_METHOD,
		@NETWORK_STATISTIC, @STUDY_TYPE_CLASSIFIER, @USER_CLASSIFIER_MODEL_ID, @STATUS)

	SET @MAG_SIMULATION_ID = @@IDENTITY
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationResults]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
CREATE PROCEDURE [dbo].[st_MagSimulationResults] 
	-- Add the parameters for the stored procedure here
	
	@MagSimulationId int
,	@OrderBy nvarchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
if @OrderBy = 'Network'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from Reviewer.dbo.TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.NETWORK_STATISTIC_SCORE desc

if @OrderBy = 'FoS'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from Reviewer.dbo.TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.FOS_DISTANCE_SCORE

if @OrderBy = 'User'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from Reviewer.dbo.TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.USER_CLASSIFIER_MODEL_SCORE desc

if @OrderBy = 'StudyType'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from Reviewer.dbo.TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.STUDY_TYPE_CLASSIFIER_SCORE desc

if @OrderBy = 'Ensemble'
	SELECT msr.MAG_SIMULATION_ID, msr.PaperId, msr.INCLUDED, msr.FOUND, msr.STUDY_TYPE_CLASSIFIER_SCORE,
		msr.USER_CLASSIFIER_MODEL_SCORE, msr.NETWORK_STATISTIC_SCORE, msr.FOS_DISTANCE_SCORE,
		msr.ENSEMBLE_SCORE
	from Reviewer.dbo.TB_MAG_SIMULATION_RESULT msr
		WHERE msr.MAG_SIMULATION_ID = @MagSimulationId and msr.SEED = 'false'
		order by msr.ENSEMBLE_SCORE desc
		
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationRun]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--RunningSimulation
-- DON'T REMOVE THE LINE ABOVE - IT'S USED IN THE WORKER PROCESS TO CHECK WHETHER THIS PROC IS RUNNING
-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all simulations for a given review
-- =============================================
CREATE PROCEDURE [dbo].[st_MagSimulationRun] 
	-- Add the parameters for the stored procedure here
	
	@MAG_SIMULATION_ID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @newIds table
	(
		PaperId bigint INDEX IX1 NONCLUSTERED
	)

	declare @nodes table
	(
		RowId int IDENTITY
	,	PaperId bigint primary key --INDEX IX1 CLUSTERED
	,	TimePoint int
	,	Found int
	,	Included int -- blue and red ones
	,	ConnectedNodes int -- yellow ones
	,	NotConnectedNodes int -- grey ones
	,	ClusterId int
	,	NetworkStatistic float
	,	StudyTypeScore float
	,	UserClassifierScore float
	,	Distance float
	,	EnsembleScore float
	)
	DROP TABLE IF EXISTS #edges
	create table #edges
	(
		rid bigint INDEX IX1 NONCLUSTERED
	,	ref bigint INDEX IX2 NONCLUSTERED
	)
	DROP TABLE IF EXISTS #PaperFos
	create table #PaperFos
	(
		PaperId bigint INDEX idx1 NONCLUSTERED
	,	IncludedBaseline int
	,	FieldOfStudyId bigint INDEX idx2 NONCLUSTERED
	,	Score float
	)

	declare @review_id int = 0
	declare @CreatedDate datetime
	declare @AttributeId bigint
	declare @AttributeIdFilter bigint = null
	declare @year int = 0
	declare @SearchMethod nvarchar(50)
	declare @NetworkStatistic nvarchar(50)
	declare @StudyTypeClassifier nvarchar(50)

	select @review_id = REVIEW_ID, @CreatedDate = CREATED_DATE, @AttributeId = WITH_THIS_ATTRIBUTE_ID, @year = [year],
			@AttributeIdFilter = FILTERED_BY_ATTRIBUTE_ID, @SearchMethod = SEARCH_METHOD, @NetworkStatistic = NETWORK_STATISTIC,
			@StudyTypeClassifier = STUDY_TYPE_CLASSIFIER
		FROM Reviewer.dbo.TB_MAG_SIMULATION where MAG_SIMULATION_ID = @MAG_SIMULATION_ID

	declare @log table
	(
		descr nvarchar(50)
	,	num int
	,	insertcount int
	)
	declare @Clusters table
	(
		PaperId bigint INDEX IX1 NONCLUSTERED
	,	Stat float
	,	Scaled float
	,	InternalRank int
	,	ClusterId int
	)

-- *********************************** STAGE 1 - PUT THE SEED RECORDS INTO A TABLE VARIABLE ******************************

-- seed set based on items published after a specific date
if @year <> 1753
begin
	insert into @nodes(PaperId, TimePoint, Included, ConnectedNodes, NotConnectedNodes, Found)
	select distinct imm.PaperId, CASE when p.Year < @year then 0 else 1 end, 1, 0, 0, 0
		from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Papers p on p.PaperID = imm.PaperId
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false'
			and ir.IS_INCLUDED = 'true'
		where imm.REVIEW_ID = @review_id
			and imm.AutoMatchScore > 0.2
			and imm.ManualFalseMatch <> 'true'
END

-- seed set based on items created after a specific date
if year(@CreatedDate) <> 1753
begin
	insert into @nodes(PaperId, TimePoint, Included, ConnectedNodes, NotConnectedNodes, Found)
	select distinct imm.PaperId, CASE when p.CreatedDate < @CreatedDate then 0 else 1 end, 1, 0, 0, 0
		from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Papers p on p.PaperID = imm.PaperId
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false'
			and ir.IS_INCLUDED = 'true'
		where imm.REVIEW_ID = @review_id
			and imm.AutoMatchScore > 0.2
			and imm.ManualFalseMatch <> 'true'
END

--seed set based on items with specific code  NEED TO FIX THIS ONE!
if @AttributeId > 0
begin
	insert into @nodes(PaperId, TimePoint)
	select distinct imm.PaperId, 0
	from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
	inner join Papers p on p.PaperID = imm.PaperId
	inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.IS_INCLUDED = 'true'
	inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = ir.ITEM_ID
	inner join Reviewer.dbo.TB_ITEM_SET iset on iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
	where imm.REVIEW_ID = @review_id
		and imm.AutoMatchScore > 0.2
		and imm.ManualFalseMatch <> 'true'
		and ia.ATTRIBUTE_ID = @AttributeId
END

-- *********************************** STAGE 2 APPLY THE FILTER (IF PRESENT) *********************************
if (@AttributeIdFilter <> null AND @AttributeIdFilter > 0)
begin
	delete from @nodes where PaperId in
		(select paperid from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.IS_INCLUDED = 'true'
		inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = ir.ITEM_ID
		inner join Reviewer.dbo.TB_ITEM_SET iset on iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
		where imm.REVIEW_ID = @review_id
			and imm.AutoMatchScore > 0.2
			and imm.ManualFalseMatch <> 'true'
			and ia.ATTRIBUTE_ID = @AttributeIdFilter)
end

-- ********************************** STAGE 3 identify nodes in the T0 + T1 NETWORK ******************************************

insert into @log
	select 'Seed nodes', count(*), 0 from @nodes where TimePoint = 0
insert into @log
	select 'Seeking nodes', count(*), 0 from @nodes where TimePoint = 1


declare @totalbefore int
declare @InsertCount int = 1
declare @foundbefore int

if @SearchMethod = 'Recommendations'
begin
	while @InsertCount > 0
	begin
		delete from @newIds

		;with cte (PaperId) as
		(
			select distinct pr.RecommendedPaperID from PaperRecommendations pr
				inner join @nodes seed on seed.PaperId = pr.PaperID where TimePoint = 0 or Found = 1
			union all
			select distinct pr.PaperID from PaperRecommendations pr
				inner join @nodes seed on seed.PaperId = pr.RecommendedPaperID where TimePoint = 0 or Found = 1
		) insert into @newIds(PaperId)
			select distinct paperid from cte
			where not PaperId in (select paperid from @nodes where TimePoint = 0 or found = 1)
		
		select @foundbefore = count(*) from @nodes where Found = 1
		update @nodes
			set Found = 1 where PaperId in (select PaperId from @newIds) and TimePoint = 1 and Included = 1

		select @InsertCount = count(*) - @foundbefore from @nodes where Found = 1
		
		insert into @log
			SELECT 'Found direct', count(*), @insertcount from @nodes where Found = 1
		
		insert into @nodes (PaperId, TimePoint, Included, ConnectedNodes, NotConnectedNodes, Found, ClusterId)
			select distinct Paperid, 1, 0, 1, 0, 0, 0
				from @newIds where not PaperId in (select paperid from @nodes)

		delete from @newIds

		;with cte (PaperId) as
		(
			select distinct pr.RecommendedPaperID from PaperRecommendations pr
				inner join @nodes seed on seed.PaperId = pr.PaperID where ConnectedNodes = 1
			union all
			select distinct pr.PaperID from PaperRecommendations pr
				inner join @nodes seed on seed.PaperId = pr.RecommendedPaperID where ConnectedNodes = 1
		) insert into @newIds(PaperId)
			select distinct paperid from cte
			--where not PaperId in (select paperid from @nodes where Included = 1)

		select @foundbefore = count(*) from @nodes where Found = 1
		update @nodes
			set Found = 1 where PaperId in (select PaperId from @newIds) and TimePoint = 1 and Included = 1

		select @InsertCount = @InsertCount + count(*) - @foundbefore from @nodes where Found = 1

		insert into @log
			SELECT 'FoundInGrey', count(*), @insertcount from @nodes where Found = 1 and Included = 1
		
		select @totalbefore = count(*) from @nodes
		insert into @nodes (PaperId, TimePoint, Included, ConnectedNodes, NotConnectedNodes, Found, ClusterId)
			select distinct Paperid, 1, 0, 0, 1, 0, 0
				from @newIds where not PaperId in (select paperid from @nodes)

		select @InsertCount = @InsertCount + count(*) - @totalbefore from @nodes
		
		insert into @log
			SELECT 'Total nodes', count(*), @insertcount from @nodes
	end
end

if @SearchMethod = 'Citations'
begin
	while @InsertCount > 0
	begin
		delete from @newIds

		;with cte (PaperId) as
		(
			select distinct pr.PaperReferenceId from PaperReferences pr
				inner join @nodes seed on seed.PaperId = pr.PaperID where TimePoint = 0 or Found = 1
			union all
			select distinct pr.PaperID from PaperReferences pr
				inner join @nodes seed on seed.PaperId = pr.PaperReferenceId where TimePoint = 0 or Found = 1
		) insert into @newIds(PaperId)
			select distinct paperid from cte
			where not PaperId in (select paperid from @nodes where TimePoint = 0 or found = 1)
		
		select @foundbefore = count(*) from @nodes where Found = 1
		update @nodes
			set Found = 1 where PaperId in (select PaperId from @newIds) and TimePoint = 1 and Included = 1

		select @InsertCount = count(*) - @foundbefore from @nodes where Found = 1
		
		insert into @log
			SELECT 'Found direct', count(*), @insertcount from @nodes where Found = 1
		
		insert into @nodes (PaperId, TimePoint, Included, ConnectedNodes, NotConnectedNodes, Found, ClusterId)
			select distinct Paperid, 1, 0, 1, 0, 0, 0
				from @newIds where not PaperId in (select paperid from @nodes)

		delete from @newIds

		;with cte (PaperId) as
		(
			select distinct pr.PaperReferenceId from PaperReferences pr
				inner join @nodes seed on seed.PaperId = pr.PaperID where ConnectedNodes = 1
			union all
			select distinct pr.PaperID from PaperReferences pr
				inner join @nodes seed on seed.PaperId = pr.PaperReferenceId where ConnectedNodes = 1
		) insert into @newIds(PaperId)
			select distinct paperid from cte
			--where not PaperId in (select paperid from @nodes where Included = 1)

		select @foundbefore = count(*) from @nodes where Found = 1
		update @nodes
			set Found = 1 where PaperId in (select PaperId from @newIds) and TimePoint = 1 and Included = 1

		select @InsertCount = @InsertCount + count(*) - @foundbefore from @nodes where Found = 1

		insert into @log
			SELECT 'FoundInGrey', count(*), @insertcount from @nodes where Found = 1 and Included = 1
		
		select @totalbefore = count(*) from @nodes
		insert into @nodes (PaperId, TimePoint, Included, ConnectedNodes, NotConnectedNodes, Found, ClusterId)
			select distinct Paperid, 1, 0, 0, 1, 0, 0
				from @newIds where not PaperId in (select paperid from @nodes)

		select @InsertCount = @InsertCount + count(*) - @totalbefore from @nodes
		
		insert into @log
			SELECT 'Total nodes', count(*), @insertcount from @nodes
	end
end

if @SearchMethod = 'Citations and recommendations'
begin
	while @InsertCount > 0
	begin
		delete from @newIds

		;with cte (PaperId) as
		(
			select distinct pr.RecommendedPaperID from PaperRecommendations pr
				inner join @nodes seed on seed.PaperId = pr.PaperID where TimePoint = 0 or Found = 1
			union all
			select distinct pr.PaperID from PaperRecommendations pr
				inner join @nodes seed on seed.PaperId = pr.RecommendedPaperID where TimePoint = 0 or Found = 1
			union all
			select distinct pr.PaperReferenceId from PaperReferences pr
				inner join @nodes seed on seed.PaperId = pr.PaperID where TimePoint = 0 or Found = 1
			union all
			select distinct pr.PaperID from PaperReferences pr
				inner join @nodes seed on seed.PaperId = pr.PaperReferenceId where TimePoint = 0 or Found = 1
		) insert into @newIds(PaperId)
			select distinct paperid from cte
			where not PaperId in (select paperid from @nodes where TimePoint = 0 or found = 1)
		
		select @foundbefore = count(*) from @nodes where Found = 1
		update @nodes
			set Found = 1 where PaperId in (select PaperId from @newIds) and TimePoint = 1 and Included = 1

		select @InsertCount = count(*) - @foundbefore from @nodes where Found = 1
		
		insert into @log
			SELECT 'Found direct', count(*), @insertcount from @nodes where Found = 1
		
		insert into @nodes (PaperId, TimePoint, Included, ConnectedNodes, NotConnectedNodes, Found, ClusterId)
			select distinct Paperid, 1, 0, 1, 0, 0, 0
				from @newIds where not PaperId in (select paperid from @nodes)

		delete from @newIds

		;with cte (PaperId) as
		(
			select distinct pr.RecommendedPaperID from PaperRecommendations pr
				inner join @nodes seed on seed.PaperId = pr.PaperID where ConnectedNodes = 1
			union all
			select distinct pr.PaperID from PaperRecommendations pr
				inner join @nodes seed on seed.PaperId = pr.RecommendedPaperID where ConnectedNodes = 1
			union all
			select distinct pr.PaperReferenceId from PaperReferences pr
				inner join @nodes seed on seed.PaperId = pr.PaperID where ConnectedNodes = 1
			union all
			select distinct pr.PaperID from PaperReferences pr
				inner join @nodes seed on seed.PaperId = pr.PaperReferenceId where ConnectedNodes = 1
		) insert into @newIds(PaperId)
			select distinct paperid from cte
			--where not PaperId in (select paperid from @nodes where Included = 1)

		select @foundbefore = count(*) from @nodes where Found = 1
		update @nodes
			set Found = 1 where PaperId in (select PaperId from @newIds) and TimePoint = 1 and Included = 1

		select @InsertCount = @InsertCount + count(*) - @foundbefore from @nodes where Found = 1

		insert into @log
			SELECT 'FoundInGrey', count(*), @insertcount from @nodes where Found = 1 and Included = 1
		
		select @totalbefore = count(*) from @nodes
		insert into @nodes (PaperId, TimePoint, Included, ConnectedNodes, NotConnectedNodes, Found, ClusterId)
			select distinct Paperid, 1, 0, 0, 1, 0, 0
				from @newIds where not PaperId in (select paperid from @nodes)

		select @InsertCount = @InsertCount + count(*) - @totalbefore from @nodes
		
		insert into @log
			SELECT 'Total nodes', count(*), @insertcount from @nodes

	end
end

/*
if @SearchMethod = 'Fields of study'
begin

end
*/

insert into @log
		select 'number found', count(*), 0 from @nodes where Found = 1
	insert into @log
		select 'number not found', count(*), 0 from @nodes where TimePoint = 1 and Found = 0 and Included = 1


-- ******************************* STAGE 4 Get the edges data ***************************************
if @SearchMethod = 'Recommendations'
begin
	insert into #edges(rid, ref)
	select pr.paperid, pr.RecommendedPaperId from AcademicSmoke.dbo.PaperRecommendations pr
	inner join @nodes n on n.PaperId = pr.PaperId  and not (n.Included = 1 and n.Found = 0)
	union all 
	select pr.paperid, pr.RecommendedPaperId from AcademicSmoke.dbo.PaperRecommendations pr
	inner join @nodes n on n.PaperId = pr.RecommendedPaperId and not (n.Included = 1 and n.Found = 0)
end
if @SearchMethod = 'Citations'
begin
	insert into #edges(rid, ref)
	select pr.paperid, pr.PaperReferenceId from AcademicSmoke.dbo.PaperReferences pr
	inner join @nodes n on n.PaperId = pr.PaperId  and not (n.Included = 1 and n.Found = 0)
	union all 
	select pr.paperid, pr.PaperReferenceId from AcademicSmoke.dbo.PaperReferences pr
	inner join @nodes n on n.PaperId = pr.PaperReferenceId and not (n.Included = 1 and n.Found = 0)
end
if @SearchMethod = 'Citations and recommendations'
begin
	insert into #edges(rid, ref)
	select pr.paperid, pr.RecommendedPaperId from AcademicSmoke.dbo.PaperRecommendations pr
	inner join @nodes n on n.PaperId = pr.PaperId  and not (n.Included = 1 and n.Found = 0)
	union all 
	select pr.paperid, pr.RecommendedPaperId from AcademicSmoke.dbo.PaperRecommendations pr
	inner join @nodes n on n.PaperId = pr.RecommendedPaperId and not (n.Included = 1 and n.Found = 0)
	union all
	select pr.paperid, pr.PaperReferenceId from AcademicSmoke.dbo.PaperReferences pr
	inner join @nodes n on n.PaperId = pr.PaperId  and not (n.Included = 1 and n.Found = 0)
	union all 
	select pr.paperid, pr.PaperReferenceId from AcademicSmoke.dbo.PaperReferences pr
	inner join @nodes n on n.PaperId = pr.PaperReferenceId and not (n.Included = 1 and n.Found = 0)
end

	insert into @log
		select 'edge count', count(*), 0 from #edges



-- ************************ STAGE 5 Get the network stats *******************************************

declare @NetworkStatisticCommand nvarchar(max) = 'None'
declare @ClusterCommand1 nvarchar(4000) =
N'
library(igraph);
library(scales)

g <- igraph::graph_from_data_frame(InputDataSet);
cluster <- clusters(g)
res <- data.frame(cluster$membership)
'
declare @ClusterCommand2 nvarchar(4000) =
N'
finalres <- cbind(dcdata, res)
finalres <- cbind(rid = as.numeric(rownames(finalres)), finalres)

OutputDataSet <- finalres;'

if @NetworkStatistic = 'degree' set @NetworkStatisticCommand =
N'
dcdata <- data.frame(degree = degree(graph = g))
sdc <- rescale(dcdata[, 1], to = c(0, 1), from = range(dcdata[, 1], na.rm = TRUE, finite = TRUE))
dcdata <- cbind(dcdata, sdc)
dcdata <- data.frame(dcdata, dcrank = rank(-dcdata[, 2], na.last = TRUE, ties.method = c("min")))
'
if @NetworkStatistic = 'closeness' set @NetworkStatisticCommand =
N'
dcdata <- data.frame(closeness = closeness(g, mode = "all"))
scc <- rescale(dcdata[, 1], to = c(0, 1), from = range(dcdata[, 1], na.rm = TRUE, finite = TRUE))
dcdata <- cbind(dcdata, scc)
dcdata <- data.frame(dcdata, ccrank = rank(-dcdata[, 2], na.last = TRUE, ties.method = c("min")))
'
if @NetworkStatistic = 'eigenscore' set @NetworkStatisticCommand =
N'
dcdata <- data.frame(eigenscore = eigen_centrality(g)$vector)
sec <- rescale(dcdata[, 1], to = c(0, 1), from = range(dcdata[, 1], na.rm = TRUE, finite = TRUE))
dcdata <- cbind(dcdata, sec)
dcdata <- data.frame(dcdata, ecrank = rank(-dcdata[, 2], na.last = TRUE, ties.method = c("min")))
'
if @NetworkStatistic = 'pagerank' set @NetworkStatisticCommand =
N'
dcdata <- data.frame(pagerank = page_rank(g)$vector)
spr <- rescale(dcdata[, 1], to = c(0, 1), from = range(dcdata[, 1], na.rm = TRUE, finite = TRUE))
dcdata <- cbind(dcdata, spr)
dcdata <- data.frame(dcdata, prrank = rank(-dcdata[, 2], na.last = TRUE, ties.method = c("min")))
'
if @NetworkStatistic = 'hubscore' set @NetworkStatisticCommand =
N'
dcdata <- data.frame(hubscore = hub.score(g)$vector)
shb <- rescale(dcdata[, 1], to = c(0, 1), from = range(dcdata[, 1], na.rm = TRUE, finite = TRUE))
dcdata <- cbind(dcdata, shb)
dcdata <- data.frame(dcdata, hbrank = rank(-dcdata[, 2], na.last = TRUE, ties.method = c("min")))
'
if @NetworkStatistic = 'authscore' set @NetworkStatisticCommand =
N'
dcdata <- data.frame(authscore = authority.score(g)$vector)
sau <- rescale(dcdata[, 1], to = c(0, 1), from = range(dcdata[, 1], na.rm = TRUE, finite = TRUE))
dcdata <- cbind(dcdata, sau)
dcdata <- data.frame(dcdata, aurank = rank(-dcdata[, 2], na.last = TRUE, ties.method = c("min")))
'
if @NetworkStatistic = 'alpha' set @NetworkStatisticCommand =
N'
dcdata <- data.frame(alphascore = alpha_centrality(g, nodes = V(g)))
sau <- rescale(dcdata[, 1], to = c(0, 1), from = range(dcdata[, 1], na.rm = TRUE, finite = TRUE))
dcdata <- cbind(dcdata, sau)
dcdata <- data.frame(dcdata, alpharank = rank(-dcdata[, 2], na.last = TRUE, ties.method = c("min")))
'
if @NetworkStatisticCommand <> 'None'
begin
	Select @NetworkStatisticCommand = Concat(@ClusterCommand1, @NetworkStatisticCommand, @ClusterCommand2)
	insert into @Clusters
	(	PaperId
	,	Stat 
	,	Scaled 
	,	InternalRank
	,	ClusterId
	)
	EXECUTE sp_execute_external_script @language = N'R'
		, @script = @NetworkStatisticCommand, @input_data_1 = N'SELECT * from #edges;'

	update n
		set n.ClusterId = c.ClusterId
		,	n.NetworkStatistic = c.Scaled
		from @nodes n inner join @Clusters c on c.PaperId = n.PaperId

	update @nodes
		set NetworkStatistic = 0 where NetworkStatistic is null
end

-- ******************************* STAGE 6 GET THE STUDY TYPE CLASSIFIER STATS *********************************
if @StudyTypeClassifier = 'RCT'
	update n
		set n.StudyTypeScore = r.Score
		from @nodes n inner join AcademicSmoke.dbo.RCTScores r on n.PaperId = r.PaperId
/*
if @StudyTypeClassifier = 'Cochrane RCT'
	update n
		set n.StudyTypeScore = r.Score
		from @nodes n inner join AcademicSmoke.dbo.RCTScores r on n.PaperId = r.PaperId

if @StudyTypeClassifier = 'Economic evaluation'
	update n
		set n.StudyTypeScore = r.Score
		from @nodes n inner join AcademicSmoke.dbo.RCTScores r on n.PaperId = r.PaperId

if @StudyTypeClassifier = 'Systematic review'
	update n
		set n.StudyTypeScore = r.Score
		from @nodes n inner join AcademicSmoke.dbo.RCTScores r on n.PaperId = r.PaperId
*/


-- ******************************** STAGE 7 GET FIELD OF STUDY SIMILARITY SCORES *******************************

insert into #PaperFos
	select pfos.PaperId, case when n.Included = 1 and n.TimePoint = 0 then 1 else 0 end,
		FieldOfStudyId, Score from AcademicSmoke.dbo.PaperFieldsOfStudy pfos
	inner join @nodes n on n.PaperId = pfos.PaperId

insert into @log
	select 'Unique items in PaperFos', count(distinct PaperId), 0 from #PaperFos

declare @tt table
( PaperId float, AverageDistance float)

insert into @tt (PaperId, AverageDistance)
EXECUTE sp_execute_external_script @language = N'Python'
  , @script = N'
import pandas as pd
from scipy.spatial.distance import cdist

myData = pd.DataFrame(InputDataSet)
fulldata = myData.pivot_table(index=["PaperId", "IncludedBaseline"], columns="FieldOfStudyId", values="Score")
fulldata = fulldata.fillna(0).reset_index()
fulldata.set_index("PaperId", inplace=True)
ReferenceSet = fulldata.loc[fulldata.IncludedBaseline == 1]
NewSet = fulldata.loc[fulldata.IncludedBaseline == 0]
t = pd.DataFrame(cdist(NewSet, ReferenceSet, metric="cosine"), index = NewSet.index)
t["avg"] = t.mean(axis=1)
t = t.reset_index()
OutputDataSet = t[["PaperId", "avg"]]
'
, @input_data_1 = N'SELECT PaperId, IncludedBaseline, FieldOfStudyId, Score from #PaperFos;'


insert into @log
	select 'Rows in distance calculation', count(*), 0 from #PaperFos
insert into @log
	select 'Unique items from distance calculation', count(distinct PaperId), 0 from @tt

update n
	set Distance = AverageDistance
	from @nodes n
		inner join @tt t on cast(t.PaperId as bigint) = n.PaperId -- Has to output as float, as python can't do bigint
			
update @nodes
	set Distance = 1 where Distance is null

-- ********************************* STAGE 8: WRITE RESULTS TO THE DATABASE *********************************

DELETE FROM Reviewer.dbo.TB_MAG_SIMULATION_RESULT where MAG_SIMULATION_ID = @MAG_SIMULATION_ID -- needed for debugging - can comment out later

insert into Reviewer.dbo.TB_MAG_SIMULATION_RESULT (MAG_SIMULATION_ID, PaperId, INCLUDED, FOUND, SEED,
	STUDY_TYPE_CLASSIFIER_SCORE, USER_CLASSIFIER_MODEL_SCORE, NETWORK_STATISTIC_SCORE,
	FOS_DISTANCE_SCORE, ENSEMBLE_SCORE)
	SELECT @MAG_SIMULATION_ID, PaperId, Included, Found, CASE when TimePoint = 0 then 'True' else 'False' end,
		StudyTypeScore, UserClassifierScore, NetworkStatistic, Distance, EnsembleScore
		from @nodes

update Reviewer.dbo.TB_MAG_SIMULATION
	set STATUS = 'Complete',
		TP = (select count(*) from @nodes where TimePoint = 1 and Found = 1 and Included = 1),
		FP = (select count(*) from @nodes where TimePoint = 1 and Included = 0 and (ConnectedNodes = 1 or NotConnectedNodes = 1)),
		FN = (select count(*) from @nodes where TimePoint = 1 and Found = 0 and Included = 1),
		NSEEDS = (select count(*) from @nodes where TimePoint = 0 and Included = 1)
	where MAG_SIMULATION_ID = @MAG_SIMULATION_ID

DROP TABLE IF EXISTS #edges
DROP TABLE IF EXISTS #PaperFos

--select * from #PaperFos
--select * from @log
--select * from @nodes
END
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulations]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all simulations for a given review
-- =============================================
CREATE PROCEDURE [dbo].[st_MagSimulations] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here


	SELECT ms.REVIEW_ID, ms.MAG_SIMULATION_ID, ms.YEAR, ms.CREATED_DATE, ms.WITH_THIS_ATTRIBUTE_ID, ms.FILTERED_BY_ATTRIBUTE_ID,
		ms.SEARCH_METHOD, ms.NETWORK_STATISTIC, ms.STUDY_TYPE_CLASSIFIER, ms.USER_CLASSIFIER_MODEL_ID,
		ms.STATUS, ms.tp, ms.FP, ms.FN, MS.NSEEDS, a.ATTRIBUTE_NAME FilteredByAttribute,
		aa.ATTRIBUTE_NAME WithThisAttribute, cm.MODEL_TITLE from REVIEWER.DBO.TB_MAG_SIMULATION ms
		left outer join Reviewer.dbo.TB_ATTRIBUTE a on a.ATTRIBUTE_ID = ms.FILTERED_BY_ATTRIBUTE_ID
		left outer join Reviewer.dbo.TB_ATTRIBUTE aa on aa.ATTRIBUTE_ID = ms.WITH_THIS_ATTRIBUTE_ID
		left outer join Reviewer.dbo.tb_CLASSIFIER_MODEL cm on cm.MODEL_ID = ms.USER_CLASSIFIER_MODEL_ID
		WHERE ms.REVIEW_ID = @REVIEW_ID
		order by ms.MAG_SIMULATION_ID
		

END
GO
/****** Object:  StoredProcedure [dbo].[st_MatchItemsToPapers]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--RunningMatches
-- DON'T REMOVE THE LINE ABOVE - IT'S USED IN THE WORKER PROCESS TO CHECK WHETHER THIS PROC IS RUNNING
-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Match EPPI-Reviewer TB_ITEM records to MAG Papers - everything in a review
-- =============================================
CREATE   PROCEDURE [dbo].[st_MatchItemsToPapers] 
	-- Add the parameters for the stored procedure here

/*
	@REVIEW_ID INT = 0
,	@CONTACT_ID INT
,	@RESULT nvarchar(50) = '' OUTPUT
*/
	@REVIEW_JOB_ID int = null
,	@REVIEW_ID INT
,	@CONTACT_ID INT = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	/*
	Declare @LAST_RUN NVARCHAR(50)
	Declare @START_TIME DATETIME
	Declare @NewReviewJob int
	
	
	select TOP(1) @LAST_RUN = CURRENT_STATE, @START_TIME = START_TIME
		FROM Reviewer.dbo.TB_REVIEW_JOB
		WHERE REVIEW_ID = @REVIEW_ID AND JOB_TYPE = 'MAG Item Matches'
		ORDER BY REVIEW_JOB_ID DESC

	IF @LAST_RUN = 'Running' AND GETDATE() < DATEADD(HOUR, 3, @START_TIME)
	BEGIN --send back a return code to signify that the matching process is still running
		set @RESULT = 'There is currently a matching process running on this review. Please check back later.'
		return -2
	END
	ELSE
	IF @LAST_RUN = 'Error' OR  (GETDATE() > DATEADD(HOUR, 3, @START_TIME) and @LAST_RUN = 'Running')
	BEGIN
		set @RESULT = 'There was an error when running the last matching process. Please contact support.'
		return -3 -- Error when running the matching
	END

	Insert into Reviewer.dbo.tb_REVIEW_JOB(REVIEW_ID, CONTACT_ID, START_TIME, JOB_TYPE, CURRENT_STATE, SUCCESS)
	values (@REVIEW_ID, @CONTACT_ID, GETDATE(), 'MAG Item Matches', 'Running', 0)
	set @NewReviewJob = @@IDENTITY
	*/

if not @REVIEW_JOB_ID is null
begin
	update reviewer.dbo.tb_REVIEW_JOB
		set START_TIME = GETDATE(),
		CURRENT_STATE = 'Running',
		SUCCESS = 0
		where REVIEW_JOB_ID = @REVIEW_JOB_ID
end
else
begin
	Insert into Reviewer.dbo.tb_REVIEW_JOB(REVIEW_ID, CONTACT_ID, START_TIME, JOB_TYPE, CURRENT_STATE, SUCCESS)
		values (@REVIEW_ID, @CONTACT_ID, GETDATE(), 'MAG Item Matches', 'Running', 0)
	set @REVIEW_JOB_ID = @@IDENTITY
end

UPDATE i
	set SearchText = dbo.ToShortSearchText(TITLE)
from Reviewer.dbo.TB_ITEM i
	inner join Reviewer.dbo.TB_ITEM_REVIEW tir on tir.ITEM_ID = i.ITEM_ID
	and tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
	and (i.SearchText = '' or i.SearchText is null)

declare @MatchedStepDoi Table
(
	RecordId bigint INDEX idxMSDOI1 CLUSTERED
,	PaperId bigint INDEX idxMSDOI2 NONCLUSTERED
,	JournalJaro float
,	VolumeMatch int
,	PageMatch int
,	YearMatch int
,	AllAuthorsLeven float
,	FirstAuthorJaro float
,	TitleLeven float
,	AutoMatchScore float
,	INDEX IX_RecordId NONCLUSTERED (RecordId)
,	INDEX IX_PaperId NONCLUSTERED (PaperId)
)
declare @MatchedStepSearchText Table
(
	RecordId bigint INDEX idxMSST1 CLUSTERED
,	PaperId bigint INDEX idxMSST2 NONCLUSTERED
,	JournalJaro float
,	VolumeMatch int
,	PageMatch int
,	YearMatch int
,	AllAuthorsLeven float
,	FirstAuthorJaro float
,	TitleLeven float
,	AutoMatchScore float
,	INDEX IX_RecordId NONCLUSTERED (RecordId)
,	INDEX IX_PaperId NONCLUSTERED (PaperId)
)
declare @MatchedStepVolFirstPage Table
(
	RecordId bigint INDEX idxMSVFP1 CLUSTERED
,	PaperId bigint INDEX idxMSVFP2 NONCLUSTERED
,	JournalJaro float
,	VolumeMatch int
,	PageMatch int
,	YearMatch int
,	AllAuthorsLeven float
,	FirstAuthorJaro float
,	TitleLeven float
,	AutoMatchScore float
,	INDEX IX_RecordId NONCLUSTERED (RecordId)
,	INDEX IX_PaperId NONCLUSTERED (PaperId)
)
declare @Results Table
(
	RecordId bigint
,	PaperId bigint
,	JournalJaro float
,	VolumeMatch int
,	PageMatch int
,	YearMatch int
,	AllAuthorsLeven float
,	FirstAuthorJaro float
,	TitleLeven float
,	AutoMatchScore float
,	INDEX IX_RecordId NONCLUSTERED (RecordId)
,	INDEX IX_PaperId NONCLUSTERED (PaperId)
)
declare @ItemInfo Table
(
	ItemId bigint
,	Info int
)

insert into @MatchedStepDoi(RecordId, PaperId, JournalJaro, VolumeMatch, PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
select distinct
	t.ITEM_ID
,	p.PaperID
,	iif(t.PARENT_TITLE is not null and j.NormalizedName is not null, dbo.Jaro(t.PARENT_TITLE, j.NormalizedName), -1) as JournalJaro
,	iif(t.VOLUME = p.VOLUME and not t.VOLUME is null, 1, 0) VolMatch
,	iif(iif (CHARINDEX('-', t.PAGES, 0) > 0, substring(t.PAGES, 0, CHARINDEX('-', t.PAGES, 0))  , t.PAGES) = p.FirstPage, 1, 0) PageMatch
,	iif(NOT t.YEAR is null AND t.YEAR = p.YEAR, 1, 0) YearMatch
,	dbo.LevenshteinSVF(dbo.ToShortSearchText(dbo.fn_ReviewerItemAuthors(t.ITEM_ID)), 
		dbo.ToShortSearchText(dbo.fn_PaperAuthors(p.PaperID)) ) AllAuthorsLeven
,	dbo.Jaro(ItemAuthor.LAST, dbo.fn_PaperFirstAuthor(p.PaperID)) FirstAuthorJaro
,	dbo.LevenshteinSVF(p.SearchText, t.SearchText) TitleLeven
from Reviewer.dbo.TB_ITEM t
inner join Reviewer.dbo.TB_ITEM_REVIEW tir on tir.ITEM_ID = t.ITEM_ID and tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
inner join Papers p on p.DOI = t.DOI
left join Reviewer.dbo.TB_ITEM_AUTHOR ItemAuthor on ItemAuthor.ITEM_ID = t.ITEM_ID and ItemAuthor.ROLE = 0 and ItemAuthor.RANK = 0
left join Journals j on j.JournalId = p.JournalID
left join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = tir.ITEM_ID 
	where imm.ITEM_ID is null and not t.TITLE is null AND LEN(T.DOI) > 5

update @MatchedStepDoi
set AutoMatchScore = 1 where
	((JournalJaro >= 90 AND VolumeMatch = 1 AND PageMatch = 1) OR
	(JournalJaro < 90 AND VolumeMatch = 1 AND PageMatch = 1 and AllAuthorsLeven > 30) OR
	(JournalJaro >= 50 and VolumeMatch = 1 and PageMatch = 1 AND FirstAuthorJaro >= 90))

insert into @MatchedStepSearchText(RecordId, PaperId, JournalJaro, VolumeMatch,	PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
select distinct
	t.ITEM_ID
,	p.PaperID
,	iif(t.PARENT_TITLE is not null and j.NormalizedName is not null, dbo.Jaro(t.PARENT_TITLE, j.NormalizedName), -1) as JournalJaro
,	iif(t.VOLUME = p.VOLUME and not t.VOLUME is null, 1, 0) VolMatch
,	iif(iif (CHARINDEX('-', t.PAGES, 0) > 0, substring(t.PAGES, 0, CHARINDEX('-', t.PAGES, 0))  , t.PAGES) = p.FirstPage, 1, 0) PageMatch
,	iif(NOT t.YEAR is null AND t.YEAR = p.YEAR, 1, 0) YearMatch
,	dbo.LevenshteinSVF(dbo.ToShortSearchText(dbo.fn_ReviewerItemAuthors(t.ITEM_ID)),
		dbo.ToShortSearchText(dbo.fn_PaperAuthors(p.PaperID)) ) AllAuthorsLeven
,	dbo.Jaro(ItemAuthor.LAST, dbo.fn_PaperFirstAuthor(p.PaperID)) FirstAuthorJaro
,	dbo.LevenshteinSVF(p.SearchText, t.SearchText) TitleLeven
from Reviewer.dbo.TB_ITEM t
inner join Reviewer.dbo.TB_ITEM_REVIEW tir on tir.ITEM_ID = t.ITEM_ID and tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
inner join Papers p on p.SearchText = t.SearchText
left join Reviewer.dbo.TB_ITEM_AUTHOR ItemAuthor on ItemAuthor.ITEM_ID = t.ITEM_ID and ItemAuthor.ROLE = 0 and ItemAuthor.RANK = 0
left join Journals j on j.JournalId = p.JournalID
left join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = tir.ITEM_ID
left join @MatchedStepDoi msdoi on msdoi.RecordId = t.ITEM_ID
	where msdoi.RecordId is null and imm.ITEM_ID is null and not t.TITLE is null

update @MatchedStepSearchText
set AutoMatchScore = 1 where
	((JournalJaro >= 90 AND VolumeMatch = 1 AND PageMatch = 1) OR
	(JournalJaro < 90 AND VolumeMatch = 1 AND PageMatch = 1 and AllAuthorsLeven > 30) OR
	(JournalJaro >= 50 and VolumeMatch = 1 and PageMatch = 1 AND FirstAuthorJaro >= 90))

insert into @MatchedStepVolFirstPage(RecordId, PaperId, JournalJaro, VolumeMatch, PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
select distinct
	t.ITEM_ID
,	p.PaperID
,	iif(t.PARENT_TITLE is not null and j.NormalizedName is not null, dbo.Jaro(t.PARENT_TITLE, j.NormalizedName), -1) as JournalJaro
,	1 as VolumeMatch
,	1 as PageMatch
,	iif(NOT t.YEAR is null AND t.YEAR = cast (p.YEAR as nvarchar(50)), 1, 0) YearMatch
,	dbo.LevenshteinSVF(dbo.ToShortSearchText(dbo.fn_ReviewerItemAuthors(t.ITEM_ID)), 
		dbo.ToShortSearchText(dbo.fn_PaperAuthors(p.PaperID)) ) AllAuthorsLeven
,	dbo.Jaro(ItemAuthor.LAST, dbo.fn_PaperFirstAuthor(p.PaperID)) FirstAuthorJaro
,	dbo.LevenshteinSVF(p.SearchText, t.SearchText) TitleLeven
from Reviewer.dbo.TB_ITEM t
inner join Reviewer.dbo.TB_ITEM_REVIEW tir on tir.ITEM_ID = t.ITEM_ID and tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
inner join Papers p on p.Volume = t.VOLUME and substring(t.PAGES, 0, CHARINDEX('-', t.PAGES, 0)) = Cast(p.FirstPage as nvarchar(50))
left join Reviewer.dbo.TB_ITEM_AUTHOR ItemAuthor on ItemAuthor.ITEM_ID = t.ITEM_ID and ItemAuthor.ROLE = 0 and ItemAuthor.RANK = 0
left join Journals j on j.JournalId = p.JournalID
left join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = tir.ITEM_ID 
left join @MatchedStepSearchText msst on msst.RecordId = t.ITEM_ID
left join @MatchedStepDoi msdoi on msdoi.RecordId = t.ITEM_ID
	where imm.ITEM_ID is null and msst.RecordId is null and msdoi.RecordId is null
	and not t.TITLE is null and not t.VOLUME is null and not t.VOLUME = ''
	and not len(p.SearchText) < 5
	and not p.Volume is null and not p.Volume = ''
	and not t.PAGES is null and not t.PAGES = ''
	and not p.FirstPage is null and not p.FirstPage = ''

update @MatchedStepVolFirstPage
	set AutoMatchScore = 1 where
	(JournalJaro + AllAuthorsLeven + FirstAuthorJaro + (TitleLeven / 100)) / 4 >= 0.50 and TitleLeven >= 85

insert into @Results
	select * from @MatchedStepSearchText order by AutoMatchScore

delete from doi
	from @MatchedStepDoi doi
		inner join @Results r on r.PaperId = doi.PaperId and r.RecordId = doi.RecordId and r.AutoMatchScore = doi.AutoMatchScore

insert into @Results(RecordId, PaperId, JournalJaro, VolumeMatch, PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
	select doi.RecordId, doi.PaperId, doi.JournalJaro, doi.VolumeMatch, doi.PageMatch, YearMatch, doi.AllAuthorsLeven,
		doi.FirstAuthorJaro, TitleLeven
		from @MatchedStepDoi doi 
		order by AutoMatchScore

delete from volf
	from @MatchedStepVolFirstPage volf
	inner join @Results r on r.PaperId = volf.PaperId and r.RecordId = volf.RecordId and r.AutoMatchScore = volf.AutoMatchScore

insert into @Results(RecordId, PaperId, JournalJaro, VolumeMatch, PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
	select VolFirst.RecordId, VolFirst.PaperId, VolFirst.JournalJaro, VolFirst.VolumeMatch, VolFirst.PageMatch, YearMatch, VolFirst.AllAuthorsLeven,
		VolFirst.FirstAuthorJaro, TitleLeven
		from @MatchedStepVolFirstPage VolFirst 
		order by AutoMatchScore


-- ********** SANITY CHECK: should only be 1 automatch for any combination of record and paper ID where automatchscore = 1 *************

--select count(*) from @Results r
--	inner join @Results r2 on r2.PaperId = r.RecordId and r2.RecordId = r.PaperId

-- ********** THE ABOVE COUNT MUST BE ZERO. Need to address if it's not. **********************


-- clear out some of the noise

DELETE FROM @Results
	WHERE not
	(
		(TitleLeven > 90 and AllAuthorsLeven > 0.9)
		or (TitleLeven > 90 and PageMatch = 1)
		or (TitleLeven > 90 and JournalJaro > 0.9)
		or (AllAuthorsLeven > 0.9 and JournalJaro > 0.9)
		or (AllAuthorsLeven > 0.9 and PageMatch = 1)
		or (JournalJaro > 0.9 and PageMatch = 1)
	) 

delete from @Results
	where not TitleLeven >= 40
--delete from @Results
--	where not AllAuthorsJaro >= 0.75

-- Set AutoMatchScore for everything that's not automatch = 1
update @Results
	set AutoMatchScore = ((TitleLeven / 100 * 2.71) + (VolumeMatch * 0.02) + (PageMatch * 0.18) + 
		(YearMatch * 0.82) + (JournalJaro * 0.55) + (AllAuthorsLeven /100 * 1.25)) / 5.53
	where AutoMatchScore is null

insert into Reviewer.dbo.tb_ITEM_MAG_MATCH(ITEM_ID, REVIEW_ID, PaperId, AutoMatchScore, ManualFalseMatch, ManualTrueMatch)
select RecordId, @REVIEW_ID, PaperId, AutoMatchScore, 'false', 'false' from @Results

update reviewer.dbo.tb_REVIEW_JOB
	set END_TIME = GETDATE(),
	CURRENT_STATE = 'Complete',
	SUCCESS = 1
	where REVIEW_JOB_ID = @REVIEW_JOB_ID

END
GO
/****** Object:  StoredProcedure [dbo].[st_MatchItemsToPapersAddJob]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add a record in TB_ITEM_JOB to match papers in review
-- =============================================
CREATE     PROCEDURE [dbo].[st_MatchItemsToPapersAddJob] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@CONTACT_ID INT
,	@RESULT INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select * from Reviewer.dbo.tb_REVIEW_JOB
	where REVIEW_ID = @REVIEW_ID and JOB_TYPE = 'MAG Item Matches' and CURRENT_STATE = 'Pending'

	if @@ROWCOUNT = 0
	begin
		Insert into Reviewer.dbo.tb_REVIEW_JOB(REVIEW_ID, CONTACT_ID, START_TIME, JOB_TYPE, CURRENT_STATE, SUCCESS)
			values (@REVIEW_ID, @CONTACT_ID, GETDATE(), 'MAG Item Matches', 'Pending', 0)
	end

	SET @RESULT = 1
END
GO
/****** Object:  StoredProcedure [dbo].[st_MatchItemsToPapersSingleItem]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Match EPPI-Reviewer TB_ITEM records to MAG Papers
-- =============================================
CREATE     PROCEDURE [dbo].[st_MatchItemsToPapersSingleItem] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT = 0
,	@ITEM_ID BIGINT = 0
,	@CONTACT_ID INT
,	@RESULT int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

UPDATE Reviewer.dbo.TB_ITEM
	set SearchText = dbo.ToShortSearchText(TITLE)
	where ITEM_ID = @ITEM_ID

declare @MatchedStepDoi Table
(
	RecordId bigint
,	PaperId bigint
,	JournalJaro float
,	VolumeMatch int
,	PageMatch int
,	YearMatch int
,	AllAuthorsLeven float
,	FirstAuthorJaro float
,	TitleLeven float
,	AutoMatchScore float
,	INDEX IX_RecordId NONCLUSTERED (RecordId)
,	INDEX IX_PaperId NONCLUSTERED (PaperId)
)
declare @MatchedStepSearchText Table
(
	RecordId bigint
,	PaperId bigint
,	JournalJaro float
,	VolumeMatch int
,	PageMatch int
,	YearMatch int
,	AllAuthorsLeven float
,	FirstAuthorJaro float
,	TitleLeven float
,	AutoMatchScore float
,	INDEX IX_RecordId NONCLUSTERED (RecordId)
,	INDEX IX_PaperId NONCLUSTERED (PaperId)
)
declare @MatchedStepVolFirstPage Table
(
	RecordId bigint
,	PaperId bigint
,	JournalJaro float
,	VolumeMatch int
,	PageMatch int
,	YearMatch int
,	AllAuthorsLeven float
,	FirstAuthorJaro float
,	TitleLeven float
,	AutoMatchScore float
,	INDEX IX_RecordId NONCLUSTERED (RecordId)
,	INDEX IX_PaperId NONCLUSTERED (PaperId)
)
declare @Results Table
(
	RecordId bigint
,	PaperId bigint
,	JournalJaro float
,	VolumeMatch int
,	PageMatch int
,	YearMatch int
,	AllAuthorsLeven float
,	FirstAuthorJaro float
,	TitleLeven float
,	AutoMatchScore float
,	INDEX IX_RecordId NONCLUSTERED (RecordId)
,	INDEX IX_PaperId NONCLUSTERED (PaperId)
)
declare @ItemInfo Table
(
	ItemId bigint
,	Info int
)

insert into @MatchedStepDoi(RecordId, PaperId, JournalJaro, VolumeMatch, PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
select distinct
	t.ITEM_ID
,	p.PaperID
,	iif(t.PARENT_TITLE is not null and j.NormalizedName is not null, dbo.Jaro(t.PARENT_TITLE, j.NormalizedName), -1) as JournalJaro
,	iif(t.VOLUME = p.VOLUME and not t.VOLUME is null, 1, 0) VolMatch
,	iif(iif (CHARINDEX('-', t.PAGES, 0) > 0, substring(t.PAGES, 0, CHARINDEX('-', t.PAGES, 0))  , t.PAGES) = p.FirstPage, 1, 0) PageMatch
,	iif(NOT t.YEAR is null AND t.YEAR = p.YEAR, 1, 0) YearMatch
,	dbo.LevenshteinSVF(dbo.ToShortSearchText(dbo.fn_ReviewerItemAuthors(t.ITEM_ID)), 
		dbo.ToShortSearchText(dbo.fn_PaperAuthors(p.PaperID)) ) AllAuthorsLeven
,	dbo.Jaro(ItemAuthor.LAST, dbo.fn_PaperFirstAuthor(p.PaperID)) FirstAuthorJaro
,	dbo.LevenshteinSVF(p.SearchText, t.SearchText) TitleLeven
from Reviewer.dbo.TB_ITEM t
inner join Reviewer.dbo.TB_ITEM_REVIEW tir on tir.ITEM_ID = t.ITEM_ID and tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
inner join Papers p on p.DOI = t.DOI
left join Reviewer.dbo.TB_ITEM_AUTHOR ItemAuthor on ItemAuthor.ITEM_ID = t.ITEM_ID and ItemAuthor.ROLE = 0 and ItemAuthor.RANK = 0
left join Journals j on j.JournalId = p.JournalID
left join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = tir.ITEM_ID 
	where imm.ITEM_ID is null and not t.TITLE is null AND LEN(T.DOI) > 5 AND t.ITEM_ID = @ITEM_ID

update @MatchedStepDoi
set AutoMatchScore = 1 where
	((JournalJaro >= 90 AND VolumeMatch = 1 AND PageMatch = 1) OR
	(JournalJaro < 90 AND VolumeMatch = 1 AND PageMatch = 1 and AllAuthorsLeven > 30) OR
	(JournalJaro >= 50 and VolumeMatch = 1 and PageMatch = 1 AND FirstAuthorJaro >= 90))

insert into @MatchedStepSearchText(RecordId, PaperId, JournalJaro, VolumeMatch,	PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
select distinct
	t.ITEM_ID
,	p.PaperID
,	iif(t.PARENT_TITLE is not null and j.NormalizedName is not null, dbo.Jaro(t.PARENT_TITLE, j.NormalizedName), -1) as JournalJaro
,	iif(t.VOLUME = p.VOLUME and not t.VOLUME is null, 1, 0) VolMatch
,	iif(iif (CHARINDEX('-', t.PAGES, 0) > 0, substring(t.PAGES, 0, CHARINDEX('-', t.PAGES, 0))  , t.PAGES) = p.FirstPage, 1, 0) PageMatch
,	iif(NOT t.YEAR is null AND t.YEAR = p.YEAR, 1, 0) YearMatch
,	dbo.LevenshteinSVF(dbo.ToShortSearchText(dbo.fn_ReviewerItemAuthors(t.ITEM_ID)),
		dbo.ToShortSearchText(dbo.fn_PaperAuthors(p.PaperID)) ) AllAuthorsLeven
,	dbo.Jaro(ItemAuthor.LAST, dbo.fn_PaperFirstAuthor(p.PaperID)) FirstAuthorJaro
,	dbo.LevenshteinSVF(p.SearchText, t.SearchText) TitleLeven
from Reviewer.dbo.TB_ITEM t
inner join Reviewer.dbo.TB_ITEM_REVIEW tir on tir.ITEM_ID = t.ITEM_ID and tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
inner join Papers p on p.SearchText = t.SearchText
left join Reviewer.dbo.TB_ITEM_AUTHOR ItemAuthor on ItemAuthor.ITEM_ID = t.ITEM_ID and ItemAuthor.ROLE = 0 and ItemAuthor.RANK = 0
left join Journals j on j.JournalId = p.JournalID
left join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = tir.ITEM_ID
left join @MatchedStepDoi msdoi on msdoi.RecordId = t.ITEM_ID
	where msdoi.RecordId is null and imm.ITEM_ID is null and not t.TITLE is null
	AND t.ITEM_ID = @ITEM_ID

update @MatchedStepSearchText
set AutoMatchScore = 1 where
	((JournalJaro >= 90 AND VolumeMatch = 1 AND PageMatch = 1) OR
	(JournalJaro < 90 AND VolumeMatch = 1 AND PageMatch = 1 and AllAuthorsLeven > 30) OR
	(JournalJaro >= 50 and VolumeMatch = 1 and PageMatch = 1 AND FirstAuthorJaro >= 90))

/*
insert into @MatchedStepVolFirstPage(RecordId, PaperId, JournalJaro, VolumeMatch, PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
select distinct
	t.ITEM_ID
,	p.PaperID
,	iif(t.PARENT_TITLE is not null and j.NormalizedName is not null, dbo.Jaro(t.PARENT_TITLE, j.NormalizedName), -1) as JournalJaro
,	1 as VolumeMatch
,	1 as PageMatch
,	iif(NOT t.YEAR is null AND t.YEAR = cast (p.YEAR as nvarchar(50)), 1, 0) YearMatch
,	dbo.LevenshteinSVF(dbo.ToShortSearchText(Reviewer.dbo.fn_REBUILD_AUTHORS(t.ITEM_ID, 0)), 
		dbo.ToShortSearchText(dbo.fn_PaperAuthors(p.PaperID)) ) AllAuthorsLeven
,	dbo.Jaro(ItemAuthor.LAST, dbo.fn_PaperFirstAuthor(p.PaperID)) FirstAuthorJaro
,	dbo.LevenshteinSVF(p.SearchText, t.SearchText) TitleLeven
from Reviewer.dbo.TB_ITEM t
inner join Reviewer.dbo.TB_ITEM_REVIEW tir on tir.ITEM_ID = t.ITEM_ID and tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
inner join Papers p on p.Volume = t.VOLUME and substring(t.PAGES, 0, CHARINDEX('-', t.PAGES, 0)) = Cast(p.FirstPage as nvarchar(50))
left join Reviewer.dbo.TB_ITEM_AUTHOR ItemAuthor on ItemAuthor.ITEM_ID = t.ITEM_ID and ItemAuthor.ROLE = 0 and ItemAuthor.RANK = 0
left join Journals j on j.JournalId = p.JournalID
	where  t.ITEM_ID = @ITEM_ID and( not t.TITLE is null and not t.VOLUME is null and not t.VOLUME = ''
	and not p.Volume is null and not p.Volume = ''
	and not t.PAGES is null and not t.PAGES = ''
	and not p.FirstPage is null and not p.FirstPage = ''
	and not t.ITEM_ID in (select recordid from @MatchedStepSearchText)
	and not t.ITEM_ID in(select RecordID from @MatchedStepDoi))

update @MatchedStepVolFirstPage
	set AutoMatchScore = 1 where
	(JournalJaro + AllAuthorsLeven + FirstAuthorJaro + (TitleLeven / 100)) / 4 >= 0.50 and TitleLeven >= 85
*/

insert into @Results
	select * from @MatchedStepSearchText order by AutoMatchScore

delete from doi
	from @MatchedStepDoi doi
		inner join @Results r on r.PaperId = doi.PaperId and r.RecordId = doi.RecordId and r.AutoMatchScore = doi.AutoMatchScore

insert into @Results(RecordId, PaperId, JournalJaro, VolumeMatch, PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
	select doi.RecordId, doi.PaperId, doi.JournalJaro, doi.VolumeMatch, doi.PageMatch, YearMatch, doi.AllAuthorsLeven,
		doi.FirstAuthorJaro, TitleLeven
		from @MatchedStepDoi doi 
		order by AutoMatchScore

delete from volf
	from @MatchedStepVolFirstPage volf
	inner join @Results r on r.PaperId = volf.PaperId and r.RecordId = volf.RecordId and r.AutoMatchScore = volf.AutoMatchScore

insert into @Results(RecordId, PaperId, JournalJaro, VolumeMatch, PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
	select VolFirst.RecordId, VolFirst.PaperId, VolFirst.JournalJaro, VolFirst.VolumeMatch, VolFirst.PageMatch, YearMatch, VolFirst.AllAuthorsLeven,
		VolFirst.FirstAuthorJaro, TitleLeven
		from @MatchedStepVolFirstPage VolFirst 
		order by AutoMatchScore


-- ********** SANITY CHECK: should only be 1 automatch for any combination of record and paper ID where automatchscore = 1 *************

--select count(*) from @Results r
--	inner join @Results r2 on r2.PaperId = r.RecordId and r2.RecordId = r.PaperId

-- ********** THE ABOVE COUNT MUST BE ZERO. Need to address if it's not. **********************


-- clear out some of the noise

DELETE FROM @Results
	WHERE not
	(
		(TitleLeven > 90 and AllAuthorsLeven > 0.9)
		or (TitleLeven > 90 and PageMatch = 1)
		or (TitleLeven > 90 and JournalJaro > 0.9)
		or (AllAuthorsLeven > 0.9 and JournalJaro > 0.9)
		or (AllAuthorsLeven > 0.9 and PageMatch = 1)
		or (JournalJaro > 0.9 and PageMatch = 1)
	) 

delete from @Results
	where not TitleLeven >= 40
--delete from @Results
--	where not AllAuthorsJaro >= 0.75

-- Set AutoMatchScore for everything that's not automatch = 1
update @Results
	set AutoMatchScore = ((TitleLeven / 100 * 2.71) + (VolumeMatch * 0.02) + (PageMatch * 0.18) + 
		(YearMatch * 0.82) + (JournalJaro * 0.55) + (AllAuthorsLeven /100 * 1.25)) / 5.53
	where AutoMatchScore is null

insert into Reviewer.dbo.tb_ITEM_MAG_MATCH(ITEM_ID, REVIEW_ID, PaperId, AutoMatchScore, ManualFalseMatch, ManualTrueMatch)
select RecordId, @REVIEW_ID, PaperId, AutoMatchScore, 'false', 'false' from @Results

set @RESULT =  @@ROWCOUNT

END
GO
/****** Object:  StoredProcedure [dbo].[st_MatchItemsToPapersWithAttribute]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Match EPPI-Reviewer TB_ITEM records to MAG Papers
-- =============================================
CREATE   PROCEDURE [dbo].[st_MatchItemsToPapersWithAttribute] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT = 0
,	@ATTRIBUTE_ID BIGINT = 0
,	@CONTACT_ID INT
,	@RESULT nvarchar(50) = '' OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Declare @LAST_RUN NVARCHAR(50)
	Declare @START_TIME DATETIME
	Declare @NewReviewJob int
	
	select TOP(1) @LAST_RUN = CURRENT_STATE, @START_TIME = START_TIME
		FROM Reviewer.dbo.TB_REVIEW_JOB
		WHERE REVIEW_ID = @REVIEW_ID AND JOB_TYPE = 'MAG Item Matches'
		ORDER BY REVIEW_JOB_ID DESC

	IF @LAST_RUN = 'Running' AND GETDATE() < DATEADD(HOUR, 3, @START_TIME)
	BEGIN --send back a return code to signify that the matching process is still running
		set @RESULT = 'There is currently a matching process running on this review. Please check back later.'
		return -2
	END
	ELSE
	IF @LAST_RUN = 'Error' OR  (GETDATE() > DATEADD(HOUR, 3, @START_TIME) and @LAST_RUN = 'Running')
	BEGIN
		set @RESULT = 'There was an error when running the last matching process. Please contact support.'
		return -3 -- Error when running the matching
	END

	Insert into Reviewer.dbo.tb_REVIEW_JOB(REVIEW_ID, CONTACT_ID, START_TIME, JOB_TYPE, CURRENT_STATE, SUCCESS)
	values (@REVIEW_ID, @CONTACT_ID, GETDATE(), 'MAG Item Matches', 'Running', 0)
	set @NewReviewJob = @@IDENTITY

UPDATE i
	set SearchText = dbo.ToShortSearchText(TITLE)
from Reviewer.dbo.TB_ITEM i
	inner join Reviewer.dbo.TB_ITEM_REVIEW tir on tir.ITEM_ID = i.ITEM_ID
	and tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
	inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = i.ITEM_ID and tia.ATTRIBUTE_ID = @ATTRIBUTE_ID
	inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_ATTRIBUTE_ID and tis.IS_COMPLETED = 'true'
	and (i.SearchText = '' or i.SearchText is null)

declare @MatchedStepDoi Table
(
	RecordId bigint
,	PaperId bigint
,	JournalJaro float
,	VolumeMatch int
,	PageMatch int
,	YearMatch int
,	AllAuthorsLeven float
,	FirstAuthorJaro float
,	TitleLeven float
,	AutoMatchScore float
,	INDEX IX_RecordId NONCLUSTERED (RecordId)
,	INDEX IX_PaperId NONCLUSTERED (PaperId)
)
declare @MatchedStepSearchText Table
(
	RecordId bigint
,	PaperId bigint
,	JournalJaro float
,	VolumeMatch int
,	PageMatch int
,	YearMatch int
,	AllAuthorsLeven float
,	FirstAuthorJaro float
,	TitleLeven float
,	AutoMatchScore float
,	INDEX IX_RecordId NONCLUSTERED (RecordId)
,	INDEX IX_PaperId NONCLUSTERED (PaperId)
)
declare @MatchedStepVolFirstPage Table
(
	RecordId bigint
,	PaperId bigint
,	JournalJaro float
,	VolumeMatch int
,	PageMatch int
,	YearMatch int
,	AllAuthorsLeven float
,	FirstAuthorJaro float
,	TitleLeven float
,	AutoMatchScore float
,	INDEX IX_RecordId NONCLUSTERED (RecordId)
,	INDEX IX_PaperId NONCLUSTERED (PaperId)
)
declare @Results Table
(
	RecordId bigint
,	PaperId bigint
,	JournalJaro float
,	VolumeMatch int
,	PageMatch int
,	YearMatch int
,	AllAuthorsLeven float
,	FirstAuthorJaro float
,	TitleLeven float
,	AutoMatchScore float
,	INDEX IX_RecordId NONCLUSTERED (RecordId)
,	INDEX IX_PaperId NONCLUSTERED (PaperId)
)
declare @ItemInfo Table
(
	ItemId bigint
,	Info int
)

insert into @MatchedStepDoi(RecordId, PaperId, JournalJaro, VolumeMatch, PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
select distinct
	t.ITEM_ID
,	p.PaperID
,	iif(t.PARENT_TITLE is not null and j.NormalizedName is not null, dbo.Jaro(t.PARENT_TITLE, j.NormalizedName), -1) as JournalJaro
,	iif(t.VOLUME = p.VOLUME and not t.VOLUME is null, 1, 0) VolMatch
,	iif(iif (CHARINDEX('-', t.PAGES, 0) > 0, substring(t.PAGES, 0, CHARINDEX('-', t.PAGES, 0))  , t.PAGES) = p.FirstPage, 1, 0) PageMatch
,	iif(NOT t.YEAR is null AND t.YEAR = p.YEAR, 1, 0) YearMatch
,	dbo.LevenshteinSVF(dbo.ToShortSearchText(Reviewer.dbo.fn_REBUILD_AUTHORS(t.ITEM_ID, 0)), 
		dbo.ToShortSearchText(dbo.fn_PaperAuthors(p.PaperID)) ) AllAuthorsLeven
,	dbo.Jaro(ItemAuthor.LAST, dbo.fn_PaperFirstAuthor(p.PaperID)) FirstAuthorJaro
,	dbo.LevenshteinSVF(p.SearchText, t.SearchText) TitleLeven
from Reviewer.dbo.TB_ITEM t
inner join Reviewer.dbo.TB_ITEM_REVIEW tir on tir.ITEM_ID = t.ITEM_ID and tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = t.ITEM_ID and tia.ATTRIBUTE_ID = @ATTRIBUTE_ID
inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_ATTRIBUTE_ID and tis.IS_COMPLETED = 'true'
inner join Papers p on p.DOI = t.DOI
left join Reviewer.dbo.TB_ITEM_AUTHOR ItemAuthor on ItemAuthor.ITEM_ID = t.ITEM_ID and ItemAuthor.ROLE = 0 and ItemAuthor.RANK = 0
left join Journals j on j.JournalId = p.JournalID
	where not t.TITLE is null AND LEN(T.DOI) > 5

update @MatchedStepDoi
set AutoMatchScore = 1 where
	((JournalJaro >= 90 AND VolumeMatch = 1 AND PageMatch = 1) OR
	(JournalJaro < 90 AND VolumeMatch = 1 AND PageMatch = 1 and AllAuthorsLeven > 30) OR
	(JournalJaro >= 50 and VolumeMatch = 1 and PageMatch = 1 AND FirstAuthorJaro >= 90))

insert into @MatchedStepSearchText(RecordId, PaperId, JournalJaro, VolumeMatch,	PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
select distinct
	t.ITEM_ID
,	p.PaperID
,	iif(t.PARENT_TITLE is not null and j.NormalizedName is not null, dbo.Jaro(t.PARENT_TITLE, j.NormalizedName), -1) as JournalJaro
,	iif(t.VOLUME = p.VOLUME and not t.VOLUME is null, 1, 0) VolMatch
,	iif(iif (CHARINDEX('-', t.PAGES, 0) > 0, substring(t.PAGES, 0, CHARINDEX('-', t.PAGES, 0))  , t.PAGES) = p.FirstPage, 1, 0) PageMatch
,	iif(NOT t.YEAR is null AND t.YEAR = p.YEAR, 1, 0) YearMatch
,	dbo.LevenshteinSVF(dbo.ToShortSearchText(Reviewer.dbo.fn_REBUILD_AUTHORS(t.ITEM_ID, 0)),
		dbo.ToShortSearchText(dbo.fn_PaperAuthors(p.PaperID)) ) AllAuthorsLeven
,	dbo.Jaro(ItemAuthor.LAST, dbo.fn_PaperFirstAuthor(p.PaperID)) FirstAuthorJaro
,	dbo.LevenshteinSVF(p.SearchText, t.SearchText) TitleLeven
from Reviewer.dbo.TB_ITEM t
inner join Reviewer.dbo.TB_ITEM_REVIEW tir on tir.ITEM_ID = t.ITEM_ID and tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = t.ITEM_ID and tia.ATTRIBUTE_ID = @ATTRIBUTE_ID
inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_ATTRIBUTE_ID and tis.IS_COMPLETED = 'true'
inner join Papers p on p.SearchText = t.SearchText
left join Reviewer.dbo.TB_ITEM_AUTHOR ItemAuthor on ItemAuthor.ITEM_ID = t.ITEM_ID and ItemAuthor.ROLE = 0 and ItemAuthor.RANK = 0
left join Journals j on j.JournalId = p.JournalID
	where not t.TITLE is null AND LEN(T.DOI) > 5

update @MatchedStepSearchText
set AutoMatchScore = 1 where
	((JournalJaro >= 90 AND VolumeMatch = 1 AND PageMatch = 1) OR
	(JournalJaro < 90 AND VolumeMatch = 1 AND PageMatch = 1 and AllAuthorsLeven > 30) OR
	(JournalJaro >= 50 and VolumeMatch = 1 and PageMatch = 1 AND FirstAuthorJaro >= 90))

insert into @MatchedStepVolFirstPage(RecordId, PaperId, JournalJaro, VolumeMatch, PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
select distinct
	t.ITEM_ID
,	p.PaperID
,	iif(t.PARENT_TITLE is not null and j.NormalizedName is not null, dbo.Jaro(t.PARENT_TITLE, j.NormalizedName), -1) as JournalJaro
,	1 as VolumeMatch
,	1 as PageMatch
,	iif(NOT t.YEAR is null AND t.YEAR = cast (p.YEAR as nvarchar(50)), 1, 0) YearMatch
,	dbo.LevenshteinSVF(dbo.ToShortSearchText(Reviewer.dbo.fn_REBUILD_AUTHORS(t.ITEM_ID, 0)), 
		dbo.ToShortSearchText(dbo.fn_PaperAuthors(p.PaperID)) ) AllAuthorsLeven
,	dbo.Jaro(ItemAuthor.LAST, dbo.fn_PaperFirstAuthor(p.PaperID)) FirstAuthorJaro
,	dbo.LevenshteinSVF(p.SearchText, t.SearchText) TitleLeven
from Reviewer.dbo.TB_ITEM t
inner join Reviewer.dbo.TB_ITEM_REVIEW tir on tir.ITEM_ID = t.ITEM_ID and tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = t.ITEM_ID and tia.ATTRIBUTE_ID = @ATTRIBUTE_ID
inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_ATTRIBUTE_ID and tis.IS_COMPLETED = 'true'
inner join Papers p on p.Volume = t.VOLUME and substring(t.PAGES, 0, CHARINDEX('-', t.PAGES, 0)) = Cast(p.FirstPage as nvarchar(50))
left join Reviewer.dbo.TB_ITEM_AUTHOR ItemAuthor on ItemAuthor.ITEM_ID = t.ITEM_ID and ItemAuthor.ROLE = 0 and ItemAuthor.RANK = 0
left join Journals j on j.JournalId = p.JournalID
	where not t.TITLE is null and not t.VOLUME is null and not t.VOLUME = ''
	and not p.Volume is null and not p.Volume = ''
	and not t.PAGES is null and not t.PAGES = ''
	and not p.FirstPage is null and not p.FirstPage = ''
	and not t.ITEM_ID in (select recordid from @MatchedStepSearchText)
	and not t.ITEM_ID in(select RecordID from @MatchedStepDoi)

update @MatchedStepVolFirstPage
	set AutoMatchScore = 1 where
	(JournalJaro + AllAuthorsLeven + FirstAuthorJaro + (TitleLeven / 100)) / 4 >= 0.50 and TitleLeven >= 85

insert into @Results
	select * from @MatchedStepSearchText order by AutoMatchScore

delete from doi
	from @MatchedStepDoi doi
		inner join @Results r on r.PaperId = doi.PaperId and r.RecordId = doi.RecordId and r.AutoMatchScore = doi.AutoMatchScore

insert into @Results(RecordId, PaperId, JournalJaro, VolumeMatch, PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
	select doi.RecordId, doi.PaperId, doi.JournalJaro, doi.VolumeMatch, doi.PageMatch, YearMatch, doi.AllAuthorsLeven,
		doi.FirstAuthorJaro, TitleLeven
		from @MatchedStepDoi doi 
		order by AutoMatchScore

delete from volf
	from @MatchedStepVolFirstPage volf
	inner join @Results r on r.PaperId = volf.PaperId and r.RecordId = volf.RecordId and r.AutoMatchScore = volf.AutoMatchScore

insert into @Results(RecordId, PaperId, JournalJaro, VolumeMatch, PageMatch, YearMatch, AllAuthorsLeven, FirstAuthorJaro, TitleLeven)
	select VolFirst.RecordId, VolFirst.PaperId, VolFirst.JournalJaro, VolFirst.VolumeMatch, VolFirst.PageMatch, YearMatch, VolFirst.AllAuthorsLeven,
		VolFirst.FirstAuthorJaro, TitleLeven
		from @MatchedStepVolFirstPage VolFirst 
		order by AutoMatchScore


-- ********** SANITY CHECK: should only be 1 automatch for any combination of record and paper ID where automatchscore = 1 *************

--select count(*) from @Results r
--	inner join @Results r2 on r2.PaperId = r.RecordId and r2.RecordId = r.PaperId

-- ********** THE ABOVE COUNT MUST BE ZERO. Need to address if it's not. **********************


-- clear out some of the noise

DELETE FROM @Results
	WHERE not
	(
		(TitleLeven > 90 and AllAuthorsLeven > 0.9)
		or (TitleLeven > 90 and PageMatch = 1)
		or (TitleLeven > 90 and JournalJaro > 0.9)
		or (AllAuthorsLeven > 0.9 and JournalJaro > 0.9)
		or (AllAuthorsLeven > 0.9 and PageMatch = 1)
		or (JournalJaro > 0.9 and PageMatch = 1)
	) 

delete from @Results
	where not TitleLeven >= 40
--delete from @Results
--	where not AllAuthorsJaro >= 0.75

-- Set AutoMatchScore for everything that's not automatch = 1
update @Results
	set AutoMatchScore = ((TitleLeven / 100 * 2.71) + (VolumeMatch * 0.02) + (PageMatch * 0.18) + 
		(YearMatch * 0.82) + (JournalJaro * 0.55) + (AllAuthorsLeven /100 * 1.25)) / 5.53
	where AutoMatchScore is null

insert into Reviewer.dbo.tb_ITEM_MAG_MATCH(ITEM_ID, REVIEW_ID, PaperId, AutoMatchScore, ManualFalseMatch, ManualTrueMatch)
select RecordId, @REVIEW_ID, PaperId, AutoMatchScore, 'false', 'false' from @Results

set @RESULT = 'Complete'
update reviewer.dbo.tb_REVIEW_JOB
	set END_TIME = GETDATE(),
	CURRENT_STATE = 'Complete',
	SUCCESS = 0
	where REVIEW_JOB_ID = @NewReviewJob

END
GO
/****** Object:  StoredProcedure [dbo].[st_Paper]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns a single paper
-- =============================================
CREATE   PROCEDURE [dbo].[st_Paper] 
	-- Add the parameters for the stored procedure here
	@PaperId bigint = 0
,	@REVIEW_ID int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, paii.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
		from Papers p
	left outer join Journals j on j.JournalId = p.JournalID
	left outer join PaperAbstractsInvertedIndex paii on paii.PaperID = p.PaperID
	left outer join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.PaperId = p.PaperID
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID
	where p.PaperID = @PaperId
END
GO
/****** Object:  StoredProcedure [dbo].[st_PaperCitations]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Grab citation information for a given Paper
-- =============================================
CREATE   PROCEDURE [dbo].[st_PaperCitations] 
	-- Add the parameters for the stored procedure here
	@PaperId bigint = 0
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@REVIEW_ID int = 0
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT @Total = count(*) from Papers p
		inner join PaperReferences pr on pr.PaperReferenceID = p.PaperID
		where pr.PaperID = @PaperId

    -- Insert statements for procedure here
	SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, pai.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
		from Papers p
		inner join PaperReferences pr on pr.PaperReferenceID = p.PaperID
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
		left outer join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.PaperId = p.PaperID
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID
		where pr.PaperID = @PaperId
		order by pr.PaperID
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END
GO
/****** Object:  StoredProcedure [dbo].[st_PaperCitedBy]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Grab citation information for a given Paper
-- =============================================
CREATE   PROCEDURE [dbo].[st_PaperCitedBy] 
	-- Add the parameters for the stored procedure here
	@PaperId bigint = 0
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@REVIEW_ID int = 0
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT @Total = count(*) from Papers p
		inner join PaperReferences pr on pr.PaperID = p.PaperID
		where pr.PaperReferenceID = @PaperId 

	SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, pai.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
		from Papers p
		inner join PaperReferences pr on pr.PaperID = p.PaperID
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
		left outer join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.PaperId = p.PaperID
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID
		where pr.PaperReferenceID = @PaperId
		order by pr.PaperID
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END
GO
/****** Object:  StoredProcedure [dbo].[st_PaperFieldsOfStudy]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Grab fields of study for a given Paper
-- =============================================
CREATE   PROCEDURE [dbo].[st_PaperFieldsOfStudy] 
	-- Add the parameters for the stored procedure here
	@PaperId bigint = 0
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT top(20) * from PaperFieldsOfStudy pfos
		inner join FieldsOfStudy fos on fos.FieldOfStudyId = pfos.FieldOfStudyID
		where pfos.PaperID = @PaperId
		order by pfos.Score desc	
END
GO
/****** Object:  StoredProcedure [dbo].[st_PaperListById]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List of MAG papers from a list of paper IDs
-- =============================================
CREATE   PROCEDURE [dbo].[st_PaperListById] 
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

	SELECT @Total = count(*) from Papers p
		inner join dbo.fn_Split_int(@PaperIds, ',') ids on ids.value = p.PaperID

    -- Insert statements for procedure here
	SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, pai.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
		from Papers p
		inner join dbo.fn_Split_int(@PaperIds, ',') ids on ids.value = p.PaperID
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
		left outer join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.PaperId = p.PaperID
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID
		order by p.PaperID
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END
GO
/****** Object:  StoredProcedure [dbo].[st_PaperRecommendations]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Fetch paper recommentations for a given PaperId
-- =============================================
CREATE   PROCEDURE [dbo].[st_PaperRecommendations] 
	-- Add the parameters for the stored procedure here
	@PaperId bigint = 0
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@REVIEW_ID int = 0
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- NB There are only 20 recommendations, so no need for paging here

    -- Insert statements for procedure here
	SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, pai.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
		from Papers p
		inner join PaperRecommendations pr on pr.RecommendedPaperID = p.PaperID
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
		left outer join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.PaperId = p.PaperID
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID
		where pr.PaperID = @PaperId
END
GO
/****** Object:  StoredProcedure [dbo].[st_PaperRecommendedBy]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Fetch paper recommentations for a given PaperId
-- =============================================
CREATE   PROCEDURE [dbo].[st_PaperRecommendedBy] 
	-- Add the parameters for the stored procedure here
	@PaperId bigint = 0
,	@PageNo int = 1
,	@RowsPerPage int = 10
,	@REVIEW_ID int = 0
,	@Total int = 0  OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT @Total = count(*) from Papers p
		inner join PaperRecommendations pr on pr.PaperID = p.PaperID
		inner join Journals j on j.JournalId = p.JournalID
		where pr.RecommendedPaperID = @PaperId

    -- Insert statements for procedure here

	SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, pai.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
		from Papers p
		inner join PaperRecommendations pr on pr.PaperID = p.PaperID
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperId = p.PaperId
		left outer join Reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.PaperId = p.PaperID
			and imm.ManualFalseMatch <> 'True' and imm.REVIEW_ID = @REVIEW_ID
		where pr.RecommendedPaperID = @PaperId
		order by pr.Score desc
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY

	SELECT  @Total as N'@Total'
END
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewMagInfo]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns statistics about matching review items to MAG
-- =============================================
CREATE   PROCEDURE [dbo].[st_ReviewMagInfo] 
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

	select @NInReviewIncluded = count(*) from reviewer.dbo.TB_ITEM_REVIEW ir
		where ir.IS_DELETED = 'FALSE' and ir.IS_INCLUDED = 'TRUE' and REVIEW_ID = @REVIEW_ID

	select @NInReviewExcluded = count(*) from reviewer.dbo.TB_ITEM_REVIEW ir
		where ir.IS_DELETED = 'false' and ir.IS_INCLUDED = 'false' and REVIEW_ID = @REVIEW_ID

	select @NMatchedAccuratelyIncluded = count(distinct imm.ITEM_ID) from reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Reviewer.dbo.tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'true'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and not imm.ManualFalseMatch = 'true'
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NMatchedAccuratelyExcluded = count(distinct imm.ITEM_ID) from reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Reviewer.dbo.tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'false'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and not imm.ManualFalseMatch = 'true'
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NRequiringManualCheckIncluded = count(distinct imm.ITEM_ID) from reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Reviewer.dbo.tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'true'
		where imm.AutoMatchScore < 0.7 and (imm.ManualFalseMatch = 'false' and imm.ManualTrueMatch = 'false')
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NRequiringManualCheckExcluded = count(distinct imm.ITEM_ID) from reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Reviewer.dbo.tb_item_review ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' 
			and ir.IS_INCLUDED = 'false'
		where imm.AutoMatchScore < 0.7 and (imm.ManualFalseMatch = 'false' and imm.ManualTrueMatch = 'false')
			 and ir.REVIEW_ID = @REVIEW_ID

	select @NNotMatchedIncluded = count(distinct ir.ITEM_ID) from reviewer.dbo.TB_ITEM_REVIEW ir
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = ir.ITEM_ID
			where ir.IS_INCLUDED = 'true' and imm.ITEM_ID is null and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'

	select @NNotMatchedExcluded = count(distinct ir.ITEM_ID) from reviewer.dbo.TB_ITEM_REVIEW ir
		left outer join reviewer.dbo.tb_ITEM_MAG_MATCH imm on imm.ITEM_ID = ir.ITEM_ID
			where ir.IS_INCLUDED = 'false' and imm.ITEM_ID is null and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false'
	
END
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewMatchedPapers]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns all the papers that are matched to a given tb_ITEM record
-- =============================================
CREATE   PROCEDURE [dbo].[st_ReviewMatchedPapers] 
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
		SELECT @Total = count(distinct imm.paperid) from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Papers p on p.PaperID = imm.PaperId
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'

		-- Insert statements for procedure here
		SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, pai.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
		from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Papers p on p.PaperID = imm.PaperId
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	end
	else if @INCLUDED = 'excluded'
	begin
		SELECT @Total = count(distinct imm.paperid) from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Papers p on p.PaperID = imm.PaperId
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
		where ir.IS_INCLUDED = 'false' and ir.IS_DELETED = 'false'

		-- Insert statements for procedure here
		SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, pai.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
		from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Papers p on p.PaperID = imm.PaperId
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
		where ir.IS_INCLUDED = 'false' and ir.IS_DELETED = 'false'
		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	end
	else
	begin -- i.e. included = included AND excluded
		SELECT @Total = count(distinct imm.paperid) from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Papers p on p.PaperID = imm.PaperId
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
		where ir.IS_DELETED = 'false'

		-- Insert statements for procedure here
		SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, pai.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
		from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Papers p on p.PaperID = imm.PaperId
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
		where ir.IS_DELETED = 'false'
		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	end

	SELECT  @Total as N'@Total'

END
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewMatchedPapersWithThisCode]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Returns all the papers that are matched to a given tb_ITEM record
-- =============================================
CREATE   PROCEDURE [dbo].[st_ReviewMatchedPapersWithThisCode] 
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

		SELECT @Total = count(distinct imm.paperid) from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Papers p on p.PaperID = imm.PaperId
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		
		inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = ir.ITEM_ID
		inner join dbo.fn_Split_int(@ATTRIBUTE_IDs, ',') ids on ids.value = tia.ATTRIBUTE_ID
		inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_ATTRIBUTE_ID and tis.IS_COMPLETED = 'true'

		-- Insert statements for procedure here
		SELECT p.PaperId, p.DOI, p.DocType, p.PaperTitle, p.OriginalTitle, p.BookTitle, p.Year, p.Date, p.Publisher, p.JournalId,
		j.DisplayName, p.ConferenceSeriesId, p.ConferenceInstanceId, p.Volume, p.FirstPage, p.LastPage, p.ReferenceCount,
		p.CitationCount, p.EstimatedCitationCount, p.CreatedDate, dbo.fn_PaperAuthors(p.PaperId) authors,
		imm.ITEM_ID, pai.IndexedAbstract, dbo.fn_PaperURLs(p.PaperID) URLs, imm.AutoMatchScore,
		imm.ManualFalseMatch, imm.ManualTrueMatch
		
		from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Papers p on p.PaperID = imm.PaperId
		left outer join Journals j on j.JournalId = p.JournalID
		left outer join PaperAbstractsInvertedIndex pai on pai.PaperID = p.PaperID
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
			and ir.IS_INCLUDED = 'true' and ir.IS_DELETED = 'false'
		inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = ir.ITEM_ID
		inner join dbo.fn_Split_int(@ATTRIBUTE_IDs, ',') ids on ids.value = tia.ATTRIBUTE_ID
		inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_ATTRIBUTE_ID and tis.IS_COMPLETED = 'true'
		ORDER BY imm.PaperId
		OFFSET (@PageNo-1) * @RowsPerPage ROWS
		FETCH NEXT @RowsPerPage ROWS ONLY
	

	SELECT  @Total as N'@Total'

END
GO
/****** Object:  StoredProcedure [dbo].[st_Simulation]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Simulation studies to give people an idea about how well MAG graph search works on their data
-- =============================================
CREATE   PROCEDURE [dbo].[st_Simulation] 
	-- Add the parameters for the stored procedure here
	@review_id int = 0
,	@DateFrom DATETIME
,	@CreatedDate datetime
,	@AttributeId bigint

,	@recommended INT OUTPUT
,	@reverse_recommended INT OUTPUT
,	@birecommended INT OUTPUT
,	@bibliography INT OUTPUT
,	@citations INT OUTPUT
,	@bicitations INT OUTPUT
,	@total_recommended INT OUTPUT
,	@total_reverse_recommended INT OUTPUT
,	@total_birecommended INT OUTPUT
,	@total_citations INT OUTPUT
,	@total_bicitations INT OUTPUT
,	@total_bibliography INT OUTPUT
,	@both INT OUTPUT
,	@total_both INT OUTPUT
,	@N_Seeds INT OUTPUT
,	@N_Seeking INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @SeedIds table
	(
		PaperId bigint INDEX idx CLUSTERED
	)
	declare @SeekingIds table
	(
		PaperId bigint INDEX idx CLUSTERED
	)
	declare @SeedIds2 table
	(
		PaperId bigint INDEX idx CLUSTERED
	)
	declare @SeekingIds2 table
	(
		PaperId bigint INDEX idx CLUSTERED
	)
	declare @year int = year(@datefrom)

-- seed set based on publication date
if @year <> 1753
begin
	insert into @SeedIds
	select distinct imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
	inner join Papers p on p.PaperID = imm.PaperId
	inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.IS_INCLUDED = 'true'
	where imm.REVIEW_ID = @review_id
		and imm.AutoMatchScore > 0.2
		and imm.ManualFalseMatch <> 'true'
		and p.Year < @year

	select @total_recommended = count(distinct pr.RecommendedPaperID) from PaperRecommendations pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
		inner join Papers p on p.PaperID = pr.RecommendedPaperID
		where p.Year >= @year

	select @total_reverse_recommended = count(distinct pr.PaperID) from PaperRecommendations pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.RecommendedPaperID
		inner join Papers p on p.PaperID = pr.PaperID
		where p.Year >= @year

	;with cte (PaperId) as
	(
		select distinct pr.RecommendedPaperID from PaperRecommendations pr
			inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
			inner join Papers p on p.PaperID = pr.RecommendedPaperID
		where p.Year >= @year
		union all
		select distinct pr.PaperID from PaperRecommendations pr
			inner join @SeedIds seed1 on seed1.PaperId = pr.RecommendedPaperID
			inner join Papers p on p.PaperID = pr.PaperID
			where p.Year >= @year
	) select @total_birecommended = count(distinct PaperId) from cte

	select @total_bibliography = count(distinct pr.PaperReferenceID) from PaperReferences pr
	inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
	inner join Papers p on p.PaperID = pr.PaperReferenceID
			where p.Year >= @year

	select @total_citations = count(distinct pr.PaperID) from PaperReferences pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperReferenceID
		inner join Papers p on p.PaperID = pr.PaperID
			where p.Year >= @year

	;with cte (PaperId) as
	(
		select distinct pr.PaperReferenceID from PaperReferences pr
			inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
			inner join Papers p on p.PaperID = pr.PaperReferenceID
			where p.Year >= @year
		union all
		select distinct pr.PaperID from PaperReferences pr
			inner join @SeedIds seed1 on seed1.PaperId = pr.PaperReferenceID
			inner join Papers p on p.PaperID = pr.PaperID
			where p.Year >= @year
	) select @total_bicitations = count(distinct PaperId) from cte

	;with cte (PaperId) as
	(
	select distinct pr.RecommendedPaperID from PaperRecommendations pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
		inner join Papers p on p.PaperID = pr.RecommendedPaperID
			where p.Year >= @year
	union all
	select distinct pr.PaperID from PaperRecommendations pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.RecommendedPaperID
		inner join Papers p on p.PaperID = pr.PaperID
			where p.Year >= @year
	union all
	select distinct pr.PaperReferenceID from PaperReferences pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
		inner join Papers p on p.PaperID = pr.PaperReferenceID
			where p.Year >= @year
	union all
	select distinct pr.PaperID from PaperReferences pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperReferenceID
		inner join Papers p on p.PaperID = pr.PaperID
			where p.Year >= @year
	) select @total_both = count(distinct PaperId) from cte
end

-- seed set based on academic creation date
if year(@CreatedDate) <> 1753
begin
	insert into @SeedIds
	select distinct imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
	inner join Papers p on p.PaperID = imm.PaperId
	inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.IS_INCLUDED = 'true'
	where imm.REVIEW_ID = @review_id
		and imm.AutoMatchScore > 0.2
		and imm.ManualFalseMatch <> 'true'
		and p.CreatedDate < @CreatedDate

	select @total_recommended = count(distinct pr.RecommendedPaperID) from PaperRecommendations pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
		inner join Papers p on p.PaperID = pr.RecommendedPaperID
		where p.CreatedDate >= @CreatedDate

	select @total_reverse_recommended = count(distinct pr.PaperID) from PaperRecommendations pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.RecommendedPaperID
		inner join Papers p on p.PaperID = pr.PaperID
		where p.CreatedDate >= @CreatedDate

	;with cte (PaperId) as
	(
		select distinct pr.RecommendedPaperID from PaperRecommendations pr
			inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
			inner join Papers p on p.PaperID = pr.RecommendedPaperID
		where p.CreatedDate >= @CreatedDate
		union all
		select distinct pr.PaperID from PaperRecommendations pr
			inner join @SeedIds seed1 on seed1.PaperId = pr.RecommendedPaperID
			inner join Papers p on p.PaperID = pr.PaperID
			where p.CreatedDate >= @CreatedDate
	) select @total_birecommended = count(distinct PaperId) from cte

	select @total_bibliography = count(distinct pr.PaperReferenceID) from PaperReferences pr
	inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
	inner join Papers p on p.PaperID = pr.PaperReferenceID
		where p.CreatedDate >= @CreatedDate

	select @total_citations = count(distinct pr.PaperID) from PaperReferences pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperReferenceID
		inner join Papers p on p.PaperID = pr.PaperID
		where p.CreatedDate >= @CreatedDate

	;with cte (PaperId) as
	(
		select distinct pr.PaperReferenceID from PaperReferences pr
			inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
			inner join Papers p on p.PaperID = pr.PaperReferenceID
		where p.CreatedDate >= @CreatedDate
		union all
		select distinct pr.PaperID from PaperReferences pr
			inner join @SeedIds seed1 on seed1.PaperId = pr.PaperReferenceID
			inner join Papers p on p.PaperID = pr.PaperID
		where p.CreatedDate >= @CreatedDate
	) select @total_bicitations = count(distinct PaperId) from cte

	;with cte (PaperId) as
	(
	select distinct pr.RecommendedPaperID from PaperRecommendations pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
		inner join Papers p on p.PaperID = pr.RecommendedPaperID
		where p.CreatedDate >= @CreatedDate
	union all
	select distinct pr.PaperID from PaperRecommendations pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.RecommendedPaperID
		inner join Papers p on p.PaperID = pr.PaperID
		where p.CreatedDate >= @CreatedDate
	union all
	select distinct pr.PaperReferenceID from PaperReferences pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
		inner join Papers p on p.PaperID = pr.PaperReferenceID
		where p.CreatedDate >= @CreatedDate
	union all
	select distinct pr.PaperID from PaperReferences pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperReferenceID
		inner join Papers p on p.PaperID = pr.PaperID
		where p.CreatedDate >= @CreatedDate
	) select @total_both = count(distinct PaperId) from cte
end

--seed set based on items with specific code
if @AttributeId > 0
begin
	insert into @SeedIds
	select distinct imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
	inner join Papers p on p.PaperID = imm.PaperId
	inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.IS_INCLUDED = 'true'
	inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = ir.ITEM_ID
	inner join Reviewer.dbo.TB_ITEM_SET iset on iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
	where imm.REVIEW_ID = @review_id
		and imm.AutoMatchScore > 0.2
		and imm.ManualFalseMatch <> 'true'
		and ia.ATTRIBUTE_ID = @AttributeId

	select @total_recommended = count(distinct pr.RecommendedPaperID) from PaperRecommendations pr
	inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID

	select @total_reverse_recommended = count(distinct pr.PaperID) from PaperRecommendations pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.RecommendedPaperID

	;with cte (PaperId) as
	(
		select distinct pr.RecommendedPaperID from PaperRecommendations pr
			inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
		union all
		select distinct pr.PaperID from PaperRecommendations pr
			inner join @SeedIds seed1 on seed1.PaperId = pr.RecommendedPaperID
	) select @total_birecommended = count(distinct PaperId) from cte

	select @total_bibliography = count(distinct pr.PaperReferenceID) from PaperReferences pr
	inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID

	select @total_citations = count(distinct pr.PaperID) from PaperReferences pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperReferenceID

	;with cte (PaperId) as
	(
		select distinct pr.PaperReferenceID from PaperReferences pr
			inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
		union all
		select distinct pr.PaperID from PaperReferences pr
			inner join @SeedIds seed1 on seed1.PaperId = pr.PaperReferenceID
	) select @total_bicitations = count(distinct PaperId) from cte

	;with cte (PaperId) as
	(
	select distinct pr.RecommendedPaperID from PaperRecommendations pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
	union all
	select distinct pr.PaperID from PaperRecommendations pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.RecommendedPaperID
	union all
	select distinct pr.PaperReferenceID from PaperReferences pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
	union all
	select distinct pr.PaperID from PaperReferences pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperReferenceID
	) select @total_both = count(distinct PaperId) from cte
end

-- We're looking for everything else (the seeking ids)
insert into @SeekingIds
	select distinct imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
	inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.IS_INCLUDED = 'true'
	where imm.REVIEW_ID = @review_id
		and imm.AutoMatchScore > 0.2
		and imm.ManualFalseMatch <> 'true'
	except select s.PaperId from @SeedIds s

-- recommended table
delete from @SeedIds2
delete from @SeekingIds2
insert into @SeedIds2(PaperId) select paperid from @SeedIds
insert into @SeekingIds2(PaperId) select paperid from @SeekingIds
	while @@ROWCOUNT > 0
	begin
	insert into @SeedIds2
	select distinct pr.RecommendedPaperID from PaperRecommendations pr
		inner join @SeedIds2 seed2 on seed2.PaperId = pr.PaperID
		inner join @SeekingIds seek on seek.PaperId = pr.RecommendedPaperID
	except select paperid from @SeedIds2 
	end
select @recommended = count(seek.PaperId) from @SeekingIds2 seek
	inner join @SeedIds2 seed on seed.PaperId = seek.PaperId

delete from @SeedIds2
delete from @SeekingIds2
insert into @SeedIds2(PaperId) select paperid from @SeedIds
insert into @SeekingIds2(PaperId) select paperid from @SeekingIds
	while @@ROWCOUNT > 0
	begin
	insert into @SeedIds2
	select distinct pr.PaperID from PaperRecommendations pr
		inner join @SeedIds2 seed2 on seed2.PaperId = pr.RecommendedPaperID
		inner join @SeekingIds seek on seek.PaperId = pr.PaperID
	except select paperid from @SeedIds2 
	end
select @reverse_recommended = count(seek.PaperId) from @SeekingIds2 seek
	inner join @SeedIds2 seed on seed.PaperId = seek.PaperId

delete from @SeedIds2
delete from @SeekingIds2
insert into @SeedIds2(PaperId) select paperid from @SeedIds
insert into @SeekingIds2(PaperId) select paperid from @SeekingIds
while @@ROWCOUNT > 0
begin
;with cte (PaperId) as
(
	select distinct pr.RecommendedPaperID from PaperRecommendations pr
		inner join @SeedIds2 seed on seed.PaperId = pr.PaperID
		inner join @SeekingIds2 seek on seek.PaperId = pr.RecommendedPaperID
	union all
	select distinct pr.PaperID from PaperRecommendations pr
		inner join @SeedIds seed on seed.PaperId = pr.RecommendedPaperID
		inner join @SeekingIds seek on seek.PaperId = pr.PaperID
) insert into @SeedIds2
	select paperid from cte
	except select paperid from @SeedIds2
end
select @birecommended = count(seek.PaperId) from @SeekingIds2 seek
	inner join @SeedIds2 seed on seed.PaperId = seek.PaperId


-- references table
delete from @SeedIds2
delete from @SeekingIds2
insert into @SeedIds2(PaperId) select paperid from @SeedIds
insert into @SeekingIds2(PaperId) select paperid from @SeekingIds
while @@ROWCOUNT > 0
	begin
	insert into @SeedIds2
	select distinct pr.PaperReferenceID from PaperReferences pr
		inner join @SeedIds2 seed2 on seed2.PaperId = pr.PaperID
		inner join @SeekingIds seek on seek.PaperId = pr.PaperReferenceID
	except select paperid from @SeedIds2 
	end
select @bibliography = count(seek.PaperId) from @SeekingIds2 seek
	inner join @SeedIds2 seed on seed.PaperId = seek.PaperId

	/*
select @bibliography = count(distinct pr.PaperReferenceID) from PaperReferences pr
	inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
	inner join @SeekingIds seek1 on seek1.PaperId = pr.PaperReferenceID
	*/

delete from @SeedIds2
delete from @SeekingIds2
insert into @SeedIds2(PaperId) select paperid from @SeedIds
insert into @SeekingIds2(PaperId) select paperid from @SeekingIds
while @@ROWCOUNT > 0
	begin
	insert into @SeedIds2
	select distinct pr.PaperID from PaperReferences pr
		inner join @SeedIds2 seed2 on seed2.PaperId = pr.PaperReferenceID
		inner join @SeekingIds seek on seek.PaperId = pr.PaperID
	except select paperid from @SeedIds2 
	end
select @citations = count(seek.PaperId) from @SeekingIds2 seek
	inner join @SeedIds2 seed on seed.PaperId = seek.PaperId
/*
select @citations = count(distinct pr.PaperID) from PaperReferences pr
	inner join @SeedIds seed1 on seed1.PaperId = pr.PaperReferenceID
	inner join @SeekingIds seek1 on seek1.PaperId = pr.PaperID
*/

delete from @SeedIds2
delete from @SeekingIds2
insert into @SeedIds2(PaperId) select paperid from @SeedIds
insert into @SeekingIds2(PaperId) select paperid from @SeekingIds
while @@ROWCOUNT > 0
begin
;with cte (PaperId) as
(
	select distinct pr.PaperReferenceID from PaperReferences pr
		inner join @SeedIds2 seed on seed.PaperId = pr.PaperID
		inner join @SeekingIds2 seek on seek.PaperId = pr.PaperReferenceID
	union all
	select distinct pr.PaperID from PaperReferences pr
		inner join @SeedIds seed on seed.PaperId = pr.PaperReferenceID
		inner join @SeekingIds seek on seek.PaperId = pr.PaperID
) insert into @SeedIds2
	select paperid from cte
	except select paperid from @SeedIds2
end
select @bicitations = count(seek.PaperId) from @SeekingIds2 seek
	inner join @SeedIds2 seed on seed.PaperId = seek.PaperId

/*(
;with cte (PaperId) as
(
	select distinct pr.PaperReferenceID from PaperReferences pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperID
		inner join @SeekingIds seek1 on seek1.PaperId = pr.PaperReferenceID
	union all
	select distinct pr.PaperID from PaperReferences pr
		inner join @SeedIds seed1 on seed1.PaperId = pr.PaperReferenceID
		inner join @SeekingIds seek1 on seek1.PaperId = pr.PaperID
) select @bicitations = count(distinct PaperId) from cte
*/

-- both
delete from @SeedIds2
delete from @SeekingIds2
insert into @SeedIds2(PaperId) select paperid from @SeedIds
insert into @SeekingIds2(PaperId) select paperid from @SeekingIds
while @@ROWCOUNT > 0
begin
	;with cte (PaperId) as
	(
	select distinct pr.RecommendedPaperID from PaperRecommendations pr
		inner join @SeedIds2 seed on seed.PaperId = pr.PaperID
		inner join @SeekingIds2 seek on seek.PaperId = pr.RecommendedPaperID
	union all
	select distinct pr.PaperID from PaperRecommendations pr
		inner join @SeedIds2 seed on seed.PaperId = pr.RecommendedPaperID
		inner join @SeekingIds2 seek on seek.PaperId = pr.PaperID
	union all
	select distinct pr.PaperReferenceID from PaperReferences pr
		inner join @SeedIds2 seed on seed.PaperId = pr.PaperID
		inner join @SeekingIds2 seek on seek.PaperId = pr.PaperReferenceID
	union all
	select distinct pr.PaperID from PaperReferences pr
		inner join @SeedIds2 seed on seed.PaperId = pr.PaperReferenceID
		inner join @SeekingIds2 seek on seek.PaperId = pr.PaperID
	) insert into @SeedIds2
		select paperid from cte
		except select paperid from @SeedIds2
end
select @both = count(seek.PaperId) from @SeekingIds2 seek
	inner join @SeedIds2 seed on seed.PaperId = seek.PaperId


select @N_Seeds = count(PaperId) from @SeedIds
select @N_Seeking = count(PaperId) from @SeekingIds
END
GO
/****** Object:  StoredProcedure [dbo].[st_SwitchMag]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Drop existing synonym and create new synonym to point to tables in the specified Academic database
-- =============================================
create PROCEDURE [dbo].[st_SwitchMag]
	-- Add the parameters for the stored procedure here
	@WhichOne int = 1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DROP SYNONYM IF EXISTS Affiliations
	DROP SYNONYM IF EXISTS Authors
	DROP SYNONYM IF EXISTS EntityRelatedEntities
	DROP SYNONYM IF EXISTS FieldOfStudyChildren
	DROP SYNONYM IF EXISTS FieldOfStudyExtendedAttributes
	DROP SYNONYM IF EXISTS FieldsOfStudy
	DROP SYNONYM IF EXISTS Journals
	DROP SYNONYM IF EXISTS PaperAbstractsInvertedIndex
	DROP SYNONYM IF EXISTS PaperExtendedAttributes
	DROP SYNONYM IF EXISTS PaperFieldsOfStudy
	DROP SYNONYM IF EXISTS PaperRecommendations
	DROP SYNONYM IF EXISTS PaperReferences
	DROP SYNONYM IF EXISTS PaperResources
	DROP SYNONYM IF EXISTS PaperUrls
	DROP SYNONYM IF EXISTS Papers
	DROP SYNONYM IF EXISTS RelatedFieldOfStudy

	if @WhichOne = 1
	begin
		CREATE SYNONYM Affiliations FOR AcademicSmoke.dbo.Affiliations
		CREATE SYNONYM Authors FOR AcademicSmoke.dbo.Authors
		CREATE SYNONYM EntityRelatedEntities FOR AcademicSmoke.dbo.EntityRelatedEntities
		CREATE SYNONYM FieldOfStudyChildren FOR AcademicSmoke.dbo.FieldOfStudyChildren
		CREATE SYNONYM FieldOfStudyExtendedAttributes FOR AcademicSmoke.dbo.FieldOfStudyExtendedAttributes
		CREATE SYNONYM FieldsOfStudy FOR AcademicSmoke.dbo.FieldsOfStudy
		CREATE SYNONYM Journals FOR AcademicSmoke.dbo.Journals
		CREATE SYNONYM PaperAbstractsInvertedIndex FOR AcademicSmoke.dbo.PaperAbstractsInvertedIndex
		CREATE SYNONYM PaperExtendedAttributes FOR AcademicSmoke.dbo.PaperExtendedAttributes
		CREATE SYNONYM PaperFieldsOfStudy FOR AcademicSmoke.dbo.PaperFieldsOfStudy
		CREATE SYNONYM PaperRecommendations FOR AcademicSmoke.dbo.PaperRecommendations
		CREATE SYNONYM PaperReferences FOR AcademicSmoke.dbo.PaperReferences
		CREATE SYNONYM PaperResources FOR AcademicSmoke.dbo.PaperResources
		CREATE SYNONYM PaperUrls FOR AcademicSmoke.dbo.PaperUrls
		CREATE SYNONYM Papers FOR AcademicSmoke.dbo.Papers
		CREATE SYNONYM RelatedFieldOfStudy FOR AcademicSmoke.dbo.RelatedFieldOfStudy
	end
	else
	begin
		CREATE SYNONYM Affiliations FOR AcademicSmoke.dbo.Affiliations
		CREATE SYNONYM Authors FOR AcademicSmoke.dbo.Authors
		CREATE SYNONYM EntityRelatedEntities FOR AcademicSmoke.dbo.EntityRelatedEntities
		CREATE SYNONYM FieldOfStudyChildren FOR AcademicSmoke.dbo.FieldOfStudyChildren
		CREATE SYNONYM FieldOfStudyExtendedAttributes FOR AcademicSmoke.dbo.FieldOfStudyExtendedAttributes
		CREATE SYNONYM FieldsOfStudy FOR AcademicSmoke.dbo.FieldsOfStudy
		CREATE SYNONYM Journals FOR AcademicSmoke.dbo.Journals
		CREATE SYNONYM PaperAbstractsInvertedIndex FOR AcademicSmoke.dbo.PaperAbstractsInvertedIndex
		CREATE SYNONYM PaperExtendedAttributes FOR AcademicSmoke.dbo.PaperExtendedAttributes
		CREATE SYNONYM PaperFieldsOfStudy FOR AcademicSmoke.dbo.PaperFieldsOfStudy
		CREATE SYNONYM PaperRecommendations FOR AcademicSmoke.dbo.PaperRecommendations
		CREATE SYNONYM PaperReferences FOR AcademicSmoke.dbo.PaperReferences
		CREATE SYNONYM PaperResources FOR AcademicSmoke.dbo.PaperResources
		CREATE SYNONYM PaperUrls FOR AcademicSmoke.dbo.PaperUrls
		CREATE SYNONYM Papers FOR AcademicSmoke.dbo.Papers
		CREATE SYNONYM RelatedFieldOfStudy FOR AcademicSmoke.dbo.RelatedFieldOfStudy
	end
	
END
GO
/****** Object:  StoredProcedure [dbo].[st_WorkerMagRelatedPapersRunsRunUpdates]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Cycles through the MAG_RELATED_RUN table, identifying those that need running, and runs them in sequence.
--		The aim of doing it this way is to reduce server load. i.e. we only run one review at a time.
--		This stored procedure should run automatically every minute
-- =============================================
 CREATE   PROCEDURE [dbo].[st_WorkerMagRelatedPapersRunsRunUpdates] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	Declare @RunCount int
	select @RunCount = count(*) from
	(
		SELECT * FROM sys.dm_exec_requests 
		where sql_handle is not null
	) a 
	CROSS APPLY  sys.dm_exec_sql_text(a.sql_handle) t 
	where t.text like '--RunningUpdates%'

	if @RunCount >=1 return

	Declare @MagInfo Table
	(
		current_version datetime,
		current_availability nvarchar(50)
	)
	declare @current_mag_version datetime
	declare @mag_available nvarchar(50)

	insert into @MagInfo(current_version, current_availability)
	exec st_CurrentMagInfo
	select @current_mag_version = current_version, @mag_available = current_availability from @MagInfo

	if @mag_available = 'available'
	begin
		declare @MAG_RELATED_RUN_ID int
		declare @DATE_FROM datetime
		declare @REVIEW_ID int

		DECLARE RelatedRuns CURSOR for
		SELECT MAG_RELATED_RUN_ID, DATE_FROM, REVIEW_ID FROM Reviewer.dbo.tb_MAG_RELATED_RUN
			where STATUS = 'Pending' or (AUTO_RERUN = 'true' and DATE_RUN < @current_mag_version)
		OPEN RelatedRuns  
		FETCH NEXT FROM RelatedRuns into @MAG_RELATED_RUN_ID, @DATE_FROM, @REVIEW_ID
		WHILE @@FETCH_STATUS = 0  
		BEGIN 
			exec st_MatchItemsToPapers NULL, @REVIEW_ID, NULL
			exec st_MagRelatedPapersWithDateFilter @MAG_RELATED_RUN_ID -- WITH RECOMPILE
			FETCH NEXT FROM RelatedRuns into @MAG_RELATED_RUN_ID, @DATE_FROM, @REVIEW_ID
		END
		CLOSE RelatedRuns  
		DEALLOCATE RelatedRuns  
	end
END
GO
/****** Object:  StoredProcedure [dbo].[st_WorkerMagSimulationRun]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Cycles through the MAG_SIMULATIONS table, identifying those that need running, and runs them in sequence.
--		The aim of doing it this way is to reduce server load. i.e. we only run one simulation at a time.
--		This stored procedure should run automatically every minute
-- =============================================

CREATE PROCEDURE [dbo].[st_WorkerMagSimulationRun] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	Declare @RunCount int
	select @RunCount = count(*) from
	(
		SELECT * FROM sys.dm_exec_requests 
		where sql_handle is not null
	) a 
	CROSS APPLY  sys.dm_exec_sql_text(a.sql_handle) t 
	where t.text like '--RunningSimulation%'

	if @RunCount >=1 return
	
	declare @MAG_SIMULATION_ID int

	DECLARE SimulationRuns CURSOR for
	SELECT MAG_SIMULATION_ID FROM Reviewer.dbo.TB_MAG_SIMULATION
		where STATUS = 'Pending'
	OPEN SimulationRuns  
	FETCH NEXT FROM SimulationRuns into @MAG_SIMULATION_ID
	WHILE @@FETCH_STATUS = 0  
	BEGIN 
		exec st_MagSimulationRun @MAG_SIMULATION_ID -- WITH RECOMPILE
		FETCH NEXT FROM SimulationRuns into @MAG_SIMULATION_ID
	END
	CLOSE SimulationRuns  
	DEALLOCATE SimulationRuns  
	
END
GO
/****** Object:  StoredProcedure [dbo].[st_WorkerMatchItemsToPapersRunUpdates]    Script Date: 16/01/2020 16:19:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Worker process to regulate the number of matching jobs running at once
-- =============================================
CREATE   PROCEDURE [dbo].[st_WorkerMatchItemsToPapersRunUpdates] 

AS
BEGIN
	SET NOCOUNT ON;

	-- Check whether we are already running sufficient matching jobs. If we are, then exit
	
	Declare @RunCount int
	select @RunCount = count(*) from
	(
		SELECT * FROM sys.dm_exec_requests 
		where sql_handle is not null
	) a 
	CROSS APPLY  sys.dm_exec_sql_text(a.sql_handle) t 
	where t.text like '--RunningMatches%'

	if @RunCount >= 1 RETURN

	Declare @LAST_RUN NVARCHAR(50)
	Declare @START_TIME DATETIME
	Declare @REVIEW_JOB_ID int
	Declare @REVIEW_ID int
	Declare @CONTACT_ID int
	Declare @RESULT NVARCHAR(50) = ''
	
	
	DECLARE RelatedRuns CURSOR for
		select CURRENT_STATE, START_TIME, REVIEW_JOB_ID, REVIEW_ID, CONTACT_ID
			FROM Reviewer.dbo.TB_REVIEW_JOB
			WHERE JOB_TYPE = 'MAG Item Matches' and CURRENT_STATE <> 'Complete'
		OPEN RelatedRuns  
		FETCH NEXT FROM RelatedRuns into @LAST_RUN, @START_TIME, @REVIEW_JOB_ID, @REVIEW_ID, @CONTACT_ID
		WHILE @@FETCH_STATUS = 0  
		BEGIN
			/*
			IF @LAST_RUN = 'Running' AND GETDATE() < DATEADD(HOUR, 3, @START_TIME)
			BEGIN --send back a return code to signify that the matching process is still running
				--set @RESULT = 'There is currently a matching process running on this review. Please check back later.'
				return -2
			END
			ELSE
			*/
			IF @LAST_RUN = 'Error' OR  (GETDATE() > DATEADD(HOUR, 3, @START_TIME) and @LAST_RUN = 'Running')
			BEGIN
				--set @RESULT = 'There was an error when running the last matching process. Please contact support.'
				update reviewer.dbo.tb_REVIEW_JOB
					set END_TIME = GETDATE(),
					CURRENT_STATE = 'Error',
					SUCCESS = 0
					where REVIEW_JOB_ID = @REVIEW_JOB_ID

				--return -3 -- Error when running the matching
			END

			exec st_MatchItemsToPapers @REVIEW_JOB_ID, @REVIEW_ID, @CONTACT_ID -- WITH RECOMPILE

			select @RunCount = count(*) from
			(
				SELECT * FROM sys.dm_exec_requests 
				where sql_handle is not null
			) a 
			CROSS APPLY  sys.dm_exec_sql_text(a.sql_handle) t 
			where t.text like '--RunningMatches%'
			if @RunCount >= 1 RETURN

			FETCH NEXT FROM RelatedRuns into @LAST_RUN, @START_TIME, @REVIEW_JOB_ID, @REVIEW_ID, @CONTACT_ID
		END
	CLOSE RelatedRuns  
	DEALLOCATE RelatedRuns
END
GO


USE [AcademicController]
GO

/****** Object:  UserDefinedFunction [dbo].[fn_Split]    Script Date: 16/01/2020 16:22:50 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[fn_Split](@sText varchar(8000), @sDelim varchar(20) = ' ')
RETURNS @retArray TABLE (idx smallint Primary Key, value varchar(8000) COLLATE Latin1_General_CI_AS)
AS
BEGIN
DECLARE @idx smallint,
	@value varchar(8000),
	@bcontinue bit,
	@iStrike smallint,
	@iDelimlength tinyint

IF @sDelim = 'Space'
	BEGIN
	SET @sDelim = ' '
	END

SET @idx = 0
SET @sText = LTrim(RTrim(@sText))
SET @iDelimlength = DATALENGTH(@sDelim)
SET @bcontinue = 1

IF NOT ((@iDelimlength = 0) or (@sDelim = 'Empty'))
	BEGIN
	WHILE @bcontinue = 1
		BEGIN

--If you can find the delimiter in the text, retrieve the first element and
--insert it with its index into the return table.
 
		IF CHARINDEX(@sDelim, @sText)>0
			BEGIN
			SET @value = SUBSTRING(@sText,1, CHARINDEX(@sDelim,@sText)-1)
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, @value)
				END
			
--Trim the element and its delimiter from the front of the string.
			--Increment the index and loop.
SET @iStrike = DATALENGTH(@value) + @iDelimlength
			SET @idx = @idx + 1
			SET @sText = LTrim(Right(@sText,DATALENGTH(@sText) - @iStrike))
		
			END
		ELSE
			BEGIN
--If you can’t find the delimiter in the text, @sText is the last value in
--@retArray.
 SET @value = @sText
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, @value)
				END
			--Exit the WHILE loop.
SET @bcontinue = 0
			END
		END
	END
ELSE
	BEGIN
	WHILE @bcontinue=1
		BEGIN
		--If the delimiter is an empty string, check for remaining text
		--instead of a delimiter. Insert the first character into the
		--retArray table. Trim the character from the front of the string.
--Increment the index and loop.
		IF DATALENGTH(@sText)>1
			BEGIN
			SET @value = SUBSTRING(@sText,1,1)
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, @value)
				END
			SET @idx = @idx+1
			SET @sText = SUBSTRING(@sText,2,DATALENGTH(@sText)-1)
			
			END
		ELSE
			BEGIN
			--One character remains.
			--Insert the character, and exit the WHILE loop.
			INSERT @retArray (idx, value)
			VALUES (@idx, @sText)
			SET @bcontinue = 0	
			END
	END

END

RETURN
END
GO


USE [AcademicController]
GO

/****** Object:  UserDefinedFunction [dbo].[fn_Split_int]    Script Date: 16/01/2020 16:23:46 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[fn_Split_int](@sText varchar(max), @sDelim varchar(20) = ' ')
RETURNS @retArray TABLE (idx int Primary Key, value bigint)
AS
BEGIN
DECLARE @idx int,
	@value varchar(8000),
	@bcontinue bit,
	@iStrike smallint,
	@iDelimlength tinyint
IF @sDelim = 'Space'
	BEGIN
	SET @sDelim = ' '
	END
SET @idx = 0
SET @sText = LTrim(RTrim(@sText))
SET @iDelimlength = DATALENGTH(@sDelim)
SET @bcontinue = 1
IF NOT ((@iDelimlength = 0) or (@sDelim = 'Empty'))
	BEGIN
	WHILE @bcontinue = 1
		BEGIN
--If you can find the delimiter in the text, retrieve the first element and
--insert it with its index into the return table.
 
		IF CHARINDEX(@sDelim, @sText)>0
			BEGIN
			SET @value = SUBSTRING(@sText,1, CHARINDEX(@sDelim,@sText)-1)
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, CAST(@value as bigint))
				END
			
--Trim the element and its delimiter from the front of the string.
			--Increment the index and loop.
SET @iStrike = DATALENGTH(@value) + @iDelimlength
			SET @idx = @idx + 1
			SET @sText = LTrim(Right(@sText,DATALENGTH(@sText) - @iStrike))
		
			END
		ELSE
			BEGIN
--If you can't find the delimiter in the text, @sText is the last value in
--@retArray.
 SET @value = @sText
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, CAST(@value AS bigint))
				END
			--Exit the WHILE loop.
SET @bcontinue = 0
			END
		END
	END
ELSE
	BEGIN
	WHILE @bcontinue=1
		BEGIN
		--If the delimiter is an empty string, check for remaining text
		--instead of a delimiter. Insert the first character into the
		--retArray table. Trim the character from the front of the string.
--Increment the index and loop.
		IF DATALENGTH(@sText)>1
			BEGIN
			SET @value = SUBSTRING(@sText,1,1)
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, CAST(@value as bigint))
				END
			SET @idx = @idx+1
			SET @sText = SUBSTRING(@sText,2,DATALENGTH(@sText)-1)
			
			END
		ELSE
			BEGIN
			--One character remains.
			--Insert the character, and exit the WHILE loop.
			INSERT @retArray (idx, value)
			VALUES (@idx, CAST(@sText as bigint))
			SET @bcontinue = 0	
			END
	END
END
RETURN
END


GO


USE [AcademicController]
GO

/****** Object:  UserDefinedFunction [dbo].[fn_PaperAuthors]    Script Date: 16/01/2020 16:24:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- De-normalising Authors function - rewritten to avoid cursors --

CREATE FUNCTION  [dbo].[fn_PaperAuthors]

(

@Paperid bigint

)

RETURNS nvarchar(max)

    BEGIN

	declare @res nvarchar(max)

	select @res = STRING_AGG (DisplayName, '; ') 
		from (select distinct top(50) DisplayName, AuthorSequenceNumber from [AcademicSmoke].dbo.PaperAuthorAffiliations pai
		inner join [AcademicSmoke].dbo.Authors a on a.AuthorId = pai.AuthorId
		where pai.PaperId = @Paperid  order by pai.AuthorSequenceNumber) SubSel

		return @res

	/*
        declare @res nvarchar(max)

        declare @res2 nvarchar(max)

        DECLARE cr CURSOR FAST_FORWARD FOR SELECT DisplayName

        FROM [AcademicSmoke].dbo.PaperAuthorAffiliations pau 
			inner join AcademicSmoke.dbo.Authors a on a.AuthorID = pau.AuthorId
		where PaperId = @Paperid

        ORDER BY pau.AuthorSequenceNumber

        open cr

        set @res = ''

        FETCH NEXT FROM cr INTO @res2

         WHILE @@FETCH_STATUS = 0

        BEGIN

                Set @res = @res  + @res2

                FETCH NEXT FROM cr INTO @res2

                set @res = @res + '; '

                END
		CLOSE cr
	    DEALLOCATE cr

        return @res
		*/
    END;

GO

USE [AcademicController]
GO

/****** Object:  UserDefinedFunction [dbo].[fn_PaperFirstAuthor]    Script Date: 16/01/2020 16:25:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION  [dbo].[fn_PaperFirstAuthor]

(

@Paperid bigint

)

RETURNS nvarchar(max)

    BEGIN
		declare @res nvarchar(max)

		declare @authorid bigint

		select top(1) @authorid = authorid from AcademicSmoke.dbo.PaperAuthorAffiliations paa
			where paa.PaperId = @Paperid
			order by paa.AuthorSequenceNumber

		select @res = iif (CHARINDEX(' ', NormalizedName) > 0,
	reverse(substring(reverse(NormalizedName),1,
	charindex(' ', reverse(NormalizedName)) -1)), NormalizedName) from AcademicSmoke.dbo.Authors
			where AuthorID = @authorid
			

        return @res

    END;
GO


USE [AcademicController]
GO

/****** Object:  UserDefinedFunction [dbo].[fn_PaperURLs]    Script Date: 16/01/2020 16:29:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION  [dbo].[fn_PaperURLs]

(

@Paperid bigint

)

RETURNS nvarchar(max)

    BEGIN

        declare @res nvarchar(max)

        declare @res2 nvarchar(max)

        DECLARE cr CURSOR FAST_FORWARD FOR SELECT SourceURL
			FROM [AcademicSmoke].dbo.PaperUrls pu
			where PaperId = @Paperid

        ORDER BY pu.SourceType
        open cr

        set @res = ''

        FETCH NEXT FROM cr INTO @res2

         WHILE @@FETCH_STATUS = 0

        BEGIN

                Set @res = @res  + @res2

                FETCH NEXT FROM cr INTO @res2

                set @res = @res + '; '

                END
		CLOSE cr
	    DEALLOCATE cr

        return @res

    END;

-- END OF: De-normalising function ByS --

GO


USE [AcademicController]
GO

/****** Object:  UserDefinedFunction [dbo].[fn_ReviewerItemAuthors]    Script Date: 16/01/2020 16:30:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- De-normalising Authors function - rewritten to avoid cursors --

CREATE FUNCTION  [dbo].[fn_ReviewerItemAuthors]

(

@ItemId bigint

)

RETURNS nvarchar(max)

    BEGIN

	declare @res nvarchar(max)

	select @res = STRING_AGG (DisplayName, '; ') 
		from (select distinct top(50) FIRST + ' ' + SECOND + ' ' + LAST as DisplayName, RANK from Reviewer.dbo.TB_ITEM_AUTHOR ia
		where ia.ITEM_ID = @ItemId and ROLE = 0 order by ia.RANK) SubSel

		return @res

	/*
        declare @res nvarchar(max)

        declare @res2 nvarchar(max)

        DECLARE cr CURSOR FAST_FORWARD FOR SELECT DisplayName

        FROM [AcademicSmoke].dbo.PaperAuthorAffiliations pau 
			inner join AcademicSmoke.dbo.Authors a on a.AuthorID = pau.AuthorId
		where PaperId = @Paperid

        ORDER BY pau.AuthorSequenceNumber

        open cr

        set @res = ''

        FETCH NEXT FROM cr INTO @res2

         WHILE @@FETCH_STATUS = 0

        BEGIN

                Set @res = @res  + @res2

                FETCH NEXT FROM cr INTO @res2

                set @res = @res + '; '

                END
		CLOSE cr
	    DEALLOCATE cr

        return @res
		*/
    END;

GO



