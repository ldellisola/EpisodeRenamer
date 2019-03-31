﻿using Newtonsoft.Json;
using System;

namespace TVDBSharp.Converters
{
    /// <summary>
    /// Converts a TimeSpan to DateTime.
    /// </summary>
    internal class TimeSpanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = (string)reader.Value;
            if (value != null && DateTime.TryParse(value, out DateTime result))
            {
                return result.TimeOfDay;
            }
            else return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}