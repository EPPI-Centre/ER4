USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ReviewWorkAllocation]    Script Date: 14/11/2023 14:30:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ReviewWorkAllocation]
(
	@REVIEW_ID INT
)

As

declare @wa table (w_id int primary key, a_id bigint, s_id int, c_id int, TOTAL_ALLOCATION int , TOTAL_STARTED int )
declare @i table (w_id int, i_id bigint,  primary key(w_id, i_id))

insert into @wa select WORK_ALLOCATION_ID, ATTRIBUTE_ID, SET_ID, CONTACT_ID, 0, 0 from TB_WORK_ALLOCATION where REVIEW_ID = @REVIEW_ID
insert into @i select distinct w.w_id, tia.ITEM_ID from @wa w 
	INNER JOIN TB_ITEM_ATTRIBUTE tia on w.a_id = tia.ATTRIBUTE_ID
	inner join tb_item_set tis on tia.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1 and tia.ITEM_ID = tis.ITEM_ID
	inner join TB_ITEM_REVIEW ir on tis.ITEM_ID = ir.ITEM_ID and ir.REVIEW_ID = @REVIEW_ID and ir.IS_DELETED = 0

--insert into @AA select distinct ATTRIBUTE_ID from TB_WORK_ALLOCATION where REVIEW_ID = @REVIEW_ID

update @wa set TOTAL_ALLOCATION = tot from 
		(SELECT wa.w_id as w_id2, COUNT(DISTINCT i.i_id) as tot FROM @wa wa
		inner join @i i on i.w_id = wa.w_id
		group by wa.w_id) 
		as tmp
WHERE tmp.w_id2 = w_id

--update @wa set TOTAL_STARTED = tot from
--		(SELECT wa.w_id as w_id2, COUNT(DISTINCT i.i_id) as tot FROM @wa wa
--		INNER JOIN @i i on i.w_id = wa.w_id
--		INNER JOIN TB_ITEM_SET tis2 ON tis2.ITEM_ID = i.i_id AND tis2.CONTACT_ID = wa.c_id AND tis2.SET_ID = wa.s_id
--		group by wa.w_id) as tmp
--WHERE tmp.w_id2 = w_id

select CONTACT_NAME, c.CONTACT_ID, SET_NAME, s.SET_ID,
	wa.w_id as WORK_ALLOCATION_ID, ATTRIBUTE_NAME, a.ATTRIBUTE_ID ,
	wa.TOTAL_ALLOCATION, case when tmp.tot is null then 0 else tmp.tot end as TOTAL_STARTED
	from @wa wa
INNER JOIN TB_CONTACT c on wa.c_id = c.CONTACT_ID
Inner join TB_ATTRIBUTE a on a.ATTRIBUTE_ID = wa.a_id
inner join tb_set s on s.SET_ID = wa.s_id
left join (SELECT wa.w_id as w_id2, COUNT(DISTINCT i.i_id) as tot FROM @wa wa
		INNER JOIN @i i on i.w_id = wa.w_id
		INNER JOIN TB_ITEM_SET tis2 ON tis2.ITEM_ID = i.i_id AND tis2.CONTACT_ID = wa.c_id AND tis2.SET_ID = wa.s_id
		group by wa.w_id) as tmp on tmp.w_id2 = w_id
order by wa.w_id

GO

USE [Reviewer]
GO

If IndexProperty(Object_Id('[dbo].[TB_ITEM_SET]'), 'IX_TB_ITEM_SET_SET_ID', 'IndexID') Is Null
  begin
    CREATE NONCLUSTERED INDEX IX_TB_ITEM_SET_SET_ID
	ON [dbo].[TB_ITEM_SET] ([SET_ID])
  end
GO

USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_CopyReviewStep11]    Script Date: 14/11/2023 11:48:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   procedure [dbo].[st_CopyReviewStep11]
(	
	@CONTACT_ID int,
	@SOURCE_REVIEW_ID int,
	@DESTINATION_REVIEW_ID int,
	@SOURCE_SET_ID int,
	@RESULT nvarchar(50) output
)

As

SET NOCOUNT ON

--used to measure performance:
--declare @start datetime = getdate(), @InterimStart datetime = getdate(), @step datetime

declare @DESTINATION_SET_ID int
declare @DESTINATION_REVIEW_SET_ID int
declare @DESTINATION_SET_NAME nvarchar (255)
declare @NEW_OUTCOME_ID int
--declare @SOURCE_REVIEW_ID int
declare @NEW_SET_ID int
declare @NEW_ITEM_SET_ID int
set @RESULT = '0'


