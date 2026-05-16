using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;

namespace DigitalRiseModel.Utility
{
	internal static class JsonExtensions
	{
		public static readonly JsonSerializerSettings DefaultOptions;

		public class ColorConverter : JsonConverter<Color>
		{
			public static readonly ColorConverter Instance = new ColorConverter();

			private ColorConverter()
			{
			}

			public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				var s = reader.Value.ToString();
				var result = ColorStorage.FromName(s);

				if (result == null)
				{
					throw new Exception($"Could not parse color {s}");
				}

				return result.Value;
			}

			public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
			{
				var str = value.GetColorName();

				if (str == null)
				{
					str = value.ToHexString();
				}

				writer.WriteValue(str);
			}
		}

		static JsonExtensions()
		{
			DefaultOptions = new JsonSerializerSettings
			{
				Culture = CultureInfo.InvariantCulture,
				Formatting = Formatting.Indented,
				TypeNameHandling = TypeNameHandling.Auto,
				DefaultValueHandling = DefaultValueHandling.Ignore,
			};

			DefaultOptions.Converters.Add(ColorConverter.Instance);
		}

		public static void SerializeToFile<T>(string path, T data)
		{
			var s = JsonConvert.SerializeObject(data, DefaultOptions);
			File.WriteAllText(path, s);
		}

		public static T DeserializeFromString<T>(string data)
		{
			return JsonConvert.DeserializeObject<T>(data, DefaultOptions);
		}
	}
}
