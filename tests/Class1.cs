using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace tests
{
    public class Message1
    { }

    public interface IHandler<T>
    {
        void Handle(T msg);
    }

    public class DomainHandler : IHandler<Message1>
    {
        public void Handle(Message1 msg)
        {
            Console.WriteLine("DomainHandler.Handle");
        }
    }

    public class LoggingHandler : IHandler<Message1>
    {
        private readonly Func<IHandler<Message1>> _nextHandler;


        public LoggingHandler(Func<IHandler<Message1>> nextHandler)
        {
            _nextHandler = nextHandler;
        }

        public void Handle(Message1 msg)
        {
            Console.WriteLine("LoggingHandler: going in...");
            _nextHandler().Handle(msg);
            Console.WriteLine("LoggingHandler: going out...");
        }
    }

    public class DummyIoc
    {
        public T Create<T>()
        {
            return default(T);
        }

        public void Register<T>(T tInstance)
        {
            
        }
    }


    [TestFixture]
    public class Class1
    {
        [Test]
        public void Handler_Chaining()
        {
            List<Func<DummyIoc, IHandler<Message1>>> handlerChain = new List<Func<DummyIoc, IHandler<Message1>>>()
            {
                i =>
                {
                    var logHandler = i.Create<LoggingHandler>();
                    i.Register(logHandler);
                    return logHandler;
                },
                i => i.Create<DomainHandler>()
            };


            Message1 m = new Message1();
            

        }

    }
}
