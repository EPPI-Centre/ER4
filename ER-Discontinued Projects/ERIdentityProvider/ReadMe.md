# EPPI-Reviewer Identity Provider (ERIP)

This project provides the foundation for two components of the EPPI-Reviewer ecosystem.
- **Single-sign on**. EPPI-Reviewer V5 (ER5) is designed to be deployable to multiple instances. ERIP allows users to log-in in different instances using the same account credentials. In production, these are the same credentials used for EPPI-Reviewer V4 (ER4).
- **Access to (forthcoming) central Data-Services**. When it is convenient to hold data in a central repository (for example: licensing, organisation membership, public data-extraction forms and more), ERIP can be used to obtain (oAuth2) access tokens to control access rights and similar.

The above implies that ER5 instances, in order to be fully functional, depend on the availability of an ERIP instance. On live envirnoment, this will be an unique deployment, serving all instances of ER5. In development and testing environment, many different configurations are supported, allowing developers and tester to pick the solutions that best suits their needs.

## In this document

This document describes the main configuration options and provides guidelines on how to configure the overall environment (including the corresponding ER5 instance(s)).

## Summary of the top level options

1. ERIP can be configured to use two types of repositories:
   1. SQL database, sharing the same schema as (production) ER4.
   3. An 'in memory' list of users, useful for developers to avoid the requirement of installing and maintaining the SQL databases.
3. ERIP can be instructed to use a "developer" (self signed) certificate to encrypt the tokens or to use a password-protected specific certificate.
3. Moreover, ER5 can be configured to use or bypass ERIP.

All these options are controlled by a combination of values within appsettings.json (ERIP and ER5) and the environment variables.

