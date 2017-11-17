using System.Threading.Tasks;

namespace CouchDB.Client
{
    public interface IStatelessHttpClient
    {
        Task<Response> SendRequestAsync(string absoluteUrl, RequestMethod requestMethod, Request request);
    }
}
