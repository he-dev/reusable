using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Net.Mail;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Net.Mail
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var testEmail = new TestEmail();

            Assert.That.ThrowsExceptionWhen<DynamicException>(() => new TestClient().SendAsync(testEmail));
        }

        private class TestEmail : Email<IEmailSubject, IEmailBody> { }

        private class TestClient : EmailClient
        {
            protected override Task SendAsyncCore<TSubject, TBody>(IEmail<TSubject, TBody> email)
            {
                throw new NotImplementedException();
            }
        }
    }
}
