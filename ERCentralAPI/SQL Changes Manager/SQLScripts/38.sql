--Sergio: Account for Arms in Comparisons.

USE [Reviewer]
GO


/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.tb_COMPARISON_ITEM_ATTRIBUTE ADD
	ITEM_ARM_ID bigint NULL
GO
ALTER TABLE dbo.tb_COMPARISON_ITEM_ATTRIBUTE SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonInsert]    Script Date: 18/09/2018 09:39:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ComparisonInsert]
(
	@REVIEW_ID INT,
	@IN_GROUP_ATTRIBUTE_ID BIGINT,
	@SET_ID INT,
	@COMPARISON_DATE DATE,
	@CONTACT_ID1 INT = NULL,
	@CONTACT_ID2 INT = NULL,
	@CONTACT_ID3 INT = NULL,
	@NEW_COMPARISON_ID INT OUTPUT,
	@Is_Screening bit OUTPUT
)

As

SET NOCOUNT ON
	
	set @Is_Screening = (Select CASE when SET_TYPE_ID = 5 then 1 else 0 end from TB_SET where SET_ID = @SET_ID)
	INSERT INTO tb_COMPARISON (REVIEW_ID, IN_GROUP_ATTRIBUTE_ID, SET_ID,
		COMPARISON_DATE, CONTACT_ID1, CONTACT_ID2, CONTACT_ID3, IS_SCREENING)
	VALUES (@REVIEW_ID, @IN_GROUP_ATTRIBUTE_ID, @SET_ID,
		@COMPARISON_DATE, @CONTACT_ID1, @CONTACT_ID2, @CONTACT_ID3, @Is_Screening)
	
	SET @NEW_COMPARISON_ID = @@IDENTITY
	
	IF (@IN_GROUP_ATTRIBUTE_ID IS NULL OR @IN_GROUP_ATTRIBUTE_ID = -1)
	BEGIN
	
		INSERT INTO tb_COMPARISON_ITEM_ATTRIBUTE (COMPARISON_ID, ITEM_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT, CONTACT_ID, SET_ID, IS_INCLUDED, ITEM_ARM_ID)
		SELECT DISTINCT @NEW_COMPARISON_ID, ia.ITEM_ID, ia.ATTRIBUTE_ID, 
			ia.ADDITIONAL_TEXT, tis.CONTACT_ID, tis.SET_ID
			, CASE 
				WHEN @Is_Screening != 1 then NULL
				WHEN tas.ATTRIBUTE_TYPE_ID = 10 then 1
				ELSE 0
			 END
			, ITEM_ARM_ID
			FROM TB_ITEM_ATTRIBUTE ia
			INNER JOIN TB_ITEM_SET tis ON tis.ITEM_SET_ID = ia.ITEM_SET_ID
				AND tis.SET_ID = @SET_ID
				AND tis.IS_COMPLETED = 'FALSE'
				AND ((@CONTACT_ID1 IS NULL OR (tis.CONTACT_ID = @CONTACT_ID1))
				OR (@CONTACT_ID2 IS NULL OR (tis.CONTACT_ID = @CONTACT_ID2))
				OR (@CONTACT_ID3 IS NULL OR (tis.CONTACT_ID = @CONTACT_ID3)))
			INNER JOIN TB_ITEM_REVIEW ir ON ir.ITEM_ID = ia.ITEM_ID
				AND ir.REVIEW_ID = @REVIEW_ID
				AND ir.IS_DELETED = 'FALSE'
			INNER JOIN TB_ATTRIBUTE_SET tas ON ia.ATTRIBUTE_ID =tas.ATTRIBUTE_ID and tas.SET_ID = @SET_ID
	END
	ELSE
	BEGIN
		INSERT INTO tb_COMPARISON_ITEM_ATTRIBUTE (COMPARISON_ID, ITEM_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT, CONTACT_ID, SET_ID, IS_INCLUDED, ITEM_ARM_ID)
		SELECT DISTINCT @NEW_COMPARISON_ID, ia.ITEM_ID, ia.ATTRIBUTE_ID, 
			ia.ADDITIONAL_TEXT, tis.CONTACT_ID, tis.SET_ID
			, CASE 
				WHEN @Is_Screening != 1 then NULL
				WHEN tas.ATTRIBUTE_TYPE_ID = 10 then 1
				ELSE 0
			 END
			, ia.ITEM_ARM_ID
			FROM TB_ITEM_ATTRIBUTE ia
			INNER JOIN TB_ITEM_SET tis ON tis.ITEM_SET_ID = ia.ITEM_SET_ID
				AND tis.SET_ID = @SET_ID
				AND tis.IS_COMPLETED = 'FALSE'
				AND ((@CONTACT_ID1 IS NULL OR (tis.CONTACT_ID = @CONTACT_ID1))
				OR (@CONTACT_ID2 IS NULL OR (tis.CONTACT_ID = @CONTACT_ID2))
				OR (@CONTACT_ID3 IS NULL OR (tis.CONTACT_ID = @CONTACT_ID3)))
			INNER JOIN TB_ITEM_ATTRIBUTE IA_FILTER ON IA_FILTER.ITEM_ID = ia.ITEM_ID
				AND IA_FILTER.ATTRIBUTE_ID = @IN_GROUP_ATTRIBUTE_ID
				INNER JOIN TB_ITEM_SET IA_FILTER_ITEM_SET ON IA_FILTER_ITEM_SET.ITEM_SET_ID = IA_FILTER.ITEM_SET_ID
				AND IA_FILTER_ITEM_SET.IS_COMPLETED = 'TRUE'
			INNER JOIN TB_ITEM_REVIEW ir ON ir.ITEM_ID = ia.ITEM_ID
				AND ir.REVIEW_ID = @REVIEW_ID
				AND ir.IS_DELETED = 'FALSE'
			Inner Join TB_ATTRIBUTE_SET tas ON ia.ATTRIBUTE_ID =tas.ATTRIBUTE_ID and tas.SET_ID = @SET_ID
	
	END
			

SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonStats]    Script Date: 18/09/2018 09:52:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ComparisonStats]
(
	@COMPARISON_ID INT,
	@Is_Screening bit OUTPUT
)
--with recompile
As

SET NOCOUNT ON

set @Is_Screening = (Select IS_SCREENING from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)

declare @n1 int , @n2 int , @n3 int --Total N items coded reviewer 1,2 & 3 (snapshot)
declare @c1 int = (select CONTACT_ID1 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
declare @c2 int = (select CONTACT_ID2 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
declare @c3 int = (select CONTACT_ID3 from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
declare @set int = (select SET_ID from tb_COMPARISON where COMPARISON_ID = @COMPARISON_ID)
DECLARE @T1 TABLE
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  ITEM_ARM_ID BIGINT
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID, ITEM_ARM_ID) 
	)
DECLARE @T1c TABLE --current attributes Reviewer1
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  ITEM_ARM_ID BIGINT
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID, ITEM_ARM_ID) 
	)
DECLARE @T2 TABLE
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  ITEM_ARM_ID BIGINT
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID, ITEM_ARM_ID)
	)
