using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PWAssetEditor.Library;

namespace PWAssetEditor
{
    public partial class AuthorChooser : Form
    {
        /// <summary>
        /// Gets a value indicating whether the AuthorChooser has a valid result text.
        /// </summary>
        public bool HasResultText => !string.IsNullOrWhiteSpace(this.ResultText);
        
        /// <summary>
        /// Gets the result text from the AuthorChooser.
        /// </summary>
        /// <remarks>
        /// The ResultText property returns the text of the authorPicker control in the AuthorChooser form.
        /// </remarks> 
        public string ResultText => this.authorPicker.Text.Trim().ToLower();

        /// <summary>
        /// Represents a form for choosing the author of assets.
        /// </summary>
        public AuthorChooser(bool isRefactor, AssetLibrary library)
        {
            InitializeComponent();

            this.label1.Text = isRefactor ?
                $"Refactoring author {library.authorName} to:" :
                "Choose who to author assets as.";

            if (isRefactor)
            {
                if(!string.IsNullOrWhiteSpace(library.authorName))
                    this.authorPicker.Text = library.authorName;
            }
            else
            {
                IEnumerable<string> authors = library.ExistingAuthorNames;
                foreach (string author in authors)
                    this.authorPicker.Items.Add(author);
            }

            this.okButton.Enabled = false;
        }
        private void authorPicker_TextUpdate(object sender, EventArgs e)
        {
            string newText = this.authorPicker.Text;

            if (newText.Any(char.IsUpper))
            {
                int cursorIndex = this.authorPicker.SelectionStart;
                this.authorPicker.Text = newText.ToLower();
                this.authorPicker.SelectionStart = cursorIndex;
                return;
            }

            bool disable = string.IsNullOrWhiteSpace(newText);
            this.okButton.Enabled = !disable;
        }
        private void authorPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedAuthor = this.authorPicker.SelectedItem as string;

            bool disable = string.IsNullOrWhiteSpace(selectedAuthor);
            this.okButton.Enabled = !disable;
        }
        
        private void authorPicker_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Return && this.okButton.Enabled)
                this.okButton.PerformClick();
        }
    }
}