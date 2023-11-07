using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for AddClanmatesWindow.xaml
    /// </summary>
    public partial class AddClanmatesWindow : Window
    {

        CollectionViewSource viewSource { get; set; }
        public MainWindow MainWindow { get; set; }
        public bool clanmatesAdded { get; set; }
        public string previous_Clanmate_Name { get; set; }

        public AddClanmatesWindow()
        {
            InitializeComponent();
            viewSource = new CollectionViewSource();
            
        }

        private void clanmatenameBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var name = clanmatenameBox.Text;
            if(e.Key == Key.Enter)
            {
                MainWindow.ClanChestManager.ClanmateManager.Add(name);

                clanmatenameBox.Text = "";
                clanmatesAdded = true;
            }
        }

        private void LoadFromFileBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed) 
            { 
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Import Clanmates From File";
                openFileDialog.Filter = "Text Files | *.txt | DB Files | *.db";
                if(openFileDialog.ShowDialog() ==  true)
                {
                    MainWindow.ClanChestManager.ClanmateManager.ImportFromFileAsync(openFileDialog.FileName);
                    //ListClanMates01.ItemsSource = MainWindow.ClanChestManager.ClanmateManager.Clanmates;
                    clanmatesAdded =true;
                }
            }
        }

        private void LoadFromCapturing_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                MessageBox.Show("This feature is not ready.");
            }
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            var clanmatefile = ClanDatabaseManager.ClanDatabase.ClanmateDatabaseFile;

            MainWindow.ClanChestManager.ClanmateManager.Save(clanmatefile);
            MainWindow.ClanChestManager.BuildData();    
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = MainWindow.ClanChestManager.ClanmateManager;
            
            ListClanMates01.ItemsSource = MainWindow.ClanChestManager.ClanmateManager.Clanmates;
            CollectionViewSource.GetDefaultView(ListClanMates01.ItemsSource).Filter = filter_clanmate;
            //--- reporting duplicates after adding new entry, saving and reopening this window.
        }
       
        private bool filter_clanmate(object item)
        {
            var clanmate_name = clanmatenameBox.Text;
            if (string.IsNullOrEmpty(clanmate_name))
                return true;
            else if (string.IsNullOrWhiteSpace(clanmate_name))
                return true;
            else
            {
                var clanmate = (Clanmate)item;
                return clanmate.Name.StartsWith(clanmate_name, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = clanmatesAdded;
        }

        private void DeleteSelectedItems_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (ListClanMates01.SelectedItems.Count > 0)
                {
                    List<Clanmate> removalList = new List<Clanmate>();

                    foreach (var selectedclanmate in ListClanMates01.SelectedItems)
                    {
                        var selectedname = selectedclanmate as Clanmate;
                        if (selectedname != null)
                        {
                            removalList.Add(new Clanmate(selectedname.Name));
                        }
                    }

                    //-- we will also need to remove the chest data because clan mate was removed.

                    foreach (var c in removalList.ToList())
                    {
                        MainWindow.ClanChestManager.RemoveChestData(c.Name);
                        MainWindow.ClanChestManager.ClanmateManager.Remove(c.Name);
                        
                    }
                    removalList.Clear();
                    removalList = null;
                    //ListClanMates01.ItemsSource = MainWindow.ClanmateManager.Clanmates;
                }
            }
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.MouseDevice.LeftButton == MouseButtonState.Pressed && e.ClickCount > 1)
            {

                TextBox txt = (TextBox)((Grid)((TextBlock)sender).Parent).Children[1];
                txt.Visibility = Visibility.Visible;
                ((TextBlock)sender).Visibility = Visibility.Collapsed;
                previous_Clanmate_Name = txt.Text;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBlock tb = (TextBlock)((Grid)((TextBox)sender).Parent).Children[0];
            tb.Visibility = Visibility.Visible;
            ((TextBox)sender).Visibility = Visibility.Collapsed;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBlock tb = (TextBlock)((Grid)((TextBox)sender).Parent).Children[0];
                tb.Visibility = Visibility.Visible;
                var textbox = ((TextBox)sender);
                var clanmate_newname = textbox.Text;
                textbox.Visibility = Visibility.Collapsed;

                var previous_chest = new List<ClanChestData>();
                var previous_clanmatestatistic_data = new Dictionary<string, List<ClanChestData>>();    
             
                //-- now let's fix clan daily chest data
               
                //-- clanmate name changed.
                var oldclanmateIndex = 0;
                if (MainWindow.ClanChestManager.ClanChestDailyData.Count() > 0)
                {
                    oldclanmateIndex = MainWindow.ClanChestManager.clanChestData.FindIndex(n => n.Clanmate.Equals(previous_Clanmate_Name));
                    foreach (var date in MainWindow.ClanChestManager.ClanChestDailyData.ToList())
                    {
                        var newdata = date.Value.Where(name => name.Clanmate.Equals(clanmate_newname, StringComparison.CurrentCultureIgnoreCase)).ToList();
                        List<ClanChestData> clandata = new List<ClanChestData>();
                        foreach (var nd in newdata.ToList())
                        {
                            clandata.Add(new ClanChestData(previous_Clanmate_Name, nd.chests));
                            previous_clanmatestatistic_data.Add(date.Key, clandata);
                        }

                        date.Value.RemoveAll(n => n.Clanmate.Equals(clanmate_newname, StringComparison.CurrentCultureIgnoreCase));
                    }
                    
                    foreach (var clanmates in MainWindow.ClanChestManager.ClanmateManager.Clanmates.ToList())
                    {
                        if(clanmates.Name.Equals(clanmate_newname, StringComparison.CurrentCultureIgnoreCase))
                        {
                            MainWindow.ClanChestManager.ClanmateManager.Clanmates.Remove(clanmates);
                        }
                    }
                    foreach(var date in MainWindow.ClanChestManager.ClanChestDailyData.ToList() )
                    {
                        var data = date.Value.Where(name => name.Clanmate.Equals(previous_Clanmate_Name, StringComparison.CurrentCultureIgnoreCase)).ToList();
                        foreach (var d in data)
                        {
                            Debug.WriteLine($"{d.Clanmate}");
                            d.Clanmate = clanmate_newname;
                            foreach(var affected_date in previous_clanmatestatistic_data.ToList())
                            {
                                if(date.Key == affected_date.Key)
                                {
                                    var chests = affected_date.Value.Select(c => c.chests).ToList()[0];
                                    if (d.chests == null)
                                        d.chests = new List<Chest>();

                                    if(chests != null)
                                        d.chests.AddRange(chests);
                                }
                            }
                        }
                    }
                }

                //-- now update clanchestdata.
                if (MainWindow.ClanChestManager.clanChestData.Count() > 0)
                {
                    foreach (var clanchest in MainWindow.ClanChestManager.clanChestData.ToList())
                    {
                        if (clanchest.Clanmate.Equals(previous_Clanmate_Name, StringComparison.CurrentCultureIgnoreCase))
                            clanchest.Clanmate = clanmate_newname;
                    }
                }

                //-- clean up
                previous_chest.Clear();
                previous_chest = null;
                previous_clanmatestatistic_data.Clear();
                previous_clanmatestatistic_data = null;

                MainWindow.ClanChestManager.SaveData();

                Debug.WriteLine($"{previous_Clanmate_Name} changed to {clanmate_newname}");
            }
        }

      
        private void clanmatenameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(ListClanMates01.ItemsSource).Refresh();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
