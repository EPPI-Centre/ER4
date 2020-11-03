USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRuns]    Script Date: 26/10/2020 15:44:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all runs for a given review
-- =============================================
ALTER     PROCEDURE [dbo].[st_MagRelatedPapersRuns] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select mrr.MAG_RELATED_RUN_ID, mrr.REVIEW_ID, mrr.USER_DESCRIPTION, mrr.ATTRIBUTE_ID, a.ATTRIBUTE_NAME,
		mrr.ALL_INCLUDED, mrr.DATE_FROM, mrr.DATE_RUN, mrr.STATUS, mrr.USER_STATUS, mrr.N_PAPERS,
		mrr.MODE, mrr.Filtered from tb_MAG_RELATED_RUN mrr
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mrr.ATTRIBUTE_ID
		where review_id = @REVIEW_ID
		order by MAG_RELATED_RUN_ID
		
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsInsert]    Script Date: 26/10/2020 15:42:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
ALTER   PROCEDURE [dbo].[st_MagRelatedPapersRunsInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int
,	@USER_DESCRIPTION NVARCHAR(1000)
,	@PaperIdList nvarchar(max)
,	@ATTRIBUTE_ID bigint = 0
,	@ALL_INCLUDED BIT
,	@DATE_FROM DATETIME
--,	@AUTO_RERUN BIT
,	@MODE nvarchar(50)
,	@FILTERED NVARCHAR(50)
,	@STATUS nvarchar(50)
,	@MAG_RELATED_RUN_ID int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	insert into tb_MAG_RELATED_RUN(REVIEW_ID, USER_DESCRIPTION, PaperIdList, ATTRIBUTE_ID,
		ALL_INCLUDED, DATE_FROM, STATUS, USER_STATUS, MODE, Filtered, N_PAPERS)
	values(@REVIEW_ID, @USER_DESCRIPTION, @PaperIdList, @ATTRIBUTE_ID,
		@ALL_INCLUDED, @DATE_FROM, @STATUS, 'Waiting', @MODE, @FILTERED, 0)

	set @MAG_RELATED_RUN_ID = @@IDENTITY

END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsUpdate]    Script Date: 26/10/2020 15:45:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers in a given run
-- =============================================
ALTER     PROCEDURE [dbo].[st_MagRelatedPapersRunsUpdate] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID int,
	--@AUTO_RERUN BIT,
	@USER_DESCRIPTION NVARCHAR(1000)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	update tb_MAG_RELATED_RUN
		set USER_STATUS = 'Checked',
		--AUTO_RERUN = @AUTO_RERUN,
		USER_DESCRIPTION = @USER_DESCRIPTION
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsUpdatePostRun]    Script Date: 26/10/2020 16:16:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	After the related item ids are found, this updates the record for the UI list
-- =============================================
ALTER   PROCEDURE [dbo].[st_MagRelatedPapersRunsUpdatePostRun] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID int,
	@N_PAPERS int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	update tb_MAG_RELATED_RUN
		set [STATUS] = 'Complete',
		N_PAPERS = @N_PAPERS,
		DATE_RUN = GETDATE(),
		USER_STATUS = 'Not imported'
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
END
GO