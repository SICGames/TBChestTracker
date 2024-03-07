using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{

    public class ClanWizardStep : INotifyPropertyChanged
    {

        public List<Uri> uris = new List<Uri>();
        private Int32 _stepindex = 0;
        public Int32 StepIndex
        {
            get
            {
                return _stepindex;
            }
            set
            {
                _stepindex = value;
                OnPropertyChanged(nameof(StepIndex));
            }
        }
        public Uri CurrentPageUri { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class ClanWizardContext
    {
        public static ClanWizardContext Instance { get; private set; }
        public ClanWizardStep ClanWizardStep { get; private set; }
        public ClanWizardContext() 
        {
            if (Instance == null)
                Instance = this;

            ClanWizardStep = new ClanWizardStep();
        }
        public void InitNavigationUris(params string[] uris)
        {
            foreach(var uri in uris) 
            {
                ClanWizardStep.uris.Add(new Uri(uri, UriKind.RelativeOrAbsolute));
            }
        }
        public void NavigateBack()
        {
            if (ClanWizardStep.StepIndex > 0)
                ClanWizardStep.StepIndex = -1;
            else
                ClanWizardStep.StepIndex = 0;

            ClanWizardStep.CurrentPageUri = ClanWizardStep.uris[ClanWizardStep.StepIndex];
        }
        public void NavigateForward()
        {
            if (ClanWizardStep.StepIndex < ClanWizardStep.uris.Count)
                ClanWizardStep.StepIndex += 1;
            else
                ClanWizardStep.StepIndex = ClanWizardStep.uris.Count;

            ClanWizardStep.CurrentPageUri = ClanWizardStep.uris[ClanWizardStep.StepIndex];
        }
    }
}
