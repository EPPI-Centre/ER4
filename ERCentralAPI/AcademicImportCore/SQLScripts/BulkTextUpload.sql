
/*
		BULK INSERT [dbo].Affiliations FROM 'D:\MSAcademic\downloads\mag\Affiliations.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            ) 

		BULK INSERT [dbo].Authors FROM 'D:\MSAcademic\downloads\mag\Authors.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

		BULK INSERT [dbo].EntityRelatedEntities FROM 'D:\MSAcademic\downloads\advanced\EntityRelatedEntities.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].FieldOfStudyChildren FROM 'D:\MSAcademic\downloads\advanced\FieldOfStudyChildren.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].FieldOfStudyExtendedAttributes FROM 'D:\MSAcademic\downloads\advanced\FieldOfStudyExtendedAttributes.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].FieldsOfStudy FROM 'D:\MSAcademic\downloads\advanced\FieldsOfStudy.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].Journals FROM 'D:\MSAcademic\downloads\mag\Journals.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperAbstractsInvertedIndex FROM 'D:\MSAcademic\downloads\nlp\PaperAbstractsInvertedIndex.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )
*/
			BULK INSERT [dbo].PaperAuthorAffiliations FROM 'D:\MSAcademic\downloads\mag\PaperAuthorAffiliations.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperExtendedAttributes FROM 'D:\MSAcademic\downloads\mag\PaperExtendedAttributes.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperFieldsOfStudy FROM 'D:\MSAcademic\downloads\advanced\PaperFieldsOfStudy.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )
			/*
			BULK INSERT [dbo].PaperLanguages FROM 'D:\MSAcademic\downloads\nlp\PaperLanguages.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )
			*/
			BULK INSERT [dbo].PaperRecommendations FROM 'D:\MSAcademic\downloads\advanced\PaperRecommendations.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperReferences FROM 'D:\MSAcademic\downloads\mag\PaperReferences.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperResources FROM 'D:\MSAcademic\downloads\mag\PaperResources.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].PaperUrls FROM 'D:\MSAcademic\downloads\mag\PaperUrls.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].Papers FROM 'D:\MSAcademic\downloads\mag\Papers.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )

			BULK INSERT [dbo].RelatedFieldOfStudy FROM 'D:\MSAcademic\downloads\advanced\RelatedFieldOfStudy.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 5000
            )










