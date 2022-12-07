## **Zotero Getting Started from ErWeb for developers**

Since the switch to "Zotero2" branch, where we moved to the new architecture, with separate projects for Er-Web-Client and API,
There is no "special" preparation needed to make Zotero features work. 

However, it all relies on the registered "ER-Web" app registered (with key, shared secret) by Patrick when he started working on this.
Perhaps it would be a good idea to create a new (maybe "ER-Web-Production") registered with Zotero App, so to keep it's key and shared secret
permanently out of Git source control...
Or perhaps not, as there are plenty of similar "secrets" currently held in appsettings/web.config files in Git itself, so one more,
kept in the "right" kind of file (as it is), won't change much...

Issue about who "owns" (and thus controls) the registered app isn't much of an issue: we can always _register a new app_ if need be.
