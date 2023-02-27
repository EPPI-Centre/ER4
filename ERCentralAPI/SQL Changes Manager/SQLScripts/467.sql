USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagGetPaperIdsForFoSImport]    Script Date: 27/01/2022 11:15:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Get PaperIds to get field of study ids for
-- =============================================
ALTER   PROCEDURE [dbo].[st_MagGetPaperIdsForFoSImport] 
	-- Add the parameters for the stored procedure here
	
	@ITEM_IDS nvarchar(max)
,	@ATTRIBUTE_ID bigint
,	@REVIEW_ID int
,	@REVIEW_SET_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @FILTERED_IDS TABLE (
	ITEM_ID BIGINT PRIMARY KEY
)
	-- these already have attributes in that set so we filter them out
	IF @REVIEW_SET_ID > 0
	BEGIN
		insert into @FILTERED_IDS
		select iset.ITEM_ID from TB_ITEM_SET iset
		inner join TB_REVIEW_SET rs on rs.SET_ID = iset.SET_ID
		where rs.REVIEW_SET_ID = @REVIEW_SET_ID and iset.IS_COMPLETED = 'true' AND RS.REVIEW_ID = @REVIEW_ID
	END

    -- Insert statements for procedure here
	
	if @ATTRIBUTE_ID < 1
	begin
		if @ITEM_IDS = '' -- everything in the review
		begin
			select PaperId, imm.ITEM_ID from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			where ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false' AND
				(imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
				and not imm.ITEM_ID in (select ITEM_ID from @FILTERED_IDS)
		end
		else
		begin -- filtred by the list of item_ids
			select PaperId, imm.ITEM_ID from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			inner join dbo.fn_Split_int(@ITEM_IDS, ',') ids on ids.value = ir.ITEM_ID
			where ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false' AND
				(imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			and not imm.ITEM_ID in (select ITEM_ID from @FILTERED_IDS)
		end
	end
	else
	begin
		if @ITEM_IDS = '' -- everything in the review filtered by attribute id
		begin
			select PaperId, imm.ITEM_ID from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			inner join TB_ITEM_ATTRIBUTE ia on ia.ITEM_ID = imm.ITEM_ID and ia.ATTRIBUTE_ID = @ATTRIBUTE_ID
			inner join TB_ITEM_SET iset on iset.ITEM_SET_ID = ia.ITEM_SET_ID and iset.IS_COMPLETED = 'true'
			where ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 'false' AND
				(imm.AutoMatchScore >= 0.8 or imm.ManualTrueMatch = 'true') and (imm.ManualFalseMatch <> 'true' or imm.ManualFalseMatch is null)
			and not imm.ITEM_ID in (select ITEM_ID from @FILTERED_IDS)
		end
	end
		
END

GO