DECLARE @T2c TABLE --current attributes Reviewer2
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  ITEM_ARM_ID BIGINT
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID, ITEM_ARM_ID)
	)

DECLARE @T3 TABLE
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  ITEM_ARM_ID BIGINT
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID, ITEM_ARM_ID)
	)
DECLARE @T3c TABLE --current attributes Reviewer3
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  ITEM_ARM_ID BIGINT
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID, ITEM_ARM_ID)
	)
DECLARE @T1a2 table (ITEM_ID bigint primary key)--snapshot agreements 1 v 2
DECLARE @T1ca2 table (ITEM_ID bigint primary key)--current agreements 1 v 2
DECLARE @T1a3 table (ITEM_ID bigint primary key)--snapshot agreements 1 v 3
DECLARE @T1ca3 table (ITEM_ID bigint primary key)--current agreements 1 v 3
DECLARE @T2a3 table (ITEM_ID bigint primary key)--snapshot agreements 2 v 3
DECLARE @T2ca3 table (ITEM_ID bigint primary key)--current agreements 2 v 3
insert into @T1
select ITEM_ID, ATTRIBUTE_ID,
	 case 
		WHEN ITEM_ARM_ID is null THEN -1
		ELSE ITEM_ARM_ID
	 END
	 from tb_COMPARISON c
inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID and c.CONTACT_ID1 = cia.CONTACT_ID and c.COMPARISON_ID = @COMPARISON_ID
set @n1 = ( select count(distinct(item_id)) from @T1 )

insert into @T1c 
select t1.ITEM_ID, tia.ATTRIBUTE_ID,
	 case 
		WHEN tia.ITEM_ARM_ID is null THEN -1
		ELSE tia.ITEM_ARM_ID
	 END
	 from (select distinct ITEM_ID from @T1) t1 --only items in the comparison
	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @set and tis.CONTACT_ID = @c1
	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID

insert into @T2
select ITEM_ID, ATTRIBUTE_ID,
	 case 
		WHEN ITEM_ARM_ID is null THEN -1
		ELSE ITEM_ARM_ID
	 END
	 from tb_COMPARISON c
inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID and c.CONTACT_ID2 = cia.CONTACT_ID and c.COMPARISON_ID = @COMPARISON_ID
set @n2 = ( select count(distinct(item_id)) from @T2 )

insert into @T2c 
select t2.ITEM_ID, tia.ATTRIBUTE_ID,
	 case 
		WHEN tia.ITEM_ARM_ID is null THEN -1
		ELSE tia.ITEM_ARM_ID
	 END
	 from (select distinct ITEM_ID from @T2) t2 
	inner join TB_ITEM_SET tis on t2.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @set and tis.CONTACT_ID = @c2
	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID

insert into @T3
select ITEM_ID, ATTRIBUTE_ID,
	 case 
		WHEN ITEM_ARM_ID is null THEN -1
		ELSE ITEM_ARM_ID
	 END
	  from tb_COMPARISON c
inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID and c.CONTACT_ID3 = cia.CONTACT_ID and c.COMPARISON_ID = @COMPARISON_ID
set @n3 = ( select count(distinct(item_id)) from @T3 )

insert into @T3c 
select t3.ITEM_ID, tia.ATTRIBUTE_ID,
	 case 
		WHEN tia.ITEM_ARM_ID is null THEN -1
		ELSE tia.ITEM_ARM_ID
	 END
	  from (select distinct ITEM_ID from @T3) t3 
	inner join TB_ITEM_SET tis on t3.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @set and tis.CONTACT_ID = @c3
	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID


-- Total N items coded reviewer 1
select @n1 as 'Total N items coded reviewer 1'
-- Total N items coded reviewer 2
select @n2 as 'Total N items coded reviewer 2'
-- Total N items coded reviewer 3
select @n3 as 'Total N items coded reviewer 3'

-- Total N items coded reviewer 1 & 2
select count(distinct(t1.item_id)) as 'Total N items coded reviewer 1 & 2' from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID

-- Total disagreements 1vs2
--the inner join (IJ) selects only records in t2 where the second reviewer has applied a different code, but this does not guarantee coding as a whole is different, we still need to check if:
--a)	R1 has also coded with the same attribute found in t2 through IJ. If that’s the case, then we should not count this as a disagreement.
--b)	R2 has also coded with the attribute from t1
--The second outer join (OJ2), with the “where t2b.ATTRIBUTE_ID is null” clause checks for a). The first outer join (OJ1), with the “where t1b.ATTRIBUTE_ID is null” clause, checks  for b).
--So overall, the first join spots all possible 1:1 coding differences, the two outer joins get rid of meaningless lines (where the differences are cancelled by other records). 
select count(distinct(t1.item_id)) as 'Total disagreements 1vs2'
	from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID and t1.ITEM_ARM_ID = t2.ITEM_ARM_ID
	left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID and t1.ITEM_ARM_ID = t2b.ITEM_ARM_ID
	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID and t1b.ITEM_ARM_ID = t2.ITEM_ARM_ID
	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

-- Total N items coded reviewer 2 & 3
select count(distinct(t2.item_id)) as 'Total N items coded reviewer 2 & 3' from @T2 t2 inner join @T3 t3 on t2.ITEM_ID = t3.ITEM_ID

-- Total disagreements 2vs3
select count(distinct(t2.item_id)) as 'Total disagreements 2vs3'
	from @T2 t2 inner join @T3 t3 on t2.ITEM_ID = t3.ITEM_ID and t2.ATTRIBUTE_ID != t3.ATTRIBUTE_ID and t2.ITEM_ARM_ID = t3.ITEM_ARM_ID
	left outer join @T3 t3b on t2.ATTRIBUTE_ID = t3b.ATTRIBUTE_ID and t2.ITEM_ID = t3b.ITEM_ID and t2.ITEM_ARM_ID = t3b.ITEM_ARM_ID
	left outer join @T2 t2b on  t2b.ATTRIBUTE_ID = t3.ATTRIBUTE_ID and t2b.ITEM_ID = t3.ITEM_ID and t2b.ITEM_ARM_ID = t3.ITEM_ARM_ID
	where t2b.ATTRIBUTE_ID is null or t3b.ATTRIBUTE_ID is null

-- Total N items coded reviewer 1 & 3
select count(distinct(t1.item_id)) as 'Total N items coded reviewer 1 & 3' from @T1 t1 inner join @T3 t3 on t1.ITEM_ID = t3.ITEM_ID

-- Total disagreements 1vs3
select count(distinct(t1.item_id)) as 'Total disagreements 1vs3'
	from @T1 t1 inner join @T3 t3 on t1.ITEM_ID = t3.ITEM_ID and t1.ATTRIBUTE_ID != t3.ATTRIBUTE_ID and t1.ITEM_ARM_ID = t3.ITEM_ARM_ID
	left outer join @T3 t3b on t1.ATTRIBUTE_ID = t3b.ATTRIBUTE_ID and t1.ITEM_ID = t3b.ITEM_ID and t1.ITEM_ARM_ID = t3b.ITEM_ARM_ID
	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t3.ATTRIBUTE_ID and t1b.ITEM_ID = t3.ITEM_ID and t1b.ITEM_ARM_ID = t3.ITEM_ARM_ID
	where t1b.ATTRIBUTE_ID is null or t3b.ATTRIBUTE_ID is null

