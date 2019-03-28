using System.Windows;
using Cpy2Usb.ViewModel;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace Cpy2Usb.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly MainViewModel _mvm;

        public MainWindow()
        {
            InitializeComponent();

            _mvm = new MainViewModel();

            DataContext = _mvm;

            // now set the Red accent and dark theme
            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent("Red"),
                ThemeManager.GetAppTheme("BaseDark"));
        }
    }
}