USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_OnlineFeedbackList]    Script Date: 10/06/2019 16:10:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER procedure [dbo].[st_OnlineFeedbackList]
(
	@MAX_LENGTH INT = 5000
)

As

SELECT top (@MAX_LENGTH) f.*, c.CONTACT_NAME, c.EMAIL from TB_ONLINE_FEEDBACK f
inner join Reviewer.dbo.TB_CONTACT c on f.CONTACT_ID = c.CONTACT_ID
order by f.DATE desc
GO