using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    [System.Serializable]   
    public class ExportSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private SortType _SortOption = SortType.DESENDING;
        public SortType SortOption
        {
            get => _SortOption;
            set
            {
                _SortOption = value;
                OnPropertyChanged(nameof(SortOption));  
            }
        }
        private DateRangeEnum _DateRange = DateRangeEnum.Today;
        public DateRangeEnum DateRange
        {
            get => _DateRange;
            set
            {
                _DateRange = value; 
                OnPropertyChanged(nameof(DateRange));
            }
        }
        private DateTime _DateRangeTo;
        public DateTime DateRangeTo
        {
            get => _DateRangeTo;
            set
            {
                _DateRangeTo = value;
                OnPropertyChanged(nameof(DateRangeTo));
            }
        }
        private DateTime _DateRangeFrom;
        public DateTime DateRangeFrom
        {
            get => _DateRangeFrom;
            set
            {
                _DateRangeFrom = value;
                OnPropertyChanged(nameof(DateRangeFrom));
            }
        }
        private int _PointsAmount = 0;
        public int PointsAmount
        {
            get { return _PointsAmount; }
            set
            {
                _PointsAmount = value;
                OnPropertyChanged(nameof(PointsAmount));
            }
        }
        private ObservableCollection<string> _ExtraHeaders = new ObservableCollection<string>();
        public ObservableCollection<string> ExtraHeaders
        {
            get { return _ExtraHeaders; }
            set
            {
                _ExtraHeaders = value;
                OnPropertyChanged(nameof(ExtraHeaders));
            }
        }

    }
}
