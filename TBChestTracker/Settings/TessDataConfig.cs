using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public enum TessDataOption
    {
        None,
        Best,
        Fast
    };
    [System.Serializable]
    public sealed class TessDataConfig 
    {
        public string Prefix { get; private set; }  
        public TessDataOption Option { get; private set; }
        public string TesseractPackage { get; private set; }

        public TessDataConfig(TessDataOption option) 
        { 
            Option = option;
            if (Option == TessDataOption.None)
            {
                TesseractPackage = "tessdata";
                Prefix = string.Empty;
            }
            else if (Option == TessDataOption.Best)
            {
                TesseractPackage = "tessdata_best";
                Prefix = "_best";
            }
            else if (Option == TessDataOption.Fast)
            {
                {
                    TesseractPackage = "tessdata_fast";
                    Prefix = "_fast";
                }
            }
        }
    }
}
