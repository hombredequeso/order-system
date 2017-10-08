using System;

namespace Hdq.RestBus.Receiver
{
    public class MessageChannelLocation
    {
        public MessageChannelLocation(string scheme, string host, int port, string path)
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