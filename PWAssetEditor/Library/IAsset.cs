using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using PWAssetEditor.Library.Asset;

namespace PWAssetEditor.Library
{
    /// <summary>
    /// A prego wars data driven asset that can be validated.
    /// </summary>
    public interface IAsset
    {
        /// <summary>
        /// The <see cref="AssetLibrary"/> containing this asset.
        /// </summary>
        AssetLibrary ContainingLibrary { get; set; }

        /// <summary>
        /// Validates this asset and ensures the input data is correct. Returns true if all is good.
        /// </summary>
        /// <param name="containingLibrary">The <see cref="AssetLibrary"/> that contains this asset.</param>
        /// <param name="errorMessages"></param>
        /// <returns>false if the asset couldn't be validated. <paramref name="errorMessages"/> will contain a list of the validation errors that occurred.</returns>
        bool Validate(AssetLibrary containingLibrary, List<string> errorMessages);
        /// <summary>
        /// Returns if this asset is filled out and can be added to the asset library without any issues.
        /// </summary>
        bool IsFilledOut { get; }

        /// <summary>
        /// Refactors elements of this asset that references identifier <paramref name="from"/> to identifier <paramref name="to"/>.
        /// Does nothing if this asset doesn't reference <paramref name="from"/>.
        /// </summary>
        /// <param name="from">The source identifier.</param>
        /// <param name="to">The new identifier.</param>
        /// <returns>True if this asset was changed.</returns>
        bool RefactorIdentifier(Identifier from, Identifier to);
        /// <summary>
        /// Refactors this asset's identifier to have a new author name, given it matches <paramref name="from"/>.
        /// </summary>
        /// <param name="from">The old author name to change. If this asset's author name doesn't match this string, then it will remain unchanged.</param>
        /// <param name="to">The new name to change matching identifiers to.</param>
        /// <returns>True if this asset was changed.</returns>
        bool RefactorAuthorName(string from, string to);

        /// <summary>
        /// Create the controls needed to edit all of this asset's properties inside the given panel.
        /// </summary>
        /// <param name="containingLibrary">The asset library to use as a reference when creating controls.</param>
        /// <param name="lockIdentifier"></param>
        /// <param name="containingEditor"></param>
        IEnumerable<Tuple<string, Control>> CreateEditControls(AssetLibrary containingLibrary, bool lockIdentifier,
            AssetEditor containingEditor);

        /// <summary>
        /// Gets/Sets the path of the JSON file that this asset is held in. 
        /// </summary>
        string JsonFilePath { get; set; }
        /// <summary>
        /// Serialize this asset to its JSON representation.
        /// </summary>
        JObject Serialize();

        /// <summary>
        /// Gets the identifier associated with this asset.
        /// </summary>
        Identifier? Identifier { get; set; }

        /// <summary>
        /// Gets the type of asset this is.
        /// </summary>
        AssetType Type { get; }

        /// <summary>
        /// Returns a collection of absolute paths to all files/assets referenced by this asset, including its own .json file.
        /// This method is used during asset deletion to find assets without references anymore, so they can be deleted.
        /// </summary>
        IEnumerable<string> GetAllReferencedFiles { get; }
        /// <summary>
        /// Returns a collection of asset identifiers which this asset depends on to work properly.
        /// </summary>
        IEnumerable<Identifier> GetAllDependedAssets { get; }
    }

    /// <summary>
    /// Utility methods for asset validation.
    /// </summary>
    public static class AssetUtils
    {
        /// <summary>
        /// Creates a dropdown ComboBox with the given options and binds an event that triggers when the selected item changes.
        /// </summary>
        /// <param name="options">Array of options to populate the ComboBox.</param>
        /// <param name="changedEvent">Action to execute when the selected item changes.</param>
        /// <param name="preSelectItem">If specified, the currently selected item.</param>
        /// <returns>A configured ComboBox control.</returns>
        public static ComboBox CreateDropDown(object[] options, Action<object> changedEvent,
            object preSelectItem = null)
        {
            var comboBox = new ComboBox();
            comboBox.Width = 300;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            foreach (object option in options)
                comboBox.Items.Add(option);
            if (preSelectItem != null)
                comboBox.SelectedItem = preSelectItem;

