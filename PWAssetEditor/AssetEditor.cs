using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PWAssetEditor.Library;
using PWAssetEditor.Library.Asset;

namespace PWAssetEditor
{
    public partial class AssetEditor : Form
    {
        private readonly AssetLibrary library;
        private readonly AssetFolderPicker source;
        private bool allowEditingExternalAssets;

        private string AuthorText
        {
            set => this.authorEditLabel.Text = $"Author: {value ?? "none"}";
        }

        public AssetEditor(AssetLibrary library, AssetFolderPicker source)
        {
            this.library = library;
            this.source = source;
            InitializeComponent();

            this.openInExplorerButton.Image = SystemIcons.WinLogo.ToBitmap();
        }
        /// <summary>
        /// Resets all components that connect the UI And the asset library.
        /// </summary>
        private void ResetAssetComponents()
        {
            this.assetListProps.Items.Clear();
            this.assetListMaterials.Items.Clear();
            this.assetListMaps.Items.Clear();
            this.AuthorText = null;

            if (this.focusControls != null)
            {
                this.editPanel.Controls.Clear();
                foreach (Control control in this.focusControls)
                    control.Dispose();
                this.focusControls = null;
            }
        }
        /// <summary>
        /// Applies the current asset library's data to this UI's components. 
        /// </summary>
        private void ApplyAssetComponents()
        {
            ApplyAssetListComponents(AssetType.prop);
            ApplyAssetListComponents(AssetType.material);
            ApplyAssetListComponents(AssetType.map);
            this.AuthorText = this.library.authorName;
        }
        /// <summary>
        /// Applies the current asset library's data to ONLY the given UI's lists, not anywhere else.
        /// </summary>
        /// <param name="type"></param>
        internal void ApplyAssetListComponents(AssetType type)
        {
            switch (type)
            {
                case AssetType.prop:
                    this.assetListProps.Items.Clear();
                    foreach (Prop prop in this.library.Props)
                    {
                        if (!this.allowEditingExternalAssets && !prop.Identifier.GetValueOrDefault().authorName
                                .Equals(this.library.authorName))
                            continue; // this is another author's prop, we don't want to edit it
                        this.assetListProps.Items.Add(prop);
                    }

                    break;
                case AssetType.material:
                    this.assetListMaterials.Items.Clear();
                    foreach (Material material in this.library.Materials)
                    {
                        if (!this.allowEditingExternalAssets && !material.Identifier.GetValueOrDefault().authorName
                                .Equals(this.library.authorName))
                            continue; // this is another author's material, we don't want to edit it
                        this.assetListMaterials.Items.Add(material);
                    }

                    break;
                case AssetType.map:
                    this.assetListMaps.Items.Clear();
                    foreach (Map map in this.library.Maps)
                    {
                        if (!this.allowEditingExternalAssets && !map.Identifier.GetValueOrDefault().authorName
                                .Equals(this.library.authorName))
                            continue; // this is another author's map, we don't want to edit it
                        this.assetListMaps.Items.Add(map);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void AssetEditor_Load(object sender, EventArgs e)
        {
            // apply the loaded assets into the UI
            ApplyAssetComponents();

            // if no author is present in the library, prompt the user to select one.
            PromptForAuthor();
        }
        private void PromptForAuthor()
        {
            do
            {
                var chooser = new AuthorChooser(false, this.library);
                DialogResult chooseResult = chooser.ShowDialog();
                if (chooseResult == DialogResult.OK && chooser.HasResultText)
                {
                    string selectedName = chooser.ResultText;
                    this.library.authorName = selectedName;
                    this.AuthorText = selectedName;
                    ResetAssetComponents();
                    ApplyAssetComponents();
                }
                else if (string.IsNullOrEmpty(this.library.authorName))
                {
                    MessageBox.Show("A name must be chosen to author assets as.",
                        "Author as...",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            } while (string.IsNullOrEmpty(this.library.authorName));
        }
        private void PromptForAuthorRefactor()
        {
            var chooser = new AuthorChooser(true, this.library);
            DialogResult chooseResult = chooser.ShowDialog();

            if (chooseResult != DialogResult.OK)
                return;
            if (!chooser.HasResultText)
                return;

            string oldName = this.library.authorName;
            string newName = chooser.ResultText;

            if (oldName.Equals(newName, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show($"Source and destination name are the same. Skipping refactor.", "Author Refactor",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            this.library.RefactorAuthorName(oldName, newName);
            this.library.authorName = newName;
            this.AuthorText = newName;
            MessageBox.Show($"Refactor successful.\n\"{oldName}\" --> \"{newName}\"", "Author Refactor");
        }
        private void AssetEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.source.Show();
        }
        private void changeAuthorButton_Click(object sender, EventArgs e)
        {
            PromptForAuthor();
        }
        private void refactorAuthorButton_Click(object sender, EventArgs e)
        {
            PromptForAuthorRefactor();
        }

        /// <summary>
        /// Called when one of the "New X" buttons is pressed.
        /// </summary>
        /// <param name="type"></param>
        private void NewAssetClick(AssetType type)
        {
            var assetCreator = new SingleAssetCreator(type, this.library);
            assetCreator.ShowDialog();

            // reload UI
            ResetAssetComponents();
            ApplyAssetComponents();
            assetCreator.Dispose();
        }
        private void newPropButton_Click(object sender, EventArgs e)
        {
            NewAssetClick(AssetType.prop);
        }
        private void newMaterialButton_Click(object sender, EventArgs e)
        {
            NewAssetClick(AssetType.material);
        }
        private void newMapButton_Click(object sender, EventArgs e)
        {
            NewAssetClick(AssetType.map);
        }
        private void refreshUIButton_Click(object sender, EventArgs e)
        {
            StopFocusAsset();
            ResetAssetComponents();
            ApplyAssetComponents();
            Focus();
        }

        private IAsset focusedAsset;
        private Control[] focusControls;
        private bool suppressChanges;
        private void SetFocusControls(Control[] controls)
        {
            this.focusControls = controls;

            // attach an event handler to all of these controls which redraws the appropriate combobox when changed.
            foreach (Control control in this.focusControls)
                switch (control)
                {
                    case Button button:
                        button.Click += (s, e) => UpdatePanelForAssetType(this.focusedAsset.Type);
                        break;
                    case CheckBox checkBox:
                        checkBox.CheckedChanged += (s, e) => UpdatePanelForAssetType(this.focusedAsset.Type);
                        break;
                    default:
                        control.TextChanged += (s, e) => UpdatePanelForAssetType(this.focusedAsset.Type);
                        break;
                }

            return;

            void UpdatePanelForAssetType(AssetType type)
            {
                int previousSelectedIndex;
                this.suppressChanges = true;
                switch (type)
                {
                    case AssetType.prop:
                        previousSelectedIndex = this.assetListProps.SelectedIndex;
                        this.assetListProps.Items.Clear();
                        ApplyAssetListComponents(AssetType.prop);
                        this.assetListProps.SelectedIndex = previousSelectedIndex;
                        break;
                    case AssetType.material:
                        previousSelectedIndex = this.assetListMaterials.SelectedIndex;
                        this.assetListMaterials.Items.Clear();
                        ApplyAssetListComponents(AssetType.material);
                        this.assetListMaterials.SelectedIndex = previousSelectedIndex;
                        break;
                    case AssetType.map:
                        previousSelectedIndex = this.assetListMaps.SelectedIndex;
                        this.assetListMaps.Items.Clear();
                        ApplyAssetListComponents(AssetType.map);
                        this.assetListMaps.SelectedIndex = previousSelectedIndex;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                this.suppressChanges = false;
            }
        }
        /// <summary>
        /// Stops focusing on/editing an asset, if any.
        /// </summary>
        /// <param name="replacing">If specified, the asset that will replace the focused asset. Used for managing selection.</param>
        private void StopFocusAsset(IAsset replacing = null)
        {
            if (this.focusedAsset != null)
            {
                if (replacing == null || replacing.Type != this.focusedAsset.Type)
                    switch (this.focusedAsset)
                    {
                        case Map _:
                            this.assetListMaps.ClearSelected();
                            break;
                        case Material _:
                            this.assetListMaterials.ClearSelected();
                            break;
                        case Prop _:
                            this.assetListProps.ClearSelected();
                            break;
                    }

                this.focusedAsset = null;
            }

            if (this.focusControls != null)
            {
                this.editPanel.Controls.Clear();
                foreach (Control control in this.focusControls)
                    control.Dispose();
                this.focusControls = null;
            }
        }
        /// <summary>
        /// Sets the editor focus on an asset, constructing and placing a menu to edit it.
        /// </summary>
        /// <param name="asset"></param>
        private void FocusAsset(IAsset asset)
        {
            // check if the asset is not the current author's asset. if not, show a confirmation message
            if (!this.allowEditingExternalAssets)
            {
                string otherAuthor = asset.Identifier.GetValueOrDefault().authorName;
                if (!this.library.authorName.Equals(otherAuthor, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        $"This asset was created by another author ({otherAuthor}). Enable 'Allow Changing External Assets' under 'Author' to do this.",
                        "Cannot Change Asset",
                        MessageBoxButtons.OK);
                    return;
                }
            }

            this.library.AssetChanged(asset);
            this.focusedAsset = asset;

            if (this.focusControls != null)
            {
                this.editPanel.Controls.Clear();
                foreach (Control control in this.focusControls)
                    control.Dispose();

                this.focusControls = null;
            }

            IEnumerable<Tuple<string, Control>> foreignControls = asset.CreateEditControls(this.library, true, this);
            List<Tuple<string, Control>> foreignControlsList =
                foreignControls as List<Tuple<string, Control>> ?? foreignControls.ToList();
            AssetUtils.LayoutVertically(this.editPanel, foreignControlsList);
            SetFocusControls(foreignControlsList.Select(t => t.Item2).ToArray());
        }
        private void assetListProps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.suppressChanges)
                return;

            var selectedAsset = (IAsset) this.assetListProps.SelectedItem;
            StopFocusAsset(selectedAsset);
            if (selectedAsset != null)
                FocusAsset(selectedAsset);
        }
        private void assetListMaterials_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.suppressChanges)
                return;

            var selectedAsset = (IAsset) this.assetListMaterials.SelectedItem;
            StopFocusAsset(selectedAsset);
            if (selectedAsset != null)
                FocusAsset(selectedAsset);
        }
        private void assetListMaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.suppressChanges)
                return;

            var selectedAsset = (IAsset) this.assetListMaps.SelectedItem;
            StopFocusAsset(selectedAsset);
            if (selectedAsset != null)
                FocusAsset(selectedAsset);
        }

        private bool hasShownExternalAssetModificationWarning;
        private void allowChangingExternalAssetsButton_Click(object sender, EventArgs e)
        {
            if (!this.allowEditingExternalAssets && !this.hasShownExternalAssetModificationWarning)
            {
                MessageBox.Show(
                    "Enabling changing of external assets is not recommended and will break things if you intend on publishing assets to the Steam Workshop. Enable anyway?",
                    "Warning",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Exclamation);
                this.hasShownExternalAssetModificationWarning = true;
            }

            this.allowEditingExternalAssets = !this.allowEditingExternalAssets;
            this.allowChangingExternalAssetsButton.Checked = this.allowEditingExternalAssets;
            ResetAssetComponents();
            ApplyAssetComponents();
        }

        private void SaveChanges(IAsset[] changes)
        {
            var saveWindow = new SaveDisplay(changes);

            // saveWindow may report OK or CANCEL if the user canceled the operation.
            DialogResult result = saveWindow.ShowDialog();

            switch (result)
            {
                case DialogResult.OK:
                    this.library.ClearChanges();
                    return;
                case DialogResult.Cancel:
                    return;
                case DialogResult.None:
                case DialogResult.Abort:
                case DialogResult.Retry:
                case DialogResult.Ignore:
                case DialogResult.Yes:
                case DialogResult.No:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void saveButton_Click(object sender, EventArgs e)
        {
            IAsset[] changes = this.library.Changes;

            if (changes.Length == 0)
                return;

            SaveChanges(changes);
        }
        private void AssetEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            IAsset[] changes = this.library.Changes;

            if (changes.Length > 0)
            {
                DialogResult result = MessageBox.Show("Some asset changes are unsaved. Would you like to save them?",
                    "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                switch (result)
                {
                    case DialogResult.Yes:
                        SaveChanges(changes);
                        e.Cancel = false;
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                    case DialogResult.No:
                        e.Cancel = false;
                        break;
                    case DialogResult.None:
                    case DialogResult.OK:
                    case DialogResult.Abort:
                    case DialogResult.Retry:
                    case DialogResult.Ignore:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        private void openInExplorerButton_Click(object sender, EventArgs e)
        {
            string folder = this.library.GetAssetsDirectory();
            System.Diagnostics.Process.Start(folder);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete && this.focusedAsset != null)
            {
                DeleteKeyPressed();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void DeleteKeyPressed()
        {
            if (this.focusedAsset == null)
                return;

            if (!this.allowEditingExternalAssets)
            {
                // check if this needs to be enabled
                string otherAuthor = this.focusedAsset.Identifier.GetValueOrDefault().authorName;
                if (!this.library.authorName.Equals(otherAuthor, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        $"This asset was created by another author ({otherAuthor}). Enable 'Allow Changing External Assets' under 'Author' to do this.",
                        "Cannot Delete Asset",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // don't let the user delete this asset if it's in use by any other assets
            Identifier[] dependsOnThisAsset = this.library.GetAssetsThatDependOn(this.focusedAsset)
                .Select(a => a.Identifier.GetValueOrDefault())
                .ToArray();
            if (dependsOnThisAsset.Length != 0)
            {
                MessageBox.Show(
                    $"This asset is being used by {dependsOnThisAsset.Length} other assets. Please remove these dependencies before deleting this asset.\n\n{string.Join("\n", dependsOnThisAsset)}",
                    "Cannot Delete Asset", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult confirmation = MessageBox.Show(
                "Are you sure you want to delete this asset? This action cannot be undone.",
                "Delete Asset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmation == DialogResult.No)
                return;

            // get all associated assets and delete the files which are no longer in use
            var associatedFiles = new List<string>(this.focusedAsset.GetAllReferencedFiles);

            foreach (IAsset asset in this.library.Assets)
            {
                if (!asset.Identifier.HasValue)
                    continue;
                if (asset.Identifier.Value.Equals(this.focusedAsset.Identifier.GetValueOrDefault()))
                    continue; // skip the same asset

                IEnumerable<string> associatedAssetsOther = asset.GetAllReferencedFiles;
                foreach (string assetInUse in associatedAssetsOther)
                    associatedFiles.Remove(assetInUse); // file is in use by another asset, keep it
            }

            string mainAssetPath = this.focusedAsset.JsonFilePath ?? string.Empty;

            int count = associatedFiles.Count;
            if (count > 1)
            {
                string focusAssetPath = this.focusedAsset.JsonFilePath ?? string.Empty;

                // don't include the main JSON file in the count of "external" files
                int minus = associatedFiles.Count(file => file.Equals(mainAssetPath));

                string listOfFilesToDelete = string.Join("\n",
                    associatedFiles
                        .Where(file => !file.Equals(mainAssetPath))
                        .Select(file => "- .\\" + AssetUtils.TruncatePath(focusAssetPath, file))
                );

                DialogResult result = MessageBox.Show(
                    $"{count - minus} file(s) will become unused. Delete them as well?:\n\n" + listOfFilesToDelete,
                    "Delete Asset", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                switch (result)
                {
                    case DialogResult.Cancel:
                        return;
                    case DialogResult.No:
                        associatedFiles.Clear();
                        associatedFiles.Add(mainAssetPath); // just this file, nothing else
                        break;
                    case DialogResult.Yes:
                        break;
                    case DialogResult.None:
                    case DialogResult.OK:
                    case DialogResult.Abort:
                    case DialogResult.Retry:
                    case DialogResult.Ignore:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            foreach (string unusedFile in associatedFiles)
                try
                {
                    File.Delete(unusedFile);

                    string directory = Path.GetDirectoryName(unusedFile);
                    if (string.IsNullOrEmpty(directory))
                        continue;
                    if (!Directory.EnumerateFileSystemEntries(directory).Any())
                        Directory.Delete(directory); // remove the directory; it's empty
                }
                catch (IOException e)
                {
                    MessageBox.Show($"Error: Couldn't delete file:\n\t{unusedFile}\n\n{e.Message}",
                        "Couldn't Delete File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            this.library.RemoveAsset(this.focusedAsset);
            StopFocusAsset();
            ResetAssetComponents();
            ApplyAssetComponents();
        }
        private void cleanupButton_Click(object sender, EventArgs e)
        {
            this.library.Cleanup();
        }
    }
}