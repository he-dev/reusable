using Newtonsoft.Json;
using Reusable.Translucent;
using Xunit;

namespace Reusable.Utilities.Mailr.Models
{
    public class HtmlTableTest
    {
        [Fact]
        public void CanBeSerializedToJson()
        {
            var expected = TestHelper.Resources.ReadTextFile(@"Http\Mailr\HtmlTable.json");

            var table = new HtmlTable(HtmlTableColumn.Create(("Name", typeof(string)), ("Age", typeof(int))));
            var row = table.Body.NewRow();
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
