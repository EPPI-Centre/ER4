# SQL Changes Manager ReadMe

The purpose of this file is to explain what the `SQL Changes Manager" program is for, how it's used, and consequently how developers write and deliver SQL changes.

## Managing changes to SQL data-structures and logic

In EPPI Reviewer (ER) most of the data created by users is relational: they will create a list of "concepts" in the form of codes ("Included on Title and Abstract", Population/Intervention/Comparator/Outcome (PICO), Risk of Bias on multiple criteria, and so forth), they would then Assign such "concepts/codes" to references. This pattern repeats for multiple functions, whereby in effect, the data created is mostly in the form of many, many "links" between records of "primary data".

As a consequence it makes sense to include a lot of business logic inside the SQL Database that drives ER, typically in the form of `Stored Procedures`. This creates a problem: the "models/business objects" in the C# codebase never contain all business logic, and instead we have two distinct "moving targets" (C# code and SQL) which need to be **explicitly** kept in synch. Automated systems like the [Entity Framework](https://learn.microsoft.com/en-us/ef/) cannot help because (roughly) they rest on the assumption that the database stores raw data and that business logic is defined exclusively outside of it, which allows to define passive "data-only" objects that in turn define the databases structure.

Instead, ER uses custom database structures and procedures, which receive a lot of attention and fine-tuning. To keep them tightly coupled with the rest of the codebase, developers will produce a single "SQL Changes Script" that gets shipped along with the non-SQL source code changes that require them.

How exactly this is done allows for plenty of leeway, but some rules do apply:

- When using the mainstream development branch, the script numbers need to be even.
    - Odd numbers are kept unused, to leave "space" in case an SQL script is produced as an emergency bugfix in a dedicated branch.
    - Devs working on a "not-mainstream" feature branch will keep their changes in a single temporary script and then give it the next available number when the feature branch in merged in the main development branch ahead of reaching production.
- Although this system ensures separate DB instances on dev/test (and eventually production) environments do share the same structure, errors do happen, and running a given script may fail in a given environment, but not others. To account for this, such scripts **need to be re-runnable** and written in such a way that after re-running a given script, the "end result" is always the same (i.e., if a script creates a new column, re-running the script doesn't add more and more columns each time it runs).

## Using the SQL Changes Manager project

When developers receive new commits produced by other developers, if they contain new SQL changes scripts, they will run the SQL Changes Manager once, and thus apply the SQL changes. If this doesn't work, they can contact whoever wrote the script, and get it fixed.

When publishing new code to production, we apply the SQL changes in exactly the same way, by running the program on the production database server.

The SQL Changes Manager project is currently (Aug 2024) a DotNet Core 2.x console application. Upon execution it will look into the `SQLScripts` folder for numbered `*.sql` scripts, check the "Databases version" number (stored in `ReviewerAdmin.TB_DB_VERSION`) and apply in order any script with a higher number (while updating `TB_DB_VERSION`). 

Errors are logged to the console window and to file, aiding inspection/resolution of errors/exceptions.

This same program is _also_ used by the `IntegrationTests` project in its setup phase (happens one time only upon running one or multiple tests).