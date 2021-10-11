USE [Reviewer]
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_WEBDB_MAP')
BEGIN
	--select 'will delete!'
	ALTER TABLE [dbo].[TB_WEBDB_MAP] DROP CONSTRAINT [FK_TB_WEBDB_MAP_TB_WEBDB_PUBLIC_SET2]
	ALTER TABLE [dbo].[TB_WEBDB_MAP] DROP CONSTRAINT [FK_TB_WEBDB_MAP_TB_WEBDB_PUBLIC_SET1]
	ALTER TABLE [dbo].[TB_WEBDB_MAP] DROP CONSTRAINT [FK_TB_WEBDB_MAP_TB_WEBDB_PUBLIC_SET]
	ALTER TABLE [dbo].[TB_WEBDB_MAP] DROP CONSTRAINT [FK_TB_WEBDB_MAP_TB_WEBDB]
	DROP TABLE [dbo].[TB_WEBDB_MAP]
END

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TB_WEBDB_MAP](
	[WEBDB_MAP_ID] [int] IDENTITY(1,1) NOT NULL,
	[WEBDB_ID] [int] NOT NULL,
	[COLUMNS_PUBLIC_ATTRIBUTE_ID] [int] NOT NULL,
	[COLUMNS_PUBLIC_SET_ID] [int] NOT NULL,
	[ROWS_PUBLIC_ATTRIBUTE_ID] [int] NOT NULL,
	[ROWS_PUBLIC_SET_ID] [int] NOT NULL,
	[SEGMENTS_PUBLIC_ATTRIBUTE_ID] [int] NOT NULL,
	[SEGMENTS_PUBLIC_SET_ID] [int] NOT NULL,
	[MAP_NAME] [nvarchar](1000) NOT NULL,
	[MAP_DESCRIPTION] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_TB_WEBDB_MAP] PRIMARY KEY CLUSTERED 
(
	[WEBDB_MAP_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[TB_WEBDB_MAP]  WITH CHECK ADD  CONSTRAINT [FK_TB_WEBDB_MAP_TB_WEBDB] FOREIGN KEY([WEBDB_ID])
REFERENCES [dbo].[TB_WEBDB] ([WEBDB_ID])
GO
ALTER TABLE [dbo].[TB_WEBDB_MAP] CHECK CONSTRAINT [FK_TB_WEBDB_MAP_TB_WEBDB]
GO
ALTER TABLE [dbo].[TB_WEBDB_MAP]  WITH CHECK ADD  CONSTRAINT [FK_TB_WEBDB_MAP_TB_WEBDB_PUBLIC_SET] FOREIGN KEY([COLUMNS_PUBLIC_SET_ID])
REFERENCES [dbo].[TB_WEBDB_PUBLIC_SET] ([WEBDB_PUBLIC_SET_ID])
GO
ALTER TABLE [dbo].[TB_WEBDB_MAP] CHECK CONSTRAINT [FK_TB_WEBDB_MAP_TB_WEBDB_PUBLIC_SET]
GO
ALTER TABLE [dbo].[TB_WEBDB_MAP]  WITH CHECK ADD  CONSTRAINT [FK_TB_WEBDB_MAP_TB_WEBDB_PUBLIC_SET1] FOREIGN KEY([ROWS_PUBLIC_SET_ID])
REFERENCES [dbo].[TB_WEBDB_PUBLIC_SET] ([WEBDB_PUBLIC_SET_ID])
GO
ALTER TABLE [dbo].[TB_WEBDB_MAP] CHECK CONSTRAINT [FK_TB_WEBDB_MAP_TB_WEBDB_PUBLIC_SET1]
GO
ALTER TABLE [dbo].[TB_WEBDB_MAP]  WITH CHECK ADD  CONSTRAINT [FK_TB_WEBDB_MAP_TB_WEBDB_PUBLIC_SET2] FOREIGN KEY([SEGMENTS_PUBLIC_SET_ID])
REFERENCES [dbo].[TB_WEBDB_PUBLIC_SET] ([WEBDB_PUBLIC_SET_ID])
GO
ALTER TABLE [dbo].[TB_WEBDB_MAP] CHECK CONSTRAINT [FK_TB_WEBDB_MAP_TB_WEBDB_PUBLIC_SET2]
GO

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_WebDbCodesetAdd]    Script Date: 09/09/2021 16:56:01 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE or ALTER PROCEDURE [dbo].[st_WebDbMapAdd]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@ColumnsSetID int,
	@ColumnsAttributeID int,
	@RowsSetID int,
	@RowsAttributeID int,
	@SegmentsSetID int,
	@SegmentsAttributeID int,
	@MapName nvarchar(1000),
	@MapDescription nvarchar(max),
	@WEBDB_MAP_ID int output
)
As
--first, check that all parameters match up and get the value we need...
declare @ColumnsPublicSetID int = (select rps.WEBDB_PUBLIC_SET_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rps 
							on  w.REVIEW_ID = @REVIEW_ID and rps.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID
						inner join TB_REVIEW_SET rs on rps.REVIEW_SET_ID = rs.REVIEW_SET_ID and rs.REVIEW_ID = w.REVIEW_ID and rs.SET_ID = @ColumnsSetID
						)
