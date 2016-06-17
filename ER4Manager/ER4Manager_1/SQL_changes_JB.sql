---- Feb 11 2016
---- these are british library changes
-- new st_BritishLibraryValuesGetAll
-- new st_BritishLibraryValuesSet
-- new st_BritishLibraryCCValuesSet
-- new st_BritishLibraryValuesGetAll
-- new st_BritishLibraryValuesSetOnLicense
-- new st_BritishLibraryCCValuesSetOnLicense
-- edit st_Site_Lic_Add_Review




USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BritishLibraryValuesGetAll]    Script Date: 02/11/2016 15:28:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<gets contact table for a contactID>
-- =============================================
CREATE PROCEDURE [dbo].[st_BritishLibraryValuesGetAll] 
(
	@REVIEW_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    -- need to lose the milliseconds as the database is setting it to 000

	select BL_ACCOUNT_CODE, BL_AUTH_CODE, BL_TX, 
	BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX 
	from Reviewer.dbo.TB_REVIEW 
	where REVIEW_ID = @REVIEW_ID
    	
	RETURN
END


GO


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BritishLibraryValuesSet]    Script Date: 02/11/2016 15:29:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE procedure [dbo].[st_BritishLibraryValuesSet]
(
	@REVIEW_ID int,
	@BL_ACCOUNT_CODE nvarchar(50), 
	@BL_AUTH_CODE nvarchar(50), 
	@BL_TX nvarchar(50)
)

As

SET NOCOUNT ON

	update Reviewer.dbo.TB_REVIEW
	set BL_ACCOUNT_CODE = @BL_ACCOUNT_CODE,
	BL_AUTH_CODE = @BL_AUTH_CODE,
	BL_TX = @BL_TX 
	where REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF



GO


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BritishLibraryCCValuesSet]    Script Date: 02/11/2016 15:29:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE procedure [dbo].[st_BritishLibraryCCValuesSet]
(
	@REVIEW_ID int,
	@BL_CC_ACCOUNT_CODE nvarchar(50), 
	@BL_CC_AUTH_CODE nvarchar(50), 
	@BL_CC_TX nvarchar(50)
)

As

SET NOCOUNT ON

	update Reviewer.dbo.TB_REVIEW
	set BL_CC_ACCOUNT_CODE = @BL_CC_ACCOUNT_CODE,
	BL_CC_AUTH_CODE = @BL_CC_AUTH_CODE,
	BL_CC_TX = @BL_CC_TX 
	where REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF




GO


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BritishLibraryValuesGetFromLicense]    Script Date: 02/24/2016 14:16:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<gets contact table for a contactID>
-- =============================================
CREATE PROCEDURE [dbo].[st_BritishLibraryValuesGetFromLicense] 
(
	@SITE_LIC_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    -- need to lose the milliseconds as the database is setting it to 000

	select BL_ACCOUNT_CODE, BL_AUTH_CODE, BL_TX, 
	BL_CC_ACCOUNT_CODE, BL_CC_AUTH_CODE, BL_CC_TX 
	from Reviewer.dbo.TB_SITE_LIC 
	where SITE_LIC_ID = @SITE_LIC_ID
    	
	RETURN
END


GO


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BritishLibraryValuesSetOnLicense]    Script Date: 02/24/2016 14:18:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE procedure [dbo].[st_BritishLibraryValuesSetOnLicense]
(
	@SITE_LIC_ID int,
	@BL_ACCOUNT_CODE nvarchar(50), 
	@BL_AUTH_CODE nvarchar(50), 
	@BL_TX nvarchar(50)
)

As

SET NOCOUNT ON

	update Reviewer.dbo.TB_SITE_LIC
	set BL_ACCOUNT_CODE = @BL_ACCOUNT_CODE,
	BL_AUTH_CODE = @BL_AUTH_CODE,
	BL_TX = @BL_TX 
	where SITE_LIC_ID = @SITE_LIC_ID

SET NOCOUNT OFF



GO


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BritishLibraryCCValuesSetOnLicense]    Script Date: 02/24/2016 14:20:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE procedure [dbo].[st_BritishLibraryCCValuesSetOnLicense]
(
	@SITE_LIC_ID int,
	@BL_CC_ACCOUNT_CODE nvarchar(50), 
	@BL_CC_AUTH_CODE nvarchar(50), 
	@BL_CC_TX nvarchar(50)
)

As

SET NOCOUNT ON

	update Reviewer.dbo.TB_SITE_LIC
	set BL_CC_ACCOUNT_CODE = @BL_CC_ACCOUNT_CODE,
	BL_CC_AUTH_CODE = @BL_CC_AUTH_CODE,
	BL_CC_TX = @BL_CC_TX 
	where SITE_LIC_ID = @SITE_LIC_ID

SET NOCOUNT OFF




GO



USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Review]    Script Date: 02/24/2016 14:44:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_Site_Lic_Add_Review]
	-- Add the parameters for the stored procedure here
	@lic_id int
	, @admin_ID int
	, @review_id int
	, @res int output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @review_name nvarchar(1000)
	set @res = 0

    -- Insert statements for procedure here
    
    -- initial check to see if review exists
    select @review_name = REVIEW_NAME from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @review_id
	if @@ROWCOUNT = 1 
	begin
		declare @ck int, @ck2 int, @lic_det_id int
		set @ck = (SELECT count(contact_id) from TB_SITE_LIC_ADMIN 
		where (CONTACT_ID = @admin_ID or @admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT 
		where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)
			) and SITE_LIC_ID = @lic_id)
		if @ck < 1 set @res = -1 --first check: see if supplied admin is actually and admin!
		
		else -- initial checks went OK, let's see if we can do it
		begin
			set @ck = (select count(review_id) from Reviewer.dbo.TB_SITE_LIC_REVIEW where REVIEW_ID = @review_id)
			set @lic_det_id = (SELECT TOP 1 ld.SITE_LIC_DETAILS_ID from Reviewer.dbo.TB_SITE_LIC sl 
									inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
									inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
									and sl.SITE_LIC_ID = @lic_id
								Order by ld.VALID_FROM desc
								)
			--@ck2 counts how many reviews can be added to current lic
			set @ck2 = (select d.REVIEWS_ALLOWANCE - count(review_id) from TB_SITE_LIC_DETAILS d inner join
							 Reviewer.dbo.TB_SITE_LIC_REVIEW lr on lr.SITE_LIC_DETAILS_ID = @lic_det_id
								and lr.SITE_LIC_DETAILS_ID = d.SITE_LIC_DETAILS_ID
							 group by d.REVIEWS_ALLOWANCE
							 )--count how many reviews can still be added
			if @ck != 0 -- review is already in a site lic
			begin
				set @ck = (select count(review_id) from Reviewer.dbo.TB_SITE_LIC_REVIEW where REVIEW_ID = @review_id and SITE_LIC_ID = @lic_id)
				if @ck = 1 set @res = -3 --review already in this site_lic
				else set @res = -4 --review is in some other site_lic
			end
			else if @ck2 < 1 --no allowance available, all review slots for current license have been used
			begin
				set @res = -5
			end
			else
			begin --all is well, let's do something!
				begin transaction --make sure we don't commit only half of the mission critical data! 
				--(we assume the update below will work, only checking for the other statement)
				update Reviewer.dbo.TB_REVIEW set [EXPIRY_DATE] = GETDATE()
					where REVIEW_ID = @review_id and REVIEW_NAME = @review_name and [EXPIRY_DATE] is null
				insert into Reviewer.dbo.TB_SITE_LIC_REVIEW (
					[SITE_LIC_ID]
					,[SITE_LIC_DETAILS_ID]
					,[REVIEW_ID]
					,[ADDED_BY]
					) values (
					@lic_id, @lic_det_id, @review_id, @admin_ID
					)
				if @@ROWCOUNT = 1
				begin --write success log
					commit transaction --all is well, commit
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @review_id, 'add review'
						from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc
						
					/*********************************************/
					-- if all of the brit lib fields are blank for this review
					declare @tvsl_BL_ACCOUNT_CODE nvarchar(50) = (select BL_ACCOUNT_CODE from Reviewer.dbo.TB_SITE_LIC where SITE_LIC_ID = @lic_id)
					declare @tvsl_BL_AUTH_CODE nvarchar(50) = (select BL_AUTH_CODE from Reviewer.dbo.TB_SITE_LIC where SITE_LIC_ID = @lic_id)
					declare @tvsl_BL_TX nvarchar(50) = (select BL_TX from Reviewer.dbo.TB_SITE_LIC where SITE_LIC_ID = @lic_id)
					declare @tvsl_BL_CC_ACCOUNT_CODE nvarchar(50) = (select BL_CC_ACCOUNT_CODE from Reviewer.dbo.TB_SITE_LIC where SITE_LIC_ID = @lic_id)
					declare @tvsl_BL_CC_AUTH_CODE nvarchar(50) = (select BL_CC_AUTH_CODE from Reviewer.dbo.TB_SITE_LIC where SITE_LIC_ID = @lic_id)
					declare @tvsl_BL_CC_TX nvarchar(50) = (select BL_CC_TX from Reviewer.dbo.TB_SITE_LIC where SITE_LIC_ID = @lic_id)
					
					declare @tvr_BL_ACCOUNT_CODE nvarchar(50) = (select BL_ACCOUNT_CODE from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @review_id)
					declare @tvr_BL_AUTH_CODE nvarchar(50) = (select BL_AUTH_CODE from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @review_id)
					declare @tvr_BL_TX nvarchar(50) = (select BL_TX from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @review_id)
					declare @tvr_BL_CC_ACCOUNT_CODE nvarchar(50) = (select BL_CC_ACCOUNT_CODE from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @review_id)
					declare @tvr_BL_CC_AUTH_CODE nvarchar(50) = (select BL_CC_AUTH_CODE from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @review_id)
					declare @tvr_BL_CC_TX nvarchar(50) = (select BL_CC_TX from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @review_id)
					
					if (((@tvr_BL_ACCOUNT_CODE = '') or (@tvr_BL_ACCOUNT_CODE is null))
						and ((@tvr_BL_AUTH_CODE = '') or (@tvr_BL_AUTH_CODE is null)) 
						and ((@tvr_BL_TX = '') or (@tvr_BL_TX is null)) 
						and ((@tvr_BL_CC_ACCOUNT_CODE = '') or (@tvr_BL_CC_ACCOUNT_CODE is null)) 
						and ((@tvr_BL_CC_AUTH_CODE = '') or (@tvr_BL_CC_AUTH_CODE is null)) 
						and ((@tvr_BL_CC_TX = '') or (@tvr_BL_CC_TX is null)))
					begin -- update TB_REVIEW - even if there aren't site license codes it will just put in blanks
						update Reviewer.dbo.TB_REVIEW
						set BL_ACCOUNT_CODE = @tvsl_BL_ACCOUNT_CODE,
							BL_AUTH_CODE = @tvsl_BL_AUTH_CODE,
							BL_TX = @tvsl_BL_TX,
							BL_CC_ACCOUNT_CODE = @tvsl_BL_CC_ACCOUNT_CODE,
							BL_CC_AUTH_CODE = @tvsl_BL_CC_AUTH_CODE,
							BL_CC_TX = @tvsl_BL_CC_TX
						where REVIEW_ID = @review_id
					end	
					/****************************************/	
					
				end
				else --write failure log, if this is fired, there is a bug somewhere
				begin
					rollback transaction --BAD! something went wrong!
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @review_id, 'add review: failed!'
						from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc	
					set @res = -6
				end
			end
		end
	end
	else
	begin
		set @res = -2
	end
	
	--select r.review_id, review_name from reviewer.dbo.TB_SITE_LIC_REVIEW lr
	--	inner join Reviewer.dbo.TB_REVIEW r on lr.REVIEW_ID = r.REVIEW_ID
	--	where SITE_LIC_ID = @lic_id
	 
	return @res
	-- error codes: -1 = supplied admin_id is not an admin of this site lice
	--				-2 = review_id does not exist
	--				-3 = review already in this site_lic
	--				-4 = review is in some other site_lic
	--				-5 = no allowance available, all review slots for current license have been used
	--				-6 = all seemed well but couldn't write changes! BUG ALERT
END









---- Jan 27 2016 
-- edit st_ContactDetails
-- new st_ContactIsSiteLicenseAdm
-- edit st_ContactDetailsFullCreateOrEdit


-- setting read-only role in summary page
-- 29/09/2015 
-- new st_ReviewMembers_2
-- new st_ReviewRoleReadOnlyUpdate





/*************************************************************************************************************************************************************/
-- oAuth
-- July 21
/*************************************************************************************************************************************************************/


-- edit st_ContactReviewsArchieProspective
-- edit st_ContactReviewsArchieFull
-- edit st_ReviewMembers_1




/*************************************************************************************************************************************************************/
-- oAuth
-- June 04
/*************************************************************************************************************************************************************/


-- new st_ContactReviewsArchie
-- new st_ReviewDetailsCochrane
-- new st_ReviewDetailsGetAllCochrane
-- new st_ReviewDetailsFilterCochrane
-- new st_ArchieIDSave
-- new st_ReviewRemoveMember_1

-- edit st_ReviewDetailsGetAll
-- edit st_ReviewDetailsFilter
-- edit st_ContactLogin
-- edit st_ContactReviewsNonShareable




/*************************************************************************************************************************************************************/
-- Fix problem with review copy error
-- starting Apri 23
/*************************************************************************************************************************************************************/
--These changes are on the live database so you can get the stored procedures from there
--edit st_CopyReviewStep11
--edit st_CopyReviewStep09


/*************************************************************************************************************************************************************/
-- Further removal of componentArts controls
-- starting Feb 23
/*************************************************************************************************************************************************************/

--These changes are on the live database so you can get the stored procedures from there
-- new st_NewsletterUpdate
-- new st_WDAttrDataGet_1
-- new st_ContactDetailsGetAllFilter_1

-- Changes to ReturnFromPayment.aspx.cs
------ Removed the componentArt bits but not sure how to check it without making payment.
------ I just want to confirm the correct tabs are selected

/*************************************************************************************************************************************************************/
-- START of fresh start with Telerik controls
-- starting Nov 17
/*************************************************************************************************************************************************************/


-- new stored procedures
--new st_CodesetsGet
--new st_ReviewDetailsFilter
--new st_ContactDetailsGetAllFilter
--new st_ContactReviewsFilter
--new st_CheckUserNameAndEmail


-- changes on TestReviewerAdmin (but are correct on live and local)
-- st_ReviewDetails
-- st_ContactDetailsGetAll
-- st_ExtensionTypesGet
















/*************************************************************************************************************************************************************/
-- old stuff from here on!

/*************************************************************************************************************************************************************/

---- change to preserve the order of the sets when setting up data viewer

--USE [ReviewerAdmin]
--GO
--/****** Object:  StoredProcedure [dbo].[st_WDSetCopyToWebDB]    Script Date: 09/05/2014 11:10:44 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO






--ALTER procedure [dbo].[st_WDSetCopyToWebDB]
--(
--	@SET_ID int,
--	@REVIEW_ID bigint, 
--	@WEBDB_ID bigint, 
--	@INSERTED bit output
--)

--As

--SET NOCOUNT ON

--	select * from Presenter.dbo.TB_WEB_DATABASE_ATTR 
--		where SET_ID = @SET_ID
--		and WEBDB_ID = @WEBDB_ID

--	if @@ROWCOUNT < 1 -- we can insert all of the the rows
--	begin
--		-- get the parts from TB_SET
--		insert into Presenter.dbo.TB_WEB_DATABASE_ATTR 
--		(ATTRIBUTE_ID, WEBDB_ID, /*LEVEL,*/ ATTRIBUTE_NAME, PARENT_ATTRIBUTE_ID,
--		SET_ID, ATTRIBUTE_ORDER)
--		select @SET_ID, @WEBDB_ID, /*0,*/ SET_NAME, 0, @SET_ID, r_s.SET_ORDER
--		from Reviewer.dbo.TB_SET s
--		inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
--		where s.SET_ID = @SET_ID
		

--		-- @attribute_id
--		declare @attribute_id table (attribute_id int, ok int)
--		--03 get source ATTRIBUTE_ID's and put in tmp table @attribute_id (avoids orphans)	
--		insert into @attribute_id 
--		Select a_s.ATTRIBUTE_ID, Reviewer.dbo.fn_IsAttributeInTree(a_s.ATTRIBUTE_ID ) as ok 
--		from Reviewer.dbo.TB_ATTRIBUTE_SET a_s 
--		inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
--		where a_s.SET_ID = @SET_ID
--		and r_s.REVIEW_ID = @REVIEW_ID

--		-- get the parts from TB_ATTRIBUTE_SET
--		insert into Presenter.dbo.TB_WEB_DATABASE_ATTR 
--		(ATTRIBUTE_ID, ATTRIBUTE_SET_ID, WEBDB_ID, /*LEVEL,*/ ATTRIBUTE_NAME, PARENT_ATTRIBUTE_ID,
--		SET_ID, ATTRIBUTE_ORDER, ATTRIBUTE_DESC)
--		select a_s.ATTRIBUTE_ID, a_s.ATTRIBUTE_SET_ID,  @WEBDB_ID, /*1,*/ a.ATTRIBUTE_NAME, a_s.PARENT_ATTRIBUTE_ID, @SET_ID, 
--		a_s.ATTRIBUTE_ORDER, a_s.ATTRIBUTE_SET_DESC
--		from Reviewer.dbo.TB_ATTRIBUTE_SET a_s 
--		inner join Reviewer.dbo.TB_ATTRIBUTE a on a_s.ATTRIBUTE_ID = a.ATTRIBUTE_ID
--		and a_s.SET_ID = @SET_ID
--		and a.ATTRIBUTE_ID in (select attribute_id from @attribute_id where ok = 1)
		
--		set @INSERTED = 1
--	end
--	else
--	begin
--		set @INSERTED = 0
--	end
	
--SET NOCOUNT OFF










---- changes to allow person to be admin for multiple site licences
---- new sp: st_Site_Lic_Get_By_Admin
---- new sp: st_Site_Lic_Get_Details_1
---- new sp: st_Site_Lic_Get_1
---- new sp: st_Site_Lic_Create_or_Edit_1
---- new sp: st_Site_Lic_Add_Remove_Admin_1

---- changes to allow role edits
---- new sp: st_ReviewMembers_1
---- new sp: st_ReviewRoleIsAdminUpdate


-------------------------------------------------------------------

---- Presenter changes to ReviewerAdmin
---- edit st_WDDescriptionGet
---- edit st_WDSaveIntroduction
---- edit st_WDRenameAttribute
---- edit st_WDSetCopyToWebDB
---- create st_WDSaveName
---- edit st_WDSaveName
---- create st_WDSetAdminEditIntro
---- create st_WDGetHeaderImage1
---- create st_WDGetHeaderImage2
---- create st_WDUploadHeaderURL
---- create st_WDUploadHeaderImage
---- edit st_WDCreateIfNotExist
---- create st_WDDeleteHeaderImage



--USE [ReviewerAdmin]
--GO
--/****** Object:  StoredProcedure [dbo].[st_WDSetCopyToWebDB]    Script Date: 10/16/2012 15:43:23 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO






--ALTER procedure [dbo].[st_WDSetCopyToWebDB]
--(
--	@SET_ID int,
--	@REVIEW_ID bigint, 
--	@WEBDB_ID bigint, 
--	@INSERTED bit output
--)

--As

--SET NOCOUNT ON

--	select * from Presenter.dbo.TB_WEB_DATABASE_ATTR 
--		where SET_ID = @SET_ID
--		and WEBDB_ID = @WEBDB_ID

--	if @@ROWCOUNT < 1 -- we can insert all of the the rows
--	begin
--		-- get the parts from TB_SET
--		insert into Presenter.dbo.TB_WEB_DATABASE_ATTR 
--		(ATTRIBUTE_ID, WEBDB_ID, /*LEVEL,*/ ATTRIBUTE_NAME, PARENT_ATTRIBUTE_ID,
--		SET_ID, ATTRIBUTE_ORDER)
--		select @SET_ID, @WEBDB_ID, /*0,*/ SET_NAME, 0, @SET_ID, 0
--		from Reviewer.dbo.TB_SET s
--		where s.SET_ID = @SET_ID
		

--		-- @attribute_id
--		declare @attribute_id table (attribute_id int, ok int)
--		--03 get source ATTRIBUTE_ID's and put in tmp table @attribute_id (avoids orphans)	
--		insert into @attribute_id 
--		Select a_s.ATTRIBUTE_ID, Reviewer.dbo.fn_IsAttributeInTree(a_s.ATTRIBUTE_ID ) as ok 
--		from Reviewer.dbo.TB_ATTRIBUTE_SET a_s 
--		inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
--		where a_s.SET_ID = @SET_ID
--		and r_s.REVIEW_ID = @REVIEW_ID

--		-- get the parts from TB_ATTRIBUTE_SET
--		insert into Presenter.dbo.TB_WEB_DATABASE_ATTR 
--		(ATTRIBUTE_ID, ATTRIBUTE_SET_ID, WEBDB_ID, /*LEVEL,*/ ATTRIBUTE_NAME, PARENT_ATTRIBUTE_ID,
--		SET_ID, ATTRIBUTE_ORDER, ATTRIBUTE_DESC)
--		select a_s.ATTRIBUTE_ID, a_s.ATTRIBUTE_SET_ID,  @WEBDB_ID, /*1,*/ a.ATTRIBUTE_NAME, a_s.PARENT_ATTRIBUTE_ID, @SET_ID, 
--		a_s.ATTRIBUTE_ORDER, a.ATTRIBUTE_DESC
--		from Reviewer.dbo.TB_ATTRIBUTE_SET a_s 
--		inner join Reviewer.dbo.TB_ATTRIBUTE a on a_s.ATTRIBUTE_ID = a.ATTRIBUTE_ID
--		and a_s.SET_ID = @SET_ID
--		and a.ATTRIBUTE_ID in (select attribute_id from @attribute_id where ok = 1)
		
--		set @INSERTED = 1
--	end
--	else
--	begin
--		set @INSERTED = 0
--	end
	
--SET NOCOUNT OFF











--USE [ReviewerAdmin]
--GO
--/****** Object:  StoredProcedure [dbo].[st_WDRenameAttribute]    Script Date: 10/03/2012 14:40:34 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO










--ALTER procedure [dbo].[st_WDRenameAttribute]
--(
--	@ATTRIBUTE_1 nvarchar(50),
--	@ATTRIBUTE_2 bigint,
--	@ATTRIBUTE_NAME nvarchar(250),
--	@SET_ID bigint
--)

--As

--SET NOCOUNT ON
--	-- level = 0 @ATTRIBUTE_1 = attribute_set_id (i.e. ''), @ATTRIBUTE_2 = attribute_id
--    -- level > 0 @ATTRIBUTE_2 = attribute_set_id,           @ATTRIBUTE_2 = attribute_set_id

--	if @ATTRIBUTE_1 = ''
--	begin
--		update Presenter.dbo.TB_WEB_DATABASE_ATTR
--		set ATTRIBUTE_NAME = @ATTRIBUTE_NAME
--		where SET_ID = @SET_ID
--		and ATTRIBUTE_ID = @ATTRIBUTE_2
--	end
--	else
--		begin
--		update Presenter.dbo.TB_WEB_DATABASE_ATTR
--		set ATTRIBUTE_NAME = @ATTRIBUTE_NAME
--		where SET_ID = @SET_ID
--		and ATTRIBUTE_SET_ID = @ATTRIBUTE_1
--	end
	
	
--SET NOCOUNT OFF













--USE [ReviewerAdmin]
--GO
--/****** Object:  StoredProcedure [dbo].[st_WDDescriptionGet]    Script Date: 09/24/2012 10:58:40 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO





--ALTER procedure [dbo].[st_WDDescriptionGet]
--(
--	@WEBDB_ID int
--)

--As

--SET NOCOUNT ON

--	select w_d.[DESCRIPTION], w_d.HEADER, w_d.WEBDB_NAME, w_d.RESTRICT_ACCESS,
--	w_d.USERNAME, w_d.PASSWD, a.ATTRIBUTE_NAME 
--	from Presenter.dbo.TB_WEB_DATABASE w_d
--	left join Reviewer.dbo.TB_ATTRIBUTE a
--	on w_d.ATTR_TO_INCLUDE = a.ATTRIBUTE_ID
--	where w_d.WEBDB_ID = @WEBDB_ID

--SET NOCOUNT OFF

---------------------------------------------

--USE [ReviewerAdmin]
--GO
--/****** Object:  StoredProcedure [dbo].[st_WDSaveIntroduction]    Script Date: 09/24/2012 11:47:31 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO








--ALTER procedure [dbo].[st_WDSaveIntroduction]
--(
--	@WEBDB_ID int,
--	@AREA nvarchar(50),
--	@DESCRIPTION nvarchar(max)
--)

--As

--SET NOCOUNT ON

--	if @AREA = 'Edit introduction text'
--	BEGIN
--		update Presenter.dbo.TB_WEB_DATABASE
--		set [DESCRIPTION] = @DESCRIPTION
--		where WEBDB_ID = @WEBDB_ID
--	END
--	ELSE
--	BEGIN
--		update Presenter.dbo.TB_WEB_DATABASE
--		set HEADER = @DESCRIPTION
--		where WEBDB_ID = @WEBDB_ID
--	END
	
--SET NOCOUNT OFF



---------------------------------------------------

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_WDSaveName]    Script Date: 09/24/2012 11:44:00 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO









--CREATE procedure [dbo].[st_WDSaveName]
--(
--	@WEBDB_ID int,
--	@WEBDB_NAME nvarchar(50)
--)

--As

--SET NOCOUNT ON

--	update Presenter.dbo.TB_WEB_DATABASE
--	set WEBDB_NAME =@WEBDB_NAME
--	where WEBDB_ID = @WEBDB_ID
	
--SET NOCOUNT OFF









--GO


--------------------------------
---- Coding only changes
----edit st_ReviewMembers
----new st_ReviewRoleCodingOnlyUpdate



--USE [ReviewerAdmin]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ReviewMembers]    Script Date: 10/03/2012 10:45:47 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
---- =============================================
---- Author:		<Jeff>
---- ALTER date: <24/03/2010>
---- Description:	<gets the review data based on contact>
---- =============================================
--ALTER PROCEDURE [dbo].[st_ReviewMembers] 
--(
--	@REVIEW_ID nvarchar(50)
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
	
--    -- Insert statements for procedure here
--    CREATE table #REVIEW_MEMBERS
--	(
--		CONTACT_ID int,
--		REVIEW_CONTACT_ID int,
--		CONTACT_NAME nvarchar(255),
--		EMAIL nvarchar(500),
--		LAST_LOGIN datetime,
--		REVIEW_ROLE nvarchar(50),
--		IS_CODING_ONLY bit
--	)
--	declare @REVIEW_CONTACT_ID int
	
	
--	insert into #REVIEW_MEMBERS (CONTACT_ID, REVIEW_CONTACT_ID, CONTACT_NAME, EMAIL)
--	select c.CONTACT_ID, rc.REVIEW_CONTACT_ID, c.CONTACT_NAME, c.EMAIL
--	from Reviewer.dbo.TB_CONTACT c
--	right join Reviewer.dbo.TB_REVIEW_CONTACT rc
--	on c.CONTACT_ID = rc.CONTACT_ID
--	where rc.REVIEW_ID = @REVIEW_ID
--	order by c.CONTACT_NAME
	
--	update #REVIEW_MEMBERS 
--	set #REVIEW_MEMBERS.LAST_LOGIN = 
--	(select max(lt.CREATED) as LAST_LOGIN from TB_LOGON_TICKET lt
--	where #REVIEW_MEMBERS.CONTACT_ID = lt.CONTACT_ID
--	and lt.REVIEW_ID = @REVIEW_ID)
	
	
--	update #REVIEW_MEMBERS
--	set IS_CODING_ONLY = 0
	
--	declare @WORKING_REVIEW_CONTACT_ID int
--	declare REVIEW_CONTACT_ID_CURSOR cursor for
--	select REVIEW_CONTACT_ID FROM #REVIEW_MEMBERS
--	open REVIEW_CONTACT_ID_CURSOR
--	fetch next from REVIEW_CONTACT_ID_CURSOR
--	into @WORKING_REVIEW_CONTACT_ID
--	while @@FETCH_STATUS = 0
--	begin 		
--		-- see if the person is coding only for this review
--		update #REVIEW_MEMBERS 
--		set #REVIEW_MEMBERS.REVIEW_ROLE = 
--		(select ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE
--		where REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
--		and ROLE_NAME = 'Coding only'
--		) 
--		where #REVIEW_MEMBERS.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID

--		FETCH NEXT FROM REVIEW_CONTACT_ID_CURSOR 
--		INTO @WORKING_REVIEW_CONTACT_ID
--	END

--	CLOSE REVIEW_CONTACT_ID_CURSOR
--	DEALLOCATE REVIEW_CONTACT_ID_CURSOR
	
--	update #REVIEW_MEMBERS
--	set IS_CODING_ONLY = 1
--	where REVIEW_ROLE = 'Coding only'
	
--	select * from #REVIEW_MEMBERS
	
--	drop table #REVIEW_MEMBERS
    

--END



--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ReviewRoleCodingOnlyUpdate]    Script Date: 10/03/2012 13:58:32 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



--CREATE procedure [dbo].[st_ReviewRoleCodingOnlyUpdate]
--(
--	@REVIEW_ID int,
--	@CONTACT_ID int,
--	@IS_CHECKED bit
--)

--As

--SET NOCOUNT ON

--	declare @REVIEW_CONTACT_ID int
--	-- get REVIEW_CONTACT_ID
--	select @REVIEW_CONTACT_ID = REVIEW_CONTACT_ID 
--    from Reviewer.dbo.TB_REVIEW_CONTACT
--    where REVIEW_ID = @REVIEW_ID
--    and CONTACT_ID = @CONTACT_ID
	
	
--	if @IS_CHECKED = 1
--	begin
--		insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
--		values (@REVIEW_CONTACT_ID, 'Coding only')
--	end
--	else
--	begin
--		delete from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE 
--		where REVIEW_CONTACT_ID = @REVIEW_CONTACT_ID
--		and ROLE_NAME = 'Coding only'
--	end
--SET NOCOUNT OFF



--GO








----------------------------


---- Email changes
---- new table TB_MANAGMENT_EMAILS
---- new stored procedures
------ st_EmailGet
------ st_EmailsGet
------ st_EmailUpdate

--USE [ReviewerAdmin]
--GO

--/****** Object:  Table [dbo].[TB_MANAGMENT_EMAILS]    Script Date: 09/11/2012 15:56:46 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

--CREATE TABLE [dbo].[TB_MANAGMENT_EMAILS](
--	[EMAIL_ID] [int] IDENTITY(1,1) NOT NULL,
--	[EMAIL_NAME] [nvarchar](50) NOT NULL,
--	[EMAIL_MESSAGE] [nvarchar](4000) NULL,
-- CONSTRAINT [PK_TB_MANAGMENT_EMAILS] PRIMARY KEY CLUSTERED 
--(
--	[EMAIL_ID] ASC
--)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
--) ON [PRIMARY]

--GO

----------------------------

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_EmailGet]    Script Date: 09/11/2012 16:05:07 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO




--CREATE PROCEDURE [dbo].[st_EmailGet] 
--(
--	@EMAIL_ID int
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here

--	select EMAIL_NAME, EMAIL_MESSAGE from TB_MANAGMENT_EMAILS
--	where EMAIL_ID = @EMAIL_ID
    	
--	RETURN
--END



--GO

------------------------------------

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_EmailUpdate]    Script Date: 09/11/2012 16:07:32 ******/
--SET ANSI_NULLS OFF
--GO

--SET QUOTED_IDENTIFIER ON
--GO



--CREATE PROCEDURE [dbo].[st_EmailUpdate]
--(
--	@EMAIL_ID int,
--	@EMAIL_NAME nvarchar(50),
--	@EMAIL_MESSAGE nvarchar(2000)
--)
--As
--SET NOCOUNT ON


--		update TB_MANAGMENT_EMAILS
--		set EMAIL_MESSAGE = @EMAIL_MESSAGE, EMAIL_NAME = @EMAIL_NAME
--		where EMAIL_ID = @EMAIL_ID 


--SET NOCOUNT OFF




--GO





--------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------

---- newsletter changes
---- new table TB_NEWSLETTER
---- new table TB_NEWSLETTER_CONTACT
---- alter relationship between the two tables




