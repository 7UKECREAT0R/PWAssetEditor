using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PWAssetEditor.Library.MapBlocks
{
    public struct BlockTransform
    {
        /// <summary>
        /// The position of the object's origin.
        /// </summary>
        [JsonProperty("position")]
        public BlockVector3 position;
        
        /// <summary>
        /// The Euler rotation of the object around each axis.
        /// </summary>
        [JsonProperty("rotation")]
        public BlockVector3 rotation;
        
        /// <summary>
        /// The scale of the object. See <see cref="IsScaleUniform" /> for information on uniformity.
        /// </summary>
        [JsonProperty("scale")]
        public BlockVector3 scale;

        /// <summary>
        /// Serializes this transform to a JSON
        /// </summary>
        /// <returns></returns>
        public JObject Serialize()
        {
            var part = new JObject
            {
                ["position"] = this.position.AsJSON(),
                ["rotation"] = this.rotation.AsJSON()
            };

            if (this.IsScaleUniform)
                part["scale"] = this.scale.x;
            else
                part["scale"] = this.scale.AsJSON();

            return part;
        }
        
        /// <summary>
        /// Returns if this transform's scale is uniform (as in, not stretched).
        /// </summary>
        public bool IsScaleUniform
        {
            get
            {
                const float TOLERANCE = 0.00001F;
                return Math.Abs(this.scale.x - this.scale.y) < TOLERANCE &&
                       Math.Abs(this.scale.y - this.scale.z) < TOLERANCE;
            }
        }
    }
}