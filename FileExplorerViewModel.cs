using System;
using SQLData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace MyFileWpfFileExplorer
{
    /// <summary>
    /// The view model for the applications main Directory view
    /// </summary>
    public class FileExplorerViewModel : BaseViewModel
    {
        #region private Fields
        private IRepository<FilesProccessed> _filesProccessedRepository;
        private IUnitOfWork _unitOfWork;
        #endregion

        #region Public Properties
        /// <summary>
        /// A list of all directories on the machine
        /// </summary>
        public ObservableCollection<DirectoryItem> Items { get; set; }
        /// <summary>
        /// A list of all directories on the ftpServer
        /// </summary>
        public ObservableCollection<DirectoryItem> FTPItems { get; set; }

        public string FtpUserName { get; set; }

        public string FtpPasssword { get; set; }

        public string FtpHost { get; set; }

        public string ConfigFtpUserName
        {
            get => AppSettings.GetKeysValue("UserName");
            set => AppSettings.SetKeysValue("UserName", value);
        }
        public string ConfigFtpPasssword
        {
            get => AppSettings.GetKeysValue("Password");
            set => AppSettings.SetKeysValue("Password", value);
        }
        public string ConfigFtpHost
        {
            get => AppSettings.GetKeysValue("Host");
            set => AppSettings.SetKeysValue("Host", value);
        }

        #endregion

        #region Public Commands

        public ICommand GoToCommand { get; set; }
        public ICommand GoToFTPCommand { get; set; }
        public ICommand DownloadCommand { get; set; }
        public ICommand UploadCommand { get; set; }
        public ICommand ProcessFileCommand { get; set; }
        public ICommand ClearFtpCommand { get; set; }
        public ICommand ChangeDefaultFtpInfoCommand { get; set; }
        public ICommand GetDefaultFtpInfoCommand { get; set; }
        #endregion

        #region Constructor

        /// <summary>
        /// constructor
        /// </summary>
        public FileExplorerViewModel()
        {
            ConfigureCommands();
            ConfigureRepository();
            // Get the logical drives
            var drives = DirectoryStructure.GetLogicalDrives();

            // Create the view models from the data
            this.Items = new ObservableCollection<DirectoryItem>(
                drives.Select(drive => new DirectoryItem(drive.FullPath, DirectoryItemType.Drive)));
            FTPDirectoryStructure.Exception += HandleExcepetion;
        }

        #endregion

        /// <summary>
        /// Handles exception that was reised from the view Model when thw event FTPDirectoryStructure.Exception was raised
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region Handle Exception
        private void HandleExcepetion(object sender, EventArgs e)
        {
            System.Windows.MessageBox.Show(((Exception)sender).Message);
        } 
        #endregion

        #region CommandMethods
        private void ExecuteUpload(object obj)
        {
            FTPDirectoryStructure.UploadFile(DirectoryItem.CurrentItem, DirectoryItem.CurrentFTPItem);
            //TODO before adding the item check if the item Already Exits
            DirectoryItem di = new DirectoryItem(DirectoryItem.CurrentFTPItem.FullPath + @"/" + DirectoryItem.CurrentItem.Name, DirectoryItem.CurrentItem.Type);
            DirectoryItem.CurrentFTPItem.Children.Add(di);
        }
        private void ExecuteDownload(object obj)
        {
           var doenloadedFiles = FTPDirectoryStructure.DownloadFile(DirectoryItem.CurrentFTPItem, DirectoryItem.CurrentItem);
            //TODO before adding the item check if the item Already Exits
            DirectoryItem di = new DirectoryItem(DirectoryItem.CurrentItem.FullPath + @"\\" + DirectoryItem.CurrentFTPItem.Name, DirectoryItem.CurrentFTPItem.Type);
            DirectoryItem.CurrentItem.Children.Add(di);

        }
        private void ExecuteGoToFTP(object obj)
        {
            FTPDirectoryStructure.UserName = FtpUserName;
            FTPDirectoryStructure.Password = FtpPasssword;
            FTPDirectoryStructure.Host = FtpHost;
            var children = FTPDirectoryStructure.GetFTPDirectoryContents();

            this.FTPItems = new ObservableCollection<DirectoryItem>(
                children.Select(ftpFile => new DirectoryItem(ftpFile?.FullPath, ftpFile?.Type)));
            DirectoryItem.CurrentFTPItem = new DirectoryItem(obj.ToString(), DirectoryItemType.Folder);
            DirectoryItem.CurrentFTPItem.Children = FTPItems;
        }
        private void ExecuteGoTo(object file)
        {
            var fileToGoTo = file as string;
            if (string.IsNullOrEmpty(fileToGoTo))
            {
                var drives = DirectoryStructure.GetLogicalDrives();
                this.Items = new ObservableCollection<DirectoryItem>(
                    drives.Select(drive => new DirectoryItem(drive.FullPath, DirectoryItemType.Drive)));
                return;
            }
            var children = DirectoryStructure.GetDirectoryContents(fileToGoTo);

            this.Items = new ObservableCollection<DirectoryItem>(
                children.Select(drive => new DirectoryItem(drive.FullPath, drive.Type)));
        }
        private void ExecuteProcessFile(object obj)
        {
            ConfigureRepository();
            var destinationDownloadedFiles =
                FTPDirectoryStructure.DownloadFile(DirectoryItem.CurrentFTPItem, DirectoryItem.CurrentItem);
            DirectoryItem.CurrentItem.IsExpanded = true;

            var sqlFileList = _filesProccessedRepository.GetAll();
            if (destinationDownloadedFiles.Count == 1 && destinationDownloadedFiles[0].Type == DirectoryItemType.Folder)
            {
                var dir = destinationDownloadedFiles[0];
                destinationDownloadedFiles = dir.GetItemChildren().ToList();
            }
            var currentFtpItemFullPath = DirectoryItem.CurrentFTPItem.Type == DirectoryItemType.Folder ? String.Copy(DirectoryItem.CurrentFTPItem.FullPath) : String.Copy(DirectoryItem.CurrentFTPItem.Path);

            foreach (var file in destinationDownloadedFiles)
            {
                if (file.Type == DirectoryItemType.File)
                {
                    var f = sqlFileList.SingleOrDefault(fi => fi.FileName == file.FullPath);
                    if (f != null) continue;
                    var suspicoousResults = _unitOfWork.SqlStoredProcedure<usp_proccessFile_Result>("usp_proccessFile @FilePathName",
                        new SqlParameter("FilePathName", SqlDbType.NVarChar) { Value = file.FullPath }).ToList();
                   
                    if (suspicoousResults.Count > 0)
                    {
                        DirectoryItem suspiciousFile =
                            CreateFile(file.Path, "SuspiciousCodes " + file.Name, suspicoousResults);
                        if (!FTPDirectoryStructure.FtpDirectoryExits(currentFtpItemFullPath, "SuspiciousFiles"))
                        {
                                FTPDirectoryStructure.CreateFtpDirectory(DirectoryItem.CurrentFTPItem,
                                    "SuspiciousFiles");
                        }

                        DirectoryItem.CurrentFTPItem.FullPath = currentFtpItemFullPath + @"/SuspiciousFiles";
                        FTPDirectoryStructure.UploadFile(suspiciousFile, DirectoryItem.CurrentFTPItem);
                        DirectoryItem.CurrentFTPItem.FullPath = currentFtpItemFullPath;

                        if (DirectoryItem.CurrentFTPItem.Type == DirectoryItemType.Folder)
                        {
                            //setting toIsFTPExpanded false so the propertyChanget should fire, Fody Weaver some how is blocking the setter of value to some value to be set
                            DirectoryItem.CurrentFTPItem.IsFTPExpanded = false;
                            //to refresh to folder
                            DirectoryItem.CurrentFTPItem.IsFTPExpanded = true;
                        }
                        else
                        {
                            DirectoryItem directoryItem = FindDirectoryItem(DirectoryItem.CurrentFTPItem.FullPath);
                            directoryItem.IsFTPExpanded = false;
                            directoryItem.IsFTPExpanded = true;
                        }
                    }
                }
            }

            System.Windows.MessageBox.Show("Process Complete");
        }

        public void ExecuteClearFtp(object obj)
        {
            this.FtpUserName = "";
            this.FtpPasssword = "";
            this.FtpHost = "";
            this.FTPItems = null;

        }

        private void ExecuteChangeDefaultFtpInfo(object obj)
        {
            ConfigFtpUserName = FtpUserName;
            ConfigFtpPasssword = FtpPasssword;
            ConfigFtpHost = FtpHost;
        }
        private void ExecuteGetDefaultFtpInfo(object obj)
        {
            FtpUserName = ConfigFtpUserName;
            FtpPasssword = ConfigFtpPasssword;
            FtpHost = ConfigFtpHost;
        }
        #endregion

        #region CouldExecuteCommand
        private bool CanExecuteExecuteGoTo(object obj) => File.Exists(obj.ToString()) || Directory.Exists(obj.ToString());
        private bool CanExecuteExecuteGoToFtp(object obj) => !string.IsNullOrEmpty(obj.ToString());
        private bool CanExecuteExecuteDownload(object obj) => DirectoryItem.CurrentItem != null && DirectoryItem.CurrentFTPItem != null;
        private bool CanExecuteExecuteUpload(object obj) => DirectoryItem.CurrentItem != null && DirectoryItem.CurrentFTPItem != null;
        private bool CanExecuteExecuteProcessFile(object obj) => DirectoryItem.CurrentItem != null && DirectoryItem.CurrentFTPItem != null;
        private bool CanExecuteExecuteClearFtp(object obj) => !string.IsNullOrEmpty(FtpUserName) || !string.IsNullOrEmpty(FtpPasssword) || !string.IsNullOrEmpty(FtpHost);
        private bool CanExecuteExecuteChangeDefaultFtpInfo(object obj) => true;
        private bool CanExecuteExecuteGetDefaultFtpInfo(object obj) => true;
        #endregion

        #region HelperMethods
        private void ConfigureRepository()
        {
            _unitOfWork = new UnitOfWork();
            _filesProccessedRepository = _unitOfWork.GetRepository<FilesProccessed>();

        }

        public void ConfigureCommands()
        {
            GoToCommand = new RelayCommand(ExecuteGoTo, CanExecuteExecuteGoTo);
            GoToFTPCommand = new RelayCommand(ExecuteGoToFTP, CanExecuteExecuteGoToFtp);
            DownloadCommand = new RelayCommand(ExecuteDownload, CanExecuteExecuteDownload);
            UploadCommand = new RelayCommand(ExecuteUpload, CanExecuteExecuteUpload);
            ProcessFileCommand = new RelayCommand(ExecuteProcessFile, CanExecuteExecuteProcessFile);
            ClearFtpCommand = new RelayCommand(ExecuteClearFtp, CanExecuteExecuteClearFtp);
            ChangeDefaultFtpInfoCommand = new RelayCommand(ExecuteChangeDefaultFtpInfo, CanExecuteExecuteChangeDefaultFtpInfo);
            GetDefaultFtpInfoCommand = new RelayCommand(ExecuteGetDefaultFtpInfo, CanExecuteExecuteGetDefaultFtpInfo);
        }
        private DirectoryItem CreateFile(string filePath, string fileName, List<usp_proccessFile_Result> claimResult)
        {
            var suspiciousFile = new DirectoryItem(filePath + @"\" + fileName, DirectoryItemType.File);
            using (var file = new StreamWriter(suspiciousFile.FullPath))
            {
                foreach (var line in claimResult)
                {
                    file.WriteLine(line.ClaimNumber + "," + line.Code);
                }
            }

            return suspiciousFile;
        }

        private DirectoryItem FindDirectoryItem(string fullpath)
        {
            DirectoryItem directoryItem = FTPItems.SingleOrDefault(i => i.FullPath == fullpath);
            return directoryItem;
        }

        #endregion
    }
}
