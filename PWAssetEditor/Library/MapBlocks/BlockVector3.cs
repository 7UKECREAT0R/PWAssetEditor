using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PWAssetEditor.Library.MapBlocks
{
    [JsonConverter(typeof(BlockVector3Converter))]
    public struct BlockVector3
    {
        /// <summary>
        /// A position component of this vector. Y is up.
        /// </summary>
        public float x, y, z;

        public BlockVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public BlockVector3 Normalized
        {
            get
            {
                float length = Length();
                return length > 0
                    ? new BlockVector3(this.x / length, this.y / length, this.z / length)
                    : new BlockVector3(this.x, this.y, this.z);
            }
        }

        /// <summary>
        /// Returns this <see cref="BlockVector3"/> as a JSON array of its three parts.
        /// </summary>
        /// <returns></returns>
        public JArray AsJSON()
        {
            return new JArray(new[] {this.x, this.y, this.z});
        }

        public float Length()
        {
            return (float) Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
        }
        public void Normalize()
        {
            float length = Length();

            if (!(length > 0))
                return;

            this.x /= length;
            this.y /= length;
            this.z /= length;
        }

        public override string ToString()
        {
            return $"({this.x}, {this.y}, {this.z})";
        }
    }

    public class BlockVector3Converter : JsonConverter<BlockVector3>
    {
        public override void WriteJson(JsonWriter writer, BlockVector3 value, JsonSerializer serializer)
        {
            const float TOLERANCE = 0.00001F;
            if (Math.Abs(value.x - value.y) < TOLERANCE &&
                Math.Abs(value.y - value.z) < TOLERANCE)
            {
                // {"field": 1} is equivalent to {"field": [1, 1, 1]}
                writer.WriteValue(value.x);
                return;
            }

            writer.WriteStartArray();
            writer.WriteValue(value.x);
            writer.WriteValue(value.y);
            writer.WriteValue(value.z);
            writer.WriteEndArray();
        }

        public override BlockVector3 ReadJson(JsonReader reader, Type objectType, BlockVector3 existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var result = new BlockVector3();

            switch (reader.TokenType)
            {
                case JsonToken.StartArray:
                {
                    int i = 0;
                    while (reader.Read() && reader.TokenType != JsonToken.EndArray && i < 3)
                    {
                        if (reader.TokenType != JsonToken.Float &&
                            reader.TokenType != JsonToken.Integer)
                            continue;

                        object value = reader.Value;
                        float number = 0F;
                        if (value != null)
                            number = Convert.ToSingle(value);

                        switch (i)
                        {
                            case 0:
                                result.x = number;
                                break;
                            case 1:
                                result.y = number;
                                break;
                            case 2:
                                result.z = number;
                                break;
                        }

                        i++;
                    }

                    // If only one number was provided, assume all three fields are the same in uniform.
                    if (i == 1)
                    {
                        result.y = result.x;
                        result.z = result.x;
                    }

                    break;
                }
                case JsonToken.Float:
                case JsonToken.Integer:
                {
                    object value = reader.Value;
                    float number = 0F;
                    if (value != null)
                        number = Convert.ToSingle(value);

                    // If only one number was provided, assume all three fields are the same
                    result = new BlockVector3(number, number, number);
                    break;
                }
                case JsonToken.None:
                case JsonToken.StartObject:
                case JsonToken.StartConstructor:
                case JsonToken.PropertyName:
                case JsonToken.Comment:
                case JsonToken.Raw:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Null:
                case JsonToken.Undefined:
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                case JsonToken.EndConstructor:
                case JsonToken.Date:
                case JsonToken.Bytes:
                default:
                    throw new JsonReaderException(
                        $"Unexpected token type in Vector located at '{reader.Path}': {reader.TokenType}");
            }

            return result;
        }
    }
}