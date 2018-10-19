using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Reflection;
using Reusable.Diagnostics;

namespace Reusable.Tests
{
    [TestClass]
    public class ExceptionTrapTest
    {
        [TestMethod]
        public void Throw()
        {
            var trap = new ExceptionTrap(new IExceptionTrigger[]
            {
                new CountedTrigger(new RegularSequence<int>(2))
            });

            var exceptionCount = 0;
            for (var i = 1; i < 10; i++)
            {
                try
                {
                    trap.Throw();
                }
                catch (DynamicException ex)
                {
                    Assert.IsTrue(i % 2 == 0);
                    exceptionCount++;
                }
            }

            Assert.AreEqual(4, exceptionCount);
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