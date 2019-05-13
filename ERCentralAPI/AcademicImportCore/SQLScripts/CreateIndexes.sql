USE [Academic]

GO

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20190511-124448] ON [dbo].[PaperAuthorAffiliations]
(
	[PaperId] ASC,
	[AuthorSequenceNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

GO


CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-173955] ON [dbo].[Affiliations]
(
	[AffiliationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-174059] ON [dbo].[Authors]
(
	[AuthorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-174148] ON [dbo].[Journals]
(
	[JournalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-174220] ON [dbo].[PaperAbstractsInvertedIndex]
(
	[PaperID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-174314] ON [dbo].[PaperFieldsOfStudy]
(
	[FieldOfStudyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-174301] ON [dbo].[PaperFieldsOfStudy]
(
	[PaperID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-174409] ON [dbo].[PaperRecommendations]
(
	[RecommendedPaperID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-174351] ON [dbo].[PaperRecommendations]
(
	[PaperID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-174457] ON [dbo].[PaperReferences]
(
	[PaperReferenceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-174445] ON [dbo].[PaperReferences]
(
	[PaperID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-174609] ON [dbo].[Papers]
(
	[SearchText] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-174555] ON [dbo].[Papers]
(
	[DOI] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-174542] ON [dbo].[Papers]
(
	[PaperID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [NonClusteredIndex-20180830-174707] ON [dbo].[PaperUrls]
(
	[PaperId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
CREATE FUNCTION [dbo].[fn_StripCharacters]
(
    @String NVARCHAR(MAX), 
    @MatchExpression VARCHAR(255)
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    SET @MatchExpression =  '%['+@MatchExpression+']%'

    WHILE PatIndex(@MatchExpression, @String) > 0
        SET @String = Stuff(@String, PatIndex(@MatchExpression, @String), 1, '')

    RETURN @String

END
GO
insert into AuthorsData(PaperID, AuthorsAll)
select paa.PaperId, STRING_AGG (cast(a.NormalizedName as nvarchar(50)), ';') WITHIN GROUP (ORDER BY authorsequencenumber ASC)

from authors a
inner join PaperAuthorAffiliations paa on paa.AuthorId = a.AuthorID
where AuthorSequenceNumber < 20
group by paa.PaperId
GO
update a
set a.normalizedname = 
	iif (CHARINDEX(' ', b.normalizedname) > 0, reverse(substring(reverse(b.normalizedname),1, charindex(' ', reverse(b.NormalizedName)) -1)), b.normalizedname)
from AuthorsData a
	inner join PaperAuthorAffiliations c on c.PaperId = a.PaperID and c.AuthorSequenceNumber = 1
	inner join Authors b on b.AuthorID = c.AuthorId
GO
/*
Alphabetic only:
SELECT dbo.fn_StripCharacters('a1!s2@d3#f4$', '^a-z')
Numeric only:
SELECT dbo.fn_StripCharacters('a1!s2@d3#f4$', '^0-9')
Alphanumeric only:
SELECT dbo.fn_StripCharacters('a1!s2@d3#f4$', '^a-z0-9')
Non-alphanumeric:
SELECT dbo.fn_StripCharacters('a1!s2@d3#f4$', 'a-z0-9')
*/
GO
