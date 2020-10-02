using System.Windows;
using Microsoft.Win32;

namespace CVCapturePanel.Helpers
{
    /// <summary>
    ///     Class with open dialog for opening video files
    /// </summary>
    public class DialogService : IDialogService
    {
        public string FilePath { get; set; }

        public bool OpenFileDialog()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "Video Files |*.mp4|*.avi|";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                return true;
            }

            return false;
        }

        public bool SaveFileDialog()
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == false)
            {
                ShowMessage("File not saved");
                return false;
            }

            FilePath = saveFileDialog.FileName;
            ShowMessage("File has been saved");
            return true;
        }

        /// <summary>
        ///     Method for show modal attention window
        /// </summary>
        /// <param name="message"></param>
        public void ShowMessage(string message)
        {
            MessageBox.Show(message, "Caution!", MessageBoxButton.OK);
        }
    }
}