using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.NativeInterlop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativePoint
    {
        public Int32 X;
        public Int32 Y;
    }
}
