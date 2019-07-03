using System;
using Reusable.Quickey;
using Xunit;

namespace Reusable.Tests.Quickey
{
    public class FeatureTest
    {
        [Fact]
        public void Throws_when_attributes_missing()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => From<NoAttributes>.Select(x => x.Text));
            Assert.Equal("Either the type 'NoAttributes' or the member 'Text' must be decorated with at least one 'UseXAttribute'.", ex.Message);
        }
        
        private class NoAttributes
        {
            public string Text { get; set; }
        }
    }
}