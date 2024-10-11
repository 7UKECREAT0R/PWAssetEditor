using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PWAssetEditor.Library;
using PWAssetEditor.Library.Asset;

namespace PWAssetEditor
{
    public partial class SingleAssetCreator : Form
    {
        /// <summary>
        /// When saving the asset, if the identifier is different, that means we likely
        /// need to perform a refactor of all assets referencing that identifier.
        /// </summary>
        private readonly Identifier? originalIdentifier;

        private readonly AssetLibrary containingLibrary;
        private readonly IAsset editingAsset;
        public SingleAssetCreator(AssetType type, AssetLibrary containingLibrary)
        {
            this.containingLibrary = containingLibrary;
            this.originalIdentifier = null;

            switch (type)
            {
                case AssetType.material:
                    this.editingAsset = new Material();
                    break;
                case AssetType.prop:
                    this.editingAsset = new Prop();
                    break;
                case AssetType.map:
                    this.editingAsset = new Map();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            this.editingAsset.ContainingLibrary = containingLibrary;
            InitializeComponent();
        }
        public SingleAssetCreator(IAsset asset, AssetLibrary containingLibrary)
        {
            this.containingLibrary = containingLibrary;
            this.originalIdentifier = asset.Identifier;
            this.editingAsset = asset;
            InitializeComponent();
        }
        private void SingleAssetCreator_Load(object sender, EventArgs e)
        {
            IEnumerable<Tuple<string, Control>> foreignControls =
                this.editingAsset.CreateEditControls(this.containingLibrary, false, null);
            Size newSize = AssetUtils.LayoutVertically(this, foreignControls);
            newSize.Height += 32; // room for buttons at the bottom
            this.ClientSize = newSize;

            // create the "Finish" and "Cancel" buttons
            var finishButton = new Button
            {
                Text = "Finish",
                DialogResult = DialogResult.OK
            };
            this.Controls.Add(finishButton);
            finishButton.PerformLayout();
            finishButton.Location = new Point(
                newSize.Width - finishButton.Width - 4,
                newSize.Height - finishButton.Height - 4
            );
            finishButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(cancelButton);
            cancelButton.PerformLayout();
            cancelButton.Location = new Point(
                4,
                newSize.Height - cancelButton.Height - 4
            );
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        }
        private void SingleAssetCreator_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK)
                return;

            // check if the asset is ready to be committed to the library
            var errors = new List<string>();
            Identifier? identifier = this.editingAsset.Identifier;

            if (this.editingAsset.Validate(this.containingLibrary, errors))
            {
                // valid asset. needs to refactor?
                if (this.originalIdentifier.HasValue && this.editingAsset.Identifier.HasValue)
                {
                    Identifier oldIdentifier = this.originalIdentifier.Value;
                    Identifier newIdentifier = this.editingAsset.Identifier.Value;
                    if (!oldIdentifier.Equals(newIdentifier))
                    {
                        DialogResult shouldRefactor = MessageBox.Show(
                            $"The identifier of this asset has changed from:\n\t{oldIdentifier}\nto:\n\t{newIdentifier}\n\nWould you like to change all occurrences of the old identifier to the new one?",
                            "Identifier Refactor",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Asterisk
                        );
                        if (shouldRefactor == DialogResult.Yes)
                        {
                            this.containingLibrary.RefactorIdentifier(oldIdentifier, newIdentifier);
                            MessageBox.Show("Refactor successful.", "Identifier Refactor");
                        }
                    }
                }

                // if the asset is not yet added to the library, add it.
                if (identifier.HasValue && !this.containingLibrary.HasIdentifier(identifier.Value))
                    this.containingLibrary.AddAsset(this.editingAsset);

                return; // valid asset, close.
            }

            string list = string.Join("\n", errors);
            DialogResult result = MessageBox.Show(
                $"Asset has error(s): \n\n{list}\n\nClose anyway and remove asset from library?",
                $"Closing {this.editingAsset.Type} editor",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            // remove asset from the library
            if (identifier.HasValue && identifier.Value.IsValid)
                this.containingLibrary.RemoveByIdentifier(identifier.Value);
        }
    }
}