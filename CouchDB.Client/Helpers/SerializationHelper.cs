using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CouchDB.Client
{
    internal static class SerializationHelper
    {
        /// <summary>
        /// Get string of default (null).
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        ///// <exception cref="NoException"></exception>
        internal static string GetStringOrDefault(JObject source, string propertyName)
        {
            JToken propertyToken;
            if (source.TryGetValue(propertyName, out propertyToken) && propertyToken.Type == JTokenType.String)
            {
                return propertyToken.Value<string>();
            }

            return null;
        }

        /// <summary>
        /// Get int or default (0).
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        ///// <exception cref="NoException"></exception>
        internal static int GetIntOrDefault(JObject source, string propertyName)
        {
            JToken propertyToken;
            if (source.TryGetValue(propertyName, out propertyToken) && propertyToken.Type == JTokenType.Integer)
            {
                return propertyToken.Value<int>();
            }

            return 0;
        }

        /// <summary>
        /// Get array or return an empty array.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        ///// <exception cref="NoException"></exception>
        internal static JArray GetArrayOrEmpty(JObject source, string propertyName)
        {
            var arrayValue = source[propertyName] as JArray;
            return arrayValue ?? new JArray();
        }

        /// <summary>
        /// Convert <see cref="JArray"/> into a list of <see cref="JObject"/>.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"><see cref="JArray"/> is not a list of objects (one of the items cannot be converted into <see cref="JObject"/>).</exception>
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

        /// <summary>
        /// Get objects or default (null).
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        ///// <exception cref="NoException"></exception>
        internal static JObject GetObjectOrDefault(JObject source, string propertyName)
        {
            var propValue = source[propertyName] as JObject;
            return propValue;
        }
    }
}
