using System.ComponentModel;

namespace PWAssetEditor
{
    partial class AuthorChooser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AuthorChooser));
            this.label1 = new System.Windows.Forms.Label();
            this.authorPicker = new System.Windows.Forms.ComboBox();
            this.okButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(302, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Choose who to author assets as.";
            // 
            // authorPicker
            // 
            this.authorPicker.FormattingEnabled = true;
            this.authorPicker.Location = new System.Drawing.Point(12, 36);
            this.authorPicker.Name = "authorPicker";
            this.authorPicker.Size = new System.Drawing.Size(302, 21);
            this.authorPicker.TabIndex = 1;
            this.authorPicker.SelectedIndexChanged += new System.EventHandler(this.authorPicker_SelectedIndexChanged);
            this.authorPicker.TextUpdate += new System.EventHandler(this.authorPicker_TextUpdate);
            this.authorPicker.KeyDown += new System.Windows.Forms.KeyEventHandler(this.authorPicker_KeyDown);
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(106, 82);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(208, 23);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(12, 82);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // AuthorChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 117);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.authorPicker);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AuthorChooser";
            this.Text = "Authoring As...";
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox authorPicker;

        #endregion
    }
}