declare @tb_item_attribute table
(NEW_ITEM_ATTRIBUTE_ID bigint,  OLD_ITEM_ATTRIBUTE_ID bigint,
 NEW_ITEM_ID bigint,  OLD_ITEM_ID bigint,  NEW_ITEM_SET_ID bigint,
 OLD_ITEM_SET_ID bigint,  NEW_ATTRIBUTE_ID bigint,
 ORIGINAL_ATTRIBUTE_ID nvarchar(50), -- needs to be nvarchar to deal with ER3 attributes
 ADDITIONAL_TEXT nvarchar(max))
 
declare @tb_item_set table
(NEW_ITEM_SET_ID bigint,  EXAMPLE_ITEM_SET_ID bigint,
 NEW_ITEM_ID bigint,  EXAMPLE_ITEM_ID bigint,
 EXAMPLE_IS_COMPLETED bit,  NEW_CONTACT_ID int,
 EXAMPLE_IS_LOCKED bit)
 
declare @tb_new_items table
 (ITEM_ID bigint,
 SOURCE_ITEM_ID bigint)
 
declare @tb_item_outcome table
(new_outcome_id int,  old_outcome_id int,
 new_item_set_id int,  old_item_set_id int,
 outcome_type_id int,
 new_item_attribute_id_intervention bigint,  old_item_attribute_id_intervention bigint,
 new_item_attribute_id_control bigint,  old_item_attribute_id_control bigint,
 new_item_attribute_id_outcome bigint,  old_item_attribute_id_outcome nvarchar(255),
 outcome_title nvarchar(255),
 data1 float, data2 float, data3 float, data4 float,
 data5 float, data6 float, data7 float, data8 float,
 data9 float, data10 float, data11 float, data12 float,
 data13 float, data14 float,
 outcome_description nvarchar(4000))

declare @tb_item_outcome_attribute table
(new_item_outcome_attribute_id int,  old_item_outcome_attribute_id int,
 new_outcome_id int,  old_outcome_id int,
 new_attribute_id bigint,  old_attribute_id bigint,
 additional_text nvarchar(max))


--01 get a few variables
	--select @SOURCE_REVIEW_ID = EXAMPLE_NON_SHAREABLE_REVIEW_ID from TB_MANAGEMENT_SETTINGS
	select @NEW_SET_ID = SET_ID from sTB_SET 
	where OLD_GUIDELINE_ID = @SOURCE_SET_ID
	and SET_ID in (select SET_ID from sTB_REVIEW_SET where REVIEW_ID = @DESTINATION_REVIEW_ID)

--02 get old data from sTB_ITEM_SET and place in @tb_item_set
	insert into @tb_item_set (EXAMPLE_ITEM_SET_ID, EXAMPLE_ITEM_ID, EXAMPLE_IS_COMPLETED, NEW_CONTACT_ID, EXAMPLE_IS_LOCKED)
	select ITEM_SET_ID, ITEM_ID, IS_COMPLETED, CONTACT_ID, IS_LOCKED 
	from sTB_ITEM_SET tis 
	where SET_ID = @SOURCE_SET_ID
	
	--select * from @tb_item_set -- 1 OK
	
	--set @step = GETDATE()
	--select 'before 03 sets up a list', DATEDIFF(millisecond, @InterimStart, @step) as MS
	--set @InterimStart = GETDATE()
	
