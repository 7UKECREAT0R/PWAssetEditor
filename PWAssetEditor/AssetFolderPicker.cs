using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using PWAssetEditor.Library;

namespace PWAssetEditor
{
    /// <summary>
    /// Allows the user to choose a Prego Wars asset folder and confirm it.
    /// Upon confirmation, an <see cref="AssetEditor"/> form will be opened configured with the selected folder.
    /// </summary>
    public partial class AssetFolderPicker : Form
    {
        private const string TITLE = "Prego Wars Asset Editor";
        private readonly string TEMP_FILE = Path.Combine(Path.GetTempPath(), "PWAssetEditor-LastDirectory.txt");

        private AssetLibrary loadedLibrary;

        private void SetFolderDisplayText(string value, bool writeTempFile)
        {
            if (value == null)
            {
                this.currentFolderDisplay.Text = string.Empty;
                this.openButton.Enabled = false;
                if (File.Exists(this.TEMP_FILE))
                    File.Delete(this.TEMP_FILE);
                return;
            }

            if (!AssetLibrary.IsValidAssetsDirectory(value))
            {
                MessageBox.Show("The selected directory is not a valid Prego Wars assets directory.",
                    TITLE,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            this.currentFolderDisplay.Text = value;
            this.openButton.Enabled = true;
            if (writeTempFile)
                File.WriteAllText(this.TEMP_FILE, value);
        }
        public AssetFolderPicker()
        {
            InitializeComponent();
        }
        private void AssetFolderPicker_Load(object sender, EventArgs e)
        {
            // check and load the contents of the temp file if valid.
            if (!File.Exists(this.TEMP_FILE))
                return;

            string lastPick = File.ReadAllText(this.TEMP_FILE).Trim();
            if (string.IsNullOrEmpty(lastPick))
                return;
            if (!Directory.Exists(lastPick))
                return;

            SetFolderDisplayText(lastPick, false); // loaded from temp file; we don't need to re-write it.
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            DialogResult result = this.folderBrowserDialog.ShowDialog(this);

            if (result != DialogResult.OK)
                return;

            string selectedFolder = this.folderBrowserDialog.SelectedPath;

            retry:
            if (!Directory.Exists(selectedFolder))
            {
                result = MessageBox.Show($"The selected directory doesn't exist:\n\n{selectedFolder}",
                    TITLE,
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Error);

                if (result == DialogResult.Retry)
                    goto retry;

                return;
            }

            // all good
            SetFolderDisplayText(selectedFolder, true);
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            // open the AssetEditor with a new asset library created from the selected folder
            string selectedFolder = this.currentFolderDisplay.Text;

            if (string.IsNullOrWhiteSpace(selectedFolder))
            {
                MessageBox.Show("uh...");
                return;
            }

            if (this.loadedLibrary == null)
            {
                var library = new AssetLibrary(selectedFolder);
                this.progressBar.Enabled = true;
                this.progressBar.Value = 0;
                LoadAssets(library);
            }
            else
            {
                var editor = new AssetEditor(this.loadedLibrary, this);
                editor.Show();
                Hide();
            }
        }
        private void currentFolderDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && this.openButton.Enabled)
                this.openButton.PerformClick();
        }

        /// <summary>
        /// Load assets into the given asset library and display the errors, if any.
        /// Progress will be reported in the progress bar.
        /// </summary>
        /// <param name="library">The library to load.</param>
        private void LoadAssets(AssetLibrary library)
        {
            // load the assets from the library given in the constructor
            library.ClearAssets();
            this.asyncLoader.RunWorkerAsync(library);
        }

        private void asyncLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!(e.Argument is AssetLibrary library))
                return; // finish. no library?

            e.Result = library;
            var worker = (BackgroundWorker) sender;

            bool success = library.Load(out List<string> errorList, worker);

            if (!success)
            {
                string constructedMessage = string.Join("\n", errorList);
                MessageBox.Show("Error(s) occurred while loading Asset Library:\n\n" + constructedMessage);
            }
        }
        private void asyncLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int percent = e.ProgressPercentage;
            this.progressBar.Value = percent;
        }
        private void asyncLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!(e.Result is AssetLibrary library))
            {
                MessageBox.Show("Asset library got lost in transit");
                return;
            }

            this.loadedLibrary = library;
            this.progressBar.Value = 0;
            this.progressBar.Enabled = false;
            var editor = new AssetEditor(library, this);
            editor.Show();
            Hide();
        }
    }
}