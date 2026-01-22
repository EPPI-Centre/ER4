USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_AttributeSetDeleteWarning]    Script Date: 22/01/2026 09:26:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_AttributeSetDeleteWarning]
(
	@ATTRIBUTE_SET_ID BIGINT,
	@SET_ID INT,
	@NUM_ITEMS BIGINT OUTPUT,
	@NUM_ALLOCATIONS int = 0 OUTPUT,
	@NUM_VIS_MAPS int = 0 OUTPUT,
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

	SELECT @NUM_ITEMS = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_SET.SET_ID
			AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
			AND TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
	SELECT @NUM_ALLOCATIONS = count(*) from TB_WORK_ALLOCATION w
	inner join TB_ATTRIBUTE_SET tas on w.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and tas.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID and w.REVIEW_ID = @REVIEW_ID
	
	-- added to check if the code is used in a visualisation
	declare @setId int
	set @setId = (select SET_ID from TB_ATTRIBUTE_SET where ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID)

	declare @numVis int = 0
	declare @numVisMaps int = 0
	set @numVis = (select count(*) from TB_WEBDB_PUBLIC_SET wps
		inner join TB_REVIEW_SET rs on rs.REVIEW_SET_ID = wps.REVIEW_SET_ID
		where rs.SET_ID = @setId)
	if @numVis > 0
	begin
		-- we know that the code is in a coding tool that is used in a visualisation 
		-- so see if it is also mentioned in a pre-configured map
		declare @attributeId int
		set @attributeId = (select ATTRIBUTE_ID from TB_ATTRIBUTE_SET where ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID)
		-- there could be multiple WEB_IDs in the visualisation so there could be multiple WEBDB_PUBLIC_ATTRIBUTE_IDs
		-- associated with the ATTRIBUTE_ID so I need to create a table of possible WEBDB_PUBLIC_ATTRIBUTE_IDs
		declare @webDbPublicAttributeIDs table (tv_webDbPublicAttributeID int)
		insert into @webDbPublicAttributeIDs
		select WEBDB_PUBLIC_ATTRIBUTE_ID from TB_WEBDB_PUBLIC_ATTRIBUTE where ATTRIBUTE_ID = @attributeId
		--select * from @webDbPublicAttributeIDs
		-- are any of these WEBDB_PUBLIC_ATTRIBUTE_IDs in TB_WEBDB_MAP ? 		
		set @numVisMaps = (select count(*) from TB_WEBDB_MAP wm
			inner join @webDbPublicAttributeIDs wpai on 
			wpai.tv_webDbPublicAttributeID = wm.COLUMNS_PUBLIC_ATTRIBUTE_ID
			or wpai.tv_webDbPublicAttributeID = wm.ROWS_PUBLIC_ATTRIBUTE_ID 
			or wpai.tv_webDbPublicAttributeID = wm.SEGMENTS_PUBLIC_ATTRIBUTE_ID)

		-- if the code is part of a coding tool that is in a visualisation but is not explicitly listed in a map then @NUM_VIS_MAPS = 1
		-- if the code is part of a coding tool that is in a visualisation and is explicitly listed in a map then @NUM_VIS_MAPS = 2
		set @NUM_VIS_MAPS = 1
		if @numVis > 0 and @numVisMaps > 0
		begin
			set @NUM_VIS_MAPS = 2
		end
	end

SET NOCOUNT OFF
GO

---------------------------------------

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewSetDeleteWarning]    Script Date: 22/01/2026 09:27:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER procedure [dbo].[st_ReviewSetDeleteWarning]
(
	@ATTRIBUTE_SET_ID BIGINT,
	@SET_ID INT,
	@NUM_ITEMS BIGINT OUTPUT,
	@NUM_ALLOCATIONS int = 0 OUTPUT,
	@NUM_VIS_MAPS int = 0 OUTPUT,
	@REVIEW_ID INT
)

As

SET NOCOUNT ON


	SELECT @NUM_ITEMS = COUNT(DISTINCT TB_ITEM_SET.ITEM_ID) FROM TB_ITEM_SET
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_SET.ITEM_ID
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		WHERE TB_ITEM_SET.SET_ID = @SET_ID
	Select @NUM_ALLOCATIONS = count(*) from TB_WORK_ALLOCATION w
	inner join TB_ATTRIBUTE_SET tas on w.REVIEW_ID = @REVIEW_ID and tas.ATTRIBUTE_ID = w.ATTRIBUTE_ID and tas.SET_ID = @SET_ID
	-- added to check is coding tool is used in a visualisation 
	SELECT @NUM_VIS_MAPS = count(*) from TB_WEBDB_PUBLIC_SET wps
		inner join TB_REVIEW_SET rs on rs.REVIEW_SET_ID = wps.REVIEW_SET_ID
		where rs.SET_ID = @SET_ID

SET NOCOUNT OFF
GO

------------------------------------

