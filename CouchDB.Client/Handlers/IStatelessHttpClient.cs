using System.Threading.Tasks;

namespace CouchDB.Client
{
    public interface IStatelessHttpClient
    {
        //Task<HttpResponseMessage> GetAsync(string relativeUrl);
        //Task<HttpResponseMessage> PutAsync(string relativeUrl, HttpContent httpContent);
        //Task<HttpResponseMessage> DeleteAsync(string relativeUrl);
        //Task<HttpResponseMessage> PostAsync(string relativeUrl, HttpContent httpContent);

        Task<Response> SendRequestAsync(string absoluteUrl, RequestMethod requestMethod, Request request);
    }
}
