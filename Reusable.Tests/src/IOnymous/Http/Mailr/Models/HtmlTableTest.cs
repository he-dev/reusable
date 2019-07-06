using Newtonsoft.Json;
using Reusable.IOnymous;
using Reusable.IOnymous.Http.Mailr.Models;
using Xunit;

namespace Reusable.Tests.IOnymous.Http.Mailr.Models
{
    public class HtmlTableTest
    {
        [Fact]
        public void CanBeSerializedToJson()
        {
            var expected = EmbeddedFileProvider<HtmlTableTest>.Default.ReadTextFile(@"res\IOnymous\Http\Mailr\HtmlTable.json");

            var table = new HtmlTable(HtmlTableColumn.Create(("Name", typeof(string)), ("Age", typeof(int))));
            var row = table.Body.NewRow();
            row[0].Value = "John";
            row[1].Styles.Add("empty");
            var actual = JsonConvert.SerializeObject(table, new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                
            });

            Assert.Equal(expected, actual);            
        }
    }
}
