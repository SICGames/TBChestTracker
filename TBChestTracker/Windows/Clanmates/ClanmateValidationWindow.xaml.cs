using CefSharp.DevTools.Page;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TBChestTracker.ViewModels;

namespace TBChestTracker
{

    public class AffectedClanmate
    {
        public AffectedClanmate() 
        {
            Aliases = new ObservableCollection<AffectedClanmate>();
        }
        public string Name { get; set; }
        public ObservableCollection<AffectedClanmate> Aliases { get; set; }
        public void AddAlias(string name)
        {
            this.Aliases.Add(new AffectedClanmate { Name = name });
        }
    }

    public partial class ClanmateValidationWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<AffectedClanmate> affectedClanmates { get; private set; }

        public VerifiedClanmatesViewModel VerifiedClanmatesViewModel { get; private set; }

        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private double? _ClanmateSimilarity;
        public double ClanmateSimilarity
        {
            get => _ClanmateSimilarity.GetValueOrDefault(97);
            set
            {
                _ClanmateSimilarity = value;
                OnPropertyChanged(nameof(ClanmateSimilarity));
            }
        }

        public ClanmateValidationWindow()
        {
            InitializeComponent();
        }

        public void NavigateTo(string page)
        {
            ClanmatesValidationViewer.Navigate(new Uri(Uri.UnescapeDataString(page.ToString()), UriKind.RelativeOrAbsolute));
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NavigateTo($"Pages/ClanmatesValidation/ClanmatesValidationStartPage.xaml");
            affectedClanmates = new ObservableCollection<AffectedClanmate>();
            VerifiedClanmatesViewModel = new VerifiedClanmatesViewModel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            VerifiedClanmatesViewModel.Dispose();
            affectedClanmates.Clear();
            affectedClanmates = null;
        }

        private void ClanmatesValidationViewer_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
