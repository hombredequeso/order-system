using System;

namespace Hdq.RestBus.Receiver
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
        public HttpChannel(string scheme, string host, int port, string path)
        {
            Scheme = scheme;
            Host = host;
            Port = port;
            Path = path;
        }

        public string Scheme { get; }
        public string Host { get; }
        public Int32 Port { get; }
        public string Path { get; }
    }
}