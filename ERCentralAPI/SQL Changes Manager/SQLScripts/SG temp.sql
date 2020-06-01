

--Use Reviewer
--GO

--declare @cleanup bit = 0

-----CHANGE this: create groups based on Items table, one group per existing combination of Reviewer1, Reviewer2, Reviewer3 [...] this
---- may allow to "distribute evenly", minimise the number of groups AS well as adding a user in two columns...


--declare @REVIEW_ID int = 7
--	,@IsPreview int = 0
--	,@OneGroupPerPerson bit = 0
--	,@PeoplePerItem int = 2
--	,@ReviewersIds varchar(8000) = '1214,1566,1512,1800,1801'
--	,@ReviewerNames varchar(8000) = 'Sergio Graziosi,Sergio Graziosi1,Steven Startle,Zak Ghouze,Zak Test1'
--	,@ItemsPerEachReviewer varchar(8000) = '1079,778,538,519,200'
--	,@GroupsPrefix varchar(100) = 'The Big Test'
--	,@ATTRIBUTE_ID_FILTER bigint = 84350
--	,@FILTER_TYPE NVARCHAR(255) = 'All with this code'
--	,@INCLUDED BIT = 1
--	,@PercentageOfWholePot int = 100
--    ,@NumberOfItemsToAssign int = 1557
--	,@SET_ID_FILTER INT = 0
--	,@Destination_set_ID int = 1834
--	,@Destination_Attribute_ID bigint = 116537
--	,@Requestor_id int = 1214
--	,@Work_to_do_setID int = 1851
--	,@NumberOfAffectedItems int = 0 --OUTPUT

----declare @REVIEW_ID int = 7
----	,@IsPreview int = 2
----	,@OneGroupPerPerson bit = 1
----	,@PeoplePerItem int = 2
----	,@ReviewersIds varchar(8000) = '1214,1566,1512,1800,1801'
----	,@ReviewerNames varchar(8000) = 'Sergio Graziosi,Sergio Graziosi1,Steven Startle,Zak Ghouze,Zak Test1'
----	,@ItemsPerEachReviewer varchar(8000) = '779,778,538,519,500'
----	,@GroupsPrefix varchar(100) = 'The Big Test'
----	,@ATTRIBUTE_ID_FILTER bigint = 84350
----	,@FILTER_TYPE NVARCHAR(255) = 'All with this code'
----	,@INCLUDED BIT = 1
----	,@PercentageOfWholePot int = 100
----    ,@NumberOfItemsToAssign int = 15571
----	,@SET_ID_FILTER INT = 0
----	,@Destination_set_ID int = 1834
----	,@Destination_Attribute_ID bigint = 116537
----	,@Requestor_id int = 1214
----	,@Work_to_do_setID int = 1851
----  ,@NumberOfAffectedItems int = 0 --OUTPUT

----declare @REVIEW_ID int = 99
----	,@IsPreview int = 2
----	,@OneGroupPerPerson bit = 0 
----	,@PeoplePerItem int = 3
----	,@ReviewersIds varchar(8000) = '1273,1266,1324,347,346,559,826,1275,649,1214'
----	,@ReviewerNames varchar(8000) = 'James Thomas,Ginny Brunton,Katy Sutcliffe,Katherine Twamley,Kate Hinds,Kelly Dickson,Jenny Caird,Rebecca Rees,Jeff Brunton,Sergio Graziosi'
----	,@ItemsPerEachReviewer varchar(8000) = '7437,200,7238,350,1000,13525,3719,3719,3719,3718'
----	,@GroupsPrefix varchar(100) = 'The Big Test'
----	,@ATTRIBUTE_ID_FILTER bigint = 72146
----	,@FILTER_TYPE NVARCHAR(255) = 'All with this code'
----	,@INCLUDED BIT = 1
----	,@PercentageOfWholePot int = 100
----    ,@NumberOfItemsToAssign int = 14875
----	,@SET_ID_FILTER INT = 0
----	,@Destination_set_ID int = 1880
----	,@Destination_Attribute_ID bigint = 0
----	,@Requestor_id int = 1214
----	,@work_to_do_setID int = 1746
----	,@NumberOfAffectedItems int = 0 --OUTPUT

	
--declare @ppl table (IDX int, C_ID int primary key, C_name nvarchar(255), N_items int, Remaining int, destination bigint null, work_alloc_id int null) --we need IDX to keep things in order

--declare @Items table (IID bigint, ui uniqueidentifier, Reviewer1 int null, Reviewer2 int null, Reviewer3 int null, destination bigint null, work_alloc_id int null, primary key(IID, ui))
--declare @SharedPots table (Reviewer1 int null, Reviewer2 int null, Reviewer3 int null, destination bigint null, work_alloc_id int null)


----FIRST build table for people and N of items they should get.
--insert into @ppl select a.idx, a.value, b.value, c.value, c.value, null, null
--	from dbo.fn_Split_int(@ReviewersIds, ',') a 
--	inner join TB_REVIEW_CONTACT rc on a.value = rc.CONTACT_ID and rc.REVIEW_ID = @REVIEW_ID
--	inner join dbo.fn_Split(@ReviewerNames, ',') b on b.idx = a.idx
--	inner join dbo.fn_Split_int(@ItemsPerEachReviewer, ',') c on c.idx = b.idx
----select * from @ppl order by IDX

----SECOND get the items we need, with NEWID() for random order
----THIS USES the SAME logic as st_RandomAllocate!!

--IF (@FILTER_TYPE = 'No code / coding tool filter')
--BEGIN
--INSERT INTO @Items(IID, ui)
--	SELECT TOP (@PercentageOfWholePot) PERCENT ITEM_ID, NEWID() as uuu FROM TB_ITEM_REVIEW
--		WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--			AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
--			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
--		ORDER BY uuu
--END
	