--/*
--   12 July 201209:34:22
--   User: 
--   Server: db-epi
--   Database: ReviewerAdmin
--   Application: 
--*/

--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--BEGIN TRANSACTION
--SET QUOTED_IDENTIFIER ON
--SET ARITHABORT ON
--SET NUMERIC_ROUNDABORT OFF
--SET CONCAT_NULL_YIELDS_NULL ON
--SET ANSI_NULLS ON
--SET ANSI_PADDING ON
--SET ANSI_WARNINGS ON
--COMMIT
--BEGIN TRANSACTION
--GO
--CREATE TABLE dbo.TB_NEWSLETTER
--	(
--	NEWSLETTER_ID int NOT NULL IDENTITY (1, 1),
--	YEAR int NOT NULL,
--	MONTH int NOT NULL,
--	NEWSLETTER nvarchar(MAX) NULL,
--	ORDER_NUMBER int NULL
--	)  ON [PRIMARY]
--	 TEXTIMAGE_ON [PRIMARY]
--GO
--ALTER TABLE dbo.TB_NEWSLETTER ADD CONSTRAINT
--	PK_TB_NEWSLETTER PRIMARY KEY CLUSTERED 
--	(
--	NEWSLETTER_ID
--	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

--GO
--ALTER TABLE dbo.TB_NEWSLETTER SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--select Has_Perms_By_Name(N'dbo.TB_NEWSLETTER', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_NEWSLETTER', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_NEWSLETTER', 'Object', 'CONTROL') as Contr_Per 


------------------------

--/*
--   12 July 201209:38:23
--   User: 
--   Server: db-epi
--   Database: ReviewerAdmin
--   Application: 
--*/

--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--BEGIN TRANSACTION
--SET QUOTED_IDENTIFIER ON
--SET ARITHABORT ON
--SET NUMERIC_ROUNDABORT OFF
--SET CONCAT_NULL_YIELDS_NULL ON
--SET ANSI_NULLS ON
--SET ANSI_PADDING ON
--SET ANSI_WARNINGS ON
--COMMIT
--BEGIN TRANSACTION
--GO
--CREATE TABLE dbo.TB_NEWSLETTER_CONTACT
--	(
--	NEWSLETTER_ID int NOT NULL,
--	CONTACT_ID int NOT NULL
--	)  ON [PRIMARY]
--GO
--ALTER TABLE dbo.TB_NEWSLETTER_CONTACT SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--select Has_Perms_By_Name(N'dbo.TB_NEWSLETTER_CONTACT', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_NEWSLETTER_CONTACT', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_NEWSLETTER_CONTACT', 'Object', 'CONTROL') as Contr_Per 


------------------------

--/*
--   12 July 201209:40:27
--   User: 
--   Server: db-epi
--   Database: ReviewerAdmin
--   Application: 
--*/

--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--BEGIN TRANSACTION
--SET QUOTED_IDENTIFIER ON
--SET ARITHABORT ON
--SET NUMERIC_ROUNDABORT OFF
--SET CONCAT_NULL_YIELDS_NULL ON
--SET ANSI_NULLS ON
--SET ANSI_PADDING ON
--SET ANSI_WARNINGS ON
--COMMIT
--BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.TB_NEWSLETTER SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--select Has_Perms_By_Name(N'dbo.TB_NEWSLETTER', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_NEWSLETTER', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_NEWSLETTER', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.TB_NEWSLETTER_CONTACT ADD CONSTRAINT
--	FK_TB_NEWSLETTER_CONTACT_TB_NEWSLETTER FOREIGN KEY
--	(
--	NEWSLETTER_ID
--	) REFERENCES dbo.TB_NEWSLETTER
--	(
--	NEWSLETTER_ID
--	) ON UPDATE  NO ACTION 
--	 ON DELETE  NO ACTION 
	
--GO
--ALTER TABLE dbo.TB_NEWSLETTER_CONTACT SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--select Has_Perms_By_Name(N'dbo.TB_NEWSLETTER_CONTACT', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_NEWSLETTER_CONTACT', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_NEWSLETTER_CONTACT', 'Object', 'CONTROL') as Contr_Per 

--------------







---- newsletter changes
---- row field in TB_CONTACT
---- st_ContactLogin
---- st_ContactDetails
---- st_SendNewsletterStatus
---- st_NewsletterStatus



--/*
--   18 June 201212:10:11
--   User: 
--   Server: db-epi
--   Database: Reviewer
--   Application: 
--*/

--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--BEGIN TRANSACTION
--SET QUOTED_IDENTIFIER ON
--SET ARITHABORT ON
--SET NUMERIC_ROUNDABORT OFF
--SET CONCAT_NULL_YIELDS_NULL ON
--SET ANSI_NULLS ON
--SET ANSI_PADDING ON
--SET ANSI_WARNINGS ON
--COMMIT
--BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.TB_CONTACT ADD
--	SEND_NEWSLETTER bit NOT NULL CONSTRAINT DF_TB_CONTACT_SEND_NEWSLETTER DEFAULT ((0))
--GO
--ALTER TABLE dbo.TB_CONTACT SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--select Has_Perms_By_Name(N'dbo.TB_CONTACT', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_CONTACT', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_CONTACT', 'Object', 'CONTROL') as Contr_Per 

-----------------


--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_SendNewsletterStatus]    Script Date: 06/18/2012 14:48:08 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

--CREATE procedure [dbo].[st_SendNewsletterStatus]
--(
--	@CONTACT_ID int,
--	@SEND_STATUS bit
--)

--As

--SET NOCOUNT ON


--	update Reviewer.dbo.TB_CONTACT 
--	set SEND_NEWSLETTER = @SEND_STATUS
--	where CONTACT_ID = @CONTACT_ID


--SET NOCOUNT OFF

--GO


--------------------

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_NewsletterStatus]    Script Date: 09/05/2012 14:44:03 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO






--CREATE PROCEDURE [dbo].[st_NewsletterStatus] 

--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--	declare @mostRecentNewsletterID int
--	declare @numRecipients int
--	declare @EPPIsupportID int
--	declare @numToBeSent int
	
--	select @EPPIsupportID = CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = 'EPPIsupport@ioe.ac.uk'
	
--	select top 1 @mostRecentNewsletterID = NEWSLETTER_ID from TB_NEWSLETTER
--	order by NEWSLETTER_ID desc
	
--	select @numRecipients = COUNT(CONTACT_ID) from TB_NEWSLETTER_CONTACT
--	where NEWSLETTER_ID = @mostRecentNewsletterID
--	and CONTACT_ID != @EPPIsupportID
--	and CONTACT_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where SEND_NEWSLETTER = 1)

--	declare @tb_newsletter_status table (numToBeSent int, numEligible int, numRecipients int)

--	insert into @tb_newsletter_status (numEligible)
--	select COUNT(CONTACT_ID) from Reviewer.dbo.TB_CONTACT
--	where SEND_NEWSLETTER = 1
--	and CONTACT_ID != @EPPIsupportID

--	update @tb_newsletter_status
--	set numRecipients = @numRecipients
	
--	update @tb_newsletter_status
--	set numToBeSent = (select COUNT(CONTACT_ID) from Reviewer.dbo.TB_CONTACT
--	where SEND_NEWSLETTER = 1
--	and CONTACT_ID != @EPPIsupportID
--	and CONTACT_ID not in (select CONTACT_ID from TB_NEWSLETTER_CONTACT
--	where NEWSLETTER_ID = @mostRecentNewsletterID))

--	select * from @tb_newsletter_status

    	
--	RETURN
--END





--GO

--------------------------------






--USE [ReviewerAdmin]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ContactLogin]    Script Date: 06/18/2012 12:19:28 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Jeff>
---- ALTER date: <24/03/10>
---- Description:	<gets contact details when loging in>
---- =============================================
--ALTER PROCEDURE [dbo].[st_ContactLogin] 
--(
--	@USERNAME nvarchar(50),
--	@PASSWORD nvarchar(50),
--	@IP_ADDRESS nvarchar(50)
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--	SELECT c.*, COUNT(sla.CONTACT_ID) as IsSLA
--	FROM Reviewer.dbo.TB_CONTACT c
--	Left outer join TB_SITE_LIC_ADMIN sla on sla.CONTACT_ID = c.CONTACT_ID
--	where c.USERNAME = @USERNAME
--	AND c.[PASSWORD] = @PASSWORD
--	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME,
--	c.PASSWORD, c.LAST_LOGIN, c.DATE_CREATED, c.EMAIL, c.EXPIRY_DATE,
--	c.MONTHS_CREDIT, c.CREATOR_ID,c.TYPE, c.IS_SITE_ADMIN, c.DESCRIPTION, c.SEND_NEWSLETTER
	
--	RETURN
--END

--------------------

--USE [ReviewerAdmin]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ContactDetails]    Script Date: 06/18/2012 14:37:10 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--ALTER PROCEDURE [dbo].[st_ContactDetails] 
--(
--	@CONTACT_ID nvarchar(50)
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
--    max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
--				CASE when l.[EXPIRY_DATE] is not null 
--				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
--					then l.[EXPIRY_DATE]
--				else c.[EXPIRY_DATE]
--				end as 'EXPIRY_DATE', 
--	c.MONTHS_CREDIT, c.CREATOR_ID,
--    c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], c.SEND_NEWSLETTER,
--    l.SITE_LIC_ID, l.SITE_LIC_NAME
--    ,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
--	from Reviewer.dbo.TB_CONTACT c
--	left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
--	on c.CONTACT_ID = lt.CONTACT_ID
--	left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
--	left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
--	where c.CONTACT_ID = @CONTACT_ID
	
--	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
--	c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID, c.SEND_NEWSLETTER,
--    c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME

--	RETURN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--END

--------------------





----BUG fix

--USE [ReviewerAdmin]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ContactActivate]    Script Date: 06/11/2012 14:17:45 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--ALTER procedure [dbo].[st_ContactActivate]
--(
--	@CONTACT_ID int
--)

--As

--SET NOCOUNT ON

--	declare @NUMBER_MONTHS int
--	declare @EXPIRY_DATE date
--	declare @creator_id int

--	select @NUMBER_MONTHS = MONTHS_CREDIT 
--	from Reviewer.dbo.TB_CONTACT
--	where CONTACT_ID = @CONTACT_ID
	
--	set @EXPIRY_DATE = dateadd(month,@NUMBER_MONTHS, sysdatetime())
	
--	update Reviewer.dbo.TB_CONTACT
--	set [EXPIRY_DATE] = @EXPIRY_DATE,
--	MONTHS_CREDIT = 0
--	where CONTACT_ID = @CONTACT_ID
	
--	select @creator_id = CREATOR_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID

--	-- add a line to TB_EXPIRY_EDIT_LOG to say the review has been activated
--	insert into ReviewerAdmin.dbo.TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, OLD_EXPIRY_DATE,
--		NEW_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
--	values (GETDATE(), 1, @CONTACT_ID, null, DATEADD(month, @NUMBER_MONTHS, GETDATE()), 
--		@creator_id, 13, 'The CreatorID activated the account')
	
	

--SET NOCOUNT OFF

--------------------------

---- BUG fix
--USE [ReviewerAdmin]
--GO
--/****** Object:  StoredProcedure [dbo].[st_GhostReviewActivate]    Script Date: 06/11/2012 10:29:11 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--ALTER PROCEDURE [dbo].[st_GhostReviewActivate]
--	-- Add the parameters for the stored procedure here
--	@revID int,
--	@Name Nvarchar(1000) = ''
	
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
	
--	declare @funderID int
--	declare @months_credit smallint

--    -- Insert statements for procedure here
--	if @Name = ''
--	begin
--		update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(month,MONTHS_CREDIT, GETDATE())
--		,REVIEW_NAME = 'Please Edit (ID=' & @revID & ')' where REVIEW_ID = @revID 
--	end
--	else
--	begin
--		update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(month,MONTHS_CREDIT, GETDATE())
--			,REVIEW_NAME = @Name where REVIEW_ID = @revID 
--	end
	
	
	
--	-- retrieve and then the months credit
--	select @months_credit = MONTHS_CREDIT from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @revID 
--	update Reviewer.dbo.TB_REVIEW set MONTHS_CREDIT = 0 where REVIEW_ID = @revID 

--	select @funderID = FUNDER_ID from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @revID

--	-- add a line to TB_EXPIRY_EDIT_LOG to say the review has been activated
--	insert into ReviewerAdmin.dbo.TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, OLD_EXPIRY_DATE,
--		NEW_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
--	values (GETDATE(), 0, @revID, null, DATEADD(month, @months_credit, GETDATE()), 
--		@funderID, 10, 'The FunderID activated the review')
	
	
--END

---------------------------------

----BUG fix
--USE [ReviewerAdmin]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ReviewAddMember]    Script Date: 06/11/2012 11:38:31 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--ALTER procedure [dbo].[st_ReviewAddMember]
--(
--	@REVIEW_ID int,
--	@CONTACT_ID int
--)

--As

--SET NOCOUNT ON

--	declare @NEW_CONTACT_REVIEW_ID int
--	declare @funderID int
	
--	insert into Reviewer.dbo.TB_REVIEW_CONTACT(CONTACT_ID, REVIEW_ID)
--	values (@CONTACT_ID, @REVIEW_ID)
	
--	set @NEW_CONTACT_REVIEW_ID = @@IDENTITY
	
--	insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE(REVIEW_CONTACT_ID, ROLE_NAME)
--	values(@NEW_CONTACT_REVIEW_ID, 'RegularUser')
	
--	-- if the contact_id of the invitee is the funderID then give them AdminUser role
--	select @funderID = FUNDER_ID from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @REVIEW_ID
--	if @funderID = @CONTACT_ID
--	begin
--		update Reviewer.dbo.TB_CONTACT_REVIEW_ROLE set ROLE_NAME = 'AdminUser' 
--		where REVIEW_CONTACT_ID = @NEW_CONTACT_REVIEW_ID
--	end
	
	

--SET NOCOUNT OFF

----------------------



---- copy review changes

-----------new tables
--TB_REVIEW_COPY_DATA

-----------edited tables
--TB_MANAGEMENT_SETTINGS


--/*
--   30 May 201212:16:26
--   User: 
--   Server: db-epi
--   Database: ReviewerAdmin
--   Application: 
--*/

--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--BEGIN TRANSACTION
--SET QUOTED_IDENTIFIER ON
--SET ARITHABORT ON
--SET NUMERIC_ROUNDABORT OFF
--SET CONCAT_NULL_YIELDS_NULL ON
--SET ANSI_NULLS ON
--SET ANSI_PADDING ON
--SET ANSI_WARNINGS ON
--COMMIT
--BEGIN TRANSACTION
--GO
--CREATE TABLE dbo.TB_REVIEW_COPY_DATA
--	(
--	REVIEW_COPY_DATA_ID int NOT NULL IDENTITY (1, 1),
--	NEW_REVIEW_ID int NULL,
--	OLD_OUTCOME_ID int NULL,
--	NEW_OUTCOME_ID int NULL
--	)  ON [PRIMARY]
--GO
--ALTER TABLE dbo.TB_REVIEW_COPY_DATA ADD CONSTRAINT
--	PK_TB_REVIEW_COPY_DATA PRIMARY KEY CLUSTERED 
--	(
--	REVIEW_COPY_DATA_ID
--	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

--GO
--ALTER TABLE dbo.TB_REVIEW_COPY_DATA SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--select Has_Perms_By_Name(N'dbo.TB_REVIEW_COPY_DATA', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_REVIEW_COPY_DATA', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_REVIEW_COPY_DATA', 'Object', 'CONTROL') as Contr_Per 

-----------------------

--/*
--   30 May 201212:11:02
--   User: 
--   Server: db-epi
--   Database: ReviewerAdmin
--   Application: 
--*/

--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--BEGIN TRANSACTION
--SET QUOTED_IDENTIFIER ON
--SET ARITHABORT ON
--SET NUMERIC_ROUNDABORT OFF
--SET CONCAT_NULL_YIELDS_NULL ON
--SET ANSI_NULLS ON
--SET ANSI_PADDING ON
--SET ANSI_WARNINGS ON
--COMMIT
--BEGIN TRANSACTION
--GO
--ALTER TABLE dbo.TB_MANAGEMENT_SETTINGS ADD
--	ENABLE_EXAMPLE_REVIEW_CREATION bit NULL,
--	EXAMPLE_NON_SHAREABLE_REVIEW_ID int NULL,
--	ENABLE_EXAMPLE_REVIEW_COPY bit NULL
--GO
--ALTER TABLE dbo.TB_MANAGEMENT_SETTINGS SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--select Has_Perms_By_Name(N'dbo.TB_MANAGEMENT_SETTINGS', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TB_MANAGEMENT_SETTINGS', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TB_MANAGEMENT_SETTINGS', 'Object', 'CONTROL') as Contr_Per 



---------sp new
----st_CopyReviewStep01
----st_CopyReviewStep03
----st_CopyReviewStep05
----st_CopyReviewStep07
----st_CopyReviewStep09
----st_CopyReviewStep11
----st_CopyReviewStep13
----st_CopyReviewStep15
----st_CopyReviewStepCleanup
----st_CountExampleReviews


-----------------

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep01]    Script Date: 05/30/2012 11:50:10 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO







--CREATE procedure [dbo].[st_CopyReviewStep01]
--(
--	@CONTACT_ID int,
--	@NEW_REVIEW_ID int output,
--	@RESULT nvarchar(50) output
--)

--As

--SET NOCOUNT ON


--BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
--BEGIN TRANSACTION


--declare @tb_review_name table (review_name1 nvarchar(255))
--declare @REVIEW_NAME nvarchar(1000) 
--declare @REVIEW_CONTACT_ID int
--set @RESULT = '0'

