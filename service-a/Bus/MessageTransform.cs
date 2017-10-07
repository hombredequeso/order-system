using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CarrierPidgin.Lib;
using Hdq.Lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CarrierPidgin.ServiceA.Bus
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

        public static Either<DeserializeError, TransportMessage> DeserializeTransportMessage(
            string s,
            Dictionary<string, Type> domainMessageTypeLookup)
        {
            var deserializedA = Deserialize<TransportMessage>(s);
            return deserializedA.Match(
                e => deserializedA,
                tm =>
                {
                    var result = new TransportMessage(tm.Header,
                        tm.Messages.Select(m => new DomainMessage(
                            m.Header, 
                            ((JObject)m.Message).ToObject(domainMessageTypeLookup[m.Header.MessageType])
                            )).ToList());
                    return new Either<DeserializeError, TransportMessage>(result);
                });
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
    }
}