using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Collections.Generators;
using Reusable.Diagnostics;
using Reusable.Diagnostics.Abstractions;
using Reusable.Diagnostics.Triggers;
using Reusable.Reflection;

namespace Reusable.Tests.Diagnostics
{
    [TestClass]
    public class PhantomExceptionTest
    {
        private static readonly IPhantomException PhantomException = new PhantomException(new IPhantomExceptionTrigger[]
        {
            new CountedTrigger(new RegularSequence<int>(2))
            {
                Filter = (default, nameof(PhantomExceptionTest), default, "James")
            },
            new CountedTrigger(new RegularSequence<int>(3))
            {
                Filter = (default, default, nameof(CanThrowByMember), default)
            }
        });

        [TestMethod]
        public void CanThrowByTypeAndId()
        {
            var exceptionCount = 0;
            for (var i = 1; i < 10; i++)
            {
                try
                {
                    PhantomException.Throw("James");
                }
                catch (DynamicException ex) when (ex.NameMatches("^Phantom"))
                {
                    Assert.IsTrue(i % 2 == 0);
                    exceptionCount++;
                }
            }

            Assert.AreEqual(4, exceptionCount);
        }

        [TestMethod]
        public void CanThrowByMember()
        {
            var exceptionCount = 0;
            for (var i = 1; i < 10; i++)
            {
                try
                {
                    PhantomException.Throw();
                }
                catch (DynamicException ex) when (ex.NameMatches("^Phantom"))
                {
                    Assert.IsTrue(i % 3 == 0);
                    exceptionCount++;
                }
            }

            Assert.AreEqual(3, exceptionCount);
        }
    }

    /*
    public class AddressRepository
    {
        [CanBeNull]
        public IExceptionTrap Trap { get; set; }

        public IEnumerable<string> GetAddressesWithoutZip()
        {
            Trap?.Throw();

            // do stuff...
        }
    }
    */


}