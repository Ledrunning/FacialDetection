namespace CVCapturePanel
{
    public interface IDialogService
    {
        /// <summary>
        ///     File path prop
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        ///     Method for show modal attention window
        /// </summary>
        /// <param name="message"></param>
        void ShowMessage(string message);

        /// <summary>
        ///     Method for creating open file dialog
        /// </summary>
        /// <returns></returns>
        bool OpenFileDialog();

        /// <summary>
        ///     Method for creating save file dialog
        /// </summary>
        /// <returns></returns>
        bool SaveFileDialog();
    }
}