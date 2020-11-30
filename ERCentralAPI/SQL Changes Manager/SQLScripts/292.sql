USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDBDeleteHeaderImage]    Script Date: 26/11/2020 17:22:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[st_WebDBDeleteHeaderImage] 
	-- Add the parameters for the stored procedure here
	(
		@RevId int 
		, @WebDbId int
		, @ImageN smallint
	)
AS
BEGIN
	--sanity check, ensure @RevId and @WebDbId match...
	Declare @CheckWebDbId int = null
	set @CheckWebDbId = (select WEBDB_ID from TB_WEBDB where REVIEW_ID = @RevId and WEBDB_ID = @WebDbId)
	IF @CheckWebDbId is null return;

	if @ImageN = 1
		update TB_WEBDB set HEADER_IMAGE_1 = null where WEBDB_ID = @WebDbId
	else if @ImageN = 2
		update TB_WEBDB set HEADER_IMAGE_2 = null where WEBDB_ID = @WebDbId
	else if @ImageN = 3
		update TB_WEBDB set HEADER_IMAGE_3 = null where WEBDB_ID = @WebDbId
END
GO