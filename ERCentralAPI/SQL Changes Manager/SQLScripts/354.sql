USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagUpdateMissingAbstract]    Script Date: 28/02/2021 10:05:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Update MAG items with abstracts (where they have been found)
-- =============================================
create or alter PROCEDURE [dbo].[st_MagUpdateMissingAbstract] 
	@ITEM_ID bigint
,	@ABSTRACT nvarchar(max)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE TB_ITEM
		SET ABSTRACT = @ABSTRACT
		WHERE ITEM_ID = @ITEM_ID

END
go
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagGetItemsWithMissingAbstracts]    Script Date: 28/02/2021 11:28:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