---- FILTER BY ALL WITH THIS ATTRIBUTE
--IF (@FILTER_TYPE = 'All with this code')
--BEGIN
--INSERT INTO @Items(IID, ui)
--	SELECT TOP (@PercentageOfWholePot) PERCENT TB_ITEM_ATTRIBUTE.ITEM_ID, NEWID() as uuu FROM TB_ITEM_ATTRIBUTE
--		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
--		WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER
--			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
--			AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
--			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
--		ORDER BY uuu
--END
		
---- FILTER BY ALL WITHOUT THIS ATTRIBUTE
--IF (@FILTER_TYPE = 'All without this code')
--BEGIN
--INSERT INTO @Items(IID, ui)
--	SELECT TOP (@PercentageOfWholePot) PERCENT ITEM_ID, NEWID() as uuu  FROM TB_ITEM_REVIEW
--		WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
--			AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
--			AND NOT ITEM_ID IN
--			(
--				SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
--				INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--				INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
--				WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
--			)
--	ORDER BY uuu
--END
	
---- FILTER BY 'ALL WITHOUT ANY CODES FROM THIS SET'
--IF (@FILTER_TYPE = 'All without any codes from this set')
--BEGIN
--INSERT INTO @Items(IID, ui)
--	SELECT TOP (@PercentageOfWholePot) PERCENT ITEM_ID, NEWID() as uuu  FROM TB_ITEM_REVIEW
--		WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--		AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
--		AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
--		AND NOT ITEM_ID IN
--		(
--		SELECT TB_ITEM_SET.ITEM_ID FROM TB_ITEM_SET
--			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_SET.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--			WHERE TB_ITEM_SET.SET_ID = @SET_ID_FILTER AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
--		)
--	ORDER BY uuu
--END
	
---- FILTER BY 'ALL WITH ANY CODES FROM THIS SET'
--IF (@FILTER_TYPE = 'All with any codes from this set')
--BEGIN
--INSERT INTO @Items(IID, ui)
--	SELECT TOP (@PercentageOfWholePot) PERCENT TB_ITEM_REVIEW.ITEM_ID, NEWID() as uuu  FROM TB_ITEM_REVIEW
--		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
--			AND TB_ITEM_SET.SET_ID = @SET_ID_FILTER
--			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
--		WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
--		AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
--		AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
--	ORDER BY uuu
--END
--select @NumberOfAffectedItems =  count(*) from @Items
	
--	--STOP HERE if we're only getting a preview...
--	IF @IsPreview = 1
--	BEGIN
--		--do something, while we can't "return"
--		delete from @ppl
--		delete from @Items
--		--set @maxTries = 0
--		select 'End here: preview!', @NumberOfAffectedItems
--	END

--	--Sanity check did we get the right number of items??
--	--First check is: we got N items from DB, does this correspond to the expected N?
--	--Second check is: does the N of items to be coded by each person add up to the expected figure?
--	--happens AFTER [IF @IsPreview = 1] because in that case, we are merely collecting the figures.
--	IF @NumberOfItemsToAssign != @NumberOfAffectedItems OR (select (Sum(N_items)) from @ppl) != @NumberOfItemsToAssign * @PeoplePerItem
--	BEGIN
--		--do something, this isn't right!
--		delete from @ppl
--		delete from @Items
--		--set @maxTries = 0
--		select 'Aborting!'
		
--	END
	
----select * from @Items order by ui

----NOW we need to add reviewers to each item.
----first column always gets filled.
--declare @maxTries int = (select count(*) from @ppl), @tries int = 0
--declare @CurrentPerson int, @remaining int
--while ((select count(*) from @Items where Reviewer1 is null) > 0 AND @tries <= @maxTries)
--BEGIN
--	set @CurrentPerson = (select top(1) C_ID from @ppl where Remaining > 0 order by IDX)
--	set @remaining = (select top(1) Remaining from @ppl where C_ID = @CurrentPerson)
	
--	update A set Reviewer1 = @CurrentPerson
--	from (select top(@remaining) * from @Items where Reviewer1 is null order by ui) as A
--	set @remaining = @remaining - @@ROWCOUNT
--	update @ppl set remaining = @remaining where C_ID = @CurrentPerson
--	set @tries = @tries + 1
--END
----select reviewer1, p.C_name, count(ui), Remaining from @Items i inner join @ppl p on i.Reviewer1 = p.C_ID	group by reviewer1, p.C_name, Remaining order by reviewer1 

