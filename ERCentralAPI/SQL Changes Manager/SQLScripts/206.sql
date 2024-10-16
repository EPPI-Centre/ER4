﻿USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewSetsForCopy]    Script Date: 06/05/2020 10:37:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ReviewSetsForCopy]
	-- Add the parameters for the stored procedure here
	
	@REVIEW_ID int,
	@CONTACT_ID int,
	@PRIVATE_SETS bit
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	if @PRIVATE_SETS = 1
	BEGIN
		SELECT REVIEW_SET_ID, RS.REVIEW_ID, RS.SET_ID, ALLOW_CODING_EDITS, S.SET_TYPE_ID, SET_NAME, SET_TYPE, CODING_IS_FINAL, SET_ORDER, MAX_DEPTH, S.SET_DESCRIPTION, S.ORIGINAL_SET_ID, USER_CAN_EDIT_URLS
		FROM TB_REVIEW_SET RS
		INNER JOIN TB_SET S ON S.SET_ID = RS.SET_ID
		INNER JOIN TB_SET_TYPE ON TB_SET_TYPE.SET_TYPE_ID = S.SET_TYPE_ID
		INNER JOIN TB_REVIEW_CONTACT rc on RS.REVIEW_ID = rc.REVIEW_ID and rc.CONTACT_ID = @CONTACT_ID
		inner join TB_CONTACT_REVIEW_ROLE crr on rc.REVIEW_CONTACT_ID = crr.REVIEW_CONTACT_ID and ROLE_NAME = 'AdminUser'
		WHERE RS.REVIEW_ID != @REVIEW_ID
		ORDER BY RS.REVIEW_ID, RS.SET_ORDER, RS.SET_ID
	END
	ELSE
	BEGIN
		SELECT REVIEW_SET_ID, RS.REVIEW_ID, RS.SET_ID, ALLOW_CODING_EDITS, S.SET_TYPE_ID, SET_NAME, SET_TYPE, CODING_IS_FINAL, SET_ORDER, MAX_DEPTH, S.SET_DESCRIPTION, S.ORIGINAL_SET_ID, USER_CAN_EDIT_URLS
		FROM TB_REVIEW_SET RS
		INNER JOIN TB_SET S ON S.SET_ID = RS.SET_ID
		INNER JOIN TB_SET_TYPE ON TB_SET_TYPE.SET_TYPE_ID = S.SET_TYPE_ID
		inner join ReviewerAdmin.dbo.TB_MANAGEMENT_SETTINGS ms on RS.REVIEW_ID = ms.PUBLIC_CODESETS_REVIEW_ID 
		ORDER BY RS.REVIEW_ID, RS.SET_ORDER, RS.SET_ID
	END
END
GO