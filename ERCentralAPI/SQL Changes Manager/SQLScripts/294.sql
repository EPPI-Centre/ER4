USE [Reviewer]
GO


IF COL_LENGTH('dbo.TB_WEBDB', 'HEADER_IMAGE_EXT_1') IS NULL
BEGIN
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION

ALTER TABLE dbo.TB_WEBDB ADD
	HEADER_IMAGE_EXT_1 varchar(10) NULL,
	HEADER_IMAGE_EXT_2 varchar(10) NULL,
	HEADER_IMAGE_EXT_3 varchar(10) NULL

ALTER TABLE dbo.TB_WEBDB SET (LOCK_ESCALATION = TABLE)

COMMIT
END
GO


/****** Object:  StoredProcedure [dbo].[st_WebDBgetClosedAccess]    Script Date: 27/11/2020 10:33:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE or ALTER   procedure [dbo].[st_WebDBgetImages]
(
	@WebDBid int
)

As
BEGIN
select HEADER_IMAGE_1, HEADER_IMAGE_EXT_1, HEADER_IMAGE_2, HEADER_IMAGE_EXT_2, HEADER_IMAGE_3, HEADER_IMAGE_EXT_3  from TB_WEBDB w where w.WEBDB_ID = @WebDBid 
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbUploadImage]    Script Date: 27/11/2020 16:13:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER   PROCEDURE [dbo].[st_WebDbUploadImage]
	-- Add the parameters for the stored procedure here
	@RevId int,
	@WebDbId int,
	@BIN IMAGE,
	@Extension nchar(10),
	@imageNumber tinyint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	if @imageNumber = 1
		update TB_WEBDB set HEADER_IMAGE_1 = @BIN, HEADER_IMAGE_EXT_1 = @Extension where WEBDB_ID = @WebDbId and REVIEW_ID = @RevId
	else if @imageNumber = 2
		update TB_WEBDB set HEADER_IMAGE_2 = @BIN, HEADER_IMAGE_EXT_2 = @Extension where WEBDB_ID = @WebDbId and REVIEW_ID = @RevId
	else if @imageNumber = 3
		update TB_WEBDB set HEADER_IMAGE_3 = @BIN, HEADER_IMAGE_EXT_3 = @Extension where WEBDB_ID = @WebDbId and REVIEW_ID = @RevId
END

GO