create procedure [dbo].[BulkTextUpload]
(
	@FileName nvarchar(50)
--,	@Path nvarchar(500) null
)

As

SET NOCOUNT ON
-- Need to find out how to parameterise the path and filename
if @FileName = 'Papers'
begin
BULK INSERT [dbo].[Papers] FROM 'writeToThisFolder\Papers.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 100000
            ) 
end

if @FileName = 'PaperAbstractsInvertedIndex'
begin
BULK INSERT [dbo].[PaperAbstractsInvertedIndex] FROM 'writeToThisFolder\PaperAbstractsInvertedIndex.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 100000
            ) 
end

if @FileName = 'PaperFieldsOfStudy'
begin
BULK INSERT [dbo].[PaperFieldsOfStudy] FROM 'writeToThisFolder\PaperFieldsOfStudy.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 100000
            ) 
end

if @FileName = 'PaperRecommendations'
begin
BULK INSERT [dbo].[PaperRecommendations] FROM 'writeToThisFolder\PaperRecommendations.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 100000
            ) 
end

if @FileName = 'FieldsOfStudy'
begin
BULK INSERT [dbo].[FieldsOfStudy] FROM 'writeToThisFolder\FieldsOfStudy.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 100000
            ) 
end

if @FileName = 'PaperReferences'
begin
BULK INSERT [dbo].[PaperReferences] FROM 'writeToThisFolder\PaperReferences.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 100000
            ) 
end

if @FileName = 'PaperUrls'
begin
BULK INSERT [dbo].[PaperUrls] FROM 'writeToThisFolder\PaperUrls.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 100000
            ) 
end

if @FileName = 'FieldOfStudyChildren'
begin
BULK INSERT [dbo].[FieldOfStudyChildren] FROM 'writeToThisFolder\FieldOfStudyChildren.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 100000
            ) 
end

if @FileName = 'FieldOfStudyRelationship'
begin
BULK INSERT [dbo].[FieldOfStudyRelationship] FROM 'writeToThisFolder\FieldOfStudyRelationship.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 100000
            ) 
end

if @FileName = 'Journals'
begin
BULK INSERT [dbo].[Journals] FROM 'writeToThisFolder\Journals.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 100000
            ) 
end

if @FileName = 'Authors'
begin
BULK INSERT [dbo].[Authors] FROM 'writeToThisFolder\Authors.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 100000
            ) 
end

if @FileName = 'Affiliations'
begin
BULK INSERT [dbo].[Affiliations] FROM 'writeToThisFolder\Affiliations.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 100000
            ) 
end

SET NOCOUNT OFF