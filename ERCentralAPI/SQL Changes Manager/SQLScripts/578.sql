USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDBget]    Script Date: 01/03/2024 11:49:52 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER       procedure [dbo].[st_WebDBget]
(
	@WebDBid int,
	@RevId int	
)

As
BEGIN
select w.*
		  ,c1.CONTACT_NAME as [CREATED_BY]
		  ,c2.CONTACT_NAME as [EDITED_BY]
	  FROM [TB_WEBDB] w
	  inner join TB_CONTACT c1 on w.CREATED_BY = c1.CONTACT_ID
	  inner join TB_CONTACT c2 on w.EDITED_BY = c2.CONTACT_ID 
	  where w.WEBDB_ID = @WebDBid AND w.REVIEW_ID = @RevId
END

GO