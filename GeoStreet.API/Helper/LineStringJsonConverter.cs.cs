using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace GeoStreet.API.Helper
{
    public class LineStringJsonConverter : JsonConverter<LineString>
    {
        public override LineString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var wkt = reader.GetString(); // Expect WKT string as input
                var wktReader = new WKTReader();
                return (LineString)wktReader.Read(wkt);
            }
            throw new JsonException("Invalid geometry format.");
        }

        public override void Write(Utf8JsonWriter writer, LineString value, JsonSerializerOptions options)
        {
            var wktWriter = new WKTWriter();
            writer.WriteStringValue(wktWriter.Write(value)); // Serialize as WKT string
        }
    }
}
