﻿USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemCreate]    Script Date: 21/02/2021 14:18:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemCreate]
(
	@ITEM_ID BIGINT OUTPUT
,	@TITLE NVARCHAR(4000) = NULL
,	@TYPE_ID TINYINT
,	@PARENT_TITLE NVARCHAR(4000)
,	@SHORT_TITLE NVARCHAR(70)
,	@DATE_CREATED DATETIME = NULL
,	@CREATED_BY NVARCHAR(50) = NULL
,	@DATE_EDITED DATETIME = NULL
,	@EDITED_BY NVARCHAR(50) = NULL
,	@YEAR NCHAR(4) = NULL
,	@MONTH NVARCHAR(10) = NULL
,	@STANDARD_NUMBER NVARCHAR(255) = NULL
,	@CITY NVARCHAR(100) = NULL
,	@COUNTRY NVARCHAR(100) = NULL
,	@PUBLISHER NVARCHAR(1000) = NULL
,	@INSTITUTION NVARCHAR(1000) = NULL
,	@VOLUME NVARCHAR(56) = NULL
,	@PAGES NVARCHAR(50) = NULL
,	@EDITION NVARCHAR(200) = NULL
,	@ISSUE NVARCHAR(100) = NULL
,	@IS_LOCAL BIT = NULL
,	@AVAILABILITY NVARCHAR(255) = NULL
,	@URL NVARCHAR(500) = NULL
,	@COMMENTS NVARCHAR(MAX) = NULL
,	@ABSTRACT NVARCHAR(MAX) = NULL
,	@REVIEW_ID INT
,	@IS_INCLUDED BIT
,	@DOI NVARCHAR(500) = NULL
,	@KEYWORDS NVARCHAR(MAX) = NULL
,	@OLD_ITEM_ID NVARCHAR(50) = NULL
)

As

SET NOCOUNT ON

INSERT INTO TB_ITEM (TITLE, [TYPE_ID], PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
	[YEAR],[MONTH],STANDARD_NUMBER,CITY,COUNTRY,PUBLISHER,INSTITUTION,VOLUME,PAGES,EDITION,ISSUE,IS_LOCAL,AVAILABILITY,URL,
	COMMENTS,ABSTRACT,DOI,KEYWORDS,OLD_ITEM_ID)
VALUES (@TITLE,@TYPE_ID,@PARENT_TITLE,@SHORT_TITLE,@DATE_CREATED,@CREATED_BY,@DATE_EDITED,@EDITED_BY,@YEAR,
	@MONTH,@STANDARD_NUMBER,@CITY,@COUNTRY,@PUBLISHER,@INSTITUTION,@VOLUME,@PAGES,@EDITION,@ISSUE,@IS_LOCAL,@AVAILABILITY,@URL,
	@COMMENTS,@ABSTRACT,@DOI,@KEYWORDS,@OLD_ITEM_ID)

SET  @ITEM_ID = @@IDENTITY

INSERT INTO TB_ITEM_REVIEW (IS_DELETED, IS_INCLUDED, ITEM_ID, MASTER_ITEM_ID, REVIEW_ID)
VALUES ('FALSE', @IS_INCLUDED, @ITEM_ID, NULL, @REVIEW_ID)

SET NOCOUNT OFF
GO
