namespace Hdq.OrderApi.ApiEntity
{
    public class Link
    {
        public string[] Rel { get; set; }
        public string Href { get; set; }

        public static string Self = "self";
    }
}