--REAL AGREEMENTS: Combine items from R1 and R2 and get only those that are not currenlty disagreements
insert into @T1ca2
Select t1.item_id from @T1c t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
	inner join @T2c t2 on t1.ITEM_ID = t2.ITEM_ID
	except
	select distinct(t1.item_id) from @T1c t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
											-- then the double outer joins as before
	inner join @T2c t2 on t1.ITEM_ID = t2.ITEM_ID
				and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID and t1.ITEM_ARM_ID = t2.ITEM_ARM_ID
	left outer join @T1c tia1a on tia1a.ITEM_ID = t1.ITEM_ID and t2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID and tia1a.ITEM_ARM_ID = t1.ITEM_ARM_ID
	left outer join @T2c tia2a on tia2a.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID and tia2a.ITEM_ARM_ID = t2.ITEM_ARM_ID
	where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null


--insert into @T1ca2
--Select t1.item_id from @T1 t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
--	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.CONTACT_ID = @c1 and tis.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID --need to join all the way to TB_ITEM_ATTRIBUTE to be 100% that some codes are present
--	inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID
--	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c2 and tis2.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID	
--	except
	--select * from @T1 t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
	--										-- then the double outer joins as before
	--inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.CONTACT_ID = @c1 and tis1.SET_ID = @set
	--inner join TB_ITEM_ATTRIBUTE tia1 on tis1.ITEM_SET_ID = tia1.ITEM_SET_ID 
	--inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID 
	--inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c2 and tis2.SET_ID = @set
	--inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID 
	--			and tia1.ATTRIBUTE_ID != tia2.ATTRIBUTE_ID
	--left outer join TB_ITEM_ATTRIBUTE tia1a on tis1.ITEM_SET_ID = tia1a.ITEM_SET_ID and tia1a.ITEM_ID = tis1.ITEM_ID and tia2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
	--left outer join TB_ITEM_ATTRIBUTE tia2a on tis2.ITEM_SET_ID = tia2a.ITEM_SET_ID and tia2a.ITEM_ID = tis2.ITEM_ID and tia1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
	--where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null

--COMPARISON AGREEMENTS: 1 V 2, uses the same techniques as before to find the list of all agreed-items according to the comparison snapshot
insert into @T1a2
Select distinct t1.ITEM_ID from @T1 t1 
	inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID
	except
select distinct(t1.item_id) from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID and t1.ITEM_ARM_ID = t2.ITEM_ARM_ID
	left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID and t2b.ITEM_ARM_ID = t1.ITEM_ARM_ID 
	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID and t1b.ITEM_ARM_ID = t2.ITEM_ARM_ID
	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

--REAL AGREEMENTS: Combine items from R1 and R3 and get only those that are not currenlty disagreements
insert into @T1ca3
Select t1.item_id from @T1c t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
	inner join @T3c t3 on t1.ITEM_ID = t3.ITEM_ID
	except
	select distinct(t1.item_id) from @T1c t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
											-- then the double outer joins as before
	inner join @T3c t3 on t1.ITEM_ID = t3.ITEM_ID
				and t1.ATTRIBUTE_ID != t3.ATTRIBUTE_ID and t1.ITEM_ARM_ID = t3.ITEM_ARM_ID
	left outer join @T1c tia1a on tia1a.ITEM_ID = t1.ITEM_ID and t3.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID and tia1a.ITEM_ARM_ID = t1.ITEM_ARM_ID
	left outer join @T3c tia3a on tia3a.ITEM_ID = t3.ITEM_ID and t1.ATTRIBUTE_ID = tia3a.ATTRIBUTE_ID and tia3a.ITEM_ARM_ID = t3.ITEM_ARM_ID
	where tia1a.ATTRIBUTE_ID is null or tia3a.ATTRIBUTE_ID is null


----REAL AGREEMENTS: Combine items from R1 and R3 and get only those that are not currenlty disagreements
--insert into @T1ca3
--Select t1.item_id from @T1 t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
--	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.CONTACT_ID = @c1 and tis.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID --need to join all the way to TB_ITEM_ATTRIBUTE to be 100% that some codes are present
--	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
--	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID	
--	except
--	select distinct(t1.item_id) from @T1 t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
--											-- then the double outer joins as before
--	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.CONTACT_ID = @c1 and tis1.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia1 on tis1.ITEM_SET_ID = tia1.ITEM_SET_ID 
--	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID 
--	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID 
--				and tia1.ATTRIBUTE_ID != tia2.ATTRIBUTE_ID
--	left outer join TB_ITEM_ATTRIBUTE tia1a on tis1.ITEM_SET_ID = tia1a.ITEM_SET_ID and tia2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
--	left outer join TB_ITEM_ATTRIBUTE tia2a on tis2.ITEM_SET_ID = tia2a.ITEM_SET_ID and tia1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
--	where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null

--COMPARISON AGREEMENTS: 1 V 3, uses the same techniques as before to find the list of all agreed-items according to the comparison snapshot
insert into @T1a3
Select distinct t1.ITEM_ID from @T1 t1 
	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
	except
select distinct(t1.item_id) from @T1 t1 inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID and t1.ITEM_ARM_ID = t2.ITEM_ARM_ID
	left outer join @T3 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID and t2b.ITEM_ARM_ID = t1.ITEM_ARM_ID
	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID and t1b.ITEM_ARM_ID = t2.ITEM_ARM_ID
	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null

--REAL AGREEMENTS: Combine items from R2 and R3 and get only those that are not currenlty disagreements
insert into @T2ca3
Select t2.item_id from @T2c t2 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
	inner join @T3c t3 on t2.ITEM_ID = t3.ITEM_ID
	except
	select distinct(t2.item_id) from @T2c t2 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
											-- then the double outer joins as before
	inner join @T3c t3 on t2.ITEM_ID = t3.ITEM_ID
				and t2.ATTRIBUTE_ID != t3.ATTRIBUTE_ID and t2.ITEM_ARM_ID = t3.ITEM_ARM_ID
	left outer join @T2c tia2a on tia2a.ITEM_ID = t2.ITEM_ID and t3.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID and tia2a.ITEM_ARM_ID = t2.ITEM_ARM_ID
	left outer join @T3c tia3a on tia3a.ITEM_ID = t3.ITEM_ID and t2.ATTRIBUTE_ID = tia3a.ATTRIBUTE_ID and tia3a.ITEM_ARM_ID = t3.ITEM_ARM_ID
	where tia2a.ATTRIBUTE_ID is null or tia3a.ATTRIBUTE_ID is null
	
