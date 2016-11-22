namespace robot_editor.Interfaces
{
    /// <summary>
    ///     An interface for file dialog services.
    ///     Allows the 'Main Window' to handle the file dialogs for the view-model.
    /// </summary>
    public interface IDialogProvider
    {
        /// <summary>
        ///     This method allows the user to select a file to open (so the view-model can implement 'Open File' functionality).
        /// </summary>
        bool UserSelectsFileToOpen(out string filePath);

        /// <summary>
        ///     This method allows the user to select a new filename for an existing file (so the view-model can implement 'Save
        ///     As' functionality).
        /// </summary>
        bool UserSelectsNewFilePath(string oldFilePath, out string newFilePath);

        /// <summary>
        ///     Display an error message dialog box.
        /// </summary>
        void ErrorMessage(string msg);

        /// <summary>
        ///     Allow the user to confirm whether they want to close a modified document.
        /// </summary>
        bool QueryCloseModifiedDocument(IEditor document);

        /// <summary>
        ///     Allow the user to confirm whether they want to close the application when 1 or more documents are modified.
        /// </summary>
        bool QueryCloseApplicationWhenDocumentsModified();
    }
}
