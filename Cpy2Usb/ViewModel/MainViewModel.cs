using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Cpy2Usb.Views;
using System.Reflection;
using Cpy2Usb.Models;
using Cpy2Usb.Services;
using System.Threading;
using System.Windows.Forms;
using Cpy2Usb.Properties;
using Ookii.Dialogs;
using MessageBox = System.Windows.MessageBox;

namespace Cpy2Usb.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        
        // Folder textbox from which to copy data to
        private string _sourceFolder;
        public string SourceFolder
        {
            get => _sourceFolder;
            set => Set(ref _sourceFolder, value);
        }

        // File -> Settings
        public ICommand ShowSettingsWindowCommand => new RelayCommand<SettingsWindow>(ShowSettingsWindow);
        private void ShowSettingsWindow(SettingsWindow window)
        {
            WindowHelperService.Instance.ShowSettingsWindow(null, null);
        }

        // File -> Exit menu
        public ICommand ExitMainWindowCommand => new RelayCommand<Window>(ExitButtonClicked);
        private void ExitButtonClicked(Window window)
        {
            OnWindowClosingEvent();
            window?.Close();
        }

        // Help -> About menu
        private About _aboutDialog;
        public About AboutDialog
        {
            get
            {
                if (_aboutDialog == null)
                {
                    _aboutDialog = new About();
                    return _aboutDialog;
                }
                else
                {
                    return _aboutDialog;
                }
            }
        }
        public ICommand ShowAboutWindowCommand => new RelayCommand(ShowAboutWindow);
        private void ShowAboutWindow()
        {
            // TODO: Account for de-initialized windows (from an exit)
            try
            {
                AboutDialog?.ShowDialog();
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        // Browse for a source folder button clicked
        public ICommand BrowseButtonClickedCommand => new RelayCommand(OnBrowseButtonClicked);
        private void OnBrowseButtonClicked()
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder from which to copy data from";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var selectedPath = dialog.SelectedPath;

                if (Directory.Exists(selectedPath) && new DirectoryInfo(selectedPath).GetFiles().Length > 0)
                {
                    SourceFolder = selectedPath;
                }
                else
                {
                    MessageBox.Show(
                        $"Err0r! Either the selected folder '{selectedPath} doesn't exist or doesn't have any " +
                        $"files to copy!", APPTITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // When the primary window is closing, perform cleanup
        public ICommand OnWindowClosingEventCommand => new RelayCommand(OnWindowClosingEvent);
        private void OnWindowClosingEvent()
        {
            if (UsbMon.Instance.WatchForUsbEvents)
            {
                try
                {
                    UsbMon.Instance.OnUsbConnectedEvent -= OnUsbConnected;
                }
                catch (Exception)
                {
                    // ignored
                }
                UsbMon.Instance.WatchForUsbEvents = false;
                UsbMon.Instance.Dispose(true);
            }
        }
        
        // Is the source folder text box enabled?
        private bool _sourceFolderTextBoxIsEnabled;
        public bool SourceFolderTextBoxIsEnabled
        {
            get => _sourceFolderTextBoxIsEnabled;
            set => Set(ref _sourceFolderTextBoxIsEnabled, value);
        }

        // Is the "Start Monitoring" button enabled?
        private bool _startMonitoringButtonIsEnabled;
        public bool StartMonitoringButtonIsEnabled
        {
            get => _startMonitoringButtonIsEnabled;
            set => Set(ref _startMonitoringButtonIsEnabled, value);
        }

        // Is the "Stop Monitoring" button enabled?
        private bool _stopMonitoringButtonIsEnabled;
        public bool StopMonitoringButtonIsEnabled
        {
            get => _stopMonitoringButtonIsEnabled;
            set => Set(ref _stopMonitoringButtonIsEnabled, value);
        }


        // Start monitoring for USB insertions
        public ICommand StartMonitoringButtonClicked => new RelayCommand(OnStartMonitoring);
        private void OnStartMonitoring()
        {
            if (string.IsNullOrEmpty(SourceFolder))
            {
                // TODO: Set an error on the SourceFolder TextBox
                MessageBox.Show("Please input a source directory!", "ERR0r!", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
           
            // If SourceFolder directory does not exist or there are no files to copy from this source
            if (!Directory.Exists(SourceFolder) || new DirectoryInfo(SourceFolder).GetFiles("*", SearchOption.AllDirectories).Length == 0)
            {
                LogWriter.Instance.WriteMsg($"The source directory of '{SourceFolder}' does not exist or there are no files to copy from!", 
                    LogWriter.MsgType.Error, Thread.CurrentThread.ManagedThreadId);

                // TODO: Set an error on the SourceFolder TextBox
                MessageBox.Show($"The folder '{SourceFolder}' does not exist or there are no files to copy from!", "404 - Folder / Files Not Found!", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            UsbMon.Instance.OnUsbConnectedEvent += OnUsbConnected;
            UsbMon.Instance.WatchForUsbEvents = true;

            SourceFolderTextBoxIsEnabled = false;
            StartMonitoringButtonIsEnabled = false;
            StopMonitoringButtonIsEnabled = true;
        }
        
        // Stop monitoring for USB insertions
        public ICommand StopMonitoringButtonClicked => new RelayCommand(OnStopMonitoring);
        private void OnStopMonitoring()
        {
            UsbMon.Instance.OnUsbConnectedEvent -= OnUsbConnected;
            UsbMon.Instance.WatchForUsbEvents = false;

            SourceFolderTextBoxIsEnabled = true;
            StartMonitoringButtonIsEnabled = true;
            StopMonitoringButtonIsEnabled = false;
        }

        // Status of application label
        private string _statusLabelContent;
        public string StatusLabelContent
        {
            get => _statusLabelContent;
            set => Set(ref _statusLabelContent, value);
        }
        
        private string _appTitle;
        public string APPTITLE
        {
            get => _appTitle;
            set { Set(ref _appTitle, value); }
        }
        
        public MainViewModel()
        {
            var asm = Assembly.GetExecutingAssembly().GetName();
            APPTITLE = asm.Name + " v" + asm.Version.ToString();

            // Write out our saved settings that we're about to load
            LogWriter.Instance.WriteMsg(APPTITLE, LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
            LogWriter.Instance.WriteMsg($"File overwriting option is ... [{(Settings.Default.OverwriteFiles ? "Enabled" : "Disabled")}]", 
                LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
            LogWriter.Instance.WriteMsg($"File checksum verification option is ... [{(Settings.Default.VerifyFileChecksum ? "Enabled" : "Disabled")}]",
                LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
            LogWriter.Instance.WriteMsg($"Auto Eject USB on finish option is ... [{(Settings.Default.AutoEjectWhenFinished ? "Enabled" : "Disabled")}]",
                LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
            LogWriter.Instance.WriteMsg($"Verbose logging option is ... [{(Settings.Default.VerboseLogs ? "Enabled" : "Disabled")}]" +
                Environment.NewLine,
                LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
            LogWriter.Instance.WriteMsg($"Strict copy destinations option is ... [{(Settings.Default.StrictCopyDestinations ? "Enabled" : "Disabled")}]" +
                                        Environment.NewLine,
                LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);

            if (Settings.Default.StrictCopyDestinations)
            {
                LogWriter.Instance.WriteMsg($"\t\tStrict copy destinations are ... [{Settings.Default.StrictCopyDestinationDrives}]" +
                                            Environment.NewLine,
                    LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
            }

            LogWriter.Instance.WriteMsg($"Delete USB contents option is ... [{(Settings.Default.DeleteUsbContents ? "Enabled" : "Disabled")}]" +
                                        Environment.NewLine,
                LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);

            SourceFolderTextBoxIsEnabled = true;
            StartMonitoringButtonIsEnabled = true;
            StopMonitoringButtonIsEnabled = false;
            
            /*MessageBox.Show("This utility will copy data to inserted external USB drives. " +
                            Environment.NewLine + Environment.NewLine + 
                            "Please ensure that all unrelated USB storage devices are disconnected and stay disconnected before continuing!" + 
                Environment.NewLine + Environment.NewLine + " \t Press OK to continue.",
                "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);*/

            UpdateStatusLabelContent("Ready!");
        }

        /// <summary>
        /// Executed when a USB drive is connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnUsbConnected(object sender, UsbConnectionArgs args)
        {
            var workerThread = new Thread(() =>
            {
                // If strict copy is enabled, does this drive match a valid destination?
                bool driveMatchesStrictDestinationDrives = false;   

                if (!string.IsNullOrEmpty(SourceFolder) && Directory.Exists(SourceFolder))
                {
                    var sourceFolderDirectory = new DirectoryInfo(SourceFolder);
                    var sourceFolderFiles = sourceFolderDirectory.GetFiles();
                    var usbDriveLetter = args.Drive + "\\";

                    LogWriter.Instance.WriteMsg(Environment.NewLine + 
                        $"{usbDriveLetter} discovered!", LogWriter.MsgType.Info, Thread.CurrentThread.ManagedThreadId);

                    if (sourceFolderFiles.Length == 0)
                    {
                        LogWriter.Instance.WriteMsg($"There are no files to copy from the source folder '{sourceFolderDirectory.FullName}'!",
                            LogWriter.MsgType.Warning, Thread.CurrentThread.ManagedThreadId);
                        return;
                    }

                    if (Settings.Default.StrictCopyDestinations)
                    {
                        var userSelectedDrives = Settings.Default.StrictCopyDestinationDrives.Split(',');
                        
                        // User wants to only copy data to a specific set of drive letters and this is drive letter isn't one of them
                        if (!userSelectedDrives.Contains(args.Drive.Replace(":", "").Replace("\\", "")))
                        {
                            LogWriter.Instance.WriteMsg(
                                $"The drive '{args.Drive}' is not in the list of user specified drives '{Settings.Default.StrictCopyDestinationDrives}'",
                                LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
                            driveMatchesStrictDestinationDrives = false;    //
                            return; //
                        }
                        else
                        {
                            LogWriter.Instance.WriteMsg($"'{args.Drive}' exists in the list of user specified drives '{Settings.Default.StrictCopyDestinationDrives}'",
                                LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
                            driveMatchesStrictDestinationDrives = true;
                        }
                    }

                    // The user wants to delete the contents of the USB drive prior to copy operations
                    if (Settings.Default.DeleteUsbContents)
                    {
                        // If strict copy destinations is not enabled, or it is and this is one of those destinations
                        // (to avoid deleting contents of an unrelated USB drive)
                        if (!Settings.Default.StrictCopyDestinations ||
                            (Settings.Default.StrictCopyDestinations && driveMatchesStrictDestinationDrives))
                        {
                            string[] filesOnDrive = null;
                            try
                            {
                                filesOnDrive = Directory.GetFiles(usbDriveLetter, "*", SearchOption.AllDirectories);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            
                            // If there are actually files to remove
                            if (filesOnDrive != null && filesOnDrive.Length > 0)
                            {
                                foreach (var file in filesOnDrive)
                                {
                                    try
                                    {
                                        LogWriter.Instance.WriteMsg($"Deleting {file}!", LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
                                        File.Delete(file);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogWriter.Instance.WriteMsg($"An exception was thrown while attempting to delete '{file}'{Environment.NewLine}" +
                                                                    $"Strict copy option ...{Settings.Default.StrictCopyDestinations}{Environment.NewLine}" +
                                                                    $"Drive '{usbDriveLetter}' matches the strict copy destinations list of '{Settings.Default.StrictCopyDestinationDrives}': {driveMatchesStrictDestinationDrives}{Environment.NewLine}" +
                                                                    $"Exception: {ex.Message}",
                                            LogWriter.MsgType.Error, Thread.CurrentThread.ManagedThreadId);
                                    }
                                }
                            }
                        }
                    }

                    Copy(SourceFolder, usbDriveLetter, Settings.Default.OverwriteFiles);
                    
                    // Eject the drive
                    if (Settings.Default.AutoEjectWhenFinished)
                    {
                        LogWriter.Instance.WriteMsg($"Ejecting {usbDriveLetter}", LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
                        SafelyEjectUsb.Instance.Eject(SafelyEjectUsb.Instance.USBEject(usbDriveLetter.Replace("\\", "")));
                    }
                }
            });

            try
            {
                workerThread.Start();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void UpdateStatusLabelContent(string msg)
        {
            StatusLabelContent = msg;
        }

        public void Copy(string sourceDirectory, string targetDirectory, bool overWrite)
        {
            DirectoryInfo sourceFolder = new DirectoryInfo(sourceDirectory);
            DirectoryInfo destinationFolder = new DirectoryInfo(targetDirectory);

            CopyAll(sourceFolder, destinationFolder, overWrite);
        }

        public void CopyAll(DirectoryInfo sourceFolder, DirectoryInfo destinationFolder, bool overWrite)
        {
            Directory.CreateDirectory(destinationFolder.FullName);
            string destinationFilePath = "";

            // Copy each file into the new directory.
            foreach (FileInfo fi in sourceFolder.GetFiles())
            {
                try
                {
                    destinationFilePath = destinationFolder.FullName + fi.Name;
                    StatusLabelContent = $"Copying {destinationFilePath}";
                    
                    if (File.Exists(destinationFilePath) && Settings.Default.OverwriteFiles)
                    {
                        LogWriter.Instance.WriteMsg($"The file '{destinationFilePath}' already exists! Overwritting..",
                            LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
                    }

                    fi.CopyTo(Path.Combine(destinationFilePath), overWrite);

                    if (Settings.Default.VerifyFileChecksum)
                    {
                        if (Helpers.Instance.FilesMatch(fi.FullName, destinationFilePath))
                        {
                            LogWriter.Instance.WriteMsg($"{destinationFilePath} copied successfully!",
                                LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
                        }
                        else
                        {
                            LogWriter.Instance.WriteMsg($"Checksum mismatch for the source file '{fi.FullName}' and the destination file " +
                                                        $"'{destinationFilePath}'!",
                                LogWriter.MsgType.Fatal, Thread.CurrentThread.ManagedThreadId);
                            MessageBox.Show(
                                $"The source file '{fi.FullName}' does not match the destination file '{destinationFilePath}'!",
                                "Checksum Mismatch", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        if (File.Exists(destinationFilePath))
                        {
                            LogWriter.Instance.WriteMsg($"{destinationFilePath} copied successfully!",
                                LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogWriter.Instance.WriteMsg($"Error while copying {fi.FullName} to {destinationFilePath}!{Environment.NewLine}Message: {e.Message}",
                        LogWriter.MsgType.Error, Thread.CurrentThread.ManagedThreadId);
                }
                
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in sourceFolder.GetDirectories())
            {
                try
                {
                    DirectoryInfo nextTargetSubDir =
                        destinationFolder.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir, overWrite);
                }
                catch (Exception e)
                {
                    LogWriter.Instance.WriteMsg($"An exception occured while trying to create the directory '{diSourceSubDir.Name}'!{Environment.NewLine}Message: {e.Message}",
                        LogWriter.MsgType.Error, Thread.CurrentThread.ManagedThreadId);
                }
                
            }

            UpdateStatusLabelContent($"Finished copying files to {destinationFolder.FullName}");
            LogWriter.Instance.WriteMsg($"Finished copying files to {destinationFolder.FullName}", LogWriter.MsgType.Debug, 
                Thread.CurrentThread.ManagedThreadId);
        }
    }
}