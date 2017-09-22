using Newtonsoft.Json.Linq;

namespace CouchDB.Client.Tests
{
    public static class AssertHelper
    {
        public static bool StringIsJsonObject(string value, JObject json)
        {
            if (value == null && json == null)
                return true;

            if (value == null || json == null)
                return false;

            var valueJson = JObject.Parse(value);

            return JToken.DeepEquals(json, valueJson);
        }

        public static bool StringIsJsonObject(string value, object json)
            => StringIsJsonObject(value, JObject.FromObject(json));
    }
}