--insert into @tb_review_name (review_name1)select CONTACT_NAME from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID
--update @tb_review_name set review_name1 = review_name1 + '''s example non-shareable review'
--set @REVIEW_NAME = (select review_name1 from @tb_review_name)


----01 create new row in Reviewer.dbo.TB_REVIEW and get @NEW_REVIEW_ID
--	insert into Reviewer.dbo.TB_REVIEW (REVIEW_NAME, DATE_CREATED, FUNDER_ID)
--	values (@REVIEW_NAME, getdate(), @CONTACT_ID)
--	set @NEW_REVIEW_ID = CAST(SCOPE_IDENTITY() AS INT)

----02 create new row in Reviewer.dbo.TB_REVIEW_CONTACT
--	insert into Reviewer.dbo.TB_REVIEW_CONTACT (REVIEW_ID, CONTACT_ID)
--	values (@NEW_REVIEW_ID, @CONTACT_ID)
--	set @REVIEW_CONTACT_ID = CAST(SCOPE_IDENTITY() AS INT)
	
----03 create new row in Reviewer.dbo.TB_CONTACT_REVIEW_ROLE
--	insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
--	values (@REVIEW_CONTACT_ID, 'AdminUser')
--	--values ('abc', 'AdminUser')


--COMMIT TRANSACTION
--END TRY

--BEGIN CATCH
--IF (@@TRANCOUNT > 0)
--begin	
--ROLLBACK TRANSACTION
--set @RESULT = 'Unable to create the new review'
--end
--END CATCH


--RETURN

--SET NOCOUNT OFF










--GO

---------------------------

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep03]    Script Date: 05/30/2012 11:50:28 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO








--CREATE procedure [dbo].[st_CopyReviewStep03]
--(
--	@CONTACT_ID int,
--	@SOURCE_REVIEW_ID int,
--	@NEW_REVIEW_ID int,
--	@RESULT nvarchar(50) output
--)

--As

--SET NOCOUNT ON


--declare @NEW_ITEM_ID int
--declare @REVIEW_CONTACT_ID int
--declare @NEW_SOURCE_ID int
--set @RESULT = '0'

--declare @tb_item table (new_item_id int, [type_id] int, 
--TITLE nvarchar(4000), PARENT_TITLE nvarchar(4000), SHORT_TITLE nvarchar(70),
--DATE_CREATED datetime, CREATED_BY nvarchar(50), DATE_EDITED datetime,
--EDITED_BY nvarchar(50), YEAR nchar(4), MONTH varchar(10), STANDARD_NUMBER nvarchar(255),
--CITY nvarchar(100), COUNTRY nvarchar(100), PUBLISHER nvarchar(1000), INSTITUTION nvarchar(1000),
--VOLUME nvarchar(56), PAGES nvarchar(50), EDITION nvarchar(200), ISSUE nvarchar(100),
--IS_LOCAL bit, AVAILABILITY nvarchar(200), URL nvarchar(500), MASTER_ITEM_ID bigint,
--source_item_id int, ABSTRACT nvarchar(max), COMMENTS nvarchar(max))

--declare @tb_item_review table (new_item_id bigint, old_item_id bigint, is_included bit,
--new_master_item_id bigint, old_master_item_id bigint, is_deleted bit)

--declare @tb_item_author table (new_item_author_id bigint, new_item_id bigint, old_item_id bigint,
--LAST nvarchar(50), FIRST nvarchar(50), SECOND nvarchar(50), ROLE tinyint, RANK smallint)

--declare @tb_source table (old_source_id int, new_source_id int, source_name nvarchar(255),
--new_review_id int, is_deleted bit, date_of_search date, date_of_import date, source_database nvarchar(200),
--search_description nvarchar(4000), search_string nvarchar(max), notes nvarchar(4000), import_filter_id int)

--declare @tb_item_source table (old_item_source_id int, new_item_source_id int, old_item_id int, 
--new_item_id int, original_source_id int, dest_source_id int)


--BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
--BEGIN TRANSACTION


----01 get example review ID
--	--select @SOURCE_REVIEW_ID = EXAMPLE_NON_SHAREABLE_REVIEW_ID from TB_MANAGEMENT_SETTINGS
	
----02 get the item_id's to copy from Reviewer.dbo.TB_ITEM_REVIEW
--	insert into @tb_item ([type_id], source_item_id)
--	Select [TYPE_ID], i.ITEM_ID from Reviewer.dbo.TB_ITEM i
--	inner join Reviewer.dbo.TB_ITEM_REVIEW i_r on i_r.ITEM_ID = i.ITEM_ID
--	where i_r.REVIEW_ID = @SOURCE_REVIEW_ID

----03 user cursors to place new row in TB_ITEM, get the new ITEM_ID and update @tb_item
--	declare @WORKING_ITEM_ID int
--	declare ITEM_ID_CURSOR cursor for
--	select source_item_id FROM @tb_item
--	open ITEM_ID_CURSOR
--	fetch next from ITEM_ID_CURSOR
--	into @WORKING_ITEM_ID
--	while @@FETCH_STATUS = 0
--	begin
--		insert into Reviewer.dbo.TB_ITEM ([TYPE_ID], OLD_ITEM_ID)	
--		SELECT [type_id], source_item_id FROM @tb_item
--		WHERE source_item_id = @WORKING_ITEM_ID
--		set @NEW_ITEM_ID = CAST(SCOPE_IDENTITY() AS INT)
		
--		update @tb_item set new_item_id = @NEW_ITEM_ID
--		WHERE source_item_id = @WORKING_ITEM_ID

--		FETCH NEXT FROM ITEM_ID_CURSOR 
--		INTO @WORKING_ITEM_ID
--	END

--	CLOSE ITEM_ID_CURSOR
--	DEALLOCATE ITEM_ID_CURSOR

----04 insert into TB_ITEM_REVIEW based on @item_id
---- need to find correct bit for IS_DELETED and fill in MASTER_ITEM_ID
--	insert into @tb_item_review (old_item_id, is_included, old_master_item_id, is_deleted)
--	Select ITEM_ID, IS_INCLUDED, MASTER_ITEM_ID, IS_DELETED 
--	from Reviewer.dbo.TB_ITEM_REVIEW 
--	where REVIEW_ID = @SOURCE_REVIEW_ID

--	update t1
--	set t1.new_item_id = t2.new_item_id
--	from @tb_item_review t1 inner join @tb_item t2
--	on t2.source_item_id = t1.old_item_id
	
--	update t1
--	set t1.new_master_item_id = t2.new_item_id
--	from @tb_item_review t1 inner join @tb_item t2
--	on t2.source_item_id = t1.old_master_item_id

--	insert into Reviewer.dbo.TB_ITEM_REVIEW (ITEM_ID, REVIEW_ID, IS_INCLUDED, MASTER_ITEM_ID, IS_DELETED)
--	select new_item_id, @NEW_REVIEW_ID, is_included, new_master_item_id, is_deleted
--	from @tb_item_review


----05 insert into @tb_item_author and TB_ITEM_AUTHOR based on @item_id
--	insert into @tb_item_author (old_item_id, LAST, FIRST, SECOND, ROLE, RANK)
--	select ITEM_ID, LAST, FIRST, SECOND, ROLE, RANK from Reviewer.dbo.TB_ITEM_AUTHOR 
--	where ITEM_ID in 
--	 (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @SOURCE_REVIEW_ID)

--	update @tb_item_author
--	set new_item_id = i.new_item_id
--	from @tb_item i
--	where old_item_id = i.source_item_id

--	insert into Reviewer.dbo.TB_ITEM_AUTHOR (ITEM_ID, LAST, FIRST, SECOND, ROLE, RANK)
--	select new_item_id, LAST, FIRST, SECOND, ROLE, RANK from @tb_item_author

	
----06 update missing info in TB_ITEM
--	update @tb_item
--	set TITLE = i.TITLE, PARENT_TITLE = i.PARENT_TITLE, SHORT_TITLE = i.SHORT_TITLE,
--	 DATE_CREATED = i.DATE_CREATED, CREATED_BY = i.CREATED_BY, DATE_EDITED = i.DATE_EDITED, 
--	 EDITED_BY = i.EDITED_BY, YEAR = i.YEAR, MONTH = i.MONTH, STANDARD_NUMBER = i.STANDARD_NUMBER,
--	 CITY = i.CITY, COUNTRY = i.COUNTRY, PUBLISHER = i.PUBLISHER, INSTITUTION = i.INSTITUTION, 
--	 VOLUME = i.VOLUME, PAGES = i.PAGES, EDITION = i.EDITION, ISSUE = i.ISSUE, IS_LOCAL = i.IS_LOCAL, 
--	 AVAILABILITY = i.AVAILABILITY, URL = i.URL, MASTER_ITEM_ID = i.MASTER_ITEM_ID, 
--	 ABSTRACT = i.ABSTRACT, COMMENTS = i.COMMENTS
--	from Reviewer.dbo.TB_ITEM i
--	where source_item_id = i.ITEM_ID
	
--	update Reviewer.dbo.TB_ITEM
--	set TITLE = tb_i.TITLE, PARENT_TITLE = tb_i.PARENT_TITLE, SHORT_TITLE = tb_i.SHORT_TITLE,
--	 DATE_CREATED = tb_i.DATE_CREATED, CREATED_BY = tb_i.CREATED_BY, DATE_EDITED = tb_i.DATE_EDITED, 
--	 EDITED_BY = tb_i.EDITED_BY, YEAR = tb_i.YEAR, MONTH = tb_i.MONTH, STANDARD_NUMBER = tb_i.STANDARD_NUMBER,
--	 CITY = tb_i.CITY, COUNTRY = tb_i.COUNTRY, PUBLISHER = tb_i.PUBLISHER, INSTITUTION = tb_i.INSTITUTION, 
--	 VOLUME = tb_i.VOLUME, PAGES = tb_i.PAGES, EDITION = tb_i.EDITION, ISSUE = tb_i.ISSUE, IS_LOCAL = tb_i.IS_LOCAL, 
--	 AVAILABILITY = tb_i.AVAILABILITY, URL = tb_i.URL, MASTER_ITEM_ID = tb_i.MASTER_ITEM_ID, 
--	 ABSTRACT = tb_i.ABSTRACT, COMMENTS = tb_i.COMMENTS
--	from @tb_item tb_i
--	where new_item_id = ITEM_ID

----07 collect the old source data
--	insert into @tb_source (old_source_id, source_name, new_review_id, is_deleted,
--	date_of_search, date_of_import, source_database, search_description, 
--	search_string, notes, import_filter_id)
--	select SOURCE_ID, SOURCE_NAME, @NEW_REVIEW_ID, IS_DELETED, DATE_OF_SEARCH, DATE_OF_IMPORT,
--	SOURCE_DATABASE, SEARCH_DESCRIPTION, SEARCH_STRING, NOTES, IMPORT_FILTER_ID
--	from Reviewer.dbo.TB_SOURCE s
--	where s.REVIEW_ID = @SOURCE_REVIEW_ID


----08 create new rows in TB_SOURCE
--	declare @WORKING_SOURCE_ID int
--	declare SOURCE_ID_CURSOR cursor for
--	select old_source_id FROM @tb_source
--	open SOURCE_ID_CURSOR
--	fetch next from SOURCE_ID_CURSOR
--	into @WORKING_SOURCE_ID
--	while @@FETCH_STATUS = 0
--	begin
--		insert into Reviewer.dbo.TB_SOURCE (SOURCE_NAME, REVIEW_ID, 
--			IS_DELETED, DATE_OF_SEARCH, DATE_OF_IMPORT, SOURCE_DATABASE, 
--			SEARCH_DESCRIPTION, SEARCH_STRING, NOTES, IMPORT_FILTER_ID)	
--		SELECT source_name, new_review_id, is_deleted,
--			date_of_search, date_of_import, source_database, search_description, 
--			search_string, notes, import_filter_id FROM @tb_source
--			WHERE old_source_id = @WORKING_SOURCE_ID
--		set @NEW_SOURCE_ID = CAST(SCOPE_IDENTITY() AS INT)
		
--		update @tb_source set new_source_id = @NEW_SOURCE_ID
--		WHERE old_source_id = @WORKING_SOURCE_ID

--		FETCH NEXT FROM SOURCE_ID_CURSOR 
--		INTO @WORKING_SOURCE_ID
--	END

--	CLOSE SOURCE_ID_CURSOR
--	DEALLOCATE SOURCE_ID_CURSOR

--	--select * from @tb_source

----09 collect the old item_source data
--	insert into @tb_item_source (old_item_source_id, old_item_id, original_source_id)
--	select ITEM_SOURCE_ID, ITEM_ID, SOURCE_ID
--	from Reviewer.dbo.TB_ITEM_SOURCE i_s
--	where i_s.SOURCE_ID in (select old_source_id from @tb_source)
	
--	--select * from @tb_item_source

----10 update @tb_item_source with the new_source_id and new_item_id
--	update t1
--	set dest_source_id = t2.new_source_id
--	from @tb_item_source t1 inner join @tb_source t2
--	on t2.old_source_id = t1.original_source_id
	
--	--select * from @tb_item_source
	
--	update t1
--	set new_item_id = t2.ITEM_ID
--	from @tb_item_source t1 inner join Reviewer.dbo.TB_ITEM t2
--	on t2.OLD_ITEM_ID = t1.old_item_id
--	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
--	where i_r.REVIEW_ID = @NEW_REVIEW_ID)
		
--	--select * from @tb_item_source
	
----11 place new rows in TB_ITEM_SOURCE based on tb_item_source
--	insert into Reviewer.dbo.TB_ITEM_SOURCE (ITEM_ID, SOURCE_ID)
--	select new_item_id, dest_source_id from @tb_item_source


--COMMIT TRANSACTION
--END TRY

--BEGIN CATCH
--IF (@@TRANCOUNT > 0)
--begin	
--ROLLBACK TRANSACTION
--set @RESULT = 'Unable to copy items and place items in new review'
--end
--END CATCH


--RETURN

--SET NOCOUNT OFF











--GO

-------------------------
--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep05]    Script Date: 05/30/2012 11:50:46 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO









--CREATE procedure [dbo].[st_CopyReviewStep05]
--(
--	@CONTACT_ID int,
--	@SOURCE_REVIEW_ID int,
--	@NEW_REVIEW_ID int,
--	@RESULT nvarchar(50) output
--)

--As

--SET NOCOUNT ON


--declare @NEW_ITEM_DUPLICATE_GROUP_ID int
--declare @NEW_GROUP_MEMBER_ID int
--set @RESULT = '0'

---- 1. create new row in TB_ITEM_DUPLICATE_GROUP to create a new ITEM_DUPLICATE_GROUP_ID
---- 2. create new row in TB_ITEM_DUPLICATE_GROUP_MEMBERS to create a new GROUP_MEMBER_ID
---- 3. use GROUP_MEMBER_ID as MASTER_MEMBER_ID back in TB_ITEM_DUPLICATE_GROUP

--declare @tb_item_duplicate_group table (new_item_duplicate_group_id int, old_item_duplicate_group_id int,
--new_master_member_id int, old_master_member_id int, review_id int, new_original_item_id bigint,
--old_original_item_id bigint)

--declare @tb_item_duplicate_group_members table (new_group_member_id int, old_group_member_id int,
--new_item_duplicate_group_id int, old_item_duplicate_group_id int, new_item_review_id bigint,
--old_item_review_id bigint, new_item_id bigint, old_item_id bigint, score float, is_checked bit, is_duplicate bit)


--BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
--BEGIN TRANSACTION

----01 collect the data for @tb_item_duplicate_group
--	insert into @tb_item_duplicate_group (old_item_duplicate_group_id, old_master_member_id,
--	review_id, old_original_item_id)
--	select i_d_g.ITEM_DUPLICATE_GROUP_ID, i_d_g.MASTER_MEMBER_ID, @NEW_REVIEW_ID,
--	i_d_g.ORIGINAL_ITEM_ID from Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP i_d_g
--	where i_d_g.REVIEW_ID = @SOURCE_REVIEW_ID

--	update t1
--	set t1.new_original_item_id = t2.ITEM_ID
--	from @tb_item_duplicate_group t1 inner join Reviewer.dbo.TB_ITEM t2
--	on t2.OLD_ITEM_ID = t1.old_original_item_id
--	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
--	where i_r.REVIEW_ID = @NEW_REVIEW_ID)
	
----02 collect the data for @tb_item_duplicate_group_members
--	insert into @tb_item_duplicate_group_members (old_group_member_id, old_item_duplicate_group_id,
--	old_item_review_id, score, is_checked, is_duplicate)
--	select i_d_g_m.GROUP_MEMBER_ID, i_d_g_m.ITEM_DUPLICATE_GROUP_ID, i_d_g_m.ITEM_REVIEW_ID,
--	i_d_g_m.SCORE, i_d_g_m.IS_CHECKED, i_d_g_m.IS_DUPLICATE from Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP_MEMBERS i_d_g_m
--	where i_d_g_m.ITEM_DUPLICATE_GROUP_ID in (select old_item_duplicate_group_id from @tb_item_duplicate_group)
	
--	update t1
--	set t1.old_item_id = t2.ITEM_ID
--	from @tb_item_duplicate_group_members t1 inner join Reviewer.dbo.TB_ITEM_REVIEW t2
--	on t1.old_item_review_id = t2.ITEM_REVIEW_ID
	
--	update t1
--	set t1.new_item_id = t2.ITEM_ID
--	from @tb_item_duplicate_group_members t1 inner join Reviewer.dbo.TB_ITEM t2
--	on t2.OLD_ITEM_ID = t1.old_item_id
--	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
--	where i_r.REVIEW_ID = @NEW_REVIEW_ID)
	
--	update t1
--	set new_item_review_id = t2.ITEM_REVIEW_ID
--	from @tb_item_duplicate_group_members t1 inner join Reviewer.dbo.TB_ITEM_REVIEW t2
--	on t2.ITEM_ID = t1.new_item_id
--	where t2.REVIEW_ID = @NEW_REVIEW_ID
	
----03 create new rows in Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP
--	declare @WORKING_ITEM_DUPLICATE_GROUP_ID int
--	declare ITEM_DUPLICATE_GROUP_ID_CURSOR cursor for
--	select old_item_duplicate_group_id FROM @tb_item_duplicate_group
--	open ITEM_DUPLICATE_GROUP_ID_CURSOR
--	fetch next from ITEM_DUPLICATE_GROUP_ID_CURSOR
--	into @WORKING_ITEM_DUPLICATE_GROUP_ID
--	while @@FETCH_STATUS = 0
--	begin
--		insert into Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP (REVIEW_ID, ORIGINAL_ITEM_ID)	
--		SELECT @NEW_REVIEW_ID, old_original_item_id FROM @tb_item_duplicate_group
--		WHERE old_item_duplicate_group_id = @WORKING_ITEM_DUPLICATE_GROUP_ID
--		set @NEW_ITEM_DUPLICATE_GROUP_ID = CAST(SCOPE_IDENTITY() AS INT)
		
--		update @tb_item_duplicate_group
--		set new_item_duplicate_group_id = @NEW_ITEM_DUPLICATE_GROUP_ID
--		where old_item_duplicate_group_id = @WORKING_ITEM_DUPLICATE_GROUP_ID
		
--		update @tb_item_duplicate_group_members 
--		set new_item_duplicate_group_id = @NEW_ITEM_DUPLICATE_GROUP_ID
--		WHERE old_item_duplicate_group_id = @WORKING_ITEM_DUPLICATE_GROUP_ID

--		FETCH NEXT FROM ITEM_DUPLICATE_GROUP_ID_CURSOR 
--		INTO @WORKING_ITEM_DUPLICATE_GROUP_ID
--	END

--	CLOSE ITEM_DUPLICATE_GROUP_ID_CURSOR
--	DEALLOCATE ITEM_DUPLICATE_GROUP_ID_CURSOR

	
----04 create new rows in Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP_MEMBERS
--	declare @WORKING_GROUP_MEMBER_ID int
--	declare GROUP_MEMBER_ID_CURSOR cursor for
--	select old_group_member_id FROM @tb_item_duplicate_group_members
--	open GROUP_MEMBER_ID_CURSOR
--	fetch next from GROUP_MEMBER_ID_CURSOR
--	into @WORKING_GROUP_MEMBER_ID
--	while @@FETCH_STATUS = 0
--	begin
--		insert into Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP_MEMBERS (ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID,
--		SCORE, IS_CHECKED, IS_DUPLICATE)	
--		SELECT new_item_duplicate_group_id, new_item_review_id, score, is_checked, is_duplicate
--		FROM @tb_item_duplicate_group_members
--		WHERE old_group_member_id = @WORKING_GROUP_MEMBER_ID
--		set @NEW_GROUP_MEMBER_ID = CAST(SCOPE_IDENTITY() AS INT)
		
--		update @tb_item_duplicate_group_members 
--		set new_group_member_id = @NEW_GROUP_MEMBER_ID
--		where old_group_member_id = @WORKING_GROUP_MEMBER_ID
		
--		FETCH NEXT FROM GROUP_MEMBER_ID_CURSOR 
--		INTO @WORKING_GROUP_MEMBER_ID
--	END

--	CLOSE GROUP_MEMBER_ID_CURSOR
--	DEALLOCATE GROUP_MEMBER_ID_CURSOR
	

--	update t1
--	set t1.new_master_member_id = t2.new_group_member_id
--	from @tb_item_duplicate_group t1 inner join @tb_item_duplicate_group_members t2
--	on t1.new_item_duplicate_group_id = t2.new_item_duplicate_group_id
--	where t2.is_duplicate = 0


----05 update Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP with the new MASTER_MEMBER_ID	
--	update t1
--	set t1.MASTER_MEMBER_ID = t2.new_master_member_id
--	from Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP t1 inner join @tb_item_duplicate_group t2
--	on t1.ITEM_DUPLICATE_GROUP_ID = t2.new_item_duplicate_group_id
	
	

--COMMIT TRANSACTION
--END TRY

--BEGIN CATCH
--IF (@@TRANCOUNT > 0)
--begin	
--ROLLBACK TRANSACTION
--set @RESULT = 'Unable to copy the duplicate information'
--end
--END CATCH


--RETURN

--SET NOCOUNT OFF












--GO

--------------------------------
--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep07]    Script Date: 05/30/2012 11:51:01 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


---- =============================================
---- Author:		<Jeff>
---- ALTER date: <>
---- Description:	<gets contact table for a contactID>
---- =============================================
--CREATE PROCEDURE [dbo].[st_CopyReviewStep07] 
--(
--	@SOURCE_REVIEW_ID int
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
	
----01 get the set_ids	
--    select * 
--	from Reviewer.dbo.TB_SET s
--	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
--	where r_s.REVIEW_ID = @SOURCE_REVIEW_ID

    	
--	RETURN
--END


--GO

-------------------------
--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep09]    Script Date: 05/30/2012 11:51:18 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO









--CREATE procedure [dbo].[st_CopyReviewStep09]
--(	
--	@CONTACT_ID int,
--	@SOURCE_REVIEW_ID int,
--	@DESTINATION_REVIEW_ID int,
--	@SOURCE_SET_ID int,
--	@RESULT nvarchar(50) output
--)

--As

--SET NOCOUNT ON


--declare @DESTINATION_SET_ID int
--declare @DESTINATION_REVIEW_SET_ID int
--declare @DESTINATION_SET_NAME nvarchar (255)
----declare @SOURCE_REVIEW_ID int
--set @RESULT = '0'


--BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
--BEGIN TRANSACTION


----00 get example review ID
--	--select @SOURCE_REVIEW_ID = EXAMPLE_NON_SHAREABLE_REVIEW_ID from TB_MANAGEMENT_SETTINGS

----01 create new row in Reviewer.dbo.TB_SET and get @DESTINATION_SET_ID
--	insert into Reviewer.dbo.TB_SET (SET_TYPE_ID, SET_NAME, OLD_GUIDELINE_ID)
--	select s.SET_TYPE_ID, s.SET_NAME, @SOURCE_SET_ID from Reviewer.dbo.TB_SET s
--	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
--	where s.SET_ID = @SOURCE_SET_ID
--	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID
--	set @DESTINATION_SET_ID = CAST(SCOPE_IDENTITY() AS INT)



----02 create new row in Reviewer.dbo.TB_REVIEW_SET and get @DESTINATION_REVIEW_SET_ID
--	insert into Reviewer.dbo.TB_REVIEW_SET (REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL)
--	values (@DESTINATION_REVIEW_ID, @DESTINATION_SET_ID, 1, 1)
--	set @DESTINATION_REVIEW_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

--	-- @attribute_id
--	declare @attribute_id table (attribute_id int, ok int)

----03 get source ATTRIBUTE_ID's and put in tmp table @attribute_id (avoids orphans)	
--	insert into @attribute_id 
--	Select a_s.ATTRIBUTE_ID, Reviewer.dbo.fn_IsAttributeInTree(a_s.ATTRIBUTE_ID ) as ok 
--	from Reviewer.dbo.TB_ATTRIBUTE_SET a_s 
--	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
--	where a_s.SET_ID = @SOURCE_SET_ID
--	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID 
	
--	--A
--	--select * from @attribute_id
	
--	-- @tb_attribute
--	declare @tb_attribute table 
--	(ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))
	
----04 use @attribute_id to get source data from TB_ATTRIBUTE and put in tmp table @tb_attribute
--	insert into @tb_attribute (ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
--		select ATTRIBUTE_ID, @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, ATTRIBUTE_ID -- use ATTRIBUTE_ID as OLD_ATTRIBUTE_ID
--		from Reviewer.dbo.TB_ATTRIBUTE
--		where ATTRIBUTE_ID in 
--		(select attribute_id from @attribute_id)

--	--B
--	--select * from @tb_attribute

--	-- @tb_attribute_set
--	declare @tb_attribute_set table 
--	(ATTRIBUTE_SET_ID int, ATTRIBUTE_ID int, SET_ID int, PARENT_ATTRIBUTE_ID int, ATTRIBUTE_TYPE_ID int, 
--	ATTRIBUTE_SET_DESC nvarchar(max), ATTRIBUTE_ORDER int)

----05 use @attribute_id to get source data from TB_ATTRIBUTE_SET and put in tmp table @tb_attribute_set	
--	insert into @tb_attribute_set (ATTRIBUTE_SET_ID, ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)
--		select ATTRIBUTE_SET_ID, ATTRIBUTE_ID, @DESTINATION_SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
--		from Reviewer.dbo.TB_ATTRIBUTE_SET
--		where ATTRIBUTE_ID in 
--		(select attribute_id from @attribute_id)

--	--C
--	--select * from @tb_attribute_set

----06 put @tb_attribute into Reviewer.dbo.TB_ATTRIBUTE
--	insert into Reviewer.dbo.TB_ATTRIBUTE (CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)	
--		select @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID 
--		from @tb_attribute

--	-- @new_tb_attribute
--	declare @new_tb_attribute table 
--	(NEW_ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))

----07 get the new rows from Reviewer.dbo.TB_ATTRIBUTE and put them into @new_tb_attribute
--	insert into @new_tb_attribute (NEW_ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
--		select ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID  
--		from Reviewer.dbo.TB_ATTRIBUTE
--		where OLD_ATTRIBUTE_ID in -- old_attribute_id will identify the items we want
--		(select attribute_id from @attribute_id) --does @attribute_id have bad data?
--		and OLD_ATTRIBUTE_ID not like 'AT%'
--		and OLD_ATTRIBUTE_ID not like 'EX%'
--		and OLD_ATTRIBUTE_ID not like 'IC%'
--		--and ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
--		--where SET_ID = @DESTINATION_SET_ID)
		
----- we need to restrict this to the correct set but we haven't populated
----- Reviewer.dbo.TB_ATTRIBUTE_SET yet!!!!!!!!!!!!!!!!!		
		

--	--D
--	--select * from @new_tb_attribute


----09 update @tb_attribute_set with the new ATTRIBUTE_IDs sitting in @new_tb_attribute
--	update @tb_attribute_set 
--	set ATTRIBUTE_ID = 
--	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
--	where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
--	where exists
--	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
--		where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
	
	
--	--E
--	--select * from @tb_attribute_set


----08 update @tb_attribute_set with a new PARENT_ATTRIBUTE_ID
--	-- the new PARENT_ATTRIBUTE_ID is the 
--	update @tb_attribute_set
--	set PARENT_ATTRIBUTE_ID = 
--	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
--	where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID
--	and PARENT_ATTRIBUTE_ID != 0)
--	where exists
--	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
--		where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID and PARENT_ATTRIBUTE_ID != 0)
	
	
--	-- clean up the nulls
--	update @tb_attribute_set
--	set PARENT_ATTRIBUTE_ID = 0
--	where PARENT_ATTRIBUTE_ID is null
	
--	--F
--	--select * from @tb_attribute_set
	


----10 put @tb_attribute_set into Reviewer.dbo.TB_ATTRIBUTE_SET
--	insert into Reviewer.dbo.TB_ATTRIBUTE_SET (ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)	
--		select ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
--		from @tb_attribute_set

----11 place the new codeset at the bottom of the list.
--	declare @number_sets int set @number_sets = 0
--	select @number_sets = COUNT(*) from [Reviewer].[dbo].[TB_REVIEW_SET]
--	where REVIEW_ID = @DESTINATION_REVIEW_ID
--	-- set the position
--	update [Reviewer].[dbo].[TB_REVIEW_SET] 
--	set SET_ORDER = @number_sets - 1
--	where REVIEW_ID = @DESTINATION_REVIEW_ID
--	and SET_ID = @DESTINATION_SET_ID


---- keep the old_attribute_id values as we need to copy over the data
--/*
----12 Clear out OLD_ATTRIBUTE_ID from TB_ATTRIBUTE in the new codeset since
--	update Reviewer.dbo.TB_ATTRIBUTE
--	set OLD_ATTRIBUTE_ID = null
--	where ATTRIBUTE_ID in 
--	(select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
--		where SET_ID = @DESTINATION_SET_ID)
--*/


--COMMIT TRANSACTION
--END TRY

--BEGIN CATCH
--IF (@@TRANCOUNT > 0)
--begin	
--ROLLBACK TRANSACTION
--set @RESULT = 'Unable to set up copies of the source codesets in the new review'
--end
--END CATCH


--RETURN

--SET NOCOUNT OFF












--GO

--------------------------
--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep11]    Script Date: 05/30/2012 11:51:33 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO










--CREATE procedure [dbo].[st_CopyReviewStep11]
--(	
--	@CONTACT_ID int,
--	@SOURCE_REVIEW_ID int,
--	@DESTINATION_REVIEW_ID int,
--	@SOURCE_SET_ID int,
--	@RESULT nvarchar(50) output
--)

--As

--SET NOCOUNT ON


--declare @DESTINATION_SET_ID int
--declare @DESTINATION_REVIEW_SET_ID int
--declare @DESTINATION_SET_NAME nvarchar (255)
--declare @NEW_OUTCOME_ID int
----declare @SOURCE_REVIEW_ID int
--declare @NEW_SET_ID int
--declare @NEW_ITEM_SET_ID int
--set @RESULT = '0'


--declare @tb_item_attribute table
--(NEW_ITEM_ATTRIBUTE_ID bigint,  OLD_ITEM_ATTRIBUTE_ID bigint,
-- NEW_ITEM_ID bigint,  OLD_ITEM_ID bigint,  NEW_ITEM_SET_ID bigint,
-- OLD_ITEM_SET_ID bigint,  NEW_ATTRIBUTE_ID bigint,
-- ORIGINAL_ATTRIBUTE_ID nvarchar(50), -- needs to be nvarchar to deal with ER3 attributes
-- ADDITIONAL_TEXT nvarchar(max))
 
--declare @tb_item_set table
--(NEW_ITEM_SET_ID bigint,  EXAMPLE_ITEM_SET_ID bigint,
-- NEW_ITEM_ID bigint,  EXAMPLE_ITEM_ID bigint,
-- EXAMPLE_IS_COMPLETED bit,  NEW_CONTACT_ID int,
-- EXAMPLE_IS_LOCKED bit)
 
--declare @tb_new_items table
-- (ITEM_ID bigint,
-- SOURCE_ITEM_ID bigint)
 
--declare @tb_item_outcome table
--(new_outcome_id int,  old_outcome_id int,
-- new_item_set_id int,  old_item_set_id int,
-- outcome_type_id int,
-- new_item_attribute_id_intervention bigint,  old_item_attribute_id_intervention bigint,
-- new_item_attribute_id_control bigint,  old_item_attribute_id_control bigint,
-- new_item_attribute_id_outcome bigint,  old_item_attribute_id_outcome nvarchar(255),
-- outcome_title nvarchar(255),
-- data1 float, data2 float, data3 float, data4 float,
-- data5 float, data6 float, data7 float, data8 float,
-- data9 float, data10 float, data11 float, data12 float,
-- data13 float, data14 float,
-- outcome_description nvarchar(4000))

--declare @tb_item_outcome_attribute table
--(new_item_outcome_attribute_id int,  old_item_outcome_attribute_id int,
-- new_outcome_id int,  old_outcome_id int,
-- new_attribute_id bigint,  old_attribute_id bigint,
-- additional_text nvarchar(max))


--BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
--BEGIN TRANSACTION


----01 get a few variables
--	--select @SOURCE_REVIEW_ID = EXAMPLE_NON_SHAREABLE_REVIEW_ID from TB_MANAGEMENT_SETTINGS
--	select @NEW_SET_ID = SET_ID from Reviewer.dbo.TB_SET 
--	where OLD_GUIDELINE_ID = @SOURCE_SET_ID
--	and SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET where REVIEW_ID = @DESTINATION_REVIEW_ID)

----02 get old data from Reviewer.dbo.TB_ITEM_SET and place in @tb_item_set
--	insert into @tb_item_set (EXAMPLE_ITEM_SET_ID, EXAMPLE_ITEM_ID, EXAMPLE_IS_COMPLETED, NEW_CONTACT_ID, EXAMPLE_IS_LOCKED)
--	select ITEM_SET_ID, ITEM_ID, IS_COMPLETED, CONTACT_ID, IS_LOCKED 
--	from Reviewer.dbo.TB_ITEM_SET 
--	where SET_ID = @SOURCE_SET_ID
	
--	--select * from @tb_item_set -- 1 OK
	
----03 sets up a list of the new items so we don't get cross-review contamination
--	-- seems like an extra step but has little overhead and makes it easier
--	-- for me to follow
--	insert into @tb_new_items (ITEM_ID, SOURCE_ITEM_ID)
--	select ITEM_ID, OLD_ITEM_ID from Reviewer.dbo.TB_ITEM where ITEM_ID in 
--	(select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
--	--select * from @tb_new_items -- 2 OK
	
----04 update @tb_item_set with the new item_id  
--	update @tb_item_set
--	set NEW_ITEM_ID = ITEM_ID
--	from @tb_new_items
--	where EXAMPLE_ITEM_ID = SOURCE_ITEM_ID
	
--	--select * from @tb_item_set -- 3 OK

----05 get old data from Reviewer.dbo.TB_ITEM_ATTRIBUTE and place in @tb_item_attribute
--	insert into @tb_item_attribute (OLD_ITEM_ATTRIBUTE_ID, OLD_ITEM_ID, OLD_ITEM_SET_ID,
--	 ORIGINAL_ATTRIBUTE_ID, ADDITIONAL_TEXT)
--	select i_a.ITEM_ATTRIBUTE_ID, i_a.ITEM_ID, i_a.ITEM_SET_ID, i_a.ATTRIBUTE_ID, i_a.ADDITIONAL_TEXT  
--	from Reviewer.dbo.TB_ITEM_ATTRIBUTE i_a
--	inner join Reviewer.dbo.TB_ITEM_SET i_s on i_s.ITEM_SET_ID = i_a.ITEM_SET_ID
--	where i_s.SET_ID = @SOURCE_SET_ID
	
--	--select * from @tb_item_attribute -- 4 OK

----06 user cursors to place new row in TB_ITEM_SET, get the new ITEM_SET_ID and update @tb_item_set
--	declare @WORKING_ITEM_ID int
--	declare ITEM_ID_CURSOR cursor for
--	select NEW_ITEM_ID FROM @tb_item_set
--	open ITEM_ID_CURSOR
--	fetch next from ITEM_ID_CURSOR
--	into @WORKING_ITEM_ID
--	while @@FETCH_STATUS = 0
--	begin
--		-- set CONTACT_ID to newly created user account (rather than orginal coder)
--		-- set IS_LOCKED to 0 as we want new user to experiment with the coding
--		insert into Reviewer.dbo.TB_ITEM_SET (ITEM_ID, SET_ID, CONTACT_ID, IS_LOCKED)	
--		SELECT NEW_ITEM_ID, @NEW_SET_ID, @CONTACT_ID, 0 FROM @tb_item_set 
--		WHERE NEW_ITEM_ID = @WORKING_ITEM_ID
--		set @NEW_ITEM_SET_ID = CAST(SCOPE_IDENTITY() AS INT)
		
--		update @tb_item_set set new_item_set_id = @NEW_ITEM_SET_ID
--		WHERE NEW_ITEM_ID = @WORKING_ITEM_ID

--		FETCH NEXT FROM ITEM_ID_CURSOR 
--		INTO @WORKING_ITEM_ID
--	END

--	CLOSE ITEM_ID_CURSOR
--	DEALLOCATE ITEM_ID_CURSOR
	
--	--select * from Reviewer.dbo.TB_ITEM_SET where ITEM_ID in -- 5 OK
--	--(select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
--	--select * from @tb_item_set -- 6 OK

----07 update @tb_item_attribute with NEW_ITEM_ID, NEW_ITEM_SET_ID, NEW_ATTRIBUTE_ID
--	update @tb_item_attribute
--	set NEW_ITEM_ID = 
--	(select NEW_ITEM_ID from @tb_item_set
--	where EXAMPLE_ITEM_ID = OLD_ITEM_ID)
	
--	--select * from @tb_item_attribute -- 7 OK
	
--	update @tb_item_attribute
--	set NEW_ITEM_SET_ID = 
--	(select NEW_ITEM_SET_ID from @tb_item_set
--	where EXAMPLE_ITEM_SET_ID = OLD_ITEM_SET_ID)
	
--	--select * from @tb_item_attribute -- 8 OK
	
--	update @tb_item_attribute
--	set NEW_ATTRIBUTE_ID = 
--	(select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE a
--	where a.OLD_ATTRIBUTE_ID = ORIGINAL_ATTRIBUTE_ID)
	
--	--select * from @tb_item_attribute -- 9 OK

----08 place new rows in TB_ITEM_ATTRIBUTE based on @tb_item_attribute
--	insert into Reviewer.dbo.TB_ITEM_ATTRIBUTE (ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT)
--	select tb_i_a.NEW_ITEM_ID, tb_i_a.NEW_ITEM_SET_ID, tb_i_a.NEW_ATTRIBUTE_ID, tb_i_a.ADDITIONAL_TEXT 
--	from @tb_item_attribute tb_i_a
	
----09 update TB_ITEM_SET with IS_COMPLETED values in @tb_item_set	
--	UPDATE Reviewer.dbo.TB_ITEM_SET
--	SET IS_COMPLETED = i_s.EXAMPLE_IS_COMPLETED
--	FROM @tb_item_set i_s
--	where i_s.NEW_ITEM_SET_ID = ITEM_SET_ID
--	and i_s.NEW_ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW -- to restrict it to items in new review
--	where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
--	--select * from Reviewer.dbo.TB_ITEM_SET where ITEM_ID in -- 10 OK
--	--(select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
----10 fill up @tb_item_outcome
--	insert into @tb_item_outcome (old_outcome_id, old_item_set_id, outcome_type_id,
--	 old_item_attribute_id_intervention, old_item_attribute_id_control,
--	 old_item_attribute_id_outcome, outcome_title, data1, data2, data3, data4, data5,
--	 data6, data7, data8, data9, data10, data11, data12, data13,
--	 data14, outcome_description)
--	select i_c.OUTCOME_ID, i_c.ITEM_SET_ID, i_c.OUTCOME_TYPE_ID, i_c.ITEM_ATTRIBUTE_ID_INTERVENTION,
--	i_c.ITEM_ATTRIBUTE_ID_CONTROL, i_c.ITEM_ATTRIBUTE_ID_OUTCOME, i_c.OUTCOME_TITLE,
--	i_c.DATA1, i_c.DATA2, i_c.DATA3, i_c.DATA4, i_c.DATA5, i_c.DATA6, i_c.DATA7,
--	i_c.DATA8, i_c.DATA9, i_c.DATA10, i_c.DATA11, i_c.DATA12, i_c.DATA13, i_c.DATA14,
--	i_c.OUTCOME_DESCRIPTION
--	from Reviewer.dbo.TB_ITEM_OUTCOME i_c 
--	inner join Reviewer.dbo.TB_ITEM_SET i_s on i_s.ITEM_SET_ID = i_c.ITEM_SET_ID
--	where i_s.SET_ID = @SOURCE_SET_ID
	
----11 update @tb_item_outcome with the 'new_' values	
--	update t1 --new_item_set_id
--	set new_item_set_id = t2.NEW_ITEM_SET_ID
--	from @tb_item_outcome t1 inner join @tb_item_set t2
--	on t2.EXAMPLE_ITEM_SET_ID = t1.old_item_set_id
	
--	update t1 --new_item_attribute_id_intervention
--	set new_item_attribute_id_intervention = t2.ATTRIBUTE_ID
--	from @tb_item_outcome t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
--	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_intervention
--	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
--	where SET_ID = @NEW_SET_ID)
	
--	update t1 --new_item_attribute_id_control
--	set new_item_attribute_id_control = t2.ATTRIBUTE_ID
--	from @tb_item_outcome t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
--	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_control
--	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
--	where SET_ID = @NEW_SET_ID)
	
--	update t1 --new_item_attribute_id_outcome
--	set new_item_attribute_id_outcome = t2.ATTRIBUTE_ID
--	from @tb_item_outcome t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
--	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_outcome
--	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
--	where SET_ID = @NEW_SET_ID)
	
----12 copy @tb_item_outcome into Reviewer.dbo.TB_ITEM_OUTCOME
--	declare @WORKING_OUTCOME_ID int
--	declare OUTCOME_ID_CURSOR cursor for
--	select old_outcome_id FROM @tb_item_outcome
--	open OUTCOME_ID_CURSOR
--	fetch next from OUTCOME_ID_CURSOR
--	into @WORKING_OUTCOME_ID
--	while @@FETCH_STATUS = 0
--	begin
--		insert into Reviewer.dbo.TB_ITEM_OUTCOME (ITEM_SET_ID, OUTCOME_TYPE_ID, 
--		ITEM_ATTRIBUTE_ID_INTERVENTION, ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, 
--		OUTCOME_TITLE, DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7,
--		DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14, OUTCOME_DESCRIPTION)	
--		SELECT new_item_set_id, outcome_type_id, new_item_attribute_id_intervention, 
--		new_item_attribute_id_control, new_item_attribute_id_outcome, outcome_title, 
--		data1, data2, data3, data4, data5, data6, data7, data8, data9, data10, data11, 
--		data12, data13, data14, outcome_description 
--		FROM @tb_item_outcome 
--		WHERE old_outcome_id = @WORKING_OUTCOME_ID
--		set @NEW_OUTCOME_ID = CAST(SCOPE_IDENTITY() AS INT)
		
--		update @tb_item_outcome set new_outcome_id = @NEW_OUTCOME_ID
--		WHERE old_outcome_id = @WORKING_OUTCOME_ID

--		FETCH NEXT FROM OUTCOME_ID_CURSOR 
--		INTO @WORKING_OUTCOME_ID
--	END

--	CLOSE OUTCOME_ID_CURSOR
--	DEALLOCATE OUTCOME_ID_CURSOR

----13 fill up @tb_item_outcome_attribute
--	insert into @tb_item_outcome_attribute (old_item_outcome_attribute_id,
--	 old_outcome_id, old_attribute_id, additional_text)
--	select ITEM_OUTCOME_ATTRIBUTE_ID, OUTCOME_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT
--	from Reviewer.dbo.TB_ITEM_OUTCOME_ATTRIBUTE i_o_a
--	inner join @tb_item_outcome i_o on i_o.OLD_OUTCOME_ID = i_o_a.OUTCOME_ID	

----14 update tb_item_outcome_attribute with the 'new_' values
--	update t1 --new_outcome_id
--	set new_outcome_id = t2.new_outcome_id
--	from @tb_item_outcome_attribute t1 inner join @tb_item_outcome t2
--	on t2.old_outcome_id = t1.old_outcome_id
	
--	update t1 --new_attribute_id
--	set new_attribute_id = t2.ATTRIBUTE_ID
--	from @tb_item_outcome_attribute t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
--	on t2.OLD_ATTRIBUTE_ID = t1.old_attribute_id
--	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
--	where SET_ID = @NEW_SET_ID)
	
----15 copy @tb_item_outcome_attribute into Reviewer.dbo.TB_ITEM_OUTCOME_ATTRIBUTE
--	insert into Reviewer.dbo.TB_ITEM_OUTCOME_ATTRIBUTE (OUTCOME_ID, ATTRIBUTE_ID,
--	ADDITIONAL_TEXT)
--	select new_outcome_id, new_attribute_id, additional_text from @tb_item_outcome_attribute
	
----16 place the outcome_id values into ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA for later use
--	insert into ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA (NEW_REVIEW_ID, OLD_OUTCOME_ID, NEW_OUTCOME_ID)
--	select @DESTINATION_REVIEW_ID, old_outcome_id, new_outcome_id  
--	from @tb_item_outcome
	
--COMMIT TRANSACTION
--END TRY

--BEGIN CATCH
--IF (@@TRANCOUNT > 0)
--begin	
--ROLLBACK TRANSACTION
--set @RESULT = 'Unable to copy the coding from the old review to the new review'
--end
--END CATCH


--RETURN

--SET NOCOUNT OFF













--GO

-------------------------
--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep13]    Script Date: 05/30/2012 11:51:59 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO











--CREATE procedure [dbo].[st_CopyReviewStep13]
--(	
--	@CONTACT_ID int,
--	@SOURCE_REVIEW_ID int,
--	@DESTINATION_REVIEW_ID int,
--	@RESULT nvarchar(50) output
--)

--As

--SET NOCOUNT ON


--declare @NEW_SEARCH_ID int
--declare @NEW_ITEM_DOCUMENT_ID bigint
--set @RESULT = '0'

--declare @tb_work_allocation table
--(new_set_id bigint, old_set_id bigint,
-- new_attribute_id bigint, old_attribute_id bigint)
 
--declare @tb_item_document table
--(new_item_document_id bigint, old_item_document_id bigint, 
-- new_item_id bigint, old_item_id bigint, document_title nvarchar(255), document_binary image, 
-- document_extenstion nvarchar(5), document_text nvarchar(max))


--BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
--BEGIN TRANSACTION


----01 copy the work assignments
--	insert into @tb_work_allocation (old_set_id, old_attribute_id)
--	select w_s.SET_ID, w_s.ATTRIBUTE_ID  
--	from Reviewer.dbo.TB_WORK_ALLOCATION w_s
--	where w_s.REVIEW_ID = @SOURCE_REVIEW_ID

--	update t1
--	set new_set_id = t2.SET_ID
--	from @tb_work_allocation t1 inner join Reviewer.dbo.TB_SET t2
--	on t2.OLD_GUIDELINE_ID = t1.old_set_id
--	where t2.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
--	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
--	update t1
--	set new_attribute_id = t2.ATTRIBUTE_ID
--	from @tb_work_allocation t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
--	on t2.OLD_ATTRIBUTE_ID = t1.old_attribute_id
--	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
--	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
--	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
--	insert into Reviewer.dbo.TB_WORK_ALLOCATION (CONTACT_ID, REVIEW_ID, SET_ID, ATTRIBUTE_ID)
--	select @CONTACT_ID, @DESTINATION_REVIEW_ID, w_a.new_set_id, w_a.new_attribute_id  
--	from @tb_work_allocation w_a

----02 copy diagrams
--	insert into Reviewer.dbo.TB_DIAGRAM (REVIEW_ID, DIAGRAM_NAME, DIAGRAM_DETAIL)
--	select @DESTINATION_REVIEW_ID, d.DIAGRAM_NAME, d.DIAGRAM_DETAIL
--	from  Reviewer.dbo.TB_DIAGRAM d
--	where d.REVIEW_ID = @SOURCE_REVIEW_ID

----03 copy searches
--	declare @tb_search table (old_search_id int, review_id int, contact_id int, search_title nvarchar(4000),
--		search_no int, answers nvarchar(4000), hits_no int)
	
--	insert into @tb_search (old_search_id, review_id, contact_id, search_title, search_no, answers, hits_no)
--	select s.SEARCH_ID, @DESTINATION_REVIEW_ID, @CONTACT_ID, s.SEARCH_TITLE, s.SEARCH_NO, s.ANSWERS, s.HITS_NO
--	from Reviewer.dbo.TB_SEARCH s
--	where s.REVIEW_ID = @SOURCE_REVIEW_ID
	
--	--select * from @tb_search
	
--	declare @tb_search_item table (new_item_id bigint, old_item_id bigint, old_search_id int, item_rank int)
	
--	insert into @tb_search_item (old_item_id, old_search_id, item_rank)
--	select s_i.ITEM_ID, s_i.SEARCH_ID, s_i.ITEM_RANK
--	from Reviewer.dbo.TB_SEARCH_ITEM s_i
--	where SEARCH_ID in (select old_search_id from @tb_search)
	
--	--select * from @tb_search_item
	
--	update t1
--	set new_item_id = t2.ITEM_ID
--	from @tb_search_item t1 inner join Reviewer.dbo.TB_ITEM t2
--	on t2.OLD_ITEM_ID = t1.old_item_id
--	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
--	where i_r.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
--	--select * from @tb_search_item
	
--	declare @WORKING_SEARCH_ID int
--	declare SEARCH_ID_CURSOR cursor for
--	select old_search_id FROM @tb_search
--	open SEARCH_ID_CURSOR
--	fetch next from SEARCH_ID_CURSOR
--	into @WORKING_SEARCH_ID
--	while @@FETCH_STATUS = 0
--	begin
--		insert into Reviewer.dbo.TB_SEARCH (REVIEW_ID, CONTACT_ID, SEARCH_TITLE, SEARCH_NO, ANSWERS, HITS_NO, SEARCH_DATE)	
--		SELECT review_id, @CONTACT_ID, search_title, search_no, answers, hits_no, GETDATE() FROM @tb_search
--		WHERE old_search_id = @WORKING_SEARCH_ID
--		set @NEW_SEARCH_ID = CAST(SCOPE_IDENTITY() AS INT)
		
--		insert into Reviewer.dbo.TB_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
--		select new_item_id, @NEW_SEARCH_ID, item_rank from @tb_search_item
--		where old_search_id = @WORKING_SEARCH_ID
		
--		FETCH NEXT FROM SEARCH_ID_CURSOR 
--		INTO @WORKING_SEARCH_ID
--	END

--	CLOSE SEARCH_ID_CURSOR
--	DEALLOCATE SEARCH_ID_CURSOR
	

----04 copy pdfs
--	insert into @tb_item_document (old_item_document_id, old_item_id, document_title, 
--	document_binary, document_extenstion, document_text)
--	select i_d.ITEM_DOCUMENT_ID, i_d.ITEM_ID, i_d.DOCUMENT_TITLE, i_d.DOCUMENT_BINARY,
--	i_d.DOCUMENT_EXTENSION, i_d.DOCUMENT_TEXT  
--	from Reviewer.dbo.TB_ITEM_DOCUMENT i_d
--	where i_d.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW
--	where REVIEW_ID = @SOURCE_REVIEW_ID)
	
--	update t1
--	set new_item_id = t2.ITEM_ID
--	from @tb_item_document t1 inner join Reviewer.dbo.TB_ITEM t2
--	on t2.OLD_ITEM_ID = t1.old_item_id
--	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
--	where i_r.REVIEW_ID = @DESTINATION_REVIEW_ID) 
	
----select * from @tb_item_document

--	declare @WORKING_ITEM_DOCUMENT_ID int
--	declare ITEM_DOCUMENT_ID_CURSOR cursor for
--	select old_item_document_id FROM @tb_item_document
--	open ITEM_DOCUMENT_ID_CURSOR
--	fetch next from ITEM_DOCUMENT_ID_CURSOR
--	into @WORKING_ITEM_DOCUMENT_ID
--	while @@FETCH_STATUS = 0
--	begin
--		insert into Reviewer.dbo.TB_ITEM_DOCUMENT (ITEM_ID, DOCUMENT_TITLE, DOCUMENT_BINARY, 
--		DOCUMENT_EXTENSION, DOCUMENT_TEXT)	
--		SELECT new_item_id, document_title, document_binary, document_extenstion, document_text 
--		FROM @tb_item_document
--		WHERE old_item_document_id = @WORKING_ITEM_DOCUMENT_ID
--		set @NEW_ITEM_DOCUMENT_ID = CAST(SCOPE_IDENTITY() AS INT)
				
--		FETCH NEXT FROM ITEM_DOCUMENT_ID_CURSOR 
--		INTO @WORKING_ITEM_DOCUMENT_ID
--	END

--	CLOSE ITEM_DOCUMENT_ID_CURSOR
--	DEALLOCATE ITEM_DOCUMENT_ID_CURSOR


	
	
--COMMIT TRANSACTION
--END TRY

--BEGIN CATCH
--IF (@@TRANCOUNT > 0)
--begin	
--ROLLBACK TRANSACTION
--set @RESULT = 'Unable to copy the work assignments, diagrams, searches, uploaded documents'
--end
--END CATCH


--RETURN

--SET NOCOUNT OFF














--GO

---------------------------
--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep15]    Script Date: 05/30/2012 11:52:24 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO











--CREATE procedure [dbo].[st_CopyReviewStep15]
--(	
--	@CONTACT_ID int,
--	@SOURCE_REVIEW_ID int,
--	@DESTINATION_REVIEW_ID int,
--	@RESULT nvarchar(50) output
--)

--As

--SET NOCOUNT ON



--set @RESULT = '0'
--declare @NEW_REPORT_ID int
--declare @NEW_REPORT_COLUMN_ID int
--declare @NEW_META_ANALYSIS_ID int
--declare @NEW_ITEM_DOCUMENT_ID int



--BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
--BEGIN TRANSACTION


----01 copy the reports
--declare @tb_report table
--(new_report_id bigint, old_report_id bigint, name nvarchar(255), report_type nvarchar(10))

--	insert into @tb_report (old_report_id, name, report_type)
--	select r.REPORT_ID, r.NAME, r.REPORT_TYPE  
--	from Reviewer.dbo.TB_REPORT r
--	where r.REVIEW_ID = @SOURCE_REVIEW_ID

--	--select * from @tb_report


--	declare @WORKING_REPORT_ID int
--	declare REPORT_ID_CURSOR cursor for
--	select old_report_id FROM @tb_report
--	open REPORT_ID_CURSOR
--	fetch next from REPORT_ID_CURSOR
--	into @WORKING_REPORT_ID
--	while @@FETCH_STATUS = 0
--	begin
--		insert into Reviewer.dbo.TB_REPORT (REVIEW_ID, CONTACT_ID, NAME, REPORT_TYPE)	
--		SELECT @DESTINATION_REVIEW_ID, @CONTACT_ID, name, report_type FROM @tb_report
--		WHERE old_report_id = @WORKING_REPORT_ID
--		set @NEW_REPORT_ID = CAST(SCOPE_IDENTITY() AS INT)
		
--		update @tb_report
--		set new_report_id = @NEW_REPORT_ID
--		where old_report_id = @WORKING_REPORT_ID
		
--		FETCH NEXT FROM REPORT_ID_CURSOR 
--		INTO @WORKING_REPORT_ID
--	END

--	CLOSE REPORT_ID_CURSOR
--	DEALLOCATE REPORT_ID_CURSOR


--declare @tb_report_column table
--(new_report_column_id int, old_report_column_id int, new_report_id bigint, old_report_id int,
--report_column_name nvarchar(255), column_order int)

--	insert into @tb_report_column (old_report_column_id, old_report_id, report_column_name, column_order)
--	select r_c.REPORT_COLUMN_ID, r_c.REPORT_ID, r_c.REPORT_COLUMN_NAME, r_c.COLUMN_ORDER  
--	from Reviewer.dbo.TB_REPORT_COLUMN r_c
--	where r_c.REPORT_ID in (select old_report_id from @tb_report)
	
----select * from @tb_report_column
	
--	update t1
--	set new_report_id = t2.new_report_id
--	from @tb_report_column t1 inner join @tb_report t2
--	on t2.old_report_id = t1.old_report_id
	
----select * from @tb_report_column	
	
	
--	declare @WORKING_REPORT_COLUMN_ID int
--	declare REPORT_COLUMN_ID_CURSOR cursor for
--	select old_report_column_id FROM @tb_report_column
--	open REPORT_COLUMN_ID_CURSOR
--	fetch next from REPORT_COLUMN_ID_CURSOR
--	into @WORKING_REPORT_COLUMN_ID
--	while @@FETCH_STATUS = 0
--	begin
--		insert into Reviewer.dbo.TB_REPORT_COLUMN (REPORT_ID, REPORT_COLUMN_NAME, COLUMN_ORDER)	
--		SELECT new_report_id, report_column_name, column_order FROM @tb_report_column
--		WHERE old_report_column_id = @WORKING_REPORT_COLUMN_ID
--		set @NEW_REPORT_COLUMN_ID = CAST(SCOPE_IDENTITY() AS INT)
		
--		update @tb_report_column
--		set new_report_column_id = @NEW_REPORT_COLUMN_ID
--		where old_report_column_id = @WORKING_REPORT_COLUMN_ID
		
--		FETCH NEXT FROM REPORT_COLUMN_ID_CURSOR 
--		INTO @WORKING_REPORT_COLUMN_ID
--	END

--	CLOSE REPORT_COLUMN_ID_CURSOR
--	DEALLOCATE REPORT_COLUMN_ID_CURSOR
	
----select * from @tb_report_column		
	
--declare @tb_report_column_code table
--(new_report_column_code_id int, old_report_column_code_id int, new_report_id bigint, old_report_id int,
--new_report_column_id int, old_report_column_id int, code_order int, new_set_id int, old_set_id nvarchar(50),
--new_attribute_id bigint, old_attribute_id bigint, new_parent_attribute_id bigint, old_parent_attribute_id bigint, parent_attribute_text nvarchar(255),
--user_def_text nvarchar(255), display_code bit, display_additional_text bit, display_coded_text bit)

--	insert into @tb_report_column_code (old_report_column_code_id, old_report_id, old_report_column_id, code_order, old_set_id,
--	old_attribute_id, old_parent_attribute_id, parent_attribute_text, user_def_text, display_code, display_additional_text, display_coded_text)
--	select r_c_c.REPORT_COLUMN_CODE_ID,  r_c_c.REPORT_ID, r_c_c.REPORT_COLUMN_ID, r_c_c.CODE_ORDER, r_c_c.SET_ID, r_c_c.ATTRIBUTE_ID, 
--	r_c_c.PARENT_ATTRIBUTE_ID, r_c_c.PARENT_ATTRIBUTE_TEXT,
--	r_c_c.USER_DEF_TEXT, r_c_c.DISPLAY_CODE, r_c_c.DISPLAY_ADDITIONAL_TEXT, r_c_c.DISPLAY_CODED_TEXT  
--	from Reviewer.dbo.TB_REPORT_COLUMN_CODE r_c_c
--	where r_c_c.REPORT_ID in (select old_report_id from @tb_report)

----select * from @tb_report_column_code
	
--	update r_c_c
--	set new_report_id = r.new_report_id
--	from @tb_report r inner join @tb_report_column_code r_c_c
--	on r.old_report_id = r_c_c.old_report_id

----select * from @tb_report_column_code

--	update r_c_c
--	set new_report_column_id = r_c.new_report_column_id
--	from @tb_report_column r_c inner join @tb_report_column_code r_c_c
--	on r_c.old_report_column_id = r_c_c.old_report_column_id
	
----select * from @tb_report_column_code

--	update r_c_c
--	set new_set_id = s.SET_ID
--	from Reviewer.dbo.TB_SET s inner join @tb_report_column_code r_c_c
--	on s.OLD_GUIDELINE_ID = r_c_c.old_set_id
--	where s.SET_ID in (select r_s.SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
--	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
----select * from @tb_report_column_code

--	update r_c_c
--	set new_attribute_id = a.ATTRIBUTE_ID
--	from Reviewer.dbo.TB_ATTRIBUTE a inner join @tb_report_column_code r_c_c
--	on a.OLD_ATTRIBUTE_ID = r_c_c.old_attribute_id
--	where a.ATTRIBUTE_ID in (select a_s.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
--	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
--	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
----select * from @tb_report_column_code

--	update r_c_c
--	set new_parent_attribute_id = a_s.PARENT_ATTRIBUTE_ID
--	from Reviewer.dbo.TB_ATTRIBUTE_SET a_s inner join @tb_report_column_code r_c_c
--	on a_s.ATTRIBUTE_ID = r_c_c.new_attribute_id
--	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
--	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID)

----select * from @tb_report_column_code	

--	insert into Reviewer.dbo.TB_REPORT_COLUMN_CODE (REPORT_ID, REPORT_COLUMN_ID, CODE_ORDER, 
--	SET_ID, ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID, PARENT_ATTRIBUTE_TEXT, USER_DEF_TEXT, 
--	DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT)  
--	select r_c_c.new_report_id, r_c_c.new_report_column_id, r_c_c.code_order, r_c_c.new_set_id,
--	r_c_c.new_attribute_id, r_c_c.new_parent_attribute_id, r_c_c.parent_attribute_text,
--	r_c_c.user_def_text, r_c_c.display_code, r_c_c.display_additional_text,
--	r_c_c.display_coded_text
--	from @tb_report_column_code r_c_c


----02 copy meta-analysis

	
--declare @tb_meta_analysis table
--(new_meta_analysis_id bigint, old_meta_analysis_id bigint, meta_analysis_title nvarchar(255), 
--contact_id int, review_id int, new_attribute_id bigint, old_attribute_id bigint, new_set_id int,
--old_set_id int, new_attribute_id_intervention bigint, old_attribute_id_intervention bigint,
--new_attribute_id_control bigint, old_attribute_id_control bigint, new_attribute_id_outcome bigint,
--old_attribute_id_outcome bigint, meta_analysis_type_id bigint)

--	insert into @tb_meta_analysis (old_meta_analysis_id, meta_analysis_title, contact_id,
--	review_id, old_attribute_id, old_set_id, old_attribute_id_intervention, old_attribute_id_control,
--	old_attribute_id_outcome, meta_analysis_type_id)
--	select m_a.META_ANALYSIS_ID, m_a.META_ANALYSIS_TITLE, m_a.CONTACT_ID, @DESTINATION_REVIEW_ID,
--	m_a.ATTRIBUTE_ID, m_a.SET_ID, m_a.ATTRIBUTE_ID_INTERVENTION, m_a.ATTRIBUTE_ID_CONTROL,
--	m_a.ATTRIBUTE_ID_OUTCOME, m_a.META_ANALYSIS_TYPE_ID  
--	from Reviewer.dbo.TB_META_ANALYSIS m_a
--	where m_a.REVIEW_ID = @SOURCE_REVIEW_ID
	
----select * from @tb_meta_analysis

--	update m_a
--	set new_set_id = s.SET_ID
--	from Reviewer.dbo.TB_SET s inner join @tb_meta_analysis m_a
--	on s.OLD_GUIDELINE_ID = m_a.old_set_id
--	where s.SET_ID in (select r_s.SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
--	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID)

----select * from @tb_meta_analysis

--	update m_a
--	set new_attribute_id = a.ATTRIBUTE_ID
--	from Reviewer.dbo.TB_ATTRIBUTE a inner join @tb_meta_analysis m_a
--	on a.OLD_ATTRIBUTE_ID = m_a.old_attribute_id
--	where a.ATTRIBUTE_ID in (select a_s.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
--	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
--	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
----cleanup new_attribute_id as it is was only used in old reviews
--	update @tb_meta_analysis set new_attribute_id = 0
--	where new_attribute_id is null
	
--	update m_a
--	set new_attribute_id_intervention = a.ATTRIBUTE_ID
--	from Reviewer.dbo.TB_ATTRIBUTE a inner join @tb_meta_analysis m_a
--	on a.OLD_ATTRIBUTE_ID = m_a.old_attribute_id_intervention
--	where a.ATTRIBUTE_ID in (select a_s.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
--	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
--	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
--	update m_a
--	set new_attribute_id_intervention = a.ATTRIBUTE_ID
--	from Reviewer.dbo.TB_ATTRIBUTE a inner join @tb_meta_analysis m_a
--	on a.OLD_ATTRIBUTE_ID = m_a.old_attribute_id_intervention
--	where a.ATTRIBUTE_ID in (select a_s.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
--	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
--	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
--	update m_a
--	set new_attribute_id_control = a.ATTRIBUTE_ID
--	from Reviewer.dbo.TB_ATTRIBUTE a inner join @tb_meta_analysis m_a
--	on a.OLD_ATTRIBUTE_ID = m_a.old_attribute_id_control
--	where a.ATTRIBUTE_ID in (select a_s.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
--	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
--	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
--	update m_a
--	set new_attribute_id_outcome = a.ATTRIBUTE_ID
--	from Reviewer.dbo.TB_ATTRIBUTE a inner join @tb_meta_analysis m_a
--	on a.OLD_ATTRIBUTE_ID = m_a.old_attribute_id_outcome
--	where a.ATTRIBUTE_ID in (select a_s.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
--	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
--	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
----select * from @tb_meta_analysis

--	declare @WORKING_META_ANALYSIS_ID int
--	declare META_ANALYSIS_ID_CURSOR cursor for
--	select old_meta_analysis_id FROM @tb_meta_analysis
--	open META_ANALYSIS_ID_CURSOR
--	fetch next from META_ANALYSIS_ID_CURSOR
--	into @WORKING_META_ANALYSIS_ID
--	while @@FETCH_STATUS = 0
--	begin
--		insert into Reviewer.dbo.TB_META_ANALYSIS (META_ANALYSIS_TITLE, CONTACT_ID, 
--		REVIEW_ID, ATTRIBUTE_ID, SET_ID, ATTRIBUTE_ID_INTERVENTION,
--		ATTRIBUTE_ID_CONTROL, ATTRIBUTE_ID_OUTCOME, META_ANALYSIS_TYPE_ID)	
--		SELECT meta_analysis_title, contact_id, review_id, new_attribute_id,
--		new_set_id, new_attribute_id_intervention, new_attribute_id_control,
--		new_attribute_id_outcome, meta_analysis_type_id 
--		FROM @tb_meta_analysis
--		WHERE old_meta_analysis_id = @WORKING_META_ANALYSIS_ID
--		set @NEW_META_ANALYSIS_ID = CAST(SCOPE_IDENTITY() AS INT)
		
--		update @tb_meta_analysis
--		set new_meta_analysis_id = @NEW_META_ANALYSIS_ID
--		where old_meta_analysis_id = @WORKING_META_ANALYSIS_ID
		
--		FETCH NEXT FROM META_ANALYSIS_ID_CURSOR 
--		INTO @WORKING_META_ANALYSIS_ID
--	END

--	CLOSE META_ANALYSIS_ID_CURSOR
--	DEALLOCATE META_ANALYSIS_ID_CURSOR

----select * from @tb_meta_analysis

--declare @tb_meta_analysis_outcome table
--(new_meta_analysis_id int, old_meta_analysis_id int, 
-- new_outcome_id int, old_outcome_id int)

--	insert into @tb_meta_analysis_outcome (old_meta_analysis_id, old_outcome_id)
--	select m_a_o.META_ANALYSIS_ID, m_a_o.OUTCOME_ID 
--	from Reviewer.dbo.TB_META_ANALYSIS_OUTCOME m_a_o
--	where m_a_o.META_ANALYSIS_ID in (select META_ANALYSIS_ID from 
--	Reviewer.dbo.TB_META_ANALYSIS where REVIEW_ID = @SOURCE_REVIEW_ID)

----select * from @tb_meta_analysis_outcome

--	update m_a_o
--	set m_a_o.new_meta_analysis_id = m_a.new_meta_analysis_id
--	from @tb_meta_analysis m_a inner join @tb_meta_analysis_outcome m_a_o
--	on m_a.old_meta_analysis_id = m_a_o.old_meta_analysis_id

---- how to find a new OUTCOME_ID using an old OUTCOME_ID in TB_ITEM_OUTCOME?
----- Database is not set up to allow this as ER4 has no need to understand the concept of 
----- old and new outcome. This is just something I have invented to allow the review copy
----- function. I need to do a bit of gymnastics to make this work.
----- The only answer I can think of is to record old and new outcomes at the time 
----- of creation (in an earlier st_ ) and hold it externally in ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA
----- until I need it.

--	update m_a_o
--	set m_a_o.new_outcome_id = r_c_d.NEW_OUTCOME_ID
--	from ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA r_c_d inner join @tb_meta_analysis_outcome m_a_o
--	on r_c_d.OLD_OUTCOME_ID = m_a_o.old_outcome_id
--	where r_c_d.NEW_REVIEW_ID = @DESTINATION_REVIEW_ID
	
----select * from @tb_meta_analysis_outcome	

--	insert into Reviewer.dbo.TB_META_ANALYSIS_OUTCOME 
--	(META_ANALYSIS_ID, OUTCOME_ID)
--	select new_meta_analysis_id, new_outcome_id from @tb_meta_analysis_outcome


---- 03 copy links

--declare @tb_item_link table
--(old_item_link_id int, new_item_id_primary bigint, old_item_id_primary bigint,
--new_item_id_secondary bigint, old_item_id_secondary bigint, link_description nvarchar(255))

--insert into @tb_item_link (old_item_link_id, old_item_id_primary, 
--	old_item_id_secondary, link_description)
--	select i_l.ITEM_LINK_ID, i_l.ITEM_ID_PRIMARY, i_l.ITEM_ID_SECONDARY,
--	i_l.LINK_DESCRIPTION  
--	from Reviewer.dbo.TB_ITEM_LINK i_l
--	where i_l.ITEM_ID_PRIMARY in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW
--	where REVIEW_ID = @SOURCE_REVIEW_ID)
	
----select * from @tb_item_link

--	update t1
--	set new_item_id_primary = t2.ITEM_ID
--	from @tb_item_link t1 inner join Reviewer.dbo.TB_ITEM t2
--	on t2.OLD_ITEM_ID = t1.old_item_id_primary
--	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
--	where i_r.REVIEW_ID = @DESTINATION_REVIEW_ID) 
	
--	update t1
--	set new_item_id_secondary = t2.ITEM_ID
--	from @tb_item_link t1 inner join Reviewer.dbo.TB_ITEM t2
--	on t2.OLD_ITEM_ID = t1.old_item_id_secondary
--	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
--	where i_r.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
----select * from @tb_item_link


--	declare @WORKING_ITEM_LINK_ID int
--	declare ITEM_LINK_ID_CURSOR cursor for
--	select old_item_link_id FROM @tb_item_link
--	open ITEM_LINK_ID_CURSOR
--	fetch next from ITEM_LINK_ID_CURSOR
--	into @WORKING_ITEM_LINK_ID
--	while @@FETCH_STATUS = 0
--	begin
--		insert into Reviewer.dbo.TB_ITEM_LINK (ITEM_ID_PRIMARY, ITEM_ID_SECONDARY,
--		LINK_DESCRIPTION)	
--		SELECT new_item_id_primary, new_item_id_secondary, link_description 
--		FROM @tb_item_link
--		WHERE old_item_link_id = @WORKING_ITEM_LINK_ID
--		set @NEW_ITEM_DOCUMENT_ID = CAST(SCOPE_IDENTITY() AS INT)
				
--		FETCH NEXT FROM ITEM_LINK_ID_CURSOR 
--		INTO @WORKING_ITEM_LINK_ID
--	END

--	CLOSE ITEM_LINK_ID_CURSOR
--	DEALLOCATE ITEM_LINK_ID_CURSOR


	
	
--COMMIT TRANSACTION
--END TRY

--BEGIN CATCH
--IF (@@TRANCOUNT > 0)
--begin	
--ROLLBACK TRANSACTION
--set @RESULT = 'Unable to copy the reports, meta-analysis, links'
--end
--END CATCH


--RETURN

--SET NOCOUNT OFF















--GO

-------------------------
--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_CopyReviewStepCleanup]    Script Date: 05/30/2012 11:52:37 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO











--CREATE procedure [dbo].[st_CopyReviewStepCleanup]
--(	
--	@DESTINATION_REVIEW_ID int,
--	@RESULT nvarchar(50) output
--)

--As

--SET NOCOUNT ON

---- tables and fields to set to null
---- TB_ATTRIBUTE - OLD_ATTRIBUTE_ID
---- TB_SET - OLD_GUIDELINE_ID
---- TB_ITEM - OLD_ITEM_ID

--set @RESULT = '0'


--BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
--BEGIN TRANSACTION


----01 clean up TB_ATTRIBUTE
--	update Reviewer.dbo.TB_ATTRIBUTE
--	set OLD_ATTRIBUTE_ID = null
--	where ATTRIBUTE_ID in 
--	(select a.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE a
--	inner join Reviewer.dbo.TB_ATTRIBUTE_SET a_s on a_s.ATTRIBUTE_ID = a.ATTRIBUTE_ID
--	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
--	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
----02 clean up TB_SET
--	update Reviewer.dbo.TB_SET
--	set OLD_GUIDELINE_ID = null
--	where SET_ID in 
--	(select s.SET_ID from  Reviewer.dbo.TB_SET s
--	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
--	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
----03 clean up TB_ITEM
--	update Reviewer.dbo.TB_ITEM
--	set OLD_ITEM_ID = null
--	where ITEM_ID in
--	(select i_r.ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
--	where i_r.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
----03 clean up ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA
--	delete from ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA
--	where NEW_REVIEW_ID = @DESTINATION_REVIEW_ID


--COMMIT TRANSACTION
--END TRY

--BEGIN CATCH
--IF (@@TRANCOUNT > 0)
--begin	
--ROLLBACK TRANSACTION
--set @RESULT = 'Unable to remove the links between the old and new review'
--end
--END CATCH


--RETURN

--SET NOCOUNT OFF














--GO

----------------------
--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_CountExampleReviews]    Script Date: 05/30/2012 11:55:04 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO








--CREATE procedure [dbo].[st_CountExampleReviews]
--(
--	@CONTACT_ID int,
--	@NUMBER_REVIEWS int output
--)

--As

--SET NOCOUNT ON


--declare @contact_name nvarchar(255)
--declare @review_name nvarchar(1000) 

--select @contact_name = CONTACT_NAME from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID
--set @review_name = @contact_name + '''s example non-shareable review'

	
--	select @NUMBER_REVIEWS = COUNT(REVIEW_NAME) 
--	from Reviewer.dbo.TB_REVIEW
--	where REVIEW_NAME = @review_name
--	and REVIEW_ID in (select REVIEW_ID from Reviewer.dbo.TB_REVIEW_CONTACT
--	where CONTACT_ID = @CONTACT_ID)
	

