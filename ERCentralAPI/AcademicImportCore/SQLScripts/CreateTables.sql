DROP TABLE IF EXISTS [dbo].[Papers]
CREATE TABLE [dbo].[Papers](
	[PaperID] [bigint] NULL,
	[Rank] [int] NULL,
	[DOI] [nvarchar](500) NULL,
	[DocType] [nvarchar](500) NULL,
	[PaperTitle] [nvarchar](max) NOT NULL,
	[OriginalTitle] [nvarchar](max) NOT NULL,
	[BookTitle] [nvarchar](max) NULL,
	[Year] [int] NULL,
	[Date] [datetime] NULL,
	[Publisher] [nvarchar](max) NULL,
	[JournalID] [bigint] NULL,
	[ConferenceSeriesID] [bigint] NULL,
	[ConferenceInstanceID] [bigint] NULL,
	[Volume] [nvarchar](50) NULL,
	[Issue] [nvarchar](50) NULL,
	[FirstPage] [nvarchar](50) NULL,
	[LastPage] [nvarchar](50) NULL,
	[ReferenceCount] [bigint] NULL,
	[CitationCount] [bigint] NULL,
	[EstimatedCitationCount] [int] NULL,
	[OriginalVenue] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

DROP TABLE IF EXISTS [dbo].[PaperReferences]
CREATE TABLE [dbo].[PaperReferences](
	[PaperID] [bigint] NULL,
	[PaperReferenceID] [bigint] NULL
) ON [PRIMARY]

DROP TABLE IF EXISTS [dbo].[PaperRecommendations]
CREATE TABLE [dbo].[PaperRecommendations](
	[PaperID] [bigint] NULL,
	[RecommendedPaperID] [bigint] NULL,
	[Score] [float] NULL
) ON [PRIMARY]

DROP TABLE IF EXISTS [dbo].[PaperFieldsOfStudy]
CREATE TABLE [dbo].[PaperFieldsOfStudy](
	[PaperID] [bigint] NULL,
	[FieldOfStudyID] [bigint] NULL,
	[Similarity] [float] NULL
) ON [PRIMARY]

DROP TABLE IF EXISTS [dbo].[PaperAbstractsInvertedIndex]
CREATE TABLE [dbo].[PaperAbstractsInvertedIndex](
	[PaperID] [bigint] NULL,
	[IndexedAbstract] [nvarchar](max) NULL
) ON [PRIMARY]

DROP TABLE IF EXISTS [dbo].[Authors]
CREATE TABLE [dbo].[Authors](
	[AuthorID] [bigint] NULL,
	[Rank] [int] NULL,
	[NormalizedName] [nvarchar](500) NULL,
	[DisplayName] [nvarchar](500) NULL,
	[LastKnownAffiliationId] [bigint] NULL,
	[PaperCount] [bigint] NULL,
	[CitationCount] [bigint] NULL,
	[CreatedDate] [datetime] NULL
) ON [PRIMARY]

DROP TABLE IF EXISTS [dbo].[Affiliations]
CREATE TABLE [dbo].[Affiliations](
	[AffiliationID] [bigint] NULL,
	[Rank] [int] NULL,
	[NormalizedName] [nvarchar](500) NULL,
	[DisplayName] [nvarchar](500) NULL,
	[GridID] [nvarchar](500) NULL,
	[OfficialPage] [nvarchar](1000) NULL,
	[WikiPage] [nvarchar](1000) NULL,
	[PaperCount] [bigint] NULL,
	[CitationCount] [bigint] NULL,
	[CreatedDate] [datetime] NULL
) ON [PRIMARY]

DROP TABLE IF EXISTS [dbo].[PaperUrls]
CREATE TABLE [dbo].[PaperUrls](
	[PaperId] [bigint] NULL,
	[SourceType] [int] NULL,
	[SourceURL] [nvarchar](2500) NULL
) ON [PRIMARY]

DROP TABLE IF EXISTS [dbo].[FieldOfStudyChildren]
CREATE TABLE [dbo].[FieldOfStudyChildren](
	[FieldOfStudyId] [bigint] NULL,
	[ChildFieldOfStudyId] [bigint] NULL
) ON [PRIMARY]

DROP TABLE IF EXISTS [dbo].[RelatedFieldOfStudy]
CREATE TABLE [dbo].[RelatedFieldOfStudy](
	[FieldOfStudyId1] [bigint] NULL,
	[Type1] [nvarchar](1000) NULL,
	[FieldOfStudyId2] [bigint] NULL,
	[Type2] [nvarchar](1000) NULL,
	[Rank] [float] NULL
) ON [PRIMARY]
GO

DROP TABLE IF EXISTS [dbo].[FieldsOfStudy]
CREATE TABLE [dbo].[FieldsOfStudy](
	[FieldOfStudyId] [bigint] NULL,
	[Rank] [int] NULL,
	[NormalizedName] [nvarchar](500) NULL,
	[DisplayName] [nvarchar](500) NULL,
	[MainType] [nvarchar](500) NULL,
	[Level] [int] NULL,
	[PaperCount] [bigint] NULL,
	[CitationCount] [bigint] NULL,
	[CreatedDate] [datetime] NULL
) ON [PRIMARY]

DROP TABLE IF EXISTS [dbo].[Journals]
CREATE TABLE [dbo].[Journals](
	[JournalId] [bigint] NULL,
	[Rank] [int] NULL,
	[NormalizedName] [nvarchar](500) NULL,
	[DisplayName] [nvarchar](500) NULL,
	[ISSN] [nvarchar](500) NULL,
	[Publisher] [nvarchar](500) NULL,
	[Webpage] [nvarchar](500) NULL,
	[PaperCount] [bigint] NULL,
	[CitationCount] [bigint] NULL,
	[CreatedDate] [datetime] NULL
) ON [PRIMARY]