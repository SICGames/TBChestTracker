using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class OCRWizardEventArgs : EventArgs
    {
        private int _retryAttempt = 1;
        private int _maxRetryAttempts = 5;

        public int RetryAttempt
        {
            get { return _retryAttempt; }
            set { _retryAttempt = value; }
        }
        public int MaxRetryAttempt
        {
            get { return _maxRetryAttempts; }
        }
        private bool _hasFailed = false;
        public bool hasFailed { get; }
        public OCRWizardEventArgs()
        {
            if (RetryAttempt < MaxRetryAttempt)
                RetryAttempt++;
            else
            {
                hasFailed = true;
            }
        }
    }
}
