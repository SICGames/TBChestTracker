using Emgu.CV.Features2D;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Windows;

namespace TBChestTracker
{
    public class ClanmateManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<Clanmate> _clanmates;
        public ObservableCollection<Clanmate> Clanmates
        {
            get 
            { 
                return _clanmates; 
            }
            set 
            {
                _clanmates = value;
                OnPropertyChanged(nameof(Clanmates));   
            }
        }
        private int _count = 0;
        public int Count
        {
            get => _count;
            set
            {
                _count = value;
                OnPropertyChanged(nameof(Count));   
            }
        }
        public void UpdateCount()
        {
            Count = _clanmates.Count;
        }
        public ClanmateManager() 
        {
            if (_clanmates == null)
                _clanmates = new ObservableCollection<Clanmate>();
        }
        public void Add(string name)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _clanmates.Add(new Clanmate(name));
            });
        }
        public void Remove(string name)
        {
            foreach(var c in _clanmates.ToList())
            {
                if (c.Name == name)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        _clanmates.Remove(c);
                    });
                }
            }
        }
        public async void ImportFromFileAsync(string path)
        {
            using(TextReader  reader = new StreamReader(path)) 
            {
                string data = await reader.ReadToEndAsync();
                if (data.Contains("\r\n"))
                {
                    data = data.Replace("\r\n", ",");
                }
                else
                {
                    data = data.Replace("\n", ",");
                }

                data = data.Substring(0, data.LastIndexOf(","));

                string[] dataCollection = data.Split(',');
                _clanmates.Clear();
                foreach (var c in dataCollection)
                {
                    if(!String.IsNullOrEmpty(c))
                        _clanmates.Add(new Clanmate(c));
                }
                reader.Close();
            }
        }
        public void Save(string path)
        {
            using(StreamWriter writer = File.CreateText(path))
            {
                var counter = 0;
                for(var c = 0; c < _clanmates.Count; c++)
                {
                    if (!String.IsNullOrEmpty(_clanmates[c].Name))
                    {
                            writer.Write($"{_clanmates[c].Name}\n");
                    }
                }
                writer.Close();
                writer.Dispose();
            }
        }
    }

    public class Clanmate
    {
        public string Name { get; set; }
        public Clanmate() { }  
        public Clanmate(string name) {  Name = name; }

    }
}
