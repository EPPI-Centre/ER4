USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Details_1]    Script Date: 2/4/2019 10:03:12 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_Site_Lic_Get_Details_1] 
(
	@admin_ID int,
	@site_license_id int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- first results set: currently active license TB_SITE_LIC_DETAILS.VALID_FROM is something and FOR_SALE.IS_ACTIVE = false
	SELECT TOP 1 *, c.CONTACT_NAME as ADMIN_NAME, ld.DATE_CREATED as DATE_PACKAGE_CREATED, 1 as SITE_LIC_MODEL from Reviewer.dbo.TB_SITE_LIC sl 
		inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
		inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
		inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and CONTACT_ID = @admin_ID
		inner join Reviewer.dbo.tb_CONTACT c on c.CONTACT_ID = @admin_ID
		where sl.SITE_LIC_ID = @site_license_id
	Order by ld.VALID_FROM desc	
	-- second results set: currently active renewal offer TB_SITE_LIC_DETAILS.VALID_FROM is NULL and FOR_SALE.IS_ACTIVE = True
	SELECT TOP 1 *, c.CONTACT_NAME as ADMIN_NAME, ld.DATE_CREATED as DATE_PACKAGE_CREATED, 1 as SITE_LIC_MODEL from Reviewer.dbo.TB_SITE_LIC sl 
		inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is null
		inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 1
		inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and CONTACT_ID = @admin_ID
		inner join Reviewer.dbo.tb_CONTACT c on c.CONTACT_ID = @admin_ID
		where sl.SITE_LIC_ID = @site_license_id
	Order by ld.VALID_FROM desc
END



GO

------------------------------------------------------------------------------------------------

-- update the WoS import filter (6). Only read the PY field for the date information
update [Reviewer].[dbo].[TB_IMPORT_FILTER]
set DATE = 'PY' where IMPORT_FILTER_ID = 6


----------------------------------------------------------------------------------------------


