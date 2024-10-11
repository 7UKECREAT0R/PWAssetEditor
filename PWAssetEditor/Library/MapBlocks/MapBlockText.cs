using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PWAssetEditor.Library.MapBlocks
{
    public class MapBlockText : MapBlock
    {
        [JsonIgnore]
        public override MapBlockType ImplementationType => MapBlockType.text;
        internal override bool RefactorIdentifier(Identifier from, Identifier to) => false;
        internal override IEnumerable<Identifier> GetAllDependedAssets => Array.Empty<Identifier>();

        public override JObject Serialize()
        {
            return new JObject
            {
                ["type"] = MapBlockType.text.ToString(),
                ["transform"] = this.transform.Serialize(),
                ["text"] = this.textInfo.Serialize()
            };
        }
        
        [JsonProperty("text")]
        public MapBlockTextInfo textInfo;
    }
    /// <summary>
    /// Contains information under the 'text' property in a map block.
    /// </summary>
    public struct MapBlockTextInfo
    {
        /// <summary>
        /// Serializes the current MapBlockTextInfo object to a JObject.
        /// </summary>
        /// <returns>A JObject containing the serialized MapBlockTextInfo object.</returns>
        public JObject Serialize()
        {
            var part = new JObject
            {
                ["content"] = this.content,
                ["color"] = this.color.AsJSON(),
            };

            if (this.wrap.HasValue)
            {
                part["wrap"] = this.wrap.Value;
                return part;
            }

            part["wrap"] = null;
            return part;
        }
        
        /// <summary>
        /// Returns if this text should wrap based on <see cref="wrap"/>
        /// </summary>
        [JsonIgnore]
        public bool ShouldWrapText => this.wrap.HasValue;
        
        /// <summary>
        /// The content inside the text, displayed to the viewer.
        /// </summary>
        [JsonProperty("content")]
        public string content;
        
        /// <summary>
        /// If specified, the distance at which the text should wrap.
        /// </summary>
        [JsonProperty("wrap", NullValueHandling = NullValueHandling.Include)]
        public float? wrap;
        
        /// <summary>
        /// The color of the text.
        /// </summary>
        [JsonProperty("color")]
        public MaterialColor color;
    }
}