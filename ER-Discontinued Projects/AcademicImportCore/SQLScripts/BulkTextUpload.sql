		BULK INSERT [dbo].Affiliations FROM 'M:\Download\mag-2019-12-26\mag\Affiliations.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            ) 

		BULK INSERT [dbo].Authors FROM 'M:\Download\mag-2019-12-26\mag\Authors.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

		BULK INSERT [dbo].EntityRelatedEntities FROM 'M:\Download\mag-2019-12-26\advanced\EntityRelatedEntities.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].FieldOfStudyChildren FROM 'M:\Download\mag-2019-12-26\advanced\FieldOfStudyChildren.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].FieldOfStudyExtendedAttributes FROM 'M:\Download\mag-2019-12-26\advanced\FieldOfStudyExtendedAttributes.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].FieldsOfStudy FROM 'M:\Download\mag-2019-12-26\advanced\FieldsOfStudy.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].Journals FROM 'M:\Download\mag-2019-12-26\mag\Journals.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperAbstractsInvertedIndex FROM 'M:\Download\mag-2019-12-26\nlp\PaperAbstractsInvertedIndex.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperAuthorAffiliations FROM 'M:\Download\mag-2019-12-26\mag\PaperAuthorAffiliations.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperExtendedAttributes FROM 'M:\Download\mag-2019-12-26\mag\PaperExtendedAttributes.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperFieldsOfStudy FROM 'M:\Download\mag-2019-12-26\advanced\PaperFieldsOfStudy.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )
			
			/*
			BULK INSERT [dbo].PaperLanguages FROM 'M:\Download\mag-2019-12-26\nlp\PaperLanguages.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )
			*/
			BULK INSERT [dbo].PaperRecommendations FROM 'M:\Download\mag-2019-12-26\advanced\PaperRecommendations.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperReferences FROM 'M:\Download\mag-2019-12-26\mag\PaperReferences.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperResources FROM 'M:\Download\mag-2019-12-26\mag\PaperResources.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperUrls FROM 'M:\Download\mag-2019-12-26\mag\PaperUrls.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].Papers FROM 'M:\Download\mag-2019-12-26\mag\Papers.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].RelatedFieldOfStudy FROM 'M:\Download\mag-2019-12-26\advanced\RelatedFieldOfStudy.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BEGIN TRANSACTION
GO
ALTER TABLE dbo.Papers ADD
	SearchText nvarchar(500) NULL
GO
ALTER TABLE dbo.Papers SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

update Papers set SearchText = AcademicControllerNew.dbo.ToShortSearchText(papers.PaperTitle)
GO

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20190928-141510] ON [dbo].[Authors] ([AuthorID]) INCLUDE ([DisplayName], [NormalizedName]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20190716-235058] ON [dbo].[FieldOfStudyChildren] ([ChildFieldOfStudyId]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-fosc-fosc] ON [dbo].[FieldOfStudyChildren] ([FieldOfStudyId]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-fos_fosid] ON [dbo].[FieldsOfStudy] ([FieldOfStudyId]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20190928-170417] ON [dbo].[Journals] ([JournalId]) INCLUDE ([DisplayName], [NormalizedName]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-pai_pid] ON [dbo].[PaperAbstractsInvertedIndex] ([PaperId]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20190919-111456] ON [dbo].[PaperAuthorAffiliations] ([PaperId]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20190927-091422] ON [dbo].[PaperAuthorAffiliations] ([AuthorId]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20190927-100030] ON [dbo].[PaperAuthorAffiliations] ([PaperId], [AuthorId], [AuthorSequenceNumber]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-pfos_fosid] ON [dbo].[PaperFieldsOfStudy] ([FieldOfStudyID]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-pfos_pid] ON [dbo].[PaperFieldsOfStudy] ([PaperId]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20190927-164350] ON [dbo].[PaperFieldsOfStudy] ([FieldOfStudyID], [Score], [PaperId]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-pr_pid] ON [dbo].[PaperRecommendations] ([PaperId]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-pr_rpid] ON [dbo].[PaperRecommendations] ([RecommendedPaperId]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [PaperIdRecommendedPaperId] ON [dbo].[PaperRecommendations] ([PaperId], [RecommendedPaperId]) INCLUDE ([Score]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [RecommendedPaperIdPaperId] ON [dbo].[PaperRecommendations] ([RecommendedPaperId], [PaperId]) INCLUDE ([Score]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-pr_pid] ON [dbo].[PaperReferences] ([PaperId]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-pr_prid] ON [dbo].[PaperReferences] ([PaperReferenceID]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [PaperIdPaperReferenceId] ON [dbo].[PaperReferences] ([PaperId], [PaperReferenceID]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [PaperReferenceIdPaperId] ON [dbo].[PaperReferences] ([PaperReferenceID], [PaperId]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [IndexSearchText] ON [dbo].[Papers] ([SearchText]) INCLUDE ([FirstPage], [JournalID], [PaperId], [Volume], [Year]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-p_pid] ON [dbo].[Papers] ([PaperId]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [IndexDoi] ON [dbo].[Papers] ([DOI]) INCLUDE ([FirstPage], [JournalID], [PaperId], [SearchText], [Volume], [Year]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [IndexVolumeFirstPage] ON [dbo].[Papers] ([Volume], [FirstPage]) INCLUDE ([JournalID], [PaperId], [SearchText], [Year]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [IndexPaperIdYear] ON [dbo].[Papers] ([PaperId], [Year]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [IndexPaperIdCreatedDate] ON [dbo].[Papers] ([PaperId], [CreatedDate]) ON [PRIMARY];
CREATE NONCLUSTERED INDEX [NonClusteredIndex-Purls_pid] ON [dbo].[PaperUrls] ([PaperId]) ON [PRIMARY];