--03 sets up a list of the new items so we don't get cross-review contamination
	-- seems like an extra step but has little overhead and makes it easier
	-- for me to follow
	insert into @tb_new_items (ITEM_ID, SOURCE_ITEM_ID)
	select ITEM_ID, OLD_ITEM_ID from sTB_ITEM where ITEM_ID in 
	(select ITEM_ID from sTB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
	--select * from @tb_new_items -- 2 OK
	

--04 update @tb_item_set with the new item_id  
	update @tb_item_set
	set NEW_ITEM_ID = ITEM_ID
	from @tb_new_items
	where EXAMPLE_ITEM_ID = SOURCE_ITEM_ID
	
	--select * from @tb_item_set -- 3 OK
	
	--set @step = GETDATE()
	--select 'before 05 get old data', DATEDIFF(millisecond, @InterimStart, @step) as MS
	--set @InterimStart = GETDATE()

--05 get old data from sTB_ITEM_ATTRIBUTE and place in @tb_item_attribute
	insert into @tb_item_attribute (OLD_ITEM_ATTRIBUTE_ID, OLD_ITEM_ID, OLD_ITEM_SET_ID,
	 ORIGINAL_ATTRIBUTE_ID, ADDITIONAL_TEXT)
	select i_a.ITEM_ATTRIBUTE_ID, i_a.ITEM_ID, i_a.ITEM_SET_ID, i_a.ATTRIBUTE_ID, i_a.ADDITIONAL_TEXT  
	from sTB_ITEM_ATTRIBUTE i_a
	inner join sTB_ITEM_SET i_s on i_s.ITEM_SET_ID = i_a.ITEM_SET_ID
		and i_s.SET_ID = @SOURCE_SET_ID
	--select * from @tb_item_attribute -- 4 OK

	--set @step = GETDATE()
	--select 'before (prev) transaction', DATEDIFF(millisecond, @InterimStart, @step) as MS
	--set @InterimStart = GETDATE()



BEGIN TRY  --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
BEGIN TRANSACTION


	--set @step = GETDATE()
	--select 'before 1st cursor', DATEDIFF(millisecond, @InterimStart, @step) as MS
	--set @InterimStart = GETDATE()



--06 user cursors to place new row in TB_ITEM_SET, get the new ITEM_SET_ID and update @tb_item_set
	declare @WORKING_ITEM_ID int
	declare ITEM_ID_CURSOR cursor FORWARD_ONLY READ_ONLY for
	select NEW_ITEM_ID FROM @tb_item_set
	open ITEM_ID_CURSOR
	fetch next from ITEM_ID_CURSOR
	into @WORKING_ITEM_ID
	while @@FETCH_STATUS = 0
	begin
		-- set CONTACT_ID to newly created user account (rather than orginal coder)
		-- set IS_LOCKED to 0 as we want new user to experiment with the coding
		insert into sTB_ITEM_SET (ITEM_ID, SET_ID, CONTACT_ID, IS_LOCKED)	
		SELECT NEW_ITEM_ID, @NEW_SET_ID, @CONTACT_ID, 0 FROM @tb_item_set 
		WHERE NEW_ITEM_ID = @WORKING_ITEM_ID
		set @NEW_ITEM_SET_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_item_set set new_item_set_id = @NEW_ITEM_SET_ID
		WHERE NEW_ITEM_ID = @WORKING_ITEM_ID

		FETCH NEXT FROM ITEM_ID_CURSOR 
		INTO @WORKING_ITEM_ID
	END

	CLOSE ITEM_ID_CURSOR
	DEALLOCATE ITEM_ID_CURSOR
	
	--set @step = GETDATE()
	--select 'after 1st cursor', DATEDIFF(millisecond, @InterimStart, @step) as MS
	--set @InterimStart = GETDATE()


	--select * from sTB_ITEM_SET where ITEM_ID in -- 5 OK
	--(select ITEM_ID from sTB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	--select * from @tb_item_set -- 6 OK

--07 update @tb_item_attribute with NEW_ITEM_ID, NEW_ITEM_SET_ID, NEW_ATTRIBUTE_ID
	--update @tb_item_attribute
	--set NEW_ITEM_ID = 
	--(select NEW_ITEM_ID from @tb_item_set
	--where EXAMPLE_ITEM_ID = OLD_ITEM_ID)
	
	update t1
	set NEW_ITEM_ID = t2.NEW_ITEM_ID
	from @tb_item_attribute t1 inner join @tb_item_set t2
	on t2.EXAMPLE_ITEM_ID = t1.OLD_ITEM_ID
	
	--select * from @tb_item_attribute -- 7 OK
	
	--update @tb_item_attribute
	--set NEW_ITEM_SET_ID = 
	--(select NEW_ITEM_SET_ID from @tb_item_set
	--where EXAMPLE_ITEM_SET_ID = OLD_ITEM_SET_ID)
	
	update t1
	set NEW_ITEM_SET_ID = t2.NEW_ITEM_SET_ID
	from @tb_item_attribute t1 inner join @tb_item_set t2
	on t2.EXAMPLE_ITEM_ID = t1.OLD_ITEM_ID
	
	--select * from @tb_item_attribute -- 8 OK
	
	--update @tb_item_attribute
	--set NEW_ATTRIBUTE_ID = 
	--(select ATTRIBUTE_ID from sTB_ATTRIBUTE a
	--where a.OLD_ATTRIBUTE_ID = ORIGINAL_ATTRIBUTE_ID
	---- added JB 22/04/15
	--and a.CONTACT_ID = @CONTACT_ID)

	declare @step_table table (A_ID bigint, O_A_ID nvarchar(50))
	insert into @step_table select a.ATTRIBUTE_ID, OLD_ATTRIBUTE_ID from sTB_ATTRIBUTE a
	inner join sTB_ATTRIBUTE_SET tas on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and tas.SET_ID = @NEW_SET_ID

	update t1
	set NEW_ATTRIBUTE_ID = t2.A_ID
	from @tb_item_attribute t1 inner join @step_table t2
	on t2.O_A_ID = t1.ORIGINAL_ATTRIBUTE_ID
	--where t2.CONTACT_ID = @CONTACT_ID

	--update t1
	--set NEW_ATTRIBUTE_ID = t2.ATTRIBUTE_ID
	--from @tb_item_attribute t1 inner join sTB_ATTRIBUTE t2
	--on t2.OLD_ATTRIBUTE_ID = t1.ORIGINAL_ATTRIBUTE_ID
	--where t2.CONTACT_ID = @CONTACT_ID
	
	--select * from @tb_item_attribute -- 9 OK

--08 place new rows in TB_ITEM_ATTRIBUTE based on @tb_item_attribute
	insert into sTB_ITEM_ATTRIBUTE (ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT)
	select tb_i_a.NEW_ITEM_ID, tb_i_a.NEW_ITEM_SET_ID, tb_i_a.NEW_ATTRIBUTE_ID, tb_i_a.ADDITIONAL_TEXT 
	from @tb_item_attribute tb_i_a
	
--09 update TB_ITEM_SET with IS_COMPLETED values in @tb_item_set	
	UPDATE sTB_ITEM_SET
	SET IS_COMPLETED = i_s.EXAMPLE_IS_COMPLETED
	FROM @tb_item_set i_s
	where i_s.NEW_ITEM_SET_ID = ITEM_SET_ID
	and i_s.NEW_ITEM_ID in (select ITEM_ID from sTB_ITEM_REVIEW -- to restrict it to items in new review
	where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
	--select * from sTB_ITEM_SET where ITEM_ID in -- 10 OK
	--(select ITEM_ID from sTB_ITEM_REVIEW where REVIEW_ID = @DESTINATION_REVIEW_ID)
	
--10 fill up @tb_item_outcome
	insert into @tb_item_outcome (old_outcome_id, old_item_set_id, outcome_type_id,
	 old_item_attribute_id_intervention, old_item_attribute_id_control,
	 old_item_attribute_id_outcome, outcome_title, data1, data2, data3, data4, data5,
	 data6, data7, data8, data9, data10, data11, data12, data13,
	 data14, outcome_description)
	select i_c.OUTCOME_ID, i_c.ITEM_SET_ID, i_c.OUTCOME_TYPE_ID, i_c.ITEM_ATTRIBUTE_ID_INTERVENTION,
	i_c.ITEM_ATTRIBUTE_ID_CONTROL, i_c.ITEM_ATTRIBUTE_ID_OUTCOME, i_c.OUTCOME_TITLE,
	i_c.DATA1, i_c.DATA2, i_c.DATA3, i_c.DATA4, i_c.DATA5, i_c.DATA6, i_c.DATA7,
	i_c.DATA8, i_c.DATA9, i_c.DATA10, i_c.DATA11, i_c.DATA12, i_c.DATA13, i_c.DATA14,
	i_c.OUTCOME_DESCRIPTION
	from sTB_ITEM_OUTCOME i_c 
	inner join sTB_ITEM_SET i_s on i_s.ITEM_SET_ID = i_c.ITEM_SET_ID
	where i_s.SET_ID = @SOURCE_SET_ID
	
--11 update @tb_item_outcome with the 'new_' values	
	update t1 --new_item_set_id
	set new_item_set_id = t2.NEW_ITEM_SET_ID
	from @tb_item_outcome t1 inner join @tb_item_set t2
	on t2.EXAMPLE_ITEM_SET_ID = t1.old_item_set_id
	
	update t1 --new_item_attribute_id_intervention
	set new_item_attribute_id_intervention = t2.ATTRIBUTE_ID
	from @tb_item_outcome t1 inner join sTB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_intervention
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from sTB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
	update t1 --new_item_attribute_id_control
	set new_item_attribute_id_control = t2.ATTRIBUTE_ID
	from @tb_item_outcome t1 inner join sTB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_control
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from sTB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
	update t1 --new_item_attribute_id_outcome
	set new_item_attribute_id_outcome = t2.ATTRIBUTE_ID
	from @tb_item_outcome t1 inner join sTB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_item_attribute_id_outcome
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from sTB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)


	--set @step = GETDATE()
	--select 'before 2nd cursor', DATEDIFF(millisecond, @InterimStart, @step) as MS
	--set @InterimStart = GETDATE()
	
--12 copy @tb_item_outcome into sTB_ITEM_OUTCOME
	declare @WORKING_OUTCOME_ID int
	declare OUTCOME_ID_CURSOR cursor for
	select old_outcome_id FROM @tb_item_outcome
	open OUTCOME_ID_CURSOR
	fetch next from OUTCOME_ID_CURSOR
	into @WORKING_OUTCOME_ID
	while @@FETCH_STATUS = 0
	begin
		insert into sTB_ITEM_OUTCOME (ITEM_SET_ID, OUTCOME_TYPE_ID, 
		ITEM_ATTRIBUTE_ID_INTERVENTION, ITEM_ATTRIBUTE_ID_CONTROL, ITEM_ATTRIBUTE_ID_OUTCOME, 
		OUTCOME_TITLE, DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7,
		DATA8, DATA9, DATA10, DATA11, DATA12, DATA13, DATA14, OUTCOME_DESCRIPTION)	
		SELECT new_item_set_id, outcome_type_id, new_item_attribute_id_intervention, 
		new_item_attribute_id_control, new_item_attribute_id_outcome, outcome_title, 
		data1, data2, data3, data4, data5, data6, data7, data8, data9, data10, data11, 
		data12, data13, data14, outcome_description 
		FROM @tb_item_outcome 
		WHERE old_outcome_id = @WORKING_OUTCOME_ID
		set @NEW_OUTCOME_ID = CAST(SCOPE_IDENTITY() AS INT)
		
		update @tb_item_outcome set new_outcome_id = @NEW_OUTCOME_ID
		WHERE old_outcome_id = @WORKING_OUTCOME_ID

		FETCH NEXT FROM OUTCOME_ID_CURSOR 
		INTO @WORKING_OUTCOME_ID
	END

	CLOSE OUTCOME_ID_CURSOR
	DEALLOCATE OUTCOME_ID_CURSOR

	--set @step = GETDATE()
	--select 'After 2nd cursor', DATEDIFF(millisecond, @InterimStart, @step) as MS
	--set @InterimStart = GETDATE()

--13 fill up @tb_item_outcome_attribute
	insert into @tb_item_outcome_attribute (old_item_outcome_attribute_id,
	 old_outcome_id, old_attribute_id, additional_text)
	select ITEM_OUTCOME_ATTRIBUTE_ID, OUTCOME_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT
	from sTB_ITEM_OUTCOME_ATTRIBUTE i_o_a
	inner join @tb_item_outcome i_o on i_o.OLD_OUTCOME_ID = i_o_a.OUTCOME_ID	

--14 update tb_item_outcome_attribute with the 'new_' values
	update t1 --new_outcome_id
	set new_outcome_id = t2.new_outcome_id
	from @tb_item_outcome_attribute t1 inner join @tb_item_outcome t2
	on t2.old_outcome_id = t1.old_outcome_id
	
	update t1 --new_attribute_id
	set new_attribute_id = t2.ATTRIBUTE_ID
	from @tb_item_outcome_attribute t1 inner join sTB_ATTRIBUTE t2
	on t2.OLD_ATTRIBUTE_ID = t1.old_attribute_id
	where t2.ATTRIBUTE_ID in (select ATTRIBUTE_ID from sTB_ATTRIBUTE_SET
	where SET_ID = @NEW_SET_ID)
	
--15 copy @tb_item_outcome_attribute into sTB_ITEM_OUTCOME_ATTRIBUTE
	insert into sTB_ITEM_OUTCOME_ATTRIBUTE (OUTCOME_ID, ATTRIBUTE_ID,
	ADDITIONAL_TEXT)
	select new_outcome_id, new_attribute_id, additional_text from @tb_item_outcome_attribute
	
--16 place the outcome_id values into TB_REVIEW_COPY_DATA for later use
	insert into TB_REVIEW_COPY_DATA (NEW_REVIEW_ID, OLD_OUTCOME_ID, NEW_OUTCOME_ID)
	select @DESTINATION_REVIEW_ID, old_outcome_id, new_outcome_id  
	from @tb_item_outcome

	--set @step = GETDATE()
	--select 'Before end of Transaction', DATEDIFF(millisecond, @InterimStart, @step) as MS
	--set @InterimStart = GETDATE()
	
COMMIT TRANSACTION

--set @step = GETDATE()
--select 'After end of Transaction', DATEDIFF(millisecond, @InterimStart, @step) as MS
--set @InterimStart = GETDATE()

END TRY

BEGIN CATCH
IF (@@TRANCOUNT > 0)
begin	
ROLLBACK TRANSACTION
set @RESULT = 'Unable to copy the coding from the old review to the new review'
end
END CATCH

--set @step = GETDATE()
--select 'END', DATEDIFF(millisecond, @Start, @step) as MS
--set @InterimStart = GETDATE()

SET NOCOUNT OFF
RETURN

GO
USE [Reviewer]
GO