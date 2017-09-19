using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.Lib;

namespace CarrierPidgin.ServiceA
{
    public class MessageProcessingContext
    {
        protected MessageProcessingContext(MessageStreamName sourceQueue)
        {
            Unprocessed = new List<DomainMessage>();
            ProcessedSuccessfully = new List<DomainMessage>();
            ProcessedUnsuccessfully = new List<DomainMessage>();
            SourceQueue = sourceQueue;
        }

        public MessageProcessingContext(
            IEnumerable<DomainMessage> processedSuccessfully, 
            IEnumerable<DomainMessage> processedUnsuccessfully, 
            IEnumerable<DomainMessage> unprocessed,
            MessageStreamName sourceQueue)
        {
            Unprocessed = unprocessed.ToList();
            ProcessedSuccessfully = processedSuccessfully.ToList();
            ProcessedUnsuccessfully = processedUnsuccessfully.ToList();
            SourceQueue = sourceQueue;
        }

        public static MessageProcessingContext Start(MessageStreamName sourceQueue)
        {
            return new MessageProcessingContext(sourceQueue);
        }

        public  MessageProcessingContext AddSuccess(DomainMessage e)
        {
            return new MessageProcessingContext(
                ProcessedSuccessfully.Concat(new[] {e}),
                ProcessedUnsuccessfully,
                Unprocessed,
                this.SourceQueue);
        }

        public  MessageProcessingContext AddFailure(DomainMessage e)
        {
            return new MessageProcessingContext(
                ProcessedSuccessfully,
                ProcessedUnsuccessfully.Concat(new[] {e}),
                Unprocessed,
                this.SourceQueue);
        }

        public  MessageProcessingContext AddUnprocessed(DomainMessage e)
        {
            return new MessageProcessingContext(
                ProcessedSuccessfully,
                ProcessedUnsuccessfully,
                Unprocessed.Concat(new[] {e}),
                this.SourceQueue);
        }

        public List<DomainMessage> ProcessedSuccessfully { get; }
        public List<DomainMessage> ProcessedUnsuccessfully { get; }
        public List<DomainMessage> Unprocessed { get; }
        public MessageStreamName SourceQueue { get;  }
        
    }
}