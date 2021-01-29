Use Reviewer

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_MAG_NEW_PAPERS')
BEGIN
CREATE TABLE dbo.TB_MAG_NEW_PAPERS
	(
	MAG_NEW_PAPERS_ID int NOT NULL IDENTITY (1, 1),
	PaperId bigint NULL,
	MagVersion nvarchar(20) NULL
	)  ON [PRIMARY]
ALTER TABLE dbo.TB_MAG_NEW_PAPERS ADD CONSTRAINT
	PK_TB_MAG_NEW_PAPERS PRIMARY KEY CLUSTERED 
	(
	MAG_NEW_PAPERS_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.TB_MAG_NEW_PAPERS SET (LOCK_ESCALATION = TABLE)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20210128-181759] ON [dbo].[TB_MAG_NEW_PAPERS]
(
	[PaperId] ASC
)
INCLUDE([MagVersion]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagSearchFilterToLatest]    Script Date: 29/01/2021 09:17:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Filter PaperIds to the last deployed set from MAG
-- =============================================
create or alter PROCEDURE [dbo].[st_MagSearchFilterToLatest] 
	-- Add the parameters for the stored procedure here
	@MagVersion nvarchar(20)
,	@IDs nvarchar(max)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select mnp.PaperId
	from TB_MAG_NEW_PAPERS mnp
	inner join dbo.fn_Split_int(@IDs, ',') s on s.value = mnp.PaperId

END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagCheckNewPapersAlreadyDownloaded]    Script Date: 29/01/2021 15:13:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Get the last row in the table to check MagVersion
-- =============================================
create or alter PROCEDURE [dbo].[st_MagCheckNewPapersAlreadyDownloaded] 
	-- Add the parameters for the stored procedure here

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select top(1) MagVersion
	from TB_MAG_NEW_PAPERS order by MAG_NEW_PAPERS_ID desc
	
END
GO