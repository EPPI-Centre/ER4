USE [ReviewerAdmin]
GO

CREATE OR ALTER procedure [dbo].[st_OnlineFeedbackListByUser]
(
	@CONTACT_ID int,
	@MAX_LENGTH INT = 5000
)

As

SELECT top (@MAX_LENGTH) f.*, c.CONTACT_NAME, c.EMAIL from TB_ONLINE_FEEDBACK f
inner join sTB_CONTACT c on f.CONTACT_ID = c.CONTACT_ID and c.CONTACT_ID = @CONTACT_ID
order by f.DATE desc
GO


