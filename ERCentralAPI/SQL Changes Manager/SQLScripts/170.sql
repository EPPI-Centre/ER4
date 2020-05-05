USE [ReviewerAdmin]
GO

IF COL_LENGTH('dbo.TB_MANAGEMENT_SETTINGS', 'ENABLE_SHOP_CREDIT') IS NULL
BEGIN
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION

ALTER TABLE dbo.TB_MANAGEMENT_SETTINGS ADD
	ENABLE_SHOP_CREDIT bit NOT NULL CONSTRAINT DF_TB_MANAGEMENT_SETTINGS_ENABLE_SHOP_CREDIT DEFAULT 0

ALTER TABLE dbo.TB_MANAGEMENT_SETTINGS SET (LOCK_ESCALATION = TABLE)

COMMIT

END
GO

IF COL_LENGTH('dbo.TB_MANAGEMENT_SETTINGS', 'ENABLE_SHOP_DEBIT') IS NULL
BEGIN
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION

ALTER TABLE dbo.TB_MANAGEMENT_SETTINGS ADD
	ENABLE_SHOP_DEBIT bit NOT NULL CONSTRAINT DF_TB_MANAGEMENT_SETTINGS_ENABLE_SHOP_DEBIT DEFAULT 0

ALTER TABLE dbo.TB_MANAGEMENT_SETTINGS SET (LOCK_ESCALATION = TABLE)

COMMIT

END
GO

USE [ReviewerAdmin]
GO
/****** Object: StoredProcedure [dbo].[st_BillMarkAsPaid] Script Date: 03/04/2020 13:17:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author: <Author,,Name>
-- Create date: <Create Date,,>
-- Description: <Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_BillMarkAsPaid]
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

