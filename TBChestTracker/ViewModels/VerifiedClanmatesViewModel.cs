using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.ViewModels
{
    public class VerifiedClanmatesViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<VerifiedClanmate> _verifiedClanmates = new ObservableCollection<VerifiedClanmate>();
        public ObservableCollection<VerifiedClanmate> VerifiedClanmates
        {
            get => _verifiedClanmates;
            set
            {
                _verifiedClanmates = value;
                OnPropertyChanged(nameof(VerifiedClanmates));   
            }
        }
        private int _Total = 0;
        public int Total
        {
            get => _Total;
            set
            {
                _Total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        public static VerifiedClanmatesViewModel Instance { get; private set; }
        public VerifiedClanmatesViewModel() 
        {
            if (Instance == null)
                Instance = this;

        }

        public void Add(string name)
        {
            VerifiedClanmates.Add(new VerifiedClanmate(name));
            Total = VerifiedClanmates.Count();
        }
        private VerifiedClanmate _selected = null;
        private bool disposedValue;

        public VerifiedClanmate Selected
        {
            get => _selected;
            set 
            {
                _selected = value;
                OnPropertyChanged(nameof(Selected));
            }
        }

        public void Remove(VerifiedClanmate verifiedClanmate)
        {
            VerifiedClanmates.Remove(verifiedClanmate);
            Total = VerifiedClanmates.Count();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    VerifiedClanmates.Clear();
                    VerifiedClanmates = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~VerifiedClanmatesViewModel()
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
    public class VerifiedClanmate
    {
        public string Name { get; set; }
        public VerifiedClanmate(string name)
        {
            Name = name;
        }
    }
}
