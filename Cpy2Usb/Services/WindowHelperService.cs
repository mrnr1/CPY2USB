using System;
using System.Windows;
using Cpy2Usb.ViewModel;
using Cpy2Usb.Views;

namespace Cpy2Usb.Services
{
    public class WindowHelperService : IDisposable
    {
        /*
         *     ,    ,    /\   /\
              /( /\ )\  _\ \_/ /_
              |\_||_/| < \_   _/ >
              \______/  \|0   0|/
                _\/_   _(_  ^  _)_          <-- SYNCR00T
               ( () ) /`\|V"""V|/`\
                 {}   \  \_____/  /
                 ()   /\   )=(   /\
            jgs  {}  /  \_/\=/\_/  \
         *
         */
        // ReSharper disable once InconsistentNaming
        private static readonly object SYNCR00T = new object();
        private static volatile WindowHelperService _instance;

        private WindowHelperService()
        {
        }

        public static WindowHelperService Instance
        {
            get
            {
                if (_instance == null)
                    lock (SYNCR00T)
                    {
                        if (_instance == null) _instance = new WindowHelperService();
                    }

                return _instance;
            }
        }

        void IDisposable.Dispose()
        {
        }

        public void ShowSettingsWindow(object arg1, object arg2)
        {
            var viewmodel = new SettingsWindowViewModel( /*arg1, arg2*/);
            var view = new SettingsWindow {DataContext = viewmodel};

            view.ShowDialog();
        }

        private void CloseWindow(Window window)
        {
            window?.Close();
        }
    }
}