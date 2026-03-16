USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ItemAttributeInsert]    Script Date: 16/03/2026 13:16:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER procedure [dbo].[st_ItemAttributeInsert] (
	@ITEM_ID BIGINT,
	@SET_ID INT,
	@CONTACT_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@ADDITIONAL_TEXT nvarchar(max),
	@REVIEW_ID INT,
	@ITEM_ARM_ID BIGINT, -- JT added item_arm_id 10/06/2018
	@ITEM_SET_ID BIGINT = NULL, --SG new optional param, for ROBOTS - April 2024

	@NEW_ITEM_ATTRIBUTE_ID BIGINT OUTPUT,
	@NEW_ITEM_SET_ID BIGINT OUTPUT
)

As
SET NOCOUNT ON
-- NORMAL route: ITEM_SET_ID is not provided:
-- First get a valid item_set_id.
-- If is_coding_final for this review then contact_id is irrelevant.
-- If coding is complete the contact_id is irrelevant.
-- Otherwise, we need a item_set_id for this specific contact.

-- ALT route for ROBOTS: ITEM_SET_ID is provided, to guarantee coding done by the ROBOT is always recorded as such

DECLARE @IS_CODING_FINAL BIT
--DECLARE @ITEM_SET_ID BIGINT = NULL
DECLARE @CHECK BIGINT

-- JB added check if attribute isExclusive and we are using a screening tool
declare @isAttibuteExclusive bit
declare @isScreeningTool bit
declare @setTypeID int
set @isAttibuteExclusive = 0
set @isScreeningTool = 0
set @setTypeID = 0

select * from TB_ATTRIBUTE where ATTRIBUTE_ID = @ATTRIBUTE_ID and IS_EXCLUSIVE = 1
if @@rowcount > 0
begin
	set @isAttibuteExclusive = 1
	set @setTypeID = (select SET_TYPE_ID from TB_SET where SET_ID = @SET_ID)
end



IF @ITEM_SET_ID is null
BEGIN --NORMAL route	
	SELECT @IS_CODING_FINAL = CODING_IS_FINAL FROM TB_REVIEW_SET WHERE SET_ID = @SET_ID AND REVIEW_ID = @REVIEW_ID

	SELECT @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND IS_COMPLETED = 'True'
	IF (@ITEM_SET_ID IS NULL)
	BEGIN
		SELECT @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND CONTACT_ID = @CONTACT_ID
	END
END
ELSE
BEGIN --ALT route for ROBOTS
	SELECT @IS_CODING_FINAL = IS_COMPLETED from TB_ITEM_SET WHERE ITEM_SET_ID = @ITEM_SET_ID and ITEM_ID = @ITEM_ID and SET_ID = @SET_ID
	IF @@ROWCOUNT = 0 OR @IS_CODING_FINAL is null
	BEGIN --@ITEM_SET_ID appears to be wrong! Can't continue
		SET @NEW_ITEM_SET_ID = -1;
		return; 
	END
END
	
IF (@ITEM_SET_ID IS NULL) -- have to create one 
BEGIN
	INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID)
	VALUES (@ITEM_ID, @SET_ID, @IS_CODING_FINAL, @CONTACT_ID)
	SET @ITEM_SET_ID = SCOPE_IDENTITY()
END

-- We (finally) have an item_set_id we can use for our insert

-- JT modified 10/06/2018 to account for item arm ids too
-- SG modified 28/08/2018 we are passing NULL into @ITEM_ARM_ID when not adding to an arm, so need to do different thing
IF @ITEM_ARM_ID is null
begin 
	SELECT TOP(1) @CHECK = ITEM_ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID AND ITEM_SET_ID = @ITEM_SET_ID AND ITEM_ARM_ID is null
end
else
begin
	SELECT TOP(1) @CHECK = ITEM_ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID AND ITEM_SET_ID = @ITEM_SET_ID AND ITEM_ARM_ID = @ITEM_ARM_ID
end

-- JT added item_arm_id
IF (@CHECK IS NULL) -- Not sure what to do if it's not null... - SHOULD REALLY THROW AN ERROR 
BEGIN

	-- JB added - for isExclusive attributes we want to uncode any coded isExclusive siblings	
	if @isAttibuteExclusive = 1 and @setTypeID = 5 -- 5 is a screening tool
	-- we should only be here if applying an isExclusive code to a screenig tool
	begin		
		-- we need to know all of the isExlusive siblings of the selected code
		declare @isExclusiveSiblings table (tv_attributeID bigint, tv_attribute_set_id bigint, tv_itemAttributeId bigint, tv_contact_id int)
		-- we have only implimented this for screening tools at present time!!!
		insert into @isExclusiveSiblings (tv_attributeID, tv_attribute_set_id, tv_itemAttributeId, tv_contact_id)
		select a_s.ATTRIBUTE_ID, a_s.ATTRIBUTE_SET_ID, i_a.ITEM_ATTRIBUTE_ID, i_s.CONTACT_ID from TB_ATTRIBUTE_SET a_s
		inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = a_s.ATTRIBUTE_ID
		inner join TB_ITEM_ATTRIBUTE i_a on i_a.ATTRIBUTE_ID = a.ATTRIBUTE_ID
		inner join TB_ITEM_SET i_s on i_s.ITEM_SET_ID = i_a.ITEM_SET_ID
		where a.IS_EXCLUSIVE = 1 and a_s.SET_ID = @SET_ID and i_a.ITEM_ID = @ITEM_ID
		and i_s.CONTACT_ID = @CONTACT_ID

		DELETE FROM TB_ITEM_ATTRIBUTE_PDF WHERE ITEM_ATTRIBUTE_ID in (select tv_itemAttributeId from @isExclusiveSiblings)
		DELETE FROM TB_ITEM_ATTRIBUTE WHERE ITEM_ATTRIBUTE_ID in (select tv_itemAttributeId from @isExclusiveSiblings)
	end


	INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT, ITEM_ARM_ID)
	VALUES (@ITEM_ID, @ITEM_SET_ID, @ATTRIBUTE_ID, @ADDITIONAL_TEXT, @ITEM_ARM_ID)
	SET @NEW_ITEM_ATTRIBUTE_ID = @@IDENTITY 

END

SET @NEW_ITEM_SET_ID = @ITEM_SET_ID

SET NOCOUNT OFF
GO

----------------------------

