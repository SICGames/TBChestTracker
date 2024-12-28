#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ConsoleInterlopEventArgs : EventArgs
    {
        public int? StatusCode { get; set; }
        public string? Data { get; set; }
        public double? Percent { get; set; }
        public ConsoleInterlopEventArgs(int? statuscode, string? data, double? percent)
        {
            this.StatusCode = statuscode;
            this.Data = data;
            this.Percent = percent;
        }
    }
}
