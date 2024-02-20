--this script reverts changes made in EPPI-Vis_World_map.sql, reverting only AS LITTLE AS POSSIBLE, so just enough
--to ensure that EPPI Vis and ER can run without errors when one leaves the EPPI-Vis_World_map branch, after having (manually) applied the SQL changes in the dedicated file
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDBget]    Script Date: 06/02/2024 14:02:37 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE or ALTER       procedure [dbo].[st_WebDBget]
(
	@WebDBid int,
	@RevId int	
)

As
BEGIN
select [WEBDB_ID]
		  ,[REVIEW_ID]
		  ,[WITH_ATTRIBUTE_ID]
		  ,[IS_OPEN]
		  ,w.[USERNAME]
		  ,[WEBDB_NAME]
		  ,SUBTITLE
		  ,w.[DESCRIPTION]
		  ,c1.CONTACT_NAME as [CREATED_BY]
		  ,c2.CONTACT_NAME as [EDITED_BY]
		  ,w.[MAP_TITLE]
		  ,w.[MAP_URL]
		  ,w.[HEADER_IMAGE_1_URL]
		  ,w.[HEADER_IMAGE_2_URL]
		  ,w.[HEADER_IMAGE_3_URL]
		  ,w.HEADER_IMAGE_1
		  ,w.HEADER_IMAGE_2
		  ,w.HEADER_IMAGE_3
		  --,[WORLD_MAP]
	  FROM [TB_WEBDB] w
	  inner join TB_CONTACT c1 on w.CREATED_BY = c1.CONTACT_ID
	  inner join TB_CONTACT c2 on w.EDITED_BY = c2.CONTACT_ID 
	  where w.WEBDB_ID = @WebDBid AND w.REVIEW_ID = @RevId
END
GO

