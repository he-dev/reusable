using Newtonsoft.Json;
using Reusable.IOnymous;
using Reusable.sdk.Mailr.Models;
using Xunit;

namespace Reusable.Tests.XUnit.sdk.Mailr.Models
{
    public class HtmlTableTest
    {
        [Fact]
        public void CanBeSerializedToJson()
        {
            var expected = EmbeddedFileProvider<HtmlTableTest>.Default.GetFile<string>(@"res\sdk\mailr\models\htmltable.json");

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
