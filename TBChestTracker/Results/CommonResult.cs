using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class CommonResultCodes
    {
        public static int Success = 0;
        public static int Error = -1;
        public static int Fail = -2;
        public static int InvalidArguments = -3;
    }
    public sealed class CommonResult : CommonResultCodes  
    {
        public int Code { get; private set; }
        public string Message { get; private set; }
        public CommonResult(int code, string message)
        {
            Code = code; Message = message;
        }
    }
}
