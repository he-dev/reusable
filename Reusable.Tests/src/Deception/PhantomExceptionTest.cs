using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Collections;
using Reusable.Deception.Patterns;
using Reusable.Exceptionize;
using Xunit;

namespace Reusable.Deception
{
    public class PhantomExceptionTest
    {
        [Fact]
        public void Can_throw_by_CountPattern()
        {
            var phantomException = new PhantomException
            {
                new CountPattern(Sequence.Constant(2))
            };

            var counts = new List<int>();
            foreach (var n in Sequence.Monotonic(0, 1).Take(10))
            {
                try
                {
                    phantomException.Throw("TooFast");
                }
                catch (DynamicException ex) when (ex.NameStartsWith("TooFast"))
                {
                    counts.Add(n);
                }
            }

            Assert.Equal(Sequence.Monotonic(1, 2).Take(5), counts);
        }

        [Fact]
        public async Task Can_throw_by_IntervalPattern()
        {
            var phantomException = new PhantomException
            {
                new IntervalPattern(Sequence.Constant(TimeSpan.FromSeconds(2)))
                {
                    //Predicate = // not using here
                }
            };
            
            var counts = new List<TimeSpan>();
            foreach (var n in Sequence.Constant(TimeSpan.FromSeconds(2.5)).Take(2))
            {
                await Task.Delay(n);
                
                try
                {
                    phantomException.Throw("TooFurious");
                }
                catch (DynamicException ex) when (ex.NameStartsWith("TooFurious"))
                {
                    counts.Add(n);
                }
            }

            Assert.Equal(2, counts.Count);
        }
    }
}