----REAL AGREEMENTS: Combine items from R2 and R3 and get only those that are not currenlty disagreements
--insert into @T2ca3
--Select t1.item_id from @T2 t1 --this list all items that are present in the comparison R1 vs R2 and have some codes applied by both
--	inner join TB_ITEM_SET tis on t1.ITEM_ID = tis.ITEM_ID and tis.CONTACT_ID = @c2 and tis.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia on tis.ITEM_SET_ID = tia.ITEM_SET_ID --need to join all the way to TB_ITEM_ATTRIBUTE to be 100% that some codes are present
--	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
--	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID	
--	except
--	select distinct(t1.item_id) from @T2 t1 --this lists all actual disagreements, going through tb_item_set->tb_item_attribute to get the real situation,
--											-- then the double outer joins as before
--	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.CONTACT_ID = @c2 and tis1.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia1 on tis1.ITEM_SET_ID = tia1.ITEM_SET_ID 
--	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID 
--	inner join TB_ITEM_SET tis2 on t2.ITEM_ID = tis2.ITEM_ID and tis2.CONTACT_ID = @c3 and tis2.SET_ID = @set
--	inner join TB_ITEM_ATTRIBUTE tia2 on tis2.ITEM_SET_ID = tia2.ITEM_SET_ID 
--				and tia1.ATTRIBUTE_ID != tia2.ATTRIBUTE_ID
--	left outer join TB_ITEM_ATTRIBUTE tia1a on tis1.ITEM_SET_ID = tia1a.ITEM_SET_ID and tia2.ATTRIBUTE_ID = tia1a.ATTRIBUTE_ID
--	left outer join TB_ITEM_ATTRIBUTE tia2a on tis2.ITEM_SET_ID = tia2a.ITEM_SET_ID and tia1.ATTRIBUTE_ID = tia2a.ATTRIBUTE_ID
--	where tia1a.ATTRIBUTE_ID is null or tia2a.ATTRIBUTE_ID is null

--COMPARISON AGREEMENTS: 2 V 3, uses the same techniques as before to find the list of all agreed-items according to the comparison snapshot
insert into @T2a3
Select distinct t1.ITEM_ID from @T2 t1 
	inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID
	except
select distinct(t1.item_id) from @T2 t1 inner join @T3 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID and t1.ITEM_ARM_ID = t2.ITEM_ARM_ID
	left outer join @T3 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID and t2b.ITEM_ARM_ID = t1.ITEM_ARM_ID
	left outer join @T2 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID and t1b.ITEM_ARM_ID = t2.ITEM_ARM_ID
	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null


-- Are all Comparison Agreements currently completed OR are current agreements different from the snapshot agreements?
-- 1 V 2
Select Case when (COUNT(distinct ITEM_ID) = SUM(Completed)
				  OR 
					( select 
							Case when (SUM(sm.ss) > 0) then 1 --
							else 0
							end
						 from
						(
						Select COUNT(t1.ITEM_ID) ss from @T1a2 t1 
							left join @T1ca2 t2 on t1.ITEM_ID = t2.ITEM_ID
							where t2.ITEM_ID is null
						UNION
						Select COUNT(t1.ITEM_ID) ss from @T1ca2 t1 
							left join @T1a2 t2 on t1.ITEM_ID = t2.ITEM_ID
							where t2.ITEM_ID is null
						) AS sm
					) = 1
				  ) then 1 else 0 end 
				  as '1v2 lock-completion OR changed'
	from 
	(Select distinct t1.ITEM_ID, Case
								when (tis1.IS_COMPLETED = 1 ) then 1
								else 0
							end as Completed
	from @T1a2 t1 
	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.SET_ID = @set --joining on item and set, not on CONTACT_ID as item could be completed by anyone
	) a

-- Are all Comparison Agreements currently completed OR are current agreements different from the snapshot agreements?
-- 1 V 3
Select Case when (COUNT(distinct ITEM_ID) = SUM(Completed)
				  OR 
					( select 
							Case when (SUM(sm.ss) > 0) then 1 --
							else 0
							end
						 from
						(
						Select COUNT(t1.ITEM_ID) ss from @T1a3 t1 
							left join @T1ca3 t2 on t1.ITEM_ID = t2.ITEM_ID
							where t2.ITEM_ID is null
						UNION
						Select COUNT(t1.ITEM_ID) ss from @T1ca3 t1 
							left join @T1a3 t2 on t1.ITEM_ID = t2.ITEM_ID
							where t2.ITEM_ID is null
						) AS sm
					) = 1
				  ) then 1 else 0 end 
				  as '1v3 lock-completion OR changed'
	from 
	(Select distinct t1.ITEM_ID, Case
								when (tis1.IS_COMPLETED = 1 ) then 1
								else 0
							end as Completed
	from @T1a3 t1 
	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.SET_ID = @set --joining on item and set, not on CONTACT_ID as item could be completed by anyone
	) a

-- Are all Comparison Agreements currently completed OR are current agreements different from the snapshot agreements?
-- 2 V 3
Select Case when (COUNT(distinct ITEM_ID) = SUM(Completed)
				  OR 
					( select 
							Case when (SUM(sm.ss) > 0) then 1 --
							else 0
							end
						 from
						(
						Select COUNT(t1.ITEM_ID) ss from @T2a3 t1 
							left join @T2ca3 t2 on t1.ITEM_ID = t2.ITEM_ID
							where t2.ITEM_ID is null
						UNION
						Select COUNT(t1.ITEM_ID) ss from @T2ca3 t1 
							left join @T2a3 t2 on t1.ITEM_ID = t2.ITEM_ID
							where t2.ITEM_ID is null
						) AS sm
					) = 1
				  ) then 1 else 0 end 
				  as '2v3 lock-completion OR changed'
	from 
	(Select distinct t1.ITEM_ID, Case
								when (tis1.IS_COMPLETED = 1 ) then 1
								else 0
							end as Completed
	from @T2a3 t1 
	inner join TB_ITEM_SET tis1 on t1.ITEM_ID = tis1.ITEM_ID and tis1.SET_ID = @set --joining on item and set, not on CONTACT_ID as item could be completed by anyone
	) a

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemComparisonList]    Script Date: 19/09/2018 10:08:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ItemComparisonList] (
	@REVIEW_ID INT,
	@COMPARISON_ID INT,
	@LIST_WHAT NVARCHAR(25),
	
	@PageNum INT = 1,
	@PerPage INT = 3,
	@CurrentPage INT OUTPUT,
	@TotalPages INT OUTPUT,
	@TotalRows INT OUTPUT 
)

As
SET NOCOUNT ON
DECLARE @T1 TABLE --item_attribute for R1, R1 and R2 are relative to the sproc, could be any couple from 
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  ITEM_ARM_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID, ITEM_ARM_ID) 
	)
DECLARE @T2 TABLE --item_attribute for R2
	(
	  ITEM_ID BIGINT,
	  ATTRIBUTE_ID BIGINT,
	  ITEM_ARM_ID BIGINT,
	  PRIMARY KEY (ITEM_ID, ATTRIBUTE_ID, ITEM_ARM_ID) 
	)
DECLARE @TT table (ITEM_ID bigint primary key)

insert into @T1 --item attributes from R1
select ITEM_ID, ATTRIBUTE_ID,
	 case 
		WHEN ITEM_ARM_ID is null THEN -1
		ELSE ITEM_ARM_ID
	 END
	 from tb_COMPARISON c
	inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID 
												and  cia.CONTACT_ID = 
													CASE  @LIST_WHAT
														WHEN 'ComparisonAgree2vs3' THEN c.CONTACT_ID2
														WHEN 'ComparisonDisagree2vs3' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID1
													END 
												and c.COMPARISON_ID = @COMPARISON_ID

