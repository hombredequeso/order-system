using Nancy;

namespace CarrierPidgin.EventBus.Module
{
    public class Health
    {
        public string Version { get; set; }
        
    }

    public class HomeModule: NancyModule
    {
        public readonly Health Health = new Health {Version = "0.1"};


            public HomeModule()
            {
                Get["/test"] = _ => "hello world";

                Get["/health"] = _ => Response.AsJson(Health);
            }
    }
}