----Fill in the second colum, if needed
--IF @PeoplePerItem > 1
--BEGIN
--	set @tries = 0
--	while ((select count(*) from @Items where Reviewer2 is null) > 0 AND @tries <= @maxTries)
--	BEGIN
--		set @CurrentPerson = (select top(1) C_ID from @ppl where Remaining > 0 order by IDX)
--		set @remaining = (select top(1) Remaining from @ppl where C_ID = @CurrentPerson)
--		update A set Reviewer2 = @CurrentPerson
--		from (select top(@remaining) * from @Items where Reviewer2 is null order by ui) as A
--		set @remaining = @remaining - @@ROWCOUNT
--		update @ppl set remaining = @remaining where C_ID = @CurrentPerson
--		set @tries = @tries + 1
--	END
--	select reviewer2, p.C_name, count(ui), Remaining from @Items i inner join @ppl p on i.Reviewer2 = p.C_ID group by reviewer2, p.C_name, Remaining order by reviewer2 
--END
----Fill in the third colum, if needed
--IF @PeoplePerItem > 2
--BEGIN
--	set @tries = 0
--	while ((select count(*) from @Items where Reviewer3 is null) > 0 AND @tries <= @maxTries)
--	BEGIN
--		set @CurrentPerson = (select top(1) C_ID from @ppl where Remaining > 0 order by IDX)
--		set @remaining = (select top(1) Remaining from @ppl where C_ID = @CurrentPerson)
--		update A set Reviewer3 = @CurrentPerson
--		from (select top(@remaining) * from @Items where Reviewer3 is null order by ui) as A
--		set @remaining = @remaining - @@ROWCOUNT
--		update @ppl set remaining = @remaining where C_ID = @CurrentPerson
--		set @tries = @tries + 1
--	END
--	select reviewer3, p.C_name, count(ui), Remaining from @Items i inner join @ppl p on i.Reviewer3 = p.C_ID group by reviewer3, p.C_name, Remaining order by reviewer3 
--END
--	--SECOND kind of preview: tell me what you'd do in detail...
--	IF @IsPreview = 2 
--	BEGIN
--		--do something, while we can't "return"
--		if @OneGroupPerPerson = 0
--		begin
--			select distinct Reviewer1, Reviewer2, Reviewer3
--				,Case WHEN Reviewer3 is null and Reviewer2 is null then 'One group (' + p1.C_name + ') One allocation'
--					WHEN  Reviewer3 is null then 'One group (' + p1.C_name + ', ' + p2.C_name  + ') Two allocations'
--					ELSE 'One group (' + p1.C_name + ', ' + p2.C_name  + ', ' + p3.C_name  + ') Three allocations'
--				END
--				,Case WHEN Reviewer3 is null and Reviewer2 is null then 1
--					WHEN  Reviewer3 is null then 2
--					ELSE 3
--				END as [Number of Allocations]
--			from @Items i 
--			inner join @ppl p1 on i.Reviewer1 = p1.C_ID
--			left join @ppl p2 on i.Reviewer2 = p2.C_ID
--			left join @ppl p3 on i.Reviewer3 = p3.C_ID
--		end
--		else
--		begin
--				--select * from @Items order by ui
--				select C_name, 'First reviewer', count(*) [Items in allocation] 
--				from @ppl p 
--				inner join  @Items i  on p.C_ID = i.Reviewer1
--				group by C_name
--				UNION
--				select C_name, 'Second reviewer', count(*) [Items in allocation] 
--				from @ppl p 
--				inner join  @Items i  on p.C_ID = i.Reviewer2
--				group by C_name
--				UNION
--				select C_name, 'Third reviewer', count(*) [Items in allocation] 
--				from @ppl p 
--				inner join  @Items i  on p.C_ID = i.Reviewer3
--				group by C_name
				 
--			--END
--		end
--		delete from @ppl
--		delete from @Items
--		set @maxTries = 0
--		select 'End here: preview!', @NumberOfAffectedItems
--	END

----select * from @ppl order by IDX
----select * from @Items order by ui

----We can NOW Create new codes and allocations, one per person! We will keep track of the new Attribute_ID in the ppl table...
----
----LOGIC is again inspired by st_RandomAllocate!!
--	SET XACT_ABORT ON  
--	BEGIN TRAN --all or nothing, see: https://docs.microsoft.com/en-us/sql/t-sql/language-elements/rollback-transaction-transact-sql?view=sql-server-2017
--		DECLARE @MAX_INDEX INT = 0 --used for the order of new attributes create
--		IF (@MAX_INDEX IS NULL) SET @MAX_INDEX = 0
--		SELECT @MAX_INDEX = MAX(ATTRIBUTE_ORDER) + 1 FROM TB_ATTRIBUTE_SET WHERE PARENT_ATTRIBUTE_ID = @Destination_Attribute_ID AND SET_ID = @Destination_set_ID
--		DECLARE @DUMMY_OUTPUT BIGINT = NULL -- WE'll use it once without looking, then again to get the work allocation ID
--		DECLARE @GroupName nvarchar(255), @GROUP1 bigint
--		set @tries = 0
--		IF @OneGroupPerPerson = 1
--			--We are using the strategy where each person gets their own allocation code and their own coding assignment
--			BEGIN
--			--ALL insertions happen within this loop:
--			--new Attributes, new ITEM_SET (as/if needed), new ITEM_ATTRIBUTE, new coding assignments (in this order)
--			while (@tries < @maxTries) --we loop, just as many times as there are rows in the ppl table
--			BEGIN
--				set @CurrentPerson = (select top(1) C_ID from @ppl where destination is null order by IDX)
--				set @GroupName = @GroupsPrefix + ' (' + (select C_name from @ppl where C_ID = @CurrentPerson) + ')'
--				--Create the allocation codes
--				EXECUTE st_AttributeSetInsert @Destination_set_ID, @Destination_Attribute_ID, 1, '', @MAX_INDEX
--						, @GroupName, '', @Requestor_id, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP1 OUTPUT
--				update @ppl set destination = @GROUP1 where C_ID = @CurrentPerson

--				--Create ITEM_SET records, we can do all of this in one go, so only the first time round
--				IF @tries = 0
--				BEGIN
--					INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID) 
--						SELECT ids.IID, @Destination_set_ID, 'True', @Requestor_id FROM @Items ids
--						EXCEPT
--						SELECT ITEM_ID, @Destination_set_ID, 'True', @Requestor_id FROM TB_ITEM_SET WHERE TB_ITEM_SET.SET_ID = @Destination_set_ID AND IS_COMPLETED = 'True'
--				END 
--				--We can now assign items to codes
--				INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
--						SELECT ITEM_ID, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @Items ids
--						INNER JOIN @ppl p on 
--							(ids.Reviewer1 = @CurrentPerson OR ids.Reviewer2 = @CurrentPerson OR ids.Reviewer3 = @CurrentPerson)
--						INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.IID AND p.destination = @GROUP1
--						WHERE TB_ITEM_SET.SET_ID = @Destination_set_ID AND TB_ITEM_SET.IS_COMPLETED = 1
--				SET @MAX_INDEX = @MAX_INDEX + 1
			

