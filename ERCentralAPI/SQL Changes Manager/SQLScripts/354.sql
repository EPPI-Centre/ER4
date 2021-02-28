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
-- Description:	Get recent MAG items without abstracts (to see if we can find them now)
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

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Get recent MAG items without abstracts (to see if we can find them now)
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_MagGetItemsWithMissingAbstracts] 
	-- Add the parameters for the stored procedure here

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select imm.ITEM_ID, imm.PaperId, imm.REVIEW_ID from tb_ITEM_MAG_MATCH imm
		inner join TB_MAG_NEW_PAPERS np on np.PaperId = imm.PaperId
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false'
		inner join TB_ITEM i on i.ITEM_ID = imm.ITEM_ID and (i.ABSTRACT is null or i.ABSTRACT = '')
		where np.MagVersion in
			(select top(4) mag_folder from TB_MAG_CURRENT_INFO order by MAG_CURRENT_INFO_ID desc) and
			imm.AutoMatchScore > 0.95

END
go
