USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsCochrane]    Script Date: 02/04/2024 11:11:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE or ALTER   PROCEDURE [dbo].[st_ReviewDetailsCochrane] 
(
	@REVIEW_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here	
	select r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, r.REVIEW_NUMBER, 
	r.DATE_CREATED, r.OLD_REVIEW_GROUP_ID,
		CASE when l.[EXPIRY_DATE] is not null 
		and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
			then l.[EXPIRY_DATE]
		else r.[EXPIRY_DATE]
		end as 'EXPIRY_DATE', 
	r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
	l.SITE_LIC_ID, l.SITE_LIC_NAME, r.SHOW_SCREENING, r.ALLOW_REVIEWER_TERMS, r.ALLOW_CLUSTERED_SEARCH,
	r.ARCHIE_ID, r.ARCHIE_CD, r.IS_CHECKEDOUT_HERE, r.CHECKED_OUT_BY, r.MAG_ENABLED, r.OPEN_AI_ENABLED
	from sTB_REVIEW r
	inner join sTB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
	left join sTB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
	left join sTB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
	where r.REVIEW_ID = @REVIEW_ID
	
	group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
	r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
	l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME, r.REVIEW_NUMBER, r.OLD_REVIEW_GROUP_ID,
	r.SHOW_SCREENING, r.ALLOW_REVIEWER_TERMS, r.ALLOW_CLUSTERED_SEARCH,
	r.ARCHIE_ID, r.ARCHIE_CD, r.IS_CHECKEDOUT_HERE, r.CHECKED_OUT_BY, r.MAG_ENABLED, r.OPEN_AI_ENABLED
	order by r.REVIEW_NAME

	RETURN
END

GO

---------------------------------------------------------------------------------------------

USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_EnableOpenAI]    Script Date: 02/04/2024 11:41:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE or ALTER     procedure [dbo].[st_EnableOpenAI]
(
	@REVIEW_ID int,
	@SETTING bit
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	
	update sTB_REVIEW
	set OPEN_AI_ENABLED = @SETTING
	where REVIEW_ID = @REVIEW_ID
	

END

GO

-----------------------------------------------------------------------

