using System;
using System.Collections.Generic;
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
using TBChestTracker.Managers;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for NewConditionWindow.xaml
    /// </summary>
    public partial class NewConditionWindow : Window, INotifyPropertyChanged
    {
        private ChestRef _ChestRef = new ChestRef();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ChestRef ChestRef
        {
            get => _ChestRef;
            set
            {
                _ChestRef = value;
                OnPropertyChanged(nameof(ChestRef));
            }
        }

        public NewConditionWindow()
        {
            InitializeComponent();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            ChestConditions chestcondition = new ChestConditions();

            chestcondition.ChestType = ChestTypeBox.Text;
            chestcondition.ChestName = ChestNameBox.Text;   
            chestcondition.ChestRef = ChestRef;

            chestcondition.level = Int32.Parse(ChestLevelCondition.Text);
            ClanManager.Instance.ClanChestSettings.ChestRequirements.ChestConditions.Add(chestcondition);
            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach(var chesttype in ApplicationManager.Instance.ChestTypes)
            {
                ComboBoxItem ci = new ComboBoxItem();
                ci.Content = chesttype.Name;
                ChestTypeBox.Items.Add(ci);
            }
            foreach (var chestname in ApplicationManager.Instance.ChestNames)
            {
                ComboBoxItem ci = new ComboBoxItem();
                ci.Content = chestname.Name;
                ChestNameBox.Items.Add(ci);
            }

            this.DataContext = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void StackPanel_Click(object sender, RoutedEventArgs e)
        {
            switch (ChestRef.ReferenceOption)
            {
                case RefEnum.BYTYPE:
                    {
                        ChestTypeBox.Visibility = Visibility.Visible;
                        //ChestTextBox.Visibility = Visibility.Collapsed;
                        ChestNameBox.Visibility = Visibility.Collapsed;
                    }
                    break;
                case RefEnum.BYNAME:
                    {
                        //ChestTextBox.Visibility = Visibility.Visible;
                        ChestNameBox.Visibility = Visibility.Visible;
                        ChestTypeBox.Visibility = Visibility.Collapsed;
                    }
                    break;
            }
        }

        private void ChestTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ChestNameBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
