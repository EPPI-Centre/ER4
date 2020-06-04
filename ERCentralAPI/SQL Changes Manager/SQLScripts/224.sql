USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_EmailUpdate]    Script Date: 03/06/2020 15:25:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[st_EmailUpdate]
(
	@EMAIL_ID int,
	@EMAIL_NAME nvarchar(50),
	@EMAIL_MESSAGE nvarchar(max)
)
As
SET NOCOUNT ON


		update TB_MANAGMENT_EMAILS
		set EMAIL_MESSAGE = @EMAIL_MESSAGE, EMAIL_NAME = @EMAIL_NAME
		where EMAIL_ID = @EMAIL_ID 


SET NOCOUNT OFF
GO