## ERIP is an ER5 core requirement:
In order to consume ERIP (and associated forthcoming services), ER5 needs to have access to an instance of ERIP. In most cases, the easiest way to achieve this is to configure ERIP as a web application running off the local IIS.
To achieve this, IIS needs to be [active](https://www.howtogeek.com/112455/how-to-install-iis-8-on-windows-8/) on the local machine.
After enabling IIS on your local machine, open VS as "administrator", select the "IIS" option in the "Run" button on the main VS toolbar and click it.
The first time this is done, VS should automatically create the virtual folder/application and configure to use the default Application Pool.
After doing this, ERIP will be available for ER5 to consume, and will come back to life each time you boot up. It's a do-it-once and forget scenario.
Developers will need to fetch the latest version and repeat the steps above if/when they will need to use the latest version of ERIP and/or associated services.

## Common Scenarios
### Development, most common
This scenario is recommended for developers that need to work on all ER5 features, but are not involved in developing ERIP and/or Central Data Services.
In this scenario, ERIP is configured to:
1. Use the developer certificate.
2. Run in localhost, without enforcing SSL/HTTPS.
3. Use in-memory data stores (does not need to use/maintain SQL databases).
The ER5 instance consuming this instance will be configured to:
1. Query the correct url, i.e. "http://localhost/eridentityprovider"
5. Doesn't bypass ERIP.

To achieve the above, the relevant sectionion of ERIP's appsettings.json file will look something like the below:
```
"AppSettings": {
		"AdmConnStr": "irrelevant",
		"ER4ConnStr": "irrelevant",
		"UseInMemoryStores": true,
		"EPPIApiClientSecret": "ClientSecret4Devs",
		"EPPIApiClientName": "EPPIoAuthClientID4Devs",
		"Certificatepath": "irrelevant",
		"CertificateSecret": "irrelevant"
	}
```
Environment Variables: set the "ASPNETCORE_ENVIRONMENT" variable to "Development". This is done via Right-click on "ERIdentityProvider" project, properties. In the "Debug" tab, you can set the environment variable.
The list of available users is provided by "\UserServices\InMemoryDevUserRepositoryData.json" editing it allows to customise the list as required (developers should not push their local changes!). This file contains no passwords: ERIP will just check that the username is valid (ER5 requires a password in the login page, but any value will do).

The appsettings.json file of ER5 should contain the following:
```
    "EPPIApiUrl": "http://localhost/eridentityprovider",
    "EPPIApiClientSecret": "ClientSecret4Devs",
    "EPPIApiClientName": "EPPIoAuthClientID4Devs",
    "SkipAuthentication": false,
```

### Development, almost production-like
This scenario is recommended for developers that are involved in developing ERIP and/or Central Data Services.
In this scenario, ERIP is configured to:
1. Use the developer certificate.
2. Run in localhost, without enforcing SSL/HTTPS.
3. Use SQL data stores (does require to maintain SQL databases).
The ER5 instance consuming this instance will be configured to:
1. Query the correct url, i.e. "http://localhost/eridentityprovider"
5. Doesn't bypass ERIP.

To achieve the above, the appsettings.json file relevant session will look something like the below:
```
"AppSettings": {
		"AdmConnStr": "Data Source=localhost;Initial Catalog=ReviewerAdmin;Integrated Security=True",
		"ER4ConnStr": "Data Source=localhost;Initial Catalog=Reviewer;Integrated Security=True",
		"UseInMemoryStores": false,
		"EPPIApiClientSecret": "ClientSecret4Devs",
		"EPPIApiClientName": "EPPIoAuthClientID4Devs",
		"Certificatepath": "irrelevant",
		"CertificateSecret": "irrelevant"
	}
```
Environment Variables: ser the "ASPNETCORE_ENVIRONMENT" variable to "Development". This is done via Right-click on "ERIdentityProvider" project, properties. In the "Debug" tab, you can set the environment variable.
The list of available users is provided by "\UserServices\InMemoryDevUserRepositoryData.json" editing it allows to customise the list as required (developers should not push their local changes!).

The appsettings.json file of ER5 should contain the following:
```
    "EPPIApiUrl": "http://localhost/eridentityprovider",
    "EPPIApiClientSecret": "ClientSecret4Devs",
    "EPPIApiClientName": "EPPIoAuthClientID4Devs",
    "SkipAuthentication": false,
```

To configure SQL, please see the "SQLScripts\Setup NOTES" file.

### Production Mode
This scenario is mandatory for production and recommended for developers that need or want to use all the encryption features in a production/like environment.
In this scenario, ERIP is configured to:
1. Use an ad-hoc, password protected certificate.
2. Be reachable via a DNS name, enforcing SSL/HTTPS.
3. Use SQL data stores (does require to maintain SQL databases).
The ER5 instance consuming this instance will be configured to:
1. Query the correct url, i.e. "https://full.dns.name/eridentityprovider"
5. Doesn't bypass ERIP.

To achieve the above, the appsettings.json file relevant session will look something like the below:
```
"AppSettings": {
		"AdmConnStr": "Data Source=localhost;Initial Catalog=ReviewerAdmin;Integrated Security=True",
		"ER4ConnStr": "Data Source=localhost;Initial Catalog=Reviewer;Integrated Security=True",
		"UseInMemoryStores": false,
		"EPPIApiClientSecret": "ClientSecret4Devs",
		"EPPIApiClientName": "EPPIoAuthClientID4Devs",
		"Certificatepath": "..\\relative\\path\\to\\your\\certificate\\file.pfx",
		"CertificateSecret": "your Certificate Password"
	}
```

The appsettings.json file of ER5 should contain the following:
```
    "EPPIApiUrl": "https://full.dns.name/eridentityprovider",
    "EPPIApiClientSecret": "ClientSecret4Devs",
    "EPPIApiClientName": "EPPIoAuthClientID4Devs",
    "SkipAuthentication": false,
```

To configure SQL, please see the "SQLScripts\Setup NOTES" file.
To allow ER5 to communicate with a URL such as https://full.dns.name (where full.dns.name points to where ERID is deployed), IIS needs to be configured to use HTTPS.
This is done by fulfilling a few requirements:
1. Have a certificate installed and bound to the website (full.dns.name).
   2. if the certificate is self-signed add the certificate in the "Certificates (local machine)\Trusted Root Certificate Authorities".
2. (Optional) enforce HTTPS for the "eridentityprovider" Application.

## Just-leave-me-be mode.
This is the mode where ER5 works in isolation, without using ERIP at all.
Please note that this mode **will be deprecated** as and when ER5 will start depending on associated central data services, as the tokens released by ERIP will become **necessary**.

ERIP config is irrelevant, obviously.
The appsettings.json file of ER5 should contain the following:
```
    "EPPIApiUrl": "irrelevant",
    "EPPIApiClientSecret": "irrelevant",
    "EPPIApiClientName": "irrelevant",
    "SkipAuthentication": true,
```
The SkipAuthentication value trumps all other settings and puts ER5 in a special mode. At startup, ER5 will not try contacting ERIP, so ER5 will not fail at startup if it can't reach it.
This allows to login using local to ER5 usernames and passwords and/or to use the signin for devs route.
Both these features should be deprecated quite soon:
1. Passwords in ER5 are hashed but not salted. Thus, the password field should be removed.
2. The "signin for devs" route offers NO security, so should be disabled when we'll reach production.


## Behind the Hood: ERIP
The main configuration "choices" are made in "Stratup.cs", "ConfigureServices" method.
There are three distinct configurations controlled by `if...else` blocks.
Controlling logic:
1. Environment = "Development" AND UseInMemoryStores = true. This corresponds to "Development, most common" scenario.
2. Environment = "Development" AND UseInMemoryStores = false. This corresponds to "Development, almost production-like" scenario.
3. All other combinations fall back to "Development" mode. Note here that this does not by itself enforce HTTPS-only connections.

ERIP uses the IdentityServer4 library/dlls, to control the Open Connect / oAuth logic.

## Behind the Hood: ER5
When ER5 uses ERIP to authenticate users, it uses Open Connect via the oAuth "password" pathway. The HTML client send username and password to ER5 instance, this sends both to ERIP.
If ER5 identifies itself with a valid clientID and clientSecret, and username/password are valid, it receives an Open Connect token in the response.
This token contains:
1. The user information in the form of a ClaimsPrincipal object, enriched with appropriate claims. This object is used to identify the user.
2. Access token. A short lived token that can be added to requests to additional data services (forthcoming).
3. Refresh token. A long lived token that can be used to obtain a fresh access token from ERIP.

ER5 uses the information contained in 1. to find (or create, if necessary) and update the local record for the "being authenticated" user. It then sets an authentication cookie in the response to the HTML client.

Whenever SkipAuthentication = false, at startup, ER5 will instantiate the ERIP/IdentityServer4 client. To do so, it will contact it's "discovery" endpoint. If the discovery endpoint cannot be reached, ER5 will fail to start.
This is _by design_, given that ER5 depends on ERIP, ER5 _cannot run_ if ERIP cannot be reached.
If SkipAuthentication = true, currently ER5 reverts to its initial "standalone" authentication mode, using usernames and passwords stored in the local DB. This will be deprecated for reasons explained above.
After deprecation, we might retain the SkipAuthentication option. In this mode ER5 will not try to instantiate the ERIP/IdentityServer4 client, so it will run normally even if ERIP is not present. This could be useful for developers, in case they already have a cookie set to certify their authentication status.