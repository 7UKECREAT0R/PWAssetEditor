using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace PWAssetEditor.Library.MapBlocks
{
    public class MapBlockBlock : MapBlock
    {
        [JsonIgnore]
        public override MapBlockType ImplementationType => MapBlockType.block;
        internal override bool RefactorIdentifier(Identifier from, Identifier to)
        {
            if (this.material.Equals(from))
            {
                this.material = to;
                return true;
            }

            return false;
        }
        internal override IEnumerable<Identifier> GetAllDependedAssets => new[] {this.material};
        public override JObject Serialize()
        {
            var json = new JObject
            {
                ["type"] = MapBlockType.block.ToString(),
                ["transform"] = this.transform.Serialize(),
                ["shape"] = this.shape.ToString(),
                ["material"] = this.material.ToString()
            };
            if (this.effects != null && this.effects.Count > 0)
                json["effects"] = this.effects;
            return json;
        }

        /// <summary>
        /// The name of this block's shape; most commonly "cube."
        /// </summary>
        [JsonProperty("shape")]
        public ShapeType shape;

        /// <summary>
        /// The material applied to the block.
        /// </summary>
        [JsonProperty("material")]
        public Identifier material;

        /// <summary>
        /// The effects (if any) applied to the block.
        /// This doesn't need to be parsed, only the data needs to be preserved.
        /// </summary>
        [JsonProperty("effects", Required = Required.DisallowNull)]
        public JArray effects;
    }

    /// <summary>
    /// The type of shape.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ShapeType
    {
        sphere,
        capsule,
        cylinder,
        cube,
        plane,
        quad
    }
}