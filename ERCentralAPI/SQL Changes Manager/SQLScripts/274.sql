-- edit 3 procedures so the data they return is ordered by REVIEW_ID desc
-- st_ContactReviewsOtherShareable
-- st_ContactReviewsNonShareable
-- st_ContactReviewsShareable



USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ContactReviewsOtherShareable]    Script Date: 18/11/2020 10:02:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactReviewsOtherShareable] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
	CREATE table #REVIEWS
	(
		REVIEW_ID int,
		REVIEW_NAME nvarchar(1000),
		REVIEW_OWNER nvarchar(255),
		DATE_REVIEW_CREATED datetime,
		LAST_ACCESSED_BY_CONTACT datetime
	)

	insert into #REVIEWS (REVIEW_ID, REVIEW_NAME, REVIEW_OWNER, DATE_REVIEW_CREATED)
	select r.REVIEW_ID, r.REVIEW_NAME, c.CONTACT_NAME as REVIEW_OWNER,
	r.DATE_CREATED as DATE_REVIEW_CREATED
	from Reviewer.dbo.TB_REVIEW r
	inner join Reviewer.dbo.TB_REVIEW_CONTACT rc
	on r.REVIEW_ID = rc.REVIEW_ID
	inner join Reviewer.dbo.TB_CONTACT c
	on c.CONTACT_ID = r.FUNDER_ID
	--restrict to the anyone but this user
	and r.FUNDER_ID != @CONTACT_ID
	--restrict to shareable reviews
	and r.EXPIRY_DATE is not null
	and rc.CONTACT_ID = @CONTACT_ID
	
	update #REVIEWS 
	set #REVIEWS.LAST_ACCESSED_BY_CONTACT = 
	(select max(lt.CREATED) as LAST_LOGIN from TB_LOGON_TICKET lt
	where lt.CONTACT_ID = @CONTACT_ID
	and lt.REVIEW_ID = #REVIEWS.REVIEW_ID)
	
	select * from #REVIEWS order by REVIEW_ID desc
	
	drop table #REVIEWS
    
    
END
GO



-----------------------------------------------


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ContactReviewsNonShareable]    Script Date: 18/11/2020 10:02:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactReviewsNonShareable] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
	select r.REVIEW_ID, r.REVIEW_NAME, 
	r.DATE_CREATED, max(lt.CREATED) as CREATED, 
	r.MONTHS_CREDIT
	from Reviewer.dbo.TB_REVIEW r
	inner join Reviewer.dbo.TB_REVIEW_CONTACT rc
	on r.REVIEW_ID = rc.REVIEW_ID
	left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
	on lt.REVIEW_ID = rc.REVIEW_ID
	where rc.CONTACT_ID = @CONTACT_ID
	-- this next line restricts it to just the contactID. Remove this line and you get the last login overall (all reviewers)
	and (lt.CONTACT_ID = @CONTACT_ID or lt.CONTACT_ID is null)
	--restrict to shareable reviews
	and r.EXPIRY_DATE is null
	-- ignore those with ArchieID
	and r.ARCHIE_ID is null
	
	group by r.REVIEW_ID, r.REVIEW_NAME, 
	r.DATE_CREATED, r.MONTHS_CREDIT
	order by r.REVIEW_ID desc

END
GO



---------------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ContactReviewsShareable]    Script Date: 18/11/2020 10:03:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactReviewsShareable] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
    declare @PURCHASED_REVIEWS table ( 
	REVIEW_ID int, 
	REVIEW_NAME nchar(1000),
	DATE_CREATED datetime,
	[EXPIRY_DATE] date,
	MONTHS_CREDIT smallint,
	FUNDER_ID int, 
	LAST_LOGIN datetime,
	SITE_LIC_ID int null,
	SITE_LIC_NAME nvarchar(50) null)

/*
Insert Into @PURCHASED_REVIEWS (REVIEW_ID, REVIEW_NAME, DATE_CREATED, [EXPIRY_DATE],
				MONTHS_CREDIT, FUNDER_ID)
SELECT [REVIEW_ID], [REVIEW_NAME], [DATE_CREATED], [EXPIRY_DATE], [MONTHS_CREDIT], [FUNDER_ID]
  FROM [Reviewer].[dbo].[TB_REVIEW] where FUNDER_ID = @CONTACT_ID
  and ((EXPIRY_DATE is null and MONTHS_CREDIT != 0)
		or
		(EXPIRY_DATE is not null))
*/

Insert Into @PURCHASED_REVIEWS (REVIEW_ID, REVIEW_NAME, DATE_CREATED, [EXPIRY_DATE],
				MONTHS_CREDIT, FUNDER_ID)
SELECT distinct r.[REVIEW_ID], r.[REVIEW_NAME], r.[DATE_CREATED], r.[EXPIRY_DATE], 
r.[MONTHS_CREDIT], r.[FUNDER_ID]
  FROM [Reviewer].[dbo].[TB_REVIEW] r
left outer join [Reviewer].[dbo].[TB_REVIEW_CONTACT] r_c
on r.REVIEW_ID = r_c.REVIEW_ID
left outer join [Reviewer].[dbo].[TB_CONTACT_REVIEW_ROLE] c_r_r
on r_c.REVIEW_CONTACT_ID = c_r_r.REVIEW_CONTACT_ID
and ((r_c.CONTACT_ID = @CONTACT_ID and c_r_r.ROLE_NAME = 'AdminUser') 
		or (r.FUNDER_ID = @CONTACT_ID))
where ((r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0 )
		or (r.EXPIRY_DATE is not null)) 
		and 
		(r.FUNDER_ID = @CONTACT_ID or (c_r_r.ROLE_NAME is not null and ROLE_NAME = 'AdminUser')) 

update  p_r
set p_r.LAST_LOGIN = 
l_t.CREATED
--max(l_t.CREATED)
from @PURCHASED_REVIEWS p_r, TB_LOGON_TICKET l_t
where l_t.CREATED in
(select max(l_t.CREATED)from TB_LOGON_TICKET l_t
where p_r.FUNDER_ID = l_t.CONTACT_ID
and p_r.REVIEW_ID = l_t.REVIEW_ID)

update @PURCHASED_REVIEWS set SITE_LIC_ID = a.site_lic_id, SITE_LIC_NAME = a.SITE_LIC_NAME
	, [EXPIRY_DATE] = case when L_EXP is not null and [EXPIRY_DATE] > L_EXP  then [EXPIRY_DATE] 
						else L_EXP
					end
from (select ld.SITE_LIC_ID, l.SITE_LIC_NAME, lr.REVIEW_ID as REV_ID , l.[EXPIRY_DATE] as L_EXP
	from TB_SITE_LIC_DETAILS ld inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on ld.SITE_LIC_ID = lr.SITE_LIC_ID
	inner join @PURCHASED_REVIEWS pr on pr.REVIEW_ID = lr.REVIEW_ID
	inner join Reviewer.dbo.TB_SITE_LIC l on lr.SITE_LIC_ID = l.SITE_LIC_ID) as a
where a.REV_ID = REVIEW_ID
select * from @PURCHASED_REVIEWS
order by REVIEW_ID desc
    
END


---------------------------------------------


GO



---------------------------------------------


