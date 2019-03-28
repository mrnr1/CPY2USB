using System;
using System.IO;
using System.Security.Cryptography;

namespace Cpy2Usb.Services
{
    public class Helpers
    {
        private static Helpers _instance;
        public static Helpers Instance => _instance ?? (_instance = new Helpers());

        public bool FilesMatch(string sourceFile, string destinationFile)
        {
            if (!File.Exists(sourceFile) || !File.Exists(destinationFile))
                return false;

            string sourceFileChecksum;
            string destinationFileChecksum;

            using (var stream = File.OpenRead(sourceFile))
            {
                var sha = new SHA256Managed();
                var checksum = sha.ComputeHash(stream);
                sourceFileChecksum = BitConverter.ToString(checksum).Replace("-", string.Empty);
            }

            using (var stream = File.OpenRead(destinationFile))
            {
                var sha = new SHA256Managed();
                var checksum = sha.ComputeHash(stream);
                destinationFileChecksum = BitConverter.ToString(checksum).Replace("-", string.Empty);
            }

            if (sourceFileChecksum == destinationFileChecksum)
                return true;
            return false;
        }
    }
}