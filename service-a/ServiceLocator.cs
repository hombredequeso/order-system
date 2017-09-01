using System;

namespace CarrierPidgin.ServiceA
{

    public class MessageStreamLocation
    {
        public MessageStreamLocation(string scheme, string host, int port, string path)
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
    public static class ServiceLocator
    {
        public static MessageStreamLocation GetMessageStreamLocation(string streamName)
        {
            if (string.IsNullOrWhiteSpace(streamName))
                return null;
            if (streamName != "teststream")
                throw new Exception("Unknown stream name");
            return new MessageStreamLocation("http", "localhost", 8080, streamName);
        }
    }
}