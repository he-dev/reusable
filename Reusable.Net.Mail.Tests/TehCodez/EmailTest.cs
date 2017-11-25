using System;
using System.Threading.Tasks;
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

            var ex = Assert.That.ThrowsExceptionFiltered<DynamicException>(() => new TestClient().SendAsync(testEmail));

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