--RETURN

--SET NOCOUNT OFF











--GO

-------------------------













------------------------------------------------------------------------
----copy codeset changes

-------------sp edited
----st_CopyCodeset

------------sp new
----st_LinkCodeset
---- you may have this already! fn_IsAttributeInTree

--USE [Reviewer]
--GO

--/****** Object:  UserDefinedFunction [dbo].[fn_IsAttributeInTree]    Script Date: 02/09/2012 11:30:37 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date, ,>
---- Description:	<Description, ,>
---- =============================================
--CREATE FUNCTION [dbo].[fn_IsAttributeInTree] 
--(
--	-- Add the parameters for the function here
--	@ATTRIBUTE_ID int
--)
--RETURNS bit
--AS
--BEGIN
--	-- Declare the return variable here
--	DECLARE @tmp int;

--	with AAA (ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID)
--	as
--	(
--	select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from TB_ATTRIBUTE_SET tas
--	where ATTRIBUTE_ID = @ATTRIBUTE_ID
--	UNION ALL
--	select tas.ATTRIBUTE_ID, tas.PARENT_ATTRIBUTE_ID from TB_ATTRIBUTE_SET tas
--	inner join AAA on AAA.PARENT_ATTRIBUTE_ID = tas.ATTRIBUTE_ID
--	)
--	select @tmp =  min( PARENT_ATTRIBUTE_ID) from AAA --where PARENT_ATTRIBUTE_ID = 0
--	if @tmp = 0 RETURN 1
--	RETURN 0
	
	

--END

--GO



----------------------------------------------


--USE [ReviewerAdmin]
--GO
--/****** Object:  StoredProcedure [dbo].[st_CopyCodeset]    Script Date: 02/29/2012 10:11:14 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO





--ALTER procedure [dbo].[st_CopyCodeset]
--(
--	@SOURCE_SET_ID int,
--	@SOURCE_REVIEW_ID int,
--	@DESTINATION_REVIEW_ID int,
--	@CONTACT_ID int,
--	@RESULT nvarchar(50) output
--)

--As

--SET NOCOUNT ON


--declare @DESTINATION_SET_ID int
--declare @DESTINATION_REVIEW_SET_ID int
--declare @DESTINATION_SET_NAME nvarchar (255)
--set @RESULT = '0'



--BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
--BEGIN TRANSACTION

----01 create new row in Reviewer.dbo.TB_SET and get @DESTINATION_SET_ID
--	insert into Reviewer.dbo.TB_SET (SET_TYPE_ID, SET_NAME)
--	select s.SET_TYPE_ID, s.SET_NAME from Reviewer.dbo.TB_SET s
--	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
--	where s.SET_ID = @SOURCE_SET_ID
--	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID
--	set @DESTINATION_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

--	-- prefix the new codeset with 'COPY - ' BUT first remove
--	--  the 'LINKED - ' prefix if it exists
--	select @DESTINATION_SET_NAME = SET_NAME
--	from Reviewer.dbo.TB_SET
--	where SET_ID = @DESTINATION_SET_ID

--	declare @Prefix nvarchar(6) 
--	set @Prefix = substring (@DESTINATION_SET_NAME, 0, 7) 
--	if @Prefix = 'LINKED'
--	begin
--		set @DESTINATION_SET_NAME = substring (@DESTINATION_SET_NAME, 9, len(@DESTINATION_SET_NAME) - 8)
--	end
	
--	update Reviewer.dbo.TB_SET
--	set SET_NAME = 'COPY - ' + @DESTINATION_SET_NAME
--	where SET_ID = @DESTINATION_SET_ID

----02 create new row in Reviewer.dbo.TB_REVIEW_SET and get @DESTINATION_REVIEW_SET_ID
--	insert into Reviewer.dbo.TB_REVIEW_SET (REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL)
--	values (@DESTINATION_REVIEW_ID, @DESTINATION_SET_ID, 1, 1)
--	set @DESTINATION_REVIEW_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

--	-- @attribute_id
--	declare @attribute_id table (attribute_id int, ok int)

----03 get source ATTRIBUTE_ID's and put in tmp table @attribute_id (avoids orphans)	
--	insert into @attribute_id 
--	Select a_s.ATTRIBUTE_ID, Reviewer.dbo.fn_IsAttributeInTree(a_s.ATTRIBUTE_ID ) as ok 
--	from Reviewer.dbo.TB_ATTRIBUTE_SET a_s 
--	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
--	where a_s.SET_ID = @SOURCE_SET_ID
--	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID 
	
--	--A
--	--select * from @attribute_id
	
--	-- @tb_attribute
--	declare @tb_attribute table 
--	(ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))
	
----04 use @attribute_id to get source data from TB_ATTRIBUTE and put in tmp table @tb_attribute
--	insert into @tb_attribute (ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
--		select ATTRIBUTE_ID, @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, ATTRIBUTE_ID -- use ATTRIBUTE_ID as OLD_ATTRIBUTE_ID
--		from Reviewer.dbo.TB_ATTRIBUTE
--		where ATTRIBUTE_ID in 
--		(select attribute_id from @attribute_id)

--	--B
--	--select * from @tb_attribute

--	-- @tb_attribute_set
--	declare @tb_attribute_set table 
--	(ATTRIBUTE_SET_ID int, ATTRIBUTE_ID int, SET_ID int, PARENT_ATTRIBUTE_ID int, ATTRIBUTE_TYPE_ID int, 
--	ATTRIBUTE_SET_DESC nvarchar(max), ATTRIBUTE_ORDER int)

----05 use @attribute_id to get source data from TB_ATTRIBUTE_SET and put in tmp table @tb_attribute_set	
--	insert into @tb_attribute_set (ATTRIBUTE_SET_ID, ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)
--		select ATTRIBUTE_SET_ID, ATTRIBUTE_ID, @DESTINATION_SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
--		from Reviewer.dbo.TB_ATTRIBUTE_SET
--		where ATTRIBUTE_ID in 
--		(select attribute_id from @attribute_id)

--	--C
--	--select * from @tb_attribute_set

----06 put @tb_attribute into Reviewer.dbo.TB_ATTRIBUTE
--	insert into Reviewer.dbo.TB_ATTRIBUTE (CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)	
--		select @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID 
--		from @tb_attribute

--	-- @new_tb_attribute
--	declare @new_tb_attribute table 
--	(NEW_ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))

----07 get the new rows from Reviewer.dbo.TB_ATTRIBUTE and put them into @new_tb_attribute
--	insert into @new_tb_attribute (NEW_ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
--		select ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID  
--		from Reviewer.dbo.TB_ATTRIBUTE
--		where OLD_ATTRIBUTE_ID in -- old_attribute_id will identify the items we want
--		(select attribute_id from @attribute_id) --wrong! you are getting attributes from other set_id's
--		and OLD_ATTRIBUTE_ID not like 'AT%'
--		and OLD_ATTRIBUTE_ID not like 'EX%'
--		and OLD_ATTRIBUTE_ID not like 'IC%'

--	--D
--	--select * from @new_tb_attribute


----09 update @tb_attribute_set with the new ATTRIBUTE_IDs sitting in @new_tb_attribute
--	update @tb_attribute_set 
--	set ATTRIBUTE_ID = 
--	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
--	where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
--	where exists
--	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
--		where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
	
	
--	--E
--	--select * from @tb_attribute_set


----08 update @tb_attribute_set with a new PARENT_ATTRIBUTE_ID
--	-- the new PARENT_ATTRIBUTE_ID is the 
--	update @tb_attribute_set
--	set PARENT_ATTRIBUTE_ID = 
--	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
--	where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID
--	and PARENT_ATTRIBUTE_ID != 0)
--	where exists
--	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
--		where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID and PARENT_ATTRIBUTE_ID != 0)
	
	
--	-- clean up the nulls
--	update @tb_attribute_set
--	set PARENT_ATTRIBUTE_ID = 0
--	where PARENT_ATTRIBUTE_ID is null
	
--	--F
--	--select * from @tb_attribute_set
	


----10 put @tb_attribute_set into Reviewer.dbo.TB_ATTRIBUTE_SET
--	insert into Reviewer.dbo.TB_ATTRIBUTE_SET (ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)	
--		select ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
--		from @tb_attribute_set

----11 place the new codeset at the bottom of the list.
--	declare @number_sets int set @number_sets = 0
--	select @number_sets = COUNT(*) from [Reviewer].[dbo].[TB_REVIEW_SET]
--	where REVIEW_ID = @DESTINATION_REVIEW_ID
--	-- set the position
--	update [Reviewer].[dbo].[TB_REVIEW_SET] 
--	set SET_ORDER = @number_sets - 1
--	where REVIEW_ID = @DESTINATION_REVIEW_ID
--	and SET_ID = @DESTINATION_SET_ID

----12 Clear out OLD_ATTRIBUTE_ID from TB_ATTRIBUTE in the new codeset since
--	update Reviewer.dbo.TB_ATTRIBUTE
--	set OLD_ATTRIBUTE_ID = null
--	where ATTRIBUTE_ID in 
--	(select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
--		where SET_ID = @DESTINATION_SET_ID)
	

--COMMIT TRANSACTION
--END TRY

--BEGIN CATCH
--IF (@@TRANCOUNT > 0)
--begin	
--ROLLBACK TRANSACTION
--set @RESULT = 'Unable to copy. Please contact EPPI-Support'
--end
--END CATCH


--RETURN

--SET NOCOUNT OFF























--------------------------------


--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_LinkCodeset]    Script Date: 02/08/2012 13:52:37 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO




--CREATE procedure [dbo].[st_LinkCodeset]
--(
--	@SOURCE_SET_ID int,
--	@SOURCE_REVIEW_ID int,
--	@DESTINATION_REVIEW_ID int,
--	@CONTACT_ID int,
--	@RESULT nvarchar(50) output
--)

--As

--SET NOCOUNT ON

--	declare @SOURCE_SET_NAME nvarchar (255)

--BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
--BEGIN TRANSACTION

--	-- find the number of codesets in the review so we can place the new one at the bottom.
--	declare @number_sets int set @number_sets = 0
--	select @number_sets = COUNT(*) from Reviewer.dbo.TB_REVIEW_SET
--	where REVIEW_ID = @DESTINATION_REVIEW_ID

--	insert into Reviewer.dbo.TB_REVIEW_SET (REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL, SET_ORDER)
--	values (@DESTINATION_REVIEW_ID, @SOURCE_SET_ID, 0, 1, @number_sets - 1)

--	-- prefix the new codeset with 'LINKED - ' unless it already is.
--	--  This will show up in all reviews but at least we will know
--	--  the codeset is linked
--	declare @IsLink nvarchar(6) set @IsLink = null 
--	select @IsLink = substring (SET_NAME, 0, 6) 
--	from Reviewer.dbo.TB_SET
--	where SET_ID = @SOURCE_SET_ID

--	if @IsLink != 'LINKED'
--	begin
--		update Reviewer.dbo.TB_SET
--		set SET_NAME = 'LINKED - ' + SET_NAME
--		where SET_ID = @SOURCE_SET_ID
--	end
	

--COMMIT TRANSACTION
--END TRY

--BEGIN CATCH
--IF (@@TRANCOUNT > 0)
--begin	
--ROLLBACK TRANSACTION
--set @RESULT = 'Unable to link codeset'
--end
--END CATCH


--RETURN

--SET NOCOUNT OFF



--GO

--------------------------------








---------- table edits
----tb_extension_types
------change APPLIES_TO to char(3)

----tb_site_lic
------change NOTES to nvarchar(2000)


-----------non-site_lic sp edited
----st_contactdetailsgetall
----st_contactlogin
----st_expirydatebulkadjust
----st_extensiontypesget
----st_reviewdetails
----st_reviewdetailsgetall


-----------site_lic sp new
----st_site_lic_create_or_edit
----st_site_lic_details_create_or_edit_andor_activate
----st_site_lic_edit_by_licadm
----st_site_lic_extension_record_get
----st_site_lic_get
----st_site_lic_get_all
----st_site_lic_get_all_packages
----st_site_lic_get_package
----st_site_lic_get_package_log
----st_site_lic_remove_review


-----------site_lic sp edited
----st_site_lic_add_review
----st_site_lic_add_remove_admin
----st_site_lic_add_remove_contact
----st_site_lic_get_accounts
----st_site_lic_get_details



--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ContactDetailsGetAll]    Script Date: 01/26/2012 11:43:39 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--ALTER PROCEDURE [dbo].[st_ContactDetailsGetAll] 
--(
--	@ER4AccountsOnly bit
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
 
--	if @ER4AccountsOnly = 1
--	begin        
--		/*
--		SELECT * FROM Reviewer.dbo.tb_CONTACT 
--		where EXPIRY_DATE > '2010-03-20 00:00:01' or
--		(EXPIRY_DATE is null and MONTHS_CREDIT != 0)
--		*/
--		select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
--		max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
--				CASE when l.[EXPIRY_DATE] is not null 
--				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
--					then l.[EXPIRY_DATE]
--				else c.[EXPIRY_DATE]
--				end as 'EXPIRY_DATE', 
--		c.MONTHS_CREDIT, c.CREATOR_ID,
--		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
--		l.SITE_LIC_ID, l.SITE_LIC_NAME
--		,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
--		from Reviewer.dbo.TB_CONTACT c
--		left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
--		on c.CONTACT_ID = lt.CONTACT_ID
--		left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
--		left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
--		where c.EXPIRY_DATE > '2010-03-20 00:00:01' or
--		(c.EXPIRY_DATE is null and MONTHS_CREDIT != 0)
		
