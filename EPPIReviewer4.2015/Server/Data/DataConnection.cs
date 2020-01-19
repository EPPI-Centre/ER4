using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace BusinessLibrary.Data
{
    public static class DataConnection
    {
        public static string ConnectionString
        {
            get
            {
                string name = Environment.MachineName;
                if (name.ToLower() == "epi2")// | name.ToLower() == "ssru38")
                    return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.ER4ConnectionString"].ConnectionString;
                else if (name.ToLower() == "epi3")// | name.ToLower() == "eppi-management")
                    return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.ER4ConnectionStringAz"].ConnectionString;
                else if (name.ToLower() == "silvie2")
                    return "Data Source=SILVIE2;Initial Catalog=Reviewer;Integrated Security=True";
                else if (name.ToLower() == "bk-epi")
                    return "Data Source=db-epi;Initial Catalog=TestReviewer;Integrated Security=True";
                    //return "Server=tcp:f6u2ejr2ty.database.windows.net;Database=Reviewer;User ID=JamesThomas100;Password=20thDecember;Trusted_Connection=False;Encrypt=True;";
                else if (name.ToLower() == "ssrulap41")
                    return "Data Source=SSRULAP41\\LAP2008;Initial Catalog=Reviewer;Integrated Security=True";
                else
                    return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.ER4ConnectionString1"].ConnectionString;
            }
        }
    
        public static string AdmConnectionString
        {
            get
            {
                string name = Environment.MachineName;
                if (name.ToLower() == "epi2")// | name.ToLower() == "ssru38")
                    return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.ADMConnectionString"].ConnectionString;
                else if (name.ToLower() == "epi3")// | name.ToLower() == "eppi-management")
                    return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.ADMConnectionStringAz"].ConnectionString;
                else if (name.ToLower() == "silvie2")
                    return "Data Source=SILVIE2;Initial Catalog=ReviewerAdmin;Integrated Security=True";
                    //return "Server=tcp:f6u2ejr2ty.database.windows.net;Database=Reviewer;User ID=JamesThomas100;Password=20thDecember;Trusted_Connection=False;Encrypt=True;";
                else if (name.ToLower() == "bk-epi")
                    return "Data Source=db-epi;Initial Catalog=TestReviewerAdmin;Integrated Security=True";
                else if (name.ToLower() == "ssrulap41")
                    return "Data Source=SSRULAP41\\LAP2008;Initial Catalog=ReviewerAdmin;Integrated Security=True";
                else
                    return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.ADMConnectionString1"].ConnectionString;
            }
        }

        public static string AcademicControllerConnectionString // need to add deployment options + other development machines as needed
        {
            get
            {
                string name = Environment.MachineName;
                if (name.ToLower() == "epi2")// | name.ToLower() == "ssru38")
                    return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.MAGConnectionString"].ConnectionString;
                else if (name.ToLower() == "epi3")// | name.ToLower() == "eppi-management")
                    return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.MAGConnectionStringAz"].ConnectionString;
                else if (name.ToLower() == "silvie2")
                    return "Data Source=SILVIE2;Initial Catalog=Reviewer;Integrated Security=True";
                else if (name.ToLower() == "bk-epi")
                    return "Data Source=db-epi;Initial Catalog=AcademicController;Integrated Security=True";
                    //return "Server=tcp:f6u2ejr2ty.database.windows.net;Database=Reviewer;User ID=JamesThomas100;Password=20thDecember;Trusted_Connection=False;Encrypt=True;";
                else if (name.ToLower() == "ssrulap41")
                    return "Data Source=SSRULAP41\\LAP2008;Initial Catalog=AcademicController;Integrated Security=True";
                else
                    return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.MAGConnectionString1"].ConnectionString;
            }
        }
    }
}


