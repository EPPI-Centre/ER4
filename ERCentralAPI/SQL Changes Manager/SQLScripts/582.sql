USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbCodeSetDelete]    Script Date: 4/19/2024 2:27:52 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_WebDbCodeSetDelete]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@Set_ID int
)
As
declare @r_set_id int = (select review_set_id from TB_WEBDB w
						inner join TB_REVIEW_SET rs on rs.SET_ID = @Set_ID and rs.REVIEW_ID = @REVIEW_ID and w.REVIEW_ID = rs.REVIEW_ID
						where w.WEBDB_ID = @WEBDB_ID and w.REVIEW_ID = @REVIEW_ID)
--Just a basic sanity check: can we get a REVIEW_SET_ID?
IF @r_set_id is null OR @r_set_id < 1 return

Declare @atts table (A_ID bigint primary key)
INSERT into @atts select distinct tas.Attribute_id from TB_WEBDB w
	inner join TB_WEBDB_PUBLIC_ATTRIBUTE wa on w.REVIEW_ID = @REVIEW_ID and w.WEBDB_ID = @WEBDB_ID and wa.WEBDB_ID = @WEBDB_ID
	inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = wa.ATTRIBUTE_ID and tas.SET_ID = @Set_ID
	inner join TB_REVIEW_SET rs on tas.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID and rs.REVIEW_SET_ID = @r_set_id

DELETE from TB_WEBDB_PUBLIC_ATTRIBUTE 
	where ATTRIBUTE_ID in (select * from @atts)
	and WEBDB_ID = @WEBDB_ID
DELETE from TB_WEBDB_PUBLIC_SET where WEBDB_ID = @WEBDB_ID and REVIEW_SET_ID = @r_set_id

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_WebDbAttributeDelete]    Script Date: 4/19/2024 2:27:58 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER   PROCEDURE [dbo].[st_WebDbAttributeDelete]
(
	@ATTRIBUTE_ID bigint,
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@Set_ID int
)
As
declare @r_set_id int = (select review_set_id from TB_WEBDB w
						inner join TB_REVIEW_SET rs on rs.SET_ID = @Set_ID and rs.REVIEW_ID = @REVIEW_ID and w.REVIEW_ID = rs.REVIEW_ID
						where w.WEBDB_ID = @WEBDB_ID and w.REVIEW_ID = @REVIEW_ID)
--Just a basic sanity check: can we get a REVIEW_SET_ID?
IF @r_set_id is null OR @r_set_id < 1 return

--select @r_set_id

declare @dels table (d_id bigint primary key)
Declare @atts table (A_ID bigint primary key) 
declare @rows int = 1
declare @count int = 0

INSERT into @atts select distinct tas.Attribute_id from TB_WEBDB w
	inner join TB_WEBDB_PUBLIC_ATTRIBUTE wa on w.REVIEW_ID = @REVIEW_ID and w.WEBDB_ID = @WEBDB_ID and wa.WEBDB_ID = @WEBDB_ID
	inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = wa.ATTRIBUTE_ID and tas.SET_ID = @Set_ID
	inner join TB_REVIEW_SET rs on tas.SET_ID = rs.SET_ID and rs.REVIEW_ID = @REVIEW_ID and rs.REVIEW_SET_ID = @r_set_id
--another check: is this Attribute inside the @atts table?
--select * from @atts order by A_ID
--select count(*) as c from @atts where A_ID = @ATTRIBUTE_ID

IF (select count(*) from @atts where A_ID = @ATTRIBUTE_ID) < 1 return
insert into @dels (d_id) values (@ATTRIBUTE_ID)

--limited recursion here: we want to remove all children of the code we're taking out
--500 rounds max: just making sure this can't run forever... Each round should handle one nesting level so in theory this works for trees that are 500 levels deep
while @rows > 0 and @count < 500 
BEGIN
	set @count = @count +1
	insert into @dels 
		SELECT attribute_id from @atts a
		inner join TB_ATTRIBUTE_SET tas on tas.ATTRIBUTE_ID = a.A_ID and tas.SET_ID = @Set_ID
		inner join TB_REVIEW_SET rs on tas.SET_ID = rs.SET_ID and rs.REVIEW_SET_ID = @r_set_id
		where tas.PARENT_ATTRIBUTE_ID in (select d_id from @dels) 
			AND A.A_ID not in (select d_id from @dels)--do not insert the same att twice
	set @rows = @@ROWCOUNT
END
DELETE from TB_WEBDB_PUBLIC_ATTRIBUTE 
	where ATTRIBUTE_ID in (select d_id from @dels)
	AND WEBDB_ID = @WEBDB_ID

GO