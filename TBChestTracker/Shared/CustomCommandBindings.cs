using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hellscape.Commands
{
    public static class CustomCommandBindings
    {
        public static readonly RoutedUICommand NewClanDatabase = new RoutedUICommand
            (
                "New CLan Database",
                "New Clan Database",
                typeof(CustomCommandBindings),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.N, ModifierKeys.Control)
                }
            );
        public static readonly RoutedUICommand LoadClanDatabase = new RoutedUICommand
            (
                "Load CLan Database",
                "Load Clan Database",
                typeof(CustomCommandBindings),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.L, ModifierKeys.Control)
                }
            );
        public static readonly RoutedUICommand SaveClanDatabase = new RoutedUICommand
            (
                "Save CLan Database",
                "Save Clan Database",
                typeof(CustomCommandBindings),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.S, ModifierKeys.Control)
                }
            );
        public static readonly RoutedUICommand SaveAsClanDatabase = new RoutedUICommand
            (
                "Save CLan Database As",
                "Save Clan Database As",
                typeof(CustomCommandBindings),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.A, ModifierKeys.Control | ModifierKeys.Shift)
                }
            );
        public static readonly RoutedUICommand ExportClanDatabase = new RoutedUICommand
            (
                "Export CLan Database",
                "Export Clan Database",
                typeof(CustomCommandBindings),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.X, ModifierKeys.Control | ModifierKeys.Shift)
                }
            );
        public static readonly RoutedUICommand QuitApplication = new RoutedUICommand
            (
                "Exit",
                "Exit",
                typeof(CustomCommandBindings),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.Q, ModifierKeys.Control)
                }
            );
        public static readonly RoutedUICommand ManageClanmates = new RoutedUICommand
            (
                "Manage Clanmates",
                "Manage Clanmates",
                typeof(CustomCommandBindings),
                null
            );
        public static readonly RoutedUICommand ManageClanchestSettings = new RoutedUICommand
            (
                "Manage Clanchest Settings",
                "Manage Clanchest Settings",
                typeof(CustomCommandBindings),
                null
            );
        public static readonly RoutedUICommand ManuallyCaptureScreen = new RoutedUICommand
            (
                "Manually Capture Screen",
                "Manually Capture Screen",
                typeof(CustomCommandBindings),
                null
            );
        public static readonly RoutedUICommand StartAutomation = new RoutedUICommand
            (
                "Start Automation Process",
                "Start Automation Process", typeof(CustomCommandBindings), null
            );
        public static readonly RoutedUICommand StopAutomation = new RoutedUICommand
            (
                "Stop Automation Process",
                "Stop Automation Process", typeof(CustomCommandBindings), null
            );

        public static readonly RoutedUICommand ClanStats = new RoutedUICommand
            (
                "Clan Statistics",
                "Clan Statistics", typeof(CustomCommandBindings), null
            );

    }
}
