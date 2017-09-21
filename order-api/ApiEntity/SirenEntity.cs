using System.Collections.Generic;

namespace CarrierPidgin.OrderService.ApiEntity
{
    // Reference for Siren definition:
    // https://github.com/kevinswiber/siren

    public class SirenEntity
    {
        public List<string> Class { get; set; }
        public object Properties { get; set; }
        public List<Link> Links { get; set; }
        public List<Action> Actions { get; set; }
    }
}