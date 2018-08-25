-- This is just an example file of bulk upload. It shows two things:
-- 1. The syntax to use in the BULK INSERT using TABLOCK and BATCHSIZE to maximise efficiency and minimise log file size
-- 2. How it's possible to use a view to insert into a table with possibly a different # of fields compared with the text file.
-- This is useful if we ever need to have a primary key on the table (which otherwise just takes forever on import).

CREATE VIEW abstract2view
as
SELECT PaperId, IndexedAbstract
FROM PaperAbstractInvertedIndex2
GO


BULK INSERT [dbo].[abstract2view] FROM 'E:\MSAcademic\PaperAbstractsInvertedIndex.txt'
           WITH ( 
               DATAFILETYPE = 'char', 
               FIELDTERMINATOR = '\t', 
               ROWTERMINATOR = '\n', 
               FIRSTROW = 1 ,
			   TABLOCK,
			   BATCHSIZE = 100000
            ) 