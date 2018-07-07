using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Reflection
{
    [TestClass]
    public class DynamicExceptionTest
    {
        [TestMethod]
        public void Build_DynamicException_UserMessage()
        {
            Assert.That.Throws<DynamicException>(
                () => throw (ErrorCode.BackgroundImageNotFoundException, "Where is the 'clouds.png' image?").ToDynamicException(),
                filter => filter.WhenName(ErrorCode.BackgroundImageNotFoundException.ToString())
            );
        }

        [TestMethod]
        public void Build_NameDoesNotEndWithException_DynamicException()
        {
            Assert.That.Throws<ArgumentException>(
                () => throw ("ExceptionMissing", "Test message").ToDynamicException(),
                // The custom message comes first and the parameter name is appended to the end of the message.
                filter => filter.WhenMessage($"^Exception name must end with '{nameof(Exception)}'.") 
            );            
        }

        private enum ErrorCode
        {
            BackgroundImageNotFoundException,
        }
    }
}
