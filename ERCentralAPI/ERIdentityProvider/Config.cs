// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityServer4;
using System.Security.Claims;
using ERIdentityProvider.UserServices;

namespace ERIdentityProvider
{
	public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource("dataeventrecordsscope",new []{ "role", "admin", "user", "dataEventRecords", "dataEventRecords.admin" , "dataEventRecords.user" } )
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                //new ApiResource("dataEventRecords")
                //{
                //    ApiSecrets =
                //    {
                //        new Secret("dataEventRecordsSecret".Sha256())
                //    },
                //    Scopes =
                //    {
                //        new Scope
                //        {
                //            Name = "userAuth",
                //            DisplayName = "Scope for the dataEventRecords ApiResource"
                //        }
                //    },
                //    UserClaims = { "role", "admin", "user", "dataEventRecords", "dataEventRecords.admin", "dataEventRecords.user" }
                //},
                new ApiResource("KlasifikiAPI")
                {
                    ApiSecrets =
                    {
                        new Secret("dataEventRecordsSecret".Sha256())
                    },
                    Scopes =
                    {
                        new Scope
                        {
                            Name = "userAuth",
                            DisplayName = "Scope for the userAuth ApiResource"
                        }
                    },
                    UserClaims = { "role", "admin", "user", "dataEventRecords", "dataEventRecords.admin", "dataEventRecords.user", "userAuth" }
                }
                , new ApiResource("SecuredAPI", "My API")
            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients(Microsoft.Extensions.Configuration.IConfiguration conf)
        {//we're getting client settings from config file, which is OK-ish for now, but should be in SQL, MORE work to do!
            string EPPIApiClientSecret = conf["AppSettings:EPPIApiClientSecret"];
            string EPPIApiClientName = conf["AppSettings:EPPIApiClientName"];
            string KlasifikiApiClientSecret = conf["AppSettings:KlasifikiApiClientSecret"];
            string KlasifikiApiClientName = conf["AppSettings:KlasifikiApiClientName"];
			return new List<Client>
            {
                new Client
                {//this defines the client application, including what scopes a given App can access
                    ClientId = EPPIApiClientName,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    AccessTokenType = AccessTokenType.Jwt,
                    AccessTokenLifetime = 3600,
                    IdentityTokenLifetime = 3600,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    SlidingRefreshTokenLifetime = 30,
                    AllowOfflineAccess = true,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    AlwaysSendClientClaims = true,
                    Enabled = true,
                    //the below is used to authenticate the client without interactive user, it's server to server direct comm...
                    //it is also used when server side uses ResourceOwnerPassword flow to talk to server on behalf of user!
                    ClientSecrets=  new List<Secret> { new Secret(EPPIApiClientSecret.Sha256()) },
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId, 
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "SecuredAPI"
                    }
                },
                new Client
                {//this defines the client application, including what scopes a given App can access
                    ClientId = KlasifikiApiClientName,
                    AllowedGrantTypes = {
                                            //GrantType.Hybrid,
                                            GrantType.ClientCredentials,
                                            GrantType.ResourceOwnerPassword
                                        },
                    AccessTokenType = AccessTokenType.Jwt,
                    AccessTokenLifetime = 3 * 3600,//3 hours
                    IdentityTokenLifetime = 3 * 3600,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    SlidingRefreshTokenLifetime = 30,
                    AllowOfflineAccess = true,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    AlwaysSendClientClaims = true,
                    Enabled = true,
                    ClientSecrets=  new List<Secret> { new Secret(KlasifikiApiClientSecret.Sha256()) },
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "KlasifikiAPI"
                    }
                }
            };
        }
		
		
	}
}