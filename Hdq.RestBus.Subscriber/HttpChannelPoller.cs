using System;
using System.Threading;
using System.Threading.Tasks;
using Hdq.Lib;
using NLog;

namespace Hdq.RestBus.Subscriber
{
    public static class HttpChannelPoller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public enum PollingError
        {
            ErrorMakingHttpRequest,
            ErrorDeserializingContent,
            UnableToConnect,
            UnknownErrorOnGet
        }

        public static async Task<Either<PollingError, TransportMessage>> Poll(
            string path, 
            IHttpService httpClient, 
            CancellationToken ct, 
            Func<string, Either<DeserializeError, TransportMessage>> deserializeTransportMessage)
        {
            Logger.Trace($"HttpChannelPoller.Poll GET {path}");
            try
            {
                var content = await httpClient.GetBodyFrom(path, ct);
                return content.Match(
                    error => new Either<PollingError, TransportMessage>(PollingError.ErrorMakingHttpRequest),
                    c =>
                    {
                        var m = deserializeTransportMessage(c);
                        return m.Match(
                            left => new Either<PollingError, TransportMessage>(PollingError
                                .ErrorDeserializingContent),
                            right => new Either<PollingError, TransportMessage>(right));
                    }
                );
            }
            catch (Exception e)
            {
                var allExceptionMessages = string.Join(Environment.NewLine,
                    ExceptionProcessor.GetAllExceptionMessages(e));
                Logger.Error($"Polling Error: {allExceptionMessages}");
                return allExceptionMessages.Contains(
                    "No connection could be made because the target machine actively refused it")
                    ? new Either<PollingError, TransportMessage>(PollingError.UnableToConnect)
                    : new Either<PollingError, TransportMessage>(PollingError.UnknownErrorOnGet);
            }
        }
    }
}