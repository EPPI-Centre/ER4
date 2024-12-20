USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ArchieReviewFindFromArchieID]    Script Date: 05/01/2021 13:07:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ArchieReviewFindFromArchieID]
	-- Add the parameters for the stored procedure here
	@A_ID char(18)
	,@CID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @ck int = (select COUNT(Review_id) from TB_REVIEW where ARCHIE_ID = @A_ID and ARCHIE_ID is not null)
	IF @ck = 1
	BEGIN
		select r.* 
		 ,CASE 
			when rc.CONTACT_ID is not null then 1
			else 0
		END as CONTACT_IS_IN_REVIEW
		,dbo.fn_REBUILD_ROLES(rc.REVIEW_CONTACT_ID) as ROLES
		, case when LR is null
			then r.DATE_CREATED
			else LR
			end
			as 'LAST_ACCESS'
		from TB_REVIEW r
		left outer join TB_REVIEW_CONTACT rc on r.REVIEW_ID = rc.REVIEW_ID and CONTACT_ID = @CID
		left join (
			select MAX(LAST_RENEWED) LR, REVIEW_ID
			from ReviewerAdmin.dbo.TB_LOGON_TICKET  
			where @CID = CONTACT_ID
			group by REVIEW_ID
			) as t
			on t.REVIEW_ID = r.REVIEW_ID
		where ARCHIE_ID = @A_ID and ARCHIE_ID is not null
	END
END
GO
