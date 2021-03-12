USE [Reviewer]
If IndexProperty(Object_Id('[dbo].[TB_MAG_AUTO_UPDATE_RUN_PAPER]'), 'idx_MagAutoUpdateRunId_and_UserClassifierScore', 'IndexID') Is Null
  begin
    CREATE NONCLUSTERED INDEX idx_MagAutoUpdateRunId_and_UserClassifierScore ON [dbo].TB_MAG_AUTO_UPDATE_RUN_PAPER
	(
		[MAG_AUTO_UPDATE_RUN_ID] ASC,
		UserClassifierScore ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
  end
GO
USE [Reviewer]
If IndexProperty(Object_Id('[dbo].[TB_MAG_AUTO_UPDATE_RUN_PAPER]'), 'idx_MagAutoUpdateRunId_and_StudyTypeClassifierScore', 'IndexID') Is Null
  begin
    CREATE NONCLUSTERED INDEX idx_MagAutoUpdateRunId_and_StudyTypeClassifierScore ON [dbo].TB_MAG_AUTO_UPDATE_RUN_PAPER
	(
		[MAG_AUTO_UPDATE_RUN_ID] ASC,
		StudyTypeClassifierScore ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
  end
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSetGetCompleted]    Script Date: 12/03/2021 11:20:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_ItemSetGetCompleted] (
	@REVIEW_ID INT,
	@ITEM_ID BIGINT,
	@SET_ID INT
)

As

SET NOCOUNT ON

	SELECT ITEM_SET_ID FROM TB_ITEM_SET iset
	INNER JOIN TB_ITEM_REVIEW IR on ir.ITEM_ID = iset.ITEM_ID and ir.IS_DELETED = 'false'
	inner join TB_REVIEW_SET rs on rs.REVIEW_ID = ir.REVIEW_ID and rs.SET_ID = iset.SET_ID
	WHERE iset.ITEM_ID = @ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and iset.SET_ID = @SET_ID and iset.IS_COMPLETED = 'true'

SET NOCOUNT OFF
GO