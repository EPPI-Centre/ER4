use [reviewer]

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_MAG_ITEM_PAPER_INSERT_TEMP')
BEGIN
DROP TABLE [dbo].[TB_MAG_ITEM_PAPER_INSERT_TEMP]
END
GO
CREATE TABLE dbo.TB_MAG_ITEM_PAPER_INSERT_TEMP
	(
	PaperId bigint NOT NULL,
	year int NULL,
	journal nvarchar(500) NULL,
	--authors nvarchar(MAX) NULL,
	volume nchar(4) NULL,
	issue nvarchar(100) NULL,
	pages nvarchar(50),
	title nvarchar(4000) NULL,
	doi nvarchar(500) NULL,
	SearchText nvarchar(500),
	GUID_JOB NVARCHAR(MAX)
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.TB_MAG_ITEM_PAPER_INSERT_TEMP SET (LOCK_ESCALATION = TABLE)
GO


IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP')
BEGIN
	DROP TABLE [dbo].[TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP]
END
GO
CREATE TABLE dbo.TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP
	(
	PaperId bigint NULL,
	GUID_JOB nvarchar(50) NULL,
	FIRST nvarchar(50) NULL,
	SECOND nvarchar(50) NULL,
	LAST nvarchar(50) NULL,
	RANK smallint NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP SET (LOCK_ESCALATION = TABLE)
GO



USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_MagItemPaperInsert]    Script Date: 01/02/2020 13:06:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Pull a limited number of MAG papers into tb_ITEM based on a list of IDs. e.g. user 'selected' list
-- =============================================
create or ALTER PROCEDURE [dbo].[st_MagItemPaperInsert] 
	-- Add the parameters for the stored procedure here
	@SOURCE_NAME nvarchar(255),
	@CONTACT_ID INT = 0,
	@REVIEW_ID INT = 0,
	@GUID_JOB nvarchar(50),
	@MAG_RELATED_RUN_ID INT = 0,
	@N_IMPORTED INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @SourceId int = 0

	insert into Reviewer.dbo.TB_SOURCE(SOURCE_NAME, REVIEW_ID, IS_DELETED, DATE_OF_SEARCH, 
		DATE_OF_IMPORT, SOURCE_DATABASE,  SEARCH_DESCRIPTION, SEARCH_STRING, NOTES, IMPORT_FILTER_ID)
	values(@SOURCE_NAME, @REVIEW_id, 'false', GETDATE(),
		GETDATE(), 'Microsoft Academic', 'Browsing items related to a given item', 'Browse', '', 0)

	set @SourceId = @@IDENTITY

	declare @ItemIds Table(ITEM_ID bigint, PaperId bigint)

	insert into Reviewer.dbo.TB_ITEM(TYPE_ID, TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, YEAR, STANDARD_NUMBER, CITY,
		COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, URL, OLD_ITEM_ID, ABSTRACT, DOI, SearchText)
	output INSERTED.ITEM_ID, cast(inserted.OLD_ITEM_ID as bigint) into @ItemIds
	SELECT 
		/*   we don't currently have publication type in MAG items (once MAKES is live we will)
		case
			when p.DocType = 'Book' then 2
			when p.DocType = 'BookChapter' then 3
			when p.DocType = 'Conference' then 5
			when p.DocType = 'Journal' then 14
			when p.DocType = 'Patent' then 1
			else 14
		end
		*/
		14, mipi.title, mipi.journal,
			(select top(1) LAST from TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP where GUID_JOB = @GUID_JOB and PaperId = mipi.PaperId and RANK = 1 ) + ' (' + cast(mipi.year as nvarchar) + ')',
			GETDATE(), @CONTACT_ID, mipi.Year, 
			'', '', '', '', '', mipi.volume, mipi.pages, '', mipi.issue, '', cast(mipi.PaperId as nvarchar), '', mipi.doi, mipi.SearchText
	 FROM  reviewer.dbo.TB_MAG_ITEM_PAPER_INSERT_TEMP mipi
	 where mipi.GUID_JOB = @GUID_JOB and not mipi.PaperId in 
		(select paperid from Reviewer.dbo.tb_ITEM_MAG_MATCH imm
			inner join Reviewer.dbo.TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			where ir.REVIEW_ID = @REVIEW_id and ir.IS_DELETED = 'false' and
				(imm.AutoMatchScore > 0.7 or (imm.ManualFalseMatch = 'true' or imm.ManualTrueMatch = 'true')))

	set @N_IMPORTED = @@ROWCOUNT

	if @N_IMPORTED > 0
	begin

		 insert into Reviewer.dbo.TB_ITEM_SOURCE(ITEM_ID, SOURCE_ID)
		 select ITEM_ID, @SourceId from @ItemIds

		 insert into Reviewer.dbo.TB_ITEM_REVIEW(ITEM_ID, REVIEW_ID, IS_DELETED, IS_INCLUDED)
		 select item_id, @REVIEW_id, 'false', 'true' from @ItemIds

		 insert into reviewer.dbo.tb_ITEM_MAG_MATCH(ITEM_ID, REVIEW_ID, PaperId, AutoMatchScore, ManualTrueMatch, ManualFalseMatch)
		 select item_id, @REVIEW_id, PaperId, 1, 'false', 'false' from @ItemIds

		 insert into Reviewer.dbo.TB_ITEM_AUTHOR(ITEM_ID, LAST, FIRST, SECOND, ROLE, RANK)
		 select ITEM_ID, LAST, FIRST, SECOND, 0, RANK FROM
			TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP auTemp INNER JOIN @ItemIds I on i.PaperId = auTemp.PaperId
			where auTemp.GUID_JOB = @GUID_JOB
	end

	IF @MAG_RELATED_RUN_ID > 0
	BEGIN
		update tb_MAG_RELATED_RUN
			set USER_STATUS = 'Imported'
			where MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
	END

	delete from TB_MAG_ITEM_PAPER_INSERT_TEMP where GUID_JOB = @GUID_JOB
	delete from TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP where GUID_JOB = @GUID_JOB
END
GO


/****** Object:  StoredProcedure [dbo].[st_MagItemMagRelatedPaperInsert]    Script Date: 01/02/2020 11:16:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		James
-- Create date: 
-- Description:	Pull MAG papers found in a related papers run into tb_ITEM
-- =============================================
create or alter PROCEDURE [dbo].[st_MagItemMagRelatedPaperInsert] 
	-- Add the parameters for the stored procedure here
	@MAG_RELATED_RUN_ID INT,
	@REVIEW_ID int = 0
--WITH RECOMPILE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select PaperId
	 FROM tb_MAG_RELATED_PAPERS mrp
		where mrp.REVIEW_ID = @REVIEW_ID and mrp.MAG_RELATED_RUN_ID = @MAG_RELATED_RUN_ID
	 
	 and not mrp.PaperId in 
		(select paperid from tb_ITEM_MAG_MATCH imm
			inner join TB_ITEM_REVIEW ir on ir.ITEM_ID = imm.ITEM_ID
			where ir.REVIEW_ID = @REVIEW_id and ir.IS_DELETED = 'false' and
				(imm.AutoMatchScore > 0.7 or (imm.ManualFalseMatch = 'true' or imm.ManualTrueMatch = 'true')))

	
END
go