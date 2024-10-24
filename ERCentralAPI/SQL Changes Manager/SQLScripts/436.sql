USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbMapEdit]    Script Date: 17/09/2021 17:15:55 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_WebDbMapEdit]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@WEBDB_MAP_ID int,
	@ColumnsSetID int,
	@ColumnsAttributeID bigint,
	@RowsSetID int,
	@RowsAttributeID bigint,
	@SegmentsSetID int,
	@SegmentsAttributeID bigint,
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
/****** Object:  StoredProcedure [dbo].[st_WebDbMapAdd]    Script Date: 20/09/2021 14:46:07 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER   PROCEDURE [dbo].[st_WebDbMapAdd]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@ColumnsSetID int,
	@ColumnsAttributeID bigint,
	@RowsSetID int,
	@RowsAttributeID bigint,
	@SegmentsSetID int,
	@SegmentsAttributeID bigint,
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