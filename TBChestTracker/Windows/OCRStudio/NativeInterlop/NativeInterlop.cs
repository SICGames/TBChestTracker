using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.NativeInterlop
{
    public class NativeInterlop
    {
        [DllImport("User32.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = true, EntryPoint = "GetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out NativePoint point);
    }
}