--				--phew. We can finish by creating the coding assignment for @CurrentPerson
--				EXECUTE [dbo].[st_ReviewWorkAllocationInsert] 
--				   @REVIEW_ID
--				  ,@CurrentPerson
--				  ,@Work_to_do_setID
--				  ,@GROUP1
--				  ,@NEW_WORK_ALLOCATION_ID = @DUMMY_OUTPUT OUTPUT
--				update @ppl set work_alloc_id = @DUMMY_OUTPUT where C_ID = @CurrentPerson
--				set @tries = @tries + 1
--			END
--		END
--		ELSE BEGIN
--			-- we will "share" allocation codes between people (more allocations need to be created!)
--			-- basic logic is: within @items table, we need one pot for each combination of (Reviewer1, Reviewer2, Reviewer3) we can store single combinations in @SharedPots 
--			INSERT into @SharedPots select distinct Reviewer1,Reviewer2,Reviewer3, null,null from @Items
			
--			select * from @SharedPots
--			select @maxTries = count(*) from @SharedPots
--			select @maxTries
--			declare @group_desc nvarchar(2000)
--			while (@tries < @maxTries) --we loop, just as many times as there are rows in the @SharedPots table
--			BEGIN
--				--We'll create a shared pot for each row in @SharedPots, assign it to all people in that row.
--				select @group_desc = (select top(1) 'Used for: ' + p.C_name +
--															CASE WHEN Reviewer2 IS null then '' else  ', ' + p2.C_name END
--															+ 
--															CASE when Reviewer3 IS null then '' else  ', ' + p3.C_name END
--															+ '.'
--														from @SharedPots sp
--															inner join @ppl p on sp.Reviewer1 = p.C_ID
--															left join @ppl p2 on sp.Reviewer2 = p2.C_ID
--															left join @ppl p3 on sp.Reviewer3 = p3.C_ID
--														where sp.destination is null)
--				select @GroupName = @GroupsPrefix + ' (group ' + CAST((@tries+1) as varchar(10)) +')'
--				--Create the allocation codes
--				EXECUTE st_AttributeSetInsert @Destination_set_ID, @Destination_Attribute_ID, 1, @group_desc, @MAX_INDEX
--						, @GroupName, @group_desc, @Requestor_id, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP1 OUTPUT
--				update a set destination = @GROUP1
--					from (select top(1) * from @SharedPots where destination is null) as a
				
--				select * from @SharedPots
				
--				--Create ITEM_SET records, we can do all of this in one go, so only the first time round
--				IF @tries = 0
--				BEGIN
--					INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID) 
--						SELECT ids.IID, @Destination_set_ID, 'True', @Requestor_id FROM @Items ids
--						EXCEPT
--						SELECT ITEM_ID, @Destination_set_ID, 'True', @Requestor_id FROM TB_ITEM_SET WHERE TB_ITEM_SET.SET_ID = @Destination_set_ID AND IS_COMPLETED = 'True'
--				END 

--				--We can now assign items to codes
--				INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
--						SELECT ITEM_ID, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @Items ids
--						INNER JOIN @SharedPots sp on 
--							sp.destination = @GROUP1
--							and
--							(
--								ids.Reviewer1 = sp.Reviewer1 
--								AND (ids.Reviewer2 = sp.Reviewer2 OR (ids.Reviewer2 is null and sp.Reviewer2 is null)) 
--								AND (ids.Reviewer3 = sp.Reviewer3 OR (ids.Reviewer3 is null and sp.Reviewer3 is null))
--							)
--						INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.IID
--						WHERE TB_ITEM_SET.SET_ID = @Destination_set_ID AND TB_ITEM_SET.IS_COMPLETED = 1
--				SET @MAX_INDEX = @MAX_INDEX + 1
				
--				--phew. We can finish by creating the coding assignment for each person in shared port
--				select @CurrentPerson = Reviewer1 from @SharedPots where destination = @GROUP1
--				EXECUTE [dbo].[st_ReviewWorkAllocationInsert] 
--				   @REVIEW_ID
--				  ,@CurrentPerson
--				  ,@Work_to_do_setID
--				  ,@GROUP1
--				  ,@NEW_WORK_ALLOCATION_ID = @DUMMY_OUTPUT OUTPUT
--				update @SharedPots set work_alloc_id = @DUMMY_OUTPUT where destination = @GROUP1

--				select @CurrentPerson = Reviewer2 from @SharedPots where destination = @GROUP1
--				IF @CurrentPerson is not null
--				begin
--						EXECUTE [dbo].[st_ReviewWorkAllocationInsert] 
--						   @REVIEW_ID
--						  ,@CurrentPerson
--						  ,@Work_to_do_setID
--						  ,@GROUP1
--						  ,@NEW_WORK_ALLOCATION_ID = @DUMMY_OUTPUT OUTPUT

--						select @CurrentPerson = Reviewer3 from @SharedPots where destination = @GROUP1
--						IF @CurrentPerson is not null
--						begin
--							EXECUTE [dbo].[st_ReviewWorkAllocationInsert] 
--							   @REVIEW_ID
--							  ,@CurrentPerson
--							  ,@Work_to_do_setID
--							  ,@GROUP1
--							  ,@NEW_WORK_ALLOCATION_ID = @DUMMY_OUTPUT OUTPUT
--						END
--				END
--				set @tries = @tries + 1
--			END
--			select 'todo'
--		END
--	COMMIT TRAN
--select * from @ppl order by IDX --return back a summary of what was done, not sure we'll use it.



