using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Serialization;
using Reusable.Flexo;
using Reusable.Flexo.Expressions;
using Reusable.Flexo.Extensions;
using Reusable.IO;

namespace Reusable.Tests.Flexo
{
    [TestClass]
    public class AllTest
    {
        [TestMethod]
        public void All_All_True()
        {
            Assert.That.ExpressionsEqual(nameof(All), true, new All
            {
                Expressions = Constant.CreateMany(nameof(All), true, true, true)
            });
        }

        [TestMethod]
        public void All_Some_False()
        {
            var expression = new All
            {
                Expressions = Constant.CreateMany(nameof(All), true, false, true)
            };

            Assert.AreEqual(Constant.Create(nameof(All), false), expression.Invoke(Helpers.CreateContext()));
        }

        [TestMethod]
        public void All_None_False()
        {
            var expression = new All
            {
                Expressions = Constant.CreateMany(nameof(All), false, false, false)
            };

            Assert.AreEqual(Constant.Create(nameof(All), false), expression.Invoke(Helpers.CreateContext()));
        }
    }

    [TestClass]
    public class AnyTest
    {
        [TestMethod]
        public void Any_Any_True()
        {
            var expression = new Any
            {
                Expressions = Constant.CreateMany(nameof(Any), false, false, true)
            };

            Assert.AreEqual(Constant.Create(nameof(Any), true), expression.Invoke(Helpers.CreateContext()));
        }

        [TestMethod]
        public void Any_None_False()
        {
            var expression = new Any
            {
                Expressions = Constant.CreateMany(nameof(Any), false, false, false)
            };

            Assert.AreEqual(Constant.Create(nameof(Any), false), expression.Invoke(Helpers.CreateContext()));
        }
    }

    [TestClass]
    public class IIfTest
    {
        [TestMethod]
        public void Invoke_True_True()
        {
            var expression = new IIf
            {
                Predicate = Constant.Create(nameof(IIf), true),
                True = Constant.Create(nameof(IIf), "foo"),
                False = Constant.Create(nameof(IIf), "bar")
            };

            Assert.That.ExpressionsEqual(nameof(IIf), "foo", expression);
        }

        [TestMethod]
        public void Invoke_False_False()
        {
            var expression = new IIf
            {
                Predicate = Constant.Create(nameof(IIf), false),
                True = Constant.Create(nameof(IIf), "foo"),
                False = Constant.Create(nameof(IIf), "bar")
            };

            Assert.That.ExpressionsEqual(nameof(IIf), "bar", expression);
        }
    }

    [TestClass]
    public class MapBooleanToDoubleTest
    {
        [TestMethod]
        public void Invoke_True_1()
        {
            var expression = new ToDouble
            {
                Expression = Constant.Create(nameof(ToDouble), true)
            };

            Assert.That.ExpressionsEqual(nameof(ToDouble), 1.0, expression);
        }

        [TestMethod]
        public void Invoke_False_0()
        {
            var expression = new ToDouble
            {
                Expression = Constant.Create(nameof(ToDouble), false)
            };

            Assert.That.ExpressionsEqual(nameof(ToDouble), 0.0, expression);
        }
    }

    [TestClass]
    public class MaxTest
    {
        [TestMethod]
        public void Invoke_Numbers_Max()
        {
            Assert.That.ExpressionsEqual(nameof(Max), 3.0, new Max
            {
                Expressions = Constant.CreateMany(nameof(Max), 2.0, 3.0, 1.0)
            });
        }
    }

    [TestClass]
    public class MinTest
    {
        [TestMethod]
        public void Invoke_Numbers_Min()
        {
            Assert.That.ExpressionsEqual(nameof(Min), 1.0, new Min
            {
                Expressions = Constant.CreateMany(nameof(Min), 2.0, 3.0, 1.0)
            });
        }
    }

    [TestClass]
    public class SumTest
    {
        [TestMethod]
        public void Invoke_Numbers_Sum()
        {
            Assert.That.ExpressionsEqual(nameof(Sum), 6.0, new Sum
            {
                Expressions = Constant.CreateMany(nameof(Sum), 2.0, 3.0, 1.0)
            });
        }
    }

    [TestClass]
    public class EqualsTest
    {
        [TestMethod]
        public void Invoke_EqualStrings_True()
        {
            Assert.That.ExpressionsEqual(nameof(Equals), true, new Equals
            {
                Expression = Constant.Create(nameof(Equals), "foo"),
                Patterns = new[] { "foo" },
            });
        }

        [TestMethod]
        public void Invoke_NotEqualStrings_False()
        {
            Assert.That.ExpressionsEqual(nameof(Equals), false, new Equals
            {
                Expression = Constant.Create(nameof(Equals), "foo"),
                Patterns = new[] { "bar" },                
            });
        }
    }

    [TestClass]
    public class GreaterThanTest
    {
        [TestMethod]
        public void Invoke_3_2_True()
        {
            Assert.That.ExpressionsEqual(nameof(GreaterThan), true, new GreaterThan
            {
                Expression1 = Constant.Create(nameof(GreaterThan), 3.0),
                Expression2 = Constant.Create(nameof(GreaterThan), 2.0),
            });
        }

        [TestMethod]
        public void Invoke_2_3_False()
        {
            Assert.That.ExpressionsEqual(nameof(GreaterThan), false, new GreaterThan
            {
                Expression1 = Constant.Create(nameof(GreaterThan), 2.0),
                Expression2 = Constant.Create(nameof(GreaterThan), 3.0),
            });
        }

