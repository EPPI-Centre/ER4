USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_WebDbDelete]    Script Date: 08/07/2026 13:54:43 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER   PROCEDURE [dbo].[st_WebDbDelete]
(
	@REVIEW_ID INT,
	@WEBDB_ID int
)
As
--basic check: does this thing exist?
if (SELECT count(*) from TB_WEBDB where WEBDB_ID = @WEBDB_ID and @REVIEW_ID = REVIEW_ID) != 1 return

-- updated by Jeff (08/07/2026) to delete any maps in the WebDb before deleting the visualisation
delete from TB_WEBDB_MAP where WEBDB_ID = @WEBDB_ID


--delete attributes
delete from TB_WEBDB_PUBLIC_ATTRIBUTE where WEBDB_ID = @WEBDB_ID 
--delete Sets
delete from TB_WEBDB_PUBLIC_SET where WEBDB_ID = @WEBDB_ID
--delete webdb
delete from TB_WEBDB where WEBDB_ID = @WEBDB_ID

GO

