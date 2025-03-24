using com.HellStormGames.Diagnostics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using TBChestTracker.Web;

namespace TBChestTracker.Windows.OCRCorrection
{
    /// <summary>
    /// Interaction logic for OcrCorrectionWindow.xaml
    /// </summary>
    public partial class OcrCorrectionWindow : Window, INotifyPropertyChanged
    {
        ObservableCollection<AffectedWords> _affectedWords = new ObservableCollection<AffectedWords>();
        TreeViewItem currentTreeViewItem = null;
        bool CanDrag = true;
        bool bSaved = false;
        
        ObservableCollection<AffectedWords> AffectedWordsCollection
        {
            get => _affectedWords;
            set
            {
                _affectedWords = value;
                OnPropertyChanged(nameof(AffectedWordsCollection));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public OcrCorrectionWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private bool LoadOcrCorrectionList(string file)
        {
            if (File.Exists(file) == false)
            {
                return false;
            }
            using (var reader = new StreamReader(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                AffectedWordsCollection = (ObservableCollection<AffectedWords>)serializer.Deserialize(reader, typeof(ObservableCollection<AffectedWords>));  
                reader.Close();
            }

            return true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var file = $"{ClanManager.Instance.CurrentProjectDirectory}\\db\\OcrCorrectionList.json";
            LoadOcrCorrectionList (file);
            AffectedWordsViewBox.ItemsSource = AffectedWordsCollection;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AffectedWordsCollection.Clear();
            AffectedWordsCollection = null;
        }

        private void AddWordButton_Click(object sender, RoutedEventArgs e)
        {
            var text = CorrectWordTextBox.Text;
            if (AffectedWordsViewBox.SelectedItem == null)
            {
                bool exists = AffectedWordsCollection.Any(w => w.Word.Equals(text));
                if (exists == false)
                {
                    AffectedWordsCollection.Add(new AffectedWords(text, new ObservableCollection<AffectedWords>()));
                }
            }
            else
            {
                var selecteditem = (AffectedWords)AffectedWordsViewBox?.SelectedItem;
                if (selecteditem != null)
                {
                    foreach (var affectedword in AffectedWordsCollection)
                    {
                        if(affectedword.Word == selecteditem.Word)
                        {
                            affectedword.AddIncorrectWord(text);
                        }
                    }
                }
            }
            CorrectWordTextBox.Text = String.Empty; 
        }

        private void PickFromListButton_Click(object sender, RoutedEventArgs e)
        {
            WordsPickerWindow wordspicker = new WordsPickerWindow();
            wordspicker.Owner = this;
            wordspicker.Show();
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("You're about to remove everything from the list of correct/incorrect word list and delete OcrCorrectionList.json file. Are you sure?", "Remove All?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                AffectedWordsCollection.Clear();
                var file = $"{ClanManager.Instance.CurrentProjectDirectory}\\db\\OcrCorrectionList.json";
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Loggio.Error(ex, "Ocr Correction Tool", "An exception caught while attempting to delete OcrCorrectionList.json");
                }
            }
        }

        private void RemoveSelectedItemsButton_Click(object sender, RoutedEventArgs e)
        {
            var currentlySelectedItems = AffectedWordsViewBox.SelectedItem as AffectedWords;
            AffectedWordsCollection.Remove(currentlySelectedItems); 
        }

        private void TextBlock_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AffectedWordsViewBox_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void AffectedWordsViewBox_DragEnter(object sender, DragEventArgs e)
        {
            
        }

        private void AffectedWordsViewBox_DragOver(object sender, DragEventArgs e)
        {

        }

        private void AffectedWordsViewBox_Drop(object sender, DragEventArgs e)
        {
            Type type = typeof(List<string>);
            
            List<string> selectedItems = e.Data.GetData(type) as List<string>;
            foreach (string item in selectedItems)
            {
                var exists = AffectedWordsCollection.Any(a => a.Word.Contains(item));
                if (exists == false && CanDrag == true)
                {
                    var affectedWord = new AffectedWords();
                    affectedWord.Word = item;
                    affectedWord.IncorrectWords = new ObservableCollection<AffectedWords>();
                    AffectedWordsCollection.Add(affectedWord);
                }
            }
            e.Handled = true;
        }

        private void TextBlock_Drop(object sender, DragEventArgs e)
        {
            
            TextBlock textBlock = sender as TextBlock;
            if(textBlock == null) { return; }
            var word = textBlock.Text;
            Type type = typeof(List<string>);
            List<string> selectedItems = e.Data.GetData(type) as List<string>;

            foreach (string item in selectedItems)
            {
                
                foreach(var affected in AffectedWordsCollection.ToList())
                {
                    if(affected.Word == word)
                    {
                        affected.AddIncorrectWord(item);
                    }
                }
            }
            CanDrag = true;
            e.Handled = true;
        }

        private void TextBlock_DragOver(object sender, DragEventArgs e)
        {
           CanDrag = false;
        }

        private void TextBlock_DragEnter(object sender, DragEventArgs e)
        {
            CanDrag = false;
        }

        private void TextBlock_DragLeave(object sender, DragEventArgs e)
        {
            CanDrag = true;
        }

        private bool SaveOcrCorrectionListToFile(string filepath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filepath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, AffectedWordsCollection);
                    writer.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;   
            }
        }
        private async Task<bool> SaveAndApply(string filename)
        {
            int errroCount = 0;

            if (!bSaved)
            {
                if (AffectedWordsCollection.Count > 0)
                {
                    bool result = SaveOcrCorrectionListToFile(filename);
                    if (result)
                    {
                        //-- let's apply the fix to every chest file
                        var chestsfolder = $"{ClanManager.Instance.CurrentProjectDirectory}\\Chests\\Data";
                        var di = new DirectoryInfo(chestsfolder);
                        if (di.Exists)
                        {
                            var files = di.EnumerateFiles("chests_*.txt", SearchOption.TopDirectoryOnly).Select(f => f.FullName).ToList();
                            var chestdataFile = $"{ClanManager.Instance.CurrentProjectDirectory}\\Chests\\ChestData.csv";
                            if (File.Exists(chestdataFile))
                            {
                                files.Add(chestdataFile);
                            }

                            foreach (var f in files)
                            {
                                var filedata = string.Empty;
                                var modified_data = string.Empty;
                                try
                                {
                                    using (var reader = new StreamReader(f))
                                    {
                                        filedata = await reader.ReadToEndAsync();
                                        reader.Close();
                                    }

                                    modified_data = filedata;
                                    string escapedStr = string.Empty;

                                    foreach (var affectedWord in AffectedWordsCollection)
                                    {
                                        var correctWord = affectedWord.Word;
                                        foreach (var incorrectWord in affectedWord.IncorrectWords)
                                        {
                                            var incorrect = incorrectWord.Word;
                                            var escaped = Regex.Escape(incorrect);
                                            //var match_pattern = @"([#-}]+" + escaped + @")+";
                                            var match_pattern = $@"({escaped})+";

                                            escapedStr = modified_data;
                                            var r = Regex.Matches(escapedStr, match_pattern, RegexOptions.Singleline);
                                            if (r.Count > 0)
                                            {
                                                //var escaped_CorrectWord = Regex.Escape(correctWord);
                                                escapedStr = Regex.Replace(escapedStr, match_pattern, correctWord);
                                            }

                                        }
                                    }

                                    if (String.IsNullOrEmpty(escapedStr) == false)
                                    {
                                        modified_data = escapedStr;
                                    }

                                    using (var writer = new StreamWriter(f))
                                    {
                                        writer.Write(modified_data);
                                        writer.Close();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Loggio.Error(ex, "Ocr Correction Tool", "Issue occurred which needs to be addressed.");
                                    errroCount++;
                                    continue;
                                }
                            }
                        }
                    }
                }
                bSaved = true;
            }
            return errroCount == 0;
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var file = $"{ClanManager.Instance.CurrentProjectDirectory}\\db\\OcrCorrectionList.json";
            bool result = await SaveAndApply(file);
            if (result)
            {
                if (MessageBox.Show("Whoohoo! All Chest Files are fixed with correct words. We can close Ocr Correction Tool now.") == MessageBoxResult.OK)
                {
                    this.Close();
                }
                else 
                    this.Close();
            }
            else
            {
                MessageBox.Show("Something went wrong. Look inside log file and send to developer.");
            }
        }

        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            WebView.Show("How To Use Ocr Correction Tool?", "http://127.0.0.1:8888/Help/OcrCorrection", 485, 800, true, false, false, false);
        }
    }
}
