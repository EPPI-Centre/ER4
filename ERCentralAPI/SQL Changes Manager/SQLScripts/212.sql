USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewMembers]    Script Date: 20/05/2020 15:00:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ReviewMembers] 
(
	@REVIEW_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
    declare @tb_review_members table 
		(contact_id int, review_contact_id int, contact_name nvarchar(255), email nvarchar(500),
		last_login datetime, review_role nvarchar(50), review_role_1 nvarchar(50), review_role_2 nvarchar(50),
		is_coding_only bit, is_read_only bit, is_admin bit, [expiry_date] datetime, site_lic_id int, site_lic_name nvarchar(50))

	declare @tb_expiry_dates table
		(contactID int, expiryDate datetime, siteLicID int, siteLicName nvarchar(50))

		
	declare @review_contact_id int
    
    insert into @tb_review_members (contact_id, review_contact_id, contact_name, email)
	select c.CONTACT_ID, rc.REVIEW_CONTACT_ID, c.CONTACT_NAME, c.EMAIL
	from Reviewer.dbo.TB_CONTACT c
	right join Reviewer.dbo.TB_REVIEW_CONTACT rc
	on c.CONTACT_ID = rc.CONTACT_ID
	where rc.REVIEW_ID = @REVIEW_ID
	order by c.CONTACT_NAME
    
    --select * from @tb_review_members
    
	/* bug
    update @tb_review_members
	set last_login = 
	(select max(lt.CREATED) as LAST_LOGIN from TB_LOGON_TICKET lt
	where contact_id = lt.CONTACT_ID
	and lt.REVIEW_ID = @REVIEW_ID)
	*/
    
    update @tb_review_members
	set is_coding_only = 0
    
    --select * from @tb_review_members

	declare @tmp_contact_id int 
    
	declare @WORKING_REVIEW_CONTACT_ID int
	declare REVIEW_CONTACT_ID_CURSOR cursor for
	select review_contact_id FROM @tb_review_members
	open REVIEW_CONTACT_ID_CURSOR
	fetch next from REVIEW_CONTACT_ID_CURSOR
	into @WORKING_REVIEW_CONTACT_ID
	while @@FETCH_STATUS = 0
	begin 		
		-- see if the person is coding only for this review
		update @tb_review_members 
		set review_role_2 = 
		(select CRR.ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'Coding only'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID
		
		-- see if the person is read only for this review
		update @tb_review_members 
		set review_role_1 = 
		(select CRR.ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'ReadOnlyUser'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID
		
		update @tb_review_members 
		set review_role = 
		(select CRR.ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'AdminUser'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID

		set @tmp_contact_id =
		(select CONTACT_ID from Reviewer.dbo.TB_REVIEW_CONTACT
		where REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID)

		update @tb_review_members
		set last_login = 
		(select max(lt.CREATED) from TB_LOGON_TICKET lt
		where contact_id = @tmp_contact_id
		and lt.REVIEW_ID = @REVIEW_ID)
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID

		FETCH NEXT FROM REVIEW_CONTACT_ID_CURSOR 
		INTO @WORKING_REVIEW_CONTACT_ID
	END

	CLOSE REVIEW_CONTACT_ID_CURSOR
	DEALLOCATE REVIEW_CONTACT_ID_CURSOR

	declare @expiryDate datetime

	declare @WORKING_CONTACT_ID int
	declare CONTACT_ID_CURSOR cursor for
	select contact_id FROM @tb_review_members
	open CONTACT_ID_CURSOR
	fetch next from CONTACT_ID_CURSOR
	into @WORKING_CONTACT_ID
	while @@FETCH_STATUS = 0
	begin 		
		insert into @tb_expiry_dates
		select c.CONTACT_ID, 
			CASE when l.[EXPIRY_DATE] is not null 
			and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
				then l.[EXPIRY_DATE]
			else c.[EXPIRY_DATE]
			end as 'EXPIRY_DATE', 
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		from Reviewer.dbo.TB_CONTACT c
		left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
		on c.CONTACT_ID = lt.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
		where c.CONTACT_ID = @WORKING_CONTACT_ID
	
		group by c.CONTACT_ID, c.[EXPIRY_DATE], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME

		FETCH NEXT FROM CONTACT_ID_CURSOR 
		INTO @WORKING_CONTACT_ID
	end

	CLOSE CONTACT_ID_CURSOR
	DEALLOCATE CONTACT_ID_CURSOR

	update t1
	set [expiry_date] = t2.expiryDate,
	site_lic_id = t2.siteLicID,
	site_lic_name = t2.siteLicName
	from @tb_review_members t1 inner join @tb_expiry_dates t2
	on t2.contactID = t1.contact_id
    

    update @tb_review_members
	set is_coding_only = 1
	where review_role_2 = 'Coding only'
    
    update @tb_review_members
	set is_read_only = 1
	where review_role_1 = 'ReadOnlyUser'
	
	update @tb_review_members
	set is_admin = 1
	where review_role = 'AdminUser'

	select * from @tb_review_members
	
	


END
GO

-----------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewMembers_2]    Script Date: 20/05/2020 15:03:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ReviewMembers_2] 
(
	@REVIEW_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
    declare @tb_review_members table 
		(contact_id int, review_contact_id int, contact_name nvarchar(255), email nvarchar(500),
		last_login datetime, review_role nvarchar(50), review_role_1 nvarchar(50), review_role_2 nvarchar(50),
		is_coding_only bit, is_read_only bit, is_admin bit)
		
	declare @review_contact_id int
    
    insert into @tb_review_members (contact_id, review_contact_id, contact_name, email)
	select c.CONTACT_ID, rc.REVIEW_CONTACT_ID, c.CONTACT_NAME, c.EMAIL
	from Reviewer.dbo.TB_CONTACT c
	right join Reviewer.dbo.TB_REVIEW_CONTACT rc
	on c.CONTACT_ID = rc.CONTACT_ID
	where rc.REVIEW_ID = @REVIEW_ID
	order by c.CONTACT_NAME
    
    --select * from @tb_review_members
    
	/* bug
    update @tb_review_members
	set last_login = 
	(select max(lt.CREATED) as LAST_LOGIN from TB_LOGON_TICKET lt
	where contact_id = lt.CONTACT_ID
	and lt.REVIEW_ID = @REVIEW_ID)
    */

    update @tb_review_members
	set is_coding_only = 0
    
    --select * from @tb_review_members

	declare @tmp_contact_id int 
    
	declare @WORKING_REVIEW_CONTACT_ID int
	declare REVIEW_CONTACT_ID_CURSOR cursor for
	select review_contact_id FROM @tb_review_members
	open REVIEW_CONTACT_ID_CURSOR
	fetch next from REVIEW_CONTACT_ID_CURSOR
	into @WORKING_REVIEW_CONTACT_ID
	while @@FETCH_STATUS = 0
	begin 		
		-- see if the person is coding only for this review
		update @tb_review_members 
		set review_role_2 = 
		(select CRR.ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'Coding only'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID
		
		-- see if the person is read only for this review
		update @tb_review_members 
		set review_role_1 = 
		(select CRR.ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'ReadOnlyUser'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID
		
		update @tb_review_members 
		set review_role = 
		(select CRR.ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'AdminUser'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID

		set @tmp_contact_id =
		(select CONTACT_ID from Reviewer.dbo.TB_REVIEW_CONTACT
		where REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID)

		update @tb_review_members
		set last_login = 
		(select max(lt.CREATED) from TB_LOGON_TICKET lt
		where contact_id = @tmp_contact_id
		and lt.REVIEW_ID = @REVIEW_ID)
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID

		FETCH NEXT FROM REVIEW_CONTACT_ID_CURSOR 
		INTO @WORKING_REVIEW_CONTACT_ID
	END

	CLOSE REVIEW_CONTACT_ID_CURSOR
	DEALLOCATE REVIEW_CONTACT_ID_CURSOR
    
    --select * from @tb_review_members
    update @tb_review_members
	set is_coding_only = 1
	where review_role_2 = 'Coding only'
    
    update @tb_review_members
	set is_read_only = 1
	where review_role_1 = 'ReadOnlyUser'
	
	update @tb_review_members
	set is_admin = 1
	where review_role = 'AdminUser'
	
	select * from @tb_review_members
	




END

GO

