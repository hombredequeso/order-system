using System;

namespace CarrierPidgin.ServiceA
{
    // https://github.com/hombredequeso/carrier-pidgin/wiki/Strong-Types

    public class MessageStreamName : IEquatable<MessageStreamName>
    {
        public string Value { get; }
        public static int MaximumStreamNameLength = 100;

        public MessageStreamName(string messageStreamNameStr)
        {
            if (string.IsNullOrWhiteSpace(messageStreamNameStr))
                throw new ArgumentNullException(nameof(messageStreamNameStr));
            if (messageStreamNameStr.Length > MaximumStreamNameLength)
                throw new ArgumentException($"Cannot be longer than {MaximumStreamNameLength}", nameof(messageStreamNameStr));
            Value = messageStreamNameStr;
        }

        public bool Equals(MessageStreamName other)
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
            return Equals((MessageStreamName) obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}