using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PWAssetEditor.Library.Asset;

namespace PWAssetEditor.Library
{
    /// <summary>
    /// Class for loading, caching, and writing Prego Wars assets.
    /// </summary>
    public class AssetLibrary
    {
        private const string ASSETS_DIR_NAME = "pw-assets";
        internal const string MAPS_DIR_NAME = "maps";
        internal const string MATERIALS_DIR_NAME = "materials";
        internal const string PROPS_DIR_NAME = "props";

        public string authorName;

        private string assetsDirectory;
        internal readonly HashSet<Identifier> definedIdentifiers;
        private readonly List<IAsset> changesToSave;
        private readonly List<IAsset> assets;

        /// <summary>
        /// Returns a read-only collection of all assets in this asset library.
        /// </summary>
        public IReadOnlyCollection<IAsset> Assets => this.assets;
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of all <see cref="Map"/>s in this Asset Library.
        /// </summary>
        public IEnumerable<Map> Maps => this.assets.OfType<Map>();
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of all <see cref="Prop"/>s in this Asset Library.
        /// </summary>
        public IEnumerable<Prop> Props => this.assets.OfType<Prop>();
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of all <see cref="Material"/>s in this Asset Library.
        /// </summary>
        public IEnumerable<Material> Materials => this.assets.OfType<Material>();
        /// <summary>
        /// Never null. Returns an array representing every asset that has been (possibly) modified and needs
        /// to be saved. Assets are sorted by identifier and are distinct.
        /// </summary>
        public IAsset[] Changes => this.changesToSave
            .GroupBy(asset => asset.Identifier)
            .Select(group => group.First())
            .ToArray();
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of all distinct author names in this Asset Library.
        /// </summary>
        public IEnumerable<string> ExistingAuthorNames => this.assets
            .Where(a => a.Identifier.HasValue)
            .Select(a => a.Identifier.Value.authorName)
            .Distinct();

