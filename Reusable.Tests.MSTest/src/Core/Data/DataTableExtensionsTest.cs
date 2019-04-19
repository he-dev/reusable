using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using Reusable.Data;

namespace Reusable.Tests.Data
{
    [TestClass]
    public class DataTableExtensionsTest
    {
        [TestMethod]
        public void AddRow_ThreeRows_ThreeRowsMore()
        {
            var dt = new DataTable();
            dt.Columns.Add("Testcolumn");

            dt.AddRow("foo");
            dt.AddRow("bar");
            dt.AddRow("baz");
            Assert.AreEqual(3, dt.Rows.Count);
        }
    }
}
