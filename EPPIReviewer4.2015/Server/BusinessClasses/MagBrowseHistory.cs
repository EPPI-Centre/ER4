using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary.BusinessClasses
{
    class MagBrowseHistory // At some point we might want to save this to the database?
    {
        public string BrowseType { get; set; }
        public DateTime DateBrowsed { get; set; }
        public Int64 ContactId { get; set; }
        public Int64 FieldOfStudyId { get; set; }
        public Int64 PaperId { get; set; }
        public string Title { get; set; }
    }
}
