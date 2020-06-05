use [Reviewer]


-- ********************* script to create the RobotReviewer codeset ***************************
declare @chk int = 0
declare @set_id int = 0
declare @parent_attribute_id bigint = 0
declare @sub_parent_pattribute_id bigint = 0
declare @child_attribute_id bigint = 0

select @chk = count(*) from tb_set where set_name = 'RobotReviewer classifications'
if @chk = 0
BEGIN
	insert into tb_set(SET_TYPE_ID, SET_NAME, OLD_GUIDELINE_ID, SET_DESCRIPTION, ORIGINAL_SET_ID, USER_CAN_EDIT_URLS)
		values(3, 'RobotReviewer classifications', 'RobotReviewer', 'Coding tool for obtaining automatic classifications and risk of bias assessment from RobotReviewer', NULL, 'False')
	set @set_id = @@IDENTITY
	
	if not @set_id is null
	BEGIN

		-- Study type classifiers
		insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
			values('Study type classifiers', 'RR1')
		set @parent_attribute_id = @@IDENTITY

		if not @parent_attribute_id is null
		BEGIN
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@parent_attribute_id, @set_id, 0, 1)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Is RCT (balanced)', 'RR2')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Is RCT (precise)', 'RR3')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Is RCT (sensitive)', 'RR4')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Is RCT', 'RR5')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Is human study', 'RR6')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
		END

		-- PICO spans (abstract)
		insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
			values('PICO Spans (from abstract)', 'RR7')
		set @parent_attribute_id = @@IDENTITY

		if not @parent_attribute_id is null
		BEGIN
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@parent_attribute_id, @set_id, 0, 1)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Population', 'RR8')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Interventions', 'RR9')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Outcomes', 'RR10')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
		END

		-- MeSH Terms
		insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
			values('MeSH Terms', 'RR11')
		set @parent_attribute_id = @@IDENTITY

		if not @parent_attribute_id is null
		BEGIN
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@parent_attribute_id, @set_id, 0, 1)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Population', 'RR12')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Interventions', 'RR13')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Outcomes', 'RR14')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
		END

		-- Risk of bias (on full text)
		insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
			values('Risk of bias (on full document)', 'RR15')
		set @sub_parent_pattribute_id = @@IDENTITY

		if not @sub_parent_pattribute_id is null
		BEGIN
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@sub_parent_pattribute_id, @set_id, 0, 1)

			-- Random sequence generation
			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Random sequence generation', 'RR16')
			set @parent_attribute_id = @@IDENTITY

			if not @parent_attribute_id is null
			BEGIN
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@parent_attribute_id, @set_id, @sub_parent_pattribute_id, 1)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('Low', 'RR17')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('High / unclear', 'RR18')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
			END

			-- Allocation concealment
			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Allocation concealment', 'RR19')
			set @parent_attribute_id = @@IDENTITY

			if not @parent_attribute_id is null
			BEGIN
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@parent_attribute_id, @set_id, @sub_parent_pattribute_id, 1)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('Low', 'RR20')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('High / unclear', 'RR21')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
			END

			-- Blinding of participants and personnel
			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Blinding of participants and personnel', 'RR22')
			set @parent_attribute_id = @@IDENTITY

			if not @parent_attribute_id is null
			BEGIN
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@parent_attribute_id, @set_id, @sub_parent_pattribute_id, 1)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('Low', 'RR23')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('High / unclear', 'RR24')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
			END

			-- Blinding of outcome assessment
			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Blinding of outcome assessment', 'RR25')
			set @parent_attribute_id = @@IDENTITY

			if not @parent_attribute_id is null
			BEGIN
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@parent_attribute_id, @set_id, @sub_parent_pattribute_id, 1)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('Low', 'RR26')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('High / unclear', 'RR27')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
			END
		END

		-- Risk of bias (on abstract)
		insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
			values('Risk of bias (on abstract alone)', 'RR28')
		set @sub_parent_pattribute_id = @@IDENTITY

		if not @sub_parent_pattribute_id is null
		BEGIN
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@sub_parent_pattribute_id, @set_id, 0, 1)

			-- Random sequence generation
			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Random sequence generation', 'RR29')
			set @parent_attribute_id = @@IDENTITY

			if not @parent_attribute_id is null
			BEGIN
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@parent_attribute_id, @set_id, @sub_parent_pattribute_id, 1)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('Low', 'RR30')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('High / unclear', 'RR31')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
			END

			-- Allocation concealment
			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Allocation concealment', 'RR32')
			set @parent_attribute_id = @@IDENTITY

			if not @parent_attribute_id is null
			BEGIN
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@parent_attribute_id, @set_id, @sub_parent_pattribute_id, 1)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('Low', 'RR33')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('High / unclear', 'RR34')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
			END

			-- Blinding of participants and personnel
			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Blinding of participants and personnel', 'RR35')
			set @parent_attribute_id = @@IDENTITY

			if not @parent_attribute_id is null
			BEGIN
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@parent_attribute_id, @set_id, @sub_parent_pattribute_id, 1)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('Low', 'RR36')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('High / unclear', 'RR37')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
			END

			-- Blinding of outcome assessment
			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Blinding of outcome assessment', 'RR38')
			set @parent_attribute_id = @@IDENTITY

			if not @parent_attribute_id is null
			BEGIN
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@parent_attribute_id, @set_id, @sub_parent_pattribute_id, 1)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('Low', 'RR39')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

				insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
					values('High / unclear', 'RR40')
				set @child_attribute_id = @@IDENTITY
				insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
					values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
			END
		END

		-- sample size
		insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
			values('Sample size', 'RR41')
		set @parent_attribute_id = @@IDENTITY

		if not @parent_attribute_id is null
		BEGIN
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@parent_attribute_id, @set_id, 0, 1)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Number randomized', 'RR42')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
		END

		-- Punchline
		insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
			values('Punchline', 'RR43')
		set @parent_attribute_id = @@IDENTITY

		if not @parent_attribute_id is null
		BEGIN
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@parent_attribute_id, @set_id, 0, 1)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Punchline text', 'RR44')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Effect', 'RR45')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
		END

		-- PICO bot (on full text)
		insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
			values('PICO text (full text)', 'RR46')
		set @parent_attribute_id = @@IDENTITY

		if not @parent_attribute_id is null
		BEGIN
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@parent_attribute_id, @set_id, 0, 1)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Population', 'RR47')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Interventions', 'RR48')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)

			insert into TB_ATTRIBUTE(ATTRIBUTE_NAME, Ext_URL)
				values('Outcomes', 'RR49')
			set @child_attribute_id = @@IDENTITY
			insert into TB_ATTRIBUTE_SET(ATTRIBUTE_ID, SET_ID, PARENT_ATTRIBUTE_ID, ATTRIBUTE_TYPE_ID)
				values(@child_attribute_id, @set_id, @parent_attribute_id, 2)
		END

	END
