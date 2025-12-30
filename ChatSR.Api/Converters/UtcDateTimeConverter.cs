using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatSR.Api.Converters;

public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var date = reader.GetDateTime();

		return date.Kind == DateTimeKind.Utc
			? date
			: DateTime.SpecifyKind(date, DateTimeKind.Utc);
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		var utc = value.Kind == DateTimeKind.Utc
			? value
			: value.ToUniversalTime();

		writer.WriteStringValue(utc.ToString("O")); // ISO 8601 + Z
	}

}
