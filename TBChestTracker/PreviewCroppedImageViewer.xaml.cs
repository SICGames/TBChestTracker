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

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for PreviewCroppedImageViewer.xaml
    /// </summary>
    public partial class PreviewCroppedImageViewer : Window, INotifyPropertyChanged
    {
        private ImageSource _PreviewCroppedImage = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource PreviewCroppedBitmap
        {
            get => _PreviewCroppedImage;
            set
            {
                _PreviewCroppedImage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewCroppedBitmap)));
            }
        }
        public PreviewCroppedImageViewer()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            PreviewCroppedBitmap = null;
        }
    }
}
