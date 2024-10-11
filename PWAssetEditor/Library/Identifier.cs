using System;
using Newtonsoft.Json;
using PWAssetEditor.Library.Asset;

namespace PWAssetEditor.Library
{
    /// <summary>
    /// An identifier in the format:
    /// <code>
    ///     name.assetType.assetName
    /// </code>
    /// </summary>
    [JsonConverter(typeof(IdentifierJsonConverter))]
    public readonly struct Identifier : IComparable<Identifier>, IEquatable<Identifier>
    {
        internal const string EXAMPLE = "name.assetType.assetName";
        
        public readonly string authorName;
        public readonly AssetType assetType;
        public readonly string assetName;

        /// <summary>
        /// An identifier in the format:
        /// <code>
        /// name.assetType.assetName
        /// </code>
        /// </summary>
        public Identifier(string authorName, AssetType assetType, string assetName)
        {
            this.authorName = authorName;
            this.assetType = assetType;
            this.assetName = assetName;
        }
        /// <summary>
        /// Tries to parse the given input string into an Identifier object.
        /// </summary>
        /// <param name="input">The input string in the format 'name.assetType.assetName'.</param>
        /// <param name="identifier">When this method returns, contains an Identifier object representing the parsed input if the conversion succeeded, or default if the conversion failed.</param>
        /// <returns>A boolean value that indicates whether the conversion succeeded or failed.</returns>
        public static bool TryParse(string input, out Identifier identifier)
        {
            string[] parts = input.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                identifier = default;
                return false;
            }

            string authorName = parts[0];
            if (!Enum.TryParse(parts[1], out AssetType assetType))
            {
                identifier = default;
                return false;
            }

            string assetName = parts[2];
            identifier = new Identifier(authorName, assetType, assetName);
            return true;
        }
        
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{this.authorName}.{this.assetType.ToString()}.{this.assetName}";
        }
        public string ToStringRoot()
        {
            return $"{this.authorName}.{this.assetType.ToString()}.";
        }
        public bool Equals(Identifier other)
        {
            return this.authorName == other.authorName && this.assetType == other.assetType && this.assetName == other.assetName;
        }
        public override bool Equals(object obj)
        {
            return obj is Identifier other && Equals(other);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (this.authorName != null ? this.authorName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) this.assetType;
                hashCode = (hashCode * 397) ^ (this.assetName != null ? this.assetName.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Returns if this identifier has all its fields filled out.
        /// </summary>
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(this.authorName) &&
            !string.IsNullOrWhiteSpace(this.assetName);
        public Identifier MutateAuthorName(string newAuthorName)
        {
            return new Identifier(newAuthorName, this.assetType, this.assetName);
        }
        
        public int CompareTo(Identifier other)
        {
            int authorNameComparison = string.Compare(this.authorName, other.authorName, StringComparison.Ordinal);
            if (authorNameComparison != 0)
                return authorNameComparison;
            int assetTypeComparison = this.assetType.CompareTo(other.assetType);
            return assetTypeComparison != 0 ?
                assetTypeComparison :
                string.Compare(this.assetName, other.assetName, StringComparison.Ordinal);
        }
    }
    
    public class IdentifierJsonConverter : JsonConverter<Identifier>
    {
        public override Identifier ReadJson(JsonReader reader, Type objectType, Identifier existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                throw new JsonReaderException($"Identifier at {reader.Path} was null and could not be parsed.");

            string input = reader.Value.ToString();
            if (Identifier.TryParse(input, out Identifier identifier))
                return identifier;

            throw new JsonReaderException($"Identifier at {reader.Path} ('{input}') could not be parsed. Use the format '{Identifier.EXAMPLE}'");
        }
        
        public override void WriteJson(JsonWriter writer, Identifier value, JsonSerializer serializer)
        {
            string output = value.ToString();
            writer.WriteValue(output);
        }
    }
}