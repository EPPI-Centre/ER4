USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ComparisonItemAttributeSaveCheckAndRun]    Script Date: 24/07/2026 12:00:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER   procedure [dbo].[st_ComparisonItemAttributeSaveCheckAndRun] (
 @CurrentContactId int
 , @DestinationContactId int
 , @SourceContactId int
 , @attributeSetId bigint
 , @comparisonId int
 , @IncludePDFcoding bit
 , @SET_ID int
 , @ITEM_ID bigint
 , @REVIEW_ID int
 , @ITEM_ARM_ID int null
 , @mergeInfoText bit

 , @Result varchar(20) Output
 , @NEW_ITEM_ATTRIBUTE_ID BIGINT OUTPUT
 , @NEW_ITEM_SET_ID BIGINT OUTPUT
)

As
SET NOCOUNT ON

--first check, is @DestinationContactId in the comparison? If not, abort
declare @check int = (select count(*) from tb_COMPARISON where COMPARISON_ID = @comparisonId and (
        @DestinationContactId = CONTACT_ID1 OR
        @DestinationContactId = CONTACT_ID2 OR
        @DestinationContactId = CONTACT_ID3 
        )
     )
if @check is null OR @check != 1 
begin
 set @Result = 'Forbidden'
 return
end
--second check, is the current contact authorised? Needs to be a site admin, or have admin rights, or be the same as the destination.
IF (@CurrentContactId != @DestinationContactId) -- doesn't need admin rights, as it's accepting someone's coding for herself.
begin
set @check = 0
 set @check = (select count(*) from TB_REVIEW_CONTACT rc
    inner join TB_CONTACT_REVIEW_ROLE crr on rc.REVIEW_ID = @REVIEW_ID and rc.REVIEW_CONTACT_ID = crr.REVIEW_CONTACT_ID
    and crr.ROLE_NAME = 'AdminUser'
     )
 IF @check < 1
 BEGIN --not and admin in this review, perhaps a siteAdmin?
  set @check = (select count(*) from TB_CONTACT c where IS_SITE_ADMIN = 1 and c.CONTACT_ID = @CurrentContactId)
  if @check != 1 
  begin
   set @Result = 'Forbidden'
   return
  end
 END
end
--if we didn't return then it's all good

declare @isAttibuteExclusive bit = 0
declare @isScreeningTool bit = 0
declare @isStandardTool bit = 0
declare @itemSetId bigint

declare @srcItemAttId bigint
declare @infoText nvarchar(max)
declare @attributeId bigint
select @srcItemAttId = item_attribute_id, @infoText = tia.ADDITIONAL_TEXT, @attributeId = tia.ATTRIBUTE_ID from TB_ITEM_ATTRIBUTE tia
 inner join TB_ATTRIBUTE_SET tas on tia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID 
  and tas.ATTRIBUTE_SET_ID = @attributeSetId and tas.SET_ID = @SET_ID
  and (tia.ITEM_ARM_ID = @ITEM_ARM_ID OR (@ITEM_ARM_ID is null AND tia.ITEM_ARM_ID is null))
 inner join TB_ITEM_SET tis on tia.ITEM_SET_ID = tis.ITEM_SET_ID and tis.CONTACT_ID = @SourceContactId and tis.ITEM_ID = @ITEM_ID
 
if @srcItemAttId is null OR @srcItemAttId < 1 OR @attributeId is null OR @attributeId < 1
begin
 set @Result = 'failed'
 return
end
else
begin
 select @isAttibuteExclusive = IS_EXCLUSIVE from TB_ATTRIBUTE where ATTRIBUTE_ID = @srcItemAttId
 set @itemSetId = (SELECT ITEM_SET_ID FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND IS_COMPLETED = 'True')
end

declare @destItemAttId bigint = (select item_attribute_id from TB_ITEM_ATTRIBUTE tia
 inner join TB_ATTRIBUTE_SET tas on tia.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and tas.ATTRIBUTE_SET_ID = @attributeSetId
  and tas.ATTRIBUTE_SET_ID = @attributeSetId and tas.SET_ID = @SET_ID and tia.ITEM_ID = @ITEM_ID
  and (tia.ITEM_ARM_ID = @ITEM_ARM_ID OR (@ITEM_ARM_ID is null AND tia.ITEM_ARM_ID is null))
 inner join TB_ITEM_SET tis on tia.ITEM_SET_ID = tis.ITEM_SET_ID and tis.CONTACT_ID = @DestinationContactId
 )


