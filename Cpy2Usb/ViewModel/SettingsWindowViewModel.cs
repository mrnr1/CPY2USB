using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Cpy2Usb.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Cpy2Usb.ViewModel
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        private bool _autoEjectCheckBoxChecked;

        private bool _deleteContentsCheckBoxChecked;

        private bool _isOverwriteCheckBoxChecked;

        private bool _isStrictCopyDestinationCheckBoxChecked;

        private bool _isVerboseCheckBoxChecked;

        private bool _isVerifyChecksumCheckBoxChecked;

        private bool _saveSettingsButtonIsEnabled;

        private string _strictCopyDestinationDrives;

        public SettingsWindowViewModel()
        {
            LoadSettings();
            PropertyChanged += OnNotifiedPropertyChanged;
        }

        public ICommand CloseWindowCommand => new RelayCommand<Window>(CloseWindow);

        public bool IsVerifyChecksumCheckBoxChecked
        {
            get => _isVerifyChecksumCheckBoxChecked;
            set => Set(ref _isVerifyChecksumCheckBoxChecked, value);
        }

        public bool IsOverwriteCheckBoxChecked
        {
            get => _isOverwriteCheckBoxChecked;
            set => Set(ref _isOverwriteCheckBoxChecked, value);
        }

        public bool IsVerboseCheckBoxChecked
        {
            get => _isVerboseCheckBoxChecked;
            set => Set(ref _isVerboseCheckBoxChecked, value);
        }

        public bool IsStrictCopyDestinationCheckBoxChecked
        {
            get => _isStrictCopyDestinationCheckBoxChecked;
            set => Set(ref _isStrictCopyDestinationCheckBoxChecked, value);
        }

        public string StrictCopyDestinationDrives
        {
            get => _strictCopyDestinationDrives;
            set => Set(ref _strictCopyDestinationDrives, value);
        }

        public bool DeleteContentsCheckBoxChecked
        {
            get => _deleteContentsCheckBoxChecked;
            set => Set(ref _deleteContentsCheckBoxChecked, value);
        }

        public bool AutoEjectCheckBoxChecked
        {
            get => _autoEjectCheckBoxChecked;
            set => Set(ref _autoEjectCheckBoxChecked, value);
        }

        public bool SaveSettingsButtonIsEnabled
        {
            get => _saveSettingsButtonIsEnabled;
            set => Set(ref _saveSettingsButtonIsEnabled, value);
        }

        public ICommand SaveSettingsButtonClicked => new RelayCommand(OnSaveSettingsButtonClicked);

        private void OnNotifiedPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SaveSettingsButtonIsEnabled")
                return;

            if (!SaveSettingsButtonIsEnabled)
                SaveSettingsButtonIsEnabled = true;
        }

        private void OnSaveSettingsButtonClicked()
        {
            SaveSettings();
            SaveSettingsButtonIsEnabled = false;
        }


        private void CloseWindow(Window window)
        {
            window?.Close();
        }

        private void SaveSettings()
        {
            Settings.Default.VerifyFileChecksum = IsVerifyChecksumCheckBoxChecked;
            Settings.Default.OverwriteFiles = IsOverwriteCheckBoxChecked;
            Settings.Default.VerboseLogs = IsVerboseCheckBoxChecked;
            Settings.Default.StrictCopyDestinations = IsStrictCopyDestinationCheckBoxChecked;
            Settings.Default.StrictCopyDestinationDrives = StrictCopyDestinationDrives;

            Settings.Default.DeleteUsbContents = DeleteContentsCheckBoxChecked;
            Settings.Default.AutoEjectWhenFinished = AutoEjectCheckBoxChecked;

            Settings.Default.Save();
        }

        private void LoadSettings()
        {
            IsVerifyChecksumCheckBoxChecked = Settings.Default.VerifyFileChecksum;
            IsOverwriteCheckBoxChecked = Settings.Default.OverwriteFiles;
            IsVerboseCheckBoxChecked = Settings.Default.VerboseLogs;
            IsStrictCopyDestinationCheckBoxChecked = Settings.Default.StrictCopyDestinations;
            StrictCopyDestinationDrives = Settings.Default.StrictCopyDestinationDrives;

            DeleteContentsCheckBoxChecked = Settings.Default.DeleteUsbContents;
            AutoEjectCheckBoxChecked = Settings.Default.AutoEjectWhenFinished;
        }
    }
}