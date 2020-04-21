USE [Reviewer]
GO



IF COL_LENGTH('dbo.TB_MAG_LOG', 'TIME_UPDATED') IS NULL
BEGIN

BEGIN TRANSACTION

ALTER TABLE dbo.TB_MAG_LOG
	DROP CONSTRAINT FK_TB_MAG_LOG_TB_CONTACT

ALTER TABLE dbo.TB_CONTACT SET (LOCK_ESCALATION = TABLE)

COMMIT
BEGIN TRANSACTION

CREATE TABLE dbo.Tmp_TB_MAG_LOG
	(
	MAG_LOG_ID int NOT NULL IDENTITY (1, 1),
	TIME_SUBMITTED datetime NULL,
	TIME_UPDATED datetime NULL,
	CONTACT_ID int NULL,
	JOB_TYPE nvarchar(50) NULL,
	JOB_STATUS nvarchar(50) NULL,
	JOB_MESSAGE nvarchar(MAX) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_TB_MAG_LOG SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_TB_MAG_LOG ON

IF EXISTS(SELECT * FROM dbo.TB_MAG_LOG)
	 EXEC('INSERT INTO dbo.Tmp_TB_MAG_LOG (MAG_LOG_ID, TIME_SUBMITTED, CONTACT_ID, JOB_TYPE, JOB_STATUS, JOB_MESSAGE)
		SELECT MAG_LOG_ID, TIME_SUBMITTED, CONTACT_ID, JOB_TYPE, JOB_STATUS, JOB_MESSAGE FROM dbo.TB_MAG_LOG WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_TB_MAG_LOG OFF

DROP TABLE dbo.TB_MAG_LOG

EXECUTE sp_rename N'dbo.Tmp_TB_MAG_LOG', N'TB_MAG_LOG', 'OBJECT' 

ALTER TABLE dbo.TB_MAG_LOG ADD CONSTRAINT
	PK_TB_MAG_LOG PRIMARY KEY CLUSTERED 
	(
	MAG_LOG_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


ALTER TABLE dbo.TB_MAG_LOG ADD CONSTRAINT
	FK_TB_MAG_LOG_TB_CONTACT FOREIGN KEY
	(
	CONTACT_ID
	) REFERENCES dbo.TB_CONTACT
	(
	CONTACT_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
COMMIT
END







/****** Object:  StoredProcedure [dbo].[st_MagSimulationGetIds]    Script Date: 09/03/2020 17:48:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag papers 
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagSimulationGetIds] 
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int = 0
,	@ATTRIBUTE_ID_FILTER BIGINT = 0
,	@ATTRIBUTE_ID_SEED BIGINT = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here


	if not @ATTRIBUTE_ID_FILTER = 0
		
		select DISTINCT imm.PaperId, case when iat.item_id is null then 0 else 1 end as Training from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER
		inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
		left outer join TB_ITEM_ATTRIBUTE iat on iat.ITEM_ID = imm.ITEM_ID and iat.ATTRIBUTE_ID = @ATTRIBUTE_ID_SEED
		left outer join tb_item_set iset on iset.ITEM_SET_ID = iat.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
	else
		
		select DISTINCT imm.PaperId, case when iat.item_id is null then 0 else 1 end as Training from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
		inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
		left outer join TB_ITEM_ATTRIBUTE iat on iat.ITEM_ID = imm.ITEM_ID and iat.ATTRIBUTE_ID = @ATTRIBUTE_ID_SEED
		left outer join tb_item_set iset on iset.ITEM_SET_ID = iat.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
		where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsUpdatePostRun]    Script Date: 11/03/2020 17:41:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	After the related item ids are found, this updates the record for the UI list
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagRelatedPapersRunsUpdatePostRun] 
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
		USER_STATUS = 'Unchecked'
		where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagLogInsert]    Script Date: 16/03/2020 13:30:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_MAG_LOG
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagLogInsert] 
	
	@CONTACT_ID int,
	@JOB_TYPE nvarchar(50),
	@JOB_STATUS nvarchar(50),
	@JOB_MESSAGE nvarchar(max),
	@MAG_LOG_ID int OUTPUT
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO TB_MAG_LOG(TIME_SUBMITTED, CONTACT_ID, JOB_TYPE, JOB_STATUS, JOB_MESSAGE)
	VALUES(GETDATE(), @CONTACT_ID, @JOB_TYPE, @JOB_STATUS, @JOB_MESSAGE)
	
	SET @MAG_LOG_ID = @@IDENTITY
END
GO

USE [Reviewer]
/****** Object:  StoredProcedure [dbo].[st_MagLogUpdate]    Script Date: 11/03/2020 18:37:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Update record in tb_MAG_LOG
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_MagLogUpdate] 
	
	@MAG_LOG_ID int,
	@JOB_STATUS nvarchar(50),
	@JOB_MESSAGE nvarchar(max)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    UPDATE TB_MAG_LOG
		SET JOB_STATUS = @JOB_STATUS, JOB_MESSAGE = @JOB_MESSAGE, TIME_UPDATED = GETDATE()
		WHERE MAG_LOG_ID = @MAG_LOG_ID
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagLogList]    Script Date: 14/03/2020 10:13:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	List all mag update jobs
-- =============================================
CREATE OR ALTER     PROCEDURE [dbo].[st_MagLogList] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	select ML.CONTACT_ID, ML.JOB_MESSAGE, ML.JOB_STATUS, ML.JOB_TYPE, ML.MAG_LOG_ID, ML.TIME_SUBMITTED,
		TIME_UPDATED, C.CONTACT_NAME
	from TB_MAG_LOG ML
		INNER JOIN TB_CONTACT C ON C.CONTACT_ID = ML.CONTACT_ID
		order by ML.MAG_LOG_ID desc
		
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagContReviewRunGetSeedIds]    Script Date: 19/03/2020 13:34:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get seed MAG IDs for the Continuous Review run when a new MAG is available
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagContReviewRunGetSeedIds] 

AS
BEGIN
	SELECT imm.PaperId, mrr.MAG_RELATED_RUN_ID RelatedRunId, 1 as Included from tb_ITEM_MAG_MATCH imm
	inner join TB_ITEM_REVIEW ir on ir.REVIEW_ID = imm.REVIEW_ID and imm.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 'false'
	inner join TB_MAG_RELATED_RUN mrr on mrr.REVIEW_ID = ir.REVIEW_ID
	where (mrr.AUTO_RERUN = 'true' OR mrr.STATUS = 'Pending') and mrr.MODE = 'New items in MAG' and mrr.ATTRIBUTE_ID = 0 and
		(imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)

	UNION ALL

	SELECT imm.PaperId, mrr.MAG_RELATED_RUN_ID RelatedRunId, 1 as Included from tb_ITEM_MAG_MATCH imm
	inner join TB_ITEM_REVIEW ir on ir.REVIEW_ID = imm.REVIEW_ID and imm.ITEM_ID = ir.ITEM_ID and ir.IS_DELETED = 'false'
	inner join TB_MAG_RELATED_RUN mrr on mrr.REVIEW_ID = ir.REVIEW_ID
	inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = ir.ITEM_ID and ia.ATTRIBUTE_ID = mrr.ATTRIBUTE_ID
	inner join TB_ITEM_SET iset on iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
	where (mrr.AUTO_RERUN = 'true' OR mrr.STATUS = 'Pending') and mrr.MODE = 'New items in MAG' and not mrr.ATTRIBUTE_ID is null and
	(imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)		
END
GO

IF NOT COL_LENGTH('dbo.TB_MAG_RELATED_RUN', 'PARENT_MAG_RELATED_RUN_ID') IS NULL
BEGIN
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
	ALTER TABLE dbo.TB_MAG_RELATED_RUN
		DROP COLUMN PARENT_MAG_RELATED_RUN_ID

	ALTER TABLE dbo.TB_MAG_RELATED_RUN SET (LOCK_ESCALATION = TABLE)
	COMMIT
END
GO

IF NOT COL_LENGTH('dbo.TB_MAG_RELATED_PAPERS', 'PARENT_MAG_RELATED_RUN_ID') IS NULL
BEGIN
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
	ALTER TABLE dbo.TB_MAG_RELATED_PAPERS
		DROP COLUMN PARENT_MAG_RELATED_RUN_ID
	ALTER TABLE dbo.TB_MAG_RELATED_PAPERS SET (LOCK_ESCALATION = TABLE)
	COMMIT
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_MAG_RELATED_PAPERS_TEMP')
BEGIN
	CREATE TABLE [dbo].[TB_MAG_RELATED_PAPERS_TEMP](
	[MAG_RELATED_RUN_ID] [int] NOT NULL,
	[PaperId] [bigint] NOT NULL,
	[SimilarityScore] [float] NULL,
	[JobId] [nvarchar](50) NULL
	)
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagContReviewInsertResults]    Script Date: 27/03/2020 19:24:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Last stage in the ContReview workflow: put the 'found' papers in and update tb_MAG_RELATED_RUN
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_MagContReviewInsertResults] 
	-- Add the parameters for the stored procedure here
	@JobId nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO TB_MAG_RELATED_PAPERS(REVIEW_ID, MAG_RELATED_RUN_ID, PaperId, SimilarityScore)
		SELECT MRR.REVIEW_ID, MRPT.MAG_RELATED_RUN_ID, MRPT.PaperId, MRPT.SimilarityScore FROM TB_MAG_RELATED_PAPERS_TEMP MRPT
		INNER JOIN TB_MAG_RELATED_RUN MRR ON MRR.MAG_RELATED_RUN_ID = MRPT.MAG_RELATED_RUN_ID
		LEFT JOIN TB_MAG_RELATED_PAPERS MRP2 ON MRP2.REVIEW_ID = MRR.REVIEW_ID AND MRP2.PaperId = MRPT.PaperId
		WHERE MRP2.PaperId IS NULL and JobId = @JobId

	UPDATE MRR 
		SET MRR.N_PAPERS = idcount,
		[STATUS] = 'Complete',
		USER_STATUS = 'Unchecked'

		FROM TB_MAG_RELATED_RUN MRR
		INNER JOIN (SELECT MAG_RELATED_RUN_ID,COUNT(*) idcount FROM TB_MAG_RELATED_PAPERS GROUP BY MAG_RELATED_RUN_ID) as COUNTS
			ON COUNTS.MAG_RELATED_RUN_ID = MRR.MAG_RELATED_RUN_ID 

	DELETE FROM TB_MAG_RELATED_PAPERS_TEMP WHERE JobId = @JobId
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunsInsert]    Script Date: 28/03/2020 10:23:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Create new mag related run record
-- =============================================
create or ALTER     PROCEDURE [dbo].[st_MagRelatedPapersRunsInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int
,	@USER_DESCRIPTION NVARCHAR(1000)
,	@PaperIdList nvarchar(max)
,	@ATTRIBUTE_ID bigint = 0
,	@ALL_INCLUDED BIT
,	@DATE_FROM DATETIME
,	@AUTO_RERUN BIT
,	@MODE nvarchar(50)
,	@FILTERED NVARCHAR(50)
,	@MAG_RELATED_RUN_ID int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	insert into tb_MAG_RELATED_RUN(REVIEW_ID, USER_DESCRIPTION, PaperIdList, ATTRIBUTE_ID,
		ALL_INCLUDED, DATE_FROM, AUTO_RERUN, STATUS, USER_STATUS, MODE, Filtered, N_PAPERS)
	values(@REVIEW_ID, @USER_DESCRIPTION, @PaperIdList, @ATTRIBUTE_ID,
		@ALL_INCLUDED, @DATE_FROM, @AUTO_RERUN, 'Pending', 'Waiting', @MODE, @FILTERED, 0)

	set @MAG_RELATED_RUN_ID = @@IDENTITY

END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagRelatedPapersRunGetSeedIds]    Script Date: 28/03/2020 10:23:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Stage 1 in getting related papers: get the list of seed MAG IDs
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagRelatedPapersRunGetSeedIds] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @PaperIdList nvarchar(max) = ''
	declare @ATTRIBUTE_ID bigint = 0
	declare @ALL_INCLUDED BIT = 'false'
	declare @REVIEW_ID int
	declare @DATE_FROM datetime = null
	declare @STATUS nvarchar(50)
	declare @DATE_FROM_INT int = 0
	declare @MODE nvarchar(50)
	declare @FILTERED nvarchar(50)

	/*
	declare @SeedIds table
	(
		PaperId bigint INDEX idx CLUSTERED
	)
	*/

	select @PaperIdList = PaperIdList
		, @ATTRIBUTE_ID = ATTRIBUTE_ID
		, @ALL_INCLUDED = ALL_INCLUDED
		, @DATE_FROM = DATE_FROM
		, @STATUS = [STATUS]
		, @MODE = MODE
		, @FILTERED = Filtered
		, @REVIEW_ID = REVIEW_ID
	FROM Reviewer.dbo.tb_MAG_RELATED_RUN mrr where mrr.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID

	if @STATUS = 'Complete' -- Create a new row for this 'run'
	begin
		update Reviewer.dbo.tb_MAG_RELATED_RUN -- Set autoupdate to 'false' for the older one, or we'll get duplicate reruns
			set AUTO_RERUN = 'false'
			where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID

		insert into Reviewer.dbo.tb_MAG_RELATED_RUN(REVIEW_ID, USER_DESCRIPTION, PaperIdList, ATTRIBUTE_ID, ALL_INCLUDED,
			DATE_FROM, DATE_RUN, AUTO_RERUN, STATUS, MODE, Filtered, N_PAPERS)
		select REVIEW_ID, USER_DESCRIPTION, PaperIdList, ATTRIBUTE_ID, ALL_INCLUDED,
			DATE_FROM, GETDATE(), 'true', 'Running', MODE, FILTERED, 0 from Reviewer.dbo.tb_MAG_RELATED_RUN
				where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID

		set @MAG_RELATED_RUN_ID = @@IDENTITY
	end

	-- Create an in-memory table of all the TB_ITEM -> MAG matches that we're working from
	if (@PaperIdList = '')
	begin
		if not @ATTRIBUTE_ID = 0
			--insert into @SeedIds
			select imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
			inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = ia.ITEM_SET_ID and tis.IS_COMPLETED = 'true'
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = ia.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			group by imm.PaperId
		else
			--insert into @SeedIds
			select imm.PaperId from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID and ir.IS_DELETED = 'false' and ir.REVIEW_ID = @REVIEW_ID
			group by imm.PaperId
	end
	else
		--insert into @SeedIds
			select value from Dbo.fn_Split_int(@PaperIdList, ',') 

	--SELECT * FROM @SeedIds
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSimulationUpdatePostRun]    Script Date: 10/04/2020 16:10:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	After the related item ids are found, this updates the record for the UI list
-- =============================================
create or ALTER   PROCEDURE [dbo].[st_MagSimulationUpdatePostRun] 
	-- Add the parameters for the stored procedure here
	@MAG_SIMULATION_ID int,
	@REVIEW_ID int,
	@SeedIds int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	update sim
	SET INCLUDED = 'True'
	from TB_MAG_SIMULATION_RESULT sim
	inner join tb_ITEM_MAG_MATCH imm ON imm.PaperId = sim.PaperId
	where (imm.AutoMatchScore >= 0.7 or imm.ManualTrueMatch = 'true') and
		(imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null) and
		sim.MAG_SIMULATION_ID = @MAG_SIMULATION_ID and imm.REVIEW_ID = @REVIEW_ID

	declare @FN int = 0
	declare @FP int = 0

	select @FN = count(*) from TB_MAG_SIMULATION_RESULT msr
		inner join dbo.fn_Split_int(@SeedIds, ',') i on i.value = msr.PaperId
		where msr.MAG_SIMULATION_ID = @MAG_SIMULATION_ID

	select @FP = count(*) from TB_MAG_SIMULATION_RESULT msr
	left outer join dbo.fn_Split_int(@SeedIds, ',') i on i.value = msr.PaperId
		where msr.MAG_SIMULATION_ID = @MAG_SIMULATION_ID and i.value is null

	update TB_MAG_SIMULATION
		set STATUS = 'Complete',
		FP = @FP,
		FN = @FN,
		TP = (SELECT COUNT(value) from dbo.fn_Split_int(@SeedIds, ','))
		where MAG_SIMULATION_ID = @MAG_SIMULATION_ID
		
END

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagContReviewInsertResults]    Script Date: 20/04/2020 21:46:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Last stage in the ContReview workflow: put the 'found' papers in and update tb_MAG_RELATED_RUN
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_MagContReviewInsertResults] 
	-- Add the parameters for the stored procedure here
	@JobId nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO TB_MAG_RELATED_PAPERS(REVIEW_ID, MAG_RELATED_RUN_ID, PaperId, SimilarityScore)
		SELECT MRR.REVIEW_ID, MRPT.MAG_RELATED_RUN_ID, MRPT.PaperId, MRPT.SimilarityScore FROM TB_MAG_RELATED_PAPERS_TEMP MRPT
		INNER JOIN TB_MAG_RELATED_RUN MRR ON MRR.MAG_RELATED_RUN_ID = MRPT.MAG_RELATED_RUN_ID
		LEFT JOIN TB_MAG_RELATED_PAPERS MRP2 ON MRP2.REVIEW_ID = MRR.REVIEW_ID AND MRP2.PaperId = MRPT.PaperId
		WHERE MRP2.PaperId IS NULL and JobId = @JobId

	UPDATE MRR 
		SET MRR.N_PAPERS = idcount,
		[STATUS] = 'Complete',
		USER_STATUS = 'Unchecked'
		FROM TB_MAG_RELATED_RUN MRR
		INNER JOIN (SELECT MAG_RELATED_RUN_ID,COUNT(*) idcount FROM TB_MAG_RELATED_PAPERS GROUP BY MAG_RELATED_RUN_ID) as COUNTS
			ON COUNTS.MAG_RELATED_RUN_ID = MRR.MAG_RELATED_RUN_ID

	UPDATE TB_MAG_RELATED_RUN
		set N_PAPERS = 0,
		[STATUS] = 'Complete',
		USER_STATUS = 'Unchecked'
		WHERE [STATUS]= 'Pending'

	DELETE FROM TB_MAG_RELATED_PAPERS_TEMP WHERE JobId = @JobId
END

GO