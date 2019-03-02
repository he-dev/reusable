using System;
using Newtonsoft.Json.Serialization;
using Reusable.Flexo;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{
    using static ExpressionAssert;

    public class ExpressionTest
    {
        [Fact]
        public void All_ReturnsTrueWhenAllTrue() => Equal(true, new All { Predicates = Constant.CreateMany(true, true, true) });

        [Fact]
        public void All_ReturnsFalseWhenSomeFalse() => Equal(false, new All { Predicates = Constant.CreateMany(true, false, true) });

        [Fact]
        public void All_ReturnsFalseWhenAllFalse() => Equal(false, new All { Predicates = Constant.CreateMany(false, false, false) });

        [Fact]
        public void Any_ReturnsTrueWhenSomeTrue() => Equal(true, new Any { Predicates = Constant.CreateMany(false, false, true) });

        [Fact]
        public void Any_ReturnsFalseWhenAllFalse() => Equal(false, new Any { Predicates = Constant.CreateMany(false, false, false) });

        [Fact]
        public void IIf_ReturnsTrueWhenTrue() => Equal("foo", new IIf
        {
            Predicate = Constant.Create(true),
            True = Constant.Create("foo"),
            False = Constant.Create("bar")
        });

        [Fact]
        public void IIf_ReturnsFalseWhenFalse() => Equal("bar", new IIf
        {
            Predicate = Constant.Create(false),
            True = Constant.Create("foo"),
            False = Constant.Create("bar")
        });

        [Fact]
        public void IIf_throws_when_no_result_specified()
        {
            Assert.Throws<InvalidOperationException>(() => new IIf { Predicate = Constant.Create(false) }.Invoke(ExpressionContext.Empty));
        }
        
        [Fact]
        public void IIf_returns_null_constant_when_True_not_specified() => Equal(Constant.Null, new IIf
        {
            Predicate = Constant.True,
            False = Constant.Zero
        });
        
        [Fact]
        public void IIf_returns_null_constant_when_False_not_specified() => Equal(Constant.Null, new IIf
        {
            Predicate = Constant.False,
            True = Constant.One,
        });

        [Fact]
        public void Max_ReturnsMax() => Equal(3.0, new Max { Expressions = Constant.CreateMany(2.0, 3.0, 1.0) });

        [Fact]
        public void Min_ReturnsMin() => Equal(1.0, new Min { Expressions = Constant.CreateMany(2.0, 3.0, 1.0) });

        [Fact]
        public void Sum_ReturnsSum() => Equal(6.0, new Sum { Expressions = Constant.CreateMany(2.0, 3.0, 1.0) });

        [Fact]
        public void Equals_ReturnsTrueWhenEqual() => Equal(true, new Equals
        {
            Left = Constant.Create("foo"),
            Right = Constant.Create("foo"),
        });

        [Fact]
        public void Equals_ReturnsFalseWhenNotEqual() => Equal(false, new Equals
        {
            Left = Constant.Create("foo"),
            Right = Constant.Create("bar"),
        });

        [Fact]
        public void GreaterThan_ReturnsTrueWhenLeftGreaterThanRight() => Equal(true, new GreaterThan
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(2.0),
        });

        [Fact]
        public void GreaterThan_ReturnsFalseWhenLeftLessThanRight() => Equal(false, new GreaterThan
        {
            Left = Constant.Create(2.0),
            Right = Constant.Create(3.0),
        });

        [Fact]
        public void GreaterThan_ReturnsFalseWhenLeftEqualsRight() => Equal(false, new GreaterThan
        {
            Left = Constant.Create(2.0),
            Right = Constant.Create(2.0),
        });

        [Fact]
        public void GreaterThanOrEqual_ReturnsTrueWhenLeftGreaterThanRight() => Equal(true, new GreaterThanOrEqual
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(2.0),
        });

        [Fact]
        public void GreaterThanOrEqual_ReturnsTrueWhenLeftEqualsRight() => Equal(true, new GreaterThanOrEqual
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(3.0),
        });

        [Fact]
        public void GreaterThanOrEqual_ReturnsFalseWhenLeftLessThanRight() => Equal(false, new GreaterThanOrEqual
        {
            Left = Constant.Create(2.0),
            Right = Constant.Create(3.0),
        });

        [Fact]
        public void LessThan_ReturnsTrueWhenLeftLessThanRight() => Equal(true, new LessThan
        {
            Left = Constant.Create(2.0),
            Right = Constant.Create(3.0),
        });

        [Fact]
        public void LessThan_ReturnsFalseWhenLeftEqualsRight() => Equal(false, new LessThan
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(3.0),
        });

        [Fact]
        public void LessThan_ReturnsFalseWhenLeftGreaterThanRight() => Equal(false, new LessThan
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(2.0),
        });

        [Fact]
        public void LessThanOrEqual_ReturnsTrueWhenLeftLessThanRight() => Equal(true, new LessThanOrEqual
        {
            Left = Constant.Create(2.0),
            Right = Constant.Create(3.0),
        });

        [Fact]
        public void LessThanOrEqual_ReturnsTrueWhenLeftEqualsRight() => Equal(true, new LessThanOrEqual
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(3.0),
        });

        [Fact]
        public void LessThanOrEqual_ReturnsFalseWhenLeftGreaterThanRight() => Equal(false, new LessThanOrEqual
        {
            Left = Constant.Create(3.0),
            Right = Constant.Create(2.0),
        });

        [Fact]
        public void Not_ReturnsTrueWhenFalse() => Equal(false, new Not { Expression = Constant.Create(true) });

        [Fact]
        public void Not_ReturnsFalseWhenTrue() => Equal(false, new Not { Expression = Constant.Create(true) });

        [Fact]
        public void ToDouble_MapsTrueToOne() => Equal(1.0, new BooleanToDouble { Expression = Constant.Create(true) });

        [Fact]
        public void ToDouble_MapsFalseToZero() => Equal(0.0, new BooleanToDouble { Expression = Constant.Create(false) });
    }
}