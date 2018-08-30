USE [DataService]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_Split_int]    Script Date: 24/07/2018 15:58:43 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
Create FUNCTION [dbo].[fn_Split_int](@sText varchar(max), @sDelim varchar(20) = ' ')
RETURNS @retArray TABLE (idx int Primary Key, value bigint)
AS
BEGIN
DECLARE @idx int,
	@value varchar(8000),
	@bcontinue bit,
	@iStrike smallint,
	@iDelimlength tinyint
IF @sDelim = 'Space'
	BEGIN
	SET @sDelim = ' '
	END
SET @idx = 0
SET @sText = LTrim(RTrim(@sText))
SET @iDelimlength = DATALENGTH(@sDelim)
SET @bcontinue = 1
IF NOT ((@iDelimlength = 0) or (@sDelim = 'Empty'))
	BEGIN
	WHILE @bcontinue = 1
		BEGIN
--If you can find the delimiter in the text, retrieve the first element and
--insert it with its index into the return table.
 
		IF CHARINDEX(@sDelim, @sText)>0
			BEGIN
			SET @value = SUBSTRING(@sText,1, CHARINDEX(@sDelim,@sText)-1)
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, CAST(@value as bigint))
				END
			
--Trim the element and its delimiter from the front of the string.
			--Increment the index and loop.
SET @iStrike = DATALENGTH(@value) + @iDelimlength
			SET @idx = @idx + 1
			SET @sText = LTrim(Right(@sText,DATALENGTH(@sText) - @iStrike))
		
			END
		ELSE
			BEGIN
--If you can't find the delimiter in the text, @sText is the last value in
--@retArray.
 SET @value = @sText
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, CAST(@value AS bigint))
				END
			--Exit the WHILE loop.
SET @bcontinue = 0
			END
		END
	END
ELSE
	BEGIN
	WHILE @bcontinue=1
		BEGIN
		--If the delimiter is an empty string, check for remaining text
		--instead of a delimiter. Insert the first character into the
		--retArray table. Trim the character from the front of the string.
--Increment the index and loop.
		IF DATALENGTH(@sText)>1
			BEGIN
			SET @value = SUBSTRING(@sText,1,1)
				BEGIN
				INSERT @retArray (idx, value)
				VALUES (@idx, CAST(@value as bigint))
				END
			SET @idx = @idx+1
			SET @sText = SUBSTRING(@sText,2,DATALENGTH(@sText)-1)
			
			END
		ELSE
			BEGIN
			--One character remains.
			--Insert the character, and exit the WHILE loop.
			INSERT @retArray (idx, value)
			VALUES (@idx, CAST(@sText as bigint))
			SET @bcontinue = 0	
			END
	END
END
RETURN
END
GO
USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_DeleteReferencesByREFID]    Script Date: 24/07/2018 15:57:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER procedure [dbo].[st_DeleteReferencesByREFID]
(
	@RefIDs nvarchar(max)
)

As

SET NOCOUNT ON
-- 


BEGIN

declare @t table (ExValue bigint primary key)
insert into @t (ExValue) select  distinct [value] from  dbo.fn_Split_int(@RefIDs, ',')

delete from TB_REFERENCE where REFERENCE_ID in (select * from @t)

END

SET NOCOUNT OFF
GO


USE [DataService]
GO
/****** Object:  StoredProcedure [dbo].[st_ReferencesImportPrepare]    Script Date: 24/07/2018 16:34:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:              Sergio
-- Create date: 23-06-09
-- Description: Prepare Tables for Bulk Item import
-- =============================================
ALTER PROCEDURE [dbo].[st_ReferencesImportPrepare]
        @Items_Number int = 0,
        @Authors_Number int,
		@Externals_Number int,
        @Item_Seed bigint OUTPUT,
        @Author_Seed bigint OUTPUT,
		@External_Seed bigint OUTPUT
AS
BEGIN
SET NOCOUNT ON;
-- This procedure Reservs some Identinty values that will be inserted
-- from C# via a Dataset bulkcopy
-- Note the Table Lock Hints used to prevent insertions to happen while dealing with a particular table
Declare @temp bigint
if @Items_Number != 0
BEGIN
	BEGIN TRAN A
			set @Item_Seed = (SELECT top 1 IDENT_CURRENT('TB_REFERENCE') FROM TB_REFERENCE WITH (HOLDLOCK, TABLOCKX))
			set @Item_Seed = ISNULL(@Item_Seed, 0);
			set @temp = @Item_Seed + @Items_Number
			DBCC CHECKIDENT('TB_REFERENCE', RESEED, @temp)
	COMMIT TRAN A
END
ELSE 
BEGIN
	set @Item_Seed = (SELECT top 1 IDENT_CURRENT('TB_REFERENCE') FROM TB_REFERENCE WITH (HOLDLOCK, TABLOCKX))
END


BEGIN TRAN B
        set @Author_Seed = (SELECT top 1 IDENT_CURRENT('TB_REFERENCE_AUTHOR') FROM TB_REFERENCE_AUTHOR WITH (HOLDLOCK, TABLOCKX))
        set @Author_Seed = ISNULL(@Author_Seed, 0);
		set @temp = @Author_Seed + @Authors_Number
        DBCC CHECKIDENT('TB_REFERENCE_AUTHOR', RESEED, @temp)
COMMIT TRAN B

BEGIN TRAN C
        set @External_Seed = (SELECT top 1 IDENT_CURRENT('TB_EXTERNALID') FROM TB_EXTERNALID WITH (HOLDLOCK, TABLOCKX))
        set @External_Seed = ISNULL(@External_Seed, 0);
		set @temp = @External_Seed + @Authors_Number
        DBCC CHECKIDENT('TB_EXTERNALID', RESEED, @temp)
COMMIT TRAN C

END
GO