-- added by Jeff 14/07/2026 for merge of infoBoxText
if @mergeInfoText = 1
begin
 declare @destinationInfoBoxText nvarchar(max)
 set @destinationInfoBoxText = (select ADDITIONAL_TEXT from TB_ITEM_ATTRIBUTE
 where ITEM_ATTRIBUTE_ID = @destItemAttId)

 declare @sourceReviewerName nvarchar(255)
 declare @destinationReviewerName nvarchar(255)
 set @sourceReviewerName = (select CONTACT_NAME from TB_CONTACT where CONTACT_ID = @SourceContactId)
 set @destinationReviewerName = (select CONTACT_NAME from TB_CONTACT where CONTACT_ID = @DestinationContactId)
  
 if ((trim(@destinationInfoBoxText) = '') OR (@destinationInfoBoxText is null))
 begin
  set @infoText = '[' + @sourceReviewerName + ']' + CHAR(13) + CHAR(10) + @infoText
 end
 else
 begin
  set @infoText = '[' + @sourceReviewerName + ']' + CHAR(13) + CHAR(10) + @infoText + CHAR(13) + CHAR(10) + '[' + @destinationReviewerName + ']' + CHAR(13) + CHAR(10) + @destinationInfoBoxText
 end
end


set @Result = 'failed' --we'll change if we reach the end!
if (@destItemAttId is null OR @destItemAttId = 0)
BEGIN
--the destination user does not have assigned this code at all, so it's an "Insert" operation
 exec st_ItemAttributeInsert
  @CONTACT_ID = @DestinationContactId
  ,@ADDITIONAL_TEXT = @infoText
  ,@ATTRIBUTE_ID = @attributeId
  ,@SET_ID = @SET_ID
  ,@ITEM_ID = @ITEM_ID
  ,@REVIEW_ID = @REVIEW_ID
  ,@ITEM_ARM_ID = @ITEM_ARM_ID
  ,@NEW_ITEM_ATTRIBUTE_ID = @NEW_ITEM_ATTRIBUTE_ID output
  ,@NEW_ITEM_SET_ID = @NEW_ITEM_SET_ID output

 set @destItemAttId = @NEW_ITEM_ATTRIBUTE_ID --for simplicity, @destItemAttId is now always set to "where we are going to..."
END
else
BEGIN
 --the destination user does not have assigned this code at all, so it's an "Update" operation
 exec st_ItemAttributeUpdate  
  @ITEM_ATTRIBUTE_ID = @destItemAttId
  ,@ADDITIONAL_TEXT = @infoText

 select @NEW_ITEM_ATTRIBUTE_ID = @destItemAttId, @NEW_ITEM_SET_ID = item_set_id 
   from TB_ITEM_ATTRIBUTE ia where ia.ITEM_ATTRIBUTE_ID = @destItemAttId

END

if @IncludePDFcoding = 1
begin
 --we also want to copy across the PDF coding...
 --we first check if there is any to copy!
 select @check = count(*) from TB_ITEM_ATTRIBUTE_PDF p where p.ITEM_ATTRIBUTE_ID = @srcItemAttId
 if @check > 0
 begin
  --we do have PDF coding to move across...
  --we do a merge, keep the destination pages in place, unless the source has coding from that pages as well, in which case we overwrite

  declare @conflictPages table (pageId bigint)
  insert into @conflictPages select p1.item_attribute_pdf_id from TB_ITEM_ATTRIBUTE_PDF p1
  inner join TB_ITEM_ATTRIBUTE_PDF p2 on p1.ITEM_DOCUMENT_ID = p2.ITEM_DOCUMENT_ID
            and p1.PAGE = p2.PAGE
            and p1.ITEM_ATTRIBUTE_ID = @destItemAttId --thus, p1 is the destination bunch
            and p2.ITEM_ATTRIBUTE_ID = @srcItemAttId
  --delete conflict pages in destination
  delete from TB_ITEM_ATTRIBUTE_PDF where ITEM_ATTRIBUTE_PDF_ID in (select pageId from @conflictPages)

  --copy the pages coding across
  INSERT INTO [dbo].[TB_ITEM_ATTRIBUTE_PDF]
       ([ITEM_DOCUMENT_ID]
       ,[ITEM_ATTRIBUTE_ID]
       ,[PAGE]
       ,[SHAPE_TEXT]
       ,[SELECTION_INTERVALS]
       ,[SELECTION_TEXTS]
       ,[PDFTRON_XML])
     select [ITEM_DOCUMENT_ID]
       ,@destItemAttId
       ,[PAGE]
       ,[SHAPE_TEXT]
       ,[SELECTION_INTERVALS]
       ,[SELECTION_TEXTS]
      ,[PDFTRON_XML] from TB_ITEM_ATTRIBUTE_PDF where ITEM_ATTRIBUTE_ID = @srcItemAttId

 end
end
set @Result = 'success' --we got here, so we think it worked...
SET NOCOUNT OFF
GO

