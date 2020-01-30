using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Reusable.Data;
using Reusable.Flexo.Abstractions;
using Reusable.Flexo.Containers;
using Reusable.Flexo.Helpers;
using Reusable.Utilities.XUnit.Annotations;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.Flexo
{
    public class ExpressionUseCaseTest : IClassFixture<ExpressionFixture>, IClassFixture<TestHelperFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly ExpressionFixture _expressionHelper;
        private static TestHelperFixture _testHelper;

        public ExpressionUseCaseTest(ITestOutputHelper output, ExpressionFixture expressionHelper, TestHelperFixture testHelper)
        {
            _output = output;
            _expressionHelper = expressionHelper;
            _testHelper = testHelper;
        }

        [Fact]
        public void Can_deserialize_values_into_constants()
        {
            var useCases = _expressionHelper.ReadExpressionFile<List<ExpressionUseCase>>(_testHelper.Resources, "ExpressionCollection.json");
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
                helper.ReadExpressionFile<IExpression>(_testHelper.Resources, "Packages.IsPositive.json")
            };
            var scope =
                ImmutableContainer
                    .Empty
                    .SetItem(ExpressionContext.Packages, new PackageContainer(packageId => packages.Single(p => p.Id == packageId) as Package))
                    .SetItem("Product", new Product());

            var useCases = helper.ReadExpressionFile<List<ExpressionUseCase>>(_testHelper.Resources, "ExpressionUseCases.json");

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

        public List<object> Expected { get; set; } = new List<object>();

        public bool Throws { get; set; }

        [JsonIgnore]
        public IImmutableContainer Scope { get; set; }

        public override string ToString()
        {
            return $"'{Description}'";
        }
    }
}