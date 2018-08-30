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
BULK INSERT [dbo].[Papers] FROM 'E:\MSAcademic\downloads\Papers.txt'
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
BULK INSERT [dbo].[PaperAbstractsInvertedIndex] FROM 'E:\MSAcademic\downloads\PaperAbstractsInvertedIndex.txt'
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
BULK INSERT [dbo].[PaperFieldsOfStudy] FROM 'E:\MSAcademic\downloads\PaperFieldsOfStudy.txt'
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
BULK INSERT [dbo].[PaperRecommendations] FROM 'E:\MSAcademic\downloads\PaperRecommendations.txt'
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
BULK INSERT [dbo].[FieldsOfStudy] FROM 'E:\MSAcademic\downloads\FieldsOfStudy.txt'
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
BULK INSERT [dbo].[PaperReferences] FROM 'E:\MSAcademic\downloads\PaperReferences.txt'
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
BULK INSERT [dbo].[PaperUrls] FROM 'E:\MSAcademic\downloads\PaperUrls.txt'
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
BULK INSERT [dbo].[FieldOfStudyChildren] FROM 'E:\MSAcademic\downloads\FieldOfStudyChildren.txt'
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
BULK INSERT [dbo].[FieldOfStudyRelationship] FROM 'E:\MSAcademic\downloads\FieldOfStudyRelationship.txt'
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
BULK INSERT [dbo].[Journals] FROM 'E:\MSAcademic\downloads\Journals.txt'
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
BULK INSERT [dbo].[Authors] FROM 'E:\MSAcademic\downloads\Authors.txt'
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
BULK INSERT [dbo].[Affiliations] FROM 'E:\MSAcademic\downloads\Affiliations.txt'
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