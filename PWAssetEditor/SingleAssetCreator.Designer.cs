using System.ComponentModel;

namespace PWAssetEditor
{
    partial class SingleAssetCreator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SingleAssetCreator));
            this.SuspendLayout();
            // 
            // SingleAssetCreator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Name = "SingleAssetCreator";
            this.Text = "Single Asset Creator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SingleAssetCreator_FormClosing);
            this.Load += new System.EventHandler(this.SingleAssetCreator_Load);
            this.ResumeLayout(false);
        }

        #endregion
    }
}