using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using PWAssetEditor.Library;

namespace PWAssetEditor
{
    public partial class SaveDisplay : Form
    {
        private readonly IAsset[] assetsToSave;

        public SaveDisplay(IReadOnlyCollection<IAsset> assetsToSave)
        {
            InitializeComponent();
            this.assetsToSave = assetsToSave.ToArray();
            this.progressBar.Maximum = this.assetsToSave.Length;
            this.progressBar.Value = 0;

            this.backgroundWorker.DoWork += (sender, args) =>
            {
                var worker = (BackgroundWorker) sender;
                var assets = (IAsset[]) args.Argument;

                int progress = 0;
                foreach (IAsset asset in assets)
                {
                    if (worker.CancellationPending)
                        return;

                    string jsonPath = asset.JsonFilePath;
                    string jsonDirectory = Path.GetDirectoryName(jsonPath);

                    if (string.IsNullOrEmpty(jsonPath))
                        throw new Exception($"Asset '{asset}' has no path to be written to. This is a bug.");
                    if (string.IsNullOrEmpty(jsonDirectory))
                        throw new Exception($"Asset '{asset}' doesn't have a directory? This is a bug.");
                    if (!Directory.Exists(jsonDirectory))
                        Directory.CreateDirectory(jsonDirectory);

                    string jsonData = asset.Serialize().ToString(Formatting.Indented);

                    try
                    {
                        // tries to write the file to disk
                        File.WriteAllText(jsonPath, jsonData);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                    }
                    finally
                    {
                        // report regardless of an error or not
                        worker.ReportProgress(++progress);
                    }
                }
            };
        }
        private void SaveDisplay_Load(object sender, EventArgs e)
        {
            this.backgroundWorker.RunWorkerAsync(this.assetsToSave);
        }
        private void SetTitleProgress(int progress)
        {
            int total = this.assetsToSave.Length;
            this.Text = $"Saving assets... ({progress}/{total})";
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
                this.DialogResult = DialogResult.Cancel;
            else
                this.DialogResult = DialogResult.OK;

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