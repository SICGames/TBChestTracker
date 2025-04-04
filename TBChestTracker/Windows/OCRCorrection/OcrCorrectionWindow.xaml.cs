using com.HellStormGames.Diagnostics;
using CsvHelper.Configuration.Attributes;
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

        private string[] GetChestFiles()
        {
            List<string> files = new List<string>();
            var chestsfolder = $"{ClanManager.Instance.CurrentProjectDirectory}\\Chests\\Data";
            var di = new DirectoryInfo(chestsfolder);
            if (di.Exists)
            {
                files = di.EnumerateFiles("chests_*.txt", SearchOption.TopDirectoryOnly).Select(f => f.FullName).ToList();
                var chestdataFile = $"{ClanManager.Instance.CurrentProjectDirectory}\\Chests\\ChestData.csv";
                if (File.Exists(chestdataFile))
                {
                    files.Add(chestdataFile);
                }
            }

            return files.ToArray();
        }
        private string GetFileData(string filename)
        {
            string data = String.Empty;
            using (StreamReader streamReader = new StreamReader(filename))
            {
                data = streamReader.ReadToEnd();
            }
            return data;    
        }
        private int FindAllOccurances(string src,string word)
        {
            var escaped = Regex.Escape(word);
            return Regex.Matches(src, $@"{escaped}").Count;
        }
        private string Fix(string src, string correctword, string incorrectword)
        {
            string result = src.Replace(incorrectword, correctword);
            return result;
        }
        private bool SaveFixedFile(string filename, string data)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.Write(data);
                writer.Close();
            }
            return true;
        }

        private bool SearchInFilesAndreplace(string[] files, List<AffectedWords> AffectedWords)
        {
            try
            {
                foreach (var file in files)
                {
                    Dictionary<string, string> WordsToFix = new Dictionary<string, string>();
                    string data = String.Empty;
                    try
                    {
                        data = GetFileData(file);
                    
                        foreach (var affectedWord in AffectedWords)
                        {
                            var correct_spelling = affectedWord.Word;
                            foreach (var incorrect_spelling in affectedWord.IncorrectWords)
                            {
                                var occurances = FindAllOccurances(data, incorrect_spelling.Word);
                                if (occurances > 0)
                                {
                                    WordsToFix.Add(incorrect_spelling.Word, correct_spelling);
                                }
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        Loggio.Error(exc,"Ocr Correction Tool","An issue has arrised while attempting to correct ocr incorrect spellings.");
                    }

                    var hasWordsToFix = WordsToFix.Keys.Count();
                    if (hasWordsToFix > 0)
                    {
                        Loggio.Info($"NUmber of words needing to be fixed is => {WordsToFix.Count}");

                        foreach(var incorrect_word in WordsToFix.Keys)
                        {
                            var correct_spelling = WordsToFix[incorrect_word];
                            data = Fix(data, correct_spelling, incorrect_word);
                            Loggio.Info($"Corrected {incorrect_word} with {correct_spelling}");
                        }
                        Loggio.Info($"Finished correcting words!");
                    }

                    if(SaveFixedFile(file, data))
                    {
                        Loggio.Info($"Updated {file} with newer data");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                //-- normally happens when same name is added in dictionary
            }
            return false;
        }


        private async Task<bool> SaveAndApply(string filename)
        {
            bool result = false;
            if (!bSaved)
            {
                if (AffectedWordsCollection.Count > 0)
                {
                    result = SaveOcrCorrectionListToFile(filename);
                    if (result)
                    {
                        result = SearchInFilesAndreplace(GetChestFiles(), AffectedWordsCollection.ToList());
                    }
                }
                bSaved = true;
            }
            return result;
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
