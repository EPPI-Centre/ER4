using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.Fixtures
{
    public abstract class FixedLoginTest : IntegrationTest
    {
        protected abstract string username { get; set; }
        protected abstract string password { get; set; }
        protected abstract int ReviewId { get; set; }
        protected FixedLoginTest(ApiWebApplicationFactory fixture) : base(fixture) { }

        protected async Task<bool> AuthenticationDone()
        {
            if (_authenticationDone) return true;
            else
            {
                await InnerLoginToReview(username, password, ReviewId);
                _authenticationDone = true;
                return true;
            }
        }
        protected bool _authenticationDone = false;
    }
}
