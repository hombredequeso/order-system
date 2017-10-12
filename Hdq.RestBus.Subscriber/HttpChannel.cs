using System;

namespace Hdq.RestBus.Subscriber
{
    public class HttpChannelBase
    {
        public HttpChannelBase(
            string scheme, 
            string host, 
            int port)
        {
            Scheme = scheme;
            Host = host;
            Port = port;
        }

        public string Scheme { get; }
        public string Host { get; }
        public Int32 Port { get; }
    }

    public class HttpChannel
    {
        public HttpChannel(HttpChannelBase httpChannelBase, string path)
        {
            HttpChannelBase = httpChannelBase;
            Path = path;
        }

        public HttpChannelBase HttpChannelBase;
        public string Path { get; }
    }
}