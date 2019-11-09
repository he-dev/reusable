using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Reusable.Data;
using Reusable.Flexo.Abstractions;
using Reusable.Flexo.Helpers;
using Reusable.Utilities.XUnit.Annotations;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.Flexo
{
    public class ExpressionUseCaseTest
    {
        private readonly ITestOutputHelper _output;

        public ExpressionUseCaseTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Can_deserialize_values_into_constants()
        {
            using var helper = new ExpressionFixture();
            var useCases = helper.ReadExpressionFile<List<ExpressionUseCase>>("ExpressionCollection.json");
            ExpressionAssert.ExpressionEqual(useCases[0], _output);
            ExpressionAssert.ExpressionEqual(useCases[1], _output);
        }
        
        [Theory]
        [SmartMemberData(nameof(GetExpressionUseCases))]
        public void Evaluate(ExpressionUseCase useCase)
        {
            ExpressionAssert.ExpressionEqual(useCase, _output);
        }

        // ReSharper disable once MemberCanBePrivate.Global - no - xunit requires it to be public
        public static IEnumerable<object> GetExpressionUseCases()
        {
            using var helper = new ExpressionFixture();
            var packages = new List<IExpression>
            {
                helper.ReadExpressionFile<IExpression>("Packages.IsPositive.json")
            };
            var scope =
                ImmutableContainer
                    .Empty
                    .SetPackages(packages)
                    .SetItem("Product", new Product());
            //.SetItem(From<ITestMeta>.Select(x => x.Sth), sth),

            var useCases = helper.ReadExpressionFile<List<ExpressionUseCase>>("ExpressionUseCases.json");

            foreach (var useCase in useCases)
            {
                // We could pass this as another parameter but xunit ToStrings it and it looks really ugly and noisy.
                useCase.Scope = scope;
                yield return new { useCase };
            }
        }

        private class Product
        {
            public long Price { get; set; } = 3;
            
            public List<Category> Categories { get; set; } = new List<Category>
            {
                new Category { Color = "Green" },
                new Category { Color = "Red" },
                new Category { Color = "Orange" },
            };
        }

        private class Category
        {
            public string Color { get; set; }
        }
    }

    public class ExpressionUseCase
    {
        public string Description { get; set; }

        public IExpression Body { get; set; }

        public object Expected { get; set; }

        public bool Throws { get; set; }

        [JsonIgnore]
        public IImmutableContainer Scope { get; set; }

        public override string ToString()
        {
            return $"'{Description}'";
        }
    }
}