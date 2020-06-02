USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupAddAddionalItem]    Script Date: 19/05/2020 20:48:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sergio - JT modified to be used automatically
-- Create date: <Create Date,,>
-- Description:	Add Item to an existing group, @MasterID is the destination master Item_ID
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_ItemDuplicateGroupAddAddionalItem]
	-- Add the parameters for the stored procedure here
	@MasterID int,
	@RevID int,
	@NewDuplicateItemID bigint,
	@Score float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @GroupID int
	--get the group ID
	select @GroupID = G.ITEM_DUPLICATE_GROUP_ID from TB_ITEM_DUPLICATE_GROUP G
		inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS GM on G.MASTER_MEMBER_ID = GM.GROUP_MEMBER_ID and G.REVIEW_ID = @RevID
		inner join TB_ITEM_REVIEW IR on IR.ITEM_REVIEW_ID = GM.ITEM_REVIEW_ID and IR.ITEM_ID = @MasterID
	if --if the item we are adding already belongs to the group, do nothing!
		(	
			(
				select COUNT(ITEM_ID) from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM inner join TB_ITEM_REVIEW IR 
				on GM.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID and IR.ITEM_ID = @NewDuplicateItemID and ITEM_DUPLICATE_GROUP_ID = @GroupID
			)
			> 0
		 ) Return;
	BEGIN TRY
	BEGIN TRANSACTION
	
	INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
	(
		ITEM_DUPLICATE_GROUP_ID
		,ITEM_REVIEW_ID
		,SCORE
		,IS_CHECKED
		,IS_DUPLICATE
	)
	SELECT @GroupID, ITEM_REVIEW_ID, @Score, 0, 0
	from TB_ITEM_REVIEW where REVIEW_ID = @RevID and ITEM_ID = @NewDuplicateItemID 
	
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
	END CATCH
END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGroupCreateNew]    Script Date: 19/05/2020 16:33:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Sergio wrote manual routine - JT modified for auto-creation of group>
-- Create date: 11/03/2011
-- Description:	adds a new duplicate group composed of master and one member
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_ItemDuplicateGroupCreateNew]
	-- Add the parameters for the stored procedure here
	@RevID int,
	@MasterID bigint,
	@MemberId bigint,
	@Score float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @group_id int;
    -- Insert statements for procedure here
	insert into TB_ITEM_DUPLICATE_GROUP (REVIEW_ID, ORIGINAL_ITEM_ID)
		Values (@RevID, @MasterID)
	set @group_id = @@IDENTITY
	--add the master and normal records in the members table
	INSERT into TB_ITEM_DUPLICATE_GROUP_MEMBERS
	(
		ITEM_DUPLICATE_GROUP_ID
		,ITEM_REVIEW_ID
		,SCORE
		,IS_CHECKED
		,IS_DUPLICATE
	)
	SELECT ITEM_DUPLICATE_GROUP_ID, ITEM_REVIEW_ID, @Score, 1, 0
	FROM TB_ITEM_DUPLICATE_GROUP DG
		INNER JOIN TB_ITEM_REVIEW IR  on IR.ITEM_ID = DG.ORIGINAL_ITEM_ID and IR.REVIEW_ID = @revID 
		and DG.ORIGINAL_ITEM_ID = @MasterID and ITEM_DUPLICATE_GROUP_ID = @group_id
	UNION
	SELECT @group_id, ITEM_REVIEW_ID, @Score, 0, 0
	from TB_ITEM_REVIEW where REVIEW_ID = @RevID and ITEM_ID = @MemberId 

	--update TB_ITEM_DUPLICATE_GROUP to set the MASTER_MEMBER_ID correctly (now that we have a line in TB_ITEM_DUPLICATE_GROUP_MEMBERS)
	UPDATE TB_ITEM_DUPLICATE_GROUP set MASTER_MEMBER_ID = a.GMI
	FROM (
		SELECT GROUP_MEMBER_ID GMI, IR.ITEM_ID from TB_ITEM_DUPLICATE_GROUP idg
			inner join TB_ITEM_DUPLICATE_GROUP_MEMBERS gm on gm.ITEM_DUPLICATE_GROUP_ID = idg.ITEM_DUPLICATE_GROUP_ID 
				and MASTER_MEMBER_ID is null and idg.ITEM_DUPLICATE_GROUP_ID = @group_id
			inner join TB_ITEM_REVIEW IR on gm.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID AND IR.REVIEW_ID = @revID and ITEM_ID = @MasterID
	) a  
	WHERE ORIGINAL_ITEM_ID = a.ITEM_ID and MASTER_MEMBER_ID is null


	/*
	-- NOT SURE WHETHER WE NEED THIS BIT??
	--mark relevant items as checked and not duplicates in other groups
	update TB_ITEM_DUPLICATE_GROUP_MEMBERS set IS_DUPLICATE =  0, IS_CHECKED = 0
	from TB_ITEM_DUPLICATE_GROUP_MEMBERS GM
		where ITEM_REVIEW_ID in 
		(
			select ITEM_REVIEW_ID from TB_ITEM_DUPLICATE_GROUP_MEMBERS where ITEM_DUPLICATE_GROUP_ID = @group_id
		) and GM.ITEM_DUPLICATE_GROUP_ID != @group_id
	 */
