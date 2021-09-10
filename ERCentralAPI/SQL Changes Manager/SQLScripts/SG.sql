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
	@ColumnsPublicSetID int,
	@ColumnsPublicAttributeID int,
	@RowsPublicSetID int,
	@RowsPublicAttributeID int,
	@SegmentsPublicSetID int,
	@SegmentsPublicAttributeID int,
	@MapName nvarchar(1000),
	@MapDescription nvarchar(max),
	@WEBDB_MAP_ID int output
)
As
--first, check that all parameters match up...
declare @check int = (select review_set_id from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rs 
							on rs.WEBDB_PUBLIC_SET_ID = @ColumnsPublicSetID and w.REVIEW_ID = @REVIEW_ID and rs.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return

set @check = (select review_set_id from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rs 
							on rs.WEBDB_PUBLIC_SET_ID = @RowsPublicSetID and w.REVIEW_ID = @REVIEW_ID and rs.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return

set @check = (select review_set_id from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rs 
							on rs.WEBDB_PUBLIC_SET_ID = @SegmentsPublicSetID and w.REVIEW_ID = @REVIEW_ID and rs.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return

set @check = (select a.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE a 
							on a.WEBDB_PUBLIC_ATTRIBUTE_ID = @ColumnsPublicAttributeID and w.REVIEW_ID = @REVIEW_ID and a.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return

set @check = (select a.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE a 
							on a.WEBDB_PUBLIC_ATTRIBUTE_ID = @RowsPublicAttributeID and w.REVIEW_ID = @REVIEW_ID and a.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return

set @check = (select a.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE a 
							on a.WEBDB_PUBLIC_ATTRIBUTE_ID = @SegmentsPublicAttributeID and w.REVIEW_ID = @REVIEW_ID and a.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return
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
	@ColumnsPublicSetID int,
	@ColumnsPublicAttributeID int,
	@RowsPublicSetID int,
	@RowsPublicAttributeID int,
	@SegmentsPublicSetID int,
	@SegmentsPublicAttributeID int,
	@MapName nvarchar(1000),
	@MapDescription nvarchar(max)
)
As
--first, check that all parameters match up...
declare @check int = (select m.WEBDB_MAP_ID from TB_WEBDB w
						inner join TB_WEBDB_MAP m
							on m.WEBDB_MAP_ID = @WEBDB_MAP_ID and w.REVIEW_ID = @REVIEW_ID and m.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return

set @check = (select review_set_id from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rs 
							on rs.WEBDB_PUBLIC_SET_ID = @ColumnsPublicSetID and w.REVIEW_ID = @REVIEW_ID and rs.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return
set @check = (select review_set_id from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rs 
							on rs.WEBDB_PUBLIC_SET_ID = @RowsPublicSetID and w.REVIEW_ID = @REVIEW_ID and rs.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return

set @check = (select review_set_id from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_SET rs 
							on rs.WEBDB_PUBLIC_SET_ID = @SegmentsPublicSetID and w.REVIEW_ID = @REVIEW_ID and rs.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return

set @check = (select a.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE a 
							on a.WEBDB_PUBLIC_ATTRIBUTE_ID = @ColumnsPublicAttributeID and w.REVIEW_ID = @REVIEW_ID and a.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return

set @check = (select a.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE a 
							on a.WEBDB_PUBLIC_ATTRIBUTE_ID = @RowsPublicAttributeID and w.REVIEW_ID = @REVIEW_ID and a.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return

set @check = (select a.WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB w
						inner join TB_WEBDB_PUBLIC_ATTRIBUTE a 
							on a.WEBDB_PUBLIC_ATTRIBUTE_ID = @SegmentsPublicAttributeID and w.REVIEW_ID = @REVIEW_ID and a.WEBDB_ID = w.WEBDB_ID 
							and w.WEBDB_ID = @WEBDB_ID 
						)
IF @check is null OR @check < 1 return
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