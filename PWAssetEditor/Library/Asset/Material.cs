using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Color = System.Drawing.Color;

namespace PWAssetEditor.Library.Asset
{
    public class Material : IAsset
    {
        public AssetLibrary ContainingLibrary { get; set; }

        private static readonly string[] SUPPORTED_TEXTURE_TYPES =
        {
            "PNG",
            "JPG"
        };
        /// <summary>
        /// Checks if the provided texture file is allowed based on the supported file types.
        /// </summary>
        /// <param name="file">The file name or path of the texture file to check.</param>
        /// <returns>True if the texture file is allowed; otherwise, false.</returns>
        public static bool IsTextureFileAllowed(string file)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (!file.Contains("."))
                return false; // no extension

            return SUPPORTED_TEXTURE_TYPES.Any(type => file.EndsWith(type, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// The identifier of this material.
        /// </summary>
        [JsonProperty("identifier", Required = Required.Always)]
        public Identifier? identifier;

        /// <summary>
        /// The static properties of this material.
        /// </summary>
        [JsonProperty("material")]
        public MaterialProperties properties = new MaterialProperties()
        {
            alpha = 1.0F,
            metallic = 0F,
            roughness = 0.7F
        };

        public bool Validate(AssetLibrary containingLibrary, List<string> errorMessages)
        {
            if (!this.identifier.HasValue)
            {
                errorMessages.Add("Material does not have an identifier defined.");
                return false;
            }

            if (!this.identifier.Value.IsValid)
            {
                errorMessages.Add($"Material {this.identifier} has invalid identifier.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.properties.name))
            {
                errorMessages.Add($"Material {this.identifier} has no name.");
                return false;
            }

            if (this.properties.texture != null)
                if (!ValidateTexture(errorMessages))
                    return false;

            // if alpha is within a small margin of 1, round it to 1
            if (Math.Abs(this.properties.alpha - 1.0F) < 0.001F)
                this.properties.alpha = 1;

            // clamp it to 0-1 using min/max
            this.properties.alpha = Math.Min(1F, Math.Max(0F, this.properties.alpha));

            return true;
        }
        private bool ValidateTexture(List<string> errorMessages)
        {
            // validate extension
            if (!IsTextureFileAllowed(this.properties.texture))
            {
                errorMessages.Add(
                    $"Material {this.identifier}: Unsupported texture extension \"{Path.GetExtension(this.properties.texture)}\"");
                return false;
            }

            // make sure the file exists
            string texturePath = this.GetSubFile(this.properties.texture);
            if (!File.Exists(texturePath))
            {
                errorMessages.Add($"Material {this.identifier}: Texture file \"{this.properties.texture}\" not found.");
                return false;
            }

            return true;
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
                        AssetLibrary.MATERIALS_DIR_NAME);
                    string materialBestName = this.identifier.HasValue ? this.identifier.Value.assetName : "unknown";

                    materialBestName += ".json";
                    string jsonFilePath = Path.Combine(folder, materialBestName);
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
        public AssetType Type => AssetType.material;

        public IEnumerable<string> GetAllReferencedFiles
        {
            get
            {
                string basePath = this.JsonFilePath;
                if (string.IsNullOrEmpty(basePath))
                    return Array.Empty<string>();
                if (string.IsNullOrEmpty(this.properties.texture))
                    return new[] {basePath};

                string texturePath = basePath.GetSubFile(this.properties.texture);

                return new[]
                {
                    basePath,
                    texturePath
                };
            }
        }
        public IEnumerable<Identifier> GetAllDependedAssets => Array.Empty<Identifier>();
        public override string ToString()
        {
            string visualInfo = this.properties.texture != null
                ? $"'{GetTruncatedTexture()}'"
                : $"{this.properties.color}";
            return $"Material '{this.identifier?.assetName ?? "???"}' - {visualInfo}";

            // returns the texture path truncated to just be relative to the assets folder
            string GetTruncatedTexture()
            {
                string path = this.properties.texture;
                string assetsFolder = "assets" + Path.DirectorySeparatorChar;

                int startIndex = path.LastIndexOf(assetsFolder, StringComparison.OrdinalIgnoreCase);

                return startIndex >= 0
                    ? "." + Path.DirectorySeparatorChar + path.Substring(startIndex + assetsFolder.Length)
                    : "." + Path.DirectorySeparatorChar + path;
            }
        }

        public JObject Serialize()
        {
            return new JObject
            {
                ["identifier"] = this.identifier.GetValueOrDefault().ToString(),
                ["material"] = this.properties.Serialize()
            };
        }

        public IEnumerable<Tuple<string, Control>> CreateEditControls(AssetLibrary containingLibrary,
            bool lockIdentifier, AssetEditor containingEditor)
        {
            Material self = this;

            Control identifierEditor =
                AssetUtils.CreateIdentifierInput(
                    this.identifier ?? new Identifier(containingLibrary.authorName, AssetType.material, ""),
                    (newIdentifier) => { self.identifier = newIdentifier; }, lockIdentifier,
                    containingLibrary, containingEditor, AssetType.material);

            // name
            var nameEditor = new TextBox();
            nameEditor.Width = 500;
            if (!string.IsNullOrWhiteSpace(this.properties.name))
                nameEditor.Text = this.properties.name.Trim();
            nameEditor.TextChanged += (sender, args) =>
            {
                string nameEditorText = nameEditor.Text;
                self.properties.name = nameEditorText.Trim();
            };

            // material properties
            var colorEditor = new Button();
            colorEditor.Width = 40;
            colorEditor.Height = 40;
            colorEditor.Text = string.Empty;
            colorEditor.FlatStyle = FlatStyle.Flat;
            colorEditor.FlatAppearance.BorderSize = 0;
            colorEditor.Enabled = this.properties.texture == null;
            Color colorEditorBackColor = this.properties.texture != null
                ? Color.Magenta
                : this.properties.color.ToWinFormsColor(this.properties.alpha);
            colorEditor.BackColor = colorEditorBackColor;
            colorEditor.FlatAppearance.BorderColor = colorEditorBackColor;
            colorEditor.Click += (s, e) =>
            {
                using (var colorDialog = new ColorDialog())
                {
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        Color chosenColor = colorDialog.Color;
                        colorEditor.BackColor = chosenColor;
                        colorEditor.FlatAppearance.BorderColor = colorEditorBackColor;
                        self.properties.color = MaterialColor.FromWinFormsColor(chosenColor);
                    }
                }
            };

            var textureCheckbox = new CheckBox();
            textureCheckbox.Text = "Uses Texture";
            textureCheckbox.Checked = this.properties.texture != null;
            textureCheckbox.CheckedChanged += (sender, e) =>
            {
                if (textureCheckbox.Checked)
                    using (var dialog = new OpenFileDialog())
                    {
                        dialog.Filter = "Texture Files (*.png;*.jpg)|*.png;*.jpg";
                        dialog.Title = "Select a texture file";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            string pick = dialog.FileName;
                            string thisAssetPath = self.JsonFilePath;

                            if (pick == null)
                                return;
                            if (thisAssetPath == null)
                            {
                                MessageBox.Show(
                                    "Material is not ready to be given a path. Possibly missing a valid identifier?",
                                    "Selecting texture file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            if (!AssetUtils.CanTruncatePath(thisAssetPath, pick))
                            {
                                // copy the chosen texture into the local directory.
                                DialogResult result = MessageBox.Show(
                                    "Texture is not relative to the asset's file. Copy the texture into the asset's folder?",
                                    "Material Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (result == DialogResult.No)
                                {
                                    self.properties.texture = null;
                                    textureCheckbox.Checked = false;
                                    colorEditor.Enabled = true;
                                    colorEditor.Visible = true;
                                    return;
                                }

                                string fileName = Path.GetFileName(pick);
                                string baseDirectory = Path.GetDirectoryName(thisAssetPath) ?? string.Empty;
                                string newDirectory = Path.Combine(baseDirectory, "textures");
                                if (!Directory.Exists(newDirectory))
                                    Directory.CreateDirectory(newDirectory);
                                string newFile = Path.Combine(newDirectory, fileName);

                                if (File.Exists(newFile))
                                {
                                    result = MessageBox.Show(
                                        $"Texture in destination directory already exists:\n\t{newFile}\n\nReplace file?",
                                        "Material Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                    if (result == DialogResult.No)
                                    {
                                        self.properties.texture = null;
                                        textureCheckbox.Checked = false;
                                        colorEditor.Enabled = true;
                                        colorEditor.Visible = true;
                                        return;
                                    }

                                    // delete to prevent collision with File.Copy
                                    File.Delete(newFile);
                                }

                                File.Copy(pick, newFile);
                                pick = newFile;
                            }

                            pick = AssetUtils.TruncatePath(thisAssetPath, pick);
                            self.properties.texture = pick;

                            var errors = new List<string>();
                            if (self.ValidateTexture(errors))
                            {
                                colorEditor.Enabled = false;
                                colorEditor.Visible = false;
                                return;
                            }

                            // error with the texture
                            MessageBox.Show("Error(s) validating texture:\n\n" + string.Join("\n", errors),
                                "Material Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            self.properties.texture = null;
                            textureCheckbox.Checked = false;
                            colorEditor.Enabled = true;
                            colorEditor.Visible = true;
                            return;
                        }

                        self.properties.texture = null;
                        textureCheckbox.Checked = false;
                        colorEditor.Enabled = true;
                        colorEditor.Visible = true;
                        return;
                    }

                self.properties.texture = null;
                colorEditor.Enabled = true;
                colorEditor.Visible = true;
            };

            TrackBar alphaEditor = AssetUtils.CreateNumberInput01(this.properties.alpha,
                newAlpha => { self.properties.alpha = (float) Math.Max(0.0, Math.Min(1.0, newAlpha)); });
            TrackBar roughnessEditor = AssetUtils.CreateNumberInput01(this.properties.roughness,
                newRoughness => { self.properties.roughness = (float) Math.Max(0.0, Math.Min(1.0, newRoughness)); });
            TrackBar metallicEditor = AssetUtils.CreateNumberInput01(this.properties.metallic,
                newMetallic => { self.properties.metallic = (float) Math.Max(0.0, Math.Min(1.0, newMetallic)); });

            return new[]
            {
                new Tuple<string, Control>("Identifier", identifierEditor),
                new Tuple<string, Control>("Name", nameEditor),
                new Tuple<string, Control>("Use Texture", textureCheckbox),
                new Tuple<string, Control>("Color", colorEditor),
                new Tuple<string, Control>("Alpha (0-1)", alphaEditor),
                new Tuple<string, Control>("Roughness (0-1)", roughnessEditor),
                new Tuple<string, Control>("Metallic (0-1)", metallicEditor)
            };
        }
        public bool IsFilledOut =>
            this.identifier.HasValue &&
            this.identifier.Value.IsValid;

        public bool RefactorIdentifier(Identifier from, Identifier to)
        {
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

    [JsonConverter(typeof(MaterialPropertiesConverter))]
    public struct MaterialProperties
    {
        public JObject Serialize()
        {
            var part = new JObject
            {
                ["name"] = this.name,
                ["roughness"] = this.roughness,
                ["metallic"] = this.metallic,
                ["alpha"] = this.alpha
            };

            if (this.texture == null)
                part["color"] = this.color.AsJSON();
            else
                part["texture"] = this.texture;

            return part;
        }

        /// <summary>
        /// The user-facing name of the material.
        /// </summary>
        public string name;

        /// <summary>
        /// The path to the texture image relative to the defining JSON file's directory.
        /// </summary>
        public string texture;
        /// <summary>
        /// The alpha of this material.
        /// </summary>
        public float alpha;
        /// <summary>
        /// The color of this material.
        /// </summary>
        public MaterialColor color;
        /// <summary>
        /// The roughness component of this material, between 0.0-1.0.<br /><br />
        /// 0.0 means completely smooth, 1.0 means completely diffuse.
        /// </summary>
        public float roughness;
        /// <summary>
        /// The metallic component of this material, between 0.0-1.0.<br /><br />
        /// 0.0 means non-metallic. 1.0 means completely metallic.
        /// </summary>
        public float metallic;
    }

    internal class MaterialPropertiesConverter : JsonConverter<MaterialProperties>
    {
        public override void WriteJson(JsonWriter writer, MaterialProperties value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            if (value.texture != null)
            {
                writer.WritePropertyName("texture");
                serializer.Serialize(writer, value.texture);
            }
            else
            {
                writer.WritePropertyName("color");
                serializer.Serialize(writer, value.color);
                writer.WritePropertyName("alpha");
                serializer.Serialize(writer, Math.Min(1F, Math.Max(0F, value.alpha)));
            }

            writer.WritePropertyName("roughness");
            serializer.Serialize(writer, value.roughness);
            writer.WritePropertyName("metallic");
            serializer.Serialize(writer, value.metallic);

            writer.WriteEndObject();
        }
        public override MaterialProperties ReadJson(JsonReader reader, Type objectType,
            MaterialProperties existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            JObject entireObject = JObject.Load(reader);
            var properties = new MaterialProperties
            {
                name = (entireObject["name"] ?? "unset").Value<string>(),
                alpha = (entireObject["alpha"] ?? 1.0F).Value<float>(),
                roughness = (entireObject["roughness"] ?? 0.75F).Value<float>(),
                metallic = (entireObject["metallic"] ?? 0.0F).Value<float>()
            };

            if (entireObject.TryGetValue("texture", out JToken textureToken))
            {
                properties.texture = textureToken.Value<string>();
            }
            else
            {
                properties.texture = null;

                JToken colorToken = entireObject["color"] ?? throw new JsonReaderException(
                    $"Missing 'color' or 'texture' property in material definition at: {reader.Path}");
                properties.color = MaterialColor.FromToken(colorToken);
            }

            return properties;
        }
    }
}