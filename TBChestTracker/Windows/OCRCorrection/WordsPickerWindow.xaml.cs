using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TBChestTracker.Managers;
using TBChestTracker.Pages;
using TBChestTracker.UI;

namespace TBChestTracker.Windows.OCRCorrection
{
    /// <summary>
    /// Interaction logic for WordsPickerWindow.xaml
    /// </summary>
    public partial class WordsPickerWindow : Window, INotifyPropertyChanged
    {
        OcrCorrectionWindow ParentWindow { get; set; }
        ObservableCollection<string> WordsCollectionList;
        private bool? _IsWordsGenerating;
        public bool IsWordsGenerating
        {
            get => _IsWordsGenerating.GetValueOrDefault(true);
            set
            {
                _IsWordsGenerating
                    = value;
                OnPropertyChanged(nameof(IsWordsGenerating));
            }
        }

        public WordsPickerWindow()
        {
            InitializeComponent();
            WordsCollectionList = new ObservableCollection<string>();
            
        }

        #region Property Changed
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private async Task<ObservableCollection<string>> GenerateWords(String[] files)
        {
            if(files == null)
            {
                return null;
            }
            var words = new ObservableCollection<string>();
            var filedata = String.Empty;
            foreach (var file in files)
            {
                using (var reader = new StreamReader(file))
                {
                    filedata += reader.ReadToEnd();
                    reader.Close();
                }
            }

            var data = Regex.Replace(filedata, @"Contains.\s[^\r\n]+", String.Empty);
            data = Regex.Replace(data, @"From.\s?", String.Empty);
            data = Regex.Replace(data, @"Level[^\d]*([^\s]+)\s?", String.Empty);
            data = Regex.Replace(data, @"Lvl[^\d]*([^\s]+)\s?", String.Empty);
            data = Regex.Replace(data, @"Source.\s?", String.Empty);
            
            data = data.Replace("\r\n", "\n");
            var wordCollection = data.Split('\n').ToList();
            foreach (var item in wordCollection.ToList())
            {
                bool exists = words.Any(word => word.Equals(item));
                if (exists == false && String.IsNullOrEmpty(item) == false)
                    words.Add(item);
            }

            return words;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;

            ParentWindow = this.Owner as OcrCorrectionWindow;
            GeneratedWordsListView.PreviewMouseMove += GeneratedWordsListView_PreviewMouseMove;
            GeneratedWordsListView.AllowDrop = true;

            var clanpath = ClanManager.Instance.CurrentProjectDirectory;
            var chestsFolder = $"{clanpath}\\Chests\\Data";
            var di = new DirectoryInfo(chestsFolder);
            String[] chestfiles = null;
            if (di.Exists)
            {
                IsWordsGenerating =true;
                chestfiles = di.EnumerateFiles("chests_*.txt", SearchOption.TopDirectoryOnly).Select(f => f.FullName).ToArray();
                WordsCollectionList = await GenerateWords(chestfiles);
                if (WordsCollectionList.Count > 0)
                {
                    GeneratedWordsListView.ItemsSource = WordsCollectionList;
                    IsWordsGenerating = false;
                    CollectionViewSource.GetDefaultView(GeneratedWordsListView.ItemsSource).Filter = filter_words;
                }
            }

        }

        private void GeneratedWordsListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                ListView source = (ListView)sender;
                List<string> items = new List<string>();
                foreach (var item in source.SelectedItems)
                {
                    items.Add(item.ToString());
                }
                DataObject data = new DataObject(items);
                DragDrop.DoDragDrop(source, data, DragDropEffects.Copy);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            WordsCollectionList.Clear();
            WordsCollectionList = null;

        }

        private bool filter_words(object item)
        {
            var word = SearchBox.Text;
            if (string.IsNullOrEmpty(word))
                return true;
            else if (string.IsNullOrWhiteSpace(word))
                return true;
            else
            {
                var w = (String)item;
                return w.StartsWith(word, StringComparison.CurrentCultureIgnoreCase);
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(GeneratedWordsListView.ItemsSource).Refresh();
        }
    }
}
