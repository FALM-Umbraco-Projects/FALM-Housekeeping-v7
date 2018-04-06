using System;
using System.IO;

namespace FALM.Housekeeping.Helpers
{
    /// <summary>
    /// HkDbHelper
    /// </summary>
    public static class HkFunctionsHelper
    {
        /// <summary>
        /// Size Suffixes
        /// </summary>
        private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        /// <summary>
        /// Get the size of given directory
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        /// <summary>
        /// Prettify dimensions
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        public static string SizeSuffix(long value, int decimalPlaces = 0)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }

            int i = 0;
            decimal dValue = (decimal)value;
            while (Math.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            string prettifiedDimension = string.Format("{0:n" + decimalPlaces + "} {1} ", dValue, SizeSuffixes[i]);
            prettifiedDimension = (SizeSuffixes[i] == "bytes") ? prettifiedDimension : string.Format("{0} ({1} bytes)", prettifiedDimension, value);

            return prettifiedDimension;
        }

        /// <summary>
        /// Delete folders recursively
        /// </summary>
        /// <param name="rootDir"></param>
        /// <param name="baseDir"></param>
        public static void DeleteFolderRecursive(DirectoryInfo rootDir, DirectoryInfo baseDir)
        {
            if (baseDir.Name != rootDir.Name)
                baseDir.Attributes = FileAttributes.Normal;

            foreach (var childDir in baseDir.GetDirectories())
                DeleteFolderRecursive(rootDir, childDir);

            foreach (var file in baseDir.GetFiles())
                file.IsReadOnly = false;

            if (baseDir.Name != rootDir.Name)
                baseDir.Delete(true);
        }

        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="rootDir"></param>
        /// <param name="fileToDelete"></param>
        public static void DeleteFile(DirectoryInfo rootDir, FileInfo fileToDelete)
        {
            fileToDelete.IsReadOnly = false;
            fileToDelete.Delete();
        }
    }
}