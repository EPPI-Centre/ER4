using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;

namespace ERIdentityProvider.UserServices
{
    public class EppiUserProfileService : IProfileService
    {
        
        protected readonly IUserRepository _userRepository;

        public EppiUserProfileService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
			
			var user = await _userRepository.FindBySubjectIdAsync(context.Subject.GetSubjectId());
			var claims = new List<Claim>
            {
                new Claim("role", "user"),
                new Claim("username", user.UserName),
            };
			if (!user.SubjectId.IsNullOrEmpty()) claims.Add(new Claim(ClaimTypes.PrimarySid, user.SubjectId));
			if (user.IsSiteAdmin == "True") claims.Add(new Claim("role", "admin"));
			if (user.IsCochrane == "True") claims.Add(new Claim("role", "CochraneUser"));
			if (!user.Email.IsNullOrEmpty()) claims.Add(new Claim(ClaimTypes.Email, user.Email));
			if (!user.DisplayName.IsNullOrEmpty()) claims.Add(new Claim(ClaimTypes.Name, user.DisplayName));
			if (!user.ExpiresOn.IsNullOrEmpty()) claims.Add(new Claim("ExpiresOn", user.ExpiresOn));
            if (!user.MemberOf.IsNullOrEmpty())
            {
                foreach (Organisation org in user.MemberOf)
                {
                    claims.Add(new Claim("Organisation", "ID:" + org.OrganisationId.ToString() + " OrgName: " + org.OrganisationName));
                }
            }
			context.IssuedClaims = claims;
        }
		
		public async Task IsActiveAsync(IsActiveContext context)
        {
			//expiry date does not prevent people from logging on, so this is always true
			context.IsActive = true;

			//if (!context.Subject.HasClaim(
			//								c => c.Type == "ExpiresOn" 
			//								&& DateTime.Parse(c.Value) >= DateTime.Now
			//							 ))
			//{
			//	context.IsActive = false;
			//	return;
			//}
			//else
			//{
			//	context.IsActive = true;
			//	return;
			//}
			
            //var sub = context.Subject.GetSubjectId();
            //var user = _userRepository.FindBySubjectId(context.Subject.GetSubjectId());
            ////you need to change this, if expired license should return false...
            //context.IsActive = user != null;
        }
    }
}