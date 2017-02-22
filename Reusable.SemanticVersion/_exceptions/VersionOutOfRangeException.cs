using System;
using System.Runtime.Serialization;

namespace Reusable
{
    [Serializable]
    public class VersionOutOfRangeException : Exception
    {
        public override string Message => "Version must be greater or equal 0.";
    }
}