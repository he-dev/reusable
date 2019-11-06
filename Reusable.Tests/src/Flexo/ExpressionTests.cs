using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Flexo.Helpers;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Reusable.Flexo
{
    public class ExpressionTest : IClassFixture<ExpressionFixture> //, IDisposable
    {
        private readonly ExpressionFixture _helper;

        public ExpressionTest(ExpressionFixture helper) => _helper = helper;

        [Fact]
        public void All_returns_True_when_all_True()
        {
            ExpressionAssert.ExpressionEqual(true, _helper.Resolve<All>(e => e.Values = Constant.CreateMany(true, true, true)));
        }

        [Fact]
        public void All_returns_False_when_some_False()
        {
            ExpressionAssert.ExpressionEqual(false, _helper.Resolve<All>(e => e.Values = Constant.CreateMany(true, false, true)));
        }

        [Fact]
        public void All_returns_False_when_all_False()
        {
            ExpressionAssert.ExpressionEqual(false, _helper.Resolve<All>(e => e.Values = Constant.CreateMany(false, false, false)));
        }

//        [Fact]
//        public void All_flows_all_contexts()
//        {
//            var actual = _helper.Resolve<All>(e =>
//            {
//                e.This = new List<IExpression>
//                {
//                    LambdaExpression.Predicate(() => (true, context.SetItem("x", (int)context["x"] + 1))),
//                    LambdaExpression.Predicate(() => (true, context.SetItem("x", (int)context["x"] + 1))),
//                    LambdaExpression.Predicate(() => (true, context.SetItem("x", (int)context["x"] + 1)))
//                };
//            });
//
//            var result = Equal(true, actual, ctx => ctx.SetItem("x", 1));
//            Assert.Equal(4, result.Context["x"]);
//        }

        [Fact]
        public void Any_returns_True_when_some_True()
        {
            ExpressionAssert.ExpressionEqual(true, _helper.Resolve<Any>(e => e.Values = Constant.CreateMany(false, false, true)));
        }

        [Fact]
        public void Any_returns_False_when_all_False()
        {
            ExpressionAssert.ExpressionEqual(false, _helper.Resolve<Any>(e => e.Values = Constant.CreateMany(false, false, false)));
        }

//        [Fact]
//        public void Any_flows_True_context()
//        {
//            var actual = _helper.Resolve<Any>(e =>
//            {
//                e.This = new List<IExpression>
//                {
//                    LambdaExpression.Predicate(context => (false, context.SetItem("x", (int)context["x"] + 2))),
//                    LambdaExpression.Predicate(context => (true, context.SetItem("x", (int)context["x"] + 3))),
//                    LambdaExpression.Predicate(context => (false, context.SetItem("x", (int)context["x"] + 4)))
//                };
//            });
//
//            var result = Equal(true, actual, ctx => ctx.SetItem("x", 1));
//            Assert.Equal(4, result.Context["x"]);
//        }

        [Fact]
        public void IIf_invokes_True_when_True()
        {
            ExpressionAssert.ExpressionEqual("foo", _helper.Resolve<IIf>(e =>
            {
                e.Predicate = Constant.FromValue(true);
                e.True = Constant.FromValue("foo");
                e.False = Constant.FromValue("bar");
            }));
        }

        [Fact]
        public void IIf_invokes_False_when_False()
        {
            ExpressionAssert.ExpressionEqual("bar", _helper.Resolve<IIf>(e =>
            {
                e.Predicate = Constant.FromValue(false);
                e.True = Constant.FromValue("foo");
                e.False = Constant.FromValue("bar");
            }));
        }

        [Fact]
        public void IIf_throws_when_no_result_specified()
        {
            Assert.Throws<InvalidOperationException>(() => _helper.Resolve<IIf>(e => e.Predicate = Constant.FromValue(false)).Invoke(ImmutableContainer.Empty));
        }

        [Fact]
        public void IIf_returns_null_constant_when_True_not_specified()
        {
            ExpressionAssert.ExpressionEqual(Constant.Null, _helper.Resolve<IIf>(e =>
            {
                e.Predicate = Constant.True;
                e.False = Double.Zero;
            }));
        }

        [Fact]
        public void IIf_returns_null_constant_when_False_not_specified()
        {
            ExpressionAssert.ExpressionEqual(Constant.Null, _helper.Resolve<IIf>(e =>
            {
                e.Predicate = Constant.False;
                e.True = Double.One;
            }));
        }

//        [Fact]
//        public void IIf_flows_context_to_True_when_Predicate_True()
//        {
//            var actual = _helper.Resolve<IIf>(e =>
//            {
//                e.This = Constant.True;
//                e.True = LambdaExpression.Double(context => (1.0, context.SetItem("x", (int)context["x"] + 1)));
//                e.False = LambdaExpression.Double(context => (2.0, context.SetItem("x", (int)context["x"] + 2)));
//            });
//
//            var result = Equal(1.0, actual, ctx => ctx.SetItem("x", 1));
//            Assert.Equal(2, result.Context["x"]);
//        }

//        [Fact]
//        public void IIf_does_not_flow_context_to_False_when_Predicate_False()
//        {
//            var actual = _helper.Resolve<IIf>(e =>
//            {
//                e.This = Constant.False;
//                e.True = LambdaExpression.Double(context => (1.0, context.SetItem("x", (int)context["x"] + 1)));
//                e.False = LambdaExpression.Double(context => (2.0, context.SetItem("x", (int)context["x"] + 2)));
//            });
//
//            var result = Equal(2.0, actual, ctx => ctx.SetItem("x", 1));
//            Assert.Equal(3, result.Context["x"]);
//        }

        [Fact]
        public void Max_returns_Max()
        {
            ExpressionAssert.ExpressionEqual(3.0, _helper.Resolve<Max>(e => e.Values = Constant.CreateMany(2.0, 3.0, 1.0)));
        }

        [Fact]
        public void Min_returns_Min()
        {
            ExpressionAssert.ExpressionEqual(1.0, _helper.Resolve<Min>(e => e.Values = Constant.CreateMany(2.0, 3.0, 1.0)));
        }

        [Fact]
        public void Sum_returns_Sum()
        {
            ExpressionAssert.ExpressionEqual(6.0, _helper.Resolve<Sum>(e => e.Values = Constant.CreateMany(2.0, 3.0, 1.0)));
        }

        [Fact]
        public void IsEqual_returns_True_when_Input_equal_Value()
        {
            ExpressionAssert.ExpressionEqual(true, _helper.Resolve<IsEqual>(e =>
            {
                e.Left = Constant.FromValue("foo");
                e.Value = Constant.FromValue("foo");
            }));
        }

        [Fact]
        public void IsEqual_returns_False_when_Input_not_equal_Value()
        {
            ExpressionAssert.ExpressionEqual(false, _helper.Resolve<IsEqual>(e =>
            {
                e.Left = Constant.FromValue("bar");
                e.Value = Constant.FromValue("foo");
            }));
        }

        [Fact]
        public void IsGreaterThan_returns_True_when_Input_GreaterThan_Value()
        {
            ExpressionAssert.ExpressionEqual(true, _helper.Resolve<IsGreaterThan>(e =>
            {
                e.Left = Constant.FromValue(3.0);
                e.Right = Constant.FromValue(2.0);
            }));
        }

        [Fact]
        public void IsGreaterThan_returns_False_when_Input_LessThan_Value()
        {
            ExpressionAssert.ExpressionEqual(false, _helper.Resolve<IsGreaterThan>(e =>
            {
                e.Left = Constant.FromValue(2.0);
                e.Right = Constant.FromValue(3.0);
            }));
        }

        [Fact]
        public void IsGreaterThan_returns_False_when_This_equal_Right()
        {
            ExpressionAssert.ExpressionEqual(false, _helper.Resolve<IsGreaterThan>(e =>
            {
                e.Left = Constant.FromValue(2.0);
                e.Right = Constant.FromValue(2.0);
            }));
        }

        [Fact]
        public void IsGreaterThanOrEqual_returns_True_when_This_greaterThan_Right()
        {
            ExpressionAssert.ExpressionEqual(true, _helper.Resolve<IsGreaterThanOrEqual>(e =>
            {
                e.Left = Constant.FromValue(3.0);
                e.Right = Constant.FromValue(2.0);
            }));
        }

        [Fact]
        public void IsGreaterThanOrEqual_returns_True_when_This_equal_Right()
        {
            ExpressionAssert.ExpressionEqual(true, _helper.Resolve<IsGreaterThanOrEqual>(e =>
            {
                e.Left = Constant.FromValue(3.0);
                e.Right = Constant.FromValue(3.0);
            }));
        }

        [Fact]
        public void IsGreaterThanOrEqual_returns_False_when_This_lessThan_Right()
        {
            ExpressionAssert.ExpressionEqual(false, _helper.Resolve<IsGreaterThanOrEqual>(e =>
            {
                e.Left = Constant.FromValue(2.0);
                e.Right = Constant.FromValue(3.0);
            }));
        }

        [Fact]
        public void IsLessThan_returns_True_when_This_lessThan_Right()
        {
            ExpressionAssert.ExpressionEqual(true, _helper.Resolve<IsLessThan>(e =>
            {
                e.Left = Constant.FromValue(2.0);
                e.Right = Constant.FromValue(3.0);
            }));
        }

        [Fact]
        public void IsLessThan_returns_False_when_This_equal_Right()
        {
            ExpressionAssert.ExpressionEqual(false, _helper.Resolve<IsLessThan>(e =>
            {
                e.Left = Constant.FromValue(3.0);
                e.Right = Constant.FromValue(3.0);
            }));
        }

        [Fact]
        public void IsLessThan_returns_False_when_This_greaterThan_Right()
        {
            ExpressionAssert.ExpressionEqual(false, _helper.Resolve<IsLessThan>(e =>
            {
                e.Left = Constant.FromValue(3.0);
                e.Right = Constant.FromValue(2.0);
            }));
        }

        [Fact]
        public void IsLessThanOrEqual_returns_True_when_This_lessThan_Right()
        {
            ExpressionAssert.ExpressionEqual(true, _helper.Resolve<IsLessThanOrEqual>(e =>
            {
                e.Left = Constant.FromValue(2.0);
                e.Right = Constant.FromValue(3.0);
            }));
        }

        [Fact]
        public void IsLessThanOrEqual_returns_True_when_This_equal_Right()
        {
            ExpressionAssert.ExpressionEqual(true, _helper.Resolve<IsLessThanOrEqual>(e =>
            {
                e.Left = Constant.FromValue(3.0);
                e.Right = Constant.FromValue(3.0);
            }));
        }

        [Fact]
        public void IsLessThanOrEqual_returns_False_when_This_greaterThan_Right()
        {
            ExpressionAssert.ExpressionEqual(false, _helper.Resolve<IsLessThanOrEqual>(e =>
            {
                e.Left = Constant.FromValue(3.0);
                e.Right = Constant.FromValue(2.0);
            }));
        }

        [Fact]
        public void Not_returns_True_when_False()
        {
            ExpressionAssert.ExpressionEqual(false, _helper.Resolve<Not>(e => e.Value = Constant.True));
        }

        [Fact]
        public void Not_returns_False_when_True()
        {
            ExpressionAssert.ExpressionEqual(false, _helper.Resolve<Not>(e => e.Value = Constant.True));
        }

        [Fact]
        public void ToDouble_maps_True_to_One()
        {
            ExpressionAssert.ExpressionEqual(1.0, _helper.Resolve<ToDouble>(e => e.Value = Constant.True));
        }

        [Fact]
        public void ToDouble_maps_False_to_Zero()
        {
            ExpressionAssert.ExpressionEqual(0.0, _helper.Resolve<ToDouble>(e => e.Value = Constant.False));
        }

        [Fact]
        public void ToString_converts_Input_to_string()
        {
            ExpressionAssert.ExpressionEqual("1", _helper.Resolve<ToString>(e => e.Value = Constant.FromValue(1.0)));
        }

        [Fact]
        public void ToString_converts_Input_to_string_with_custom_format()
        {
            ExpressionAssert.ExpressionEqual("1.00", _helper.Resolve<ToString>(e =>
            {
                e.Value = Constant.FromValue(1.0);
                e.Format = Constant.FromValue("{0:F2}");
            }));
        }

//        [Fact]
//        public void Constant_flows_context()
//        {
//            var c = Constant.True;
//            var actual = c.Invoke(Expression.DefaultSession.SetItem("x", 1));
//            Assert.NotNull(actual.Context);
//            Assert.True(actual.Context.ContainsKey("x"));
//        }

        [Fact]
        public void Switch_uses_ObjectEqual_by_default()
        {
            var s = _helper.Resolve<Switch>(e =>
            {
                e.Value = Double.One;
                e.Cases = new List<SwitchCase>
                {
                    new SwitchCase
                    {
                        When = Double.One,
                        Body = Constant.True
                    },
                    new SwitchCase
                    {
                        When = Double.Zero,
                        Body = Constant.False
                    }
                };
            });

            ExpressionAssert.ExpressionEqual(true, s);
        }

//        [Fact]
//        public void Switch_passes_SwitchValue_to_context()
//        {
//            var s = _helper.Resolve<Switch>(e =>
//            {
//                e.This = Constant.FromValue("Test", "bar");
//                e.Cases = new List<SwitchCase>
//                {
//                    new SwitchCase
//                    {
//                        When = _helper.Resolve<Contains>(ee =>
//                        {
//                            ee.This = Constant.CreateMany("foo", "bar").ToList();
//                            ee.Value = _helper.Resolve<GetSingle>(eee => eee.Path = "SwitchSession.Value");
//                        }),
//                        Body = Constant.True
//                    },
//                    new SwitchCase
//                    {
//                        When = Double.Zero,
//                        Body = Constant.False
//                    }
//                };
//            });
//
//            ExpressionAssert.ExpressionEqual(Constant.True, s);
//        }

        [Fact]
        public void Switch_uses_Default_when_no_match()
        {
            var s = _helper.Resolve<Switch>(e =>
            {
                e.Value = Constant.FromValue("Test", "bar");
                e.Cases = new List<SwitchCase>
                {
                    new SwitchCase
                    {
                        When = Double.One,
                        Body = Constant.True
                    },
                    new SwitchCase
                    {
                        When = Double.Zero,
                        Body = Constant.False
                    }
                };
                e.Default = new Constant<double>("Actual", 2.0);
            });

            ExpressionAssert.ExpressionEqual(2.0, s);
        }

        [Fact]
        public void Contains_returns_True_when_contains_value()
        {
            ExpressionAssert.ExpressionEqual(true, _helper.Resolve<Contains>(e =>
            {
                e.Values = Constant.CreateMany("foo", "bar").ToList();
                e.Value = Constant.FromValue("blub", "bar");
            }));
        }

        [Fact]
        public void Contains_uses_custom_comparer()
        {
            ExpressionAssert.ExpressionEqual(true, _helper.Resolve<Contains>(e =>
            {
                e.Values = Constant.CreateMany("foo", "BAR").ToList();
                e.Value = Constant.FromValue("Value", "bar");
                e.ComparerName = "SoftString";
            }));
        }

        [Fact]
        public void Matches_return_True_when_Pattern_matches()
        {
            ExpressionAssert.ExpressionEqual(true, _helper.Resolve<Matches>(e =>
            {
                e.IgnoreCase = true;
                e.Value = Constant.FromValue("Value", "Hallo");
                e.Pattern = Constant.FromValue("Pattern", "hallo");
            }));
        }

//        [Fact]
//        public void GetContextItem_can_get_item_by_key()
//        {
//            Assert.That.ExpressionEqual(1, _helper.Resolve<GetSingle>(e => e.Path = "SwitchSession.Value"), ctx => ctx.SetItem(From<ISwitchMeta>.Select(x => x.Value), 1));
//        }
//
//        [Fact]
//        public void Can_use_references()
//        {
//            var expressions = new IExpression[] { _helper.Resolve<Not>(e => e.Name = "Nope") };
//            var expression1 = new Constant<bool>("Yes", true) { Extensions = new List<IExpression> { _helper.Resolve<Ref>(e => e.Path = "Nope") } };
//            var expression2 = new Constant<bool>("No", false) { Extensions = new List<IExpression> { _helper.Resolve<Ref>(e => e.Path = "Nope") } };
//
//            Equal(false, expression1, ctx => ctx.WithReferences(expressions));
//            Equal(true, expression2, ctx => ctx.WithReferences(expressions));
//        }

        //public void Dispose() => _helper.Dispose();
    }
}