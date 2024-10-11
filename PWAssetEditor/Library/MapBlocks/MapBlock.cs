using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PWAssetEditor.Library.MapBlocks
{
    public abstract class MapBlock
    {
        [JsonIgnore]
        public abstract MapBlockType ImplementationType { get; }

        /// <summary>
        /// Refactors ALL assets referencing identifier <paramref name="from"/> to identifier <paramref name="to"/>.
        /// </summary>
        /// <param name="from">The source identifier.</param>
        /// <param name="to">The new identifier.</param>
        /// <returns>True if this asset was changed.</returns>
        internal abstract bool RefactorIdentifier(Identifier from, Identifier to);
        /// <summary>
        /// Returns a collection of asset identifiers which this block depends on to work properly.
        /// </summary>
        internal abstract IEnumerable<Identifier> GetAllDependedAssets { get; }
        /// <summary>
        /// Serialize this MapBlock to its JSON format.
        /// </summary>
        /// <returns></returns>
        public abstract JObject Serialize();

        [JsonProperty("type")]
        public MapBlockType type;
        [JsonProperty("transform")]
        public BlockTransform transform;
    }

    public enum MapBlockType
    {
        block,
        prop,
        entity,
        text
    }

    public static class MapBlockConverter
    {
        public static MapBlock ReadJson(JObject json)
        {
            string typeString = json["type"]?.ToString() ??
                                throw new JsonSerializationException(
                                    $"Map block at '{json.Path}' is missing block type.");

            if (!Enum.TryParse(typeString, out MapBlockType type))
                throw new JsonSerializationException($"Invalid MapBlockType at '{json.Path}': '{typeString}'");

            switch (type)
            {
                case MapBlockType.block:
                    return json.ToObject<MapBlockBlock>();
                case MapBlockType.prop:
                    return json.ToObject<MapBlockProp>();
                case MapBlockType.entity:
                    return json.ToObject<MapBlockEntity>();
                case MapBlockType.text:
                    return json.ToObject<MapBlockText>();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}