		BULK INSERT [dbo].Affiliations FROM 'M:\Download\mag\Affiliations.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            ) 

		BULK INSERT [dbo].Authors FROM 'M:\Download\mag\Authors.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

		BULK INSERT [dbo].EntityRelatedEntities FROM 'M:\Download\advanced\EntityRelatedEntities.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].FieldOfStudyChildren FROM 'M:\Download\advanced\FieldOfStudyChildren.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].FieldOfStudyExtendedAttributes FROM 'M:\Download\advanced\FieldOfStudyExtendedAttributes.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].FieldsOfStudy FROM 'M:\Download\advanced\FieldsOfStudy.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].Journals FROM 'M:\Download\mag\Journals.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperAbstractsInvertedIndex FROM 'M:\Download\nlp\PaperAbstractsInvertedIndex.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperAuthorAffiliations FROM 'M:\Download\mag\PaperAuthorAffiliations.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperExtendedAttributes FROM 'M:\Download\mag\PaperExtendedAttributes.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperFieldsOfStudy FROM 'M:\Download\advanced\PaperFieldsOfStudy.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )
			
			/*
			BULK INSERT [dbo].PaperLanguages FROM 'M:\Download\nlp\PaperLanguages.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )
			*/
			BULK INSERT [dbo].PaperRecommendations FROM 'M:\Download\advanced\PaperRecommendations.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperReferences FROM 'M:\Download\mag\PaperReferences.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperResources FROM 'M:\Download\mag\PaperResources.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperUrls FROM 'M:\Download\mag\PaperUrls.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].Papers FROM 'M:\Download\mag\Papers.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].RelatedFieldOfStudy FROM 'M:\Download\advanced\RelatedFieldOfStudy.txt'
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
	st2 nvarchar(1000) NULL
GO
ALTER TABLE dbo.Papers SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

update Papers set SearchText = dbo.ToShortSearchText(papers.PaperTitle)








