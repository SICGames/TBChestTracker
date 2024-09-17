using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Runtime.InteropServices;
using com.HellstormGames.ScreenCapture;

namespace TBChestTracker
{
    public class Automator
    {
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy,
                      int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
   int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);
        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);
        [DllImport("user32.dll")] 
        static extern short VkKeyScan(char ch);

        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        const uint KEYEVENTF_KEYUP = 0x0002;

        const uint MAPVK_VK_TO_VSC = 0x00;
        const uint MAPVK_VSC_TO_VK = 0x01;
        const uint MAPVK_VK_TO_CHAR = 0x02;
        const uint MAPVK_VSC_TO_VK_EX = 0x03;
        const uint MAPVK_VK_TO_VSC_EX = 0x04;

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }
        public static void KeyboardButtonPress(uint keycode)
        {
            uint vkey = MapVirtualKey(keycode, MAPVK_VSC_TO_VK);
            uint scankey = MapVirtualKey(keycode, MAPVK_VK_TO_VSC);

            keybd_event((byte)keycode, (byte)scankey, KEYEVENTF_EXTENDEDKEY | 0, 0); //-- down 
            keybd_event((byte)keycode, (byte)scankey, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0); //-- down 

        }
        public static void LeftClick(int x, int y)
        {
            //-- need to convert x, y
            var mx = (x * 65535 / Snapture.Instance.ScreenWidth);
            var my = (y * 65535 / Snapture.Instance.ScreenHeight);
            mouse_event((int)(MouseEventFlags.MOVE | MouseEventFlags.ABSOLUTE), mx,my, 0, 0);    
            mouse_event((int)(MouseEventFlags.LEFTDOWN), mx, my, 0, 0);
            mouse_event((int)(MouseEventFlags.LEFTUP), mx, my, 0, 0);
        }
        public static short GetVirtualKeyCode(char c)
        {
            return VkKeyScan(c);
        }

        public static void MoveTo(int x, int y)
        {
            var mx = (x * 65535 / Snapture.Instance.ScreenWidth);
            var my = (y * 65535 / Snapture.Instance.ScreenHeight);
            mouse_event((int)(MouseEventFlags.MOVE | MouseEventFlags.ABSOLUTE), mx, my , 0, 0); 
        }
    }
}
