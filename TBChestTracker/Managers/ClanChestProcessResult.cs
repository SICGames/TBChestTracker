using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public enum ClanChestProcessEnum 
    { 
        NO_GIFTS = 0,
        ERROR = 1,
        SUCCESS = 2
    }

    public class ClanChestProcessResult
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public ClanChestProcessEnum Result {  get; set; }

        public ClanChestProcessResult(string message, int statusCode, ClanChestProcessEnum result)
        {
            Message = message;
            StatusCode = statusCode;
            Result = result;
        }
    }
}
