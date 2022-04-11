USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ContactDetailsEdit]    Script Date: 07/04/2022 11:43:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_ContactDetailsEdit]
(
	@CONTACT_ID int,
	@CONTACT_NAME nvarchar(255),
	@USERNAME nvarchar(50),
	@EMAIL nvarchar(500),
	@OLD_PASSWORD varchar(50),
	@NEW_PASSWORD varchar(50),
	@RESULT int output

	
	-- RESULT
        -- 0 - everything OK and account updated
        -- 1 - email already in use
        -- 2 - username already in use
        -- 3 - oldPassword is not correct
	
)

As

SET NOCOUNT ON

set @RESULT = 0
declare @BREAK int = 0
declare @EXISTING_USERNAME nvarchar(50) 

	-- check if email is already in use
	select * from Reviewer.dbo.TB_CONTACT
	where EMAIL = @EMAIL
	and CONTACT_ID != @CONTACT_ID
	if @@ROWCOUNT > 0 
	begin
		set @RESULT = 1
		set @BREAK = 1
	end
	
	 -- username
	if @BREAK = 0
	begin
		-- check if username is already in use
		select * from Reviewer.dbo.TB_CONTACT
		where USERNAME = @USERNAME
		and CONTACT_ID != @CONTACT_ID
		if @@ROWCOUNT > 0 
		begin
			set @RESULT = 2
			set @BREAK = 1
		end
	end

	-- password
	if (@BREAK = 0) AND (@NEW_PASSWORD != '')
	begin
		-- check if @OLD_PASSWORD is correct
		-- the USERNAME might be changing as well so use the original username for this check
		set @EXISTING_USERNAME = (select @USERNAME from Reviewer.dbo.TB_CONTACT
		where CONTACT_ID = @CONTACT_ID)

		select * from Reviewer.dbo.TB_CONTACT
		where USERNAME = @EXISTING_USERNAME
		and PWASHED = HASHBYTES('SHA1', @OLD_PASSWORD + FLAVOUR)
		if @@ROWCOUNT != 1
		begin
			set @RESULT = 3
			set @BREAK = 1
		end
	end


	if @BREAK = 0
	begin
		-- everything is good so update the account details
		if @NEW_PASSWORD = ''
		begin
			update Reviewer.dbo.TB_CONTACT 
			set CONTACT_NAME = @CONTACT_NAME,
			USERNAME = @USERNAME,
			EMAIL = @EMAIL
			where CONTACT_ID = @CONTACT_ID
		end
		else
		begin
			--create salt!
			DECLARE @chars char(100) = '!ò#$%&à()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~èéêëì'
			declare @rnd varchar(20)
			declare @cnt int = 0
			set @rnd = ''
			WHILE (@cnt <= 20) 
			BEGIN
				SELECT @rnd = @rnd + 
					SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
				SELECT @cnt = @cnt + 1
			END
			update Reviewer.dbo.TB_CONTACT 
			set CONTACT_NAME = @CONTACT_NAME
				, USERNAME = @USERNAME
				, EMAIL = @EMAIL
				, FLAVOUR = @rnd
				, PWASHED = HASHBYTES('SHA1', @NEW_PASSWORD + @rnd)
			where CONTACT_ID = @CONTACT_ID
		end
	end


RETURN @RESULT


SET NOCOUNT OFF


GO

