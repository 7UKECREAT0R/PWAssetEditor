using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PWAssetEditor.Library.Asset
{
    /// <summary>
    /// A prop that can be placed into a map for decoration.
    /// </summary>
    public class Prop : IAsset
    {
        public AssetLibrary ContainingLibrary { get; set; }

        private static string MODEL_TYPES_FILTER =>
            string.Join(";", SUPPORTED_MODEL_TYPES.Select(m => $"*.{m.ToLower()}"));
        private static readonly string[] SUPPORTED_MODEL_TYPES =
        {
            "OBJ",
            "STL",
            "GLB",
            "GLTF",
            "DAE",
            "FBX"
        };
        private static readonly string[] PARTIALLY_SUPPORTED_MODEL_TYPES =
        {
            "STL"
        };
        /// <summary>
        /// Determines if a model file is allowed based on its extension.
        /// </summary>`
        /// <param name="file">The file path of the model file.</param>
        /// <returns>True if the model file is allowed, false otherwise.</returns>
        private static bool IsModelFileAllowed(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                return false;

            string extension = Path.GetExtension(file);
            return !string.IsNullOrEmpty(extension) &&
                   SUPPORTED_MODEL_TYPES.Any(type => file.EndsWith(type, StringComparison.OrdinalIgnoreCase));
        }
        /// <summary>
        /// Determines if a model file is partially supported based on its extension.
        /// </summary>
        /// <param name="file">The file path of the model file.</param>
        /// <returns>True if the model file is partially supported, false if it's either not supported at all or fully supported.</returns>
        private static bool IsModelFilePartiallySupported(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                return false;

            string extension = Path.GetExtension(file);
            return !string.IsNullOrEmpty(extension) &&
                   PARTIALLY_SUPPORTED_MODEL_TYPES.Any(type => file.EndsWith(type, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// The identifier of this prop.
        /// </summary>
        [JsonProperty("identifier", Required = Required.Always)]
        public Identifier? identifier;

        /// <summary>
        /// The static properties of this prop.
        /// </summary>
        [JsonProperty("prop")]
        public PropProperties properties = new PropProperties()
        {
            baseScale = 1F
        };

        /// <summary>
        /// Validates the Prop asset and ensures the input data is correct. Returns true if all is good.
        /// </summary>
        /// <param name="containingLibrary"></param>
        /// <param name="errorMessages"></param>
        /// <returns>False if the asset couldn't be validated, true otherwise.</returns>
        public bool Validate(AssetLibrary containingLibrary, List<string> errorMessages)
        {
            if (!this.identifier.HasValue)
            {
                errorMessages.Add("Prop does not have an identifier defined.");
                return false;
            }

            if (!this.identifier.Value.IsValid)
            {
                errorMessages.Add($"Prop {this.identifier} has invalid identifier.");
                return false;
            }

            if (this.properties.baseScale == 0F)
            {
                errorMessages.Add(
                    $"Prop {this.identifier}: Base scale is zero; model will not be visible and can't be scaled to be visible.");
                return false;
            }

            // validate materials
            bool hasMaterial = this.properties.material != null;
            bool hasMaterials = this.properties.materials != null;

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (hasMaterial && hasMaterials)
            {
                errorMessages.Add($"Prop {this.identifier}: Both 'material' and 'materials' fields cannot be set.");
                return false;
            }

            if (!hasMaterial && !hasMaterials)
            {
                errorMessages.Add($"Prop {this.identifier}: No materials set.");
                return false;
            }

            // validate model file
            return ValidateModel(containingLibrary, errorMessages);
        }
        private static bool hasAcknowledgedUnsupportedFiles = false;
        private bool ValidateModel(AssetLibrary containingLibrary, List<string> errorMessages)
        {
            if (!this.identifier.HasValue)
            {
                errorMessages.Add("Prop does not have an identifier defined.");
                return false;
            }

            // validate model file
            if (!IsModelFileAllowed(this.properties.modelFilePath))
            {
                string extension = Path.GetExtension(this.properties.modelFilePath);
                errorMessages.Add($"Unsupported model extension: {extension}");
                return false;
            }

            if (!hasAcknowledgedUnsupportedFiles && IsModelFilePartiallySupported(this.properties.modelFilePath))
            {
                hasAcknowledgedUnsupportedFiles = true;
                MessageBox.Show(
                    "Warning: This model file extension is only partially supported. UVs and custom normals will not be applied to models of this type.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            string modelPath = this.GetSubFile(this.properties.modelFilePath);
            if (!File.Exists(modelPath))
            {
                errorMessages.Add($"Prop {this.identifier}: Model file \"{this.properties.modelFilePath}\" not found.");
                return false;
            }

            // validate material identifiers
            Identifier[] materialIdentifiers = this.properties.Materials;
            Identifier callingIdentifier = this.identifier.Value;
            return materialIdentifiers.Aggregate(true, (current, materialIdentifier) =>
                current & AssetUtils.ValidateIdentifier(materialIdentifier, callingIdentifier, containingLibrary,
                    errorMessages));
        }

        private string _jsonFilePath;
        [JsonIgnore]
        public string JsonFilePath
        {
            get
            {
                if (this._jsonFilePath == null)
                {
                    // generate a location based on the properties
                    string folder = Path.Combine(this.ContainingLibrary.GetAssetsDirectory(),
                        AssetLibrary.PROPS_DIR_NAME);
                    string propBestName =
                        string.IsNullOrEmpty(this.properties.name)
                            ? this.identifier.HasValue ? this.identifier.Value.assetName : string.Empty
                            : this.properties.name.Trim().Replace(' ', '_');
                    if (string.IsNullOrEmpty(propBestName))
                        propBestName = "unknown";

                    propBestName += ".json";
                    string jsonFilePath = Path.Combine(folder, propBestName);
                    return jsonFilePath;
                }

                return this._jsonFilePath;
            }
            set => this._jsonFilePath = value;
        }
        [JsonIgnore]
        public Identifier? Identifier
        {
            get => this.identifier;
            set => this.identifier = value;
        }
        [JsonIgnore]
        public AssetType Type => AssetType.prop;

        public IEnumerable<string> GetAllReferencedFiles
        {
            get
            {
                string basePath = this.JsonFilePath;
                if (string.IsNullOrEmpty(basePath))
                    return Array.Empty<string>();
                if (string.IsNullOrEmpty(this.properties.modelFilePath))
                    return new[] {basePath};

                string modelPath = basePath.GetSubFile(this.properties.modelFilePath);

                return new[]
                {
                    basePath,
                    modelPath
                };
            }
        }
        public IEnumerable<Identifier> GetAllDependedAssets => this.properties.Materials;
        public override string ToString()
        {
            return $"Prop '{this.properties.name}' - '.\\{this.properties.modelFilePath}'";
        }
        public JObject Serialize()
        {
            return new JObject
            {
                ["identifier"] = this.identifier.GetValueOrDefault().ToString(),
                ["prop"] = new JObject
                {
                    ["name"] = this.properties.name,
                    ["model"] = this.properties.modelFilePath,
                    ["base_scale"] = this.properties.baseScale,
                    ["materials"] = new JArray(
                        this.properties.Materials
                            .Select(m => m.ToString())
                            .Cast<object>()
                            .ToArray()
                    )
                }
            };
        }

        public IEnumerable<Tuple<string, Control>> CreateEditControls(AssetLibrary containingLibrary,
            bool lockIdentifier, AssetEditor containingEditor)
        {
            Prop self = this;
            var controls = new List<Tuple<string, Control>>();

            // Identifier Control
            Control identifierEditor =
                AssetUtils.CreateIdentifierInput(
                    this.identifier ?? new Identifier(containingLibrary.authorName, AssetType.prop, ""),
                    (newIdentifier) => { self.identifier = newIdentifier; }, lockIdentifier,
                    containingLibrary, containingEditor, AssetType.prop);
            controls.Add(new Tuple<string, Control>("Identifier", identifierEditor));

            // Name Control
            var nameEditor = new TextBox
            {
                Text = this.properties.name
            };
            nameEditor.TextChanged += (sender, args) =>
            {
                string text = nameEditor.Text;
                if (string.IsNullOrWhiteSpace(text))
                    return;

                self.properties.name = text;
            };
            controls.Add(new Tuple<string, Control>("Name", nameEditor));

            // Model File Path Control
            var modelFilePathEditor = new Button
            {
                Text = string.IsNullOrEmpty(this.properties.modelFilePath)
                    ? "unset"
                    : "." + Path.DirectorySeparatorChar + this.properties.modelFilePath
            };
            modelFilePathEditor.Width *= 3;
            modelFilePathEditor.Click += (sender, args) =>
            {
                using (var dialog = new OpenFileDialog())
                {
                    string extensionFilter = MODEL_TYPES_FILTER;
                    dialog.Filter = $"Model files ({extensionFilter})|{extensionFilter}";
                    dialog.Title = "Select a model file";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string pick = dialog.FileName;
                        string thisAssetPath = self.JsonFilePath;

                        if (pick == null)
                            return;
                        if (thisAssetPath == null)
                        {
                            MessageBox.Show(
                                "Asset is not ready to be given a path. Possibly missing a valid identifier?",
                                "Selecting model file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        if (!AssetUtils.CanTruncatePath(thisAssetPath, pick))
                        {
                            // copy the chosen model into the local directory.
                            DialogResult result = MessageBox.Show(
                                "Model is not relative to the asset's file. Copy the model into the asset's folder?",
                                "Model Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (result == DialogResult.No)
                                return;

                            string fileName = Path.GetFileName(pick);
                            string baseDirectory = Path.GetDirectoryName(thisAssetPath) ?? string.Empty;
                            string newDirectory = Path.Combine(baseDirectory, "models");
                            if (!Directory.Exists(newDirectory))
                                Directory.CreateDirectory(newDirectory);
                            string newFile = Path.Combine(newDirectory, fileName);

                            if (File.Exists(newFile))
                            {
                                result = MessageBox.Show(
                                    $"Model in destination directory already exists:\n\t{newFile}\n\nReplace file?",
                                    "Model Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (result == DialogResult.No)
                                    return;

                                // delete it to prevent collision with File.Copy
                                File.Delete(newFile);
                            }

                            File.Copy(pick, newFile);
                            pick = newFile;
                        }

                        pick = AssetUtils.TruncatePath(thisAssetPath, pick);

                        string previousModelPath = self.properties.modelFilePath;
                        self.properties.modelFilePath = pick;

                        var errors = new List<string>();
                        if (self.ValidateModel(containingLibrary, errors))
                        {
                            modelFilePathEditor.Text =
                                "." + Path.DirectorySeparatorChar + self.properties.modelFilePath;
                            return;
                        }

                        // error with the model
                        MessageBox.Show("Error(s) validating model:\n\n" + string.Join("\n", errors),
                            "Model Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        self.properties.modelFilePath = previousModelPath;
                    }
                }
            };
            controls.Add(new Tuple<string, Control>("3D Model", modelFilePathEditor));

            // BaseScale Control
            if (this.properties.baseScale == 0F)
                this.properties.baseScale = 1F;
            float inputValue = this.properties.baseScale;

            TextBox baseScaleEditor = AssetUtils.CreateNumberInput(inputValue,
                newBaseScale => { this.properties.baseScale = (float) newBaseScale; });
            controls.Add(new Tuple<string, Control>("Model Base Scale", baseScaleEditor));

            // Material Identifiers Control
            var materialsControl = new TextBox
            {
                Multiline = true
            };
            materialsControl.Height *= 4;
            materialsControl.Width *= 2;
            Identifier[] materialIdentifiers = this.properties.Materials;
            bool isValid = materialIdentifiers.Any();

            if (isValid)
            {
                string[] materialsLines = materialIdentifiers.Select(id => id.ToString()).ToArray();
                materialsControl.Lines = materialsLines;
            }

            materialsControl.ForeColor = isValid ? Color.Black : Color.DarkRed;
            materialsControl.TextChanged += (sender, e) =>
            {
                string[] newLines = materialsControl.Lines;
                if (newLines.Length == 0)
                    return;

                int parseErrors = 0;
                var validIdentifiers = new List<Identifier>();
                foreach (string line in newLines)
                {
                    if (!PWAssetEditor.Library.Identifier.TryParse(line, out Identifier i))
                    {
                        parseErrors++;
                        continue;
                    }

                    if (this.identifier.HasValue)
                        if (!AssetUtils.ValidateIdentifier(i, this.identifier.Value, containingLibrary,
                                new List<string>()))
                        {
                            parseErrors++;
                            continue;
                        }

                    validIdentifiers.Add(i);
                }

                bool valid = parseErrors == 0 && validIdentifiers.Count > 0;
                materialsControl.ForeColor = valid ? Color.Black : Color.DarkRed;

                if (!valid)
                    return;

                if (validIdentifiers.Count == 1)
                {
                    this.properties.materials = null;
                    this.properties.material = validIdentifiers.First();
                }
                else
                {
                    this.properties.material = null;
                    this.properties.materials = validIdentifiers.ToArray();
                }
            };

            controls.Add(new Tuple<string, Control>("Materials; each line is an identifier.", materialsControl));

            var materialsListView = new ListBox();
            List<string> materialsList = containingLibrary.Materials.Where(m => m.identifier.HasValue)
                .Select(m => m.identifier.Value.ToString())
                .ToList();
            materialsList.Sort();
            materialsListView.Items.AddRange(materialsList.Cast<object>().ToArray());
            materialsListView.Click += (sender, args) =>
            {
                string selectedLine = (string) materialsListView.SelectedItem;
                if (!string.IsNullOrEmpty(selectedLine))
                    Clipboard.SetText(selectedLine);
            };
            controls.Add(new Tuple<string, Control>("Available materials (click to copy)", materialsListView));

            return controls;
        }
        public bool IsFilledOut
        {
            get
            {
                if (!this.identifier.HasValue)
                    return false;
                if (!this.identifier.Value.IsValid)
                    return false;

                bool hasMaterial = this.properties.material != null;
                bool hasMaterials = this.properties.materials != null;

                if (hasMaterial == hasMaterials)
                    return false;
                if (string.IsNullOrWhiteSpace(this.properties.name))
                    return false;
                if (string.IsNullOrWhiteSpace(this.properties.modelFilePath))
                    return false;
                if (!IsModelFileAllowed(this.properties.modelFilePath))
                    return false;
                if (this.properties.baseScale == 0F)
                    return false;

                // validate material identifiers
                var errorMessages = new List<string>();
                if (this.properties.material.HasValue)
                {
                    Identifier material = this.properties.material.Value;
                    if (!AssetUtils.ValidateIdentifier(material, this.identifier.Value, this.ContainingLibrary,
                            errorMessages))
                        return false;
                }
                else
                {
                    // ReSharper disable once PossibleNullReferenceException
                    Identifier[] materials = this.properties.materials;
                    return materials != null && materials.All(material =>
                        AssetUtils.ValidateIdentifier(material, this.identifier.Value, this.ContainingLibrary,
                            errorMessages));
                }

                // validate model exists
                string modelPath = this.GetSubFile(this.properties.modelFilePath);
                return File.Exists(modelPath);
            }
        }
        public bool RefactorIdentifier(Identifier from, Identifier to)
        {
            if (this.properties.material.HasValue)
            {
                if (this.properties.material.Value.Equals(from))
                {
                    this.properties.material = to;
                    return true;
                }

                return false;
            }

            if (this.properties.materials != null)
            {
                bool anyChanges = false;
                for (int i = 0; i < this.properties.materials.Length; i++)
                    if (this.properties.materials[i].Equals(from))
                    {
                        this.properties.materials[i] = to;
                        anyChanges = true;
                    }

                return anyChanges;
            }

            return false;
        }
        public bool RefactorAuthorName(string from, string to)
        {
            if (!this.identifier.HasValue)
                return false;

            Identifier id = this.identifier.Value;

            if (id.authorName.Equals(from))
            {
                this.identifier = id.MutateAuthorName(to);
                return true;
            }

            return false;
        }
    }

    public struct PropProperties
    {
        /// <summary>
        /// The name of the prop; only shown in the map editor.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string name;

        /// <summary>
        /// The path to the model file relative to the defining JSON file's directory.
        /// </summary>
        [JsonProperty("model", Required = Required.Always)]
        public string modelFilePath;

        /// <summary>
        /// The base scale of the prop, applied by default when it's added to the map with the editor.
        /// </summary>
        [JsonProperty("base_scale")]
        public float baseScale;

        [JsonProperty("material", Required = Required.DisallowNull)]
        internal Identifier? material;
        [JsonProperty("materials", Required = Required.DisallowNull)]
        internal Identifier[] materials;

        /// <summary>
        /// The materials of this prop, starting from slot 0.
        /// </summary>
        public Identifier[] Materials => this.material.HasValue
            ? new[] {this.material.Value}
            : this.materials ?? Array.Empty<Identifier>();
    }
}