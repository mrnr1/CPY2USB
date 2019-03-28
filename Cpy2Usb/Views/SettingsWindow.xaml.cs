using System.Windows;
using Cpy2Usb.ViewModel;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace Cpy2Usb.Views
{
    /// <summary>
    ///     Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        private readonly SettingsWindowViewModel _settingsViewModel;

        public SettingsWindow()
        {
            InitializeComponent();

            _settingsViewModel = new SettingsWindowViewModel();
            DataContext = _settingsViewModel;

            // now set the Red accent and dark theme
            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent("Red"),
                ThemeManager.GetAppTheme("BaseDark"));
        }
    }
}