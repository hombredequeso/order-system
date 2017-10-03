using CarrierPidgin.ServiceA;
using CarrierPidgin.ServiceA.Bus;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace tests
{

    // https://github.com/hombredequeso/carrier-pidgin/wiki/Strong-Types

    [TestFixture]
    public class MessageStreamNameTests
    {
        [Test]
        public void Exhibits_Equality_By_Value_Behaviour()
        {
            MessageStreamName streamName = new MessageStreamName("abc");
            MessageStreamName streamName2 = new MessageStreamName("abc");

            // Compare a MessageStreamName object with itself (i.e. same reference, same value)
            Assert.IsTrue(streamName == streamName);
            Assert.IsTrue(streamName.Equals(streamName));


            // Compare a MessageStreamName object with a different MessageStreamObject,
            // that happens to have the same values.
            // If MessageStreamName behaved with by reference equality, these tests would fail.
            Assert.IsTrue(Equals(streamName, streamName2));
            Assert.IsTrue(streamName == streamName2);

            Assert.IsFalse(streamName != streamName2);


            // Check the inverse, comparing one MessageStreamName object
            // with a different MessageStreamName object with different values.
            MessageStreamName anotherStream = new MessageStreamName("xyz");

            Assert.IsFalse(Equals(streamName, anotherStream));
            Assert.IsFalse(streamName == anotherStream);

            Assert.IsTrue(streamName != anotherStream);
        }

        [Test]
        public void Null_Equality_Tests()
        {
            MessageStreamName streamName = new MessageStreamName("abc");
            MessageStreamName streamName2 = new MessageStreamName("abc");
            
            // Check for null
            Assert.IsFalse(streamName.Equals(null));

            // Null's all round: Yuck.
            // a null MessageStreamName equals another null MessageStreamName.
            // (If interpreted as saying one non-existence is the same as another
            // non-existencce this is probably what we want, but is it really
            // philosophically right to say that one non-existence is the same
            // as another? In fact, does it even make sense to ask the question...)
            MessageStreamName nullStream1 = null;
            MessageStreamName nullStream2 = null;
            Assert.IsTrue(nullStream1 == nullStream2);

            // I'm philosophically opposed to this, but in C# different types of null's are equal... :-)
            object somethingElseEntirely = null;
            Assert.IsTrue(nullStream1 == somethingElseEntirely);
        }
        
    }
}