-------------------------------------------------------------------------------------------------------
-----END of FUTURE SP, BEGIN CLEANUP-------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------

--if @cleanup = 1
--BEGIN
--	set @tries = 0
--	IF @OneGroupPerPerson = 1
--		--We are using the strategy where each person gets their own allocation code and their own coding assignment
--		BEGIN
--			while (@tries < @maxTries) --we loop, just as many times as there are rows in the ppl table
--			BEGIN
--				set @CurrentPerson = (select top(1) C_ID from @ppl where work_alloc_id is NOT null order by IDX)
--				set @DUMMY_OUTPUT = (select work_alloc_id from @ppl where C_ID = @CurrentPerson)

--				EXECUTE [dbo].[st_ReviewWorkAllocationDelete] 
--				   @Work_allocation_id = @DUMMY_OUTPUT
--				  ,@REVIEW_ID = @REVIEW_ID
--				set @GROUP1 = (select destination from @ppl where C_ID = @CurrentPerson)
--				declare @ASI bigint, @PAI bigint
--				select @ASI = tas.ATTRIBUTE_SET_ID, @PAI = tas.PARENT_ATTRIBUTE_ID, @MAX_INDEX = tas.ATTRIBUTE_ORDER from TB_ATTRIBUTE_SET tas inner join @ppl p on p.C_ID = @CurrentPerson and tas.ATTRIBUTE_ID = p.destination
--				EXECUTE [dbo].[st_AttributeSetDelete] 
--				   @ATTRIBUTE_SET_ID = @ASI
--				  ,@ATTRIBUTE_ID = @GROUP1
--				  ,@PARENT_ATTRIBUTE_ID = @PAI
--				  ,@ATTRIBUTE_ORDER = @MAX_INDEX
--				  ,@REVIEW_ID = @REVIEW_ID
--				UPDATE @ppl set work_alloc_id = null where C_ID = @CurrentPerson
--				set @tries = @tries + 1
--			END
--		END
--		ELSE
--		BEGIN
--			select 'To do: cleanup for shared pots'
--		END
	
--END
--GO






