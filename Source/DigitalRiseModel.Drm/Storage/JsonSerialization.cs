using Microsoft.Xna.Framework;
using System.Globalization;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DigitalRiseModel.Storage
{
	internal static class JsonSerialization
	{
		private static float ParseFloat(string s)
		{
			return float.Parse(s.Trim(), CultureInfo.InvariantCulture);
		}

		private static int ParseInt(string s)
		{
			return int.Parse(s.Trim());
		}

		private static byte ParseByte(string s)
		{
			return (byte)ParseInt(s);
		}

		internal class PointConverter : JsonConverter<Point>
		{
			public static readonly PointConverter Instance = new PointConverter();

			private PointConverter()
			{
			}

			public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				string s = reader.Value.ToString();

				var p = s.Split(',');
				var result = new Point(ParseInt(p[0]), ParseInt(p[1]));

				return result;
			}

			public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer)
			{
				var str = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", value.X, value.Y);
				writer.WriteValue(str);
			}
		}

		internal class ColorConverter : JsonConverter<Color>
		{
			public static readonly ColorConverter Instance = new ColorConverter();

			private ColorConverter()
			{
			}

			public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				var s = reader.Value.ToString();

				var p = s.Split(',');
				var result = new Color(ParseByte(p[0]), ParseByte(p[1]), ParseByte(p[2]), ParseByte(p[3]));

				return result;
			}

			public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
			{
				var str = string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", (int)value.R, (int)value.G, (int)value.B, (int)value.A);
				writer.WriteValue(str);
			}
		}

		internal class QuaternionConverter : JsonConverter<Quaternion>
		{
			public static readonly QuaternionConverter Instance = new QuaternionConverter();

			private QuaternionConverter()
			{

			}

			public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				var s = reader.Value.ToString();

				var p = s.Split(',');
				var result = new Quaternion(ParseFloat(p[0]), ParseFloat(p[1]), ParseFloat(p[2]), ParseFloat(p[3]));

				return result;
			}

			public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer options)
			{
				var str = string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", value.X, value.Y, value.Z, value.W);
				writer.WriteValue(str);
			}
		}

		internal class MatrixConverter : JsonConverter<Matrix>
		{
			public static readonly MatrixConverter Instance = new MatrixConverter();

			private MatrixConverter()
			{
			}

			public override Matrix ReadJson(JsonReader reader, Type objectType, Matrix existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				var s = reader.Value.ToString();

				var p = s.Split(',');

				var result = new Matrix(
					ParseFloat(p[0]), ParseFloat(p[1]), ParseFloat(p[2]), ParseFloat(p[3]),
					ParseFloat(p[4]), ParseFloat(p[5]), ParseFloat(p[6]), ParseFloat(p[7]),
					ParseFloat(p[8]), ParseFloat(p[9]), ParseFloat(p[10]), ParseFloat(p[11]),
					ParseFloat(p[12]), ParseFloat(p[13]), ParseFloat(p[14]), ParseFloat(p[15]));

				return result;
			}

			public override void WriteJson(JsonWriter writer, Matrix value, JsonSerializer serializer)
			{
				var str = string.Format(CultureInfo.InvariantCulture,
					"{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}",
					value.M11, value.M12, value.M13, value.M14,
				value.M21, value.M22, value.M23, value.M24,
				value.M31, value.M32, value.M33, value.M34,
					value.M41, value.M42, value.M43, value.M44);
				writer.WriteValue(str);
			}
		}

		public static JsonSerializerSettings CreateOptions(bool indented = true)
		{
			var result = new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				DefaultValueHandling = DefaultValueHandling.Ignore
			};

			result.Converters.Add(new StringEnumConverter());
			result.Converters.Add(PointConverter.Instance);
			result.Converters.Add(ColorConverter.Instance);
			result.Converters.Add(QuaternionConverter.Instance);
			result.Converters.Add(MatrixConverter.Instance);

			return result;
		}

		public static string SerializeToString<T>(T data, bool indented = true)
		{
			var options = CreateOptions(indented);
			return JsonConvert.SerializeObject(data, options);
		}

		public static void SerializeToFile<T>(string path, T data)
		{
			var s = SerializeToString(data);
			File.WriteAllText(path, s);
		}

		public static T DeserializeFromString<T>(string data)
		{
			var options = CreateOptions();
			return JsonConvert.DeserializeObject<T>(data, options);
		}
	}
}
