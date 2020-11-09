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

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_MAG_AUTO_UPDATE_RUN_PAPER')
BEGIN
DROP TABLE [dbo].[TB_MAG_AUTO_UPDATE_RUN_PAPER]
END
GO
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_MAG_AUTO_UPDATE_RUN')
BEGIN
DROP TABLE [dbo].[TB_MAG_AUTO_UPDATE_RUN]
END
GO
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_MAG_AUTO_UPDATE')
BEGIN
DROP TABLE [dbo].[TB_MAG_AUTO_UPDATE]
END
GO
CREATE TABLE dbo.TB_MAG_AUTO_UPDATE
	(
	MAG_AUTO_UPDATE_ID int NOT NULL IDENTITY (1, 1),
	USER_DESCRIPTION nvarchar(1000) NULL,
	ATTRIBUTE_ID bigint NULL,
	ALL_INCLUDED bit NULL,
	STUDY_TYPE_CLASSIFIER nvarchar(50) NULL,
	USER_CLASSIFIER_MODEL_ID int NULL,
	USER_CLASSIFIER_REVIEW_ID int NULL,
	REVIEW_ID INT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TB_MAG_AUTO_UPDATE ADD CONSTRAINT
	PK_TB_MAG_AUTO_UPDATE PRIMARY KEY CLUSTERED 
	(
	MAG_AUTO_UPDATE_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
BEGIN TRANSACTION
GO
ALTER TABLE dbo.tb_CLASSIFIER_MODEL SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_MAG_AUTO_UPDATE ADD CONSTRAINT
	FK_TB_MAG_AUTO_UPDATE_tb_CLASSIFIER_MODEL FOREIGN KEY
	(
	USER_CLASSIFIER_MODEL_ID
	) REFERENCES dbo.tb_CLASSIFIER_MODEL
	(
	MODEL_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_MAG_AUTO_UPDATE SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
GO

CREATE TABLE dbo.TB_MAG_AUTO_UPDATE_RUN
	(
	MAG_AUTO_UPDATE_RUN_ID int NOT NULL IDENTITY (1, 1),
	MAG_AUTO_UPDATE_ID int NULL,
	USER_DESCRIPTION nvarchar(1000) NULL,
	ATTRIBUTE_ID bigint NULL,
	ALL_INCLUDED bit NULL,
	STUDY_TYPE_CLASSIFIER nvarchar(50) NULL,
	USER_CLASSIFIER_MODEL_ID int NULL,
	USER_CLASSIFIER_REVIEW_ID int NULL,
	DATE_RUN datetime NULL,
	N_PAPERS int NULL,
	REVIEW_ID INT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TB_MAG_AUTO_UPDATE_RUN ADD CONSTRAINT
	PK_TB_MAG_AUTO_UPDATE_RUN PRIMARY KEY CLUSTERED 
	(
	MAG_AUTO_UPDATE_RUN_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
BEGIN TRANSACTION
GO
ALTER TABLE dbo.TB_MAG_AUTO_UPDATE_RUN ADD CONSTRAINT
	FK_TB_MAG_AUTO_UPDATE_RUN_tb_CLASSIFIER_MODEL FOREIGN KEY
	(
	USER_CLASSIFIER_MODEL_ID
	) REFERENCES dbo.tb_CLASSIFIER_MODEL
	(
	MODEL_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TB_MAG_AUTO_UPDATE_RUN SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
GO

CREATE TABLE dbo.TB_MAG_AUTO_UPDATE_RUN_PAPER
	(
	MAG_AUTO_UPDATE_RUN_PAPER_ID int NOT NULL IDENTITY (1, 1),
	MAG_AUTO_UPDATE_RUN_ID int NULL,
	REVIEW_ID int NULL,
	PaperId bigint NULL,
	ContReviewScore float(53) NULL,
	UserClassifierScore float(53) NULL,
	StudyTypeClassifierScore float(53) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TB_MAG_AUTO_UPDATE_RUN_PAPER ADD CONSTRAINT
	PK_TB_MAG_AUTO_UPDATE_RUN_PAPER PRIMARY KEY CLUSTERED 
	(
	MAG_AUTO_UPDATE_RUN_PAPER_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.TB_MAG_AUTO_UPDATE_RUN_PAPER ADD CONSTRAINT
	FK_MAG_AUTO_UPDATE_RUN FOREIGN KEY
	(
	MAG_AUTO_UPDATE_RUN_ID
	) REFERENCES dbo.TB_MAG_AUTO_UPDATE_RUN
	(
	MAG_AUTO_UPDATE_RUN_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
GO
ALTER TABLE dbo.TB_MAG_AUTO_UPDATE_RUN_PAPER ADD CONSTRAINT
	FK_REVIEW_ID FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
GO
ALTER TABLE dbo.TB_MAG_AUTO_UPDATE ADD CONSTRAINT
	FK_REVIEW_ID2 FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
GO
ALTER TABLE dbo.TB_MAG_AUTO_UPDATE_RUN ADD CONSTRAINT
	FK_REVIEW_ID3 FOREIGN KEY
	(
	REVIEW_ID
	) REFERENCES dbo.TB_REVIEW
	(
	REVIEW_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdates]    Script Date: 09/11/2020 19:35:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all runs for a given review
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagAutoUpdates] 
	-- Add the parameters for the stored procedure here	
	@REVIEW_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select mau.MAG_AUTO_UPDATE_ID, mau.USER_DESCRIPTION, mau.ATTRIBUTE_ID, mau.ALL_INCLUDED,
		mau.STUDY_TYPE_CLASSIFIER, mau.USER_CLASSIFIER_MODEL_ID, mau.USER_CLASSIFIER_REVIEW_ID,
		a.ATTRIBUTE_NAME from TB_MAG_AUTO_UPDATE mau
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mau.ATTRIBUTE_ID
		where mau.REVIEW_ID = @REVIEW_ID
		order by MAU.MAG_AUTO_UPDATE_ID
		
END
go
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRuns]    Script Date: 09/11/2020 19:35:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all runs for a given review
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagAutoUpdateRuns] 
	-- Add the parameters for the stored procedure here	
	@REVIEW_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select mau.MAG_AUTO_UPDATE_ID, mau.USER_DESCRIPTION, mau.ATTRIBUTE_ID, mau.ALL_INCLUDED,
		mau.STUDY_TYPE_CLASSIFIER, mau.USER_CLASSIFIER_MODEL_ID, mau.USER_CLASSIFIER_REVIEW_ID,
		a.ATTRIBUTE_NAME, mau.DATE_RUN, mau.N_PAPERS from TB_MAG_AUTO_UPDATE_RUN mau
		left outer join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = mau.ATTRIBUTE_ID
		where mau.REVIEW_ID = @REVIEW_ID
		order by MAU.MAG_AUTO_UPDATE_ID
		
END
go
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateDelete]    Script Date: 09/11/2020 20:05:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagAutoUpdateDelete] 
	-- Add the parameters for the stored procedure here
	@MAG_AUTO_UPDATE_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	delete from TB_MAG_AUTO_UPDATE
	where MAG_AUTO_UPDATE_ID = @MAG_AUTO_UPDATE_ID

END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateInsert]    Script Date: 09/11/2020 20:12:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagAutoUpdateInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int
,	@USER_DESCRIPTION NVARCHAR(1000)
,	@ATTRIBUTE_ID bigint = 0
,	@ALL_INCLUDED BIT
,	@STUDY_TYPE_CLASSIFIER NVARCHAR(50)
,	@USER_CLASSIFIER_MODEL_ID INT = NULL
,	@USER_CLASSIFIER_MODEL_REVIEW_ID INT = NULL
,	@MAG_AUTO_UPDATE_ID int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	insert into TB_MAG_AUTO_UPDATE(REVIEW_ID, USER_DESCRIPTION, ATTRIBUTE_ID,
		ALL_INCLUDED, STUDY_TYPE_CLASSIFIER, USER_CLASSIFIER_MODEL_ID, USER_CLASSIFIER_REVIEW_ID)
	values(@REVIEW_ID, @USER_DESCRIPTION, @ATTRIBUTE_ID,
		@ALL_INCLUDED, @STUDY_TYPE_CLASSIFIER, @USER_CLASSIFIER_MODEL_ID, @USER_CLASSIFIER_MODEL_REVIEW_ID)

	set @MAG_AUTO_UPDATE_ID = @@IDENTITY

END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagAutoUpdateRunDelete]    Script Date: 09/11/2020 20:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagAutoUpdateRunDelete] 
	-- Add the parameters for the stored procedure here
	@MAG_AUTO_UPDATE_RUN_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	delete from TB_MAG_AUTO_UPDATE_RUN_PAPER
		where MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID

	delete from TB_MAG_AUTO_UPDATE_RUN
		WHERE MAG_AUTO_UPDATE_RUN_ID = @MAG_AUTO_UPDATE_RUN_ID

END
GO