insert into @T2 --item attributes from R2
select ITEM_ID, ATTRIBUTE_ID,
	 case 
		WHEN ITEM_ARM_ID is null THEN -1
		ELSE ITEM_ARM_ID
	 END
	 from tb_COMPARISON c
inner join tb_COMPARISON_ITEM_ATTRIBUTE cia on c.COMPARISON_ID = cia.COMPARISON_ID 
												and cia.CONTACT_ID = 
													CASE  @LIST_WHAT
														WHEN 'ComparisonAgree1vs2' THEN c.CONTACT_ID2
														WHEN 'ComparisonDisagree1vs2' THEN c.CONTACT_ID2
														ELSE c.CONTACT_ID3
													END 
												and c.COMPARISON_ID = @COMPARISON_ID


IF (@LIST_WHAT LIKE 'ComparisonAgree%')
BEGIN
	insert into @TT --add all agreements; see st_ComparisonStats to understand how this works
	Select distinct t1.ITEM_ID from @T1 t1 
		inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID
		except
	select distinct(t1.item_id) from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID  and t1.ITEM_ARM_ID = t2.ITEM_ARM_ID
		left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID and t1.ITEM_ARM_ID = t2b.ITEM_ARM_ID
		left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID and t1b.ITEM_ARM_ID = t2.ITEM_ARM_ID
		where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null
END
ELSE
BEGIN
	insert into @TT --add all disagreements; see st_ComparisonStats to understand how this works
	select distinct(t1.item_id)
	from @T1 t1 inner join @T2 t2 on t1.ITEM_ID = t2.ITEM_ID and t1.ATTRIBUTE_ID != t2.ATTRIBUTE_ID and t1.ITEM_ARM_ID = t2.ITEM_ARM_ID
	left outer join @T2 t2b on t1.ATTRIBUTE_ID = t2b.ATTRIBUTE_ID and t1.ITEM_ID = t2b.ITEM_ID and t1.ITEM_ARM_ID = t2b.ITEM_ARM_ID
	left outer join @T1 t1b on  t1b.ATTRIBUTE_ID = t2.ATTRIBUTE_ID and t1b.ITEM_ID = t2.ITEM_ID and t1b.ITEM_ARM_ID = t2.ITEM_ARM_ID
	where t1b.ATTRIBUTE_ID is null or t2b.ATTRIBUTE_ID is null
	
END

declare @RowsToRetrieve int

	SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
	FROM TB_ITEM I
	INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
		TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
	WHERE I.ITEM_ID IN (SELECT ITEM_ID FROM @TT)

	set @TotalPages = @TotalRows/@PerPage

	if @PageNum < 1
	set @PageNum = 1

	if @TotalRows % @PerPage != 0
	set @TotalPages = @TotalPages + 1

	set @RowsToRetrieve = @PerPage * @PageNum
	set @CurrentPage = @PageNum;

	WITH SearchResults AS
	(

SELECT DISTINCT(I.ITEM_ID), I.[TYPE_ID], I.OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
	TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
	[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
	AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENTAUTHORS
	,TB_ITEM_REVIEW.MASTER_ITEM_ID, DOI, KEYWORDS 
	, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
FROM TB_ITEM I
INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
	TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
WHERE I.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	
)
	Select ITEM_ID, [TYPE_ID], OLD_ITEM_ID, AUTHORS,
		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
		AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, PARENTAUTHORS
		,SearchResults.MASTER_ITEM_ID, DOI, KEYWORDS
		, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
	FROM SearchResults 
	WHERE RowNum > @RowsToRetrieve - @PerPage
	AND RowNum <= @RowsToRetrieve 

SELECT	@CurrentPage as N'@CurrentPage',
		@TotalPages as N'@TotalPages',
		@TotalRows as N'@TotalRows'

SET NOCOUNT OFF

--DECLARE @TT TABLE
--	(
--	  ITEM_ID BIGINT,
--	  ATTRIBUTE_ID BIGINT
--	)

--IF (@LIST_WHAT LIKE 'ComparisonAgree%')
--BEGIN
--	INSERT INTO @TT(ITEM_ID, ATTRIBUTE_ID)
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--			CASE  @LIST_WHAT
--				WHEN 'ComparisonAgree2vs3' THEN tb_COMPARISON.CONTACT_ID2
--				ELSE tb_COMPARISON.CONTACT_ID1
--			END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	INTERSECT --  ********** AGREEMENT - THEREFORE INTERSECT
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--		CASE @LIST_WHAT
--			WHEN 'ComparisonAgree1vs2' THEN TB_COMPARISON.CONTACT_ID2
--			ELSE tb_COMPARISON.CONTACT_ID3
--		END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	ORDER BY ITEM_ID, ATTRIBUTE_ID
--END
--ELSE
--BEGIN
--INSERT INTO @TT(ITEM_ID, ATTRIBUTE_ID)
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--			CASE  @LIST_WHAT
--				WHEN 'ComparisonDisagree2vs3' THEN tb_COMPARISON.CONTACT_ID2
--				ELSE tb_COMPARISON.CONTACT_ID1
--			END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	EXCEPT -- ******************* DISAGREEMENT THEREFORE EXCEPT
--	SELECT ITEM_ID, ATTRIBUTE_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE
--	INNER JOIN TB_COMPARISON ON TB_COMPARISON.COMPARISON_ID = TB_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID
--		AND TB_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID =
--		CASE @LIST_WHAT
--			WHEN 'ComparisonDisagree1vs2' THEN TB_COMPARISON.CONTACT_ID2
--			ELSE tb_COMPARISON.CONTACT_ID3
--		END
--		AND TB_COMPARISON.COMPARISON_ID = @COMPARISON_ID
--	ORDER BY ITEM_ID, ATTRIBUTE_ID
	
--	-- Make sure that both have coded each item (only needed for disagreements - as agreements work by definition)
--	DELETE FROM @TT WHERE NOT ITEM_ID IN
--	(
--	SELECT DISTINCT CIA1.ITEM_ID
--	FROM TB_COMPARISON_ITEM_ATTRIBUTE CIA1
--		INNER JOIN tb_COMPARISON_ITEM_ATTRIBUTE CIA2 ON CIA1.ITEM_ID = CIA2.ITEM_ID
--		INNER JOIN tb_COMPARISON COMP1 ON COMP1.COMPARISON_ID = @COMPARISON_ID AND CIA1.CONTACT_ID =
--			CASE @LIST_WHAT
--				WHEN 'ComparisonDisagree2vs3' THEN COMP1.CONTACT_ID2
--				ELSE COMP1.CONTACT_ID1
--			END
--		INNER JOIN tb_COMPARISON COMP2 ON COMP2.COMPARISON_ID = @COMPARISON_ID AND CIA2.CONTACT_ID =
--			CASE @LIST_WHAT
--				WHEN 'ComparisonDisagree1vs2' THEN COMP2.CONTACT_ID2
--				ELSE COMP2.CONTACT_ID3
--			END
--	WHERE CIA1.COMPARISON_ID = @COMPARISON_ID AND CIA2.COMPARISON_ID = @COMPARISON_ID
--	)
	
--END

--declare @RowsToRetrieve int

--	SELECT @TotalRows = count(DISTINCT I.ITEM_ID)
--	FROM TB_ITEM I
--	INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
--	INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
--		TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
--	WHERE I.ITEM_ID IN (SELECT ITEM_ID FROM @TT)

--	set @TotalPages = @TotalRows/@PerPage

--	if @PageNum < 1
--	set @PageNum = 1

--	if @TotalRows % @PerPage != 0
--	set @TotalPages = @TotalPages + 1

--	set @RowsToRetrieve = @PerPage * @PageNum
--	set @CurrentPage = @PageNum;

--	WITH SearchResults AS
--	(

--SELECT DISTINCT(I.ITEM_ID), I.[TYPE_ID], I.OLD_ITEM_ID, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 0) as AUTHORS,
--	TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
--	[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
--	AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, [dbo].fn_REBUILD_AUTHORS(I.ITEM_ID, 1) as PARENTAUTHORS
--	,TB_ITEM_REVIEW.MASTER_ITEM_ID 
--	, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
--FROM TB_ITEM I
--INNER JOIN TB_ITEM_TYPE ON TB_ITEM_TYPE.[TYPE_ID] = I.[TYPE_ID]
--INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = I.ITEM_ID AND 
--	TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
--WHERE I.ITEM_ID IN (SELECT ITEM_ID FROM @TT)
	
--)
--	Select ITEM_ID, [TYPE_ID], OLD_ITEM_ID, AUTHORS,
--		TITLE, PARENT_TITLE, SHORT_TITLE, DATE_CREATED, CREATED_BY, DATE_EDITED, EDITED_BY,
--		[YEAR], [MONTH], STANDARD_NUMBER, CITY, COUNTRY, PUBLISHER, INSTITUTION, VOLUME, PAGES, EDITION, ISSUE, IS_LOCAL,
--		AVAILABILITY, URL, ABSTRACT, COMMENTS, [TYPE_NAME], IS_DELETED, IS_INCLUDED, PARENTAUTHORS
--		,SearchResults.MASTER_ITEM_ID
--		, ROW_NUMBER() OVER(order by SHORT_TITLE, TITLE) RowNum
--	FROM SearchResults 
--	WHERE RowNum > @RowsToRetrieve - @PerPage
--	AND RowNum <= @RowsToRetrieve 

--SELECT	@CurrentPage as N'@CurrentPage',
--		@TotalPages as N'@TotalPages',
--		@TotalRows as N'@TotalRows'

--SET NOCOUNT OFF
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ComparisonAttributesList]    Script Date: 20/09/2018 12:10:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ComparisonAttributesList]
(
	@COMPARISON_ID INT,
	@PARENT_ATTRIBUTE_ID BIGINT = 0,
	@SET_ID INT
)