IF @ColumnsPublicSetID is null OR @ColumnsPublicSetID < 1 return

declare @RowsPublicSetID int = (select rps.WEBDB_PUBLIC_SET_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rps 
							on  w.REVIEW_ID = @REVIEW_ID and rps.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID
						inner join TB_REVIEW_SET rs on rps.REVIEW_SET_ID = rs.REVIEW_SET_ID and rs.REVIEW_ID = w.REVIEW_ID and rs.SET_ID = @RowsSetID
						)
IF @RowsPublicSetID is null OR @RowsPublicSetID < 1 return

declare @SegmentsPublicSetID int = (select rps.WEBDB_PUBLIC_SET_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rps 
							on  w.REVIEW_ID = @REVIEW_ID and rps.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID
						inner join TB_REVIEW_SET rs on rps.REVIEW_SET_ID = rs.REVIEW_SET_ID and rs.REVIEW_ID = w.REVIEW_ID and rs.SET_ID = @SegmentsSetID
						)
IF @SegmentsPublicSetID is null OR @SegmentsPublicSetID < 1 return

declare @ColumnsPublicAttributeID int
IF @ColumnsAttributeID > 0
begin 
	set @ColumnsPublicAttributeID = (select pa.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa 
							on w.REVIEW_ID = @REVIEW_ID and pa.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						inner join TB_ATTRIBUTE a on pa.ATTRIBUTE_ID = a.ATTRIBUTE_ID and pa.ATTRIBUTE_ID = @ColumnsAttributeID
						)
	IF @ColumnsPublicAttributeID is null OR @ColumnsPublicAttributeID < 1 return
end
else SET @ColumnsPublicAttributeID = 0


declare @RowsPublicAttributeID int
IF @RowsAttributeID > 0
BEGIN
	set @RowsPublicAttributeID = (select pa.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa 
							on w.REVIEW_ID = @REVIEW_ID and pa.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						inner join TB_ATTRIBUTE a on pa.ATTRIBUTE_ID = a.ATTRIBUTE_ID and pa.ATTRIBUTE_ID = @RowsAttributeID
						)
	IF @RowsPublicAttributeID is null OR @RowsPublicAttributeID < 1 return
END
else SET @RowsPublicAttributeID = 0

declare @SegmentsPublicAttributeID int
IF @SegmentsAttributeID > 0
BEGIN
	set @SegmentsPublicAttributeID = (select pa.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa 
							on w.REVIEW_ID = @REVIEW_ID and pa.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						inner join TB_ATTRIBUTE a on pa.ATTRIBUTE_ID = a.ATTRIBUTE_ID and pa.ATTRIBUTE_ID = @SegmentsAttributeID
						)
	IF @SegmentsPublicAttributeID is null OR @SegmentsPublicAttributeID < 1 return
END
else SET @SegmentsPublicAttributeID = 0
--OK, all checks match up, phew


