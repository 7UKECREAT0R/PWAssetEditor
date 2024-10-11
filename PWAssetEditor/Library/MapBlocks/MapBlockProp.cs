using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PWAssetEditor.Library.MapBlocks
{
    public class MapBlockProp : MapBlock
    {
        [JsonIgnore]
        public override MapBlockType ImplementationType => MapBlockType.prop;
        internal override bool RefactorIdentifier(Identifier from, Identifier to)
        {
            if (this.prop.Equals(from))
            {
                this.prop = to;
                return true;
            }

            return false;
        }
        internal override IEnumerable<Identifier> GetAllDependedAssets => new[] { this.prop };

        public override JObject Serialize()
        {
            return new JObject
            {
                ["type"] = MapBlockType.prop.ToString(),
                ["transform"] = this.transform.Serialize(),
                ["prop"] = this.prop.ToString()
            };
        }

        /// <summary>
        /// The identifier of the prop to be placed.
        /// </summary>
        [JsonProperty("prop")]
        public Identifier prop;
    }
}