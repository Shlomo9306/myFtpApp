using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace MyFileWpfFileExplorer
{
    /// <summary>
    /// A helper class to query information about directories
    /// </summary>
    public static class FTPDirectoryStructure
    {

        #region private Fields
        public static string UserName;
        public static string Password;
        public static string Host;
        public static EventHandler Exception;
        private static FtpWebRequest _request;
        private static readonly List<DirectoryItem> DestinationDowloadedFiles = new List<DirectoryItem>();

        #endregion
        //TODO Make async
        #region Public static Methods
        public static List<DirectoryItem> GetFTPDirectoryContents()
        {
            // Create empty list
            var items = new List<DirectoryItem>();

            #region Get Folders

            // Try and get directories from the folder
            // ignoring any issues doing so

            try
            {
                var dirs = GetDirectories();

                if (dirs.Count > 0)
                {
                    items.AddRange(dirs.Select(dir =>
                        new DirectoryItem(Host + @"/" + GetFileFolderName(dir), DirectoryItemType.Folder)));
                }
                //else { items.Add(null);}
            }
            //TODO handle exceptions
            catch (Exception e) { Exception?.Invoke(e, EventArgs.Empty); }

            #endregion

            #region Get Files

            // Try and get files from the folder
            // ignoring any issues doing so
            try
            {
                var fs = GetFiles();

                if (fs.Count > 0)
                    items.AddRange(fs.Select(file => new DirectoryItem(Host + @"/" + GetFileFolderName(file), DirectoryItemType.File)));
            }
            catch (Exception e) { Exception?.Invoke(e, EventArgs.Empty); }

            #endregion

            return items;
        }
        //TODO make async
        public static List<DirectoryItem> DownloadFile(DirectoryItem fileToDownload, DirectoryItem locationToDownload = null, string locationToDownloadString = null)
        {
            if (fileToDownload.Type == DirectoryItemType.File)
            {
                Host = fileToDownload.FullPath;
                _request = BuildUri();
                _request.Method = WebRequestMethods.Ftp.DownloadFile;
                try
                {
                    using (Stream ftpStream = _request.GetResponse().GetResponseStream())
                    {
                        string locationToDownloadPath = locationToDownload == null ? locationToDownloadString : locationToDownload.Type == DirectoryItemType.File ? locationToDownload.Path : locationToDownload.FullPath;
                        using (Stream fileStream = File.Create(locationToDownloadPath + @"\" + fileToDownload.Name))
                        {
                            ftpStream?.CopyTo(fileStream);
                            DestinationDowloadedFiles.Add(new DirectoryItem(locationToDownloadPath + @"\" + fileToDownload.Name, DirectoryItemType.File));
                        }
                    }
                }
                //TODO Handle Exceptions
                catch (WebException e)
                {

                    String status = ((FtpWebResponse)e.Response).StatusDescription;
                    Console.WriteLine(status);
                }
            }
            else
            {
                string downloadLocation = locationToDownload == null ? locationToDownloadString : locationToDownload.Type == DirectoryItemType.Folder ? locationToDownload.FullPath : locationToDownload.Path;
                var dir = Directory.CreateDirectory(downloadLocation + @"\" + fileToDownload.Name);
                Host = fileToDownload.FullPath;
                List<DirectoryItem> folderContent = GetFTPDirectoryContents();
                foreach (var item in folderContent)
                {
                    DownloadFile(item, locationToDownloadString: dir.FullName);
                    if (item.Type == DirectoryItemType.File) continue;
                    DestinationDowloadedFiles.Add(new DirectoryItem(downloadLocation + @"\" + fileToDownload.Name, item.Type));
                }
            }
            return DestinationDowloadedFiles;

        }
        //TODO make async
        public static void UploadFile(DirectoryItem fileToUpload, DirectoryItem uploadTofullPath)
        {
            Host = uploadTofullPath.FullPath + @"/" + fileToUpload.Name;
            if (fileToUpload.Type == DirectoryItemType.Folder)
            {
                _request = BuildUri();
                _request.Method = WebRequestMethods.Ftp.MakeDirectory;
                using (var resp = (FtpWebResponse)_request.GetResponse())
                {
                    var files = Directory.GetFiles(fileToUpload.FullPath);
                    foreach (var f in files)
                    {
                        DirectoryItem toUpload = new DirectoryItem(f, DirectoryItemType.File);
                        DirectoryItem uploadTo = new DirectoryItem(uploadTofullPath.FullPath + @"/" + fileToUpload.Name, DirectoryItemType.Folder);
                        UploadFile(toUpload, uploadTo);
                    }
                }
            }
            else
            {
                //Host = uploadTofullPath.FullPath + @"/"+ fileToUpload.Name; 
                _request = BuildUri();
                _request.Method = WebRequestMethods.Ftp.UploadFile;
                using (Stream fileStream = File.OpenRead(fileToUpload.FullPath))
                using (Stream ftpStream = _request.GetRequestStream())
                {
                    fileStream.CopyTo(ftpStream);
                }
            }

        }
        private static List<string> GetDirectories()
        {
            StreamReader readerDetails = ExecuteRequest();
            List<string> responseDirectoryDetails = new List<string>();
            var folders = string.Concat(readerDetails.ReadToEnd().ToList());
            if (!string.IsNullOrEmpty(folders))
            {
                responseDirectoryDetails = folders
                        .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(r => (r.Substring(r.LastIndexOf(' ') + 1, 1) != ".") &&
                                (r.Contains("<DIR>") || r[0].ToString() == "d")).ToList();
            }

            return responseDirectoryDetails;

        }
        private static List<string> GetFiles()
        {
            StreamReader readerDetails = ExecuteRequest();
            List<string> responseFilesDetails = new List<string>();
            var files = string.Concat(readerDetails.ReadToEnd().ToList());
            if (!string.IsNullOrEmpty(files))
            {
                responseFilesDetails = files
                        .Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(r => (r.Substring(r.LastIndexOf(' ') + 1, 1) != ".") &&
                                (!r.Contains("<DIR>") && r[0].ToString() != "d")).ToList();
            }

            return responseFilesDetails;
        }
        /// <summary>
        /// Biulds the requset string with FTP Server Credentials 
        /// </summary>
        /// <param name="host">The full path</param>
        /// <returns></returns>
        private static FtpWebRequest BuildUri()
        {
            _request = (FtpWebRequest)WebRequest.Create(Host);
            _request.Credentials = new NetworkCredential(UserName, Password);
            return _request;

        } 
        #endregion

        #region Helpers
        /// <summary>
        /// Executes the requset fo FTP Server and returns the StreamReader for the request
        /// </summary>
        /// <param name="path">The full path</param>
        /// <returns></returns>
        private static StreamReader ExecuteRequest()
        {
            _request = BuildUri();
            _request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            var responseDetails = (FtpWebResponse)_request.GetResponse();
            Stream responseDetailsStream = responseDetails.GetResponseStream();
            StreamReader readerDetails = new StreamReader(responseDetailsStream);
            return readerDetails;
        }
        /// <summary>
        /// Find the file or folder name from a full path
        /// </summary>
        /// <param name="path">The full path</param>
        /// <returns></returns>
        public static string GetFileFolderName(string path)
        {
            // If we have no path, return empty
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            string Name = "";

            String[] directorytokens = path.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (path[0].ToString() == "d" || path[0].ToString() == "-")
            {
                for (int i = 8; i < directorytokens.Length; i++)
                {
                    Name += " " + directorytokens[i];

                }
            }
            else
            {
                for (int i = 3; i < directorytokens.Length; i++)
                {
                    Name += " " + directorytokens[i];
                }

            }
            Name = Name.TrimEnd(' ');
            Name = Name.TrimStart(' ');
            return Name;
        }
        /// <summary>
        /// Creates a Directory in the FTP Server
        /// </summary>
        /// <param name="fullPath">full host nae from server</param>
        /// <param name="directoryName">the name of directory to create</param>
        /// <returns></returns>
        public static string CreateFtpDirectory(DirectoryItem fullPath, string directoryName)
        {
            Host = fullPath.FullPath + @"/" + directoryName;
            _request = BuildUri();
            _request.Method = WebRequestMethods.Ftp.MakeDirectory;
            using (var resp = (FtpWebResponse)_request.GetResponse())
            {
            };
            return Host;
        }
        /// <summary>
        /// Checks if a directory exits in ftp serer
        /// </summary>
        /// <param name="directoryItem"></param>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        public static bool FtpDirectoryExits(string directoryItem, string directoryName = null)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryItem + @"/" + directoryName);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch (WebException)
            {
                return false;
            }
        }

        #endregion
    }
}
//public static List<string> ReadFile(DirectoryItem item)
//{
//    string data = "";
//    List<string> dataFromFTPFile = new List<string>();
//    Host = item.FullPath;
//    _request = BuildURI();
//    _request.Method = WebRequestMethods.Ftp.DownloadFile;
//    try
//    {
//        using (Stream ftpStream = _request.GetResponse().GetResponseStream())
//        {
//            using (StreamReader ftpReader = new StreamReader(ftpStream))
//            {
//                data = ftpReader.ReadToEnd();
//                dataFromFTPFile = Regex.Split(data, "\r\n").ToList();
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine(ex.Message);
//        throw;
//    }
//    return dataFromFTPFile;
//}

