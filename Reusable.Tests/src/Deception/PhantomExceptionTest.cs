using System;
using Reusable.Collections;
using Reusable.Collections.Generators;
using Reusable.Deception;
using Reusable.Deception.Triggers;
using Reusable.Exceptionize;
using Xunit;

namespace Reusable.Tests.Deception
{
    public class PhantomExceptionTest
    {
        private static readonly IPhantomException PhantomException = new PhantomException(new IPhantomExceptionTrigger[]
        {
            new CountedTrigger(Sequence.Constant(2))
            {
                Filter = name => name == "James"
            },
            new CountedTrigger(Sequence.Constant(3))
            {
                Filter = name => name == "Nobody" // (default, default, nameof(Can_throw_by_member), default)
            }
        });

        [Fact]
        public void Can_throw_by_type_and_id()
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
                    Assert.True(i % 2 == 0);
                    exceptionCount++;
                }
            }

            Assert.Equal(4, exceptionCount);
        }

        [Fact]
        public void Can_throw_by_member()
        {
            var exceptionCount = 0;
            for (var i = 1; i < 10; i++)
            {
                try
                {
                    PhantomException.Throw("Nobody");
                }
                catch (DynamicException ex) when (ex.NameMatches("^Phantom"))
                {
                    Assert.True(i % 3 == 0);
                    exceptionCount++;
                }
            }

            Assert.Equal(3, exceptionCount);
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