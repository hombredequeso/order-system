using Newtonsoft.Json;
using NUnit.Framework;

namespace tests
{
    public class STest
    {
        public string Description { get; set; }
        public int Value { get; set; }
    }

    public class Wrapper
    {
        public string Description { get; set; }
        public object Value { get; set; }
    }

    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void DoubleSerialization()
        {
            STest x = new STest(){Description = "abc", Value = 123};
            string xSeri = JsonConvert.SerializeObject(x);

            object x2 = JsonConvert.DeserializeObject(xSeri);
            string xSeri2 = JsonConvert.SerializeObject(x2);

            Assert.NotNull(xSeri2);

            Wrapper w = new Wrapper(){Description = "aaa", Value = x2};
            string xSeri3 = JsonConvert.SerializeObject(w);

            Assert.NotNull(xSeri3);
        }
        
    }
}