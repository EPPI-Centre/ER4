using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary.BusinessClasses
{
    public class TimePointTypeList : INotifyPropertyChanged
    {
        private string _selectedType;

        public TimePointTypeList()
        {
            PopulateList();
        }
        public string SelectedType
        {
            get { return _selectedType; }
            set
            {
                _selectedType = value;
                NotifyPropertyChanged("SelectedType");
            }
        }

        private List<string> _timepointTypes;

        public List<string> TimepointTypes
        {
            get { return _timepointTypes; }
            set
            {
                _timepointTypes = value;
                NotifyPropertyChanged("Timepoints");
            }
        }
        private void PopulateList()
        {
            TimepointTypes = new List<string>()
            {"seconds", "minutes", "hours", "days", "weeks", "months", "years"};
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }



    }
}
