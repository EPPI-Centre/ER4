USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbCodeSetEdit]    Script Date: 23/03/2021 17:47:27 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_WebDbCodeSetEdit]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@Set_ID int,
	@Public_Name nvarchar(255),
	@Public_Descr nvarchar(2000)
)
As
declare @r_set_id int = (select review_set_id from TB_WEBDB w
						inner join TB_REVIEW_SET rs on rs.SET_ID = @Set_ID and rs.REVIEW_ID = @REVIEW_ID and w.REVIEW_ID = rs.REVIEW_ID
						where w.WEBDB_ID = @WEBDB_ID and w.REVIEW_ID = @REVIEW_ID)
--Just a basic sanity check: can we get a REVIEW_SET_ID?
IF @r_set_id is null OR @r_set_id < 1 return
update TB_WEBDB_PUBLIC_SET set WEBDB_SET_NAME = @Public_Name, WEBDB_SET_DESCRIPTION = @Public_Descr 
 where REVIEW_SET_ID = @r_set_id and WEBDB_ID = @WEBDB_ID
 GO