using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.Web
{
    public sealed class WebViewConfig : IWebViewConfig
    {
        public string Uri { get; private set; }
        public WebViewConfig(String uri) 
        { 
            Uri = uri;
        }
    }
}