        [TestMethod]
        public void Invoke_2_2_False()
        {
            Assert.That.ExpressionsEqual(nameof(GreaterThan), false, new GreaterThan
            {
                Expression1 = Constant.Create(nameof(GreaterThan), 2.0),
                Expression2 = Constant.Create(nameof(GreaterThan), 2.0),
            });
        }
    }

    [TestClass]
    public class GreaterThanOrEqualTest
    {
        [TestMethod]
        public void Invoke_3_2_True()
        {
            Assert.That.ExpressionsEqual(nameof(GreaterThanOrEqual), true, new GreaterThanOrEqual
            {
                Expression1 = Constant.Create(nameof(GreaterThanOrEqual), 3.0),
                Expression2 = Constant.Create(nameof(GreaterThanOrEqual), 2.0),
            });
        }

        [TestMethod]
        public void Invoke_2_3_False()
        {
            Assert.That.ExpressionsEqual(nameof(GreaterThanOrEqual), false, new GreaterThanOrEqual
            {
                Expression1 = Constant.Create(nameof(GreaterThanOrEqual), 2.0),
                Expression2 = Constant.Create(nameof(GreaterThanOrEqual), 3.0),
            });
        }

        [TestMethod]
        public void Invoke_2_2_False()
        {
            Assert.That.ExpressionsEqual(nameof(GreaterThanOrEqual), true, new GreaterThanOrEqual
            {
                Expression1 = Constant.Create(nameof(GreaterThanOrEqual), 2.0),
                Expression2 = Constant.Create(nameof(GreaterThanOrEqual), 2.0),
            });
        }
    }

    [TestClass]
    public class LessThanTest
    {
        [TestMethod]
        public void Invoke_2_3_True()
        {
            Assert.That.ExpressionsEqual(nameof(LessThan), true, new LessThan
            {
                Expression1 = Constant.Create(nameof(LessThan), 2.0),
                Expression2 = Constant.Create(nameof(LessThan), 3.0),
            });
        }

        [TestMethod]
        public void Invoke_3_2_False()
        {
            Assert.That.ExpressionsEqual(nameof(LessThan), false, new LessThan
            {
                Expression1 = Constant.Create(nameof(LessThan), 3.0),
                Expression2 = Constant.Create(nameof(LessThan), 2.0),
            });
        }

        [TestMethod]
        public void Invoke_2_2_False()
        {
            Assert.That.ExpressionsEqual(nameof(LessThan), false, new LessThan
            {
                Expression1 = Constant.Create(nameof(LessThan), 2.0),
                Expression2 = Constant.Create(nameof(LessThan), 2.0),
            });
        }
    }

    [TestClass]
    public class LessThanOrEqualTest
    {
        [TestMethod]
        public void Invoke_2_3_True()
        {
            Assert.That.ExpressionsEqual(nameof(LessThanOrEqual), true, new LessThanOrEqual
            {
                Expression1 = Constant.Create(nameof(LessThanOrEqual), 2.0),
                Expression2 = Constant.Create(nameof(LessThanOrEqual), 3.0),
            });
        }

        [TestMethod]
        public void Invoke_3_2_False()
        {
            Assert.That.ExpressionsEqual(nameof(LessThanOrEqual), false, new LessThanOrEqual
            {
                Expression1 = Constant.Create(nameof(LessThanOrEqual), 3.0),
                Expression2 = Constant.Create(nameof(LessThanOrEqual), 2.0),
            });
        }

        [TestMethod]
        public void Invoke_2_2_False()
        {
            Assert.That.ExpressionsEqual(nameof(LessThanOrEqual), true, new LessThanOrEqual
            {
                Expression1 = Constant.Create(nameof(LessThanOrEqual), 2.0),
                Expression2 = Constant.Create(nameof(LessThanOrEqual), 2.0),
            });
        }
    }

    [TestClass]
    public class NotTest
    {
        [TestMethod]
        public void Invoke_True_False()
        {
            Assert.That.ExpressionsEqual(nameof(Not), false, new Not
            {
                Expression = Constant.Create(nameof(Not), true)
            });
        }

        [TestMethod]
        public void Invoke_False_True()
        {
            Assert.That.ExpressionsEqual(nameof(Not), true, new Not
            {
                Expression = Constant.Create(nameof(Not), false)
            });
        }
    }

    [TestClass]
    public class ExpresionSerializerTest
    {
        private static IFileProvider Resources { get; } = new RelativeFileProvider(new EmbeddedFileProvider(typeof(ExpresionSerializerTest).Assembly), @"Gems\Tests\res\Flexo");

        [TestMethod]
        public void Deserialize_IIf_True()
        {
            var serializer = new ExpressionSerializer(); //new DefaultContractResolver());
            var iif = serializer.Deserialize<IIf>(Resources.GetFileInfo(@"IIf.json").CreateReadStream());
            var result = iif.Invoke(Helpers.CreateContext());

            Assert.AreEqual(Constant.Create("True", "foo"), result);
        }

        [TestMethod]
        public void Deserialize_Full_True()
        {
            var serializer = new ExpressionSerializer(); //new DefaultContractResolver());
            var expressions = serializer.Deserialize<IExpression[]>(Resources.GetFileInfo(@"Full.json").CreateReadStream());
            var results = expressions.InvokeWithValidation(Helpers.CreateContext()).ToList();

            Assert.AreEqual(2, results.Count);

            Assert.AreEqual(Constant.Create("True", "foo"), results[0]);
            Assert.AreEqual(Constant.Create("ToDouble", 1.0), results[1]);
        }
    }
}
