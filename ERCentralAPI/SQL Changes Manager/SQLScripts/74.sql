use Reviewer
GO

IF COL_LENGTH('dbo.TB_ITEM_ATTRIBUTE_PDF', 'PDFTRON_XML') IS NULL
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

ALTER TABLE dbo.TB_ITEM_ATTRIBUTE_PDF ADD
	PDFTRON_XML nvarchar(MAX) NULL

ALTER TABLE dbo.TB_ITEM_ATTRIBUTE_PDF SET (LOCK_ESCALATION = TABLE)

COMMIT
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributePDFInsert]    Script Date: 15/04/2019 19:20:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ItemAttributePDFInsert]
	-- Add the parameters for the stored procedure here
	@ITEM_ATTRIBUTE_ID bigint
	,@ITEM_DOCUMENT_ID bigint
	,@PAGE int
	,@SHAPE_TEXT varchar(max)
	,@INTERVALS varchar(max)
	,@TEXTS nvarchar(max)
	,@ITEM_ATTRIBUTE_PDF_ID bigint output
	,@PDFTRON_XML varchar(max) = ''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    INSERT INTO TB_ITEM_ATTRIBUTE_PDF
           ([ITEM_DOCUMENT_ID]
           ,[ITEM_ATTRIBUTE_ID]
           ,[PAGE]
           ,[SHAPE_TEXT]
           ,[SELECTION_INTERVALS]
           ,[SELECTION_TEXTS]
		   ,PDFTRON_XML)
     VALUES
           (@ITEM_DOCUMENT_ID
           ,@ITEM_ATTRIBUTE_ID
           ,@PAGE
           ,@SHAPE_TEXT
           ,@INTERVALS
           ,@TEXTS
		   ,@PDFTRON_XML)
	set @ITEM_ATTRIBUTE_PDF_ID = @@IDENTITY
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributePDFUpdate]    Script Date: 15/04/2019 19:21:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ItemAttributePDFUpdate]
	-- Add the parameters for the stored procedure here
	@ITEM_ATTRIBUTE_PDF_ID bigint
	,@SHAPE_TEXT varchar(max)
	,@INTERVALS varchar(max)
	,@TEXTS nvarchar(max)
	,@PDFTRON_XML nvarchar(max) = ''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    UPDATE TB_ITEM_ATTRIBUTE_PDF
    SET SHAPE_TEXT = @SHAPE_TEXT
      ,SELECTION_INTERVALS = @INTERVALS
      ,SELECTION_TEXTS = @TEXTS
	  ,PDFTRON_XML = @PDFTRON_XML
    WHERE ITEM_ATTRIBUTE_PDF_ID = @ITEM_ATTRIBUTE_PDF_ID


END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ItemAttributePDFSinglePage]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].st_ItemAttributePDFSinglePage
GO
USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ItemAttributePDF]    Script Date: 16/04/2019 15:33:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ItemAttributePDFSinglePage]
	-- Add the parameters for the stored procedure here
	@ITEM_ATTRIBUTE_PDF_ID bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT * from TB_ITEM_ATTRIBUTE_PDF where ITEM_ATTRIBUTE_PDF_ID = @ITEM_ATTRIBUTE_PDF_ID 
END
GO

