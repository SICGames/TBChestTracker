using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    //--- will expand on this more.

    public sealed class DateParser
    {
        private DateKeys _keys = new DateKeys();
        
        //-- Date Keys 
        /*
         11/12/2024 
         11 - month key - 0 indice
         12 - day key - 1 indice
         2024 - year key - 2 indice
         
         12/11/2024 (en_GB)
         1 - indice (Month)
         0 - Indice (Day)
         2 - Indice (Year)
        */
        public DateKeys GetDateKeys() { return _keys; }

        public DateParser() 
        { 

        }
    }
}
