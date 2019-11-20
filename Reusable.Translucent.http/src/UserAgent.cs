namespace Reusable.Translucent
{
    public class UserAgent
    {
        public UserAgent(string productName, string productVersion)
        {
            ProductName = productName;
            ProductVersion = productVersion;
        }

        public string ProductName { get; }

        public string ProductVersion { get; }
    }
}