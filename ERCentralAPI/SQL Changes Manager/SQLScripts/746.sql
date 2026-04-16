Use Reviewer
GO

IF COL_LENGTH('dbo.TB_SOURCE', 'NOTES') = 8000 AND COL_LENGTH('dbo.TB_SOURCE', 'NOTES') != -1
BEGIN 
	--select 'I will do it'
	print 'Expanding TB_SOURCE.NOTES column'
	ALTER TABLE TB_SOURCE DROP CONSTRAINT [DF_TB_SOURCE_NOTES]
	ALTER TABLE TB_SOURCE ALTER COLUMN NOTES NVARCHAR (MAX) NOT NULL;
	ALTER TABLE TB_SOURCE ADD CONSTRAINT [DF_TB_SOURCE_NOTES]  DEFAULT ('') FOR [NOTES]
END
GO

/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRun]    Script Date: 05/10/2019 00:01:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get a single run
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagRelatedPapersRun] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_id int = 0
,	@MAG_RELATED_RUN_ID INT = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here


	
	select mrr.MAG_RELATED_RUN_ID, mrr.REVIEW_ID, mrr.USER_DESCRIPTION, mrr.ATTRIBUTE_ID, a.ATTRIBUTE_NAME,
		mrr.ALL_INCLUDED, mrr.DATE_FROM, mrr.DATE_RUN, mrr.AUTO_RERUN, mrr.STATUS, mrr.USER_STATUS, mrr.N_PAPERS,
		mrr.MODE, mrr.Filtered
		
		from Reviewer.dbo.tb_MAG_RELATED_RUN mrr
		left outer join Reviewer.dbo.TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mrr.ATTRIBUTE_ID
		where review_id = @REVIEW_id AND MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
		order by MAG_RELATED_RUN_ID
		
END
GO