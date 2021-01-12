using BusinessLibrary.BusinessClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDatabasesMVC.ViewModels
{
    public class WebDbWithRevInfo
    {
        public WebDB WebDb { get; set; }
        public ReviewInfo RevInfo { get; set; }
    }
}
