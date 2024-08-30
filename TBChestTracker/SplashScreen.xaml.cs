#define PREVIEW_BUILD
using System;
using System.ComponentModel;
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
using System.Reflection;

namespace TBChestTracker
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window, INotifyPropertyChanged
    {

        public MainWindow mainWindow { get; set; }
        public StartPageWindow startPageWindow { get; set; }

        private string _buildMode = string.Empty;
        public string BuildMode
        {
            get => _buildMode;
            set
            {
                _buildMode = value;
                OnPropertyChanged(nameof(BuildMode));   
            }
        }

        private string pStatusMessage = "Getting Things Ready...";
        public string StatusMessage
        {
            get => pStatusMessage;
            set
            {
                pStatusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
        private double pStatusProgress = 0;
        public double StatusProgress
        {
            get => pStatusProgress;
            set
            {
                pStatusProgress = value;
                OnPropertyChanged(nameof(StatusProgress));
            }
        }
        private string pAppVersion = "v1.0.0";
        public string AppVersion
        {
            get => pAppVersion;
            set
            {
                pAppVersion = value;
                OnPropertyChanged(nameof(AppVersion));
            }
        }

        public event EventHandler<EventArgs> onSplashScreenComplete;

        protected virtual void SplashScreenComplete(object sender, EventArgs e)
        {
            EventHandler<EventArgs> completed = onSplashScreenComplete;
            if(completed != null) 
                completed?.Invoke(this, EventArgs.Empty);   
        }
        public SplashScreen()
        {
            InitializeComponent();

            //--- configure AppVersion
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            AppVersion = $"v{version.Major}.{version.Minor}.{version.Build}";
#if PREVIEW_BUILD
            BuildMode = "['Preview']";
#endif
        
        }

        public void Complete()
        {
            SplashScreenComplete(this, new EventArgs());
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateStatus(string message, double progress, int delayMs = 3000)
        {
            StatusMessage = message;
            StatusProgress = progress;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;

            if (mainWindow != null)
                mainWindow.Init(this);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