INSERT INTO TB_WEBDB_MAP
           ([WEBDB_ID]
           ,[COLUMNS_PUBLIC_ATTRIBUTE_ID]
           ,[COLUMNS_PUBLIC_SET_ID]
           ,[ROWS_PUBLIC_ATTRIBUTE_ID]
           ,[ROWS_PUBLIC_SET_ID]
           ,[SEGMENTS_PUBLIC_ATTRIBUTE_ID]
           ,[SEGMENTS_PUBLIC_SET_ID]
           ,[MAP_NAME]
           ,[MAP_DESCRIPTION])
     VALUES
           (@WEBDB_ID
           , @ColumnsPublicAttributeID
           , @ColumnsPublicSetID
           , @RowsPublicAttributeID
           , @RowsPublicSetID
           , @SegmentsPublicAttributeID
           , @SegmentsPublicSetID
           , @MapName
           , @MapDescription)

set @WEBDB_MAP_ID = SCOPE_IDENTITY()

GO

CREATE or ALTER PROCEDURE [dbo].[st_WebDbMapEdit]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@WEBDB_MAP_ID int,
	@ColumnsSetID int,
	@ColumnsAttributeID int,
	@RowsSetID int,
	@RowsAttributeID int,
	@SegmentsSetID int,
	@SegmentsAttributeID int,
	@MapName nvarchar(1000),
	@MapDescription nvarchar(max)
)
As
--first, check that all parameters match up and get the value we need...
declare @ColumnsPublicSetID int = (select rps.WEBDB_PUBLIC_SET_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rps 
							on  w.REVIEW_ID = @REVIEW_ID and rps.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID
						inner join TB_REVIEW_SET rs on rps.REVIEW_SET_ID = rs.REVIEW_SET_ID and rs.REVIEW_ID = w.REVIEW_ID and rs.SET_ID = @ColumnsSetID
						)
IF @ColumnsPublicSetID is null OR @ColumnsPublicSetID < 1 return

declare @RowsPublicSetID int = (select rps.WEBDB_PUBLIC_SET_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rps 
							on  w.REVIEW_ID = @REVIEW_ID and rps.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID
						inner join TB_REVIEW_SET rs on rps.REVIEW_SET_ID = rs.REVIEW_SET_ID and rs.REVIEW_ID = w.REVIEW_ID and rs.SET_ID = @RowsSetID
						)
IF @RowsPublicSetID is null OR @RowsPublicSetID < 1 return

declare @SegmentsPublicSetID int = (select rps.WEBDB_PUBLIC_SET_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rps 
							on  w.REVIEW_ID = @REVIEW_ID and rps.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID
						inner join TB_REVIEW_SET rs on rps.REVIEW_SET_ID = rs.REVIEW_SET_ID and rs.REVIEW_ID = w.REVIEW_ID and rs.SET_ID = @SegmentsSetID
						)
IF @SegmentsPublicSetID is null OR @SegmentsPublicSetID < 1 return

declare @ColumnsPublicAttributeID int
IF @ColumnsAttributeID > 0
begin 
	set @ColumnsPublicAttributeID = (select pa.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa 
							on w.REVIEW_ID = @REVIEW_ID and pa.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						inner join TB_ATTRIBUTE a on pa.ATTRIBUTE_ID = a.ATTRIBUTE_ID and pa.ATTRIBUTE_ID = @ColumnsAttributeID
						)
	IF @ColumnsPublicAttributeID is null OR @ColumnsPublicAttributeID < 1 return
end
else SET @ColumnsPublicAttributeID = 0


declare @RowsPublicAttributeID int
IF @RowsAttributeID > 0
BEGIN
	set @RowsPublicAttributeID = (select pa.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa 
							on w.REVIEW_ID = @REVIEW_ID and pa.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						inner join TB_ATTRIBUTE a on pa.ATTRIBUTE_ID = a.ATTRIBUTE_ID and pa.ATTRIBUTE_ID = @RowsAttributeID
						)
	IF @RowsPublicAttributeID is null OR @RowsPublicAttributeID < 1 return
END
else SET @RowsPublicAttributeID = 0

