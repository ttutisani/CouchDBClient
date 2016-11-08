namespace CouchDB.ClientDemo
{
    internal static class SerializationHelper
    {
        public static string Serialize(object any)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(any);
        }
    }
}
