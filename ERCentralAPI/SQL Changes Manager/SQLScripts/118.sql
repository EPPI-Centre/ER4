USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributePDFUpdate]    Script Date: 10/14/2019 12:18:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[st_ItemAttributePDFUpdate]
	-- Add the parameters for the stored procedure here
	@ITEM_ATTRIBUTE_PDF_ID bigint
	,@SHAPE_TEXT varchar(max)
	,@INTERVALS varchar(max)
	,@TEXTS nvarchar(max)
	,@REVIEW_ID INT
	,@PDFTRON_XML nvarchar(max) = ''
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

-- Insert statements for procedure here

	declare @check int = 0

	declare @itemID bigint = (select IA.ITEM_ID from 
	TB_ITEM_ATTRIBUTE IA INNER JOIN 
	TB_ITEM_ATTRIBUTE_PDF IAP ON IA.ITEM_ATTRIBUTE_ID = IAP.ITEM_ATTRIBUTE_ID
	where ITEM_ATTRIBUTE_PDF_ID = @ITEM_ATTRIBUTE_PDF_ID) 

	set @check = (select count(*) from 
	TB_ITEM_REVIEW where ITEM_ID = @itemID AND REVIEW_ID = @REVIEW_ID)

	if(@check != 1) return

	
	UPDATE TB_ITEM_ATTRIBUTE_PDF
	SET SHAPE_TEXT = @SHAPE_TEXT
		,SELECTION_INTERVALS = @INTERVALS
		,SELECTION_TEXTS = @TEXTS
		,PDFTRON_XML = @PDFTRON_XML
	WHERE ITEM_ATTRIBUTE_PDF_ID = @ITEM_ATTRIBUTE_PDF_ID


END
GO