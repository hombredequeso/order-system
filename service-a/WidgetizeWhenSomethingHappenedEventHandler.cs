using CarrierPidgin.Lib;
using NLog;

namespace CarrierPidgin.ServiceA
{
    public interface IMessageHandler<T>
    {
        void Handle(T msg);
    }

    public class WidgetizeWhenSomethingHappenedEventHandler: IMessageHandler<SomethingHappenedEvent>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Handle(SomethingHappenedEvent msg)
        {
            Logger.Trace($"Widgetizing - Something happened: {msg.Description}");
        }
    }
}