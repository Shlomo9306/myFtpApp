using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MyFileWpfFileExplorer
{
    /// <summary>
    /// A view model for each directory item
    /// </summary>
    public class DirectoryItem : BaseViewModel
    {

        #region Public Static Properties
        /// <summary>
        /// Currens selected File Or Folder Or Drive
        /// </summary>
        public static DirectoryItem CurrentItem { get; set; }
        /// <summary>
        /// Currens selected FTP  File Or Folder
        /// </summary>
        public static DirectoryItem CurrentFTPItem { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The type of this item
        /// </summary>
        public DirectoryItemType? Type { get; set; }

        /// <summary>
        /// The full path to the item
        /// </summary>
        public string FullPath { get; set; }
        /// <summary>
        /// The path to the item
        /// </summary>
        public string Path => this.Type == DirectoryItemType.Drive ? this.FullPath : DirectoryStructure.GetPathName(this.FullPath);

        /// <summary>
        /// The name of this directory item
        /// </summary>
        public string Name => this.Type == DirectoryItemType.Drive ? this.FullPath : DirectoryStructure.GetFileFolderName(this.FullPath);

        /// <summary>
        /// A list of all children contained inside this item
        /// </summary>
        public ObservableCollection<DirectoryItem> Children { get; set; }


        /// <summary>
        /// Indicates if this item can be expanded
        /// </summary>
        public bool CanExpand => this.Type != DirectoryItemType.File && this.Type != null;
        #endregion

        #region settings for when the CurentItem is selected or expended
        /// <summary>
        /// Indicates if the current item is expanded or not
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return this.Children?.Count(f => f != null) > 0;
            }
            set
            {
                // If the UI tells us to expand...
                if (value == true)
                    // Find all children
                    Expand();
                // If the UI tells us to close
                else
                    this.SetUpChildren();
            }
        }
        public bool IsFTPExpanded
        {
            get
            {
                return this.Children?.Count(f => f != null) > 0;
            }
            set
            {
                // If the UI tells us to expand...
                if (value == true)
                    // Find all children
                    ExpandFTP();
                // If the UI tells us to close
                else
                    this.SetUpChildren();
            }
        }
        /// <summary>
        /// Indicates if the current item is selected or not
        /// </summary>
        private bool _isSelected;
        private bool _isFTPSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; if (IsSelected) CurrentItem = this; } }

        public bool IsFTPSelected { get => _isFTPSelected; set { _isFTPSelected = value; if (IsFTPSelected) CurrentFTPItem = this; } }

        #endregion

        #region Public Commands

        public ICommand ExpandCommand { get; set; }
        public ICommand OpenFileCommand { get; set; }
        public ICommand OpenFtpFileCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fullPath">The full path of this item</param>
        /// <param name="type">The type of item</param>
        public DirectoryItem(string fullPath, DirectoryItemType? type)
        {
            // Create commands
            ConfigureCommands();

            // Set path and type
            this.FullPath = fullPath;
            this.Type = type;

            // Setup the children as needed
            this.SetUpChildren();

        }

        #endregion

        #region Command Methods
        /// <summary>
        ///  Expands this directory and finds all children
        /// </summary>
        private void Expand(object obj = null)
        {
            // We cannot expand a file
            if (this.Type == DirectoryItemType.File)
            {
                return;
            }

            // Find all children
            var children = DirectoryStructure.GetDirectoryContents(this.FullPath);
            this.Children = new ObservableCollection<DirectoryItem>(
                                children.Select(content => new DirectoryItem(content.FullPath, content.Type)));

        }
        private void ExpandFTP()
        {
            // We cannot expand a file
            if (this.Type == DirectoryItemType.File)
            {
                return;
            }

            // Find all children
            FTPDirectoryStructure.Host = FullPath +"/";
            var children = FTPDirectoryStructure.GetFTPDirectoryContents();
            this.Children = new ObservableCollection<DirectoryItem>(
                                children.Select(content => new DirectoryItem(content.FullPath, content.Type)));
        }
        private void OpenFile(object obj)
        {
            if (this.Type == DirectoryItemType.File)
            {
                System.Diagnostics.Process.Start(FullPath);
            }
            else
            {
                Expand();
            }
        }
        private void OpenFtpFile(object obj)
        {
            if (this.Type == DirectoryItemType.File)
            {
                System.Diagnostics.Process.Start(FullPath);
            }
            else
            {
                ExpandFTP();
            }
        }
        #endregion

        #region Helper Methods

        private void ConfigureCommands()
        {
            ExpandCommand = new RelayCommand(Expand, null);
            OpenFileCommand = new RelayCommand(OpenFile, null);
            OpenFtpFileCommand = new RelayCommand(OpenFtpFile, null);
        }
        /// <summary>
        /// Removes all children from the list, adding a dummy item to show the expand icon if required
        /// </summary>
        private void SetUpChildren()
        {
            // Clear items
            this.Children = new ObservableCollection<DirectoryItem>();

            // Show the expand arrow if we are not a file
            if (this.Type != DirectoryItemType.File)
                this.Children.Add(null);
        }
        public ObservableCollection<DirectoryItem> GetItemChildren()
        {
            Expand();
            return CurrentItem.Children;
        }
        #endregion
    }
}
