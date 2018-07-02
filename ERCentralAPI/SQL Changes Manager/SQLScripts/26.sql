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