        /// <summary>
        /// Retrieves assets of a specified type from this Asset Library.
        /// </summary>
        /// <typeparam name="T">The type of assets to retrieve.</typeparam>
        /// <returns>An enumerable collection of assets matching the specified type.</returns>
        public IEnumerable<T> GetAssetsOfType<T>() where T : IAsset
        {
            return this.assets.OfType<T>();
        }
        /// <summary>
        /// Retrieves assets of a specified <see cref="AssetType"/> from this Asset Library.
        /// </summary>
        /// <param name="type">The enum type of assets to retrieve.</param>
        /// <returns>An enumerable collection of assets matching the specified enum type.</returns>
        internal IEnumerable<IAsset> GetAssetsOfEnumType(AssetType type)
        {
            switch (type)
            {
                case AssetType.material:
                    return this.Materials;
                case AssetType.prop:
                    return this.Props;
                case AssetType.map:
                    return this.Maps;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        /// <summary>
        /// Retrieves a collection of assets that depend on the specified asset.
        /// </summary>
        /// <param name="asset">The asset to retrieve dependencies for.</param>
        /// <returns>An enumerable collection of assets that depend on the specified asset.</returns>
        internal IEnumerable<IAsset> GetAssetsThatDependOn(IAsset asset)
        {
            Identifier? assetIdentifier = asset.Identifier;
            return !assetIdentifier.HasValue
                ? Array.Empty<IAsset>()
                : this.assets.Where(a => a.GetAllDependedAssets.Contains(assetIdentifier.Value));
        }
        /// <summary>
        /// Resolves materials based on a given list of identifiers.
        /// </summary>
        /// <param name="identifiers">The list of identifiers to resolve materials for.</param>
        /// <returns>An enumerable collection of resolved materials.</returns>
        internal IEnumerable<Material> ResolveMaterials(IEnumerable<Identifier> identifiers)
        {
            return identifiers.Select(
                    identifier => this.assets.Where(a => a is Material)
                        .Cast<Material>()
                        .FirstOrDefault(m => m.Identifier.HasValue && m.Identifier.Value.Equals(identifier))
                )
                .Where(found => found != null)
                .ToList();
        }

        /// <summary>
        /// Create a new <see cref="AssetLibrary"/> with the root directory being at the root of the currently running application.
        /// </summary>
        public AssetLibrary()
        {
            this.assetsDirectory = GetDefaultAssetsDirectory();
            this.assets = new List<IAsset>();
            this.changesToSave = new List<IAsset>();
            this.definedIdentifiers = new HashSet<Identifier>();
        }
        /// <summary>
        /// Create a new <see cref="AssetLibrary"/> with a root directory set.
        /// </summary>
        /// <param name="assetsDirectory"></param>
        public AssetLibrary(string assetsDirectory)
        {
            this.assetsDirectory = assetsDirectory;
            this.assets = new List<IAsset>();
            this.changesToSave = new List<IAsset>();
            this.definedIdentifiers = new HashSet<Identifier>();
        }
        /// <summary>
        /// Returns the default assets directory.
        /// </summary>
        /// <returns>The default assets directory</returns>
        private static string GetDefaultAssetsDirectory()
        {
            string entryExecutable = Assembly.GetEntryAssembly()?.Location;
            if (string.IsNullOrEmpty(entryExecutable))
                return ASSETS_DIR_NAME; // relative to environment directory

            string entryDirectory = Path.GetDirectoryName(entryExecutable);

            return string.IsNullOrEmpty(entryDirectory)
                ? ASSETS_DIR_NAME
                : Path.Combine(entryDirectory, ASSETS_DIR_NAME);
        }

        /// <summary>
        /// Returns if the given directory is a valid Prego Wars assets directory.
        /// </summary>
        /// <param name="newDirectory"></param>
        /// <returns></returns>
        internal static bool IsValidAssetsDirectory(string newDirectory)
        {
            // ensure the directory is named 'assets'
            return newDirectory.EndsWith(ASSETS_DIR_NAME);
        }
        /// <summary>
        /// Sets the root directory for the asset library.<br />
        /// To prevent issues, ensure the input <see cref="IsValidAssetsDirectory"/> before using in this method.
        /// </summary>
        /// <param name="newDirectory">The new root directory.</param>
        public void SetAssetsDirectory(string newDirectory)
        {
            this.assetsDirectory = newDirectory;
        }
        /// <summary>
        /// Returns this asset library's base directory.
        /// </summary>
        /// <returns></returns>
        internal string GetAssetsDirectory()
        {
            return this.assetsDirectory ?? GetDefaultAssetsDirectory();
        }

        /// <summary>
        /// Adds an asset to this <see cref="AssetLibrary"/>.
        /// </summary>
        /// <param name="asset">The asset to add.</param>
        public void AddAsset(IAsset asset)
        {
            Identifier? assetIdentifier = asset.Identifier;
            if (assetIdentifier.HasValue)
            {
                this.assets.Add(asset);
                this.definedIdentifiers.Add(assetIdentifier.Value);
                this.changesToSave.Add(asset);
                return;
            }

            const string ERROR_MESSAGE =
                "Attempted to add an asset which has an invalid identifier (or no identifier at all).";

            MessageBox.Show(ERROR_MESSAGE, "While Committing Asset", MessageBoxButtons.OK, MessageBoxIcon.Error);
            throw new ArgumentException(ERROR_MESSAGE);
        }
        /// <summary>
        /// Signals a change to the AssetLibrary.
        /// </summary>
        /// <param name="asset"></param>
        public void AssetChanged(IAsset asset)
        {
            this.changesToSave.Add(asset);
        }
        /// <summary>
        /// Clears the list of changes made to the assets in the AssetLibrary.
        /// Use this when the changes have been saved and don't need to be tracked anymore.
        /// </summary>
        public void ClearChanges()
        {
            this.changesToSave.Clear();
        }
        /// <summary>
        /// Clear all assets from this <see cref="AssetLibrary"/>
        /// </summary>
        public void ClearAssets()
        {
            this.assets.Clear();
            this.definedIdentifiers.Clear();
        }
        /// <summary>
        /// Begins an asynchronous cleanup operation.
        /// </summary>
        public void Cleanup()
        {
            var display = new CleanupDisplay(this);
            display.ShowDialog();
        }
        /// <summary>
        /// Removes an asset from the AssetLibrary.
        /// </summary>
        /// <param name="assetToRemove">The asset to be removed.</param>
        public void RemoveAsset(IAsset assetToRemove)
        {
            if (assetToRemove.Identifier.HasValue)
            {
                RemoveByIdentifier(assetToRemove.Identifier.Value);
                return;
            }

            MessageBox.Show("Error: Cannot remove asset; has no identifier.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        /// <summary>
        /// Removes an identifier from the Asset Library and all associated assets with that identifier.
        /// </summary>
        /// <param name="identifier">The identifier to remove.</param>
        public void RemoveByIdentifier(Identifier identifier)
        {
            this.changesToSave.RemoveAll(a =>
            {
                Identifier? assetIdentifier = a.Identifier;
                return assetIdentifier.HasValue && assetIdentifier.Value.Equals(identifier);
            });
            this.definedIdentifiers.Remove(identifier);
            IAsset assetToRemove = this.assets.First(asset => asset.Identifier.Equals(identifier));
            string filePath = assetToRemove.JsonFilePath;

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                File.Delete(filePath);

            this.assets.RemoveAll(asset => asset.Identifier.Equals(identifier));
        }
        /// <summary>
        /// Returns if the given identifier is registered in this AssetLibrary.
        /// </summary>
        /// <param name="identifier"></param>
        public bool HasIdentifier(Identifier identifier)
        {
            return this.definedIdentifiers.Contains(identifier);
        }

        /// <summary>
        /// Refactors ALL assets referencing identifier <paramref name="from"/> to identifier <paramref name="to"/>.
        /// </summary>
        /// <param name="from">The source identifier.</param>
        /// <param name="to">The new identifier.</param>
        public void RefactorIdentifier(Identifier from, Identifier to)
        {
            this.definedIdentifiers.Remove(from);
            this.definedIdentifiers.Add(to);

            foreach (IAsset asset in this.assets)
            {
                if (asset.Identifier.Equals(from))
                {
                    asset.Identifier = to;
                    this.changesToSave.Add(asset);
                }

                if (asset.RefactorIdentifier(from, to))
                    this.changesToSave.Add(asset); // record change
            }
        }
        /// <summary>
        /// Refactors ALL assets under the author name <paramref name="oldName"/> to <paramref name="newName"/>.
        /// </summary>
        /// <param name="oldName">The source name to refactor.</param>
        /// <param name="newName">The new name to refactor to.</param>
        public void RefactorAuthorName(string oldName, string newName)
        {
            // records identifiers that changed, so they can be refactored as well
            var identifierChanges = new List<Tuple<Identifier, Identifier>>();

            foreach (IAsset asset in this.assets)
            {
                if (!asset.Identifier.HasValue)
                    continue;
                Identifier oldIdentifier = asset.Identifier.Value;
                if (asset.RefactorAuthorName(oldName, newName))
                {
                    this.changesToSave.Add(asset); // record change

                    Identifier newIdentifier = asset.Identifier.Value;
                    identifierChanges.Add(new Tuple<Identifier, Identifier>(oldIdentifier, newIdentifier));
                }
            }

            // apply identifier changes
            // ReSharper disable once UseDeconstruction
            foreach (Tuple<Identifier, Identifier> identifierChange in identifierChanges)
            {
                Identifier oldIdentifier = identifierChange.Item1;
                Identifier newIdentifier = identifierChange.Item2;
                RefactorIdentifier(oldIdentifier, newIdentifier);
            }
        }

        /// <summary>
        /// Attempts to load an asset library at the currently set root directory.
        /// </summary>
        /// <param name="errorList">A list of user-friendly strings representing the errors encountered during loading.</param>
        /// <param name="worker">The <see cref="BackgroundWorker"/> loading this asset library. Progress will be reported through this.</param>
        /// <returns>If the load succeeded.</returns>
        public bool Load(out List<string> errorList, BackgroundWorker worker = null)
        {
            errorList = new List<string>();

            // no assets directory set yet by user
            if (this.assetsDirectory == null)
            {
                errorList.Add("No assets directory was set.");
                return false;
            }

            // ensure it's a valid assets directory
            if (!IsValidAssetsDirectory(this.assetsDirectory))
            {
                errorList.Add(
                    $"Input directory '{this.assetsDirectory}' is not a valid Prego Wars 'assets' directory.");
                return false;
            }

            string mapsDirectory = Path.Combine(this.assetsDirectory, MAPS_DIR_NAME);
            string propsDirectory = Path.Combine(this.assetsDirectory, PROPS_DIR_NAME);
            string materialsDirectory = Path.Combine(this.assetsDirectory, MATERIALS_DIR_NAME);

            string[] mapFiles, propFiles, materialFiles;

            bool success = true;

            // Load maps
            if (Directory.Exists(mapsDirectory))
            {
                mapFiles = Directory.GetFiles(mapsDirectory, "*.json", SearchOption.AllDirectories);
            }
            else
            {
                Directory.CreateDirectory(mapsDirectory);
                mapFiles = Array.Empty<string>();
            }

            worker?.ReportProgress(4);

            // Load props
            if (Directory.Exists(propsDirectory))
            {
                propFiles = Directory.GetFiles(propsDirectory, "*.json", SearchOption.AllDirectories);
            }
            else
            {
                Directory.CreateDirectory(propsDirectory);
                propFiles = Array.Empty<string>();
            }

            worker?.ReportProgress(7);

            // Load materials
            // ReSharper disable once InvertIf
            if (Directory.Exists(materialsDirectory))
            {
                materialFiles = Directory.GetFiles(materialsDirectory, "*.json", SearchOption.AllDirectories);
            }
            else
            {
                Directory.CreateDirectory(materialsDirectory);
                materialFiles = Array.Empty<string>();
            }

            worker?.ReportProgress(10);

            int totalAssetsToLoad = mapFiles.Length + propFiles.Length + materialFiles.Length;
            int assetsLoaded = 0;

            foreach (string mapFile in mapFiles)
            {
                success &= LoadMap(mapFile, errorList);
                assetsLoaded++;
                worker?.ReportProgress(10 + (int) ((float) assetsLoaded / totalAssetsToLoad * 80F));
            }

            foreach (string propFile in propFiles)
            {
                success &= LoadProp(propFile, errorList);
                assetsLoaded++;
                worker?.ReportProgress(10 + (int) ((float) assetsLoaded / totalAssetsToLoad * 80F));
            }

            foreach (string materialFile in materialFiles)
            {
                success &= LoadMaterial(materialFile, errorList);
                assetsLoaded++;
                worker?.ReportProgress(10 + (int) ((float) assetsLoaded / totalAssetsToLoad * 80F));
            }

            // register all identifiers
            foreach (IAsset asset in this.assets)
            {
                Identifier? assetIdentifier = asset.Identifier;
                if (assetIdentifier.HasValue)
                    this.definedIdentifiers.Add(assetIdentifier.Value);
            }

            if (!success)
                return false;

            worker?.ReportProgress(90);
            int assetsToValidate = this.assets.Count;
            int assetsValidated = 0;

            // validate all assets
            foreach (IAsset asset in this.assets)
            {
                success &= asset.Validate(this, errorList);
                assetsValidated++;
                worker?.ReportProgress(90 + (int) ((float) assetsValidated / assetsToValidate * 10F));
            }

            worker?.ReportProgress(100);
            return success;
        }

        /// <summary>
        /// Loads a map from a JSON file and adds it to the list of assets.
        /// </summary>
        /// <param name="jsonFile">The path to the JSON file representing the map.</param>
        /// <param name="errorList">A list to store any loading errors encountered.</param>
        /// <returns>Returns true if the map is successfully loaded and added to the asset library, otherwise returns false.</returns>
        private bool LoadMap(string jsonFile, List<string> errorList)
        {
            try
            {
                string jsonContent = File.ReadAllText(jsonFile);
                JObject json = JObject.Parse(jsonContent, new JsonLoadSettings()
                {
                    CommentHandling = CommentHandling.Ignore,
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
                    LineInfoHandling = LineInfoHandling.Ignore
                });

                var map = json.ToObject<Map>();

                // load blocks since JSON serialization don't work
                if (json.TryGetValue("blocks", out JToken blocks))
                    if (blocks.Type == JTokenType.Array)
                        map.LoadBlocksFromJSON(blocks as JArray);

                map.JsonFilePath = jsonFile;
                map.ContainingLibrary = this;
                Debug.Assert(map.identifier != null, "map.identifier != null");
                this.definedIdentifiers.Add(map.identifier.Value);
                this.assets.Add(map);
                return true;
            }
            catch (JsonException exception)
            {
                errorList.Add(exception.Message);
                return false;
            }
        }
        /// <summary>
        /// Loads a prop from a JSON file and adds it to the list of assets.
        /// </summary>
        /// <param name="jsonFile">The path to the JSON file containing the property definition.</param>
        /// <param name="errorList">A list to store any loading errors encountered.</param>
        /// <returns>Returns true if the prop is successfully loaded and added to the asset library, otherwise returns false.</returns>
        private bool LoadProp(string jsonFile, List<string> errorList)
        {
            try
            {
                string jsonContent = File.ReadAllText(jsonFile);
                JObject json = JObject.Parse(jsonContent, new JsonLoadSettings()
                {
                    CommentHandling = CommentHandling.Ignore,
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
                    LineInfoHandling = LineInfoHandling.Ignore
                });

                var prop = json.ToObject<Prop>();
                prop.JsonFilePath = jsonFile;
                prop.ContainingLibrary = this;
                Debug.Assert(prop.identifier != null, "prop.identifier != null");
                this.definedIdentifiers.Add(prop.identifier.Value);
                this.assets.Add(prop);
                return true;
            }
            catch (JsonException exception)
            {
                errorList.Add(exception.Message);
                return false;
            }
        }
        /// <summary>
        /// Loads a material from a JSON file and adds it to the list of assets.
        /// </summary>
        /// <param name="jsonFile">The path to the JSON file containing the property definition.</param>
        /// <param name="errorList">A list to store any loading errors encountered.</param>
        /// <returns>Returns true if the prop is successfully loaded and added to the asset library, otherwise returns false.</returns>
        private bool LoadMaterial(string jsonFile, List<string> errorList)
        {
            try
            {
                string jsonContent = File.ReadAllText(jsonFile);
                JObject json = JObject.Parse(jsonContent, new JsonLoadSettings()
                {
                    CommentHandling = CommentHandling.Ignore,
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
                    LineInfoHandling = LineInfoHandling.Ignore
                });

                var material = json.ToObject<Material>();
                material.JsonFilePath = jsonFile;
                material.ContainingLibrary = this;
                Debug.Assert(material.identifier != null, "material.identifier != null");
                this.definedIdentifiers.Add(material.identifier.Value);
                this.assets.Add(material);
                return true;
            }
            catch (JsonException exception)
            {
                errorList.Add(exception.Message);
                return false;
            }
        }
    }
}