As

SET NOCOUNT ON

	SELECT COMPARISON_ITEM_ATTRIBUTE_ID, COMPARISON_ID, tb_COMPARISON_ITEM_ATTRIBUTE.ITEM_ID, tb_COMPARISON_ITEM_ATTRIBUTE.ATTRIBUTE_ID,
		ADDITIONAL_TEXT, tb_COMPARISON_ITEM_ATTRIBUTE.CONTACT_ID, tb_COMPARISON_ITEM_ATTRIBUTE.SET_ID,
		ATTRIBUTE_NAME, TITLE, CAST('FALSE' AS BIT) IS_COMPLETED
		, CASE when ARM_NAME is null then '' ELSE ARM_NAME end AS ARM_NAME
	FROM tb_COMPARISON_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = tb_COMPARISON_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = tb_COMPARISON_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = tb_COMPARISON_ITEM_ATTRIBUTE.ATTRIBUTE_ID
			AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @PARENT_ATTRIBUTE_ID
			AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
		LEFT OUTER JOIN TB_ITEM_ARM on tb_COMPARISON_ITEM_ATTRIBUTE.ITEM_ARM_ID = TB_ITEM_ARM.ITEM_ARM_ID
	WHERE COMPARISON_ID = @COMPARISON_ID
	
	UNION
	
	SELECT 0, @COMPARISON_ID, TB_ITEM_ATTRIBUTE.ITEM_ID, TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID, TB_ITEM_ATTRIBUTE.ADDITIONAL_TEXT, 
		TB_ITEM_SET.CONTACT_ID, TB_ITEM_SET.SET_ID, ATTRIBUTE_NAME, TITLE, CAST('TRUE' AS BIT) IS_COMPLETED
		, CASE when ARM_NAME is null then '' ELSE ARM_NAME end AS ARM_NAME
	FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.SET_ID = @SET_ID
		INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID
		INNER JOIN TB_ITEM ON TB_ITEM.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
		INNER JOIN tb_COMPARISON_ITEM_ATTRIBUTE ON tb_COMPARISON_ITEM_ATTRIBUTE.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
			AND tb_COMPARISON_ITEM_ATTRIBUTE.COMPARISON_ID = @COMPARISON_ID
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = tb_COMPARISON_ITEM_ATTRIBUTE.ATTRIBUTE_ID
			AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @PARENT_ATTRIBUTE_ID
			AND TB_ATTRIBUTE_SET.SET_ID = @SET_ID
		LEFT OUTER JOIN TB_ITEM_ARM on tb_COMPARISON_ITEM_ATTRIBUTE.ITEM_ARM_ID = TB_ITEM_ARM.ITEM_ARM_ID
	WHERE TB_ITEM_SET.IS_COMPLETED = 'TRUE'
	
	ORDER BY ITEM_ID, CONTACT_ID
	
SET NOCOUNT OFF
GO

--SERGIO Crosstabs

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ItemAttributeCrosstabs]    Script Date: 20/09/2018 13:41:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER procedure [dbo].[st_ItemAttributeCrosstabs]
(
	@PARENT_ATTRIBUTE_ID1 BIGINT,
	@PARENT_SET_ID1 INT,
	@PARENT_SET_ID2 INT,
	@PARENT_ATTRIBUTE_ID2 BIGINT,
	@FILTER_ATTRIBUTE_ID BIGINT,
	@REVIEW_ID INT
)

As

SET NOCOUNT ON

DECLARE @TT TABLE
	(
	  ATTRIBUTE_ID BIGINT,
	  ATTRIBUTE_NAME NVARCHAR(255),
	  ATTRIBUTE_ORDER INT
	)
	
INSERT INTO @TT(ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER)
SELECT TB_ATTRIBUTE.ATTRIBUTE_ID, ATTRIBUTE_NAME, ATTRIBUTE_ORDER FROM TB_ATTRIBUTE
INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ATTRIBUTE.ATTRIBUTE_ID
	AND TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = @PARENT_ATTRIBUTE_ID1
	AND TB_ATTRIBUTE_SET.SET_ID = @PARENT_SET_ID1
ORDER BY ATTRIBUTE_ORDER

