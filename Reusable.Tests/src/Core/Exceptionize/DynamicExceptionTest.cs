using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Exceptionable
{
    [TestClass]
    public class DynamicExceptionTest
    {
        [TestMethod]
        public void Build_DynamicException_UserMessage()
        {
            Assert.That.ThrowsExceptionFiltered<DynamicException>(
                () => throw (ErrorCode.BackgroundImageNotFoundException, "Where is the 'clouds.png' image?").ToDynamicException(),
                ex => ex.NameEquals(ErrorCode.BackgroundImageNotFoundException)
            );
        }

        [TestMethod]
        public void Build_NameDoesNotEndWithException_DynamicException()
        {
            Assert.That.ThrowsExceptionFiltered<ArgumentException>(
                () => throw ("ExceptionMissing", "Test message").ToDynamicException(),
                // The custom message comes first and the parameter name is appended to the end of the message.
                ex => ex.Message.StartsWith($"Exception name must end with '{nameof(Exception)}'.") 
            );            
        }

        private enum ErrorCode
        {
            BackgroundImageNotFoundException,
        }
    }
}
