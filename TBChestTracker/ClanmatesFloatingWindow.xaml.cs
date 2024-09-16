using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Interaction logic for ClanmatesFloatingWindow.xaml
    /// </summary>
    public partial class ClanmatesFloatingWindow : Window
    {
        public Window ParentWindow { get; set; }


        public ClanmatesFloatingWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = 10;
            this.Top = 10;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void RemoveClanmate_Click(object sender, RoutedEventArgs e)
        {
            var selectedClanmates = VERIFIED_CLANMATES_LISTVIEW.SelectedItems;
            var garbagePileList = new List<VerifiedClanmate>();

            foreach (var selectedItem in selectedClanmates)
            {
                var selectedClanmate = selectedItem as VerifiedClanmate;
                garbagePileList.Add(selectedClanmate);

            }
            foreach (var garbage in garbagePileList)
            {
                TBChestTracker.ViewModels.VerifiedClanmatesViewModel.Instance.Remove(garbage);
            }

            garbagePileList.Clear();
            garbagePileList = null;

        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            var clanmateEditor = (ClanmateEditorWindow)ParentWindow;
            if (clanmateEditor.editorMode == EditorMode.SELECTION)
            {
                clanmateEditor.bCanDraw = false;
            }

        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            var clanmateEditor = (ClanmateEditorWindow)ParentWindow;
            if (clanmateEditor.editorMode == EditorMode.SELECTION)
            {
                clanmateEditor.bCanDraw = true;
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            var clanmateEditor = (ClanmateEditorWindow)ParentWindow;
            if (clanmateEditor.editorMode == EditorMode.SELECTION)
            {
                clanmateEditor.bCanDraw = false;
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
                foreach (var verifiedClanmate in VerifiedClanmatesViewModel.Instance.VerifiedClanmates.ToList())
                {
                    if (verifiedClanmate.Name.ToLower().Equals(clanmate_newname.ToLower())) {
                        verifiedClanmate.Name = clanmate_newname;
                    }
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private string oldVerifiedClanmate {get; set;}

        private void EditClanmate_Click(object sender, RoutedEventArgs e)
        {
            if(parent is Grid itemGrid)
            {
                TextBox tb = (TextBox)itemGrid.Children[1];
                tb.Visibility = Visibility.Visible;
                ((TextBlock)itemGrid.Children[0]).Visibility = Visibility.Collapsed;
                oldVerifiedClanmate = tb.Text;
            }
        }

        UIElement parent {  get; set; } 
        private void _TEXTBLOCK__PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            parent = ((Grid)((TextBlock)sender).Parent);
        }
    }
}
