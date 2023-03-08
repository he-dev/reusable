﻿using Newtonsoft.Json;
using Reusable.Translucent;
using Xunit;

namespace Reusable.Utilities.Mailr.Models
{
    public class HtmlTableTest : IClassFixture<TestHelperFixture>
    {
        private readonly TestHelperFixture _testHelper;

        public HtmlTableTest(TestHelperFixture testHelper)
        {
            _testHelper = testHelper;
        }
        
        [Fact]
        public void CanBeSerializedToJson()
        {
            var expected = _testHelper.Resource.ReadTextFile(@"Http\Mailr\HtmlTable.json");

            var table = new HtmlTable(("Name", typeof(string)), ("Age", typeof(int)));
            var row = table.Body.AddRow();
            row[0].Value = "John";
            row[1].Tags.Add("empty");
            var actual = JsonConvert.SerializeObject(table, new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,
                
            });

            Assert.Equal(expected, actual);            
        }
    }
}
