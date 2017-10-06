using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Hdq.Lib
{
    public class HttpError
    {
        public HttpError(HttpStatusCode httpErrorCode)
        {
            HttpErrorCode = httpErrorCode;
        }

        public HttpStatusCode HttpErrorCode { get; }
    }

    public interface IHttpService : IDisposable
    {
        Task<Either<HttpError, string>> GetBodyFrom(string path, CancellationToken ct);
    }

    public class TestHttpService : IHttpService
    {
        private List<Either<HttpError, string>> _orderedResponses;
        private int _nextResponse = 0;
        public TestHttpService(IEnumerable<Either<HttpError, string>> orderedResponses)
        {
            _orderedResponses = orderedResponses.ToList();
        }

        public void Dispose()
        {
        }

        public Task<Either<HttpError, string>> GetBodyFrom(string path, CancellationToken ct)
        {
            var result = Task.FromResult(_orderedResponses[_nextResponse++]);
            return result;
        }
    }

    public class HttpService : IHttpService
    {
        private HttpClient HttpClient { get; }

        public HttpService(Uri baseAddress)
        {
            HttpClient = new HttpClient {BaseAddress = baseAddress};
            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                HttpClient?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<Either<HttpError, string>> GetBodyFrom(string url, CancellationToken ct)
        {
            using (HttpResponseMessage response = await HttpClient.GetAsync(url, ct))
            {
                if (!response.IsSuccessStatusCode)
                    return new Either<HttpError, string>(new HttpError(response.StatusCode));
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    using (var sr = new StreamReader(stream))
                    {
                        var s = sr.ReadToEnd();
                        return new Either<HttpError, string>(s);
                    }
                }
            }
        }
    }
}