END








GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicatesGetCandidatesOnSearchText]    Script Date: 24/05/2020 15:26:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or ALTER     procedure [dbo].[st_ItemDuplicatesGetCandidatesOnSearchText]
(
      @REVIEW_ID INT
)
As

SET NOCOUNT ON

	select        I.ITEM_ID, I2.ITEM_ID ITEM_ID2, I.TITLE, I2.TITLE TITLE2,
				[dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
                   I.PARENT_TITLE, I.[YEAR], I.VOLUME, I.PAGES, I.ISSUE, I.ABSTRACT,
				  I.DOI, case when ise.item_id is null then 0 else 1 end as HAS_CODES,
				  case when (NOT DG.MASTER_MEMBER_ID IS NULL AND (idg.GROUP_MEMBER_ID = DG.MASTER_MEMBER_ID))
					then 1 else 0 end as IS_MASTER,

				 [dbo].fn_REBUILD_AUTHORS(I2.ITEM_ID, 0) as AUTHORS2,
                   I2.PARENT_TITLE PARENT_TITLE2, I2.[YEAR] YEAR2, I2.VOLUME VOLUME2, I2.PAGES PAGES2,
				   I2.ISSUE ISSUE2, I2.ABSTRACT ABSTRACT2,
				  I2.DOI DOI2,
				  case when ise2.item_id is null then 0 else 1 end as HAS_CODES2,
				case when (NOT DG2.MASTER_MEMBER_ID IS NULL AND (idg2.GROUP_MEMBER_ID = DG2.MASTER_MEMBER_ID))
				then 1 else 0 end as IS_MASTER2,

				i.SearchText, i2.SearchText
				  
	from TB_ITEM I
		INNER JOIN TB_ITEM I2 ON I2.SearchText = I.SearchText
		inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = i.ITEM_ID
		INNER JOIN TB_ITEM_REVIEW IR2 ON IR2.ITEM_ID = I2.ITEM_ID

		LEFT OUTER JOIN TB_ITEM_SET ISE ON ISE.ITEM_ID = I.ITEM_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS IDG ON IDG.ITEM_REVIEW_ID = IR.ITEM_REVIEW_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP DG ON DG.ITEM_DUPLICATE_GROUP_ID = IDG.ITEM_DUPLICATE_GROUP_ID

		LEFT OUTER JOIN TB_ITEM_SET ISE2 ON ISE2.ITEM_ID = I2.ITEM_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP_MEMBERS IDG2 ON IDG2.ITEM_REVIEW_ID = IR2.ITEM_REVIEW_ID
		LEFT OUTER JOIN TB_ITEM_DUPLICATE_GROUP DG2 ON DG2.ITEM_DUPLICATE_GROUP_ID = IDG2.ITEM_DUPLICATE_GROUP_ID

		where ir.IS_DELETED = 'False' and ir.REVIEW_ID = @REVIEW_ID 
			and ir2.IS_DELETED = 'False' and ir2.REVIEW_ID = @REVIEW_ID
			and i.ITEM_ID != I2.ITEM_ID and i.ITEM_ID < i2.ITEM_ID
			
			and (idg.GROUP_MEMBER_ID is null or (NOT DG.MASTER_MEMBER_ID IS NULL
				AND (idg.GROUP_MEMBER_ID = DG.MASTER_MEMBER_ID)))
			and (idg2.GROUP_MEMBER_ID is null or (NOT DG2.MASTER_MEMBER_ID IS NULL
				AND (idg2.GROUP_MEMBER_ID = DG2.MASTER_MEMBER_ID)))
		order by I.SearchText, IS_MASTER, IS_MASTER2
		
SET NOCOUNT OFF
GO



USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateGetTitles]    Script Date: 22/05/2020 17:08:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or ALTER procedure [dbo].[st_ItemDuplicateGetTitles]
(
      @REVIEW_ID INT
)
As

SET NOCOUNT ON
	SELECT TITLE, i.ITEM_ID FROM TB_ITEM I
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = I.ITEM_ID
		WHERE IR.REVIEW_ID = @REVIEW_ID AND IR.IS_DELETED = 'FALSE'
SET NOCOUNT OFF
GO


USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemDuplicateSaveShortSearchText]    Script Date: 19/05/2020 17:13:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER   procedure [dbo].[st_ItemDuplicateSaveShortSearchText]
(
      @ITEM_ID BIGINT,
	  @SearchText nvarchar(500)
)
As

SET NOCOUNT ON
	update TB_ITEM
		SET SearchText = @SearchText WHERE ITEM_ID = @ITEM_ID
SET NOCOUNT OFF
GO

