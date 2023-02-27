USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[sp_OnlineHelpGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_OnlineHelpGet]
	-- Add the parameters for the stored procedure here
	@CONTEXT nvarchar(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT * from TB_ONLINE_HELP where CONTEXT = @CONTEXT
END
GO
/****** Object:  StoredProcedure [dbo].[st_AllowReviewOwnershipChangeInLicense]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER procedure [dbo].[st_AllowReviewOwnershipChangeInLicense]
(
	@SITE_LIC_ID int,
	@ALLOW_REVIEW_OWNERSHIP_CHANGE bit 
)

As

SET NOCOUNT ON

	update Reviewer.dbo.TB_SITE_LIC
	set ALLOW_REVIEW_OWNERSHIP_CHANGE = @ALLOW_REVIEW_OWNERSHIP_CHANGE
	where SITE_LIC_ID = @SITE_LIC_ID

SET NOCOUNT OFF






GO
/****** Object:  StoredProcedure [dbo].[st_ApplyCreditToAccount]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER   procedure [dbo].[st_ApplyCreditToAccount]
(
	@CREDIT_PURCHASE_ID int,
	@CONTACT_ID int,
	@MONTHS_EXTENDED int,
	@PERSON_EXTENDING_CONTACT_ID int
)

As

SET NOCOUNT ON

	-- extend the review by the selected number of months
	
	-- is the review activated
	declare @monthsCredit int
	declare @oldExpiryDate date
	declare @newExpiryDate date
	declare @newExpiryEditID int

	set @monthsCredit = (select MONTHS_CREDIT from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = @CONTACT_ID)

	declare @extensionTypeID int = (select EXTENSION_TYPE_ID from TB_EXTENSION_TYPES where EXTENSION_TYPE = 'Using credit purchase')

	if @monthsCredit > 0
	begin
		-- this is an unactivated user account so increase the months of credit
		set @monthsCredit = @monthsCredit + @MONTHS_EXTENDED
		update Reviewer.dbo.TB_CONTACT set MONTHS_CREDIT = @monthsCredit
		where CONTACT_ID = @CONTACT_ID
		set @oldExpiryDate = null
		set @newExpiryDate = null
	end
	else
	begin
		set @oldExpiryDate = (select EXPIRY_DATE from Reviewer.dbo.TB_CONTACT
			where CONTACT_ID = @CONTACT_ID)
		if @oldExpiryDate <= getdate()
		begin
			-- it expired so add from today
			update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
				where CONTACT_ID = @CONTACT_ID
			set @newExpiryDate = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
		end
		else
		begin
			-- not yet expired so add from old expiry date
			update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,@oldExpiryDate)
				where CONTACT_ID = @CONTACT_ID
			set @newExpiryDate = DATEADD(MONTH,@MONTHS_EXTENDED,@oldExpiryDate)
		end
	end

	-- add the extension to TB_EXPIRY_EDIT_LOG
	insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE, 
		EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
	values (GETDATE(), 1, @CONTACT_ID, @oldExpiryDate, @newExpiryDate, @PERSON_EXTENDING_CONTACT_ID, @extensionTypeID,
		'Extended using a credit purchase')
	SET @newExpiryEditID = @@IDENTITY

	-- add entry into TB_CREDIT_EXTENSIONS
	insert into TB_CREDIT_EXTENSIONS (CREDIT_PURCHASE_ID, EXPIRY_EDIT_ID)
	values (@CREDIT_PURCHASE_ID, @newExpiryEditID)
		

SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[st_ApplyCreditToReview]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER   procedure [dbo].[st_ApplyCreditToReview]
(
	@CREDIT_PURCHASE_ID int,
	@REVIEW_ID int,
	@MONTHS_EXTENDED int,
	@PERSON_EXTENDING_CONTACT_ID int
)

As

SET NOCOUNT ON

	-- extend the review by the selected number of months
	
	-- is the review activated
	declare @monthsCredit int
	declare @oldExpiryDate date
	declare @newExpiryDate date
	declare @newExpiryEditID int

	set @monthsCredit = (select MONTHS_CREDIT from Reviewer.dbo.TB_REVIEW
	where REVIEW_ID = @REVIEW_ID)

	declare @extensionTypeID int = (select EXTENSION_TYPE_ID from TB_EXTENSION_TYPES where EXTENSION_TYPE = 'Using credit purchase')

	if @monthsCredit > 0
	begin
		-- this is an unactivated review so increase the months of credit
		set @monthsCredit = @monthsCredit + @MONTHS_EXTENDED
		update Reviewer.dbo.TB_REVIEW set MONTHS_CREDIT = @monthsCredit
		where REVIEW_ID = @REVIEW_ID
		set @oldExpiryDate = null
		set @newExpiryDate = null
	end
	else
	begin
		set @oldExpiryDate = (select EXPIRY_DATE from Reviewer.dbo.TB_REVIEW
			where REVIEW_ID = @REVIEW_ID)
		if @oldExpiryDate is null
		begin
			-- this is a non-shareable review so make it shareable 
			update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
			where REVIEW_ID = @REVIEW_ID
			set @newExpiryDate = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
		end
		else
		begin
			-- this is a shareable review so extend the expiry date
			if @oldExpiryDate <= getdate()
			begin
				-- it expired so add from today
				update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
				where REVIEW_ID = @REVIEW_ID
				set @newExpiryDate = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
			end
			else
			begin
				-- not yet expired so add from old expiry date
				update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,@oldExpiryDate)
				where REVIEW_ID = @REVIEW_ID
				set @newExpiryDate = DATEADD(MONTH,@MONTHS_EXTENDED,@oldExpiryDate)
			end
		end
	end



	-- add the extension to TB_EXPIRY_EDIT_LOG
	insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE, 
		EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
	values (GETDATE(), 0, @REVIEW_ID, @oldExpiryDate, @newExpiryDate, @PERSON_EXTENDING_CONTACT_ID, @extensionTypeID,
		'Extended using a credit purchase')
	SET @newExpiryEditID = @@IDENTITY

	-- add entry into TB_CREDIT_EXTENSIONS
	insert into TB_CREDIT_EXTENSIONS (CREDIT_PURCHASE_ID, EXPIRY_EDIT_ID)
	values (@CREDIT_PURCHASE_ID, @newExpiryEditID)
		

SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[st_ArchieCDSave]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER procedure [dbo].[st_ArchieCDSave]
(
	@REVIEW_ID int,
	@ARCHIE_CD char(18)
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	if @ARCHIE_CD = ''
	begin
		update Reviewer.dbo.TB_REVIEW
		set ARCHIE_CD = null
		where REVIEW_ID = @REVIEW_ID
	end
	else
	begin
		update Reviewer.dbo.TB_REVIEW
		set ARCHIE_CD = @ARCHIE_CD
		where REVIEW_ID = @REVIEW_ID	
	end
	



END






GO
/****** Object:  StoredProcedure [dbo].[st_ArchieIDSave]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_ArchieIDSave]
(
	@REVIEW_ID int,
	@ARCHIE_ID char(18)
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	if @ARCHIE_ID = ''
	begin
		update Reviewer.dbo.TB_REVIEW
		set ARCHIE_ID = null
		where REVIEW_ID = @REVIEW_ID
	end
	else
	begin
		update Reviewer.dbo.TB_REVIEW
		set ARCHIE_ID = @ARCHIE_ID
		where REVIEW_ID = @REVIEW_ID	
	end
	



END





GO
/****** Object:  StoredProcedure [dbo].[st_ArchieIsCheckedOutHere]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_ArchieIsCheckedOutHere]
(
	@REVIEW_ID int,
	@IS_CHECKEDOUT_HERE bit
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	update Reviewer.dbo.TB_REVIEW
		set IS_CHECKEDOUT_HERE = @IS_CHECKEDOUT_HERE
		where REVIEW_ID = @REVIEW_ID
	

END
GO
/****** Object:  StoredProcedure [dbo].[st_AttrCountChildren]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_AttrCountChildren]
(
                @ATTRIBUTE_ID bigint,
                @ATTRIBUTE_SET_ID bigint,
                @PARENT_ATTRIBUTE_ID bigint,
                @SET_ID int
)

As

SET NOCOUNT ON

                -- orginal live version
                /*select COUNT(w_d_a.ATTRIBUTE_ID) as NUM_CHILDREN 
                from Presenter.dbo.TB_WEB_DATABASE_ATTR w_d_a
                where w_d_a.SET_ID = @ATTRIBUTE_ID
                and w_d_a.PARENT_ATTRIBUTE_ID = 0
                and w_d_a.ATTRIBUTE_ID != @ATTRIBUTE_ID*/
                

                select COUNT(a_s.ATTRIBUTE_ID) as NUM_CHILDREN 
                from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
                where a_s.SET_ID = @SET_ID
                and a_s.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID
                --and w_d_a.ATTRIBUTE_ID != @ATTRIBUTE_ID

                

SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[st_AttrDataGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







CREATE OR ALTER procedure [dbo].[st_AttrDataGet]
(
      @ATTRIBUTE_ID int,
      @ATTRIBUTE_SET_ID nvarchar(50),
      @SET_ID int
)

As

SET NOCOUNT ON


      if @ATTRIBUTE_SET_ID = '0'
      begin
            select a.ATTRIBUTE_ID, a_s.ATTRIBUTE_SET_ID, a.ATTRIBUTE_NAME, a_s.PARENT_ATTRIBUTE_ID, a_s.SET_ID 
            from Reviewer.dbo.TB_ATTRIBUTE_SET a_s 
            inner join Reviewer.dbo.TB_ATTRIBUTE a on a.ATTRIBUTE_ID = a_s.ATTRIBUTE_ID
            where a_s.SET_ID = @SET_ID
            --and a_s.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID
            order by a_s.ATTRIBUTE_ORDER
      end
      else
            begin
            select a.ATTRIBUTE_ID, a_s.ATTRIBUTE_SET_ID, a.ATTRIBUTE_NAME, a_s.PARENT_ATTRIBUTE_ID, a_s.SET_ID 
            from Reviewer.dbo.TB_ATTRIBUTE_SET a_s 
            inner join Reviewer.dbo.TB_ATTRIBUTE a on a.ATTRIBUTE_ID = a_s.ATTRIBUTE_ID
            where a_s.SET_ID = @SET_ID
            and a_s.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID
            order by a_s.ATTRIBUTE_ORDER
      end

/*
      if @LEVEL = '0' 
      begin
            select ATTRIBUTE_ID, ATTRIBUTE_SET_ID, ATTRIBUTE_NAME, PARENT_ATTRIBUTE_ID 
            from Presenter.dbo.TB_WEB_DATABASE_ATTR
            where [LEVEL] = 1
            and WEBDB_ID = @WEBDB_ID
            and PARENT_ATTRIBUTE_ID = 0
            and SET_ID = @ATTRIBUTE_ID
            order by ATTRIBUTE_ORDER
      end
      
      if @LEVEL = '1' 
      begin
            select ATTRIBUTE_ID, ATTRIBUTE_SET_ID, ATTRIBUTE_NAME, PARENT_ATTRIBUTE_ID 
            from Presenter.dbo.TB_WEB_DATABASE_ATTR
            where [LEVEL] = 1
            and WEBDB_ID = @WEBDB_ID
            and PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID
            order by ATTRIBUTE_ORDER
      end
*/


SET NOCOUNT OFF







GO
/****** Object:  StoredProcedure [dbo].[st_BillAddContactExtension]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_BillAddContactExtension]
(
	@ACCOUNT_CREATOR_ID int,
	@CONTACT_ID int,
	@EXTEND_BY int,
	@BILL_ID int
)

As

SET NOCOUNT ON
--declare @COST int
--declare @ACCOUNT_COST int
declare @FOR_SALE_ID int
declare @CHK int
	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
	--if there is no draft corresponding to the current user, or if there is more than one, then return
	if @CHK != 1 return
	
	
	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
	where [TYPE_NAME] = 'Professional'
	order by LAST_CHANGED desc
	--set @ACCOUNT_COST = @COST * @EXTEND_BY 
	SELECT @CHK = COUNT(line_ID) from TB_BILL_LINE
		where BILL_ID = @BILL_ID and AFFECTED_ID = @CONTACT_ID and FOR_SALE_ID = @FOR_SALE_ID
	if @CHK = 1 
	begin 
		if @EXTEND_BY= 0 delete from TB_BILL_LINE where BILL_ID = @BILL_ID and AFFECTED_ID = @CONTACT_ID and FOR_SALE_ID = @FOR_SALE_ID
		else update TB_BILL_LINE set MONTHS = @EXTEND_BY 
			where BILL_ID = @BILL_ID and AFFECTED_ID = @CONTACT_ID and FOR_SALE_ID = @FOR_SALE_ID
	end
	else
	begin
		if @EXTEND_BY= 0 delete from TB_BILL_LINE where BILL_ID = @BILL_ID and AFFECTED_ID = @CONTACT_ID and FOR_SALE_ID = @FOR_SALE_ID
		else insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
			values (@BILL_ID, @FOR_SALE_ID, @CONTACT_ID, @EXTEND_BY)
	end

SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[st_BillAddCredit]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		Sergio
-- Create date: <Create Date,,>
-- Description:	adds an existing review to a draft bill need exact review_ID and matching full name.
-- =============================================
CREATE   PROCEDURE [dbo].[st_BillAddCredit]
	-- Add the parameters for the stored procedure here
	@bill_ID int,
	@CreditAmount int,
	@Result nvarchar(100) = 'Success' out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    declare @FOR_SALE_ID int
	declare @CHK int
	declare @numberMonths int
	 
	set @Result = 'Success'

	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
	where [TYPE_NAME] = 'Credit purchase'
	order by LAST_CHANGED desc

	declare @pricePerMonth int = (select PRICE_PER_MONTH from TB_FOR_SALE where TYPE_NAME = 'Credit purchase')
	set @numberMonths = @CreditAmount / @pricePerMonth
	
	-- if there is already a credit purchase in the bill then it is an update
	select * from TB_BILL_LINE
	where BILL_ID = @BILL_ID
	and FOR_SALE_ID = @FOR_SALE_ID
	if @@ROWCOUNT = 0
	begin	
		insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
		VALUES (@bill_ID, @FOR_SALE_ID, 0, @numberMonths)
		if @@ROWCOUNT != 1 set @Result = 'Unknown Error, please contact Support.'
	end
	else
	begin
		update TB_BILL_LINE
		set MONTHS = @numberMonths
		where BILL_ID = @bill_ID
		and FOR_SALE_ID = @FOR_SALE_ID
	end
		
	
	
	RETURN
END
GO
/****** Object:  StoredProcedure [dbo].[st_BillAddExistingAccount]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		Sergio
-- Create date: <Create Date,,>
-- Description:	adds an existing review to a draft bill need exact review_ID and matching full name.
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillAddExistingAccount]
	-- Add the parameters for the stored procedure here
	@bill_ID int,
	@ContactID int,
	@Email nvarchar(1000),
	@Result nvarchar(100) = 'Success' out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    declare @FOR_SALE_ID int
	declare @CHK int
	set @Result  = 'Success'
	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
	--if there is no draft corresponding to the current user, or if there is more than one, then return
	if @CHK != 1 
	begin 
		set @Result = 'Current Draft Bill is invalid, please contact support.'
		return
	end
	select @CHK = count(contact_id) from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @ContactID and EMAIL = @Email
	if @CHK != 1 
	begin 
		set @Result = 'Could not find this Account, please make sure that Account ID and E-Mail address are correct.'
		return
	end
	select @CHK = COUNT(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT where CONTACT_ID = @ContactID 
	if @CHK != 0 
	begin 
		set @Result = 'This account is in a Site License and can''t be purchased individually'
		return
	end
	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
	where [TYPE_NAME] = 'Professional'
	order by LAST_CHANGED desc
	
	--set @EXPIRY_DATE = dateadd(month,@NUMBER_MONTHS, sysdatetime())
	
	
	
		insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
		VALUES (@BILL_ID, @FOR_SALE_ID, @ContactID, 3)
		if @@ROWCOUNT != 1 set @Result = 'Unknown Error, please contact Support.'
	
	
	
	RETURN
END
GO
/****** Object:  StoredProcedure [dbo].[st_BillAddExistingReview]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sergio
-- Create date: <Create Date,,>
-- Description:	adds an existing review to a draft bill need exact review_ID and matching full name.
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillAddExistingReview]
	-- Add the parameters for the stored procedure here
	@bill_ID int,
	@revID int,
	@Rev_name nvarchar(1000),
	@Result nvarchar(100) = 'Success' out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    declare @FOR_SALE_ID int
	declare @CHK int
	
	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
	--if there is no draft corresponding to the current user, or if there is more than one, then return
	if @CHK != 1 
	begin 
		set @Result = 'Current Draft Bill is invalid, please contact support.'
		return
	end
	select @CHK = count(review_id) from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @revID and REVIEW_NAME = @Rev_name
	if @CHK != 1 
	begin 
		set @Result = 'Could not find this review, please make sure that Review ID and Review Name are correct.'
		return
	end
	select @CHK = COUNT(REVIEW_ID) from Reviewer.dbo.TB_SITE_LIC_REVIEW where REVIEW_ID = @revID 
	if @CHK != 0 
	begin 
		set @Result = 'This review is in a Site License and can''t be purchased individually'
		return
	end
	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
	where [TYPE_NAME] = 'Shareable Review'
	order by LAST_CHANGED desc
	
	--set @EXPIRY_DATE = dateadd(month,@NUMBER_MONTHS, sysdatetime())
	
	
	
		insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
		VALUES (@BILL_ID, @FOR_SALE_ID, @revID, 3)
		if @@ROWCOUNT != 1 set @Result = 'Unknown Error, please contact Support.'
		else set @Result = 'Success'
	
	
	RETURN
END


GO
/****** Object:  StoredProcedure [dbo].[st_BillAddNewContact]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[st_BillAddNewContact]
(
	@CREATOR_ID int, 
	@NUMBER_MONTHS int,
	@BILL_ID int 
	--@WHEN_TO_START nvarchar(50)
)
AS

	declare @FOR_SALE_ID int
	declare @CHK int
	
	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
	--if there is no draft corresponding to the current user, or if there is more than one, then return
	if @CHK != 1 return
	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
	where [TYPE_NAME] = 'Professional'
	order by LAST_CHANGED desc
	
	--set @EXPIRY_DATE = dateadd(month,@NUMBER_MONTHS, sysdatetime())
	
	
	
		insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
		VALUES (@BILL_ID, @FOR_SALE_ID, NULL, @NUMBER_MONTHS)
		
		--INSERT INTO TB_CONTACT_TMP (PURCHASER_ID, DATE_CREATED, EXPIRY_DATE, WHEN_TO_START, NUMBER_MONTHS, MONTHS_CREDIT)
		--VALUES (@CREATOR_ID, GETDATE(), null, @WHEN_TO_START, @NUMBER_MONTHS, @NUMBER_MONTHS)
	--END
	--ELSE
	--BEGIN
	--	INSERT INTO TB_CONTACT_TMP (PURCHASER_ID, DATE_CREATED, EXPIRY_DATE, WHEN_TO_START, NUMBER_MONTHS, MONTHS_CREDIT)
	--	VALUES (@CREATOR_ID, GETDATE(), @EXPIRY_DATE, @WHEN_TO_START, @NUMBER_MONTHS, '0')	
	--END
	
	--set @NEW_CONTACT_ID = @@IDENTITY
	
	--insert into TB_ACCOUNT_EXTENSION (ACCOUNT_CREATOR_ID, CONTACT_ID, EXTEND_BY)
	--values (@CREATOR_ID, @NEW_CONTACT_ID, @NUMBER_MONTHS)
	
	
	RETURN

GO
/****** Object:  StoredProcedure [dbo].[st_BillAddNewReview]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[st_BillAddNewReview]
(
	@CREATOR_ID int, 
	@NUMBER_MONTHS int,
	@BILL_ID int 
	--@WHEN_TO_START nvarchar(50)
)
AS

	declare @FOR_SALE_ID int
	declare @CHK int
	
	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
	--if there is no draft corresponding to the current user, or if there is more than one, then return
	if @CHK != 1 return
	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
	where [TYPE_NAME] = 'Shareable Review'
	order by LAST_CHANGED desc
	insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
	VALUES (@BILL_ID, @FOR_SALE_ID, NULL, @NUMBER_MONTHS)
	RETURN

GO
/****** Object:  StoredProcedure [dbo].[st_BillAddOutstandingFee]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		Sergio
-- Create date: <Create Date,,>
-- Description:	adds an existing review to a draft bill need exact review_ID and matching full name.
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_BillAddOutstandingFee]
	-- Add the parameters for the stored procedure here
	@bill_ID int,
	@OutstandingFeeAmount int,
	@Result nvarchar(100) = 'Success' out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    declare @FOR_SALE_ID int
	declare @CHK int
	declare @numberMonths int
	 
	set @Result = 'Success'

	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
	where [TYPE_NAME] = 'Outstanding fee'
	order by LAST_CHANGED desc

	declare @pricePerMonth int = (select PRICE_PER_MONTH from TB_FOR_SALE where TYPE_NAME = 'Outstanding fee')

	set @numberMonths = @OutstandingFeeAmount / @pricePerMonth
	
	-- if there is already an outstanding fee in the bill then it is an update
	select * from TB_BILL_LINE
	where BILL_ID = @BILL_ID
	and FOR_SALE_ID = @FOR_SALE_ID
	if @@ROWCOUNT = 0
	begin	
		insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
		VALUES (@bill_ID, @FOR_SALE_ID, 0, @numberMonths)
		if @@ROWCOUNT != 1 set @Result = 'Unknown Error, please contact Support.'
	end
	else
	begin
		update TB_BILL_LINE
		set MONTHS = @numberMonths
		where BILL_ID = @bill_ID
		and FOR_SALE_ID = @FOR_SALE_ID
	end
		
	
	
	RETURN
END
GO
/****** Object:  StoredProcedure [dbo].[st_BillAddressEdit]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_BillAddressEdit]
(
	@CONTACT_ID int,
	@ORGANISATION nvarchar(250),
	@ADDRESS nvarchar(500),
	@COUNTRY_ID int,
	@EU_VAT_REG_NUMBER nvarchar(50)
)

As

SET NOCOUNT ON

	select * from TB_BILL_ADDRESS where PURCHASER_CONTACT_ID = @CONTACT_ID
	if @@ROWCOUNT > 0
	begin
		update TB_BILL_ADDRESS
		set ADDRESS = @ADDRESS, COUNTRY_ID = @COUNTRY_ID,
		ORGANISATION = @ORGANISATION,
		EU_VAT_REG_NUMBER = @EU_VAT_REG_NUMBER
		where PURCHASER_CONTACT_ID = @CONTACT_ID
	end
	else
	begin
		insert into TB_BILL_ADDRESS(PURCHASER_CONTACT_ID, ADDRESS, COUNTRY_ID,
		 ORGANISATION, EU_VAT_REG_NUMBER)
		values (@CONTACT_ID, @ADDRESS, @COUNTRY_ID, @ORGANISATION, @EU_VAT_REG_NUMBER)
	end


SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[st_BillAddressGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<gets contact table for a contactID>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillAddressGet] 
(
	@CONTACT_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    -- need to lose the milliseconds as the database is setting it to 000

	select c.CONTACT_NAME,b_a.ADDRESS, b_a.COUNTRY_ID, c.EMAIL, b_a.ORGANISATION, 
	cnt.COUNTRY_NAME, b_a.EU_VAT_REG_NUMBER 
	from TB_BILL_ADDRESS b_a
	inner join Reviewer.dbo.TB_CONTACT c
	on b_a.PURCHASER_CONTACT_ID = c.CONTACT_ID
	inner join TB_COUNTRIES cnt
	on cnt.COUNTRY_ID = b_a.COUNTRY_ID
	where b_a.PURCHASER_CONTACT_ID = @CONTACT_ID
    	
	RETURN
END
GO
/****** Object:  StoredProcedure [dbo].[st_BillCreate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_BillCreate]
(
	@CONTACT_ID int,
	@NOMINAL_PRICE int,
	@VAT nvarchar(50),
	@DUE_PRICE nvarchar(50),
	@BILL_ID int output
)

As

SET NOCOUNT ON
declare @NEW_BILL_ID int
declare @VAT_CHARGED bit
declare @VAT_RATE nvarchar(50)
declare @EU_VAT_REG_NUMBER nvarchar(50)

	insert into TB_BILL(PURCHASER_CONTACT_ID, NOMINAL_PRICE, VAT, DUE_PRICE)
	values (@CONTACT_ID, @NOMINAL_PRICE, @VAT, 0)
	
	set @NEW_BILL_ID = @@IDENTITY
	
	-- add tax and condition details
	
	update TB_BILL
	set CONDITIONS_ID = 
	(select top 1 CONDITIONS_ID from TB_TERMS_AND_CONDITIONS order by CONDITIONS_ID DESC)
	where BILL_ID = @NEW_BILL_ID
	
	update TB_BILL
	set VAT_RATE = 
	(select top 1 VAT_RATE from TB_TERMS_AND_CONDITIONS order by CONDITIONS_ID DESC)
	where BILL_ID = @NEW_BILL_ID
	
	update TB_BILL
	set VAT = @VAT where BILL_ID = @NEW_BILL_ID
	
	select @BILL_ID = BILL_ID from TB_BILL where BILL_ID = @NEW_BILL_ID

SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[st_BillDetailsAccounts]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillDetailsAccounts] 
(
	@BILL_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select b_l.LINE_ID, f_s.DETAILS, f_s.TYPE_NAME, b_l.AFFECTED_ID, c.CONTACT_NAME as NAME, b_l.MONTHS,
	c.MONTHS_CREDIT, f_s.PRICE_PER_MONTH, (f_s.PRICE_PER_MONTH * b_l.MONTHS) as COST
	from TB_BILL_LINE b_l
	inner join TB_FOR_SALE f_s
	on b_l.FOR_SALE_ID = f_s.FOR_SALE_ID
	inner join Reviewer.dbo.TB_CONTACT c
	on c.CONTACT_ID = b_l.AFFECTED_ID
	where b_l.BILL_ID = @BILL_ID
	and f_s.DETAILS = 'Account'
    
    /*
	select b_l.LINE_ID, f_s.DETAILS, b_l.AFFECTED_ID, c.CONTACT_NAME as NAME, b_l.MONTHS, 
	f_s.PRICE_PER_MONTH, (f_s.PRICE_PER_MONTH * b_l.MONTHS) as COST
	from TB_BILL_LINE b_l
	inner join TB_FOR_SALE f_s
	on f_s.FOR_SALE_ID = b_l.FOR_SALE_ID
	inner join Reviewer.dbo.TB_CONTACT c
	on c.CONTACT_ID = b_l.AFFECTED_ID
	and b_l.BILL_ID = @BILL_ID
	and f_s.DETAILS = 'Account'
	*/
	RETURN

END
GO
/****** Object:  StoredProcedure [dbo].[st_BillDetailsCredit]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_BillDetailsCredit] 
(
	@BILL_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @ForSaleID int
	set @ForSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Credit purchase')

	select b_l.LINE_ID, (f_s.PRICE_PER_MONTH * b_l.MONTHS) as COST
	from TB_BILL_LINE b_l
	inner join TB_FOR_SALE f_s
	on b_l.FOR_SALE_ID = f_s.FOR_SALE_ID
	where b_l.BILL_ID = @BILL_ID
	and f_s.FOR_SALE_ID = @ForSaleID
	RETURN

END
GO
/****** Object:  StoredProcedure [dbo].[st_BillDetailsOutstandingFees]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_BillDetailsOutstandingFees] 
(
	@BILL_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @ForSaleID int
	set @ForSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Outstanding fee')

	select b_l.LINE_ID, (f_s.PRICE_PER_MONTH * b_l.MONTHS) as COST
	from TB_BILL_LINE b_l
	inner join TB_FOR_SALE f_s
	on b_l.FOR_SALE_ID = f_s.FOR_SALE_ID
	where b_l.BILL_ID = @BILL_ID
	and f_s.FOR_SALE_ID = @ForSaleID
	RETURN

END
GO
/****** Object:  StoredProcedure [dbo].[st_BillDetailsReviews]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillDetailsReviews] 
(
	@BILL_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select b_l.LINE_ID, f_s.DETAILS, f_s.TYPE_NAME, b_l.AFFECTED_ID, r.REVIEW_NAME as NAME, b_l.MONTHS,
	r.MONTHS_CREDIT, f_s.PRICE_PER_MONTH, (f_s.PRICE_PER_MONTH * b_l.MONTHS) as COST
	from TB_BILL_LINE b_l
	inner join TB_FOR_SALE f_s
	on b_l.FOR_SALE_ID = f_s.FOR_SALE_ID
	inner join Reviewer.dbo.TB_REVIEW r
	on r.REVIEW_ID = b_l.AFFECTED_ID
	where b_l.BILL_ID = @BILL_ID
	and f_s.DETAILS = 'Review'
    
    /*
	select b_l.LINE_ID, f_s.DETAILS, b_l.AFFECTED_ID, r.REVIEW_NAME as NAME, b_l.MONTHS, 
	f_s.PRICE_PER_MONTH, (f_s.PRICE_PER_MONTH * b_l.MONTHS) as COST
	from TB_BILL_LINE b_l
	inner join TB_FOR_SALE f_s
	on f_s.FOR_SALE_ID = b_l.FOR_SALE_ID
	inner join Reviewer.dbo.TB_REVIEW r
	on r.REVIEW_ID = b_l.AFFECTED_ID
	and b_l.BILL_ID = @BILL_ID
	and f_s.DETAILS = 'Review'
	*/
	RETURN

END
GO
/****** Object:  StoredProcedure [dbo].[st_BillExtendGhost]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[st_BillExtendGhost]
(
	@CREATOR_ID int, 
	@BILL_LINE_ID int,
	@NUMBER_MONTHS int,
	@BILL_ID int
)
AS

	--declare @FOR_SALE_ID int
	declare @CHK int
	
	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
	--if there is no draft corresponding to the current user, or if there is more than one, then return
	if @CHK != 1 return
	SELECT @CHK = bl.LINE_ID from TB_BILL_LINE bl 
		inner join TB_BILL b on b.BILL_ID = bl.BILL_ID and bl.BILL_ID = @BILL_ID and LINE_ID = @BILL_LINE_ID and b.BILL_STATUS = 'Draft'

	--select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
	--where [TYPE_NAME] = 'Professional'
	--order by LAST_CHANGED desc
	if @NUMBER_MONTHS = 0 delete from TB_BILL_LINE where LINE_ID = @CHK --AND @FOR_SALE_ID = FOR_SALE_ID
	else UPDATE TB_BILL_LINE set MONTHS = @NUMBER_MONTHS
		where LINE_ID = @CHK --AND @FOR_SALE_ID = FOR_SALE_ID

GO
/****** Object:  StoredProcedure [dbo].[st_BillGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillGet] 
(
	@BILL_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select c.CONTACT_NAME, b.BILL_ID, b.DATE_PURCHASED, b.NOMINAL_PRICE, b.DISCOUNT,
	b.DUE_PRICE, b.CONDITIONS_ID, b.BILL_STATUS, b.DATE_PAYMENT_RECEIVED,
	b.PURCHASER_CONTACT_ID, b.VAT
	from TB_BILL b
	inner join Reviewer.dbo.TB_CONTACT c
	on b.PURCHASER_CONTACT_ID = c.CONTACT_ID
	where b.BILL_ID = @BILL_ID

	RETURN

END
GO
/****** Object:  StoredProcedure [dbo].[st_BillGetDraft]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillGetDraft]
(
	@CONTACT int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	--first, mark as expired the submitted bills that did not end up with success or failure
	update TB_BILL set BILL_STATUS = 'submitted to WPM: timed out'
		where BILL_STATUS = 'submitted to WPM' and PURCHASER_CONTACT_ID = @CONTACT
    -- get the details from tb_bill
	select c.CONTACT_NAME, b.BILL_ID, b.DATE_PURCHASED, b.NOMINAL_PRICE, b.DISCOUNT,
	b.DUE_PRICE, b.CONDITIONS_ID, b.BILL_STATUS, b.DATE_PAYMENT_RECEIVED,
	b.PURCHASER_CONTACT_ID, b.VAT
	from TB_BILL b
	inner join Reviewer.dbo.TB_CONTACT c
	on b.PURCHASER_CONTACT_ID = c.CONTACT_ID AND c.CONTACT_ID = @CONTACT
	and b.BILL_STATUS = 'Draft'
	--second reader get the bill lines
	select LINE_ID, bl.BILL_ID, bl.FOR_SALE_ID, fs.TYPE_NAME, AFFECTED_ID, MONTHS MONTHS_CREDIT, b.PURCHASER_CONTACT_ID, PRICE_PER_MONTH * MONTHS COST
	, c.CONTACT_NAME as AFFECTED_NAME
	From TB_BILL_LINE bl inner join TB_BILL b 
		on bl.BILL_ID = b.BILL_ID and b.BILL_STATUS = 'Draft' and b.PURCHASER_CONTACT_ID = @CONTACT
		inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Professional'
		left outer join Reviewer.dbo.TB_CONTACT c on bl.AFFECTED_ID = c.CONTACT_ID and bl.AFFECTED_ID is not null
	UNION
	select LINE_ID, bl.BILL_ID, bl.FOR_SALE_ID, fs.TYPE_NAME, AFFECTED_ID, MONTHS MONTHS_CREDIT, b.PURCHASER_CONTACT_ID, PRICE_PER_MONTH * MONTHS COST
	, r.REVIEW_NAME as AFFECTED_NAME
	From TB_BILL_LINE bl inner join TB_BILL b 
		on bl.BILL_ID = b.BILL_ID and b.BILL_STATUS = 'Draft' and b.PURCHASER_CONTACT_ID = @CONTACT
		inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Shareable Review'
		left outer join Reviewer.dbo.TB_REVIEW r on bl.AFFECTED_ID = r.REVIEW_ID and bl.AFFECTED_ID is not null
	UNION
	select LINE_ID, bl.BILL_ID, bl.FOR_SALE_ID, fs.TYPE_NAME, AFFECTED_ID, MONTHS MONTHS_CREDIT, b.PURCHASER_CONTACT_ID, PRICE_PER_MONTH * MONTHS COST
	, '' as AFFECTED_NAME
	From TB_BILL_LINE bl inner join TB_BILL b 
		on bl.BILL_ID = b.BILL_ID and b.BILL_STATUS = 'Draft' and b.PURCHASER_CONTACT_ID = @CONTACT
		inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Credit purchase'
	UNION
	select LINE_ID, bl.BILL_ID, bl.FOR_SALE_ID, fs.TYPE_NAME, AFFECTED_ID, MONTHS MONTHS_CREDIT, b.PURCHASER_CONTACT_ID, PRICE_PER_MONTH * MONTHS COST
	, '' as AFFECTED_NAME
	From TB_BILL_LINE bl inner join TB_BILL b 
		on bl.BILL_ID = b.BILL_ID and b.BILL_STATUS = 'Draft' and b.PURCHASER_CONTACT_ID = @CONTACT
		inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Outstanding fee'
	RETURN

END

GO
/****** Object:  StoredProcedure [dbo].[st_BillGetSubmitted]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillGetSubmitted]
	-- Add the parameters for the stored procedure here
	@CONTACT_ID int,
	@BILL_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT * from TB_BILL
	where TB_BILL.BILL_ID = @BILL_ID and (BILL_STATUS = 'submitted to WPM' or BILL_STATUS = 'submitted to WPM: timed out')
	 and PURCHASER_CONTACT_ID = @CONTACT_ID
END

GO
/****** Object:  StoredProcedure [dbo].[st_BillMarkAsCancelled]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillMarkAsCancelled]
	-- Add the parameters for the stored procedure here
	@CONTACT_ID int,
	@BILL_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	UPDATE TB_BILL set BILL_STATUS = 'Cancelled by User'
	where TB_BILL.BILL_ID = @BILL_ID 
		and (BILL_STATUS = 'submitted to WPM' or BILL_STATUS = 'submitted to WPM: timed out') 
		and PURCHASER_CONTACT_ID = @CONTACT_ID
END


GO
/****** Object:  StoredProcedure [dbo].[st_BillMarkAsFailed]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillMarkAsFailed]
	-- Add the parameters for the stored procedure here
	@BILL_ID int,
	@WHY nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
	
	update TB_BILL set BILL_STATUS = @WHY where BILL_ID = @bill_ID
	

END

GO
/****** Object:  StoredProcedure [dbo].[st_BillMarkAsPaid]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author: <Author,,Name>
-- Create date: <Create Date,,>
-- Description: <Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillMarkAsPaid]
-- Add the parameters for the stored procedure here
@BILL_ID int
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;
--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRY
BEGIN TRANSACTION
------------------------------------------------------------------------------------------------------------
-- Existing Accounts
--1a.collect data for TB_EXPIRY_EDIT_LOG
CREATE TABLE #ExistingAccounts(DATE_OF_EDIT datetime, TYPE_EXTENDED int, ID_EXTENDED int,
NEW_EXPIRY_DATE date, OLD_EXPIRY_DATE date, LENGTH_OF_EXTENSION int, EXTENDED_BY_ID int,
EXTENSION_TYPE_ID int)
INSERT INTO #ExistingAccounts (DATE_OF_EDIT, ID_EXTENDED, TYPE_EXTENDED, EXTENSION_TYPE_ID, LENGTH_OF_EXTENSION)
Select GETDATE(), AFFECTED_ID as ID_EXTENDED, '1', '2', bl.MONTHS from TB_BILL_LINE bl
inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
and BILL_ID = @bill_ID and fs.TYPE_NAME = 'professional' and AFFECTED_ID is not null
-- TYPE_EXTENDED = 1 for contacts
update #ExistingAccounts set OLD_EXPIRY_DATE =
(select c.EXPIRY_DATE from [Reviewer].[dbo].[TB_CONTACT] c
where c.CONTACT_ID = #ExistingAccounts.ID_EXTENDED)
update #ExistingAccounts set EXTENDED_BY_ID =
(select PURCHASER_CONTACT_ID from TB_BILL b
inner join [Reviewer].[dbo].[TB_CONTACT] c on c.CONTACT_ID = b.PURCHASER_CONTACT_ID
where b.BILL_ID = @bill_ID)
--1b.extend existing accounts
update Reviewer.dbo.TB_CONTACT set
[EXPIRY_DATE] = case
when ([EXPIRY_DATE] is null) then null
when ([EXPIRY_DATE] > getdate()) then DATEADD(month, a.MONTHS, [EXPIRY_DATE])
ELSE DATEADD(month, a.MONTHS, getdate())
end
, MONTHS_CREDIT = case when (EXPIRY_DATE is null and MONTHS_CREDIT is null)
then MONTHS
when (EXPIRY_DATE is null and MONTHS_CREDIT is not null) then MONTHS_CREDIT + a.MONTHS
else 0
end
from (
Select AFFECTED_ID, bl.MONTHS from TB_BILL_LINE bl
inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
and BILL_ID = @bill_ID and fs.TYPE_NAME = 'professional' and AFFECTED_ID is not null
) a
where CONTACT_ID = a.AFFECTED_ID
--1c. update TB_EXPIRY_EDIT_LOG
update #ExistingAccounts set NEW_EXPIRY_DATE =
(select c.EXPIRY_DATE from [Reviewer].[dbo].[TB_CONTACT] c
where c.CONTACT_ID = #ExistingAccounts.ID_EXTENDED)
insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID)
select DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID
from #ExistingAccounts
drop table #ExistingAccounts
------------------------------------------------------------------------------------------------------------
-- Existing Reviews
--2a.collect data for TB_EXPIRY_EDIT_LOG
CREATE TABLE #ExistingReviews(DATE_OF_EDIT datetime, TYPE_EXTENDED int, ID_EXTENDED int,
NEW_EXPIRY_DATE date, OLD_EXPIRY_DATE date, LENGTH_OF_EXTENSION int, EXTENDED_BY_ID int,
EXTENSION_TYPE_ID int)
INSERT INTO #ExistingReviews (DATE_OF_EDIT, ID_EXTENDED, TYPE_EXTENDED, EXTENSION_TYPE_ID, LENGTH_OF_EXTENSION)
Select GETDATE(), AFFECTED_ID as ID_EXTENDED, '0', '2', bl.MONTHS from TB_BILL_LINE bl
inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
and BILL_ID = @bill_ID and fs.TYPE_NAME = 'Shareable Review' and AFFECTED_ID is not null
-- TYPE_EXTENDED = 0 for reviews
update #ExistingReviews set OLD_EXPIRY_DATE =
(select r.EXPIRY_DATE from [Reviewer].[dbo].[TB_REVIEW] r
where r.REVIEW_ID = #ExistingReviews.ID_EXTENDED)
update #ExistingReviews set EXTENDED_BY_ID = PURCHASER_CONTACT_ID from TB_BILL b
where b.BILL_ID = @bill_ID
--2b.extend existing reviews
update Reviewer.dbo.TB_REVIEW set
[EXPIRY_DATE] = case
When ([EXPIRY_DATE] is null) then null
when ([EXPIRY_DATE] > getdate()) then DATEADD(month, a.MONTHS, [EXPIRY_DATE])
ELSE DATEADD(month, a.MONTHS, getdate())
end
, MONTHS_CREDIT = Case When (EXPIRY_DATE is null and MONTHS_CREDIT is null) then a.MONTHS
when (EXPIRY_DATE is null and MONTHS_CREDIT is not null) then MONTHS_CREDIT + a.MONTHS
else 0
end
from (
Select AFFECTED_ID, bl.MONTHS from TB_BILL_LINE bl
inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
and BILL_ID = @bill_ID and fs.TYPE_NAME = 'Shareable Review' and AFFECTED_ID is not null
) a
where REVIEW_ID = a.AFFECTED_ID
--2c. update TB_EXPIRY_EDIT_LOG
update #ExistingReviews set NEW_EXPIRY_DATE =
(select r.EXPIRY_DATE from [Reviewer].[dbo].[TB_REVIEW] r
where r.REVIEW_ID = #ExistingReviews.ID_EXTENDED)
insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID)
select DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID
from #ExistingReviews
drop table #ExistingReviews
------------------------------------------------------------------------------------------------------------
--3.create accounts
declare @bl int
declare cr cursor FAST_FORWARD
for select LINE_ID from tb_bill b
inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID
inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'professional'
open cr
fetch next from cr into @bl
while @@fetch_status=0
begin
insert into Reviewer.dbo.TB_CONTACT (CONTACT_NAME, [DATE_CREATED], [EXPIRY_DATE],
MONTHS_CREDIT, CREATOR_ID, [TYPE], IS_SITE_ADMIN)
Select Null ,getdate(), Null, MONTHS + 1, PURCHASER_CONTACT_ID, 'Professional', 0
from TB_BILL b
inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID and LINE_ID = @bl
update TB_BILL_LINE set AFFECTED_ID = @@IDENTITY where LINE_ID = @bl
fetch next from cr into @bl
end
close cr
deallocate cr
--4.create reviews
declare cr cursor FAST_FORWARD
for select LINE_ID from tb_bill b
inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID
inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Shareable Review'
open cr
fetch next from cr into @bl
while @@fetch_status=0
begin
insert into Reviewer.dbo.TB_REVIEW (REVIEW_NAME, [DATE_CREATED], [EXPIRY_DATE],
MONTHS_CREDIT, FUNDER_ID)
select Null, GETDATE(), Null, MONTHS, PURCHASER_CONTACT_ID
from TB_BILL b
inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID and LINE_ID = @bl
update TB_BILL_LINE set AFFECTED_ID = @@IDENTITY where LINE_ID = @bl
fetch next from cr into @bl
end
close cr
deallocate cr
---------------------------------------------------------------------------------------------
--5. mark any outstanding fees as paid
-- there should only be one line for outstanding fees in the bill as they get added up and put in one line
declare @PurchaserContactID int
set @PurchaserContactID = (select PURCHASER_CONTACT_ID from TB_BILL where BILL_ID = @bill_ID)
declare @ForSaleID int
set @ForSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Outstanding fee')
select * from TB_BILL_LINE
where BILL_ID = @bill_ID
and FOR_SALE_ID = @ForSaleID
if @@ROWCOUNT > 0
begin
-- there was an outstanding fee in the bill so mark them as paid in TB_OUTSTANDING_FEE
update TB_OUTSTANDING_FEE
set STATUS = 'Paid'
where ACCOUNT_ID = @PurchaserContactID
and STATUS like 'Outstanding'
end
--6. fill in TB_CREDIT_PURCHASE if a credit purchase was made
set @PurchaserContactID = (select PURCHASER_CONTACT_ID from TB_BILL where BILL_ID = @bill_ID)
set @ForSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Credit purchase')
declare @amountPurchased int
select * from TB_BILL_LINE
where BILL_ID = @bill_ID
and FOR_SALE_ID = @ForSaleID
if @@ROWCOUNT > 0
begin
-- get the amount purchased
set @amountPurchased = (select MONTHS from TB_BILL_LINE where BILL_ID = @bill_ID and FOR_SALE_ID = @ForSaleID)
set @amountPurchased = @amountPurchased * 5
-- there was a credit purchase in the bill so add it TB_CREDIT_PURCHASE
insert into TB_CREDIT_PURCHASE (PURCHASER_CONTACT_ID, DATE_PURCHASED, CREDIT_PURCHASED, NOTES, PURCHASE_TYPE)
values (@PurchaserContactID, getdate(), @amountPurchased, 'Online shop purchase', 'Shop')
end
--7.change bill to paid
update TB_BILL set BILL_STATUS = 'OK: Paid and data committed', DATE_PURCHASED = GETDATE() where BILL_ID = @bill_ID
COMMIT TRANSACTION
END TRY
BEGIN CATCH
IF (@@TRANCOUNT > 0)
BEGIN
--error corrections: 1.undo all changes
ROLLBACK TRANSACTION
--2.mark bill appropriately
update TB_BILL set BILL_STATUS = 'FAILURE: paid but data NOT committed', DATE_PURCHASED = GETDATE() where BILL_ID = @bill_ID
END
END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[st_BillMarkAsSubmitted]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillMarkAsSubmitted]
	-- Add the parameters for the stored procedure here
	@CONTACT_ID int,
	@BILL_ID int,
	@VAT float 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	UPDATE TB_BILL set BILL_STATUS = 'submitted to WPM', NOMINAL_PRICE = price, DUE_PRICE = price, VAT = @VAT --CONVERT(nvarchar(50),price*VAT_RATE/100) 
	from (
		select CAST(sum(bl.MONTHS * fs.PRICE_PER_MONTH) as float ) price from TB_BILL_LINE bl 
			inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
		where bl.BILL_ID = @BILL_ID
		) as a
	where TB_BILL.BILL_ID = @BILL_ID and BILL_STATUS = 'Draft' and PURCHASER_CONTACT_ID = @CONTACT_ID
END

GO
/****** Object:  StoredProcedure [dbo].[st_BillRemoveGhost]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[st_BillRemoveGhost]
(
	@CREATOR_ID int, 
	@BILL_LINE_ID int,
	@BILL_ID int
)
AS

	declare @FOR_SALE_ID int
	declare @CHK int
	
	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
	--if there is no draft corresponding to the current user, or if there is more than one, then return
	if @CHK != 1 return
	SELECT @CHK = bl.LINE_ID from TB_BILL_LINE bl 
		inner join TB_BILL b on b.BILL_ID = bl.BILL_ID and bl.BILL_ID = @BILL_ID and LINE_ID = @BILL_LINE_ID and b.BILL_STATUS = 'Draft'

	DELETE from TB_BILL_LINE where LINE_ID = @CHK

GO
/****** Object:  StoredProcedure [dbo].[st_BillRemoveOutstandingFee]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO








-- =============================================
-- Author:		Sergio
-- Create date: <Create Date,,>
-- Description:	adds an existing review to a draft bill need exact review_ID and matching full name.
-- =============================================
CREATE OR ALTER     PROCEDURE [dbo].[st_BillRemoveOutstandingFee]
(
	@bill_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    declare @FOR_SALE_ID int
	declare @CHK int
	declare @numberMonths int
	 

	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
	where [TYPE_NAME] = 'Outstanding fee'
	order by LAST_CHANGED desc

	
	-- find the relevant entry in TB_BILL_LINE and remove it
	select * from TB_BILL_LINE
	where BILL_ID = @BILL_ID
	and FOR_SALE_ID = @FOR_SALE_ID
	if @@ROWCOUNT = 1
	begin	
		delete from TB_BILL_LINE
		where BILL_ID = @bill_ID
		and FOR_SALE_ID = @FOR_SALE_ID
	end

END
GO
/****** Object:  StoredProcedure [dbo].[st_BillReviewExtend]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[st_BillReviewExtend]
(
	@CREATOR_ID int, 
	@REVIEW_ID int,
	@NUMBER_MONTHS int,
	@BILL_ID int
)
AS

	SET NOCOUNT ON
--declare @COST int
--declare @ACCOUNT_COST int
declare @FOR_SALE_ID int
declare @CHK int
	SELECT @CHK = count(BILL_ID) from TB_BILL where BILL_STATUS = 'Draft' and BILL_ID = @BILL_ID
	--if there is no draft corresponding to the current user, or if there is more than one, then return
	if @CHK != 1 return
	
	
	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
	where [TYPE_NAME] = 'Shareable Review'
	order by LAST_CHANGED desc
	
	SELECT @CHK = COUNT(line_ID) from TB_BILL_LINE
		where BILL_ID = @BILL_ID and AFFECTED_ID = @REVIEW_ID and FOR_SALE_ID = @FOR_SALE_ID
	if @CHK = 1 
	begin 
		if @NUMBER_MONTHS = 0 delete from TB_BILL_LINE where BILL_ID = @BILL_ID and AFFECTED_ID = @REVIEW_ID and FOR_SALE_ID = @FOR_SALE_ID
		else update TB_BILL_LINE set MONTHS = @NUMBER_MONTHS 
			where BILL_ID = @BILL_ID and AFFECTED_ID = @REVIEW_ID and FOR_SALE_ID = @FOR_SALE_ID
	end
	else
	begin
		if @NUMBER_MONTHS = 0 delete from TB_BILL_LINE 
			where BILL_ID = @BILL_ID and AFFECTED_ID = @REVIEW_ID and FOR_SALE_ID = @FOR_SALE_ID
		else insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
			values (@BILL_ID, @FOR_SALE_ID, @REVIEW_ID, @NUMBER_MONTHS)
	end

SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[st_BritishLibraryCCValuesSet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_BritishLibraryCCValuesSet]
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
/****** Object:  StoredProcedure [dbo].[st_BritishLibraryCCValuesSetOnLicense]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_BritishLibraryCCValuesSetOnLicense]
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
/****** Object:  StoredProcedure [dbo].[st_BritishLibraryValuesGetAll]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<gets contact table for a contactID>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BritishLibraryValuesGetAll] 
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
/****** Object:  StoredProcedure [dbo].[st_BritishLibraryValuesGetFromLicense]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<gets contact table for a contactID>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BritishLibraryValuesGetFromLicense] 
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
/****** Object:  StoredProcedure [dbo].[st_BritishLibraryValuesSet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_BritishLibraryValuesSet]
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
/****** Object:  StoredProcedure [dbo].[st_BritishLibraryValuesSetOnLicense]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_BritishLibraryValuesSetOnLicense]
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
/****** Object:  StoredProcedure [dbo].[st_ChangeLicenseModel]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







CREATE OR ALTER procedure [dbo].[st_ChangeLicenseModel]
(
	@SITE_LIC_ID int,
	@SITE_LIC_MODEL int 
)

As

SET NOCOUNT ON

	update Reviewer.dbo.TB_SITE_LIC
	set SITE_LIC_MODEL = @SITE_LIC_MODEL
	where SITE_LIC_ID = @SITE_LIC_ID

SET NOCOUNT OFF







GO
/****** Object:  StoredProcedure [dbo].[st_ChangeReviewOwner]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER procedure [dbo].[st_ChangeReviewOwner]
(
	@CONTACT_ID int,
	@REVIEW_ID int
)

As

SET NOCOUNT ON

	declare @review_contact_id int

	-- change the review owner
	update Reviewer.dbo.TB_REVIEW
	set FUNDER_ID = @CONTACT_ID
	where REVIEW_ID = @REVIEW_ID
	
	select * from Reviewer.dbo.TB_REVIEW_CONTACT where CONTACT_ID = @CONTACT_ID and REVIEW_ID = @REVIEW_ID
	if @@ROWCOUNT = 0
	begin
		-- we know they weren't in the review so put them in and make them an admin
		insert into Reviewer.dbo.TB_REVIEW_CONTACT (REVIEW_ID, CONTACT_ID)
		values (@REVIEW_ID, @CONTACT_ID)		
		
		set @review_contact_id = (select REVIEW_CONTACT_ID from Reviewer.dbo.TB_REVIEW_CONTACT
			where CONTACT_ID = @CONTACT_ID and REVIEW_ID = @REVIEW_ID)
		insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
		values (@review_contact_id, 'AdminUser')	
	end
	else
	begin
		-- they are in the review so get their @review_contact_id
		set @review_contact_id = (select REVIEW_CONTACT_ID from Reviewer.dbo.TB_REVIEW_CONTACT
			where CONTACT_ID = @CONTACT_ID and REVIEW_ID = @REVIEW_ID)
			
		-- lots of users have more than one role in a review so first check if they already have the admin role
		select * from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE where REVIEW_CONTACT_ID = @review_contact_id and ROLE_NAME = 'AdminUser'
		if @@ROWCOUNT = 0
		begin
			insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
			values (@review_contact_id, 'AdminUser')
		end
	
	end	
	

	

SET NOCOUNT OFF






GO
/****** Object:  StoredProcedure [dbo].[st_CheckGhostAccountBeforeActivation]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_CheckGhostAccountBeforeActivation] 
(
	@CONTACT_ID int,
	--@USERNAME nvarchar(50),
	@EMAIL nvarchar(500),
	@RESULT nvarchar(100) out
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    Declare @Chk int = 0
  --   set @Chk = (select COUNT(Contact_id) from Reviewer.dbo.TB_CONTACT where USERNAME = @USERNAME and CONTACT_ID != @CONTACT_ID)
  --   if @Chk > 0
  --   begin
		--set @RESULT = 'Username is already in use, please choose a different username'
		--RETURN
  --   end
  --   else
  --   begin
		set @Chk = (select COUNT(Contact_id) from Reviewer.dbo.TB_CONTACT where EMAIL = @EMAIL and CONTACT_ID != @CONTACT_ID)
		if @Chk > 0 set @RESULT = 'E-Mail is already in use'
		else
		begin
			select @Chk = COUNT(contact_id) from Reviewer.dbo.TB_CONTACT where [EXPIRY_DATE] is null and PWASHED is null and CONTACT_ID = @CONTACT_ID
			if @Chk = 0
			begin
				select @RESULT = 'E-Mail is not in use, but Contact_ID does not match a Ghost account'
			end
			if @Chk = 1
			begin
				set @RESULT = 'Valid'
				update Reviewer.dbo.TB_CONTACT set [EMAIL] = @EMAIL where CONTACT_ID = @CONTACT_ID
			end
			
		end

END



GO
/****** Object:  StoredProcedure [dbo].[st_CheckLinkAddFailureLog]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_CheckLinkAddFailureLog]
	-- Add the parameters for the stored procedure here
	
	@CID int = null
	, @UID uniqueidentifier = null
	, @TYPE varchar(15) = null
	, @QUERY_ST nvarchar(1000) = null
	, @IP_B1 tinyint
	, @IP_B2 tinyint
	, @IP_B3 tinyint
	, @IP_B4 tinyint
	, @REASON nchar(500)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- First add the line to the log
	INSERT into TB_CHECKLINK_LOG
           ([CHECKLINK_UID]
           ,[TYPE]
           ,[QUERY_STRING]
           ,[CONTACT_ID]
           ,[FAILURE_REASON]
           ,[ALREADY_SENT]
           ,[IP_B1]
           ,[IP_B2]
           ,[IP_B3]
           ,[IP_B4])
     VALUES
           (@UID
           ,@TYPE
           ,@QUERY_ST
           ,@CID
           ,@REASON
           ,0
           ,@IP_B1 
			, @IP_B2 
			, @IP_B3 
			, @IP_B4)
	--Second, grab the data that should be sent to eppisupport@ioe.ac.uk, if any
	--this is complex to avoid sending one message per error in case many attempts are failing in short time-spans
	
	DECLARE @count int
	Declare @MinutesFromLastSent int
	select @MinutesFromLastSent = DATEDIFF(MINUTE,  max(DATE_CREATED), GETDATE()) from TB_CHECKLINK_LOG where ALREADY_SENT = 1
	
	if @MinutesFromLastSent >= 30 --OK to send, last email was sent at least 30 minutes ago
	BEGIN
		UPDATE TB_CHECKLINK_LOG set ALREADY_SENT = 1 where ALREADY_SENT = 0
		Select top(@@ROWCOUNT) * from TB_CHECKLINK_LOG order by DATE_CREATED desc --should return the rows that need to be sent
		--code side will read the rows and compose the email notif
	END
	

END

GO
/****** Object:  StoredProcedure [dbo].[st_CheckLinkCheck]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_CheckLinkCheck]
	-- Add the parameters for the stored procedure here
	 @CID int
	, @UID uniqueidentifier 
	, @RESULT varchar(15) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    DECLARE @DateCheck Datetime2(1)
    DECLARE @Type varchar(15)
    Select @DateCheck = DATE_CREATED, @Type = [TYPE] 
		from TB_CHECKLINK where CONTACT_ID = @CID and CHECKLINK_UID = @UID and 
		(
			(IS_STALE = 0 and WAS_SUCCESS is null)
			or
			( --if previous "successful" attempt happened less than 6 seconds ago, it was probably the Outlook "safelink" mechanism,
			  --which visits a page before letting the user seeying it
				(
					([TYPE] = 'CheckEmail' and IS_STALE = 1 and WAS_SUCCESS = 1)
					or
					([TYPE] = 'ResetPw' and IS_STALE = 1 and WAS_SUCCESS is null)
					--'ActivateGhost' can be attempted more than once already, so no need to account for "safelink" mechanism
				)
				and DATEDIFF(ss, DATE_USED, GETDATE()) <= 6
			)
		)
		
	if @DateCheck is null OR @@ROWCOUNT != 1 
	begin
		set @RESULT = 'Not found'
		return
	end
	--possible types are: CheckEmail, ResetPw and ActivateGhost
	IF @Type = 'CheckEmail'
	Begin
		if DATEDIFF(D, @DateCheck, GETDATE()) <= 7
		begin
			set @RESULT = @Type
			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE(), WAS_SUCCESS = 1 where CHECKLINK_UID = @UID and @CID = CONTACT_ID
		end
		else 
		Begin
			set @RESULT = 'ExpiredCkEmail'
			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE(), WAS_SUCCESS = 0 where CHECKLINK_UID = @UID and @CID = CONTACT_ID
		end
		return
	END
	if @Type = 'ResetPw'
	Begin
		if DATEDIFF(MINUTE, @DateCheck, GETDATE()) <= 60
		begin
			set @RESULT = @Type
			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE() where CHECKLINK_UID = @UID and @CID = CONTACT_ID
		end
		else
		begin
			set @RESULT = 'ExpiredResetPw'
			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE(), WAS_SUCCESS = 0 where CHECKLINK_UID = @UID and @CID = CONTACT_ID
		end
		return
	END
	if @Type = 'ActivateGhost'
	Begin
		if DATEDIFF(D, @DateCheck, GETDATE()) <= 14
		begin
			set @RESULT = @Type
		end
		else 
		begin
			set @RESULT = 'ExpiredActGhost'
			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE(), WAS_SUCCESS = 0 where CHECKLINK_UID = @UID and @CID = CONTACT_ID
		end
		return
	END
	
END

GO
/****** Object:  StoredProcedure [dbo].[st_CheckLinkCreate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_CheckLinkCreate]
	-- Add the parameters for the stored procedure here
	@TYPE varchar(15)
	, @CID int
	, @CC_EMAIL nvarchar(500) = null
	, @UID uniqueidentifier OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    --should we check if the Contact_ID actually exist?
	UPDATE TB_CHECKLINK set IS_STALE = 1 where [TYPE] = @TYPE and CONTACT_ID = @CID
	
	SET @UID = NEWID()
	INSERT INTO TB_CHECKLINK
           (CHECKLINK_UID
           ,TYPE
           ,DATE_CREATED
           ,CONTACT_ID
           ,IS_STALE
           ,DATE_USED
           ,WAS_SUCCESS
           ,CC_EMAIL
           )
     VALUES
           (@UID
           ,@TYPE
           ,GETDATE()
           ,@CID
           ,0
           ,null
           ,null
           ,@CC_EMAIL
           )

END

GO
/****** Object:  StoredProcedure [dbo].[st_CheckUserNameAndEmail]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:                            <Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:   <Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_CheckUserNameAndEmail] 
(
                @CONTACT_ID int,
                @USERNAME nvarchar(50),
                @EMAIL nvarchar(500),
                @RESULT nvarchar(100) out
)
AS
BEGIN
                -- SET NOCOUNT ON added to prevent extra result sets from
                -- interfering with SELECT statements.
                SET NOCOUNT ON;

    -- Insert statements for procedure here
    Declare @Chk int = 0
     set @Chk = (select COUNT(Contact_id) from Reviewer.dbo.TB_CONTACT where USERNAME = @USERNAME and CONTACT_ID != @CONTACT_ID)
     if @Chk > 0
     begin
                                set @RESULT = 'Username is already in use, please choose a different username'
                                RETURN
     end
     else
     begin
                                set @Chk = (select COUNT(Contact_id) from Reviewer.dbo.TB_CONTACT where EMAIL = @EMAIL and CONTACT_ID != @CONTACT_ID)
                                if @Chk > 0 set @RESULT = 'E-Mail is already in use, please choose a different E-Mail'
                                else set @RESULT = 'Valid'
     end
                RETURN

END

GO
/****** Object:  StoredProcedure [dbo].[st_CodesetsGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_CodesetsGet]
(
                @REVIEW_ID int
)

As

SET NOCOUNT ON

                select s.SET_ID, s.SET_NAME, st.SET_TYPE from Reviewer.dbo.TB_SET s 
                inner join Reviewer.dbo.TB_REVIEW_SET rs
                on s.SET_ID = rs.SET_ID
                inner join Reviewer.dbo.TB_SET_TYPE st
                on s.SET_TYPE_ID = st.SET_TYPE_ID
                and REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[st_ConditionsGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<gets contact table for a contactID>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ConditionsGet] 
(
	@CONDITIONS_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    -- need to lose the milliseconds as the database is setting it to 000
    if @CONDITIONS_ID = 0
    begin
		select top 1 CONDITIONS from TB_TERMS_AND_CONDITIONS
		order by DATE_CREATED desc
    
    end
    else
    begin
		select CONDITIONS from TB_TERMS_AND_CONDITIONS
		where CONDITIONS_ID = @CONDITIONS_ID
		order by DATE_CREATED 
    end
    	
	RETURN
END
GO
/****** Object:  StoredProcedure [dbo].[st_ConditionsUploadNew]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[st_ConditionsUploadNew]
(
	@NEW_CONDITIONS nvarchar(MAX)
)
AS

	declare @NEW_CONTACT_ID int
	
	INSERT INTO TB_TERMS_AND_CONDITIONS(CONDITIONS)
	VALUES (@NEW_CONDITIONS)
	
	RETURN
GO
/****** Object:  StoredProcedure [dbo].[st_ContactAccessControl]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_ContactAccessControl] 
(
	@CONTACT_ID int,
	@PAGE_TO_ACCESS nvarchar(20)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select * from TB_ACCESS_CONTROL 
	where CONTACT_ID = @CONTACT_ID
	and PAGE_TO_ACCESS = @PAGE_TO_ACCESS


	RETURN

END



GO
/****** Object:  StoredProcedure [dbo].[st_ContactActivate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_ContactActivate]
(
	@CONTACT_ID int
)

As

SET NOCOUNT ON

	declare @NUMBER_MONTHS int
	declare @EXPIRY_DATE date
	declare @creator_id int
	declare @reason nvarchar(35) = 'The CreatorID activated the account'

	select @NUMBER_MONTHS = MONTHS_CREDIT 
	from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = @CONTACT_ID and [EXPIRY_DATE] is null
	
	--activating an account, this could be a ghost account (MONTHS_CREDIT != 0) or a normal one (MONTHS_CREDIT is null)
	if @NUMBER_MONTHS is null OR @NUMBER_MONTHS = 0 
	BEGIN
		select @NUMBER_MONTHS = 1
		select @reason = 'Email Verified'
	end
	
	set @EXPIRY_DATE = dateadd(month,@NUMBER_MONTHS, sysdatetime())
	
	update Reviewer.dbo.TB_CONTACT
	set [EXPIRY_DATE] = @EXPIRY_DATE, MONTHS_CREDIT = 0
	where CONTACT_ID = @CONTACT_ID
	
	select @creator_id = CREATOR_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID

	-- add a line to TB_EXPIRY_EDIT_LOG to say the account has been activated
	insert into ReviewerAdmin.dbo.TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, OLD_EXPIRY_DATE,
		NEW_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
	values (GETDATE(), 1, @CONTACT_ID, null, DATEADD(month, @NUMBER_MONTHS, GETDATE()), 
		@creator_id, 19, @reason)
	
	

SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[st_ContactAddToReview]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER     procedure [dbo].[st_ContactAddToReview]
(
	@EMAIL nvarchar(500),
	@REVIEW_ID int,
	@RESULT int output

	
	-- RESULT
        -- 0 - everything OK and account updated
        -- 1 - email not found
		-- 2 - there is more than 1 account with this email address

	
)

As

SET NOCOUNT ON

set @RESULT = 0
declare @BREAK int = 0
declare @CONTACT_ID int
declare @NEW_REVIEW_CONTACT_ID int

	-- check if email is already in use
	select * from Reviewer.dbo.TB_CONTACT
	where EMAIL = @EMAIL
	if @@ROWCOUNT = 0 
	begin
		set @RESULT = 1
		set @BREAK = 1
	end
	else if @@ROWCOUNT > 1
	begin 
		set @RESULT = 1
		set @BREAK = 1
	end
	else -- @@ROWCOUNT = 1 so everything OK
	begin
		set @CONTACT_ID = (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = @EMAIL)

		insert into Reviewer.dbo.TB_REVIEW_CONTACT(CONTACT_ID, REVIEW_ID)
		values (@CONTACT_ID, @REVIEW_ID)	
		set @NEW_REVIEW_CONTACT_ID = @@IDENTITY

		insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE(REVIEW_CONTACT_ID, ROLE_NAME)
		values(@NEW_REVIEW_CONTACT_ID, 'RegularUser')
	end



RETURN @RESULT


SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[st_ContactBills]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactBills] 
(
	@CONTACT_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select c.CONTACT_NAME, b.BILL_ID, b.DATE_PURCHASED, b.NOMINAL_PRICE, b.DISCOUNT,
	b.DUE_PRICE, b.CONDITIONS_ID, b.BILL_STATUS, b.DATE_PAYMENT_RECEIVED,
	b.PURCHASER_CONTACT_ID, b.VAT 
	from TB_BILL b
	inner join Reviewer.dbo.TB_CONTACT c
	on b.PURCHASER_CONTACT_ID = c.CONTACT_ID
	where b.PURCHASER_CONTACT_ID = @CONTACT_ID

	RETURN

END

GO
/****** Object:  StoredProcedure [dbo].[st_ContactCheckUnameOrEmail]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactCheckUnameOrEmail]
	-- Add the parameters for the stored procedure here
	@Uname varchar(50) = '' OUTPUT
	,@Email nvarchar(500) = '' OUTPUT
	,@CID int = 0 OUTPUT
	,@CONTACT_NAME nvarchar(255) = '' OUTPUT
	,@IS_ACTIVE bit = 0 OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	if (@Uname is not null and @Uname != '') AND (@Email is null or @Email = '')
	begin 
		Select @CID = CONTACT_ID, @Email = EMAIL, @CONTACT_NAME = CONTACT_NAME, 
					@IS_ACTIVE = CASE 
							WHEN FLAVOUR IS NULL OR EXPIRY_DATE is null then 0
							ELSE 1
						END
		  from Reviewer.dbo.TB_CONTACT where @Uname = USERNAME
	End
	ELSE
	begin 
		if (@Email is not null and @Email != '') AND (@Uname is null or @Uname = '')
		BEGIN
			Select @CID = CONTACT_ID, @CONTACT_NAME = CONTACT_NAME, @Uname = USERNAME, 
					@IS_ACTIVE = CASE 
							WHEN FLAVOUR IS NULL OR EXPIRY_DATE is null then 0
							ELSE 1
						END
		   from Reviewer.dbo.TB_CONTACT where @Email = EMAIL
		END
		ELSE 
		BEGIN 
			IF (@Email is not null and @Email != '') AND (@Uname is not null and @Uname != '')
			BEGIN
				Select @CID = CONTACT_ID, @CONTACT_NAME = CONTACT_NAME, 
						@IS_ACTIVE = CASE 
							WHEN FLAVOUR IS NULL OR EXPIRY_DATE is null then 0
							ELSE 1
						END
		   from Reviewer.dbo.TB_CONTACT where @Email = EMAIL and @Uname = USERNAME
			END
		END
	END
	if @CID is null select @CID = 0
	if @CONTACT_NAME is null select @CONTACT_NAME = ''
	if @Uname is null select @Uname = ''
	if @IS_ACTIVE is null select @IS_ACTIVE = 1 --we don't want to do stuff when we didn't find the account that needs activating
END

GO
/****** Object:  StoredProcedure [dbo].[st_ContactCreate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[st_ContactCreate]
(
	@CONTACT_NAME nvarchar(255), 
	@USERNAME nvarchar(50), 
	@PASSWORD varchar(50), 
	@DATE_CREATED datetime, 
	--@EXPIRY_DATE date, 
	@EMAIL nvarchar(500),
	@DESCRIPTION nvarchar(1000),
	@CONTACT_ID int output
)
AS

	declare @NEW_CONTACT_ID int
	--create salt!
	DECLARE @chars char(100) = '!т#$%&а()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~ийклм'
	declare @rnd varchar(20)
	declare @cnt int = 0
	set @rnd = ''
	WHILE (@cnt <= 20) 
	BEGIN
		SELECT @rnd = @rnd + 
			SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
		SELECT @cnt = @cnt + 1
	END
		
	INSERT INTO Reviewer.dbo.TB_CONTACT(CONTACT_NAME, USERNAME, DATE_CREATED, [EXPIRY_DATE], EMAIL, DESCRIPTION
										,FLAVOUR, PWASHED)
	VALUES (@CONTACT_NAME, @USERNAME, @DATE_CREATED, Null, @EMAIL, @DESCRIPTION
			, @rnd, HASHBYTES('SHA1', @PASSWORD + @rnd))
	
	set @NEW_CONTACT_ID = @@IDENTITY
	
	update Reviewer.dbo.TB_CONTACT set CREATOR_ID = @NEW_CONTACT_ID
	where CONTACT_ID = @NEW_CONTACT_ID
	
	select @CONTACT_ID = CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @NEW_CONTACT_ID
	
	RETURN

GO
/****** Object:  StoredProcedure [dbo].[st_ContactDetails]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactDetails] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
    max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
				CASE when l.[EXPIRY_DATE] is not null 
				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then l.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end as 'EXPIRY_DATE', 
	c.MONTHS_CREDIT, c.CREATOR_ID,
    c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], c.SEND_NEWSLETTER, c.ARCHIE_ID,
    l.SITE_LIC_ID, l.SITE_LIC_NAME
    ,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
	from Reviewer.dbo.TB_CONTACT c
	left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
	on c.CONTACT_ID = lt.CONTACT_ID
	left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
	left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
	where c.CONTACT_ID = @CONTACT_ID
	
	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
	c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID, c.SEND_NEWSLETTER,
    c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], c.ARCHIE_ID, l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME

	RETURN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

END
GO
/****** Object:  StoredProcedure [dbo].[st_ContactDetailsEdit]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER   procedure [dbo].[st_ContactDetailsEdit]
(
	@CONTACT_ID int,
	@CONTACT_NAME nvarchar(255),
	@USERNAME nvarchar(50),
	@EMAIL nvarchar(500),
	@OLD_PASSWORD varchar(50),
	@NEW_PASSWORD varchar(50),
	@RESULT int output

	
	-- RESULT
        -- 0 - everything OK and account updated
        -- 1 - email already in use
        -- 2 - username already in use
        -- 3 - oldPassword is not correct
	
)

As

SET NOCOUNT ON

set @RESULT = 0
declare @BREAK int = 0
declare @EXISTING_USERNAME nvarchar(50) 

	-- check if email is already in use
	select * from Reviewer.dbo.TB_CONTACT
	where EMAIL = @EMAIL
	and CONTACT_ID != @CONTACT_ID
	if @@ROWCOUNT > 0 
	begin
		set @RESULT = 1
		set @BREAK = 1
	end
	
	 -- username
	if @BREAK = 0
	begin
		-- check if username is already in use
		select * from Reviewer.dbo.TB_CONTACT
		where USERNAME = @USERNAME
		and CONTACT_ID != @CONTACT_ID
		if @@ROWCOUNT > 0 
		begin
			set @RESULT = 2
			set @BREAK = 1
		end
	end

	-- password
	if (@BREAK = 0) AND (@NEW_PASSWORD != '')
	begin
		-- check if @OLD_PASSWORD is correct
		-- the USERNAME might be changing as well so use the original username for this check
		set @EXISTING_USERNAME = (select @USERNAME from Reviewer.dbo.TB_CONTACT
		where CONTACT_ID = @CONTACT_ID)

		select * from Reviewer.dbo.TB_CONTACT
		where USERNAME = @EXISTING_USERNAME
		and PWASHED = HASHBYTES('SHA1', @OLD_PASSWORD + FLAVOUR)
		if @@ROWCOUNT != 1
		begin
			set @RESULT = 3
			set @BREAK = 1
		end
	end


	if @BREAK = 0
	begin
		-- everything is good so update the account details
		if @NEW_PASSWORD = ''
		begin
			update Reviewer.dbo.TB_CONTACT 
			set CONTACT_NAME = @CONTACT_NAME,
			USERNAME = @USERNAME,
			EMAIL = @EMAIL
			where CONTACT_ID = @CONTACT_ID
		end
		else
		begin
			--create salt!
			DECLARE @chars char(100) = '!т#$%&а()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~ийклм'
			declare @rnd varchar(20)
			declare @cnt int = 0
			set @rnd = ''
			WHILE (@cnt <= 20) 
			BEGIN
				SELECT @rnd = @rnd + 
					SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
				SELECT @cnt = @cnt + 1
			END
			update Reviewer.dbo.TB_CONTACT 
			set CONTACT_NAME = @CONTACT_NAME
				, USERNAME = @USERNAME
				, EMAIL = @EMAIL
				, FLAVOUR = @rnd
				, PWASHED = HASHBYTES('SHA1', @NEW_PASSWORD + @rnd)
			where CONTACT_ID = @CONTACT_ID
		end
	end


RETURN @RESULT


SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[st_ContactDetailsEmail]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactDetailsEmail] 
(
	@EMAIL nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
    max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, c.[EXPIRY_DATE], c.MONTHS_CREDIT, c.CREATOR_ID,
    c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN
	from Reviewer.dbo.TB_CONTACT c
	left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
	on c.CONTACT_ID = lt.CONTACT_ID
	where c.EMAIL like @EMAIL
	
	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
	c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
    c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN

	RETURN

END
GO
/****** Object:  StoredProcedure [dbo].[st_ContactDetailsFullCreateOrEdit]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_ContactDetailsFullCreateOrEdit]
(
	@NEW_ACCOUNT bit,
	@CONTACT_NAME nvarchar(255),
	@USERNAME nvarchar(50),
	@PASSWORD varchar(50),
	@DATE_CREATED datetime,
	@EXPIRY_DATE date,
	@CREATOR_ID int,
	@EMAIL nvarchar(500),
	@DESCRIPTION nvarchar(1000),
	@IS_SITE_ADMIN bit,
	@CONTACT_ID nvarchar(50), -- it might say 'New'
	@EXTENSION_TYPE_ID int,
	@EXTENSION_NOTES nvarchar(500),
	@EDITOR_ID int,
	@MONTHS_CREDIT nvarchar(50),
	@ARCHIE_ID varchar(32),
	@RESULT nvarchar(50) output
)


As

SET NOCOUNT ON
DECLARE @ORIGINAL_EXPIRY datetime
DECLARE @NEW_ROW_ID int

if @EXPIRY_DATE = ''
begin
	SET @EXPIRY_DATE = NULL
end

BEGIN TRY

BEGIN TRANSACTION
	if @NEW_ACCOUNT = 1
	begin
				
		-- create a new account
		INSERT INTO Reviewer.dbo.TB_CONTACT(CONTACT_NAME, USERNAME, /*[PASSWORD],*/ 
			DATE_CREATED, [EXPIRY_DATE], EMAIL, [DESCRIPTION], CREATOR_ID, MONTHS_CREDIT)
		VALUES (@CONTACT_NAME, @USERNAME, /*@PASSWORD,*/ @DATE_CREATED, @EXPIRY_DATE,
			@EMAIL, @DESCRIPTION, @CREATOR_ID, @MONTHS_CREDIT)
	
		set @RESULT = @@IDENTITY
		if (@CREATOR_ID != @CONTACT_ID)
		begin
			update Reviewer.dbo.TB_CONTACT 
			set CREATOR_ID = @RESULT
			where CONTACT_ID = @RESULT
			and USERNAME = @USERNAME
		end
		
		DECLARE @chars char(100) = '!т#$%&а()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~ийклм'
		declare @rnd varchar(20)
		declare @cnt int = 0
		set @rnd = ''
		WHILE (@cnt <= 20) 
		BEGIN
			SELECT @rnd = @rnd + 
				SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
			SELECT @cnt = @cnt + 1
		END
		update Reviewer.dbo.TB_CONTACT 
		set FLAVOUR = @rnd, PWASHED = HASHBYTES('SHA1', @PASSWORD + @rnd)
		where CONTACT_ID = @RESULT
		
		
		
    end
	else  -- edit an existing account
	begin	
		-- get original expiry date from TB_CONTACT
		select @ORIGINAL_EXPIRY = c.EXPIRY_DATE 
		from Reviewer.dbo.TB_CONTACT c
		where c.CONTACT_ID = @CONTACT_ID
		
		--update TB_CONTACT
		update Reviewer.dbo.TB_CONTACT 
		set 
		CONTACT_NAME = @CONTACT_NAME,
		USERNAME = @USERNAME,
		/*[PASSWORD] = @PASSWORD,*/
		DATE_CREATED = @DATE_CREATED,
		[EXPIRY_DATE] = @EXPIRY_DATE,
		CREATOR_ID = @CREATOR_ID,
		EMAIL = @EMAIL,
		[DESCRIPTION] = @DESCRIPTION,
		IS_SITE_ADMIN = @IS_SITE_ADMIN,
		MONTHS_CREDIT = @MONTHS_CREDIT,
		ARCHIE_ID = @ARCHIE_ID
		where CONTACT_ID = @CONTACT_ID
		
		-- to get the null value correct
		if @ARCHIE_ID = ''
		begin
			update Reviewer.dbo.TB_CONTACT 
			set ARCHIE_ID = null
			where CONTACT_ID = @CONTACT_ID
		end
		
		
		
		if (@PASSWORD != '')
		begin
			DECLARE @chars1 char(100) = '!т#$%&а()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~ийклм'
			declare @rnd1 varchar(20)
			declare @cnt1 int = 0
			set @rnd1 = ''
			WHILE (@cnt1 <= 20) 
			BEGIN
				SELECT @rnd1 = @rnd1 + 
					SUBSTRING(@chars1, CONVERT(int, RAND() * 100), 1)
				SELECT @cnt1 = @cnt1 + 1
			END
			update Reviewer.dbo.TB_CONTACT 
			set FLAVOUR = @rnd1, PWASHED = HASHBYTES('SHA1', @PASSWORD + @rnd1)
			where CONTACT_ID = @CONTACT_ID
		end
		
		
		if (@EXTENSION_TYPE_ID > 1)
		begin
			-- create new row in TB_EXPIRY_EDIT_LOG -- using bogus old expiry date
			insert into TB_EXPIRY_EDIT_LOG
			(DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
			OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
			values (GETDATE(), 1, @CONTACT_ID, @EXPIRY_DATE, @EXPIRY_DATE,
			@EDITOR_ID, @EXTENSION_TYPE_ID, @EXTENSION_NOTES)
			set @NEW_ROW_ID = @@IDENTITY
			
			--  update TB_EXPIRY_EDIT_LOG
			update TB_EXPIRY_EDIT_LOG
			set OLD_EXPIRY_DATE = @ORIGINAL_EXPIRY
			where EXPIRY_EDIT_ID = @NEW_ROW_ID
	
			set @RESULT = 'Valid'		 
		end
	end
	
	
	
COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Invalid'
end
END CATCH

RETURN

SET NOCOUNT OFF



GO
/****** Object:  StoredProcedure [dbo].[st_ContactDetailsGetAll]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactDetailsGetAll] 
(
	@ER4AccountsOnly bit
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 
	if @ER4AccountsOnly = 1
	begin        
		/*
		SELECT * FROM Reviewer.dbo.tb_CONTACT 
		where EXPIRY_DATE > '2010-03-20 00:00:01' or
		(EXPIRY_DATE is null and MONTHS_CREDIT != 0)
		*/
		select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
		max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
				CASE when l.[EXPIRY_DATE] is not null 
				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then l.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end as 'EXPIRY_DATE', 
		c.MONTHS_CREDIT, c.CREATOR_ID,
		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
		from Reviewer.dbo.TB_CONTACT c
		left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
		on c.CONTACT_ID = lt.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
		where c.EXPIRY_DATE > '2010-03-20 00:00:01' or
		(c.EXPIRY_DATE is null and MONTHS_CREDIT != 0)
		
		group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		
	end
	else
	begin
		/*
		SELECT * FROM Reviewer.dbo.tb_CONTACT
		*/
		
		select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
		max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
				CASE when l.[EXPIRY_DATE] is not null 
				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then l.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end as 'EXPIRY_DATE', 
		c.MONTHS_CREDIT, c.CREATOR_ID,
		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
		from Reviewer.dbo.TB_CONTACT c
		left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
		on c.CONTACT_ID = lt.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
			group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		
	end
       
END


GO
/****** Object:  StoredProcedure [dbo].[st_ContactDetailsGetAllFilter]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:                            <Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:   <Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactDetailsGetAllFilter] 
(
                @ER4AccountsOnly bit,
                @TEXT_BOX nvarchar(255)
)
AS
BEGIN
                -- SET NOCOUNT ON added to prevent extra result sets from
                -- interfering with SELECT statements.
                SET NOCOUNT ON;

                if @ER4AccountsOnly = 1
                begin        
                                select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
                                max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
                                                                CASE when l.[EXPIRY_DATE] is not null 
                                                                and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
                                                                                then l.[EXPIRY_DATE]
                                                                else c.[EXPIRY_DATE]
                                                                end as 'EXPIRY_DATE', 
                                c.MONTHS_CREDIT, c.CREATOR_ID,
                                c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
                                l.SITE_LIC_ID, l.SITE_LIC_NAME
                                ,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
                                from Reviewer.dbo.TB_CONTACT c
                                left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
                                on c.CONTACT_ID = lt.CONTACT_ID
                                left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
                                left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
                                where (c.EXPIRY_DATE > '2010-03-20 00:00:01' or
                                (c.EXPIRY_DATE is null and MONTHS_CREDIT != 0))
                                
                                and ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
                                                (c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
                                                (c.EMAIL like '%' + @TEXT_BOX + '%'))
                                
                                group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
                                c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
                                c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
                                
                end
                else
                begin
                                select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
                                max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
                                                                CASE when l.[EXPIRY_DATE] is not null 
                                                                and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
                                                                                then l.[EXPIRY_DATE]
                                                                else c.[EXPIRY_DATE]
                                                                end as 'EXPIRY_DATE', 
                                c.MONTHS_CREDIT, c.CREATOR_ID,
                                c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
                                l.SITE_LIC_ID, l.SITE_LIC_NAME
                                ,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
                                from Reviewer.dbo.TB_CONTACT c
                                left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
                                on c.CONTACT_ID = lt.CONTACT_ID
                                left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
                                left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
                                where ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
                                                                (c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
                                                                (c.EMAIL like '%' + @TEXT_BOX + '%'))
                                
                                group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
                                c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
                                c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
                                
                end
       
END




GO
/****** Object:  StoredProcedure [dbo].[st_ContactDetailsGetAllFilter_1]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactDetailsGetAllFilter_1] 
(
	@ER4AccountsOnly bit,
	@TEXT_BOX nvarchar(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 
	if @ER4AccountsOnly = 1
	begin        
		select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
		max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
				CASE when l.[EXPIRY_DATE] is not null 
				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then l.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end as 'EXPIRY_DATE', 
		c.MONTHS_CREDIT, c.CREATOR_ID,
		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
		from Reviewer.dbo.TB_CONTACT c
		left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
		on c.CONTACT_ID = lt.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
		
		--where (c.EXPIRY_DATE > '2010-03-20 00:00:01' or
		--(c.EXPIRY_DATE is null and MONTHS_CREDIT != 0))
		
		where (c.EXPIRY_DATE > '2010-03-20 00:00:01' or
		(c.EXPIRY_DATE is null /*and MONTHS_CREDIT != 0*/))
		
		and ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
			(c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
			(c.EMAIL like '%' + @TEXT_BOX + '%'))
		
		group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		
	end
	else
	begin
		select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
		max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
				CASE when l.[EXPIRY_DATE] is not null 
				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then l.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end as 'EXPIRY_DATE', 
		c.MONTHS_CREDIT, c.CREATOR_ID,
		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
		from Reviewer.dbo.TB_CONTACT c
		left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
		on c.CONTACT_ID = lt.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
		where ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
				(c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
				(c.EMAIL like '%' + @TEXT_BOX + '%'))
		
		group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		
	end
       
END



GO
/****** Object:  StoredProcedure [dbo].[st_ContactDetailsGetAllFilter_2]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_ContactDetailsGetAllFilter_2] 
(
	@ER4AccountsOnly bit,
	@TEXT_BOX nvarchar(255),
	@SiteLicence nvarchar(10)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 
	if @ER4AccountsOnly = 1
	begin        		
		if @SiteLicence != '0'
		begin			
			select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL from Reviewer.dbo.TB_CONTACT c
              inner join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID and lc.SITE_LIC_ID = @SiteLicence
              where ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
				(c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
				(c.EMAIL like '%' + @TEXT_BOX + '%'))		
			group by c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL	
		end
		else
		begin	
			select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
			max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
					CASE when l.[EXPIRY_DATE] is not null 
					and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
						then l.[EXPIRY_DATE]
					else c.[EXPIRY_DATE]
					end as 'EXPIRY_DATE', 
			c.MONTHS_CREDIT, c.CREATOR_ID,
			c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
			l.SITE_LIC_ID, l.SITE_LIC_NAME
			,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
			from Reviewer.dbo.TB_CONTACT c
			left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
			on c.CONTACT_ID = lt.CONTACT_ID
			left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
			left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
			
			--where (c.EXPIRY_DATE > '2010-03-20 00:00:01' or
			--(c.EXPIRY_DATE is null and MONTHS_CREDIT != 0))
			
			where (c.EXPIRY_DATE > '2010-03-20 00:00:01' or
			(c.EXPIRY_DATE is null /*and MONTHS_CREDIT != 0*/))
			
			and ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
				(c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
				(c.USERNAME like '%' + @TEXT_BOX + '%') OR
				(c.EMAIL like '%' + @TEXT_BOX + '%'))
			
			group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
			c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
			c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME	
		end
	end
	else
	begin
		select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
		max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
				CASE when l.[EXPIRY_DATE] is not null 
				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then l.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end as 'EXPIRY_DATE', 
		c.MONTHS_CREDIT, c.CREATOR_ID,
		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
		from Reviewer.dbo.TB_CONTACT c
		left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
		on c.CONTACT_ID = lt.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
		where ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
				(c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
				(c.USERNAME like '%' + @TEXT_BOX + '%') OR
				(c.EMAIL like '%' + @TEXT_BOX + '%'))
		
		group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
		c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		
	end
       
END

GO
/****** Object:  StoredProcedure [dbo].[st_ContactEdit]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_ContactEdit]
(
	@CONTACT_NAME nvarchar(255),
	@USERNAME nvarchar(50),
	@EMAIL nvarchar(500),
	@PASSWORD varchar(50),
	@CONTACT_ID int
)

As

SET NOCOUNT ON

	if @PASSWORD = ''
	begin
		update Reviewer.dbo.TB_CONTACT 
		set CONTACT_NAME = @CONTACT_NAME,
		USERNAME = @USERNAME,
		EMAIL = @EMAIL
		where CONTACT_ID = @CONTACT_ID
    end
	else
	begin
		--create salt!
		DECLARE @chars char(100) = '!т#$%&а()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~ийклм'
		declare @rnd varchar(20)
		declare @cnt int = 0
		set @rnd = ''
		WHILE (@cnt <= 20) 
		BEGIN
			SELECT @rnd = @rnd + 
				SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
			SELECT @cnt = @cnt + 1
		END
		update Reviewer.dbo.TB_CONTACT 
		set CONTACT_NAME = @CONTACT_NAME
			, USERNAME = @USERNAME
			, EMAIL = @EMAIL
			, FLAVOUR = @rnd
			, PWASHED = HASHBYTES('SHA1', @PASSWORD + @rnd)
		where CONTACT_ID = @CONTACT_ID
	end

SET NOCOUNT OFF

GO

/****** Object:  StoredProcedure [dbo].[st_ContactFullCreateOrEdit]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER procedure [dbo].[st_ContactFullCreateOrEdit]
(
	@NEW_ACCOUNT bit,
	@CONTACT_NAME nvarchar(255),
	@USERNAME nvarchar(50),
	@PASSWORD nvarchar(50),
	@DATE_CREATED datetime,
	@EXPIRY_DATE date,
	@CREATOR_ID int,
	@EMAIL nvarchar(500),
	@DESCRIPTION nvarchar(1000),
	@IS_SITE_ADMIN bit,
	@CONTACT_ID nvarchar(50), -- it might say 'New'
	@EXTENSION_TYPE_ID int,
	@EXTENSION_NOTES nvarchar(500),
	@EDITOR_ID int,
	@MONTHS_CREDIT nvarchar(50),
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON
DECLARE @ORIGINAL_EXPIRY datetime
DECLARE @NEW_ROW_ID int

if @EXPIRY_DATE = ''
begin
	SET @EXPIRY_DATE = NULL
end

BEGIN TRY

BEGIN TRANSACTION
	if @NEW_ACCOUNT = 1
	begin
		-- create a new account
		INSERT INTO Reviewer.dbo.TB_CONTACT(CONTACT_NAME, USERNAME, [PASSWORD], 
			DATE_CREATED, [EXPIRY_DATE], EMAIL, [DESCRIPTION], CREATOR_ID, MONTHS_CREDIT)
		VALUES (@CONTACT_NAME, @USERNAME, @PASSWORD, @DATE_CREATED, @EXPIRY_DATE,
			@EMAIL, @DESCRIPTION, @CREATOR_ID, @MONTHS_CREDIT)
	
		set @RESULT = @@IDENTITY
		if (@CREATOR_ID != @CONTACT_ID)
		begin
			update Reviewer.dbo.TB_CONTACT 
			set CREATOR_ID = @RESULT
			where CONTACT_ID = @RESULT
			and USERNAME = @USERNAME
		end
    end
	else  -- edit an existing account
	begin	
		-- get original expiry date from TB_CONTACT
		select @ORIGINAL_EXPIRY = c.EXPIRY_DATE 
		from Reviewer.dbo.TB_CONTACT c
		where c.CONTACT_ID = @CONTACT_ID
		
		--update TB_CONTACT
		update Reviewer.dbo.TB_CONTACT 
		set 
		CONTACT_NAME = @CONTACT_NAME,
		USERNAME = @USERNAME,
		[PASSWORD] = @PASSWORD,
		DATE_CREATED = @DATE_CREATED,
		[EXPIRY_DATE] = @EXPIRY_DATE,
		CREATOR_ID = @CREATOR_ID,
		EMAIL = @EMAIL,
		[DESCRIPTION] = @DESCRIPTION,
		IS_SITE_ADMIN = @IS_SITE_ADMIN,
		MONTHS_CREDIT = @MONTHS_CREDIT
		where CONTACT_ID = @CONTACT_ID
		
		if (@EXTENSION_TYPE_ID > 1)
		begin
			-- create new row in TB_EXPIRY_EDIT_LOG -- using bogus old expiry date
			insert into TB_EXPIRY_EDIT_LOG
			(DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
			OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
			values (GETDATE(), 1, @CONTACT_ID, @EXPIRY_DATE, @EXPIRY_DATE,
			@EDITOR_ID, @EXTENSION_TYPE_ID, @EXTENSION_NOTES)
			set @NEW_ROW_ID = @@IDENTITY
			
			--  update TB_EXPIRY_EDIT_LOG
			update TB_EXPIRY_EDIT_LOG
			set OLD_EXPIRY_DATE = @ORIGINAL_EXPIRY
			where EXPIRY_EDIT_ID = @NEW_ROW_ID
	
			set @RESULT = 'Valid'		 
		end
	end
	
	
	
COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Invalid'
end
END CATCH

RETURN

SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[st_ContactIsSiteLicenseAdm]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactIsSiteLicenseAdm] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select * from TB_SITE_LIC_ADMIN
	where CONTACT_ID = @CONTACT_ID

	RETURN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

END

GO
/****** Object:  StoredProcedure [dbo].[st_ContactLogin]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/10>
-- Description:	<gets contact details when loging in>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactLogin] 
(
	@USERNAME varchar(50),
	@PASSWORD varchar(50),
	@IP_ADDRESS nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
--first check if the username/pw are correct

	SELECT c.*, COUNT(sla.CONTACT_ID) as IsSLA, COUNT(o_a.CONTACT_ID) as IsOA
	FROM Reviewer.dbo.TB_CONTACT c
	Left outer join TB_SITE_LIC_ADMIN sla on sla.CONTACT_ID = c.CONTACT_ID
	Left outer join ReviewerAdmin.dbo.TB_ORGANISATION_ADMIN o_a on o_a.CONTACT_ID = c.CONTACT_ID
	where c.USERNAME = @USERNAME
	and HASHBYTES('SHA1', @Password + FLAVOUR) = PWASHED
	and EXPIRY_DATE is not null
	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME,
	c.PASSWORD, c.LAST_LOGIN, c.DATE_CREATED, c.EMAIL, c.EXPIRY_DATE,
	c.MONTHS_CREDIT, c.CREATOR_ID,c.TYPE, c.IS_SITE_ADMIN, c.DESCRIPTION, c.SEND_NEWSLETTER, c.FLAVOUR, c.PWASHED,
	c.ARCHIE_ACCESS_TOKEN, c.ARCHIE_ID, c.ARCHIE_REFRESH_TOKEN, c.ARCHIE_TOKEN_VALID_UNTIL, c.LAST_ARCHIE_CODE, c.LAST_ARCHIE_STATE
	
	RETURN
END



GO
/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedAccounts]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactPurchasedAccounts] 
(
	@CREATOR_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, 
		c.DATE_CREATED, c.[EXPIRY_DATE], c.MONTHS_CREDIT, c.CREATOR_ID, l.SITE_LIC_ID, l.SITE_LIC_NAME
		from Reviewer.dbo.TB_CONTACT c
		left outer join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID
		left outer join Reviewer.dbo.TB_SITE_LIC l on lc.SITE_LIC_ID = l.SITE_LIC_ID
		where c.CREATOR_ID = @CREATOR_ID
		or c.CONTACT_ID = @CREATOR_ID
		group by c.CONTACT_ID, c.CONTACT_NAME,
		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID, l.SITE_LIC_ID, l.SITE_LIC_NAME
		
	UNION
	select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, 
		c.DATE_CREATED, c.[EXPIRY_DATE], c.MONTHS_CREDIT, c.CREATOR_ID, l.SITE_LIC_ID, l.SITE_LIC_NAME
		from Reviewer.dbo.TB_CONTACT c
		inner join TB_BILL b on b.PURCHASER_CONTACT_ID = @CREATOR_ID
		inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID = c.CONTACT_ID
		inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Professional'
		left outer join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID
		left outer join Reviewer.dbo.TB_SITE_LIC l on lc.SITE_LIC_ID = l.SITE_LIC_ID
		group by c.CONTACT_ID, c.CONTACT_NAME,
		c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID, l.SITE_LIC_ID, l.SITE_LIC_NAME
	
	order by c.CONTACT_NAME

	RETURN

END


GO
/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedDetails]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactPurchasedDetails] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
        declare @PURCHASED_ACCOUNTS table ( 
	CONTACT_ID int, 
	CONTACT_NAME nchar(1000),
	EMAIL nvarchar(500),
	DATE_CREATED datetime,
	[EXPIRY_DATE] date,
	MONTHS_CREDIT smallint,
	CREATOR_ID int, 
	LAST_LOGIN datetime,
	SITE_LIC_ID int null,
	SITE_LIC_NAME nvarchar(50) null,
	FLAVOUR char(20) null,
	IS_FULLY_ACTIVE bit null,
	IS_STALE_AGHOST bit null)

	Insert Into @PURCHASED_ACCOUNTS (CONTACT_ID, CONTACT_NAME, EMAIL, DATE_CREATED, [EXPIRY_DATE],
					MONTHS_CREDIT, CREATOR_ID, SITE_LIC_ID, SITE_LIC_NAME, FLAVOUR, IS_FULLY_ACTIVE, IS_STALE_AGHOST)
	SELECT c.[CONTACT_ID], [CONTACT_NAME], EMAIL, c.[DATE_CREATED],
		 CASE when l.[EXPIRY_DATE] is not null 
				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
					then l.[EXPIRY_DATE]
				else c.[EXPIRY_DATE]
				end as 'EXPIRY_DATE',
		  [MONTHS_CREDIT], c.[CREATOR_ID], l.SITE_LIC_ID, l.SITE_LIC_NAME
		  ,FLAVOUR, 0, null
	  FROM [Reviewer].[dbo].[TB_CONTACT] c 
	  left outer join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID
	  left outer join Reviewer.dbo.TB_SITE_LIC l on lc.SITE_LIC_ID = l.SITE_LIC_ID
	  where c.CREATOR_ID = @CONTACT_ID
	  and ((c.EXPIRY_DATE is null and MONTHS_CREDIT != 0)
			or
			(c.EXPIRY_DATE is not null))
	and c.CONTACT_ID != @CONTACT_ID

	update  p_a --add usage info
	set p_a.LAST_LOGIN = l_t.CREATED
	--max(l_t.CREATED)
	from @PURCHASED_ACCOUNTS p_a, TB_LOGON_TICKET l_t
	where l_t.CREATED in
	(select max(l_t.CREATED)from TB_LOGON_TICKET l_t
	where p_a.CONTACT_ID = l_t.CONTACT_ID)
	
	--set whether the account is fully active (the person who purchased it won't be able to edit the account details)
	update t set IS_FULLY_ACTIVE = 1
	from @PURCHASED_ACCOUNTS t where EMAIL is not null 
			and CONTACT_NAME is not null and [EXPIRY_DATE] is not null and FLAVOUR is not null
	
	--we need to figure out if ghost accounts awaiting for activation are out of time and need re-activation
	declare @DATES table (CONTACT_ID int, MDATE Datetime2(1))
			
	insert into @DATES
		SELECT t.CONTACT_ID,  MAX(c.DATE_CREATED)
	from @PURCHASED_ACCOUNTS t inner join TB_CHECKLINK c on t.CONTACT_ID = c.CONTACT_ID
											and c.TYPE = 'ActivateGhost'
											and t.IS_FULLY_ACTIVE = 0
		group by t.CONTACT_ID, c.TYPE, t.IS_FULLY_ACTIVE
	update t set IS_STALE_AGHOST = 1
	from @PURCHASED_ACCOUNTS t inner join @DATES d on t.CONTACT_ID = d.CONTACT_ID and DATEDIFF(D, d.MDATE, GETDATE()) > 14
	
	--mark ghost account that are waiting for the end user to activate them but are still in time to do so
	update t set IS_STALE_AGHOST = 0
	from @PURCHASED_ACCOUNTS t where t.CONTACT_NAME is null and t.FLAVOUR is null and IS_STALE_AGHOST is null and EMAIL is not null
	--email field is populated to send the activation request to the intended user
	
	select CONTACT_ID , 
	CONTACT_NAME ,
	EMAIL ,
	DATE_CREATED ,
	[EXPIRY_DATE] ,
	MONTHS_CREDIT ,
	CREATOR_ID , 
	LAST_LOGIN ,
	SITE_LIC_ID ,
	SITE_LIC_NAME  ,
	IS_FULLY_ACTIVE ,
	IS_STALE_AGHOST 
	 from @PURCHASED_ACCOUNTS
    

	RETURN

END

GO
/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedReviews]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactPurchasedReviews] 
(
	@FUNDER_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
select r.REVIEW_ID, r.REVIEW_NAME, r.FUNDER_ID, 
     r.DATE_CREATED, 
		CASE when l.[EXPIRY_DATE] is not null 
				and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
					then l.[EXPIRY_DATE]
				else r.[EXPIRY_DATE]
				end as 'EXPIRY_DATE'
     , r.MONTHS_CREDIT
     , l.SITE_LIC_ID, l.SITE_LIC_NAME
	from Reviewer.dbo.TB_REVIEW r
	left outer join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID
	left outer join Reviewer.dbo.TB_SITE_LIC l on lr.SITE_LIC_ID = l.SITE_LIC_ID
	where r.FUNDER_ID = @FUNDER_ID
	--and r.EXPIRY_DATE is not null
	
	group by r.REVIEW_ID, r.REVIEW_NAME,
	r.DATE_CREATED, r.[EXPIRY_DATE], r.MONTHS_CREDIT, r.FUNDER_ID, l.SITE_LIC_ID, l.[EXPIRY_DATE], l.SITE_LIC_NAME
	UNION
	
	select r.REVIEW_ID, r.REVIEW_NAME, r.FUNDER_ID, 
     r.DATE_CREATED, 
		CASE when l.[EXPIRY_DATE] is not null 
				and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
					then l.[EXPIRY_DATE]
				else r.[EXPIRY_DATE]
				end as 'EXPIRY_DATE'
     , r.MONTHS_CREDIT
    , l.SITE_LIC_ID, l.SITE_LIC_NAME   
	from Reviewer.dbo.TB_REVIEW r
	inner join Reviewer.dbo.TB_REVIEW_CONTACT rc
	on rc.REVIEW_ID = r.REVIEW_ID and rc.CONTACT_ID = @FUNDER_ID
	left outer join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID
	left outer join Reviewer.dbo.TB_SITE_LIC l on lr.SITE_LIC_ID = l.SITE_LIC_ID
	
	UNION
	select r.REVIEW_ID, r.REVIEW_NAME, r.FUNDER_ID, 
     r.DATE_CREATED,
		CASE when l.[EXPIRY_DATE] is not null 
				and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
					then l.[EXPIRY_DATE]
				else r.[EXPIRY_DATE]
				end as 'EXPIRY_DATE'
     , r.MONTHS_CREDIT
     , l.SITE_LIC_ID, l.SITE_LIC_NAME  
	from Reviewer.dbo.TB_REVIEW r
	inner join TB_BILL b on b.PURCHASER_CONTACT_ID = @FUNDER_ID
	inner join TB_BILL_LINE bl on bl.BILL_ID = b.BILL_ID and bl.AFFECTED_ID = r.REVIEW_ID
	inner join TB_FOR_SALE fs on fs.FOR_SALE_ID = bl.FOR_SALE_ID and fs.TYPE_NAME ='Shareable Review'
	left outer join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID
	left outer join Reviewer.dbo.TB_SITE_LIC l on lr.SITE_LIC_ID = l.SITE_LIC_ID

	group by r.REVIEW_ID, r.REVIEW_NAME,
	r.DATE_CREATED, r.[EXPIRY_DATE], r.MONTHS_CREDIT, r.FUNDER_ID, l.SITE_LIC_ID, l.[EXPIRY_DATE], l.SITE_LIC_NAME
	
	order by r.REVIEW_NAME

	RETURN

END


GO
/****** Object:  StoredProcedure [dbo].[st_ContactReviewDeleteRoles]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactReviewDeleteRoles] 
(
	@REVIEW_ID int,
	@CONTACT_ID int,
	@REVIEW_CONTACT_ID int output
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select @REVIEW_CONTACT_ID = REVIEW_CONTACT_ID 
    from Reviewer.dbo.TB_REVIEW_CONTACT
    where REVIEW_ID = @REVIEW_ID
    and CONTACT_ID = @CONTACT_ID
    
    if @@ROWCOUNT > 0
    begin
		delete from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE
		where REVIEW_CONTACT_ID = @REVIEW_CONTACT_ID
    end
   

	RETURN

END


GO
/****** Object:  StoredProcedure [dbo].[st_ContactReviewRole]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactReviewRole] 
(
	@CONTACT_ID int,
	@REVIEW_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select c_r_r.ROLE_NAME  
	from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE c_r_r
	inner join Reviewer.dbo.TB_REVIEW_CONTACT r_c on r_c.REVIEW_CONTACT_ID = c_r_r.REVIEW_CONTACT_ID
	where r_c.CONTACT_ID = @CONTACT_ID
	and r_c.REVIEW_ID = @REVIEW_ID

	RETURN

END

GO
/****** Object:  StoredProcedure [dbo].[st_ContactReviews]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jeff>
-- Create date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactReviews] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
    select r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED,
	SUM(DATEDIFF(HOUR,t.CREATED ,t.LAST_RENEWED )) as HOURS 
	from Reviewer.dbo.TB_REVIEW r
	inner join Reviewer.dbo.TB_REVIEW_CONTACT r_c on r_c.REVIEW_ID = r.REVIEW_ID
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r_c.CONTACT_ID
	left outer join ReviewerAdmin.dbo.[TB_LOGON_TICKET] t on t.REVIEW_ID = r_c.REVIEW_ID and t.CONTACT_ID = r_c.CONTACT_ID
	where r_c.CONTACT_ID = @CONTACT_ID
	group by r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED


END

GO
/****** Object:  StoredProcedure [dbo].[st_ContactReviewsAdmin]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_ContactReviewsAdmin] 
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
		or (r.EXPIRY_DATE is not null)
		or (r.EXPIRY_DATE is null)) 
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
    
END


GO
/****** Object:  StoredProcedure [dbo].[st_ContactReviewsArchie]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactReviewsArchie] 
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
	r.MONTHS_CREDIT, r.ARCHIE_ID
	from Reviewer.dbo.TB_REVIEW r
	inner join Reviewer.dbo.TB_REVIEW_CONTACT rc
	on r.REVIEW_ID = rc.REVIEW_ID
	left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
	on lt.REVIEW_ID = rc.REVIEW_ID
	where rc.CONTACT_ID = @CONTACT_ID
	-- this next line restricts it to just the contactID. Remove this line and you get the last login overall (all reviewers)
	and (lt.CONTACT_ID = @CONTACT_ID or lt.CONTACT_ID is null)
	-- Archie reviews do not have an expiry date
	and r.EXPIRY_DATE is null
	-- and have something in the ArchieID field
	and r.ARCHIE_ID is not null
	
	group by r.REVIEW_ID, r.REVIEW_NAME, 
	r.DATE_CREATED, r.MONTHS_CREDIT, r.ARCHIE_ID
	order by r.REVIEW_NAME
	

END


GO
/****** Object:  StoredProcedure [dbo].[st_ContactReviewsArchieFull]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactReviewsArchieFull] 
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
	r.MONTHS_CREDIT, r.ARCHIE_ID
	from Reviewer.dbo.TB_REVIEW r
	inner join Reviewer.dbo.TB_REVIEW_CONTACT rc
	on r.REVIEW_ID = rc.REVIEW_ID
	left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
	on lt.REVIEW_ID = rc.REVIEW_ID
	where rc.CONTACT_ID = @CONTACT_ID
	-- this next line restricts it to just the contactID. Remove this line and you get the last login overall (all reviewers)
	-- and (lt.CONTACT_ID = @CONTACT_ID or lt.CONTACT_ID is null)
	-- Archie reviews do not have an expiry date
	and r.EXPIRY_DATE is null
	-- and have something in the ArchieID field
	and r.ARCHIE_ID is not null
	--and we don't want the prospective reviews
	and r.ARCHIE_ID != 'prospective_______'
	
	group by r.REVIEW_ID, r.REVIEW_NAME, 
	r.DATE_CREATED, r.MONTHS_CREDIT, r.ARCHIE_ID
	order by r.REVIEW_NAME
	

END


GO
/****** Object:  StoredProcedure [dbo].[st_ContactReviewsArchieProspective]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactReviewsArchieProspective] 
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
	r.MONTHS_CREDIT, r.ARCHIE_ID
	from Reviewer.dbo.TB_REVIEW r
	inner join Reviewer.dbo.TB_REVIEW_CONTACT rc
	on r.REVIEW_ID = rc.REVIEW_ID
	left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
	on lt.REVIEW_ID = rc.REVIEW_ID
	where rc.CONTACT_ID = @CONTACT_ID
	-- this next line restricts it to just the contactID. Remove this line and you get the last login overall (all reviewers)
	-- and (lt.CONTACT_ID = @CONTACT_ID or lt.CONTACT_ID is null)
	-- Archie reviews do not have an expiry date
	and r.EXPIRY_DATE is null
	-- and have something in the ArchieID field
	and r.ARCHIE_ID is not null
	--and we just want the prospective reviews
	and r.ARCHIE_ID = 'prospective_______'
	
	group by r.REVIEW_ID, r.REVIEW_NAME, 
	r.DATE_CREATED, r.MONTHS_CREDIT, r.ARCHIE_ID
	order by r.REVIEW_NAME
	

END


GO
/****** Object:  StoredProcedure [dbo].[st_ContactReviewsFilter]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:                            <Jeff>
-- Create date: <24/03/2010>
-- Description:   <gets the review data based on contact>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactReviewsFilter] 
(
                @CONTACT_ID nvarchar(50),
                @TEXT_BOX nvarchar(255)
)
AS
BEGIN
                -- SET NOCOUNT ON added to prevent extra result sets from
                -- interfering with SELECT statements.
                SET NOCOUNT ON;
                
    -- Insert statements for procedure here
    select r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED,
                SUM(DATEDIFF(HOUR,t.CREATED ,t.LAST_RENEWED )) as HOURS 
                from Reviewer.dbo.TB_REVIEW r
                inner join Reviewer.dbo.TB_REVIEW_CONTACT r_c on r_c.REVIEW_ID = r.REVIEW_ID
                inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r_c.CONTACT_ID
                left outer join ReviewerAdmin.dbo.[TB_LOGON_TICKET] t on t.REVIEW_ID = r_c.REVIEW_ID and t.CONTACT_ID = r_c.CONTACT_ID
                where r_c.CONTACT_ID = @CONTACT_ID
                
                and ((r.REVIEW_ID like '%' + @TEXT_BOX + '%') OR
                                                                (r.REVIEW_NAME like '%' + @TEXT_BOX + '%'))
                
                group by r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED


END



GO
/****** Object:  StoredProcedure [dbo].[st_ContactReviewsNonShareable]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_ContactReviewsNonShareable] 
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
/****** Object:  StoredProcedure [dbo].[st_ContactReviewsOtherShareable]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_ContactReviewsOtherShareable] 
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
/****** Object:  StoredProcedure [dbo].[st_ContactReviewsShareable]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_ContactReviewsShareable] 
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
/****** Object:  StoredProcedure [dbo].[st_ContactTable]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<gets contact table for a contactID>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactTable] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT * from Reviewer.dbo.TB_CONTACT
		WHERE Reviewer.dbo.TB_CONTACT.CONTACT_ID = @CONTACT_ID	
	RETURN
END
GO
/****** Object:  StoredProcedure [dbo].[st_ContactTableByEmail]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<gets contact table for a contactID>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactTableByEmail] 
(
	@EMAIL nvarchar(500)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT * from Reviewer.dbo.TB_CONTACT
		WHERE Reviewer.dbo.TB_CONTACT.EMAIL = @EMAIL	
	RETURN
END
GO
/****** Object:  StoredProcedure [dbo].[st_ContactTableByName]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<gets contact table for a contactID>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactTableByName] 
(
	@USERNAME nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT * from Reviewer.dbo.TB_CONTACT
		WHERE Reviewer.dbo.TB_CONTACT.USERNAME = @USERNAME	
	RETURN
END
GO
/****** Object:  StoredProcedure [dbo].[st_ContactUndoCreate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactUndoCreate]
	-- Add the parameters for the stored procedure here
	@CID int
	,@USERNAME nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Delete from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CID and [USERNAME] = @USERNAME
END

GO
/****** Object:  StoredProcedure [dbo].[st_CopyCodeset]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER procedure [dbo].[st_CopyCodeset]
(
	@SOURCE_SET_ID int,
	@SOURCE_REVIEW_ID int,
	@DESTINATION_REVIEW_ID int,
	@CONTACT_ID int,
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON


declare @DESTINATION_SET_ID int
declare @DESTINATION_REVIEW_SET_ID int
declare @DESTINATION_SET_NAME nvarchar (255)
set @RESULT = '0'



BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION

--01 create new row in Reviewer.dbo.TB_SET and get @DESTINATION_SET_ID
	insert into Reviewer.dbo.TB_SET (SET_TYPE_ID, SET_NAME)
	select s.SET_TYPE_ID, s.SET_NAME from Reviewer.dbo.TB_SET s
	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
	where s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID
	set @DESTINATION_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

	-- prefix the new codeset with 'COPY - ' BUT first remove
	--  the 'LINKED - ' prefix if it exists
	select @DESTINATION_SET_NAME = SET_NAME
	from Reviewer.dbo.TB_SET
	where SET_ID = @DESTINATION_SET_ID

	declare @Prefix nvarchar(6) 
	set @Prefix = substring (@DESTINATION_SET_NAME, 0, 7) 
	if @Prefix = 'LINKED'
	begin
		set @DESTINATION_SET_NAME = substring (@DESTINATION_SET_NAME, 9, len(@DESTINATION_SET_NAME) - 8)
	end
	
	update Reviewer.dbo.TB_SET
	set SET_NAME = 'COPY - ' + @DESTINATION_SET_NAME
	where SET_ID = @DESTINATION_SET_ID

--02 create new row in Reviewer.dbo.TB_REVIEW_SET and get @DESTINATION_REVIEW_SET_ID
	insert into Reviewer.dbo.TB_REVIEW_SET (REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL)
	values (@DESTINATION_REVIEW_ID, @DESTINATION_SET_ID, 1, 1)
	set @DESTINATION_REVIEW_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

	-- @attribute_id
	declare @attribute_id table (attribute_id int, ok int)

--03 get source ATTRIBUTE_ID's and put in tmp table @attribute_id (avoids orphans)	
	insert into @attribute_id 
	Select a_s.ATTRIBUTE_ID, Reviewer.dbo.fn_IsAttributeInTree(a_s.ATTRIBUTE_ID ) as ok 
	from Reviewer.dbo.TB_ATTRIBUTE_SET a_s 
	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
	where a_s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID 
	
	--A
	--select * from @attribute_id
	
	-- @tb_attribute
	declare @tb_attribute table 
	(ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))
	
--04 use @attribute_id to get source data from TB_ATTRIBUTE and put in tmp table @tb_attribute
	insert into @tb_attribute (ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
		select ATTRIBUTE_ID, @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, ATTRIBUTE_ID -- use ATTRIBUTE_ID as OLD_ATTRIBUTE_ID
		from Reviewer.dbo.TB_ATTRIBUTE
		where ATTRIBUTE_ID in 
		(select attribute_id from @attribute_id)

	--B
	--select * from @tb_attribute

	-- @tb_attribute_set
	declare @tb_attribute_set table 
	(ATTRIBUTE_SET_ID int, ATTRIBUTE_ID int, SET_ID int, PARENT_ATTRIBUTE_ID int, ATTRIBUTE_TYPE_ID int, 
	ATTRIBUTE_SET_DESC nvarchar(max), ATTRIBUTE_ORDER int)

--05 use @attribute_id to get source data from TB_ATTRIBUTE_SET and put in tmp table @tb_attribute_set	
	insert into @tb_attribute_set (ATTRIBUTE_SET_ID, ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)
		select ATTRIBUTE_SET_ID, ATTRIBUTE_ID, @DESTINATION_SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
		from Reviewer.dbo.TB_ATTRIBUTE_SET
		where ATTRIBUTE_ID in 
		(select attribute_id from @attribute_id)
		-- 20/09/2012 JB added to avoid any orphan set_id's that are not in Reviewer.dbo.TB_REVIEW_SET
		and SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET)

	--C
	--select * from @tb_attribute_set

--06 put @tb_attribute into Reviewer.dbo.TB_ATTRIBUTE
	insert into Reviewer.dbo.TB_ATTRIBUTE (CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)	
		select @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID 
		from @tb_attribute

	-- @new_tb_attribute
	declare @new_tb_attribute table 
	(NEW_ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))

--07 get the new rows from Reviewer.dbo.TB_ATTRIBUTE and put them into @new_tb_attribute
	insert into @new_tb_attribute (NEW_ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
		select ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID  
		from Reviewer.dbo.TB_ATTRIBUTE
		where OLD_ATTRIBUTE_ID in -- old_attribute_id will identify the items we want
		(select attribute_id from @attribute_id) --wrong! you are getting attributes from other set_id's
		and OLD_ATTRIBUTE_ID not like 'AT%'
		and OLD_ATTRIBUTE_ID not like 'EX%'
		and OLD_ATTRIBUTE_ID not like 'IC%'

	--D
	--select * from @new_tb_attribute


--09 update @tb_attribute_set with the new ATTRIBUTE_IDs sitting in @new_tb_attribute
	update @tb_attribute_set 
	set ATTRIBUTE_ID = 
	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
	where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
	where exists
	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
		where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
	
	
	--E
	--select * from @tb_attribute_set


--08 update @tb_attribute_set with a new PARENT_ATTRIBUTE_ID
	-- the new PARENT_ATTRIBUTE_ID is the 
	update @tb_attribute_set
	set PARENT_ATTRIBUTE_ID = 
	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
	where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID
	and PARENT_ATTRIBUTE_ID != 0)
	where exists
	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
		where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID and PARENT_ATTRIBUTE_ID != 0)
	
	
	-- clean up the nulls
	update @tb_attribute_set
	set PARENT_ATTRIBUTE_ID = 0
	where PARENT_ATTRIBUTE_ID is null
	
	--F
	--select * from @tb_attribute_set
	


--10 put @tb_attribute_set into Reviewer.dbo.TB_ATTRIBUTE_SET
	insert into Reviewer.dbo.TB_ATTRIBUTE_SET (ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)	
		select ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
		from @tb_attribute_set

--11 place the new codeset at the bottom of the list.
	declare @number_sets int set @number_sets = 0
	select @number_sets = COUNT(*) from [Reviewer].[dbo].[TB_REVIEW_SET]
	where REVIEW_ID = @DESTINATION_REVIEW_ID
	-- set the position
	update [Reviewer].[dbo].[TB_REVIEW_SET] 
	set SET_ORDER = @number_sets - 1
	where REVIEW_ID = @DESTINATION_REVIEW_ID
	and SET_ID = @DESTINATION_SET_ID

--12 Clear out OLD_ATTRIBUTE_ID from TB_ATTRIBUTE in the new codeset since
	update Reviewer.dbo.TB_ATTRIBUTE
	set OLD_ATTRIBUTE_ID = null
	where ATTRIBUTE_ID in 
	(select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
		where SET_ID = @DESTINATION_SET_ID)
	

COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to copy. Please contact EPPI-Support'
end
END CATCH


RETURN

SET NOCOUNT OFF








GO
/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep01]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







CREATE OR ALTER procedure [dbo].[st_CopyReviewStep01]
(
	@CONTACT_ID int,
	@NEW_REVIEW_ID int output,
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON


BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION


declare @tb_review_name table (review_name1 nvarchar(255))
declare @REVIEW_NAME nvarchar(1000) 
declare @REVIEW_CONTACT_ID int
set @RESULT = '0'

insert into @tb_review_name (review_name1)select CONTACT_NAME from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID
update @tb_review_name set review_name1 = review_name1 + '''s example non-shareable review'
set @REVIEW_NAME = (select review_name1 from @tb_review_name)


--01 create new row in Reviewer.dbo.TB_REVIEW and get @NEW_REVIEW_ID
	insert into Reviewer.dbo.TB_REVIEW (REVIEW_NAME, DATE_CREATED, FUNDER_ID)
	values (@REVIEW_NAME, getdate(), @CONTACT_ID)
	set @NEW_REVIEW_ID = CAST(SCOPE_IDENTITY() AS INT)

--02 create new row in Reviewer.dbo.TB_REVIEW_CONTACT
	insert into Reviewer.dbo.TB_REVIEW_CONTACT (REVIEW_ID, CONTACT_ID)
	values (@NEW_REVIEW_ID, @CONTACT_ID)
	set @REVIEW_CONTACT_ID = CAST(SCOPE_IDENTITY() AS INT)
	
--03 create new row in Reviewer.dbo.TB_CONTACT_REVIEW_ROLE
	insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
	values (@REVIEW_CONTACT_ID, 'AdminUser')
	--values ('abc', 'AdminUser')


COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to create the new review'
end
END CATCH


RETURN

SET NOCOUNT OFF










GO
/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep03]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO








CREATE OR ALTER procedure [dbo].[st_CopyReviewStep03]
(
	@CONTACT_ID int,
	@SOURCE_REVIEW_ID int,
	@NEW_REVIEW_ID int,
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON


declare @NEW_ITEM_ID int
declare @REVIEW_CONTACT_ID int
declare @NEW_SOURCE_ID int
set @RESULT = '0'

declare @tb_item table (new_item_id int, [type_id] int, 
TITLE nvarchar(4000), PARENT_TITLE nvarchar(4000), SHORT_TITLE nvarchar(70),
DATE_CREATED datetime, CREATED_BY nvarchar(50), DATE_EDITED datetime,
EDITED_BY nvarchar(50), YEAR nchar(4), MONTH varchar(10), STANDARD_NUMBER nvarchar(255),
CITY nvarchar(100), COUNTRY nvarchar(100), PUBLISHER nvarchar(1000), INSTITUTION nvarchar(1000),
VOLUME nvarchar(56), PAGES nvarchar(50), EDITION nvarchar(200), ISSUE nvarchar(100),
IS_LOCAL bit, AVAILABILITY nvarchar(250), URL nvarchar(500), MASTER_ITEM_ID bigint,
source_item_id int, ABSTRACT nvarchar(max), COMMENTS nvarchar(max), DOI nvarchar(500), KEYWORDS nvarchar(max))

declare @tb_item_review table (new_item_id bigint, old_item_id bigint, is_included bit,
new_master_item_id bigint, old_master_item_id bigint, is_deleted bit)

declare @tb_item_author table (new_item_author_id bigint, new_item_id bigint, old_item_id bigint,
LAST nvarchar(50), FIRST nvarchar(50), SECOND nvarchar(50), ROLE tinyint, RANK smallint)

declare @tb_source table (old_source_id int, new_source_id int, source_name nvarchar(255),
new_review_id int, is_deleted bit, date_of_search date, date_of_import date, source_database nvarchar(200),
search_description nvarchar(4000), search_string nvarchar(max), notes nvarchar(4000), import_filter_id int)

declare @tb_item_source table (old_item_source_id int, new_item_source_id int, old_item_id int, 
new_item_id int, original_source_id int, dest_source_id int)


BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION


--01 get example review ID
	--select @SOURCE_REVIEW_ID = EXAMPLE_NON_SHAREABLE_REVIEW_ID from TB_MANAGEMENT_SETTINGS
	
--02 get the item_id's to copy from Reviewer.dbo.TB_ITEM_REVIEW
	insert into @tb_item ([type_id], source_item_id)
	Select [TYPE_ID], i.ITEM_ID from Reviewer.dbo.TB_ITEM i
	inner join Reviewer.dbo.TB_ITEM_REVIEW i_r on i_r.ITEM_ID = i.ITEM_ID
	where i_r.REVIEW_ID = @SOURCE_REVIEW_ID

--03 user cursors to place new row in TB_ITEM, get the new ITEM_ID and update @tb_item
	declare @WORKING_ITEM_ID int
	declare ITEM_ID_CURSOR cursor for
	select source_item_id FROM @tb_item
	open ITEM_ID_CURSOR
	fetch next from ITEM_ID_CURSOR
	into @WORKING_ITEM_ID
	while @@FETCH_STATUS = 0
	begin
		insert into Reviewer.dbo.TB_ITEM ([TYPE_ID], OLD_ITEM_ID)	
		SELECT [type_id], source_item_id FROM @tb_item
		WHERE source_item_id = @WORKING_ITEM_ID
		set @NEW_ITEM_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_item set new_item_id = @NEW_ITEM_ID
		WHERE source_item_id = @WORKING_ITEM_ID

		FETCH NEXT FROM ITEM_ID_CURSOR 
		INTO @WORKING_ITEM_ID
	END

	CLOSE ITEM_ID_CURSOR
	DEALLOCATE ITEM_ID_CURSOR

--04 insert into TB_ITEM_REVIEW based on @item_id
-- need to find correct bit for IS_DELETED and fill in MASTER_ITEM_ID
	insert into @tb_item_review (old_item_id, is_included, old_master_item_id, is_deleted)
	Select ITEM_ID, IS_INCLUDED, MASTER_ITEM_ID, IS_DELETED 
	from Reviewer.dbo.TB_ITEM_REVIEW 
	where REVIEW_ID = @SOURCE_REVIEW_ID

	update t1
	set t1.new_item_id = t2.new_item_id
	from @tb_item_review t1 inner join @tb_item t2
	on t2.source_item_id = t1.old_item_id
	
	update t1
	set t1.new_master_item_id = t2.new_item_id
	from @tb_item_review t1 inner join @tb_item t2
	on t2.source_item_id = t1.old_master_item_id

	insert into Reviewer.dbo.TB_ITEM_REVIEW (ITEM_ID, REVIEW_ID, IS_INCLUDED, MASTER_ITEM_ID, IS_DELETED)
	select new_item_id, @NEW_REVIEW_ID, is_included, new_master_item_id, is_deleted
	from @tb_item_review


--05 insert into @tb_item_author and TB_ITEM_AUTHOR based on @item_id
	insert into @tb_item_author (old_item_id, LAST, FIRST, SECOND, ROLE, RANK)
	select ITEM_ID, LAST, FIRST, SECOND, ROLE, RANK from Reviewer.dbo.TB_ITEM_AUTHOR 
	where ITEM_ID in 
	 (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @SOURCE_REVIEW_ID)

	update @tb_item_author
	set new_item_id = i.new_item_id
	from @tb_item i
	where old_item_id = i.source_item_id

	insert into Reviewer.dbo.TB_ITEM_AUTHOR (ITEM_ID, LAST, FIRST, SECOND, ROLE, RANK)
	select new_item_id, LAST, FIRST, SECOND, ROLE, RANK from @tb_item_author

	
--06 update missing info in TB_ITEM
	update @tb_item
	set TITLE = i.TITLE, PARENT_TITLE = i.PARENT_TITLE, SHORT_TITLE = i.SHORT_TITLE,
	 DATE_CREATED = i.DATE_CREATED, CREATED_BY = i.CREATED_BY, DATE_EDITED = i.DATE_EDITED, 
	 EDITED_BY = i.EDITED_BY, YEAR = i.YEAR, MONTH = i.MONTH, STANDARD_NUMBER = i.STANDARD_NUMBER,
	 CITY = i.CITY, COUNTRY = i.COUNTRY, PUBLISHER = i.PUBLISHER, INSTITUTION = i.INSTITUTION, 
	 VOLUME = i.VOLUME, PAGES = i.PAGES, EDITION = i.EDITION, ISSUE = i.ISSUE, IS_LOCAL = i.IS_LOCAL, 
	 AVAILABILITY = i.AVAILABILITY, URL = i.URL, MASTER_ITEM_ID = i.MASTER_ITEM_ID, 
	 ABSTRACT = i.ABSTRACT, COMMENTS = i.COMMENTS, DOI = i.DOI, KEYWORDS = i.KEYWORDS
	from Reviewer.dbo.TB_ITEM i
	where source_item_id = i.ITEM_ID
	
	update Reviewer.dbo.TB_ITEM
	set TITLE = tb_i.TITLE, PARENT_TITLE = tb_i.PARENT_TITLE, SHORT_TITLE = tb_i.SHORT_TITLE,
	 DATE_CREATED = tb_i.DATE_CREATED, CREATED_BY = tb_i.CREATED_BY, DATE_EDITED = tb_i.DATE_EDITED, 
	 EDITED_BY = tb_i.EDITED_BY, YEAR = tb_i.YEAR, MONTH = tb_i.MONTH, STANDARD_NUMBER = tb_i.STANDARD_NUMBER,
	 CITY = tb_i.CITY, COUNTRY = tb_i.COUNTRY, PUBLISHER = tb_i.PUBLISHER, INSTITUTION = tb_i.INSTITUTION, 
	 VOLUME = tb_i.VOLUME, PAGES = tb_i.PAGES, EDITION = tb_i.EDITION, ISSUE = tb_i.ISSUE, IS_LOCAL = tb_i.IS_LOCAL, 
	 AVAILABILITY = tb_i.AVAILABILITY, URL = tb_i.URL, MASTER_ITEM_ID = tb_i.MASTER_ITEM_ID, 
	 ABSTRACT = tb_i.ABSTRACT, COMMENTS = tb_i.COMMENTS, DOI = tb_i.DOI, KEYWORDS = tb_i.KEYWORDS
	from @tb_item tb_i
	where new_item_id = ITEM_ID

--07 collect the old source data
	insert into @tb_source (old_source_id, source_name, new_review_id, is_deleted,
	date_of_search, date_of_import, source_database, search_description, 
	search_string, notes, import_filter_id)
	select SOURCE_ID, SOURCE_NAME, @NEW_REVIEW_ID, IS_DELETED, DATE_OF_SEARCH, DATE_OF_IMPORT,
	SOURCE_DATABASE, SEARCH_DESCRIPTION, SEARCH_STRING, NOTES, IMPORT_FILTER_ID
	from Reviewer.dbo.TB_SOURCE s
	where s.REVIEW_ID = @SOURCE_REVIEW_ID


--08 create new rows in TB_SOURCE
	declare @WORKING_SOURCE_ID int
	declare SOURCE_ID_CURSOR cursor for
	select old_source_id FROM @tb_source
	open SOURCE_ID_CURSOR
	fetch next from SOURCE_ID_CURSOR
	into @WORKING_SOURCE_ID
	while @@FETCH_STATUS = 0
	begin
		insert into Reviewer.dbo.TB_SOURCE (SOURCE_NAME, REVIEW_ID, 
			IS_DELETED, DATE_OF_SEARCH, DATE_OF_IMPORT, SOURCE_DATABASE, 
			SEARCH_DESCRIPTION, SEARCH_STRING, NOTES, IMPORT_FILTER_ID)	
		SELECT source_name, new_review_id, is_deleted,
			date_of_search, date_of_import, source_database, search_description, 
			search_string, notes, import_filter_id FROM @tb_source
			WHERE old_source_id = @WORKING_SOURCE_ID
		set @NEW_SOURCE_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_source set new_source_id = @NEW_SOURCE_ID
		WHERE old_source_id = @WORKING_SOURCE_ID

		FETCH NEXT FROM SOURCE_ID_CURSOR 
		INTO @WORKING_SOURCE_ID
	END

	CLOSE SOURCE_ID_CURSOR
	DEALLOCATE SOURCE_ID_CURSOR

	--select * from @tb_source

--09 collect the old item_source data
	insert into @tb_item_source (old_item_source_id, old_item_id, original_source_id)
	select ITEM_SOURCE_ID, ITEM_ID, SOURCE_ID
	from Reviewer.dbo.TB_ITEM_SOURCE i_s
	where i_s.SOURCE_ID in (select old_source_id from @tb_source)
	
	--select * from @tb_item_source

--10 update @tb_item_source with the new_source_id and new_item_id
	update t1
	set dest_source_id = t2.new_source_id
	from @tb_item_source t1 inner join @tb_source t2
	on t2.old_source_id = t1.original_source_id
	
	--select * from @tb_item_source
	
	update t1
	set new_item_id = t2.ITEM_ID
	from @tb_item_source t1 inner join Reviewer.dbo.TB_ITEM t2
	on t2.OLD_ITEM_ID = t1.old_item_id
	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
	where i_r.REVIEW_ID = @NEW_REVIEW_ID)
		
	--select * from @tb_item_source
	
--11 place new rows in TB_ITEM_SOURCE based on tb_item_source
	insert into Reviewer.dbo.TB_ITEM_SOURCE (ITEM_ID, SOURCE_ID)
	select new_item_id, dest_source_id from @tb_item_source


COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to copy items and place items in new review'
end
END CATCH


RETURN

SET NOCOUNT OFF











GO
/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep05]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO









CREATE OR ALTER procedure [dbo].[st_CopyReviewStep05]
(
	@CONTACT_ID int,
	@SOURCE_REVIEW_ID int,
	@NEW_REVIEW_ID int,
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON


declare @NEW_ITEM_DUPLICATE_GROUP_ID int
declare @NEW_GROUP_MEMBER_ID int
set @RESULT = '0'

-- 1. create new row in TB_ITEM_DUPLICATE_GROUP to create a new ITEM_DUPLICATE_GROUP_ID
-- 2. create new row in TB_ITEM_DUPLICATE_GROUP_MEMBERS to create a new GROUP_MEMBER_ID
-- 3. use GROUP_MEMBER_ID as MASTER_MEMBER_ID back in TB_ITEM_DUPLICATE_GROUP

declare @tb_item_duplicate_group table (new_item_duplicate_group_id int, old_item_duplicate_group_id int,
new_master_member_id int, old_master_member_id int, review_id int, new_original_item_id bigint,
old_original_item_id bigint)

declare @tb_item_duplicate_group_members table (new_group_member_id int, old_group_member_id int,
new_item_duplicate_group_id int, old_item_duplicate_group_id int, new_item_review_id bigint,
old_item_review_id bigint, new_item_id bigint, old_item_id bigint, score float, is_checked bit, is_duplicate bit)


BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION

--01 collect the data for @tb_item_duplicate_group
	insert into @tb_item_duplicate_group (old_item_duplicate_group_id, old_master_member_id,
	review_id, old_original_item_id)
	select i_d_g.ITEM_DUPLICATE_GROUP_ID, i_d_g.MASTER_MEMBER_ID, @NEW_REVIEW_ID,
	i_d_g.ORIGINAL_ITEM_ID from Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP i_d_g
	where i_d_g.REVIEW_ID = @SOURCE_REVIEW_ID

	update t1
	set t1.new_original_item_id = t2.ITEM_ID
	from @tb_item_duplicate_group t1 inner join Reviewer.dbo.TB_ITEM t2
	on t2.OLD_ITEM_ID = t1.old_original_item_id
	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
	where i_r.REVIEW_ID = @NEW_REVIEW_ID)
	
--02 collect the data for @tb_item_duplicate_group_members
	insert into @tb_item_duplicate_group_members (old_group_member_id, old_item_duplicate_group_id,
	old_item_review_id, score, is_checked, is_duplicate)
	select i_d_g_m.GROUP_MEMBER_ID, i_d_g_m.ITEM_DUPLICATE_GROUP_ID, i_d_g_m.ITEM_REVIEW_ID,
	i_d_g_m.SCORE, i_d_g_m.IS_CHECKED, i_d_g_m.IS_DUPLICATE from Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP_MEMBERS i_d_g_m
	where i_d_g_m.ITEM_DUPLICATE_GROUP_ID in (select old_item_duplicate_group_id from @tb_item_duplicate_group)
	
	update t1
	set t1.old_item_id = t2.ITEM_ID
	from @tb_item_duplicate_group_members t1 inner join Reviewer.dbo.TB_ITEM_REVIEW t2
	on t1.old_item_review_id = t2.ITEM_REVIEW_ID
	
	update t1
	set t1.new_item_id = t2.ITEM_ID
	from @tb_item_duplicate_group_members t1 inner join Reviewer.dbo.TB_ITEM t2
	on t2.OLD_ITEM_ID = t1.old_item_id
	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
	where i_r.REVIEW_ID = @NEW_REVIEW_ID)
	
	update t1
	set new_item_review_id = t2.ITEM_REVIEW_ID
	from @tb_item_duplicate_group_members t1 inner join Reviewer.dbo.TB_ITEM_REVIEW t2
	on t2.ITEM_ID = t1.new_item_id
	where t2.REVIEW_ID = @NEW_REVIEW_ID
	
--03 create new rows in Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP
	declare @WORKING_ITEM_DUPLICATE_GROUP_ID int
	declare ITEM_DUPLICATE_GROUP_ID_CURSOR cursor for
	select old_item_duplicate_group_id FROM @tb_item_duplicate_group
	open ITEM_DUPLICATE_GROUP_ID_CURSOR
	fetch next from ITEM_DUPLICATE_GROUP_ID_CURSOR
	into @WORKING_ITEM_DUPLICATE_GROUP_ID
	while @@FETCH_STATUS = 0
	begin
		insert into Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP (REVIEW_ID, ORIGINAL_ITEM_ID)	
		SELECT @NEW_REVIEW_ID, old_original_item_id FROM @tb_item_duplicate_group
		WHERE old_item_duplicate_group_id = @WORKING_ITEM_DUPLICATE_GROUP_ID
		set @NEW_ITEM_DUPLICATE_GROUP_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_item_duplicate_group
		set new_item_duplicate_group_id = @NEW_ITEM_DUPLICATE_GROUP_ID
		where old_item_duplicate_group_id = @WORKING_ITEM_DUPLICATE_GROUP_ID
		
		update @tb_item_duplicate_group_members 
		set new_item_duplicate_group_id = @NEW_ITEM_DUPLICATE_GROUP_ID
		WHERE old_item_duplicate_group_id = @WORKING_ITEM_DUPLICATE_GROUP_ID

		FETCH NEXT FROM ITEM_DUPLICATE_GROUP_ID_CURSOR 
		INTO @WORKING_ITEM_DUPLICATE_GROUP_ID
	END

	CLOSE ITEM_DUPLICATE_GROUP_ID_CURSOR
	DEALLOCATE ITEM_DUPLICATE_GROUP_ID_CURSOR

	
--04 create new rows in Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP_MEMBERS
	declare @WORKING_GROUP_MEMBER_ID int
	declare GROUP_MEMBER_ID_CURSOR cursor for
	select old_group_member_id FROM @tb_item_duplicate_group_members
	open GROUP_MEMBER_ID_CURSOR
	fetch next from GROUP_MEMBER_ID_CURSOR
	into @WORKING_GROUP_MEMBER_ID
	while @@FETCH_STATUS = 0
	begin
		insert into Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP_MEMBERS (ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID,
		SCORE, IS_CHECKED, IS_DUPLICATE)	
		SELECT new_item_duplicate_group_id, new_item_review_id, score, is_checked, is_duplicate
		FROM @tb_item_duplicate_group_members
		WHERE old_group_member_id = @WORKING_GROUP_MEMBER_ID
		set @NEW_GROUP_MEMBER_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_item_duplicate_group_members 
		set new_group_member_id = @NEW_GROUP_MEMBER_ID
		where old_group_member_id = @WORKING_GROUP_MEMBER_ID
		
		FETCH NEXT FROM GROUP_MEMBER_ID_CURSOR 
		INTO @WORKING_GROUP_MEMBER_ID
	END

	CLOSE GROUP_MEMBER_ID_CURSOR
	DEALLOCATE GROUP_MEMBER_ID_CURSOR
	

	update t1
	set t1.new_master_member_id = t2.new_group_member_id
	from @tb_item_duplicate_group t1 inner join @tb_item_duplicate_group_members t2
	on t1.new_item_duplicate_group_id = t2.new_item_duplicate_group_id
	where t2.is_duplicate = 0


--05 update Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP with the new MASTER_MEMBER_ID	
	update t1
	set t1.MASTER_MEMBER_ID = t2.new_master_member_id
	from Reviewer.dbo.TB_ITEM_DUPLICATE_GROUP t1 inner join @tb_item_duplicate_group t2
	on t1.ITEM_DUPLICATE_GROUP_ID = t2.new_item_duplicate_group_id
	
	

COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to copy the duplicate information'
end
END CATCH


RETURN

SET NOCOUNT OFF












GO
/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep07]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<gets contact table for a contactID>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_CopyReviewStep07] 
(
	@SOURCE_REVIEW_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
--01 get the set_ids	
    select * 
	from Reviewer.dbo.TB_SET s
	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
	where r_s.REVIEW_ID = @SOURCE_REVIEW_ID

    	
	RETURN
END


GO
/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep09]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO









CREATE OR ALTER procedure [dbo].[st_CopyReviewStep09]
(	
	@CONTACT_ID int,
	@SOURCE_REVIEW_ID int,
	@DESTINATION_REVIEW_ID int,
	@SOURCE_SET_ID int,
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON


declare @DESTINATION_SET_ID int
declare @DESTINATION_REVIEW_SET_ID int
declare @DESTINATION_SET_NAME nvarchar (255)
--declare @SOURCE_REVIEW_ID int
set @RESULT = '0'


BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION


--00 get example review ID
	--select @SOURCE_REVIEW_ID = EXAMPLE_NON_SHAREABLE_REVIEW_ID from TB_MANAGEMENT_SETTINGS

--01 create new row in Reviewer.dbo.TB_SET and get @DESTINATION_SET_ID
	insert into Reviewer.dbo.TB_SET (SET_TYPE_ID, SET_NAME, OLD_GUIDELINE_ID)
	select s.SET_TYPE_ID, s.SET_NAME, @SOURCE_SET_ID from Reviewer.dbo.TB_SET s
	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
	where s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID
	set @DESTINATION_SET_ID = CAST(SCOPE_IDENTITY() AS INT)



--02 create new row in Reviewer.dbo.TB_REVIEW_SET and get @DESTINATION_REVIEW_SET_ID
	insert into Reviewer.dbo.TB_REVIEW_SET (REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL)
	values (@DESTINATION_REVIEW_ID, @DESTINATION_SET_ID, 1, 1)
	set @DESTINATION_REVIEW_SET_ID = CAST(SCOPE_IDENTITY() AS INT)

	-- @attribute_id
	declare @attribute_id table (attribute_id int, ok int)

--03 get source ATTRIBUTE_ID's and put in tmp table @attribute_id (avoids orphans)	
	insert into @attribute_id 
	Select a_s.ATTRIBUTE_ID, Reviewer.dbo.fn_IsAttributeInTree(a_s.ATTRIBUTE_ID ) as ok 
	from Reviewer.dbo.TB_ATTRIBUTE_SET a_s 
	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
	where a_s.SET_ID = @SOURCE_SET_ID
	and r_s.REVIEW_ID = @SOURCE_REVIEW_ID 
	
	--A
	--select * from @attribute_id
	
	-- @tb_attribute
	declare @tb_attribute table 
	(ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))
	
--04 use @attribute_id to get source data from TB_ATTRIBUTE and put in tmp table @tb_attribute
	insert into @tb_attribute (ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
		select ATTRIBUTE_ID, @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, ATTRIBUTE_ID -- use ATTRIBUTE_ID as OLD_ATTRIBUTE_ID
		from Reviewer.dbo.TB_ATTRIBUTE
		where ATTRIBUTE_ID in 
		(select attribute_id from @attribute_id)

	--B
	--select * from @tb_attribute

	-- @tb_attribute_set
	declare @tb_attribute_set table 
	(ATTRIBUTE_SET_ID int, ATTRIBUTE_ID int, SET_ID int, PARENT_ATTRIBUTE_ID int, ATTRIBUTE_TYPE_ID int, 
	ATTRIBUTE_SET_DESC nvarchar(max), ATTRIBUTE_ORDER int)

--05 use @attribute_id to get source data from TB_ATTRIBUTE_SET and put in tmp table @tb_attribute_set	
	insert into @tb_attribute_set (ATTRIBUTE_SET_ID, ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)
		select ATTRIBUTE_SET_ID, ATTRIBUTE_ID, @DESTINATION_SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
		from Reviewer.dbo.TB_ATTRIBUTE_SET
		where ATTRIBUTE_ID in 
		(select attribute_id from @attribute_id)
		-- 20/09/2012 JB added to avoid any orphan set_id's that are not in Reviewer.dbo.TB_REVIEW_SET
		and SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET)

	--C
	--select * from @tb_attribute_set

--06 put @tb_attribute into Reviewer.dbo.TB_ATTRIBUTE
	insert into Reviewer.dbo.TB_ATTRIBUTE (CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)	
		select @CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID 
		from @tb_attribute

	-- @new_tb_attribute
	declare @new_tb_attribute table 
	(NEW_ATTRIBUTE_ID int, CONTACT_ID int, ATTRIBUTE_NAME nvarchar(255), ATTRIBUTE_DESC nvarchar(2000), OLD_ATTRIBUTE_ID nvarchar(50))

--07 get the new rows from Reviewer.dbo.TB_ATTRIBUTE and put them into @new_tb_attribute
	insert into @new_tb_attribute (NEW_ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID)
		select ATTRIBUTE_ID, CONTACT_ID, ATTRIBUTE_NAME, ATTRIBUTE_DESC, OLD_ATTRIBUTE_ID  
		from Reviewer.dbo.TB_ATTRIBUTE
		where OLD_ATTRIBUTE_ID in -- old_attribute_id will identify the items we want
		(select attribute_id from @attribute_id) --does @attribute_id have bad data?
		and OLD_ATTRIBUTE_ID not like 'AT%'
		and OLD_ATTRIBUTE_ID not like 'EX%'
		and OLD_ATTRIBUTE_ID not like 'IC%'
		--and ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
		--where SET_ID = @DESTINATION_SET_ID)
		-- added by JB 21/04/15 to be sure we are only getting data for this review
		and CONTACT_ID = @CONTACT_ID
		
--- we need to restrict this to the correct set but we haven't populated
--- Reviewer.dbo.TB_ATTRIBUTE_SET yet!!!!!!!!!!!!!!!!!		
		

	--D
	--select * from @new_tb_attribute


--09 update @tb_attribute_set with the new ATTRIBUTE_IDs sitting in @new_tb_attribute
	update @tb_attribute_set 
	set ATTRIBUTE_ID = 
	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
	where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
	where exists
	(select n_tb_a.NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
		where n_tb_a.OLD_ATTRIBUTE_ID = ATTRIBUTE_ID)
	
	
	--E
	--select * from @tb_attribute_set


--08 update @tb_attribute_set with a new PARENT_ATTRIBUTE_ID
	-- the new PARENT_ATTRIBUTE_ID is the 
	update @tb_attribute_set
	set PARENT_ATTRIBUTE_ID = 
	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
	where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID
	and PARENT_ATTRIBUTE_ID != 0)
	where exists
	(select NEW_ATTRIBUTE_ID from @new_tb_attribute n_tb_a
		where n_tb_a.OLD_ATTRIBUTE_ID = PARENT_ATTRIBUTE_ID and PARENT_ATTRIBUTE_ID != 0)
	
	
	-- clean up the nulls
	update @tb_attribute_set
	set PARENT_ATTRIBUTE_ID = 0
	where PARENT_ATTRIBUTE_ID is null
	
	--F
	--select * from @tb_attribute_set
	


--10 put @tb_attribute_set into Reviewer.dbo.TB_ATTRIBUTE_SET
	insert into Reviewer.dbo.TB_ATTRIBUTE_SET (ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER)	
		select ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID, ATTRIBUTE_SET_DESC, ATTRIBUTE_ORDER
		from @tb_attribute_set

--11 place the new codeset at the bottom of the list.
	declare @number_sets int set @number_sets = 0
	select @number_sets = COUNT(*) from [Reviewer].[dbo].[TB_REVIEW_SET]
	where REVIEW_ID = @DESTINATION_REVIEW_ID
	-- set the position
	update [Reviewer].[dbo].[TB_REVIEW_SET] 
	set SET_ORDER = @number_sets - 1
	where REVIEW_ID = @DESTINATION_REVIEW_ID
	and SET_ID = @DESTINATION_SET_ID


-- keep the old_attribute_id values as we need to copy over the data
/*
--12 Clear out OLD_ATTRIBUTE_ID from TB_ATTRIBUTE in the new codeset since
	update Reviewer.dbo.TB_ATTRIBUTE
	set OLD_ATTRIBUTE_ID = null
	where ATTRIBUTE_ID in 
	(select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
		where SET_ID = @DESTINATION_SET_ID)
*/


COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to set up copies of the source codesets in the new review'
end
END CATCH


RETURN

SET NOCOUNT OFF












GO
/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep11]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_CopyReviewStep11]
(	
	@CONTACT_ID int,
	@SOURCE_REVIEW_ID int,
	@DESTINATION_REVIEW_ID int,
	@SOURCE_SET_ID int,
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON


declare @DESTINATION_SET_ID int
declare @DESTINATION_REVIEW_SET_ID int
declare @DESTINATION_SET_NAME nvarchar (255)
declare @NEW_OUTCOME_ID int
--declare @SOURCE_REVIEW_ID int
declare @NEW_SET_ID int
declare @NEW_ITEM_SET_ID int
set @RESULT = '0'


declare @tb_item_attribute table
(NEW_ITEM_ATTRIBUTE_ID bigint,  OLD_ITEM_ATTRIBUTE_ID bigint,
 NEW_ITEM_ID bigint,  OLD_ITEM_ID bigint,  NEW_ITEM_SET_ID bigint,
 OLD_ITEM_SET_ID bigint,  NEW_ATTRIBUTE_ID bigint,
 ORIGINAL_ATTRIBUTE_ID nvarchar(50), -- needs to be nvarchar to deal with ER3 attributes
 ADDITIONAL_TEXT nvarchar(max))
 
declare @tb_item_set table
(NEW_ITEM_SET_ID bigint,  EXAMPLE_ITEM_SET_ID bigint,
 NEW_ITEM_ID bigint,  EXAMPLE_ITEM_ID bigint,
 EXAMPLE_IS_COMPLETED bit,  NEW_CONTACT_ID int,
 EXAMPLE_IS_LOCKED bit)
 
declare @tb_new_items table
 (ITEM_ID bigint,
 SOURCE_ITEM_ID bigint)
 
declare @tb_item_outcome table
(new_outcome_id int,  old_outcome_id int,
 new_item_set_id int,  old_item_set_id int,
 outcome_type_id int,
 new_item_attribute_id_intervention bigint,  old_item_attribute_id_intervention bigint,
 new_item_attribute_id_control bigint,  old_item_attribute_id_control bigint,
 new_item_attribute_id_outcome bigint,  old_item_attribute_id_outcome nvarchar(255),
 outcome_title nvarchar(255),
 data1 float, data2 float, data3 float, data4 float,
 data5 float, data6 float, data7 float, data8 float,
 data9 float, data10 float, data11 float, data12 float,
 data13 float, data14 float,
 outcome_description nvarchar(4000))

declare @tb_item_outcome_attribute table
(new_item_outcome_attribute_id int,  old_item_outcome_attribute_id int,
 new_outcome_id int,  old_outcome_id int,
 new_attribute_id bigint,  old_attribute_id bigint,
 additional_text nvarchar(max))


BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION


--01 get a few variables
	--select @SOURCE_REVIEW_ID = EXAMPLE_NON_SHAREABLE_REVIEW_ID from TB_MANAGEMENT_SETTINGS
	select @NEW_SET_ID = SET_ID from Reviewer.dbo.TB_SET 
	where OLD_GUIDELINE_ID = @SOURCE_SET_ID
	and SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET where REVIEW_ID = @DESTINATION_REVIEW_ID)

--02 get old data from Reviewer.dbo.TB_ITEM_SET and place in @tb_item_set
	insert into @tb_item_set (EXAMPLE_ITEM_SET_ID, EXAMPLE_ITEM_ID, EXAMPLE_IS_COMPLETED, NEW_CONTACT_ID, EXAMPLE_IS_LOCKED)
	select ITEM_SET_ID, ITEM_ID, IS_COMPLETED, CONTACT_ID, IS_LOCKED 
	from Reviewer.dbo.TB_ITEM_SET 
	where SET_ID = @SOURCE_SET_ID
	
	--select * from @tb_item_set -- 1 OK
	
--03 sets up a list of the new items so we don't get cross-review contamination
	-- seems like an extra step but has little overhead and makes it easier
	-- for me to follow
	insert into @tb_new_items (ITEM_ID, SOURCE_ITEM_ID)
	select ITEM_ID, OLD_ITEM_ID from Reviewer.dbo.TB_ITEM where ITEM_ID in 
	(select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
	--select * from @tb_new_items -- 2 OK
	
--04 update @tb_item_set with the new item_id  
	update @tb_item_set
	set NEW_ITEM_ID = ITEM_ID
	from @tb_new_items
	where EXAMPLE_ITEM_ID = SOURCE_ITEM_ID
	
	--select * from @tb_item_set -- 3 OK

--05 get old data from Reviewer.dbo.TB_ITEM_ATTRIBUTE and place in @tb_item_attribute
	insert into @tb_item_attribute (OLD_ITEM_ATTRIBUTE_ID, OLD_ITEM_ID, OLD_ITEM_SET_ID,
	 ORIGINAL_ATTRIBUTE_ID, ADDITIONAL_TEXT)
	select i_a.ITEM_ATTRIBUTE_ID, i_a.ITEM_ID, i_a.ITEM_SET_ID, i_a.ATTRIBUTE_ID, i_a.ADDITIONAL_TEXT  
	from Reviewer.dbo.TB_ITEM_ATTRIBUTE i_a
	inner join Reviewer.dbo.TB_ITEM_SET i_s on i_s.ITEM_SET_ID = i_a.ITEM_SET_ID
	where i_s.SET_ID = @SOURCE_SET_ID
	
	--select * from @tb_item_attribute -- 4 OK

--06 user cursors to place new row in TB_ITEM_SET, get the new ITEM_SET_ID and update @tb_item_set
	declare @WORKING_ITEM_ID int
	declare ITEM_ID_CURSOR cursor for
	select NEW_ITEM_ID FROM @tb_item_set
	open ITEM_ID_CURSOR
	fetch next from ITEM_ID_CURSOR
	into @WORKING_ITEM_ID
	while @@FETCH_STATUS = 0
	begin
		-- set CONTACT_ID to newly created user account (rather than orginal coder)
		-- set IS_LOCKED to 0 as we want new user to experiment with the coding
		insert into Reviewer.dbo.TB_ITEM_SET (ITEM_ID, SET_ID, CONTACT_ID, IS_LOCKED)	
		SELECT NEW_ITEM_ID, @NEW_SET_ID, @CONTACT_ID, 0 FROM @tb_item_set 
		WHERE NEW_ITEM_ID = @WORKING_ITEM_ID
		set @NEW_ITEM_SET_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_item_set set new_item_set_id = @NEW_ITEM_SET_ID
		WHERE NEW_ITEM_ID = @WORKING_ITEM_ID

		FETCH NEXT FROM ITEM_ID_CURSOR 
		INTO @WORKING_ITEM_ID
	END

	CLOSE ITEM_ID_CURSOR
	DEALLOCATE ITEM_ID_CURSOR
	
	--select * from Reviewer.dbo.TB_ITEM_SET where ITEM_ID in -- 5 OK
	--(select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	--select * from @tb_item_set -- 6 OK

--07 update @tb_item_attribute with NEW_ITEM_ID, NEW_ITEM_SET_ID, NEW_ATTRIBUTE_ID
	--update @tb_item_attribute
	--set NEW_ITEM_ID = 
	--(select NEW_ITEM_ID from @tb_item_set
	--where EXAMPLE_ITEM_ID = OLD_ITEM_ID)
	
	update t1
	set NEW_ITEM_ID = t2.NEW_ITEM_ID
	from @tb_item_attribute t1 inner join @tb_item_set t2
	on t2.EXAMPLE_ITEM_ID = t1.OLD_ITEM_ID
	
	--select * from @tb_item_attribute -- 7 OK
	
	--update @tb_item_attribute
	--set NEW_ITEM_SET_ID = 
	--(select NEW_ITEM_SET_ID from @tb_item_set
	--where EXAMPLE_ITEM_SET_ID = OLD_ITEM_SET_ID)
	
	update t1
	set NEW_ITEM_SET_ID = t2.NEW_ITEM_SET_ID
	from @tb_item_attribute t1 inner join @tb_item_set t2
	on t2.EXAMPLE_ITEM_ID = t1.OLD_ITEM_ID
	
	--select * from @tb_item_attribute -- 8 OK
	
	--update @tb_item_attribute
	--set NEW_ATTRIBUTE_ID = 
	--(select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE a
	--where a.OLD_ATTRIBUTE_ID = ORIGINAL_ATTRIBUTE_ID
	---- added JB 22/04/15
	--and a.CONTACT_ID = @CONTACT_ID)

	declare @step_table table (A_ID bigint, O_A_ID nvarchar(50))
	insert into @step_table select a.ATTRIBUTE_ID, OLD_ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE a
	inner join Reviewer.dbo.TB_ATTRIBUTE_SET tas on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and tas.SET_ID = @NEW_SET_ID

	update t1
	set NEW_ATTRIBUTE_ID = t2.A_ID
	from @tb_item_attribute t1 inner join @step_table t2
	on t2.O_A_ID = t1.ORIGINAL_ATTRIBUTE_ID
	--where t2.CONTACT_ID = @CONTACT_ID

	--update t1
	--set NEW_ATTRIBUTE_ID = t2.ATTRIBUTE_ID
	--from @tb_item_attribute t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	--on t2.OLD_ATTRIBUTE_ID = t1.ORIGINAL_ATTRIBUTE_ID
	--where t2.CONTACT_ID = @CONTACT_ID
	
	--select * from @tb_item_attribute -- 9 OK

--08 place new rows in TB_ITEM_ATTRIBUTE based on @tb_item_attribute
	insert into Reviewer.dbo.TB_ITEM_ATTRIBUTE (ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT)
	select tb_i_a.NEW_ITEM_ID, tb_i_a.NEW_ITEM_SET_ID, tb_i_a.NEW_ATTRIBUTE_ID, tb_i_a.ADDITIONAL_TEXT 
	from @tb_item_attribute tb_i_a
	
--09 update TB_ITEM_SET with IS_COMPLETED values in @tb_item_set	
	UPDATE Reviewer.dbo.TB_ITEM_SET
	SET IS_COMPLETED = i_s.EXAMPLE_IS_COMPLETED
	FROM @tb_item_set i_s
	where i_s.NEW_ITEM_SET_ID = ITEM_SET_ID
	and i_s.NEW_ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW -- to restrict it to items in new review
	where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
	--select * from Reviewer.dbo.TB_ITEM_SET where ITEM_ID in -- 10 OK
	--(select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
--10 fill up @tb_item_outcome
	insert into @tb_item_outcome (old_outcome_id, old_item_set_id, outcome_type_id,
	 old_item_attribute_id_intervention, old_item_attribute_id_control,
	 old_item_attribute_id_outcome, outcome_title, data1, data2, data3, data4, data5,
	 data6, data7, data8, data9, data10, data11, data12, data13,
	 data14, outcome_description)
	select i_c.OUTCOME_ID, i_c.ITEM_SET_ID, i_c.OUTCOME_TYPE_ID, i_c.ITEM_ATTRIBUTE_ID_INTERVENTION,
	i_c.ITEM_ATTRIBUTE_ID_CONTROL, i_c.ITEM_ATTRIBUTE_ID_OUTCOME, i_c.OUTCOME_TITLE,
	i_c.DATA1, i_c.DATA2, i_c.DATA3, i_c.DATA4, i_c.DATA5, i_c.DATA6, i_c.DATA7,
	i_c.DATA8, i_c.DATA9, i_c.DATA10, i_c.DATA11, i_c.DATA12, i_c.DATA13, i_c.DATA14,
	i_c.OUTCOME_DESCRIPTION
	from Reviewer.dbo.TB_ITEM_OUTCOME i_c 
	inner join Reviewer.dbo.TB_ITEM_SET i_s on i_s.ITEM_SET_ID = i_c.ITEM_SET_ID
	where i_s.SET_ID = @SOURCE_SET_ID
	
--11 update @tb_item_outcome with the 'new_' values	
	update t1 --new_item_set_id
	set new_item_set_id = t2.NEW_ITEM_SET_ID
	from @tb_item_outcome t1 inner join @tb_item_set t2
	on t2.EXAMPLE_ITEM_SET_ID = t1.old_item_set_id
	
	update t1 --new_item_attribute_id_intervention
	set new_item_attribute_id_intervention = t2.ATTRIBUTE_ID
	from @tb_item_outcome t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_intervention
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
	update t1 --new_item_attribute_id_control
	set new_item_attribute_id_control = t2.ATTRIBUTE_ID
	from @tb_item_outcome t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_control
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
	update t1 --new_item_attribute_id_outcome
	set new_item_attribute_id_outcome = t2.ATTRIBUTE_ID
	from @tb_item_outcome t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_outcome
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
--12 copy @tb_item_outcome into Reviewer.dbo.TB_ITEM_OUTCOME
	declare @WORKING_OUTCOME_ID int
	declare OUTCOME_ID_CURSOR cursor for
	select old_outcome_id FROM @tb_item_outcome
	open OUTCOME_ID_CURSOR
	fetch next from OUTCOME_ID_CURSOR
	into @WORKING_OUTCOME_ID
	while @@FETCH_STATUS = 0
	begin
		insert into Reviewer.dbo.TB_ITEM_OUTCOME (ITEM_SET_ID, OUTCOME_TYPE_ID, 
		ITEM_ATTRIBUTE_ID_INTERVENTION, ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, 
		OUTCOME_TITLE, DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7,
		DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14, OUTCOME_DESCRIPTION)	
		SELECT new_item_set_id, outcome_type_id, new_item_attribute_id_intervention, 
		new_item_attribute_id_control, new_item_attribute_id_outcome, outcome_title, 
		data1, data2, data3, data4, data5, data6, data7, data8, data9, data10, data11, 
		data12, data13, data14, outcome_description 
		FROM @tb_item_outcome 
		WHERE old_outcome_id = @WORKING_OUTCOME_ID
		set @NEW_OUTCOME_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_item_outcome set new_outcome_id = @NEW_OUTCOME_ID
		WHERE old_outcome_id = @WORKING_OUTCOME_ID

		FETCH NEXT FROM OUTCOME_ID_CURSOR 
		INTO @WORKING_OUTCOME_ID
	END

	CLOSE OUTCOME_ID_CURSOR
	DEALLOCATE OUTCOME_ID_CURSOR

--13 fill up @tb_item_outcome_attribute
	insert into @tb_item_outcome_attribute (old_item_outcome_attribute_id,
	 old_outcome_id, old_attribute_id, additional_text)
	select ITEM_OUTCOME_ATTRIBUTE_ID, OUTCOME_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT
	from Reviewer.dbo.TB_ITEM_OUTCOME_ATTRIBUTE i_o_a
	inner join @tb_item_outcome i_o on i_o.OLD_OUTCOME_ID = i_o_a.OUTCOME_ID	

--14 update tb_item_outcome_attribute with the 'new_' values
	update t1 --new_outcome_id
	set new_outcome_id = t2.new_outcome_id
	from @tb_item_outcome_attribute t1 inner join @tb_item_outcome t2
	on t2.old_outcome_id = t1.old_outcome_id
	
	update t1 --new_attribute_id
	set new_attribute_id = t2.ATTRIBUTE_ID
	from @tb_item_outcome_attribute t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_attribute_id
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
--15 copy @tb_item_outcome_attribute into Reviewer.dbo.TB_ITEM_OUTCOME_ATTRIBUTE
	insert into Reviewer.dbo.TB_ITEM_OUTCOME_ATTRIBUTE (OUTCOME_ID, ATTRIBUTE_ID,
	ADDITIONAL_TEXT)
	select new_outcome_id, new_attribute_id, additional_text from @tb_item_outcome_attribute
	
--16 place the outcome_id values into ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA for later use
	insert into ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA (NEW_REVIEW_ID, OLD_OUTCOME_ID, NEW_OUTCOME_ID)
	select @DESTINATION_REVIEW_ID, old_outcome_id, new_outcome_id  
	from @tb_item_outcome
	
COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to copy the coding from the old review to the new review'
end
END CATCH


RETURN

SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep11Test]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO











CREATE OR ALTER procedure [dbo].[st_CopyReviewStep11Test]
(	
	@CONTACT_ID int,
	@SOURCE_REVIEW_ID int,
	@DESTINATION_REVIEW_ID int,
	@SOURCE_SET_ID int,
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON


declare @DESTINATION_SET_ID int
declare @DESTINATION_REVIEW_SET_ID int
declare @DESTINATION_SET_NAME nvarchar (255)
declare @NEW_OUTCOME_ID int
--declare @SOURCE_REVIEW_ID int
declare @NEW_SET_ID int
declare @NEW_ITEM_SET_ID int
set @RESULT = '0'


declare @tb_item_attribute table
(NEW_ITEM_ATTRIBUTE_ID bigint,  OLD_ITEM_ATTRIBUTE_ID bigint,
 NEW_ITEM_ID bigint,  OLD_ITEM_ID bigint,  NEW_ITEM_SET_ID bigint,
 OLD_ITEM_SET_ID bigint,  NEW_ATTRIBUTE_ID bigint,
 ORIGINAL_ATTRIBUTE_ID nvarchar(50), -- needs to be nvarchar to deal with ER3 attributes
 ADDITIONAL_TEXT nvarchar(max))
 
declare @tb_item_set table
(NEW_ITEM_SET_ID bigint,  EXAMPLE_ITEM_SET_ID bigint,
 NEW_ITEM_ID bigint,  EXAMPLE_ITEM_ID bigint,
 EXAMPLE_IS_COMPLETED bit,  NEW_CONTACT_ID int,
 EXAMPLE_IS_LOCKED bit)
 
declare @tb_new_items table
 (ITEM_ID bigint,
 SOURCE_ITEM_ID bigint)
 
declare @tb_item_outcome table
(new_outcome_id int,  old_outcome_id int,
 new_item_set_id int,  old_item_set_id int,
 outcome_type_id int,
 new_item_attribute_id_intervention bigint,  old_item_attribute_id_intervention bigint,
 new_item_attribute_id_control bigint,  old_item_attribute_id_control bigint,
 new_item_attribute_id_outcome bigint,  old_item_attribute_id_outcome nvarchar(255),
 outcome_title nvarchar(255),
 data1 float, data2 float, data3 float, data4 float,
 data5 float, data6 float, data7 float, data8 float,
 data9 float, data10 float, data11 float, data12 float,
 data13 float, data14 float,
 outcome_description nvarchar(4000))

declare @tb_item_outcome_attribute table
(new_item_outcome_attribute_id int,  old_item_outcome_attribute_id int,
 new_outcome_id int,  old_outcome_id int,
 new_attribute_id bigint,  old_attribute_id bigint,
 additional_text nvarchar(max))


BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION


--01 get a few variables
	--select @SOURCE_REVIEW_ID = EXAMPLE_NON_SHAREABLE_REVIEW_ID from TB_MANAGEMENT_SETTINGS
	select @NEW_SET_ID = SET_ID from Reviewer.dbo.TB_SET 
	where OLD_GUIDELINE_ID = @SOURCE_SET_ID
	and SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET where REVIEW_ID = @DESTINATION_REVIEW_ID)

--02 get old data from Reviewer.dbo.TB_ITEM_SET and place in @tb_item_set
	insert into @tb_item_set (EXAMPLE_ITEM_SET_ID, EXAMPLE_ITEM_ID, EXAMPLE_IS_COMPLETED, NEW_CONTACT_ID, EXAMPLE_IS_LOCKED)
	select ITEM_SET_ID, ITEM_ID, IS_COMPLETED, @CONTACT_ID, IS_LOCKED 
	from Reviewer.dbo.TB_ITEM_SET 
	where SET_ID = @SOURCE_SET_ID
	and IS_COMPLETED = 1 -- added to deal with double coded items (we only want the completed version)
	
	select * from @tb_item_set -- 1 OK
	print 'Step 1 OK'

	
--03 sets up a list of the new items so we don't get cross-review contamination
	-- seems like an extra step but has little overhead and makes it easier
	-- for me to follow
	insert into @tb_new_items (ITEM_ID, SOURCE_ITEM_ID)
	select ITEM_ID, OLD_ITEM_ID from Reviewer.dbo.TB_ITEM where ITEM_ID in 
	(select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
	select * from @tb_new_items -- 2 OK
	print 'Step 2 OK'
	
--04 update @tb_item_set with the new item_id  
	update @tb_item_set
	set NEW_ITEM_ID = ITEM_ID
	from @tb_new_items
	where EXAMPLE_ITEM_ID = SOURCE_ITEM_ID
	
	select * from @tb_item_set -- 3 OK
	print 'Step 3 OK'

--05 get old data from Reviewer.dbo.TB_ITEM_ATTRIBUTE and place in @tb_item_attribute
	insert into @tb_item_attribute (OLD_ITEM_ATTRIBUTE_ID, OLD_ITEM_ID, OLD_ITEM_SET_ID,
	 ORIGINAL_ATTRIBUTE_ID, ADDITIONAL_TEXT)
	select i_a.ITEM_ATTRIBUTE_ID, i_a.ITEM_ID, i_a.ITEM_SET_ID, i_a.ATTRIBUTE_ID, i_a.ADDITIONAL_TEXT  
	from Reviewer.dbo.TB_ITEM_ATTRIBUTE i_a
	inner join Reviewer.dbo.TB_ITEM_SET i_s on i_s.ITEM_SET_ID = i_a.ITEM_SET_ID
	where i_s.SET_ID = @SOURCE_SET_ID
	
	select * from @tb_item_attribute -- 4 OK
	print 'Step 4 OK'

--06 user cursors to place new row in TB_ITEM_SET, get the new ITEM_SET_ID and update @tb_item_set
	declare @WORKING_ITEM_ID int
	declare ITEM_ID_CURSOR cursor for
	select NEW_ITEM_ID FROM @tb_item_set
	open ITEM_ID_CURSOR
	fetch next from ITEM_ID_CURSOR
	into @WORKING_ITEM_ID
	while @@FETCH_STATUS = 0
	begin
		-- set CONTACT_ID to newly created user account (rather than orginal coder)
		-- set IS_LOCKED to 0 as we want new user to experiment with the coding
		print @WORKING_ITEM_ID
		insert into Reviewer.dbo.TB_ITEM_SET (ITEM_ID, SET_ID, CONTACT_ID, IS_LOCKED)	
		SELECT NEW_ITEM_ID, @NEW_SET_ID, @CONTACT_ID, EXAMPLE_IS_LOCKED FROM @tb_item_set 
		WHERE NEW_ITEM_ID = @WORKING_ITEM_ID
		set @NEW_ITEM_SET_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_item_set set new_item_set_id = @NEW_ITEM_SET_ID
		WHERE NEW_ITEM_ID = @WORKING_ITEM_ID
		print @WORKING_ITEM_ID

		FETCH NEXT FROM ITEM_ID_CURSOR 
		INTO @WORKING_ITEM_ID
	END

	CLOSE ITEM_ID_CURSOR
	DEALLOCATE ITEM_ID_CURSOR
	
	--select * from Reviewer.dbo.TB_ITEM_SET where ITEM_ID in -- 5 OK
	--(select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	--select * from @tb_item_set -- 6 OK

--07 update @tb_item_attribute with NEW_ITEM_ID, NEW_ITEM_SET_ID, NEW_ATTRIBUTE_ID
	print 'starting 7'
	update @tb_item_attribute
	set NEW_ITEM_ID = 
	(select NEW_ITEM_ID from @tb_item_set
	where EXAMPLE_ITEM_ID = OLD_ITEM_ID)
	
	select * from @tb_item_attribute -- 7 OK
	print 'Step 7 OK'
	
	update @tb_item_attribute
	set NEW_ITEM_SET_ID = 
	(select NEW_ITEM_SET_ID from @tb_item_set
	where EXAMPLE_ITEM_SET_ID = OLD_ITEM_SET_ID)
	
	select * from @tb_item_attribute -- 8 OK
	print 'Step 8 OK'
	
	update @tb_item_attribute
	set NEW_ATTRIBUTE_ID = 
	(select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE a
	where a.OLD_ATTRIBUTE_ID = ORIGINAL_ATTRIBUTE_ID)
	
	select * from @tb_item_attribute -- 9 OK
	print 'Step 9 OK'

--08 place new rows in TB_ITEM_ATTRIBUTE based on @tb_item_attribute
	insert into Reviewer.dbo.TB_ITEM_ATTRIBUTE (ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT)
	select tb_i_a.NEW_ITEM_ID, tb_i_a.NEW_ITEM_SET_ID, tb_i_a.NEW_ATTRIBUTE_ID, tb_i_a.ADDITIONAL_TEXT 
	from @tb_item_attribute tb_i_a
	
--09 update TB_ITEM_SET with IS_COMPLETED values in @tb_item_set	
	UPDATE Reviewer.dbo.TB_ITEM_SET
	SET IS_COMPLETED = i_s.EXAMPLE_IS_COMPLETED
	FROM @tb_item_set i_s
	where i_s.NEW_ITEM_SET_ID = ITEM_SET_ID
	and i_s.NEW_ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW -- to restrict it to items in new review
	where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
	--select * from Reviewer.dbo.TB_ITEM_SET where ITEM_ID in -- 10 OK
	--(select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
--10 fill up @tb_item_outcome
	insert into @tb_item_outcome (old_outcome_id, old_item_set_id, outcome_type_id,
	 old_item_attribute_id_intervention, old_item_attribute_id_control,
	 old_item_attribute_id_outcome, outcome_title, data1, data2, data3, data4, data5,
	 data6, data7, data8, data9, data10, data11, data12, data13,
	 data14, outcome_description)
	select i_c.OUTCOME_ID, i_c.ITEM_SET_ID, i_c.OUTCOME_TYPE_ID, i_c.ITEM_ATTRIBUTE_ID_INTERVENTION,
	i_c.ITEM_ATTRIBUTE_ID_CONTROL, i_c.ITEM_ATTRIBUTE_ID_OUTCOME, i_c.OUTCOME_TITLE,
	i_c.DATA1, i_c.DATA2, i_c.DATA3, i_c.DATA4, i_c.DATA5, i_c.DATA6, i_c.DATA7,
	i_c.DATA8, i_c.DATA9, i_c.DATA10, i_c.DATA11, i_c.DATA12, i_c.DATA13, i_c.DATA14,
	i_c.OUTCOME_DESCRIPTION
	from Reviewer.dbo.TB_ITEM_OUTCOME i_c 
	inner join Reviewer.dbo.TB_ITEM_SET i_s on i_s.ITEM_SET_ID = i_c.ITEM_SET_ID
	where i_s.SET_ID = @SOURCE_SET_ID
	
--11 update @tb_item_outcome with the 'new_' values	
	update t1 --new_item_set_id
	set new_item_set_id = t2.NEW_ITEM_SET_ID
	from @tb_item_outcome t1 inner join @tb_item_set t2
	on t2.EXAMPLE_ITEM_SET_ID = t1.old_item_set_id
	
	update t1 --new_item_attribute_id_intervention
	set new_item_attribute_id_intervention = t2.ATTRIBUTE_ID
	from @tb_item_outcome t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_intervention
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
	update t1 --new_item_attribute_id_control
	set new_item_attribute_id_control = t2.ATTRIBUTE_ID
	from @tb_item_outcome t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_control
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
	update t1 --new_item_attribute_id_outcome
	set new_item_attribute_id_outcome = t2.ATTRIBUTE_ID
	from @tb_item_outcome t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_outcome
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
--12 copy @tb_item_outcome into Reviewer.dbo.TB_ITEM_OUTCOME
	declare @WORKING_OUTCOME_ID int
	declare OUTCOME_ID_CURSOR cursor for
	select old_outcome_id FROM @tb_item_outcome
	open OUTCOME_ID_CURSOR
	fetch next from OUTCOME_ID_CURSOR
	into @WORKING_OUTCOME_ID
	while @@FETCH_STATUS = 0
	begin
		insert into Reviewer.dbo.TB_ITEM_OUTCOME (ITEM_SET_ID, OUTCOME_TYPE_ID, 
		ITEM_ATTRIBUTE_ID_INTERVENTION, ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, 
		OUTCOME_TITLE, DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7,
		DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14, OUTCOME_DESCRIPTION)	
		SELECT new_item_set_id, outcome_type_id, new_item_attribute_id_intervention, 
		new_item_attribute_id_control, new_item_attribute_id_outcome, outcome_title, 
		data1, data2, data3, data4, data5, data6, data7, data8, data9, data10, data11, 
		data12, data13, data14, outcome_description 
		FROM @tb_item_outcome 
		WHERE old_outcome_id = @WORKING_OUTCOME_ID
		set @NEW_OUTCOME_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_item_outcome set new_outcome_id = @NEW_OUTCOME_ID
		WHERE old_outcome_id = @WORKING_OUTCOME_ID

		FETCH NEXT FROM OUTCOME_ID_CURSOR 
		INTO @WORKING_OUTCOME_ID
	END

	CLOSE OUTCOME_ID_CURSOR
	DEALLOCATE OUTCOME_ID_CURSOR

--13 fill up @tb_item_outcome_attribute
	insert into @tb_item_outcome_attribute (old_item_outcome_attribute_id,
	 old_outcome_id, old_attribute_id, additional_text)
	select ITEM_OUTCOME_ATTRIBUTE_ID, OUTCOME_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT
	from Reviewer.dbo.TB_ITEM_OUTCOME_ATTRIBUTE i_o_a
	inner join @tb_item_outcome i_o on i_o.OLD_OUTCOME_ID = i_o_a.OUTCOME_ID	

--14 update tb_item_outcome_attribute with the 'new_' values
	update t1 --new_outcome_id
	set new_outcome_id = t2.new_outcome_id
	from @tb_item_outcome_attribute t1 inner join @tb_item_outcome t2
	on t2.old_outcome_id = t1.old_outcome_id
	
	update t1 --new_attribute_id
	set new_attribute_id = t2.ATTRIBUTE_ID
	from @tb_item_outcome_attribute t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_attribute_id
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
--15 copy @tb_item_outcome_attribute into Reviewer.dbo.TB_ITEM_OUTCOME_ATTRIBUTE
	insert into Reviewer.dbo.TB_ITEM_OUTCOME_ATTRIBUTE (OUTCOME_ID, ATTRIBUTE_ID,
	ADDITIONAL_TEXT)
	select new_outcome_id, new_attribute_id, additional_text from @tb_item_outcome_attribute
	
--16 place the outcome_id values into ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA for later use
	insert into ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA (NEW_REVIEW_ID, OLD_OUTCOME_ID, NEW_OUTCOME_ID)
	select @DESTINATION_REVIEW_ID, old_outcome_id, new_outcome_id  
	from @tb_item_outcome
	
COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to copy the coding from the old review to the new review'
end
END CATCH


RETURN

SET NOCOUNT OFF














GO
/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep13]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO











CREATE OR ALTER procedure [dbo].[st_CopyReviewStep13]
(	
	@CONTACT_ID int,
	@SOURCE_REVIEW_ID int,
	@DESTINATION_REVIEW_ID int,
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON


declare @NEW_SEARCH_ID int
declare @NEW_ITEM_DOCUMENT_ID bigint
set @RESULT = '0'

declare @tb_work_allocation table
(new_set_id bigint, old_set_id bigint,
 new_attribute_id bigint, old_attribute_id bigint)
 
declare @tb_item_document table
(new_item_document_id bigint, old_item_document_id bigint, 
 new_item_id bigint, old_item_id bigint, document_title nvarchar(255), document_binary image, 
 document_extenstion nvarchar(5), document_text nvarchar(max))


BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION


--01 copy the work assignments
	insert into @tb_work_allocation (old_set_id, old_attribute_id)
	select w_s.SET_ID, w_s.ATTRIBUTE_ID  
	from Reviewer.dbo.TB_WORK_ALLOCATION w_s
	where w_s.REVIEW_ID = @SOURCE_REVIEW_ID

	update t1
	set new_set_id = t2.SET_ID
	from @tb_work_allocation t1 inner join Reviewer.dbo.TB_SET t2
	on t2.OLD_GUIDELINE_ID = t1.old_set_id
	where t2.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
	update t1
	set new_attribute_id = t2.ATTRIBUTE_ID
	from @tb_work_allocation t1 inner join Reviewer.dbo.TB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_attribute_id
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
	insert into Reviewer.dbo.TB_WORK_ALLOCATION (CONTACT_ID, REVIEW_ID, SET_ID, ATTRIBUTE_ID)
	select @CONTACT_ID, @DESTINATION_REVIEW_ID, w_a.new_set_id, w_a.new_attribute_id  
	from @tb_work_allocation w_a

--02 copy diagrams
	insert into Reviewer.dbo.TB_DIAGRAM (REVIEW_ID, DIAGRAM_NAME, DIAGRAM_DETAIL)
	select @DESTINATION_REVIEW_ID, d.DIAGRAM_NAME, d.DIAGRAM_DETAIL
	from  Reviewer.dbo.TB_DIAGRAM d
	where d.REVIEW_ID = @SOURCE_REVIEW_ID

--03 copy searches
	declare @tb_search table (old_search_id int, review_id int, contact_id int, search_title nvarchar(4000),
		search_no int, answers nvarchar(4000), hits_no int)
	
	insert into @tb_search (old_search_id, review_id, contact_id, search_title, search_no, answers, hits_no)
	select s.SEARCH_ID, @DESTINATION_REVIEW_ID, @CONTACT_ID, s.SEARCH_TITLE, s.SEARCH_NO, s.ANSWERS, s.HITS_NO
	from Reviewer.dbo.TB_SEARCH s
	where s.REVIEW_ID = @SOURCE_REVIEW_ID
	
	--select * from @tb_search
	
	declare @tb_search_item table (new_item_id bigint, old_item_id bigint, old_search_id int, item_rank int)
	
	insert into @tb_search_item (old_item_id, old_search_id, item_rank)
	select s_i.ITEM_ID, s_i.SEARCH_ID, s_i.ITEM_RANK
	from Reviewer.dbo.TB_SEARCH_ITEM s_i
	where SEARCH_ID in (select old_search_id from @tb_search)
	
	--select * from @tb_search_item
	
	update t1
	set new_item_id = t2.ITEM_ID
	from @tb_search_item t1 inner join Reviewer.dbo.TB_ITEM t2
	on t2.OLD_ITEM_ID = t1.old_item_id
	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
	where i_r.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
	--select * from @tb_search_item
	
	declare @WORKING_SEARCH_ID int
	declare SEARCH_ID_CURSOR cursor for
	select old_search_id FROM @tb_search
	open SEARCH_ID_CURSOR
	fetch next from SEARCH_ID_CURSOR
	into @WORKING_SEARCH_ID
	while @@FETCH_STATUS = 0
	begin
		insert into Reviewer.dbo.TB_SEARCH (REVIEW_ID, CONTACT_ID, SEARCH_TITLE, SEARCH_NO, ANSWERS, HITS_NO, SEARCH_DATE)	
		SELECT review_id, @CONTACT_ID, search_title, search_no, answers, hits_no, GETDATE() FROM @tb_search
		WHERE old_search_id = @WORKING_SEARCH_ID
		set @NEW_SEARCH_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		insert into Reviewer.dbo.TB_SEARCH_ITEM (ITEM_ID, SEARCH_ID, ITEM_RANK)
		select new_item_id, @NEW_SEARCH_ID, item_rank from @tb_search_item
		where old_search_id = @WORKING_SEARCH_ID
		
		FETCH NEXT FROM SEARCH_ID_CURSOR 
		INTO @WORKING_SEARCH_ID
	END

	CLOSE SEARCH_ID_CURSOR
	DEALLOCATE SEARCH_ID_CURSOR
	

--04 copy pdfs
	insert into @tb_item_document (old_item_document_id, old_item_id, document_title, 
	document_binary, document_extenstion, document_text)
	select i_d.ITEM_DOCUMENT_ID, i_d.ITEM_ID, i_d.DOCUMENT_TITLE, i_d.DOCUMENT_BINARY,
	i_d.DOCUMENT_EXTENSION, i_d.DOCUMENT_TEXT  
	from Reviewer.dbo.TB_ITEM_DOCUMENT i_d
	where i_d.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW
	where REVIEW_ID = @SOURCE_REVIEW_ID)
	
	update t1
	set new_item_id = t2.ITEM_ID
	from @tb_item_document t1 inner join Reviewer.dbo.TB_ITEM t2
	on t2.OLD_ITEM_ID = t1.old_item_id
	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
	where i_r.REVIEW_ID = @DESTINATION_REVIEW_ID) 
	
--select * from @tb_item_document

	declare @WORKING_ITEM_DOCUMENT_ID int
	declare ITEM_DOCUMENT_ID_CURSOR cursor for
	select old_item_document_id FROM @tb_item_document
	open ITEM_DOCUMENT_ID_CURSOR
	fetch next from ITEM_DOCUMENT_ID_CURSOR
	into @WORKING_ITEM_DOCUMENT_ID
	while @@FETCH_STATUS = 0
	begin
		insert into Reviewer.dbo.TB_ITEM_DOCUMENT (ITEM_ID, DOCUMENT_TITLE, DOCUMENT_BINARY, 
		DOCUMENT_EXTENSION, DOCUMENT_TEXT)	
		SELECT new_item_id, document_title, document_binary, document_extenstion, document_text 
		FROM @tb_item_document
		WHERE old_item_document_id = @WORKING_ITEM_DOCUMENT_ID
		set @NEW_ITEM_DOCUMENT_ID = CAST(SCOPE_IDENTITY() AS INT)
				
		FETCH NEXT FROM ITEM_DOCUMENT_ID_CURSOR 
		INTO @WORKING_ITEM_DOCUMENT_ID
	END

	CLOSE ITEM_DOCUMENT_ID_CURSOR
	DEALLOCATE ITEM_DOCUMENT_ID_CURSOR


	
	
COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to copy the work assignments, diagrams, searches, uploaded documents'
end
END CATCH


RETURN

SET NOCOUNT OFF














GO
/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep15]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO











CREATE OR ALTER procedure [dbo].[st_CopyReviewStep15]
(	
	@CONTACT_ID int,
	@SOURCE_REVIEW_ID int,
	@DESTINATION_REVIEW_ID int,
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON



set @RESULT = '0'
declare @NEW_REPORT_ID int
declare @NEW_REPORT_COLUMN_ID int
declare @NEW_META_ANALYSIS_ID int
declare @NEW_ITEM_DOCUMENT_ID int



BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION


--01 copy the reports
declare @tb_report table
(new_report_id bigint, old_report_id bigint, name nvarchar(255), report_type nvarchar(10))

	insert into @tb_report (old_report_id, name, report_type)
	select r.REPORT_ID, r.NAME, r.REPORT_TYPE  
	from Reviewer.dbo.TB_REPORT r
	where r.REVIEW_ID = @SOURCE_REVIEW_ID

	--select * from @tb_report


	declare @WORKING_REPORT_ID int
	declare REPORT_ID_CURSOR cursor for
	select old_report_id FROM @tb_report
	open REPORT_ID_CURSOR
	fetch next from REPORT_ID_CURSOR
	into @WORKING_REPORT_ID
	while @@FETCH_STATUS = 0
	begin
		insert into Reviewer.dbo.TB_REPORT (REVIEW_ID, CONTACT_ID, NAME, REPORT_TYPE)	
		SELECT @DESTINATION_REVIEW_ID, @CONTACT_ID, name, report_type FROM @tb_report
		WHERE old_report_id = @WORKING_REPORT_ID
		set @NEW_REPORT_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_report
		set new_report_id = @NEW_REPORT_ID
		where old_report_id = @WORKING_REPORT_ID
		
		FETCH NEXT FROM REPORT_ID_CURSOR 
		INTO @WORKING_REPORT_ID
	END

	CLOSE REPORT_ID_CURSOR
	DEALLOCATE REPORT_ID_CURSOR


declare @tb_report_column table
(new_report_column_id int, old_report_column_id int, new_report_id bigint, old_report_id int,
report_column_name nvarchar(255), column_order int)

	insert into @tb_report_column (old_report_column_id, old_report_id, report_column_name, column_order)
	select r_c.REPORT_COLUMN_ID, r_c.REPORT_ID, r_c.REPORT_COLUMN_NAME, r_c.COLUMN_ORDER  
	from Reviewer.dbo.TB_REPORT_COLUMN r_c
	where r_c.REPORT_ID in (select old_report_id from @tb_report)
	
--select * from @tb_report_column
	
	update t1
	set new_report_id = t2.new_report_id
	from @tb_report_column t1 inner join @tb_report t2
	on t2.old_report_id = t1.old_report_id
	
--select * from @tb_report_column	
	
	
	declare @WORKING_REPORT_COLUMN_ID int
	declare REPORT_COLUMN_ID_CURSOR cursor for
	select old_report_column_id FROM @tb_report_column
	open REPORT_COLUMN_ID_CURSOR
	fetch next from REPORT_COLUMN_ID_CURSOR
	into @WORKING_REPORT_COLUMN_ID
	while @@FETCH_STATUS = 0
	begin
		insert into Reviewer.dbo.TB_REPORT_COLUMN (REPORT_ID, REPORT_COLUMN_NAME, COLUMN_ORDER)	
		SELECT new_report_id, report_column_name, column_order FROM @tb_report_column
		WHERE old_report_column_id = @WORKING_REPORT_COLUMN_ID
		set @NEW_REPORT_COLUMN_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_report_column
		set new_report_column_id = @NEW_REPORT_COLUMN_ID
		where old_report_column_id = @WORKING_REPORT_COLUMN_ID
		
		FETCH NEXT FROM REPORT_COLUMN_ID_CURSOR 
		INTO @WORKING_REPORT_COLUMN_ID
	END

	CLOSE REPORT_COLUMN_ID_CURSOR
	DEALLOCATE REPORT_COLUMN_ID_CURSOR
	
--select * from @tb_report_column		
	
declare @tb_report_column_code table
(new_report_column_code_id int, old_report_column_code_id int, new_report_id bigint, old_report_id int,
new_report_column_id int, old_report_column_id int, code_order int, new_set_id int, old_set_id nvarchar(50),
new_attribute_id bigint, old_attribute_id bigint, new_parent_attribute_id bigint, old_parent_attribute_id bigint, parent_attribute_text nvarchar(255),
user_def_text nvarchar(255), display_code bit, display_additional_text bit, display_coded_text bit)

	insert into @tb_report_column_code (old_report_column_code_id, old_report_id, old_report_column_id, code_order, old_set_id,
	old_attribute_id, old_parent_attribute_id, parent_attribute_text, user_def_text, display_code, display_additional_text, display_coded_text)
	select r_c_c.REPORT_COLUMN_CODE_ID,  r_c_c.REPORT_ID, r_c_c.REPORT_COLUMN_ID, r_c_c.CODE_ORDER, r_c_c.SET_ID, r_c_c.ATTRIBUTE_ID, 
	r_c_c.PARENT_ATTRIBUTE_ID, r_c_c.PARENT_ATTRIBUTE_TEXT,
	r_c_c.USER_DEF_TEXT, r_c_c.DISPLAY_CODE, r_c_c.DISPLAY_ADDITIONAL_TEXT, r_c_c.DISPLAY_CODED_TEXT  
	from Reviewer.dbo.TB_REPORT_COLUMN_CODE r_c_c
	where r_c_c.REPORT_ID in (select old_report_id from @tb_report)

--select * from @tb_report_column_code
	
	update r_c_c
	set new_report_id = r.new_report_id
	from @tb_report r inner join @tb_report_column_code r_c_c
	on r.old_report_id = r_c_c.old_report_id

--select * from @tb_report_column_code

	update r_c_c
	set new_report_column_id = r_c.new_report_column_id
	from @tb_report_column r_c inner join @tb_report_column_code r_c_c
	on r_c.old_report_column_id = r_c_c.old_report_column_id
	
--select * from @tb_report_column_code

	update r_c_c
	set new_set_id = s.SET_ID
	from Reviewer.dbo.TB_SET s inner join @tb_report_column_code r_c_c
	on s.OLD_GUIDELINE_ID = r_c_c.old_set_id
	where s.SET_ID in (select r_s.SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
--select * from @tb_report_column_code

	update r_c_c
	set new_attribute_id = a.ATTRIBUTE_ID
	from Reviewer.dbo.TB_ATTRIBUTE a inner join @tb_report_column_code r_c_c
	on a.OLD_ATTRIBUTE_ID = r_c_c.old_attribute_id
	where a.ATTRIBUTE_ID in (select a_s.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
--select * from @tb_report_column_code

	update r_c_c
	set new_parent_attribute_id = a_s.PARENT_ATTRIBUTE_ID
	from Reviewer.dbo.TB_ATTRIBUTE_SET a_s inner join @tb_report_column_code r_c_c
	on a_s.ATTRIBUTE_ID = r_c_c.new_attribute_id
	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID)

--select * from @tb_report_column_code	

	insert into Reviewer.dbo.TB_REPORT_COLUMN_CODE (REPORT_ID, REPORT_COLUMN_ID, CODE_ORDER, 
	SET_ID, ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID, PARENT_ATTRIBUTE_TEXT, USER_DEF_TEXT, 
	DISPLAY_CODE, DISPLAY_ADDITIONAL_TEXT, DISPLAY_CODED_TEXT)  
	select r_c_c.new_report_id, r_c_c.new_report_column_id, r_c_c.code_order, r_c_c.new_set_id,
	r_c_c.new_attribute_id, r_c_c.new_parent_attribute_id, r_c_c.parent_attribute_text,
	r_c_c.user_def_text, r_c_c.display_code, r_c_c.display_additional_text,
	r_c_c.display_coded_text
	from @tb_report_column_code r_c_c


--02 copy meta-analysis

	
declare @tb_meta_analysis table
(new_meta_analysis_id bigint, old_meta_analysis_id bigint, meta_analysis_title nvarchar(255), 
contact_id int, review_id int, new_attribute_id bigint, old_attribute_id bigint, new_set_id int,
old_set_id int, new_attribute_id_intervention bigint, old_attribute_id_intervention bigint,
new_attribute_id_control bigint, old_attribute_id_control bigint, new_attribute_id_outcome bigint,
old_attribute_id_outcome bigint, meta_analysis_type_id bigint)

	insert into @tb_meta_analysis (old_meta_analysis_id, meta_analysis_title, contact_id,
	review_id, old_attribute_id, old_set_id, old_attribute_id_intervention, old_attribute_id_control,
	old_attribute_id_outcome, meta_analysis_type_id)
	select m_a.META_ANALYSIS_ID, m_a.META_ANALYSIS_TITLE, m_a.CONTACT_ID, @DESTINATION_REVIEW_ID,
	m_a.ATTRIBUTE_ID, m_a.SET_ID, m_a.ATTRIBUTE_ID_INTERVENTION, m_a.ATTRIBUTE_ID_CONTROL,
	m_a.ATTRIBUTE_ID_OUTCOME, m_a.META_ANALYSIS_TYPE_ID  
	from Reviewer.dbo.TB_META_ANALYSIS m_a
	where m_a.REVIEW_ID = @SOURCE_REVIEW_ID
	
--select * from @tb_meta_analysis

	update m_a
	set new_set_id = s.SET_ID
	from Reviewer.dbo.TB_SET s inner join @tb_meta_analysis m_a
	on s.OLD_GUIDELINE_ID = m_a.old_set_id
	where s.SET_ID in (select r_s.SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID)

--select * from @tb_meta_analysis

	update m_a
	set new_attribute_id = a.ATTRIBUTE_ID
	from Reviewer.dbo.TB_ATTRIBUTE a inner join @tb_meta_analysis m_a
	on a.OLD_ATTRIBUTE_ID = m_a.old_attribute_id
	where a.ATTRIBUTE_ID in (select a_s.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
--cleanup new_attribute_id as it is was only used in old reviews
	update @tb_meta_analysis set new_attribute_id = 0
	where new_attribute_id is null
	
	update m_a
	set new_attribute_id_intervention = a.ATTRIBUTE_ID
	from Reviewer.dbo.TB_ATTRIBUTE a inner join @tb_meta_analysis m_a
	on a.OLD_ATTRIBUTE_ID = m_a.old_attribute_id_intervention
	where a.ATTRIBUTE_ID in (select a_s.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
	update m_a
	set new_attribute_id_intervention = a.ATTRIBUTE_ID
	from Reviewer.dbo.TB_ATTRIBUTE a inner join @tb_meta_analysis m_a
	on a.OLD_ATTRIBUTE_ID = m_a.old_attribute_id_intervention
	where a.ATTRIBUTE_ID in (select a_s.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
	update m_a
	set new_attribute_id_control = a.ATTRIBUTE_ID
	from Reviewer.dbo.TB_ATTRIBUTE a inner join @tb_meta_analysis m_a
	on a.OLD_ATTRIBUTE_ID = m_a.old_attribute_id_control
	where a.ATTRIBUTE_ID in (select a_s.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
	update m_a
	set new_attribute_id_outcome = a.ATTRIBUTE_ID
	from Reviewer.dbo.TB_ATTRIBUTE a inner join @tb_meta_analysis m_a
	on a.OLD_ATTRIBUTE_ID = m_a.old_attribute_id_outcome
	where a.ATTRIBUTE_ID in (select a_s.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
	where a_s.SET_ID in (select SET_ID from Reviewer.dbo.TB_REVIEW_SET r_s
	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID))
	
--select * from @tb_meta_analysis

	declare @WORKING_META_ANALYSIS_ID int
	declare META_ANALYSIS_ID_CURSOR cursor for
	select old_meta_analysis_id FROM @tb_meta_analysis
	open META_ANALYSIS_ID_CURSOR
	fetch next from META_ANALYSIS_ID_CURSOR
	into @WORKING_META_ANALYSIS_ID
	while @@FETCH_STATUS = 0
	begin
		insert into Reviewer.dbo.TB_META_ANALYSIS (META_ANALYSIS_TITLE, CONTACT_ID, 
		REVIEW_ID, ATTRIBUTE_ID, SET_ID, ATTRIBUTE_ID_INTERVENTION,
		ATTRIBUTE_ID_CONTROL, ATTRIBUTE_ID_OUTCOME, META_ANALYSIS_TYPE_ID)	
		SELECT meta_analysis_title, contact_id, review_id, new_attribute_id,
		new_set_id, new_attribute_id_intervention, new_attribute_id_control,
		new_attribute_id_outcome, meta_analysis_type_id 
		FROM @tb_meta_analysis
		WHERE old_meta_analysis_id = @WORKING_META_ANALYSIS_ID
		set @NEW_META_ANALYSIS_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_meta_analysis
		set new_meta_analysis_id = @NEW_META_ANALYSIS_ID
		where old_meta_analysis_id = @WORKING_META_ANALYSIS_ID
		
		FETCH NEXT FROM META_ANALYSIS_ID_CURSOR 
		INTO @WORKING_META_ANALYSIS_ID
	END

	CLOSE META_ANALYSIS_ID_CURSOR
	DEALLOCATE META_ANALYSIS_ID_CURSOR

--select * from @tb_meta_analysis

declare @tb_meta_analysis_outcome table
(new_meta_analysis_id int, old_meta_analysis_id int, 
 new_outcome_id int, old_outcome_id int)

	insert into @tb_meta_analysis_outcome (old_meta_analysis_id, old_outcome_id)
	select m_a_o.META_ANALYSIS_ID, m_a_o.OUTCOME_ID 
	from Reviewer.dbo.TB_META_ANALYSIS_OUTCOME m_a_o
	where m_a_o.META_ANALYSIS_ID in (select META_ANALYSIS_ID from 
	Reviewer.dbo.TB_META_ANALYSIS where REVIEW_ID = @SOURCE_REVIEW_ID)

--select * from @tb_meta_analysis_outcome

	update m_a_o
	set m_a_o.new_meta_analysis_id = m_a.new_meta_analysis_id
	from @tb_meta_analysis m_a inner join @tb_meta_analysis_outcome m_a_o
	on m_a.old_meta_analysis_id = m_a_o.old_meta_analysis_id

-- how to find a new OUTCOME_ID using an old OUTCOME_ID in TB_ITEM_OUTCOME?
--- Database is not set up to allow this as ER4 has no need to understand the concept of 
--- old and new outcome. This is just something I have invented to allow the review copy
--- function. I need to do a bit of gymnastics to make this work.
--- The only answer I can think of is to record old and new outcomes at the time 
--- of creation (in an earlier st_ ) and hold it externally in ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA
--- until I need it.

	update m_a_o
	set m_a_o.new_outcome_id = r_c_d.NEW_OUTCOME_ID
	from ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA r_c_d inner join @tb_meta_analysis_outcome m_a_o
	on r_c_d.OLD_OUTCOME_ID = m_a_o.old_outcome_id
	where r_c_d.NEW_REVIEW_ID = @DESTINATION_REVIEW_ID
	
--select * from @tb_meta_analysis_outcome	

	insert into Reviewer.dbo.TB_META_ANALYSIS_OUTCOME 
	(META_ANALYSIS_ID, OUTCOME_ID)
	select new_meta_analysis_id, new_outcome_id from @tb_meta_analysis_outcome


-- 03 copy links

declare @tb_item_link table
(old_item_link_id int, new_item_id_primary bigint, old_item_id_primary bigint,
new_item_id_secondary bigint, old_item_id_secondary bigint, link_description nvarchar(255))

insert into @tb_item_link (old_item_link_id, old_item_id_primary, 
	old_item_id_secondary, link_description)
	select i_l.ITEM_LINK_ID, i_l.ITEM_ID_PRIMARY, i_l.ITEM_ID_SECONDARY,
	i_l.LINK_DESCRIPTION  
	from Reviewer.dbo.TB_ITEM_LINK i_l
	where i_l.ITEM_ID_PRIMARY in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW
	where REVIEW_ID = @SOURCE_REVIEW_ID)
	
--select * from @tb_item_link

	update t1
	set new_item_id_primary = t2.ITEM_ID
	from @tb_item_link t1 inner join Reviewer.dbo.TB_ITEM t2
	on t2.OLD_ITEM_ID = t1.old_item_id_primary
	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
	where i_r.REVIEW_ID = @DESTINATION_REVIEW_ID) 
	
	update t1
	set new_item_id_secondary = t2.ITEM_ID
	from @tb_item_link t1 inner join Reviewer.dbo.TB_ITEM t2
	on t2.OLD_ITEM_ID = t1.old_item_id_secondary
	where t2.ITEM_ID in (select ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
	where i_r.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
--select * from @tb_item_link


	declare @WORKING_ITEM_LINK_ID int
	declare ITEM_LINK_ID_CURSOR cursor for
	select old_item_link_id FROM @tb_item_link
	open ITEM_LINK_ID_CURSOR
	fetch next from ITEM_LINK_ID_CURSOR
	into @WORKING_ITEM_LINK_ID
	while @@FETCH_STATUS = 0
	begin
		insert into Reviewer.dbo.TB_ITEM_LINK (ITEM_ID_PRIMARY, ITEM_ID_SECONDARY,
		LINK_DESCRIPTION)	
		SELECT new_item_id_primary, new_item_id_secondary, link_description 
		FROM @tb_item_link
		WHERE old_item_link_id = @WORKING_ITEM_LINK_ID
		set @NEW_ITEM_DOCUMENT_ID = CAST(SCOPE_IDENTITY() AS INT)
				
		FETCH NEXT FROM ITEM_LINK_ID_CURSOR 
		INTO @WORKING_ITEM_LINK_ID
	END

	CLOSE ITEM_LINK_ID_CURSOR
	DEALLOCATE ITEM_LINK_ID_CURSOR


	
	
COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to copy the reports, meta-analysis, links'
end
END CATCH


RETURN

SET NOCOUNT OFF















GO
/****** Object:  StoredProcedure [dbo].[st_CopyReviewStepCleanup]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO











CREATE OR ALTER procedure [dbo].[st_CopyReviewStepCleanup]
(	
	@DESTINATION_REVIEW_ID int,
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON

-- tables and fields to set to null
-- TB_ATTRIBUTE - OLD_ATTRIBUTE_ID
-- TB_SET - OLD_GUIDELINE_ID
-- TB_ITEM - OLD_ITEM_ID

set @RESULT = '0'


BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION


--01 clean up TB_ATTRIBUTE
	update Reviewer.dbo.TB_ATTRIBUTE
	set OLD_ATTRIBUTE_ID = null
	where ATTRIBUTE_ID in 
	(select a.ATTRIBUTE_ID from Reviewer.dbo.TB_ATTRIBUTE a
	inner join Reviewer.dbo.TB_ATTRIBUTE_SET a_s on a_s.ATTRIBUTE_ID = a.ATTRIBUTE_ID
	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = a_s.SET_ID
	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
--02 clean up TB_SET
	update Reviewer.dbo.TB_SET
	set OLD_GUIDELINE_ID = null
	where SET_ID in 
	(select s.SET_ID from  Reviewer.dbo.TB_SET s
	inner join Reviewer.dbo.TB_REVIEW_SET r_s on r_s.SET_ID = s.SET_ID
	where r_s.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
--03 clean up TB_ITEM
	update Reviewer.dbo.TB_ITEM
	set OLD_ITEM_ID = null
	where ITEM_ID in
	(select i_r.ITEM_ID from Reviewer.dbo.TB_ITEM_REVIEW i_r
	where i_r.REVIEW_ID = @DESTINATION_REVIEW_ID)
	
--03 clean up ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA
	delete from ReviewerAdmin.dbo.TB_REVIEW_COPY_DATA
	where NEW_REVIEW_ID = @DESTINATION_REVIEW_ID


COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to remove the links between the old and new review'
end
END CATCH


RETURN

SET NOCOUNT OFF














GO
/****** Object:  StoredProcedure [dbo].[st_CountExampleReviews]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO








CREATE OR ALTER procedure [dbo].[st_CountExampleReviews]
(
	@CONTACT_ID int,
	@NUMBER_REVIEWS int output
)

As

SET NOCOUNT ON


declare @contact_name nvarchar(255)
declare @review_name nvarchar(1000) 

select @contact_name = CONTACT_NAME from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID
set @review_name = @contact_name + '''s example non-shareable review'

	
	select @NUMBER_REVIEWS = COUNT(REVIEW_NAME) 
	from Reviewer.dbo.TB_REVIEW
	where REVIEW_NAME = @review_name
	and REVIEW_ID in (select REVIEW_ID from Reviewer.dbo.TB_REVIEW_CONTACT
	where CONTACT_ID = @CONTACT_ID)
	

RETURN

SET NOCOUNT OFF











GO
/****** Object:  StoredProcedure [dbo].[st_CountriesGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<gets contact table for a contactID>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_CountriesGet] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

		select * from TB_COUNTRIES
		order by ORDER_NUMBER 
    	
	RETURN
END
GO
/****** Object:  StoredProcedure [dbo].[st_CreditHistoryByPurchase]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO








-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER     PROCEDURE [dbo].[st_CreditHistoryByPurchase] 
(
	@CREDIT_PURCHASE_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @reviewPrice int
	declare @accountPrice int
	declare @forSaleID int 
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Professional') -- account
	set @accountPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Shareable Review') -- review
	set @reviewPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)

	declare @tv_credit_history table (tv_credit_extension_id int, tv_type_extended_id int, 
		tv_type_extended_name nvarchar(1000), tv_id_extended int, tv_name nvarchar(255), 
		tv_date_extended date, tv_new_date date, tv_old_date date, tv_true_old_date date, tv_months_extended int, tv_cost int)

	insert into @tv_credit_history (tv_credit_extension_id, tv_type_extended_id, tv_id_extended, 
		tv_date_extended, tv_new_date, tv_old_date)
	select ce.CREDIT_EXTENSION_ID, eel.TYPE_EXTENDED, eel.ID_EXTENDED, eel.DATE_OF_EDIT, eel.NEW_EXPIRY_DATE, 
		eel.OLD_EXPIRY_DATE from TB_CREDIT_EXTENSIONS ce
	inner join TB_EXPIRY_EDIT_LOG eel on eel.EXPIRY_EDIT_ID = ce.EXPIRY_EDIT_ID
	where ce.CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID
	order by CREDIT_EXTENSION_ID

	-- where a non-sharable review was extended and didn't have a tv_old_date
	update @tv_credit_history set tv_old_date = tv_date_extended where tv_old_date is null		

	-- reviews or accounts
	update @tv_credit_history set tv_type_extended_name = 'Review' where tv_type_extended_id = 0
	update @tv_credit_history set tv_type_extended_name = 'Account' where tv_type_extended_id = 1

	-- review name
	update t1
	set tv_name = t2.REVIEW_NAME
	from @tv_credit_history t1 inner join Reviewer.dbo.TB_REVIEW t2
	on t2.REVIEW_ID = t1.tv_id_extended
	where t1.tv_type_extended_name = 'Review'

	-- account name
	update t1
	set tv_name = t2.CONTACT_NAME
	from @tv_credit_history t1 inner join Reviewer.dbo.TB_CONTACT t2
	on t2.CONTACT_ID = t1.tv_id_extended
	where t1.tv_type_extended_name = 'Account'

	-- get true old_date (i.e. whether the account/review was expired when it was extended)
	update @tv_credit_history set tv_true_old_date = tv_old_date
	update @tv_credit_history set tv_true_old_date = tv_date_extended where tv_date_extended > tv_old_date

	-- months extended
	update @tv_credit_history set tv_months_extended = (select DATEDIFF(MONTH, tv_true_old_date, tv_new_date))

	-- add the total cost
	update @tv_credit_history set tv_cost = tv_months_extended * @accountPrice
	where tv_type_extended_name = 'Account'
	update @tv_credit_history set tv_cost = tv_months_extended * @reviewPrice
	where tv_type_extended_name = 'Review'

	select * from @tv_credit_history
       
	END



GO
/****** Object:  StoredProcedure [dbo].[st_CreditPurchaseCreateOrEdit]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER   procedure [dbo].[st_CreditPurchaseCreateOrEdit]
(
	@CREDIT_PURCHASE_ID int,
	@CONTACT_ID int,
	@CREDIT_PURCHASED int,
	@DATE_PURCHASED datetime,
	@NOTES nvarchar(max),
	@PURCHASE_TYPE nvarchar(10)
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	if @CREDIT_PURCHASE_ID = 0
	begin
	insert into TB_CREDIT_PURCHASE (PURCHASER_CONTACT_ID, CREDIT_PURCHASED, DATE_PURCHASED, NOTES, PURCHASE_TYPE)
		values (@CONTACT_ID, @CREDIT_PURCHASED, @DATE_PURCHASED, @NOTES, @PURCHASE_TYPE)
	end
	else
	begin
		update TB_CREDIT_PURCHASE
		set PURCHASER_CONTACT_ID = @CONTACT_ID, CREDIT_PURCHASED = @CREDIT_PURCHASED,
		    DATE_PURCHASED = @DATE_PURCHASED, NOTES = @NOTES, PURCHASE_TYPE = @PURCHASE_TYPE
		where CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID

	end

END


GO
/****** Object:  StoredProcedure [dbo].[st_CreditPurchaseGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER     PROCEDURE [dbo].[st_CreditPurchaseGet] 
(
	@CREDIT_PURCHASE_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	/*
	select cp.PURCHASER_CONTACT_ID, cp.DATE_PURCHASED, cp.CREDIT_PURCHASED, 
	     cp.NOTES, c.CONTACT_NAME, c.EMAIL from TB_CREDIT_PURCHASE cp
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = cp.PURCHASER_CONTACT_ID
	where cp.CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID
	*/


	declare @reviewPrice int
	declare @accountPrice int
	declare @forSaleID int 
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Professional') -- account
	set @accountPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Shareable Review') -- review
	set @reviewPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)


	declare @tv_credit_purchase table (tv_credit_purchase_id int, tv_credit_purchaser_id int, tv_date_purchased date, tv_credit_purchased int,
		tv_credit_remaining int, tv_notes nvarchar(max), tv_contact_name nvarchar(255), tv_email nvarchar(500))
	insert into @tv_credit_purchase (tv_credit_purchase_id, tv_credit_purchaser_id, tv_date_purchased, tv_credit_purchased, tv_notes)
	select CREDIT_PURCHASE_ID, PURCHASER_CONTACT_ID, DATE_PURCHASED, CREDIT_PURCHASED, NOTES from TB_CREDIT_PURCHASE 
	where CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID

	declare @tv_credit_history table (tv_credit_extension_id int, tv_type_extended_id int, 
		tv_type_extended_name nvarchar(1000), tv_id_extended int,  
		tv_date_extended date, tv_new_date date, tv_old_date date, tv_true_old_date date, tv_months_extended int, tv_cost int)


	declare @totalUsed int = 0
	declare @remainingCredit int = 0
	declare @creditPurchased int = 0


	set @creditPurchased = (select tv_credit_purchased from @tv_credit_purchase
		where tv_credit_purchase_id = @CREDIT_PURCHASE_ID)
		
	insert into @tv_credit_history (tv_credit_extension_id, tv_type_extended_id, tv_id_extended, 
		tv_date_extended, tv_new_date, tv_old_date)
	select ce.CREDIT_EXTENSION_ID, eel.TYPE_EXTENDED, eel.ID_EXTENDED, eel.DATE_OF_EDIT, eel.NEW_EXPIRY_DATE, 
		eel.OLD_EXPIRY_DATE from TB_CREDIT_EXTENSIONS ce
	inner join TB_EXPIRY_EDIT_LOG eel on eel.EXPIRY_EDIT_ID = ce.EXPIRY_EDIT_ID
	where ce.CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID
	order by CREDIT_EXTENSION_ID

	-- reviews or accounts
	update @tv_credit_history set tv_type_extended_name = 'Review' where tv_type_extended_id = 0
	update @tv_credit_history set tv_type_extended_name = 'Account' where tv_type_extended_id = 1

	-- get true old_date (i.e. whether the account/review was expired when it was extended)
	update @tv_credit_history set tv_true_old_date = tv_old_date
	update @tv_credit_history set tv_true_old_date = tv_date_extended where tv_date_extended > tv_old_date OR tv_old_date is null

	-- months extended
	update @tv_credit_history set tv_months_extended = (select DATEDIFF(MONTH, tv_true_old_date, tv_new_date))

	-- add the total cost
	update @tv_credit_history set tv_cost = tv_months_extended * @accountPrice
	where tv_type_extended_name = 'Account'
	update @tv_credit_history set tv_cost = tv_months_extended * @reviewPrice
	where tv_type_extended_name = 'Review'

	set @totalUsed = (SELECT SUM(tv_cost)
		FROM @tv_credit_history)
	IF @totalUsed is null set @totalUsed = 0
	set @remainingCredit = @creditPurchased - @totalUsed

	update @tv_credit_purchase set tv_credit_remaining = @remainingCredit
	where tv_credit_purchase_id = @CREDIT_PURCHASE_ID


	update @tv_credit_purchase
	set tv_contact_name = (select CONTACT_NAME from Reviewer.dbo.TB_CONTACT 
	where CONTACT_ID = (select tv_credit_purchaser_id from @tv_credit_purchase))

	update @tv_credit_purchase
	set tv_email = (select EMAIL from Reviewer.dbo.TB_CONTACT 
	where CONTACT_ID = (select tv_credit_purchaser_id from @tv_credit_purchase))


	select * from @tv_credit_purchase

    	
	RETURN
END


GO
/****** Object:  StoredProcedure [dbo].[st_CreditPurchasesByPurchaser]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER     PROCEDURE [dbo].[st_CreditPurchasesByPurchaser] 
(
	@CONTACT_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @reviewPrice int
	declare @accountPrice int
	declare @forSaleID int 
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Professional') -- account
	set @accountPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Shareable Review') -- review
	set @reviewPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)


	declare @tv_credit_purchases table (tv_credit_purchase_id int, tv_date_purchased date, tb_credit_purchased int,
		tv_credit_remaining int)
	insert into @tv_credit_purchases (tv_credit_purchase_id, tv_date_purchased, tb_credit_purchased)
	select CREDIT_PURCHASE_ID, DATE_PURCHASED, CREDIT_PURCHASED from TB_CREDIT_PURCHASE 
	where PURCHASER_CONTACT_ID = @CONTACT_ID

	declare @tv_credit_history table (tv_credit_extension_id int, tv_type_extended_id int, 
		tv_type_extended_name nvarchar(1000), tv_id_extended int,  
		tv_date_extended date, tv_new_date date, tv_old_date date, tv_true_old_date date, tv_months_extended int, tv_cost int)


	declare @totalUsed int = 0
	declare @remainingCredit int = 0
	declare @creditPurchased int = 0

	declare @WORKING_CREDIT_PURCHASE_ID int
	declare CREDIT_PURCHASE_ID_CURSOR cursor for
	select tv_credit_purchase_id FROM @tv_credit_purchases
	open CREDIT_PURCHASE_ID_CURSOR
	fetch next from CREDIT_PURCHASE_ID_CURSOR
	into @WORKING_CREDIT_PURCHASE_ID
	while @@FETCH_STATUS = 0
	begin
		set @creditPurchased = (select tb_credit_purchased from @tv_credit_purchases
			where tv_credit_purchase_id = @WORKING_CREDIT_PURCHASE_ID)
		
		insert into @tv_credit_history (tv_credit_extension_id, tv_type_extended_id, tv_id_extended, 
			tv_date_extended, tv_new_date, tv_old_date)
		select ce.CREDIT_EXTENSION_ID, eel.TYPE_EXTENDED, eel.ID_EXTENDED, eel.DATE_OF_EDIT, eel.NEW_EXPIRY_DATE, 
			eel.OLD_EXPIRY_DATE from TB_CREDIT_EXTENSIONS ce
		inner join TB_EXPIRY_EDIT_LOG eel on eel.EXPIRY_EDIT_ID = ce.EXPIRY_EDIT_ID
		where ce.CREDIT_PURCHASE_ID = @WORKING_CREDIT_PURCHASE_ID
		order by CREDIT_EXTENSION_ID

		-- where a non-sharable review was extended and didn't have a tv_old_date
		update @tv_credit_history set tv_old_date = tv_date_extended where tv_old_date is null		

		-- reviews or accounts
		update @tv_credit_history set tv_type_extended_name = 'Review' where tv_type_extended_id = 0
		update @tv_credit_history set tv_type_extended_name = 'Account' where tv_type_extended_id = 1

		-- get true old_date (i.e. whether the account/review was expired when it was extended)
		update @tv_credit_history set tv_true_old_date = tv_old_date
		update @tv_credit_history set tv_true_old_date = tv_date_extended where tv_date_extended > tv_old_date

		-- months extended
		update @tv_credit_history set tv_months_extended = (select DATEDIFF(MONTH, tv_true_old_date, tv_new_date))

		-- add the total cost
		update @tv_credit_history set tv_cost = tv_months_extended * @accountPrice
		where tv_type_extended_name = 'Account'
		update @tv_credit_history set tv_cost = tv_months_extended * @reviewPrice
		where tv_type_extended_name = 'Review'

		set @totalUsed = (SELECT SUM(tv_cost)
			FROM @tv_credit_history)

		set @remainingCredit = @creditPurchased - @totalUsed

		update @tv_credit_purchases set tv_credit_remaining = @remainingCredit
		where tv_credit_purchase_id = @WORKING_CREDIT_PURCHASE_ID

		delete from @tv_credit_history

		set @totalUsed = 0
		set @remainingCredit = 0
		set @creditPurchased = 0

		FETCH NEXT FROM CREDIT_PURCHASE_ID_CURSOR 
		INTO @WORKING_CREDIT_PURCHASE_ID
	END

	CLOSE CREDIT_PURCHASE_ID_CURSOR
	DEALLOCATE CREDIT_PURCHASE_ID_CURSOR

	select * from @tv_credit_purchases
       
END



GO
/****** Object:  StoredProcedure [dbo].[st_CreditPurchasesGetAll]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_CreditPurchasesGetAll] 
(
	@TEXT_BOX nvarchar(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select cp.CREDIT_PURCHASE_ID, cp.DATE_PURCHASED, cp.PURCHASER_CONTACT_ID, c.CONTACT_NAME, 
	c.EMAIL, cp.CREDIT_PURCHASED, cp.PURCHASE_TYPE 
	from TB_CREDIT_PURCHASE cp
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = cp.PURCHASER_CONTACT_ID
	where ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
			(c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
			(c.EMAIL like '%' + @TEXT_BOX + '%'))

	group by cp.CREDIT_PURCHASE_ID, cp.DATE_PURCHASED, cp.PURCHASER_CONTACT_ID, 
	c.CONTACT_NAME, cp.CREDIT_PURCHASED, c.EMAIL, cp.PURCHASE_TYPE
       
END



GO
/****** Object:  StoredProcedure [dbo].[st_DbVersionAdd]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_DbVersionAdd]
(
	@VERSION_NUMBER int 
)	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO [TB_DB_VERSION]
           ([VERSION_NUMBER], [DATE_APPLIED])
     VALUES
           (@VERSION_NUMBER, getdate())

END

GO
/****** Object:  StoredProcedure [dbo].[st_DbVersionGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_DbVersionGet] 
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select top(1) * from TB_DB_VERSION order by VERSION_ID desc
END

GO
/****** Object:  StoredProcedure [dbo].[st_DetailedExtensionRecordGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_DetailedExtensionRecordGet] 
(
	@r int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
		select c.CONTACT_NAME, c.CONTACT_ID, c.EMAIL, ed.*, et.EXTENSION_TYPE 
		 , case 
			when datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) < datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE) 
			then datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) 
			else datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE)
			END
			AS [Months_Ext]
		 , case 
			when datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) < datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE) 
			then datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) * 10
			else datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE) * 10
			END
			AS [Ј]
		from Reviewer.dbo.TB_REVIEW_CONTACT rc 
		inner join Reviewer.dbo.TB_CONTACT c on rc.REVIEW_ID = @r and rc.CONTACT_ID = c.CONTACT_ID
		inner join TB_EXPIRY_EDIT_LOG ed on ed.ID_EXTENDED = c.CONTACT_ID and EXTENSION_TYPE_ID !=19
		inner join TB_EXTENSION_TYPES et on ed.TYPE_EXTENDED = 1 and et.EXTENSION_TYPE_ID = ed.EXTENSION_TYPE_ID
		order by DATE_OF_EDIT


		select r.REVIEW_NAME, r.REVIEW_ID, ed.*, et.EXTENSION_TYPE
		 , case 
			when datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) < datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE) 
			then datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) 
			else datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE)
			END
			AS [MonthsExt]
		 , case 
			when datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) < datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE) 
			then datediff(month, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE) * 35
			else datediff(month, DATE_OF_EDIT, NEW_EXPIRY_DATE) * 35
			END
			AS [Ј]
		from Reviewer.dbo.TB_REVIEW r
		inner join TB_EXPIRY_EDIT_LOG ed on ed.TYPE_EXTENDED = 0 and r.REVIEW_ID = @r and ed.ID_EXTENDED = r.REVIEW_ID and EXTENSION_TYPE_ID !=19
		inner join TB_EXTENSION_TYPES et on et.EXTENSION_TYPE_ID = ed.EXTENSION_TYPE_ID
		order by DATE_OF_EDIT

	RETURN

END




GO
/****** Object:  StoredProcedure [dbo].[st_DNNContactDetails]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_DNNContactDetails] 
(
	@Username nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select top 1 c.CONTACT_ID
    , c.old_contact_id
    , c.CONTACT_NAME
    , c.USERNAME
    --, c.[PASSWORD]
    , c.EMAIL
    , lt.CREATED as LAST_LOGIN
    , lt.LAST_RENEWED as LAST_ACTIVITY
    , c.DATE_CREATED
    , c.[EXPIRY_DATE]
    --, c.MONTHS_CREDIT
    --, c.CREATOR_ID
    , c.[TYPE]
    , c.IS_SITE_ADMIN
	from Reviewer.dbo.TB_CONTACT c
	left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
	on c.CONTACT_ID = lt.CONTACT_ID
	where c.USERNAME = @Username
	Order by LAST_ACTIVITY

	RETURN

END

GO
/****** Object:  StoredProcedure [dbo].[st_EmailGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER PROCEDURE [dbo].[st_EmailGet] 
(
	@EMAIL_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	select EMAIL_NAME, EMAIL_MESSAGE from TB_MANAGMENT_EMAILS
	where EMAIL_ID = @EMAIL_ID
    	
	RETURN
END



GO
/****** Object:  StoredProcedure [dbo].[st_EmailsGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER PROCEDURE [dbo].[st_EmailsGet] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	select EMAIL_ID, EMAIL_NAME 
	from TB_MANAGMENT_EMAILS

    	
	RETURN
END




GO
/****** Object:  StoredProcedure [dbo].[st_EmailUpdate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER   PROCEDURE [dbo].[st_EmailUpdate]
(
	@EMAIL_ID int,
	@EMAIL_NAME nvarchar(50),
	@EMAIL_MESSAGE nvarchar(max)
)
As
SET NOCOUNT ON


		update TB_MANAGMENT_EMAILS
		set EMAIL_MESSAGE = @EMAIL_MESSAGE, EMAIL_NAME = @EMAIL_NAME
		where EMAIL_ID = @EMAIL_ID 


SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[st_EnableMag]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER   procedure [dbo].[st_EnableMag]
(
	@REVIEW_ID int,
	@SETTING int
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	
	update Reviewer.dbo.TB_REVIEW
	set MAG_ENABLED = @SETTING
	where REVIEW_ID = @REVIEW_ID
	

END
GO
/****** Object:  StoredProcedure [dbo].[st_Eppi_Vis_Get_All]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_Eppi_Vis_Get_All] 
	-- Add the parameters for the stored procedure here
(
	@TEXT_BOX nvarchar(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 
	declare @tb_eppi_vis table (tv_webdb_id int, tv_webdb_name nvarchar(1000),
		tv_review_id int, tv_review_name nvarchar(1000), tv_contact_id int,
		tv_contact_name nvarchar(255), tv_is_open bit)

	insert into @tb_eppi_vis (tv_webdb_id, tv_webdb_name, tv_review_id, tv_contact_id, tv_is_open)
	select WEBDB_ID, WEBDB_NAME, REVIEW_ID, CREATED_BY, IS_OPEN from Reviewer.dbo.TB_WEBDB

	update @tb_eppi_vis
	set tv_contact_name = CONTACT_NAME
	from Reviewer.dbo.TB_CONTACT
	where tv_contact_id = CONTACT_ID 

	update @tb_eppi_vis
	set tv_review_name = REVIEW_NAME
	from Reviewer.dbo.TB_REVIEW
	where tv_review_id = REVIEW_ID 



	select * from @tb_eppi_vis
	where ((tv_webdb_id like '%' + @TEXT_BOX + '%') OR
					(tv_webdb_name like '%' + @TEXT_BOX + '%') OR
					(tv_review_name like '%' + @TEXT_BOX + '%') OR
					(tv_contact_name like '%' + @TEXT_BOX + '%')OR
					(tv_review_id like '%' + @TEXT_BOX + '%'))


       
END





GO
/****** Object:  StoredProcedure [dbo].[st_Eppi_Vis_Get_Log]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER       PROCEDURE [dbo].[st_Eppi_Vis_Get_Log] 
	-- Add the parameters for the stored procedure here
(
	@WEBDB_ID int,
	@FROM datetime,
	@UNTIL datetime,
	@TYPE nvarchar(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 
	declare @tb_eppi_vis_log table (tv_webdb_log_identity int, tv_created datetime,
		tv_log_type nvarchar(25), tv_details nvarchar(max))

	if @UNTIL = '1980/01/01 00:00:00'
	begin
		insert into @tb_eppi_vis_log (tv_webdb_log_identity, tv_created, tv_log_type, tv_details)
		select top 5000 WEBDB_LOG_IDENTITY, CREATED, LOG_TYPE, DETAILS from TB_WEBDB_LOG
		where WEBDB_ID = @WEBDB_ID
		and CREATED >= @FROM
	end
	else
	begin
		insert into @tb_eppi_vis_log (tv_webdb_log_identity, tv_created, tv_log_type, tv_details)
		select top 5000 WEBDB_LOG_IDENTITY, CREATED, LOG_TYPE, DETAILS from TB_WEBDB_LOG
		where WEBDB_ID = @WEBDB_ID
		and CREATED >= @FROM
		and CREATED <= @UNTIL
	end

	if @TYPE = 'All'
	begin
		select * from @tb_eppi_vis_log 
		order by tv_created desc
	end
	else
	begin
		select * from @tb_eppi_vis_log 
		where tv_log_type like @TYPE
		order by tv_created desc
	end

	--select * from @tb_eppi_vis_log
	--where ((tv_log_type like '%' + @TEXT_BOX + '%') OR
	--		(tv_details like '%' + @TEXT_BOX + '%'))


         
END



GO
/****** Object:  StoredProcedure [dbo].[st_EPPI_Vis_Name_and_Review]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER   procedure [dbo].[st_EPPI_Vis_Name_and_Review]
(
	@WEBDB_ID nvarchar(10),
	@WEBDB_NAME nvarchar(200) output,
	@REVIEW_NAME nvarchar(200) output,
	@REVIEW_ID nvarchar(10) output
)

As

SET NOCOUNT ON

	set @WEBDB_NAME = (select WEBDB_NAME from Reviewer.dbo.TB_WEBDB where WEBDB_ID = @WEBDB_ID)
	set @REVIEW_ID = (select REVIEW_ID from Reviewer.dbo.TB_WEBDB where WEBDB_ID = @WEBDB_ID)
	set @REVIEW_NAME = (select REVIEW_NAME from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @REVIEW_ID)



SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[st_ExpiryDateBulkAdjust]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER procedure [dbo].[st_ExpiryDateBulkAdjust]
(
	@ADD_OR_REMOVE nvarchar(10),
	@NUMBER_DAYS int,
	@DATE date,
	@CONTACT_ID int,
	@EXTENSION_NOTES nvarchar(500),
	@RESULT_C nvarchar(50) output,
	@RESULT_R nvarchar(50) output,
	@RESULT_SL nvarchar (50) output
)

As

SET NOCOUNT ON
declare @COST int
declare @ACCOUNT_COST int

CREATE TABLE #EXPIRY_TABLE (
DATE_OF_EDIT datetime,
TYPE_EXTENDED char(1),
ID_EXTENDED int,
NEW_EXPIRY_DATE date,
OLD_EXPIRY_DATE date,
EXTENDED_BY_ID int,
EXTENSION_TYPE_ID int,
EXTENSION_NOTES nvarchar(500))

CREATE TABLE #EXPIRY_TABLE_SITE_LIC (
SITE_LIC_ID int,
SITE_LIC_DETAILS_ID int,
EXTENDED_BY_ID int,
ID_EXTENDED int,
CHANGE_TYPE nvarchar(50),
DATE_OF_EDIT datetime,
REASON nvarchar(2000))


if @ADD_OR_REMOVE = 'Remove'
begin
	set @NUMBER_DAYS = @NUMBER_DAYS * -1
end


BEGIN TRY
BEGIN TRANSACTION

	-- Contacts --------------------------------------------------------- TYPE_EXTENDED = 1
	insert into #EXPIRY_TABLE (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE, OLD_EXPIRY_DATE,
		EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
	select GETDATE(), 1, c.CONTACT_ID, DATEADD(day, @NUMBER_DAYS, EXPIRY_DATE), EXPIRY_DATE,
		@CONTACT_ID, 18, @EXTENSION_NOTES 
	from Reviewer.dbo.TB_CONTACT c
	where c.EXPIRY_DATE >= @DATE
	
	update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(day, @NUMBER_DAYS, EXPIRY_DATE) 
	where EXPIRY_DATE >= @DATE
	
	insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
		OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
	select * from #EXPIRY_TABLE
	
	select * from #EXPIRY_TABLE
	set @RESULT_C = @@ROWCOUNT
	
	delete from #EXPIRY_TABLE
	
	
	-- Reviews ---------------------------------------------------------- TYPE_EXTENDED = 0
	insert into #EXPIRY_TABLE (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE, OLD_EXPIRY_DATE,
		EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
	select GETDATE(), 0, r.REVIEW_ID, DATEADD(day, @NUMBER_DAYS, EXPIRY_DATE), EXPIRY_DATE,
		@CONTACT_ID, 18, @EXTENSION_NOTES 
	from Reviewer.dbo.TB_REVIEW r
	where r.EXPIRY_DATE >= @DATE
	
	update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(day, @NUMBER_DAYS, EXPIRY_DATE) 
	where EXPIRY_DATE >= @DATE
	
	insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
		OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
	select * from #EXPIRY_TABLE		
	
	select * from #EXPIRY_TABLE
	set @RESULT_R = @@ROWCOUNT
	
	delete from #EXPIRY_TABLE
	
	-- Site License -- put an entry into both TB_EXPIRY_EDIT_LOG and TB_SITE_LIC_LOG ------------------------
	insert into #EXPIRY_TABLE_SITE_LIC (SITE_LIC_ID, SITE_LIC_DETAILS_ID, EXTENDED_BY_ID, ID_EXTENDED, CHANGE_TYPE,
		DATE_OF_EDIT, REASON)
	select s_l.SITE_LIC_ID, null, @CONTACT_ID, s_l.SITE_LIC_ID, 'Compensation for network down time', GETDATE(), @EXTENSION_NOTES
	from Reviewer.dbo.TB_SITE_LIC s_l
	where s_l.EXPIRY_DATE >= @DATE
	
	update #EXPIRY_TABLE_SITE_LIC
	set SITE_LIC_DETAILS_ID = 
	(select top 1 ld.SITE_LIC_DETAILS_ID from TB_SITE_LIC_DETAILS ld 
        inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and ld.VALID_FROM is not null
        and fs.IS_ACTIVE = 0 and ld.SITE_LIC_ID = #EXPIRY_TABLE_SITE_LIC.SITE_LIC_ID
        order by ld.VALID_FROM desc)
    where #EXPIRY_TABLE_SITE_LIC.SITE_LIC_DETAILS_ID is null
    
    
    insert into #EXPIRY_TABLE (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE, OLD_EXPIRY_DATE,
		EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
	select GETDATE(), 2, s_l.SITE_LIC_ID, DATEADD(day, @NUMBER_DAYS, EXPIRY_DATE), EXPIRY_DATE,
		@CONTACT_ID, 18, @EXTENSION_NOTES 
	from Reviewer.dbo.TB_SITE_LIC s_l
	where s_l.EXPIRY_DATE >= @DATE
	
	update #EXPIRY_TABLE
	set ID_EXTENDED = 
	(select #EXPIRY_TABLE_SITE_LIC.SITE_LIC_DETAILS_ID from #EXPIRY_TABLE_SITE_LIC 
	where #EXPIRY_TABLE_SITE_LIC.SITE_LIC_ID = #EXPIRY_TABLE.ID_EXTENDED)
	
	update Reviewer.dbo.TB_SITE_LIC set EXPIRY_DATE = DATEADD(day, @NUMBER_DAYS, EXPIRY_DATE) 
	where EXPIRY_DATE >= @DATE
	
	insert into TB_SITE_LIC_LOG (SITE_LIC_DETAILS_ID, CONTACT_ID, AFFECTED_ID, CHANGE_TYPE,
		DATE, REASON)
	select SITE_LIC_DETAILS_ID, EXTENDED_BY_ID, ID_EXTENDED, CHANGE_TYPE,
		DATE_OF_EDIT, REASON
	from #EXPIRY_TABLE_SITE_LIC
	
	insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
		OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
	select * from #EXPIRY_TABLE	
	
	select * from #EXPIRY_TABLE_SITE_LIC
	set @RESULT_SL = @@ROWCOUNT

	drop table #EXPIRY_TABLE_SITE_LIC
	drop table #EXPIRY_TABLE

COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT_C = 'Invalid'
end
END CATCH

RETURN

SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[st_ExtensionRecordGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ExtensionRecordGet] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select e_e_l.EXPIRY_EDIT_ID ,e_e_l.DATE_OF_EDIT, 
	--convert(date, e_e_l.OLD_EXPIRY_DATE), convert (date, e_e_l.NEW_EXPIRY_DATE), 
	e_e_l.OLD_EXPIRY_DATE, e_e_l.NEW_EXPIRY_DATE,
	e_e_l.EXTENDED_BY_ID, c.CONTACT_NAME, e_t.EXTENSION_TYPE, 
	e_e_l.EXTENSION_NOTES
	from TB_EXPIRY_EDIT_LOG e_e_l
	inner join TB_EXTENSION_TYPES e_t on e_t.EXTENSION_TYPE_ID = e_e_l.EXTENSION_TYPE_ID
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = e_e_l.EXTENDED_BY_ID
	where e_e_l.ID_EXTENDED = @CONTACT_ID

	RETURN

END


GO
/****** Object:  StoredProcedure [dbo].[st_ExtensionRecordGet_1]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ExtensionRecordGet_1] 
(
	@CONTACT_ID nvarchar(50),
	@TYPE_EXTENDED char(1)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select e_e_l.EXPIRY_EDIT_ID ,e_e_l.DATE_OF_EDIT, 
	--convert(date, e_e_l.OLD_EXPIRY_DATE), convert (date, e_e_l.NEW_EXPIRY_DATE), 
	e_e_l.OLD_EXPIRY_DATE, e_e_l.NEW_EXPIRY_DATE,
	e_e_l.EXTENDED_BY_ID, c.CONTACT_NAME, e_t.EXTENSION_TYPE, 
	e_e_l.EXTENSION_NOTES
	from TB_EXPIRY_EDIT_LOG e_e_l
	inner join TB_EXTENSION_TYPES e_t on e_t.EXTENSION_TYPE_ID = e_e_l.EXTENSION_TYPE_ID
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = e_e_l.EXTENDED_BY_ID
	where e_e_l.ID_EXTENDED = @CONTACT_ID
	and e_e_l.TYPE_EXTENDED = @TYPE_EXTENDED

	RETURN

END



GO
/****** Object:  StoredProcedure [dbo].[st_ExtensionTypesGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ExtensionTypesGet] 
(
	@APPLIES_TO nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    if @APPLIES_TO = 'Contact'
    begin
		select * from TB_EXTENSION_TYPES 
		where (APPLIES_TO = '100' or APPLIES_TO = '111')
		order by [ORDER]
    end
    
    if @APPLIES_TO = 'Review'
    begin
		select * from TB_EXTENSION_TYPES 
		where (APPLIES_TO = '010' or APPLIES_TO = '111')
		order by [ORDER]
    end
    
    if @APPLIES_TO = 'SiteLic'
    begin
		select * from TB_EXTENSION_TYPES 
		where (APPLIES_TO = '001' or APPLIES_TO = '111')
		order by [ORDER]
    end

	RETURN

END


GO
/****** Object:  StoredProcedure [dbo].[st_GetHelpText]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_GetHelpText] 
(
	@HELP_NAME nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select * from TB_MANAGMENT_EMAILS 
	where EMAIL_NAME = @HELP_NAME


	RETURN

END



GO
/****** Object:  StoredProcedure [dbo].[st_GetLatestUpdateMsg]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_GetLatestUpdateMsg] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select top 1 * from TB_UPDATE_MSG 
	order by UPDATE_MSG_ID desc
END

GO
/****** Object:  StoredProcedure [dbo].[st_GetReviewOwner]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_GetReviewOwner] 
(
	@REVIEW_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select c.CONTACT_ID, c.CONTACT_NAME from Reviewer.dbo.TB_CONTACT c
    inner join Reviewer.dbo.TB_REVIEW r on r.FUNDER_ID = c.CONTACT_ID
    where r.REVIEW_ID = @REVIEW_ID

	RETURN

END



GO
/****** Object:  StoredProcedure [dbo].[st_GhostContactActivate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[st_GhostContactActivate]
(
	@CONTACT_NAME nvarchar(255), 
	@USERNAME nvarchar(50), 
	@PASSWORD varchar(50), 
	@EMAIL nvarchar(500),
	@DESCRIPTION nvarchar(1000),
	@CONTACT_ID int,
	@UID uniqueidentifier,
	@RES nvarchar(50) = 'Done' output
)
AS
	declare @chk int = 0
	select @RES = 'Done'
	select @chk = COUNT(CONTACT_ID) from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID
	if @chk = 0
	begin
		select @RES = 'User does not exist'
		return
	end
	
	select @chk = COUNT(CONTACT_ID) from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID 
					and (EXPIRY_DATE is null OR PWASHED is null)
	if @chk != 1
	begin
		select @RES = 'Not a ghost account'
		return
	end
	
	--create salt!
	DECLARE @chars char(100) = '!т#$%&а()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~ийклм'
	declare @rnd varchar(20)
	declare @cnt int = 0
	set @rnd = ''
	WHILE (@cnt <= 20) 
	BEGIN
		SELECT @rnd = @rnd + 
			SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
		SELECT @cnt = @cnt + 1
	END
	update Reviewer.dbo.TB_CONTACT set 
		[EMAIL] = @EMAIL 
		,CONTACT_NAME = @CONTACT_NAME
		,USERNAME = @USERNAME
		,[DESCRIPTION] = @DESCRIPTION
		,PWASHED = HASHBYTES('SHA1', @PASSWORD + @rnd)
		, FLAVOUR = @rnd
	where CONTACT_ID = @CONTACT_ID	
	
	EXEC dbo.st_ContactActivate	@CONTACT_ID --activate the account by setting the expiry date
	
	--check if this worked
	Select @chk = COUNT(contact_id) from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID and EXPIRY_DATE is not null
	if @chk != 1
	BEGIN --not good
		select @RES = 'Tried to activate, but something didn''t work'
		update TB_CHECKLINK set IS_STALE = 1, WAS_SUCCESS = 0, DATE_USED = GETDATE() where CONTACT_ID = @CONTACT_ID and CHECKLINK_UID = @UID
	END
	ELSE
	Begin
		update TB_CHECKLINK set IS_STALE = 1, WAS_SUCCESS = 1, DATE_USED = GETDATE() where CONTACT_ID = @CONTACT_ID and CHECKLINK_UID = @UID
	END
	
	RETURN

GO
/****** Object:  StoredProcedure [dbo].[st_GhostContactActivateRevoke]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_GhostContactActivateRevoke]
	-- Add the parameters for the stored procedure here
	@CONTACT_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Update TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE() where 
		IS_STALE != 1 and CONTACT_ID = @CONTACT_ID and [TYPE] = 'ActivateGhost'
	if @@ROWCOUNT > 0
	begin
		Update Reviewer.dbo.TB_CONTACT set EMAIL = null where CONTACT_ID = @CONTACT_ID and FLAVOUR is null and EXPIRY_DATE is null
	end
END

GO
/****** Object:  StoredProcedure [dbo].[st_GhostReviewActivate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_GhostReviewActivate]
	-- Add the parameters for the stored procedure here
	@revID int,
	@Name Nvarchar(1000) = ''
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	declare @funderID int
	declare @months_credit smallint

    -- Insert statements for procedure here
	if @Name = ''
	begin
		update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(month,MONTHS_CREDIT, GETDATE())
		,REVIEW_NAME = 'Please Edit (ID=' & @revID & ')' where REVIEW_ID = @revID 
	end
	else
	begin
		update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(month,MONTHS_CREDIT, GETDATE())
			,REVIEW_NAME = @Name where REVIEW_ID = @revID 
	end
	
	-- retrieve and then zero the months credit
	select @months_credit = MONTHS_CREDIT from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @revID 
	update Reviewer.dbo.TB_REVIEW set MONTHS_CREDIT = 0 where REVIEW_ID = @revID 

	select @funderID = FUNDER_ID from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @revID

	-- add a line to TB_EXPIRY_EDIT_LOG to say the review has been activated
	insert into ReviewerAdmin.dbo.TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, OLD_EXPIRY_DATE,
		NEW_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
	values (GETDATE(), 0, @revID, null, DATEADD(month, @months_credit, GETDATE()), 
		@funderID, 10, 'The FunderID activated the review') 
		
	
	
END

GO
/****** Object:  StoredProcedure [dbo].[st_InsertLatestUpdateMsg]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_InsertLatestUpdateMsg]
	-- Add the parameters for the stored procedure here
	@Ver nvarchar(15) 
	,@Description nvarchar(500) 
	,@url nvarchar(160)
	,@cid int 
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    --first, check that the contact id provided is a siteadmin (small additional precaution to make mischief a little more difficult)
    declare @chk int = 0
    set @chk = (select COUNT(CONTACT_ID) from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @cid and IS_SITE_ADMIN = 1)
    if @chk != 1 return 
    
	INSERT INTO [TB_UPDATE_MSG]
           (
           [VERSION_NUMBER]
           ,[DESCRIPTION]
           ,[URL]
           )
     VALUES
           (@Ver
           ,@Description
           ,@url
           )
END

GO
/****** Object:  StoredProcedure [dbo].[st_ItemDocumentBinInsertFromER3]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER procedure [dbo].[st_ItemDocumentBinInsertFromER3]
(
	@OLD_ITEM_ID NVARCHAR(50),
	@DESTINATION_REVIEW_ID int,
	@DOCUMENT_TITLE NVARCHAR(255),
	@DOCUMENT_EXTENSION NVARCHAR(5),
	@BIN IMAGE
	--,@DOCUMENT_TEXT NVARCHAR(MAX)
)

As

SET NOCOUNT ON

DECLARE @ITEM_ID bigint = 0

--set @ITEM_ID = (select ITEM_ID from Reviewer.dbo.TB_ITEM where OLD_ITEM_ID = @OLD_ITEM_ID)
set @ITEM_ID = (select i.ITEM_ID from Reviewer.dbo.TB_ITEM i
inner join Reviewer.dbo.TB_ITEM_REVIEW i_r on i_r.ITEM_ID = i.ITEM_ID
where i_r.REVIEW_ID = @DESTINATION_REVIEW_ID
and i.OLD_ITEM_ID = @OLD_ITEM_ID)

if @ITEM_ID != 0
begin
--SET @DOCUMENT_TEXT = replace(@DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10))
INSERT INTO Reviewer.dbo.TB_ITEM_DOCUMENT(ITEM_ID, DOCUMENT_TITLE, DOCUMENT_EXTENSION, DOCUMENT_BINARY/*, DOCUMENT_TEXT*/)
VALUES(@ITEM_ID, @DOCUMENT_TITLE, @DOCUMENT_EXTENSION, @BIN/*, Reviewer.[dbo].fn_CLEAN_SIMPLE_TEXT(@DOCUMENT_TEXT)*/)
end

SET NOCOUNT OFF


/****** Object:  StoredProcedure [dbo].[st_SearchDelete]    Script Date: 07/20/2009 21:32:31 ******/
SET ANSI_NULLS ON


GO
/****** Object:  StoredProcedure [dbo].[st_LinkCodeset]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE OR ALTER procedure [dbo].[st_LinkCodeset]
(
	@SOURCE_SET_ID int,
	@SOURCE_REVIEW_ID int,
	@DESTINATION_REVIEW_ID int,
	@CONTACT_ID int,
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON

	declare @SOURCE_SET_NAME nvarchar (255)

BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION

	-- find the number of codesets in the review so we can place the new one at the bottom.
	declare @number_sets int set @number_sets = 0
	select @number_sets = COUNT(*) from Reviewer.dbo.TB_REVIEW_SET
	where REVIEW_ID = @DESTINATION_REVIEW_ID

	insert into Reviewer.dbo.TB_REVIEW_SET (REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL, SET_ORDER)
	values (@DESTINATION_REVIEW_ID, @SOURCE_SET_ID, 0, 1, @number_sets - 1)

	-- prefix the new codeset with 'LINKED - ' unless it already is.
	--  This will show up in all reviews but at least we will know
	--  the codeset is linked
	declare @IsLink nvarchar(6) set @IsLink = null 
	select @IsLink = substring (SET_NAME, 0, 6) 
	from Reviewer.dbo.TB_SET
	where SET_ID = @SOURCE_SET_ID

	if @IsLink != 'LINKED'
	begin
		update Reviewer.dbo.TB_SET
		set SET_NAME = 'LINKED - ' + SET_NAME
		where SET_ID = @SOURCE_SET_ID
	end
	

COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to link codeset'
end
END CATCH


RETURN

SET NOCOUNT OFF



GO
/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Check_Expiration]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sergio
-- ALTER date: 08/03/10
-- Description:	check ticket validity: checks if another more recent ticket is present and if current ticket is expired
-- Description: if ticket is valid retrieves also the latest message from the server
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_LogonTicket_Check_Expiration] 
	-- Add the parameters for the stored procedure here
	@guid uniqueidentifier, 
	@c_ID int,
	@result nvarchar(9) OUTPUT,
	@message nvarchar(1000) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @RN_tm datetime2(0), @RC int
	DECLARE @myT TABLE 
( 
    RN datetime2(0),
    TK uniqueidentifier
) 
    set @message = ''
    -- to check for all possible situations, we get all valid tickets for user, ignoring the GUID
	insert into @myT 
		SELECT LAST_RENEWED, TICKET_GUID from TB_LOGON_TICKET WHERE CONTACT_ID = @c_ID AND State=1
	SET @RC = @@ROWCOUNT
	IF @RC = 0 --user does not have any valid ticket
	BEGIN
		SET @result = 'None'
	END
	
	ELSE IF @RC = 1 --just one found, so far so good!
	Begin
		IF @guid = (SELECT TK from @myT) --is the ticket the one the user sent us? (check GUID)
		BEGIN --the GUID is the right one, let's see if it's still valid (= not expired)
			--SET @RN_tm = 
			IF (SELECT RN from @myT) > DATEADD(HH, -3, GETDATE())
			BEGIN
				SET @result = 'Valid' --all is well, get latest message from server
				SET @message = (SELECT top 1 MESSAGE from TB_LATEST_SERVER_MESSAGE)
			END
			ELSE
			BEGIN
				SET @result = 'Expired' 
				--ticket is too old, set ticket state to FALSE
				UPDATE TB_LOGON_TICKET SET State=0 WHERE TICKET_GUID = @guid
			END
		END
		ELSE
		BEGIN
		--GUID didn't match, same credentials have been used to log on somewhere else
			SET @result = 'Invalid'
		END
	END
	ELSE IF @RC > 1 --for some reason, more than one valid ticket exist for current user
	BEGIN
		-- this shouldn't happen: invalidate ALL tickets
		UPDATE TB_LOGON_TICKET SET State=0
		WHERE CONTACT_ID = @c_ID AND State=1
		SET @result = 'Multiple'
	END
END
GO
/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Check_RI]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Sergio>
-- ALTER date: <05/03/10>
-- Description:	<Updates LAST_RENEWED on a valid ticket, used to make sure only one user is logged with the same credentials and to renew a ticket validity>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_LogonTicket_Check_RI] 
	-- Add the parameters for the stored procedure here
	@guid uniqueidentifier, 
	@c_ID int,
	@ROWS int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	UPDATE TB_LOGON_TICKET SET LAST_RENEWED = GETDATE() 
	WHERE TICKET_GUID =@guid AND CONTACT_ID = @c_ID AND State=1
	set @ROWS = @@ROWCOUNT
END
GO
/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Check_Ticket]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_LogonTicket_Check_Ticket]
	-- Add the parameters for the stored procedure here
	@guid uniqueidentifier, 
	@c_ID int,
	@RID int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select @RID = 0
	select @RID = l.REVIEW_ID from TB_LOGON_TICKET l where l.TICKET_GUID = @guid and l.STATE = 1 and l.CONTACT_ID = @c_ID
	if @RID is null select @RID = 0
END

GO
/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Check_UserIsLogged]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_LogonTicket_Check_UserIsLogged]
	-- Add the parameters for the stored procedure here
	@c_ID int
	,@RES int output --1 if user is logged
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT @RES = COUNT(CONTACT_ID) from TB_LOGON_TICKET 
		where @c_ID = CONTACT_ID 
		and LAST_RENEWED > DATEADD(HH, -3, GETDATE())
		and STATE = 1
	
END

GO
/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Get_UserID_FROM_GUID]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Sergio>
-- ALTER date: <05/03/10>
-- Description:	
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_LogonTicket_Get_UserID_FROM_GUID] 
	-- Add the parameters for the stored procedure here
	@guid uniqueidentifier 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @ROWS int
    -- Insert statements for procedure here
	UPDATE TB_LOGON_TICKET SET LAST_RENEWED = GETDATE() 
	WHERE TICKET_GUID =@guid AND State=1 and LAST_RENEWED >= DATEADD(HH, -3, GETDATE())
	set @ROWS = @@ROWCOUNT
	if @ROWS = 1
	Begin
		Select CONTACT_ID, Review_id from TB_LOGON_TICKET WHERE TICKET_GUID =@guid AND State=1
	END	
END
GO
/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Insert]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sergio
-- ALTER date: 
-- Description:	ALTER a new ticket, relies on default values of most of the columns
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_LogonTicket_Insert] 
	-- Add the parameters for the stored procedure here
	@Contact_ID int = 0, 
	@Review_ID int = 0,
	@Client nvarchar(10) = 'ER4',
	@GUID uniqueidentifier OUTPUT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here
    set @GUID = newid()
    UPDATE TB_LOGON_TICKET
    SET STATE = 0
    WHERE CONTACT_ID = @Contact_ID AND STATE = 1
    INSERT into TB_LOGON_TICKET(TICKET_GUID, CONTACT_ID, REVIEW_ID, CLIENT)
	VALUES (@GUID, @Contact_ID, @Review_ID, @Client)
	
END
GO
/****** Object:  StoredProcedure [dbo].[st_ManagementSettings]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jeff>
-- ALTER date: <>
-- Description:	<>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ManagementSettings] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

		select * from TB_MANAGEMENT_SETTINGS 
    	
	RETURN
END
GO
/****** Object:  StoredProcedure [dbo].[st_NewsletterContacts]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER PROCEDURE [dbo].[st_NewsletterContacts] 
(
	@TEST_SEND bit
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	-- We want the contacts that are eligible but have not been sent the newsletter yet
	declare @mostRecentNewsletterID int
	
	select top 1 @mostRecentNewsletterID = NEWSLETTER_ID from TB_NEWSLETTER
	order by NEWSLETTER_ID desc
	
	declare @tb_newsletter_recipients table (contact_id int)
	
	insert into @tb_newsletter_recipients (contact_id)
	select CONTACT_ID from TB_NEWSLETTER_CONTACT
	where NEWSLETTER_ID = @mostRecentNewsletterID
	
	if @TEST_SEND = 0
	BEGIN
		select CONTACT_ID, CONTACT_NAME, EMAIL from Reviewer.dbo.TB_CONTACT
		where SEND_NEWSLETTER = 1
		and CONTACT_ID not in (select contact_id from @tb_newsletter_recipients)
	END
	
	if @TEST_SEND = 1
	BEGIN
		select CONTACT_ID, CONTACT_NAME, EMAIL from Reviewer.dbo.TB_CONTACT
		where EMAIL = 'EPPIsupport@ioe.ac.uk'
	END
    	
	RETURN
END



GO
/****** Object:  StoredProcedure [dbo].[st_NewsletterContactUpdate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_NewsletterContactUpdate]
(
	@CONTACT_ID int
)

As

SET NOCOUNT ON

	declare @mostRecentNewsletterID int
	select top 1 @mostRecentNewsletterID = NEWSLETTER_ID from TB_NEWSLETTER
	order by NEWSLETTER_ID desc

	insert into TB_NEWSLETTER_CONTACT (NEWSLETTER_ID, CONTACT_ID)
	values (@mostRecentNewsletterID, @CONTACT_ID)

SET NOCOUNT OFF





GO
/****** Object:  StoredProcedure [dbo].[st_NewsletterDelay]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_NewsletterDelay]
(
	@DELAY nvarchar(50)
)
As

SET NOCOUNT ON

	declare @something int
	WAITFOR DELAY @DELAY;
	--WAITFOR DELAY '00:00:1.000';
	set @something = 1

SET NOCOUNT OFF






GO
/****** Object:  StoredProcedure [dbo].[st_NewsletterGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER PROCEDURE [dbo].[st_NewsletterGet] 
(
	@NEWSLETTER_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	declare @working_newsletter_id int
	set @working_newsletter_id = @NEWSLETTER_ID
	
	if @NEWSLETTER_ID = 0
	begin
		select top 1 @working_newsletter_id = NEWSLETTER_ID
		from TB_NEWSLETTER order by NEWSLETTER_ID desc
	end

	select NEWSLETTER, [YEAR], [MONTH] from TB_NEWSLETTER
	where NEWSLETTER_ID = @working_newsletter_id
    	
	RETURN
END


GO
/****** Object:  StoredProcedure [dbo].[st_NewslettersGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER PROCEDURE [dbo].[st_NewslettersGet] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    /*
    declare @EPPIsupportID int
	select @EPPIsupportID = CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = 'EPPIsupport@ioe.ac.uk'

	select n.NEWSLETTER_ID, n.[YEAR], n.[MONTH], COUNT(n_c.CONTACT_ID) as NUMBER_SENT  
	from TB_NEWSLETTER n
	left join TB_NEWSLETTER_CONTACT n_c on n_c.NEWSLETTER_ID = n.NEWSLETTER_ID
	where n_c.CONTACT_ID != @EPPIsupportID 
	group by n_c.NEWSLETTER_ID, n.NEWSLETTER_ID, n.YEAR, n.MONTH
	*/

	select n.NEWSLETTER_ID, n.[YEAR], n.[MONTH], COUNT(n_c.CONTACT_ID) as NUMBER_SENT  
	from TB_NEWSLETTER n
	left join TB_NEWSLETTER_CONTACT n_c on n_c.NEWSLETTER_ID = n.NEWSLETTER_ID
	group by n_c.NEWSLETTER_ID, n.NEWSLETTER_ID, n.YEAR, n.MONTH

    	
	RETURN
END



GO
/****** Object:  StoredProcedure [dbo].[st_NewsletterStatus]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER PROCEDURE [dbo].[st_NewsletterStatus] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @mostRecentNewsletterID int
	declare @numRecipients int
	declare @EPPIsupportID int
	declare @numToBeSent int
	
	select @EPPIsupportID = CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = 'EPPIsupport@ioe.ac.uk'
	
	select top 1 @mostRecentNewsletterID = NEWSLETTER_ID from TB_NEWSLETTER
	order by NEWSLETTER_ID desc
	
	select @numRecipients = COUNT(CONTACT_ID) from TB_NEWSLETTER_CONTACT
	where NEWSLETTER_ID = @mostRecentNewsletterID
	and CONTACT_ID != @EPPIsupportID
	and CONTACT_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where SEND_NEWSLETTER = 1)

	declare @tb_newsletter_status table (numToBeSent int, numEligible int, numRecipients int)

	insert into @tb_newsletter_status (numEligible)
	select COUNT(CONTACT_ID) from Reviewer.dbo.TB_CONTACT
	where SEND_NEWSLETTER = 1
	and CONTACT_ID != @EPPIsupportID

	update @tb_newsletter_status
	set numRecipients = @numRecipients
	
	update @tb_newsletter_status
	set numToBeSent = (select COUNT(CONTACT_ID) from Reviewer.dbo.TB_CONTACT
	where SEND_NEWSLETTER = 1
	and CONTACT_ID != @EPPIsupportID
	and CONTACT_ID not in (select CONTACT_ID from TB_NEWSLETTER_CONTACT
	where NEWSLETTER_ID = @mostRecentNewsletterID))

	select * from @tb_newsletter_status

    	
	RETURN
END




GO
/****** Object:  StoredProcedure [dbo].[st_NewsletterUpdate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER PROCEDURE [dbo].[st_NewsletterUpdate]
(
	@NEWSLETTER_ID int,
	@NEWSLETTER nvarchar(max)
)
As
SET NOCOUNT ON


		update TB_NEWSLETTER
		set NEWSLETTER = @NEWSLETTER
		where NEWSLETTER_ID = @NEWSLETTER_ID 


SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[st_NewsletterUpload]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[st_NewsletterUpload]
(
	@NEWSLETTER nvarchar(MAX),
	@YEAR int,
	@MONTH int,
	@RESULT nvarchar(50) output
)
AS

BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION

	set @RESULT = '0'
	declare @newsletter_count int
	select @newsletter_count = count(NEWSLETTER_ID) from TB_NEWSLETTER
	where [YEAR] = @YEAR and [MONTH] = @MONTH
	
	if @newsletter_count = 1
	begin
		update TB_NEWSLETTER
		set NEWSLETTER = @NEWSLETTER
		where [YEAR] = @YEAR and [MONTH] = @MONTH
	end
	else if @newsletter_count = 0
	begin
		insert into TB_NEWSLETTER ([YEAR], [MONTH], NEWSLETTER)
		values (@YEAR, @MONTH, @NEWSLETTER)
	end
	else if @newsletter_count > 1
	begin
		set @RESULT = 'Multiple newsletters exist for this date!'
	end
	
	
COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
end
END CATCH


RETURN

SET NOCOUNT OFF



GO
/****** Object:  StoredProcedure [dbo].[st_OnlineFeedbackCreate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER procedure [dbo].[st_OnlineFeedbackCreate]
(
	@ONLINE_FEEDBACK_ID int output,
	@CONTEXT nchar(100) null,
	@CONTACT_ID int,
	@REVIEW_ID int,
	@IS_ERROR bit,
	@MESSAGE nvarchar(4000)
)

As

SET NOCOUNT ON
INSERT INTO [dbo].[TB_ONLINE_FEEDBACK]
           ([CONTEXT]
           ,[CONTACT_ID]
		   ,[REVIEW_ID]
           ,[IS_ERROR]
           ,[MESSAGE]
           )
     VALUES
           (@CONTEXT
           ,@CONTACT_ID
		   ,@REVIEW_ID
           ,@IS_ERROR
           ,@MESSAGE)
SET  @ONLINE_FEEDBACK_ID = @@IDENTITY
SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[st_OnlineFeedbackList]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_OnlineFeedbackList]
(
	@MAX_LENGTH INT = 5000
)

As

SELECT top (@MAX_LENGTH) f.*, c.CONTACT_NAME, c.EMAIL from TB_ONLINE_FEEDBACK f
inner join Reviewer.dbo.TB_CONTACT c on f.CONTACT_ID = c.CONTACT_ID
order by f.DATE desc
GO
/****** Object:  StoredProcedure [dbo].[st_Organisation_Add_Remove_Admin]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Organisation_Add_Remove_Admin]
	-- Add the parameters for the stored procedure here
	@org_id int
	, @admin_ID int
	, @contact_email nvarchar(500)
	, @res int output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here
    declare @contact_id int
	set @res = 0
    
    -- initial check to see if email exists
	select @contact_id = CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = @contact_email and EXPIRY_DATE > '2010-03-20 00:00:01'
	if @@ROWCOUNT = 1 
	begin
		declare @ck int
		set @ck = (SELECT count(contact_id) from TB_ORGANISATION_ADMIN 
		where (CONTACT_ID = @admin_ID or @admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1
				and EXPIRY_DATE > '2010-03-20 00:00:01')) -- we only want ER4 accounts
				and ORGANISATION_ID = @org_id)
		if @ck < 1 set @res = -1 --first check: see if supplied admin is actually an admin!
		else if  (select count(contact_id) from Reviewer.dbo.TB_CONTACT where @contact_email = EMAIL 
			and @contact_id = CONTACT_ID
			and EXPIRY_DATE > '2010-03-20 00:00:01') != 1
			--second check, see if c_id and email exist
			set @res = -2
		else -- checks went OK, let's see if we can do it
		begin
			set @ck = (select count(contact_id) from TB_ORGANISATION_ADMIN where CONTACT_ID = @contact_id and ORGANISATION_ID = @org_id)
			if @ck = 1 -- contact is an admin, we should remove it
			begin
				delete from TB_ORGANISATION_ADMIN where CONTACT_ID = @contact_id and @org_id = ORGANISATION_ID
				if @@ROWCOUNT = 1
				begin --write success log
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'remove admin')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin'
					--	from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc
				end
				else --write failure log, if this is fired, there is a bug somewhere
				begin
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'remove admin: failed!')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin: failed!'
					--	from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc	
					set @res = -3
				end
			end	
			
			else --contact is not an admin, we should add it
			begin
				insert into TB_ORGANISATION_ADMIN (CONTACT_ID, ORGANISATION_ID)
				values (@contact_id, @org_id)
				if @@ROWCOUNT = 1
				begin
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'add admin')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin'
					--	from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc
				end
				else
				begin
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'add admin: failed!')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin: failed!'
					--	from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc	
					set @res = -4
				end
			end
		end
	end
	--select c.CONTACT_ID, CONTACT_NAME, EMAIL from TB_SITE_LIC_ADMIN a
	--	inner join Reviewer.dbo.TB_CONTACT c on a.CONTACT_ID = c.CONTACT_ID
	--where SITE_LIC_ID = @lic_id
	
	return @res 
	-- error codes: -1 = supplied admin_id is not an admin of this site lice
	--				-2 = contact_id already in a site license
	--				-3 = no allowance available, all account slots for current license have been used
	--				-4 = tried to remove account but couldn't write changes! BUG ALERT
	--				-5 = tried to add account but couldn't write changes! BUG ALERT
	--				-6 = email check returned no contact_id or multiple contact_ids
	
END
GO
/****** Object:  StoredProcedure [dbo].[st_Organisation_Add_Remove_Contact]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Organisation_Add_Remove_Contact]
(
	  @org_id int
	, @admin_ID int
	, @contact_email nvarchar(500)
	, @res int output
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	-- Insert statements for procedure here
	
	declare @contact_id int
	set @res = 0

	-- initial check to see if email exists
	select @contact_id = CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = @contact_email and EXPIRY_DATE > '2010-03-20 00:00:01'
	if @@ROWCOUNT = 1 
	begin
		declare @ck int, @ck2 int = 0, @acc_all int
		set @ck = (SELECT count(contact_id) from TB_ORGANISATION_ADMIN 
			where (CONTACT_ID = @admin_ID or @admin_ID in 
				(select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)) 
			and ORGANISATION_ID = @org_id)
		--set @acc_all = (select top 1 ACCOUNTS_ALLOWANCE from TB_SITE_LIC_DETAILS 
		--	where SITE_LIC_ID = @lic_id and VALID_FROM is not null order by VALID_FROM desc)
		--set @ck2 = (select @acc_all - COUNT(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT 
		--	where SITE_LIC_ID = @lic_id)
		if @ck < 1 set @res = -1 --first check: see if supplied admin id is actually an admin!
		
		--else if  (select count(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT 
		--	where @contact_id = CONTACT_ID and SITE_LIC_ID != @lic_id) != 0
		--	--second check, see if contact_id is already in a site license (not including this one)
		--	set @res = -2	
		
		else -- checks went OK, let's see if we can do it
		begin
			set @ck = (select count(contact_id) from TB_ORGANISATION_CONTACT 
				where CONTACT_ID = @contact_id
				and ORGANISATION_ID = @org_id)
			if @ck = 1 -- contact is in the license, we should remove it
			begin
				delete from TB_ORGANISATION_CONTACT where CONTACT_ID = @contact_id and @org_id = ORGANISATION_ID
				if @@ROWCOUNT = 1
				begin --write success log
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'remove contact')
					--) select @org_id, @admin_ID, @contact_id, 'remove contact'
					--	from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc
				end
				else --write failure log, if this is fired, there is a bug somewhere
				begin
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'remove contact: failed!')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove contact: failed!'
					--	from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc	
					set @res = -4
				end
			end	
			
			--else if @ck2 < 1 --accounts allowance is all used up
			--set @res = -3
			
			else --contact is not in the license, we should add it
			begin
				insert into TB_ORGANISATION_CONTACT (CONTACT_ID, ORGANISATION_ID)
				values (@contact_id, @org_id)
				if @@ROWCOUNT = 1
				begin
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'add contact')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add contact'
					--	from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc
				end
				else
				begin
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE])
					values (@org_id, @admin_ID, @contact_id, 'add contact: failed!')
					--) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add contact: failed!'
					--	from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
					--	order by DATE_CREATED desc	
					set @res = -5
				end
			end
		end
	end
	else
	begin
		set @res = -6
	end
	
	--select c.CONTACT_ID, CONTACT_NAME, EMAIL from Reviewer.dbo.TB_SITE_LIC_CONTACT lc 
	--	inner join Reviewer.dbo.TB_CONTACT c on lc.CONTACT_ID = c.CONTACT_ID
	--	where SITE_LIC_ID = @lic_id
	
	return @res
	-- error codes: -1 = supplied admin_id is not an admin of this site lice
	--				-2 = contact_id already in a site license
	--				-3 = no allowance available, all account slots for current license have been used
	--				-4 = tried to remove account but couldn't write changes! BUG ALERT
	--				-5 = tried to add account but couldn't write changes! BUG ALERT
	--				-6 = email check returned no contact_id or multiple contact_ids
END




GO
/****** Object:  StoredProcedure [dbo].[st_Organisation_Add_Review]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Organisation_Add_Review]
	-- Add the parameters for the stored procedure here
	@org_id int
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
		declare @ck int, @ck2 int, @org_det_id int
		set @ck = (SELECT count(contact_id) from TB_ORGANISATION_ADMIN 
		where (CONTACT_ID = @admin_ID or @admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT 
		where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)
			) and ORGANISATION_ID = @org_id)
		if @ck < 1 set @res = -1 --first check: see if supplied admin is actually and admin!
		
		else -- initial checks went OK, let's see if we can do it
		begin
			set @ck = (select count(review_id) from TB_ORGANISATION_REVIEW where REVIEW_ID = @review_id)
			--set @org_det_id = (SELECT TOP 1 ld.SITE_LIC_DETAILS_ID from Reviewer.dbo.TB_SITE_LIC sl 
			--						inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
			--						inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
			--						and sl.SITE_LIC_ID = @org_id
			--					Order by ld.VALID_FROM desc
			--					)
			--@ck2 counts how many reviews can be added to current lic
			--set @ck2 = (select d.REVIEWS_ALLOWANCE - count(review_id) from TB_SITE_LIC_DETAILS d inner join
			--				 Reviewer.dbo.TB_SITE_LIC_REVIEW lr on lr.SITE_LIC_DETAILS_ID = @org_det_id
			--					and lr.SITE_LIC_DETAILS_ID = d.SITE_LIC_DETAILS_ID
			--				 group by d.REVIEWS_ALLOWANCE
			--				 )--count how many reviews can still be added
			if @ck != 0 -- review is already in an organisation
			begin
				--set @ck = (select count(review_id) from TB_ORGANISATION_REVIEW 
				--    where REVIEW_ID = @review_id and ORGANISATION_ID = @org_id)
				--if @ck = 1 set @res = -3 --review already in this site_lic
				--else 
				set @res = -4 --review is already in an organisation
			end
			--else if @ck2 < 1 --no allowance available, all review slots for current license have been used
			--begin
			--	set @res = -5
			--end
			else
			begin --all is well, let's do something!
				begin transaction --make sure we don't commit only half of the mission critical data! 
				--(we assume the update below will work, only checking for the other statement)
				update Reviewer.dbo.TB_REVIEW set ORGANISATION_ID = @org_id
					where REVIEW_ID = @review_id
				insert into TB_ORGANISATION_REVIEW ([ORGANISATION_ID], [REVIEW_ID], [DATE_ADDED], [ADDED_BY]) 
					values (@org_id, @review_id, getdate(), @admin_ID)
				if @@ROWCOUNT = 1
				begin --write success log
					commit transaction --all is well, commit
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE]) 
					values (@org_id, @admin_ID, @review_id, 'add review')															
				end
				else --write failure log, if this is fired, there is a bug somewhere
				begin
					rollback transaction --BAD! something went wrong!
					insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE]) 
					values (@org_id, @admin_ID, @review_id, 'add review: failed!')	
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




GO
/****** Object:  StoredProcedure [dbo].[st_Organisation_Create_or_Edit]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Organisation_Create_or_Edit] 
	-- Add the parameters for the stored procedure here
(
	  @ORG_ID nvarchar(50)
	, @creator_id INT
	, @admin_id  int -- who will be the first administrator of the site lic, 
	, @ORGANISATION_NAME nvarchar(50)
	, @ORGANISATION_ADDRESS nvarchar(500)
	, @TELEPHONE nvarchar(50)
	, @NOTES nvarchar(4000)
	, @DATE_CREATED datetime
	, @RESULT nvarchar(50) output
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @ck int

	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
	BEGIN TRANSACTION
	if @ORG_ID = 'N/A' -- we are creating a new site license
	BEGIN		
		declare @O_ID int
		
		insert into TB_ORGANISATION (ORGANISATION_NAME, ORGANISATION_ADDRESS, 
			TELEPHONE, NOTES, CREATOR_ID, DATE_CREATED)
		VALUES (@ORGANISATION_NAME, @ORGANISATION_ADDRESS, @TELEPHONE,
			@NOTES, @creator_id, @DATE_CREATED)	
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback1'
			return 
		end
		else
		begin
			set @O_ID = @@IDENTITY
			insert into TB_ORGANISATION_ADMIN([ORGANISATION_ID], [CONTACT_ID]) 
			VALUES (@O_ID, @admin_id)
			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
			begin
				ROLLBACK TRANSACTION
				set @RESULT = 'rollback2'
				return 
			end
			else
			begin
				set @RESULT = @O_ID
			end
		end		
	END
	else
	BEGIN
		-- get the existing admin
		declare @O_ADM_ID int
		select @O_ADM_ID = CONTACT_ID from TB_ORGANISATION_ADMIN where ORGANISATION_ID = @ORG_ID
				
		-- we are editing the site license details
		update TB_ORGANISATION
		set ORGANISATION_NAME = @ORGANISATION_NAME, 
		ORGANISATION_ADDRESS = @ORGANISATION_ADDRESS,
		TELEPHONE = @TELEPHONE,
		NOTES = @NOTES,
		DATE_CREATED = @DATE_CREATED
		where ORGANISATION_ID = @ORG_ID
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback3'
			return 
		end
		else
		begin			
			update TB_ORGANISATION_ADMIN
			set CONTACT_ID = @admin_id
			where CONTACT_ID = @O_ADM_ID
			and ORGANISATION_ID = @ORG_ID
			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
			begin
				ROLLBACK TRANSACTION
				set @RESULT = 'rollback4'
				return 
			end
			else
			begin			
				set @RESULT = 'valid'
			end
		end		

	END
	
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	set @RESULT = 'rollback' --to tell the code data was not committed!
	END CATCH
	
	return 

END






GO
/****** Object:  StoredProcedure [dbo].[st_Organisation_Edit_By_OrgAdm]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_Organisation_Edit_By_OrgAdm]
(
	@ORGANISATION_ID int,
	@ORGANISATION_ADDRESS nvarchar(500),
	@TELEPHONE nvarchar(50),
	@NOTES nvarchar(2000)
)

As

SET NOCOUNT ON

	update TB_ORGANISATION
	set ORGANISATION_ADDRESS = @ORGANISATION_ADDRESS,
	TELEPHONE = @TELEPHONE,
	NOTES = @NOTES
	where ORGANISATION_ID = @ORGANISATION_ID

SET NOCOUNT OFF



GO
/****** Object:  StoredProcedure [dbo].[st_Organisation_Get]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Organisation_Get] 
	-- Add the parameters for the stored procedure here
	
	@admin_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @Organisation TABLE
(
    ORGANISATION_ID int,
    ORGANISATION_NAME nvarchar(50),
	ORGANISATION_ADDRESS nvarchar(500),
	TELEPHONE nvarchar(50),
	NOTES nvarchar(4000),
	T_CREATOR_ID int,
	CREATOR_NAME nvarchar(255),
	ADMINISTRATOR_ID int,
	ADMINISTRATOR_NAME nvarchar(255),
	DATE_CREATED date	
)

	INSERT INTO @Organisation (ORGANISATION_ID, ORGANISATION_NAME,
	ORGANISATION_ADDRESS, TELEPHONE, NOTES, T_CREATOR_ID, DATE_CREATED, ADMINISTRATOR_ID)
	select o.ORGANISATION_ID, o.ORGANISATION_NAME, o.ORGANISATION_ADDRESS,
	o.TELEPHONE, o.NOTES, o.CREATOR_ID, o.DATE_CREATED, o_a.CONTACT_ID
	from TB_ORGANISATION o
	inner join TB_ORGANISATION_ADMIN o_a on o_a.ORGANISATION_ID = o.ORGANISATION_ID
	where o_a.CONTACT_ID = @admin_ID

	update @Organisation
	set CREATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = T_CREATOR_ID)
	
	update @Organisation
	set ADMINISTRATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = ADMINISTRATOR_ID)

	select * from @Organisation
	

END




GO
/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_Accounts]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:           <Author,,Name>
-- Create date: <Create Date,,>
-- Description:      <Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Organisation_Get_Accounts] 
(
       @organisation_id int
)
AS
BEGIN
       -- SET NOCOUNT ON added to prevent extra result sets from
       -- interfering with SELECT statements.
       SET NOCOUNT ON;

    -- Insert statements for procedure here


       --reviews that were added to the currently active SITE_LIC_DETAILS
       select * from Reviewer.dbo.TB_REVIEW r
              inner join TB_ORGANISATION_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID 
			  and lr.ORGANISATION_ID = @organisation_id
       --accounts in the license
       select * from Reviewer.dbo.TB_CONTACT c
              inner join TB_ORGANISATION_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID 
			  and lc.ORGANISATION_ID = @organisation_id
       --license admins
       select * from Reviewer.dbo.TB_CONTACT c
              inner join TB_ORGANISATION_ADMIN la on c.CONTACT_ID = la.CONTACT_ID 
			  and la.ORGANISATION_ID = @organisation_id
END


GO
/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_All]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Organisation_Get_All] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 

	DECLARE @Organisations TABLE
	(
		ORGANISATION_ID int,
		ORGANISATION_NAME nvarchar(50),
		CONTACT_ID int,
		CONTACT_NAME nvarchar(500)
	)
	DECLARE @org_lic_id int
	DECLARE @first_contact_id int
	DECLARE @counter int
	set @counter = 0

	INSERT INTO @Organisations (ORGANISATION_ID, ORGANISATION_NAME)	
	select ORGANISATION_ID, ORGANISATION_NAME
	from TB_ORGANISATION

	DECLARE org_id CURSOR FOR 
	SELECT ORGANISATION_ID from @Organisations 
	OPEN org_id
	FETCH NEXT FROM org_id INTO @org_lic_id

	WHILE @@FETCH_STATUS = 0
	BEGIN	
		DECLARE contact_id CURSOR FOR 
		SELECT CONTACT_ID from TB_ORGANISATION_ADMIN o_a
		where o_a.ORGANISATION_ID = @org_lic_id
		OPEN contact_id
		FETCH NEXT FROM contact_id INTO @first_contact_id
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if @counter = 0
			begin
				update @Organisations
				set CONTACT_ID = @first_contact_id
				where ORGANISATION_ID = @org_lic_id
				
				update @Organisations
				set CONTACT_NAME = 
				(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT c
				where c.CONTACT_ID = @first_contact_id)
				where ORGANISATION_ID = @org_lic_id

				set @counter = 1
			end
			FETCH NEXT FROM contact_id INTO @first_contact_id
		END
		CLOSE contact_id
		DEALLOCATE contact_id
		set @counter = 0
		FETCH NEXT FROM org_id INTO @org_lic_id
	END

	CLOSE org_id
	DEALLOCATE org_id


select * from @Organisations


       
END





GO
/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_By_Admin]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Organisation_Get_By_Admin] 
(	
	@organisation_adm_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select distinct o_a.ORGANISATION_ID, o.ORGANISATION_NAME from TB_ORGANISATION_ADMIN o_a
	inner join TB_ORGANISATION o on o.ORGANISATION_ID = o_a.ORGANISATION_ID
	where o_a.CONTACT_ID = @organisation_adm_ID



	
END





GO
/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_By_ContactID]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Organisation_Get_By_ContactID] 
	-- Add the parameters for the stored procedure here
	
	@CONTACT_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select ORGANISATION_ID, ORGANISATION_NAME from TB_ORGANISATION
	where ORGANISATION_ID in (select ORGANISATION_ID from TB_ORGANISATION_CONTACT
	where CONTACT_ID = @CONTACT_ID)


	

END





GO
/****** Object:  StoredProcedure [dbo].[st_Organisation_Get_ByID]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Organisation_Get_ByID] 
	-- Add the parameters for the stored procedure here
	
	@admin_ID int,
	@organisation_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @Organisation TABLE
(
    ORGANISATION_ID int,
    ORGANISATION_NAME nvarchar(50),
	ORGANISATION_ADDRESS nvarchar(500),
	TELEPHONE nvarchar(50),
	NOTES nvarchar(4000),
	T_CREATOR_ID int,
	CREATOR_NAME nvarchar(255),
	ADMINISTRATOR_ID int,
	ADMINISTRATOR_NAME nvarchar(255),
	DATE_CREATED date	
)

	INSERT INTO @Organisation (ORGANISATION_ID, ORGANISATION_NAME,
	ORGANISATION_ADDRESS, TELEPHONE, NOTES, T_CREATOR_ID, DATE_CREATED, ADMINISTRATOR_ID)
	select o.ORGANISATION_ID, o.ORGANISATION_NAME, o.ORGANISATION_ADDRESS,
	o.TELEPHONE, o.NOTES, o.CREATOR_ID, o.DATE_CREATED, o_a.CONTACT_ID
	from TB_ORGANISATION o
	inner join TB_ORGANISATION_ADMIN o_a on o_a.ORGANISATION_ID = o.ORGANISATION_ID
	where o_a.CONTACT_ID = @admin_ID
	and o.ORGANISATION_ID = @organisation_id

	update @Organisation
	set CREATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = T_CREATOR_ID)
	
	update @Organisation
	set ADMINISTRATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = ADMINISTRATOR_ID)

	select * from @Organisation
	

END





GO
/****** Object:  StoredProcedure [dbo].[st_Organisation_Remove_Review]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Organisation_Remove_Review]
	-- Add the parameters for the stored procedure here
	@org_id int
	, @admin_ID int
	, @review_id int
	, @res int output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	set @res = 0

    -- Insert statements for procedure here
    declare @ck int, @ck2 int, @org_det_id int
	
	set @ck = (SELECT count(contact_id) from TB_ORGANISATION_ADMIN 
	where (CONTACT_ID = @admin_ID or @admin_ID in 
	(select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)
		   ) and ORGANISATION_ID = @org_id)
	if @ck < 1 set @res = -1 --first check: see if supplied admin is actually an admin!
	
	--second check, see if review_id exists
	else if  
	(select count(REVIEW_ID) from Reviewer.dbo.TB_REVIEW 
	 where @review_id = REVIEW_ID) != 1
		set @res = -2
	
	else -- initial checks went OK, let's see if we can do it
	begin
		set @ck = 
		(select count(review_id) from TB_ORGANISATION_REVIEW 
		where REVIEW_ID = @review_id and ORGANISATION_ID = @org_id)
		
		--set @lic_det_id = 
		--(SELECT TOP 1 ld.SITE_LIC_DETAILS_ID from Reviewer.dbo.TB_SITE_LIC sl 
		--	inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
		--	inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
		--	Order by ld.VALID_FROM desc)
		
		if @ck = 0 -- review is NOT in the site lic
		begin
			set @res = -3 --review not in this site_lic
		end
		else
		begin --all is well, let's do something!
			begin transaction --make sure we don't commit only half of the mission critical data! (we assume the update below will work, only checking for the other statement)
			
			delete from TB_ORGANISATION_REVIEW
			where REVIEW_ID = @review_id

			update Reviewer.dbo.TB_REVIEW set ORGANISATION_ID = 0
					where REVIEW_ID = @review_id
			
			if @@ROWCOUNT = 1
			begin --write success log
				commit transaction --all is well, commit
				insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE]) 
				values (@org_id, @admin_ID, @review_id, 'remove review')
			end
			else --write failure log, if this is fired, there is a bug somewhere
			begin
				rollback transaction --BAD! something went wrong!
				insert into TB_ORGANISATION_LOG ([ORGANISATION_ID], [CONTACT_ID], [AFFECTED_ID], [CHANGE_TYPE]) 
				values (@org_id, @admin_ID, @review_id, 'remove review: failed!')	
				set @res = -4
			end
		end
	end
	--select r.review_id, review_name from reviewer.dbo.TB_SITE_LIC_REVIEW lr
	--	inner join Reviewer.dbo.TB_REVIEW r on lr.REVIEW_ID = r.REVIEW_ID
	-- where SITE_LIC_ID = @lic_id
	return @res
	-- error codes: -1 = supplied admin_id is not an admin of this site lice
	--				-2 = review_id does not exist
	--				-3 = review not in this site_lic
	--				-4 = all seemed well but couldn't write changes! BUG ALERT
END




GO
/****** Object:  StoredProcedure [dbo].[st_OrganisationRemoveMember]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER procedure [dbo].[st_OrganisationRemoveMember]
(
	@ORGANISATION_ID int,
	@CONTACT_ID int
)

As

SET NOCOUNT ON


	                
	delete from TB_ORGANISATION_CONTACT 
	where CONTACT_ID = @CONTACT_ID
	and ORGANISATION_ID = @ORGANISATION_ID

		

SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[st_OutstandingFeeByAccountID]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_OutstandingFeeByAccountID] 
(
	@CONTACT_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select * from TB_OUTSTANDING_FEE
	where ACCOUNT_ID = @CONTACT_ID
	and STATUS like 'Outstanding'
       
END



GO
/****** Object:  StoredProcedure [dbo].[st_OutstandingFeeCreateOrEdit]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER   procedure [dbo].[st_OutstandingFeeCreateOrEdit]
(
	@OUTSTANDING_FEE_ID int,
	@ACCOUNT_ID int,
	@AMOUNT int,
	@DATE_CREATED datetime,
	@NOTES nvarchar(max)
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	if @NOTES = 'DeleteThisFee'
	begin
		delete from TB_OUTSTANDING_FEE where OUTSTANDING_FEE_ID = @OUTSTANDING_FEE_ID
	end
	else
	begin
		if @OUTSTANDING_FEE_ID = 0
		begin
		insert into TB_OUTSTANDING_FEE (ACCOUNT_ID, AMOUNT, DATE_CREATED, NOTES, [STATUS])
			values (@ACCOUNT_ID, @AMOUNT, @DATE_CREATED, @NOTES, 'Outstanding')
		end
		else
		begin
			update TB_OUTSTANDING_FEE
			set ACCOUNT_ID = @ACCOUNT_ID, AMOUNT = @AMOUNT, DATE_CREATED = @DATE_CREATED, NOTES = @NOTES
			where OUTSTANDING_FEE_ID = @OUTSTANDING_FEE_ID

		end
	end

END


GO
/****** Object:  StoredProcedure [dbo].[st_OutstandingFeeGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE OR ALTER   PROCEDURE [dbo].[st_OutstandingFeeGet] 
(
	@OUTSTANDING_FEE_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select  o_f.DATE_CREATED, o_f.ACCOUNT_ID, c.CONTACT_NAME, 
	c.EMAIL, o_f.AMOUNT, o_f.[STATUS], o_f.NOTES from TB_OUTSTANDING_FEE o_f
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = o_f.ACCOUNT_ID
	where o_f.OUTSTANDING_FEE_ID = @OUTSTANDING_FEE_ID
    	
	RETURN
END


GO
/****** Object:  StoredProcedure [dbo].[st_OutstandingFeesGetAll]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_OutstandingFeesGetAll] 
(
	@TEXT_BOX nvarchar(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select  o_f.OUTSTANDING_FEE_ID, o_f.DATE_CREATED, o_f.ACCOUNT_ID, c.CONTACT_NAME, 
	c.EMAIL, o_f.AMOUNT, o_f.[STATUS] 
	from TB_OUTSTANDING_FEE o_f
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = o_f.ACCOUNT_ID
	where ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
			(c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
			(c.EMAIL like '%' + @TEXT_BOX + '%'))

	group by o_f.OUTSTANDING_FEE_ID, o_f.DATE_CREATED, o_f.ACCOUNT_ID, c.CONTACT_NAME, 
	c.EMAIL, o_f.AMOUNT, o_f.[STATUS] 
       
END



GO
/****** Object:  StoredProcedure [dbo].[st_PersistedGrantAdd]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[st_PersistedGrantAdd]
	-- Add the parameters for the stored procedure here
	@KEY nvarchar(200),
	@TYPE nvarchar(50),
	@CLIENT_ID [nvarchar](200),
	@DATE_CREATED datetime2(1),
	@DATA nvarchar(MAX),
	@EXPIRATION datetime2(1),
	@CONTACT_ID [int]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	    declare @check int
    select @check = PERSISTED_GRANT_ID from TB_PERSISTED_GRANT where [KEY] = @KEY
    if @check is null
    BEGIN
		INSERT INTO [TB_PERSISTED_GRANT]
			   ([KEY]
			   ,[TYPE]
			   ,[CLIENT_ID]
			   ,[DATE_CREATED]
			   ,[DATA]
			   ,[EXPIRATION]
			   ,[CONTACT_ID])
		 VALUES
			   (@KEY ,
				@TYPE,
				@CLIENT_ID,
				@DATE_CREATED,
				@DATA,
				@EXPIRATION,
				@CONTACT_ID)
	END
	ELSE
	BEGIN
		UPDATE TB_PERSISTED_GRANT
			SET [KEY] = @KEY
			   ,[TYPE] = @TYPE
			   ,[CLIENT_ID] = @CLIENT_ID
			   ,[DATE_CREATED] = @DATE_CREATED
			   ,[DATA] = @DATA
			   ,[EXPIRATION] = @EXPIRATION
			   ,[CONTACT_ID] = @CONTACT_ID
			WHERE [KEY] = @KEY
	END
END

GO
/****** Object:  StoredProcedure [dbo].[st_PersistedGrantGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[st_PersistedGrantGet]
	-- Add the parameters for the stored procedure here
	@KEY nvarchar(200)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select * from  TB_PERSISTED_GRANT
	Where [KEY] = @KEY
END

GO
/****** Object:  StoredProcedure [dbo].[st_PersistedGrantGetAll]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[st_PersistedGrantGetAll]
	-- Add the parameters for the stored procedure here
	@CONTACT_ID [int]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select * from  TB_PERSISTED_GRANT
	Where CONTACT_ID = @CONTACT_ID
END

GO
/****** Object:  StoredProcedure [dbo].[st_PersistedGrantRemove]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[st_PersistedGrantRemove]
	-- Add the parameters for the stored procedure here
	@KEY nvarchar(200)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DELETE from TB_PERSISTED_GRANT where [KEY] = @KEY
END

GO
/****** Object:  StoredProcedure [dbo].[st_PersistedGrantRemoveAll]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[st_PersistedGrantRemoveAll]
	-- Add the parameters for the stored procedure here
	@CONTACT_ID [int],
	@CLIENT_ID [nvarchar](200),
	@TYPE nvarchar(50)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	IF @TYPE = 'ALLTYPES'
	BEGIN
		DELETE from TB_PERSISTED_GRANT where [CONTACT_ID] = @CONTACT_ID AND [CLIENT_ID] = @CLIENT_ID
	END
	ELSE
	BEGIN
		DELETE from TB_PERSISTED_GRANT where [CONTACT_ID] = @CONTACT_ID AND [CLIENT_ID] = @CLIENT_ID AND [TYPE] = @TYPE 
	END
	
END

GO
/****** Object:  StoredProcedure [dbo].[st_PriorityScreeningTurnOnOff]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_PriorityScreeningTurnOnOff]
(
	@REVIEW_ID int,
	@FIELD nvarchar(50),
	@SETTING bit
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	if @FIELD = 'ShowScreening'
	begin
		update Reviewer.dbo.TB_REVIEW
		set SHOW_SCREENING = @SETTING
		where REVIEW_ID = @REVIEW_ID
	end
	
	if @FIELD = 'AllowReviewerTerms'
	begin
		update Reviewer.dbo.TB_REVIEW
		set ALLOW_REVIEWER_TERMS = @SETTING
		where REVIEW_ID = @REVIEW_ID
	end
	
	if @FIELD = 'AllowClusteredSearch'
	begin
		update Reviewer.dbo.TB_REVIEW
		set ALLOW_CLUSTERED_SEARCH = @SETTING
		where REVIEW_ID = @REVIEW_ID
	end

END



GO
/****** Object:  StoredProcedure [dbo].[st_RecentActivityGetAllFilter]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_RecentActivityGetAllFilter] 
(
	@TEXT_BOX nvarchar(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 
	SELECT t.[CONTACT_ID] as C_ID        
	,t.[REVIEW_ID] as R_ID        
	,t.[CREATED]        
	,t.[LAST_RENEWED]        
	,CONTACT_NAME        
	,EMAIL        
	,REVIEW_NAME        
	,case             
		when r.EXPIRY_DATE is null AND r.ARCHIE_ID is null 
			then 'Private'            
		when r.EXPIRY_DATE is null AND r.ARCHIE_ID is not null AND r.ARCHIE_ID != 'prospective_______' 
			then 'Archie'                  
		when r.EXPIRY_DATE is null AND r.ARCHIE_ID = 'prospective_______' 
			then 'P-Archie'                  
		else 'Shared'                  
		end 
	as 'rev type'          
	,SUM(DATEDIFF(HOUR, t1.CREATED, t1.LAST_RENEWED)) as [active hours]      
	FROM[TB_LOGON_TICKET] t     
	Inner JOIN Reviewer.dbo.TB_CONTACT c on t.CONTACT_ID = c.CONTACT_ID      
	Inner Join Reviewer.dbo.TB_REVIEW r on r.REVIEW_ID = t.REVIEW_ID      
	inner join TB_LOGON_TICKET t1 on t.CONTACT_ID = t1.CONTACT_ID      
	where t.STATE = 1 and t.LAST_RENEWED > DATEADD(hh, -3, GETDATE())  
	
	and ((c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR 
                (c.EMAIL like '%' + @TEXT_BOX + '%') OR
                (t.REVIEW_ID like '%' + @TEXT_BOX + '%') OR
                (c.CONTACT_ID like '%' + @TEXT_BOX + '%'))
	    
	group by t.[CONTACT_ID]          
		,t.[REVIEW_ID]          
		,t.[CREATED]         
		,t.[LAST_RENEWED]          
		,t.[STATE]          
		,CONTACT_NAME          
		,EMAIL          
		,REVIEW_NAME         
		, r.EXPIRY_DATE          
		, r.ARCHIE_ID 
		     
	order by LAST_RENEWED desc
       
END





GO
/****** Object:  StoredProcedure [dbo].[st_RemoveReviewsFromPreviousPackages]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_RemoveReviewsFromPreviousPackages]
(
	@SITE_LIC_ID int
)

As

SET NOCOUNT ON

	declare @det_id int
    set @det_id = (select top 1 ld.SITE_LIC_DETAILS_ID from TB_SITE_LIC_DETAILS ld 
        inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and ld.VALID_FROM is not null
        and fs.IS_ACTIVE = 0 and ld.SITE_LIC_ID = @SITE_LIC_ID
        order by ld.VALID_FROM desc)

		declare @reviewList table
		(
			tv_review_id int
		)


       --reviews that were added in the past (not associated with currently active SITE_LIC_DETAILS)
	   insert into @reviewList
       select r.REVIEW_ID from Reviewer.dbo.TB_REVIEW r
              inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID and SITE_LIC_ID = @SITE_LIC_ID and SITE_LIC_DETAILS_ID != @det_id

		delete from Reviewer.dbo.TB_SITE_LIC_REVIEW where REVIEW_ID = (select tv_review_id from @reviewList)


SET NOCOUNT OFF








GO
/****** Object:  StoredProcedure [dbo].[st_ResetPassword]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ResetPassword] 
	-- Add the parameters for the stored procedure here
	@CID int
	, @UID uniqueidentifier
	, @UNAME varchar(50)
	, @PW varchar(50)
	, @RES int = 0 OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT @RES = COUNT(c.CONTACT_ID) from Reviewer.dbo.TB_CONTACT c
		inner join TB_CHECKLINK l on c.CONTACT_ID = l.CONTACT_ID and c.CONTACT_ID = @CID
		where l.CHECKLINK_UID = @UID and c.USERNAME = @UNAME and l.IS_STALE = 1 
			and l.WAS_SUCCESS is null --should we not check for this and allow the user to correct the username in case it was mispelled?
			and (@pw is not null and LEN(@pw) > 7)
	IF @RES = 1 --all is well: we found one result as expected
	BEGIN
		--PW hash!
		DECLARE @chars char(100) = '!т#$%&а()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~ийклм'
		declare @rnd varchar(20)
		declare @cnt int = 0
		set @rnd = ''
		WHILE (@cnt <= 20) 
		BEGIN
			SELECT @rnd = @rnd + 
				SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
			SELECT @cnt = @cnt + 1
		END
		update c
		set c.FLAVOUR = @rnd, c.PWASHED = HASHBYTES('SHA1', @pw + @rnd)
		from Reviewer.dbo.TB_CONTACT c
			inner join TB_CHECKLINK l on c.CONTACT_ID = l.CONTACT_ID and c.CONTACT_ID = @CID
		where l.CHECKLINK_UID = @UID and c.USERNAME = @UNAME and l.IS_STALE = 1 and l.WAS_SUCCESS is null
		Select @RES = @@ROWCOUNT
		if @RES = 1
		begin --still doing good: we changed one row as expected
			--mark the link as used successfully 
			UPDATE TB_CHECKLINK set IS_STALE = 1, WAS_SUCCESS = 1, DATE_USED = GETDATE() where CONTACT_ID = @CID and CHECKLINK_UID = @UID
		end
		ELSE BEGIN
			UPDATE TB_CHECKLINK set IS_STALE = 1, WAS_SUCCESS = 0, DATE_USED = GETDATE() where CONTACT_ID = @CID and CHECKLINK_UID = @UID
			select @RES = -1 --means the failure happened when trying to change the password
		END
	END
	ELSE BEGIN
		UPDATE TB_CHECKLINK set IS_STALE = 1, WAS_SUCCESS = 0, DATE_USED = GETDATE() where CONTACT_ID = @CID and CHECKLINK_UID = @UID
		select @RES = 0 --means the failure happened when trying to validate the link/account info
	END
END

GO
/****** Object:  StoredProcedure [dbo].[st_ReviewAddContact]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER procedure [dbo].[st_ReviewAddContact]
(
	@REVIEW_ID int,
	@CONTACT_ID int,
	@old_review_id nvarchar(50)
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	declare @REVIEW_CONTACT_ID int
	declare @old_contact_id nvarchar(50)
	
	select CONTACT_ID from Reviewer.dbo.TB_REVIEW_CONTACT
	where CONTACT_ID = @CONTACT_ID and REVIEW_ID = @REVIEW_ID
	
	if @@ROWCOUNT = 0  --i.e. it's not already there
	begin
		select @old_contact_id = old_contact_id from Reviewer.dbo.TB_CONTACT
		where CONTACT_ID = @CONTACT_ID

		insert into Reviewer.dbo.TB_REVIEW_CONTACT (REVIEW_ID, CONTACT_ID, old_review_id, old_contact_id)
		values (@REVIEW_ID, @CONTACT_ID, @old_review_id, @old_contact_id)
		set @REVIEW_CONTACT_ID = @@IDENTITY

		insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
		values (@REVIEW_CONTACT_ID, 'RegularUser')
	end


END



GO
/****** Object:  StoredProcedure [dbo].[st_ReviewAddMember]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_ReviewAddMember]
(
	@REVIEW_ID int,
	@CONTACT_ID int
)

As

SET NOCOUNT ON

	declare @NEW_CONTACT_REVIEW_ID int
	declare @funderID int
	
	insert into Reviewer.dbo.TB_REVIEW_CONTACT(CONTACT_ID, REVIEW_ID)
	values (@CONTACT_ID, @REVIEW_ID)
	
	set @NEW_CONTACT_REVIEW_ID = @@IDENTITY
	
	insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE(REVIEW_CONTACT_ID, ROLE_NAME)
	values(@NEW_CONTACT_REVIEW_ID, 'RegularUser')
	
	-- if the contact_id of the invitee is the funderID then give them AdminUser role
	select @funderID = FUNDER_ID from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @REVIEW_ID
	if @funderID = @CONTACT_ID
	begin
		update Reviewer.dbo.TB_CONTACT_REVIEW_ROLE set ROLE_NAME = 'AdminUser' 
		where REVIEW_CONTACT_ID = @NEW_CONTACT_REVIEW_ID
	end
	
	

SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewContacts]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- Create date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ReviewContacts] 
(
	@REVIEW_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here 
    CREATE TABLE #REVIEW_CONTACTS (
	CONTACT_ID int,
	CONTACT_NAME nvarchar(255),
	EMAIL nvarchar(500),
	LAST_LOGIN datetime,
	EXPIRY_DATE date,
	HOURS int)
    
    insert into #REVIEW_CONTACTS (CONTACT_ID, CONTACT_NAME, EMAIL, EXPIRY_DATE)
	select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, c.EXPIRY_DATE 
	FROM Reviewer.dbo.tb_CONTACT c
	INNER JOIN Reviewer.dbo.tb_REVIEW_CONTACT r_c ON c.CONTACT_ID = r_c.CONTACT_ID
	AND r_c.REVIEW_ID = @REVIEW_ID
	group by c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, c.EXPIRY_DATE
	ORDER BY c.CONTACT_NAME
	
	update #REVIEW_CONTACTS
	set HOURS = 
	(select SUM(DATEDIFF(HOUR,l_t.CREATED ,l_t.LAST_RENEWED )) as HOURS
	from TB_LOGON_TICKET l_t where l_t.REVIEW_ID = @REVIEW_ID 
	and CONTACT_ID = #REVIEW_CONTACTS.CONTACT_ID)

	update #REVIEW_CONTACTS
	set LAST_LOGIN = 
	(select max(l_t.CREATED) as LAST_LOGIN
	from TB_LOGON_TICKET l_t where l_t.REVIEW_ID = @REVIEW_ID 
	and CONTACT_ID = #REVIEW_CONTACTS.CONTACT_ID)
	
    select * from #REVIEW_CONTACTS

	drop table #REVIEW_CONTACTS
	
    /*
    select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL,
    c.LAST_LOGIN, c.EXPIRY_DATE, SUM(DATEDIFF(HOUR,l_t.CREATED ,l_t.LAST_RENEWED )) as HOURS
    FROM Reviewer.dbo.tb_CONTACT c
    INNER JOIN Reviewer.dbo.tb_REVIEW_CONTACT r_c ON c.CONTACT_ID = r_c.CONTACT_ID 
    inner join TB_LOGON_TICKET l_t on l_t.CONTACT_ID = r_c.CONTACT_ID and l_t.REVIEW_ID = @REVIEW_ID
    AND r_c.REVIEW_ID = @REVIEW_ID
    group by c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, c.LAST_LOGIN, c.EXPIRY_DATE
    ORDER BY c.CONTACT_NAME*/

END
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewDetails]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ReviewDetails] 
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
	l.SITE_LIC_ID, l.SITE_LIC_NAME, r.SHOW_SCREENING, r.ALLOW_REVIEWER_TERMS, r.ALLOW_CLUSTERED_SEARCH
	from Reviewer.dbo.TB_REVIEW r
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
	left join Reviewer.dbo.TB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
	left join Reviewer.dbo.TB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
	where r.REVIEW_ID = @REVIEW_ID
	
	group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
	r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
	l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME, r.REVIEW_NUMBER, r.OLD_REVIEW_GROUP_ID, 
	r.SHOW_SCREENING, r.ALLOW_REVIEWER_TERMS, r.ALLOW_CLUSTERED_SEARCH
	order by r.REVIEW_NAME
	

	RETURN

END

GO
/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsCochrane]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ReviewDetailsCochrane] 
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
	r.ARCHIE_ID, r.ARCHIE_CD, r.IS_CHECKEDOUT_HERE, r.CHECKED_OUT_BY, r.MAG_ENABLED
	from Reviewer.dbo.TB_REVIEW r
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
	left join Reviewer.dbo.TB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
	left join Reviewer.dbo.TB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
	where r.REVIEW_ID = @REVIEW_ID
	
	group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
	r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
	l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME, r.REVIEW_NUMBER, r.OLD_REVIEW_GROUP_ID,
	r.SHOW_SCREENING, r.ALLOW_REVIEWER_TERMS, r.ALLOW_CLUSTERED_SEARCH,
	r.ARCHIE_ID, r.ARCHIE_CD, r.IS_CHECKEDOUT_HERE, r.CHECKED_OUT_BY, r.MAG_ENABLED
	order by r.REVIEW_NAME
	

	RETURN

END

GO
/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsFilter]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ReviewDetailsFilter] 
(
	@SHAREABLE bit,
	@TEXT_BOX nvarchar(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    if @SHAREABLE = 1
	begin        
		/*
		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
		r.FUNDER_ID, c.CONTACT_NAME, r.MONTHS_CREDIT
		FROM Reviewer.dbo.tb_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
		where 
		(r.EXPIRY_DATE is not null) OR
		(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0)*/
		
		select r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME,  
		r.DATE_CREATED, 
			CASE when l.[EXPIRY_DATE] is not null 
			and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
				then l.[EXPIRY_DATE]
			else r.[EXPIRY_DATE]
			end as 'EXPIRY_DATE', 
		r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		from Reviewer.dbo.TB_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
		left join Reviewer.dbo.TB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
		where 
			((r.EXPIRY_DATE is not null) OR
			(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0))
		and ((r.REVIEW_ID like '%' + @TEXT_BOX + '%') OR
			(r.REVIEW_NAME like '%' + @TEXT_BOX + '%') OR
			(c.CONTACT_NAME like '%' + @TEXT_BOX + '%'))
		and r.ARCHIE_ID is null
		
		
		group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
		r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
		l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		order by r.REVIEW_NAME
		 
	end
	else
	begin
		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
		r.FUNDER_ID, c.CONTACT_NAME
		FROM Reviewer.dbo.tb_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID 
		where (r.EXPIRY_DATE is null and r.MONTHS_CREDIT = 0)
		and r.ARCHIE_ID is null
		
		and ((r.REVIEW_ID like '%' + @TEXT_BOX + '%') OR
			(r.REVIEW_NAME like '%' + @TEXT_BOX + '%') OR
			(c.CONTACT_NAME like '%' + @TEXT_BOX + '%'))
		
	end

	RETURN

END



GO
/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsFilterCochrane]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ReviewDetailsFilterCochrane] 
(
	@SHAREABLE bit,
	@TEXT_BOX nvarchar(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    if @SHAREABLE = 1
	begin        
		/*
		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
		r.FUNDER_ID, c.CONTACT_NAME, r.MONTHS_CREDIT
		FROM Reviewer.dbo.tb_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
		where 
		(r.EXPIRY_DATE is not null) OR
		(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0)*/
		
		select r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME,  
		r.DATE_CREATED, 
			CASE when l.[EXPIRY_DATE] is not null 
			and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
				then l.[EXPIRY_DATE]
			else r.[EXPIRY_DATE]
			end as 'EXPIRY_DATE', 
		r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		from Reviewer.dbo.TB_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
		left join Reviewer.dbo.TB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
		where 
			((r.EXPIRY_DATE is not null) OR
			(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0))
		and ((r.REVIEW_ID like '%' + @TEXT_BOX + '%') OR
			(r.REVIEW_NAME like '%' + @TEXT_BOX + '%') OR
			(c.CONTACT_NAME like '%' + @TEXT_BOX + '%'))
		and r.ARCHIE_ID is not null
		
		
		group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
		r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
		l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		order by r.REVIEW_NAME
		 
	end
	else
	begin
		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
		r.FUNDER_ID, c.CONTACT_NAME
		FROM Reviewer.dbo.tb_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID 
		where (r.EXPIRY_DATE is null and r.MONTHS_CREDIT = 0)
		
		and ((r.REVIEW_ID like '%' + @TEXT_BOX + '%') OR
			(r.REVIEW_NAME like '%' + @TEXT_BOX + '%') OR
			(c.CONTACT_NAME like '%' + @TEXT_BOX + '%'))
		and r.ARCHIE_ID is not null
		
	end

	RETURN

END




GO
/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsGetAll]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ReviewDetailsGetAll] 
(
	@SHAREABLE bit
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    if @SHAREABLE = 1
	begin        
		/*
		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
		r.FUNDER_ID, c.CONTACT_NAME, r.MONTHS_CREDIT
		FROM Reviewer.dbo.tb_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
		where 
		(r.EXPIRY_DATE is not null) OR
		(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0)*/
		

		
		select r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME,  
		r.DATE_CREATED, 
			CASE when l.[EXPIRY_DATE] is not null 
			and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
				then l.[EXPIRY_DATE]
			else r.[EXPIRY_DATE]
			end as 'EXPIRY_DATE', 
		r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		from Reviewer.dbo.TB_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
		left join Reviewer.dbo.TB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
		where 
			(r.EXPIRY_DATE is not null) OR
			(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0)
		and r.ARCHIE_ID is null
		
		group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
		r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
		l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		order by r.REVIEW_NAME
		
	end
	else
	begin
		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
		r.FUNDER_ID, c.CONTACT_NAME
		FROM Reviewer.dbo.tb_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID 
		where (r.EXPIRY_DATE is null and r.MONTHS_CREDIT = 0)
		and r.ARCHIE_ID is null
	end

	RETURN

END


GO
/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsGetAllCochrane]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ReviewDetailsGetAllCochrane] 
(
	@SHAREABLE bit
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    if @SHAREABLE = 1
	begin        
		/*
		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
		r.FUNDER_ID, c.CONTACT_NAME, r.MONTHS_CREDIT
		FROM Reviewer.dbo.tb_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
		where 
		(r.EXPIRY_DATE is not null) OR
		(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0)*/
		
		select r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME,  
		r.DATE_CREATED, 
			CASE when l.[EXPIRY_DATE] is not null 
			and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
				then l.[EXPIRY_DATE]
			else r.[EXPIRY_DATE]
			end as 'EXPIRY_DATE', 
		r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		from Reviewer.dbo.TB_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
		left join Reviewer.dbo.TB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
		where 
			(r.EXPIRY_DATE is not null) OR
			(r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0)
		and r.ARCHIE_ID is not null
		
		group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
		r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
		l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
		order by r.REVIEW_NAME
		 
	end
	else
	begin
		SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
		r.FUNDER_ID, c.CONTACT_NAME
		FROM Reviewer.dbo.tb_REVIEW r
		inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID 
		where (r.EXPIRY_DATE is null and r.MONTHS_CREDIT = 0)
		and r.ARCHIE_ID is not null
	end

	RETURN

END



GO
/****** Object:  StoredProcedure [dbo].[st_ReviewEditName]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_ReviewEditName]
(
	@REVIEW_ID int,
	@REVIEW_NAME nvarchar(1000)
)

As

SET NOCOUNT ON

/*
declare @DATE date
declare @MONTHS int

	select @DATE = EXPIRY_DATE, @MONTHS = MONTHS_CREDIT 
	from Reviewer.dbo.TB_REVIEW
	where REVIEW_ID = @REVIEW_ID 
	
	if @DATE is null
	begin
		update Reviewer.dbo.TB_REVIEW
		set EXPIRY_DATE = dateadd(month,@MONTHS, sysdatetime()),
		MONTHS_CREDIT = 0
		where REVIEW_ID = @REVIEW_ID
	end
*/	
	
	update Reviewer.dbo.TB_REVIEW 
	set REVIEW_NAME = @REVIEW_NAME
    where REVIEW_ID = @REVIEW_ID
	

SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewExtend]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_ReviewExtend]
(
	@REVIEW_FUNDER_ID int,
	@REVIEW_ID int,
	@EXTEND_BY int
)

As

SET NOCOUNT ON
declare @COST int
declare @REVIEW_COST int

	select top 1 @COST = PRICE_PER_MONTH from TB_FOR_SALE
	where [TYPE_NAME] = 'Shareable Review'
	order by LAST_CHANGED desc
	
	set @REVIEW_COST = @COST * @EXTEND_BY
	
	insert into TB_REVIEW_EXTENSION (REVIEW_FUNDER_ID, REVIEW_ID, EXTEND_BY, COST)
	values (@REVIEW_FUNDER_ID, @REVIEW_ID, @EXTEND_BY, @REVIEW_COST)

SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewFullCreateOrEdit]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_ReviewFullCreateOrEdit]
(
	@NEW_REVIEW bit,
	@REVIEW_NAME nvarchar(255),
	@DATE_CREATED datetime,
	@EXPIRY_DATE nvarchar(50),
	@FUNDER_ID int,
	@REVIEW_NUMBER nvarchar(50),
	@REVIEW_ID nvarchar(50), -- it might say 'New'
	@EXTENSION_TYPE_ID int,
	@EXTENSION_NOTES nvarchar(500),
	@EDITOR_ID int,
	@MONTHS_CREDIT nvarchar(50),
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON
DECLARE @ORIGINAL_EXPIRY datetime
DECLARE @NEW_ROW_ID int

if @EXPIRY_DATE = ''
begin
	SET @EXPIRY_DATE = NULL
end

BEGIN TRY

BEGIN TRANSACTION
	if @NEW_REVIEW = 1
	begin
		-- create a new account
		INSERT INTO Reviewer.dbo.TB_REVIEW(REVIEW_NAME, DATE_CREATED, [EXPIRY_DATE], MONTHS_CREDIT, REVIEW_NUMBER, FUNDER_ID)
		VALUES (@REVIEW_NAME, @DATE_CREATED, @EXPIRY_DATE, @MONTHS_CREDIT, @REVIEW_NUMBER, @FUNDER_ID)
	
		set @RESULT = @@IDENTITY
    end
	else  -- edit an existing account
	begin	
		-- get original expiry date from TB_REVIEW
		select @ORIGINAL_EXPIRY = r.EXPIRY_DATE 
		from Reviewer.dbo.TB_REVIEW r
		where r.REVIEW_ID = @REVIEW_ID
		
		--update TB_CONTACT
		update Reviewer.dbo.TB_REVIEW 
		set 
		REVIEW_NAME = @REVIEW_NAME,
		DATE_CREATED = @DATE_CREATED,
		[EXPIRY_DATE] = @EXPIRY_DATE,
		FUNDER_ID = @FUNDER_ID,
		REVIEW_NUMBER = @REVIEW_NUMBER,
		MONTHS_CREDIT = @MONTHS_CREDIT
		where REVIEW_ID = @REVIEW_ID
		
		if (@EXTENSION_TYPE_ID > 1)
		begin
			-- create new row in TB_EXPIRY_EDIT_LOG -- using bogus old expiry date
			insert into TB_EXPIRY_EDIT_LOG
			(DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
			OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
			values (GETDATE(), 0, @REVIEW_ID, @EXPIRY_DATE, @EXPIRY_DATE,
			@EDITOR_ID, @EXTENSION_TYPE_ID, @EXTENSION_NOTES)
			set @NEW_ROW_ID = @@IDENTITY
			
			--  update TB_EXPIRY_EDIT_LOG
			update TB_EXPIRY_EDIT_LOG
			set OLD_EXPIRY_DATE = @ORIGINAL_EXPIRY
			where EXPIRY_EDIT_ID = @NEW_ROW_ID
	
			set @RESULT = 'Valid'		 
		end
	end
	
	
	
COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Invalid'
end
END CATCH

RETURN

SET NOCOUNT OFF



GO
/****** Object:  StoredProcedure [dbo].[st_ReviewMembers]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_ReviewMembers] 
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
/****** Object:  StoredProcedure [dbo].[st_ReviewMembers_1]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ReviewMembers_1] 
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
		last_login datetime, review_role nvarchar(50), review_role_2 nvarchar(50),
		is_coding_only bit, is_admin bit)
		
	declare @review_contact_id int
    
    insert into @tb_review_members (contact_id, review_contact_id, contact_name, email)
	select c.CONTACT_ID, rc.REVIEW_CONTACT_ID, c.CONTACT_NAME, c.EMAIL
	from Reviewer.dbo.TB_CONTACT c
	right join Reviewer.dbo.TB_REVIEW_CONTACT rc
	on c.CONTACT_ID = rc.CONTACT_ID
	where rc.REVIEW_ID = @REVIEW_ID
	order by c.CONTACT_NAME
    
    --select * from @tb_review_members
    
    update @tb_review_members 
	set last_login = tt.LAST_LOGIN
	FROm (select max(lt.CREATED)as LAST_LOGIN, lt.CONTACT_ID CID from TB_LOGON_TICKET lt
	where lt.REVIEW_ID = @REVIEW_ID
	group by CONTACT_ID) tt
	where contact_id = CID	
	
    update @tb_review_members
	set is_coding_only = 0
    
    --select * from @tb_review_members
    
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
		
		update @tb_review_members 
		set review_role = 
		(select CRR.ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'AdminUser'
		) 
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
	set is_admin = 1
	where review_role = 'AdminUser'
	
	select * from @tb_review_members
    
    
    
    
    
    
    
    
    
    
    
    
    /*
    CREATE table #REVIEW_MEMBERS
	(
		CONTACT_ID int,
		REVIEW_CONTACT_ID int,
		CONTACT_NAME nvarchar(255),
		EMAIL nvarchar(500),
		LAST_LOGIN datetime,
		REVIEW_ROLE nvarchar(50),
		REVIEW_ROLE_2 nvarchar(50),
		IS_CODING_ONLY bit,
		IS_ADMIN bit
	)
	declare @REVIEW_CONTACT_ID int
	
	
	insert into #REVIEW_MEMBERS (CONTACT_ID, REVIEW_CONTACT_ID, CONTACT_NAME, EMAIL)
	select c.CONTACT_ID, rc.REVIEW_CONTACT_ID, c.CONTACT_NAME, c.EMAIL
	from Reviewer.dbo.TB_CONTACT c
	right join Reviewer.dbo.TB_REVIEW_CONTACT rc
	on c.CONTACT_ID = rc.CONTACT_ID
	where rc.REVIEW_ID = @REVIEW_ID
	order by c.CONTACT_NAME
	
	update #REVIEW_MEMBERS 
	set #REVIEW_MEMBERS.LAST_LOGIN = 
	(select max(lt.CREATED) as LAST_LOGIN from TB_LOGON_TICKET lt
	where #REVIEW_MEMBERS.CONTACT_ID = lt.CONTACT_ID
	and lt.REVIEW_ID = @REVIEW_ID)
	
	
	update #REVIEW_MEMBERS
	set IS_CODING_ONLY = 0
	
	declare @WORKING_REVIEW_CONTACT_ID int
	declare REVIEW_CONTACT_ID_CURSOR cursor for
	select REVIEW_CONTACT_ID FROM #REVIEW_MEMBERS
	open REVIEW_CONTACT_ID_CURSOR
	fetch next from REVIEW_CONTACT_ID_CURSOR
	into @WORKING_REVIEW_CONTACT_ID
	while @@FETCH_STATUS = 0
	begin 		
		-- see if the person is coding only for this review
		update #REVIEW_MEMBERS 
		set #REVIEW_MEMBERS.REVIEW_ROLE_2 = 
		(select ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE
		where REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and ROLE_NAME = 'Coding only'
		) 
		where #REVIEW_MEMBERS.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		
		update #REVIEW_MEMBERS 
		set #REVIEW_MEMBERS.REVIEW_ROLE = 
		(select ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE
		where REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and ROLE_NAME = 'AdminUser'
		) 
		where #REVIEW_MEMBERS.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID

		FETCH NEXT FROM REVIEW_CONTACT_ID_CURSOR 
		INTO @WORKING_REVIEW_CONTACT_ID
	END

	CLOSE REVIEW_CONTACT_ID_CURSOR
	DEALLOCATE REVIEW_CONTACT_ID_CURSOR
	
	update #REVIEW_MEMBERS
	set IS_CODING_ONLY = 1
	where REVIEW_ROLE_2 = 'Coding only'
	
	update #REVIEW_MEMBERS
	set IS_ADMIN = 1
	where REVIEW_ROLE = 'AdminUser'
	
	select * from #REVIEW_MEMBERS
	
	drop table #REVIEW_MEMBERS
    */


END


GO
/****** Object:  StoredProcedure [dbo].[st_ReviewMembers_2]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_ReviewMembers_2] 
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
/****** Object:  StoredProcedure [dbo].[st_ReviewNewNonShareableAddContact]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_ReviewNewNonShareableAddContact]
(
	@REVIEW_ID int,
	@FUNDER_ID int
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 
	declare @REVIEW_CONTACT_ID int
	
	select CONTACT_ID from Reviewer.dbo.TB_REVIEW_CONTACT
	where CONTACT_ID = @FUNDER_ID and REVIEW_ID = @REVIEW_ID
	
	if @@ROWCOUNT = 0  --i.e. it's not already there
	begin
		insert into Reviewer.dbo.TB_REVIEW_CONTACT (REVIEW_ID, CONTACT_ID, old_review_id, old_contact_id)
		values (@REVIEW_ID, @FUNDER_ID, null, null)
		set @REVIEW_CONTACT_ID = @@IDENTITY

		insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
		values (@REVIEW_CONTACT_ID, 'AdminUser')
	end


END




GO
/****** Object:  StoredProcedure [dbo].[st_ReviewRemoveMember]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_ReviewRemoveMember]
(
	@REVIEW_ID int,
	@CONTACT_ID int
)

As

SET NOCOUNT ON

	
	delete Reviewer.dbo.TB_CONTACT_REVIEW_ROLE from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE crr
    inner join Reviewer.dbo.TB_REVIEW_CONTACT rc
    on rc.REVIEW_CONTACT_ID = crr.REVIEW_CONTACT_ID
    and rc.CONTACT_ID = @CONTACT_ID
    and rc.REVIEW_ID = @REVIEW_ID
                
    delete from Reviewer.dbo.TB_REVIEW_CONTACT 
    where CONTACT_ID = @CONTACT_ID
    and REVIEW_ID = @REVIEW_ID
		

SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewRemoveMember_1]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER procedure [dbo].[st_ReviewRemoveMember_1]
(
	@REVIEW_ID int,
	@CONTACT_ID int
)

As

SET NOCOUNT ON

	declare @funder_id int
	
	select @funder_id = FUNDER_ID from Reviewer.dbo.TB_REVIEW
	where REVIEW_ID = @REVIEW_ID
	
	if @funder_id != @CONTACT_ID
	begin
		delete Reviewer.dbo.TB_CONTACT_REVIEW_ROLE from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE crr
		inner join Reviewer.dbo.TB_REVIEW_CONTACT rc
		on rc.REVIEW_CONTACT_ID = crr.REVIEW_CONTACT_ID
		and rc.CONTACT_ID = @CONTACT_ID
		and rc.REVIEW_ID = @REVIEW_ID
	                
		delete from Reviewer.dbo.TB_REVIEW_CONTACT 
		where CONTACT_ID = @CONTACT_ID
		and REVIEW_ID = @REVIEW_ID
    end
		

SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[st_ReviewRoleCodingOnlyUpdate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_ReviewRoleCodingOnlyUpdate]
(
	@REVIEW_ID int,
	@CONTACT_ID int,
	@IS_CHECKED bit
)

As

SET NOCOUNT ON

	declare @REVIEW_CONTACT_ID int
	-- get REVIEW_CONTACT_ID
	select @REVIEW_CONTACT_ID = REVIEW_CONTACT_ID 
    from Reviewer.dbo.TB_REVIEW_CONTACT
    where REVIEW_ID = @REVIEW_ID
    and CONTACT_ID = @CONTACT_ID
	
	
	if @IS_CHECKED = 1
	begin
		insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
		values (@REVIEW_CONTACT_ID, 'Coding only')
	end
	else
	begin
		delete from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE 
		where REVIEW_CONTACT_ID = @REVIEW_CONTACT_ID
		and ROLE_NAME = 'Coding only'
	end
SET NOCOUNT OFF



GO
/****** Object:  StoredProcedure [dbo].[st_ReviewRoleIsAdminUpdate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_ReviewRoleIsAdminUpdate]
(
	@REVIEW_ID int,
	@CONTACT_ID int,
	@IS_CHECKED bit
)

As

SET NOCOUNT ON

	declare @REVIEW_CONTACT_ID int
	declare @REVIEW_OWNER int
	
	-- get REVIEW_CONTACT_ID
	select @REVIEW_OWNER = FUNDER_ID from Reviewer.dbo.TB_REVIEW
		where REVIEW_ID = @REVIEW_ID
	
	if @CONTACT_ID != @REVIEW_OWNER  -- do nothing if this is review owner
	begin
		select @REVIEW_CONTACT_ID = REVIEW_CONTACT_ID 
		from Reviewer.dbo.TB_REVIEW_CONTACT
		where REVIEW_ID = @REVIEW_ID
		and CONTACT_ID = @CONTACT_ID
			
		if @IS_CHECKED = 1
		begin
			insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
			values (@REVIEW_CONTACT_ID, 'AdminUser')
			
			delete from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE 
			where REVIEW_CONTACT_ID = @REVIEW_CONTACT_ID
			and ROLE_NAME = 'RegularUser'
		end
		else
		begin
			delete from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE 
			where REVIEW_CONTACT_ID = @REVIEW_CONTACT_ID
			and ROLE_NAME = 'AdminUser'
			
			-- to avoid inserting extra 'RegularUser' rows
			select REVIEW_CONTACT_ID from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE
			where REVIEW_CONTACT_ID = @REVIEW_CONTACT_ID
			and ROLE_NAME = 'RegularUser'
			if @@ROWCOUNT = 0
			begin
				insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
				values (@REVIEW_CONTACT_ID, 'RegularUser')
			end
		end
	end
SET NOCOUNT OFF




GO
/****** Object:  StoredProcedure [dbo].[st_ReviewRoleReadOnlyUpdate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_ReviewRoleReadOnlyUpdate]
(
	@REVIEW_ID int,
	@CONTACT_ID int,
	@IS_CHECKED bit
)

As

SET NOCOUNT ON

	declare @REVIEW_CONTACT_ID int
	-- get REVIEW_CONTACT_ID
	select @REVIEW_CONTACT_ID = REVIEW_CONTACT_ID 
    from Reviewer.dbo.TB_REVIEW_CONTACT
    where REVIEW_ID = @REVIEW_ID
    and CONTACT_ID = @CONTACT_ID
	
	
	if @IS_CHECKED = 1
	begin
		insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
		values (@REVIEW_CONTACT_ID, 'ReadOnlyUser')
	end
	else
	begin
		delete from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE 
		where REVIEW_CONTACT_ID = @REVIEW_CONTACT_ID
		and ROLE_NAME = 'ReadOnlyUser'
	end
SET NOCOUNT OFF




GO
/****** Object:  StoredProcedure [dbo].[st_ReviewRolesAll]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ReviewRolesAll] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select * from Reviewer.dbo.TB_REVIEW_ROLE

	RETURN

END

GO
/****** Object:  StoredProcedure [dbo].[st_ReviewRoleUpdate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_ReviewRoleUpdate]
(
	@REVIEW_CONTACT_ID int,
	@ROLE nvarchar(50)
)

As

SET NOCOUNT ON

	insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
	values (@REVIEW_CONTACT_ID, @ROLE)

SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[st_ReviewRoleUpdateByContactID]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE OR ALTER      procedure [dbo].[st_ReviewRoleUpdateByContactID]
(
	@REVIEW_ID int,
	@CONTACT_ID int,
	@ROLE_NAME nvarchar(50)
)

As

SET NOCOUNT ON



	declare @REVIEW_CONTACT_ID int
	-- get REVIEW_CONTACT_ID
	set @REVIEW_CONTACT_ID = (select REVIEW_CONTACT_ID 
    from Reviewer.dbo.TB_REVIEW_CONTACT
    where REVIEW_ID = @REVIEW_ID
    and CONTACT_ID = @CONTACT_ID)

	if ((@REVIEW_CONTACT_ID = '') or (@REVIEW_CONTACT_ID is null)) 
	begin
		-- something is going wrong so let's not do anything
		declare @RESULT int = 1
	end
	else
	begin
		-- remove all existing roles for this user in this review (to avoid duplicates)
		delete from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE 
			where REVIEW_CONTACT_ID = @REVIEW_CONTACT_ID

		-- add this role
		insert into Reviewer.dbo.TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME)
			values (@REVIEW_CONTACT_ID, @ROLE_NAME)
	end



SET NOCOUNT OFF



GO
/****** Object:  StoredProcedure [dbo].[st_SendNewsletterStatus]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_SendNewsletterStatus]
(
	@CONTACT_ID int,
	@SEND_STATUS bit
)

As

SET NOCOUNT ON


	update Reviewer.dbo.TB_CONTACT 
	set SEND_NEWSLETTER = @SEND_STATUS
	where CONTACT_ID = @CONTACT_ID


SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[st_ShowCodingStatus]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_ShowCodingStatus]
(
	@WEBDB_ID int,
	@SHOW_CODING bit
)

As

SET NOCOUNT ON


	update Presenter.dbo.TB_WEB_DATABASE 
	set SHOW_CODING = @SHOW_CODING
	where WEBDB_ID = @WEBDB_ID


SET NOCOUNT OFF



GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Activate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	manually activate a site license (not through the shop!)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Activate]
	-- Add the parameters for the stored procedure here
	@lic_id int
	, @admin_ID int
	, @lic_details_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- do all checks: see if you can get a joined record of site_lic, 
	-- with a site_lic_details line that needs to be activated and a for_sale line that is active and can be administered by @admin_ID
	declare @check int, @res int = 1
	set @check = (
				select count(sld.SITE_LIC_DETAILS_ID) from Reviewer.dbo.TB_SITE_LIC sl 
					inner join TB_SITE_LIC_DETAILS sld on sl.SITE_LIC_ID = sld.SITE_LIC_ID and sld.VALID_FROM is null 
						and sld.SITE_LIC_DETAILS_ID = @lic_details_ID
					inner join TB_FOR_SALE fs on sld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 'True'
					inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and 
						(@admin_ID = la.CONTACT_ID or 
						@admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)
						)
				)
	if @check < 1 return -1 --don't really know what to do if this fails!
	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
	BEGIN TRANSACTION
		declare @months int
		set @months = (select  d.MONTHS from TB_SITE_LIC_DETAILS d
						inner join Reviewer.dbo.TB_SITE_LIC l on l.SITE_LIC_ID = d.SITE_LIC_ID
						where SITE_LIC_DETAILS_ID = @lic_details_ID)
		--update expiration date
		update Reviewer.dbo.TB_SITE_LIC 
			set EXPIRY_DATE = CASE when [EXPIRY_DATE] is not null and [EXPIRY_DATE] > GETDATE()
									then DATEADD(month, @months, [EXPIRY_DATE])
								else DATEADD(month, @months, GETDATE())
								end
		where SITE_LIC_ID = @lic_id
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			return -2
		end
		-- update valid from
		update TB_SITE_LIC_DETAILS set VALID_FROM = GETDATE() where SITE_LIC_DETAILS_ID = @lic_details_ID
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			return -2
		end
		-- make the offer inactive
		update TB_FOR_SALE set IS_ACTIVE = 'False'
		from TB_FOR_SALE fs inner join TB_SITE_LIC_DETAILS d on d.FOR_SALE_ID = fs.FOR_SALE_ID and d.SITE_LIC_DETAILS_ID = @lic_details_ID
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			return -2
		end
		-- create a record in the log
		insert into TB_SITE_LIC_LOG
		(
		  [SITE_LIC_DETAILS_ID]
		  ,[CONTACT_ID]
		  ,[AFFECTED_ID]
		  ,[CHANGE_TYPE]
		  ,[REASON]
		) values
		(
		  @lic_details_ID
		  ,@admin_ID
		  ,@lic_details_ID
		  ,'ACTIVATE'
		  ,'This was done manually, not via the shop'
		)
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			return -2
		end
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	set @res = -2 --to tell the code data was not committed!
	END CATCH
	
	return @res

END

GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_New]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Add_New] 
	-- Add the parameters for the stored procedure here
	@creator_id INT
	, @admin_id  int -- who will be the first administrator of the site lic, 
	, @Accounts int 
	, @reviews int
	, @months int
	, @Totalprice int
	, @SITE_LIC_NAME nvarchar(50)
	, @COMPANY_NAME nvarchar(50) = ''
	, @COMPANY_ADDRESS nvarchar(500) = ''
	, @TELEPHONE nvarchar(50) = ''
	, @NOTES nvarchar(4000) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    declare @ck int = 0
    set @ck = (SELECT COUNT(contact_id) from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @creator_id and IS_SITE_ADMIN = 1)
    if @ck <1 return -1 --only site_admins can create new site licenses!
	declare @L_ID int, @FS_ID int
	insert into Reviewer.dbo.TB_SITE_LIC 
		(
		SITE_LIC_NAME
		, COMPANY_NAME
		, COMPANY_ADDRESS
		, TELEPHONE
		, NOTES
		, CREATOR_ID
		)
		VALUES
		(
		@SITE_LIC_NAME
		, @COMPANY_NAME
		, @COMPANY_ADDRESS
		, @TELEPHONE
		, @NOTES
		, @creator_id
		)
	
	set @L_ID = @@IDENTITY
	
	insert into TB_FOR_SALE 
	(
	  [TYPE_NAME]
      ,[IS_ACTIVE]
      ,[LAST_CHANGED]
      ,[PRICE_PER_MONTH]
      ,[DETAILS]
     )
    VALUES
    (
     'Site License'
     , 1
     , GETDATE()
     ,CAST((@Totalprice/@months) as int)
     ,''
    )
    
    set @FS_ID = @@IDENTITY
    
    INSERT into TB_SITE_LIC_DETAILS
    ( 
	  [SITE_LIC_ID]
      ,[FOR_SALE_ID]
      ,[ACCOUNTS_ALLOWANCE]
      ,[REVIEWS_ALLOWANCE]
      ,[MONTHS]
      ,[VALID_FROM]
    )
    values
    (
     @L_ID
     ,@FS_ID
     ,@Accounts
     ,@reviews
     ,@months
     ,null
    )
    insert into TB_SITE_LIC_ADMIN
    ( 
     [SITE_LIC_ID]
     ,[CONTACT_ID]
    ) VALUES (
     @L_ID
     ,@admin_id
    ) 
END

GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Remove_Admin]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Add_Remove_Admin]
	-- Add the parameters for the stored procedure here
	@lic_id int
	, @admin_ID int
	, @contact_email nvarchar(500)
	, @res int output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here
    declare @contact_id int
	set @res = 0
    
    -- initial check to see if email exists
	select @contact_id = CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = @contact_email
	if @@ROWCOUNT = 1 
	begin
		declare @ck int
		set @ck = (SELECT count(contact_id) from TB_SITE_LIC_ADMIN 
		where (CONTACT_ID = @admin_ID or @admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1))
				and SITE_LIC_ID = @lic_id)
		if @ck < 1 set @res = -1 --first check: see if supplied admin is actually and admin!
		else if  (select count(contact_id) from Reviewer.dbo.TB_CONTACT where @contact_email = EMAIL and @contact_id = CONTACT_ID) != 1
			--second check, see if c_id and email exist
			set @res = -2
		else -- checks went OK, let's see if we can do it
		begin
			set @ck = (select count(contact_id) from TB_SITE_LIC_ADMIN where CONTACT_ID = @contact_id and SITE_LIC_ID = @lic_id)
			if @ck = 1 -- contact is an admin, we should remove it
			begin
				delete from TB_SITE_LIC_ADMIN where CONTACT_ID = @contact_id and @lic_id = SITE_LIC_ID
				if @@ROWCOUNT = 1
				begin --write success log
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin'
						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc
				end
				else --write failure log, if this is fired, there is a bug somewhere
				begin
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin: failed!'
						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc	
					set @res = -3
				end
			end	
			
			else --contact is not an admin, we should add it
			begin
				insert into TB_SITE_LIC_ADMIN (CONTACT_ID, SITE_LIC_ID)
				values (@contact_id, @lic_id)
				if @@ROWCOUNT = 1
				begin
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin'
						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc
				end
				else
				begin
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin: failed!'
						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc	
					set @res = -4
				end
			end
		end
	end
	--select c.CONTACT_ID, CONTACT_NAME, EMAIL from TB_SITE_LIC_ADMIN a
	--	inner join Reviewer.dbo.TB_CONTACT c on a.CONTACT_ID = c.CONTACT_ID
	--where SITE_LIC_ID = @lic_id
	
	return @res 
	-- error codes: -1 = supplied admin_id is not an admin of this site lice
	--				-2 = contact_id already in a site license
	--				-3 = no allowance available, all account slots for current license have been used
	--				-4 = tried to remove account but couldn't write changes! BUG ALERT
	--				-5 = tried to add account but couldn't write changes! BUG ALERT
	--				-6 = email check returned no contact_id or multiple contact_ids
	
END




GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Remove_Admin_1]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Add_Remove_Admin_1]
	-- Add the parameters for the stored procedure here
	@lic_id int
	, @admin_ID int
	, @contact_email nvarchar(500)
	, @res int output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here
    declare @contact_id int
	set @res = 0
    
    -- initial check to see if email exists
	select @contact_id = CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = @contact_email
	if @@ROWCOUNT = 1 
	begin
		declare @ck int
		set @ck = (SELECT count(contact_id) from TB_SITE_LIC_ADMIN 
		where (CONTACT_ID = @admin_ID or @admin_ID in (select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1))
				and SITE_LIC_ID = @lic_id)
		if @ck < 1 set @res = -1 --first check: see if supplied admin is actually and admin!
		else if  (select count(contact_id) from Reviewer.dbo.TB_CONTACT where @contact_email = EMAIL and @contact_id = CONTACT_ID) != 1
			--second check, see if c_id and email exist
			set @res = -2
		else -- checks went OK, let's see if we can do it
		begin
			set @ck = (select count(contact_id) from TB_SITE_LIC_ADMIN where CONTACT_ID = @contact_id and SITE_LIC_ID = @lic_id)
			if @ck = 1 -- contact is an admin, we should remove it
			begin
				delete from TB_SITE_LIC_ADMIN where CONTACT_ID = @contact_id and @lic_id = SITE_LIC_ID
				if @@ROWCOUNT = 1
				begin --write success log
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin'
						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc
				end
				else --write failure log, if this is fired, there is a bug somewhere
				begin
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove admin: failed!'
						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc	
					set @res = -3
				end
			end	
			
			else --contact is not an admin, we should add it
			begin
				insert into TB_SITE_LIC_ADMIN (CONTACT_ID, SITE_LIC_ID)
				values (@contact_id, @lic_id)
				if @@ROWCOUNT = 1
				begin
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin'
						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc
				end
				else
				begin
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add admin: failed!'
						from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc	
					set @res = -4
				end
			end
		end
	end
	--select c.CONTACT_ID, CONTACT_NAME, EMAIL from TB_SITE_LIC_ADMIN a
	--	inner join Reviewer.dbo.TB_CONTACT c on a.CONTACT_ID = c.CONTACT_ID
	--where SITE_LIC_ID = @lic_id
	
	return @res 
	-- error codes: -1 = supplied admin_id is not an admin of this site lice
	--				-2 = contact_id already in a site license
	--				-3 = no allowance available, all account slots for current license have been used
	--				-4 = tried to remove account but couldn't write changes! BUG ALERT
	--				-5 = tried to add account but couldn't write changes! BUG ALERT
	--				-6 = email check returned no contact_id or multiple contact_ids
	
END




GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Remove_Contact]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Add_Remove_Contact]
(
	  @lic_id int
	, @admin_ID int
	, @contact_email nvarchar(500)
	, @res int output
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	-- Insert statements for procedure here
	
	declare @contact_id int
	set @res = 0

	-- initial check to see if email exists
	select @contact_id = CONTACT_ID from Reviewer.dbo.TB_CONTACT where EMAIL = @contact_email
	if @@ROWCOUNT = 1 
	begin
		declare @ck int, @ck2 int = 0, @acc_all int
		set @ck = (SELECT count(contact_id) from TB_SITE_LIC_ADMIN 
			where (CONTACT_ID = @admin_ID or @admin_ID in 
				(select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)) 
			and SITE_LIC_ID = @lic_id)
		set @acc_all = (select top 1 ACCOUNTS_ALLOWANCE from TB_SITE_LIC_DETAILS 
			where SITE_LIC_ID = @lic_id and VALID_FROM is not null order by VALID_FROM desc)
		set @ck2 = (select @acc_all - COUNT(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT 
			where SITE_LIC_ID = @lic_id)
		if @ck < 1 set @res = -1 --first check: see if supplied admin id is actually an admin!
		
		else if  (select count(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT 
			where @contact_id = CONTACT_ID and SITE_LIC_ID != @lic_id) != 0
			--second check, see if contact_id is already in a site license (not including this one)
			set @res = -2	
		
		else -- checks went OK, let's see if we can do it
		begin
			set @ck = (select count(contact_id) from Reviewer.dbo.TB_SITE_LIC_CONTACT where CONTACT_ID = @contact_id)
			if @ck = 1 -- contact is in the license, we should remove it
			begin
				delete from Reviewer.dbo.TB_SITE_LIC_CONTACT where CONTACT_ID = @contact_id and @lic_id = SITE_LIC_ID
				if @@ROWCOUNT = 1
				begin --write success log
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove contact'
						from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc
				end
				else --write failure log, if this is fired, there is a bug somewhere
				begin
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'remove contact: failed!'
						from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc	
					set @res = -4
				end
			end	
			
			else if @ck2 < 1 --accounts allowance is all used up
			set @res = -3
			
			else --contact is not in the license, we should add it
			begin
				insert into Reviewer.dbo.TB_SITE_LIC_CONTACT (CONTACT_ID, SITE_LIC_ID)
				values (@contact_id, @lic_id)
				if @@ROWCOUNT = 1
				begin
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add contact'
						from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc
				end
				else
				begin
					insert into TB_SITE_LIC_LOG (
						[SITE_LIC_DETAILS_ID]
						  ,[CONTACT_ID]
						  ,[AFFECTED_ID]
						  ,[CHANGE_TYPE]
					) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @contact_id, 'add contact: failed!'
						from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
						order by DATE_CREATED desc	
					set @res = -5
				end
			end
		end
	end
	else
	begin
		set @res = -6
	end
	
	--select c.CONTACT_ID, CONTACT_NAME, EMAIL from Reviewer.dbo.TB_SITE_LIC_CONTACT lc 
	--	inner join Reviewer.dbo.TB_CONTACT c on lc.CONTACT_ID = c.CONTACT_ID
	--	where SITE_LIC_ID = @lic_id
	
	return @res
	-- error codes: -1 = supplied admin_id is not an admin of this site lice
	--				-2 = contact_id already in a site license
	--				-3 = no allowance available, all account slots for current license have been used
	--				-4 = tried to remove account but couldn't write changes! BUG ALERT
	--				-5 = tried to add account but couldn't write changes! BUG ALERT
	--				-6 = email check returned no contact_id or multiple contact_ids
END



GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Add_Review]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Add_Review]
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



GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Archive_Review]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_Site_Lic_Archive_Review] 
	-- Add the parameters for the stored procedure here
(
	  @LIC_ID nvarchar(50)
	, @SITE_LIC_DETAILS_ID nvarchar(50)
	, @admin_id INT 
	, @review_id nvarchar(50)
	, @RESULT nvarchar(255) output
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	set @RESULT = 'Success'
	
	if @SITE_LIC_DETAILS_ID != 'N/A' 
	BEGIN

		declare @previousPackageID int
		set @previousPackageID = (select top 1 SITE_LIC_DETAILS_ID from TB_SITE_LIC_DETAILS
			where SITE_LIC_ID = @LIC_ID
			and SITE_LIC_DETAILS_ID != @SITE_LIC_DETAILS_ID
			and VALID_FROM is not null
			order by SITE_LIC_DETAILS_ID)

		if @previousPackageID != ''
		BEGIN
			-- change the package
			update Reviewer.dbo.TB_SITE_LIC_REVIEW
			set SITE_LIC_DETAILS_ID = @previousPackageID
			where SITE_LIC_ID = @LIC_ID
			and REVIEW_ID = @review_id
			and SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID

			-- log what has happened
			insert into TB_SITE_LIC_LOG ([SITE_LIC_DETAILS_ID], [CONTACT_ID], 
					[AFFECTED_ID],[CHANGE_TYPE], [REASON])
			values (@previousPackageID, @admin_id, @review_id,
				'ARCHIVE', 'This was done manually by eppiadmin')			
		END
		else
		begin
			set @RESULT = 'Unable to move review'
		end


	END
	else
	begin
		set @RESULT = 'Unable to move review'
	end
	
	return

END






GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Create_or_Edit]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_Site_Lic_Create_or_Edit] 
	-- Add the parameters for the stored procedure here
(
	  @LIC_ID nvarchar(50)
	, @creator_id INT
	, @admin_id  int -- who will be the first administrator of the site lic, 
	, @SITE_LIC_NAME nvarchar(50)
	, @COMPANY_NAME nvarchar(50)
	, @COMPANY_ADDRESS nvarchar(500)
	, @TELEPHONE nvarchar(50)
	, @NOTES nvarchar(4000)
	, @EPPI_NOTES nvarchar(4000)
	, @DATE_CREATED datetime
	, @SITE_LIC_MODEL int
	, @RESULT nvarchar(50) output
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @ck int
	
	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
	BEGIN TRANSACTION
	if @LIC_ID = 'N/A' -- we are creating a new site license
	BEGIN		
		declare @L_ID int
		
		insert into Reviewer.dbo.TB_SITE_LIC (SITE_LIC_NAME, COMPANY_NAME,
			COMPANY_ADDRESS, TELEPHONE, NOTES, EPPI_NOTES, CREATOR_ID, SITE_LIC_MODEL)
		VALUES (@SITE_LIC_NAME, @COMPANY_NAME, @COMPANY_ADDRESS, @TELEPHONE,
			@NOTES, @EPPI_NOTES, @creator_id, @SITE_LIC_MODEL)	
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback1'
			return 
		end
		else
		begin
			set @L_ID = @@IDENTITY
			insert into TB_SITE_LIC_ADMIN([SITE_LIC_ID], [CONTACT_ID]) 
			VALUES (@L_ID, @admin_id)
			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
			begin
				ROLLBACK TRANSACTION
				set @RESULT = 'rollback2'
				return 
			end
			else
			begin
				set @RESULT = @L_ID
			end
		end		
	END
	else
	BEGIN
		-- get the existing top admin
		declare @TOP_ADM_ID int
		set @TOP_ADM_ID = (select top 1 CONTACT_ID from TB_SITE_LIC_ADMIN where SITE_LIC_ID = @LIC_ID)
		


				
		-- we are editing the site license details
		update Reviewer.dbo.TB_SITE_LIC
		set SITE_LIC_NAME = @SITE_LIC_NAME, 
		COMPANY_NAME = @COMPANY_NAME,
		COMPANY_ADDRESS = @COMPANY_ADDRESS,
		TELEPHONE = @TELEPHONE,
		NOTES = @NOTES,
		EPPI_NOTES = @EPPI_NOTES,
		SITE_LIC_MODEL = @SITE_LIC_MODEL,
		DATE_CREATED = @DATE_CREATED
		where SITE_LIC_ID = @LIC_ID
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback3'
			return 
		end
		else
		begin	
			-- check if @admin_id is at top of list of admins
			if @TOP_ADM_ID = @admin_id -- already top admin so nothing to do
			begin
				set @RESULT = 'valid'
			end
			else  -- replace the list of admins with a new list where @admin_id is at the test
			begin
				-- create a table to contain what the admin list should look like
				declare @tb_listOfAdmins table (tv_site_lic_admin_id int, tv_site_lic_id int, tv_contact_id int)

				-- top admin
				insert into @tb_listOfAdmins (tv_site_lic_admin_id, tv_site_lic_id, tv_contact_id)
				values (0, @LIC_ID, @admin_id)

				-- rest of the admins
				insert into @tb_listOfAdmins (tv_site_lic_admin_id, tv_site_lic_id, tv_contact_id)
				select SITE_LIC_ADMIN_ID, SITE_LIC_ID, CONTACT_ID from TB_SITE_LIC_ADMIN
				where CONTACT_ID != @admin_id

				-- delete the existing list
				delete from TB_SITE_LIC_ADMIN where SITE_LIC_ID = @LIC_ID
				
				-- insert the new list
				insert into TB_SITE_LIC_ADMIN (SITE_LIC_ID, CONTACT_ID)
				select tv_site_lic_id, tv_contact_id from @tb_listOfAdmins order by tv_site_lic_admin_id
				
				-- check that the top admin is now there
				select * from TB_SITE_LIC_ADMIN where CONTACT_ID = @admin_id and SITE_LIC_ID = @LIC_ID
				if @@ROWCOUNT = 0 -- top admin is missing
				begin
					ROLLBACK TRANSACTION
					set @RESULT = 'rollback4'
					return 
				end
				else
				begin			
					set @RESULT = 'valid'
				end
			end
							
		end


	END
	
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	set @RESULT = 'rollback' --to tell the code data was not committed!
	END CATCH
	
	return 

END






GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Create_or_Edit_1]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Create_or_Edit_1] 
	-- Add the parameters for the stored procedure here
(
	  @LIC_ID nvarchar(50)
	, @creator_id INT
	, @admin_id  int -- who will be the first administrator of the site lic, 
	, @SITE_LIC_NAME nvarchar(50)
	, @COMPANY_NAME nvarchar(50)
	, @COMPANY_ADDRESS nvarchar(500)
	, @TELEPHONE nvarchar(50)
	, @NOTES nvarchar(4000)
	, @DATE_CREATED datetime
	, @RESULT nvarchar(50) output
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @ck int
	
	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
	BEGIN TRANSACTION
	if @LIC_ID = 'N/A' -- we are creating a new site license
	BEGIN		
		declare @L_ID int
		
		insert into Reviewer.dbo.TB_SITE_LIC (SITE_LIC_NAME, COMPANY_NAME,
			COMPANY_ADDRESS, TELEPHONE, NOTES, CREATOR_ID)
		VALUES (@SITE_LIC_NAME, @COMPANY_NAME, @COMPANY_ADDRESS, @TELEPHONE,
			@NOTES, @creator_id)	
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback1'
			return 
		end
		else
		begin
			set @L_ID = @@IDENTITY
			insert into TB_SITE_LIC_ADMIN([SITE_LIC_ID], [CONTACT_ID]) 
			VALUES (@L_ID, @admin_id)
			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
			begin
				ROLLBACK TRANSACTION
				set @RESULT = 'rollback2'
				return 
			end
			else
			begin
				set @RESULT = @L_ID
			end
		end		
	END
	else
	BEGIN
		-- get the existing admin
		declare @L_ADM_ID int
		select @L_ADM_ID = CONTACT_ID from TB_SITE_LIC_ADMIN where SITE_LIC_ID = @LIC_ID
				
		-- we are editing the site license details
		update Reviewer.dbo.TB_SITE_LIC
		set SITE_LIC_NAME = @SITE_LIC_NAME, 
		COMPANY_NAME = @COMPANY_NAME,
		COMPANY_ADDRESS = @COMPANY_ADDRESS,
		TELEPHONE = @TELEPHONE,
		NOTES = @NOTES,
		DATE_CREATED = @DATE_CREATED
		where SITE_LIC_ID = @LIC_ID
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback3'
			return 
		end
		else
		begin			
			update TB_SITE_LIC_ADMIN
			set CONTACT_ID = @admin_id
			where CONTACT_ID = @L_ADM_ID
			and SITE_LIC_ID = @LIC_ID
			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
			begin
				ROLLBACK TRANSACTION
				set @RESULT = 'rollback4'
				return 
			end
			else
			begin			
				set @RESULT = 'valid'
			end
		end		

	END
	
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	set @RESULT = 'rollback' --to tell the code data was not committed!
	END CATCH
	
	return 

END





GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Details_Create_or_Edit_AndOr_Activate]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO








-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER     PROCEDURE [dbo].[st_Site_Lic_Details_Create_or_Edit_AndOr_Activate] 
	-- Add the parameters for the stored procedure here
(
	  @LIC_ID int
	, @SITE_LIC_DETAILS_ID nvarchar(50)
	, @creator_id INT 
	, @Accounts int 
	, @reviews int
	, @months int
	, @Totalprice int
	, @pricePerMonth int
	, @dateCreated date
	, @forSaleID nvarchar(50)
	, @VALID_FROM nvarchar(50)
	, @EXPIRY_DATE nvarchar(50)
	, @ER4_ADM nvarchar(50)
	, @EXTENSION_TYPE nvarchar(50)
	, @EXTENSION_NOTES nvarchar(4000)
	, @SITE_LIC_MODEL int
	, @LATEST_SITE_LIC_DETAILS_ID nvarchar(50)
	, @RESULT nvarchar(50) output
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @ck int
	
	BEGIN TRY	--to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
	BEGIN TRANSACTION
	
	if @SITE_LIC_DETAILS_ID = 'N/A' -- we are creating a new site license package
	BEGIN	
	
		declare @FS_ID int
		declare @SLD_ID int
		
		insert into TB_FOR_SALE ([TYPE_NAME],[IS_ACTIVE],[LAST_CHANGED],
			[PRICE_PER_MONTH],[DETAILS])
		VALUES ('Site License', 1, GETDATE(),CAST((@Totalprice/@months) as int),'')
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback'
			return 
		end
		else
		begin
			set @FS_ID = @@IDENTITY
			INSERT into TB_SITE_LIC_DETAILS ([SITE_LIC_ID],[FOR_SALE_ID],
			[ACCOUNTS_ALLOWANCE],[REVIEWS_ALLOWANCE],[MONTHS],[VALID_FROM])
			values (@LIC_ID,@FS_ID,@Accounts,@reviews,@months,null)
				
			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
			begin
				ROLLBACK TRANSACTION
				set @RESULT = 'rollback'
				return 
			end
			else
			begin
				set @SLD_ID = @@IDENTITY
				
				if (@VALID_FROM != '') and (@EXPIRY_DATE != '')
				begin
					-- activate the package
					update Reviewer.dbo.TB_SITE_LIC 
					set EXPIRY_DATE = @EXPIRY_DATE
					where SITE_LIC_ID = @LIC_ID
				
					update TB_SITE_LIC_DETAILS 
					set VALID_FROM = @VALID_FROM
					where SITE_LIC_DETAILS_ID = @SLD_ID
					
					update TB_FOR_SALE 
					set IS_ACTIVE = 'False'
					from TB_FOR_SALE fs 
					inner join TB_SITE_LIC_DETAILS d on d.FOR_SALE_ID = fs.FOR_SALE_ID 
					and d.SITE_LIC_DETAILS_ID = @SLD_ID	
					
					insert into TB_SITE_LIC_LOG ([SITE_LIC_DETAILS_ID], [CONTACT_ID], 
						[AFFECTED_ID],[CHANGE_TYPE], [REASON])
					values (@SITE_LIC_DETAILS_ID, @ER4_ADM, @SITE_LIC_DETAILS_ID,
						'ACTIVATE', 'This was done manually, not via the shop')	
						
					set @RESULT = 'valid'
				end
				else
				begin
					set @RESULT = 'new offer'
				end
			end
		end

	END
	else
	BEGIN
		-- this is an existing site license package

		update TB_SITE_LIC_DETAILS
			set ACCOUNTS_ALLOWANCE = @Accounts, 
			REVIEWS_ALLOWANCE = @reviews,
			DATE_CREATED = @dateCreated,
			MONTHS = @months
			where SITE_LIC_ID = @LIC_ID
			and SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
		if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
		begin
			ROLLBACK TRANSACTION
			set @RESULT = 'rollback'
			return 
		end
		else
		begin	
			update TB_FOR_SALE
				set LAST_CHANGED = GETDATE(),
				PRICE_PER_MONTH = @pricePerMonth
				where FOR_SALE_ID = @forSaleID
			if @@ROWCOUNT != 1 --check that one record was affected, if not rollback and return with error code
			begin
				ROLLBACK TRANSACTION
				set @RESULT = 'rollback'
				return 
			end
			else
			begin
				if @EXTENSION_TYPE > 0
				begin
					-- put an entry into TB_EXPIRY_EDIT_LOG			
					insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
						OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
					select GETDATE(), 2, @SITE_LIC_DETAILS_ID, @EXPIRY_DATE, s_l.EXPIRY_DATE, @ER4_ADM, @EXTENSION_TYPE, @EXTENSION_NOTES 
					from Reviewer.dbo.TB_SITE_LIC s_l
					where s_l.SITE_LIC_ID = @LIC_ID
				end
			end

			if (@VALID_FROM != '') and (@EXPIRY_DATE != '') 
			begin

				-- is this an activation?				
				declare @validFrom datetime
				declare @initialModelType int
				declare @rm2rm int = 0
				set @validFrom = (select VALID_FROM from TB_SITE_LIC_DETAILS
					where SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID)
				-- if VALID_FROM is null in the database for this package but a value is being passed 
				-- for @VALID_FROM then it is a renewal/activation
				if @validFrom is null
				begin
					-- we are activating but are we renewing as well?
					-- (activing the first package in a license is not a renewal!)
					-- is there a 'latest' package?
					if @LATEST_SITE_LIC_DETAILS_ID != 'NA'
					begin
						-- we are renewing
						-- but are we renewing as 'removeable' to 'removeable'
						set @initialModelType = (select SITE_LIC_MODEL from Reviewer.dbo.TB_SITE_LIC 
						where SITE_LIC_ID = @LIC_ID)
						if @initialModelType = 2 and @SITE_LIC_MODEL = 2
						begin
							set @rm2rm = 1
						end
					end
				end

				-- change expiry date or activate the package
				update Reviewer.dbo.TB_SITE_LIC 
				set EXPIRY_DATE = @EXPIRY_DATE
				where SITE_LIC_ID = @LIC_ID
			
				update TB_SITE_LIC_DETAILS 
				set VALID_FROM = @VALID_FROM
				where SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
				
				update TB_FOR_SALE 
				set IS_ACTIVE = 'False'
				from TB_FOR_SALE fs 
				inner join TB_SITE_LIC_DETAILS d on d.FOR_SALE_ID = fs.FOR_SALE_ID 
				and d.SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID	
				
				insert into TB_SITE_LIC_LOG (
				  [SITE_LIC_DETAILS_ID], [CONTACT_ID], [AFFECTED_ID]
				  ,[CHANGE_TYPE], [REASON])
				values (
				  @SITE_LIC_DETAILS_ID, @ER4_ADM, @SITE_LIC_DETAILS_ID
				  ,'Activate or Extension', 'This was done manually, not via the shop')		
				  				
				-- update the license type (it might be a change)
				update Reviewer.dbo.TB_SITE_LIC 
				set SITE_LIC_MODEL = @SITE_LIC_MODEL
				where SITE_LIC_ID = @LIC_ID

				-- is this a 'removeable' to 'removeable' renewal
				if @rm2rm = 1
				begin
					
					-- how many slots are availble in the new package?
					declare @reviewsAllowance int = 
						(select REVIEWS_ALLOWANCE from TB_SITE_LIC_DETAILS 
						where SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID)

					-- rather than removing reviews from the license when the renewal has fewer slots the 
					-- check will be made in code and the EPPIAdmin can sort it out
					update Reviewer.dbo.TB_SITE_LIC_REVIEW 
					set SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
					where SITE_LIC_DETAILS_ID = @LATEST_SITE_LIC_DETAILS_ID
					and SITE_LIC_ID = @LIC_ID --extra check for safety
				end



			end
			set @RESULT = 'valid'

		end

	END

	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	set @RESULT = 'rollback' --to tell the code data was not committed!
	END CATCH
	
	return

END






GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Edit_By_LicAdm]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER procedure [dbo].[st_Site_Lic_Edit_By_LicAdm]
(
	@SITE_LIC_ID int,
	@COMPANY_NAME nvarchar(500),
	@COMPANY_ADDRESS nvarchar(500),
	@TELEPHONE nvarchar(50),
	@NOTES nvarchar(2000)
)

As

SET NOCOUNT ON

	update Reviewer.dbo.TB_SITE_LIC
	set COMPANY_NAME = @COMPANY_NAME, 
	COMPANY_ADDRESS = @COMPANY_ADDRESS,
	TELEPHONE = @TELEPHONE,
	NOTES = @NOTES
	where SITE_LIC_ID = @SITE_LIC_ID

SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Extension_Record_Get]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Extension_Record_Get] 
(
	@SITE_LIC_DETAILS_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select e_e_l.EXPIRY_EDIT_ID ,e_e_l.DATE_OF_EDIT, e_e_l.TYPE_EXTENDED, e_e_l.ID_EXTENDED, 
	e_e_l.OLD_EXPIRY_DATE, e_e_l.NEW_EXPIRY_DATE,
	e_e_l.EXTENDED_BY_ID, c.CONTACT_NAME, e_t.EXTENSION_TYPE, 
	e_e_l.EXTENSION_NOTES
	from TB_EXPIRY_EDIT_LOG e_e_l
	inner join TB_EXTENSION_TYPES e_t on e_t.EXTENSION_TYPE_ID = e_e_l.EXTENSION_TYPE_ID
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = e_e_l.EXTENDED_BY_ID
	where e_e_l.ID_EXTENDED = @SITE_LIC_DETAILS_ID
	and e_e_l.TYPE_EXTENDED = 2

	RETURN

END



GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Get] 
	-- Add the parameters for the stored procedure here
	
	@admin_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @License TABLE
(
    SITE_LIC_ID int,
    SITE_LIC_NAME nvarchar(50),
	COMPANY_NAME nvarchar(50),
	COMPANY_ADDRESS nvarchar(500),
	TELEPHONE nvarchar(50),
	NOTES nvarchar(4000),
	T_CREATOR_ID int,
	CREATOR_NAME nvarchar(255),
	ADMINISTRATOR_ID int,
	ADMINISTRATOR_NAME nvarchar(255),
	DATE_CREATED date	
)

	INSERT INTO @License (SITE_LIC_ID, SITE_LIC_NAME, COMPANY_NAME,
	COMPANY_ADDRESS, TELEPHONE, NOTES, T_CREATOR_ID, DATE_CREATED, ADMINISTRATOR_ID)
	select sl.SITE_LIC_ID, sl.SITE_LIC_NAME, sl.COMPANY_NAME, sl.COMPANY_ADDRESS,
	sl.TELEPHONE, sl.NOTES, sl.CREATOR_ID, sl.DATE_CREATED, sla.CONTACT_ID
	from Reviewer.dbo.TB_SITE_LIC sl
	inner join TB_SITE_LIC_ADMIN sla on sla.SITE_LIC_ID = sl.SITE_LIC_ID
	where sla.CONTACT_ID = @admin_ID

	update @License
	set CREATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = T_CREATOR_ID)
	
	update @License
	set ADMINISTRATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = ADMINISTRATOR_ID)

	select * from @License
	

END



GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_1]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Get_1] 
	-- Add the parameters for the stored procedure here
	
	@admin_ID int,
	@site_license_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @License TABLE
(
    SITE_LIC_ID int,
    SITE_LIC_NAME nvarchar(50),
	COMPANY_NAME nvarchar(50),
	COMPANY_ADDRESS nvarchar(500),
	TELEPHONE nvarchar(50),
	NOTES nvarchar(4000),
	T_CREATOR_ID int,
	CREATOR_NAME nvarchar(255),
	ADMINISTRATOR_ID int,
	ADMINISTRATOR_NAME nvarchar(255),
	DATE_CREATED date	
)

	INSERT INTO @License (SITE_LIC_ID, SITE_LIC_NAME, COMPANY_NAME,
	COMPANY_ADDRESS, TELEPHONE, NOTES, T_CREATOR_ID, DATE_CREATED, ADMINISTRATOR_ID)
	select sl.SITE_LIC_ID, sl.SITE_LIC_NAME, sl.COMPANY_NAME, sl.COMPANY_ADDRESS,
	sl.TELEPHONE, sl.NOTES, sl.CREATOR_ID, sl.DATE_CREATED, sla.CONTACT_ID
	from Reviewer.dbo.TB_SITE_LIC sl
	inner join TB_SITE_LIC_ADMIN sla on sla.SITE_LIC_ID = sl.SITE_LIC_ID
	where sla.CONTACT_ID = @admin_ID
	and sla.SITE_LIC_ID = @site_license_id

	update @License
	set CREATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = T_CREATOR_ID)
	
	update @License
	set ADMINISTRATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = ADMINISTRATOR_ID)

	select * from @License
	

END




GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Accounts]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:           <Author,,Name>
-- Create date: <Create Date,,>
-- Description:      <Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Get_Accounts] 
(
       @lic_id int
)
AS
BEGIN
       -- SET NOCOUNT ON added to prevent extra result sets from
       -- interfering with SELECT statements.
       SET NOCOUNT ON;

    -- Insert statements for procedure here
    declare @det_id int
    set @det_id = (select top 1 ld.SITE_LIC_DETAILS_ID from TB_SITE_LIC_DETAILS ld 
        inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and ld.VALID_FROM is not null
        and fs.IS_ACTIVE = 0 and ld.SITE_LIC_ID = @lic_id
        order by ld.VALID_FROM desc)
       --first, reviews that were added in the past (not associated with currently active SITE_LIC_DETAILS)
       select * from Reviewer.dbo.TB_REVIEW r
              inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID and SITE_LIC_ID = @lic_id and SITE_LIC_DETAILS_ID != @det_id
       --second, reviews that were added to the currently active SITE_LIC_DETAILS
       select * from Reviewer.dbo.TB_REVIEW r
              inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID and SITE_LIC_ID = @lic_id and SITE_LIC_DETAILS_ID = @det_id
       --accounts in the license
       select * from Reviewer.dbo.TB_CONTACT c
              inner join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID and lc.SITE_LIC_ID = @lic_id
       --license admins
       select * from Reviewer.dbo.TB_CONTACT c
              inner join TB_SITE_LIC_ADMIN la on c.CONTACT_ID = la.CONTACT_ID and la.SITE_LIC_ID = @lic_id
END

GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Accounts_ADM]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:           <Author,,Name>
-- Create date: <Create Date,,>
-- Description:      <Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Get_Accounts_ADM] 
(
       @lic_id int
)
AS
BEGIN
       -- SET NOCOUNT ON added to prevent extra result sets from
       -- interfering with SELECT statements.
       SET NOCOUNT ON;

	   declare @contactList table
		(
			tv_contact_id int, 
			tv_contact_name nvarchar(255),
			tv_email nvarchar(500),
			tv_last_access datetime,
			tv_review_id int
		)

    -- Insert statements for procedure here
    declare @det_id int
    set @det_id = (select top 1 ld.SITE_LIC_DETAILS_ID from TB_SITE_LIC_DETAILS ld 
        inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and ld.VALID_FROM is not null
        and fs.IS_ACTIVE = 0 and ld.SITE_LIC_ID = @lic_id
        order by ld.VALID_FROM desc)
       --first, reviews that were added in the past (not associated with currently active SITE_LIC_DETAILS)
       select * from Reviewer.dbo.TB_REVIEW r
              inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID and SITE_LIC_ID = @lic_id and SITE_LIC_DETAILS_ID != @det_id
       --second, reviews that were added to the currently active SITE_LIC_DETAILS
       select * from Reviewer.dbo.TB_REVIEW r
              inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on r.REVIEW_ID = lr.REVIEW_ID and SITE_LIC_ID = @lic_id and SITE_LIC_DETAILS_ID = @det_id
       --accounts in the license
	   insert into @contactList (tv_contact_id, tv_contact_name, tv_email, tv_last_access)
	   select c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, max(lt.CREATED) as LAST_LOGIN
		from Reviewer.dbo.TB_CONTACT c
		left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
		on c.CONTACT_ID = lt.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
		where c.CONTACT_ID in (select CONTACT_ID from Reviewer.dbo.TB_SITE_LIC_CONTACT where SITE_LIC_ID = @lic_id)	
		group by c.CONTACT_ID, c.CONTACT_NAME, c.EMAIL, c.DATE_CREATED
		UPDATE @contactList 
		SET tv_review_id = TB_LOGON_TICKET.REVIEW_ID 
		FROM TB_LOGON_TICKET, @contactList 
		WHERE CONTACT_ID = tv_contact_id
		and CREATED = tv_last_access
		select * from @contactList order by tv_contact_name
       -- select * from Reviewer.dbo.TB_CONTACT c
       --    inner join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID and lc.SITE_LIC_ID = @lic_id
       --license admins
       select * from Reviewer.dbo.TB_CONTACT c
              inner join TB_SITE_LIC_ADMIN la on c.CONTACT_ID = la.CONTACT_ID and la.SITE_LIC_ID = @lic_id
END

GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_All]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Get_All] 
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
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_all_Packages]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Get_all_Packages] 
(	
	@site_lic_ID int,
	@site_lic_adm_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if @site_lic_ID = '0'
	begin
		-- we are using the adm parameter
		SELECT s_l_c.*, f_s.* from TB_SITE_LIC_DETAILS s_l_c
		inner join TB_FOR_SALE f_s on f_s.FOR_SALE_ID = s_l_c.FOR_SALE_ID
		inner join TB_SITE_LIC_ADMIN s_l_a on s_l_a.SITE_LIC_ID = s_l_c.SITE_LIC_ID
		where s_l_a.CONTACT_ID = @site_lic_adm_ID
		Order by DATE_CREATED desc
	end
	else
	begin
		-- we are using the site_lic parameter
		SELECT s_l_c.*, f_s.* from TB_SITE_LIC_DETAILS s_l_c
		inner join TB_FOR_SALE f_s on f_s.FOR_SALE_ID = s_l_c.FOR_SALE_ID
		where SITE_LIC_ID = @site_lic_ID
		Order by DATE_CREATED desc
	end
	
END



GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_By_Admin]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Get_By_Admin] 
(	
	@site_lic_adm_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select distinct s_l_c.SITE_LIC_ID, s_l.SITE_LIC_NAME from TB_SITE_LIC_ADMIN s_l_c
	inner join Reviewer.dbo.TB_SITE_LIC s_l on s_l.SITE_LIC_ID = s_l_c.SITE_LIC_ID
	where s_l_c.CONTACT_ID = @site_lic_adm_ID

	
END




GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_By_ID]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Get_By_ID] 
	-- Add the parameters for the stored procedure here
	
	@admin_ID int,
	@site_license_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @License TABLE
(
    SITE_LIC_ID int,
    SITE_LIC_NAME nvarchar(50),
	COMPANY_NAME nvarchar(50),
	COMPANY_ADDRESS nvarchar(500),
	TELEPHONE nvarchar(50),
	NOTES nvarchar(4000),
	EPPI_NOTES nvarchar(4000),
	T_CREATOR_ID int,
	CREATOR_NAME nvarchar(255),
	ADMINISTRATOR_ID int,
	ADMINISTRATOR_NAME nvarchar(255),
	DATE_CREATED date,
	SITE_LIC_MODEL int,
	ALLOW_REVIEW_OWNERSHIP_CHANGE bit
)

	INSERT INTO @License (SITE_LIC_ID, SITE_LIC_NAME, COMPANY_NAME,
	COMPANY_ADDRESS, TELEPHONE, NOTES, EPPI_NOTES, T_CREATOR_ID, DATE_CREATED, ADMINISTRATOR_ID, SITE_LIC_MODEL, ALLOW_REVIEW_OWNERSHIP_CHANGE)
	select sl.SITE_LIC_ID, sl.SITE_LIC_NAME, sl.COMPANY_NAME, sl.COMPANY_ADDRESS,
	sl.TELEPHONE, sl.NOTES, sl.EPPI_NOTES, sl.CREATOR_ID, sl.DATE_CREATED, sla.CONTACT_ID, sl.SITE_LIC_MODEL, sl.ALLOW_REVIEW_OWNERSHIP_CHANGE
	from Reviewer.dbo.TB_SITE_LIC sl
	inner join TB_SITE_LIC_ADMIN sla on sla.SITE_LIC_ID = sl.SITE_LIC_ID
	where sla.CONTACT_ID = @admin_ID
	and sla.SITE_LIC_ID = @site_license_id

	update @License
	set CREATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = T_CREATOR_ID)
	
	update @License
	set ADMINISTRATOR_NAME = 
	(select CONTACT_NAME from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = ADMINISTRATOR_ID)

	select * from @License
	

END





GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Details]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Get_Details] 
(
	@admin_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- first results set: currently active license TB_SITE_LIC_DETAILS.VALID_FROM is something and FOR_SALE.IS_ACTIVE = false
	SELECT TOP 1 *, c.CONTACT_NAME as ADMIN_NAME, ld.DATE_CREATED as DATE_PACKAGE_CREATED from Reviewer.dbo.TB_SITE_LIC sl 
		inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
		inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
		inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and CONTACT_ID = @admin_ID
		inner join Reviewer.dbo.tb_CONTACT c on c.CONTACT_ID = @admin_ID
	Order by ld.VALID_FROM desc	
	-- second results set: currently active renewal offer TB_SITE_LIC_DETAILS.VALID_FROM is NULL and FOR_SALE.IS_ACTIVE = True
	SELECT TOP 1 *, c.CONTACT_NAME as ADMIN_NAME, ld.DATE_CREATED as DATE_PACKAGE_CREATED from Reviewer.dbo.TB_SITE_LIC sl 
		inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is null
		inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 1
		inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and CONTACT_ID = @admin_ID
		inner join Reviewer.dbo.tb_CONTACT c on c.CONTACT_ID = @admin_ID
	Order by ld.VALID_FROM desc
END


GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Details_1]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Get_Details_1] 
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
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Details_By_ID]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Get_Details_By_ID] 
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
	SELECT TOP 1 *, c.CONTACT_NAME as ADMIN_NAME, ld.DATE_CREATED as DATE_PACKAGE_CREATED, SITE_LIC_MODEL from Reviewer.dbo.TB_SITE_LIC sl 
		inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
		inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
		inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and CONTACT_ID = @admin_ID
		inner join Reviewer.dbo.tb_CONTACT c on c.CONTACT_ID = @admin_ID
		where sl.SITE_LIC_ID = @site_license_id
	Order by ld.VALID_FROM desc	
	-- second results set: currently active renewal offer TB_SITE_LIC_DETAILS.VALID_FROM is NULL and FOR_SALE.IS_ACTIVE = True
	SELECT TOP 1 *, c.CONTACT_NAME as ADMIN_NAME, ld.DATE_CREATED as DATE_PACKAGE_CREATED, SITE_LIC_MODEL from Reviewer.dbo.TB_SITE_LIC sl 
		inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is null
		inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 1
		inner join TB_SITE_LIC_ADMIN la on sl.SITE_LIC_ID = la.SITE_LIC_ID and CONTACT_ID = @admin_ID
		inner join Reviewer.dbo.tb_CONTACT c on c.CONTACT_ID = @admin_ID
		where sl.SITE_LIC_ID = @site_license_id
	Order by ld.VALID_FROM desc
END





GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Package]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Get_Package] 
(	
	@SITE_LIC_DETAILS_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select * from TB_SITE_LIC_DETAILS ld 
	inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID
	inner join Reviewer.dbo.TB_SITE_LIC sl on sl.SITE_LIC_ID = ld.SITE_LIC_ID
	where SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
	
END



GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Get_Package_Log]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Get_Package_Log] 
(	
	@SITE_LIC_DETAILS_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    select * from TB_SITE_LIC_LOG
    where SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
    order by [DATE]
	
END




GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_PullForwardForTesting]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO










CREATE OR ALTER     procedure [dbo].[st_Site_Lic_PullForwardForTesting]
(
	@SITE_LIC_ID int
)

As

SET NOCOUNT ON

	declare @tb_package_table table (tv_site_lic_details_id int, tv_date_created datetime, 
		tv_months int, tv_valid_from datetime)

	insert into @tb_package_table (tv_site_lic_details_id, tv_date_created, tv_months, tv_valid_from)
	select SITE_LIC_DETAILS_ID, DATE_CREATED, MONTHS, VALID_FROM from TB_SITE_LIC_DETAILS
	where SITE_LIC_ID = @SITE_LIC_ID
	and VALID_FROM is not null

	--select * from @tb_package_table

	declare @months int
	declare @validFrom datetime

	declare @WORKING_SITE_LIC_DETAILS_ID int
	declare SITE_LIC_DETAILS_ID_CURSOR cursor for
	select tv_site_lic_details_id FROM @tb_package_table
	open SITE_LIC_DETAILS_ID_CURSOR
	fetch next from SITE_LIC_DETAILS_ID_CURSOR
	into @WORKING_SITE_LIC_DETAILS_ID
	while @@FETCH_STATUS = 0
	begin		
		set @months = (select tv_months from @tb_package_table
		where tv_site_lic_details_id = @WORKING_SITE_LIC_DETAILS_ID)		

		update @tb_package_table
		set tv_date_created = DATEADD(MONTH, @months, tv_date_created),
		tv_valid_from = DATEADD(MONTH, @months, tv_valid_from)
		where tv_site_lic_details_id = @WORKING_SITE_LIC_DETAILS_ID

		set @validFrom = (select tv_valid_from from @tb_package_table
		where tv_site_lic_details_id = @WORKING_SITE_LIC_DETAILS_ID)

		FETCH NEXT FROM SITE_LIC_DETAILS_ID_CURSOR 
		INTO @WORKING_SITE_LIC_DETAILS_ID
	END

	CLOSE SITE_LIC_DETAILS_ID_CURSOR
	DEALLOCATE SITE_LIC_DETAILS_ID_CURSOR

	--select * from @tb_package_table

	--select * from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @SITE_LIC_ID 

	update TB_SITE_LIC_DETAILS
	set DATE_CREATED = tv_date_created,
	VALID_FROM = tv_valid_from
	from @tb_package_table
	where SITE_LIC_DETAILS_ID = tv_site_lic_details_id
	and SITE_LIC_ID = @SITE_LIC_ID -- for extra safety

	
	-- the last @months and @validFrom will be the latest package 
	-- so use this to adjust the license expiry date
	declare @newExpiryDate datetime
	set @newExpiryDate = DATEADD(MONTH, @months, @validFrom)

	update Reviewer.dbo.TB_SITE_LIC
	set EXPIRY_DATE = @newExpiryDate
	where SITE_LIC_ID = @SITE_LIC_ID

SET NOCOUNT OFF





GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_PushBackForTesting]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO









CREATE OR ALTER     procedure [dbo].[st_Site_Lic_PushBackForTesting]
(
	@SITE_LIC_ID int
)

As

SET NOCOUNT ON

	declare @tb_package_table table (tv_site_lic_details_id int, tv_date_created datetime, 
		tv_months int, tv_valid_from datetime)

	insert into @tb_package_table (tv_site_lic_details_id, tv_date_created, tv_months, tv_valid_from)
	select SITE_LIC_DETAILS_ID, DATE_CREATED, MONTHS, VALID_FROM from TB_SITE_LIC_DETAILS
	where SITE_LIC_ID = @SITE_LIC_ID
	and VALID_FROM is not null

	--select * from @tb_package_table

	declare @months int
	declare @validFrom datetime

	declare @WORKING_SITE_LIC_DETAILS_ID int
	declare SITE_LIC_DETAILS_ID_CURSOR cursor for
	select tv_site_lic_details_id FROM @tb_package_table
	open SITE_LIC_DETAILS_ID_CURSOR
	fetch next from SITE_LIC_DETAILS_ID_CURSOR
	into @WORKING_SITE_LIC_DETAILS_ID
	while @@FETCH_STATUS = 0
	begin		
		set @months = (select tv_months from @tb_package_table
		where tv_site_lic_details_id = @WORKING_SITE_LIC_DETAILS_ID)		
		
		update @tb_package_table
		set tv_date_created = DATEADD(MONTH, -@months, tv_date_created),
		tv_valid_from = DATEADD(MONTH, -@months, tv_valid_from)
		where tv_site_lic_details_id = @WORKING_SITE_LIC_DETAILS_ID

		set @validFrom = (select tv_valid_from from @tb_package_table
		where tv_site_lic_details_id = @WORKING_SITE_LIC_DETAILS_ID)

		FETCH NEXT FROM SITE_LIC_DETAILS_ID_CURSOR 
		INTO @WORKING_SITE_LIC_DETAILS_ID
	END

	CLOSE SITE_LIC_DETAILS_ID_CURSOR
	DEALLOCATE SITE_LIC_DETAILS_ID_CURSOR

	--select * from @tb_package_table

	--select * from TB_SITE_LIC_DETAILS where SITE_LIC_ID = @SITE_LIC_ID 

	update TB_SITE_LIC_DETAILS
	set DATE_CREATED = tv_date_created,
	VALID_FROM = tv_valid_from
	from @tb_package_table
	where SITE_LIC_DETAILS_ID = tv_site_lic_details_id
	and SITE_LIC_ID = @SITE_LIC_ID -- for extra safety

	-- the last @months and @validFrom will be the latest package 
	-- so use this to adjust the license expiry date
	declare @newExpiryDate datetime
	set @newExpiryDate = DATEADD(MONTH, @months, @validFrom)

	update Reviewer.dbo.TB_SITE_LIC
	set EXPIRY_DATE = @newExpiryDate
	where SITE_LIC_ID = @SITE_LIC_ID

SET NOCOUNT OFF





GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Remove_Admin]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_Site_Lic_Remove_Admin]
(	
	@SITE_LIC_ADMIN_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here

	delete from TB_SITE_LIC_ADMIN 
	where SITE_LIC_ADMIN_ID = @SITE_LIC_ADMIN_ID

	
END




GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_Remove_Review]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Site_Lic_Remove_Review]
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
	set @res = 0

    -- Insert statements for procedure here
    declare @ck int, @ck2 int, @lic_det_id int
	
	set @ck = (SELECT count(contact_id) from TB_SITE_LIC_ADMIN 
	where (CONTACT_ID = @admin_ID or @admin_ID in 
	(select CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @admin_ID and IS_SITE_ADMIN = 1)
		   ) and SITE_LIC_ID = @lic_id)
	if @ck < 1 set @res = -1 --first check: see if supplied admin is actually an admin!
	
	--second check, see if review_id exists
	else if  
	(select count(REVIEW_ID) from Reviewer.dbo.TB_REVIEW 
	 where @review_id = REVIEW_ID) != 1
		set @res = -2
	
	else -- initial checks went OK, let's see if we can do it
	begin
		set @ck = 
		(select count(review_id) from Reviewer.dbo.TB_SITE_LIC_REVIEW 
		where REVIEW_ID = @review_id and SITE_LIC_ID = @lic_id)
		
		set @lic_det_id = 
		(SELECT TOP 1 ld.SITE_LIC_DETAILS_ID from Reviewer.dbo.TB_SITE_LIC sl 
			inner join TB_SITE_LIC_DETAILS ld on sl.SITE_LIC_ID = ld.SITE_LIC_ID and ld.VALID_FROM is not null
			inner join TB_FOR_SALE fs on ld.FOR_SALE_ID = fs.FOR_SALE_ID and fs.IS_ACTIVE = 0
			Order by ld.VALID_FROM desc)
		
		if @ck = 0 -- review is NOT in the site lic
		begin
			set @res = -3 --review not in this site_lic
		end
		else
		begin --all is well, let's do something!
			begin transaction --make sure we don't commit only half of the mission critical data! (we assume the update below will work, only checking for the other statement)
			
			delete from Reviewer.dbo.TB_SITE_LIC_REVIEW
			where REVIEW_ID = @review_id
			
			if @@ROWCOUNT = 1
			begin --write success log
				commit transaction --all is well, commit
				insert into TB_SITE_LIC_LOG (
					[SITE_LIC_DETAILS_ID]
					  ,[CONTACT_ID]
					  ,[AFFECTED_ID]
					  ,[CHANGE_TYPE]
				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @review_id, 'remove review'
					from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
					order by DATE_CREATED desc
			end
			else --write failure log, if this is fired, there is a bug somewhere
			begin
				rollback transaction --BAD! something went wrong!
				insert into TB_SITE_LIC_LOG (
					[SITE_LIC_DETAILS_ID]
					  ,[CONTACT_ID]
					  ,[AFFECTED_ID]
					  ,[CHANGE_TYPE]
				) select top 1 SITE_LIC_DETAILS_ID, @admin_ID, @review_id, 'remove review: failed!'
					from TB_SITE_LIC_DETAILS where VALID_FROM IS NOT NULL and SITE_LIC_ID = @lic_id
					order by DATE_CREATED desc	
				set @res = -4
			end
		end
	end
	--select r.review_id, review_name from reviewer.dbo.TB_SITE_LIC_REVIEW lr
	--	inner join Reviewer.dbo.TB_REVIEW r on lr.REVIEW_ID = r.REVIEW_ID
	-- where SITE_LIC_ID = @lic_id
	return @res
	-- error codes: -1 = supplied admin_id is not an admin of this site lice
	--				-2 = review_id does not exist
	--				-3 = review not in this site_lic
	--				-4 = all seemed well but couldn't write changes! BUG ALERT
END



GO
/****** Object:  StoredProcedure [dbo].[st_Site_Lic_RemoveOffer]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







CREATE OR ALTER   procedure [dbo].[st_Site_Lic_RemoveOffer]
(
	@SITE_LIC_DETAILS_ID int,
	@SITE_LIC_ID int
)

As

SET NOCOUNT ON

	delete from TB_SITE_LIC_DETAILS
	where SITE_LIC_DETAILS_ID = @SITE_LIC_DETAILS_ID
	and SITE_LIC_ID = @SITE_LIC_ID


SET NOCOUNT OFF





GO
/****** Object:  StoredProcedure [dbo].[st_TransferAccountCredit]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_TransferAccountCredit] 
	-- Add the parameters for the stored procedure here
	@CONTACT_ID int,
	@PURCHASER_ID int,
	@EMAIL nvarchar(500),
	@RESULT nvarchar(100) out,
	@CREDIT smallint out,
	@NEWDATE date out,
	@CONTACT_NAME nvarchar(255) out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @DEST_CID int = 0
	select @DEST_CID = contact_id from Reviewer.dbo.TB_CONTACT c where @CONTACT_ID != c.CONTACT_ID and @EMAIL = c.EMAIL
	
	IF (@DEST_CID is null OR @DEST_CID < 1)--account was not found
	BEGIN
		select @RESULT = 'The destination account was not found'
		return
	END
	
	select @CREDIT = c.MONTHS_CREDIT from Reviewer.dbo.TB_CONTACT c where @CONTACT_ID = c.CONTACT_ID and (@EMAIL != c.EMAIL OR c.EMAIL is null)
	if (@CREDIT is null OR @CREDIT < 1)--there is no credit to transfer
	BEGIN
		select @RESULT = 'There is no credit to transfer'
		return
	END
	--checks worked, so we know what to do: add to
	if (@CREDIT > 1) select @CREDIT = @CREDIT - 1 --ghost accounts get the purchased amount + 1 month free trial, we need to remove this
	declare @oldDate DATE
	select @oldDate = c.[EXPIRY_DATE] from  Reviewer.dbo.TB_CONTACT c where c.CONTACT_ID = @DEST_CID
	UPDATE c set c.EXPIRY_DATE = CASE 
									when (c.[EXPIRY_DATE] is null) then null
									when (c.[EXPIRY_DATE] > getdate()) then DATEADD(month, @CREDIT, c.[EXPIRY_DATE])
									ELSE DATEADD(month, @CREDIT, getdate())
								  END
			     ,c.MONTHS_CREDIT = CASE 
									WHEN c.EXPIRY_DATE is null AND c.MONTHS_CREDIT is not null
										THEN c.MONTHS_CREDIT + @CREDIT
									WHEN c.EXPIRY_DATE is null AND c.MONTHS_CREDIT is null
										THEN @CREDIT + 1 --this is a new account that was never activated
									ELSE 0
								  END
		from Reviewer.dbo.TB_CONTACT c where c.CONTACT_ID = @DEST_CID
	declare @chk int
	select @chk = COUNT(CONTACT_ID) from Reviewer.dbo.TB_CONTACT where @DEST_CID = CONTACT_ID 
										and (
											 ([EXPIRY_DATE] is not null AND [EXPIRY_DATE] > GETDATE())
											  OR 
											  (MONTHS_CREDIT is not null AND MONTHS_CREDIT >= @CREDIT)
											)
	if @chk = 1
		BEGIN --all good! remove previous account, update bill info (to show the dest account in the old bill)
		-- update the extensions log and return
		declare @BillLine int
		select @BillLine = MAX(bl.LINE_ID) from TB_BILL_LINE bl
			inner join TB_BILL b on bl.BILL_ID = b.BILL_ID and b.BILL_STATUS = 'OK: Paid and data committed'
					AND AFFECTED_ID = @CONTACT_ID
		update TB_BILL_LINE set AFFECTED_ID = @DEST_CID where LINE_ID = @BillLine
		
		DELETE from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID --ghost is now empty, why keep it?
		
		--mark the extensios as a "Purchase"
		 insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
			OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
		 select GETDATE(), 1, @DEST_CID, [EXPIRY_DATE],
			@oldDate, @PURCHASER_ID, 2, 'Transfer ghost credit, from ID =' + CONVERT(varchar(10), @CONTACT_ID) + ' (deleted).'
		 from Reviewer.dbo.TB_CONTACT where @DEST_CID = CONTACT_ID 
		 
		 --get the affected account details
		 Select @RESULT = 'Success', @NEWDATE = c.EXPIRY_DATE , @CONTACT_NAME = c.CONTACT_NAME
			from Reviewer.dbo.TB_CONTACT c where @DEST_CID = CONTACT_ID 
		 return
	END	
	select @RESULT = 'Unspecified error: checks where successful but credit was not transferred'
END

GO
/****** Object:  StoredProcedure [dbo].[st_VatGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_VatGet] 
(
	@COUNTRY_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    declare @VAT_REQUIRED bit
    
	select @VAT_REQUIRED = VAT_REQUIRED from TB_COUNTRIES
	where COUNTRY_ID = @COUNTRY_ID
	
	if @VAT_REQUIRED = 1
	BEGIN
		select top 1 VAT_RATE from TB_TERMS_AND_CONDITIONS
		ORDER by CONDITIONS_ID desc
	END
	
	RETURN

END

GO
/****** Object:  StoredProcedure [dbo].[st_WDAttrCountChildren]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER procedure [dbo].[st_WDAttrCountChildren]
(
	@ATTRIBUTE_ID int,
	@LEVEL int
)

As

SET NOCOUNT ON

	if (@LEVEL = 0)
	begin
		select COUNT(a_s.ATTRIBUTE_ID) as NUM_CHILDREN 
		from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
		where a_s.PARENT_ATTRIBUTE_ID = 0
		and a_s.SET_ID = @ATTRIBUTE_ID
	end
	else
	begin
		select COUNT(a_s.ATTRIBUTE_ID) as NUM_CHILDREN 
		from Reviewer.dbo.TB_ATTRIBUTE_SET a_s
		where a_s.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID
	end

SET NOCOUNT OFF





GO
/****** Object:  StoredProcedure [dbo].[st_WDAttrCountChildrenFromWD]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE OR ALTER procedure [dbo].[st_WDAttrCountChildrenFromWD]
(
	@ATTRIBUTE_ID int,
	@LEVEL int
)

As

SET NOCOUNT ON

	select COUNT(w_d_a.ATTRIBUTE_ID) as NUM_CHILDREN 
	from Presenter.dbo.TB_WEB_DATABASE_ATTR w_d_a
	where w_d_a.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID

SET NOCOUNT OFF






GO
/****** Object:  StoredProcedure [dbo].[st_WDAttrDataGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_WDAttrDataGet]
(
	@LEVEL int,
	@ATTRIBUTE_ID int,
	@SET_ID int
)

As

SET NOCOUNT ON

	if @LEVEL = '0' -- we have clicked on a set and are looking for level 1
	begin
		select a_s.SET_ID, a.ATTRIBUTE_ID, a.CONTACT_ID, a.ATTRIBUTE_NAME, 
		a_s.PARENT_ATTRIBUTE_ID, a_s.ATTRIBUTE_ORDER 
		from Reviewer.dbo.TB_ATTRIBUTE a inner join Reviewer.dbo.TB_ATTRIBUTE_SET a_s
		on a.ATTRIBUTE_ID = a_s.ATTRIBUTE_ID
		and a_s.SET_ID = @ATTRIBUTE_ID
		and a_s.PARENT_ATTRIBUTE_ID = 0
		order by a_s.ATTRIBUTE_ORDER
	end
	
	if @LEVEL = '1' --  we have click on a level 1 and
	begin
		select a_s.SET_ID, a.ATTRIBUTE_ID, a.CONTACT_ID, a.ATTRIBUTE_NAME, 
		a_s.PARENT_ATTRIBUTE_ID, a_s.ATTRIBUTE_ORDER 
		from Reviewer.dbo.TB_ATTRIBUTE a inner join Reviewer.dbo.TB_ATTRIBUTE_SET a_s
		on a.ATTRIBUTE_ID = a_s.ATTRIBUTE_ID
		and a_s.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID
		and a_s.SET_ID = @SET_ID -- added
		order by a_s.ATTRIBUTE_ORDER
	end

SET NOCOUNT OFF




GO
/****** Object:  StoredProcedure [dbo].[st_WDAttrDataGet_1]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER procedure [dbo].[st_WDAttrDataGet_1]
(
	@LEVEL int,
	@ATTRIBUTE_ID int,
	@SET_ID int
)

As

SET NOCOUNT ON

	if @LEVEL = '0' -- we have clicked on a set and are looking for level 1
	begin
		select a_s.SET_ID, a_s.ATTRIBUTE_SET_ID, a.ATTRIBUTE_ID, a.CONTACT_ID, a.ATTRIBUTE_NAME, 
		a_s.PARENT_ATTRIBUTE_ID, a_s.ATTRIBUTE_ORDER 
		from Reviewer.dbo.TB_ATTRIBUTE a inner join Reviewer.dbo.TB_ATTRIBUTE_SET a_s
		on a.ATTRIBUTE_ID = a_s.ATTRIBUTE_ID
		--and a_s.SET_ID = @ATTRIBUTE_ID
		and a_s.SET_ID = @SET_ID
		and a_s.PARENT_ATTRIBUTE_ID = 0
		order by a_s.ATTRIBUTE_ORDER
	end
	
	if @LEVEL = '1' --  we have click on a level 1 and
	begin
		select a_s.SET_ID, a_s.ATTRIBUTE_SET_ID, a.ATTRIBUTE_ID, a.CONTACT_ID, a.ATTRIBUTE_NAME, 
		a_s.PARENT_ATTRIBUTE_ID, a_s.ATTRIBUTE_ORDER 
		from Reviewer.dbo.TB_ATTRIBUTE a inner join Reviewer.dbo.TB_ATTRIBUTE_SET a_s
		on a.ATTRIBUTE_ID = a_s.ATTRIBUTE_ID
		and a_s.PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID
		and a_s.SET_ID = @SET_ID -- added
		order by a_s.ATTRIBUTE_ORDER
	end

SET NOCOUNT OFF





GO
/****** Object:  StoredProcedure [dbo].[st_WDAttrDataGetFromWD]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER procedure [dbo].[st_WDAttrDataGetFromWD]
(
	--@LEVEL int,
	--@ATTRIBUTE_ID int,
	--@WEBDB_ID int
	
	@ATTRIBUTE_ID int,
	@ATTRIBUTE_SET_ID nvarchar(50),
	@WEBDB_ID int
)

As

SET NOCOUNT ON

	if @ATTRIBUTE_SET_ID = ''
	begin
		select ATTRIBUTE_ID, ATTRIBUTE_SET_ID, ATTRIBUTE_NAME, PARENT_ATTRIBUTE_ID, SET_ID 
		from Presenter.dbo.TB_WEB_DATABASE_ATTR
		where WEBDB_ID = @WEBDB_ID
		and PARENT_ATTRIBUTE_ID = 0
		and SET_ID = @ATTRIBUTE_ID
		and ATTRIBUTE_SET_ID != ''
		order by ATTRIBUTE_ORDER
	end
	else
	begin
		select ATTRIBUTE_ID, ATTRIBUTE_SET_ID, ATTRIBUTE_NAME, PARENT_ATTRIBUTE_ID, SET_ID 
		from Presenter.dbo.TB_WEB_DATABASE_ATTR
		where WEBDB_ID = @WEBDB_ID
		and PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID
		and ATTRIBUTE_SET_ID != ''
		order by ATTRIBUTE_ORDER
	end
	/*
	if @LEVEL = '0' 
	begin
		select ATTRIBUTE_ID, ATTRIBUTE_SET_ID, ATTRIBUTE_NAME, PARENT_ATTRIBUTE_ID 
		from Presenter.dbo.TB_WEB_DATABASE_ATTR
		where [LEVEL] = 1
		and WEBDB_ID = @WEBDB_ID
		and PARENT_ATTRIBUTE_ID = 0
		and SET_ID = @ATTRIBUTE_ID
		order by ATTRIBUTE_ORDER
	end
	
	if @LEVEL = '1' 
	begin
		select ATTRIBUTE_ID, ATTRIBUTE_SET_ID, ATTRIBUTE_NAME, PARENT_ATTRIBUTE_ID 
		from Presenter.dbo.TB_WEB_DATABASE_ATTR
		where [LEVEL] = 1
		and WEBDB_ID = @WEBDB_ID
		and PARENT_ATTRIBUTE_ID = @ATTRIBUTE_ID
		order by ATTRIBUTE_ORDER
	end
	*/


SET NOCOUNT OFF





GO
/****** Object:  StoredProcedure [dbo].[st_WDCodesetsGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_WDCodesetsGet]
(
	@REVIEW_ID int
)

As

SET NOCOUNT ON

	select s.SET_ID, s.SET_NAME, st.SET_TYPE from Reviewer.dbo.TB_SET s 
	inner join Reviewer.dbo.TB_REVIEW_SET rs
	on s.SET_ID = rs.SET_ID
	inner join Reviewer.dbo.TB_SET_TYPE st
	on s.SET_TYPE_ID = st.SET_TYPE_ID
	and REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF



GO
/****** Object:  StoredProcedure [dbo].[st_WDCreateIfNotExist]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE OR ALTER procedure [dbo].[st_WDCreateIfNotExist]
(
	@REVIEW_ID int,
	@WEBDB_NAME nvarchar(100),
	@WEBDB_ID int output	
)

As

SET NOCOUNT ON

	declare @PlaceHolderReviewID int
	set @PlaceHolderReviewID = 1
	
	select * from Presenter.dbo.TB_WEB_DATABASE w_d
	where w_d.REVIEW_ID = @REVIEW_ID
	and w_d.WEBDB_NAME = @WEBDB_NAME
	
	if @@ROWCOUNT = 0
	begin
		insert into Presenter.dbo.TB_WEB_DATABASE (REVIEW_ID, WEBDB_NAME, [DESCRIPTION])
		values (@REVIEW_ID, @WEBDB_NAME, 'Enter introduction')
		
		select @WEBDB_ID = WEBDB_ID from Presenter.dbo.TB_WEB_DATABASE w_d
		where w_d.REVIEW_ID = @REVIEW_ID
		and w_d.WEBDB_NAME = @WEBDB_NAME
		
		insert into Presenter.dbo.TB_WEB_DATABASE_INTRODUCTION (WEBDB_ID)
		values (@WEBDB_ID)
		
		-- place blank images in the header_image
		update Presenter.dbo.TB_WEB_DATABASE
		set HEADER_IMAGE_1 = (select HEADER_IMAGE_1 from Presenter.dbo.TB_WEB_DATABASE
		where REVIEW_ID = @PlaceHolderReviewID)
		where WEBDB_ID = @WEBDB_ID

		update Presenter.dbo.TB_WEB_DATABASE
		set HEADER_IMAGE_2 = (select HEADER_IMAGE_2 from Presenter.dbo.TB_WEB_DATABASE
		where REVIEW_ID = @PlaceHolderReviewID)
		where WEBDB_ID = @WEBDB_ID
		
		update Presenter.dbo.TB_WEB_DATABASE
		set HEADER_IMAGE_3 = (select HEADER_IMAGE_3 from Presenter.dbo.TB_WEB_DATABASE
		where REVIEW_ID = @PlaceHolderReviewID)
		where WEBDB_ID = @WEBDB_ID
	end
	else
	begin
		set @WEBDB_ID = 0;
	end
	

SET NOCOUNT OFF






GO
/****** Object:  StoredProcedure [dbo].[st_WDDeleteChildrenFromCodeset]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_WDDeleteChildrenFromCodeset]
(
	@WEBDB_ID int,
	@START_ATTRIBUTE_SET_ID int,
	@set_id int
)

As

SET NOCOUNT ON


-- start at the chosen attribute_id
-- collect the children for that node and place those attribure_id(s) in @handover_table
-- place contents of @handover_table into @handover_table
--1 collect all of children for the attributes in @handover_table and place in @child_attributes
--2 move the attribute_id(s) in @handover_table to @final_table
--3 empty @handover_table
--4 move the attribute_id(s) in @child_attributes to @handover_table
--5 empty @child_attributes
--6 go back to 1 until there are no more children to collect
-- delete all of the rows in TB_WEB_DATABASE_ATTR where attribute_id are in @storage_table

--declare @start_attribute_id int set @start_attribute_id = 125286
--declare @set_id int set @set_id = 1712
declare @child_attributes table (attribute_id int, parent_attribute_id int)
declare @handover_table table (attribute_id int, parent_attribute_id int)
declare @final_table table (attribute_id int, parent_attribute_id int)

declare @START_ATTRIBUTE_ID int
set @START_ATTRIBUTE_ID = (select ATTRIBUTE_ID from Presenter.dbo.TB_WEB_DATABASE_ATTR
where ATTRIBUTE_SET_ID = @START_ATTRIBUTE_SET_ID)

-- put the starting attribute into @final_table
insert into @final_table (attribute_id) values (@START_ATTRIBUTE_ID)

-- collect the children of @start_attribute_id and place in @handover_table
insert into @handover_table (attribute_id, parent_attribute_id)
select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from Presenter.dbo.TB_WEB_DATABASE_ATTR 
where PARENT_ATTRIBUTE_ID = @START_ATTRIBUTE_ID
and SET_ID = @set_id 


-- move the attribute_id(s) in @handover_table to @final_table
insert into @final_table (attribute_id, parent_attribute_id)
select * from @handover_table 

declare @WORKING_ATTRIBUTE_ID int
-----------------------------------------------------------------------
declare ATTRIBUTE_ID_CURSOR cursor for
select attribute_id FROM @handover_table
open ATTRIBUTE_ID_CURSOR
fetch next from ATTRIBUTE_ID_CURSOR
into @WORKING_ATTRIBUTE_ID
while @@FETCH_STATUS = 0
begin
	-- collect the children of @handover_table and place in @child_attributes
	insert into @child_attributes (attribute_id, parent_attribute_id)
	select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from Presenter.dbo.TB_WEB_DATABASE_ATTR 
	where PARENT_ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	and SET_ID = @set_id and WEBDB_ID = @WEBDB_ID
	
	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
	INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR

-- empty @handover_table  
delete from @handover_table

-- move the attribute_id(s) in @child_attributes to @handover_table
insert into @handover_table (attribute_id, parent_attribute_id)
select * from @child_attributes

-- move the attribute_id(s) in @handover_table to @final_table
insert into @final_table (attribute_id, parent_attribute_id)
select * from @handover_table 

-- empty @child_attributes
delete from @child_attributes


------------------------------------------------------------------------------------------------
-- Now just keep doing the same thing over and over moving further down into the codeset. 
-- This is embarrassing code but it runs fast!
-- We will allow for 10 levels of hierarchy. 
-- Any codeset with more than that must be having a laugh!
------------------------------------------------------------------------------------------------
-- level 1
declare ATTRIBUTE_ID_CURSOR cursor for
select attribute_id FROM @handover_table
open ATTRIBUTE_ID_CURSOR
fetch next from ATTRIBUTE_ID_CURSOR
into @WORKING_ATTRIBUTE_ID
while @@FETCH_STATUS = 0
begin
	-- collect the children of @handover_table and place in @child_attributes
	insert into @child_attributes (attribute_id, parent_attribute_id)
	select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from Presenter.dbo.TB_WEB_DATABASE_ATTR 
	where PARENT_ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	and SET_ID = @set_id and WEBDB_ID = @WEBDB_ID
	
	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
	INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR

-- empty @handover_table  
delete from @handover_table

-- move the attribute_id(s) in @child_attributes to @handover_table
insert into @handover_table (attribute_id, parent_attribute_id)
select * from @child_attributes

-- move the attribute_id(s) in @handover_table to @final_table
insert into @final_table (attribute_id, parent_attribute_id)
select * from @handover_table 

-- empty @child_attributes
delete from @child_attributes
-----------------------------------------------------------------------------------
-----------------------------------------------------------------------------
-- level 2
declare ATTRIBUTE_ID_CURSOR cursor for
select attribute_id FROM @handover_table
open ATTRIBUTE_ID_CURSOR
fetch next from ATTRIBUTE_ID_CURSOR
into @WORKING_ATTRIBUTE_ID
while @@FETCH_STATUS = 0
begin
	-- collect the children of @handover_table and place in @child_attributes
	insert into @child_attributes (attribute_id, parent_attribute_id)
	select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from Presenter.dbo.TB_WEB_DATABASE_ATTR 
	where PARENT_ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	and SET_ID = @set_id and WEBDB_ID = @WEBDB_ID
	
	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
	INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR

-- empty @handover_table  
delete from @handover_table

-- move the attribute_id(s) in @child_attributes to @handover_table
insert into @handover_table (attribute_id, parent_attribute_id)
select * from @child_attributes

-- move the attribute_id(s) in @handover_table to @final_table
insert into @final_table (attribute_id, parent_attribute_id)
select * from @handover_table 

-- empty @child_attributes
delete from @child_attributes
-----------------------------------------------------------------------------
-----------------------------------------------------------------------------------
-- level 3
declare ATTRIBUTE_ID_CURSOR cursor for
select attribute_id FROM @handover_table
open ATTRIBUTE_ID_CURSOR
fetch next from ATTRIBUTE_ID_CURSOR
into @WORKING_ATTRIBUTE_ID
while @@FETCH_STATUS = 0
begin
	-- collect the children of @handover_table and place in @child_attributes
	insert into @child_attributes (attribute_id, parent_attribute_id)
	select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from Presenter.dbo.TB_WEB_DATABASE_ATTR 
	where PARENT_ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	and SET_ID = @set_id and WEBDB_ID = @WEBDB_ID
	
	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
	INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR

-- empty @handover_table  
delete from @handover_table

-- move the attribute_id(s) in @child_attributes to @handover_table
insert into @handover_table (attribute_id, parent_attribute_id)
select * from @child_attributes

-- move the attribute_id(s) in @handover_table to @final_table
insert into @final_table (attribute_id, parent_attribute_id)
select * from @handover_table 

-- empty @child_attributes
delete from @child_attributes
-----------------------------------------------------------------------------
-----------------------------------------------------------------------------
-- level 4
declare ATTRIBUTE_ID_CURSOR cursor for
select attribute_id FROM @handover_table
open ATTRIBUTE_ID_CURSOR
fetch next from ATTRIBUTE_ID_CURSOR
into @WORKING_ATTRIBUTE_ID
while @@FETCH_STATUS = 0
begin
	-- collect the children of @handover_table and place in @child_attributes
	insert into @child_attributes (attribute_id, parent_attribute_id)
	select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from Presenter.dbo.TB_WEB_DATABASE_ATTR 
	where PARENT_ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	and SET_ID = @set_id and WEBDB_ID = @WEBDB_ID
	
	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
	INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR

-- empty @handover_table  
delete from @handover_table

-- move the attribute_id(s) in @child_attributes to @handover_table
insert into @handover_table (attribute_id, parent_attribute_id)
select * from @child_attributes

-- move the attribute_id(s) in @handover_table to @final_table
insert into @final_table (attribute_id, parent_attribute_id)
select * from @handover_table 

-- empty @child_attributes
delete from @child_attributes
-----------------------------------------------------------------------------
-----------------------------------------------------------------------------
-- level 5
declare ATTRIBUTE_ID_CURSOR cursor for
select attribute_id FROM @handover_table
open ATTRIBUTE_ID_CURSOR
fetch next from ATTRIBUTE_ID_CURSOR
into @WORKING_ATTRIBUTE_ID
while @@FETCH_STATUS = 0
begin
	-- collect the children of @handover_table and place in @child_attributes
	insert into @child_attributes (attribute_id, parent_attribute_id)
	select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from Presenter.dbo.TB_WEB_DATABASE_ATTR 
	where PARENT_ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	and SET_ID = @set_id and WEBDB_ID = @WEBDB_ID
	
	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
	INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR

-- empty @handover_table  
delete from @handover_table

-- move the attribute_id(s) in @child_attributes to @handover_table
insert into @handover_table (attribute_id, parent_attribute_id)
select * from @child_attributes

-- move the attribute_id(s) in @handover_table to @final_table
insert into @final_table (attribute_id, parent_attribute_id)
select * from @handover_table 

-- empty @child_attributes
delete from @child_attributes
-----------------------------------------------------------------------------
-----------------------------------------------------------------------------
-- level 6
declare ATTRIBUTE_ID_CURSOR cursor for
select attribute_id FROM @handover_table
open ATTRIBUTE_ID_CURSOR
fetch next from ATTRIBUTE_ID_CURSOR
into @WORKING_ATTRIBUTE_ID
while @@FETCH_STATUS = 0
begin
	-- collect the children of @handover_table and place in @child_attributes
	insert into @child_attributes (attribute_id, parent_attribute_id)
	select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from Presenter.dbo.TB_WEB_DATABASE_ATTR 
	where PARENT_ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	and SET_ID = @set_id and WEBDB_ID = @WEBDB_ID
	
	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
	INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR

-- empty @handover_table  
delete from @handover_table

-- move the attribute_id(s) in @child_attributes to @handover_table
insert into @handover_table (attribute_id, parent_attribute_id)
select * from @child_attributes

-- move the attribute_id(s) in @handover_table to @final_table
insert into @final_table (attribute_id, parent_attribute_id)
select * from @handover_table 

-- empty @child_attributes
delete from @child_attributes
-----------------------------------------------------------------------------
-----------------------------------------------------------------------------
-- level 7
declare ATTRIBUTE_ID_CURSOR cursor for
select attribute_id FROM @handover_table
open ATTRIBUTE_ID_CURSOR
fetch next from ATTRIBUTE_ID_CURSOR
into @WORKING_ATTRIBUTE_ID
while @@FETCH_STATUS = 0
begin
	-- collect the children of @handover_table and place in @child_attributes
	insert into @child_attributes (attribute_id, parent_attribute_id)
	select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from Presenter.dbo.TB_WEB_DATABASE_ATTR 
	where PARENT_ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	and SET_ID = @set_id and WEBDB_ID = @WEBDB_ID
	
	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
	INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR

-- empty @handover_table  
delete from @handover_table

-- move the attribute_id(s) in @child_attributes to @handover_table
insert into @handover_table (attribute_id, parent_attribute_id)
select * from @child_attributes

-- move the attribute_id(s) in @handover_table to @final_table
insert into @final_table (attribute_id, parent_attribute_id)
select * from @handover_table 

-- empty @child_attributes
delete from @child_attributes
-----------------------------------------------------------------------------
-----------------------------------------------------------------------------
-- level 8
declare ATTRIBUTE_ID_CURSOR cursor for
select attribute_id FROM @handover_table
open ATTRIBUTE_ID_CURSOR
fetch next from ATTRIBUTE_ID_CURSOR
into @WORKING_ATTRIBUTE_ID
while @@FETCH_STATUS = 0
begin
	-- collect the children of @handover_table and place in @child_attributes
	insert into @child_attributes (attribute_id, parent_attribute_id)
	select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from Presenter.dbo.TB_WEB_DATABASE_ATTR 
	where PARENT_ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	and SET_ID = @set_id and WEBDB_ID = @WEBDB_ID
	
	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
	INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR

-- empty @handover_table  
delete from @handover_table

-- move the attribute_id(s) in @child_attributes to @handover_table
insert into @handover_table (attribute_id, parent_attribute_id)
select * from @child_attributes

-- move the attribute_id(s) in @handover_table to @final_table
insert into @final_table (attribute_id, parent_attribute_id)
select * from @handover_table 

-- empty @child_attributes
delete from @child_attributes
-----------------------------------------------------------------------------
-----------------------------------------------------------------------------
-- level 9
declare ATTRIBUTE_ID_CURSOR cursor for
select attribute_id FROM @handover_table
open ATTRIBUTE_ID_CURSOR
fetch next from ATTRIBUTE_ID_CURSOR
into @WORKING_ATTRIBUTE_ID
while @@FETCH_STATUS = 0
begin
	-- collect the children of @handover_table and place in @child_attributes
	insert into @child_attributes (attribute_id, parent_attribute_id)
	select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from Presenter.dbo.TB_WEB_DATABASE_ATTR 
	where PARENT_ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	and SET_ID = @set_id and WEBDB_ID = @WEBDB_ID
	
	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
	INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR

-- empty @handover_table  
delete from @handover_table

-- move the attribute_id(s) in @child_attributes to @handover_table
insert into @handover_table (attribute_id, parent_attribute_id)
select * from @child_attributes

-- move the attribute_id(s) in @handover_table to @final_table
insert into @final_table (attribute_id, parent_attribute_id)
select * from @handover_table 

-- empty @child_attributes
delete from @child_attributes
-----------------------------------------------------------------------------
-----------------------------------------------------------------------------
-- level 10
declare ATTRIBUTE_ID_CURSOR cursor for
select attribute_id FROM @handover_table
open ATTRIBUTE_ID_CURSOR
fetch next from ATTRIBUTE_ID_CURSOR
into @WORKING_ATTRIBUTE_ID
while @@FETCH_STATUS = 0
begin
	-- collect the children of @handover_table and place in @child_attributes
	insert into @child_attributes (attribute_id, parent_attribute_id)
	select ATTRIBUTE_ID, PARENT_ATTRIBUTE_ID from Presenter.dbo.TB_WEB_DATABASE_ATTR 
	where PARENT_ATTRIBUTE_ID = @WORKING_ATTRIBUTE_ID
	and SET_ID = @set_id and WEBDB_ID = @WEBDB_ID
	
	FETCH NEXT FROM ATTRIBUTE_ID_CURSOR 
	INTO @WORKING_ATTRIBUTE_ID
END

CLOSE ATTRIBUTE_ID_CURSOR
DEALLOCATE ATTRIBUTE_ID_CURSOR

-- empty @handover_table  
delete from @handover_table

-- move the attribute_id(s) in @child_attributes to @handover_table
insert into @handover_table (attribute_id, parent_attribute_id)
select * from @child_attributes

-- move the attribute_id(s) in @handover_table to @final_table
insert into @final_table (attribute_id, parent_attribute_id)
select * from @handover_table 

-- empty @child_attributes
delete from @child_attributes
-----------------------------------------------------------------------------
-----------------------------------------------------------------------------
select * from @final_table

delete from Presenter.dbo.TB_WEB_DATABASE_ATTR
where ATTRIBUTE_ID in
(select attribute_id from @final_table)
and SET_ID = @set_id and WEBDB_ID = @WEBDB_ID


SET NOCOUNT OFF




GO
/****** Object:  StoredProcedure [dbo].[st_WDDeleteFromWebDB]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_WDDeleteFromWebDB]
(
	@WEBDB_ID int,
	@ID int,
	@CASE nvarchar(50)
)

As

SET NOCOUNT ON

	if @CASE = 'whole set'
	begin
		delete from Presenter.dbo.TB_WEB_DATABASE_ATTR 
		where SET_ID = @ID
		and WEBDB_ID = @WEBDB_ID
	end
	else if @CASE = 'single attribute'
	begin
		delete from Presenter.dbo.TB_WEB_DATABASE_ATTR
		where ATTRIBUTE_ID =  @ID
	end
	else if @CASE = 'multiple attributes'
	begin
		select * from Presenter.dbo.TB_WEB_DATABASE_ATTR
	end
	else
	begin
		select * from Presenter.dbo.TB_WEB_DATABASE_ATTR
	end
	
	
SET NOCOUNT OFF




GO
/****** Object:  StoredProcedure [dbo].[st_WDDeleteHeaderImage]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER procedure [dbo].[st_WDDeleteHeaderImage]
(
	@IMAGE_NUMBER nvarchar(10),
	@WEBDB_ID int
)

As

SET NOCOUNT ON
	
	declare @PlaceHolderReviewID int
	set @PlaceHolderReviewID = 1
	
	-- place blank images in the header_image
	if @IMAGE_NUMBER = '1'
	begin
		update Presenter.dbo.TB_WEB_DATABASE
		set HEADER_IMAGE_1 = (select HEADER_IMAGE_1 from Presenter.dbo.TB_WEB_DATABASE
		where REVIEW_ID = @PlaceHolderReviewID)
		where WEBDB_ID = @WEBDB_ID
	end

	if @IMAGE_NUMBER = '2'
	begin
		update Presenter.dbo.TB_WEB_DATABASE
		set HEADER_IMAGE_2 = (select HEADER_IMAGE_2 from Presenter.dbo.TB_WEB_DATABASE
		where REVIEW_ID = @PlaceHolderReviewID)
		where WEBDB_ID = @WEBDB_ID
	end
	
	if @IMAGE_NUMBER = '3'
	begin
		update Presenter.dbo.TB_WEB_DATABASE
		set HEADER_IMAGE_3 = (select HEADER_IMAGE_3 from Presenter.dbo.TB_WEB_DATABASE
		where REVIEW_ID = @PlaceHolderReviewID)
		where WEBDB_ID = @WEBDB_ID
	end
	
SET NOCOUNT OFF











GO
/****** Object:  StoredProcedure [dbo].[st_WDDeleteIncludeCode]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE OR ALTER procedure [dbo].[st_WDDeleteIncludeCode]
(
	@WEBDB_ID int
)

As

SET NOCOUNT ON

	update Presenter.dbo.TB_WEB_DATABASE
	set ATTR_TO_INCLUDE = null
	where WEBDB_ID = @WEBDB_ID
	
	
SET NOCOUNT OFF









GO
/****** Object:  StoredProcedure [dbo].[st_WDDeleteWebDB]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_WDDeleteWebDB]
(
	@WEBDB_ID int
)

As

SET NOCOUNT ON
	
	delete from Presenter.dbo.TB_WEB_DATABASE_ATTR
	where WEBDB_ID = @WEBDB_ID
	
	delete from Presenter.dbo.TB_WEB_DATABASE
	where WEBDB_ID = @WEBDB_ID
	
SET NOCOUNT OFF









GO
/****** Object:  StoredProcedure [dbo].[st_WDDescriptionGet]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER procedure [dbo].[st_WDDescriptionGet]
(
	@WEBDB_ID int
)

As

SET NOCOUNT ON

	select w_d.[DESCRIPTION], w_d.WEBDB_NAME, w_d.RESTRICT_ACCESS,
	w_d.USERNAME, w_d.PASSWD, a.ATTRIBUTE_NAME 
	from Presenter.dbo.TB_WEB_DATABASE w_d
	left join Reviewer.dbo.TB_ATTRIBUTE a
	on w_d.ATTR_TO_INCLUDE = a.ATTRIBUTE_ID
	where w_d.WEBDB_ID = @WEBDB_ID

SET NOCOUNT OFF





GO
/****** Object:  StoredProcedure [dbo].[st_WDDescriptionGet_1]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE OR ALTER procedure [dbo].[st_WDDescriptionGet_1]
(
	@WEBDB_ID int
)

As

SET NOCOUNT ON

	select w_d.[DESCRIPTION], w_d.HEADER, w_d.WEBDB_NAME, w_d.RESTRICT_ACCESS,
	w_d.USERNAME, w_d.PASSWD, a.ATTRIBUTE_NAME, w_d.REPORT_INTRO, w_d.HEADER_IMAGE_URL_1,
	w_d.HEADER_IMAGE_URL_2, w_d.HEADER_IMAGE_URL_3, w_d.DESCRIPTION_ADMIN_EDIT, w_d.SHOW_CODING
	from Presenter.dbo.TB_WEB_DATABASE w_d
	left join Reviewer.dbo.TB_ATTRIBUTE a
	on w_d.ATTR_TO_INCLUDE = a.ATTRIBUTE_ID
	where w_d.WEBDB_ID = @WEBDB_ID

SET NOCOUNT OFF






GO
/****** Object:  StoredProcedure [dbo].[st_WDDescriptionGet_2]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







CREATE OR ALTER procedure [dbo].[st_WDDescriptionGet_2]
(
	@WEBDB_ID int
)

As

SET NOCOUNT ON

	select w_d.[DESCRIPTION], w_d.HEADER, w_d.WEBDB_NAME, w_d.RESTRICT_ACCESS,
	w_d.USERNAME, w_d.PASSWD, a.ATTRIBUTE_NAME, w_d.REPORT_INTRO, w_d.HEADER_IMAGE_URL_1,
	w_d.HEADER_IMAGE_URL_2, w_d.HEADER_IMAGE_URL_3, w_d.DESCRIPTION_ADMIN_EDIT, w_d.SHOW_CODING,
	w_d.DISPLAY_SAVED_CROSSTABS, w_d.SAVE_CROSSTABS
	from Presenter.dbo.TB_WEB_DATABASE w_d
	left join Reviewer.dbo.TB_ATTRIBUTE a
	on w_d.ATTR_TO_INCLUDE = a.ATTRIBUTE_ID
	where w_d.WEBDB_ID = @WEBDB_ID

SET NOCOUNT OFF







GO
/****** Object:  StoredProcedure [dbo].[st_WDDisplaySavedCrosstabs]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







CREATE OR ALTER procedure [dbo].[st_WDDisplaySavedCrosstabs]
(
	@WEBDB_ID int,
	@DISPLAY_SAVED_CROSSTABS bit	
)

As

SET NOCOUNT ON

	update Presenter.dbo.TB_WEB_DATABASE
	set DISPLAY_SAVED_CROSSTABS = @DISPLAY_SAVED_CROSSTABS 
	where WEBDB_ID = @WEBDB_ID
	
	
SET NOCOUNT OFF










GO
/****** Object:  StoredProcedure [dbo].[st_WDGetHeaderImage1]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO








CREATE OR ALTER procedure [dbo].[st_WDGetHeaderImage1]
(
	@WEBDB_ID int
)

As

SET NOCOUNT ON

	select HEADER_IMAGE_1 from Presenter.dbo.TB_WEB_DATABASE
	where WEBDB_ID = @WEBDB_ID

SET NOCOUNT OFF








GO
/****** Object:  StoredProcedure [dbo].[st_WDGetHeaderImage2]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO








CREATE OR ALTER procedure [dbo].[st_WDGetHeaderImage2]
(
	@WEBDB_ID int
)

As

SET NOCOUNT ON

	select HEADER_IMAGE_2 from Presenter.dbo.TB_WEB_DATABASE
	where WEBDB_ID = @WEBDB_ID

SET NOCOUNT OFF








GO
/****** Object:  StoredProcedure [dbo].[st_WDGetHeaderImage3]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO








CREATE OR ALTER procedure [dbo].[st_WDGetHeaderImage3]
(
	@WEBDB_ID int
)

As

SET NOCOUNT ON

	select HEADER_IMAGE_3 from Presenter.dbo.TB_WEB_DATABASE
	where WEBDB_ID = @WEBDB_ID

SET NOCOUNT OFF








GO
/****** Object:  StoredProcedure [dbo].[st_WDGetIntroductionData]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO










CREATE OR ALTER procedure [dbo].[st_WDGetIntroductionData]
(
	@WEBDB_ID int
)

As

SET NOCOUNT ON

	select * from Presenter.dbo.TB_WEB_DATABASE_INTRODUCTION
	where WEBDB_ID = @WEBDB_ID 

	
SET NOCOUNT OFF










GO
/****** Object:  StoredProcedure [dbo].[st_WDGetWebDatabases]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE OR ALTER procedure [dbo].[st_WDGetWebDatabases]
(
	@REVIEW_ID int
)

As

SET NOCOUNT ON

	select WEBDB_ID, WEBDB_NAME from Presenter.dbo.TB_WEB_DATABASE
	where REVIEW_ID = @REVIEW_ID

SET NOCOUNT OFF






GO
/****** Object:  StoredProcedure [dbo].[st_WDSaveCrosstabs]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO








CREATE OR ALTER procedure [dbo].[st_WDSaveCrosstabs]
(
	@WEBDB_ID int,
	@SAVE_CROSSTABS bit	
)

As

SET NOCOUNT ON

	update Presenter.dbo.TB_WEB_DATABASE
	set SAVE_CROSSTABS = @SAVE_CROSSTABS 
	where WEBDB_ID = @WEBDB_ID
	
	
SET NOCOUNT OFF











GO
/****** Object:  StoredProcedure [dbo].[st_WDSaveIntroduction]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO








CREATE OR ALTER procedure [dbo].[st_WDSaveIntroduction]
(
	@WEBDB_ID int,
	@WEBDB_NAME nvarchar(50),
	@DESCRIPTION nvarchar(max)
)

As

SET NOCOUNT ON

	update Presenter.dbo.TB_WEB_DATABASE
	set [DESCRIPTION] = @DESCRIPTION, 
	WEBDB_NAME =@WEBDB_NAME
	where WEBDB_ID = @WEBDB_ID
	
SET NOCOUNT OFF








GO
/****** Object:  StoredProcedure [dbo].[st_WDSaveIntroductionData]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO










CREATE OR ALTER procedure [dbo].[st_WDSaveIntroductionData]
(
	@WEBDB_ID int,
	@HEADER_1 nvarchar(max),
	@HEADER_2 nvarchar(max),
	@HEADER_3 nvarchar(max),
	@HEADER_4 nvarchar(max),
	@HEADER_5 nvarchar(max),
	@HEADER_6 nvarchar(max),
	@HEADER_7 nvarchar(max),
	@HEADER_8 nvarchar(max),
	@HEADER_9 nvarchar(max),
	@HEADER_10 nvarchar(max),
	@HEADER_11 nvarchar(max),
	@HEADER_12 nvarchar(max),
	@HEADER_13 nvarchar(max),
	@HEADER_14 nvarchar(max),
	@HEADER_15 nvarchar(max),
	@HEADER_16 nvarchar(max),
	@HEADER_17 nvarchar(max),
	@HEADER_18 nvarchar(max),
	@HEADER_19 nvarchar(max),
	@HEADER_20 nvarchar(max),
	@PARAGRAPH_1 nvarchar(max),
	@PARAGRAPH_2 nvarchar(max),
	@PARAGRAPH_3 nvarchar(max),
	@PARAGRAPH_4 nvarchar(max),
	@PARAGRAPH_5 nvarchar(max),
	@PARAGRAPH_6 nvarchar(max),
	@PARAGRAPH_7 nvarchar(max),
	@PARAGRAPH_8 nvarchar(max),
	@PARAGRAPH_9 nvarchar(max),
	@PARAGRAPH_10 nvarchar(max),
	@PARAGRAPH_11 nvarchar(max),
	@PARAGRAPH_12 nvarchar(max),
	@PARAGRAPH_13 nvarchar(max),
	@PARAGRAPH_14 nvarchar(max),
	@PARAGRAPH_15 nvarchar(max),
	@PARAGRAPH_16 nvarchar(max),
	@PARAGRAPH_17 nvarchar(max),
	@PARAGRAPH_18 nvarchar(max),
	@PARAGRAPH_19 nvarchar(max),
	@PARAGRAPH_20 nvarchar(max)
)

As

SET NOCOUNT ON

	update Presenter.dbo.TB_WEB_DATABASE_INTRODUCTION
	set 
	HEADER_1 = @HEADER_1,
	HEADER_2 = @HEADER_2,
	HEADER_3 = @HEADER_3,
	HEADER_4 = @HEADER_4,
	HEADER_5 = @HEADER_5,
	HEADER_6 = @HEADER_6,
	HEADER_7 = @HEADER_7,
	HEADER_8 = @HEADER_8,
	HEADER_9 = @HEADER_9,
	HEADER_10 = @HEADER_10,
	HEADER_11 = @HEADER_11,
	HEADER_12 = @HEADER_12,
	HEADER_13 = @HEADER_13,
	HEADER_14 = @HEADER_14,
	HEADER_15 = @HEADER_15,
	HEADER_16 = @HEADER_16,
	HEADER_17 = @HEADER_17,
	HEADER_18 = @HEADER_18,
	HEADER_19 = @HEADER_19,
	HEADER_20 = @HEADER_20,
	PARAGRAPH_1 = @PARAGRAPH_1,
	PARAGRAPH_2 = @PARAGRAPH_2,
	PARAGRAPH_3 = @PARAGRAPH_3,
	PARAGRAPH_4 = @PARAGRAPH_4,
	PARAGRAPH_5 = @PARAGRAPH_5,
	PARAGRAPH_6 = @PARAGRAPH_6,
	PARAGRAPH_7 = @PARAGRAPH_7,
	PARAGRAPH_8 = @PARAGRAPH_8,
	PARAGRAPH_9 = @PARAGRAPH_9,
	PARAGRAPH_10 = @PARAGRAPH_10,
	PARAGRAPH_11 = @PARAGRAPH_11,
	PARAGRAPH_12 = @PARAGRAPH_12,
	PARAGRAPH_13 = @PARAGRAPH_13,
	PARAGRAPH_14 = @PARAGRAPH_14,
	PARAGRAPH_15 = @PARAGRAPH_15,
	PARAGRAPH_16 = @PARAGRAPH_16,
	PARAGRAPH_17 = @PARAGRAPH_17,
	PARAGRAPH_18 = @PARAGRAPH_18,
	PARAGRAPH_19 = @PARAGRAPH_19,
	PARAGRAPH_20 = @PARAGRAPH_20
	where WEBDB_ID = @WEBDB_ID
	
	
SET NOCOUNT OFF










GO
/****** Object:  StoredProcedure [dbo].[st_WDSaveName]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO










CREATE OR ALTER procedure [dbo].[st_WDSaveName]
(
	@WEBDB_ID int,
	@WEBDB_NAME nvarchar(500)
)

As

SET NOCOUNT ON



	update Presenter.dbo.TB_WEB_DATABASE
	set WEBDB_NAME = @WEBDB_NAME
	where WEBDB_ID = @WEBDB_ID
	
	update Presenter.dbo.TB_WEB_DATABASE
	set HEADER = @WEBDB_NAME
	where WEBDB_ID = @WEBDB_ID
	
	
	
SET NOCOUNT OFF










GO
/****** Object:  StoredProcedure [dbo].[st_WDSetCopyToWebDB]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE OR ALTER procedure [dbo].[st_WDSetCopyToWebDB]
(
	@SET_ID int,
	@REVIEW_ID bigint, 
	@WEBDB_ID bigint, 
	@INSERTED bit output
)

As

SET NOCOUNT ON

	select * from Presenter.dbo.TB_WEB_DATABASE_ATTR 
		where SET_ID = @SET_ID
		and WEBDB_ID = @WEBDB_ID

	if @@ROWCOUNT < 1 -- we can insert all of the the rows
	begin
		-- get the parts from TB_SET
		insert into Presenter.dbo.TB_WEB_DATABASE_ATTR 
		(ATTRIBUTE_ID, WEBDB_ID, /*LEVEL,*/ ATTRIBUTE_NAME, PARENT_ATTRIBUTE_ID,
		SET_ID, ATTRIBUTE_ORDER)
		select @SET_ID, @WEBDB_ID, /*0,*/ SET_NAME, 0, @SET_ID, 0
		from Reviewer.dbo.TB_SET s
		where s.SET_ID = @SET_ID
		
		-- get the parts from TB_ATTRIBUTE_SET
		insert into Presenter.dbo.TB_WEB_DATABASE_ATTR 
		(ATTRIBUTE_ID, ATTRIBUTE_SET_ID, WEBDB_ID, /*LEVEL,*/ ATTRIBUTE_NAME, PARENT_ATTRIBUTE_ID,
		SET_ID, ATTRIBUTE_ORDER, ATTRIBUTE_DESC)
		select a_s.ATTRIBUTE_ID, a_s.ATTRIBUTE_SET_ID,  @WEBDB_ID, /*1,*/ a.ATTRIBUTE_NAME, a_s.PARENT_ATTRIBUTE_ID, @SET_ID, 
		a_s.ATTRIBUTE_ORDER, a_s.ATTRIBUTE_SET_DESC
		from Reviewer.dbo.TB_ATTRIBUTE_SET a_s inner join Reviewer.dbo.TB_ATTRIBUTE a
		on a_s.ATTRIBUTE_ID = a.ATTRIBUTE_ID
		and a_s.SET_ID = @SET_ID
		
		set @INSERTED = 1
	end
	else
	begin
		set @INSERTED = 0
	end
	
SET NOCOUNT OFF






GO
/****** Object:  StoredProcedure [dbo].[st_WDSetIncludeCode]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER procedure [dbo].[st_WDSetIncludeCode]
(
	@WEBDB_ID int,
	@ATTRIBUTE_ID int
)

As

SET NOCOUNT ON

	update Presenter.dbo.TB_WEB_DATABASE
	set ATTR_TO_INCLUDE = @ATTRIBUTE_ID
	where WEBDB_ID = @WEBDB_ID
	
	
SET NOCOUNT OFF








GO
/****** Object:  StoredProcedure [dbo].[st_WDSetPassword]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_WDSetPassword]
(
	@RESTRICT_ACCESS bit,
	@PASSWD nvarchar(50),
	@USERNAME nvarchar(50),
	@WEBDB_ID int
)

As

SET NOCOUNT ON

	update Presenter.dbo.TB_WEB_DATABASE
	set RESTRICT_ACCESS = @RESTRICT_ACCESS, 
	USERNAME = @USERNAME, PASSWD = @PASSWD
	where WEBDB_ID = @WEBDB_ID
	
	
SET NOCOUNT OFF







GO
/****** Object:  StoredProcedure [dbo].[st_WDSetRestrictAccess]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER procedure [dbo].[st_WDSetRestrictAccess]
(
	@RESTRICT_ACCESS bit,
	@WEBDB_ID int
)

As

SET NOCOUNT ON

	update Presenter.dbo.TB_WEB_DATABASE
	set RESTRICT_ACCESS = @RESTRICT_ACCESS
	where WEBDB_ID = @WEBDB_ID
	
	
SET NOCOUNT OFF








GO
/****** Object:  StoredProcedure [dbo].[st_WDTopLevelGetFromWD]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_WDTopLevelGetFromWD]
(
	@WEBDB_ID int
)

As

SET NOCOUNT ON

	select ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_SET_ID, SET_ID 
	from Presenter.dbo.TB_WEB_DATABASE_ATTR
	where ATTRIBUTE_SET_ID is null
	and WEBDB_ID = @WEBDB_ID
	order by ATTRIBUTE_ID

SET NOCOUNT OFF




GO
/****** Object:  StoredProcedure [dbo].[st_WDUploadHeaderImage]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO









CREATE OR ALTER procedure [dbo].[st_WDUploadHeaderImage]
( 
	@IMAGE_NUMBER int,
	@WEBDB_ID bigint,
	@IMAGE image
)

As

SET NOCOUNT ON


		
		if @IMAGE_NUMBER = '1'
		begin
		update Presenter.dbo.TB_WEB_DATABASE set HEADER_IMAGE_1 = @IMAGE
		where WEBDB_ID = @WEBDB_ID
		end
		
		if @IMAGE_NUMBER = '2'
		begin
		update Presenter.dbo.TB_WEB_DATABASE set HEADER_IMAGE_2 = @IMAGE
		where WEBDB_ID = @WEBDB_ID
		end 
		
		if @IMAGE_NUMBER = '3'
		begin
		update Presenter.dbo.TB_WEB_DATABASE set HEADER_IMAGE_3 = @IMAGE
		where WEBDB_ID = @WEBDB_ID
		end 
		

	
SET NOCOUNT OFF









GO
/****** Object:  StoredProcedure [dbo].[st_WDUploadHeaderURL]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO










CREATE OR ALTER procedure [dbo].[st_WDUploadHeaderURL]
( 
	@IMAGE_NUMBER int,
	@WEBDB_ID bigint,
	@HEADER_IMAGE_URL nvarchar(100)
)

As

SET NOCOUNT ON


		if @IMAGE_NUMBER = '1'
		begin
		update Presenter.dbo.TB_WEB_DATABASE set HEADER_IMAGE_URL_1 = @HEADER_IMAGE_URL
		where WEBDB_ID = @WEBDB_ID
		end
		
		if @IMAGE_NUMBER = '2'
		begin
		update Presenter.dbo.TB_WEB_DATABASE set HEADER_IMAGE_URL_2 = @HEADER_IMAGE_URL
		where WEBDB_ID = @WEBDB_ID
		end 
		
		if @IMAGE_NUMBER = '3'
		begin
		update Presenter.dbo.TB_WEB_DATABASE set HEADER_IMAGE_URL_3 = @HEADER_IMAGE_URL
		where WEBDB_ID = @WEBDB_ID
		end
		

	
SET NOCOUNT OFF










GO
/****** Object:  StoredProcedure [dbo].[st_WebDBWriteToLog]    Script Date: 2/27/2023 11:25:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER       procedure [dbo].[st_WebDBWriteToLog] (
 
	@WebDbId int
	, @ReviewID int
	, @Type nvarchar(25)
	, @Details nvarchar(max)
)

As

SET NOCOUNT ON


	if @Type = 'GetSetFrequency'
	begin
		set @Details = (select SET_NAME from Reviewer.dbo.TB_SET where SET_ID = @Details)
	end

	if @Type = 'GetFrequency'
	begin
		set @Details = (select ATTRIBUTE_NAME from Reviewer.dbo.TB_ATTRIBUTE where ATTRIBUTE_ID = @Details)
	end

	if @Type = 'ItemDetailsFromList'
	begin
		declare @itemID nvarchar(20) = @Details
		set @Details = @itemID + ',' + 
		(select SHORT_TITLE from Reviewer.dbo.TB_ITEM where ITEM_ID = @itemID)
	end
	
	insert into TB_WEBDB_LOG (WEBDB_ID, REVIEW_ID, LOG_TYPE, DETAILS)
	values (@WebDbId, @ReviewID, @Type, @Details)


	
SET NOCOUNT OFF
GO