DECLARE @cols NVARCHAR(2000)
SELECT  @cols = COALESCE(@cols + ',[' + CAST(ATTRIBUTE_ID AS NVARCHAR(10)) + ']',
                         '[' + CAST(ATTRIBUTE_ID AS NVARCHAR(10)) + ']')
FROM    @TT
ORDER BY ATTRIBUTE_ORDER

DECLARE @query NVARCHAR(4000)

IF (@FILTER_ATTRIBUTE_ID IS NULL OR @FILTER_ATTRIBUTE_ID = 0)
BEGIN
SET @query = N'SELECT ATTRIBUTE_NAME, ATTRIBUTE_ID2, ATTRIBUTE_ORDER2, '+
@cols +' 
FROM
(
select IA1.ATTRIBUTE_ID ATTRIBUTE_ID1, TBA2.ATTRIBUTE_NAME ATTRIBUTE_NAME, TBA2.ATTRIBUTE_ID ATTRIBUTE_ID2,
	IA1.ITEM_ID ITEM_ID1, AS2.ATTRIBUTE_ORDER ATTRIBUTE_ORDER2
from TB_ITEM_ATTRIBUTE IA1
INNER JOIN TB_ATTRIBUTE_SET AS1 ON AS1.ATTRIBUTE_ID = IA1.ATTRIBUTE_ID
	AND AS1.PARENT_ATTRIBUTE_ID = ' + CAST(@PARENT_ATTRIBUTE_ID1 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_SET IS1 ON IS1.ITEM_SET_ID = IA1.ITEM_SET_ID AND IS1.IS_COMPLETED = 1
	AND IS1.SET_ID = ' + CAST(@PARENT_SET_ID1 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = IA1.ITEM_ID
INNER JOIN TB_ATTRIBUTE_SET AS2 ON AS2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
	AND AS2.PARENT_ATTRIBUTE_ID = ' + CAST(@PARENT_ATTRIBUTE_ID2 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 1
	AND IS2.SET_ID = ' + CAST(@PARENT_SET_ID2 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ATTRIBUTE TBA2 ON TBA2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = IA1.ITEM_ID
	AND TB_ITEM_REVIEW.IS_DELETED = 0
	AND TB_ITEM_REVIEW.REVIEW_ID = ' + CAST(@REVIEW_ID AS NVARCHAR(10)) + ' 
	group by  IA1.ATTRIBUTE_ID,TBA2.ATTRIBUTE_NAME, TBA2.ATTRIBUTE_ID, IA1.ITEM_ID, AS2.ATTRIBUTE_ORDER
) p
PIVOT
(
COUNT (P.ITEM_ID1)
FOR ATTRIBUTE_ID1 IN
( '+
@cols +' )
) AS pvt
ORDER BY ATTRIBUTE_ORDER2;'
END
ELSE
BEGIN
SET @query = N'SELECT ATTRIBUTE_NAME, ATTRIBUTE_ID2, ATTRIBUTE_ORDER2, '+
@cols +'
FROM
(
select IA1.ATTRIBUTE_ID ATTRIBUTE_ID1, TBA2.ATTRIBUTE_NAME ATTRIBUTE_NAME, TBA2.ATTRIBUTE_ID ATTRIBUTE_ID2,
	IA1.ITEM_ID ITEM_ID1, AS2.ATTRIBUTE_ORDER ATTRIBUTE_ORDER2
from TB_ITEM_ATTRIBUTE IA1
INNER JOIN TB_ATTRIBUTE_SET AS1 ON AS1.ATTRIBUTE_ID = IA1.ATTRIBUTE_ID
	AND AS1.PARENT_ATTRIBUTE_ID = ' + CAST(@PARENT_ATTRIBUTE_ID1 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_SET IS1 ON IS1.ITEM_SET_ID = IA1.ITEM_SET_ID AND IS1.IS_COMPLETED = 1
	AND IS1.SET_ID = ' + CAST(@PARENT_SET_ID1 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_ATTRIBUTE IA2 ON IA2.ITEM_ID = IA1.ITEM_ID
INNER JOIN TB_ATTRIBUTE_SET AS2 ON AS2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
	AND AS2.PARENT_ATTRIBUTE_ID = ' + CAST(@PARENT_ATTRIBUTE_ID2 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_SET IS2 ON IS2.ITEM_SET_ID = IA2.ITEM_SET_ID AND IS2.IS_COMPLETED = 1
	AND IS2.SET_ID = ' + CAST(@PARENT_SET_ID2 AS NVARCHAR(10)) + ' 
INNER JOIN TB_ATTRIBUTE TBA2 ON TBA2.ATTRIBUTE_ID = IA2.ATTRIBUTE_ID
INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = IA1.ITEM_ID
	AND TB_ITEM_REVIEW.IS_DELETED = 0
	AND TB_ITEM_REVIEW.REVIEW_ID = ' + CAST(@REVIEW_ID AS NVARCHAR(10)) + ' 
-- Code to filter
INNER JOIN TB_ITEM_ATTRIBUTE IA3 ON IA3.ITEM_ID = IA1.ITEM_ID
	AND IA3.ATTRIBUTE_ID = ' + CAST(@FILTER_ATTRIBUTE_ID AS NVARCHAR(10)) + ' 
INNER JOIN TB_ITEM_SET IS3 ON IS3.ITEM_SET_ID = IA3.ITEM_SET_ID
	AND IS3.IS_COMPLETED = 1
	group by  IA1.ATTRIBUTE_ID,TBA2.ATTRIBUTE_NAME, TBA2.ATTRIBUTE_ID, IA1.ITEM_ID, AS2.ATTRIBUTE_ORDER
) p
PIVOT
(
COUNT (P.ITEM_ID1)
FOR ATTRIBUTE_ID1 IN
( '+
@cols +' )
) AS pvt
ORDER BY ATTRIBUTE_ORDER2;'
END

EXECUTE(@query)
select @query
SET NOCOUNT OFF
GO

--SERGIO configurable reports.

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReportData]    Script Date: 20/09/2018 14:53:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ReportData]
	-- Add the parameters for the stored procedure here
	@REVIEW_ID INT
