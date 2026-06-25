USE [REVIEWER]
GO
IF COL_LENGTH('dbo.TB_SET', 'SET_DESCRIPTION') != -1 --'-1' stands for max, apparently!
BEGIN 
	select 'expanding set description fields'
	ALTER TABLE TB_SET ALTER COLUMN SET_DESCRIPTION NVARCHAR(max) NULL;
	ALTER TABLE TB_WEBDB_PUBLIC_SET ALTER COLUMN WEBDB_SET_DESCRIPTION NVARCHAR(max) NULL;
END

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetInsert]    Script Date: 03/04/2026 12:27:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   procedure [dbo].[st_ReviewSetInsert]
(
	@REVIEW_ID INT,
	@SET_TYPE_ID INT = 3,
	@ALLOW_CODING_EDITS BIT = false,
	@SET_NAME NVARCHAR(255),
	@CODING_IS_FINAL BIT = true,
	@SET_ORDER INT = 0,
	@SET_DESCRIPTION nvarchar(max) = '',
	@ORIGINAL_SET_ID int = null,
	@OLDEST_KNOWN_SET_ID int = null,
	@NEW_REVIEW_SET_ID INT OUTPUT,
	@NEW_SET_ID INT OUTPUT
)

As

SET NOCOUNT ON

	INSERT INTO TB_SET(SET_TYPE_ID, SET_NAME, SET_DESCRIPTION, ORIGINAL_SET_ID, OLDEST_KNOWN_SET_ID, USER_CAN_EDIT_URLS)
		VALUES(@SET_TYPE_ID, @SET_NAME, @SET_DESCRIPTION, @ORIGINAL_SET_ID, @OLDEST_KNOWN_SET_ID, 'False')

	SET @NEW_SET_ID = @@IDENTITY

	INSERT INTO TB_REVIEW_SET(REVIEW_ID, SET_ID, ALLOW_CODING_EDITS, CODING_IS_FINAL, SET_ORDER)
		VALUES(@REVIEW_ID, @NEW_SET_ID, @ALLOW_CODING_EDITS, @CODING_IS_FINAL, @SET_ORDER)

	SET @NEW_REVIEW_SET_ID = @@IDENTITY


SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetUpdate]    Script Date: 03/04/2026 12:29:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   procedure [dbo].[st_ReviewSetUpdate]
(
	@REVIEW_SET_ID INT,
	@SET_ID INT,
	@ALLOW_CODING_EDITS BIT,
	@CODING_IS_FINAL BIT,
	@SET_NAME NVARCHAR(255),
	@SET_ORDER INT,
	@SET_DESCRIPTION nvarchar(max),
	@ITEM_SET_ID BIGINT = NULL,
	@IS_COMPLETED BIT = NULL,
	@IS_LOCKED BIT = NULL,
	@REVIEW_ID INT,
	@USER_CAN_EDIT_URLS BIT = 'False'
)

As

SET NOCOUNT ON


declare @check int = 0
set @check = (select count(REVIEW_SET_ID) from 
TB_REVIEW_SET where REVIEW_SET_ID = @REVIEW_SET_ID and REVIEW_ID = @REVIEW_ID)
if(@check != 1) return

UPDATE TB_SET SET SET_NAME = @SET_NAME, SET_DESCRIPTION = @SET_DESCRIPTION, USER_CAN_EDIT_URLS = @USER_CAN_EDIT_URLS
	WHERE SET_ID = @SET_ID
UPDATE TB_REVIEW_SET SET ALLOW_CODING_EDITS = @ALLOW_CODING_EDITS,
	CODING_IS_FINAL = @CODING_IS_FINAL,
	SET_ORDER = @SET_ORDER
WHERE REVIEW_SET_ID = @REVIEW_SET_ID
	
IF (@ITEM_SET_ID > 0)
BEGIN
	UPDATE TB_ITEM_SET
	SET IS_COMPLETED = @IS_COMPLETED, IS_LOCKED = @IS_LOCKED
	WHERE ITEM_SET_ID = @ITEM_SET_ID
END

SET NOCOUNT OFF
GO


ALTER   PROCEDURE [dbo].[st_WebDbCodeSetEdit]
(
	@REVIEW_ID INT,
	@WEBDB_ID int,
	@Set_ID int,
	@Public_Name nvarchar(255),
	@Public_Descr nvarchar(max)
)
As
declare @r_set_id int = (select review_set_id from TB_WEBDB w
						inner join TB_REVIEW_SET rs on rs.SET_ID = @Set_ID and rs.REVIEW_ID = @REVIEW_ID and w.REVIEW_ID = rs.REVIEW_ID
						where w.WEBDB_ID = @WEBDB_ID and w.REVIEW_ID = @REVIEW_ID)
