using System;

namespace Reusable.Shelly
{
    public class ArgumentNotFoundException : Exception
    {
        public string ArgumentName { get; set; }
    }

    public class ArgumentMappingException : Exception
    {
        public ArgumentMappingException(Exception innerException) : base(null, innerException) { }

        public string ArgumentName { get; set; }
        public string ArgumentType { get; set; }
        public string Value { get; set; }
    }

    public class CommnadNotFoundException : Exception
    {
        public CommnadNotFoundException(string message) : base(message) { }
    }
}