END
GO
-- ***************** end create RobotReviewer codeset ****************


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeUpdateWithoutKey]    Script Date: 04/06/2020 16:13:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create or ALTER procedure [dbo].[st_ItemAttributeUpdateWithoutKey] (
	@CONTACT_ID INT,
	@ADDITIONAL_TEXT nvarchar(max),
	@ATTRIBUTE_ID BIGINT,
	@SET_ID INT,
	@ITEM_ID BIGINT,
	@REVIEW_ID INT,
	@ITEM_ATTRIBUTE_ID BIGINT OUTPUT
)

As
SET NOCOUNT ON


DECLARE @IS_CODING_FINAL BIT
DECLARE @ITEM_SET_ID BIGINT = NULL

SELECT @IS_CODING_FINAL = CODING_IS_FINAL FROM TB_REVIEW_SET WHERE SET_ID = @SET_ID AND REVIEW_ID = @REVIEW_ID

SELECT @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND IS_COMPLETED = 'True'
IF (@ITEM_SET_ID IS NULL)
BEGIN
	SELECT @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND CONTACT_ID = @CONTACT_ID
END

SELECT @ITEM_ATTRIBUTE_ID = ITEM_ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE WHERE ITEM_SET_ID = @ITEM_SET_ID AND ATTRIBUTE_ID = @ATTRIBUTE_ID
	
IF (NOT @ITEM_ATTRIBUTE_ID IS NULL)
BEGIN
	UPDATE TB_ITEM_ATTRIBUTE
		SET ADDITIONAL_TEXT = @ADDITIONAL_TEXT
		WHERE ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID
END


SET NOCOUNT OFF

GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributePDFInsert]    Script Date: 05/06/2020 10:59:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create or ALTER PROCEDURE [dbo].[st_ItemAttributePDFInsert]
	-- Add the parameters for the stored procedure here
	@ITEM_ATTRIBUTE_ID bigint
	,@ITEM_DOCUMENT_ID bigint
	,@PAGE int
	,@SHAPE_TEXT varchar(max)
	,@INTERVALS varchar(max)
	,@TEXTS nvarchar(max)
	,@ITEM_ATTRIBUTE_PDF_ID bigint output
	,@PDFTRON_XML varchar(max) = ''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @COUNT_DUPS int = 0
set @ITEM_ATTRIBUTE_PDF_ID = 0

IF @INTERVALS = '0;0' -- this is from RobotReviewer: we don't want to add duplicates
BEGIN
	select @COUNT_DUPS = count(*) from TB_ITEM_ATTRIBUTE_PDF
		where ITEM_ATTRIBUTE_ID = @ITEM_ATTRIBUTE_ID and ITEM_DOCUMENT_ID = @ITEM_DOCUMENT_ID and
			SELECTION_TEXTS = @TEXTS
END

if @COUNT_DUPS = 0
BEGIN
    -- Insert statements for procedure here
    INSERT INTO TB_ITEM_ATTRIBUTE_PDF
           ([ITEM_DOCUMENT_ID]
           ,[ITEM_ATTRIBUTE_ID]
           ,[PAGE]
           ,[SHAPE_TEXT]
           ,[SELECTION_INTERVALS]
           ,[SELECTION_TEXTS]
		   ,PDFTRON_XML)
     VALUES
           (@ITEM_DOCUMENT_ID
           ,@ITEM_ATTRIBUTE_ID
           ,@PAGE
           ,@SHAPE_TEXT
           ,@INTERVALS
           ,@TEXTS
		   ,@PDFTRON_XML)
	set @ITEM_ATTRIBUTE_PDF_ID = @@IDENTITY
end

END
GO


