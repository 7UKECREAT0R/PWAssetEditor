using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PWAssetEditor.Library.MapBlocks
{
    public class MapBlockEntity : MapBlock
    {
        [JsonIgnore]
        public override MapBlockType ImplementationType => MapBlockType.entity;
        internal override bool RefactorIdentifier(Identifier from, Identifier to) => false;
        internal override IEnumerable<Identifier> GetAllDependedAssets => Array.Empty<Identifier>();
        public override JObject Serialize()
        {
            return new JObject
            {
                ["type"] = MapBlockType.entity.ToString(),
                ["transform"] = this.transform.Serialize(),
                ["entity"] = this.entityName
            };
        }
        
        /// <summary>
        /// The name of the entity that should be placed.
        /// </summary>
        [JsonProperty("entity")]
        public string entityName;
    }
}