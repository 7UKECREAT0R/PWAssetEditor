using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PWAssetEditor.Library;

namespace PWAssetEditor
{
    public partial class CleanupDisplay : Form
    {
        private int deletedFiles;
        private readonly int totalEntries;
        private readonly string[] nonJSONFileEntries;
        private readonly AssetLibrary libraryToCleanup;

        public CleanupDisplay(AssetLibrary libraryToCleanup)
        {
            InitializeComponent();

            // collect files that are not .JSON in the "assets" folder. this will be the target.
            this.nonJSONFileEntries = Directory
                .EnumerateFiles(libraryToCleanup.GetAssetsDirectory(), "*", SearchOption.AllDirectories)
                .Where(file => !Path.GetExtension(file).Equals(".JSON", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            this.totalEntries = this.nonJSONFileEntries.Length;

            this.libraryToCleanup = libraryToCleanup;
            this.progressBar.Maximum = 100;
            this.progressBar.Value = 0;

            this.backgroundWorker.DoWork += (sender, args) =>
            {
                var worker = (BackgroundWorker) sender;
                var self = (CleanupDisplay) args.Argument;
                self.deletedFiles = 0;

                // gets an array of all non-json assets
                string[] usedFiles = self.libraryToCleanup.Assets
                    .SelectMany(a => a.GetAllReferencedFiles)
                    .Where(file => !Path.GetExtension(file).Equals(".JSON", StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .ToArray();

                string[] files = self.nonJSONFileEntries;
                int progress = 0;

                foreach (string file in files)
                {
                    if (worker.CancellationPending)
                        return;

                    try
                    {
                        // is this file in the usedFiles array? if not, it's unused.
                        if (!usedFiles.Contains(file))
                        {
                            // delete the file
                            File.Delete(file);
                            this.deletedFiles++;

                            // if the containing directory is empty, remove it as well
                            string directory = Path.GetDirectoryName(file);
                            if (string.IsNullOrEmpty(directory))
                                continue;
                            if (!Directory.EnumerateFiles(directory).Any())
                                Directory.Delete(directory);
                        }
                    }
                    catch (IOException e)
                    {
                        Debug.WriteLine(e.ToString());
                    }
                    finally
                    {
                        worker.ReportProgress(++progress);
                    }
                }
            };
        }
        private void SaveDisplay_Load(object sender, EventArgs e)
        {
            this.backgroundWorker.RunWorkerAsync(this);
        }
        private void SetTitleProgress(int progress)
        {
            this.Text = $"Cleaning up asset files... ({progress}/{this.totalEntries})";
        }
        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int newProgress = e.ProgressPercentage;
            SetTitleProgress(newProgress);
            this.progressBar.Value = newProgress;
        }
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled && this.userRequestedCancellation)
            {
                this.DialogResult = DialogResult.Cancel;
            }
            else
            {
                string msg;
                switch (this.deletedFiles)
                {
                    case 0:
                        msg = "No unused files were found.";
                        break;
                    case 1:
                        msg = $"Deleted {this.deletedFiles} unused file from the asset library.";
                        break;
                    default:
                        msg = $"Deleted {this.deletedFiles} unused files from the asset library.";
                        break;
                }

                MessageBox.Show(msg, "Cleanup Results",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }

            this.workDone = true;
            Close();
        }

        private bool workDone;
        private bool userRequestedCancellation;
        private void SaveDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.workDone)
                return;

            // the user is trying to close the save dialog; request cancellation.
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (this.backgroundWorker.IsBusy && !this.userRequestedCancellation)
                    this.backgroundWorker.CancelAsync();

                e.Cancel = true;
                this.userRequestedCancellation = true;
            }
        }
    }
}