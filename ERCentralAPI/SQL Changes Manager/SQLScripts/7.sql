USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_SetTypeList]    Script Date: 04/10/2018 11:39:41 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_fakeSproc]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_fakeSproc]
GO