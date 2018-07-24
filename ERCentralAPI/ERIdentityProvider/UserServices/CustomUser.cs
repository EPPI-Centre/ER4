using System.Collections.Generic;

namespace ERIdentityProvider.UserServices
{
    public class CustomUser
    {
        public string SubjectId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string ExpiresOn { get; set; }
        public string IsSiteAdmin { get; set; }
        public string IsCochrane { get; set; }
        public List<Organisation> MemberOf { get; set; }
        public CustomUser()
        {
            MemberOf = new List<Organisation>();
        }
    }
    public class Organisation
    {
        public string OrganisationName { get; set; }
        public int OrganisationId { get; set; }
    }
}