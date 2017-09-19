using System;

namespace CarrierPidgin.ServiceA
{
    // MessageStreamName is a wrapper around a string.
    // From the perspective of data types, it avoids treating many different things as strings.
    // For instance, this would be nonsense code:
    // string messageStreamName = "myMessageStreamName";
    // string serializedMessage = JsonConvert.Serialize(someMessageObject);
    // messageStreamName = serializedMessage;
    //
    // It is clearly nonsense because a messageStreamName is not the same type of thing as a serializedMessage.
    // The benefits of this pattern is:
    // 1. It avoids assigning one type to another type, as above.
    // 2. It enforces validity at the edges of the architecture. By the time one is in the handler/domain model
    //    validation of the type has taken place, and it is sufficient to simply keep passing a MessageStreamName
    //    around.
    //    Of course, the main problem that then remains is that C# allows a MessageStreamName instance to be null.
    //    If that bothers you, use F# (it bothers me!).

    public class MessageStreamName
    {
        public MessageStreamName(string messageStreamNameStr)
        {
            if (string.IsNullOrWhiteSpace(messageStreamNameStr))
                throw new ArgumentNullException(nameof(messageStreamNameStr));
            if (messageStreamNameStr.Length > MaximumStreamNameLength)
                throw new ArgumentException($"Cannot be longer than {MaximumStreamNameLength}", nameof(messageStreamNameStr));
            Value = messageStreamNameStr;
        }

        public string Value { get; }
        public static int MaximumStreamNameLength = 100;

    }
}