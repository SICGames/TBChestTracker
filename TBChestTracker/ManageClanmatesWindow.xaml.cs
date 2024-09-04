using com.HellStormGames.Logging;
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
using TBChestTracker.Managers;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for AddClanmatesWindow.xaml
    /// </summary>
    public partial class ManageClanmatesWindow : Window
    {

        CollectionViewSource viewSource { get; set; }
        public bool clanmatesAdded { get; set; }
        public string previous_Clanmate_Name { get; set; }

        public ManageClanmatesWindow()
        {
            InitializeComponent();
            viewSource = new CollectionViewSource();
        }

        private void clanmatenameBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var name = clanmatenameBox.Text;
            if(e.Key == Key.Enter)
            {
                //-- ClanManager will handle everything now.
                ClanManager.Instance.ClanmateManager.Add(name);
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
                    ClanManager.Instance.ClanmateManager.ImportFromFileAsync(openFileDialog.FileName);
                    ClanManager.Instance.ClanmateManager.UpdateCount();
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
            var clanmatefile = $"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanmateDatabaseFile}";
            ClanManager.Instance.ClanmateManager.Save(clanmatefile);
            ClanManager.Instance.ClanChestManager.BuildData();
            this.DialogResult = true;
            AppContext.Instance.ClanmatesBeenAdded = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = ClanManager.Instance.ClanmateManager.Database; 

            ListClanMates01.ItemsSource = ClanManager.Instance.ClanmateManager.Database.Clanmates; 
            ClanManager.Instance.ClanmateManager.UpdateCount();
            CollectionViewSource.GetDefaultView(ListClanMates01.ItemsSource).Filter = filter_clanmate;
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
                        ClanManager.Instance.ClanChestManager.RemoveChestData(c.Name);
                        ClanManager.Instance.ClanmateManager.Remove(c.Name);
                    }
                    removalList.Clear();
                    removalList = null;
                    ClanManager.Instance.ClanmateManager.UpdateCount();
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
                var previous_clanmatestatistic_data = new Dictionary<string, List<ClanChestData>>();    
             
                //-- now let's fix clan daily chest data
                //-- clanmate name changed.
                if (ClanManager.Instance.ClanChestManager.ClanChestDailyData.Count() > 0)
                {
                    foreach (var date in ClanManager.Instance.ClanChestManager.ClanChestDailyData.ToList())
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
                    
                    foreach (var clanmates in ClanManager.Instance.ClanmateManager.Database.Clanmates.ToList())
                    {
                        if(clanmates.Name.Equals(clanmate_newname, StringComparison.CurrentCultureIgnoreCase))
                        {
                            ClanManager.Instance.ClanmateManager.Database.Clanmates.Remove(clanmates);
                        }
                    }

                    foreach(var date in ClanManager.Instance.ClanChestManager.ClanChestDailyData.ToList() )
                    {
                        var data = date.Value.Where(name => name.Clanmate.Equals(previous_Clanmate_Name, StringComparison.CurrentCultureIgnoreCase)).ToList();
                        foreach (var d in data)
                        {
                            //Debug.WriteLine($"{d.Clanmate}");
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
                if (ClanManager.Instance.ClanChestManager.clanChestData.Count() > 0)
                {
                    foreach (var clanchest in ClanManager.Instance.ClanChestManager.clanChestData.ToList())
                    {
                        if (clanchest.Clanmate.Equals(previous_Clanmate_Name, StringComparison.CurrentCultureIgnoreCase))
                            clanchest.Clanmate = clanmate_newname;
                    }
                }

                //-- clean up
                previous_clanmatestatistic_data.Clear();
                previous_clanmatestatistic_data = null;

                ClanManager.Instance.ClanChestManager.SaveData();
                Loggy.Write($"{previous_Clanmate_Name} changed to {clanmate_newname}", LogType.INFO);
                //Debug.WriteLine($"{previous_Clanmate_Name} changed to {clanmate_newname}");
            }
        }

      
        private void clanmatenameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(ListClanMates01.ItemsSource).Refresh();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void VerifyClanmates_Click(object sender, RoutedEventArgs e)
        {
            ClanmateVerificationWindow window = new ClanmateVerificationWindow();   
            window.ShowDialog();
        }

        private void MergeClanmate_Click(object sender, RoutedEventArgs e)
        {

            //-- should create backup incase shit hits the fan.
            var backup_file = $@"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder}\clanchests.old";
            ClanManager.Instance.ClanChestManager.SaveData(backup_file);

            var selected = ListClanMates01.SelectedItems;
            var parent_clanmate = ((Clanmate)selected[0]).Name;
            var aliases = new List<string>();
            for(var x = 1; x < selected.Count; x++)
            {
                var clanmate = ((Clanmate)selected[x]);
                aliases.Add(clanmate.Name);
            }
            ClanManager.Instance.ClanmateManager.AddAliases(parent_clanmate, aliases);

            //-- now we need to fix any aliases that may have done chests and place them into parent.
            var parent_clanmatestatistic_data = new Dictionary<string, List<ClanChestData>>();
            var alias_clanmatestatistic_data = new Dictionary<string, List<ClanChestData>>();
            var hasDailyChestData = ClanManager.Instance.ClanChestManager.ClanChestDailyData.Count > 0;

            if(hasDailyChestData)
            {
                foreach (var alias in aliases)
                {
                    //-- loop through aliases
                    //-- grab chests done by aliases
                    //-- stuff them into parent
                    //-- remove aliases 
                    //-- perform cleanup

                    foreach (var date in ClanManager.Instance.ClanChestManager.ClanChestDailyData.ToList())
                    {
                        var chest_dates = date.Value.Where(name => name.Clanmate.Equals(alias, StringComparison.CurrentCultureIgnoreCase)).ToList();

                        List<ClanChestData> chest_data = new List<ClanChestData>();
                        foreach (var chest_date in chest_dates.ToList())
                        {
                            if (chest_date.chests != null)
                            {
                                chest_data.Add(new ClanChestData(alias, chest_date.chests));
                                //--- exception will occur when more identical keys attempted to be added.
                                if(alias_clanmatestatistic_data.ContainsKey(date.Key))
                                {
                                    alias_clanmatestatistic_data[date.Key].Add(chest_date);
                                }
                                else 
                                    alias_clanmatestatistic_data.Add(date.Key, chest_data);
                            }
                        }

                        date.Value.RemoveAll(n => n.Clanmate.Equals(alias, StringComparison.CurrentCultureIgnoreCase));
                    }

                    //-- clean up chest count.
                    if (ClanManager.Instance.ClanChestManager.clanChestData.Count() > 0)
                    {
                        foreach (var clanchest in ClanManager.Instance.ClanChestManager.clanChestData.ToList())
                        {
                            if (clanchest.Clanmate.Equals(alias, StringComparison.CurrentCultureIgnoreCase))
                                clanchest.Clanmate = parent_clanmate;
                        }
                    }

                    //--- remove alias from clanmate list.
                    foreach (var clanmate in ClanManager.Instance.ClanmateManager.Database.Clanmates.ToList())
                    {
                        if (clanmate.Name.Equals(alias, StringComparison.CurrentCultureIgnoreCase))
                        {
                            ClanManager.Instance.ClanmateManager.Database.Clanmates.Remove(clanmate);
                        }
                    }

                }

                foreach (var dailydata in ClanManager.Instance.ClanChestManager.ClanChestDailyData.ToList())
                {
                    var chest_dates = dailydata.Value.Where(name => name.Clanmate.Equals(parent_clanmate, StringComparison.CurrentCultureIgnoreCase)).ToList();
                    foreach (var d in chest_dates)
                    {
                        foreach (var affected_date in alias_clanmatestatistic_data.ToList())
                        {
                            if (dailydata.Key == affected_date.Key)
                            {
                                var chests = affected_date.Value.Select(c => c.chests).ToList()[0];
                                if (d.chests == null)
                                    d.chests = new List<Chest>();

                                if (chests != null)
                                    d.chests.InsertRange(d.chests.Count,chests);
                            }
                        }
                    }
                }

                alias_clanmatestatistic_data.Clear();
                alias_clanmatestatistic_data = null;
                ClanManager.Instance.ClanChestManager.SaveData();

            }

            //-- after that, we remove aliases from chest count database. 
        }

        private void SelectClanmateNameMenuItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PreviewScreenshotWindow previewWindow = new PreviewScreenshotWindow();
            previewWindow.Show();
        }
       
        private void ListClanMates01_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A && e.SystemKey == Key.LeftCtrl)
            {
                ListClanMates01.SelectionMode = SelectionMode.Multiple;
                foreach (ListViewItem item in ListClanMates01.Items)
                {
                    item.IsSelected = !item.IsSelected;
                }
            }
        }
    }
}
