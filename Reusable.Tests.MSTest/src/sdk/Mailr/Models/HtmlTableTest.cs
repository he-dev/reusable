using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Reusable.IO;
using Reusable.IO.Extensions;
using Reusable.sdk.Mailr.Models;

namespace Reusable.Tests.sdk.Mailr.Models
{
    [TestClass]
    public class HtmlTableTest
    {
        [TestMethod]
        public void CanBeSerializedToJson()
        {
            var expected = EmbeddedFileProvider<HtmlTableTest>.Default.GetFileInfoAsync(@"res\sdk\mailr\models\htmltable.json").GetAwaiter().GetResult().ReadAllText();

            var table = new HtmlTable(HtmlTableColumn.Create(("Name", typeof(string)), ("Age", typeof(int))));
            var row = table.Body.NewRow();
            row[0].Value = "John";
            row[1].Styles.Add("empty");
            var actual = JsonConvert.SerializeObject(table, new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                
            });

            Assert.AreEqual(expected, actual);
            
        }
    }
}