--		group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
--		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
--		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		
--	end
--	else
--	begin
--		/*
--		SELECT * FROM Reviewer.dbo.tb_CONTACT
--		*/
		
--		select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
--		max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
--				CASE when l.[EXPIRY_DATE] is not null 
--				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
--					then l.[EXPIRY_DATE]
--				else c.[EXPIRY_DATE]
--				end as 'EXPIRY_DATE', 
--		c.MONTHS_CREDIT, c.CREATOR_ID,
--		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
--		l.SITE_LIC_ID, l.SITE_LIC_NAME
--		,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
--		from Reviewer.dbo.TB_CONTACT c
--		left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
--		on c.CONTACT_ID = lt.CONTACT_ID
--		left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
--		left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
--			group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
--		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
--		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		
--	end
       
--END


--GO




--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ContactLogin]    Script Date: 01/26/2012 11:40:30 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Jeff>
---- ALTER date: <24/03/10>
---- Description:	<gets contact details when loging in>
---- =============================================
--ALTER PROCEDURE [dbo].[st_ContactLogin] 
--(
--	@USERNAME nvarchar(50),
--	@PASSWORD nvarchar(50),
--	@IP_ADDRESS nvarchar(50)
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--	SELECT c.*, COUNT(sla.CONTACT_ID) as IsSLA
--	FROM Reviewer.dbo.TB_CONTACT c
--	Left outer join TB_SITE_LIC_ADMIN sla on sla.CONTACT_ID = c.CONTACT_ID
--	where c.USERNAME = @USERNAME
--	AND c.[PASSWORD] = @PASSWORD
--	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME,
--	c.PASSWORD, c.LAST_LOGIN, c.DATE_CREATED, c.EMAIL, c.EXPIRY_DATE,
--	c.MONTHS_CREDIT, c.CREATOR_ID,c.TYPE, c.IS_SITE_ADMIN, c.DESCRIPTION
	
--	RETURN
--END

--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ExpiryDateBulkAdjust]    Script Date: 01/26/2012 11:44:06 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


--ALTER procedure [dbo].[st_ExpiryDateBulkAdjust]
--(
--	@ADD_OR_REMOVE nvarchar(10),
--	@NUMBER_DAYS int,
--	@DATE date,
--	@CONTACT_ID int,
--	@EXTENSION_NOTES nvarchar(500),
--	@RESULT_C nvarchar(50) output,
--	@RESULT_R nvarchar(50) output,
--	@RESULT_SL nvarchar (50) output
--)

--As

--SET NOCOUNT ON
--declare @COST int
--declare @ACCOUNT_COST int

--CREATE TABLE #EXPIRY_TABLE (
--DATE_OF_EDIT datetime,
--TYPE_EXTENDED char(1),
--ID_EXTENDED int,
--NEW_EXPIRY_DATE date,
--OLD_EXPIRY_DATE date,
--EXTENDED_BY_ID int,
--EXTENSION_TYPE_ID int,
--EXTENSION_NOTES nvarchar(500))

--CREATE TABLE #EXPIRY_TABLE_SITE_LIC (
--SITE_LIC_ID int,
--SITE_LIC_DETAILS_ID int,
--EXTENDED_BY_ID int,
--ID_EXTENDED int,
--CHANGE_TYPE nvarchar(50),
--DATE_OF_EDIT datetime,
--REASON nvarchar(2000))


--if @ADD_OR_REMOVE = 'Remove'
--begin
--	set @NUMBER_DAYS = @NUMBER_DAYS * -1
--end


--BEGIN TRY
--BEGIN TRANSACTION

--	-- Contacts ---------------------------------------------------------
--	insert into #EXPIRY_TABLE (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE, OLD_EXPIRY_DATE,
--		EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
--	select GETDATE(), 0, c.CONTACT_ID, DATEADD(day, @NUMBER_DAYS, EXPIRY_DATE), EXPIRY_DATE,
--		@CONTACT_ID, 8, @EXTENSION_NOTES 
--	from Reviewer.dbo.TB_CONTACT c
--	where c.EXPIRY_DATE >= @DATE
	
--	update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(day, @NUMBER_DAYS, EXPIRY_DATE) 
--	where EXPIRY_DATE >= @DATE
	
--	insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
--		OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
--	select * from #EXPIRY_TABLE
	
--	select * from #EXPIRY_TABLE
--	set @RESULT_C = @@ROWCOUNT
	
--	delete from #EXPIRY_TABLE
	
	
--	-- Reviews ----------------------------------------------------------
--	insert into #EXPIRY_TABLE (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE, OLD_EXPIRY_DATE,
--		EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
--	select GETDATE(), 1, r.REVIEW_ID, DATEADD(day, @NUMBER_DAYS, EXPIRY_DATE), EXPIRY_DATE,
--		@CONTACT_ID, 8, @EXTENSION_NOTES 
--	from Reviewer.dbo.TB_REVIEW r
--	where r.EXPIRY_DATE >= @DATE
	
--	update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(day, @NUMBER_DAYS, EXPIRY_DATE) 
--	where EXPIRY_DATE >= @DATE
	
--	insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
--		OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
--	select * from #EXPIRY_TABLE		
	
--	select * from #EXPIRY_TABLE
--	set @RESULT_R = @@ROWCOUNT
	
--	delete from #EXPIRY_TABLE
	
--	-- Site License -- put an entry into both TB_EXPIRY_EDIT_LOG and TB_SITE_LIC_LOG ------------------------
--	insert into #EXPIRY_TABLE_SITE_LIC (SITE_LIC_ID, SITE_LIC_DETAILS_ID, EXTENDED_BY_ID, ID_EXTENDED, CHANGE_TYPE,
--		DATE_OF_EDIT, REASON)
--	select s_l.SITE_LIC_ID, null, @CONTACT_ID, s_l.SITE_LIC_ID, 'Maintenance extension', GETDATE(), @EXTENSION_NOTES
--	from Reviewer.dbo.TB_SITE_LIC s_l
--	where s_l.EXPIRY_DATE >= @DATE
	
--	update #EXPIRY_TABLE_SITE_LIC
--	set SITE_LIC_DETAILS_ID = 
--	(select top 1 ld.SITE_LIC_DETAILS_ID from TB_SITE_LIC_DETAILS ld 
--        inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and ld.VALID_FROM is not null
--        and fs.IS_ACTIVE = 0 and ld.SITE_LIC_ID = #EXPIRY_TABLE_SITE_LIC.SITE_LIC_ID
--        order by ld.VALID_FROM desc)
--    where #EXPIRY_TABLE_SITE_LIC.SITE_LIC_DETAILS_ID is null
    
    
--    insert into #EXPIRY_TABLE (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE, OLD_EXPIRY_DATE,
--		EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
--	select GETDATE(), 2, s_l.SITE_LIC_ID, DATEADD(day, @NUMBER_DAYS, EXPIRY_DATE), EXPIRY_DATE,
--		@CONTACT_ID, 8, @EXTENSION_NOTES 
--	from Reviewer.dbo.TB_SITE_LIC s_l
--	where s_l.EXPIRY_DATE >= @DATE
	
--	update #EXPIRY_TABLE
--	set ID_EXTENDED = 
--	(select #EXPIRY_TABLE_SITE_LIC.SITE_LIC_DETAILS_ID from #EXPIRY_TABLE_SITE_LIC 
--	where #EXPIRY_TABLE_SITE_LIC.SITE_LIC_ID = #EXPIRY_TABLE.ID_EXTENDED)
	
--	update Reviewer.dbo.TB_SITE_LIC set EXPIRY_DATE = DATEADD(day, @NUMBER_DAYS, EXPIRY_DATE) 
--	where EXPIRY_DATE >= @DATE
	
--	insert into TB_SITE_LIC_LOG (SITE_LIC_DETAILS_ID, CONTACT_ID, AFFECTED_ID, CHANGE_TYPE,
--		DATE, REASON)
--	select SITE_LIC_DETAILS_ID, EXTENDED_BY_ID, ID_EXTENDED, CHANGE_TYPE,
--		DATE_OF_EDIT, REASON
--	from #EXPIRY_TABLE_SITE_LIC
	
--	insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
--		OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
--	select * from #EXPIRY_TABLE	
	
--	select * from #EXPIRY_TABLE_SITE_LIC
--	set @RESULT_SL = @@ROWCOUNT

--	drop table #EXPIRY_TABLE_SITE_LIC
--	drop table #EXPIRY_TABLE

--COMMIT TRANSACTION
--END TRY

--BEGIN CATCH
--IF (@@TRANCOUNT > 0)
--begin	
--ROLLBACK TRANSACTION
--set @RESULT_C = 'Invalid'
--end
--END CATCH

--RETURN

--SET NOCOUNT OFF


--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ExtensionTypesGet]    Script Date: 01/26/2012 11:44:27 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--ALTER PROCEDURE [dbo].[st_ExtensionTypesGet] 
--(
--	@APPLIES_TO nvarchar(50)
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    if @APPLIES_TO = 'Contact'
--    begin
--		select * from TB_EXTENSION_TYPES 
--		where (APPLIES_TO = '100' or APPLIES_TO = '111')
--		order by [ORDER]
--    end
    
--    if @APPLIES_TO = 'Review'
--    begin
--		select * from TB_EXTENSION_TYPES 
--		where (APPLIES_TO = '010' or APPLIES_TO = '111')
--		order by [ORDER]
--    end
    
--    if @APPLIES_TO = 'SiteLic'
--    begin
--		select * from TB_EXTENSION_TYPES 
--		where (APPLIES_TO = '001' or APPLIES_TO = '111')
--		order by [ORDER]
--    end

--	RETURN

--END


--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ReviewDetails]    Script Date: 01/26/2012 11:44:46 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--ALTER PROCEDURE [dbo].[st_ReviewDetails] 
--(
--	@REVIEW_ID nvarchar(50)
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here	
--	select r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, r.REVIEW_NUMBER, 
--	r.DATE_CREATED, r.OLD_REVIEW_GROUP_ID,
--		CASE when l.[EXPIRY_DATE] is not null 
--		and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
--			then l.[EXPIRY_DATE]
--		else r.[EXPIRY_DATE]
--		end as 'EXPIRY_DATE', 
--	r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
--	l.SITE_LIC_ID, l.SITE_LIC_NAME
--	from Reviewer.dbo.TB_REVIEW r
--	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
--	left join Reviewer.dbo.TB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
--	left join Reviewer.dbo.TB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
--	where r.REVIEW_ID = @REVIEW_ID
	
--	group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
--	r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
--	l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME, r.REVIEW_NUMBER, r.OLD_REVIEW_GROUP_ID
--	order by r.REVIEW_NAME
	

--	RETURN

--END

--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsGetAll]    Script Date: 01/26/2012 11:45:04 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--ALTER PROCEDURE [dbo].[st_ReviewDetailsGetAll] 
--(
--	@SHAREABLE bit
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    if @SHAREABLE = 1
--	begin        
--		/*
--		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
--		r.FUNDER_ID, c.CONTACT_NAME, r.MONTHS_CREDIT
--		FROM Reviewer.dbo.tb_REVIEW r
--		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
--		where 
--		(r.EXPIRY_DATE is not null) OR
--		(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0)*/
		
--		select r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME,  
--		r.DATE_CREATED, 
--			CASE when l.[EXPIRY_DATE] is not null 
--			and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
--				then l.[EXPIRY_DATE]
--			else r.[EXPIRY_DATE]
--			end as 'EXPIRY_DATE', 
--		r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
--		l.SITE_LIC_ID, l.SITE_LIC_NAME
--		from Reviewer.dbo.TB_REVIEW r
--		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
--		left join Reviewer.dbo.TB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
--		left join Reviewer.dbo.TB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
--		where 
--			(r.EXPIRY_DATE is not null) OR
--			(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0)
		
--		group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
--		r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
--		l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
--		order by r.REVIEW_NAME
		 
--	end
--	else
--	begin
--		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
--		r.FUNDER_ID, c.CONTACT_NAME
--		FROM Reviewer.dbo.tb_REVIEW r
--		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID 
--		where (r.EXPIRY_DATE is null and r.MONTHS_CREDIT = 0)
--	end

--	RETURN

--END


--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Review]    Script Date: 01/26/2012 11:46:13 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--ALTER PROCEDURE [dbo].[st_Site_Lic_Add_Review]
--	-- Add the parameters for the stored procedure here
--	@lic_id int
--	, @admin_ID int
--	, @review_id int
--	, @res int output
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--	declare @review_name nvarchar(1000)
--	set @res = 0

--    -- Insert statements for procedure here
    
--    -- initial check to see if review exists
--    select @review_name = REVIEW_NAME from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @review_id
--	if @@ROWCOUNT = 1 
--	begin
--		declare @ck int, @ck2 int, @lic_det_id int
--		set @ck = (SELECT count(contact_id) from TB_SITE_LIC_ADMIN 
--		where (CONTACT_ID = @admin_ID or @admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT 
--		where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)
--			) and SITE_LIC_ID = @lic_id)
--		if @ck < 1 set @res = -1 --first check: see if supplied admin is actually and admin!
		
