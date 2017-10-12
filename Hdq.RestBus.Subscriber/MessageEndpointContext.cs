using System.Collections.Generic;
using System.Linq;

namespace Hdq.RestBus.Subscriber
{
    public class MessageEndpointContext
    {
        protected MessageEndpointContext(MessageEndpointName sourceQueue)
        {
            Unprocessed = new List<DomainMessage>();
            ProcessedSuccessfully = new List<DomainMessage>();
            ProcessedUnsuccessfully = new List<DomainMessage>();
            SourceQueue = sourceQueue;
        }

        public MessageEndpointContext(
            IEnumerable<DomainMessage> processedSuccessfully, 
            IEnumerable<DomainMessage> processedUnsuccessfully, 
            IEnumerable<DomainMessage> unprocessed,
            MessageEndpointName sourceQueue)
        {
            Unprocessed = unprocessed.ToList();
            ProcessedSuccessfully = processedSuccessfully.ToList();
            ProcessedUnsuccessfully = processedUnsuccessfully.ToList();
            SourceQueue = sourceQueue;
        }

        public static MessageEndpointContext Start(MessageEndpointName sourceQueue)
        {
            return new MessageEndpointContext(sourceQueue);
        }

        public  MessageEndpointContext AddSuccess(DomainMessage e)
        {
            return new MessageEndpointContext(
                ProcessedSuccessfully.Concat(new[] {e}),
                ProcessedUnsuccessfully,
                Unprocessed,
                this.SourceQueue);
        }

        public  MessageEndpointContext AddFailure(DomainMessage e)
        {
            return new MessageEndpointContext(
                ProcessedSuccessfully,
                ProcessedUnsuccessfully.Concat(new[] {e}),
                Unprocessed,
                this.SourceQueue);
        }

        public  MessageEndpointContext AddUnprocessed(DomainMessage e)
        {
            return new MessageEndpointContext(
                ProcessedSuccessfully,
                ProcessedUnsuccessfully,
                Unprocessed.Concat(new[] {e}),
                this.SourceQueue);
        }

        public List<DomainMessage> ProcessedSuccessfully { get; }
        public List<DomainMessage> ProcessedUnsuccessfully { get; }
        public List<DomainMessage> Unprocessed { get; }
        public MessageEndpointName SourceQueue { get;  }
        
    }
}