--Just a basic sanity check: can we get a REVIEW_SET_ID?
IF @r_set_id is null OR @r_set_id < 1 return
update TB_WEBDB_PUBLIC_SET set WEBDB_SET_NAME = @Public_Name, WEBDB_SET_DESCRIPTION = @Public_Descr 
 where REVIEW_SET_ID = @r_set_id and WEBDB_ID = @WEBDB_ID
 GO
 USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeInsert]    Script Date: 25/06/2026 15:43:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   procedure [dbo].[st_ItemAttributeInsert] (
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
declare @isAttibuteExclusive bit = 0
declare @isScreeningTool bit = 0
declare @isStandardTool bit = 0

select @isAttibuteExclusive = IS_EXCLUSIVE from TB_ATTRIBUTE where ATTRIBUTE_ID = @ATTRIBUTE_ID 



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

	-- JB&SG added - for isExclusive attributes we want to uncode any coded isExclusive siblings	
	if @isAttibuteExclusive = 1 
	-- and @setTypeID = 5 -- 5 is a screening tool
	-- we should only be here if applying an isExclusive code to a screenig tool
	begin
		select @isScreeningTool = 1 from TB_SET s
		inner join TB_SET_TYPE t on s.SET_TYPE_ID = t.SET_TYPE_ID where t.SET_TYPE = 'Screening' and s.SET_ID = @SET_ID
		if (@isScreeningTool = 0)
		begin
			select @isStandardTool = 1 from TB_SET s
			inner join TB_SET_TYPE t on s.SET_TYPE_ID = t.SET_TYPE_ID where t.SET_TYPE = 'Standard' and s.SET_ID = @SET_ID
		end

		-- we need to know all of the isExlusive siblings of the selected code
		declare @siblings table (AttId bigint primary key)
		declare @check2 bit = 0
		if @isScreeningTool = 1
		BEGIN
			insert into @siblings SELECT a.ATTRIBUTE_ID from TB_ATTRIBUTE_SET tas
				inner join TB_ATTRIBUTE a on tas.ATTRIBUTE_ID = a.ATTRIBUTE_ID and a.ATTRIBUTE_ID != @ATTRIBUTE_ID  
				where SET_ID = @SET_ID and a.IS_EXCLUSIVE = 1
			if @@ROWCOUNT > 0 set @check2 = 1
		END
		else if @isStandardTool = 1
		BEGIN
			insert into @siblings SELECT tas2.ATTRIBUTE_ID from 
				TB_ATTRIBUTE_SET tas
				inner join TB_ATTRIBUTE_SET tas2 on  tas.PARENT_ATTRIBUTE_ID = tas2.PARENT_ATTRIBUTE_ID and tas2.SET_ID = @SET_ID and tas2.ATTRIBUTE_ID != @ATTRIBUTE_ID
				inner join TB_ATTRIBUTE a on tas2.ATTRIBUTE_ID = a.ATTRIBUTE_ID and a.IS_EXCLUSIVE = 1
				where tas.ATTRIBUTE_ID = @ATTRIBUTE_ID and tas.SET_ID = @SET_ID
			if @@ROWCOUNT > 0 set @check2 = 1
		END
		if @check2 = 1
		BEGIN
			DELETE FROM TB_ITEM_ATTRIBUTE_PDF WHERE ITEM_ATTRIBUTE_ID in 
			(
				select tia.ITEM_ATTRIBUTE_ID from @siblings s
				inner join TB_ITEM_ATTRIBUTE tia on s.AttId = tia.ATTRIBUTE_ID and tia.ITEM_SET_ID = @ITEM_SET_ID and tia.ITEM_ID = @ITEM_ID
					AND 
					(
						(@ITEM_ARM_ID is null AND tia.ITEM_ARM_ID is null)--whole study
						OR
						(@ITEM_ARM_ID = tia.ITEM_ARM_ID)--a specific itemArm
					)
			)

			DELETE FROM TB_ITEM_ATTRIBUTE WHERE ITEM_ATTRIBUTE_ID in 
			(
				select tia.ITEM_ATTRIBUTE_ID from @siblings s
				inner join TB_ITEM_ATTRIBUTE tia on s.AttId = tia.ATTRIBUTE_ID and tia.ITEM_SET_ID = @ITEM_SET_ID and tia.ITEM_ID = @ITEM_ID
				AND 
					(
						(@ITEM_ARM_ID is null AND tia.ITEM_ARM_ID is null)--whole study
						OR
						(@ITEM_ARM_ID = tia.ITEM_ARM_ID)--a specific itemArm
					)
			)
		END
		--declare @isExclusiveSiblings table (tv_attributeID bigint, tv_attribute_set_id bigint, tv_itemAttributeId bigint, tv_contact_id int)
		---- we have only implimented this for screening tools at present time!!!
		--insert into @isExclusiveSiblings (tv_attributeID, tv_attribute_set_id, tv_itemAttributeId, tv_contact_id)
		--select a_s.ATTRIBUTE_ID, a_s.ATTRIBUTE_SET_ID, i_a.ITEM_ATTRIBUTE_ID, i_s.CONTACT_ID from TB_ATTRIBUTE_SET a_s
		--inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = a_s.ATTRIBUTE_ID
		--inner join TB_ITEM_ATTRIBUTE i_a on i_a.ATTRIBUTE_ID = a.ATTRIBUTE_ID
		--inner join TB_ITEM_SET i_s on i_s.ITEM_SET_ID = i_a.ITEM_SET_ID
		--where a.IS_EXCLUSIVE = 1 and a_s.SET_ID = @SET_ID and i_a.ITEM_ID = @ITEM_ID
		--and i_s.CONTACT_ID = @CONTACT_ID

		--DELETE FROM TB_ITEM_ATTRIBUTE_PDF WHERE ITEM_ATTRIBUTE_ID in (select tv_itemAttributeId from @isExclusiveSiblings)
		--DELETE FROM TB_ITEM_ATTRIBUTE WHERE ITEM_ATTRIBUTE_ID in (select tv_itemAttributeId from @isExclusiveSiblings)
	end


	INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT, ITEM_ARM_ID)
	VALUES (@ITEM_ID, @ITEM_SET_ID, @ATTRIBUTE_ID, @ADDITIONAL_TEXT, @ITEM_ARM_ID)
	SET @NEW_ITEM_ATTRIBUTE_ID = SCOPE_IDENTITY() 

END

SET @NEW_ITEM_SET_ID = @ITEM_SET_ID

SET NOCOUNT OFF
GO