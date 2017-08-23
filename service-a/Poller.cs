using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CarrierPidgin.Lib;
using Hdq.Lib;
using NLog;

namespace CarrierPidgin.ServiceA
{
    public static class Poller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public enum PollingError
        {
            ErrorMakingHttpRequest,
            ErrorDeserializingContent,
            UnableToConnect,
            UnknownErrorOnGet
        }

        public static async Task<Either<PollingError, TransportMessage>> Poll(string path, HttpClient httpClient, CancellationToken ct)
        {
            Logger.Trace($"Poller.Poll GET {path}");
            try
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(path, ct))
                {
                    var content = await MessageTransform.GetContent(response);
                    return content.Match(
                        error => new Either<PollingError, TransportMessage>(PollingError.ErrorMakingHttpRequest),
                        c =>
                        {
                            var m = MessageTransform.Deserialize<TransportMessage>(c);
                            return m.Match(
                                left => new Either<PollingError, TransportMessage>(PollingError
                                    .ErrorDeserializingContent),
                                right => new Either<PollingError, TransportMessage>(right));
                        }
                    );
                }
            }
            catch (Exception e)
            {
                var allExceptionMessages = string.Join(Environment.NewLine, ExceptionProcessor.GetAllExceptionMessages(e));
                Logger.Error($"Polling Error: {allExceptionMessages}");
                return allExceptionMessages.Contains("No connection could be made because the target machine actively refused it")
                    ? new Either<PollingError, TransportMessage>(PollingError.UnableToConnect)
                    : new Either<PollingError, TransportMessage>(PollingError.UnknownErrorOnGet);
            }
        }
    }
}