USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_ContactBills]    Script Date: 6/7/2023 4:39:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_ContactBills] 
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
	inner join sTB_CONTACT c
	on b.PURCHASER_CONTACT_ID = c.CONTACT_ID
	where b.PURCHASER_CONTACT_ID = @CONTACT_ID AND DUE_PRICE is not null

	RETURN

END

GO