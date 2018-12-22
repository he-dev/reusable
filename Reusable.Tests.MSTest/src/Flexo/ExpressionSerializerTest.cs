using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;
using Reusable.Flexo;
using Reusable.IOnymous;

namespace Reusable.Tests.Flexo
{
    using static Assert;

    [TestClass]
    public class ExpressionSerializerTest
    {
        private static IResourceProvider Resources { get; } = new RelativeResourceProvider(new EmbeddedFileProvider(typeof(ExpressionSerializerTest).Assembly), @"res\Flexo");

        [TestMethod]
        public void CanDeserializeAllExpressions()
        {
            var serializer = new ExpressionSerializer();
            var expressions = serializer.Deserialize<IList<IExpression>>(Resources.GetFile<string>(@"Expressions.json").ToStreamReader().BaseStream);

            AreEqual(3, expressions.Count);

            AreEqual(Constant.Create(true), expressions[0].Invoke(new ExpressionContext()));
            AreEqual(Constant.Create(false), expressions[1].Invoke(new ExpressionContext()));
            AreEqual(Constant.Create(3.0), expressions[2].Invoke(new ExpressionContext()));
        }

        [TestMethod]
        public void CanDeserializeSingleExpression()
        {
            using (var json = Resources.GetFile<string>(@"Single-expression.json").ToStreamReader())
            {
                var expression = Expression.Parse(json.BaseStream, new ExpressionSerializer());
                IsNotNull(expression);
            }
        }
    }

    internal class MyTest
    {
        public IExpression MyExpression { get; set; }
    }
}
