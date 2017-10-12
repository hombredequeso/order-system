using System;

namespace Hdq.RestBus.Subscriber
{
    // https://github.com/hombredequeso/carrier-pidgin/wiki/Strong-Types

    public class MessageEndpointName : IEquatable<MessageEndpointName>
    {
        public string Value { get; }
        public static int MaximumEndpointNameLength = 100;

        public MessageEndpointName(string messageEndpointNameStr)
        {
            if (string.IsNullOrWhiteSpace(messageEndpointNameStr))
                throw new ArgumentNullException(nameof(messageEndpointNameStr));
            if (messageEndpointNameStr.Length > MaximumEndpointNameLength)
                throw new ArgumentException($"Cannot be longer than {MaximumEndpointNameLength}", nameof(messageEndpointNameStr));
            Value = messageEndpointNameStr;
        }

        public bool Equals(MessageEndpointName other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MessageEndpointName) obj);
        }

        public static bool operator ==(MessageEndpointName a, MessageEndpointName b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);

        }

        public static bool operator !=(MessageEndpointName a, MessageEndpointName b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}