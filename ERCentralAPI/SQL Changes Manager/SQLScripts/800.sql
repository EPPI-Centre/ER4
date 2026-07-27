USE [Reviewer]
GO

/****** Object:  UserDefinedFunction [dbo].[fn_IsAttributeAnAncestor]    Script Date: 21/05/2026 10:08:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE OR ALTER FUNCTION [dbo].[fn_IsAttributeAnAncestor] 
(
	-- this function will return true is the ancestor attribute_id is directly "related" to the descendant attribute_id
	@ANCS_ATTRIBUTE_ID int,  -- this is the code higher up in the tool
	@DESC_ATTRIBUTE_ID int,  -- this is the code lower down in the tool
	@SET_ID int
)
RETURNS bit
AS
BEGIN
	-- Declare the return variable here	
	declare @ancestors table (anc_id bigint, distance int)
	declare @count int = 1
	declare @numberMatches int

	declare @currentAncestor bigint = (select PARENT_ATTRIBUTE_ID from TB_ATTRIBUTE_SET 
	where SET_ID = @SET_ID and ATTRIBUTE_ID = @DESC_ATTRIBUTE_ID and PARENT_ATTRIBUTE_ID != 0);

	if @currentAncestor is not null 
	 Insert into @ancestors select @currentAncestor, @count 

	while @count < 100 and @currentAncestor is not null
	begin 
	 set @count = @count +1;
	 set @currentAncestor = (select PARENT_ATTRIBUTE_ID from TB_ATTRIBUTE_SET 
	 where SET_ID = @SET_ID and ATTRIBUTE_ID = @currentAncestor and PARENT_ATTRIBUTE_ID != 0);
	 if @currentAncestor is not null 
	  Insert into @ancestors select @currentAncestor, @count
	end 

	set @numberMatches = (select count(*) from @ancestors where anc_id = @ANCS_ATTRIBUTE_ID)
	if @numberMatches > 0 return 1
	return 0

END
GO

-------------------------------------------------

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_AttributeSetDeleteWarning]    Script Date: 21/05/2026 10:10:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER     procedure [dbo].[st_AttributeSetDeleteWarning]
(
	@ATTRIBUTE_SET_ID BIGINT,
	@SET_ID INT,
	@NUM_ITEMS int OUTPUT,
	@NUM_OUTCOMES int OUTPUT,
	@NUM_ALLOCATIONS int = 0 OUTPUT,
	@NUM_VIS_MAPS int = 0 OUTPUT,
	@REVIEW_ID INT
)

As

SET NOCOUNT ON


-- if the code is part of a coding tool that is in a visualisation but is not explicitly listed in a map then @NUM_VIS_MAPS = 1
-- if the code is part of a coding tool that is in a visualisation and is explicitly listed in a map then @NUM_VIS_MAPS = 2
-- if the code is part of a coding tool that is in a visualisation and is an ancestor of a code explicitly listed in a map then @NUM_VIS_MAPS = 3


	SELECT @NUM_ITEMS = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
		INNER JOIN TB_SET ON TB_SET.SET_ID = TB_ITEM_SET.SET_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_SET.SET_ID
			AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
			AND TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
	SELECT @NUM_OUTCOMES = COUNT(distinct tis.ITEM_ID) from TB_ITEM_OUTCOME_ATTRIBUTE ioa
		INNER JOIN TB_ITEM_OUTCOME tio on ioa.OUTCOME_ID = tio.OUTCOME_ID
		inner join TB_ATTRIBUTE_SET tas on ioa.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and tas.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID
		inner join TB_ITEM_SET tis on tio.ITEM_SET_ID = tis.ITEM_SET_ID and tas.SET_ID = tis.SET_ID

	SELECT @NUM_ALLOCATIONS = count(*) from TB_WORK_ALLOCATION w
	inner join TB_ATTRIBUTE_SET tas on w.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and tas.ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID and w.REVIEW_ID = @REVIEW_ID
	
	-- added to check if the code is used in a visualisation
	declare @setId int
	set @setId = (select SET_ID from TB_ATTRIBUTE_SET where ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID)

	set @NUM_VIS_MAPS = 0
	declare @numVis int = 0
	declare @numVisMaps int = 0
	set @numVis = (select count(*) from TB_WEBDB_PUBLIC_SET wps
		inner join TB_REVIEW_SET rs on rs.REVIEW_SET_ID = wps.REVIEW_SET_ID
		where rs.SET_ID = @setId)
	if @numVis > 0
	begin
		-- we know that the code is in a coding tool that is used in a visualisation 

		-- added by JB 14/05/26 to check if the code to delete is an ancestor of a code explicitly listed in a map
		-- this is used in the visualisation setup page
		declare @tv_map_attributes table (tv_pub_attr_id int, tv_pub_set_id int, tv_attr_id int, tv_review_set_id int, tv_set_id int)
		insert into @tv_map_attributes (tv_pub_attr_id, tv_pub_set_id)
			select COLUMNS_PUBLIC_ATTRIBUTE_ID, COLUMNS_PUBLIC_SET_ID from TB_WEBDB_MAP where WEBDB_ID in 
			(select WEBDB_ID from TB_WEBDB where REVIEW_ID = @REVIEW_ID)
		insert into @tv_map_attributes (tv_pub_attr_id, tv_pub_set_id)
			select ROWS_PUBLIC_ATTRIBUTE_ID, ROWS_PUBLIC_SET_ID from TB_WEBDB_MAP where WEBDB_ID in 
			(select WEBDB_ID from TB_WEBDB where REVIEW_ID = @REVIEW_ID)
		insert into @tv_map_attributes (tv_pub_attr_id, tv_pub_set_id)
			select SEGMENTS_PUBLIC_ATTRIBUTE_ID, SEGMENTS_PUBLIC_SET_ID from TB_WEBDB_MAP where WEBDB_ID in 
			(select WEBDB_ID from TB_WEBDB where REVIEW_ID = @REVIEW_ID)

		update @tv_map_attributes
		set tv_attr_id = ATTRIBUTE_Id
		from TB_WEBDB_PUBLIC_ATTRIBUTE
		where WEBDB_PUBLIC_ATTRIBUTE_ID = tv_pub_attr_id
		update @tv_map_attributes
		set tv_review_set_id = REVIEW_SET_ID
		from TB_WEBDB_PUBLIC_SET
		where WEBDB_PUBLIC_SET_ID = tv_pub_set_id
		update @tv_map_attributes
		set tv_set_id = SET_ID
		from TB_REVIEW_SET
		where REVIEW_SET_ID = tv_review_set_id

		declare @attributeId int
		set @attributeId = (select ATTRIBUTE_ID from TB_ATTRIBUTE_SET where ATTRIBUTE_SET_ID = @ATTRIBUTE_SET_ID)

		set @numVisMaps = (select count(*) from @tv_map_attributes where dbo.fn_IsAttributeAnAncestor(@attributeId, tv_attr_id, tv_set_id) = 1)
		set @NUM_VIS_MAPS = 1
		if @numVisMaps > 0 
		begin
			set @NUM_VIS_MAPS = 3
		end


		-- We also want to check if the attribute is explicitly mentioned in a pre-configured map
		-- that last check doesn't look for that and this check is probably more important
		
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
		-- if the code is part of a coding tool that is in a visualisation and is an ancestor of a code explicitly listed in a map then @NUM_VIS_MAPS = 3
		
		if @numVis > 0 and @numVisMaps > 0
		begin
			set @NUM_VIS_MAPS = 2
		end
	end

SET NOCOUNT OFF
GO

----------------------------------

USE [Reviewer]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewSetDeleteWarning]    Script Date: 21/05/2026 10:10:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER   procedure [dbo].[st_ReviewSetDeleteWarning]
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

	declare @num_vis_maps_tmp int = 0

	SELECT @NUM_ITEMS = COUNT(DISTINCT TB_ITEM_SET.ITEM_ID) FROM TB_ITEM_SET
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_SET.ITEM_ID
			AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		WHERE TB_ITEM_SET.SET_ID = @SET_ID
	Select @NUM_ALLOCATIONS = count(*) from TB_WORK_ALLOCATION w
	inner join TB_ATTRIBUTE_SET tas on w.REVIEW_ID = @REVIEW_ID and tas.ATTRIBUTE_ID = w.ATTRIBUTE_ID and tas.SET_ID = @SET_ID
	-- added to check is coding tool is used in a visualisation 
	set @num_vis_maps_tmp = (SELECT count(*) from TB_WEBDB_PUBLIC_SET wps
		inner join TB_REVIEW_SET rs on rs.REVIEW_SET_ID = wps.REVIEW_SET_ID
		where rs.SET_ID = @SET_ID)
	
	-- added by JB 14/05/26 to check if the the coding tool was used in a preconfigured map
	-- if the coding tool that is in a visualisation but not in a map then @NUM_VIS_MAPS = 1
	-- if the coding tool that is in a visualisation and is in a map then @NUM_VIS_MAPS = 2
	set @NUM_VIS_MAPS = @num_vis_maps_tmp
	if @NUM_VIS_MAPS > 0 
	begin
	    set @NUM_VIS_MAPS = 1
		-- we know the tool is in a visualisation. Lets see if that tool is a preconfigured map
		declare @review_set_id int
		declare @tv_public_set_ids table (tv_webdb_public_set_id int)
		
		set @review_set_id = (select REVIEW_SET_ID from TB_REVIEW_SET where SET_ID = @SET_ID and REVIEW_ID = @REVIEW_ID)
		
		insert into @tv_public_set_ids (tv_webdb_public_set_id)
		select WEBDB_PUBLIC_SET_ID from TB_WEBDB_PUBLIC_SET where REVIEW_SET_ID = @review_set_id

		-- see if any of those @tv_public_set_ids are in TB_WEBDB_MAP
		declare @instances_used int
		
		set @instances_used = (select count(*) from TB_WEBDB_MAP wm
			inner join @tv_public_set_ids wpai on 
			wpai.tv_webdb_public_set_id = wm.COLUMNS_PUBLIC_SET_ID
			or wpai.tv_webdb_public_set_id = wm.ROWS_PUBLIC_SET_ID 
			or wpai.tv_webdb_public_set_id = wm.SEGMENTS_PUBLIC_SET_ID)	
		
		if @instances_used > 0
		begin
			set @NUM_VIS_MAPS = 2
		end

	end

SET NOCOUNT OFF
GO

---------------------------------







