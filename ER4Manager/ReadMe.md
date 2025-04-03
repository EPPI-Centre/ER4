# The ER4Manager solution/project

The solution file points to two projects: `SQL Changes Manager` which is is described elsewhere and `ER4Manager_1` (Website) which is known as the "**Account Manager**" in the EPPI Reviewer ecosystem of apps.

This is an old DotNet3.x website project which is used to create EPPI Reviewer account, purchase subscriptions and manage accounts/reviews via superuser logins.

This project relies on ancient third party components: `ComponentArt.Web.UI` and `Telerik.Web.UI`. The DLLs for these are provided for convenience, but you do need to purchase licenses for these as they are proprietary software.

For this project to complile you may need to right click the project node, "Add\References...", then "Browse" to the `Additional files` folder and pick the three DLLs from CompomentArt and Telerik.