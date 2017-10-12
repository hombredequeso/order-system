namespace Hdq.Statistics.Tests
{
    public static class TestConfig
    {
        public const string BaseConnectionString =
            "Host=localhost;Username=postgres;Password=mypassword;Database=carrierpidgin;Search Path=statistics";

        public static string GetConnectionString(string domain)
        {
            string schema = $";Search Path={domain}";
            return BaseConnectionString + schema;
        }
    }
}