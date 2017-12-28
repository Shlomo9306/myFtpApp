using System;
using System.Windows.Input;

namespace MyFileWpfFileExplorer
{
    /// <summary>
    /// A basic command that runs an Action
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Private Members

        
        Action<object> executeAction;
        Func<object, bool> canExecute;

        #endregion

        #region Constructor

        public RelayCommand(Action<Object> executeAction, Func<object, bool> canExecute)
        {
            this.executeAction = executeAction;
            this.canExecute = canExecute;
        }

        #endregion


        #region Public Events

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        #endregion

       
        #region Command Methods

        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            executeAction(parameter);
        }

        #endregion
    }
}
