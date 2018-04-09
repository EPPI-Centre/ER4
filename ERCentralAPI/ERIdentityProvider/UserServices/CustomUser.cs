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
        //public string Password { get; set; }
    }
}