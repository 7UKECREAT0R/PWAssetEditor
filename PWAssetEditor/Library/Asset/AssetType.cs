namespace PWAssetEditor.Library.Asset
{
    public enum AssetType
    {
        /// <summary>
        /// A material that can be applied to map blocks and props. <see cref="Material"/>
        /// </summary>
        material,
        /// <summary>
        /// A prop that can be placed in the map for decoration. <see cref="Prop"/>
        /// </summary>
        prop,
        /// <summary>
        /// A map made up of blocks, props, entities, and text. <see cref="Map"/>
        /// </summary>
        map
    }
}