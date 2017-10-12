using NUnit.Framework;

namespace Hdq.RestBus.Subscriber.Tests
{

    // https://github.com/hombredequeso/carrier-pidgin/wiki/Strong-Types

    [TestFixture]
    public class MessageStreamNameTests
    {
        [Test]
        public void Exhibits_Equality_By_Value_Behaviour()
        {
            MessageEndpointName endpointName = new MessageEndpointName("abc");
            MessageEndpointName endpointName2 = new MessageEndpointName("abc");

            // Compare a MessageEndpointName object with itself (i.e. same reference, same value)
            Assert.IsTrue(endpointName == endpointName);
            Assert.IsTrue(endpointName.Equals(endpointName));


            // Compare a MessageEndpointName object with a different MessageStreamObject,
            // that happens to have the same values.
            // If MessageEndpointName behaved with by reference equality, these tests would fail.
            Assert.IsTrue(Equals(endpointName, endpointName2));
            Assert.IsTrue(endpointName == endpointName2);

            Assert.IsFalse(endpointName != endpointName2);


            // Check the inverse, comparing one MessageEndpointName object
            // with a different MessageEndpointName object with different values.
            MessageEndpointName anotherEndpoint = new MessageEndpointName("xyz");

            Assert.IsFalse(Equals(endpointName, anotherEndpoint));
            Assert.IsFalse(endpointName == anotherEndpoint);

            Assert.IsTrue(endpointName != anotherEndpoint);
        }

        [Test]
        public void Null_Equality_Tests()
        {
            MessageEndpointName endpointName = new MessageEndpointName("abc");
            MessageEndpointName endpointName2 = new MessageEndpointName("abc");
            
            // Check for null
            Assert.IsFalse(endpointName.Equals(null));

            // Null's all round: Yuck.
            // a null MessageEndpointName equals another null MessageEndpointName.
            // (If interpreted as saying one non-existence is the same as another
            // non-existencce this is probably what we want, but is it really
            // philosophically right to say that one non-existence is the same
            // as another? In fact, does it even make sense to ask the question...)
            MessageEndpointName nullStream1 = null;
            MessageEndpointName nullStream2 = null;
            Assert.IsTrue(nullStream1 == nullStream2);

            // I'm philosophically opposed to this, but in C# different types of null's are equal... :-)
            object somethingElseEntirely = null;
            Assert.IsTrue(nullStream1 == somethingElseEntirely);
        }
        
    }
}