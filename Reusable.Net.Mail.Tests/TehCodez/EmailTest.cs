using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Exceptionize;
using Reusable.Tester;

namespace Reusable.Net.Mail.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var testEmail = new TestEmail();

            var ex = Assert.That.ThrowsExceptionFiltered<DynamicException>(() => new TestClient().Send(testEmail));

        }

        private class TestEmail : Email<EmailSubject, EmailBody> { }

        private class TestClient : EmailClient
        {
            protected override void SendCore<TSubject, TBody>(IEmail<TSubject, TBody> email)
            {
                throw new NotImplementedException();
            }
        }
    }
}
