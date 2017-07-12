using System;
using System.Runtime.Serialization;

namespace Reusable
{
    [Serializable]
    public class InvalidVersionException : Exception
    {
        public InvalidVersionException(string message) : base(message) { }
    }
}