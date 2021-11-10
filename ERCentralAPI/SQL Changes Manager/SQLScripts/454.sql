USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ClusterGetXmlFiltered]    Script Date: 11/9/2021 12:07:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_ClusterGetXmlFiltered]
(
	@REVIEW_ID INT,
	@ITEM_ID_LIST NVARCHAR(max)
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       cast(Title as varchar(4000)) as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		CAST(abstract as varchar(max)) as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID


order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')


GO


CREATE OR ALTER procedure [dbo].[st_ClusterGetXmlAll]
(
	@REVIEW_ID INT
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       CAST(Title as varchar(4000)) as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		CAST(abstract as varchar(max)) as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')

GO

CREATE OR ALTER procedure [dbo].[st_ClusterGetXmlFilteredCode]
(
	@REVIEW_ID INT,
	@ATTRIBUTE_SET_ID_LIST NVARCHAR(max)
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       CAST(Title as varchar(4000)) as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		CAST(abstract as varchar(max)) as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'


order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')

GO




CREATE OR ALTER procedure [dbo].[st_ClusterGetXmlAllDocs]
(
	@REVIEW_ID INT
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       CAST(Title as varchar(4000)) as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		CAST(DOCUMENT_TEXT as varchar(max)) as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID
and TB_ITEM_REVIEW.IS_INCLUDED = 'true' and TB_ITEM_REVIEW.IS_DELETED != 'true'

order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')

GO

CREATE OR ALTER procedure [dbo].[st_ClusterGetXmlFilteredCodeDocs]
(
	@REVIEW_ID INT,
	@ATTRIBUTE_SET_ID_LIST NVARCHAR(max)
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       CAST(Title as varchar(4000)) as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		CAST(DOCUMENT_TEXT as varchar(max)) as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
INNER JOIN TB_ITEM_ATTRIBUTE ON TB_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN dbo.fn_Split_int(@ATTRIBUTE_SET_ID_LIST, ',') attribute_list ON attribute_list.value = TB_ATTRIBUTE_SET.ATTRIBUTE_SET_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
		INNER JOIN TB_REVIEW_SET ON TB_REVIEW_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_REVIEW_SET.REVIEW_ID = @REVIEW_ID
		inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED != 'true' AND TB_ITEM_REVIEW.IS_INCLUDED = 'TRUE'


order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')

GO


CREATE OR ALTER procedure [dbo].[st_ClusterGetXmlFilteredDocs]
(
	@REVIEW_ID INT,
	@ITEM_ID_LIST NVARCHAR(max)
)

As

select 1 as Tag, 
	null as PARENT, 
	tb_item.item_id as [document!1!id],
	null as [title!2],
	null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID

UNION ALL

SELECT 2 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       CAST(Title as varchar(4000)) as [title!2],
		null as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID

union all

SELECT 3 as Tag, 1 as Parent,
       tb_item.item_id as [Document!1!id],
       null as [title!2],
		CAST(DOCUMENT_TEXT as varchar(max)) as [snippet!3]
from tb_item
inner join tb_item_review on tb_item_review.item_id = tb_item.item_id
inner join DBO.fn_split_int(@ITEM_ID_LIST, ',') ItemList on ItemList.value = TB_ITEM.ITEM_ID
inner join TB_ITEM_DOCUMENT on TB_ITEM_DOCUMENT.ITEM_ID = TB_ITEM.ITEM_ID
where tb_item_review.review_id = @REVIEW_ID


order by [Document!1!id], [title!2], [snippet!3]
FOR XML explicit, root ('searchresult')

GO




GO