--		else -- initial checks went OK, let's see if we can do it
--		begin
--			set @ck = (select count(review_id) from Reviewer.dbo.TB_SITE_LIC_REVIEW where REVIEW_ID = @review_id)
--			set @lic_det_id = (SELECT TOP 1 ld.SITE_LIC_DETAILS_ID from Reviewer.dbo.TB_SITE_LIC sl 
--									inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
--									inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
--									and sl.SITE_LIC_ID = @lic_id
--								Order by ld.VALID_FROM desc
--								)
--			--@ck2 counts how many reviews can be added to current lic
--			set @ck2 = (select d.REVIEWS_ALLOWANCE - count(review_id) from TB_SITE_LIC_DETAILS d inner join
--							 Reviewer.dbo.TB_SITE_LIC_REVIEW lr on lr.SITE_LIC_DETAILS_ID = @lic_det_id
--								and lr.SITE_LIC_DETAILS_ID = d.SITE_LIC_DETAILS_ID
--							 group by d.REVIEWS_ALLOWANCE
--							 )--count how many reviews can still be added
--			if @ck != 0 -- review is already in a site lic
--			begin
--				set @ck = (select count(review_id) from Reviewer.dbo.TB_SITE_LIC_REVIEW where REVIEW_ID = @review_id and SITE_LIC_ID = @lic_id)
--				if @ck = 1 set @res = -3 --review already in this site_lic
--				else set @res = -4 --review is in some other site_lic
--			end
--			else if @ck2 < 1 --no allowance available, all review slots for current license have been used
--			begin
--				set @res = -5
--			end
--			else
--			begin --all is well, let's do something!
--				begin transaction --make sure we don't commit only half of the mission critical data! 
--				--(we assume the update below will work, only checking for the other statement)
--				update Reviewer.dbo.TB_REVIEW set [EXPIRY_DATE] = GETDATE()
--					where REVIEW_ID = @review_id and REVIEW_NAME = @review_name and [EXPIRY_DATE] is null
--				insert into Reviewer.dbo.TB_SITE_LIC_REVIEW (
--					[SITE_LIC_ID]
--					,[SITE_LIC_DETAILS_ID]
--					,[REVIEW_ID]
--					,[ADDED_BY]
--					) values (
--					@lic_id, @lic_det_id, @review_id, @admin_ID
--					)
--				if @@ROWCOUNT = 1
--				begin --write success log
--					commit transaction --all is well, commit
--					insert into TB_SITE_LIC_LOG (
--						[SITE_LIC_DETAILS_ID]
--						  ,[CONTACT_ID]
--						  ,[AFFECTED_ID]
--						  ,[CHANGE_TYPE]
--					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @review_id, 'add review'
--						from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--						order by DATE_CREATED desc
--				end
--				else --write failure log, if this is fired, there is a bug somewhere
--				begin
--					rollback transaction --BAD! something went wrong!
--					insert into TB_SITE_LIC_LOG (
--						[SITE_LIC_DETAILS_ID]
--						  ,[CONTACT_ID]
--						  ,[AFFECTED_ID]
--						  ,[CHANGE_TYPE]
--					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @review_id, 'add review: failed!'
--						from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--						order by DATE_CREATED desc	
--					set @res = -6
--				end
--			end
--		end
--	end
--	else
--	begin
--		set @res = -2
--	end
	
--	--select r.review_id, review_name from reviewer.dbo.TB_SITE_LIC_REVIEW lr
--	--	inner join Reviewer.dbo.TB_REVIEW r on lr.REVIEW_ID = r.REVIEW_ID
--	--	where SITE_LIC_ID = @lic_id
	 
--	return @res
--	-- error codes: -1 = supplied admin_id is not an admin of this site lice
--	--				-2 = review_id does not exist
--	--				-3 = review already in this site_lic
--	--				-4 = review is in some other site_lic
--	--				-5 = no allowance available, all review slots for current license have been used
--	--				-6 = all seemed well but couldn't write changes! BUG ALERT
--END



--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Remove_Admin]    Script Date: 01/26/2012 11:46:43 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--ALTER PROCEDURE [dbo].[st_Site_Lic_Add_Remove_Admin]
--	-- Add the parameters for the stored procedure here
--	@lic_id int
--	, @admin_ID int
--	, @contact_email nvarchar(500)
--	, @res int output
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
--    -- Insert statements for procedure here
--    declare @contact_id int
--	set @res = 0
    
--    -- initial check to see if email exists
--	select @contact_id = CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = @contact_email
--	if @@ROWCOUNT = 1 
--	begin
--		declare @ck int
--		set @ck = (SELECT count(contact_id) from TB_SITE_LIC_ADMIN 
--		where (CONTACT_ID = @admin_ID or @admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1))
--				and SITE_LIC_ID = @lic_id)
--		if @ck < 1 set @res = -1 --first check: see if supplied admin is actually and admin!
--		else if  (select count(contact_id) from Reviewer.dbo.TB_CONTACT where @contact_email = EMAIL and @contact_id = CONTACT_ID) != 1
--			--second check, see if c_id and email exist
--			set @res = -2
--		else -- checks went OK, let's see if we can do it
--		begin
--			set @ck = (select count(contact_id) from TB_SITE_LIC_ADMIN where CONTACT_ID = @contact_id)
--			if @ck = 1 -- contact is an admin, we should remove it
--			begin
--				delete from TB_SITE_LIC_ADMIN where CONTACT_ID = @contact_id and @lic_id = SITE_LIC_ID
--				if @@ROWCOUNT = 1
--				begin --write success log
--					insert into TB_SITE_LIC_LOG (
--						[SITE_LIC_DETAILS_ID]
--						  ,[CONTACT_ID]
--						  ,[AFFECTED_ID]
--						  ,[CHANGE_TYPE]
--					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin'
--						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
--						order by DATE_CREATED desc
--				end
--				else --write failure log, if this is fired, there is a bug somewhere
--				begin
--					insert into TB_SITE_LIC_LOG (
--						[SITE_LIC_DETAILS_ID]
--						  ,[CONTACT_ID]
--						  ,[AFFECTED_ID]
--						  ,[CHANGE_TYPE]
--					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin: failed!'
--						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
--						order by DATE_CREATED desc	
--					set @res = -3
--				end
--			end	
			
--			else --contact is not an admin, we should add it
--			begin
--				insert into TB_SITE_LIC_ADMIN (CONTACT_ID, SITE_LIC_ID)
--				values (@contact_id, @lic_id)
--				if @@ROWCOUNT = 1
--				begin
--					insert into TB_SITE_LIC_LOG (
--						[SITE_LIC_DETAILS_ID]
--						  ,[CONTACT_ID]
--						  ,[AFFECTED_ID]
--						  ,[CHANGE_TYPE]
--					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin'
--						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
--						order by DATE_CREATED desc
--				end
--				else
--				begin
--					insert into TB_SITE_LIC_LOG (
--						[SITE_LIC_DETAILS_ID]
--						  ,[CONTACT_ID]
--						  ,[AFFECTED_ID]
--						  ,[CHANGE_TYPE]
--					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin: failed!'
--						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
--						order by DATE_CREATED desc	
--					set @res = -4
--				end
--			end
--		end
--	end
--	--select c.CONTACT_ID, CONTACT_NAME, EMAIL from TB_SITE_LIC_ADMIN a
--	--	inner join Reviewer.dbo.TB_CONTACT c on a.CONTACT_ID = c.CONTACT_ID
--	--where SITE_LIC_ID = @lic_id
	
--	return @res 
--	-- error codes: -1 = supplied admin_id is not an admin of this site lice
--	--				-2 = contact_id already in a site license
--	--				-3 = no allowance available, all account slots for current license have been used
--	--				-4 = tried to remove account but couldn't write changes! BUG ALERT
--	--				-5 = tried to add account but couldn't write changes! BUG ALERT
--	--				-6 = email check returned no contact_id or multiple contact_ids
	
--END



--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Remove_Contact]    Script Date: 01/26/2012 11:46:58 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--ALTER PROCEDURE [dbo].[st_Site_Lic_Add_Remove_Contact]
--(
--	  @lic_id int
--	, @admin_ID int
--	, @contact_email nvarchar(500)
--	, @res int output
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
--	-- Insert statements for procedure here
	
--	declare @contact_id int
--	set @res = 0

--	-- initial check to see if email exists
--	select @contact_id = CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = @contact_email
--	if @@ROWCOUNT = 1 
--	begin
--		declare @ck int, @ck2 int = 0, @acc_all int
--		set @ck = (SELECT count(contact_id) from TB_SITE_LIC_ADMIN 
--			where (CONTACT_ID = @admin_ID or @admin_ID in 
--				(select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)) 
--			and SITE_LIC_ID = @lic_id)
--		set @acc_all = (select top 1 ACCOUNTS_ALLOWANCE from TB_SITE_LIC_DETAILS 
--			where SITE_LIC_ID = @lic_id and VALID_FROM is not null order by VALID_FROM desc)
--		set @ck2 = (select @acc_all - COUNT(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT 
--			where SITE_LIC_ID = @lic_id)
--		if @ck < 1 set @res = -1 --first check: see if supplied admin id is actually an admin!
		
--		else if  (select count(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT 
--			where @contact_id = CONTACT_ID and SITE_LIC_ID != @lic_id) != 0
--			--second check, see if contact_id is already in a site license (not including this one)
--			set @res = -2	
		
--		else -- checks went OK, let's see if we can do it
--		begin
--			set @ck = (select count(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT where CONTACT_ID = @contact_id)
--			if @ck = 1 -- contact is in the license, we should remove it
--			begin
--				delete from Reviewer.dbo.TB_SITE_LIC_CONTACT where CONTACT_ID = @contact_id and @lic_id = SITE_LIC_ID
--				if @@ROWCOUNT = 1
--				begin --write success log
--					insert into TB_SITE_LIC_LOG (
--						[SITE_LIC_DETAILS_ID]
--						  ,[CONTACT_ID]
--						  ,[AFFECTED_ID]
--						  ,[CHANGE_TYPE]
--					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove contact'
--						from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--						order by DATE_CREATED desc
--				end
--				else --write failure log, if this is fired, there is a bug somewhere
--				begin
--					insert into TB_SITE_LIC_LOG (
--						[SITE_LIC_DETAILS_ID]
--						  ,[CONTACT_ID]
--						  ,[AFFECTED_ID]
--						  ,[CHANGE_TYPE]
--					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove contact: failed!'
--						from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--						order by DATE_CREATED desc	
--					set @res = -4
--				end
--			end	
			
--			else if @ck2 < 1 --accounts allowance is all used up
--			set @res = -3
			
--			else --contact is not in the license, we should add it
--			begin
--				insert into Reviewer.dbo.TB_SITE_LIC_CONTACT (CONTACT_ID, SITE_LIC_ID)
--				values (@contact_id, @lic_id)
--				if @@ROWCOUNT = 1
--				begin
--					insert into TB_SITE_LIC_LOG (
--						[SITE_LIC_DETAILS_ID]
--						  ,[CONTACT_ID]
--						  ,[AFFECTED_ID]
--						  ,[CHANGE_TYPE]
--					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add contact'
--						from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--						order by DATE_CREATED desc
--				end
--				else
--				begin
--					insert into TB_SITE_LIC_LOG (
--						[SITE_LIC_DETAILS_ID]
--						  ,[CONTACT_ID]
--						  ,[AFFECTED_ID]
--						  ,[CHANGE_TYPE]
--					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add contact: failed!'
--						from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--						order by DATE_CREATED desc	
--					set @res = -5
--				end
--			end
--		end
--	end
--	else
--	begin
--		set @res = -6
--	end
	
--	--select c.CONTACT_ID, CONTACT_NAME, EMAIL from Reviewer.dbo.TB_SITE_LIC_CONTACT lc 
--	--	inner join Reviewer.dbo.TB_CONTACT c on lc.CONTACT_ID = c.CONTACT_ID
--	--	where SITE_LIC_ID = @lic_id
	
--	return @res
--	-- error codes: -1 = supplied admin_id is not an admin of this site lice
--	--				-2 = contact_id already in a site license
--	--				-3 = no allowance available, all account slots for current license have been used
--	--				-4 = tried to remove account but couldn't write changes! BUG ALERT
--	--				-5 = tried to add account but couldn't write changes! BUG ALERT
--	--				-6 = email check returned no contact_id or multiple contact_ids
--END



--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Accounts]    Script Date: 01/26/2012 11:47:22 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:           <Author,,Name>
---- Create date: <Create Date,,>
---- Description:      <Description,,>
---- =============================================
--ALTER PROCEDURE [dbo].[st_Site_Lic_Get_Accounts] 
--(
--       @lic_id int
--)
--AS
--BEGIN
--       -- SET NOCOUNT ON added to prevent extra result sets from
--       -- interfering with SELECT statements.
--       SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    declare @det_id int
--    set @det_id = (select top 1 ld.SITE_LIC_DETAILS_ID from TB_SITE_LIC_DETAILS ld 
--        inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and ld.VALID_FROM is not null
--        and fs.IS_ACTIVE = 0 and ld.SITE_LIC_ID = @lic_id
--        order by ld.VALID_FROM desc)
--       --first, reviews that were added in the past (not associated with currently active SITE_LIC_DETAILS)
--       select * from Reviewer.dbo.TB_REVIEW r
--              inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID and SITE_LIC_ID = @lic_id and SITE_LIC_DETAILS_ID != @det_id
--       --second, reviews that were added to the currently active SITE_LIC_DETAILS
--       select * from Reviewer.dbo.TB_REVIEW r
--              inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID and SITE_LIC_ID = @lic_id and SITE_LIC_DETAILS_ID = @det_id
--       --accounts in the license
--       select * from Reviewer.dbo.TB_CONTACT c
--              inner join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID and lc.SITE_LIC_ID = @lic_id
--       --license admins
--       select * from Reviewer.dbo.TB_CONTACT c
--              inner join TB_SITE_LIC_ADMIN la on c.CONTACT_ID = la.CONTACT_ID and la.SITE_LIC_ID = @lic_id
--END

--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Details]    Script Date: 01/26/2012 11:47:36 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--ALTER PROCEDURE [dbo].[st_Site_Lic_Get_Details] 
--(
--	@admin_ID int
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- first results set: currently active license TB_SITE_LIC_DETAILS.VALID_FROM is something and FOR_SALE.IS_ACTIVE = false
--	SELECT TOP 1 *, c.CONTACT_NAME as ADMIN_NAME, ld.DATE_CREATED as DATE_PACKAGE_CREATED from Reviewer.dbo.TB_SITE_LIC sl 
--		inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
--		inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
--		inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and CONTACT_ID = @admin_ID
--		inner join Reviewer.dbo.tb_CONTACT c on c.CONTACT_ID = @admin_ID
--	Order by ld.VALID_FROM desc	
--	-- second results set: currently active renewal offer TB_SITE_LIC_DETAILS.VALID_FROM is NULL and FOR_SALE.IS_ACTIVE = True
--	SELECT TOP 1 *, c.CONTACT_NAME as ADMIN_NAME, ld.DATE_CREATED as DATE_PACKAGE_CREATED from Reviewer.dbo.TB_SITE_LIC sl 
--		inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is null
--		inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 1
--		inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and CONTACT_ID = @admin_ID
--		inner join Reviewer.dbo.tb_CONTACT c on c.CONTACT_ID = @admin_ID
--	Order by ld.VALID_FROM desc
--END


--GO

--------------------------------------------------

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Create_or_Edit]    Script Date: 01/26/2012 11:48:03 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO




---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Create_or_Edit] 
--	-- Add the parameters for the stored procedure here
--(
--	  @LIC_ID nvarchar(50)
--	, @creator_id INT
--	, @admin_id  int -- who will be the first administrator of the site lic, 
--	, @SITE_LIC_NAME nvarchar(50)
--	, @COMPANY_NAME nvarchar(50)
--	, @COMPANY_ADDRESS nvarchar(500)
--	, @TELEPHONE nvarchar(50)
--	, @NOTES nvarchar(4000)
--	, @DATE_CREATED datetime
--	, @RESULT nvarchar(50) output
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
--	declare @ck int
	
--	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
--	BEGIN TRANSACTION
--	if @LIC_ID = 'N/A' -- we are creating a new site license
--	BEGIN	
--		-- check if contact is already a site admin?
--		set @ck = (select count(contact_id) from TB_SITE_LIC_ADMIN 
--		where CONTACT_ID = @admin_id)
--		if @ck = 0 
--		BEGIN	
--			declare @L_ID int
			
--			insert into Reviewer.dbo.TB_SITE_LIC (SITE_LIC_NAME, COMPANY_NAME,
--				COMPANY_ADDRESS, TELEPHONE, NOTES, CREATOR_ID)
--			VALUES (@SITE_LIC_NAME, @COMPANY_NAME, @COMPANY_ADDRESS, @TELEPHONE,
--				@NOTES, @creator_id)	
--			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
--			begin
--				ROLLBACK TRANSACTION
--				set @RESULT = 'rollback'
--				return 
--			end
--			else
--			begin
--				set @L_ID = @@IDENTITY
--				insert into TB_SITE_LIC_ADMIN([SITE_LIC_ID], [CONTACT_ID]) 
--				VALUES (@L_ID, @admin_id)
--				if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
--				begin
--					ROLLBACK TRANSACTION
--					set @RESULT = 'rollback'
--					return 
--				end
--				else
--				begin
--					set @RESULT = @L_ID
--				end
--			end		
--		END
--		else
--		BEGIN
--			-- contact is already a site lic admin, don't proceed
--			set @RESULT = 'invalid adm'
--		END
--	END
--	else
--	BEGIN
--		-- get the existing admin
--		declare @L_ADM_ID int
--		select @L_ADM_ID = CONTACT_ID from TB_SITE_LIC_ADMIN where SITE_LIC_ID = @LIC_ID
		
--		set @ck = (select count(contact_id) from TB_SITE_LIC_ADMIN 
--		where CONTACT_ID = @admin_id and SITE_LIC_ID != @LIC_ID)
--		if @ck = 1 and @L_ADM_ID != @admin_id
--		BEGIN
--			-- adm is already used on another site license
--			set @RESULT = 'invalid adm'
--		END
--		else
--		begin
--			-- we are editing the site license details
--			update Reviewer.dbo.TB_SITE_LIC
--			set SITE_LIC_NAME = @SITE_LIC_NAME, 
--			COMPANY_NAME = @COMPANY_NAME,
--			COMPANY_ADDRESS = @COMPANY_ADDRESS,
--			TELEPHONE = @TELEPHONE,
--			NOTES = @NOTES,
--			DATE_CREATED = @DATE_CREATED
--			where SITE_LIC_ID = @LIC_ID
--			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
--			begin
--				ROLLBACK TRANSACTION
--				set @RESULT = 'rollback'
--				return 
--			end
--			else
--			begin			
--				update TB_SITE_LIC_ADMIN
--				set CONTACT_ID = @admin_id
--				where CONTACT_ID = @L_ADM_ID
--				if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
--				begin
--					ROLLBACK TRANSACTION
--					set @RESULT = 'rollback'
--					return 
--				end
--				else
--				begin			
--					set @RESULT = 'valid'
--				end
--			end		
--		end
--	END
	
--	COMMIT TRANSACTION
--	END TRY

--	BEGIN CATCH
--	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
--	set @RESULT = 'rollback' --to tell the code data was not committed!
--	END CATCH
	
--	return 

--END




--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Details_Create_or_Edit_AndOr_Activate]    Script Date: 01/26/2012 11:48:23 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO





---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Details_Create_or_Edit_AndOr_Activate] 
--	-- Add the parameters for the stored procedure here
--(
--	  @LIC_ID int
--	, @SITE_LIC_DETAILS_ID nvarchar(50)
--	, @creator_id INT 
--	, @Accounts int 
--	, @reviews int
--	, @months int
--	, @Totalprice int
--	, @pricePerMonth int
--	, @dateCreated date
--	, @forSaleID nvarchar(50)
--	, @VALID_FROM nvarchar(50)
--	, @EXPIRY_DATE nvarchar(50)
--	, @ER4_ADM nvarchar(50)
--	, @EXTENSION_TYPE nvarchar(50)
--	, @RESULT nvarchar(50) output
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
--	declare @ck int
	
--	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
--	BEGIN TRANSACTION
	
--	if @SITE_LIC_DETAILS_ID = 'N/A' -- we are creating a new site license package
--	BEGIN	
	
--		declare @FS_ID int
--		declare @SLD_ID int
		
--		insert into TB_FOR_SALE ([TYPE_NAME],[IS_ACTIVE],[LAST_CHANGED],
--			[PRICE_PER_MONTH],[DETAILS])
--		VALUES ('Site License', 1, GETDATE(),CAST((@Totalprice/@months) as int),'')
--		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
--		begin
--			ROLLBACK TRANSACTION
--			set @RESULT = 'rollback'
--			return 
--		end
--		else
--		begin
--			set @FS_ID = @@IDENTITY
--			INSERT into TB_SITE_LIC_DETAILS ([SITE_LIC_ID],[FOR_SALE_ID],
--			[ACCOUNTS_ALLOWANCE],[REVIEWS_ALLOWANCE],[MONTHS],[VALID_FROM])
--			values (@LIC_ID,@FS_ID,@Accounts,@reviews,@months,null)
				
--			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
--			begin
--				ROLLBACK TRANSACTION
--				set @RESULT = 'rollback'
--				return 
--			end
--			else
--			begin
--				set @SLD_ID = @@IDENTITY
				
--				if (@VALID_FROM != '') and (@EXPIRY_DATE != '')
--				begin
--					-- activate the package
--					update Reviewer.dbo.TB_SITE_LIC 
--					set EXPIRY_DATE = @EXPIRY_DATE
--					where SITE_LIC_ID = @LIC_ID
				
--					update TB_SITE_LIC_DETAILS 
--					set VALID_FROM = @VALID_FROM
--					where SITE_LIC_DETAILS_ID = @SLD_ID
					
--					update TB_FOR_SALE 
--					set IS_ACTIVE = 'False'
--					from TB_FOR_SALE fs 
--					inner join TB_SITE_LIC_DETAILS d on d.FOR_SALE_ID = fs.FOR_SALE_ID 
--					and d.SITE_LIC_DETAILS_ID = @SLD_ID	
					
--					insert into TB_SITE_LIC_LOG ([SITE_LIC_DETAILS_ID], [CONTACT_ID], 
--						[AFFECTED_ID],[CHANGE_TYPE], [REASON])
--					values (@SITE_LIC_DETAILS_ID, @ER4_ADM, @SITE_LIC_DETAILS_ID,
--						'ACTIVATE', 'This was done manually, not via the shop')			
--				end					
--				set @RESULT = 'valid'
--			end
--		end

--	END
--	else
--	BEGIN
--		-- this is an existing site license
--		update TB_SITE_LIC_DETAILS
--			set ACCOUNTS_ALLOWANCE = @Accounts, 
--			REVIEWS_ALLOWANCE = @reviews,
--			DATE_CREATED = @dateCreated,
--			MONTHS = @months
--			where SITE_LIC_ID = @LIC_ID
--			and SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
--		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
--		begin
--			ROLLBACK TRANSACTION
--			set @RESULT = 'rollback'
--			return 
--		end
--		else
--		begin	
--			update TB_FOR_SALE
--				set LAST_CHANGED = GETDATE(),
--				PRICE_PER_MONTH = @pricePerMonth
--				where FOR_SALE_ID = @forSaleID
--			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
--			begin
--				ROLLBACK TRANSACTION
--				set @RESULT = 'rollback'
--				return 
--			end
--			else
--			begin
--				if @EXTENSION_TYPE > 0
--				begin
--					-- put an entry into TB_EXPIRY_EDIT_LOG			
--					insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
--						OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
--					select GETDATE(), 2, @SITE_LIC_DETAILS_ID, @EXPIRY_DATE, s_l.EXPIRY_DATE, @ER4_ADM, @EXTENSION_TYPE, '' 
--					from Reviewer.dbo.TB_SITE_LIC s_l
--					where s_l.SITE_LIC_ID = @LIC_ID
--				end
--			end

--			if (@VALID_FROM != '') and (@EXPIRY_DATE != '') 
--			begin
--				-- change expiry date or activate the package
--				update Reviewer.dbo.TB_SITE_LIC 
--				set EXPIRY_DATE = @EXPIRY_DATE
--				where SITE_LIC_ID = @LIC_ID
			
--				update TB_SITE_LIC_DETAILS 
--				set VALID_FROM = @VALID_FROM
--				where SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
				
--				update TB_FOR_SALE 
--				set IS_ACTIVE = 'False'
--				from TB_FOR_SALE fs 
--				inner join TB_SITE_LIC_DETAILS d on d.FOR_SALE_ID = fs.FOR_SALE_ID 
--				and d.SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID	
				
--				insert into TB_SITE_LIC_LOG (
--				  [SITE_LIC_DETAILS_ID], [CONTACT_ID], [AFFECTED_ID]
--				  ,[CHANGE_TYPE], [REASON])
--				values (
--				  @SITE_LIC_DETAILS_ID, @ER4_ADM, @SITE_LIC_DETAILS_ID
--				  ,'Activate or Extension', 'This was done manually, not via the shop')				
--			end
--			set @RESULT = 'valid'

--		end

--	END

--	COMMIT TRANSACTION
--	END TRY

--	BEGIN CATCH
--	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
--	set @RESULT = 'rollback' --to tell the code data was not committed!
--	END CATCH
	
--	return

--END





--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Edit_By_LicAdm]    Script Date: 01/26/2012 11:48:38 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


--CREATE procedure [dbo].[st_Site_Lic_Edit_By_LicAdm]
--(
--	@SITE_LIC_ID int,
--	@COMPANY_NAME nvarchar(500),
--	@COMPANY_ADDRESS nvarchar(500),
--	@TELEPHONE nvarchar(50),
--	@NOTES nvarchar(2000)
--)

--As

--SET NOCOUNT ON

--	update Reviewer.dbo.TB_SITE_LIC
--	set COMPANY_NAME = @COMPANY_NAME, 
--	COMPANY_ADDRESS = @COMPANY_ADDRESS,
--	TELEPHONE = @TELEPHONE,
--	NOTES = @NOTES
--	where SITE_LIC_ID = @SITE_LIC_ID

--SET NOCOUNT OFF


--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Extension_Record_Get]    Script Date: 01/26/2012 11:48:53 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Extension_Record_Get] 
--(
--	@SITE_LIC_DETAILS_ID int
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--	select e_e_l.EXPIRY_EDIT_ID ,e_e_l.DATE_OF_EDIT, e_e_l.TYPE_EXTENDED, e_e_l.ID_EXTENDED, 
--	e_e_l.OLD_EXPIRY_DATE, e_e_l.NEW_EXPIRY_DATE,
--	e_e_l.EXTENDED_BY_ID, c.CONTACT_NAME, e_t.EXTENSION_TYPE, 
--	e_e_l.EXTENSION_NOTES
--	from TB_EXPIRY_EDIT_LOG e_e_l
--	inner join TB_EXTENSION_TYPES e_t on e_t.EXTENSION_TYPE_ID = e_e_l.EXTENSION_TYPE_ID
--	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = e_e_l.EXTENDED_BY_ID
--	where e_e_l.ID_EXTENDED = @SITE_LIC_DETAILS_ID
--	and e_e_l.TYPE_EXTENDED = 2

--	RETURN

--END



--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get]    Script Date: 01/26/2012 11:49:12 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Get] 
--	-- Add the parameters for the stored procedure here
	
--	@admin_ID int
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--DECLARE @License TABLE
--(
--    SITE_LIC_ID int,
--    SITE_LIC_NAME nvarchar(50),
--	COMPANY_NAME nvarchar(50),
--	COMPANY_ADDRESS nvarchar(500),
--	TELEPHONE nvarchar(50),
--	NOTES nvarchar(4000),
--	T_CREATOR_ID int,
--	CREATOR_NAME nvarchar(255),
--	ADMINISTRATOR_ID int,
--	ADMINISTRATOR_NAME nvarchar(255),
--	DATE_CREATED date	
--)

--	INSERT INTO @License (SITE_LIC_ID, SITE_LIC_NAME, COMPANY_NAME,
--	COMPANY_ADDRESS, TELEPHONE, NOTES, T_CREATOR_ID, DATE_CREATED, ADMINISTRATOR_ID)
--	select sl.SITE_LIC_ID, sl.SITE_LIC_NAME, sl.COMPANY_NAME, sl.COMPANY_ADDRESS,
--	sl.TELEPHONE, sl.NOTES, sl.CREATOR_ID, sl.DATE_CREATED, sla.CONTACT_ID
--	from Reviewer.dbo.TB_SITE_LIC sl
--	inner join TB_SITE_LIC_ADMIN sla on sla.SITE_LIC_ID = sl.SITE_LIC_ID
--	where sla.CONTACT_ID = @admin_ID

--	update @License
--	set CREATOR_NAME = 
--	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
--	where CONTACT_ID = T_CREATOR_ID)
	
--	update @License
--	set ADMINISTRATOR_NAME = 
--	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
--	where CONTACT_ID = ADMINISTRATOR_ID)

--	select * from @License
	

--END



--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_All]    Script Date: 01/26/2012 11:49:34 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO




---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Get_All] 

--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
 

--DECLARE @Site_Licenses TABLE
--	(
--		SITE_LIC_ID int,
--		SITE_LIC_NAME nvarchar(50),
--		CONTACT_ID int,
--		CONTACT_NAME nvarchar(500),
--		[EXPIRY_DATE] date	
--	)
--	DECLARE @site_lic_id int
--	DECLARE @first_contact_id int
--	DECLARE @counter int
--	set @counter = 0

--	INSERT INTO @Site_Licenses (SITE_LIC_ID, SITE_LIC_NAME, [EXPIRY_DATE])	
--	select s_l.SITE_LIC_ID, s_l.SITE_LIC_NAME, s_l.EXPIRY_DATE 
--	from Reviewer.dbo.TB_SITE_LIC s_l

--	DECLARE site_id CURSOR FOR 
--	SELECT SITE_LIC_ID from @Site_Licenses 
--	OPEN site_id
--	FETCH NEXT FROM site_id INTO @site_lic_id

--	WHILE @@FETCH_STATUS = 0
--	BEGIN	
--		DECLARE contact_id CURSOR FOR 
--		SELECT CONTACT_ID from TB_SITE_LIC_ADMIN sla
--		where sla.SITE_LIC_ID = @site_lic_id
--		OPEN contact_id
--		FETCH NEXT FROM contact_id INTO @first_contact_id
--		WHILE @@FETCH_STATUS = 0
--		BEGIN
--			if @counter = 0
--			begin
--				update @Site_Licenses
--				set CONTACT_ID = @first_contact_id
--				where SITE_LIC_ID = @site_lic_id
				
--				update @Site_Licenses
--				set CONTACT_NAME = 
--				(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT c
--				where c.CONTACT_ID = @first_contact_id)
--				where SITE_LIC_ID = @site_lic_id

--				set @counter = 1
--			end
--			FETCH NEXT FROM contact_id INTO @first_contact_id
--		END
--		CLOSE contact_id
--		DEALLOCATE contact_id
--		set @counter = 0
--		FETCH NEXT FROM site_id INTO @site_lic_id
--	END

--	CLOSE site_id
--	DEALLOCATE site_id


--select * from @Site_Licenses


       
--END




--GO


--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_all_Packages]    Script Date: 01/26/2012 11:49:48 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Get_all_Packages] 
--(	
--	@site_lic_ID int,
--	@site_lic_adm_ID int
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--	if @site_lic_ID = '0'
--	begin
--		-- we are using the adm parameter
--		SELECT s_l_c.*, f_s.* from TB_SITE_LIC_DETAILS s_l_c
--		inner join TB_FOR_SALE f_s on f_s.FOR_SALE_ID = s_l_c.FOR_SALE_ID
--		inner join TB_SITE_LIC_ADMIN s_l_a on s_l_a.SITE_LIC_ID = s_l_c.SITE_LIC_ID
--		where s_l_a.CONTACT_ID = @site_lic_adm_ID
--		Order by DATE_CREATED desc
--	end
--	else
--	begin
--		-- we are using the site_lic parameter
--		SELECT s_l_c.*, f_s.* from TB_SITE_LIC_DETAILS s_l_c
--		inner join TB_FOR_SALE f_s on f_s.FOR_SALE_ID = s_l_c.FOR_SALE_ID
--		where SITE_LIC_ID = @site_lic_ID
--		Order by DATE_CREATED desc
--	end
	
--END



--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Package]    Script Date: 01/26/2012 11:50:04 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Get_Package] 
--(	
--	@SITE_LIC_DETAILS_ID int
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--	select * from TB_SITE_LIC_DETAILS ld 
--	inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID
--	inner join Reviewer.dbo.TB_SITE_LIC sl on sl.SITE_LIC_ID = ld.SITE_LIC_ID
--	where SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
	
--END



--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Package_Log]    Script Date: 01/26/2012 11:50:20 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO




---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Get_Package_Log] 
--(	
--	@SITE_LIC_DETAILS_ID int
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    select * from TB_SITE_LIC_LOG
--    where SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
--    order by [DATE]
	
--END




--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Remove_Review]    Script Date: 01/26/2012 11:50:33 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_Site_Lic_Remove_Review]
--	-- Add the parameters for the stored procedure here
--	@lic_id int
--	, @admin_ID int
--	, @review_id int
--	, @res int output
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
--	set @res = 0

--    -- Insert statements for procedure here
--    declare @ck int, @ck2 int, @lic_det_id int
	
--	set @ck = (SELECT count(contact_id) from TB_SITE_LIC_ADMIN 
--	where (CONTACT_ID = @admin_ID or @admin_ID in 
--	(select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)
--		   ) and SITE_LIC_ID = @lic_id)
--	if @ck < 1 set @res = -1 --first check: see if supplied admin is actually an admin!
	
--	--second check, see if review_id exists
--	else if  
--	(select count(REVIEW_ID) from Reviewer.dbo.TB_REVIEW 
--	 where @review_id = REVIEW_ID) != 1
--		set @res = -2
	
--	else -- initial checks went OK, let's see if we can do it
--	begin
--		set @ck = 
--		(select count(review_id) from Reviewer.dbo.TB_SITE_LIC_REVIEW 
--		where REVIEW_ID = @review_id and SITE_LIC_ID = @lic_id)
		
--		set @lic_det_id = 
--		(SELECT TOP 1 ld.SITE_LIC_DETAILS_ID from Reviewer.dbo.TB_SITE_LIC sl 
--			inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
--			inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
--			Order by ld.VALID_FROM desc)
		
--		if @ck = 0 -- review is NOT in the site lic
--		begin
--			set @res = -3 --review not in this site_lic
--		end
--		else
--		begin --all is well, let's do something!
--			begin transaction --make sure we don't commit only half of the mission critical data! (we assume the update below will work, only checking for the other statement)
			
--			delete from Reviewer.dbo.TB_SITE_LIC_REVIEW
--			where REVIEW_ID = @review_id
			
--			if @@ROWCOUNT = 1
--			begin --write success log
--				commit transaction --all is well, commit
--				insert into TB_SITE_LIC_LOG (
--					[SITE_LIC_DETAILS_ID]
--					  ,[CONTACT_ID]
--					  ,[AFFECTED_ID]
--					  ,[CHANGE_TYPE]
--				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @review_id, 'remove review'
--					from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--					order by DATE_CREATED desc
--			end
--			else --write failure log, if this is fired, there is a bug somewhere
--			begin
--				rollback transaction --BAD! something went wrong!
--				insert into TB_SITE_LIC_LOG (
--					[SITE_LIC_DETAILS_ID]
--					  ,[CONTACT_ID]
--					  ,[AFFECTED_ID]
--					  ,[CHANGE_TYPE]
--				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @review_id, 'remove review: failed!'
--					from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
--					order by DATE_CREATED desc	
--				set @res = -4
--			end
--		end
--	end
--	--select r.review_id, review_name from reviewer.dbo.TB_SITE_LIC_REVIEW lr
--	--	inner join Reviewer.dbo.TB_REVIEW r on lr.REVIEW_ID = r.REVIEW_ID
--	-- where SITE_LIC_ID = @lic_id
--	return @res
--	-- error codes: -1 = supplied admin_id is not an admin of this site lice
--	--				-2 = review_id does not exist
--	--				-3 = review not in this site_lic
--	--				-4 = all seemed well but couldn't write changes! BUG ALERT
--END



--GO










----use [revieweradmin]
----go
----/****** object:  storedprocedure [dbo].[st_contactlogin]    script date: 12/08/2011 15:22:25 ******/
----set ansi_nulls on
----go
----set quoted_identifier on
----go
------ =============================================
------ author:		<jeff>
------ alter date: <24/03/10>
------ description:	<gets contact details when loging in>
------ =============================================
----alter procedure [dbo].[st_contactlogin] 
----(
----	@username nvarchar(50),
----	@password nvarchar(50),
----	@ip_address nvarchar(50)
----)
----as
----begin
----	-- set nocount on added to prevent extra result sets from
----	-- interfering with select statements.
----	set nocount on;

----    -- insert statements for procedure here
----	select c.*, count(sla.contact_id) as issla
----	from reviewer.dbo.tb_contact c
----	left outer join tb_site_lic_admin sla on sla.contact_id = c.contact_id
----	where c.username = @username
----	and c.[password] = @password
----	group by c.contact_id, c.old_contact_id, c.contact_name, c.username,
----	c.password, c.last_login, c.date_created, c.email, c.expiry_date,
----	c.months_credit, c.creator_id,c.type, c.is_site_admin, c.description
	

----	/*
----	select c.*
----	from reviewer.dbo.tb_contact c
----	where c.username = @username
----	and c.[password] = @password
----	*/
	
----	return
----end

----go

----------------------------------------------------------

----use [revieweradmin]
----go

----/****** object:  storedprocedure [dbo].[st_extensionrecordget]    script date: 12/12/2011 10:19:05 ******/
----set ansi_nulls on
----go

----set quoted_identifier on
----go


------ =============================================
------ author:		<author,,name>
------ alter date: <alter date,,>
------ description:	<description,,>
------ =============================================
----create procedure [dbo].[st_extensionrecordget] 
----(
----	@contact_id nvarchar(50)
----)
----as
----begin
----	-- set nocount on added to prevent extra result sets from
----	-- interfering with select statements.
----	set nocount on;

----    -- insert statements for procedure here
----	select e_e_l.expiry_edit_id ,e_e_l.date_of_edit, 
----	--convert(date, e_e_l.old_expiry_date), convert (date, e_e_l.new_expiry_date), 
----	e_e_l.old_expiry_date, e_e_l.new_expiry_date,
----	e_e_l.extended_by_id, c.contact_name, e_t.extension_type, 
----	e_e_l.extension_notes
----	from tb_expiry_edit_log e_e_l
----	inner join tb_extension_types e_t on e_t.extension_type_id = e_e_l.extension_type_id
----	inner join reviewer.dbo.tb_contact c on c.contact_id = e_e_l.extended_by_id
----	where e_e_l.id_extended = @contact_id

----	return

----end


----go

--------------------------------------------------------------

----use [revieweradmin]
----go

----/****** object:  storedprocedure [dbo].[st_site_lic_get_details]    script date: 12/12/2011 13:45:11 ******/
----set ansi_nulls on
----go

----set quoted_identifier on
----go


------ =============================================
------ author:		<author,,name>
------ create date: <create date,,>
------ description:	<description,,>
------ =============================================
----alter procedure [dbo].[st_site_lic_get_details] 
----	-- add the parameters for the stored procedure here
	
----	@admin_id int
----as
----begin
----	-- set nocount on added to prevent extra result sets from
----	-- interfering with select statements.
----	set nocount on;

----    -- first results set: currently active license tb_site_lic_details.valid_from is something and for_sale.is_active = false
----	select top 1 *, c.contact_name as admin_name from reviewer.dbo.tb_site_lic sl 
----		inner join tb_site_lic_details ld on sl.site_lic_id = ld.site_lic_id and ld.valid_from is not null
----		inner join tb_for_sale fs on ld.for_sale_id = fs.for_sale_id and fs.is_active = 0
----		inner join tb_site_lic_admin la on sl.site_lic_id = la.site_lic_id and contact_id = @admin_id
----		inner join reviewer.dbo.tb_contact c on c.contact_id = @admin_id
----	order by ld.valid_from desc	
----	-- second results set: currently active renewal offer tb_site_lic_details.valid_from is null and for_sale.is_active = true
----	select top 1 * from reviewer.dbo.tb_site_lic sl 
----		inner join tb_site_lic_details ld on sl.site_lic_id = ld.site_lic_id and ld.valid_from is null
----		inner join tb_for_sale fs on ld.for_sale_id = fs.for_sale_id and fs.is_active = 1
----		inner join tb_site_lic_admin la on sl.site_lic_id = la.site_lic_id and contact_id = @admin_id
----	order by ld.valid_from desc
----end


----go

-----------------------------------------------------------------

----use [revieweradmin]
----go

----/****** object:  storedprocedure [dbo].[st_contactreviews]    script date: 12/13/2011 09:51:26 ******/
----set ansi_nulls on
----go

----set quoted_identifier on
----go

------ =============================================
------ author:		<jeff>
------ create date: <24/03/2010>
------ description:	<gets the review data based on contact>
------ =============================================
----alter procedure [dbo].[st_contactreviews] 
----(
----	@contact_id nvarchar(50)
----)
----as
----begin
----	-- set nocount on added to prevent extra result sets from
----	-- interfering with select statements.
----	set nocount on;
	
----    -- insert statements for procedure here
----    select r.review_id, r.review_name, r.date_created,
----	sum(datediff(hour,t.created ,t.last_renewed )) as hours 
----	from reviewer.dbo.tb_review r
----	inner join reviewer.dbo.tb_review_contact r_c on r_c.review_id = r.review_id
----	inner join reviewer.dbo.tb_contact c on c.contact_id = r_c.contact_id
----	left outer join revieweradmin.dbo.[tb_logon_ticket] t on t.review_id = r_c.review_id and t.contact_id = r_c.contact_id
----	where r_c.contact_id = @contact_id
----	group by r.review_id, r.review_name, r.date_created


----end


----go

----------------------------------------------------------




------use [revieweradmin]
------go

------/****** object:  table [dbo].[tb_extension_types]    script date: 12/06/2011 15:18:27 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go

--------set ansi_padding on
------go

------create table [dbo].[tb_extension_types](
------	[extension_type_id] [int] identity(1,1) not null,
------	[extension_type] [nvarchar](50) not null,
------	[description] [nvarchar](500) not null,
------	[applies_to] [char](2) null,
------	[order] [int] not null,
------ constraint [pk_tb_extension_types_new] primary key clustered 
------(
------	[extension_type_id] asc
------)with (pad_index  = off, statistics_norecompute  = off, ignore_dup_key = off, allow_row_locks  = on, allow_page_locks  = on) on [primary]
------) on [primary]

------go

------set ansi_padding off
------go

---------------------------------------------------------------------

------use [revieweradmin]
------go

------/****** object:  table [dbo].[tb_expiry_edit_log]    script date: 12/06/2011 15:19:01 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go

------set ansi_padding on
------go

------create table [dbo].[tb_expiry_edit_log](
------	[expiry_edit_id] [int] identity(1,1) not null,
------	[date_of_edit] [datetime] not null,
------	[type_extended] [char](1) not null,
------	[id_extended] [int] not null,
------	[old_expiry_date] [date] null,
------	[new_expiry_date] [date] null,
------	[extended_by_id] [int] not null,
------	[extension_type_id] [int] not null,
------	[extension_notes] [nvarchar](max) null,
------ constraint [pk_tb_expiry_edit_log] primary key clustered 
------(
------	[expiry_edit_id] asc
------)with (pad_index  = off, statistics_norecompute  = off, ignore_dup_key = off, allow_row_locks  = on, allow_page_locks  = on) on [primary]
------) on [primary]

------go

------set ansi_padding off
------go

------alter table [dbo].[tb_expiry_edit_log]  with check add  constraint [fk_tb_expiry_edit_log_tb_extension_types] foreign key([extension_type_id])
------references [dbo].[tb_extension_types] ([extension_type_id])
------go

------alter table [dbo].[tb_expiry_edit_log] check constraint [fk_tb_expiry_edit_log_tb_extension_types]
------go

--------------------------------------------------------


------insert into tb_extension_types (extension_type, [description], applies_to, [order])
------select 'no extension', 'this no change to the extension date', '11', '0' union all
------select 'purchase', 'extension due to purchase', '11', '1' union all
------select 'staff', 'extension for eppi staff', '11', '2' union all
------select 'maintenance', 'extension for network down time', '11', '3' union all
------select 'making review non-shareable', 'change review to non-shareable', '01', '4'

---------------------------------------------------------------


------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_reviewdetailsgetall]    script date: 10/25/2011 11:17:17 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go


-------- =============================================
-------- author:		<author,,name>
-------- alter date: <alter date,,>
-------- description:	<description,,>
-------- =============================================
------create procedure [dbo].[st_reviewdetailsgetall] 
------(
------	@shareable bit
------)
------as
------begin
------	-- set nocount on added to prevent extra result sets from
------	-- interfering with select statements.
------	set nocount on;

------    -- insert statements for procedure here
------    if @shareable = 1
------	begin        
------		select r.review_id, r.review_name, r.date_created, r.expiry_date, 
------		r.funder_id, c.contact_name, r.months_credit
------		from reviewer.dbo.tb_review r
------		inner join reviewer.dbo.tb_contact c on c.contact_id = r.funder_id
------		where 
------		(r.expiry_date is not null) or
------		(r.expiry_date is null and r.months_credit != 0) 
------	end
------	else
------	begin
------		select r.review_id, r.review_name, r.date_created, r.expiry_date, 
------		r.funder_id, c.contact_name
------		from reviewer.dbo.tb_review r
------		inner join reviewer.dbo.tb_contact c on c.contact_id = r.funder_id 
------		where (r.expiry_date is null and r.months_credit = 0)
------	end

------	return

------end


------go

---------------------------------------------------------------------------



------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_contactdetailsgetall]    script date: 10/25/2011 11:16:26 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go


-------- =============================================
-------- author:		<author,,name>
-------- alter date: <alter date,,>
-------- description:	<description,,>
-------- =============================================
------create procedure [dbo].[st_contactdetailsgetall] 
------(
------	@er4accountsonly bit
------)
------as
------begin
------	-- set nocount on added to prevent extra result sets from
------	-- interfering with select statements.
------	set nocount on;
 
------	if @er4accountsonly = 1
------	begin        
------		select * from reviewer.dbo.tb_contact 
------		where expiry_date > '2010-03-20 00:00:01' or
------		(expiry_date is null and months_credit != 0) 
------	end
------	else
------	begin
------		select * from reviewer.dbo.tb_contact
------	end
       
------end


------go

-------------------------------------------------------------------------------

------use [revieweradmin]
------go
------/****** object:  storedprocedure [dbo].[st_contactreviews]    script date: 10/25/2011 11:53:43 ******/
------set ansi_nulls on
------go
------set quoted_identifier on
------go
-------- =============================================
-------- author:		<jeff>
-------- create date: <24/03/2010>
-------- description:	<gets the review data based on contact>
-------- =============================================
------alter procedure [dbo].[st_contactreviews] 
------(
------	@contact_id nvarchar(50)
------)
------as
------begin
------	-- set nocount on added to prevent extra result sets from
------	-- interfering with select statements.
------	set nocount on;
	
------    -- insert statements for procedure here
------    select r.review_id, r.review_name, r.date_created,
------	sum(datediff(hour,t.created ,t.last_renewed )) as hours 
------	from reviewer.dbo.tb_review r
------	inner join reviewer.dbo.tb_review_contact r_c on r_c.review_id = r.review_id
------	inner join reviewer.dbo.tb_contact c on c.contact_id = r_c.contact_id
------	inner join revieweradmin.dbo.[tb_logon_ticket] t on t.review_id = r_c.review_id and t.contact_id = r_c.contact_id
------	where r_c.contact_id = @contact_id
------	group by r.review_id, r.review_name, r.date_created


------end

------go

-------------------------------------------------------------------------

------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_extensiontypesget]    script date: 11/10/2011 12:18:20 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go


-------- =============================================
-------- author:		<author,,name>
-------- alter date: <alter date,,>
-------- description:	<description,,>
-------- =============================================
------create procedure [dbo].[st_extensiontypesget] 
------(
------	@applies_to nvarchar(50)
------)
------as
------begin
------	-- set nocount on added to prevent extra result sets from
------	-- interfering with select statements.
------	set nocount on;

------    -- insert statements for procedure here
------    if @applies_to = 'contact'
------    begin
------		select * from tb_extension_types 
------		where (applies_to = '10' or applies_to = '11')
------		order by [order]
------    end
    
------    if @applies_to = 'review'
------    begin
------		select * from tb_extension_types 
------		where (applies_to = '01' or applies_to = '11')
------		order by [order]
------    end

------	return

------end


------go

-------------------------------------------------



------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_contactfullcreateoredit]    script date: 11/07/2011 14:28:57 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go


