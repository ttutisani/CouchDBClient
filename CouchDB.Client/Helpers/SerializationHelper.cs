using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CouchDB.Client
{
    internal static class SerializationHelper
    {
        internal static string GetStringOrDefault(JObject source, string propertyName)
        {
            JToken propertyToken;
            if (source.TryGetValue(propertyName, out propertyToken) && propertyToken.Type == JTokenType.String)
            {
                return propertyToken.Value<string>();
            }

            return null;
        }

        internal static int GetIntOrDefault(JObject source, string propertyName)
        {
            JToken propertyToken;
            if (source.TryGetValue(propertyName, out propertyToken) && propertyToken.Type == JTokenType.Integer)
            {
                return propertyToken.Value<int>();
            }

            return 0;
        }

        internal static JArray GetArrayOrEmpty(JObject source, string propertyName)
        {
            var arrayValue = source[propertyName] as JArray;
            return arrayValue ?? new JArray();
        }

        internal static List<JObject> JArrayMap(JArray array)
        {
            var result = new List<JObject>();

            for (int index = 0; index < array.Count; index++)
            {
                var currentItem = array[index] as JObject;
                if (currentItem == null)
                    throw new InvalidOperationException($"Cannot retrieve list of objects as item at index '{index}' is null or not '{nameof(JObject)}'.");

                result.Add(currentItem);
            }

            return result;
        }

        internal static JObject GetObjectOrDefault(JObject source, string propertyName)
        {
            var propValue = source[propertyName] as JObject;
            return propValue;
        }
    }
}
