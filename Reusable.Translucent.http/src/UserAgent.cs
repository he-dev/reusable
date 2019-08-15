namespace Reusable.Translucent
{
    public class UserAgent
    {
        public UserAgent() { }

        public UserAgent(string productName, string productVersion)
        {
            ProductName = productName;
            ProductVersion = productVersion;
        }

        public string ProductName { get; set; }

        public string ProductVersion { get; set; }
    }
}