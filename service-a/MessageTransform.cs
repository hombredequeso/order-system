using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Hdq.Lib;
using Newtonsoft.Json;

namespace CarrierPidgin.ServiceA
{
    public static class MessageTransform
    {
        public static Either<DeserializeError, T> Deserialize<T>(Func<T> f)
        {
            try
            {
                var m = f();
                return new Either<DeserializeError, T>(m);
            }
            catch (JsonException e)
            {
                return new Either<DeserializeError, T>(new DeserializeError(e));
            }
        }

        public static Either<DeserializeError, T> Deserialize<T>(string s)
        {
            return Deserialize(() => JsonConvert.DeserializeObject<T>(s));
        }

        public static Either<DeserializeError, object> Deserialize(string s, Type t)
        {
            return Deserialize(() => JsonConvert.DeserializeObject(s, t));
        }

        public static async Task<Either<HttpError, string>> GetContent(HttpResponseMessage m)
        {
            if (!m.IsSuccessStatusCode)
                return new Either<HttpError, string>(new HttpError(m.StatusCode));

            using (var stream = await m.Content.ReadAsStreamAsync())
            {
                using (var sr = new StreamReader(stream))
                {
                    var s = sr.ReadToEnd();
                    return new Either<HttpError, string>(s);
                }
            }
        }

        public class DeserializeError
        {
            public DeserializeError(Exception exception)
            {
                Exception = exception;
            }

            public Exception Exception { get; }
        }

        public class HttpError
        {
            public HttpError(HttpStatusCode httpErrorCode)
            {
                HttpErrorCode = httpErrorCode;
            }

            public HttpStatusCode HttpErrorCode { get; }
        }
    }
}