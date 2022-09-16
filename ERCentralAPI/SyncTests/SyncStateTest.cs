using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Csla;
using EPPIDataServices.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace SyncTests
{
    public class SyncTests {
        private ZoteroItemReviewIDs _zoteroItemReviewIds;
        private IConfigurationRoot _configuration;

        [SetUp]
        public void Setup()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            _configuration = builder.Build();
            var SqlHelper = new SQLHelper(_configuration, null);

            DataConnection.DataConnectionConfigure(SqlHelper);
            SetAuthenticationToBeChangedWithoutRealParamValues();

            string itemIds = "222788, 220330, 75006";




            var dp = new DataPortal<ZoteroItemReviewIDs>();
            var criteria = new SingleCriteria<string>(itemIds);
            _zoteroItemReviewIds = dp.Fetch(criteria);
        }

        private void SetAuthenticationToBeChangedWithoutRealParamValues()
        {
            string username = "qtnvpod";
            string password = "CrapBirkbeck1";
            int reviewId = 0;
            string LoginMode = "";
            string roles = "";
            ReviewerIdentity ri = ReviewerIdentity.GetIdentity(username, password, reviewId, LoginMode, null);
            if (ri.IsAuthenticated)
            {
                ri.Token = BuildToken(ri);
            }

            ReviewerPrincipal principal = new ReviewerPrincipal(ri);
            Csla.ApplicationContext.User = principal;

            var test = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
        }

        private string BuildToken(ReviewerIdentity ri)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:EPPIApiClientSecret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            IIdentity id = ri as IIdentity;
            ClaimsIdentity riCI = new ClaimsIdentity(id);
            IEnumerable<Claim> claims = riCI.Claims;
            riCI.AddClaim(new Claim("reviewId", ri.ReviewId.ToString()));
            riCI.AddClaim(new Claim("userId", ri.UserId.ToString()));
            riCI.AddClaim(new Claim("name", ri.Name));
            riCI.AddClaim(new Claim("reviewTicket", ri.Ticket));
            riCI.AddClaim(new Claim("isSiteAdmin", ri.IsSiteAdmin.ToString()));
            riCI.AddClaim(new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            foreach (var userRole in ri.Roles)
            {
                riCI.AddClaim(new Claim(ClaimTypes.Role, userRole));
            }
            var token = new JwtSecurityToken(_configuration["AppSettings:EPPIApiUrl"],
              _configuration["AppSettings:EPPIApiClientName"],
              riCI.Claims,
              notBefore: DateTime.Now.AddSeconds(-60),
              expires: DateTime.Now.AddHours(6),
              //expires: DateTime.Now.AddSeconds(15),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [TearDown]
        public void TearDown()
        {
            _zoteroItemReviewIds.Clear();
        }

        [Test]
        public void CheckWhetherItemsExistInZotero(){

            var dpItemsInBoth = new DataPortal<ZoteroERWebReviewItemList>();
            var result = dpItemsInBoth.Fetch();

            var itemsInZotero = result.Select(x => x.ITEM_REVIEW_ID).Intersect(_zoteroItemReviewIds.Select(x => x.ITEM_REVIEW_ID));

            Assert.That(!itemsInZotero.Any());
        }
    }
}