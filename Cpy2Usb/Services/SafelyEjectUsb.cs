using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Cpy2Usb.Services
{
    public class SafelyEjectUsb
    {
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const int FILE_SHARE_READ = 0x1;
        private const int FILE_SHARE_WRITE = 0x2;
        private const int FSCTL_LOCK_VOLUME = 0x00090018;
        private const int FSCTL_DISMOUNT_VOLUME = 0x00090020;
        private const int IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;
        private const int IOCTL_STORAGE_MEDIA_REMOVAL = 0x002D4804;
        private static SafelyEjectUsb _instance;

        private readonly IntPtr handle = IntPtr.Zero;

        public static SafelyEjectUsb Instance => _instance ?? (_instance = new SafelyEjectUsb());

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr SecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped
        );

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            byte[] lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        ///     Constructor for the USBEject class
        /// </summary>
        /// <param name="driveLetter">This should be the drive letter. Format: F:/, C:/..</param>
        public IntPtr USBEject(string driveLetter)
        {
            var filename = @"\\.\" + driveLetter[0] + ":";
            return CreateFile(filename, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero,
                0x3, 0, IntPtr.Zero);
        }

        public bool Eject(IntPtr handle)
        {
            var result = false;

            if (LockVolume(handle) && DismountVolume(handle))
            {
                PreventRemovalOfVolume(handle, false);
                result = AutoEjectVolume(handle);
            }

            CloseHandle(handle);
            return result;
        }

        private bool LockVolume(IntPtr handle)
        {
            for (var i = 0; i < 10; i++)
            {
                if (DeviceIoControl(handle, FSCTL_LOCK_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out var byteReturned,
                    IntPtr.Zero)) return true;
                Thread.Sleep(500);
            }

            return false;
        }

        private bool PreventRemovalOfVolume(IntPtr handle, bool prevent)
        {
            var buf = new byte[1];

            buf[0] = prevent ? (byte) 1 : (byte) 0;
            return DeviceIoControl(handle, IOCTL_STORAGE_MEDIA_REMOVAL, buf, 1, IntPtr.Zero, 0, out var retVal,
                IntPtr.Zero);
        }

        private bool DismountVolume(IntPtr handle)
        {
            return DeviceIoControl(handle, FSCTL_DISMOUNT_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out var byteReturned,
                IntPtr.Zero);
        }

        private bool AutoEjectVolume(IntPtr handle)
        {
            return DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0,
                out var byteReturned, IntPtr.Zero);
        }

        private bool CloseVolume(IntPtr handle)
        {
            return CloseHandle(handle);
        }
    }
}