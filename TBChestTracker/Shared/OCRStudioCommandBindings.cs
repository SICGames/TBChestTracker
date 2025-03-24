using Hellscape.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TBChestTracker
{
    public static class OCRStudioCommandBindings
    {
        public static readonly RoutedUICommand SelectObject = new RoutedUICommand
           (
               "Select Object",
               "Select Object",
               typeof(CustomCommandBindings),
               new InputGestureCollection()
               {
                    new KeyGesture(Key.V, ModifierKeys.Control)
               }
           );
        public static readonly RoutedUICommand DrawRegion = new RoutedUICommand
           (
               "Draw Region Of Interest",
               "Draw Region Of Interest",
               typeof(CustomCommandBindings),
               new InputGestureCollection()
               {
                    new KeyGesture(Key.D, ModifierKeys.Control)
               }
           );
        public static readonly RoutedUICommand DrawClickMarker = new RoutedUICommand
           (
               "Draw Click Marker",
               "Draw Click Marker",
               typeof(CustomCommandBindings),
               new InputGestureCollection()
               {
                    new KeyGesture(Key.M, ModifierKeys.Control)
               }
           );
        public static readonly RoutedUICommand PreviewOCRResults = new RoutedUICommand
           (
               "Preview OCR Results",
               "Preview OCR Results",
               typeof(CustomCommandBindings),
               new InputGestureCollection()
               {
                    new KeyGesture(Key.P, ModifierKeys.Control)
               }
           );
        public static readonly RoutedUICommand StartOver = new RoutedUICommand
           (
               "Start Over",
               "Start Over",
               typeof(CustomCommandBindings),
               new InputGestureCollection()
               {
                    new KeyGesture(Key.C, ModifierKeys.Control)
               }
           );
    }
}
