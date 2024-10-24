USE [Reviewer]
GO


/****** Object:  StoredProcedure [dbo].[st_ItemSetPrepareForRobot]    Script Date: 02/05/2024 17:11:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[st_ItemSetPrepareForRobot] 
(
	@REVIEW_ID int,
	@ROBOT_CONTACT_ID int,
	@ITEM_ID bigint,
	@REVIEW_SET_ID int,
	@IS_LOCKED bit,
	@NEW_ITEM_SET_ID bigint OUTPUT
)
AS
BEGIN
	DECLARE @IS_CODING_FINAL BIT
	declare @SET_ID int = (select SET_ID from TB_REVIEW_SET where REVIEW_ID = @REVIEW_ID and REVIEW_SET_ID = @REVIEW_SET_ID)
	if @SET_ID is null OR @SET_ID < 1 
	BEGIN --didn't find a SET_ID, can't continue
		SET @NEW_ITEM_SET_ID = -1;
		return;
	END
	declare @check bigint = (SELECT ITEM_SET_ID from TB_ITEM_REVIEW ir
							INNER JOIN TB_ITEM_SET tis on ir.REVIEW_ID = @REVIEW_ID and ir.ITEM_ID = @ITEM_ID 
								AND ir.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @SET_ID and tis.CONTACT_ID = @ROBOT_CONTACT_ID)
	IF @check is NOT NULL
	BEGIN
		--ITEM_SET exists for this robot, SET and ITEM triplet, we make sure the coding is added as @IS_LOCKED and then just return it
		UPDATE TB_ITEM_SET set IS_LOCKED = @IS_LOCKED where ITEM_SET_ID = @check
		set @NEW_ITEM_SET_ID = @check;
		select @IS_CODING_FINAL = IS_COMPLETED from TB_ITEM_SET where ITEM_SET_ID = @NEW_ITEM_SET_ID
		return;
	END
	--there is no ITEM_SET for this robot, SET and ITEM triplet, need to create one. So we need to find out if it should be COMPLETED or not
	--RULES: ITEM_SET gets created as complete ONLY if the set is in "NORMAL data entry" (1) and does NOT have coding added by someone else already (2)
	--in all other cases, the coding made via ROBOTS gets added as incomplete
	
	SELECT @IS_CODING_FINAL = CODING_IS_FINAL FROM TB_REVIEW_SET WHERE REVIEW_SET_ID = @REVIEW_SET_ID AND REVIEW_ID = @REVIEW_ID
	IF @IS_CODING_FINAL = 1 --coding tool is in normal mode Condition (1) is met
	BEGIN
		select @check = count(*) from TB_ITEM_REVIEW ir
							INNER JOIN TB_ITEM_SET tis on ir.REVIEW_ID = @REVIEW_ID and ir.ITEM_ID = @ITEM_ID 
								AND ir.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @SET_ID and tis.CONTACT_ID != @ROBOT_CONTACT_ID
		IF @check > 0 set @IS_CODING_FINAL = 0 --Condition (2) is NOT met, we will add the Robot coding as incomplete.
	END
	INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_LOCKED, IS_COMPLETED, CONTACT_ID)
	VALUES (@ITEM_ID, @SET_ID, @IS_LOCKED, @IS_CODING_FINAL, @ROBOT_CONTACT_ID)
	SET @NEW_ITEM_SET_ID = SCOPE_IDENTITY()
END

GO
