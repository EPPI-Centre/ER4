USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonComplete]    Script Date: 31/05/2019 10:08:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ComparisonComplete]
(
	@COMPARISON_ID INT,
	@CONTACT_ID INT,
	@WHICH_REVIEWERS NVARCHAR(20),
	@IS_LOCKED bit = NULL
	--@RECORDS_AFFECTED INT OUTPUT
)

As

--SET NOCOUNT ON

DECLARE @T1 TABLE --item_attribute for R1, R1 and R2 are relative to the sproc, could be any couple from 
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)
DECLARE @T2 TABLE --item_attribute for R2
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID) 
	)
DECLARE @T1a2 table (ITEM_ID bigint primary key)--snapshot agreements 1 v 2
--declare @c1 int
--declare @c2 int


--If @WHICH_REVIEWERS = 'Complete1vs2'
--	BEGIN
--		select @c1 = CONTACT_ID1, @c2 = CONTACT_ID2 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END
--ELSE If @WHICH_REVIEWERS = 'Complete1vs3'
--	BEGIN
--		select @c1 = CONTACT_ID1, @c2 = CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END
--ELSE  --Complete2vs3
--	BEGIN
--		select @c1 = CONTACT_ID2, @c2 = CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END

insert into @T1
select distinct ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
	inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID 
												and  cia.CONTACT_ID = 
													CASE  @WHICH_REVIEWERS
														WHEN 'Complete2vs3' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID1
													END 
												and c.COMPARISON_ID = @COMPARISON_ID

insert into @T2
select distinct ITEM_ID, ATTRIBUTE_ID from tb_COMPARISON c
inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID 
												and cia.CONTACT_ID = 
													CASE  @WHICH_REVIEWERS
														WHEN 'Complete1vs2' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID3
													END 
												and c.COMPARISON_ID = @COMPARISON_ID



insert into @T1a2 --add all agreements; see st_ComparisonStats to understand how this works
Select distinct t1.ITEM_ID from @T1 t1 
	inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID
	except
select distinct(t1.item_id) from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID
	left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID
	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID
	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

--select * from @T1
--select * from @T2
--select * from @T1a2

Delete from @T1a2 where ITEM_ID in --remove all items that are already completed
(
	select tis.ITEM_ID from TB_ITEM_SET tis inner join 
		tb_COMPARISON c on tis.SET_ID = c.SET_ID and COMPARISON_ID = @COMPARISON_ID
		Inner join @T1a2 t1 on tis.ITEM_ID = t1.ITEM_ID and tis.IS_COMPLETED = 1
)

--select * from @T1a2

IF @IS_LOCKED is null
BEGIN
	Update TB_ITEM_SET set IS_COMPLETED = 1
		where CONTACT_ID = @CONTACT_ID
			and ITEM_SET_ID in
				(	
					select ITEM_SET_ID from TB_ITEM_SET tis 
					inner join tb_COMPARISON c on tis.SET_ID = c.SET_ID and c.COMPARISON_ID = @COMPARISON_ID
					inner join @T1a2 t on tis.ITEM_ID = t.ITEM_ID
				)
END
ELSE
BEGIN
	Update TB_ITEM_SET set IS_COMPLETED = 1 , IS_LOCKED = @IS_LOCKED
		where CONTACT_ID = @CONTACT_ID
			and ITEM_SET_ID in
				(	
					select ITEM_SET_ID from TB_ITEM_SET tis 
					inner join tb_COMPARISON c on tis.SET_ID = c.SET_ID and c.COMPARISON_ID = @COMPARISON_ID
					inner join @T1a2 t on tis.ITEM_ID = t.ITEM_ID
				)
END








--INSERT INTO @TT(ITEM_ID, ATTRIBUTE_ID)
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--			CASE  @WHICH_REVIEWERS
--				WHEN 'Complete2vs3' THEN tb_COMPARISON.CONTACT_ID2
--				ELSE tb_COMPARISON.CONTACT_ID1
--			END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	INTERSECT --  ********** AGREEMENT - THEREFORE INTERSECT
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--		CASE @WHICH_REVIEWERS
--			WHEN 'Complete1vs2' THEN TB_COMPARISON.CONTACT_ID2
--			ELSE tb_COMPARISON.CONTACT_ID3
--		END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	ORDER BY ITEM_ID, ATTRIBUTE_ID

--UPDATE TB_ITEM_SET
--SET IS_COMPLETED = 'TRUE' WHERE TB_ITEM_SET.CONTACT_ID = @CONTACT_ID AND TB_ITEM_SET.ITEM_SET_ID IN
--(
--	SELECT ITEM_SET_ID FROM TB_ITEM_SET TB_IS
--	INNER JOIN tb_COMPARISON_ITEM_ATTRIBUTE ON
--		tb_COMPARISON_ITEM_ATTRIBUTE.SET_ID = TB_IS.SET_ID AND
--		tb_COMPARISON_ITEM_ATTRIBUTE.ITEM_ID = TB_IS.ITEM_ID AND
--		tb_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID = TB_IS.CONTACT_ID AND
--		tb_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID = @COMPARISON_ID
--	WHERE TB_IS.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
--)


