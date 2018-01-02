using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyFileWpfFileExplorer
{
    /// <summary>
    /// A helper class to query information about directories
    /// </summary>
    public static class DirectoryStructure
    {
        /// <summary>
        /// Gets all logical drives on the computer
        /// </summary>
        /// <returns></returns>
        public static List<DirectoryItem> GetLogicalDrives()
        {
            // Get every logical drive on the machine
            return Directory.GetLogicalDrives().Select(drive => new DirectoryItem(drive, DirectoryItemType.Drive)).ToList();
        }

        /// <summary>
        /// Gets the directories top-level content
        /// </summary>
        /// <param name="fullPath">The full path to the directory</param>
        /// <returns></returns>
        public static List<DirectoryItem> GetDirectoryContents(string fullPath)
        {
            // Create empty list
            var items = new List<DirectoryItem>();

            #region Get Folders
            
            // Try and get directories from the folder
            // ignoring any issues doing so
            try
            {
                var dirs = Directory.GetDirectories(fullPath);

                if (dirs.Length > 0)
                    items.AddRange(dirs.Select(dir => new DirectoryItem(dir,DirectoryItemType.Folder)));
            }
            catch { }

            #endregion

            #region Get Files

            // Try and get files from the folder
            // ignoring any issues doing so
            try
            {
                var fs = Directory.GetFiles(fullPath);

                if (fs.Length > 0)
                    items.AddRange(fs.Select(file => new DirectoryItem(file, DirectoryItemType.File)));
            }
            catch { }

            #endregion

            return items;
        }

        #region Helpers

        /// <summary>
        /// Find the file or folder name from a full path
        /// </summary>
        /// <param name="fullPath">The full path</param>
        /// <returns></returns>
        public static string GetFileFolderName(string fullPath)
        {
            // If we have no path, return empty
            if (string.IsNullOrEmpty(fullPath))
                return string.Empty;

            // Make all slashes back slashes
            var normalizedPath = fullPath.Replace('/', '\\');
            if (normalizedPath.EndsWith("\\"))
            {
               normalizedPath = normalizedPath.Remove(normalizedPath.LastIndexOf("\\", StringComparison.Ordinal));}
            // Find the last backslash in the path
            var lastIndex = normalizedPath.LastIndexOf('\\');

            // If we don't find a backslash, return the path itself
            if (lastIndex <= 0)
                return fullPath;

            // Return the name after the last back slash
            return fullPath.Substring(lastIndex + 1);
        }
        /// <summary>
        /// Find the path from a full path
        /// </summary>
        /// <param name="fullPath">The full path</param>
        /// <returns></returns>
        public static string GetPathName (string fullPath)
        {
            // If we have no path, return empty
            if (string.IsNullOrEmpty(fullPath))
                return string.Empty;

            var normalizedPath = fullPath.Replace('/', '\\');

            // Find the last backslash in the path
            var lastIndex = normalizedPath.LastIndexOf('\\');

            if (lastIndex <= 0)
                return fullPath;
            return fullPath.Substring(0, lastIndex);
        }

        #endregion
    }
}