Use Reviewer
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		SG
-- Create date: <Create Date,,>
-- Description:	Used to create Coding assignments, given "abstract" requirements generated on client side.
-- WILL CREATE codes and coding Assignments, or generate previews, depending on parameters.
-- =============================================
CREATE OR ALTER PROCEDURE st_ReviewWorkAllocationCheckOrInsertFromWizard
	-- Add the parameters for the stored procedure here
	@REVIEW_ID int 
	,@IsPreview int = 1
	,@Requestor_id int 
	,@FILTER_TYPE NVARCHAR(255)
	,@ATTRIBUTE_ID_FILTER bigint
	,@SET_ID_FILTER INT = 0
	,@Destination_Attribute_ID bigint = -1
	,@Destination_set_ID int = -1
	,@PercentageOfWholePot int
	,@INCLUDED BIT
	,@Work_to_do_setID int = -1
	,@OneGroupPerPerson bit = 0
	,@PeoplePerItem int = 1
	,@ReviewersIds varchar(8000) = ''
	,@ReviewerNames varchar(8000) = ''
	,@ItemsPerEachReviewer varchar(8000) = ''
	,@GroupsPrefix varchar(100) = ''
    ,@NumberOfItemsToAssign int = 0
	,@NumberOfAffectedItems int = 0 OUTPUT
	,@Success bit = 1 OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	set @Success = 1
	declare @ppl table (IDX int, C_ID int primary key, C_name nvarchar(255), N_items int, Remaining int, destination bigint null, work_alloc_id int null) --we need IDX to keep things in order
	declare @Items table (IID bigint, ui uniqueidentifier, Reviewer1 int null, Reviewer2 int null, Reviewer3 int null, destination bigint null, work_alloc_id int null, primary key(IID, ui))
	declare @SharedPots table (Reviewer1 int null, Reviewer2 int null, Reviewer3 int null, destination bigint null, work_alloc_id int null)

	--FIRST build table for people and N of items they should get.
	insert into @ppl select a.idx, a.value, b.value, c.value, c.value, null, null
		from dbo.fn_Split_int(@ReviewersIds, ',') a 
		inner join TB_REVIEW_CONTACT rc on a.value = rc.CONTACT_ID and rc.REVIEW_ID = @REVIEW_ID
		inner join dbo.fn_Split(@ReviewerNames, ',') b on b.idx = a.idx
		inner join dbo.fn_Split_int(@ItemsPerEachReviewer, ',') c on c.idx = b.idx
	
	--SECOND: get the items we need, with NEWID() for random order
	--THIS USES the SAME logic as st_RandomAllocate!!
	IF (@FILTER_TYPE = 'No code / coding tool filter' OR @FILTER_TYPE = 'No code / code set filter')
	BEGIN
	INSERT INTO @Items(IID, ui)
		SELECT TOP (@PercentageOfWholePot) PERCENT ITEM_ID, NEWID() as uuu FROM TB_ITEM_REVIEW
			WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
			ORDER BY uuu
	END	
	-- FILTER BY ALL WITH THIS ATTRIBUTE
	ELSE IF (@FILTER_TYPE = 'All with this code')
	BEGIN
	INSERT INTO @Items(IID, ui)
		SELECT TOP (@PercentageOfWholePot) PERCENT TB_ITEM_ATTRIBUTE.ITEM_ID, NEWID() as uuu FROM TB_ITEM_ATTRIBUTE
			INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
				AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			ORDER BY uuu
	END
	-- FILTER BY ALL WITHOUT THIS ATTRIBUTE
	ELSE IF (@FILTER_TYPE = 'All without this code')
	BEGIN
	INSERT INTO @Items(IID, ui)
		SELECT TOP (@PercentageOfWholePot) PERCENT ITEM_ID, NEWID() as uuu  FROM TB_ITEM_REVIEW
			WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
				AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
				AND NOT ITEM_ID IN
				(
					SELECT TB_ITEM_ATTRIBUTE.ITEM_ID FROM TB_ITEM_ATTRIBUTE
					INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
					INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
					WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = @ATTRIBUTE_ID_FILTER AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
				)
		ORDER BY uuu
	END
	-- FILTER BY 'ALL WITHOUT ANY CODES FROM THIS SET'
	ELSE IF (@FILTER_TYPE = 'All without any codes from this set' OR @FILTER_TYPE = 'All without any codes from this coding tool')
	BEGIN
	INSERT INTO @Items(IID, ui)
		SELECT TOP (@PercentageOfWholePot) PERCENT ITEM_ID, NEWID() as uuu  FROM TB_ITEM_REVIEW
			WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
			AND NOT ITEM_ID IN
			(
			SELECT TB_ITEM_SET.ITEM_ID FROM TB_ITEM_SET
				INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_SET.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
				WHERE TB_ITEM_SET.SET_ID = @SET_ID_FILTER AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			)
		ORDER BY uuu
	END
	-- FILTER BY 'ALL WITH ANY CODES FROM THIS SET'
	ELSE IF (@FILTER_TYPE = 'All with any codes from this set' OR @FILTER_TYPE = 'All with any codes from this coding tool')
	BEGIN
	INSERT INTO @Items(IID, ui)
		SELECT TOP (@PercentageOfWholePot) PERCENT TB_ITEM_REVIEW.ITEM_ID, NEWID() as uuu  FROM TB_ITEM_REVIEW
			INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = TB_ITEM_REVIEW.ITEM_ID
				AND TB_ITEM_SET.SET_ID = @SET_ID_FILTER
				AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			WHERE TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
			AND TB_ITEM_REVIEW.IS_INCLUDED = @INCLUDED
			AND TB_ITEM_REVIEW.IS_DELETED = 'FALSE'
		ORDER BY uuu
	END
	select @NumberOfAffectedItems =  count(*) from @Items --Used to check if the expected numbers are still valid. It is also an OUTPUT param, so rest of the APP will know how many

	--STOP HERE if we're only getting a preview...
	IF @IsPreview = 1
	BEGIN
		delete from @ppl
		delete from @Items
		return 
	END

	--Sanity check did we get the right number of items??
	--First check is: we got N items from DB, does this correspond to the expected N?
	--Second check is: does the N of items to be coded by each person add up to the expected figure?
	--happens AFTER [IF @IsPreview = 1] because in that case, we are merely collecting the figures.
	IF @NumberOfItemsToAssign != @NumberOfAffectedItems OR (select (Sum(N_items)) from @ppl) != @NumberOfItemsToAssign * @PeoplePerItem
	BEGIN
		--do something, this isn't right!
		delete from @ppl
		delete from @Items
		set @Success = 0 --this failed, user needs to recalculate amounts, as number of items in pot has probably changed :-(
		return
	END

	--NOW we need to add reviewers to each item in our table variable @Items.
	--first column always gets filled.
	declare @maxTries int = (select count(*) from @ppl), @tries int = 0
	declare @CurrentPerson int, @remaining int
	while ((select count(*) from @Items where Reviewer1 is null) > 0 AND @tries <= @maxTries)
	BEGIN
		set @CurrentPerson = (select top(1) C_ID from @ppl where Remaining > 0 order by IDX)
		set @remaining = (select top(1) Remaining from @ppl where C_ID = @CurrentPerson)
	
		update A set Reviewer1 = @CurrentPerson
		from (select top(@remaining) * from @Items where Reviewer1 is null order by ui) as A
		set @remaining = @remaining - @@ROWCOUNT
		update @ppl set remaining = @remaining where C_ID = @CurrentPerson
		set @tries = @tries + 1
	END
	--Fill in the second colum, if needed
	IF @PeoplePerItem > 1
	BEGIN
		set @tries = 0
		while ((select count(*) from @Items where Reviewer2 is null) > 0 AND @tries <= @maxTries)
		BEGIN
			set @CurrentPerson = (select top(1) C_ID from @ppl where Remaining > 0 order by IDX)
			set @remaining = (select top(1) Remaining from @ppl where C_ID = @CurrentPerson)
			update A set Reviewer2 = @CurrentPerson
			from (select top(@remaining) * from @Items where Reviewer2 is null order by ui) as A
			set @remaining = @remaining - @@ROWCOUNT
			update @ppl set remaining = @remaining where C_ID = @CurrentPerson
			set @tries = @tries + 1
		END
		select reviewer2, p.C_name, count(ui), Remaining from @Items i inner join @ppl p on i.Reviewer2 = p.C_ID group by reviewer2, p.C_name, Remaining order by reviewer2 
	END
	--Fill in the third colum, if needed
	IF @PeoplePerItem > 2
	BEGIN
		set @tries = 0
		while ((select count(*) from @Items where Reviewer3 is null) > 0 AND @tries <= @maxTries)
		BEGIN
			set @CurrentPerson = (select top(1) C_ID from @ppl where Remaining > 0 order by IDX)
			set @remaining = (select top(1) Remaining from @ppl where C_ID = @CurrentPerson)
			update A set Reviewer3 = @CurrentPerson
			from (select top(@remaining) * from @Items where Reviewer3 is null order by ui) as A
			set @remaining = @remaining - @@ROWCOUNT
			update @ppl set remaining = @remaining where C_ID = @CurrentPerson
			set @tries = @tries + 1
		END
		select reviewer3, p.C_name, count(ui), Remaining from @Items i inner join @ppl p on i.Reviewer3 = p.C_ID group by reviewer3, p.C_name, Remaining order by reviewer3 
	END

	--SECOND kind of preview: tell me what you'd do in detail...
	IF @IsPreview = 2 
	BEGIN
		--We return data in different "shapes", because we want it to be understandable by users...
		if @OneGroupPerPerson = 0
		begin
			--we would create the minimum amount of groups, and then one allocation per group, per person
			select distinct Reviewer1, Reviewer2, Reviewer3
				,Case WHEN Reviewer3 is null and Reviewer2 is null then 'One group (' + p1.C_name + ') One allocation'
					WHEN  Reviewer3 is null then 'One group (' + p1.C_name + ', ' + p2.C_name  + ') Two allocations'
					ELSE 'One group (' + p1.C_name + ', ' + p2.C_name  + ', ' + p3.C_name  + ') Three allocations'
				END as [Description]
				,Case WHEN Reviewer3 is null and Reviewer2 is null then 1
					WHEN  Reviewer3 is null then 2
					ELSE 3
				END as [Number of Allocations]
			from @Items i 
			inner join @ppl p1 on i.Reviewer1 = p1.C_ID
			left join @ppl p2 on i.Reviewer2 = p2.C_ID
			left join @ppl p3 on i.Reviewer3 = p3.C_ID
		end
		else
		begin
			--In this case, we get one group and one allocation per person, but each person might "bleed" into the "round" (might confuse?)
			SELECT [Reviewer], [Role], [Items in allocation] from 
			(
				select IDX, C_name [Reviewer], 'First reviewer' [Role], count(*) [Items in allocation] 
				from @ppl p 
				inner join  @Items i  on p.C_ID = i.Reviewer1
				group by C_name, IDX
				UNION
				select IDX, C_name [Reviewer], 'Second reviewer' [Role], count(*) [Items in allocation] 
				from @ppl p 
				inner join  @Items i  on p.C_ID = i.Reviewer2
				group by C_name, IDX
				UNION
				select IDX, C_name [Reviewer], 'Third reviewer' [Role], count(*) [Items in allocation] 
				from @ppl p 
				inner join  @Items i  on p.C_ID = i.Reviewer3
				group by C_name, IDX
			) AS A
			order by [Role], IDX --WE order BY IDX so to ensure that when one person bleeds into the next round, the two rows for that person are shown together.
		end
		delete from @ppl
		delete from @Items
		return
	END

	--LOGIC is again inspired by st_RandomAllocate!!
	--TIME to start making changes, all wrapped within a transaction (XACT_ABORT ON) so to ensure we always do all or nothing
	--we'll set the Success value to failure here, and again to success before closing the transaction.
	set @Success = 0
	SET XACT_ABORT ON  
	BEGIN TRAN --all or nothing, see: https://docs.microsoft.com/en-us/sql/t-sql/language-elements/rollback-transaction-transact-sql?view=sql-server-2017
		
		--We have two big IF-ELSE blocks, but these variables are used in both.
		DECLARE @MAX_INDEX INT = 0 --used for the order of new attributes create
		SELECT @MAX_INDEX = MAX(ATTRIBUTE_ORDER) + 1 FROM TB_ATTRIBUTE_SET WHERE PARENT_ATTRIBUTE_ID = @Destination_Attribute_ID AND SET_ID = @Destination_set_ID
		IF (@MAX_INDEX IS NULL) SET @MAX_INDEX = 0
		DECLARE @DUMMY_OUTPUT BIGINT = NULL -- WE'll use it once without looking, then again to store  work allocation IDs
		DECLARE @GroupName nvarchar(255), @GROUP1 bigint
		set @tries = 0
		IF @OneGroupPerPerson = 1
			--We are using the strategy where each person gets their own allocation code and their own coding assignment
			BEGIN
			--ALL insertions happen within this loop:
			--new Attributes, new ITEM_SET (as/if needed), new ITEM_ATTRIBUTE, new coding assignments (in this order)
			while (@tries < @maxTries) --we loop, just as many times as there are rows in the ppl table
			BEGIN
				set @CurrentPerson = (select top(1) C_ID from @ppl where destination is null order by IDX)
				set @GroupName = @GroupsPrefix + ' (' + (select C_name from @ppl where C_ID = @CurrentPerson) + ')'
				--Create the allocation codes
				EXECUTE st_AttributeSetInsert @Destination_set_ID, @Destination_Attribute_ID, 1, '', @MAX_INDEX
						, @GroupName, '', @Requestor_id, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP1 OUTPUT
				update @ppl set destination = @GROUP1 where C_ID = @CurrentPerson

				--Create ITEM_SET records, we can do all of this in one go, so only the first time round
				IF @tries = 0
				BEGIN
					INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID) 
						SELECT ids.IID, @Destination_set_ID, 'True', @Requestor_id FROM @Items ids
						EXCEPT
						SELECT ITEM_ID, @Destination_set_ID, 'True', @Requestor_id FROM TB_ITEM_SET WHERE TB_ITEM_SET.SET_ID = @Destination_set_ID AND IS_COMPLETED = 'True'
				END 
				--We can now assign items to codes
				INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
						SELECT ITEM_ID, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @Items ids
						INNER JOIN @ppl p on 
							(ids.Reviewer1 = @CurrentPerson OR ids.Reviewer2 = @CurrentPerson OR ids.Reviewer3 = @CurrentPerson)
						INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.IID AND p.destination = @GROUP1
						WHERE TB_ITEM_SET.SET_ID = @Destination_set_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				SET @MAX_INDEX = @MAX_INDEX + 1
			

				--phew. We can finish by creating the coding assignment for @CurrentPerson
				EXECUTE [dbo].[st_ReviewWorkAllocationInsert] 
				   @REVIEW_ID
				  ,@CurrentPerson
				  ,@Work_to_do_setID
				  ,@GROUP1
				  ,@NEW_WORK_ALLOCATION_ID = @DUMMY_OUTPUT OUTPUT
				update @ppl set work_alloc_id = @DUMMY_OUTPUT where C_ID = @CurrentPerson
				set @tries = @tries + 1
			END
		END
		ELSE BEGIN
			-- we will "share" allocation codes between people (more allocations need to be created!)
			-- basic logic is: within @items table, we need one pot for each combination of (Reviewer1, Reviewer2, Reviewer3) we can store single combinations in @SharedPots 
			INSERT into @SharedPots select distinct Reviewer1,Reviewer2,Reviewer3, null,null from @Items
			
			select * from @SharedPots
			select @maxTries = count(*) from @SharedPots
			select @maxTries
			declare @group_desc nvarchar(2000)
			--ALL insertions happen within this loop:
			--new Attributes, new ITEM_SET (as/if needed), new ITEM_ATTRIBUTE, new coding assignments (in this order)
			while (@tries < @maxTries) --we loop, just as many times as there are rows in the @SharedPots table
			BEGIN
				--We'll create a shared pot for each row in @SharedPots, assign it to all people in that row.
				select @group_desc = (select top(1) 'Used for: ' + p.C_name +
															CASE WHEN Reviewer2 IS null then '' else  ', ' + p2.C_name END
															+ 
															CASE when Reviewer3 IS null then '' else  ', ' + p3.C_name END
															+ '.'
														from @SharedPots sp
															inner join @ppl p on sp.Reviewer1 = p.C_ID
															left join @ppl p2 on sp.Reviewer2 = p2.C_ID
															left join @ppl p3 on sp.Reviewer3 = p3.C_ID
														where sp.destination is null)
				select @GroupName = @GroupsPrefix + ' (group ' + CAST((@tries+1) as varchar(10)) +')'
				--Create the allocation codes
				EXECUTE st_AttributeSetInsert @Destination_set_ID, @Destination_Attribute_ID, 1, @group_desc, @MAX_INDEX
						, @GroupName, @group_desc, @Requestor_id, @NEW_ATTRIBUTE_SET_ID = @DUMMY_OUTPUT OUTPUT, @NEW_ATTRIBUTE_ID = @GROUP1 OUTPUT
				update a set destination = @GROUP1
					from (select top(1) * from @SharedPots where destination is null) as a
				
				select * from @SharedPots
				
				--Create ITEM_SET records, we can do all of this in one go, so only the first time round
				IF @tries = 0
				BEGIN
					INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID) 
						SELECT ids.IID, @Destination_set_ID, 'True', @Requestor_id FROM @Items ids
						EXCEPT
						SELECT ITEM_ID, @Destination_set_ID, 'True', @Requestor_id FROM TB_ITEM_SET WHERE TB_ITEM_SET.SET_ID = @Destination_set_ID AND IS_COMPLETED = 'True'
				END 

				--We can now assign items to codes
				INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID)
						SELECT ITEM_ID, TB_ITEM_SET.ITEM_SET_ID, @GROUP1 FROM @Items ids
						INNER JOIN @SharedPots sp on 
							sp.destination = @GROUP1
							and
							(
								ids.Reviewer1 = sp.Reviewer1 
								AND (ids.Reviewer2 = sp.Reviewer2 OR (ids.Reviewer2 is null and sp.Reviewer2 is null)) 
								AND (ids.Reviewer3 = sp.Reviewer3 OR (ids.Reviewer3 is null and sp.Reviewer3 is null))
							)
						INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_ID = ids.IID
						WHERE TB_ITEM_SET.SET_ID = @Destination_set_ID AND TB_ITEM_SET.IS_COMPLETED = 1
				SET @MAX_INDEX = @MAX_INDEX + 1
				
				--phew. We can finish by creating the coding assignment for each person in shared port
				select @CurrentPerson = Reviewer1 from @SharedPots where destination = @GROUP1
				EXECUTE [dbo].[st_ReviewWorkAllocationInsert] 
				   @REVIEW_ID
				  ,@CurrentPerson
				  ,@Work_to_do_setID
				  ,@GROUP1
				  ,@NEW_WORK_ALLOCATION_ID = @DUMMY_OUTPUT OUTPUT
				update @SharedPots set work_alloc_id = @DUMMY_OUTPUT where destination = @GROUP1

				select @CurrentPerson = Reviewer2 from @SharedPots where destination = @GROUP1
				--IF people per item = 1, there is nothing in Reviewer2 column from @SharedPots
				IF @CurrentPerson is not null
				begin
						EXECUTE [dbo].[st_ReviewWorkAllocationInsert] 
						   @REVIEW_ID
						  ,@CurrentPerson
						  ,@Work_to_do_setID
						  ,@GROUP1
						  ,@NEW_WORK_ALLOCATION_ID = @DUMMY_OUTPUT OUTPUT

						select @CurrentPerson = Reviewer3 from @SharedPots where destination = @GROUP1
						--IF people per item = 2, there is nothing in Reviewer3 column from @SharedPots
						IF @CurrentPerson is not null
						begin
							EXECUTE [dbo].[st_ReviewWorkAllocationInsert] 
							   @REVIEW_ID
							  ,@CurrentPerson
							  ,@Work_to_do_setID
							  ,@GROUP1
							  ,@NEW_WORK_ALLOCATION_ID = @DUMMY_OUTPUT OUTPUT
						END
				END
				set @tries = @tries + 1
			END
			select 'todo'
		END
		--We reached this point without error, so we've succeeded (presumably)
		set @Success = 1
	COMMIT TRAN
END
GO

