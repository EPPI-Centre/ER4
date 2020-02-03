USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchedPaperManualEdit]    Script Date: 02/02/2020 21:28:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_item_mag_match based on manual lookup
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagMatchedPaperManualEdit] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_ID BIGINT
,	@PaperId BIGINT
,	@ManualTrueMatch bit
,	@ManualFalseMatch bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	declare @chk int
	select @chk = count(*) from tb_ITEM_MAG_MATCH
		where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId

	if @chk = 0
	begin
		insert into tb_ITEM_MAG_MATCH(REVIEW_ID, ITEM_ID, PaperId, ManualTrueMatch, ManualFalseMatch, AutoMatchScore)
		values (@REVIEW_ID, @ITEM_ID, @PaperId, @ManualTrueMatch, @ManualFalseMatch, 1)
	end
	else
	begin
		update tb_ITEM_MAG_MATCH
			set ManualTrueMatch = @ManualTrueMatch,
				ManualFalseMatch = @ManualFalseMatch
			where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId
	end
END
GO

USE Reviewer
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchItemsGetIdList]    Script Date: 02/02/2020 22:58:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 12/07/2019
-- Description:	Match EPPI-Reviewer TB_ITEM records to MAG Papers
-- =============================================
create or alter   PROCEDURE [dbo].[st_MagMatchItemsGetIdList] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ATTRIBUTE_ID BIGINT = 0
AS
BEGIN

if @ATTRIBUTE_ID > 0
begin
	select distinct tir.ITEM_ID
		from Reviewer.dbo.TB_ITEM_REVIEW tir 
		inner join Reviewer.dbo.TB_ITEM_ATTRIBUTE tia on tia.ITEM_ID = tir.ITEM_ID and tia.ATTRIBUTE_ID = @ATTRIBUTE_ID
		inner join Reviewer.dbo.TB_ITEM_SET tis on tis.ITEM_SET_ID = tia.ITEM_ATTRIBUTE_ID and tis.IS_COMPLETED = 'true'
		where tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
			and not tir.ITEM_ID IN (SELECT ITEM_ID FROM tb_ITEM_MAG_MATCH IMM
				WHERE IMM.ITEM_ID = TIR.ITEM_ID AND IMM.REVIEW_ID = @REVIEW_ID)
end
else
begin
	select distinct tir.ITEM_ID
		from Reviewer.dbo.TB_ITEM_REVIEW tir where tir.REVIEW_ID = @REVIEW_ID and tir.IS_DELETED = 'false'
		and not tir.ITEM_ID IN (SELECT ITEM_ID FROM tb_ITEM_MAG_MATCH IMM
				WHERE IMM.ITEM_ID = TIR.ITEM_ID AND IMM.REVIEW_ID = @REVIEW_ID)
end

END
go

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagMatchedPapersInsert]    Script Date: 03/02/2020 12:32:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Add record to tb_item_mag_match based on manual lookup
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagMatchedPapersInsert] 
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_ID BIGINT
,	@PaperId bigint
,	@AutoMatchScore float
,	@ManualTrueMatch bit = null
,	@ManualFalseMatch bit = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	declare @chk int
	select @chk = count(*) from tb_ITEM_MAG_MATCH
		where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId

	if @chk = 0
	begin
		insert into tb_ITEM_MAG_MATCH(REVIEW_ID, ITEM_ID, PaperId, ManualTrueMatch, ManualFalseMatch, AutoMatchScore)
		values (@REVIEW_ID, @ITEM_ID, @PaperId, @ManualTrueMatch, @ManualFalseMatch, @AutoMatchScore)
	end
	else
	begin
		update Reviewer.dbo.tb_ITEM_MAG_MATCH
			set ManualTrueMatch = @ManualTrueMatch,
				ManualFalseMatch = @ManualFalseMatch
			where REVIEW_ID = @REVIEW_ID and ITEM_ID = @ITEM_ID and PaperId = @PaperId
	end
END
go