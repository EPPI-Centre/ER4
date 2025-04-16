USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_CreditPurchasesByPurchaser]    Script Date: 15/04/2025 16:42:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


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
	insert into @tv_credit_purchases (tv_credit_purchase_id, tv_date_purchased, tb_credit_purchased, tv_credit_remaining)
	  SELECT tv_credit_purchase_id, tv_date_purchased, tv_credit_purchased, tv_credit_remaining 
	  from TB_CREDIT_PURCHASE cp
		Cross apply dbo.fn_CreditRemainingDetails(cp.CREDIT_PURCHASE_ID)  as details 
		where details.tv_credit_purchase_id = cp.CREDIT_PURCHASE_ID and cp.PURCHASER_CONTACT_ID = @CONTACT_ID


	select * from @tv_credit_purchases
       
END

GO

