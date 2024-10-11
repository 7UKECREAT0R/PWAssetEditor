namespace PWAssetEditor
{
    partial class AssetFolderPicker
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssetFolderPicker));
            this.label1 = new System.Windows.Forms.Label();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.currentFolderDisplay = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.openButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.asyncLoader = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(458, 50);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select your Prego Wars \'pw-assets\' directory ";
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.Description = "Choose the \'assets\' directory you would like to edit in.";
            // 
            // currentFolderDisplay
            // 
            this.currentFolderDisplay.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.currentFolderDisplay.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.currentFolderDisplay.ForeColor = System.Drawing.SystemColors.GrayText;
            this.currentFolderDisplay.Location = new System.Drawing.Point(59, 63);
            this.currentFolderDisplay.Name = "currentFolderDisplay";
            this.currentFolderDisplay.ReadOnly = true;
            this.currentFolderDisplay.Size = new System.Drawing.Size(411, 26);
            this.currentFolderDisplay.TabIndex = 1;
            this.currentFolderDisplay.KeyDown += new System.Windows.Forms.KeyEventHandler(this.currentFolderDisplay_KeyDown);
            // 
            // browseButton
            // 
            this.browseButton.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.browseButton.FlatAppearance.BorderSize = 0;
            this.browseButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.browseButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlDark;
            this.browseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.browseButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.browseButton.Location = new System.Drawing.Point(12, 63);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(41, 26);
            this.browseButton.TabIndex = 2;
            this.browseButton.Text = "...";
            this.browseButton.UseVisualStyleBackColor = false;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // openButton
            // 
            this.openButton.Enabled = false;
            this.openButton.ForeColor = System.Drawing.Color.Black;
            this.openButton.Location = new System.Drawing.Point(12, 95);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(458, 23);
            this.openButton.TabIndex = 3;
            this.openButton.Text = "open";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // progressBar
            // 
            this.progressBar.Enabled = false;
            this.progressBar.Location = new System.Drawing.Point(12, 124);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(458, 23);
            this.progressBar.Step = 1;
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 4;
            // 
            // asyncLoader
            // 
            this.asyncLoader.WorkerReportsProgress = true;
            this.asyncLoader.DoWork += new System.ComponentModel.DoWorkEventHandler(this.asyncLoader_DoWork);
            this.asyncLoader.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.asyncLoader_ProgressChanged);
            this.asyncLoader.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.asyncLoader_RunWorkerCompleted);
            // 
            // AssetFolderPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(482, 159);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.openButton);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.currentFolderDisplay);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Name = "AssetFolderPicker";
            this.Text = "Prego Wars Asset Editor";
            this.Load += new System.EventHandler(this.AssetFolderPicker_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        private System.Windows.Forms.ProgressBar progressBar;
        private System.ComponentModel.BackgroundWorker asyncLoader;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox currentFolderDisplay;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Label label1;

        #endregion
    }
}