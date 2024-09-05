using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class AbsentClanmatesViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string AbsentDuration { get; set; }

        private string _ProcessedText = string.Empty;
        public string ProcessedText
        {
            get => _ProcessedText;
            set
            {
                _ProcessedText = value;
                OnPropertyChanged(nameof(ProcessedText));
            }
        }
        private double _MaxProcessingProgress = 100;
        public double MaxProcessingProgress
        {
            get => _MaxProcessingProgress;
            set
            {
                _MaxProcessingProgress = value;
                OnPropertyChanged(nameof(MaxProcessingProgress));
            }
        }
        private double _ProcessingProgressValue = 0;
        public double ProcessingProgressValue
        {
            get => _ProcessingProgressValue;
            set
            {
                _ProcessingProgressValue = value;
                OnPropertyChanged(nameof(ProcessingProgressValue));
            }
        }

        private string _AbsentMessage = "";
        public string AbsentMessage
        {
            get => _AbsentMessage;
            set
            {
                _AbsentMessage = value;
                OnPropertyChanged(nameof(AbsentMessage));
            }
        }
        private ObservableCollection<string> _absentClanmateList = new ObservableCollection<string>();
        private bool disposedValue;

        public ObservableCollection<string> AbsentClanmateList
        {
            get => _absentClanmateList;
            set
            {
                _absentClanmateList = value;
                OnPropertyChanged(nameof(AbsentClanmateList));
            }
        }

        public static AbsentClanmatesViewModel Instance { get; private set; } 
        public AbsentClanmatesViewModel()
        {
                Instance = this;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (AbsentClanmateList != null)
                    {
                        AbsentClanmateList.Clear();
                        AbsentClanmateList = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AbsentClanmatesViewModel()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
