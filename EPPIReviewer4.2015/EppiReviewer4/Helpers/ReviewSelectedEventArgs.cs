using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace EppiReviewer4
{
    public class ReviewSelectedEventArgs : EventArgs
    {
        private ReviewSelectedEventArgs() { }
        public ReviewSelectedEventArgs(int reviewID, string reviewName)
        {
            ReviewID = reviewID;
            ReviewName = reviewName;
            LoginMode = "Standard";
        }
        public ReviewSelectedEventArgs(int reviewID, string reviewName, string loginMode)
        {
            ReviewID = reviewID;
            ReviewName = reviewName;
            LoginMode = loginMode;
        }
        public ReviewSelectedEventArgs(int reviewID, BusinessLibrary.Security.ReviewerIdentity ri)
        {
            ReviewName = "Invalid";
            if (ri.IsSiteAdmin)
            {
                ReviewID = reviewID;
                ReviewName= "REV_ID = " + (-reviewID).ToString() + " (SiteAdmin manual access)";
                LoginMode = ri.LoginMode;
            }
            else ReviewID = 0;
             
        }
        public int ReviewID { get; private set; }
        public string ReviewName { get; private set; }
        public string LoginMode { get; private set; }
    }
}