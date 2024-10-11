using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PWAssetEditor.Library.MapBlocks;

namespace PWAssetEditor.Library.Asset
{
    public class Map : IAsset
    {
        public AssetLibrary ContainingLibrary { get; set; }

        /// <summary>
        /// The identifier of the map.
        /// </summary>
        [JsonProperty("identifier", Required = Required.Always)]
        public Identifier? identifier = null;

        /// <summary>
        /// The static info about the map.
        /// </summary>
        [JsonProperty("map", Required = Required.Always)]
        public MapInfo info = new MapInfo
        {
            deathPlaneY = -100F
        };

        /// <summary>
        /// The blocks that make up the map; blocks, entities, props, and text.
        /// </summary>
        [JsonIgnore]
        public MapBlock[] blocks = Array.Empty<MapBlock>();

        public bool Validate(AssetLibrary containingLibrary, List<string> errorMessages)
        {
            if (!this.identifier.HasValue)
            {
                errorMessages.Add("Map does not have an identifier defined.");
                return false;
            }

            if (!this.identifier.Value.IsValid)
            {
                errorMessages.Add($"Map {this.identifier} has invalid identifier.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.info.name))
            {
                errorMessages.Add($"Map {this.identifier}: Name is empty.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.info.description))
            {
                errorMessages.Add($"Map {this.identifier}: Description is empty.");
                return false;
            }

            return true;
        }
        public void LoadBlocksFromJSON(JArray json)
        {
            this.blocks = (
                from token in json
                where token.Type == JTokenType.Object
                select MapBlockConverter.ReadJson(token as JObject)
            ).ToArray();
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
                        AssetLibrary.MAPS_DIR_NAME);
                    string mapBestName =
                        string.IsNullOrEmpty(this.info.name)
                            ? this.identifier.HasValue ? this.identifier.Value.assetName : string.Empty
                            : this.info.name.Trim().Replace(' ', '_');
                    if (string.IsNullOrEmpty(mapBestName))
                        mapBestName = "unknown";

                    mapBestName += ".json";
                    string jsonFilePath = Path.Combine(folder, mapBestName);
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
        public AssetType Type => AssetType.map;

        public IEnumerable<string> GetAllReferencedFiles
        {
            get
            {
                string basePath = this.JsonFilePath;
                return string.IsNullOrEmpty(basePath) ? Array.Empty<string>() : new[] {basePath};
            }
        }
        public IEnumerable<Identifier> GetAllDependedAssets => this.blocks
            .SelectMany(block => block.GetAllDependedAssets)
            .Distinct();

        public override string ToString()
        {
            return $"Map '{this.info.name}' - {this.blocks.Length} blocks";
        }

        public JObject Serialize()
        {
            return new JObject
            {
                ["identifier"] = this.identifier.GetValueOrDefault().ToString(),
                ["map"] = new JObject
                {
                    ["name"] = this.info.name,
                    ["description"] = this.info.description,
                    ["death_plane"] = this.info.deathPlaneY
                },
                ["blocks"] = new JArray(
                    this.blocks
                        .Select(b => b.Serialize())
                        .Cast<object>()
                        .ToArray()
                )
            };
        }
        public IEnumerable<Tuple<string, Control>> CreateEditControls(AssetLibrary containingLibrary,
            bool lockIdentifier, AssetEditor containingEditor)
        {
            Map self = this;

            Control identifierEditor =
                AssetUtils.CreateIdentifierInput(
                    this.identifier ?? new Identifier(containingLibrary.authorName, AssetType.map, ""),
                    (newIdentifier) => { self.identifier = newIdentifier; }, lockIdentifier,
                    containingLibrary, containingEditor, AssetType.map);

            var nameEditor = new TextBox();
            nameEditor.Width = 500;
            if (!string.IsNullOrWhiteSpace(this.info.name))
                nameEditor.Text = this.info.name.Trim();
            nameEditor.TextChanged += (sender, args) =>
            {
                string nameEditorText = nameEditor.Text;
                self.info.name = nameEditorText.Trim();
            };

            var descriptionEditor = new TextBox();
            descriptionEditor.Width = 500;
            descriptionEditor.Height *= 3;
            descriptionEditor.Multiline = true;
            descriptionEditor.ScrollBars = ScrollBars.Vertical;
            if (!string.IsNullOrWhiteSpace(this.info.description))
                descriptionEditor.Text = this.info.description.Trim();
            descriptionEditor.TextChanged += (sender, args) =>
            {
                string descriptionEditorText = descriptionEditor.Text;
                self.info.description = descriptionEditorText.Trim();
            };

            TextBox deathPlaneEditor = AssetUtils.CreateNumberInput(this.info.deathPlaneY,
                (newValue) => { self.info.deathPlaneY = (float) newValue; });

            return new[]
            {
                new Tuple<string, Control>("Identifier", identifierEditor),
                new Tuple<string, Control>("Map Name", nameEditor),
                new Tuple<string, Control>("Map Description", descriptionEditor),
                new Tuple<string, Control>("Death Plane Y", deathPlaneEditor)
            };
        }

        public bool IsFilledOut =>
            this.identifier.HasValue &&
            this.identifier.Value.IsValid &&
            !string.IsNullOrWhiteSpace(this.info.name) &&
            !string.IsNullOrWhiteSpace(this.info.description) &&
            !float.IsNaN(this.info.deathPlaneY) &&
            !float.IsInfinity(this.info.deathPlaneY);

        public bool RefactorIdentifier(Identifier from, Identifier to)
        {
            return this.blocks.Aggregate(false, (current, mapBlock) =>
                current | mapBlock.RefactorIdentifier(from, to));
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

    public struct MapInfo
    {
        /// <summary>
        /// The name of the map, as shown in the UI.
        /// </summary>
        [JsonProperty("name")]
        public string name;

        /// <summary>
        /// The description of the map, shown below the name.
        /// </summary>
        [JsonProperty("description")]
        public string description;

        /// <summary>
        /// The position in y-level that should be considered the death plane, at which players are
        /// teleported back up and almost killed when touching.
        /// </summary>
        [JsonProperty("death_plane")]
        public float deathPlaneY;

        /// <summary>
        /// Internal workshop upload information.
        /// </summary>
        [JsonProperty("workshop", Required = Required.DisallowNull)]
        internal WorkshopMetadata? workshop;
    }

    internal struct WorkshopMetadata
    {
        [JsonProperty("item")]
        internal long itemID;

        [JsonProperty("publisher")]
        internal long publisherID;

        private WorkshopMetadata(long itemID, long publisherID)
        {
            this.itemID = itemID;
            this.publisherID = publisherID;
        }
    }
}