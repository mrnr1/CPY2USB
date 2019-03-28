using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using Cpy2Usb.Models;
using GalaSoft.MvvmLight;

namespace Cpy2Usb.Services
{
    public class UsbMon : ViewModelBase
    {
        private static UsbMon _instance;

        private readonly ManagementEventWatcher _insertWatcher;
        private readonly ManagementEventWatcher _queryRemoveWatcher; // request to remove usb
        private readonly ManagementEventWatcher _removeWatcher;

        private List<string> _connectedRemovableDrives;

        private bool _watchForUsbEvents;

        public UsbMon()
        {
            ConnectedRemovableDrives = new List<string>();

            _insertWatcher = new ManagementEventWatcher(
                "SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            _insertWatcher.EventArrived += OnUsbConnected;

            _queryRemoveWatcher = new ManagementEventWatcher(
                "SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 or EventType = 3");
            _queryRemoveWatcher.EventArrived += OnUsbQueryDisconnect; // user requested a safe eject
            _removeWatcher = new ManagementEventWatcher(
                "SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            _removeWatcher.EventArrived += OnUsbDisconnected;

            // detect already connected & ready drives
            var drives = DriveInfo.GetDrives().Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable);
            var driveInfos = drives as IList<DriveInfo> ?? drives.ToList();

            foreach (var drive in driveInfos)
            {
                ConnectedRemovableDrives.Add(drive.Name.Replace("\\", string.Empty)); // watch the root of the drive
                var dir = new DirectoryInfo(drive.Name);
            }

            // this._worker = new Thread(() => this.OnUsbConnected(null, null));
        }

        public static UsbMon Instance => _instance ?? (_instance = new UsbMon());

        public List<string> ConnectedRemovableDrives
        {
            get => _connectedRemovableDrives;
            set => Set(ref _connectedRemovableDrives, value);
        }

        public bool WatchForUsbEvents
        {
            get => _watchForUsbEvents;
            set
            {
                Set(ref _watchForUsbEvents, value);

                if (value)
                    StartMonitoring();
                else
                    StopMonitoring();
            }
        }

        public event EventHandler<UsbConnectionArgs> OnUsbConnectedEvent;

        private void StartMonitoring()
        {
            OnUsbDisconnected(null, null); // Force us to rescan for disconnected drives

            _insertWatcher.Start();
            _removeWatcher.Start();
            LogWriter.Instance.WriteMsg("USB monitoring started!", LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
        }

        private void StopMonitoring()
        {
            OnUsbDisconnected(null, null); // Force us to rescan for disconnected drives

            _removeWatcher.Stop();
            _insertWatcher.Stop();
            LogWriter.Instance.WriteMsg("USB monitoring stopped!", LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _insertWatcher.Stop();
                _insertWatcher?.Dispose();

                _queryRemoveWatcher.Stop();
                _queryRemoveWatcher?.Dispose();

                _removeWatcher.Stop();
                ;
                _removeWatcher?.Dispose();
            }
        }

        // When a device is connected, iterate through all removables
        private void OnUsbConnected(object sender, EventArrivedEventArgs e)
        {
            if (!WatchForUsbEvents)
            {
                LogWriter.Instance.WriteMsg("A newly connected USB device was detected, however monitoring is disabled!", LogWriter.MsgType.Debug, Thread.CurrentThread.ManagedThreadId);
                return;
            }

            // detect newly connected & ready drives
            var drives = DriveInfo.GetDrives().Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable);
            var driveInfos = drives as IList<DriveInfo> ?? drives.ToList();

            foreach (var drive in driveInfos)
            {
                var driveName = drive.Name.Replace("\\", string.Empty);

                if (!ConnectedRemovableDrives.Contains(driveName))
                {
                    ConnectedRemovableDrives.Add(driveName); // watch the root of the drive

                    var args = new UsbConnectionArgs {Drive = driveName};
                    OnUsbConnected(args);
                }
            }
        }

        protected virtual void OnUsbConnected(UsbConnectionArgs args)
        {
            var handler = OnUsbConnectedEvent;

            // Raise an event
            handler?.Invoke(this, args);
        }

        private void OnUsbQueryDisconnect(object sender, EventArrivedEventArgs e)
        {
        }

        private void OnUsbDisconnected(object sender, EventArrivedEventArgs e)
        {
            try
            {
                lock (ConnectedRemovableDrives)
                {
                    var drives = DriveInfo.GetDrives()
                        .Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable);
                    var driveInfos = drives as IList<DriveInfo> ?? drives.ToList();
                    var drivesNow = new List<string>();

                    foreach (var drive in driveInfos)
                    {
                        var driveName = drive.Name.Replace("\\", string.Empty);
                        drivesNow.Add(driveName);
                    }

#pragma warning disable 1587
                    /// 
                    /// TODO: DO NOT CHANGE THIS TO A FOREACH LOOP!
                    /// 
#pragma warning restore 1587
                    for (var it = 0; it < ConnectedRemovableDrives.Count; it++)
                        if (!drivesNow.Contains(ConnectedRemovableDrives[it]))
                            ConnectedRemovableDrives.Remove(ConnectedRemovableDrives[it]);
                }
            }
            catch (Exception exception)
            {
                LogWriter.Instance.WriteMsg($"Exception encountered! Details: {exception.Message}",
                    LogWriter.MsgType.Error, Thread.CurrentThread.ManagedThreadId);
            }
        }
    }
}