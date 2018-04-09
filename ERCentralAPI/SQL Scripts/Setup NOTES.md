# Setup guide for using ERIP with SQL stores.

In NON-PRODUCTION environments, first setup stages are manual!

SQL scripts:

Manually create two DBs, default names are Reviewer and ReviewerAdmin.

Run scripts, in ORDER:

ReviewerGenerateScript, ReviewerAdminGenerateScript, "generateReviewer 2nd Passage - DATA" (In Alpha env, run also "Generate ALPHA users") and Changes.

It is possible that you'll need to edit the first two scripts in order to grant (minimal) permissions to the Windows Identity used by IIS to run the present APP/API.
To do so, you will need to find out what identity is used by IIS to run the Application Pool used 

Current script sets permissions for [NT AUTHORITY\SYSTEM], other options are ready to go but commented out.
This all happens in the first area of the scripts, 3 things are needed:
1. The login to Exist for SQL as a whole.
2. The (database)User to exist (for both DBs!).
3. Grant "executor", "datareader" and "datawriter" to the database user (on both DBs!).


## Setting up production-like HTTP/SSL
For SSL/HTTPS to work, you need certificates.

If DNS names are public, and you already have a real certificate installed where ERIP lives (and are enforcing https connection), nothing needs to be done. Otherwise, you can produce two certs in this way:

Authority:
> makecert.exe -n "CN=UCL Company Development Root CA,O=UCL,OU=EPPI,L=London,S=LDN,C=gb" -pe -ss Root -sr LocalMachine -sky exchange -m 120 -a sha256 -len 2048 -r
Certificate issued by authority:
> makecert.exe -n "CN=win-er5-alpha-we.westeurope.cloudapp.azure.com" -pe -ss My -sr LocalMachine -sky exchange -m 120 -in "UCL Company Development Root CA" -is Root -ir LocalMachine -a sha256 -eku 1.3.6.1.5.5.7.3.1
The resulting certificate will last 10 years.

"makecert.exe" can be found in "Visual Studio Command Prompt (2010)" & other places/SDKs.
Both Certificates then need to be installed in the "Trusted Root Certification Authorities" of the machine(s) used to Run the oAuth client (ER5 instance).

In Appsettings.json, three values need to be checked/edited:
"EPPIApiClientSecret": "ClientSecret4Devs",
"EPPIApiClientName": "EPPIoAuthClientID4Devs",
"Certificatepath": "..\\SSRU38.pfx",

The first two (secret and client name) need to be matched by the appsettings.json values in the corresponding given ER5 instance(s).
In future versions, these values will sit in the SQL DB!
The Certificatepath value is paramount. oAuth API won't work without a certificate to sign its responses. You can export the certificate used for SSL
(the second one created above, or the "real" one already in use by IIS), save it somewhere in the FS and adjust the value of Certificatepath accordingly.
Note that the example above uses a relative path!

## Troubleshooting:
If/when debugging is impossible, you can run ERIP from the command-line (will use Kestrel).
> [path where ERIP is deployed]\\> dotnet ERIdentityProvider.dll

It will default to port 5000. If you can configure an ER5 instance to reach the relevant endpoint (via the appsettings.json file of the ER5 instance), any errors produced by trying to consume the ERIP service will show up on the console. This is very useful when some configuration mistake is preventing ERIP from working as expected.

