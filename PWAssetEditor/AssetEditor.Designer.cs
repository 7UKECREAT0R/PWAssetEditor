using System.ComponentModel;

namespace PWAssetEditor
{
    partial class AssetEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssetEditor));
            this.assetPanel = new System.Windows.Forms.TableLayoutPanel();
            this.assetListMaps = new System.Windows.Forms.ListBox();
            this.assetListProps = new System.Windows.Forms.ListBox();
            this.assetListMaterials = new System.Windows.Forms.ListBox();
            this.miniToolStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveButton = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshUIButton = new System.Windows.Forms.ToolStripMenuItem();
            this.cleanupButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openInExplorerButton = new System.Windows.Forms.ToolStripMenuItem();
            this.assetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newPropButton = new System.Windows.Forms.ToolStripMenuItem();
            this.newMaterialButton = new System.Windows.Forms.ToolStripMenuItem();
            this.newMapButton = new System.Windows.Forms.ToolStripMenuItem();
            this.authorEditLabel = new System.Windows.Forms.ToolStripMenuItem();
            this.changeAuthorButton = new System.Windows.Forms.ToolStripMenuItem();
            this.refactorAuthorButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.allowChangingExternalAssetsButton = new System.Windows.Forms.ToolStripMenuItem();
            this.editPanel = new System.Windows.Forms.Panel();
            this.assetPanel.SuspendLayout();
            this.miniToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // assetPanel
            // 
            this.assetPanel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
            this.assetPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.assetPanel.ColumnCount = 1;
            this.assetPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.assetPanel.Controls.Add(this.assetListMaps, 0, 2);
            this.assetPanel.Controls.Add(this.assetListProps, 0, 0);
            this.assetPanel.Controls.Add(this.assetListMaterials, 0, 1);
            this.assetPanel.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.assetPanel.Location = new System.Drawing.Point(12, 27);
            this.assetPanel.Name = "assetPanel";
            this.assetPanel.RowCount = 3;
            this.assetPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.assetPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.assetPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.assetPanel.Size = new System.Drawing.Size(315, 763);
            this.assetPanel.TabIndex = 0;
            // 
            // assetListMaps
            // 
            this.assetListMaps.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.assetListMaps.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.assetListMaps.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.assetListMaps.FormattingEnabled = true;
            this.assetListMaps.IntegralHeight = false;
            this.assetListMaps.ItemHeight = 20;
            this.assetListMaps.Location = new System.Drawing.Point(3, 511);
            this.assetListMaps.Name = "assetListMaps";
            this.assetListMaps.Size = new System.Drawing.Size(309, 249);
            this.assetListMaps.TabIndex = 3;
            this.assetListMaps.SelectedIndexChanged += new System.EventHandler(this.assetListMaps_SelectedIndexChanged);
            // 
            // assetListProps
            // 
            this.assetListProps.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.assetListProps.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.assetListProps.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.assetListProps.FormattingEnabled = true;
            this.assetListProps.IntegralHeight = false;
            this.assetListProps.Location = new System.Drawing.Point(3, 3);
            this.assetListProps.Name = "assetListProps";
            this.assetListProps.Size = new System.Drawing.Size(309, 248);
            this.assetListProps.TabIndex = 1;
            this.assetListProps.SelectedIndexChanged += new System.EventHandler(this.assetListProps_SelectedIndexChanged);
            // 
            // assetListMaterials
            // 
            this.assetListMaterials.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.assetListMaterials.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.assetListMaterials.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.assetListMaterials.FormattingEnabled = true;
            this.assetListMaterials.IntegralHeight = false;
            this.assetListMaterials.ItemHeight = 16;
            this.assetListMaterials.Location = new System.Drawing.Point(3, 257);
            this.assetListMaterials.Name = "assetListMaterials";
            this.assetListMaterials.Size = new System.Drawing.Size(309, 248);
            this.assetListMaterials.TabIndex = 2;
            this.assetListMaterials.SelectedIndexChanged += new System.EventHandler(this.assetListMaterials_SelectedIndexChanged);
            // 
            // miniToolStrip
            // 
            this.miniToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {this.fileToolStripMenuItem, this.assetsToolStripMenuItem, this.authorEditLabel});
            this.miniToolStrip.Location = new System.Drawing.Point(0, 0);
            this.miniToolStrip.Name = "miniToolStrip";
            this.miniToolStrip.Size = new System.Drawing.Size(1073, 24);
            this.miniToolStrip.TabIndex = 1;
            this.miniToolStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {this.saveButton, this.refreshUIButton, this.cleanupButton, this.toolStripSeparator1, this.openInExplorerButton});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveButton
            // 
            this.saveButton.Name = "saveButton";
            this.saveButton.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveButton.Size = new System.Drawing.Size(162, 22);
            this.saveButton.Text = "Save...";
            this.saveButton.ToolTipText = "Saves all unsaved assets to disk.";
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // refreshUIButton
            // 
            this.refreshUIButton.Name = "refreshUIButton";
            this.refreshUIButton.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.refreshUIButton.Size = new System.Drawing.Size(162, 22);
            this.refreshUIButton.Text = "Refresh UI";
            this.refreshUIButton.ToolTipText = "Refreshes the UI incase something\'s not quite right.";
            this.refreshUIButton.Click += new System.EventHandler(this.refreshUIButton_Click);
            // 
            // cleanupButton
            // 
            this.cleanupButton.Name = "cleanupButton";
            this.cleanupButton.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.cleanupButton.Size = new System.Drawing.Size(162, 22);
            this.cleanupButton.Text = "Cleanup";
            this.cleanupButton.ToolTipText = "Clean up unused files and folders in the assets directory.";
            this.cleanupButton.Click += new System.EventHandler(this.cleanupButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(159, 6);
            // 
            // openInExplorerButton
            // 
            this.openInExplorerButton.Name = "openInExplorerButton";
            this.openInExplorerButton.Size = new System.Drawing.Size(162, 22);
            this.openInExplorerButton.Text = "Open in Explorer";
            this.openInExplorerButton.ToolTipText = "Open the \"assets\" folder in windows explorer.";
            this.openInExplorerButton.Click += new System.EventHandler(this.openInExplorerButton_Click);
            // 
            // assetsToolStripMenuItem
            // 
            this.assetsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {this.newPropButton, this.newMaterialButton, this.newMapButton});
            this.assetsToolStripMenuItem.Name = "assetsToolStripMenuItem";
            this.assetsToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.assetsToolStripMenuItem.Text = "Assets";
            // 
            // newPropButton
            // 
            this.newPropButton.Name = "newPropButton";
            this.newPropButton.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.newPropButton.Size = new System.Drawing.Size(202, 22);
            this.newPropButton.Text = "New Prop";
            this.newPropButton.Click += new System.EventHandler(this.newPropButton_Click);
            // 
            // newMaterialButton
            // 
            this.newMaterialButton.Name = "newMaterialButton";
            this.newMaterialButton.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.newMaterialButton.Size = new System.Drawing.Size(202, 22);
            this.newMaterialButton.Text = "New Material";
            this.newMaterialButton.Click += new System.EventHandler(this.newMaterialButton_Click);
            // 
            // newMapButton
            // 
            this.newMapButton.Name = "newMapButton";
            this.newMapButton.ShortcutKeys = ((System.Windows.Forms.Keys) (((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) | System.Windows.Forms.Keys.M)));
            this.newMapButton.Size = new System.Drawing.Size(202, 22);
            this.newMapButton.Text = "New Map";
            this.newMapButton.Click += new System.EventHandler(this.newMapButton_Click);
            // 
            // authorEditLabel
            // 
            this.authorEditLabel.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {this.changeAuthorButton, this.refactorAuthorButton, this.toolStripSeparator2, this.allowChangingExternalAssetsButton});
            this.authorEditLabel.Name = "authorEditLabel";
            this.authorEditLabel.Size = new System.Drawing.Size(92, 20);
            this.authorEditLabel.Text = "Author: name";
            // 
            // changeAuthorButton
            // 
            this.changeAuthorButton.Name = "changeAuthorButton";
            this.changeAuthorButton.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.changeAuthorButton.Size = new System.Drawing.Size(240, 22);
            this.changeAuthorButton.Text = "Change";
            this.changeAuthorButton.ToolTipText = "Change the author you\'re working as.";
            this.changeAuthorButton.Click += new System.EventHandler(this.changeAuthorButton_Click);
            // 
            // refactorAuthorButton
            // 
            this.refactorAuthorButton.Name = "refactorAuthorButton";
            this.refactorAuthorButton.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F2)));
            this.refactorAuthorButton.Size = new System.Drawing.Size(240, 22);
            this.refactorAuthorButton.Text = "Rename";
            this.refactorAuthorButton.ToolTipText = "Rename the active author and refactor any assets under the old name.";
            this.refactorAuthorButton.Click += new System.EventHandler(this.refactorAuthorButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(237, 6);
            // 
            // allowChangingExternalAssetsButton
            // 
            this.allowChangingExternalAssetsButton.Name = "allowChangingExternalAssetsButton";
            this.allowChangingExternalAssetsButton.Size = new System.Drawing.Size(240, 22);
            this.allowChangingExternalAssetsButton.Text = "Allow Changing External Assets";
            this.allowChangingExternalAssetsButton.ToolTipText = "Whether to allow changes to assets not authored by the active author. Generally, " + "this is not recommended as it may break your assets for other users.";
            this.allowChangingExternalAssetsButton.Click += new System.EventHandler(this.allowChangingExternalAssetsButton_Click);
            // 
            // editPanel
            // 
            this.editPanel.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.editPanel.BackColor = System.Drawing.SystemColors.Control;
            this.editPanel.Location = new System.Drawing.Point(333, 30);
            this.editPanel.Name = "editPanel";
            this.editPanel.Size = new System.Drawing.Size(728, 760);
            this.editPanel.TabIndex = 2;
            // 
            // AssetEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1073, 802);
            this.Controls.Add(this.editPanel);
            this.Controls.Add(this.assetPanel);
            this.Controls.Add(this.miniToolStrip);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.miniToolStrip;
            this.Name = "AssetEditor";
            this.Text = "Asset Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AssetEditor_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AssetEditor_FormClosed);
            this.Load += new System.EventHandler(this.AssetEditor_Load);
            this.assetPanel.ResumeLayout(false);
            this.miniToolStrip.ResumeLayout(false);
            this.miniToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem cleanupButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem openInExplorerButton;
        private System.Windows.Forms.ToolStripMenuItem allowChangingExternalAssetsButton;
        private System.Windows.Forms.Panel editPanel;
        private System.Windows.Forms.ToolStripMenuItem refreshUIButton;
        private System.Windows.Forms.ToolStripMenuItem authorEditLabel;
        private System.Windows.Forms.ToolStripMenuItem changeAuthorButton;
        private System.Windows.Forms.ToolStripMenuItem refactorAuthorButton;
        private System.Windows.Forms.ToolStripMenuItem newPropButton;
        private System.Windows.Forms.ToolStripMenuItem newMaterialButton;
        private System.Windows.Forms.ToolStripMenuItem newMapButton;
        private System.Windows.Forms.ToolStripMenuItem saveButton;
        private System.Windows.Forms.MenuStrip miniToolStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assetsToolStripMenuItem;
        private System.Windows.Forms.ListBox assetListProps;
        private System.Windows.Forms.ListBox assetListMaterials;
        private System.Windows.Forms.ListBox assetListMaps;
        private System.Windows.Forms.TableLayoutPanel assetPanel;

        #endregion
    }
}