            comboBox.SelectedValueChanged += (sender, args) =>
            {
                if (!(comboBox.SelectedItem is object selectedItem))
                    return;
                changedEvent.Invoke(selectedItem);
            };

            return comboBox;
        }

        /// <summary>
        /// Constructs a ComboBox that allows the user to select between all registered identifiers of the given type.
        /// The selectable items are <see cref="Identifier"/>s and are displayed using their ToString method.
        /// </summary>
        /// <param name="type">The type of asset identifiers to allow.</param>
        /// <param name="changedEvent">The event that will be triggered when the input is changed.</param>
        /// <param name="currentLibrary">The asset library to pull from.</param>
        /// <returns></returns>
        public static ComboBox CreateIdentifierDropDown(AssetType type, Action<Identifier> changedEvent,
            AssetLibrary currentLibrary)
        {
            var box = new ComboBox();
            box.Width = 300;
            box.DropDownStyle = ComboBoxStyle.DropDownList;
            box.SelectedValueChanged += (sender, args) =>
            {
                object newValue = box.SelectedItem;
                if (!(newValue is Identifier identifier))
                    return;
                changedEvent.Invoke(identifier);
            };

            // add items based on the input AssetType
            Identifier[] matchingIdentifiers = currentLibrary
                .GetAssetsOfEnumType(type)
                .Select(asset => asset.Identifier)
                .Where(i => i.HasValue)
                .Select(i => i.Value)
                .ToArray();

            // sort by author name first, then asset type, then asset name.
            Array.Sort(matchingIdentifiers);

            foreach (Identifier matchingIdentifier in matchingIdentifiers)
                box.Items.Add(matchingIdentifier);

            return box;
        }
        /// <summary>
        /// Creates a TextBox control that allows the user to input an identifier for the specified asset type.
        /// The input is validated and calls the specified event argument when changed.
        /// </summary>
        /// <param name="identifier">The identifier to start the input with.</param>
        /// <param name="changedEvent">The event that will be triggered after the input is changed and the validation is successful.</param>
        /// <param name="lockEditing">Whether editing of the input should be locked and only able to be changed by an "Edit/Confirm"
        ///     button which performs a refactor.</param>
        /// <param name="library">The asset library to perform refactorings on (if any)</param>
        /// <param name="editorWindow">The editor window that will contain this identifier field. Used to refresh it when refactor is applied.</param>
        /// <param name="assetType">The type of asset that this identifier will encompass.</param>
        /// <returns>A TextBox control that represents the input field for the identifier.</returns>
        public static Control CreateIdentifierInput(Identifier identifier, Action<Identifier> changedEvent,
            bool lockEditing, AssetLibrary library, AssetEditor editorWindow, AssetType assetType)
        {
            // I'm sure this will not cause any issues later on that I'll be coming back to fix
            string existingIdentifier = identifier.ToString();
            string rootIdentifier = identifier.ToStringRoot();

            var box = new TextBox();
            box.Width = 250;
            box.Multiline = false;
            box.Text = existingIdentifier;
            box.CharacterCasing = CharacterCasing.Lower;
            box.AcceptsReturn = false;
            box.ForeColor = identifier.IsValid ? Color.Black : Color.DarkRed;
            box.TextChanged += (sender, e) =>
            {
                if (!box.Text.StartsWith(rootIdentifier))
                {
                    box.Text = rootIdentifier;
                    box.SelectionStart = rootIdentifier.Length;
                    box.ForeColor = Color.DarkRed;
                    return;
                }

                if (box.Text.Length <= rootIdentifier.Length)
                {
                    box.ForeColor = Color.DarkRed;
                    return;
                }

                // try to parse the identifier
                if (Identifier.TryParse(box.Text, out Identifier parsed))
                {
                    box.ForeColor = Color.Black;
                    if (!lockEditing) // if editing is locked and confirmed manually, don't change it automatically
                        changedEvent(parsed);
                }
                else
                {
                    // error parsing
                    box.ForeColor = Color.DarkRed;
                }
            };

            if (lockEditing)
            {
                const int PADDING = 4;
                var changeButton = new Button()
                {
                    Text = "Refactor",
                    Width = 120,
                    Height = box.Height
                };
                box.Enabled = false;

                var container = new Panel();
                container.Height = box.Height;
                container.Width = box.Width + PADDING + changeButton.Width;

                container.Controls.Add(box);
                box.Parent = container;
                box.Left = 0;
                box.Top = 0;

                container.Controls.Add(changeButton);
                changeButton.Parent = container;
                changeButton.Left = box.Right + PADDING;
                changeButton.Top = 0;
                changeButton.Click += (sender, e) =>
                {
                    var self = (Button) sender;
                    box.Enabled = !box.Enabled;

                    // different action depending on the stage of the click
                    if (box.Enabled)
                    {
                        // change to "Confirm" and store existing value
                        self.Text = "Confirm";
                        if (Identifier.TryParse(box.Text, out Identifier parsed))
                            self.Tag = parsed; // store previous value for refactoring
                        else
                            self.Tag = null;
                    }
                    else
                    {
                        // change back to the original state and perform the refactor
                        string newIdentifierString = box.Text;
                        if (!Identifier.TryParse(newIdentifierString, out Identifier newIdentifier))
                        {
                            MessageBox.Show("Couldn't parse the new identifier; cannot confirm refactor.",
                                "Refactoring Identifier", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            box.Enabled = true;
                            return;
                        }

                        self.Text = "Refactor";
                        if (self.Tag != null && library != null)
                        {
                            // do refactoring
                            var previousIdentifier = (Identifier) self.Tag;
                            if (previousIdentifier.Equals(newIdentifier))
                                return; // silently cancel; nothing changed
                            library.RefactorIdentifier(previousIdentifier, newIdentifier);
                            editorWindow.ApplyAssetListComponents(assetType);
                        }

                        changedEvent(newIdentifier);
                    }
                };
                return container;
            }

            return box;
        }
        /// <summary>
        /// Creates a TextBox input field that allows the user to enter a number.
        /// </summary>
        /// <param name="initialNumber">The initial value of the number input field. Pass null for no value.</param>
        /// <param name="changedEvent">An action that will be called when the value of the number input changes.</param>
        /// <returns>A TextBox control representing the number input field.</returns>
        public static TextBox CreateNumberInput(double? initialNumber, Action<double> changedEvent)
        {
            bool initialNumberIsValid =
                initialNumber.HasValue &&
                !double.IsNaN(initialNumber.Value) &&
                !double.IsInfinity(initialNumber.Value);

            var box = new TextBox();
            box.Width = 100;
            box.Multiline = false;
            box.Text = initialNumberIsValid ? initialNumber.ToString() : string.Empty;
            box.CharacterCasing = CharacterCasing.Normal;
            box.AcceptsReturn = false;
            box.ForeColor = initialNumberIsValid ? Color.Black : Color.DarkRed;

            box.TextChanged += (sender, e) =>
            {
                // try to parse the number
                if (double.TryParse(box.Text, out double parsed))
                {
                    box.ForeColor = Color.Black;
                    changedEvent(parsed);
                }
                else
                {
                    // error parsing
                    box.ForeColor = Color.DarkRed;
                }
            };

            return box;
        }
        /// <summary>
        /// Creates a slider input field which allows the user to choose a value between 0-1.
        /// </summary>
        /// <param name="initialNumber">The initial value of the input. The value will be clamped between 0-1 if it exceeds those limits.</param>
        /// <param name="changedEvent">An action that will be called when the value of the slider changes.</param>
        /// <returns></returns>
        public static TrackBar CreateNumberInput01(float initialNumber, Action<float> changedEvent)
        {
            // clamp value
            if (initialNumber > 1F)
                initialNumber = 1F;
            if (initialNumber < 0F)
                initialNumber = 0F;

            // scale by 100 so it can be incremented by the TrackBar
            int valueInteger = (int) (initialNumber * 100F);

            var slider = new TrackBar();
            slider.Minimum = 0;
            slider.Maximum = 100;
            slider.Value = valueInteger;
            slider.TickFrequency = 25;
            slider.Width = 300;
            slider.Orientation = Orientation.Horizontal;
            slider.TickStyle = TickStyle.BottomRight;
            slider.ValueChanged += (sender, e) =>
            {
                int newValue = slider.Value;
                float scaledValue = newValue / 100F;
                changedEvent(scaledValue);
            };

            changedEvent(initialNumber);
            return slider;
        }

        /// <summary>
        /// Lays out the given controls vertically inside the destination control, fills their width, and enables anchoring.
        /// </summary>
        /// <param name="destination">The control in which the controls will be laid out into.</param>
        /// <param name="controls">The controls to be laid out vertically.</param>
        /// <returns>The minimum size that the container of the controls should be set to.</returns>
        public static Size LayoutVertically(Control destination, IEnumerable<Tuple<string, Control>> controls)
        {
            const int PADDING = 4;
            int tabIndex = 0;
            int y = PADDING;
            int highestWidth = PADDING * 2;

            foreach ((string labelText, Control control) in controls)
            {
                var label = new Label();
                label.AutoSize = true;
                label.Text = labelText;
                label.Top = y;
                label.Left = PADDING;
                destination.Controls.Add(label);
                label.PerformLayout();
                int labelHeight = label.Height;
                y += labelHeight;

                control.TabIndex = tabIndex++;
                control.Top = y;
                control.Left = PADDING;
                destination.Controls.Add(control);
                control.PerformLayout();
                int controlWidth = control.Width + PADDING * 2;
                int controlHeight = control.Height;
                y += controlHeight;
                y += PADDING;
                if (controlWidth > highestWidth)
                    highestWidth = controlWidth;
            }

            y += PADDING;
            return new Size(highestWidth, y);
        }

        /// <summary>
        /// Validates an identifier and ensures the input data is correct. Returns false if the identifier couldn't be validated.
        /// </summary>
        /// <param name="input">The <see cref="Identifier"/> to validate.</param>
        /// <param name="identifierOfCallingAsset">The <see cref="Identifier"/> of the calling asset.</param>
        /// <param name="containingLibrary">The <see cref="AssetLibrary"/> that contains the asset.</param>
        /// <param name="errorMessages">A list of validation errors that occurred.</param>
        /// <returns>False if the asset couldn't be validated. The <paramref name="errorMessages"/> will contain a list of the validation errors that occurred.</returns>
        public static bool ValidateIdentifier(Identifier input, Identifier identifierOfCallingAsset,
            AssetLibrary containingLibrary, List<string> errorMessages)
        {
            if (containingLibrary.definedIdentifiers.Contains(input))
                return true;

            errorMessages.Add($"Undefined identifier '{input}' in asset {identifierOfCallingAsset}.");
            return false;
        }

        /// <summary>
        /// Returns if <paramref name="filePath"/> can be truncated to be relative to <paramref name="assetPath"/>.
        /// </summary>
        public static bool CanTruncatePath(string assetPath, string filePath)
        {
            if (string.IsNullOrEmpty(assetPath) || string.IsNullOrEmpty(filePath))
                return false;

            try
            {
                assetPath = Path.GetFullPath(assetPath);
                filePath = Path.GetFullPath(filePath);
            }
            catch (Exception)
            {
                return false;
            }

            assetPath = Path.GetDirectoryName(assetPath) ?? assetPath;
            return filePath.StartsWith(assetPath, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// Returns the given path to the model relative to this asset's directory.
        /// </summary>
        public static string TruncatePath(string assetPath, string filePath)
        {
            // assetPath might be ".../assets/mars_map/mars.json"
            // filePath might be ".../assets/mars_map/textures/space_rock.png"

            if (string.IsNullOrEmpty(filePath))
                return string.Empty;
            if (string.IsNullOrEmpty(assetPath))
                return filePath;

            // remove file from assetPath
            assetPath = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrEmpty(assetPath))
                return filePath;

            // split both paths into parts
            string[] assetPathParts = assetPath.Split(Path.DirectorySeparatorChar);
            string[] filePathParts = filePath.Split(Path.DirectorySeparatorChar);

            // find the common parts between the two paths
            int commonParts = 0;
            while (commonParts < assetPathParts.Length &&
                   commonParts < filePathParts.Length &&
                   assetPathParts[commonParts] == filePathParts[commonParts])
                commonParts++;

            // remove the common parts from the file path
            string[] truncatedParts = filePathParts
                .Skip(commonParts)
                .ToArray();
            return Path.Combine(truncatedParts);
        }
    }

    /// <summary>
    /// Extension methods for IAsset instances.
    /// </summary>
    public static class AssetExtensionMethods
    {
        /// <summary>
        /// Returns the absolute path of a sub-file to this given asset. <br />
        /// Example:
        /// <code lang="c#">
        /// IAsset asset = (IAsset) "props/test/testprop.json";
        /// string file = "textures/pink.png";
        ///  
        /// // assertion
        /// asset.GetSubFile(file) == "C:/ABC/.../props/test/textures/pink.png"
        /// </code>
        /// </summary>
        /// <param name="asset">The asset containing the base path.</param>
        /// <param name="file">The file, relative to the asset, to get.</param>
        /// <returns>The absolute path to the sub-file.</returns>
        public static string GetSubFile(this IAsset asset, string file)
        {
            string initialPath = asset.JsonFilePath;

            if (string.IsNullOrEmpty(initialPath))
                return file;

            string directoryName = Path.GetDirectoryName(initialPath);

            if (string.IsNullOrEmpty(directoryName))
                return file;

            // intersection of paths
            string[] parts0 = directoryName.Split(Path.DirectorySeparatorChar);
            string[] parts1 = file.Split(Path.DirectorySeparatorChar);
            IEnumerable<string> intersection = parts0.Intersect(parts1);
            string[] finalPath = parts0.Concat(parts1.Except(intersection)).ToArray();

            if (finalPath[0].EndsWith(":")) // drive letter, needs manual directory separator character
                finalPath[0] += Path.DirectorySeparatorChar;

            return Path.Combine(finalPath);
        }
        /// <summary>
        /// Returns the absolute path of a sub-file to the asset at the given directory.<br />
        /// Example:
        /// <code lang="c#">
        /// string assetJSONFilePath = "props/test/testprop.json";
        /// string file = "textures/pink.png";
        ///  
        /// // assertion
        /// assetJSONFilePath.GetSubFile(file) == "C:/ABC/.../props/test/textures/pink.png"
        /// </code>
        /// </summary>
        /// <param name="assetJSONFilePath">The path of the asset's base path.</param>
        /// <param name="file">The file, relative to the asset, to get.</param>
        /// <returns>The absolute path to the sub-file.</returns>
        public static string GetSubFile(this string assetJSONFilePath, string file)
        {
            if (string.IsNullOrEmpty(assetJSONFilePath))
                return file;

            string directoryName = Path.GetDirectoryName(assetJSONFilePath);

            if (string.IsNullOrEmpty(directoryName))
                return file;

            // intersection of paths
            string[] parts0 = directoryName.Split(Path.DirectorySeparatorChar);
            string[] parts1 = file.Split(Path.DirectorySeparatorChar);
            IEnumerable<string> intersection = parts0.Intersect(parts1);
            string[] finalPath = parts0.Concat(parts1.Except(intersection)).ToArray();
            finalPath[0] += Path.DirectorySeparatorChar; // make the drive letter absolute

            return Path.Combine(finalPath);
        }
        /// <summary>
        /// Returns the directory path of the given asset. If the asset's JSON file path is null or empty,
        /// returns an empty string representing the current directory.
        /// </summary>
        /// <param name="asset">The asset for which to get the directory path.</param>
        /// <returns>The directory path of the asset.</returns>
        public static string GetDirectory(this IAsset asset)
        {
            string initialPath = asset.JsonFilePath;
            return string.IsNullOrEmpty(initialPath) ? string.Empty : Path.GetDirectoryName(initialPath);
        }
    }
}