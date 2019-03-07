using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Serialization;
using Reusable.Flexo;

namespace Reusable.Tests.Flexo
{
    [TestClass]
    public class ExpressionTest
    {
        [TestMethod]
        public void All_ReturnsTrueWhenAllTrue() => Assert.That.ExpressionsEqual(true, new All { Predicates = Constant.CreateMany(true, true, true) });

        [TestMethod]
        public void All_ReturnsFalseWhenSomeFalse() => Assert.That.ExpressionsEqual(false, new All { Predicates = Constant.CreateMany(true, false, true) });

        [TestMethod]
        public void All_ReturnsFalseWhenAllFalse() => Assert.That.ExpressionsEqual(false, new All { Predicates = Constant.CreateMany(false, false, false) });

        [TestMethod]
        public void Any_ReturnsTrueWhenSomeTrue() => Assert.That.ExpressionsEqual(true, new Any { Predicates = Constant.CreateMany(false, false, true) });

        [TestMethod]
        public void Any_ReturnsFalseWhenAllFalse() => Assert.That.ExpressionsEqual(false, new Any { Predicates = Constant.CreateMany(false, false, false) });

        [TestMethod]
        public void IIf_ReturnsTrueWhenTrue() => Assert.That.ExpressionsEqual("foo", new IIf
        {
            Predicate = Constant.Create(true),
            True = Constant.Create("foo"),
            False = Constant.Create("bar")
        });

        [TestMethod]
        public void IIf_ReturnsFalseWhenFalse() => Assert.That.ExpressionsEqual("bar", new IIf
        {
            Predicate = Constant.Create(false),
            True = Constant.Create("foo"),
            False = Constant.Create("bar")
        });

        [TestMethod]
        public void Max_ReturnsMax() => Assert.That.ExpressionsEqual(3.0, new Max { Expressions = Constant.CreateMany(2.0, 3.0, 1.0) });

        [TestMethod]
        public void Min_ReturnsMin() => Assert.That.ExpressionsEqual(1.0, new Min { Expressions = Constant.CreateMany(2.0, 3.0, 1.0) });

        [TestMethod]
        public void Sum_ReturnsSum() => Assert.That.ExpressionsEqual(6.0, new Sum { Expressions = Constant.CreateMany(2.0, 3.0, 1.0) });

        [TestMethod]
        public void Equals_ReturnsTrueWhenEqual() => Assert.That.ExpressionsEqual(true, new Equals
        {
            Left = Constant.Create("foo"),
            Right = Constant.Create("foo"),
        });

        [TestMethod]
        public void Equals_ReturnsFalseWhenNotEqual() => Assert.That.ExpressionsEqual(false, new Equals
        {
            Left = Constant.Create("foo"),
            Right = Constant.Create("bar"),
        });

        [TestMethod]
        public void GreaterThan_ReturnsTrueWhenLeftGreaterThanRight() => Assert.That.ExpressionsEqual(true, new GreaterThan
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(2.0),
        });

        [TestMethod]
        public void GreaterThan_ReturnsFalseWhenLeftLessThanRight() => Assert.That.ExpressionsEqual(false, new GreaterThan
        {
            Left = Constant.Create(2.0),
            Right = Constant.Create(3.0),
        });

        [TestMethod]
        public void GreaterThan_ReturnsFalseWhenLeftEqualsRight() => Assert.That.ExpressionsEqual(false, new GreaterThan
        {
            Left = Constant.Create(2.0),
            Right = Constant.Create(2.0),
        });

        [TestMethod]
        public void GreaterThanOrEqual_ReturnsTrueWhenLeftGreaterThanRight() => Assert.That.ExpressionsEqual(true, new GreaterThanOrEqual
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(2.0),
        });

        [TestMethod]
        public void GreaterThanOrEqual_ReturnsTrueWhenLeftEqualsRight() => Assert.That.ExpressionsEqual(true, new GreaterThanOrEqual
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(3.0),
        });

        [TestMethod]
        public void GreaterThanOrEqual_ReturnsFalseWhenLeftLessThanRight() => Assert.That.ExpressionsEqual(false, new GreaterThanOrEqual
        {
            Left = Constant.Create(2.0),
            Right = Constant.Create(3.0),
        });

        [TestMethod]
        public void LessThan_ReturnsTrueWhenLeftLessThanRight() => Assert.That.ExpressionsEqual(true, new LessThan
        {
            Left = Constant.Create(2.0),
            Right = Constant.Create(3.0),
        });

        [TestMethod]
        public void LessThan_ReturnsFalseWhenLeftEqualsRight() => Assert.That.ExpressionsEqual(false, new LessThan
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(3.0),
        });

        [TestMethod]
        public void LessThan_ReturnsFalseWhenLeftGreaterThanRight() => Assert.That.ExpressionsEqual(false, new LessThan
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(2.0),
        });

        [TestMethod]
        public void LessThanOrEqual_ReturnsTrueWhenLeftLessThanRight() => Assert.That.ExpressionsEqual(true, new LessThanOrEqual
        {
            Left = Constant.Create(2.0),
            Right = Constant.Create(3.0),
        });

        [TestMethod]
        public void LessThanOrEqual_ReturnsTrueWhenLeftEqualsRight() => Assert.That.ExpressionsEqual(true, new LessThanOrEqual
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(3.0),
        });

        [TestMethod]
        public void LessThanOrEqual_ReturnsFalseWhenLeftGreaterThanRight() => Assert.That.ExpressionsEqual(false, new LessThanOrEqual
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(2.0),
        });

        [TestMethod]
        public void Not_ReturnsTrueWhenFalse() => Assert.That.ExpressionsEqual(false, new Not { Predicate = Constant.Create(true) });

        [TestMethod]
        public void Not_ReturnsFalseWhenTrue() => Assert.That.ExpressionsEqual(false, new Not { Predicate = Constant.Create(true) });

        [TestMethod]
        public void ToDouble_MapsTrueToOne() => Assert.That.ExpressionsEqual(1.0, new BooleanToDouble { Expression = Constant.Create(true) });

        [TestMethod]
        public void ToDouble_MapsFalseToZero() => Assert.That.ExpressionsEqual(0.0, new BooleanToDouble { Expression = Constant.Create(false) });
    }
}