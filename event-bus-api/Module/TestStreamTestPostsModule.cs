using CarrierPidgin.EventBus.Dal;
using CarrierPidgin.Lib;
using Nancy;
using Nancy.ModelBinding;

namespace CarrierPidgin.EventBus.Module
{
    public class SomethingHappenedPostData
    {
        public string Description { get; set; }
    }

    public class TestStreamTestPostsModule : NancyModule
    {
        public TestStreamTestPostsModule()
        {
            Post["/teststream/somethinghappened"] = parameters =>
            {
                var postContent = this.Bind<SomethingHappenedPostData>();
                SomethingHappenedEvent evt = new SomethingHappenedEvent()
                {
                    Description = postContent.Description
                };
                TestStreamRepository.AddEvent(evt);
                return HttpStatusCode.Created;
            };
        }
    }
}