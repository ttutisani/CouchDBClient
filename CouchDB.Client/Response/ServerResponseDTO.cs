namespace CouchDB.Client
{
    internal sealed class ServerResponseDTO
    {
        public bool OK { get; set; }

        public string Error { get; set; }

        public string Reason { get; set; }
    }
}