declare @SegmentsPublicAttributeID int
IF @SegmentsAttributeID = 0
BEGIN
	set @SegmentsPublicAttributeID = (select pa.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE pa 
							on w.REVIEW_ID = @REVIEW_ID and pa.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						inner join TB_ATTRIBUTE a on pa.ATTRIBUTE_ID = a.ATTRIBUTE_ID and pa.ATTRIBUTE_ID = @SegmentsAttributeID
						)
	IF @SegmentsPublicAttributeID is null OR @SegmentsPublicAttributeID < 1 return
END
else SET @SegmentsPublicAttributeID = 0
--OK, all checks match up, phew

UPDATE TB_WEBDB_MAP set 
           [COLUMNS_PUBLIC_ATTRIBUTE_ID] =  @ColumnsPublicAttributeID
           ,[COLUMNS_PUBLIC_SET_ID] = @ColumnsPublicSetID
           ,[ROWS_PUBLIC_ATTRIBUTE_ID] = @RowsPublicAttributeID
           ,[ROWS_PUBLIC_SET_ID] = @RowsPublicSetID
           ,[SEGMENTS_PUBLIC_ATTRIBUTE_ID] = @SegmentsPublicAttributeID
           ,[SEGMENTS_PUBLIC_SET_ID] = @SegmentsPublicSetID
           ,[MAP_NAME] = @MapName
           ,[MAP_DESCRIPTION] = @MapDescription
     where WEBDB_MAP_ID = @WEBDB_MAP_ID and WEBDB_ID = @WEBDB_ID

GO

