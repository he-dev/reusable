using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Tests
{
    [TestClass]
    public class ComparerFactoryTest
    {
        [TestMethod]
        public void CanCreateComparer1()
        {
            var comparer = ComparerFactory<Product>.Create(
                x => x.Price,
                (builder, x, y) =>
                {
                    builder
                        .LessThen(() => x < y)
                        .Equal(() => x == y)
                        .GreaterThan(() => x > y);
                });

            var products = new[]
            {
                new Product {Name = "Car", Price = 7 },
                new Product {Name = "Table", Price = 3 },
                new Product {Name = "Orange", Price = 1 },
            };

            var sorted = products.OrderByDescending(p => p, comparer).ToList();

            Assert.AreEqual("Car", sorted.ElementAt(0).Name);
            Assert.AreEqual("Table", sorted.ElementAt(1).Name);
            Assert.AreEqual("Orange", sorted.ElementAt(2).Name);
        }

        [TestMethod]
        public void CanCreateCanonicalComparer()
        {
            var comparer = ComparerFactory<Product>.Create(
                x => new { x.Name.Length, x.Price },
                (builder, x, y) =>
                {
                    builder
                        .LessThen(() => x.Length < y.Length || x.Price < y.Price)
                        .Equal(() => x.Length == y.Length || x.Price == y.Price)
                        .GreaterThan(() => x.Length > y.Length || x.Price > y.Price);
                });

            var products = new[]
            {
                new Product {Name = "Car", Price = 7 },
                new Product {Name = "Table", Price = 3 },
                new Product {Name = "Orange", Price = 1 },
            };

            var sorted = products.OrderByDescending(p => p, comparer).ToList();

            Assert.AreEqual(3, sorted.Count);
            Assert.AreEqual("Orange", sorted.ElementAt(0).Name); // because added first
            Assert.AreEqual("Car", sorted.ElementAt(1).Name); // because added first
            Assert.AreEqual("Table", sorted.ElementAt(2).Name); // because ItemCount
        }

        private class Product
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int Price { get; set; }
        }
    }
}
