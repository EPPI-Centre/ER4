USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedRun_Update]    Script Date: 11/02/2021 11:43:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio
-- Create date: 
-- Description:	mark a line in tb_MAG_RELATED_RUN as @STATUS
-- =============================================
ALTER PROCEDURE [dbo].[st_MagRelatedRun_Update] 
	-- Add the parameters for the stored procedure here
	@USER_STATUS nvarchar(50),
	@STATUS nvarchar(50),
	@REVIEW_ID INT,
	@MAG_RELATED_RUN_ID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	update tb_MAG_RELATED_RUN
			set USER_STATUS = @USER_STATUS,
			[STATUS] = @STATUS
			where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID and REVIEW_ID = @REVIEW_ID
END
GO

IF COL_LENGTH('dbo.TB_MAG_AUTO_UPDATE_RUN', 'MAG_VERSION') IS NULL
BEGIN
ALTER TABLE dbo.TB_MAG_AUTO_UPDATE_RUN ADD
	MAG_VERSION nvarchar(20) NULL
end
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRuns]    Script Date: 11/02/2021 15:37:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all runs for a given review
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdateRuns] 
	-- Add the parameters for the stored procedure here	
	@REVIEW_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select mau.MAG_AUTO_UPDATE_RUN_ID, mau.USER_DESCRIPTION, mau.ATTRIBUTE_ID, mau.ALL_INCLUDED,
		a.ATTRIBUTE_NAME, mau.DATE_RUN, mau.N_PAPERS, mau.REVIEW_ID, mau.MAG_AUTO_UPDATE_ID,
		mau.STUDY_TYPE_CLASSIFIER, mau.USER_CLASSIFIER_MODEL_ID, mau.USER_CLASSIFIER_REVIEW_ID,
		cm.MODEL_TITLE, mau.MAG_VERSION
		from TB_MAG_AUTO_UPDATE_RUN mau
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mau.ATTRIBUTE_ID
		left outer join tb_CLASSIFIER_MODEL cm on cm.MODEL_ID = mau.USER_CLASSIFIER_MODEL_ID
		where mau.REVIEW_ID = @REVIEW_ID
		order by MAU.MAG_AUTO_UPDATE_RUN_ID desc
		
END
go
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagContReviewInsertResults]    Script Date: 11/02/2021 15:29:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Last stage in the ContReview workflow: put the 'found' papers in and update tb_MAG_RELATED_RUN
-- =============================================
ALTER PROCEDURE [dbo].[st_MagContReviewInsertResults] 
	-- Add the parameters for the stored procedure here
	@MAG_VERSION nvarchar(20)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE MAUR
		SET MAUR.N_PAPERS = idcount,
			MAUR.MAG_VERSION = @MAG_VERSION
		FROM TB_MAG_AUTO_UPDATE_RUN MAUR
		inner join (SELECT MAG_AUTO_UPDATE_RUN_ID, COUNT(*) idcount FROM TB_MAG_AUTO_UPDATE_RUN_PAPER
			GROUP BY MAG_AUTO_UPDATE_RUN_ID) AS COUNTS ON COUNTS.MAG_AUTO_UPDATE_RUN_ID = MAUR.MAG_AUTO_UPDATE_RUN_ID
		where maur.N_PAPERS = -1

	UPDATE MAURP
		SET MAURP.REVIEW_ID = MAUR.REVIEW_ID
		FROM TB_MAG_AUTO_UPDATE_RUN_PAPER MAURP
		INNER JOIN TB_MAG_AUTO_UPDATE_RUN MAUR on MAUR.MAG_AUTO_UPDATE_RUN_ID = MAURP.MAG_AUTO_UPDATE_RUN_ID
		WHERE MAURP.REVIEW_ID IS NULL

	-- i.e. in the rare situation that we have zero papers returned
	UPDATE TB_MAG_AUTO_UPDATE_RUN
		SET N_PAPERS = 0,
		MAG_VERSION = @MAG_VERSION
		WHERE N_PAPERS = -1

END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRunCountResults]    Script Date: 11/02/2021 22:17:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all results for a given simulation
-- =============================================
ALTER PROCEDURE [dbo].[st_MagAutoUpdateRunCountResults] 
	-- Add the parameters for the stored procedure here
	
	@MagAutoUpdateRunId int
,	@AutoUpdateScore float
,	@StudyTypeClassifierScore float
,	@UserClassifierScore float
,	@ResultCount int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select @ResultCount = count(distinct PaperId) from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MagAutoUpdateRunId and
			ContReviewScore >= @AutoUpdateScore and
			StudyTypeClassifierScore >= @StudyTypeClassifierScore and
			UserClassifierScore >= @UserClassifierScore
END
GO