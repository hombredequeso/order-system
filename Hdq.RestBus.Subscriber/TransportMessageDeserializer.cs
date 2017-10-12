using System;
using System.Collections.Generic;
using System.Linq;
using Hdq.Lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hdq.RestBus.Subscriber
{
    public class DeserializeError
    {
        public DeserializeError(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }


    public static class TransportMessageDeserializer
    {
        public static Either<DeserializeError, TransportMessage> Deserialize(
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

        private static Either<DeserializeError, T> Deserialize<T>(Func<T> f)
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

        private static Either<DeserializeError, T> Deserialize<T>(string s)
        {
            return Deserialize(() => JsonConvert.DeserializeObject<T>(s));
        }
    }
}