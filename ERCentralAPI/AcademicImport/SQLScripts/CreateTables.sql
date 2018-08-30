/****** Object:  Table [dbo].[Papers]    Script Date: 23/08/2018 21:22:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

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
	[CreatedDate] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [dbo].[PaperReferences]    Script Date: 23/08/2018 21:23:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PaperReferences](
	[PaperID] [bigint] NULL,
	[PaperReferenceID] [bigint] NULL
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[PaperRecommendations]    Script Date: 23/08/2018 21:23:51 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PaperRecommendations](
	[PaperID] [bigint] NULL,
	[RecommendedPaperID] [bigint] NULL,
	[Score] [float] NULL
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[PaperFieldsOfStudy]    Script Date: 23/08/2018 21:24:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PaperFieldsOfStudy](
	[PaperID] [bigint] NULL,
	[FieldOfStudyID] [bigint] NULL,
	[Similarity] [float] NULL
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[PaperAbstractInvertedIndex]    Script Date: 23/08/2018 21:24:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PaperAbstractInvertedIndex](
	[PaperID] [bigint] NULL,
	[IndexedAbstract] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO