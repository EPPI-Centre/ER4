--Sergio get by ReferenceID list (not pubmed IDs)
USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_findCitationsByExternalIDs]    Script Date: 02/07/2018 11:58:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[st_findCitationsByReferenceIDs]
(
	@RefIDs nvarchar(max)
)

As

SET NOCOUNT ON
declare @t table (RefId bigint primary key)

insert into @t (RefId) select distinct CAST([value] as bigint) from dbo.fn_Split(@RefIDs, '¬')

if (@@ROWCOUNT > 0)
BEGIN
	select *
	, rt.[TYPE_NAME]
	, dbo.fn_REBUILD_AUTHORS(REFERENCE_ID, 0) as AUTHORS
	, dbo.fn_REBUILD_AUTHORS(REFERENCE_ID, 1) as PARENT_AUTHORS
	, dbo.fn_REBUILD_EXTERNAL_IDS(REFERENCE_ID) as EXTERNAL_IDS
	 from TB_REFERENCE r
	 inner join TB_REFERENCE_TYPE rt on r.TYPE_ID = rt.TYPE_ID
	 inner join @t t on r.REFERENCE_ID = t.RefId
END

SET NOCOUNT OFF
GO


-- PATRICK SQL CHANGES
-- 2 SPs altered 02/07/2018

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_RCT_GET_LATEST_YEARLY_FILE_NAMES]    Script Date: 02/07/2018 12:35:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_RCT_GET_LATEST_YEARLY_FILE_NAMES]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT [RCT_FILE_NAME], [TB_RCT_UPDATE_FILE].RCT_UPLOAD_DATE
	FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
	WHERE [RCT_FILE_NAME] LIKE '%gz%'
	ORDER BY [RCT_UPLOAD_DATE] DESC
 
	SET NOCOUNT OFF
END


GO
/****** Object:  StoredProcedure [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]    Script Date: 02/07/2018 12:35:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP (1) [RCT_FILE_NAME], [TB_RCT_UPDATE_FILE].RCT_UPLOAD_DATE
	FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
	WHERE [RCT_FILE_NAME] NOT LIKE '%gz%'
	ORDER BY [RCT_UPLOAD_DATE] DESC

 
	SET NOCOUNT OFF
END


-- PATRICK SQL Changes 03/07/2018
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
GO
ALTER TABLE dbo.TB_REFERENCE ADD
	HUMAN_SCORE float(53) NULL
GO
ALTER TABLE dbo.TB_REFERENCE SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_ReferenceUpdate_Arrow_Scores]    Script Date: 03/07/2018 11:13:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ReferenceUpdate_Arrow_Scores]
	-- Add the parameters for the stored procedure here
	(
		@IDS NVARCHAR(MAX) = NULL,
		@SCORES NVARCHAR(MAX) = NULL,
		@ID int = NULL
	)
AS
BEGIN

	SET NOCOUNT ON;

	declare @t table (tidx int, [value] nvarchar(100), score float null, ref_id bigint, primary key(value, ref_id))
	insert into @t 
	select t.*, null, -1 from dbo.fn_Split(@IDS, ',') t 
				
	update @t set score = s.value
	from dbo.fn_Split(@SCORES, ',') s
	where tidx = s.idx
	update @t set ref_id = reference_id from
	 TB_EXTERNALID e inner join @t t on e.TYPE = 'pubmed' and t.VALUE = e.[value]
  
	--select * from @t
	--delete from @t
	--select r.* from @t t inner join TB_EXTERNALID ext on ext.TYPE = 'pubmed' AND ext.VALUE = t.value
	--inner join TB_REFERENCE r on ext.REFERENCE_ID = r.REFERENCE_ID
	

	-- FROM here need to update the references table in the appropriate place
	IF @ID = 'RCT'
			UPDATE [dbo].[TB_REFERENCE]  
			SET ARROW_SCORE = t.score
			FROM @t t INNER JOIN [dbo].[TB_REFERENCE] R 
			ON t.ref_id = R.REFERENCE_ID
	ELSE
			UPDATE [dbo].[TB_REFERENCE]  
			SET HUMAN_SCORE = t.score
			FROM @t t INNER JOIN [dbo].[TB_REFERENCE] R 
			ON t.ref_id = R.REFERENCE_ID

	SET NOCOUNT OFF
END
GO

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]    Script Date: 03/07/2018 13:21:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP (1) [RCT_FILE_NAME], [TB_RCT_UPDATE_FILE].RCT_UPLOAD_DATE
	FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
	WHERE [RCT_FILE_NAME] NOT LIKE '%gz%' AND [RCT_FILE_NAME] LIKE '%rct%'
	ORDER BY [RCT_UPLOAD_DATE] DESC

 
	SET NOCOUNT OFF
END

GO

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]    Script Date: 03/07/2018 13:21:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_HUMAN_GET_LATEST_UPLOAD_FILE_NAME]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP (1) [RCT_FILE_NAME], [TB_RCT_UPDATE_FILE].RCT_UPLOAD_DATE
	FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
	WHERE [RCT_FILE_NAME] NOT LIKE '%gz%' AND [RCT_FILE_NAME] LIKE '%tagger%'
	ORDER BY [RCT_UPLOAD_DATE] DESC

 
	SET NOCOUNT OFF
END

GO

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.RCT_UPDATE_FILE_ID', N'Tmp_UPDATE_FILE_ID', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.RCT_FILE_NAME', N'Tmp_FILE_NAME_1', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.RCT_IMPORT_DATE', N'Tmp_IMPORT_DATE_2', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.RCT_UPLOAD_DATE', N'Tmp_UPLOAD_DATE_3', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.Tmp_UPDATE_FILE_ID', N'UPDATE_FILE_ID', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.Tmp_FILE_NAME_1', N'FILE_NAME', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.Tmp_IMPORT_DATE_2', N'IMPORT_DATE', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.TB_RCT_UPDATE_FILE.Tmp_UPLOAD_DATE_3', N'UPLOAD_DATE', 'COLUMN' 
GO
ALTER TABLE dbo.TB_RCT_UPDATE_FILE SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

GO

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]    Script Date: 03/07/2018 13:28:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP (1) [FILE_NAME], [TB_RCT_UPDATE_FILE].UPLOAD_DATE
	FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
	WHERE [FILE_NAME] NOT LIKE '%gz%' AND [FILE_NAME] LIKE '%rct%'
	ORDER BY [UPLOAD_DATE] DESC

 
	SET NOCOUNT OFF
END

GO


USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_RCT_IMPORT_UPDATE_INSERT]    Script Date: 03/07/2018 13:28:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_RCT_IMPORT_UPDATE_INSERT]
	-- Add the parameters for the stored procedure here
	(
		@RCT_FILE_NAME varchar(max),
		@RCT_IMPORT_DATE datetime,
		@RCT_UPLOAD_DATE datetime
	)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

		INSERT INTO [dbo].[TB_RCT_UPDATE_FILE]
		([FILE_NAME], IMPORT_DATE, UPLOAD_DATE)
		VALUES (@RCT_FILE_NAME, @RCT_IMPORT_DATE, @RCT_UPLOAD_DATE )
 
	SET NOCOUNT OFF
END

GO

USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_HUMAN_GET_LATEST_UPLOAD_FILE_NAME]    Script Date: 03/07/2018 13:30:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_HUMAN_GET_LATEST_UPLOAD_FILE_NAME]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP (1) [FILE_NAME], [TB_RCT_UPDATE_FILE].UPLOAD_DATE
	FROM [DataService].[dbo].[TB_RCT_UPDATE_FILE]
	WHERE [FILE_NAME] NOT LIKE '%gz%' AND [FILE_NAME] LIKE '%tagger%'
	ORDER BY [UPLOAD_DATE] DESC

 
	SET NOCOUNT OFF
END










