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

        private ObservableCollection<VerifiedClanmate> _verifiedClanmates = null;
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
            Instance = this;
            _verifiedClanmates = new ObservableCollection<VerifiedClanmate>();
        }

        public void Add(string name)
        {
            VerifiedClanmates.Add(new VerifiedClanmate(name));
            Total = VerifiedClanmates.Count();
        }
        private VerifiedClanmate _selected = null;
        
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

        public void Dispose()
        {
            _verifiedClanmates.Clear();
            _verifiedClanmates = null;
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
