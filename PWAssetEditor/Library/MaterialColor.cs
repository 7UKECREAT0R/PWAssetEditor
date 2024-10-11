using System;
using System.Diagnostics.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PWAssetEditor.Library
{
    [JsonConverter(typeof(MaterialColorJsonConverter))]
    public readonly struct MaterialColor
    {
        public static readonly MaterialColor WHITE = new MaterialColor(1F, 1F, 1F);
        public static readonly MaterialColor GRAY = new MaterialColor(0.5F, 0.5F, 0.5F);
        public static readonly MaterialColor BLACK = new MaterialColor(0F, 0F, 0F);

        public static readonly MaterialColor RED = new MaterialColor(1F, 0F, 0F);
        public static readonly MaterialColor YELLOW = new MaterialColor(1F, 1F, 0F);
        public static readonly MaterialColor GREEN = new MaterialColor(0F, 1F, 0F);
        public static readonly MaterialColor CYAN = new MaterialColor(0F, 1F, 1F);
        public static readonly MaterialColor BLUE = new MaterialColor(0F, 0F, 1F);
        public static readonly MaterialColor MAGENTA = new MaterialColor(1F, 0F, 1F);

        /// <summary>
        /// Color component from 0.0-1.0
        /// </summary>
        public readonly float red, green, blue;

        public MaterialColor(float red, float green, float blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
        }
        /// <summary>
        /// Construct a material color from a JSON array containing three numbers.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private static MaterialColor FromArray(JArray array)
        {
            if (array.Count < 3)
                throw new JsonReaderException(
                    $"Color property at {array.Path} contained only {array.Count} color components (needs 3).");
            if (array.Count > 3)
                throw new JsonReaderException(
                    $"Color property at {array.Path} contained {array.Count} color components (needs only 3).");

            float red = (float) array[0].Value<double>();
            float green = (float) array[1].Value<double>();
            float blue = (float) array[2].Value<double>();

            return new MaterialColor(
                Clamp01(red),
                Clamp01(green),
                Clamp01(blue)
            );
        }
        public static MaterialColor FromToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                    return FromArray((JArray) token);
                case JTokenType.Float:
                {
                    float n = Clamp01(token.Value<float>());
                    return new MaterialColor(n, n, n);
                }
                case JTokenType.None:
                case JTokenType.Object:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Integer:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Date:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                default:
                    throw new JsonReaderException(
                        $"Color property at {token.Path} was invalid. (supports single number, or array of numbers representing R, G, and B.)");
            }
        }

        /// <summary>
        /// Converts a WinForms color to a MaterialColor.
        /// </summary>
        /// <param name="color">The WinForms color to convert.</param>
        /// <returns>The converted MaterialColor.</returns>
        public static MaterialColor FromWinFormsColor(System.Drawing.Color color)
        {
            return new MaterialColor(color.R / 255F, color.G / 255F, color.B / 255F);
        }
        /// <summary>
        /// Converts this MaterialColor to a WinForms color.
        /// </summary>
        /// <param name="alpha">The alpha value to give the WinForms color.</param>
        /// <returns>The converted WinForms Color.</returns>
        public System.Drawing.Color ToWinFormsColor(float alpha = 1F)
        {
            byte r = (byte) (this.red * 255F);
            byte g = (byte) (this.green * 255F);
            byte b = (byte) (this.blue * 255F);
            byte a = (byte) (alpha * 255F);
            return System.Drawing.Color.FromArgb(a, r, g, b);
        }

        public override string ToString()
        {
            return $"[{this.red}, {this.green}, {this.blue}]";
        }

        [Pure]
        private static float Clamp01(float number)
        {
            if (number > 1F)
                return 1F;
            return number < 0F ? 0F : number;
        }

        /// <summary>
        /// Converts the MaterialColor object to a JSON array.
        /// </summary>
        /// <returns>A JSON array containing the red, green, and blue values of the MaterialColor object.</returns>
        public JArray AsJSON()
        {
            return new JArray(new[] {this.red, this.green, this.blue});
        }
    }

    public class MaterialColorJsonConverter : JsonConverter<MaterialColor>
    {
        public override MaterialColor ReadJson(JsonReader reader, Type objectType, MaterialColor existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            return MaterialColor.FromToken(token);
        }

        public override void WriteJson(JsonWriter writer, MaterialColor value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.red);
            writer.WriteValue(value.green);
            writer.WriteValue(value.blue);
            writer.WriteEndArray();
        }
    }
}