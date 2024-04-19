using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.CaptainHookSharp;

namespace TBChestTracker.Pages.Settings
{
    /// <summary>
    /// Interaction logic for HotkeySettingsPage.xaml
    /// </summary>
    public partial class HotkeySettingsPage : Page
    {
        private CaptainHook CaptainHook;
        private object Target;
        private string KeyComboString = $"";
        private int KeyComboCount = 0;
        private Key previousKey = Key.None;
        private Key NewKey = Key.None;
        private List<Key> Keys = new List<Key>();

        public HotkeySettingsPage()
        {
            InitializeComponent();
            CaptainHook = new CaptainHook();
            CaptainHook.onKeyboardMessage += CaptainHook_onKeyboardMessage;
        }

        private void StartAutomationText_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Text = "";
            Keys.Clear();

            CaptainHook.Install();
            Target = (TextBox)sender;
        }

        private void StartAutomationText_LostFocus(object sender, RoutedEventArgs e)
        {
            CaptainHook.Uninstall();
            KeyComboString = $"";
            previousKey = Key.None;
            NewKey = Key.None;
        }

        private void StopAutomationText_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Text = "";
            Keys.Clear();
            CaptainHook.Install();
            Target = (TextBox)sender;
        }

        private void StopAutomationText_LostFocus(object sender, RoutedEventArgs e)
        {
            CaptainHook.Uninstall();
            KeyComboString = $"";
            previousKey = Key.None;
            NewKey = Key.None;
        }

        private void CaptainHook_onKeyboardMessage(object sender, CaptainHook.KeyboardHookMessageEventArgs e)
        {
            var msg = e.MessageType;
            var vkey = e.VirtKeyCode;
            var key = KeyInterop.KeyFromVirtualKey(vkey);
            bool isKeyDown = false;
            bool isNewKey = false;

            isKeyDown = msg == com.CaptainHookSharp.Interop.KeyboardMessage.KeyDown ? true : false;
            
            if (isKeyDown)
            {
                NewKey = key;
                if(previousKey != NewKey)
                {
                    isNewKey = true;
                    previousKey = NewKey;
                }
                else
                {
                    isNewKey = false;
                }
                if(isNewKey)
                {
                    KeyComboCount++;
                    Keys.Add(key);
                }
            }
            else
            {
                var t = ((TextBox)Target);
                t.Text = BuildKeyComboString(Keys, true);

                if(t.Name.Equals("StartAutomationText"))
                {
                    SettingsManager.Instance.Settings.HotKeySettings.StartAutomationKeys = BuildKeyComboString(Keys, false);
                }
                else if(t.Name.Equals("StopAutomationText"))
                {
                    SettingsManager.Instance.Settings.HotKeySettings.StopAutomationKeys = BuildKeyComboString(Keys, false);
                }
                isNewKey = false;
            }
        }

        private string BuildKeyComboString(List<Key> keys, bool bFriendly = false)
        {
            var keyStr = String.Empty;
            if (keys.Count > 0)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    var k = keys[i].ToString();
                    if(bFriendly)
                    {
                        if (k.Contains("Left"))
                            k = k.Replace("Left", "");
                        if (k.Contains("Right"))
                            k = k.Replace("Right", "");
                    }
                    if (i == 0)
                        keyStr = k;
                    else
                        keyStr += $"+{k}";
                }
            }
            return keyStr;
        }
        private string BuildKeyComboString(List<string> keys, bool bFriendly = false)
        {
            var keyStr = String.Empty;
            if (keys.Count > 0)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    var k = keys[i];
                    if (bFriendly)
                    {
                        if (k.Contains("Left"))
                            k = k.Replace("Left", "");
                        if (k.Contains("Right"))
                            k = k.Replace("Right", "");
                    }
                    if (i == 0)
                        keyStr = k;
                    else
                        keyStr += $"+{k}";
                }
            }
            return keyStr;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalDeclarations.IsConfiguringHotKeys = true;

            //--- incase people get curious and mess up the settings file to break program.
            var startAutomationKeys = SettingsManager.Instance.Settings.HotKeySettings.StartAutomationKeys.Split('+').ToList();
            var stopAutomationKeys = SettingsManager.Instance.Settings.HotKeySettings.StopAutomationKeys.Split('+').ToList();

            var friendlyStartAutomationKey = BuildKeyComboString(startAutomationKeys, true);
            var friendlyStopAutomationKey = BuildKeyComboString(stopAutomationKeys, true);

            StartAutomationText.Text = friendlyStartAutomationKey;
            StopAutomationText.Text = friendlyStopAutomationKey;

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            GlobalDeclarations.IsConfiguringHotKeys = false;
            Keys.Clear();
            Keys = null;
        }
    }
}
