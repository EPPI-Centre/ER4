USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemSetBulkCompleteOnAttributePreview]    Script Date: 01/11/2024 09:17:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER FUNCTION [dbo].[fn_IsContactReviewAdmin] 
(
	-- Add the parameters for the function here
	@CONTACT_ID int,
	@REVIEW_ID int
)
RETURNS bit
AS
BEGIN
	declare @check int = 0;
	--does the contact requesting this have admin rights over the review?
	select @check = (select count(*) from TB_CONTACT c inner join TB_REVIEW_CONTACT rc on c.CONTACT_ID = rc.CONTACT_ID and c.CONTACT_ID = @CONTACT_ID and rc.REVIEW_ID = @REVIEW_ID
						Inner join TB_CONTACT_REVIEW_ROLE cr on rc.REVIEW_CONTACT_ID = cr.REVIEW_CONTACT_ID and ROLE_NAME = 'AdminUser');
	if @check = 0
	BEGIN --Is the Contact a site admin?
		select @check = (select count(*) from TB_CONTACT c where c.CONTACT_ID = @CONTACT_ID and c.IS_SITE_ADMIN = 1);
	END
	
	if @check > 0 Return 1;
	Return 0;
END
GO

CREATE OR ALTER procedure [dbo].[st_ItemSetBulkDeletePreview]
(
	@SET_ID INT,
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@DELETE_CODING_OF_CONTACT_ID INT	
)

As

SET NOCOUNT ON
declare @ItemsSets table (ItemSetId bigint primary key, ItemId bigint, IsCompleted INT, CompletedCodingExists INT null);

IF dbo.fn_IsContactReviewAdmin(@CONTACT_ID, @REVIEW_ID) = 0 RETURN;--we refuse to do this!

INSERT into @ItemsSets SELECT tis.ITEM_SET_ID, tis.ITEM_ID, tis.IS_COMPLETED, 0 from TB_ITEM_SET tis 
		inner join TB_ITEM_REVIEW ir on tis.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
		Where tis.CONTACT_ID = @DELETE_CODING_OF_CONTACT_ID and tis.SET_ID = @SET_ID

UPDATE @ItemsSets set CompletedCodingExists = 1 from
	@ItemsSets iss inner join tb_item_set tis on iss.ItemId = tis.ITEM_ID and tis.SET_ID = @SET_ID and tis.CONTACT_ID != @DELETE_CODING_OF_CONTACT_ID and iss.IsCompleted = 0 and tis.IS_COMPLETED = 1

select count(*) as TotItems, sum(IsCompleted) as Completed, sum(CompletedCodingExists) as AdditionalIncomplete from @ItemsSets
SET NOCOUNT OFF
GO

CREATE OR ALTER procedure [dbo].[st_ItemSetBulkDelete]
(
	@SET_ID INT,
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@DELETE_CODING_OF_CONTACT_ID INT,
	@Affected int output
)
WITH RECOMPILE
As


declare @ItemsSets table (ItemSetId bigint primary key);
declare @ItemsAttributes table (ItemAttributeId bigint primary key);
set @Affected= 0;
IF dbo.fn_IsContactReviewAdmin(@CONTACT_ID, @REVIEW_ID) = 0 
BEGIN 
	RETURN;--we refuse to do this!
END

INSERT into @ItemsSets SELECT tis.ITEM_SET_ID from TB_ITEM_SET tis 
		inner join TB_ITEM_REVIEW ir on tis.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID
		Where tis.CONTACT_ID = @DELETE_CODING_OF_CONTACT_ID and tis.SET_ID = @SET_ID
INSERT into @ItemsAttributes select tia.ITEM_ATTRIBUTE_ID from @ItemsSets iss inner join TB_ITEM_ATTRIBUTE tia on iss.ItemSetId = tia.ITEM_SET_ID;

BEGIN TRANSACTION;
	--We try to delete all things exiplictly (not relying on CASCADE deletions) hoping to make this faster
	DELETE from TB_ITEM_ATTRIBUTE_TEXT where ITEM_ATTRIBUTE_ID in (SELECT ItemAttributeId from @ItemsAttributes);
	DELETE from TB_ITEM_ATTRIBUTE_PDF where ITEM_ATTRIBUTE_ID in (SELECT ItemAttributeId from @ItemsAttributes);
	DELETE from TB_ITEM_ATTRIBUTE where ITEM_ATTRIBUTE_ID in (SELECT ItemAttributeId from @ItemsAttributes);
	DELETE from TB_ITEM_OUTCOME_ATTRIBUTE where ITEM_OUTCOME_ATTRIBUTE_ID in 
		(SELECT ITEM_OUTCOME_ATTRIBUTE_ID from @ItemsSets iss 
			inner join TB_ITEM_OUTCOME tio on iss.ItemSetId = tio.ITEM_SET_ID
			inner join TB_ITEM_OUTCOME_ATTRIBUTE oa on tio.OUTCOME_ID = oa.OUTCOME_ID
		);
	DELETE from TB_META_ANALYSIS_OUTCOME where OUTCOME_ID in 
		(SELECT OUTCOME_ID from @ItemsSets iss inner join TB_ITEM_OUTCOME tio on iss.ItemSetId = tio.ITEM_SET_ID);
	DELETE from TB_ITEM_OUTCOME where ITEM_SET_ID in (SELECT ItemSetId from @ItemsSets); 
	DELETE from TB_ITEM_SET where ITEM_SET_ID in (SELECT ItemSetId from @ItemsSets);
	set @Affected = @@ROWCOUNT;
COMMIT TRANSACTION;

GO