SELECT @@ROWCOUNT
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonScreeningComplete]    Script Date: 31/05/2019 11:04:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ComparisonScreeningComplete]
(
	@COMPARISON_ID INT,
	@CONTACT_ID INT,
	@WHICH_REVIEWERS NVARCHAR(20),
	@IS_LOCKED bit = NULL
	--@RECORDS_AFFECTED INT OUTPUT
)

As

--SET NOCOUNT ON

DECLARE @T1 TABLE --item_attribute for R1, R1 and R2 are relative to the sproc, could be any couple from 
	(
	  ITEM_ID BIGINT,
	  [STATE] char(1),
		  PRIMARY KEY (ITEM_ID, [STATE]) 
	)
DECLARE @T2 TABLE --item_attribute for R2
	(
	  ITEM_ID BIGINT,
	  [STATE] char(1),
		  PRIMARY KEY (ITEM_ID, [STATE]) 
	)
DECLARE @T1a2 table (ITEM_ID bigint primary key)--snapshot agreements 1 v 2
--declare @c1 int
--declare @c2 int


--If @WHICH_REVIEWERS = 'Complete1vs2'
--	BEGIN
--		select @c1 = CONTACT_ID1, @c2 = CONTACT_ID2 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END
--ELSE If @WHICH_REVIEWERS = 'Complete1vs3'
--	BEGIN
--		select @c1 = CONTACT_ID1, @c2 = CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END
--ELSE  --Complete2vs3
--	BEGIN
--		select @c1 = CONTACT_ID2, @c2 = CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID
--	END

insert into @T1
SELECT sub.ITEM_ID 
		 ,CASE 
			when [is incl] > 0 and [is ex] > 0 then 'B'
			when [is incl] > 0 then 'I'
			WHEN [is ex] > 0 then 'E'
			ELSE Null
		END
		from
		(select inc.ITEM_ID,  Sum(CASE inc.IS_INCLUDED when 1 then 1 else 0 end) [is incl]
			, Sum(CASE inc.IS_INCLUDED when 0 then 1 else 0 end) [is ex]
			from 
			tb_COMPARISON c
			left join tb_COMPARISON_ITEM_ATTRIBUTE inc on c.COMPARISON_ID = inc.COMPARISON_ID 
												and  inc.CONTACT_ID = 
													CASE  @WHICH_REVIEWERS
														WHEN 'Complete2vs3Sc' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID1
													END 
												and c.COMPARISON_ID = @COMPARISON_ID
			group by inc.ITEM_ID
		) sub
		where ITEM_ID is not null

insert into @T2
SELECT sub.ITEM_ID 
		 ,CASE 
			when [is incl] > 0 and [is ex] > 0 then 'B'
			when [is incl] > 0 then 'I'
			WHEN [is ex] > 0 then 'E'
			ELSE Null
		END
		from
		(select inc.ITEM_ID,  Sum(CASE inc.IS_INCLUDED when 1 then 1 else 0 end) [is incl]
			, Sum(CASE inc.IS_INCLUDED when 0 then 1 else 0 end) [is ex]
			from 
			tb_COMPARISON c
			left join tb_COMPARISON_ITEM_ATTRIBUTE inc on c.COMPARISON_ID = inc.COMPARISON_ID 
												and  inc.CONTACT_ID = 
													CASE  @WHICH_REVIEWERS
														WHEN 'Complete1vs2Sc' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID3
													END 
												and c.COMPARISON_ID = @COMPARISON_ID
			group by inc.ITEM_ID
		) sub
		where ITEM_ID is not null



insert into @T1a2
Select distinct t1.ITEM_ID from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.STATE = t2.STATE
	

--select * from @T1
--select * from @T2
--select * from @T1a2

Delete from @T1a2 where ITEM_ID in --remove all items that are already completed
(
	select tis.ITEM_ID from TB_ITEM_SET tis inner join 
		tb_COMPARISON c on tis.SET_ID = c.SET_ID and COMPARISON_ID = @COMPARISON_ID
		Inner join @T1a2 t1 on tis.ITEM_ID = t1.ITEM_ID and tis.IS_COMPLETED = 1
)

--select * from @T1a2

IF @IS_LOCKED is null
BEGIN
	Update TB_ITEM_SET set IS_COMPLETED = 1 
		where CONTACT_ID = @CONTACT_ID
			and ITEM_SET_ID in
				(	
					select ITEM_SET_ID from TB_ITEM_SET tis 
					inner join tb_COMPARISON c on tis.SET_ID = c.SET_ID and c.COMPARISON_ID = @COMPARISON_ID
					inner join @T1a2 t on tis.ITEM_ID = t.ITEM_ID
				)
	SELECT @@ROWCOUNT
END
ELSE
BEGIN
	Update TB_ITEM_SET set IS_COMPLETED = 1, IS_LOCKED = @IS_LOCKED 
		where CONTACT_ID = @CONTACT_ID
			and ITEM_SET_ID in
				(	
					select ITEM_SET_ID from TB_ITEM_SET tis 
					inner join tb_COMPARISON c on tis.SET_ID = c.SET_ID and c.COMPARISON_ID = @COMPARISON_ID
					inner join @T1a2 t on tis.ITEM_ID = t.ITEM_ID
				)
	SELECT @@ROWCOUNT
END
GO