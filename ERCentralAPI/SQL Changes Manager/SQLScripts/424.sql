USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClassifierModels]    Script Date: 03/08/2021 14:49:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ClassifierModels]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	select MODEL_ID, MODEL_TITLE, REVIEW_ID, A1.ATTRIBUTE_NAME ATTRIBUTE_ON, A2.ATTRIBUTE_NAME ATTRIBUTE_NOT_ON,
		CONTACT_NAME, tb_CLASSIFIER_MODEL.CONTACT_ID, ACCURACY, AUC, [PRECISION], RECALL,
		ATTRIBUTE_ID_ON, ATTRIBUTE_ID_NOT_ON
		from tb_CLASSIFIER_MODEL
	INNER JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_ON
	INNER JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = tb_CLASSIFIER_MODEL.ATTRIBUTE_ID_NOT_ON
	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = tb_CLASSIFIER_MODEL.CONTACT_ID
	
	where REVIEW_ID = @REVIEW_ID
	order by MODEL_ID

SET NOCOUNT OFF

GO

ALTER   procedure [dbo].[st_ClassifierContactModels]
(
	@CONTACT_ID INT
)

As

SET NOCOUNT ON

	select MODEL_ID, MODEL_TITLE, CM.REVIEW_ID, R.REVIEW_NAME, 
	A1.ATTRIBUTE_NAME ATTRIBUTE_ON, A2.ATTRIBUTE_NAME ATTRIBUTE_NOT_ON,
	 ATTRIBUTE_ID_NOT_ON, ATTRIBUTE_ID_ON,
		CONTACT_NAME, CM.CONTACT_ID, ACCURACY, AUC, [PRECISION], RECALL
		from tb_CLASSIFIER_MODEL CM
	INNER JOIN TB_ATTRIBUTE A1 ON A1.ATTRIBUTE_ID = CM.ATTRIBUTE_ID_ON
	INNER JOIN TB_ATTRIBUTE A2 ON A2.ATTRIBUTE_ID = CM.ATTRIBUTE_ID_NOT_ON
	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = CM.CONTACT_ID
	INNER JOIN TB_REVIEW_CONTACT RC ON RC.REVIEW_ID = CM.REVIEW_ID
	INNER JOIN TB_REVIEW R ON R.REVIEW_ID = RC.REVIEW_ID
	
	where RC.CONTACT_ID = @CONTACT_ID
	order by CM.REVIEW_ID, MODEL_ID

SET NOCOUNT OFF

GO