CREATE or ALTER PROCEDURE [dbo].[st_WebDbMapDelete]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@WEBDB_MAP_ID int
)
As
--first, check that all parameters match up...
declare @check int = (select m.WEBDB_MAP_ID from TB_WEBDB w
						inner join TB_WEBDB_MAP m
							on m.WEBDB_MAP_ID = @WEBDB_MAP_ID and w.REVIEW_ID = @REVIEW_ID and m.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return
--OK, all checks match up, phew

delete from TB_WEBDB_MAP
     where WEBDB_MAP_ID = @WEBDB_MAP_ID and WEBDB_ID = @WEBDB_ID

GO

CREATE or ALTER PROCEDURE [dbo].[st_WebDbMapsList]
(
	@REVIEW_ID INT,
	@WEBDB_ID int
)
As
--first, check that all parameters match up...
declare @check int = (select WEBDB_ID from TB_WEBDB w
						where w.REVIEW_ID = @REVIEW_ID and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return
--OK, all checks match up, phew

select m.*, s1.SET_ID as [COLUMNS_SET_ID], s2.SET_ID as [ROWS_SET_ID], s3.SET_ID as [SEGMENTS_SET_ID]
	, a1.ATTRIBUTE_ID as [COLUMNS_ATTRIBUTE_ID], a2.ATTRIBUTE_ID as [ROWS_ATTRIBUTE_ID], a3.ATTRIBUTE_ID as [SEGMENTS_ATTRIBUTE_ID]
	, CASE when (ps1.WEBDB_SET_NAME = '' OR ps1.WEBDB_SET_NAME is null) then s1.SET_NAME
		else ps1.WEBDB_SET_NAME
	END as COLUMNS_SET_NAME
	, CASE when (ps2.WEBDB_SET_NAME = '' OR ps2.WEBDB_SET_NAME is null) then s2.SET_NAME
		else ps2.WEBDB_SET_NAME
	END as ROWS_SET_NAME
	, CASE when (ps3.WEBDB_SET_NAME = '' OR ps3.WEBDB_SET_NAME is null) then s3.SET_NAME
		else ps3.WEBDB_SET_NAME
	END as SEGMENTS_SET_NAME

	, CASE when ((pa1.WEBDB_ATTRIBUTE_NAME = '' OR pa1.WEBDB_ATTRIBUTE_NAME is null) AND m.COLUMNS_PUBLIC_ATTRIBUTE_ID > 0) then a1.ATTRIBUTE_NAME
		WHEN (pa1.WEBDB_ATTRIBUTE_NAME is null and m.COLUMNS_PUBLIC_ATTRIBUTE_ID = 0) then ''
		else pa1.WEBDB_ATTRIBUTE_NAME
	END as COLUMNS_ATTRIBUTE_NAME
	, CASE when ((pa2.WEBDB_ATTRIBUTE_NAME = '' OR pa2.WEBDB_ATTRIBUTE_NAME is null )AND m.ROWS_PUBLIC_ATTRIBUTE_ID > 0) then a2.ATTRIBUTE_NAME
		WHEN (pa2.WEBDB_ATTRIBUTE_NAME is null and m.ROWS_PUBLIC_ATTRIBUTE_ID = 0) then ''
		else pa2.WEBDB_ATTRIBUTE_NAME
	END as ROWS_ATTRIBUTE_NAME
	, CASE when ((pa3.WEBDB_ATTRIBUTE_NAME = '' OR pa3.WEBDB_ATTRIBUTE_NAME is null) AND m.SEGMENTS_PUBLIC_ATTRIBUTE_ID > 0) then a3.ATTRIBUTE_NAME
		WHEN (pa3.WEBDB_ATTRIBUTE_NAME is null and m.SEGMENTS_PUBLIC_ATTRIBUTE_ID = 0) then ''
		else pa3.WEBDB_ATTRIBUTE_NAME
	END as SEGMENTS_ATTRIBUTE_NAME


	from TB_WEBDB_MAP m
	inner join TB_WEBDB_PUBLIC_SET ps1 on m.COLUMNS_PUBLIC_SET_ID = ps1.WEBDB_PUBLIC_SET_ID and ps1.WEBDB_ID = m.WEBDB_ID
	inner join TB_REVIEW_SET rs1 on ps1.REVIEW_SET_ID = rs1.REVIEW_SET_ID and rs1.REVIEW_ID = @REVIEW_ID
	inner join tb_set s1 on rs1.SET_ID = s1.SET_ID
	left join TB_WEBDB_PUBLIC_ATTRIBUTE pa1 on m.COLUMNS_PUBLIC_ATTRIBUTE_ID = pa1.WEBDB_PUBLIC_ATTRIBUTE_ID
	left join TB_ATTRIBUTE a1 on pa1.ATTRIBUTE_ID = a1.ATTRIBUTE_ID

	inner join TB_WEBDB_PUBLIC_SET ps2 on m.ROWS_PUBLIC_SET_ID = ps2.WEBDB_PUBLIC_SET_ID and ps2.WEBDB_ID = m.WEBDB_ID
	inner join TB_REVIEW_SET rs2 on ps2.REVIEW_SET_ID = rs2.REVIEW_SET_ID and rs2.REVIEW_ID = @REVIEW_ID
	inner join tb_set s2 on rs2.SET_ID = s2.SET_ID
	left join TB_WEBDB_PUBLIC_ATTRIBUTE pa2 on m.ROWS_PUBLIC_ATTRIBUTE_ID = pa2.WEBDB_PUBLIC_ATTRIBUTE_ID
	left join TB_ATTRIBUTE a2 on pa2.ATTRIBUTE_ID = a2.ATTRIBUTE_ID

	inner join TB_WEBDB_PUBLIC_SET ps3 on m.SEGMENTS_PUBLIC_SET_ID = ps3.WEBDB_PUBLIC_SET_ID and ps3.WEBDB_ID = m.WEBDB_ID
	inner join TB_REVIEW_SET rs3 on ps3.REVIEW_SET_ID = rs3.REVIEW_SET_ID and rs3.REVIEW_ID = @REVIEW_ID
	inner join tb_set s3 on rs3.SET_ID = s3.SET_ID
	left join TB_WEBDB_PUBLIC_ATTRIBUTE pa3 on m.SEGMENTS_PUBLIC_ATTRIBUTE_ID = pa3.WEBDB_PUBLIC_ATTRIBUTE_ID
	left join TB_ATTRIBUTE a3 on pa3.ATTRIBUTE_ID = a3.ATTRIBUTE_ID
    where m.WEBDB_ID = @WEBDB_ID

GO

CREATE or ALTER PROCEDURE [dbo].[st_WebDbMap]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@WEBDB_MAP_ID INT
)
As
--first, check that all parameters match up...
declare @check int = (select WEBDB_ID from TB_WEBDB w
						where w.REVIEW_ID = @REVIEW_ID and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return
--OK, all checks match up, phew

select m.*, s1.SET_ID as [COLUMNS_SET_ID], s2.SET_ID as [ROWS_SET_ID], s3.SET_ID as [SEGMENTS_SET_ID]
	, a1.ATTRIBUTE_ID as [COLUMNS_ATTRIBUTE_ID], a2.ATTRIBUTE_ID as [ROWS_ATTRIBUTE_ID], a3.ATTRIBUTE_ID as [SEGMENTS_ATTRIBUTE_ID]
	, CASE when (ps1.WEBDB_SET_NAME = '' OR ps1.WEBDB_SET_NAME is null) then s1.SET_NAME
		else ps1.WEBDB_SET_NAME
	END as COLUMNS_SET_NAME
	, CASE when (ps2.WEBDB_SET_NAME = '' OR ps2.WEBDB_SET_NAME is null) then s2.SET_NAME
		else ps2.WEBDB_SET_NAME
	END as ROWS_SET_NAME
	, CASE when (ps3.WEBDB_SET_NAME = '' OR ps3.WEBDB_SET_NAME is null) then s3.SET_NAME
		else ps3.WEBDB_SET_NAME
	END as SEGMENTS_SET_NAME

	, CASE when ((pa1.WEBDB_ATTRIBUTE_NAME = '' OR pa1.WEBDB_ATTRIBUTE_NAME is null) AND m.COLUMNS_PUBLIC_ATTRIBUTE_ID > 0) then a1.ATTRIBUTE_NAME
		WHEN (pa1.WEBDB_ATTRIBUTE_NAME is null and m.COLUMNS_PUBLIC_ATTRIBUTE_ID = 0) then ''
		else pa1.WEBDB_ATTRIBUTE_NAME
	END as COLUMNS_ATTRIBUTE_NAME
	, CASE when ((pa2.WEBDB_ATTRIBUTE_NAME = '' OR pa2.WEBDB_ATTRIBUTE_NAME is null )AND m.ROWS_PUBLIC_ATTRIBUTE_ID > 0) then a2.ATTRIBUTE_NAME
		WHEN (pa2.WEBDB_ATTRIBUTE_NAME is null and m.ROWS_PUBLIC_ATTRIBUTE_ID = 0) then ''
		else pa2.WEBDB_ATTRIBUTE_NAME
	END as ROWS_ATTRIBUTE_NAME
	, CASE when ((pa3.WEBDB_ATTRIBUTE_NAME = '' OR pa3.WEBDB_ATTRIBUTE_NAME is null) AND m.SEGMENTS_PUBLIC_ATTRIBUTE_ID > 0) then a3.ATTRIBUTE_NAME
		WHEN (pa3.WEBDB_ATTRIBUTE_NAME is null and m.SEGMENTS_PUBLIC_ATTRIBUTE_ID = 0) then ''
		else pa3.WEBDB_ATTRIBUTE_NAME
	END as SEGMENTS_ATTRIBUTE_NAME

	from TB_WEBDB_MAP m
	inner join TB_WEBDB_PUBLIC_SET ps1 on m.COLUMNS_PUBLIC_SET_ID = ps1.WEBDB_PUBLIC_SET_ID and ps1.WEBDB_ID = m.WEBDB_ID
	inner join TB_REVIEW_SET rs1 on ps1.REVIEW_SET_ID = rs1.REVIEW_SET_ID and rs1.REVIEW_ID = @REVIEW_ID
	inner join tb_set s1 on rs1.SET_ID = s1.SET_ID
	left join TB_WEBDB_PUBLIC_ATTRIBUTE pa1 on m.COLUMNS_PUBLIC_ATTRIBUTE_ID = pa1.WEBDB_PUBLIC_ATTRIBUTE_ID
	left join TB_ATTRIBUTE a1 on pa1.ATTRIBUTE_ID = a1.ATTRIBUTE_ID

	inner join TB_WEBDB_PUBLIC_SET ps2 on m.ROWS_PUBLIC_SET_ID = ps2.WEBDB_PUBLIC_SET_ID and ps2.WEBDB_ID = m.WEBDB_ID
	inner join TB_REVIEW_SET rs2 on ps2.REVIEW_SET_ID = rs2.REVIEW_SET_ID and rs2.REVIEW_ID = @REVIEW_ID
	inner join tb_set s2 on rs2.SET_ID = s2.SET_ID
	left join TB_WEBDB_PUBLIC_ATTRIBUTE pa2 on m.ROWS_PUBLIC_ATTRIBUTE_ID = pa2.WEBDB_PUBLIC_ATTRIBUTE_ID
	left join TB_ATTRIBUTE a2 on pa2.ATTRIBUTE_ID = a2.ATTRIBUTE_ID

	inner join TB_WEBDB_PUBLIC_SET ps3 on m.SEGMENTS_PUBLIC_SET_ID = ps3.WEBDB_PUBLIC_SET_ID and ps3.WEBDB_ID = m.WEBDB_ID
	inner join TB_REVIEW_SET rs3 on ps3.REVIEW_SET_ID = rs3.REVIEW_SET_ID and rs3.REVIEW_ID = @REVIEW_ID
	inner join tb_set s3 on rs3.SET_ID = s3.SET_ID
	left join TB_WEBDB_PUBLIC_ATTRIBUTE pa3 on m.SEGMENTS_PUBLIC_ATTRIBUTE_ID = pa3.WEBDB_PUBLIC_ATTRIBUTE_ID
	left join TB_ATTRIBUTE a3 on pa3.ATTRIBUTE_ID = a3.ATTRIBUTE_ID
     where WEBDB_MAP_ID = @WEBDB_MAP_ID

GO



--to be commented out! the below only works on SG's machine, but you can use it with appropriate ID values to make it work elsewhere
--INSERT INTO [TB_WEBDB_MAP]
--           ([WEBDB_ID]
--           ,[COLUMNS_PUBLIC_ATTRIBUTE_ID]
--           ,[COLUMNS_PUBLIC_SET_ID]
--           ,[ROWS_PUBLIC_ATTRIBUTE_ID]
--           ,[ROWS_PUBLIC_SET_ID]
--           ,[SEGMENTS_PUBLIC_ATTRIBUTE_ID]
--           ,[SEGMENTS_PUBLIC_SET_ID]
--           ,[MAP_NAME]
--           ,[MAP_DESCRIPTION])
--     VALUES
--           (2,4551,46,4565, 46, 4547, 46, 'my first map', 'yep, <strong>the first</strong>!! <p>I''m not joking, although this gets <u>recreated</u> by a script.</p>')

--INSERT INTO [TB_WEBDB_MAP]
--           ([WEBDB_ID]
--           ,[COLUMNS_PUBLIC_ATTRIBUTE_ID]
--           ,[COLUMNS_PUBLIC_SET_ID]
--           ,[ROWS_PUBLIC_ATTRIBUTE_ID]
--           ,[ROWS_PUBLIC_SET_ID]
--           ,[SEGMENTS_PUBLIC_ATTRIBUTE_ID]
--           ,[SEGMENTS_PUBLIC_SET_ID]
--           ,[MAP_NAME]
--           ,[MAP_DESCRIPTION])
--     VALUES
--           (2,4528,13,4529, 13, 1440, 13, 'my 2nd map', 'yep, <strong>the second</strong>!! <p>I''m not joking, although this gets <u>recreated</u> by a script.</p>')


--DECLARE	@return_value int,
--		@WEBDB_MAP_ID int

--EXEC	@return_value = [dbo].[st_WebDbMapAdd]
--		@REVIEW_ID = 99,
--		@WEBDB_ID = 2,
--		@ColumnsSetID = 644,
--		@ColumnsAttributeID = 0,
--		@RowsSetID = 1880,
--		@RowsAttributeID = 121689,
--		@SegmentsSetID = 894,
--		@SegmentsAttributeID = 72166,
--		@MapName = N'third map!',
--		@MapDescription = N'<h4>really</h4> this is the 3rd map and is probably broken.',
--		@WEBDB_MAP_ID = @WEBDB_MAP_ID OUTPUT

--SELECT	@WEBDB_MAP_ID as N'@WEBDB_MAP_ID'

--END OF: to be commented out!


GO





