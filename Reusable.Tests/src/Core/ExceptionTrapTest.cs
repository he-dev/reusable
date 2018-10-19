using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Tests
{
    [TestClass]
    public class ExceptionTrapTest { }

    public interface IExceptionTrap
    {
        void Throw
        (
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null
        );
    }

    public class ExceptionTrap : IExceptionTrap
    {
        //private readonly ConcurrentDictionary<string, string> _
        public void Throw(string callerMemberName = null, int callerLineNumber = 0, string callerFilePath = null) { }
    }

    public interface IExceptionTrigger
    {
        bool Enabled { get; }

        bool Next();
    }

    public abstract class ExceptionTrigger : IExceptionTrigger
    {
        public bool Enabled { get; }
        
        public string Exception { get; }

        public abstract bool Next();
    }

    public class CountedTrigger : ExceptionTrigger
    {
        private int _current;
        
        public int Max { get; set; }
        
        public override bool Next()
        {
            if (_current++ == Max)
            {
                _current = 0;
                throw new Exception();
            }
        }
    }
    
    public  class TimedTrigger : ExceptionTrigger
    {
        public override bool Next()
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class FilteredTrigger : ExceptionTrigger
    {
        public override bool Next()
        {
            throw new System.NotImplementedException();
        }
    }
}