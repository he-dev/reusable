using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Flexo;
using Reusable.IO;

namespace Reusable.Tests.Flexo
{
    using static Assert;

    [TestClass]
    public class ExpressionSerializerTest
    {
        private static IFileProvider Resources { get; } = new RelativeFileProvider(new EmbeddedFileProvider(typeof(ExpressionSerializerTest).Assembly), @"res\Flexo");

        [TestMethod]
        public void CanDeserializeAllExpressions()
        {
            var serializer = new ExpressionSerializer();
            var expressions = serializer.Deserialize<IList<IExpression>>(Resources.GetFileInfoAsync(@"Expressions.json").Result.CreateReadStream());

            AreEqual(3, expressions.Count);

            AreEqual(Constant.Create(true), expressions[0].Invoke(new ExpressionContext()));
            AreEqual(Constant.Create(false), expressions[1].Invoke(new ExpressionContext()));
            AreEqual(Constant.Create(3.0), expressions[2].Invoke(new ExpressionContext()));
        }

        [TestMethod]
        public void CanDeserializeSingleExpression()
        {
            using (var json = Resources.GetFileInfoAsync(@"Single-expression.json").Result.CreateReadStream())
            {
                var expression = Expression.Parse(json, new ExpressionSerializer());
                IsNotNull(expression);
            }
        }
    }

    internal class MyTest
    {
        public IExpression MyExpression { get; set; }
    }
}