------create procedure [dbo].[st_contactfullcreateoredit]
------(
------	@new_account bit,
------	@contact_name nvarchar(255),
------	@username nvarchar(50),
------	@password nvarchar(50),
------	@date_created datetime,
------	@expiry_date date,
------	@creator_id int,
------	@email nvarchar(500),
------	@description nvarchar(1000),
------	@is_site_admin bit,
------	@contact_id nvarchar(50), -- it might say 'new'
------	@extension_type_id int,
------	@extension_notes nvarchar(500),
------	@editor_id int,
------	@months_credit nvarchar(50),
------	@result nvarchar(50) output
------)

------as

------set nocount on
------declare @original_expiry datetime
------declare @new_row_id int

------if @expiry_date = ''
------begin
------	set @expiry_date = null
------end

------begin try

------begin transaction
------	if @new_account = 1
------	begin
------		-- create a new account
------		insert into reviewer.dbo.tb_contact(contact_name, username, [password], 
------			date_created, [expiry_date], email, [description], creator_id, months_credit)
------		values (@contact_name, @username, @password, @date_created, @expiry_date,
------			@email, @description, @creator_id, @months_credit)
	
------		set @result = @@identity
------		if (@creator_id != @contact_id)
------		begin
------			update reviewer.dbo.tb_contact 
------			set creator_id = @result
------			where contact_id = @result
------			and username = @username
------		end
------    end
------	else  -- edit an existing account
------	begin	
------		-- get original expiry date from tb_contact
------		select @original_expiry = c.expiry_date 
------		from reviewer.dbo.tb_contact c
------		where c.contact_id = @contact_id
		
------		--update tb_contact
------		update reviewer.dbo.tb_contact 
------		set 
------		contact_name = @contact_name,
------		username = @username,
------		[password] = @password,
------		date_created = @date_created,
------		[expiry_date] = @expiry_date,
------		creator_id = @creator_id,
------		email = @email,
------		[description] = @description,
------		is_site_admin = @is_site_admin,
------		months_credit = @months_credit
------		where contact_id = @contact_id
		
------		if (@extension_type_id > 1)
------		begin
------			-- create new row in tb_expiry_edit_log -- using bogus old expiry date
------			insert into tb_expiry_edit_log
------			(date_of_edit, type_extended, id_extended, new_expiry_date,
------			old_expiry_date, extended_by_id, extension_type_id, extension_notes)
------			values (getdate(), 1, @contact_id, @expiry_date, @expiry_date,
------			@editor_id, @extension_type_id, @extension_notes)
------			set @new_row_id = @@identity
			
------			--  update tb_expiry_edit_log
------			update tb_expiry_edit_log
------			set old_expiry_date = @original_expiry
------			where expiry_edit_id = @new_row_id
	
------			set @result = 'valid'		 
------		end
------	end
	
	
	
------commit transaction
------end try

------begin catch
------if (@@trancount > 0)
------begin	
------rollback transaction
------set @result = 'invalid'
------end
------end catch

------return

------set nocount off


------go



----------------------------------------------------------------------

------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_reviewdetails]    script date: 11/07/2011 14:27:33 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go

-------- =============================================
-------- author:		<author,,name>
-------- alter date: <alter date,,>
-------- description:	<description,,>
-------- =============================================
------create procedure [dbo].[st_reviewdetails] 
------(
------	@review_id nvarchar(50)
------)
------as
------begin
------	-- set nocount on added to prevent extra result sets from
------	-- interfering with select statements.
------	set nocount on;

------    -- insert statements for procedure here
------    select r.* , c.contact_name from reviewer.dbo.tb_review r
------    inner join reviewer.dbo.tb_contact c on c.contact_id = r.funder_id
------	where review_id = @review_id

------	return

------end

------go

--------------------------------------------------------------


------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_reviewcontacts]    script date: 11/07/2011 16:06:04 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go


-------- =============================================
-------- author:		<jeff>
-------- create date: <24/03/2010>
-------- description:	<gets the review data based on contact>
-------- =============================================
------create procedure [dbo].[st_reviewcontacts] 
------(
------	@review_id nvarchar(50)
------)
------as
------begin
------	-- set nocount on added to prevent extra result sets from
------	-- interfering with select statements.
------	set nocount on;
	
------    -- insert statements for procedure here 
------    create table #review_contacts (
------	contact_id int,
------	contact_name char(50),
------	email nvarchar(500),
------	last_login datetime,
------	expiry_date date,
------	hours int)
    
------    insert into #review_contacts (contact_id, contact_name, email, expiry_date)
------	select c.contact_id, c.contact_name, c.email, c.expiry_date 
------	from reviewer.dbo.tb_contact c
------	inner join reviewer.dbo.tb_review_contact r_c on c.contact_id = r_c.contact_id
------	and r_c.review_id = @review_id
------	group by c.contact_id, c.contact_name, c.email, c.expiry_date
------	order by c.contact_name
	
------	update #review_contacts
------	set hours = 
------	(select sum(datediff(hour,l_t.created ,l_t.last_renewed )) as hours
------	from tb_logon_ticket l_t where l_t.review_id = @review_id 
------	and contact_id = #review_contacts.contact_id)

------	update #review_contacts
------	set last_login = 
------	(select max(l_t.created) as last_login
------	from tb_logon_ticket l_t where l_t.review_id = @review_id 
------	and contact_id = #review_contacts.contact_id)
	
------    select * from #review_contacts

------	drop table #review_contacts
	
------    /*
------    select c.contact_id, c.contact_name, c.email,
------    c.last_login, c.expiry_date, sum(datediff(hour,l_t.created ,l_t.last_renewed )) as hours
------    from reviewer.dbo.tb_contact c
------    inner join reviewer.dbo.tb_review_contact r_c on c.contact_id = r_c.contact_id 
------    inner join tb_logon_ticket l_t on l_t.contact_id = r_c.contact_id and l_t.review_id = @review_id
------    and r_c.review_id = @review_id
------    group by c.contact_id, c.contact_name, c.email, c.last_login, c.expiry_date
------    order by c.contact_name*/

------end


------go

--------------------------------------------------------

------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_reviewrolesall]    script date: 11/07/2011 15:59:51 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go

-------- =============================================
-------- author:		<author,,name>
-------- alter date: <alter date,,>
-------- description:	<description,,>
-------- =============================================
------create procedure [dbo].[st_reviewrolesall] 

------as
------begin
------	-- set nocount on added to prevent extra result sets from
------	-- interfering with select statements.
------	set nocount on;

------    -- insert statements for procedure here
------    select * from reviewer.dbo.tb_review_role

------	return

------end

------go


-------------------------------------------------

------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_contactreviewrole]    script date: 11/07/2011 16:24:28 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go

-------- =============================================
-------- author:		<author,,name>
-------- alter date: <alter date,,>
-------- description:	<description,,>
-------- =============================================
------create procedure [dbo].[st_contactreviewrole] 
------(
------	@contact_id int,
------	@review_id int
------)
------as
------begin
------	-- set nocount on added to prevent extra result sets from
------	-- interfering with select statements.
------	set nocount on;

------    -- insert statements for procedure here
------    select c_r_r.role_name  
------	from reviewer.dbo.tb_contact_review_role c_r_r
------	inner join reviewer.dbo.tb_review_contact r_c on r_c.review_contact_id = c_r_r.review_contact_id
------	where r_c.contact_id = @contact_id
------	and r_c.review_id = @review_id

------	return

------end

------go


---------------------------------------------------------

------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_reviewfullcreateoredit]    script date: 11/10/2011 15:19:00 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go



------create procedure [dbo].[st_reviewfullcreateoredit]
------(
------	@new_review bit,
------	@review_name nvarchar(255),
------	@date_created datetime,
------	@expiry_date nvarchar(50),
------	@funder_id int,
------	@review_number nvarchar(50),
------	@review_id nvarchar(50), -- it might say 'new'
------	@extension_type_id int,
------	@extension_notes nvarchar(500),
------	@editor_id int,
------	@months_credit nvarchar(50),
------	@result nvarchar(50) output
------)

------as

------set nocount on
------declare @original_expiry datetime
------declare @new_row_id int

------if @expiry_date = ''
------begin
------	set @expiry_date = null
------end

------begin try

------begin transaction
------	if @new_review = 1
------	begin
------		-- create a new account
------		insert into reviewer.dbo.tb_review(review_name, date_created, [expiry_date], months_credit, review_number, funder_id)
------		values (@review_name, @date_created, @expiry_date, @months_credit, @review_number, @funder_id)
	
------		set @result = @@identity
------    end
------	else  -- edit an existing account
------	begin	
------		-- get original expiry date from tb_review
------		select @original_expiry = r.expiry_date 
------		from reviewer.dbo.tb_review r
------		where r.review_id = @review_id
		
------		--update tb_contact
------		update reviewer.dbo.tb_review 
------		set 
------		review_name = @review_name,
------		date_created = @date_created,
------		[expiry_date] = @expiry_date,
------		funder_id = @funder_id,
------		review_number = @review_number,
------		months_credit = @months_credit
------		where review_id = @review_id
		
------		if (@extension_type_id > 1)
------		begin
------			-- create new row in tb_expiry_edit_log -- using bogus old expiry date
------			insert into tb_expiry_edit_log
------			(date_of_edit, type_extended, id_extended, new_expiry_date,
------			old_expiry_date, extended_by_id, extension_type_id, extension_notes)
------			values (getdate(), 0, @review_id, @expiry_date, @expiry_date,
------			@editor_id, @extension_type_id, @extension_notes)
------			set @new_row_id = @@identity
			
------			--  update tb_expiry_edit_log
------			update tb_expiry_edit_log
------			set old_expiry_date = @original_expiry
------			where expiry_edit_id = @new_row_id
	
------			set @result = 'valid'		 
------		end
------	end
	
	
	
------commit transaction
------end try

------begin catch
------if (@@trancount > 0)
------begin	
------rollback transaction
------set @result = 'invalid'
------end
------end catch

------return

------set nocount off



------go

----------------------------------------------------


------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_reviewaddcontact]    script date: 11/10/2011 16:52:27 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go


------create procedure [dbo].[st_reviewaddcontact]
------(
------	@review_id int,
------	@contact_id int,
------	@old_review_id nvarchar(50)
------)

------as
------begin
------	-- set nocount on added to prevent extra result sets from
------	-- interfering with select statements.
------	set nocount on;
------    -- insert statements for procedure here 

------	declare @review_contact_id int
------	declare @old_contact_id nvarchar(50)
	
------	select contact_id from reviewer.dbo.tb_review_contact
------	where contact_id = @contact_id and review_id = @review_id
	
------	if @@rowcount = 0  --i.e. it's not already there
------	begin
------		select @old_contact_id = old_contact_id from reviewer.dbo.tb_contact
------		where contact_id = @contact_id

------		insert into reviewer.dbo.tb_review_contact (review_id, contact_id, old_review_id, old_contact_id)
------		values (@review_id, @contact_id, @old_review_id, @old_contact_id)
------		set @review_contact_id = @@identity

------		insert into reviewer.dbo.tb_contact_review_role (review_contact_id, role_name)
------		values (@review_contact_id, 'regularuser')
------	end


------end



------go

-----------------------------------------

------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_reviewroleupdate]    script date: 11/16/2011 15:57:22 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go

------create procedure [dbo].[st_reviewroleupdate]
------(
------	@review_contact_id int,
------	@role nvarchar(50)
------)

------as

------set nocount on

------	insert into reviewer.dbo.tb_contact_review_role (review_contact_id, role_name)
------	values (@review_contact_id, @role)

------set nocount off

------go


----------------------------------------------

------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_contactreviewdeleteroles]    script date: 11/16/2011 14:13:41 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go


-------- =============================================
-------- author:		<author,,name>
-------- alter date: <alter date,,>
-------- description:	<description,,>
-------- =============================================
------create procedure [dbo].[st_contactreviewdeleteroles] 
------(
------	@review_id int,
------	@contact_id int,
------	@review_contact_id int output
------)
------as
------begin
------	-- set nocount on added to prevent extra result sets from
------	-- interfering with select statements.
------	set nocount on;

------    -- insert statements for procedure here
------    select @review_contact_id = review_contact_id 
------    from reviewer.dbo.tb_review_contact
------    where review_id = @review_id
------    and contact_id = @contact_id
    
------    if @@rowcount > 0
------    begin
------		delete from reviewer.dbo.tb_contact_review_role
------		where review_contact_id = @review_contact_id
------    end
   

------	return

------end


------go


----------------------------------------------

------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_reviewnewnonshareableaddcontact]    script date: 11/17/2011 12:46:50 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go



------create procedure [dbo].[st_reviewnewnonshareableaddcontact]
------(
------	@review_id int,
------	@funder_id int
------)

------as
------begin
------	-- set nocount on added to prevent extra result sets from
------	-- interfering with select statements.
------	set nocount on;
------    -- insert statements for procedure here 
------	declare @review_contact_id int
	
------	select contact_id from reviewer.dbo.tb_review_contact
------	where contact_id = @funder_id and review_id = @review_id
	
------	if @@rowcount = 0  --i.e. it's not already there
------	begin
------		insert into reviewer.dbo.tb_review_contact (review_id, contact_id, old_review_id, old_contact_id)
------		values (@review_id, @funder_id, null, null)
------		set @review_contact_id = @@identity

------		insert into reviewer.dbo.tb_contact_review_role (review_contact_id, role_name)
------		values (@review_contact_id, 'adminuser')
------	end


------end




------go

-------------------------------------------------------

------use [revieweradmin]
------go

------/****** object:  storedprocedure [dbo].[st_expirydatebulkadjust]    script date: 12/06/2011 14:20:26 ******/
------set ansi_nulls on
------go

------set quoted_identifier on
------go


------create procedure [dbo].[st_expirydatebulkadjust]
------(
------	@add_or_remove nvarchar(10),
------	@number_days int,
------	@date date,
------	@contact_id int,
------	@extension_notes nvarchar(500),
------	@result_c nvarchar(50) output,
------	@result_r nvarchar(50) output,
------	@result_sl nvarchar (50) output
------)

------as

------set nocount on
------declare @cost int
------declare @account_cost int

------create table #expiry_table (
------date_of_edit datetime,
------type_extended char(1),
------id_extended int,
------new_expiry_date date,
------old_expiry_date date,
------extended_by_id int,
------extension_type_id int,
------extension_notes nvarchar(500))

------create table #expiry_table_site_lic (
------site_lic_id int,
------site_lic_details_id int,
------extended_by_id int,
------id_extended int,
------change_type nvarchar(50),
------date_of_edit datetime,
------reason nvarchar(2000))


------if @add_or_remove = 'remove'
------begin
------	set @number_days = @number_days * -1
------end


------begin try
------begin transaction

------	-- contacts ---------------------------------------------------------
------	insert into #expiry_table (date_of_edit, type_extended, id_extended, new_expiry_date, old_expiry_date,
------		extended_by_id, extension_type_id, extension_notes)
------	select getdate(), 0, c.contact_id, dateadd(day, @number_days, expiry_date), expiry_date,
------		@contact_id, 8, @extension_notes 
------	from reviewer.dbo.tb_contact c
------	where c.expiry_date >= @date
	
------	update reviewer.dbo.tb_contact set expiry_date = dateadd(day, @number_days, expiry_date) 
------	where expiry_date >= @date
	
------	insert into tb_expiry_edit_log (date_of_edit, type_extended, id_extended, new_expiry_date,
------		old_expiry_date, extended_by_id, extension_type_id, extension_notes)
------	select * from #expiry_table
	
------	select * from #expiry_table
------	set @result_c = @@rowcount
	
------	delete from #expiry_table
	
	
------	-- reviews ----------------------------------------------------------
------	insert into #expiry_table (date_of_edit, type_extended, id_extended, new_expiry_date, old_expiry_date,
------		extended_by_id, extension_type_id, extension_notes)
------	select getdate(), 1, r.review_id, dateadd(day, @number_days, expiry_date), expiry_date,
------		@contact_id, 8, @extension_notes 
------	from reviewer.dbo.tb_review r
------	where r.expiry_date >= @date
	
------	update reviewer.dbo.tb_review set expiry_date = dateadd(day, @number_days, expiry_date) 
------	where expiry_date >= @date
	
------	insert into tb_expiry_edit_log (date_of_edit, type_extended, id_extended, new_expiry_date,
------		old_expiry_date, extended_by_id, extension_type_id, extension_notes)
------	select * from #expiry_table		
	
------	select * from #expiry_table
------	set @result_r = @@rowcount
	
------	drop table #expiry_table
	
------	-- site license -------------------------------------------------------
------	insert into #expiry_table_site_lic (site_lic_id, site_lic_details_id, extended_by_id, id_extended, change_type,
------		date_of_edit, reason)
------	select s_l.site_lic_id, null, @contact_id, s_l.site_lic_id, 'maintenance', getdate(), @extension_notes
------	from reviewer.dbo.tb_site_lic s_l
------	where s_l.expiry_date >= @date
	
------	update #expiry_table_site_lic
------	set site_lic_details_id = 
------	(select top 1 ld.site_lic_details_id from tb_site_lic_details ld 
------        inner join tb_for_sale fs on ld.for_sale_id = fs.for_sale_id and ld.valid_from is not null
------        and fs.is_active = 0 and ld.site_lic_id = #expiry_table_site_lic.site_lic_id
------        order by ld.valid_from desc)
------    where #expiry_table_site_lic.site_lic_details_id is null
	
------	update reviewer.dbo.tb_site_lic set expiry_date = dateadd(day, @number_days, expiry_date) 
------	where expiry_date >= @date
	
------	insert into tb_site_lic_log (site_lic_details_id, contact_id, affected_id, change_type,
------		date, reason)
------	select site_lic_details_id, extended_by_id, id_extended, change_type,
------		date_of_edit, reason
------	from #expiry_table_site_lic
	
------	select * from #expiry_table_site_lic
------	set @result_sl = @@rowcount

------	drop table #expiry_table_site_lic

------commit transaction
------end try

------begin catch
------if (@@trancount > 0)
------begin	
------rollback transaction
------set @result_c = 'invalid'
------end
------end catch

------return

------set nocount off


------go

----------------------------------------------------------


------use [revieweradmin]
------go
------/****** object:  storedprocedure [dbo].[st_billmarkaspaid]    script date: 12/06/2011 15:11:30 ******/
------set ansi_nulls on
------go
------set quoted_identifier on
------go


-------- =============================================
-------- author:		<author,,name>
-------- create date: <create date,,>
-------- description:	<description,,>
-------- =============================================
------alter procedure [dbo].[st_billmarkaspaid]
------	-- add the parameters for the stored procedure here
------	@bill_id int
------as
------begin
------	-- set nocount on added to prevent extra result sets from
------	-- interfering with select statements.
------	set nocount on;

------    --to be very sure this doesn't happen in part, we nest a transaction inside a try catch clause
------	begin try   
------	begin transaction

------------------------------------------------------------------------------------------------------------------	 
------	-- existing accounts	
		
------	 --1a.collect data for tb_expiry_edit_log
------	 create table #existingaccounts(date_of_edit datetime, type_extended int, id_extended int, 
------	 new_expiry_date date, old_expiry_date date, length_of_extension int, extended_by_id int, 
------	 extension_type_id int)

------	 insert into #existingaccounts (date_of_edit, id_extended, type_extended, extension_type_id, length_of_extension)
------	 select getdate(),  affected_id as id_extended, '1', '2', bl.months from tb_bill_line bl
------	 inner join tb_for_sale fs on bl.for_sale_id = fs.for_sale_id
------	 and bill_id = @bill_id and fs.type_name = 'professional' and affected_id is not null
------	 -- type_extended = 1 for contacts
	 
------	 update #existingaccounts set old_expiry_date =
------	 (select c.expiry_date from [reviewer].[dbo].[tb_contact] c 
------	 where c.contact_id = #existingaccounts.id_extended)

------	 update #existingaccounts set extended_by_id =
------	 (select purchaser_contact_id from tb_bill b
------	 inner join [reviewer].[dbo].[tb_contact] c on c.contact_id = b.purchaser_contact_id
------	 where b.bill_id = @bill_id)
	
------	 --1b.extend existing accounts
------	 update reviewer.dbo.tb_contact set 
------		[expiry_date] = case 
------			when ([expiry_date] is null) then null
------			when ([expiry_date] > getdate()) then dateadd(month, a.months, [expiry_date])
------			else dateadd(month, a.months, getdate())
------		end
------		, months_credit = case when (expiry_date is null and  months_credit is null)
------			then months
------			when (expiry_date is null and months_credit is not null) then months_credit + a.months
------		else 0
------		end
------		from (
------				select affected_id, bl.months from tb_bill_line bl
------				inner join tb_for_sale fs on bl.for_sale_id = fs.for_sale_id
------				and bill_id = @bill_id and fs.type_name = 'professional' and affected_id is not null
------			) a
------	 where contact_id = a.affected_id
	 
------	 --1c. update tb_expiry_edit_log
------	 update #existingaccounts set new_expiry_date =
------	 (select c.expiry_date from [reviewer].[dbo].[tb_contact] c 
------	 where c.contact_id = #existingaccounts.id_extended)
	 
	 
------	 insert into tb_expiry_edit_log (date_of_edit, type_extended, id_extended, new_expiry_date,
------		old_expiry_date, extended_by_id, extension_type_id)
------	 select date_of_edit, type_extended, id_extended, new_expiry_date,
------		old_expiry_date, extended_by_id, extension_type_id
------	 from #existingaccounts
	 
------	 drop table #existingaccounts
	 
------------------------------------------------------------------------------------------------------------------	 
	
------	-- existing reviews
	 
------	 --2a.collect data for tb_expiry_edit_log
------	 create table #existingreviews(date_of_edit datetime, type_extended int, id_extended int, 
------	 new_expiry_date date, old_expiry_date date, length_of_extension int, extended_by_id int, 
------	 extension_type_id int)

------	 insert into #existingreviews (date_of_edit, id_extended, type_extended, extension_type_id, length_of_extension)
------	 select getdate(),  affected_id as id_extended, '0', '2', bl.months from tb_bill_line bl
------	 inner join tb_for_sale fs on bl.for_sale_id = fs.for_sale_id
------	 and bill_id = @bill_id and fs.type_name = 'shareable review' and affected_id is not null
------	 -- type_extended = 0 for reviews

------	 update #existingreviews set old_expiry_date =
------	 (select r.expiry_date from [reviewer].[dbo].[tb_review] r 
------	 where r.review_id = #existingreviews.id_extended)

------	 update #existingreviews set extended_by_id = purchaser_contact_id from tb_bill b
------	 where b.bill_id = @bill_id
	 
------	 --2b.extend existing reviews
------	 update reviewer.dbo.tb_review set 
------		[expiry_date] = case 
------			when ([expiry_date] is null) then null
------			when ([expiry_date] > getdate()) then dateadd(month, a.months, [expiry_date])
------			else dateadd(month, a.months, getdate())
------		end
------		, months_credit = case when (expiry_date is null and months_credit is null) then a.months
------		when (expiry_date is null and months_credit is not null) then months_credit + a.months
------		else 0
------		end
------		from (
------				select affected_id, bl.months from tb_bill_line bl
------				inner join tb_for_sale fs on bl.for_sale_id = fs.for_sale_id
------				and bill_id = @bill_id and fs.type_name = 'shareable review' and affected_id is not null
------			) a
------	 where review_id = a.affected_id
	 
------	 --2c. update tb_expiry_edit_log
------	 update #existingreviews set new_expiry_date =
------	 (select r.expiry_date from [reviewer].[dbo].[tb_review] r 
------	 where r.review_id = #existingreviews.id_extended)
	 
	 
------	 insert into tb_expiry_edit_log (date_of_edit, type_extended, id_extended, new_expiry_date,
------		old_expiry_date, extended_by_id, extension_type_id)
------	 select date_of_edit, type_extended, id_extended, new_expiry_date,
------		old_expiry_date, extended_by_id, extension_type_id
------	 from #existingreviews
	 
------	 drop table #existingreviews
	 
------------------------------------------------------------------------------------------------------------------	 
	 
------	 --3.create accounts
------		declare @bl int
------		declare cr cursor fast_forward
------		for select line_id from tb_bill b
------			inner join tb_bill_line bl on b.bill_id = bl.bill_id and bl.affected_id is null and b.bill_id = @bill_id
------				inner join tb_for_sale fs on bl.for_sale_id = fs.for_sale_id and fs.type_name = 'professional'
------		open cr
------		fetch next from cr into @bl
------		while @@fetch_status=0
------		begin
------			 insert into reviewer.dbo.tb_contact (contact_name, [date_created], [expiry_date], 
------				months_credit, creator_id, [type], is_site_admin)
------				select null ,getdate(), null, months + 1, purchaser_contact_id, 'professional', 0
------					from tb_bill b 
------						inner join tb_bill_line bl on b.bill_id = bl.bill_id and bl.affected_id is null and b.bill_id = @bill_id and line_id = @bl
------			update tb_bill_line set affected_id = @@identity where line_id = @bl
------			fetch next from cr into @bl
------		end
------		close cr
------		deallocate cr
	
			
------	 --4.create reviews
------		declare cr cursor fast_forward
------		for select line_id from tb_bill b
------			inner join tb_bill_line bl on b.bill_id = bl.bill_id and bl.affected_id is null and b.bill_id = @bill_id
------				inner join tb_for_sale fs on bl.for_sale_id = fs.for_sale_id and fs.type_name = 'shareable review'
------		open cr
------		fetch next from cr into @bl
------		while @@fetch_status=0
------		begin
------			 insert into reviewer.dbo.tb_review (review_name, [date_created], [expiry_date], 
------				months_credit, funder_id)
------				select null, getdate(), null, months, purchaser_contact_id
------				from tb_bill b 
------					inner join tb_bill_line bl on b.bill_id = bl.bill_id and bl.affected_id is null and b.bill_id = @bill_id and line_id = @bl
------			update tb_bill_line set affected_id = @@identity where line_id = @bl
------			fetch next from cr into @bl
------		end
------		close cr
------		deallocate cr

------	--5.change bill to paid
------	update tb_bill set bill_status = 'ok: paid and data committed', date_purchased = getdate() where bill_id = @bill_id
------	commit transaction
------	end try

------	begin catch
------	if (@@trancount > 0) 
------		begin 
------			--error corrections: 1.undo all changes
------			rollback transaction
------			--2.mark bill appropriately
------			update tb_bill set bill_status = 'failure: paid but data not committed', date_purchased = getdate() where bill_id = @bill_id
------		end 
------	end catch

------end


------------------------------------------------------------------




--------use [revieweradmin]
--------go
--------/****** object:  storedprocedure [dbo].[st_contactdetails]    script date: 09/14/2011 15:22:56 ******/
--------set ansi_nulls on
--------go
--------set quoted_identifier on
--------go
---------- =============================================
---------- author:		<author,,name>
---------- alter date: <alter date,,>
---------- description:	<description,,>
---------- =============================================
--------alter procedure [dbo].[st_contactdetails] 
--------(
--------	@contact_id nvarchar(50)
--------)
--------as
--------begin
--------	-- set nocount on added to prevent extra result sets from
--------	-- interfering with select statements.
--------	set nocount on;

--------    -- insert statements for procedure here
--------    select c.contact_id, c.old_contact_id, c.contact_name, c.username, c.[password], c.email, 
--------    max(lt.created) as last_login, c.date_created, c.[expiry_date], c.months_credit, c.creator_id,
--------    c.months_credit, c.[type], c.is_site_admin, c.[description], 
--------    sum(datediff(hour,lt.created ,lt.last_renewed )) as [active_hours]
--------	from reviewer.dbo.tb_contact c
--------	left join revieweradmin.dbo.tb_logon_ticket lt
--------	on c.contact_id = lt.contact_id
--------	where c.contact_id = @contact_id
	
--------	group by c.contact_id, c.old_contact_id, c.contact_name, c.username, c.[password], 
--------	c.date_created, c.[expiry_date], c.email, c.months_credit, c.creator_id,
--------    c.months_credit, c.[type], c.is_site_admin, c.[description]

--------	return

--------end






----------/* procedures changed 
----------st_wdattrdataget

----------*/



----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdattrcountchildren]    script date: 04/04/2011 17:00:50 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go





----------create procedure [dbo].[st_wdattrcountchildren]
----------(
----------	@attribute_id int,
----------	@level int
----------)

----------as

----------set nocount on

----------	if (@level = 0)
----------	begin
----------		select count(a_s.attribute_id) as num_children 
----------		from reviewer.dbo.tb_attribute_set a_s
----------		where a_s.parent_attribute_id = 0
----------		and a_s.set_id = @attribute_id
----------	end
----------	else
----------	begin
----------		select count(a_s.attribute_id) as num_children 
----------		from reviewer.dbo.tb_attribute_set a_s
----------		where a_s.parent_attribute_id = @attribute_id
----------	end

----------set nocount off





----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdattrcountchildrenfromwd]    script date: 04/04/2011 17:01:13 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go






----------create procedure [dbo].[st_wdattrcountchildrenfromwd]
----------(
----------	@attribute_id int,
----------	@level int
----------)

----------as

----------set nocount on

----------	select count(w_d_a.attribute_id) as num_children 
----------	from presenter.dbo.tb_web_database_attr w_d_a
----------	where w_d_a.parent_attribute_id = @attribute_id

----------set nocount off






----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdattrdataget]    script date: 04/04/2011 17:01:25 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go




