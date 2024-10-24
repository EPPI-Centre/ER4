USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_All]    Script Date: 27/09/2019 13:29:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_Site_Lic_Get_All] 
(
	@TEXT_BOX nvarchar(255)
)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 

	DECLARE @Site_Licenses TABLE
	(
		SITE_LIC_ID int,
		SITE_LIC_NAME nvarchar(50),
		CONTACT_ID int,
		CONTACT_NAME nvarchar(500),
		COMPANY_NAME nvarchar(50),
		[EXPIRY_DATE] date	
	)
	DECLARE @site_lic_id int
	DECLARE @first_contact_id int
	DECLARE @counter int
	set @counter = 0

	INSERT INTO @Site_Licenses (SITE_LIC_ID, SITE_LIC_NAME, COMPANY_NAME, [EXPIRY_DATE])	
	select s_l.SITE_LIC_ID, s_l.SITE_LIC_NAME, s_l.COMPANY_NAME, s_l.EXPIRY_DATE 
	from Reviewer.dbo.TB_SITE_LIC s_l

	DECLARE site_id CURSOR FOR 
	SELECT SITE_LIC_ID from @Site_Licenses 
	OPEN site_id
	FETCH NEXT FROM site_id INTO @site_lic_id

	WHILE @@FETCH_STATUS = 0
	BEGIN	
		DECLARE contact_id CURSOR FOR 
		SELECT CONTACT_ID from TB_SITE_LIC_ADMIN sla
		where sla.SITE_LIC_ID = @site_lic_id
		OPEN contact_id
		FETCH NEXT FROM contact_id INTO @first_contact_id
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if @counter = 0
			begin
				update @Site_Licenses
				set CONTACT_ID = @first_contact_id
				where SITE_LIC_ID = @site_lic_id
				
				update @Site_Licenses
				set CONTACT_NAME = 
				(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT c
				where c.CONTACT_ID = @first_contact_id)
				where SITE_LIC_ID = @site_lic_id

				set @counter = 1
			end
			FETCH NEXT FROM contact_id INTO @first_contact_id
		END
		CLOSE contact_id
		DEALLOCATE contact_id
		set @counter = 0
		FETCH NEXT FROM site_id INTO @site_lic_id
	END

	CLOSE site_id
	DEALLOCATE site_id


select * from @Site_Licenses
where ((SITE_LIC_ID like '%' + @TEXT_BOX + '%') OR
				(SITE_LIC_NAME like '%' + @TEXT_BOX + '%') OR
				(CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
				([EXPIRY_DATE] like '%' + @TEXT_BOX + '%')OR
				([COMPANY_NAME] like '%' + @TEXT_BOX + '%'))

/*

	SELECT s_l.*, s_l_a.*, c.CONTACT_NAME 
	from Reviewer.dbo.TB_SITE_LIC s_l
	inner join TB_SITE_LIC_ADMIN s_l_a on s_l_a.SITE_LIC_ID = s_l.SITE_LIC_ID
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = s_l_a.CONTACT_ID
	order by s_l.SITE_LIC_ID
*/
       
END
GO




