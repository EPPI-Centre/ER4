USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbListGet]    Script Date: 22/02/2021 15:46:47 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_WebDbListGet]
(
	@RevId INT,
	@ContactId int
)
As
BEGIN
	declare @check int = -1
	--check is this user in the review? (or a site_admin?)
	set @check = (select count(*) from TB_REVIEW_CONTACT rc
					inner join TB_CONTACT c on (rc.CONTACT_ID = @ContactId and rc.CONTACT_ID = c.CONTACT_ID and REVIEW_ID = @RevId)
												OR (c.CONTACT_ID = @ContactId and c.IS_SITE_ADMIN = 1)
				 )

	if @check < 1 return
	
	SELECT [WEBDB_ID]
		  ,[REVIEW_ID]
		  ,[WITH_ATTRIBUTE_ID]
		  ,[IS_OPEN]
		  ,w.[USERNAME]
		  
		  ,[WEBDB_NAME]
		  ,w.[DESCRIPTION]
		  ,c1.CONTACT_NAME as [CREATED_BY]
		  ,c2.CONTACT_NAME as [EDITED_BY]
		  , w.HEADER_IMAGE_1
		  , w.HEADER_IMAGE_2
		  , w.MAP_TITLE
		  , w.MAP_URL
	  FROM [TB_WEBDB] w
	  inner join TB_CONTACT c1 on w.CREATED_BY = c1.CONTACT_ID
	  inner join TB_CONTACT c2 on w.EDITED_BY = c2.CONTACT_ID
	  where REVIEW_ID = @RevId
END
GO