----------create procedure [dbo].[st_wdattrdataget]
----------(
----------	@level int,
----------	@attribute_id int
----------)

----------as

----------set nocount on

----------	if @level = '0' -- we have clicked on a set and are looking for level 1
----------	begin
----------		select a_s.set_id, a.attribute_id, a.contact_id, a.attribute_name, 
----------		a_s.parent_attribute_id, a_s.attribute_order 
----------		from reviewer.dbo.tb_attribute a inner join reviewer.dbo.tb_attribute_set a_s
----------		on a.attribute_id = a_s.attribute_id
----------		and a_s.set_id = @attribute_id
----------		and a_s.parent_attribute_id = 0
----------		order by a_s.attribute_order
----------	end
	
----------	if @level = '1' --  we have click on a level 1 and
----------	begin
----------		select a_s.set_id, a.attribute_id, a.contact_id, a.attribute_name, 
----------		a_s.parent_attribute_id, a_s.attribute_order 
----------		from reviewer.dbo.tb_attribute a inner join reviewer.dbo.tb_attribute_set a_s
----------		on a.attribute_id = a_s.attribute_id
----------		and a_s.parent_attribute_id = @attribute_id
----------		order by a_s.attribute_order
----------	end

----------set nocount off




----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdattrdatagetfromwd]    script date: 04/04/2011 17:01:41 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go





----------create procedure [dbo].[st_wdattrdatagetfromwd]
----------(
----------	--@level int,
----------	--@attribute_id int,
----------	--@webdb_id int
	
----------	@attribute_id int,
----------	@attribute_set_id nvarchar(50),
----------	@webdb_id int
----------)

----------as

----------set nocount on

----------	if @attribute_set_id = ''
----------	begin
----------		select attribute_id, attribute_set_id, attribute_name, parent_attribute_id, set_id 
----------		from presenter.dbo.tb_web_database_attr
----------		where webdb_id = @webdb_id
----------		and parent_attribute_id = 0
----------		and set_id = @attribute_id
----------		and attribute_set_id != ''
----------		order by attribute_order
----------	end
----------	else
----------	begin
----------		select attribute_id, attribute_set_id, attribute_name, parent_attribute_id, set_id 
----------		from presenter.dbo.tb_web_database_attr
----------		where webdb_id = @webdb_id
----------		and parent_attribute_id = @attribute_id
----------		and attribute_set_id != ''
----------		order by attribute_order
----------	end
----------	/*
----------	if @level = '0' 
----------	begin
----------		select attribute_id, attribute_set_id, attribute_name, parent_attribute_id 
----------		from presenter.dbo.tb_web_database_attr
----------		where [level] = 1
----------		and webdb_id = @webdb_id
----------		and parent_attribute_id = 0
----------		and set_id = @attribute_id
----------		order by attribute_order
----------	end
	
----------	if @level = '1' 
----------	begin
----------		select attribute_id, attribute_set_id, attribute_name, parent_attribute_id 
----------		from presenter.dbo.tb_web_database_attr
----------		where [level] = 1
----------		and webdb_id = @webdb_id
----------		and parent_attribute_id = @attribute_id
----------		order by attribute_order
----------	end
----------	*/


----------set nocount off





----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdcodesetsget]    script date: 04/04/2011 17:01:56 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go



----------create procedure [dbo].[st_wdcodesetsget]
----------(
----------	@review_id int
----------)

----------as

----------set nocount on

----------	select s.set_id, s.set_name, st.set_type from reviewer.dbo.tb_set s 
----------	inner join reviewer.dbo.tb_review_set rs
----------	on s.set_id = rs.set_id
----------	inner join reviewer.dbo.tb_set_type st
----------	on s.set_type_id = st.set_type_id
----------	and review_id = @review_id

----------set nocount off



----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdcreateifnotexist]    script date: 04/04/2011 17:02:15 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go






----------create procedure [dbo].[st_wdcreateifnotexist]
----------(
----------	@review_id int,
----------	@webdb_name nvarchar(100),
----------	@webdb_id int output	
----------)

----------as

----------set nocount on

----------	select * from presenter.dbo.tb_web_database w_d
----------	where w_d.review_id = @review_id
----------	and w_d.webdb_name = @webdb_name
	
----------	if @@rowcount = 0
----------	begin
----------		insert into presenter.dbo.tb_web_database (review_id, webdb_name, [description])
----------		values (@review_id, @webdb_name, 'enter introduction')
		
----------		select @webdb_id = webdb_id from presenter.dbo.tb_web_database w_d
----------		where w_d.review_id = @review_id
----------		and w_d.webdb_name = @webdb_name
----------	end
----------	else
----------	begin
----------		set @webdb_id = 0;
----------	end
	

----------set nocount off






----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wddeletefromwebdb]    script date: 04/04/2011 17:02:30 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go




----------create procedure [dbo].[st_wddeletefromwebdb]
----------(
----------	@webdb_id int,
----------	@id int,
----------	@case nvarchar(50)
----------)

----------as

----------set nocount on

----------	if @case = 'whole set'
----------	begin
----------		delete from presenter.dbo.tb_web_database_attr 
----------		where set_id = @id
----------		and webdb_id = @webdb_id
----------	end
----------	else if @case = 'single attribute'
----------	begin
----------		delete from presenter.dbo.tb_web_database_attr
----------		where attribute_id =  @id
----------	end
----------	else if @case = 'multiple attributes'
----------	begin
----------		select * from presenter.dbo.tb_web_database_attr
----------	end
----------	else
----------	begin
----------		select * from presenter.dbo.tb_web_database_attr
----------	end
	
	
----------set nocount off




----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wddeleteincludecode]    script date: 04/04/2011 17:02:40 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go






----------create procedure [dbo].[st_wddeleteincludecode]
----------(
----------	@webdb_id int
----------)

----------as

----------set nocount on

----------	update presenter.dbo.tb_web_database
----------	set attr_to_include = null
----------	where webdb_id = @webdb_id
	
	
----------set nocount off









----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wddeletewebdb]    script date: 04/04/2011 17:02:50 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go



----------create procedure [dbo].[st_wddeletewebdb]
----------(
----------	@webdb_id int
----------)

----------as

----------set nocount on
	
----------	delete from presenter.dbo.tb_web_database_attr
----------	where webdb_id = @webdb_id
	
----------	delete from presenter.dbo.tb_web_database
----------	where webdb_id = @webdb_id
	
----------set nocount off









----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wddescriptionget]    script date: 04/04/2011 17:03:00 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go





----------create procedure [dbo].[st_wddescriptionget]
----------(
----------	@webdb_id int
----------)

----------as

----------set nocount on

----------	select w_d.[description], w_d.webdb_name, w_d.restrict_access,
----------	w_d.username, w_d.passwd, a.attribute_name 
----------	from presenter.dbo.tb_web_database w_d
----------	left join reviewer.dbo.tb_attribute a
----------	on w_d.attr_to_include = a.attribute_id
----------	where w_d.webdb_id = @webdb_id

----------set nocount off





----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdgetwebdatabases]    script date: 04/04/2011 17:03:11 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go






----------create procedure [dbo].[st_wdgetwebdatabases]
----------(
----------	@review_id int
----------)

----------as

----------set nocount on

----------	select webdb_id, webdb_name from presenter.dbo.tb_web_database
----------	where review_id = @review_id

----------set nocount off






----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdrenameattribute]    script date: 04/04/2011 17:03:21 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go






----------create procedure [dbo].[st_wdrenameattribute]
----------(
----------	@level int,
----------	@attribute_id int,
----------	@name nvarchar (500)
----------)

----------as

----------set nocount on

----------	if @level = '0' -- we are renaming a set
----------	begin
----------		update presenter.dbo.tb_web_database_attr
----------		set attribute_name =  @name
----------		where set_id = @attribute_id
----------		and level = 0
----------	end
	
----------	if @level = '1' --  we are renaming an attribute
----------	begin
----------		update presenter.dbo.tb_web_database_attr
----------		set attribute_name =  @name
----------		where attribute_id = @attribute_id

----------	end

----------set nocount off






----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdsaveintroduction]    script date: 04/04/2011 17:03:31 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go








----------create procedure [dbo].[st_wdsaveintroduction]
----------(
----------	@webdb_id int,
----------	@webdb_name nvarchar(50),
----------	@description nvarchar(max)
----------)

----------as

----------set nocount on

----------	update presenter.dbo.tb_web_database
----------	set [description] = @description, 
----------	webdb_name =@webdb_name
----------	where webdb_id = @webdb_id
	
----------set nocount off








----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdsetcopytowebdb]    script date: 04/04/2011 17:03:40 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go






----------create procedure [dbo].[st_wdsetcopytowebdb]
----------(
----------	@set_id int,
----------	@review_id bigint, 
----------	@webdb_id bigint, 
----------	@inserted bit output
----------)

----------as

----------set nocount on

----------	select * from presenter.dbo.tb_web_database_attr 
----------		where set_id = @set_id
----------		and webdb_id = @webdb_id

----------	if @@rowcount < 1 -- we can insert all of the the rows
----------	begin
----------		-- get the parts from tb_set
----------		insert into presenter.dbo.tb_web_database_attr 
----------		(attribute_id, webdb_id, /*level,*/ attribute_name, parent_attribute_id,
----------		set_id, attribute_order)
----------		select @set_id, @webdb_id, /*0,*/ set_name, 0, @set_id, 0
----------		from reviewer.dbo.tb_set s
----------		where s.set_id = @set_id
		
----------		-- get the parts from tb_attribute_set
----------		insert into presenter.dbo.tb_web_database_attr 
----------		(attribute_id, attribute_set_id, webdb_id, /*level,*/ attribute_name, parent_attribute_id,
----------		set_id, attribute_order)
----------		select a_s.attribute_id, a_s.attribute_set_id,  @webdb_id, /*1,*/ a.attribute_name, a_s.parent_attribute_id, @set_id, 
----------		a_s.attribute_order
----------		from reviewer.dbo.tb_attribute_set a_s inner join reviewer.dbo.tb_attribute a
----------		on a_s.attribute_id = a.attribute_id
----------		and a_s.set_id = @set_id
		
----------		set @inserted = 1
----------	end
----------	else
----------	begin
----------		set @inserted = 0
----------	end
	
----------set nocount off






----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdsetincludecode]    script date: 04/04/2011 17:03:52 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go





----------create procedure [dbo].[st_wdsetincludecode]
----------(
----------	@webdb_id int,
----------	@attribute_id int
----------)

----------as

----------set nocount on

----------	update presenter.dbo.tb_web_database
----------	set attr_to_include = @attribute_id
----------	where webdb_id = @webdb_id
	
	
----------set nocount off








----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdsetpassword]    script date: 04/04/2011 17:04:10 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go




----------create procedure [dbo].[st_wdsetpassword]
----------(
----------	@restrict_access bit,
----------	@passwd nvarchar(50),
----------	@username nvarchar(50),
----------	@webdb_id int
----------)

----------as

----------set nocount on

----------	update presenter.dbo.tb_web_database
----------	set restrict_access = @restrict_access, 
----------	username = @username, passwd = @passwd
----------	where webdb_id = @webdb_id
	
	
----------set nocount off







----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdsetrestrictaccess]    script date: 04/04/2011 17:04:19 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go





----------create procedure [dbo].[st_wdsetrestrictaccess]
----------(
----------	@restrict_access bit,
----------	@webdb_id int
----------)

----------as

----------set nocount on

----------	update presenter.dbo.tb_web_database
----------	set restrict_access = @restrict_access
----------	where webdb_id = @webdb_id
	
	
----------set nocount off








----------go

----------use [revieweradmin]
----------go

----------/****** object:  storedprocedure [dbo].[st_wdtoplevelgetfromwd]    script date: 04/04/2011 17:04:33 ******/
----------set ansi_nulls on
----------go

----------set quoted_identifier on
----------go




----------create procedure [dbo].[st_wdtoplevelgetfromwd]
----------(
----------	@webdb_id int
----------)

----------as

----------set nocount on

----------	select attribute_id, attribute_name, attribute_set_id, set_id 
----------	from presenter.dbo.tb_web_database_attr
----------	where attribute_set_id is null
----------	and webdb_id = @webdb_id

----------set nocount off




----------go






------------use [revieweradmin]
------------go
------------/****** object:  storedprocedure [dbo].[st_contactdetails]    script date: 12/20/2010 16:30:54 ******/
------------set ansi_nulls on
------------go
------------set quoted_identifier on
------------go
-------------- =============================================
-------------- author:		<author,,name>
-------------- alter date: <alter date,,>
-------------- description:	<description,,>
-------------- =============================================
------------alter procedure [dbo].[st_contactdetails] 
------------(
------------	@contact_id nvarchar(50)
------------)
------------as
------------begin
------------	-- set nocount on added to prevent extra result sets from
------------	-- interfering with select statements.
------------	set nocount on;

------------    -- insert statements for procedure here
------------    select c.contact_id, c.old_contact_id, c.contact_name, c.username, c.[password], c.email, 
------------    max(lt.created) as last_login, c.date_created, c.[expiry_date], c.months_credit, c.creator_id,
------------    c.months_credit, c.[type], c.is_site_admin, c.[description]
------------	from reviewer.dbo.tb_contact c
------------	left join revieweradmin.dbo.tb_logon_ticket lt
------------	on c.contact_id = lt.contact_id
------------	where c.contact_id = @contact_id
	
------------	group by c.contact_id, c.old_contact_id, c.contact_name, c.username, c.[password], 
------------	c.date_created, c.[expiry_date], c.email, c.months_credit, c.creator_id,
------------    c.months_credit, c.[type], c.is_site_admin, c.[description]

------------	return

------------end




------------/*
------------   16 december 201015:52:06
------------   user: 
------------   server: db-epi
------------   database: revieweradmin
------------   application: 
------------*/

------------/* to prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
------------begin transaction
------------set quoted_identifier on
------------set arithabort on
------------set numeric_roundabort off
------------set concat_null_yields_null on
------------set ansi_nulls on
------------set ansi_padding on
------------set ansi_warnings on
------------commit
------------begin transaction
------------go
------------execute sp_rename n'dbo.tb_contact_emails_sent.[[4_wk_email_sent]', n'tmp_4_wk_email_sent_2', 'column' 
------------go
------------execute sp_rename n'dbo.tb_contact_emails_sent.tmp_4_wk_email_sent_2', n'4_wk_email_sent', 'column' 
------------go
------------alter table dbo.tb_contact_emails_sent set (lock_escalation = table)
------------go
------------commit
------------select has_perms_by_name(n'dbo.tb_contact_emails_sent', 'object', 'alter') as alt_per, has_perms_by_name(n'dbo.tb_contact_emails_sent', 'object', 'view definition') as view_def_per, has_perms_by_name(n'dbo.tb_contact_emails_sent', 'object', 'control') as contr_per 


----------------------------------------------------------------


------------use [revieweradmin]
------------go
------------/****** object:  storedprocedure [dbo].[st_reviewaddmember]    script date: 12/13/2010 10:30:12 ******/
------------set ansi_nulls on
------------go
------------set quoted_identifier on
------------go

------------alter procedure [dbo].[st_reviewaddmember]
------------(
------------	@review_id int,
------------	@contact_id int
------------)

------------as

------------set nocount on

------------	declare @new_contact_review_id int
	
------------	insert into reviewer.dbo.tb_review_contact(contact_id, review_id)
------------	values (@contact_id, @review_id)
	
------------	set @new_contact_review_id = @@identity
	
------------	insert into reviewer.dbo.tb_contact_review_role(review_contact_id, role_name)
------------	values(@new_contact_review_id, 'regularuser')
	
	
	
	

------------set nocount off

------------------------------------------------------------------






--------------insert into tb_for_sale (type_name, is_active, last_changed, price_per_month, details)
--------------select 'professional', '1', '2010-09-15 00:00:00.000', '10', 'account' union all
--------------select 'shareable review', '1', '2010-09-15 00:00:00.000', '35', 'review'



--------------insert into tb_countries (country_code, country_name, order_number)
--------------select 'us', 'united states', '2' union all
--------------select 'ca', 'canada', '3' union all
--------------select 'af', 'afghanistan', '4' union all
--------------select 'al', 'albania', '5' union all
--------------select 'dz', 'algeria', '6' union all
--------------select 'ds', 'american samoa', '7' union all
--------------select 'ad', 'andorra', '8' union all
--------------select 'ao', 'angola', '9' union all
--------------select 'ai', 'anguilla', '10' union all
--------------select 'aq', 'antarctica', '11' union all
--------------select 'ag', 'antigua and/or barbuda', '12' union all
--------------select 'ar', 'argentina', '13' union all
--------------select 'am', 'armenia', '14' union all
--------------select 'aw', 'aruba', '15' union all
--------------select 'au', 'australia', '16' union all
--------------select 'at', 'austria', '17' union all
--------------select 'az', 'azerbaijan', '18' union all
--------------select 'bs', 'bahamas', '19' union all
--------------select 'bh', 'bahrain', '20' union all
--------------select 'bd', 'bangladesh', '21' union all
--------------select 'bb', 'barbados', '22' union all
--------------select 'by', 'belarus', '23' union all
--------------select 'be', 'belgium', '24' union all
--------------select 'bz', 'belize', '25' union all
--------------select 'bj', 'benin', '26' union all
--------------select 'bm', 'bermuda', '27' union all
--------------select 'bt', 'bhutan', '28' union all
--------------select 'bo', 'bolivia', '29' union all
--------------select 'ba', 'bosnia and herzegovina', '30' union all
--------------select 'bw', 'botswana', '31' union all
--------------select 'bv', 'bouvet island', '32' union all
--------------select 'br', 'brazil', '33' union all
--------------select 'io', 'british lndian ocean territory', '34' union all
--------------select 'bn', 'brunei darussalam', '35' union all
--------------select 'bg', 'bulgaria', '36' union all
--------------select 'bf', 'burkina faso', '37' union all
--------------select 'bi', 'burundi', '38' union all
--------------select 'kh', 'cambodia', '39' union all
--------------select 'cm', 'cameroon', '40' union all
--------------select 'cv', 'cape verde', '41' union all
--------------select 'ky', 'cayman islands', '42' union all
--------------select 'cf', 'central african republic', '43' union all
--------------select 'td', 'chad', '44' union all
--------------select 'cl', 'chile', '45' union all
--------------select 'cn', 'china', '46' union all
--------------select 'cx', 'christmas island', '47' union all
--------------select 'cc', 'cocos (keeling) islands', '48' union all
--------------select 'co', 'colombia', '49' union all
--------------select 'km', 'comoros', '50' union all
--------------select 'cg', 'congo', '51' union all
--------------select 'ck', 'cook islands', '52' union all
--------------select 'cr', 'costa rica', '53' union all
--------------select 'hr', 'croatia (hrvatska)', '54' union all
--------------select 'cu', 'cuba', '55' union all
--------------select 'cy', 'cyprus', '56' union all
--------------select 'cz', 'czech republic', '57' union all
--------------select 'dk', 'denmark', '58' union all
--------------select 'dj', 'djibouti', '59' union all
--------------select 'dm', 'dominica', '60' union all
--------------select 'do', 'dominican republic', '61' union all
--------------select 'tp', 'east timor', '62' union all
--------------select 'ec', 'ecudaor', '63' union all
--------------select 'eg', 'egypt', '64' union all
--------------select 'sv', 'el salvador', '65' union all
--------------select 'gq', 'equatorial guinea', '66' union all
--------------select 'er', 'eritrea', '67' union all
--------------select 'ee', 'estonia', '68' union all
--------------select 'et', 'ethiopia', '69' union all
--------------select 'fk', 'falkland islands (malvinas)', '70' union all
--------------select 'fo', 'faroe islands', '71' union all
--------------select 'fj', 'fiji', '72' union all
--------------select 'fi', 'finland', '73' union all
--------------select 'fr', 'france', '74' union all
--------------select 'fx', 'france, metropolitan', '75' union all
--------------select 'gf', 'french guiana', '76' union all
--------------select 'pf', 'french polynesia', '77' union all
--------------select 'tf', 'french southern territories', '78' union all
--------------select 'ga', 'gabon', '79' union all
--------------select 'gm', 'gambia', '80' union all
--------------select 'ge', 'georgia', '81' union all
--------------select 'de', 'germany', '82' union all
--------------select 'gh', 'ghana', '83' union all
--------------select 'gi', 'gibraltar', '84' union all
--------------select 'gr', 'greece', '85' union all
--------------select 'gl', 'greenland', '86' union all
--------------select 'gd', 'grenada', '87' union all
--------------select 'gp', 'guadeloupe', '88' union all
--------------select 'gu', 'guam', '89' union all
--------------select 'gt', 'guatemala', '90' union all
--------------select 'gn', 'guinea', '91' union all
--------------select 'gw', 'guinea-bissau', '92' union all
--------------select 'gy', 'guyana', '93' union all
--------------select 'ht', 'haiti', '94' union all
--------------select 'hm', 'heard and mc donald islands', '95' union all
--------------select 'hn', 'honduras', '96' union all
--------------select 'hk', 'hong kong', '97' union all
--------------select 'hu', 'hungary', '98' union all
--------------select 'is', 'iceland', '99' union all
--------------select 'in', 'india', '100' union all
--------------select 'id', 'indonesia', '101' union all
--------------select 'ir', 'iran (islamic republic of)', '102' union all
--------------select 'iq', 'iraq', '103' union all
--------------select 'ie', 'ireland', '104' union all
--------------select 'il', 'israel', '105' union all
--------------select 'it', 'italy', '106' union all
--------------select 'ci', 'ivory coast', '107' union all
--------------select 'jm', 'jamaica', '108' union all
--------------select 'jp', 'japan', '109' union all
--------------select 'jo', 'jordan', '110' union all
--------------select 'kz', 'kazakhstan', '111' union all
--------------select 'ke', 'kenya', '112' union all
--------------select 'ki', 'kiribati', '113' union all
--------------select 'kp', 'korea, democratic people''s republic of', '114' union all
--------------select 'kr', 'korea, republic of', '115' union all
--------------select 'kw', 'kuwait', '116' union all
--------------select 'kg', 'kyrgyzstan', '117' union all
--------------select 'la', 'lao people''s democratic republic', '118' union all
--------------select 'lv', 'latvia', '119' union all
--------------select 'lb', 'lebanon', '120' union all
--------------select 'ls', 'lesotho', '121' union all
--------------select 'lr', 'liberia', '122' union all
--------------select 'ly', 'libyan arab jamahiriya', '123' union all
--------------select 'li', 'liechtenstein', '124' union all
--------------select 'lt', 'lithuania', '125' union all
--------------select 'lu', 'luxembourg', '126' union all
--------------select 'mo', 'macau', '127' union all
--------------select 'mk', 'macedonia', '128' union all
--------------select 'mg', 'madagascar', '129' union all
--------------select 'mw', 'malawi', '130' union all
--------------select 'my', 'malaysia', '131' union all
--------------select 'mv', 'maldives', '132' union all
--------------select 'ml', 'mali', '133' union all
--------------select 'mt', 'malta', '134' union all
--------------select 'mh', 'marshall islands', '135' union all
--------------select 'mq', 'martinique', '136' union all
--------------select 'mr', 'mauritania', '137' union all
--------------select 'mu', 'mauritius', '138' union all
--------------select 'ty', 'mayotte', '139' union all
--------------select 'mx', 'mexico', '140' union all
--------------select 'fm', 'micronesia, federated states of', '141' union all
--------------select 'md', 'moldova, republic of', '142' union all
--------------select 'mc', 'monaco', '143' union all
--------------select 'mn', 'mongolia', '144' union all
--------------select 'ms', 'montserrat', '145' union all
--------------select 'ma', 'morocco', '146' union all
--------------select 'mz', 'mozambique', '147' union all
--------------select 'mm', 'myanmar', '148' union all
--------------select 'na', 'namibia', '149' union all
--------------select 'nr', 'nauru', '150' union all
--------------select 'np', 'nepal', '151' union all
--------------select 'nl', 'netherlands', '152' union all
--------------select 'an', 'netherlands antilles', '153' union all
--------------select 'nc', 'new caledonia', '154' union all
--------------select 'nz', 'new zealand', '155' union all
--------------select 'ni', 'nicaragua', '156' union all
--------------select 'ne', 'niger', '157' union all
--------------select 'ng', 'nigeria', '158' union all
--------------select 'nu', 'niue', '159' union all
--------------select 'nf', 'norfork island', '160' union all
--------------select 'mp', 'northern mariana islands', '161' union all
--------------select 'no', 'norway', '162' union all
--------------select 'om', 'oman', '163' union all
--------------select 'pk', 'pakistan', '164' union all
--------------select 'pw', 'palau', '165' union all
--------------select 'pa', 'panama', '166' union all
--------------select 'pg', 'papua new guinea', '167' union all
--------------select 'py', 'paraguay', '168' union all
--------------select 'pe', 'peru', '169' union all
--------------select 'ph', 'philippines', '170' union all
--------------select 'pn', 'pitcairn', '171' union all
--------------select 'pl', 'poland', '172' union all
--------------select 'pt', 'portugal', '173' union all
--------------select 'pr', 'puerto rico', '174' union all
--------------select 'qa', 'qatar', '175' union all
--------------select 're', 'reunion', '176' union all
--------------select 'ro', 'romania', '177' union all
--------------select 'ru', 'russian federation', '178' union all
--------------select 'rw', 'rwanda', '179' union all
--------------select 'kn', 'saint kitts and nevis', '180' union all
--------------select 'lc', 'saint lucia', '181' union all
--------------select 'vc', 'saint vincent and the grenadines', '182' union all
--------------select 'ws', 'samoa', '183' union all
--------------select 'sm', 'san marino', '184' union all
--------------select 'st', 'sao tome and principe', '185' union all
--------------select 'sa', 'saudi arabia', '186' union all
--------------select 'sn', 'senegal', '187' union all
--------------select 'sc', 'seychelles', '188' union all
--------------select 'sl', 'sierra leone', '189' union all
--------------select 'sg', 'singapore', '190' union all
--------------select 'sk', 'slovakia', '191' union all
--------------select 'si', 'slovenia', '192' union all
--------------select 'sb', 'solomon islands', '193' union all
--------------select 'so', 'somalia', '194' union all
--------------select 'za', 'south africa', '195' union all
--------------select 'gs', 'south georgia south sandwich islands', '196' union all
--------------select 'es', 'spain', '197' union all
--------------select 'lk', 'sri lanka', '198' union all
--------------select 'sh', 'st. helena', '199' union all
--------------select 'pm', 'st. pierre and miquelon', '200' union all
--------------select 'sd', 'sudan', '201' union all
--------------select 'sr', 'suriname', '202' union all
--------------select 'sj', 'svalbarn and jan mayen islands', '203' union all
--------------select 'sz', 'swaziland', '204' union all
--------------select 'se', 'sweden', '205' union all
--------------select 'ch', 'switzerland', '206' union all
--------------select 'sy', 'syrian arab republic', '207' union all
--------------select 'tw', 'taiwan', '208' union all
--------------select 'tj', 'tajikistan', '209' union all
--------------select 'tz', 'tanzania, united republic of', '210' union all
--------------select 'th', 'thailand', '211' union all
--------------select 'tg', 'togo', '212' union all
--------------select 'tk', 'tokelau', '213' union all
--------------select 'to', 'tonga', '214' union all
--------------select 'tt', 'trinidad and tobago', '215' union all
--------------select 'tn', 'tunisia', '216' union all
--------------select 'tr', 'turkey', '217' union all
--------------select 'tm', 'turkmenistan', '218' union all
--------------select 'tc', 'turks and caicos islands', '219' union all
--------------select 'tv', 'tuvalu', '220' union all
--------------select 'ug', 'uganda', '221' union all
--------------select 'ua', 'ukraine', '222' union all
--------------select 'ae', 'united arab emirates', '223' union all
--------------select 'gb', 'united kingdom', '1' union all
--------------select 'um', 'united states minor outlying islands', '225' union all
--------------select 'uy', 'uruguay', '226' union all
--------------select 'uz', 'uzbekistan', '227' union all
--------------select 'vu', 'vanuatu', '228' union all
--------------select 'va', 'vatican city state', '229' union all
--------------select 've', 'venezuela', '230' union all
--------------select 'vn', 'vietnam', '231' union all
--------------select 'vg', 'virigan islands (british)', '232' union all
--------------select 'vi', 'virgin islands (u.s.)', '233' union all
--------------select 'wf', 'wallis and futuna islands', '234' union all
--------------select 'eh', 'western sahara', '235' union all
--------------select 'ye', 'yemen', '236' union all
--------------select 'yu', 'yugoslavia', '237' union all
--------------select 'zr', 'zaire', '238' union all
--------------select 'zm', 'zambia', '239' union all
--------------select 'zw', 'zimbabwe', '240'

