using System;
using Hdq.RestBus;
using Hdq.RestBus.Receiver;
using CarrierPidgin.ServiceA.Dal;
using Dapper;
using Optional;
using Optional.Linq;

namespace CarrierPidgin.ServiceA.Handlers
{
    public class MessageQueueProcessingDetailsRow
    {
        public Guid Id { get; set; }
        public string QueueName { get; set; }
        public long LastMessageNumber { get; set; }
        public long Version { get; set; }
        public DateTimeOffset UpdatedTimestamp { get; set; }
    }

    public static class MessageNumberRepository
    {
        public static Option<Tuple<long, MessageQueueProcessingDetailsRow>> GetLastProcessedMessageNumber(
            UnitOfWork uow, 
            MessageEndpointName queueName)
        {
            var getQuery =
                @"SELECT ""Id"", ""QueueName"", ""LastMessageNumber"", ""Version"", ""UpdatedTimestamp"" from ""MessageQueueProcessingDetails"" where ""QueueName"" = @queueName";
            object queryParameters = new {QueueName = queueName.Value};

            Option<MessageQueueProcessingDetailsRow> result2 = 
                DbRepository.GetSingle<MessageQueueProcessingDetailsRow>(getQuery, queryParameters, uow);

            return result2.Map(r => new Tuple<long, MessageQueueProcessingDetailsRow>(r.LastMessageNumber, r));
        }

        public static readonly string MessageQueueProcessingDetailsUpdateQuery =
            @"UPDATE ""MessageQueueProcessingDetails"" SET ( ""LastMessageNumber"", ""Version"", ""UpdatedTimestamp"") = (@LastMessageNumber, @Version, @UpdatedTimestamp) WHERE ""Id"" = @Id AND ""Version"" = @ExistingVersionNumber";

        public static void UpdateLastProcessedMessage(
            UnitOfWork uow,
            MessageQueueProcessingDetailsRow queueDetails)
        {
            int rowCountUpdated = uow.DbConnection.Execute(
                MessageQueueProcessingDetailsUpdateQuery,
                new
                {
                    Id = queueDetails.Id,
                    LastMessageNumber = queueDetails.LastMessageNumber,
                    Version = queueDetails.Version + 1,
                    ExistingVersionNumber = queueDetails.Version,
                    UpdatedTimestamp = DateTimeOffset.UtcNow,
                },
                uow.Transaction);

            if (rowCountUpdated != 1)
            {
                throw new Exception("MessageQueueProcessingDetails: Concurrency Exception, (probably!)");
            }
        }

        public static readonly string InsertStatement =
            @"INSERT INTO ""MessageQueueProcessingDetails"" 
                ( ""Id"", ""QueueName"", ""Version"", ""LastMessageNumber"", ""UpdatedTimestamp"") 
                VALUES (@Id, @QueueName, @Version, @LastMessageNumber, @UpdatedTimestamp)";

        public static void InsertLastProcessedMessage(
            UnitOfWork uow, 
            long messageNumber, 
            MessageEndpointName queueName)
        {
            MessageQueueProcessingDetailsRow d = new MessageQueueProcessingDetailsRow()
            {
                Id = Guid.NewGuid(),
                LastMessageNumber = messageNumber,
                QueueName = queueName.Value,
                Version = 0,
                UpdatedTimestamp = DateTimeOffset.UtcNow
            };

            int rowsInserted = uow.DbConnection.Execute(
                InsertStatement,
                d,
                uow.Transaction);
        }

        internal static void InsertOrUpdateLastProcessedMessage(
            UnitOfWork uow, 
            MessageEndpointName queueName, 
            long messageNumber, 
            Option<MessageQueueProcessingDetailsRow> option)
        {
            option.Match(
                x =>
                {
                    x.LastMessageNumber = messageNumber;
                    UpdateLastProcessedMessage(uow, x);
                },
                () =>
                {
                    InsertLastProcessedMessage(uow, messageNumber, queueName);
                }
            );
        }
    }

    public class DeDupHandler<T>
    {
        private readonly UnitOfWork _uow;
        public Action<T> _next;

        public DeDupHandler(UnitOfWork uow)
        {
            _uow = uow;
        }

        public void Handle(
            T evt, 
            MessageEndpointName queueName, 
            long messageNumber)
        {
            Option<Tuple<long, MessageQueueProcessingDetailsRow>> lastProcessedMessage = 
                MessageNumberRepository.GetLastProcessedMessageNumber(_uow, queueName);
            if (IsNewMessage(lastProcessedMessage, messageNumber))
            {
                MessageNumberRepository.InsertOrUpdateLastProcessedMessage(
                    _uow, 
                    queueName,
                    messageNumber,
                    lastProcessedMessage.Select(x => x.Item2));
                _next(evt);
            }
        }

        private bool IsNewMessage(Option<Tuple<long, MessageQueueProcessingDetailsRow>> lastProcessedMessage, long messageNumber)
        {
            return lastProcessedMessage.Match(
                x => messageNumber > x.Item1,
                () => true);
        }
    }
}