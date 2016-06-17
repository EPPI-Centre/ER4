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
using BusinessLibrary.BusinessClasses;

namespace EppiReviewer4
{
    public class LaunchReportViewerEventArgs : EventArgs
    {
        private LaunchReportViewerEventArgs() { }
        public LaunchReportViewerEventArgs(string reportText)
        {
            ReportText = reportText;
        }
        public string ReportText { get; private set; }
    }
}