,	@ITEM_IDS NVARCHAR(MAX)
,	@REPORT_ID INT
,	@ORDER_BY NVARCHAR(15)
,	@ATTRIBUTE_ID BIGINT
,	@IS_QUESTION bit
,	@FULL_DETAILS bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @TT TABLE
	(
	  ITEM_ID BIGINT primary key
	)
	DECLARE @AA TABLE
	(
	  A_ID BIGINT 
	  , REPORT_COLUMN_CODE_ID int
	  , ATTRIBUTE_ORDER int
	)
	IF @ATTRIBUTE_ID != 0
	BEGIN
		INSERT INTO @TT
			SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID
				AND TB_ITEM_REVIEW.IS_DELETED = 0
				AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID
	END
	ELSE
	BEGIN
		INSERT INTO @TT
			SELECT VALUE FROM dbo.fn_Split_int(@ITEM_IDS, ',')
	END
	IF @IS_QUESTION = 1
	BEGIN
		INSERT INTO @AA SELECT distinct tas.ATTRIBUTE_ID, cc.REPORT_COLUMN_CODE_ID, tas.ATTRIBUTE_ORDER
			from TB_REPORT_COLUMN_CODE cc
			INNER JOIN TB_ATTRIBUTE_SET tas ON tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID 
				AND tas.SET_ID = cc.SET_ID And cc.REPORT_ID = @REPORT_ID
			inner join TB_ITEM_ATTRIBUTE ia on tas.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
			inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
			order by tas.ATTRIBUTE_ID
	END
	ELSE
	BEGIN
		INSERT INTO @AA SELECT distinct tas.ATTRIBUTE_ID, cc.REPORT_COLUMN_CODE_ID, tas.ATTRIBUTE_ORDER
			from TB_REPORT_COLUMN_CODE cc
			INNER JOIN TB_ATTRIBUTE_SET tas ON tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID 
				AND tas.SET_ID = cc.SET_ID And cc.REPORT_ID = @REPORT_ID
			inner join TB_ITEM_ATTRIBUTE ia on tas.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
			inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	END
	--select * from @AA
    --First: the main report properties
	SELECT * from TB_REPORT where REPORT_ID = @REPORT_ID
	--Second: list of report columns
	SELECT * from TB_REPORT_COLUMN where REPORT_ID = @REPORT_ID ORDER BY COLUMN_ORDER
	--Third: what goes into each column, AKA "Rows" (In C# side)
	SELECT * from TB_REPORT_COLUMN_CODE  
		where REPORT_ID = @REPORT_ID ORDER BY CODE_ORDER
	
	
	--Fourth: most of the real data
	SELECT distinct cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID,cc.USER_DEF_TEXT
				,a.*, ia.*, i.ITEM_ID, i.OLD_ITEM_ID, i.SHORT_TITLE, CODE_ORDER, ATTRIBUTE_ORDER
				, CASE when tia.ARM_NAME is null then '' else tia.ARM_NAME END as ARM_NAME
	from TB_REPORT_COLUMN_CODE cc
	inner join @AA ats on ats.REPORT_COLUMN_CODE_ID = cc.REPORT_COLUMN_CODE_ID
	--INNER JOIN TB_ATTRIBUTE_SET tas ON (--Question reports fetch data about a given code children
	--									(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
	--									OR 
	--									(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
	--								   )
	--	AND tas.SET_ID = cc.SET_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = ats.A_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID 
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	left outer join TB_ITEM_ARM tia on ia.ITEM_ARM_ID = tia.ITEM_ARM_ID
	where REPORT_ID = @REPORT_ID 
	ORDER BY 
		i.SHORT_TITLE -- we ignore the sorting order required for this report, sorting is done on c# side.
		, i.ITEM_ID, CODE_ORDER, ATTRIBUTE_ORDER, a.ATTRIBUTE_ID
	
	
	--Fift: data about coded TXT, uses "UNION" to grab data from TXT and PDF tables
	SELECT cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID, a.ATTRIBUTE_ID, tt.ITEM_ID, id.DOCUMENT_TITLE
	, 'Page ' + CONVERT(varchar(10),PAGE) + ':' + CHAR(10) + '[¬s]"' + replace(SELECTION_TEXTS, '¬', '"' + CHAR(10) + '"') +'[¬e]"' CODED_TEXT
	, CASE when tia.ARM_NAME is null then '' else tia.ARM_NAME END as ARM_NAME
	  from TB_REPORT_COLUMN_CODE cc
	  inner join @AA ats on ats.REPORT_COLUMN_CODE_ID = cc.REPORT_COLUMN_CODE_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = ats.A_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	inner join TB_ITEM_DOCUMENT id on ia.ITEM_ID = id.ITEM_ID
	inner join TB_ITEM_ATTRIBUTE_PDF pdf on id.ITEM_DOCUMENT_ID = pdf.ITEM_DOCUMENT_ID and ia.ITEM_ATTRIBUTE_ID = pdf.ITEM_ATTRIBUTE_ID
	left outer join TB_ITEM_ARM tia on ia.ITEM_ARM_ID = tia.ITEM_ARM_ID
	UNION
	SELECT cc.REPORT_COLUMN_ID, cc.REPORT_COLUMN_CODE_ID, a.ATTRIBUTE_ID, tt.ITEM_ID, id.DOCUMENT_TITLE
	, SUBSTRING(
					replace(id.DOCUMENT_TEXT,CHAR(13)+CHAR(10),CHAR(10)), TEXT_FROM + 1, TEXT_TO - TEXT_FROM
				 ) CODED_TEXT
	, CASE when tia.ARM_NAME is null then '' else tia.ARM_NAME END as ARM_NAME
	  from TB_REPORT_COLUMN_CODE cc
	inner join @AA ats on ats.REPORT_COLUMN_CODE_ID = cc.REPORT_COLUMN_CODE_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = ats.A_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM i on tt.ITEM_ID = i.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	inner join TB_ITEM_DOCUMENT id on ia.ITEM_ID = id.ITEM_ID
	inner join TB_ITEM_ATTRIBUTE_TEXT txt on id.ITEM_DOCUMENT_ID = txt.ITEM_DOCUMENT_ID and ia.ITEM_ATTRIBUTE_ID = txt.ITEM_ATTRIBUTE_ID
	left outer join TB_ITEM_ARM tia on ia.ITEM_ARM_ID = tia.ITEM_ARM_ID
	
	--sixth, items that do not have anything to report
	
	SELECT i.ITEM_ID, i.OLD_ITEM_ID, i.SHORT_TITLE from TB_ITEM i
	inner join @TT t on t.ITEM_ID = i.ITEM_ID
	where t.ITEM_ID not in
	(SELECT distinct tt.ITEM_ID
	from TB_REPORT_COLUMN_CODE cc
	INNER JOIN TB_ATTRIBUTE_SET tas ON (--Question reports fetch data about a given code children
										(tas.PARENT_ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 1) 
										OR 
										(tas.ATTRIBUTE_ID = cc.ATTRIBUTE_ID and @IS_QUESTION = 0)
									   )
		AND tas.SET_ID = cc.SET_ID
	INNER JOIN TB_ATTRIBUTE a ON a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID
	inner join TB_ITEM_ATTRIBUTE ia on a.ATTRIBUTE_ID = ia.ATTRIBUTE_ID
	inner join @TT tt on ia.ITEM_ID = tt.ITEM_ID
	inner join TB_ITEM_SET tis on cc.SET_ID = tis.SET_ID and tt.ITEM_ID = tis.ITEM_ID and tis.IS_COMPLETED = 1 and tis.ITEM_SET_ID = ia.ITEM_SET_ID
	where REPORT_ID = @REPORT_ID)
	ORDER BY 
		i.SHORT_TITLE -- we ignore the sorting order required for this report, sorting is done on c# side.
		, i.ITEM_ID
	--optional Seventh: get Title, Abstract and Year, only if some of this is needed.
	if (@FULL_DETAILS = 1)
	BEGIN
		select i.ITEM_ID, TITLE, ABSTRACT, [YEAR] from TB_ITEM i
			inner join @TT t on t.ITEM_ID = i.ITEM_ID
	END
END
GO