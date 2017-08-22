using System;
using CarrierPidgin.Lib;

namespace CarrierPidgin.ServiceA
{
    public interface IMessageHandler<T>
    {
        void Handle(T msg);
    }

    public class WidgetizeWhenSomethingHappenedEventHandler: IMessageHandler<SomethingHappenedEvent>
    {
        public void Handle(SomethingHappenedEvent msg)
        {
            Console.WriteLine("Widgetizing, because something happened.");
        }
    }
}